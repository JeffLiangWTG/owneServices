using System.Text;

namespace CargoWise.eServices.Encryption.Common
{
	public static class Constants
	{
		public const int KeySize = 2048;
		public const int MaxLengthBinary = 86;
		public const bool WithOAEPPadding = true;
		public static readonly Encoding Converter = Encoding.UTF8;
	}
}