namespace GameBoy
{
    partial class Cartridge
    {
        public void LoadFromFile(string file)
        {
            _rom = System.IO.File.ReadAllBytes(file);
        }

        public void LoadFromArray(byte[] source)
        {
            _rom = source;
        }

        public byte Read(ushort address)
        {
            if (address < 0x4000)
            {
                // Always fixed at bank 00... probably
                if (address >= _rom.Length)
                {
                    return 0xFF; // What would a realy gb cart do?
                }
                return _rom[address];
            }
            else
            {
                if (address >= 0x8000)
                {
                    throw new System.Exception($"Cartridge.Read expects an address without offset. Got address: {address}");
                }

                ushort bankOffset = (ushort)((_firstBankIndex - 1) * 0x4000);
                ushort romAddress = (ushort)(bankOffset + address);
                if (romAddress >= _rom.Length)
                {
                    return 0xFF; // what would a real gb cart do?
                }
                return _rom[romAddress];
            }
        }

        public void SetFirstBankIndex(ushort index)
        {
            if (index < 0)
            {
                throw new System.Exception($"Invalid cartridge bank index: {index}");
            }

            _firstBankIndex = index;
        }

        private byte[] _rom = new byte[0];

        public byte[] Rom { get { return _rom; } }

        private ushort _firstBankIndex = 1;
    }
}