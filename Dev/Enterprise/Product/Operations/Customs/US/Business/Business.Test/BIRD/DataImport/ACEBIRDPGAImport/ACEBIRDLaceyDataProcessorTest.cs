using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDLaceyDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.B;
			invoiceLine.JI_Description = "TEST ACE Lacey";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_LaceyIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.B, invoiceLineImported.US_LaceyDisclaimReason);
			AssertEquals("TEST ACE LACEY", invoiceLineImported.JI_Description);
			AssertEquals(0, invoiceLineImported.ACE_FDALines.Count);
		}

		public void TestProcessLaceyMessageBlocksWithUnknownBreakdownTotalTicked()
		{
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);

			invoiceLine.JI_Description = "SOFTWOOD PULPWOOD";

			var laceyActOne = CreateNewLaceyActLine(invoiceLine, "SOFTWOOD PULPWOOD", 30000m, false, true, "EU PINE PULPWOOD", 100m, ACELaceyUnitsOfMeasureList.Codes.Kilogram);
			CreateNewConstituentElement(laceyActOne, "PINUS", "TAEDA");
			CreateNewConstituentElement(laceyActOne, "PINUS", "RIGIDA");
			CreateNewCountry(laceyActOne, Core.Constants.CountryCodes.UnitedKingdom);

			var laceyActTwo = CreateNewLaceyActLine(invoiceLine, "SOFTWOOD PULPWOOD", 10000m, false, true, "EU PINE PULPWOOD", 200m, ACELaceyUnitsOfMeasureList.Codes.Kilogram);
			CreateNewConstituentElement(laceyActTwo, "PINUS", "TAEDA");
			CreateNewCountry(laceyActTwo, Core.Constants.CountryCodes.France);
			CreateNewCountry(laceyActTwo, Core.Constants.CountryCodes.Germany);
			CreateNewCountry(laceyActTwo, Core.Constants.CountryCodes.Belgium);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			AssertEquals(1, DeclarationImported.CusContainers.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals("SOFTWOOD PULPWOOD", invoiceLineImported.JI_Description);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_LaceyIndicator);
			AssertEquals(2, invoiceLineImported.LaceyActLines.Count);

			var laceyActOneImported = invoiceLineImported.LaceyActLines[0];
			AssertEquals("SOFTWOOD PULPWOOD", laceyActOneImported.US_PGACommercialDescription);
			AssertEquals("EU PINE PULPWOOD", laceyActOneImported.US_NameOfConstituentElement);
			AssertEquals(30000m, laceyActOneImported.US_InvCurrPGAValue);
			AssertEquals(100m, laceyActOneImported.US_QuantityOfConstituentElement);
			AssertEquals(ACELaceyUnitsOfMeasureList.Codes.Kilogram, laceyActOneImported.US_UnitOfMeasure);
			AssertEquals(false, laceyActOneImported.US_UnknownBreakdown);
			AssertEquals(true, laceyActOneImported.US_UnknownBreakdownTotal);
			AssertEquals(2, laceyActOneImported.PG04ConstituentElements.Count);
			AssertEquals(1, laceyActOneImported.LaceyCountries.Count);

			var constituentElement0_0 = laceyActOneImported.PG04ConstituentElements[0];
			AssertEquals("PINUS", constituentElement0_0.US_GenusName);
			AssertEquals("TAEDA", constituentElement0_0.US_SpeciesName);
			var constituentElement0_1 = laceyActOneImported.PG04ConstituentElements[1];
			AssertEquals("PINUS", constituentElement0_1.US_GenusName);
			AssertEquals("RIGIDA", constituentElement0_1.US_SpeciesName);

			var laceyCountry0_0 = laceyActOneImported.LaceyCountries[0];
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, laceyCountry0_0.US_CountryCode);

			var laceyActTwoImported = invoiceLineImported.LaceyActLines[1];
			AssertEquals("SOFTWOOD PULPWOOD", laceyActTwoImported.US_PGACommercialDescription);
			AssertEquals("EU PINE PULPWOOD", laceyActTwoImported.US_NameOfConstituentElement);
			AssertEquals(10000m, laceyActTwoImported.US_InvCurrPGAValue);
			AssertEquals(200m, laceyActTwoImported.US_QuantityOfConstituentElement);
			AssertEquals(ACELaceyUnitsOfMeasureList.Codes.Kilogram, laceyActTwoImported.US_UnitOfMeasure);
			AssertEquals(false, laceyActTwoImported.US_UnknownBreakdown);
			AssertEquals(true, laceyActTwoImported.US_UnknownBreakdownTotal);
			AssertEquals(1, laceyActTwoImported.PG04ConstituentElements.Count);
			AssertEquals(3, laceyActTwoImported.LaceyCountries.Count);

			var constituentElement1_0 = laceyActTwoImported.PG04ConstituentElements[0];
			AssertEquals("PINUS", constituentElement1_0.US_GenusName);
			AssertEquals("TAEDA", constituentElement1_0.US_SpeciesName);

			var laceyCountry1_0 = laceyActTwoImported.LaceyCountries[0];
			AssertEquals(Core.Constants.CountryCodes.France, laceyCountry1_0.US_CountryCode);
			var laceyCountry1_1 = laceyActTwoImported.LaceyCountries[1];
			AssertEquals(Core.Constants.CountryCodes.Germany, laceyCountry1_1.US_CountryCode);
			var laceyCountry1_2 = laceyActTwoImported.LaceyCountries[2];
			AssertEquals(Core.Constants.CountryCodes.Belgium, laceyCountry1_2.US_CountryCode);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101475", true);

			invoiceLine.JI_Description = "USED RAILROAD TIES";

			var laceyAct = CreateNewLaceyActLine(invoiceLine, "USED MAPLE AND OAK RAILROAD TIES", 10000m, true);
			CreateNewConstituentElement(laceyAct, "ACER", "SACCHARUM", "MAPLE TIES", 100m, ACELaceyUnitsOfMeasureList.Codes.Kilogram, Core.Constants.CountryCodes.Canada);
			CreateNewConstituentElement(laceyAct, "QUERCUS", "ALBA", "OAK TIES", 500m, ACELaceyUnitsOfMeasureList.Codes.Kilogram, Core.Constants.CountryCodes.UnitedStates);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			AssertEquals(2, DeclarationImported.CusContainers.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals("USED RAILROAD TIES", invoiceLineImported.JI_Description);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_LaceyIndicator);
			AssertEquals(1, invoiceLineImported.LaceyActLines.Count);

			CombineAssertions(() =>
			{
				var laceyActImported = invoiceLineImported.LaceyActLines[0];
				AssertEquals("USED MAPLE AND OAK RAILROAD TIES", laceyActImported.US_PGACommercialDescription);
				AssertEquals(10000m, laceyActImported.US_InvCurrPGAValue);
				AssertEquals(true, laceyActImported.US_UnknownBreakdown);
				AssertEquals(false, laceyActImported.US_UnknownBreakdownTotal);
				AssertEquals(2, laceyActImported.PG04ConstituentElements.Count);

				var constituentElement0 = laceyActImported.PG04ConstituentElements[0];
				AssertEquals("MAPLE TIES", constituentElement0.US_PGANameOfTheConstituentElement);
				AssertEquals(100m, constituentElement0.US_PGAQuantityOfConstituentElement);
				AssertEquals(ACELaceyUnitsOfMeasureList.Codes.Kilogram, constituentElement0.US_PGAUnitOfMeasure);
				AssertEquals("ACER", constituentElement0.US_GenusName);
				AssertEquals("SACCHARUM", constituentElement0.US_SpeciesName);
				AssertEquals(Core.Constants.CountryCodes.Canada, constituentElement0.US_UnknownBreakdownCountryCode);

				var constituentElement1 = laceyActImported.PG04ConstituentElements[1];
				AssertEquals("OAK TIES", constituentElement1.US_PGANameOfTheConstituentElement);
				AssertEquals(500m, constituentElement1.US_PGAQuantityOfConstituentElement);
				AssertEquals(ACELaceyUnitsOfMeasureList.Codes.Kilogram, constituentElement1.US_PGAUnitOfMeasure);
				AssertEquals("QUERCUS", constituentElement1.US_GenusName);
				AssertEquals("ALBA", constituentElement1.US_SpeciesName);
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, constituentElement1.US_UnknownBreakdownCountryCode);

				var cusContainer0 = DeclarationImported.CusContainers[0];
				AssertEquals("WHXU4101474", cusContainer0.CO_ContainerNumber);
				var cusContainer1 = DeclarationImported.CusContainers[1];
				AssertEquals("WHXU4101475", cusContainer1.CO_ContainerNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
		}

		PGA CreateNewLaceyActLine(JobComInvoiceLine invoiceLine, ZString commercialDescription, ZDecimal lineValue, bool unknowBreakdown = false, bool unknownBreakdownTotal = false, string nameOfConstituent = "", decimal quantity = 0m, string unitOfMeasure = "")
		{
			var result = invoiceLine.LaceyActLines.AddNew();
			result.US_PGACommercialDescription = commercialDescription;
			result.US_InvCurrPGAValue = lineValue;
			result.US_UnknownBreakdown = unknowBreakdown;
			result.US_UnknownBreakdownTotal = unknownBreakdownTotal;
			result.US_NameOfConstituentElement = nameOfConstituent;
			result.US_QuantityOfConstituentElement = quantity;
			result.US_UnitOfMeasure = unitOfMeasure;

			return result;
		}

		ConstituentElement CreateNewConstituentElement(PGA pga, ZString genusName, ZString speciesName, string name = "", decimal quantity = 0m, string unitOfMeasure = "", string countryCode = "")
		{
			var result = pga.PG04ConstituentElements.AddNew();
			result.US_PGANameOfTheConstituentElement = name;
			result.US_PGAQuantityOfConstituentElement = quantity;
			result.US_PGAUnitOfMeasure = unitOfMeasure;
			result.US_GenusName = genusName;
			result.US_SpeciesName = speciesName;
			result.US_UnknownBreakdownCountryCode = countryCode;

			return result;
		}

		LaceyCountry CreateNewCountry(PGA pga, ZString countryCode)
		{
			var result = pga.LaceyCountries.AddNew();
			result.US_CountryCode = countryCode;

			return result;
		}
	}
}
