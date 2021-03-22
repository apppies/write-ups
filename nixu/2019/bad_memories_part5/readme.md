# Bad memories - part 5

## Problem

The lead graphical designer at ACME has noticed some kind of strange activity on her computer. Their IT support believes it is a false positive and the computer will fix itself after turning it off and on again. During the earlier investigation, it turned out that our user has forgotten her password. She only remembers it was very strong.

This is part 5 of 5 in a memory dump analysis challenge. The [memory dump file](https://thenixuchallenge.com/c/bad_memories_part1/) can be found from the first part.

## Solution

Run lsadump and obtain the credentials

### Hashes

Passwords are stored in multiple places, Volatility has several ways to obtain them. First, let's get some hashes. This is done in two steps

1. Get the hives SYSTEM en SAM (hivelist)
2. Obtain the hashes (hashdump)

```shell
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 hivelist

Volatility Foundation Volatility Framework 2.6.1
Virtual            Physical           Name
------------------ ------------------ ----
0xfffff8a00000f010 0x0000000024fc9010 [no name]
0xfffff8a000024010 0x0000000024ed4010 \REGISTRY\MACHINE\SYSTEM
0xfffff8a000054010 0x000000001a184010 \REGISTRY\MACHINE\HARDWARE
0xfffff8a000118010 0x0000000024deb010 \SystemRoot\System32\Config\DEFAULT
0xfffff8a0001f5010 0x0000000023421010 \??\C:\Windows\ServiceProfiles\LocalService\NTUSER.DAT
0xfffff8a000ab3010 0x0000000025640010 \Device\HarddiskVolume1\Boot\BCD
0xfffff8a000abf010 0x0000000025b71010 \SystemRoot\System32\Config\SOFTWARE
0xfffff8a000c12010 0x0000000023b4c010 \SystemRoot\System32\Config\SECURITY
0xfffff8a000cad010 0x0000000010f80010 \SystemRoot\System32\Config\SAM
0xfffff8a000d54380 0x0000000022adc380 \??\C:\Windows\ServiceProfiles\NetworkService\NTUSER.DAT
0xfffff8a0010ce010 0x000000000f211010 \??\C:\Users\sshd_server\ntuser.dat
0xfffff8a001169010 0x000000000e196010 \??\C:\Users\sshd_server\AppData\Local\Microsoft\Windows\UsrClass.dat
0xfffff8a0012e6410 0x0000000010f69410 \??\C:\Users\Alice\ntuser.dat
0xfffff8a0012f1010 0x000000000a5b3010 \??\C:\Users\Alice\AppData\Local\Microsoft\Windows\UsrClass.dat
```

```shell
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 hashdump -s 0xfffff8a000cad010 -y 0xfffff8a000024010

Volatility Foundation Volatility Framework 2.6.1
Administrator:500:aad3b435b51404eeaad3b435b51404ee:fc525c9683e8fe067095ba2ddc971889:::
Guest:501:aad3b435b51404eeaad3b435b51404ee:31d6cfe0d16ae931b73c59d7e0c089c0:::
IEUser:1000:aad3b435b51404eeaad3b435b51404ee:fc525c9683e8fe067095ba2ddc971889:::
sshd:1001:aad3b435b51404eeaad3b435b51404ee:31d6cfe0d16ae931b73c59d7e0c089c0:::
sshd_server:1002:aad3b435b51404eeaad3b435b51404ee:8d0a16cfc061c3359db455d00ec27035:::
Alice:1003:aad3b435b51404eeaad3b435b51404ee:e556acdd3d96937e55035acef5f037b0:::
```

Running the hash for Alice (`e556acdd3d96937e55035acef5f037b0`) through crackstation or hashkiller produces no results. So before brute forcing the hash, let's look at the LSA secrets.

### LSA

LSA secrets can be easily obtained using Volatility.

```shell
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 lsadump 

Volatility Foundation Volatility Framework 2.6.1
DefaultPassword
0x00000000  3e 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00   >...............
0x00000010  4e 00 49 00 58 00 55 00 7b 00 77 00 61 00 73 00   N.I.X.U.{.w.a.s.
0x00000020  5f 00 69 00 74 00 5f 00 65 00 76 00 65 00 6e 00   _.i.t._.e.v.e.n.
0x00000030  5f 00 68 00 61 00 72 00 64 00 5f 00 66 00 6f 00   _.h.a.r.d._.f.o.
0x00000040  72 00 5f 00 79 00 6f 00 75 00 3f 00 7d 00 00 00   r._.y.o.u.?.}...

_SC_OpenSSHd
0x00000000  14 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00   ................
0x00000010  44 00 40 00 72 00 6a 00 33 00 33 00 6c 00 31 00   D.@.r.j.3.3.l.1.
0x00000020  6e 00 67 00 00 00 00 00 00 00 00 00 00 00 00 00   n.g.............

DPAPI_SYSTEM
0x00000000  2c 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00   ,...............
0x00000010  01 00 00 00 d7 e7 10 e0 e1 fe e9 a6 fa 72 6c e5   .............rl.
0x00000020  6d bd f2 fb b3 20 2d 1e ac 17 fe 50 74 dd ae a2   m.....-....Pt...
0x00000030  1a 32 dc d9 18 b6 5f 26 91 b1 dd c4 00 00 00 00   .2...._&........
```
