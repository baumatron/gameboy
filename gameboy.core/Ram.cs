namespace GameBoy
{
    class Ram: MemoryBase
    {
        public Ram()
        {
            _ram = new byte[0x10000];
        }
        public override void Write(ushort address, byte value)
        {
            WriteDirect(address, value);
        }

        public override void WriteDirect(ushort address, byte value)
        {
            _ram[address] = value;
        }

        public override byte Read(ushort address)
        {
            return _ram[address];
        }

        byte[] _ram;
    }
}