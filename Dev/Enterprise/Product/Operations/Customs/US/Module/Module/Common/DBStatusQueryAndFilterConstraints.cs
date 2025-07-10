using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public static class DBStatusQueryAndFilterConstraints
	{
		internal static ZDBOnlySubQuery GetStatusQueryAgainstEntry(SQLComparisonOperator filterOperator, ZString value, string[] cH_MessageTypes)
		{
			var queryNotIn = filterOperator == SQLComparisonOperator.NotEqual || value == DeclarationFilterConstants.MessageStatus.NotSentForFilter;
			var result = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, queryNotIn);

			if (!value.IsEmpty)
			{
				if (value == DeclarationFilterConstants.MessageStatus.NotSentForFilter && filterOperator != SQLComparisonOperator.Equal)
				{
					result.IsNoResultQuery = true;
				}
				else
				{
					if (value == DeclarationFilterConstants.MessageStatus.NotSentForFilter)
					{
						value = ZString.Empty;
					}

					if (filterOperator != SQLComparisonOperator.NotEqual)
					{
						result.AddToFilter(CusEntryHeaderSchema.CH_MessageType, cH_MessageTypes);
					}

					if (filterOperator == SQLComparisonOperator.StartsWith)
					{
						result.AddToFilter(CusEntryHeaderSchema.CH_Status, filterOperator, value);
					}
					else if (value.IsEmpty)
					{
						result.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.NotEqual, value);
					}
					else
					{
						result.AddToFilter(CusEntryHeaderSchema.CH_Status, value);
					}
				}
			}

			return result;
		}

		internal static void SetFilterConstraints(ModuleTextFilter moduleFilter, FilterCategory category)
		{
			moduleFilter.Category = category;
			moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
		}
	}
}
