using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class UniversalDataCarrierMessagingPlugIn : ZPlugIn
	{
		public UniversalDataCarrierMessagingPlugIn(ForwardingConsol consol)
			: base(consol)
		{
			this.consol = Argument.NotNull(consol, "consol");
			Init();
		}

		readonly ForwardingConsol consol;

		protected override MenuItem GetNewTopLevelMenu()
		{
			return CarrierMessageMenuItem;
		}

		MenuItem CarrierMessageMenuItem
		{
			get
			{
				var menuCaption = ResString.GetMultilingualString("807f1481-54a0-4c17-8d30-86dcb3bafe0d", "Send Forward Air Booking Request");
				return carrierMessageMenuItem ?? (carrierMessageMenuItem = new ZMenuItem(menuCaption, SendCarrierMessage));
			}
		}

		MenuItem carrierMessageMenuItem;

		void SendCarrierMessage(object sender, EventArgs args)
		{
			if (CanCreateForwardAirMessage())
			{
				using (var progressForm = new ManualDataExportProgressForm())
				{
					var validation = CarrierMessagingValidationFactory.GetValidation(consol);
					var behaviour = new ManualDataExportProgressFormCarrierMessagingBehaviour(
						CarrierMessageMenuItem.Text,
						consol,
						validation);

					behaviour.Apply(progressForm);
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);
				}
			}
		}

		bool CanCreateForwardAirMessage()
		{
			bool sendForwardAirMessage = false;
			if (consol.HasChanges)
			{
				Globals.Message.Show(Res.GetString("bba2950e-a3b3-4e73-90d3-5d52c6e80763", "Please save your changes before you continue."));
			}
			else
			{
				sendForwardAirMessage = true;
			}

			return sendForwardAirMessage;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return consol;
		}

		protected override ZBool HasUserControl
		{
			get { return false; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Forwarder; }
		}

		public override string Name
		{
			get { return "CarrierMessaging"; }
		}

		void Init()
		{
			HookEventsForConsol();

			SetEnabled();
		}

		void SetEnabled()
		{
			Enabled = consol.IsSuitableForForwardAirMessage();
		}

		#region Events on Consol

		void ConsolTranportModeOrLoadPortChanged(object sender, EventArgs e)
		{
			SetEnabled();
		}

		void HookEventsForConsol()
		{
			consol.JK_TransportModeInfo.ValueChanged += ConsolTranportModeOrLoadPortChanged;
			consol.JK_RL_NKLoadPortInfo.ValueChanged += ConsolTranportModeOrLoadPortChanged;

			HookTransports();

			consol.Transports.CountChanged += ConsolRoutingListChanged;
		}

		void ConsolRoutingListChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var transport = (Transport)e.BizObject;

			if (e.ItemAdded)
			{
				HookTransport(transport);
			}

			if (e.ItemRemoved)
			{
				ConsolTranportModeOrLoadPortChanged(sender, e);
				UnHookTransport(transport);
			}
		}

		void UnHookTransport(Transport transport)
		{
			transport.JW_RL_NKLoadPortInfo.ValueChanged -= ConsolTranportModeOrLoadPortChanged;
			transport.JW_TransportModeInfo.ValueChanged -= ConsolTranportModeOrLoadPortChanged;
		}

		void HookTransports()
		{
			foreach (Transport transport in consol.Transports)
			{
				transport.JW_RL_NKLoadPortInfo.ValueChanged += ConsolTranportModeOrLoadPortChanged;
				transport.JW_TransportModeInfo.ValueChanged += ConsolTranportModeOrLoadPortChanged;
			}
		}

		void HookTransport(Transport transport)
		{
			transport.JW_RL_NKLoadPortInfo.ValueChanged += ConsolTranportModeOrLoadPortChanged;
			transport.JW_TransportModeInfo.ValueChanged += ConsolTranportModeOrLoadPortChanged;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookEventsForConsol();
			}

			base.Dispose(disposing);
		}

		void UnHookEventsForConsol()
		{
			consol.JK_TransportModeInfo.ValueChanged -= ConsolTranportModeOrLoadPortChanged;
			consol.JK_RL_NKLoadPortInfo.ValueChanged -= ConsolTranportModeOrLoadPortChanged;

			UnHookTransports();

			consol.Transports.CountChanged -= ConsolRoutingListChanged;
		}

		void UnHookTransports()
		{
			foreach (Transport transport in consol.Transports)
			{
				transport.JW_RL_NKLoadPortInfo.ValueChanged -= ConsolTranportModeOrLoadPortChanged;
				transport.JW_TransportModeInfo.ValueChanged -= ConsolTranportModeOrLoadPortChanged;
			}
		}

		#endregion
	}
}
