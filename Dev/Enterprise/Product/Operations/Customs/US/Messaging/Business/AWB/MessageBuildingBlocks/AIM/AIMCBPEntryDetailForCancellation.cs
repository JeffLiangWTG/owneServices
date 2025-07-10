namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM
{
	using Enterprise.Messaging.Business.AWB;

	public class AIMCBPEntryDetailForCancellation : AWBMessageBlock
	{
		protected override void SetupSpecialFields()
		{
			AddSpecialField(1, StatusType.Mandatory, CharType.Alpha, ValueType.LineIdentifier, "CED");
			AddSpecialField(2, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.Slant);
			AddSpecialField(3, StatusType.Mandatory, CharType.Numeric, ValueType.Value, "000");
			AddSpecialField(4, StatusType.Mandatory, CharType.Special, ValueType.Special, SpecialChars.CRLF);
		}
	}
}
