using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefApplicabilityView))]
	public class CusRefApplicabilityViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var applicabilityView = cusRate.FilteredRateApplicabilities.AddNew();
			AssertEquals(Core.Constants.Customs.Universal.DataSetTypes.OWNData, applicabilityView.ZZT_DataSet);
		}

		public void TestRate()
		{
			AssertEquals(cusRate.PK, cusRefApplicabilityView.Rate.PK);
		}

		public void TestZZT_DataSet()
		{
			var applibility = Factory.New<CusRefApplicabilityView>();
			AssertEquals(Core.Constants.Customs.Universal.DataSetTypes.OWNData, applibility.ZZT_DataSet);
			AssertEquals(true, applibility.ZZT_DataSetInfo.ReadOnly);
		}

		protected override bool CanPersistedObjectBeDeleted => false; // Ref BizObjs cannot be deleted; suspending TestSaveAndDeleteBusinessObject before all the related Cus BizObjs have created.
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return cusRefApplicabilityView;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return cusRefApplicabilityView;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, "ADD");
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "RATE0", rateType.PK, isSystem: true);
			Factory.Save();
			cusRate = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "DUMMYFORMULA", dataGrouping: Core.Constants.CountryCodes.Eritrea, isSystem: true);
			Factory.Save();
			var refBo = helper.CreateCusApplicability(cusRate, null, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), "ADD", "1");
			Factory.Save();
			cusRefApplicabilityView = Factory.Load<CusRefApplicabilityView>(refBo.PK);
		}

		RateView cusRate;
		CusRefApplicabilityView cusRefApplicabilityView;
	}
}
