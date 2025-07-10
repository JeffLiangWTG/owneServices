using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class ConformanceDeclarationCodeList
	{
		public static bool IsDefaultElectronicImageSubmitted(ZString code)
		{
			return code == Codes._7A1 || code == Codes._7A2 || code == Codes._7A4;
		}

		public static bool IsAquacultureFacilityRequired(ZString code)
		{
			return code == Codes._7A1 || code == Codes._7B;
		}

		public static bool IsBox8Mandatory(ZString code)
		{
			return code == Codes._7A1 || code == Codes._7A2 || code == Codes._7A4;
		}
	}
}
