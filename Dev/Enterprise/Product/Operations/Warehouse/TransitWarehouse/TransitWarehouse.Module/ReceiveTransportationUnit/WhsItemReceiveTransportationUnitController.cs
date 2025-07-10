using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsItemReceiveTransportationUnitController : WhsTransitController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.WhsItemReceiveTransportationUnit; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsItemReceiveTransportationUnit; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsItemReceiveTransportationUnit); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var rtu = (WhsItemReceiveTransportationUnit)businessEntity;
			var canOpenForm = TransitWarehouseHelper.IsTransitWarehouseInCurrentBranch(Factory, rtu.WRH_WW_Warehouse);

			if (!canOpenForm)
			{
				Globals.Message.ShowError(Res.GetString("WhsItemReceiveTransportationUnitController|GetForm", "The RTU could not be found. Please check that you are logged into the RTU's Warehouse Branch and try again."));
			}

			return canOpenForm ? new ReceiveTransportationUnitForm(rtu) : null;
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("WhsItemReceiveTransportationUnitController|ShowFormForNewEntity", "You cannot create a Transit Receive Transportation Unit on the {0} Desktop, please go to the Transit Warehouse Management Portal.", "CargoWise"));

			return null;
		}
	}
}
