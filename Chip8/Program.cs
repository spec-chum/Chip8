namespace Chip8
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var Chip8Machine = new Machine();

            Chip8Machine.Run();
        }
    }
}