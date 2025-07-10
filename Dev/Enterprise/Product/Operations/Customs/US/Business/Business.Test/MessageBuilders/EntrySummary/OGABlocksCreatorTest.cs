using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class OGABlocksCreatorTest : TestCaseWithFactory
	{
		public void TestDOTTariff_()
		{
			invoiceLine.JI_Tariff = "8706005000";
			invoiceLine.ImportTariff.UE_OGACodes = "DT1";
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();

			helper.MessageMustNotContainElement<OGADT01>(message);

			invoiceLine.JI_Tariff = "8703230042";
			invoiceLine.ImportTariff.UE_OGACodes = "DT2";
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;

			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2A;
			dot.US_DOTTireBrandName = "TRADE NAME";
			dot.US_DOTPassport = "124HJ";
			dot.US_DOTCountryOfOrigin = "AU";
			dot.US_DOTBondSuretyCode = "111";
			dot.US_DOTPriorApproval = true;
			dot.US_DOTImpSubstStatement = true;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTTireID = "ZZZ";
			dot.US_DOTCommercialDesc = "DESCRIPTION1";
			DOTVIN dotvin = dot.DOTVINs.AddNew();//empty line

			dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._08;
			dot.US_DOTCommercialDesc = "DESCRIPTION2";

			dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "MAKE1";

			dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "MAKE2";

			message = builder.PopulateMessage();

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			AssertEquals("OI        DESCRIPTION1                                                          ", block.MessageBlocks[0].Serialise());
			AssertEquals("DT010012AY124HJ              AU111YYVZZZTRADE NAME                              ", block.MessageBlocks[1].Serialise());
			AssertEquals("OI        DESCRIPTION2                                                          ", block.MessageBlocks[2].Serialise());
			AssertEquals("DT0100208Y                                                                      ", block.MessageBlocks[3].Serialise());
			AssertEquals("DT02MAKE1                                                                       ", block.MessageBlocks[4].Serialise());
			AssertEquals("DT0100308Y                                                                      ", block.MessageBlocks[5].Serialise());
			AssertEquals("DT02MAKE2                                                                       ", block.MessageBlocks[6].Serialise());
		}

		public void TestDOTLineNumbers()
		{
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";

			invoiceLine.JI_Tariff = "8706005000";
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;

			DOT dot1 = invoiceLine.DOTs.AddNew();
			dot1.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2A;
			dot1.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;

			DOTVIN dotvin1 = dot1.DOTVINs.AddNew();
			dotvin1.US_DOTMake = "MAKE1";

			DOTVIN dotvin2 = dot1.DOTVINs.AddNew();
			dotvin2.US_DOTMake = "MAKE2";

			DOT dot2 = invoiceLine.DOTs.AddNew();
			dot2.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2A;
			dot2.US_DOTClarCode = ClarificationCodeList.Codes.Tire;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, true);
			MQEDIMessage message = builder.PopulateMessage();

			List<MessageBlock> blocks = message.MessageBlock.MessageBlocks.FindAll(x => x.GetType() == typeof(OGADT01));

			for (ZInt index = 0; index < blocks.Count; index++)
			{
				OGADT01 dt01 = (OGADT01)blocks[index];

				AssertEquals("line number", index + 1, dt01.DOTLineNumber);
			}

			builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, false);
			message = builder.PopulateMessage();

			blocks = message.MessageBlock.MessageBlocks.FindAll(x => x.GetType() == typeof(OGADT01));

			for (ZInt index = 0; index < blocks.Count; index++)
			{
				OGADT01 dt01 = (OGADT01)blocks[index];

				AssertEquals("line number", index + 1, dt01.DOTLineNumber);
			}
		}

		public void TestNonDOTTariffButDOTDataEntered()
		{
			invoiceLine.JI_Tariff = "8703105060";

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();

			helper.MessageMustNotContainElement<OGADT01>(message);
			invoiceLine.JI_Tariff = "8708402000";
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;

			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2A;
			dot.US_DOTTireBrandName = "TRADE NAME";
			dot.US_DOTPassport = "124HJ";
			dot.US_DOTCountryOfOrigin = "AU";
			dot.US_DOTBondSuretyCode = "111";
			dot.US_DOTPriorApproval = true;
			dot.US_DOTImpSubstStatement = true;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTTireID = "ZZZ";
			dot.US_DOTCommercialDesc = "DESCRIPTION1";
			DOTVIN dotvin = dot.DOTVINs.AddNew();

			dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._08;
			dot.US_DOTCommercialDesc = "DESCRIPTION2";

			dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "MAKE1";

			dotvin = dot.DOTVINs.AddNew();
			dotvin.US_DOTMake = "MAKE2";

			message = builder.PopulateMessage();

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			AssertEquals("OI        DESCRIPTION1                                                          ", block.MessageBlocks[0].Serialise());
			AssertEquals("DT010012AY124HJ              AU111YYVZZZTRADE NAME                              ", block.MessageBlocks[1].Serialise());
			AssertEquals("OI        DESCRIPTION2                                                          ", block.MessageBlocks[2].Serialise());
			AssertEquals("DT0100208Y                                                                      ", block.MessageBlocks[3].Serialise());
			AssertEquals("DT02MAKE1                                                                       ", block.MessageBlocks[4].Serialise());
			AssertEquals("DT0100308Y                                                                      ", block.MessageBlocks[5].Serialise());
			AssertEquals("DT02MAKE2                                                                       ", block.MessageBlocks[6].Serialise());
		}

		public void TestETPIncludedForFTZWarehouse()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			invoiceLine.JI_Tariff = "2208204000";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLine.FDAs.AddNew();
			fda.AffirmationCodes.AddNew().CY_Code = AffirmationCodeConstants.Codes.SLN;

			var blocks = new List<MessageBlock>(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			var etpBlock = blocks.OfType<OGAFD05>().FirstOrDefault(x => x.AffirmationOfComplianceCode == AffirmationCodeConstants.Codes.ETP);
			AssertNotNull(etpBlock);
			AssertEquals(EntryTypeList.Codes.WarehouseFTZ, etpBlock.AffirmationOfComplianceQualifier);
		}

		public void TestFDATariff()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceHeader.US_FDAContactName = ZString.Empty;
			invoiceHeader.US_FDAContactPhoneNo = ZString.Empty;
			invoiceHeader.US_FDAContactEmail = ZString.Empty;
			invoiceLine.JI_Tariff = "2208204000";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs.AddNew();

			invoiceLine.FDAs[0].US_FDAQty1 = 100.124545454m;
			invoiceLine.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;

			invoiceLine.FDAs[0].US_FDAQty2 = 120.7897454m;
			invoiceLine.FDAs[0].US_FDAMeasure2 = ShippingOrPackingingUnitList.Codes.Aerosol;

			invoiceLine.FDAs[0].US_FDAQty3 = 130.56465465m;
			invoiceLine.FDAs[0].US_FDAMeasure3 = ShippingOrPackingingUnitList.Codes.AmpouleNonProtected;

			invoiceLine.FDAs[0].US_FDAQty4 = 140.7897541541m;
			invoiceLine.FDAs[0].US_FDAMeasure4 = ShippingOrPackingingUnitList.Codes.AmpouleProtected;

			invoiceLine.FDAs[0].US_FDAQty5 = 150.21312m;
			invoiceLine.FDAs[0].US_FDAMeasure5 = ShippingOrPackingingUnitList.Codes.Atomizer;

			invoiceLine.FDAs[0].US_FDAQty6 = 160.1234554m;
			invoiceLine.FDAs[0].US_FDAMeasure6 = ShippingOrPackingingUnitList.Codes.Barrel;

			invoiceLine.FDAs[0].US_ContainerDim1 = 2.89m;
			invoiceLine.FDAs[0].US_ContainerDim2 = 30.52m;
			invoiceLine.FDAs[0].US_ContainerDim3 = 44.78m;
			invoiceLine.FDAs[0].US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneTenthDecimals;
			invoiceLine.FDAs[0].US_FDAContainerDimType = CylindricalRectangularList.Codes.Rectangular;
			invoiceLine.FDAs[0].US_FDACargoStorageCode = CargoStorageCodeList.Codes.AmbientTemperature;
			invoiceLine.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine.FDAs[0].US_FDACommercialDesc = "OTH DIST WINE>$3.43/L, AND";
			invoiceLine.FDAs[0].US_FDAValue = 10000.01m;
			invoiceLine.FDAs[0].US_FDAProductCode = "34AA.AA";

			IPriorNoticeLine btaLine = invoiceLine.FDAs[0];
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			AssertEquals("OI        OTH DIST WINE>$3.43/L, AND                                            ", block.MessageBlocks[0].Serialise());
			AssertEquals("FD0100134AA.AAA  XYOFTI                        XYBEREQU6LON   XYBEREQU6LON      ", block.MessageBlocks[1].Serialise());
			AssertEquals("FD020000016012BA  0000015021AT  0000014079AP  0000013056AM  0000012079AE        ", block.MessageBlocks[2].Serialise());
			AssertEquals("FD030000010000            TRADE NAME                            021430084412    ", block.MessageBlocks[3].Serialise());
			AssertEquals("FD040000010012KG  JESSIE JAM3273958841                                          ", block.MessageBlocks[4].Serialise());
			AssertEquals("FD05PFR12345678901                                                              ", block.MessageBlocks[5].Serialise());
			AssertEquals("FD05PFTG                                                                        ", block.MessageBlocks[6].Serialise());
			AssertEquals("FD05SA1#1                                                                       ", block.MessageBlocks[7].Serialise());
			AssertEquals("FD05SASCA                                                                       ", block.MessageBlocks[8].Serialise());

			consigneeCustomsCode.OK_CustomsRegNo = "0131990020";

			BlockControlGenerator block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			AssertEquals("FD030000010000            TRADE NAME                            021430084412    ", block1.MessageBlocks[3].Serialise());

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "0010814927964");
			invoiceLine.FDAs[0].US_OA_FDAFEI = ultimateConsignee.MainAddress.PK;

			block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			AssertEquals("FD030000010000************TRADE NAME                            021430084412    ", block1.MessageBlocks[3].Serialise());
		}

		public void TestPNDisBuiltInToMessage()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs[0].US_PND = true;

			IPriorNoticeLine btaLine = invoiceLine.FDAs[0];
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			string expected = "B01                                                        <<MSGNO PLACEHOLDER>>OI        TEST                                                                  FD0100124DCS18   XYPND                         XYBEREQU6LON   XYBEREQU6LON      ";
			Assert("PND is included in FD01", block.Serialise().StartsWith(expected));
		}

		public void TestPNDAffirmationCodeIsSentInFD01WhenMultipleAffirmationCodesExist()
		{
			//If the PNC or PND affirmation of compliance codes are used, they must appear in this position first.
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs[0].US_PND = true;

			AffirmationCode affirmationCode1 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode1.CY_Code = AffirmationCodeConstants.Codes.AWB;
			affirmationCode1.CY_Data = "8839485859";

			AffirmationCode affirmationCode2 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode2.CY_Code = AffirmationCodeConstants.Codes.CNO;
			affirmationCode2.CY_Data = "APLU0458394";

			AffirmationCode affirmationCode3 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode3.CY_Code = AffirmationCodeConstants.Codes.PND;

			AffirmationCode affirmationCode4 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode4.CY_Code = AffirmationCodeConstants.Codes.CNO;//duplicate one that will be ignored
			affirmationCode4.CY_Data = "APLU0394857";

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			string expected = "FD0100124DCS18   XYPND                         XYBEREQU6LON   XYBEREQU6LON      ";
			AssertContains("PND is required to be included in FD01 segment", expected, block.Serialise());

			string tEMAffirmationSegment = "FD05TEMWALTER.DOODLEBERRY@COMPAN                                                ";
			AssertNotContains("Transmitter email (TEM) should not be in in a PND message", tEMAffirmationSegment, block.Serialise());

			string furtherAffirmationCodes = "FD05CNOAPLU0394857";
			AssertNotContains("Duplicate Affirmation codes wont be sent", furtherAffirmationCodes, block.Serialise());

			furtherAffirmationCodes = "FD05AWB8839485859";
			AssertContains("Remaining Affirmation Codes should be sent even with PND", furtherAffirmationCodes, block.Serialise());

			furtherAffirmationCodes = "FD05CNOAPLU0458394";
			AssertContains("Remaining Affirmation Codes should be sent even with PND", furtherAffirmationCodes, block.Serialise());
		}

		public void TestFD04isBuiltOnlyWhenRequired()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs.AddNew();
			invoiceHeader.US_FDAContactName = ZString.Empty;
			declaration.US_FDAContactName = ZString.Empty;
			invoiceHeader.US_FDAContactPhoneNo = ZString.Empty;
			declaration.US_FDAContactPhoneNo = ZString.Empty;
			invoiceHeader.US_FDAContactEmail = ZString.Empty;
			declaration.US_FDAContactEmail = ZString.Empty;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IPriorNoticeLine btaLine = invoiceLine.FDAs[0];
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			string expected = "B01                                                        <<MSGNO PLACEHOLDER>>OI        TEST                                                                  FD0100124DCS18   XYOFTI                        XYBEREQU6LON   XYBEREQU6LON      FD020000900000KG                                                                FD030000000001                                                                  FD05PFR12345678901                                                              FD05PFTG                                                                        FD05SA1#1                                                                       FD05SASCA                                                                       FD05SCCUS                                                                       FD05SCNIMPORTER                                                                 FD05SEMNONE                                                                     FD05SFX0000000000                                                               FD05TEMNONE                                                                     OI                                                                              FD01002            OFTI                        XYBEREQU6LON   XYBEREQU6LON      FD02                                                                            FD030000009999                                                                  FD05SA1#1                                                                       FD05SASCA                                                                       FD05SCCUS                                                                       FD05SCNIMPORTER                                                                 FD05SEMNONE                                                                     FD05SFX0000000000                                                               FD05TEMNONE                                                                     Y           00024                                                               ";
			AssertMultilineASCIIEquals("FD04 SHOULD BE BLANK BECAUSE IT IS CONDITIONAL", expected, block.Serialise());
		}

		public void TestFD04isBuiltOnlyWhenRequiredPostFinalRuleImplementation()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs.AddNew();
			invoiceHeader.US_FDAContactName = ZString.Empty;
			declaration.US_FDAContactName = ZString.Empty;
			invoiceHeader.US_FDAContactPhoneNo = ZString.Empty;
			declaration.US_FDAContactPhoneNo = ZString.Empty;
			invoiceHeader.US_FDAContactEmail = ZString.Empty;
			declaration.US_FDAContactEmail = ZString.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			IPriorNoticeLine btaLine = invoiceLine.FDAs[0];
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			string expected = "B01                                                        <<MSGNO PLACEHOLDER>>OI        TEST                                                                  FD0100124DCS18   XYOFTI                        XYBEREQU6LON   XYBEREQU6LON      FD020000900000KG                                                                FD030000000001                                                                  FD05PFR12345678901                                                              FD05PFTG                                                                        FD05SA1#1                                                                       FD05SASCA                                                                       FD05SCCUS                                                                       FD05SCNIMPORTER                                                                 FD05SEMNONE                                                                     FD05SFX0000000000                                                               FD05TEMNONE                                                                     OI                                                                              FD01002            OFTI                        XYBEREQU6LON   XYBEREQU6LON      FD02                                                                            FD030000009999                                                                  FD05SA1#1                                                                       FD05SASCA                                                                       FD05SCCUS                                                                       FD05SCNIMPORTER                                                                 FD05SEMNONE                                                                     FD05SFX0000000000                                                               FD05TEMNONE                                                                     Y           00024                                                               ";
			AssertMultilineASCIIEquals("FD04 SHOULD BE BLANK BECAUSE IT IS CONDITIONAL", expected, block.Serialise());
		}

		public void TestGetOGADisclaimedBlocksDoesNotReturnDeclaredBlocks()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "2208204000";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs[0].US_FDAQty1 = 100;
			invoiceLine.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;
			invoiceLine.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine.FDAs[0].US_FDACommercialDesc = "OTH DIST WINE>$3.43/L, AND";
			invoiceLine.FDAs[0].US_FDAValue = 10000.01m;
			invoiceLine.FDAs[0].US_FDAProductCode = "34AA.AA";

			BlockControlGenerator block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			AssertNull("OGA disclaiming block should NOT be returned", block1.MessageBlocks.Find(x => x.MandatoryCharacters == "OA"));
		}

		public void TestNoIssueReportOnInvalidContainerDimension()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "2208204000";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs[0].US_FDAQty1 = 100;
			invoiceLine.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;
			invoiceLine.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine.FDAs[0].US_FDACommercialDesc = "OTH DIST WINE>$3.43/L, AND";
			invoiceLine.FDAs[0].US_FDAValue = 10000.01m;
			invoiceLine.FDAs[0].US_FDAProductCode = "34AA.AA";
			invoiceLine.FDAs[0].US_ContainerDim1 = 10000m;
			invoiceLine.FDAs[0].US_DimUQ = FDAMeasurementUnitList.Codes.Centimeters;

			BlockControlGenerator block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			block1.Serialise();
			AssertNull("OGA disclaiming block should NOT be returned", block1.MessageBlocks.Find(x => x.MandatoryCharacters == "OA"));
			AssertEquals("No issue report as a result of invalid US_ContainerDim1", "", ErrorReporter.LastMessageReported);
		}

		public void TestWhenMultipleDisclaimsExist()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invoiceLine.ImportTariff.UE_OGACodes = "FD1DT1";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = invoiceLine.CusEntryLine;

			BlockControlGenerator block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetDisclaimingBlocks(entryLine, true));
			AssertEquals("OGA disclaiming block should NOT be returned", 1, block1.MessageBlocks.Count);

			OGAOA oa = (OGAOA)block1.MessageBlocks[0];
			AssertEquals("DT0", oa.OtherAgencyDeclaration);
			AssertEquals("FD0", oa.OtherAgencyDeclaration1);
		}

		public void TestNonFDATariffButFDADataEntered()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "2208900500";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.InvoiceHeader.US_FDAContactName = "Walter Doo";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "8473645600";
			invoiceLine.InvoiceHeader.US_FDAContactEmail = "walter.doolittle@big.com";
			invoiceLine.FDAs.AddNew();

			invoiceLine.FDAs[0].US_FDAQty1 = 100;
			invoiceLine.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;

			invoiceLine.FDAs[0].US_FDAQty2 = 120;
			invoiceLine.FDAs[0].US_FDAMeasure2 = ShippingOrPackingingUnitList.Codes.Aerosol;

			invoiceLine.FDAs[0].US_FDAQty3 = 130;
			invoiceLine.FDAs[0].US_FDAMeasure3 = ShippingOrPackingingUnitList.Codes.AmpouleNonProtected;

			invoiceLine.FDAs[0].US_FDAQty4 = 140;
			invoiceLine.FDAs[0].US_FDAMeasure4 = ShippingOrPackingingUnitList.Codes.AmpouleProtected;

			invoiceLine.FDAs[0].US_FDAQty5 = 150;
			invoiceLine.FDAs[0].US_FDAMeasure5 = ShippingOrPackingingUnitList.Codes.Atomizer;

			invoiceLine.FDAs[0].US_FDAQty6 = 160;
			invoiceLine.FDAs[0].US_FDAMeasure6 = ShippingOrPackingingUnitList.Codes.Barrel;

			invoiceLine.FDAs[0].US_ContainerDim1 = 2.89m;
			invoiceLine.FDAs[0].US_ContainerDim2 = 30.52m;
			invoiceLine.FDAs[0].US_ContainerDim3 = 44.78m;
			invoiceLine.FDAs[0].US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneTenthDecimals;
			invoiceLine.FDAs[0].US_FDAContainerDimType = CylindricalRectangularList.Codes.Rectangular;
			invoiceLine.FDAs[0].US_FDACargoStorageCode = CargoStorageCodeList.Codes.AmbientTemperature;
			invoiceLine.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine.FDAs[0].US_FDACommercialDesc = "OTH DIST WINE>$3.43/L, AND";
			invoiceLine.FDAs[0].US_FDAValue = 10000.01m;
			invoiceLine.FDAs[0].US_FDAProductCode = "34AA.AA";

			IPriorNoticeLine btaLine = invoiceLine.FDAs[0];

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			AssertEquals("OI        OTH DIST WINE>$3.43/L, AND                                            ", block.MessageBlocks[0].Serialise());
			AssertEquals("FD0100134AA.AAA  XY                            XYBEREQU6LON   XYBEREQU6LON      ", block.MessageBlocks[1].Serialise());
			AssertEquals("FD020000016000BA  0000015000AT  0000014000AP  0000013000AM  0000012000AE        ", block.MessageBlocks[2].Serialise());
			AssertEquals("FD030000010000            TRADE NAME                            021430084412    ", block.MessageBlocks[3].Serialise());
			AssertEquals("FD040000010000KG  WALTER DOO8473645600                                          ", block.MessageBlocks[4].Serialise());
		}

		public void TestGetOGADeclaredBlocksReturnsDeclaredBlocks()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "2208204000";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs[0].US_FDAQty1 = 100;
			invoiceLine.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;
			invoiceLine.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine.FDAs[0].US_FDACommercialDesc = "OTH DIST WINE>$3.43/L, AND";
			invoiceLine.FDAs[0].US_FDAValue = 10000.01m;
			invoiceLine.FDAs[0].US_FDAProductCode = "34AA.AA";

			BlockControlGenerator block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));
			Assert("OGA declared blocks should be returned", block1.MessageBlocks.Count > 0);
		}

		public void TestGetOGADisclaimedBlocksReturnsDisclaimedBlocks()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = invoiceLine.CusEntryLine;

			BlockControlGenerator block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetDisclaimingBlocks(entryLine, true));
			AssertEquals("OGA disclaimed block should be returned", 1, block1.MessageBlocks.Count);
		}

		public void TestNonPriorNoticeFDAReportsAffirmationCodes()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceHeader.US_FDAContactName = ZString.Empty;
			invoiceHeader.US_FDAContactPhoneNo = ZString.Empty;
			invoiceHeader.US_FDAContactEmail = ZString.Empty;
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AffirmationCode affirmationCode = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode.CY_Code = "XYZ";
			affirmationCode.CY_Data = "TESTXYZ";
			affirmationCode = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode.CY_Code = "REG";
			affirmationCode.CY_Data = "TEST";

			IPriorNoticeLine priorNoticeLine = invoiceLine.FDAs[0];
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			AssertContains("FD05XYZTESTXYZ                                                                  ", block.Serialise());
		}

		public void TestPNCAffirmationCodeIsSentInFD01WhenMultipleAffirmationCodesExist()
		{
			//If the PNC or PND affirmation of compliance codes are used, they must appear in this position first.
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs[0].US_PNC = "PNC1";

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			string expected = "FD0100124DCS18   XYPNCPNC1                     XYBEREQU6LON   XYBEREQU6LON      ";
			AssertContains("PNC is required to be sent in the FD01 segment", expected, block.Serialise());

			string tEMAffirmationSegment = "FD05TEMWALTER.DOODLEBERRY@COMPAN                                                ";
			AssertNotContains("Transmitter email (TEM) should not be resent once PNC has been received", tEMAffirmationSegment, block.Serialise());
		}

		public void TestAdditionalAffirmationCodesAreSentWithPNC()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs[0].US_PNC = "PNC1";

			AffirmationCode affirmationCode1 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode1.CY_Code = "FCE";
			affirmationCode1.CY_Data = "7545";

			AffirmationCode affirmationCode2 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode2.CY_Code = "BIS";

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			string actual = block.Serialise();
			AssertContains("FD05FCE7545                                                                     FD05BIS                                                                         ", actual);
		}

		public void TestSeveralHouseBillsSelectedForFDALine_NoDuplicates()
		{
			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			declaration.US_EnableENS = true;
			declaration.JE_HouseBill = "HB10020023";
			declaration.JE_MasterBill = "12599675660";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillNum = "HB2";
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_CU_ParentBill = declaration.PrimaryMasterBill.PK;

			var houseBill3 = declaration.Bills.AddNew();
			houseBill3.CU_BillNum = "HB3";
			houseBill3.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill3.CU_CU_ParentBill = declaration.PrimaryMasterBill.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLine.FDAs[0];

			fda.BillsForFDALine.AddPivotFor(declaration.PrimaryHouseBill);
			fda.BillsForFDALine.AddPivotFor(houseBill2);
			fda.BillsForFDALine.AddPivotFor(houseBill3);
			AssertEquals("Bills for FDA Line selected", 3, fda.BillsForFDALine.Count);

			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Precondition: Declaration is surface. This means that affirmation code for bills will be 'BOL' and 'NHB'", true, !declaration.IsAir);

			var entryLine = invoiceLine.CusEntryLine;

			block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.EntrySummary));
			AssertEquals("OGA blocks created", 18, block1.MessageBlocks.Count);

			AssertBlock("BOL", "12599675660");

			var blocks = block1.MessageBlocks.FindAll(x => x.GetType() == typeof(OGAFD05) && ((OGAFD05)x).AffirmationOfComplianceCode == "NHB").ToArray();
			AssertEquals("3 FD05 blocks with affirmation code 'NHB' (House Bill) should be found", 3, blocks.Length);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals("Precondition: Declaration is AIR. This means that affirmation code for bills will be 'AWB' and 'AWH'", true, declaration.IsAir);

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillNum = "MB20050";
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;

			var houseBill2ForMasterBill2 = declaration.Bills.AddNew();
			houseBill2ForMasterBill2.CU_BillNum = "HB2Master2";
			houseBill2ForMasterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2ForMasterBill2.CU_CU_ParentBill = masterBill2.PK;

			var houseBill3ForMasterBill2 = declaration.Bills.AddNew();
			houseBill3ForMasterBill2.CU_BillNum = "HB3Master2";
			houseBill3ForMasterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill3ForMasterBill2.CU_CU_ParentBill = masterBill2.PK;

			fda.BillsForFDALine.AddPivotFor(houseBill2ForMasterBill2);
			fda.BillsForFDALine.AddPivotFor(houseBill3ForMasterBill2);
			AssertEquals("Precondition: 5 bills selected for FDA Line", 5, fda.BillsForFDALine.Count);

			block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.EntrySummary));
			AssertEquals("OGA blocks created", 21, block1.MessageBlocks.Count);

			blocks = block1.MessageBlocks.FindAll(x => x.GetType() == typeof(OGAFD05) && ((OGAFD05)x).AffirmationOfComplianceCode == "AWB").ToArray();
			AssertEquals("Should be 2 Master Bills affirmation code", 2, blocks.Length);
			AssertNotEquals("Blocks are not duplicated, because Bill Numbers are different", ((OGAFD05)blocks[0]).AffirmationOfComplianceQualifier, ((OGAFD05)blocks[1]).AffirmationOfComplianceQualifier);

			AssertBlock("AWB", "12599675660");
			AssertBlock("AWB", "MB20050");

			blocks = block1.MessageBlocks.FindAll(x => x.GetType() == typeof(OGAFD05) && ((OGAFD05)x).AffirmationOfComplianceCode == "AWH").ToArray();
			AssertEquals("5 FD05 blocks with affirmation code 'AWH' (House Bills) should be found", 5, blocks.Length);

			AssertBlock("AWH", "HB10020023");
			AssertBlock("AWH", "HB2");
			AssertBlock("AWH", "HB3");
			AssertBlock("AWH", "HB2Master2");
			AssertBlock("AWH", "HB3Master2");
		}

		public void TestTEMAffirmationCodeIsLastInSortedListOfCodes()
		{
			//If the PNC or PND affirmation of compliance codes are used, they must appear in this position first.
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			AffirmationCode affirmationCode1 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode1.CY_Code = AffirmationCodeConstants.Codes.AWB;
			affirmationCode1.CY_Data = "8839485859";

			AffirmationCode affirmationCode2 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode2.CY_Code = AffirmationCodeConstants.Codes.CNO;
			affirmationCode2.CY_Data = "APLU0458394";

			AffirmationCode affirmationCode4 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode4.CY_Code = AffirmationCodeConstants.Codes.CNO;
			affirmationCode4.CY_Data = "APLU0394857";

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			string expected = "FD05CNOAPLU0458394                                                              FD05OFTI                                                                        FD05PFR12345678901                                                              FD05PFTG                                                                        FD05SA1#1                                                                       FD05SASCA                                                                       FD05SCCUS                                                                       FD05SCNIMPORTER                                                                 FD05SEMNONE                                                                     FD05SFX0000000000                                                               FD05TEMWALTER.DOODLEBERRY@COMPAN                                                ";
			AssertContains("TEM should be added as the last affirmation code", expected, block.Serialise());
		}

		public void TestTEMAffirmationCodeIsNotIncludedInNonPriorNoticeCodes()
		{
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "8714.20.0000";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			AffirmationCode affirmationCode1 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode1.CY_Code = AffirmationCodeConstants.Codes.AWB;
			affirmationCode1.CY_Data = "8839485859";

			AffirmationCode affirmationCode2 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode2.CY_Code = AffirmationCodeConstants.Codes.CNO;
			affirmationCode2.CY_Data = "APLU0458394";

			AffirmationCode affirmationCode4 = invoiceLine.FDAs[0].AffirmationCodes.AddNew();
			affirmationCode4.CY_Code = AffirmationCodeConstants.Codes.CNO;
			affirmationCode4.CY_Data = "APLU0394857";

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions));

			string tEMAffirmationSegment = "FD05TEMWALTER.DOODLEBERRY@COMPAN                                                ";
			AssertNotContains("Transmitter email (TEM) should not be in in a non Prior Notice message", tEMAffirmationSegment, block.Serialise());
		}

		public void TestGenerateOGAWithFDAReferenceDetails()
		{
			var consignee = helper.Consignee;
			consignee.MainAddress.OA_Address2 = "STREET 2";
			consignee.MainAddress.OA_Email = "consignee@test.com";
			consignee.MainAddress.OA_Fax = "1236598746";
			consignee.MainAddress.OA_PostCode = "26594";
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "9546866315", Core.Constants.CountryCodes.UnitedStates);
			DeclarationTestHelper.AddPGAContact(consignee, "BOB", "BROWN", null, null, null);

			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "45638235";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_UI_NKCarrierSCAC = "AAPJ";
			declaration.US_US_NKLocationOfGoods = "AJD5";
			declaration.JE_MasterBill = "5685636";
			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAForcePN = true;

			var importer = declaration.Importer;
			importer.MainAddress.OA_Address1 = "IMPORTER STREET 1";
			importer.MainAddress.OA_Address2 = "IMPORTER STREET 2";
			importer.MainAddress.OA_Email = "importer@test.com";
			importer.MainAddress.OA_Phone = "6934568700";
			importer.MainAddress.OA_Fax = "";
			importer.MainAddress.OA_PostCode = "96358";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importer.MainAddress.OA_City = "LOS ANGELES";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "936528466", Core.Constants.CountryCodes.UnitedStates);
			DeclarationTestHelper.AddPGAContact(importer, "JOHN", "SMITH", null, null, null);

			declaration.JE_OH_FDASubmitter = helper.ImportForwarder.PK;
			var submitter = declaration.FDASubmitter;
			submitter.OH_FullName = "SUBMITTER DUMMY";
			submitter.MainAddress.OA_Address1 = "SUBMITTER STREET 1";
			submitter.MainAddress.OA_Address2 = "SUBMITTER STREET 2";
			submitter.MainAddress.OA_Email = "submitter@test.com";
			submitter.MainAddress.OA_Phone = "8663245866";
			submitter.MainAddress.OA_Fax = "8663245899";
			submitter.MainAddress.OA_PostCode = "60025";
			submitter.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			submitter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "8639601566", Core.Constants.CountryCodes.UnitedStates);
			DeclarationTestHelper.AddPGAContact(submitter, "JONES", "DOLE", "8663245910", "b.paine@submittertest.com", null);
			var submitterWrapper = OrgHeaderWrapper.New(submitter);
			submitterWrapper.ZO_SubmitterFirmType = "I";
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetFDABlocksForOneLine(fda, 1, true));

			string expected = "B01                                                        <<MSGNO PLACEHOLDER>>" +
				"OI                                                                              " +
				"FD01001            SLNDOLE                     XYBEREQU6LON                     " + // Consignee Address Line 1
				"FD02                                                                            " +
				"FD030000010000                                                                  " +
				"FD04              WALTER    8473645600                                          " +
				"FD05CO1STREET 1                                                                 " + // Consignee Address Line 1
				"FD05CO2STREET 2                                                                 " + // Consignee Address Line 2
				"FD05COAAU                                                                       " + // Consignee ISO Country Code
				"FD05COCCONSIGNEE NAME                                                           " + // Consignee Firm Name
				"FD05COECONSIGNEE@TEST.COM                                                       " + // Consignee Email
				"FD05COFBOB                                                                      " + // Consignee First Name
				"FD05COMBROWN                                                                    " + // Consignee Last Name
				"FD05CON9546866315                                                               " + // Consignee Number
				"FD05COP0987654321                                                               " + // Consignee Phone Number
				"FD05COUCITY                                                                     " + // Consignee Address City
				"FD05COVFN                                                                       " + // Consignee Address State or Canadian Province
				"FD05COX1236598746                                                               " + // Consignee FAX
				"FD05COZ26594                                                                    " + // Consignee Zip or Mail Code
				"FD05EFCXJ5                                                                      " + // Entry Filer Code
				"FD05ENT45638235                                                                 " + // Entry Number (Last 8 digits)
				"FD05ETP01                                                                       " + // Entry Type
				"FD05FIRAJD5                                                                     " + // Location of Goods (FIRMS Code)
				"FD05HTS0804504040                                                               " + // Harmonized Tariff Number
				"FD05IM1IMPORTER STREET 1                                                        " + // Importer Address Line 1
				"FD05IM2IMPORTER STREET 2                                                        " + // Importer Address Line 2
				"FD05IMAUS                                                                       " + // Importer ISO Country Code
				"FD05IMCIMPORTER                                                                 " + // Importer Firm Name
				"FD05IMEIMPORTER@TEST.COM                                                        " + // Importer Email
				"FD05IMFJOHN                                                                     " + // Importer First Name
				"FD05IMMSMITH                                                                    " + // Importer Last Name
				"FD05IMN936528466                                                                " + // Importer Number
				"FD05IMP6934568700                                                               " + // Importer Phone Number
				"FD05IMSCA                                                                       " + // Importer Address State or Canadian Province.
				"FD05IMULOS ANGELES                                                              " + // Importer Address City
				"FD05IMX0000000000                                                               " + // Importer Fax
				"FD05IMZ96358                                                                    " + // Importer ZIP or Mail Code
				"FD05MOT11                                                                       " + // Mode of Transportation
				"FD05OFTI                                                                        " + // Owner Firm Type
				"FD05SA1SUBMITTER STREET 1                                                       " + // Submitter Address Line 1
				"FD05SA2SUBMITTER STREET 2                                                       " + // Submitter Address Line 2
				"FD05SACCITY                                                                     " + // Submitter Address City
				"FD05SASIL                                                                       " + // Submitter Address State or Canadian Province.
				"FD05SCAAAPJ                                                                     " + // Importing Carrier
				"FD05SCCUS                                                                       " + // Submitter ISO Country Code 
				"FD05SCNSUBMITTER DUMMY                                                          " + // Submitter Firm Name
				"FD05SCZ60025                                                                    " + // Submitter ZIP or Mail Code
				"FD05SEMB.PAINE@SUBMITTERTEST.COM                                                " + // Submitter E-mail
				"FD05SFNJONES                                                                    " + // Submitter First Name
				"FD05SFTI                                                                        " + // Submitter Firm Type
				"FD05SFX8663245899                                                               " + // Submitter Fax
				"FD05SPN8663245910                                                               " + // Submitter Phone Number
				"FD05BOL5685636                                                                  " + // Bill of Lading
				"FD05TEMWALTER.DOODLEBERRY@COMPAN                                                " +
				"Y           00053                                                               ";
			AssertMultilineASCIIEquals("AFFIRMATION CODES ARE IN THE MESSAGE (IN ORDER)", expected, block.Serialise());
		}

		public void TestGenerateOGAWithFDAReferenceDetailsPostFinalRuleImplementation()
		{
			var consignee = helper.Consignee;
			consignee.MainAddress.OA_Address2 = "STREET 2";
			consignee.MainAddress.OA_Email = "consignee@test.com";
			consignee.MainAddress.OA_Fax = "1236598746";
			consignee.MainAddress.OA_PostCode = "26594";
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "9546866315", Core.Constants.CountryCodes.UnitedStates);
			DeclarationTestHelper.AddPGAContact(consignee, "BOB", "BROWN", null, null, null);

			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "45638235";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_UI_NKCarrierSCAC = "AAPJ";
			declaration.US_US_NKLocationOfGoods = "AJD5";
			declaration.JE_MasterBill = "5685636";
			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAForcePN = true;

			var importer = declaration.Importer;
			importer.MainAddress.OA_Address1 = "IMPORTER STREET 1";
			importer.MainAddress.OA_Address2 = "IMPORTER STREET 2";
			importer.MainAddress.OA_Email = "importer@test.com";
			importer.MainAddress.OA_Phone = "6934568700";
			importer.MainAddress.OA_Fax = "7239500999";
			importer.MainAddress.OA_PostCode = "96358";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importer.MainAddress.OA_City = "LOS ANGELES";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "936528466", Core.Constants.CountryCodes.UnitedStates);
			DeclarationTestHelper.AddPGAContact(importer, "JOHN", "SMITH", null, null, null);

			declaration.JE_OH_FDASubmitter = helper.ImportForwarder.PK;
			var submitter = declaration.FDASubmitter;
			submitter.OH_FullName = "SUBMITTER DUMMY";
			submitter.MainAddress.OA_Address1 = "SUBMITTER STREET 1";
			submitter.MainAddress.OA_Address2 = "SUBMITTER STREET 2";
			submitter.MainAddress.OA_Email = "submitter@test.com";
			submitter.MainAddress.OA_Phone = "8663245866";
			submitter.MainAddress.OA_Fax = "8663245899";
			submitter.MainAddress.OA_PostCode = "60025";
			submitter.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			submitter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "8639601566", Core.Constants.CountryCodes.UnitedStates);
			DeclarationTestHelper.AddPGAContact(submitter, "JONES", "DOLE", "8663245910", "b.paine@submittertest.com", "8663245111");
			var submitterWrapper = OrgHeaderWrapper.New(submitter);
			submitterWrapper.ZO_SubmitterFirmType = "I";
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetFDABlocksForOneLine(fda, 1, true));

			string expected = "B01                                                        <<MSGNO PLACEHOLDER>>" +
				"OI                                                                              " +
				"FD01001            SLNDOLE                     XYBEREQU6LON                     " + // Consignee Address Line 1
				"FD02                                                                            " +
				"FD030000010000                                                                  " +
				"FD04              WALTER    8473645600                                          " + // Transmitter Name & Phone No.
				"FD05CO1STREET 1                                                                 " + // Consignee Address Line 1
				"FD05CO2STREET 2                                                                 " + // Consignee Address Line 2
				"FD05COAAU                                                                       " + // Consignee ISO Country Code
				"FD05COCCONSIGNEE NAME                                                           " + // Consignee Firm Name
				"FD05COECONSIGNEE@TEST.COM                                                       " + // Consignee Email
				"FD05COFBOB                                                                      " + // Consignee First Name
				"FD05COMBROWN                                                                    " + // Consignee Last Name
				"FD05CON9546866315                                                               " + // Consignee Number
				"FD05COP0987654321                                                               " + // Consignee Phone Number
				"FD05COUCITY                                                                     " + // Consignee Address City
				"FD05COVFN                                                                       " + // Consignee Address State or Canadian Province
				"FD05COX1236598746                                                               " + // Consignee FAX
				"FD05COZ26594                                                                    " + // Consignee Zip or Mail Code
				"FD05EFCXJ5                                                                      " + // Entry Filer Code
				"FD05ENT45638235                                                                 " + // Entry Number (Last 8 digits)
				"FD05ETP01                                                                       " + // Entry Type
				"FD05FIRAJD5                                                                     " + // Location of Goods (FIRMS Code)
				"FD05HTS0804504040                                                               " + // Harmonized Tariff Number
				"FD05IM1IMPORTER STREET 1                                                        " + // Importer Address Line 1
				"FD05IM2IMPORTER STREET 2                                                        " + // Importer Address Line 2
				"FD05IMAUS                                                                       " + // Importer ISO Country Code
				"FD05IMCIMPORTER                                                                 " + // Importer Firm Name
				"FD05IMEIMPORTER@TEST.COM                                                        " + // Importer Email
				"FD05IMFJOHN                                                                     " + // Importer First Name
				"FD05IMMSMITH                                                                    " + // Importer Last Name
				"FD05IMN936528466                                                                " + // Importer Number
				"FD05IMP6934568700                                                               " + // Importer Phone Number
				"FD05IMSCA                                                                       " + // Importer Address State or Canadian Province.
				"FD05IMULOS ANGELES                                                              " + // Importer Address City
				"FD05IMX7239500999                                                               " + // Importer Fax
				"FD05IMZ96358                                                                    " + // Importer ZIP or Mail Code
				"FD05MOT11                                                                       " + // Mode of Transportation
				"FD05OFTI                                                                        " + // Owner Firm Type
				"FD05SA1SUBMITTER STREET 1                                                       " + // Submitter Address Line 1
				"FD05SA2SUBMITTER STREET 2                                                       " + // Submitter Address Line 2
				"FD05SACCITY                                                                     " + // Submitter Address City
				"FD05SASIL                                                                       " + // Submitter Address State or Canadian Province.
				"FD05SCAAAPJ                                                                     " + // Importing Carrier
				"FD05SCCUS                                                                       " + // Submitter ISO Country Code 
				"FD05SCNSUBMITTER DUMMY                                                          " + // Submitter Firm Name
				"FD05SCZ60025                                                                    " + // Submitter ZIP or Mail Code
				"FD05SEMB.PAINE@SUBMITTERTEST.COM                                                " + // Submitter E-mail
				"FD05SFNJONES                                                                    " + // Submitter First Name
				"FD05SFTI                                                                        " + // Submitter Firm Type
				"FD05SFX8663245111                                                               " + // Submitter Fax
				"FD05SPN8663245910                                                               " + // Submitter Phone Number
				"FD05BOL5685636                                                                  " + // Bill of Lading
				"FD05TEMWALTER.DOODLEBERRY@COMPAN                                                " + // Transmitter email
				"Y           00053                                                               ";
			AssertMultilineASCIIEquals("AFFIRMATION CODES ARE IN THE MESSAGE (IN ORDER)", expected, block.Serialise());
		}

		public void TestGenerateWPSends3LetterCodeInSCAFieldForAirline()
		{
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_UI_NKCarrierSCAC = "QF";
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			var fda = invoiceLine.FDAs.AddNew();
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetFDABlocksForOneLine(fda, 1, true));
			AssertContains("FD05SCAQFA                                                                      ", block.Serialise());
		}

		public void TestEnsureFDAandDOTareOnlySentInCargoRelease()
		{
			//checking disclaimed
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invoiceLine.JI_Tariff = "8706005000";
			invoiceLine.ImportTariff.UE_OGACodes = "FD1FC3";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FCCIndicator = ZString.Empty;

			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = invoiceLine.CusEntryLine;

			BlockControlGenerator block1 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetDisclaimingBlocks(entryLine, false));
			block1.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, false, ApplicationIdentifierCodeList.Codes.EntrySummary));

			//checking declared
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetDisclaimingBlocks(entryLine, false));
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, false, ApplicationIdentifierCodeList.Codes.EntrySummary));
			AssertNotContains("FD01", block.Serialise());
		}

		public void TestDoNotSendVFTForTruck()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_VoyageFlightNo = "12345";
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAForcePN = true;
			var block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.EntrySummary));
			var message = block.Serialise();

			AssertNotContains("FD05VFT12345", message);
		}

		public void TestFlightNoForAir()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_VoyageFlightNo = "QF123";
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAForcePN = true;
			var block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.EntrySummary));
			var message = block.Serialise();

			AssertContains("FD05VFT123", message);
		}

		public void TestOnlyFirstFiveVoyageForSea()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_VoyageFlightNo = "123456";
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAForcePN = true;
			var block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetOGABlocks(entryLine, true, ApplicationIdentifierCodeList.Codes.EntrySummary));
			var message = block.Serialise();

			AssertNotContains("FD05VFT123456", message);
			AssertContains("FD05VFT12345", message);
		}

		public void TestEmptyAffirmationCodesAreSent()
		{
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "45638235";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			var fda = invoiceLine.FDAs.AddNew();
			var affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = "ABP";
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetFDABlocksForOneLine(fda, 1, true));

			string expected = "B01                                                        <<MSGNO PLACEHOLDER>>" +
				"OI                                                                              " +
				"FD01001            ABP                         XYBEREQU6LON                     " + //ABP should be included
				"FD02                                                                            " +
				"FD030000010000                                                                  " +
				"FD04              WALTER    8473645600                                          " +
				"Y           00005                                                               ";
			AssertMultilineASCIIEquals("AFFIRMATION CODES ARE IN THE MESSAGE (IN ORDER)", expected, block.Serialise());

			fda.US_FDAForcePN = true;
			block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.AddRange(OGABlocksCreator.GetFDABlocksForOneLine(fda, 1, true));
			expected = "B01                                                        <<MSGNO PLACEHOLDER>>" +
				"OI                                                                              " +
				"FD01001            SLN                         XYBEREQU6LON                     " +
				"FD02                                                                            " +
				"FD030000010000                                                                  " +
				"FD04              WALTER    8473645600                                          " +
				"FD05ABP                                                                         " + // ABP should be included
				"FD05CO1#1                                                                       " +
				"FD05COAUS                                                                       " +
				"FD05COCIMPORTER                                                                 " +
				"FD05COENONE                                                                     " +
				"FD05COX0000000000                                                               " +
				"FD05EFCXJ5                                                                      " +
				"FD05ENT45638235                                                                 " +
				"FD05ETP01                                                                       " +
				"FD05HTS2853000095                                                               " +
				"FD05IM1#1                                                                       " +
				"FD05IMAUS                                                                       " +
				"FD05IMCIMPORTER                                                                 " +
				"FD05IMENONE                                                                     " +
				"FD05IMX0000000000                                                               " +
				"FD05MOT11                                                                       " +
				"FD05OFTI                                                                        " +
				"FD05SA1#1                                                                       " +
				"FD05SCCUS                                                                       " +
				"FD05SCNIMPORTER                                                                 " +
				"FD05SEMNONE                                                                     " +
				"FD05SFX0000000000                                                               " +
				"FD05TEMWALTER.DOODLEBERRY@COMPAN                                                " +
				"Y           00028                                                               ";
			AssertMultilineASCIIEquals("AFFIRMATION CODES ARE IN THE MESSAGE (IN ORDER)", expected, block.Serialise());
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new DeclarationTestHelper(Factory);

			declaration = Factory.New<JobDeclaration>();
			consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_FullName = "Importer";
			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			consigneeCustomsCode = consignee.CustomsCodes.AddNew();
			consigneeCustomsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			consigneeCustomsCode.OK_CustomsRegNo = "91-013199000";
			consigneeCustomsCode.OK_CodeType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			manufacturer = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			manufacturer.FillWithValidTestData();
			manufacturer.OH_FullName = "Manufacturer";
			contact = manufacturer.Contacts.AddNew();
			contact.OC_ContactName = "Walter Doodleberry";
			contact.OC_Phone = "+1 (847) 364 5600";
			invoiceHeader.US_FDAContactName = "Walter";
			invoiceHeader.US_FDAContactPhoneNo = "8473645600100";
			invoiceHeader.US_FDAContactEmail = "walter.doodleberry@company.com";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			entryLine = invoiceLine.CusEntryLine;
		}

		OGABlocksCreator OGABlocksCreator => ogaBlocksCreator ?? (ogaBlocksCreator = new OGABlocksCreator());
		OGABlocksCreator ogaBlocksCreator;

		void AssertBlock(ZString affirmationCode, ZString billNumber)
		{
			var blocks = block1.MessageBlocks.FindAll(x => x.GetType() == typeof(OGAFD05) && ((OGAFD05)x).AffirmationOfComplianceCode == affirmationCode && ((OGAFD05)x).AffirmationOfComplianceQualifier == billNumber).ToArray();
			AssertEquals(string.Format("Only one FD05 block for Bill Number '{0}' should be found", billNumber), 1, blocks.Length);
		}

		DeclarationTestHelper helper;
		OrgHeader manufacturer;
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entry;
		CusEntryLine entryLine;
		OrgContact contact;
		OrgHeader consignee;
		OrgCusCode consigneeCustomsCode;
		ABIInputBlockControlGenerator block1;
	}
}
