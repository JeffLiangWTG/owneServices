using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class OrganizationDataObjectWriter : DataObjectWriter<OrgAddress, OrganizationAddress>
	{
		public OrganizationDataObjectWriter(IDataWritingManager writeManager, ZString addressType, OrgContact contact = null)
			: base(writeManager)
		{
			this.addressType = addressType;
			this.contact = contact;
		}

		readonly ZString addressType;
		readonly OrgContact contact;
		public bool PopulateGeoLocation { get; set; }
		public bool PopulateValidationStatus { get; set; }

		protected override OrganizationAddress PopulateDataObject(OrgAddress addressBO)
		{
			if (addressBO == null || addressBO.Header == null)
			{
				return null;
			}

			var organizationBO = addressBO.Header;

			var addressData = new OrganizationAddress(writeManager.WriterStrategy);
			addressData.AddressType = addressType;

			if (addressType == "LocalClient")
			{
				ErrorReporter.ReportOnce("You have tried to export XML with AddressType LocalClient. Should be SendersLocalClient");
				addressData.AddressType = AddressTypes.SendersLocalClient;
			}
			else if (addressType == "OverseasAgent")
			{
				ErrorReporter.ReportOnce("You have tried to export XML with AddressType OverseasAgent. Should be SendersOverseasAgent");
				addressData.AddressType = AddressTypes.SendersOverseasAgent;
			}

			addressData.AddressOverride = ZBool.False;
			addressData.OrganizationCode = organizationBO.OH_Code;
			addressData.OrganizationCategory = organizationBO.OH_Category;
			addressData.Port = ListHelper.GetWithName(addressBO.OA_RL_NKRelatedPortCode, organizationBO.Lookups.ClosestPorts);
			var companyNameOverride = addressBO.OA_CompanyNameOverride;
			addressData.CompanyName = companyNameOverride.IsEmpty ? organizationBO.OH_FullName : companyNameOverride;
			addressData.Country = Country.New(addressBO.Country);
			addressData.ScreeningStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(organizationBO.OH_ScreeningStatus, organizationBO.Lookups.ScreeningStatusesList);

			addressData.AddressShortCode = addressBO.OA_Code;
			addressData.Address1 = addressBO.OA_Address1;
			addressData.Address2 = addressBO.OA_Address2;
			addressData.City = addressBO.OA_City;
			addressData.Postcode = addressBO.OA_PostCode;
			addressData.State = OrganizationAddressState.New(addressBO.OA_State, (ZString stateCode) => GetStateNameFromCode(addressBO.Factory, addressBO.Country?.Code, stateCode));

			if (contact != null)
			{
				addressData.Contact = contact.OC_ContactName;
				addressData.Email = contact.EmailFallbackToOrganisation;
				addressData.Fax = contact.FaxFallbackToOrganisation;
				addressData.Mobile = contact.OC_Mobile;
				addressData.Phone = contact.PhoneFallbackToOrganisation;
			}
			else
			{
				addressData.Email = addressBO.OA_Email;
				addressData.Fax = addressBO.OA_Fax;
				addressData.Phone = addressBO.OA_Phone;
			}

			PopulateRegistrationNumbers(organizationBO, addressData, addressBO);

			PopulateLocalAddresses(addressData, addressBO);

			PopulateAdditionalProperties(addressData, addressBO);

			return addressData;
		}

		void PopulateAdditionalProperties(OrganizationAddress output, OrgAddress addressBO)
		{
			if (PopulateGeoLocation)
			{
				output.GeoLocation = GeoLocation.New(addressBO.OA_GeoLocation);
			}

			if (PopulateValidationStatus)
			{
				output.ValidationStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(addressBO.OA_ValidationStatus, AddressValidationStatusList.AddressValidationStatuses);
			}
		}

		public static OrganizationAddress GetDataObject(BusinessObjectFactory factory, IDataWritingManager writeManager, IColumnIndexer row, ZString addressType, SchemaStringColumn nameColumn, SchemaStringColumn address1Column, SchemaStringColumn address2Column, SchemaStringColumn cityColumn, SchemaStringColumn stateColumn, SchemaStringColumn postcodeColumn, SchemaStringColumn countryColumn, SchemaStringColumn phoneColumn, SchemaStringColumn faxColumn, SchemaStringColumn contactNameColumn)
		{
			var bizO = row as BusinessObject;

			if (bizO == null)
			{
				return null;
			}

			Func<OrganizationAddress> getOrganizationAddress = () => new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = addressType,
				CompanyName = row.GetValue(nameColumn),
				Address1 = row.GetValue(address1Column),
				Address2 = row.GetValue(address2Column),
				City = row.GetValue(cityColumn),
				Postcode = row.GetValue(postcodeColumn),
				Country = Country.New(factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, row.GetValue(countryColumn))),
				State = OrganizationAddressState.New(row.GetValue(stateColumn), (ZString stateCode) => GetStateNameFromCode(factory, row.GetValue(countryColumn), stateCode)),
				Contact = row.GetValue(contactNameColumn),
				Phone = row.GetValue(phoneColumn),
				Fax = row.GetValue(faxColumn),
			};

			return new ColumnIndexerWriter(writeManager, getOrganizationAddress).GetDataObject(bizO);
		}

		public static ZString? GetStateNameFromCode(BusinessObjectFactory factory, ZString? countryCode, ZString stateCode)
		{
			return new RefCountryStates.Loader(factory).LoadRefCountryStatesFromCode(stateCode, countryCode.GetValueOrDefault())?.RW_Description ?? stateCode;
		}

		void PopulateLocalAddresses(OrganizationAddress targetAddress, OrgAddress addressBO)
		{
			var localAddressWriter = new OrganizationLocalAddressDataObjectWriter(writeManager);
			var localAddrsses = addressBO.TranslatedAddresses.Select(localAddress => localAddressWriter.GetDataObject(localAddress)).ToArray();
			if (localAddrsses.Any() && (targetAddress.LocalAddressCollection != null || targetAddress.SetLocalAddressCollection(() => new List<OrganizationLocalAddress>())))
			{
				targetAddress.LocalAddressCollection.AddRange(localAddrsses);
			}
		}

		class ColumnIndexerWriter : DataObjectWriter<BusinessObject, OrganizationAddress>
		{
			public ColumnIndexerWriter(IDataWritingManager writeManager, Func<OrganizationAddress> getOrganizationAddress)
				: base(writeManager)
			{
				this.getOrganizationAddress = getOrganizationAddress;
			}

			readonly Func<OrganizationAddress> getOrganizationAddress;

			protected override OrganizationAddress PopulateDataObject(BusinessObject bizO)
			{
				return getOrganizationAddress();
			}
		}

		void PopulateRegistrationNumbers(OrgHeader organizationBO, OrganizationAddress addressData, OrgAddress addressBO)
		{
			var govRegNumBO = organizationBO.PrimaryRegistrationNumber.CusCode;
			if (govRegNumBO != null)
			{
				addressData.GovRegNum = govRegNumBO.OK_CustomsRegNo;
				addressData.GovRegNumType = ListHelper.GetWithDescription<RegistrationNumberType>(govRegNumBO.OK_CodeType, govRegNumBO.Lookups.OK_CodeType_List);
			}

			foreach (OrgCusCode registrationCodeBO in organizationBO.CustomsCodes)
			{
				if (registrationCodeBO.OK_CodeType == OrgCusCode.CodeTypes.UniversalNettingCode)
				{
					addressData.UniversalNettingCode = registrationCodeBO.OK_CustomsRegNo;
				}
				else if (registrationCodeBO.OK_CodeType == OrgCusCode.CodeTypes.UniversalOfficeCode)
				{
					addressData.UniversalOfficeCode = registrationCodeBO.OK_CustomsRegNo;
				}
				else if (registrationCodeBO != govRegNumBO && (registrationCodeBO.OK_OA_PremisesAddress == ZGuid.Empty || registrationCodeBO.OK_OA_PremisesAddress == addressBO.PK))
				{
					if (addressData.RegistrationNumberCollection != null || addressData.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()))
					{
						addressData.RegistrationNumberCollection.Add(new RegistrationNumber()
						{
							Type = ListHelper.GetWithDescription<RegistrationNumberType>(registrationCodeBO.OK_CodeType, registrationCodeBO.Lookups.OK_CodeType_List),
							CountryOfIssue = Country.New(registrationCodeBO.CodeCountry),
							Value = registrationCodeBO.OK_CustomsRegNo,
						});
					}
				}
			}
		}
	}
}
