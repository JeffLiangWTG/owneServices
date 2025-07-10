using System;
using System.Collections.Generic;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class USAirInBondBillsAndMovementsDetailsUserControl : ZUserControl
	{
		public USAirInBondBillsAndMovementsDetailsUserControl()
		{
			InitializeComponent();
			BillsMovementDetailsSplitContainer.Panel1MinSize = 150;
			BillsMovementDetailsSplitContainer.Panel2MinSize = 150;
			ChangeVisibility();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (BusinessEntity != null)
			{
				BusinessEntity.BH_FTZMoveInfo.ValueChanged -= new EventHandler(BH_FTZMoveInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (BusinessEntity != null)
			{
				BusinessEntity.BH_FTZMoveInfo.ValueChanged += new EventHandler(BH_FTZMoveInfo_ValueChanged);
			}
			ChangeVisibility();
		}

		void BH_FTZMoveInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibility();
		}

		void ChangeVisibility()
		{
			if (BusinessEntity != null)
			{
				BillsGrid.SaveUserLayoutSettings();
				using (BillsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					if (BusinessEntity.BH_FTZMove)
					{
						BillsGrid.RemoveFromAvailableColumns(CusInBondBill.Schema.B0_IssuerCode);
					}
					else
					{
						BillsGrid.AddToAvailableColumns(CusInBondBill.Schema.B0_IssuerCode);
						BillsGrid.ReOrderColumns(ColumnNamesInSortOrder);
					}
				}
			}
		}

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columns = new List<string>();
					columns.Add(CusInBondBill.Schema.B0_IssuerCode);
					columns.Add(CusInBondBill.Schema.B0_MasterBillNumber);
					columns.Add(CusInBondBill.Schema.B0_HouseBillNumber);
					columns.Add("Consignee+OrganisationPK");
					columns.Add("Consignee+E2_OA_Address");
					columnNamesInSortOrder = columns.ToArray();
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;

		public CusInBondHeader BusinessEntity
		{
			get { return (CusInBondHeader)base.CurrentDataItem; }
		}
	}
}
