using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
			Pixels = new byte[ScreenWidth * ScreenHeight * sizeof(uint)];
		}

		public byte[] Pixels { get; }

		public byte DrawSprite(int x, int y, int height, ushort dataAddr)
		{
			x &= 63;
			y &= 31;

			var collision = 0;

			for (int dy = y; dy < y + height && dy < ScreenHeight; dy++)
			{
				var bits = ram.Ram[dataAddr + dy - y];
				var yPos = dy * ScreenWidth;

				for (int dx = x; dx < x + 8 && dx < ScreenWidth; dx++)
				{
					var xyPos = yPos + dx;
					if ((bits & 0x80) != 0)
					{
						if (screen[xyPos] == 1)
						{
							collision = 1;
						}

						screen[xyPos] ^= 1;
					}

					bits <<= 1;
				}
			}

			return (byte)collision;
		}

		public void Clear()
		{
			Array.Clear(screen, 0, screen.Length);
		}

		public void UpdatePixels(Span<uint> pixels)
		{
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = screen[i] == 1 ? Ink : Paper;
			}
		}
	}
}