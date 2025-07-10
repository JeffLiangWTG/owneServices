using System.IO;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class BlockPadderTest : TestCase
	{
		public void TestPad()
		{
			AssertEquals(0, BlockPadder.Pad("").Length);
			AssertEquals(80, BlockPadder.Pad("".PadRight(79, 'X')).Length);
			AssertEquals(80, BlockPadder.Pad("".PadRight(80, 'X')).Length);
			AssertEquals(160, BlockPadder.Pad("".PadRight(81, 'X')).Length);
			AssertEquals(160, BlockPadder.Pad("".PadRight(160, 'X')).Length);
			AssertEquals(240, BlockPadder.Pad("".PadRight(161, 'X')).Length);
		}

		public void TestGetNoOfCharactersToPad()
		{
			AssertEquals(0, BlockPadder.GetNoOfCharactersToPad(0));
			AssertEquals(1, BlockPadder.GetNoOfCharactersToPad(79));
			AssertEquals(0, BlockPadder.GetNoOfCharactersToPad(80));
			AssertEquals(79, BlockPadder.GetNoOfCharactersToPad(81));
			AssertEquals(0, BlockPadder.GetNoOfCharactersToPad(160));
			AssertEquals(79, BlockPadder.GetNoOfCharactersToPad(161));
		}

		public void TestAddPaddedStream()
		{
			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream);
				writer.AddPaddedStream(new StringReader("HELLO WORLD"));
				stream.Position = 0;
				AssertEquals(80, stream.Length);
				var data = new byte[80];
				stream.Read(data, 0, 80);
				AssertEquals("HELLO WORLD".PadRight(80), System.Text.ASCIIEncoding.ASCII.GetString(data));
			}
		}

		public void TestTextReaderGetMemoryStream()
		{
			var reader = new StringReader("Test Text Reader Get Memory Stream");
			var stream = reader.GetPaddedMemoryStream();
			AssertEquals("Length", 80, stream.Length);
			var data = new byte[stream.Length];
			stream.Read(data, 0, (int)stream.Length);
			AssertEquals("Test Text Reader Get Memory Stream".PadRight(80), System.Text.ASCIIEncoding.ASCII.GetString(data));
		}
	}
}
