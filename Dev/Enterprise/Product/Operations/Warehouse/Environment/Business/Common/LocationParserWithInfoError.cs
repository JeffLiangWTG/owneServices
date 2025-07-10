using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business
{
	public class LocationParserWithInfoError : LocationParser
	{
		public LocationParserWithInfoError(IBusiness parent) : base(parent) { }

		protected override void GetLocationComponentError(WhsRow row, ZPropertyInfo info, ZString component, ZBool useAlpha, ZBool isZeroBased, ZShort max, string componentName)
		{
			info.AddError(row.RangeMessage(componentName, max, isZeroBased));
		}
	}
}
