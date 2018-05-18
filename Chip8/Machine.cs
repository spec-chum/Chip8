using System;
using System.Collections.Generic;
using System.IO;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

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
		private readonly Dictionary<Keyboard.Key, int> keyCodes = new Dictionary<Keyboard.Key, int>
		{
			[Keyboard.Key.X] = 0x0,
			[Keyboard.Key.Num1] = 0x1,
			[Keyboard.Key.Num2] = 0x2,
			[Keyboard.Key.Num3] = 0x3,
			[Keyboard.Key.Q] = 0x4,
			[Keyboard.Key.W] = 0x5,
			[Keyboard.Key.E] = 0x6,
			[Keyboard.Key.A] = 0x7,
			[Keyboard.Key.S] = 0x8,
			[Keyboard.Key.D] = 0x9,
			[Keyboard.Key.Z] = 0xa,
			[Keyboard.Key.C] = 0xb,
			[Keyboard.Key.Num4] = 0xc,
			[Keyboard.Key.R] = 0xd,
			[Keyboard.Key.F] = 0xe,
			[Keyboard.Key.V] = 0xf
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

			window.Closed += (s, e) => window.Close();

			window.KeyPressed += OnKeyPressed;
			window.KeyReleased += OnKeyReleased;

			var texture = new Texture(64, 32);
			var frameBuffer = new Sprite(texture);
			frameBuffer.Scale = new Vector2f(20, 20);

			var program = File.ReadAllBytes(romName);
			Array.Copy(program, 0, ram.Ram, 0x200, program.Length);

			while (window.IsOpen)
			{
				window.DispatchEvents();

				cpu.Decode();
				display.UpdatePixels();
				texture.Update(display.Pixels);
				window.Draw(frameBuffer);

				window.Display();
			}
		}

		private void OnKeyReleased(object sender, KeyEventArgs e)
		{
			if (keyCodes.TryGetValue(e.Code, out int key))
			{
				KeysPressed[key] = false;
			}
		}

		private void OnKeyPressed(object sender, KeyEventArgs e)
		{
			if (keyCodes.TryGetValue(e.Code, out int key))
			{
				KeysPressed[key] = true;
			}
			else
			{
				if (e.Code == Keyboard.Key.Escape)
				{
					var window = (Window)sender;
					window.Close();
				}
			}
		}
	}
}