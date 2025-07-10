using System;
using System.Linq;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllCharges()
		{
			var allCharges = incoTermAndChargeFactory.GetAllCharges();
			AssertEquals(11, allCharges.Length);
		}

		public override void TestGetCharge()
		{
			CombineAssertions("From Base", () =>
			{
				AssertGetCharge(CustomsChargeTypeList.Codes.Commission, CustomsChargeCodeProvider.Commission);
				AssertEquals("Commission", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.Commission).ParentTypes);
				AssertGetCharge(CustomsChargeTypeList.Codes.DeductionCharge, CustomsChargeCodeProvider.DeductionCharge);
				AssertEquals("Deduction", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.DeductionCharge).ParentTypes);
				AssertGetCharge(CustomsChargeTypeList.Codes.ExWorks, CustomsChargeCodeProvider.ExWorks);
				AssertEquals("ExWorks", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.ExWorks).ParentTypes);
				AssertGetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeCodeProvider.ForeignInlandFreight);
				AssertEquals("ForeignInlandFreight", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight).ParentTypes);
				AssertGetCharge(CustomsChargeTypeList.Codes.LandingCharges, CustomsChargeCodeProvider.LandingCharges);
				AssertEquals("LandingCharges", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.LandingCharges).ParentTypes);
				AssertGetCharge(CustomsChargeTypeList.Codes.OtherCharges, CustomsChargeCodeProvider.OtherCharges);
				AssertEquals("OtherCharges", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OtherCharges).ParentTypes);
				AssertGetCharge(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeCodeProvider.OverseasFreight);
				AssertEquals("OverseasFreight", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight).ParentTypes);
				AssertGetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeCodeProvider.OverseasInsurance);
				AssertEquals("OverseasInsurance", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance).ParentTypes);
				AssertGetCharge(CustomsChargeTypeList.Codes.PackingCost, CustomsChargeCodeProvider.PackingCost);
				AssertEquals("PackingCost", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.PackingCost).ParentTypes);
				AssertGetCharge(CustomsChargeTypeList.Codes.AdditionCharge, CustomsChargeCodeProvider.AdditionCharge);
				AssertEquals("Addition", ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice, incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.AdditionCharge).ParentTypes);
			});
			CombineAssertions("From ZA", () =>
			{
				AssertGetCharge(CustomsChargeTypeList.Codes.Discount, IncoTermAndCustomsChargeFactory.Discount);
				AssertEquals(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, IncoTermAndCustomsChargeFactory.Discount.ParentTypes);
			});
		}

		public void TestImportIncotermAndChargeFactory()
		{
			var importZAFactory = Customs.Common.IncoTermAndCustomsChargeFactory.GetByCountryCode("ZAIMP");
			AssertEquals(12, importZAFactory.GetAllCharges().Length);
			CombineAssertions("From ZA", () =>
			{
				AssertGetCharge(importZAFactory, InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue, IncoTermAndCustomsChargeFactory.IntellectualValue);
				AssertEquals(ChargeParentTypes.InvoiceLine, IncoTermAndCustomsChargeFactory.IntellectualValue.ParentTypes);
				AssertEquals(2, importZAFactory.GetChargeList(ChargeParentTypes.InvoiceLine).Count());
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\ZA\Business\Business\InvoiceCharge\ChargeCode\TestFile\IncoTermAndCustomsChargeConfiguration.csv";

		protected override string GetCountryContext() => Core.Constants.CountryCodes.SouthAfrica;

		protected override Type GetCustomsChargeCodeProviderActualType() => typeof(IncoTermAndCustomsChargeFactory);

		void AssertGetCharge(Common.IncoTermAndCustomsChargeFactory chargeFactory, string chargeType, CustomsChargeCode expectedChargeCode)
		{
			var actualChargeCode = chargeFactory.GetCharge(chargeType);
			AssertEquals("Correct Charge Code should have been retrieved", expectedChargeCode.GetType(), actualChargeCode.GetType());
		}
	}
}
