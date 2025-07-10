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
	public class WhsItemReceiveASNController : WhsTransitController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.WhsItemReceiveASN; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsItemReceiveASN; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsItemReceiveASN); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var asn = (WhsItemReceiveASN)businessEntity;
			var canOpenForm = TransitWarehouseHelper.IsTransitWarehouseInCurrentBranch(Factory, asn.WRP_WW_IntendedWarehouse);

			if (!canOpenForm)
			{
				Globals.Message.ShowError(Res.GetString("WhsItemReceiveASNController|GetForm", "The ASN could not be found. Please check that you are logged into the ASN's Warehouse Branch and try again."));
			}

			return canOpenForm ? new ReceiveASNForm(asn) : null;
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("WhsItemReceiveASNController|ShowFormForNewEntity", "You cannot create a Transit Advanced Shipping Notice on the {0} Desktop, please go to the Transit Warehouse Management Portal.", "CargoWise"));

			return null;
		}
	}
}
