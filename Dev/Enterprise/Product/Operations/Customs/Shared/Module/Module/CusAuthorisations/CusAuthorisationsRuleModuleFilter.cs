using System;
using System.Collections.Immutable;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class CusAuthorisationsRuleModuleFilter : ModuleCodeFilter
	{
		public delegate ZQuery GetAuthorisationRuleDetailsQuery(ZString value1, SQLComparisonOperator comparisonOperator, ZString value2);

		public CusAuthorisationsRuleModuleFilter(ZString description, GetAuthorisationRuleDetailsQuery queryDelegate, CodeDescriptionPairList list)
			: base(description, (val1, val2) => new ZQuery(), () => null, () => null)
		{
			Property1Validation = ListValidation.WarnIfInvalidCode;
			QueryDelegate = queryDelegate;
			RuleCodeList = list;
		}

		protected override object[] QueryDelegateParameters => new object[] { Property1, ModuleTextFilter.GetSqlComparisonOperator(ComparisonOperator, SQLComparisonOperator.StartsWith), Property2 };

		protected override FilterCategory DefaultCategory => FilterCategories.TextSearch;

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException(GetType().Name + " does not support 'Common'.");
		}

		[MaxLength(3)]
		[List(nameof(RuleCodeList))]
		public override ZString Property1
		{
			get { return base.Property1; }
			set { base.Property1 = value; }
		}

		[MaxLength(255)]
		public override ZString Property2
		{
			get { return base.Property2; }
			set { base.Property2 = value; }
		}

		public new BusinessObjectFactory Factory => factory ?? (factory = CreateNewFactory());
		BusinessObjectFactory factory;

		public CodeDescriptionPairList RuleCodeList { get; private set; }

		[BusinessObjectTestExclude]
		[List(nameof(ComparisonOperator_List))]
		public ZString ComparisonOperator
		{
			get { return fComparisonOperator; }
			set
			{
				if (fComparisonOperator != value)
				{
					fComparisonOperator = ComparisonOperatorsHashSet.Contains(value) ? value : (ZString)ModuleTextFilter.ComparisonConstants.Default;
					ComparisonOperatorInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString fComparisonOperator = ModuleTextFilter.ComparisonConstants.Default;

		public ZPropertyInfo ComparisonOperatorInfo => GetZPropertyInfo(nameof(ComparisonOperator));

		public CodeDescriptionPairList ComparisonOperator_List
		{
			get
			{
				if (fComparisonOperator_List == null)
				{
					fComparisonOperator_List = new CodeDescriptionPairList();
					foreach (var op in ComparisonOperators)
					{
						fComparisonOperator_List.Add(GetComparisonOperator(op));
					}
				}
				return fComparisonOperator_List;
			}
		}
		CodeDescriptionPairList fComparisonOperator_List;

		CodeDescriptionPair GetComparisonOperator(string code) => ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(code);

		static readonly ImmutableArray<string> ComparisonOperators = ImmutableArray.Create(
			ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact,
			ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith,
			ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains,
			ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual,
			ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith,
			ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain
		);

		static readonly ImmutableHashSet<string> ComparisonOperatorsHashSet = ComparisonOperators.ToImmutableHashSet();
	}
}
