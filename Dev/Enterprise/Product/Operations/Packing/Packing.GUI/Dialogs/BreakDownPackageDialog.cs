using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.GUI
{
	public partial class BreakDownPackageDialog : ZChildForm
	{
		public BreakDownPackageDialog(IEnumerable<PkgPackage> selectedPackages)
			: base(new BreakDownPackage(selectedPackages))
		{
			InitializeComponent();
		}

		/// <summary>
		/// For the designer.
		/// </summary>
		public BreakDownPackageDialog()
		{
			InitializeComponent();
		}

		public new BreakDownPackage DataSource
		{
			get { return (BreakDownPackage)base.DataSource; }
		}

		public override string FormHeading
		{
			get { return BreakDownPackage.BreakDownPackageDescription; }
		}

		#region SetFocusToCalcEdit

		void BreakDownPackageDialog_Shown(object sender, EventArgs e)
		{
			SetFocusToCalcEdit();
		}

		void SetFocusToCalcEdit()
		{
			ActiveControl = splitQuantityCalcEdit;
		}

		#endregion

		#region OK Button

		void OkButton_Click(object sender, EventArgs e)
		{
			HandleOkButtonClick();
		}

		void HandleOkButtonClick()
		{
			if (!DataSource.SplitQuantityInfo.HasErrors())
			{
				this.DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		#endregion

		#region Cancel Button

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Defaulting Value to 1

		void SplitQuantityCalcEdit_TextChanged(object sender, EventArgs e)
		{
			SetDefaultValueTo1();
		}

		void SetDefaultValueTo1()
		{
			if (string.IsNullOrEmpty(splitQuantityCalcEdit.Text))
			{
				splitQuantityCalcEdit.Text = "1";
			}
		}

		#endregion
	}
}
