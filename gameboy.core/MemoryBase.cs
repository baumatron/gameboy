namespace GameBoy
{
    abstract class MemoryBase: IMemory
    {
        public abstract void Write(ushort address, byte value);

        public void WriteWord(ushort address, ushort value)
        {
            Write(address, (byte)(value & 0xFF));
            Write((ushort)(address + 1), (byte)(value >> 8));
        }

        public abstract byte Read(ushort address);

        public ushort ReadWord(ushort address)
        {
            return (ushort)(Read(address) | Read((ushort)(address + 1)) << 8);
        }
    }
}