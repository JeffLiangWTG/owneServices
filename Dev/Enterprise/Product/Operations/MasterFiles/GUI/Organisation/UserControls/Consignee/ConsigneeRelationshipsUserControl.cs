using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsigneeRelationshipsUserControl : OrganisationContainerControl
	{
		public ConsigneeRelationshipsUserControl()
		{
			InitializeComponent();
		}

		public new OrgSupplierBuyerLink CurrentDataItem
		{
			get { return (OrgSupplierBuyerLink)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			UnHookOrgSupplierLinkCollectionEvents();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			HookOrgSupplierLinkCollectionEvents();
		}

		void HookOrgSupplierLinkCollectionEvents()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.OL_RN_NKImporterCountryInfo.ValueChanged += OL_RN_NKImporterCountryInfo_ValueChanged;
				OL_RN_NKImporterCountryInfo_ValueChanged(this, null);
			}
		}

		void UnHookOrgSupplierLinkCollectionEvents()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.OL_RN_NKImporterCountryInfo.ValueChanged -= OL_RN_NKImporterCountryInfo_ValueChanged;
			}
		}

		void OL_RN_NKImporterCountryInfo_ValueChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				var valuationBasisResourceString = CurrentDataItem.OL_RN_NKImporterCountry == Core.Constants.CountryCodes.Canada
					? Res.GetData("5732872c-6d90-471e-a071-b075ddd31081", "VFD Code", "Value for Duty Code", "")
					: Res.GetData("455d266a-f63c-414d-8c0c-558506f1f70a", "Val. Basis", "Valuation Basis", "");
				OL_ValuationBasisBoundDropEdit.CaptionResourceString = valuationBasisResourceString;
				OL_ValuationBasisBoundDropEdit.UpdateCaption();

				var columnName = OrgSupplierBuyerLink.Schema.OL_ValuationBasis;
				var column = OrgSupplierLinkBoundGrid.Columns[columnName];
				if (column == null)
				{
					OrgSupplierLinkBoundGrid.GetColumnStyle(columnName).CaptionResourceString = valuationBasisResourceString;
				}
				else
				{
					((ZDropEditColumnStyle)column.ColumnStyle).CaptionResourceString = valuationBasisResourceString;
					OrgSupplierLinkBoundGrid.Extensions.Get<CargoWise.Windows.UI.IAutomaticLabelExtension>().Refresh();
				}

				TransactionsRelatedDropEdit.Visible = CurrentDataItem.OL_RN_NKImporterCountry != Core.Constants.CountryCodes.Canada;
			}
		}
	}
}
