using GameBoy;
using Xunit;

namespace GameBoyTests
{
    class InstructionTest
    {
        public InstructionTest()
        {
            _cpu = new Cpu();
            _cpu.Init();

            _startingClock = _cpu.clock;

            _testPc = 0x100;
        }

        public virtual void PrepareTest()
        {

        }

        public virtual void ExecuteTestSubject()
        {
            _cpu.ExecuteNextInstruction();
        }

        public virtual void ValidatePreExecute()
        {
            Assert.Equal(ExpectedClockCycles, _cpu.clock - _startingClock);
        }

        public virtual void ValidatePostExecute()
        {
            Assert.Equal(ExpectedClockCycles, _cpu.clock - _startingClock);
            Assert.Equal(_testPc, _cpu.PC);
        }

        public virtual int ExpectedClockCycles => 4;

        protected int _startingClock;

        protected ushort _testPc;

        protected Cpu _cpu;
    }
}