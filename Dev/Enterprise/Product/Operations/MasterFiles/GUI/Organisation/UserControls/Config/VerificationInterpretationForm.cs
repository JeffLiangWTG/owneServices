using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class VerificationInterpretationForm : ZChildForm
	{
		public VerificationInterpretationForm()
		{
			InitializeComponent();
		}

		public VerificationInterpretationForm(ZString verifiedContext)
			: this()
		{
			this.DocumentText = verifiedContext;
		}

		public override string FormHeading => Res.GetString("cb209083-2b50-4ae9-8ade-d7791dfd7bd0", "Verified Information");

		public string DocumentText
		{
			get => HtmlInterpretationBox.DocumentText;
			set => HtmlInterpretationBox.DocumentText = value;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
