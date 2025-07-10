using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.JobDeclarationDrawbackSupporter;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackNoticeOfIntentDocLine))]
	class DrawbackNoticeOfIntentDocLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var line = Factory.NewWithValidTestData<JobComInvoiceLine>();
			return new DrawbackNoticeOfIntentDocLine(line);
		}
	}
}
