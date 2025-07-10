using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomsNumberViewStmNumsEditorForm : ZChildForm
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public CustomsNumberViewStmNumsEditorForm()
		{
			InitializeComponent();
		}

		public CustomsNumberViewStmNumsEditorForm(CustomsNumberViewStmNumsWrapper wrapper)
			: base(wrapper)
		{
			InitializeComponent();
			DialogResult = DialogResult.Cancel;
			Wrapper.StmNums.SN_TypeInfo.ValueChanged += SN_TypeInfo_ValueChanged;
		}

		#region Implementation

		#region Events

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			UpdateControlsVisibility();
		}

		void ValidateAndSaveButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void SN_TypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateControlsVisibility();
		}

		protected virtual void UpdateControlsVisibility()
		{
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (Wrapper != null)
			{
				Wrapper.StmNums.SN_TypeInfo.ValueChanged -= SN_TypeInfo_ValueChanged;
			}
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected CustomsNumberViewStmNumsWrapper Wrapper
		{
			get { return (CustomsNumberViewStmNumsWrapper)BusinessEntity; }
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (Wrapper?.IsInDatabase ?? false)
			{
				SetEditInitialFocus();
			}
		}
		internal protected virtual void SetEditInitialFocus()
		{
			MinimumValueCalcEdit.Focus();
		}

		#endregion
	}
}
