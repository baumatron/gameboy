namespace GameBoy
{
    public class GameBoy
    {
        public GameBoy()
        {
            var ram = new Ram();
            var cartridge = new Cartridge();
            _memory = new MemoryController(ram, cartridge);
            _cpu = new Cpu(_memory);
            _compositor = new Compositor(_memory);
        }

        public void LoadCartridge(string file)
        {
            var ram = new Ram();
            var cartridge = new Cartridge();
            cartridge.LoadFromFile(file);
            _memory = new MemoryController(ram, cartridge);
            _cpu = new Cpu(_memory);
            _compositor = new Compositor(_memory);

            _cpu.Init();
        }

        public void Run()
        {
            // TODO: Proper timing
            bool quit = false;

            // TODO: More likely, this needs to be next vsync, and single lines need to be rendered instead of the entire screen

            do
            {
                Step();

                if (_cpu.clock > 1000000)
                {
                    quit = true;
                }
            } while (!quit);
        }

        public void Step()
        {
            _cpu.ExecuteNextInstruction();

            if (_cpu.clock > nextScanlineClock)
            {
                _compositor.RenderScanline();
                var LY = _cpu._memory.Read(0xFF44);
                _cpu._memory.WriteDirect(0xFF44, (byte)((LY + 1) % 154));
                nextScanlineClock += scanlineCycleDelay;
            }

            if (_cpu._memory.Read(0xFF44) == 144)
            {
                _compositor.SaveScreenshot();
            }
        }

        public void RunToAddress(ushort address)
        {
            do
            {
                Step();
            } while (_cpu.PC != address);
        }

        const ushort scanlineCycleDelay = 70224 / 154;

        int nextScanlineClock = scanlineCycleDelay;

        public Cpu _cpu;

        public IMemory _memory;

        Compositor _compositor;
    }
}