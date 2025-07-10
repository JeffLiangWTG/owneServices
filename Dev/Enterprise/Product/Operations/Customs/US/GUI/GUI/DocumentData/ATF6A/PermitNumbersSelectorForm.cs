using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class PermitNumbersSelectorForm : ZChildForm
	{
		public PermitNumbersSelectorForm(PermitNumbersToSelectFromForPrintingCollection businessObject)
			: base(businessObject)
		{
			InitializeComponent();
		}

		PermitNumbersToSelectFromForPrintingCollection PermitNumbersCollection
		{
			get { return (PermitNumbersToSelectFromForPrintingCollection)BusinessEntity; }
		}

		void PrintButton_Click(object sender, EventArgs e)
		{
			var selectedPermitNumbers = PermitNumbersCollection.SelectedPermitNumbers;
			if (selectedPermitNumbers.Count == 0)
			{
				Globals.Message.Show(Res.GetString("dba0266c-db57-417d-9aec-9c451ba86f7c", "Please select a permit number."));
			}
			else
			{
				DialogResult = DialogResult.Yes;
			}
		}

		void CancelPrintButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			foreach(PermitNumberToSelectFromForPrinting permitNumber in PermitNumbersCollection)
			{
				permitNumber.NeedPrint = true;
			}
		}

		void SelectNoneButton_Click(object sender, EventArgs e)
		{
			foreach (PermitNumberToSelectFromForPrinting permitNumber in PermitNumbersCollection)
			{
				permitNumber.NeedPrint = false;
			}
		}
	}
}
