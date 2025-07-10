using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class EquipmentLookups : CusInBondEquipmentLookups
	{
		public EquipmentLookups(Equipment parent)
			: base(parent)
		{
		}

		new Equipment Parent
		{
			get { return (Equipment)base.Parent; }
		}

		public IBusinessObjectCollection Equipment
		{
			get
			{
				var result = new RefEquipmentCollection(Factory);

				var value = Parent.BJ_IsConveyance ? new ZString("Is a Vehicle") : new ZString("Is not a Vehicle");
				var filterDefault = new FilterBusinessObjectDefault("Vehicle Status", "Property", value, true);
				result.FilterBusinessObjectDefaults.Add(filterDefault);

				value = Parent.BJ_IsConveyance ? EquipmentTypeFilterList.Codes.Conveyance : EquipmentTypeFilterList.Codes.Equipment;
				filterDefault = new FilterBusinessObjectDefault("e-Manifest Type", "Property", value, true);
				result.FilterBusinessObjectDefaults.Add(filterDefault);
				return result;
			}
		}

		public IBusinessObjectCollection Currencies
		{
			get { return Parent.Trip.Lookups.Currencies; }
		}

		public ICodeDescriptionPairList EquipmentTypes
		{
			get { return Factory.GetCachedValue<EquipmentTypes>(); }
		}

		public ICodeDescriptionPairList ConveyanceTypes
		{
			get { return Factory.GetCachedValue<ConveyanceTypes>(); }
		}

		public override RefContainerCollection RoadContainerTypes
		{
			get
			{
				if (fRoadContainerTypesCollection == null)
				{
					fRoadContainerTypesCollection = new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Road);
				}
				return fRoadContainerTypesCollection;
			}
		}
		RefContainerCollection fRoadContainerTypesCollection;

		public override RefCountryStatesCollection RegistrationStates
		{
			get
			{
				var countryCode = Parent?.BJ_RN_NKRegistrationCountry ?? ZString.Empty;
				if (!countryCode.IsEmpty)
				{
					return Factory.GetCachedValue("US.eManifest.Business.Equipment.RegistrationStates." + countryCode,
						() =>
						{
							var result = base.RegistrationStates;
							result.AdditionalFilter = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, countryCode);
							return result;
						});
				}
				else
				{
					return Factory.GetCachedValue("US.eManifest.Business.Equipment.RegistrationStates",
						() =>
						{
							var result = base.RegistrationStates;
							result.AdditionalFilter = ZQuery.NoResultQuery;
							return result;
						});
				}
			}
		}
	}
}
