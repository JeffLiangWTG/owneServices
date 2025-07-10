using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.JobDeclarationDrawbackSupporter;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackNoticeOfIntentDocLineCollection))]
	class DrawbackNoticeOfIntentDocLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DrawbackNoticeOfIntentDocLineCollection>
	{
		protected override DrawbackNoticeOfIntentDocLineCollection GetCollectionToTest()
		{
			var supporter = new JobDeclarationDrawbackSupporter(Factory.New<JobDeclaration>(), UpdateActionCode.Add);
			return new DrawbackNoticeOfIntentDocLineCollection(supporter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			return new DrawbackNoticeOfIntentDocLine(invLine);
		}
	}
}
