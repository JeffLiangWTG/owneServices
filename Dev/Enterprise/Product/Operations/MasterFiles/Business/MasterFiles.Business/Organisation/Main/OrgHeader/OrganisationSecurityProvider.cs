using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationSecurityProvider
	{
		public OrganisationSecurityProvider(OrgHeader header)
		{
			this.header = header;
		}

		readonly OrgHeader header;

		#region HasSecurityByName

		/// <summary>
		/// This method reflects the security item by name from the internal property list. 
		/// </summary>
		/// <param name="securityProviderItemName">The name of the security item to search</param>
		/// <returns>If it finds a security item by the name provided, it returns that.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public bool HasSecurityByName(string securityProviderItemName)
		{
			string sanitisedName = securityProviderItemName.StartsWith((NoResString)"Is") ? securityProviderItemName.Remove(0, 2) : securityProviderItemName;
			sanitisedName = sanitisedName.EndsWith((NoResString)"Security") ? sanitisedName.Remove(sanitisedName.LastIndexOf("Security")) : sanitisedName;
			sanitisedName = "Has" + sanitisedName;

			PropertyInfo matchedProperty;
			PropertyItemCache.TryGetValue(sanitisedName, out matchedProperty);

			if (matchedProperty == null)
			{
				sanitisedName = sanitisedName + "Security";
				PropertyItemCache.TryGetValue(sanitisedName, out matchedProperty);
			}

			if (matchedProperty == null)
			{
				throw new ArgumentException("The security provider item name provided (" + securityProviderItemName + ") was not a recognised provider. Please check that your security provider is correctly declared in the class " + this.GetType().Name);
			}

			return (bool)matchedProperty.GetValue(this, null);
		}

		#endregion

		#region Property Item Cache

		Dictionary<string, PropertyInfo> PropertyItemCache
		{
			get
			{
				if (fPropertyItemCache == null)
				{
					fPropertyItemCache = new Dictionary<string, PropertyInfo>();
					foreach (PropertyInfo property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
					{
						fPropertyItemCache.Add(property.Name, property);
					}
				}

				return fPropertyItemCache;
			}
		}

		Dictionary<string, PropertyInfo> fPropertyItemCache;

		#endregion

		#region Security Items

		#region HasModifyDetailsCodeSecurity

		public bool HasModifyDetailsCodeSecurity
		{
			get { return Env.Security.OrgDetailsModifyCode.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsSecurity

		public bool HasModifyDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModify.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsRelatedPartiesSecurity

		public bool HasModifyDetailsRelatedPartiesSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgDetailsModifyRelatedParties.IsAllowed : Env.Security.OrgDetailsNewModifyRelatedParties.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsFinancialRelatedPartiesSecurity

		public bool HasModifyDetailsFinancialRelatedPartiesSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsNonFinancialRelatedPartiesSecurity

		public bool HasModifyDetailsNonFinancialRelatedPartiesSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity

		public bool HasModifyDetailsControllingAgentRelatedPartyToAnyOrgSecurity
		{
			get { return !OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value || (header.IsInDatabase ? Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed : Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToAnyOrg.IsAllowed); }
		}

		#endregion

		#region HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity

		public bool HasModifyDetailsControllingAgentRelatedPartyToOwnOrgSecurity
		{
			get { return !OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value || (header.IsInDatabase ? Env.Security.OrgDetailsModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed : Env.Security.OrgDetailsNewModifyCtrlAgentRelatedPartyToOwnOrg.IsAllowed); }
		}

		#endregion

		#region HasModifyDetailsIsActiveOrgSecurity

		public bool HasModifyDetailsIsActiveOrgSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsIsGlobalOrgSecurity

		public bool HasModifyDetailsIsGlobalOrgSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyIsGlobalOrg.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsIsNationalOrgSecurity

		public bool HasModifyDetailsIsNationalOrgSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyIsNationalOrg.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsIsTemporaryOrgSecurity

		public bool HasModifyDetailsIsTemporaryOrgSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyIsTemporaryOrg.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsCategorySecurity

		public bool HasModifyDetailsCategorySecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgDetailsModifyCategory.IsAllowed : Env.Security.OrgDetailsNewModifyCategory.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsNameAndAddressSecurity

		public bool HasModifyDetailsNameAndAddressSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsAddressShortCodeSecurity

		public bool HasModifyDetailsAddressShortCodeSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyAddressShortCode.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsPhFaxWebDetailsSecurity

		public bool HasModifyDetailsPhFaxWebDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyPhFaxWebDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagAR

		public bool HasModifyDetailsOrgTypeFlagAR
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagAP

		public bool HasModifyDetailsOrgTypeFlagAP
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempARFlag

		public bool HasModifyDetailsOrgTypeTempARFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempAPFlag

		public bool HasModifyDetailsOrgTypeTempAPFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagSP

		public bool HasModifyDetailsOrgTypeFlagSP
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagSP.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempSPFlag

		public bool HasModifyDetailsOrgTypeTempSPFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempSPFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagCon

		public bool HasModifyDetailsOrgTypeFlagCon
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagCon.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempConFlag

		public bool HasModifyDetailsOrgTypeTempConFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempConFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagTC

		public bool HasModifyDetailsOrgTypeFlagTC
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagTC.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempTCFlag

		public bool HasModifyDetailsOrgTypeTempTCFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTemlTCFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagWH

		public bool HasModifyDetailsOrgTypeFlagWH
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagWH.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempWHFlag

		public bool HasModifyDetailsOrgTypeTempWHFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempWHFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagCrr

		public bool HasModifyDetailsOrgTypeFlagCrr
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagCrr.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagFA

		public bool HasModifyDetailsOrgTypeFlagFA
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagFA.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempFAFlag

		public bool HasModifyDetailsOrgTypeTempFAFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempFAFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagBR

		public bool HasModifyDetailsOrgTypeFlagBR
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagBR.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempBRFlag

		public bool HasModifyDetailsOrgTypeTempBRFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempBRFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagSV

		public bool HasModifyDetailsOrgTypeFlagSV
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagSV.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempSVFlag

		public bool HasModifyDetailsOrgTypeTempSVFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempSVFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagCM

		public bool HasModifyDetailsOrgTypeFlagCM
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagCM.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempCMFlag

		public bool HasModifyDetailsOrgTypeTempCMFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempCMFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagSal

		public bool HasModifyDetailsOrgTypeFlagSal
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagSal.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeTempSalFlag

		public bool HasModifyDetailsOrgTypeTempSalFlag
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeTempSalFlag.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagCtrlAgent

		public bool HasModifyDetailsOrgTypeFlagCtrlAgent
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagCtrlAgent.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagCtrlCustomer

		public bool HasModifyDetailsOrgTypeFlagCtrlCustomer
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomer.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent

		public bool HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomerWithoutAgt.IsAllowed; }
		}

		#endregion

		#region HasModifyCustomFieldsSecurity

		public bool HasModifyDetailsCustomFieldsSecurity => Env.Security.OrgDetailsModifyCustomFields.IsAllowed;

		#endregion

		#region HasNewDetailsIsTemporaryOrgSecurity

		public bool HasNewDetailsIsNationalOrgSecurity
		{
			get { return Env.Security.OrgDetailsNewIsNationalOrg.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsIsGlobalOrgSecurity

		public bool HasNewDetailsIsGlobalOrgSecurity
		{
			get { return Env.Security.OrgDetailsNewIsGlobalOrg.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsIsTemporaryOrgSecurity

		public bool HasNewDetailsIsTemporaryOrgSecurity
		{
			get { return Env.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagAR

		public bool HasNewDetailsOrgTypeFlagAR
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagAP

		public bool HasNewDetailsOrgTypeFlagAP
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempARFlag

		public bool HasNewDetailsOrgTypeTempARFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempAPFlag

		public bool HasNewDetailsOrgTypeTempAPFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagSP

		public bool HasNewDetailsOrgTypeFlagSP
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagSP.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempSPFlag

		public bool HasNewDetailsOrgTypeTempSPFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempSPFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagCon

		public bool HasNewDetailsOrgTypeFlagCon
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagCon.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempConFlag

		public bool HasNewDetailsOrgTypeTempConFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempConFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagTC

		public bool HasNewDetailsOrgTypeFlagTC
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagTC.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempTCFlag

		public bool HasNewDetailsOrgTypeTempTCFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempTCFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagWH

		public bool HasNewDetailsOrgTypeFlagWH
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagWH.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempWHFlag

		public bool HasNewDetailsOrgTypeTempWHFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempWHFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagCrr

		public bool HasNewDetailsOrgTypeFlagCrr
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagCrr.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagFA

		public bool HasNewDetailsOrgTypeFlagFA
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagFA.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempFAFlag

		public bool HasNewDetailsOrgTypeTempFAFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempFAFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagBR

		public bool HasNewDetailsOrgTypeFlagBR
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagBR.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempBRFlag

		public bool HasNewDetailsOrgTypeTempBRFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempBRFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagSV

		public bool HasNewDetailsOrgTypeFlagSV
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagSV.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempSVFlag

		public bool HasNewDetailsOrgTypeTempSVFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempSVFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagCM

		public bool HasNewDetailsOrgTypeFlagCM
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagCM.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempCMFlag

		public bool HasNewDetailsOrgTypeTempCMFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempCMFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagSal

		public bool HasNewDetailsOrgTypeFlagSal
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagSal.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeTempSalFlag

		public bool HasNewDetailsOrgTypeTempSalFlag
		{
			get { return Env.Security.OrgDetailsNewOrgTypeTempSalFlag.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagCtrlAgent

		public bool HasNewDetailsOrgTypeFlagCtrlAgent
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagCtrlAgent.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagCtrlCustomer

		public bool HasNewDetailsOrgTypeFlagCtrlCustomer
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagCtrlCustomer.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent

		public bool HasNewDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent
		{
			get { return Env.Security.OrgDetailsNewOrgTypeFlagCtrlCustomerWithoutAgt.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsStaffAssignmentsSecurity

		public bool HasModifyDetailsStaffAssignmentsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOtherCompanysStaffAssignmentsSecurity

		public bool HasModifyDetailsOtherCompanysStaffAssignmentsSecurity
		{
			get { return Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsWebSecurity

		public bool HasModifyDetailsWebSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyWebSecurity.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsWebSecurity

		public bool HasNewDetailsWebSecurity
		{
			get { return header.IsInDatabase || Env.Security.OrgDetailsNewWebSecurity.IsAllowed; }
		}

		#endregion

		#region HasNewDetailsAllowCreationOutsideLoginCountry

		public bool HasNewDetailsAllowCreationOutsideLoginCountry
		{
			get { return Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsRatingAndTariffsSecurity

		public bool HasModifyDetailsRatingAndTariffsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed; }
		}

		#endregion

		#region HasModifyDetailsOrganisationTypeSecurity

		public bool HasModifyDetailsOrganisationTypeSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgDetailsModifyOrganisationType.IsAllowed : Env.Security.OrgDetailsNewOrganisationType.IsAllowed; }
		}

		#endregion

		#region HasModifyAddressSecurity

		public bool HasModifyAddressSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressModify.IsAllowed : Env.Security.OrgAddressNew.IsAllowed; }
		}

		#endregion

		#region HasModifyAddressListSecurity

		public bool HasModifyAddressListSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressListModify.IsAllowed : Env.Security.OrgAddressListNew.IsAllowed; }
		}

		#endregion

		#region HasModifyAddressDetailsSecurity

		public bool HasModifyAddressDetailsSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressDetailsModify.IsAllowed : Env.Security.OrgAddressDetailsNew.IsAllowed; }
		}

		#endregion

		#region HasModifyAddressShortCodeSecurity

		public bool HasModifyAddressShortCodeSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressShortCodeModify.IsAllowed : Env.Security.OrgAddressShortCodeNew.IsAllowed; }
		}

		#endregion

		#region ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities

		public SecurityCheckpoint[] ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities(OrgAddress address)
		{
			var blockedSecurities = new List<SecurityCheckpoint>();

			if (address.AddressCapability.EnabledCapabilities.All(c =>
				c.Code != OrgConstants.AddressType.Payables && c.Code != OrgConstants.AddressType.Receivables))
			{
				if (header.IsInDatabase && !Env.Security.OrgAddressDetailsNonARAP.IsAllowed)
				{
					blockedSecurities.Add(Env.Security.OrgAddressDetailsNonARAP);
				}
				else if (!header.IsInDatabase && !Env.Security.OrgAddressDetailsNonARAPNew.IsAllowed)
				{
					blockedSecurities.Add(Env.Security.OrgAddressDetailsNonARAPNew);
				}
			}
			else if (address.AddressCapability.EnabledCapabilities.All(c =>
				c.Code == OrgConstants.AddressType.Payables || c.Code == OrgConstants.AddressType.Receivables))
			{
				if (header.IsInDatabase && !Env.Security.OrgAddressDetailsARAP.IsAllowed)
				{
					blockedSecurities.Add(Env.Security.OrgAddressDetailsARAP);
				}
				else if (!header.IsInDatabase && !Env.Security.OrgAddressDetailsARAPNew.IsAllowed)
				{
					blockedSecurities.Add(Env.Security.OrgAddressDetailsARAPNew);
				}
			}
			else
			{
				if (header.IsInDatabase)
				{
					if (!Env.Security.OrgAddressDetailsNonARAP.IsAllowed)
					{
						blockedSecurities.Add(Env.Security.OrgAddressDetailsNonARAP);
					}

					if (!Env.Security.OrgAddressDetailsARAP.IsAllowed)
					{
						blockedSecurities.Add(Env.Security.OrgAddressDetailsARAP);
					}
				}
				else
				{
					if (!Env.Security.OrgAddressDetailsNonARAPNew.IsAllowed)
					{
						blockedSecurities.Add(Env.Security.OrgAddressDetailsNonARAPNew);
					}

					if (!Env.Security.OrgAddressDetailsARAPNew.IsAllowed)
					{
						blockedSecurities.Add(Env.Security.OrgAddressDetailsARAPNew);
					}
				}
			}

			return blockedSecurities.ToArray();
		}

		#endregion

		#region HasModifyAddressCapabilitiesSecurity

		public bool HasModifyAddressCapabilitiesSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressCapabilitiesModify.IsAllowed : Env.Security.OrgAddressCapabilitiesNew.IsAllowed; }
		}

		#endregion

		#region HasModifyAddressAdditionalDetailsSecurity

		public bool HasModifyAddressAdditionalDetailsSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressAdditionalDetailsModify.IsAllowed : Env.Security.OrgAddressAdditionalDetailsNew.IsAllowed; }
		}

		#endregion

		#region HasModifyCustomsAddressSecurity

		public bool HasModifyCustomsAddressSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressCustomsAddressModify.IsAllowed : Env.Security.OrgAddressCustomsAddressNew.IsAllowed; }
		}

		#endregion

		#region HasModifyEUCustomsAddressSecurity

		public bool HasModifyEUCustomsAddressSecurity
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed : Env.Security.OrgAddressEUCustomsAddressNew.IsAllowed; }
		}

		#endregion

		#region HasModifyContactSecurity

		public bool HasModifyContactSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgContactModify.IsAllowed; }
		}

		#endregion

		#region HasModifyContactContactDetailsSecurity

		public bool HasModifyContactContactDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgContactModifyContactDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyContactPersonalInformationSecurity

		public bool HasModifyContactPersonalInformationSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgContactModifyPersonalInformation.IsAllowed; }
		}

		#endregion

		#region HasModifyContactDocDeliveryDetailsSecurity

		public bool HasModifyContactDocDeliveryDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgContactModifyDocDeliveryDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyContactMobileNumberSecurity

		public bool HasModifyContactMobileNumberSecurity => !header.IsInDatabase || Env.Security.OrgContactModifyMobileNumber.IsAllowed;

		#endregion

		#region HasModifyContactHomePhoneNumberSecurity

		public bool HasModifyContactHomePhoneNumberSecurity => !header.IsInDatabase || Env.Security.OrgContactModifyHomePhoneNumber.IsAllowed;

		#endregion

		#region HasModifyReceivablesSecurity

		public bool HasModifyReceivablesSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModify.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesConfigSecurity

		public bool HasModifyReceivablesConfigSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyConfig.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesAccountDetailsSecurity

		public bool HasModifyReceivablesAccountDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesInvoicingSecurity

		public bool HasModifyReceivablesInvoicingSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyInvoicing.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesCreditControlSecurity

		public bool HasModifyReceivablesCreditControlSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyCreditControl.IsAllowed; }
		}

		#endregion

		#region HasReceivablesGlobalCreditControlSecurity

		public bool HasReceivablesGlobalCreditControlSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesCurrencyUpliftSecurity

		public bool HasModifyReceivablesCurrencyUpliftSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyCurrencyUplift.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesTaxDetailsSecurity

		public bool HasModifyReceivablesTaxDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyTaxDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesQualityAssuranceSecurity

		public bool HasModifyReceivablesQualityAssuranceSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyQualityAssurance.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesExternalDebtorSecurity

		public bool HasModifyReceivablesExternalDebtorSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyExternalDebtor.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesSettlementGroupSecurity

		public bool HasModifyReceivablesSettlementGroupSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifySettlementGroup.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesPaymentTermsSecurity

		public bool HasModifyReceivablesPaymentTermsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesInvoiceDetailsSecurity

		public bool HasModifyReceivablesInvoiceDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyInvoiceDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesChargeGroupingSecurity

		public bool HasModifyReceivablesChargeGroupingSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesInvoiceBatchingSecurity

		public bool HasModifyReceivablesInvoiceBatchingSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesExchangeRatesSecurity

		public bool HasModifyReceivablesExchangeRatesSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyExchangeRates.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesBuyersConsolInvoicingSecurity

		public bool HasModifyReceivablesBuyersConsolInvoicingSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyBuyersConsolInvoicing.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesCreditCardDetails

		public bool HasModifyReceivablesCreditCardDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgReceivablesModifyCreditCardDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyReceivablesTaxConfigurationSecurity

		public bool HasModifyReceivablesTaxConfigurationTemplateSecurity
		{
			get { return Env.Security.OrgReceivablesModifyTaxConfigurationTemplate.IsAllowed; }
		}

		public bool HasModifyReceivablesTaxConfigurationGridSecurity
		{
			get { return Env.Security.OrgReceivablesModifyTaxConfigurationGrid.IsAllowed; }
		}

		public bool HasModifyReceivablesTaxConfigurationRatesSecurity
		{
			get { return Env.Security.OrgReceivablesModifyTaxConfigurationRates.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesSecurity

		public bool HasModifyPayablesSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesModify.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigPayablesSecurity

		public bool HasModifyConfigPayablesSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesModifyConfig.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesCreditorDetailsSecurity

		public bool HasModifyPayablesCreditorDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesCreditorDetailsModify.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesDefaultsSecurity

		public bool HasModifyPayablesDefaultsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesDefaultsModify.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesAccountDetailsSecurity

		public bool HasModifyPayablesAccountDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesAccountDetailsModify.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesAccountDetailsEPaymentSecurity

		public bool HasModifyPayablesAccountDetailsEPaymentSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesAccountDetailsEPaymentModify.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesCreditDetailsSecurity

		public bool HasModifyPayablesCreditDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesCreditDetailsModify.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesPaymentTermsSecurity

		public bool HasModifyPayablesPaymentTermsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesPaymentTermsModify.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesTaxDetailsSecurity

		public bool HasModifyPayablesTaxDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesTaxDetailsModify.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesOtherDetailsSecurity

		public bool HasModifyPayablesOtherDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesOtherDetailsModify.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesQualityAssuranceSecurity

		public bool HasModifyPayablesQualityAssuranceSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesQualityAssuranceModify.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesExternalCreditorSecurity

		public bool HasModifyPayablesExternalCreditorSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgPayablesModifyExternalCreditor.IsAllowed; }
		}

		#endregion

		#region HasModifyPayablesTaxConfigurationSecurity

		public bool HasModifyPayablesTaxConfigurationTemplateSecurity
		{
			get { return Env.Security.OrgPayablesModifyTaxConfigurationTemplate.IsAllowed; }
		}

		public bool HasModifyPayablesTaxConfigurationGridSecurity
		{
			get { return Env.Security.OrgPayablesModifyTaxConfigurationGrid.IsAllowed; }
		}

		public bool HasModifyPayablesTaxConfigurationRatesSecurity
		{
			get { return Env.Security.OrgPayablesModifyTaxConfigurationRates.IsAllowed; }
		}

		#endregion

		#region HasOrgPayablesModifyConfigTransCreationRestrictionSecurity

		public bool HasOrgPayablesModifyConfigTransCreationRestrictionSecurity
		{
			get { return Env.Security.OrgPayablesModifyConfigTransCreationRestriction.IsAllowed; }
		}

		#endregion

		#region HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity

		public bool HasOrgReceivablesModifyConfigTransCreationRestrictionSecurity
		{
			get { return Env.Security.OrgReceivablesModifyConfigTransCreationRestriction.IsAllowed; }
		}

		#endregion

		#region HasOrgReceivablesModifySurchargeConfigurationSecurity

		public bool HasOrgReceivablesModifySurchargeConfigurationSecurity
		{
			get
			{
				return Env.Security.OrgReceivablesModifySurchargeConfiguration.IsAllowedWithConstraint();
			}
		}

		#endregion

		#region HasModifyConsignorSecurity

		public bool HasModifyConsignorSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConsignorModify.IsAllowed; }
		}

		#endregion

		#region HasModifyConsignorDetailsSecurity

		public bool HasModifyConsignorDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConsignorModifyDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyConsignorRelationshipsSecurity

		public bool HasModifyConsignorRelationshipsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConsignorModifyRelationships.IsAllowed; }
		}

		#endregion

		#region HasModifyConsignorExporterSchemeSecurity

		public bool HasModifyConsignorExporterSchemeSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConsignorModifyExporterScheme.IsAllowed; }
		}

		#endregion

		#region HasModifyConsigneeSecurity

		public bool HasModifyConsigneeSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConsigneeModify.IsAllowed; }
		}

		#endregion

		#region HasModifyConsigneeDetailsSecurity

		public bool HasModifyConsigneeDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConsigneeModifyDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyConsigneeRelationshipsSecurity

		public bool HasModifyConsigneeRelationshipsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConsigneeModifyRelationships.IsAllowed; }
		}

		#endregion

		#region HasModifyConsigneeLandedCostingSecurity

		public bool HasModifyConsigneeLandedCostingSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed; }
		}

		#endregion

		#region HasModifyConsolidationCategory

		public bool HasModifyConsolidationCategory
		{
			get { return !header.IsInDatabase || Env.Security.OrgModifyConsolidationCategory.IsAllowed; }
		}

		#endregion

		#region HasModifyGlobalSupplierDetails

		public bool HasModifyGlobalSupplierDetails
		{
			get { return !header.IsInDatabase || Env.Security.OrgGlobalSupplierDetailsModify.IsAllowed; }
		}

		#endregion

		#region CanEditGlobalSupplier

		public bool CanEditGlobalSupplier
		{
			get { return !header.OH_IsGlobalAccount || HasModifyGlobalSupplierDetails; }
		}

		#endregion

		#region HasModifyWarehouseSecurity

		public bool HasModifyWarehouseSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgWarehouseModify.IsAllowed; }
		}

		#endregion

		#region HasModifyForwarderSecurity

		public bool HasModifyForwarderSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgForwarderModify.IsAllowed; }
		}

		#endregion

		#region HasModifyForwarderDetailsSecurity

		public bool HasModifyForwarderDetailsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgForwarderModifyDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyForwarderProfitShareSecurity

		public bool HasModifyForwarderProfitShareSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgForwarderModifyProfitShare.IsAllowed; }
		}

		#endregion

		#region HasModifyCarrierSecurity

		public bool HasModifyCarrierSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgCarrierModify.IsAllowed; }
		}

		#endregion

		#region HasModifyCarrierSecurityAir

		public bool HasModifyCarrierSecurityAir
		{
			get { return !header.IsInDatabase || Env.Security.OrgCarrierModifyAir.IsAllowed; }
		}

		#endregion

		#region HasModifyCarrierSecuritySea

		public bool HasModifyCarrierSecuritySea
		{
			get { return !header.IsInDatabase || Env.Security.OrgCarrierModifySea.IsAllowed; }
		}

		#endregion

		#region HasModifyCarrierSecurityLand

		public bool HasModifyCarrierSecurityLand
		{
			get { return !header.IsInDatabase || Env.Security.OrgCarrierModifyLand.IsAllowed; }
		}

		#endregion

		#region HasModifyServicesSecurity

		public bool HasModifyServicesSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgServicesModify.IsAllowed; }
		}

		#endregion

		#region HasModifySalesSecurity

		public bool HasModifySalesSecurity
		{
			get { return !header.IsInDatabase || Env.Security.ClientIntelligenceModify.IsAllowed; }
		}

		#endregion

		#region HasModifySalesClientSummarySecurity

		public bool HasModifySalesClientSummarySecurity
		{
			get { return !header.IsInDatabase || Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed; }
		}

		#endregion

		#region HasModifySalesOpportunityManagementSecurity

		public bool HasModifySalesOpportunityManagementSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OpportunityManagementEdit.IsAllowed; }
		}

		#endregion

		#region HasModifySalesTradeProfileSecurity

		public bool HasModifySalesTradeProfileSecurity
		{
			get { return !header.IsInDatabase || Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed; }
		}

		#endregion

		#region HasModifySalesClientRelationshipSecurity

		public bool HasModifySalesClientRelationshipSecurity
		{
			get { return !header.IsInDatabase || Env.Security.ClientIntelligenceModifyClientRelationship.IsAllowed; }
		}

		#endregion

		#region HasModifyOrgServiceLevelsSecurity

		public bool HasModifyOrgServiceLevelsSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConsignorModifyDetails.IsAllowed; }
		}

		#endregion

		#region HasModifyCompetitorSecurity

		public bool HasModifyCompetitorSecurity
		{
			get { return !header.IsInDatabase || Env.Security.CompetitorIntelligenceModify.IsAllowed; }
		}

		#endregion

		#region HasModifyCustomSecurity

		public bool HasModifyCustomSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgCustomModify.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigSecurity

		public bool HasModifyConfigSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModify.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigRegistrationNumbersSecurity

		public bool HasModifyConfigRegistrationNumbersSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyRegistrationNumbers.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigFinancialRegistrationNumbersSecurity

		public bool HasModifyConfigFinancialRegistrationNumbersSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyFinancialRegistrationNumbers.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigFinancialARAPRegistrationNumbersSecurity

		public bool HasModifyConfigFinancialARAPRegistrationNumbersSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity

		public bool HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigNonFinancialRegistrationNumbersSecurity

		public bool HasModifyConfigNonFinancialRegistrationNumbersSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyNonFinancialRegistrationNumbers.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity

		public bool HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity

		public bool HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigEDICodeMappingSecurity

		public bool HasModifyConfigEDICodeMappingSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigBrandsAndCompanyNamesSecurity

		public bool HasModifyConfigBrandsAndCompanyNamesSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyBrandsAndCompanyNames.IsAllowed; }
		}

		#endregion

		#region HasModifyConfigGeneralSecurity

		public bool HasModifyConfigGeneralSecurity
		{
			get { return !header.IsInDatabase || Env.Security.OrgConfigModifyGeneral.IsAllowed; }
		}

		#endregion

		#region HasConfigNewSecurity

		public bool HasConfigNewSecurity
		{
			get { return Env.Security.OrgConfigNew.IsAllowed; }
		}

		#endregion

		#region HasNewConfigRegistrationNumbersSecurity

		public bool HasNewConfigRegistrationNumbersSecurity
		{
			get { return Env.Security.OrgConfigNewModifyRegistrationNumbers.IsAllowed; }
		}

		#endregion

		#region HasNewConfigModifyFinancialRegistrationNosSecurity

		public bool HasNewConfigModifyFinancialRegistrationNosSecurity
		{
			get { return Env.Security.OrgConfigNewModifyFinancialRegistrationNos.IsAllowed; }
		}

		#endregion

		#region HasNewConfigModifyNonFinancialRegistrationNosSecurity

		public bool HasNewConfigModifyNonFinancialRegistrationNosSecurity
		{
			get { return Env.Security.OrgConfigNewModifyNonFinancialRegistrationNos.IsAllowed; }
		}

		#endregion

		#region HasModifyAddressCapabilitiesARAP

		public bool HasModifyAddressCapabilitiesARAP
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressCapabilitiesARAP.IsAllowed : Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed; }
		}

		#endregion

		#region HasModifyAddressCapabilitiesNonARAP

		public bool HasModifyAddressCapabilitiesNonARAP
		{
			get { return header.IsInDatabase ? Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed : Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed; }
		}

		#endregion

		#region HasModifyBranchProxies

		public bool HasModifyBranchProxies
		{
			get
			{
				return !header.IsInDatabase || SpecialProxyCondition || !header.IsProxyOrgOfAnyBranchOnly(false) || Env.Security.OrgBranchProxiesEdit.IsAllowed;
			}
		}

		#endregion

		#region HasModifyCompanyProxies

		public bool HasModifyCompanyProxies
		{
			get
			{
				return !header.IsInDatabase || SpecialProxyCondition || !header.IsProxyOrgOfAnyCompanyOnly(false) || Env.Security.OrgCompanyProxiesEdit.IsAllowed;
			}
		}

		bool SpecialProxyCondition => Db.Connection.DatabaseUpgradedExceptionHasBeenThrown || !header.Factory.IsOwnedByCurrentThread;

		#endregion

		#region CRM security

		public bool CanModifyDetailsStaffAssignment(string role)
		{
			SecurityCheckpoint checkpoint = null;
			return Env.Security.OrgDetailsModifyStaffAssignmentsLookup.TryGetValue(role, out checkpoint) ?
				checkpoint.IsAllowed : Env.Security.OrgDetailsModifyStaffAssignments.IsAllowed;
		}

		public bool HasModifyDetailsStaffAssignmentsAnyRoleSecurity
		{
			get { return Env.Security.OrgDetailsModifyStaffAssignmentsLookup.Any(x => x.Value.IsAllowed); }
		}

		#endregion

		#endregion
	}
}
