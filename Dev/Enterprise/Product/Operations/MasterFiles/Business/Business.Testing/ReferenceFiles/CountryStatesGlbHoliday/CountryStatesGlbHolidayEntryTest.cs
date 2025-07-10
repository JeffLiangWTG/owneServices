using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CountryStatesGlbHolidayEntry))]
	public class CountryStatesGlbHolidayEntryTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_ParentID = country.PK;
			glbHoliday.GH_ParentTableCode = country.TablePrefix;
			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, glbHoliday);
			return new CountryStatesGlbHolidayEntry(glbHolidayBizo, ZGuid.NewZGuid(), "ANY", true);
		}

		#endregion
	}
}
