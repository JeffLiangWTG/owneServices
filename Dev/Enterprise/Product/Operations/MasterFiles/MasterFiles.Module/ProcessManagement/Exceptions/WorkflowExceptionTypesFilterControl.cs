using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class WorkflowExceptionTypesFilterControl : ZFilterStripControl
	{
		public WorkflowExceptionTypesFilterControl()
		{
			InitializeComponent();
		}

		public WorkflowExceptionTypesFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
