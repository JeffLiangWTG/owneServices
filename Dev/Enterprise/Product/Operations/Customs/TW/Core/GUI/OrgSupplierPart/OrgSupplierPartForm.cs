using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.GUI
{
	public partial class OrgSupplierPartForm : MasterFiles.GUI.OrgSupplierPartForm
	{
		public OrgSupplierPartForm() : base()
		{
		}

		public OrgSupplierPartForm(OrgSupplierPart part) : base(part)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
