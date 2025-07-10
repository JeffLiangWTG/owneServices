using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusRate))]
	public class RefCusRateTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return UniversalReferenceTestDataHelper.CreateInternalRefCusRate(Factory, false, cusTariff.PK, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return UniversalReferenceTestDataHelper.CreateInternalRefCusRate(factory, false, cusTariff.PK, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0");
		}

		protected override bool CanPersistedObjectBeDeleted => false;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, Constants.RateTypes.Duty, "Duty");
			Factory.Save();
			djcRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DJC", dutyRateType.PK);
			cusTariff = UniversalReferenceTestDataHelper.CreateInternalRefCusTariff(Factory, Core.Constants.CountryCodes.Eritrea, helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1").PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			Factory.Save();
		}

		RefCusRateType dutyRateType;
		CusRefRateCodeView djcRateCode;
		RefCusTariff cusTariff;
		UniversalReferenceTestDataHelper helper;
	}
}
