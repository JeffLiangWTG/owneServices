using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusEntryHeaderDocumentSupportTest : DocumentSupporterTest
	{
		public virtual void TestSupportedDataContexts()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("Enterprise.Core.Constants.DataContext.CusEntryHeader is Supported", true, entryHeader.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusEntryHeader)));
		}

		protected virtual ZString GetMainNameSpace()
		{
			return "Enterprise.DocumentWrappers.Customs." + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + ".";
		}

		public virtual void TestGetDocBusinessObjects()
		{
			ZString mainNameSpace = GetMainNameSpace();
			CusEntryHeader entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();

			DocumentWrapper[] result = entryHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			AssertEquals("Document wrapper for data context of CusEntryHeader is of type DocCusEntryHeader", mainNameSpace + "DocCusEntryHeader", result[0].GetType().ToString());
		}

		public virtual void TestGetBODocDataProvidersNotFoundMessage()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("Entry Header cannot be found.", entryHeader.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusEntryHeader), null));
		}

		public virtual void TestShowReasonForNotPrinting()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals(true, entryHeader.DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.CusEntryHeader, null));
		}

		public void TestBusinessContext()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("CusEntryHeader has a business context of CusEntryHeader", BusinessContext.CusEntryHeader, entryHeader.DocumentSupporter.BusinessContext);
		}

		public void TestGetMenuTemplateFilterValueForBillOfEntry()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			DocCusEntryHeaderForTest entryHeaderWrapper = DocCusEntryHeaderForTest.New(entryHeader, Factory);
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals("MenuTemplateFilter ZAEXP", countryCode + declaration.JE_MessageType, entryHeader.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.BOE, entryHeaderWrapper));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("MenuTemplateFilter ZAIMP", countryCode + declaration.JE_MessageType, entryHeader.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.BOE, entryHeaderWrapper));

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("MenuTemplateFilter ZAEXW", countryCode + declaration.JE_MessageType, entryHeader.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.BOE, entryHeaderWrapper));
		}

		public void TestGetMultiSupplierMenuTemplateFilterValue()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			BaseJobComInvoiceHeader header1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader header2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			header1.JobComInvoiceLines.AddNew().JI_CL = entryLine.PK;
			header2.JobComInvoiceLines.AddNew().JI_CL = entryLine1.PK;

			DocCusEntryHeaderForTest wrapper = DocCusEntryHeaderForTest.New(entryHeader, Factory);

			ZString result = entryHeader.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.MULTISUPPLIER, wrapper);
			AssertEquals("Declaration type is Export, no supplier list", "No", result);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			result = entryHeader.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.MULTISUPPLIER, wrapper);
			AssertEquals("Declaration type is Import, but no suppliers", "No", result);

			OrgHeader supplier1 = Factory.New<OrgHeader>();
			OrgHeader supplier2 = Factory.New<OrgHeader>();
			header1.JZ_OH_Supplier = supplier1.PK;

			result = entryHeader.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.MULTISUPPLIER, wrapper);
			AssertEquals("Declaration type is Import, but only 1 supplier", "No", result);

			header2.JZ_OH_Supplier = supplier2.PK;
			result = entryHeader.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.MULTISUPPLIER, wrapper);
			AssertEquals("Declaration type is Import, and is multi supplier", "Yes", result);
		}

		public void TestGetFilterValue()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("Current company country", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.CTY));

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("Filtervalue for MSGBKRCTYAPP", declaration.JE_MessageType + customsCountry + "CMR", entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYAPP));

			AssertEquals("Filtervalue for MSGGDSCTRY", declaration.JE_MessageType + entryHeader.GoodsTypeForDocumentFilter + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGGDSCTRY));

			AssertEquals("Filtervalue for MSGBKRCTY", declaration.JE_MessageType + customsCountry, entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("Filtervalue for MSGBKRCTYAPP", declaration.JE_MessageType + "USCMR", entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYAPP));
			}

			entryHeader.CH_MessageType = "AAA";

			AssertEquals("AAA" + customsCountry + "CMR", entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGENTCTRYAPP));
		}

		public void TestGetFilterValueASYCUDA()
		{
			//Filter = ASYCUDA
			var entryHeader = Factory.New<CusEntryHeader>();
			var isAsycudaCountry = ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(entryHeader.CountryCode);
			if (isAsycudaCountry)
			{
				AssertEquals("For filter 'ASYCUDA' result is Y for Asycuda country", "Y", entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.ASYCUDA));
			}
			else
			{
				AssertEquals("For filter 'ASYCUDA' result is N", "N", entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.ASYCUDA));
			}
		}

		public void TestStorageDocsAreEditableIfInRelated()
		{
			CusEntryHeader entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			AssertEquals("Storage docs should be modifyable if the business object is a related business object and not a top level business object", true, entryHeader.DocumentSupporter.StorageDocsAreEditableIfInRelated);
		}

		public virtual void TestGetContactOrganisation()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			var declaration = entryHeader.Declaration;
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertNull("GetContactOrganisation on FWE doesnt blow up on null Forwarder", entryHeader.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader);

			var fWD = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Forwarder = fWD.PK;
			AssertEquals("GetContactOrganisation(FWE) returning the Forwarder", fWD, entryHeader.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals("GetContactOrganisation(FWI) returning the Forwarder", fWD, entryHeader.DocumentSupporter.GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ANY).OrgHeader);

			var cTOOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cTOAddress = cTOOrg.Addresses.AddNew();
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;
			AssertEquals("Contact Type should be CTO", cTOOrg.PK, entryHeader.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.CTO, DocumentDirection.ANY).OrgHeader.PK);

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			AssertEquals("GetContactOrganisation(TIP) returning the Carrier", shippingLine, entryHeader.DocumentSupporter.GetContactOrganisation("", ContactType.ShippingLine, DocumentDirection.ANY).OrgHeader);

			var lOCOrg = Factory.NewWithValidTestData<OrgHeader>();
			var lOCAddress = lOCOrg.Addresses.AddNew();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			declaration.Job.JH_OA_LocalChargesAddr = lOCAddress.PK;
			AssertEquals("Contact Type should be LOC", lOCOrg.PK, entryHeader.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalClient, DocumentDirection.ANY).OrgHeader.PK);
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

			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			var declaration = entryHeader.Declaration;
			declaration.JE_JS = shipment.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			Assert(shipment.IsExport());

			var docSupporter = new CusEntryHeaderDocumentSupporter(entryHeader);
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

			var declaration = BaseJobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_GB = branch.PK;

			var docSupporter = new CusEntryHeaderDocumentSupporter(entryHeader);
			AssertEquals(orgproxy.PK, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.ImportBroker, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(orgproxy.PK, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.ExportBroker, DocumentDirection.ANY).OrgHeader.PK);
		}

		#region Implementation

		public class DocCusEntryHeaderForTest : DocumentWrapper
		{
			DocCusEntryHeaderForTest(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
				: base(entryHeader, factoryToWrap)
			{
			}

			public static DocCusEntryHeaderForTest New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			{
				return (entryHeader == null) ? null : new DocCusEntryHeaderForTest(entryHeader, factoryToWrap);
			}

			public override string ToString()
			{
				return ZString.Empty;
			}
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			return declaration.CustomsEntryHeaders.AddNew();
		}

		#endregion

	}
}
