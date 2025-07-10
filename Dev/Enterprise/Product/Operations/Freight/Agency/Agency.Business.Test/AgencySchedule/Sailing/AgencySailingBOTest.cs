using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencySailing))]
	internal class AgencySailingBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			JobSailing sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			AgencyPrincipal principal = new AgencyPrincipal(Factory);
			principal.Schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			return principal.Sailings[0];
		}
		#endregion
	}
}
