using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefCusTradeGroupController))]
	sealed class RefCusTradeGroupControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.Universal.RefCusTradeGroup;

		public void TestBusinessObject()
		{
			AssertEquals(typeof(RefCusTradeGroup), GetBusinessObjectType());
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Poland;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var eun = Factory.NewWithValidTestData<RefDataGrouping>();
			eun.ZZZ_DataGrouping = "TGR";
			eun.ZZZ_Description = "Test group";
			var refCusTradeGroup = Factory.NewWithValidTestData<RefCusTradeGroup>();
			refCusTradeGroup.ZZA_ZZZ_NKDataGrouping = "TGR";
			Factory.Save();
			return refCusTradeGroup;
		}
	}
}
