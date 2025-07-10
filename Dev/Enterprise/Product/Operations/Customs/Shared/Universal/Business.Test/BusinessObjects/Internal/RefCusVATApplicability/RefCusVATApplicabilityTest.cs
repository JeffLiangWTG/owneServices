using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusVATApplicability))]
	class RefCusVATApplicabilityTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Eritrea, tariffTypePK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			return UniversalReferenceTestDataHelper.CreateInternalRefCusVATApplicability(Factory, tariff, Core.Constants.CountryCodes.Eritrea, "VAT", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Eritrea, tariffTypePK, "DUMMYTRF1", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			return UniversalReferenceTestDataHelper.CreateInternalRefCusVATApplicability(factory, tariff, Core.Constants.CountryCodes.Eritrea, "VAD", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
		}

		protected override void SetUp()
		{
			base.SetUp();
			tariffTypePK = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "T1T").PK;
			Factory.Save();
		}

		ZGuid tariffTypePK;
	}
}
