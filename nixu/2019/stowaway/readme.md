# Stowaway

Our field support engineers got a case of a legacy IoT device that is not accepting the latest update. Help them to find out what's wrong, fix it and see if you can provide an acceptable update.

[Device software update portal](http://stowaway.thenixuchallenge.com/)

## Solution

Unpack the package, add a certificate that is valid with the right name, repack and upload.

### Introduction

Following the update portal link gives a website with a working firmware, and the latest non-working firmware. These files have the extension `.nxa.b64`. Uploading the last working firmware yields

```
Please use NXA format.
``` 

So the download is not readily usable. Assuming the b64 stands for Base64 encoded. First step is to decode the files

```Powershell
[System.Convert]::FromBase64String((Get-Content C:\NIXU\version_1.nxa.b64)) | Set-Content C:\NIXU\version_1.nxa -Encoding Byte
[System.Convert]::FromBase64String((Get-Content C:\NIXU\version_2.nxa.b64)) | Set-Content C:\NIXU\version_2.nxa -Encoding Byte
``` 

Uploading these files to the server yields:

```
Uploaded 'version_1.nxa'
Unpacking archive... Done.
Looking for certificate... Done.
Validating certificate... Done.
Validating manifest signature... Done.
Validating checksums for files listed in manifest... All files found and hashes match.
Checking version... Uploaded version is not newer. Update to get flag.
```

```
Uploaded 'version_2.nxa'
Unpacking archive... Done.
Looking for certificate... Done.
Validating certificate... Certificate error.
```

Version 2 gives a certificate error. Let's look inside these NXA files.

### NXA files

A quick search reveals zero information on these NXA. So let's open them in HxD and see what is inside. The first six words give 

```
4E 58 41 31 00 00 00 62 61 73 65 2D 66 69 6C 65  NXA1...base-file
73 5F 31 39 32 2D 72 37 31 38 38 2D 62 30 62 35  s_192-r7188-b0b5
63 36 34 63 32 32 5F 6D 69 70 73 65 6C 5F 6D 69  c64c22_mipsel_mi
70 73 33 32 2E 69 70 6B 6F 9C 00 00 1F 8B 08 00  ps32.ipkoœ...‹..
00 00 00 00 00 03 EC B9 43 90 2E 4C 10 AE D9 B6  ......ì¹C..L.®Ù¶
AD D3 B6 6D DB 38 6D DB B6 BB 4F DB B6 6D DB B6  ­Ó¶mÛ8mÛ¶»OÛ¶mÛ¶
```

1. NXA header `4E 58 41`
2. Int32 value with the length of the filename `31 00 00 00`
3. Filename with the length specified in 2 `62 61 73 65 2D 66 69 6C 65 73 5F 31 39 32 2D 72 37 31 38 38 2D 62 30 62 35 63 36 34 63 32 32 5F 6D 69 70 73 65 6C 5F 6D 69 70 73 33 32 2E 69 70 6B`
4. Int32 value with the length of the file `6F 9C 00 00`
5. Content of the file with the length specified in 4
6. Repeat steps 2 to 5 until the end of file

An unpack function in c# looks like

```cs
static void Unpack(string file, string path)
{
	using (var re = new BinaryReader(File.Open(file, FileMode.Open)))
	{
		var header = new String(re.ReadChars(3));
		if (header != "NXA")
		{
			Debug.WriteLine("Invalid header");
			return;
		}

		while (re.BaseStream.Position != re.BaseStream.Length)
		{
			var l = re.ReadInt32();
			var n = new string(re.ReadChars(l));
			l = re.ReadInt32();
			var b = re.ReadBytes(l);

			File.WriteAllBytes($"{path}\\{n}", b);
		}
	}
}
```

As a new NXA file is later needed, a pack function is also needed.

```cs
static void Pack(string path, string file)
{
	using (var wr = new BinaryWriter(File.Open(file, FileMode.Create)))
	{
		wr.Write("NXA".ToCharArray());

		var files = Directory.EnumerateFiles(path).ToList();
		for (int i = 0; i < files.Count; i++)
		{
			var n  = Path.GetFileName(files[i]);
			var b = File.ReadAllBytes(files[i]);
			wr.Write((Int32)n.Length);
			wr.Write(n.ToCharArray());
			wr.Write((Int32)b.Length);
			wr.Write(b);
		}
	}
}
```

### NXA content

The content of version 1

```
base-files_192-r7188-b0b5c64c22_mipsel_mips32.ipk
cert-john.crt
changelog
fstools_2018-04-16-e2436836-1_mipsel_mips32.ipk
kmod-b43_4.14.54+2017-11-01-9_mipsel_mips32.ipk
kmod-mac80211_4.14.54+2017-11-01-9_mipsel_mips32.ipk
manifest
manifest.sig
```

The content of version 2

```
base-files_192-r7258-5eb055306f_mipsel_mips32.ipk
cert-coder.crt
changelog
kmod-crypto-hash_4.14.63-1_mipsel_mips32.ipk
kmod-crypto-hmac_4.14.63-1_mipsel_mips32.ipk
kmod-crypto-manager_4.14.63-1_mipsel_mips32.ipk
manifest
manifest.sig
```

### Fixing the update

The website showed an error with the certificate of version 2, and indeed, certificate `cert-coder.crt` has a reversed start and end date, and it's end date is in the past. `cert-john.crt` from version 1 is valid.

Replacing cert-coder with a renamed cert-john should not work as there is a signed manifest. Let's try anyway.

```
Uploaded 'version_cert.nxa'
Unpacking archive... Done.
Looking for certificate... Done.
Validating certificate... Done.
Validating manifest signature... Done.
Validating checksums for files listed in manifest... Invalid checksum for cert-coder.crt.
```

The certificate is accepted, but the checksum is invalid. Without the private key creating the signature is hard. If needed, that is something for later.

How does the system look for a certificate? The name is not fixed, and there does not seem to be value somewhere indicating which certificate to use. If it uses the first certificate it finds we should be able to abuse it. A good manifest file should protect against additional files. Let's try anyway by copying coder-john.crt twice and renaming it to a.crt and z.crt.

```
Uploaded 'version_cert.nxa'
Unpacking archive... Done.
Looking for certificate... Done.
Validating certificate... Done.
Validating manifest signature... Done.
Validating checksums for files listed in manifest... All files found and hashes match.
Checking version... OK! Updating...

Congratulations! Have a flag:
NIXU{###_########_####_#####_####_######_##_########}
```
