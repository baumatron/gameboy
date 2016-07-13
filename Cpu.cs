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

        // Flag convenience properties
        internal bool getBitWithIndex(byte target, ushort index)
        {
            return 0 != (target & (1 << index));
        }

        internal void setBitWithIndex(ref byte target, ushort index, bool bitValue)
        {
            target = (byte)((target & ~(1 << index)) | ((bitValue ? 1 : 0) << index));
        }

        // TODO: Could use a closure or a generic here to specify
        //       the index, and define a function that work son self.F directly instead
        //       of passing it by reference.
        internal bool flagZ
        {
            get
            {
                return getBitWithIndex(this.F, 7);
            }
            set
            {
                setBitWithIndex(ref this.F, 7, value);
            }
        }

        internal bool flagN
        {
            get
            {
                return getBitWithIndex(this.F, 6);
            }
            set
            {
                setBitWithIndex(ref this.F, 6, value);
            }
        }

        internal bool flagH
        {
            get
            {
                return getBitWithIndex(this.F, 5);
            }
            set
            {
                setBitWithIndex(ref this.F, 5, value);
            }
        }

        internal bool flagC
        {
            get
            {
                return getBitWithIndex(this.F, 4);
            }
            set
            {
                setBitWithIndex(ref this.F, 4, value);
            }
        }
    }
}