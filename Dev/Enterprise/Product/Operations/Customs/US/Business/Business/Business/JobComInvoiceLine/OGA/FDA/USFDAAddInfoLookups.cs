//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFDAAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSFDAAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USFDAAddInfoLookups : AutoUSFDAAddInfoLookups
	{
		public USFDAAddInfoLookups(AutoUSFDAAddInfo parent)
			: base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection US_FDAProductNumberList
		{
			get
			{
				var fdaAddInfo = Parent as USFDAAddInfo;
				var fda = fdaAddInfo?.Parent;
				var effectiveDate = fda?.InvoiceLine?.EffectiveDateForDutyRate ?? ZDateTime.Today;
				var countryCode = Core.Constants.CountryCodes.UnitedStates;
				var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;

				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, countryCode, listType, effectiveDate);
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(countryCode), false));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", new ZString(listType), false));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", effectiveDate));
				return collection;
			}
		}

		public USCCountryCollection USCCountryList
		{
			get { return new USCCountryCollection(Factory); }
		}

		public CargoStorageCodeList US_CargoStorageCodeList
		{
			get { return new CargoStorageCodeList(); }
		}

		public FDABaseUQList FDABaseUQs
		{
			get { return new FDABaseUQList(); }
		}

		public FDAUQList FDAUQs
		{
			get
			{
				return Factory.GetCachedValue("ACS_FDAUQs",
				delegate
				{
					var result = new FDAUQList();
					result.RemoveCode(FDAUQList.Codes.CAR);
					result.RemoveCode(FDAUQList.Codes.CGM);
					result.RemoveCode(FDAUQList.Codes.FOZ);
					result.RemoveCode(FDAUQList.Codes.G);
					result.RemoveCode(FDAUQList.Codes.GAL);
					result.RemoveCode(FDAUQList.Codes.KGM);
					result.RemoveCode(FDAUQList.Codes.L);
					result.RemoveCode(FDAUQList.Codes.LB);
					result.RemoveCode(FDAUQList.Codes.M3);
					result.RemoveCode(FDAUQList.Codes.MG);
					result.RemoveCode(FDAUQList.Codes.MCG);
					result.RemoveCode(FDAUQList.Codes.ML);
					result.RemoveCode(FDAUQList.Codes.OZ);
					result.RemoveCode(FDAUQList.Codes.PCS);
					result.RemoveCode(FDAUQList.Codes.PTL);
					result.RemoveCode(FDAUQList.Codes.QTL);
					result.RemoveCode(FDAUQList.Codes.STN);
					result.RemoveCode(FDAUQList.Codes.SUP);
					result.RemoveCode(FDAUQList.Codes.T);
					result.RemoveCode(FDAUQList.Codes.TAB);
					result.RemoveCode(FDAUQList.Codes.TON);
					result.RemoveCode(FDAUQList.Codes.TOZ);
					return result;
				});
			}
		}

		public CylindricalRectangularList US_CylindricalRectangularList
		{
			get { return new CylindricalRectangularList(); }
		}

		public ConsignorCollection Consignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		public ConsigneeCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public OrgContactCollection ContactList
		{
			get { return new OrgContactCollection(Factory); }
		}

		public OwnerFirmTypeList OwnerFirmTypes
		{
			get { return Factory.GetCachedValue<OwnerFirmTypeList>(); }
		}

		public ProducerFirmTypeList ProducerFirmTypes
		{
			get { return Factory.GetCachedValue<ProducerFirmTypeList>(); }
		}

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(Parent.Factory); }
		}

		public FDAPriorNoticeExemptCodeList FoodFacilityRegistrationExemptionCodes
		{
			get { return Factory.GetCachedValue<FDAPriorNoticeExemptCodeList>(); }
		}

		public FDAMeasurementUnitList DimensionUQs
		{
			get { return Factory.GetCachedValue<FDAMeasurementUnitList>(); }
		}
	}
}
