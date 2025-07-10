using System;
using Enterprise.CryptoUtilities;

namespace Enterprise.CMREdifact.CryptoUtilities.Testing
{
	sealed class FunctionsTest : NUnit.Framework.TestCase
	{
		public void TestToBase64String()
		{
			var inBytes = new byte[1000];
			new Random().NextBytes(inBytes);
			var base64String = Functions.ToBase64String(inBytes);
			var outBytes = Convert.FromBase64String(base64String);
			AssertEquals("ByteArrayLength", inBytes.Length, outBytes.Length);
			for (int i = 0; i < inBytes.Length; i++)
			{
				AssertEquals("Byte #" + i, inBytes[i], outBytes[i]);
			}

			AssertSplitToLines(base64String);
		}

		internal static void AssertSplitToLines(string base64String)
		{
			var lines = base64String.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				// 76 characters per line is MIME requirement.
				// We also experimentally confirmed that AU customs rejects single-line base64 messages.
				AssertLessThanOrEqualTo("encoded message should be split into lines that are no longer than 76 characters", line.Length, 76);
			}
		}
	}
}
