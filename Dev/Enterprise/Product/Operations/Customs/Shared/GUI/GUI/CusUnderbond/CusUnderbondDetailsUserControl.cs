using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusUnderbondDetailsUserControl : ZUserControl
	{
		public CusUnderbondDetailsUserControl()
		{
			InitializeComponent();
			AddCusOutturnUserControl();
			MessagesTabPage.AdditionalText = Res.GetString("d611ef92-da31-4320-b4ca-26f10f201903", "Messages");
			OutturnTabPage.AdditionalText = Res.GetString("19c2d57f-5604-440f-8eb2-cefe8ae3a69f", "Outturn");
			IsMoveFromDischargeCheckBox.CheckedChanged += IsMoveFromDischargeCheckBox_CheckedChanged;

			CoveringLabel.AllowOverlap(NilOutturnButton);
			CoveringLabel.AllowOverlap(OutturnUserControl);
			NilOutturnButton.AllowOverlap(OutturnUserControl);
		}

		void AddCusOutturnUserControl()
		{
			this.OutturnUserControl = GetCusOutturnUserControl();
			this.OutturnUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutturnUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OutturnUserControl.Name = "OutturnUserControl";
			this.OutturnUserControl.ShouldSerializeTabPageMethods = false;
			this.OutturnUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.OutturnUserControl.TabIndex = 0;
			this.OutturnTabPage.Controls.Add(this.OutturnUserControl);
		}

		public bool OutturnDisabled
		{
			get { return fOutturnDisabled; }
			set
			{
				bool hasChanges = OutturnDisabled != value;
				fOutturnDisabled = value;
				if (hasChanges)
				{
					SetOutturnTabVisibility();
				}
			}
		}

		bool fOutturnDisabled;

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				IsMoveFromDischargeCheckBox.CheckedChanged -= IsMoveFromDischargeCheckBox_CheckedChanged;
				UnhookAllUnderbondsEvents();
				if (!MainTabControl.TabPages.Contains(OutturnTabPage))
				{
					OutturnTabPage.Dispose();
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		protected virtual CusOutturnUserControl GetCusOutturnUserControl()
		{
			return new CusOutturnUserControl();
		}

		#region Binding

		public virtual void SetBindPrepend(ZString bindPrepend)
		{
			OriginAddressControl.BindToAddress = bindPrepend + OriginAddressControl.BindToAddress;
			OriginAddressControl.BindToOrgList = bindPrepend + OriginAddressControl.BindToOrgList;
			DestinationAddressControl.BindToAddress = bindPrepend + DestinationAddressControl.BindToAddress;
			DestinationAddressControl.BindToOrgList = bindPrepend + DestinationAddressControl.BindToOrgList;
			DischargeAddressControl.BindToAddress = bindPrepend + DischargeAddressControl.BindToAddress;
			DischargeAddressControl.BindToOrgList = bindPrepend + DischargeAddressControl.BindToOrgList;
			OriginIDTextBox.BindTo = bindPrepend + OriginIDTextBox.BindTo;
			DestinationIDTextBox.BindTo = bindPrepend + DestinationIDTextBox.BindTo;
			DischargeIDTextBox.BindTo = bindPrepend + DischargeIDTextBox.BindTo;
			SendersReferenceTextBox.BindTo = bindPrepend + SendersReferenceTextBox.BindTo;
			VoyageTextBox.BindTo = bindPrepend + VoyageTextBox.BindTo;
			MovementModeDropEdit.BindTo = bindPrepend + MovementModeDropEdit.BindTo;
			MovementModeDropEdit.BindToList = bindPrepend + MovementModeDropEdit.BindToList;
			IsMoveFromDischargeCheckBox.BindTo = bindPrepend + IsMoveFromDischargeCheckBox.BindTo;
			UnderbondForDropEdit.BindTo = bindPrepend + UnderbondForDropEdit.BindTo;
			UnderbondForDropEdit.BindToList = bindPrepend + UnderbondForDropEdit.BindToList;
			VesselFindBox.BindTo = bindPrepend + VesselFindBox.BindTo;
			VesselFindBox.BindToList = bindPrepend + VesselFindBox.BindToList;
			RequestReasonDropEdit.BindTo = bindPrepend + RequestReasonDropEdit.BindTo;
			RequestReasonDropEdit.BindToList = bindPrepend + RequestReasonDropEdit.BindToList;
			MessageStatusTextBox.BindTo = bindPrepend + MessageStatusTextBox.BindTo;
			CustomsStatusTextBox.BindTo = bindPrepend + CustomsStatusTextBox.BindTo;
			FlightNoTextBox.BindTo = bindPrepend + FlightNoTextBox.BindTo;
			ResponsiblePartyTextBox.BindTo = bindPrepend + ResponsiblePartyTextBox.BindTo;
			ArrivalDateEdit.BindTo = bindPrepend + ArrivalDateEdit.BindTo;
			PiecesTextBox.BindTo = bindPrepend + PiecesTextBox.BindTo;
			TranshipmentCodeFindBox.BindTo = bindPrepend + TranshipmentCodeFindBox.BindTo;
			TranshipmentCodeFindBox.BindToList = bindPrepend + TranshipmentCodeFindBox.BindToList;
			MessageUserControl.SetBindPrepend(bindPrepend);
			OutturnUserControl.SetBindPrepend(bindPrepend);
		}

		ICusUnderbondUnionCollectionParent fDataSource;
		internal new ICusUnderbondUnionCollectionParent DataSource
		{
			get { return fDataSource; }
			set
			{
				if (fDataSource != value)
				{
					fDataSource = value;
				}
			}
		}

		void AllUnderbonds_ModeOfMovementChanged(object sender, EventArgs e)
		{
			ChangeVesselVoyageVisibility(CurrentUnderbond);
		}

		void AllUnderbonds_CanSendOutturnChanged(object sender, EventArgs e)
		{
			ChangeOutturnTabVisibility(CurrentUnderbond);
		}

		int fRowIndex;
		internal int RowIndex
		{
			get { return fRowIndex; }
			set { fRowIndex = value; }
		}

		public CusUnderbond CurrentUnderbond
		{
			get
			{
				CusUnderbond result = null;
				if (fCurrentUnderbond != null)
				{
					result = fCurrentUnderbond;
				}
				else
				{
					if (DataSource != null && RowIndex >= 0 && RowIndex < DataSource.AllUnderbonds.Count)
					{
						result = DataSource.AllUnderbonds[RowIndex];
					}
					else if (DataSource != null && DataSource.AllUnderbonds.Count > 0)
					{
						result = DataSource.AllUnderbonds[0];
					}
				}
				return result;
			}
			set
			{
				UnhookAllUnderbondsEvents();
				fCurrentUnderbond = value;
				HookAllUnderbondsEvents();
				ChangeOutturnTabVisibility(fCurrentUnderbond);
			}
		}

		CusUnderbond fCurrentUnderbond;

		void HookAllUnderbondsEvents()
		{
			if (fCurrentUnderbond != null)
			{
				fCurrentUnderbond.C4_ModeOfMovementInfo.ValueChanged += AllUnderbonds_ModeOfMovementChanged;
				fCurrentUnderbond.CanDoOutturnChanged += AllUnderbonds_CanSendOutturnChanged;
			}
		}

		void UnhookAllUnderbondsEvents()
		{
			if (fCurrentUnderbond != null)
			{
				fCurrentUnderbond.C4_ModeOfMovementInfo.ValueChanged -= AllUnderbonds_ModeOfMovementChanged;
				fCurrentUnderbond.CanDoOutturnChanged -= AllUnderbonds_CanSendOutturnChanged;
			}
		}

		#endregion

		#region Visibility

		internal void ChangeVisibility(CusUnderbond currentUnderbond)
		{
			ChangeOutturnTabVisibility(currentUnderbond);
			ChangeTranshipmentPortVisibility(currentUnderbond);
			ChangeVesselVoyageVisibility(currentUnderbond);
		}

		void ChangeTranshipmentPortVisibility(CusUnderbond currentUnderbond)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				bool result = currentUnderbond != null
						&& currentUnderbond.LinkedObject != null
						&& currentUnderbond.LinkedObject.UsesTranshipmentPortOnUnderbond
						&& currentUnderbond.TranshipmentPortVisible;

				TranshipmentLabel.Visible = result;
				TranshipmentCodeFindBox.Visible = result;
			}
		}

		internal void ChangePartShipmentVisibility(bool partShipVisible)
		{
			PartShipmentTabPage.TabVisible = partShipVisible;
		}

		void ChangeVesselVoyageVisibility(CusUnderbond currentUnderbond)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				bool voyageAndVesselDetailsVisible = false;
				if (currentUnderbond != null)
				{
					voyageAndVesselDetailsVisible = currentUnderbond.VoyageAndVesselDetailsVisible;
				}

				BySeaTabPage.TabVisible = voyageAndVesselDetailsVisible;
			}
		}

		void IsMoveFromDischargeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool isVisible = !((ZCheckBox)sender).Checked;
			DischargeAddressControl.Visible = isVisible;
			DischargeIDTextBox.Visible = isVisible;
			DischargeLabel.Visible = isVisible;
		}

		void ChangeOutturnTabVisibility(CusUnderbond currentUnderbond)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				bool outturnTabVisibile = false;
				bool seaItemsVisible = false;
				bool lastMessageDateSupported = false;
				if (!OutturnDisabled)
				{
					if (currentUnderbond != null)
					{
						outturnTabVisibile = currentUnderbond.CanDoOutturn;
						seaItemsVisible = currentUnderbond.IsUnderbondForSeaShipment;
						lastMessageDateSupported = currentUnderbond.IsLastMessageDateSupported;
					}
				}
				SetOutturnFieldsVisibility(outturnTabVisibile, seaItemsVisible, lastMessageDateSupported);
			}
		}

		void SetOutturnTabVisibility()
		{
			if (OutturnDisabled && MainTabControl.TabPages.Contains(OutturnTabPage))
			{
				lastOutturnTabPageIndex = MainTabControl.TabPages.IndexOf(OutturnTabPage);
				MainTabControl.TabPages.Remove(OutturnTabPage);
			}
			else if (!OutturnDisabled && !MainTabControl.TabPages.Contains(OutturnTabPage))
			{
				MainTabControl.TabPages.Insert(OutturnTabPage, lastOutturnTabPageIndex);
			}
		}
		int lastOutturnTabPageIndex;

		void SetOutturnFieldsVisibility(bool visible, bool seaItemsVisible, bool lastMessageDateSupported)
		{
			if (visible)
			{
				CoveringLabel.Visible = false;
				CoveringLabel.SendToBack();
				OutturnUserControl.SetSeaControlsVisibility(seaItemsVisible);
				OutturnUserControl.SetLastMessageDatelsVisibility(lastMessageDateSupported);
			}
			else
			{
				CoveringLabel.Visible = true;
				CoveringLabel.BringToFront();
			}
		}

		#endregion

		#region Nil Outturns

		internal void SetUnderbondParent(ICusUnderbondNilUnderbondPerformer parent)
		{
			UnderbondParent = parent;
			NilOutturnButton.Visible = parent != null;
		}

		internal ICusUnderbondNilUnderbondPerformer UnderbondParent;

		internal void NilOutturnButton_Click(object sender, EventArgs e)
		{
			if (CurrentUnderbond == null)
			{
				Globals.Message.ShowWarning(Res.GetString("bc0dd381-b25b-4af7-a9a0-fd03c39cc193", "There is no Underbond to outturn."));
			}
			else
			{
				ZString message = UnderbondParent.PerformNilUnderbond(CurrentUnderbond);
				if (!message.IsEmpty)
				{
					Globals.Message.ShowWarning(message);
				}
			}
		}

		#endregion
	}
}
