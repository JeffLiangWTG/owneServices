using System;
using System.Collections.Specialized;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Manifest.MessageProcessors
{
	public abstract class TWManifestMessageHelper
	{
		protected TWManifestMessageHelper(AsycudaMessage message)
		{
			Message = message;
		}

		protected readonly AsycudaMessage Message;

		public static TWManifestMessageHelper NewOutgoingHelper(AsycudaMessage message)
		{
			TWManifestMessageHelper result = null;
			if (message.EM_MessageType == MessageTypeList.Codes.FHM)
			{
				result = new N5101HMessageHelper(message);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html elements")]
		public string ToHtml()
		{
			var table = new HtmlTableCreator(new NameValueCollection { { "border", "0" } }) { EnableHTMLEncoding = false };
			WriteTable(table);
			return table.ToHtml();
		}

		protected abstract void WriteTable(HtmlTableCreator table);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html elements")]
		protected void WriteTitle(HtmlTableCreator table, string name)
		{
			var cell = new CellWithFormatting() { CellValue = FormattableString.Invariant($"<span style='font-weight:bold;text-decoration:underline;'>{name}</span>") };
			cell.HtmlAttributes.Add("style", "font-size: 13px;color: #000000;border:0px;margin: 10px;");
			table.WriteRow(cell);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html elements")]
		protected void WriteRow(HtmlTableCreator table, string name, string value)
		{
			var cell = new CellWithFormatting() { CellValue = FormattableString.Invariant($"<span style='text-decoration:underline;'>{name}:</span><span>&nbsp;{value}</span>") };
			cell.HtmlAttributes.Add("style", "font-size: 12px;color: #000000;border:0px;margin: 10px;");
			table.WriteRow(cell);
		}

		protected void WriteRow(HtmlTableCreator table, string name, decimal? value, bool shouldWriteWhenZero = false)
		{
			var shouldWrite = shouldWriteWhenZero || (value.HasValue && value.Value != 0);
			var stringValue = shouldWrite ? (value != null ? value.Value.ToString() : "0") : string.Empty;
			WriteRow(table, name, stringValue);
		}

		protected string GetSubTtile(string name, int value) => FormattableString.Invariant($"{name}-{value}");
	}
}
