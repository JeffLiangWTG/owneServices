using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ACEOceanManifestJobDocAddressValidation : JobDocAddressValidation
	{
		public ACEOceanManifestJobDocAddressValidation(JobDocAddress parent)
			: base(parent)
		{ }

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			if (IsAddressOverridenValidationEnabled)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.E2_CompanyNameInfo);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_CompanyNameInfo);
			}
		}

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();
			if (IsAddressOverridenValidationEnabled)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.E2_Address1Info);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_Address1Info);
			}
		}

		protected override void CheckE2_Address2()
		{
			base.CheckE2_Address2();
			if (IsAddressOverridenValidationEnabled)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.E2_Address2Info);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_Address2Info);
			}
		}

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();
			if (IsAddressOverridenValidationEnabled)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.E2_PostcodeInfo);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_PostcodeInfo);
			}
		}

		protected override void CheckE2_City()
		{
			base.CheckE2_City();
			if (IsAddressOverridenValidationEnabled)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.E2_CityInfo);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_CityInfo);
			}
		}

		protected override void CheckE2_State()
		{
			base.CheckE2_State();
			if (IsAddressOverridenValidationEnabled)
			{
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_StateInfo);
			}
		}

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();
			if (IsAddressOverridenValidationEnabled)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.E2_ContactInfo);
				EnglishAddressCharactersValidation.MessageErrorIfNotEnglish(Parent.E2_ContactInfo);
			}
		}

		protected override void CheckE2_Phone()
		{
			base.CheckE2_Phone();
			if (IsAddressOverridenValidationEnabled)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.E2_PhoneInfo);
			}
		}

		protected override void CheckE2_Fax()
		{
			base.CheckE2_Fax();
			if (IsAddressOverridenValidationEnabled)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.E2_FaxInfo);
			}
		}

		protected override void CheckE2_Email()
		{
			base.CheckE2_Email();
			if (IsAddressOverridenValidationEnabled)
			{
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.E2_EmailInfo);
			}
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (Parent.HasRealOrganisation)
			{
				using (GetEnableAddressOverridenFieldsValidation())
				{
					ValidateE2_CompanyName();
					Parent.OrganisationPKInfo.AddAllNotificationsFrom(Parent.E2_CompanyNameInfo);
				}

				ValidateContactPK();
				ValidateE2_OA_Address();

				Parent.OrganisationPKInfo.AddAllNotificationsFrom(Parent.ContactPKInfo);
				Parent.OrganisationPKInfo.AddAllNotificationsFrom(Parent.E2_OA_AddressInfo);
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (Parent.HasRealAddress)
			{
				using (GetEnableAddressOverridenFieldsValidation())
				{
					ValidateE2_Address1();
					ValidateE2_Address2();
					ValidateE2_Postcode();
					ValidateE2_City();
					ValidateE2_RN_NKCountryCode();
					ValidateE2_GovRegNumType();
					ValidateE2_GovRegNum();

					Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_Address1Info);
					Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_Address2Info);
					Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_PostcodeInfo);
					Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_CityInfo);
					Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_RN_NKCountryCodeInfo);
					Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_GovRegNumTypeInfo);
					Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_GovRegNumInfo);

					if (!Parent.ContactPK.IsValid)
					{
						ValidateE2_Phone();
						ValidateE2_Fax();
						ValidateE2_Email();

						Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_PhoneInfo);
						Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_FaxInfo);
						Parent.E2_OA_AddressInfo.AddAllNotificationsFrom(Parent.E2_EmailInfo);
					}
				}
			}

			ValidateOrganisationPK();
		}

		protected override void CheckContactPK()
		{
			base.CheckContactPK();
			if (Parent.ContactPK.IsValid)
			{
				using (GetEnableAddressOverridenFieldsValidation())
				{
					ValidateE2_Contact();
					ValidateE2_Phone();
					ValidateE2_Fax();
					ValidateE2_Email();

					Parent.ContactPKInfo.AddAllNotificationsFrom(Parent.E2_ContactInfo);
					Parent.ContactPKInfo.AddAllNotificationsFrom(Parent.E2_PhoneInfo);
					Parent.ContactPKInfo.AddAllNotificationsFrom(Parent.E2_FaxInfo);
					Parent.ContactPKInfo.AddAllNotificationsFrom(Parent.E2_EmailInfo);
				}
			}

			ValidateOrganisationPK();
		}

		protected override bool IsAddressOverridenValidationEnabled
		{
			get { return fEnableAddressOverridenFieldsValidationLock > 0 || base.IsAddressOverridenValidationEnabled; }
		}

		protected IDisposable GetEnableAddressOverridenFieldsValidation()
		{
			return new EnableAddressOverridenFieldsValidation(this);
		}

		class EnableAddressOverridenFieldsValidation : IDisposable
		{
			public EnableAddressOverridenFieldsValidation(ACEOceanManifestJobDocAddressValidation validation)
			{
				this.validation = validation;
				this.validation.fEnableAddressOverridenFieldsValidationLock++;
			}
			readonly ACEOceanManifestJobDocAddressValidation validation;

			public void Dispose()
			{
				this.validation.fEnableAddressOverridenFieldsValidationLock--;
			}
		}
		int fEnableAddressOverridenFieldsValidationLock;
	}
}
