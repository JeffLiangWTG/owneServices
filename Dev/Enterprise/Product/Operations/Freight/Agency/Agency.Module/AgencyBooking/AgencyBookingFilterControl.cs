using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module
{
	public partial class AgencyBookingFilterControl : ZFilterStripControl
	{
		public AgencyBookingFilterControl()
		{
			InitializeComponent();
		}

		public AgencyBookingFilterControl(IBusinessObjectCollection gridCollection, AgencyBookingFilterStrip filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode);
		}

		#region Implementation

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CustomModuleFilterControlKludge();
		}

		#endregion
	}
}
