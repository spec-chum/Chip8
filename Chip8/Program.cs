namespace Chip8
{
	internal static class Program
	{
		private static void Main()
		{
			var machine = new Machine("SI.ch8");
			machine.Run();
		}
	}
}