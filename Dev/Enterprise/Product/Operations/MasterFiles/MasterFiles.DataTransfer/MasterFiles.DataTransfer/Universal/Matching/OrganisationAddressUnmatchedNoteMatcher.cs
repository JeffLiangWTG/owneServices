using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Matching
{
	public class OrganisationAddressUnmatchedNoteMatcher : IOrganisationAddressUnmatchedNoteMatcher
	{
		public bool IsMatchToOrgInUnmatchedNote(OrganizationAddress organisationAddress, BusinessObject matchingBO, OrganisationTypes unmatchedOrgNoteType, string unmatchedOrgNoteSubType)
		{
			var noteParent = matchingBO as IStmNoteParent
				?? throw new InvalidOperationException(string.Format("You cannot specify an 'Unmatched Note' fallback match where it's not possible to have notes. ({0} does not implement IStmNoteParent)", matchingBO.GetType().FullName));

			var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationType = unmatchedOrgNoteType };
			unmatchOrgRecordCriteria.OrganisationSubType = string.IsNullOrEmpty(unmatchedOrgNoteSubType) ? unmatchedOrgNoteType.ToString() : unmatchedOrgNoteSubType;
			var unmatchedOrgs = new UnmatchOrgRecords(noteParent);
			var unmatchedOrg = unmatchedOrgs.FindUnmatchOrg(unmatchOrgRecordCriteria);

			return unmatchedOrg != null
				&& organisationAddress.CompanyName.GetValueOrDefault() == unmatchedOrg.OrganisationName
				&& organisationAddress.Address1.GetValueOrDefault() == unmatchedOrg.AddressLine1
				&& organisationAddress.Address2.GetValueOrDefault() == unmatchedOrg.AddressLine2
				&& organisationAddress.City.GetValueOrDefault() == unmatchedOrg.City
				&& ((ZString?)organisationAddress.State).GetValueOrDefault() == unmatchedOrg.StateOrProvince
				&& organisationAddress.Postcode.GetValueOrDefault() == unmatchedOrg.PostCode
				&& organisationAddress.Country.GetCodeAsUpperCase() == unmatchedOrg.Country.ToUpper();
		}
	}
}
