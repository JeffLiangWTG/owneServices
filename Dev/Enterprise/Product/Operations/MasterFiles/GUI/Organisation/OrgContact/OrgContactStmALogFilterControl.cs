using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgContactStmALogFilterControl : ZStmALogFilterControl
	{
		public OrgContactStmALogFilterControl(StmALogCollection collection, OrgContactStmALogFilterBusinessObject filterBusinessObject)
			: base(collection, filterBusinessObject)
		{
			InitializeComponent();
			ToolStripFindDropButton.Enabled = false;
		}

		protected override ZBool ShouldPerformSearch()
		{
			return false;
		}
	}
}
