using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class DocAddressPassportDetailForm : ZChildForm
	{
		public DocAddressPassportDetailForm(JobDocAddress docAddress)
			: base(docAddress)
		{
		}

		public new JobDocAddress BusinessEntity
		{
			get { return (JobDocAddress)base.BusinessEntity; }
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
