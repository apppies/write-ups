# Exfiltration

## Problem

A file was exfiltrated using common protocol. In fact if this protocol didn't exist using internet would be annoying.
Can you find the header and then extract the file?

[exfiltration.pcap](https://thenixuchallenge.com/c/exfiltration/static/exfiltration.pcap)

## Solution

Using DNS queries data is being sent to an external point, getting the data from the DNS queries and converting it yields the flag.

### Introduction

After opening the provided pcap file in Wireshark it looks like normal browsing traffic at first. However, the conversation shows list a lot of traffic between two addresses, 10.0.2.15 and 1.1.1.1. Filtering on that it appears to be DNS traffic. The hint in the problem also hints that DNS might need to be looked at.

Browsing trough the DNS entries some special onces appear, for example

```
6332	46.519448	10.0.2.15	1.1.1.1	DNS	294	Standard query 0x15bf MX c8710152831cc16d694eb9cba112e0908d99ddba750ba1a1a1080f0f676a.27223231499250a74e1dfcf1c71f68d2a409e79458300612334a4949c13b.efbc83888808f68c101199892449a85bb72e962d5b86a64d9bca5d0e1581.433666929090804f3ef9043b77ee6418212232.malicious.pw
```

1. That is a really long url,
2. It has malicious.pw as domain,
3. And there are plenty of them.

Let's extract all DNS queries for domain malicious.pw using tshark.

```Powershell
$tshark = "C:\Wireshark\tshark.exe"
$qry = . $tshark -r "C:\Nixu\exfiltration.pcap" -Y 'ip.dst==1.1.1.1 and ip.src==10.0.2.15 and  dns.qry.name contains "malicious.pw"' -T fields -e dns.qry.name
$filtered = $qry | ForEach-Object { $_.Replace('.malicious.pw','').Replace('.','') }
```

The data is formatted hexadecimal. So the next step is to see if it means something.

```Powershell
[System.Text.Encoding]::utf8.GetString([byte[]] -split ($filtered -replace '..', '0x$& '))
```

This shows a lot of output, but somewhere at the beginning it shows

```
d?w-r--r-- 1 root root 21768 Nov 26 09:28 flag.png
```

The problem suggested to look for a header, let's look for PNG. The PNG header is defined by `89 50 4e 47 0d 0a 1a 0a`. Dumping the whole byte array to a file, opening it in HXD, and searching for the PNG header shows that it starts at `0x2F7`. PNG files are marked with `IEND` at the end of the image data, found at `0x6055`. Removing the data before and after, saving the file and opening it as a PNG file should bring the answer. To bad, a black image appears. Perhaps the data needs to be parsed before putting it all in the file.

Taking a closer look at the DNS requests, many lines start with something similar to `69 C0 01 52 83 0D 21 6D 58` or `B5 19 01 52 83 0D 21 6D 69`. Perhaps some header of some kind. Also the exfiltrated data will most likely be present in the long requests, so remove shorter lines as well.

```Powershell
$filtered2 = $filtered | Where-Object {$_.Length -gt 36} | ForEach-Object {$_.Substring(18)}
[byte[]] -split ($output -replace '..', '0x$& ') | Set-Content C:\Nixu\exfil.png -Encoding Byte
```

Looking at this data in HxD the PNG header is located at 0xC0. Removing all data before that point and saving it results in a working PNG file, showing a nice flag.

```
NIXU{####_#######_#########_#########}
```

Combining all:

```Powershell
$tshark = "C:\Wireshark\tshark.exe"
$qry = . $tshark -r "C:\Nixu\exfiltration.pcap" -Y 'ip.dst==1.1.1.1 and ip.src==10.0.2.15 and  dns.qry.name contains "malicious.pw"' -T fields -e dns.qry.name
$data = ($qry | ForEach-Object { $_.Replace('.malicious.pw','').Replace('.','').Substring(18)} | Where-Object {$_.Length -gt 18}) -join ''
[byte[]] -split ($data.Substring($data.IndexOf('89504e470d0a1a0a')) -replace '..', '0x$& ') | Set-Content C:\Nixu\exfil.png -Encoding Byte
```
