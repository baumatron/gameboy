namespace GameBoy
{
    public interface IMemory
    {
        void Write(ushort address, byte value);

        void WriteWord(ushort address, ushort value);

        void WriteDirect(ushort address, byte value);

        byte Read(ushort address);

        ushort ReadWord(ushort address);
    }
}