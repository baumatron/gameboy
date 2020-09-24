namespace GameBoy
{
    abstract class MemoryBase: IMemory
    {
        public abstract void Write(ushort address, byte value);

        public void WriteWord(ushort address, ushort value)
        {
            // Z80 is little endian, so need to write the low byte at address, then the
            // more significant byte at address + 1
            Write(address, (byte)(value & 0xFF));
            Write((ushort)(address + 1), (byte)(value >> 8));
        }

        public abstract byte Read(ushort address);

        public ushort ReadWord(ushort address)
        {
            // Z80 is little endian, so need to read the low byte at address, then the
            // more significant byte at address + 1
            return (ushort)(Read(address) | Read((ushort)(address + 1)) << 8);
        }

        public abstract void WriteDirect(ushort address, byte value);
    }
}