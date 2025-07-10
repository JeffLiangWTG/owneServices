using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	static class CustomFieldOldValueHelper
	{
		public static IZType GetOrSetOriginalValue(this BusinessObject bizo, UserDefinedPropertyManager propertyManager, ICustomColumnDefinition columnDefinition)
		{
			return bizo?.Factory?.GetCachedValue($"CustomFieldOriginalValue-{bizo.PK}-{columnDefinition.Identifier}-{columnDefinition.Name}",
				() => propertyManager.GetValue(columnDefinition.Name, AddOnColumnDataType.GetTypeFromCode(columnDefinition.Type)), CacheStalenessPolicy.StaleOnFactorySave);
		}

		public static IZType GetOrSetOriginalWrappedValue(this BusinessObject bizo, ICustomColumnDefinition columnDefinition)
		{
			return bizo?.Factory?.GetCachedValue($"CustomFieldOriginalValue-{bizo.PK}-{columnDefinition.Identifier}-{columnDefinition.Name}",
				() => bizo[columnDefinition.Name] as IZType, CacheStalenessPolicy.StaleOnFactorySave);
		}
	}
}
