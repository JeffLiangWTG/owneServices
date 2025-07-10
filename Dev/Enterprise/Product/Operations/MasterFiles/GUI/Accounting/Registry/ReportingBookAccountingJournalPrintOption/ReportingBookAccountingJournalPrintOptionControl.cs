using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ReportingBookAccountingJournalPrintOptionControl : RegistryZUserControl
	{
		public ReportingBookAccountingJournalPrintOptionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ReportingBookAccountingJournalPrintOptionGrid.ReadOnly = readOnly;
		}
	}
}
