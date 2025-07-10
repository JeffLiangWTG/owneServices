using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffType))]
	class RefCusTariffTypeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZZI_Description()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Taiwan);
			helper.CreateOrGetLanguage("ZHT", "Chinese Traditional");
			helper.CreateOrGetLanguage("FR", "French");
			helper.CreateOrGetLanguage("EN", "English");
			Factory.Save();

			var tariffTypeAT = helper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			tariffTypeAT.ZZI_Description = "Alcohol Tax";
			helper.CreateRefCusTariffTypeLanguage(tariffTypeAT, "ZHT", "酒稅");
			helper.CreateRefCusTariffTypeLanguage(tariffTypeAT, "FR", "Taxe sur l'alcool");
			var tariffTypeTT = helper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "TT");
			tariffTypeTT.ZZI_Description = "Tobacco Tax";
			helper.CreateRefCusTariffTypeLanguage(tariffTypeTT, "ZHT", "菸稅");
			helper.CreateRefCusTariffTypeLanguage(tariffTypeTT, "FR", "Taxe sur le tabac");

			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "EN";
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("ZZI_Description in English", "Alcohol Tax", tariffTypeAT.ZZI_Description);
				AssertEquals("ZZI_Description in English", "Tobacco Tax", tariffTypeTT.ZZI_Description);
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("ZZI_Description in French", "Taxe sur l'alcool", tariffTypeAT.ZZI_Description);
				AssertEquals("ZZI_Description in French", "Taxe sur le tabac", tariffTypeTT.ZZI_Description);
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseTraditional;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("ZZI_Description in ChineseTraditional", "酒稅", tariffTypeAT.ZZI_Description);
				AssertEquals("ZZI_Description in ChineseTraditional", "菸稅", tariffTypeTT.ZZI_Description);
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.German;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("ZZI_Description by defalut", "Alcohol Tax", tariffTypeAT.ZZI_Description);
				AssertEquals("ZZI_Description by defalut", "Tobacco Tax", tariffTypeTT.ZZI_Description);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<RefCusTariffType>();
		}
	}
}
