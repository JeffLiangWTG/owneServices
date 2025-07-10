using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBondedWarehouseAttributeLookups : AutoWhsBondedWarehouseAttributeLookups
	{
		public WhsBondedWarehouseAttributeLookups(AutoWhsBondedWarehouseAttribute parent)
			: base(parent) { }

		#region UQList

		public CodeDescriptionPairList UQList
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion

		#region CountryList

		public RefCountryCollection CountryList
		{
			get { return Factory.GetCachedValue("WhsBondedWarehouseAttributeLookups|CountryList", () => new RefCountryCollection(Factory)); }
		}

		#endregion

		#region Manufacturers

		public OrgHeaderCollection Manufacturers
		{
			get { return Factory.GetCachedValue("WhsBondedWarehouseAttributeLookups|Manufacturers", () => new OrganisationsFindBoxCollection(Factory)); }
		}

		#endregion

		#region OutwardTypes

		public WhsBondedWarehouseAttributeOutwardType OutwardTypes
		{
			get { return Factory.GetCachedValue("WhsBondedWarehouseAttributeLookups|OutwardTypes", () => new WhsBondedWarehouseAttributeOutwardType()); }
		}

		#endregion

		#region ZoneStatusList

		public CodeDescriptionPairList ZoneStatusList
		{
			get { return Factory.GetCachedValue("WhsBondedWarehouseAttributeLookups|ZoneStatusList", () => IsUSFTZWarehouse ? new ZoneStatusList() : new CodeDescriptionPairList()); }
		}

		bool IsUSFTZWarehouse => ((WhsBondedWarehouseAttribute)Parent).IsUSFTZWarehouse;

		#endregion

	}
}
