using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed partial class FormWithMultipleWorkflowTabs : ZForm
	{
		public FormWithMultipleWorkflowTabs(BusinessObject dataSource)
			: base(dataSource)
		{
			InitializeComponent();
		}
	}
}
