namespace GameBoy
{
    class Ram: MemoryBase
    {
        public Ram()
        {
            _ram = new byte[0xFFFF];
        }
        public override void Write(ushort address, byte value)
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