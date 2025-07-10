using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaManifestHeaderLookups : ManifestBase.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ExcessIndicatorList => Factory.GetCachedValue<ExcessIndicatorList>();

		public CodeDescriptionPairList CustomsStatusList => GetCachedRefCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus);

		public CodeDescriptionPairList RegistrationStatusList
		{
			get
			{
				return Factory.GetCachedValue("ZAOutturn.RegistrationStatusList", () =>
				{
					CodeDescriptionPairList list = new Common.Shared.AsycudaRegistrationStatuses();
					list.AddRange(new Common.ZA.ZAMessageStatusList());
					list.AddRange(GetCachedRefCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus));
					return list;
				});
			}
		}

		public CodeDescriptionPairList ManifestTypeList
		{
			get
			{
				var transportMode = Parent.AMA_TransportMode;
				return transportMode.IsEmpty
					? new CodeDescriptionPairList()
					: Factory.GetCachedValue(Invariant($"ZAOutturn.ManifestTypeList.{transportMode}"), () =>
					{
						var result = new CodeDescriptionPairList();
						switch (transportMode)
						{
							case Core.Constants.TransportModes.Air:
								result.AddPair(ZA.Business.ManifestTypeList.Codes.AirCargoOutturnReport, ZA.Business.ManifestTypeList.Descriptions.AirCargoOutturnReport);
								result.AddPair(ZA.Business.ManifestTypeList.Codes.AirExcessOutturnReport, ZA.Business.ManifestTypeList.Descriptions.AirExcessOutturnReport);
								result.AddPair(ZA.Business.ManifestTypeList.Codes.AirLoadDischarge, ZA.Business.ManifestTypeList.Descriptions.AirLoadDischarge);
								break;
							case Core.Constants.TransportModes.Sea:
								result.AddPair(ZA.Business.ManifestTypeList.Codes.BulkBreakBulkOutturnReport, ZA.Business.ManifestTypeList.Descriptions.BulkBreakBulkOutturnReport);
								result.AddPair(ZA.Business.ManifestTypeList.Codes.DepotOutturnReport, ZA.Business.ManifestTypeList.Descriptions.DepotOutturnReport);
								result.AddPair(ZA.Business.ManifestTypeList.Codes.VesselOutturnReport, ZA.Business.ManifestTypeList.Descriptions.VesselOutturnReport);
								break;
						}
						return result;
					});
			}
		}

		public CodeDescriptionPairList Natures => Factory.GetCachedValue<NatureList>();

		public CodeDescriptionPairList CustomsOfficeList => GetCachedRefCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);

		CodeDescriptionPairList GetCachedRefCusCodeList(ZString codeType)
		{
			return Enterprise.Customs.Universal.AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, codeType);
		}

		public CodeDescriptionPairList TransportModeList => Factory.GetCachedValue("ZAOutturn.TransportModeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air);
			result.AddPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea);
			return result;
		});

		public CodeDescriptionPairList ContainerModeList => Factory.GetCachedValue("ZAOutturn.ContainerModeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
			result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
			result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
			result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
			result.AddPair(Core.Constants.ContainerModes.Other, Core.Constants.ContainerModeDescriptions.Other);
			return result;
		});

		public CodeDescriptionPairList AgentTypeList => Factory.GetCachedValue("ZAOutturn.AgentTypeList", () => new CodeDescriptionPairList(OLookUpEditType.AgentType));

		public ShippingProviderCollection CarrierList =>
			Parent.IsAir
				? AirShippingLineList
				: Parent.IsSea
					? SeaShippingLineList
					: AirOrSeaShippingLineList;

		ShippingProviderCollection fAirOrSeaShippingLineList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		ShippingProviderCollection AirOrSeaShippingLineList => fAirOrSeaShippingLineList ?? (fAirOrSeaShippingLineList = new ShippingProviderCollection(Factory));

		AirShippingProviderCollection fAirShippingLineList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		AirShippingProviderCollection AirShippingLineList => fAirShippingLineList ?? (fAirShippingLineList = new AirShippingProviderCollection(Factory));

		SeaShippingProviderCollection fSeaShippingLineList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		SeaShippingProviderCollection SeaShippingLineList => fSeaShippingLineList ?? (fSeaShippingLineList = new SeaShippingProviderCollection(Factory));

		public ZZRefCusCodeListCombinedCollection OutturnProviderList
		{
			get
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					Core.Constants.CountryCodes.SouthAfrica,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities,
					ZDateTime.Today);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, false));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", (ZString)Core.Constants.CountryCodes.SouthAfrica, false));
				return result;
			}
		}

		public CodeDescriptionPairList GateInOutMessageTypeList
		{
			get
			{
				var res = new CodeDescriptionPairList();
				if (Parent.IsAir)
				{
					res.AddPair(GateInOutMessageTypeCodeList.Codes.AirDepotGateIn, GateInOutMessageTypeCodeList.Descriptions.AirDepotGateIn);
					res.AddPair(GateInOutMessageTypeCodeList.Codes.AirTerminalGateIn, GateInOutMessageTypeCodeList.Descriptions.AirTerminalGateIn);
				}
				if (Parent.IsSea)
				{
					res.AddPair(GateInOutMessageTypeCodeList.Codes.DepotGateIn, GateInOutMessageTypeCodeList.Descriptions.DepotGateIn);
					res.AddPair(GateInOutMessageTypeCodeList.Codes.DepotGateOut, GateInOutMessageTypeCodeList.Descriptions.DepotGateOut);
					res.AddPair(GateInOutMessageTypeCodeList.Codes.SeaDepotConsignmentGateIn, GateInOutMessageTypeCodeList.Descriptions.SeaDepotConsignmentGateIn);
					res.AddPair(GateInOutMessageTypeCodeList.Codes.BreakBulkGateIn, GateInOutMessageTypeCodeList.Descriptions.BreakBulkGateIn);
					res.AddPair(GateInOutMessageTypeCodeList.Codes.TerminalGateIn, GateInOutMessageTypeCodeList.Descriptions.TerminalGateIn);
					res.AddPair(GateInOutMessageTypeCodeList.Codes.TerminalGateOut, GateInOutMessageTypeCodeList.Descriptions.TerminalGateOut);
				}
				return res;
			}
		}

		public OrganisationsFindBoxCollection OrganizationsFindBoxList => new OrganisationsFindBoxCollection(Factory);
		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public CodeDescriptionPairList CustomsOffices
		{
			get
			{
				var ports = GetEffectivePortForCustomsOffices();
				return GetCustomsOfficesListForCountry(Factory, Parent.AMA_RN_NKCountry, ports.ToArray(), Parent.AMA_TransportMode);
			}
		}

		public static CodeDescriptionPairList GetCustomsOfficesListForCountry(BusinessObjectFactory factory, string countryCode, ZString[] ports = null, string transportMode = null)
		{
			return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				ZDateTime.Today, true, RefCusCodeListAttributeTypes.Codes.Port, ports ?? System.Array.Empty<ZString>(), transportMode ?? string.Empty);
		}

		List<ZString> GetEffectivePortForCustomsOffices()
		{
			var ports = new List<ZString>();

			ports.Add(Parent.MasterBill.ABL_RL_NKPortOfLoading);
			ports.Add(Parent.MasterBill.ABL_RL_NKPortOfDischarge);
			ports.RemoveAll(x => x.IsEmpty);
			return ports;
		}
	}
}
