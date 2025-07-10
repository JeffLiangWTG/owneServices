using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Packing.Business
{
	public static class PkgPackageItemDivotCustomPropertiesHelper
	{
		public static IEnumerable<ICustomProperty> GetCustomProperties<T>(IPackableItemParent packableItemParent)
			where T : BusinessObject, IPackableItemParentWrapper
		{
			var customProperties = Enumerable.Empty<ICustomProperty>();

			if (packableItemParent != null) // PackableItem is null if the DB record no longer exists eg. Unallocate stock from Pick
			{
				var customPropertyContainer = packableItemParent.AdditionalProperties;
				if (customPropertyContainer != null)
				{
					customProperties = customPropertyContainer.CustomProperties.Select(
						innerProperty => new CustomPropertyImplementation<T>(innerProperty.Identifier, innerProperty.Info, bizo => innerProperty.GetValue((bizo).PackableItemParent as BusinessObject)));
				}
			}

			return customProperties;
		}
	}

	public interface IPackableItemParentWrapper
	{
		IPackableItemParent PackableItemParent { get; }
	}
}
