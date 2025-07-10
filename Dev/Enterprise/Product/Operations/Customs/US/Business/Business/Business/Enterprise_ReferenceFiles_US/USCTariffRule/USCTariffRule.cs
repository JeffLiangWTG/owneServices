using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public sealed class USCTariffRule : AutoUSCTariffRule
	{
		public USCTariffRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoUSCTariffRule.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public USCTariffRule[] LoadDuplicates(ZString ruleCode, ZString tariffNumberFrom, ZString tariffNumberTo, ZDateTime dateFrom, ZDateTime dateTo)
			{
				var query = GetTariffQuery(tariffNumberFrom, tariffNumberTo);
				query.AddToFilter(USCTariffRuleSchema.U1_RuleCode, ruleCode);

				var rules = Factory.Load<USCTariffRule>(query);
				var result = new List<USCTariffRule>();
				foreach (var rule in rules)
				{
					if (rule.U1_DateFrom <= dateFrom && (rule.U1_DateTo >= dateTo || rule.U1_DateTo.IsEmpty || dateTo.IsEmpty))
					{
						result.Add(rule);
					}
				}

				return result.ToArray();
			}

			public static ZQuery GetTariffRange(ZString tariffNumber)
			{
				var query = new ZQuery();

				var nonSTNQuery = new ZQuery(USCTariffRuleSchema.U1_RuleCode, SQLComparisonOperator.NotEqual, TariffRuleList.Codes.EligibleForSecondaryTariffNumbers);
				nonSTNQuery.AddToFilter(GetTariffQuery(tariffNumber, tariffNumber));
				query.AddToFilter(nonSTNQuery, JoinCondition.Or);

				var sTNQuery = new ZQuery(USCTariffRuleSchema.U1_RuleCode, SQLComparisonOperator.Equal, TariffRuleList.Codes.EligibleForSecondaryTariffNumbers);
				sTNQuery.AddToFilter(USCTariffRuleSchema.U1_Tariff, tariffNumber);
				query.AddToFilter(sTNQuery, JoinCondition.Or);

				return query;
			}

			internal static ZQuery GetTariffQuery(ZString tariffNumberFrom, ZString tariffNumberTo)
			{
				var tariffsFrom = new List<ZString>();
				for (int index = 2; index <= tariffNumberFrom.Length; index = index + 2)
				{
					tariffsFrom.Add(tariffNumberFrom.Left(index));
				}

				var tariffsTo = new List<ZString>();
				for (int index = 2; index <= tariffNumberTo.Length; index = index + 2)
				{
					tariffsTo.Add(tariffNumberTo.Left(index));
				}

				var partialTariffQuery = new ZQuery(USCTariffRuleSchema.U1_Tariff, tariffsFrom);
				partialTariffQuery.AddToFilter(JoinCondition.And, USCTariffRuleSchema.U1_TariffTo, SQLComparisonOperator.Equal, ZString.Empty);
				var result = new ZQuery();
				result.AddToFilter(partialTariffQuery, JoinCondition.Or);

				partialTariffQuery = new ZQuery(USCTariffRuleSchema.U1_Tariff, SQLComparisonOperator.LessThanOrEqualTo, tariffNumberFrom);
				partialTariffQuery.AddToFilter(JoinCondition.And, USCTariffRuleSchema.U1_TariffTo, tariffsTo);

				result.AddToFilter(partialTariffQuery, JoinCondition.Or);

				partialTariffQuery = new ZQuery(USCTariffRuleSchema.U1_Tariff, SQLComparisonOperator.LessThanOrEqualTo, tariffNumberFrom);
				partialTariffQuery.AddToFilter(JoinCondition.And, USCTariffRuleSchema.U1_TariffTo, SQLComparisonOperator.GreaterThanOrEqualTo, tariffNumberTo);
				if (tariffNumberFrom != tariffNumberTo)
				{
					partialTariffQuery.AddToFilter(JoinCondition.And, USCTariffRuleSchema.U1_TariffTo, SQLComparisonOperator.NotEqual, ZString.Empty);
				}
				result.AddToFilter(partialTariffQuery, JoinCondition.Or);
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(USCTariffRule);
			}
		}

		#endregion

		#region Overriden Properties/Methods

		[List(nameof(Lookups) + "." + nameof(USCTariffRuleLookups.RuleList))]
		public override ZString U1_RuleCode
		{
			get { return base.U1_RuleCode; }
			set { base.U1_RuleCode = value; }
		}

		public override ZDateTime U1_DateFrom
		{
			get { return base.U1_DateFrom; }
			set
			{
				bool hasChanged = base.U1_DateFrom != value;
				base.U1_DateFrom = value;
				if (hasChanged)
				{
					RuleExceptions.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime U1_DateTo
		{
			get { return base.U1_DateTo; }
			set
			{
				bool hasChanged = base.U1_DateTo != value;
				base.U1_DateTo = value;
				if (hasChanged)
				{
					RuleExceptions.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString U1_Tariff
		{
			get { return base.U1_Tariff; }
			set
			{
				bool hasChanged = base.U1_Tariff != value;
				base.U1_Tariff = value;
				if (hasChanged)
				{
					RuleExceptions.MarkAsNeedingValidation();
				}
			}
		}

		public override void Delete()
		{
			RuleExceptions.DeleteAll();
			SecondaryTariffs.DeleteAll();
			base.Delete();
		}

		public bool Applies(ZString ruleCode, ZString tariffNumber, ZDateTime effectiveDate)
		{
			return
				U1_RuleCode == ruleCode &&
				IsTariffWithinTheRange(tariffNumber) &&
				U1_DateFrom <= effectiveDate &&
				(U1_DateTo.IsEmpty || U1_DateTo >= effectiveDate) &&
				!RuleExceptions.Applies(tariffNumber, effectiveDate);
		}

		bool IsTariffWithinTheRange(ZString tariffNumber)
		{
			if (U1_TariffTo.IsEmpty)
			{
				return tariffNumber.StartsWith(U1_Tariff);
			}
			else
			{
				return U1_Tariff.CompareTo(tariffNumber) <= 0 && U1_TariffTo.CompareTo(tariffNumber.SubstringSafe(0, U1_TariffTo.Length)) >= 0;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new USCTariffRuleFetchStrategy(this);
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region New Properties/Methods

		public ZString RuleCodeDescription
		{
			get
			{
				string result = Lookups.RuleList.GetDescriptionFromCode(U1_RuleCode);
				return result ?? string.Empty;
			}
		}

		public ZPropertyInfo RuleCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RuleCodeDescription)); }
		}

		[List(nameof(Lookups) + "." + nameof(USCTariffRuleLookups.Tariffs))]
		[BusinessObjectTestExclude]
		[MaxLength(13)]
		public ZString FormattedTariff
		{
			get { return new TariffFormatter().DisplayFormat(U1_Tariff); }
			set
			{
				U1_Tariff = new TariffFormatter().Format(value);
				Validation.ValidateFormattedTariff();
				FormattedTariffInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedTariffInfo
		{
			get { return GetZPropertyInfo(nameof(FormattedTariff)); }
		}

		[List(nameof(Lookups) + "." + nameof(USCTariffRuleLookups.Tariffs))]
		[BusinessObjectTestExclude]
		[MaxLength(13)]
		public ZString FormattedTariffTo
		{
			get { return new TariffFormatter().DisplayFormat(U1_TariffTo); }
			set
			{
				U1_TariffTo = new TariffFormatter().Format(value);
				Validation.ValidateFormattedTariffTo();
				FormattedTariffToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FormattedTariffToInfo
		{
			get { return GetZPropertyInfo(nameof(FormattedTariffTo)); }
		}

		public ZString EffectiveTariffTo
		{
			get { return U1_TariffTo.IsEmpty ? U1_Tariff : U1_TariffTo; }
		}

		public bool EligibleForAssociatedSecondaryTariffs
		{
			get { return (U1_RuleCode == TariffRuleList.Codes.EligibleForSecondaryTariffNumbers); }
		}

		public bool ShouldDeleteSecondaryTariffs
		{
			get { return !EligibleForAssociatedSecondaryTariffs && (SecondaryTariffs.Count > 0); }
		}

		#endregion

		#region Collections

		public USCTariffRuleExceptionCollection RuleExceptions
		{
			get
			{
				if (fRuleExceptions == null)
				{
					fRuleExceptions = new USCTariffRuleExceptionCollection(this);
				}
				return fRuleExceptions;
			}
		}
		USCTariffRuleExceptionCollection fRuleExceptions;

		public USCRuleSecondaryTariffCollection SecondaryTariffs
		{
			get
			{
				if (secondaryTariffs == null)
				{
					secondaryTariffs = new USCRuleSecondaryTariffCollection(this);
				}
				return secondaryTariffs;
			}
		}
		USCRuleSecondaryTariffCollection secondaryTariffs;

		public void RegisterChildrenAsEditableChild()
		{
			this.RegisterEditableChildObject(RuleExceptions);
			this.RegisterEditableChildObject(SecondaryTariffs);
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("AC25475A-7ABF-44CC-82A4-C02156241D64", "Tariff Rule {0}", U1_Tariff);
	}
}
