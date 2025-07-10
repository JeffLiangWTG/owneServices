using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;
using Inst = Enterprise.Customs.ZA.Business.MessageDataProviderInstruction;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class MessageSendingObjectTest_ICUSDECMessageDataProvider : TestCaseWithFactory
	{
		public void TestDeclarationType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			var provider = (ICUSDECMessageDataProvider)sendingObject;
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				AssertEquals("RCD", provider.DeclarationType);
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
				{
					var declarationTypeCode = declarationTypePair.Code;
					sendingObject.DeclarationType = declarationTypeCode;
					AssertEquals(declarationTypeCode, provider.DeclarationType);
				}
			}
		}

		public void TestShouldOutputFinancialAccountNumber()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "45", "00", "", "", "EXW", group: UniversalReferenceConstants.RefCusProcedureGroup.Excise);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "45";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInstruction1.PK;
			invLine.JI_Procedure = "4500";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.Free;
			var provider = (ICUSDECMessageDataProvider)new MessageSendingObject(entry);
			Assert(!provider.ShouldOutputFinancialAccountNumber);
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			Assert(provider.ShouldOutputFinancialAccountNumber);
		}

		public void TestVesselAgent()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_VesselAgent = "1234";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				AssertEquals("1234", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).VesselAgent);
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				AssertEquals("", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).VesselAgent);
			}
		}

		public void TestMasterCargoCarrier()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_CarrierCode = "1234";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				AssertEquals("1234", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).MasterCargoCarrier);
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				AssertEquals("", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).MasterCargoCarrier);
			}
		}

		public void TestMessageTypeTranslateToEDIFACTMessageFunctionCode()
		{
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			var sendingObject = new MessageSendingObject(invoiceLine1.CusEntryLine.Header);
			sendingObject.MessageType = "org";
			AssertEquals(MessageFunctionCodeList.Codes.Original, ((ICUSDECMessageDataProvider)sendingObject).MessageType);
		}

		public void TestFinancialAccountNumber()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "buyer";
			buyer.OH_IsConsignee = true;
			buyer.OH_Code = "IMP#@$43";
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = collection.AddNew();
			creditor.OrganizationPK = buyer.PK;
			creditor.CustomsOfficeCode = "JHB";
			creditor.FinancialAccountNumber = "3234002346";
			creditor.ImporterPays = ZBool.True;
			creditor.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			declaration.JE_CustomsOffice = "JHB";
			declaration.JE_AGTCode = "ASBSD";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals("3234002346", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).FinancialAccountNumber);
		}

		public void TestFANNumberForVOC()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("1");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("6");
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "buyer";
			buyer.OH_IsConsignee = true;
			buyer.OH_Code = "IMP#@$43";
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mappingForF = collection.AddNew();
			mappingForF.OrganizationPK = buyer.PK;
			mappingForF.CustomsOfficeCode = "JHB";
			mappingForF.FinancialAccountNumber = "3234002346";
			mappingForF.ImporterPays = ZBool.True;
			mappingForF.AccountStartDay = 1;
			var mappingForC = collection.AddNew();
			mappingForC.OrganizationPK = buyer.PK;
			mappingForC.FinancialAccountNumber = "3234002347";
			mappingForC.ImporterPays = ZBool.True;
			mappingForC.Cash = ZBool.True;
			mappingForC.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			declaration.JE_CustomsOffice = "JHB";
			declaration.JE_AGTCode = "ASBSD";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			var header = invoiceLine1.CusEntryLine.Header;
			CombineAssertions("Empty Message", () =>
			{
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "C", "ORG-C", "3234002347");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "C", "CHG-C", "3234002347");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "F", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "F", "CHG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "V", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "V", "CHG-F", "3234002346");
			});
			CombineAssertions("With one failed CUSDEC", () =>
			{
				var cusdecMessage = Factory.New<CUSDECEDIMessage>();
				cusdecMessage.EM_ReceiveTransmit = "TRX";
				cusdecMessage.EM_ApplicationCode = "ZAC";
				cusdecMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
				cusdecMessage.EM_Status = "SNT";
				cusdecMessage.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("RFF+ACD:202'", "RFF+ABI:FANNO1'RFF+ACD:202'").Replace("UNT+109+202'", "UNT+110+202'");
				cusdecMessage.EM_MessageNum = "202";
				header.Messages.Add(cusdecMessage);
				var cusresMessage = Factory.New<CUSRESEDIMessage>();
				cusresMessage.EM_ReceiveTransmit = "RCV";
				cusresMessage.EM_ApplicationCode = "ZAC";
				cusresMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
				cusresMessage.EM_Status = "PRS";
				cusresMessage.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestGIS6MessageWithMRNNumber.Replace("\r\n", "");
				header.Messages.Add(cusresMessage);
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "C", "ORG-C", "3234002347");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "C", "CHG-C", "3234002347");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "F", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "F", "CHG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "V", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "V", "CHG-F", "3234002346");
			});
			CombineAssertions("With one successful CUSDEC but no ABI FAN Number", () =>
			{
				var cusdecMessage = Factory.New<CUSDECEDIMessage>();
				cusdecMessage.EM_ReceiveTransmit = "TRX";
				cusdecMessage.EM_ApplicationCode = "ZAC";
				cusdecMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
				cusdecMessage.EM_Status = "SNT";
				cusdecMessage.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("202'", "201'");
				cusdecMessage.EM_MessageNum = "201";
				header.Messages.Add(cusdecMessage);
				var cusresMessage = Factory.New<CUSRESEDIMessage>();
				cusresMessage.EM_ReceiveTransmit = "RCV";
				cusresMessage.EM_ApplicationCode = "ZAC";
				cusresMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
				cusresMessage.EM_Status = "PRS";
				cusresMessage.EM_MessageText = ZAMessageTest.CUSRESTestMessage.Replace("\r\n", "").Replace("202", "201");
				header.Messages.Add(cusresMessage);
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "C", "ORG-C", "3234002347");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "C", "CHG-C", "3234002347");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "F", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "F", "CHG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "V", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "V", "CHG-F", "3234002346");
			});
			CombineAssertions("With one successful CUSDEC", () =>
			{
				var cusdecMessage = Factory.New<CUSDECEDIMessage>();
				cusdecMessage.EM_ReceiveTransmit = "TRX";
				cusdecMessage.EM_ApplicationCode = "ZAC";
				cusdecMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
				cusdecMessage.EM_Status = "SNT";
				cusdecMessage.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("RFF+ACD:202'", "RFF+ABI:FANNO2'RFF+ACD:202'").Replace("UNT+109+202'", "UNT+110+202'").Replace("202", "203");
				cusdecMessage.EM_MessageNum = "203";
				header.Messages.Add(cusdecMessage);
				var cusresMessage = Factory.New<CUSRESEDIMessage>();
				cusresMessage.EM_ReceiveTransmit = "RCV";
				cusresMessage.EM_ApplicationCode = "ZAC";
				cusresMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
				cusresMessage.EM_Status = "PRS";
				cusresMessage.EM_MessageText = ZAMessageTest.CUSRESTestMessage.Replace("\r\n", "").Replace("202", "203");
				header.Messages.Add(cusresMessage);
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "C", "ORG-C", "3234002347");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "C", "CHG-C", "FANNO2");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "F", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "F", "CHG-F", "FANNO2");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "V", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "V", "CHG-F", "FANNO2");
			});
			CombineAssertions("With one failed CUSDEC after the successful CUSDEC", () =>
			{
				var cusdecMessage = Factory.New<CUSDECEDIMessage>();
				cusdecMessage.EM_ReceiveTransmit = "TRX";
				cusdecMessage.EM_ApplicationCode = "ZAC";
				cusdecMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
				cusdecMessage.EM_Status = "SNT";
				cusdecMessage.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("RFF+ACD:202'", "RFF+ABI:FANNO2'RFF+ACD:202'").Replace("UNT+109+202'", "UNT+110+202'").Replace("202", "204");
				cusdecMessage.EM_MessageNum = "204";
				header.Messages.Add(cusdecMessage);
				var cusresMessage = Factory.New<CUSRESEDIMessage>();
				cusresMessage.EM_ReceiveTransmit = "RCV";
				cusresMessage.EM_ApplicationCode = "ZAC";
				cusresMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
				cusresMessage.EM_Status = "PRS";
				cusresMessage.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestGIS6MessageWithMRNNumber.Replace("\r\n", "").Replace("202", "204");
				header.Messages.Add(cusresMessage);
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "C", "ORG-C", "3234002347");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "C", "CHG-C", "FANNO2");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "F", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "F", "CHG-F", "FANNO2");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Original, "V", "ORG-F", "3234002346");
				AssertFANNumberWithMessageTypeAndPaymentmethod(header, MessageSubTypeCodes.Codes.Change, "V", "CHG-F", "FANNO2");
			});
		}

		public void TestPartClearanceQuantity()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = ProcedureCodes._20;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction2.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals(2, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			AssertEquals(2, (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertEquals(2, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			AssertEquals(2, (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals(ZInt.Zero, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			AssertEquals(ZInt.Zero, (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceLine1.CusEntryLine.Header.MovementReferenceNumberSetter("MRN_REP", ZDateTime.Now);
			invoiceLine2.CusEntryLine.Header.MovementReferenceNumberSetter("MRN_NOREP", ZDateTime.Now);
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = ProcedureCodes._11;
			testInstruction3.CEI_MRNToBeReplaced = "MRN_REP";
			var testInstruction4 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction4.CEI_Style = ProcedureCodes._11;
			testInstruction4.CEI_MRNToBeReplaced = "MRN_REP_NOTINJOB";
			var invoiceHeader3 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceHeader4 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine3 = invoiceHeader3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = testInstruction3.PK;
			var invoiceLine4 = invoiceHeader4.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = testInstruction4.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals(3, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			AssertEquals(3, (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			AssertEquals(3, (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			AssertEquals(3, (new MessageSendingObject(invoiceLine4.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals(0, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			AssertEquals(0, (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			AssertEquals(0, (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
			AssertEquals(0, (new MessageSendingObject(invoiceLine4.CusEntryLine.Header) as ICUSDECMessageDataProvider).PartClearanceQuantity);
		}

		public void TestHouseBill()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			var header = invoiceLine1.CusEntryLine.Header;

			ZString getHouseBillFromProvider() => (new MessageSendingObject(header) as ICUSDECMessageDataProvider).HouseBill;

			AssertEquals(ZString.Empty, getHouseBillFromProvider());
			declaration.JE_HouseBill = "HOUSEBILL";
			AssertEquals("HOUSEBILL", getHouseBillFromProvider());
			declaration.JE_HouseBill = ZString.Empty;
			declaration.JE_CargoCarrier = "AAA";
			AssertEquals("AAA", getHouseBillFromProvider());
			declaration.JE_HouseBill = "HOUSEBILL";
			AssertEquals("AAA     HOUSEBILL", getHouseBillFromProvider());

			header.EntryInstruction.CEI_HAWBOverride = "OVERRIDE HOUSEBILL";
			header.EntryInstruction.CEI_CargoCarrierOverride = "BBB";
			AssertEquals("BBB     OVERRIDE HOUSEBILL", getHouseBillFromProvider());
		}

		public void TestRelatedPartyIndicator()
		{
			ZAUniversalReferenceTestDataHelper.SetupRefCusMapAndRefCusMapType(Factory);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			invoiceHeader1.JZ_RelatedIndicator = "Y";
			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "2";
			invoiceHeader2.JZ_RelatedIndicator = "N";
			var invoiceHeader3 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader3.JZ_InvoiceNumber = "3";
			invoiceHeader3.JZ_RelatedIndicator = "E";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction.PK;
			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + ProcedureCodes._00;
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction.PK;
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + ProcedureCodes._00;
			var invoiceLine3 = invoiceHeader3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = testInstruction.PK;
			invoiceLine3.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + ProcedureCodes._00;
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("R", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).RelatedPartyIndicator);
				AssertEquals("N", (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).RelatedPartyIndicator);
				AssertEquals("E", (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).RelatedPartyIndicator);
			});
		}

		public void TestToWarehouseAndFromWarehouse()
		{
			var testWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			var orgCode = testWarehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CWPNUMBER", Core.Constants.CountryCodes.SouthAfrica);
			var warehouseAddress = testWarehouse.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			orgCode.OK_OA_PremisesAddress = testWarehouse.MainAddress.PK;
			declaration.JE_RL_NKFinalDestination = "";
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = ProcedureCodes._40;
			testInstruction.CEI_OA_Warehouse = testWarehouse.MainAddress.PK;
			testInstruction.CEI_OA_Warehouse2 = testWarehouse.MainAddress.PK;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction.PK;
			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + ProcedureCodes._00;
			new LineMerger(declaration).DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("From Empty", string.Empty, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).FromWarehouse);
				AssertEquals("To CWPNUMBER", "CWPNUMBER", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).ToWarehouse);
				testInstruction.CEI_Style = ProcedureCodes._15;
				invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + ProcedureCodes._40;
				AssertEquals("From CWPNUMBER", "CWPNUMBER", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).FromWarehouse);
				AssertEquals("To Empty", string.Empty, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).ToWarehouse);
				testInstruction.CEI_Style = ProcedureCodes._41;
				invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + ProcedureCodes._40;
				AssertEquals("From CWPNUMBER 02", "CWPNUMBER", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).FromWarehouse);
				AssertEquals("To CWPNUMBER 02", "CWPNUMBER", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).ToWarehouse);
				testInstruction.CEI_Style = ProcedureCodes._77;
				invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + ProcedureCodes._75;
				AssertEquals("From Empty 02", string.Empty, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).FromWarehouse);
				AssertEquals("To Empty 02", string.Empty, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).ToWarehouse);
				testInstruction.CEI_Style = ProcedureCodes._20;
				invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + ProcedureCodes._00;
				AssertEquals("To Empty 03", string.Empty, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).ToWarehouse);
				AssertEquals("To Empty 03", string.Empty, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).ToWarehouse);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_RL_NKFinalDestination = "BWBBK";
				new LineMerger(declaration).DoMerge();
				AssertEquals("From CWPNUMBER 03", "CWPNUMBER", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).FromWarehouse);
				AssertEquals("To CWPNUMBER 03", "CWPNUMBER", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).ToWarehouse);
			});
		}

		public void TestRemoverTransporterCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "", "", "", "");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", ZAJobMessageTypeList.Codes.Import);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "41", "", "", "", ZAJobMessageTypeList.Codes.ExBond);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "42", "", "", "", ZAJobMessageTypeList.Codes.Import);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "10", "", "", "", ZAJobMessageTypeList.Codes.Import);
			Factory.Save();
			var testRemover = Factory.NewWithValidTestData<OrgHeader>();
			testRemover.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "00111110", Core.Constants.CountryCodes.SouthAfrica);
			testRemover.OH_RL_NKClosestPort = "ZACPT";
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._40;
			testInstruction1.CEI_OH_Carrier = testRemover.PK;
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = ProcedureCodes._11;
			testInstruction2.CEI_OH_Carrier = testRemover.PK;
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = ProcedureCodes._42;
			testInstruction3.CEI_OH_Carrier = testRemover.PK;
			var testInstruction4 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction4.CEI_Style = ProcedureCodes._10;
			testInstruction4.CEI_OH_Carrier = testRemover.PK;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction2.PK;
			var invoiceLine3 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = testInstruction3.PK;
			var invoiceLine4 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = testInstruction4.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals("00111110", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemoverTransporterCode);
			AssertEquals("00111110", (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemoverTransporterCode);
			AssertEquals("00111110", (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemoverTransporterCode);
			AssertEquals("00111110", (new MessageSendingObject(invoiceLine4.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemoverTransporterCode);
			declaration.JE_RL_NKFinalDestination = "LSMSU";
			new LineMerger(declaration).DoMerge();
			AssertEquals("00111110", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemoverTransporterCode);
			AssertEquals("00111110", (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemoverTransporterCode);
			AssertEquals("00111110", (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemoverTransporterCode);
			AssertEquals("00111110", (new MessageSendingObject(invoiceLine4.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemoverTransporterCode);
		}

		public void TestSupplier()
		{
			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = testSupplier.PK;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._10;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			testSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "00111110", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			invoiceHeader1.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Yes;
			invoiceHeader1.JZ_VDN = "112233";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertNotNull("IMP, with VDN", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			invoiceHeader1.JZ_RelatedIndicator = RelatedIndicatorList.Codes.No;
			invoiceHeader1.JZ_VDN = "";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertNotNull("IMP, empty VDN", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
		}

		public void TestRemovalTransportModeForBLNSFinalDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "", "", "", "", group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", ZAJobMessageTypeList.Codes.Import, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "41", "", "", "", ZAJobMessageTypeList.Codes.ExBond, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "42", "", "", "", ZAJobMessageTypeList.Codes.Import, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "10", "", "", "", ZAJobMessageTypeList.Codes.Import, group: "BLNS");
			Factory.Save();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKFinalDestination = "LSMSU";
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._40;
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = ProcedureCodes._11;
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = ProcedureCodes._42;
			var testInstruction4 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction4.CEI_Style = ProcedureCodes._41;
			var testInstruction5 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction5.CEI_Style = ProcedureCodes._10;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction2.PK;
			var invoiceLine3 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = testInstruction3.PK;
			var invoiceLine4 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = testInstruction4.PK;
			var invoiceLine5 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = testInstruction5.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals("3", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("0", (new MessageSendingObject(invoiceLine4.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("", (new MessageSendingObject(invoiceLine5.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
		}

		public void TestRemovalTransportModeForNonBLNSFinalDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "", "", "", "", group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "", "", "", ZAJobMessageTypeList.Codes.Import, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "41", "", "", "", ZAJobMessageTypeList.Codes.ExBond, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "42", "", "", "", ZAJobMessageTypeList.Codes.Import, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "10", "", "", "", ZAJobMessageTypeList.Codes.Import, group: "BLNS");
			Factory.Save();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._40;
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = ProcedureCodes._11;
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = ProcedureCodes._42;
			var testInstruction4 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction4.CEI_Style = ProcedureCodes._41;
			var testInstruction5 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction5.CEI_Style = ProcedureCodes._10;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction2.PK;
			var invoiceLine3 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = testInstruction3.PK;
			var invoiceLine4 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = testInstruction4.PK;
			var invoiceLine5 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = testInstruction5.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals("3", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("0", (new MessageSendingObject(invoiceLine4.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("", (new MessageSendingObject(invoiceLine5.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			declaration.JE_RL_NKFinalDestination = "LSMSU";
			new LineMerger(declaration).DoMerge();
			AssertEquals("3", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("0", (new MessageSendingObject(invoiceLine4.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("", (new MessageSendingObject(invoiceLine5.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
		}

		public void TestRemovalTransportModeForExports()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "", "", "", ZAJobMessageTypeList.Codes.Import, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "F", "52", "", "", "", ZAJobMessageTypeList.Codes.Export, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "F", "53", "", "", "", ZAJobMessageTypeList.Codes.Export, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "67", "", "", "", ZAJobMessageTypeList.Codes.Export, group: "BLNS");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H", "68", "", "", "", ZAJobMessageTypeList.Codes.Export, group: "BLNS");
			Factory.Save();
			declaration.JE_MessageType = "EXP";
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Other;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._40;
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = ProcedureCodes._52;
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = ProcedureCodes._53;
			var testInstruction4 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction4.CEI_Style = ProcedureCodes._67;
			var testInstruction5 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction5.CEI_Style = ProcedureCodes._68;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction2.PK;
			var invoiceLine3 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = testInstruction3.PK;
			var invoiceLine4 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = testInstruction4.PK;
			var invoiceLine5 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = testInstruction5.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals("", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("0", (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("0", (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("0", (new MessageSendingObject(invoiceLine4.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("0", (new MessageSendingObject(invoiceLine5.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			AssertEquals("", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine4.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
			AssertEquals("3", (new MessageSendingObject(invoiceLine5.CusEntryLine.Header) as ICUSDECMessageDataProvider).RemovalTransportMode);
		}

		[TestDate(2016, 01, 01)]
		public void TestInvoiceInformations()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = ProcedureCodes._10;
			var testHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testHeader.JZ_InvoiceNumber = "1";
			testHeader.JZ_InvoiceDate = ZDateTime.Today;
			var testLine = testHeader.JobComInvoiceLines.AddNew();
			testLine.JI_CEI = testInstruction.PK;
			new LineMerger(declaration).DoMerge();
			var header = testLine.CusEntryLine.Header;
			var sendingObject = new MessageSendingObject(header);
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			var invoiceInfo = (sendingObject as ICUSDECMessageDataProvider).InvoiceInformations.Cast<IInvoiceInformation>().First();
			AssertEquals("1", invoiceInfo.InvoiceNumber);
			AssertEquals(ZDateTime.Today, invoiceInfo.InvoiceDate);
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			invoiceInfo = (sendingObject as ICUSDECMessageDataProvider).InvoiceInformations.Cast<IInvoiceInformation>().First();
			AssertEquals("1", invoiceInfo.InvoiceNumber);
			AssertEquals(ZDateTime.Today, invoiceInfo.InvoiceDate);
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Replace;
			invoiceInfo = (sendingObject as ICUSDECMessageDataProvider).InvoiceInformations.Cast<IInvoiceInformation>().First();
			AssertEquals("1", invoiceInfo.InvoiceNumber);
			AssertEquals(ZDateTime.Today, invoiceInfo.InvoiceDate);
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			invoiceInfo = (sendingObject as ICUSDECMessageDataProvider).InvoiceInformations.Cast<IInvoiceInformation>().First();
			AssertEquals("1", invoiceInfo.InvoiceNumber);
			AssertEquals(ZDateTime.Today, invoiceInfo.InvoiceDate);
			var cusdecMessage = Factory.New<CUSDECEDIMessage>();
			cusdecMessage.EM_ReceiveTransmit = "TRX";
			cusdecMessage.EM_ApplicationCode = "ZAC";
			cusdecMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			cusdecMessage.EM_Status = "SNT";
			cusdecMessage.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "");
			cusdecMessage.EM_MessageNum = "202";
			header.Messages.Add(cusdecMessage);
			var cusresMessage = Factory.New<CUSRESEDIMessage>();
			cusresMessage.EM_ReceiveTransmit = "RCV";
			cusresMessage.EM_ApplicationCode = "ZAC";
			cusresMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
			cusresMessage.EM_Status = "PRS";
			cusresMessage.EM_MessageText = ZAMessageTest.CUSRESTestMessage.Replace("\r\n", "");
			header.Messages.Add(cusresMessage);
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			invoiceInfo = (sendingObject as ICUSDECMessageDataProvider).InvoiceInformations.Cast<IInvoiceInformation>().First();
			AssertEquals("INVH2FOB", invoiceInfo.InvoiceNumber);
			AssertEquals(new ZDateTime(2016, 4, 15), invoiceInfo.InvoiceDate);
		}

		public void TestLineLevelDetails()
		{
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._40;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			MessageSendingObject messageSendingObject = new MessageSendingObject(invoiceLine1.CusEntryLine.Header);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			ICUSDECMessageDataProvider provider = messageSendingObject;
			AssertNotNull("Line Level Details must be sent with Original", provider.LineLevelDetails);
			foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
			{
				messageSendingObject.MessageKeyFactor.DeclarationType = declarationTypePair.Code;
				foreach (var providerLineLevelDetail in provider.LineLevelDetails)
				{
					AssertEquals(declarationTypePair.Code, ((CusEntryLine)providerLineLevelDetail).MessageKeyFactor.DeclarationType);
				}
			}

			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			AssertNull("Line Level Details must no be sent with Cancellation", provider.LineLevelDetails);
		}

		public void TestTransportNameSea()
		{
			var newFactory = new BusinessObjectFactory();
			var systemVessel = newFactory.NewWithValidTestData<RefVesselZZ>();
			systemVessel.ZZO_Code = "TEST VESSEL";
			systemVessel.ZZO_RadioCallSign = "123321";
			newFactory.Save();
			var overrideVessel = Factory.NewWithValidTestData<RefVessel>();
			overrideVessel.RV_Code = "TEST VESSEL";
			overrideVessel.RV_RadioCallSign = "1234567";
			overrideVessel.RV_CarrierCode = "COS";
			newFactory.Save();
			var longNameVessel = Factory.NewWithValidTestData<RefVessel>();
			longNameVessel.RV_Code = "TEST VESSEL WITH A VERY LONG NAME";
			longNameVessel.RV_RadioCallSign = "1234567";
			longNameVessel.RV_CarrierCode = "AR1";
			Factory.Save();
			var vessel = RefVessel.LookupVesselByCode("TEST VESSEL", Factory);
			AssertEquals(overrideVessel.PK, vessel.PK);
			Assert(vessel.HasSystemVessel);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_VoyageFlightNo = "AC75PPGP";
			declaration.JE_VesselName = "TEST VESSEL";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals("COS 123321   TEST VESSEL", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).TransportName);
			declaration.JE_VesselName = "TEST BLAS 3";
			declaration.JE_RadioCallSign = "1234567";
			declaration.JE_Carrier = "ZDZ";
			AssertEquals("ZDZ 1234567  TEST BLAS 3", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).TransportName);

			declaration.JE_RadioCallSign = "1234567";
			declaration.JE_Carrier = "AR1";
			declaration.JE_VesselName = "TEST VESSEL WITH A VERY LONG NAME";
			AssertEquals("AR1 1234567  TEST VESSEL WITH A VER", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).TransportName);
		}

		public void TestTransportNameRoad()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_VoyageFlightNo = "AC75PPGP";
			declaration.JE_Trailer1 = "BC75PPGP";
			declaration.JE_Trailer2 = "CC75PPGP";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals("AC75PPGP  BC75PPGP  CC75PPGP  ", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).TransportName);
		}

		public void TestTransportDocumentNumber_SEA_IMP()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "MSCU123450";
			declaration.JE_CarrierCode = "MSC";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._40;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				AssertEquals("MSCU123450", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).TransportDocumentNumber);
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				AssertEquals("MSC MSCU123450", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).TransportDocumentNumber);
			}
		}

		public void TestTransportDocumentNumberDateIssuedAt()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "08300000001";
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 6, 16);
			declaration.JE_RL_NKMasterBillIssuedAt = "ZAJNB";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._40;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals(declaration.TransportDocumentNumber, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).TransportDocumentNumber);
			AssertEquals(declaration.JE_MasterBillIssuedDate, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).TransportDocumentDate);
			AssertEquals(declaration.JE_RL_NKMasterBillIssuedAt, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).TransportDocumentIssuedAt);
		}

		public void TestTotalLineCount()
		{
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._40;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			MessageSendingObject messageSendingObject = new MessageSendingObject(invoiceLine1.CusEntryLine.Header);
			var messageTypes = new string[] { MessageSubTypeCodes.Codes.Original, MessageSubTypeCodes.Codes.Change, MessageSubTypeCodes.Codes.Replace };
			foreach (var messageType in messageTypes)
			{
				messageSendingObject.MessageType = messageType;
				var provider1 = (messageSendingObject as ICUSDECMessageDataProvider);
				AssertEquals("Total Line Count must be sent with " + messageType, 1, provider1.TotalLineCount);
			}

			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			var provider2 = (messageSendingObject as ICUSDECMessageDataProvider);
			AssertEquals("Total Line Count must be 0 for Cancellation", 0, provider2.TotalLineCount);
		}

		public void TestOriginalMRN()
		{
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._40;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			MessageSendingObject messageSendingObject = new MessageSendingObject(invoiceLine1.CusEntryLine.Header);
			messageSendingObject.MovementReferenceNumber = "JSA201607011234567";
			var messageTypes = new string[] { MessageSubTypeCodes.Codes.Original };
			foreach (var messageType in messageTypes)
			{
				messageSendingObject.MessageType = messageType;
				var provider1 = (messageSendingObject as ICUSDECMessageDataProvider);
				AssertEquals("Original MRN must be empty with " + messageType, ZString.Empty, provider1.OriginalMRN);
			}

			messageTypes = new string[] { MessageSubTypeCodes.Codes.Change, MessageSubTypeCodes.Codes.Cancellation };
			foreach (var messageType in messageTypes)
			{
				messageSendingObject.MessageType = messageType;
				var provider1 = (messageSendingObject as ICUSDECMessageDataProvider);
				AssertEquals("Original MRN must be sent with " + messageType, messageSendingObject.MovementReferenceNumber, provider1.OriginalMRN);
			}

			messageTypes = new string[] { MessageSubTypeCodes.Codes.Replace };
			foreach (var messageType in messageTypes)
			{
				messageSendingObject.MessageType = messageType;
				var provider1 = (messageSendingObject as ICUSDECMessageDataProvider);
				AssertEquals("Original MRN must NOT be sent with " + messageType, ZString.Empty, provider1.OriginalMRN);
				AssertEquals("MRNToBeReplaced must be sent with " + messageType, messageSendingObject.MovementReferenceNumber, provider1.MRNToBeReplaced);
			}
		}

		public void TestMRNToBeReplaced()
		{
			const string Cancellation = MessageSubTypeCodes.Codes.Cancellation; // "CNL";
			const string Change = MessageSubTypeCodes.Codes.Change; // "CHG";
			const string Original = MessageSubTypeCodes.Codes.Original; // "ORG";
			const string Replace = MessageSubTypeCodes.Codes.Replace; // "REP";

			var scenarios = new MrnToBeReplaced_TestCase_Collection();
			scenarios.Add("01", Cancellation, mustOutputOriginalMRN: true, mustOutputMRNToBeReplaced: true);
			scenarios.Add("02", Change, mustOutputOriginalMRN: true, mustOutputMRNToBeReplaced: true);
			scenarios.Add("03", Original, mustOutputOriginalMRN: false, mustOutputMRNToBeReplaced: false);
			scenarios.Add("04", Replace, mustOutputOriginalMRN: false, mustOutputMRNToBeReplaced: true);
			scenarios.TestAll(Factory);
		}

		public void TestPortOfExit()
		{
			var portOfExit = "KFN";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._20;
			testInstruction1.CEI_PortOfExit = portOfExit;
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = ProcedureCodes._21;
			testInstruction2.CEI_PortOfExit = portOfExit;
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = ProcedureCodes._11;
			testInstruction3.CEI_PortOfExit = portOfExit;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction2.PK;
			var invoiceLine3 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = testInstruction3.PK;
			new LineMerger(declaration).DoMerge();
			AssertEquals(portOfExit, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).PortOfExit);
			AssertEquals(portOfExit, (new MessageSendingObject(invoiceLine2.CusEntryLine.Header) as ICUSDECMessageDataProvider).PortOfExit);
			AssertEquals(portOfExit, (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).PortOfExit);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var transportModes = new string[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road };
			foreach (var transportMode in transportModes)
			{
				declaration.JE_TransportMode = transportMode;
				new LineMerger(declaration).DoMerge();
				AssertEquals(portOfExit, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).PortOfExit);
			}

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			declaration.JE_RL_NKFinalDestination = "LSMSU";
			AssertEquals(portOfExit, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).PortOfExit);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "LSMSU";
			AssertEquals(portOfExit, (new MessageSendingObject(invoiceLine3.CusEntryLine.Header) as ICUSDECMessageDataProvider).PortOfExit);
		}

		public void TestPortOfExitEmptyInstruction()
		{
			var portOfExit = "KFN";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._20;
			testInstruction1.CEI_PortOfExit = portOfExit;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			invoiceLine1.CusEntryLine.Header.CH_CEI_Instruction = ZGuid.Empty;
			AssertNoExceptionThrown(() =>
			{
				var messageSendingObject = new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider;
				var testPortOfExit = messageSendingObject.PortOfExit;
				AssertEquals(ZString.Empty, testPortOfExit);
			});
		}

		public void TestOverrideCustomsOffice()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "JHB";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._20;
			testInstruction1.CEI_CustomsOfficeOverride = "KFN";
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();

			CombineAssertions(() =>
			{
				var messageSendingObject = new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider;
				AssertEquals("Has CustomsOfficeOverride", "KFN", messageSendingObject.CustomsOfficeCode);

				testInstruction1.CEI_CustomsOfficeOverride = "";
				messageSendingObject = new MessageSendingObject(invoiceLine1.CusEntryLine.Header);
				AssertEquals("No CustomsOfficeOverride then fallback to JE_CustomsOffice", "JHB", messageSendingObject.CustomsOfficeCode);
			});
		}

		public void TestDateOfAssessment()
		{
			var assessmentDate = new ZDateTime(2016, 08, 02);
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._20;
			testInstruction1.CEI_DateForDuty = assessmentDate;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			new LineMerger(declaration).DoMerge();
			var sender = new MessageSendingObject(invoiceLine1.CusEntryLine.Header);
			sender.MessageType = MessageSubTypeCodes.Codes.Original;
			AssertEquals(ZDateTime.Empty, (sender as ICUSDECMessageDataProvider).DateOfAssessment);
			sender.MessageType = MessageSubTypeCodes.Codes.Change;
			AssertEquals(assessmentDate, (sender as ICUSDECMessageDataProvider).DateOfAssessment);
		}

		public void TestLocationOfGoods()
		{
			declaration.JE_LocationOfGoods = "64";
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var shipmentTypes = new string[] { ZAJobMessageTypeList.Codes.Import, ZAJobMessageTypeList.Codes.Export };
			var transportModes = new string[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea };
			foreach (var shipmentType in shipmentTypes)
			{
				declaration.JE_MessageType = shipmentType;
				foreach (var transportMode in transportModes)
				{
					declaration.JE_TransportMode = transportMode;
					new LineMerger(declaration).DoMerge();
					AssertEquals("64", (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).LocationOfGoods);
				}
			}
		}

		public void TestDateOfDepartureOrDateOfFlight()
		{
			var dateofArrival = ZDateTime.Now;
			declaration.JE_DateOfArrival = dateofArrival;
			var dateofDeparture = ZDateTime.Now;
			declaration.JE_ExportDate = dateofDeparture;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var shipmentTypes = new string[] { ZAJobMessageTypeList.Codes.Export, ZAJobMessageTypeList.Codes.ExBond };
			var transportModes = new string[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road };
			foreach (var shipmentType in shipmentTypes)
			{
				declaration.JE_MessageType = shipmentType;
				foreach (var transportMode in transportModes)
				{
					declaration.JE_TransportMode = transportMode;
					new LineMerger(declaration).DoMerge();
					AssertEquals(shipmentType + " " + transportMode, dateofDeparture, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).DateOfDepartureOrDateOfFlight);
				}
			}

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			new LineMerger(declaration).DoMerge();
			AssertEquals(dateofArrival, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).DateOfDepartureOrDateOfFlight);
		}

		public void TestDateOfArrival()
		{
			var dateofArrival = ZDateTime.Now;
			declaration.JE_DateOfArrival = dateofArrival;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = ProcedureCodes._11;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var shipmentTypes = new string[] { ZAJobMessageTypeList.Codes.Import };
			var transportModes = new string[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road };
			foreach (var shipmentType in shipmentTypes)
			{
				declaration.JE_MessageType = shipmentType;
				foreach (var transportMode in transportModes)
				{
					declaration.JE_TransportMode = transportMode;
					new LineMerger(declaration).DoMerge();
					AssertEquals(dateofArrival, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).DateOfArrival);
				}
			}

			shipmentTypes = new string[] { ZAJobMessageTypeList.Codes.Export, ZAJobMessageTypeList.Codes.ExBond };
			foreach (var shipmentType in shipmentTypes)
			{
				declaration.JE_MessageType = shipmentType;
				new LineMerger(declaration).DoMerge();
				AssertEquals(ZDateTime.Empty, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).DateOfArrival);
			}
		}

		public void TestImporter()
		{
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_ProcedureCode = ProcedureCodes._41;
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure2.ZZ6_ProcedureCode = ProcedureCodes._36;
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			var entry = testDeclaration.ActiveEntryHeaders.AddNew();
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.OH_FullName = "Importer Full Name";
			testDeclaration.JE_OH_Importer = testImporter.PK;
			var testInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceLine = testDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = testInstruction.PK;
			testInstruction.CEI_Style = ProcedureCodes._41;
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "Test Org Full Name";
			testInstruction.CEI_OH_Owner = testOrg.PK;
			entry.CH_CEI_Instruction = testInstruction.PK;
			new LineMerger(testDeclaration).DoMerge();
			AssertEquals("Importer", testImporter.OH_FullName, (new MessageSendingObject(entry) as ICUSDECMessageDataProvider).Importer.Name);
			testInstruction.CEI_Style = ProcedureCodes._36;
			new LineMerger(testDeclaration).DoMerge();
			AssertEquals("Importer", testImporter.OH_FullName, (new MessageSendingObject(entry) as ICUSDECMessageDataProvider).Importer.Name);
		}

		public void TestImporterForExportJob()
		{
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_ProcedureCode = ProcedureCodes._41;
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure2.ZZ6_ProcedureCode = ProcedureCodes._36;
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entry = testDeclaration.ActiveEntryHeaders.AddNew();
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.OH_FullName = "Importer Full Name";
			testDeclaration.JE_OH_Importer = testImporter.PK;
			var testInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceLine = testDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = testInstruction.PK;
			testInstruction.CEI_Style = ProcedureCodes._41;
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "Test Org Full Name";
			testInstruction.CEI_OH_Owner = testOrg.PK;
			entry.CH_CEI_Instruction = testInstruction.PK;
			new LineMerger(testDeclaration).DoMerge();
			AssertEquals("TestImporterForExportJob", testImporter.OH_FullName, (new MessageSendingObject(entry) as ICUSDECMessageDataProvider).ImporterForExportJob.Name);
			testInstruction.CEI_Style = ProcedureCodes._36;
			new LineMerger(testDeclaration).DoMerge();
			AssertEquals("TestImporterForExportJob", testImporter.OH_FullName, (new MessageSendingObject(entry) as ICUSDECMessageDataProvider).ImporterForExportJob.Name);
		}

		public void TestOwnerCode()
		{
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_ProcedureCode = ProcedureCodes._41;
			procedure1.ZZ6_Description = "41";
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "00000001", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_Importer = testImporter.PK;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = ProcedureCodes._41;
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "Test Org Full Name";
			testOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "00000002", Core.Constants.CountryCodes.SouthAfrica);
			testInstruction.CEI_OH_Owner = testOrg.PK;
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine = testInvHeader.InvoiceLines.AddNew();
			testInvLine.JI_CEI = testInstruction.PK;
			new LineMerger(declaration).DoMerge();
			var testHeader = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			AssertEquals("Not EXW", "", (new MessageSendingObject(testHeader) as ICUSDECMessageDataProvider).OwnerCode);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			new LineMerger(declaration).DoMerge();
			AssertEquals("EXW - Change of ownership", "00000002", (new MessageSendingObject(testHeader) as ICUSDECMessageDataProvider).OwnerCode);

			testInstruction.CEI_Style = ProcedureCodes._11;
			testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + ProcedureCodes._40;
			new LineMerger(declaration).DoMerge();
			AssertEquals("EXW - NOT change of ownership", "00000001", (new MessageSendingObject(testHeader) as ICUSDECMessageDataProvider).OwnerCode);
		}

		public void TestTotalDutiesDue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			invLine1.JI_ZZF_NKTaxType = "VAT";
			var invLine2 = invoiceHeader.InvoiceLines.AddNew();
			invLine2.JI_ZZF_NKTaxType = "VAT";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate("1P1", 10m);
			entryLine1.Fees.AddOrUpdate("13A", 20m);
			entryLine1.Fees.AddOrUpdate("13C", 30m);
			entryLine1.Fees.AddOrUpdate("15B", 40m);
			entryLine1.Fees.AddOrUpdate("13D", 50m);
			entryLine1.Fees.AddOrUpdate("13B", 60m);
			entryLine1.Fees.AddOrUpdate("15A", 70m);
			entryLine1.Fees.AddOrUpdate("VAT", 80m);
			entryLine1.Fees.AddOrUpdate("PPA", 90m);
			entryLine1.InvoiceLines.Add(invLine1);
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate("1P1", 11m);
			entryLine2.Fees.AddOrUpdate("13A", 21m);
			entryLine2.Fees.AddOrUpdate("13C", 31m);
			entryLine2.Fees.AddOrUpdate("15B", 41m);
			entryLine2.Fees.AddOrUpdate("13D", 51m);
			entryLine2.Fees.AddOrUpdate("13B", 61m);
			entryLine2.Fees.AddOrUpdate("15A", 81m);
			entryLine2.Fees.AddOrUpdate("VAT", 91m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 13.00m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 23.00m);
			entryLine2.ProvisionalPayments.AddNew("PPC", 33.00m);
			entryLine2.ProvisionalPayments.AddNew("PEN", 43.11m);
			entryLine2.ProvisionalPayments.AddNew("FOR", 53.22m);
			entryLine2.ProvisionalPayments.AddNew("XXX", 63.00m);
			entryLine2.InvoiceLines.Add(invLine2);
			CombineAssertions(() =>
			{
				var tester = new MessageSendingObject(entry);
				var testerAsIVOCAfterValues = tester as IVOCAfterValues;
				AssertEquals("DTY", 577m, testerAsIVOCAfterValues.CustomsDutyNoS1P2B);
				AssertEquals("12B", 0m, testerAsIVOCAfterValues.S1P2BDuty);
				AssertEquals("VAT", 171m, testerAsIVOCAfterValues.ValueAddedTax);
				AssertEquals("PRP", 69m, testerAsIVOCAfterValues.ProvisionalPaymentAmount);
				AssertEquals("PEN", 96.33m, testerAsIVOCAfterValues.PenaltyAmount);
				AssertEquals(742.33m, (tester as ICUSDECMessageDataProvider).TotalDutiesDue);
				foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
				{
					tester.MessageKeyFactor.DeclarationType = declarationTypePair.Code;
					switch (declarationTypePair.Code)
					{
						case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
						case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
							AssertEquals(0m, (tester as ICUSDECMessageDataProvider).TotalDutiesDue);
							break;
						default:
							AssertEquals(742.33m, (tester as ICUSDECMessageDataProvider).TotalDutiesDue);
							break;
					}
				}
			});
		}

		public void TestTotalDutiesDueForDiamondLevyAmount()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.ProvisionalPayments.AddNew("DLA", "111.11");
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.ProvisionalPayments.AddNew("DLA", "222.22");
			CombineAssertions(() =>
			{
				var tester = new MessageSendingObject(entry);
				foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
				{
					tester.MessageKeyFactor.DeclarationType = declarationTypePair.Code;
					switch (declarationTypePair.Code)
					{
						case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
						case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
							AssertEquals(0m, (tester as ICUSDECMessageDataProvider).TotalDutiesDue);
							break;
						default:
							AssertEquals(333.33m, (tester as ICUSDECMessageDataProvider).TotalDutiesDue);
							break;
					}
				}
			});
		}

		public void TestTotalVATDue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			invLine1.JI_ZZF_NKTaxType = "VAT";
			var invLine2 = invoiceHeader.InvoiceLines.AddNew();
			invLine2.JI_ZZF_NKTaxType = "VAT";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate("1P1", 10m);
			entryLine1.Fees.AddOrUpdate("13A", 20m);
			entryLine1.Fees.AddOrUpdate("13C", 30m);
			entryLine1.Fees.AddOrUpdate("15B", 40m);
			entryLine1.Fees.AddOrUpdate("13D", 50m);
			entryLine1.Fees.AddOrUpdate("13B", 60m);
			entryLine1.Fees.AddOrUpdate("15A", 70m);
			entryLine1.Fees.AddOrUpdate("VAT", 80m);
			entryLine1.Fees.AddOrUpdate("PPA", 90m);
			entryLine1.InvoiceLines.Add(invLine1);
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate("1P1", 11m);
			entryLine2.Fees.AddOrUpdate("13A", 21m);
			entryLine2.Fees.AddOrUpdate("13C", 31m);
			entryLine2.Fees.AddOrUpdate("15B", 41m);
			entryLine2.Fees.AddOrUpdate("13D", 51m);
			entryLine2.Fees.AddOrUpdate("13B", 61m);
			entryLine2.Fees.AddOrUpdate("15A", 81m);
			entryLine2.Fees.AddOrUpdate("VAT", 91m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 13.00m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 23.00m);
			entryLine2.ProvisionalPayments.AddNew("PPC", 33.00m);
			entryLine2.ProvisionalPayments.AddNew("PEN", 43.11m);
			entryLine2.ProvisionalPayments.AddNew("FOR", 53.22m);
			entryLine2.ProvisionalPayments.AddNew("XXX", 63.00m);
			entryLine2.InvoiceLines.Add(invLine2);
			CombineAssertions(() =>
			{
				var tester = new MessageSendingObject(entry);
				var testerAsIVOCAfterValues = tester as IVOCAfterValues;
				AssertEquals("DTY", 577m, testerAsIVOCAfterValues.CustomsDutyNoS1P2B);
				AssertEquals("12B", 0m, testerAsIVOCAfterValues.S1P2BDuty);
				AssertEquals("VAT", 171m, testerAsIVOCAfterValues.ValueAddedTax);
				AssertEquals("PRP", 69m, testerAsIVOCAfterValues.ProvisionalPaymentAmount);
				AssertEquals("PEN", 96.33m, testerAsIVOCAfterValues.PenaltyAmount);
				AssertEquals(171m, (tester as ICUSDECMessageDataProvider).TotalVATDue);
				foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
				{
					tester.MessageKeyFactor.DeclarationType = declarationTypePair.Code;
					switch (declarationTypePair.Code)
					{
						case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
						case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
							AssertEquals(declarationTypePair.Code, 0m, (tester as ICUSDECMessageDataProvider).TotalVATDue);
							break;
						default:
							AssertEquals(declarationTypePair.Code, 171m, (tester as ICUSDECMessageDataProvider).TotalVATDue);
							break;
					}
				}
			});
		}

		public void TestContainers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var container1 = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();
			new LineMerger(declaration).DoMerge();
			var testHeader = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			var containers = (new MessageSendingObject(testHeader) as ICUSDECMessageDataProvider).Containers;
			AssertEquals("Count", 2, containers.Count());
			AssertCollectionContains(container1, containers);
			AssertCollectionContains(container2, containers);
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			new LineMerger(declaration).DoMerge();
			testHeader = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			containers = (new MessageSendingObject(testHeader) as ICUSDECMessageDataProvider).Containers;
			AssertEquals("Count", 0, containers.Count());
			invoiceLine1.JI_CEI = instruction1.PK;
			var pivot1 = invoiceLine1.ContainersPivot.AddNew();
			pivot1.C2_CO = container1.PK;
			new LineMerger(declaration).DoMerge();
			testHeader = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			containers = (new MessageSendingObject(testHeader) as ICUSDECMessageDataProvider).Containers;
			AssertEquals("Count", 1, containers.Count());
			AssertCollectionContains(container1, containers);
			AssertCollectionNotContains(container2, containers);
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction1.PK;
			var pivot2 = invoiceLine2.ContainersPivot.AddNew();
			pivot2.C2_CO = container1.PK;
			new LineMerger(declaration).DoMerge();
			testHeader = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			containers = (new MessageSendingObject(testHeader) as ICUSDECMessageDataProvider).Containers;
			AssertEquals("Count", 1, containers.Count());
			AssertCollectionContains(container1, containers);
		}

		public void TestCountryOfOrigin()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			RefUNLOCO originNLOCO = Factory.New<RefUNLOCO>();
			originNLOCO.RL_Code = "ZAAM";
			declaration.JE_RL_NKOrigin = originNLOCO.RL_Code;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var messageSendingObject = new MessageSendingObject(entryHeader);
			var countryOfOrigin = messageSendingObject.MessageKeyFactor.CountryOfOrigin;
			AssertEquals("ZA", countryOfOrigin.Code);
		}

		public void TestNADDTIDOisInTheMessageIfIDOSisGivenInExportWhateverCPCValue()
		{
			OrgHeader testSupplier;
			CusEntryInstruction testInstruction1;
			JobComInvoiceLine invoiceLine1;
			CusEntryHeader entry;
			DeclarationForNADDT(out testSupplier, out testInstruction1, out invoiceLine1, out entry);
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			testSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "70707070", Core.Constants.CountryCodes.SouthAfrica);
			testSupplier.SetLocalCustomsCode(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "1234567890128");
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			var messageKeyFactor = CreateMessageDataProviderKeyFactor(testInstruction1, ZAJobMessageTypeList.Codes.Export);
			Assert(Inst.ShouldOutputSupplierUnregisteredTrader(messageKeyFactor));
			var testSendingObject = new MessageSendingObject(entry)
			{ MessageType = MessageSubTypeCodes.Codes.Original };
			var builder = new CUSDECMessageBuilder(testSendingObject, Common.MessageBuilders.MessageSubTypes.Create);
			var result = builder.PopulateMessages().GetBuilderResults().ToArray();
			AssertEquals(1, result.Length);
			AssertContains("NAD+DT+", result[0].Message.EM_MessageText);
			AssertContains("1234567890128", result[0].Message.EM_MessageText);
		}

		public void TestNADDTPASisInTheMessageIfPASisGivenInExportWhateverCPCValue()
		{
			OrgHeader testSupplier;
			CusEntryInstruction testInstruction1;
			JobComInvoiceLine invoiceLine1;
			CusEntryHeader entry;
			DeclarationForNADDT(out testSupplier, out testInstruction1, out invoiceLine1, out entry);
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			testSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "70707070", Core.Constants.CountryCodes.SouthAfrica);
			testSupplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.PassportID, "1234567890129");
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			var messageKeyFactor = CreateMessageDataProviderKeyFactor(testInstruction1, ZAJobMessageTypeList.Codes.Export);
			Assert(Inst.ShouldOutputSupplierUnregisteredTrader(messageKeyFactor));
			var testSendingObject = new MessageSendingObject(entry)
			{ MessageType = MessageSubTypeCodes.Codes.Original };
			var builder = new CUSDECMessageBuilder(testSendingObject, Common.MessageBuilders.MessageSubTypes.Create);
			var result = builder.PopulateMessages().GetBuilderResults().ToArray();
			AssertEquals(1, result.Length);
			AssertContains("NAD+DT+", result[0].Message.EM_MessageText);
			AssertContains("1234567890129", result[0].Message.EM_MessageText);
		}

		public void TestNADDTisNotInTheMessageIfPASOrIDOareNotGivenInExportWhateverCPCValue()
		{
			OrgHeader testSupplier;
			CusEntryInstruction testInstruction1;
			JobComInvoiceLine invoiceLine1;
			CusEntryHeader entry;
			DeclarationForNADDT(out testSupplier, out testInstruction1, out invoiceLine1, out entry);
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			testSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "70707070", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			var messageKeyFactor = CreateMessageDataProviderKeyFactor(testInstruction1, ZAJobMessageTypeList.Codes.Export);
			Assert(Inst.ShouldOutputSupplierUnregisteredTrader(messageKeyFactor));
			var testSendingObject = new MessageSendingObject(entry)
			{ MessageType = MessageSubTypeCodes.Codes.Original };
			var builder = new CUSDECMessageBuilder(testSendingObject, Common.MessageBuilders.MessageSubTypes.Create);
			var result = builder.PopulateMessages().GetBuilderResults().ToArray();
			AssertEquals(1, result.Length);
			AssertNotContains("NAD+DT+", result[0].Message.EM_MessageText);
			AssertContains("NAD+EX+", result[0].Message.EM_MessageText);
			AssertContains("70707070+", result[0].Message.EM_MessageText);
		}

		public void TestNADDTPASisInTheMessageIfPASAndIDOisGivenInExportWhateverCPCValue()
		{
			OrgHeader testSupplier;
			CusEntryInstruction testInstruction1;
			JobComInvoiceLine invoiceLine1;
			CusEntryHeader entry;
			DeclarationForNADDT(out testSupplier, out testInstruction1, out invoiceLine1, out entry);
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			testSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "70707070", Core.Constants.CountryCodes.SouthAfrica);
			testSupplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.PassportID, "1234567890128");
			testSupplier.SetLocalCustomsCode(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "1234567890129");
			AssertEquals(null, (new MessageSendingObject(invoiceLine1.CusEntryLine.Header) as ICUSDECMessageDataProvider).Supplier);
			var messageKeyFactor = CreateMessageDataProviderKeyFactor(testInstruction1, ZAJobMessageTypeList.Codes.Export);
			Assert(Inst.ShouldOutputSupplierUnregisteredTrader(messageKeyFactor));
			var testSendingObject = new MessageSendingObject(entry)
			{ MessageType = MessageSubTypeCodes.Codes.Original };
			var builder = new CUSDECMessageBuilder(testSendingObject, Common.MessageBuilders.MessageSubTypes.Create);
			var result = builder.PopulateMessages().GetBuilderResults().ToArray();
			AssertEquals(1, result.Length);
			AssertContains("NAD+DT+", result[0].Message.EM_MessageText);
			AssertContains("1234567890129", result[0].Message.EM_MessageText);
		}

		public void TestShouldOutputInvoiceDetails()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "45", "00", "", "", ZAJobMessageTypeList.Codes.ExBond);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "46", "00", "", "", ZAJobMessageTypeList.Codes.ExBond);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "47", "00", "", "", ZAJobMessageTypeList.Codes.ExBond);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			new LineMerger(declaration).DoMerge();

			var entryHeader = declaration.ActiveEntryHeaders[0];
			ICUSDECMessageDataProvider provider;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, false))
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				provider = new MessageSendingObject(entryHeader);
				AssertEquals(false, provider.ShouldOutputInvoiceDetails);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				provider = new MessageSendingObject(entryHeader);
				AssertEquals(false, provider.ShouldOutputInvoiceDetails);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				entryInstruction.CEI_Style = "46";
				provider = new MessageSendingObject(entryHeader);
				AssertEquals(false, provider.ShouldOutputInvoiceDetails);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				provider = new MessageSendingObject(entryHeader);
				AssertEquals(true, provider.ShouldOutputInvoiceDetails);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				provider = new MessageSendingObject(entryHeader);
				AssertEquals(true, provider.ShouldOutputInvoiceDetails);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				entryInstruction.CEI_Style = "45";
				provider = new MessageSendingObject(entryHeader);
				AssertEquals(false, provider.ShouldOutputInvoiceDetails);

				entryInstruction.CEI_Style = "46";
				provider = new MessageSendingObject(entryHeader);
				AssertEquals(true, provider.ShouldOutputInvoiceDetails);

				entryInstruction.CEI_Style = "47";
				provider = new MessageSendingObject(entryHeader);
				AssertEquals(true, provider.ShouldOutputInvoiceDetails);
			}
		}

		public void TestPaymentTerms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			new LineMerger(declaration).DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];

			entryInstruction.CEI_CreditTerms = "1";
			invoice.JZ_PaymentTerms = "2";

			var wrapper = (ICUSDECMessageDataProvider)new MessageSendingObject(entryHeader);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: false))
			{
				AssertEquals("ZAINVDET off, PaymentTerms comes from CEI_CreditTerms", "1", wrapper.PaymentTerms);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: true))
			{
				AssertEquals("ZAINVDET on, PaymentTerms comes from JZ_PaymentTerms", "2", wrapper.PaymentTerms);
			}
		}

		[TestDate(2024, 11, 1)]
		public void TestExchangeRateDateTime()
		{
			using var func = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: true);

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			ICUSDECMessageDataProvider provider = new MessageSendingObject(entryHeader);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_MasterBillIssuedDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, provider.ExchangeRateDateTime);

			declaration.JE_MasterBillIssuedDate = new ZDateTime(2024, 8, 1);
			AssertEquals(new ZDateTime(2024, 8, 1), provider.ExchangeRateDateTime);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_ValuationDate = ZDate.Empty;
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2024, 10, 31), provider.ExchangeRateDateTime);

			entryHeader.CH_EntrySubmittedDate = ZDateTime.MinSmallDateTimeValue;
			AssertEquals(ZDateTime.Empty, provider.ExchangeRateDateTime);

			entryHeader.CH_EntrySubmittedDate = new ZDateTime(2024, 9, 1);
			AssertEquals(new ZDateTime(2024, 8, 31), provider.ExchangeRateDateTime);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Miscellaneous;
			AssertEquals(ZDateTime.Empty, provider.ExchangeRateDateTime);
		}

		public void TestInvoiceHeaderAndInvoiceLineInformation()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateCusMapType(RefCusMapTypeList.Codes.ChargeCode, MapDirectionList.Codes.OUT, "CW1 Charge Codes to Customs codes", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.AdditionCharge, "160", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.LandingCharges, "78", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.ChargeCode, InvoiceLineCustomsChargeTypeList.Codes.OtherCharges, "160", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.AdvancePaymentNo);
			Factory.Save();

			var gbpCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedKingdom);
			gbpCurrency.ExchangeRates.DeleteAll();
			gbpCurrency.SetCustomsRate(new ZDateTime(2024, 12, 10), new ZDateTime(2024, 12, 10), 0.04m);
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usdCurrency.ExchangeRates.DeleteAll();
			usdCurrency.SetCustomsRate(new ZDateTime(2024, 12, 10), new ZDateTime(2024, 12, 10), 0.05m);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SUPPLIER FULL NAME";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			supplier.MainAddress.OA_Address1 = "SUP ADDRESS 1";
			supplier.MainAddress.OA_Address2 = "SUP ADDRESS 2";
			supplier.MainAddress.OA_City = "SUP CITY";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2024, 12, 10);
			declaration.JE_OH_Supplier = supplier.PK;

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-1234";
			invoice.JZ_InvoiceAmount = 123.45m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_PaymentAmount = 12.34m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedKingdom;

			var invoiceCharge1 = invoice.Charges.AddNew();
			invoiceCharge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.AdditionCharge;
			invoiceCharge1.J7_Amount = 10m;
			invoiceCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoiceCharge2 = invoice.Charges.AddNew();
			invoiceCharge2.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.OtherCharges;
			invoiceCharge2.J7_ChargeDescription = "SPECIAL CHARGE";
			invoiceCharge2.J7_Amount = 20m;
			invoiceCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceCharge3 = invoice.Charges.AddNew();
			invoiceCharge3.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.Discount;
			invoiceCharge3.J7_Amount = 30m;
			invoiceCharge3.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;

			for (var i = 1; i <= 10; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = $"{entryInstruction.CEI_Style}00";
				invoiceLine.JI_Description = $"ITEM {i}";
				invoiceLine.JI_PartNo = $"PART {i}";
				invoiceLine.JI_InvoiceQuantity = i;
				invoiceLine.JI_InvoiceUQ = "KG";
				invoiceLine.JI_LinePrice = i * 10m;
				invoiceLine.JI_ValuationMarkup = i * 1.1m;
				invoiceLine.JI_BrandName = $"BRAND {i}";
				invoiceLine.JI_AdvancePaymentNo = $"APN-{i}";
			}

			var invoiceLine1 = invoice.InvoiceLines[0];
			var invoiceLine1Charge1 = invoiceLine1.Charges.AddNew();
			invoiceLine1Charge1.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.LandingCharges;
			invoiceLine1Charge1.J7_Amount = 1.23m;
			invoiceLine1Charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;

			var invoiceLine1Charge2 = invoiceLine1.Charges.AddNew();
			invoiceLine1Charge2.J7_ChargeType = InvoiceLineCustomsChargeTypeList.Codes.Discount;
			invoiceLine1Charge2.J7_Amount = 2.34m;
			invoiceLine1Charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			declaration.DoMerge();

			var invoiceHeaderInformation = (IInvoiceHeaderInformation)invoice;
			CombineAssertions("IInvoiceHeaderInformation", () =>
			{
				AssertEquals("SUPPLIER FULL NAME", invoiceHeaderInformation.NameOfIssuer);
				AssertEquals("AU", invoiceHeaderInformation.CountryOfIssuer);
				AssertEquals("SUP ADDRESS 1", invoiceHeaderInformation.Address1);
				AssertEquals("SUP ADDRESS 2", invoiceHeaderInformation.Address2);
				AssertEquals("SUP CITY", invoiceHeaderInformation.Address3);
				AssertEquals("New South Wales", invoiceHeaderInformation.Address4);
				AssertEquals(123.45m, invoiceHeaderInformation.TotalInvoiceAmount);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedKingdom, invoiceHeaderInformation.InvoiceCurrencyCoded);
				AssertEquals(0.04m, invoiceHeaderInformation.ExchangeRate);
				AssertEquals(1161.23m, invoiceHeaderInformation.TotalChargesInLocalCurrency);
				AssertEquals(invoice.JZ_Calc_ConversionFactor, invoiceHeaderInformation.CommonFactor);
				AssertEquals(Core.Constants.IncoTerms.FreeOnBoard, invoiceHeaderInformation.TermsOfDelivery);
				AssertEquals(12.34m, invoiceHeaderInformation.AdvancePaymentAmount);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedKingdom, invoiceHeaderInformation.AdvancePaymentCurrencyCode);
				AssertContainsExactElementsInExactOrder(["APN-1", "APN-2", "APN-3", "APN-4", "APN-5"], invoiceHeaderInformation.AdvancePaymentNotificationDetails);
			});

			CombineAssertions("IInvoiceChargeInformation", () =>
			{
				var charges = invoiceHeaderInformation.InvoiceChargeInformations.ToArray();
				AssertEquals(4, charges.Length);

				var charge1 = charges[0];
				AssertEquals("Other Additional Charges", charge1.ChargeDescription);
				AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, charge1.ChargeCurrency);
				AssertEquals(10m, charge1.ChargeAmount);
				AssertEquals("160", charge1.MonetaryAmountChargeType);
				AssertEquals(0m, charge1.MonetaryDiscountAmount);
				AssertEquals(false, charge1.IsOtherCharge);
				AssertEquals(1m, charge1.GetChargeCurrencyConversionRate(new ZDateTime(2024, 12, 10)));

				var charge2 = charges[1];
				AssertEquals("SPECIAL CHARGE", charge2.ChargeDescription);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, charge2.ChargeCurrency);
				AssertEquals(20m, charge2.ChargeAmount);
				AssertEquals("160", charge2.MonetaryAmountChargeType);
				AssertEquals(0m, charge2.MonetaryDiscountAmount);
				AssertEquals(true, charge2.IsOtherCharge);
				AssertEquals(0.05m, charge2.GetChargeCurrencyConversionRate(new ZDateTime(2024, 12, 10)));

				var charge3 = charges[2];
				AssertEquals("Discount", charge3.ChargeDescription);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedKingdom, charge3.ChargeCurrency);
				AssertEquals(30m, charge3.ChargeAmount);
				AssertEquals(ZString.Empty, charge3.MonetaryAmountChargeType);
				AssertEquals(30m, charge3.MonetaryDiscountAmount);
				AssertEquals(false, charge3.IsOtherCharge);
				AssertEquals(0.04m, charge3.GetChargeCurrencyConversionRate(new ZDateTime(2024, 12, 10)));

				var charge4 = charges[3];
				AssertEquals(ZString.Empty, charge4.ChargeDescription);
				AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, charge4.ChargeCurrency);
				AssertEquals(1.23m, charge4.ChargeAmount);
				AssertEquals("78", charge4.MonetaryAmountChargeType);
				AssertEquals(0m, charge4.MonetaryDiscountAmount);
				AssertEquals(false, charge4.IsOtherCharge);
				AssertEquals(1m, charge4.GetChargeCurrencyConversionRate(new ZDateTime(2024, 12, 10)));
			});

			var lines = invoiceHeaderInformation.InvoiceLineInformations.ToArray();
			CombineAssertions("IInvoiceLineInformation", () =>
			{
				AssertEquals(invoice.InvoiceLines.Count, lines.Length);

				var line1 = lines[0];
				AssertEquals((short)1, line1.InvoiceLineNumber);
				AssertEquals((short)1, line1.RelatedDeclarationLineNumber);
				AssertEquals("PART 1", line1.ProductCode);
				AssertEquals(1m, line1.Quantity);
				AssertEquals("KG", line1.QuantityUnit);
				AssertEquals(10m, line1.PriceDetails);
				AssertEquals(10m, line1.ItemAmount);
				AssertEquals(1.1m, line1.RateDetails);
				AssertEquals("BRAND 1", line1.BrandName);
				AssertEquals("ITEM 1", line1.CommercialInvoiceItemDescription);
			});

			CombineAssertions("IInvoiceLineChargeInformation", () =>
			{
				var lineCharges = lines[0].InvoiceLineChargeInformations.ToArray();
				AssertEquals(4, lineCharges.Length);

				var lineCharge1 = lineCharges[0];
				AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, lineCharge1.ChargeCurrency);
				AssertEquals(0m, lineCharge1.MonetaryDiscountAmount);

				var lineCharge2 = lineCharges[1];
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, lineCharge2.ChargeCurrency);
				AssertEquals(2.34m, lineCharge2.MonetaryDiscountAmount);
			});
		}

		protected override void SetUp()
		{
			helper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
			temporaryFUNCS = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now, true);
			temporaryPFUNC = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now, true);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		}

		protected override void TearDown()
		{
			base.TearDown();
			temporaryFUNCS.Dispose();
			temporaryPFUNC.Dispose();
		}

		void DeclarationForNADDT(out OrgHeader testSupplier, out CusEntryInstruction testInstruction1, out JobComInvoiceLine invoiceLine1, out CusEntryHeader entry)
		{
			testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = testSupplier.PK;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			address.OA_OH = testSupplier.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKPortOfLoading = "ZAJNB";
			declaration.JE_RL_NKPortOfArrival = "NAWDH";
			declaration.JE_RL_NKOrigin = "ZADUR";
			declaration.JE_RL_NKFinalDestination = "NAWDH";
			testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "10";
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			entry = declaration.CustomsEntryHeaders.AddNew();
			var line = entry.MergedLines.AddNew();
			invoiceLine1.JI_CL = line.PK;
			new LineMerger(declaration).DoMerge();
		}

		MessageDataProviderKeyFactor CreateMessageDataProviderKeyFactor(CusEntryInstruction testInstruction1, ZString shipmentType)
		{
			return new MessageDataProviderKeyFactor()
			{
				ShipmentType = shipmentType,
				TransportMode = "AIR",
				ProcedureCategory = "H",
				CPC = testInstruction1.CEI_Style,
				PPC = "00",
				FirstNonSpecificTariffTypeConcession = ZString.Empty,
				MessageType = "CHG",
				RemovalTransportMode = declaration.JE_RemovalTransportCode,
				CountryOfDestination = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Namibia),
				CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica),
				RelationshipIndicator = ZString.Empty,
				DeclarationType = "EXP",
				VDN = ZString.Empty
			};
		}

		static void AssertFANNumberWithMessageTypeAndPaymentmethod(CusEntryHeader header, string type, string paymentMethod, string caption, string expect)
		{
			header.CH_PaymentMethod = paymentMethod;
			var sendingObject = new MessageSendingObject(header);
			sendingObject.MessageType = type;
			AssertEquals(caption, expect, (sendingObject as ICUSDECMessageDataProvider).FinancialAccountNumber);
		}

		IDisposable temporaryFUNCS;
		IDisposable temporaryPFUNC;
		JobDeclaration declaration;
		ZAUniversalReferenceTestDataHelper helper;

		sealed class MrnToBeReplaced_TestCase
		{
			public MrnToBeReplaced_TestCase(string testId, string messageSubType, bool mustOutputOriginalMRN, bool mustOutputMRNToBeReplaced)
			{
				TestId = testId;
				MessageSubType = messageSubType;
				MustOutputOriginalMRN = mustOutputOriginalMRN;
				MustOutputMRNToBeReplaced = mustOutputMRNToBeReplaced;
			}

			#region Instance Variables:
			public readonly string TestId;
			public readonly string MessageSubType;
			public readonly bool MustOutputOriginalMRN;
			public readonly bool MustOutputMRNToBeReplaced;
			#endregion Instance Variables.
		}

		sealed class MrnToBeReplaced_Tester
		{
			public static void TestScenario(MrnToBeReplaced_TestCase scenario, BusinessObjectFactory factory)
			{
				var dec = factory.NewWithValidTestData<JobDeclaration>();
				dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				var instr = dec.CustomsEntryInstructions.AddNew();
				instr.CEI_Style = "11";
				instr.CEI_MRNToBeReplaced = "DBN202004061234567";
				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = instr.PK;
				if (scenario.MessageSubType != MessageSubTypeCodes.Codes.Original)
				{
					entry.MovementReferenceNumberSetter("DBN202004061234570");
				}

				var line = entry.MergedLines.AddNew();
				var invoice = dec.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = line.PK;
				var sendingObject = new MessageSendingObject(entry);
				sendingObject.MessageType = scenario.MessageSubType;
				var provider = (sendingObject as ICUSDECMessageDataProvider);
				var assertMsg = $"{scenario.TestId} ({scenario.MessageSubType})";
				CheckProvidedOutputValue(assertMsg, scenario.MustOutputOriginalMRN, entry.MovementReferenceNumber, provider.OriginalMRN);
				CheckProvidedOutputValue(assertMsg, scenario.MustOutputMRNToBeReplaced, instr.CEI_MRNToBeReplaced, provider.MRNToBeReplaced);
			}

			static void CheckProvidedOutputValue(string assertMsg, bool mustProvideData, ZString dataValue, ZString providedDataValue)
			{
				var expectedValue = ZString.Empty;
				if (mustProvideData)
				{
					expectedValue = dataValue;
				}

				AssertEquals(assertMsg, expectedValue, providedDataValue);
			}
		}

		sealed class MrnToBeReplaced_TestCase_Collection
		{
			public void Add(string testId, string messageSubType, bool mustOutputOriginalMRN, bool mustOutputMRNToBeReplaced)
			{
				list.Add(new MrnToBeReplaced_TestCase(testId, messageSubType, mustOutputOriginalMRN, mustOutputMRNToBeReplaced));
			}

			public void TestAll(BusinessObjectFactory factory)
			{
				list.ForEach(x => MrnToBeReplaced_Tester.TestScenario(x, factory));
			}

			readonly List<MrnToBeReplaced_TestCase> list = new List<MrnToBeReplaced_TestCase>();
		}
	}
}
