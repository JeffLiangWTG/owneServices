using System.IO;

namespace CargoWise.BizTalk.UnitTestFX
{
	/// <summary>
	/// Compares the two streams character by character.
	/// </summary>
	public class TextComparer : ICompare
	{
		public MapResult Execute(Stream actual, Stream expected, bool ignoreComments = true)
		{
			string actualOutput = new StreamReader(actual).ReadToEnd();
			string expectedOutput = new StreamReader(expected).ReadToEnd();
			// StringBuilder expectedBuilder = new StringBuilder();

			// long totalBytesRead = 0;
			// long totalByteCount = expected.Length;
			// while (totalBytesRead < totalByteCount)
			// {
			//     byte[] expectedBuffer = new byte[500];
			//     int bytesJustRead = expected.Read(expectedBuffer, 0, 500);
			//     expectedBuilder.Append(Encoding.UTF8.GetString(expectedBuffer));
			//     totalBytesRead += bytesJustRead;
			// }

			MapResult result = new MapResult();
			result.Success = (actualOutput == expectedOutput);
			if (!result.Success)
			{
				result.MapOutput = actualOutput;
				result.OutputUpdateGram = expectedOutput;
			}

			return result;
		}
	}
}
