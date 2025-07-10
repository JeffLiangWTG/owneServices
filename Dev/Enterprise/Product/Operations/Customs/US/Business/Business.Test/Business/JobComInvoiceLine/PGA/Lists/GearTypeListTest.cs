using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class GearTypeListTest : TestCaseWithFactory
	{
		public void TestGetListFor370Program()
		{
			var fullList = new GearTypeList();
			var list = GearTypeList.GetListFor370Program(Factory);
			AssertEquals("Data should be cached", list, GearTypeList.GetListFor370Program(Factory));
			var expectedCodes = new[] {
				GearTypeList.Codes.LargeScaleDriftnetHighSeas,
				GearTypeList.Codes.GillnetLessThan15Miles24KmInTotalLength,
				GearTypeList.Codes.Longline,
				GearTypeList.Codes.OtherType,
				GearTypeList.Codes.PoleAndLineHookAndLine,
				GearTypeList.Codes.PurseSeineNet,
			};
			AssertEquals(expectedCodes.Length, list.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list.GetDescriptionFromCode(code));
			}
		}
	}
}
