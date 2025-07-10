using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExQuayDutyPaidIncoTermTest : Common.Testing.BaseDDPIncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var invoice = CreateInvoice();
			AssertEquals("With no charges attached to invoice, ITOT incoterm should be itself", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			var lCH = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.LandingCharges, 100m, invoice.LocalCurrencyCode);
			var oFT = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 100m, invoice.LocalCurrencyCode);
			var oNS = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasInsurance, 100m, invoice.LocalCurrencyCode);
			AssertEquals("ITOT Incoterm should be FOB", TermsOfDeliveryList.Codes.FOB, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			oFT.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT Incoterm should be CAF", TermsOfDeliveryList.Codes.CAF, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("ITOT Incoterm should be CIF", TermsOfDeliveryList.Codes.CIF, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			lCH.J7_IsIncludedInITOT = true;
			AssertEquals("With LCH in lines, ITOT Incoterm should be itself", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		protected override string CountryContext() => JobDeclaration.USIMPIncoTermAndCharge;

		protected override string IncotermToTest => TermsOfDeliveryList.Codes.EXQ;

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge) => charge.Code == USCustomsChargeTypeList.Codes.OverseasFreight || charge.Code == USCustomsChargeTypeList.Codes.LandingCharges;

		protected override bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode charge) => charge.Code == USCustomsChargeTypeList.Codes.OverseasFreight || charge.Code == USCustomsChargeTypeList.Codes.OverseasInsurance || charge.Code == USCustomsChargeTypeList.Codes.LandingCharges;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge) => true;
	}
}
