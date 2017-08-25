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

        // Memory
        internal Memory memory;

        internal void Init()
        {
            // Initialize memory
            memory = new Memory();
            memory.Init();
            // Set the program counter to it's initial value
            PC = 0x100;
            // Set initial stack pointer value
            SP = 0xFFFE;
        }

        internal void ExecuteNextInstruction()
        {
            // Executes the provided instruction.
            // Some instructions leave the CPU in a state
            // that will mean the following instruction
            // is an alternate, so at the end of this
            // method we need to either record that state
            // or reset the instruction table
            byte instruction = memory.Read(this.PC);
            ushort cyclesUsed = 4;

            System.Console.WriteLine($"Instruction 0x{instruction:X2}");

            switch (instruction)
            {
                case 0x02:
                    memory.Write(BC, A);
                    break;
                case 0x06:
                    B = memory.Read(++PC);
                    cyclesUsed += 4;
                    break;
                case 0x0A:
                    A = memory.Read(BC);
                    cyclesUsed += 4;
                    break;
                case 0x0E:
                    C = memory.Read(++PC);
                    cyclesUsed += 4;
                    break;
                case 0x12:
                    memory.Write(DE, A);
                    break;
                case 0x16:
                    D = memory.Read(++PC);
                    cyclesUsed += 4;
                    break;
                case 0x1A:
                    A = memory.Read(DE);
                    cyclesUsed += 4;
                    break;
                case 0x1E:
                    E = memory.Read(++PC);
                    cyclesUsed += 4;
                    break;
                case 0x26:
                    H = memory.Read(++PC);
                    cyclesUsed += 4;
                    break;
                case 0x2E:
                    L = memory.Read(++PC);
                    cyclesUsed += 4;
                    break;
                case 0x36:
                    memory.Write(HL, memory.Read(++PC));
                    cyclesUsed += 8;
                    break;
                case 0x3E:
                    A = memory.Read(++PC);
                    cyclesUsed += 4;
                    break;
                // LD B, register
                case 0x40:
                    // LD B, B
                    break;
                case 0x41:
                    B = C;
                    break;
                case 0x42:
                    B = D;
                    break;
                case 0x43:
                    B = E;
                    break;
                case 0x44:
                    B = H;
                    break;
                case 0x45:
                    B = L;
                    break;
                case 0x46:
                    B = memory.Read(HL);
                    cyclesUsed += 4;
                    break;
                case 0x47:
                    B = A;
                    break;
                // LD C, register
                case 0x48:
                    C = B;
                    break;
                case 0x49:
                    // LD C, C
                    break;
                case 0x4A:
                    C = D;
                    break;
                case 0x4B:
                    C = E;
                    break;
                case 0x4C:
                    C = H;
                    break;
                case 0x4D:
                    C = L;
                    break;
                case 0x4E:
                    C = memory.Read(HL);
                    cyclesUsed += 4;
                    break;
                case 0x4F:
                    C = A;
                    break;
                // LD D, register
                case 0x50:
                    D = B;
                    break;
                case 0x51:
                    D = C;
                    break;
                case 0x52:
                    // LD D, D
                    break;
                case 0x53:
                    D = E;
                    break;
                case 0x54:
                    D = H;
                    break;
                case 0x55:
                    D = L;
                    break;
                case 0x56:
                    D = memory.Read(HL);
                    cyclesUsed += 4;
                    break;
                case 0x57:
                    D = A;
                    break;
                // LD E, register
                case 0x58:
                    E = B;
                    break;
                case 0x59:
                    E = C;
                    break;
                case 0x5A:
                    E = D;
                    break;
                case 0x5B:
                    // LD E, E
                    break;
                case 0x5C:
                    E = H;
                    break;
                case 0x5D:
                    E = L;
                    break;
                case 0x5E:
                    E = memory.Read(HL);
                    cyclesUsed += 4;
                    break;
                case 0x5F:
                    E = A;
                    break;
                // LD H, register
                case 0x60:
                    H = B;
                    break;
                case 0x61:
                    H = C;
                    break;
                case 0x62:
                    H = D;
                    break;
                case 0x63:
                    H = E;
                    break;
                case 0x64:
                    // LD H, H
                    break;
                case 0x65:
                    H = L;
                    break;
                case 0x66:
                    H = memory.Read(HL);
                    cyclesUsed += 4;
                    break;
                case 0x67:
                    H = A;
                    break;
                // LD L, register
                case 0x68:
                    L = B;
                    break;
                case 0x69:
                    L = C;
                    break;
                case 0x6A:
                    L = D;
                    break;
                case 0x6B:
                    L = E;
                    break;
                case 0x6C:
                    L = H;
                    break;
                case 0x6D:
                    // LD L, L
                    break;
                case 0x6E:
                    L = memory.Read(HL);
                    cyclesUsed += 4;
                    break;
                case 0x6F:
                    L = A;
                    break;
                // LD (HL), register
                case 0x70:
                    memory.Write(HL, B);
                    cyclesUsed += 4;
                    break;
                case 0x71:
                    memory.Write(HL, C);
                    cyclesUsed += 4;
                    break;
                case 0x72:
                    memory.Write(HL, D);
                    cyclesUsed += 4;
                    break;
                case 0x73:
                    memory.Write(HL, E);
                    cyclesUsed += 4;
                    break;
                case 0x74:
                    memory.Write(HL, H);
                    cyclesUsed += 4;
                    break;
                case 0x75:
                    memory.Write(HL, L);
                    cyclesUsed += 4;
                    break;
                case 0x77:
                    memory.Write(HL, A);
                    cyclesUsed += 4;
                    break;
                // LD A, register
                case 0x78:
                    A = B;
                    break;
                case 0x79:
                    A = C;
                    break;
                case 0x7A:
                    A = D;
                    break;
                case 0x7B:
                    A = E;
                    break;
                case 0x7C:
                    A = H;
                    break;
                case 0x7D:
                    A = L;
                    break;
                case 0x7E:
                    A = memory.Read(HL);
                    cyclesUsed += 4;
                    break;
                case 0x7F:
                    // LD A, A
                    break;
                case 0xEA:
                    // LD (nn), A
                    // Least significant byte first
                    memory.Write((ushort)(memory.Read(++PC) | memory.Read(++PC) << 8), A);
                    cyclesUsed += 12;
                    break;
                case 0xFA:
                    // LD A, (nn)
                    // Least significant byte first
                    A = memory.Read((ushort)(memory.Read(++PC) | memory.Read(++PC) << 8));
                    cyclesUsed += 12;
                    break;
            }

            // Increment the program counter
            this.PC++;
        }
    }
}