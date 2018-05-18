namespace Chip8
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var machine = new Machine("SI.ch8");
            machine.Run();
        }
    }
}