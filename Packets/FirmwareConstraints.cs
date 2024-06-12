namespace K5TOOL.Packets
{
    public static class FirmwareConstraints
    {
        public static int MinEepromAddr = 0x0000;
        public static int MaxEepromAddr = 0x2000-1;
        public static int MinFlashAddr  = 0x0000;
        public static int MaxFlashAddr  = 0xec00-1; // tested with egzumer/uv-k5-firmware-custom (uses 0xec00)
    }
}
