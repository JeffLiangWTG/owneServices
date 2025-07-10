using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(TradeGroupsController))]
	sealed class TradeGroupsControllerTest : ZControllerBasherTest
	{
		public void TestGetForm_New()
		{
			using (var module = new TradeGroupsModule())
			{
				var controller = new TradeGroupsController { ParentModule = module };
				var expected = new ZString(Core.Constants.CountryCodes.Australia);
				var defaults = new FilterBusinessObjectDefaults { new FilterBusinessObjectDefault(CusRefTradeGroupCollection.FilterConstants.CountryCode, "Property", expected, false) };
				module.FilterBusinessObject.SetExternalDefaults(defaults);
				module.FilterBusinessObject.Filter.AddToFilter(new ZQuery(CusRefTradeGroupSchema.CR9_RN_NKCountryCode, SQLComparisonOperator.Equal, expected));
				using (var form = (CusRefTradeGroupForm)controller.ShowNewForm())
				{
					AssertEquals(expected, ((CusRefTradeGroup)form.BusinessEntity).CR9_RN_NKCountryCode);
				}
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.TradeGroups;
	}
}
