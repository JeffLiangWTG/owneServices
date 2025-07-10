using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageContentFilterDocumentCollection))]
	class EDIMessageContentFilterDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EDIMessageContentFilterDocumentCollection>
	{
		protected override void SetUp()
		{
			base.SetUp();
			Filter = Factory.New<EDIMessageContentFilter>();
		}

		EDIMessageContentFilter Filter { get; set; }

		protected override EDIMessageContentFilterDocumentCollection GetCollectionToTest() => Filter.UniversalEvent.Documents;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EDIMessageContentFilterDocument(Filter.UniversalEvent);
	}
}
