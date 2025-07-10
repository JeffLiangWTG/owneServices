using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Matching;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Matching
{
	public class OrganizationAddressMatchToJobDocAddress : OrganizationAddressMatchBase, IOrganizationAddressMatchToJobDocAddress
	{
		/// <summary>
		/// Defines an Organisation Match where the target is a JobDocAddress in a collection.
		/// PLEASE NOTE: The BusinessObject you are matching to MUST implement IDocAddresses.
		/// </summary>
		/// <param name="organisationAddressType"></param>
		/// <param name="jobDocAddressType"></param>
		public OrganizationAddressMatchToJobDocAddress(MatchableOrganizationType organisationAddressType, DocAddressType jobDocAddressType, OrganizationAddressMatchPool matchPool, OrganisationTypes unmatchedOrgNoteType, string unmatchedOrgNoteSubType)
			: base(organisationAddressType, matchPool, unmatchedOrgNoteType, unmatchedOrgNoteSubType)
		{
			this.jobDocAddressType = jobDocAddressType;
		}
		readonly DocAddressType jobDocAddressType;

		protected override ZGuid[] GetTargetOrgHeaderPKsToMatchTo(BusinessObject matchingBO)
		{
			var jobDocAddress = GetJobDocAddress(matchingBO, jobDocAddressType);
			if (jobDocAddress != null && !jobDocAddress.E2_AddressOverride)
			{
				var orgAddress = jobDocAddress.Address;
				return orgAddress != null ? new[] { orgAddress.OA_OH } : null;
			}

			return null;
		}

		Tuple<BusinessObject, JobDocAddress> lastResult;

		JobDocAddress GetJobDocAddress(BusinessObject matchingBO, DocAddressType jobDocAddressType)
		{
			if (lastResult == null || lastResult.Item1 != matchingBO)
			{
				var jobDocAddressParent = matchingBO as IDocAddresses
					?? throw new InvalidOperationException(string.Format("Matching BO type [{0}] does not implement IDocAddresses.", matchingBO.GetType().FullName));

				lastResult = new Tuple<BusinessObject, JobDocAddress>(matchingBO, jobDocAddressParent.DocAddresses.FindByDocAddressType(jobDocAddressType));
			}

			return lastResult.Item2;
		}

		protected override bool IsFallbackMatch(OrganizationAddress organisationAddress, BusinessObject matchingBO)
		{
			var jobDocAddress = GetJobDocAddress(matchingBO, jobDocAddressType);
			var organisationAddressReaderObject = new OrganizationAddressFormatted(organisationAddress);

			if (jobDocAddress != null && jobDocAddress.E2_AddressOverride)
			{
				return organisationAddressReaderObject.CompanyName.GetValueOrDefault() == jobDocAddress.E2_CompanyName &&
					organisationAddressReaderObject.Address1.GetValueOrDefault() == jobDocAddress.E2_Address1 &&
					organisationAddressReaderObject.Address2.GetValueOrDefault() == jobDocAddress.E2_Address2 &&
					organisationAddressReaderObject.Postcode.GetValueOrDefault() == jobDocAddress.E2_Postcode &&
					organisationAddressReaderObject.City.GetValueOrDefault() == jobDocAddress.E2_City &&
					organisationAddressReaderObject.State.GetValueOrDefault() == jobDocAddress.E2_State &&
					organisationAddressReaderObject.Country.GetCodeAsUpperCase() == jobDocAddress.E2_RN_NKCountryCode.ToUpper() &&
					organisationAddressReaderObject.Phone.GetValueOrDefault() == jobDocAddress.E2_Phone &&
					organisationAddressReaderObject.Fax.GetValueOrDefault() == jobDocAddress.E2_Fax &&
					organisationAddressReaderObject.Email.GetValueOrDefault() == jobDocAddress.E2_Email &&
					organisationAddressReaderObject.Contact.GetValueOrDefault() == jobDocAddress.E2_Contact;
			}

			return base.IsFallbackMatch(organisationAddress, matchingBO);
		}
	}
}
