using System;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class USChildInBondCommodityForm : ZChildForm
	{
		public USChildInBondCommodityForm()
		{
			InitializeComponent();
		}

		public USChildInBondCommodityForm(CusInBondCargoDesc commodity)
			: base(commodity)
		{
			InitializeComponent();
			var hasPartDetail = !CurrentDataItem.BY_PartNumber.IsEmpty;
			ClassificationGrid.GridId = ClassificationGrid.GridId + (hasPartDetail ? "PART" : "NON-PART");
			using (ClassificationGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				if (!hasPartDetail)
				{
					AddPartAttribColumns();
				}
				var header = commodity == null ? null : commodity.Header;
				var shouldShowWarehouseData = header != null;
				ClassificationGrid.SetAvailability(!hasPartDetail && shouldShowWarehouseData, [CusInBondCargoDesc.Schema.BY_InvoiceQuantity, CusInBondCargoDesc.Schema.BY_WarehouseEntryNumber, CusInBondCargoDesc.Schema.BY_WarehouseEntryLineNo]);
				ClassificationGrid.SetAvailability(!hasPartDetail, [CusInBondCargoDesc.Schema.BY_OH_Supplier, CusInBondCargoDesc.Schema.BY_PartNumberForBinding]);
			}
			CommodityRelationShipSplitContainer.Panel2Collapsed = hasPartDetail;
		}

		public new CusInBondCargoDesc CurrentDataItem
		{
			get { return (CusInBondCargoDesc)base.CurrentDataItem; }
		}

		public override string FormCaption
		{
			get { return "Commodities"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetPartAttributeCaptions();
		}

		void SetPartAttributeCaptions()
		{
			if (CurrentDataItem != null && CurrentDataItem.BY_PartNumber.IsEmpty)
			{
				var importer = CurrentDataItem.Importer;
				if (importer != null)
				{
					ClassificationGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib1, importer.PartAttributeManager.PartAttributeName1);
					ClassificationGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib2, importer.PartAttributeManager.PartAttributeName2);
					ClassificationGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib3, importer.PartAttributeManager.PartAttributeName3);
				}
				else
				{
					ClassificationGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib1, "Part Attrib. 1");
					ClassificationGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib2, "Part Attrib. 2");
					ClassificationGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib3, "Part Attrib. 3");
				}

				ClassificationGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_SerialNumber, "Serial Number");
			}
		}

		void AddPartAttribColumns()
		{
			var partAttrib1TextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib1TextBoxColumnStyleInfo.ColumnName = CusInBondCargoDesc.Schema.BY_PartAttrib1;
			partAttrib1TextBoxColumnStyleInfo.IsVisible = false;
			partAttrib1TextBoxColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;

			var partAttrib2TextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib2TextBoxColumnStyleInfo.ColumnName = CusInBondCargoDesc.Schema.BY_PartAttrib2;
			partAttrib2TextBoxColumnStyleInfo.IsVisible = false;
			partAttrib2TextBoxColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;

			var partAttrib3TextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib3TextBoxColumnStyleInfo.ColumnName = CusInBondCargoDesc.Schema.BY_PartAttrib3;
			partAttrib3TextBoxColumnStyleInfo.IsVisible = false;
			partAttrib3TextBoxColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;

			var partNumberColumnStyle = ClassificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartNumberForBinding);
			var partNumberColumnStyleIndex = ClassificationGrid.ColumnStyles.IndexOf(partNumberColumnStyle);

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusInBondCargoDesc)((System.Collections.IList)((CusInBondCargoDesc)null).ChildCommodities).SyncRoot).BY_PartAttrib1);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusInBondCargoDesc)((System.Collections.IList)((CusInBondCargoDesc)null).ChildCommodities).SyncRoot).BY_PartAttrib2);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusInBondCargoDesc)((System.Collections.IList)((CusInBondCargoDesc)null).ChildCommodities).SyncRoot).BY_PartAttrib3);
			ClassificationGrid.ColumnStyles.Insert(++partNumberColumnStyleIndex, partAttrib1TextBoxColumnStyleInfo);
			ClassificationGrid.ColumnStyles.Insert(++partNumberColumnStyleIndex, partAttrib2TextBoxColumnStyleInfo);
			ClassificationGrid.ColumnStyles.Insert(++partNumberColumnStyleIndex, partAttrib3TextBoxColumnStyleInfo);

			var serialNumberTextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
			serialNumberTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			serialNumberTextBoxColumnStyleInfo.ColumnName = CusInBondCargoDesc.Schema.BY_SerialNumber;
			serialNumberTextBoxColumnStyleInfo.IsVisible = false;
			serialNumberTextBoxColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusInBondCargoDesc)((System.Collections.IList)((CusInBondCargoDesc)null).ChildCommodities).SyncRoot).BY_SerialNumber);
			ClassificationGrid.ColumnStyles.Insert(++partNumberColumnStyleIndex, serialNumberTextBoxColumnStyleInfo);
		}
	}
}
