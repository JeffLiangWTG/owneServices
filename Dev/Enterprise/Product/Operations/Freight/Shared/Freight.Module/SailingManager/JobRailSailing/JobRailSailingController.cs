using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobRailSailingController : JobSailingController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.JobRailSailing; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobRailSailing; }
		}

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Rail; }
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForCreateFromJob
		{
			get { return Env.Security.RailScheduleCreateFromJob; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.RailSchedule; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.RailScheduleNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.RailScheduleEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.RailScheduleDelete; }
		}
		#endregion
	}
}
