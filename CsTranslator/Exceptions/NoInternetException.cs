namespace CsTranslator.Exceptions
{
    public class NoInternetException : TranslatorException
    {
        public NoInternetException() : base("No internet") { }
    }
}