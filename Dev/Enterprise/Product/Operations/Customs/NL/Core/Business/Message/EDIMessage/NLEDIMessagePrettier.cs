using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public class NLEDIMessagePrettier
{
	public NLEDIMessagePrettier(NLEDIMessage ediMessage)
	{
		Argument.NotNull(ediMessage, nameof(ediMessage));
		this.ediMessage = ediMessage;
	}
	protected readonly NLEDIMessage ediMessage;

	public ZString GetFormatted()
	{
		var stringBuilder = new ZStringBuilder();
		GetFormattedCore(stringBuilder);
		return stringBuilder.ToString();
	}

	#region SuppressResourceStringsCheckRegion
	protected virtual void GetFormattedCore(ZStringBuilder stringBuilder)
	{
		stringBuilder.Append("<font size='2' face='Courier New'>");
		stringBuilder.Append(ediMessage.EM_MessageText);
		stringBuilder.Append("</font>");
	}

	protected void AppendTableRow(ZStringBuilder stringBuilder, ZString headingText, ZString valueText, string tableHeaderStyle = "")
	{
		stringBuilder.Append("<tr><td");
		if (!tableHeaderStyle.Equals(ZString.Empty))
		{
			stringBuilder.Append(FormattableString.Invariant($" style='{tableHeaderStyle}'"));
		}
		stringBuilder.Append("><b>");
		if (!headingText.IsEmpty)
		{
			stringBuilder.Append(FormattableString.Invariant($"{headingText}:"));
		}
		stringBuilder.Append("</b></td><td>");
		if (!valueText.IsEmpty)
		{
			stringBuilder.Append(FormattableString.Invariant($"<i>{valueText}</i>"));
		}
		stringBuilder.Append("</td></tr>");
	}
	#endregion
}
