using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EInvoicingPendingTransactionsNotificationGroupControl : RegistryZUserControl
	{ 
		public EInvoicingPendingTransactionsNotificationGroupControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GroupPKGuidFindBox.ReadOnly = readOnly;
			DateTypeDropEdit.ReadOnly = readOnly;
			DaysIntEdit.ReadOnly = readOnly;
		}
	}
}
