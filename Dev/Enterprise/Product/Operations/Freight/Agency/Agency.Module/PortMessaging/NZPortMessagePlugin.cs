using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Agency.Module.PortManifest;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module
{
	public class NZPortMessagePlugin : ZPlugIn
	{
		public NZPortMessagePlugin(JobVoyage voyage)
			: base(voyage)
		{
			this.voyage = voyage;

			voyage.Origins.CountChanged -= OnOriginsAndDestinationsCountChanged;
			voyage.Origins.CountChanged += OnOriginsAndDestinationsCountChanged;
			voyage.Destinations.CountChanged -= OnOriginsAndDestinationsCountChanged;
			voyage.Destinations.CountChanged += OnOriginsAndDestinationsCountChanged;

			HookEventsForAllOriginsAndDestinations();
		}

		#region ZPlugIn

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (Env.Security.SailingSchedulePortMessaging.IsAllowed)
			{
				var itemName = ResString.GetMultilingualString("27757aef-468a-4258-8b7c-03271e7d2321", "Send Load and Discharge Manifest Message");

				return new ZMenuItem(itemName, SendPortMessageMenuItemClicked);
			}
			else
			{
				var itemName = ResString.GetMultilingualString("6a29bb54-a3f1-4435-b5b0-9531db3aba89", "Access denied, click this menu for details.");

				return new ZMenuItem(itemName, (_, x_) => Env.Security.SailingSchedulePortMessaging.ShowError());
			}
		}

		protected override ZBool HasUserControl
		{
			get { return false; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ShippingManager; }
		}

		public override string Name
		{
			get { return Res.GetString("e1193616-7cc8-4bc0-8038-46a4fc2cbe3b", "Port Messages"); }
		}

		#endregion

		#region Implementation

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
			Enabled = PortManifestHelper.IsVoyageContainsEnabledPorts(voyage);

			if (TopLevelMenu.Parent != null)
			{
				((MenuItem)TopLevelMenu.Parent).Visible = TopLevelMenu.Parent.MenuItems.Cast<MenuItem>().Any(i => i.Visible);
			}
		}

		#endregion

		void SendPortMessageMenuItemClicked(object sender, EventArgs args)
		{
			if (voyage.HasChanges || !voyage.IsInDatabase)
			{
				Globals.Message.ShowError(Res.GetString("e282a702-b6f9-11e4-b09e-902b34dc814a", "This schedule has changes, please save and try again."), Res.GetString("a61c10e6-b6f9-11e4-bb85-902b34dc814a", "Unsaved Changes"));
			}
			else if (!voyage.IsMainVoyage && voyage.FindOtherVoyagesWithSameVesselVoyageCombination().Any(v => v.IsMainVoyage))
			{
				Globals.Message.ShowInformation(Res.GetString("831e4a93-c645-41c2-ba3f-1df567ae7c73", "You have Main and Slot schedules for this vessel. Please send Load and Discharge Manifest Message from the Main schedule."));
			}
			else
			{
				voyage.RunPreSaveValidation();

				if (voyage.HasErrors)
				{
					Globals.Message.ShowError(Res.GetString("c20a4cf0-b6f9-11e4-b590-902b34dc814a", "This schedule has errors, please fix and try again."), Res.GetString("c9451dec-b6f9-11e4-824d-902b34dc814a", "Errors"));
				}
				else
				{
					NZPortMessageDialog.ShowDialog(voyage);
				}
			}
		}

		readonly JobVoyage voyage;

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

		#endregion
	}
}



