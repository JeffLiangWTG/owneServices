using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CustomsRule : CommonCusPermitHeader, Integration.Customs.ICustomsRule
	{
		public CustomsRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public const string CustomsRuleType = "RUL";

		#region Override

		protected override ZString HumanReadableNameCore
			=> Res.GetString("26CF316B-8ABF-4FDC-BDC7-4B3DECDF2E57", "Rule {0} - {1}", PermitHolder?.OH_Code ?? ZString.Empty, CPH_PermitDescription);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			this.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Rule;
			this.CPH_Type = CustomsRuleType;
			this.CPH_GC_Company = GlbCompany.CurrentCompany.PK;
			this.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		public override ZString ShortName => Res.GetString("4348DC7E-79B9-4D75-AE63-84E8F6052A19", "Rule");

		public override ZGuid CPH_OH_PermitHolder
		{
			get => base.CPH_OH_PermitHolder;
			set
			{
				base.CPH_OH_PermitHolder = value;
				Validation.ValidateCPH_StartDate();
			}
		}

		public override ZDate CPH_StartDate
		{
			get => base.CPH_StartDate;
			set
			{
				base.CPH_StartDate = value;
				Validation.ValidateCPH_OH_PermitHolder();
			}
		}

		public override ZDate CPH_EndDate
		{
			get => base.CPH_EndDate;
			set
			{
				base.CPH_EndDate = value;
				Validation.ValidateCPH_StartDate();
			}
		}

		public override void Delete()
		{
			if (!IsDeleted && CanDelete)
			{
				Rules.DeleteAll();
			}

			base.Delete();
		}

		#endregion

		#region New Properties

		public ZString OrganizationDescription
		{
			get
			{
				var result = Res.GetString("3090ACED-E50D-42B0-AAB5-A8CA5BE8F03D", "Apply to All");

				if (PermitHolder is OrgHeader permitHolder)
				{
					result = permitHolder.OH_FullName;
				}
				else if (!CPH_OH_PermitHolder.IsEmpty && !CPH_OH_PermitHolder.IsValid)
				{
					result = Core.Constants.FindBoxMessages.InvalidSelection;
				}

				return result;
			}
		}

		#endregion

		#region Rules

		[ChildEditable(true)]
		public CustomsRuleRuleCollection Rules
		{
			get
			{
				if (rules == null)
				{
					rules = new CustomsRuleRuleCollection(this);
					RegisterEditableChildObject(rules);
				}
				return rules;
			}
		}
		CustomsRuleRuleCollection rules;

		#endregion

		protected override CusPermitHeaderValidation GetNewValidation()
		{
			return new CustomsRuleValidation(this);
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public static CustomsRule Load(BusinessObjectFactory factory, ZDate filterDate, OrgHeader importer, ZString countryCode)
			{
				return factory.GetCachedValue($"CustomsRule|{filterDate.ToISO8601ShortDateString()}|{importer?.OH_Code}|{countryCode}", () =>
				{
					CustomsRule result = null;
					if (filterDate.IsValid)
					{
						var query = new ZDBOnlyQuery(typeof(CustomsRule));
						query.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Rule);
						query.AddToFilter(CusPermitHeaderSchema.CPH_Type, CustomsRule.CustomsRuleType);
						query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCode);
						query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, filterDate);

						var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, null);
						endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, filterDate);
						query.AddToFilter(endDateQuery);

						var permitHolderQuery = new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, null);
						if (importer != null)
						{
							permitHolderQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_OH_PermitHolder, importer.PK);
						}
						query.AddToFilter(permitHolderQuery);

						query.OrderBy = CusPermitHeaderSchema.Constants.CPH_OH_PermitHolder + OrderByClause.Descending + ", " + CusPermitHeaderSchema.Constants.CPH_StartDate + OrderByClause.Descending;

						result = factory.LoadTop1<CustomsRule>(query);
					}
					return result;
				});
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CustomsRule);
			}
		}
	}
}
