using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
using Enterprise.Customs.NZ.Business.Declaration.FormalEntry;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.Business.MessageProcessors;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : BaseJobDeclarationTest<JobDeclaration>
	{
		public void TestDefaultJE_ApplicationCode()
		{
			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID" };

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("JE_ApplicationCode: Builtin", JobApplicationCodeList.Codes.TSW, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("JE_ApplicationCode: Interfaced", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("JE_ApplicationCode: BothBuiltInDefaulted", JobApplicationCodeList.Codes.TSW, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("JE_ApplicationCode: BothInterfaceDefaulted", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
		}

		public void TestIsDeclarationIntegrated()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = ZString.Empty;
				AssertEquals("Empty", false, declaration.IsDeclarationIntegrated);

				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("TSW", false, declaration.IsDeclarationIntegrated);

				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals("ITF", true, declaration.IsDeclarationIntegrated);
			});
		}

		public void TestShowSubmitMenuItem()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = ZString.Empty;
				AssertEquals("Empty", false, declaration.ShowSubmitMenuItem);

				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("TSW", false, declaration.ShowSubmitMenuItem);

				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals("ITF", true, declaration.ShowSubmitMenuItem);
			});
		}

		public void TestAutoBilling()
		{
			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var option = new AccountingIntegrationOptions
			{
				EnableAccountingIntegration = true,
				APPostDSB = true,
				ARPostDSB = true
			};
			using (CustomsDataRegistry.Instance.EnableAccountingIntegration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option))
			{
				var declaration = Factory.New<JobDeclaration>();
				var importer = testHelper.Importer;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.CashPaidByBroker;
				declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.EntryNumber = "44051192";
				entry.CH_MessageType = EntryMessageTypeList.Codes.FormalEntry;
				entry.Charges.AddNew(EntryChargeTypeList.Codes.EntryFeeGST, 7.27m);
				entry.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOnPayment;
				declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOnPayment;
				Factory.Save();

				var jobLoader = new Job.Loader(declaration);
				var job = jobLoader.Load();
				AssertNull("No job created as it's a drawback declaration", job);

				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				entry.CH_TotalPaid = 10m; // Update a property of the entry header so that the factory would trigger entry.OnSaved
				Factory.Save();
				job = jobLoader.Load();
				AssertNotNull("Should create a job as it's not a drawback declaration", job);
				AssertEquals("A charge should create for the job", 1, job.Charges.Count);
			}
		}

		public void TestPackingInformation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.IsPackingInformationRelevant);
		}

		public void TestPackingGroups()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(DeclarationLevelPackingGroupCollection), declaration.PackingGroups.GetType());
		}

		public void TestEntryFeeIsRemovedFromConsolidatedDeclarationsExceptLeadDeclaration()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.Charges.AddNew(EntryChargeTypeList.Codes.EntryFee);
			entryHeader1.Charges.AddNew(EntryChargeTypeList.Codes.EntryFeeGST);
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.Charges.AddNew(EntryChargeTypeList.Codes.EntryFee);
			entryHeader2.Charges.AddNew(EntryChargeTypeList.Codes.EntryFeeGST);
			declaration1.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			declaration2.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			var consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
			consolidatedDeclaration.JobDeclarations.Add(declaration1);
			consolidatedDeclaration.JobDeclarations.Add(declaration2);

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition", consolidatedDeclaration.LeadDeclaration.PK, declaration1.PK);
				AssertNotNull("ENF type entry fee is not removed for lead declaration", declaration1.CusEntryHeader.Charges.GetChargeWithThisCode(EntryChargeTypeList.Codes.EntryFee));
				AssertNotNull("EFG type entry fee is not removed for lead declaration", declaration1.CusEntryHeader.Charges.GetChargeWithThisCode(EntryChargeTypeList.Codes.EntryFeeGST));
				AssertNull("ENF type entry fee is removed for non-lead declaration", declaration2.CusEntryHeader.Charges.GetChargeWithThisCode(EntryChargeTypeList.Codes.EntryFee));
				AssertNull("EFG type entry fee is removed for non-lead declaration", declaration2.CusEntryHeader.Charges.GetChargeWithThisCode(EntryChargeTypeList.Codes.EntryFeeGST));
			});
		}

		public void TestEntryStatusIsSetByMessageStatusForPreConsolidationEntry()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_MessageStatus = ZString.Empty;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Assert(Declaration.HasNotBeenSentToCustoms);
			AssertEquals("Not Sent to Customs", "NSC", Declaration.JE_EntryStatus);

			Declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			AssertEquals("Entry Status should show the Consolidation preparation status, (Ready for Consolidation), for Consolidation jobs prior to sending", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, Declaration.JE_EntryStatus);

			Declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			AssertEquals("Entry Status should show the Consolidation preparation status, (Applied to Consolidation), for Consolidation jobs prior to sending", ConsolidatedEntryStatusList.Codes.AppliedToConsolidation, Declaration.JE_EntryStatus);

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			AssertEquals("Sent to Customs", "STC", Declaration.JE_EntryStatus);
		}

		public void TestEntryFeeUnPayable()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			Declaration.Invoices.DeleteAll();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertEquals("Precondition", 400m, Declaration.VFDDeminimus);
			Assert("Declaration.EntryFeeUnPayable", Declaration.EntryFeeUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Assert("Declaration.EntryFeeUnPayable", Declaration.EntryFeeUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Excise;
			Assert("Declaration.EntryFeeUnPayable", !Declaration.EntryFeeUnPayable);

			Declaration.JE_DateOfArrival = new ZDateTime(2019, 08, 01);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Declaration.EntryFeeUnPayable", !Declaration.EntryFeeUnPayable);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = Declaration.Invoices.AddNew();
			header.JZ_InvoiceAmount = 200m;
			header.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			Assert("Declaration.EntryFeeUnPayable", Declaration.EntryFeeUnPayable);

			header.JZ_InvoiceAmount = 500m;
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6110300201K";
			invoiceLine.JI_LinePrice = 500m;
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "NUM";
			invoiceLine.JI_CountryOfOrigin = "CN";
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Assert("Declaration.EntryFeeUnPayable", !Declaration.EntryFeeUnPayable);
		}

		public void TestIsECIWriteoffAndGSTIsApplicable()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Assert(Declaration.IsECIWriteoffAndGSTIsApplicable);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(!Declaration.IsECIWriteoffAndGSTIsApplicable);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Assert(!Declaration.IsECIWriteoffAndGSTIsApplicable);
		}

		public void TestHasTheSameGSTDetailsOnAllInvoices()
		{
			AssertEquals(0, Declaration.Invoices.Count);
			Assert(Declaration.HasTheSameGSTDetailsOnAllInvoices);

			var invoiceHeader1 = Declaration.Invoices.AddNew();
			AssertEquals(1, Declaration.Invoices.Count);
			Assert(Declaration.HasTheSameGSTDetailsOnAllInvoices);

			invoiceHeader1.JZ_SupplierGSTNumber = "AA111";
			invoiceHeader1.JZ_IsGSTPrePaid = "Y";
			Assert(Declaration.HasTheSameGSTDetailsOnAllInvoices);

			var invoiceHeader2 = Declaration.Invoices.AddNew();
			AssertEquals(2, Declaration.Invoices.Count);
			Assert(!Declaration.HasTheSameGSTDetailsOnAllInvoices);

			invoiceHeader2.JZ_SupplierGSTNumber = "BB222";
			Assert(!Declaration.HasTheSameGSTDetailsOnAllInvoices);

			invoiceHeader2.JZ_IsGSTPrePaid = "N";
			Assert(!Declaration.HasTheSameGSTDetailsOnAllInvoices);

			invoiceHeader2.JZ_SupplierGSTNumber = "AA111";
			Assert(!Declaration.HasTheSameGSTDetailsOnAllInvoices);

			invoiceHeader2.JZ_IsGSTPrePaid = "Y";
			Assert(Declaration.HasTheSameGSTDetailsOnAllInvoices);

			invoiceHeader2.JZ_SupplierGSTNumber = "BB222";
			Assert(!Declaration.HasTheSameGSTDetailsOnAllInvoices);
		}

		public void TestITranshipmentRequestParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = new ZDateTime(2019, 7, 10);
			declaration.JE_DateOfArrival = new ZDateTime(2019, 7, 11);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var parent = (ITranshipmentRequestParent)declaration;
			AssertEquals(declaration, parent.TransportParent);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "AU";
			shipment.JS_RL_NKOrigin = "US";
			declaration.JE_JS = shipment.PK;
			AssertEquals(shipment, parent.TransportParent);

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKDischargePort = "AU";
			arrivalConsol.JK_RL_NKLoadPort = "CN";
			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_RL_NKDischargePort = "CN";
			departureConsol.JK_RL_NKLoadPort = "US";

			var index = 0;
			parent.MessageTypeChanged += (s, e) =>
			{
				index++;
			};

			var msg = "";
			parent.MessageSubTypeChanged += (s, e) =>
			{
				msg = "me";
			};

			var transport = "";
			parent.TransportModeChanged += (s, e) =>
			{
				transport = "sea";
			};

			Assert(parent.IsExport);
			Assert(!parent.IsImport);
			AssertEquals(new ZDateTime(2019, 7, 10), parent.DepartureDate);
			AssertEquals(new ZDateTime(2019, 7, 11), parent.ArrivalDate);
			AssertEquals(departureConsol, parent.TransportParent);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!parent.IsExport);
			Assert(parent.IsImport);
			AssertEquals(1, index);
			AssertEquals(arrivalConsol, shipment.ArrivalConsol);
			AssertEquals(arrivalConsol, parent.TransportParent);

			declaration.JE_MessageSubType = "XX";
			AssertEquals("me", msg);

			declaration.JE_TransportMode = "Air";
			AssertEquals("sea", transport);
		}

		public void TestIsMiscellaneousImporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(!declaration.IsMiscellaneousImporter);
			declaration.JE_OH_Importer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			Assert(declaration.IsMiscellaneousImporter);
		}

		public void TestTranshipmentRequest()
		{
			var dec = Factory.New<JobDeclaration>();
			var query = new ZQuery(CusUnderbondSchema.C4_ParentID, dec.PK);
			AssertNull("No TranshipmentRequest created", Factory.LoadTop1<TranshipmentRequest>(query));
			TranshipmentRequest.Create(dec);
			AssertNotNull("TranshipmentRequest now loading", dec.TranshipmentRequest);
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.AreMultipleEntryInstructionsAllowed);
		}

		public void TestProcessRestoredEntry()
		{
			var decCreator = new TestFormalEntryCreator(Declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Declaration.JE_DeclarationReference = "B00001268";
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be 1 Entry Header after merge", 1, Declaration.CustomsEntryHeaders.Count);
			AssertEquals("Should be 1 Merged Line after merge", 1, Declaration.CusEntryHeader.MergedLines.Count);
			var entryHeader = Declaration.CusEntryHeader;

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.SaveHandlingSaveExceptions();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00001268";
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_MessageText =
			#region OutgoingMessage Text
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>B00001268</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">150</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20131121</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <Content>IM1 Sea Job - goods location / approved establishment place</Content>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>00009908C</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>CAI YUN HE</Name>
    <ID>9228758</ID>
    <TypeCode>1</TypeCode>
    <JourneyID>165E</JourneyID>
  </BorderTransportMeans>
  <CurrencyExchange>
    <RateNumeric>1.00</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>gary.odea@cargowise.com</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>+61 2 80012200</ID>
      <TypeID>TE</TypeID>
    </Communication>
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>C</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">403.50</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">403.50</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>AU</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <AdditionalInformation>
        <StatementDescription>MSKU5549283,YNYYN</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <GoodsLocation>
        <ID>7175H</ID>
      </GoodsLocation>
      <LoadingLocation>
        <ID>AUSYD</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>OB82372</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>S0224984</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>45</CharacteristicCode>
        <FullnessCode>5</FullnessCode>
        <ID>MSKU5549283</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Seal>
          <SequenceNumeric>1</SequenceNumeric>
          <ID>H04392J</ID>
        </Seal>
      </TransportEquipment>
      <UnloadingLocation>
        <ID>NZWLG</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">155</FreightChargeAmount>
      <FreightChargeApportionmentCode>161</FreightChargeApportionmentCode>
    </CustomsValuation>
    <DeliveryDestination>
      <Name>ADULATION BOOKS LTD (NZ CUSTOMS)</Name>
      <Address>
        <CityName>AUCKLAND</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>AUK</CountrySubDivisionName>
        <Line>1 MAIN STREET</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">2500</CustomsValueAmount>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <ApprovedEstablishmentPlace>
        <ID>25002</ID>
      </ApprovedEstablishmentPlace>
      <Commodity>
        <Description>HOT-WATER BOTTLES</Description>
        <ValueAmount currencyID=""NZD"">2500</ValueAmount>
        <Classification>
          <ID>4014900100B</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">403.50</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Source>
          <CountryCode>AU</CountryCode>
        </Source>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">150</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
        <TariffQuantity unitCode=""NMB"">1000.000</TariffQuantity>
      </GoodsMeasure>
      <Origin>
        <CountryCode>CN</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>174824-HY-17</MarksNumbersID>
        <QuantityQuantity>144</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">1</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">155</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">35</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID>G48221</ID>
      <ConditionCode>FOB</ConditionCode>
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
    <Seller>
      <Name>TEST SUPPLIER FOR EXPORT</Name>
      <Address>
        <CityName>SYDNEY</CityName>
        <CountryCode>AU</CountryCode>
        <CountrySubDivisionName>NSW</CountrySubDivisionName>
        <Line>1 GEORGE ST</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
    </Seller>
    <StuffingEstablishment>
      <Name>MAERSK LINES</Name>
      <Address>
        <CityName>Port Botany</CityName>
        <CountryCode>AU</CountryCode>
        <CountrySubDivisionName>NSW</CountrySubDivisionName>
        <Line>1001 Link Rd</Line>
        <PostcodeID>2012</PostcodeID>
      </Address>
    </StuffingEstablishment>
    <Supplier>
      <ID>00710841Y</ID>
      <Contact>
        <Name>Contact Person</Name>
        <Communication>
          <ID>brendon.paine@cargowise.com</ID>
          <TypeID>EM</TypeID>
        </Communication>
        <Communication>
          <ID>+61 (2) 8888-9999</ID>
          <TypeID>TE</TypeID>
        </Communication>
        <Communication>
          <ID>+61 (2) 6666-5557</ID>
          <TypeID>FX</TypeID>
        </Communication>
      </Contact>
    </Supplier>
  </GoodsShipment>
  <Importer>
    <ID>51352368J</ID>
    <Contact>
      <Name>Contact Person</Name>
      <Communication>
        <ID>brendon.paine@cargowise.com</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>+64 (9) 6666-7777</ID>
        <TypeID>TE</TypeID>
      </Communication>
      <Communication>
        <ID>+64 (9) 6666-7777</ID>
        <TypeID>FX</TypeID>
      </Communication>
    </Contact>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>80</QuantityQuantity>
    <TypeCode>CT</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			#endregion // Message Text

			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText =
			#region IncomingMessage Text
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20131115112604</IssueDateTime>
    <FunctionalReferenceID>540</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79569757</ID>
        <AcceptanceDateTime formatCode=""204"">20131115112604</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>822</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			#endregion // Message Text

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Message links to correct Entry Header", entryHeader, incomingMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIFood & MPIBio have not been updated in this test.", "699", Declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should should be extracted from the response on the declaration", "79569757", Declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "822", entryHeader.CH_NZCSStatus);

			// Simulate having processed MPI responses as well as updated DO customs response
			Declaration.JE_TSWCombinedStatus = "000";

			// unsolicited NZ Customs cancel message
			#region CancelMessage Text
			string cancelMessage =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20131115112604</IssueDateTime>
    <FunctionalReferenceID>581</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79569757</ID>
        <AcceptanceDateTime formatCode=""204"">20131115112604</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <CancellationDateTime formatCode=""204"">20131115112604</CancellationDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>814</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			#endregion
			var incomingCancelMessage = Factory.New<TSWMessage>();
			incomingCancelMessage.EM_MessageText = cancelMessage;
			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(incomingCancelMessage);
			AssertEquals("EntryHeader is linked correctly", entryHeader, incomingCancelMessage.EM_LinkedObject);
			AssertEquals("Cancelled EntryHeader should be set to inactive", false, entryHeader.IsActive);
			AssertEquals("Job Entry Status should also reflect the entry has been cancelled", FormalEntryStatusList.Codes.EntryCancelled, Declaration.JE_EntryStatus);

			Declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Add another dummy entry header", 2, Declaration.CustomsEntryHeaders.Count);

			// unsolicited NZ Customs restore message
			#region RestoreMessage Text
			string restoreMessage =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20131115125604</IssueDateTime>
    <FunctionalReferenceID>540</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79569757</ID>
        <AcceptanceDateTime formatCode=""204"">20131115125604</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>815</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			#endregion
			var incomingRestoreMessage = Factory.New<TSWMessage>();
			incomingRestoreMessage.EM_MessageText = restoreMessage;
			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(incomingRestoreMessage);
			AssertEquals("Restore messages finds the correct Header to link this message to correctly", entryHeader, incomingRestoreMessage.EntryHeader);
			AssertEquals("EntryHeader should now be active again", true, entryHeader.IsActive);
			AssertEquals("Should be only 1 Entry Header after this restore message is processed", 1, Declaration.CustomsEntryHeaders.Count);
			AssertEquals("Job Entry Status", FormalEntryStatusList.Codes.EntryRestored, Declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus to reflect entry has been restored", TSWEntryStatusList.Codes.RES, Declaration.JE_TSWCombinedStatus);
		}

		public void TestNoCES_DORorWOFEventUntilAllAgenciesClear()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "B00004810";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var entryHeader = declaration.CusEntryHeader;
			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration.SaveHandlingSaveExceptions();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00004810";
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_MessageText =
			#region OutgoingMessage Text
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>B00004810</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">50</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20191217</JurisdictionDateTime>
  <Submitter>
    <ID>51358596K</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementDescription>tes999,EDI TEST BRANCH NZAKL</StatementDescription>
    <StatementTypeCode>MAC</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>51358596K</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <ApprovedEstablishmentPlace>
    <ID>25004</ID>
  </ApprovedEstablishmentPlace>
  <BorderTransportMeans>
    <Name>QF11</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>
  <Carrier>
    <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
  </Carrier>
  <CurrencyExchange>
    <RateNumeric>1</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID>51361819A</ID>
    <Communication>
      <ID>gary.odea@wisetechglobal.com</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>61280012200</ID>
      <TypeID>TE</TypeID>
    </Communication>
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>D</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">60.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">204.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">264.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>AU</ExportationCountryCode>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <GoodsLocation>
        <ID>7179L</ID>
      </GoodsLocation>
      <LoadingLocation>
        <ID>AUSYD</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>08100562328</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>G628912</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">50</FreightChargeAmount>
      <FreightChargeApportionmentCode>161</FreightChargeApportionmentCode>
    </CustomsValuation>
    <DeliveryDestination>
      <Name>ADBOOKS LTD (NZ CUSTOMS)</Name>
      <Address>
        <CityName>AUCKLAND</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>AUK</CountrySubDivisionName>
        <Line>1 MAIN PLACE</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">1200</CustomsValueAmount>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSP</StatementCode>
        <StatementDescription>N</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <AdditionalInformation>
        <StatementCode>OSR</StatementCode>
        <StatementDescription>141-135-236</StatementDescription>
        <StatementTypeCode>OIN</StatementTypeCode>
      </AdditionalInformation>
      <ApprovedEstablishmentPlace>
        <ID>25004</ID>
      </ApprovedEstablishmentPlace>
      <Commodity>
        <Description>ARTICLES OF PLASTICS OF OTHER MATERIALS ETC</Description>
        <ValueAmount currencyID=""NZD"">1200</ValueAmount>
        <Classification>
          <ID>3926906979F</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">60.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">204.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Source>
          <CountryCode>AU</CountryCode>
        </Source>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">1</NetNetWeightMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>Unknown</MarksNumbersID>
        <QuantityQuantity>0</QuantityQuantity>
        <TypeCode>PK</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">0</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">50</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">50</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID>WI00275884-3</ID>
      <ConditionCode>FOB</ConditionCode>
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
    <Supplier>
      <ID>00710841Y</ID>
    </Supplier>
  </GoodsShipment>
  <Importer>
    <ID>51352368J</ID>
    <Contact>
      <Name>brett testing</Name>
      <Communication>
        <ID>Brett.test@wisetech.com</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </Contact>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>PK</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";
			#endregion // Message Text

			var incomingNZCS_DORMessage = Factory.New<TSWMessage>();
			incomingNZCS_DORMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			incomingNZCS_DORMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingNZCS_DORMessage.EM_MessageText =
			#region incomingNZCS_DORMessage Text
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191217131355</IssueDateTime>
    <FunctionalReferenceID>7811</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>1 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>7963566</ID>
        <AcceptanceDateTime formatCode=""204"">20191217131355</AcceptanceDateTime>
        <FunctionalReferenceID>B00004810</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>D</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">319.71</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191217131355</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20191217131355</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			#endregion // Message Text

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(incomingNZCS_DORMessage);
			AssertEquals("Entry status will be set to DOR", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			declaration.Factory.Save();

			var logProvider = declaration as IStmALogProvider;
			var customsCESEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			customsCESEventQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, FormalEntryStatusList.Codes.DeliveryOrderReceived);
			var customsDORLog = logProvider.Logs.Find(customsCESEventQuery);
			AssertEquals("CES DOR event log should NOT have been created yet for this NZ Import entry, as is not completely clear yet, (other agency messages yet to be processed).", 0, customsDORLog.Length);

			// process other agency messages

			var incomingBioFoodMessage = Factory.New<TSWMessage>();
			incomingBioFoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			incomingBioFoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingBioFoodMessage.EM_MessageText =
			#region incomingBioFoodMessage Text
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191217131338</IssueDateTime>
    <FunctionalReferenceID>7810</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>7963566</ID>
        <AcceptanceDateTime formatCode=""204"">20191217131338</AcceptanceDateTime>
        <FunctionalReferenceID>B00004810</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191217131338</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20191217131338</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			#endregion // Message Text

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(incomingBioFoodMessage);
			AssertEquals("Entry status should remain set to DOR", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);

			var incomingBiosecurityMessage = Factory.New<TSWMessage>();
			incomingBiosecurityMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			incomingBiosecurityMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingBiosecurityMessage.EM_MessageText =
			#region incomingBiosecurityMessage Text
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191217131357</IssueDateTime>
    <FunctionalReferenceID>7812</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>7963566</ID>
        <AcceptanceDateTime formatCode=""204"">20191217131357</AcceptanceDateTime>
        <FunctionalReferenceID>B00004810</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191217131357</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20191217131357</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			#endregion // Message Text

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(incomingBiosecurityMessage);
			AssertEquals("Entry status should remain set to DOR", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);

			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should now have been created as all agencies have cleared the entry", true, logEntries.Length > 0);
			AssertEquals("Only 1 CLR event should be created", 1, logEntries.Length);
			AssertEquals("NZ Import", logEntries[0].SL_Reference);

			var expectedDORLogReference = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			logProvider = declaration;
			var customsDOREventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			customsDOREventQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, expectedDORLogReference);
			customsDORLog = logProvider.Logs.Find(customsDOREventQuery);
			AssertEquals("CES DOR event log should have been created with the CLR event as all Agencies have now cleared the entry.", expectedDORLogReference, customsDORLog[0].SL_Reference);
		}

		[TestDate(2014, 05, 30, 09, 09, 09)]
		public void TestLinePriceMaximum()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(0m, "DUS", 0m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "DUS", 10000m);
			decCreator.SetupImportInvoiceLine("8888.88.88.88A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			var invoiceLine = declaration.FilteredInvoiceLines[0];
			invoiceLine.JI_LinePrice = 999999999999999m;
			AssertEquals("The value is smaller than decimal's maxinum, but larger than money's maximum.", true, invoiceLine.Notifications.HasErrors());

			invoiceLine.JI_LinePrice = 922337203685477.5807m;
			AssertEquals("The value is not larger than money's maximum.", false, invoiceLine.Notifications.HasErrors());

			AssertNoExceptionThrown(() => declaration.Factory.Save());
		}

		public void TesteDocsForSelection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "IM1_Delivery_Order.pdf", "FCT");
			AssertEquals("Declaration document should be available for selection in attachment drop down control", 1, declaration.eDocsForSelection.Count);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <PackingList> %EOF\n"), "PackingList.pdf", "FCT");
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <Invoice> %EOF\n"), "Invoice.pdf", "INV");
			AssertEquals("Declaration & Shipment documents should now be available for selection in attachment drop down control", 2, declaration.eDocsForSelection.Count);

			int docsInCollection = 0;
			foreach (IStorageDocsBaseCollection docCollection in declaration.eDocsForSelection)
			{
				foreach (IeDoc doc in docCollection)
				{
					docsInCollection++;
				}
			}

			AssertEquals("There should be 3 documents in total available for selection", 3, docsInCollection);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "IM1_Delivery_Order2.pdf", "FCT");
			declaration.ActiveEntryHeaders.AddNew();
			declaration2.ActiveEntryHeaders.AddNew();
			declaration.JE_EntryStatus = declaration2.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			var consolidatedDeclaration = Factory.NewWithValidTestData<ConsolidatedDeclaration>();
			consolidatedDeclaration.JobDeclarations.Add(declaration);
			consolidatedDeclaration.JobDeclarations.Add(declaration2);

			AssertEquals("Once consolidated, declaration aggregate all eDocs for selection", 4, declaration2.eDocsForSelection.Sum(x => x.Count));
		}

		public void TestAutoRatingForExportDeclarations()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.Invoices.AddNew();

			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(1, declaration.ActiveEntryHeaders[0].MergedLines.Count);

			var entries = ((IAutoRatingCustomsInfo)declaration.RatingAdapter).Entries;
			AssertEquals("should be one entry", 1, entries.Count);
			AssertEquals("one entry line", 1, entries[0].EntryLines);
		}

		public void TestJE_Cal_GoodsLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportModeCodeList.Codes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			declaration.JE_RL_NKFinalDestination = "NZWLG";

			var fwOrg = Factory.NewWithValidTestData<OrgHeader>();
			fwOrg.OH_Code = "FWCODE";
			fwOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "FW-OH-000", Core.Constants.CountryCodes.NewZealand);
			declaration.JE_OH_Forwarder = fwOrg.PK;

			AssertEquals("Should be equal to JE_Cal_GoodsLocationMaxLength.", JobDeclaration.Schema.JE_Cal_GoodsLocationMaxLength, declaration.JE_Cal_GoodsLocationInfo.MaxLength);
			AssertEquals("Should be equal to JE_Cal_GoodsLocationMaxLength.", JobDeclaration.Schema.JE_Cal_GoodsLocationMaxLength, declaration.JE_LocationOfGoodsInfo.MaxLength);

			declaration.JE_GoodsLocatedAt = GoodsLocatedAtListForSeaImport.Codes.DES;
			AssertEquals("Goods Location is Final Destination", "NZWLG", declaration.JE_Cal_GoodsLocation);
			AssertEquals("Goods Location is Final Destination", "NZWLG", declaration.JE_LocationOfGoods);
			Assert("Should be readonly when JE_GoodsLocatedAt is DES or DIS.", declaration.JE_Cal_GoodsLocationInfo.ReadOnly);

			declaration.JE_RL_NKFinalDestination = "NZAKL";
			AssertEquals("Goods Location is Final Destination", "NZAKL", declaration.JE_Cal_GoodsLocation);
			AssertEquals("Goods Location is Final Destination", "NZAKL", declaration.JE_LocationOfGoods);

			declaration.JE_LocationOfGoods = string.Empty;
			AssertEquals("Goods Location is get from calculation.", "NZAKL", declaration.JE_Cal_GoodsLocation);
			AssertEquals("Should be empty", string.Empty, declaration.JE_LocationOfGoods);

			declaration.JE_GoodsLocatedAt = GoodsLocatedAtListForSeaImport.Codes.DIS;
			AssertEquals("Goods Location is now Port of Discharge", "NZAKL", declaration.JE_Cal_GoodsLocation);
			Assert("Should be readonly when JE_GoodsLocatedAt is DES or DIS.", declaration.JE_Cal_GoodsLocationInfo.ReadOnly);

			declaration.JE_GoodsLocatedAt = GoodsLocatedAtListForSeaImport.Codes.FW;
			AssertEquals("Goods Location is forwarder", "FW-OH-000", declaration.JE_Cal_GoodsLocation);
			Assert("Should not be readonly when JE_GoodsLocatedAt is not DES or DIS.", !declaration.JE_Cal_GoodsLocationInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKPortOfLoading = "NZWLG";
			declaration.JE_GoodsLocatedAt = GoodsLocatedAtListForSeaExport.Codes.PC;
			AssertEquals("Goods Location is now Port of Loading", "NZWLG", declaration.JE_Cal_GoodsLocation);
			Assert("Should be readonly when JE_GoodsLocatedAt is PC.", declaration.JE_Cal_GoodsLocationInfo.ReadOnly);
		}

		public void TestGoodsLocationOrg()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var fwOrg = Factory.NewWithValidTestData<OrgHeader>();
			fwOrg.OH_Code = "FWCODE";
			declaration.JE_OH_Forwarder = fwOrg.PK;

			var slOrg = Factory.NewWithValidTestData<OrgHeader>();
			slOrg.OH_Code = "SLCODE";
			declaration.JE_OH_ShippingLine = slOrg.PK;

			var whOrg = Factory.NewWithValidTestData<OrgHeader>();
			whOrg.OH_Code = "WHCODE";
			declaration.WarehouseDocAddress.E2_OA_Address = whOrg.MainAddress.PK;

			var cyOrg = Factory.NewWithValidTestData<OrgHeader>();
			cyOrg.OH_Code = "CYCODE";
			declaration.ContainerYardDocAddress.E2_OA_Address = cyOrg.MainAddress.PK;

			var ctOrg = Factory.NewWithValidTestData<OrgHeader>();
			ctOrg.OH_Code = "CTCODE";
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ctOrg.MainAddress.PK;

			var deOrg = Factory.NewWithValidTestData<OrgHeader>();
			deOrg.OH_Code = "DECODE";
			declaration.DepotDocAddress.E2_OA_Address = deOrg.MainAddress.PK;

			fwOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "FW-OH-000", Core.Constants.CountryCodes.NewZealand);
			slOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "C0-OH-000", Core.Constants.CountryCodes.NewZealand);
			whOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "BW-OH-000", Core.Constants.CountryCodes.NewZealand);
			cyOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CY-OH-000", Core.Constants.CountryCodes.NewZealand);
			ctOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CTO-OH-00", Core.Constants.CountryCodes.NewZealand);
			deOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "D0-OH-000", Core.Constants.CountryCodes.NewZealand);

			CombineAssertions(() =>
			{
				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
				AssertEquals("Bonded Warehouse", whOrg, declaration.GetGoodsLocationOrg());
				AssertEquals("Bonded Warehouse - JE_Cal_GoodsLocation", "BW-OH-000", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Bonded Warehouse - JE_LocationOfGoods", "BW-OH-000", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.C;
				AssertEquals("Shipping Line", slOrg, declaration.GetGoodsLocationOrg());
				AssertEquals("Shipping Line - JE_Cal_GoodsLocation", "C0-OH-000", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Shipping Line - JE_LocationOfGoods", "C0-OH-000", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.CTO;
				AssertEquals("Container Terminal Operator", ctOrg, declaration.GetGoodsLocationOrg());
				AssertEquals("Container Terminal Operator - JE_Cal_GoodsLocation", "CTO-OH-00", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Container Terminal Operator - JE_LocationOfGoods", "CTO-OH-00", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.CY;
				AssertEquals("Container Yard", cyOrg, declaration.GetGoodsLocationOrg());
				AssertEquals("Container Yard - JE_Cal_GoodsLocation", "CY-OH-000", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Container Yard - JE_LocationOfGoods", "CY-OH-000", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.D;
				AssertEquals("Depot", deOrg, declaration.GetGoodsLocationOrg());
				AssertEquals("Depot - JE_Cal_GoodsLocation", "D0-OH-000", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Depot - JE_LocationOfGoods", "D0-OH-000", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.FW;
				AssertEquals("Forwarder", fwOrg, declaration.GetGoodsLocationOrg());
				AssertEquals("Forwarder - JE_Cal_GoodsLocation", "FW-OH-000", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Forwarder - JE_LocationOfGoods", "FW-OH-000", declaration.JE_LocationOfGoods);
			});
		}

		public void TestResetContainerModeForNZAirJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			var containers = declaration.CusContainers;
			var container1 = containers.AddNew();
			container1.CO_ContainerNumber = "C1";
			var container2 = containers.AddNew();
			container2.CO_ContainerNumber = "C2";
			Factory.Save();
			AssertEquals("CNT", declaration.JE_ContainerMode);

			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Factory.Save();
			AssertEquals("No container", 0, declaration.CusContainers.Count);
			AssertEquals("", declaration.JE_ContainerMode);

			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Factory.Save();
			AssertEquals(0, declaration.CusContainers.Count);
			AssertEquals("", declaration.JE_ContainerMode);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = JobTransportModeList.Codes.Sea;
			var containers2 = declaration2.CusContainers;
			var container3 = containers2.AddNew();
			container3.CO_ContainerNumber = "C1";
			AssertEquals("CNT", declaration2.JE_ContainerMode);
			declaration2.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("", declaration2.JE_ContainerMode);
			declaration2.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("CNT", declaration2.JE_ContainerMode);
		}

		public void TestGetGoodsLocationAddressPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var fwOrg = Factory.NewWithValidTestData<OrgHeader>();
			fwOrg.OH_Code = "FWCODE";
			declaration.JE_OH_Forwarder = fwOrg.PK;

			var slOrg = Factory.NewWithValidTestData<OrgHeader>();
			slOrg.OH_Code = "SLCODE";
			declaration.JE_OH_ShippingLine = slOrg.PK;

			var whOrg = Factory.New<OrgHeader>();
			whOrg.OH_Code = "WHCODE";
			declaration.WarehouseDocAddress.OrganisationPK = whOrg.PK;

			var whOrgAddr2 = whOrg.Addresses.AddNew();
			whOrgAddr2.OA_Address1 = "Bonded Warehouse alternate address";
			whOrgAddr2.OA_Code = "WH2";
			declaration.WarehouseDocAddress.E2_OA_Address = whOrgAddr2.PK;

			var cyOrg = Factory.New<OrgHeader>();
			cyOrg.OH_Code = "CYCODE";

			var cyAddr = cyOrg.MainAddress;

			declaration.ContainerYardDocAddress.E2_OA_Address = cyAddr.PK;

			var ctOrg = Factory.New<OrgHeader>();
			ctOrg.OH_Code = "CTCODE";

			var ctOrgAddr2 = ctOrg.Addresses.AddNew();
			ctOrgAddr2.OA_Address1 = "CTO alternate address";
			ctOrgAddr2.OA_Code = "CTO2";
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ctOrgAddr2.PK;

			var deOrg = Factory.New<OrgHeader>();
			deOrg.OH_Code = "DECODE";

			var deOrgAddr2 = deOrg.Addresses.AddNew();
			deOrgAddr2.OA_Address1 = "Depot alternate address";
			deOrgAddr2.OA_Code = "CTO2";
			declaration.DepotDocAddress.E2_OA_Address = deOrgAddr2.PK;

			fwOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "FW-OH-000", Core.Constants.CountryCodes.NewZealand);
			slOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "C0-OH-000", Core.Constants.CountryCodes.NewZealand);
			whOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "BW-OH-000", Core.Constants.CountryCodes.NewZealand);
			cyOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CY-OH-000", Core.Constants.CountryCodes.NewZealand);
			ctOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CTO-OH-00", Core.Constants.CountryCodes.NewZealand);
			deOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "D0-OH-000", Core.Constants.CountryCodes.NewZealand);

			whOrgAddr2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "BW-OA-001", Core.Constants.CountryCodes.NewZealand);
			cyAddr.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CY-OA-001", Core.Constants.CountryCodes.NewZealand);
			ctOrgAddr2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CTO-OA-001", Core.Constants.CountryCodes.NewZealand);
			deOrgAddr2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "D0-OA-001", Core.Constants.CountryCodes.NewZealand);

			Factory.Save();

			CombineAssertions(() =>
			{
				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
				AssertEquals("Bonded Warehouse", whOrgAddr2.PK, declaration.GetGoodsLocationAddressPK());
				AssertEquals("Bonded Warehouse - JE_Cal_GoodsLocation", "BW-OA-001", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Bonded Warehouse - JE_LocationOfGoods", "BW-OA-001", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.C;
				AssertEquals("Shipping Line - does not use jobDocAddress", ZGuid.Empty, declaration.GetGoodsLocationAddressPK());
				AssertEquals("Shipping Line - does not use jobDocAddress - JE_Cal_GoodsLocation", "C0-OH-000", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Shipping Line - does not use jobDocAddress - JE_LocationOfGoods", "C0-OH-000", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.CTO;
				AssertEquals("Container Terminal Operator", ctOrgAddr2.PK, declaration.GetGoodsLocationAddressPK());
				AssertEquals("Container Terminal Operator - JE_Cal_GoodsLocation", "CTO-OA-001", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Container Terminal Operator - JE_LocationOfGoods", "CTO-OA-001", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.CY;
				AssertEquals("Container Yard", cyAddr.PK, declaration.GetGoodsLocationAddressPK());
				AssertEquals("Container Yard - JE_Cal_GoodsLocation", "CY-OA-001", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Container Yard - JE_LocationOfGoods", "CY-OA-001", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.D;
				AssertEquals("Depot", deOrgAddr2.PK, declaration.GetGoodsLocationAddressPK());
				AssertEquals("Depot - JE_Cal_GoodsLocation", "D0-OA-001", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Depot - JE_LocationOfGoods", "D0-OA-001", declaration.JE_LocationOfGoods);

				declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.FW;
				AssertEquals("Forwarder - does not use jobDocAddress", ZGuid.Empty, declaration.GetGoodsLocationAddressPK());
				AssertEquals("Forwarder - does not use jobDocAddress - JE_Cal_GoodsLocation", "FW-OH-000", declaration.JE_Cal_GoodsLocation);
				AssertEquals("Forwarder - does not use jobDocAddress - JE_LocationOfGoods", "FW-OH-000", declaration.JE_LocationOfGoods);
			});
		}

		public void TestReciprocalRates()
		{
			Assert(!Factory.New<JobDeclaration>().IsReciprocalRates);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.NewZealand, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
		}

		public void TestCompletionEntryNumber()
		{
			AssertEquals("Declaration default", ZString.Empty, Declaration.CompletionEntryNumber);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			AssertEquals("Declaration entry header is not completion entry", ZString.Empty, Declaration.CompletionEntryNumber);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			var completionEntry = Declaration.CusEntryHeader;
			completionEntry.EntryNumber = "1234567";
			AssertEquals("TSW Declaration completion entry needs to be sent as a new original entry", "1234567", Declaration.CompletionEntryNumber);
		}

		public void TestEmptyJE_ContainerModeWithAutoRating()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = "FCL";

			try
			{
				declaration.JE_ContainerMode = ZString.Empty;
				AssertEquals("NZ ContainerMode set to empty", ErrorReporter.LastKeyReported);

				AssertEquals(FreightMode.FCL, declaration.RatingAdapter.FreightMode);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestIsQuarantineGroupBoxVisible()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("IsQuarantineGroupBoxVisible - default Export job", true, Declaration.IsQuarantineGroupBoxVisible);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsQuarantineGroupBoxVisible - Import job", true, Declaration.IsQuarantineGroupBoxVisible);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsQuarantineGroupBoxVisible - Import TSW job", true, Declaration.IsQuarantineGroupBoxVisible);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsQuarantineGroupBoxVisible - Export TSW job", false, Declaration.IsQuarantineGroupBoxVisible);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsQuarantineGroupBoxVisible - Import TSW write-off (ICR) job requires these fields", true, Declaration.IsQuarantineGroupBoxVisible);
		}

		public void TestJE_GoodsLocatedAtVisible()
		{
			AssertEquals("JE_GoodsLocatedAtVisible - default Export job", false, Declaration.JE_GoodsLocatedAtVisible);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JE_GoodsLocatedAtVisible - Import job", false, Declaration.JE_GoodsLocatedAtVisible);

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("JE_GoodsLocatedAtVisible - TSW Import job", true, Declaration.JE_GoodsLocatedAtVisible);
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("JE_GoodsLocatedAtVisible - TSW Export job", true, Declaration.JE_GoodsLocatedAtVisible);
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Post;
			AssertEquals("JE_GoodsLocatedAtVisible - required for Post", true, Declaration.JE_GoodsLocatedAtVisible);
		}

		public void TestSetDefaultGoodsLocatedAt()
		{
			CombineAssertions("IMP", () =>
			{
				fDeclaration = null;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Starting MsgType", JobMessageTypeList.Codes.Import, Declaration.JE_MessageType);
				AssertEquals("Starting MsgSubType", JobMessageSubTypeList.Codes.Normal, Declaration.JE_MessageSubType);
				AssertEquals("Starting TransMode", string.Empty, Declaration.JE_TransportMode);
				AssertEquals("Starting applicationCode", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);
				AssertEquals("Starting GoodsLocatesAt", string.Empty, Declaration.JE_GoodsLocatedAt);

				Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				AssertEquals("Air-CTO", GoodsLocatedAtList.Codes.CTO, Declaration.JE_GoodsLocatedAt);
				Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
				AssertEquals("Sea-DIS-UponTransModeChange", GoodsLocatedAtListForSeaImport.Codes.DIS, Declaration.JE_GoodsLocatedAt);
				Declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
				AssertEquals("ChangeChecker", GoodsLocatedAtList.Codes.BW, Declaration.JE_GoodsLocatedAt);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Sea-DIS-UponMsgTypeChange", GoodsLocatedAtListForSeaImport.Codes.DIS, Declaration.JE_GoodsLocatedAt);
			});
			CombineAssertions("EXP", () =>
			{
				fDeclaration = null;
				AssertEquals("Starting MsgType", JobMessageTypeList.Codes.Export, Declaration.JE_MessageType);
				AssertEquals("Starting MsgSubType", JobMessageSubTypeList.Codes.Normal, Declaration.JE_MessageSubType);
				AssertEquals("Starting TransMode", string.Empty, Declaration.JE_TransportMode);
				AssertEquals("Starting applicationCode", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);
				AssertEquals("Starting GoodsLocatesAt", string.Empty, Declaration.JE_GoodsLocatedAt);

				Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				AssertEquals("Air-CTO", GoodsLocatedAtList.Codes.CTO, Declaration.JE_GoodsLocatedAt);
				Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
				AssertEquals("Sea-PC-UponTransModeChange", GoodsLocatedAtListForSeaExport.Codes.PC, Declaration.JE_GoodsLocatedAt);
				Declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
				AssertEquals("ChangeChecker", GoodsLocatedAtList.Codes.BW, Declaration.JE_GoodsLocatedAt);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Sea-PC-UponMsgTypeChange", GoodsLocatedAtListForSeaExport.Codes.PC, Declaration.JE_GoodsLocatedAt);
			});
			CombineAssertions("CRE", () =>
			{
				fDeclaration = null;
				Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				AssertEquals("Starting MsgType", JobMessageTypeList.Codes.Export, Declaration.JE_MessageType);
				AssertEquals("Starting MsgSubType", JobMessageSubTypeList.Codes.WriteOff, Declaration.JE_MessageSubType);
				AssertEquals("Starting TransMode", string.Empty, Declaration.JE_TransportMode);
				AssertEquals("Starting applicationCode", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);
				AssertEquals("Starting GoodsLocatesAt", string.Empty, Declaration.JE_GoodsLocatedAt);

				Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				AssertEquals("Air-CTO", GoodsLocatedAtList.Codes.CTO, Declaration.JE_GoodsLocatedAt);
				Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
				AssertEquals("Sea-PC-UponTransModeChange", GoodsLocatedAtListForSeaExport.Codes.PC, Declaration.JE_GoodsLocatedAt);
				Declaration.JE_GoodsLocatedAt = GoodsLocatedAtList.Codes.BW;
				AssertEquals("ChangeChecker", GoodsLocatedAtList.Codes.BW, Declaration.JE_GoodsLocatedAt);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Sea-PC-UponMsgTypeChange", GoodsLocatedAtListForSeaExport.Codes.PC, Declaration.JE_GoodsLocatedAt);
			});
		}

		public void TestNZIsMergeDone()
		{
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = jobdeclaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			var nzEntry = jobdeclaration.CusEntryHeader;
			nzEntry.CH_MessageType = Customs.Business.CusEntryHeader.EntryHeaderTypes.NZ.FormalEntry;
			nzEntry.CH_JE = jobdeclaration.PK;
			nzEntry.CH_Status = MessageStatusList.Codes.ClearOriginal;
			jobdeclaration.CustomsEntryHeaders.Add(nzEntry);

			Assert(jobdeclaration.ActiveEntryHeaders.Count > 0);
			Assert(!jobdeclaration.IsMergeDone);

			var entryHeader = jobdeclaration.ActiveEntryHeaders[0];
			entryHeader.Messages.AddNew();
			entryHeader.MergedLines.AddNew();

			Assert(jobdeclaration.IsMergeDone);
		}

		public void TestCurrencyIndicator_On_JE_MessageType_Changed_AreResetToDefaultValues()
		{
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice1 = jobdeclaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.NewZealand;
			invoice1.JZ_ExchangeRateIndicator = "xxx";
			invoice1.JobComInvoiceLines.AddNew();

			var invoice2 = jobdeclaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			invoice2.JZ_ExchangeRateIndicator = "yyy";
			invoice2.JobComInvoiceLines.AddNew();

			var invoice3 = jobdeclaration.Invoices.AddNew();
			invoice3.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			invoice3.JZ_ExchangeRateIndicator = "zzz";
			invoice3.JobComInvoiceLines.AddNew();

			Assert("Job declaration has more than 1 invoices", jobdeclaration.Invoices.Count >= 2);

			jobdeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertCurrencyIndicatorIsResetToDefaultValues(jobdeclaration);

			jobdeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertCurrencyIndicatorIsResetToDefaultValues(jobdeclaration);
		}

		void AssertCurrencyIndicatorIsResetToDefaultValues(JobDeclaration jobDeclaration)
		{
			if (jobDeclaration.IsExport)
			{
				var currencyIndicators = new Dictionary<string, string>();
				foreach (JobComInvoiceHeader invoiceHeader in jobDeclaration.Invoices)
				{
					if (invoiceHeader.JZ_RX_NKInvoice_Currency == Enterprise.Core.Constants.CurrencyCodes.NewZealand)
					{
						AssertEquals(
							"JZ_ExchangeRateIndicator is reset to NZD for NZD currency",
							ExchangeRateIndicatorList.Codes.NZD,
							invoiceHeader.JZ_ExchangeRateIndicator);
					}
					else
					{
						if (!currencyIndicators.ContainsKey(invoiceHeader.JZ_RX_NKInvoice_Currency))
						{
							currencyIndicators[invoiceHeader.JZ_RX_NKInvoice_Currency] = invoiceHeader.JZ_ExchangeRateIndicator;
						}
						else
						{
							AssertEquals(
								"JZ_ExchangeRateIndicator is reset to be the same as for the same foreign currency",
								currencyIndicators[invoiceHeader.JZ_RX_NKInvoice_Currency],
								invoiceHeader.JZ_ExchangeRateIndicator);
						}
					}
				}
			}
			else
			{
				foreach (JobComInvoiceHeader invoiceHeader in jobDeclaration.Invoices)
				{
					if (invoiceHeader.JZ_RX_NKInvoice_Currency == Enterprise.Core.Constants.CurrencyCodes.NewZealand)
					{
						AssertEquals(
							"JZ_ExchangeRateIndicator is reset to NZD for NZD currency",
							ExchangeRateIndicatorList.Codes.NZD,
							invoiceHeader.JZ_ExchangeRateIndicator);
					}
					else
					{
						AssertEquals(
							"JZ_ExchangeRateIndicator is reset to empty when the JE_MessageType is changed to Import on foreign currencies",
							ZString.Empty,
							invoiceHeader.JZ_ExchangeRateIndicator);
					}
				}
			}
		}

		public void TestJE_TSWCombinedStatusDescription()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JE_TSWCombinedStatusDesc", "", Declaration.JE_TSWCombinedStatusDesc);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			AssertEquals("JE_TSWCombinedStatusDesc", "MPI Food and Biosecurity Cleared, pending NZCS response", Declaration.JE_TSWCombinedStatusDesc);

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.EEE;
			AssertEquals("JE_TSWCombinedStatusDesc", "MPI Food and Biosecurity and NZCS in error", Declaration.JE_TSWCombinedStatusDesc);

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.NCC;
			AssertEquals("Code is in the list", "MPI Food and MPI Biosecurity Cleared", Declaration.JE_TSWCombinedStatusDesc);

			var writeOffDec = Factory.NewWithValidTestData<JobDeclaration>();
			writeOffDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			writeOffDec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			writeOffDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("Pre-condition: combined status before declaration is sent", "", writeOffDec.JE_TSWCombinedStatusDesc);

			writeOffDec.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.CP;
			AssertEquals("MPI Pending / Customs Cleared", "NZCS - Consignment Written Off / BIO - Pending", writeOffDec.JE_TSWCombinedStatusDesc);

			writeOffDec.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.CC;
			AssertEquals("Bio Cleared / Customs Cleared", "Consignment Written Off/Cleared", writeOffDec.JE_TSWCombinedStatusDesc);

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.NCP;
			AssertEquals("Code for IPI responses is in the list", "Pending MPI Food, MPI Biosecurity Cleared", Declaration.JE_TSWCombinedStatusDesc);

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.NCE;
			AssertEquals("Code for IPI responses is in the list", "MPI Food in error, MPI Biosecurity Cleared", Declaration.JE_TSWCombinedStatusDesc);

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.NPC;
			AssertEquals("Code for IPI responses is in the list", "MPI Food Cleared, pending MPI Biosecurity", Declaration.JE_TSWCombinedStatusDesc);

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PPI;
			AssertEquals("Description for inspection &/or audit requirements has changed", "MPI Food Inspection / Audit requirements, pending MPI Biosecurity and NZCS responses", Declaration.JE_TSWCombinedStatusDesc);

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.IIT;
			AssertEquals("Description for inspection &/or audit requirements has changed", "MPI Food Inspection Confirmation of Transaction, MPI Biosecurity and NZCS Inspection / Audit requirements", Declaration.JE_TSWCombinedStatusDesc);
		}

		public void TestIsAttachedToShipment()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsAttachedToShipment", false, Declaration.IsAttachedToShipment);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Declaration.JE_JS = shipment.PK;
			AssertEquals("IsAttachedToShipment", true, Declaration.IsAttachedToShipment);
		}

		public void TestJE_TSWCombinedStatusVisible()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JE_TSWCombinedStatusVisible should be visible - TSW is fully active", true, Declaration.JE_TSWCombinedStatusVisible);
		}

		public void TestIsNZCSDeliveryOnPayment()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsNZCSDeliveryOnPayment", false, Declaration.IsNZCSDeliveryOnPayment);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var entryHeader = Declaration.CusEntryHeader;
			entryHeader.CH_NZCSStatus = StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified;
			AssertEquals("IsNZCSDeliveryOnPayment", false, Declaration.IsNZCSDeliveryOnPayment);

			entryHeader.CH_NZCSStatus = StatusList.Codes.EntryClearedCashToPayPriorToDeliveryEntryRoutedToDocumentAudit;
			AssertEquals("IsNZCSDeliveryOnPayment", true, Declaration.IsNZCSDeliveryOnPayment);
		}

		public void TestATFFromOrganisation()
		{
			var deliveryAddressOrg = Factory.New<OrgHeader>();
			deliveryAddressOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "1234567890123");
			deliveryAddressOrg.OH_RL_NKClosestPort = "NZAKL";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = deliveryAddressOrg.PK;

			declaration.ImporterDeliveryAddress.OrganisationPK = deliveryAddressOrg.PK;
			AssertEquals("JE_ATF", "1234567890", declaration.JE_ATFOtherInfoValue);
		}

		public void TestDefaultingOfATFCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var atfOrg = Factory.New<OrgHeader>();
			atfOrg.OH_FullName = "John Smith Industries P/L";
			atfOrg.OH_Code = "JOHSMIAKL";
			atfOrg.OH_RL_NKClosestPort = "NZAKL";
			atfOrg.MainAddress.OA_Address1 = "1 Main St.";
			var atfDepot1Address = atfOrg.Addresses.AddNew();
			atfDepot1Address.OA_Address1 = "JSI Depot 1";
			var atfDepot2Address = atfOrg.Addresses.AddNew();
			atfDepot2Address.OA_Address1 = "JSI Depot 2";
			var atfWarehouseAddress = atfOrg.Addresses.AddNew();
			atfWarehouseAddress.OA_Address1 = "JSI Warehouse";

			atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "7015H");
			var atf2 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "7039J");
			atf2.OK_OA_PremisesAddress = atfOrg.MainAddress.PK;
			var atf3 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "2075R");
			atf3.OK_OA_PremisesAddress = atfDepot1Address.PK;
			var atf4 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "4619W");
			atf4.OK_OA_PremisesAddress = atfDepot2Address.PK;
			var atf5 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "1270P");
			atf5.OK_OA_PremisesAddress = atfWarehouseAddress.PK;

			declaration.JE_OH_Importer = atfOrg.PK;
			AssertEquals("ATF Code from appropriate premise address (Main Address) is being defaulted - Declaration.JE_ATFOtherInfoValue should default to '7039J'", "7039J", declaration.JE_ATFOtherInfoValue);

			declaration.DepotDocAddress.E2_OA_Address = atf4.OK_OA_PremisesAddress;
			AssertEquals("ATF Code from appropriate premise address (JSI Depot 2) is being defaulted - Declaration.JE_ATFOtherInfoValue should default to '4619W'", "4619W", declaration.JE_ATFOtherInfoValue);

			declaration.DepotDocAddress.E2_OA_Address = atf5.OK_OA_PremisesAddress;
			AssertEquals("ATF Code from appropriate premise address (JSI Warehouse) is being defaulted - Declaration.JE_ATFOtherInfoValue should default to '1270P'", "1270P", declaration.JE_ATFOtherInfoValue);
		}

		public void TestDefaultingOfATFCodeFromShipment()
		{
			var atfOrg = Factory.New<OrgHeader>();
			atfOrg.OH_FullName = "John Smith Industries P/L";
			atfOrg.OH_Code = "JOHSMIAKL";
			atfOrg.OH_RL_NKClosestPort = "NZAKL";
			atfOrg.MainAddress.OA_Address1 = "1 Main St.";
			var atfDepot1Address = atfOrg.Addresses.AddNew();
			atfDepot1Address.OA_Address1 = "JSI Depot 1";
			var atfDepot2Address = atfOrg.Addresses.AddNew();
			atfDepot2Address.OA_Address1 = "JSI Depot 2";
			var atfDepot3Address = atfOrg.Addresses.AddNew();
			atfDepot3Address.OA_Address1 = "JSI Depot 3";
			var atfDepot4Address = atfOrg.Addresses.AddNew();
			atfDepot4Address.OA_Address1 = "JSI Depot 4";

			atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "7015H");
			var atf2 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "7039J");
			atf2.OK_OA_PremisesAddress = atfOrg.MainAddress.PK;
			var atf3 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "2075R");
			atf3.OK_OA_PremisesAddress = atfDepot1Address.PK;
			var atf4 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "4619W");
			atf4.OK_OA_PremisesAddress = atfDepot2Address.PK;
			var atf5 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "1270P");
			atf5.OK_OA_PremisesAddress = atfDepot3Address.PK;
			var atf6 = atfOrg.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "9876E");
			atf6.OK_OA_PremisesAddress = atfDepot4Address.PK;
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = atfOrg.PK;
			var newDeclaration = Factory.New<JobDeclaration>();
			newDeclaration.JE_ExportDate = new ZDateTime(2019, 7, 10);
			newDeclaration.JE_DateOfArrival = new ZDateTime(2019, 7, 11);
			newDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			newDeclaration.JE_JS = shipment.PK;

			var declaration = (JobDeclaration)shipment.GetDeclaration();
			((IJobDeclarationWithShipmentSynchonisation)declaration).SynchroniseWithShipmentIfNeeded();

			AssertEquals("ATF Code from Shipment Consignee Main Address", "7039J", declaration.JE_ATFOtherInfoValue);

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = atfDepot1Address.PK;
			AssertEquals("ATF Code is updated from Shipment Delivery Address (JSI Depot 1) when delivery address is set", "2075R", declaration.JE_ATFOtherInfoValue);

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = atfDepot2Address.PK;
			AssertEquals("ATF Code is updated from Shipment Delivery Address (JSI Depot 2) when delivery address is changed", "4619W", declaration.JE_ATFOtherInfoValue);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var shipmentF2 = factory2.Load<ForwardingShipment>(shipment.PK);
			var declarationF2 = (JobDeclaration)shipmentF2.GetDeclaration();
			((IJobDeclarationWithShipmentSynchonisation)declarationF2).SynchroniseWithShipmentIfNeeded();

			AssertEquals(false, declarationF2.JE_OverrideFreightDefaults);
			AssertEquals("ATF Code is premises address (JSI Depot 2) as saved", "4619W", declarationF2.JE_ATFOtherInfoValue);

			AssertSame(shipmentF2, declarationF2.Shipment);
			shipmentF2.ConsigneeDeliveryAddress.E2_OA_Address = atfDepot3Address.PK;
			AssertEquals("ATF Code is updated from Shipment Delivery Address (JSI Depot 3) when not overridden", "1270P", declarationF2.JE_ATFOtherInfoValue);

			declarationF2.JE_OverrideFreightDefaults = true;
			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;
			var shipmentF3 = factory3.Load<ForwardingShipment>(shipment.PK);
			var declarationF3 = (JobDeclaration)shipmentF3.GetDeclaration();
			((IJobDeclarationWithShipmentSynchonisation)declarationF3).SynchroniseWithShipmentIfNeeded();

			AssertEquals(true, declarationF3.JE_OverrideFreightDefaults);
			AssertEquals("ATF Code is premises address (JSI Depot 3) as saved", "1270P", declarationF3.JE_ATFOtherInfoValue);

			shipmentF3.ConsigneeDeliveryAddress.E2_OA_Address = atfDepot4Address.PK;
			AssertEquals("ATF Code still updated from Shipment Delivery Address (JSI Depot 4) when is overridden", "9876E", declarationF3.JE_ATFOtherInfoValue);
		}

		[TestDate(2019, 7, 30)]
		public void TestJE_ECI_InvoiceAmountInLocalCurrency()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ECI_InvoiceCurrency = Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "NZD").PK;
			Declaration.JE_ECI_InvoiceAmount = 400m;
			AssertEquals(400m, Declaration.JE_ECI_InvoiceAmountInLocalCurrency);

			Declaration.JE_ECI_InvoiceCurrency = Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "USD").PK;
			Declaration.JE_ECI_InvoiceAmount = 400m;
			AssertEquals(287.77m, Declaration.JE_ECI_InvoiceAmountInLocalCurrency);

			Declaration.JE_ECI_InvoiceCurrency = ZGuid.Empty;
			Declaration.JE_ECI_InvoiceAmount = 400m;
			AssertEquals("For invalid currency, we use local currency as fallback.", 400m, Declaration.JE_ECI_InvoiceAmountInLocalCurrency);

			Declaration.JE_ECI_InvoiceCurrency = ZGuid.NewZGuid();
			Declaration.JE_ECI_InvoiceAmount = 400m;
			AssertEquals("For invalid currency, we use local currency as fallback.", 400m, Declaration.JE_ECI_InvoiceAmountInLocalCurrency);
		}

		[ExpectNoExceptions]
		public void TestDefaultSEPCodeFromSupplierOnExportClearances()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supplier = Factory.New<OrgHeader>();
			var cusCode = supplier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.NZCodeTypes.SecureExportPartner;
			cusCode.OK_CustomsRegNo = "SEP5136119";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Pre-condiion", declaration.IsExport);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("SEP5136119", declaration.JE_SEPOtherInfoValue);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			cusCode.OK_CustomsRegNo = "SEP51361196L";
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("SEP51361196L", declaration.JE_SEPOtherInfoValue);
		}

		public void TestCOOValidForTSW()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TEOTH", "TEOTH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTH", "COO", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var otherInfo = Declaration.OtherInfos.AddNew();
			otherInfo.ZO_Code = "COO";
			otherInfo.ZO_Data = "1234567890";
			otherInfo.ValidateZO_Code();
			AssertNoMessageErrors(otherInfo.ZO_CodeInfo);
		}

		public void TestNZCSStatusDesc()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("NZCSStatusDesc", "", Declaration.NZCSStatusDesc);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var entryHeader = Declaration.CusEntryHeader;
			entryHeader.CH_NZCSStatus = StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecifiedPleaseNoteWarningsCorrectIfNecessary;
			AssertEquals("NZCSStatusDesc", "Delivery Order Herewith, method of payment as specified - Please note warnings, correct if necessary", Declaration.NZCSStatusDesc);
		}

		public void TestMPIFoodStatusDesc()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("MPIFoodStatusDesc", "", Declaration.MPIFoodStatusDesc);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var entryHeader = Declaration.CusEntryHeader;
			entryHeader.CH_MPIFoodStatus = StatusList.Codes.MPIFoodDirectionsGiven;
			AssertEquals("MPIFoodStatusDesc", "MPI Food - Directions Given", Declaration.MPIFoodStatusDesc);
		}

		public void TestMPIBiosecurityStatusDesc()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("MPIBiosecurityStatusDesc", "", Declaration.MPIBiosecurityStatusDesc);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var entryHeader = Declaration.CusEntryHeader;
			entryHeader.CH_MPIBioStatus = StatusList.Codes.MPIBiosecurityCleared;
			AssertEquals("MPIBiosecurityStatusDesc", "MPI Biosecurity - Cleared", Declaration.MPIBiosecurityStatusDesc);
		}

		public void TestPackingLinesGetAddedAtMBillLevelForDirects()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MasterBill = "MASTERBILL1";
			declaration.JE_TotalNoOfPacks = 12;
			declaration.JE_TotalNoOfPacksPackType = "CS";

			AssertEquals(1, declaration.Packages.Count);
			var package = declaration.Packages[0];

			AssertEquals(12, package.CW_PackQty);
			AssertEquals("CS", package.CW_PackType);

			var packingLineBill = package.PackingGroup.Bill;
			AssertEquals("In TSW direct masters can be sent", "MASTERBILL1", packingLineBill.CU_BillNum);
			AssertEquals(BillTypeList.Codes.MasterBill, packingLineBill.CU_BillType);
		}

		public void TestSupportJE_PaymentMethodUsage()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.SupportJE_PaymentMethodUsage);
		}

		public void TestTemplateCopiedDeclarationDoesntCopyMAFStatuses()
		{
			IHaveNZAddInfo declaration = Factory.New<JobDeclaration>();
			declaration.AddInfo.ZN_MAF_MessagingStatus = "XXX";
			declaration.AddInfo.ZN_MAF_ConsignmentNumber = "YYY";
			declaration.AddInfo.ZN_MAF_ReceiptNumber = "ZZZ";

			IHaveNZAddInfo copiedDec = ((JobDeclaration)declaration).TemplateCopy();

			AssertEquals("copiedDec.MAFMessaging.ZX_MessagingStatus", ZString.Empty, copiedDec.AddInfo.ZN_MAF_MessagingStatus);
			AssertEquals("copiedDec.MAFMessaging.ZX_ConsignmentNumber", ZString.Empty, copiedDec.AddInfo.ZN_MAF_ConsignmentNumber);
			AssertEquals("copiedDec.MAFMessaging.ZX_ReceiptNumber", ZString.Empty, copiedDec.AddInfo.ZN_MAF_ReceiptNumber);
		}

		public void TestPDOOtherInfoDefaultValue()
		{
			CombineAssertions("Legacy Application Code", () =>
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				Assert(!declaration.JE_PDOOtherInfoValue);
				AssertEquals(0, declaration.OtherInfos.Count);

				declaration.OtherInfos.RemoveAndDeleteAll();
				Assert(!declaration.JE_PDOOtherInfoValue);
				AssertEquals(0, declaration.OtherInfos.Count);

				JobDeclaration copiedDec = declaration.TemplateCopy();
				Assert(copiedDec.JE_PDOOtherInfoValue);
				AssertEquals(1, copiedDec.OtherInfos.Count);
			});

			CombineAssertions("TSW Application Code- Normal", () =>
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				Assert(declaration.JE_PDOOtherInfoValue);
				AssertEquals(1, declaration.OtherInfos.Count);
				Assert(declaration.OtherInfos.AggregatedCodes.Contains(HeaderOtherInfoList.Codes.PrintDeliveryOrder));

				JobDeclaration copiedDec = declaration.TemplateCopy();
				Assert(copiedDec.JE_PDOOtherInfoValue);
				AssertEquals(1, declaration.OtherInfos.Count);
				Assert(declaration.OtherInfos.AggregatedCodes.Contains(HeaderOtherInfoList.Codes.PrintDeliveryOrder));

				declaration.OtherInfos.RemoveAndDeleteAll();
				Assert(!declaration.JE_PDOOtherInfoValue);
				AssertEquals(0, declaration.OtherInfos.Count);
			});

			CombineAssertions("TSW Application Code-WriteOff", () =>
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				AssertEquals(JobApplicationCodeList.Codes.CUS, declaration.JE_ApplicationCode);
				Assert(!declaration.JE_PDOOtherInfoValue);
				AssertEquals(0, declaration.OtherInfos.Count);

				JobDeclaration copiedDec = declaration.TemplateCopy();
				Assert(copiedDec.JE_PDOOtherInfoValue);
				AssertEquals(JobApplicationCodeList.Codes.TSW, copiedDec.JE_ApplicationCode);
				AssertEquals(JobMessageSubTypeList.Codes.WriteOff, copiedDec.JE_MessageSubType);
				AssertEquals(0, declaration.OtherInfos.Count);
			});
		}

		public void TestICusAddInfoTypeSupporter()
		{
			ICusAddInfoTypeSupporter supporter = Factory.New<JobDeclaration>();
			supporter.AssertType(typeof(CusAddInfo<MAFFile>), CusAddInfoTypeAttribute.Codes.NZMAFFiles);
			supporter.AssertType(null, "ZZ!");
		}

		public void TestGetDocManagerInfo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("No Shipment", declaration.DocManagerInfo, declaration.GetDocManagerInfo());
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals("With Shipment", shipment.DocManagerInfo, declaration.GetDocManagerInfo());
		}

		public void TestIncoTerm()
		{
			Declaration.JE_ShipmentIncoTerm = "XXX";
			AssertEquals("XXX", Declaration.JE_ShipmentIncoTerm);
			AssertEquals("Now does apply to NZ - WI00045362", "XXX", Declaration.IncoTerm);
		}

		public void TestSettingPERIODICVARIOUSVesselAddsPeriodicDrawbackOtherInfo()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("Precondition: Declaration.OtherInfos.Count", 0, Declaration.OtherInfos.Count);
			Declaration.JE_VesselName = "PERIODIC VARIOUS";
			AssertEquals("Declaration.OtherInfos.Count", 1, Declaration.OtherInfos.Count);
			AssertEquals("Declaration.OtherInfos[0].ZO_Code", HeaderOtherInfoList.Codes.PeriodicDrawbackEntry, Declaration.OtherInfos[0].ZO_Code);
			AssertEquals("Declaration.OtherInfos[0].ZO_Data", "", Declaration.OtherInfos[0].ZO_Data);
		}

		public void TestSettingPD0006FlightNoAddsPeriodicDrawbackOtherInfo()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("Precondition: Declaration.OtherInfos.Count", 0, Declaration.OtherInfos.Count);
			Declaration.JE_VoyageFlightNo = "PD0006";
			AssertEquals("Declaration.OtherInfos.Count", 1, Declaration.OtherInfos.Count);
			AssertEquals("Declaration.OtherInfos[0].ZO_Code", HeaderOtherInfoList.Codes.PeriodicDrawbackEntry, Declaration.OtherInfos[0].ZO_Code);
			AssertEquals("Declaration.OtherInfos[0].ZO_Data", "", Declaration.OtherInfos[0].ZO_Data);
		}

		public void TestIsPeriodicDrawback()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry, "");
			AssertEquals("Declaration.IsPeriodicDrawback", true, Declaration.IsPeriodicDrawback);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertEquals("Declaration.IsPeriodicDrawback", false, Declaration.IsPeriodicDrawback);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			AssertEquals("Declaration.IsPeriodicDrawback", true, Declaration.IsPeriodicDrawback);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Declaration.IsPeriodicDrawback", false, Declaration.IsPeriodicDrawback);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			AssertEquals("Declaration.IsPeriodicDrawback", true, Declaration.IsPeriodicDrawback);

			Declaration.OtherInfos[0].ZO_Code = "ZAZ";
			AssertEquals("Declaration.IsPeriodicDrawback", true, Declaration.IsPeriodicDrawback);
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PeriodicDrawbackEntry, "");
			AssertEquals("Declaration.IsPeriodicDrawback", true, Declaration.IsPeriodicDrawback);
		}

		public void TestMISCNamesGetBlankedOutOnSaveIfMISCNoLongerSelected()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Declaration.MiscSupplierName = "FRED'S SPARE NUTS";
			Declaration.MiscImporterName = "JOHN'S SPARE BOLTS";
			AssertEquals("FRED'S SPARE NUTS", Declaration.MiscSupplierName);
			AssertEquals("JOHN'S SPARE BOLTS", Declaration.MiscImporterName);

			Factory.Save();

			AssertEquals("", Declaration.MiscSupplierName);
			AssertEquals("", Declaration.MiscImporterName);

			Declaration.JE_OH_Supplier = Declaration.CachedMiscOrgPK;
			Declaration.MiscSupplierName = "FRED'S SPARE NUTS";
			Declaration.JE_OH_Importer = Declaration.CachedMiscOrgPK;
			Declaration.MiscImporterName = "JOHN'S SPARE BOLTS";

			Factory.Save();

			AssertEquals("FRED'S SPARE NUTS", Declaration.MiscSupplierName);
			AssertEquals("JOHN'S SPARE BOLTS", Declaration.MiscImporterName);
		}

		public void TestMiscFieldsAccessors()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZGuid otherOrgPK = Factory.New<OrgHeader>().PK;
			ZGuid miscOrgPK = declaration.CachedMiscOrgPK;
			AssertEquals("", declaration.JE_SupplierMiscFields);
			AssertEquals("", declaration.JE_ImporterMiscFields);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", declaration.JE_SupplierMiscFields);
			AssertEquals("", declaration.JE_ImporterMiscFields);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertEquals("", declaration.JE_SupplierMiscFields);
			AssertEquals("", declaration.JE_ImporterMiscFields);

			declaration.JE_OH_Supplier = miscOrgPK;
			AssertEquals(JobDeclaration.Schema.MiscSupplierName, declaration.JE_SupplierMiscFields);
			AssertEquals("", declaration.JE_ImporterMiscFields);
			declaration.JE_OH_Importer = miscOrgPK;
			AssertEquals(JobDeclaration.Schema.MiscSupplierName, declaration.JE_SupplierMiscFields);
			AssertEquals(JobDeclaration.Schema.MiscImporterName, declaration.JE_ImporterMiscFields);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("", declaration.JE_SupplierMiscFields);
			AssertEquals("", declaration.JE_ImporterMiscFields);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertEquals(JobDeclaration.Schema.MiscSupplierName, declaration.JE_SupplierMiscFields);
			AssertEquals(JobDeclaration.Schema.MiscImporterName, declaration.JE_ImporterMiscFields);
			declaration.JE_OH_Supplier = otherOrgPK;
			AssertEquals("", declaration.JE_SupplierMiscFields);
			AssertEquals(JobDeclaration.Schema.MiscImporterName, declaration.JE_ImporterMiscFields);
			declaration.JE_OH_Importer = otherOrgPK;
			AssertEquals("", declaration.JE_SupplierMiscFields);
			AssertEquals("", declaration.JE_ImporterMiscFields);
		}

		public void TestDefaultMessageTypeFromSupplierOrImporter()
		{
			OrgHeader localOrg = Factory.New<OrgHeader>();
			localOrg.OH_RL_NKClosestPort = "NZAKL";
			OrgHeader osOrg = Factory.New<OrgHeader>();
			osOrg.OH_RL_NKClosestPort = "USLAX";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZGuid miscOrgPK = declaration.CachedMiscOrgPK;

			declaration.JE_OH_Supplier = osOrg.PK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, declaration.JE_MessageSubType);

			declaration.JE_OH_Supplier = localOrg.PK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals("declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, declaration.JE_MessageSubType);

			declaration.JE_OH_Supplier = ZGuid.Empty;

			declaration.JE_OH_Importer = localOrg.PK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, declaration.JE_MessageSubType);

			declaration.JE_OH_Importer = osOrg.PK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals("declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, declaration.JE_MessageSubType);

			declaration.JE_OH_Importer = ZGuid.Empty;

			declaration.JE_OH_Supplier = miscOrgPK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Simplified, declaration.JE_MessageSubType);

			declaration.JE_OH_Supplier = localOrg.PK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals("declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, declaration.JE_MessageSubType);

			declaration.JE_OH_Importer = miscOrgPK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Simplified, declaration.JE_MessageSubType);

			declaration.JE_OH_Importer = osOrg.PK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals("declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, declaration.JE_MessageSubType);

			declaration.ReturnInvalidMiscOrg = true;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Supplier = osOrg.PK;
			AssertEquals("declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, declaration.JE_MessageSubType);
		}

		public void TestMiscFieldsAreForcedToUpper()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZGuid miscOrgPK = declaration.CachedMiscOrgPK;

			declaration.JE_OH_Supplier = miscOrgPK;
			declaration.MiscSupplierName = "MISC Supplier";

			AssertEquals(miscOrgPK, declaration.JE_OH_Supplier);
			AssertEquals("MISC SUPPLIER", declaration.MiscSupplierName);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			Factory.Save();
			AssertEquals("", declaration.MiscSupplierName);

			declaration.JE_OH_Importer = miscOrgPK;
			declaration.MiscImporterName = "MISC Importer";

			AssertEquals(miscOrgPK, declaration.JE_OH_Importer);
			AssertEquals("MISC IMPORTER", declaration.MiscImporterName);

			declaration.JE_OH_Importer = ZGuid.Empty;
			Factory.Save();
			AssertEquals("", declaration.MiscImporterName);
		}

		public void TestILandedCostHeaderTotalDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(500m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);

			declaration.ResetToOriginal();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Assert("CustomsEntryHeaders.Count > ActiveEntryHeaders.Count", declaration.CustomsEntryHeaders.Count > declaration.ActiveEntryHeaders.Count);

			ILandedCostHeader lcHeader = declaration;
			var total = lcHeader.TotalDutyTaxEntryFeeItems;
			AssertEquals("Total Duty", 500m, total["TDT"]);
		}

		public override void TestSwitchFromSeaToAirWithContainersDereferencesAndDeletesContainers()
		{
			var jobDec = GetJobDeclaration();
			jobDec.FillWithValidTestData();
			jobDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var container = jobDec.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123";

			var houseBill = jobDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HB123";

			var pack = (jobDec.PackingGroups.Count > 0) ? jobDec.PackingGroups[0] : jobDec.PackingGroups.AddNew();
			pack.CR_CO_Container = container.PK;
			pack.CR_CU_HouseBill = houseBill.PK;

			Factory.Save();

			jobDec.JE_TransportMode = jobDec.TransportModeAirCodeForTesting;

			Factory.Save();

			if (jobDec.ContainersRequired)
			{
				AssertEquals("Containers were deleted or rereferenced on change to Air", 1, jobDec.CusContainers.Count);
			}
			else
			{
				AssertEquals("Containers were not deleted or rereferenced on change to Air", ZGuid.Empty, pack.CR_CO_Container);
				AssertEquals("Containers were not deleted or rereferenced on change to Air", 0, jobDec.CusContainers.Count);
			}
		}

		public void TestApplicationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Factory.Save();
			AssertEquals("JE_ApplicationCode", JobApplicationCodeList.Codes.TSW, declaration.JE_ApplicationCode);
		}

		public void TestApplicationCodeWhenCopyDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			var newDeclaration = declaration.TemplateCopy();
			AssertEquals("Application code should be reset to TSW.", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;

			newDeclaration = declaration.TemplateCopy();
			AssertEquals("Application code should be reset to TSW.", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			newDeclaration = declaration.TemplateCopy();
			AssertEquals("Application code should be reset to TSW.", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			newDeclaration = declaration.TemplateCopy();
			AssertEquals("Application code should be reset to TSW.", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);
		}

		public void TestPDOOtherInfoWhenCopyDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			var newDeclaration = declaration.TemplateCopy();
			AssertEquals("Application code should be reset to TSW.", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);
			Assert("PDO Should Exist", newDeclaration.JE_PDOOtherInfoValue);
			AssertEquals("PDO Should Exist 1th Only", 1, newDeclaration.OtherInfos.Count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;

			newDeclaration = declaration.TemplateCopy();
			AssertEquals("Application code should be reset to TSW.", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);
			Assert("PDO Should Exist", newDeclaration.JE_PDOOtherInfoValue);
			AssertEquals("PDO Should Exist 1th Only", 1, newDeclaration.OtherInfos.Count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			newDeclaration = declaration.TemplateCopy();
			AssertEquals("Application code should be reset to TSW.", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);
			Assert("PDO Should Exist", newDeclaration.JE_PDOOtherInfoValue);
			AssertEquals("PDO Should Exist 1th Only", 1, newDeclaration.OtherInfos.Count);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			newDeclaration = declaration.TemplateCopy();
			AssertEquals("Application code should not be reset to CUS.", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);
			Assert("PDO Should Exist", newDeclaration.JE_PDOOtherInfoValue);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			newDeclaration = declaration.TemplateCopy();
			AssertEquals("Application code should be reset to TSW.", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);
			Assert("PDO Should Exist", newDeclaration.JE_PDOOtherInfoValue);
		}

		internal static string GetEntryTotalAmountDoesNotMatchReturnedOneMessage(ZDecimal total, ZDecimal totalReturned)
		{
			const string format = "C";
			return string.Format(@"The total figure calculated for the current entry does not match the last figure returned from Customs.

Calculated Total: {0}
Total from Customs: {1}

This means that the values on this job are not in sync with the values currently lodged with Customs.
This can be caused by Customs making an 'Off the Page' modification where they make manual adjustments
to a Declaration at their end.

If this happens the System has no way of knowing what changes have been made and must be manually
updated with the same changes that Customs made before the right figures will be shown.", total.ToString(format), totalReturned.ToString(format));
		}

		#region Common Tests
		public void TestOverseasInsurance()
		{
			NZCustomsDataRegistry.Instance.DefaultOverseasInsurance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.75m);

			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader importer = Factory.New<OrgHeader>();

			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew(importer);

			link.OL_InsuranceUplift = 0;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("Precondition: Charges.Count", 0, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Charges.Count", 1, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);
			BaseGroupInvoiceCharge charge = declaration.JobComInvoiceGroupHeaders[0].Charges[0];
			AssertEquals("charge.J7_ChargeType", CustomsChargeTypeList.Codes.OverseasInsurance, charge.J7_ChargeType);
			AssertEquals("charge.J7_Percentage", 1.75m, charge.J7_Percentage);
		}

		public void TestOverseasInsuranceDefaultsWhenSupplierChangedWithLink()
		{
			NZCustomsDataRegistry.Instance.DefaultOverseasInsurance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.75m);

			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader importer = Factory.New<OrgHeader>();

			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew(importer);

			link.OL_InsuranceUplift = 2.25m;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("Precondition: Charges.Count", 0, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Charges.Count", 1, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);
			BaseGroupInvoiceCharge charge = declaration.JobComInvoiceGroupHeaders[0].Charges[0];
			AssertEquals("charge.J7_ChargeType", CustomsChargeTypeList.Codes.OverseasInsurance, charge.J7_ChargeType);
			AssertEquals("charge.J7_Percentage", 2.25m, charge.J7_Percentage);
		}

		public void TestOverseasInsuranceDefaultsWhenSupplierChangedNoLink()
		{
			NZCustomsDataRegistry.Instance.DefaultOverseasInsurance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1.75m);

			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader importer = Factory.New<OrgHeader>();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("Precondition: Charges.Count", 0, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Charges.Count", 1, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);
			BaseGroupInvoiceCharge charge = declaration.JobComInvoiceGroupHeaders[0].Charges[0];
			AssertEquals("charge.J7_ChargeType", CustomsChargeTypeList.Codes.OverseasInsurance, charge.J7_ChargeType);
			AssertEquals("charge.J7_Percentage", 1.75m, charge.J7_Percentage);
		}

		public void TestOverseasInsuranceDoesntDefaultsUnlessSetupInRegistry()
		{
			AssertEquals("Precondition: NZCustomsDataRegistry.Instance.DefaultInsuranceUplift", 0.00m, NZCustomsDataRegistry.Instance.DefaultOverseasInsurance.Value);

			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader importer = Factory.New<OrgHeader>();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("Precondition: Charges.Count", 0, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Charges.Count", 0, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);
		}

		public void TestZeroRatedAllValidationSwitchesOffWhenSettingTheDeclarationBackToExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoiceHeader.JZ_IsZeroRatedDuty = "F";
			invoiceHeader.JZ_IsZeroRatedExcise = "A";
			invoiceHeader.JZ_IsZeroRatedGST = "R";
			invoiceHeader.JZ_IsZeroRatedLevies = "K";

			AssertHasMessageErrors(invoiceHeader.JZ_IsZeroRatedDutyInfo);
			AssertHasMessageErrors(invoiceHeader.JZ_IsZeroRatedExciseInfo);
			AssertHasMessageErrors(invoiceHeader.JZ_IsZeroRatedGSTInfo);
			AssertHasMessageErrors(invoiceHeader.JZ_IsZeroRatedLeviesInfo);

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_IsZeroRatedDuty = "P";
			invoiceLine.JI_IsZeroRatedExcise = "U";
			invoiceLine.JI_IsZeroRatedGST = "D";
			invoiceLine.JI_IsZeroRatedLevies = "W";

			AssertEquals("Precondition: FilteredInvoiceLines.Count", 1, declaration.FilteredInvoiceLines.Count);

			AssertHasMessageErrors(invoiceLine.JI_IsZeroRatedDutyInfo);
			AssertHasMessageErrors(invoiceLine.JI_IsZeroRatedExciseInfo);
			AssertHasMessageErrors(invoiceLine.JI_IsZeroRatedGSTInfo);
			AssertHasMessageErrors(invoiceLine.JI_IsZeroRatedLeviesInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedDutyInfo);
			AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedExciseInfo);
			AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedGSTInfo);
			AssertNoMessageErrors(invoiceHeader.JZ_IsZeroRatedLeviesInfo);
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedDutyInfo);
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedExciseInfo);
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedGSTInfo);
			AssertNoMessageErrors(invoiceLine.JI_IsZeroRatedLeviesInfo);
		}

		public void TestIsInTestMode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals(true, declaration.IsExcise);

			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("IsInTestMode", true, declaration.IsInTestMode);

			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("IsInTestMode", false, declaration.IsInTestMode);
		}

		public void TestTSWIsInTestMode()
		{
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("TSW Dec IsInTestMode", false, declaration.IsInTestMode);

			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("TSW Dec IsInTestMode", true, declaration.IsInTestMode);

			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("TSW Dec IsInTestMode", false, declaration.IsInTestMode);

			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("TSW Dec IsInTestMode", true, declaration.IsInTestMode);

			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("TSW Dec IsInTestMode", false, declaration.IsInTestMode);

			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("TSW Dec IsInTestMode", true, declaration.IsInTestMode);

			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("TSW Dec IsInTestMode", false, declaration.IsInTestMode);

			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("TSW Dec IsInTestMode", true, declaration.IsInTestMode);

			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("TSW Dec IsInTestMode", false, declaration.IsInTestMode);
		}

		public void TestIsTSWDeclaration()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWDeclaration", true, Declaration.IsTSWDeclaration);
			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals("IsTSWDeclaration", true, Declaration.IsTSWDeclaration);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("IsTSWDeclaration", false, Declaration.IsTSWDeclaration);
		}

		public void TestIsTSW_IPI_Declaration()
		{
			AssertEquals("IsTSW_IPI_Declaration", false, Declaration.IsTSW_IPI_Declaration);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSW_IPI_Declaration", false, Declaration.IsTSW_IPI_Declaration);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("IsTSW_IPI_Declaration", false, Declaration.IsTSW_IPI_Declaration);
			Declaration.JE_MessageSubType = MessageTypeList.Codes.IPI;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSW_IPI_Declaration", true, Declaration.IsTSW_IPI_Declaration);
		}

		public void TestIsTSWExportDeclaration()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWExportDeclaration", true, Declaration.IsTSWExportDeclaration);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("IsTSWExportDeclaration", false, Declaration.IsTSWExportDeclaration);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsTSWExportDeclaration", false, Declaration.IsTSWExportDeclaration);
		}

		public void TestIsTSWImportDeclaration()
		{
			AssertEquals("IsTSWImportDeclaration", false, Declaration.IsTSWImportDeclaration);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWImportDeclaration", false, Declaration.IsTSWImportDeclaration);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWImportDeclaration", true, Declaration.IsTSWImportDeclaration);
		}

		public void TestVoyageFlightVariousDoesNotUpdateRouting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_VoyageFlightNo = "VARIOUS";
			AssertEquals("There should be no routing legs generated for flight 'VARIOUS'", 0, declaration.Transports.Count);

			declaration.JE_VoyageFlightNo = "QF1";
			AssertEquals("There should now be 1 routing legs generated for this flight no.", 1, declaration.Transports.Count);
		}

		public void TestMAWBPrefixIsNotDefaultedForVarious()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_TransportMode = "AIR";
			AssertEquals("Pre-condition", "", declaration.JE_MasterBill);

			declaration.JE_VoyageFlightNo = "VARIOUS";
			AssertEquals("Special flight number should not default a Mawb prefix for Airline code", "", declaration.JE_MasterBill);

			declaration.JE_VoyageFlightNo = "QF1";
			AssertEquals("Mawb prefix should default", "081", declaration.JE_MasterBill);

			declaration.JE_VoyageFlightNo = "VARIOUS";
			AssertEquals("Special flight term should clear out Mawb value", "", declaration.JE_MasterBill);
		}

		public void TestJE_VoyageFlightNoCaption()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Road;
			var info = dec.JE_VoyageFlightNoInfo;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Road", "Registration", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Registration", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Sea", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air", "Flight Number", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Flight Number", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Rail", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
		}

		public override void TestHouseBillLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			var databoundBO = new DataBoundBusinessObject(declaration);
			declaration.JE_TransportMode = declaration.TransportModeMailCodeForTesting;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_HouseBillInfo, databoundBO);
			AssertEquals("Postal Label", "Parcel No", resourceStringData.Caption);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_HouseBillInfo, databoundBO);
			AssertEquals("Default Label", "House Bill", resourceStringData.Caption);
		}

		public void TestBarrierPort()
		{
			Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			Declaration.JE_RL_NKPortOfArrival = "GBLON";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("GBLON", Declaration.BarrierPort);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("AUSYD", Declaration.BarrierPort);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals("", Declaration.BarrierPort);

			Declaration.JE_MessageType = "";
			AssertEquals("", Declaration.BarrierPort);
		}

		public void TestCopiedECIManifestDeclarationHasNoEntryHeader()
		{
			ECIWriteOff.Manifesting.CusEntryHeader manifestEntryHeader = Factory.New<ECIWriteOff.Manifesting.CusEntryHeader>();
			ECIWriteOff.Manifesting.Testing.TestManifestCreator manifestCreator = new ECIWriteOff.Manifesting.Testing.TestManifestCreator(manifestEntryHeader, "081-11111111", "QF253", "USLAX", "NZAKL", new ZDateTime(2005, 11, 30), new ZDateTime(2005, 12, 1));
			JobDeclaration declarationToBeCopied = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			Factory.Save();
			ZString declarationReferenceAfterSavingBeforeCopy = declarationToBeCopied.JE_DeclarationReference;

			AssertEquals("Precondition: declarationToBeCopied.CusEntryHeader.GetType()", typeof(ECIWriteOff.Manifesting.CusEntryHeader), declarationToBeCopied.CusEntryHeader.GetType());
			AssertEquals("Precondition: declarationReferenceAfterSavingBeforeCopy.IsEmpty", false, declarationReferenceAfterSavingBeforeCopy.IsEmpty);

			JobDeclaration copiedDeclaration = declarationToBeCopied.TemplateCopy();

			AssertNotEquals("manifestEntryHeader and copiedDeclaration.CusEntryHeader", manifestEntryHeader, copiedDeclaration.CusEntryHeader);
			AssertEquals("copiedDeclaration.CusEntryHeader", typeof(ECIWriteOff.CusEntryHeader), copiedDeclaration.CusEntryHeader.GetType());
			AssertEquals("declarationToBeCopied.HasChanges", false, declarationToBeCopied.HasChanges);
			AssertEquals("declarationToBeCopied.JE_DeclarationReference", declarationReferenceAfterSavingBeforeCopy, declarationToBeCopied.JE_DeclarationReference);
		}

		public void TestECIValuesTriggerRefreshBinding()
		{
			decBindingList_ListChangedCount = 0;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			IBindingList decBindingList = declaration;
			decBindingList.ListChanged += new ListChangedEventHandler(decBindingList_ListChanged);
			AssertEquals("decBindingList_ListChangedCount", 0, decBindingList_ListChangedCount);
			declaration.JE_ECI_InvoiceAmount = 0.00m;
			AssertEquals("decBindingList_ListChangedCount", 1, decBindingList_ListChangedCount);
			declaration.JE_ECI_InvoiceCurrency = Guid.Empty;
			AssertEquals("decBindingList_ListChangedCount", 2, decBindingList_ListChangedCount);
		}

		public void TestContainsThisOtherInfoCode()
		{
			Declaration.JE_SEPOtherInfoValue = "54321";
			AssertNotNull("Precondition", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));
			Declaration.JE_ATFOtherInfoValue = "12345";
			AssertNotNull("Precondition", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility));
			Assert(!Declaration.ContainsThisOtherInfoCode(HeaderOtherInfoList.Codes.AwareOfContents));
			Assert(Declaration.ContainsThisOtherInfoCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility));
			Assert(Declaration.ContainsThisOtherInfoCode(HeaderOtherInfoList.Codes.SecureExportPartnership));
		}

		public void TestMessageTypeDefaultsCorrectlyFromSupplierAndImporterPortCodes()
		{
			OrgHeader localParty = OrgHeader.New(Factory);
			localParty.OH_FullName = "LocalParty";
			localParty.OH_RL_NKClosestPort = "NZAKL";

			OrgHeader osParty = OrgHeader.New(Factory);
			osParty.OH_FullName = "OSParty";
			osParty.OH_RL_NKClosestPort = "GBTIL";

			OrgHeader anotherlocalParty = OrgHeader.New(Factory);
			anotherlocalParty.OH_FullName = "anotherlocalParty";
			anotherlocalParty.OH_RL_NKClosestPort = "NZAKL";

			OrgHeader anotherOSParty = OrgHeader.New(Factory);
			anotherOSParty.OH_FullName = "AnotherOSParty";
			anotherOSParty.OH_RL_NKClosestPort = "FJSUV";

			CheckDeclarationGetsRightMessageTypeFrom(localParty, osParty, JobMessageTypeList.Codes.Import);
			CheckDeclarationGetsRightMessageTypeFrom(osParty, localParty, JobMessageTypeList.Codes.Export);
			CheckDeclarationGetsRightMessageTypeFrom(osParty, anotherOSParty, JobMessageTypeList.Codes.Export);
			CheckDeclarationGetsRightMessageTypeFrom(localParty, anotherlocalParty, JobMessageTypeList.Codes.Excise);
		}

		public override void TestTotalCustomsWeight()
		{
			ZWeight result = new ZWeight(0.0m, Core.Constants.Weight.Kilograms);
			AssertEquals(result, Declaration.TotalCustomsWeight);

			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_CustomsUnitQty = StatisticalUQList.Codes.Kilograms;
			line1.JI_CustomsQuantity = 100;
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_CustomsUnitQty = StatisticalUQList.Codes.NumberOfPairs;
			line2.JI_CustomsQuantity = 20;
			JobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_CustomsUnitQty = StatisticalUQList.Codes.Tonnes;
			line3.JI_CustomsQuantity = 2;

			result = new ZWeight(2100.0m, Core.Constants.Weight.Kilograms);
			AssertEquals(result, Declaration.TotalCustomsWeight);
		}

		public void TestECI_ApportionedContainerAndLoosePackageValues()
		{
			AssertEquals("Declaration.ContainerValues.GetType()", typeof(ContainerAndLoosePackagesValueApportioner), Declaration.ECI_ApportionedContainerAndLoosePackageValues.GetType());
		}

		public void TestSEPDefaultsFromExporter()
		{
			OrgHeader exporter1 = Factory.New<OrgHeader>();
			OrgCusCode cusCode1 = exporter1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.NZCodeTypes.SecureExportPartner;
			cusCode1.OK_RN_NKCodeCountry = "NZ";
			cusCode1.OK_CustomsRegNo = "88754";
			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;

			OrgHeader exporter2 = Factory.New<OrgHeader>();

			OrgHeader exporter3 = Factory.New<OrgHeader>();
			OrgCusCode cusCode3 = exporter3.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = OrgCusCode.NZCodeTypes.SecureExportPartner;
			cusCode3.OK_RN_NKCodeCountry = "NZ";
			cusCode3.OK_CustomsRegNo = "11223";
			cusCode3.OK_OA_PremisesAddress = ZGuid.Empty;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;

			AssertEquals("Precondition: Declaration.JE_SEPOtherInfoValue", "", Declaration.JE_SEPOtherInfoValue);
			Declaration.JE_OH_Supplier = exporter1.PK;
			AssertEquals("Declaration.JE_SEPOtherInfoValue", "88754", Declaration.JE_SEPOtherInfoValue);
			Declaration.JE_OH_Supplier = exporter2.PK;
			AssertEquals("Declaration.JE_SEPOtherInfoValue", "88754", Declaration.JE_SEPOtherInfoValue);
			Declaration.JE_OH_Supplier = exporter3.PK;
			AssertEquals("Declaration.JE_SEPOtherInfoValue", "11223", Declaration.JE_SEPOtherInfoValue);
		}

		public void TestATFDefaults()
		{
			OrgHeader importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "IMP1";
			OrgCusCode cusCode1 = importer1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			cusCode1.OK_RN_NKCodeCountry = "NZ";
			cusCode1.OK_CustomsRegNo = "CODE1";
			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;

			OrgHeader importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "IMP2";

			OrgHeader importer3 = Factory.NewWithValidTestData<OrgHeader>();
			importer3.OH_Code = "IMP3";
			OrgAddress addressPickup = importer3.Addresses.AddNew();
			addressPickup.Address1 = "Pickup Addr1";
			addressPickup.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			OrgAddress addressDelivery = importer3.Addresses.AddNew();
			addressDelivery.Address1 = "Delivery Addr1";
			addressDelivery.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			OrgCusCode cusCode3 = importer3.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			cusCode3.OK_RN_NKCodeCountry = "NZ";
			cusCode3.OK_CustomsRegNo = "CODE3";
			cusCode3.OK_OA_PremisesAddress = ZGuid.Empty;
			OrgCusCode cusCodePickup = importer3.CustomsCodes.AddNew();
			cusCodePickup.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			cusCodePickup.OK_RN_NKCodeCountry = "NZ";
			cusCodePickup.OK_CustomsRegNo = "PICKUP";
			cusCodePickup.OK_OA_PremisesAddress = addressPickup.PK;
			OrgCusCode cusCodeDelivery = importer3.CustomsCodes.AddNew();
			cusCodeDelivery.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			cusCodeDelivery.OK_RN_NKCodeCountry = "NZ";
			cusCodeDelivery.OK_CustomsRegNo = "DELIVERY";
			cusCodeDelivery.OK_OA_PremisesAddress = addressDelivery.PK;

			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "DEPOT1";
			OrgAddress depotAddress = depot.Addresses.AddNew();
			depotAddress.Address1 = "Depot Addr1";
			depotAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			OrgCusCode cusCode4 = depot.CustomsCodes.AddNew();
			cusCode4.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			cusCode4.OK_RN_NKCodeCountry = "NZ";
			cusCode4.OK_CustomsRegNo = "CODE4";
			cusCode4.OK_OA_PremisesAddress = ZGuid.Empty;
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;

			AssertEquals("Precondition: Declaration.JE_ATFOtherInfoValue", "", Declaration.JE_ATFOtherInfoValue);
			Declaration.JE_OH_Importer = importer1.PK;
			AssertEquals("Declaration.JE_ATFOtherInfoValue", "CODE1", Declaration.JE_ATFOtherInfoValue);
			Declaration.JE_OH_Importer = importer2.PK;
			AssertEquals("Declaration.JE_ATFOtherInfoValue", "CODE1", Declaration.JE_ATFOtherInfoValue);
			Declaration.JE_OH_Importer = importer3.PK;
			AssertEquals("Declaration.JE_ATFOtherInfoValue", "DELIVERY", Declaration.JE_ATFOtherInfoValue);
			Declaration.ImporterDeliveryAddress.E2_OA_Address = addressPickup.PK;
			AssertEquals("Declaration.JE_ATFOtherInfoValue", "PICKUP", Declaration.JE_ATFOtherInfoValue);
			//Declaration.DepotDocAddress.OrganisationPK = Depot.PK;
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			AssertEquals("Declaration.JE_ATFOtherInfoValue", "CODE4", Declaration.JE_ATFOtherInfoValue);
		}

		public void TestJE_SEPOtherInfoValue()
		{
			AssertEquals("Precondition: JE_SEPOtherInfoValue", "", Declaration.JE_SEPOtherInfoValue);
			AssertEquals("Precondition: GetOtherInfo(SecureExportPartnership)", null, Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));

			Declaration.JE_SEPOtherInfoValue = "";
			AssertEquals("JE_SEPOtherInfoValue", "", Declaration.JE_SEPOtherInfoValue);
			AssertNull("GetOtherInfo(SecureExportPartnership)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));

			Declaration.JE_SEPOtherInfoValue = "12345";
			AssertEquals("JE_SEPOtherInfoValue", "12345", Declaration.JE_SEPOtherInfoValue);
			AssertNotNull("GetOtherInfo(SecureExportPartnership)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));

			Declaration.JE_SEPOtherInfoValue = "54321";
			AssertEquals("JE_SEPOtherInfoValue", "54321", Declaration.JE_SEPOtherInfoValue);
			AssertNotNull("GetOtherInfo(SecureExportPartnership)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));

			Declaration.JE_SEPOtherInfoValue = "";
			AssertEquals("JE_SEPOtherInfoValue", "", Declaration.JE_SEPOtherInfoValue);
			AssertNull("GetOtherInfo(SecureExportPartnership)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));
		}

		public void TestSeaJobSEPValueSetByDepotOrg()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("Precondition: JE_SEPOtherInfoValue", "", Declaration.JE_SEPOtherInfoValue);
			AssertEquals("Precondition: GetOtherInfo(SecureExportPartnership)", null, Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));

			var depot = Factory.NewWithValidTestData<OrgHeader>();
			var depotAddress1 = depot.Addresses.AddNew();
			depotAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			var depotSEPCode = depot.CustomsCodes.AddNew();
			depotSEPCode.OK_CodeType = OrgCusCode.NZCodeTypes.SecureExportPartner;
			depotSEPCode.OK_RN_NKCodeCountry = "NZ";
			depotSEPCode.OK_CustomsRegNo = "SEP1234";

			Declaration.DepotDocAddress.OrganisationPK = depot.PK;
			AssertEquals("JE_SEPOtherInfoValue should have been populated", "SEP1234", Declaration.JE_SEPOtherInfoValue);
			AssertNotNull("GetOtherInfo(SecureExportPartnership)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));

			var depotAddress2 = depot.Addresses.AddNew();
			depotAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			Declaration.JE_SEPOtherInfoValue = "OVERRIDE";
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress2.PK;
			AssertEquals("Overriden SEP OtherInfoValue should not be re-overriden from Depot Organization when changing Depot address", "OVERRIDE", Declaration.JE_SEPOtherInfoValue);

			Declaration.DepotDocAddress.OrganisationPK = ZGuid.Empty;
			Declaration.DepotDocAddress.OrganisationPK = depot.PK;
			AssertEquals("JE_SEPOtherInfoValue should re-populate from same organization if field is cleared out & same code used again.", "SEP1234", Declaration.JE_SEPOtherInfoValue);
			AssertNotNull("GetOtherInfo(SecureExportPartnership)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));
		}

		public void TestJE_ATFOtherInfoValue()
		{
			AssertEquals("Precondition: JE_ATFOtherInfoValue", "", Declaration.JE_ATFOtherInfoValue);
			AssertEquals("Precondition: GetOtherInfo(ApprovedTransitionalFacility)", null, Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility));

			Declaration.JE_ATFOtherInfoValue = "";
			AssertEquals("JE_ATFOtherInfoValue", "", Declaration.JE_ATFOtherInfoValue);
			AssertNull("GetOtherInfo(ApprovedTransitionalFacility)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility));

			Declaration.JE_ATFOtherInfoValue = "12345";
			AssertEquals("JE_ATFOtherInfoValue", "12345", Declaration.JE_ATFOtherInfoValue);
			AssertNotNull("GetOtherInfo(ApprovedTransitionalFacility)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility));

			Declaration.JE_ATFOtherInfoValue = "54321";
			AssertEquals("JE_ATFOtherInfoValue", "54321", Declaration.JE_ATFOtherInfoValue);
			AssertNotNull("GetOtherInfo(ApprovedTransitionalFacility)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility));

			Declaration.JE_ATFOtherInfoValue = "";
			AssertEquals("JE_ATFOtherInfoValue", "", Declaration.JE_ATFOtherInfoValue);
			AssertNull("GetOtherInfo(ApprovedTransitionalFacility)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility));
		}

		public void TestJE_PDOOtherInfoValue()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("Precondition: JE_PDOOtherInfoValue", false, Declaration.JE_PDOOtherInfoValue);
			AssertEquals("Precondition: GetOtherInfo(PrintDeliveryOrder)", null, Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.PrintDeliveryOrder));

			Declaration.JE_PDOOtherInfoValue = false;
			AssertEquals("JE_PDOOtherInfoValue", false, Declaration.JE_PDOOtherInfoValue);
			AssertNull("GetOtherInfo(PrintDeliveryOrder)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.PrintDeliveryOrder));

			Declaration.JE_PDOOtherInfoValue = true;
			AssertEquals("JE_PDOOtherInfoValue", true, Declaration.JE_PDOOtherInfoValue);
			AssertNotNull("GetOtherInfo(PrintDeliveryOrder)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.PrintDeliveryOrder));

			Declaration.JE_PDOOtherInfoValue = true;
			AssertEquals("JE_PDOOtherInfoValue", true, Declaration.JE_PDOOtherInfoValue);
			AssertNotNull("GetOtherInfo(PrintDeliveryOrder)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.PrintDeliveryOrder));

			Declaration.JE_PDOOtherInfoValue = false;
			AssertEquals("JE_PDOOtherInfoValue", false, Declaration.JE_PDOOtherInfoValue);
			AssertNull("GetOtherInfo(PrintDeliveryOrder)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.PrintDeliveryOrder));
		}

		public void TestHasNotBeenSentToCustoms()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
			Assert(Declaration.HasNotBeenSentToCustoms);

			Declaration.JE_EntryStatus = "foo";
			Assert(!Declaration.HasNotBeenSentToCustoms);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Assert(Declaration.HasNotBeenSentToCustoms);

			Declaration.JE_EntryStatus = "foo";
			Assert(!Declaration.HasNotBeenSentToCustoms);

			Declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			Assert("Consolidated status (ATC) is equivalent of Not Sent to Customs", Declaration.HasNotBeenSentToCustoms);

			Declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			Assert("Consolidated status (RFC) is equivalent of Not Sent to Customs", Declaration.HasNotBeenSentToCustoms);

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Assert(!Declaration.HasNotBeenSentToCustoms);
		}

		public void TestEntryStatusDescShowsConsolidationStatusPreSendingIfRelevant()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_MessageStatus = ZString.Empty;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Assert(Declaration.HasNotBeenSentToCustoms);
			AssertEquals("Not Sent to Customs", Declaration.JE_EntryStatusDescription);

			Declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			AssertEquals("Entry Status Description should show the Consolidation preparation status, (Ready for Consolidation), for Consolidation jobs prior to sending", ConsolidatedEntryStatusList.Descriptions.ReadyForConsolidation, Declaration.JE_EntryStatusDescription);

			Declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			AssertEquals("Entry Status Description should show the Consolidation preparation status, (Applied to Consolidation), for Consolidation jobs prior to sending", ConsolidatedEntryStatusList.Descriptions.AppliedToConsolidation, Declaration.JE_EntryStatusDescription);

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			AssertEquals("Sent to Customs", Declaration.JE_EntryStatusDescription);
		}

		public void TestEntryNumberForCompletionEntryFromSight()
		{
			ZString sightNumber = "SIGHT123";
			ZString completionNumber = "COMPLETE";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			Declaration.DeclarationNumber = sightNumber;
			AssertEquals("Declaration.DeclarationNumber", sightNumber, Declaration.DeclarationNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumber", ZString.Empty, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumberInfo.ReadOnly", true, Declaration.JE_OriginalEntryNumberInfo.ReadOnly);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertEquals("Declaration.DeclarationNumber", ZString.Empty, Declaration.DeclarationNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumber", sightNumber, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumberInfo.ReadOnly", true, Declaration.JE_OriginalEntryNumberInfo.ReadOnly);

			Declaration.DeclarationNumber = completionNumber;
			AssertEquals("Declaration.DeclarationNumber", completionNumber, Declaration.DeclarationNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumber", sightNumber, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumberInfo.ReadOnly", true, Declaration.JE_OriginalEntryNumberInfo.ReadOnly);
		}

		public void TestOriginalEntryType()
		{
			ZString sightNumber = "SIGHT123";
			ZString completionNumber = "COMPLETE";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("Pre-condition: DeclarationNumber", ZString.Empty, Declaration.DeclarationNumber);
			AssertEquals("Pre-condition: JE_OriginalEntryNumber", ZString.Empty, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Stand alone completion entry should allow entry of original entry number", false, Declaration.JE_OriginalEntryNumberInfo.ReadOnly);
			AssertEquals("Stand alone completion entry should allow entry of original entry type", false, Declaration.JE_OriginalEntryTypeInfo.ReadOnly);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			Declaration.DeclarationNumber = sightNumber;
			AssertEquals("Declaration.DeclarationNumber", sightNumber, Declaration.DeclarationNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumber", ZString.Empty, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Declaration.JE_OriginalEntryTypeInfo.ReadOnly", true, Declaration.JE_OriginalEntryTypeInfo.ReadOnly);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertEquals("Declaration.DeclarationNumber", ZString.Empty, Declaration.DeclarationNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumber", sightNumber, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Declaration.JE_OriginalEntryTypeInfo.ReadOnly", true, Declaration.JE_OriginalEntryTypeInfo.ReadOnly);

			Declaration.DeclarationNumber = completionNumber;
			AssertEquals("Declaration.DeclarationNumber", completionNumber, Declaration.DeclarationNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumber", sightNumber, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Declaration.JE_OriginalEntryTypeInfo.ReadOnly", true, Declaration.JE_OriginalEntryTypeInfo.ReadOnly);
		}

		public void TestEntryNumberForCompletionEntryFromTemporary()
		{
			ZString temporaryNumber = "SIGHT123";
			ZString completionNumber = "COMPLETE";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertEquals("Declaration.DeclarationNumber", ZString.Empty, Declaration.DeclarationNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumber", ZString.Empty, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumberInfo.ReadOnly", false, Declaration.JE_OriginalEntryNumberInfo.ReadOnly);

			Declaration.JE_OriginalEntryNumber = temporaryNumber;
			AssertEquals("Declaration.DeclarationNumber", ZString.Empty, Declaration.DeclarationNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumber", temporaryNumber, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumberInfo.ReadOnly", false, Declaration.JE_OriginalEntryNumberInfo.ReadOnly);

			Declaration.DeclarationNumber = completionNumber;
			AssertEquals("Declaration.DeclarationNumber", completionNumber, Declaration.DeclarationNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumber", temporaryNumber, Declaration.JE_OriginalEntryNumber);
			AssertEquals("Declaration.JE_OriginalEntryNumberInfo.ReadOnly", false, Declaration.JE_OriginalEntryNumberInfo.ReadOnly);
		}

		public void TestDateOfValuation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = new ZDateTime(2005, 1, 1);
			declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 31);
			AssertEquals("DateOfValuation", new ZDateTime(2005, 1, 31), declaration.DateOfValuation);
		}

		public void TestDocumentSuporterIsRightType()
		{
			AssertEquals("Declaration.DocumentSupporter.GetType()", typeof(JobDeclarationDocumentSupporter), Declaration.DocumentSupporter.GetType());
		}

		public void TestMergeManagerGetsRightTypeWhenEntrySubTypeIsChanged()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Started off as an ECI Writeoff.... Declaration.MergeManager is", typeof(ECIWriteOff.MergeManager), Declaration.MergeManager.GetType());
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Customs.Business.MergeManager mergeManager = Declaration.MergeManager;
			AssertEquals("Turned it into a formal entry and.... Declaration.MergeManager is", typeof(FormalEntry.MergeManager), mergeManager.GetType());
			AssertEquals("Make sure MergeManager is Cached and not a new object being returned every time", mergeManager, Declaration.MergeManager);
		}

		public void TestAutoCreatePackagesIfPossible()
		{
			Declaration.AutoCreatePackagesIfPossible();
			AssertEquals("Declaration.Packages.Count", 0, Declaration.Packages.Count);
			Declaration.JE_HouseBill = "123HOUSE";
			Declaration.AutoCreatePackagesIfPossible();
			AssertEquals("Declaration.Packages.Count", 0, Declaration.Packages.Count);
			Declaration.JE_TotalNoOfPacksPackType = "PK";
			Declaration.AutoCreatePackagesIfPossible();
			AssertEquals("Declaration.Packages.Count", 0, Declaration.Packages.Count);
			Declaration.JE_TotalNoOfPacks = 12;
			Declaration.AutoCreatePackagesIfPossible();
			AssertEquals("Declaration.Packages.Count", 1, Declaration.Packages.Count);
			Package pack = Declaration.Packages[0];
			AssertEquals("Pack.CW_PackQty", 12, pack.CW_PackQty);
			AssertEquals("Pack.CW_PackType", "PK", pack.CW_PackType);
			AssertEquals("Pack.CW_HouseBill", "123HOUSE", pack.CW_HouseBill);
			AssertEquals("Pack.CW_ContainerNoOrEquipmentNo", "", pack.CW_ContainerNoOrEquipmentNo);
			Declaration.Packages.RemoveAndDeleteAll();
			CusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OOCL0000006";
			Declaration.AutoCreatePackagesIfPossible();
			AssertEquals("Declaration.Packages.Count", 1, Declaration.Packages.Count);
			pack = Declaration.Packages[0];
			AssertEquals("Pack.CW_PackQty", 12, pack.CW_PackQty);
			AssertEquals("Pack.CW_PackType", "PK", pack.CW_PackType);
			AssertEquals("Pack.CW_HouseBill", "123HOUSE", pack.CW_HouseBill);
			AssertEquals("Pack.CW_ContainerNoOrEquipmentNo", "OOCL0000006", pack.CW_ContainerNoOrEquipmentNo);
			Declaration.AutoCreatePackagesIfPossible();
			AssertEquals("Declaration.Packages.Count", 1, Declaration.Packages.Count);
			Declaration.Packages.RemoveAndDeleteAll();
			CusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OOCL0000011";
			Declaration.AutoCreatePackagesIfPossible();
			AssertEquals("Declaration.Packages.Count", 0, Declaration.Packages.Count);
		}

		public void TestActiveEntryHeadersOnlyReturnsEntryHeadersOfTheCurrentType()
		{
			FormalEntry.CusEntryHeader formalHeader = (FormalEntry.CusEntryHeader)Declaration.CusEntryHeader;
			formalHeader.Messages.AddNew();
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			ECIWriteOff.CusEntryHeader eCIHeader = (ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader;
			eCIHeader.Messages.AddNew();

			AssertEquals("ActiveEntryHeaders.Length", 1, Declaration.ActiveEntryHeaders.Count);
			AssertEquals("ActiveEntryHeaders[0]", eCIHeader, Declaration.ActiveEntryHeaders[0]);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("ActiveEntryHeaders.Length", 1, Declaration.ActiveEntryHeaders.Count);
			AssertEquals("ActiveEntryHeaders[0]", formalHeader, Declaration.ActiveEntryHeaders[0]);
		}

		public void TestLoadCorrectEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_RecordAdded = ZDateTime.Today;
			entryHeader2.CH_RecordAdded = ZDateTime.Today.AddDays(-1);
			AssertEquals("Load the earliest header", entryHeader2.PK, declaration.CusEntryHeader.PK);
			Assert(entryHeader1.IsDeleted);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration1.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCC; // declaration1.IsTSWCancellation = false
			declaration1.CustomsEntryHeaders.RemoveAndDeleteAll();
			var entryHeader3 = declaration1.CustomsEntryHeaders.AddNew();
			var entryHeader4 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_RecordAdded = ZDateTime.Today;
			entryHeader4.CH_RecordAdded = ZDateTime.Today.AddDays(-1);
			entryHeader3.CH_IsActive = false;
			entryHeader4.CH_IsActive = false;
			entryHeader3.Messages.AddNew();
			AssertEquals("Load the header with messages", entryHeader3.PK, declaration1.CusEntryHeader.PK);
			Assert(entryHeader4.IsDeleted);
		}

		public void TestHasDuplicatedActiveEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_RecordAdded = ZDateTime.Today;
			entryHeader2.CH_RecordAdded = ZDateTime.Today.AddDays(-1);
			Assert(declaration.HasDuplicatedActiveEntryHeader());
		}

		public void TestFormalEntryFieldsDefaultProperlyWhenFlickingAcrossFromAFormalEntry()
		{
			NZCustomsDataRegistry.Instance.DefaultCustomsProcessingPort.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NZCHC");

			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_FullName = "Importer";
			importer.OH_RL_NKClosestPort = "NZAKL";
			importer.MiscServ.OM_IMPaymentMethod = PaymentMethodList.Codes.ClientDeferred;
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways;

			Declaration.JE_OH_Importer = importer.PK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_DateOfArrival = Declaration.CachedTodaysDate;

			Declaration.JE_EDITransmitDate = ZDateTime.Empty;
			Declaration.JE_RL_NKProcessingPort = ZString.Empty;
			Declaration.JE_PaymentMethod = ZString.Empty;
			Declaration.JE_MergeBy = ZString.Empty;

			AssertEquals("Precondition: Declaration.JE_RL_NKProcessingPort", ZString.Empty, Declaration.JE_RL_NKProcessingPort);
			AssertEquals("Precondition: Declaration.JE_PaymentMethod", ZString.Empty, Declaration.JE_PaymentMethod);
			AssertEquals("Precondition: Declaration.JE_MergeBy", ZString.Empty, Declaration.JE_MergeBy);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			AssertEquals("Declaration.JE_RL_NKProcessingPort", "NZCHC", Declaration.JE_RL_NKProcessingPort);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.ClientDeferred, Declaration.JE_PaymentMethod);
			AssertEquals("Declaration.JE_MergeBy", OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways, Declaration.JE_MergeBy);
		}

		public void TestDeclarationHasNoCusEntryHeadersByDefault()
		{
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 0, Declaration.CustomsEntryHeaders.Count);
		}

		public void TestChangingBetweenDifferentMessageTypesChangesDeclarationStatusFlags()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_EDITransmitDate = Declaration.CachedTodaysDate.AddDays(42);
			AssertEquals("Precondition: Declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, Declaration.JE_MessageSubType);

			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.JE_ECI_LastResponseStatus", ZString.Empty, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, Declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Declaration.JE_EDITransmitDate", Declaration.CachedTodaysDate.AddDays(42), Declaration.JE_EDITransmitDate);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.NoStatusReported, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Declaration.JE_EDITransmitDate", ZDateTime.Empty, Declaration.JE_EDITransmitDate);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.NoStatusReported, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Declaration.JE_EDITransmitDate", Declaration.JE_EDITransmitDate, Declaration.JE_EDITransmitDate);

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.Messages.AddNew();

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.NoStatusReported, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Declaration.JE_EDITransmitDate", ZDateTime.Empty, Declaration.JE_EDITransmitDate);

			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.Messages.AddNew();

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.NoStatusReported, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Declaration.JE_EDITransmitDate", Declaration.JE_EDITransmitDate, Declaration.JE_EDITransmitDate);
		}

		public void TestChangingSubTypeFlipsBetweenDifferentKindsOfEntryHeaderWithOldReference()
		{
			AssertChangingSubTypeFlipsBetweenDifferentKindsOfEntryHeader(NumberFountains.OldECIManifestReferencePrefix);
		}

		public void TestChangingSubTypeFlipsBetweenDifferentKindsOfEntryHeaderWithNewReference()
		{
			AssertChangingSubTypeFlipsBetweenDifferentKindsOfEntryHeader(NumberFountains.ECIManifestReferencePrefix);
		}

		void AssertChangingSubTypeFlipsBetweenDifferentKindsOfEntryHeader(string eciManifestingReferencePrefix)
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.CusEntryHeader.GetType()", typeof(FormalEntry.CusEntryHeader), Declaration.CusEntryHeader.GetType());
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 1, Declaration.CustomsEntryHeaders.Count);
			Factory.Save();

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertEquals("Declaration.CusEntryHeader.GetType()", typeof(FormalEntry.CusEntryHeader), Declaration.CusEntryHeader.GetType());
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 1, Declaration.CustomsEntryHeaders.Count);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.CusEntryHeader.GetType()", typeof(ECIWriteOff.CusEntryHeader), Declaration.CusEntryHeader.GetType());
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 1, Declaration.CustomsEntryHeaders.Count);
			Factory.Save();

			ZString manifestReference = eciManifestingReferencePrefix + "00001000";
			Declaration.JE_DeclarationReference = manifestReference + "-1";
			Declaration.CusEntryHeader.CH_IsActive = false;
			AssertEquals("Declaration.CusEntryHeader.GetType()", typeof(ECIWriteOff.Manifesting.CusEntryHeader), Declaration.CusEntryHeader.GetType());

			Declaration.CusEntryHeader.CH_IsActive = false;
			ECIWriteOff.Manifesting.CusEntryHeader manifestEntryHeader = Factory.New<ECIWriteOff.Manifesting.CusEntryHeader>();
			Declaration.CustomsEntryHeaders.Add(manifestEntryHeader);
			manifestEntryHeader.CH_JE = Declaration.PK;
			manifestEntryHeader.CH_BGMReference = manifestReference;
			AssertEquals("Declaration.CusEntryHeader.GetType()", typeof(ECIWriteOff.Manifesting.CusEntryHeader), Declaration.CusEntryHeader.GetType());

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.CusEntryHeader.GetType()", typeof(FormalEntry.CusEntryHeader), Declaration.CusEntryHeader.GetType());
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 4, Declaration.CustomsEntryHeaders.Count);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.CusEntryHeader.GetType()", typeof(ECIWriteOff.Manifesting.CusEntryHeader), Declaration.CusEntryHeader.GetType());
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 3, Declaration.CustomsEntryHeaders.Count);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.CusEntryHeader.GetType()", typeof(FormalEntry.CusEntryHeader), Declaration.CusEntryHeader.GetType());
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 4, Declaration.CustomsEntryHeaders.Count);

			Declaration.CusEntryHeader.Messages.AddNew();
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.CusEntryHeader.GetType()", typeof(ECIWriteOff.Manifesting.CusEntryHeader), Declaration.CusEntryHeader.GetType());
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 4, Declaration.CustomsEntryHeaders.Count);
		}

		public void TestSettingJE_MessageTypeUpdatesJE_MessageSubTypeIfSubTypeIsInvalid()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals("Declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Excise, Declaration.JE_MessageSubType);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, Declaration.JE_MessageSubType);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Declaration.JE_MessageSubType", JobMessageSubTypeList.Codes.WriteOff, Declaration.JE_MessageSubType);
		}

		public void TestTypeOfEntryHeaderRequiredForCurrentDeclarationSettings()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings", typeof(FormalEntry.CusEntryHeader), Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings", typeof(ECIWriteOff.CusEntryHeader), Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings);

			Declaration.JE_DeclarationReference = NumberFountains.OldECIManifestReferencePrefix + "00001000-1";
			AssertEquals("Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings", typeof(ECIWriteOff.Manifesting.CusEntryHeader), Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings);

			Declaration.JE_DeclarationReference = NumberFountains.ECIManifestReferencePrefix + "00001000-1";
			AssertEquals("Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings", typeof(ECIWriteOff.Manifesting.CusEntryHeader), Declaration.TypeOfEntryHeaderRequiredForCurrentDeclarationSettings);
		}

		public void TestECIInvoiceAmountAndCurrency()
		{
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			RefCurrency currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			RefCurrency currencyAUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			RefCurrency currencyUSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			AssertEquals("Default Value for JE_ECI_InvoiceAmount", 0.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("Default Value for JE_ECI_InvoiceAmountReadOnly", false, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("Default Value for JE_ECI_InvoiceCurrencyReadOnly", false, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count - InvoiceHeader should not be created just by reading values", 0, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count - InvoiceLine should not be created just by reading values", 0, Declaration.InvoiceLines.Count);

			Declaration.JE_ECI_InvoiceAmount = 100.00m;
			AssertEquals("JE_ECI_InvoiceAmount", 100.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceCurrency", JobDeclaration.LocalCurrencyConstantCode, Declaration.ECI_InvoiceCurrency.RX_Code);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", false, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", false, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 0, Declaration.InvoiceLines.Count);

			Declaration.JE_ECI_InvoiceCurrency = currencyUSD.PK;
			AssertEquals("JE_ECI_InvoiceAmount", 100.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceCurrency", currencyUSD.RX_Code, Declaration.ECI_InvoiceCurrency.RX_Code);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", false, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", false, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 0, Declaration.InvoiceLines.Count);

			JobComInvoiceHeader invoiceHeader1 = Declaration.Invoices[0];
			invoiceHeader1.JZ_InvoiceAmount = 300.00m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = currencyAUD.RX_Code;
			AssertEquals("JE_ECI_InvoiceAmount", 300.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceCurrency", currencyAUD.RX_Code, Declaration.ECI_InvoiceCurrency.RX_Code);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", false, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", false, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 0, Declaration.InvoiceLines.Count);

			JobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			AssertEquals("JE_ECI_InvoiceAmount", 0.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", true, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", true, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 1, Declaration.InvoiceLines.Count);

			invoiceLine1.JI_LinePrice = 300.00m;
			AssertEquals("JE_ECI_InvoiceAmount", 300.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceCurrency", currencyAUD.RX_Code, Declaration.ECI_InvoiceCurrency.RX_Code);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", true, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", true, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 1, Declaration.InvoiceLines.Count);

			JobComInvoiceHeader invoiceHeader2 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 50.00m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = currencyUSD.RX_Code;
			AssertEquals("JE_ECI_InvoiceAmount", 300.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceCurrency", currencyAUD.RX_Code, Declaration.ECI_InvoiceCurrency.RX_Code);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", true, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", true, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 2, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 1, Declaration.InvoiceLines.Count);

			JobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 50.00m;
			AssertEquals("JE_ECI_InvoiceAmount", 411.46m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceCurrency", currencyNZD.RX_Code, Declaration.ECI_InvoiceCurrency.RX_Code);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", true, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", true, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 2, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 2, Declaration.InvoiceLines.Count);

			invoiceHeader1.JobComInvoiceLines.RemoveAndDeleteAll();
			invoiceHeader2.JobComInvoiceLines.RemoveAndDeleteAll();
			AssertEquals("JE_ECI_InvoiceAmount", 0.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", true, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", true, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 2, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 0, Declaration.InvoiceLines.Count);

			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Delete(invoiceHeader1);
			AssertEquals("JE_ECI_InvoiceAmount", 50.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceCurrency", currencyUSD.RX_Code, Declaration.ECI_InvoiceCurrency.RX_Code);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", false, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", false, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 0, Declaration.InvoiceLines.Count);

			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Delete(invoiceHeader2);
			AssertEquals("JE_ECI_InvoiceAmount", 0.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("JE_ECI_InvoiceAmountReadOnly", false, Declaration.JE_ECI_InvoiceAmountReadOnly);
			AssertEquals("JE_ECI_InvoiceCurrencyReadOnly", false, Declaration.JE_ECI_InvoiceCurrencyReadOnly);
			AssertEquals("Declaration.Invoices.Count", 0, Declaration.Invoices.Count);
			AssertEquals("Declaration.InvoiceLines.Count", 0, Declaration.InvoiceLines.Count);
		}

		public void TestInvoiceAmountReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("Default Value for JE_ECI_InvoiceAmountReadOnly", false, Declaration.JE_ECI_InvoiceAmountReadOnly);

			var invLine1 = Declaration.InvoiceLines.AddNew();
			invLine1.JI_Tariff = "2101.11.00.01C";
			invLine1.JI_LinePrice = 450m;
			AssertEquals("JE_ECI_InvoiceAmountReadOnly needs to be set when invoice lines are being used in write-off declaration", true, Declaration.JE_ECI_InvoiceAmountReadOnly);
		}

		public void TestYouCanSetJE_ECI_InvoiceCurrencyToZGuidDotEmpty()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ECI_InvoiceCurrency = ZGuid.Empty;
			AssertEquals("Declaration.JE_ECI_InvoiceCurrency", ZGuid.Empty, Declaration.JE_ECI_InvoiceCurrency);
		}

		public override void TestMasterBillsCommaSeparated()
		{
			Declaration.JE_MasterBill = "MBL1";
			AssertEquals("MBL1", Declaration.MasterBillsCommaSeparated);
		}

		public void TestPackagesCollectionIsRightType()
		{
			AssertEquals("Declaration.Packages.GetType()", typeof(BaseDeclarationLevelPackageCollection<Package>), Declaration.Packages.GetType());
		}

		public void TestPaymentMethod()
		{
			Declaration.JE_PaymentMethod = "";
			AssertEquals("Payment Method should be blank", "", Declaration.PaymentMethod);
			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByClient;
			AssertEquals("Payment Method should Match Description", PaymentMethodList.Descriptions.CashPaidByClient, Declaration.PaymentMethod);
		}

		public void TestLastBrokerToSubmitOrBrokerSelectedOrCurrentUser()
		{
			AssertEquals("Expect CurrentUser as no messages have been sent.", Declaration.LastBrokerToSubmitOrBrokerSelectedOrCurrentUser, GlbStaff.CurrentUser);

			GlbStaff broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BRO";
			Declaration.JE_GS_NKCusAgent = broker.GS_Code;
			AssertEquals("Expect Broker from Declaration as no messages have been sent.", Declaration.LastBrokerToSubmitOrBrokerSelectedOrCurrentUser, broker);

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ZXY";
			NZCMessage message = Declaration.CusEntryHeader.Messages.AddNew();
			message.IsTransmitMessage = true;
			message.EM_SystemCreateUser = staff.GS_Code;
			AssertEquals("Expect User against First Message Sent.", Declaration.LastBrokerToSubmitOrBrokerSelectedOrCurrentUser, staff);
		}

		public override void TestMessageTypeForDocumentFilter()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import type", JobMessageTypeList.Codes.Import, Declaration.MessageTypeForDocumentFilter);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export type", JobMessageTypeList.Codes.Export, Declaration.MessageTypeForDocumentFilter);
		}

		public override void TestMessageTypeForHSAssist()
		{
			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Import Type", SharedJobMessageTypeList.Codes.Import, Declaration.MessageTypeForHSAssist);
			Declaration.JE_MessageType = NZJobMessageTypeList.Codes.Excise;
			AssertEquals("Import Type", SharedJobMessageTypeList.Codes.Import, Declaration.MessageTypeForHSAssist);
			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Export Type", SharedJobMessageTypeList.Codes.Export, Declaration.MessageTypeForHSAssist);
			Declaration.JE_MessageType = "INV";
			AssertEquals("Invalid Type still returned", "INV", Declaration.MessageTypeForHSAssist);
		}

		public override void TestMessageTypesForDocumentFilter()
		{
			Declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals("Import type", "," + JobMessageTypeList.Codes.Import + ",", Declaration.MessageTypesForDocumentFilter);

			Declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals("Export type", "," + JobMessageTypeList.Codes.Export + ",", Declaration.MessageTypesForDocumentFilter);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			AssertEquals("Drawback type", "," + JobMessageTypeList.Codes.Export + "," + JobMessageSubTypeList.Codes.Drawback + ",", Declaration.MessageTypesForDocumentFilter);

			Declaration.JE_MessageType = ZString.Empty;
			Declaration.JE_MessageSubType = ZString.Empty;
			AssertEquals("Empty type", "", Declaration.MessageTypesForDocumentFilter);
		}

		public override void TestIsDrawback()
		{
			AssertEquals("Precondition: Declaration.IsDrawback", false, Declaration.IsDrawback);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			AssertEquals("Declaration.IsDrawback", true, Declaration.IsDrawback);
		}

		public void TestHasContainersAndTheyreAllEmpty()
		{
			AssertEquals("Declaration.HasContainersAndTheyreAllEmpty", false, Declaration.HasContainersAndTheyreAllEmpty);
			CusContainer container = Declaration.CusContainers.AddNew();
			AssertEquals("Declaration.HasContainersAndTheyreAllEmpty", false, Declaration.HasContainersAndTheyreAllEmpty);
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertEquals("Declaration.HasContainersAndTheyreAllEmpty", true, Declaration.HasContainersAndTheyreAllEmpty);
			Declaration.CusContainers.AddNew();
			AssertEquals("Declaration.HasContainersAndTheyreAllEmpty", false, Declaration.HasContainersAndTheyreAllEmpty);
		}

		public void TestIsTSWCREWriteOff()
		{
			AssertEquals("IsTSWCREWriteOff (default)", false, Declaration.IsTSWCREWriteOff);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWCREWriteOff (TSW / Export)", false, Declaration.IsTSWCREWriteOff);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("IsTSWCREWriteOff (TSW / Import / Write-off)", false, Declaration.IsTSWCREWriteOff);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWCREWriteOff (TSW / Export / Normal)", false, Declaration.IsTSWCREWriteOff);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWCREWriteOff (TSW / Export / Write-off)", true, Declaration.IsTSWCREWriteOff);
		}

		public void TestIsTSWEmptyContainerWriteOff()
		{
			AssertEquals("IsTSWEmptyContainerWriteOff (default)", false, Declaration.IsTSWEmptyContainerWriteOff);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWEmptyContainerWriteOff (TSW CRE no containers)", false, Declaration.IsTSWEmptyContainerWriteOff);

			var container1 = Declaration.CusContainers.AddNew();
			AssertEquals("IsTSWEmptyContainerWriteOff (TSW CRE not all empty containers)", false, Declaration.IsTSWEmptyContainerWriteOff);

			container1.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			var container2 = Declaration.CusContainers.AddNew();
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("IsTSWEmptyContainerWriteOff (TSW CRE not all empty containers)", false, Declaration.IsTSWEmptyContainerWriteOff);

			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertEquals("IsTSWEmptyContainerWriteOff (TSW CRE and all empty containers)", true, Declaration.IsTSWEmptyContainerWriteOff);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWEmptyContainerWriteOff (TSW ICR entry with all empty containers)", true, Declaration.IsTSWEmptyContainerWriteOff);
		}

		public void TestIsTSWICRWriteOff()
		{
			AssertEquals("IsTSWICRWriteOff (default)", false, Declaration.IsTSWICRWriteOff);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWICRWriteOff (TSW / Import)", false, Declaration.IsTSWICRWriteOff);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWICRWriteOff (TSW / Import / Write-off)", true, Declaration.IsTSWICRWriteOff);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("IsTSWICRWriteOff (Export / Normal)", false, Declaration.IsTSWICRWriteOff);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWICRWriteOff (TSW / Export / Write-off)", false, Declaration.IsTSWICRWriteOff);
		}

		public void TestIsTSWWriteOff()
		{
			AssertEquals("IsTSWWriteOff (default)", false, Declaration.IsTSWWriteOff);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWWriteOff (TSW / Import)", false, Declaration.IsTSWWriteOff);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWWriteOff (TSW / Import / Write-off)", true, Declaration.IsTSWWriteOff);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("IsTSWWriteOff (Export / Normal)", false, Declaration.IsTSWWriteOff);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("IsTSWWriteOff (TSW / Export / Write-off)", true, Declaration.IsTSWWriteOff);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("IsTSWWriteOff (Legacy / Export / Write-off)", false, Declaration.IsTSWWriteOff);
		}

		public void TestTSWSimplifiedMiscEntry()
		{
			AssertEquals("TSWSimplifiedMiscEntry (default)", false, Declaration.TSWSimplifiedMiscEntry);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("TSWSimplifiedMiscEntry (TSW / Import)", false, Declaration.TSWSimplifiedMiscEntry);
			Declaration.JE_OH_Importer = Declaration.CachedMiscOrgPK;
			Declaration.MiscImporterName = "JOHN'S SPARE BOLTS";
			AssertEquals("TSWSimplifiedMiscEntry (TSW / Import / Misc Importer)", true, Declaration.TSWSimplifiedMiscEntry);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("TSWSimplifiedMiscEntry (TSW / Import - Simplified / Misc Importer)", true, Declaration.TSWSimplifiedMiscEntry);
		}

		public void TestDateForDutyRateReturnsTodayIfBarrierDateIsInvalid()
		{
			ZDateTime validDate = new ZDateTime(2005, 12, 12);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			Declaration.JE_DateOfArrival = validDate;
			AssertEquals("Declaration.DateForDutyRate for Valid Date", validDate, Declaration.DateForDutyRate);

			Declaration.JE_DateOfArrival = ZDateTime.Invalid;
			AssertEquals("Declaration.DateForDutyRate for Invalid Date", Declaration.CachedTodaysDate, Declaration.DateForDutyRate);

			Declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals("Declaration.DateForDutyRate for Empty Date", Declaration.CachedTodaysDate, Declaration.DateForDutyRate);

			Declaration.JE_DateOfArrival = validDate;
			AssertEquals("Declaration.DateForDutyRate for Valid Date when Set again", validDate, Declaration.DateForDutyRate);
		}

		public void TestJE_ExchangeRateDateReturnsTodayIfJE_EDITransmitDateIsInvalid()
		{
			ZDateTime validDate = new ZDateTime(2005, 12, 12);

			Declaration.JE_EDITransmitDate = validDate;
			AssertEquals("Declaration.JE_ExchangeRateDate for Valid Date", validDate, Declaration.JE_ExchangeRateDate);

			Declaration.JE_EDITransmitDate = ZDateTime.Invalid;
			AssertEquals("Declaration.JE_ExchangeRateDate for Invalid Date", Declaration.CachedTodaysDate, Declaration.JE_ExchangeRateDate);

			Declaration.JE_EDITransmitDate = ZDateTime.Empty;
			AssertEquals("Declaration.JE_ExchangeRateDate for Empty Date", Declaration.CachedTodaysDate, Declaration.JE_ExchangeRateDate);

			Declaration.JE_EDITransmitDate = validDate;
			AssertEquals("Declaration.JE_ExchangeRateDate for Valid Date when Set again", validDate, Declaration.JE_ExchangeRateDate);
		}

		public void TestTemplateCopyWhenDeclarationIsOnShipmentDoesntBarfWhenCloningNotes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Declaration.JE_JS = shipment.PK;
			var note = Declaration.Notes.AddNew(isCustomDescription: true, "My Note", "Remember to Fix Exceptions");
			AssertEquals("Precondition: Shipment.Notes[0] is the note that was stored on the Declaration", note, ((StmNoteCollection)shipment.Notes.GetAllNotes())[0]);
			AssertEquals("Precondition: Declaration.Notes[0] is the note that was stored on the Declaration", note, ((StmNoteCollection)Declaration.Notes.GetAllNotes())[0]);
			var shipmentNotesType = shipment.Notes.GetType();
			AssertEquals(typeof(ForwardingShipmentStmNotes), shipmentNotesType);

			var clonedShipment = shipment.TemplateCopy() as ForwardingShipment;
			AssertNotNull("ClonedShipment", clonedShipment);
			AssertEquals("ClonedShipment.Declarations.Length", 1, clonedShipment.Declarations.Length);
			var clonedDeclaration = clonedShipment.Declarations[0] as JobDeclaration;
			AssertNotNull("ClonedDeclaration", clonedDeclaration);
			var clonedNotesType = clonedShipment.Notes.GetType();
			AssertEquals(typeof(ForwardingShipmentStmNotes), clonedNotesType);

			var secondNote = clonedDeclaration.Notes.AddNew(isCustomDescription: true, "My Second Note", "Remember to Fix Exceptions");
			Assert("ClonedShipment.Notes has the new note stored", ((StmNoteCollection)clonedShipment.Notes.GetAllNotes()).Contains(secondNote));
			Assert("ClonedDeclaration.Notes has the new note stored", ((StmNoteCollection)clonedDeclaration.Notes.GetAllNotes()).Contains(secondNote));
		}

		public void TestCloningDeclarationDoesntClobberTSWStatusOnSaving()
		{
			var originalDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			originalDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			originalDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			originalDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			originalDeclaration.JE_DeclarationReference = "G00010001";
			originalDeclaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCC;
			Factory.Save();

			var clonedDeclaration = originalDeclaration.TemplateCopy();
			Factory.Save();

			AssertEquals("Cloned declaration should have a blank JE_TSWCombinedStatus", ZString.Empty, clonedDeclaration.JE_TSWCombinedStatus);
			AssertEquals("Original declaration should still have status in JE_TSWCombinedStatus", TSWEntryStatusList.Codes.PCC, originalDeclaration.JE_TSWCombinedStatus);
		}

		public void TestDefaultingNumberOfPacksToPivot()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_TotalNoOfPacks = 123;
			AssertEquals("House Bill Containers", 0, declaration.PackingGroups.Count);

			declaration.JE_HouseBill = "HouseBill";
			AssertEquals("House Bill Containers", 1, declaration.PackingGroups.Count);
			AssertEquals("Should use the primary bill", declaration.PrimaryHouseBill, declaration.PackingGroups[0].Bill);
			AssertEquals("House Bill Containers", 123, declaration.Packages[0].CW_PackQty);

			declaration.JE_TotalNoOfPacksPackType = "XX";
			AssertEquals("Pack type", "XX", declaration.Packages[0].CW_PackType);
		}

		public void TestCusDecHouseBills()
		{
			Bill houseBill = Declaration.Bills.AddNew();
			AssertNotNull(houseBill);
		}

		public void TestJE_ExchangeRateDate()
		{
			AssertEquals("Declaration.JE_ExchangeRateDate", Declaration.CachedTodaysDate, Declaration.JE_ExchangeRateDate);
			Declaration.JE_EDITransmitDate = new ZDateTime(2004, 1, 1);
			AssertEquals("Declaration.JE_ExchangeRateDate", new ZDateTime(2004, 1, 1), Declaration.JE_ExchangeRateDate);
			Declaration.JE_EDITransmitDate = ZDateTime.Empty;
			AssertEquals("Declaration.JE_ExchangeRateDate", Declaration.CachedTodaysDate, Declaration.JE_ExchangeRateDate);
		}

		public void TestJE_ExchangeRateDate_CompletionEntry()
		{
			var pastEntry = Declaration.CustomsEntryHeaders.AddNew(typeof(FormalEntry.CusEntryHeader));
			pastEntry.CH_EDITransmitDate = new ZDateTime(2004, 1, 1);
			pastEntry.Messages.AddNew();
			pastEntry.EntryNumber = "123";
			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Completion;
			AssertEquals("Completion entry uses past exchange rate", new ZDateTime(2004, 1, 1), Declaration.JE_ExchangeRateDate);
		}

		public void TestHasCustomsMessages()
		{
			AssertEquals("Precondition: Declaration.HasCustomsMessages", false, Declaration.HasCustomsMessages);
			Declaration.Messages.AddNew();
			AssertEquals("Messages in Declaration.Messages should not affect Declaration.HasCustomsMessages in NZ", false, Declaration.HasCustomsMessages);
			NZCMessage message2 = Declaration.CusEntryHeader.Messages.AddNew();
			message2.EM_Status = NZCMessage.Status.Cancelled;
			AssertEquals("Declaration.HasCustomsMessages", false, Declaration.HasCustomsMessages);
			Declaration.CusEntryHeader.Messages.AddNew();
			AssertEquals("Declaration.HasCustomsMessages", true, Declaration.HasCustomsMessages);
			Declaration.CusEntryHeader.Messages.RemoveAndDeleteAll();
			AssertEquals("Declaration.HasCustomsMessages", false, Declaration.HasCustomsMessages);
		}
		public void TestIsTSWCancellation()
		{
			Declaration.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
			AssertEquals(true, Declaration.IsTSWCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CAN;
			AssertEquals(true, Declaration.IsTSWCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCC;
			AssertEquals(true, Declaration.IsTSWCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCP;
			AssertEquals(true, Declaration.IsTSWCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCA;
			AssertEquals(true, Declaration.IsTSWCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;
			AssertEquals(true, Declaration.IsTSWCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CTE;
			AssertEquals(false, Declaration.IsTSWCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;
			AssertEquals(false, Declaration.IsTSWCancellation);
		}

		public void TestIsTSWCancelledOrPendingCancellation()
		{
			Declaration.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
			AssertEquals(true, Declaration.IsTSWCancelledOrPendingCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CAN;
			AssertEquals(true, Declaration.IsTSWCancelledOrPendingCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCC;
			AssertEquals(true, Declaration.IsTSWCancelledOrPendingCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCP;
			AssertEquals(true, Declaration.IsTSWCancelledOrPendingCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCA;
			AssertEquals("Customs have not approved this cancellation - need futher info - user needs to be able to process job still", false, Declaration.IsTSWCancelledOrPendingCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;
			AssertEquals("Customs have not approved this cancellation - need futher info - user needs to be able to enter the job still", false, Declaration.IsTSWCancelledOrPendingCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CTE;
			AssertEquals(false, Declaration.IsTSWCancelledOrPendingCancellation);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCC;
			AssertEquals(false, Declaration.IsTSWCancelledOrPendingCancellation);
		}

		public void TestValidationIsOfRightType()
		{
			ZValidation validation = Declaration.Validation;
			Assert("Declaration.Validation is JobDeclarationValidation", validation is JobDeclarationValidation);
		}

		public void TestJE_EDITransmitDateDefaultsToEmpty()
		{
			AssertEquals("Declaration.JE_EDITransmitDate.IsEmpty", true, Declaration.JE_EDITransmitDate.IsEmpty);
		}

		public void TestJE_EDITransmitDatePersistsToEntryHeader()
		{
			Declaration.JE_EDITransmitDate = new ZDateTime(2004, 1, 1);
			AssertEquals("Declaration.JE_EDITransmitDate", new ZDateTime(2004, 1, 1), Declaration.JE_EDITransmitDate);
			AssertEquals("Declaration.CusEntryHeader.CH_EDITransmitDate", new ZDateTime(2004, 1, 1), Declaration.CusEntryHeader.CH_EDITransmitDate);

			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);
			AssertEquals("Declaration.JE_EDITransmitDate", new ZDateTime(2005, 1, 1), Declaration.JE_EDITransmitDate);
			AssertEquals("Declaration.CusEntryHeader.CH_EDITransmitDate", new ZDateTime(2005, 1, 1), Declaration.CusEntryHeader.CH_EDITransmitDate);
		}

		public void TestJE_EDITransmitDateIsReadOnlyAfterAMessageIsGenerated()
		{
			AssertEquals("Precondition: Declaration.JE_EDITransmitDateInfo.ReadOnly", false, Declaration.JE_EDITransmitDateInfo.ReadOnly);
			AssertEquals("Precondition: Declaration.JE_EDITransmitDateFinalised", false, Declaration.JE_EDITransmitDateFinalised);
			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);
			AssertEquals("After Setting JE_EDITransmitDate: Declaration.JE_EDITransmitDateInfo.ReadOnly", false, Declaration.JE_EDITransmitDateInfo.ReadOnly);
			AssertEquals("After Setting JE_EDITransmitDate: Declaration.JE_EDITransmitDateFinalised", false, Declaration.JE_EDITransmitDateFinalised);
			var message = Declaration.CusEntryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;//happens for reinstated entries
			AssertEquals("After Setting JE_EDITransmitDate: Declaration.JE_EDITransmitDateInfo.ReadOnly", false, Declaration.JE_EDITransmitDateInfo.ReadOnly);
			AssertEquals("After Setting JE_EDITransmitDate: Declaration.JE_EDITransmitDateFinalised", false, Declaration.JE_EDITransmitDateFinalised);

			var message2 = Declaration.CusEntryHeader.Messages.AddNew();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("After Sending: Declaration.JE_EDITransmitDateInfo.ReadOnly", true, Declaration.JE_EDITransmitDateInfo.ReadOnly);
			AssertEquals("After Sending JE_EDITransmitDate: Declaration.JE_EDITransmitDateFinalised", true, Declaration.JE_EDITransmitDateFinalised);

			Declaration.CusEntryHeader.CH_IsRestored = true;
			AssertEquals(false, Declaration.JE_EDITransmitDateInfo.ReadOnly);

			Declaration.ResetToOriginal();
			AssertEquals("After ResetToOriginal: Declaration.JE_EDITransmitDateInfo.ReadOnly", false, Declaration.JE_EDITransmitDateInfo.ReadOnly);
			AssertEquals("After ResetToOriginal: Declaration.JE_EDITransmitDateFinalised", false, Declaration.JE_EDITransmitDateFinalised);
		}

		public void TestJE_EDITransmitDateIsReadOnlyAfterAMessageIsGenerated_ConsolidatedDeclaration()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				var declaration1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[0];
				var declaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
				declaration1.JE_EDITransmitDate = ZDateTime.Today;
				declaration2.JE_EDITransmitDate = ZDateTime.Today;
				Factory.Save();
				AssertEquals("Pre-condition", false, declaration1.JE_EDITransmitDateInfo.ReadOnly);
				AssertEquals("Pre-condition", false, declaration2.JE_EDITransmitDateInfo.ReadOnly);

				var aggregateDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
				var message = aggregateDeclaration.CusEntryHeader.Messages.AddNew();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessage.Status.Sent;
				consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);
				AssertEquals("After sending message", true, declaration1.JE_EDITransmitDateInfo.ReadOnly);
				AssertEquals("After sending message", true, declaration2.JE_EDITransmitDateInfo.ReadOnly);
			}
		}

		public void TestEntryTypeAndStyle()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.EntryType", JobMessageTypeList.Descriptions.Import, Declaration.EntryType);
			AssertEquals("Declaration.EntryStyle", JobMessageSubTypeList.Descriptions.Normal, Declaration.EntryStyle);
			AssertEquals("Declaration.EntryTypeAndStyle", JobMessageTypeList.Descriptions.Import + " (" + JobMessageSubTypeList.Descriptions.Normal + ")", Declaration.EntryTypeAndStyle);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			AssertEquals("Declaration.EntryType", JobMessageTypeList.Descriptions.Export, Declaration.EntryType);
			AssertEquals("Declaration.EntryStyle", JobMessageSubTypeList.Descriptions.Drawback, Declaration.EntryStyle);
			AssertEquals("Declaration.EntryTypeAndStyle", JobMessageTypeList.Descriptions.Export + " (" + JobMessageSubTypeList.Descriptions.Drawback + ")", Declaration.EntryTypeAndStyle);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Excise;
			AssertEquals("Declaration.EntryType", JobMessageTypeList.Descriptions.Excise, Declaration.EntryType);
			AssertEquals("Declaration.EntryStyle", JobMessageSubTypeList.Descriptions.Excise, Declaration.EntryStyle);
			AssertEquals("Declaration.EntryTypeAndStyle", JobMessageTypeList.Descriptions.Excise, Declaration.EntryTypeAndStyle);
		}

		public void TestDeclarationNumber()
		{
			Declaration.DeclarationNumber = "90210898";
			AssertEquals("90210898", Declaration.DeclarationNumber);
			Declaration.DeclarationNumber = "";
			AssertEquals("", Declaration.DeclarationNumber);
			Declaration.DeclarationNumber = "90210898";
			AssertEquals("90210898", Declaration.DeclarationNumber);
			Declaration.DeclarationNumber = "90210897";
			AssertEquals("90210897", Declaration.DeclarationNumber);
			Declaration.DeclarationNumber = "";
			AssertEquals("", Declaration.DeclarationNumber);
		}

		public void TestDeclarationNumberReallyWorks1()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.FillWithValidTestData();
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			_ = declaration.CusEntryHeader;
			AssertEquals("Declaration.DeclarationNumber", "", declaration.DeclarationNumber);
			declaration.SaveHandlingSaveExceptions();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			JobDeclaration declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			declaration2.DeclarationNumber = "82345678";
			factory2.Save();
			AssertEquals(declaration.GetType(), declaration2.GetType());
			AssertEquals("Declaration.DeclarationNumber", "82345678", declaration.DeclarationNumber);
		}

		public void TestDeclarationNumberReallyWorks2()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.FillWithValidTestData();
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			_ = declaration.CusEntryHeader;
			AssertEquals("Declaration.DeclarationNumber", "", declaration.DeclarationNumber);
			declaration.SaveHandlingSaveExceptions();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			JobDeclaration declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			declaration2.DeclarationNumber = "72345678";
			factory2.Save();

			AssertEquals(declaration.GetType(), declaration2.GetType());

			var declaration3 = Factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Declaration3.DeclarationNumber", "72345678", declaration3.DeclarationNumber);

			AssertEquals("Declaration.DeclarationNumber", "72345678", declaration.DeclarationNumber);
		}

		public void TestIsECIWriteoff()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals(true, Declaration.IsECIWriteoff);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals(false, Declaration.IsECIWriteoff);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			AssertEquals("For TSW, Cargo Report Export (CRE) is the equivalent ECI Write Off message", true, Declaration.IsECIWriteoff);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			AssertEquals("For TSW, Inward Cargo Report (ICR) is the equivalent ECI Write Off message", true, Declaration.IsECIWriteoff);
		}

		public void TestIsImport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, Declaration.IsImport);
		}

		public void TestFormattedMasterBill()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MasterBill = "08111111111";
			AssertEquals("08111111111", Declaration.FormattedMasterBill);

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_MasterBill = "081-11111111";
			AssertEquals("081-11111111", Declaration.FormattedMasterBill);
			Declaration.JE_MasterBill = "08111111111";
			AssertEquals("081-11111111", Declaration.FormattedMasterBill);
			Declaration.JE_MasterBill = "081";
			AssertEquals("081-", Declaration.FormattedMasterBill);
			Declaration.JE_MasterBill = "";
			AssertEquals("", Declaration.FormattedMasterBill);
		}

		public void TestCusEntryHeader()
		{
			AssertEquals("Precondition: Should have no CusEntryHeaders", 0, Declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = Declaration.CusEntryHeader;
			AssertEquals("Should now have 1 CusEntryHeader", 1, Declaration.CustomsEntryHeaders.Count);
			AssertEquals("CusEntryHeaders should match", Declaration.CustomsEntryHeaders[0], entryHeader);
		}

		public void TestCustomsMessageRemarksWhenNotAttachedToShipment()
		{
			AssertEquals("", Declaration.CustomsMessageRemarks);

			Declaration.CustomsMessageRemarks = "Test 1";
			AssertEquals("Test 1", Declaration.CustomsMessageRemarks);

			Declaration.CustomsMessageRemarks = "Test 2";
			AssertEquals("Test 2", Declaration.CustomsMessageRemarks);

			Declaration.CustomsMessageRemarks = "";
			AssertEquals("", Declaration.CustomsMessageRemarks);
		}

		public void TestCustomsMessageRemarksWhenAttachedToShipment()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			Declaration.JE_JS = shipment.PK;

			AssertEquals("", Declaration.CustomsMessageRemarks);

			Declaration.CustomsMessageRemarks = "Test 1";
			AssertEquals("Test 1", Declaration.CustomsMessageRemarks);

			Declaration.CustomsMessageRemarks = "Test 2";
			AssertEquals("Test 2", Declaration.CustomsMessageRemarks);

			Declaration.CustomsMessageRemarks = "";
			AssertEquals("", Declaration.CustomsMessageRemarks);
		}

		public void TestCustomsDeliveryInstructionsWithITR_Sea()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD address";
			transitDestOrgAddr.OA_Code = "TD1";
			Factory.Save();

			Declaration.CustomsDeliveryInstructions = "Response Status: ITA-International Transhipment Approved\r\nInternational Transhipment Approved";
			Declaration.JE_DeclarationReference = "B00004206";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MasterBill = "08100234920";
			Declaration.JE_HouseBill = "G45023";
			Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			Declaration.JE_RL_NKPortOfArrival = "NZAKL";
			var decTR = TranshipmentRequest.Create(Declaration);
			decTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decTR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decTR.C4_TranshipBySeaVessel = "HYOGO MARU";
			decTR.C4_TranshipBySeaVoyage = "1795E";
			decTR.C4_TranshipDepartureDate = new ZDateTime(2020, 02, 29);
			decTR.C4_Status = CombinedMovementStatus.Codes.II;
			decTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;

			var expectedITRInstructions = @"Response Status: ITA-International Transhipment Approved
International Transhipment Approved

To:     TD address
Method of Transport of Transfer:     Road
Exporting Craft: HYOGO MARU     Voyage Number: 1795E     Date of Export: 29/02/2020";
			AssertEquals("CustomsDeliveryInstructionsWithITR for Sea", expectedITRInstructions, Declaration.CustomsDeliveryInstructionsWithITR);
		}

		public void TestCustomsDeliveryInstructionsWithITR_SeaPort()
		{
			Declaration.CustomsDeliveryInstructions = "Response Status: ITA-International Transhipment Approved\r\nInternational Transhipment Approved";
			Declaration.JE_DeclarationReference = "B00004206";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MasterBill = "08100234920";
			Declaration.JE_HouseBill = "G45023";
			Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			Declaration.JE_RL_NKPortOfArrival = "NZAKL";
			var decTR = TranshipmentRequest.Create(Declaration);
			decTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decTR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decTR.C4_TranshipBySeaVessel = "HYOGO MARU";
			decTR.C4_TranshipBySeaVoyage = "1795E";
			decTR.C4_TranshipDepartureDate = new ZDateTime(2020, 02, 29);
			decTR.C4_Status = CombinedMovementStatus.Codes.II;
			decTR.C4_RL_NKTranshipDestPort = "NZWLG";

			var expectedITRInstructions = @"Response Status: ITA-International Transhipment Approved
International Transhipment Approved

To:     NZWLG
Method of Transport of Transfer:     Road
Exporting Craft: HYOGO MARU     Voyage Number: 1795E     Date of Export: 29/02/2020";
			AssertEquals("CustomsDeliveryInstructionsWithITR for Sea", expectedITRInstructions, Declaration.CustomsDeliveryInstructionsWithITR);
		}

		public void TestCustomsDeliveryInstructionsWithITR_Air()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "Port of Tauranga - Sulphur Point";
			transitDestOrgAddr.OA_Code = "TD1";
			Factory.Save();

			Declaration.CustomsDeliveryInstructions = "Response Status: ITA-International Transhipment Approved\r\nInternational Transhipment Approved";
			Declaration.JE_DeclarationReference = "B00004206";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MasterBill = "08100234920";
			Declaration.JE_HouseBill = "G45023";
			Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			Declaration.JE_RL_NKPortOfArrival = "NZAKL";
			var decTR = TranshipmentRequest.Create(Declaration);
			decTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Rail;
			decTR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			decTR.C4_FlightNo = "SQ618";
			decTR.C4_TranshipDepartureDate = new ZDateTime(2020, 03, 04);
			decTR.C4_Status = CombinedMovementStatus.Codes.II;
			decTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;

			var expectedITRInstructions = @"Response Status: ITA-International Transhipment Approved
International Transhipment Approved

To:     Port of Tauranga - Sulphur Point
Method of Transport of Transfer:     Rail
Flight Number: SQ618     Date of Export: 04/03/2020";
			AssertEquals("CustomsDeliveryInstructionsWithITR - for Air", expectedITRInstructions, Declaration.CustomsDeliveryInstructionsWithITR);
		}

		public void TestCustomsDeliveryInstructionsWithITR_Sea_Export()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "TD address";
			transitDestOrgAddr.OA_Code = "TD1";
			Factory.Save();

			Declaration.CustomsDeliveryInstructions = "Response Status: ITA-International Transhipment Approved\r\nInternational Transhipment Approved";
			Declaration.JE_DeclarationReference = "B00004206";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MasterBill = "08100234920";
			Declaration.JE_HouseBill = "G45023";
			Declaration.JE_RL_NKPortOfLoading = "NZAKL";
			Declaration.JE_RL_NKPortOfArrival = "AUSYD";
			var decTR = TranshipmentRequest.Create(Declaration);
			decTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.SeaCV;
			decTR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decTR.C4_TranshipBySeaVessel = "AAL FREMANTLE";
			decTR.C4_TranshipBySeaVoyage = "56W";
			decTR.C4_ArrivalDate = new ZDateTime(2020, 03, 15);
			decTR.C4_Status = CombinedMovementStatus.Codes.II;
			decTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;

			var expectedITRInstructions = @"Response Status: ITA-International Transhipment Approved
International Transhipment Approved

Method of Transport of Transfer:     Sea (Coastal vessel)
Incoming Craft: AAL FREMANTLE     Voyage Number: 56W     Arrival Date: 15/03/2020";
			AssertEquals("CustomsDeliveryInstructionsWithITR for Sea", expectedITRInstructions, Declaration.CustomsDeliveryInstructionsWithITR);
		}

		public void TestCustomsDeliveryInstructionsWithITR_Air_Import()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "TDCODE";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "Port of Tauranga - Sulphur Point";
			transitDestOrgAddr.OA_Code = "TD1";
			Factory.Save();

			Declaration.CustomsDeliveryInstructions = "Response Status: ITA-International Transhipment Approved\r\nInternational Transhipment Approved";
			Declaration.JE_DeclarationReference = "B00004206";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MasterBill = "08100234920";
			Declaration.JE_HouseBill = "G45023";
			Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			Declaration.JE_RL_NKPortOfArrival = "NZAKL";
			var decTR = TranshipmentRequest.Create(Declaration);
			decTR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Rail;
			decTR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			decTR.C4_Status = CombinedMovementStatus.Codes.II;
			decTR.C4_OA_DestinationAddress = transitDestOrgAddr.PK;

			var expectedITRInstructions = @"Response Status: ITA-International Transhipment Approved
International Transhipment Approved

To:     Port of Tauranga - Sulphur Point
Method of Transport of Transfer:     Rail";
			AssertEquals("CustomsDeliveryInstructionsWithITR - for Import Air - flight details are not entered", expectedITRInstructions, Declaration.CustomsDeliveryInstructionsWithITR);
		}

		public void TestCustomsDeliveryInstructionsWhenNotAttachedToShipment()
		{
			AssertEquals("NO DELIVERY ORDER HAS BEEN ISSUED FOR THESE GOODS", Declaration.CustomsDeliveryInstructions);

			Declaration.CustomsDeliveryInstructions = "Test 1";
			AssertEquals("Test 1", Declaration.CustomsDeliveryInstructions);

			Declaration.CustomsDeliveryInstructions = "Test 2";
			AssertEquals("Test 2", Declaration.CustomsDeliveryInstructions);

			Declaration.CustomsDeliveryInstructions = "";
			AssertEquals("NO DELIVERY ORDER HAS BEEN ISSUED FOR THESE GOODS", Declaration.CustomsDeliveryInstructions);
		}

		public void TestCustomsDeliveryInstructionsWhenAttachedToShipment()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			Declaration.JE_JS = shipment.PK;

			AssertEquals("NO DELIVERY ORDER HAS BEEN ISSUED FOR THESE GOODS", Declaration.CustomsDeliveryInstructions);

			Declaration.CustomsDeliveryInstructions = "Test 1";
			AssertEquals("Test 1", Declaration.CustomsDeliveryInstructions);

			Declaration.CustomsDeliveryInstructions = "Test 2";
			AssertEquals("Test 2", Declaration.CustomsDeliveryInstructions);

			Declaration.CustomsDeliveryInstructions = "";
			AssertEquals("NO DELIVERY ORDER HAS BEEN ISSUED FOR THESE GOODS", Declaration.CustomsDeliveryInstructions);
		}

		public void TestEntryStyle()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals(JobMessageSubTypeList.Descriptions.Normal, Declaration.EntryStyle);
		}

		public void TestFilteredInvoiceLines()
		{
			AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(Declaration.FilteredInvoiceLines);
		}

		public override void TestDeveloperErrorOnAddNewNotesToDeclarationAttachedToShipment()
		{
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			Declaration.Notes.AddNew();
			AssertEquals("Should be no developer errors adding notes to a declaration", 0, ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);

			var shipment = Factory.New<ForwardingShipment>();
			Declaration.JE_JS = shipment.PK;
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			try
			{
				Declaration.Notes.AddNew();
				AssertEquals("Should be no developer error adding notes to a declaration with a shipment attached for NZ", 0, ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			}
			finally
			{
				ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public override void TestInvoiceLinesFromJobDeclaration()
		{
			Assert("NZ base JobDeclaration doesnt have invoices and invoice lines. And the generic test fails as it tries to create them", condition: true);
		}

		public void TestJE_RL_NKPortOfFirstArrivalNeverSetToAValue()
		{
			AssertEquals("Precondition - TestDec.JE_RL_NKPortOfFirstArrival is empty", "", Declaration.JE_RL_NKPortOfFirstArrival);
			AssertEquals("Precondition - TestDec.JE_RL_NKPortOfFirstArrival has no notifications", false, Declaration.JE_RL_NKPortOfFirstArrivalInfo.HasNotifications());

			Declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			AssertEquals("This is hack as base synchroniser copies shipment data into dec data including this field and NZ doesnt need this. This field is hidden from form", "", Declaration.JE_RL_NKPortOfFirstArrival);
			AssertEquals("No error", false, Declaration.JE_RL_NKPortOfFirstArrivalInfo.HasNotifications());
		}

		public void TestJE_DateOfFirstArrivalNeverSetToAValue()
		{
			AssertEquals("Precondition - TestDec.JE_RL_NKPortOfFirstArrival is empty", ZDateTime.Empty, Declaration.JE_DateOfFirstArrival);
			AssertEquals("Precondition - TestDec.JE_RL_NKPortOfFirstArrival has no notifications", false, Declaration.JE_DateOfFirstArrivalInfo.HasNotifications());

			Declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			AssertEquals("This is hack as base synchroniser copies shipment data into dec data including this field and NZ doesnt need this. This field is hidden from form", ZDateTime.Empty, Declaration.JE_DateOfFirstArrival);
			AssertEquals("No error", false, Declaration.JE_DateOfFirstArrivalInfo.HasNotifications());
		}

		public void TestBarrierDate()
		{
			ZDateTime testDate1 = new ZDateTime(2004, 12, 12);
			ZDateTime testDate2 = new ZDateTime(2004, 12, 18);

			Declaration.JE_ExportDate = testDate1;
			Declaration.JE_DateOfArrival = testDate2;

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(testDate1, Declaration.BarrierDate);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(testDate2, Declaration.BarrierDate);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(ZDateTime.Empty, Declaration.BarrierDate);

			Declaration.JE_ExportDate = ZDateTime.Empty;
			Declaration.JE_DateOfArrival = ZDateTime.Empty;

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(ZDateTime.Empty, Declaration.BarrierDate);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(ZDateTime.Empty, Declaration.BarrierDate);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(ZDateTime.Empty, Declaration.BarrierDate);
		}

		public override void TestDateForDutyRate()
		{
			AssertEquals("Env.Registry.NZCustoms.ImportEciTestMode", false, NZCustomsDataRegistry.Instance.ImportEciTestMode.Value);
			AssertEquals("Env.Registry.NZCustoms.ExportEciTestMode", false, NZCustomsDataRegistry.Instance.ExportEciTestMode.Value);
			AssertEquals("Env.Registry.NZCustoms.ImportDeclarationsTestMode", false, NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.Value);
			AssertEquals("Env.Registry.NZCustoms.ExportDeclarationsTestMode", false, NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.Value);

			ZDateTime testDate1 = new ZDateTime(2004, 12, 12);
			ZDateTime testDate2 = new ZDateTime(2004, 12, 18);

			Declaration.JE_ExportDate = testDate1;
			Declaration.JE_DateOfArrival = testDate2;

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(testDate1, Declaration.DateForDutyRate);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(testDate2, Declaration.DateForDutyRate);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(Declaration.CachedTodaysDate, Declaration.DateForDutyRate);

			Declaration.JE_ExportDate = ZDateTime.Empty;
			Declaration.JE_DateOfArrival = ZDateTime.Empty;

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(Declaration.CachedTodaysDate, Declaration.DateForDutyRate);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(Declaration.CachedTodaysDate, Declaration.DateForDutyRate);

			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(Declaration.CachedTodaysDate, Declaration.DateForDutyRate);
		}

		public void TestChargeCodeGroups()
		{
			Env.Registry.Rating.SetBrokerageRatedCodes("BRK,BON,CDS,DST");
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(Declaration.IsImport);
			var autoRating = Declaration.RatingAdapter;
			AssertEquals(4, autoRating.ChargeCodeGroups.Count);
		}

		public void TestDescriptionAlwaysFocesItselfToUpperCase()
		{
			Declaration.JE_GoodsDescription = "asdfg";
			AssertEquals("Declaration.JE_GoodsDescription", "ASDFG", Declaration.JE_GoodsDescription);
		}

		public override void TestFirstArrivalVoyageDestination()
		{
			Assert("No port of first arrival, another test will test fall back to port of arrival", condition: true);
		}

		public void TestActiveEntryHeaders()
		{
			var entryHeader = Declaration.CusEntryHeader;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader.CH_IsActive = true;
			Factory.Save();
			var autoRatingInfo = Declaration.RatingAdapter as IAutoRatingCustomsInfo;
			AssertEquals("Should now have 1 Entry Header", 1, autoRatingInfo.Entries.Count);
			entryHeader.CH_IsActive = false;
			Factory.Save();
			AssertEquals("Should not have any Entry Headers", 0, autoRatingInfo.Entries.Count);
		}

		public void TestRefreshBindingOnInvoiceHeader()
		{
			var testHeader = Declaration.Invoices.AddNew();

			Declaration.JE_IsZeroRatedAll = "";
			AssertEquals("Zero Rated Duty on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedDuty);
			AssertEquals("Zero Rated Excise on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedExcise);
			AssertEquals("Zero Rated Levies on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedLevies);
			AssertEquals("Zero Rated GST on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedGST);

			Declaration.JE_IsZeroRatedAll = "Y";
			AssertEquals("Zero Rated Duty on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedDuty);
			AssertEquals("Zero Rated Excise on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedExcise);
			AssertEquals("Zero Rated Levies on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedLevies);
			AssertEquals("Zero Rated GST on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedGST);

			Declaration.JE_IsZeroRatedAll = "N";
			AssertEquals("Zero Rated Duty on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedDuty);
			AssertEquals("Zero Rated Excise on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedExcise);
			AssertEquals("Zero Rated Levies on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedLevies);
			AssertEquals("Zero Rated GST on Declaration", Declaration.JE_IsZeroRatedAll, testHeader.JZ_IsZeroRatedGST);
		}

		public void TestIsAnyZeroRated()
		{
			AssertEquals(false, Declaration.IsAnyZeroRated);

			Declaration.JE_IsZeroRatedAll = YesNoList.Codes.Yes;
			AssertEquals(false, Declaration.IsAnyZeroRated);

			var testHeader = Declaration.Invoices.AddNew();
			AssertEquals(false, Declaration.IsAnyZeroRated);

			var testLine = (JobComInvoiceLine)(testHeader.InvoiceLines.AddNew());
			AssertEquals(true, Declaration.IsAnyZeroRated);

			testLine.JI_IsZeroRatedDuty = YesNoList.Codes.No;
			testLine.JI_IsZeroRatedGST = YesNoList.Codes.No;
			testLine.JI_IsZeroRatedExcise = YesNoList.Codes.No;
			testLine.JI_IsZeroRatedLevies = YesNoList.Codes.No;
			AssertEquals(false, Declaration.IsAnyZeroRated);

			testLine.JI_IsZeroRatedDuty = string.Empty;
			testLine.JI_IsZeroRatedGST = string.Empty;
			testLine.JI_IsZeroRatedExcise = string.Empty;
			testLine.JI_IsZeroRatedLevies = string.Empty;
			AssertEquals(true, Declaration.IsAnyZeroRated);

			Declaration.JE_IsZeroRatedAll = YesNoList.Codes.No;
			testLine.JI_IsZeroRatedDuty = string.Empty;
			testLine.JI_IsZeroRatedGST = string.Empty;
			testLine.JI_IsZeroRatedExcise = string.Empty;
			testLine.JI_IsZeroRatedLevies = string.Empty;
			AssertEquals(false, Declaration.IsAnyZeroRated);

			testHeader.JZ_IsZeroRatedGST = YesNoList.Codes.Yes;
			AssertEquals(true, Declaration.IsAnyZeroRated);

			testLine.JI_IsZeroRatedGST = YesNoList.Codes.No;
			AssertEquals(false, Declaration.IsAnyZeroRated);
		}

		public void TestSaveCancelledDeclarationWhenAllEntriesAreCancelled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCC;
			AssertEquals("Pre-condition: Entry Header count", 1, declaration.CustomsEntryHeaders.Count);
			var savedHeaderPK = declaration.CusEntryHeader.PK;

			var entryHeader1 = declaration.CusEntryHeader;
			entryHeader1.EntryNumber = "11111111";
			entryHeader1.CH_IsEntryCancelled = true;

			var entryHeader2 = declaration.CusEntryHeader;
			entryHeader2.EntryNumber = "22222222";
			entryHeader2.CH_IsEntryCancelled = true;
			Factory.Save();

			AssertEquals("No new entry header is saved", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("No new entry header is created", savedHeaderPK, declaration.CusEntryHeader.PK);
		}

		public void TestRetrieverCancelledEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.EntryNumber = "111111";
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCC;
			entryHeader1.CH_IsActive = false;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			declaration = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Should retrieve the cancelled entry header for TSW cancellation", declaration.CusEntryHeader.PK, entryHeader1.PK);
		}

		#endregion

		#region ECI WriteOff Tests
		public void TestECIManifestValue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			declaration.JE_ECI_InvoiceAmountInfo.Value = new ZDecimal(100.00m);
			AssertEquals("declaration.JE_ECI_InvoiceAmount", 100.00m, declaration.JE_ECI_InvoiceAmountInfo.Value);
		}

		public void TestECIManifestCurrency()
		{
			RefCurrency currencyAUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			declaration.JE_ECI_InvoiceCurrency = currencyAUD.PK;
			AssertEquals("declaration.JE_ECI_InvoiceCurrency", currencyAUD.PK, declaration.JE_ECI_InvoiceCurrency);
		}

		public void TestECIManifestValueAndCurrency()
		{
			RefCurrency currencyAUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			declaration.JE_ECI_InvoiceAmount = 100.00m;
			declaration.JE_ECI_InvoiceCurrency = currencyAUD.PK;
			AssertEquals("declaration.JE_ECI_InvoiceAmount", 100.00m, declaration.JE_ECI_InvoiceAmount);
			AssertEquals("declaration.JE_ECI_InvoiceCurrency", currencyAUD.PK, declaration.JE_ECI_InvoiceCurrency);
		}

		public void TestECILinkToManifest()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			CusEntryHeader entryHeader = Declaration.CusEntryHeader;
			Factory.Save();

			ZString savedDeclarationReference = Declaration.JE_DeclarationReference;
			ZString manifestReference = NumberFountains.ECIManifestReferencePrefix + "01010101";

			ECIWriteOff.Manifesting.CusEntryHeader manifestEntryHeader = Factory.New<ECIWriteOff.Manifesting.CusEntryHeader>();
			manifestEntryHeader.CH_BGMReference = manifestReference;
			Declaration.LinkToManifest(manifestEntryHeader, 1);

			AssertEquals("Declaration.JE_DeclarationReference", manifestReference + "-1", Declaration.JE_DeclarationReference);
			AssertEquals("Declaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ManifestedReadyToSend, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, Declaration.CusEntryHeader.PK);
			AssertEquals("Declaration.CustomsEntryHeaders.Contains(EntryHeader)", true, Declaration.CustomsEntryHeaders.Contains(entryHeader));
			AssertEquals("Declaration.CustomsEntryHeaders.Contains(ManifestEntryHeader)", true, Declaration.CustomsEntryHeaders.Contains(manifestEntryHeader));

			AssertEquals("EntryHeader.CH_BGMReference", savedDeclarationReference, entryHeader.CH_BGMReference);
			AssertEquals("EntryHeader.CH_JE", Declaration.PK, entryHeader.CH_JE);
			AssertEquals("EntryHeader.IsActive", false, entryHeader.IsActive);

			AssertEquals("ManifestEntryHeader.CH_BGMReference", manifestReference, manifestEntryHeader.CH_BGMReference);
			AssertEquals("ManifestEntryHeader.CH_JE", ZGuid.Empty, manifestEntryHeader.CH_JE);
			AssertEquals("ManifestEntryHeader.IsActive", true, manifestEntryHeader.IsActive);
		}

		public void TestECIShipmentLinkedManifestDeclarationDoesntGetTheDeclarationReferenceBlownAwayOnSaveForOldReferences()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			ZString manifestDeclarationReference = NumberFountains.OldECIManifestReferencePrefix + "00001000-1";
			Declaration.JE_DeclarationReference = manifestDeclarationReference;
			Factory.Save();
			AssertEquals("Declaration.JE_DeclarationReference", manifestDeclarationReference, Declaration.JE_DeclarationReference);
		}

		public void TestECIShipmentLinkedManifestDeclarationDoesntGetTheDeclarationReferenceBlownAwayOnSaveForNewReferences()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			ZString manifestDeclarationReference = NumberFountains.ECIManifestReferencePrefix + "00001000-1";
			Declaration.JE_DeclarationReference = manifestDeclarationReference;
			Factory.Save();
			AssertEquals("Declaration.JE_DeclarationReference", manifestDeclarationReference, Declaration.JE_DeclarationReference);
		}

		public void TestECIManifestReferenceWithOldReference()
		{
			string eciManifestReferencePrefix = NumberFountains.OldECIManifestReferencePrefix;
			AssertECIManifestReference(eciManifestReferencePrefix);
		}

		public void TestECIManifestReferenceWithNewReference()
		{
			string eciManifestReferencePrefix = NumberFountains.ECIManifestReferencePrefix;
			AssertECIManifestReference(eciManifestReferencePrefix);
		}

		void AssertECIManifestReference(string eciManifestReferencePrefix)
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Default: Declaration.ManifestReference", "", Declaration.ECIManifestReference);
			Declaration.JE_DeclarationReference = eciManifestReferencePrefix + "00001001-1";
			AssertEquals("Default: Declaration.ManifestReference", eciManifestReferencePrefix + "00001001", Declaration.ECIManifestReference);
			Declaration.JE_DeclarationReference = eciManifestReferencePrefix + "00001002-2";
			AssertEquals("Default: Declaration.ManifestReference", eciManifestReferencePrefix + "00001002", Declaration.ECIManifestReference);
		}

		public void TestECIManifestLineNumberWithOldReference()
		{
			string eciManifestReferencePrefix = NumberFountains.OldECIManifestReferencePrefix;
			AssertECIManifestLineNumber(eciManifestReferencePrefix);
		}

		public void TestECIManifestLineNumberWithNewReference()
		{
			string eciManifestReferencePrefix = NumberFountains.ECIManifestReferencePrefix;
			AssertECIManifestLineNumber(eciManifestReferencePrefix);
		}

		void AssertECIManifestLineNumber(string eciManifestReferencePrefix)
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Default: Declaration.ManifestLineNumber", "1", Declaration.ECIManifestLineNumber);
			Declaration.JE_DeclarationReference = eciManifestReferencePrefix + "00001000-1";
			AssertEquals("Default: Declaration.ManifestLineNumber", "1", Declaration.ECIManifestLineNumber);
			Declaration.JE_DeclarationReference = eciManifestReferencePrefix + "00001000-2";
			AssertEquals("Default: Declaration.ManifestLineNumber", "2", Declaration.ECIManifestLineNumber);
		}

		public void TestIsECIManifestDeclarationReferenceWithOldReference()
		{
			string eciManifestReferencePrefix = NumberFountains.OldECIManifestReferencePrefix;
			AssertIsECIManifestDeclarationReference(eciManifestReferencePrefix);
		}

		public void TestIsECIManifestDeclarationReferenceWithNewReference()
		{
			string eciManifestReferencePrefix = NumberFountains.ECIManifestReferencePrefix;
			AssertIsECIManifestDeclarationReference(eciManifestReferencePrefix);
		}

		void AssertIsECIManifestDeclarationReference(string eciManifestReferencePrefix)
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Default: Declaration.IsManifestDeclaration", false, Declaration.IsECIManifestDeclarationReference);
			Declaration.JE_DeclarationReference = eciManifestReferencePrefix + "00001000-1";
			AssertEquals("Declaration.IsManifestDeclaration", true, Declaration.IsECIManifestDeclarationReference);
		}

		public void TestECILastStatusIsImpediment()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			AssertEquals(true, Declaration.LastCustomsStatusIsImpediment);
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			AssertEquals(false, Declaration.LastCustomsStatusIsImpediment);
		}

		public void TestECITemplateCopySetsEntryStatusCorrectlyAndResetsDeclarationNumber()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.DeclarationNumber = "12345678";
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			Declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestInError;
			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);
			Declaration.DeclarationNumber = "12345678";
			JobDeclaration copiedDeclaration = Declaration.TemplateCopy();
			AssertEquals("JE_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, copiedDeclaration.JE_EntryStatus);
			AssertEquals("JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.NoStatusReported, copiedDeclaration.JE_ECI_LastResponseStatus);
			AssertEquals("CusEntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, copiedDeclaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EDITransmitDate", ZDateTime.Empty, copiedDeclaration.JE_EDITransmitDate);
			AssertEquals("DeclarationNumber", "", copiedDeclaration.DeclarationNumber);
		}

		public void TestECIResetToOriginal()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);
			Declaration.DeclarationNumber = "12345678";
			Declaration.CusEntryHeader.Messages.AddNew();

			Declaration.ResetToOriginal();

			AssertEquals("JE_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.NoStatusReported, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("CusEntryHeader.Messages.Count", 0, Declaration.CusEntryHeader.Messages.Count);
			AssertEquals("JE_EDITransmitDate.IsEmpty", true, Declaration.JE_EDITransmitDate.IsEmpty);
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, Declaration.JE_EntrySubmittedDate.IsEmpty);
			AssertEquals("DeclarationNumber", ZString.Empty, Declaration.DeclarationNumber);
		}

		public void TestCREResetToOriginal()
		{
			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			Declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			Declaration.JE_EDITransmitDate = new ZDateTime(2013, 05, 24);
			Declaration.DeclarationNumber = "12345678";
			Declaration.CusEntryHeader.Messages.AddNew();

			Declaration.ResetToOriginal();

			AssertEquals("JE_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.NoStatusReported, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("CusEntryHeader.Messages.Count", 0, Declaration.CusEntryHeader.Messages.Count);
			AssertEquals("JE_EDITransmitDate.IsEmpty", true, Declaration.JE_EDITransmitDate.IsEmpty);
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, Declaration.JE_EntrySubmittedDate.IsEmpty);
			AssertEquals("DeclarationNumber", ZString.Empty, Declaration.DeclarationNumber);
		}

		public void TestECISetDefaultValues()
		{
			ZGuid localCurrency = JobDeclaration.GetLocalCurrency() != null ? JobDeclaration.GetLocalCurrency().PK : ZGuid.Empty;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("JE_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.NoStatusReported, Declaration.JE_ECI_LastResponseStatus);
			AssertEquals("Message style", JobMessageSubTypeList.Codes.WriteOff, Declaration.JE_MessageSubType);
			AssertEquals("Currency", localCurrency, Declaration.JE_ECI_InvoiceCurrency);
		}

		public void TestECIIDocManagerSupportIsNotOverridden()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.DocManagerInfo.DocManagerCode", "DEC", Declaration.DocManagerInfo.DocManagerCode);
		}

		public void TestIDocManagerSupport()
		{
			var standaloneDec = Factory.New<JobDeclaration>();
			standaloneDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			standaloneDec.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			standaloneDec.GetDocManagerInfo();

			var shipment = Factory.New<CommonShipment>();
			var shipmentDec = Factory.New<JobDeclaration>();
			shipmentDec.JE_JS = shipment.PK;
			shipmentDec.GetDocManagerInfo();
			Assert("DocManager for declaration attached to shipment should have access to both objects eDocs", shipmentDec.eDocsForSelection.Count > standaloneDec.eDocsForSelection.Count);
		}

		public void TestECIILandedCostHeader()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("ILandedCostHeader.IsLCSupported", false, ((ILandedCostHeader)Declaration).IsLCSupported);
			AssertEquals("ILandedCostHeader.MessageShownWhenLCIsNotSupported", "Landed Costing is not supported for ECI Writeoffs.", ((ILandedCostHeader)Declaration).MessageShownWhenLCIsNotSupported);
		}

		public void TestECIClonedObjectDoesntGetExceptionOnAccessingInvoiceAmount()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ECI_InvoiceAmount = 12.00m;
			Factory.Save();
			JobDeclaration copiedDec = Declaration.TemplateCopy();
			AssertEquals(Declaration.JE_ECI_InvoiceAmount, copiedDec.JE_ECI_InvoiceAmount);
		}

		public void TestECIProxiedValuesActuallySaveAndAreReloadable()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ECI_InvoiceAmount = 30.00m;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration declarationAfterSaveAndLoad = newFactory.Load<JobDeclaration>(Declaration.PK);

			AssertEquals(30.00m, declarationAfterSaveAndLoad.JE_ECI_InvoiceAmount);
		}

		public void TestECIMergeManager()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Requires merge", false, Declaration.MergeManager.RequiresMerge);
			AssertEquals("Supports auto-merge", false, Declaration.MergeManager.SupportsAutoMerge);
		}

		public void TestECIGettingInvoiceDetailDoesntCreateAnInvoiceHeader()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.JE_ECI_InvoiceAmount", 0.00m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("Declaration.JE_ECI_InvoiceCurrency should default to NZD", RefCurrency.LoadFromCurrencyCode(Factory, "NZD").PK, Declaration.JE_ECI_InvoiceCurrency);
			AssertEquals("Declaration.Invoices.Count", 0, Declaration.Invoices.Count);
		}

		public void TestECIInvoiceDetailsArePersistedProperly()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			var aUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			Declaration.JE_ECI_InvoiceAmount = 1000;
			Declaration.JE_ECI_InvoiceCurrency = aUD.PK;
			AssertEquals("Declaration.Invoices.Count", 1, Declaration.Invoices.Count);
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = secondFactory.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("InvoiceAmount", 1000m, loadedDeclaration.JE_ECI_InvoiceAmount);
			AssertEquals("InvoiceCurrency", aUD.PK, loadedDeclaration.JE_ECI_InvoiceCurrency);
			AssertEquals("LoadedDeclaration.Invoices.Count", 1, loadedDeclaration.Invoices.Count);
		}

		public void TestECISettingInvoiceDetails()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			var aUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			Declaration.JE_ECI_InvoiceAmount = 1000;
			Declaration.JE_ECI_InvoiceCurrency = aUD.PK;

			AssertEquals("InvoiceAmount", 1000m, Declaration.JE_ECI_InvoiceAmount);
			AssertEquals("InvoiceCurrency", aUD.PK, Declaration.JE_ECI_InvoiceCurrency);
		}

		#endregion

		#region Formal Entry Tests
		public void TestPermitCodesCanBeSavedAndLoadedWhenThereAreMultiplePermitsOfTheSameType()
		{
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380775E");
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380610D");
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380624D");
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380546J");
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380535C");
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380630J");
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380689J");
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380602C");
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380566C");
			Declaration.PermitCodes.AddNew(PermitCodeList.Codes.EnvironmentalProtectionAuthority, "00100380550G");
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = secondFactory.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("LoadedDeclaration.PermitCodes.Count", 10, loadedDeclaration.PermitCodes.Count);
		}

		public void TestMergeAndSaveIfNotMergedAlreadyReturningErrors()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertContains("Declaration.MergeAndSaveIfNotMergedAlreadyReturningErrors", Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, declaration.MergeAndSaveIfNotMergedAlreadyReturningErrors());
		}

		public void TestTemplateCopySetsEntryStatusCorrectlyAndResetsDeclarationNumber()
		{
			Declaration.DeclarationNumber = "12345678";
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);
			Declaration.DeclarationNumber = "12345678";
			JobDeclaration copiedDeclaration = Declaration.TemplateCopy();
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, copiedDeclaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, copiedDeclaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Declaration.JE_EDITransmitDate", ZDateTime.Empty, copiedDeclaration.JE_EDITransmitDate);
			AssertEquals("Declaration.DeclarationNumber", "", copiedDeclaration.DeclarationNumber);
		}

		public void TestResetToOriginal()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);
			Declaration.DeclarationNumber = "12345678";
			NZCMessage message = Declaration.CusEntryHeader.Messages.AddNew();
			message.EM_Status = NZCMessage.Status.Queued;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;

			Declaration.ResetToOriginal();

			AssertEquals("JE_EntryStatus should be 'Not Sent To Customs'", FormalEntryStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, Declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.Messages.Count", 0, Declaration.CusEntryHeader.Messages.Count);
			AssertEquals("Declaration.JE_EDITransmitDate.IsEmpty", true, Declaration.JE_EDITransmitDate.IsEmpty);
			AssertEquals("Declaration.JE_EntrySubmittedDate.IsEmpty", true, Declaration.JE_EntrySubmittedDate.IsEmpty);
			AssertEquals("Declaration.DeclarationNumber", ZString.Empty, Declaration.DeclarationNumber);
			AssertEquals("Message.EM_Status", NZCMessage.Status.Cancelled, message.EM_Status);
		}

		public void TestIsQueuedForSending()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.IsQueuedForSending", false, Declaration.IsQueuedForSending);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			AssertEquals("Declaration.IsQueuedForSending", false, Declaration.IsQueuedForSending);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			AssertEquals("Declaration.IsQueuedForSending", false, Declaration.IsQueuedForSending);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			AssertEquals("Declaration.IsQueuedForSending", true, Declaration.IsQueuedForSending);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.IsQueuedForSending", false, Declaration.IsQueuedForSending);
		}

		public void TestCancelQueuedMessagesAndResetEDITransmitDate()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_DateOfArrival = Declaration.CachedTodaysDate.AddDays(1);
			Declaration.JE_EDITransmitDate = Declaration.CachedTodaysDate.AddDays(1);
			CusEntryHeader entryHeader = Declaration.CusEntryHeader;
			NZCMessage message = entryHeader.Messages.AddNew();
			message.EM_Status = NZCMessage.Status.Queued;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			AssertEquals("Precondition: Declaration.IsQueuedForSending", false, Declaration.IsQueuedForSending);
			Declaration.CancelQueuedMessagesAndResetEDITransmitDate();
			AssertEquals("Message.EM_Status", NZCMessage.Status.Queued, message.EM_Status);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("entryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.PK = Declaration.CusEntryHeader.PK", entryHeader.PK, Declaration.CusEntryHeader.PK);

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			AssertEquals("Precondition: Declaration.IsQueuedForSending", true, Declaration.IsQueuedForSending);
			AssertEquals("Declaration.JE_EDITransmitDate", Declaration.CachedTodaysDate.AddDays(1), Declaration.JE_EDITransmitDate);
			Declaration.CancelQueuedMessagesAndResetEDITransmitDate();
			AssertEquals("Message.EM_Status", NZCMessage.Status.Cancelled, message.EM_Status);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("entryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.PK = Declaration.CusEntryHeader.PK", entryHeader.PK, Declaration.CusEntryHeader.PK);
			AssertEquals("For import, we will not reset Declaration.JE_EDITransmitDate", Declaration.CachedTodaysDate.AddDays(1), Declaration.JE_EDITransmitDate);
		}

		public void TestLastStatusIsImpediment()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryInError;
			AssertEquals(true, Declaration.LastCustomsStatusIsImpediment);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			AssertEquals(false, Declaration.LastCustomsStatusIsImpediment);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.BrokerDeferred, Declaration.JE_PaymentMethod);
			AssertEquals("Declaration.JE_ApplicationCode", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);

			var tswDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, tswDeclaration.JE_EntryStatus);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.BrokerDeferred, tswDeclaration.JE_PaymentMethod);
			AssertEquals("Declaration.JE_ApplicationCode should be defaulted to TSW for Export Job when TSW EX1 message is active.", JobApplicationCodeList.Codes.TSW, tswDeclaration.JE_ApplicationCode);
		}

		public void TestSetDefaultValuesWhenTSW()
		{
			var legacyDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, legacyDeclaration.JE_MessageSubType);
			AssertEquals("JE_PaymentMethod", PaymentMethodList.Codes.BrokerDeferred, legacyDeclaration.JE_PaymentMethod);
			AssertEquals("JE_ApplicationCode", JobApplicationCodeList.Codes.TSW, legacyDeclaration.JE_ApplicationCode);

			var tswDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_MessageSubType", JobMessageSubTypeList.Codes.Normal, tswDeclaration.JE_MessageSubType);
			AssertEquals("JE_PaymentMethod", PaymentMethodList.Codes.BrokerDeferred, tswDeclaration.JE_PaymentMethod);
			AssertEquals("JE_ApplicationCode", JobApplicationCodeList.Codes.TSW, tswDeclaration.JE_ApplicationCode);
			AssertEquals("JE_TransactionNature", NatureOfTransactionList.Codes.N10, tswDeclaration.JE_TransactionNature);
		}

		public void TestMappedTSWMessageSubType()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("Non TSW Declaration", MessageTypeList.Codes.I10, Declaration.MappedTSWMessageSubType);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("TSW Declaration", MessageTypeList.Codes.I10, Declaration.MappedTSWMessageSubType);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("TSW Declaration", MessageTypeList.Codes.E40, Declaration.MappedTSWMessageSubType);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			AssertEquals("TSW Declaration", MessageTypeList.Codes.I52, Declaration.MappedTSWMessageSubType);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertEquals("TSW Declaration completion entry needs to be sent as a new original entry", MessageTypeList.Codes.I10, Declaration.MappedTSWMessageSubType);
		}

		public void TestTSWActiveDefaultValues()
		{
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.BrokerDeferred, Declaration.JE_PaymentMethod);
			AssertEquals("Message style", MessageSubTypeCombinedList.Codes.Normal, Declaration.JE_MessageSubType);
			AssertEquals("Declaration.JE_ApplicationCode", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);
		}

		public void TestMergeByAndPaymentMethodDefaultFromClientWhenClientChanges()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_FullName = "Importer";
			importer.OH_RL_NKClosestPort = "NZAKL";
			importer.MiscServ.OM_IMPaymentMethod = PaymentMethodList.Codes.ClientDeferred;
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways;

			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "Supplier";
			supplier.OH_RL_NKClosestPort = "NZAKL";
			supplier.MiscServ.OM_IMPaymentMethod = PaymentMethodList.Codes.CashPaidByClient;
			supplier.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription;

			Declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.ClientDeferred, Declaration.JE_PaymentMethod);
			AssertEquals("Declaration.JE_MergeBy", OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways, Declaration.JE_MergeBy);

			Declaration.JE_OH_Importer = ZGuid.Empty;
			Declaration.JE_OH_Supplier = supplier.PK;

			AssertEquals("Declaration.JE_PaymentMethod", PaymentMethodList.Codes.CashPaidByClient, Declaration.JE_PaymentMethod);
			AssertEquals("Declaration.JE_MergeBy", OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription, Declaration.JE_MergeBy);
		}

		public override void TestILandedCostHeader()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_EntrySubmittedDate = ZDateTime.Today;

			testDec.JE_EntryStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			AssertEquals("IsLCRunnableState", false, ((ILandedCostHeader)testDec).IsJobInLCRunnableState);

			testDec.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			AssertEquals("IsLCRunnableState", true, ((ILandedCostHeader)testDec).IsJobInLCRunnableState);

			testDec.JE_EntryStatus = FormalEntryStatusList.Codes.AdjustmentAccepted;
			AssertEquals("IsLCRunnableState", true, ((ILandedCostHeader)testDec).IsJobInLCRunnableState);

			testDec.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			AssertEquals("IsLCRunnableState", true, ((ILandedCostHeader)testDec).IsJobInLCRunnableState);

			testDec.JE_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
			AssertEquals("IsLCRunnableState", true, ((ILandedCostHeader)testDec).IsJobInLCRunnableState);

			testDec.JE_EntryStatus = FormalEntryStatusList.Codes.EntryInError;
			AssertEquals("IsLCRunnableState", false, ((ILandedCostHeader)testDec).IsJobInLCRunnableState);

			testDec.JE_EntryStatus = FormalEntryStatusList.Codes.EntryRestored;
			AssertEquals("IsLCRunnableState", true, ((ILandedCostHeader)testDec).IsJobInLCRunnableState);
		}

		public void TestILandedCostHeaderNZ()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();

			FormalEntry.CusEntryHeader entryHeader = (FormalEntry.CusEntryHeader)testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryFeeAmount = 10m;
			entryHeader.EntryFeeGST = 1.25m;

			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.ACCFuelLevyAmount = 10m;
			entryLine1.ALACLevyAmount = 20m;
			entryLine1.DutyAmount = 30m;
			entryLine1.HERALevyAmount = 40m;
			entryLine1.AntiDumpingDutyAmount = 50m;
			entryLine1.CountervailingDutyAmount = 60m;
			entryLine1.SyntheticGreenhouseGasesLevyAmount = 88;

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.ACCFuelLevyAmount = 11m;
			entryLine2.ALACLevyAmount = 21m;
			entryLine2.DutyAmount = 31m;
			entryLine2.HERALevyAmount = 41m;
			entryLine2.AntiDumpingDutyAmount = 51m;
			entryLine2.CountervailingDutyAmount = 61m;
			entryLine2.SyntheticGreenhouseGasesLevyAmount = 99;

			DutyTaxEntryFee total = ((ILandedCostHeader)testDec).TotalDutyTaxEntryFeeItems;
			AssertEquals("TotalEntryFee", 10m, total["ENT"]);
			AssertEquals("TotalDuty", 61m, total["TDT"]);
			AssertEquals("TotalExcise", 0m, total["EXC"]);
			AssertEquals("TotalOtherOrFlat Duty", 409m, total["OTH"]);
			AssertEquals("TotalSpecialTax1", 41m, total["ST1"]);
			AssertEquals("TotalSpecialTax2", 81m, total["ST2"]);
			AssertEquals("TotalSpecialTax3", 21m, total["ST3"]);
		}

		public void TestDeliveryAuthority()
		{
			OrgHeader testOrgHeader1 = OrgHeader.New(Factory);
			OrgHeader testOrgHeader2 = OrgHeader.New(Factory);

			AssertEquals(null, Declaration.NotifyParty);
			Declaration.JE_OH_NotifyParty = testOrgHeader1.PK;
			AssertEquals(testOrgHeader1, Declaration.NotifyParty);
			Declaration.JE_OH_NotifyParty = testOrgHeader2.PK;
			AssertEquals(testOrgHeader2, Declaration.NotifyParty);
			Declaration.JE_OH_NotifyParty = ZGuid.Empty;
			AssertEquals(null, Declaration.NotifyParty);
		}

		public void TestJE_MessageSubTypeDefaultValue()
		{
			AssertEquals("Message style", JobMessageSubTypeList.Codes.Normal, Declaration.JE_MessageSubType);
		}

		public void TestCusEntryHeaderCollection()
		{
			CusEntryHeader entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			AssertNotNull(entryHeader);
		}

		public void TestCusEntryHeaderIsNotNull()
		{
			AssertNotNull(Declaration.CusEntryHeader);
		}

		public void TestCusEntryHeaderWithNonPersistentDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CusEntryHeader;

			Assert("Should create a normal CusEntryHeader in the current factory and memory.", !entryHeader.IsNull);

			declaration = Factory.New<JobDeclaration>();
			declaration.MakeNonPersistent();

			entryHeader = declaration.CusEntryHeader;
			Assert("Should create a Null CusEntryHeader in the current factory but not null reference in memory.", entryHeader.IsNull);
			AssertNoExceptionThrown("Should create a Null CusEntryHeader in the current factory but not null reference in memory.", () => _ = entryHeader.MergedLines);
		}

		public void TestShipmentSyncroniserGetsRightType()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Declaration.JE_JS = shipment.PK;
			AssertEquals("Declaration.ShipmentSynchroniser is JobDeclarationSynchroniser", true, Declaration.ShipmentSynchroniser is JobDeclarationSynchroniser);
		}

		public void TestShipmentsHouseBillsCollectionSynchronised()
		{
			AssertEquals("PreCondition:House bill is none", 0, Declaration.Bills.Count);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB1";
			ForwardingShipment sub1 = consol.Shipments.AddNew();
			sub1.JS_JS_ColoadMasterShipment = shipment.PK;
			sub1.JS_HouseBill = "11111";
			ForwardingShipment sub2 = consol.Shipments.AddNew();
			sub2.JS_JS_ColoadMasterShipment = shipment.PK;
			sub2.JS_HouseBill = "22222";
			Declaration.JE_JS = shipment.PK;

			JobDeclarationSynchroniser sync = new JobDeclarationSynchroniser(Declaration);
			sync.Synchronise(new SynchroniseEventArgs(Enterprise.Customs.Business.SynchroniseAction.Force));
			AssertEquals("House bill added", 1, Declaration.Bills.Count);
		}

		public void TestDefaultProcessingPortForImport()
		{
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("Processing port is defaulted", "NZAKL", Declaration.JE_RL_NKProcessingPort);
		}

		public void TestDefaultProcessingPortForExport()
		{
			Declaration.JE_MessageType = "EXP";
			Declaration.JE_RL_NKPortOfLoading = "NZAKL";
			AssertEquals("Processing port is defaulted", "NZAKL", Declaration.JE_RL_NKProcessingPort);
		}

		public void TestProcessingPortVisible()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("Processing port is visible", true, Declaration.ProcessingPortVisible);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("Processing port is not visible", false, Declaration.ProcessingPortVisible);
		}

		public void TestMerge()
		{
			var decCreator = new TestFormalEntryCreator(Declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(Declaration.SaveHandlingSaveExceptions());
			AssertEquals("Should be no Merged Lines on a fresh Declaration", 0, Declaration.CusEntryHeader.MergedLines.Count);

			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be 1 Merged Line after merge", 1, Declaration.CusEntryHeader.MergedLines.Count);
			Declaration.SaveHandlingSaveExceptions();
			AssertEquals("Should be 1 Merged Line after merge and save", 1, Declaration.CusEntryHeader.MergedLines.Count);

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.JE_DateOfArrival = Declaration.JE_DateOfArrival.AddDays(1);
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Declaration.SaveHandlingSaveExceptions();
			AssertEquals("Should still be 1 Merged Line after merge and save when Status is 'Sent to Customs'", 1, Declaration.CusEntryHeader.MergedLines.Count);

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Declaration.JE_DateOfArrival = Declaration.JE_DateOfArrival.AddDays(1);
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Declaration.SaveHandlingSaveExceptions();
			AssertEquals("Should be 1 Merged Line after changing the Declaration and saving it.", 1, Declaration.CusEntryHeader.MergedLines.Count);
		}

		public void TestNoMergeForECIEvenWhenHasLines()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Should be no Merged Lines on a fresh Declaration", 0, Declaration.CusEntryHeader.MergedLines.Count);
			Assert(Declaration.SaveHandlingSaveExceptions());
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be no Merged Lines on a standard WriteOff Declaration", 0, Declaration.CusEntryHeader.MergedLines.Count);

			var invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 200m;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should be no Merged Lines on a TSW WriteOff Declaration with invoice lines", 0, Declaration.CusEntryHeader.MergedLines.Count);
		}

		public void TestPackageCountFromPackagesCollection()
		{
			AssertEquals("Should be no Packages on a blank Declaration", 0, Declaration.PackageCountFromPackagesCollection);

			Package package1 = Declaration.Packages.AddNew();
			package1.CW_PackQty = 10;
			AssertEquals("Should be Packages now", 10, Declaration.PackageCountFromPackagesCollection);

			Package package2 = Declaration.Packages.AddNew();
			package2.CW_PackQty = 20;
			AssertEquals("Should be more Packages now", 30, Declaration.PackageCountFromPackagesCollection);
		}

		public void TestHasCancellationPending()
		{
			AssertEquals("Precondition: Declaration.HasCancellationPending", false, Declaration.HasCancellationPending);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCP;
			AssertEquals("Declaration.HasCancellationPending", true, Declaration.HasCancellationPending);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCA;
			AssertEquals("Declaration.HasCancellationPending", true, Declaration.HasCancellationPending);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCC;
			AssertEquals("Declaration.HasCancellationPending", false, Declaration.HasCancellationPending);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;
			AssertEquals("Declaration.HasCancellationPending", false, Declaration.HasCancellationPending);
		}

		public void TestIsTSWCustomsCleared()
		{
			AssertEquals("Precondition: Declaration.IsTSWCustomsCleared", false, Declaration.IsTSWCustomsCleared);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCP;
			AssertEquals("IsTSWCustomsCleared - no", false, Declaration.IsTSWCustomsCleared);
			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.CCI;
			AssertEquals("IsTSWCustomsCleared - yes", true, Declaration.IsTSWCustomsCleared);
			Declaration.JE_TSWCombinedStatus = "";
			AssertEquals("IsTSWCustomsCleared - no, empty status", false, Declaration.IsTSWCustomsCleared);
		}

		public void TestIsInternationalTranshipmentApproved()
		{
			AssertEquals("Precondition: Declaration.IsInternationalTranshipmentApproved", false, Declaration.IsInternationalTranshipmentApproved);
			Declaration.JE_EntryStatus = ConsignmentGoodsStatusList.Codes.InternationalTranshipmentApproved;
			AssertEquals("Declaration.IsInternationalTranshipmentApproved", true, Declaration.IsInternationalTranshipmentApproved);
		}

		public void TestIsExcise()
		{
			AssertEquals("Precondition: Declaration.IsExcise", false, Declaration.IsExcise);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals("Declaration.IsExcise", true, Declaration.IsExcise);
		}

		public void TestIsPeriodic()
		{
			AssertEquals("Precondition: Declaration.IsPeriodic", false, Declaration.IsPeriodic);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			AssertEquals("Declaration.IsPeriodic", true, Declaration.IsPeriodic);
		}

		public void TestIsPrimaryIndustriesImportDeclaration()
		{
			AssertEquals("Precondition: Declaration.IsPrimaryIndustriesImportDeclaration", false, Declaration.IsPrimaryIndustriesImportDeclaration);
			Declaration.JE_MessageSubType = MessageTypeList.Codes.IPI;
			AssertEquals("Declaration.IsPrimaryIndustriesImportDeclaration", true, Declaration.IsPrimaryIndustriesImportDeclaration);
		}

		public void TestIsTemporary()
		{
			AssertEquals("Precondition: Declaration.IsTemporary", false, Declaration.IsTemporary);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			AssertEquals("Declaration.IsTemporary", true, Declaration.IsTemporary);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("TSW Declaration - Is not Temporary dec", false, Declaration.IsTemporary);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			AssertEquals("TSW Declaration - Is Temporary dec", true, Declaration.IsTemporary);
		}

		public void TestIsSight()
		{
			AssertEquals("Precondition: Declaration.IsSight", false, Declaration.IsSight);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			AssertEquals("Declaration.IsSight", true, Declaration.IsSight);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("TSW Declaration - Is not Sight dec", false, Declaration.IsSight);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			AssertEquals("TSW Declaration - Is Sight dec", true, Declaration.IsSight);
		}

		public void TestIsBond()
		{
			AssertEquals("Precondition: Declaration.IsBond", false, Declaration.IsBond);
			Declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.NewZGuid();
			AssertEquals("Declaration.IsBond", true, Declaration.IsBond);
		}

		public void TestIsDutyDeminimus()
		{
			CusEntryLine entryLine = Declaration.CusEntryHeader.MergedLines.AddNew();
			entryLine.DutyAmount = 100.00m;
			entryLine.GSTAmount = 100.00m;
			AssertEquals("Precondition: Declaration.IsDutyDeminimus", false, Declaration.IsDutyDeminimus);
			entryLine.DutyAmount = 0.00m;
			entryLine.GSTAmount = 0.00m;
			AssertEquals("Declaration.IsDutyDeminimus", true, Declaration.IsDutyDeminimus);
		}

		public void TestIsPrivateImport()
		{
			AssertEquals("Precondition: Declaration.IsPrivateImport", false, Declaration.IsPrivateImport);
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PrivateImportTransactionFee, "DATAONE");
			AssertEquals("Declaration.IsPrivateImport", true, Declaration.IsPrivateImport);
		}

		public void TestIsDiplomatic()
		{
			AssertEquals("Precondition: Declaration.IsDiplomatic", false, Declaration.IsDiplomatic);
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.DiplomaticPrivilege, "");
			AssertEquals("Declaration.IsDiplomatic", true, Declaration.IsDiplomatic);
		}

		public void TestIsCompletion()
		{
			AssertEquals("Precondition: Declaration.IsCompletion", false, Declaration.IsCompletion);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertEquals("Declaration.IsCompletion", true, Declaration.IsCompletion);
		}

		public void TestIsWriteOffChangedToFormal()
		{
			AssertEquals("Precondition: Declaration.IsWriteOffChangedToFormal", false, Declaration.IsWriteOffChangedToFormal);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Formal export declaration", false, Declaration.IsWriteOffChangedToFormal);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Export write off declaration", false, Declaration.IsWriteOffChangedToFormal);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			ECIWriteOff.CusEntryHeader eCIHeader = (ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader;
			eCIHeader.Messages.AddNew();
			AssertEquals("Export write off declaration", false, Declaration.IsWriteOffChangedToFormal);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			FormalEntry.CusEntryHeader formalHeader = (FormalEntry.CusEntryHeader)Declaration.CusEntryHeader;
			formalHeader.Messages.AddNew();
			AssertEquals("Formal export declaration (non TSW) after write off declaration has been made", false, Declaration.IsWriteOffChangedToFormal);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			eCIHeader.EntryNumber = "67837754";
			AssertEquals("Formal TSW export declaration after write off declaration has been made", true, Declaration.IsWriteOffChangedToFormal);
		}

		public void TestIsFormalChangedToIPI()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var entryHeader = Declaration.CusEntryHeader;
			entryHeader.Messages.AddNew();
			entryHeader.EntryNumber = "75328491";

			Declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			PrimaryIndustriesCusEntryHeader iPIHeader = (PrimaryIndustriesCusEntryHeader)Declaration.GetAppropriateCusEntryHeaderIfExists();
			AssertNotNull("IPIHeader should have been created when approved formal declaration was changed to add an IPI entry", iPIHeader);
			iPIHeader.Messages.AddNew();

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			iPIHeader.EntryNumber = "67837754";
		}

		public void TestIsFormalChangedToIPIIsSaved()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Factory.Save();
			var entryHeader = declaration.CusEntryHeader;
			AssertEquals("entryHeader should be Formal Entry header", typeof(FormalEntry.CusEntryHeader), entryHeader.GetType());
			Assert("IsActive", entryHeader.IsActive);
			entryHeader.EntryNumber = "1122392";

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Factory.Save();
			AssertEquals("Dec should have Formal entry & IPI entry", 2, declaration.CustomsEntryHeaders.Count);

			var ipiEntryHeader = declaration.CustomsEntryHeaders[1];
			entryHeader = declaration.CusEntryHeader;
			AssertEquals("ipiEntryHeader should be Primary Industries Entry header", typeof(PrimaryIndustriesCusEntryHeader), ipiEntryHeader.GetType());
			AssertSame("ipiEntryHeader should now be the Active Entry header", ipiEntryHeader, entryHeader);
			Assert("IsActive", ipiEntryHeader.IsActive);
		}

		public void TestIsCRE()
		{
			AssertEquals("Precondition: Declaration.IsCRE", false, Declaration.IsCRE);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertEquals("Declaration.IsCRE", false, Declaration.IsCRE);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("Declaration.IsCRE", false, Declaration.IsCRE);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.IsCRE", false, Declaration.IsCRE);
		}

		public void TestApplicationCodeForExportWriteOff()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("JE_ApplicationCode", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("JE_ApplicationCode - default for write-off", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Factory.Save();

			var reloadedDeclaration = Factory.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("JE_ApplicationCode once saved", JobApplicationCodeList.Codes.CUS, reloadedDeclaration.JE_ApplicationCode);
		}

		public void TestApplicationCodeForImportWriteOff()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("JE_ApplicationCode", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("JE_ApplicationCode - default for write-off", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Factory.Save();

			var reloadedDeclaration = Factory.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("JE_ApplicationCode once saved", JobApplicationCodeList.Codes.CUS, reloadedDeclaration.JE_ApplicationCode);
		}

		public void TestJE_ApplicationCodeInfoReadOnly()
		{
			AssertEquals("JE_ApplicationCode should currently be accesible", true, Declaration.JE_ApplicationCodeInfo.ReadOnly);
			AssertEquals("Default value", JobApplicationCodeList.Codes.TSW, Declaration.JE_ApplicationCode);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			Declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			AssertEquals("JE_ApplicationCode should now be inaccesible", true, Declaration.JE_ApplicationCodeInfo.ReadOnly);

			Declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Declaration.DeclarationNumber = "59775527";
			AssertEquals("JE_ApplicationCode should now be inaccesible", true, Declaration.JE_ApplicationCodeInfo.ReadOnly);
		}

		public void TestJE_ApplicationCodeInfoReadOnly_ForInterface()
		{
			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID" };

			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("Builtin - JE_ApplicationCode should be ReadOnly", true, Factory.New<JobDeclaration>().JE_ApplicationCodeInfo.ReadOnly);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("BothBuiltInDefaulted - JE_ApplicationCode should not be ReadOnly when user can choose between TSW and Interfaced submission", false, Factory.New<JobDeclaration>().JE_ApplicationCodeInfo.ReadOnly);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("BothInterfaceDefaulted - JE_ApplicationCode should not be ReadOnly when user can choose between TSW and Interfaced submission", false, Factory.New<JobDeclaration>().JE_ApplicationCodeInfo.ReadOnly);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("Interfaced - JE_ApplicationCode should be ReadOnly", true, Factory.New<JobDeclaration>().JE_ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestIsExportedUnderSecureExportPartnershipScheme()
		{
			AssertEquals("Not under SEP scheme by default", false, Declaration.IsExportedUnderSecureExportPartnershipScheme);
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.SecureExportPartnership, "DATAONE");
			AssertEquals("Now under SEP scheme", true, Declaration.IsExportedUnderSecureExportPartnershipScheme);
		}

		public void TestIsSimplified()
		{
			AssertEquals("Precondition: Declaration.IsSimplified", false, Declaration.IsSimplified);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertEquals("Declaration.IsSimplified", true, Declaration.IsSimplified);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("TSW Declaration - Is not Simplified dec", false, Declaration.IsSimplified);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertEquals("TSW Declaration - Is Simplified dec", true, Declaration.IsSimplified);
		}

		public void TestIsNormal()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Assert("Declaration.IsNormal", Declaration.IsNormal);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Assert("TSW Declaration - Is not Normal dec", !Declaration.IsNormal);
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Assert("TSW Declaration - Is Normal dec", Declaration.IsNormal);
		}

		public void TestIsEntryStyleUnPayable()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			Assert("IsEntryStyleUnPayable is true when the entry style is SIM or NOR.", Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Assert("IsEntryStyleUnPayable is true when the entry style is SIM or NOR.", Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Assert(!Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			Assert(!Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Excise;
			Assert(!Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			Assert(!Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			Assert(!Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			Assert(!Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Assert(!Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = ZString.Empty;
			Assert(!Declaration.IsEntryStyleUnPayable);

			Declaration.JE_MessageSubType = "XXX";
			Assert(!Declaration.IsEntryStyleUnPayable);
		}

		public void TestVFDDeminimus()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			Declaration.JE_DateOfArrival = new ZDateTime(2000, 1, 1);
			AssertEquals("Not a valid date.", 0m, Declaration.VFDDeminimus);

			Declaration.JE_DateOfArrival = new ZDateTime(2019, 11, 30);
			AssertEquals("Valid", 400m, Declaration.VFDDeminimus);

			Declaration.JE_DateOfArrival = new ZDateTime(2019, 12, 1);
			AssertEquals("Valid", 1000m, Declaration.VFDDeminimus);

			Declaration.JE_DateOfArrival = new ZDateTime(2100, 1, 1);
			AssertEquals("Not a valid date.", 0m, Declaration.VFDDeminimus);
			ErrorReporter.Clear();
		}

		public void TestVFDDeminimusShouldBeCached()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			Declaration.JE_DateOfArrival = new ZDateTime(2020, 1, 1);
			var vfdDeminimus = Declaration.VFDDeminimus;
			var dbLoadCount = Factory.DatabaseLoadCount;
			vfdDeminimus = Declaration.VFDDeminimus;
			AssertEquals("No new db hit.", dbLoadCount, Factory.DatabaseLoadCount);
		}

		public void TestIsVFDUnderDeminimus()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			Declaration.JE_DateOfArrival = new ZDateTime(2019, 11, 30);

			AssertEquals("Precondition", 400m, Declaration.VFDDeminimus);

			Declaration.Invoices.DeleteAll();
			var header1 = Declaration.Invoices.AddNew();
			var header2 = Declaration.Invoices.AddNew();
			header1.JZ_InvoiceAmount = 200m;
			header1.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			var invoiceLine = header1.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6110300201K";
			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_InvoiceUQ = "NMB";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "NUM";
			invoiceLine.JI_CountryOfOrigin = "CN";

			header2.JZ_InvoiceAmount = 100m;
			header2.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			var invoiceLine2 = header2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6110300201K";
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_InvoiceUQ = "NMB";
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_CustomsUnitQty = "NUM";
			invoiceLine2.JI_CountryOfOrigin = "CN";

			AssertEquals("Precondition", 300m, Declaration.TotalFOBInLocalCurrency.Amount);
			Assert("$300 <= $400", Declaration.IsVFDUnderDeminimus);

			header2.JZ_InvoiceAmount = 200m;
			invoiceLine2.JI_LinePrice = 200m;
			AssertEquals("Precondition", 400m, Declaration.TotalFOBInLocalCurrency.Amount);
			Assert("$400 <= $400", Declaration.IsVFDUnderDeminimus);

			header2.JZ_InvoiceAmount = 300m;
			invoiceLine2.JI_LinePrice = 300m;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Precondition", 500m, Declaration.TotalFOBInLocalCurrency.Amount);
			Assert("$500 > $400", !Declaration.IsVFDUnderDeminimus);

			Declaration.JE_DateOfArrival = new ZDateTime(2019, 12, 1);
			AssertEquals("Precondition", 1000m, Declaration.VFDDeminimus);

			header2.JZ_InvoiceAmount = 800.49m;
			invoiceLine2.JI_LinePrice = 800.49m;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Precondition", 1000.49m, Declaration.TotalFOBInLocalCurrency.Amount);
			Assert(Declaration.IsVFDUnderDeminimus);

			header2.JZ_InvoiceAmount = 800.5m;
			invoiceLine2.JI_LinePrice = 800.5m;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Precondition", 1000.5m, Declaration.TotalFOBInLocalCurrency.Amount);
			Assert(!Declaration.IsVFDUnderDeminimus);
		}

		[TestDate(2020, 08, 15)]
		public void TestVFDUnderDeminimusBasedOnCusEntryLines()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var usdRate = usdCurrency.ExchangeRates.AddNew();
			usdRate.RE_ExRateType = "CUS";
			usdRate.RE_StartDate = new ZDateTime(2020, 08, 10);
			usdRate.RE_ExpiryDate = new ZDateTime(2020, 08, 16);
			usdRate.RE_SellRate = 0.64m;

			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DateOfArrival = new ZDateTime(2020, 08, 12);
			AssertEquals("Pre-condition", 1000m, declaration.VFDDeminimus);

			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "Inv-0001";
			invHeader.JZ_InvoiceAmount = 640m;
			invHeader.JZ_RX_NKInvoice_Currency = "USD";
			invHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;
			AssertEquals("Pre-condition - Invoice entered equals deminimus value", 1000m, declaration.TotalFOBInLocalCurrency.Amount);
			Assert("Entry prior to entry of lines has declaration correctly under Deminimus", declaration.IsVFDUnderDeminimus);

			var invoiceLine1 = invHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "6110300201K";
			invoiceLine1.JI_LinePrice = 280m;
			invoiceLine1.JI_InvoiceQuantity = 10m;
			invoiceLine1.JI_InvoiceUQ = "NMB";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsUnitQty = "NUM";
			invoiceLine1.JI_CountryOfOrigin = "CN";

			var invoiceLine2 = invHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6104630211G";
			invoiceLine2.JI_LinePrice = 360m;
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_InvoiceUQ = "NMB";
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_CustomsUnitQty = "NUM";
			invoiceLine2.JI_CountryOfOrigin = "CN";

			declaration.Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AssertEquals(2, declaration.ActiveEntryHeaders[0].MergedLines.Count);

			var entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals("Line 1 calculated entry value (Converted/rounded to whole dollars as required by NZCS)", 438m, entryLine1.CL_CustomsValue);

			var entryLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			AssertEquals("Line 2 calculated entry value (Converted/rounded to whole dollars as required by NZCS)", 563m, entryLine2.CL_CustomsValue);

			Assert("Entry after lines have been entered and merged now has the declaration correctly being above the Deminimus threshold", !declaration.IsVFDUnderDeminimus);
		}

		[TestDate(2020, 08, 15)]
		public void TestVFDUnderDeminimusMultiInvoicesAndLines()
		{
			TaxOrFeeTestHelper.SetUp(Factory);
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var usdRate = usdCurrency.ExchangeRates.AddNew();
			usdRate.RE_ExRateType = "CUS";
			usdRate.RE_StartDate = new ZDateTime(2020, 08, 10);
			usdRate.RE_ExpiryDate = new ZDateTime(2020, 08, 16);
			usdRate.RE_SellRate = 0.64m;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_DateOfArrival = new ZDateTime(2020, 08, 12);
			AssertEquals("Pre-condition", 1000m, Declaration.VFDDeminimus);

			var invHeader = Declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "Inv-0001";
			invHeader.JZ_InvoiceAmount = 330m;
			invHeader.JZ_RX_NKInvoice_Currency = "USD";
			invHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;

			var invHeader2 = Declaration.Invoices.AddNew();
			invHeader2.JZ_InvoiceNumber = "Inv-0002";
			invHeader2.JZ_InvoiceAmount = 310m;
			invHeader2.JZ_RX_NKInvoice_Currency = "USD";
			invHeader2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.Floating;
			AssertEquals("Pre-condition - Invoices entered equal deminimus value", 1000m, Declaration.TotalFOBInLocalCurrency.Amount);
			Assert("Entry prior to entry of lines has declaration correctly under Deminimus", Declaration.IsVFDUnderDeminimus);

			var invoiceLine1 = invHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "6110300201K";
			invoiceLine1.JI_LinePrice = 181m;
			invoiceLine1.JI_InvoiceQuantity = 10m;
			invoiceLine1.JI_InvoiceUQ = "NMB";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsUnitQty = "NUM";
			invoiceLine1.JI_CountryOfOrigin = "CN";

			var invoiceLine2 = invHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6104630211G";
			invoiceLine2.JI_LinePrice = 149m;
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_InvoiceUQ = "NMB";
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_CustomsUnitQty = "NUM";
			invoiceLine2.JI_CountryOfOrigin = "CN";

			var invoiceLine3 = invHeader2.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6110300201K";
			invoiceLine3.JI_LinePrice = 149m;
			invoiceLine3.JI_InvoiceQuantity = 10m;
			invoiceLine3.JI_InvoiceUQ = "NMB";
			invoiceLine3.JI_CustomsQuantity = 10m;
			invoiceLine3.JI_CustomsUnitQty = "NUM";
			invoiceLine3.JI_CountryOfOrigin = "CN";

			var invoiceLine4 = invHeader2.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "6104630211G";
			invoiceLine4.JI_LinePrice = 161m;
			invoiceLine4.JI_InvoiceQuantity = 10m;
			invoiceLine4.JI_InvoiceUQ = "NMB";
			invoiceLine4.JI_CustomsQuantity = 10m;
			invoiceLine4.JI_CustomsUnitQty = "NUM";
			invoiceLine4.JI_CountryOfOrigin = "CN";

			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, Declaration.ActiveEntryHeaders.Count);
			AssertEquals(4, Declaration.ActiveEntryHeaders[0].MergedLines.Count);

			var entryLine1 = Declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals("Line 1 calculated entry value (Converted/rounded to whole dollars as required by NZCS)", 283m, entryLine1.CL_CustomsValue);
			var entryLine2 = Declaration.ActiveEntryHeaders[0].MergedLines[1];
			AssertEquals("Line 2 calculated entry value (Converted/rounded to whole dollars as required by NZCS)", 233m, entryLine2.CL_CustomsValue);
			var entryLine3 = Declaration.ActiveEntryHeaders[0].MergedLines[2];
			AssertEquals("Line 3 calculated entry value (Converted/rounded to whole dollars as required by NZCS)", 233m, entryLine3.CL_CustomsValue);
			var entryLine4 = Declaration.ActiveEntryHeaders[0].MergedLines[3];
			AssertEquals("Line 4 calculated entry value (Converted/rounded to whole dollars as required by NZCS)", 252m, entryLine4.CL_CustomsValue);
			Assert("Entry after lines have been entered and merged now has the declaration correctly being above ($1001) the Deminimus threshold ($1000)", !Declaration.IsVFDUnderDeminimus);
		}

		public void TestJE_TransactionNatureVisible()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("Precondition: Declaration.JE_TransactionNatureVisible", true, Declaration.JE_TransactionNatureVisible);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			AssertEquals("For non TSW declarations, Transaction Nature should not be visible", true, Declaration.JE_TransactionNatureVisible);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("For TSW declarations, Transaction Nature should be visible", true, Declaration.JE_TransactionNatureVisible);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Transaction Nature should be visible for standard Export jobs", true, Declaration.JE_TransactionNatureVisible);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
			AssertEquals("For TSW declarations, Transaction Nature should be visible for TSW Export jobs also", true, Declaration.JE_TransactionNatureVisible);
		}

		public void TestDefaultPaymentMethodToSupplierInThisCountry()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			var supplierInThisCountry = Factory.LoadTop1<OrgHeader>(filter);
			supplierInThisCountry.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			supplierInThisCountry.MiscServ.OM_IMPaymentMethod = PaymentMethodList.Codes.ClientDeferred;

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_OH_Supplier = supplierInThisCountry.PK;
			AssertEquals("Supplier is in this country and its payment method is used to default to the job", PaymentMethodList.Codes.ClientDeferred, testDec.JE_PaymentMethod);
		}

		public void TestDontDefaultPaymentMethodToSupplierIfNotInList()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			var supplierInForeignCountry = Factory.LoadTop1<OrgHeader>(filter);
			supplierInForeignCountry.OH_RL_NKClosestPort = "ZZAAA";
			supplierInForeignCountry.MiscServ.OM_IMPaymentMethod = "Z!!";

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;

			testDec.JE_OH_Supplier = supplierInForeignCountry.PK;
			AssertEquals("Supplier's payment method is not valid for this job", PaymentMethodList.Codes.CashPaidByBroker, testDec.JE_PaymentMethod);
		}

		public void TestDefaultPaymentMethodToImporterInThisCountry()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
			var importerInThisCountry = Factory.LoadTop1<OrgHeader>(filter);
			importerInThisCountry.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			importerInThisCountry.MiscServ.OM_IMPaymentMethod = PaymentMethodList.Codes.ClientDeferred;

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = DefaultImportMessageType;
			testDec.JE_OH_Importer = importerInThisCountry.PK;

			AssertEquals("Importer is in this country and its payment method is used to default to the job", PaymentMethodList.Codes.ClientDeferred, testDec.JE_PaymentMethod);
		}

		public void TestDontDefaultPaymentMethodToImporterInForeignCountry()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			var importerInForeignCountry = Factory.LoadTop1<OrgHeader>(filter);
			importerInForeignCountry.OH_RL_NKClosestPort = "ZZAAA";
			importerInForeignCountry.MiscServ.OM_IMPaymentMethod = PaymentMethodList.Codes.ClientDeferred;

			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;

			testDec.JE_OH_Importer = importerInForeignCountry.PK;
			AssertEquals("Importer is in the foreign country and its payment method is not used to default to the job", PaymentMethodList.Codes.CashPaidByBroker, testDec.JE_PaymentMethod);
		}

		public void TestMergeErrorsAfterMergingIfRequiredFromShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			CommonShipmentDocumentSupporter documentSupporter = (CommonShipmentDocumentSupporter)shipment.DocumentSupporter;

			StmMenuItem menuItem = StmMenuItem.New(Factory);
			menuItem.SU_MenuName = "Entry Print";
			menuItem.SU_MenuPath = "Customs/";
			menuItem.SU_FilterList = "NZ";
			DocumentSupporterDataState dataState = documentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataState.IsValid", false, dataState.IsValid);
			AssertContains("DataState.ErrorMessage", Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, dataState.ErrorMessage);

			menuItem.SU_MenuName = "Customs Certificate";
			dataState = documentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataState.IsValid", false, dataState.IsValid);
			AssertContains("DataState.ErrorMessage", Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, dataState.ErrorMessage);

			menuItem.SU_MenuName = "SCOTTS SINKING BROTHEL";
			dataState = documentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("DataState.IsValid", true, dataState.IsValid);
			AssertEquals("DataState.ErrorMessage", "", dataState.ErrorMessage);
		}

		public void TestOtherInfoCollectionIsSetToReadOnlyWhenDeclarationIsSetToReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			OtherInfo otherInfo = declaration.OtherInfos.AddNew();
			AssertEquals(false, declaration.ReadOnly);
			AssertEquals(false, declaration.OtherInfos.ReadOnly);
			AssertEquals(false, otherInfo.ReadOnly);

			declaration.SetReadOnlyIncludingChildren(true);
			AssertEquals(true, declaration.ReadOnly);
			AssertEquals(true, declaration.OtherInfos.ReadOnly);
			AssertEquals(true, otherInfo.ReadOnly);
		}

		public void TestPermitCodesCollectionIsSetToReadOnlyWhenDeclarationIsSetToReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			PermitCode permitCode = declaration.PermitCodes.AddNew();
			AssertEquals(false, declaration.ReadOnly);
			AssertEquals(false, declaration.PermitCodes.ReadOnly);
			AssertEquals(false, permitCode.ReadOnly);

			declaration.SetReadOnlyIncludingChildren(true);
			AssertEquals(true, declaration.ReadOnly);
			AssertEquals(true, declaration.PermitCodes.ReadOnly);
			AssertEquals(true, permitCode.ReadOnly);
		}

		public void TestAppropriatePermitCodeListIsUsed()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TEPRM", "TEPRM");
			helper.CreateCusCodeType("TIPRM", "TIPRM");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEPRM", "EPA", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, "TIPRM", "BIP", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			var permit = declaration.PermitCodes.AddNew();
			permit.ZO_Code = PermitCodeList.Codes.CustomsDepartmentApproval;
			AssertEquals("Valid code for legacy", false, permit.ZO_CodeInfo.HasMessageErrors());

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			permit.ZO_Code = PermitCodeList.Codes.MinistryOfAgriculture5;
			AssertEquals("'AF5' is an invalid code for TSW Export", true, permit.ZO_CodeInfo.HasMessageErrors());

			permit.ZO_Code = "EPA";
			AssertEquals("'EPA' is a valid code for TSW Export", false, permit.ZO_CodeInfo.HasMessageErrors());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			permit.ZO_Code = "ZIL";
			AssertEquals("'ZIL' is an invalid code for TSW Import", true, permit.ZO_CodeInfo.HasMessageErrors());

			permit.ZO_Code = "BIP";
			AssertEquals("'BIP' is a valid code for TSW Import", false, permit.ZO_CodeInfo.HasMessageErrors());
		}

		public void TestMCDOtherInfo()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Assert(!(Declaration.OtherInfos.Count > 0));
			Assert("MCDOtherInfo None", Declaration.MCDOtherInfoValue.IsEmpty);

			var newOtherInfo = Declaration.OtherInfos.AddNew();
			newOtherInfo.ZO_Code = HeaderOtherInfoList.Codes.OtherDocument;
			Assert("MCDOtherInfo OtherDocument", Declaration.MCDOtherInfoValue.IsEmpty);

			var newMCDOtherInfo = Declaration.OtherInfos.AddNew();
			newMCDOtherInfo.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			newMCDOtherInfo.ZO_Data = "YYYNNN";
			AssertEquals("YYYNNN", Declaration.MCDOtherInfoValue);
		}

		public void TestEnsureTreatedAndTreatmentCertificateFlagsAreMutuallyExclusive()
		{
			// Currently these fields update each other. This should not happen, either can be ticked without the other.
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_IsWoodPackagingTreatmentCertificateAvailable = true;
			AssertEquals("Treated flag should not be set when TreatmentCertificateAvailable is set", false, declaration.JE_IsWoodPackagingTreated);

			declaration.JE_IsWoodPackagingTreated = true;
			declaration.JE_IsWoodPackagingTreatmentCertificateAvailable = false;
			AssertEquals("Treated flag should not be overriden when TreatmentCertificateAvailable is changed", true, declaration.JE_IsWoodPackagingTreated);

			declaration.JE_IsWoodPackagingTreated = false;
			declaration.JE_IsWoodPackagingTreatmentCertificateAvailable = false;

			declaration.JE_IsWoodPackagingTreated = true;
			AssertEquals("TreatmentCertificateAvailable flag simmilarly should not be set when Treated is set", false, declaration.JE_IsWoodPackagingTreatmentCertificateAvailable);

			declaration.JE_IsWoodPackagingTreatmentCertificateAvailable = true;
			declaration.JE_IsWoodPackagingTreated = false;
			AssertEquals("Treated flag should not be overriden when TreatmentCertificateAvailable is changed", true, declaration.JE_IsWoodPackagingTreatmentCertificateAvailable);
		}

		public void TestContainerQuarantineFieldsAreSaved()
		{
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_IsContainerClean = true;
			declaration1.JE_IsPackagingMaterialContaminated = true;
			declaration1.JE_IsWoodPackagingUsed = true;
			declaration1.JE_IsWoodPackagingTreated = true;
			declaration1.JE_IsWoodPackagingTreatmentCertificateAvailable = true;
			AssertEquals("JE_IsContainerClean", true, declaration1.JE_IsContainerClean);
			AssertEquals("JE_IsPackagingMaterialContaminated", true, declaration1.JE_IsPackagingMaterialContaminated);
			AssertEquals("JE_IsWoodPackagingUsed", true, declaration1.JE_IsWoodPackagingUsed);
			AssertEquals("JE_IsWoodPackagingTreated", true, declaration1.JE_IsWoodPackagingTreated);
			AssertEquals("JE_IsWoodPackagingTreatmentCertificateAvailable", true, declaration1.JE_IsWoodPackagingTreatmentCertificateAvailable);
		}

		public void TestContainerQuarantineQuestionsUpdateMCDCode()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			Declaration.JE_IsContainerClean = true;
			AssertEquals("JE_IsContainerClean", true, Declaration.JE_IsContainerClean);
			AssertEquals("JE_HaveMAFContainerDeclaration", true, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			var mCDOtherInfo = (OtherInfo)Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.MAFContainerDeclaration);
			AssertNotNull("MCDOtherInfo", mCDOtherInfo);
			AssertEquals("MCDOtherInfo.ZO_Data", "YNNNN", mCDOtherInfo.ZO_Data);

			Declaration.JE_IsContainerClean = false;
			AssertEquals("JE_IsContainerClean", false, Declaration.JE_IsContainerClean);
			AssertEquals("JE_HaveMAFContainerDeclaration", true, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			mCDOtherInfo = (OtherInfo)Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.MAFContainerDeclaration);
			AssertNotNull("MCDOtherInfo", mCDOtherInfo);
			AssertEquals("MCDOtherInfo.ZO_Data", "NNNNN", mCDOtherInfo.ZO_Data);

			Declaration.JE_HaveMAFContainerDeclaration = false;
			AssertEquals("JE_IsContainerClean", false, Declaration.JE_IsContainerClean);
			AssertEquals("JE_HaveMAFContainerDeclaration", false, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			mCDOtherInfo = (OtherInfo)Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.MAFContainerDeclaration);
			AssertNotNull("MCDOtherInfo", mCDOtherInfo);
			AssertEquals("MCDOtherInfo.ZO_Data", "", mCDOtherInfo.ZO_Data);

			Declaration.JE_SendMCDContainerQuarantineDeclaration = false;
			AssertEquals("JE_IsContainerClean", false, Declaration.JE_IsContainerClean);
			AssertEquals("JE_HaveMAFContainerDeclaration", false, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", false, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			mCDOtherInfo = (OtherInfo)Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.MAFContainerDeclaration);
			AssertNull("MCDOtherInfo", mCDOtherInfo);
		}

		public void TestContainerQuarantineQuestionsFlagCorrectAfterReloadAddInfo()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Declaration.JE_GoodsLocatedAt = "";

			Declaration.JE_IsContainerClean = true;
			AssertEquals("JE_IsContainerClean", true, Declaration.JE_IsContainerClean);
			AssertEquals("JE_HaveMAFContainerDeclaration", true, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			var mCDOtherInfo = (OtherInfo)Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.MAFContainerDeclaration);
			AssertNotNull("MCDOtherInfo", mCDOtherInfo);
			AssertEquals("MCDOtherInfo.ZO_Data", "YNNNN", mCDOtherInfo.ZO_Data);
			AssertEquals("Declaration.JE_AddInfo", "OtherInfos=MCD=YNNNN*TransactionNature=10", Declaration.JE_AddInfo);
			Declaration.OtherInfos.LoadFromString("MCD=YNNNN");
			AssertEquals("JE_HaveMAFContainerDeclaration", true, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals("JE_IsContainerClean", true, Declaration.JE_IsContainerClean);
		}

		public void TestQuarantineDeclarationNotInCollectionException()
		{
			ErrorReporter.Clear();

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = "FCL";

			Declaration.JE_SendMCDContainerQuarantineDeclaration = true;

			OtherInfo mCDOtherInfo = (OtherInfo)Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.MAFContainerDeclaration);
			AssertNotNull(mCDOtherInfo);
			AssertEquals(1, Declaration.OtherInfos.Count);

			mCDOtherInfo.ZO_Code = ZString.Empty;
			AssertHasError(mCDOtherInfo.ZO_CodeInfo, "Please enter a Code.");

			Declaration.OtherInfos.RemoveAndDelete(mCDOtherInfo);

			Declaration.JE_SendMCDContainerQuarantineDeclaration = false;
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);

			Declaration.JE_SendMCDContainerQuarantineDeclaration = true;
			Declaration.JE_SendMCDContainerQuarantineDeclaration = false;
			AssertEquals(0, Declaration.OtherInfos.Count);
		}

		public void TestContainerQuarantineFieldsUpdateEachOtherBackwardsAndForwardsToKeepTheSetValid()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			Declaration.JE_IsContainerClean = true;
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals("JE_HaveMAFContainerDeclaration", true, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_IsContainerClean", true, Declaration.JE_IsContainerClean);
			AssertEquals("JE_IsPackagingMaterialContaminated", false, Declaration.JE_IsPackagingMaterialContaminated);
			AssertEquals("JE_IsWoodPackagingUsed", false, Declaration.JE_IsWoodPackagingUsed);
			AssertEquals("JE_IsWoodPackagingTreated", false, Declaration.JE_IsWoodPackagingTreated);
			AssertEquals("JE_IsWoodPackagingTreatmentCertificateAvailable", false, Declaration.JE_IsWoodPackagingTreatmentCertificateAvailable);

			Declaration.JE_SendMCDContainerQuarantineDeclaration = false;
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", false, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals("JE_HaveMAFContainerDeclaration", false, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_IsContainerClean", false, Declaration.JE_IsContainerClean);
			AssertEquals("JE_IsPackagingMaterialContaminated", false, Declaration.JE_IsPackagingMaterialContaminated);
			AssertEquals("JE_IsWoodPackagingUsed", false, Declaration.JE_IsWoodPackagingUsed);
			AssertEquals("JE_IsWoodPackagingTreated", false, Declaration.JE_IsWoodPackagingTreated);
			AssertEquals("JE_IsWoodPackagingTreatmentCertificateAvailable", false, Declaration.JE_IsWoodPackagingTreatmentCertificateAvailable);

			Declaration.JE_IsPackagingMaterialContaminated = true;
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals("JE_HaveMAFContainerDeclaration", true, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_IsContainerClean", false, Declaration.JE_IsContainerClean);
			AssertEquals("JE_IsPackagingMaterialContaminated", true, Declaration.JE_IsPackagingMaterialContaminated);
			AssertEquals("JE_IsWoodPackagingUsed", false, Declaration.JE_IsWoodPackagingUsed);
			AssertEquals("JE_IsWoodPackagingTreated", false, Declaration.JE_IsWoodPackagingTreated);
			AssertEquals("JE_IsWoodPackagingTreatmentCertificateAvailable", false, Declaration.JE_IsWoodPackagingTreatmentCertificateAvailable);

			Declaration.JE_HaveMAFContainerDeclaration = false;
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals("JE_HaveMAFContainerDeclaration", false, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_IsContainerClean", false, Declaration.JE_IsContainerClean);
			AssertEquals("JE_IsPackagingMaterialContaminated", false, Declaration.JE_IsPackagingMaterialContaminated);
			AssertEquals("JE_IsWoodPackagingUsed", false, Declaration.JE_IsWoodPackagingUsed);
			AssertEquals("JE_IsWoodPackagingTreated", false, Declaration.JE_IsWoodPackagingTreated);
			AssertEquals("JE_IsWoodPackagingTreatmentCertificateAvailable", false, Declaration.JE_IsWoodPackagingTreatmentCertificateAvailable);

			Declaration.JE_IsWoodPackagingTreatmentCertificateAvailable = true;
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals("JE_HaveMAFContainerDeclaration", true, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals("JE_IsContainerClean", false, Declaration.JE_IsContainerClean);
			AssertEquals("JE_IsPackagingMaterialContaminated", false, Declaration.JE_IsPackagingMaterialContaminated);
			AssertEquals("JE_IsWoodPackagingUsed - entry of TreatmentCertificate is no longer setting other flags", false, Declaration.JE_IsWoodPackagingUsed);
			AssertEquals("JE_IsWoodPackagingTreated - no longer setting other flags", false, Declaration.JE_IsWoodPackagingTreated);
			AssertEquals("JE_IsWoodPackagingTreatmentCertificateAvailable", true, Declaration.JE_IsWoodPackagingTreatmentCertificateAvailable);
		}

		public void TestMCDCodeUpdatesContainerQuarantineQuestions()
		{
			string stage = "Precondition: ";
			AssertEquals(stage + "JE_SendMCDContainerQuarantineDeclaration", false, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals(stage + "JE_HaveMAFContainerDeclaration", false, Declaration.JE_HaveMAFContainerDeclaration);

			stage = "MCD Other Info Code Now Added: ";
			OtherInfo mCDOtherInfo = (OtherInfo)Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.MAFContainerDeclaration, "");
			AssertEquals(stage + "JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals(stage + "JE_HaveMAFContainerDeclaration", false, Declaration.JE_HaveMAFContainerDeclaration);

			stage = "MCD Data Filled In with YYYYY: ";
			mCDOtherInfo.ZO_Data = "YYYYY";
			AssertEquals(stage + "JE_SendMCDContainerQuarantineDeclaration", true, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals(stage + "JE_HaveMAFContainerDeclaration", true, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals(stage + "JE_IsContainerClean", true, Declaration.JE_IsContainerClean);
			AssertEquals(stage + "JE_IsPackagingMaterialContaminated", true, Declaration.JE_IsPackagingMaterialContaminated);
			AssertEquals(stage + "JE_IsWoodPackagingUsed", true, Declaration.JE_IsWoodPackagingUsed);
			AssertEquals(stage + "JE_IsWoodPackagingTreated", true, Declaration.JE_IsWoodPackagingTreated);
			AssertEquals(stage + "JE_IsWoodPackagingTreatmentCertificateAvailable", true, Declaration.JE_IsWoodPackagingTreatmentCertificateAvailable);

			stage = "MCD Other Info Code Removed: ";
			Declaration.OtherInfos.RemoveAndDelete(mCDOtherInfo);
			AssertEquals(stage + "JE_SendMCDContainerQuarantineDeclaration", false, Declaration.JE_SendMCDContainerQuarantineDeclaration);
			AssertEquals(stage + "JE_HaveMAFContainerDeclaration", false, Declaration.JE_HaveMAFContainerDeclaration);
			AssertEquals(stage + "JE_IsContainerClean", false, Declaration.JE_IsContainerClean);
			AssertEquals(stage + "JE_IsPackagingMaterialContaminated", false, Declaration.JE_IsPackagingMaterialContaminated);
			AssertEquals(stage + "JE_IsWoodPackagingUsed", false, Declaration.JE_IsWoodPackagingUsed);
			AssertEquals(stage + "JE_IsWoodPackagingTreated", false, Declaration.JE_IsWoodPackagingTreated);
			AssertEquals(stage + "JE_IsWoodPackagingTreatmentCertificateAvailable", false, Declaration.JE_IsWoodPackagingTreatmentCertificateAvailable);
		}

		public void TestOtherInfoMCDCodeDoesntGetDuplicatedIfItExistsUponLoad()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.MAFContainerDeclaration, "YYNNN");
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			JobDeclaration loadedDec = secondFactory.Load<JobDeclaration>(Declaration.PK);
			AssertEquals("Declaration.OtherInfos.Count touched just to initialise NZAddInfo", 1, loadedDec.OtherInfos.Count);
			loadedDec.JE_IsContainerClean = true;
			AssertEquals("Declaration.OtherInfos.Count should only have 1", 1, loadedDec.OtherInfos.Count);
			HeaderOtherInfo otherInfo = loadedDec.OtherInfos[0];
			AssertEquals("OtherInfo.ZO_Code", HeaderOtherInfoList.Codes.MAFContainerDeclaration, otherInfo.ZO_Code);
			AssertEquals("OtherInfo.ZO_Data", "YYNNN", otherInfo.ZO_Data);
		}

		public void TestJE_MessageTypeHasChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_ApplicationCode default value", JobApplicationCodeList.Codes.TSW, declaration.JE_ApplicationCode);
			AssertEquals("JE_MessageType default value", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

			var newDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_ApplicationCode default value now set for TSW", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);
			AssertEquals("JE_MessageType default value", "EXP", newDeclaration.JE_MessageType);

			newDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JE_ApplicationCode should be reset to non TSW default value", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);

			newDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("JE_ApplicationCode should be reset to TSW for Export as TSWExport is active", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);

			var newDeclaration2 = Factory.New<JobDeclaration>();
			AssertEquals("JE_ApplicationCode default value set for TSW", JobApplicationCodeList.Codes.TSW, newDeclaration2.JE_ApplicationCode);
			AssertEquals("JE_MessageType default value", "EXP", newDeclaration2.JE_MessageType);

			newDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JE_ApplicationCode should be reset to TSW default value as TSW-IM1 is active", JobApplicationCodeList.Codes.TSW, newDeclaration2.JE_ApplicationCode);

			newDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("JE_ApplicationCode should be reset to TSW for Export as TSW-EX1 is active", JobApplicationCodeList.Codes.TSW, newDeclaration2.JE_ApplicationCode);

			newDeclaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("JE_ApplicationCode should be able to be changed by user to send CUSMOD message if required", JobApplicationCodeList.Codes.CUS, newDeclaration2.JE_ApplicationCode);
		}

		public void TestEntryStyleHasChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_ApplicationCode default value", JobApplicationCodeList.Codes.TSW, declaration.JE_ApplicationCode);
			AssertEquals("JE_MessageType default value", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

			var newDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("JE_ApplicationCode default value should still be CUS as EX1 & IM1 are not active", JobApplicationCodeList.Codes.TSW, newDeclaration.JE_ApplicationCode);
			AssertEquals("JE_MessageType default value", "EXP", newDeclaration.JE_MessageType);
		}

		public void TestJE_MessageTypeDoesNotDefaultOnChangeWhenAlreadyInDatabase()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals("JE_ApplicationCode default value", JobApplicationCodeList.Codes.TSW, declaration.JE_ApplicationCode);
			AssertEquals("JE_MessageType default value", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JE_ApplicationCode default value should be TSW for this job now", JobApplicationCodeList.Codes.TSW, declaration.JE_ApplicationCode);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("JE_ApplicationCode should reset to the default messaging mode even if job is in the database, when the type or sub type is changed", JobApplicationCodeList.Codes.CUS, declaration.JE_ApplicationCode);
		}

		public void TestRefreshBindingGetsCalled()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_SendMCDContainerQuarantineDeclarationInfo.ValueChanged += new EventHandler(JE_SendMCDContainerQuarantineDeclarationInfo_ValueChanged);
			Declaration.JE_HaveMAFContainerDeclarationInfo.ValueChanged += new EventHandler(JE_HaveMAFContainerDeclarationInfo_ValueChanged);
			Declaration.JE_IsContainerCleanInfo.ValueChanged += new EventHandler(JE_IsContainerCleanInfo_ValueChanged);
			Declaration.JE_IsPackagingMaterialContaminatedInfo.ValueChanged += new EventHandler(JE_IsPackagingMaterialContaminatedInfo_ValueChanged);
			Declaration.JE_IsWoodPackagingUsedInfo.ValueChanged += new EventHandler(JE_IsWoodPackagingUsedInfo_ValueChanged);
			Declaration.JE_IsWoodPackagingTreatedInfo.ValueChanged += new EventHandler(JE_IsWoodPackagingTreatedInfo_ValueChanged);
			Declaration.JE_IsWoodPackagingTreatmentCertificateAvailableInfo.ValueChanged += new EventHandler(JE_IsWoodPackagingTreatmentCertificateAvailableInfo_ValueChanged);

			var mcdCode = Declaration.OtherInfos.AddNew();
			mcdCode.ZO_Code = HeaderOtherInfoList.Codes.MAFContainerDeclaration;
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration value chaged.", true, sendMCDContainerQuarantineDeclarationRefreshed);
			AssertEquals("JE_HaveMAFContainerDeclaration value chaged.", true, haveMAFContainerDeclarationRefreshed);
			AssertEquals("JE_IsContainerClean value chaged.", true, isContainerCleanRefreshed);
			AssertEquals("JE_IsPackagingMaterialContaminated value chaged.", true, isPackagingMaterialContaminatedRefreshed);
			AssertEquals("JE_IsWoodPackagingUsed value chaged.", true, isWoodPackagingUsedRefreshed);
			AssertEquals("JE_IsWoodPackagingTreated value chaged.", true, isWoodPackagingTreatedRefreshed);
			AssertEquals("JE_IsWoodPackagingTreatmentCertificateAvailable value chaged.", true, isWoodPackagingTreatmentCertificateAvailableRefreshed);

			sendMCDContainerQuarantineDeclarationRefreshed = false;
			haveMAFContainerDeclarationRefreshed = false;
			isContainerCleanRefreshed = false;
			isPackagingMaterialContaminatedRefreshed = false;
			isWoodPackagingUsedRefreshed = false;
			isWoodPackagingTreatedRefreshed = false;
			isWoodPackagingTreatmentCertificateAvailableRefreshed = false;

			mcdCode.ZO_Data = "NNNNN";
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration value chaged.", true, sendMCDContainerQuarantineDeclarationRefreshed);
			AssertEquals("JE_HaveMAFContainerDeclaration value chaged.", true, haveMAFContainerDeclarationRefreshed);
			AssertEquals("JE_IsContainerClean value chaged.", true, isContainerCleanRefreshed);
			AssertEquals("JE_IsPackagingMaterialContaminated value chaged.", true, isPackagingMaterialContaminatedRefreshed);
			AssertEquals("JE_IsWoodPackagingUsed value chaged.", true, isWoodPackagingUsedRefreshed);
			AssertEquals("JE_IsWoodPackagingTreated value chaged.", true, isWoodPackagingTreatedRefreshed);
			AssertEquals("JE_IsWoodPackagingTreatmentCertificateAvailable value chaged.", true, isWoodPackagingTreatmentCertificateAvailableRefreshed);

			sendMCDContainerQuarantineDeclarationRefreshed = false;
			haveMAFContainerDeclarationRefreshed = false;
			isContainerCleanRefreshed = false;
			isPackagingMaterialContaminatedRefreshed = false;
			isWoodPackagingUsedRefreshed = false;
			isWoodPackagingTreatedRefreshed = false;
			isWoodPackagingTreatmentCertificateAvailableRefreshed = false;

			Declaration.OtherInfos.Remove(mcdCode);
			AssertEquals("JE_SendMCDContainerQuarantineDeclaration value chaged.", true, sendMCDContainerQuarantineDeclarationRefreshed);
			AssertEquals("JE_HaveMAFContainerDeclaration value chaged.", true, haveMAFContainerDeclarationRefreshed);
			AssertEquals("JE_IsContainerClean value chaged.", true, isContainerCleanRefreshed);
			AssertEquals("JE_IsPackagingMaterialContaminated value chaged.", true, isPackagingMaterialContaminatedRefreshed);
			AssertEquals("JE_IsWoodPackagingUsed value chaged.", true, isWoodPackagingUsedRefreshed);
			AssertEquals("JE_IsWoodPackagingTreated value chaged.", true, isWoodPackagingTreatedRefreshed);
			AssertEquals("JE_IsWoodPackagingTreatmentCertificateAvailable value chaged.", true, isWoodPackagingTreatmentCertificateAvailableRefreshed);
		}

		public void TestDefaultProcessingPortIsSetOnNewJobDeclaration()
		{
			NZCustomsDataRegistry.Instance.DefaultCustomsProcessingPort.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NZAKL");
			JobDeclaration declaration1 = JobDeclaration.New(Factory);
			AssertEquals("Declaration1.JE_RL_NKProcessingPort", "NZAKL", declaration1.JE_RL_NKProcessingPort);

			NZCustomsDataRegistry.Instance.DefaultCustomsProcessingPort.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NZCHC");
			JobDeclaration declaration2 = JobDeclaration.New(Factory);
			AssertEquals("Declaration1.JE_RL_NKProcessingPort", "NZCHC", declaration2.JE_RL_NKProcessingPort);
		}

		public void TestIsInDatabaseIncludingChildrenWorksOnDeclarationWithAddInfoAgainstIt()
		{
			AssertEquals("Precondition: Declaration.IsInDatabaseIncludingChildren", false, Declaration.IsInDatabaseIncludingChildren);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MYCODE";
			Declaration.WarehouseDocAddress.E2_OA_Address = org.MainAddress.PK;
			Declaration.ContainerYardDocAddress.E2_OA_Address = org.MainAddress.PK;
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = org.MainAddress.PK;
			Declaration.DepotDocAddress.E2_OA_Address = org.MainAddress.PK;
			Declaration.ImporterDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			Declaration.ImporterDeliveryAddress.E2_OA_Address = org.MainAddress.PK;
			Factory.Save();
			AssertEquals("Declaration.IsInDatabaseIncludingChildren", true, Declaration.IsInDatabaseIncludingChildren);
		}

		public void TestSettingEDITransmitDateSetsExchangeRatesOnAllInvoiceHeaders()
		{
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			aUD.SetCustomsRate(new ZDateTime(2000, 1, 1), new ZDateTime(2004, 6, 6), 1.1m);
			aUD.SetCustomsRate(new ZDateTime(2004, 6, 7), new ZDateTime(2005, 6, 6), 1.2m);
			uSD.SetCustomsRate(new ZDateTime(2000, 1, 1), new ZDateTime(2004, 6, 6), 1.3m);
			uSD.SetCustomsRate(new ZDateTime(2004, 6, 7), new ZDateTime(2005, 6, 6), 1.4m);

			Declaration.JE_EDITransmitDate = new ZDateTime(2004, 1, 1);

			JobComInvoiceGroupHeader groupHeader1 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			JobComInvoiceGroupHeader groupHeader2 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();

			JobComInvoiceHeader invoiceHeader1 = groupHeader1.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			JobComInvoiceHeader invoiceHeader2 = groupHeader2.JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = uSD.RX_Code;

			AssertEquals("InvoiceHeader1.JZ_InvoiceCurrExRate", 1.1m, invoiceHeader1.JZ_InvoiceCurrExRate);
			AssertEquals("InvoiceHeader2.JZ_InvoiceCurrExRate", 1.3m, invoiceHeader2.JZ_InvoiceCurrExRate);

			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);

			AssertEquals("InvoiceHeader1.JZ_InvoiceCurrExRate", 1.2m, invoiceHeader1.JZ_InvoiceCurrExRate);
			AssertEquals("InvoiceHeader2.JZ_InvoiceCurrExRate", 1.4m, invoiceHeader2.JZ_InvoiceCurrExRate);
		}

		public void TestSettingEDITransmitDateTriggersValidationOnAllJobComInvoiceHeaders()
		{
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			aUD.SetCustomsRate(new ZDateTime(2000, 1, 1), new ZDateTime(2004, 6, 6), 1.1m);
			aUD.SetCustomsRate(new ZDateTime(2004, 6, 7), new ZDateTime(2005, 6, 6), 1.2m);
			uSD.SetCustomsRate(new ZDateTime(2000, 1, 1), new ZDateTime(2004, 6, 6), 1.3m);
			uSD.SetCustomsRate(new ZDateTime(2004, 6, 7), new ZDateTime(2005, 6, 6), 1.4m);

			Declaration.JE_EDITransmitDate = new ZDateTime(1999, 1, 1);

			JobComInvoiceGroupHeader groupHeader1 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			JobComInvoiceGroupHeader groupHeader2 = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();

			JobComInvoiceHeader invoiceHeader1 = groupHeader1.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			JobComInvoiceHeader invoiceHeader2 = groupHeader2.JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = uSD.RX_Code;

			AssertHasMessageError(invoiceHeader1.JZ_InvoiceCurrExRateInfo, JobComInvoiceHeaderValidationFormalEntry.MissingExchangeRate);
			AssertHasMessageError(invoiceHeader2.JZ_InvoiceCurrExRateInfo, JobComInvoiceHeaderValidationFormalEntry.MissingExchangeRate);

			Declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);

			AssertNoMessageError(invoiceHeader1.JZ_InvoiceCurrExRateInfo, JobComInvoiceHeaderValidationFormalEntry.MissingExchangeRate);
			AssertNoMessageError(invoiceHeader2.JZ_InvoiceCurrExRateInfo, JobComInvoiceHeaderValidationFormalEntry.MissingExchangeRate);
		}

		public void TestSettingJE_ExportDateTriggersDefaultingOfJE_EDITransmitDateIfPossible()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			AssertEquals("Precondition: Declaration.JE_EDITransmitDate", ZDateTime.Empty, declaration.JE_EDITransmitDate);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Declaration.JE_EDITransmitDate", ZDateTime.Empty, declaration.JE_EDITransmitDate);
			declaration.JE_ExportDate = declaration.CachedTodaysDate;
			AssertEquals("Declaration.JE_EDITransmitDate", declaration.CachedTodaysDate, declaration.JE_EDITransmitDate);
		}

		public void TestMarkApportionmentDirtyWhenTransmitDateIsChanged()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.ResumeApportionment();

			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate;
			AssertEquals("should be dirty", true, declaration.ApportionmentDirty);
		}

		public void TestDefaultingOfJE_EDITransmitDateDoesntHappenIfAMessageHasBeenSent()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = declaration.CachedTodaysDate;
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate;
			AssertEquals("Precondition: Declaration.JE_EDITransmitDate", declaration.CachedTodaysDate, declaration.JE_EDITransmitDate);
			declaration.CusEntryHeader.Messages.AddNew();
			declaration.JE_DateOfArrival = declaration.CachedTodaysDate.AddDays(100);
			AssertEquals("Declaration.JE_EDITransmitDate", declaration.CachedTodaysDate, declaration.JE_EDITransmitDate);
		}

		#endregion

		public void TestIsEntryClear()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				testDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;

				testDeclaration.JE_EntryStatus = TSWEntryStatusList.Codes.CCC;
				Assert("AppCode:CUS - Test CCC", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
				Assert("AppCode:CUS - Test ConsignmentWrittenOff", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.AdjustmentAccepted;
				Assert("AppCode:CUS - Test AdjustmentAccepted", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
				Assert("AppCode:CUS - Test DeliveryOrderReceived", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryRestored;
				Assert("AppCode:CUS - Test EntryRestored", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
				Assert("AppCode:CUS - Test ManualEntryCannotBeSentToCustoms", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
				Assert("AppCode:CUS - Test EntryCleared", testDeclaration.IsEntryClear);

				testDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

				testDeclaration.JE_EntryStatus = TSWEntryStatusList.Codes.CCC;
				Assert("AppCode:TSW - Test CCC", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
				Assert("AppCode:TSW - Test ConsignmentWrittenOff", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.AdjustmentAccepted;
				Assert("AppCode:TSW - Test AdjustmentAccepted", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
				Assert("AppCode:TSW - Test DeliveryOrderReceived", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryRestored;
				Assert("AppCode:TSW - Test EntryRestored", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
				Assert("AppCode:TSW - Test ManualEntryCannotBeSentToCustoms", testDeclaration.IsEntryClear);
				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
				Assert("AppCode:TSW - Test EntryCleared", testDeclaration.IsEntryClear);
			});
		}

		public void TestShipmentGetCusEntryNumberFromDeclaration()
		{
			var testShipment = Factory.New<ForwardingShipment>();
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_JS = testShipment.PK;
			testDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			var testEntryHeader = testDeclaration.CusEntryHeader;
			var testCusEntryNumber = Factory.New<CusEntryNumber>();
			testCusEntryNumber.CE_EntryNum = "TST1";
			testCusEntryNumber.CE_EntryType = "TST";
			testCusEntryNumber.CE_EntryIsSystemGenerated = true;
			testCusEntryNumber.CE_ParentID = testEntryHeader.PK;
			testCusEntryNumber.CE_ParentTable = testEntryHeader.TableName;
			Factory.Save();

			CombineAssertions("CUS Testing", () =>
			{
				testDeclaration.JE_EntryStatus = TSWEntryStatusList.Codes.CCC;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:CUS - Test CCC", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:CUS - Test ConsignmentWrittenOff", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.AdjustmentAccepted;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:CUS - Test AdjustmentAccepted", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:CUS - Test DeliveryOrderReceived", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryRestored;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:CUS - Test EntryRestored", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:CUS - Test ManualEntryCannotBeSentToCustoms", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:CUS - Test EntryCleared", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);
			});

			testDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Factory.Save();
			CombineAssertions("TSW Testing", () =>
			{
				testDeclaration.JE_EntryStatus = TSWEntryStatusList.Codes.CCC;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:TSW - Test CCC", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:TSW - Test ConsignmentWrittenOff", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.AdjustmentAccepted;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:TSW - Test AdjustmentAccepted", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:TSW - Test DeliveryOrderReceived", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryRestored;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:TSW - Test EntryRestored", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:TSW - Test ManualEntryCannotBeSentToCustoms", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);

				testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
				testShipment.ResetCusEntryNumbers();
				AssertEquals("AppCode:TSW - Test EntryCleared", "TST-TST1", testShipment.CustomsEntryNumberType + "-" + testShipment.CustomsEntryNumber);
			});
		}

		public void TestSettingAccountDetailsDefaultToAccount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
			declaration.ZX_AccountNumber = "12345";
			declaration.ZX_AccountHolder = "SOME OTHER BUGGER";

			AssertEquals("ZX_PaymentMethod", MAFPaymentMethodList.Codes.Account, declaration.ZX_PaymentMethod);
			AssertEquals("ZX_AccountNumber", "12345", declaration.ZX_AccountNumber);
			AssertEquals("ZX_AccountHolder", "SOME OTHER BUGGER", declaration.ZX_AccountHolder);

			declaration.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Cash;
			AssertEquals("ZX_PaymentMethod", MAFPaymentMethodList.Codes.Cash, declaration.ZX_PaymentMethod);
			AssertEquals("ZX_AccountNumber", "", declaration.ZX_AccountNumber);
			AssertEquals("ZX_AccountHolder", "", declaration.ZX_AccountHolder);

			declaration.ZX_AccountNumber = "12345";
			AssertEquals("ZX_PaymentMethod", MAFPaymentMethodList.Codes.Account, declaration.ZX_PaymentMethod);
			AssertEquals("ZX_AccountNumber", "12345", declaration.ZX_AccountNumber);
			AssertEquals("ZX_AccountHolder", "", declaration.ZX_AccountHolder);

			declaration.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Cash;
			AssertEquals("ZX_PaymentMethod", MAFPaymentMethodList.Codes.Cash, declaration.ZX_PaymentMethod);
			AssertEquals("ZX_AccountNumber", "", declaration.ZX_AccountNumber);
			AssertEquals("ZX_AccountHolder", "", declaration.ZX_AccountHolder);

			declaration.ZX_AccountHolder = "SOME OTHER BUGGER";
			AssertEquals("ZX_PaymentMethod", MAFPaymentMethodList.Codes.Account, declaration.ZX_PaymentMethod);
			AssertEquals("ZX_AccountNumber", "", declaration.ZX_AccountNumber);
			AssertEquals("ZX_AccountHolder", "SOME OTHER BUGGER", declaration.ZX_AccountHolder);

			declaration.ZX_PaymentMethod = ZString.Empty;
			AssertEquals("ZX_PaymentMethod", ZString.Empty, declaration.ZX_PaymentMethod);
			AssertEquals("ZX_AccountNumber", "", declaration.ZX_AccountNumber);
			AssertEquals("ZX_AccountHolder", "", declaration.ZX_AccountHolder);
		}

		public void TestMPIPaymentDetailsSent()
		{
			const string expectedImporterAccountHolderName = "DA MAIN MAN";
			const string expectedImporterAccountNumber = "234987324";
			const string expectedBranchAccountHolderName = "BRANCH FIDDY";
			const string expectedBranchAccountNumber = "239087422";

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = expectedImporterAccountHolderName;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			AssertMultilineASCIIEquals("PaymentDetailsSent", @"
CSH - Cash
NB: You can setup a default MPI QE # against Importer/Branch Organization.
".Trim(), declaration.PaymentDetailsSent);

			var branchOrganisation = declaration.Branch.OrgProxy;
			AssertNotNull("Precondition: declaration.Branch.OrgProxy", branchOrganisation);
			branchOrganisation.OH_FullName = expectedBranchAccountHolderName;
			branchOrganisation.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, expectedBranchAccountNumber);
			AssertMultilineASCIIEquals("PaymentDetailsSent", @"
ACC - MPI Account
239087422 / BRANCH FIDDY
NB: Defaulted from Branch Organization.
".Trim(), declaration.PaymentDetailsSent);

			importer.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, expectedImporterAccountNumber);
			AssertMultilineASCIIEquals("PaymentDetailsSent", @"
ACC - MPI Account
234987324 / DA MAIN MAN
NB: Defaulted from Importer.
".Trim(), declaration.PaymentDetailsSent);

			declaration.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
			AssertMultilineASCIIEquals("PaymentDetailsSent", @"
ACC - MPI Account
234987324 / DA MAIN MAN
NB: Defaulted from Importer.
".Trim(), declaration.PaymentDetailsSent);

			declaration.ZX_PaymentMethod = ZString.Empty;
			declaration.ZX_AccountNumber = "12345";
			declaration.ZX_AccountHolder = "SOME OTHER BUGGER";
			AssertMultilineASCIIEquals("PaymentDetailsSent", @"
ACC - MPI Account
12345 / SOME OTHER BUGGER
".Trim(), declaration.PaymentDetailsSent);
		}

		public void TestMPIPaymentDetailsSentIndividualFields()
		{
			const string expectedAccoungHolderName = "Test Importer";
			const string expectedAccountNumber = "888";
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = expectedAccoungHolderName;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("PaymentTypeToBeSent", MAFPaymentMethodList.Codes.Cash, declaration.PaymentTypeToBeSent);
			AssertEquals("AccountHolderNameToBeSent", "", declaration.AccountHolderNameToBeSent);
			AssertEquals("AccountNumberToBeSent", "", declaration.AccountNumberToBeSent);

			importer.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, expectedAccountNumber);
			AssertEquals("PaymentTypeToBeSent", MAFPaymentMethodList.Codes.Account, declaration.PaymentTypeToBeSent);
			AssertEquals("AccountHolderNameToBeSent", expectedAccoungHolderName, declaration.AccountHolderNameToBeSent);
			AssertEquals("AccountNumberToBeSent", expectedAccountNumber, declaration.AccountNumberToBeSent);
		}

		public void TestShipmentContainsEntryNumber()
		{
			const string TestTag = "TST";
			var testShipments = new[] {
				FormalEntryStatusList.Codes.EntryCleared,
				FormalEntryStatusList.Codes.DeliveryOnPayment,
			}.Select(status =>
			{
				var testShipment = Factory.New<ForwardingShipment>();
				testShipment.JS_ShipmentStatus = TestTag;
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_JS = testShipment.PK;
				testDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				testDeclaration.JE_EntryStatus = status;
				var testEntryHeader = testDeclaration.CusEntryHeader;
				var testCusEntryNumber = Factory.New<CusEntryNumber>();
				testCusEntryNumber.CE_EntryNum = TestTag + status;
				testCusEntryNumber.CE_EntryType = TestTag;
				testCusEntryNumber.CE_EntryIsSystemGenerated = true;
				testCusEntryNumber.CE_ParentID = testEntryHeader.PK;
				testCusEntryNumber.CE_ParentTable = testEntryHeader.TableName;
				return testShipment;
			}).ToArray();

			Factory.Save();

			var loadedShipments = new ForwardingShipmentCollection(Factory, new ZQuery(JobShipmentSchema.JS_ShipmentStatus, TestTag));
			loadedShipments.Load();

			foreach (ForwardingShipment shipment in loadedShipments)
			{
				AssertNotNullOrEmpty(shipment.CustomsEntryNumber);
			}
		}

		protected override void AssertHouseBillProxiedThroughHouseBillsFirstElement(BaseJobDeclaration declaration)
		{
			AssertEquals("JE_HouseBill", "", declaration.JE_HouseBill);
			AssertEquals("HouseBills.Count", 0, declaration.Bills.Count);
		}

		public override void TestSetDefaultPackagesType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("default value for JE_TotalNoOfPacksPackType", "PK", declaration.JE_TotalNoOfPacksPackType);
		}

		public void TestDeliveryNotificationCCPATFDocAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(DocAddressType.NotifyParty2, declaration.NotifyParty2DocumentaryAddress.Requirement.DefaultDocAddressType);
			AssertEquals(ContactType.NotifyParty, declaration.NotifyParty2DocumentaryAddress.Requirement.DefaultContactType);
		}

		public void TestJE_RL_NKPortOfDeliveryNotification()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfDeliveryNotify = "DORNE";
			AssertEquals("DORNE", declaration.JE_RL_NKPortOfDeliveryNotify);
		}

		public void TestRefreshDefaultGoodsLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GoodsLocatedAt = GoodsLocatedAtListForSeaImport.Codes.DIS;
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("NZAKL", declaration.JE_Cal_GoodsLocation);
			declaration.JE_RL_NKPortOfArrival = "NZIVC";
			AssertEquals("NZIVC", declaration.JE_Cal_GoodsLocation);

			declaration.JE_GoodsLocatedAt = GoodsLocatedAtListForSeaExport.Codes.PC;
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			AssertEquals("NZAKL", declaration.JE_Cal_GoodsLocation);
			declaration.JE_RL_NKPortOfLoading = "NZIVC";
			AssertEquals("NZIVC", declaration.JE_Cal_GoodsLocation);
		}

		#region Test ICusEntryNumFilterProvider

		public override void TestICusEntryNumFilterProviderImplementation()
		{
			// NZ implementation of ValidCusEntryNumFilter is different from implementation in shared.
			// NZ delarations can have only one entry header and CusEntryNumber cannot be attached to declaration directly.

			var declaration = Factory.New<JobDeclaration>();
			var cusEntryNumFilterProvider = (ICusEntryNumFilterProvider)declaration;
			var entryNumbers = new Customs.Business.CusEntryNumCollection(Factory);

			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertEquals("pre-condition", 0, entryNumbers.Count);

			var entryHeader1 = declaration.CusEntryHeader;
			entryHeader1.EntryNumber = "12121212";
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"12121212"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var entryNum1 = Factory.New<CusEntryNumber>();
			entryNum1.CE_ParentID = entryHeader1.PK;
			entryNum1.CE_EntryNum = "12345678";
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"12121212",
				"12345678"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			// NZ declaration should have only one active CusEntryHeader
			entryHeader1.CH_IsActive = false;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "21212121";
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"21212121"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			// test with cancelled entry
			entryHeader2.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCancelled;
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(Array.Empty<string>(), entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var otherDeclaration = Factory.New<JobDeclaration>();
			var otherEntryHeader = otherDeclaration.CusEntryHeader;
			otherEntryHeader.EntryNumber = "55555555";
			entryNumbers.Load(((ICusEntryNumFilterProvider)otherDeclaration).ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"55555555"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var otherEntryNum = Factory.New<CusEntryNumber>();
			otherEntryNum.CE_ParentID = otherEntryHeader.PK;
			otherEntryNum.CE_EntryNum = "66666667";
			entryNumbers.Load(((ICusEntryNumFilterProvider)otherDeclaration).ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"55555555",
				"66666667"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());
		}

		public void TestIsTSWMessagingValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert(!declaration.IsTSWMessagingValidation);
			using (declaration.TemporarilySetTSWMessagingValidation())
			{
				Assert(declaration.IsTSWMessagingValidation);
				using (declaration.TemporarilySetTSWMessagingValidation())
				{
					Assert(declaration.IsTSWMessagingValidation);
				}
				Assert(declaration.IsTSWMessagingValidation);
			}
			Assert(!declaration.IsTSWMessagingValidation);
		}

		public void TestGetAppropriateCusEntryHeaderIfExists()
		{
			var entryHeader1 = Declaration.CusEntryHeader;
			entryHeader1.CH_IsActive = true;
			var entryHeader2 = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_IsActive = false;
			var entryHeader3 = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_IsActive = false;
			entryHeader3.EntryNumber = "1234";
			var entryHeader4 = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_IsActive = false;
			entryHeader4.Messages.AddNew();
			var entryHeader5 = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader5.CH_IsActive = true;

			Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCC; // Declaration IsTSWCancellation = false

			entryHeader2.CH_IsActive = false;
			var entryHeader = Declaration.GetAppropriateCusEntryHeaderIfExists();
			AssertEquals("Should return active header", entryHeader1.PK, entryHeader.PK);
			AssertEquals("Should delete headers with EntryNumber empty", true, entryHeader2.IsDeleted);
			AssertEquals("Should not delete headers with EntryNumber not empty", false, entryHeader3.IsDeleted);
			AssertEquals("Should not delete headers with Messages", false, entryHeader4.IsDeleted);
			AssertEquals("Should not delete headers with EntryNumber empty even when it is active", true, entryHeader5.IsDeleted);
		}

		#endregion

		#region Implementation

		int decBindingList_ListChangedCount;
		void decBindingList_ListChanged(object sender, ListChangedEventArgs e)
		{
			decBindingList_ListChangedCount++;
		}

		bool sendMCDContainerQuarantineDeclarationRefreshed;
		void JE_SendMCDContainerQuarantineDeclarationInfo_ValueChanged(object sender, EventArgs e)
		{
			sendMCDContainerQuarantineDeclarationRefreshed = true;
		}

		bool haveMAFContainerDeclarationRefreshed;
		void JE_HaveMAFContainerDeclarationInfo_ValueChanged(object sender, EventArgs e)
		{
			haveMAFContainerDeclarationRefreshed = true;
		}

		bool isContainerCleanRefreshed;
		void JE_IsContainerCleanInfo_ValueChanged(object sender, EventArgs e)
		{
			isContainerCleanRefreshed = true;
		}

		bool isPackagingMaterialContaminatedRefreshed;
		void JE_IsPackagingMaterialContaminatedInfo_ValueChanged(object sender, EventArgs e)
		{
			isPackagingMaterialContaminatedRefreshed = true;
		}

		bool isWoodPackagingUsedRefreshed;
		void JE_IsWoodPackagingUsedInfo_ValueChanged(object sender, EventArgs e)
		{
			isWoodPackagingUsedRefreshed = true;
		}

		bool isWoodPackagingTreatedRefreshed;
		void JE_IsWoodPackagingTreatedInfo_ValueChanged(object sender, EventArgs e)
		{
			isWoodPackagingTreatedRefreshed = true;
		}

		bool isWoodPackagingTreatmentCertificateAvailableRefreshed;
		void JE_IsWoodPackagingTreatmentCertificateAvailableInfo_ValueChanged(object sender, EventArgs e)
		{
			isWoodPackagingTreatmentCertificateAvailableRefreshed = true;
		}

		ZString CheckDeclarationGetsRightMessageTypeFrom(OrgHeader importer, OrgHeader supplier, ZString expectedMessageType)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("declaration.JE_MessageType set Supplier First - " + "Supplier:" + supplier.OH_FullName + " - Importer:" + importer.OH_FullName, expectedMessageType, declaration.JE_MessageType);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("declaration.JE_MessageType set Importer First - " + "Supplier:" + supplier.OH_FullName + " - Importer:" + importer.OH_FullName, expectedMessageType, declaration.JE_MessageType);
			return expectedMessageType;
		}

		protected override bool WillDefaultMessageTypeDueToImporter => false;

		public JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = (JobDeclaration)GetJobDeclaration();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			BaseJobDeclaration result = (JobDeclaration)GetNewBusinessObject();
			result.DisableDefaultPackingInformation = true;
			return result;
		}

		protected override void CreatePartAndClassification(string partNum, string tariff, OrgHeader importer)
		{
			var part = Factory.New<Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = partNum;
			part.RelatedOrganisations.AddOwner(importer);

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_TariffNum = tariff;

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
		}

		protected override System.Collections.Hashtable ExpectedDocAddressTypes
		{
			get
			{
				if (expectedDocAddressTypes == null)
				{
					expectedDocAddressTypes = base.ExpectedDocAddressTypes;
					expectedDocAddressTypes.Add(DocAddressTypes.Codes.CustomsTreatmentProviderAddress, DocAddressType.CustomsTreatmentProviderAddress);
				}
				return expectedDocAddressTypes;
			}
		}
		System.Collections.Hashtable expectedDocAddressTypes;

		protected override string ExpectedPackingListDescription => "GOODS DESCRIPTION";

		protected override JobDeclaration JobDeclarationForAllEntriesClearedTest => GetJobDeclarationForTesting();

		JobDeclarationForTesting GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclarationForTesting>();
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}

		#endregion
	}

	[TestedType(typeof(JobDeclaration.DeclarationDocManagerInfo))]
	public class DeclarationDocManagerInfoTest : Enterprise.MasterFiles.Business.Testing.DocManagerInfoTestCase
	{
		public void TestMessagesWitheDocsGetIncludedAsChildren()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mafMessaging = MAFeBACCa.Testing.TestDataBuilder.GetMAFMessaging(declaration);
			var message1 = mafMessaging.Messages.AddNew();
			var message2 = mafMessaging.Messages.AddNew();
			message2.DocManagerInfo.AddFileOrDocument(new byte[] { 42 }, "Fred.PDF", "MCD");

			var info = declaration.DocManagerInfo;
			var relatedObjects = new List<BusinessObject>(info.RelatedObjects);
			AssertEquals("relatedObjects.Contains(message1)", false, relatedObjects.Contains(message1));
			AssertEquals("relatedObjects.Contains(message2)", true, relatedObjects.Contains(message2));
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<JobDeclaration>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.NewWithValidTestData<JobDeclaration>();
		}
	}

	#region JobDeclarationForTesting
	public class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocalCurrencyCodeCoreExposed
		{
			get { return LocalCurrencyCodeCore; }
		}

		public override Type TypeOfEntryHeaderRequiredForCurrentDeclarationSettings => typeof(Customs.Business.CusEntryHeader);
	}

	#endregion
}
