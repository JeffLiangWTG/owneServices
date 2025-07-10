using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class FWSEditForm : ZChildForm
	{
		public FWSEditForm()
		{
		}

		public FWSEditForm(FWSHeader header)
			: base(header)
		{
		}

		public override string FormCaption
		{
			get
			{
				return Res.GetString("31C4BEF2-CA76-43B8-8266-6873CADB44C8", "FWS Details");
			}
		}

		public override string FormVerb
		{
			get { return FormVerbs.Edit; }
		}

		FWSHeader Header
		{
			get { return (FWSHeader)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			DocumentsGroupBox.Visible = !Header.US_ConfirmationNum.IsEmpty;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
