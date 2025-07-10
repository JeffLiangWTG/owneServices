using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.InBond.Business
{
	public class BillJobDocAddressRequirement
	{
		public JobDocAddressRequirement GetJobDocAddressRequirement(DocAddressType addressType)
		{
			JobDocAddressRequirement result;
			if (!PartiesJobDocAddressRequirements.TryGetValue(addressType, out result))
			{
				result = new JobDocAddressRequirement(addressType);
				result.DefaultMax = 1;
				AddDocAddressValidation(result, addressType);
				PartiesJobDocAddressRequirements.Add(addressType, result);
			}
			return result;
		}

		Dictionary<DocAddressType, JobDocAddressRequirement> PartiesJobDocAddressRequirements
		{
			get { return fPartiesJobDocAddressRequirements ?? (fPartiesJobDocAddressRequirements = new Dictionary<DocAddressType, JobDocAddressRequirement>()); }
		}
		Dictionary<DocAddressType, JobDocAddressRequirement> fPartiesJobDocAddressRequirements;

		void AddDocAddressValidation(JobDocAddressRequirement requirement, DocAddressType addressType)
		{
			requirement.ValidateCompanyName = DocAddressValidateCompanyName;
			requirement.ValidateAddress1 = DocAddressValidateAddress1;
			if (addressType == DocAddressType.ForeignShipperDocumentaryAddress || addressType == DocAddressType.ConsigneeAddress)
			{
				requirement.ValidateOrganisationPK = DocAddressValidateOrganisationPK;
			}
		}

		void DocAddressValidateOrganisationPK(JobDocAddressValidation validation)
		{
			JobDocAddress parent = validation.Parent;
			CusInBondBill bill = (CusInBondBill)parent.Parent;
			if (bill != null && bill.IsDetailedInBond && !bill.IsAir && !parent.E2_AddressOverride && !parent.HasRealOrganisation)
			{
				var caption = parent.DocAddressType == DocAddressType.ForeignShipperDocumentaryAddress && bill.Header != null && bill.Header.BH_FTZMove ? "Shipper Documentary Address"
					: parent.AddressCaption.ToString();
				parent.OrganisationPKInfo.AddMessageError(ValidationConstants.JobDocAddress.GetOrganisationPKRequired(caption));
			}
		}

		void DocAddressValidateCompanyName(JobDocAddressValidation validation)
		{
			JobDocAddress parent = validation.Parent;
			if (parent.E2_CompanyName.IsEmpty)
			{
				parent.E2_CompanyNameInfo.AddMessageError(ValidationConstants.JobDocAddress.CompanyNameRequired);
			}
		}

		void DocAddressValidateAddress1(JobDocAddressValidation validation)
		{
			JobDocAddress parent = validation.Parent;
			if (parent.E2_Address1.IsEmpty)
			{
				parent.E2_Address1Info.AddMessageError(ValidationConstants.JobDocAddress.AddressRequired);
			}
		}
	}
}
