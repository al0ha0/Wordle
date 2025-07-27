using Wordle.Game;
namespace Wordle.Main;

static class Program
{
	private static async Task Main()
	{
		await Game.Game.Start();
	}
}