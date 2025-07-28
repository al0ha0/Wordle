namespace Wordle.Errors;

public class WordsLoadingErrorException : Exception
{
    public const string DEFAULT_ERROR = "Error while loading the Words dictionary";
    public WordsLoadingErrorException() : base(DEFAULT_ERROR) { }
    public WordsLoadingErrorException(string message) : base(message) { }
    public WordsLoadingErrorException(Exception inner) : base(DEFAULT_ERROR, inner) {}
    public WordsLoadingErrorException(string message, Exception inner) : base(message, inner) { }
}