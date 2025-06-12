using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	abstract class EnvelopContext
	{
        public string ClientAWB { get; set; }
        public string SenderPIMA { get; set; }
        public string RecipientPIMA { get; set; }
		public string SenderID { get; set; }
		public string RecipientID { get; set; }
		public string MessageType { get; set; }
		public string MessageVersion { get; set; }
		public string InternalMessage { get; set; }
		public string Reference { get; set; }
        public string OriginalMessageType { get; set; }

		public abstract void Populate(IBaseMessage message, PartyResolver partyResolver);

		public void ExtractMessageTypeAndVersion()
		{
			if (String.IsNullOrEmpty(InternalMessage)) throw new ApplicationException(String.Format("Method ExtractMessageTypeAndVersion: InternalMessage is empty"));
			string[] lines = InternalMessage.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			if (lines.Length > 0)
			{
				string header = lines[0];
				string[] parts = header.Split('/');
				if (parts.Length >= 1)
				{
					MessageType = parts[0];

					if (!Regex.IsMatch(MessageType, @"^[a-zA-Z]+$"))
					{
						throw new ApplicationException("MessageType is not correct");
					}
				}

				if (parts.Length >= 2)
				{
					MessageVersion = parts[1];
				}

				var originalMessageType = "";
				int count = 2;
				while(lines.Length > 2 && originalMessageType == "" && count < lines.Length)
				{
					if (lines[count] == string.Empty)
					{
						throw new ApplicationException("Invalid Message content");
					}

					if (lines[count].Substring(0, 1) != "/") originalMessageType = lines[count];
					count += 1;
				}

                if (!String.IsNullOrEmpty(originalMessageType))
                {
                    string[] originalParts = originalMessageType.Split('/');
                    if (originalParts.Length >= 1)
                    {
                        OriginalMessageType = originalParts[0];
                    }
                }
			}
		}

        public string RemoveExtraHeaderFromInternalMessage(string originalInternalMessage)
        {
            var internalMessage = originalInternalMessage;
            if (string.IsNullOrEmpty(originalInternalMessage)) throw new ApplicationException("Method RemoveExtraHeaderFromInternalMessage: InternalMessage is empty");
            if (MessageType == "FMA" || MessageType == "FNA")
            {
                var lines = originalInternalMessage.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                var list = new List<string>();
                list.AddRange(lines);
                var indexOfAckLastLine = -1;
                var index = -1;

                if (list.Count <= 0) return internalMessage;
                for (var i = 0; i < list.Count; i++)
                {
                    var line = list[i];
                    if (line.StartsWith("ACK"))
                    {
                        indexOfAckLastLine = i;
                        while (list[i + 1].StartsWith("/") && i < list.Count)
                        {
                            i++;
                            indexOfAckLastLine = i;
                        }
                    }
                    else if (line.StartsWith("FWB"))
                    {
                        index = list.IndexOf(line);
                    }
                    else if (line.StartsWith("FHL"))
                    {
                        index = list.IndexOf(line);
                    }
                    if (index > -1)
                    {
                        break;
                    }
                }

                if (index - indexOfAckLastLine > 1)
                {
                    var firstIndexOfExtraLines = indexOfAckLastLine + 1;
                    list.RemoveRange(firstIndexOfExtraLines, index - firstIndexOfExtraLines);
                }

                internalMessage = string.Join(Environment.NewLine, list.ToArray());
            }
            return internalMessage;
        }
    }
}
