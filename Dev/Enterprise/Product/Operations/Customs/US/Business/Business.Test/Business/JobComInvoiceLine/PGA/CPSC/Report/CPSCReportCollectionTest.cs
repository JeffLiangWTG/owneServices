using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCReportCollection))]
	public class CPSCReportCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var header = invoiceLine.CPSCHeaders.AddNew();
			var rule = header.RuleAndLabs.AddNew();
			return new CPSCReportCollection(rule);
		}
	}
}
