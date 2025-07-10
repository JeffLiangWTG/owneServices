using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.JobDeclarationDrawbackSupporter;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackSummaryDocLine))]
	class DrawbackSummaryDocLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var line = Factory.NewWithValidTestData<JobComInvoiceLine>();
			return new DrawbackSummaryDocLine(line);
		}
	}
}
