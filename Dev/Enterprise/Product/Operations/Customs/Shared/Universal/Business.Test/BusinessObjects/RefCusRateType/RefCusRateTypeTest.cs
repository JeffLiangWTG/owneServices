using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusRateType))]
	sealed class RefCusRateTypeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetRateTypesByDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			var cnRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.China, Constants.RateTypes.Duty, "Duty");
			var krRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.KoreaSouth, Constants.RateTypes.Duty, "Duty");
			var cnRateType2 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.China, Constants.RateTypes.Refund, "REF");
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { cnRateType.PK, cnRateType2.PK }, RefCusRateType.Loader.GetRateTypesByDataGrouping(Factory, Core.Constants.CountryCodes.China).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { krRateType.PK }, RefCusRateType.Loader.GetRateTypesByDataGrouping(Factory, Core.Constants.CountryCodes.KoreaSouth).Select(x => x.PK));
		}

		public void TestZZR_Description()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateOrGetLanguage("ZHS", "ChineseSimplified");
			helper.CreateOrGetLanguage("KO", "Korean");
			Factory.Save();

			var cnRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.China, Constants.RateTypes.Duty, "Duty");
			var krRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.KoreaSouth, Constants.RateTypes.Duty, "Duty");
			helper.CreateNewOrGetExistingCusRateTypeLanguage(cnRateType, "ZHS", "进口关税");
			helper.CreateNewOrGetExistingCusRateTypeLanguage(krRateType, "KO", "수입 관세");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Description in Chinese Simplified", "进口关税", cnRateType.ZZR_Description);
				AssertEquals("Description in English", "Duty", krRateType.ZZR_Description);
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.English;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Description in English", "Duty", cnRateType.ZZR_Description);
				AssertEquals("Description in English", "Duty", krRateType.ZZR_Description);
			}

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.Korean;
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Description in English", "Duty", cnRateType.ZZR_Description);
				AssertEquals("Description in Korean", "수입 관세", krRateType.ZZR_Description);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<RefCusRateType>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return new UniversalReferenceTestDataHelper(factory).CreateCusRateType(Core.Constants.CountryCodes.SouthAfrica, "ABC", "Daniel");
		}
	}
}
