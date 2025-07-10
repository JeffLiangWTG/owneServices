using System.Collections.Generic;
using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business
{
	class NZMMessageStreamFormatter : IStreamFormatter
	{
		readonly string header;
		public NZMMessageStreamFormatter(string header)
		{
			this.header = header;
		}

		public void FormatStream(ref Stream stream)
		{
			stream = stream.AddHeader(header);
			stream = stream.ReplaceChars(new KeyValuePair<char, string>[] { new KeyValuePair<char, string>('\t', "   ") });
		}
	}
}
