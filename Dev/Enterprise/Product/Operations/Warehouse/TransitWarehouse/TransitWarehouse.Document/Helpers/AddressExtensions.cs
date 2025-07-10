using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.CountryCodes;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Warehouse.Transit.Document
{
	static class AddressExtensions
	{
		#region GetRegistrationNumberWithFallback

		public static RegistrationNumber GetRegistrationNumberWithFallback(this OrgAddress address, IContext context, string type)
		{
			return address.GetRegistrationNumberWithFallback(context, type, true, null);
		}

		public static RegistrationNumber GetRegistrationNumberWithFallback(this OrgAddress address, IContext context, string type, bool isForwarderCode, IUnloco operationalPort)
		{
			var registrationNumber = CreateRegistrationNumber(context, type);
			registrationNumber.Value = GetOrgCusCodeWithFallback(address, type, isForwarderCode, operationalPort);

			return registrationNumber;
		}

		static RegistrationNumber CreateRegistrationNumber(IContext context, string type)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = type
				}
			};
		}

		static OrgCusCode GetAPPlusOrgCusCode(IEnumerable<OrgCusCode> customsCodes, string type, OrgAddress premisesAddress = null)
		{
			return customsCodes?.FirstOrDefault(x => (premisesAddress == null || premisesAddress.PK == x.OK_OA_PremisesAddress) && x.OK_CodeType == type && IsFranceOrTerritory(x.OK_RN_NKCodeCountry));
		}

		public static ZString GetOrgCusCodeWithFallback(OrgAddress address, string type, bool isForwarderCode = true, IUnloco operationalPort = null)
		{
			var result = GetOrgCusCode(type, isForwarderCode, operationalPort);

			if (result.IsEmpty)
			{
				var orgCusCode = GetAPPlusOrgCusCode(address?.CustomsCodes.OfType<OrgCusCode>(), type, address)
					?? GetAPPlusOrgCusCode(address?.Header?.CustomsCodes.OfType<OrgCusCode>().Where(c => c.PremisesAddress == null), type);
				result = orgCusCode?.OK_CustomsRegNo ?? ZString.Empty;
			}

			return result;
		}
		public static ZString GetOrgCusCode(string type, bool isForwarderCode = true, IUnloco operationalPort = null)
		{
			var result = ZString.Empty;
			if (operationalPort != null)
			{
				var pcsType = type != null && type == OrgCusCode.FranceCodeTypes.CI5 ? FrenchPortSystemCodeList.Codes.MGI : FrenchPortSystemCodeList.Codes.SOGET;

				var communitySystemCodesOfForwarderAndAgent = PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)
					.OfType<CommunitySystemCodesOfForwarderAndAgent>()
					.FirstOrDefault(x => x.Port == operationalPort.Code && x.PCS == pcsType);

				if (communitySystemCodesOfForwarderAndAgent != null)
				{
					result = isForwarderCode ? communitySystemCodesOfForwarderAndAgent.ForwarderCode : communitySystemCodesOfForwarderAndAgent.AgentCode;
				}
			}

			return result;
		}

		public static ZString GetPortArea(this OrgAddress address)
		{
			var regNo = GetOrgCusCodeWithFallback(address, OrgCusCode.CodeTypes.PortSystemNumber);
			if (!string.IsNullOrEmpty(regNo))
			{
				var splitArray = regNo.Split('\\');
				return (splitArray.Length == 1) ? ZString.Empty : splitArray[0];
			}

			return ZString.Empty;
		}

		public static ZString GetPortLocation(this OrgAddress address)
		{
			var regNo = GetOrgCusCodeWithFallback(address, OrgCusCode.CodeTypes.PortSystemNumber);
			if (!string.IsNullOrEmpty(regNo))
			{
				var splitArray = regNo.Split('\\');
				return (splitArray.Length == 1) ? splitArray[0] : splitArray[1];
			}

			return ZString.Empty;
		}

		#endregion
	}
}
