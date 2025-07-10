using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(JobDeclarationInvoicingSupporter))]
	sealed class JobDeclarationInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestOverridenDepartmentPK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				var departmentOther = Factory.New<GlbDepartment>();
				departmentOther.GE_Code = "OTH";
				var departmentOtherImport = Factory.New<GlbDepartment>();
				departmentOtherImport.GE_Code = "OTI";
				var departmentEXW = Factory.New<GlbDepartment>();
				departmentEXW.GE_Code = "EXW";
				var departmentIMPAIR = Factory.New<GlbDepartment>();
				departmentIMPAIR.GE_Code = "IAR";
				var departmentIMPSEAFcl = Factory.New<GlbDepartment>();
				departmentIMPSEAFcl.GE_Code = "ISF";
				var departmentIMPSEALcl = Factory.New<GlbDepartment>();
				departmentIMPSEALcl.GE_Code = "ISL";
				var departmentIMPRAI = Factory.New<GlbDepartment>();
				departmentIMPRAI.GE_Code = "IRA";
				var departmentIMPROA = Factory.New<GlbDepartment>();
				departmentIMPROA.GE_Code = "IRO";
				var departmentIMPMAI = Factory.New<GlbDepartment>();
				departmentIMPMAI.GE_Code = "IMA";
				var departmentEXPAIR = Factory.New<GlbDepartment>();
				departmentEXPAIR.GE_Code = "EAR";
				var departmentEXPSEAFcl = Factory.New<GlbDepartment>();
				departmentEXPSEAFcl.GE_Code = "ESF";
				var departmentEXPSEALcl = Factory.New<GlbDepartment>();
				departmentEXPSEALcl.GE_Code = "ESL";
				var departmentEXPRAI = Factory.New<GlbDepartment>();
				departmentEXPRAI.GE_Code = "ERA";
				var departmentEXPROA = Factory.New<GlbDepartment>();
				departmentEXPROA.GE_Code = "ERR";
				var departmentEXPMAI = Factory.New<GlbDepartment>();
				departmentEXPMAI.GE_Code = "EMA";
				AccountingConfigurationRegistry.Instance.CustomsOther.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentOther.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsImportOther.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentOtherImport.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsExWarehouse.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentEXW.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsImportAirUld.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentIMPAIR.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentIMPSEAFcl.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsImportSeaLcl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentIMPSEALcl.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsImportRail.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentIMPRAI.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsImportRoad.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentIMPROA.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsImportPost.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentIMPMAI.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsExportAirUld.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentEXPAIR.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsExportSeaFcl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentEXPSEAFcl.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsExportSeaLcl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentEXPSEALcl.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsExportRail.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentEXPRAI.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsExportRoad.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentEXPROA.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CustomsExportPost.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, departmentEXPMAI.PK.ToGuid());
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.Sea;
				dec.JE_ContainerCount = 1;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_ContainerCount = 0;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				AssertEquals(departmentOtherImport.PK, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.Sea;
				dec.JE_ContainerCount = 1;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_ContainerCount = 0;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertEquals(ZGuid.Empty, dec.InvoicingSupporter.OverriddenDepartmentPK);
				dec.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				AssertEquals(departmentOther.PK, dec.InvoicingSupporter.OverriddenDepartmentPK);
			}
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject() => Factory.NewWithValidTestData<JobDeclaration>();

		protected override ZString TestingCountry => Constants.CountryCodes.SouthAfrica;
	}
}
