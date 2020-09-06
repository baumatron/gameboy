namespace GameBoy
{
    class GameBoy
    {
        public GameBoy()
        {
            var ram = new Ram();
            var cartridge = new Cartridge();
            _memory = new MemoryController(ram, cartridge);
            _cpu = new Cpu(_memory);
        }

        Cpu _cpu;

        IMemory _memory;
    }
}