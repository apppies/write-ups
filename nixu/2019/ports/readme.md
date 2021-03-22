# Ports

## Problem

The ports might be interesting, is there a way to hide information into numbers?

[ports.pcap](https://thenixuchallenge.com/c/ports/static/ports.pcap)

## Solution

Get all destination ports, interpret as ASCII codes, Base64 decode and ROT13 decrypt to get the answer.

### Introduction

Opening the pcap file in Wireshark shows high numbers for the TCP source ports and TCP low numbers for the destination ports. These destination ports seem to be somewhere within the ASCII range. So first focus is on that

### TShark

Using powershell and tshark all destination ports are obtained and used to build a string

```Powershell
# Get all destination ports and interpret as ASCII codes
$tshark = "C:\Wireshark\tshark.exe"
$ports = . $tshark -r "C:\NIXU\ports.pcap" -T fields -e tcp.dstport
$data = ($ports | Foreach-Object { [char][int]$_ }) -join ''
```

This results in `QVZLSHtmbHpvYnlmX25hcV9haHpvcmVmX25lcl9zaGFfZ2JfY3lubF9qdmd1fQ==` what typically looks like Base64 encoded data. Decoding this using powershell results in 

```Powershell
$data = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($data))

AVKH{flzobyf_naq_ahzoref_ner_sha_gb_cynl_jvgu}
```

That looks like a flag, but does not start with NIXU. It does start with the ROT13 version of NIXU. Applying an additional ROT13 to decode yields

```
NIXU{#######_###_#######_###_###_##_####_####}
```