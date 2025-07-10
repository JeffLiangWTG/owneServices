using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Agency.Module.DangerousGoodsManifest;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module
{
	public class DangerousGoodsManifestPlugin : ZPlugIn
	{
		public DangerousGoodsManifestPlugin(JobVoyage voyage) : base(voyage)
		{
			this.voyage = voyage;

			voyage.Origins.CountChanged -= OnOriginsAndDestinationsCountChanged;
			voyage.Origins.CountChanged += OnOriginsAndDestinationsCountChanged;
			voyage.Destinations.CountChanged -= OnOriginsAndDestinationsCountChanged;
			voyage.Destinations.CountChanged += OnOriginsAndDestinationsCountChanged;

			HookEventsForAllOriginsAndDestinations();
		}
		readonly JobVoyage voyage;

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (Env.Security.SailingSchedulePortMessaging.IsAllowed)
			{
				var itemName = ResString.GetMultilingualString("614a64bd-c1ee-4fa4-9a39-609d5ec9196a", "Send Dangerous Goods Manifest Message");

				return new ZMenuItem(itemName, SendDangerousGoodsManifestMessageMenuItemClicked);
			}
			else
			{
				var itemName = ResString.GetMultilingualString("99266d95-1f17-4469-97a3-fe815d620e2e", "Access denied, click this menu for details.");

				return new ZMenuItem(itemName, (_, x_) => Env.Security.SailingSchedulePortMessaging.ShowError());
			}
		}

		#region MenuItem Visibility

		void OnOriginsAndDestinationsCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var origin = e.BizObject as VoyageOrigin;
			var destination = e.BizObject as VoyageDestination;
			if (e.ItemRemoved)
			{
				if (origin != null)
				{
					origin.JA_RL_NKPortOfLoadingInfo.ValueChanged -= ChangeTheVisibility;
				}

				if (destination != null)
				{
					destination.JB_RL_NKPortOfDischargeInfo.ValueChanged -= ChangeTheVisibility;
				}
			}
			else if (e.ItemAdded)
			{
				if (origin != null)
				{
					origin.JA_RL_NKPortOfLoadingInfo.ValueChanged -= ChangeTheVisibility;
					origin.JA_RL_NKPortOfLoadingInfo.ValueChanged += ChangeTheVisibility;
				}

				if (destination != null)
				{
					destination.JB_RL_NKPortOfDischargeInfo.ValueChanged -= ChangeTheVisibility;
					destination.JB_RL_NKPortOfDischargeInfo.ValueChanged += ChangeTheVisibility;
				}
			}

			ChangeTheVisibility(null, null);
		}

		void HookEventsForAllOriginsAndDestinations()
		{
			foreach (VoyageOrigin origin in voyage.Origins)
			{
				origin.JA_RL_NKPortOfLoadingInfo.ValueChanged -= ChangeTheVisibility;
				origin.JA_RL_NKPortOfLoadingInfo.ValueChanged += ChangeTheVisibility;
			}

			foreach (VoyageDestination destination in voyage.Destinations)
			{
				destination.JB_RL_NKPortOfDischargeInfo.ValueChanged -= ChangeTheVisibility;
				destination.JB_RL_NKPortOfDischargeInfo.ValueChanged += ChangeTheVisibility;
			}

			ChangeTheVisibility(null, null);
		}

		void ChangeTheVisibility(object sender, EventArgs e)
		{
			Enabled = DangerousGoodsManifestHelper.IsVoyageContainsEnabledPorts(voyage);

			if (TopLevelMenu.Parent != null)
			{
				((MenuItem)TopLevelMenu.Parent).Visible = TopLevelMenu.Parent.MenuItems.Cast<MenuItem>().Any(i => i.Visible);
			}
		}

		#endregion

		#region MenuItem Clicked

		void SendDangerousGoodsManifestMessageMenuItemClicked(object sender, EventArgs e)
		{
			if (voyage.HasChanges || !voyage.IsInDatabase)
			{
				Globals.Message.ShowError(Res.GetString("81a12169-1db8-4b81-87d7-061226440efd", "This schedule has changes, please save and try again."), Res.GetString("4f8955b0-d67e-4f57-b28a-acf8703bd985", "Unsaved Changes"));
			}
			else if (!voyage.IsMainVoyage && voyage.FindOtherVoyagesWithSameVesselVoyageCombination().Any(v => v.IsMainVoyage))
			{
				Globals.Message.ShowInformation(Res.GetString("75a93c85-f31e-49d7-a555-1d783a6e0e88", "You have Main and Slot schedules for this vessel. Please send Dangerous Goods Manifest Message from the Main schedule."));
			}
			else
			{
				voyage.RunPreSaveValidation();

				if (voyage.HasErrors)
				{
					Globals.Message.ShowError(Res.GetString("1af5ab59-9e2d-4ef8-a65d-d2b793180228", "This schedule has errors, please fix and try again."), Res.GetString("58aaae04-5513-4016-9c41-2c4edffb7f22", "Errors"));
				}
				else
				{
					DangerousGoodsManifestMessageDialog.ShowDialog(voyage);
				}
			}
		}

		#endregion

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			if (disposing && voyage != null)
			{
				voyage.Origins.CountChanged -= OnOriginsAndDestinationsCountChanged;
				voyage.Destinations.CountChanged -= OnOriginsAndDestinationsCountChanged;

				foreach (VoyageOrigin origin in voyage.Origins)
				{
					origin.JA_RL_NKPortOfLoadingInfo.ValueChanged -= ChangeTheVisibility;
				}

				foreach (VoyageDestination destination in voyage.Destinations)
				{
					destination.JB_RL_NKPortOfDischargeInfo.ValueChanged -= ChangeTheVisibility;
				}
			}

			base.Dispose(disposing);
		}

		public override string Name => Res.GetString("17db17e9-26f5-4972-9050-885f6e898495", "Dangerous Goods Manifest");

		protected override ZBool HasUserControl => false;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.ShippingManager;

		#endregion
	}
}
