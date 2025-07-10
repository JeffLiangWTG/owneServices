using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskFriendlyView))]
	sealed class ProcessTaskFriendlyViewTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetDescriptionWithNullProcessHeader()
		{
			var view = (ProcessTaskFriendlyView)GetNewBusinessObject();
			AssertEquals(new ZString("0    "), view.Description);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			return new ProcessTaskFriendlyView(task);
		}
	}
}
