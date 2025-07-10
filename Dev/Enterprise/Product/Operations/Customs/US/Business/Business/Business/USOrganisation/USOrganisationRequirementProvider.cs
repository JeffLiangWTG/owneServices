using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class USOrganisationRequirementProvider
	{
		public USOrganisationRequirementProvider(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public JobDocAddressRequirement USPPIDocAddressRequirement
		{
			get
			{
				if (f_USPPIDocAddressRequirement == null)
				{
					f_USPPIDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.USPrincipalPartyInInterest, AddressType.NoDefault, ContactType.Consignor);
					f_USPPIDocAddressRequirement.GetRegistrationNumberResult = (docAddress) =>
					{
						return new RegistrationNumberResult(factory, true, () =>
						{
							var result = new RegistrationNumber();
							if (docAddress != null && !docAddress.IsDeleted)
							{
								var cusCode = OrgHeaderWrapper.GetOrgCusCodeObjectMatching(docAddress.Organisation, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.ForeignRegistrationNumber }) ?? (docAddress.Address?.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates));

								if (cusCode != null)
								{
									result.Number = cusCode.OK_CustomsRegNo;
									result.NumberType = cusCode.OK_CodeType;
								}
							}

							return result;
						});
					};
					f_USPPIDocAddressRequirement.LookupsGovRegNumTypes = Lookups.GetUSPPIDocAddressRegNumTypes;
					f_USPPIDocAddressRequirement.ValidateGovRegNumType = Validation.ValidateUSPPIGovRegNumType;
					f_USPPIDocAddressRequirement.ValidateGovRegNo = Validation.ValidateRegistrationNumber;

					JobDocAddressRequirement.ValidationDelegate noValidation = delegate
					{ };
					f_USPPIDocAddressRequirement.ValidateAddress1 = noValidation;
					f_USPPIDocAddressRequirement.ValidateAddress2 = noValidation;
					f_USPPIDocAddressRequirement.ValidateCountry = noValidation;
					f_USPPIDocAddressRequirement.ValidateCity = noValidation;
					f_USPPIDocAddressRequirement.ValidateState = noValidation;
					f_USPPIDocAddressRequirement.ValidatePostCode = noValidation;
				}
				return f_USPPIDocAddressRequirement;
			}
		}
		JobDocAddressRequirement f_USPPIDocAddressRequirement;

		public JobDocAddressRequirement UltimateConsigneeDocAddressRequirement
		{
			get
			{
				if (ultimateConsigneeDocAddressRequirement == null)
				{
					ultimateConsigneeDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.UltimateConsignee, AddressType.NoDefault, ContactType.Consignee);
					ultimateConsigneeDocAddressRequirement.GetRegistrationNumberResult = (docAddress) =>
					{
						return new RegistrationNumberResult(factory, true, () =>
						{
							var result = new RegistrationNumber();
							if (docAddress != null && !docAddress.IsDeleted)
							{
								var cusCode = OrgHeaderWrapper.GetOrgCusCodeObjectMatching(docAddress.Organisation, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber }) ?? (docAddress.Address?.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates));

								if (cusCode != null)
								{
									result.Number = cusCode.OK_CustomsRegNo;
									result.NumberType = cusCode.OK_CodeType;
								}
							}

							return result;
						});
					};
					ultimateConsigneeDocAddressRequirement.LookupsGovRegNumTypes = Lookups.GetConsigneeDocAddressRegNumTypes;
					ultimateConsigneeDocAddressRequirement.ValidateGovRegNumType = Validation.ValidateUSPPIGovRegNumType;
					ultimateConsigneeDocAddressRequirement.ValidateGovRegNo = Validation.ValidateRegistrationNumber;
				}
				return ultimateConsigneeDocAddressRequirement;
			}
		}
		JobDocAddressRequirement ultimateConsigneeDocAddressRequirement;

		public JobDocAddressRequirement IntermediateConsigneeDocAddressRequirement
		{
			get
			{
				if (intermediateConsigneeDocAddressRequirement == null)
				{
					intermediateConsigneeDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.IntermediateConsignee, AddressType.NoDefault, ContactType.Consignee);
					intermediateConsigneeDocAddressRequirement.GetRegistrationNumberResult = (docAddress) =>
					{
						return new RegistrationNumberResult(factory, true, () =>
						{
							var result = new RegistrationNumber();
							if (docAddress != null && !docAddress.IsDeleted)
							{
								var cusCode = OrgHeaderWrapper.GetOrgCusCodeObjectMatching(docAddress.Organisation, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber }) ?? (docAddress.Address?.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates));

								if (cusCode != null)
								{
									result.Number = cusCode.OK_CustomsRegNo;
									result.NumberType = cusCode.OK_CodeType;
								}
							}

							return result;
						});
					};
					intermediateConsigneeDocAddressRequirement.LookupsGovRegNumTypes = Lookups.GetConsigneeDocAddressRegNumTypes;
					intermediateConsigneeDocAddressRequirement.ValidateGovRegNumType = Validation.ValidateUSPPIGovRegNumType;
					intermediateConsigneeDocAddressRequirement.ValidateGovRegNo = Validation.ValidateRegistrationNumber;
				}
				return intermediateConsigneeDocAddressRequirement;
			}
		}
		JobDocAddressRequirement intermediateConsigneeDocAddressRequirement;

		public JobDocAddressRequirement SupplierPicDlvAddressRequirement
		{
			get { return supplierPicDlvAddressRequirement ?? (supplierPicDlvAddressRequirement = new JobDocAddressRequirement(DocAddressType.SupplierPickupDeliveryAddress, AddressType.PIC)); }
		}
		JobDocAddressRequirement supplierPicDlvAddressRequirement;

		USOrganisationRequirementLookups Lookups
		{
			get { return fLookups ?? (fLookups = new USOrganisationRequirementLookups(factory)); }
		}
		USOrganisationRequirementLookups fLookups;

		USOrganisationRequirementValidation Validation
		{
			get { return fValidation ?? (fValidation = new USOrganisationRequirementValidation()); }
		}
		USOrganisationRequirementValidation fValidation;
	}
}
