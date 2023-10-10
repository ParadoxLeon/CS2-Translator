using System;

namespace CsTranslator.Exceptions
{
    public class LogfileNotFoundException : TranslatorException
    {
        public LogfileNotFoundException() : base("Logfile not found") { }
    }
}