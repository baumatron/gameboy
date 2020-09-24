using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace GameBoy
{
    class Compositor
    {
        internal Compositor(IMemory memory)
        {
            _memory = memory;
        }

        internal void RenderScanline()
        {
            byte lcdc = _memory.Read(0xFF40);
            byte yPos = _memory.Read(0xFF44); // TODO: there's also the FF41 register that needs to be updated

            // Determine which tile data will be used
            // The tile data is stored at either 0x8000 or 0x8800.
            // Indexing changes for each. For 0x8000, the index is unsigned. For 0x8800 the index is signed (pattern 0 is at address 0x9000).
            // TODO: Can this be changed by an interrupt while drawing? Maybe need to update this for each line?
            var tileData = (lcdc >> 4) & 1;
            ushort tileDataBaseAddress = (tileData == 0) ? (ushort)0x8800 : (ushort)0x8000;

            // Determine which tile map will be used for the background. This is a 32x32 map of indices into the tile data table
            var bgTileMap = (lcdc >> 3) & 1;
            ushort tileMapBaseAddress = (bgTileMap == 0) ? (ushort)0x9800 : (ushort)0x9C00;

            // Copy all background tiles to render buffer
            //for (ushort y = 0; y < _renderBufferHeight; ++y)
            {
                ushort tileY = (ushort)(yPos / 8);
                ushort tileYCoordinate = (ushort)(yPos % 8);

                // TODO: Allow for scan line interrupt handling
                // TODO: update LY
                for (ushort tileX = 0; tileX < _renderBufferWidth / 8; ++tileX)
                {
                    // TODO: Use SCROLLX/Y
                    // TODO: Allow for proper signed index for mode 0
                    byte tileIndex = _memory.Read((ushort)(bgTileMap + tileX + tileY * 32));
                    // Tile data is arranged as 2 bits per pixel
                    // Read 2 bytes, which is the width of the tile, and write it to the buffer
                    for (ushort i = 0; i < 2; ++i)
                    {
                        _renderBuffer[yPos * _renderBufferWidth + tileX * 8 + i] = _memory.Read((ushort)(tileDataBaseAddress + tileIndex + tileYCoordinate * 2 + i));
                    }
                }
            }
        }

        internal void SaveScreenshot()
        {
            var colorMap = new Dictionary<byte, Color>
            {
                [0x0] = Color.Black,
                [0x1] = Color.DarkGray,
                [0x2] = Color.LightGray,
                [0x3] = Color.White
            };

            using (var bitmap = new Bitmap(_renderBufferWidth, _renderBufferHeight))
            {
                for (int x = 0; x < _renderBufferWidth; x++)
                {
                    for (int y = 0; y < _renderBufferHeight; y++)
                    {
                        ushort index = (ushort)(y * _renderBufferWidth + x);
                        // Each byte represents 4 pixels
                        ushort renderBufferIndex = (ushort)(index / 4);
                        byte packedPixels = _renderBuffer[renderBufferIndex];
                        // Select the pixel we're copying
                        byte selectedPixel = (byte)(index % 4);
                        // TODO: Validate which pixel index is MSB
                        byte pixelBits = (byte)((packedPixels >> selectedPixel * 2) & (byte)0x3);
                        Color pixelColor = colorMap[pixelBits];
                        bitmap.SetPixel(x, y, pixelColor);
                    }
                }

                bitmap.Save("screenshot.png", ImageFormat.Png);
            }
        }

        const ushort _renderBufferHeight = 256;
        const ushort _renderBufferWidth = 256;

        // Render buffer is larger than the displayable region
        // Display region is 160x144 and offset is determined by 
        // SCROLLX and SCROLLY i/o mapped registers
        byte[] _renderBuffer = new byte[256 * 256];

        IMemory _memory;
        byte SCROLLX;
        byte SCROLLY;
    }
}