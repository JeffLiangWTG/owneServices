using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class AllocateNumberForm : ZChildForm
	{
		public AllocateNumberForm(AllocateNumber allocateNumber)
			: base(allocateNumber)
		{
			OKButton.CaptionResourceString = allocateNumber.FormCaption;
			DescriptionLabel.CaptionResourceString = allocateNumber.FormDescription;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			OKButton.Enabled = IsOKButtonEnabled;
		}

		public override string FormCaption => AllocateNumber.FormCaption.Caption;

		public override string FormVerb => string.Empty;

		void OKButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else if (AllocateNumber.IsPart4NumberEmpty)
			{
				Globals.Message.ShowError(AllocateNumber.Part4Caption);
			}
			else if (AllocateNumber.IsPart5NumberOutrangeWhenAllowOutrange)
			{
				var question = Res.GetString("EB03298A-1CF4-4D92-86E3-26EED8A61392", "The entered number is not within the defined entry number range. Do you want to continue?");
				var dialogResult = Globals.Message.Show(question, FormCaption, MessageBoxButtons.YesNo, DialogResult.No);
				if (dialogResult == DialogResult.Yes)
				{
					DialogResult = DialogResult.OK;
				}
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
		}

		AllocateNumber AllocateNumber => (AllocateNumber)BusinessEntity;

		bool IsOKButtonEnabled
		{
			get
			{
				var result = true;
				var part5TextBoxValue = Part5NumberTextBox.Text.Trim();
				var isPart5NumberEmpty = string.IsNullOrEmpty(part5TextBoxValue);
				if (AllocateNumber is JobDeclarationAllocateNumber && isPart5NumberEmpty)
				{
					result = false;
				}
				else if (AllocateNumber.EntryNumberGenerator is EntryNumberGeneratorCatA1 entryNumberGeneratorCatA1)
				{
					if (isPart5NumberEmpty)
					{
						result = false;
					}
					else if (entryNumberGeneratorCatA1.Sequenceformatter is TWSequenceformatterOnlyFirstCharCanHaveEnglishForA1 sequenceformatterWithFirstCharLimitationForA1)
					{
						result = sequenceformatterWithFirstCharLimitationForA1.FirstAlphabets.Contains(part5TextBoxValue[0]);
					}
				}
				return result;
			}
		}

		void Part5NumberTextBox_TextChanged(object sender, System.EventArgs e)
		{
			OKButton.Enabled = IsOKButtonEnabled;
		}
	}
}
