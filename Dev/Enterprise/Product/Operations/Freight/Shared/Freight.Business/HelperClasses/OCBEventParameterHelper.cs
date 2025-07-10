using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public static class OCBEventParameterHelper
	{
		public static Dictionary<string, string> GetParametersForEvent(string typeValue, string newValue, string old, string maximum, string quantity, string status, string company)
		{
			var result = new Dictionary<string, string>();

			result.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, typeValue);
			result.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, newValue);
			result.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, old);
			result.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Maximum, maximum);
			result.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Quantity, quantity);
			result.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, status);
			result.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Company, company);

			return result;
		}

		public static string GetSCAC(CommonConsol consol)
		{
			if (consol == null)
			{
				return string.Empty;
			}

			var orgHeader = (consol.IsCoLoad ? consol.Creditor : consol.ShippingLine);
			var refShippingLine = orgHeader?.ShippingLine;

			if (refShippingLine != null)
			{
				return refShippingLine.RSL_StandardCarrierAlphaCode.IsEmpty ? refShippingLine.RSL_CargoWiseOneCode : refShippingLine.RSL_StandardCarrierAlphaCode;
			}

			var orgCusCode = GetOrgCusCodeCarrierWithFallback(orgHeader);
			return orgCusCode?.OK_CustomsRegNo ?? string.Empty;
		}

		public static RefShippingLine GetRefShippingLineFromCarrierWithFallback(CommonConsol consol)
		{
			var orgHeader = (consol.IsCoLoad ? consol.Creditor : consol.ShippingLine);
			var orgCusCode = GetOrgCusCodeCarrierWithFallback(orgHeader);

			if (!(orgCusCode?.OK_CustomsRegNo ?? ZString.Empty).IsEmpty)
			{
				switch (orgCusCode.OK_CodeType)
				{
					case OrgCusCode.CodeTypes.CarrierCode:
						return consol.Factory.LoadFromNaturalKey<RefShippingLine>(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, orgCusCode.OK_CustomsRegNo);
					case OrgCusCode.CodeTypes.CargoWiseOneCarrierCode:
						return GetActiveRefShippingLineFromCargoWiseOneCarrierCode(consol, orgCusCode.OK_CustomsRegNo);
				}
			}

			return null;
		}

		static RefShippingLine GetActiveRefShippingLineFromCargoWiseOneCarrierCode(CommonConsol consol, ZString cargoWiseOneCode)
		{
			var query = new ZQuery(RefShippingLineSchema.RSL_CargoWiseOneCode, cargoWiseOneCode);
			query.AddToFilter(RefShippingLineSchema.RSL_IsActive, true);
			return consol.Factory.Load<RefShippingLine>(query).FirstOrDefault();
		}

		static OrgCusCode GetOrgCusCodeCarrierWithFallback(OrgHeader orgHeader)
		{
			if (orgHeader == null)
			{
				return null;
			}

			var customsCodes = orgHeader?.CustomsCodes;

			var usCCCOrgCusCode = customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Constants.CountryCodes.UnitedStates);
			if (!(usCCCOrgCusCode?.OK_CustomsRegNo ?? ZString.Empty).IsEmpty)
			{
				return usCCCOrgCusCode;
			}

			return customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, ZString.Empty);
		}

		public static decimal GetTotalTEU(CommonConsol consol)
		{
			var totalTEU = 0m;

			if (consol != null)
			{
				var containers = consol.Containers.OfType<CommonContainer>().Where(c => c.RefContainer != null && c.JC_ContainerCount != 0);

				foreach (var container in containers)
				{
					var refContainer = container.RefContainer;

					if (!refContainer.RC_TEU.IsEmpty && refContainer.RC_TEU > 0m)
					{
						totalTEU += refContainer.RC_TEU * container.JC_ContainerCount;
					}
					else
					{
						totalTEU += TransformByISOType(refContainer.RC_ISOType.SubstringSafe(0, 1)) * container.JC_ContainerCount;
					}
				}
			}

			return totalTEU;
		}

		static decimal TransformByISOType(string type)
		{
			decimal result;
			switch (type)
			{
				case "1":
					result = 10m;
					break;
				case "2":
					result = 20m;
					break;
				case "3":
					result = 30m;
					break;
				case "4":
					result = 40m;
					break;
				case "A":
					result = 23.5m;
					break;
				case "B":
					result = 24m;
					break;
				case "C":
					result = 24.5m;
					break;
				case "D":
					result = 24.5m;
					break;
				case "E":
					result = 25.7m;
					break;
				case "F":
					result = 26.6m;
					break;
				case "G":
					result = 41m;
					break;
				case "H":
					result = 43m;
					break;
				case "K":
					result = 44.6m;
					break;
				case "L":
					result = 45m;
					break;
				case "M":
					result = 48m;
					break;
				case "N":
					result = 49m;
					break;
				case "P":
					result = 53m;
					break;
				default:
					result = 40m;
					break;
			}
			return decimal.Round(result / 20m, 2);
		}
	}
}
