using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class EntrySummaryMessageBuilderMiscTest : EntrySummaryMessageBuilderAbstractTest
	{
		public void Test22RecordForEntryType22()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_ITDate = new ZDateTime(2013, 1, 1);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ens = declaration.ActiveEntryHeaders[0];
			var message = new EntrySummaryMessageBuilder(ens, UpdateActionCode.Add, false).PopulateMessage();

			var ens22 = message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is ENS22);
			AssertNotNull(ens22);
		}

		public void Test22RecordForEntryType31()
		{
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ens = declaration.ActiveEntryHeaders[0];
			var message = new EntrySummaryMessageBuilder(ens, UpdateActionCode.Add, false).PopulateMessage();

			var ens22 = message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is ENS22);
			AssertNull(ens22);
		}

		[TestDate(2016, 03, 27)]
		public void TestSoftwoodLumberExportChanges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._11, "Atlantic Lumber Board (ALB) Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "4407100115";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_LumberExportCharges = 120m;
			invoiceLine.US_LumberExportPrice = 2000m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("CA Lumber");

			CusEntryHeader ens = declaration.ActiveEntryHeaders[0];
			MQEDIMessage message = (MQEDIMessage)ens.Messages.FirstOutgoingMessage;
			ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			var ens52 = block.MessageBlocks.OfType<ENS52>().FirstOrDefault();
			AssertEquals("OtherDataIndicator1", "01", ens52.OtherDataIndicator1);
			AssertEquals("OtherDataElement1", "000002000", ens52.OtherDataElement1);
			AssertEquals("OtherDataIndicator2", "01", ens52.OtherDataIndicator2);
			AssertEquals("OtherDataElement2", "Y00000000120", ens52.OtherDataElement2);
		}

		[TestDate(2016, 03, 27)]
		public void TestDeferredTaxIndicator()
		{
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("Deferred Tax Indicator");

			CusEntryHeader ens = declaration.ActiveEntryHeaders[0];
			MQEDIMessage message = (MQEDIMessage)ens.Messages.FirstOutgoingMessage;
			ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			var ens90 = block.MessageBlocks.OfType<ENS90>().FirstOrDefault();
			AssertEquals("DeferredTaxIndicator", TaxDeferIndicatorList.Codes.DeferredTax, ens90.DeferredTaxIndicator);
		}

		[TestDate(2016, 03, 27)]
		public void TestTariffNumberNotIncludedForHMF()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("HMF");

			CusEntryHeader ens = declaration.ActiveEntryHeaders[0];
			MQEDIMessage message = (MQEDIMessage)ens.Messages.FirstOutgoingMessage;

			ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			List<MessageBlock> ens62s = block.MessageBlocks.FindAll((MessageBlock msgBlock) => msgBlock is ENS62);

			bool has62ForHMF = false;

			foreach (ENS62 ens62 in ens62s)
			{
				if (ens62.ClassCode.ToString() == Core.Constants.USCustoms.FeeCodes.HMF)
				{
					has62ForHMF = true;
					AssertEquals("tariff number should be empty for HMF", "", ens62.TariffNumber);
				}
			}

			Assert(has62ForHMF);
		}

		[TestDate(2016, 03, 27)]
		public void TestPortOfLadingSentFor10_11_12_Only()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60204", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);

			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2801", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			refHelper.CreateTransportModeForCusCodeList(port.PK, TransportTypeList.Codes.Air);
			var attributeNameUnlading = refHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			Factory.Save();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_SchDLoading = "60204";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("LADING");

			CusEntryHeader ens = declaration.ActiveEntryHeaders[0];
			MQEDIMessage message = (MQEDIMessage)ens.Messages[0];

			ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			List<MessageBlock> ens40s = block.MessageBlocks.FindAll((MessageBlock msgBlock) => msgBlock is ENS40);

			foreach (ENS40 ens40 in ens40s)
			{
				AssertEquals("60204", ens40.PortOfLading);
			}

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			MergeAndSend("LADING");

			message = (MQEDIMessage)ens.Messages[1];
			block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			ens40s = block.MessageBlocks.FindAll((MessageBlock msgBlock) => msgBlock is ENS40);

			foreach (ENS40 ens40 in ens40s)
			{
				AssertEquals("60204", ens40.PortOfLading);
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			declaration.US_UI_NKCarrierSCAC = "QF";
			declaration.JE_MasterBillIssuerSCAC = "QF";
			declaration.US_SchDArrival = "2801";
			declaration.US_SchDEntry = "2801";
			MergeAndSend("LADING");

			message = (MQEDIMessage)ens.Messages[2];
			block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			ens40s = block.MessageBlocks.FindAll((MessageBlock msgBlock) => msgBlock is ENS40);

			foreach (ENS40 ens40 in ens40s)
			{
				AssertEquals("60204", ens40.PortOfLading);
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			scacCode.OK_CustomsRegNo = "QF";
			declaration.US_UI_NKCarrierSCAC = "QF";
			declaration.JE_MasterBillIssuerSCAC = "QF";
			declaration.US_IsHMFApplicable = "";
			declaration.JE_VoyageFlightNo = "123";

			MergeAndSend("LADING");

			message = (MQEDIMessage)ens.Messages[3];
			block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			ens40s = block.MessageBlocks.FindAll((MessageBlock msgBlock) => msgBlock is ENS40);

			foreach (ENS40 ens40 in ens40s)
			{
				AssertEquals("", ens40.PortOfLading);
			}
		}

		[TestDate(2016, 03, 27)]
		public void TestDeleteMessaging()
		{
			declaration.JE_MasterBill = "OBL10000";
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "0709902000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 105000m;
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.US_OverrideDuty = true;
			invoiceLine.US_Duty = 100;

			declaration.JE_RL_NKPortOfLoading = "HNGJA";
			invoiceLine.US_UC_NKCountryOfOrigin = "HN";
			invoiceLine.CountryOfOrigin_US.UC_MiscellaneousSPIEndDate = ZDateTime.Empty;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.P;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";

			MergeAndSend("10000");

			CusEntryHeader ens = declaration.ActiveEntryHeaders[0];
			EntrySummaryMessageBuilder messageBuilder = new EntrySummaryMessageBuilder(ens, UpdateActionCode.Delete, false);
			MQEDIMessage deleteMessage = messageBuilder.PopulateMessage();
			ens.Messages.Add(deleteMessage);
			Factory.Save();

			ABIInputBlockControlGenerator deleteBlock = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			deleteBlock.Deserialise(BlockPadder.Pad(deleteMessage.EM_MessageText));

			AssertEquals(1, deleteBlock.MessageBlocks.Count);
			ENS10 ens10 = (ENS10)deleteBlock.MessageBlocks[0];

			AssertEquals("UpdateActionCode", "D", ens10.UpdateActionCode);
			AssertEquals("DistrictPortOfEntry", "8888", ens10.DistrictPortOfEntry);
			AssertEquals("EntryFiler", "XJ5", ens10.EntryFilerCode);
			AssertEquals("EntryNumber", "00000014", ens10.EntryNumber);
			AssertEquals("EntryType", "01", ens10.EntryType);

			Assert(ens10.ImporterOfRecordNumber.IsEmpty);
			Assert(ens10.UltimateConsigneeNumber.IsEmpty);
			Assert(ens10.CBPF4811ReferenceNumber.IsEmpty);
			Assert(ens10.LiveEntryIndicator.IsEmpty);
			Assert(ens10.MissingDocumentCodes.IsEmpty);
			Assert(ens10.BondType.IsEmpty);
			Assert(ens10.EstimatedEntryDate.IsEmpty);
			Assert(ens10.ElectronicInvoiceIndicator.IsEmpty);
			Assert(ens10.SuretyCode.IsEmpty);
			Assert(ens10.StateOfDestination.IsEmpty);
			Assert(ens10.OGALineReleaseIndicator.IsEmpty);

			APLY aplY = deleteBlock.Y;
			Assert(aplY.TotalEstimatedDuty.IsEmpty);
		}

		[TestDate(2009, 6, 2)]
		public void TestCommercialDescriptionIsSendInFull()
		{
			string longString1 = "THIS IS A LONG DESCRIPTION LINE 1".PadRight(70, '*');
			string longString2 = "THIS IS A LONG DESCRIPTION LINE 2".PadRight(70, '*');
			string longString3 = "THIS IS A LONG DESCRIPTION LINE 3".PadRight(70, '*');
			invoiceLine.JI_Description = longString1 + longString2 + longString3;

			string longString4 = "THIS IS A LONG DESCRIPTION FOR THE SECOND LINE 1".PadRight(70, '*');
			string longString5 = "THIS IS A LONG DESCRIPTION FOR THE SECOND LINE 2".PadRight(70, '*');
			string longString6 = "THIS IS A LONG DESCRIPTION FOR THE SECOND LINE 3".PadRight(70, '*');
			JobComInvoiceLine invoiceLine2 = invoiceLine.Clone();
			invoiceLine2.JI_LineNo = 2;
			invoiceHeader.JobComInvoiceLines.Add(invoiceLine2);
			invoiceLine2.JI_Tariff = invoiceLine.JI_Tariff;

			invoiceLine2.JI_Weight = 2000m;
			invoiceLine2.JI_LinePrice = 3000m;
			invoiceLine2.JI_InvoiceQuantity = 1000m;
			invoiceLine2.JI_InvoiceUQ = "NO";
			invoiceLine2.JI_CustomsQuantity = 20m;
			invoiceLine2.JI_Description = longString4 + longString5 + longString6;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_IsInvoiceByRequest = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(2);
			declaration.US_EnableAII = true;
			invoiceHeader.JZ_OH_Supplier = manufacturer.PK;
			MergeAndSend("COMDESC", false, false);

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);
			MQEDIMessage message = (MQEDIMessage)entry.Messages.LastOutgoingMessage;
			List<ENS43> ens43CommercialDescriptions = new List<ENS43>();
			ENS43 ens43InvReq = null;
			foreach (ENS43 ens43 in message.GetMessageBlocks<ENS43>())
			{
				if (ens43.TypeIndicator == PIRPRulingTypeList.Codes.CommercialDescription)
				{
					ens43CommercialDescriptions.Add(ens43);
				}
				else if (ens43.TypeIndicator == PIRPRulingTypeList.Codes.BindingRulings && ens43.PreImportationReviewProgramPIRPRulingsNumber == "INVREQ")
				{
					ens43InvReq = ens43;
				}
			}
			AssertNotNull("Should have ENS43 with INVREQ", ens43InvReq);
			AssertArrayEqualsByElements("No ENS43 for Commercial Description should be send when INVREQ is sent", ens43CommercialDescriptions.ToArray(), System.Array.Empty<ENS43>());

			CargoWise.Common.ErrorReporter.Clear();
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			if (CargoWise.Common.ErrorReporter.LastMessageReported == "Deleting CusEntryHeader with messages")
			{
				CargoWise.Common.ErrorReporter.Clear();
			}
			declaration.US_IsInvoiceByRequest = false;
			MergeAndSend("COMDESC", false, false);

			entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);
			message = (MQEDIMessage)entry.Messages.LastOutgoingMessage;
			List<string> expectedDescriptions = new List<string>(6);
			expectedDescriptions.Add(longString1);
			expectedDescriptions.Add(longString2);
			expectedDescriptions.Add(longString3);
			expectedDescriptions.Add(longString4);
			expectedDescriptions.Add(longString5);
			expectedDescriptions.Add(longString6);
			ens43InvReq = null;
			foreach (ENS43 ens43 in message.GetMessageBlocks<ENS43>())
			{
				if (ens43.TypeIndicator == PIRPRulingTypeList.Codes.CommercialDescription)
				{
					if (expectedDescriptions.Count == 0)
					{
						Fail("Unexpected ENS43 with Commercial Descrption:" + ens43.CommercialDescription);
					}
					else
					{
						AssertEquals(expectedDescriptions[0], ens43.CommercialDescription);
						expectedDescriptions.RemoveAt(0);
					}
				}
				else if (ens43.TypeIndicator == PIRPRulingTypeList.Codes.BindingRulings && ens43.PreImportationReviewProgramPIRPRulingsNumber == "INVREQ")
				{
					ens43InvReq = ens43;
				}
			}
			AssertNull("Should not have ENS43 with INVREQ", ens43InvReq);
			if (expectedDescriptions.Count > 0)
			{
				ZStringBuilder builder = new ZStringBuilder(expectedDescriptions);
				Fail("The following extra commercial description where not found:\r\n" + builder.ToStringWithNewLineBetweenAppends());
			}
		}

		[TestDate(2016, 03, 27)]
		public void Test9813WithADD_CVDDetails()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._01, "Steel Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "L888", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A307815000";
			uscCase.U5_ISOCountryCode = "VE";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;

			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "7209170030";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "7225506000";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "98130020";

			var rate1 = uscCase.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 0.43m;
			rate1.U6_EffectiveDate = ZDateTime.Today;
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_US_NKLocationOfGoods = "L888";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			invoiceLine.US_SupTariff = "9813.00.20";
			invoiceLine.US_UC_NKCountryOfOrigin = "VE";
			invoiceLine.US_UC_NKCountryOfExport = "VE";

			invoiceLine.JI_Tariff = "7209.17.0030";
			invoiceLine.JI_CustomsQuantity = 1500m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_Description = "Blah";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A307815000";
			//This test is to assert and ADD amounts for TIB entries.
			invoiceLine.AntidumpingDutyCase.U5_CaseStatus = ACCaseStatusList.Codes.IT;

			MergeAndSend("TIB with ADD_CVD");

			AssertEquals("ADD duty calculated for TIB entries", 4300.00m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty);

			MQEDIMessage message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];
			ENS60 ens60 = (ENS60)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "60");

			AssertNotNull(60);
			AssertEquals("ADD details should be reported", "A307815000", ens60.AntidumpingCaseNumber);
			AssertEquals("ADD details should be reported", "0", ens60.BondedADDIndicator);
			AssertEquals("ADD details should be reported", 4300.00m, ens60.AntidumpingDuty);
			AssertEquals("ADD details should be reported", 0.43m, ens60.ADDDepositRate);
		}

		[TestDate(2016, 03, 27)]
		public void TestOnlyFirst5CharsFromVoyageIsSent()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VoyageFlightNo = "1234E678";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("5Voyage");

			MQEDIMessage message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];
			AssertEquals("1234E", message.GetMessageBlocks<ENS20>()[0].VoyageFlightTripManifestNumber);
		}

		[TestDate(2016, 03, 27)]
		public void TestDerivedDutyCalculationWithADD_CVDDetails()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A570204006";
			uscCase.U5_ISOCountryCode = "CN";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;

			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "8205203000";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "8205595510";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "8465960015";

			var rate1 = uscCase.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.73m;
			rate1.U6_EffectiveDate = ZDateTime.Today;
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;

			invoiceHeader.JZ_InvoiceAmount = 24000m;

			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			line2.JI_Tariff = "8205.20.3000";   // .4c per unit + 6.1% = 24000 * .061 = $1464 (this is sent)
			line2.JI_Description = "line with the highest duty rate";
			line2.JI_CustomsQuantity = 0m;
			line2.JI_LinePrice = 16000m;
			line2.US_ADDCaseNo = "A570204006";
			line2.US_ADDDepositValue = 16000m;
			line2.US_UC_NKCountryOfOrigin = "CN";

			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (24000 * .054) = $1296
			line3.JI_Description = "line with the lower duty rate";
			line3.JI_CustomsQuantity = 0m;//due to this
			line3.JI_LinePrice = 8000m;
			line3.US_UC_NKCountryOfOrigin = "CN";

			MergeAndSend("DerivedWithADD");

			MQEDIMessage message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];

			ENS35 ens35 = (ENS35)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "35");
			AssertEquals("ADD in ENS35", 27680m, ens35.PayableADDDuty);

			ENS40 ens40 = (ENS40)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "40");
			AssertNotNull(ens40);
			AssertEquals("Deposit Value in ENS40", 16000m, ens40.ADDSpecificDepositValue);

			ENS60 ens60 = (ENS60)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "60");
			AssertNotNull(ens60);
			AssertEquals("Case Number in ENS60", "A570204006", ens60.AntidumpingCaseNumber);
			AssertEquals("Deposit rate in ENS60", 1.73m, ens60.ADDDepositRate);
			AssertEquals("ADD in ENS60", 27680m, ens60.AntidumpingDuty);

			ENS90 ens90 = (ENS90)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "90");
			AssertEquals("ADD in ENS90", 27680m, ens90.TotalAntidumpingDutyAmount);
		}

		//CS00094954
		[TestDate(2016, 03, 27)]
		public void TestCustomsValueIsRounded50cUp()
		{
			invoiceHeader.JZ_InvoiceAmount = 10000.50;
			invoiceLine.JI_LinePrice = 10000.50m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("CustomsValueRounding");

			MQEDIMessage message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];
			ENS40 ens40 = (ENS40)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "40");

			AssertEquals(10001m, ens40.Value);
		}

		//CS00094954
		[TestDate(2008, 03, 27)]
		public void TestCustomsValueIsRounded50cUpInSecondaryLines()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_SupTariff = "9802004020";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 10000.50m;
			invoiceLine.JI_Tariff = "8407344800";
			invoiceLine.JI_LinePrice = 200.50m;
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 1, 3);

			MergeAndSend("ValueRounding");

			MQEDIMessage message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];

			ENS40 ens40 = (ENS40)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "40");
			AssertEquals(10000m, ens40.Value);

			ENS70 ens70 = (ENS70)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "70");
			AssertEquals(201m, ens70.Value);
		}

		//CS00136226
		[TestDate(2016, 03, 27)]
		public void TestBondAMountIsInWholeDollars()
		{
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount = 25000.26;
			declaration.US_BondProducerAccNo = "123213";
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.US_SuretyCode = "891";

			MergeAndSend("BondAmountWholeDollars");

			MQEDIMessage message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];
			ENS21 ens21 = (ENS21)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "21");

			AssertEquals(25001m, ens21.BondAmount);
		}

		protected override void PopulateMessage(CusEntryHeader entryHeader, bool certifyCargoRelease)
		{
			base.PopulateMessage(entryHeader, certifyCargoRelease);
			entryHeader.CH_BGMReference = "Misc " + entryHeader.CH_BGMReference;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
