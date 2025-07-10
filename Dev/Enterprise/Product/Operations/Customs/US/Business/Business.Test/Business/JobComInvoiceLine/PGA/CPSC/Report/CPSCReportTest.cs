using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCReport))]
	internal class CPSCReportTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CPSCReport>
	{
		public void TestProperties()
		{
			var report = (CPSCReport)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals("US_RemarksType: List", "AddInfoLookups.LabReportInformationTypeList", report.US_RemarksTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups: Type", typeof(USCPSCLabReportAddInfoLookups), report.AddInfoLookups.GetType());
			AssertEquals("AddInfoValidation: Type", typeof(USCPSCLabReportAddInfoValidation), report.AddInfoValidation.GetType());
		}

		public void TestOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			var rule = cpsc.RuleAndLabs.AddNew();
			rule.US_CPSCAccreditedLabID = "Test";
			rule.ReportAndLabs.AddNew();
			var report = rule.ReportAndLabs.AddNew();
			report.US_RemarksText = "Test";
			Factory.Save();

			AssertEquals(1, rule.ReportAndLabs.Count);
			AssertEquals("Test", rule.ReportAndLabs[0].US_RemarksText);
		}

		protected override IEnumerable<CPSCReport> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (CPSCReport)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			var rule = cpsc.RuleAndLabs.AddNew();
			rule.US_CPSCAccreditedLabID = "Test";
			var report = rule.ReportAndLabs.AddNew();
			report.US_RemarksText = "Test";
			return report;
		}
	}
}
