using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaTax))]
	public class AsycudaTaxTest : EnterpriseBusinessObjectTestCase
	{
		[TestedType(typeof(AsycudaTax))]
		public class AsycudaTaxBaseTest : CargoWise.EntityFramework.Testing.BusinessObjectBaseTestCase
		{
			protected override BusinessObject GetNewBusinessObject() => Factory.New<AsycudaTax>();
		}

		public void TestSetDefaultValues()
		{
			var tax = (AsycudaTax)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals(tax.AET_MethodOfCalculation, "%");
		}

		public void TestCanDelete()
		{
			var tax = (AsycudaTax)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals("AsycudaTax Can Delete", tax.CanDelete, true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();
			return asycudaTax;
		}

		public void TestIsStampTax()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var asycudaTax = bill.AsycudaTaxes.AddNew();
			asycudaTax.AET_ABL = bill.PK;
			CombineAssertions(() =>
			{
				asycudaTax.AET_ChargeType = TaxCodeList.Codes.CustomsDuty;
				AssertEquals("IsStampTax false", false, asycudaTax.IsStampTax);

				asycudaTax.AET_ChargeType = TaxCodeList.Codes.StampTax;
				AssertEquals("IsStampTax true", true, asycudaTax.IsStampTax);
			});
		}

		public void TestReadonlyWithConditions()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();
			CombineAssertions("Read-Only", () =>
			{
				AssertEquals("Tax Code", false, asycudaTax.AET_ChargeTypeInfo.ReadOnly);
				AssertEquals("Tax Base", true, asycudaTax.AET_BaseValueInfo.ReadOnly);
				AssertEquals("Tax Percent", true, asycudaTax.AET_RateInfo.ReadOnly);
				AssertEquals("Tax Amount", true, asycudaTax.AET_ChargeAmountInfo.ReadOnly);
				AssertEquals("Payment Type", false, asycudaTax.AET_MethodOfPaymentInfo.ReadOnly);
				AssertEquals("Overridden", false, asycudaTax.AET_RateOverrideReasonCodeInfo.ReadOnly);
			});

			asycudaTax.AET_RateOverrideReasonCode = RateOverrideReasonCodeList.Codes.Override;

			CombineAssertions("Read-Only", () =>
			{
				AssertEquals("Tax Code", false, asycudaTax.AET_ChargeTypeInfo.ReadOnly);
				AssertEquals("Tax Base", false, asycudaTax.AET_BaseValueInfo.ReadOnly);
				AssertEquals("Tax Percent", false, asycudaTax.AET_RateInfo.ReadOnly);
				AssertEquals("Tax Amount", false, asycudaTax.AET_ChargeAmountInfo.ReadOnly);
				AssertEquals("Payment Type", false, asycudaTax.AET_MethodOfPaymentInfo.ReadOnly);
				AssertEquals("Overridden", false, asycudaTax.AET_RateOverrideReasonCodeInfo.ReadOnly);
			});
		}

		public void TestLogWhenValuechanged()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();
			asycudaTax.AET_ChargeType = "10";
			asycudaTax.AET_BaseValue = 200;
			asycudaTax.AET_Rate = 20;
			asycudaTax.AET_MethodOfPayment = "CSH";

			Factory.Save();

			asycudaTax.AET_ChargeType = "75";
			asycudaTax.AET_BaseValue = 100;
			asycudaTax.AET_Rate = 10;
			asycudaTax.AET_MethodOfPayment = "PYS";

			Factory.Save();

			AssertEquals(2, asycudaTax.Logs.Find(l => l.SL_SE_NKEvent == Events.EditedARecord.Code).Count());
		}

		public void TestDeleted()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();
			asycudaTax.Delete();
			Assert(asycudaTax.IsDeleted);
		}

		public void TestCalculatePercentage()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			CombineAssertions(() =>
			{
				asycudaTax.AET_BaseValue = 200m;
				asycudaTax.AET_Rate = 15m;

				AssertEquals("AET_ChargeAmount", 30m, asycudaTax.AET_ChargeAmount);
				AssertEquals("AET_BaseValue", 200m, asycudaTax.AET_BaseValue);
				AssertEquals("TAET_Rate", 15m, asycudaTax.AET_Rate);

				asycudaTax.AET_ChargeAmount = 0m;
				asycudaTax.AET_Rate = 10m;

				AssertEquals("AET_ChargeAmount", 20m, asycudaTax.AET_ChargeAmount);
				AssertEquals("AET_BaseValue", 200m, asycudaTax.AET_BaseValue);
				AssertEquals("AET_Rate", 10m, asycudaTax.AET_Rate);

				asycudaTax.AET_BaseValue = 50m;
				asycudaTax.AET_ChargeAmount = 2m;
				asycudaTax.AET_Rate = 4m;

				AssertEquals("AET_ChargeAmount", 2m, asycudaTax.AET_ChargeAmount);
				AssertEquals("AET_BaseValue", 50m, asycudaTax.AET_BaseValue);
				AssertEquals("AET_Rate", 4m, asycudaTax.AET_Rate);

				asycudaTax.AET_BaseValue = ZDecimal.Zero;
				asycudaTax.AET_ChargeAmount = 4m;
				asycudaTax.AET_Rate = 5m;

				AssertEquals("AET_ChargeAmount", ZDecimal.Zero, asycudaTax.AET_ChargeAmount);
			});
		}

		public void TestCalculatePercentageForTRTBan()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var asycudaTax = bill.AsycudaTaxes.AddNew();

			asycudaTax.AET_ChargeType = TaxCodeList.Codes.TRTBandrol;

			asycudaTax.AET_BaseValue = 200m;
			asycudaTax.AET_Rate = 10m;

			CombineAssertions(() =>
			{
				asycudaTax.AET_BaseValue = 200m;
				AssertEquals("AET_ChargeAmount", 2000m, asycudaTax.AET_ChargeAmount);
				AssertEquals("AET_BaseValue", 200m, asycudaTax.AET_BaseValue);
				AssertEquals("TAET_Rate", 10m, asycudaTax.AET_Rate);

				asycudaTax.AET_BaseValue = 50m;
				asycudaTax.AET_ChargeAmount = 200m;
				asycudaTax.AET_Rate = 4m;

				AssertEquals("AET_ChargeAmount", 200m, asycudaTax.AET_ChargeAmount);
				AssertEquals("AET_BaseValue", 50m, asycudaTax.AET_BaseValue);
				AssertEquals("AET_Rate", 4m, asycudaTax.AET_Rate);

				asycudaTax.AET_BaseValue = ZDecimal.Zero;
				asycudaTax.AET_ChargeAmount = 4m;
				asycudaTax.AET_Rate = 5m;

				AssertEquals("AET_ChargeAmount", ZDecimal.Zero, asycudaTax.AET_ChargeAmount);
			});
		}

		public void TestDecimalPlaces()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<DecimalPlacesAttribute>(asycudaTax.GetType(), "AET_ChargeAmount", false, attr => attr.DecimalPlaces == 2);
				AssertHasCustomAttribute<DecimalPlacesAttribute>(asycudaTax.GetType(), "AET_BaseValue", false, attr => attr.DecimalPlaces == 2);
				AssertHasCustomAttribute<DecimalPlacesAttribute>(asycudaTax.GetType(), "AET_Rate", false, attr => attr.DecimalPlaces == 2);
			});
		}

		public void TestAET_TypeDescription()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			AssertEquals("AET_TypeDescription", ZString.Empty, asycudaTax.AET_TypeDescription);
			asycudaTax.AET_ChargeType = TaxCodeList.Codes.StampTax;
			AssertEquals("AET_TypeDescription", TaxCodeList.Descriptions.StampTax, asycudaTax.AET_TypeDescription);
		}

		public void TestAET_RateOverrideReasonCode_Caption()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			AssertEquals("Overridden", DataBoundResourceStrings.GetDataForProperty(asycudaTax.AET_RateOverrideReasonCodeInfo).Caption);
		}

		AsycudaTax SetupTestEnvironmentForAsycudaTax()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.AsycudaTaxes.AddNew();
		}
	}
}
