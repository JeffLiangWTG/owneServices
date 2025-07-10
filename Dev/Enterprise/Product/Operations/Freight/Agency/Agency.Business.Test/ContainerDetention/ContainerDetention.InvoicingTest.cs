using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class ContainerDetentionTest
	{
		public void TestGetDefaultCreditor()
		{
			AccChargeCode pChargeCode = Factory.New<AccChargeCode>();
			pChargeCode.AC_Code = "_PC";
			pChargeCode.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;

			AccChargeCode aChargeCode = Factory.New<AccChargeCode>();
			aChargeCode.AC_Code = "_AC";
			aChargeCode.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Agent;

			Detention.NC_OH_Principal = Principal.PK;

			AgencyRegistry.Instance.DefaultCreditorFromPrincipal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(Principal, ((IJobInvoicingPlugIn)Detention).InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, null)));
			AssertEquals(null, ((IJobInvoicingPlugIn)Detention).InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, null)));

			AgencyRegistry.Instance.DefaultCreditorFromPrincipal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(null, ((IJobInvoicingPlugIn)Detention).InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, null)));
			AssertEquals(null, ((IJobInvoicingPlugIn)Detention).InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, null)));
		}

		public void TestConsumerType()
		{
			AssertEquals(JobInvoicingConsumerTypes.AgencyDetentionInvoice, ((IJobInvoicingPlugIn)Detention).InvoicingSupporter.ConsumerType);
		}

		public void TestSecurityCheckpoints()
		{
			IJobInvoicingPlugIn invoice = Detention;

			AssertEquals("JobInvoicing", Env.Security.AgencyContainerDetentionJobInvoicing, invoice.InvoicingSupporter.JobInvoicingSecurity);
			AssertEquals("AuditBilling", Env.Security.AgencyContainerDetentionAuditBilling, invoice.InvoicingSupporter.AuditSecurity);
			AssertEquals("EditSecurity", Env.Security.None, invoice.InvoicingSupporter.EditSecurityCheckpoint);
		}

		public void TestInvoiceImportExport()
		{
			GlbDepartment sid = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "SID");
			GlbDepartment sed = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "SED");

			LinerAgencyDataRegistry.Instance.ImportDetentionDefaultDepartment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, sid.PK.ToGuid());
			LinerAgencyDataRegistry.Instance.ExportDetentionDefaultDepartment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, sed.PK.ToGuid());

			IJobInvoicingPlugIn invoice = Detention;

			Detention.NC_DetentionType = "";
			AssertEquals("Import: IsExport", false, invoice.InvoicingSupporter.IsExport);
			AssertEquals("Import: IsImport", false, invoice.InvoicingSupporter.IsImport);
			AssertEquals("Import: DefaultDepartment", ZGuid.Empty, invoice.InvoicingSupporter.OverriddenDepartmentPK);

			Detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			AssertEquals("Import: IsExport", false, invoice.InvoicingSupporter.IsExport);
			AssertEquals("Import: IsImport", true, invoice.InvoicingSupporter.IsImport);
			AssertEquals("Import: DefaultDepartment", sid.PK, invoice.InvoicingSupporter.OverriddenDepartmentPK);

			Detention.NC_DetentionType = DetentionInvoiceType.Codes.Export;
			AssertEquals("Export: IsExport", true, invoice.InvoicingSupporter.IsExport);
			AssertEquals("Export: IsImport", false, invoice.InvoicingSupporter.IsImport);
			AssertEquals("Export: DefaultDepartment", sed.PK, invoice.InvoicingSupporter.OverriddenDepartmentPK);
		}

		public void TestConsignorConsignee()
		{
			// we don't actually have a consignor or consignee so this is technically incorrect,
			// but nessisary so that the local client is defaulted correctly.

			Detention.NC_OH_Client = Client.PK;
			Detention.NC_OH_Principal = Principal.PK;

			IJobInvoicingPlugIn invoice = Detention;

			AssertEquals("Consignor", Client, invoice.InvoicingSupporter.Consignor);
			AssertEquals("Consignee", Client, invoice.InvoicingSupporter.Consignee);
		}

		public void TestInvoiceMembers()
		{
			IJobInvoicingPlugIn invoice = Detention;
			AssertEquals(JobInvoicingConsumerTypes.AgencyDetentionInvoice, invoice.InvoicingSupporter.ConsumerType);
			AssertEquals(true, invoice.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Detention;
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent containerDetention = Factory.New<ContainerDetention>();
			Assert(containerDetention.AllowInvoiceDeletion);
		}

		#endregion
	}
}
