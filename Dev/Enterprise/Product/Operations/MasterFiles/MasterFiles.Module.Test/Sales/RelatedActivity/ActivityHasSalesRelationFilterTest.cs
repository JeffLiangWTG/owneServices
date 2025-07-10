using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ActivityHasSalesRelationFilter))]
	sealed class ActivityHasSalesRelationFilterTest : HasSalesRelationFilterTestCase<ActivityHasSalesRelationFilter>
	{
		#region Constructor

		public override void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BoolProperty", true, Filter.BoolProperty);
				AssertEquals("TypeProperty", SalesRelationActivityFilterHelper.AnySalesRelationTypeCode, Filter.TypeProperty);
			});
		}

		#endregion

		#region Clear / IsEmpty

		public override void TestClear()
		{
			Filter.BoolProperty = false;
			Filter.TypeProperty = "OPP";
			Filter.BizObjPK = ZGuid.NewZGuid();

			Filter.Clear();

			CombineAssertions(() =>
			{
				AssertEquals("BoolProperty", true, Filter.BoolProperty);
				AssertEquals("TypeProperty", ZString.Empty, Filter.TypeProperty);
				AssertEquals("BizObjPK", ZGuid.Empty, Filter.BizObjPK);
			});
		}

		#endregion

		#region Query

		public override void TestGetQuery()
		{
			#region Test Data

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity4 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity5 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity6 = Factory.NewWithValidTestData<OrgOpportunity>();

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = Factory.NewWithValidTestData<OrgHeader>().PK;
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();

			(campaign as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity1);
			(campaign as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity2);
			opportunity1.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			opportunity3.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			opportunity4.RelatedChildActivityPivotCollection.AddNewPivot(opportunity5);

			/*
			 * CAM      OP4   OP6
			 * |  \      |
			 * |   \     |
			 * OP1 OP2  OP5
			 * |
			 * |
			 * INQ OP3
			 * |   /
			 * |  /
			 * COM
			 * 
			 * */

			Factory.Save();

			#endregion

			var filters = new ModuleFilterCollection();
			SalesRelationActivityFilterHelper.AddAllModuleFilters(Factory, filters, OrgOpportunitySchema.Instance, OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			var filter = (ActivityHasSalesRelationFilter)filters[SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation];
			AssertNotNull(filter);

			filter.IsActive = true;

			AssertHasSalesRelationFilterResults(filter, true, "", ZGuid.Empty, new[] { opportunity1, opportunity2, opportunity3, opportunity4, opportunity5, opportunity6 });
			AssertHasSalesRelationFilterResults(filter, true, SalesRelationActivityFilterHelper.AnySalesRelationTypeCode, ZGuid.Empty, new[] { opportunity1, opportunity2, opportunity3, opportunity4, opportunity5 });
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.CampaignManagement, ZGuid.Empty, new[] { opportunity1, opportunity2 });
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.Communication, ZGuid.Empty, new[] { opportunity1, opportunity3 });
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.InquiryManager, ZGuid.Empty, new[] { opportunity1 });
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.OpportunityManager, ZGuid.Empty, new[] { opportunity4, opportunity5 });

			AssertHasSalesRelationFilterResults(filter, false, "", ZGuid.Empty, new[] { opportunity1, opportunity2, opportunity3, opportunity4, opportunity5, opportunity6 });
			AssertHasSalesRelationFilterResults(filter, false, SalesRelationActivityFilterHelper.AnySalesRelationTypeCode, ZGuid.Empty, new[] { opportunity6 });
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.CampaignManagement, ZGuid.Empty, new[] { opportunity3, opportunity4, opportunity5, opportunity6 });
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.Communication, ZGuid.Empty, new[] { opportunity2, opportunity4, opportunity5, opportunity6 });
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.InquiryManager, ZGuid.Empty, new[] { opportunity2, opportunity3, opportunity4, opportunity5, opportunity6 });
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.OpportunityManager, ZGuid.Empty, new[] { opportunity1, opportunity2, opportunity3, opportunity6 });

			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.CampaignManagement, campaign.PK, new[] { opportunity1, opportunity2 });
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.CampaignManagement, campaign.PK, new[] { opportunity3, opportunity4, opportunity5, opportunity6 });
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.OpportunityManager, opportunity4.PK, new[] { opportunity5 });
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.OpportunityManager, opportunity5.PK, new[] { opportunity1, opportunity2, opportunity3, opportunity5, opportunity6 });
		}

		public void TestGetQuery_WithCascadingCampaign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCD";

			OrgContact contact = org.Contacts.AddNew();

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity4 = Factory.NewWithValidTestData<OrgOpportunity>();

			var previousCampaign = Factory.New<IGlbCompanyCampaign>();
			previousCampaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			previousCampaign.G0_CampaignID = "42";

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "43";

			BusinessObject campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = contact.PK;

			(previousCampaign as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(campaign as ISalesRelationActivity);
			(previousCampaign as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity2);
			(previousCampaign as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity3);
			((ISalesRelationActivity)campaignItem).RelatedChildActivityPivotCollection.AddNewPivot(opportunity1);

			/*
			 * CAM _______
			 * |  \      |
			 * |   \     |
			 * OP2 CAM  OP3
			 *       \
			 *        \
			 *        OP1
			 * */

			Factory.Save();

			var filters = new ModuleFilterCollection();
			SalesRelationActivityFilterHelper.AddAllModuleFilters(Factory, filters, OrgOpportunitySchema.Instance, OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			var filter = (ActivityHasSalesRelationFilter)filters[SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation];
			AssertNotNull(filter);

			filter.IsActive = true;

			AssertHasSalesRelationFilterResults(filter, true, "", ZGuid.Empty, new[] { opportunity1, opportunity2, opportunity3, opportunity4 });
			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.CampaignManagement, campaign.PK, new[] { opportunity1 });

			AssertHasSalesRelationFilterResults(filter, false, "", ZGuid.Empty, new[] { opportunity1, opportunity2, opportunity3, opportunity4 });
			AssertHasSalesRelationFilterResults(filter, false, RelatableActivityTypeList.Codes.CampaignManagement, campaign.PK, new[] { opportunity2, opportunity3, opportunity4 });

			var filters2 = new ModuleFilterCollection();
			SalesRelationActivityFilterHelper.AddAllModuleFilters(Factory, filters2, GlbCompanyCampaignSchema.Instance, GlbCompanyCampaignSchema.Constants.Prefix, GlbCompanyCampaignSchema.G0_SystemLastEditTimeUtc, typeof(IGlbCompanyCampaign));
			var campaignFilter = (ActivityHasSalesRelationFilter)filters2[SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation];
			AssertNotNull(campaignFilter);

			campaignFilter.IsActive = true;

			AssertHasSalesRelationFilterResults(campaignFilter, true, "", ZGuid.Empty, new[] { previousCampaign, campaign });
			AssertHasSalesRelationFilterResults(campaignFilter, true, RelatableActivityTypeList.Codes.OpportunityManager, opportunity1.PK, new[] { campaign, previousCampaign });

			AssertHasSalesRelationFilterResults(campaignFilter, false, "", ZGuid.Empty, new[] { previousCampaign, campaign });
			AssertHasSalesRelationFilterResults(campaignFilter, false, RelatableActivityTypeList.Codes.OpportunityManager, opportunity1.PK, new[] { previousCampaign });
		}

		public void TestGetQuery_OpportunityAttachingCampaign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCD";

			OrgContact contact = org.Contacts.AddNew();

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";

			BusinessObject campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = contact.PK;

			opportunity1.RelatedChildActivityPivotCollection.AddNewPivot((ISalesRelationActivity)campaignItem);

			Factory.Save();

			var filters = new ModuleFilterCollection();
			SalesRelationActivityFilterHelper.AddAllModuleFilters(Factory, filters, OrgOpportunitySchema.Instance, OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			var filter = (ActivityHasSalesRelationFilter)filters[SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation];
			AssertNotNull(filter);

			filter.IsActive = true;

			AssertHasSalesRelationFilterResults(filter, true, RelatableActivityTypeList.Codes.CampaignManagement, campaign.PK, new[] { opportunity1 });
		}

		public override void TestQueryGetsRefreshedWhenFilterIsReused()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity4 = Factory.NewWithValidTestData<OrgOpportunity>();

			var campaign1 = Factory.New<IGlbCompanyCampaign>();
			campaign1.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var campaign2 = Factory.New<IGlbCompanyCampaign>();
			campaign2.G0_CampaignName = ZGuid.NewZGuid().ToString();

			(campaign1 as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity1);
			(campaign2 as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity2);
			(campaign2 as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity4);

			/*
			 *  CAM1     CAM2      OP3
			 *  |        |  \
			 *  |        |   \
			 *  OP1     OP2  OP4
			 * 
			 * */

			Factory.Save();

			var filters = new ModuleFilterCollection();
			SalesRelationActivityFilterHelper.AddAllModuleFilters(Factory, filters, OrgOpportunitySchema.Instance, OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			var filter = (ActivityHasSalesRelationFilter)filters[SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation];
			filter.IsActive = true;
			filter.BoolProperty = ZBool.True;
			filter.TypeProperty = RelatableActivityTypeList.Codes.CampaignManagement;
			filter.BizObjPK = campaign1.PK;

			var filterCombiner = new ModuleFilterCombiner();
			Func<ZQuery> getQuery = () => filterCombiner.GetCombinedFilter(new[] { filter });

			var collection = Factory.Load<OrgOpportunity>(getQuery());
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgOpportunity>.PKOnlyComparer, new[] { opportunity1 }, collection);

			filter.BizObjPK = campaign2.PK;
			collection = Factory.Load<OrgOpportunity>(getQuery());
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgOpportunity>.PKOnlyComparer, new[] { opportunity2, opportunity4 }, collection);

			filter.BizObjPK = ZGuid.Empty;
			collection = Factory.Load<OrgOpportunity>(getQuery());
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgOpportunity>.PKOnlyComparer, new[] { opportunity1, opportunity2, opportunity4 }, collection);

			filter.BoolProperty = ZBool.False;
			collection = Factory.Load<OrgOpportunity>(getQuery());
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgOpportunity>.PKOnlyComparer, new[] { opportunity3 }, collection);
		}

		#endregion

		#region Properties

		public void TestBizObjPK()
		{
			var filters = new ModuleFilterCollection();
			SalesRelationActivityFilterHelper.AddAllModuleFilters(Factory, filters, OrgOpportunitySchema.Instance, OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			var filter = (ActivityHasSalesRelationFilter)filters[SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation];
			AssertNotNull(filter);

			filter.IsActive = true;

			filter.BizObjPK = ZGuid.NewZGuid();
			filter.BoolProperty = true;
			filter.TypeProperty = RelatableActivityTypeList.Codes.Communication;
			AssertNotEquals(ZGuid.Empty, filter.BizObjPK);
			AssertEquals(false, filter.BizObjPKInfo.ReadOnly);

			filter.BizObjPK = ZGuid.NewZGuid();
			filter.BoolProperty = true;
			filter.TypeProperty = RelatableActivityTypeList.Codes.CampaignManagement;
			AssertNotEquals(ZGuid.Empty, filter.BizObjPK);
			AssertEquals(false, filter.BizObjPKInfo.ReadOnly);

			filter.TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			AssertEquals(ZGuid.Empty, filter.BizObjPK);
			AssertEquals(true, filter.BizObjPKInfo.ReadOnly);

			filter.BizObjPK = ZGuid.NewZGuid();
			filter.TypeProperty = ZString.Empty;
			AssertEquals(ZGuid.Empty, filter.BizObjPK);
			AssertEquals(true, filter.BizObjPKInfo.ReadOnly);
		}

		#endregion

		public void TestErrorReportWhenBizObjCollectionIsNull()
		{
			var filter = new ActivityHasSalesRelationFilter(Factory, "mo", typeof(OrgOpportunity));
			_ = filter.BizObjCollection;
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Implementation

		void AssertHasSalesRelationFilterResults(HasSalesRelationFilter hasSalesRelationFilter, ZBool boolProperty, ZString typeProperty, ZGuid bizObjPK, OrgOpportunity[] expectedResult)
		{
			var activityHasSalesRelationFilter = (ActivityHasSalesRelationFilter)hasSalesRelationFilter;
			activityHasSalesRelationFilter.BoolProperty = boolProperty;
			activityHasSalesRelationFilter.TypeProperty = typeProperty;
			activityHasSalesRelationFilter.BizObjPK = bizObjPK;
			var filterCombiner = new ModuleFilterCombiner();
			var query = filterCombiner.GetCombinedFilter(new[] { activityHasSalesRelationFilter });
			var actualResult = Factory.Load<OrgOpportunity>(query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgOpportunity>.PKOnlyComparer, expectedResult, actualResult);
		}

		void AssertHasSalesRelationFilterResults(HasSalesRelationFilter hasSalesRelationFilter, ZBool boolProperty, ZString typeProperty, ZGuid bizObjPK, IGlbCompanyCampaign[] expectedResult)
		{
			var activityHasSalesRelationFilter = (ActivityHasSalesRelationFilter)hasSalesRelationFilter;
			activityHasSalesRelationFilter.BoolProperty = boolProperty;
			activityHasSalesRelationFilter.TypeProperty = typeProperty;
			activityHasSalesRelationFilter.BizObjPK = bizObjPK;
			var filterCombiner = new ModuleFilterCombiner();
			var query = filterCombiner.GetCombinedFilter(new[] { activityHasSalesRelationFilter });
			var actualResult = Factory.Load<IGlbCompanyCampaign>(query);
			AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
		}

		protected override ActivityHasSalesRelationFilter GetNewModuleFilter()
		{
			var filter = new ActivityHasSalesRelationFilter(Factory, "moo", typeof(OrgOpportunity));
			filter.SubGroup = new SalesRelationActivityFilterHelper.SalesRelationNodeSubGroup(OrgOpportunitySchema.Instance, typeof(OrgOpportunity));
			return filter;
		}

		#endregion
	}
}
