using System;
using System.Collections.Generic;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class USInBondBillsUserControl : ZUserControl
	{
		public USInBondBillsUserControl()
		{
			InitializeComponent();
			BillsTopSplitContainer.Panel2MinSize = 150;
			SetBillsGridGridID();
			ChangeVisibilityAndControlsCaption();
		}

		void BillsGrid_AfterBind(object sender, EventArgs e)
		{
			UpdateAMSHBRFields(US.Business.ZZCustomsFunctionality.IsAMSHBREffective && BusinessEntity != null && BusinessEntity.IsSea);
		}

		public void UpdateAMSHBRFields(bool shouldBeVisible)
		{
			if (shouldBeVisible)
			{
				BillsGrid.AddToAvailableColumns(CusInBondBill.Schema.B0_HouseBillIssuerCode);
				BillsGrid.AddToAvailableColumns(CusInBondBill.Schema.B0_HouseBillNumber);
				BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 332, true);
				BillsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 52, true);
			}
			else
			{
				BillsGrid.RemoveFromAvailableColumns(CusInBondBill.Schema.B0_HouseBillIssuerCode);
				BillsGrid.RemoveFromAvailableColumns(CusInBondBill.Schema.B0_HouseBillNumber);
				BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 304, true);
				BillsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 25, true);
			}
			B0_HouseBillIssuerCodeCodeFindBox.Visible = shouldBeVisible;
			B0_HouseBillNumberTextBox.Visible = shouldBeVisible;
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
			ChangeVisibilityAndControlsCaption();
		}

		void BH_FTZMoveInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibilityAndControlsCaption();
		}

		void SetBillsGridGridID()
		{
			var prefix = "NON";
			var businessEntity = BusinessEntity;
			if (businessEntity != null)
			{
				if (businessEntity.IsAir)
				{
					prefix = "AIR";
				}
				else if (businessEntity.BH_FTZMove)
				{
					prefix = "FTZ";
				}
			}
			BillsGrid.GridId = prefix + billsGridGridID;
			BillsGrid.CurrentColumnLayout = null;
		}
		const string billsGridGridID = "76926D34-0036-4EA4-A95E-9EE9D45D8D1D";

		void ChangeVisibilityAndControlsCaption()
		{
			if (BusinessEntity != null)
			{
				BillsGrid.SaveUserLayoutSettings();
				using (BillsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					SetBillsGridGridID();
					if (BusinessEntity.BH_FTZMove)
					{
						BillForeignShipperDocAddressControl.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("994ED718-3293-45AF-B8F4-6C423FBB782B", "Shipper");
						B0_MasterBillNumberTextBox.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("481CC6D2-6B60-41B4-AFD8-7EF44958B1DB", "Master Bill");
						BillsGrid.SetColumnCaption(CusInBondBill.Schema.B0_MasterBillNumber, Enterprise.Customs.US.InBond.GUI.Res.GetData("C0FA0A19-497D-425E-A5EF-B07DCA2C4BC9", "Master Bill").Caption);
						BillsGrid.SetColumnWidth(CusInBondBill.Schema.B0_MasterBillNumber, 160);

						BillsGrid.SetColumnCaption(CusInBondBill.Schema.B0_IssuerCode, Enterprise.Customs.US.InBond.GUI.Res.GetData("20B4336F-20E3-4FB7-85C3-C4CD5990E3C5", "Master Issuer/FIRMS Code").Caption);
						BillsGrid.SetColumnWidth(CusInBondBill.Schema.B0_IssuerCode, 125);

						BillsGrid.SetColumnCaption("ForeignShipper+E2_AddressOverride", Enterprise.Customs.US.InBond.GUI.Res.GetData("19E6D2D4-DA38-47CD-BB1C-C13243467461", "Shipper Override").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+OrganisationPK", Enterprise.Customs.US.InBond.GUI.Res.GetData("A48B3BA5-6123-4ACA-9DE2-0A45ABAA9655", "Shipper Organization").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_OA_Address", Enterprise.Customs.US.InBond.GUI.Res.GetData("0BE587D1-A420-4CB4-8C1F-8503AD771E61", "Shipper Address Selector").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_CompanyName", Enterprise.Customs.US.InBond.GUI.Res.GetData("249FDC5E-E300-4A09-846C-F36AE0F4BCC5", "Shipper Company Name").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Address1", Enterprise.Customs.US.InBond.GUI.Res.GetData("B24C8D50-D731-4ACF-8035-47932BBA41A4", "Shipper Address 1").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Address2", Enterprise.Customs.US.InBond.GUI.Res.GetData("965039A0-85AE-43A6-A275-F05FCFF24863", "Shipper Address 2").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_RN_NKCountryCode", Enterprise.Customs.US.InBond.GUI.Res.GetData("574F7866-78BD-40DB-A94C-BFBB813016ED", "Shipper Country Code").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_State", Enterprise.Customs.US.InBond.GUI.Res.GetData("F0431602-6825-467F-A6D9-9A431C60578F", "Shipper State").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_City", Enterprise.Customs.US.InBond.GUI.Res.GetData("3832F6F3-008F-483A-80BD-79A78E18949A", "Shipper City").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Postcode", Enterprise.Customs.US.InBond.GUI.Res.GetData("DA8D8E76-D6B3-4DCF-A034-6C668C462CEF", "Shipper Postcode").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Phone", Enterprise.Customs.US.InBond.GUI.Res.GetData("7722ABF1-83CF-42E8-9DC5-9D4B560FA369", "Shipper Telephone").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Fax", Enterprise.Customs.US.InBond.GUI.Res.GetData("B1FA433D-17EE-4289-8509-54D09FFC9CAB", "Shipper Fax").Caption);
					}
					else
					{
						BillForeignShipperDocAddressControl.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("USInBondBillsUserControl|55e293ac-41a5-4204-a073-03da06756962", "Foreign Shipper");
						B0_MasterBillNumberTextBox.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("4908D0FD-6440-4414-9842-19ACC1EF9A9D", "Master Bill");
						BillsGrid.SetColumnCaption(CusInBondBill.Schema.B0_MasterBillNumber, Enterprise.Customs.US.InBond.GUI.Res.GetData("870C20E3-5E1B-405D-8CEA-22980FC64CFF", "Master Bill").Caption);
						BillsGrid.SetColumnWidth(CusInBondBill.Schema.B0_MasterBillNumber, 100);

						BillsGrid.SetColumnCaption(CusInBondBill.Schema.B0_IssuerCode, Enterprise.Customs.US.InBond.GUI.Res.GetData("8574FC17-CF46-4D3D-B720-28BB3BCE4A30", "Master Issuer").Caption);
						BillsGrid.SetColumnWidth(CusInBondBill.Schema.B0_IssuerCode, 75);

						BillsGrid.SetColumnCaption("ForeignShipper+E2_AddressOverride", Enterprise.Customs.US.InBond.GUI.Res.GetData("E625E076-5891-49AB-9D3D-1D5BE7921AD5", "Foreign Shipper Override").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+OrganisationPK", Enterprise.Customs.US.InBond.GUI.Res.GetData("25DA7600-BA34-4104-92DA-2F4E428F3D21", "Foreign Shipper Organization").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_OA_Address", Enterprise.Customs.US.InBond.GUI.Res.GetData("70A88E9C-2EF1-427A-856C-EE2E6EF62E42", "Foreign Shipper Address Selector").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_CompanyName", Enterprise.Customs.US.InBond.GUI.Res.GetData("20C43C9B-9E7F-40DC-BF0E-42DB4C9AB607", "Foreign Shipper Company Name").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Address1", Enterprise.Customs.US.InBond.GUI.Res.GetData("3EA398F5-5815-4AB1-A369-7B9DB8610533", "Foreign Shipper Address 1").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Address2", Enterprise.Customs.US.InBond.GUI.Res.GetData("EBFD22DC-8B81-4B05-9C4E-16917519FC12", "Foreign Shipper Address 2").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_RN_NKCountryCode", Enterprise.Customs.US.InBond.GUI.Res.GetData("B8B83DD0-D0C5-4FA1-AC44-A7D8DC695743", "Foreign Shipper Country Code").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_State", Enterprise.Customs.US.InBond.GUI.Res.GetData("047D5E8C-E0A3-4A34-96C2-DC85E2D70EB7", "Foreign Shipper State").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_City", Enterprise.Customs.US.InBond.GUI.Res.GetData("36381482-C52A-4EF9-B44C-DA49388FBED2", "Foreign Shipper City").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Postcode", Enterprise.Customs.US.InBond.GUI.Res.GetData("F01A9183-4DDD-4FB8-A099-E19D54CF6FBC", "Foreign Shipper Postcode").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Phone", Enterprise.Customs.US.InBond.GUI.Res.GetData("A9DF5B5D-66F7-4769-B676-DD0BB7E161F0", "Foreign Shipper Telephone").Caption);
						BillsGrid.SetColumnCaption("ForeignShipper+E2_Fax", Enterprise.Customs.US.InBond.GUI.Res.GetData("C0BBF22D-065B-4D1C-A208-B178204AF37C", "Foreign Shipper Fax").Caption);

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
					columns.Add(CusInBondBill.Schema.B0_HouseBillIssuerCode);
					columns.Add(CusInBondBill.Schema.B0_HouseBillNumber);
					columns.Add(CusInBondBill.Schema.B0_ManifestQty);
					columns.Add(CusInBondBill.Schema.B0_ManifestUQ);
					columns.Add(CusInBondBill.Schema.B0_Weight);
					columns.Add(CusInBondBill.Schema.B0_WeightUQ);
					columns.Add(CusInBondBill.Schema.B0_PortOfLadingKCode);
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
