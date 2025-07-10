using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	partial class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestHumanReadableShortcutName()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.Header.AMA_JobReference = "MAN54321";
			AssertEquals("US Air AMS MAN54321", bill.HumanReadableShortcutName);

			bill.ABL_BillNumber = "HS08152401";
			AssertEquals("US Air AMS MAN54321 - HS08152401", bill.HumanReadableShortcutName);
		}

		public void TestDefaultValues()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(Core.Constants.ShipmentTypes.StandardHouse, bill.ABL_BolType);
			AssertEquals(AsycudaBill.ChildBolCode, bill.Header.MasterBill.ABL_BolType);
		}

		public void TestAsycudaBillValidationForRegularBill()
		{
			var bizObj = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaBillValidationForRegularBill>(bizObj.Validation);
		}

		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestBillStatusDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode, "AMSAD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1 DESC", startDate, endDate);
			Factory.Save();

			var bill1 = Factory.NewWithValidTestData<AsycudaBill>();
			bill1.ABL_BillStatus = "Z1";
			AssertEquals("Z1 DESC", bill1.ABL_BillStatusDescription);
		}

		public void TestResetMessageStatus()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_MessageStatus = ZString.Empty;
			Assert(!bill.IsSent);
			bill.ABL_MessageStatus = EDIMessage.Status.Sent;
			Assert(bill.IsSent);

			bill.ResetMessageStatus();
			AssertEquals("Reset if Sent", ZString.Empty, bill.ABL_MessageStatus);
			Assert(!bill.IsSent);
			Assert(!bill.IsError);

			bill.ABL_MessageStatus = EDIMessage.Status.Error;
			Assert(!bill.IsSent);
			Assert(bill.IsError);
			bill.ResetMessageStatus();
			AssertEquals("Reset if Error", ZString.Empty, bill.ABL_MessageStatus);

			bill.ABL_MessageStatus = EDIMessage.Status.Queued;
			bill.ResetMessageStatus();
			AssertEquals("NOT Reset if other", EDIMessage.Status.Queued, bill.ABL_MessageStatus);
			Assert(!bill.IsSent);
			Assert(!bill.IsError);
		}

		public void TestGoodsValueInUSD()
		{
			var oldReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				SetupExchangeRate(Core.Constants.CurrencyCodes.Australia, 0.75m, ZDateTime.Today);
				SetupExchangeRate(Core.Constants.CurrencyCodes.Australia, 0.5m, ZDateTime.Today.AddDays(-7));
				Factory.Save();

				var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill = manifestHeader.Bills.AddNew();
				bill.ABL_GoodsValue = 12m;

				bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("US Rate matches the GoodsValue", 12m, bill.GoodsValueInUSD);

				bill.ABL_RX_NKGoodsValueCurrency = "XXX";
				AssertEquals("Unknown rate is not converted.", 0m, bill.GoodsValueInUSD);

				bill.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Australia;
				manifestHeader.AMA_E_DEP = ZDateTime.Empty;
				AssertEquals("AUD converted to USD uses todays rate when departure date is not set", 9m, bill.GoodsValueInUSD);

				manifestHeader.AMA_E_DEP = ZDateTime.Today;
				AssertEquals("AUD converted to USD uses todays rate", 9m, bill.GoodsValueInUSD);

				manifestHeader.AMA_E_DEP = ZDateTime.Today.AddDays(-7);
				AssertEquals("AUD converted to USD uses earlier rate", 6m, bill.GoodsValueInUSD);
			}
			finally
			{
				// US rates are Reciprocal, but the EDI test company is not.
				GlbCompany.CurrentCompany.GC_IsReciprocal = oldReciprocal;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		void SetupExchangeRate(string currency, ZDecimal rate, ZDateTime expiryDate)
		{
			var query = new ZQuery(RefExchangeRateSchema.RE_GC, Env.CurrentCompany.PK);
			query.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			query.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, expiryDate.Date.AddDays(1));
			query.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, expiryDate);
			query.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, currency);
			query.OrderBy = RefExchangeRateSchema.RE_StartDate.Name + OrderByClause.Descending;

			var exchRate = Factory.LoadTop1<RefExchangeRate>(query);
			if (exchRate == null)
			{
				exchRate = Factory.New<RefExchangeRate>();
				exchRate.RE_GC = Env.CurrentCompany.PK;
				exchRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchRate.RE_RX_NKExCurrency = currency;
			}

			exchRate.RE_StartDate = expiryDate;
			exchRate.RE_ExpiryDate = expiryDate;
			exchRate.RE_SellRate = rate;
		}
	}
}
