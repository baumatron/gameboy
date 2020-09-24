using System;
using GameBoy;

namespace GameBoy.console
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameboy = new GameBoy();
            gameboy.LoadCartridge("./galaxy.bin");
            // 0x271 - 0x275
            // Reads 0xFF44 - y location
            // compares to 0x91 (dec 145), so waiting for blank period where screen isn't being drawn
            // Then continues
            // Basically looping until it can draw
            bool quit = false;
            bool run = false;
            do 
            {
                if (!run)
                {
                    var instruction = gameboy._cpu._memory.Read(gameboy._cpu.PC);
                    Console.WriteLine($"{gameboy._cpu.PC:X4} {instruction:X2}");
                    Console.WriteLine("s - step, r address - run to address, d - dump registers, f - run, q - quit");
                    var input = Console.ReadLine();
                    if (input.ToLower().StartsWith("s"))
                    {
                        gameboy.Step();
                    }
                    else if (input.ToLower().StartsWith("r "))
                    {
                        var inputs = input.Split(" ");
                        if (inputs.Length != 2)
                        {
                            Console.WriteLine("Invalid input");
                        }
                        int address = 0;
                        if (int.TryParse(inputs[1], System.Globalization.NumberStyles.HexNumber, null, out address))
                        {
                            gameboy.RunToAddress((ushort)address);
                        }
                    }
                    if (input.ToLower().StartsWith("d"))
                    {
                        Console.WriteLine($"AF: {gameboy._cpu.AF:x4}");
                        Console.WriteLine($"BC: {gameboy._cpu.BC:x4}");
                        Console.WriteLine($"DE: {gameboy._cpu.DE:x4}");
                        Console.WriteLine($"HL: {gameboy._cpu.HL:x4}");
                        Console.WriteLine($"SP: {gameboy._cpu.SP:x4}");
                        Console.WriteLine($"PC: {gameboy._cpu.PC:x4}");
                        Console.WriteLine($"Z: {gameboy._cpu.flagZ}");
                        Console.WriteLine($"N: {gameboy._cpu.flagN}");
                        Console.WriteLine($"H: {gameboy._cpu.flagH}");
                        Console.WriteLine($"C: {gameboy._cpu.flagC}");
                    }
                    else if (input.ToLower().StartsWith("q"))
                    {
                        quit = true;
                    }
                    if (input.ToLower().StartsWith("f"))
                    {
                        run = true;
                    }
                }
                else
                {
                    gameboy.Step();
                }
               
            } while (!quit);
        }
    }
}
