using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainer()
		{
			AssertEquals(lookup.Container, container);
		}

		public void TestContainerModeCanBeTranslatedToWCOCodeList()
		{
			foreach (ZString code in lookup.CO_FCL_LCL_NCT_List.GetAllCodes())
			{
				AssertNotNullOrEmpty(code.TranslateToWCOContainerModeCode());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<CusContainer>();
			lookup = new CusContainerLookups(container);
		}

		CusContainerLookups lookup;
		CusContainer container;
	}
}
