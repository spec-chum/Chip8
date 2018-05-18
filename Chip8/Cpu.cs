using System;
using System.Collections.Generic;
using System.Timers;

namespace Chip8
{
    public class Cpu
    {
        private Timer delayTimer;

        // Registers
        private byte[] v;                       // registers V0 to VF

        private byte dt;                        // Delay timer
        private byte st;                        // Sound Timer
        private ushort i;                       // Address register
        private ushort pc;                      // Program Counter

        // Halt CPU?
        private bool halt;

        // 4K RAM
        private Memory ram;

        // Stack
        private Stack<ushort> stack;

        // Display
        private Display display;

        // Keys
        private bool[] keysPressed;

        // BEEP!
        private Audio beep;

        Random rnd;

        public Cpu(Display display, Memory ram, Audio audio, bool[] keysPressed)
        {
            v = new byte[16];
            stack = new Stack<ushort>();
            pc = 0x200;
            dt = 0;
            st = 0;

            this.display = display;
            this.ram = ram;
            this.keysPressed = keysPressed;
            beep = audio;

            delayTimer = new Timer(16);
            delayTimer.Elapsed += (s, e) =>
            {
                if (dt > 0)
                {
                    dt--;
                }

                if (st > 0)
                {
                    beep.Play();
                    st--;
                }
                else
                {
                    beep.Stop();
                }
            };
            delayTimer.Start();

            rnd = new Random();
        }

        public void Decode()
        {
            var opcode = ram.ram[pc];
            var param = opcode & 0xf;
            var data = ram.ram[pc + 1];

            // Helpers for when using Vx and Vy to make them easier to read
            byte x = (byte)param;
            byte y = (byte)((data >> 4) & 0xf);

            Console.SetCursorPosition(0, 0);
            Console.WriteLine("PC = {0:X4}", pc);

            // 1000Hz speed (approx - not critical)
            System.Threading.Thread.Sleep(1);

            pc += 2;

            switch (opcode >> 4)
            {
                case 0x0:
                    if (data == 0xe0)
                    {
                        // CLS
                        display.Clear();
                    }
                    else if (data == 0xee)
                    {
                        // RET
                        if (stack.Count > 0)
                        {
                            pc = stack.Pop();
                        }
                    }
                    break;

                case 0x1:     // JP nnn
                    pc = (ushort)(param * 256 + data);
                    break;

                case 0x2:     // CALL nnn
                    if (stack.Count < 15)
                    {
                        stack.Push(pc);
                        pc = (ushort)(param * 256 + data);
                    }
                    break;

                case 0x3:     // SE Vx, byte
                    if (v[x] == data)
                    {
                        pc += 2;        // skip next instruction if equal
                    }
                    break;

                case 0x4:     // SNE Vx, byte
                    if (v[x] != data)
                    {
                        pc += 2;        // skip next instruction if not equal
                    }
                    break;

                case 0x5:     // SE Vx, Vy
                    if (v[x] == v[y])
                    {
                        pc += 2;        // skip next instruction if equal
                    }
                    break;

                case 0x6:     // LD Vx, byte
                    v[x] = data;
                    break;

                case 0x7:     // ADD Vx, byte
                    v[x] += data;
                    break;

                case 0x8:     // Maths ops
                    switch (data & 0xf)
                    {
                        case 0x0:     // LD Vx, Vy
                            v[x] = v[y];
                            break;

                        case 0x1:     // OR Vx, Vy
                            v[x] |= v[y];
                            break;

                        case 0x2:     // AND Vx, Vy
                            v[x] &= v[y];
                            break;

                        case 0x3:     // XOR Vx, Vy
                            v[x] ^= v[y];
                            break;

                        case 0x4:     // ADC Vx, Vy
                            // If result is over 255, we overflowed, so set carry flag, otherwise clear it
                            v[0xf] = (byte)(v[x] + v[y] > 255 ? 1 : 0);
                            v[x] += v[y];
                            break;

                        case 0x5:     // SBC Vx, Vy
                            // If Vx is more or equal Vy, we can't overflow, so set carry flag, otherwise clear it
                            v[0xf] = (byte)(v[x] >= v[y] ? 1 : 0);
                            v[x] -= v[y];
                            break;

                        case 0x6:    // SHR Vx
                            v[0xf] = (byte)(v[x] & 1);
                            v[x] >>= 1;
                            break;

                        case 0x7:   // SUBN Vx, Vy
                            // If Vy is more or equal Vx, we can't overflow, so set carry flag, otherwise clear it
                            v[0xf] = (byte)(v[y] >= v[x] ? 1 : 0);
                            v[x] = (byte)(v[y] - v[x]);
                            break;

                        case 0xe:    // SHL Vx
                            v[0xf] = (byte)((v[x] & 128) != 0 ? 1 : 0);
                            v[x] <<= 1;
                            break;
                    }
                    break;

                case 0x9:     // SNE Vx, Vy
                    if (v[x] != v[y])
                    {
                        pc += 2;    // skip next instruction if not equal
                    }
                    break;

                case 0xa:    // LD I, nnn
                    i = (ushort)(param * 256 + data);
                    break;

                case 0xb:    // JP V0, nnn
                    pc = (ushort)(param * 256 + data + v[0]);
                    break;

                case 0xc:    // RND Vx, byte
                    v[x] = (byte)(rnd.Next(0, 255) & data);
                    break;

                case 0xd:   // DRW Vx, Vy, bytes
                    v[0xf] = display.DrawSprite(v[x], v[y], data & 0xf, i);
                    break;

                case 0xe:   // Keyboard
                    switch (data)
                    {
                        case 0x9e:  // SKP Vx
                            // Skip next instruction if key pressed
                            if (keysPressed[v[x]])
                            {
                                pc += 2;
                            }
                            break;

                        case 0xa1:  // SKNP Vx
                            // Skip next instruction if key not pressed
                            if (!keysPressed[v[x]])
                            {
                                pc += 2;
                            }
                            break;
                    }
                    break;

                case 0xf:   // Misc and keyboard
                    switch (data)
                    {
                        case 0x7:   // LD Vx, DT
                            v[x] = dt;
                            break;

                        case 0xa:   // LD Vx, K
                            if (halt)
                            {
                                for (int k = 0; k < 16; k++)
                                {
                                    if (keysPressed[k])
                                    {
                                        halt = false;
                                        v[x] = (byte)k;
                                        Console.WriteLine("Key {0} pressed.", k);
                                        return;
                                    }
                                }
                            }

                            halt = true;
                            pc -= 2;
                            break;

                        case 0x15:  // LD DT, Vx
                            dt = v[x];
                            break;

                        case 0x18:  // LD ST, Vx
                            st = v[x];
                            break;

                        case 0x1e:  // ADD I, Vx
                            i += v[x];
                            break;

                        case 0x29:  // LD F, Vx
                            i = (ushort)(v[x] * 5);
                            break;

                        case 0x33:  // LD B, Vx
                            ram.ram[i] = (byte)(v[x] / 100);
                            ram.ram[i + 1] = (byte)((v[x] / 10) % 10);
                            ram.ram[i + 2] = (byte)(v[x] % 10);
                            break;

                        case 0x55:  // LD [I], Vx
                            for (int reg = 0; reg <= x; reg++)
                            {
                                ram.ram[i + reg] = v[reg];
                            }
                            break;

                        case 0x65:  // LD Vx, [I]
                            for (int reg = 0; reg <= x; reg++)
                            {
                                v[reg] = ram.ram[i + reg];
                            }
                            break;
                    }
                    break;
            }
        }
    }
}