using SFML.Audio;

namespace Chip8
{
    public class Audio
    {
        private readonly short[] raw;
        private readonly SoundBuffer buffer;
        private readonly Sound sound;

        public Audio()
        {
            raw = new short[44100];

            // Generate square wave for beep
            for (uint i = 0; i < 44100; i++)
            {
                raw[i] = (short)(((i / (44100 / 256) / 2) % 2) == 1 ? 10000 : -10000);
            }

            buffer = new SoundBuffer(raw, 1, 44100);
            sound = new Sound(buffer);
        }

        public void Play()
        {
            sound.Play();
        }

        public void Stop()
        {
            sound.Stop();
        }
    }
}