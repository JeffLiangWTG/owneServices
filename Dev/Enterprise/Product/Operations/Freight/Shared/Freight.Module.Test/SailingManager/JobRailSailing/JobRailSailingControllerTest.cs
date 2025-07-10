using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobRailSailingController))]
	sealed class JobRailSailingControllerTest : JobSailingControllerTest<JobRailSailingController>
	{
		protected override ZString TransportType
		{
			get { return Constants.TransportModes.Rail; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobRailSailing;
		}

		protected override void InitializeLine(OrgHeader line) => line.OH_IsRailProvider = true;

		protected override ZString TransportMode => Constants.TransportModes.Rail;
		protected override SecurityCheckpoint CheckPointForCreateFromJob => Env.Security.RailScheduleCreateFromJob;
	}
}
