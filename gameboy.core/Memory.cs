namespace GameBoy
{
    class Memory
    {
        public void Write(ushort address, byte value)
        {
            memory[address] = value;
        }

        public byte Read(ushort address)
        {
            return memory[address];
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