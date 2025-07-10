using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDPSTDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_PSTDisclaimProgram = PSTProductTypeList.Codes.PS1;
			invoiceLine.JI_Description = "TEST PST DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_PSTIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_PSTDisclaimReason);
			AssertEquals(PSTProductTypeList.Codes.PS1, invoiceLineImported.US_PSTDisclaimProgram);
			AssertEquals("TEST PST DISCLAIMED", invoiceLineImported.JI_Description);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "TEST PST DESC";
			pesticide.US_PSTLabelsSent = true;

			var detailLineOne = pesticide.PesticideLines.AddNew();
			detailLineOne.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLineOne.US_LPCONumber = "NUMBERA";
			detailLineOne.US_NameOfActiveIngredient = "INNREDIENTA";
			detailLineOne.US_ActiveIngredientPercentage = 45.999m;

			var detailLineTwo = pesticide.PesticideLines.AddNew();
			detailLineTwo.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLineTwo.US_LPCONumber = "NUMBERB";
			detailLineTwo.US_NameOfActiveIngredient = "INNREDIENTB";
			detailLineTwo.US_ActiveIngredientPercentage = 54.001m;

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(Carrier.PK, DeclarationImported.JE_OH_ShippingLine);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_PSTIndicator);
			AssertEquals("TEST PST DESC", invoiceLineImported.JI_Description);
			AssertEquals(1, invoiceLineImported.PSTLines.Count);

			CombineAssertions(() =>
			{
				var pesticideImported = invoiceLineImported.PSTLines[0];
				AssertEquals(PSTProductTypeList.Codes.PS3, pesticideImported.US_ProductType);
				AssertEquals(PSTIntendedUseCodesList.Codes._130026, pesticideImported.US_IntendedUseCode);
				AssertEquals("RD", pesticideImported.US_UnregReasonCode);
				AssertEquals("TEST REMARKS TEXT", pesticideImported.US_UnregReasonRemarks);
				AssertEquals("UC-HDO", pesticideImported.US_BrandName);
				AssertEquals("0123456", pesticideImported.US_ProducerEstNo);
				AssertEquals("1234567", pesticideImported.US_ProducerEstNoForeign);
				AssertEquals(123m, pesticideImported.US_NoOfUnit1);
				AssertEquals(ShippingOrPackingingUnitList.Codes.Bag, pesticideImported.US_UQ1);
				AssertEquals(456m, pesticideImported.US_NoOfUnit2);
				AssertEquals(ShippingOrPackingingUnitList.Codes.Bar, pesticideImported.US_UQ2);
				AssertEquals(789m, pesticideImported.US_NetWeight);
				AssertEquals(Core.Constants.Weight.Kilograms, pesticideImported.US_WeightUQ);
				AssertEquals(ShipperOrg.MainAddress.PK, pesticideImported.US_OA_ShipperAddress);
				AssertEquals(LocationOrg.MainAddress.PK, pesticideImported.US_OA_ExaminationLocation);
				AssertEquals(true, pesticideImported.US_PSTLabelsSent);
				AssertEquals(2, pesticideImported.PesticideLines.Count);

				var pesticideLine0Imported = pesticideImported.PesticideLines[0];
				AssertEquals(ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber, pesticideLine0Imported.US_LPCOType);
				AssertEquals("NUMBERA", pesticideLine0Imported.US_LPCONumber);
				AssertEquals("INNREDIENTA", pesticideLine0Imported.US_NameOfActiveIngredient);
				AssertEquals(45.999m, pesticideLine0Imported.US_ActiveIngredientPercentage);

				var pesticideLine1Imported = pesticideImported.PesticideLines[1];
				AssertEquals(ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber, pesticideLine1Imported.US_LPCOType);
				AssertEquals("NUMBERB", pesticideLine1Imported.US_LPCONumber);
				AssertEquals("INNREDIENTB", pesticideLine1Imported.US_NameOfActiveIngredient);
				AssertEquals(54.001m, pesticideLine1Imported.US_ActiveIngredientPercentage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_OH_ShippingLine = Carrier.PK;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			pesticide = invoiceLine.PSTLines.AddNew();
			pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;
			pesticide.US_IntendedUseCode = PSTIntendedUseCodesList.Codes._130026;
			pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.RD;
			pesticide.US_UnregReasonRemarks = "TEST REMARKS TEXT";
			pesticide.US_BrandName = "UC-HDO";
			pesticide.US_ProducerEstNo = "0123456";
			pesticide.US_ProducerEstNoForeign = "1234567";
			pesticide.US_OA_ExaminationLocation = LocationOrg.MainAddress.PK;
			pesticide.US_OA_ShipperAddress = ShipperOrg.MainAddress.PK;
			pesticide.US_NoOfUnit1 = 123m;
			pesticide.US_UQ1 = ShippingOrPackingingUnitList.Codes.Bag;
			pesticide.US_NoOfUnit2 = 456m;
			pesticide.US_UQ2 = ShippingOrPackingingUnitList.Codes.Bar;
			pesticide.US_NetWeight = 789m;
			pesticide.US_WeightUQ = Core.Constants.Weight.Kilograms;
		}
		Pesticide pesticide;

		OrgHeader LocationOrg
		{
			get
			{
				if (locationOrg == null)
				{
					locationOrg = Factory.New<OrgHeader>();
					locationOrg.OH_FullName = "TEST LOCATION";
					locationOrg.OH_Code = "TESTLOCAT";
					locationOrg.MainAddress.OA_Address1 = "TEST LOCATION ADDRESS";
				}

				return locationOrg;
			}
		}
		OrgHeader locationOrg;

		OrgHeader ShipperOrg
		{
			get
			{
				if (shipperOrg == null)
				{
					shipperOrg = Factory.New<OrgHeader>();
					shipperOrg.OH_FullName = "TEST SHIPPER";
					shipperOrg.OH_Code = "TESTSHIP";
					shipperOrg.MainAddress.OA_Address1 = "TEST LOCATION ADDRESS";
				}

				return locationOrg;
			}
		}
		OrgHeader shipperOrg;

		OrgHeader Carrier
		{
			get
			{
				if (carrier == null)
				{
					carrier = Factory.New<OrgHeader>();
					carrier.OH_FullName = "TEST CARRIER";
					carrier.OH_Code = "TESTCAR";
					var address = carrier.MainAddress;
					address.OA_Address1 = "ADDRESS 1";
					address.OA_City = "SYDNEY";
					address.OA_State = "NSW";
					address.OA_PostCode = "2017";
					address.OA_RL_NKRelatedPortCode = "AUSYD";
					address.OA_RN_NKCountryCode = "AU";
				}

				return carrier;
			}
		}
		OrgHeader carrier;
	}
}
