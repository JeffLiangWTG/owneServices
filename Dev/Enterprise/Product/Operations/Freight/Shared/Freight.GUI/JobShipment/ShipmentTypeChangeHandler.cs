using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.GUI
{
	public static class ShipmentTypeChangingHandler
	{
		public static void Handle(CommonShipment shipment, ShipmentTypeChangingCancelEventArgs args)
		{
			Argument.NotNull(shipment, "shipment");
			Argument.NotNull(args, "args");

			bool result = false;
			string caption = Res.GetString("b6b0aac1-8e9b-4bea-805c-2579be6456a0", "Confirm Shipment Type Change");

			switch (args.NewShipmentType)
			{
				case Core.Constants.ShipmentTypes.CoLoadMaster:
				case Core.Constants.ShipmentTypes.BlindCoLoadMaster:
				case Core.Constants.ShipmentTypes.AssemblyMaster:
					result = Globals.Message.Show(Res.GetString("ec4926ee-fb65-4174-9fd9-1f2f9959cb35", @"Marking this shipment as a Co-Load Master or Assembly master will mean that inner pack-line and outer pack-line data will be removed from this shipment, as it will be calculated from the related sub-shipments.

Are you sure you want to continue?"), caption,
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Question, DialogResult.OK) == DialogResult.Cancel;
					break;

				case Core.Constants.ShipmentTypes.StandardHouse:
					result = Globals.Message.Show(Res.GetString("ac2d6266-bf36-422c-9a0b-30b51d8f5e41", @"Changing this shipment to be a Standard shipment will remove all related shipments from it.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?"), caption,
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Question, DialogResult.OK) == DialogResult.Cancel;
					break;

				case Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy:
				case Core.Constants.ShipmentTypes.HighVolumeLowValue:
					if (shipment.IsInDatabase &&
						!Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed &&
						((ZGuid)shipment.JS_JS_ColoadMasterShipmentInfo.OriginalValue).IsValid)
					{
						Globals.Message.Show(Res.GetString("f415a7ba-4ea5-4b2f-aa0e-a131a71ec45e", "Changing this shipment to be a High Volume Low Value shipment needs to remove master but you lack security permissions to '{0}'", Env.Security.MaintainShipmentAllowDetachingSubShipments.DisplayTextPathToSecurityRight),
							caption, MessageBoxButtons.OK, DialogResult.OK);
					}
					else
					{
						result = Globals.Message.Show(Res.GetString("96f77fc9-9398-4139-84f7-849385fb802e", @"Changing this shipment to be a High Volume Low Value shipment will remove Master / Lead shipment and all related shipments.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?"), caption,
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Question, DialogResult.OK) == DialogResult.Cancel;
					}

					break;
			}

			args.Cancel = result;
		}
	}
}
