using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobRailSailingModule : JobSailingModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.JobRailSailing; }
		}

		protected override ControllerID ControllerID => ControllerIDs.JobRailSailing;

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobRailSailingFilterControl(GridCollection, (JobRailSailingFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobRailSailingFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.RailSchedule; }
		}
	}
}
