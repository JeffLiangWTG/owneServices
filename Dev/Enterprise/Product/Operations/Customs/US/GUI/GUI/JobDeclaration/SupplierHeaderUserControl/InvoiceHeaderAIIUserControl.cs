using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class InvoiceHeaderAIIUserControl : ZUserControl
	{
		public InvoiceHeaderAIIUserControl()
		{
			InitializeComponent();
		}

		const string IsVisibleForBindingConst = "IsVisibleForBinding";

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			TermsOfDeliveryLocationTextBox.DataBindings.RemoveBinding(IsVisibleForBindingConst);
			TermsOfDeliveryLocationCountryCodeFindBox.DataBindings.RemoveBinding(IsVisibleForBindingConst);
			TermsOfDeliveryLocationScheduleDCodeFindBox.DataBindings.RemoveBinding(IsVisibleForBindingConst);
			US_TermsOfDeliveryLocationScheduleKCodeFindBox.DataBindings.RemoveBinding(IsVisibleForBindingConst);

			if (dataSource != null)
			{
				TermsOfDeliveryLocationTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingConst, BindingSource.DataSource, "FilteredInvoices.US_IsOtherTermsOfDeliveryLocation", false, DataSourceUpdateMode.Never));
				TermsOfDeliveryLocationCountryCodeFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingConst, BindingSource.DataSource, "FilteredInvoices.US_IsISOCountryCodeTermsOfDeliveryLocation", false, DataSourceUpdateMode.Never));
				TermsOfDeliveryLocationScheduleDCodeFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingConst, BindingSource.DataSource, "FilteredInvoices.US_IsScheduleDTermsOfDeliveryLocation", false, DataSourceUpdateMode.Never));
				US_TermsOfDeliveryLocationScheduleKCodeFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingConst, BindingSource.DataSource, "FilteredInvoices.US_IsScheduleKTermsOfDeliveryLocation", false, DataSourceUpdateMode.Never));
			}
		}

		public void ManageDeclarationRelatedControlsVisibility(bool isVisible)
		{
			DefaultRelatedDocumentsButton.Visible = isVisible;
			RelatedBillPanel.Visible = isVisible;

			if (!isVisible)
			{
				AIIRelatedDocumentsGrid.Dock = DockStyle.Fill;
			}
		}

		public new IInvoicesProvider CurrentDataItem
		{
			get { return (IInvoicesProvider)base.CurrentDataItem; }
		}

		void DefaultRelatedDocumentsButton_Click(object sender, EventArgs e)
		{
			JobComInvoiceHeader currentInvoice = GetCurrentlySelectedInvoice();

			if (currentInvoice == null)
			{
				Globals.Message.ShowInformation("Please select an invoice header.");
			}
			else
			{
				new RelatedDocumentsDataPopulator(currentInvoice).PopulateRelatedDocuments();
			}
		}

		JobComInvoiceHeader GetCurrentlySelectedInvoice()
		{
			BindingManagerBase bindingManager = GetBindingManager("FilteredInvoices");

			return bindingManager.Position == -1 ? null : (JobComInvoiceHeader)bindingManager.GetCurrent();
		}
	}
}
