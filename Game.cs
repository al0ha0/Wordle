using System.Text.Json;
using Wordle.Errors;

namespace Wordle.Game;

public static class Game
{
    public const int MAX_GUESSES = 8;
    private const string URL = $"https://al0ha0.github.io/Wordle/db.json";

    public static bool IsRunning { get; private set; } = false;
    public static int Gamemode { get; private set; } = 0;

    private static Dictionary<string, List<string>> wordsDictionary = new();
    private static string SecretWord = string.Empty;

    public static async Task Start()
    {
        await Request();

        Loop();
    }

    private static string? RandomWord()
    {
        if (Gamemode == 0) return null;
        string key = $"{Gamemode}_letter_nouns";
        if (!wordsDictionary.TryGetValue(key, out var wordList) || wordList.Count == 0) return null;

        Random rand = new();
        return wordList[rand.Next(wordList.Count)];
    }

    private static void PlayRound()
    {
        bool isCorrectGuess = false;

        char[] correctChars = Enumerable.Repeat(' ', Gamemode).ToArray();
        List<char> presentLetters = new();
        List<string> history = new();

        for (int guessCount = 1; guessCount <= MAX_GUESSES && !isCorrectGuess; guessCount++)
        {
            Console.WriteLine($"\nWordle Console Edition - Guess {guessCount}/{MAX_GUESSES}");
            Console.Write("Word: ");
            foreach (var ch in correctChars)
                Console.Write($"[{ch}]");
            Console.WriteLine();

            if (presentLetters.Count == 0)
                Console.WriteLine($"Also present (wrong place): {string.Join(" ", presentLetters.Distinct().Select(char.ToUpper))}");

            Console.Write("Enter your guess: ");
            string? guess = Console.ReadLine()?.ToLower();

            if (string.IsNullOrWhiteSpace(guess) || guess.Length != Gamemode)
            {
                Console.WriteLine("Invalid input length. Try again.");
                guessCount--;
                continue;
            }

            if (!IsExistingWord(guess))
            {
                Console.WriteLine($"'{guess}' is not a valid word in dictionary.");
                guessCount--;
                continue;
            }

            history.Add(guess);

            if (guess == SecretWord)
            {
                Console.WriteLine($"YOU WON! The word was: {SecretWord.ToUpper()}");
                isCorrectGuess = true;
                break;
            }

            for (int i = 0; i < Gamemode; i++)
            {
                if (SecretWord[i] == guess[i])
                    correctChars[i] = SecretWord[i];
                else if (SecretWord.Contains(guess[i]) && !presentLetters.Contains(guess[i]))
                    presentLetters.Add(guess[i]);
            }

            Console.WriteLine($"Incorrect. Guesses so far:");
            foreach (string g in history)
            {
                string display = "";
                for (int i = 0; i < Gamemode; i++)
                {
                    if (g[i] == SecretWord[i]) display += $"[{char.ToUpper(g[i])}]";
                    else if (SecretWord.Contains(g[i])) display += $"({g[i]})";
                    else display += $" {g[i]} ";
                }
                Console.WriteLine(display);
            }
        }

        if (!isCorrectGuess)
            Console.WriteLine($"Game over! The word was: {SecretWord.ToUpper()}");
    }

    private static bool IsExistingWord(string word)
    {
        string key = $"{Gamemode}_letter_nouns";
        return wordsDictionary.ContainsKey(key) &&
               wordsDictionary[key].Contains(word);
    }

    private static void Loop()
    {
        IsRunning = true;
        while (IsRunning)
        {
            Console.WriteLine("\nWould you like to play? (y/n)");
            string? response = Console.ReadLine()?.Trim().ToLower();

            switch (response)
            {
                case "y":
                case "yes":
                    Console.WriteLine("Choose a Gamemode (4 / 5 / 6 letters):");
                    string? modeInput = Console.ReadLine();

                    if (modeInput == "4" || modeInput == "5" || modeInput == "6")
                    {
                        Gamemode = Convert.ToInt32(modeInput);
                        string? word = RandomWord();
                        if (word != null)
                        {
                            Console.WriteLine($"Starting {modeInput}-letter game...\n");
                            SecretWord = word;
                            Console.WriteLine("The word is: "+SecretWord);
                            PlayRound();
                        }
                        else
                        {
                            SecretWord = string.Empty;
                            Console.WriteLine("No words found for that mode.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid gamemode.");
                    }
                    break;

                case "n":
                case "no":
                    IsRunning = false;
                    break;

                default:
                    Console.WriteLine("Invalid response. Type 'y' or 'n'.");
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
            string json = await response.Content.ReadAsStringAsync();
            wordsDictionary = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? throw new JsonException(
                "Couldn't deserialize Words dictionary"
            );
            if (wordsDictionary.Count == 0) throw new Exception("Words dictionary is empty due to an unknown error");
            Console.WriteLine("Words loaded successfully.");
        }
        catch (Exception ex)
        {
            throw new WordsLoadingErrorException(ex);
        }
    }
}
