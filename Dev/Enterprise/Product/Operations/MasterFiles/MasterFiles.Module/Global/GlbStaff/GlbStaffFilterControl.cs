using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class GlbStaffFilterControl : ZFilterStripControl
	{
		public GlbStaffFilterControl()
		{
			InitializeComponent();
		}

		public GlbStaffFilterControl(IBusinessObjectCollection gridCollection, GlbStaffFilterBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.GlbStaffDescriptorCode);
			StaffManagerRegistryFieldsGridReadOnlyInitializer.AddStaffCustomFieldsColumns(FilteredGrid, gridCollection, false);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new StaffSecurityFilterStrip();
		}

		protected override bool CanAcceptDataCore(IDataObject dataObject)
		{
			if (dataObject.GetDataPresent(GridRowsDataObject.DataFormatType) && dataObject is GridRowsDataObject gridRowsDataObject)
			{
				return gridRowsDataObject.Elements.All(bizo => bizo.BaseBusinessObject is GlbGroup);
			}

			return false;
		}

		protected override void AcceptDataCore(IDataObject dataObject, BusinessObject targetBizO)
		{
			var factory = targetBizO.Factory;
			var glbGroups = ((GridRowsDataObject)dataObject).Elements.Select(bizo => factory.ImportFromAnotherFactorySafe((GlbGroup)bizo.BaseBusinessObject));
			((GlbStaff)targetBizO)?.Groups.AddRange(glbGroups);
		}

		protected override void AcceptDataCore(IDataObject dataObject, ZForm editForm)
		{
			((GlbStaffForm)editForm).GlbStaffTabControl.SelectTab("GroupsTabPage");
		}
	}
}
