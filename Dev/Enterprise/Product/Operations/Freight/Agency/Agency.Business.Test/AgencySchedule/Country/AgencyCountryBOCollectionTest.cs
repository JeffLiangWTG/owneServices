using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyCountryCollection))]
	internal class AgencyCountryBOCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AgencyCountryCollection>
	{
		#region Implementation
		protected override AgencyCountryCollection GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			return new AgencyCountryCollection(voyage);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			return new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
		}
		#endregion
	}
}
