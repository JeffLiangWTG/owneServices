using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.GUI
{
	public partial class USAMSBillsUserControl : ZUserControl
	{
		public USAMSBillsUserControl()
		{
			InitializeComponent();
			gridOverrideDefaultValuesSupporter = new ZGridOverrideDefaultValuesSupporter(BillContainerCommoditiesGrid);
		}

		public CusInBondHeader BusinessEntity
		{
			get { return (CusInBondHeader)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				ChangeVisibility();
				// Should only be done onced
				var isNVOCCHeader = BusinessEntity.IsNVOCCHeader;
				FirmsPanel.Visible = !isNVOCCHeader;
				UnladingPanel.Visible = !isNVOCCHeader;
				if (isNVOCCHeader)
				{
					BillsGrid.RemoveFromAvailableColumns(CusInBondBill.Schema.B0_Firms, CusInBondBill.Schema.B0_MasterInBondIndicator,
						CusInBondBill.Schema.B0_RL_NKInBondPortOfDest, CusInBondBill.Schema.B0_InBondPortOfDestDCode, CusInBondBill.Schema.B0_DateOfDischarge);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (gridOverrideDefaultValuesSupporter != null)
				{
					gridOverrideDefaultValuesSupporter.Dispose();
					gridOverrideDefaultValuesSupporter = null;
				}
			}
			base.Dispose(disposing);
		}
		ZGridOverrideDefaultValuesSupporter gridOverrideDefaultValuesSupporter;

		void ChangeVisibility()
		{
			using (BillsGrid?.SuspendCancelOfNonEditedRowOnLeaving())
			{
				ISFTabPage.TabVisible = currentBill != null && currentBill.IsISFBill;
			}
		}

		void BillsGrid_AfterBind(object sender, EventArgs e)
		{
			BillsGrid.ListManager.PositionChanged += new EventHandler(BillsGridListManager_PositionChanged);
			BillsGridListManager_PositionChanged(null, null);
		}

		void BillsGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var listManager = BillsGrid.ListManager;
			if (listManager != null)
			{
				if (listManager.Count > 0)
				{
					var current = (CusInBondBill)listManager.GetCurrent();
					if (current != null)
					{
						var isDiff = (currentBill != current);
						if (isDiff)
						{
							UnHookB0_BillStatusInfo_ValueChanged(currentBill);
							currentBill = current;
							HookB0_BillStatusInfo_ValueChanged(currentBill);
							ChangeVisibility();
						}
					}
				}
				else
				{
					UnHookB0_BillStatusInfo_ValueChanged(currentBill);
					currentBill = null;
					ChangeVisibility();
				}
			}
		}
		CusInBondBill currentBill;

		void UnHookB0_BillStatusInfo_ValueChanged(CusInBondBill bill)
		{
			if (bill != null)
			{
				bill.B0_BillStatusInfo.ValueChanged -= B0_BillStatusInfo_ValueChanged;
				bill.OnPortOfLadingChangedEvent = null;
			}
		}

		void HookB0_BillStatusInfo_ValueChanged(CusInBondBill bill)
		{
			if (bill != null)
			{
				UnHookB0_BillStatusInfo_ValueChanged(bill);
				bill.B0_BillStatusInfo.ValueChanged += B0_BillStatusInfo_ValueChanged;
				bill.OnPortOfLadingChangedEvent += delegate(ZString oldPort, ZString newPort, ZDateTime arrivalTime)
				{
					var result = true;

					if (!oldPort.IsEmpty && oldPort != newPort && !arrivalTime.IsEmpty)
					{
						if (bill.Header != null && !bill.Header.PortArrivalDetails.HasPortCode(newPort))
						{
							result = Globals.Message.ShowConfirmation(Res.GetString("6d7a4e91-ed38-43b5-a01f-af23f3e928cb", "Actual Arrival Date will be cleared. Continue?"), Res.GetString("fb9ba2a4-9dee-41d9-9ebd-8f602e51fa60", "Confirm Reset"), "YES", MessageBoxIcon.Warning) == DialogResult.OK;
						}
					}

					return result;
				};
			}
		}

		void B0_BillStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibility();
		}
	}
}
