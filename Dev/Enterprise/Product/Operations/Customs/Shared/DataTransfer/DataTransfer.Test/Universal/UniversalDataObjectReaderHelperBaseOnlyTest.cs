using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class UniversalDataObjectReaderHelperBaseOnlyTest : UniversalDataObjectReaderHelperAbstractTest
	{
		public void TestGetMatchingKeys()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00000001");

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = new CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() }
			};

			var invoiceCollection = declarationDataObject.CommercialInfo.CommercialInvoiceCollection;

			invoiceCollection.Add(new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV0001",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
				new DataObjectList<CommercialInvoiceLine>(new[]
				{
						new CommercialInvoiceLine() { LineNo = 1, DataImportMatchingKey = "MK0001" },
						new CommercialInvoiceLine() { LineNo = 2, DataImportMatchingKey = string.Empty },
						new CommercialInvoiceLine() { LineNo = 3, DataImportMatchingKey = "MK0002" },
						new CommercialInvoiceLine() { LineNo = 4, DataImportMatchingKey = "MK0002" },
						new CommercialInvoiceLine() { LineNo = 5 }
				})))
			);

			var helper = new UniversalDataObjectReaderHelper(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var expectedKeys = new ZString[] { "MK0001", "MK0002", "MK0002" };
			var actualKeys = helper.GetDataImportMatchingKeys(invoiceCollection[0]);

			AssertArrayEqualsByElements("Should get all matching keys.", expectedKeys, actualKeys);
			AssertSame("Should cached the result.", helper.GetDataImportMatchingKeys(invoiceCollection[0]), actualKeys);
		}

		public void TestGetDataImportMatchingKeys_ShouldReturnEmptySet_WhenCommercialInvoiceLineCollectionNotInit()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00000001");

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = new CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() }
			};

			var invoiceCollection = declarationDataObject.CommercialInfo.CommercialInvoiceCollection;

			invoiceCollection.Add(new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV0001",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => null)));

			var helper = new UniversalDataObjectReaderHelper(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var actualKeys = helper.GetDataImportMatchingKeys(invoiceCollection[0]);

			AssertNotNull("Should return empty result set", actualKeys);
		}

		public void TestGetMatchKeyOrderDictionaryCached()
		{
			var orgheader = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var helper = new UniversalDataObjectReaderHelper(Factory, "ER", "ER");

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001111");
			declarationDataObject.DataContext = dataContext;
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001112";

			var dic1 = helper.GetMatchKeyOrderDictionary("B00001111", declarationDataObject, declaration, orgheader);
			var dic2 = helper.GetMatchKeyOrderDictionary("B00001111", declarationDataObject, declaration, orgheader);

			Assert("Dictionary Cached", object.ReferenceEquals(dic1, dic2));
		}

		public void TestGetCustomsUnitForPackType()
		{
			var helper = new UniversalDataObjectReaderHelper(Factory, "ER", "ER");
			AssertNull(helper.GetCustomsUnitForPackType(null));
			var packType = new PackageType();
			packType.Code = null;
			AssertNull(helper.GetCustomsUnitForPackType(packType));
			packType.Code = "AB";
			AssertEquals("AB", helper.GetCustomsUnitForPackType(packType));
			packType.Code = ZString.Empty;
			AssertEquals(ZString.Empty, helper.GetCustomsUnitForPackType(packType));
		}

		public void TestDeleteAll()
		{
			var newFactory = new BusinessObjectFactory();
			var dec = newFactory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var undg1 = invoiceLine.UNDGs.AddNew();
			undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(newFactory, "1968", "", "IMO").First().PK;
			var undg2 = invoiceLine.UNDGs.AddNew();
			undg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(newFactory, "3477", "B", "IMO").First().PK;
			var undg3 = invoiceLine.UNDGs.AddNew();
			undg3.DI_DG = UNDGSubstanceLoader.LoadSubstances(newFactory, "3475", "", "IMO").First().PK;
			var undg4 = invoiceLine.UNDGs.AddNew();
			undg4.DI_DG = UNDGSubstanceLoader.LoadSubstances(newFactory, "1033", "", "IMO").First().PK;
			var undg5 = invoiceLine.UNDGs.AddNew();
			undg5.DI_DG = UNDGSubstanceLoader.LoadSubstances(newFactory, "2031", "b", "IMO").First().PK;
			var undg6 = invoiceLine.UNDGs.AddNew();
			undg6.DI_DG = UNDGSubstanceLoader.LoadSubstances(newFactory, "0033", "", "IMO").First().PK;
			newFactory.Save();

			var helper = new UniversalDataObjectReaderHelper(Factory, "US", "US");
			var query = new ZQuery(UNDGDataItemSchema.DI_ParentID, invoiceLine.PK);
			query.AddToFilter(UNDGDataItemSchema.DI_ParentTableCode, invoiceLine.TablePrefix);
			helper.DeleteAll(UNDGDataItemSchema.Instance, query);
			Factory.SaveForTesting();
			newFactory = new BusinessObjectFactory();
			AssertNull(newFactory.Load<UNDGDataItem>(undg1.PK));
			AssertNull(newFactory.Load<UNDGDataItem>(undg2.PK));
			AssertNull(newFactory.Load<UNDGDataItem>(undg3.PK));
			AssertNull(newFactory.Load<UNDGDataItem>(undg4.PK));
			AssertNull(newFactory.Load<UNDGDataItem>(undg5.PK));
			AssertNull(newFactory.Load<UNDGDataItem>(undg6.PK));
		}

		public void TestLoad()
		{
			var newFactory = new BusinessObjectFactory();
			var dec = newFactory.New<BaseJobDeclaration>();
			var invoice1 = dec.Invoices.AddNew();
			var invoice2 = dec.Invoices.AddNew();
			var invoice3 = dec.Invoices.AddNew();
			newFactory.Save();

			var helper = new UniversalDataObjectReaderHelper(Factory, "US", "US");
			var query = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, dec.PK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
			var rows = ConvertToColumnIndexer(helper.Load(JobComInvoiceHeaderSchema.Instance, query));
			var list = new List<ZGuid>(rows.Select(x => x.GetValue(JobComInvoiceHeaderSchema.PK)));
			AssertEquals(3, list.Count);
			AssertEquals(true, list.Contains(invoice1.PK));
			AssertEquals(true, list.Contains(invoice2.PK));
			AssertEquals(true, list.Contains(invoice3.PK));
		}

		public void TestGetJobComInvoiceLineCustomLabelsProvider()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineRow = (IColumnIndexer)((IBusinessObjectInternals)invoiceLine).Row;
			var helper = new UniversalDataObjectReaderHelper(Factory, "US", "US");
			var provider = helper.GetJobComInvoiceLineCustomLabelsProvider(invoiceLineRow);
			AssertEquals(typeof(BaseJobComInvoiceLine.CustomLabelsProvider), provider.GetType());
		}

		public void TestGetCusContainerCustomLabelsProvider()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var container = dec.CusContainers.AddNew();
			var containerRow = (IColumnIndexer)((IBusinessObjectInternals)container).Row;
			var helper = new UniversalDataObjectReaderHelper(Factory, "US", "US");
			var provider = helper.GetCusContainerCustomLabelsProvider(containerRow);
			AssertEquals(typeof(BaseCusContainer.CustomLabelsProvider), provider.GetType());
		}

		public void TestIsSourceAndTargetCountrySame()
		{
			AssertEquals(true, new UniversalDataObjectReaderHelper(Factory, "US", "US").IsSourceAndTargetCountrySame);
			AssertEquals(false, new UniversalDataObjectReaderHelper(Factory, "US", "AU").IsSourceAndTargetCountrySame);
		}

		public void TestEntryNumberToEntryLineMapDictionary()
		{
			var helper = new UniversalDataObjectReaderHelper(Factory, "US", "CN");

			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_LineNumber = 1;
			var entryLine3 = Factory.New<CusEntryLine>();
			entryLine3.CL_LineNumber = 2;
			var entryLine4 = Factory.New<CusEntryLine>();
			entryLine4.CL_LineNumber = 1;
			var entryLine5 = Factory.New<CusEntryLine>();
			entryLine5.CL_LineNumber = 2;
			helper.AddEntryLineMap(entryLine1, "EXP", "1111111");
			helper.AddEntryLineMap(entryLine2, "IMP", "1111111");
			helper.AddEntryLineMap(entryLine3, "IMP", "2222222");
			helper.AddEntryLineMap(entryLine4, "IMP", "2222222");
			helper.AddEntryLineMap(entryLine5, "EXP", "1111111");

			var entryLines = helper.GetEntryLines("1111111", 1);
			AssertEquals(2, entryLines.Length);
			AssertEquals(entryLine1, entryLines[0]);
			AssertEquals(entryLine2, entryLines[1]);
			entryLines = helper.GetEntryLines("1111111", 2);
			AssertEquals(1, entryLines.Length);
			AssertEquals(entryLine5, entryLines[0]);
			entryLines = helper.GetEntryLines("2222222", 1);
			AssertEquals(entryLine4, entryLines[0]);
			entryLines = helper.GetEntryLines("2222222", 2);
			AssertEquals(entryLine3, entryLines[0]);
			AssertEquals(0, helper.GetEntryLines("2222222", 3).Length);
		}

		[ExpectNoExceptions]
		public void TestDeletedLinesAreIgnored()
		{
			var helper = new UniversalDataObjectReaderHelper(Factory, "US", "CN");

			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_LineNumber = 1;

			helper.AddEntryLineMap(entryLine1, "EXP", "1111111");

			entryLine1.Delete();

			var entryLines = helper.GetEntryLines("1111111", 1);
			AssertEquals(0, entryLines.Length);
		}

		public void TestGetOrganisationPKAndGetAddressPK()
		{
			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			#region Setup Organization

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var buyerOrg = Factory.New<OrgHeader>();
			buyerOrg.OH_Code = "INCBuyer";
			invoice.JZ_OH_Buyer = buyerOrg.PK;

			var supplierOrg = Factory.New<OrgHeader>();
			supplierOrg.OH_Code = "INCSUPPILER";
			invoice.JZ_OH_Supplier = supplierOrg.PK;

			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;

			#endregion

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration));
			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.Buyer, "Buyer");
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.Supplier, "Supplier");
			invoiceHeaderDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.SellingParty),
				CompanyName = "BOB THE BUILDER",
				Address1 = "BOB'S ADDRESS 1"
			});

			Factory.SaveForTesting();

			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceHeaderDataObject, Logger, new UniversalDataObjectReaderHelper(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode), topGroupInvoice);
			var helper = new UniversalDataObjectReaderHelper(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var buyerPK = helper.GetOrganisationPK(reader, invoiceHeaderDataObject, invoice, "Buyer", OrganisationTypes.None);
			AssertEquals(buyerOrg.PK, buyerPK);
			var buyerAddrssPK = helper.GetAddressPK(reader, invoiceHeaderDataObject, invoice, "Buyer", OrganisationTypes.None);
			AssertEquals(buyerOrg.MainAddress.PK, buyerAddrssPK);
			var supplierPK = helper.GetOrganisationPK(reader, invoiceHeaderDataObject, invoice, "Supplier", OrganisationTypes.None);
			AssertEquals(supplierOrg.PK, supplierPK);
			var supplierAddressPK = helper.GetAddressPK(reader, invoiceHeaderDataObject, invoice, "Supplier", OrganisationTypes.None);
			AssertEquals(supplierOrg.MainAddress.PK, supplierAddressPK);

			// Unmatch test
			var foundNotes = invoice.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("No unmatched note", 0, foundNotes.Length);
			var unmatchOrg = OrgHeader.UnmatchOrg(Factory.BOFactory);
			var sellerPK = helper.GetOrganisationPK(reader, invoiceHeaderDataObject, invoice, nameof(DocAddressType.SellingParty), OrganisationTypes.Consignor);
			AssertEquals(unmatchOrg.PK, sellerPK);
			foundNotes = invoice.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("There should be one UNMATCHED note", 1, foundNotes.Length);
			ZString serialisedNoteText = @"
Organisation Type: Consignor
Owner Code: 
EDI Code: 
Organisation Name: BOB THE BUILDER
Address Line 1: BOB'S ADDRESS 1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type:".Trim();
			AssertMultilineASCIIEquals("Note text", serialisedNoteText, foundNotes[0].ST_NoteText.TrimEnd());

			var sellerAddressPK = helper.GetAddressPK(reader, invoiceHeaderDataObject, invoice, nameof(DocAddressType.SellingParty), OrganisationTypes.Sales);
			AssertEquals(unmatchOrg.MainAddress.PK, sellerAddressPK);
			foundNotes = invoice.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("There should be one UNMATCHED note", 1, foundNotes.Length);
			serialisedNoteText = @"
Organisation Type: Consignor
Owner Code: 
EDI Code: 
Organisation Name: BOB THE BUILDER
Address Line 1: BOB'S ADDRESS 1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 
Organisation Type: Sales
Owner Code: 
EDI Code: 
Organisation Name: BOB THE BUILDER
Address Line 1: BOB'S ADDRESS 1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type:".Trim();
			AssertMultilineASCIIEquals("Note text", serialisedNoteText, foundNotes[0].ST_NoteText.TrimEnd());

			sellerPK = helper.GetOrganisationPK(reader, invoiceHeaderDataObject, invoice, nameof(DocAddressType.SellingParty), OrganisationTypes.Creditor, "BOB CREDITOR");
			AssertEquals(unmatchOrg.PK, sellerPK);
			sellerAddressPK = helper.GetAddressPK(reader, invoiceHeaderDataObject, invoice, nameof(DocAddressType.SellingParty), OrganisationTypes.Debtor, "BOB DEBTOR");
			AssertEquals(unmatchOrg.MainAddress.PK, sellerAddressPK);
			foundNotes = invoice.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("There should be one UNMATCHED note", 1, foundNotes.Length);
			serialisedNoteText = @"
Organisation Type: Consignor
Owner Code: 
EDI Code: 
Organisation Name: BOB THE BUILDER
Address Line 1: BOB'S ADDRESS 1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 
Organisation Type: Sales
Owner Code: 
EDI Code: 
Organisation Name: BOB THE BUILDER
Address Line 1: BOB'S ADDRESS 1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 
Organisation Type: BOB CREDITOR
Owner Code: 
EDI Code: 
Organisation Name: BOB THE BUILDER
Address Line 1: BOB'S ADDRESS 1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 
Organisation Type: BOB DEBTOR
Owner Code: 
EDI Code: 
Organisation Name: BOB THE BUILDER
Address Line 1: BOB'S ADDRESS 1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
".Trim();
			AssertMultilineASCIIEquals("Note text", serialisedNoteText, foundNotes[0].ST_NoteText.TrimEnd());
		}

		public void TestEntryInstructionLinkDictionary()
		{
			var testHelper = new UniversalDataObjectReaderHelper(Factory, "ZA", "ZA", null);
			var id1 = ZGuid.NewZGuid();
			var id2 = ZGuid.NewZGuid();
			var id3 = ZGuid.NewZGuid();
			testHelper.RegisterEntryInstructionPK(1, id1);
			testHelper.RegisterEntryInstructionPK(2, id2);
			testHelper.RegisterEntryInstructionPK(2, id3);

			CombineAssertions(() =>
			{
				AssertEquals(null, testHelper.GetEntryInstructionPK(null));
				AssertEquals(null, testHelper.GetEntryInstructionPK(0));
				AssertEquals(id1, testHelper.GetEntryInstructionPK(1));
				AssertEquals(id3, testHelper.GetEntryInstructionPK(2));
				AssertEquals(null, testHelper.GetEntryInstructionPK(3));
			});
		}

		public void TestLoadOrCreateStmNoteForReaderUpdate()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var dec2 = Factory.New<BaseJobDeclaration>();
			var helper = new UniversalDataObjectReaderHelper(Factory, "US", "US");

			var existingVisibleNote = dec.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description, "TEST1");
			AssertEquals(1, dec.Notes.VisibleNotes.Count);
			AssertEquals(0, dec2.Notes.VisibleNotes.Count);
			var decNote = helper.LoadOrCreateStmNoteForReaderUpdate(dec.PK, JobDeclarationSchema.Constants.TableName, dec.IsInDatabase, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description);
			var decNote2 = helper.LoadOrCreateStmNoteForReaderUpdate(dec2.PK, JobDeclarationSchema.Constants.TableName, dec2.IsInDatabase, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description);

			AssertEquals(nameof(StmNoteVisibility.DOC), decNote.ST_NoteType);
			AssertEquals(ZString.Empty, decNote.ST_NoteText);
			AssertEquals(1, dec.Notes.VisibleNotes.Count);
			AssertNotEquals(existingVisibleNote.PK, decNote.PK);

			AssertEquals(nameof(StmNoteVisibility.DOC), decNote2.ST_NoteType);
			AssertEquals(ZString.Empty, decNote2.ST_NoteText);
			AssertEquals(0, dec2.Notes.VisibleNotes.Count);

			var decNote3 = helper.LoadOrCreateStmNoteForReaderUpdate(dec.PK, JobDeclarationSchema.Constants.TableName, dec.IsInDatabase, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description);
			AssertSame("The existing note should be load", decNote3, decNote3);

			Factory.SaveForTesting();

			var anotherFactory = new UniversalObjectFactory();
			var anotherHelper = new UniversalDataObjectReaderHelper(anotherFactory, "US", "US");
			var decNoteInAnotherFactory = anotherHelper.LoadOrCreateStmNoteForReaderUpdate(dec.PK, JobDeclarationSchema.Constants.TableName, dec.IsInDatabase, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description);
			AssertEquals("The existing note should be load", decNote.PK, decNoteInAnotherFactory.PK);
		}

		public void TestGetFreightUnitForPackType()
		{
			var refPack = Factory.New<BaseRefPacks>();
			refPack.RP_Type = RPTypeList.Codes.PackingDeclaration;
			refPack.RP_CustomsCountry = Core.Constants.CountryCodes.Austria;
			refPack.RP_CustomsPack = "PK";
			refPack.RP_CommercialPack = "CTN";
			var helperAT2FN = new DataObjectReaderHelperForTest(Factory, Core.Constants.CountryCodes.Finland, Core.Constants.CountryCodes.Austria);
			var helperFN2AT = new DataObjectReaderHelperForTest(Factory, Core.Constants.CountryCodes.Austria, Core.Constants.CountryCodes.Finland);
			CombineAssertions(() =>
			{
				ZString? packType = null;
				Assert("null parameter in AT2FN should has no value", !helperAT2FN.GetFreightUnitForPackType(packType).HasValue);
				Assert("null parameter in FN2AT should has no value", !helperFN2AT.GetFreightUnitForPackType(packType).HasValue);
				packType = "PK";
				AssertEquals("PK in FN2AT should be CTN", "CTN", helperFN2AT.GetFreightUnitForPackType(packType).Value);
				AssertEquals("PK in AT2FN should be PK", "PK", helperAT2FN.GetFreightUnitForPackType(packType).Value);

				refPack.RP_OH_Supplier = ZGuid.Empty;
				AssertEquals("PK in FN2AT(when RP_OH_Supplier=ZGuid.Empty) should be CTN", "CTN", helperFN2AT.GetFreightUnitForPackType(packType).Value);

				refPack.RP_OH_Supplier = ZGuid.NewZGuid();
				AssertEquals("PK in FN2AT(when RP_OH_Supplier=ZGuid.NewZGuid()) should be PK", "PK", helperFN2AT.GetFreightUnitForPackType(packType).Value);
			});
		}

		public void TestGetCustomsBillType()
		{
			var helper = new UniversalDataObjectReaderHelper(Factory, "US", "US");
			AssertEquals("HB", helper.GetCustomsBillType(new WayBillType { Code = "HWB" }));
			AssertEquals("MB", helper.GetCustomsBillType(new WayBillType { Code = "MWB" }));
			AssertEquals("C55", helper.GetCustomsBillType(new WayBillType { Code = "C55" }));

			helper = new DataObjectReaderHelperForTest(Factory, "TW");
			AssertEquals("CN", helper.GetCustomsBillType(new WayBillType { Code = "C55" }));
		}

		sealed class DataObjectReaderHelperForTest : UniversalDataObjectReaderHelper
		{
			internal DataObjectReaderHelperForTest(UniversalObjectFactory factory, ZString sourceCountryCode, string dataProviderForCodeMapping = null)
				: this(factory, sourceCountryCode, sourceCountryCode, dataProviderForCodeMapping)
			{
			}

			DataObjectReaderHelperForTest(UniversalObjectFactory factory, ZString targetCountryCode, ZString sourceCountryCode, string dataProviderForCodeMapping = null)
				: base(factory, targetCountryCode, sourceCountryCode, dataProviderForCodeMapping)
			{
			}

			public override ZString GetCustomsBillType(WayBillType wayBillType)
			{
				return wayBillType.Code.HasValue && wayBillType.Code.Value == "C55" ? new ZString("CN") : base.GetCustomsBillType(wayBillType);
			}
		}
	}
}
