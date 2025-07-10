using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	partial class DocAddressSocialSecurityNumberDetailForm : ZChildForm
	{
		public DocAddressSocialSecurityNumberDetailForm(ISFDocAddress docAddress)
			: base(docAddress)
		{
		}

		public new ISFDocAddress BusinessEntity
		{
			get { return (ISFDocAddress)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}

