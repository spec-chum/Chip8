namespace Chip8;

internal static class Program
{
	private static void Main()
	{
		var machine = new Machine("si.ch8");
		machine.Run();
	}
}