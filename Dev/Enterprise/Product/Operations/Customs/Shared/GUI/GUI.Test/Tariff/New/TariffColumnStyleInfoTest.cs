using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TariffColumnStyleTest : TestCase
	{
		public void TestConstructor()
		{
			var info = new TariffColumnStyleInfo { BindToTariffPropertyInfo = "PropertyInfo", TariffCode = "99", TariffType = TariffType.Import };
			using (var columnStyle = new TariffColumnStyle(info))
			{
				AssertEquals("EditControl type", typeof(TariffGridFindBox), columnStyle.EditControl.GetType());
				var findBox = (TariffGridFindBox)columnStyle.EditControl;
				AssertEquals("ColumnStyle type", typeof(TariffColumnStyle), findBox.ColumnStyle.GetType());
				AssertEquals("BindToTariffPropertyInfo", info.BindToTariffPropertyInfo, findBox.BindToTariffPropertyInfo);
				AssertEquals("TariffCode", info.TariffCode, findBox.TariffInfo.TariffCode);
				AssertEquals("TariffType", info.TariffType, findBox.TariffInfo.TariffType);
			}
		}

		public void TestICustomModuleFilterProvider()
		{
			Assert("Interface ICustomModuleFilterProvider should be implemented", new TariffColumnStyleInfo() is ICustomModuleFilterProvider);
		}
	}
}
