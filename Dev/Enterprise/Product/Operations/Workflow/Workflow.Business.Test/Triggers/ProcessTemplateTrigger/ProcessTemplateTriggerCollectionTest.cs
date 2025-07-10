using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessTemplateTriggerCollection))]
	class ProcessTemplateTriggerCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessTemplateTriggerCollection>
	{
		public void TestSequence()
		{
			AssertEquals(1, Collection.AddNew().P9T_Sequence);
			AssertEquals(2, Collection.AddNew().P9T_Sequence);
		}

		#region Implementation

		protected override ProcessTemplateTriggerCollection GetCollectionToTest()
		{
			return new ProcessTemplateTriggerCollection(Factory.New<ProcessTaskTemplate>());
		}

		#endregion
	}
}
