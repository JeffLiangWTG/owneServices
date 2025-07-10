using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusRefTariffVersionModule))]
	sealed class CusRefTariffVersionModuleTest : ZModuleBasherTest
	{
		public void TestTariffVersionModuleAllows()
		{
			using (var module = new CusRefTariffVersionModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", true, module.AllowDelete);
				AssertEquals("module.AllowView", true, module.AllowView);
				AssertEquals("module.AllowUniversalCopy", false, module.AllowUniversalCopy);
				AssertEquals("module.AllowCopyFilterGridHyperlinkToClipboard", false, module.AllowCopyFilterGridHyperlinkToClipboard);
			}
		}

		public void TestShowDeleteFrom()
		{
			var version1 = Factory.New<CusRefTariffVersion>();
			version1.CRT_Version = "HS2021";
			version1.CRT_Description = "HS2021 desc";
			version1.CRT_EffectiveDate = new ZDate(2020, 3, 8);
			var version2 = Factory.New<CusRefTariffVersion>();
			version2.CRT_Version = "HS2022";
			version2.CRT_Description = "HS2022 desc";
			version2.CRT_EffectiveDate = new ZDate(2020, 3, 9);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("AU", "TTX");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff("AU", tariffType.PK, "122231", new ZDateTime(2021, 3, 3), new ZDateTime(2021, 3, 10), "Tariff desc", isSystem: false, versionCode: "HS2021");
			Factory.Save();
			using (var module = new CusRefTariffVersionModuleForUnitTest())
			{
				UnitTestUserNotification.Instance.ClearMessages();
				using (var form = module.GetDeleteForm(version2))
				{
					AssertNotNull(form);
				}

				tariff.ZZ1_IsSystem = true;
				using (var form = module.GetDeleteForm(version1))
				{
					AssertNull(form);
					AssertEquals("There are Tariffs linked to the version and it can not be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CusRefTariffVersion;

		sealed class CusRefTariffVersionModuleForUnitTest : CusRefTariffVersionModule
		{
			public IZForm GetDeleteForm(CusRefTariffVersion version) => ShowDeleteForm(version);
		}
	}
}
