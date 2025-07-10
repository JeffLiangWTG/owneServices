namespace Enterprise.Customs.US.Business
{
	public class ItemIdentityNumbersMultipleCode : CommaSeparatedNumber
	{
		public ItemIdentityNumbersMultipleCode()
		{
			NumberInfo.HumanReadableName = "Value";
		}

		public override int NumberMaxLength
		{
			get { return 17; }
		}
	}
}
