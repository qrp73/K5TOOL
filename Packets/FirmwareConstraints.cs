namespace K5TOOL.Packets
{
    public static class FirmwareConstraints
    {
        public static int MinEepromAddr = 0x0000;
        public static int MaxEepromAddr = 0x2000-1;
        public static int MinFlashAddr  = 0x0000;
        public static int MaxFlashAddr  = 0xf000-1; // tested with egzumer/1o11_fagci_spectrum_v0.6_packed.bin (size=0xeff4, offsetFinal=0xf000)
    }
}
