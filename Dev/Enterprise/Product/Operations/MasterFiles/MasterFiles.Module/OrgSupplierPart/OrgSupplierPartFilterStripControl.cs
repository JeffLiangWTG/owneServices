
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Filter control for OrgSupplierPart.
	/// </summary>
	public partial class OrgSupplierPartFilterStripControl : ZFilterStripControl
	{
		public OrgSupplierPartFilterStripControl()
		{
			InitializeComponent();
		}

		public OrgSupplierPartFilterStripControl(IBusinessObjectCollection gridCollection, OrgSupplierPartFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ImporterSupplierFilterStrip();
		}

		protected override void UpdateNumberLoadedMessageCore(ZString message, int numberOfRecordsFound, bool shouldShowNumberLoadedMessageBox)
		{
			bool showMsgBox = shouldShowNumberLoadedMessageBox;
			if (OverrideShouldShowNumberLoadedMessageBox)
			{
				showMsgBox = false;
			}

			base.UpdateNumberLoadedMessageCore(message, numberOfRecordsFound, showMsgBox);
		}

		public bool OverrideShouldShowNumberLoadedMessageBox;
	}
}
