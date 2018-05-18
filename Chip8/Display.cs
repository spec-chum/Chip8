using System;

namespace Chip8
{
	public class Display
	{
		private const uint Ink = 0xffffffff;
		private const uint Paper = 0x000000ff;
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

		public byte[] Pixels { get; set; }

		public byte DrawSprite(int x, int y, int height, ushort dataAddr)
		{
			byte collision = 0;

			for (int dy = 0; dy < height; dy++)
			{
				var bits = ram.Ram[dataAddr + dy];

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
			Array.Clear(screen, 0, 2048);
		}

		public void UpdatePixels()
		{
			uint pixelColour;

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 64; x++)
				{
					int pos = (y * 64) + x;

					pixelColour = screen[pos] == 1 ? Ink : Paper;

					pos *= 4;
					Pixels[pos + 0] = (byte)((pixelColour >> 8) & 255);
					Pixels[pos + 1] = (byte)((pixelColour >> 16) & 255);
					Pixels[pos + 2] = (byte)((pixelColour >> 24) & 255);
					Pixels[pos + 3] = 0xff;
				}
			}
		}
	}
}