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
	public class WhsItemDispatchTransportationUnitController : WhsTransitController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.WhsItemDispatchTransportationUnit; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsItemDispatchTransportationUnit; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsItemDispatchTransportationUnit); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var dtu = (WhsItemDispatchTransportationUnit)businessEntity;
			var canOpenForm = TransitWarehouseHelper.IsTransitWarehouseInCurrentBranch(Factory, dtu.WDH_WW_Warehouse);

			if (!canOpenForm)
			{
				Globals.Message.ShowError(Res.GetString("WhsItemDispatchTransportationUnitController|GetForm", "The DTU could not be found. Please check that you are logged into the DTU's Warehouse Branch and try again."));
			}

			return canOpenForm ? new DispatchTransportationUnitForm(dtu) : null;
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("WhsItemDispatchTransportationUnitController|ShowFormForNewEntity", "You cannot create a Transit Dispatch Transportation Unit on the {0} Desktop, please go to the Transit Warehouse Management Portal.", "CargoWise"));

			return null;
		}
	}
}
