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

        internal bool interruptsEnabled = true;

        // Instructions tend to encode register operands as follows
        internal enum RegisterEncoding
        {
            B = 0b000,
            C = 0b001,
            D = 0b010,
            E = 0b011,
            H = 0b100,
            L = 0b101,
            HLDeref = 0b110,
            A = 0b111
        }

        // Instructions tend to encode word register operands as follows
        internal enum WordRegisterEncoding
        {
            BC = 0b00,
            DE = 0b01,
            HL = 0b10,
            SP = 0b11
        }

        internal static int RegisterEncodingToIndex(RegisterEncoding encoding) => ((int)encoding + 1) & 0x7; // TODO: No need to add 1, could skip that operation altogether

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
        //       the index, and define a function that works on self.F directly instead
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
        internal IMemory _memory;

        internal Cpu(IMemory memory)
        {
            _memory = memory;
        }

        internal void Init()
        {
            // Set the program counter to it's initial value
            PC = 0x100;
            // Set initial stack pointer value
            SP = 0xFFFE;
        }

        internal void InstructionNotImplemented(byte instruction)
        {
            throw new Exception($"Instruction not implemented: 0x{instruction:X2}");
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
            byte instruction = _memory.Read(this.PC);
            ushort cyclesUsed = 4;
            bool shouldIncrementPc = true;

            switch (instruction)
            {
                case 0x00:
                    // NOP
                    break;
                case 0x08:
                    _memory.WriteWord(GetImmediateOperandWord(ref cyclesUsed), SP);
                    cyclesUsed += 8;
                    break;
                case 0x0A:
                    // LD A, (BC)
                    A = _memory.Read(BC);
                    cyclesUsed += 4;
                    break;
                case 0x20:
                    // JR nz, n
                    // http://z80.info/zip/z80cpu_um.pdf page 287
                    if (flagZ == false)
                    {
                        PC = (ushort)(PC + GetImmediateOperand(ref cyclesUsed));
                        shouldIncrementPc = false;
                    }
                    break;
                case 0x1A:
                    // LD A, (DE)
                    A = _memory.Read(DE);
                    cyclesUsed += 4;
                    break;
                case 0x22:
                    // LDI (HL), A
                    _memory.Write(HL++, A);
                    cyclesUsed += 4;
                    break;
                case 0x28:
                    // JR z, n
                    // http://z80.info/zip/z80cpu_um.pdf page 285
                    if (flagZ == true)
                    {
                        PC = (ushort)(PC + GetImmediateOperand(ref cyclesUsed));
                        shouldIncrementPc = false;
                    }
                    break;
                case 0x2A:
                    // LDI A, (HL)
                    A = _memory.Read(HL++);
                    cyclesUsed += 4;
                    break;
                case 0x32:
                    // LDD (HL), A
                    _memory.Write(HL--, A);
                    cyclesUsed += 4;
                    break;
                case 0x3A:
                    // LDD A, (HL)
                    A = _memory.Read(HL--);
                    cyclesUsed += 4;
                    break;
                case 0xC1:
                    // POP BC
                    BC = _memory.ReadWord(SP);
                    SP += 2;
                    cyclesUsed += 8;
                    break;
                case 0xC3:
                    // JP nn
                    PC = GetImmediateOperandWord(ref cyclesUsed);
                    shouldIncrementPc = false;
                    break;
                case 0xC5:
                    // PUSH BC
                    SP -= 2;
                    _memory.WriteWord(SP, BC);
                    cyclesUsed += 12;
                    break;
                case 0xCD:
                    // CALL nn
                    // push PC onto stack then load immediate word to PC
                    SP += 2;
                    _memory.WriteWord(SP, PC);
                    PC = GetImmediateOperandWord(ref cyclesUsed);
                    shouldIncrementPc = false;
                    break;
                case 0xC6:
                case 0xCE:
                    // ADD A, n
                    // ADC A, n
                    A = Add(A, GetImmediateOperand(ref cyclesUsed), useCarry: (instruction & 0x80) > 0);
                    break;
                case 0xD1:
                    // POP DE
                    DE = _memory.ReadWord(SP);
                    SP += 2;
                    cyclesUsed += 8;
                    break;
                case 0xD5:
                    // PUSH DE
                    SP -= 2;
                    _memory.WriteWord(SP, DE);
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
                    _memory.Write((ushort)(0xFF00 + GetImmediateOperand(ref cyclesUsed)), A);
                    cyclesUsed += 4;
                    break;
                case 0xE1:
                    // POP HL
                    HL = _memory.ReadWord(SP);
                    SP += 2;
                    cyclesUsed += 8;
                    break;
                case 0xE2:
                    // LD (0xFF00 + C), A
                    _memory.Write((ushort)(0xFF00 + C), A);
                    cyclesUsed += 4;
                    break;
                case 0xE5:
                    // PUSH HL
                    SP -= 2;
                    _memory.WriteWord(SP, HL);
                    cyclesUsed += 12;
                    break;
                case 0xE6:
                    // AND A, n
                    A = And(A, GetImmediateOperand(ref cyclesUsed));
                    break;
                case 0xEA:
                    // LD (nn), A
                    // Least significant byte first
                    _memory.Write(GetImmediateOperandWord(ref cyclesUsed), A);
                    cyclesUsed += 4;
                    break;
                case 0xEE:
                    // XOR A, n
                    A = Xor(A, GetImmediateOperand(ref cyclesUsed));
                    break;
                case 0xF0:
                    // LDH A, (0xFF00 + n)
                    A = _memory.Read((ushort)(0xFF00 + GetImmediateOperand(ref cyclesUsed)));
                    cyclesUsed += 4;
                    break;
                case 0xF1:
                    // POP AF
                    AF = _memory.ReadWord(SP);
                    SP += 2;
                    cyclesUsed += 8;
                    break;
                case 0xF2:
                    // LD A, (0xFF00 + C)
                    A = _memory.Read((ushort)(0xFF00 + C));
                    cyclesUsed += 4;
                    break;
                case 0xF3:
                    // DI
                    interruptsEnabled = false;
                    break;
                case 0xF5:
                    // PUSH AF
                    SP -= 2;
                    _memory.WriteWord(SP, AF);
                    cyclesUsed += 12;
                    break;
                case 0xF6:
                    // OR A, n
                    A = Or(A, GetImmediateOperand(ref cyclesUsed));
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
                    A = _memory.Read(GetImmediateOperandWord(ref cyclesUsed));
                    cyclesUsed += 4;
                    break;
                case 0xFB:
                    // EI
                    interruptsEnabled = true;
                    break;
                case 0xFE:
                    // CP A, n
                    Compare(A, GetImmediateOperand(ref cyclesUsed));
                    break;
                default:
                    byte instructionHighNibble = (byte)(instruction >> 4);
                    byte instructionLowNibble = (byte)(instruction & 0xF);
                    switch (instructionHighNibble)
                    {
                        case 0x0:
                        case 0x1:
                        case 0x2:
                        case 0x3:
                            switch (instructionLowNibble) 
                            {
                                case 0x01:
                                    {
                                        // LD RR, nn
                                        var target = (WordRegisterEncoding)(instructionHighNibble & 0x3);
                                        WriteToTargetWordRegister(target, GetImmediateOperandWord(ref cyclesUsed));
                                    }
                                    break;
                                case 0x02:
                                    {
                                        // 0x02
                                        // 0x12
                                        // LD (register), A
                                        if (instructionHighNibble <= 0x1)
                                        {
                                            var target = (WordRegisterEncoding)(instructionHighNibble & 0x3);
                                            _memory.WriteWord(ReadWordRegisterValue(target), A);
                                            cyclesUsed += 4;
                                        }
                                        else
                                        {
                                            InstructionNotImplemented(instruction);
                                        }
                                    }
                                    break;
                                case 0x3:
                                    {
                                        // 0x03
                                        // 0x13
                                        // 0x23
                                        // 0x33
                                        // INC nn (register word)
                                        var target = (WordRegisterEncoding)(instructionHighNibble & 0x3);
                                        WriteToTargetWordRegister(target, (ushort)(ReadWordRegisterValue(target) + 1));
                                        cyclesUsed += 4;
                                    }
                                    break;
                                case 0xB:
                                    {
                                        // 0x0B
                                        // 0x1B
                                        // 0x2B
                                        // 0x3B
                                        // DEC nn (register word)
                                        var target = (WordRegisterEncoding)(instructionHighNibble & 0x3);
                                        WriteToTargetWordRegister(target, (ushort)(ReadWordRegisterValue(target) - 1));
                                        cyclesUsed += 4;
                                    }
                                    break;
                                case 0x4:
                                case 0xC:
                                    {
                                        // INC *
                                        var target = DecodeHighOperand(instruction);
                                        // TODO: Supposedly the INC instruction doesn't affect the C flag, but Add does?
                                        var result = Add(GetOperandValue(target, ref cyclesUsed), 1, false);
                                        WriteToTargetRegister(target, result, ref cyclesUsed);
                                    }
                                    break;
                                case 0x5:
                                case 0xD:
                                    {
                                        // DEC *
                                        var target = DecodeHighOperand(instruction);
                                        // TODO: Supposedly the DEC instruction doesn't affect the C flag, but Sub does?
                                        var result = Sub(GetHighOperandValue(instruction, ref cyclesUsed), 1, false);
                                        WriteToTargetRegister(target, result, ref cyclesUsed);
                                    }
                                    break;

                                case 0x6:
                                case 0xE:
                                    if (instructionHighNibble <= 0x3) // LD immediate
                                    {
                                        var target = (RegisterEncoding)((instruction >> 3) & 0x7);
                                        WriteToTargetRegister(target, GetImmediateOperand(ref cyclesUsed), ref cyclesUsed);
                                    }
                                    else
                                    {
                                        goto default;
                                    }
                                    break;
                                default:
                                    InstructionNotImplemented(instruction);
                                    break;
                            }
                            break;
                        case 0x4:
                        case 0x5:
                        case 0x6:
                        case 0x7:
                            // LD *, *
                            {
                                var target = (RegisterEncoding)((instruction >> 3) & 0x7);
                                WriteToTargetRegister(target, GetLowOperand(instruction, ref cyclesUsed), ref cyclesUsed);
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
                            if ((instruction & 0x8) == 0)
                            {
                                // AND
                                A = And(A, GetLowOperand(instruction, ref cyclesUsed));
                            }
                            else
                            {
                                // XOR
                                A = Xor(A, GetLowOperand(instruction, ref cyclesUsed));
                            }
                            break;

                        case 0xB:
                            if ((instruction & 0x8) == 0)
                            {
                                // OR
                                A = Or(A, GetLowOperand(instruction, ref cyclesUsed));
                            }
                            else
                            {
                                // CP
                                Compare(A, GetLowOperand(instruction, ref cyclesUsed));
                            }
                            break;

                        default:
                            InstructionNotImplemented(instruction);
                            break;
                    }

                    break;
            }

            // Increment the program counter
            if (shouldIncrementPc)
            {
                PC++;
            }

            // Increment the clock by the number of cycles used
            clock += cyclesUsed;
        }

        RegisterEncoding DecodeHighOperand(byte instruction)
        {
            // Gets encoding for "high" operand, which may be a register or (HL) (dereferenced value at HL)
            // instruction: xx yyy zzz
            // Where yyy is the high operand, defined by RegisterEncoding
            return (RegisterEncoding)((instruction >> 3) & 0x7);
        }

        RegisterEncoding DecodeLowOperand(byte instruction)
        {
            // Gets encoding for the "low" operand, which may be a register or (HL) (dereferenced value at HL)
            // instruction: xx yyy zzz
            // Where zzz is the low operand, defined by RegisterEncoding
            return (RegisterEncoding)((instruction) & 0x7);
        }

        byte GetLowOperand(byte instruction, ref ushort cyclesUsed)
        {
            return GetOperandValue(DecodeLowOperand(instruction), ref cyclesUsed);
        }

        byte GetHighOperandValue(byte instruction, ref ushort cyclesUsed)
        {

            return GetOperandValue(DecodeHighOperand(instruction), ref cyclesUsed);
        }

        byte GetOperandValue(RegisterEncoding operand, ref ushort cyclesUsed)
        {
            byte value;
            if (RegisterEncoding.HLDeref == operand)
            {
                value = _memory.Read(HL);
                cyclesUsed += 4;
            }
            else
            {
                value = registers[RegisterEncodingToIndex(operand)];
            }
            return value;
        }

        void WriteToTargetRegister(RegisterEncoding target, byte value, ref ushort cyclesUsed)
        {
            if (RegisterEncoding.HLDeref == target)
            {
                _memory.Write(HL, value);
                cyclesUsed += 4;
            }
            else
            {
                registers[RegisterEncodingToIndex(target)] = value;
            }
        }

        internal void WriteToTargetWordRegister(WordRegisterEncoding target, ushort value)
        {
            // TODO: Could probably change the layout of the registers so that the target binary value could be used as an index
            // to then write to the register array, ensuring endian-ness is correct
            switch (target)
            {
                case WordRegisterEncoding.BC:
                    BC = value;
                    break;
                case WordRegisterEncoding.DE:
                    DE = value;
                    break;
                case WordRegisterEncoding.HL:
                    HL = value;
                    break;
                case WordRegisterEncoding.SP:
                    SP = value;
                    break;
                default:
                    throw new Exception("Invalid target register");
            }
        }

        internal ushort ReadWordRegisterValue(WordRegisterEncoding register)
        {
            // TODO: Could probably change the layout of the registers so that the target binary value could be used as an index
            // to then write to the register array, ensuring endian-ness is correct
            ushort value;
            switch (register)
            {
                case WordRegisterEncoding.BC:
                    value = BC;
                    break;
                case WordRegisterEncoding.DE:
                    value = DE;
                    break;
                case WordRegisterEncoding.HL:
                    value = HL;
                    break;
                case WordRegisterEncoding.SP:
                    value = SP;
                    break;
                default:
                    throw new Exception("Invalid register");
            }
            return value;
        }

        byte GetImmediateOperand(ref ushort cyclesUsed)
        {
            cyclesUsed += 4;
            return _memory.Read(++PC);;
        }

        ushort GetImmediateOperandWord(ref ushort cyclesUsed)
        {
            cyclesUsed += 8;
            return (ushort)(_memory.Read(++PC) | _memory.Read(++PC) << 8);
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
            flagN = true;
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

        void Compare(byte lhs, byte rhs)
        {
            SetCarryFlagsForSub(lhs, rhs);
            flagN = true;
            flagZ = rhs == lhs;
        }
    }
}