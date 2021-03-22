# Device Control Pwnel

## Problem

https://thenixuchallenge.com/c/overflow/

You found an exposed device controller interface... 

nc overflow.thenixuchallenge.com 20191 

[device.tgz](https://thenixuchallenge.com/c/overflow/static/device.tgz)

## Solution

Pass four strings that are too large as menu input to set the variable `id` to 0.

### Introduction

After downloading the device.tgz and extracting it we get two files: the source device.c and the executable device. Let's look at the source first. Note: the link to the challenge is called overflow...

### device.c

The code is straight forward: in main two flags are loaded and it presents a menu. After choosing an option the corresponding function is called and the menu is returned afterwards.

In the function `admin_menu` the first flag is printed. The only thing left to do is to call this function. To do so, we need to get an id of 0. However, it is set 999999 at the start of the mainloop. Lucky for us, `char choice[8]` is declared just before `int id`, and is filled using `fgets`. The call to `fgets` reads up to 126 character, 118 too many to fit in choice. Because `id` is next to `choice` in memory, it will get overwritten by the next 4 bytes given by the input.

The type `int` has a size of 4 bytes, and all of them need to be changed to 0. Luckily `fgets` adds a `\0` at the end of the string, and if the string does not fit in `choice` this 0 will be placed in the adjacent piece of memory. So by setting the input to a string that is to long four times all the four bytes of the id can be overwritten.

### Control panel

Firing up nc and connecting to the provided address presents us with a menu, as expected from the source.

```console
Welcome to ACME corp control panel

1) List devices
2) Add device
3) Edit device
4) Process
5) Quit
```

Let's overwrite `id` by entering strings that are too long (also include the newline `\n` when counting the number of characters).

```console
1234567890
123456789
12345678
1234567
```

And behold, an additional option presents itself:

```console
1) List devices
2) Add device
3) Edit device
4) Process
5) Quit
8) Admin
```

Choosing option 8 shows the flag

```console
Admin functionality is not fully implemented but here's a flag:
NIXU{######_######_########}
```
