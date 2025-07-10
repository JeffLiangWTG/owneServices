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
	public class WhsTransitReceiveConsignmentController : WhsTransitController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.WhsTransitReceiveConsignment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsTransitReceiveConsignment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsItemReceiveConsignment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var rcn = (WhsItemReceiveConsignment)businessEntity;
			var canOpenForm = TransitWarehouseHelper.IsTransitWarehouseInCurrentBranch(Factory, rcn.WRC_WW_IntendedWarehouse);

			if (!canOpenForm)
			{
				Globals.Message.ShowError(Res.GetString("WhsTransitReceiveConsignmentController|GetForm", "The RCN could not be found. Please check that you are logged into the RCN's Warehouse Branch and try again."));
			}

			return canOpenForm ? new ReceiveConsignmentForm(rcn) : null;
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("WhsTransitReceiveConsignmentController|ShowFormForNewEntity", "You cannot create a Transit Receive Consignment on the {0} Desktop, please go to the Transit Warehouse Management Portal.", "CargoWise"));

			return null;
		}
	}
}
