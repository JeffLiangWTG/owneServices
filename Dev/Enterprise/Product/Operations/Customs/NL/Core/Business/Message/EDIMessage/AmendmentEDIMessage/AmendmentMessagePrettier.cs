using System.Collections.Generic;
using CargoWise.Customs.NL.MessageDefinitions.DMS;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class AmendmentMessagePrettier : NLEDIMessagePrettier
{
	public AmendmentMessagePrettier(NLEDIMessage ediMessage, IReadOnlyList<Change> changes) : base(ediMessage)
	{
		this.changes = changes;
	}
	readonly IReadOnlyList<Change> changes;

	#region SuppressResourceStringsCheckRegion
	protected override void GetFormattedCore(ZStringBuilder stringBuilder)
	{
		if (ediMessage.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			stringBuilder.Append("<font size='2' face='Courier New'>");
			stringBuilder.Append($"<H1>{ResStrings.MainTitle}</H1>");

			stringBuilder.Append("<table style='margin-left:10pt'>");
			AppendTableRow(stringBuilder, "MRN", entryHeader.EntryNumber);
			AppendTableRow(stringBuilder, "Funct. Reference ID", entryHeader.CH_BGMReference);
			stringBuilder.Append("</table>");

			AddChangesToInterpretation(stringBuilder);

			stringBuilder.Append("</font>");
		}
	}

	void AddChangesToInterpretation(ZStringBuilder details)
	{
		if (changes.Count == 0)
		{
			return;
		}

		details.Append($"<H2>{ResStrings.SubTitle}</H2>");
		details.Append("<table style='margin-left:10pt'>");

		foreach (var change in changes)
		{
			AppendTableRow(details, "Name path", change.NamePath);
			AppendTableRow(details, "Old value", change.OldValue);
			AppendTableRow(details, "New value", change.NewValue);
			details.Append("<tr><td colspan='2'>&nbsp;</td></tr>");
		}

		details.Append("</table>");
	}
	#endregion

	static class ResStrings
	{
		public static ZString MainTitle => Res.GetString("A3ED0D4C-A431-4040-B297-6557A2DED59E", "Request to Amend");

		public static ZString SubTitle => Res.GetString("EEED5BC8-26AE-4EFB-87B3-EC4C9ED9A72D", "Changes");
	}
}
