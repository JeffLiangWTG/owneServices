using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class FreightInsuranceCurrencyCalculatorTest : TestCaseWithFactory
	{
		public void TestDoInvoicesIncotermsAllowThisChargePartOfInvoice()
		{
			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals("CIF should allow OFT part of invoice", true, testCalculator.DoInvoicesIncotermsAllowThisChargePartOfInvoice(CustomsChargeCodeProvider.OverseasFreight));
			AssertEquals("CIF should allow ONS part of invoice", true, testCalculator.DoInvoicesIncotermsAllowThisChargePartOfInvoice(CustomsChargeCodeProvider.OverseasInsurance));

			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			AssertEquals("CFR/CIF should allow OFT part of invoice", true, testCalculator.DoInvoicesIncotermsAllowThisChargePartOfInvoice(CustomsChargeCodeProvider.OverseasFreight));
			AssertEquals("CFR/CIF should not allow ONS part of invoice", false, testCalculator.DoInvoicesIncotermsAllowThisChargePartOfInvoice(CustomsChargeCodeProvider.OverseasInsurance));

			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertEquals("DDP/CIF should allow OFT part of invoice", true, testCalculator.DoInvoicesIncotermsAllowThisChargePartOfInvoice(CustomsChargeCodeProvider.OverseasFreight));
			AssertEquals("DDP/CIF should allow ONS part of invoice", true, testCalculator.DoInvoicesIncotermsAllowThisChargePartOfInvoice(CustomsChargeCodeProvider.OverseasInsurance));
		}

		public void TestGetCurrencyWhenIncotermsAllowChargeCodeAndOneCurrency()
		{
			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			ZString result = testCalculator.GetDefaultCurrency(CustomsChargeCodeProvider.OverseasFreight);
			AssertEquals("Default currency should be AUD", "AUD", result);
		}

		public void TestGetCurrencyWhenIncotermsDoesNotAllowChargeCodeAndOneCurrencyAir()
		{
			testDec.JE_TransportMode = testDec.TransportModeAirCodeForTesting;

			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			AssertEquals("PreCondition:ONS", false, testCalculator.DoInvoicesIncotermsAllowThisChargePartOfInvoice(CustomsChargeCodeProvider.OverseasInsurance));
			ZString result = testCalculator.GetDefaultCurrency(CustomsChargeCodeProvider.OverseasInsurance);
			AssertEquals("Default currency for ONS should be AUD", "AUD", result);
		}

		public void TestGetCurrencyWhenThereAreMoreThanOneCurrencyAndSEA()
		{
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;

			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = nZDCurrency.RX_Code;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			ZString result = testCalculator.GetDefaultCurrency(CustomsChargeCodeProvider.OverseasFreight);
			AssertEquals("Default currency for OFT should be USD", "USD", result);

			result = testCalculator.GetDefaultCurrency(CustomsChargeCodeProvider.OverseasInsurance);
			AssertEquals("Default currency for ONS should be USD", "USD", result);
		}

		public void TestGetCurrencyForChargesOtherThanOFTAndONSMoreThanOneCurrencies()
		{
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;

			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = nZDCurrency.RX_Code;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			groupHeader.Charges.RemoveAndDeleteAll();
			ZString result = testCalculator.GetDefaultCurrency(CustomsChargeCodeProvider.LandingCharges);
			AssertEquals("Default currency for LCH is incalculable", "", result);

			BaseGroupInvoiceCharge charge = groupHeader.Charges.AddNew();
			charge.J7_RX_NKCurrency = nZDCurrency.RX_Code;

			result = testCalculator.GetDefaultCurrency(CustomsChargeCodeProvider.LandingCharges);
			AssertEquals("Default currency for LCH should be 'NZD'", "NZD", result);
		}

		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader groupHeader;
		GroupChargeCurrencyCalculator testCalculator;

		RefCurrency nZDCurrency;
		RefCurrency aUDCurrency;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = BaseJobDeclaration.New(Factory);
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			testCalculator = new GroupChargeCurrencyCalculator(groupHeader);

			nZDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
		}
	}
}
