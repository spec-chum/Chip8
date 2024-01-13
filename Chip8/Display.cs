using System;

namespace Chip8;

public class Display(Memory ram)
{
	private const uint Ink = 0xffffffff;
	private const uint Paper = 0xff000000;
	private const int ScreenWidth = 64;
	private const int ScreenHeight = 32;

	public readonly byte[] Pixels = new byte[ScreenWidth * ScreenHeight * sizeof(uint)];

	private readonly Memory ram = ram;
	private readonly byte[] screen = new byte[ScreenWidth * ScreenHeight];

	public byte DrawSprite(int x, int y, int height, ushort dataAddr)
	{
		x %= ScreenWidth;
		y %= ScreenHeight;

		int collision = 0;

		for (int dy = y; dy < y + height && dy < ScreenHeight; dy++)
		{
			byte bits = ram.Ram[dataAddr + dy - y];
			int yPos = dy * ScreenWidth;

			for (int dx = x; dx < x + 8 && dx < ScreenWidth; dx++)
			{
				int xyPos = yPos + dx;
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
		Array.Clear(screen);
	}

	public void UpdatePixels(Span<uint> pixels)
	{
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = screen[i] == 1 ? Ink : Paper;
		}
	}
}