using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public enum OrganizationFieldType { Header, Address }

	public abstract class ACEBIRDCommonPGADataProcessor
	{
		protected ACEBIRDCommonPGADataProcessor(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}
		protected readonly JobComInvoiceLine invoiceLine;

		public abstract ZPropertyInfo IndicatorInfo { get; }
		public abstract ZPropertyInfo DisclaimReasonInfo { get; }
		public abstract ZString PGAName { get; }

		public void DoImport(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var isDisclaimed = !pg01.Disclaimer.IsEmpty;
			if (isDisclaimed)
			{
				if (DisclaimReasonInfo != null)
				{
					IndicatorInfo.Value = (ZString)OGAIndicatorList.Codes.Disclaimed;
					DisclaimReasonInfo.Value = pg01.Disclaimer;
					ProcessDisclaimedPGABlocks(pg01, pgaBlocks, notifications);
				}
			}
			else
			{
				IndicatorInfo.Value = (ZString)OGAIndicatorList.Codes.Declared;
				ProcessDeclaredPGABlocks(pg01, pgaBlocks, notifications);
			}
		}

		protected abstract void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications);

		protected virtual void ProcessDisclaimedPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
		}

		protected void SetBrokerDetails(ZString contactName, ZString contactPhone, ZString contactEmal, INotifications notifications)
		{
			invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.US_FDAContactName, contactName, contactName, "Broker PGA Contact Name", notifications);
			invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.US_FDAContactPhoneNo, contactPhone, contactPhone, "Broker PGA Contact Phone", notifications);
			invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.US_FDAContactEmail, contactEmal, contactEmal, "Broker PGA Contact Email", notifications);
		}

		protected ZGuid FindMatchedOrgAddressPK(ZString organizationCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			return BIRDOrganisationMatching.FindMatchedOrgAddress(invoiceLine.Factory, organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, PGAName, notifications)?.PK ?? ZGuid.Empty;
		}

		protected void SetOrganizationOrAddress(OrganizationFieldType fieldType, ZString organizationCode, ZPropertyInfo orgPropertyInfo, ZGuid orgAddressPK, ZBool overrideExistingValue, INotifications notifications)
		{
			if (!orgAddressPK.IsEmpty)
			{
				var currentGuid = (ZGuid)orgPropertyInfo.Value;
				var matchedGuid = orgAddressPK;
				if (fieldType == OrganizationFieldType.Header)
				{
					var orgAddress = invoiceLine.Factory.Load<OrgAddress>(orgAddressPK);
					matchedGuid = orgAddress.Header.PK;
				}

				if (currentGuid.IsEmpty || overrideExistingValue)
				{
					orgPropertyInfo.Value = matchedGuid;
				}
				else if (currentGuid != matchedGuid)
				{
					notifications.AddWarning(organizationCode + OrganizationHasDifferentCompanyNameOrAddress);
				}
			}
		}
		internal const string OrganizationHasDifferentCompanyNameOrAddress = ": The matched organization has different company name or address details with current organization, system will retain current organization info.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void ProcessOrganizations(BusinessObject pga, Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>> organizationsDict, INotifications notifications)
		{
			foreach (var organization in organizationsDict)
			{
				var address1 = organization.Value != null && organization.Value.Item1 != null ? organization.Value.Item1.Address1 : ZString.Empty;
				var address2 = organization.Value != null && organization.Value.Item2 != null ? organization.Value.Item2.Address2 : ZString.Empty;
				var countryCode = organization.Value != null && organization.Value.Item3 != null ? organization.Value.Item3.Country : ZString.Empty;
				var city = organization.Value != null && organization.Value.Item3 != null ? organization.Value.Item3.City : ZString.Empty;
				var postCode = organization.Value != null && organization.Value.Item3 != null ? organization.Value.Item3.ZipCode : ZString.Empty;
				SetOrganizationOrAddressDetails(pga, organization.Key.OrganizationType, GetMatchedCustomsNoTypeInOrganization(organization.Key.CustomsNoType), organization.Key.CustomsNumber, organization.Key.CompanyName, address1, address2, countryCode, city, postCode, notifications);
			}
		}

		protected virtual ZString GetMatchedCustomsNoTypeInOrganization(ZString customsNoTypeInMessage)
		{
			return customsNoTypeInMessage;
		}

		protected virtual void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
		}
	}
}
