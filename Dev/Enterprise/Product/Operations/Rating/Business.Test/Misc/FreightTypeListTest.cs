using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class FreightTypeListTest : TestCase
	{
		public void TestCodeFromRateMode_Category()
		{
			AssertCode(FreightTypeList.Codes.Air, RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD);
			AssertCode(FreightTypeList.Codes.Air, RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE);

			AssertCode(FreightTypeList.Codes.Air, RatingConstants.RateCategory.CAI, Core.Constants.RateMode.ULD);
			AssertCode(FreightTypeList.Codes.Air, RatingConstants.RateCategory.CAI, Core.Constants.RateMode.LSE);

			AssertCode(FreightTypeList.Codes.FCL, RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);
			AssertCode(FreightTypeList.Codes.FCL_Road, RatingConstants.RateCategory.FCL, Core.Constants.RateMode.ROA);
			AssertCode(FreightTypeList.Codes.FCL_Rail, RatingConstants.RateCategory.FCL, Core.Constants.RateMode.RAI);

			AssertCode(FreightTypeList.Codes.FCL, RatingConstants.RateCategory.CFC, Core.Constants.RateMode.SEA);
			AssertCode(FreightTypeList.Codes.FCL_Road, RatingConstants.RateCategory.CFC, Core.Constants.RateMode.ROA);
			AssertCode(FreightTypeList.Codes.FCL_Rail, RatingConstants.RateCategory.CFC, Core.Constants.RateMode.RAI);

			AssertCode(FreightTypeList.Codes.Sea, RatingConstants.RateCategory.LCL, Core.Constants.RateMode.SEA);
			AssertCode(FreightTypeList.Codes.Road, RatingConstants.RateCategory.LCL, Core.Constants.RateMode.ROA);
			AssertCode(FreightTypeList.Codes.Rail, RatingConstants.RateCategory.LCL, Core.Constants.RateMode.RAI);

			AssertCode(FreightTypeList.Codes.Sea, RatingConstants.RateCategory.CLC, Core.Constants.RateMode.SEA);
			AssertCode(FreightTypeList.Codes.Road, RatingConstants.RateCategory.CLC, Core.Constants.RateMode.ROA);
			AssertCode(FreightTypeList.Codes.Rail, RatingConstants.RateCategory.CLC, Core.Constants.RateMode.RAI);

			AssertCode(FreightTypeList.Codes.FCL, RatingConstants.RateCategory.SCO, Core.Constants.RateMode.SEA);
			AssertCode(FreightTypeList.Codes.Sea, RatingConstants.RateCategory.SNC, Core.Constants.RateMode.SEA);
			AssertCode(FreightTypeList.Codes.FCL, RatingConstants.RateCategory.SID, Core.Constants.RateMode.SEA);
			AssertCode(FreightTypeList.Codes.FCL, RatingConstants.RateCategory.SED, Core.Constants.RateMode.SEA);

			AssertCode(null, RatingConstants.RateCategory.UNP, Core.Constants.RateMode.MAI);
		}

		public void TestCodeFromRateMode_HasContainers()
		{
			AssertCode(FreightTypeList.Codes.Air, false, Core.Constants.RateMode.LSE);
			AssertCode(FreightTypeList.Codes.Air, false, Core.Constants.RateMode.ULD);
			AssertCode(FreightTypeList.Codes.Air, false, Core.Constants.RateMode.COU);

			AssertCode(FreightTypeList.Codes.FCL, true, Core.Constants.RateMode.SEA);
			AssertCode(FreightTypeList.Codes.Sea, false, Core.Constants.RateMode.SEA);
			AssertCode(FreightTypeList.Codes.Sea, false, Core.Constants.RateMode.LCL);
			AssertCode(FreightTypeList.Codes.FCL, true, Core.Constants.RateMode.FCL);

			AssertCode(FreightTypeList.Codes.Road, false, Core.Constants.RateMode.ROA);
			AssertCode(FreightTypeList.Codes.Road, false, Core.Constants.RateMode.LRO);
			AssertCode(FreightTypeList.Codes.FCL_Road, true, Core.Constants.RateMode.FRO);
			AssertCode(FreightTypeList.Codes.FTL_Road, false, Core.Constants.RateMode.FTL);

			AssertCode(FreightTypeList.Codes.Rail, false, Core.Constants.RateMode.RAI);
			AssertCode(FreightTypeList.Codes.Rail, false, Core.Constants.RateMode.LRA);
			AssertCode(FreightTypeList.Codes.FCL_Rail, false, Core.Constants.RateMode.FRA);
			AssertCode(FreightTypeList.Codes.FTL_Rail, false, Core.Constants.RateMode.FWL);

			AssertCode(null, false, Core.Constants.RateMode.MAI);
		}

		#region Implementation

		void AssertCode(string expected, string rateCategory, string rateMode)
		{
			AssertEquals(string.Format("CodeFromRateMode(\"{0}\", \"{1}\")", rateCategory, rateMode), expected, FreightTypeList.CodeFromRateMode(rateCategory, rateMode));
		}

		void AssertCode(string expected, bool hasContainers, string rateMode)
		{
			AssertEquals(string.Format("CodeFromRateMode({0}, \"{1}\")", hasContainers ? "true" : "false", rateMode), expected, FreightTypeList.CodeFromRateMode(hasContainers, rateMode));
		}

		#endregion
	}
}
