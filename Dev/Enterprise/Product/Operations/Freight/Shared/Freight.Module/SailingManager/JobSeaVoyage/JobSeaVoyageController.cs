using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobSeaVoyageController : JobVoyageController
	{
		protected override ZString TransportType
		{
			get { return Constants.TransportModes.Sea; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobSeaVoyage; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.JobSeaVoyage; }
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SailingSchedule; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SailingScheduleNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SailingScheduleEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.SailingScheduleDelete; }
		}

		#endregion
	}
}
