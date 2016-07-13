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
        }
    }
}
