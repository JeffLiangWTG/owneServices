using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class WhsControllerBaseBasherTest : ZControllerBasherTest
	{
		public void TestShowDeleteForm()
		{
			var iCanDelete = BizO as ICanDelete;
			IZForm deleteForm;

			if (iCanDelete == null || iCanDelete.CanDelete)
			{
				deleteForm = Controller.ShowDeleteForm(BizO);
				AssertNotNull("DeleteForm Available", deleteForm);
			}
			else
			{
				deleteForm = Controller.ShowDeleteForm(BizO);
				AssertNull("DeleteFrom Unavailable", deleteForm);
			}
		}

		BusinessObject fBizO;
		BusinessObject BizO
		{
			get
			{
				if (fBizO == null)
				{
					fBizO = GetBusinessObjectThatIsInTheDatabase();
				}
				return fBizO;
			}
		}
	}
}
