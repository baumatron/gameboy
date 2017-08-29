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
            // Handy link: http://clrhome.org/table/
            byte instruction = memory.Read(this.PC);
            ushort cyclesUsed = 4;

            switch (instruction)
            {
                case 0x01:
                    // LD BC, nn
                    BC = GetImmediateOperandWord(ref cyclesUsed);
                    break;
                case 0x02:
                    // LD (BC), A
                    memory.Write(BC, A);
                    break;
                case 0x08:
                    memory.WriteWord(GetImmediateOperandWord(ref cyclesUsed), SP);
                    cyclesUsed += 8;
                    break;
                case 0x0A:
                    // LD A, (BC)
                    A = memory.Read(BC);
                    cyclesUsed += 4;
                    break;
                case 0x11:
                    // LD DE, nn
                    DE = GetImmediateOperandWord(ref cyclesUsed);
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
                    HL = GetImmediateOperandWord(ref cyclesUsed);
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
                    SP = GetImmediateOperandWord(ref cyclesUsed);
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
                case 0xC1:
                    // POP BC
                    BC = memory.ReadWord(SP);
                    SP += 2;
                    cyclesUsed += 8;
                    break;
                case 0xC5:
                    memory.WriteWord(SP, BC);
                    SP -= 2;
                    cyclesUsed += 12;
                    break;
                case 0xC6:
                case 0xCE:
                    // ADD A, n
                    // ADC A, n
                    A = Add(A, GetImmediateOperand(ref cyclesUsed), useCarry: (instruction & 0x80) > 0);
                    break;
                case 0xD1:
                    // POP DE
                    DE = memory.ReadWord(SP);
                    SP += 2;
                    cyclesUsed += 8;
                    break;
                case 0xD5:
                    memory.WriteWord(SP, DE);
                    SP -= 2;
                    cyclesUsed += 12;
                    break;
                case 0xD6:
                case 0xDE:
                    // SUB A, n
                    // SBC A, n
                    A = Sub(A, GetImmediateOperand(ref cyclesUsed), useCarry: (instruction & 0x80) > 0);
                    break;
                case 0xE0:
                    // LDH (0xFF00 + n), A
                    memory.Write((ushort)(0xFF00 + GetImmediateOperand(ref cyclesUsed)), A);
                    cyclesUsed += 4;
                    break;
                case 0xE1:
                    // POP HL
                    HL = memory.ReadWord(SP);
                    SP += 2;
                    cyclesUsed += 8;
                    break;
                case 0xE2:
                    // LD (0xFF00 + C), A
                    memory.Write((ushort)(0xFF00 + C), A);
                    cyclesUsed += 4;
                    break;
                case 0xE5:
                    memory.WriteWord(SP, HL);
                    SP -= 2;
                    cyclesUsed += 12;
                    break;
                case 0xEA:
                    // LD (nn), A
                    // Least significant byte first
                    memory.Write(GetImmediateOperandWord(ref cyclesUsed), A);
                    cyclesUsed += 4;
                    break;
                case 0xF0:
                    // LDH A, (0xFF00 + n)
                    A = memory.Read((ushort)(0xFF00 + GetImmediateOperand(ref cyclesUsed)));
                    cyclesUsed += 4;
                    break;
                case 0xF1:
                    // POP AF
                    AF = memory.ReadWord(SP);
                    SP += 2;
                    cyclesUsed += 8;
                    break;
                case 0xF2:
                    // LD A, (0xFF00 + C)
                    A = memory.Read((ushort)(0xFF00 + C));
                    cyclesUsed += 4;
                    break;
                case 0xF5:
                    memory.WriteWord(SP, AF);
                    SP -= 2;
                    cyclesUsed += 12;
                    break;
                case 0xF8:
                    // LD HL, SP+n
                    int lhs = SP;
                    int rhs = (sbyte)GetImmediateOperand(ref cyclesUsed);
                    HL = (ushort)(lhs + rhs);
                    SetCarryFlagsForAdd(lhs, rhs);
                    flagZ = false;
                    flagN = false;
                    cyclesUsed += 4;
                    break;
                case 0xF9:
                    // LD SP, HL
                    SP = HL;
                    cyclesUsed += 4;
                    break;
                case 0xFA:
                    // LD A, (nn)
                    // Least significant byte first
                    A = memory.Read(GetImmediateOperandWord(ref cyclesUsed));
                    cyclesUsed += 4;
                    break;

                default:
                    byte instructionHighNibble = (byte)(instruction >> 4);
                    switch (instructionHighNibble)
                    {
                        case 0x4:
                        case 0x5:
                        case 0x6:
                        case 0x7:
                            // LD *, *
                            {
                                var target = (RegisterEncoding)((instruction >> 3) & 0x7);

                                if (RegisterEncoding.HLderef == target)
                                {
                                    // Confirm that 0x76 isn't a valid instruction, which would be ld (HL), HL
                                    // presumably it does something else
                                    memory.Write(HL, GetLowOperand(instruction, ref cyclesUsed));
                                    cyclesUsed += 4;
                                }
                                else
                                {
                                    int iTargetRegister = RegisterEncodingToIndex(target);
                                    registers[iTargetRegister] = GetLowOperand(instruction, ref cyclesUsed);
                                }
                            }
                            break;

                        case 0x8:
                            // ADD, ADC
                            A = Add(A, GetLowOperand(instruction, ref cyclesUsed), useCarry: (instruction & 0x8) != 0);
                            break;

                        case 0x9:
                            // SUB, SBC
                            A = Sub(A, GetLowOperand(instruction, ref cyclesUsed), useCarry: (instruction & 0x8) != 0);
                            break;

                        case 0xA:
                            if ((instruction & 0x8) != 0)
                            {
                                // XOR
                                A = Xor(A, GetLowOperand(instruction, ref cyclesUsed));
                            }
                            else
                            {
                                // AND
                                A = And(A, GetLowOperand(instruction, ref cyclesUsed));
                            }
                            break;

                        default:

                            byte instructionLowNibble = (byte)(instruction & 0xF);
                            switch (instructionLowNibble)
                            {
                                case 0x6:
                                case 0xE:
                                    if (instructionHighNibble <= 0x3) // LD immediate
                                    {
                                        var target = (RegisterEncoding)((instruction >> 3) & 0x7);
                                        if (RegisterEncoding.HLderef == target)
                                        {
                                            memory.Write(HL, GetImmediateOperand(ref cyclesUsed));
                                            cyclesUsed += 4;
                                        }
                                        else
                                        {
                                            registers[RegisterEncodingToIndex(target)] = GetImmediateOperand(ref cyclesUsed);
                                        }
                                    }
                                    else
                                    {
                                        goto default;
                                    }
                                    break;

                                default:
                                    throw new Exception($"Instruction not implemented: 0x{instruction:X2}");
                            }
                            break;
                    }

                    break;
            }

            // Increment the program counter
            PC++;

            // Increment the clock by the number of cycles used
            clock += cyclesUsed;
        }

        byte GetLowOperand(byte instruction, ref ushort cyclesUsed)
        {
            var lowOperand = (RegisterEncoding)((instruction) & 0x7);
            byte lowOperandValue;
            if (RegisterEncoding.HLderef == lowOperand)
            {
                lowOperandValue = memory.Read(HL);
                cyclesUsed += 4;
            }
            else
            {
                lowOperandValue = registers[RegisterEncodingToIndex(lowOperand)];
            }
            return lowOperandValue;
        }

        byte GetImmediateOperand(ref ushort cyclesUsed)
        {
            cyclesUsed += 4;
            return memory.Read(++PC);;
        }

        ushort GetImmediateOperandWord(ref ushort cyclesUsed)
        {
            cyclesUsed += 8;
            return (ushort)(memory.Read(++PC) | memory.Read(++PC) << 8);
        }

        void SetCarryFlagsForAdd(int lhs, int rhs, bool useCarry = false)
        {
            int carry = useCarry && flagC ? 1 : 0;
            flagH = ((lhs & 0xF) + (rhs & 0xF) + carry) > 0xF;
            flagC = ((lhs & 0xFF) + (rhs & 0xFF) + carry) > 0xFF;
        }

        void SetCarryFlagsForSub(int lhs, int rhs, bool useCarry = false)
        {
            int carry = useCarry && flagC ? 1 : 0;
            flagH = ((lhs & 0xF) - ((rhs & 0xF) + carry)) < 0;
            flagC = ((lhs & 0xFF) - ((rhs & 0xFF) + carry)) < 0;
        }

        byte Add(byte lhs, byte rhs, bool useCarry)
        {
            int carry = useCarry && flagC ? 1 : 0;
            byte result = (byte)(lhs + rhs + carry);
            SetCarryFlagsForAdd(lhs, rhs, useCarry);
            flagN = false;
            flagZ = result == 0;
            return result;
        }

        byte Sub(byte lhs, byte rhs, bool useCarry)
        {
            int carry = useCarry && flagC ? 1 : 0;
            byte result = (byte)(lhs - (rhs + carry));
            SetCarryFlagsForSub(lhs, rhs, useCarry);
            flagN = false;
            flagZ = result == 0;
            return result;
        }

        byte And(byte lhs, byte rhs)
        {
            byte result = (byte)(lhs & rhs);
            flagN = false;
            flagH = true;
            flagC = false;
            flagZ = result == 0;
            return result;
        }

        byte Xor(byte lhs, byte rhs)
        {
            byte result = (byte)(lhs ^ rhs);
            flagN = false;
            flagH = false;
            flagC = false;
            flagZ = result == 0;
            return result;
        }

        byte Or(byte lhs, byte rhs)
        {
            byte result = (byte)(lhs | rhs);
            flagN = false;
            flagH = false;
            flagC = false;
            flagZ = result == 0;
            return result;
        }

        void Cp(byte lhs, byte rhs)
        {
            SetCarryFlagsForSub(lhs, rhs);
            flagN = true;
            flagZ = rhs == lhs;
        }
    }
}