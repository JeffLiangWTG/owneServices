namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.PinGenerator.Testing
{
	using NUnit.Framework;

	public class DataBlockTest : TestCase
	{
		public void TestGetHumanReadableBlock()
		{
			string expected = "ONE   =[ TWO]\r\nTHREE =[FOUR ]\r\nblock_a=[ TWOFOUR        ]\r\n";
			AssertEquals(expected, dataBlock.GetHumanReadableBlock());
		}
		public void TestGetPinGenerationBlock()
		{
			string expected = " TWOFOUR        ";
			AssertEquals(expected, dataBlock.GetPinGenerationBlock());
		}

		protected DataBlock dataBlock;
		protected override void SetUp()
		{
			base.SetUp();
			dataBlock = new DataBlock("a", "Header Data");
			dataBlock.AddDataField("ONE   ", " TWO");
			dataBlock.AddDataField("THREE ", "FOUR ");
		}
	}
}
