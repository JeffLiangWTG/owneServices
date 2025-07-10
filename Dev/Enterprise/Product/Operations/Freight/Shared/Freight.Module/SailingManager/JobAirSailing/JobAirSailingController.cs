using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobAirSailingController : JobSailingController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.JobAirSailing; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobAirSailing; }
		}

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Air; }
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForCreateFromJob
		{
			get { return Env.Security.FlightScheduleCreateFromJob; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.FlightSchedule; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.FlightScheduleNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.FlightScheduleEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.FlightScheduleDelete; }
		}
		#endregion
	}
}
