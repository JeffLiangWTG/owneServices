using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefMessagingBussCarrierInfoForm : ZTemplateForm, ICustomerServiceMenuSectionCodeOverridable
	{
		public RefMessagingBussCarrierInfoForm(RefMessagingBussCarrierInfo refMessagingBussCarrierInfo) : base(refMessagingBussCarrierInfo)
		{
			InitializeComponent();
		}

		public override string FormCaption => BusinessEntity?.HumanReadableName;

		public string SectionCode => ModuleTreeCustomerServiceMenuSectionList.Codes.ReferenceFiles;

		protected override bool AllowNew => false;

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;
	}
}
