using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI.AWB;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class WeightVolChargeableControl : ZUserControl
	{
		public WeightVolChargeableControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(DocsChargeableUnitLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(DocsVolumeUnitLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(DocsWeightUnitLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		#region Bind

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Shipment != null)
			{
				Shipment.JS_TransportModeInfo.ValueChanged -= JS_TransportModeInfo_ValueChanged;
				Shipment.TransportsIncludingRelated.CountChanged -= TransportsIncludingRelated_CountChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Shipment != null)
			{
				Shipment.JS_TransportModeInfo.ValueChanged += JS_TransportModeInfo_ValueChanged;
				Shipment.TransportsIncludingRelated.CountChanged += TransportsIncludingRelated_CountChanged;
				SetupControlsBasedOnTransportMode();
			}
		}

		#endregion

		#region Shipment

		ForwardingShipment Shipment
		{
			get { return CurrentDataItem as ForwardingShipment; }
		}

		#endregion

		#region TransportsIncludingRelated Changed

		void TransportsIncludingRelated_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			InitViewEditBillButtonText();
		}

		#endregion

		#region TransportMode Changing

		void JS_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupControlsBasedOnTransportMode();
		}

		void SetupControlsBasedOnTransportMode()
		{
			InitViewEditBillButtonText();
			SetLoadingMetersVisibility();
		}

		void SetLoadingMetersVisibility()
		{
			bool loadingMetersVisible = Shipment != null && Shipment.IsRoadLoadingMetersEnabled;
			JS_DocumentedLoadingMetersCalcEdit.Visible = loadingMetersVisible;
			JS_ManifestedLoadingMetersCalcEdit.Visible = loadingMetersVisible;

			if (loadingMetersVisible)
			{
				JS_DocumentedChargeableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 66, true);
				JS_ManifestedChargeableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 66, true);
				DocsChargeableUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 70, true);
			}
			else
			{
				JS_DocumentedChargeableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 44, true);
				JS_ManifestedChargeableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 44, true);
				DocsChargeableUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 48, true);
			}
		}

		#endregion

		#region ViewButton

		void InitViewEditBillButtonText()
		{
			if (Shipment.IsAirOrSeaAirAndHasFirstAirLegLoadingInCurrentCountry)
			{
				ViewEditBillButton.Text = Res.GetString("Forwarding|WeightVolChargeableControl|ViewEditAWB", "View/Edit AWB");
			}
			else
			{
				ViewEditBillButton.Text = Res.GetString("Forwarding|WeightVolChargeableControl|ViewBillOfLading", "View Bill Of Lading");
			}
		}

		void ViewEditBillButton_Click(object sender, EventArgs e)
		{
			if (Shipment != null && Shipment.IsAirOrSeaAirAndHasFirstAirLegLoadingInCurrentCountry)
			{
				ZFormModaliser.ShowDialogAndDispose(new AWBViewEditForm(Shipment));
			}
			else if (Shipment != null && Shipment.IsSea)
			{
				if (!ShowFormBuilderBillOfLading()
					&& !ShowDocEngineBillOfLading())
				{
					Globals.Message.Show(Res.GetString("eb8aa771-498b-49ef-b8b4-9258a5f32487",
						"Bill of Lading applicable to this shipment could not be found."));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("3995d535-8af3-44f1-b8ff-fb66f4dfe56f",
					"You can only view/edit the Bill for Air or Sea Shipments."));
			}

			ClearCachedDocumentInformation();
		}

		void ClearCachedDocumentInformation()
		{
			if (Shipment != null )
			{
				Shipment.ClearCachedDocumentData();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		protected DocumentPack GetDocumentPack()
		{
			DocumentCommand menuItem = GetBillOfLadingMenuItem((NoResString)"Bill of Lading");
			DocumentPack pack = new DocumentPack(menuItem, Shipment, DocumentNote.LoadNote(Shipment).GetCompleteFieldList(), null);
			if (pack.Count == 0)
			{
				menuItem = GetBillOfLadingMenuItem((NoResString)"Bill of Lading To Preprinted");
				pack = new DocumentPack(menuItem, Shipment, DocumentNote.LoadNote(Shipment).GetCompleteFieldList(), null);
			}

			return pack;
		}

		DocumentCommand GetBillOfLadingMenuItem(string menuItemName)
		{
			ZQuery menuItemQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, menuItemName);
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Shipment);
			menuItemQuery.AddToFilter(StmMenuItemSchema.SU_MenuType, SQLComparisonOperator.NotEqual, Core.Constants.StmMenuItemTypes.Forms);
			return Shipment.Factory.LoadTop1<DocumentCommand>(menuItemQuery);
		}

		internal bool ShowDocEngineBillOfLading()
		{
			if (Shipment.HasChanges)
			{
				Globals.Message.Show(Res.GetString("328F8CF9-40D4-4461-87BB-A21F1F939E91", "Please save your record before running View Bill Of Lading."));
				return true;
			}

			var pack = GetDocumentPack();

			if (!pack.Any())
			{
				return false;
			}

			var instructions = new DeliveryInstructions(pack)
			{
				Destination = DeliveryInstructionDestination.Preview,
				DeliveryOptions = AllowedDeliveryOptions.PreviewOnly
			};

			if (instructions.Recipients.Any())
			{
				instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			}
			else
			{
				var contact = instructions.Recipients.AddNew();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			}

			using (var task = new PrintTask())
			{
				task.Add(pack);
				task.Run(instructions);
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool ShowFormBuilderBillOfLading()
		{
			var shipment = Shipment;
			var menuPK = ZGuid.Empty;

			switch (shipment.JS_HouseBillOfLadingType)
			{
				case Core.Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStates:
				case Core.Constants.HouseBillOfLadingTypes.Code.DataHawkBill:
				case Core.Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZ:
				case Core.Constants.HouseBillOfLadingTypes.Code.FIATAHBL:
				case Core.Constants.HouseBillOfLadingTypes.Code.TANHBL:
				case Core.Constants.HouseBillOfLadingTypes.Code.CargowiseBill:
				case Core.Constants.HouseBillOfLadingTypes.Code.ITClubAustralia:
				case Core.Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTerms:
				case Core.Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTermsNoLaw:
				case Core.Constants.HouseBillOfLadingTypes.Code.ITClubNewZealand:
					if (RawDataRegistry.Instance.UseFormBuilderHouseBills.Value)
					{
						menuPK = ShipmentSystemFormMenuItems.BillOfLadingPK;
					}
					break;

				case Core.Constants.AddtionalHouseBillTypeMenu.Code.DHLHBL:
					menuPK = ShipmentSystemFormMenuItems.DHLBillOfLadingPK;
					break;

				case Core.Constants.AddtionalHouseBillTypeMenu.Code.YusenHBL:
					menuPK = ShipmentSystemFormMenuItems.YusenBillOfLadingPK;
					break;

				case Core.Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStatesPreprinted:
				case Core.Constants.HouseBillOfLadingTypes.Code.FIATAHBLPreprinted:
				case Core.Constants.HouseBillOfLadingTypes.Code.CargowiseBillPreprinted:
				case Core.Constants.HouseBillOfLadingTypes.Code.ITClubNewZealandPreprinted:
				case Core.Constants.HouseBillOfLadingTypes.Code.TANHBLPreprinted:
				case Core.Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZPreprinted:
				case Core.Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaPreprinted:
					if (RawDataRegistry.Instance.UseFormBuilderHouseBills.Value)
					{
						menuPK = ShipmentSystemFormMenuItems.PreprintedBillOfLadingPK;
					}
					break;

				default:
					if (RawDataRegistry.Instance.UseFormBuilderHouseBills.Value
						&& FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.Value.ContainsCode(shipment.JS_HouseBillOfLadingType))
					{
						menuPK = ShipmentSystemFormMenuItems.BillOfLadingPK;
						break;
					}

					return false;
			}

			var query = new ZQuery(StmMenuItemSchema.PK, menuPK);
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Forms);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Shipment);

			var menuItem = shipment.Factory.LoadTop1<StmMenuItemBase>(query);

			var provider = ObjectFactory.Get<IVisualizableDocumentCommandProvider>();
			var command = provider.GetCommand(Shipment, menuItem, ModuleIDs.JobShipment);
			command?.Execute();

			return command != null;
		}

		#endregion
	}
}
