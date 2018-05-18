namespace Chip8
{
    public class Display
    {
        public Memory ram;

        public int[] screen;
        public byte[] pixels;

        private const uint ink = 0xffffffff;
        private const uint paper = 0x000000ff;
        private const int ScreenWidth = 64;
        private const int ScreenHeight = 32;

        public Display(Memory ram)
        {
            this.ram = ram;
            screen = new int[ScreenWidth * ScreenHeight];
            pixels = new byte[ScreenWidth * ScreenHeight * 4];
        }

        public byte DrawSprite(int x, int y, int height, ushort dataAddr)
        {
            byte collision = 0;

            for (int dy = 0; dy < height; dy++)
            {
                var bits = ram.ram[dataAddr + dy];

                for (int dx = 0; dx < 8; dx++)
                {
                    int pos = ((y + dy) * 64) + (x + dx);
                    if ((bits & 128) != 0)
                    {
                        if (screen[pos] == 1 && collision == 0)
                        {
                            collision = 1;
                        }
                        screen[pos] ^= 1;
                    }
                    bits <<= 1;
                }
            }

            return collision;
        }

        public void Clear()
        {
            for (int i = 0; i < 64 * 32; i++)
            {
                screen[i] = 0;
            }
        }

        public void UpdatePixels()
        {
            uint pixelColour;

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    int pos = y * 64 + x;

                    pixelColour = screen[pos] == 1 ? ink : paper;

                    pos *= 4;
                    pixels[pos + 0] = (byte)((pixelColour >> 8) & 255);
                    pixels[pos + 1] = (byte)((pixelColour >> 16) & 255);
                    pixels[pos + 2] = (byte)((pixelColour >> 24) & 255);
                    pixels[pos + 3] = 0xff;
                }
            }
        }
    }
}