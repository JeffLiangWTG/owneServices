using System.Collections.Generic;
using System.Text;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext
{
	public abstract class MessageContext
	{
		readonly Dictionary<string, string> context;
		public string Message { get; private set; }
		public string eHubSenderId { get; private set; }
		public string eHubRecipientId { get; private set; }

		public MessageContext(string message, string eHubSenderId, string eHubRecipientId)
		{
			this.Message = message;
			this.eHubSenderId = eHubSenderId;
			this.eHubRecipientId = eHubRecipientId;

			context = new Dictionary<string, string>();
		}

		public void AddContext(string name, string value)
		{
			if (!context.ContainsKey(name)) context.Add(name, value);
		}

		public string GetValue(string name)
		{
			return context.ContainsKey(name) ? context[name] : string.Empty;
		}

		public abstract void Load();

		public override string ToString()
		{
			var text = new StringBuilder();
			text.Append("Context: ");
			foreach (var key in context.Keys)
			{
				text.AppendFormat("{0}={1}", key, context[key]).AppendLine();
			}

			return text.ToString();
		}
	}
}