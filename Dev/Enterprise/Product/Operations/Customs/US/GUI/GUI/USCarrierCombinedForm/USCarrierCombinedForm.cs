using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USCarrierCombinedForm : ZTemplateForm
	{
		public USCarrierCombinedForm(USCarrierCombined carrier)
			: base(carrier)
		{
		}

		public new USCarrierCombined BusinessEntity
		{
			get { return (USCarrierCombined)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}
	}
}
