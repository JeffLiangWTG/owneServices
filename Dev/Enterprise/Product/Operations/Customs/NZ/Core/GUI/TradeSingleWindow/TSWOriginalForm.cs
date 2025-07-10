using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class TSWOriginalForm : ZChildForm
	{
		public TSWOriginalForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
			this.additionalMessageInformation = additionalMessageInformation;
		}

		public TSWOriginalForm()
			: base()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected readonly AdditionalMessageInformation additionalMessageInformation;

		public override string FormVerb
		{
			get { return Res.GetString("433AF21B-0FA7-4FE0-BB53-4554CCBBC166", "Trade Single Window - "); }
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
				DialogResult = DialogResult.OK;
			}
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
