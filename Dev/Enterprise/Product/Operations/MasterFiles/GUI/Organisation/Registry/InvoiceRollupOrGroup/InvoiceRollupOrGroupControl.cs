using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class InvoiceRollupOrGroupControl : RegistryZUserControl
	{
		public InvoiceRollupOrGroupControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GroupChargesGrid.ReadOnly = readOnly;
			JobTypeDropEdit.ReadOnly = readOnly;
			DirectionDropEdit.ReadOnly = readOnly;
			ModeDropEdit.ReadOnly = readOnly;
			DisplayDropEdit.ReadOnly = readOnly;
			StyleDropEdit.ReadOnly = readOnly;
			InvoiceDropEdit.ReadOnly = readOnly;
			PostingDropEdit.ReadOnly = readOnly;
			InvoiceCurrencyCodeFindBox.ReadOnly = readOnly;
		}
	}
}
