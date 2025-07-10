using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AdditionalDutiesTariffTypeListTest : TestCaseWithFactory
	{
		public void TestGetList()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			var list = new AdditionalDutiesTariffTypeList(Factory);
			AssertEquals("12A,12B,13A,13B,13C,13D,15A,15B,2P1,2P2,2P3,3P1,3P2,4P1,4P2,4P3,4P4,4P5,4P6,5P1,5P2,5P3,5P4,5P5,6P1,6P2,6P3,6P4",
				string.Join(",", list.OfType<RefCusTariffType>().Select(x => x.ZZI_TariffType).OrderBy(x => x)));
		}
	}
}
