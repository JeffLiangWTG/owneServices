using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Integration.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCRule))]
	internal class CPSCRuleTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CPSCRule>
	{
		public void TestProperties()
		{
			var rule = (CPSCRule)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals("AddInfoLookups: Type", typeof(USCPSCRuleAddInfoLookups), rule.AddInfoLookups.GetType());
			AssertEquals("AddInfoValidation: Type", typeof(USCPSCRuleAddInfoValidation), rule.AddInfoValidation.GetType());
			AssertEquals("ReportAndLabs: Type", typeof(CPSCReportCollection), rule.ReportAndLabs.GetType());
		}

		public void TestICusAddInfoTypeSupporterMembers()
		{
			var rule = (CPSCRule)GetNewBusinessObjectForDeleteTest(Factory);
			ICusAddInfoTypeSupporter supporter = rule;
			supporter.AssertType(typeof(CPSCReport), CusAddInfoTypeAttribute.Codes.USCPSCReport);
		}

		public void TestGetFetchStrategies()
		{
			var rule = (CPSCRule)GetNewBusinessObjectForDeleteTest(Factory);
			var expectedTypes = new[]
			{
				typeof(CusAddInfoTypeSupporterFetchStrategy),
			};
			var actualTypes = ((IAdditionalBusinessObjectFetchStrategyProvider)rule).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void TestOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			cpsc.RuleAndLabs.AddNew();
			var rule = cpsc.RuleAndLabs.AddNew();
			rule.US_CPSCAccreditedLabID = "Test";
			Factory.Save();

			AssertEquals(1, cpsc.RuleAndLabs.Count);
			AssertEquals("Test", cpsc.RuleAndLabs[0].US_CPSCAccreditedLabID);
		}

		protected override IEnumerable<CPSCRule> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (CPSCRule)GetNewBusinessObjectForDeleteTest(factory);
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
			return rule;
		}
	}
}
