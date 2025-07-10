using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbStaffHolidayModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GlbStaffHoliday;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Staff;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.GlbStaffHoliday);

		public override bool SupportsWorkflow => true;
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GlbStaffHolidayFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GlbStaffHolidayFilterControl(GridCollection, (GlbStaffHolidayFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ActiveBusinessObjectCollection<GlbStaffHoliday>(Factory);

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		public override string WorkflowType => WorkflowDescriptors.GlbStaffHolidayDescriptorCode;
	}
}
