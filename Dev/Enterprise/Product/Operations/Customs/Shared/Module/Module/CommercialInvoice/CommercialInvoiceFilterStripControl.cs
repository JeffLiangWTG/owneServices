using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class CommercialInvoiceFilterControl : ZFilterStripControl<CommercialInvoiceFilterStrip>
	{
		public CommercialInvoiceFilterControl(IBusinessObjectCollection gridCollection, CommercialInvoiceFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode);
			}
		}

		public ZFilterStrip GetEmptyFilterStrip() => Strips.FirstOrDefault(x => x.CurrentDataItem.IsFilterDescriptionEmpty);

		protected override Size DefaultMinimumSize => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 700);
	}
}
