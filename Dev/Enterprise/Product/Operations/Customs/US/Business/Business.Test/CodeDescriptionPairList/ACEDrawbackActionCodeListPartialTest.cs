namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEDrawbackActionCodeListTest : NUnit.Framework.TestCase
	{
		public void TestIsManufacturedAction()
		{
			Assert(ACEDrawbackActionCodeList.IsManufacturedAction(ACEDrawbackActionCodeList.Codes.Manufactured));
			Assert(ACEDrawbackActionCodeList.IsManufacturedAction(ACEDrawbackActionCodeList.Codes.ManuAndTrans));
			Assert(!ACEDrawbackActionCodeList.IsManufacturedAction(ACEDrawbackActionCodeList.Codes.Exported));
			Assert(!ACEDrawbackActionCodeList.IsManufacturedAction(ACEDrawbackActionCodeList.Codes.Destroyed));
		}
	}
}
