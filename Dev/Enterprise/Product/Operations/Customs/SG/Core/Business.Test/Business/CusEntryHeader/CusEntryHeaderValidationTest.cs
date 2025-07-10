namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusEntryHeaderValidationTest : Customs.Business.Testing.CusEntryHeaderValidationTest
	{
		public void TestParent()
		{
			CusEntryHeader parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Validation.Parent, parent);
		}
	}
}
