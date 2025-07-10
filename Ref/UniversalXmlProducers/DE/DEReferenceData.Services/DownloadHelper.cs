using System.Text;

namespace CargoWise.RefDbRepo.DEReferenceData.Services
{
	class DownloadHelper
	{
		internal static Encoding GetEncoding(byte[] content)
		{
			Ude.CharsetDetector charsetDetector = new Ude.CharsetDetector();
			charsetDetector.Feed(content, 0, content.Length);
			charsetDetector.DataEnd();
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			return Encoding.GetEncoding(charsetDetector.Charset);
		}
	}
}
