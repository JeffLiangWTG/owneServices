using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using DataTransfer.Common.GUI.MenuItems;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.HelperClasses;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI.AWB;
using Enterprise.Freight.Forwarding.GUI.Consol;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Common.TemplateRecords;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.SystemToSystemTrust;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
#if !WINZOR
using Enterprise.RemoteDesktopServices.Server;
#endif
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using WTG.IdentitySecurity;
using static Enterprise.Freight.Forwarding.Business.TransitWarehouseInstructionHelper;
using Constants = Enterprise.Core.Constants;
using EventRefParams = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ConsolForm : ZTemplateForm, ICarrierContractAssignableJobForm, INotifications, ICustomerServiceMenuSectionCodeOverridable
	{
		protected ConsolForm()
		{
			InitializeComponent();
		}

		public ConsolForm(ForwardingConsol businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

			Consol = businessEntity;
			Consol.RequestPermissionByImpersonation = ShipmentVsConsolGUIMessageHelper.Instance.RequestPermissionByImpersonation;

			ShipmentDomainService.GetInstance(businessEntity.Factory).DocAddressOrganisationValidationWarnOnly = true;
			HookConsol(Consol);
			SetupConsolUserControl();
			DataContext = Constants.DataContext.Consol;

			if (!Consol.IsTemplateRecord)
			{
				AddPlugins();
			}

			SetupAWBTabPage();
			SetupMenuItems();

			if (Consol.IsTemplateRecord)
			{
				WorkflowTabPage.Dispose();
				ElectronicMessagingTabPage.Dispose();

				if (ConsolControl?.ShipmentModuleButtonGrid != null)
				{
					ConsolControl.ShipmentModuleButtonGrid.Dispose();
				}
			}
			else
			{
				WorkflowTabPage.Initialize(businessEntity);
			}

			ForwardingConsolDocumentSupporterGuiQueryProvider.Register(Consol.Factory);
			ForwardingShipmentDocumentSupporterGuiQueryProvider.Register(Consol.Factory);
			ServicesSelectionGuiProvider.Register(Consol.Factory);

			if (SupplyChainSecurityConfiguration.SCSSupportedCountryList.Any(countryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode == countryCode))
			{
				SecuredFreightVerificationChecker.Register(Consol.Factory);
			}

			BusyIndicatorProvider.Register(this);
			ConfirmationProvider.Register(this);

			if (ContractsPermissions.IsAllocationsVisible())
			{
				MultiAllocationRouteSelectorProvider.Register(this);
				OverrideAllocationRouteDialogProvider.Register(this);

				if (FreightConfigurationRegistry.Instance.EnableContainerWeightLimitSupportOnAllocationRoutes.Value)
				{
					AllocationContainerWeightLimitDialogsProvider.Register(Consol.Factory);
				}
			}

			RefreshBillingAndApportionmentComponents();
			RefreshConsolCostingTab();

			AddScreeningLogsTabPage();
			AddTemplateLabelIfApplicable();
			SetupControlsBasedOnTransportMode();
			AddComplianceRiskMessageBannerIfNeeded();
		}

		readonly ForwardingConsol Consol;

		ICCACommonAssignmentValidationData ICarrierContractAssignableJobForm.ParentJob => Consol;

		#region User Control

		void SetupConsolUserControl()
		{
			ConsolControl.Consol = Consol;
			ConsolControl.DockInside(MainTabPage);
			ConsolContainerControl.DockInside(ContainersTabPage);
		}

		public
#if DEBUG
 virtual
#endif
 bool IsAnyShipmentOpenForEdit
		{
			get { return ConsolControl.ShipmentModuleButtonGrid.BusinessObjectsOpenForEdit.Count > 0; }
		}

		#endregion

		#region Menus

		void SetupMenuItems()
		{
			SetupActionsMenu();

			if (!Consol.IsTemplateRecord)
			{
				SetupElectronicMessagingMenu();
			}

			var deniedPartyScreeningPresentationManager = new DeniedPartyScreeningPresentationManager();
			deniedPartyScreeningPresentationManager.CreateMenusForJob(this);
			new DeniedPartyScreeningActionsProvider(this, Consol).AddJobsMenuItem();
		}

		void SetupElectronicMessagingMenu()
		{
			ElectronicMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Consol.ElectronicMessaging", "Electronic Messaging"));
			MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, ElectronicMessagingMenuItem);

			ElectronicMessagingMenuItem.AddFormsMenuItems(Consol,
				ModuleIDs.JobConsol,
				CreateElectronicMenuItemInfos());
		}

		IEnumerable<IMenuItemInfo> CreateElectronicMenuItemInfos()
		{
			yield return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("ed89d27e-3b22-40ce-92ed-cb66574c07f5", "Advanced Air Cargo Report"),
				SubMenus = new IMenuItemInfo[]
				{
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuACASHouseChecklistUSPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.CCTHouseManifestPK
					}
				}
			};

			yield return new ParentMenuItemInfo
			{
				Name = ResString.GetMultilingualString("79e3e7dc-f07d-49ce-9bdd-0ae7b599bc55", "Carrier"),
				SubMenus = new IMenuItemInfo[]
				{
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuBookingRequestPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuShippingInstructionPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuVerifiedGrossContainerWeightPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuShippingOrderCNKPK
					},
					new SystemMenuItemInfo
					{
						ID = ConsolSystemFormMenuItems.DocumentMenuAirBookingRequestPK
					},
					new CustomMenuItemInfo
					{
						Name = Res.GetString("84C8475A-2495-4C90-99B7-36959C459F3C", "View/Transact eBL"),
						OnClick = VieweBL_Click,
						IsApplicable = IsElectronicBillOfLadingReferenceApplicable
					}
				}
			};
		}

		void VieweBL_Click(BusinessObject bizObj)
		{
			OpenBolero(Consol);
		}

		public static void OpenBolero(ForwardingConsol consol)
		{
			if (!Env.Security.MaintainConsolAllowViewTransactEBL.IsAllowed)
			{
				Globals.Message.ShowError(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainConsolAllowViewTransactEBL));
				return;
			}

			if (consol.JK_ElectronicBillOfLadingReference.IsEmpty)
			{
				Globals.Message.ShowError(Res.GetString("3C0B4F01-E1E7-4EA5-8129-1FF7D3B55850", "eBL Identifier not found."));
				return;
			}

			var postUrl = FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.GetGalileoEndPointUrl();
			var galileoAudience = FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.GetGalileoAudience();
			if (!FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.EnableEBLIntegration
				|| postUrl.IsEmpty
				|| galileoAudience.IsEmpty)
			{
				Globals.Message.ShowError(Res.GetString("AA7D801E-4194-4407-8AA1-AAC4DDEE008C", "Please set the configuration in Registry > Freight > Enable Bolero eBL Integration."));
				return;
			}

			var rid = TRIRecordHelper.GetTRIRecord();

			if (rid.IsEmpty)
			{
				Globals.Message.ShowError(Res.GetString("AE1A4369-F6FE-4751-8EBC-E26DC10D6126", "RID not found."));
				return;
			}

			var systemToSystemCertificateValue = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			if (string.IsNullOrEmpty(systemToSystemCertificateValue.ClientId) || string.IsNullOrEmpty(systemToSystemCertificateValue.PrivateKey) || systemToSystemCertificateValue.CertificateBytes.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("3A48600C-2864-4B18-9B26-91C605D6A15F", "The system does not have a valid certificate. Please update the certificate via service task '{0}'. If this issue persists, please contact your administrator.", "TCM"));
				return;
			}
			var privateKey = RSAKeyProvider.ImportPrivateKey(systemToSystemCertificateValue.PrivateKey);
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			var clientId = systemToSystemCertificateValue.ClientId;
			var cert = new X509Certificate2(systemToSystemCertificateValue.CertificateBytes);
			var payload = GeneratePayload(clientId, rid, consol.JK_ElectronicBillOfLadingReference, out ZString errorMessage);
			if (!errorMessage.IsEmpty)
			{
				Globals.Message.ShowError(errorMessage);
				return;
			}
#if !WINZOR
			var systemToSystemTrustMessage = SystemToSystemTrustMessageFactory.Create(privateKey, new Guid(clientId), new Guid(galileoAudience), new Uri(postUrl), payload);
			var systemToSystemTrustHandler = ObjectFactory.Get<ISystemToSystemTrustHandler>();
			systemToSystemTrustHandler.SendMessage(systemToSystemTrustMessage.AccessToken, systemToSystemTrustMessage.PostUrl);
#endif
		}

		public static JwtPayload GeneratePayload(ZString eblIssuer, ZString rid, ZString eblDocumentId, out ZString errorMessage)
		{
			errorMessage = "";

			var companyCode = GlbCompany.CurrentCompany?.GC_Code ?? ZString.Empty;
			if (companyCode.IsEmpty)
			{
				errorMessage = Res.GetString("0EC92846-D4B1-4001-B723-D2885071D378", "The company code is not found in the current company.");
				return null;
			}

			var userCode = GlbStaff.CurrentUser?.GS_Code ?? ZString.Empty;
			if (userCode.IsEmpty)
			{
				errorMessage = Res.GetString("726A7A45-C53A-4305-AD7D-8AAC15B0D929", "The user code is not found in the current user.");
				return null;
			}

			var language = GlbStaff.CurrentUser?.Language ?? "EN";
			var timeZone = GlbBranch.CurrentBranch?.HomePort?.TimeZoneSet?.StandardZone?.R2_CivilianTimeZoneCode ?? "UTC";

			var jwtPayload = new JwtPayload
				{
					{ "companyCode", companyCode.ToString() },
					{ "eblDocumentId", eblDocumentId.ToString() },
					{ "timeZone", timeZone.ToString() },
					{ (NoResString)"language", language.ToString() },
					{ (NoResString)"rid", rid.ToString() },
					{ "userCode", userCode.ToString() },
					{ (NoResString)"action", "" },
					{ "eblIssuer", eblIssuer.ToString() }
				};

			return jwtPayload;
		}

		bool IsElectronicBillOfLadingReferenceApplicable(BusinessObject bizObj)
		{
			return !Consol.JK_ElectronicBillOfLadingReference.IsEmpty && Consol.IsSea && FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.EnableEBLIntegration;
		}

		void ViewConsignmentMenuItem_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(Consol.JobNumber) || Consol.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("cc76b8c4-854a-4d69-bc1f-8bf2163e0b52", "Please save consol before opening Cargo Tracker"));
				return;
			}

			var licenseCode = GlbCompany.CurrentCompany.GetLicenceCode();
			var consignmentNumber = Consol.JK_UniqueConsignRef;
			var cargoTrackerUrlGenerator = ObjectFactory.Get<ICargoTrackerUrlGenerator>();

			var tokenResult = cargoTrackerUrlGenerator.GetSelfSignedSystemToSystemToken(licenseCode);
			var cargoTrackerErrorMessage = Res.GetString("98c3e0ab-3337-4a5e-81fe-8f1c52aaa4f0", "Unable to show consignment in Cargo Tracker");

			if (!string.IsNullOrWhiteSpace(tokenResult.ErrorMessage))
			{
				Globals.Message.ShowError(cargoTrackerErrorMessage);
				return;
			}

			if (tokenResult.RedirectUrl != null)
			{
				WebUrlLauncher.Launch(tokenResult.RedirectUrl.ToString());
				return;
			}

			var (consignmentUrl, consignmentErrorMessage) = cargoTrackerUrlGenerator.Generate(tokenResult.Token, licenseCode, consignmentNumber);
			if (!string.IsNullOrWhiteSpace(consignmentErrorMessage))
			{
				Globals.Message.ShowError(cargoTrackerErrorMessage);
				return;
			}

			WebUrlLauncher.Launch(consignmentUrl.ToString());
		}

		void AirlineConnectMenuItem_Click(object sender, EventArgs e)
		{
			if (Consol.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("cb76b4c4-45fa-4d69-bc1f-89f2863e0b57", "Please save before opening AirlineConnect form"));
				return;
			}

			if (Consol.JK_TransportMode != Constants.TransportModes.Air)
			{
				Globals.Message.ShowError(Res.GetString("cb76b4c5-45fa-4d69-bc1f-89f2863e1a54", "AirlineConnect is only available for consols with a transport mode of AIR"));
				return;
			}

			var validationMessage = Consol.ValidateAirlineConnectPreRequisites();
			if (!validationMessage.IsEmpty)
			{
				Globals.Message.ShowError(validationMessage);
				return;
			}

			string valueWithoutFallback = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (string.IsNullOrWhiteSpace(valueWithoutFallback))
			{
				Globals.Message.ShowError(Res.GetString("12c6b55f-d6ad-429a-a9e0-4aac17f0af43", "{0} cannot be opened in a browser as GLOW has not been configured for this client.\r\nRegistry: {1}/{2}", "AirlineConnect", GlowRegistry.Instance.GlowPortalsUri.Category, GlowRegistry.Instance.GlowPortalsUri.Caption));
				return;
			}

			string pk = (NoResString)"pk";
			var url = UrlBuilder.GenerateURL(new Uri(valueWithoutFallback), "RTS", null, additionalQueryStrings: new[] { (pk, Consol.PK.ToString()) });

			WebUrlLauncher.Launch(url.ToString());
		}

		void SetupActionsMenu()
		{
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("ConsolForm|Menu|RecalculateCreditorForLoggedInCompany", "Recalculate Creditor for logged in Company"), new EventHandler(RecalculateCreditor_Click));
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("ConsolForm|Menu|RecalculateRelatedPartiesForLoggedInCompany", "Recalculate Related Parties for logged in Company"), new EventHandler(RecalculateRelatedParties_Click));

			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("ConsolForm|Menu|CreateNewMAWB", "Create New MAWB"), NewAWBFormBuilder.NewEventHandler(Consol));
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("ConsolForm|Menu|AttachBookings", "Attach Bookings"), AttachBookingModuleGridHelper.NewEventHandler(Consol));
			ZString[] enableCargoTrackerVisibleList = { Constants.TransportModes.Air, Constants.TransportModes.Sea };

			if (FreightDataRegistry.Instance.EnableCargoTracker.Value && enableCargoTrackerVisibleList.Contains(Consol.JK_TransportMode))
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("ConsolForm|Menu|ViewConsignmentInCargoTracker", "View in Cargo Tracker"), new EventHandler(ViewConsignmentMenuItem_Click));
			}

			if (FreightDataRegistry.Instance.EnableAirlineConnectMenu.Value)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("ConsolForm|Menu|AirlineConnect", "AirlineConnect"), new EventHandler(AirlineConnectMenuItem_Click));
			}

			AddAttachLoadListActionsMenuIfNecessary();
			AddCreateTestHVLVDataActionsMenuIfNecessary();
			AddTransitWarehouseMenuItem();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Iceland)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("ConsolForm|Menu|GenerateShipmentSendingarnumers", "Generate Shipment Sendingarnumers"), new EventHandler(GenerateShipmentSendingarnumers));
			}

			var exportToVerboseXmlMenuItem = CreateExportToVerboseXmlMenuItem();
			ZFormMenuStrategy.AddInterfaceConnectorMenuItem(this, exportToVerboseXmlMenuItem);

			var exportToLightWeightXmlMenuItem = CreateExportToLightWeightXmlMenuItem();
			ZFormMenuStrategy.AddInterfaceConnectorMenuItem(this, exportToLightWeightXmlMenuItem);

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };
		}

		MenuItem CreateExportToVerboseXmlMenuItem()
		{
			MenuItem exportToXmlMenuItem = new ZMenuItem(ExportXmlMenuItemHelper.VerboseMenuItemText);
			exportToXmlMenuItem.Name = ExportXmlMenuItemHelper.VerboseMenuItemName;
			exportToXmlMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Consol.Actions.ExportVerboseXml.StoreFile", "Store as File"), ExportToXmlMenuItem.VerboseHandler(OnStoreAsFile_Click)));
			exportToXmlMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Consol.Actions.ExportVerboseXml.ExportOverseasAgent", "Export to Overseas Agent"), ExportToXmlMenuItem.VerboseHandler(OnExportToOverseasAgent_Click)));
			return exportToXmlMenuItem;
		}

		MenuItem CreateExportToLightWeightXmlMenuItem()
		{
			MenuItem exportToLightWeightXmlMenuItem = new ZMenuItem(ExportXmlMenuItemHelper.LightWeightMenuItemText);
			exportToLightWeightXmlMenuItem.Name = ExportXmlMenuItemHelper.LightWeightMenuItemName;
			exportToLightWeightXmlMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Consol.Actions.ExportLightWeightXml.StoreFile", "Store as File"), ExportToXmlMenuItem.LightWeighHandler(OnStoreAsFile_Click)));
			exportToLightWeightXmlMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Consol.Actions.ExportLightWeightXml.ExportOverseasAgent", "Export to Overseas Agent"), ExportToXmlMenuItem.LightWeighHandler(OnExportToOverseasAgent_Click)));
			return exportToLightWeightXmlMenuItem;
		}

		void AddAttachLoadListActionsMenuIfNecessary()
		{
			ZFormMenuStrategy.AddActionsMenuItem(
				this,
				ResString.GetMultilingualString("ConsolForm|Menu|AttachELoadList", "Attach HVLV Origin Load List"),
				OnAttachLoadListActionsMenu_EventHandler);
		}

		void AddCreateTestHVLVDataActionsMenuIfNecessary()
		{
			var createTestHVLVDataMenuItem = ObjectFactory.Get<IHVLVTestDataProvider>(nameof(IHVLVTestDataProvider)).GetCreateTestHVLVShipmentMenuGroupIfNecessary(DataSource as IBusiness);
			if(createTestHVLVDataMenuItem != null && createTestHVLVDataMenuItem is ZMenuItem menuItem)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, menuItem);
			}
		}

		void OnAttachLoadListActionsMenu_EventHandler(object sender, EventArgs e)
		{
			if (Consol.HasChanges || !Consol.IsInDatabase)
			{
				Globals.Message.ShowWarning(Res.GetString("37c8f398-e95f-4556-9a61-c6cbc80dbff3", "Please save {0} before attaching load lists to it.", Consol.HumanReadableName));
				return;
			}
			AttachLoadListToConsolGUIHelper.ShowPopupModuleAndAttachSelected(Consol);
		}

		#region Transit Warehouse

		void AddTransitWarehouseMenuItem()
		{
			var departureMenu = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.Departure", "Departure"));
			departureMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendDepartureTWReceiptInstruction", "Send Receipt Instruction"), OnSendTransitWarehousePickupReceiptInstruction));
			departureMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendDepartureTWDispatchInstruction", "Send Dispatch Instruction"), OnSendTransitWarehousePickupDispatchInstruction));
			departureMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendDepartureTWReceiptAndDispatchInstruction", "Send Receipt and Dispatch Instruction"), OnSendTransitWarehousePickupReceiptAndDispatchInstruction));
			departureMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendDepartureTWPrepareDispatchInstruction", "Send Prepare Dispatch Instruction"), OnSendDepartureTWPrepareDispatchInstruction));

			departureMenu.MenuItems.Add(ZMenuItem.Separator);
			departureMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendDepartureTWDispatchStopLoadInstruction", "Stop Load Request"), OnSendDepartureTWDispatchStopLoadInstruction));
			departureMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendDepartureTWDispatchCancelStopLoadInstruction", "Cancel Stop Load Request"), OnSendDepartureTWDispatchCancelStopLoadInstruction));

			var arrivalMenu = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.Arrival", "Arrival"));
			arrivalMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendArrivalTWReceiptInstruction", "Send Receipt Instruction"), OnSendTransitWarehouseDeliveryReceiptInstruction));
			arrivalMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendArrivalTWDispatchInstruction", "Send Dispatch Instruction"), OnSendTransitWarehouseDeliveryDispatchInstruction));
			arrivalMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendArrivalTWReceiptAndDispatchInstruction", "Send Receipt and Dispatch Instruction"), OnSendTransitWarehouseDeliveryReceiptAndDispatchInstruction));
			arrivalMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehousel.SendArrivalTWPrepareDispatchInstruction", "Send Prepare Dispatch Instruction"), OnSendArrivalTWPrepareDispatchInstruction));

			arrivalMenu.MenuItems.Add(ZMenuItem.Separator);
			arrivalMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendArrivalTWDispatchStopLoadInstruction", "Stop Load Request"), OnSendArrivalTWDispatchStopLoadInstruction));
			arrivalMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse.SendArrivalTWDispatchCancelStopLoadInstruction", "Cancel Stop Load Request"), OnSendArrivalTWDispatchCancelStopLoadInstruction));

			var transitWarehouseMenu = new ZMenuItem(ResString.GetMultilingualString("Forwarding.Shipment.Actions.TransitWarehouse", "Transit Warehouse"));
			transitWarehouseMenu.MenuItems.Add(departureMenu);
			transitWarehouseMenu.MenuItems.Add(arrivalMenu);

			ZFormMenuStrategy.AddActionsMenuItem(this, transitWarehouseMenu);
		}

		void OnSendTransitWarehousePickupReceiptInstruction(object sender, EventArgs e) => RequestMessageSending(Direction.Pickup, ServiceRequest.Receipt);

		void OnSendTransitWarehousePickupDispatchInstruction(object sender, EventArgs e) => RequestMessageSending(Direction.Pickup, ServiceRequest.Dispatch);

		void OnSendTransitWarehousePickupReceiptAndDispatchInstruction(object sender, EventArgs e) => RequestMessageSending(Direction.Pickup, ServiceRequest.ReceiveAndDispatch);

		void OnSendTransitWarehouseDeliveryReceiptInstruction(object sender, EventArgs e) => RequestMessageSending(Direction.Delivery, ServiceRequest.Receipt);

		void OnSendTransitWarehouseDeliveryDispatchInstruction(object sender, EventArgs e) => RequestMessageSending(Direction.Delivery, ServiceRequest.Dispatch);

		void OnSendTransitWarehouseDeliveryReceiptAndDispatchInstruction(object sender, EventArgs e) => RequestMessageSending(Direction.Delivery, ServiceRequest.ReceiveAndDispatch);

		void RequestMessageSending(Direction direction, ServiceRequest service)
		{
			if (service == ServiceRequest.Dispatch || service == ServiceRequest.ReceiveAndDispatch)
			{
				var shipments = Consol.Shipments.Cast<ForwardingShipment>().Where(s => s.IsLinkedDCNSplitted).ToList();
				if (shipments.Count > 0)
				{
					Globals.Message.ShowError(Res.GetString("71006046-a8c1-44d9-a74b-49837eac0fb0", "Failed to send Dispatch Instruction - Transit Warehouse Dispatch Consignments linked to {0} have already been split.", shipments[0].JobNumber));
					return;
				}
			}

			if (TransitWarehouseInstructionGUIHelper.CheckMatchingStatus(SupporterType.Consol, Consol.RelatedPackLines.Cast<ForwardingPackLine>()))
			{
				if (Consol.Shipments.Count <= TransitWarehouseInstructionGUIHelper.MaximumNumberOfShipmentsCanBeProcessedInGUI || !FreightDataRegistry.Instance.EnableSendingForwardingConsolToTWHAsynchronously.Value)
				{
					using (new ZWaitCursorChanger(this))
					{
						new TransitWarehouseInstructionHelper(Consol, this).SendTransitWarehouseInstruction(direction, service);
					}
				}
				else
				{
					if (Consol.HasChanges)
					{
						Globals.Message.ShowError(Res.GetString("83f1fada-6890-4b74-90e0-f59ad3b932bc", "Please save your changes before sending the Transit Warehouse Instruction."));
						return;
					}

					var parameters = new Dictionary<string, string>();
					parameters[EventRefParams.Direction] = direction.ToString();
					parameters[EventRefParams.Service] = service.ToString();

					Consol.Logs.AddNew(AutoEvents.MessageSendingRequest, ZDateTimeOffset.Now, parameters.ToArray());
					Consol.Factory.Save();
					Globals.Message.Show(Res.GetString("db99b595-06d4-4ed6-b842-b505771ce438", "Message Sending Request is in Progress. User may need to check to confirm UXML sending completion status after a short period."));
				}
			}
		}

		void OnSendDepartureTWDispatchStopLoadInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseDispatchStopLoadInstruction(true, false, Consol.JK_PackDepotDispatchRequested);
		}

		void OnSendDepartureTWDispatchCancelStopLoadInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseDispatchStopLoadInstruction(true, true, Consol.JK_PackDepotDispatchRequested);
		}

		void OnSendArrivalTWDispatchStopLoadInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseDispatchStopLoadInstruction(false, false, Consol.JK_UnpackDepotDispatchRequested);
		}

		void OnSendArrivalTWDispatchCancelStopLoadInstruction(object sender, EventArgs e)
		{
			SendTransitWarehouseDispatchStopLoadInstruction(false, true, Consol.JK_UnpackDepotDispatchRequested);
		}

		void SendTransitWarehouseDispatchStopLoadInstruction(bool isDeparture, bool isCancel, ZDateTime requestedTime)
		{
			if (TransitWarehouseInstructionGUIHelper.CheckMatchingStatus(TransitWarehouseInstructionHelper.SupporterType.Consol, Consol.RelatedPackLines.Cast<ForwardingPackLine>()))
			{
				using (new ZWaitCursorChanger(this))
				{
					var direction = isDeparture ? TransitWarehouseInstructionHelper.Direction.Pickup : TransitWarehouseInstructionHelper.Direction.Delivery;
					new TransitWarehouseInstructionHelper(Consol, this).SendTransitWarehouseDispatchStopLoadInstruction(direction, isCancel, requestedTime.IsValid);
				}
			}
		}

		void OnSendDepartureTWPrepareDispatchInstruction(object sender, EventArgs e)
		{
			ShowFormForPrepareDispatchInstructionIfValidConsol(Direction.Pickup);
		}

		void OnSendArrivalTWPrepareDispatchInstruction(object sender, EventArgs e)
		{
			ShowFormForPrepareDispatchInstructionIfValidConsol(Direction.Delivery);
		}

		void ShowFormForPrepareDispatchInstructionIfValidConsol(Direction direction)
		{
			if (Consol.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("a97f86b8-d759-9aac-4080-22fc7c301d73", "Please save your changes before sending the Transit Warehouse Instruction."));
				return;
			}

			var supporter = Consol as ITransitWarehouseInstructionSupporter;
			var address = TransitWarehouseInstructionHelper.GetTransitWarehouseAddress(supporter, direction);
			if (address == null)
			{
				Globals.Message.ShowError(Res.GetString("40add235-97e8-0ea0-488e-95843c2debca",
				"The {0} {1} must be entered before the {0} TW {2} Instruction can be sent.",
				direction == TransitWarehouseInstructionHelper.Direction.Pickup
					? supporter.PickupDescription
					: supporter.DeliveryDescription,
				supporter.TransitWarehouseDescription,
				Res.GetString("a6dd0e76-0a9f-f5b3-4d24-ad996374fd4a", "Prepare Dispatch")));

				return;
			}

			PrepareDispatchInstructionForm.ShowDialog(Consol, direction);
		}

		#endregion

		#region Recalculate Related Parties for logged in Company

		void RecalculateRelatedParties_Click(object sender, EventArgs e)
		{
			Consol.AttemptToUpdateExistingValueInRecalculation += Consol_AttemptToUpdateExistingValueInRecalculation;
			var result = RelatedPartiesHelper.RecalculateRelatedParties(Consol);

			if (!result.WasSuccessful)
			{
				Globals.Message.ShowError(result.Log);
			}
			Consol.AttemptToUpdateExistingValueInRecalculation -= Consol_AttemptToUpdateExistingValueInRecalculation;
		}

		void Consol_AttemptToUpdateExistingValueInRecalculation(object sender, ForwardingConsol.RecalculateRelatedPartyCancelEventArgs e)
		{
			var message = Res.GetString("c291cd7f-d6b8-471e-9f87-ce5bf829be05",
				"{0} has already been entered. Do you wish to update the {0} based on your Company Related Party Configuration?", e.RecalculatedPropertyName);

			var context = new DialogDefaultContext(
				new ZGuid("dc371403-ff08-4b6a-b94d-3bb9712a97fe"),
				ResString.GetMultilingualString("0f05b746-7d31-40d6-85b3-1786da68a3c7", "Confirmation"),
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Question,
				null,
				showCheckboxOnly: true);
			var result = Globals.Message.ShowOrDefault(context, message);

			e.Cancel = result == ZDialogResult.No;
		}

		void RecalculateCreditor_Click(object sender, EventArgs e)
		{
			var errors = RelatedPartiesHelper.TryRedefaultCreditor(Consol);

			if (errors.Length > 0)
			{
				errors = Res.GetString("29aaa4aa-84e9-4ea9-a8be-a64e09280a7c", @"Please review the following warnings:

{0}", errors);

				var caption = Res.GetString("0D995D67-6797-4540-A333-0D64895D94A6", "Related Parties re-defaulting for logged in Company");
				Globals.Message.ShowWarning(errors, caption);
			}
			else
			{
				RefreshConsolAddressTab();
			}
		}

		#endregion

		protected override bool AllowActionDataMenuItem
		{
			get { return true; }
		}

		protected MenuItem ExportMenuItem = new ZMenuItem(ResString.GetMultilingualString("Export", "&Export"));
		protected MenuItem ImportMenuItem = new ZMenuItem(ResString.GetMultilingualString("Import", "&Import"));
		MenuItem ElectronicMessagingMenuItem;

		#endregion

		#region Plug-Ins

		void AddPlugins()
		{
			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				PlugIns.Add(ControllerIDs.CO2ePlugin);
			}

			PlugIns.Add(ControllerIDs.ComplexPickup);
			PlugIns.Add(ControllerIDs.ComplexDelivery);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, 1);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.JobConsolContainersPacking, 3);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.US.AMS, GetRequestedTabPageIndexBeforeAccountingTabPage);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.JP.AFRPluggedIntoConsol, GetRequestedTabPageIndexBeforeAccountingTabPage);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.HK.Traxon, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.CargoManifestPlugInForConsol, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.AU.AirCargo, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.AU.SeaCargo, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.AU.SeaCargoDepot, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.NZ.ECIWriteOffManifestingConsolSynchroniser, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.NZ.ConsolToExpressECIConverter, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.NZ.ConsolToSeaCargoWriteOffConverter, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.NZ.MAFeBACCaConsolPlugIn, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.NZ.OutwardReport, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.SG.CMDConsol, GetRequestedTabPageIndexBeforeAccountingTabPage);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.US.InBond, GetRequestedTabPageIndexBeforeAccountingTabPage);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.CA.CAConsolACI, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.CA.CAConsoleManifest, GetRequestedTabPageIndexBeforeAccountingTabPage);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.GB.CcsukAirInventory, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.EU.NctsMovementController, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, GetRequestedTabPageIndexBeforeAccountingTabPage);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.FR.CINTemporaryStorageConsolController, GetRequestedTabPageIndexBeforeAccountingTabPage);

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.CA.RNSConsolPlugIn, GetRequestedTabPageIndexBeforeAccountingTabPage);

			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.Customs.EU.ExitSummaryController);
			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.Add(ControllerIDs.UniversalDataCarrierMessaging);

			ElectronicMessagingTabControl.PlugIns.Add(ControllerIDs.Customs.GB.ChiefExportConsolIntegrationController);
			ElectronicMessagingTabControl.PlugIns.Add(ControllerIDs.Customs.FR.CINExportConsolIntegrationController);
			ElectronicMessagingTabControl.PlugIns.Add(ControllerIDs.Customs.IL.CustomsMessaging);

			ElectronicMessagingTabControl.PlugIns.Add(ControllerIDs.PortMessaging);

			PlugIns.Add(ControllerIDs.DocumentVisualizer);
			PlugIns.Add(ControllerIDs.DtbBooking);

			ConsolContainerControl.SetupPlugIn();

			PlugIns.Add(ControllerIDs.JobInvoicing, null, () => AccountingTabControl);
			PlugIns.Add(ControllerIDs.SellApportionmentForGateway, GetConsolSecurityForJobInvoicing(ControllerIDs.SellApportionmentForGateway), () => AccountingTabControl);
			PlugIns.Add(ControllerIDs.Apportionment, GetConsolSecurityForJobInvoicing(ControllerIDs.Apportionment), () => AccountingTabControl);
			PlugIns.Add(ControllerIDs.JobInvoicingConsol, GetConsolSecurityForJobInvoicing(ControllerIDs.JobInvoicingConsol), () => AccountingTabControl);
			PlugIns.Add(ControllerIDs.JobProfitLossConsol, Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsol, SecurityCore.ProfitLoss), () => AccountingTabControl);

			if (ComplianceRiskHelper.IsFreightEnabledComplianceWise)
			{
				PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
			}
		}

		[return: DpiState(DpiState.Unscaled)]
		int GetRequestedTabPageIndexBeforeAccountingTabPage()
		{
			var tabPage = TopLevelTabControl.GetTabPage("AccountingTabPage");
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

		protected override Menu GetMenuForPlugInCore(ControllerID controllerID)
		{
			if (controllerID.Equals(ControllerIDs.UniversalDataCarrierMessaging)
				|| controllerID.Equals(ControllerIDs.Customs.GB.ChiefExportConsolIntegrationController)
				|| controllerID.Equals(ControllerIDs.Customs.FR.CINExportConsolIntegrationController)
				|| controllerID.Equals(ControllerIDs.PortMessaging)
				|| controllerID.Equals(ControllerIDs.Customs.IL.CustomsMessaging))
			{
				return ElectronicMessagingMenuItem;
			}

			return base.GetMenuForPlugInCore(controllerID);
		}

		void SetConsolJobInvoicingSecurityCheckpoints()
		{
			PlugIns.GetPlugIn(ControllerIDs.JobInvoicingConsol).SetSecurityCheckpoint(GetConsolSecurityForJobInvoicing(ControllerIDs.JobInvoicingConsol), null);
			PlugIns.GetPlugIn(ControllerIDs.Apportionment).SetSecurityCheckpoint(GetConsolSecurityForJobInvoicing(ControllerIDs.Apportionment), null);
			PlugIns.GetPlugIn(ControllerIDs.SellApportionmentForGateway).SetSecurityCheckpoint(GetConsolSecurityForJobInvoicing(ControllerIDs.SellApportionmentForGateway), null);
		}

		SecurityCheckpoint GetConsolSecurityForJobInvoicing(ControllerID controllerID)
		{
			if (controllerID == ControllerIDs.Apportionment)
			{
				return Env.Security.MaintainConsolJobInvoicing;
			}
			else if (controllerID == ControllerIDs.SellApportionmentForGateway)
			{
				return Env.Security.GatewayConsolJobInvoicing;
			}
			else if (controllerID == ControllerIDs.JobInvoicingConsol)
			{
				return Consol.IsGateway()
					? Env.Security.GatewayConsolJobInvoicing
					: Env.Security.MaintainConsolJobInvoicing;
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Saving

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = ShowAllocationAdjustmentDialog();

			if (result == ContinueWithSave.Yes)
			{
				TryCreateJobHeadersForNewShipmentsIfAllowed();
				SyncGatewaySellToCostIfNecessary();
				result = base.ValidateAndSave();
			}

			return result;
		}

		protected ContinueWithSave ShowAllocationAdjustmentDialog()
		{
			var result = ContinueWithSave.Yes;

			if (Consol.IsPreAllocationExceededAndRestricted)
			{
				result = AllocationAdjustmentDialog.ConfirmAllocationAdjustments(new[] { Consol }) ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes &&
				(Consol.Validation.ReceivingForwarderHasBeenChanged()
				|| Consol.Validation.SendingForwarderHasBeenChanged()
				|| ShipmentConsigneeHasBeenChanged
				|| ShipmentConsignorHasBeenChanged))
			{
				result = NonMatchingAgentsDialog.CheckAndConfirm(new[] { Consol }, Consol.Shipments.Cast<ForwardingShipment>())
							? ContinueWithSave.Yes
							: ContinueWithSave.No;
			}

			if (result == ContinueWithSave.Yes && Consol.HasUpdatedAssemblyMasterAsDirectMaster() && !hasUserConfirmedAssemblyMasterAsDirectMaster)
			{
				var assemblyMasterAcknowledgementHelper = new Common.AssemblyMasterAcknowledgementHelper();
				var (dialogResult, keyValuePairsForLogging) = assemblyMasterAcknowledgementHelper.ShowDialog(() => GlbStaff.CurrentUser.GS_Code);
				result = dialogResult == DialogResult.OK ? ContinueWithSave.Yes : ContinueWithSave.No;
				if (result == ContinueWithSave.Yes)
				{
					hasUserConfirmedAssemblyMasterAsDirectMaster = true;
					Consol.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.Acknowledged, ZDateTimeOffset.Now, keyValuePairsForLogging.ToArray());
				}
			}

			if (result == ContinueWithSave.Yes && AttachedToHVLShipment)
			{
				var hvlShipments = Consol.Shipments.Cast<ForwardingShipment>().Where(shipment => shipment.IsHighVolumeLowValue);

				result = ShowPreSaveDialogsForHVLShipmentsWithNoHVLVConsignments(result, hvlShipments);

				if (result == ContinueWithSave.Yes)
				{
					result = ShowPreSaveDialogsForCancellableCustomsJobs(result, hvlShipments);
				}
			}

			if (result == ContinueWithSave.Yes)
			{
				result = RequestShipmentsControllingPartySecurity(DocAddressType.ControllingAgent);
			}

			if (result == ContinueWithSave.Yes)
			{
				result = RequestShipmentsControllingPartySecurity(DocAddressType.ControllingCustomer);
			}

			return result;
		}

		ContinueWithSave ShowPreSaveDialogsForHVLShipmentsWithNoHVLVConsignments(ContinueWithSave result, IEnumerable<ForwardingShipment> hvlShipments)
		{
			if (hvlShipments.Any(shipment => !shipment.HasHVLVDataCreated))
			{
				var message = Res.GetString("acf46ba3-e058-468e-90c7-d082b6aa8a52", "There are no HVLV Consignments on the HVL Shipment(s). Do you want to continue saving? Shipment type cannot be changed after consolidation is saved.");

				result = Globals.Message.Show(message, Res.GetString("590bfb97-c5d3-4045-87b5-c1d9f8975b1f", "Warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK
					? ContinueWithSave.Yes
					: ContinueWithSave.No;
			}
			return result;
		}

		ContinueWithSave ShowPreSaveDialogsForCancellableCustomsJobs(ContinueWithSave result, IEnumerable<ForwardingShipment> hvlShipments)
		{
			var primaryFieldPropertyInfos = new List<ZPropertyInfo>
				{
					Consol.JK_MasterBillNumInfo,
					Consol.Transports?.MostInterestingTransport?.JW_VoyageFlightInfo
				};

			if (Consol.Shipments.Any())
			{
				var forwardingShipments = Consol.Shipments.OfType<ForwardingShipment>();
				primaryFieldPropertyInfos.AddRange(forwardingShipments.SelectMany(s => new[] { s.JS_HouseBillInfo, s.JS_TransportModeInfo }));
			}

			if (primaryFieldPropertyInfos.Any(info => info != null && info.HasChanges))
			{
				var hvlShipmentsWithCancellableCustomsJobs = hvlShipments.Where(shipment => shipment.HVLVConsignmentHeader != null && shipment.HVLVConsignmentHeader.CancellableCustomsJobs.Any(job => !job.IsCancelled));

				if (hvlShipmentsWithCancellableCustomsJobs.Any(shipment => shipment.HVLVConsignmentHeader.CustomsJobCanNotCancelReason.Any()))
				{
					result = ContinueWithSave.No;
					var shipmentErrorMessage = new List<string>();
					var shipmentErrorReason = new List<string>();

					var cannotCancelShipments = hvlShipmentsWithCancellableCustomsJobs.Where(shipment => shipment.HVLVConsignmentHeader.CustomsJobCanNotCancelReason.Any());

					cannotCancelShipments.Select(shipment => shipment.HVLVConsignmentHeader.CustomsJobCanNotCancelReason).ForEach(shipmentErrorReason.AddRange);

					foreach (var reason in shipmentErrorReason.Distinct())
					{
						var shipmentCollection = string.Join(",", cannotCancelShipments.Where(shipment => shipment.HVLVConsignmentHeader.CustomsJobCanNotCancelReason.Any(x => x == reason))
																					 .Select(shipment => shipment.JS_UniqueConsignRef));
						shipmentErrorMessage.Add(reason + " : " + shipmentCollection);
					}

					var errorSection = string.Join(System.Environment.NewLine, shipmentErrorMessage);
					var errorMessage = Res.GetString("7909a35e-27f5-4871-9c2c-16d933cb5d94", @"Consolidation can't be saved for primary field(s) change because Customs job(s) have already been created for the following shipment(s) and reason:{0}
Please detach these shipment(s) from the consolidation and create new shipment(s) and/or consolidation as required.", parameters: errorSection);
					Globals.Message.ShowError(errorMessage);
				}
				else if (hvlShipmentsWithCancellableCustomsJobs.Any(shipment => shipment.HVLVConsignmentHeader.CancellableCustomsJobs.Any(job => !job.IsCancelled)))
				{
					var shipmentsName = string.Join(",", hvlShipmentsWithCancellableCustomsJobs.Select(x => x.JS_UniqueConsignRef).ToList());
					var message = Res.GetString("9b9f338e-3913-407f-a05c-8f0e640062c1", @"Customs job(s) have already been created for the following shipment(s), saving the consolidation may negatively affect existing Customs job(s) due to primary field(s) update.
{0}", parameters: shipmentsName);

					var dialogResult = Globals.Message.Show(message, "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
					if (dialogResult == DialogResult.Cancel)
					{
						result = ContinueWithSave.No;
					}
				}
			}

			return result;
		}

		ContinueWithSave RequestShipmentsControllingPartySecurity(DocAddressType docAddressType)
		{
			foreach (ForwardingShipment shipment in Consol.Shipments)
			{
				SecurityCheckpoint securityCheckPoint;

				if (docAddressType == DocAddressType.ControllingAgent)
				{
					securityCheckPoint = shipment.GetControllingAgentSecurityCheckPoint();
				}
				else
				{
					securityCheckPoint = shipment.GetControllingCustomerSecurityCheckPoint();
				}

				var result = ControllingPartySecurityHelper.RequestSaveWithEmptyControllingPartyAuthorization(shipment, docAddressType, securityCheckPoint);

				if (result.AuthorizationWasRun)
				{
					return result.ContinueWithSave;
				}
			}

			return ContinueWithSave.Yes;
		}

		bool hasUserConfirmedAssemblyMasterAsDirectMaster;

		bool ShipmentConsigneeHasBeenChanged
		{
			get { return Consol.Shipments.Cast<CommonShipment>().Any(shipment => shipment.Validation.ConsigneeHasBeenChanged()); }
		}

		bool ShipmentConsignorHasBeenChanged
		{
			get { return Consol.Shipments.Cast<CommonShipment>().Any(shipment => shipment.Validation.ConsignorHasBeenChanged()); }
		}

		bool AttachedToHVLShipment
		{
			get { return Consol.Shipments.Cast<ForwardingShipment>().Any(shipment => shipment.JS_ShipmentType == Constants.ShipmentTypes.HighVolumeLowValue); }
		}

		void TryCreateJobHeadersForNewShipmentsIfAllowed()
		{
			foreach (var shipment in Consol.Shipments
					.Cast<ForwardingShipment>()
					.Where(s => !s.IsInDatabase && s.JobHeader == null))
			{
				var handler = new LocalClientJobHandler(shipment);
				handler.Initialize();
			}
		}

		protected override void SaveInternal()
		{
			using (DeleteApportionmentChargesWhenSaveJobConsolCostMonitor.AddTempService(Consol.Factory))
			{
				base.SaveInternal();
			}
		}

		#endregion

		#region Loading

		protected override void OnLoad(EventArgs e)
		{
			if (Consol != null && (Consol.IsGateway() || Consol.IsLegacyGateway))
			{
				//Initalising the enabled tabs overrides the 'Gateway Billing' text that we want to see for Gateway Consols
				var invoicingTab = PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				if (invoicingTab != null)
				{
					invoicingTab.TabPage.Text = Res.GetString("eb9ae777-76fd-4891-87c8-2775ec61b687", "Gateway Billing");
				}
			}

			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				new TabConfigurationManager(MainMenu, MainTabControl).Enabled = true;
			}

			using (Consol.SuspendSettingHasChanges())
			{
				if (Consol is not IComplianceItemRiskStatusProvider provider || !provider.IsEnabledComplianceWise)
				{
					new DeniedPartyScreeningPresentationManager().ResynchronizeScreeningStatus(false, new[] { Consol }, null);
				}
			}
		}

		#endregion

		#region Validation

		protected override void PerformValidation()
		{
			foreach (ForwardingShipment shipment in Consol.Shipments)
			{
				shipment.MarkAsNeedingValidationWhenControllingCustomerOrAgentRequireDefaulting();
			}

			base.PerformValidation();
		}

		#endregion

		#region Caption

		public override string FormCaption
		{
			get { return Res.GetString("ConsolForm|FormCaption", "Consol") + " " + Consol.JK_UniqueConsignRef; }
		}

		#endregion

		public override bool IsResizableByTabPageAllowed => true;

		#region Delete

		protected override DialogResult ShowConfirmationForDelete()
		{
			if (Consol.Shipments.Count > 0)
			{
				var msg = Res.GetString("0fbc9ff8-da34-485a-ac6a-e3f6f85f7fa4", "Shipments on this Consol may become unattached.\r\nThe Shipments can be marked inactive from the Shipments menu.\r\n\r\nDo you wish to permanently delete this Consol from the system?");
				return Globals.Message.Show(msg, Res.GetString("10030872-2312-4628-afef-b69dbe3813cc", "Delete Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
			}
			else
			{
				return base.ShowConfirmationForDelete();
			}
		}

		#endregion

		#region Errors

		protected override DialogResult ShowErrorsDialogCore(bool includeIgnoreOption)
		{
			if (!ConsolControl.ShowSubHouseBillsCheckBox.Checked && Consol.SubShipmentsHaveErrors)
			{
				ConsolControl.ShowSubHouseBillsCheckBox.Checked = true;
			}
			return base.ShowErrorsDialogCore(includeIgnoreOption);
		}

		protected override void HandleSaveException(Exception ex)
		{
			var mawbAllocationException = ex as MAWBAllocationException;
			if (mawbAllocationException != null)
			{
				Globals.Message.ShowError(mawbAllocationException.Message, mawbAllocationException.Heading);
			}
			else
			{
				base.HandleSaveException(ex);
			}
		}

		#endregion

		#region Denied Party Screening

		public void AddScreeningLogsTabPage()
		{
			if (!LogsTabPage.IsDisposed)
			{
				var screenStatusControl = new RelatedDeniedPartyScreeningStatusControl();
				screenStatusControl.SetBindingMember("RelatedOrgPartyScreeningStatusCollection");

				ComplianceLogTabHelper.AddLogTabIfNeeded(Consol, LogsTabPage, screenStatusControl);
			}
		}

		void AddComplianceRiskMessageBannerIfNeeded()
		{
			ComplianceRiskPresentationHelper.AddComplianceRiskWarningMessageBannerIfNeeded(this);
		}

		#endregion

		#region Events

		void HookConsol(ForwardingConsol consol)
		{
			ShipmentDomainService.GetInstance(consol.Factory).AttachRelatedOrderRequest += new EventHandler<AttachRelatedOrderRequestEventArgs>(Service_QueryAttachRelatedOrder);
			ShipmentDomainService.GetInstance(consol.Factory).NewSupplierBuyerLink += new EventHandler<NewSupplierBuyerLinkEventArgs>(Service_OnNewSupplierBuyer);
			consol.OnOverrideWaybillDefaultsChanging += new CancelEventHandler(ConsolForm_OnOverrideWaybillDefaultsChanging);
			consol.OnOverrideSecurityDeclarationDefaultsChanging += new CancelEventHandler(ConsolForm_OnOverrideSecurityDeclarationDefaultsChanging);
			consol.DeallocatePrintedNeutralMAWB += new CancelEventHandler(Consol_DeallocatePrintedNeutralMAWB);
			consol.OnReallocatingPrintedMawb += OnConsolReallocatingPrintedMawb;
			consol.OnOverridingChargeableRate += new CancelEventHandler(Consol_OnOverridingChargeableRate);

			consol.ShowMessageOnGUI += Consol_ShowMessageOnGUI;
			consol.SendingForwarderChanged += ConsolSendingOrReceivingForwarderChanged;
			consol.ReceivingForwarderChanged += ConsolSendingOrReceivingForwarderChanged;
			consol.JK_AgentTypeInfo.ValueChanged += JK_AgentTypeInfo_ValueChanged;
			consol.JK_SendingForwarderHandlingTypeInfo.ValueChanged += JK_SendingForwarderHandlingTypeInfo_ValueChanged;
			consol.JK_ReceivingForwarderHandlingTypeInfo.ValueChanged += JK_ReceivingForwarderHandlingTypeInfo_ValueChanged;
			consol.JK_TransportModeInfo.ValueChanged += JK_TransportModeInfo_ValueChanged;
			consol.OnShowConfirmMessageOnGUI += OnShowConfirmMessageOnGUIHandler;
			consol.BookingAgentDefaultingAsker = new ConfirmationPrompt();
		}

		void UnhookConsol(ForwardingConsol consol)
		{
			ShipmentDomainService.GetInstance(consol.Factory).AttachRelatedOrderRequest -= new EventHandler<AttachRelatedOrderRequestEventArgs>(Service_QueryAttachRelatedOrder);
			ShipmentDomainService.GetInstance(consol.Factory).NewSupplierBuyerLink -= new EventHandler<NewSupplierBuyerLinkEventArgs>(Service_OnNewSupplierBuyer);
			consol.OnOverrideWaybillDefaultsChanging -= new CancelEventHandler(ConsolForm_OnOverrideWaybillDefaultsChanging);
			consol.OnOverrideSecurityDeclarationDefaultsChanging -= new CancelEventHandler(ConsolForm_OnOverrideSecurityDeclarationDefaultsChanging);
			consol.DeallocatePrintedNeutralMAWB -= new CancelEventHandler(Consol_DeallocatePrintedNeutralMAWB);
			consol.OnReallocatingPrintedMawb -= OnConsolReallocatingPrintedMawb;
			consol.OnOverridingChargeableRate -= new CancelEventHandler(Consol_OnOverridingChargeableRate);

			consol.ShowMessageOnGUI -= Consol_ShowMessageOnGUI;
			consol.SendingForwarderChanged -= ConsolSendingOrReceivingForwarderChanged;
			consol.ReceivingForwarderChanged -= ConsolSendingOrReceivingForwarderChanged;
			consol.JK_AgentTypeInfo.ValueChanged -= JK_AgentTypeInfo_ValueChanged;
			consol.JK_SendingForwarderHandlingTypeInfo.ValueChanged -= JK_SendingForwarderHandlingTypeInfo_ValueChanged;
			consol.JK_ReceivingForwarderHandlingTypeInfo.ValueChanged -= JK_ReceivingForwarderHandlingTypeInfo_ValueChanged;
			consol.JK_TransportModeInfo.ValueChanged -= JK_TransportModeInfo_ValueChanged;
			consol.OnShowConfirmMessageOnGUI -= OnShowConfirmMessageOnGUIHandler;
			consol.BookingAgentDefaultingAsker = null;
		}

		#endregion

		#region Gateway Billing

		void ConsolSendingOrReceivingForwarderChanged(object sender, EventArgs e)
		{
			if (descriptionUpdaterTask == null)
			{
				descriptionUpdaterTask = UserIdleWorker.QueueWorkItem(this, "SendingOrReceivingForwarderChanged", 0,
					ZArchitecture.UserIdleWorkItemOptions.AllowAlways, new MethodInvoker(() =>
					{
						Consol.Validation.ValidateJK_AgentType();
						RefreshBillingAndApportionmentComponents();

						descriptionUpdaterTask = null;
					}));
			}
		}
		IDisposable descriptionUpdaterTask;

		void JK_AgentTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetConsolJobInvoicingSecurityCheckpoints();

			var valueChangedEvent = e as ValueChangedEventArgs;
			if (valueChangedEvent != null && DoesSupportAgentBilling((ZString)valueChangedEvent.OldValue) != DoesSupportAgentBilling((ZString)valueChangedEvent.NewValue))
			{
				RefreshBillingAndApportionmentComponents();
			}
			if (valueChangedEvent != null && DoesSupportConsolCosting((ZString)valueChangedEvent.OldValue) != DoesSupportConsolCosting((ZString)valueChangedEvent.NewValue))
			{
				RefreshConsolCostingTab();
			}
		}

		void JK_SendingForwarderHandlingTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var valueChangedEvent = e as ValueChangedEventArgs;
			if (valueChangedEvent != null && (ZString)valueChangedEvent.OldValue != (ZString)valueChangedEvent.NewValue)
			{
				RefreshBillingAndApportionmentComponents();
			}
		}

		void JK_ReceivingForwarderHandlingTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var valueChangedEvent = e as ValueChangedEventArgs;
			if (valueChangedEvent != null && (ZString)valueChangedEvent.OldValue != (ZString)valueChangedEvent.NewValue)
			{
				RefreshBillingAndApportionmentComponents();
			}
		}

		bool DoesSupportAgentBilling(ZString agentType)
		{
			switch (agentType)
			{
				case Constants.AgentType.Agent:
				case Constants.AgentType.CoLoad:
				case Constants.AgentType.AWBCoload:
				case Constants.AgentType.Direct:
					return true;
				default:
					return false;
			}
		}

		bool DoesSupportConsolCosting(ZString agentType) => agentType != Constants.AgentType.AWBMaster;

		void RefreshConsolAddressTab()
		{
			if (Consol != null)
			{
				var plugIn = PlugIns.GetPlugIn(ControllerIDs.DocAddresses) as DocAddressesPlugIn;
				if (plugIn?.TabPage?.TabVisible ?? false)
				{
					plugIn.RefreshAddresses();
				}
			}
		}

		void RefreshConsolCostingTab()
		{
			void SetPluginState(ControllerID controllerID, bool enabled)
			{
				var plugin = PlugIns.GetPlugIn(controllerID);
				if (plugin != null)
				{
					plugin.Enabled = enabled;
				}
			}

			var doesSupportConsolCosting = DoesSupportConsolCosting(Consol?.JK_AgentType ?? ZString.Empty);
			SetPluginState(ControllerIDs.JobInvoicingConsol, doesSupportConsolCosting);
			SetPluginState(ControllerIDs.JobProfitLossConsol, doesSupportConsolCosting);
			SetPluginState(ControllerIDs.Apportionment, doesSupportConsolCosting);
		}

		void RefreshBillingAndApportionmentComponents()
		{
			if (Consol != null)
			{
				var isGatewayBillingEnabled = Consol.IsGateway() || Consol.IsLegacyGateway;
				(PlugIns.GetPlugIn(ControllerIDs.JobInvoicing) as IPluginShouldRefreshMenuForGateway)?.RefreshGatewayElements(isGatewayBillingEnabled);

				var shouldHideSellApportionmentTab = !AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && !Consol.HasGatewaySellApportionments(GlbCompany.CurrentCompany);
				var consolCostingPlugIn = PlugIns.GetPlugIn(ControllerIDs.Apportionment);
				if (consolCostingPlugIn != null)
				{
					(consolCostingPlugIn as IPluginShouldRefreshMenuForGateway)?.RefreshGatewayElements(isGatewayBillingEnabled);
				}

				var sellApportionmentPlugIn = PlugIns.GetPlugIn(ControllerIDs.SellApportionmentForGateway);
				if (sellApportionmentPlugIn != null)
				{
					var isPlugInEnabled = isGatewayBillingEnabled && !shouldHideSellApportionmentTab;
					(sellApportionmentPlugIn as IPluginShouldRefreshMenuForGateway)?.RefreshGatewayElements(isPlugInEnabled);
				}
			}
		}

		void SyncGatewaySellToCostIfNecessary()
		{
			if (Consol != null)
			{
				IPluginForGatewaySellApportionments plugin;
				plugin = PlugIns.GetPlugIn(ControllerIDs.SellApportionmentForGateway) as IPluginForGatewaySellApportionments;
				if (plugin != null)
				{
					plugin.SyncGatewaySellToCostIfNecessary();
				}
			}
		}

		#endregion

		#region Multi AWB Master Support

		void Consol_ShowMessageOnGUI(object sender, ShowMessageOnGUIEventArgs e)
		{
			Globals.Message.ShowInformation(e.Message, e.Title);
		}

		ZDialogResult OnShowConfirmMessageOnGUIHandler(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon)
		{
			return Globals.Message.Show(message, caption, buttons, icon);
		}

		#endregion

		#region Neutral MAWB Allocation

		void Consol_DeallocatePrintedNeutralMAWB(object sender, CancelEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("aec01c73-c6a5-4da3-a15f-b8602cc6aedf", "This neutral MAWB has already been printed in final.\r\nYou should only deallocate this MAWB from the Consolidation if it was issued in error, if you have reason to use a new MAWB for this Consolidation, or the Consolidation was canceled.\r\n\r\nAre you sure you wish to deallocate this MAWB number?"), Res.GetString("4af329fd-7afe-4009-9828-40e545c0a5a1", "Neutral Printed MAWB"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			e.Cancel = result != DialogResult.Yes;
		}

		void OnConsolReallocatingPrintedMawb(object sender, JobMawbReallocationEventArgs e)
		{
			if (IsSavingInProgress)
			{
				e.Cancel = false;
				ErrorReporter.ReportOnce("OnConsolReallocatingPrintedMawbDuringSave", "Attempt to reallocate printed MAWB on save.");
			}
			else
			{
				e.Cancel = e.Mawb == null || Globals.Message.Show(
					Res.GetString("b592af81-a0f4-410e-ae57-1b6399df0093", "Do you wish to reallocate printed neutral MAWB {0}?", string.Format(CultureInfo.InvariantCulture, "{0}-{1}", e.Mawb.JM_Airline3DigitPrefix, e.Mawb.JM_MAWB)),
					Res.GetString("4af329fd-7afe-4009-9828-40e545c0a5a1", "Neutral Printed MAWB"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No;
			}
		}

		#endregion

		#region Addresses Tab

		ZTabPage AddressesTabPage => addressesTabPage ?? (addressesTabPage = PlugIns.GetPlugIn(ControllerIDs.DocAddresses)?.TabPage);
		ZTabPage addressesTabPage;

		#endregion

		#region AWB Tab

		void SetupAWBTabPage()
		{
			MAWBWithMessages.MAWBTabPage.ParentAWBTabPage = AWBTabPage;
			MAWBWithMessages.MAWBTabPage.TopLevelTabControl = MainTabControl;
			MAWBWithMessages.MAWBTabPage.AWBInterface = Consol;
			MAWBWithMessages.MAWBTabPage.ExcludeFromBindingOnSave = true;
			AWBTabPage.ExcludeFromBindingOnSave = true;
		}

		void MainTabControl_SelectedIndexChanging(object sender, EventArgs e)
		{
			if (DesignModeFinder.IsDesigning)
			{
				return;
			}

			if (MainTabControl.SelectedTab == AWBTabPage)
			{
				MAWBWithMessages.MAWBTabPage.ConstructUserControl();
				var consol = (ForwardingConsol)BusinessEntity;

				try
				{
					consol.PopulateAWB();
				}
				catch (ExportAWBHeaderReplaceMacrosException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}

				consol.ResetAddressPickerDropLists();
			}
			else if (MainTabControl.SelectedTab == AddressesTabPage)
			{
				try
				{
					if (BusinessEntity is ForwardingConsol consol)
					{
						consol.HookupDocAddressEventHandler();
					}
				}
				catch (ExportAWBHeaderReplaceMacrosException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void ConsolForm_OnOverrideWaybillDefaultsChanging(object sender, CancelEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("259dec4e-3d63-43b3-9a00-9333089c480f", "Removing the override will reset your AWB data.\r\nYou will lose changes that you have made to the AWB data.\r\n\r\nProceed?"), Res.GetString("067eb474-5172-475b-ab11-cccb08987b3c", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

		void ConsolForm_OnOverrideSecurityDeclarationDefaultsChanging(object sender, CancelEventArgs e)
		{
			var result = Globals.Message.Show(Res.GetString("7ad5f054-4acb-4baf-820e-ac0909b46551", "Removing the override will reset your CSD data.\r\nYou will lose changes that you have made to the CSD data.\r\n\r\nProceed?"), Res.GetString("0a413d03-edaa-41c9-a852-0f02ffda588f", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

		void Consol_OnOverridingChargeableRate(object sender, CancelEventArgs e)
		{
			string message = Res.GetString("09cbeeae-119a-4ce5-998f-44c1715c1cb4", "By overriding chargeable rate, you will disable MAWB information based on detailed Auto Rating results.\r\nProceed?");
			DialogResult result = Globals.Message.Show(message, Res.GetString("2df67609-e642-4077-b3ca-d5f042b37d0c", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

#if DEBUG
		internal void SelectAddressesTabPage_ForTest()
		{
			MainTabControl.SelectedTab = AddressesTabPage;
		}

		internal void SelectAWBTabPage_ForTest()
		{
			MainTabControl.SelectedTab = AWBTabPage;
		}

		internal void SelectContainersTabPage_ForTest()
		{
			MainTabControl.SelectedTab = ContainersTabPage;
		}
#endif

		#endregion

		#region JK_TransportModeInfo_ValueChanged

		void JK_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupControlsBasedOnTransportMode();
		}

		void SetupControlsBasedOnTransportMode()
		{
			bool isVGMVisible = (string)Consol?.TransportMode != Constants.TransportModes.Air;
			ConsolContainerControl.VGMVisible = isVGMVisible;
		}

		#endregion

		#region Related Orders

		void Service_QueryAttachRelatedOrder(object sender, AttachRelatedOrderRequestEventArgs e)
		{
			if (!IsDisposed && !Disposing)
			{
				SelectOrdersForm.ShowForm((ForwardingShipment)e.Shipment);
			}
		}

		#endregion

		#region Buyer Suppler Link

		void Service_OnNewSupplierBuyer(object sender, NewSupplierBuyerLinkEventArgs eventArg)
		{
			eventArg.Result = Globals.Message.Show(Res.GetString("4debe138-355e-4e47-b3b0-b3e650b7a37c", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?"), Res.GetString("b12061be-4bf8-428f-a8a2-284f9296e997", "Save"), ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.Yes);
		}

		#endregion

		#region Shipment Sendingarnumers - Iceland

		void GenerateShipmentSendingarnumers(object sender, EventArgs e)
		{
			var result = Globals.Message.Show(
						Res.GetString("ead83289-5296-4421-9d4b-d6feebb10236", "This action will reset and re-generate ALL shipment's Sendingarnumers. Are you sure you want to proceed?"),
						Res.GetString("6acc11a6-b1ee-44cc-b6ff-2af545a4378c", "Warning"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);

			if (result == DialogResult.Yes)
			{
				var generator = new ShipmentSendingarnumerGenerator(BusinessEntity as ForwardingConsol);
				generator.GenerateSendingarnumers();
			}
		}

		#endregion

		#region Export to XML

		void OnStoreAsFile_Click(object sender, EventArgs e)
		{
			var consol = (ForwardingConsol)BusinessEntity;
			var exporter = GetNewXmlDataTransferExporter(new ForwardingConsolValueObjectDataAdapter(), true);
			exporter.DefaultFileName = consol.JK_UniqueConsignRef + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
			if (!(new ZString(SystemDataRegistry.Instance.ConsolExportDirectory.Value).IsEmpty))
			{
				exporter.InitialDirectory = SystemDataRegistry.Instance.ConsolExportDirectory.Value;
			}
			exporter.PromptUserAndExport(new[] { consol });
		}

#if DEBUG
		protected virtual
#endif
		XmlDataTransferExporter GetNewXmlDataTransferExporter(IValueObjectDataAdapter adapter, bool checkForLicence)
		{
			return new XmlDataTransferExporter(adapter, checkForLicence);
		}

		void OnExportToOverseasAgent_Click(object sender, EventArgs e)
		{
			new EmailToOverseaAgentExporter().PromptUserAndExport((ForwardingConsol)BusinessEntity);
		}

		#endregion

		#region Edit Checking

		protected override Dictionary<IBusiness, ZString> GetListOfBizObjectsToCheckEditing()
		{
			var result = base.GetListOfBizObjectsToCheckEditing();
			foreach (ForwardingShipment obj in Consol.Shipments)
			{
				result.Add(obj, obj.HumanReadableName + " " + obj.JS_UniqueConsignRef);
			}
			return result;
		}

		#endregion

		#region SkipRecentItems

		public bool? SkipRecentItems { get; set; }

		protected override void SaveToRecentItems()
		{
			if (!SkipRecentItems.HasValue || !SkipRecentItems.Value)
			{
				base.SaveToRecentItems();
			}
		}

		#endregion

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion

		#region Template Methods

		void AddTemplateLabelIfApplicable()
		{
			if (Consol.IsTemplateRecord)
			{
				var labelTemplateRecord = new TemplateRecordLabelControl(Consol.TemplateRecord);
				Controls.Add(labelTemplateRecord);

				labelTemplateRecord.SendToBack();
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
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (Consol != null)
				{
					UnhookConsol(Consol);
				}

				if (!AWBTabPage.IsDisposed)
				{
					AWBTabPage.Dispose();
				}
			}

			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
