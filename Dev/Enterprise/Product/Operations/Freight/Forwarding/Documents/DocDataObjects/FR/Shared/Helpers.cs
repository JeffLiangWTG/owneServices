using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	static class Helpers
	{
		public static ZString CreateFormattedProviderID(string pcs, string sPropertyType, ZPropertyInfo sPropertyInfo, ZPropertyInfo ci5PropertyInfo)
		{
			switch (pcs)
			{
				case FrenchPortsConstants.PCS.MGI:
					return !ci5PropertyInfo.Value.IsEmpty ? Res.GetString("ad99446c-a325-4772-982c-d24ebeb802fd", "CI5: {0}", ci5PropertyInfo.Value) : string.Empty;
				case FrenchPortsConstants.PCS.Soget:
					return !sPropertyInfo.Value.IsEmpty ? Res.GetString("300bf593-4d14-488b-8f22-bdb311204b84", "{0}: {1}", sPropertyType, sPropertyInfo.Value) : string.Empty;
				default:
					return string.Empty;
			}
		}

		#region Validations

		public static void AddPortLocationValidation(this ZPropertyInfo locationPropertyInfo, Func<ZString> getPort, string errorMessage = null)
		{
			if (locationPropertyInfo == null || getPort == null)
			{
				return;
			}

			locationPropertyInfo.AddMessageError(() =>
			{
				if (!locationPropertyInfo.Value.IsEmpty)
				{
					return false;
				}

				return LocationList.GetLocationList(getPort.Invoke(), string.Empty).ToArray().Any();
			}, errorMessage ?? Res.GetString("e901d4ec-117a-4027-8e8b-cabaf67ba521", "Port Location within Area is required."));

			locationPropertyInfo.AddAsciiCharactersValidation();
		}

		public static void AddPortAreaValidation(this ZPropertyInfo areaPropertyInfo, Func<ZString> getPort, string errorMessage = null)
		{
			if (areaPropertyInfo == null || getPort == null)
			{
				return;
			}

			areaPropertyInfo.AddMessageError(() =>
			{
				if (!areaPropertyInfo.Value.IsEmpty)
				{
					return false;
				}

				return AreaList.GetAreaList(getPort.Invoke()).ToArray().Any();
			}, errorMessage ?? Res.GetString("ea76f6c0-695d-4abd-be68-60979f837053", "Port Area is required."));

			areaPropertyInfo.AddAsciiCharactersValidation();
		}

		public static void AddPCSValidation(this ZPropertyInfo pcsPropertyInfo)
			=> pcsPropertyInfo.AddMessageErrorIfEmpty(Res.GetString("c9543ccc-675f-4c78-918a-1e264f25238e", "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)"));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		public static void AddFormattedProviderIDValidations(this ZPropertyInfo formattedProviderIDPropertyInfo, string pcs, string sPropertyType, string partyType = "", string partyCodeType = "", string configPath = "", bool isOrganization = false, string extraConfigPath = "")
		{
			switch (pcs)
			{
				case FrenchPortsConstants.PCS.MGI:
					if (!string.IsNullOrEmpty(extraConfigPath))
					{
						extraConfigPath = string.Format((NoResString)"OR CI5 code in {0} > Config > Registration Numbers / Codes[Type = CI5],", extraConfigPath);
					}

					if (isOrganization)
					{
						formattedProviderIDPropertyInfo.AddMessageErrorIfEmpty(string.Format("{0} Port Community System (PCS) code missing from Organization {1} > Config > Registration Numbers/Codes [Type=CI5].", partyType, configPath));
					}
					else
					{
						if (string.IsNullOrEmpty(extraConfigPath))
						{
							formattedProviderIDPropertyInfo.AddMessageErrorIfEmpty(string.Format("{0} Port Community System (PCS) code is required.\r\nProvide {1} Code of Operational Port in Registry > Freight > Port Messaging > France > Port Community System Code of Forwarder and Agent,\r\nOR CI5 code in {2} > Config > Registration Numbers / Codes[Type = CI5].", partyType, partyCodeType, configPath));
						}
						else
						{
							formattedProviderIDPropertyInfo.AddMessageErrorIfEmpty(string.Format("{0} Port Community System (PCS) code is required.\r\nProvide {1} Code of Operational Port in Registry > Freight > Port Messaging > France > Port Community System Code of Forwarder and Agent,\r\n{3}\r\nOR CI5 code in {2} > Config > Registration Numbers / Codes[Type = CI5].", partyType, partyCodeType, configPath, extraConfigPath));
						}
					}
					break;
				case FrenchPortsConstants.PCS.Soget:
					if (!string.IsNullOrEmpty(extraConfigPath))
					{
						extraConfigPath = string.Format("OR {1} code in {0} > Config > Registration Numbers / Codes[Type = {1}],", extraConfigPath, sPropertyType);
					}

					if (isOrganization)
					{
						formattedProviderIDPropertyInfo.AddMessageErrorIfEmpty(string.Format("{0} Port Community System (PCS) code missing from Organization {1} > Config > Registration Numbers/Codes [Type={2}].", partyType, configPath, sPropertyType));
					}
					else
					{
						if (string.IsNullOrEmpty(extraConfigPath))
						{
							formattedProviderIDPropertyInfo.AddMessageErrorIfEmpty(string.Format("{0} Port Community System (PCS) code is required.\r\nProvide {1} Code of Operational Port in Registry > Freight > Port Messaging > France > Port Community System Code of Forwarder and Agent,\r\nOR {3} code in {2} > Config > Registration Numbers / Codes[Type = {3}].", partyType, partyCodeType, configPath, sPropertyType));
						}
						else
						{
							formattedProviderIDPropertyInfo.AddMessageErrorIfEmpty(string.Format("{0} Port Community System (PCS) code is required.\r\nProvide {1} Code of Operational Port in Registry > Freight > Port Messaging > France > Port Community System Code of Forwarder and Agent,\r\n{4}\r\nOR {3} code in {2} > Config > Registration Numbers / Codes[Type = {3}].", partyType, partyCodeType, configPath, sPropertyType, extraConfigPath));
						}
					}
					break;
				default:
					formattedProviderIDPropertyInfo.AddMessageErrorIfEmpty(string.Format("{0} Port Community System (PCS) code cannot be defaulted due to error on PCS field. Verify error on PCS field.", partyType));
					break;
			}
		}

		#endregion

		public static IRefUNLOCO GetFranceTransshipmentPort(this IEnumerable<Freight.Business.Transport> seaTransportsInLegOrder, ZBool isExport)
		{
			if (seaTransportsInLegOrder == null)
			{
				return null;
			}

			var franceAndDependentCountriesCodes = new ZString[]
			{
				Core.Constants.CountryCodes.France,
				Core.Constants.CountryCodes.FrenchGuyana,
				Core.Constants.CountryCodes.FrenchPolynesia,
				Core.Constants.CountryCodes.Guadeloupe,
				Core.Constants.CountryCodes.Martinique,
				Core.Constants.CountryCodes.Mayotte,
				Core.Constants.CountryCodes.NewCaledonia,
				Core.Constants.CountryCodes.SaintBarthelemy,
				Core.Constants.CountryCodes.SaintMartin,
				Core.Constants.CountryCodes.StPierreEtMiquelon,
				Core.Constants.CountryCodes.WallisAndFutunaIslands
			};

			if (isExport)
			{
				Freight.Business.Transport firstDepartureFRSeaLeg = null;
				foreach (var transport in seaTransportsInLegOrder)
				{
					if (firstDepartureFRSeaLeg == null && franceAndDependentCountriesCodes.Contains(transport.JW_RL_NKLoadPort.Left(2)))
					{
						firstDepartureFRSeaLeg = transport;
					}
					else if (firstDepartureFRSeaLeg != null)
					{
						return transport.LoadPort;
					}
				}
			}
			else
			{
				Freight.Business.Transport lastArrivalFRSeaLeg = null;

				foreach (var transport in seaTransportsInLegOrder.Reverse())
				{
					if (lastArrivalFRSeaLeg == null && franceAndDependentCountriesCodes.Contains(transport.JW_RL_NKDiscPort.Left(2)))
					{
						lastArrivalFRSeaLeg = transport;
					}
					else if (lastArrivalFRSeaLeg != null)
					{
						return transport.DiscPort;
					}
				}
			}

			return null;
		}
	}
}
