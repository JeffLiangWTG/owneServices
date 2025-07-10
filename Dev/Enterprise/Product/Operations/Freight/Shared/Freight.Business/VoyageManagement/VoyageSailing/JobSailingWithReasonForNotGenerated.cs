using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class JobSailingWithReasonForNotGenerated
	{
		public JobSailingWithReasonForNotGenerated(JobSailing sailing, ZString reasonForNotGenerated)
		{
			Sailing = sailing;
			ReasonForNotGenerated = reasonForNotGenerated;
		}

		public readonly JobSailing Sailing;
		public readonly ZString ReasonForNotGenerated;
	}
}
