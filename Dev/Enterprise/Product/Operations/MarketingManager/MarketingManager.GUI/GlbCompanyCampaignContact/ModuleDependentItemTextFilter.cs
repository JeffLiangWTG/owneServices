using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class ModuleDependentItemTextFilter : ModuleTextFilter
	{
		#region Constructor

		public ModuleDependentItemTextFilter(ZString description, GetTextQueryWithOperator queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
			Initialize();
		}

		protected ModuleDependentItemTextFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
			Initialize();
		}

		void Initialize()
		{
			ErrorOnCodeNotPresent = true;
			SetupComparisonOperatorList();
		}

		#endregion

		#region Properties

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Comparison Operators

		void SetupComparisonOperatorList()
		{
			ComparisonOperator_List.Clear();
			ComparisonOperator_List.AddPair(ResString.GetMultilingualString("1355a2cb-14fe-4453-9b26-0462494b18a2", "contains"), ResString.GetMultilingualString("95ce7b7a-3295-404f-aa6d-b54959c7d9b7", "Search for records that contain an item matching the supplied text"));
			ComparisonOperator_List.AddPair(ResString.GetMultilingualString("107f5213-b0ae-47aa-ba03-0e2735054c4d", "not contain"), ResString.GetMultilingualString("8a8ea973-e606-404d-a442-78d3f7728433", "Search for records that do not contain an item matching the supplied text"));
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get { return new[] { ComparisonConstants.Contains, ComparisonConstants.NotContain }; }
		}

		#endregion

		#region Overrides

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleDependentItemTextFilter(category, parentCollection);
		}

		#endregion
	}
}
