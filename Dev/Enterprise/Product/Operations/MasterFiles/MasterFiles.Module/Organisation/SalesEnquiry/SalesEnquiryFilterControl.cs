using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class SalesEnquiryFilterControl : ZFilterStripControl
	{
		public SalesEnquiryFilterControl(IBusinessObjectCollection gridCollection, SalesEnquiryFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode);
			}
		}
	}
}
