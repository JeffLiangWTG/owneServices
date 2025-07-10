using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.GUI
{
	public static class ManifestFormhelper
	{
		public static void CreateANewDeclaration(Trip tripBO)
		{
			var declarationReferences = new ZStringBuilder();
			var dialogResult = DialogResult.Abort;
			Shipment[] selectedShipments = null;

			var shipments = tripBO.Shipments.Where(x => x != null && !x.B0_ReferenceID.IsEmpty).ToArray();
			using (var dlg = new ShipmentItemSelectionDialog(tripBO, shipments))
			{
				dlg.DeSelectAll();
				dialogResult = ZFormModaliser.ShowDialogWithoutDispose(dlg);
				selectedShipments = dlg.GetSelectedItems().OfType<Shipment>().ToArray();
			}

			if (dialogResult == DialogResult.OK)
			{
				foreach (var itemShipment in selectedShipments)
				{
					var declaration = itemShipment.CreateCustomsDeclaration(tripBO);
					if (declaration != null)
					{
						var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
						declarationReferences.Append(FormattableString.Invariant($"{itemShipment.B0_ReferenceID} => {declaration.JE_DeclarationReference}"));
						controller.ShowFormForNewEntity(declaration);
						declaration.HasChanges = true;
					}
				}
			}

			if (!declarationReferences.IsEmpty)
			{
				Globals.Message.ShowInformation(Res.GetString("551B5420-315D-4CC3-B547-30CC32EB5932", "Declaration has been created for the following:\r\n{0}", declarationReferences.ToStringWithNewLineBetweenAppends()));
			}
		}
	}
}
