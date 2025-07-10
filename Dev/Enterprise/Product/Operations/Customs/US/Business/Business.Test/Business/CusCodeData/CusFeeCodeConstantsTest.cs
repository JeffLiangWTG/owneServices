using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusFeeCodeConstantsTest : NUnit.Framework.TestCase
	{
		public void TestGetTaxCodes()
		{
			string[] taxCodes = CusFeeCodeConstants.GetTaxCodes();

			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, taxCodes[0]);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Tobacco, taxCodes[1]);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, taxCodes[2]);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.OtherExcise, taxCodes[3]);
		}

		public void TestIsExciseTax()
		{
			AssertEquals(true, CusFeeCodeConstants.IsExciseTax(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals(false, CusFeeCodeConstants.IsExciseTax(Core.Constants.USCustoms.FeeCodes.DutiableMail));

			AssertEquals(true, CusFeeCodeConstants.IsExciseTax(Core.Constants.USCustoms.FeeCodes.Wines));
			AssertEquals(false, CusFeeCodeConstants.IsExciseTax(Core.Constants.USCustoms.FeeCodes.HMF));

			AssertEquals(true, CusFeeCodeConstants.IsExciseTax(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals(false, CusFeeCodeConstants.IsExciseTax(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge));

			AssertEquals(true, CusFeeCodeConstants.IsExciseTax(Core.Constants.USCustoms.FeeCodes.DistilledSpirits));
			AssertEquals(false, CusFeeCodeConstants.IsExciseTax(Core.Constants.USCustoms.FeeCodes.Sugar));
		}

		public void TestIsHeaderLevelFee()
		{
			AssertEquals(false, CusFeeCodeConstants.IsHeaderLevelFee(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals(true, CusFeeCodeConstants.IsHeaderLevelFee(Core.Constants.USCustoms.FeeCodes.DutiableMail));

			AssertEquals(false, CusFeeCodeConstants.IsHeaderLevelFee(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals(true, CusFeeCodeConstants.IsHeaderLevelFee(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge));

			AssertEquals(false, CusFeeCodeConstants.IsHeaderLevelFee(Core.Constants.USCustoms.FeeCodes.DistilledSpirits));
			AssertEquals(true, CusFeeCodeConstants.IsHeaderLevelFee(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal));
		}

		public void TestIsRelatedToTariffNumber()
		{
			AssertEquals(true, CusFeeCodeConstants.IsRelatedToTariffNumber(Core.Constants.USCustoms.FeeCodes.Avocado));
			AssertEquals(false, CusFeeCodeConstants.IsRelatedToTariffNumber(Core.Constants.USCustoms.FeeCodes.DutiableMail));

			AssertEquals(true, CusFeeCodeConstants.IsRelatedToTariffNumber(Core.Constants.USCustoms.FeeCodes.Beef));
			AssertEquals(false, CusFeeCodeConstants.IsRelatedToTariffNumber(Core.Constants.USCustoms.FeeCodes.HMF));

			AssertEquals(true, CusFeeCodeConstants.IsRelatedToTariffNumber(Core.Constants.USCustoms.FeeCodes.Blueberry));
			AssertEquals(false, CusFeeCodeConstants.IsRelatedToTariffNumber(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			AssertEquals(true, CusFeeCodeConstants.IsRelatedToTariffNumber(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals(false, CusFeeCodeConstants.IsRelatedToTariffNumber(Core.Constants.USCustoms.FeeCodes.OtherAgencies));

			AssertEquals(true, CusFeeCodeConstants.IsRelatedToTariffNumber(Core.Constants.USCustoms.FeeCodes.Sorghum));
		}

		public void TestIsLineLevel62Recored()
		{
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Pork));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Beef));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Honey));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Raspberry));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Potato));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Sugar));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.FreshLimes));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Mushroom));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Watermelon));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber));

			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Blueberry));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Avocado));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Mango));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Sorghum));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.DairyFee));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals(false, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Coffee));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.Pecan));
			AssertEquals(true, CusFeeCodeConstants.IsLineLevel62Record(Core.Constants.USCustoms.FeeCodes.ChristmasTree));
		}

		public void TestOtherFeeCodesToExcludeForACEDrawback()
		{
			AssertEquals(true, CusFeeCodeConstants.OtherFeeCodesToExcludeForACEDrawback(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(true, CusFeeCodeConstants.OtherFeeCodesToExcludeForACEDrawback(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals(true, CusFeeCodeConstants.OtherFeeCodesToExcludeForACEDrawback(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals(false, CusFeeCodeConstants.OtherFeeCodesToExcludeForACEDrawback(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal));
			AssertEquals(false, CusFeeCodeConstants.OtherFeeCodesToExcludeForACEDrawback(Core.Constants.USCustoms.FeeCodes.DutiableMail));
		}
	}

	public static class CusFeeCodeConstantsTestHelper
	{
		public static void CreateCusFeeCodeDescriptionPairListForTest()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, RefCusCodeListTypes.Codes.AccountingClassFeeCode);

			new[] { Core.Constants.USCustoms.FeeCodes.Avocado, Core.Constants.USCustoms.FeeCodes.Beef, Core.Constants.USCustoms.FeeCodes.Blueberry,
				Core.Constants.USCustoms.FeeCodes.Coffee, Core.Constants.USCustoms.FeeCodes.Cotton, Core.Constants.USCustoms.FeeCodes.DairyFee,
				Core.Constants.USCustoms.FeeCodes.DistilledSpirits, Core.Constants.USCustoms.FeeCodes.DutiableMail, Core.Constants.USCustoms.FeeCodes.FreshLimes,
				Core.Constants.USCustoms.FeeCodes.HMF, Core.Constants.USCustoms.FeeCodes.Honey, Core.Constants.USCustoms.FeeCodes.Mango,
				Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge,
				Core.Constants.USCustoms.FeeCodes.Mushroom, Core.Constants.USCustoms.FeeCodes.OtherAgencies, Core.Constants.USCustoms.FeeCodes.OtherExcise,
				Core.Constants.USCustoms.FeeCodes.Pork, Core.Constants.USCustoms.FeeCodes.Potato, Core.Constants.USCustoms.FeeCodes.Raspberry,
				Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, Core.Constants.USCustoms.FeeCodes.Sorghum, Core.Constants.USCustoms.FeeCodes.Sugar,
				Core.Constants.USCustoms.FeeCodes.Tobacco, Core.Constants.USCustoms.FeeCodes.Watermelon, Core.Constants.USCustoms.FeeCodes.Wines }.
				ForEach(x => helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode, x, x + " Desc from DB", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6)));
			factory.Save();
		}
	}

	public class ExciseTaxListForTesting : ZArchitecture.Core.CodeDescriptionPairList
	{
		public ExciseTaxListForTesting()
		{
			AddPair(Codes.DistilledSpirits, Descriptions.DistilledSpirits);
			AddPair(Codes.Wines, Descriptions.Wines);
			AddPair(Codes.Tobacco, Descriptions.Tobacco);
			AddPair(Codes.OtherExcise, Descriptions.OtherExcise);
		}

		public static class Codes
		{
			public const string DistilledSpirits = "016";
			public const string Wines = "017";
			public const string Tobacco = "018";
			public const string OtherExcise = "022";
		}

		public static class Descriptions
		{
			public const string DistilledSpirits = "016";
			public const string Wines = "017";
			public const string Tobacco = "018";
			public const string OtherExcise = "022";
		}
	}
}
