using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EDICodeMappingForm : ZTemplateForm
	{
		EDICodeMappingForm()
		{
			InitializeComponent();
		}

		public EDICodeMappingForm(OrgPatternMatchOverride orgPatternMatchOverride) : base(orgPatternMatchOverride)
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Res.GetString("1d61fff8-26a6-4e35-9c6c-afe731144cc7", "EDI Code Mapping Form"); }
		}

		protected override bool SupportsEDocs => false;
		protected override bool ShowNotesTab => false;
	}
}
