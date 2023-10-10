using System;
using CsTranslator.Exceptions;

namespace CsTranslator.EventArgs
{
    public class TranslatorExceptionEventArgs : System.EventArgs
    {
        public TranslatorException Exception;
    }
}