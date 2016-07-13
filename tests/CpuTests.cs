using Xunit;

namespace GameBoyTests
{
    public class Cpu
    {
        public class CpuTests
        {
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
        }
    }
}
