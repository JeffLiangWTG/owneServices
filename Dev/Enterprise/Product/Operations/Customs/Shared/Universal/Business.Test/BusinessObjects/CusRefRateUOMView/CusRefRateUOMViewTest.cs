using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefRateUOMView))]
	class CusRefRateUOMViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZXG_DataSet()
		{
			var rateUOM = Factory.New<CusRefRateUOMView>();
			AssertEquals("O", rateUOM.ZXG_DataSet);
			Assert(rateUOM.ZXG_DataSetInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject() => UOM;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => UOM;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => UOM;
		//ZZRef RefCusRateUOM is not supported to delete by CusRefRateUOMView, so suspend the delete testing in TestSaveAndDeleteBusinessObject().
		protected override bool CanPersistedObjectBeDeleted => false;
		public override void TestCallsBaseSetDefaultValues() => Assert(true);
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, "ADD");
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "RATE0", rateType.PK, isSystem: true);
			Factory.Save();
			cusRate = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "DUMMYFORMULA", dataGrouping: Core.Constants.CountryCodes.Eritrea, isSystem: true);
			Factory.Save();
		}

		RateView cusRate;
		UniversalReferenceTestDataHelper helper;
		CusRefRateUOMView UOM => uom ?? (uom = helper.CreateRateUOM(cusRate.PK, "ASVX", true));
		CusRefRateUOMView uom;
	}
}
