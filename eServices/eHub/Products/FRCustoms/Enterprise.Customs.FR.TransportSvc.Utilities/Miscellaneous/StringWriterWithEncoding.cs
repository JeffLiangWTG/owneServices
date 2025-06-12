using System.IO;
using System.Text;

namespace Enterprise.Customs.FR.TransportSvc.Utilities
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
	public class StringWriterWithEncoding : StringWriter
	{
		public StringWriterWithEncoding(Encoding encoding) : base()
		{
			myEncoding = encoding;
		}

		public override Encoding Encoding
		{
			get
			{
				return myEncoding;
			}
		}

		readonly Encoding myEncoding;
	}
}
