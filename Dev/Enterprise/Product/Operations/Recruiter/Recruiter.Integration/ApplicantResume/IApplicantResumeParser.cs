using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Recruiter.Integration
{
	public enum ApplicantResumeParseStatus
	{
		Success,
		Fail
	}

	public enum ApplicantBatchGetDataStatus
	{
		Processing,
		Finished,
		Zip,
		Error
	}

	public interface IApplicantResumeParseResult
	{
		ApplicantResumeParseStatus Status { get; }
		IApplicantResume ParsedResume { get; }
		string Message { get; }
	}

	public interface IApplicantResumeBatchSendResult
	{
		ApplicantResumeParseStatus Status { get; }
		string Message { get; }
		string Token { get; }
	}

	public interface IApplicantResumeParser
	{
		IApplicantResumeParseResult Parse(string resumeFileName, byte[] resumeFileContent);
		IApplicantResume ReadFromXml(string resumeXml);
		IApplicantResumeBatchSendResult SendBatch(byte[] contents);
		IApplicantResumeBatchGetDataResult GetData(string token);
	}

	public interface IApplicantResumeBatchGetDataResult
	{
		ApplicantBatchGetDataStatus Status { get; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		byte[] ZipContent { get; }
		string Error { get; }
	}
}
