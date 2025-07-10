using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CargoWise.IO;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruitment.Testing.Common
{
	sealed class TestConvertApiImpl : IConvertApi
	{
		public TestConvertApiImpl()
		{
			streamToReturn = new MemoryStream();
		}

		public TestConvertApiImpl(Stream stream)
		{
			streamToReturn = stream ?? new MemoryStream();
		}
		readonly Stream streamToReturn;

		public async Task<Stream> ConvertAsync(string fromFormat, string toFormat, Stream data)
		{
			Converted.Add(Encoding.UTF8.GetString(data.ToByteArray()));
			await Task.Delay(1000).ConfigureAwait(false);
			return streamToReturn;
		}
		public List<string> Converted = new List<string>();
	}
}
