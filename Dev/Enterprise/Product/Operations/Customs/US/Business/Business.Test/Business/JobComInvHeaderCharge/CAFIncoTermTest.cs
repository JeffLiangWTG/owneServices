using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CAFIncoTermTest : Common.Testing.BaseCFRIncoTermTest
	{
		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("With no charges attached to invoice, ITOT incoterm should be itself", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			var oFT = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			AssertEquals("With OFT, ITOT Incoterm should be FOB", "FOB", IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			oFT.J7_IsIncludedInITOT = true;
			AssertEquals("With OFT in lines, ITOT Incoterm should be itself", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		protected override string CountryContext() => JobDeclaration.USIMPIncoTermAndCharge;

		protected override string IncotermToTest => TermsOfDeliveryList.Codes.CAF;

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge) => base.ExpectedValueForThisChargeRecommeded(charge) && charge.Code != USCustomsChargeTypeList.Codes.OverseasInsurance;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
		{
			switch (charge.Code)
			{
				case CustomsChargeTypeList.Codes.ExWorks:
				case CustomsChargeTypeList.Codes.PackingCost:
				case CustomsChargeTypeList.Codes.ForeignInlandFreight:
				case CustomsChargeTypeList.Codes.OverseasFreight:
				case CustomsChargeTypeList.Codes.AdditionCharge:
					return true;
				default:
					return charge.IsIncoTermNeutral;
			}
		}
	}
}
