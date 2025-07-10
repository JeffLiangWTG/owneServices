using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.JobDeclarationDrawbackSupporter;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackSummaryDocLineCollection))]
	class DrawbackSummaryDocLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DrawbackSummaryDocLineCollection>
	{
		protected override DrawbackSummaryDocLineCollection GetCollectionToTest()
		{
			var supporter = new JobDeclarationDrawbackSupporter(Factory.New<JobDeclaration>(), UpdateActionCode.Add);
			return new DrawbackSummaryDocLineCollection(supporter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = Factory.NewWithValidTestData<JobComInvoiceLine>();
			return new DrawbackSummaryDocLine(line);
		}
	}
}
