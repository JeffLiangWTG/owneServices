using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupProcessTaskCollection))]
	public class GlbGroupProcessTaskCollectionTest : ProcessTaskCollectionTest<GlbGroupProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(GlbGroupProcessTask), collection.AddNew().GetType());
		}

		#region Implementation

		protected override GlbGroupProcessTaskCollection GetCollectionToTestCore()
		{
			return (GlbGroupProcessTaskCollection)Factory.NewWithValidTestData<GlbGroup>().WorkflowItems;
		}

		#endregion
	}
}
