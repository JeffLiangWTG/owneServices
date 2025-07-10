using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Rating.Business.Testing
{
	public class RateAttachmentComparerTest : TestCaseWithFactory
	{
		public void TestCompareWithSamePages()
		{
			TestCompare("", 1, "", 2, true);
			TestCompare(RatingConstants.DocTemplateTypes.CoverPage, 1, RatingConstants.DocTemplateTypes.CoverPage, 2, true);
			TestCompare(RatingConstants.DocTemplateTypes.StandardPricingPage, 1, RatingConstants.DocTemplateTypes.StandardPricingPage, 2, true);
			TestCompare(RatingConstants.DocTemplateTypes.OneOffPricingPage, 1, RatingConstants.DocTemplateTypes.OneOffPricingPage, 2, true);
			TestCompare(RatingConstants.DocTemplateTypes.TrailingPage, 1, RatingConstants.DocTemplateTypes.TrailingPage, 2, true);
		}

		public void TestCompareWithDifferingPages()
		{
			TestCompare(RatingConstants.DocTemplateTypes.CoverPage, 1, RatingConstants.DocTemplateTypes.StandardPricingPage, 1, true);
			TestCompare(RatingConstants.DocTemplateTypes.CoverPage, 1, RatingConstants.DocTemplateTypes.OneOffPricingPage, 1, true);
			TestCompare(RatingConstants.DocTemplateTypes.StandardPricingPage, 1, RatingConstants.DocTemplateTypes.TrailingPage, 1, true);
			TestCompare(RatingConstants.DocTemplateTypes.OneOffPricingPage, 1, RatingConstants.DocTemplateTypes.TrailingPage, 1, true);
			TestCompare(RatingConstants.DocTemplateTypes.CoverPage, 1, RatingConstants.DocTemplateTypes.TrailingPage, 1, true);
		}

		public void TestComparePricingPages()
		{
			TestCompare(RatingConstants.DocTemplateTypes.StandardPricingPage, 1, RatingConstants.DocTemplateTypes.OneOffPricingPage, 2, true);
			TestCompare(RatingConstants.DocTemplateTypes.StandardPricingPage, 2, RatingConstants.DocTemplateTypes.OneOffPricingPage, 1, false);
		}

		public void TestEquals()
		{
			((IBusinessObjectInternals)Set1).IsCopying = true;
			((IBusinessObjectInternals)Set2).IsCopying = true;
			Set1.TS_TemplateType = RatingConstants.DocTemplateTypes.StandardPricingPage;
			Set1.TS_Sequence = 1;
			Set2.TS_TemplateType = RatingConstants.DocTemplateTypes.StandardPricingPage;
			Set2.TS_Sequence = 1;
			((IBusinessObjectInternals)Set1).IsCopying = false;
			((IBusinessObjectInternals)Set2).IsCopying = false;
			AssertEquals("Both are equal", 0, Comparer.Compare(Set1, Set2));

			((IBusinessObjectInternals)Set1).IsCopying = true;
			((IBusinessObjectInternals)Set2).IsCopying = true;
			Set1.TS_TemplateType = "";
			Set1.TS_Sequence = 1;
			Set2.TS_TemplateType = "";
			Set2.TS_Sequence = 1;
			((IBusinessObjectInternals)Set1).IsCopying = false;
			((IBusinessObjectInternals)Set2).IsCopying = false;
			AssertEquals("Both are still equal", 0, Comparer.Compare(Set1, Set2));
		}

		void TestCompare(ZString templateType1, int sequence1, ZString templateType2, int sequence2, bool isLessThan)
		{
			Set1.TS_Sequence = (ZShort)sequence1;
			Set2.TS_Sequence = (ZShort)sequence2;
			Set1.TS_TemplateType = templateType1;
			Set2.TS_TemplateType = templateType2;

			if (isLessThan)
			{
				Assert("1 comes before 2", Comparer.Compare(Set1, Set2) < 0);
				Assert("2 comes after 1", Comparer.Compare(Set2, Set1) > 0);
			}
			else
			{
				Assert("2 comes before 1", Comparer.Compare(Set1, Set2) > 0);
				Assert("1 comes after 2", Comparer.Compare(Set2, Set1) < 0);
			}
		}

		#region Implementation

		readonly RateAttachmentComparer Comparer = new RateAttachmentComparer();
		RateAttachmentSet Set1;
		RateAttachmentSet Set2;

		protected override void SetUp()
		{
			base.SetUp();
			Set1 = Factory.New<RateAttachmentSet>();
			Set2 = Factory.New<RateAttachmentSet>();
		}

		#endregion
	}
}
