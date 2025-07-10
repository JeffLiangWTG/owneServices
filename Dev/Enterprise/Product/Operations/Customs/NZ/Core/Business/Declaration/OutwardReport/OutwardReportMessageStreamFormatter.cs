using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport
{
	class OutwardReportMessageStreamFormatter : IStreamFormatter
	{
		readonly string header;
		public OutwardReportMessageStreamFormatter(string header)
		{
			this.header = header;
		}

		public void FormatStream(ref Stream stream)
		{
			stream = stream.AddHeader(header);
		}
	}
}
