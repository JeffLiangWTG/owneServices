using System.Collections.Generic;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.AMS.Business
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

		void AddDocAddressValidation(JobDocAddressRequirement requirement, DocAddressType addressType)
		{
			if (addressType == DocAddressType.ImportBroker)
			{
				requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = DocAddressValidateOrganisationPKForCustomsBroker;
				requirement.GetRegistrationNumberResult = GetCustomsBrokerRegistrationNumber;
			}
			else
			{
				requirement.ValidateCompanyName = DocAddressValidateCompanyName;
				requirement.ValidateContact = DocAddressContact;
				requirement.ValidateAddress1 = DocAddressValidateAddress1;
				requirement.ValidateAddress2 = DocAddressValidateAddress2;

				if (addressType == DocAddressType.ForeignShipperDocumentaryAddress || addressType == DocAddressType.ConsigneeAddress)
				{
					requirement.ValidateOrganisationPK = DocAddressValidateOrganisationPK;
				}
			}
		}

		RegistrationNumberResult GetCustomsBrokerRegistrationNumber(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true,
				delegate
				{
					var result = new RegistrationNumber() { NumberType = OrgCusCode.USACodeTypes.ABIRoutingCode };
					if (docAddress != null && !docAddress.IsDeleted)
					{
						var address = docAddress.Address;

						if (address != null)
						{
							var cusCode = address.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(OrgCusCode.USACodeTypes.ABIRoutingCode, Core.Constants.CountryCodes.UnitedStates);
							if (cusCode != null)
							{
								result.Number = cusCode.OK_CustomsRegNo;
								result.NumberType = cusCode.OK_CodeType;
							}
						}
					}

					return result;
				});
		}

		void DocAddressValidateOrganisationPKForCustomsBroker(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var address = parent.Address;
			if (address != null)
			{
				var bill = (CusInBondBill)parent.Parent;
				if (bill != null && (bill.IsInventoryRecordValidationMode || bill.IsSubsequentInBondValidationMode))
				{
					if (address.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ABIRoutingCode, Core.Constants.CountryCodes.UnitedStates).IsEmpty)
					{
						parent.E2_OA_AddressInfo.AddMessageError(ValidationConstants.JobDocAddress.CustomsBrokerAddressRequiresABIRouting);
					}
				}
			}
		}

		void DocAddressValidateOrganisationPK(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var bill = (CusInBondBill)parent.Parent;
			if (bill != null && bill.IsInventoryRecordValidationMode && !parent.E2_AddressOverride && !parent.HasRealOrganisation)
			{
				parent.OrganisationPKInfo.AddMessageError(ValidationConstants.JobDocAddress.GetOrganisationPKRequired(parent.AddressCaption));
			}
		}

		void DocAddressValidateCompanyName(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var bill = (CusInBondBill)parent.Parent;
			if (bill != null && bill.IsInventoryRecordValidationMode)
			{
				if (parent.E2_CompanyName.IsEmpty)
				{
					parent.E2_CompanyNameInfo.AddMessageError(ValidationConstants.JobDocAddress.CompanyNameRequired);
				}
				else
				{
					AMSCharactersValidator.ValidateCharacters(parent.E2_CompanyNameInfo);
				}
			}
		}

		void DocAddressValidateAddress1(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var bill = (CusInBondBill)parent.Parent;
			if (bill != null && bill.IsInventoryRecordValidationMode)
			{
				if (parent.E2_Address1.IsEmpty)
				{
					parent.E2_Address1Info.AddMessageError(ValidationConstants.JobDocAddress.AddressRequired);
				}
				else
				{
					AMSCharactersValidator.ValidateCharacters(parent.E2_Address1Info);
				}
			}
		}

		void DocAddressValidateAddress2(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (!parent.E2_Address2.IsEmpty)
			{
				var bill = (CusInBondBill)parent.Parent;
				if (bill != null && bill.IsInventoryRecordValidationMode)
				{
					AMSCharactersValidator.ValidateCharacters(parent.E2_Address2Info);
				}
			}
		}

		void DocAddressContact(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (!parent.E2_Contact.IsEmpty)
			{
				var bill = (CusInBondBill)parent.Parent;
				if (bill != null && bill.IsInventoryRecordValidationMode)
				{
					AMSCharactersValidator.ValidateCharacters(parent.E2_ContactInfo);
				}
			}
		}

		Dictionary<DocAddressType, JobDocAddressRequirement> PartiesJobDocAddressRequirements
		{
			get { return fPartiesJobDocAddressRequirements ?? (fPartiesJobDocAddressRequirements = new Dictionary<DocAddressType, JobDocAddressRequirement>()); }
		}
		Dictionary<DocAddressType, JobDocAddressRequirement> fPartiesJobDocAddressRequirements;
	}
}
