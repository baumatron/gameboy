namespace GameBoy
{
    interface IMemory
    {
        void Write(ushort address, byte value);

        void WriteWord(ushort address, ushort value);

        byte Read(ushort address);

        ushort ReadWord(ushort address);
    }
}