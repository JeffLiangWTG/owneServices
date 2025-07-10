using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class AgencyHouseBillLookups
	{
		public AgencyHouseBillLookups(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		#region ReleaseTypes

		public ICodeDescriptionPairList ReleaseTypes => releaseTypes ?? (releaseTypes = AgencyRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList());

		ICodeDescriptionPairList releaseTypes;

		#endregion

		#region Currencies

		public IFindBoxListProvider Currencies => factory.GetCachedValue<IFindBoxListProvider>("Agency.Documents.DocDataObjects.AgencyHouseBillLookups.Curriencies", () => new RefCurrencyCollection(factory));

		#endregion

		#region Countries

		public IRefCountryCollection Countries => factory.GetCachedValue<IRefCountryCollection>("Agency.Documents.DocDataObjects.AgencyHouseBillLookups.Countries", () => new RefCountryCollection(factory));

		#endregion
	}
}
