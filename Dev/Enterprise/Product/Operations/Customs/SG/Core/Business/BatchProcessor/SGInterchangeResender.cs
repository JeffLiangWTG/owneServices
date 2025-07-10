using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	[CodeAlive("This is to be used with the introduction of the new National Trade Platform (NTP) in SG")]
	public class SGInterchangeResender : InterchangeResender
	{
		protected internal SGInterchangeResender(EDIInterchange interchange) : base(interchange)
		{
		}

		protected override void SetToQueued(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
		}
	}
}
