using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Lists;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class BindToLists : TransportBindToLists
	{
		public static BindToLists GetCachedLists(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.TransportBookings.Business.BindToLists", () => new BindToLists(factory));
		}

		public BindToLists(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DtbBookingTmplCollection BookingTemplates
		{
			get
			{
				var query = new ZQuery(DtbBookingTmplSchema.KT_IsActive, true);
				query.AddToFilter(bookingTemplatesAdditionalQuery, JoinCondition.And);

				var bookingTemplates = new DtbBookingTmplCollection(Factory, query);
				bookingTemplates.ApplySort(DtbBookingTmplSchema.Constants.KT_Description, ListSortDirection.Ascending);

				return bookingTemplates;
			}
		}

		public ZQuery BookingTemplatesAdditionalQuery
		{
			get => bookingTemplatesAdditionalQuery ?? ZQuery.NoResultQuery;
			set => bookingTemplatesAdditionalQuery = value;
		}
		ZQuery bookingTemplatesAdditionalQuery;

		public CodeDescriptionPairList BookingConsolidatedStatuses
		{
			get { return bookingConsolidated ?? (bookingConsolidated = new BookingConsolidatedStatuses().List); }
		}

		CodeDescriptionPairList bookingConsolidated;

		public CodeDescriptionPairList ConfirmationTypes
		{
			get
			{
				if (confirmationTypes == null)
				{
					confirmationTypes = new ConfirmationTypes().List;
					foreach (DateAndReference dateAndReference in TransportRegistry.Instance.DateAndReference.Value)
					{
						confirmationTypes.AddPairIfNotExist(dateAndReference.Code, dateAndReference.Description);
					}
				}
				return confirmationTypes;
			}
		}

		public CodeDescriptionPairList GetConfirmationTypes(ZString bookingDirection, ZString instructionType, ZString organisationType)
		{
			var result = new CodeDescriptionPairList();
			var dateAndReferences = TransportRegistry.Instance.DateAndReference.Value;

			foreach (DateAndReference dateAndReference in dateAndReferences)
			{
				if ((dateAndReference.BookingDirection == DatesAndReference.Any || dateAndReference.BookingDirection == bookingDirection) &&
					(dateAndReference.InstructionType == DatesAndReference.Any || dateAndReference.InstructionType == instructionType) &&
					(dateAndReference.OrganisationType == DatesAndReference.Any || dateAndReference.OrganisationType == organisationType))
				{
					result.AddPairIfNotExist(dateAndReference.Code, dateAndReference.Description);
				}
			}

			return result;
		}

		CodeDescriptionPairList confirmationTypes;

		public CodeDescriptionPairList GetConfirmationDescriptions(ZString bookingDirection, ZString instructionType, ZString organisationType)
		{
			var result = new CodeDescriptionPairList();

			foreach (CodeDescriptionPair codePair in GetConfirmationTypes(bookingDirection, instructionType, organisationType))
			{
				result.AddPair(codePair.Description, codePair.CodeAndDescription);
			}

			return result;
		}

		public CodeDescriptionPairList ContainerTypes
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.ContainerType); }
		}

		public CodeDescriptionPairList Directions
		{
			get
			{
				return Factory.GetCachedValue("22E4E683-73F9-43A5-B317-FFF316E97012",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Constants.CartageDirection.Import, Constants.CartageDirectionDescription.Import);
						result.AddPair(Constants.CartageDirection.Export, Constants.CartageDirectionDescription.Export);
						result.AddPair(Constants.CartageDirection.Origin, Constants.CartageDirectionDescription.Origin);
						result.AddPair(Constants.CartageDirection.Destination, Constants.CartageDirectionDescription.Destination);
						result.AddPair(Constants.CartageDirection.Local, Constants.CartageDirectionDescription.Local);

						return result;
					});
			}
		}

		public CodeDescriptionPairList DirectionsChar
		{
			get
			{
				return Factory.GetCachedValue("77D30FC2-3580-4F59-993E-DA5DFCBBA596",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Constants.CartageDirectionChar.Import, Constants.CartageDirection.Import);
						result.AddPair(Constants.CartageDirectionChar.Export, Constants.CartageDirection.Export);
						result.AddPair(Constants.CartageDirectionChar.Origin, Constants.CartageDirection.Origin);
						result.AddPair(Constants.CartageDirectionChar.Destination, Constants.CartageDirection.Destination);
						result.AddPair(Constants.CartageDirectionChar.Local, Constants.CartageDirection.Local);

						return result;
					});
			}
		}

		public CodeDescriptionPairList BookingConsolidationJobDirections
		{
			get
			{
				return Factory.GetCachedValue("29C8C112-5E9E-46F9-A0E7-BC50660A9104",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(nameof(DtbBookingDirection.PIC), DtbBookingDirectionDescription.GetDescription(DtbBookingDirection.PIC));
						result.AddPair(nameof(DtbBookingDirection.DLV), DtbBookingDirectionDescription.GetDescription(DtbBookingDirection.DLV));

						return result;
					});
			}
		}

		public CodeDescriptionPairList DropModes_Containerized
		{
			get { return dropModes_Containerized ?? (dropModes_Containerized = new FCLEquipmentNeededList()); }
		}

		public CodeDescriptionPairList DropModes_Loose
		{
			get { return dropModes_Loose ?? (dropModes_Loose = new LCLAIREquipmentNeededList()); }
		}

		FCLEquipmentNeededList dropModes_Containerized;
		LCLAIREquipmentNeededList dropModes_Loose;

		public CodeDescriptionPairList IsOverriddenStatuses
		{
			get { return isOverriddenStatuses ?? (isOverriddenStatuses = new IsOverriddenStatuses().List); }
		}

		CodeDescriptionPairList isOverriddenStatuses;

		public CodeDescriptionPairList BookingQuoteStatuses
		{
			get { return bookingQuoteStatuses ?? (bookingQuoteStatuses = new BookingQuoteStatuses()); }
		}

		CodeDescriptionPairList bookingQuoteStatuses;

		public CodeDescriptionPairList IsHazardousStatuses
		{
			get { return isHazardousStatuses ?? (isHazardousStatuses = new IsHazardousStatuses().List); }
		}

		CodeDescriptionPairList isHazardousStatuses;

		public LocalCartageJobOrgTypeList OrganisationTypes
		{
			get { return LocalCartageJobOrgTypeList.Instance; }
		}

		public RefPackTypeCollection PackageTypes
		{
			get { return packageTypes ?? (packageTypes = new RefPackTypeCollection(Factory, excludeCNT: false)); }
		}

		RefPackTypeCollection packageTypes;

		public PackageCategories DefaultPackageTypes
		{
			get { return defaultPackageTypes ?? (defaultPackageTypes = new PackageCategories()); }
		}

		PackageCategories defaultPackageTypes;

		public CodeDescriptionPairList ParentJobTypes
		{
			get
			{
				return Factory.GetCachedValue("C91A55B5-8C62-485F-9E96-3715BDBE6AE1",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(StandAloneBookingCode, TransportCommon.Business.Res.GetString("88aa4250-fec8-4232-9530-4a3baf9fc05f", "Standalone Booking"));
						result.AddPair("CUS", Res.GetString("f7fc0813-474d-4830-bf48-f038b3e47386", "Customs Declaration"));
						result.AddPair(ForwardingShipmentCode, Res.GetString("ef6080ab-bd0a-4b1d-a72e-cb48ffe9260e", "Forwarding Shipment"));
						result.AddPair("CON", Res.GetString("6bd0f5dd-8984-4156-ba64-05a989e08407", "Forwarding Consolidation"));
						result.AddPair("ASH", Res.GetString("4f7d484a-d8e9-41b4-a52e-b30a7fda6f29", "Liner and Agency"));
						result.AddPair("WHO", Res.GetString("32e132eb-0d0d-404e-bcfa-dfb1066e72b9", "Warehouse Order"));
						result.AddPair("WHR", Res.GetString("ef589467-2358-46fa-afb9-f865df5ce01c", "Warehouse Receipt"));
						result.AddPair("TWD", Res.GetString("c9a5097a-029b-4ec9-bdc1-4c5f39bda04d", "Transit Warehouse Dispatch"));
						result.AddPair("HVC", Res.GetString("44cf068b-8139-4c27-be79-271b2968ac58", "HVLV Consignment"));
						result.AddPair("HVH", Res.GetString("15642188-ace5-4fa2-b9a4-f119b0df6ca6", "HVLV Booking Header"));
						return result;
					});
			}
		}

		public const string StandAloneBookingCode = "STB";
		public const string ForwardingShipmentCode = "SHP";
		public const string ForwardingBookingViewCode = "VB";

		public RatingFreightModes RatingFreightModes
		{
			get { return ratingFreightModes ?? (ratingFreightModes = new RatingFreightModes()); }
		}

		RatingFreightModes ratingFreightModes;

		public CodeDescriptionPairList RequiresRefrigerationStatuses
		{
			get { return requiresRefrigerationStatuses ?? (requiresRefrigerationStatuses = new RequiresRefrigerationStatuses().List); }
		}

		CodeDescriptionPairList requiresRefrigerationStatuses;

		public BookingShowStandaloneValues BookingShowStandaloneValues
		{
			get { return bookingShowStandaloneValues ?? (bookingShowStandaloneValues = new BookingShowStandaloneValues()); }
		}

		BookingShowStandaloneValues bookingShowStandaloneValues;

		public CodeDescriptionPairList IsMasterBookingStatuses
		{
			get { return isMasterBookingStatuses ?? (isMasterBookingStatuses = new IsMasterBookingStatuses().List); }
		}

		CodeDescriptionPairList isMasterBookingStatuses;

		public CodeDescriptionPairList IsSubBookingStatuses
		{
			get { return isSubBookingStatuses ?? (isSubBookingStatuses = new IsSubBookingStatuses().List); }
		}

		CodeDescriptionPairList isSubBookingStatuses;

		public BookingTransportModes BookingTransportModes
		{
			get { return bookingTransportModes ?? (bookingTransportModes = new BookingTransportModes()); }
		}

		BookingTransportModes bookingTransportModes;

		public CodeDescriptionPairList TemperatureUnits
		{
			get { return temperatureUnits ?? (temperatureUnits = new CodeDescriptionPairList(OLookUpEditType.TemperatureTypes)); }
		}
		CodeDescriptionPairList temperatureUnits;

		public CodeDescriptionPairList AirVentFlowRateUnits
		{
			get
			{
				if (airVentFlowRateUnits == null)
				{
					airVentFlowRateUnits = new CodeDescriptionPairList();
					airVentFlowRateUnits.AddPair("2L", Res.GetString("5dd9a9b7-a843-4f6a-a93a-ec493b715750", "Cubic feet per minute"));
					airVentFlowRateUnits.AddPair("MQH", Res.GetString("637fb9bb-813f-48b8-a706-81d0aa3afeb7", "Cubic meters per hour"));
					airVentFlowRateUnits.AddPair("P1", Res.GetString("e8a3b0f8-34f4-479b-be3d-b631ef4ea919", "Percent"));
				}
				return airVentFlowRateUnits;
			}
		}
		CodeDescriptionPairList airVentFlowRateUnits;

		public CodeDescriptionPairList DimensionUnits
		{
			get { return dimensionUnits ?? (dimensionUnits = new CodeDescriptionPairList(OLookUpEditType.Length)); }
		}
		CodeDescriptionPairList dimensionUnits;

		public CodeDescriptionPairList RadioactiveUnits
		{
			get { return radioactiveUnits ?? (radioactiveUnits = new CodeDescriptionPairList(OLookUpEditType.RadioactiveTypes)); }
		}
		CodeDescriptionPairList radioactiveUnits;
	}
}
