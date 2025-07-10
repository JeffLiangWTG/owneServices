using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Module
{
	public partial class ISFFilterControl : ZFilterStripControl
	{
		public ISFFilterControl(CusISFHeaderCollection headers, ISFFilterBusinessObject filterBizo)
			: base(headers, filterBizo)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode);
			}
		}
	}
}
