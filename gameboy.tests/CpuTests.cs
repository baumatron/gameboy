﻿using Xunit;
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
            var cpu = new GameBoy.Cpu(new Ram());

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
            var cpu = new GameBoy.Cpu(new Ram());

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
            var cpu = new GameBoy.Cpu(new Ram());

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
            var cpu = new GameBoy.Cpu(new Ram());

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
            var cpu = new GameBoy.Cpu(new Ram());
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
            byte oldMemory = cpu._memory.Read(hlAddress);

            cpu.HL = hlAddress;
            byte expected = (byte)random.Next(0xFF);
            cpu._memory.Write(cpu.HL, expected);
            cpu.ExecuteNextInstruction();
            Assert.Equal(expected, to);

            // Restore previous register and memory state
            cpu._memory.Write(hlAddress, oldMemory);
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
            byte oldMemory = cpu._memory.Read(hlAddress);

            cpu.HL = hlAddress;
            cpu.ExecuteNextInstruction();
            Assert.Equal(from, cpu._memory.Read(cpu.HL));

            // Restore previous register and memory state
            cpu._memory.Write(hlAddress, oldMemory);
            cpu.HL = oldHL;
        }

        internal void TestLDToAddress(GameBoy.Cpu cpu, ushort address, byte expected)
        {
            cpu.ExecuteNextInstruction();
            Assert.Equal(expected, cpu._memory.Read(address));
        }

        [Fact]
        public void TestLD()
        {
            var cpu = new GameBoy.Cpu(new Ram());
            cpu.Init();

            var testTasks = new List<Action>();

            // Set up some test data for address loads
            ushort pc = 0x100;
            ushort testLoadAddress = 0x0001;
            byte testLoadData = 0x1A;
            cpu._memory.Write(testLoadAddress, testLoadData);

            // LD A, reg
            byte baseOpCode = 0x78;
            cpu._memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            // LD (BC), A
            cpu._memory.Write(pc++, 0x02);
            testTasks.Add(() =>
            {
                cpu._memory.Write(testLoadAddress, 0xFF); // Clear old data
                cpu.A = testLoadData;
                cpu.BC = testLoadAddress;
                TestLDToAddress(cpu, testLoadAddress, cpu.A);
            });

            // LD (DE), A
            cpu._memory.Write(pc++, 0x12);
            testTasks.Add(() =>
            {
                cpu._memory.Write(testLoadAddress, 0xFF); // Clear old data
                cpu.A = testLoadData;
                cpu.DE = testLoadAddress;
                TestLDToAddress(cpu, testLoadAddress, cpu.A);
            });

            // LD (HL), A
            cpu._memory.Write(pc++, 0x77);
            testTasks.Add(() =>
            {
                cpu._memory.Write(testLoadAddress, 0xFF); // Clear old data
                cpu.A = testLoadData;
                cpu.HL = testLoadAddress;
                TestLDToAddress(cpu, testLoadAddress, cpu.A);
            });

            // LD (nn), A
            cpu._memory.Write(pc++, 0xEA);
            cpu._memory.Write(pc++, (byte)(testLoadAddress & 0xFF));
            cpu._memory.Write(pc++, (byte)(testLoadAddress >> 8));
            testTasks.Add(() =>
            {
                cpu._memory.Write(testLoadAddress, 0xFF); // Clear old data
                cpu.A = testLoadData;
                TestLDToAddress(cpu, testLoadAddress, cpu.A);
            });

            // LD A, (nn)
            cpu._memory.Write(pc++, 0xFA);
            cpu._memory.Write(pc++, (byte)(testLoadAddress & 0xFF));
            cpu._memory.Write(pc++, (byte)(testLoadAddress >> 8));
            testTasks.Add(() =>
            {
                TestLDFromAddress(cpu, testLoadData, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]);
            });

            // LD B, reg
            baseOpCode = 0x40;
            cpu._memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            // LD C, reg
            baseOpCode = 0x48;
            cpu._memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            // LD D, reg
            baseOpCode = 0x50;
            cpu._memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            // LD E, reg
            baseOpCode = 0x58;
            cpu._memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            // LD H, reg
            baseOpCode = 0x60;
            cpu._memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            // LD L, reg
            baseOpCode = 0x68;
            cpu._memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x06));
            testTasks.Add(() => TestLDFromHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDFromRegister(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)], ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            // LD (HL), reg
            baseOpCode = 0x70;
            cpu._memory.Write(pc++, baseOpCode);
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.B)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x01));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.C)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x02));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.D)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x03));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.E)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x04));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.H)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x05));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.L)]));

            cpu._memory.Write(pc++, (byte)(baseOpCode + 0x07));
            testTasks.Add(() => TestLDToHLAddress(cpu, ref cpu.registers[Cpu.RegisterEncodingToIndex(Cpu.RegisterEncoding.A)]));

            foreach (var task in testTasks)
            {
                task();
            }
        }

        [Fact]
        public void TestLDImmediate()
        {
            var cpu = new GameBoy.Cpu(new Ram());
            cpu.Init();

            // Set up some test data for address loads
            ushort pc = 0x100;
            const ushort testLoadAddress = 0x0001;
            const byte immediateValue = 0x1A;

            for (int registerEncoding = 0; registerEncoding <= 7; ++registerEncoding)
            {
                int startingClock = cpu.clock;

                byte instruction = (byte)(registerEncoding << 3 | 0b110);
                cpu._memory.Write(pc++, instruction);
                cpu._memory.Write(pc++, immediateValue);

                Cpu.RegisterEncoding encoding = (Cpu.RegisterEncoding)registerEncoding;

                // Prep the HL register with an address to load into
                if (encoding == Cpu.RegisterEncoding.HLDeref)
                {
                    // LD (HL), n
                    cpu.HL = testLoadAddress;

                    Assert.NotEqual(immediateValue, cpu._memory.Read(testLoadAddress));
                }
                else
                {
                    Assert.NotEqual(immediateValue, cpu.registers[Cpu.RegisterEncodingToIndex(encoding)]);
                }

                cpu.ExecuteNextInstruction();

                // Validate cpu state
                if (encoding == Cpu.RegisterEncoding.HLDeref)
                {
                    // LD (HL), n
                    Assert.Equal(immediateValue, cpu._memory.Read(testLoadAddress));
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
            var cpu = new GameBoy.Cpu(new Ram());
            cpu.Init();

            int startingClock = cpu.clock;

            // Set up some test data for address loads
            ushort pc = 0x100;
            const ushort testLoadAddress = 0x0001;
            const byte testValue = 0x1A;

            // Prep memory with LD A, (BC) instruction
            cpu._memory.Write(pc++, 0x0A);

            // Prep memory with value
            cpu._memory.Write(testLoadAddress, testValue);

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
            var cpu = new GameBoy.Cpu(new Ram());
            cpu.Init();

            int startingClock = cpu.clock;

            // Set up some test data for address loads
            ushort pc = 0x100;
            const ushort testLoadAddress = 0x0001;
            const byte testValue = 0x1A;

            // Prep memory with LD A, (DE) instruction
            cpu._memory.Write(pc++, 0x1A);

            // Prep memory with value
            cpu._memory.Write(testLoadAddress, testValue);

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
            var cpu = new Cpu(new Ram());
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const byte addressOffset = 0x12;
            const ushort testLoadAddress = 0xFF00 + addressOffset;
            const byte testValue = 0x65;

            cpu._memory.Write(pc++, 0xF2);
            cpu._memory.Write(testLoadAddress, testValue);
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
            var cpu = new Cpu(new Ram());
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const byte addressOffset = 0x12;
            const ushort testLoadAddress = 0xFF00 + addressOffset;
            const byte testValue = 0x65;

            cpu._memory.Write(pc++, 0xE2);
            cpu.C = addressOffset;
            cpu.A = testValue;

            Assert.NotEqual(testValue, cpu._memory.Read(testLoadAddress));

            cpu.ExecuteNextInstruction();

            Assert.Equal(testValue, cpu._memory.Read(testLoadAddress));
            Assert.Equal(pc, cpu.PC);
            Assert.Equal(8, cpu.clock - startingClock);
        }

        internal void DoLDFromHLAddressToATest(byte instruction, int hlOffsetAfterLoad)
        {
            var cpu = new Cpu(new Ram());
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const ushort testLoadAddress = 0xF000;
            const byte testValue = 0x65;

            cpu._memory.Write(pc++, instruction);
            cpu.HL = testLoadAddress;
            cpu._memory.Write(testLoadAddress, testValue);

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
            var cpu = new Cpu(new Ram());
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const ushort testLoadAddress = 0xF000;
            const byte testValue = 0x65;

            cpu._memory.Write(pc++, instruction);
            cpu.HL = testLoadAddress;
            cpu.A = testValue;

            Assert.NotEqual(testValue, cpu._memory.Read(testLoadAddress));

            cpu.ExecuteNextInstruction();

            Assert.Equal(testValue, cpu._memory.Read(testLoadAddress));
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
            var cpu = new Cpu(new Ram());
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const byte offset = 0x35;
            const ushort testLoadAddress = 0xFF00 + offset;
            const byte testValue = 0x65;

            cpu._memory.Write(pc++, 0xE0);
            cpu._memory.Write(pc++, offset);
            cpu.A = testValue;

            Assert.NotEqual(testValue, cpu._memory.Read(testLoadAddress));

            cpu.ExecuteNextInstruction();

            Assert.Equal(testValue, cpu._memory.Read(testLoadAddress));
            Assert.Equal(pc, cpu.PC);
            Assert.Equal(12, cpu.clock - startingClock);
        }

        [Fact]
        public void TestLoadFromImmediateAddressByteToA()
        {
            var cpu = new Cpu(new Ram());
            cpu.Init();

            int startingClock = cpu.clock;

            ushort pc = 0x100;
            const byte offset = 0x35;
            const ushort testLoadAddress = 0xFF00 + offset;
            const byte testValue = 0x65;

            cpu._memory.Write(pc++, 0xF0);
            cpu._memory.Write(pc++, offset);
            cpu._memory.Write(testLoadAddress, testValue);

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
                _expectedClockCycles = 12;
            }

            public override void PrepareTest()
            {
                base.PrepareTest();

                _cpu._memory.Write(_testPc++, (byte)(_testWord & 0xFF));
                _cpu._memory.Write(_testPc++, (byte)(_testWord >> 8));
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

        [Fact]
        public void TestLoadFromHLToSP()
        {
            ushort testWord = 0x1234;
            var test = new InstructionTest(0xF9)
                .WithTestPreparation(cpu => cpu.HL = testWord)
                .WithPreValidation(cpu => Assert.NotEqual(cpu.HL, cpu.SP))
                .WithPostValidation(cpu => Assert.Equal(cpu.HL, cpu.SP))
                .WithClockCycles(8);
            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestLoadFromSPAndImmediateToHLBothCarries()
        {
            ushort testWord = 0x12FF;
            sbyte testOffset = 1;
            var test = new InstructionTest(0xF8)
                .WithImmediateByte((byte)testOffset)
                .WithTestPreparation(cpu =>
                {
                    cpu.SP = testWord;
                    cpu.flagZ = true;
                    cpu.flagN = true;
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(cpu.HL, cpu.SP + testOffset);
                    Assert.Equal(cpu.flagZ, true);
                    Assert.Equal(cpu.flagN, true);
                    Assert.Equal(cpu.flagH, false);
                    Assert.Equal(cpu.flagC, false);
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal((int)cpu.HL, (int)cpu.SP + (int)testOffset);
                    Assert.Equal(cpu.flagZ, false);
                    Assert.Equal(cpu.flagN, false);
                    Assert.Equal(cpu.flagH, true);
                    Assert.Equal(cpu.flagC, true);
                })
                .WithClockCycles(12);

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestLoadFromSPAndImmediateToHLHalfCarry()
        {
            ushort testWord = 0x12EF;
            sbyte testOffset = 1;
            var test = new InstructionTest(0xF8)
                .WithImmediateByte((byte)testOffset)
                .WithTestPreparation(cpu =>
                {
                    cpu.SP = testWord;
                    cpu.flagZ = true;
                    cpu.flagN = true;
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(cpu.HL, cpu.SP + testOffset);
                    Assert.Equal(cpu.flagZ, true);
                    Assert.Equal(cpu.flagN, true);
                    Assert.Equal(cpu.flagH, false);
                    Assert.Equal(cpu.flagC, false);
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal((int)cpu.HL, (int)cpu.SP + (int)testOffset);
                    Assert.Equal(cpu.flagZ, false);
                    Assert.Equal(cpu.flagN, false);
                    Assert.Equal(cpu.flagH, true);
                    Assert.Equal(cpu.flagC, false);
                })
                .WithClockCycles(12);

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestLoadFromSPAndImmediateToHLFullCarry()
        {
            ushort testWord = 0x12FF;
            sbyte testOffset = 0x10;
            var test = new InstructionTest(0xF8)
                .WithImmediateByte((byte)testOffset)
                .WithTestPreparation(cpu =>
                {
                    cpu.SP = testWord;
                    cpu.flagZ = true;
                    cpu.flagN = true;
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(cpu.HL, cpu.SP + testOffset);
                    Assert.Equal(cpu.flagZ, true);
                    Assert.Equal(cpu.flagN, true);
                    Assert.Equal(cpu.flagH, false);
                    Assert.Equal(cpu.flagC, false);
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal((int)cpu.HL, (int)cpu.SP + (int)testOffset);
                    Assert.Equal(cpu.flagZ, false);
                    Assert.Equal(cpu.flagN, false);
                    Assert.Equal(cpu.flagH, false);
                    Assert.Equal(cpu.flagC, true);
                })
                .WithClockCycles(12);

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestLoadFromSPAndImmediateToHLNoCarry()
        {
            ushort testWord = 0x12EE;
            sbyte testOffset = 0x11;
            var test = new InstructionTest(0xF8)
                .WithImmediateByte((byte)testOffset)
                .WithTestPreparation(cpu =>
                {
                    cpu.SP = testWord;
                    cpu.flagZ = true;
                    cpu.flagN = true;
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(cpu.HL, cpu.SP + testOffset);
                    Assert.Equal(cpu.flagZ, true);
                    Assert.Equal(cpu.flagN, true);
                    Assert.Equal(cpu.flagH, false);
                    Assert.Equal(cpu.flagC, false);
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal(cpu.HL, cpu.SP + testOffset);
                    Assert.Equal(cpu.flagZ, false);
                    Assert.Equal(cpu.flagN, false);
                    Assert.Equal(cpu.flagH, false);
                    Assert.Equal(cpu.flagC, false);
                })
                .WithClockCycles(12);

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestLoadFromSPAndImmediateToHLNegativeImmediate()
        {
            ushort testWord = 0x12EE;
            sbyte testOffset = -1;
            var test = new InstructionTest(0xF8)
                .WithImmediateByte((byte)testOffset)
                .WithTestPreparation(cpu =>
                {
                    cpu.SP = testWord;
                    cpu.flagZ = true;
                    cpu.flagN = true;
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(cpu.HL, cpu.SP + testOffset);
                    Assert.Equal(cpu.flagZ, true);
                    Assert.Equal(cpu.flagN, true);
                    Assert.Equal(cpu.flagH, false);
                    Assert.Equal(cpu.flagC, false);
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal(cpu.HL, cpu.SP + testOffset);
                    Assert.Equal(cpu.flagZ, false);
                    Assert.Equal(cpu.flagN, false);
                    Assert.Equal(cpu.flagH, true); // TODO: Verify absolutely that this is correct
                    Assert.Equal(cpu.flagC, true);
                })
                .WithClockCycles(12);

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestLoadFromSPToImmediateAddress()
        {
            ushort immediateAddress = 0x12EE;
            ushort spValue = 0x4321;
            var test = new InstructionTest(0x08)
                .WithImmediateWord(immediateAddress)
                .WithTestPreparation(cpu =>
                {
                    cpu.SP = spValue;
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(cpu.SP, cpu._memory.ReadWord(immediateAddress));
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal(cpu.SP, cpu._memory.ReadWord(immediateAddress));
                })
                .WithClockCycles(20);

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        public void TestPushRegisterWord(byte instruction, Func<Cpu, ushort> GetRegister, Action<Cpu, ushort> SetRegister)
        {
            ushort sp = 0x1234;
            ushort registerWord = 0x4321;
            var test = new InstructionTest(instruction)
                .WithClockCycles(16)
                .WithTestPreparation(cpu =>
                {
                    cpu.SP = sp;
                    SetRegister(cpu, registerWord);
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(GetRegister(cpu), cpu._memory.ReadWord(sp));
                })
                .WithPostValidation(cpu =>
                {
                    // Need to validate the register against the memory that SP points to,
                    // since after the push the stack points to the word that was just written
                    Assert.Equal(GetRegister(cpu), cpu._memory.ReadWord(cpu.SP));
                    Assert.Equal(sp - 2, cpu.SP);
                });

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestPushAF()
        {
            TestPushRegisterWord(0xF5, cpu => cpu.AF, (cpu, value) => cpu.AF = value);
        }

        [Fact]
        public void TestPushBC()
        {
            TestPushRegisterWord(0xC5, cpu => cpu.BC, (cpu, value) => cpu.BC = value);
        }

        [Fact]
        public void TestPushDE()
        {
            TestPushRegisterWord(0xD5, cpu => cpu.DE, (cpu, value) => cpu.DE = value);
        }

        [Fact]
        public void TestPushHL()
        {
            TestPushRegisterWord(0xE5, cpu => cpu.HL, (cpu, value) => cpu.HL = value);
        }

        public void TestPopRegisterWord(byte instruction, Func<Cpu, ushort> GetRegister)
        {
            ushort sp = 0x1234;
            ushort registerWord = 0x4321;
            var test = new InstructionTest(instruction)
                .WithClockCycles(12)
                .WithMemoryWord(sp, registerWord)
                .WithTestPreparation(cpu =>
                {
                    cpu.SP = sp;
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(GetRegister(cpu), cpu._memory.ReadWord(sp));
                })
                .WithPostValidation(cpu =>
                {
                    // Need to validate against the memory that was at SP before the 
                    // instruction ran (or current SP + 2), since SP was updated to discard
                    // that value off the stack
                    Assert.Equal(GetRegister(cpu), cpu._memory.ReadWord(sp));
                    Assert.Equal(sp + 2, cpu.SP);
                });

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestPopAF()
        {
            TestPopRegisterWord(0xF1, cpu => cpu.AF);
        }

        [Fact]
        public void TestPopBC()
        {
            TestPopRegisterWord(0xC1, cpu => cpu.BC);
        }

        [Fact]
        public void TestPopDE()
        {
            TestPopRegisterWord(0xD1, cpu => cpu.DE);
        }

        [Fact]
        public void TestPopHL()
        {
            TestPopRegisterWord(0xE1, cpu => cpu.HL);
        }

        public void TestAdd(byte instruction, byte lhs, byte rhs, Action<Cpu, byte> SetRegister, bool setFlagC, bool halfCarry, bool fullCarry, int cycles = 4, bool rhsIsImmediate = false)
        {
            byte result = (byte)(rhs + lhs + ((setFlagC && ((instruction & 0x80) > 0)) ? 1 : 0));
            var test = new InstructionTest(instruction)
                .WithClockCycles(cycles)
                .WithTestPreparation(cpu =>
                {
                    cpu.A = lhs;
                    cpu.F = 0;
                    cpu.flagN = true;
                    cpu.flagC = setFlagC;
                    if (!rhsIsImmediate)
                    {
                        SetRegister(cpu, rhs);
                    }
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(result, cpu.A);
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal(result, cpu.A);
                    Assert.Equal(cpu.A == 0, cpu.flagZ);
                    Assert.Equal(false, cpu.flagN);
                    Assert.Equal(halfCarry, cpu.flagH);
                    Assert.Equal(fullCarry, cpu.flagC);
                });

            if (rhsIsImmediate)
            {
                test.WithImmediateByte(rhs);
            }

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestAddAllRegisters()
        {
            // Test rhs of B-D. Skip A and (HL)
            for (byte i = 0; i < 6; ++i)
            {
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)(i & 0b111);
                byte instruction = (byte)(0x80 | i);
                Action<Cpu, byte> SetRegister = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                TestAdd(instruction, 0x40, 0x05, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false);
                TestAdd(instruction, 0x4F, 0x01, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false);
                TestAdd(instruction, 0xF0, 0x10, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true);
                TestAdd(instruction, 0xFF, 0x01, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true);
            }
        }

        [Fact]
        public void TestAdcAllRegisters()
        {
            // Test rhs of B-D. Skip A and (HL)
            for (byte i = 0; i < 6; ++i)
            {
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)(i & 0b111);
                byte instruction = (byte)(0x88 | i);
                Action<Cpu, byte> SetRegister = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                TestAdd(instruction, 0x40, 0x05, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false);
                TestAdd(instruction, 0x4F, 0x01, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false);
                TestAdd(instruction, 0xF0, 0x10, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true);
                TestAdd(instruction, 0xFF, 0x01, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true);

                TestAdd(instruction, 0xF0, 0x0E, SetRegister, setFlagC: true, halfCarry: false, fullCarry: false);
                TestAdd(instruction, 0x4F, 0x00, SetRegister, setFlagC: true, halfCarry: true, fullCarry: false);
                TestAdd(instruction, 0xF0, 0x10, SetRegister, setFlagC: true, halfCarry: false, fullCarry: true);
                TestAdd(instruction, 0xFF, 0x00, SetRegister, setFlagC: true, halfCarry: true, fullCarry: true);
            }
        }

        [Fact]
        public void TestAddHLAddress()
        {
            byte instruction = 0x86;
            Action<Cpu, byte> SetRegister = (cpu, value) =>
            {
                cpu.HL = 0x1234;
                cpu._memory.Write(cpu.HL, value);
            };
            TestAdd(instruction, 0x40, 0x05, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false, cycles: 8);
            TestAdd(instruction, 0x4F, 0x01, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false, cycles: 8);
            TestAdd(instruction, 0xF0, 0x10, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true, cycles: 8);
            TestAdd(instruction, 0xFF, 0x01, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true, cycles: 8);
        }

        [Fact]
        public void TestAdcHLAddress()
        {
            byte instruction = 0x8E;
            Action<Cpu, byte> SetRegister = (cpu, value) =>
            {
                cpu.HL = 0x1234;
                cpu._memory.Write(cpu.HL, value);
            };
            TestAdd(instruction, 0x40, 0x05, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false, cycles: 8);
            TestAdd(instruction, 0x4F, 0x01, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false, cycles: 8);
            TestAdd(instruction, 0xF0, 0x10, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true, cycles: 8);
            TestAdd(instruction, 0xFF, 0x01, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true, cycles: 8);

            TestAdd(instruction, 0xF0, 0x0E, SetRegister, setFlagC: true, halfCarry: false, fullCarry: false, cycles: 8);
            TestAdd(instruction, 0x4F, 0x00, SetRegister, setFlagC: true, halfCarry: true, fullCarry: false, cycles: 8);
            TestAdd(instruction, 0xF0, 0x10, SetRegister, setFlagC: true, halfCarry: false, fullCarry: true, cycles: 8);
            TestAdd(instruction, 0xFF, 0x00, SetRegister, setFlagC: true, halfCarry: true, fullCarry: true, cycles: 8);
        }

        [Fact]
        public void TestAddImmediate()
        {
            byte instruction = 0xC6;
            TestAdd(instruction, 0x40, 0x05, null, setFlagC: false, halfCarry: false, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestAdd(instruction, 0x4F, 0x01, null, setFlagC: false, halfCarry: true, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestAdd(instruction, 0xF0, 0x10, null, setFlagC: false, halfCarry: false, fullCarry: true, cycles: 8, rhsIsImmediate: true);
            TestAdd(instruction, 0xFF, 0x01, null, setFlagC: false, halfCarry: true, fullCarry: true, cycles: 8, rhsIsImmediate: true);
        }

        [Fact]
        public void TestAdcImmediate()
        {
            byte instruction = 0xCE;
            TestAdd(instruction, 0x40, 0x05, null, setFlagC: false, halfCarry: false, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestAdd(instruction, 0x4F, 0x01, null, setFlagC: false, halfCarry: true, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestAdd(instruction, 0xF0, 0x10, null, setFlagC: false, halfCarry: false, fullCarry: true, cycles: 8, rhsIsImmediate: true);
            TestAdd(instruction, 0xFF, 0x01, null, setFlagC: false, halfCarry: true, fullCarry: true, cycles: 8, rhsIsImmediate: true);

            TestAdd(instruction, 0xF0, 0x0E, null, setFlagC: true, halfCarry: false, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestAdd(instruction, 0x4F, 0x00, null, setFlagC: true, halfCarry: true, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestAdd(instruction, 0xF0, 0x10, null, setFlagC: true, halfCarry: false, fullCarry: true, cycles: 8, rhsIsImmediate: true);
            TestAdd(instruction, 0xFF, 0x00, null, setFlagC: true, halfCarry: true, fullCarry: true, cycles: 8, rhsIsImmediate: true);
        }

        [Fact]
        public void TestAddA()
        {
            byte instruction = 0x80 | (byte)Cpu.RegisterEncoding.A;
            Action<Cpu, byte> SetRegister = (cpu, value) => cpu.A = value;
            TestAdd(instruction, 0x40, 0x40, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false);
            TestAdd(instruction, 0x08, 0x08, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false);
            TestAdd(instruction, 0x80, 0x80, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true);
            TestAdd(instruction, 0x88, 0x88, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true);
        }

        [Fact]
        public void TestAdcA()
        {
            byte instruction = 0x88 | (byte)Cpu.RegisterEncoding.A;
            Action<Cpu, byte> SetRegister = (cpu, value) => cpu.A = value;
            TestAdd(instruction, 0x40, 0x40, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false);
            TestAdd(instruction, 0x08, 0x08, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false);
            TestAdd(instruction, 0x80, 0x80, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true);
            TestAdd(instruction, 0x88, 0x88, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true);

            TestAdd(instruction, 0x40, 0x40, SetRegister, setFlagC: true, halfCarry: false, fullCarry: false);
            TestAdd(instruction, 0x08, 0x08, SetRegister, setFlagC: true, halfCarry: true, fullCarry: false);
            TestAdd(instruction, 0x80, 0x80, SetRegister, setFlagC: true, halfCarry: false, fullCarry: true);
            TestAdd(instruction, 0x88, 0x88, SetRegister, setFlagC: true, halfCarry: true, fullCarry: true);

            TestAdd(instruction, 0x40, 0x40, SetRegister, setFlagC: true, halfCarry: false, fullCarry: false);
            TestAdd(instruction, 0x07, 0x07, SetRegister, setFlagC: true, halfCarry: false, fullCarry: false);
            TestAdd(instruction, 0x70, 0x70, SetRegister, setFlagC: true, halfCarry: false, fullCarry: false);
            TestAdd(instruction, 0x77, 0x77, SetRegister, setFlagC: true, halfCarry: false, fullCarry: false);
        }

        public void TestSub(
            byte instruction,
            byte lhs,
            byte rhs,
            Action<Cpu, byte> SetRegister,
            bool setFlagC,
            bool halfCarry,
            bool fullCarry,
            int cycles = 4,
            bool rhsIsImmediate = false)
        {
            byte result = (byte)(lhs - (rhs + ((setFlagC && ((instruction & 0x80) > 0)) ? 1 : 0)));
            var test = new InstructionTest(instruction)
                .WithClockCycles(cycles)
                .WithTestPreparation(cpu =>
                {
                    cpu.A = lhs;
                    cpu.F = 0;
                    cpu.flagN = true;
                    cpu.flagC = setFlagC;
                    if (!rhsIsImmediate)
                    {
                        SetRegister(cpu, rhs);
                    }
                })
                .WithPreValidation(cpu =>
                {
                    Assert.NotEqual(result, cpu.A);
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal(result, cpu.A);
                    Assert.Equal(cpu.A == 0, cpu.flagZ);
                    Assert.Equal(true, cpu.flagN);
                    Assert.Equal(halfCarry, cpu.flagH);
                    Assert.Equal(fullCarry, cpu.flagC);
                });

            if (rhsIsImmediate)
            {
                test.WithImmediateByte(rhs);
            }

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestSubAllRegisters()
        {
            // Test rhs of B-D. Skip A and (HL)
            for (byte i = 0; i < 6; ++i)
            {
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)(i & 0b111);
                byte instruction = (byte)(0x90 | i);
                Action<Cpu, byte> SetRegister = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                TestSub(instruction, 0x45, 0x05, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false);
                TestSub(instruction, 0x45, 0x06, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false);
                TestSub(instruction, 0x10, 0x20, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true);
                TestSub(instruction, 0x11, 0x12, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true);
            }
        }

        [Fact]
        public void TestSbcAllRegisters()
        {
            // Test rhs of B-D. Skip A and (HL)
            for (byte i = 0; i < 6; ++i)
            {
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)(i & 0b111);
                byte instruction = (byte)(0x98 | i);
                Action<Cpu, byte> SetRegister = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                TestSub(instruction, 0x45, 0x05, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false);
                TestSub(instruction, 0x45, 0x06, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false);
                TestSub(instruction, 0x10, 0x20, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true);
                TestSub(instruction, 0x11, 0x12, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true);

                TestSub(instruction, 0x45, 0x04, SetRegister, setFlagC: true, halfCarry: false, fullCarry: false);
                TestSub(instruction, 0x45, 0x05, SetRegister, setFlagC: true, halfCarry: true, fullCarry: false);
                TestSub(instruction, 0x18, 0x27, SetRegister, setFlagC: true, halfCarry: false, fullCarry: true);
                TestSub(instruction, 0x11, 0x11, SetRegister, setFlagC: true, halfCarry: true, fullCarry: true);
            }
        }

        [Fact]
        public void TestSubHLDeref()
        {
            byte instruction = (byte)(0x90 | (byte)Cpu.RegisterEncoding.HLDeref);
            Action<Cpu, byte> SetRegister = (cpu, value) =>
            {
                cpu.HL = 0x1234;
                cpu._memory.Write(cpu.HL, value);
            };
            TestSub(instruction, 0x45, 0x05, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false, cycles: 8);
            TestSub(instruction, 0x45, 0x06, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false, cycles: 8);
            TestSub(instruction, 0x10, 0x20, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true, cycles: 8);
            TestSub(instruction, 0x11, 0x12, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true, cycles: 8);
        }

        [Fact]
        public void TestSbcHLDeref()
        {
            byte instruction = (byte)(0x98 | (byte)Cpu.RegisterEncoding.HLDeref);
            Action<Cpu, byte> SetRegister = (cpu, value) =>
            {
                cpu.HL = 0x1234;
                cpu._memory.Write(cpu.HL, value);
            };
            TestSub(instruction, 0x45, 0x05, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false, cycles: 8);
            TestSub(instruction, 0x45, 0x06, SetRegister, setFlagC: false, halfCarry: true, fullCarry: false, cycles: 8);
            TestSub(instruction, 0x10, 0x20, SetRegister, setFlagC: false, halfCarry: false, fullCarry: true, cycles: 8);
            TestSub(instruction, 0x11, 0x12, SetRegister, setFlagC: false, halfCarry: true, fullCarry: true, cycles: 8);

            TestSub(instruction, 0x45, 0x04, SetRegister, setFlagC: true, halfCarry: false, fullCarry: false, cycles: 8);
            TestSub(instruction, 0x45, 0x05, SetRegister, setFlagC: true, halfCarry: true, fullCarry: false, cycles: 8);
            TestSub(instruction, 0x18, 0x27, SetRegister, setFlagC: true, halfCarry: false, fullCarry: true, cycles: 8);
            TestSub(instruction, 0x11, 0x11, SetRegister, setFlagC: true, halfCarry: true, fullCarry: true, cycles: 8);
        }

        [Fact]
        public void TestSubImmediate()
        {
            byte instruction = 0xD6;
            TestSub(instruction, 0x45, 0x05, null, setFlagC: false, halfCarry: false, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestSub(instruction, 0x45, 0x06, null, setFlagC: false, halfCarry: true, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestSub(instruction, 0x10, 0x20, null, setFlagC: false, halfCarry: false, fullCarry: true, cycles: 8, rhsIsImmediate: true);
            TestSub(instruction, 0x11, 0x12, null, setFlagC: false, halfCarry: true, fullCarry: true, cycles: 8, rhsIsImmediate: true);
        }

        [Fact]
        public void TestSbcImmediate()
        {
            byte instruction = 0xDE;
            TestSub(instruction, 0x45, 0x05, null, setFlagC: false, halfCarry: false, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestSub(instruction, 0x45, 0x06, null, setFlagC: false, halfCarry: true, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestSub(instruction, 0x10, 0x20, null, setFlagC: false, halfCarry: false, fullCarry: true, cycles: 8, rhsIsImmediate: true);
            TestSub(instruction, 0x11, 0x12, null, setFlagC: false, halfCarry: true, fullCarry: true, cycles: 8, rhsIsImmediate: true);

            TestSub(instruction, 0x45, 0x04, null, setFlagC: true, halfCarry: false, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestSub(instruction, 0x45, 0x05, null, setFlagC: true, halfCarry: true, fullCarry: false, cycles: 8, rhsIsImmediate: true);
            TestSub(instruction, 0x18, 0x27, null, setFlagC: true, halfCarry: false, fullCarry: true, cycles: 8, rhsIsImmediate: true);
            TestSub(instruction, 0x11, 0x11, null, setFlagC: true, halfCarry: true, fullCarry: true, cycles: 8, rhsIsImmediate: true);
        }

        [Fact]
        public void TestSubA()
        {
            byte instruction = 0x90 | (byte)Cpu.RegisterEncoding.A;
            Action<Cpu, byte> SetRegister = (cpu, value) => cpu.A = value;
            TestSub(instruction, 0x45, 0x45, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false);
        }

        [Fact]
        public void TestSbcA()
        {
            byte instruction = 0x98 | (byte)Cpu.RegisterEncoding.A;
            Action<Cpu, byte> SetRegister = (cpu, value) => cpu.A = value;
            TestSub(instruction, 0x45, 0x45, SetRegister, setFlagC: false, halfCarry: false, fullCarry: false);
            TestSub(instruction, 0x18, 0x18, SetRegister, setFlagC: true, halfCarry: true, fullCarry: true);
        }

        public void TestOperation(
            byte instruction,
            byte lhs,
            byte rhs,
            Func<byte, byte, byte> GetExpectedResult,
            Action<Cpu, byte> SetRegister,
            Func<Cpu, byte> GetResult = null,
            bool expectedFlagH = false,
            bool expectedFlagC = false,
            bool expectedFlagN = false,
            bool? expectedFlagZ = null,
            int cycles = 4,
            bool rhsIsImmediate = false)
        {
            if (null == GetResult)
            {
                GetResult = (cpu) => cpu.A;
            }

            var test = new InstructionTest(instruction)
                .WithClockCycles(cycles)
                .WithTestPreparation(cpu =>
                {
                    cpu.A = lhs;
                    cpu.F = 0;
                    if (!rhsIsImmediate)
                    {
                        SetRegister(cpu, rhs);
                    }
                })
                .WithPostValidation(cpu =>
                {
                    if (null != GetExpectedResult)
                    {
                        Assert.Equal(GetExpectedResult(lhs, rhs), GetResult(cpu));
                    }
                    if (null == expectedFlagZ)
                    {
                        Assert.Equal(cpu.A == 0, cpu.flagZ);
                    }
                    else
                    {
                        Assert.Equal(expectedFlagZ.Value, cpu.flagZ);
                    }
                    Assert.Equal(expectedFlagN, cpu.flagN);
                    Assert.Equal(expectedFlagH, cpu.flagH);
                    Assert.Equal(expectedFlagC, cpu.flagC);
                });

            if (rhsIsImmediate)
            {
                test.WithImmediateByte(rhs);
            }

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestAndSimpleRegisters()
        {
            // Test rhs of B-D. Skip A and (HL)
            for (byte i = 0; i < 6; ++i)
            {
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)i;
                byte instruction = (byte)(0xA0 | (byte)register);
                Func<byte, byte, byte> GetExpectedResult = (lhs, rhs) => (byte)(lhs & rhs);
                Action<Cpu, Byte> SetRightOperand = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                TestOperation(instruction, 0x0, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: true);
                TestOperation(instruction, 0x0, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: true);
                TestOperation(instruction, 0xF, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: true);
                TestOperation(instruction, 0xF, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: true);
            }
        }

        [Fact]
        public void TestAndHLDeref()
        {
            Cpu.RegisterEncoding register = Cpu.RegisterEncoding.HLDeref;
            byte instruction = (byte)(0xA0 | (byte)register);
            Func<byte, byte, byte> GetExpectedResult = (lhs, rhs) => (byte)(lhs & rhs);
            Action<Cpu, Byte> SetRightOperand = (cpu, value) =>
            {
                cpu.HL = 0x1234;
                cpu._memory.Write(cpu.HL, value);
            };
            TestOperation(instruction, 0x0, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: true, cycles: 8);
            TestOperation(instruction, 0x0, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: true, cycles: 8);
            TestOperation(instruction, 0xF, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: true, cycles: 8);
            TestOperation(instruction, 0xF, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: true, cycles: 8);
        }

        [Fact]
        public void TestAndImmediate()
        {
            byte instruction = 0xE6;
            Func<byte, byte, byte> GetExpectedResult = (lhs, rhs) => (byte)(lhs & rhs);
            TestOperation(instruction, 0x0, 0x0, GetExpectedResult, null, expectedFlagH: true, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0x0, 0xF, GetExpectedResult, null, expectedFlagH: true, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0xF, 0x0, GetExpectedResult, null, expectedFlagH: true, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0xF, 0xF, GetExpectedResult, null, expectedFlagH: true, cycles: 8, rhsIsImmediate: true);
        }

        [Fact]
        public void TestXorSimpleRegisters()
        {
            // Test rhs of B-D. Skip A and (HL)
            for (byte i = 0; i < 6; ++i)
            {
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)i;
                byte instruction = (byte)(0xA8 | (byte)register);
                Func<byte, byte, byte> GetExpectedResult = (lhs, rhs) => (byte)(lhs ^ rhs);
                Action<Cpu, Byte> SetRightOperand = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                TestOperation(instruction, 0x0, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: false);
                TestOperation(instruction, 0x0, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: false);
                TestOperation(instruction, 0xF, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: false);
                TestOperation(instruction, 0xF, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: false);
            }
        }

        [Fact]
        public void TestXorHLDeref()
        {
            Cpu.RegisterEncoding register = Cpu.RegisterEncoding.HLDeref;
            byte instruction = (byte)(0xA8 | (byte)register);
            Func<byte, byte, byte> GetExpectedResult = (lhs, rhs) => (byte)(lhs ^ rhs);
            Action<Cpu, Byte> SetRightOperand = (cpu, value) =>
            {
                cpu.HL = 0x1234;
                cpu._memory.Write(cpu.HL, value);
            };
            TestOperation(instruction, 0x0, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: false, cycles: 8);
            TestOperation(instruction, 0x0, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: false, cycles: 8);
            TestOperation(instruction, 0xF, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: false, cycles: 8);
            TestOperation(instruction, 0xF, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: false, cycles: 8);
        }

        [Fact]
        public void TestXorImmediate()
        {
            byte instruction = 0xEE;
            Func<byte, byte, byte> GetExpectedResult = (lhs, rhs) => (byte)(lhs ^ rhs);
            TestOperation(instruction, 0x0, 0x0, GetExpectedResult, null, expectedFlagH: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0x0, 0xF, GetExpectedResult, null, expectedFlagH: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0xF, 0x0, GetExpectedResult, null, expectedFlagH: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0xF, 0xF, GetExpectedResult, null, expectedFlagH: false, cycles: 8, rhsIsImmediate: true);
        }

        [Fact]
        public void TestOrSimpleRegisters()
        {
            // Test rhs of B-D. Skip A and (HL)
            for (byte i = 0; i < 6; ++i)
            {
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)i;
                byte instruction = (byte)(0xB0 | (byte)register);
                Func<byte, byte, byte> GetExpectedResult = (lhs, rhs) => (byte)(lhs | rhs);
                Action<Cpu, Byte> SetRightOperand = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                TestOperation(instruction, 0x0, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: false);
                TestOperation(instruction, 0x0, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: false);
                TestOperation(instruction, 0xF, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: false);
                TestOperation(instruction, 0xF, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: false);
            }
        }

        [Fact]
        public void TestOrHLDeref()
        {
            Cpu.RegisterEncoding register = Cpu.RegisterEncoding.HLDeref;
            byte instruction = (byte)(0xB0 | (byte)register);
            Func<byte, byte, byte> GetExpectedResult = (lhs, rhs) => (byte)(lhs | rhs);
            Action<Cpu, Byte> SetRightOperand = (cpu, value) =>
            {
                cpu.HL = 0x1234;
                cpu._memory.Write(cpu.HL, value);
            };
            TestOperation(instruction, 0x0, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: false, cycles: 8);
            TestOperation(instruction, 0x0, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: false, cycles: 8);
            TestOperation(instruction, 0xF, 0x0, GetExpectedResult, SetRightOperand, expectedFlagH: false, cycles: 8);
            TestOperation(instruction, 0xF, 0xF, GetExpectedResult, SetRightOperand, expectedFlagH: false, cycles: 8);
        }

        [Fact]
        public void TestOrImmediate()
        {
            byte instruction = 0xF6;
            Func<byte, byte, byte> GetExpectedResult = (lhs, rhs) => (byte)(lhs | rhs);
            TestOperation(instruction, 0x0, 0x0, GetExpectedResult, null, expectedFlagH: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0x0, 0xF, GetExpectedResult, null, expectedFlagH: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0xF, 0x0, GetExpectedResult, null, expectedFlagH: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0xF, 0xF, GetExpectedResult, null, expectedFlagH: false, cycles: 8, rhsIsImmediate: true);
        }

        [Fact]
        public void TestCpSimpleRegisters()
        {
            // Test rhs of B-D. Skip A
            for (byte i = 0; i < 7; ++i)
            {
                int cycles = 4;
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)i;
                byte instruction = (byte)(0xB8 | (byte)register);
                Action<Cpu, Byte> SetRightOperand;

                if (register == Cpu.RegisterEncoding.HLDeref)
                {
                    cycles = 8;
                    SetRightOperand = (cpu, value) =>
                    {
                        cpu.HL = 0x1234;
                        cpu._memory.Write(cpu.HL, value);
                    };
                }
                else
                {
                    SetRightOperand = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                }

                TestOperation(instruction, 0x45, 0x05, null, SetRightOperand, expectedFlagH: false, expectedFlagC: false, expectedFlagN: true, expectedFlagZ: false, cycles: cycles);
                TestOperation(instruction, 0x45, 0x06, null, SetRightOperand, expectedFlagH: true, expectedFlagC: false, expectedFlagN: true, expectedFlagZ: false, cycles: cycles);
                TestOperation(instruction, 0x10, 0x20, null, SetRightOperand, expectedFlagH: false, expectedFlagC: true, expectedFlagN: true, expectedFlagZ: false, cycles: cycles);
                TestOperation(instruction, 0x11, 0x12, null, SetRightOperand, expectedFlagH: true, expectedFlagC: true, expectedFlagN: true, expectedFlagZ: false, cycles: cycles);
                TestOperation(instruction, 0x12, 0x12, null, SetRightOperand, expectedFlagH: false, expectedFlagC: false, expectedFlagN: true, expectedFlagZ: true, cycles: cycles);
            }
        }

        [Fact]
        public void TestCpImmediate()
        {
            byte instruction = 0xFE;
            TestOperation(instruction, 0x45, 0x05, null, null, expectedFlagH: false, expectedFlagC: false, expectedFlagN: true, expectedFlagZ: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0x45, 0x06, null, null, expectedFlagH: true, expectedFlagC: false, expectedFlagN: true, expectedFlagZ: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0x10, 0x20, null, null, expectedFlagH: false, expectedFlagC: true, expectedFlagN: true, expectedFlagZ: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0x11, 0x12, null, null, expectedFlagH: true, expectedFlagC: true, expectedFlagN: true, expectedFlagZ: false, cycles: 8, rhsIsImmediate: true);
            TestOperation(instruction, 0x12, 0x12, null, null, expectedFlagH: false, expectedFlagC: false, expectedFlagN: true, expectedFlagZ: true, cycles: 8, rhsIsImmediate: true);
        }

        [Fact]
        public void TestCpA()
        {
            Cpu.RegisterEncoding register = Cpu.RegisterEncoding.A;
            byte instruction = (byte)(0xB8 | (byte)register);
            Action<Cpu, Byte> SetRightOperand = (cpu, value) => cpu.A = value;
            TestOperation(instruction, 0xFF, 0xFF, null, SetRightOperand, expectedFlagH: false, expectedFlagC: false, expectedFlagN: true, expectedFlagZ: true);
        }

        [Fact]
        public void TestInc()
        {
            List<byte> testInstructions = new List<byte>
            {
                0x04,
                0x0C,
                0x14,
                0x1C,
                0x24,
                0x2C,
                0x34,
                0x3C
            };

            foreach (var instruction in testInstructions)
            {
                int cycles = 4;
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)((instruction >> 3) & 0x7);
                Action<Cpu, Byte> SetOperand;
                Func<Cpu, byte> GetResult;

                if (register == Cpu.RegisterEncoding.HLDeref)
                {
                    cycles = 12;
                    ushort address = 0x1234;
                    SetOperand = (cpu, value) =>
                    {
                        cpu.HL = address;
                        cpu._memory.Write(cpu.HL, value);
                    };
                    GetResult = (cpu) =>
                    {
                        return cpu._memory.Read(address);
                    };
                }
                else
                {
                    SetOperand = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                    GetResult = (cpu) => cpu.registers[Cpu.RegisterEncodingToIndex(register)];
                }

                Func<byte, byte, byte> GetExpectedResult = (byte lhs, byte rhs) => (byte)((lhs + 1) & 0xFF);

                TestOperation(instruction, 0x00, 0x00, GetExpectedResult, SetOperand, GetResult, expectedFlagH: false, expectedFlagC: false, expectedFlagZ: false, cycles: cycles);
                TestOperation(instruction, 0xFF, 0xFF, GetExpectedResult, SetOperand, GetResult, expectedFlagH: true, expectedFlagC: true, expectedFlagZ: true, cycles: cycles); // TODO: Some documentation says that C isn't affected by this instruction
                TestOperation(instruction, 0xEF, 0xEF, GetExpectedResult, SetOperand, GetResult, expectedFlagH: true, expectedFlagC: false, expectedFlagZ: false, cycles: cycles);
                TestOperation(instruction, 0xF0, 0xF0, GetExpectedResult, SetOperand, GetResult, expectedFlagH: false, expectedFlagC: false, expectedFlagZ: false, cycles: cycles);
            }
        }

        [Fact]
        public void TestDec()
        {
            List<byte> testInstructions = new List<byte>
            {
                0x05,
                0x0D,
                0x15,
                0x1D,
                0x25,
                0x2D,
                0x35,
                0x3D
            };

            foreach (var instruction in testInstructions)
            {
                int cycles = 4;
                Cpu.RegisterEncoding register = (Cpu.RegisterEncoding)((instruction >> 3) & 0x7);
                Action<Cpu, Byte> SetOperand;
                Func<Cpu, byte> GetResult;

                if (register == Cpu.RegisterEncoding.HLDeref)
                {
                    cycles = 12;
                    ushort address = 0x1234;
                    SetOperand = (cpu, value) =>
                    {
                        cpu.HL = address;
                        cpu._memory.Write(cpu.HL, value);
                    };
                    GetResult = (cpu) =>
                    {
                        return cpu._memory.Read(address);
                    };
                }
                else
                {
                    SetOperand = (cpu, value) => cpu.registers[Cpu.RegisterEncodingToIndex(register)] = value;
                    GetResult = (cpu) => cpu.registers[Cpu.RegisterEncodingToIndex(register)];
                }

                Func<byte, byte, byte> GetExpectedResult = (byte lhs, byte rhs) => (byte)((lhs - 1) & 0xFF);

                TestOperation(instruction, 0x00, 0x00, GetExpectedResult, SetOperand, GetResult, expectedFlagH: true, expectedFlagC: true, expectedFlagZ: false, expectedFlagN: true, cycles: cycles);
                TestOperation(instruction, 0xFF, 0xFF, GetExpectedResult, SetOperand, GetResult, expectedFlagH: false, expectedFlagC: false, expectedFlagZ: false, expectedFlagN: true, cycles: cycles);
                TestOperation(instruction, 0x01, 0x01, GetExpectedResult, SetOperand, GetResult, expectedFlagH: false, expectedFlagC: false, expectedFlagZ: true, expectedFlagN: true, cycles: cycles);
                TestOperation(instruction, 0xF0, 0xF0, GetExpectedResult, SetOperand, GetResult, expectedFlagH: true, expectedFlagC: false, expectedFlagZ: false, expectedFlagN: true, cycles: cycles);
            }
        }


        [Fact]
        public void TestIncWord()
        {
            List<byte> testInstructions = new List<byte>
            {
                0x03,
                0x13,
                0x23,
                0x33
            };

            Dictionary<byte, Cpu.WordRegisterEncoding> instructionToRegisterMap = new Dictionary<byte, Cpu.WordRegisterEncoding>
            {
                [0x03] = Cpu.WordRegisterEncoding.BC,
                [0x13] = Cpu.WordRegisterEncoding.DE,
                [0x23] = Cpu.WordRegisterEncoding.HL,
                [0x33] = Cpu.WordRegisterEncoding.SP
            };

            foreach (var instruction in testInstructions)
            {
                Cpu.WordRegisterEncoding register = instructionToRegisterMap[instruction];
                ushort initialValue = 0x11;

                var test = new InstructionTest(instruction)
                    .WithClockCycles(8)
                    .WithTestPreparation(cpu =>
                    {
                        cpu.WriteToTargetWordRegister(register, initialValue);
                    })
                    .WithPostValidation(cpu =>
                    {
                        Assert.Equal(initialValue + 1, cpu.ReadWordRegisterValue(register));
                        // TODO: Would be nice to ensure flags aren't affected (if documentation is correct)
                    });

                var runner = new InstructionTestRunner(test);
                runner.Run();
            }
        }

        // TODO: This is very similar to above... should share code
        [Fact]
        public void TestDecWord()
        {
            List<byte> testInstructions = new List<byte>
            {
                0x0B,
                0x1B,
                0x2B,
                0x3B
            };

            Dictionary<byte, Cpu.WordRegisterEncoding> instructionToRegisterMap = new Dictionary<byte, Cpu.WordRegisterEncoding>
            {
                [0x0B] = Cpu.WordRegisterEncoding.BC,
                [0x1B] = Cpu.WordRegisterEncoding.DE,
                [0x2B] = Cpu.WordRegisterEncoding.HL,
                [0x3B] = Cpu.WordRegisterEncoding.SP
            };

            foreach (var instruction in testInstructions)
            {
                Cpu.WordRegisterEncoding register = instructionToRegisterMap[instruction];
                ushort initialValue = 0x11;

                var test = new InstructionTest(instruction)
                    .WithClockCycles(8)
                    .WithTestPreparation(cpu =>
                    {
                        cpu.WriteToTargetWordRegister(register, initialValue);
                    })
                    .WithPostValidation(cpu =>
                    {
                        Assert.Equal(initialValue - 1, cpu.ReadWordRegisterValue(register));
                        // TODO: Would be nice to ensure flags aren't affected (if documentation is correct)
                    });

                var runner = new InstructionTestRunner(test);
                runner.Run();
            }
        }

        [Fact]
        public void TestLdHLInc()
        {
            ushort initialHLValue = 0x1234;
            byte initialMemoryValue = 0x05;
            byte finalMemoryValue = 0x20;
            var test = new InstructionTest(0x22)
                .WithClockCycles(8)
                .WithTestPreparation(cpu =>
                {
                    cpu.WriteToTargetWordRegister(Cpu.WordRegisterEncoding.HL, initialHLValue);
                    cpu._memory.Write(initialHLValue, initialMemoryValue);
                    cpu.A = finalMemoryValue;
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal(initialHLValue + 1, cpu.ReadWordRegisterValue(Cpu.WordRegisterEncoding.HL));
                    Assert.Equal(finalMemoryValue, cpu._memory.Read(initialHLValue));
                    Assert.Equal(initialHLValue + 1, cpu.HL);
                    // TODO: Would be nice to ensure flags aren't affected (if documentation is correct)
                });

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestLdHLDec()
        {
            ushort initialHLValue = 0x1234;
            byte initialMemoryValue = 0x05;
            byte finalMemoryValue = 0x20;
            var test = new InstructionTest(0x32)
                .WithClockCycles(8)
                .WithTestPreparation(cpu =>
                {
                    cpu.WriteToTargetWordRegister(Cpu.WordRegisterEncoding.HL, initialHLValue);
                    cpu._memory.Write(initialHLValue, initialMemoryValue);
                    cpu.A = finalMemoryValue;
                })
                .WithPostValidation(cpu =>
                {
                    Assert.Equal(initialHLValue - 1, cpu.ReadWordRegisterValue(Cpu.WordRegisterEncoding.HL));
                    Assert.Equal(finalMemoryValue, cpu._memory.Read(initialHLValue));
                    Assert.Equal(initialHLValue - 1, cpu.HL);
                    // TODO: Would be nice to ensure flags aren't affected (if documentation is correct)
                });

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }

        [Fact]
        public void TestRST()
        {
            var instructions = new List<byte>
            {
                0xC7,
                0xD7,
                0xE7,
                0xF7,
                0xCF,
                0xDF,
                0xEF,
                0xFF
            };

            var instructionToAddressMap = new Dictionary<byte, ushort>
            {
                [0xC7] = 0x00,
                [0xD7] = 0x10,
                [0xE7] = 0x20,
                [0xF7] = 0x30,
                [0xCF] = 0x08,
                [0xDF] = 0x18,
                [0xEF] = 0x28,
                [0xFF] = 0x38,
            };

            ushort initialPC = 0x100;
            ushort initialSP = 0xFFFE;

            foreach (var instruction in instructions)
            {
                var test = new InstructionTest(instruction)
                    .WithClockCycles(16)
                    .WithTestPreparation(cpu =>
                    {
                        cpu.PC = initialPC;
                        cpu.SP = initialSP;
                    })
                    .WithPostExecutionPC(instructionToAddressMap[instruction])
                    .WithPostValidation(cpu =>
                    {
                        Assert.Equal(initialSP - 2, cpu.SP);
                        Assert.Equal(initialPC, cpu._memory.ReadWord(cpu.SP));
                    });

                var runner = new InstructionTestRunner(test);
                runner.Run();
            }
        }

        [Fact]
        public void TestJumpRelative()
        {
            ushort initialPC = 0x100;

            // Array of instruction, if jump should be performed, and function to set the flag
            var configs = new List<Tuple<byte, bool, sbyte, Action<Cpu>>>
            {
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x18, true, 10, (Cpu cpu) => {} ),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x18, true, -10, (Cpu cpu) => {} ),

                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x20, true, 10, (Cpu cpu) => cpu.flagZ = false),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x20, false, 10, (Cpu cpu) => cpu.flagZ = true),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x20, true, -10, (Cpu cpu) => cpu.flagZ = false),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x20, false, -10, (Cpu cpu) => cpu.flagZ = true),

                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x30, true, 10, (Cpu cpu) => cpu.flagC = false),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x30, false, 10, (Cpu cpu) => cpu.flagC = true),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x30, true, -10, (Cpu cpu) => cpu.flagC = false),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x30, false, -10, (Cpu cpu) => cpu.flagC = true),

                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x28, true, 10, (Cpu cpu) => cpu.flagZ = true),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x28, false, 10, (Cpu cpu) => cpu.flagZ = false),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x28, true, -10, (Cpu cpu) => cpu.flagZ = true),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x28, false, -10, (Cpu cpu) => cpu.flagZ = false),

                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x38, true, 10, (Cpu cpu) => cpu.flagC = true),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x38, false, 10, (Cpu cpu) => cpu.flagC = false),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x38, true, -10, (Cpu cpu) => cpu.flagC = true),
                new Tuple<byte, bool, sbyte, Action<Cpu>>(0x38, false, -10, (Cpu cpu) => cpu.flagC = false),
            };

            foreach (var config in configs)
            {
                var instruction = config.Item1;
                var shouldPerformJump = config.Item2;
                var offset = config.Item3;
                var setFlag = config.Item4;

                var test = new InstructionTest(instruction)
                    .WithClockCycles(shouldPerformJump ? 12 : 8)
                    .WithTestPreparation(cpu =>
                    {
                        cpu.PC = initialPC;
                        setFlag(cpu);
                    })
                    .WithImmediateByte(unchecked((byte)offset));

                if (shouldPerformJump)
                {
                    test = test.WithPostExecutionPC((ushort)(initialPC + offset + 2));
                }

                var runner = new InstructionTestRunner(test);
                runner.Run();
            }
        }

        [Fact]
        public void TestReturnConditional()
        {
            ushort initialPC = 0x100;

            // Array of instruction, if jump should be performed, and function to set the flag
            var configs = new List<Tuple<byte, bool, Action<Cpu>>>
            {
                new Tuple<byte, bool, Action<Cpu>>(0xC0, true, (Cpu cpu) => cpu.flagZ = false ),
                new Tuple<byte, bool, Action<Cpu>>(0xC0, false, (Cpu cpu) => cpu.flagZ = true ),

                new Tuple<byte, bool, Action<Cpu>>(0xD0, true, (Cpu cpu) => cpu.flagC = false ),
                new Tuple<byte, bool, Action<Cpu>>(0xD0, false, (Cpu cpu) => cpu.flagC = true ),

                new Tuple<byte, bool, Action<Cpu>>(0xC8, false, (Cpu cpu) => cpu.flagZ = false ),
                new Tuple<byte, bool, Action<Cpu>>(0xC8, true, (Cpu cpu) => cpu.flagZ = true ),

                new Tuple<byte, bool, Action<Cpu>>(0xD8, false, (Cpu cpu) => cpu.flagC = false ),
                new Tuple<byte, bool, Action<Cpu>>(0xD8, true, (Cpu cpu) => cpu.flagC = true )
            };

            foreach (var config in configs)
            {
                var instruction = config.Item1;
                var shouldPerformJump = config.Item2;
                var setFlag = config.Item3;

                ushort returnAddress = 0x0150;

                var test = new InstructionTest(instruction)
                    .WithClockCycles(shouldPerformJump ? 20 : 8)
                    .WithTestPreparation(cpu =>
                    {
                        cpu.PC = initialPC;
                        cpu.SP -= 2;
                        cpu._memory.WriteWord(cpu.SP, returnAddress);
                        setFlag(cpu);
                    });

                if (shouldPerformJump)
                {
                    test = test.WithPostExecutionPC(returnAddress);
                }

                var runner = new InstructionTestRunner(test);
                runner.Run();
            }
        }

        [Fact]
        public void TestReturn()
        {
            ushort returnAddress = 0x0150;

            var test = new InstructionTest(0xC9)
                .WithClockCycles(16)
                .WithTestPreparation(cpu =>
                {
                    cpu.SP -= 2;
                    cpu._memory.WriteWord(cpu.SP, returnAddress);
                })
                .WithPostExecutionPC(returnAddress);

            var runner = new InstructionTestRunner(test);
            runner.Run();
        }
        // Test debt
        // NOP (0x00)
        // LD, nn (0xC3)
        // DI (0xF3)
        // EI (0xFB)
        // JR NZ, n (0x20)
        // JR Z, n (0x28)
        // CALL nn (0xCD)
        // INC register word (0x0-3 3) 0x03, 0x13, 0x23, 0x33 - this is implemented but maybe needs tests for flags
    }
}
