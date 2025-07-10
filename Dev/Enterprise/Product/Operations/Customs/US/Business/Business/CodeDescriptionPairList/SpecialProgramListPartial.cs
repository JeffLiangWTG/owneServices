using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	partial class SpecialProgramList
	{
		public static bool IsNAFTASPI(string spiCountry)
		{
			return
				spiCountry == Codes.BSharp ||
				spiCountry == Codes.CSharp ||
				spiCountry == Codes.KSharp ||
				spiCountry == Codes.LSharp;
		}

		public static SpecialProgramList GetCachedList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<SpecialProgramList>();
		}
	}
}
