using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PostClearanceBrokerageConsumerType : BrokerageConsumerType
	{
		public PostClearanceBrokerageConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override bool OverseasAgentApplicable => false;
		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}

		public override bool IsDirectionSupported => false;

		public override bool IsTransportModeSupported => false;
	}
}
