using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEDrawbackProvisionsListTest : TestCaseWithFactory
	{
		public void TestIsRejectedMerchandise()
		{
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._03));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._04));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._05));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._06));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._17));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._18));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._19));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._20));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._53));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._54));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._55));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._56));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._67));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._68));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._69));
			Assert(ACEDrawbackProvisionsList.IsRejectedMerchandise(ACEDrawbackProvisionsList.Codes._70));
		}

		public void TestIsApplicableProvisionsForOneTimeWaiverInd()
		{
			var result = ZString.Empty;
			var stringBuilder = new ZStringBuilder();
			foreach (var oneItem in ACEDrawbackProvisionsList.ApplicableProvisionsExceptionList)
			{
				stringBuilder.AppendIfNotEmpty(oneItem);
			}
			result = stringBuilder.ToStringWithDelimiterBetweenAppends(",");
			AssertEquals("01,02,07,10,11,12,13,21,22,51,52,57,60,61,62,63,71,72,75,76", result);
			AssertEquals("All Applicable except 01, 02, 07, 10, 11, 12, 13, 21, 22, 51, 52, 57, 60, 61, 62, 63, 71, 72, 75, 76", true, ACEDrawbackProvisionsList.IsApplicableProvisionsForOneTimeWaiverInd(ACEDrawbackProvisionsList.Codes._05));
			AssertEquals("Not Applicable", false, ACEDrawbackProvisionsList.IsApplicableProvisionsForOneTimeWaiverInd(ACEDrawbackProvisionsList.Codes._13));
		}

		public void TestIsHMF_MPFClaimable()
		{
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._08));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._09));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._15));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._16));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._58));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._59));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._65));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._66));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._73));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._74));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._76));
			Assert(ACEDrawbackProvisionsList.IsHMF_MPFClaimable(ACEDrawbackProvisionsList.Codes._77));
		}

		public void TestIsDirectIdentificationManufacturing()
		{
			Assert(ACEDrawbackProvisionsList.IsDirectIdentificationManufacturing(ACEDrawbackProvisionsList.Codes._01));
			Assert(ACEDrawbackProvisionsList.IsDirectIdentificationManufacturing(ACEDrawbackProvisionsList.Codes._21));
			Assert(ACEDrawbackProvisionsList.IsDirectIdentificationManufacturing(ACEDrawbackProvisionsList.Codes._51));
			Assert(ACEDrawbackProvisionsList.IsDirectIdentificationManufacturing(ACEDrawbackProvisionsList.Codes._71));
		}

		public void TestIsSubstitutionManufacturing()
		{
			Assert(ACEDrawbackProvisionsList.IsSubstitutionManufacturing(ACEDrawbackProvisionsList.Codes._02));
			Assert(ACEDrawbackProvisionsList.IsSubstitutionManufacturing(ACEDrawbackProvisionsList.Codes._22));
			Assert(ACEDrawbackProvisionsList.IsSubstitutionManufacturing(ACEDrawbackProvisionsList.Codes._52));
			Assert(ACEDrawbackProvisionsList.IsSubstitutionManufacturing(ACEDrawbackProvisionsList.Codes._72));
			Assert(ACEDrawbackProvisionsList.IsSubstitutionManufacturing(ACEDrawbackProvisionsList.Codes._75));
		}

		public void TestIsTFTEA()
		{
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._51));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._52));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._53));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._54));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._55));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._56));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._57));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._58));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._71));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._76));
			Assert(ACEDrawbackProvisionsList.IsTFTEA(ACEDrawbackProvisionsList.Codes._77));
		}

		public void TestIsSubstitutedValueRequired()
		{
			Assert(ACEDrawbackProvisionsList.IsSubstitutedValueRequired(ACEDrawbackProvisionsList.Codes._52));
			Assert(ACEDrawbackProvisionsList.IsSubstitutedValueRequired(ACEDrawbackProvisionsList.Codes._59));
			Assert(ACEDrawbackProvisionsList.IsSubstitutedValueRequired(ACEDrawbackProvisionsList.Codes._66));
			Assert(ACEDrawbackProvisionsList.IsSubstitutedValueRequired(ACEDrawbackProvisionsList.Codes._72));
			Assert(ACEDrawbackProvisionsList.IsSubstitutedValueRequired(ACEDrawbackProvisionsList.Codes._73));
			Assert(ACEDrawbackProvisionsList.IsSubstitutedValueRequired(ACEDrawbackProvisionsList.Codes._75));
			Assert(ACEDrawbackProvisionsList.IsSubstitutedValueRequired(ACEDrawbackProvisionsList.Codes._76));
			Assert(ACEDrawbackProvisionsList.IsSubstitutedValueRequired(ACEDrawbackProvisionsList.Codes._77));
		}

		public void TestIs1313A()
		{
			AssertEquals(true, ACEDrawbackProvisionsList.Is1313A(ACEDrawbackProvisionsList.Codes._01));
			AssertEquals(true, ACEDrawbackProvisionsList.Is1313A(ACEDrawbackProvisionsList.Codes._21));
			AssertEquals(true, ACEDrawbackProvisionsList.Is1313A(ACEDrawbackProvisionsList.Codes._51));
			AssertEquals(true, ACEDrawbackProvisionsList.Is1313A(ACEDrawbackProvisionsList.Codes._71));
		}

		public void TestIs1313D()
		{
			AssertEquals(true, ACEDrawbackProvisionsList.Is1313D(ACEDrawbackProvisionsList.Codes._07));
			AssertEquals(true, ACEDrawbackProvisionsList.Is1313D(ACEDrawbackProvisionsList.Codes._57));
		}

		public void TestIs5062()
		{
			AssertEquals(true, ACEDrawbackProvisionsList.Is5062(ACEDrawbackProvisionsList.Codes._14));
			AssertEquals(true, ACEDrawbackProvisionsList.Is5062(ACEDrawbackProvisionsList.Codes._64));
		}

		public void TestGetDrawbackProvisionList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "Drawback Provision Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "01", "1313(A) - Direct Identification Manufacturing Drawback (Articles made from imported merchandise)", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "64", "TFTEA 5062(C) - TFTEA Distilled spirits, wines, or beer which are unmerchantable or do not conform to sample or specifications", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			var list = ACEDrawbackProvisionsList.GetDrawbackProvisionList(Factory);

			AssertEquals(2, list.Count);
		}

		public void TestGetSectionDescriptionFromCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "Drawback Provision Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "01", "1313(A) - Direct Identification Manufacturing Drawback (Articles made from imported merchandise)", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "64", "TFTEA 5062(C) - TFTEA Distilled spirits, wines, or beer which are unmerchantable or do not conform to sample or specifications", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			AssertEquals("1313(A)", ACEDrawbackProvisionsList.GetSectionDescriptionFromCode(Factory, "01"));
			AssertEquals("TFTEA 5062(C)", ACEDrawbackProvisionsList.GetSectionDescriptionFromCode(Factory, "64"));
		}
	}
}
