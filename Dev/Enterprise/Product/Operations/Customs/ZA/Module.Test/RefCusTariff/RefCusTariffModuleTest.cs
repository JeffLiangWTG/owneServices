using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(RefCusTariffModuleForTest))]
	sealed class RefCusTariffModuleTest : Universal.Module.Testing.RefCusTariffModuleTest
	{
		public void TestSetFindBoxCodeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("ZA", "3P1");
			var tariff = helper.LoadOrCreateNewTariff("ZA", tariffType.PK, "123456789", new CargoWise.Types.ZDateTime(1990, 12, 4), new CargoWise.Types.ZDateTime(2050, 12, 5));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "12", tariff);
			tariff.ZZ1_IsSystem = false;
			using (var module = new RefCusTariffModuleForTest())
			{
				using (var findBox = new ZGuidFindBox())
				{
					module.SetFindBoxCodeDescription(findBox, tariff);
					AssertEquals("FindBox Code", "123456789 12", findBox.CurrentCode);
					AssertEquals("Tariff View", "123456789 12", tariff.ZZ1_TariffCode);
				}
			}
		}

		public void TestModuleDecisionProvider()
		{
			using (var module = new RefCusTariffModuleForTest())
			{
				using (var findBox = new ZGuidFindBox())
				{
					var decisionProvider = module.GetModuleDecisionProviderForFindBox_Exposed(findBox);
					AssertType("Module Decision Provider", typeof(ZAModuleDecisionProvider), decisionProvider);
				}
			}
		}

		public void TestPopupModuleDecisionProvider()
		{
			using (var module = new RefCusTariffModuleForTest())
			{
				using (var findBox = new ZGuidFindBox())
				{
					var decisionProvider = module.GetModuleDecisionProviderForFindBoxPopup_Exposed(findBox);
					AssertType("Popup Module Decision Provider", typeof(PopupModuleDecisionProvider), decisionProvider);
				}
			}
		}
	}
}
