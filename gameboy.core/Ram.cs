namespace GameBoy
{
    class Ram: IMemory
    {
        public Ram()
        {
            _ram = new byte[0xFFFF];
        }
        public void Write(ushort address, byte value)
        {
            _ram[address] = value;
        }

        public void WriteWord(ushort address, ushort value)
        {
            _ram[address] = (byte)(value & 0xFF);
            _ram[address+1] = (byte)(value >> 8);
        }

        public byte Read(ushort address)
        {
            return _ram[address];
        }

        public ushort ReadWord(ushort address)
        {
            return (ushort)(Read(address) | Read((ushort)(address + 1)) << 8);
        }

        byte[] _ram;
    }
}