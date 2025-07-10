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
	public class WhsTransitDispatchConsignmentController : WhsTransitController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.WhsTransitDispatchConsignment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsTransitDispatchConsignment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsItemDispatchConsignment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var dcn = (WhsItemDispatchConsignment)businessEntity;
			var canOpenForm = TransitWarehouseHelper.IsTransitWarehouseInCurrentBranch(Factory, dcn.WDC_WW_Warehouse);

			if (!canOpenForm)
			{
				Globals.Message.ShowError(Res.GetString("WhsTransitDispatchConsignmentController|GetForm", "The DCN could not be found. Please check that you are logged into the DCN's Warehouse Branch and try again."));
			}

			return canOpenForm ? new DispatchConsignmentForm(dcn) : null;
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("WhsTransitDispatchConsignmentController|ShowFormForNewEntity", "You cannot create a Transit Dispatch Consignment on the {0} Desktop, please go to the Transit Warehouse Management Portal.", "CargoWise"));

			return null;
		}
	}
}
