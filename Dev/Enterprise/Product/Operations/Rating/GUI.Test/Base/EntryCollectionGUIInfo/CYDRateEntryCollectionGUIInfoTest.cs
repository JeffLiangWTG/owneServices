using System;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	public sealed class CYDRateEntryCollectionGUIInfoTest : BaseRateEntryCollectionGUIInfoTest
	{
		protected override Type GUIInfoType => typeof(CYDRateEntryCollectionGUIInfo);
		protected override string RateCategory => RatingConstants.RateCategory.CYD;

		public override void TestClientRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(ClientRate), false);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_CYC_WW_Facility}
{RateEntry.Schema.TI_YardUnitType}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_YardUnitLoad}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.IsPublished}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}

		public override void TestQuotationRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(Quote), false);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_CYC_WW_Facility}
{RateEntry.Schema.TI_YardUnitType}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_YardUnitLoad}
{RateEntry.Schema.TI_DataChecked}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}

		public override void TestStandardCostingRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(Costing), false, true);
			var expectedForNonSupportUser = $@"
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_CYC_WW_Facility}
{RateEntry.Schema.TI_YardUnitType}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_YardUnitLoad}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.IsPublished}
";

			var extraColumnsForSupportUser = $@"
{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForNonSupportUser);
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForNonSupportUser, extraColumnsForSupportUser);
		}
	}
}
