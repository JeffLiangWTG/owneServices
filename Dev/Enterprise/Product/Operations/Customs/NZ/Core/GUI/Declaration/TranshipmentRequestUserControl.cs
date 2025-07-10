using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class TranshipmentRequestUserControl : ZUserControl
	{
		public TranshipmentRequestUserControl()
		{
			InitializeComponent();
			ShowCoveringLabel(string.Empty);
			transhipmentRequestMutexes = new Dictionary<ZGuid, ZGlobalMutex>();
			VesselCodeFindBox.PopupSelected += OnVesselSelected;
		}

		protected TranshipmentRequest TranshipmentRequestBO
		{
			get
			{
				return fTranshipmentRequest;
			}
			set
			{
				var oldValue = TranshipmentRequestBO;
				if (oldValue != value)
				{
					if (oldValue != null)
					{
						UnHook(oldValue);
					}
					fTranshipmentRequest = value;
					if (value != null)
					{
						Hook(fTranshipmentRequest);
						if (!fTranshipmentRequest.HasChanges && !fTranshipmentRequest.IsInDatabase)
						{
							fTranshipmentRequest.SuspendValidation();
							fTranshipmentRequest.HasChangesChanged += HasChangesChanged_ResumeValidation;
						}
					}
				}
			}
		}
		TranshipmentRequest fTranshipmentRequest;

		void Hook(TranshipmentRequest transhipmentRequest)
		{
			UnHook(transhipmentRequest);
			transhipmentRequest.C4_MovementReasonInfo.ValueChanged += C4_MovementReasonInfo_ValueChanged;
			transhipmentRequest.C4_TranshipModeOfMovementInfo.ValueChanged += C4_TranshipModeOfMovementInfo_ValueChanged;
		}

		void UnHook(TranshipmentRequest transhipmentRequest)
		{
			transhipmentRequest.C4_MovementReasonInfo.ValueChanged -= C4_MovementReasonInfo_ValueChanged;
			transhipmentRequest.C4_TranshipModeOfMovementInfo.ValueChanged -= C4_TranshipModeOfMovementInfo_ValueChanged;
			transhipmentRequest.HasChangesChanged -= HasChangesChanged_ResumeValidation;
		}

		void HookParent(ITranshipmentRequestParent transhipmentRequestParent)
		{
			transhipmentRequestParent.MessageSubTypeChanged += MessageSubTypeInfo_ValueChanged;
			transhipmentRequestParent.MessageTypeChanged += MessageTypeInfo_ValueChanged;
			transhipmentRequestParent.TransportModeChanged += TransportModeInfo_ValueChanged;
		}

		void UnHookParent(ITranshipmentRequestParent transhipmentRequestParent)
		{
			transhipmentRequestParent.MessageSubTypeChanged -= MessageSubTypeInfo_ValueChanged;
			transhipmentRequestParent.MessageTypeChanged -= MessageTypeInfo_ValueChanged;
			transhipmentRequestParent.TransportModeChanged -= TransportModeInfo_ValueChanged;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null && dataSource is ITranshipmentRequestParent)
			{
				parent = dataSource as ITranshipmentRequestParent;
				if (parent != null)
				{
					UnHookParent(parent);
					HookParent(parent);
				}
				base.SetDataBinding(dataSource, dataMember);
			}
			else if (parent != null)
			{
				base.SetDataBinding(parent, "");
			}
		}

		public void RefreshBinding()
		{
			if (parent != null)
			{
				TranshipmentRequestBO = parent.TranshipmentRequest;
				if (TranshipmentRequestBO == null)
				{
					var parentHAWB = DataSource as CusHAWB;
					if (parentHAWB != null)
					{
						parentHAWB.ReLoadTranshipmentFromDB();
						TranshipmentRequestBO = parent.TranshipmentRequest;
					}
					else
					{
						var parentSCAHouse = DataSource as CusSCAHouse;
						if (parentSCAHouse != null)
						{
							parentSCAHouse.ReLoadTranshipmentFromDB();
							TranshipmentRequestBO = parent.TranshipmentRequest;
						}
					}

					if (parent is not NonPersistentBusinessObject && !parent.IsInDatabase)
					{
						ShowCoveringLabel(CreateAndSaveConsignmentFirst);
					}
					else
					{
						if (TranshipmentRequestBO == null)
						{
							if (Mutex != null && (!Mutex.IsLocked || Mutex.HasLock))
							{
								if (CreateTranshipmentRequest())
								{
									ShowTranshipmentRequest();
								}
								else
								{
									ShowCoveringLabel(MutexLockText);
								}
							}
							else
							{
								ShowCoveringLabel(MutexLockText);
							}
						}
						else
						{
							ShowTranshipmentRequest();
						}
					}
				}
				else
				{
					ShowTranshipmentRequest();
				}
			}
		}

		bool CreateTranshipmentRequest()
		{
			var result = Mutex.Lock();
			if (result)
			{
				parent.Factory.Saved -= Factory_Saved;
				parent.Factory.Saved += Factory_Saved;
				_ = TranshipmentRequest.Create(parent, savesOnChanges: true);
				TranshipmentRequestBO = parent.TranshipmentRequest;
			}
			return result;
		}

		void ShowTranshipmentRequest()
		{
			MainPanel.SuspendLayout();
			if (TranshipmentRequestBO != null)
			{
				TranshipmentRequestBO.RefreshBindingIncludingChildren();
				MessageTypeInfo_ValueChanged(null, null);
			}
		}

		void SetTranshipmentRequestVisibility()
		{
			CoveringLabel.Visible = false;
			MainPanel.Visible = true;
			MainPanel.ResumeLayout();
			MainPanel.PerformLayout();
			if (parent != null && parent.IsTSWICRWriteOff)
			{
				InOutTransportModeDropEdit.CaptionResourceString = Res.GetData("19BF75E9-6014-4D62-8156-0B4C87AD69C9", "Outgoing Transport Mode");
				InOutCraftDetailsGroupBox.CaptionResourceString = Res.GetData("1C5D2487-231E-4B1B-9961-E8DA99D7EA3A", "Outgoing Craft Details");
				TransitDestinationGroupBox.Visible = true;
			}
			else
			{
				InOutTransportModeDropEdit.CaptionResourceString = Res.GetData("AEA299AC-7FC9-483D-8A85-C69491AED633", "Incoming Transport Mode");
				InOutCraftDetailsGroupBox.CaptionResourceString = Res.GetData("10DBDD30-7A2A-4B1B-964E-E3717C8EBC80", "Incoming Craft Details");
				TransitDestinationGroupBox.Visible = false;
			}
		}

		void ShowCoveringLabel(string text)
		{
			CoveringLabel.Text = text;
			CoveringLabel.Dock = DockStyle.Fill;
			CoveringLabel.Visible = true;
			MainPanel.Visible = false;
		}

		static string CreateAndSaveConsignmentFirst => Res.GetString(
			"BA2FB958-6B7E-4468-A26E-2702F9061028",
			"Please create and save this consignment before creating the Transhipment Request.\r\nChange to the Details tab, create and save the consignment information, then click back on this tab to create the Transhipment Request.");

		public static string MutexLockText =>
			Res.GetString("TranshipmentRequestUserControl|MutexLockText", "Someone else is already in the process of creating a Transhipment Request for this job.\r\nYou should be able to access the Transhipment Request when the person has saved or canceled. Please try later.");

		#region Mutex

		ZGlobalMutex Mutex
		{
			get
			{
				if (parent != null)
				{
					fMutex = GetParentMutex(parent.PK);
					if (fMutex == null)
					{
						fMutex = new ZGlobalMutex(Enterprise.ZArchitecture.Modules.MutexIDs.TranshipmentRequest, parent.PK.ToString());
						transhipmentRequestMutexes.Add(parent.PK, fMutex);
					}
				}

				return fMutex;
			}
		}
		ZGlobalMutex fMutex;

		ZGlobalMutex GetParentMutex(ZGuid parentPK)
		{
			return transhipmentRequestMutexes.ContainsKey(parentPK) ? transhipmentRequestMutexes[parentPK] : null;
		}

		readonly Dictionary<ZGuid, ZGlobalMutex> transhipmentRequestMutexes;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				UnlockMutexIfLockedByThisInstance();
			}
		}

		void UnlockMutexIfLockedByThisInstance()
		{
			foreach (KeyValuePair<ZGuid, ZGlobalMutex> mutex in transhipmentRequestMutexes)
			{
				if (mutex.Value != null && mutex.Value.HasLock)
				{
					mutex.Value.Unlock();
				}
			}

			if (fMutex != null)
			{
				((IDisposable)fMutex).Dispose();
				fMutex = null;
			}
		}

		#endregion

		void OnVesselSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var bizos = e.SelectedBusinessObjects;
			if (bizos.Length == 1 && bizos[0] is RefVessel vessel)
			{
				TranshipmentRequestBO.SetTranshipBySeaVessel(vessel);
			}
		}

		void MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (TranshipmentRequestBO != null)
			{
				SetTranshipmentRequestVisibility();
				C4_MovementReasonInfo_ValueChanged(null, null);
				C4_TranshipModeOfMovementInfo_ValueChanged(null, null);
			}
		}

		void MessageSubTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			MessageTypeInfo_ValueChanged(null, null);
		}

		void TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (TranshipmentRequestBO != null)
			{
				TransitDestinationPortFindBox.Visible = TranshipmentRequestBO.IsSeaJob;
			}
		}

		void C4_MovementReasonInfo_ValueChanged(object sender, EventArgs e)
		{
			var isDTR = TranshipmentRequestBO.IsDTR;
			bool isValidMovementRequest = !TranshipmentRequestBO.C4_TranshipModeOfMovement.IsEmpty;
			InOutTransportModeDropEdit.Visible = !isDTR;
			InOutCraftDetailsGroupBox.Visible = !isDTR && isValidMovementRequest;
			SetTransitDestGroupBoxLocation();
		}

		void HasChangesChanged_ResumeValidation(object sender, EventArgs e)
		{
			if (TranshipmentRequestBO.HasChanges)
			{
				TranshipmentRequestBO.ResumeValidation();
				TranshipmentRequestBO.HasChangesChanged -= HasChangesChanged_ResumeValidation;
			}
		}

		void SetTransitDestGroupBoxLocation()
		{
			var isICR = parent != null && parent.IsTSWICRWriteOff;
			var mode = TranshipmentRequestBO.C4_TranshipModeOfMovement;
			var isAir = TranshipmentRequestBO.IsITR && TranshipmentRequestModeOfMovement.IsAir(mode);
			var isSea = TranshipmentRequestBO.IsITR && TranshipmentRequestModeOfMovement.IsSea(mode);
			if (isSea || (isAir && !isICR))
			{
				if (parent != null && (parent.TablePrefix == CusHAWBSchema.Constants.Prefix || parent.TablePrefix == CusSCAHouseSchema.Constants.Prefix))
				{
					TransitDestinationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 76, true);
				}
				else
				{
					TransitDestinationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 209, true);
				}
			}
			else
			{
				TransitDestinationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 76, true);
			}

			TransitDestinationGroupBox.Visible = isICR;
			TransitDestinationGroupBox.Refresh();
		}

		void C4_TranshipModeOfMovementInfo_ValueChanged(object sender, EventArgs e)
		{
			var isICR = parent != null && parent.IsTSWICRWriteOff;
			var mode = TranshipmentRequestBO.C4_TranshipModeOfMovement;
			var isAir = TranshipmentRequestBO.IsITR && TranshipmentRequestModeOfMovement.IsAir(mode);
			var isSea = TranshipmentRequestBO.IsITR && TranshipmentRequestModeOfMovement.IsSea(mode);
			InOutCraftDetailsGroupBox.Visible = isSea || (isAir && !isICR);

			VesselCodeFindBox.Visible = isSea;
			LloydsIMOTextBox.Visible = isSea;
			VoyageTextBox.Visible = isSea;
			FlightTextBox.Visible = isAir;
			TransportModeInfo_ValueChanged(null, null);

			if (isAir)
			{
				ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 49, true);
				DepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 49, true);
				VesselAndFlightLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 26, true);
			}
			else
			{
				ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 93, true);
				DepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 93, true);
				VesselAndFlightLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 73, true);
			}

			SetTransitDestGroupBoxLocation();

			if (InOutCraftDetailsGroupBox.Visible)
			{
				bool isValidMovementRequest = !mode.IsEmpty;
				VesselAndFlightLinkLabel.Visible = isValidMovementRequest;
				bool needsIncomingCraftDetails = InOutCraftDetailsGroupBox.CaptionResourceString.Caption == "Incoming Craft Details";
				ArrivalDateEdit.Visible = isValidMovementRequest && needsIncomingCraftDetails;
				DepartureDateEdit.Visible = isValidMovementRequest && isICR && !needsIncomingCraftDetails;
			}

			InOutCraftDetailsGroupBox.Refresh();
		}

		void VesselAndFlightLinkLabel_Click(object sender, EventArgs e)
		{
			WebUrlLauncher.Launch(NZEDIMenu.UrlCustomsFindVesselOrFlight);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				VesselCodeFindBox.PopupSelected -= OnVesselSelected;

				components?.Dispose();

				if (TranshipmentRequestBO != null)
				{
					UnHook(TranshipmentRequestBO);
				}
				if (parent != null)
				{
					UnHookParent(parent);
				}

				UnlockMutexIfLockedByThisInstance();
			}

			base.Dispose(disposing);
		}

		ITranshipmentRequestParent parent;
	}
}
