using GameBoy;
using Xunit;

namespace GameBoyTests
{
    class InstructionTest
    {
        public InstructionTest(byte instruction)
        {
            _instruction = instruction;
            _cpu = new Cpu();
            _cpu.Init();

            _startingClock = _cpu.clock;

            _testPc = 0x100;
        }

        public virtual void PrepareTest()
        {
            _cpu.memory.Write(_testPc++, _instruction);
        }

        public virtual void ExecuteTestSubject()
        {
            _cpu.ExecuteNextInstruction();
        }

        public virtual void ValidatePreExecute()
        {
        }

        public virtual void ValidatePostExecute()
        {
            Assert.Equal(ExpectedClockCycles, _cpu.clock - _startingClock);
            Assert.Equal(_testPc, _cpu.PC);
        }

        protected readonly byte _instruction;

        protected readonly ushort _testWord = 0x6523;

        protected readonly byte _testByte = 0x73;

        public virtual int ExpectedClockCycles => 4;

        protected readonly int _startingClock;

        protected ushort _testPc;

        protected Cpu _cpu;
    }
}