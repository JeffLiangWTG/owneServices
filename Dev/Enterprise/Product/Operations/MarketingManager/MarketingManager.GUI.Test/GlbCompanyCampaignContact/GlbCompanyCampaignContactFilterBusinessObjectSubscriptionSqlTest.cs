using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignContactFilterBusinessObject))]
	class GlbCompanyCampaignContactFilterBusinessObjectSubscriptionSqlTest : GlbCompanyCampaignContactFilterBusinessObjectTestBase
	{
		#region Subscribe

		public void TestErrorQueryFilter()
		{
			var filter = (ModuleTextFilter)CampaignFilter["SubscriptionStatus"];
			AssertNotNull(filter);
			filter.Property = "aaa";
			AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
			AssertMultilineASCIIEquals("SQL query should be empty", "", filter.Query.LiteralTextADOFormatted);
			AssertEquals(true, filter.Query.IsNoResultQuery);
		}

		public void TestSubscribedOnlyFilter()
		{
			var filter = (ModuleTextFilter)CampaignFilter[GlbCompanyCampaignContactFilterBusinessObject.SubscriptionFilterCode];
			AssertNotNull(filter);
			filter.Property = "SUB";
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			AssertSubscribeFilter(filter, true, true);
		}

		public void TestUnsubscribedOnlyFilter()
		{
			var filter = (ModuleTextFilter)CampaignFilter[GlbCompanyCampaignContactFilterBusinessObject.SubscriptionFilterCode];
			AssertNotNull(filter);
			filter.Property = "UNS";
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			AssertSubscribeFilter(filter, false, true);
		}

		public void TestNotUnsubscribedFilter()
		{
			var filter = (ModuleTextFilter)CampaignFilter[GlbCompanyCampaignContactFilterBusinessObject.SubscriptionFilterCode];
			AssertNotNull(filter);
			filter.Property = "UNS";
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			AssertSubscribeFilter(filter, false, false);
		}

		public void TestNotSubscribedFilter()
		{
			var filter = (ModuleTextFilter)CampaignFilter[GlbCompanyCampaignContactFilterBusinessObject.SubscriptionFilterCode];
			AssertNotNull(filter);
			filter.Property = "SUB";
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			AssertSubscribeFilter(filter, true, false);
		}

		public void TestDefaultFilter()
		{
			var filter = (ModuleTextFilter)CampaignFilter[GlbCompanyCampaignContactFilterBusinessObject.SubscriptionFilterCode];
			AssertSubscribeFilter(filter, false, false);
		}

		static void AssertSubscribeFilter(ModuleTextFilter filter, bool sendOnlyToSubscribed, bool orCondition)
		{
			AssertNotNull(filter);
			AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);

			var firstQueryPart = $@"VCC_Email <> '' 
AND
(
	VCC_Email {(orCondition ? "IN" : "NOT IN")} 
	(
		SELECT GCS_Email FROM dbo.GlbCompanyCampaignSubscription WHERE {GetSubscriptionFilterFormat(sendOnlyToSubscribed, 2)}
	)
)";

			// query optimiser removes braces when condition is OR so we surround query and move it a tab right
			var expectedQuery = new StringBuilder(!orCondition
				? $"{string.Join("\r\n", firstQueryPart.Split(new[] { "\r\n" }, StringSplitOptions.None))}\r\n"
				: $"{firstQueryPart}\r\n");

			expectedQuery.AppendLine($@"{(orCondition ? "OR" : "AND")}
(
	(
		VCC_Email <> '' 
		AND
		(
			VCC_PK {(orCondition ? "IN" : "NOT IN")} 
			(
				SELECT OC_PK FROM dbo.OrgContact WHERE 
				(
					OC_OH IN 
					(
						SELECT GCS_OH FROM dbo.GlbCompanyCampaignSubscription WHERE GCS_OH IS NOT NULL 
						AND
						{GetSubscriptionFilterFormat(sendOnlyToSubscribed, 6)}
					)
				)
				AND
				(
					OC_Email NOT IN 
					(
						SELECT GCS_Email FROM dbo.GlbCompanyCampaignSubscription WHERE {GetSubscriptionFilterFormat(!sendOnlyToSubscribed, 6)}
					)
				)
			)
		)
	)
	{(orCondition ? "OR" : "AND")}
	(
		VCC_Email <> '' 
		AND
		(
			VCC_Email {(orCondition ? "IN" : "NOT IN")} 
			(
				SELECT O1_Email FROM dbo.OrgColdCallRegister WHERE 
				(
					O1_OH_ConvertedToQualifiedLead IN 
					(
						SELECT GCS_OH FROM dbo.GlbCompanyCampaignSubscription WHERE GCS_OH IS NOT NULL 
						AND
						{GetSubscriptionFilterFormat(sendOnlyToSubscribed, 6)}
					)
				)
				AND
				(
					O1_Email NOT IN 
					(
						SELECT GCS_Email FROM dbo.GlbCompanyCampaignSubscription WHERE {GetSubscriptionFilterFormat(!sendOnlyToSubscribed, 6)}
					)
				)
			)
		)
	)
)");

			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertMultilineASCIIEquals("SQL query should match", expectedQuery.ToString(), filter.Query.LiteralTextADOFormatted);
			}
		}

		static string GetSubscriptionFilterFormat(ZBool sendOnlyToSubscribed, int tabOffest)
		{
			var initialString = $@"GCS_IsSubscribed = {(sendOnlyToSubscribed ? 1 : 0)} 
AND
(
	(
		GCS_MediaCategory in 
		(
			'', 'Cat'
		)
	)
	AND
	(
		GCS_MediaType in 
		(
			'', 'Type'
		)
	)
)";
			if (tabOffest <= 0)
			{
				return initialString;
			}
			var tabString = new string('\t', tabOffest);
			return string.Join("\r\n", initialString.Split(new[] { "\r\n" }, StringSplitOptions.None).Select((s, i) => i > 0 ? tabString + s : s));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Campaign.G0_Category = "Cat";
			Campaign.G0_Type = "Type";
		}

		#endregion
	}
}
