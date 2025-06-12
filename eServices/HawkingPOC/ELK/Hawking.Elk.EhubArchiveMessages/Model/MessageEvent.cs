using System.Collections.Generic;

namespace Hawking.Elk.EhubArchiveMessages.Model
{
    public class MessageEvent
    {
        public IDictionary<string, string> EventHeaders { get; set; }

        public EhubArchiveMessageLog Message { get; set; }
    }
}
