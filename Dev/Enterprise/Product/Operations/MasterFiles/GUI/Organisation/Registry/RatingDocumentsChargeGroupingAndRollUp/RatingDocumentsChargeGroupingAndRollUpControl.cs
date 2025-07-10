using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RatingDocumentsChargeGroupingAndRollUpControl : RegistryZUserControl
	{
		public RatingDocumentsChargeGroupingAndRollUpControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GroupChargesGrid.ReadOnly = readOnly;
			JobTypeDropEdit.ReadOnly = readOnly;
			DisplayDropEdit.ReadOnly = readOnly;
			StyleDropEdit.ReadOnly = readOnly;
			TransportModeDropEdit.ReadOnly = readOnly;
			ModuleDropEdit.ReadOnly = readOnly;
		}
	}
}
