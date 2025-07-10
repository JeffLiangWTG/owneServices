using System.IO;
using System.Text;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.Messaging.Business
{
	public static class BlockPadder
	{
		public static string Pad(ZString message)
		{
			ZString result = message;
			int noOfChars = GetNoOfCharactersToPad(result.Length);
			if (noOfChars != 0)
			{
				result += "".PadRight(noOfChars);
			}
			return result;
		}

		public static void AddPaddedStream(this StreamWriter writer, TextReader reader)
		{
			using (reader)
			{
				writer.AddStream(reader);
			}
			int length = (int)writer.BaseStream.Length;
			if (length > 0)
			{
				writer.Write("".PadRight(GetNoOfCharactersToPad(length)));
			}
			writer.Flush();
		}

		public static int GetNoOfCharactersToPad(int length)
		{
			int result = 0;
			int modulus = length % 80;
			if (modulus != 0)
			{
				result = 80 - modulus;
			}
			return result;
		}

		public static VirtualMemoryStream GetPaddedMemoryStream(this TextReader reader)
		{
			var stream = new VirtualMemoryStream();
			var writer = new StreamWriter(stream, ASCIIEncoding.ASCII);
			writer.AddPaddedStream(reader);
			stream.Position = 0;
			return stream;
		}
	}
}
