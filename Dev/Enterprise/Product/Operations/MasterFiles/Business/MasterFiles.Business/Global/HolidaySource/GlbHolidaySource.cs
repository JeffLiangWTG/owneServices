using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbHolidaySource : AutoGlbHolidaySource
	{
		public GlbHolidaySource(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
