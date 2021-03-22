# Bad memories - part 2

## Problem

The lead graphical designer at ACME has noticed some kind of strange activity on her computer. Their IT support believes it is a false positive and the computer will fix itself after turning it off and on again. However, the user managed to take a memory dump just before the crash. She mentioned something about lost files before the call audio got all garbled.

This is part 2 of 5 in a memory dump analysis challenge. The [memory dump file](https://thenixuchallenge.com/c/bad_memories_part1/) can be found from the first part.

## Solution

Find the deleted text file in the MFT, recover and decode it.

### Scanning for files

There are two places to look for files: resident files in the MFT or non-resident files cached in memory. Let's first look at the MFT table.

```
python vol.py -f c:\dump\mem.dmp --profile Win7SP1x64 mftparser > C:\dump\badmem2\mftparser.txt
```

After extracting the MFT and opening it in notepad++ it is time to look for interesting files. Searching for ".txt" gives some interesting files:

- `$Recycle.Bin\S-1-5-21-1610009768-122519599-941061767-1003\$INA5LHA.txt`. Which is resident and has the following content:

```
0000000000: 01 00 00 00 00 00 00 00 f8 01 00 00 00 00 00 00   ................
0000000010: c0 c2 57 e7 24 98 d4 01 43 00 3a 00 5c 00 55 00   ..W.$...C.:.\.U.
0000000020: 73 00 65 00 72 00 73 00 5c 00 41 00 6c 00 69 00   s.e.r.s.\.A.l.i.
0000000030: 63 00 65 00 5c 00 44 00 6f 00 63 00 75 00 6d 00   c.e.\.D.o.c.u.m.
0000000040: 65 00 6e 00 74 00 73 00 5c 00 66 00 6c 00 61 00   e.n.t.s.\.f.l.a.
0000000050: 67 00 2e 00 74 00 78 00 74 00 00 00 00 00 00 00   g...t.x.t.......
```

Two links to a flag.txt file

- `Users\Alice\AppData\Roaming\Microsoft\Windows\Recent\flag.txt.lnk`
- `Users\Alice\AppData\Roaming\Microsoft\Windows\Recent\FLAGTX~1.LNK`

- `$Recycle.Bin\S-1-5-21-1610009768-122519599-941061767-1003\$RNA5LHA.txt` with content

```
0000000000: 4d 44 45 77 4d 44 45 78 4d 54 41 67 4d 44 45 77   MDEwMDExMTAgMDEw
0000000010: 4d 44 45 77 4d 44 45 67 4d 44 45 77 4d 54 45 77   MDEwMDEgMDEwMTEw
0000000020: 4d 44 41 67 4d 44 45 77 4d 54 41 78 4d 44 45 67   MDAgMDEwMTAxMDEg
0000000030: 4d 44 45 78 4d 54 45 77 4d 54 45 67 4d 44 45 78   MDExMTEwMTEgMDEx
0000000040: 4d 54 41 78 4d 54 45 67 4d 44 45 78 4d 44 41 77   MTAxMTEgMDExMDAw
0000000050: 4d 44 45 67 4d 44 45 78 4d 44 45 77 4d 44 45 67   MDEgMDExMDEwMDEg
0000000060: 4d 44 45 78 4d 54 41 78 4d 44 41 67 4d 44 45 77   MDExMTAxMDAgMDEw
0000000070: 4d 54 45 78 4d 54 45 67 4d 44 45 78 4d 44 41 78   MTExMTEgMDExMDAx
0000000080: 4d 44 41 67 4d 44 45 78 4d 44 45 77 4d 44 45 67   MDAgMDExMDEwMDEg
0000000090: 4d 44 45 78 4d 44 41 78 4d 44 41 67 4d 44 45 78   MDExMDAxMDAgMDEx
00000000a0: 4d 44 45 78 4d 54 41 67 4d 44 45 78 4d 54 41 78   MDExMTAgMDExMTAx
00000000b0: 4d 44 41 67 4d 44 45 77 4d 54 45 78 4d 54 45 67   MDAgMDEwMTExMTEg
00000000c0: 4d 44 45 78 4d 44 45 77 4d 44 45 67 4d 44 45 77   MDExMDEwMDEgMDEw
00000000d0: 4d 54 45 78 4d 54 45 67 4d 44 45 78 4d 44 41 77   MTExMTEgMDExMDAw
00000000e0: 4d 44 45 67 4d 44 45 78 4d 44 45 78 4d 44 41 67   MDEgMDExMDExMDAg
00000000f0: 4d 44 45 78 4d 54 41 77 4d 54 41 67 4d 44 45 78   MDExMTAwMTAgMDEx
0000000100: 4d 44 41 78 4d 44 45 67 4d 44 45 78 4d 44 41 77   MDAxMDEgMDExMDAw
0000000110: 4d 44 45 67 4d 44 45 78 4d 44 41 78 4d 44 41 67   MDEgMDExMDAxMDAg
0000000120: 4d 44 45 78 4d 54 45 77 4d 44 45 67 4d 44 45 77   MDExMTEwMDEgMDEw
0000000130: 4d 54 45 78 4d 54 45 67 4d 44 45 78 4d 54 41 77   MTExMTEgMDExMTAw
0000000140: 4d 54 41 67 4d 44 45 78 4d 44 41 78 4d 44 45 67   MTAgMDExMDAxMDEg
0000000150: 4d 44 45 78 4d 44 45 78 4d 44 45 67 4d 44 45 78   MDExMDExMDEgMDEx
0000000160: 4d 44 45 78 4d 54 45 67 4d 44 45 78 4d 54 41 78   MDExMTEgMDExMTAx
0000000170: 4d 54 41 67 4d 44 45 78 4d 44 41 78 4d 44 45 67   MTAgMDExMDAxMDEg
0000000180: 4d 44 45 77 4d 54 45 78 4d 54 45 67 4d 44 45 78   MDEwMTExMTEgMDEx
0000000190: 4d 44 41 78 4d 44 45 67 4d 44 45 78 4d 54 41 78   MDAxMDEgMDExMTAx
00000001a0: 4d 54 41 67 4d 44 45 78 4d 44 45 77 4d 44 45 67   MTAgMDExMDEwMDEg
00000001b0: 4d 44 45 78 4d 44 41 78 4d 44 41 67 4d 44 45 78   MDExMDAxMDAgMDEx
00000001c0: 4d 44 41 78 4d 44 45 67 4d 44 45 78 4d 44 45 78   MDAxMDEgMDExMDEx
00000001d0: 4d 54 41 67 4d 44 45 78 4d 44 41 77 4d 54 45 67   MTAgMDExMDAwMTEg
00000001f0: 4d 54 45 78 4d 44 45 3d                           MTExMDE=
00000001e0: 4d 44 45 78 4d 44 41 78 4d 44 45 67 4d 44 45 78   MDExMDAxMDEgMDEx
```

The last one seem to have some Base64 encoded data. So let's focus on that. Decoding it yields a binary code:

```
01001110 01001001 01011000 01010101 01111011 01110111 01100001 01101001 01110100 01011111 01100100 01101001 01100100 01101110 01110100 01011111 01101001 01011111 01100001 01101100 01110010 01100101 01100001 01100100 01111001 01011111 01110010 01100101 01101101 01101111 01110110 01100101 01011111 01100101 01110110 01101001 01100100 01100101 01101110 01100011 01100101 01111101
```

Assuming each byte represents an ASCII character this translates to

```
NIXU{####_#####_#_#######_######_########}
```