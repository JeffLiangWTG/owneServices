using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class UYCMessageStreamFormatter : IStreamFormatter
	{
		readonly string header;
		public UYCMessageStreamFormatter(string header)
		{
			this.header = header;
		}

		public void FormatStream(ref Stream stream)
		{
			stream = stream.AddHeader(header);
		}
	}
}
