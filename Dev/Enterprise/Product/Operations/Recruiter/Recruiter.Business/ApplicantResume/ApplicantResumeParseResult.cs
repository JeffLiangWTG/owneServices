using System.Diagnostics.CodeAnalysis;
using Enterprise.Recruiter.Integration;

namespace Enterprise.Recruiter.Business
{
	public class ApplicantResumeParseResult : IApplicantResumeParseResult
	{
		public ApplicantResumeParseResult()
		{
			Status = ApplicantResumeParseStatus.Fail;
		}

		public ApplicantResumeParseStatus Status { get; set; }

		public IApplicantResume ParsedResume { get; set; }

		public string Message { get; set; }
	}

	public class ApplicantResumeBatchSendResult : IApplicantResumeBatchSendResult
	{
		public ApplicantResumeParseStatus Status { get; set; }

		public string Message { get; set; }

		public string Token { get; set; }
	}

	public class ApplicantResumeBatchGetDataResult : IApplicantResumeBatchGetDataResult
	{
		public ApplicantBatchGetDataStatus Status { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] ZipContent { get; set; }
		public string Error { get; set; }
	}
}
