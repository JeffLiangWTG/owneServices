using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.Module
{
	public partial class CartageFilterControl : ZFilterStripControl<CartageModuleStrip>
	{
		public CartageFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, JobInvoicingConsumerTypes.LocalCartage.Code);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CartageModuleStrip();
		}
	}
}