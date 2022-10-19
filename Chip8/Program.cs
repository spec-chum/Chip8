namespace Chip8
{
	internal static class Program
	{
		private static void Main()
		{
			var machine = new Machine("chip8-test-suite.ch8");
			machine.Run();
		}
	}
}