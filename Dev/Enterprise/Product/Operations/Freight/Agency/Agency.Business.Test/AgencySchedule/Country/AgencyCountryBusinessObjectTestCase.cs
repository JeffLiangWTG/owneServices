using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyCountry))]
	internal class AgencyCountryBusinessObjectTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AgencyCountry(Factory.New<JobVoyage>(), new RefCountry.Loader(Factory).LoadForCountry("AU"));
		}
		#endregion
	}
}
