using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class StmTemplateFilterControl : ZFilterStripControl
	{
		public StmTemplateFilterControl(IBusinessObjectCollection gridCollection, StmTemplateFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
