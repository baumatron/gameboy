namespace GameBoy
{
    class Memory
    {
        public void Write(ushort address, byte value)
        {
            memory[address] = value;
        }

        public void WriteWord(ushort address, ushort value)
        {
            memory[address] = (byte)(value & 0xFF);
            memory[address+1] = (byte)(value >> 8);
        }

        public byte Read(ushort address)
        {
            return memory[address];
        }

        public ushort ReadWord(ushort address)
        {
            return (ushort)(memory[address] | memory[address+1] << 8);
        }

        public void Init()
        {
            memory = new byte[0xFFFF];
        }

        // TODO: This guy needs to map i/o and handle
        //       memory bank controllers and such.
        //       Also, something needs to handle
        //       loading of cartridges and selecting a MBC class.
        byte[] memory;
    }
}