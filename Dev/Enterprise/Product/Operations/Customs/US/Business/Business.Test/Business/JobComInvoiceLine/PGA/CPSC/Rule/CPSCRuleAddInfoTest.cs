using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCRuleAddInfo))]
	sealed class CPSCRuleAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var ruleDetail = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().CPSCHeaders.AddNew().RuleAndLabs.AddNew();
			return new CPSCRuleAddInfo(ruleDetail.B7_AddInfoDataInfo);
		}
	}
}
