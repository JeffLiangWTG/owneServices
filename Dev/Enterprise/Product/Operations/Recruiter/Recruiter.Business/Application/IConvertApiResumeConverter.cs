using System.IO;
using System.Threading.Tasks;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Recruiter.Business
{
	public interface IResumeConverter
	{
		bool CanConvert(string ext);
		IeDoc GetResumeToConvert(HRJobApplication host);
		void ConvertAvailableResumes(HRJobApplication host);
		Task<Stream> ConvertToPdfAsync(string inputFileExtension, Stream input);
		void AttachEdocCore(HRJobApplication jobApplication, string filename, Stream input, string docType);

		string ConvertedFileExtension { get; }
	}

	public interface IConvertApi
	{
		Task<Stream> ConvertAsync(string fromFormat, string toFormat, Stream input);
	}
}
