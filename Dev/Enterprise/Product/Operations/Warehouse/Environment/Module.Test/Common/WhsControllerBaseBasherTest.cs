using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	public abstract class WhsControllerBaseBasherTest : ZControllerBasherTest
	{
		public void TestShowDeleteForm()
		{
			var bizO = GetBusinessObjectThatIsInTheDatabase();
			var allowUserToDelete = bizO as ICanDelete;
			if (allowUserToDelete == null || allowUserToDelete.CanDelete)
			{
				var deleteForm = Controller.ShowDeleteForm(bizO);
				AssertNotNull("DeleteForm Available", deleteForm);
			}
			else
			{
				var deleteForm = Controller.ShowDeleteForm(bizO);
				AssertNull("DeleteFrom Unavailable", deleteForm);
			}
		}
	}
}
