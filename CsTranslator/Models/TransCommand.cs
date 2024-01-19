using CsTranslator.Enums;
using CsTranslator.Helpers;

namespace CsTranslator.Models
{
    public class TransCommand : Command
    {
        private ChatType ExportChatType { get; }
        private Translation Translation { get; set; }

        public TransCommand(string rawString, ChatType exportChatType, ChatType chatType, string name, string message, string commandParams = null) : base(rawString, chatType, name, message, commandParams) 
        {
            ExportChatType = exportChatType;
        }

        public override void Execute()
        {
            Translation = Translator.GetTranslation(Message, LangParam);
            Executed = true;
        }
    }
}
