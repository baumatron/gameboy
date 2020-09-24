namespace GameBoy
{
    class MemoryController: MemoryBase
    {
        public MemoryController()
        {
        }

        public MemoryController(Ram ram, Cartridge cartridge)
        {
            _ram = ram;
            _cartridge = cartridge;
        }

        public override void Write(ushort address, byte value)
        {
            switch (address)
            {
                case 0xFF44:
                    // Writes to LY register reset the counter
                    value = 0;
                    break;
                default:
                    break;
            }
            // TODO: What happens if a write to < 0x8000 happens? As is, nothing will happen since reads go to the cartridge
            WriteDirect(address, value);
        }

        public override void WriteDirect(ushort address, byte value)
        {
            _ram.Write(address, value);
        }

        public override byte Read(ushort address)
        {
            // If there is a cartridge instance, use that, otherwise just use the memory array.
            // This makes testing the CPU simpler as there is no need to worry about which address you are reading or writing,
            // it's all just memory
            // TODO: Really there should be an interface defined for memory, then there should be an implementation for testing the CPU that is dumb
            if (address < 0x8000 && _cartridge != null) 
            {
                // This address range maps to the cartridge memory controller
                return _cartridge.Read(address);
            }
            else
            {
                if (address >= 0xE000 && address <= 0xFDFF)
                {
                    // This address range maps to 0xC000-0xDDFF
                    address = (ushort)(address - 0xE000 + 0xC000);
                }
                return _ram.Read(address);
            }
        }

        IMemory _ram = null;

        Cartridge _cartridge = null;
    }
}