using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	internal static class IBusinessExtensions
	{
		internal static bool HasChangesOnChildrenNotValidIfFinalised(this IBusiness bizO)
		{
			foreach (var child in bizO.Children)
			{
				var packingChild = child as IPackingHasChanges;
				if (packingChild != null && packingChild.HasChangesThatAreInvalidIfFinalised)
				{
					return true;
				}
			}

			return false;
		}

		internal static bool HasChangesOnChildrenNotValidIfFinalised(this IBusinessObjectCollection collection)
		{
			if (!collection.IsLoaded)
			{
				return collection.HasChanges;
			}

			foreach (IPackingHasChanges bizO in collection)
			{
				if (bizO.HasChangesThatAreInvalidIfFinalised)
				{
					return true;
				}
			}

			return false;
		}
	}
}
