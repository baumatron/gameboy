using System;
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
            _cpu.memory.Write(_testPc++, _instruction);
        }

        public virtual void PrepareTest()
        {
            if (null != _prepareAction)
            {
                _prepareAction(_cpu);
            }
        }

        public virtual void ExecuteTestSubject()
        {
            _cpu.ExecuteNextInstruction();
        }

        public virtual void ValidatePreExecute()
        {
            if (null != _preExecuteValidationAction)
            {
                _preExecuteValidationAction(_cpu);
            }
        }

        public virtual void ValidatePostExecute()
        {
            Assert.Equal(ExpectedClockCycles, _cpu.clock - _startingClock);
            Assert.Equal(_testPc, _cpu.PC);

            if (null != _postExecuteValidationAction)
            {
                _postExecuteValidationAction(_cpu);
            }
        }

        public InstructionTest WithTestPreparation(Action<Cpu> action)
        {
            _prepareAction = action;
            return this;
        }

        public InstructionTest WithPreValidation(Action<Cpu> action)
        {
            _preExecuteValidationAction = action;
            return this;
        }

        public InstructionTest WithPostValidation(Action<Cpu> action)
        {
            _postExecuteValidationAction = action;
            return this;
        }

        public InstructionTest WithClockCycles(int expectedClockCycles)
        {
            _expectedClockCycles = expectedClockCycles;
            return this;
        }

        public InstructionTest WithMemory(ushort address, byte value)
        {
            _cpu.memory.Write(address, value);
            return this;
        }

        public InstructionTest WithImmediateByte(byte value)
        {
            _cpu.memory.Write(_testPc++, value);
            return this;
        }

        public InstructionTest WithImmediateWord(ushort value)
        {
            _cpu.memory.Write(_testPc++, (byte)(value & 0xFF));
            _cpu.memory.Write(_testPc++, (byte)(value >> 8));
            return this;
        }
        protected Action<Cpu> _prepareAction;
        protected Action<Cpu> _preExecuteValidationAction;
        protected Action<Cpu> _postExecuteValidationAction;

        protected readonly byte _instruction;

        protected readonly ushort _testWord = 0x6523;

        protected readonly byte _testByte = 0x73;

        protected int _expectedClockCycles = 4;
        public virtual int ExpectedClockCycles => _expectedClockCycles;

        protected readonly int _startingClock;

        protected ushort _testPc;

        protected Cpu _cpu;
    }
}