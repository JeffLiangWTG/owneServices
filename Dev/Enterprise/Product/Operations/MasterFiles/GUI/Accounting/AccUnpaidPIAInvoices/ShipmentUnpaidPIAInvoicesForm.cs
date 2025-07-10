using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ShipmentUnpaidPIAInvoicesForm : ZChildForm
	{
		public static void ShowForm(IRelatedJobNumber relatedJobNumber)
		{
			ZFormModaliser.ShowDialogAndDispose(new ShipmentUnpaidPIAInvoicesForm(relatedJobNumber));
		}

		#region Protected Constructors

		protected internal ShipmentUnpaidPIAInvoicesForm(IRelatedJobNumber relatedJobNumber)
			: base(new UnpaidPaymentInAdvanceTransactionFilter(relatedJobNumber))
		{
		}

		ShipmentUnpaidPIAInvoicesForm()
		{
			InitializeComponent();
			SetupControlParentsAndBinding();
		}

		void ShipmentUnpaidPIAInvoicesForm_Load(object sender, EventArgs e)
		{
		}

		#endregion

		#region Overrides

		public override string FormVerb
		{
			get { return String.Empty; }
		}

		#endregion

		#region Controls and Binding

		void SetupControlParentsAndBinding()
		{
			//this.InvoicesGrid.
		}

		#endregion
	}
}
