using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondHeaderLookups : Customs.Business.CusInBondHeaderLookups
	{
		public CusInBondHeaderLookups(CusInBondHeader parent)
			: base(parent)
		{
		}

		public USCarrierCombinedCollection CarrierCollection
		{
			get
			{
				USCarrierCombinedCollection result = new USCarrierCombinedCollection(Factory);
				if (!Parent.BH_ImportTransportMode.IsEmpty)
				{
					result.AdditionalFilter = new ZQuery(USCarrierCombinedSchema.UI_ModeOfTransportation, SQLComparisonOperator.StartsWith, Parent.BH_ImportTransportMode.SubstringSafe(0, 1));
					result.AddNotificationWhenAdditionalFilterNotMetOverride = AddNotificationWhenAdditionalFilterNotMetOverrideDelegate;
				}
				return result;
			}
		}

		void AddNotificationWhenAdditionalFilterNotMetOverrideDelegate(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			if (!Parent.BH_ImportTransportMode.IsEmpty)
			{
				USCarrierCombined carrier = (USCarrierCombined)selectedBusinessObject;
				if (carrier.UI_ModeOfTransportation != Parent.BH_ImportTransportMode)
				{
					errors.Add("This carrier's mode of transportation is different to the import carrier's transport mode.");
				}
			}
		}

		public ThreeLetterRefAirlineCollection AirlineCollection
		{
			get
			{
				var airlineCollection = new ThreeLetterRefAirlineCollection(Factory);

				airlineCollection.AdditionalFilter = new ZQuery(RefAirlineSchema.RM_ThreeLetterCode, SQLComparisonOperator.NotEqual, "");
				airlineCollection.AdditionalFilter.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, "");

				airlineCollection.AddNotificationWhenAdditionalFilterNotMetOverride = (StringCollectionX errors, BusinessObject selectedBusinessObject) =>
				{
					var airline = (ThreeLetterRefAirline)selectedBusinessObject;
					if (airline.RM_ThreeLetterCode.IsEmpty)
					{
						errors.Add("This Airline requires a Three Letter Code to be used as an Inbond Carrier.");
					}

					if (airline.RM_EagleAddedAirlinePrefixOrAccountingCode.IsEmpty)
					{
						errors.Add("This Airline requires an Airline Numeric Code to be used as an Inbond Carrier.");
					}
				};

				return airlineCollection;
			}
		}

		public InBondTransportModeCodes TransportModeCodes
		{
			get
			{
				if (Parent.BH_HeaderType == InBondHeaderTypeList.Codes.FullData && !Parent.BH_FTZMove)
				{
					return InBondTransportModeCodes.GetNonAMSCachedValue(Factory);
				}
				else
				{
					return InBondTransportModeCodes.GetCachedValue(Factory);
				}
			}
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleKCodes
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(
					Factory,
					Core.Constants.CountryCodes.UnitedStates,
					new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port },
					ZDateTime.Today,
					new[]
					{
					new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, new ZString[] { ForeignPortTypeList.Codes.Common, ForeignPortTypeList.Codes.InBond })
					});
			}
		}

		public ZZRefCusCodeListCombinedCollection ScheduleDCodes
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombinedCollection FIRMSCollection
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		public RefVesselCollection ImportingConveyanceList
		{
			get { return new RefVesselCollection(Factory); }
		}

		public InBondHeaderTypeList HeaderTypeList
		{
			get { return Factory.GetCachedValue<InBondHeaderTypeList>(); }
		}

		public ConsigneeCollection ImporterList
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public override OrgHeaderCollection Suppliers
		{
			get { return new ConsignorCollection(Factory); }
		}

		protected new CusInBondHeader Parent
		{
			get { return (CusInBondHeader)base.Parent; }
		}
	}
}
