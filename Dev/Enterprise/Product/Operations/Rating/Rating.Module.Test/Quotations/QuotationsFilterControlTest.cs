using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.Module.Test.Quotations
{
	public class QuotationsFilterControlTest : TestCaseWithFactory
	{
		public void TestWorkflowCustomFieldColums()
		{
			QuotationTestHelper.CreateQuotationWorkflowWithCustomFields(Factory);
			using (QuotationsFilterControl control = new QuotationsFilterControl(new QuoteCollection(Factory), new QuotationsFilterBusinessObject()))
			{
				string[] workflowColumns = control.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>()
					.Where(col => !string.IsNullOrEmpty(col.Caption) && col.Caption.StartsWith("custom ", StringComparison.Ordinal))
					.Select(col => col.Caption).ToArray();

				AssertContainsExactElementsInAnyOrder(new[] { "custom text", "custom int", "custom decimal", "custom datetime" }, workflowColumns);
			}
		}

		public void TestFollowUpDateColumnExists()
		{
			QuotationTestHelper.CreateQuotationWorkflowWithCustomFields(Factory);

			using (QuotationsFilterControl control = new QuotationsFilterControl(new QuoteCollection(Factory), new QuotationsFilterBusinessObject()))
			{
				var filterColumns = control.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(col => col.ColumnName).ToList();

				AssertContainsExactElementsInAnyOrder
				(
					filterColumns,
					new[]
					{
						AutoRatingHeader.Schema.TH_FollowUpDate,
						RatingHeader.Schema.TH_ClientCode,
						RatingHeader.Schema.TH_ClientFullName,
						AutoRatingHeader.Schema.TH_QuoteNumber,
						AutoRatingHeader.Schema.TH_QuoteDate,
						AutoRatingHeader.Schema.TH_QuoteEndDate,
						"QuoteStatus",
						RatingHeader.Schema.TH_StatusDate,
						"Header+OverallSalesRepStaff",
						"Header+OH_RL_NKClosestPort",
						AutoRatingHeader.Schema.TH_QuoteCancellationReason,
						"QuoteCancellationReasonDescription",
						AutoRatingHeader.Schema.TH_SystemCreateUser,
						AutoRatingHeader.Schema.TH_SystemCreateBranch,
						AutoRatingHeader.Schema.TH_SystemCreateDepartment,
						AutoRatingHeader.Schema.TH_SystemCreateTimeUtc,
						AutoRatingHeader.Schema.TH_SystemLastEditUser,
						AutoRatingHeader.Schema.TH_SystemLastEditTimeUtc,
						"__CUSTOM DATETIME__prop__ZDateTime",
						"__CUSTOM DECIMAL__prop__ZDecimal",
						"__CUSTOM INT__prop__ZInt",
						"__CUSTOM TEXT__prop__ZString"
					}
				);
			}
		}
	}
}
