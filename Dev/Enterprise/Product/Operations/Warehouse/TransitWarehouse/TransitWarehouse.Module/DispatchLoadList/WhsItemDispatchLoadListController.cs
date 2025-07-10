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
	public class WhsItemDispatchLoadListController : WhsTransitController
	{
		public override ControllerID ID => ControllerIDs.WhsItemDispatchLoadList;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsItemDispatchLoadList;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsItemDispatchLoadList);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var dll = (WhsItemDispatchLoadList)businessEntity;
			var canOpenForm = TransitWarehouseHelper.IsTransitWarehouseInCurrentBranch(Factory, dll.WDL_WW_Warehouse);

			if (!canOpenForm)
			{
				Globals.Message.ShowError(Res.GetString("WhsItemDispatchLoadListController|GetForm", "The DLL could not be found. Please check that you are logged into the DLL's Warehouse Branch and try again."));
			}

			return canOpenForm ? new DispatchLoadListForm(dll) : null;
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("WhsItemDispatchLoadListController|ShowFormForNewEntity", "You cannot create a Transit Dispatch Load List on the {0} Desktop, please go to the Transit Warehouse Management Portal.", "CargoWise"));

			return null;
		}
	}
}
