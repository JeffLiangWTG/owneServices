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
	public partial class AMSEditForm : ZChildForm
	{
		public AMSEditForm()
		{
		}

		public AMSEditForm(AMS header)
			: base(header)
		{
		}

		public override string FormCaption
		{
			get
			{
				return Res.GetString("0A3E7B8F-D9A6-4E38-8B66-ED1F5BDAEBC7", "AMS Details");
			}
		}

		public override string FormVerb
		{
			get { return FormVerbs.Edit; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			US_ProgramDropEdit.ReadOnly = true;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
