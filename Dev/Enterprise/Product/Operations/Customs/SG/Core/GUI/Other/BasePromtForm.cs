using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	[SuppressBindingMemberBashingTest] // to suppress zCheckShowMessage and DeclarationCheckBox
	public partial class BasePromtForm : ZChildForm
	{
		public BasePromtForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		protected BasePromtForm()
			: base()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				DeclarationCheckBox.Select();
				if (GlbStaff.CurrentUser.GS_IsDeveloper)
				{
					zCheckShowMessage.Visible = true;
				}
				else
				{
					zCheckShowMessage.CheckState = CheckState.Unchecked;
					zCheckShowMessage.Visible = false;
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			OKButton.Enabled = DeclarationCheckBox.Checked;
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void DeclarationCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			OKButton.Enabled = DeclarationCheckBox.Checked;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				(BusinessEntity as AdditionalMessageInformation).OnMessageCreated += new MessageEventHandler(additionalMessageInformation_OnMessageCreated);
				DialogResult = DialogResult.OK;
			}
		}

		void additionalMessageInformation_OnMessageCreated(MessageEventArgs args)
		{
			if (zCheckShowMessage.CheckState == CheckState.Checked)
			{
				using (var editForm = new MessageEditForm())
				{
					args.MessageText = editForm.EditMessage(args.MessageText);
				}
			}
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
