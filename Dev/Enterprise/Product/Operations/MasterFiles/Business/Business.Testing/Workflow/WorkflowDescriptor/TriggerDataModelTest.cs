using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TriggerDataModelTest : TestCaseWithFactory
	{
		public void TestPreviousEventTime()
		{
			var datetime = new ZDateTime(2021, 3, 15, 6, 12, 52);
			var model = new TriggerDataModel(Lazy.Create(() => datetime));
			AssertEquals("PreviousEventDate Value", datetime, model.PreviousEventDate);
		}
	}
}
