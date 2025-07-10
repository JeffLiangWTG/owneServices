using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESTIRMessageNumberEncoderTest : TestCase
	{
		public void TestEncodeAndDecode()
		{
			int[] numbersToTest = new int[]
			{
				0,
				1,
				23,
				456,
				7890,
				12345,
				678901,
				2345678,
				90123456,
				789012345,
				2147483647,
				999999999
			};

			foreach (int mumber in numbersToTest)
			{
				string data = AESTIRMessageNumberEncoder.Encode(mumber);
				AssertEquals(mumber.ToString() + " " + data, false, data.Length > 6);
				AssertEquals(mumber, AESTIRMessageNumberEncoder.Decode(data));
			}
		}
	}
}
