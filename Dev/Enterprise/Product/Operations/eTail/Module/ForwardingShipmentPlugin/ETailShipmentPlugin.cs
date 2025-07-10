using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Module
{
	public class ETailShipmentPlugin : ZPlugIn
	{
		public ETailShipmentPlugin(ForwardingShipment hostEntity)
			: base(hostEntity)
		{
			Shipment = hostEntity;
			Enabled = GetIsActiveCore();
		}

		ForwardingShipment Shipment { get; }

		#region Plugin Overrides

		protected override ZBool HasUserControl => true;

		protected override Control GetNewUserControl()
		{
			var newControl = new HVLVShipmentUserControl();
			newControl.UpdateVolumeWeightTextBox(Shipment.IsShipmentChargeableByWeight);
			return newControl;
		}

		protected override LicenceCheckpoint LicenceCheckPoint => null;

		protected override ZBool IsActive => GetIsActiveCore();

		ZBool GetIsActiveCore() => Shipment.JS_ShipmentType == ShipmentTypes.HighVolumeLowValue;

		public override string Name => Res.GetString("22dc05ed-f2d9-41a8-93aa-5a97609c77e6", "eManifest Import");

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			var consignmentHeaderIsArchived = ConsignmentHeader.HCH_IsArchived;
			return !consignmentHeaderIsArchived;
		}

		public override ZString PlugInNotDisplayedMessage => Res.GetString("2f87e9b3-267e-4496-bee7-1f1f22fb2e4f", "HVLV Consignments and Item details for this Shipment are not available as they have exceeded the archive period.");

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();
			Shipment.JS_ShipmentTypeInfo.ValueChanged += ShipmentTypeChanged;
			Shipment.ConsignorPKInfo.ValueChanged += ConsignorChanged;
			Shipment.JS_TransportModeInfo.ValueChanged += TransportModeChanged;
			Form.Load += ParentFormLoaded;
		}

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			Shipment.JS_ShipmentTypeInfo.ValueChanged -= ShipmentTypeChanged;
			Shipment.ConsignorPKInfo.ValueChanged -= ConsignorChanged;
			Shipment.JS_TransportModeInfo.ValueChanged -= TransportModeChanged;

			if (Form != null)
			{
				Form.Load -= ParentFormLoaded;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!Shipment.IsDeleted && Shipment.IsHighVolumeLowValue)
			{
				JobComInvoiceLinePartSynchronisationManager.StopManagingWhenActiveDeciderPKWasDisposed(ConsignmentHeader.Factory, ConsignmentHeader.PK);
			}
		}

		#endregion

		#region Top Level Menu

		public override bool ShouldHideTopLevelMenuWithTab => true;

		protected override MenuItem GetNewTopLevelMenu() => HVLVActionsTopLevelMenu;

		MenuItem HVLVActionsTopLevelMenu
		{
			get
			{
				if (hvlvActionsTopLevelMenu == null)
				{
					hvlvActionsTopLevelMenu = new HVLVActionsTopLevelMenu(Shipment);
				}

				return hvlvActionsTopLevelMenu;
			}
		}

		MenuItem hvlvActionsTopLevelMenu;

		#endregion

		#region Event Handlers

		void ShipmentTypeChanged(object sender, EventArgs e)
		{
			Enabled = IsActive;
			UpdateTabVisibility();
		}

		void ConsignorChanged(object sender, EventArgs e)
		{
			if (IsActive)
			{
				var header = Shipment.GetHVLVConsignmentHeader();
				if (header != null)
				{
					var query = new ZQuery(HVLVConsignmentSchema.HVC_HCH_Header, header.PK);
					var consignments = Factory.Load<HVLVConsignment>(query).Where(x =>
						x.HVC_PreScreeningStatus != HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown);
					if (ETailPreScreeningProvider.IsPreScreeningHVLVDetailsEnabled && consignments.Any())
					{
						var messageContent = Res.GetString("26e8d37e-c78c-4767-9822-5141dbeb8bc2", "HVLV Pre-Screening is enabled, changing the eTailer will set the Pre-Screening Status on all HVLV Consignments to Unknown.");
						var caption = Res.GetString("ba0fee35-3db0-4d09-ad0d-6c4d22f1a39d", "eTailer");

						Globals.Message.Show(messageContent, caption, MessageBoxButtons.OK, DialogResult.OK);

						consignments.ForEach(x => x.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown);
					}
				}
			}
		}

		void TransportModeChanged(object sender, EventArgs e)
		{
			var currentControl = UserControl as HVLVShipmentUserControl;
			currentControl.UpdateVolumeWeightTextBox(Shipment.IsShipmentChargeableByWeight);
		}

		void ParentFormLoaded(object sender, EventArgs e)
		{
			Enabled = IsActive;
		}

		#endregion

		#region Saving

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes && ConsignmentHeader != null)
			{
				if (!ConsignmentHeader.IsInDatabase && !ConsignmentHeader.Consignments.Any())
				{
					var message = Res.GetString("ed402c87-6223-44fc-8b58-bbf61e835fbd", "There are no HVLV Consignments on this HVL Shipment. Do you want to continue saving? Shipment type cannot be changed after shipment is saved.");
					var userResponse = Globals.Message.Show(message, Res.GetString("38115355-db6e-4c6e-8d9e-ea8147875678", "Warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					switch (userResponse)
					{
						case DialogResult.OK:
							result = ContinueWithSave.Yes;
							break;
						case DialogResult.Cancel:
						default:
							result = ContinueWithSave.No;
							break;
					}
				}
				else if (IsRegisteredWithPrimaryFieldChanges())
				{
					if (ConsignmentHeader.CancellableCustomsJobs.Any(job => !job.IsCancelled) && CheckJobsCanCancel(ref result))
					{
						var message = Res.GetString("838e8155-b65a-4cf7-9af0-7c08b4d70311", "Customs job(s) have already been created, saving the shipment may negatively affect existing Customs job(s) due to primary field(s) update.");
						var dialogResult = Globals.Message.Show(message, "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
						if (dialogResult == DialogResult.Cancel)
						{
							result = ContinueWithSave.No;
						}
					}
				}
			}

			return result;
		}

		bool CheckJobsCanCancel(ref ContinueWithSave result)
		{
			var cannotCancelInfoCollection = ConsignmentHeader.CustomsJobCanNotCancelReason;

			if (cannotCancelInfoCollection.Count > 0)
			{
				var errorMessage = Res.GetString("9d4ab356-25d3-49a6-976b-b0ac76aaaca4", "Shipment can’t be saved for primary field(s) update because Customs job(s) have already been created. {0} Please create a new shipment instead.", string.Join(",", cannotCancelInfoCollection));
				Globals.Message.ShowError(errorMessage);
				result = ContinueWithSave.No;
				return false;
			}

			return true;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			RegisterPrimaryFieldChangesIfRequired();
		}

		void RegisterPrimaryFieldChangesIfRequired()
		{
			var primaryFieldPropertyInfos = new[]
			{
				Shipment.JS_HouseBillInfo,
				Shipment.JS_TransportModeInfo,
				Shipment.DepartureConsol?.JK_MasterBillNumInfo,
				Shipment.DepartureConsol?.Transports?.MostInterestingTransport?.JW_VoyageFlightInfo,
				Shipment.ArrivalConsol?.JK_MasterBillNumInfo,
				Shipment.ArrivalConsol?.Transports?.MostInterestingTransport?.JW_VoyageFlightInfo
			};

			if (primaryFieldPropertyInfos.Any(info => info != null && info.HasChanges))
			{
				RegisterPrimaryFieldChanges();
			}
		}

		void RegisterPrimaryFieldChanges()
		{
			if (Shipment != null && Shipment.IsCargoReportCreated())
			{
				var list = GetShipmentsWithPrimaryFieldChangesList(Shipment.Factory);
				if (!list.Contains(Shipment.PK))
				{
					list.Add(Shipment.PK);
				}
			}
		}

		bool IsRegisteredWithPrimaryFieldChanges()
		{
			var result = false;
			if (Shipment != null)
			{
				var list = GetShipmentsWithPrimaryFieldChangesList(Shipment.Factory);
				result = list.Contains(Shipment.PK);
			}

			return result;
		}

		List<ZGuid> GetShipmentsWithPrimaryFieldChangesList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ETailShipmentPlugin|ShipmentPKsWithPrimaryFieldChanges", () => new List<ZGuid>());
		}

		public override void OnSaveCompletedOrAborted(bool saved)
		{
			base.OnSaveCompletedOrAborted(saved);

			if (saved)
			{
				var latestCargoReportingLog = Shipment.GetLatestCargoReportingLog();
				if (latestCargoReportingLog != null)
				{
					var hlrAdded = HasMessageErrorsOnAnyOfSurplusedConsignments(ConsignmentHeader.Consignments.OfType<HVLVConsignment>())
						? false
						: AddHLRForCargoReporting(Shipment);

					if (hlrAdded.HasValue && !hlrAdded.Value && IsCargoReportingUpToDate(latestCargoReportingLog))
					{
						Shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.AmendmentProcessing));
						TrySaveFactory(Factory);
					}
				}
			}
		}

		bool IsCargoReportingUpToDate(StmALog latestCargoReportingLog) => latestCargoReportingLog != null && latestCargoReportingLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason] == EventReferenceParameterReasons.CargoReporting;

		bool? AddHLRForCargoReporting(ForwardingShipment shipment, bool isSendingFromMenuItem = false)
		{
			var logInitialized = (shipment.GetLatestCargoReportingLog()) != null;
			var isLogInitializedMessage = Res.GetString("70bdfde7-cfc2-46d0-9b93-0f825038fc2e", @"Cargo Reporting has already been initialized for this Shipment. Would you like to amend Cargo Reporting with your latest changes?

Selecting '{0}' will send all data for Cargo Reporting.
(It is recommended to select '{0}' when you have completed amending the Shipment to avoid possible duplicate amendments on Customs site).",
				DialogResultCaptions.GetTextForDialogResult(DialogResult.Yes));
			var isNotLogInitializedMessage = Res.GetString("439a85a3-8268-4796-b8e4-ecc186f3b57e", @"Selecting '{0}' will send all data for Cargo Reporting.
If you select '{1}' you may send the data at a later time using the menu item, '{2}'",
				DialogResultCaptions.GetTextForDialogResult(DialogResult.Yes), DialogResultCaptions.GetTextForDialogResult(DialogResult.No),
				Invariant($"{CaptionFormatter.WithoutHotKeyFormatting("HVLV")} > {GetNameOfCargoReportDependOnCountryTransportModeAndDirection(shipment)}"));
			string message;

			if (logInitialized)
			{
				message = isLogInitializedMessage;
			}
			else
			{
				message = isNotLogInitializedMessage;
			}

			if (!isSendingFromMenuItem)
			{
				var menuItemInstruction = Res.GetString("22d55423-2c2b-472b-9b91-a592f0edf144",
					"If you select '{0}' you may send the amendment at a later time using the menu item, '{1}'",
					DialogResultCaptions.GetTextForDialogResult(DialogResult.No),
					Invariant($"{CaptionFormatter.WithoutHotKeyFormatting("HVLV")} > {GetNameOfCargoReportDependOnCountryTransportModeAndDirection(shipment)}"));

				message = Invariant($@"{message}

{menuItemInstruction}"); // Paragraph spacing
			}

			var caption = logInitialized ?
				Res.GetString("fcd1ef3f-e8c6-4de2-aee9-862dc95523bc", "Send Cargo Reporting Amendment") :
				Res.GetString("95d2ec10-23bd-433b-a2bf-a2a6d3a2e2a9", "Send Cargo Reporting");

			var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (dialogResult == DialogResult.Yes)
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReporting));
				if (TrySaveFactory(shipment.Factory))
				{
					var queuedMessage = logInitialized ?
						Res.GetString("af96238d-5950-443a-83ee-2ce9bc8528f7", "HVLV Cargo Report Amendment queued for processing.") :
						Res.GetString("d5b12e13-c310-4575-a919-23040fd35c1d", "HVLV Cargo Report queued for processing.");

					Globals.Message.Show(queuedMessage);
					return true;
				}

				return null;
			}

			return false;
		}

		bool HasMessageErrorsOnAnyOfSurplusedConsignments(IEnumerable<HVLVConsignment> consignments)
		{
			return consignments.Where(x => x.IsSurplusAtDestination).Any(x =>
			{
				x.Validation.ValidateAll();
				return x.HasMessageErrors();
			});
		}

		bool TrySaveFactory(BusinessObjectFactory factory)
		{
			try
			{
				factory.Save();
				return true;
			}
			catch (ZSaveConcurrencyException)
			{
				Globals.Message.ShowWarning(Res.GetString("2c21f872-5a6b-450f-992f-c1b9114665e0", "While you have been working, another user has made changes affecting current shipment."));
				return false;
			}
		}

		string GetNameOfCargoReportDependOnCountryTransportModeAndDirection(ForwardingShipment shipment)
		{
			switch (GlbBranch.CurrentBranch.Country.Code)
			{
				case CountryCodes.UnitedStates:
					return Res.GetString("369c55bf-eb97-471e-829f-437e9c3e25fe", "HVLV {0}", LowValueEntries);
				case CountryCodes.Australia:
					return Res.GetString("6016e385-e270-4a97-a655-4a0df30ec690", "HVLV {0} Report",
						shipment.TransportMode == TransportModes.Air ? AirCargo : SeaCargo);
				case CountryCodes.NewZealand:
					return Res.GetString("9d68579a-56f5-4dd9-8102-c31ca8523a21", "HVLV {0} {1}",
						shipment.TransportMode == TransportModes.Air ? AirCargo : SeaCargo,
						shipment.JobDirection == Directions.Import ? "ICR" : "CRE");
				case CountryCodes.Singapore:
					return Res.GetString("8d8caf28-b633-449b-b10a-740391c68a83", "SG ACCESS {0} Manifest",
						shipment.JobDirection == Directions.Import ? Import : Export);
				case CountryCodes.Taiwan:
					return Res.GetString("bbd892e5-6dac-4805-9a3e-efe982327a32", "TW Forwarder Manifest ({0})",
						shipment.JobDirection == Directions.Import ? Import : Export);
				default:
					return string.Empty;
			}
		}

		static string LowValueEntries => DataBoundResourceStrings.GetDataForTable(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(CusUSLVClearanceSchema.Constants.Prefix)).Caption;

		static string SeaCargo => Res.GetString("6b34ad5e-112e-41c7-82dd-37fe08600a68", "SeaCargo");

		static string AirCargo => Res.GetString("fe711bbc-4d65-4abb-9d9b-c1a7ea55bfd2", "AirCargo");

		static string Import => Res.GetString("e1904003-a324-4d86-9440-421a20c4f245", "Import");

		static string Export => Res.GetString("0c1b474d-f70a-4d62-ae5b-623493741fb4", "Export");

		#endregion

		#region Tab

		void UpdateTabVisibility()
		{
			if (TabPage.TabVisible != Shipment.IsHighVolumeLowValue)
			{
				TabPage.TabVisible = Shipment.IsHighVolumeLowValue;

				if (Form is ITabVisibilityDeciderPersistence formWithTabVisibilityPersistence)
				{
					formWithTabVisibilityPersistence.StoreTabVisible(TabPage);
				}
			}
		}

		#endregion

		#region HVLVConsignmentHeader

		public HVLVConsignmentHeader ConsignmentHeader => BusinessEntity as HVLVConsignmentHeader;

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			var consignmentHeader = default(HVLVConsignmentHeader);
			if (Shipment.IsHighVolumeLowValue)
			{
				consignmentHeader = Shipment.GetOrCreateHVLVConsignmentHeader();
				JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(consignmentHeader.Factory, consignmentHeader.PK);
			}

			return consignmentHeader;
		}

		#endregion
	}
}
