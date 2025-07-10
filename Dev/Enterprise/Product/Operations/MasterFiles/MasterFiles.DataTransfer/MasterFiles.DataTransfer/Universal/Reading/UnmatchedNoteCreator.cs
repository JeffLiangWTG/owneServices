using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	class UnmatchedNoteCreator
	{
		internal UnmatchedNoteCreator(BusinessObjectFactory factory, OrganizationAddressFormatted dataObject)
		{
			this.factory = Argument.NotNull(factory, "BusinessObjectFactory factory");
			this.dataObject = Argument.NotNull(dataObject, "OrganizationAddressFormatted dataObject");
		}

		readonly BusinessObjectFactory factory;
		readonly OrganizationAddressFormatted dataObject;

		internal void AddUnmatchedNote(BusinessObject sourceObject, OrganisationTypes orgCategory, string orgType = null)
		{
			var unmatchOrg = ConvertOrgUnmatchDetail(orgCategory, orgType);
			new UnmatchNoteCreator(factory).Create(EntityInfo.New(sourceObject), unmatchOrg);
		}

		UnmatchOrgRecord ConvertOrgUnmatchDetail(OrganisationTypes orgCategory, string orgType)
		{
			var unmatchOrg = new UnmatchOrgRecord();
			unmatchOrg.OrganisationType = orgCategory.ToString();
			unmatchOrg.EDICode = dataObject.OrganizationCode.GetValueOrDefault();
			unmatchOrg.OrganisationName = dataObject.CompanyName.GetValueOrDefault();
			unmatchOrg.OrganisationSubType = string.IsNullOrEmpty(orgType) ? orgCategory.ToString() : orgType;
			unmatchOrg.AddressLine1 = dataObject.Address1.GetValueOrDefault();
			unmatchOrg.AddressLine2 = dataObject.Address2.GetValueOrDefault();
			unmatchOrg.PostCode = dataObject.Postcode.GetValueOrDefault();
			unmatchOrg.City = dataObject.City.GetValueOrDefault();
			unmatchOrg.StateOrProvince = dataObject.State.GetValueOrDefault();
			unmatchOrg.Country = dataObject.Country.GetCodeAsUpperCase();

			return unmatchOrg;
		}
	}
}
