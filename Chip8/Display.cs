using System;

namespace Chip8
{
	public class Display
	{
		private const uint Ink = 0xffffffff;
		private const uint Paper = 0xff000000;
		private const int ScreenWidth = 64;
		private const int ScreenHeight = 32;

		private readonly Memory ram;
		private readonly byte[] screen;

		public Display(Memory ram)
		{
			this.ram = ram;
			screen = new byte[ScreenWidth * ScreenHeight];
			Pixels = new byte[ScreenWidth * ScreenHeight * 4];
		}

		public byte[] Pixels { get; }

		public byte DrawSprite(int x, int y, int height, ushort dataAddr)
		{
			byte collision = 0;

			for (int dy = 0; dy < height; dy++)
			{
				var bits = ram.Ram[dataAddr + dy];

				for (int dx = 0; dx < 8; dx++)
				{
					int pos = ((y + dy) * ScreenWidth) + (x + dx);
					if ((bits & 0x80) != 0)
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
			Array.Clear(screen, 0, screen.Length);
		}

		public unsafe void UpdatePixels()
		{
			uint pixelColour;

			for (int y = 0; y < ScreenHeight; y++)
			{
				for (int x = 0; x < ScreenWidth; x++)
				{
					int pos = (y * ScreenWidth) + x;

					pixelColour = screen[pos] == 1 ? Ink : Paper;

					fixed (byte* bptr = &Pixels[0])
					{
						uint* pixel = (uint*)bptr;
						pixel[pos] = pixelColour;
					}
				}
			}
		}
	}
}