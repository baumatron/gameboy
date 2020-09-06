using Xunit;
using Xunit.Abstractions;
using GameBoy;
using System;

namespace GameBoyTests
{
    public class CartridgeTests
    {
        private readonly ITestOutputHelper output;

        public CartridgeTests(ITestOutputHelper output)
        {
            this.output = output;
        }
    
        [Fact]
        public void TestLoadingCart()
        {
            var cart = new Cartridge();
            cart.LoadFromFile("../../../test_programs/print_test.rom");

            output.WriteLine($"Loaded cartridge of size: {cart.Rom.Length}");

            byte firstValue = cart.Read(0x0000);
            output.WriteLine($"First value: 0x{firstValue:X}");
            Assert.Equal(0xC9, firstValue);

            byte lastValue = cart.Read(0x1CCB);
            output.WriteLine($"Last value: 0x{lastValue:X}");
            Assert.Equal(0x00, lastValue);

            byte beyondCartRomValue = cart.Read(0x1CCC);
            Assert.Equal(0xFF, beyondCartRomValue);
        }

        [Fact]
        public void TestCartPaging()
        {
            ushort cartSize = 0xC000;
            var cartMemory = new byte[cartSize];

            // Assign the page index to every byte of that page
            for (ushort i = 0x0000; i < cartSize; ++i)
            {
                cartMemory[i] = (byte)(i / 0x4000);
            }

            var cart = new Cartridge();
            cart.LoadFromArray(cartMemory);

            // Do reads for each page
            // Page 0
            for (ushort address = 0x0000; address < 0x4000; ++address)
            {
                var value = cart.Read(address);
                Assert.Equal(0x00, value);
            }

            // Page 1
            for (ushort address = 0x4000; address < 0x8000; ++address)
            {
                var value = cart.Read(address);
                Assert.Equal(0x01, value);
            }

            // Page 2
            cart.SetFirstBankIndex(2);
            for (ushort address = 0x4000; address < 0x8000; ++address)
            {
                var value = cart.Read(address);
                Assert.Equal(0x02, value);
            }

            // Page 3 (beyond the size of the rom)
            cart.SetFirstBankIndex(3);
            for (ushort address = 0x4000; address < 0x8000; ++address)
            {
                var value = cart.Read(address);
                Assert.Equal(0xFF, value);
            }
        }
    }
}