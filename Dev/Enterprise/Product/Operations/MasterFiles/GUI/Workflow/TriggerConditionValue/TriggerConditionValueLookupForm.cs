using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TriggerConditionValueLookupForm : ZChildForm, IFindBoxPopup
	{
		IFindBox findBox;

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public TriggerConditionValueLookupForm()
		{
			InitializeComponent();
		}

		public TriggerConditionValueLookupForm(TriggerConditionValueParameters bo)
			: base(bo)
		{
			InitializeComponent();
		}

		#region ConfirmButton_Click

		void ConfirmButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.RunPreSaveValidation();

				if (BusinessEntity.HasErrors)
				{
					Globals.Message.ShowError(Res.GetString("b8c8f543-07f2-40ee-92be-cbd41d52bbb9", "Please fix all the errors before proceeding."));
					return;
				}

				int maxLength = ProcessTask.TriggerConditionValueMaxLength;

				if (BusinessEntity.CompleteText.Length > maxLength)
				{
					Globals.Message.ShowError(Res.GetString("83d374d7-163e-409e-9fd8-f18aa209fce2", "The trigger condition value generated cannot be more than {0} characters.", maxLength));
					return;
				}

				if (findBox != null)
				{
					findBox.Code = BusinessEntity != null ? BusinessEntity.CompleteText : ZString.Empty;
				}

				DialogResult = DialogResult.OK;
			}

			Close();
		}

		#endregion

		#region CloseButton_Click

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region IFindBoxPopup Members

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			this.findBox = findBox;
			ZFormModaliser.Show(this, parentForm);
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		public void SelectRowByPK(ZGuid pK)
		{
		}

		public new TriggerConditionValueParameters BusinessEntity
		{
			get { return base.BusinessEntity as TriggerConditionValueParameters; }
		}

		#endregion
	}
}
