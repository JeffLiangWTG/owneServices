using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Documents.Testing.DocDataObjects.HouseBill
{
	[TestedType(typeof(PaymentBasisCollection))]
	class PaymentBasisCollectionTest : TestCaseWithFactory
	{
		public void TestPaymentBases()
		{
			var billOfLading = CreateBillOfLading();
			var header = billOfLading.Job;
			var lineCharge = CreateLineCharge(header, header.LocalChargesPK, 1000m, "FRT", "AUD");

			var flatBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			flatBasis.PBS_FlatRate = 100m;
			flatBasis.PBS_RX_NKRateCurrency = "AUD";
			flatBasis.PBS_ChargeableDescription = "DESC";
			flatBasis.PBS_AdapterID = "SHP01";
			flatBasis.PBS_JR = lineCharge.PK;
			flatBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.FLT);
			flatBasis.PBS_IsCost = false;

			var perUnitBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			perUnitBasis.PBS_PerUnitRate = 5m;
			perUnitBasis.PBS_RateUnit = "KG";
			perUnitBasis.PBS_ChargeableAmount = 100m;
			perUnitBasis.PBS_ChargeableUnit = "KG";
			perUnitBasis.PBS_RX_NKRateCurrency = "AUD";
			perUnitBasis.PBS_AdapterID = "SHP01";
			perUnitBasis.PBS_RateUnitType = "Weight";
			perUnitBasis.PBS_AdapterType = nameof(AdapterType.Shipment);
			perUnitBasis.PBS_ChargeableUnitType = "Weight";
			perUnitBasis.PBS_JR = lineCharge.PK;
			perUnitBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);
			perUnitBasis.PBS_IsCost = false;

			var percentageBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			percentageBasis.PBS_ChargeableAmount = 1000;
			percentageBasis.PBS_ChargeableUnit = "AUD";
			percentageBasis.PBS_PerUnitRate = 110m;
			percentageBasis.PBS_RateUnit = "100";
			percentageBasis.PBS_RX_NKRateCurrency = "AUD";
			percentageBasis.PBS_AdapterID = "SHP01";
			percentageBasis.PBS_JR = lineCharge.PK;
			percentageBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.PER);
			percentageBasis.PBS_IsCost = false;

			var costBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			costBasis.PBS_ChargeableAmount = 1000;
			costBasis.PBS_ChargeableUnit = "AUD";
			costBasis.PBS_PerUnitRate = 110m;
			costBasis.PBS_RateUnit = "200";
			costBasis.PBS_RX_NKRateCurrency = "AUD";
			costBasis.PBS_AdapterID = "SHP01";
			costBasis.PBS_JR = lineCharge.PK;
			costBasis.PBS_RateReference = nameof(RateInfo.RateInfoType.PER);
			costBasis.PBS_IsCost = true;

			Factory.Save();
			var lineChargeInNewFactory = new BusinessObjectFactory().Load<JobCharge>(lineCharge.PK);
			var paymentBasisCollection = new PaymentBasisCollection(lineChargeInNewFactory);

			AssertEquals(@"Base Rate 100.00 AUD
5.00 AUD/KG
AUD 110%", paymentBasisCollection.CalculationBasis);
			AssertEquals(@"100
1000", paymentBasisCollection.Quantity);
		}

		#region Implementation

		BillOfLading CreateBillOfLading()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "USCHI";
			billOfLading.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			billOfLading.JS_HBLAWBChargesDisplay = "ALL";

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_FullName = "Ziggy Z";
			localClient.OH_RL_NKClosestPort = "AUSYD";
			localClient.MainAddress.Address1 = "Unit 13";
			localClient.MainAddress.Address2 = "4 Lost Lane";
			localClient.MainAddress.City = "Sydney";
			localClient.MainAddress.Postcode = "2000";
			localClient.MainAddress.OA_RN_NKCountryCode = "AU";

			var agentCollect = Factory.New<OrgHeader>();
			agentCollect.OH_FullName = "Airmarine Inc.";
			agentCollect.OH_RL_NKClosestPort = "USCHI";
			agentCollect.MainAddress.Address1 = "5638 S Central Ave";
			agentCollect.MainAddress.City = "Chicago";
			agentCollect.MainAddress.Postcode = "60638";
			agentCollect.MainAddress.OA_RN_NKCountryCode = "US";

			var loader = new JobHeader.Loader(billOfLading);
			var header = loader.TryLoadOrCreate();

			header.LocalChargesPK = localClient.PK;
			header.AgentCollectPK = agentCollect.PK;

			var exRates = (BusinessObjectCollection)header["ExchangeRates"];

			var usdRate = exRates.AddNew();
			usdRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "USD";
			usdRate[JobExRateSchema.Constants.JF_BaseRate] = 1.2;

			var auRate = exRates.AddNew();
			auRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "AUD";
			auRate[JobExRateSchema.Constants.JF_BaseRate] = 1.1;

			return billOfLading;
		}

		JobCharge CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, header.JH_GC);

			var accChargeCode = Factory.LoadTop1<AccChargeCode>(query);

			AssertNotNull($"prerequisite: charge code '{chargeCode}' was found", accChargeCode);

			var lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = header.PK;
			lineCharge.JR_GE = header.JH_GE;
			lineCharge.JR_GB = header.JH_GB;
			lineCharge.JR_AC = accChargeCode.PK;
			lineCharge.JR_OH_SellAccount = sellAccountPK;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;
			lineCharge.JR_OSSellAmt = osSellAmount;
			lineCharge.JR_Desc = accChargeCode.AC_Desc;

			return lineCharge;
		}

		#endregion
	}
}
