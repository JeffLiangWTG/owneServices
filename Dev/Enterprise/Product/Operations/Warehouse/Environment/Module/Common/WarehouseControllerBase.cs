using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public abstract class WhsControllerBase : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			IZForm result = null;

			var userDeleteCondition = sourceEntity as ICanDelete;
			if (userDeleteCondition == null || userDeleteCondition.CanDelete)
			{
				result = base.ShowDeleteForm(sourceEntity);
			}
			else
			{
				Globals.Message.Show(userDeleteCondition.ReasonForNotAbleToDelete);
			}

			return result;
		}
	}
}
