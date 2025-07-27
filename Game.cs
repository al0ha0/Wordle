using System.Text.Json;
using System.Linq;
namespace Wordle.Game;

public class Game
{
	private const string URL = $"https://al0ha0.github.io/Wordle/db.json";
	private static Dictionary<string, List<string>>? WordsDictionary = new();
	
	public static async Task Start()
	{
		await Request();
		if (WordsDictionary is null)
		{
			Console.WriteLine("FAILED TO LOAD DATA!!!");
		}
		else
		{
			Loop();
		}

	}
	
	private static string? RandomWord(string? Gamemode)
	{
		Random rand = new();
		if (Gamemode != null)
			return WordsDictionary?[$"{Gamemode}_letter_nouns"][rand.Next(WordsDictionary[$"{Gamemode}_letter_nouns"].Count)];
		return null;
	}
	
	private static void PlayRound(string WORD, string MODE)
{
    int LetterAmt = int.Parse(MODE);
    int MAXGuesses = 8;
    bool correctguess = false;

    string[] CorrectChars = new string[LetterAmt];
    List<string> PresentLetters = new(); // stores letters that are in the word (wrong place)

    for (int i = 0; i < LetterAmt; i++)
        CorrectChars[i] = " ";

    for (int guessCount = 1; guessCount <= MAXGuesses && !correctguess; guessCount++)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Wordle Console Edition");
            Console.Write("Word: ");
            for (int j = 0; j < LetterAmt; j++)
                Console.Write($"[{CorrectChars[j]}]");

            Console.WriteLine();
            if (PresentLetters.Any())
            {
                var distinct = PresentLetters.Select(c => c.ToUpper()).Distinct();
                Console.WriteLine($"{string.Join(" ", distinct)} are also in the word!");
            }

            Console.WriteLine($"Guess {guessCount}/{MAXGuesses}");
            Console.Write("Enter a Word: ");
            string? guess = Console.ReadLine()?.ToLower();

            if (string.IsNullOrWhiteSpace(guess) || guess.Length != LetterAmt)
            {
                Console.WriteLine("Invalid input length.");
                continue;
            }

            if (!IsExistingWord(guess, MODE))
            {
                Console.WriteLine("Invalid Word.");
                continue;
            }

            if (guess == WORD)
            {
                Console.WriteLine("YOU WON!!!");
                correctguess = true;
                break;
            }

            for (int k = 0; k < LetterAmt; k++)
            {
                if (WORD[k] == guess[k])
                {
                    CorrectChars[k] = WORD[k].ToString();
                    Console.WriteLine($"'{CorrectChars[k]}' is correct.");
                }
                else if (WORD.Contains(guess[k]))
                {
                    string ch = guess[k].ToString();
                    if (!PresentLetters.Contains(ch)) PresentLetters.Add(ch);
                    Console.WriteLine($"'{ch}' is in the word, wrong place.");
                }
            }

            Console.WriteLine("Oopsie! You didn't guess the word.");
            Thread.Sleep(1500); // small pause for user feedback
            break;
        }
    }

    if (!correctguess)
        Console.WriteLine($"Game over! The word was: {WORD.ToUpper()}");
}


	private static bool IsExistingWord(string WORD, string MODE) => WordsDictionary[$"{MODE}_letter_nouns"].Contains(WORD);
	
	private static void Loop()
	{
		bool Exit = false;
		while (!Exit)
		{
			Console.WriteLine("Would you like to Play? (y/n)");
			string? Response = Console.ReadLine();
			
				switch (Response)
				{
					case "y":
					case "yes":
						Console.WriteLine("Choose a Gamemode(4/5/6)\n4-Letter\n5-Letter\n6-Letter");
						string? modeInput = Console.ReadLine();
						if (modeInput != null && (modeInput == "4" || modeInput == "5" || modeInput == "6"))
						{
							string? word = RandomWord(modeInput);

							if (word != null)
							{
								Console.WriteLine($"Starting game with {modeInput}-letter word.");
								PlayRound(word, modeInput);
							}
							else
							{
								Console.WriteLine("Sorry, no words available for that gamemode.");
							}
						}
						else
						{
							Console.WriteLine("Invalid gamemode selected.");
						}
						break;
					case "no":
					case "n":
						Exit = true;
						break;
					default:
						Console.WriteLine("Invalid response!");
						break;
				}

		}
	}
	private static async Task Request()
	{
		using HttpClient client = new();
		try
		{
			var response = await client.GetAsync(URL);
			response.EnsureSuccessStatusCode();
			string responseString = await response.Content.ReadAsStringAsync();
			WordsDictionary = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(responseString);
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
		}
	}
}