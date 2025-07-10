using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module
{
	public partial class BillOfLadingFilterControl : ZFilterStripControl
	{
		public BillOfLadingFilterControl()
		{
			InitializeComponent();
		}

		public BillOfLadingFilterControl(IBusinessObjectCollection gridCollection, BillOfLadingFilterStrip filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode);
		}

		#region Implementation

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CustomModuleFilterControlKludge();
		}

		#endregion
	}
}
