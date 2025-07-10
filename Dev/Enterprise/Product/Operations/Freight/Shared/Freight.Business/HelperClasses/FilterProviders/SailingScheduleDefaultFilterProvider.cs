using CargoWise.EntityFramework;
using CargoWise.Types;
using CharterFilterCodes = Enterprise.Freight.Business.FreightConstants.CharterFilter;

namespace Enterprise.Freight.Business
{
	public class SailingScheduleDefaultFilterProvider : DefaultFilterProvider
	{
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }
		public ZDateTime ETDFrom { get; set; }
		public ZDateTime ETDTo { get; set; }
		public ZDateTime ETAFrom { get; set; }
		public ZDateTime ETATo { get; set; }
		public ZBool? IsChartered { get; set; }

		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddFilterDefaults(collection, "Load / Discharge", LoadPort, DischargePort);
			AddFilterDefaults(collection, "Load Port ETD", ETDFrom, ETDTo);
			AddFilterDefaults(collection, "Load Port ETA", ETAFrom, ETATo);

			if (IsChartered.HasValue)
			{
				AddFilterDefaults(collection, "Charter", IsChartered.Value ? CharterFilterCodes.CharterOnlyCode : CharterFilterCodes.NonCharterOnlyCode);
			}
		}
	}
}
