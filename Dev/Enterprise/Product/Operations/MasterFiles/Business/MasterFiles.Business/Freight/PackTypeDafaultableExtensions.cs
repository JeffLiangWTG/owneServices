using CargoWise.Common;

namespace Enterprise.MasterFiles.Business
{
	public static class PackTypeDafaultableExtensions
	{
		public static void SetupDefaultWeightAndDimensions(this IPackTypeDafaultable packTypeDafaultable, OrgBuyerSupplierLinkPackPivot packageDetails, RefPackType packType, int count)
		{
			Argument.NotNull(packTypeDafaultable, "packTypeDafaultable");

			bool dimensionsAreOverriddenOnPivot = false;
			bool weightIsOverriddenOnPivot = false;

			if (packageDetails != null)
			{
				if (!packageDetails.Q0_Height.IsEmpty || !packageDetails.Q0_Length.IsEmpty || !packageDetails.Q0_Width.IsEmpty)
				{
					packTypeDafaultable.Height = packageDetails.Q0_Height;
					packTypeDafaultable.Length = packageDetails.Q0_Length;
					packTypeDafaultable.Width = packageDetails.Q0_Width;
					packTypeDafaultable.UnitOfDimension = packageDetails.Q0_UnitOfDimension;
					dimensionsAreOverriddenOnPivot = true;
				}

				if (!packageDetails.Q0_Weight.IsEmpty)
				{
					packTypeDafaultable.Weight = packageDetails.Q0_Weight * count;
					packTypeDafaultable.UnitOfWeight = packageDetails.Q0_UnitOfWeight;
					weightIsOverriddenOnPivot = true;
				}
			}

			if (packType != null)
			{
				if (!dimensionsAreOverriddenOnPivot && (!packType.F3_Height.IsEmpty || !packType.F3_Length.IsEmpty || !packType.F3_Width.IsEmpty))
				{
					packTypeDafaultable.Height = packType.F3_Height;
					packTypeDafaultable.Length = packType.F3_Length;
					packTypeDafaultable.Width = packType.F3_Width;
					packTypeDafaultable.UnitOfDimension = packType.F3_UnitOfDimension;
				}

				if (!weightIsOverriddenOnPivot && !packType.F3_Weight.IsEmpty)
				{
					packTypeDafaultable.Weight = packType.F3_Weight * count;
					packTypeDafaultable.UnitOfWeight = packType.F3_UnitOfWeight;
				}
			}
		}
	}
}