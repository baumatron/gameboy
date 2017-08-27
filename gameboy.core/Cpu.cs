using System;

namespace GameBoy
{
    public class Cpu
    {
        internal int clock;
        // Registers
        internal ushort SP, PC;

        // LD instructions (and others?) specify registers according to this encoding:
        //  111 - A
        //  000 - B
        //  001 - C
        //  010 - D
        //  011 - E
        //  100 - H
        //  101 - L
        //  110 - (HL)
        // If we add 1 to the encoding, we get a contiguous array for the registers, with a special value of 7 for the value at address HL
        internal byte[] registers = new byte[7];
        internal byte F;

        internal enum RegisterEncoding
        {
            B = 0b000,
            C = 0b001,
            D = 0b010,
            E = 0b011,
            H = 0b100,
            L = 0b101,
            HLderef = 0b110,
            A = 0b111
        }

        internal static int RegisterEncodingToIndex(RegisterEncoding encoding) => ((int)encoding + 1) & 0x7;

        internal byte A
        {
            get
            {
                return registers[RegisterEncodingToIndex(RegisterEncoding.A)];
            }
            set
            {
                registers[RegisterEncodingToIndex(RegisterEncoding.A)] = value;
            }
        }

        internal byte B
        {
            get
            {
                return registers[RegisterEncodingToIndex(RegisterEncoding.B)];
            }
            set
            {
                registers[RegisterEncodingToIndex(RegisterEncoding.B)] = value;
            }
        }

        internal byte C
        {
            get
            {
                return registers[RegisterEncodingToIndex(RegisterEncoding.C)];
            }
            set
            {
                registers[RegisterEncodingToIndex(RegisterEncoding.C)] = value;
            }
        }

        internal byte D
        {
            get
            {
                return registers[RegisterEncodingToIndex(RegisterEncoding.D)];
            }
            set
            {
                registers[RegisterEncodingToIndex(RegisterEncoding.D)] = value;
            }
        }

        internal byte E
        {
            get
            {
                return registers[RegisterEncodingToIndex(RegisterEncoding.E)];
            }
            set
            {
                registers[RegisterEncodingToIndex(RegisterEncoding.E)] = value;
            }
        }

        internal byte H
        {
            get
            {
                return registers[RegisterEncodingToIndex(RegisterEncoding.H)];
            }
            set
            {
                registers[RegisterEncodingToIndex(RegisterEncoding.H)] = value;
            }
        }

        internal byte L
        {
            get
            {
                return registers[RegisterEncodingToIndex(RegisterEncoding.L)];
            }
            set
            {
                registers[RegisterEncodingToIndex(RegisterEncoding.L)] = value;
            }
        }

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

            switch (instruction)
            {
                case 0x01:
                    // LD BC, nn
                    BC = (ushort)(memory.Read(++PC) | (memory.Read(++PC) << 8));
                    cyclesUsed += 8;
                    break;
                case 0x02:
                    // LD (BC), A
                    memory.Write(BC, A);
                    break;
                case 0x0A:
                    // LD A, (BC)
                    A = memory.Read(BC);
                    cyclesUsed += 4;
                    break;
                case 0x11:
                    // LD DE, nn
                    DE = (ushort)(memory.Read(++PC) | (memory.Read(++PC) << 8));
                    cyclesUsed += 8;
                    break;
                case 0x12:
                    // LD (DE), A
                    memory.Write(DE, A);
                    break;
                case 0x1A:
                    // LD A, (DE)
                    A = memory.Read(DE);
                    cyclesUsed += 4;
                    break;
                case 0x21:
                    // LD HL, nn
                    HL = (ushort)(memory.Read(++PC) | (memory.Read(++PC) << 8));
                    cyclesUsed += 8;
                    break;
                case 0x22:
                    // LDI (HL), A
                    memory.Write(HL++, A);
                    cyclesUsed += 4;
                    break;
                case 0x2A:
                    // LDI A, (HL)
                    A = memory.Read(HL++);
                    cyclesUsed += 4;
                    break;
                case 0x31:
                    // LD SP, nn
                    SP = (ushort)(memory.Read(++PC) | (memory.Read(++PC) << 8));
                    cyclesUsed += 8;
                    break;
                case 0x32:
                    // LDD (HL), A
                    memory.Write(HL--, A);
                    cyclesUsed += 4;
                    break;
                case 0x3A:
                    // LDD A, (HL)
                    A = memory.Read(HL--);
                    cyclesUsed += 4;
                    break;
                case 0xE0:
                    // LDH (0xFF00 + n), A
                    memory.Write((ushort)(0xFF00 + memory.Read(++PC)), A);
                    cyclesUsed += 8;
                    break;
                case 0xE2:
                    // LD (0xFF00 + C), A
                    memory.Write((ushort)(0xFF00 + C), A);
                    cyclesUsed += 4;
                    break;
                case 0xEA:
                    // LD (nn), A
                    // Least significant byte first
                    memory.Write((ushort)(memory.Read(++PC) | memory.Read(++PC) << 8), A);
                    cyclesUsed += 12;
                    break;
                case 0xF2:
                    // LD A, (0xFF00 + C)
                    A = memory.Read((ushort)(0xFF00 + C));
                    cyclesUsed += 4;
                    break;
                case 0xF0:
                    // LDH A, (0xFF00 + n)
                    A = memory.Read((ushort)(0xFF00 + memory.Read(++PC)));
                    cyclesUsed += 8;
                    break;
                case 0xF9:
                    // LD SP, HL
                    SP = HL;
                    cyclesUsed += 4;
                    break;
                case 0xFA:
                    // LD A, (nn)
                    // Least significant byte first
                    A = memory.Read((ushort)(memory.Read(++PC) | memory.Read(++PC) << 8));
                    cyclesUsed += 12;
                    break;

                default:
                    // Execute generalized instructions based on operand encodings extracted from the opcode
                    byte subInstruction = (byte)((instruction >> 6) & 0x3);
                    switch (subInstruction)
                    {
                        case 0x0:
                            {
                                if ((int)(instruction & 0x7) == 0b110) // LD immediate
                                {
                                    var target = (RegisterEncoding)((instruction >> 3) & 0x7);
                                    var value = memory.Read(++PC);
                                    cyclesUsed += 4;
                                    if (RegisterEncoding.HLderef == target)
                                    {
                                        memory.Write(HL, value);
                                        cyclesUsed += 4;
                                        // TODO: Validate that this is the cost of this instruction.
                                    }
                                    else
                                    {
                                        registers[RegisterEncodingToIndex(target)] = value;
                                    }
                                }
                                else
                                {
                                    throw new Exception($"Instruction not implemented 0x{instruction:X2}");
                                }
                            }
                            break;
                        case 0x1: // LD
                            {
                                var target = (RegisterEncoding)((instruction >> 3) & 0x7);
                                var source = (RegisterEncoding)((instruction) & 0x7);

                                if (RegisterEncoding.HLderef == target)
                                {
                                    // Confirm that 0x76 isn't a valid instruction, which would be ld (HL), HL
                                    // presumably it does something else
                                    memory.Write(HL, registers[RegisterEncodingToIndex(source)]);
                                    cyclesUsed += 4;
                                }
                                else
                                {
                                    int iTargetRegister = RegisterEncodingToIndex(target);
                                    if (RegisterEncoding.HLderef == source)
                                    {
                                        registers[iTargetRegister] = memory.Read(HL);
                                        cyclesUsed += 4;
                                    }
                                    else
                                    {
                                        registers[iTargetRegister] = registers[RegisterEncodingToIndex(source)];
                                    }
                                }
                            }
                            break;

                        default:
                            throw new Exception($"Instruction not implemented: 0x{instruction:X2}");
                    }
                    break;
            }

            // Increment the program counter
            PC++;

            // Increment the clock by the number of cycles used
            clock += cyclesUsed;
        }
    }
}