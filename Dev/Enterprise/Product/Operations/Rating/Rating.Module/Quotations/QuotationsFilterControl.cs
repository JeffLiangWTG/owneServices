using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.Module
{
	public partial class QuotationsFilterControl : ZFilterStripControl
	{
		public QuotationsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.QuotationWorkflowDescriptorCode);
		}

		protected override ZFilterStrip NewZFilterStrip()
			=> new WorkflowFilterStrip();

		#region Dispose

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
