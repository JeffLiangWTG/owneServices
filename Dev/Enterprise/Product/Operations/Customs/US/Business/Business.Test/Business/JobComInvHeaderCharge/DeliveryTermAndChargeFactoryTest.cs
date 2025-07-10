using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DeliveryTermAndChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllCharges()
		{
			var chargeCodes = incoTermAndChargeFactory.GetAllCharges();
			AssertEquals(11, chargeCodes.Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(USCustomsChargeTypeList.Codes.Commission, Common.CustomsChargeCodeProvider.Commission);
			AssertGetCharge(USCustomsChargeTypeList.Codes.DeductionCharge, DeliveryTermAndChargeFactory.DeductionCharge);
			AssertGetCharge(USCustomsChargeTypeList.Codes.DisbursementCharge, DeliveryTermAndChargeFactory.DisbursementCharge);
			AssertGetCharge(USCustomsChargeTypeList.Codes.Discount, Common.CustomsChargeCodeProvider.Discount);
			AssertGetCharge(USCustomsChargeTypeList.Codes.ForeignInlandFreight, DeliveryTermAndChargeFactory.ForeignInlandFreight);
			AssertGetCharge(USCustomsChargeTypeList.Codes.LandingCharges, DeliveryTermAndChargeFactory.LandingCharges);
			AssertGetCharge(USCustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeCodeProvider.OtherCharges);
			AssertGetCharge(USCustomsChargeTypeList.Codes.OverseasFreight, DeliveryTermAndChargeFactory.OverseasFreight);
			AssertGetCharge(USCustomsChargeTypeList.Codes.OverseasInsurance, DeliveryTermAndChargeFactory.OverseasInsurance);
			AssertGetCharge(USCustomsChargeTypeList.Codes.PackingCost, Common.CustomsChargeCodeProvider.PackingCost);
			AssertGetCharge(USCustomsChargeTypeList.Codes.AdditionCharge, DeliveryTermAndChargeFactory.AdditionCharge);
		}

		public void TestAdditionCharge()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var headerCharge = invoiceHeader.Charges.AddNew();
			headerCharge.J7_ChargeType = USCustomsChargeTypeList.Codes.AdditionCharge;
			headerCharge.J7_Percentage = 5m;
			var groupCharge = invoiceHeader.GroupCharges.AddNew();
			groupCharge.J7_ChargeType = USCustomsChargeTypeList.Codes.AdditionCharge;
			groupCharge.J7_Percentage = 5m;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.AdditionCharge;
			charge.J7_Percentage = 5m;

			AssertNoErrors(charge.J7_PercentageInfo);
			AssertNoErrors(headerCharge.J7_PercentageInfo);
			AssertNoErrors(groupCharge.J7_PercentageInfo);

			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.Commission;
			AssertNoErrors(charge.J7_PercentageInfo);
			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.LandingCharges;
			AssertHasErrors(charge.J7_PercentageInfo);

			groupCharge.J7_ChargeType = USCustomsChargeTypeList.Codes.Commission;
			AssertNoErrors(groupCharge.J7_PercentageInfo);
			groupCharge.J7_ChargeType = USCustomsChargeTypeList.Codes.LandingCharges;
			AssertHasErrors(groupCharge.J7_PercentageInfo);

			headerCharge.J7_ChargeType = USCustomsChargeTypeList.Codes.Commission;
			AssertNoErrors(headerCharge.J7_PercentageInfo);
			headerCharge.J7_ChargeType = USCustomsChargeTypeList.Codes.LandingCharges;
			AssertHasErrors(headerCharge.J7_PercentageInfo);
		}

		public class CustomsChargeTypeListTest : TestCaseWithFactory
		{
			public void TestOFTandONSDescription()
			{
				USCustomsChargeTypeList customsChargeType = new USCustomsChargeTypeList();
				AssertEquals("International Freight", customsChargeType["OFT"].Description);
				AssertEquals("International Insurance", customsChargeType["ONS"].Description);
			}
		}

		public void TestOFTandONSDescription()
		{
			USCustomsChargeTypeList customsChargeType = new USCustomsChargeTypeList();
			AssertEquals("International Freight", customsChargeType["OFT"].Description);
			AssertEquals("International Insurance", customsChargeType["ONS"].Description);
		}

		public override void TestGetAllIncoTerms()
		{
			var list = new TermsOfDeliveryList();
			var allIncoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			AssertEquals("Length", list.Count + 1, allIncoTerms.Length);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, true, allIncoTerms.Contains(pair.Code));
			}
			AssertEquals(DeliveryTermAndChargeFactory.ErrorIncoTermCode, true, allIncoTerms.Contains(DeliveryTermAndChargeFactory.ErrorIncoTermCode));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\Business\Business.Test\Business\JobComInvHeaderCharge\Testing\IncoTermAndCustomsChargeConfiguration.csv";

		protected override string GetCountryContext() => JobDeclaration.USIMPIncoTermAndCharge;

		protected override Type GetCustomsChargeCodeProviderActualType() => typeof(DeliveryTermAndChargeFactory);
	}
}
