using Enterprise.Customs.Common.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	sealed class EXWIncoTermTest : ExWorksIncoTermTest
	{
		protected override string CountryContext()
		{
			return Core.Constants.CountryCodes.Singapore;
		}
	}
}
