using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow
{
	public class TSWMessageStreamFormatter : IStreamFormatter
	{
		readonly string header;
		public TSWMessageStreamFormatter(string header)
		{
			this.header = header;
		}

		public void FormatStream(ref Stream stream)
		{
			stream = stream.AddHeader(header);
		}
	}
}
