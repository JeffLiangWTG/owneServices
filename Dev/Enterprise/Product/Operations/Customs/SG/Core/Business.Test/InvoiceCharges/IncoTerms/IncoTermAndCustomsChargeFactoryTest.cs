namespace Enterprise.Customs.SG.V4.Business.Testing
{
	partial class IncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllIncoTerms()
		{
			var allIncoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			AssertEquals(7, allIncoTerms.Length);
			AssertCollectionContains(UnitPriceTermTypeCodeList.Codes.EXW, allIncoTerms);
			AssertCollectionContains(UnitPriceTermTypeCodeList.Codes.FAS, allIncoTerms);
			AssertCollectionContains(UnitPriceTermTypeCodeList.Codes.FOB, allIncoTerms);
			AssertCollectionContains(UnitPriceTermTypeCodeList.Codes.CFR, allIncoTerms);
			AssertCollectionContains(UnitPriceTermTypeCodeList.Codes.CNI, allIncoTerms);
			AssertCollectionContains(UnitPriceTermTypeCodeList.Codes.CIF, allIncoTerms);
			AssertCollectionContains(IncoTermAndCustomsChargeFactory.ErrorIncoTermCode, allIncoTerms);
		}

		public void TestIsThisChargeRecommendedForThisInvoice()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			CombineAssertions(() =>
			{
				foreach (var messageType in new MessageTypeCodeList().GetAllCodes())
				{
					var expected = !(messageType == MessageTypeCodeList.Codes.INP || messageType == MessageTypeCodeList.Codes.IPT);
					CheckResult(expected, messageType, UnitPriceTermTypeCodeList.Codes.CIF, Common.CustomsChargeTypeList.Codes.OverseasFreight);
					CheckResult(expected, messageType, UnitPriceTermTypeCodeList.Codes.CIF, Common.CustomsChargeTypeList.Codes.OverseasInsurance);
					CheckResult(true, messageType, UnitPriceTermTypeCodeList.Codes.CIF, Common.CustomsChargeTypeList.Codes.OtherCharges);
				}

				foreach (var incoterm in new UnitPriceTermTypeCodeList().GetAllCodes())
				{
					if (incoterm != UnitPriceTermTypeCodeList.Codes.CIF)
					{
						CheckResult(true, MessageTypeCodeList.Codes.INP, incoterm, Common.CustomsChargeTypeList.Codes.OverseasFreight);
						CheckResult(true, MessageTypeCodeList.Codes.INP, incoterm, Common.CustomsChargeTypeList.Codes.OverseasInsurance);
					}
				}
			});

			void CheckResult(bool expected, string messageType, string incoterm, string chargeType)
			{
				declaration.JE_MessageType = messageType;
				Assert($"IsThisChargeRecommendedForThisInvoice should return {(expected ? "true" : "false")} for '{chargeType}' charge when ShipmentType is '{messageType}' and Incoterm is '{incoterm}'", incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, incoterm, chargeType) == expected);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business\InvoiceCharges\IncoTerms\TestFile\IncoTermAndCustomsChargeConfiguration.csv";
		protected override string GetCountryContext()
		{
			return Core.Constants.CountryCodes.Singapore;
		}
	}
}
