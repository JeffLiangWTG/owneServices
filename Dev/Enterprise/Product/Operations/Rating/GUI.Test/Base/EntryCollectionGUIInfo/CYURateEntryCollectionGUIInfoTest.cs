using System;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI.Testing
{
	public sealed class CYURateEntryCollectionGUIInfoTest : BaseRateEntryCollectionGUIInfoTest
	{
		protected override Type GUIInfoType => typeof(CYURateEntryCollectionGUIInfo);
		protected override string RateCategory => RatingConstants.RateCategory.CYU;

		public override void TestClientRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(ClientRate), false);
			var expectedForSupportUser = $@"
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_CYC_WW_Facility}
{RateEntry.Schema.TI_YardUnitType}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_YardUnitLoad}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.IsPublished}
{RateEntry.Schema.TI_CreationSource}
{RateEntry.Schema.TI_OH_ControllingCustomer}
";

			var expectedNotForNormalUser = $@"{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info, expectedForSupportUser, expectedNotForNormalUser);
		}

		public override void TestQuotationRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(Quote), false);
			var expectedForSupportUser = $@"
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_CYC_WW_Facility}
{RateEntry.Schema.TI_YardUnitType}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_YardUnitLoad}
{RateEntry.Schema.TI_DataChecked}
{RateEntry.Schema.TI_CreationSource}
{RateEntry.Schema.TI_OH_ControllingCustomer}
";

			var expectedNotForNormalUser = $@"{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info, expectedForSupportUser, expectedNotForNormalUser);
		}

		public override void TestStandardCostingRateColumns()
		{
			var info = GetInfos(RateCategory, typeof(Costing), false, true);
			var expectedForSupportUser = $@"
{RateEntry.Schema.TI_Mode}
{RateEntry.Schema.TI_CYC_WW_Facility}
{RateEntry.Schema.TI_YardUnitType}
{RateEntry.Schema.TI_RC}
{RateEntry.Schema.TI_MatchContainerRateClass}
{RateEntry.Schema.TI_YardUnitLoad}
{RateEntry.Schema.TI_RateStartDate}
{RateEntry.Schema.TI_RateEndDate}
{RateEntry.Schema.IsPublished}
{RateEntry.Schema.TI_CreationSource}
{RateEntry.Schema.TI_OH_ControllingCustomer}
";

			var expectedNotForNormalUser = $@"{RateEntry.Schema.TI_CreationSource}
";

			AssertCollectionColumnsExistInExpectedOrder(info, expectedForSupportUser, expectedNotForNormalUser);
		}

		void AssertCollectionColumnsExistInExpectedOrder((string[] NormalUserColumns, string[] SupportUserColumns, RateEntryCollectionGUIInfo Infos) info, string expectedForSupportUser, string expectedNotForNormalUser)
		{
			AssertCollectionColumnsExistInExpectedOrder(info.NormalUserColumns, expectedForSupportUser.Replace(expectedNotForNormalUser, string.Empty));
			AssertCollectionColumnsExistInExpectedOrder(info.SupportUserColumns, expectedForSupportUser);
		}
	}
}
