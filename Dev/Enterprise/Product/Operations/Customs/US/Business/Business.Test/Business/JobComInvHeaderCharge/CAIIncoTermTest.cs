namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CAIIncoTermTest : Common.Testing.IncoTermTest
	{
		public override void TestMissingMandatoryCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var result = IncoTermAndChargeFactory.MissingMandatoryCharges(IncotermToTest, invoice);
			AssertEquals("Missing charges ", 2, result.Length);
		}

		public override void TestITOTIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("With no charges attached to invoice, ITOT incoterm should be itself", IncotermToTest, IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			var oNS = invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			AssertEquals("With ONS, ITOT Incoterm should be FOB", "FOB", IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
			oNS.J7_IsIncludedInITOT = true;
			AssertEquals("With ONS in lines, ITOT Incoterm should be CAI", "CAI", IncoTermAndChargeFactory.ITOTIncoTerm(IncotermToTest, invoice));
		}

		protected override string CountryContext() => JobDeclaration.USIMPIncoTermAndCharge;

		protected override string IncotermToTest => TermsOfDeliveryList.Codes.CAI;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(Common.ICustomsChargeCode charge)
			=> charge.IsIncoTermNeutral
			|| charge.Code == USCustomsChargeTypeList.Codes.PackingCost
			|| charge.Code == USCustomsChargeTypeList.Codes.ForeignInlandFreight
			|| charge.Code == USCustomsChargeTypeList.Codes.OverseasInsurance;

		protected override bool ExpectedValueForThisChargeRecommeded(Common.ICustomsChargeCode charge) => charge.Code == USCustomsChargeTypeList.Codes.OverseasFreight || charge.Code == USCustomsChargeTypeList.Codes.OverseasInsurance;

		protected override bool ExpectedValueForThisChargeMandatory(Common.ICustomsChargeCode charge) => charge.Code == USCustomsChargeTypeList.Codes.OverseasFreight || charge.Code == USCustomsChargeTypeList.Codes.OverseasInsurance;
	}
}
