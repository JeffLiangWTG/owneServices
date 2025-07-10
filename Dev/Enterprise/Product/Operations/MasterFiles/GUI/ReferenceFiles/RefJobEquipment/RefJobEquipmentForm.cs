using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefJobEquipmentForm : ZTemplateForm
	{
		public RefJobEquipmentForm(JobEquipment jobEquipment) : base(jobEquipment)
		{
			InitializeComponent();
		}

		public override string FormCaption => BusinessEntity?.HumanReadableName;

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;
	}
}
