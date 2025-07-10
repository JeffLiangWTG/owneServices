using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.GvmsReferenceData
{
	public static class GvmsReferenceDataConverter
	{
		public static RefCarrierCode ConvertCarrier(Carrier carrier)
		{
			RefCarrierCode refCarrierCode = new RefCarrierCode
			{
				ZZ4_Code = carrier.CarrierId,
				ZZ4_Description = carrier.CarrierName,
				ZZ4_ZZZ_NKDataGrouping = CargoWise.RefDbRepo.GBReferenceData.Business.Constants.DefaultValues.GBDataGrouping
			};

			RefCarrierCodeAttribute refCarrierCodeAttribute = new RefCarrierCodeAttribute
			{
				ZZG_Name = Constants.AttributeNames.Nationality,
				ZZG_Value = carrier.CountryCode
			};

			refCarrierCode.RefCarrierCodeAttributes = new RefCarrierCodeAttribute[] { refCarrierCodeAttribute };

			return refCarrierCode;
		}

		public static RefCusCodeList ConvertRoute(Route route, List<Port> allPorts, List<Carrier> allCarriers)
		{
			RefCusCodeList refCusCodeList = new RefCusCodeList
			{
				ZZD_Code = route.RouteId,
				ZZD_Description = string.Format(CultureInfo.CurrentCulture, "Route #{0} from {1} ({2}) to {3} ({4}) via {5} ({6})",
				route.RouteId,
				allPorts.Find(x => x.PortId == route.DeparturePortId)?.PortDescription ?? "Unknown",
				route.DeparturePortId,
				allPorts.Find(x => x.PortId == route.ArrivalPortId)?.PortDescription ?? "Unknown",
				route.ArrivalPortId,
				allCarriers.Find(x => x.CarrierId == route.CarrierId)?.CarrierName ?? "Unknown",
				route.CarrierId),
				ZZD_ZZZ_NKDataGrouping = Constants.DefaultValues.GBDataGrouping,
				ZZD_ZZK_NKCodeType = Constants.GvmsDefaults.Codes.GvmsRoutes,
				ZZD_StartDate = route.RouteEffectiveFrom,
				ZZD_EndDate = CommonHelper.CalcMaxDate(route.RouteEffectiveTo)
			};

			RefCusCodeListAttribute arrivalPortId = new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.ArrivalPortId,
				ZZE_Value = route.ArrivalPortId
			};

			RefCusCodeListAttribute departurePortId = new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.DeparturePortId,
				ZZE_Value = route.DeparturePortId
			};

			RefCusCodeListAttribute carrier = new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.Carrier,
				ZZE_Value = route.CarrierId
			};

			RefCusCodeListAttribute direction = new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.Direction,
				ZZE_Value = route.RouteDirection
			};

			refCusCodeList.RefCusCodeListAttributes = new RefCusCodeListAttribute[] { arrivalPortId, departurePortId, carrier, direction };

			return refCusCodeList;
		}

		public static RefCusCodeList ConvertRuleFailure(RuleFailure ruleFailure)
		{
			RefCusCodeList refCusCodeList = new RefCusCodeList
			{
				ZZD_Code = ruleFailure.RuleId,
				ZZD_ZZK_NKCodeType = Constants.GvmsDefaults.Codes.ErrorCode,
				ZZD_ZZZ_NKDataGrouping = Constants.DefaultValues.GBDataGrouping,
				ZZD_Description = ruleFailure.RuleDescription
			};

			RefCusCodeListAttribute refCusCodeListAttribute = new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.Category,
				ZZE_Value = Constants.GvmsDefaults.GVMS
			};

			refCusCodeList.RefCusCodeListAttributes = new RefCusCodeListAttribute[] { refCusCodeListAttribute };

			return refCusCodeList;
		}

		public static RefCusCodeList ConvertGBPort(Port port, string cdsOrChiefPortCode, string dataGrouping, string locationType = "")
		{
			RefCusCodeList refCusCodeList = new RefCusCodeList
			{
				ZZD_Code = cdsOrChiefPortCode,
				ZZD_Description = port.PortDescription,
				ZZD_ZZK_NKCodeType = Constants.GvmsDefaults.Codes.PORT,
				ZZD_ZZZ_NKDataGrouping = dataGrouping
			};

			RefCusCodeListAttribute refCusCodeListGvmsAttribute = new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.GvmsPortId,
				ZZE_Value = port.PortId
			};

			refCusCodeList.RefCusCodeListAttributes = new RefCusCodeListAttribute[] { refCusCodeListGvmsAttribute };

			if (!string.IsNullOrEmpty(locationType))
			{
				RefCusCodeListAttribute refCusCodeListFactyAttribute = new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.AttributeNames.LocationTypeCode,
					ZZE_Value = locationType
				};

				refCusCodeList.RefCusCodeListAttributes = new RefCusCodeListAttribute[] { refCusCodeListGvmsAttribute, refCusCodeListFactyAttribute };
			}

			return refCusCodeList;
		}

		public static RefCusCodeList ConvertChiefPort(Port port)
		{
			return ConvertGBPort(port, port.ChiefPortCode, Constants.DefaultValues.GBDataGrouping);
		}

		public static RefCusCodeList ConvertCDSPort(Port port)
		{
			string portCode = port.CdsPortCode;
			string locationType = "";
			if (IsCdsPortLongFormat(port))
			{
				locationType = portCode.Substring(2, 2);
				portCode = portCode.Substring(4);
			}
			return ConvertGBPort(port, portCode, Constants.DefaultValues.CDSDataGrouping, locationType);
		}

		public static List<RefLocoMap> ConvertForeignPort(Port port)
		{
			var refLocoMaps = new List<RefLocoMap>();
			var portId = port.PortId;
			if (!string.IsNullOrEmpty(portId) && GvmsUnlocoLookupConfig.Instance.GvmsCodeUnlocoLookup.ContainsKey(portId))
			{
				var unlocosFromForeignPortId = GvmsUnlocoLookupConfig.Instance.GvmsCodeUnlocoLookup[portId];
				foreach (var unloco in unlocosFromForeignPortId)
				{
					RefLocoMap refLocoMap = new RefLocoMap();
					refLocoMap.RY_LocalPortCode = port.PortId;
					refLocoMap.RY_RL_NKLocoPort = unloco;
					refLocoMaps.Add(refLocoMap);
				}
			}

			return refLocoMaps;
		}

		public static RefCusCodeList ConvertInspectionLocation(InspectionLocation location)
		{
			var refCusCodeList = new RefCusCodeList
			{
				ZZD_Code = location.LocationId,
				ZZD_Description = location.LocationDescription,
				ZZD_ZZK_NKCodeType = Constants.GvmsDefaults.Codes.GVMSIL,
				ZZD_ZZZ_NKDataGrouping = Constants.DefaultValues.GBDataGrouping,
				ZZD_StartDate = location.LocationEffectiveFrom,
				ZZD_EndDate = CommonHelper.CalcMaxDate(location.LocationEffectiveTo),
			};

			var refGvmsAddressAttribute = new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.GvmsAddress,
				ZZE_Value = location.Address.ToString(),
			};

			var refGvmsTypeAttribute = new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.GvmsType,
				ZZE_Value = GetLocationType(location),
			};

			refCusCodeList.RefCusCodeListAttributes = new RefCusCodeListAttribute[] { refGvmsAddressAttribute, refGvmsTypeAttribute };
			return refCusCodeList;
		}

		public static RefCusCodeList ConvertInspectionType(InspectionType inspectionType)
		{
			var refCusCodeList = new RefCusCodeList
			{
				ZZD_Code = inspectionType.InspectionTypeId,
				ZZD_Description = inspectionType.Description,
				ZZD_ZZK_NKCodeType = Constants.GvmsDefaults.Codes.GVMSIT,
				ZZD_ZZZ_NKDataGrouping = Constants.DefaultValues.GBDataGrouping,
				ZZD_StartDate = DefaultValues.MinimumDateTime,
				ZZD_EndDate = DefaultValues.MaximumDateTime,
			};

			return refCusCodeList;
		}

		public static bool IsCdsPort(Port port) => !string.IsNullOrEmpty(port.CdsPortCode);
		public static bool IsCdsPortLongFormat(Port port) => port.CdsPortCode.Length >= 13;
		public static bool IsChiefPort(Port port) => !string.IsNullOrEmpty(port.ChiefPortCode);
		public static bool IsForeignPort(Port port) => string.IsNullOrEmpty(port.ChiefPortCode) && string.IsNullOrEmpty(port.CdsPortCode);

		static string GetLocationType(InspectionLocation location)
		{
			var locationType = location.LocationType ?? string.Empty;
			switch (locationType)
			{
				case "ALL":
					return "A facility that carries out all inspection types";
				case "BCP":
					return "Border Control Post";
				case "IBF":
					return "Inland Border Facility";
				default:
					return locationType;
			}
		}
	}
}
