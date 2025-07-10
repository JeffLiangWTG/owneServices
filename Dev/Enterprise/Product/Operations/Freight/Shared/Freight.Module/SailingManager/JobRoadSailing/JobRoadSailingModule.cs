using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobRoadSailingModule : JobSailingModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.JobRoadSailing; }
		}

		protected override ControllerID ControllerID => ControllerIDs.JobRoadSailing;

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobRoadSailingFilterControl(GridCollection, (JobRoadSailingFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobRoadSailingFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TruckSchedule; }
		}
	}
}
