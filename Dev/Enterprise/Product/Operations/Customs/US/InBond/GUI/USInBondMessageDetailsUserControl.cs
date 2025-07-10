using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class USInBondMessageDetailsUserControl : ZUserControl
	{
		public USInBondMessageDetailsUserControl()
		{
			InitializeComponent();
			this.MoveDetailDropEdit.SetControlWidth(250);
			MessagesTopSplitContainer.Panel2MinSize = 150;
			MessagesBottomSplitContainer.Panel2MinSize = 150;
			movementHeadersMessagesGridID = this.MovementHeadersMessagesGrid.GridId;
		}
		readonly ZString movementHeadersMessagesGridID;

		public CusInBondHeader BusinessEntity
		{
			get { return (CusInBondHeader)base.CurrentDataItem; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			ColumnsAvailability();
			this.MovementHeadersMessagesGrid.GridId = movementHeadersMessagesGridID + BusinessEntity.BH_ImportTransportMode;
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (BusinessEntity != null)
			{
				BusinessEntity.BH_ImportTransportModeInfo.ValueChanged -= new EventHandler(BH_ImportTransportModeInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (BusinessEntity != null)
			{
				BusinessEntity.BH_ImportTransportModeInfo.ValueChanged += new EventHandler(BH_ImportTransportModeInfo_ValueChanged);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.BH_ImportTransportModeInfo.ValueChanged -= new EventHandler(BH_ImportTransportModeInfo_ValueChanged);
			}

			base.Dispose(disposing);
		}

		void BH_ImportTransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			var valueChangedEvent = e as ValueChangedEventArgs;
			if (valueChangedEvent != null)
			{
				this.MovementHeadersMessagesGrid.GridId = movementHeadersMessagesGridID + valueChangedEvent.OldValue;
				this.MovementHeadersMessagesGrid.SaveUserLayoutSettings();
			}
			ColumnsAvailability();
			if (valueChangedEvent != null)
			{
				this.MovementHeadersMessagesGrid.GridId = movementHeadersMessagesGridID + valueChangedEvent.NewValue;
				this.MovementHeadersMessagesGrid.LoadUserLayoutSettings();
			}
		}

		void ColumnsAvailability()
		{
			using (MovementHeadersMessagesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				MovementHeadersMessagesGrid.SetAllAvailability(false);
				foreach (KeyValuePair<string, bool> pair in GetAvailableColumns())
				{
					MovementHeadersMessagesGrid.SetAvailability(true, pair.Key);
					MovementHeadersMessagesGrid.SetColumnVisible(pair.Value, pair.Key);
				}
			}
		}

		IEnumerable<KeyValuePair<string, bool>> GetAvailableColumns()
		{
			Dictionary<string, bool> result = new Dictionary<string, bool>();
			if (BusinessEntity != null && BusinessEntity.IsAir)
			{
				result.Add(CusInBondMoveHeader.Schema.BM_CustomsStatus, true);
				result.Add(CusInBondMoveHeader.Schema.BM_MessageStatus, true);
				result.Add(CusInBondMoveHeader.Schema.MovementUniqueCode, true);
				result.Add(CusInBondMoveHeader.Schema.BM_InBondEntryType, true);
				result.Add(CusInBondMoveHeader.Schema.InBondCarrierOrgPK, true);
				result.Add(CusInBondMoveHeader.Schema.BM_OA_InBondCarrier, false);
				result.Add(CusInBondMoveHeader.Schema.BM_InBondCarrierID, true);
				result.Add(CusInBondMoveHeader.Schema.BM_InBondCarrierSCAC, true);
				result.Add(CusInBondMoveHeader.Schema.BM_DestinationPortCode, true);
				result.Add(CusInBondMoveHeader.Schema.BM_ForeignDestPortKCode, true);
				result.Add(CusInBondMoveHeader.Schema.BM_MonetaryValue, true);
				result.Add(CusInBondMoveHeader.Schema.BM_CustomsStatusDescription, false);
				result.Add(CusInBondMoveHeader.Schema.BM_MessageStatusDescription, false);
				result.Add(CusInBondMoveHeader.Schema.BM_ArrivalDate, false);
				result.Add(CusInBondMoveHeader.Schema.BM_ExportDate, false);
				result.Add(CusInBondMoveHeader.Schema.BM_ExportTransportMode, false);
			}
			else
			{
				result.Add(CusInBondMoveHeader.Schema.BM_CustomsStatus, true);
				result.Add(CusInBondMoveHeader.Schema.BM_MessageStatus, true);
				result.Add(CusInBondMoveHeader.Schema.MovementUniqueCode, true);
				result.Add(CusInBondMoveHeader.Schema.BM_InBondEntryType, true);
				result.Add(CusInBondMoveHeader.Schema.BM_BTAIndicator, true);
				result.Add(CusInBondMoveHeader.Schema.InBondCarrierOrgPK, true);
				result.Add(CusInBondMoveHeader.Schema.BM_OA_InBondCarrier, false);
				result.Add(CusInBondMoveHeader.Schema.BM_InBondCarrierID, true);
				result.Add(CusInBondMoveHeader.Schema.BM_InBondCarrierSCAC, true);
				result.Add(CusInBondMoveHeader.Schema.BM_DestinationPortCode, true);
				result.Add(CusInBondMoveHeader.Schema.BM_ForeignDestPortKCode, true);
				result.Add(CusInBondMoveHeader.Schema.BM_MonetaryValue, true);
				result.Add(CusInBondMoveHeader.Schema.BM_CustomsStatusDescription, false);
				result.Add(CusInBondMoveHeader.Schema.BM_MessageStatusDescription, false);
				result.Add(CusInBondMoveHeader.Schema.BM_ArrivalDate, false);
				result.Add(CusInBondMoveHeader.Schema.BM_ExportDate, false);
				result.Add(CusInBondMoveHeader.Schema.BM_ExportTransportMode, false);
				result.Add(CusInBondMoveHeader.Schema.BM_ExportLadenOn, false);
				result.Add(CusInBondMoveHeader.Schema.BM_TOLDate, false);
				result.Add(CusInBondMoveHeader.Schema.TOLCarrierOrgPK, false);
				result.Add(CusInBondMoveHeader.Schema.BM_OA_TOLCarrier, false);
				result.Add(CusInBondMoveHeader.Schema.BM_TOLCarrierCode, false);
				result.Add(CusInBondMoveHeader.Schema.BM_TOLCarrierID, false);
				result.Add(CusInBondMoveHeader.Schema.BM_TOLCityName, false);
				result.Add(CusInBondMoveHeader.Schema.BM_TOLStateCode, false);
			}
			return result;
		}

		void HeadersMessagesTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			CusInBondMoveHeaderMessageDetailsTextBox.Text = "";
			CusInBondMoveHeaderMessageTextTextBox.Text = "";

			if (this.HeadersMessagesTabControl.SelectedTab == BillsofLadingTabPage)
			{
				this.BindingSource.SetBindingMember(this.CusInBondMoveHeaderMessagesGrid, "BillsOfLadingMessages.Messages");
				this.BindingSource.SetBindingMember(this.CusInBondMoveHeaderMessageDetailsTextBox, "BillsOfLadingMessages.Messages.EM_MessageInterpretation");
				this.BindingSource.SetBindingMember(this.CusInBondMoveHeaderMessageTextTextBox, "BillsOfLadingMessages.Messages.EM_FormattedMessageText");
			}
			else if (this.HeadersMessagesTabControl.SelectedTab == MovementHeadersTabPage)
			{
				this.BindingSource.SetBindingMember(this.CusInBondMoveHeaderMessagesGrid, "MovementHeadersMessages.Messages");
				this.BindingSource.SetBindingMember(this.CusInBondMoveHeaderMessageDetailsTextBox, "MovementHeadersMessages.Messages.EM_MessageInterpretation");
				this.BindingSource.SetBindingMember(this.CusInBondMoveHeaderMessageTextTextBox, "MovementHeadersMessages.Messages.EM_FormattedMessageText");
			}
			else if (this.HeadersMessagesTabControl.SelectedTab == ContainersTabPage)
			{
				this.BindingSource.SetBindingMember(this.CusInBondMoveHeaderMessagesGrid, "SelectedMovementDetails.Containers.Messages");
				this.BindingSource.SetBindingMember(this.CusInBondMoveHeaderMessageDetailsTextBox, "SelectedMovementDetails.Containers.Messages.EM_MessageInterpretation");
				this.BindingSource.SetBindingMember(this.CusInBondMoveHeaderMessageTextTextBox, "SelectedMovementDetails.Containers.Messages.EM_FormattedMessageText");
			}
		}
	}
}
