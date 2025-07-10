using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Registry;

namespace Enterprise.Freight.Forwarding.Business
{
	static class CargoDimensionsHelpers
	{
		public struct CheckFitsInConsolResult
		{
			public bool ItemFitsHeight { get; }
			public bool ItemFitsWidthAndLength { get; }
			public CheckFitsInConsolResult(bool itemFitsHeight, bool itemFitsWidthAndLength)
			{
				ItemFitsHeight = itemFitsHeight;
				ItemFitsWidthAndLength = itemFitsWidthAndLength;
			}
		}

		public static void AddPreAllocationCheckDimensionsError(ZPropertyInfo propertyInfo, string message)
		{
			var checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			var dimensionsCheck = checks.Cast<PreAllocationCheck>().FirstOrDefault(check => check.Measure == PreAllocationCheck.Measures.Dimensions);

			if (dimensionsCheck != null && dimensionsCheck.Action == PreAllocationCheck.Actions.Restriction)
			{
				propertyInfo.AddError(message);
			}
		}

		public static CheckFitsInConsolResult CheckCanFitInConsol(ForwardingConsol consol, ICargoDimensions item)
		{
			if (ConsolAndItemDimensionsAreNotSet(consol, item))
			{
				return new CheckFitsInConsolResult(true, true);
			}

			var consolFieldsAreUnbounded = ConsolCargoDimensionsAreUnbounded(consol);
			var itemFitsHeight = consolFieldsAreUnbounded || ItemHeightFitsInConsol(consol, item);
			var itemFitsWidthAndLength = consolFieldsAreUnbounded || ItemWidthAndLengthFitsInConsol(consol, item);

			return new CheckFitsInConsolResult(itemFitsHeight, itemFitsWidthAndLength);
		}

		static bool ConsolAndItemDimensionsAreNotSet(ForwardingConsol consol, ICargoDimensions item)
		{
			if (consol == null || item == null)
			{
				return true;
			}

			var preallocationDimensionsAreNotSet = consol.JK_MaximumAllowablePackageUnit.IsEmpty;

			return preallocationDimensionsAreNotSet || item.DimensionsUnits.IsEmpty || item.Width == 0 || item.Length == 0 || item.Height == 0;
		}

		public static bool ConsolCargoDimensionsAreUnbounded(ForwardingConsol consol)
		{
			return consol.JK_MaximumAllowablePackageLength == 0
				&& consol.JK_MaximumAllowablePackageWidth == 0
				&& consol.JK_MaximumAllowablePackageHeight == 0
				&& consol.JK_MaximumAllowablePackageUnit.IsEmpty;
		}

		static bool ItemHeightFitsInConsol(ForwardingConsol consol, ICargoDimensions item)
		{
			var itemUnits = item.DimensionsUnits;
			var consolUnits = consol.JK_MaximumAllowablePackageUnit;
			var itemHeight = Core.Constants.Length.ConvertSafe(item.Height, itemUnits, consolUnits);

			return itemHeight <= consol.JK_MaximumAllowablePackageHeight;
		}

		static bool ItemWidthAndLengthFitsInConsol(ForwardingConsol consol, ICargoDimensions item)
		{
			var itemUnits = item.DimensionsUnits;
			var consolUnits = consol.JK_MaximumAllowablePackageUnit;

			var itemLength = Core.Constants.Length.ConvertSafe(item.Length, itemUnits, consolUnits);
			var itemWidth = Core.Constants.Length.ConvertSafe(item.Width, itemUnits, consolUnits);

			var itemLengthAndWidthFitsInConsol = itemLength <= consol.JK_MaximumAllowablePackageLength
				&& itemWidth <= consol.JK_MaximumAllowablePackageWidth;

			var itemLengthAndWidthFitsInConsolRotated = itemWidth <= consol.JK_MaximumAllowablePackageLength
				&& itemLength <= consol.JK_MaximumAllowablePackageWidth;

			return itemLengthAndWidthFitsInConsol || itemLengthAndWidthFitsInConsolRotated;
		}
	}
}
