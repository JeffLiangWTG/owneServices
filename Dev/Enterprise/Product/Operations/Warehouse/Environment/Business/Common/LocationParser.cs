using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	public class LocationParser
	{
		public LocationParser(IBusiness parent = null)
		{
			this.parent = parent;
		}

		bool IsValidationSuspendedOnParent
		{
			get { return parent != null && parent.IsValidationSuspended; }
		}

		#region GetLocationComponentError

		protected virtual void GetLocationComponentError(WhsRow row, ZPropertyInfo info, ZString value, ZBool useAlpha, ZBool isZeroBased, ZShort max, string componentName)
		{
		}

		#endregion

		#region Methods

		public ZShort GetLocationComponent(WhsRow row, ZPropertyInfo info, ZString value, ZBool useAlpha, ZBool isZeroBased, ZShort max, string componentName)
		{
			ZShort result = Parse(value, useAlpha);

			if (!IsValidationSuspendedOnParent)
			{
				ZShort firstElement = isZeroBased ? (ZShort)0 : (ZShort)1;
				ZShort lastElement = isZeroBased ? max - 1 : max;

				if (firstElement > result || result > lastElement)
				{
					GetLocationComponentError(row, info, value, useAlpha, isZeroBased, max, componentName);
				}
			}

			return result;
		}

		public ZShort Parse(ZString value, bool useAlpha)
		{
			return LocationComponentParser.Parse(value, useAlpha);
		}

		#endregion

		readonly IBusiness parent;
	}
}

