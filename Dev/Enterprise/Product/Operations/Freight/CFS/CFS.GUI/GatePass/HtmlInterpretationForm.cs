using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class HtmlInterpretationForm : ZChildForm
	{
		public HtmlInterpretationForm()
		{
			InitializeComponent();
		}

		public HtmlInterpretationForm(ZString documentText)
			: this()
		{
			this.DocumentText = documentText;
		}

		public override string FormHeading
		{
			get
			{
				return Res.GetString("f103e633-65c6-423f-ab86-105f2a02cc09", "Customs Information");
			}
		}

		public string DocumentText
		{
			get
			{
				return this.HtmlInterpretationBox.FormattedDocumentText;
			}
			set
			{
				this.HtmlInterpretationBox.DocumentText = value;
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
