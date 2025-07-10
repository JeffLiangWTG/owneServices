using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ComplianceDocumentImageCollectionControl : RegistryZUserControl
	{
		public ComplianceDocumentImageCollectionControl()
		{
			InitializeComponent();

			ReceiptImageSelectionControl.ImageObjectChangedByUser += (sender, e) =>
			{
				NotifyChanges();
			};
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			ReceiptGrid.ReadOnly = readOnly;
			ReceiptImageSelectionControl.ReadOnly = readOnly;
		}
	}
}
