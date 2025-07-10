using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobRoadSailingController : JobSailingController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobRoadSailing; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.JobRoadSailing; }
		}

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Road; }
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForCreateFromJob
		{
			get { return Env.Security.TruckScheduleCreateFromJob; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TruckSchedule; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TruckScheduleNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TruckScheduleEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TruckScheduleDelete; }
		}

		#endregion
	}
}
