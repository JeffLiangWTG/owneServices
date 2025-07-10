using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(JobRoadSailingController))]
	sealed class JobRoadSailingControllerTest : JobSailingControllerTest<JobRoadSailingController>
	{
		protected override ZString TransportType
		{
			get { return Constants.TransportModes.Road; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobRoadSailing;
		}

		protected override void InitializeLine(OrgHeader line) => line.OH_IsLineHaulProvider = true;

		protected override ZString TransportMode => Constants.TransportModes.Road;
		protected override SecurityCheckpoint CheckPointForCreateFromJob => Env.Security.TruckScheduleCreateFromJob;
	}
}
