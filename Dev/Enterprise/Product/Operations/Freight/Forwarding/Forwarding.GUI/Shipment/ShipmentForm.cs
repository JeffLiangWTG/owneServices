using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using DataTransfer.Common.GUI.MenuItems;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.HelperClasses;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Common.TemplateRecords;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using Constants = Enterprise.Core.Constants;
using FreightRegistry = Enterprise.Registry.Business.FreightDataRegistry;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentForm : ZForm,
		Integration.Forwarding.IForwardingShipmentForm,
		ITabVisibilityDeciderPersistence,
		INotifications,
		IRequireInactivationPrompt,
		ISupportSwitchTabPage,
		ICustomerServiceMenuSectionCodeOverridable,
		IPreviousNextControlOverrideProvider
	{
		// Do not remove. Used by Form Designer when open inherited forms
		protected ShipmentForm()
		{
			InitializeComponent();
		}

		public ShipmentForm(ForwardingShipment shipment)
			: base(shipment)
		{
			if (shipment != null && ChildEditableService.GetState(shipment.Factory) != ChildEditableServiceStates.Shipment)
			{
				ErrorReporter.ReportOnce("The state should be set to ChildEditableServiceStates.Shipment before loading or instantiating the Shipment if this Shipment is to be shown on a ShipmentForm");
			}

#if !WINZOR
			Enterprise.Freight.Forwarding.PerformanceMonitor.PerformanceTracker.Instance.StartTrack(nameof(Load), shipment.GetType().Name, shipment.PK.ToString(), shipment.Factory.SaveCount);
#endif
			InitializeComponent();

			shipment.RequestPermissionByImpersonation = ShipmentVsConsolGUIMessageHelper.Instance.RequestPermissionByImpersonation;

			ShipmentDetailsTabPage.Controls.Add(ShipmentUserControl);

			if (shipment != null && shipment.IsTemplate)
			{
				WorkflowTabPage.Dispose();
			}
			else
			{
				WorkflowTabPage.Initialize(shipment);
			}

			ActionsMenuItem.AddDeferredMenuItems(this, GetActionMenuItems);
			ActionsMenuItem.Popup += (s, e) =>
			{
				InitialiseTransitWarehouseMenuItem();
				DisableResetValuesFromSubShipmentsMenuItemWhenHVLShipmentType();
				InitialiseProductWarehouseMenuItem(Shipment.RelatedWarehouseReceive);
				OrderActionMenu();
				ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this);
			};

			if (shipment != null)
			{
				shipment.OnExportOrImportBrokerUpdate += new EventHandler<BrokerDefaultingEventArgs>(UpdateExportOrImportBrokerHelper.UpdateExportOrImportBroker);
				if (!Shipment.IsTemplate)
				{
					shipment.GetReasonChangingSecurityInspectionStatusEventHandler += MessagePopupHelper.PromptReasonForChangingSecurityInspectionStatusEventHandler;
					shipment.GetReasonChangingSecurityAdditionalInspectionStatusEventHandler += MessagePopupHelper.PromptReasonForChangingSecurityAdditionalInspectionStatusEventHandler;
					shipment.GetReasonForChangingDeliveryDueDateEventHandler += MessagePopupHelper.PromptReasonForChangingDeliveryDueDateEventHandler;
					shipment.DeliveryDueDateNotChangedInManualCalculation += MessagePopupHelper.NotifyDeliveryDueDateNotChangedInManualCalculation;
				}
			}

			InitialiseScreeningMenuItems();
			InitialiseElectronicMessagingMenu();

			InitialisePlugIns();

			SetupForm();

			ReportBigNumberOfFactories();
		}

		public override IBusiness BusinessEntityForHasChanges => ZTemplateForm.GetTopLevelBusinessObjectIfTemplateRecord(base.BusinessEntityForHasChanges);
		protected override IBusiness BusinessEntityForValidation => ZTemplateForm.GetTopLevelBusinessObjectIfTemplateRecord(base.BusinessEntityForValidation);

		#region Shipment

		protected ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)DataSource; }
		}

		#endregion

		#region SkipRecentItems

		public bool? SkipRecentItems { get; set; }

		protected override void SaveToRecentItems()
		{
			if (!SkipRecentItems.HasValue || !SkipRecentItems.Value)
			{
				if (!IsConvertingFromBooking(Shipment))
				{
					base.SaveToRecentItems();
				}
			}
		}

		static bool IsConvertingFromBooking(ForwardingShipment shipment)
		{
			if (shipment == null || !shipment.IsInDatabase || !shipment.JS_IsForwardRegisteredInfo.HasChanges)
			{
				return false;
			}

			return shipment.JS_IsBooking && shipment.JS_IsForwardRegistered && !((ZBool)shipment.JS_IsForwardRegisteredInfo.OriginalValue);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

#if !WINZOR
			if (Shipment != null)
			{
				Enterprise.Freight.Forwarding.PerformanceMonitor.PerformanceTracker.Instance.EndTrack(nameof(Load), Shipment.GetType().Name, Shipment.PK.ToString(), Shipment.OuterPackLines.Count);
			}
#endif

			ShowAlertWhenEHBLRequestsPending(Shipment);
		}

		#endregion

		#region ElectronicMessaging Menu

		IDocDataObjectWithoutUIMessageSender CargoReceiptAdviceMessageSender
		{
			get
			{
				if (cargoReceiptAdviceMessageSender == null)
				{
					var processorDict = ObjectFactory.Get<Hashtable>("DocDataObjectWithoutUIMessageSendersProvider");
					cargoReceiptAdviceMessageSender = ((ObjectHandle)processorDict["CargoReceiptAdvice"])?.GetObject() as IDocDataObjectWithoutUIMessageSender;
				}
				return cargoReceiptAdviceMessageSender;
			}
		}
		IDocDataObjectWithoutUIMessageSender cargoReceiptAdviceMessageSender;

		void SendCargoReceiptAdvice(BusinessObject bizObj)
		{
			if (bizObj is ForwardingShipment shipment)
			{
				var showMessage = shipment.BookingParty == null
					? Res.GetString("7C46FDE1-C5E0-47AE-AD22-5B89B308534D", @"Do you want to send Cargo Receipt Advice to Booking Party?")
					: Res.GetString("BDAF519A-27FA-4495-9F16-4B438DE3C557", @"Do you want to send Cargo Receipt Advice to Booking Party: {0}?", shipment.BookingParty.OH_FullName);

				var confirmationDialogResult = Globals.Message.Show(showMessage, Res.GetString("F772B3D5-B6A0-4B67-BA98-2EF1BDBFDE0E", "Cargo Receipt Advice"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);

				if (confirmationDialogResult == DialogResult.No)
				{
					return;
				}

				var message = string.Empty;
				var notificationsHandler = new NotificationsHandler();
				if (CargoReceiptAdviceMessageSender.SendMessage(shipment, notificationsHandler))
				{
					message = Res.GetString("543B32E5-280D-48A2-8011-BEF949EF445D", "Cargo Receipt Advice has been sent.");
				}

				else
				{
					message = string.Join(System.Environment.NewLine, notificationsHandler.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.MessageError).Select(x => x.Message));
				}

				if (!string.IsNullOrWhiteSpace(message))
				{
					var caption = Res.GetString("36928037-AF47-434B-A47F-B32CD6B9347E", "Information");
					Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		bool IsCargoReceiptAdviceApplicable(BusinessObject bizObj)
		{
			if (bizObj is ForwardingShipment shipment)
			{
				var hasHIRReference = shipment.Numbers.Cast<CusEntryNumber>()
				.Any(x => !x.IsDeleted && x.CE_EntryType == CustomsReferenceNumberType.eHubInterchangeReference.HIR && x.CE_EntryNum.StartsWith("SHP"));

				return shipment.TransportMode == TransportModes.Sea
					&& shipment.IsElectronicBookingReceived
					&& hasHIRReference;
			}

			return false;
		}

		void InitialiseElectronicMessagingMenu()
		{
			if (Shipment == null || !Shipment.IsTemplate)
			{
				electronicMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.ElectronicMessaging", "Electronic Messaging"));
				MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, electronicMessagingMenuItem);

				electronicMessagingMenuItem.AddFormsMenuItems(Shipment,
					ModuleIDs.JobShipment,
					CreateElectronicMessagingMenuItemInfos());
			}
		}

		IEnumerable<IMenuItemInfo> CreateElectronicMessagingMenuItemInfos()
		{
			yield return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("520c8698-1cdd-4198-8c15-d06317da4cd6", "Booking Party"),
				SubMenus = new IMenuItemInfo[]
				{
					new SystemMenuItemInfo
					{
						ID = ShipmentSystemFormMenuItems.DocumentMenuConsolidationAdvicePK
					},
					new CustomMenuItemInfo
					{
						Name = Res.GetString("258CCB29-CAEB-46AD-8584-4C1D3AC968DC", "Cargo Receipt Advice"),
						OnClick = SendCargoReceiptAdvice,
						IsApplicable = IsCargoReceiptAdviceApplicable
					},
					new SystemMenuItemInfo
					{
						ID = ShipmentSystemFormMenuItems.DocumentMenuCarrierBillOfLadingPK
					}
				}
			};

			yield return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("D430DF41-6D05-4B48-9764-AB221EB01E08", "Carrier"),
				SubMenus = new IMenuItemInfo[]
				{
					new SystemMenuItemInfo
					{
						ID = ShipmentSystemFormMenuItems.EasipassPK
					},
					new SystemMenuItemInfo
					{
						ID = ShipmentSystemFormMenuItems.BookingRequestPK
					}
				}
			};

			yield return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("18d8fa7f-bf49-2f85-4427-6592135ebdf0", "Advanced Air Cargo Report"),
				SubMenus = new[]
				{
					new SystemMenuItemInfo
					{
						ID = ShipmentSystemFormMenuItems.DocumentMenuACASShipmentReport
					},
					new SystemMenuItemInfo
					{
						ID = ShipmentSystemFormMenuItems.DocumentMenuCCTShipmentReport
					}
				}
			};

			yield return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("340CAB95-405F-4417-BBA9-F68D68AAC7B2", "Port Messaging"),
				SubMenus = new[]
				{
						new ParentMenuItemInfo
						{
							Name = ResString.GetMultilingualString("1A2B09D3-FE28-4CF8-A08F-6DC4067DBAF6","Export"),
							SubMenus = new[]
							{
								new SystemMenuItemInfo
								{
									ID =  ShipmentSystemFormMenuItems.DocumentMenuDOSRequestFRPortMessagingExportPK
								},
								new SystemMenuItemInfo
								{
									ID = ShipmentSystemFormMenuItems.DocumentMenuCAEDRequestFRPortMessagingExportPK
								},
								new SystemMenuItemInfo
								{
									ID = ShipmentSystemFormMenuItems.DocumentMenuGoodsReceivedFRPortMessagingExportPK
								},
								new SystemMenuItemInfo
								{
									ID = ShipmentSystemFormMenuItems.CINExportNotificationPK
								}
							}
						},
						new ParentMenuItemInfo
						{
							Name = ResString.GetMultilingualString("56897B24-8A10-4372-B6F9-8B13DA6864E4","Import"),
							SubMenus = new[]
							{
								new SystemMenuItemInfo
								{
									ID = ShipmentSystemFormMenuItems.DocumentMenuDOSRequestFRPortMessagingImportPK
								},
								new SystemMenuItemInfo
								{
									ID = ShipmentSystemFormMenuItems.DocumentMenuCAEDRequestFRPortMessagingImportPK
								}
							}
						}
					}
			};

			yield return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("340CAB95-405F-4417-BBA9-F68D68AAC7B2", "Port Messaging"),
				SubMenus = new[]
				{
					new ParentMenuItemInfo
					{
						Name = ResString.GetMultilingualString("1A2B09D3-FE28-4CF8-A08F-6DC4067DBAF6", "Export"),
						SubMenus = new[]
						{
							new SystemMenuItemInfo
							{
								ID = ShipmentSystemFormMenuItems.DocumentMenuXFZBRequestMXPortMessagingExportPK
							}
						}
					}
				}
			};
		}

		#endregion

		#region Actions Menu

		void InitialiseScreeningMenuItems()
		{
			var deniedPartyScreeningPresentationManager = new DeniedPartyScreeningPresentationManager();
			deniedPartyScreeningPresentationManager.CreateMenusForJob(this);
			new DeniedPartyScreeningActionsProvider(this, Shipment).AddJobsMenuItem();
		}

		IEnumerable<MenuItem> GetActionMenuItems()
		{
			yield return new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.RegenearateHouseBill", "Regenerate House Bill Number"), delegate
			{ RegenerateHouseBill(true); });

			if (FreightConfigurationRegistry.Instance.EnableOverpacksAndSealsProjectFeatures.Value && FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.Value)
			{
				yield return new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.GeneratePackagesWithIDs", "Generate Packages with IDs"), GeneratePackagesWithIDs_Click);
			}

			yield return new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.DeliveryOrderHandedOver", "Delivery Order Handed Over"), DeliveryOrderHandedOver_Click);

			if (ZFormMenuStrategy.HasInterfaceConnector)
			{
				yield return CreateExportToVerboseXml();
				yield return CreateExportToLightWeightXml();
			}

			if (CanStoreAsALPO())
			{
				yield return new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.ExportVerboseXml.StoreALPO", "Store as ALPO"), ExportToXmlMenuItem.VerboseHandler(StoreAsALPOMenuClick));
			}

			yield return new ZMenuItem(Constants.MenuNameConstants.ResetValuesFromSubShipments, delegate
			{ Shipment.ResetAllValuesFromSubs(); });

			yield return new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.CalculateInspectionStatus", "Calculate Inspection Status"), delegate
			{ SetApprovedShipperStatus(); });

			if (FreightRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(Shipment?.JS_TransportMode ?? ZString.Empty))
			{
				yield return new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.CalculateDeliveryDueDate", "Calculate Delivery Due Date"), CalculateDeliveryDueDate_Click);
			}

			yield return new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.CopyHarmonizedDetails", "Copy harmonized details from Booking to Declaration Commercial Invoice"), new EventHandler(OnCopyHarmonisedDetails_Click));
			yield return new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.RecalculateRelatedPartiesForThisCompany", "Recalculate Related Parties for logged in Company"), new EventHandler(OnRecalculateRelatedParties_Click));
			yield return new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.SelectionOfRateCommodity", "Selection of Rate Commodity (with FMC Tariff ID)"), new EventHandler(OnRateCommoditySelection_Click));

			var createTestHVLVDataMenuItem = ObjectFactory.Get<IHVLVTestDataProvider>(nameof(IHVLVTestDataProvider)).GetCreateTestHVLShipmentMenuItemIfNecessary(DataSource as IBusiness);
			if (createTestHVLVDataMenuItem != null && createTestHVLVDataMenuItem is ZMenuItem menuItem)
			{
				yield return menuItem;
			}
		}

		MenuItem CreateExportToVerboseXml()
		{
			var result = new ZMenuItem(ExportXmlMenuItemHelper.VerboseMenuItemText);
			result.Name = ExportXmlMenuItemHelper.VerboseMenuItemName;
			result.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.ExportVerboseXml.StoreShipmentXML", "Store as Shipment XML file"), ExportToXmlMenuItem.VerboseHandler(StoreAsShipmentMenuClick)));
			result.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.ExportVerboseXml.StoreDeclarationXML", "Store as Declaration XML file"), ExportToXmlMenuItem.VerboseHandler(StoreAsDeclarationMenuClick)));
			return result;
		}

		MenuItem CreateExportToLightWeightXml()
		{
			var result = new ZMenuItem(ExportXmlMenuItemHelper.LightWeightMenuItemText);
			result.Name = ExportXmlMenuItemHelper.LightWeightMenuItemName;
			result.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.ExportLightWeightXml.StoreShipmentXML", "Store as Shipment XML file"), ExportToXmlMenuItem.LightWeighHandler(StoreAsShipmentMenuClick)));
			result.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.ExportLightWeightXml.StoreDeclarationXML", "Store as Declaration XML file"), ExportToXmlMenuItem.LightWeighHandler(StoreAsDeclarationMenuClick)));
			return result;
		}

		bool CanStoreAsALPO()
		{
			if (!canStoreAsALPO.HasValue)
			{
				canStoreAsALPO = ALPOHelper.IsLegacyALPOInterfaceAvailable() && (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Germany
					|| ALPOHelper.GetShipmentTransport(Shipment) != null
					|| ALPOHelper.GetConsol(Shipment) != null);
			}

			return canStoreAsALPO.Value;
		}

		void DisableResetValuesFromSubShipmentsMenuItemWhenHVLShipmentType()
		{
			var resetValuesFromSubShipmentsMenuItem = ActionsMenuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(item => item.Text == Constants.MenuNameConstants.ResetValuesFromSubShipments);
			if (resetValuesFromSubShipmentsMenuItem == null)
			{
				return;
			}

			var isHighVolumeLowValue = Shipment.IsHighVolumeLowValue;
			var isMenuItemVisible = resetValuesFromSubShipmentsMenuItem.Visible;

			if (isHighVolumeLowValue && isMenuItemVisible)
			{
				resetValuesFromSubShipmentsMenuItem.Visible = false;
			}
			else if (!isHighVolumeLowValue && !isMenuItemVisible)
			{
				resetValuesFromSubShipmentsMenuItem.Visible = true;
			}
		}

		void OrderActionMenu()
		{
			if (!listOrdered)
			{
				var nativeItem = ActionsMenuItem.MenuItems[ExportXmlMenuItemHelper.NativeExportMenuItemName];
				var lightWeight = ActionsMenuItem.MenuItems[ExportXmlMenuItemHelper.LightWeightMenuItemName];
				if (nativeItem != null && lightWeight != null)
				{
					var lightWeightIndex = lightWeight.Index;
					var correctNativeIndex = lightWeightIndex + 1;

					var nativeExportIndex = nativeItem.Index;

					if (nativeExportIndex != correctNativeIndex)
					{
						ActionsMenuItem.MenuItems.RemoveAt(nativeExportIndex);
						ActionsMenuItem.MenuItems.Add(correctNativeIndex, nativeItem);
					}
				}
				listOrdered = true;
			}
		}

		internal bool listOrdered;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				new TabConfigurationManager(MainMenu, MainTabControl).Enabled = true;
			}

			if (FormVerb == FormVerbs.Deactivate)
			{
				((IRequireInactivationPrompt)this).PromptForInactivation();
			}

			var details = ShipmentUserControl.Details.HostedControl as DetailsEntryControl;
			details?.RefreshLayout();
			details?.HookLayout();

			using (Shipment.SuspendSettingHasChanges())
			{
				if (Shipment is not IComplianceItemRiskStatusProvider provider || !provider.IsEnabledComplianceWise)
				{
					new DeniedPartyScreeningPresentationManager().ResynchronizeScreeningStatus(false, new[] { Shipment }, null);
				}
			}

			SetupViewShipmentTrackingButton();
		}

		MenuItem electronicMessagingMenuItem;
		bool? canStoreAsALPO;

		#endregion

		#region PlugIns

		void InitialisePlugIns()
		{
			SetupPickupDelivery();

			if (Shipment != null && Shipment.IsTemplate)
			{
				PlugIns.Add(ControllerIDs.DocAddresses);

				return;
			}

			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled && !Shipment.IsTemplate)
			{
				PlugIns.Add(ControllerIDs.CO2ePlugin);
			}

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, 2);
			PlugIns.Add(ControllerIDs.Customs.SG.CMDShipment);

			PlugIns.Add(ControllerIDs.Customs.AU.HouseAirCargo);

			PlugIns.Add(ControllerIDs.Customs.AU.SeaCargo);
			PlugIns.Add(ControllerIDs.Customs.AU.SeaCargoDepot);

			PlugIns.Add(ControllerIDs.Customs.CA.CAShipmentCargoReport);
			PlugIns.Add(ControllerIDs.Customs.CA.RNSShipmentPlugIn);

			PlugIns.Add(ControllerIDs.Customs.NZ.ShipmentToExpressECIConverter);
			PlugIns.Add(ControllerIDs.Customs.NZ.ShipmentToSeaCargoWriteOffConverter);

			if (ComplianceRiskHelper.IsGlobalCommercialInvoiceEnabled)
			{
				PlugIns.Add(ControllerIDs.GlobalCommercialInvoicePlugin);
			}

			PlugIns.Add(ControllerIDs.Customs.JobDeclaration);
			PlugIns.Add(ControllerIDs.Customs.US.InBond);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.GB.CcsukAirInventoryHouse, GetRequestedTabPageIndexBeforeBrokerageTabPage);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.EU.NctsMovementController, GetRequestedTabPageIndexAfterBrokerageTabPage);

			PlugIns.AddJobInvoicing(Shipment.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocAddresses);

			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.DocumentVisualizer);

			PlugIns.Add(ControllerIDs.CartagePlugin);
			PlugIns.Add(ControllerIDs.DtbBooking);

			PlugIns.Add(ControllerIDs.TransitWarehouseAttachPackages);

			ElectronicMessagingTabControl.PlugIns.Add(ControllerIDs.ElectronicBOL);
			Shipment.JS_ReleaseTypeInfo.ValueChanged += EnableElectronicBOL;
			Shipment.JS_HouseBillInfo.ValueChanged += EnableElectronicBOL;
			Shipment.JS_TransportModeInfo.ValueChanged += EnableElectronicBOL;
			ElectronicMessagingTabControl.PlugIns.Add(ControllerIDs.CargoIMPPhase2);
			ElectronicMessagingTabControl.PlugIns.Add(ControllerIDs.PortMessaging);
			ElectronicMessagingTabControl.PlugIns.Add(ControllerIDs.ExportConsignmentReleaseAdvice);
			ElectronicMessagingTabControl.PlugIns.Add(ControllerIDs.Customs.IL.CustomsMessaging);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.ETailShipment, 3);

			PlugIns.Add(ControllerIDs.Customs.EU.ExitSummaryController);

			if (GlowRegistry.Instance.NeoEnableConversations.Value)
			{
				PlugIns.Add(ControllerIDs.eConversationPlugIn);
			}

			if (ComplianceRiskHelper.IsFreightEnabledComplianceWise)
			{
				PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
			}

			if (FreightConfigurationRegistry.Instance.EnableDangerousGoodsPortal.Value)
			{
				PlugIns.Add(ControllerIDs.DangerousGoodsPlugin);
			}
		}

		void EnableElectronicBOL(object sender, EventArgs e)
		{
			ElectronicMessagingTabControl.PlugIns.GetPlugIn(ControllerIDs.ElectronicBOL).Enabled = Shipment.EnabledElectronicBOL;
		}

		int GetRequestedTabPageIndexBeforeBrokerageTabPage()
		{
			var tabPage = TopLevelTabControl.GetTabPage("BrokerageTabPage");
			if (tabPage != null)
			{
				var index = TopLevelTabControl.TabPages.IndexOf(tabPage);
				if (index != -1)
				{
					return index;
				}
			}
			return -1;
		}

		int GetRequestedTabPageIndexAfterBrokerageTabPage()
		{
			var index = GetRequestedTabPageIndexBeforeBrokerageTabPage();
			return index == -1 ? index : index + 1;
		}

		protected override Menu GetMenuForPlugInCore(ControllerID controllerID)
		{
			if (controllerID == ControllerIDs.CargoIMPPhase2
				|| controllerID == ControllerIDs.PortMessaging
				|| controllerID == ControllerIDs.Customs.IL.CustomsMessaging)
			{
				return electronicMessagingMenuItem;
			}

			return base.GetMenuForPlugInCore(controllerID);
		}

		void SetupPickupDelivery()
		{
			PickupTabPage.BindingOrFirstShown += (o, e) =>
			{
				var pickupDetailsTabPage = new ZTabPage
				{
					Name = "PickupDetailsTabPage",
					Text = Res.GetString("Shipment|PickupDetails", "Details")
				};

				var pickupTabControl = new ZTabControl
				{
					Name = "PickupTabControl",
					Dock = DockStyle.Fill
				};

				var shipmentPickupDetailsControl = new ShipmentPickupDetailsControl(Shipment)
				{
					Dock = DockStyle.Fill
				};

				pickupDetailsTabPage.Controls.Add(shipmentPickupDetailsControl);
				pickupTabControl.TabPages.Add(pickupDetailsTabPage);
				PickupTabPage.Controls.Add(pickupTabControl);

				pickupTabControl.PlugIns.Add(ControllerIDs.ConfirmationsPlugin);
				IConfirmationPlugin pickupConfirmaitonPlugin = (IConfirmationPlugin)pickupTabControl.PlugIns.GetPlugIn(ControllerIDs.ConfirmationsPlugin);
				pickupConfirmaitonPlugin.SetStrategy(ConfirmationType.OriginPickup, Res.GetString("Shipment|PickupConfirms", "Confirmations"));

				pickupTabControl.Selected += new TabControlEventHandler(PickupTabControl_Selected);
			};

			DeliveryTabPage.BindingOrFirstShown += (o, e) =>
			{
				var deliveryTabControl = new ZTabControl
				{
					Name = "DeliveryTabControl",
					Dock = DockStyle.Fill
				};

				var deliveryDetailsTabPage = new ZTabPage
				{
					Name = "DeliveryDetailsTabPage",
					Text = Res.GetString("Shipment|DeliveryDetails", "Details")
				};

				var shipmentDeliveryDetailsControl = new ShipmentDeliveryDetailsControl(Shipment)
				{
					Dock = DockStyle.Fill
				};

				deliveryDetailsTabPage.Controls.Add(shipmentDeliveryDetailsControl);
				deliveryTabControl.TabPages.Add(deliveryDetailsTabPage);
				DeliveryTabPage.Controls.Add(deliveryTabControl);

				deliveryTabControl.PlugIns.Add(ControllerIDs.ConfirmationsPlugin);
				IConfirmationPlugin deliveryConfirmationPlugin = (IConfirmationPlugin)deliveryTabControl.PlugIns.GetPlugIn(ControllerIDs.ConfirmationsPlugin);
				deliveryConfirmationPlugin.SetStrategy(ConfirmationType.DestinationDelivery, Res.GetString("Shipment|DeliveryConfirms", "Confirmations"));

				deliveryTabControl.Selected += new TabControlEventHandler(DeliveryTabControl_Selected);
			};
		}

		void PickupTabControl_Selected(object sender, TabControlEventArgs e)
		{
			PickupDeliveryTabControl_Selected(sender, e, ConfirmTimesSyncHelper.ConfirmType.Pickup);
		}

		void DeliveryTabControl_Selected(object sender, TabControlEventArgs e)
		{
			PickupDeliveryTabControl_Selected(sender, e, ConfirmTimesSyncHelper.ConfirmType.Delivery);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters")]
		void PickupDeliveryTabControl_Selected(object sender, TabControlEventArgs e, ConfirmTimesSyncHelper.ConfirmType confirmType)
		{
			if (e.TabPage.Name == "ConfirmationsPluginTabPage"
				&& Env.Security.PickupDeliveryConfirmationsNew.IsAllowed)
			{
				if (ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(Shipment, confirmType))
				{
					string caption = Res.GetString("304192db-f879-458d-8165-ba91cf1b2cf7", "Create Loose Confirmations");
					string message = Res.GetString("0117442c-2d5b-4385-8b66-40fc9aa3d216",
						"The Confirmation was not previously created, would you like to create it now?");

					if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						ConfirmTimesSyncHelper.CreateLooseConfirmIfNeeded(Shipment, confirmType);
					}
				}

				ShowShipmentConfirmationsObsoleteWarning();
			}
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		#endregion

		#region Saving

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			if (Shipment == null)
			{
				throw new InvalidOperationException("The property \"Shipment\" happens to be null.");
			}
			if (Shipment.Factory == null)
			{
				throw new InvalidOperationException("The property \"Shipment.Factory\" happens to be null.");
			}
			ForwardingConsolManyToManyCollection shipmentConsols = Shipment.Consols ?? throw new InvalidOperationException("The property \"Shipment.Consols\" happens to be null.");
			// Shipment.Validation can never be null. See its implementation.
			Shipment.Validation.ValidateJS_HouseBill();
			if (Shipment.Validation.HasDuplicateHouseBillWarning)
			{
				// Shipment.JS_HouseBillInfo and Shipment.JS_HouseBillInfo.GetWarnings() can never be null.
				var message = Shipment.JS_HouseBillInfo.GetWarnings().ToUniqueMessageListString();
				if (ShowConfirmationForDuplicateHouseBill(message) == DialogResult.No)
				{
					return ContinueWithSave.No;
				}
			}

			if (Shipment.HasBeenUnallocatedSinceRetrieve)
			{
				if (ShowConfirmationForUnallocatingShipment() == DialogResult.No)
				{
					return ContinueWithSave.No;
				}
			}

			if (Shipment.BuyerSupplierLinksHelper?.ShouldPromptToSaveSupplierBuyerRelationship ?? false)
			{
				if (ShowConfirmationForNewSupplierBuyerRelationship() == DialogResult.Yes)
				{
					Shipment.BuyerSupplierLinksHelper?.AddNewBuyerSupplierLink();
				}
			}

			if (Shipment.ShouldRegenerateHouseBillNumber && PopulateHouseBillAfterServiceLevelChanged() == DialogResult.Yes)
			{
				RegenerateHouseBill();
			}

			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				IEnumerable<ForwardingConsol> consolsWithPreAllocationExceeded = shipmentConsols.Cast<ForwardingConsol>().Where(consol => consol.IsPreAllocationExceededAndRestricted);
				if (consolsWithPreAllocationExceeded.Any())
				{
					result = AllocationAdjustmentDialog.ConfirmAllocationAdjustments(consolsWithPreAllocationExceeded) ? ContinueWithSave.Yes : ContinueWithSave.No;
				}
			}

			if (result == ContinueWithSave.Yes &&
				(Shipment.Validation.ConsigneeHasBeenChanged()
				|| Shipment.Validation.ConsignorHasBeenChanged()
				|| ReceivingAgentHasBeenChanged
				|| SendingAgentHasBeenChanged))
			{
				result = NonMatchingAgentsDialog.CheckAndConfirm(shipmentConsols.Cast<ForwardingConsol>(), new[] { Shipment })
							? ContinueWithSave.Yes
							: ContinueWithSave.No;
			}

			if (result == ContinueWithSave.Yes && shipmentConsols.Cast<ForwardingConsol>().Any(c => c.HasUpdatedAssemblyMasterAsDirectMaster()) && !hasUserConfirmedAssemblyMasterAsDirectMaster)
			{
				var assemblyMasterAcknowledgementHelper = new Common.AssemblyMasterAcknowledgementHelper();
				var (dialogResult, keyValuePairsForLogging) = assemblyMasterAcknowledgementHelper.ShowDialog(() => GlbStaff.CurrentUser.GS_Code);
				result = dialogResult == DialogResult.OK ? ContinueWithSave.Yes : ContinueWithSave.No;
				if (result == ContinueWithSave.Yes)
				{
					hasUserConfirmedAssemblyMasterAsDirectMaster = true;
					Shipment.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.Acknowledged, ZDateTimeOffset.Now, keyValuePairsForLogging.ToArray());
				}
			}

			if (result == ContinueWithSave.Yes)
			{
				result = RequestControllingPartySecurity();
			}

			return result;
		}

		bool ReceivingAgentHasBeenChanged => Shipment.Consols.Cast<CommonConsol>().Any(consol => consol.Validation.ReceivingForwarderHasBeenChanged());

		bool SendingAgentHasBeenChanged => Shipment.Consols.Cast<CommonConsol>().Any(consol => consol.Validation.SendingForwarderHasBeenChanged());

		ContinueWithSave RequestControllingPartySecurity()
		{
			ControllingPartyAuthorizationResult result = ControllingPartySecurityHelper.RequestSaveWithEmptyControllingPartyAuthorization(Shipment, DocAddressType.ControllingAgent, Shipment.GetControllingAgentSecurityCheckPoint());

			if (result.ContinueWithSave == ContinueWithSave.Yes)
			{
				result = ControllingPartySecurityHelper.RequestSaveWithEmptyControllingPartyAuthorization(Shipment, DocAddressType.ControllingCustomer, Shipment.GetControllingCustomerSecurityCheckPoint());
			}

			return result.ContinueWithSave;
		}

		bool hasUserConfirmedAssemblyMasterAsDirectMaster;

		DialogResult PopulateHouseBillAfterServiceLevelChanged()
		{
			return Globals.Message.Show(Res.GetString("9ca19c2d-7b46-49b2-ac2a-99b42b862fc4", "Service Level has changed from '{0}' to '{1}'. Do you wish to regenerate the House Bill Number?", Shipment.JS_RS_NKServiceLevelInfo.OriginalValue, Shipment.JS_RS_NKServiceLevel), Res.GetString("489d9bc5-162f-4beb-bb91-127c11bffec9", "House Bill Number Generator"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No);
		}

		DialogResult ShowConfirmationForDuplicateHouseBill(string message)
		{
			return Globals.Message.Show(Res.GetString("db91f4b6-6289-407e-95dc-d84280f03b08", "{0}\r\nDo you wish to continue?", message), Res.GetString("9e935e4b-c038-41a8-9ffc-606a004b59a4", "House Bill Duplicate"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
		}

		DialogResult ShowConfirmationForUnallocatingShipment()
		{
			return Globals.Message.Show(Res.GetString("25c7db02-c9b8-4b30-98c8-d7ac56982be0", "This Shipment is no longer on a Consol and will be unallocated after you save. Do you wish to continue?"), Res.GetString("47ffd927-e13c-4964-aac0-16913e6253d4", "Shipment with no Consol"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);
		}

		DialogResult ShowConfirmationForNewSupplierBuyerRelationship()
		{
			return Globals.Message.Show(Res.GetString("e464241e-9e9b-4193-8d97-260836dbb4e1", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?"), Res.GetString("65c8c6b6-6897-4796-b2b0-a71f6925d871", "Save"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
		}

		void ShowShipmentConfirmationsObsoleteWarning()
		{
			if (!Globals.IsTest)
			{
				if (!IsShipmentConfirmationsObsoleteWarningPrompted)
				{
					var message = Res.GetString("8beaa3aa-a6a2-4366-88b0-be7c51a227ba", @"This feature has been superseded by the Transport Bookings module and will become obsolete in the future system releases.
The information recorded under the Confirmations tab will become read only and retained for historical purposes.

For more information on Transport Bookings click on the link below or press OK to continue.");

					var messageLink = "https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#item=A338779E-F019-4C36-8A01-E9FAD8385062&video=11582944,5a806e6b150b0b065b32750bd332e6b4";

					using (var messageBox = new ZMessageBoxWithCheckbox(message, (NoResString)"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, messageLink, null))
					{
						messageBox.SetDontAskMeAgainCheckBoxVisibility(false);
						messageBox.ShowDialog();
					}
				}
			}
			IsShipmentConfirmationsObsoleteWarningPrompted = true;
		}

#if DEBUG
		internal
#endif
		bool IsShipmentConfirmationsObsoleteWarningPrompted
		{ set; get; }

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = ContinueWithSave.No;

			if (Shipment.CoLoadMasterShipment != null)
			{
				Shipment.CoLoadMasterShipment.IsMasterInSubShipmentContext = true;
			}

			Shipment.CheckTotalsDiffer();

			if (!Env.Registry.AllowManualShipmentEntry ||
				!Shipment.JS_UniqueConsignRef.IsEmpty ||
				!Shipment.RequiresShipmentEntryNumberSelection ||
				ShowShipmentNumberEntryForm())
			{
				if (Shipment != null)
				{
					CreateJobHeadersForNewSubShipmentsIfAllowed();
					result = base.ValidateAndSave();
				}
			}

			this.NotesTabPage.UpdateNoteImageOnRelatedChanges();

			return result;
		}

#if DEBUG
		public void TestValidateAndSave() => base.ValidateAndSave();
#endif

		protected virtual bool ShowShipmentNumberEntryForm()
		{
			return ShipmentNumberEntryForm.ShowForm(Shipment);
		}

		void CreateJobHeadersForNewSubShipmentsIfAllowed()
		{
			foreach (var shipment in Shipment.CoLoadShipments
					.OfType<ForwardingShipment>()
					.Where(s => !s.IsInDatabase && s.JobHeader == null))
			{
				var handler = new LocalClientJobHandler(shipment);
				handler.Initialize();
			}
		}

		#endregion

		#region Validation

		protected override void PerformValidation()
		{
			Shipment.MarkAsNeedingValidationWhenControllingCustomerOrAgentRequireDefaulting();

			Shipment.OuterPackLines.MarkAsNeedingValidation();
			Shipment.InnerPackLines.MarkAsNeedingValidation();

			base.PerformValidation();
		}

		#endregion

		public void ActivateRelatedShipmentsTabPage()
		{
			MainTabControl.SelectedTab = RelatedShipmentsTabPage;
		}

		#region Form Setup

		public override string FormCaption
		{
			get
			{
				var nameWithoutId = Shipment != null ? (string)Shipment.HumanReadableNameWithoutID : Res.GetString("Forwarding|ShipmentForm|FromCaptionPrefix", "Shipment");
				return nameWithoutId + (Shipment != null ? " " + Shipment.JS_UniqueConsignRef : string.Empty);
			}
		}

		protected internal ShipmentBasicRegistrationControl ShipmentUserControl
		{
			get
			{
				if (fShipmentUserControl == null)
				{
					fShipmentUserControl = GetShipmentUserControl();
					fShipmentUserControl.Dock = DockStyle.Fill;
					fShipmentUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
					fShipmentUserControl.Name = "ShipmentBasicRegistrationControl";
					fShipmentUserControl.TabIndex = 0;
					fShipmentUserControl.Visible = false;
				}
				return fShipmentUserControl;
			}
		}

		protected virtual ShipmentBasicRegistrationControl GetShipmentUserControl()
		{
			return new ShipmentBasicRegistrationControl();
		}

		void SetupForm()
		{
			DataContext = Constants.DataContext.Shipment;

			ShipmentUserControl.DockInside(ShipmentDetailsTabPage);

			ContainerDetailsTabPage.RunWhenBindingOrFirstShown(delegate
			{
				ContainersUserControl.DockInside(ContainerDetailsTabPage);
			});

			BookingDetailsTabPage.TabVisible = Shipment?.IsStandAloneShipmentFromBooking ?? false;
			this.Saved += ShipmentForm_Saved;

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			ForwardingShipmentDocumentSupporterGuiQueryProvider.Register(Shipment.Factory);
			ServicesSelectionGuiProvider.Register(Shipment.Factory);

			if (Shipment.IsTakingPartInElectronicMessagingExchangeBetweenNVOCCAndForwarder())
			{
				Shipment.SetBookingPartyDocumentaryAddressReadonly(true);
			}

			if (Shipment != null && Shipment.IsTemplate)
			{
				ElectronicMessagingTabPage?.Dispose();
				ElectronicMessagingTabPage = null;
				RelatedShipmentsTabPage?.Dispose();
				RelatedShipmentsTabPage = null;

				var labelTemplateRecord = new TemplateRecordLabelControl(Shipment.TemplateRecord);
				Controls.Add(labelTemplateRecord);

				labelTemplateRecord.SendToBack();
			}

			AddScreeningLogsTabPage();

			AddComplianceRiskMessageBannerIfNeeded();
		}

		void ShipmentForm_Saved(object sender, EventArgs e)
		{
			BookingDetailsTabPage.TabVisible = Shipment?.IsStandAloneShipmentFromBooking ?? false;
			if (Shipment?.RequireSendingPrepareDispatchTWInstructionForBlindPackages ?? false)
			{
				if (Shipment.IsLinkedDCNSplitted)
				{
					Globals.Message.ShowError(Res.GetString("7298b689-b0bf-4d3e-b363-7e5e3cce9492",
						"Failed to send Dispatch Instruction as blind packages cannot be attached or detached when the Shipment is already linked to a split DCN."));
					return;
				}

				var pickupWarehouseDCN = Shipment.PickupWarehouseDispatchConsignments.Cast<BusinessObject>().FirstOrDefault();
				var deliveryWarehouseDCN = Shipment.DeliveryWarehouseDispatchConsignments.Cast<BusinessObject>().FirstOrDefault();
				Shipment.PreviousShipmentsByAttachedPackages.ForEach(s => SendTransitWarehouseInstruction(Shipment.PrepareDispatchTWInstructionForBlindPackagesDirection,
					TransitWarehouseInstructionHelper.ServiceRequest.PrepareDispatch, true, s));

				if ((Shipment.PrepareDispatchTWInstructionForBlindPackagesDirection == TransitWarehouseInstructionHelper.Direction.Pickup && pickupWarehouseDCN != null && (ZBool)pickupWarehouseDCN[WhsItemDispatchConsignmentSchema.WDC_IsAuthorizedForDispatch])
					|| (Shipment.PrepareDispatchTWInstructionForBlindPackagesDirection == TransitWarehouseInstructionHelper.Direction.Delivery && deliveryWarehouseDCN != null && (ZBool)deliveryWarehouseDCN[WhsItemDispatchConsignmentSchema.WDC_IsAuthorizedForDispatch])
					|| (Shipment.PrepareDispatchTWInstructionForBlindPackagesDirection == TransitWarehouseInstructionHelper.Direction.Both && pickupWarehouseDCN != null && (ZBool)pickupWarehouseDCN[WhsItemDispatchConsignmentSchema.WDC_IsAuthorizedForDispatch] && deliveryWarehouseDCN != null && (ZBool)deliveryWarehouseDCN[WhsItemDispatchConsignmentSchema.WDC_IsAuthorizedForDispatch]))
				{
					Globals.Message.ShowInformation(Res.GetString("143f9d75-27b5-45b3-b901-1b6a02850b3d",
						"You have modified Blind packages attached to this shipment. A Dispatch Instruction will be sent to Transit Warehouse."));
					SendTransitWarehouseInstruction(Shipment.PrepareDispatchTWInstructionForBlindPackagesDirection,
						TransitWarehouseInstructionHelper.ServiceRequest.Dispatch, true);
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("5881ffe1-8744-4bb8-8258-8fcba2c4191d",
						"You have modified Blind packages attached to this shipment. A Prepare Dispatch Instruction will be sent to Transit Warehouse."));
					SendTransitWarehouseInstruction(Shipment.PrepareDispatchTWInstructionForBlindPackagesDirection,
						TransitWarehouseInstructionHelper.ServiceRequest.PrepareDispatch, true);
				}
				Shipment.RequireSendingPrepareDispatchTWInstructionForBlindPackages = false;
				Shipment.PreviousShipmentsByAttachedPackages = new List<ForwardingShipment>();
			}
		}

		void SetupViewShipmentTrackingButton()
		{
			if (Shipment != null && FreightRegistry.Instance.GlobalTrackingShipmentVisibility.Value.IsActive)
			{
				ViewShipmentTrackingButton.Visible = true;

				PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
				PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(ViewShipmentTrackingButton);

				// Resize the panel to fit the button
				PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 29, true);

				foreach (Control control in PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Parent.Controls)
				{
					if (control is ZPreviousNextControl)
					{
						var originalLocation = PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location;
						var newLocation = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);

						PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = control.Visible ? originalLocation : newLocation;
					
						control.VisibleChanged += (s, e) =>
						{
							// Adjust the location of the button when the ZPreviousNextControl's visibility changes
							PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = ((ZPreviousNextControl)s).Visible ? originalLocation : newLocation;
						};
					}
				}
			}
			else
			{
				ViewShipmentTrackingButton.Visible = false;
			}
		}

		void ViewShipmentTrackingButton_Click(object sender, EventArgs e)
		{
			ViewShipmentTrackingHelper.LaunchURL(Shipment);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Shipment != null)
			{
				Shipment.OnlyShipmentInConsol -= new CommonShipment.OnlyShipmentInConsolEventHandler(Shipment_OnlyShipmentInConsol);
				Shipment.OnOverrideWaybillDefaultsChanging -= new CancelEventHandler(ShipmentForm_OnOverrideWaybillDefaultsChanging);
				Shipment.UpdateShipmentTotalsPackQuantityVariation -= new CancelEventHandler(Shipment_CheckUpdateShipmentTotals);
				Shipment.ReDefaultPackLineInspectionTypeCodesEventHandler -= new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineInspectionTypeCodesFromShipment);
				Shipment.ReDefaultPackLineAdditionalInspectionTypeCodesEventHandler -= new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineAdditionalInspectionTypeCodesFromShipment);
				Shipment.ReDefaultPackLineIsHighRiskEventHandler -= new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineIsHighRiskFromShipment);
				Shipment.UpdateSubHVLShipmentsInspectionTypeEventHandler -= new CancelEventHandler(MessagePopupHelper.CheckUpdateSubHVLShipmentInspectionTypeFromShipment);
				Shipment.ShipmentTypeChanging -= Shipment_TypeChanging;
				Shipment.OnShipmentBookingStatusUpdate -= OnShipmentBookingStatusUpdate;
				Shipment.ContainerAdded -= OnPackLineAddedToContainer;
				Shipment.Consols.CountChanged -= OnConsolChange;

				if (ShipmentLinkingMessagesSupporter != null)
				{
					ShipmentLinkingMessagesSupporter.UnHookShipment();
				}
			}

			base.SetDataBinding(dataSource, dataMember);

			if (Shipment != null)
			{
				Shipment.OnlyShipmentInConsol += new CommonShipment.OnlyShipmentInConsolEventHandler(Shipment_OnlyShipmentInConsol);
				Shipment.OnOverrideWaybillDefaultsChanging += new CancelEventHandler(ShipmentForm_OnOverrideWaybillDefaultsChanging);
				Shipment.UpdateShipmentTotalsPackQuantityVariation += new CancelEventHandler(Shipment_CheckUpdateShipmentTotals);
				Shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineInspectionTypeCodesFromShipment);
				Shipment.ReDefaultPackLineAdditionalInspectionTypeCodesEventHandler += new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineAdditionalInspectionTypeCodesFromShipment);
				Shipment.ReDefaultPackLineIsHighRiskEventHandler += new CancelEventHandler(MessagePopupHelper.CheckRedefaultPackLineIsHighRiskFromShipment);
				Shipment.UpdateSubHVLShipmentsInspectionTypeEventHandler += new CancelEventHandler(MessagePopupHelper.CheckUpdateSubHVLShipmentInspectionTypeFromShipment);
				Shipment.ShipmentTypeChanging += Shipment_TypeChanging;
				Shipment.OnShipmentBookingStatusUpdate += OnShipmentBookingStatusUpdate;
				Shipment.ContainerAdded += OnPackLineAddedToContainer;
				Shipment.Consols.CountChanged += OnConsolChange;

				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					ShipmentLinkingMessagesSupporter = ObjectFactory.Get<Enterprise.Integration.Customs.CA.IShipmentLinkingMessagesSupporterProvider>().Create(Shipment);
					ShipmentLinkingMessagesSupporter.HookShipment();
				}

				foreach (ForwardingConsol consol in Shipment.Consols)
				{
					consol.BookingAgentDefaultingAsker = new ConfirmationPrompt();
				}
			}
		}

		void OnConsolChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is ForwardingConsol consol)
			{
				if (e.ItemAdded)
				{
					consol.BookingAgentDefaultingAsker = new ConfirmationPrompt();
				}
				else
				{
					consol.BookingAgentDefaultingAsker = null;
				}
			}
		}

		void OnPackLineAddedToContainer(object sender, CommonShipment.ContainerAddedEventArgs e)
		{
			var container = e.Container;
			if (container != null && container.IsGrossWeightOverrideActive)
			{
				var consol = Shipment.Consols?.OfType<CommonConsol>().FirstOrDefault(cnsl => cnsl.Containers?.OfType<CommonContainer>().Any(cont => cont.PK == container.PK) ?? false);

				var shipmentAlreadyHasThisContainer = Shipment.OuterPackLines.OfType<PackLine>().Count(x => x.GetContainer(consol)?.PK == container.PK) > 1;

				bool hasOtherPacklines = consol != null && shipmentAlreadyHasThisContainer;

				if (!hasOtherPacklines)
				{
					var firstPackLineOfShipment = Shipment.OuterPackLines.OfType<PackLine>().FirstOrDefault(x => x.GetContainer(consol)?.PK == container.PK);

					container.AdjustOverriddenGrossWeightWithUserConfirmation(firstPackLineOfShipment);
				}
			}
		}

		Enterprise.Integration.Customs.CA.IShipmentLinkingMessagesSupporter ShipmentLinkingMessagesSupporter;

		void ShipmentForm_OnOverrideWaybillDefaultsChanging(object sender, CancelEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("95072b82-6510-4827-b82a-ec2e1536b9de", "Removing the override will reset your AWB data.\r\nYou will lose changes that you have made to the AWB data.\r\n\r\nProceed?"), Res.GetString("013f4a49-606e-4a34-920f-efc4ad092406", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

		void ReportBigNumberOfFactories()
		{
			var registrySetting = SystemDataRegistry.Instance.NumberOfFactoriesNeededForWarningReport.Value;
			if (registrySetting > 0 && !PersistentFactoryCacheManager.Instance.UsingStrongReferences)
			{
				var sw = Stopwatch.StartNew();
				var openedForms = ZApplication.GetOpenForms();
				var shipmentFormsOpen = openedForms.Count(x => x.Name.Contains((NoResString)"Shipment"));
				var nbFactoriesCounted = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Length;
				var ratioDoesntLookRight = shipmentFormsOpen == 0 || nbFactoriesCounted / shipmentFormsOpen > 50;
				sw.Stop();
				if (nbFactoriesCounted >= registrySetting && ratioDoesntLookRight)
				{
					var message = string.Format(CultureInfo.InvariantCulture, (NoResString)@"This instance of {0} seems to have a worrying number of factories currently open. Please contact Core team (Alexander Korotun, Alex Chalumeau) immediately so they can log on to the customer rapidly and download a dump file.
Calculation : {1} seconds.
Number of Factories: {2}
Forms open at the time: {3}", BrandingFactory.Instance.ProductName, sw.Elapsed.TotalSeconds, nbFactoriesCounted, string.Join(System.Environment.NewLine, openedForms.Select(x => x.Name)));
					ErrorReporter.ReportOnce("ShipmentForm_WarningNumberFactories", message);
				}
			}
		}

		#endregion

		#region AutoRating

		void Shipment_OnlyShipmentInConsol(CommonShipment.OnlyShipmentInConsolEventArgs args)
		{
			var response = Globals.Message.Show(args.Message, Res.GetString("b407334c-5dbd-47e0-aa41-8caec8667f10", "AutoRating"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (response == DialogResult.No)
			{
				args.Cancel = true;
			}
		}

		#endregion

		#region Delivery Order Event

		void DeliveryOrderHandedOver_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(NewRaiseEventLogForm(ZArchitecture.Business.Events.DeliveryOrderHandedOver), this);
		}

		void GeneratePackagesWithIDs_Click(object sender, EventArgs e)
		{
			if (TransitWarehouseInstructionGUIHelper.CheckMatchingStatus(TransitWarehouseInstructionHelper.SupporterType.Shipment, Shipment.OuterPackLines.Cast<ForwardingPackLine>()))
			{
				using (new ZWaitCursorChanger(this))
				{
					new TransitWarehouseInstructionHelper(Shipment, this).GeneratePackagesWithIDs();
				}
			}
		}

#if DEBUG
		protected
#endif
		ZStmALogAddForm NewRaiseEventLogForm(Event @event)
		{
			var view = new StmALogCollectionView(Shipment);
			var form = new ZStmALogAddForm(view, Shipment.HasChanges);
			var newLog = (BaseStmALog)form.BusinessEntity;
			newLog.SL_SE_NKEvent = @event.Code;
			return form;
		}

		#endregion

		#region Export to XML

		#region ALPO

		void StoreAsALPOMenuClick(object sender, EventArgs e)
		{
			var message = Res.GetString("39d7a969-02f1-443c-8671-4dd3c3a227f4", @"You are attempting to run the legacy ALPO Interface, which will be decommissioned by 31 December 2025. To connect to the new ALPO Interface, please raise a CR9 eRequest with WiseTech and refer to the Update Note 'ALPO Port Order Integration'.");
			var messageLink = "https://wisetechacademy.com/search?quickstart=daf4190a-ea3b-4611-82ef-e506a36344b5";
			using (var messageBox = new ZMessageBoxWithCheckbox(message, (NoResString)"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, messageLink, null))
			{
				messageBox.SetDontAskMeAgainCheckBoxVisibility(false);
				ZFormModaliser.ShowDialogAndDispose(messageBox);
			}

			var exportor = new ALPOXMLDataTransferExporter(new ALPOConsolWithShipmentValueObjectDataAdapter(), true);
			exportor.PromptUserAndExport(new[] { (CommonShipment)BusinessEntity });
		}

		#endregion

#if DEBUG
		protected
#endif
		void StoreAsShipmentMenuClick(object sender, EventArgs e)
		{
			if (Env.Security.MaintainShipmentExportToXml.IsAllowed)
			{
				var exporter = GetNewXmlDataTransferExporter(new ForwardingShipmentValueObjectDataAdapter(), true);
				exporter.DefaultFileName = Shipment.JS_UniqueConsignRef + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
				if (!(new ZString(SystemDataRegistry.Instance.ShipmentExportDirectory.Value).IsEmpty))
				{
					exporter.InitialDirectory = SystemDataRegistry.Instance.ShipmentExportDirectory.Value;
				}
				exporter.PromptUserAndExport(new[] { (CommonShipment)BusinessEntity });
			}
			else
			{
				Env.Security.MaintainShipmentExportToXml.ShowError();
			}
		}

#if DEBUG
		protected virtual
#endif
		XmlDataTransferExporter GetNewXmlDataTransferExporter(IValueObjectDataAdapter adapter, bool checkForLicence)
		{
			return new XmlDataTransferExporter(adapter, checkForLicence);
		}

#if DEBUG
		protected
#endif
		void StoreAsDeclarationMenuClick(object sender, EventArgs e)
		{
			if (Env.Security.MaintainShipmentExportToXml.IsAllowed)
			{
				ShipmentWithConsolExporter.Export(Shipment);
			}
			else
			{
				Env.Security.MaintainShipmentExportToXml.ShowError();
			}
		}

#if DEBUG
		protected
#endif
		ShipmentWithConsolExporter ShipmentWithConsolExporter
		{
			get
			{
				if (fShipmentWithConsolExporter == null)
				{
					fShipmentWithConsolExporter = GetShipmentWithConsolExporter();
				}
				return fShipmentWithConsolExporter;
			}
		}
		ShipmentWithConsolExporter fShipmentWithConsolExporter;

#if DEBUG
		protected virtual
#endif
		ShipmentWithConsolExporter GetShipmentWithConsolExporter()
		{
			return new ShipmentWithConsolExporter();
		}

		#endregion

		#region Action Menu: Rate Commodity Selection

		void OnRateCommoditySelection_Click(object sender, EventArgs e)
		{
			if (Shipment.RateCommodity == null)
			{
				using (var box = new ZMessageBox(ResString.GetMultilingualString("9a31a657-309d-4625-987b-c594d02386fb", "Please make sure the Rate Commodity field is not empty and valid."), (NoResString)"Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Information))
				{
					ZFormModaliser.ShowDialogWithoutDispose(box);
				}
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var supporter = (IRatingSupporter)Shipment;
				var selector = ObjectFactory.Get<IRateCommodityFMCSelectorController>("IRateCommodityFMCSelectorController", this);

				selector.SelectAndUpdate(
					factory,
					supporter,
					new DetailedGoodsDescriptionProxy()
					{
						IsAvailable = true,
						IsEmpty = Shipment.DetailedGoodsDescriptionNoteText.IsEmpty
					});
			}
		}

		#endregion

		#region Action Menu: Copy Harmonised Details

		void OnCopyHarmonisedDetails_Click(object sender, EventArgs e)
		{
			var declaration = (Integration.Forwarding.ICommercialInvoice)Shipment.GetDeclaration();
			if (declaration == null)
			{
				Globals.Message.ShowError(Res.GetString("00a104ab-b0a1-4ece-8d13-438ea7f67709", "Please create a declaration before copying harmonized details"), Res.GetString("bfac5a75-26c0-401a-9a47-fc381dc4fb9d", "No declaration exists"));
			}
			else
			{
				declaration.PopulateCommercialInvoice(Shipment.OuterPackLines);
			}
		}

		#endregion

		#region Action menu:Recalculate Related Parties

		void OnRecalculateRelatedParties_Click(object sender, EventArgs e)
		{
			Shipment.AttemptToUpdateExistingValueInRecalculation += Shipment_AttemptToUpdateExistingValueInRecalculation;
			var result = Shipment.RecalculateRelatedParties();

			if (!result.WasSuccessful)
			{
				Globals.Message.ShowError(result.Log);
			}
			Shipment.AttemptToUpdateExistingValueInRecalculation -= Shipment_AttemptToUpdateExistingValueInRecalculation;
		}

		void Shipment_AttemptToUpdateExistingValueInRecalculation(object sender, ForwardingShipment.RecalculateRelatedPartyCancelEventArgs e)
		{
			var message = Res.GetString("671e5499-7302-4f4c-8dff-eb592c081abc",
				"{0} has already been entered. Do you wish to update the {0} based on your Company Related Party Configuration?", e.RecalculatedPropertyName);

			var context = new DialogDefaultContext(
				new ZGuid("39178d3e-e153-4cd1-bd28-acc2c02376d4"),
				ResString.GetMultilingualString("ec7f42c7-268a-4747-8521-cbe099226848", "Confirmation"),
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Question,
				null,
				showCheckboxOnly: true);
			var result = Globals.Message.ShowOrDefault(context, message);

			e.Cancel = result == ZDialogResult.No;
		}

		#endregion

		#region Product Warehouse

		void InitialiseProductWarehouseMenuItem(BusinessObject warehouseReceive)
		{
			if (!productWarehouseMenuItemInitialised)
			{
				var productWarehouseMenu = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.ProductWarehouse", "Product Warehouse"));
				ActionsMenuItem.MenuItems.Add(productWarehouseMenu);

				viewOrEditReceiveMenu = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.ProductWarehouse.ViewOrEditReceive", "View/Edit Warehouse Receive"), ShowFormReceiver);
				productWarehouseMenu.MenuItems.Add(viewOrEditReceiveMenu);

				createOrOverrideReceiveMenu = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.ProductWarehouse.CreateReceive", "Create Warehouse Receive"), OnSendProductWarehouseCreateWhsReceive);
				productWarehouseMenu.MenuItems.Add(createOrOverrideReceiveMenu);

				productWarehouseMenuItemInitialised = true;
			}

			var isReceiveAttached = warehouseReceive != null;
			var createOrOverrideMenuCaption = isReceiveAttached
				? ResString.GetMultilingualString("Forwarding.Shipment.Actions.ProductWarehouse.OverrideReceive", "Override Warehouse Receive")
				: ResString.GetMultilingualString("Forwarding.Shipment.Actions.ProductWarehouse.CreateReceive", "Create Warehouse Receive");

			viewOrEditReceiveMenu.Visible = isReceiveAttached;
			viewOrEditReceiveMenu.Enabled = isReceiveAttached;
			createOrOverrideReceiveMenu.Caption = createOrOverrideMenuCaption;
		}

		ZMenuItem viewOrEditReceiveMenu;
		ZMenuItem createOrOverrideReceiveMenu;

		bool productWarehouseMenuItemInitialised;

		#endregion

		#region Transit Warehouse

		void InitialiseTransitWarehouseMenuItem()
		{
			if (!transitWarehouseMenuItemInitialised)
			{
				var transitWarehouseMenu = ActionsMenuItem.MenuItems.FindByName(nameof(ControllerIDs.TransitWarehouseAttachPackages));
				if (transitWarehouseMenu == null)
				{
					transitWarehouseMenu = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse", "Transit Warehouse"));
					ActionsMenuItem.MenuItems.Add(transitWarehouseMenu);
				}

				var menuItems = transitWarehouseMenu.MenuItems;
				var pickupTW = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.PickupTW", "Pickup TW"));
				var deliveryTW = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.DeliveryTW", "Delivery TW"));

				menuItems.Add(pickupTW);
				menuItems.Add(deliveryTW);

				var pickupTWMenuItems = pickupTW.MenuItems;

				pickupTWMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendPickupTWReceiptInstruction", "Send Receipt Instruction"), OnSendTransitWarehousePickupReceiptInstruction));
				pickupTWMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendPickupTWDispatchInstruction", "Send Dispatch Instruction"), OnSendTransitWarehousePickupDispatchInstruction));
				pickupTWMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendPickupTWReceiptAndDispatchInstruction", "Send Receipt and Dispatch Instruction"), OnSendTransitWarehousePickupReceiptAndDispatchInstruction));
				pickupTWMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendPickupTWPrepareDispatchInstruction", "Send Prepare to Dispatch Instruction"), OnSendTransitWarehousePickupPrepareDispatchInstruction));
				pickupTWMenuItems.Add(ZMenuItem.Separator);

				var deliveryTWMenuItems = deliveryTW.MenuItems;
				deliveryTWMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendDeliveryTWReceiptInstruction", "Send Receipt Instruction"), OnSendTransitWarehouseDeliveryReceiptInstruction));
				deliveryTWMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendDeliveryTWDispatchInstruction", "Send Dispatch Instruction"), OnSendTransitWarehouseDeliveryDispatchInstruction));
				deliveryTWMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendDeliveryTWReceiptAndDispatchInstruction", "Send Receipt and Dispatch Instruction"), OnSendTransitWarehouseDeliveryReceiptAndDispatchInstruction));
				deliveryTWMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendPickupTWDeliveryDispatchInstruction", "Send Prepare to Dispatch Instruction"), OnSendTransitWarehouseDeliveryPrepareDispatchInstruction));
				deliveryTWMenuItems.Add(ZMenuItem.Separator);

				InitialiseTransitPlanningPortalMenuItems(pickupTW, deliveryTW);
				transitWarehouseMenuItemInitialised = true;
			}
		}

		bool transitWarehouseMenuItemInitialised;

		void InitialiseTransitPlanningPortalMenuItems(MenuItem pickupTWMenu, MenuItem deliveryTWMenu)
		{
			var pickupWarehouse = Shipment.PickupWarehouse;
			if (pickupWarehouse == null)
			{
				pickupTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("C258CD62-92D9-483B-B4D4-2D88C93971BB", "Receipt Instruction Planning Portal"), (o, e) => TransitWarehouseInstructionGUIHelper.ShowNoPickupTransitWarehouseErrorMessage()));
				pickupTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("818AA507-511C-4026-B296-A1839A3BC9F4", "Dispatch Instruction Planning Portal"), (o, e) => TransitWarehouseInstructionGUIHelper.ShowNoPickupTransitWarehouseErrorMessage()));
			}
			else
			{
				var pickupRcns = Shipment.PickupWarehouseReceiveConsignments;
				if (pickupRcns != null && pickupRcns.Length > 0)
				{
					foreach (var rcn in pickupRcns.Cast<BusinessObject>().OrderBy(x => x[WhsItemReceiveConsignmentSchema.WRC_JobID]))
					{
						pickupTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("A06DEF9E-1428-41C0-892D-0ADFF6C1B999", "Receipt Instruction Planning Portal - {0}", rcn[WhsItemReceiveConsignmentSchema.WRC_JobID]), (o, e) => TransitWarehouseInstructionGUIHelper.OpenReceiptPortal(rcn, Shipment, pickupWarehouse)));
					}
				}
				else
				{
					pickupTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("C258CD62-92D9-483B-B4D4-2D88C93971BB", "Receipt Instruction Planning Portal"), (o, e) => TransitWarehouseInstructionGUIHelper.ShowNoInstructionErrorMessage()));
				}
				var pickupDcns = Shipment.PickupWarehouseDispatchConsignments;
				if (pickupDcns != null && pickupDcns.Length > 0)
				{
					foreach (var dcn in pickupDcns.Cast<BusinessObject>().OrderBy(x => x[WhsItemDispatchConsignmentSchema.WDC_JobID]))
					{
						pickupTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("6021DF0F-2B52-497D-8555-1F53EFE0AA3D", "Dispatch Instruction Planning Portal - {0}", dcn[WhsItemDispatchConsignmentSchema.WDC_JobID]), (o, e) => TransitWarehouseInstructionGUIHelper.OpenDispatchPortal(dcn, Shipment, pickupWarehouse)));
					}
				}
				else
				{
					pickupTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("818AA507-511C-4026-B296-A1839A3BC9F4", "Dispatch Instruction Planning Portal"), (o, e) => TransitWarehouseInstructionGUIHelper.ShowNoInstructionErrorMessage()));
				}
			}
			var deliveryWarehouse = Shipment.DeliveryWarehouse;
			if (deliveryWarehouse == null)
			{
				deliveryTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("77963328-3958-42DC-8066-DE10BB715E1A", "Receipt Instruction Planning Portal"), (o, e) => TransitWarehouseInstructionGUIHelper.ShowNoDeliveryTransitWarehouseErrorMessage()));
				deliveryTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("BA7D310A-81B8-4925-8869-C8E17C601E69", "Dispatch Instruction Planning Portal"), (o, e) => TransitWarehouseInstructionGUIHelper.ShowNoDeliveryTransitWarehouseErrorMessage()));
			}
			else
			{
				var deliveryRcns = Shipment.DeliveryWarehouseReceiveConsignments;
				if (deliveryRcns != null && deliveryRcns.Length > 0)
				{
					foreach (var rcn in deliveryRcns.Cast<BusinessObject>().OrderBy(x => x[WhsItemReceiveConsignmentSchema.WRC_JobID]))
					{
						deliveryTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("B60CF213-2560-4729-BB14-3F23B16622A4", "Receipt Instruction Planning Portal - {0}", rcn[WhsItemReceiveConsignmentSchema.WRC_JobID]), (o, e) => TransitWarehouseInstructionGUIHelper.OpenReceiptPortal(rcn, Shipment, deliveryWarehouse)));
					}
				}
				else
				{
					deliveryTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("77963328-3958-42DC-8066-DE10BB715E1A", "Receipt Instruction Planning Portal"), (o, e) => TransitWarehouseInstructionGUIHelper.ShowNoInstructionErrorMessage()));
				}
				var deliveryDcns = Shipment.DeliveryWarehouseDispatchConsignments;
				if (deliveryDcns != null && deliveryDcns.Length > 0)
				{
					foreach (var dcn in deliveryDcns.Cast<BusinessObject>().OrderBy(x => x[WhsItemDispatchConsignmentSchema.WDC_JobID]))
					{
						deliveryTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("6ADAACE1-DDC1-4C50-8158-74D2D8141EAC", "Dispatch Instruction Planning Portal - {0}", dcn[WhsItemDispatchConsignmentSchema.WDC_JobID]), (o, e) => TransitWarehouseInstructionGUIHelper.OpenDispatchPortal(dcn, Shipment, deliveryWarehouse)));
					}
				}
				else
				{
					deliveryTWMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("BA7D310A-81B8-4925-8869-C8E17C601E69", "Dispatch Instruction Planning Portal"), (o, e) => TransitWarehouseInstructionGUIHelper.ShowNoInstructionErrorMessage()));
				}
			}
		}

		void ShowFormReceiver(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.WhsReceive);
			controller.ShowEditForm(Shipment.RelatedWarehouseReceive);
		}

		void OnSendProductWarehouseCreateWhsReceive(object sender, EventArgs e)
		{
			using (new ZWaitCursorChanger(this))
			{
				Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = (outboundSessionTracker) => new ShipmentDataObjectWriter(outboundSessionTracker, checkSubShipments: true, checkForParent: true, alwaysExportTB: true);
				new ProductWarehouseHelper(Shipment, this, dataWriterGetter)
					.CreateProductWarehouseReceive();
			}
		}

		void OnSendTransitWarehousePickupPrepareDispatchInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.PrepareDispatch);
		}

		void OnSendTransitWarehouseDeliveryPrepareDispatchInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Delivery, TransitWarehouseInstructionHelper.ServiceRequest.PrepareDispatch);
		}

		void OnSendTransitWarehousePickupReceiptInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);
		}

		void OnSendTransitWarehousePickupDispatchInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.Dispatch);
		}

		void OnSendTransitWarehousePickupReceiptAndDispatchInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Pickup, TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch);
		}

		void OnSendTransitWarehouseDeliveryReceiptInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Delivery, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);
		}

		void OnSendTransitWarehouseDeliveryDispatchInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Delivery, TransitWarehouseInstructionHelper.ServiceRequest.Dispatch);
		}

		void OnSendTransitWarehouseDeliveryReceiptAndDispatchInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction.Delivery, TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch);
		}

		void SendTransitWarehouseInstruction(TransitWarehouseInstructionHelper.Direction direction, TransitWarehouseInstructionHelper.ServiceRequest receiptDispatch, bool skipCheckStatus = false, ForwardingShipment shipment = null)
		{
			shipment = shipment ?? Shipment;
			if ((receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Dispatch || receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch
				|| receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.PrepareDispatch) && Shipment.IsLinkedDCNSplitted)
			{
				Globals.Message.ShowError(Res.GetString("af0d9781-b772-4b79-979d-f61d2f713b12", "Failed to send Dispatch Instruction - Transit Warehouse Dispatch Consignments have already been split."));
			}
			else if (skipCheckStatus || TransitWarehouseInstructionGUIHelper.CheckMatchingStatus(TransitWarehouseInstructionHelper.SupporterType.Shipment, shipment.OuterPackLines.Cast<ForwardingPackLine>()))
			{
				using (new ZWaitCursorChanger(this))
				{
					new TransitWarehouseInstructionHelper(shipment, this)
						.SendTransitWarehouseInstruction(direction, receiptDispatch);
				}
			}
		}

		#endregion

		#region Shipment_TypeChanging

		void Shipment_TypeChanging(CommonShipment sender, ShipmentTypeChangingCancelEventArgs e)
		{
			ShipmentTypeChangingHandler.Handle(sender, e);
		}

		#endregion

		#region Shipment Weight/Vol Totals

		void Shipment_CheckUpdateShipmentTotals(object sender, CancelEventArgs e)
		{
			if (Shipment.IsHighVolumeLowValue)
			{
				var updateMethod = HVLVDataRegistry.Instance.HVLVShipmentWeightUpdateMethod.Value;

				if (Core.Constants.ShipmentWeightUpdateOptions.IsAlwaysUpdate(updateMethod))
				{
					e.Cancel = false;
				}
				else if (updateMethod == Core.Constants.ShipmentWeightUpdateOptions.Code.DoNotUpdate)
				{
					e.Cancel = true;
				}
				else
				{
					var message = string.Empty;
					if (updateMethod == Core.Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromConsignmentsDetails)
					{
						message = Res.GetString("9B1229BD-3498-4AD4-BAB8-8BC2FA553B45", "Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the item totals?");
					}
					else if (updateMethod == Core.Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromPackingDetails)
					{
						if (Shipment.OuterPacksTotalsDifferAndCanBeUpdated && Shipment.InnerPacksTotalsDifferAndCanBeUpdated)
						{
							message = Res.GetString("f30c785f-d90d-45a4-b863-214ad658e58f", "Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the packing details and total items?");
						}
						else if (Shipment.OuterPacksTotalsDifferAndCanBeUpdated)
						{
							message = Res.GetString("b3cc9717-b962-435f-955a-c29c6ffbc687", "Total items, weight and volume do not match the shipment total. Would you like to update the shipment to match the packing details?");
						}
						else
						{
							message = Res.GetString("9112dc68-b60c-4726-a53c-fd9ba6a1ce25", "Total items do not match shipment inners, would you like to update shipment inners to match total items?");
						}
					}

					PopupMessage(e, message);
				}
			}
			else
			{
				PopupMessage(e, Res.GetString("8b67ffdf-90b8-4be4-b889-89572a21c123", "Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?"));
			}

			void PopupMessage(CancelEventArgs e, string message)
			{
				if (!e.Cancel)
				{
					var result = Globals.Message.Show(message, Res.GetString("bfa2270b-0589-4118-9f60-76eb420dc928", "Totals do not match"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					e.Cancel = result != DialogResult.Yes;
				}
			}
		}

		#endregion

		#region SetApprovedShipperStatus

		void SetApprovedShipperStatus()
		{
			if (Shipment.IsTemplate)
			{
				Globals.Message.ShowInformation(Res.GetString("E4923494-302B-46A7-9DD9-DBC3AEEB0A3F", "The Inspection status of Template record is not used when creating Shipment so this function is not applicable."));
			}
			else if (!Shipment.GetEditInspectionErrorForUncertifiedUser().IsEmpty)
			{
				Globals.Message.ShowInformation(Res.GetString("d7ef4184-b2a9-474f-8060-c88ba5dfb119", "Inspection Status can only be calculated by users with valid BKG and DTA certificate types saved in their Staff Profile."));
			}
			else if (!Shipment.SetApprovedShipperStatus(ZString.Empty, true))
			{
				Globals.Message.ShowInformation(Shipment.AviationSecurity.ErrorMessageForCantSetApprovedShipperStatus, Res.GetString("6de420eb-b2a1-43d2-8e62-e7c4e96bd57f", "Calculate Inspection Status"));
			}
		}

		#endregion

		#region Prompt Reason For Changing Shipment Booking Status

		void OnShipmentBookingStatusUpdate(object sender, ShipmentBookingStatusEventArgs e)
		{
			if (e.NewStatus == ShipmentStatusList.Codes.SIRejected)
			{
				var userResponseArgs = new UserResponseArgument
				{
					Caption = Res.GetString("2260e346-9edb-40e1-8623-fb8b4619826d", "Rejection Reason"),
					Message = Res.GetString("ad04924a-88a1-42fb-a66a-7176bb4f044c", "Please enter the reason of rejection."),
					Buttons = ZMessageBoxButtons.OKCancel,
					DefaultButton = ZMessageBoxDefaultButton.Button1,
					Icon = ZMessageBoxIcon.Information,
					MinimumResponseLength = 1
				};

				e.StatusUpdatedReason = Globals.Message.QueryUserResponse(userResponseArgs);
			}
			else if (e.CurrentStatus == ShipmentStatusList.Codes.ElectronicShippingInstruction && e.NewStatus == ShipmentStatusList.Codes.Confirmed)
			{
				var shipment = Shipment;
				if (shipment != null)
				{
					var provider = ObjectFactory.Get<IVisualizableDocumentCommandProvider>();
					var stmMenuItem = LoadFormsStmMenuItem(shipment.Factory, new ZGuid("d23fd083-3b81-4539-9c26-837c67cbcaa2"));

					if (stmMenuItem != null)
					{
						var command = provider?.GetCommand(shipment, stmMenuItem, ModuleIDs.JobShipment);
						if (command != null && command.IsApplicable)
						{
							command.Execute();
						}
					}
				}
			}
		}

		IStmMenuItem LoadFormsStmMenuItem(BusinessObjectFactory factory, ZGuid menuPK)
		{
			var query = new ZQuery(StmMenuItemSchema.PK, menuPK);
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, Constants.StmMenuItemTypes.Forms);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Shipment));

			return factory.LoadTop1<StmMenuItemBase>(query);
		}

		#endregion

		#region Denied Party Screening

		public void AddScreeningLogsTabPage()
		{
			if (!EventTabPage.IsDisposed)
			{
				var screenStatusControl = new RelatedDeniedPartyScreeningStatusControl();
				screenStatusControl.SetBindingMember("RelatedOrgPartyScreeningStatusCollection");

				ComplianceLogTabHelper.AddLogTabIfNeeded(Shipment, EventTabPage, screenStatusControl);
			}
		}

		void AddComplianceRiskMessageBannerIfNeeded()
		{
			ComplianceRiskPresentationHelper.AddComplianceRiskWarningMessageBannerIfNeeded(this);
		}

		#endregion

		#region Regenerate House Bill

		void RegenerateHouseBill(bool manuallyRegenerated = false)
		{
			var shouldShowHouseBillRegenerationError = Shipment.IsPropertyReadOnlyDueToPhase(CommonShipment.Schema.JS_HouseBill) &&
				(!FreightRegistry.Instance.EnforceUniqueHAWBNumbers.Value || !Shipment.IsAir);
			if (shouldShowHouseBillRegenerationError)
			{
				var messsageError = Shipment.IsAir
					? Res.GetString("ef5e9e06-ff0c-4fa2-affc-14419985e813", "When House Bill is read only, it can only be generated for Air shipment if “Registry > Freight > AWB > HAWB > Enforce Unique HAWB Numbers” is set to Yes")
					: Res.GetString("7cc6cc12-0a9b-4769-97d2-02995536aec9", "The House Bill Number cannot be regenerated while the House Bill field has been set to read-only status");

				Globals.Message.Show(
					messsageError,
					Res.GetString("e88e1c5a-bc6a-4138-9c0e-1d4be4ca439e", "Regenerate House Bill Number"),
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			Shipment.RegenerateHouseBillInSaving(manuallyRegenerated);
		}

		#endregion

		#region ITabVisibilityDeciderPersistence Members

		bool ITabVisibilityDeciderPersistence.HasTabVisiblePersisted
		{
			get { return Shipment.IsInDatabase && (!Shipment.JS_InvisibleTabsXML.IsEmpty || Shipment.JS_VisibleTabs > 0); }
		}

		bool ITabVisibilityDeciderPersistence.RetrieveTabPageVisible(ZTabPage page)
		{
			return this.RetrieveTabPageVisible(page, Shipment, JobShipmentSchema.JS_InvisibleTabsXML);
		}

		void ITabVisibilityDeciderPersistence.StoreTabVisible(ZTabPage page)
		{
			this.StoreTabVisible(page, Shipment, JobShipmentSchema.JS_InvisibleTabsXML);
		}

		#endregion

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion

		#region Prompt for Inactivation

		void IRequireInactivationPrompt.PromptForInactivation()
		{
			using (Shipment.SuspendShipmentStatusUpdateActions())
			{
				if (IsShipmentCreatedByEDI())
				{
					var message = ResString.GetMultilingualString("18AD73C3-35E4-4DEC-BDC5-AE53547C6DDA", @"This Shipment was created electronically, would you like to send a Shipping Instruction Rejection message to the booking party?");
					var caption = ResString.GetMultilingualString("003281F3-EE95-40F8-A47E-93485E132792", "Shipment Rejection");
					var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.None);
					if (dialogResult == DialogResult.Yes)
					{
						Shipment.GenerateStatusEvent(ShipmentStatusList.Codes.SIRejected, (NoResString)"Shipment Cancelled");
					}
				}

				Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;
			}
		}

		bool IsShipmentCreatedByEDI()
		{
			return Shipment.Logs.GetAllLogs().Cast<StmALog>().Any(log => log.Parameters.TryGetValue(Params.New, out var newParameter) && newParameter == ShipmentStatusList.Codes.ElectronicShippingInstruction);
		}

		#endregion

		#region ISupportSwitchTabPage

		void ISupportSwitchTabPage.SwitchTabPage(string tabPageName)
		{
			var tabPage = MainTabControl.GetTabPage(tabPageName);
			if (tabPage != null)
			{
				MainTabControl.SelectedTab = tabPage;
			}
		}
		#endregion

		#region Calculate Delivery Due Date

		void CalculateDeliveryDueDate_Click(object sender, EventArgs e)
		{
			if (Shipment.JS_DeliveryDueDateInfo.ReadOnly)
			{
				Env.Security.MaintainShipmentDeliveryDueDateOverride.ShowError();
			}
			else
			{
				Shipment.CalculateDeliveryDueDate();
			}
		}

		#endregion

		#region ICustomerServiceMenuSectionCodeOverridable

		string ICustomerServiceMenuSectionCodeOverridable.SectionCode
			=> MainTabControl.SelectedTab.Name == ComplianceWiseConstants.ComplianceRiskTabPageName
			? ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise
			: ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding;

		#endregion ICustomerServiceMenuSectionCodeOverridable

		#region Dispose

		System.ComponentModel.IContainer components;
		protected override void Dispose(bool disposing)
		{
			if (Shipment != null && !Shipment.IsTemplate)
			{
				Shipment.GetReasonChangingSecurityInspectionStatusEventHandler -= MessagePopupHelper.PromptReasonForChangingSecurityInspectionStatusEventHandler;
				Shipment.GetReasonChangingSecurityAdditionalInspectionStatusEventHandler -= MessagePopupHelper.PromptReasonForChangingSecurityAdditionalInspectionStatusEventHandler;
				Shipment.GetReasonForChangingDeliveryDueDateEventHandler -= MessagePopupHelper.PromptReasonForChangingDeliveryDueDateEventHandler;
				Shipment.DeliveryDueDateNotChangedInManualCalculation -= MessagePopupHelper.NotifyDeliveryDueDateNotChangedInManualCalculation;
			}

			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Save

		protected override void Save(ITransactionParticipant[] factories)
		{
#if !WINZOR
			Enterprise.Freight.Forwarding.PerformanceMonitor.PerformanceTracker.Instance.StartTrack(nameof(Save), Shipment.GetType().Name, Shipment.PK.ToString(), Shipment.OuterPackLines.Count);
			try
			{
#endif
				base.Save(factories);

#if !WINZOR
				Enterprise.Freight.Forwarding.PerformanceMonitor.PerformanceTracker.Instance.EndTrack(nameof(Save), Shipment.GetType().Name, Shipment.PK.ToString(), Shipment.OuterPackLines.Count);
			}
			catch (Exception ex)
			{
				Enterprise.Freight.Forwarding.PerformanceMonitor.PerformanceTracker.Instance.EndTrack(nameof(Save), Shipment.GetType().Name, Shipment.PK.ToString(), Shipment.OuterPackLines.Count, ex.Message);

				throw;
			}
#endif
		}

		#endregion

		#region ShowAlertWhenEHBLRequestsPending

		void ShowAlertWhenEHBLRequestsPending(ForwardingShipment shipment)
		{
			if (shipment == null || !shipment.EnabledElectronicBOL)
			{
				return;
			}

			var caption = Res.GetString("984837b0-a1ff-4f39-b783-d908ab7f94b2", "Electronic House Bill Alert");

			if (shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress)
			{
				Globals.Message.ShowInformation(Res.GetString("8fba1ee5-039d-4bb3-89b5-347a579b640c",
					"The Electronic House Bill in this Shipment has an open Amendment Request.\r\nTo action, please go to Electronic Messages > Electronic Bill Of Lading."), caption);
			}
			else if (shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper && !HasDocumentDeliveredEventAfterSwitchedToPaper(shipment))
			{
				Globals.Message.ShowInformation(Res.GetString("8396390a-364a-4519-a0e3-61166e0df30a",
					"The Electronic House Bill in this Shipment has an unactioned 'Switched To Paper' Instruction.\r\nPlease print the Original House Bill of Lading on paper."), caption);
			}
		}

		bool HasDocumentDeliveredEventAfterSwitchedToPaper(ForwardingShipment shipment)
		{
			var mostRecentSwitchedToPaperLog = shipment.Logs.MostRecentLogByPostedTime(ZArchitecture.Business.Events.BillStatusUpdated, log => log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType)
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, out var department)
				&& eventType == Constants.BillStatusUpdatedTypes.SwitchedToPaper && department == ElectronicBOLConstants.EHBLEventDepartments.TitleRegistry);

			if (mostRecentSwitchedToPaperLog == null)
			{
				return false;
			}

			return shipment.Logs.GetAllLogs().OfType<StmALog>().Any(log => log.SL_PostedTimeUtc >= mostRecentSwitchedToPaperLog.SL_PostedTimeUtc && !log.IsCancelled
				&& log.SL_SE_NKEvent == ZArchitecture.Business.Events.DocumentDeliveredCode
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, out var name) && name == "Bill Of Lading"
				&& log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType) && eventType == "Original");
		}

		#endregion

		#region IPreviousNextControlOverrideProvider Members

		bool IPreviousNextControlOverrideProvider.OverridesSetPreviousNextControlParentAndPosition => true;

		bool IPreviousNextControlOverrideProvider.ShouldDoBaseSetPreviousNextControlParentAndPosition => false;

		bool IPreviousNextControlOverrideProvider.OverridesGetPreviousNextControl => false;

		void IPreviousNextControlOverrideProvider.SetPreviousNextControlParentAndPosition(ZPreviousNextControl control)
		{
			control.Parent = PreviousNextControlForDesigner.Parent;
			control.Location = PreviousNextControlForDesigner.Location;
			Controls.Remove(PreviousNextControlForDesigner);
			PreviousNextControlForDesigner.Dispose();
		}

		ZPreviousNextControl IPreviousNextControlOverrideProvider.GetPreviousNextControl(ModuleResultsBusinessObject bizObj, ZController controller) => throw new NotImplementedException();

		#endregion
	}
}
