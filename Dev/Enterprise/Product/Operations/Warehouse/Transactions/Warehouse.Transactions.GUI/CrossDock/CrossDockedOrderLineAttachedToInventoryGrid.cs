using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class CrossDockedOrderLineAttachedToInventoryGrid : ZModuleButtonGrid
	{
		protected override BusinessObject GetObjectToEdit(BusinessObject selected)
		{
			return ((WhsPickLine)selected).DocketLine;
		}

		protected override ZController GetNewControllerCore(BusinessObject selected)
		{
			var docketLineController = base.GetNewControllerCore(selected);
			var docketLine = (WhsPickableDocketLine)selected;
			var docket = docketLine.PickableDocket;
			IControllerIDProvider controllerIDProvider = docket;

			var orderController = ZControllerFactory.Create(controllerIDProvider.ControllerID);
			var openedForm = orderController.GetOpenedForm(docket);
			if (openedForm != null)
			{
				docketLineController.SetFormsModalTo(openedForm);
			}

			return docketLineController;
		}
	}
}
