using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobAirSailingController))]
	sealed class JobAirSailingControllerTest : JobSailingControllerTest<JobAirSailingController>
	{
		protected override ZString TransportType
		{
			get { return Constants.TransportModes.Air; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobAirSailing;
		}

		protected override void InitializeLine(OrgHeader line)
		{
			line.OH_IsAirLine = true;
			Factory.Save();
		}

		protected override ZString TransportMode => Constants.TransportModes.Air;
		protected override SecurityCheckpoint CheckPointForCreateFromJob => Env.Security.FlightScheduleCreateFromJob;
	}
}
