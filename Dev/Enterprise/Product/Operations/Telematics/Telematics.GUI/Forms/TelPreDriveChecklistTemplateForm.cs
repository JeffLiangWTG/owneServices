using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Telematics.GUI.Forms
{
	public partial class TelPreDriveChecklistTemplateForm : ZTemplateForm
	{
		public TelPreDriveChecklistTemplateForm(TelPreDriveChecklistTemplateHeader header)
			: base(header)
		{
			InitializeComponent();
		}

		public override string FormCaption => BusinessEntity.HumanReadableName;

		protected override bool AllowNew => false;
	}
}
