using System;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	internal class AllJobsConsumerType : JobInvoicingConsumerType
	{
		public AllJobsConsumerType()
			: base("ALL", ResString.GetMultilingualString("FFF12670-3509-44A8-A549-E26603CAA260", "Any Job Type"))
		{
		}

		public override ControllerID ControllerID => throw new NotImplementedException();

		public override Type BizoType => throw new NotImplementedException();

		public override SecurityCheckpoint DistanceCalculationCheckpoint => Env.Security.None;

		public override bool IsDirectionSupported => true;

		public override bool IsTransportModeSupported => true;
	}
}
