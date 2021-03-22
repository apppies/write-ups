# Bad memories - part 1

## Problem

The lead graphical designer at ACME has noticed some kind of strange activity on her computer. Their IT support believes it is a false positive and the computer will fix itself after turning it off and on again. However, the user managed to take a memory dump just before the crash. Could you help us recover the documentation she was working on?

This is part 1 of 5 in a memory dump analysis challenge found in [mem.7z](https://thenixuchallenge.com/c/bad_memories_part1/static/mem.7z). The parts are numbered loosely according to the difficulty level. (If something doesn't work, just try elsewhere.)

## Solution

Extract the notepad.exe process, obtain all string, find the encoded value, decode it

### Introduction

The bad memories challenges are an excercise in analyzing a memory dump. [Volatility](https://github.com/volatilityfoundation/volatility) is an excellent tool for that and can answer these questions.

The problem states that we need to find some documentation. So we are looking for something with text. Let's look at the memory dump and see what activity was present at the time of the dump.

### Analysis

First let volatility analyze the memory dump and determine the operating system

```shell
python vol.py -f c:\dump\mem.dmp imageinfo

Volatility Foundation Volatility Framework 2.6.1
INFO    : volatility.debug    : Determining profile based on KDBG search...
          Suggested Profile(s) : Win7SP1x64, Win7SP0x64, Win2008R2SP0x64, Win2008R2SP1x64_24000, Win2008R2SP1x64_23418, Win2008R2SP1x64, Win7SP1x64_24000, Win7SP1x64_23418
                     AS Layer1 : WindowsAMD64PagedMemory (Kernel AS)
                     AS Layer2 : FileAddressSpace (C:\dump\mem.dmp)
                      PAE type : No PAE
                           DTB : 0x187000L
                          KDBG : 0xf80002a03110L
          Number of Processors : 4
     Image Type (Service Pack) : 1
                KPCR for CPU 0 : 0xfffff80002a04d00L
                KPCR for CPU 1 : 0xfffff880009ee000L
                KPCR for CPU 2 : 0xfffff88002f69000L
                KPCR for CPU 3 : 0xfffff88002fdf000L
             KUSER_SHARED_DATA : 0xfffff78000000000L
           Image date and time : 2018-12-20 05:30:11 UTC+0000
     Image local date and time : 2018-12-19 21:30:11 -0800
```

So the dump is Windows 7 or Server 2008 based, 64 bit. Let's use Win7SP1x64 for now and see if further specification is later needed.

Next step is to obtain the process tree

```
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 pstree

Name                                                  Pid   PPid   Thds   Hnds Time
-------------------------------------------------- ------ ------ ------ ------ ----
 0xfffffa80024255d0:csrss.exe                         384    364     10    268 2018-12-20 05:28:25 UTC+0000
. 0xfffffa8000d89b10:conhost.exe                     3420    384      2     53 2018-12-20 05:30:11 UTC+0000
. 0xfffffa8003737730:conhost.exe                     1144    384      3     54 2018-12-20 05:28:37 UTC+0000
 0xfffffa80024867d0:winlogon.exe                      512    364      4    120 2018-12-20 05:28:26 UTC+0000
 0xfffffa80021c2930:csrss.exe                         320    304     10    535 2018-12-20 05:28:25 UTC+0000
. 0xfffffa8000dc4060:conhost.exe                     1440    320      2     33 2018-12-20 05:28:27 UTC+0000
 0xfffffa8002426060:wininit.exe                       372    304      3     85 2018-12-20 05:28:25 UTC+0000
. 0xfffffa800248a790:services.exe                     428    372      9    247 2018-12-20 05:28:25 UTC+0000
.. 0xfffffa80033ef400:svchost.exe                     960    428      6    120 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa80033c08b0:dllhost.exe                    2336    428     18    214 2018-12-20 05:28:28 UTC+0000
.. 0xfffffa80034a23c0:spoolsv.exe                    1040    428     14    297 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa800173eb10:svchost.exe                    2724    428      9    113 2018-12-20 05:29:18 UTC+0000
.. 0xfffffa8003a3eb10:msdtc.exe                      2452    428     15    159 2018-12-20 05:28:28 UTC+0000
.. 0xfffffa80033d5b10:svchost.exe                     900    428     41    837 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa80033cdb10:svchost.exe                    1180    428     13    158 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa8003339b10:vmacthlp.exe                    672    428      4     60 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa8002479b10:svchost.exe                     832    428     19    401 2018-12-20 05:28:26 UTC+0000
... 0xfffffa8001d71060:dwm.exe                       1768    832      4     80 2018-12-20 05:28:27 UTC+0000
.. 0xfffffa800399e6b0:dllhost.exe                    2212    428     25    215 2018-12-20 05:28:28 UTC+0000
.. 0xfffffa8001d09b10:wlms.exe                       1608    428      4     50 2018-12-20 05:28:27 UTC+0000
.. 0xfffffa8003537b10:vmtoolsd.exe                   1540    428     12    294 2018-12-20 05:28:27 UTC+0000
.. 0xfffffa80034bc2c0:svchost.exe                    1072    428     21    317 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa8003438a30:svchost.exe                     308    428     16    351 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa8000de7700:VGAuthService.                 1464    428      4     88 2018-12-20 05:28:27 UTC+0000
.. 0xfffffa8003567720:svchost.exe                    1224    428     10    202 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa8003bfa660:svchost.exe                    3536    428     12    260 2018-12-20 05:30:22 UTC+0000
.. 0xfffffa800334a9c0:svchost.exe                     720    428      7    282 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa8003a9eb10:VSSVC.exe                      2616    428      7    120 2018-12-20 05:28:30 UTC+0000
.. 0xfffffa80037d24a0:sppsvc.exe                     1108    428      5    152 2018-12-20 05:28:28 UTC+0000
.. 0xfffffa800246c850:svchost.exe                     856    428     16    553 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa8001d1eb10:taskhost.exe                   1636    428      8    148 2018-12-20 05:28:27 UTC+0000
.. 0xfffffa8003312060:svchost.exe                     604    428     12    369 2018-12-20 05:28:26 UTC+0000
... 0xfffffa8003a83b10:WmiPrvSE.exe                  2220    604     15    325 2018-12-20 05:28:48 UTC+0000
... 0xfffffa8003be0060:dllhost.exe                   3268    604     11    240 2018-12-20 05:30:05 UTC+0000
... 0xfffffa800399db10:WmiPrvSE.exe                  2172    604     12    205 2018-12-20 05:28:28 UTC+0000
.. 0xfffffa800337e060:svchost.exe                     784    428     21    339 2018-12-20 05:28:26 UTC+0000
.. 0xfffffa8000d4c060:cygrunsrv.exe                  1380    428      6    105 2018-12-20 05:28:27 UTC+0000
... 0xfffffa8000d705f0:cygrunsrv.exe                 1416   1380      0 ------ 2018-12-20 05:28:27 UTC+0000
.... 0xfffffa8000df1060:sshd.exe                     1480   1416      6    107 2018-12-20 05:28:27 UTC+0000
.. 0xfffffa80035cba70:svchost.exe                    2152    428      7    104 2018-12-20 05:28:28 UTC+0000
.. 0xfffffa8003cc8270:WmiApSrv.exe                   2644    428      8    125 2018-12-20 05:28:49 UTC+0000
.. 0xfffffa8003b6d060:SearchIndexer.                 2836    428     16    699 2018-12-20 05:28:33 UTC+0000
. 0xfffffa80024a3b10:lsass.exe                        444    372     10    592 2018-12-20 05:28:26 UTC+0000
. 0xfffffa80024a56f0:lsm.exe                          452    372      9    144 2018-12-20 05:28:26 UTC+0000
 0xfffffa8000c3bb10:System                              4      0     93    366 2018-12-20 05:28:24 UTC+0000
. 0xfffffa8001692b10:smss.exe                         236      4      2     32 2018-12-20 05:28:24 UTC+0000
 0xfffffa8001da1b10:explorer.exe                     1840   1748     47   1059 2018-12-20 05:28:27 UTC+0000
. 0xfffffa8003728b10:vmtoolsd.exe                    1472   1840     10    194 2018-12-20 05:28:27 UTC+0000
. 0xfffffa80013ff1c0:cmd.exe                         1548   1840      1     19 2018-12-20 05:28:37 UTC+0000
. 0xfffffa80014d2060:mspaint.exe                     2816   1840      8    184 2018-12-20 05:29:18 UTC+0000
. 0xfffffa8003bda930:winpmem-2.1.po                  3408   1840      1     47 2018-12-20 05:30:11 UTC+0000
. 0xfffffa8003caf060:notepad.exe                      700   1840      2     57 2018-12-20 05:29:22 UTC+0000
``` 

The processes under explorer.exe show the user started processes. 

### Notepad

The documentation needed is most likely done in notepad.exe. The next step is to extract the memory of notepad which has a PID of 700. Sadly the notepad plugin of volatility does not work with Windows 7.

```
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 memdump -D c:\dump\notepad\ -p 700

Volatility Foundation Volatility Framework 2.6.1
************************************************************************
Writing notepad.exe [   700] to 700.dmp
```

Use [strings](https://docs.microsoft.com/en-us/sysinternals/downloads/strings) to obtain all strings present in the memory dump and put them in a text file

```
strings64.exe -n 20 700.dmp > string.txt
```

The last thing to do is browse through the text file and find the flag.	This is tedious work, but at some point the following string will present itself:

```
AVKH{guvf_j4f_gu3_rnfl_bar}
```

This looks like a ROT13 encryption. Decrypting it yields

```
NIXU{####_###_###_####_###}
```

### Improvements

To speed up the search, the places to look can be reduced by excluding the memory dumps of loaded DLL's. To do so, use the vadtree options and find the memory locations associated with the heap.

First create a graph with the VAD tree
```shell
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 vadtree --output=dot --output-file=graph.dot -p 700
```

Of interest are the red squares in the dot file. Next, extract all the vad nodes.
```
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 vaddump -D c:\dump\notepad\ -p 700 
```

Apply strings on the interesting ones
```
strings64.exe -n 10 notepad.exe.8c45060.0x0000000000390000-0x000000000048ffff.dmp

<snip>
n the right track! try harder
AVKH{guvf_j4f_gu3_rnfl_bar}try harder
you're on the right track! try harder
 !"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~
0P0P``````````00
P0`p`p`@pp00`0
Pp00``p`0PP
@@0p`0 @P`
`````0000ppppppp
```

And the answer is presented much faster.