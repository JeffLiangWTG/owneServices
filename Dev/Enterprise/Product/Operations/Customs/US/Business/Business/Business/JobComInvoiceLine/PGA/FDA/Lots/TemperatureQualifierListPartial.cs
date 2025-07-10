namespace Enterprise.Customs.US.Business
{
	partial class TemperatureQualifierList
	{
		public static bool AreDetailsMandatory(string code)
		{
			return code == Codes.Frozen ||
				code == Codes.Refrigerated ||
				code == Codes.DryIce;
		}
	}
}
