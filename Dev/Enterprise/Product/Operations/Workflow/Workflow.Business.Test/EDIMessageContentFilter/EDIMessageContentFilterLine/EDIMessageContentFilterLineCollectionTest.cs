
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageContentFilterLineCollection))]
	class EDIMessageContentFilterLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EDIMessageContentFilterLineCollection>
	{
		protected override void SetUp()
		{
			base.SetUp();
			Filter = Factory.New<EDIMessageContentFilter>();
		}

		EDIMessageContentFilter Filter { get; set; }

		protected override EDIMessageContentFilterLineCollection GetCollectionToTest() => Filter.UniversalEvent.Lines;
		protected override BusinessObject GetNewElementToAddToTheCollection() => new EDIMessageContentFilterLine(Filter.UniversalEvent);
	}
}
