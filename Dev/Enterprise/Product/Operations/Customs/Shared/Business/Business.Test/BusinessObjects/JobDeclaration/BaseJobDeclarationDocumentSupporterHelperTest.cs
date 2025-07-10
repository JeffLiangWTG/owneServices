using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationDocumentSupporterHelperTest : TestCaseWithFactory
	{
		public void TestGetContactOrganisation_Consignee()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertNull("GetContactOrganisation on CNE doesnt blow up on null importer", BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.Consignee).OrgHeader);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("GetContactOrganisation(CNE) returning the Importer (Consignee) when not null", importer, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.Consignee).OrgHeader);
		}

		public void TestGetContactOrganisation_LocalTransport()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = ZGuid.Empty;
			AssertNull("GetContactOrganisation on TRN doesnt blow up on null DeliveryOrPickupCartageCo", BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.LocalTransport).OrgHeader);

			var deliveryCo = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = deliveryCo.MainAddress.PK;
			AssertEquals("GetContactOrganisation(TRN) returning the DeliveryOrPickupCartageCo", deliveryCo, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.LocalTransport).OrgHeader);
		}

		public void TestGetContactOrganisation_Consignor()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNull("GetContactOrganisation on CNR doesnt blow up on null supplier", BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.Consignor).OrgHeader);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("GetContactOrganisation(CNR) returning the Supplier (Consignor) when not null", supplier, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.Consignor).OrgHeader);
		}

		public void TestGetContactOrganisation_FreightAgent()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertNull("GetContactOrganisation on FWE doesnt blow up on null Forwarder", BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.ExportFreightAgent).OrgHeader);

			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertEquals("GetContactOrganisation(FWE) returning the Forwarder", forwarder, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.ExportFreightAgent).OrgHeader);
			AssertEquals("GetContactOrganisation(FWI) returning the Forwarder", forwarder, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.ImportFreightAgent).OrgHeader);
		}

		public void TestGetContactOrganisation_Receivables()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			AssertNull("GetContactOrganisation on A/R doesnt blow up on null LocalCharges in JobHeader", BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.Receivables).OrgHeader);

			var localCharges = Factory.NewWithValidTestData<OrgHeader>();
			job.JH_OA_LocalChargesAddr = localCharges.MainAddress.PK;
			AssertEquals("GetContactOrganisation(A/R) returning the LocalCharges", localCharges, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.Receivables).OrgHeader);
		}

		public void TestGetContactOrganisation_CTO()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var terminalOperator = Factory.NewWithValidTestData<OrgHeader>();
			declaration.ContainerTerminalOperatorDocAddress.E2_AddressOverride = false;
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = terminalOperator.MainAddress.PK;
			AssertEquals("GetContactOrganisation(CTO) returning the ContainerTerminalOperatorDocAddress.Organisation", terminalOperator, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.CTO).OrgHeader);
		}

		public void TestGetContactOrganisation_ShippingLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			AssertEquals("GetContactOrganisation(TIP) returning the Carrier", shippingLine, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.ShippingLine).OrgHeader);
		}

		public void TestGetContactOrganisation_Broker()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			company.GC_OH_OrgProxy = orgProxy.PK;
			declaration.JE_GC = company.PK;
			AssertEquals("GetContactOrganisation(BRI) returning the company's Orgproxy", orgProxy, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.ImportBroker).OrgHeader);
			AssertEquals("GetContactOrganisation(BRE) returning the company's Orgproxy", orgProxy, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.ExportBroker).OrgHeader);
		}

		public void TestGetContactOrganisation_LocalClient()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var lOCOrg = Factory.NewWithValidTestData<OrgHeader>();
			var lOCAddress = lOCOrg.Addresses.AddNew();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			declaration.Job.JH_OA_LocalChargesAddr = lOCAddress.PK;
			AssertEquals("Contact Type should be LOC", lOCOrg.PK, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.LocalClient).OrgHeader.PK);
		}

		public void TestGetContactOrganisation_Declarant()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var declarantAddress = declarant.Addresses.AddNew();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			AssertEquals("Contact Type should be DEC", declarant.PK, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.Declarant).OrgHeader.PK);
		}

		public void TestGetJobHeaderForeignKeyLink()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertNull("Declaration has no JobHeader associated", BaseJobDeclarationDocumentSupporterHelper.GetJobHeaderForeignKeyLink(declaration));

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_GB = Env.CurrentBranch.PK;
			Factory.Save();
			AssertEquals("Declaration has one JobHeader associated", job, BaseJobDeclarationDocumentSupporterHelper.GetJobHeaderForeignKeyLink(declaration));
		}

		public void TestGetContactOrganisation_ControllingCustomer()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var controllingCustomer = Factory.New<OrgHeader>();
			declaration.JE_OH_ControllingCustomer = controllingCustomer.PK;
			AssertEquals("Contact Type should be CCU", controllingCustomer.PK, BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(declaration, ContactType.ControllingCustomer).OrgHeader.PK);
		}
	}
}
