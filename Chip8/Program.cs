namespace Chip8;

internal static class Program
{
	private static void Main()
	{
		var machine = new Machine("4-flags.ch8");
		machine.Run();
	}
}