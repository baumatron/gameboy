using Xunit;
using Xunit.Abstractions;
using GameBoy;
using System;

namespace GameBoyTests
{
    public class ProgramTests
    {
        private readonly ITestOutputHelper output;

        public ProgramTests(ITestOutputHelper output)
        {
            this.output = output;
        }
    
        [Fact]
        public void TestPrintProgram()
        {
            var cart = new Cartridge();
            cart.LoadFromFile("../../../test_programs/print_test.rom");

            var ram = new Ram();

            var memoryController = new MemoryController(ram, cart);
            var cpu = new Cpu(memoryController);

            cpu.Init();
            
            // Need to figure out halt...
            bool done = false;

            int iterations = 0;
            while (!done && iterations++ < 10000)
            {
                cpu.ExecuteNextInstruction();
            } 
        }
    }
}