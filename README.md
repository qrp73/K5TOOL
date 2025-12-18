# K5TOOL
UV-K5/UV-5R Toolkit Utility for Reading/Writing EEPROM and Flashing Firmware

I created this tool to provide a more robust protocol with logging capabilities for working with the UV-K5 radio.

This utility allows you to read and write EEPROM, as well as flash firmware images. It includes commands for packing and unpacking firmware images and supports both encrypted and plain formats.

The tool supports firmware sizes up to 0xF000 (61,440) bytes and has been tested with large firmware files.

Additionally, the tool can be used as a software simulator for the UV-K5 bootloader, which is useful for testing firmware updater software.

The tool supports different bootloader versions, including bootloader v5.00.01, it uses AES encryption internally, so you don't need to care about it.

## Notes

**Note** that newer radio models may use a different processor and therefore require their own specific firmware. These newer revisions can be identified by the V2 marking under the battery compartment, a V3 revision also exists. If you have one of these newer models and flashed firmware intended for an older revision, the device will not boot. Old V1 use processor DP32G030 and usual custom firmware are developed for this processor.

[![Image](https://github.com/user-attachments/assets/133a9232-0bfc-4044-abc2-a9ec4a44ceaa)](https://github.com/user-attachments/assets/654face1-c150-4340-877c-db2670b02f78)

Below are the recovery firmware images for different radio models:

**Recovery for V1 (processor DP32G030, bootloader 2, use -wrflash):** [RT590_v2.01.32_publish.zip](https://github.com/user-attachments/files/24219076/RT590_v2.01.32_publish.zip)

**Recovery for V1 (processor DP32G030, bootloader 5, use -wrflash):** [ORIGINAL_v5.00.05.bin.zip](https://github.com/user-attachments/files/24219092/ORIGINAL_v5.00.05.bin.zip)

**Recovery for V2 (PCB version V1.8, processor PY32F030, use -wrflash):** [k5_py030_v1.01.07.zip](https://github.com/user-attachments/files/24218399/k5_py030_v1.01.07.zip), [k5_py030_v1.02.01.zip](https://github.com/user-attachments/files/24218396/k5_py030_v1.02.01.zip)

**Recovery for V3 (processor PY32F071, use -wrflashraw):** [K5_V3_PY32F071_firmware.zip](https://github.com/user-attachments/files/24218416/K5_V3_PY32F071_firmware.zip)


## Prerequisites and Dependencies

The tool requires the Mono runtime. 
On Windows, Mono is available just out of the box.

On Linux, you can install Mono runtime package with the following command:
```
sudo apt install mono-runtime
```

## Installation

1. **Download and Unzip:**
   - Download the ZIP file from the following link: [K5TOOL Releases](https://github.com/qrp73/K5TOOL/releases).
   - Unzip the downloaded file.

2. **Run on Linux/MacOS**
   - On Linux or MacOS, you may need to set execute permissions for the `k5tool` script:
```bash
sudo chmod +x k5tool
```

3. **Run on Windows:**
   - On Windows, you can run the tool from the console as `K5TOOL.exe`.

## Compile

You can compile the source code using MonoDevelop (which I use) or Visual Studio.

On Windows, run the compiled tool from the command line as `K5TOOL.exe`.

On Linux/MacOS, run the compiled tool using the provided bash launcher script `k5tool`.


## Build and Install on Linux/macOS

```
$ sudo apt install mono-runtime mono-mcs
$ git clone https://github.com/qrp73/K5TOOL.git
$ cd K5TOOL
$ mkdir build && cd build
$ cmake ..
$ make
$ sudo make install
```

Testing:
```
$ k5tool -port /dev/ttyUSB0 -hello
```

## Uninstall on Linux/macOS

Just remove ```/usr/local/lib/k5tool``` folder and ```/usr/local/bin/k5tool``` file. Thats it.


## Logging

The tool generates a detailed communication log that includes all error details. You can view the log in the `K5TOOL.log` file (current folder).

- Lines starting with the `rx` and `tx` tags represent raw communication data exchanged with the radio.
- Lines starting with the `RX` and `TX` tags contain decrypted messages.
- Lines starting with the `recv` and `send` tags show parsed protocol messages.

If an error occurs, you can find all communication details and error information in the log.

When the tool starts, it creates a backup of the previous log as `K5TOOL.log.bak` and begins writing a new `K5TOOL.log` file. The old `K5TOOL.log.bak` file is deleted. If an error occurs, make sure to copy the `K5TOOL.log` file for analysis before running the tool again to avoid losing important information.


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

## Specify Serial Port Name

By default, the tool uses the last serial port from the available list. However, if you want to specify a different port, you can do so by adding the `-port <portName>` argument:

For Linux/MacOS:
```
./k5tool -port /dev/ttyUSB1 -hello
```
For Windows:
```
./k5tool -port COM3 -hello
```

You can use the `-port` option without specifying a port name to list all available serial ports on the system:
```
$ ./k5tool -port
/dev/ttyS0
/dev/ttyUSB0
```

**Note:** Some ports may not appear in this list. For example, the built-in serial port on a Raspberry Pi might show up as `/dev/ttyS0` but may require using the name `/dev/ttyAMA0`, which might not be listed. This is specific to the operating system.


## Reboot the Radio and Display the Bootloader Version

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

**Note:** The value displayed is not in Volts but is a raw reading from the ADC.



## Read EEPROM data from UV-K5 radio

```$ ./k5tool -rdee [<offset> <size>] [<fileName>]```

You can specify optional parameters for the starting address (offset) and the length (size) of the read block. The file name is also optional. 

By default, the parameters have the following values:
- `<offset>` = 0x0000
- `<size>` = 0x2000
- `<fileName>` = 'eeprom-{hex-offset}-{hex-size}.raw'

**Note:** Reading the EEPROM should be done when the radio is operating in normal mode (not to be confused with flashing mode). 
Before running read/write EEPROM command:
1. Disconnect the cable.
2. Turn off the radio.
3. Turn the radio back on (DO NOT HOLD the Push-to-Talk button!). 
4. Reconnect the cable and then execute the command.

This ensures that the radio is in the correct mode for reading/writing EEPROM data.

### Read Full EEPROM Dump
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

It will create a full backup image file named `eeprom-0000-2000.raw`.

You can specify a different filename with the `-rdee` option:
```
$ ./k5tool -rdee eeprom-full.raw
```

### Read UV-K5 Calibration Backup dump

```
$ ./k5tool -rdee 0x1e00 0x0200 eeprom-calib.raw
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

In this example, `0x1e00` is the starting address, `0x200` is the size of the data block to read, and `eeprom-calib.raw` is the file where the calibration data will be saved.


## Write EEPROM from File

```$ ./k5tool -wree [<offset>] <fileName>```

You can specify optional parameter for the starting address (offset).

By default, the parameter have the following value:
- `<offset>` = 0x0000

### Write Full EEPROM Backup Dump to UV-K5 from a File
```
$ ./k5tool -wree eeprom-0000-2000.raw
Opening /dev/ttyUSB0
Handshake...
   Firmware:         "2.01.32"
   HasCustomAesKey:  0
   IsPasswordLocked: 0
Write EEPROM offset=0x0000, size=0x2000 from eeprom-0000-2000.raw
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

## Flash Firmware Image to the Radio

```-wrflash <fileName>```

This command flashes the firmware image in the standard format (encrypted and with a checksum). 
It checks the checksum, if the checksum is incorrect, the firmware will not be flashed. 
It is recommended to use this command and the standard format for flashing to avoid potential errors in the firmware image file.

**Note:** This command should be executed in flashing mode. To switch to flashing mode, follow these steps:

1. Disconnect the cable.
2. Turn off the radio.
3. Turn the radio back on while holding down the PTT (Push-to-Talk) button. The LED should light up.
4. Reconnect the cable and then execute the command.

This ensures that the radio is in the correct mode for flashing the firmware image.

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

If the bootloader returns an error when writing the first block, it means that the bootloader is refusing to accept the firmware with the specified version.


## Flash Raw (Decrypted) Firmware Image to the Radio

```-wrflashraw [<version>] <fileName>```

This command is used to write the firmware in raw format (as is), meaning you can use the binary file resulting from compilation. This format does not include a checksum, so it cannot be verified. Make sure you are using the correct unencrypted firmware file before writing it.

You can specify an optional `<version>` parameter to provide the version string required by the bootloader to unlock flash mode.

By default, the parameter value depends on the radio bootloader version:

- For bootloader v2: `<version>` = "2.01.23"
- For bootloader v5: `<version>` = "5.00.05"

You can use the `*` symbol as version string to bypass the bootloader's version check.

**Note:** This command should be executed in flashing mode. To switch to flashing mode, follow these steps:

1. Disconnect the cable.
2. Turn off the radio.
3. Turn the radio back on while holding down the PTT (Push-to-Talk) button. The LED should light up.
4. Reconnect the cable and then execute the command.

This ensures that the radio is in the correct mode for flashing the firmware image.

```
$ ./k5tool -wrflashraw \*.01.32 RT590-2.01.32.raw
```
where 
*.01.32 is version sent to the radio to unlock flashing.

You can ommit version argument:
```
$ ./k5tool -wrflashraw RT590-2.01.32.raw
```
in this case it will use default version "2.01.23" to unlock flashing mode.


## Unpack firmware image

```$ ./k5tool -unpack <fileName> [<outputName>]```

This command converts the firmware from the standard format (encrypted and versioned, with check sum) to the raw format, which is the format used for writing the firmware to the microcontroller's memory.

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

## Pack Firmware Image

```$ ./k5tool -pack <version> <fileName> [<outputName>]```

This command packs a raw firmware image (the compilation result, i.e., data in the format used for writing to the microcontroller's memory) into the packed format used for UV-K5 firmware. The packed format is encrypted and includes a checksum for verification and a version string for the bootloader.

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

## UV-K5 bootloader simulator

```$ ./k5tool -port /dev/ttyUSB1 -simula```

This command is used to simulate the bootloader. It can be useful for testing other firmware update software or for dumping the firmware from the original flasher without connecting to a real radio.

Where ```/dev/ttyUSB1``` is a name of serial port which is used for UV-K5 device simulation.


## Protocol sniffer mode

```
$ ./k5tool -sniffer
```

Can be used for diagnostic purpose. In this mode the tool don't sends anything, just monitor for packets on RxD line in a loop and prints decrypted packets to console.


## Parse hex data

```
$ ./k5tool -parse abcd2800036930e66bd657156c7087606533c7b2246c14e62e910d402135d5401303e980166c14e62e910d40decadcba
48 bytes
rx: abcd2800036930e66bd657156c7087606533c7b2246c14e62e910d402135d5401303e980166c14e62e910d40decadcba
RX: 1505240045475a554d45522076302e32320000000000000000000000000000000000000000000000
recv PacketHelloAck {
  HdrSize=36
  Version="EGZUMER v0.22"
  HasCustomAesKey=0
  IsPasswordLocked=0
  Padding[0]=0x00
  Padding[1]=0x00
  Challenge[0]=0x00000000
  Challenge[1]=0x00000000
  Challenge[2]=0x00000000
  Challenge[3]=0x00000000
}
Done

$ ./k5tool -parse-plain 1505240045475a554d45522076302e32320000000000000000000000000000000000000000000000
40 bytes
RX: 1505240045475a554d45522076302e32320000000000000000000000000000000000000000000000
recv PacketHelloAck {
  HdrSize=36
  Version="EGZUMER v0.22"
  HasCustomAesKey=0
  IsPasswordLocked=0
  Padding[0]=0x00
  Padding[1]=0x00
  Challenge[0]=0x00000000
  Challenge[1]=0x00000000
  Challenge[2]=0x00000000
  Challenge[3]=0x00000000
}
Done
```

