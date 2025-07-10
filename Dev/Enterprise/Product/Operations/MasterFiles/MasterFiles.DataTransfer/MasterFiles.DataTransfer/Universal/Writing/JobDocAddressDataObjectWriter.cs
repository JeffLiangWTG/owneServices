using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class JobDocAddressDataObjectWriter : DataObjectWriter<JobDocAddress, OrganizationAddress>
	{
		public JobDocAddressDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		public bool PopulateIsResidential { get; set; }
		public bool PopulateGeoLocation { get; set; }
		public bool PopulateValidationStatus { get; set; }

		protected override OrganizationAddress PopulateDataObject(JobDocAddress docAddressBO)
		{
			if (docAddressBO == null || docAddressBO.IsEmpty)
			{
				return null;
			}

			var addressType = docAddressBO.DocAddressType;
			if (!docAddressBO.E2_AddressOverride && docAddressBO.HasRealAddress)
			{
				var addressBO = docAddressBO.Address;
				var addressNew = new OrganizationDataObjectWriter(writeManager, addressType.ToString(), docAddressBO.Contact)
				{
					PopulateGeoLocation = PopulateGeoLocation,
					PopulateValidationStatus = PopulateValidationStatus,
				}.GetDataObject(addressBO);
				if (docAddressBO.E2_Contact != "")
				{
					addressNew.Contact = docAddressBO.E2_Contact;
				}

				if (docAddressBO.E2_AdditionalAddressInformation != "")
				{
					addressNew.AdditionalAddressInformation = docAddressBO.E2_AdditionalAddressInformation;
				}

				PopulateAdditionalProperties(addressNew, docAddressBO);

				return addressNew;
			}

			var addressData = new OrganizationAddress(writeManager.WriterStrategy);
			addressData.AddressType = addressType.ToString();
			addressData.AddressOverride = docAddressBO.E2_AddressOverride;
			addressData.CompanyName = docAddressBO.E2_CompanyNameTruncated;
			addressData.Country = Country.New(docAddressBO.Country);
			addressData.ScreeningStatus = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair>(docAddressBO.E2_ScreeningStatus, new ScreeningStatusesList());

			addressData.Address1 = docAddressBO.E2_Address1;
			addressData.Address2 = docAddressBO.E2_Address2;
			addressData.AdditionalAddressInformation = docAddressBO.E2_AdditionalAddressInformation;
			addressData.City = docAddressBO.E2_City;
			addressData.Postcode = docAddressBO.E2_Postcode;
			addressData.State = OrganizationAddressState.New(docAddressBO.E2_State, (ZString stateCode) => OrganizationDataObjectWriter.GetStateNameFromCode(docAddressBO.Factory, docAddressBO.Country?.Code, stateCode));

			if (PopulateIsResidential || docAddressBO.E2_IsResidential)
			{
				addressData.IsResidential = docAddressBO.E2_IsResidential;
			}

			addressData.Contact = docAddressBO.E2_Contact;
			addressData.Email = docAddressBO.E2_Email;
			addressData.Fax = docAddressBO.E2_Fax;
			addressData.Mobile = docAddressBO.E2_Mobile;
			addressData.Phone = docAddressBO.E2_Phone;

			if (docAddressBO.SupportsDocAddressNumbers)
			{
				PopulateDocAddressNumbers(addressData, docAddressBO);
			}
			else
			{
				addressData.GovRegNum = docAddressBO.E2_GovRegNum;
				addressData.GovRegNumType = ListHelper.GetWithDescription<RegistrationNumberType>(docAddressBO.E2_GovRegNumType, docAddressBO.Lookups.GovRegNumTypes);
			}

			PopulateAdditionalProperties(addressData, docAddressBO);

			return addressData;
		}

		void PopulateAdditionalProperties(OrganizationAddress output, JobDocAddress docAddressBO)
		{
			if (PopulateGeoLocation)
			{
				output.GeoLocation = GeoLocation.New(docAddressBO.E2_GeoLocation);
			}

			if (PopulateValidationStatus)
			{
				output.ValidationStatus = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair>(docAddressBO.E2_ValidationStatus, AddressValidationStatusList.AddressValidationStatuses);
			}
		}

		void PopulateDocAddressNumbers(OrganizationAddress output, JobDocAddress docAddressBO)
		{
			var bizObjNumbers = docAddressBO.DocAddressNumbers.Cast<JobDocAddressNumber>().Where(c => !c.E2N_NumberType.IsEmpty && !c.E2N_Number.IsEmpty);

			var numberTypeCodeDescriptionList = docAddressBO.Lookups.GovRegNumTypes;
			PopulateGovRegNumber(output, bizObjNumbers, numberTypeCodeDescriptionList, docAddressBO.Country);
			PopulateRegistrationNumbers(output, bizObjNumbers, numberTypeCodeDescriptionList);
		}

		void PopulateGovRegNumber(OrganizationAddress output, IEnumerable<JobDocAddressNumber> bizObjNumbers, CodeDescriptionPairList numberTypeCodeDescriptionList, RefCountry country)
		{
			JobDocAddressNumber registrationNumber = null;

			if (country != null)
			{
				var applicableRegNumberTypes = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(country);
				registrationNumber = bizObjNumbers.FirstOrDefault(x => applicableRegNumberTypes.Contains<string>(x.E2N_NumberType));
			}

			if (registrationNumber != null)
			{
				output.GovRegNum = registrationNumber.E2N_Number;
				output.GovRegNumType = ListHelper.GetWithDescription<RegistrationNumberType>(registrationNumber.E2N_NumberType, numberTypeCodeDescriptionList);
			}
			else
			{
				output.GovRegNum = ZString.Empty;
				output.GovRegNumType = null;
			}
		}

		void PopulateRegistrationNumbers(OrganizationAddress output, IEnumerable<JobDocAddressNumber> bizObjNumbers, CodeDescriptionPairList numberTypeCodeDescriptionList)
		{
			var docAddressNumbers = output.GovRegNumType == null
				? bizObjNumbers
				: bizObjNumbers.Where(number => number.E2N_NumberType != output.GovRegNumType.Code.Value);

			if (docAddressNumbers.Any() && output.SetRegistrationNumberCollection(() => output.RegistrationNumberCollection ?? new List<RegistrationNumber>()))
			{
				var registrationNumberCollection = docAddressNumbers.Select(
					number =>
					new RegistrationNumber
					{
						Type = ListHelper.GetWithDescription<RegistrationNumberType>(number.E2N_NumberType, numberTypeCodeDescriptionList),
						CountryOfIssue = Country.New(number.Country),
						Value = number.E2N_Number,
					}
				);

				output.RegistrationNumberCollection.AddRange(registrationNumberCollection);
			}
		}
	}
}
