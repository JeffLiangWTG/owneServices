using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.JobDeclarationDrawbackSupporter;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackNoticeOfIntentLineToPrint))]
	class DrawbackNoticeOfIntentLineToPrintTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var line = Factory.NewWithValidTestData<JobComInvoiceLine>();
			return new DrawbackNoticeOfIntentLineToPrint(line);
		}
	}
}
