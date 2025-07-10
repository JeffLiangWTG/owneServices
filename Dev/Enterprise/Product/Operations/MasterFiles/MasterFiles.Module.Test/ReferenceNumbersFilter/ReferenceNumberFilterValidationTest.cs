using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ReferenceNumberFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCountry()
		{
			Filter.Country = "AU";
			AssertNoNotifications(Filter.CountryInfo);

			Filter.Country = "XX";
			AssertHasError(Filter.CountryInfo, "Enter a valid selection.");

			Filter.Country = "";
			AssertNoNotifications(Filter.CountryInfo);
		}

		public void TestType()
		{
			Filter.Country = "AU";

			Filter.Type = "COC";
			AssertNoNotifications(Filter.TypeInfo);

			Filter.Type = "XXX";
			AssertHasError(Filter.TypeInfo, "Enter a valid selection.");

			Filter.Type = "";
			AssertNoNotifications(Filter.TypeInfo);
		}

		#region Implementation

		ReferenceNumberFilter Filter
		{
			get { return filter ?? (filter = new ReferenceNumberFilter("description", delegate { return new ZQuery(); }, new RefCountryCollection(Factory))); }
		}
		ReferenceNumberFilter filter;

		#endregion
	}
}
