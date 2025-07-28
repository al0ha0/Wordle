using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace Wordle.Game
{
    public class Game
    {
        private const string Url = $"https://al0ha0.github.io/Wordle/db.json";
        private static Dictionary<string, List<string>>? wordsDictionary = new();

        public static async Task Start()
        {
            await Request();
            if (wordsDictionary is null)
            {
                Console.WriteLine("FAILED TO LOAD DATA!!!");
                return;
            }

            Loop();
        }

        private static string? RandomWord(string? gamemode)
        {
            if (gamemode is null || wordsDictionary is null) return null;
            string key = $"{gamemode}_letter_nouns";
            if (!wordsDictionary.ContainsKey(key) || wordsDictionary[key].Count == 0)
                return null;

            Random rand = new();
            return wordsDictionary[key][rand.Next(wordsDictionary[key].Count)];
        }

        private static void PlayRound(string word, string mode)
        {
            int letterAmt = int.Parse(mode);
            int maxGuesses = 8;
            bool correctGuess = false;

            string[] correctChars = Enumerable.Repeat(" ", letterAmt).ToArray();
            List<string> presentLetters = new();
            List<string> history = new();

            for (int guessCount = 1; guessCount <= maxGuesses && !correctGuess; guessCount++)
            {
                Console.WriteLine($"\nWordle Console Edition - Guess {guessCount}/{maxGuesses}");
                Console.Write("Word: ");
                foreach (var ch in correctChars)
                    Console.Write($"[{ch}]");
                Console.WriteLine();

                if (presentLetters.Any())
                    Console.WriteLine($"Also present (wrong place): {string.Join(" ", presentLetters.Distinct().Select(c => c.ToUpper()))}");

                Console.Write("Enter your guess: ");
                string? guess = Console.ReadLine()?.ToLower();

                if (string.IsNullOrWhiteSpace(guess) || guess.Length != letterAmt)
                {
                    Console.WriteLine("Invalid input length. Try again.");
                    guessCount--;
                    continue;
                }

                if (!IsExistingWord(guess, mode))
                {
                    Console.WriteLine($"'{guess}' is not a valid word in dictionary.");
                    guessCount--;
                    continue;
                }

                history.Add(guess);

                if (guess == word)
                {
                    Console.WriteLine($"YOU WON! The word was: {word.ToUpper()}");
                    correctGuess = true;
                    break;
                }

                for (int i = 0; i < letterAmt; i++)
                {
                    if (word[i] == guess[i])
                        correctChars[i] = word[i].ToString();
                    else if (word.Contains(guess[i]) && !presentLetters.Contains(guess[i].ToString()))
                        presentLetters.Add(guess[i].ToString());
                }

                Console.WriteLine($"Incorrect. Guesses so far:");
                foreach (string g in history)
                {
                    string display = "";
                    for (int i = 0; i < letterAmt; i++)
                    {
                        if (g[i] == word[i]) display += $"[{char.ToUpper(g[i])}]";
                        else if (word.Contains(g[i])) display += $"({g[i]})";
                        else display += $" {g[i]} ";
                    }
                    Console.WriteLine(display);
                }
            }

            if (!correctGuess)
                Console.WriteLine($"Game over! The word was: {word.ToUpper()}");
        }

        private static bool IsExistingWord(string word, string mode)
        {
            string key = $"{mode}_letter_nouns";
            return wordsDictionary != null &&
                   wordsDictionary.ContainsKey(key) &&
                   wordsDictionary[key].Contains(word);
        }

        private static void Loop()
        {
            bool exit = false;
            while (!exit)
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
                            string? word = RandomWord(modeInput);
                            if (word != null)
                            {
                                Console.WriteLine($"Starting {modeInput}-letter game...\n");
                                PlayRound(word, modeInput);
                            }
                            else
                            {
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
                        exit = true;
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
                var response = await client.GetAsync(Url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                wordsDictionary = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                Console.WriteLine("Words loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!Error loading dictionary: {ex.Message}");
            }
        }
    }
}
