using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencySailingDependentCollection))]
	internal class AgencySailingDependentBOCollection : NonPersistentBusinessObjectCollectionTestCase<AgencySailingDependentCollection>
	{
		#region Implementation
		protected override AgencySailingDependentCollection GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyPrincipal principal = new AgencyPrincipal(Factory);
			principal.Schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			return principal.Sailings;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobSailing sailing = Factory.New<JobSailing>();
			return new AgencySailing(sailing, ZGuid.Empty);
		}
		#endregion
	}
}
