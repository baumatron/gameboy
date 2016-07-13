using System;

namespace GameBoy
{
    public class Cpu
    {
        // Registers
        internal byte A, F, B, C, D, E, H, L;
        internal ushort SP, PC;

        // Combined registers
        internal ushort AF
        {
            get
            {
                return (ushort)(A << 8 | F);
            }
            set
            {
                A = (byte)(value >> 8);
                F = (byte)(value & 0xFF);
            }
        }

        internal ushort BC
        {
            get
            {
                return (ushort)(B << 8 | C);
            }
            set
            {
                B = (byte)(value >> 8);
                C = (byte)(value & 0xFF);
            }
        }
        internal ushort DE
        {
            get
            {
                return (ushort)(D << 8 | E);
            }
            set
            {
                D = (byte)(value >> 8);
                E = (byte)(value & 0xFF);
            }
        }

        internal ushort HL
        {
            get
            {
                return (ushort)(H << 8 | L);
            }
            set
            {
                H = (byte)(value >> 8);
                L = (byte)(value & 0xFF);
            }
        }

        public void TestRegisters()
        {
            A = 0xff;
            F = 0x10;

            Console.WriteLine($"A: 0x{A:X2} F: 0x{F:X2} AF: 0x{AF:X4}");

            if (AF != 0xff10)
            {
                Console.WriteLine("AF don't work.");
            }
            else
            {
                Console.WriteLine("Yes!");
            }
        }
    }
}