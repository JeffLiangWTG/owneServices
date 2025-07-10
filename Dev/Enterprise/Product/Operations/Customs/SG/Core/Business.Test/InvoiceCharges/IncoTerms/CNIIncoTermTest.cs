using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	sealed class CNIIncoTermTest : IncoTermTest
	{
		public override void TestMissingMandatoryCharges()
		{
			var invoice = Factory.New<TestInvoice>();
			invoice.IncoTerm = IncotermToTest;
			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("One missing charge", 1, result.Length);
		}

		public override void TestITOTIncoterm()
		{
			var invoice = Factory.New<TestInvoice>();
			invoice.IncoTerm = IncotermToTest;
			AssertEquals("With no charges attached to invoice, ITOT incoterm should be itself", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			var oNS = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, invoice.LocalCurrencyCode);
			AssertEquals("With ONS, ITOT Incoterm should be FOB", Core.Constants.IncoTerms.FreeOnBoard, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("With ONS in lines, ITOT Incoterm should be C&I", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		#region Implementation
		protected override string IncotermToTest
		{
			get
			{
				return UnitPriceTermTypeCodeList.Codes.CNI;
			}
		}

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
		{
			return charge.IsIncoTermNeutral || charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance;
		}

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge)
		{
			return charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance || charge.Code == CustomsChargeTypeList.Codes.OverseasFreight;
		}

		protected override bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode charge)
		{
			return charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance;
		}

		protected override string CountryContext()
		{
			return Core.Constants.CountryCodes.Singapore;
		}
		#endregion
	}
}
