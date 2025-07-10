using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.Messaging.Business
{
	class CBPEDMessageStreamFormatter : IStreamFormatter
	{
		public void FormatStream(ref Stream stream)
		{
			stream = stream.AddStringAfterEachNumberOfChars(80, System.Environment.NewLine);
		}
	}
}
