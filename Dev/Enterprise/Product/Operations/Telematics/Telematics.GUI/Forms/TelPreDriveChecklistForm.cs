using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Telematics.GUI.Forms
{
	public partial class TelPreDriveChecklistForm : ZChildForm
	{
		public TelPreDriveChecklistForm(TelPreDriveChecklistHeader header)
			: base(header)
		{
			InitializeComponent();
		}

		public override string FormCaption => BusinessEntity.HumanReadableName;
	}
}
