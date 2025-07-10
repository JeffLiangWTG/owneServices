using Enterprise.Edifact;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business
{
	internal partial class CMMMessageDecoderTest : TestCase
	{
		public void TestGarbage()
		{
			const string messageText =
				"Want to make your finglonger?" +
				"Buy Agrofass!" +
				"";

			try
			{
				CMMMessageDecoder.Parse(messageText);
				Fail("Should have thrown an InvalidFormatException");
			}
			catch (InvalidFormatException ex)
			{
				AssertEquals("Corrupt or Malformed D95B CODECO or COARRI message. Cannot Process.", ex.Message);
			}
		}
	}
}
