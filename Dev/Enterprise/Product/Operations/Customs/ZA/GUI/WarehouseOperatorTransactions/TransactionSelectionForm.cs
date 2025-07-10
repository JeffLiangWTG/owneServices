using System;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class TransactionSelectionForm : ZChildForm
	{
		public TransactionSelectionForm(TransactionSelectionController businessObject) : base(businessObject)
		{
			InitializeComponent();
			Controller = businessObject;
		}

		protected readonly TransactionSelectionController Controller;

		public override string FormVerb => string.Empty;

		protected virtual bool IsConfirmedToProcess => true;

		void FormOkButton_Click(object sender, EventArgs e)
		{
			if (IsConfirmedToProcess)
			{
				Controller.ProcessSelectedRecords();
				Close();
			}
		}

		void TransactionSelectionForm_Resize(object sender, EventArgs e)
		{
			TransactionsDisplayGrid.Refresh();
		}
	}
}
