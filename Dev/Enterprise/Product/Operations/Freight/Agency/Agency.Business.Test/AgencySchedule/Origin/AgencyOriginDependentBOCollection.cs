using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyOriginDependentCollection))]
	internal class AgencyOriginDependentBOCollection : AgencyAllocationItemCollectionTest<AgencyOriginDependentCollection, AgencyOrigin, VoyageOrigin>
	{
		#region Implementation
		protected override AgencyOriginDependentCollection GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			AgencyPrincipal principal = new AgencyPrincipal(Factory);
			principal.Schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			return principal.Origins;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AgencyOrigin(GetNewInnerElement(), ZGuid.Empty);
		}

		protected override VoyageOrigin GetNewInnerElement()
		{
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			return origin;
		}

		protected override BusinessObjectCollection GetCollectToWrap()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			return voyage.Origins;
		}

		protected override AgencyOriginDependentCollection WrapCollection(BusinessObjectCollection collectionToWrap)
		{
			VoyageOriginDependentCollection origins = (VoyageOriginDependentCollection)collectionToWrap;
			JobVoyage voyage = origins.Master;
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			return new AgencyOriginDependentCollection(schedule.GenericPrincipal);
		}
		#endregion
	}
}
