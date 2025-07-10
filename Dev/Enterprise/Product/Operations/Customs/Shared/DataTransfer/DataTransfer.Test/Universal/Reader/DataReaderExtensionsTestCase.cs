using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions.Testing
{
	sealed class DataReaderExtensionsTestCase : DataObjectReaderTest
	{
		public void TestGetResponsiblePartyID()
		{
			var factory = new UniversalObjectFactory();

			var org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.MainAddress.FillWithValidTestData();
			org1.PrimaryRegistrationNumber.Number = "121212";

			var company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = org1.PK;

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_Code = "YYY";

			factory.SaveForTesting();

			var logger = new DummyLogger();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.Branch = new Branch() { Code = "YYY" };
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetCompanyAndDataProviderDetails(company);

			AssertEquals("121212", shipment.GetResponsiblePartyID(logger, factory));

			var org = factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.FillWithValidTestData();
			org.PrimaryRegistrationNumber.Number = "1234567";

			var writeManager = new DataWritingManager(new ActionInfo(null, factory.New<DummyBusinessObject>()));
			var responsibleParty = new OrganizationDataObjectWriter(writeManager, AddressTypes.ResponsibleParty, null).GetDataObject(org.MainAddress);

			shipment.SetOrganizationAddressCollection(() => new System.Collections.Generic.List<OrganizationAddress>());
			shipment.OrganizationAddressCollection.Add(responsibleParty);

			AssertEquals("1234567", shipment.GetResponsiblePartyID(logger, factory));

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference()
			{
				Type = new EntryType() { Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID },
				ReferenceNumber = "7654321"
			});

			AssertEquals("7654321", shipment.GetResponsiblePartyID(logger, factory));
		}

		public void TestGetMatchedOrganisationData()
		{
			var org1 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var org3 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var declaration = Factory.New<BaseJobDeclaration>();
			var writeManager = new DataWritingManager(new ActionInfo(null, declaration));
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.AddOrgAddress(writeManager, org2, "DUMMY ADDRESS 2");
			shipmentData.AddOrgAddress(writeManager, org3, "DUMMY ADDRESS 3");
			shipmentData.AddOrgAddress(writeManager, org1, "DUMMY ADDRESS 1");
			var mockSupporter = new Mock<IOrganisationDataObjectReaderSupporter>();
			mockSupporter
				.Setup(m => m.CreateNewReader(It.IsAny<OrganizationAddress>()))
				.Returns<OrganizationAddress>((reader) => new OrganisationDataObjectReader(reader, Logger, Factory));
			var supporter = mockSupporter.Object;

			AssertEquals(false, supporter.TryGetMatchedOrganisationData(out var organisationPK, out var addressPK, shipmentData, declaration, "DUMMY ADDRESS", OrganisationTypes.None));
			AssertEquals(ZGuid.Empty, organisationPK);
			AssertEquals(ZGuid.Empty, addressPK);

			AssertEquals(true, supporter.TryGetMatchedOrganisationData(out organisationPK, out addressPK, shipmentData, declaration, "DUMMY ADDRESS 3", OrganisationTypes.None));
			AssertEquals(org3.PK, organisationPK);
			AssertEquals(org3.MainAddress.PK, addressPK);

			AssertEquals(true, supporter.TryGetMatchedOrganisationData(out organisationPK, out addressPK, shipmentData, declaration, "DUMMY ADDRESS 1", OrganisationTypes.None));
			AssertEquals(org1.PK, organisationPK);
			AssertEquals(org1.MainAddress.PK, addressPK);

			AssertEquals(true, supporter.TryGetMatchedOrganisationData(out organisationPK, out addressPK, shipmentData, declaration, "DUMMY ADDRESS 2", OrganisationTypes.None));
			AssertEquals(org2.PK, organisationPK);
			AssertEquals(org2.MainAddress.PK, addressPK);
			mockSupporter.VerifyAll();
		}

		public void TestGetMatchedOrganisation()
		{
			var org1 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var org3 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var declaration = Factory.New<BaseJobDeclaration>();
			var writeManager = new DataWritingManager(new ActionInfo(null, declaration));
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentData.AddOrgAddress(writeManager, org2, "DUMMY ADDRESS 2");
			shipmentData.AddOrgAddress(writeManager, org3, "DUMMY ADDRESS 3");
			shipmentData.AddOrgAddress(writeManager, org1, "DUMMY ADDRESS 1");
			var mockSupporter = new Mock<IOrganisationDataObjectReaderSupporter>();
			mockSupporter
				.Setup(m => m.CreateNewReader(It.IsAny<OrganizationAddress>()))
				.Returns<OrganizationAddress>((reader) => new OrganisationDataObjectReader(reader, Logger, Factory));
			var supporter = mockSupporter.Object;
			AssertEquals(false, supporter.TryGetMatchedOrganisation(out var addressBO, shipmentData, declaration, "DUMMY ADDRESS", OrganisationTypes.None));
			AssertNull(addressBO);
			AssertEquals(true, supporter.TryGetMatchedOrganisation(out addressBO, shipmentData, declaration, "DUMMY ADDRESS 3", OrganisationTypes.None));
			AssertEquals(org3.MainAddress, addressBO);
			AssertEquals(true, supporter.TryGetMatchedOrganisation(out addressBO, shipmentData, declaration, "DUMMY ADDRESS 1", OrganisationTypes.None));
			AssertEquals(org1.MainAddress, addressBO);
			AssertEquals(true, supporter.TryGetMatchedOrganisation(out addressBO, shipmentData, declaration, "DUMMY ADDRESS 2", OrganisationTypes.None));
			AssertEquals(org2.MainAddress, addressBO);
			mockSupporter.VerifyAll();
		}

		public void TestHandlingUnmatchNoteInvoice()
		{
			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consignee = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				consignee.OH_IsConsignee = ZBool.True;
				var consignor = OrganizationAddressTestHelper.GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				consignor.OH_IsConsignor = ZBool.True;
				Factory.SaveForTesting();

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
				invoiceData.Buyer = invoiceData.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
				invoiceData.Supplier = invoiceData.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
				var manufacturerData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.Manufacturer),
					CompanyName = "BOB THE BUILDER",
					Address1 = "BOB'S ADDRESS 1"
				};
				invoiceData.OrganizationAddressCollection.Add(manufacturerData);
				invoiceData.InvoiceNumber = "INVTEST1";
				invoiceData.BillNumber = "HB1";
				invoiceData.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };

				var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						Name = "GROUP1",
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
					}
				};

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalShipmentMessage(shipmentData);
				manager.Process(message);

				var newFactory = new BusinessObjectFactory();
				var query = new ZQuery(JobComInvoiceHeaderSchema.JZ_OH_Buyer, consignee.PK);
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, consignor.PK);
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "INVTEST1");
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
				var invoices = newFactory.Load<BaseJobComInvoiceHeader>(query);
				AssertEquals("invoices.Length", 1, invoices.Length);
				var invoice = invoices[0];
				var foundNotes = invoice.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
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
				AssertMultilineASCIIEquals("Note text should describe TOPGROUPINV1 organizations", serialisedNoteText, foundNotes[0].ST_NoteText.TrimEnd());
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Invoice INVTEST1 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Successfully saved Invoice INVTEST1 (SUP:WUFSHIJNB IMP:CRAHOLSYD).
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
No matching JobComInvoiceHeader found, creating new JobComInvoiceHeader.
Populating JobComInvoiceHeader...
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'Manufacturer':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Added Invoice INVTEST1 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Successfully saved Invoice INVTEST1 (SUP:WUFSHIJNB IMP:CRAHOLSYD).
".Trim(), logNoteText);

				invoiceData.OrganizationAddressCollection.Remove(manufacturerData);
				invoiceData.AddOrgAddress(writeManager, consignor, DocAddressType.Manufacturer);
				message = GetQueuedUniversalShipmentMessage(shipmentData);
				serviceTaskLog = new ServiceTaskLogForTesting();
				manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				newFactory = new BusinessObjectFactory();
				invoices = newFactory.Load<BaseJobComInvoiceHeader>(query);
				AssertEquals("invoices.Length", 1, invoices.Length);
				invoice = invoices[0];
				foundNotes = invoice.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("There should be no UNMATCHED note", 0, foundNotes.Length);
				AssertMultilineASCIIEquals("Service Task Log", @"
Updated Invoice INVTEST1 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Successfully saved Invoice INVTEST1 (SUP:WUFSHIJNB IMP:CRAHOLSYD).
".Trim(), serviceTaskLog.ToString());

				logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Successfully loaded matching JobComInvoiceHeader.
Populating JobComInvoiceHeader...
Matching 'SupplierDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Matching 'BuyerDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Matching 'Manufacturer':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Updated Invoice INVTEST1 (SUP:WUFSHIJNB IMP:CRAHOLSYD) from UniversalShipment.
Successfully saved Invoice INVTEST1 (SUP:WUFSHIJNB IMP:CRAHOLSYD).
".Trim(), logNoteText);
			}
		}

		public void TestGetCustomsBillTye()
		{
			var code = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			AssertEquals("Master", BillTypeList.Codes.MasterBill, code.GetCustomsBillType());
			code.Code = WayBillTypeList.Codes.House;
			AssertEquals("House", BillTypeList.Codes.HouseBill, code.GetCustomsBillType());
			code.Code = WayBillTypeList.Codes.SubHouse;
			AssertEquals("SubHouse", BillTypeList.Codes.SubHouseBill, code.GetCustomsBillType());
			code.Code = WayBillTypeList.Codes.House.ToLower();
			AssertEquals("House", BillTypeList.Codes.HouseBill, code.GetCustomsBillType());
			code.Code = "Z#";
			AssertEquals("Unknown", "Z#", code.GetCustomsBillType());
		}

		public void TestGetTargetCountryCodeAndSourceContryCode()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			company.GC_Name = "DUMMY COMPANY";
			company.GC_Code = "Z#Z";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Zimbabwe))
			{
				shipment.DataContext = DataContextFactory.New();
				AssertEquals(Core.Constants.CountryCodes.Zimbabwe, shipment.GetTargetCountryCode());
				AssertEquals(Core.Constants.CountryCodes.Zimbabwe, shipment.GetSourceCountryCode());

				shipment.DataContext.SetCompanyAndDataProviderDetails(company);
				shipment.DataContext.CodesMappedToTarget = ZBool.True;
				AssertEquals("TargetCountry should be the environment country", Core.Constants.CountryCodes.Zimbabwe, shipment.GetTargetCountryCode());
				AssertEquals("XML country code", Core.Constants.CountryCodes.Singapore, shipment.GetSourceCountryCode());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				shipment.DataContext = DataContextFactory.New();
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, shipment.GetTargetCountryCode());
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, shipment.GetSourceCountryCode());

				shipment.DataContext.SetCompanyAndDataProviderDetails(company);
				shipment.DataContext.CodesMappedToTarget = ZBool.True;
				AssertEquals("TargetCountry should be the environment country", Core.Constants.CountryCodes.UnitedStates, shipment.GetTargetCountryCode());
				AssertEquals("XML country code", Core.Constants.CountryCodes.Singapore, shipment.GetSourceCountryCode());

				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
				shipment.DataContext.SetCompanyAndDataProviderDetails(company);
				shipment.DataContext.CodesMappedToTarget = ZBool.True;
				AssertEquals("TargetCountry should be the environment country", Core.Constants.CountryCodes.UnitedStates, shipment.GetTargetCountryCode());
				AssertEquals("XML country code", Core.Constants.CountryCodes.UnitedStates, shipment.GetSourceCountryCode());
			}
		}

		public void TestGetBranchPK()
		{
			CombineAssertions(() =>
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.DataContext = DataContextFactory.New();
				AssertEquals("No Branch or EventBranchCode", ZGuid.Empty, shipment.GetBranchPK(Factory.BOFactory));

				shipment.Branch = new Branch() { Code = "Z!Z", Name = "DUMMY BRANCH" };
				AssertEquals("Branch is invalid", ZGuid.Invalid, shipment.GetBranchPK(Factory.BOFactory));

				shipment.Branch.Code = GlbBranch.CurrentBranch.GB_Code;
				AssertEquals("Branch is valid", GlbBranch.CurrentBranch.PK, shipment.GetBranchPK(Factory.BOFactory));

				var anotherBranchInCurrentCompany = Factory.New<GlbBranch>();
				anotherBranchInCurrentCompany.GB_Code = "BR2";
				anotherBranchInCurrentCompany.GB_GC = GlbCompany.CurrentCompany.PK;

				shipment.Branch = null;
				((DataContext)shipment.DataContext).EventBranch = new Branch { Code = "BR2", Name = "DUMMY BRANCH 2" };
				AssertEquals("Fall back to EventBranchCode", anotherBranchInCurrentCompany.PK, shipment.GetBranchPK(Factory.BOFactory));

				var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
				var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
				otherBranch.GB_GC = otherCompany.PK;
				shipment.Branch = new Branch { Code = otherBranch.GB_Code };
				AssertEquals("GetBranchPK will only match to current company", ZGuid.Invalid, shipment.GetBranchPK(Factory.BOFactory));
			});
		}

		public void TestFindCusDecHouseBillByBillNumberAndType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "NO1";
			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.CU_BillNum = "NO1";
			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.CU_BillNum = "NO2";
			var subHouseBill = houseBill1.ChildBills.AddNew();
			subHouseBill.CU_BillNum = "NO1";
			var rowFactory = Factory.RowFactory;
			AssertEquals(((IBusinessObjectInternals)houseBill1).Row, rowFactory.FindFirstCusDecHouseBillByBillNumberAndType(declaration.PK, "NO1", BillTypeList.Codes.HouseBill));
			AssertEquals(((IBusinessObjectInternals)houseBill2).Row, rowFactory.FindFirstCusDecHouseBillByBillNumberAndType(declaration.PK, "NO2", BillTypeList.Codes.HouseBill));
			AssertEquals(((IBusinessObjectInternals)masterBill).Row, rowFactory.FindFirstCusDecHouseBillByBillNumberAndType(declaration.PK, "NO1", BillTypeList.Codes.MasterBill));
			AssertEquals(((IBusinessObjectInternals)subHouseBill).Row, rowFactory.FindFirstCusDecHouseBillByBillNumberAndType(declaration.PK, "NO1", BillTypeList.Codes.SubHouseBill));
			AssertNull(rowFactory.FindFirstCusDecHouseBillByBillNumberAndType(declaration.PK, "NO2", BillTypeList.Codes.SubHouseBill));

			houseBill1.CU_BillNum = ZString.Empty;
			houseBill2.CU_BillNum = ZString.Empty;
			AssertEquals(2, rowFactory.FindCusDecHouseBillByBillNumberAndType(declaration.PK, ZString.Empty, BillTypeList.Codes.HouseBill).Count());
		}

		public void TestClearUnmatchedOrgDetailsNotes()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var note1 = declaration.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			var note2 = declaration.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			var note3 = declaration.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			Factory.ClearUnmatchedOrgDetailsNotes(declaration.PK, declaration.TableName);
			AssertEquals(false, note2.IsDeleted);
			AssertEquals(true, note3.IsDeleted);
			AssertEquals(true, note3.IsDeleted);
		}
	}
}
