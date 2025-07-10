using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ServiceTypeDateFilterControlTest : TestCaseWithFactory
	{
		public void TestJobServiceTypeEdit()
		{
			using (var form = new ZForm())
			using (var filterControl = new ServiceTypeDateFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();

				var dropEdit = form.Controls.Find("jobServiceTypeEdit", true).FirstOrDefault();
				AssertNotNull(dropEdit);
			}
		}
	}
}
