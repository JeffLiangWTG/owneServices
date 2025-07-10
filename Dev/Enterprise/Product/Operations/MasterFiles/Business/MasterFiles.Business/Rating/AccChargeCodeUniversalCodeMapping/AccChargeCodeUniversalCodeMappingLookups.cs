//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeCodeUniversalCodeMappingLookups
//
//    This class should be used for overriding collections in AutoAccChargeCodeUniversalCodeMappingLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.MasterFiles.Business.Rating;

public class AccChargeCodeUniversalCodeMappingLookups(AutoAccChargeCodeUniversalCodeMapping parent) : AutoAccChargeCodeUniversalCodeMappingLookups(parent)
{
	AccChargeCodeUniversalCodeMapping CodeMapping => Parent as AccChargeCodeUniversalCodeMapping;

	public IBusinessObjectCollection ChargeCodeBizoCollection =>
		CodeMapping.AUP_Type == AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Carrier
			? new CarrierChargeCodeBizoCollection(Factory)
			: new UniversalChargeCodeBizoCollection(Factory);

	public ReadOnlyCodeDescriptionPairList ChargeCodeMappingTypeList { get; } = new CodeDescriptionPairList()
	{
		new CodeDescriptionPair(
			AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Universal,
			AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypeDescriptions.Universal),
		new CodeDescriptionPair(
			AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Carrier,
			AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypeDescriptions.Carrier),
	};

	public ReadOnlyCodeDescriptionPairList TransportModeList { get; } = new CodeDescriptionPairList()
	{
		new CodeDescriptionPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea),
		new CodeDescriptionPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air),
	};

	public static ChargeCodeWithMappingInfo[] GetUniversalChargeCodesWithMappingInfo(BusinessObjectFactory factory) =>
		factory.GetCachedValue(
			"UniversalChargeCodesWithMappingInfo",
			() =>
			{
				var requestId = WiseRatesClient.GenerateTraceID();
				var clientFactory = ObjectFactory.Get<IWiseRatesClientFactory>();
				var (client, _) = clientFactory.TryCreate(requestId);
				return client?.GetChargeCodesWithMappingInfo() ?? [];
			});

	public static ChargeCodeWithMappingInfo[] GetAllUniversalChargeCodesWithMappingInfo(BusinessObjectFactory factory) =>
		factory.GetCachedValue(
			"AllUniversalChargeCodesWithMappingInfo",
			() =>
			{
				var requestId = WiseRatesClient.GenerateTraceID();
				var clientFactory = ObjectFactory.Get<IWiseRatesClientFactory>();
				var (client, _) = clientFactory.TryCreate(requestId);
				return client?.GetAllChargeCodesWithMappingInfo() ?? [];
			});

	public static ChargeCodeWithMappingInfo[] GetAllChargeCodesWithMappingInfoLinkedToCarrierChargeCode(BusinessObjectFactory factory, string carrierChargeCode, string transportMode) =>
		GetAllUniversalChargeCodesWithMappingInfo(factory)
			.Where(mapping => mapping.Provider == ConvertTransportModeToProvider(transportMode)
				&& mapping.ForeignCode == carrierChargeCode)
				.ToArray();

	public static bool IsCarrierUnrestrictedForChargeCode(BusinessObjectFactory factory, string carrierChargeCode, string transportMode)
		=> GetAllChargeCodesWithMappingInfoLinkedToCarrierChargeCode(factory, carrierChargeCode, transportMode)
			.Any(mapping => string.IsNullOrEmpty(mapping.Carrier));

	public static bool TryGetCarrierFromScac(BusinessObjectFactory factory, string scac, out OrgHeader orgHeader)
	{
		orgHeader = null;

		var refShippingLineSubQuery = new ZDBOnlySubQuery(typeof(RefShippingLine), RefShippingLineSchema.PK);
		refShippingLineSubQuery.AddToFilter(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, scac);

		var carrierQuery = new ZDBOnlyQuery(typeof(OrgHeader));
		carrierQuery.AddSubQuery(OrgHeaderSchema.OH_RSL_ShippingLine, refShippingLineSubQuery, JoinCondition.And);
		carrierQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);

		var orgs = factory.Load<OrgHeader>(carrierQuery);

		if (orgs.Length != 1)
		{
			return false;
		}

		orgHeader = orgs[0];
		return true;
	}

	public static bool TryGetCarrierFromIata(BusinessObjectFactory factory, string iata, out OrgHeader orgHeader)
	{
		orgHeader = null;

		var airLineSubQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK, OrgMiscServSchema.OM_RM_Airline);
		airLineSubQuery.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty);
		if (iata.Length == 2)
		{
			airLineSubQuery.AddToFilter(RefAirlineSchema.RM_TwoCharacterCode, iata);
		}
		else if (iata.Length == 3)
		{
			airLineSubQuery.AddToFilter(RefAirlineSchema.RM_ThreeLetterCode, iata);
		}

		var orgMiscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH, OrgHeaderSchema.PK);
		orgMiscServSubQuery.AddSubQuery(airLineSubQuery, JoinCondition.And);

		var organizationQuery = new ZDBOnlyQuery(typeof(OrgHeader));
		organizationQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
		organizationQuery.AddSubQuery(orgMiscServSubQuery, JoinCondition.And);

		var orgs = factory.Load<OrgHeader>(organizationQuery);

		if (orgs.Length < 1)
		{
			return false;
		}

		orgHeader = orgs[0];
		return true;
	}

	public static OrgHeaderCollection TryGetAllCarriersFromCarrierChargeCode(BusinessObjectFactory factory, string carrierChargeCode, string transportMode)
	{
		var carriers = new OrgHeaderCollection(factory);
		var allCarriersLinkedToCarrierChargeCode =
			GetAllChargeCodesWithMappingInfoLinkedToCarrierChargeCode(factory, carrierChargeCode, transportMode).Where(code => !string.IsNullOrEmpty(code.Carrier));

		if (transportMode.Equals(Core.Constants.TransportModes.Sea))
		{
			carriers.AddRange(
				allCarriersLinkedToCarrierChargeCode
					.Select(code => TryGetCarrierFromScac(factory, code.Carrier, out var seaCarrier) ? seaCarrier : null)
					.Where(seaCarrier => seaCarrier != null)
			);
		}
		else if (transportMode.Equals(Core.Constants.TransportModes.Air))
		{
			carriers.AddRange(
				allCarriersLinkedToCarrierChargeCode
					.Select(code => TryGetCarrierFromIata(factory, code.Carrier, out var airCarrier) ? airCarrier : null)
					.Where(airCarrier => airCarrier != null)
			);
		}

		return carriers;
	}

	public static string GetCarrierChargeCodeDescription(BusinessObjectFactory factory, string carrierChargeCode, string transportMode, string carrier)
	{
		var chargeCodeMappings = GetAllChargeCodesWithMappingInfoLinkedToCarrierChargeCode(factory, carrierChargeCode, transportMode);

		if (string.IsNullOrEmpty(carrier))
		{
			return chargeCodeMappings.FirstOrDefault(mapping => string.IsNullOrEmpty(mapping.Carrier))?.ForeignName
				?? string.Empty;
		}

		return chargeCodeMappings.FirstOrDefault(mapping => mapping.Carrier == carrier)?.ForeignName
			?? chargeCodeMappings.FirstOrDefault(mapping => string.IsNullOrEmpty(mapping.Carrier))?.ForeignName
			?? string.Empty;
	}

	static string ConvertTransportModeToProvider(string transportMode) =>
		transportMode switch
		{
			Core.Constants.TransportModes.Sea => WRConstants.RateProviders.CargoSphere,
			Core.Constants.TransportModes.Air => WRConstants.RateProviders.CargoGuide,
			_ => string.Empty,
		};
}
