using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationDocumentSupporterInternalTest : TestCaseWithFactory
	{
		public void TestGetDataStateBeforeRun_CusEntryHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var menu = Factory.New<StmMenuItemBase>();
				menu.SU_MenuName = "Doc Print";
				var template = Factory.New<StmTemplateBase>();
				template.SO_DataContext = JobDeclarationSchema.Constants.TableName;
				var pivot = menu.Documents.AddNew();
				pivot.SI_SO = template.PK;
				var supporter = declaration.DocumentSupporter;
				var documentSupporterDataState = supporter.GetDataStateBeforeRun(menu);
				AssertEquals("", documentSupporterDataState.ErrorMessage);

				template = Factory.New<StmTemplateBase>();
				template.SO_DataContext = CusEntryHeaderSchema.Constants.TableName;
				pivot = menu.Documents.AddNew();
				pivot.SI_SO = template.PK;
				documentSupporterDataState = supporter.GetDataStateBeforeRun(menu);
				AssertEquals("Doc Print cannot be printed until the Declaration is Merged. Selecting Brokerage > Answer Declaration Questions will Merge the Declaration.", documentSupporterDataState.ErrorMessage);
			}
		}

		public void TestGetContactOrganisationFromShipment()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			var contact = importer.Contacts.AddNew();
			contact.OC_Email = "import@Doc.Support";

			var exporter = Factory.New<OrgHeader>();
			exporter.OH_Code = "EXPORTER";
			var contact2 = exporter.Contacts.AddNew();
			contact2.OC_Email = "export@Doc.Support";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OH_ImportBroker = importer.PK;
			shipment.JS_OH_ExportBroker = exporter.PK;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			Assert(shipment.IsExport());

			var docSupporter = new BaseJobDeclarationDocumentSupporter(declaration);
			AssertEquals(exporter.PK, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportBroker, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(exporter.PK, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportBroker, DocumentDirection.ANY).OrgHeader.PK);

			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";
			Assert(!shipment.IsExport());
			AssertEquals(importer.PK, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportBroker, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(importer.PK, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportBroker, DocumentDirection.ANY).OrgHeader.PK);
		}

		public void TestGetContactOrganisationFromShipmentAndDeclaration()
		{
			var orgproxy = Factory.New<OrgHeader>();
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = orgproxy.PK;

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			AssertEquals(company.PK, branch.Company.PK);

			var jobdeclaration = Factory.New<BaseJobDeclaration>();
			jobdeclaration.JE_GB = branch.PK;

			var docSupporter = new BaseJobDeclarationDocumentSupporter(jobdeclaration);
			AssertEquals(orgproxy.PK, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportBroker, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(orgproxy.PK, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportBroker, DocumentDirection.ANY).OrgHeader.PK);
		}

		public void TestGetPorts()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKPortOfLoading = "AUMEL";
			declaration.JE_RL_NKPortOfArrival = "USLAX";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USCHI";

			AssertEquals("local port", "USLAX", declaration.DocumentSupporter.LocalPort(ContactType.ImportFreightAgent, DocumentDirection.DEP));
			AssertEquals("local port", "AUMEL", declaration.DocumentSupporter.LocalPort(ContactType.ExportFreightAgent, DocumentDirection.DEP));
			AssertEquals("local port", "USLAX", declaration.DocumentSupporter.LocalPort(ContactType.ImportFreightAgent, DocumentDirection.ARV));
			AssertEquals("local port", "AUMEL", declaration.DocumentSupporter.LocalPort(ContactType.ExportFreightAgent, DocumentDirection.ARV));
			AssertEquals("local port", "USLAX", declaration.DocumentSupporter.LocalPort(ContactType.ImportFreightAgent, DocumentDirection.ANY));
			AssertEquals("local port", "AUMEL", declaration.DocumentSupporter.LocalPort(ContactType.ExportFreightAgent, DocumentDirection.ANY));

			AssertEquals("Departure local port", "AUSYD", declaration.DocumentSupporter.LocalPort(ContactType.LocalTransport, DocumentDirection.DEP));
			AssertEquals("Departure local port", "AUSYD", declaration.DocumentSupporter.LocalPort(ContactType.CTO, DocumentDirection.DEP));
			AssertEquals("Departure local port", "AUSYD", declaration.DocumentSupporter.LocalPort(ContactType.ShippingLine, DocumentDirection.DEP));
			AssertEquals("Departure local port", "AUSYD", declaration.DocumentSupporter.LocalPort(ContactType.FreightAgent, DocumentDirection.DEP));

			AssertEquals("Arrival Local Port", "USCHI", declaration.DocumentSupporter.LocalPort(ContactType.LocalTransport, DocumentDirection.ARV));
			AssertEquals("Arrival Local Port", "USCHI", declaration.DocumentSupporter.LocalPort(ContactType.CTO, DocumentDirection.ARV));
			AssertEquals("Arrival Local Port", "USCHI", declaration.DocumentSupporter.LocalPort(ContactType.ShippingLine, DocumentDirection.ARV));
			AssertEquals("Arrival Local Port", "USCHI", declaration.DocumentSupporter.LocalPort(ContactType.FreightAgent, DocumentDirection.ARV));

			AssertEquals("Local Port for ANY", "", declaration.DocumentSupporter.LocalPort(ContactType.LocalTransport, DocumentDirection.ANY));
			AssertEquals("Local Port for ANY", "", declaration.DocumentSupporter.LocalPort(ContactType.CTO, DocumentDirection.ANY));
			AssertEquals("Local Port for ANY", "", declaration.DocumentSupporter.LocalPort(ContactType.ShippingLine, DocumentDirection.ANY));
			AssertEquals("Local Port for ANY", "", declaration.DocumentSupporter.LocalPort(ContactType.FreightAgent, DocumentDirection.ANY));

			AssertEquals("Departure foreign port", "AUSYD", declaration.DocumentSupporter.ForeignPort(ContactType.LocalTransport, DocumentDirection.ARV));
			AssertEquals("Departure foreign port", "AUSYD", declaration.DocumentSupporter.ForeignPort(ContactType.CTO, DocumentDirection.ARV));
			AssertEquals("Departure foreign port", "AUSYD", declaration.DocumentSupporter.ForeignPort(ContactType.ShippingLine, DocumentDirection.ARV));
			AssertEquals("Departure foreign port", "AUSYD", declaration.DocumentSupporter.ForeignPort(ContactType.FreightAgent, DocumentDirection.ARV));

			AssertEquals("Arrival foreign Port", "USCHI", declaration.DocumentSupporter.ForeignPort(ContactType.LocalTransport, DocumentDirection.DEP));
			AssertEquals("Arrival foreign Port", "USCHI", declaration.DocumentSupporter.ForeignPort(ContactType.CTO, DocumentDirection.DEP));
			AssertEquals("Arrival foreign Port", "USCHI", declaration.DocumentSupporter.ForeignPort(ContactType.ShippingLine, DocumentDirection.DEP));
			AssertEquals("Arrival foreign Port", "USCHI", declaration.DocumentSupporter.ForeignPort(ContactType.FreightAgent, DocumentDirection.DEP));

			AssertEquals("foreign Port for ANY", "", declaration.DocumentSupporter.ForeignPort(ContactType.LocalTransport, DocumentDirection.ANY));
			AssertEquals("foreign Port for ANY", "", declaration.DocumentSupporter.ForeignPort(ContactType.CTO, DocumentDirection.ANY));
			AssertEquals("foreign Port for ANY", "", declaration.DocumentSupporter.ForeignPort(ContactType.ShippingLine, DocumentDirection.ANY));
			AssertEquals("foreign Port for ANY", "", declaration.DocumentSupporter.ForeignPort(ContactType.FreightAgent, DocumentDirection.ANY));
		}

		public void TestIsImport()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(declaration.DocumentSupporter.IsImport);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(!declaration.DocumentSupporter.IsImport);
		}

		public void TestDefaultContactOnLocalTransport()
		{
			var localTransport = Factory.New<OrgHeader>();
			localTransport.Addresses.AddNewMainAddress();

			var contact = localTransport.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Fax = "123456";
			contact.OC_Email = "test@edi.com.au";

			var orgDocument = contact.Documents.AddNew();
			orgDocument.OD_DocumentGroup = "TRN";
			orgDocument.OD_FilterLocalPort = "AUSYD";
			orgDocument.OD_FilterForeignPort = "HKHKG";
			orgDocument.OD_DeliverBy = "FAX";
			orgDocument.OD_FilterDirection = "IMP";

			orgDocument = contact.Documents.AddNew();
			orgDocument.OD_DocumentGroup = "TRN";
			orgDocument.OD_FilterLocalPort = "AUMEL";
			orgDocument.OD_FilterForeignPort = "HKHKG";
			orgDocument.OD_DeliverBy = "EML";
			orgDocument.OD_FilterDirection = "IMP";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKOrigin = "HKHKG";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.DeliveryOrPickupCartageCoPK = localTransport.PK;

			var menu = Factory.Load<StmMenuItem>(new ZGuid("DAAD79D1-79BB-4FBA-ADD4-BA143166E221"));
			AssertNotNull(menu);
			var documentPack = new DocumentPack(menu);
			var autoDelivery = documentPack.AutoDocumentDelivery ?? new DocAutoDelivery();
			var deliveryContacts = autoDelivery.GetDeliveryContactsForDocPack(menu, declaration.DocumentSupporter, documentPack.DocumentGroup);

			AssertEquals(1, deliveryContacts.Count);
			AssertEquals("FAX", deliveryContacts[0].DeliveryMethod);
			AssertEquals("123456", deliveryContacts[0].DeliveryAddress);
		}

		public void TestGetFilterValueMSGBKRLCO()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Export Declaration with Actual Landed Costing", "EXPACT", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRLCO));
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			AssertEquals("Drawback Declaration with Actual Landed Costing", "DRWACT", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRLCO));
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("Import Declaration with Actual Landed Costing", "IMPACT", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRLCO));
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("Ex Warehouse Declaration with Actual Landed Costing", "IMPACT", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRLCO));
		}

		public void TestConcurrencyErrorHandlingOnJobDocsAndCartage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "ABC";//JobDocsAndCartage is created
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var declaration2 = factory2.Load<BaseJobDeclaration>(declaration.PK);

			var documentSupporter = (BaseJobDeclarationDocumentSupporter)declaration2.DocumentSupporter;
			var menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Cartage Advice"));
			var args = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menu);

			declaration2.JP_Calc_CartageAdvised = ZDateTime.Empty;
			declaration.DocsAndCartage.JP_LCLAvailable = ZDateTime.Now;
			Factory.Save();

			AssertNoExceptionThrown(delegate
			{ documentSupporter.DocumentEventSource_DocumentPrinted(menu, args); });
		}

		public void TestGetChildCollection()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			AssertEquals("normally should show", true, declaration.DocumentSupporter.ShowReasonForNotPrinting(DataContext.None, null));

			var menu = Factory.New<StmMenuItem>();
			AssertEquals(0, declaration.DocumentSupporter.GetChildCollection(menu, BusinessContext.DtbBooking, null).Length);

			AssertEquals(0, declaration.DocumentSupporter.GetChildCollection(menu, BusinessContext.Shipment, null).Length);

			AssertEquals(1, declaration.DocumentSupporter.GetChildCollection(menu, BusinessContext.Customs, null).Length);
			AssertEquals(declaration, declaration.DocumentSupporter.GetChildCollection(menu, BusinessContext.Customs, null)[0]);
		}

		public void TestGetChildCollection_CusEntryHeader()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var menu = Factory.New<StmMenuItem>();

			CombineAssertions(() =>
			{
				AssertEquals(0, declaration.DocumentSupporter.GetChildCollection(menu, BusinessContext.CusEntryHeader, null).Length);

				declaration.CustomsEntryHeaders.AddNew();

				AssertEquals(1, declaration.DocumentSupporter.GetChildCollection(menu, BusinessContext.CusEntryHeader, null).Length);
			});
		}
	}
}
