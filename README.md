# K5TOOL
UV-K5 toolkit utility to read/write EEPROM and flashing firmware

I created this tool to get more robust protocol with logging to work with UV-K5 radio.

This tool allows to read/write EEPROM and write flash images.
It also include pack/unpack commands and support both formats (encrypted/plain) of firmware images.

## Install

Just download zip file and unzip it: https://github.com/qrp73/K5TOOL/releases

On Linux/MacOS it may need to set execute permission for k5tool script:
```sudo chmod +x k5tool```

On Windows you can run it from console as usual K5TOOL.exe.


## Dependencies

The tool requires mono runtime. On Windows it is available out of the box.

On Linux you can install it with:
```
sudo apt install mono-runtime
```

## Compile

You can compile source code with MonoDevelop (which I'm using) or Visual Studio.
On Windows you can run it from command line as usual executable K5TOOL.exe.
On Linux/MacOS you can use it as ```mono K5TOOL.exe```.
In order to get shortened command on Linux/MacOS you can create text file ```k5tool``` with the following content:
```
#!/bin/bash
mono ./K5TOOL.exe "$@"
```
and make it executable with ```chmod +x k5tool```

After that you can run it as Linux command k5tool.


## Logging

The tool writes detailed communication log with all error details. You can read log file in K5TOOL.log.

```rx``` and ```tx``` tags shows raw communication with radio.

```RX``` and ```TX``` tags shows decrypted messages.

```recv``` and ```send``` tags shows parsed protocol messages

In case of any error you can find all communication and detailed error info in the log.

When the tool is started it copy log backup to K5TOOL.log.bak and starts the new K5TOOL.log file.
Old K5TOOL.log.bak is removed. So be careful if some error happens, take a copy of K5TOOL.log for analysis before trying to run the tool again.


## Check connection

```
$ ./k5tool -hello
Opening /dev/ttyUSB0
Handshake...
   Firmware:         "2.01.32"
   HasCustomAesKey:  0
   IsPasswordLocked: 0
Done
```

## Specify serial port name

By default the tool using the last serial port from available list. But if you want to specify other port, you can do it by adding ```-port <portName>``` argument:

For Linux/MacOS:
```
./k5tool -port /dev/ttyUSB1 -hello
```
for Windows:
```
./k5tool -port COM3 -hello
```

## Reboot the radio and show bootloader version

```
$ ./k5tool -reboot
Opening /dev/ttyUSB0
Handshake...
   Firmware:         "2.01.32"
   HasCustomAesKey:  0
   IsPasswordLocked: 0
Reboot device...
   Bootloader:       "2.00.06"
Done
```

## Read battery ADC

```
$ ./k5tool -rdadc
Opening /dev/ttyUSB0
Handshake...
   Firmware:         "2.01.32"
   HasCustomAesKey:  0
   IsPasswordLocked: 0
Read ADC...
   Voltage:          2190
   Current:          0
Done
```

## Read full EEPROM of UV-K5 radio

```
$ ./k5tool -rdee
Opening /dev/ttyUSB0
Handshake...
   Firmware:         "2.01.32"
   HasCustomAesKey:  0
   IsPasswordLocked: 0
Read EEPROM offset=0x0000, size=0x2000 to eeprom-0000-2000.raw
   Read 0000...0080: OK
   Read 0080...0100: OK
   Read 0100...0180: OK
...
   Read 1f00...1f80: OK
   Read 1f80...2000: OK
Done
```
it will create backup image ```eeprom-0000-2000.raw```

You can specify filename:
```
$ ./k5tool -rdee eeprom-full.raw
```

## Read calibration backup of UV-K5

```
$ ./k5tool -rdee 0x1E00 0x0200 eeprom-calib.raw
Opening /dev/ttyUSB0
Handshake...
   Firmware:         "2.01.32"
   HasCustomAesKey:  0
   IsPasswordLocked: 0
Read EEPROM offset=0x1e00, size=0x0200 to eeprom-calib.raw
   Read 1e00...1e80: OK
   Read 1e80...1f00: OK
   Read 1f00...1f80: OK
   Read 1f80...2000: OK
Done
```
where:
0x1E00 is starting offset to read from EEPROM
0x0200 block size to read from EEPROM

## Write full EEPROM from backup file

```
$ ./k5tool -wree eeprom-full.raw
Opening /dev/ttyUSB0
Handshake...
   Firmware:         "2.01.32"
   HasCustomAesKey:  0
   IsPasswordLocked: 0
Write EEPROM offset=0x0000, size=0x2000 from eeprom-full.raw
   Write 0000...0080: OK
   Write 0080...0100: OK
   Write 0100...0180: OK
   Write 0180...0200: OK
   Write 0200...0280: OK
   Write 0280...0300: OK
...
   Write 1e80...1f00: OK
   Write 1f00...1f80: OK
   Write 1f80...2000: OK
Done
```

## Flash firmware image to radio

Note: this command should be executed in flashing mode. You can switch to flashing mode by disconnect cable, turn off radio and then turn on with pressed PTT button. The led should light. Then connect the cable and execute command.

```
$ ./k5tool -wrflash RT590_v2.01.32_publish.bin
Opening /dev/ttyUSB0
Read packed FLASH image from RT590_v2.01.32_publish.bin...
Unpack image...
CRC check passed...
Write FLASH size=0xe5dc
Waiting for bootloader beacon...
   Bootloader: 2.00.06
Send version "2.01.32"...
   Bootloader: 2.00.06
   Write 0000...0100: OK
   Write 0100...0200: OK
   Write 0200...0300: OK
   Write 0300...0400: OK
...
   Write e300...e400: OK
   Write e400...e500: OK
   Write e500...e5dc: OK
Done
```

## Flash raw firmware image (decrypted) to radio

Note: this command should be executed in flashing mode.

Warning: this command don't check firmware image content. Make sure you're using proper unpacked (decrypted) image.

```
$ ./k5tool -wrflashraw *.01.32 RT590-2.01.32.raw
```
where 
*.01.32 is version sent to the radio to unlock flashing.

You can ommit version argument:
```
$ ./k5tool -wrflashraw RT590-2.01.32.raw
```
in this case it will use default version "2.01.23" to unlock flashing mode.


## Unpack firmware image

```
$ ./k5tool -unpack RT590_v2.01.32_publish.bin
CRC check passed...
   Version: 2.01.32
Write RT590_v2.01.32_publish-2.01.32.raw...
Done
```
it will write file ```RT590_v2.01.32_publish-{version}.bin``` where {version} is version encoded in the image.

you can also specify output file manually:
```
./k5tool -unpack RT590_v2.01.32_publish.bin RT590.raw
CRC check passed...
   Version: 2.01.32
Write RT590.raw...
Done
```

## Pack firmware image

```
$ ./k5tool -pack 2.01.32 RT590.raw
Write RT590.bin...
Done
```
where
2.01.32 is version string which will be encoded in the image.

It will write RT590.bin encrypted flash image

You can also specify output file manually:
```
$ ./k5tool -pack 2.01.32 RT590.raw RT590-encoded.bin
Write RT590-encoded.bin...
Done
```
