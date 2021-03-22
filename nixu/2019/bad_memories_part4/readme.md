# Bad memories - part 4

## Problem

The lead graphical designer at ACME has noticed some kind of strange activity on her computer. Their IT support believes it is a false positive and the computer will fix itself after turning it off and on again. The user also complained about slow startup or some "black windows" appearing.

This is part 4 of 5 in a memory dump analysis challenge. The [memory dump file](https://thenixuchallenge.com/c/bad_memories_part1/) can be found from the first part.

## Solution

Extract the registry values in the Run key and obtain the flag.

### Reading the registry

Using Volatility the entire in-memory registry can be read. We are interested in the Run and RunOnce keys.

```
HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce
Microsoft\Windows\CurrentVersion\Run
```

To read those locations use

```
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 printkey -K "Software\Microsoft\Windows\CurrentVersion\Run"
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 printkey -K "Microsoft\Windows\CurrentVersion\Run"
```

And the latter one shows some interesting registry values!

```
python vol.py -f c:\dump\mem.dmp --profile=Win7SP1x64 printkey -K "Microsoft\Windows\CurrentVersion\Run"
Volatility Foundation Volatility Framework 2.6.1
Legend: (S) = Stable   (V) = Volatile

----------------------------
Registry: \SystemRoot\System32\Config\SOFTWARE
Key name: Run (S)
Last updated: 2018-12-20 04:03:46 UTC+0000

Subkeys:

Values:
REG_SZ        bginfo          : (S) C:\BGinfo\Bginfo.exe /accepteula /ic:\bginfo\bgconfig.bgi /timer:0
REG_SZ        VMware User Process : (S) "C:\Program Files\VMware\VMware Tools\vmtoolsd.exe" -n vmusr
REG_SZ        1               : (S) I
REG_SZ        2               : (S) X
REG_SZ        3               : (S) U
REG_SZ        4               : (S) {
REG_SZ        5               : (S) o
REG_SZ        6               : (S) h
REG_SZ        7               : (S) o
REG_SZ        8               : (S) !
REG_SZ        9               : (S) _
REG_SZ        10              : (S) u
REG_SZ        11              : (S) _
REG_SZ        12              : (S) k
REG_SZ        13              : (S) n
REG_SZ        14              : (S) o
REG_SZ        15              : (S) w
REG_SZ        16              : (S) _
REG_SZ        17              : (S) t
REG_SZ        18              : (S) h
REG_SZ        19              : (S) e
REG_SZ        20              : (S) s
REG_SZ        21              : (S) e
REG_SZ        22              : (S) _
REG_SZ        23              : (S) t
REG_SZ        24              : (S) r
REG_SZ        25              : (S) i
REG_SZ        26              : (S) x
REG_SZ        27              : (S) x
REG_SZ        28              : (S) }
REG_SZ        0               : (S) N
```