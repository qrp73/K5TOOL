https://github.com/sq5bpf/k5prog/blob/main/k5prog.c
https://github.com/DualTachyon/uv-k5-firmware/blob/main/app/uart.c

https://github.com/losehu/uv-k5-firmware-custom/blob/main/driver/uart.c
https://github.com/OneOfEleven/uv-k5-firmware-custom/blob/main/app/uart.c


================================================================================
firmware commands

0x0514 HelloReq         CMD_CONNECT
0x0515 HelloAck   
0x0529 ReadAdcReq       CMD_ADC_GET
0x052a ReadAdcAck
0x051b ReadEepromReq    CMD_EEPB_GET    [needs handshake]
0x051c ReadEepromAck
0x05dd RebootReq        CMD_RESET       [no reply]
0x051d WriteEepromReq   CMD_EEPB_PUT    [needs handshake]
0x051e WriteEepromAck

0x0527 ReadRssiReq      CMD_RSSI_GET
0x0528 ReadRssiAck

0x052f HelloTestReq     CMD_CONN_TEST


0x052D aes auth         CMD_MNG_LOGIN
    https://github.com/Lar-Sen/Quansheng_UV-K5_Kitchen/blob/main/v2.01.31_mods/utils/cmd052D_authenticate.py

0x051F Not implementing non-authentic command
0x0521 Not implementing non-authentic command

0x6902                  CMD_6902        [no reply] May force payload obfuscation?

===For RAM Reader MOD
0x05DB                  CMD_MEMB_GET    [no reply]  Raw serial dump whatever memory area

===For Read/Write BK4819 register MOD
0x0601                  CMD_BKREG_GET
0x0603                  CMD_BKREG_PUT



================================================================================
bootloader commands

0x0516 SetDigitalSignature?     CMD_NVR_PUT
    NVRAM block WRITE. Dangerous for OFW! Max payload size is 104 bytes
    https://github.com/amnemonic/Quansheng_UV-K5_Firmware/issues/107

0x0518 BootloaderAck    
0x0519 FlashWriteReq    CMD_ROMB_PUT
0x0530 FlashVersionReq  CMD_FLASH_ON
    Platform check ('02' or '*') then enter flash ROM write mode [needs handshake]
    https://github.com/sq5bpf/k5prog/blob/main/k5prog.c

    
https://github.com/egzumer/uvtools/blob/main/js/fwpack.js
https://github.com/egzumer/uvtools/blob/main/js/tool_patcher.js    
    
================================================================================
https://github.com/Lar-Sen/Quansheng_UV-K5_Kitchen/blob/main/v2.01.31_mods/utils/libuvk5.py
https://github.com/Lar-Sen/Quansheng_UV-K5_Kitchen/blob/main/v2.01.31_mods/utils/flash_tool.py
