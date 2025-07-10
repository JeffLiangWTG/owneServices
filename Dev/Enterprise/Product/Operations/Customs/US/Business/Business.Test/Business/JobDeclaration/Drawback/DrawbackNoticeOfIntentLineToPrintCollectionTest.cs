using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.JobDeclarationDrawbackSupporter;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackNoticeOfIntentLineToPrintCollection))]
	class DrawbackNoticeOfIntentLineToPrintCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DrawbackNoticeOfIntentLineToPrintCollection>
	{
		protected override DrawbackNoticeOfIntentLineToPrintCollection GetCollectionToTest()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new DrawbackNoticeOfIntentLineToPrintCollection(jobDeclaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = Factory.NewWithValidTestData<JobComInvoiceLine>();
			return new DrawbackNoticeOfIntentLineToPrint(line);
		}
	}
}
