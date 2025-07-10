using System;
using System.IO;
using System.Threading.Tasks;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruitment.Testing.Common
{
	sealed class MockResumeConverter : IResumeConverter
	{
		public void ConvertAvailableResumes(HRJobApplication app)
		{
			app.HP_CurrentStatus = "___";
			app.Factory.Save();
			CallCount++;
		}

		public int CallCount;

		public bool CanConvert(string ext) => true;

		public IeDoc GetResumeToConvert(HRJobApplication host)
		{
			throw new NotImplementedException();
		}

		public Task<Stream> ConvertToPdfAsync(string filename, Stream input)
		{
			throw new NotImplementedException();
		}

		public void AttachEdocCore(HRJobApplication jobApplication, string filename, Stream input, string docType)
		{
			throw new NotImplementedException();
		}

		public string ConvertedFileExtension => "PDF";
	}
}
