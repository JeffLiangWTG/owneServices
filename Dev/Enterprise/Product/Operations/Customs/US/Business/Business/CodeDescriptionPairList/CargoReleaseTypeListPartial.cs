namespace Enterprise.Customs.US.Business
{
	partial class CargoReleaseTypeList
	{
		public static bool IsACECargoReleaseType(string code)
		{
			return code == Codes.ACE ||
				code == Codes.SE;
		}
	}
}
