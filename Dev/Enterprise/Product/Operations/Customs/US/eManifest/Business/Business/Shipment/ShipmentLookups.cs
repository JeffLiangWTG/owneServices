using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentLookups : CusInBondBillLookups
	{
		public ShipmentLookups(Shipment parent)
			: base(parent)
		{
		}

		new Shipment Parent
		{
			get { return (Shipment)base.Parent; }
		}

		public ICodeDescriptionPairList ShipmentTypes
		{
			get { return Factory.GetCachedValue<ShipmentTypes>(); }
		}

		public ICodeDescriptionPairList ServiceTypes
		{
			get { return Factory.GetCachedValue<ServiceTypes>(); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleKPortCodes
		{
			get
			{
				return Factory.GetCachedValue("ScheduleKPortCodes", delegate()
				{
					var foreignPorts = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
					var result = new ZZRefCusCodeListCombinedCollection(Factory);
					foreach (var foreignPort in foreignPorts)
					{
						result.Add(foreignPort);
					}
					return result;
				});
			}
		}

		public IBusinessObjectCollection SCACCarrierCodes
		{
			get
			{
				return Factory.GetCachedValue("SCACCarrierCodes", delegate()
				{
					return new USCarrierCombinedCollection(Factory, Trip.Truck);
				});
			}
		}

		public IBusinessObjectCollection FIRMSCodes
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);
			}
		}

		public ICodeDescriptionPairList WeightUnits
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public ICodeDescriptionPairList VolumeUnits
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public ICodeDescriptionPairList QuantityUnits
		{
			get { return Factory.GetCachedValue<PackageTypes>(); }
		}

		public ICodeDescriptionPairList ReleaseStatusList
		{
			get { return Factory.GetCachedValue<ShipmentEntryStatusList>(); }
		}

		public IBusinessObjectCollection Organizations
		{
			get
			{
				return Factory.GetCachedValue("Organizations", delegate()
				{
					return new OrgHeaderCollection(Factory);
				});
			}
		}

		public IBusinessObjectCollection Currencies
		{
			get
			{
				return Factory.GetCachedValue("Currencies", delegate()
				{
					return new RefCurrencyCollection(Factory);
				});
			}
		}

		public IBusinessObjectCollection Countries
		{
			get
			{
				return Factory.GetCachedValue("Countries", delegate()
				{
					return Parent.Trip.Lookups.Countries;
				});
			}
		}

		public ContainerModeList ContainerModeList
		{
			get { return Factory.GetCachedValue<ContainerModeList>(); }
		}

		public IBusinessObjectCollection Equipment
		{
			get
			{
				return Factory.GetCachedValue("Equipment", delegate()
				{
					return Parent.Trip.AllEquipmentIncludingMainConveyance;
				});
			}
		}

		public IBusinessObjectCollection UNDGSubs
		{
			get
			{
				return Factory.GetCachedValue("UNDGSubs", delegate()
				{
					return new UNDGSubstanceCollection(Factory);
				});
			}
		}

		public IBusinessObjectCollection Contacts
		{
			get
			{
				return Factory.GetCachedValue("Contacts", delegate()
				{
					return new OrgContactCollection(Factory);
				});
			}
		}
	}
}
