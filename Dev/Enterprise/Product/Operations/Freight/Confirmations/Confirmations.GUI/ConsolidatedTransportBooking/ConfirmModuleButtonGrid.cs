using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Confirmations.GUI
{
	public partial class ConfirmsModuleButtonGrid : ZModuleButtonGrid
	{
		public ConfirmsModuleButtonGrid()
			: base()
		{
		}

		protected override BusinessObject GetObjectToEdit(BusinessObject selected)
		{
			CommonPickupDeliveryConfirm confirm = (CommonPickupDeliveryConfirm)selected;
			return confirm.FirstShipment;
		}

		protected override ZController GetNewControllerCore(BusinessObject selected)
		{
			return ZControllerFactory.Create(ControllerIDs.JobShipment);
		}
	}
}
