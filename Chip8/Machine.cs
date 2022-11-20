using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Chip8
{
	internal class Machine
	{
		private readonly Cpu cpu;
		private readonly Memory ram;
		private readonly Display display;
		private readonly Audio audio;
		private readonly bool[] KeysPressed;
		private readonly string romName;

		public Machine(string rom)
		{
			KeysPressed = new bool[16];
			ram = new Memory();
			audio = new Audio();
			display = new Display(ram);
			cpu = new Cpu(display, ram, audio, KeysPressed);

			romName = rom;
		}
		public void Run()
		{
			var window = new RenderWindow(new VideoMode(1280, 640), "CHIP-8", Styles.Default);
			window.SetFramerateLimit(60);
			window.SetVerticalSyncEnabled(false);

			window.Closed += (s, e) => window.Close();

			window.KeyPressed += OnKeyPressed;
			window.KeyReleased += OnKeyReleased;

			var texture = new Texture(64, 32);
			var frameBuffer = new Sprite(texture);
			frameBuffer.Scale = new Vector2f(20, 20);

			using (var fs = File.Open(romName, FileMode.Open))
			{
				fs.Read(ram.Ram, 0x200, (int)fs.Length);
			}

			var fpsClock = new Clock();
			var pixels = MemoryMarshal.Cast<byte, uint>(display.Pixels.AsSpan());

			while (window.IsOpen)
			{
				var fps = 1 / fpsClock.Restart().AsSeconds();

				window.DispatchEvents();

				Console.SetCursorPosition(0, 2);
				Console.WriteLine("FPS: {0}", fps.ToString().PadRight(4));

				cpu.CanDrawSprite = true;
				for (int i = 0; i < 500 / 60; i++)
				{
					cpu.Decode();
				}

				display.UpdatePixels(pixels);
				texture.Update(display.Pixels);
				window.Draw(frameBuffer);

				window.Display();
			}
		}

		private static int GetKeyIndex(Keyboard.Key key) => key switch
		{
			Keyboard.Key.X => 0,
			Keyboard.Key.Num1 => 1,
			Keyboard.Key.Num2 => 2,
			Keyboard.Key.Num3 => 3,
			Keyboard.Key.Q => 4,
			Keyboard.Key.W => 5,
			Keyboard.Key.E => 6,
			Keyboard.Key.A => 7,
			Keyboard.Key.S => 8,
			Keyboard.Key.D => 9,
			Keyboard.Key.Z => 10,
			Keyboard.Key.C => 11,
			Keyboard.Key.Num4 => 12,
			Keyboard.Key.R => 13,
			Keyboard.Key.F => 14,
			Keyboard.Key.V => 15,
			_ => -1
		};

		private void OnKeyReleased(object sender, KeyEventArgs e)
		{
			int index = GetKeyIndex(e.Code);
			if (index != -1)
			{
				KeysPressed[index] = false;
			}
		}

		private void OnKeyPressed(object sender, KeyEventArgs e)
		{
			if (e.Code == Keyboard.Key.Escape)
			{
				(sender as Window).Close();
				return;
			}

			int index = GetKeyIndex(e.Code);
			if (index != -1)
			{
				KeysPressed[index] = true;
			}			
		}
	}
}
