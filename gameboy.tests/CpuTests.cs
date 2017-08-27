using Xunit;
using System.Collections.Generic;
using System;
using GameBoy;

namespace GameBoyTests
{
    public class CpuTests
    {
        System.Random random = new System.Random(123456);

        [Fact]
        public void TestGettingCombinedRegisters()
        {
            var cpu = new GameBoy.Cpu();

            cpu.A = 0x0A;
            cpu.F = 0x0F;
            Assert.Equal(0x0A0F, cpu.AF);

            cpu.D = 0x0D;
            cpu.E = 0x0E;
            Assert.Equal(0x0D0E, cpu.DE);

            cpu.B = 0x0B;
            cpu.C = 0x0C;
            Assert.Equal(0x0B0C, cpu.BC);

            cpu.H = 0x1A;
            cpu.L = 0xB1;
            Assert.Equal(0x1AB1, cpu.HL);
        }

        [Fact]
        public void TestSettingCombinedRegisters()
        {
            var cpu = new GameBoy.Cpu();

            cpu.AF = 0x0A0F;
            Assert.Equal(0x0A, cpu.A);
            Assert.Equal(0x0F, cpu.F);

            cpu.BC = 0x0B0C;
            Assert.Equal(0x0B, cpu.B);
            Assert.Equal(0x0C, cpu.C);

            cpu.DE = 0x0D0E;
            Assert.Equal(0x0D, cpu.D);
            Assert.Equal(0x0E, cpu.E);

            cpu.HL = 0x1AB1;
            Assert.Equal(0x1A, cpu.H);
            Assert.Equal(0xB1, cpu.L);
        }

        [Fact]
        public void TestGettingFlags()
        {
            var cpu = new GameBoy.Cpu();

            cpu.F = (byte)(1 << 7);
            Assert.True(cpu.flagZ);
            Assert.False(cpu.flagN);
            Assert.False(cpu.flagH);
            Assert.False(cpu.flagC);

            cpu.F = 0;
            Assert.False(cpu.flagZ);
            Assert.False(cpu.flagN);
            Assert.False(cpu.flagH);
            Assert.False(cpu.flagC);

            cpu.F = (byte)(1 << 6);
            Assert.False(cpu.flagZ);
            Assert.True(cpu.flagN);
            Assert.False(cpu.flagH);
            Assert.False(cpu.flagC);

            cpu.F = 0;
            Assert.False(cpu.flagZ);
            Assert.False(cpu.flagN);
            Assert.False(cpu.flagH);
            Assert.False(cpu.flagC);

            cpu.F = (byte)(1 << 5);
            Assert.False(cpu.flagZ);
            Assert.False(cpu.flagN);
            Assert.True(cpu.flagH);
            Assert.False(cpu.flagC);

            cpu.F = 0;
            Assert.False(cpu.flagZ);
            Assert.False(cpu.flagN);
            Assert.False(cpu.flagH);
            Assert.False(cpu.flagC);

            cpu.F = (byte)(1 << 4);
            Assert.False(cpu.flagZ);
            Assert.False(cpu.flagN);
            Assert.False(cpu.flagH);
            Assert.True(cpu.flagC);

            cpu.F = 0;
            Assert.False(cpu.flagZ);
            Assert.False(cpu.flagN);
            Assert.False(cpu.flagH);
            Assert.False(cpu.flagC);
        }

        [Fact]
        public void TestSettingFlags()
        {
            var cpu = new GameBoy.Cpu();

            cpu.flagZ = true;
            Assert.True(0 != (cpu.F & (1 << 7)));

            cpu.flagZ = false;
            Assert.True(0 == (cpu.F & (1 << 7)));

            cpu.flagN = true;
            Assert.True(0 != (cpu.F & (1 << 6)));

            cpu.flagN = false;
            Assert.True(0 == (cpu.F & (1 << 6)));

            cpu.flagH = true;
            Assert.True(0 != (cpu.F & (1 << 5)));

            cpu.flagH = false;
            Assert.True(0 == (cpu.F & (1 << 5)));

            cpu.flagC = true;
            Assert.True(0 != (cpu.F & (1 << 4)));

            cpu.flagC = false;
            Assert.True(0 == (cpu.F & (1 << 4)));
        }

        [Fact]
        public void TestCpuInit()
        {
            var cpu = new GameBoy.Cpu();
            cpu.Init();

            // Ensure the program counter is in the right place
            Assert.Equal(0x100, cpu.PC);
            // Check stack pointer
            Assert.Equal(0xFFFE, cpu.SP);
        }

        [Fact]
        public void TestStackPointerIncrement()
        {
            // Ensure the stack pointer increments when executing certain instructions
        }

        internal void TestLDFromRegister(GameBoy.Cpu cpu, ref byte from, ref byte to)
        {
            from = (byte)random.Next(0xFF);

            int startingClock = cpu.clock;

            cpu.ExecuteNextInstruction();

            Assert.Equal(from, to);
            Assert.Equal(4, cpu.clock - startingClock);
        }

        internal void TestLDFromHLAddress(GameBoy.Cpu cpu, ref byte to)
        {
            ushort hlAddress = 0x0001;
            // Make sure we don't mess with the instruction we're about to execute
            Assert.NotEqual(hlAddress, cpu.PC);
            Assert.NotEqual(hlAddress, cpu.PC + 1);
            // Save register and memory state
            ushort oldHL = cpu.HL;
            byte oldMemory = cpu.memory.Read(hlAddress);

            cpu.HL = hlAddress;
            byte expected = (byte)random.Next(0xFF);
            cpu.memory.Write(cpu.HL, expected);
            cpu.ExecuteNextInstruction();
            Assert.Equal(expected, to);

            // Restore previous register and memory state
            cpu.memory.Write(hlAddress, oldMemory);
            cpu.HL = oldHL;
        }

        internal void TestLDFromAddress(GameBoy.Cpu cpu, byte expected, ref byte to)
        {
            // Execute the instruction that will do the load
            cpu.ExecuteNextInstruction();
            // Ensure the value was loaded
            Assert.Equal(expected, to);
        }

        internal void TestLDToHLAddress(GameBoy.Cpu cpu, ref byte from)
        {
            ushort hlAddress = 0x0001;
            Assert.NotEqual(hlAddress, cpu.PC);
            Assert.NotEqual(hlAddress, cpu.PC + 1);
            // Save register and memory state
            ushort oldHL = cpu.HL;
            byte oldMemory = cpu.memory.Read(hlAddress);

            cpu.HL = hlAddress;
            cpu.ExecuteNextInstruction();
            Assert.Equal(from, cpu.memory.Read(cpu.HL));

            // Restore previous register and memory state
            cpu.memory.Write(hlAddress, oldMemory);
            cpu.HL = oldHL;
        }

        internal void TestLDToAddress(GameBoy.Cpu cpu, ushort address, byte expected)
        {
            cpu.ExecuteNextInstruction();
            Assert.Equal(expected, cpu.memory.Read(address));
        }

        [Fact]
        public void TestLD()
        {
            var cpu = new GameBoy.Cpu();
            cpu.Init();

            var testTasks = new List<Action>();

            // Set up some test data for address loads
            ushort pc = 0x100;
            ushort testLoadAddress = 0x0001;
            byte testLoadData = 0x1A;
            cpu.memory.Write(testLoadAddress, testLoadData);

            // LD A, reg
            byte baseOpCode = 0x78;
            cpu.memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            // LD (BC), A
            cpu.memory.Write(pc++, 0x02);
            testTasks.Add(() =>
            {
                cpu.memory.Write(testLoadAddress, 0xFF); // Clear old data
                cpu.A = testLoadData;
                cpu.BC = testLoadAddress;
                TestLDToAddress(cpu, testLoadAddress, cpu.A);
            });

            // LD (DE), A
            cpu.memory.Write(pc++, 0x12);
            testTasks.Add(() =>
            {
                cpu.memory.Write(testLoadAddress, 0xFF); // Clear old data
                cpu.A = testLoadData;
                cpu.DE = testLoadAddress;
                TestLDToAddress(cpu, testLoadAddress, cpu.A);
            });

            // LD (HL), A
            cpu.memory.Write(pc++, 0x77);
            testTasks.Add(() =>
            {
                cpu.memory.Write(testLoadAddress, 0xFF); // Clear old data
                cpu.A = testLoadData;
                cpu.HL = testLoadAddress;
                TestLDToAddress(cpu, testLoadAddress, cpu.A);
            });

            // LD (nn), A
            cpu.memory.Write(pc++, 0xEA);
            cpu.memory.Write(pc++, (byte)(testLoadAddress & 0xFF));
            cpu.memory.Write(pc++, (byte)(testLoadAddress >> 8));
            testTasks.Add(() =>
            {
                cpu.memory.Write(testLoadAddress, 0xFF); // Clear old data
                cpu.A = testLoadData;
                TestLDToAddress(cpu, testLoadAddress, cpu.A);
            });

            // LD A, (nn)
            cpu.memory.Write(pc++, 0xFA);
            cpu.memory.Write(pc++, (byte)(testLoadAddress & 0xFF));
            cpu.memory.Write(pc++, (byte)(testLoadAddress >> 8));
            testTasks.Add(() =>
            {
                TestLDFromAddress(cpu, testLoadData, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]);
            });

            // LD B, reg
            baseOpCode = 0x40;
            cpu.memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            // LD C, reg
            baseOpCode = 0x48;
            cpu.memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            // LD D, reg
            baseOpCode = 0x50;
            cpu.memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            // LD E, reg
            baseOpCode = 0x58;
            cpu.memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            // LD H, reg
            baseOpCode = 0x60;
            cpu.memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            // LD L, reg
            baseOpCode = 0x68;
            cpu.memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            // LD (HL), reg
            baseOpCode = 0x70;
            cpu.memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu.memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            foreach (var task in testTasks)
            {
                task();
            }
        }

        [Fact]
        public void TestLDImmediate()
        {
            var cpu = new GameBoy.Cpu();
            cpu.Init();

            // Set up some test data for address loads
            ushort pc = 0x100;
            const ushort testLoadAddress = 0x0001;
            const byte immediateValue = 0x1A;

            for (int registerEncoding = 0; registerEncoding <= 7; ++registerEncoding)
            {
                int startingClock = cpu.clock;

                byte instruction = (byte)(registerEncoding << 3 | 0b110);
                cpu.memory.Write(pc++, instruction);
                cpu.memory.Write(pc++, immediateValue);

                Cpu.RegisterEncoding encoding = (Cpu.RegisterEncoding)registerEncoding;

                // Prep the HL register with an address to load into
                if (encoding == Cpu.RegisterEncoding.HLderef)
                {
                    // LD (HL), n
                    cpu.HL = testLoadAddress;

                    Assert.NotEqual(immediateValue, cpu.memory.Read(testLoadAddress));
                }
                else
                {
                    Assert.NotEqual(immediateValue, cpu.registers[Cpu.RegisterEncodingToIndex(encoding)]);
                }

                cpu.ExecuteNextInstruction();

                // Validate cpu state
                if (encoding == Cpu.RegisterEncoding.HLderef)
                {
                    // LD (HL), n
                    Assert.Equal(immediateValue, cpu.memory.Read(testLoadAddress));
                    Assert.Equal(12, cpu.clock - startingClock);
                }
                else
                {
                    // LD register, n
                    Assert.Equal(immediateValue, cpu.registers[Cpu.RegisterEncodingToIndex(encoding)]);
                    Assert.Equal(8, cpu.clock - startingClock);
                }

                Assert.Equal(pc, cpu.PC);
            }
        }

        [Fact]
        public void TestLDFromBCtoA()
        {
            var cpu = new GameBoy.Cpu();
            cpu.Init();

            int startingClock = cpu.clock;

            // Set up some test data for address loads
            ushort pc = 0x100;
            const ushort testLoadAddress = 0x0001;
            const byte testValue = 0x1A;

            // Prep memory with LD A, (BC) instruction
            cpu.memory.Write(pc++, 0x0A);

            // Prep memory with value
            cpu.memory.Write(testLoadAddress, testValue);

            // Prep BC register
            cpu.BC = testLoadAddress;

            // Execute the instruction
            cpu.ExecuteNextInstruction();

            // Value from memory should be stored in A
            Assert.Equal(testValue, cpu.A);

            Assert.Equal(8, cpu.clock - startingClock);

            Assert.Equal(pc, cpu.PC);
        }

        [Fact]
        public void TestLDFromDEtoA()
        {
            var cpu = new GameBoy.Cpu();
            cpu.Init();

            int startingClock = cpu.clock;

            // Set up some test data for address loads
            ushort pc = 0x100;
            const ushort testLoadAddress = 0x0001;
            const byte testValue = 0x1A;

            // Prep memory with LD A, (DE) instruction
            cpu.memory.Write(pc++, 0x1A);

            // Prep memory with value
            cpu.memory.Write(testLoadAddress, testValue);

            // Prep DE register
            cpu.DE = testLoadAddress;

            Assert.NotEqual(testValue, cpu.A);
            byte oldF = cpu.F;

            // Execute the instruction
            cpu.ExecuteNextInstruction();

            // Value from memory should be stored in A
            Assert.Equal(testValue, cpu.A);

            Assert.Equal(oldF, cpu.F);

            Assert.Equal(8, cpu.clock - startingClock);

            Assert.Equal(pc, cpu.PC);
        }

        [Fact]
        public void TestLDFromCOffsetToA()
        {
            var cpu = new Cpu();
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const byte addressOffset = 0x12;
            const ushort testLoadAddress = 0xFF00 + addressOffset;
            const byte testValue = 0x65;

            cpu.memory.Write(pc++, 0xF2);
            cpu.memory.Write(testLoadAddress, testValue);
            cpu.C = addressOffset;

            Assert.NotEqual(testValue, cpu.A);

            cpu.ExecuteNextInstruction();

            Assert.Equal(testValue, cpu.A);
            Assert.Equal(pc, cpu.PC);
            Assert.Equal(8, cpu.clock - startingClock);
        }

        [Fact]
        public void TestLDToCOffsetFromA()
        {
            var cpu = new Cpu();
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const byte addressOffset = 0x12;
            const ushort testLoadAddress = 0xFF00 + addressOffset;
            const byte testValue = 0x65;

            cpu.memory.Write(pc++, 0xE2);
            cpu.C = addressOffset;
            cpu.A = testValue;

            Assert.NotEqual(testValue, cpu.memory.Read(testLoadAddress));

            cpu.ExecuteNextInstruction();

            Assert.Equal(testValue, cpu.memory.Read(testLoadAddress));
            Assert.Equal(pc, cpu.PC);
            Assert.Equal(8, cpu.clock - startingClock);
        }

        internal void DoLDFromHLAddressToATest(byte instruction, int hlOffsetAfterLoad)
        {
            var cpu = new Cpu();
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const ushort testLoadAddress = 0xF000;
            const byte testValue = 0x65;

            cpu.memory.Write(pc++, instruction);
            cpu.HL = testLoadAddress;
            cpu.memory.Write(testLoadAddress, testValue);

            Assert.NotEqual(testValue, cpu.A);

            cpu.ExecuteNextInstruction();

            Assert.Equal(testValue, cpu.A);
            Assert.Equal(pc, cpu.PC);
            Assert.Equal(testLoadAddress + hlOffsetAfterLoad, cpu.HL);
            Assert.Equal(8, cpu.clock - startingClock);
        }

        [Fact]
        public void TestLDFromHLAddressToAAndDecrement()
        {
            DoLDFromHLAddressToATest(0x3A, -1);
        }

        [Fact]
        public void TestLDFromHLAddressToAAndIncrement()
        {
            DoLDFromHLAddressToATest(0x2A, 1);
        }

        internal void DoLDFromAToHLAddressTest(byte instruction, int hlOffsetAfterLoad)
        {
            var cpu = new Cpu();
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const ushort testLoadAddress = 0xF000;
            const byte testValue = 0x65;

            cpu.memory.Write(pc++, instruction);
            cpu.HL = testLoadAddress;
            cpu.A = testValue;

            Assert.NotEqual(testValue, cpu.memory.Read(testLoadAddress));

            cpu.ExecuteNextInstruction();

            Assert.Equal(testValue, cpu.memory.Read(testLoadAddress));
            Assert.Equal(pc, cpu.PC);
            Assert.Equal(testLoadAddress + hlOffsetAfterLoad, cpu.HL);
            Assert.Equal(8, cpu.clock - startingClock);
        }

        [Fact]
        public void TestLDFromAToHLAddressAndDecrement()
        {
            DoLDFromAToHLAddressTest(0x32, -1);
        }

        [Fact]
        public void TestLDFromAToHLAddressAndIncrement()
        {
            DoLDFromAToHLAddressTest(0x22, 1);
        }

        [Fact]
        public void TestLoadFromAToImmediateAddressByte()
        {
            var cpu = new Cpu();
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const byte offset = 0x35;
            const ushort testLoadAddress = 0xFF00 + offset;
            const byte testValue = 0x65;

            cpu.memory.Write(pc++, 0xE0);
            cpu.memory.Write(pc++, offset);
            cpu.A = testValue;

            Assert.NotEqual(testValue, cpu.memory.Read(testLoadAddress));

            cpu.ExecuteNextInstruction();

            Assert.Equal(testValue, cpu.memory.Read(testLoadAddress));
            Assert.Equal(pc, cpu.PC);
            Assert.Equal(12, cpu.clock - startingClock);
        }

        [Fact]
        public void TestLoadFromImmediateAddressByteToA()
        {
            var cpu = new Cpu();
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const byte offset = 0x35;
            const ushort testLoadAddress = 0xFF00 + offset;
            const byte testValue = 0x65;

            cpu.memory.Write(pc++, 0xF0);
            cpu.memory.Write(pc++, offset);
            cpu.memory.Write(testLoadAddress, testValue);

            Assert.NotEqual(testValue, cpu.A);

            cpu.ExecuteNextInstruction();

            Assert.Equal(testValue, cpu.A);
            Assert.Equal(pc, cpu.PC);
            Assert.Equal(12, cpu.clock - startingClock);
        }

        class LoadImmediateWordToRegistersTest : InstructionTest
        {
            public LoadImmediateWordToRegistersTest(byte instruction, Func<Cpu, ushort> GetRegisterValue)
                : base(instruction)
            {
                _GetRegisterValue = GetRegisterValue;
            }

            public override void PrepareTest()
            {
                base.PrepareTest();

                _cpu.memory.Write(_testPc++, (byte)(_testWord & 0xFF));
                _cpu.memory.Write(_testPc++, (byte)(_testWord >> 8));
            }

            public override void ValidatePreExecute()
            {
                base.ValidatePreExecute();

                Assert.NotEqual(_testWord, _GetRegisterValue(_cpu));
            }

            public override void ValidatePostExecute()
            {
                base.ValidatePostExecute();

                Assert.Equal(_testWord, _GetRegisterValue(_cpu));
            }

            readonly Func<Cpu, ushort> _GetRegisterValue;

            public override int ExpectedClockCycles => 12;
        };

        [Fact]
        public void TestLoadImmediateWordToBC()
        {
            var runner = new InstructionTestRunner(new LoadImmediateWordToRegistersTest(0x01, x => x.BC));
            runner.Run();
        }

        [Fact]
        public void TestLoadImmediateWordToDE()
        {
            var runner = new InstructionTestRunner(new LoadImmediateWordToRegistersTest(0x11, x => x.DE));
            runner.Run();
        }

        [Fact]
        public void TestLoadImmediateWordToHL()
        {
            var runner = new InstructionTestRunner(new LoadImmediateWordToRegistersTest(0x21, x => x.HL));
            runner.Run();
        }

        [Fact]
        public void TestLoadImmediateWordToSP()
        {
            var runner = new InstructionTestRunner(new LoadImmediateWordToRegistersTest(0x31, x => x.SP));
            runner.Run();
        }

        class LoadFromHLToSPTest : InstructionTest
        {
            public LoadFromHLToSPTest()
                : base(0xF9)
                {}

            public override void PrepareTest()
            {
                base.PrepareTest();
                _cpu.HL = _testWord;
            }

            public override void ValidatePreExecute()
            {
                base.ValidatePreExecute();
                Assert.NotEqual(_cpu.HL, _cpu.SP);
            }

            public override void ValidatePostExecute()
            {
                base.ValidatePostExecute();
                Assert.Equal(_cpu.HL, _cpu.SP);
            }

            public override int ExpectedClockCycles => 8;
        };

        [Fact]
        public void TestLoadFromHLToSP()
        {
            var runner = new InstructionTestRunner(new LoadFromHLToSPTest());
            runner.Run();
        }
    }
}
