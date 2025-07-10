using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DefineGLSectionsForm : ZChildForm
	{
		public DefineGLSectionsForm(AccGLHeaderBulkUpdater updater)
			: base(updater)
		{
			InitializeComponent();
			this.InstructionsLabel.Text = Res.GetString("DefineGLSectionsForm|E5F8EE4E-B242-442f-9A5A-48C14C0CF2C0",
@"Use this screen to define the Sections for your General Ledger reports.
There are the six different sections that are used in the calculation of your General Ledger Reports.

You must nominate at least one 'Profit and Loss' section and at least one 'Balance Sheet' section.
These sections will be used in the calculation of Total and Consolidation Accounts.");
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveButton, CloseButton);
		}

		protected DefineGLSectionsForm()
			: base()
		{
			InitializeComponent();
		}

		public AccGLHeaderBulkUpdater AccGLHeaderBulkUpdater
		{
			get { return (AccGLHeaderBulkUpdater)BusinessEntity; }
		}
	}
}
