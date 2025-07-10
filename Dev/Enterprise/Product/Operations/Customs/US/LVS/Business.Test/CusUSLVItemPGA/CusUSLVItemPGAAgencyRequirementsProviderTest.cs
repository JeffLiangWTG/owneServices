using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	public class CusUSLVItemPGAAgencyRequirementsProviderTest : TestCaseWithFactory
	{
		public void TestIsPGAReqirementRelevant()
		{
			var provider = new CusUSLVItemPGAAgencyRequirementsProvider(Factory.New<CusUSLVItem>());
			Assert(provider.IsPGAReqirementRelevant);
		}

		public void TestDoesMatchCertificationMode()
		{
			var provider = new CusUSLVItemPGAAgencyRequirementsProvider(Factory.New<CusUSLVItem>());
			CombineAssertions(() =>
			{
				Assert(!provider.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.DOT));
				Assert(provider.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FCC));
				Assert(provider.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FSIS));
				Assert(provider.DoesMatchCertificationMode(GovernmentAgencyProgramCodeList.Codes.FDA));
			});
		}

		public void TestIsPGA()
		{
			var provider = new CusUSLVItemPGAAgencyRequirementsProvider(Factory.New<CusUSLVItem>());
			CombineAssertions(() =>
			{
				foreach (var code in new GovernmentAgencyProgramCodeList().GetAllCodes())
				{
					if (code == GovernmentAgencyProgramCodeList.Codes.DOT)
					{
						Assert(!provider.IsPGA(code));
					}
					else
					{
						Assert(provider.IsPGA(code));
					}
				}
			});
		}

		public void TestGetGovernmentAgencyProgramCodeList()
		{
			var codes = new GovernmentAgencyProgramCodeList();
			var provider = new CusUSLVItemPGAAgencyRequirementsProvider(Factory.New<CusUSLVItem>());
			var list = provider.GetGovernmentAgencyProgramCodeList();
			CombineAssertions(() =>
			{
				foreach (var code in codes.GetAllCodes())
				{
					if (code == GovernmentAgencyProgramCodeList.Codes.EPA
						|| code == GovernmentAgencyProgramCodeList.Codes.FCC
						|| code == GovernmentAgencyProgramCodeList.Codes.COA)
					{
						Assert(!list.ContainsCode(code));
					}
					else
					{
						Assert(list.ContainsCode(code));
					}
				}
			});
		}

		public void TestWhenLaceyAgencyCode_ThenReturnEmptyDescription()
		{
			var provider = new CusUSLVItemPGAAgencyRequirementsProvider(Factory.New<CusUSLVItem>());

			var laceyDescription = provider.GetRequirementDescription(GovernmentAgencyProgramCodeList.Codes.Lacey);
			AssertEquals("Expected empty description for lacey agency code", ZString.Empty, laceyDescription);
		}

		public void TestHFC()
		{
			const string tariffNum = "8923894890";
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNum;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "EH2";

			var lvItem = Factory.New<CusUSLVItem>();
			lvItem.ULI_Tariff = "8923894890";
			var provider = new CusUSLVItemPGAAgencyRequirementsProvider(lvItem);

			CombineAssertions(() =>
			{
				var disclaimReasonList = provider.GetDisclaimReasonList(GovernmentAgencyProgramCodeList.Codes.HFC);
				AssertEquals(4, disclaimReasonList.Count);
				AssertCollectionContains("A", disclaimReasonList.GetAllCodes());
				AssertCollectionContains("B", disclaimReasonList.GetAllCodes());
				AssertCollectionContains("C", disclaimReasonList.GetAllCodes());
				AssertCollectionContains("D", disclaimReasonList.GetAllCodes());
				var agencyProgram = provider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.HFC);
				AssertEquals("EPA", agencyProgram.Agency);
				AssertEquals("HFC", agencyProgram.Program);
				var desc = provider.PopulateAgencyCodeWithDescription(new ZArchitecture.Core.CodeDescriptionPair(GovernmentAgencyProgramCodeList.Codes.HFC, GovernmentAgencyProgramCodeList.Descriptions.HFC));
				AssertEquals("EPA - HFC - Hydrofluorocarbons", desc);
				var requirementDesc = provider.GetRequirementDescription(GovernmentAgencyProgramCodeList.Codes.HFC);
				AssertEquals("Hydrofluorocarbon specific data must be Reported (EH2)", requirementDesc);
			});
		}
	}
}
