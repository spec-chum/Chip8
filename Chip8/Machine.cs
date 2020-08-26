using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Chip8
{
	internal class Machine
	{
		private readonly Cpu cpu;
		private readonly Memory ram;
		private readonly Display display;
		private readonly Audio audio;
		private readonly string romName;

		// Keyboard
		private readonly List<Keyboard.Key> keyCodes = new List<Keyboard.Key>
		{
			Keyboard.Key.X,
			Keyboard.Key.Num1,
			Keyboard.Key.Num2,
			Keyboard.Key.Num3,
			Keyboard.Key.Q,
			Keyboard.Key.W,
			Keyboard.Key.E,
			Keyboard.Key.A,
			Keyboard.Key.S,
			Keyboard.Key.D,
			Keyboard.Key.Z,
			Keyboard.Key.C,
			Keyboard.Key.Num4,
			Keyboard.Key.R,
			Keyboard.Key.F,
			Keyboard.Key.V
		};

		public Machine(string rom)
		{
			KeysPressed = new bool[16];
			ram = new Memory();
			audio = new Audio();
			display = new Display(ram);
			cpu = new Cpu(display, ram, audio, KeysPressed);

			romName = rom;
		}

		public bool[] KeysPressed { get; set; }

		public void Run()
		{
			var window = new RenderWindow(new VideoMode(1280, 640), "CHIP-8", Styles.Default);
			window.SetFramerateLimit(60);

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

			while (window.IsOpen)
			{
				var fps = 1 / fpsClock.Restart().AsSeconds();

				window.DispatchEvents();

				Console.SetCursorPosition(0, 2);
				Console.WriteLine("FPS: {0}", fps.ToString().PadRight(4));

				for (int i = 0; i < 500 / (int)fps; i++)
				{
					cpu.Decode();
				}

				display.UpdatePixels();
				texture.Update(display.Pixels);
				window.Draw(frameBuffer);

				window.Display();
			}
		}

		private void OnKeyReleased(object sender, KeyEventArgs e)
		{
			var keyPressedIndex = keyCodes.FindIndex(key => key == e.Code);
			if (keyPressedIndex != -1)
			{
				KeysPressed[keyPressedIndex] = false;
			}
		}

		private void OnKeyPressed(object sender, KeyEventArgs e)
		{
			var keyPressedIndex = keyCodes.FindIndex(key => key == e.Code);
			if (keyPressedIndex != -1)
			{
				KeysPressed[keyPressedIndex] = true;
			}
			else
			{
				if (e.Code == Keyboard.Key.Escape)
				{
					((Window)sender).Close();
				}
			}
		}
	}
}