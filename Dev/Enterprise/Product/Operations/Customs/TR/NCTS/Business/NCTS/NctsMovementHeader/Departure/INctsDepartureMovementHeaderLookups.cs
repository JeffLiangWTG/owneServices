using System.Collections;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business;

public interface INctsDepartureMovementHeaderLookups
{
	CodeDescriptionPairList GoodsShippingLocationList { get; }
	CodeDescriptionPairList CustomsStatusList { get; }
	CodeDescriptionPairList DeclarationTypeList { get; }
	CodeDescriptionPairList SpecificCircumstanceIndicatorList { get; }
	CodeDescriptionPairList GoodsShiptoCodeList { get; }
	ZZRefCusCodeListCombinedCollection TRWarehouseList { get; }
	CodeDescriptionPairList TypeOfSecurityList { get; }
	CodeDescriptionPairList AdditionalDeclarationTypeList { get; }
	ICollection ForeignDestPortCodes { get; }
	ICollection PortOfPresentationCodes { get; }
	BondedWarehouseCollection BondedWarehouseCollection { get; }
	OrgHeaderCollection Organisations { get; }
	CodeDescriptionPairList NctsSpecificCircumstanceIndicatorList { get; }
	CodeDescriptionPairList CountryOfDestinationList { get; }
	CodeDescriptionPairList CountryOfDispatchList { get; }
	CodeDescriptionPairList BorderModeOfTransportList { get; }
	CodeDescriptionPairList ModeOfTransportList { get; }
	GlbStaffCollection CusAgents { get; }
	CodeDescriptionPairList TransportChargesModeOfPaymentList { get; }
	RefUNLOCOCollection ForeignDestPorts { get; }
	ICollection TOLCarrierNationalityList { get; }
	ZZRefCusCodeListCombinedCollection TransportNationalityList { get; }
	RefCountryCollection TransportAtDepartureTrailer1Nationalities { get; }
	RefCountryCollection TransportAtDepartureTrailer2Nationalities { get; }
	CodeDescriptionPairList SealTypeList { get; }
	CodeDescriptionPairList NctsMovementHeaderTransactionStatusList { get; }
	CodeDescriptionPairList NctsMessageStatusList { get; }
	CodeDescriptionPairList TransportAtDepartureTypeOfIdList { get; }
	RefVesselCollection Vessels { get; }
	ICollection TOLCarrierIDList { get; }
	CodeDescriptionPairList TransportAtBorderTypeOfIdList { get; }
	CodeDescriptionPairList LocationOfGoodsCodeList { get; }
	CodeDescriptionPairList NctsControlResultList { get; }
	CodeDescriptionPairList NctsTransitStatusList { get; }
	CodeDescriptionPairList OfficeCodeList { get; }
	RefUNLOCOCollection PortsOfUnloading { get; }
	OrganisationsFindBoxCollection Representatives { get; }
	RefCountryCollection TOLCarrierNationalities { get; }
	RefCountryCollection TransportAtDepartureCountries { get; }
	CodeDescriptionPairList WeightUnitList { get; }
	CodeDescriptionPairList TankerStatusList { get; }
}
