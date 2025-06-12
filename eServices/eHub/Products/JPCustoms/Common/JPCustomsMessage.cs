using System;

namespace CargoWise.eHub.Products.JPCustoms.Common
{
	public class JPCustomsMessage
	{
		readonly string message;

		JPCustomsMessage(string message)
		{
			if (string.IsNullOrEmpty(message)) throw new ArgumentNullException("message");
			this.message = message;
		}

		public static JPCustomsMessage Create(string message)
		{
			var jpCustomsMessage = new JPCustomsMessage(message);
			jpCustomsMessage.Parse();
			return jpCustomsMessage;
		}

		void Parse()
		{
			var lines = message.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			if (lines.Length == 1) throw new ArgumentException("Invalid JP Customs message format: message has only 1 line.");
			if (lines[0].Length <= 45) throw new ArgumentException("Invalid JP Customs message format: line 1 has less than 45 symbols");

			ReporterId = lines[0].Substring(29, 8).Trim();
			ProcedureCode = lines[0].Substring(3, 3).Trim();
		}

		public string ReporterId { get; private set; }
		public string ProcedureCode { get; private set; }
	}
}