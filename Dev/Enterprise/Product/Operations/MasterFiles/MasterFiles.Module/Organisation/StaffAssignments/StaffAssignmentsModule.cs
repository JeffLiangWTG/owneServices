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
	public class StaffAssignmentsModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.StaffAssignments; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.StaffAssignments);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new StaffAssignmentsFilterControl(GridCollection, (StaffAssignmentsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ActiveBusinessObjectCollection<OrgStaffAssignments>(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new StaffAssignmentsFilterBusinessObject();
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#region License

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.OrgDetailsViewCompanysStaffAssignments; }
		}

		#endregion
	}
}

