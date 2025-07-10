using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Licensing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.GUI
{
	public enum CargoIMPPhase2MessageTypes
	{
		RouteMapInformation,
		RouteMapCancellation,
		MilestoneStatusUpdate
	}

	public class CargoIMPPhase2PlugIn : ZPlugIn
	{
		public CargoIMPPhase2PlugIn(ForwardingShipment hostEntity)
			: base(hostEntity)
		{
			this.shipment = hostEntity;

			Enabled = shipment != null
				&& new ZString[]
				{
					Constants.TransportModes.Air,
					Constants.TransportModes.AirSea,
					Constants.TransportModes.SeaAir
				}.Contains(shipment.JS_TransportMode);
		}

		readonly ForwardingShipment shipment;

		#region IZPlugIn Members

		public override string Name
		{
			get { return Res.GetString("99ea603e-6da5-4151-b9f3-f501579f7280", "CargoIMP Phase 2"); }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (Enabled && MainMenuItem == null)
			{
				MainMenuItem = new ZMenuItem(ResString.GetMultilingualString("ShipmentForm|Menu|CargoIMPPhase2", "CargoIMP Phase 2"));

				if (Env.Security.CargoIMPPhase2.IsAllowed)
				{
					MainMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ShipmentForm|Menu|CargoIMPPhase2|SendRouteMap", "Send Route Map"), CreateRouteMapMessage));
					MainMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ShipmentForm|Menu|CargoIMPPhase2|SendCancellation", "Send Cancellation"), CreateCancelMessage));
				}
				else
				{
					MainMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ShipmentForm|Menu|CargoIMPPhase2|AccessDenied", "Access denied, click this menu for details."), AccessDeniedMenuItemClick));
				}
			}

			return MainMenuItem;
		}

		MenuItem MainMenuItem;

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return MessageManager;
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.EzycargoInterfacePhase2; }
		}

		protected override Control GetNewUserControl()
		{
			return new CargoIMPPhase2Control();
		}

		#endregion

		#region Implementation

		CargoIMPPhase2MessageManager MessageManager
		{
			get
			{
				if (this.messageManager == null && this.shipment != null)
				{
					this.messageManager = CargoIMPPhase2MessageManager.New(this.shipment);
				}

				return this.messageManager;
			}
		}
		CargoIMPPhase2MessageManager messageManager;

		bool CanCreateMessage()
		{
			bool doGeneration = false;
			if (!Enabled)
			{
				Globals.Message.Show(Res.GetString("97cf4ed1-9dc1-45b7-a6ee-801cf5ca0582", "CargoIMP Phase 2 is not enabled."));
			}
			else if (this.shipment.HasChanges)
			{
				DialogResult dialogResult = Globals.Message.Show(Res.GetString("9eb02e30-8ed4-42b8-bdfa-3b02c215c8c2", "You must save the form before attempting to send any electronic messages. Do you wish to save the Shipment form now?"), Res.GetString("be84ffb8-3ab2-42c7-b4ac-c35e29887003", "Save"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dialogResult == DialogResult.Yes)
				{
					if (TopLevelTabControl != null)
					{
						ZForm form = TopLevelTabControl.FindForm() as ZForm;
						if (form != null)
						{
							ContinueWithSave continueWithSave = form.FireSaveButton();
							if (continueWithSave == ContinueWithSave.Yes)
							{
								doGeneration = true;
							}
						}
					}
				}
			}
			else
			{
				doGeneration = true;
			}

			if (doGeneration)
			{
				doGeneration = CheckServiceTaskRunning();
			}

			if (doGeneration)
			{
				doGeneration = CheckServiceTaskEnvironment();
			}

			return doGeneration;
		}

		static bool CheckServiceTaskEnvironment()
		{
			bool isCorrect = true;
			NotificationBuffer notifications = new NotificationBuffer();
			if (!ServiceTaskChecker.IsServiceTaskEnvironmentValid(notifications))
			{
				if (Globals.Message.ShowConfirmation(Res.GetString("b0014fc9-e68c-4a04-9037-e9efe96ddaa4", "The CargoIMP Phase 2 Service Task (code CI2) environment is not configured correctly.\r\n\r\nMessages can still be created however sending functionality cannot be performed due to the following reasons:\r\n{0}\r\nPlease type \"YES\" to acknowledge that you understand and have read this explanation.", notifications.AsString), Res.GetString("4d95f15e-a914-481e-925f-91ecee640246", "CargoIMP Phase 2 Service Task"), "YES", MessageBoxIcon.Question, MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					isCorrect = false;
				}
			}

			return isCorrect;
		}

		static bool CheckServiceTaskRunning()
		{
			bool isCorrect = true;
			if (!ServiceTaskChecker.IsServiceTaskActive)
			{
				if (Globals.Message.ShowConfirmation(Res.GetString("fe3232cc-88af-4ab6-b7e7-edfdbfe93bd3", "The CargoIMP Phase 2 Service Task (code CI2) is not currently running.\r\n\r\nMessages can still be created however sending functionality cannot be performed.\r\n\r\nPlease type \"YES\" to acknowledge that you understand and have read this explanation."), Res.GetString("4d95f15e-a914-481e-925f-91ecee640246", "CargoIMP Phase 2 Service Task"), "YES", MessageBoxIcon.Question, MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					isCorrect = false;
				}
			}

			return isCorrect;
		}

		void CreateMessage(CargoIMPPhase2MessageTypes messageType)
		{
			if (CanCreateMessage())
			{
				ZFormModaliser.ShowDialogAndDispose(new CargoIMPPhase2CreateMessageForm(MessageManager, messageType));
			}
		}

		void CreateRouteMapMessage(object sender, EventArgs args)
		{
			NotificationBuffer buffer = new NotificationBuffer();
			if (MessageManager.IsRouteMapInformationMessageCanBeCreated(buffer))
			{
				CreateMessage(CargoIMPPhase2MessageTypes.RouteMapInformation);
			}
			else
			{
				Globals.Message.Show(Res.GetString("e3a540ad-8411-4a24-99f6-118972f6da16", "Route Map Message Can't be Sent:\r\n\r\n{0}", buffer.AsString), Res.GetString("e2e6f0d7-f32f-46be-b1b0-f89b6512eaa3", "Message Error"), MessageBoxButtons.OK, DialogResult.OK);
			}
		}

		void CreateCancelMessage(object sender, EventArgs args)
		{
			if (Globals.Message.Show(Res.GetString("5a571db3-5fe1-4ef0-a4f8-c001b1ac5e89", "Do you really want to send Cancellation?"), Res.GetString("7a2d2ca3-9fc6-4b45-9a8e-1a05211fe6fe", "CargoIMP Phase 2 Cancellation"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
			{
				NotificationBuffer buffer = new NotificationBuffer();
				if (MessageManager.IsRouteMapCancellationMessageCanBeCreated(buffer))
				{
					CreateMessage(CargoIMPPhase2MessageTypes.RouteMapCancellation);
				}
				else
				{
					Globals.Message.Show(Res.GetString("4e10d8c6-34b2-4f86-aa0a-6d33dcbbac88", "Cancellation Message Can't be Sent:\r\n\r\n{0}", buffer.AsString), Res.GetString("e2e6f0d7-f32f-46be-b1b0-f89b6512eaa3", "Message Error"), MessageBoxButtons.OK, DialogResult.OK);
				}
			}
		}

		void AccessDeniedMenuItemClick(object sender, EventArgs args)
		{
			Env.Security.CargoIMPPhase2.ShowError();
		}

		#endregion
	}
}
