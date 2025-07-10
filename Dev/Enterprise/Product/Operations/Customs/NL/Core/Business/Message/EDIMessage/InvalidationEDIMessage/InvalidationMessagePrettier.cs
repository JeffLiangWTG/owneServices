using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public class InvalidationMessagePrettier : NLEDIMessagePrettier
{
	public InvalidationMessagePrettier(NLEDIMessage invalidationMessage) : base(invalidationMessage)
	{
		this.invalidationMessage = (CancelDeclarationEDIMessage)invalidationMessage;
	}

	#region SuppressResourceStringsCheckRegion
	protected override void GetFormattedCore(ZStringBuilder stringBuilder)
	{
		stringBuilder.Append("<font size='2' face='Courier New'><table style='margin-left:10pt'>");
		AppendTableRow(stringBuilder, ResStrings.InvalidationReason, invalidationMessage.InvalidationReason);
		stringBuilder.Append("</table></font>");
	}
	#endregion

	readonly CancelDeclarationEDIMessage invalidationMessage;

	static class ResStrings
	{
		public static ZString InvalidationReason => Res.GetString("83DD8B2B-8E5A-44CA-8B1E-0F20F136E295", "Invalidation reason");
	}
}
