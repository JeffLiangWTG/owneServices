using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntrySummaryPrintingFromMessage))]
	sealed class EntrySummaryPrintingFromMessageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAdditionalLineSectionPrintingFlags()
		{
			CusEntryLine entryLine1 = Entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = Entry.MergedLines.AddNew();
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               27322                10A390191-01319900091-013199000                 8         XJ5 7000557001891  IL 20     CAPE SCOTT          103901111108B00151012            583  111108I310     22            003988      876766                  00000600CT         APLUAPLU   30                                  0               2112108             APLU    40001AU00000018520000000062                    000000110060267                  50 9802004040                                                       AU111108Y   51                                                                              60                                        US8495956                             709102111010 0000012871            NO                               0000003406  809802004040                                                        0000001010  819102111020 0000006080            NO                               0000001609  819802004040                                                                    819102111030 0000005083            NO                               0000001345  819802004040                                                        0000000204  819102111040                       NO                                           9000000024034           0                                  00000009426          Y  8888XJ5EI00016000000024034";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("EntryHasADDCVDLines", false, printBO.EntryHasADDCVDLines);
			AssertEquals("EntryHasSecondaryTariffLines", true, printBO.EntryHasSecondaryTariffLines);
			AssertEquals("EntryHasAdditionalTariffLines", true, printBO.EntryHasAdditionalTariffLines);
			AssertEquals("EntryHasProRatedCalculation", false, printBO.EntryHasProRatedCalculation);
			AssertEquals("EntryHasAdValoremConversionCalculation ", false, printBO.EntryHasAdValoremConversionCalculation);
		}

		class ACEEntryMessage7501PrintTestDummy : ACEEntryMessage7501Print
		{
			public ACEEntryMessage7501PrintTestDummy(CusEntryHeader entryHeader, MQEDIMessage outgoingMessage, MQEDIMessage incomingMessage, EDIMessage bluMessage)
				: base(entryHeader, outgoingMessage, incomingMessage, bluMessage)
			{ }

			public ZString EffectiveUltimateConsigneeCustomsRegNoCoreString => this.EffectiveUltimateConsigneeCustomsRegNoCore;
		}

		public void TestIORUltimateConsingneeWhenOrgHasSSNNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var otherOrg = Factory.New<OrgHeader>();
			otherOrg.FillWithValidTestData();
			otherOrg.OH_FullName = "IOR#";
			otherOrg.OH_FullName = "Other Org#";
			otherOrg.MainAddress.CompanyName = "OTHER#1";
			otherOrg.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.SocialSecurityNumber, "666-99-4444", Core.Constants.CountryCodes.UnitedStates);
			declaration.IOROrgPK = otherOrg.PK;

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_Code = "TES#ULT";
			ultimateConsignee.OH_FullName = "Ultimate Consignee#";
			ultimateConsignee.MainAddress.CompanyName = "COMPA#1";
			ultimateConsignee.MainAddress.State = "NY";
			var ssnCode = ultimateConsignee.CustomsCodes.AddNew();
			ssnCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			ssnCode.OK_CustomsRegNo = "555-99-4444";

			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertEquals("ultimateConsignee is Importer of Record", otherOrg.PK, entry.ImporterOfRecord.PK);

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = Entry.PK;
			outMsg.EM_MessageText = "B  0708267AE                                  4601267  1   " + MQEDIMessage.MessageNumberPlaceHolder + "10A267  02008504 0708100014595   0130 XYY         60822110902                   11666-99-4444 555-99-4444                      081011       PA                  20ATIC0708081011A888                                                            2200000840CS                                                                    23MOTOE26088202                                                                 318B 856                                                                        40  001 XQCA081011      CA0000001200     0000019051    Y                        42XQFREMED3600VAUSOI132205         0001 0001                                    44LIQUID BICARBONATE 4000                                                       47MXQHAEINC383VAU                                                               47C555-99-4444                                                                  47S555-99-4444                                                                  503004909170 0000000000 0000010198 000001809800KG                               OI        LIQUID BICARBONATE 4000                                               FD0100178K--POA  CADEV1225714                  XQHAEINC383VAU XQFREMED3600VAU   FD020000084000CS  0000000300BO  0000000640L                                     FD030000010198            LIQUID BICARBONATE 4000                               FD04              TONY PATTI9086030660                                          FD05LSTE621843                                                                  FD05PMNK071387                                                                  CW02     27C50                                                                  9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  0708267AE";

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incoming7501.EM_MessageText = "B000708267AX                                               <<MSGNO PLACEHOLDER>>E0 SUMMRY 000001 REF ID: SV9 73058107 100014595    000                          E1A 995   SUMMARY HAS BEEN ADDED                  267  7305810700100100014595   Y  0708267AX00002";
			var printBO = new ACEEntryMessage7501PrintTestDummy(Entry, outMsg, incoming7501, null);
			AssertEquals("Ultimate Consignee with SSN", "555-99-4444", printBO.EffectiveUltimateConsigneeCustomsRegNoCoreString);
			AssertEquals("No SSN on the document", "", printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNoCore is not SAME as Importer", "NY", printBO.EffectiveUltimateConsigneeState);

			AssertEquals("No SSN", "", printBO.ImporterOfRecordCustomsRegNo);

			AssertEquals("Ultimate Consignee#", "COMPA#1", printBO.EffectiveUltimateConsigneeCompanyName);
			AssertEquals("Importer Of Record#", "OTHER#1", printBO.ImporterCompanyName);
			AssertEquals("ImporterAddressLine1", "#1", printBO.ImporterAddressLine1);

			outMsg.EM_MessageText = "B  0708267AE                                  4601267  1   " + MQEDIMessage.MessageNumberPlaceHolder + "10A267  02008504 0708100014595   0130 XYY         60822110902                   11555-99-4444 555-99-4444                      081011       PA                  20ATIC0708081011A888                                                            2200000840CS                                                                    23MOTOE26088202                                                                 318B 856                                                                        40  001 XQCA081011      CA0000001200     0000019051    Y                        42XQFREMED3600VAUSOI132205         0001 0001                                    44LIQUID BICARBONATE 4000                                                       47MXQHAEINC383VAU                                                               47C555-99-4444                                                                  47S555-99-4444                                                                  503004909170 0000000000 0000010198 000001809800KG                               OI        LIQUID BICARBONATE 4000                                               FD0100178K--POA  CADEV1225714                  XQHAEINC383VAU XQFREMED3600VAU   FD020000084000CS  0000000300BO  0000000640L                                     FD030000010198            LIQUID BICARBONATE 4000                               FD04              TONY PATTI9086030660                                          FD05LSTE621843                                                                  FD05PMNK071387                                                                  CW02     27C50                                                                  9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  0708267AE";
			printBO = new ACEEntryMessage7501PrintTestDummy(Entry, outMsg, incoming7501, null);
			AssertEquals("Ultimate Consignee with the same SNN number of Importer", "SAME", printBO.EffectiveUltimateConsigneeCustomsRegNoCoreString);
			AssertEquals("EffectiveUltimateConsigneeState when EffectiveUltimateConsigneeCustomsRegNoCore is SAME", "PA", printBO.EffectiveUltimateConsigneeState);
		}

		public void TestPaymentTypeIncoporatingSTU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var outgoing = Factory.New<MQEDIMessage>();
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageText = "B018888XJ5EI                                               " + MQEDIMessage.MessageNumberPlaceHolder + "10A390191-01319900091-013199000                 8         XJ5 7000557001891  IL 20     CAPE SCOTT          103901111108B00151012            583  111108I310     22            003988      876766                  00000600CT         APLUAPLU   30                                  0               2112108             APLU    40001AU00000018520000000062                    000000110060267                  50 9802004040                                                       AU111108Y   51                                                                              60                                        US8495956                             709102111010 0000012871            NO                               0000003406  809802004040                                                        0000001010  819102111020 0000006080            NO                               0000001609  819802004040                                                                    819102111030 0000005083            NO                               0000001345  819802004040                                                        0000000204  819102111040                       NO                                           9000000024034           0                                  00000009426          Y  8888XJ5EI00016000000024034";
			outgoing.EM_SystemCreateTimeUtc = new ZDateTime(2011, 10, 10, 13, 1, 0);
			entry.Messages.Add(outgoing);

			var incoming = Factory.New<MQEDIMessage>();
			incoming.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incoming.EM_SystemCreateTimeUtc = new ZDateTime(2011, 10, 10, 13, 5, 0);
			entry.Messages.Add(incoming);

			var printBO = new EntryMessageENS7501Print(entry, outgoing, incoming, null);
			AssertEquals("ABI/A", printBO.EntryTypeCode);

			var outgoingSTU = DeclarationTestHelper.CreateTransmittedSTUMsg(Factory, "30448", new ZDateTime(2011, 10, 10, 13, 05, 0), "B018888XJ5HP                                               " + MQEDIMessage.MessageNumberPlaceHolder + "H8888XJ5 700078811                                                              Y  8888XJ5HP00001");
			declaration.Messages.Add(outgoingSTU);

			var incomingSTU = DeclarationTestHelper.CreateIncomingSTUMsg(Factory, "30448", new ZDateTime(2011, 10, 10, 13, 06, 0), "B018888XJ5HT                                               30448                H18888XJ5 020074492GBDATA REPLACED AS REQUESTED               7081711B00151238  Y  8888XJ5HT00001");
			declaration.Messages.Add(incomingSTU);

			outgoingSTU = DeclarationTestHelper.CreateTransmittedSTUMsg(Factory, "30449", new ZDateTime(2011, 10, 10, 13, 10, 0), "B018888XJ5HP                                               " + MQEDIMessage.MessageNumberPlaceHolder + "H8888XJ5 700078811                                                              Y  8888XJ5HP00001");
			declaration.Messages.Add(outgoingSTU);

			incomingSTU = DeclarationTestHelper.CreateIncomingSTUMsg(Factory, "30449", new ZDateTime(2011, 10, 10, 13, 15, 0), "B018888XJ5HT                                               30449                H18888XJ5 7000788157FSUMM REMVD FR STMT, DOCS NOW REQD        1      B00151238  Y  8888XJ5HT00001");
			declaration.Messages.Add(incomingSTU);

			printBO = new EntryMessageENS7501Print(entry, outgoing, incoming, null);
			AssertEquals("STU has been accepted & removed from statement", "ABI/N", printBO.EntryTypeCode);
		}

		public void TestSummaryStatusForEntrySummaryAcceptedWithWarnings()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings;

			var outgoing = Factory.New<MQEDIMessage>();
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageText = "B018888XJ5EI                                               " + MQEDIMessage.MessageNumberPlaceHolder + "10A390191-01319900091-013199000                 8         XJ5 7000557001891  IL 20     CAPE SCOTT          103901111108B00151012            583  111108I310     22            003988      876766                  00000600CT         APLUAPLU   30                                  0               2112108             APLU    40001AU00000018520000000062                    000000110060267                  50 9802004040                                                       AU111108Y   51                                                                              60                                        US8495956                             709102111010 0000012871            NO                               0000003406  809802004040                                                        0000001010  819102111020 0000006080            NO                               0000001609  819802004040                                                                    819102111030 0000005083            NO                               0000001345  819802004040                                                        0000000204  819102111040                       NO                                           9000000024034           0                                  00000009426          Y  8888XJ5EI00016000000024034";
			outgoing.EM_SystemCreateTimeUtc = new ZDateTime(2011, 10, 10, 13, 1, 0);
			entry.Messages.Add(outgoing);

			var incoming = Factory.New<MQEDIMessage>();
			incoming.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incoming.EM_SystemCreateTimeUtc = new ZDateTime(2011, 10, 10, 13, 5, 0);
			entry.Messages.Add(incoming);

			var printBO = new EntryMessageENS7501Print(entry, outgoing, incoming, null);

			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			AssertEquals("Summary Status should be set as Paperless.", USConstants.EntrySummaryDisposition.Paperless, printBO.SummaryStatus);

			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertEquals("Summary Status should be set as Paperless.", USConstants.EntrySummaryDisposition.Paperless, printBO.SummaryStatus);

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals("Summary Status should be set as Non-paperless.", USConstants.EntrySummaryDisposition.DocsRequired, printBO.SummaryStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = Entry.PK;
			outMsg.EM_MessageText = "B  0708267AE                                  4601267  1   " + MQEDIMessage.MessageNumberPlaceHolder + "10A267  02008504 0708100014595   0130 XYY         60822110902                   1104-34759790004-347597900                     081011       PA                  20ATIC0708081011A888                                                            2200000840CS                                                                    23MOTOE26088202                                                                 318B 856                                                                        40  001 XQCA081011      CA0000001200     0000019051    Y                        42XQFREMED3600VAUSOI132205         0001 0001                                    44LIQUID BICARBONATE 4000                                                       47MXQHAEINC383VAU                                                               47C04-347597900                                                                 47S04-347597900                                                                 503004909170 0000000000 0000010198 000001809800KG                               OI        LIQUID BICARBONATE 4000                                               FD0100178K--POA  CADEV1225714                  XQHAEINC383VAU XQFREMED3600VAU   FD020000084000CS  0000000300BO  0000000640L                                     FD030000010198            LIQUID BICARBONATE 4000                               FD04              TONY PATTI9086030660                                          FD05LSTE621843                                                                  FD05PMNK071387                                                                  CW02     27C50                                                                  9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  0708267AE";

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var printBO = new ACEEntryMessage7501Print(Entry, outMsg, incoming7501, null);
			return printBO;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EnableENS = true;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

					var filer = new EntryFiler();
					filer.EntryFilerCode = "XJ5";
					USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

					declaration.US_EntryFilerCode = "XJ5";
				}
				return declaration;
			}
		}

		CusEntryHeader entry;
		CusEntryHeader Entry => entry ?? (entry = Declaration.CustomsEntryHeaders.AddNew());
	}
}
