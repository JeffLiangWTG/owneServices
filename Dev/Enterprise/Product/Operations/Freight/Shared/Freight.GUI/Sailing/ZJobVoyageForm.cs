using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.GUI
{
	public partial class ZJobVoyageForm : ZTemplateForm
	{
		[Obsolete("You should use the constructor that takes a voyage, this constructor is just for the designer")]
		ZJobVoyageForm() { }

		public ZJobVoyageForm(JobVoyage voyage)
			: base(voyage)
		{
			PlugIns.Add(ControllerIDs.AgencyAllocation);
			PlugIns.Add(ControllerIDs.Customs.AU.ArrivalPluginToSailingController);

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
			{
				messagingTabControl.PlugIns.Add(ControllerIDs.AgencyPortMessaging);
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.SouthAfrica)
			{
				messagingTabControl.PlugIns.Add(ControllerIDs.Customs.ZA.CALINFMessagingPlugin);
			}

			if (voyage.IsSea)
			{
				PlugIns.Add(ControllerIDs.Customs.AU.ManifestPluginToSailingController);
				messagingTabControl.PlugIns.Add(ControllerIDs.Customs.US.StowPlan);

				messagingTabControl.PlugIns.Add(ControllerIDs.AgencyNZPortMessaging);
				messagingTabControl.PlugIns.Add(ControllerIDs.AgencyDangerousGoodsManifest);
				InitialiseShippingManagerMenu();

				AddMenuItemToCreateSlotVoyageFromMainOne();

				PlugIns.Add(ControllerIDs.ETerminalReleaseManifestPortMessaging);
				InitialiseElectronicMessagingMenu();
			}

			if (!this.IsDesignMode())
			{
				ScheduleUpdateGuiQueryProvider.Set(voyage.Factory, CommonConsol.AdditionalETDUpdateMsg, CommonConsol.AdditionalETAUpdateMsg);
			}

			BusyIndicatorProvider.Register(this);

			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItems);
			workflowTabPage.Initialize(voyage);

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Voyage != null)
			{
				UnhookEvents(Voyage);
			}
			base.SetDataBinding(dataSource, dataMember);
			if (Voyage != null)
			{
				HookEvents(Voyage);
			}
		}

		public override string FormCaption
		{
			get
			{
				if (!this.IsDesignMode())
				{
					switch (Voyage.JV_AirSeaRoad)
					{
						case Core.Constants.TransportModes.Air:
							return Res.GetString("Freight|JobVoyageForm|FormCaption|FlightSchedule", "Flight Schedule");

						case Core.Constants.TransportModes.Sea:
							return Res.GetString("Freight|JobVoyageForm|FormCaption|SailingSchedule", "Sailing Schedule");

						case Core.Constants.TransportModes.Road:
							return Res.GetString("Freight|JobVoyageForm|FormCaption|TruckingSchedule", "Trucking Schedule");

						case Core.Constants.TransportModes.Rail:
							return Res.GetString("Freight|JobVoyageForm|FormCaption|RailSchedule", "Rail Schedule");
					}
				}

				return Res.GetString("Freight|JobVoyageForm|FormCaption|Schedule", "Schedule");
			}
		}

		#region Events

		void CannotDeletePort(object sender, CannotDeletePortEventArgs e)
		{
			string caption = Res.GetString("b7731d3b-3caf-48ed-8f88-c47c2cda1709", "Cannot Delete Port");
			Globals.Message.ShowWarning(e.ReasonForNotAbleToDelete, caption);
		}

		#endregion

		#region Sea Voyages Specific

		void AddMenuItemToCreateSlotVoyageFromMainOne()
		{
			if (!Voyage.IsSea)
			{
				return;
			}

			var createSlotVoyageMenuItem = new ZMenuItem(ResString.GetMultilingualString("e28a68c0-f154-4486-ac2d-e10c877233a3", "Create Slot Sailing Schedule"));
			createSlotVoyageMenuItem.Click += delegate
			{
				if (!Voyage.IsMainVoyage)
				{
					Globals.Message.ShowError(Res.GetString("8db94da1-940e-49e3-86b5-47ea0dc6272a", "Slot Sailing Schedule can be created from the Main Sailing Schedule only."));
					return;
				}
				else if (!Voyage.IsInDatabase || Voyage.HasChanges)
				{
					Globals.Message.ShowWarning(Res.GetString("49ec784d-eeab-4864-9448-1adf2ea75866", "Please save the voyage before proceeding."));
					return;
				}
				else
				{
					var controller = ZControllerFactory.Create(ControllerIDs.JobSeaVoyage);

					var reloadedVoyage = controller.Factory.Load<JobVoyage>(Voyage.PK);
					if (reloadedVoyage != null)
					{
						var clonedVoyage = reloadedVoyage.Clone() as JobVoyage;
						if (clonedVoyage != null)
						{
							clonedVoyage.JV_OH_Line = ZGuid.Empty;
							clonedVoyage.JV_VoyageType = Constants.VoyageType.SlotVoyage;

							controller.ShowEditForm(clonedVoyage);
						}
					}
				}
			};

			ZFormMenuStrategy.AddActionsMenuItem(this, createSlotVoyageMenuItem);
			ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem("-"));
		}

		#endregion

		#region Electronic Messaging

		void InitialiseElectronicMessagingMenu()
		{
			var voyage = Voyage;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China
				&& voyage.Origins.Cast<VoyageOrigin>().Any(o => new[] { "CNNGB", "CNNBO", "CNNBG" }.Any(s => s == o.JA_RL_NKPortOfLoading)))
			{
				var electronicMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight|JobVoyageForm|ElectronicMessaging", "Electronic Messaging"));
				portMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight|JobVoyageForm|PortMessaging", "Port Messaging"));

				MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, electronicMessagingMenuItem);
				electronicMessagingMenuItem.MenuItems.Add(portMessagingMenuItem);
			}
		}

		ZMenuItem portMessagingMenuItem;

		#endregion

		#region Shipping Manager

		void InitialiseShippingManagerMenu()
		{
			shippingManagerMenuItem = new ZMenuItem(ResString.GetMultilingualString("60048e30-1ed2-4389-a7cd-959dc98f5519", "Shipping Manager"));

			MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, shippingManagerMenuItem);

			shippingManagerMenuItem.Visible = (messagingTabControl.PlugIns.GetPlugIn(ControllerIDs.AgencyPortMessaging)?.Enabled ?? false)
				|| (messagingTabControl.PlugIns.GetPlugIn(ControllerIDs.AgencyNZPortMessaging)?.Enabled ?? false)
				|| (messagingTabControl.PlugIns.GetPlugIn(ControllerIDs.AgencyDangerousGoodsManifest)?.Enabled ?? false);
		}

		ZMenuItem shippingManagerMenuItem;

		#endregion

		#region Implementation

		List<MenuItem> ExportToXmlMenuItems
		{
			get
			{
				return new ExportToXmlMenuItemSet<JobVoyage>(() => Exporter, (JobVoyage)BusinessEntity);
			}
		}

		IXmlDataTransferExporter Exporter
		{
			get
			{
				return new XmlDataTransferExporter(new ScheduleValueObjectDataAdapter(), true);
			}
		}

		JobVoyage Voyage
		{
			get { return (JobVoyage)DataSource; }
		}

		void HookEvents(JobVoyage voyage)
		{
			Voyage.Destinations.CannotDeletePort += CannotDeletePort;
			Voyage.Origins.CannotDeletePort += CannotDeletePort;
		}

		void UnhookEvents(JobVoyage voyage)
		{
			Voyage.Destinations.CannotDeletePort -= CannotDeletePort;
			Voyage.Origins.CannotDeletePort -= CannotDeletePort;
		}

		protected override Menu GetMenuForPlugInCore(ControllerID controllerID)
		{
			if (controllerID == ControllerIDs.ETerminalReleaseManifestPortMessaging)
			{
				return portMessagingMenuItem;
			}

			var controllersInShippingManagerMenu = new List<ControllerID>()
			{
				ControllerIDs.AgencyPortMessaging,
				ControllerIDs.AgencyNZPortMessaging,
				ControllerIDs.AgencyDangerousGoodsManifest
			};

			if (controllersInShippingManagerMenu.Contains(controllerID))
			{
				return shippingManagerMenuItem;
			}

			return base.GetMenuForPlugInCore(controllerID);
		}

		#endregion
	}
}
