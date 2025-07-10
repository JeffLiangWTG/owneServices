
namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.PinGenerator.Testing
{
	using NUnit.Framework;

	public class DataFieldTest : TestCase
	{
		public void TestEverything()
		{
			DataField dataField = new DataField("ONE", "TWO");
			AssertEquals("ONE", dataField.FieldName);
			AssertEquals("TWO", dataField.FieldValue);
		}
	}
}
