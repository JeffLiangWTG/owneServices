using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class AdditionalSupplementaryCodesForm : ZChildForm
	{
		public AdditionalSupplementaryCodesForm(ISupplementaryCodeSupporter supplementaryCodeSupporter)
		{
			SupplementaryCodeSupporter = Argument.NotNull(supplementaryCodeSupporter, nameof(supplementaryCodeSupporter));
			SupplementaryCodes = Argument.NotNull(supplementaryCodeSupporter.AdditionalSupplementaryCodes, nameof(ISupplementaryCodeSupporter.AdditionalSupplementaryCodes));
			InitializeComponent();
			SetFormDataBinding();
			SetSupplementaryCodeColumnStyleInfo();
		}

		internal readonly ISupplementaryCodeSupporter SupplementaryCodeSupporter;
		internal readonly ICusCodeDataCollection<BaseSupplementaryCode> SupplementaryCodes;

		public static void ShowDialog(ISupplementaryCodeSupporter supporter, Form parentForm = null)
		{
			ZFormModaliser.ShowDialogAndDispose(new AdditionalSupplementaryCodesForm(supporter) { Owner = parentForm });
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			oldItems = SupplementaryCodes.AsString;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			_ = closeButton.Focus();
			if (DialogResult == DialogResult.Cancel)
			{
				SupplementaryCodes.AsString = oldItems;
			}
			else
			{
				BusinessEntity.RunPreSaveValidation();
				if (SupplementaryCodes.SelectMany(code => code.NotificationsIncludingChildren.GetErrors()).Any())
				{
					Globals.Message.ShowError(Res.GetString("8F3073D4-B95A-4AA9-8585-66B6D2CC945A", "The form has errors. Please fix them before continuing."));
					e.Cancel = true;
				}
			}

			base.OnClosing(e);
		}

		void SetFormDataBinding()
		{
			SuspendLayout();
			SetDataBinding(SupplementaryCodes, "");
			ResumeLayout();
		}

		void SetSupplementaryCodeColumnStyleInfo()
		{
			ZMultiControlColumnStyleInfo supplementaryCode = new ()
			{
				CaptionResourceString = SupplementaryCodeSupporter.SupplementaryCodeCaption ?? Res.GetData("C64CBC67-01D2-4E24-9325-66F52A259DBC", "Supplementary Code"),
				ColumnName = BaseSupplementaryCode.Schema.CY_Code,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
				FieldTypeColumnName = "SupplementaryCodesFieldType"
			};

			additionalSupplementaryCodesGrid.ColumnStyles.Add(supplementaryCode);
		}

		#region Buttons

		void OnOKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		string oldItems;
	}
}


