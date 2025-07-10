using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CustomsRuleValidation : CusPermitHeaderValidation
	{
		public CustomsRuleValidation(CustomsRule parent) : base(parent)
		{
		}

		protected override void CheckCPH_OH_PermitHolder()
		{
			base.CheckCPH_OH_PermitHolder();

			if (ExistsCustomsRuleUniqueIndex())
			{
				Parent.CPH_OH_PermitHolderInfo.AddError(CustomsRuleUniqueIndexExists);
			}
		}

		protected override void CheckCPH_EndDate()
		{
			base.CheckCPH_EndDate();

			if (!Parent.CPH_StartDate.IsEmpty && !Parent.CPH_EndDate.IsEmpty && Parent.CPH_EndDate < Parent.CPH_StartDate)
			{
				Parent.CPH_EndDateInfo.AddError(StartDateEarlierThanOrEqualToEndDate);
			}
		}

		protected override void CheckCPH_StartDate()
		{
			base.CheckCPH_StartDate();

			if (ExistsCustomsRuleUniqueIndex())
			{
				Parent.CPH_StartDateInfo.AddError(CustomsRuleUniqueIndexExists);
			}

			if (!Parent.CPH_StartDate.IsEmpty)
			{
				var overlappedCustomsRules = GetOverlappedCustomsRules();
				if (overlappedCustomsRules.Length > 0)
				{
					var ruleInfos = overlappedCustomsRules.Cast<CustomsRule>()
						.Select(x => string.Format(CustomsRuleInfo,
						x.HumanReadableName,
						x.CPH_StartDate.ToISO8601ShortDateString(),
						x.CPH_EndDate.ToISO8601ShortDateString())).ToArray();
					Parent.CPH_StartDateInfo.AddError(string.Format(OverlappedCustomsRules, string.Join(",\n", ruleInfos)));
				}

				if (!Parent.CPH_EndDate.IsEmpty && Parent.CPH_EndDate < Parent.CPH_StartDate)
				{
					Parent.CPH_StartDateInfo.AddError(StartDateEarlierThanOrEqualToEndDate);
				}
			}
		}

		bool ExistsCustomsRuleUniqueIndex()
		{
			var searchQuery = Parent.Factory.GetCachedValue(string.Format("GetCustomsRuleBaseQuery|{0}|{1}|{2}|{3}", Parent.CPH_OH_PermitHolder, Parent.CPH_RN_NKCountryCode, Parent.PK, Parent.CPH_StartDate.ToShortDateString()), () =>
			{
				var query = new ZDBOnlyQuery(typeof(CustomsRule));
				query.AddEmptyAsNullToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, Parent.CPH_OH_PermitHolder);
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Parent.CPH_RN_NKCountryCode);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Number, ZString.Empty);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, CustomsRule.CustomsRuleType);
				query.AddToFilter(CusPermitHeaderSchema.CPH_SubType, ZString.Empty);
				query.AddToFilter(CusPermitHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, Parent.CPH_StartDate);
				query.ReLoadExistingRows = true;
				return query;
			});
			return Parent.Factory.Exists(typeof(CustomsRule), searchQuery);
		}

		public string CustomsRuleUniqueIndexExists = Res.GetString("E37230D6-2D18-4E2F-9F71-59BE4072E727", "There is already a Customs Rule (maybe in inactive status) that exits with the same \"Permit Holder\" and \"Start Date\".\nPlease change the \"Permit Holder\" or \"Start Date\" or modify the already existing Customs Rule.");

		CustomsRule[] GetOverlappedCustomsRules()
		{
			var searchQuery = Parent.Factory.GetCachedValue(string.Format("GetCustomsRuleBaseQuery|{0}|{1}|{2}|{3}|{4}", Parent.CPH_OH_PermitHolder, Parent.CPH_RN_NKCountryCode, Parent.PK, Parent.CPH_StartDate.ToShortDateString(), Parent.CPH_EndDate.ToShortDateString()), () =>
			{
				var query = new ZDBOnlyQuery(typeof(CustomsRule));
				query.AddEmptyAsNullToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, Parent.CPH_OH_PermitHolder);
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Parent.CPH_RN_NKCountryCode);
				query.AddToFilter(CusPermitHeaderSchema.CPH_GC_Company, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Number, ZString.Empty);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, CustomsRule.CustomsRuleType);
				query.AddToFilter(CusPermitHeaderSchema.CPH_SubType, ZString.Empty);
				query.AddToFilter(CusPermitHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var dateQuery = new ZQuery();
				var dateQuery1 = new ZQuery(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, Parent.CPH_StartDate);
				var endDateQuery1 = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, Parent.CPH_StartDate);
				endDateQuery1.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, DBNull.Value);
				dateQuery1.AddToFilter(endDateQuery1);

				var dateQuery2 = new ZQuery(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.GreaterThanOrEqualTo, Parent.CPH_StartDate);
				if (!Parent.CPH_EndDate.IsEmpty)
				{
					dateQuery2.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, Parent.CPH_EndDate);
				}

				dateQuery.AddToFilter(dateQuery1, JoinCondition.Or);
				dateQuery.AddToFilter(dateQuery2, JoinCondition.Or);
				query.AddToFilter(dateQuery);

				query.ReLoadExistingRows = true;

				return query;
			});
			return Parent.Factory.Load<CustomsRule>(searchQuery);
		}

		public string OverlappedCustomsRules = Res.GetString("5584F53F-1286-409E-A26C-8CD994D2B0B9", "Overlapping Customs Rules exist. Please adjust the start and end dates. Overlapping Customs Rules are:\n{0}");
		public string CustomsRuleInfo = Res.GetString("DFF185D3-1D91-43C4-A6E3-36091478CF80", "{0}: start from {1} to {2}");
		public string StartDateEarlierThanOrEqualToEndDate = Res.GetString("B020F03F-56B4-499B-9411-0A4D1B46E37F", "Start Date should be earlier than or equal to End Date.");
	}
}
