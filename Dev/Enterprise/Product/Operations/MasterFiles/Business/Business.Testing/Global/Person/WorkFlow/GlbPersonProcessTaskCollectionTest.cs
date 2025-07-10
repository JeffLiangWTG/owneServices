using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPersonProcessTaskCollection))]
	sealed class GlbPersonProcessTaskCollectionTest : ProcessTaskCollectionTest<GlbPersonProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			GlbPersonProcessTaskCollection collection = GetCollectionToTestCore();
			AssertEquals(typeof(GlbPersonProcessTask), collection.AddNew().GetType());
		}

		#region Implementation

		protected override GlbPersonProcessTaskCollection GetCollectionToTestCore()
		{
			return (GlbPersonProcessTaskCollection)Factory.NewWithValidTestData<GlbPerson>().WorkflowItems;
		}

		#endregion
	}
}
