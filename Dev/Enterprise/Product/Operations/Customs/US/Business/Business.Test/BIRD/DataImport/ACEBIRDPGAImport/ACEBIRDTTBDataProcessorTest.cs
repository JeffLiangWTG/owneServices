using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDTTBDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public void TestProcessCOLAAndCertificatesMessageBlocks()
		{
			invoiceLine.JI_Description = "LARGE CIGARS";

			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ttbLine.US_ProcessingCode = TTBWINProcessingCodeList.Codes.T11;
			ttbLine.US_PermitNumber = "OH-TI-99999";

			var colaLine1 = ttbLine.COLAAndCertificates.AddNew();
			colaLine1.US_COLAExemptionCode = TTBExemptionCodeList.Codes.TTBEX7;
			colaLine1.US_ForeignCertificateCountry = Core.Constants.CountryCodes.HongKong;

			var colaLine2 = ttbLine.COLAAndCertificates.AddNew();
			colaLine2.US_COLA = "11419999999999";
			colaLine2.HasForeignCertificate = true;
			colaLine2.US_ForeignCertificateCountry = Core.Constants.CountryCodes.Mexico;

			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_TTBInd);
			AssertEquals("LARGE CIGARS", invoiceLineImported.JI_Description);
			AssertEquals(1, invoiceLine.TTBLines.Count);

			var ttbLineImported = invoiceLineImported.TTBLines[0];
			AssertEquals(TTBProgramCodeList.Codes.Wine, ttbLineImported.US_ProgramCode);
			AssertEquals(TTBWINProcessingCodeList.Codes.T11, ttbLineImported.US_ProcessingCode);
			AssertEquals("OH-TI-99999", ttbLineImported.US_PermitNumber);
			AssertEquals(false, ttbLineImported.US_IsReleaseUnderBond);
			AssertEquals(2, ttbLineImported.COLAAndCertificates.Count);

			var colaLine0Imported = ttbLineImported.COLAAndCertificates[0];
			AssertEquals(TTBExemptionCodeList.Codes.TTBEX7, colaLine0Imported.US_COLAExemptionCode);
			AssertEquals(Core.Constants.CountryCodes.HongKong, colaLine0Imported.US_ForeignCertificateCountry);
			var colaLine1Imported = ttbLineImported.COLAAndCertificates[1];
			AssertEquals("11419999999999", colaLine1Imported.US_COLA);
			AssertEquals(true, colaLine1Imported.HasForeignCertificate);
			AssertEquals(Core.Constants.CountryCodes.Mexico, colaLine1Imported.US_ForeignCertificateCountry);
		}

		public void TestProcessQuantityOfCigaretteTubesMessageBlocks()
		{
			invoiceLine.JI_Description = "CIGARETTE TUBES";

			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T51;
			ttbLine.US_QuantityInPCS = 500000m;

			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals("CIGARETTE TUBES", invoiceLineImported.JI_Description);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_TTBInd);

			var ttbLineImported = invoiceLineImported.TTBLines[0];
			AssertEquals(TTBProgramCodeList.Codes.Tobacco, ttbLineImported.US_ProgramCode);
			AssertEquals(TTBTOBProcessingCodeList.Codes.T51, ttbLineImported.US_ProcessingCode);
			AssertEquals(500000m, ttbLineImported.US_QuantityInPCS);
		}

		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST TTB DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_TTBInd);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_TTBDisclaimReason);
			AssertEquals("TEST TTB DISCLAIMED", invoiceLineImported.JI_Description);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "LARGE CIGARS";

			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T42;
			ttbLine.US_PermitNumber = "OH-TI-99999";
			ttbLine.US_IsReleaseUnderBond = true;
			ttbLine.US_OA_ConsigneeAddress = consignee.MainAddress.PK;
			ttbLine.US_NumberForIRC = "TP-OH-77777";

			var cigar1 = ttbLine.Cigars.AddNew();
			cigar1.US_Quantity = 100;
			cigar1.US_UnitPrice = 39.5m;
			var cigar2 = ttbLine.Cigars.AddNew();
			cigar2.US_Quantity = 200;
			cigar2.IsMaximumRate = ZBool.True;
			var cigar3 = ttbLine.Cigars.AddNew();
			cigar3.US_Quantity = 100;
			cigar3.US_IsSmall = true;

			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_TTBInd);
			AssertEquals("LARGE CIGARS", invoiceLineImported.JI_Description);
			AssertEquals(1, invoiceLine.TTBLines.Count);

			CombineAssertions(() =>
			{
				var ttbLineImported = invoiceLineImported.TTBLines[0];
				AssertEquals(TTBProgramCodeList.Codes.Tobacco, ttbLineImported.US_ProgramCode);
				AssertEquals(TTBTOBProcessingCodeList.Codes.T42, ttbLineImported.US_ProcessingCode);
				AssertEquals("OH-TI-99999", ttbLineImported.US_PermitNumber);
				AssertEquals(true, ttbLineImported.US_IsReleaseUnderBond);
				AssertEquals(consignee.MainAddress.PK, ttbLineImported.US_OA_ConsigneeAddress);
				AssertEquals("TP-OH-77777", ttbLineImported.US_NumberForIRC);
				AssertEquals(0, ttbLineImported.COLAAndCertificates.Count);
				AssertEquals(3, ttbLineImported.Cigars.Count);

				var cigar0Imported = ttbLineImported.Cigars[0];
				AssertEquals(100, cigar0Imported.US_Quantity);
				AssertEquals(39.5m, cigar0Imported.US_UnitPrice);
				var cigar1Imported = ttbLineImported.Cigars[1];
				AssertEquals(200, cigar1Imported.US_Quantity);
				AssertEquals(true, cigar1Imported.IsMaximumRate);
				var cigar2imported = ttbLineImported.Cigars[2];
				AssertEquals(100, cigar2imported.US_Quantity);
				AssertEquals(true, cigar2imported.US_IsSmall);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;

			consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "IMP324KDS";
			consignee.OH_FullName = "YUMMY TOBACCO MANUFACTURING";
			consignee.OH_RL_NKClosestPort = "USOHY";
			consignee.MainAddress.OA_Address1 = "1 BUCKEYE AVE";
			consignee.MainAddress.OA_City = "WESTERVILLE";
			consignee.MainAddress.OA_State = "OH";
			consignee.MainAddress.OA_PostCode = "44081";
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "34811-0000001", Core.Constants.CountryCodes.UnitedStates);
		}
		OrgHeader consignee;
	}
}
