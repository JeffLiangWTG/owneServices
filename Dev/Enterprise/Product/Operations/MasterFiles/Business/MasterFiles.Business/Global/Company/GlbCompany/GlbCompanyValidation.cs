using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyValidation : AutoGlbCompanyValidation
	{
		public GlbCompanyValidation(AutoGlbCompany parent)
			: base(parent)
		{
			phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());
		}

		new GlbCompany Parent
		{
			get { return (GlbCompany)base.Parent; }
		}

		AddressValidation addressValidation;
		AddressValidation AddressValidation => addressValidation ?? (addressValidation = new AddressValidation());

		#region GC_IsGSTRegistered

		protected override void CheckGC_IsGSTRegistered()
		{
			base.CheckGC_IsGSTRegistered();
			if (Parent.HasGSTRegisteredChangedToUnregistered)
			{
				Parent.GC_IsGSTRegisteredInfo.AddWarning(Res.GetString("42da6aa3-5ccc-4396-91a2-2dc16b0aa55a", "You have changed the Tax Registration status of this company, which can have dire consequences if it was not intended."));
			}
		}

		#endregion

		#region GC_Code

		protected override void CheckGC_Code()
		{
			base.CheckGC_Code();

			var reason = CheckCode(Parent.GC_Code);
			if (reason != null)
			{
				Parent.GC_CodeInfo.AddError(reason);
			}

			if (!Parent.GC_CodeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.GC_CodeInfo);
				if (!Parent.GC_CodeInfo.HasErrors())
				{
					ZQuery filter = new ZQuery(GlbCompanySchema.GC_Code, Parent.GC_Code);
					filter.AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					GlbCompany existingCompany = Parent.Factory.LoadTop1<GlbCompany>(filter);
					if (existingCompany != null)
					{
						Parent.GC_CodeInfo.AddError(Res.GetString("dcebd0b1-f5a7-41b7-b454-68e7a6af89da", "A company with this Code already exists."));
					}
				}
			}
		}

		/// <summary>
		/// Returns null if code is valid, or reason if invalid.
		/// </summary>
		public static string CheckCode(ZString code)
		{
			if (!Regex.IsMatch(code, @"^[a-zA-Z0-9]{3}$"))
			{
				return Res.GetString("cbdbc5a8-080d-4829-ad8a-6eaea19736d0", "Company code must be 3 characters in length and consist only of letters and digits.");
			}

			return null;
		}

		#endregion

		#region GC_Name

		protected override void CheckGC_Name()
		{
			base.CheckGC_Name();
			MandatoryValidation.CheckEntered(Parent.GC_NameInfo);
		}

		#endregion

		#region GC_OH

		protected override void CheckGC_OH_OrgProxy()
		{
			base.CheckGC_OH_OrgProxy();

			if (!Parent.GC_OH_OrgProxyInfo.HasErrors() && !Parent.IsDemoCompany)
			{
				MandatoryValidation.CheckEntered(Parent.GC_OH_OrgProxyInfo);
			}
		}

		#endregion

		#region GC_RX_NKLocalCurrency

		protected override void CheckGC_RX_NKLocalCurrency()
		{
			base.CheckGC_RX_NKLocalCurrency();
			MandatoryValidation.CheckEntered(Parent.GC_RX_NKLocalCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GC_RX_NKLocalCurrencyInfo);

			if (Parent.GC_RX_NKLocalCurrencyInfo.HasChanges && ((ObjectFactory.Get<IAccounting>().Registry?.EnableElectronicProcessingChargeFunctionality as BooleanRegistryItem)?.GetFallBackValueAtAllLevels(Parent.PK.ToGuid(), Guid.Empty, Guid.Empty) ?? false))
			{
				var filter = new ZQuery(RefAccElectronicProcessingFeeSchema.EPF_Currency, Parent.GC_RX_NKLocalCurrency);
				filter.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_SystemCode, RefAccElectronicProcessingFeeLookups.SystemCodes.CWN);
				filter.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_Category, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL);
				filter.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_Code, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD);
				filter.AddToFilter(RefAccElectronicProcessingFeeSchema.EPF_ValidFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
				var exists = Parent.Factory.Exists(typeof(RefAccElectronicProcessingFee), filter);
				if (!exists)
				{
					Parent.GC_RX_NKLocalCurrencyInfo.AddError(Res.GetString("5e9e2b79-52a8-46b0-a6bf-cadbaf1e2ddf", @"Please log a Customer Service Incident requesting support for <{0}> to be introduced.
A number of key settings must first be defined by WiseTech Global before a functional currency <{0}> - <{1}> can be selected.
Please open a new Customer Service Incident requesting action to enable <{0}>.", Parent.GC_RX_NKLocalCurrency, Parent.LocalCurrency.RX_Desc));
				}
			}
		}

		#endregion

		#region GC_RN_NKCountryCode

		protected override void CheckGC_RN_NKCountryCode()
		{
			base.CheckGC_RN_NKCountryCode();
			MandatoryValidation.CheckEntered(Parent.GC_RN_NKCountryCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GC_RN_NKCountryCodeInfo);

			if (!Parent.GC_RN_NKCountryCodeInfo.HasNotifications() && (Parent.GC_RN_NKCountryCodeInfo.HasChanges || !Parent.IsInDatabase))
			{
				if (!Country.IsSupportedForLicenceBuilder(Parent.GC_RN_NKCountryCode))
				{
					var lookups = new GlbCompanyLookups(Parent);
					var findBoxListProvider = lookups.Countries as IFindBoxListProvider;
					var countryName = findBoxListProvider == null ? "" : findBoxListProvider.DescriptionFromCode(Parent.GC_RN_NKCountryCode);

					Parent.GC_RN_NKCountryCodeInfo.AddError(Res.GetString("4f81eba0-2d89-42a2-a8ae-9551e4969723", @"Please log a Customer Service Incident requesting support for <{1}> to be introduced.

																																																					A number of key settings must first be defined by WiseTech Global before a Login Company for <{0}> - <{1}> can be created.

																																																					Please open a new Customer Service Incident requesting action to enable <{1}>.", Parent.GC_RN_NKCountryCode, countryName));
				}
			}
		}

		#endregion

		#region GC_IsActive

		protected override void CheckGC_IsActive()
		{
			base.CheckGC_IsActive();

			if (Parent.GC_IsActive &&
				!((ZBool)Parent.GC_IsActiveInfo.OriginalValue) &&
				!Parent.IsDemoCompany &&
				!Parent.LicenceEnterpriseCode.IsEmpty)
			{
				var query = new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True);
				query.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode);
				query.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, Parent.GC_Code);
				var activeCompanies = Parent.Factory.Load<GlbCompany>(query);
				if (activeCompanies.Length > 0 && !activeCompanies.Any(x => x.LicenceEnterpriseCode == Parent.LicenceEnterpriseCode))
				{
					Parent.GC_IsActiveInfo.AddError(Res.GetString("5EC3C3EE-E3D6-46CD-A2D1-61672855BF1A", "License Enterprise Code '{0}' does not match the value of any other active company. Please obtain a new license key.", Parent.LicenceEnterpriseCode));
				}
			}
			if (!Parent.GC_IsActive)
			{
				bool hasActiveBranch = Parent.Branches.Any(x => x.GB_IsActive);

				if (hasActiveBranch)
				{
					Parent.GC_IsActiveInfo.AddError(Res.GetString("3403A33E-9851-4D67-8F77-7A09167D0DB4", "Deactivate all active branches before deactivating the global company"));
				}
			}
		}

		#endregion

		#region GC_StartDate

		protected override void CheckGC_StartDateIsValidZDateTimeRange()
		{
			if (Parent.GC_StartDate < new ZDateTime(1900, 01, 01) || Parent.GC_StartDate > new ZDateTime(2079, 06, 06))
			{
				Parent.GC_StartDateInfo.AddError(Res.GetString("163c7acb-5d5d-4f9a-9b20-b008ac30a9ca", "Date must be within 01-JAN-1900 and 06-JUN-2079."));
			}
		}

		#endregion

		#region Phone Numbers

		#region GC_Phone

		public void ValidateGC_Phone_Formatted()
		{
			ValidateCalculatedProperty(Parent.GC_Phone_FormattedInfo);
		}

		protected virtual void CheckGC_Phone_Formatted()
		{
			if (Parent.GC_IsActive)
			{
				ValidatePhoneNumber(Parent.GC_Phone_FormattedInfo, Parent.GC_PhoneInfo, Parent.GC_Phone_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region GC_Fax

		public void ValidateGC_Fax_Formatted()
		{
			ValidateCalculatedProperty(Parent.GC_Fax_FormattedInfo);
		}

		protected virtual void CheckGC_Fax_Formatted()
		{
			ValidatePhoneNumber(Parent.GC_Fax_FormattedInfo, Parent.GC_FaxInfo, Parent.GC_Fax_IsManuallyVerifiedInfo);
		}

		#endregion

		#region Implementations

		void ValidatePhoneNumber(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty)
		{
			PhoneNumberFormatterAndValidator.Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, Parent.DefaultCountryCodeForPhoneNumbers);
		}

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk;

		#endregion

		#endregion

		#region GC_Address1

		protected override void CheckGC_Address1()
		{
			MandatoryValidation.CheckEntered(Parent.GC_Address1Info);
		}

		#endregion

		#region GC_City

		protected override void CheckGC_City()
		{
			if (!ShouldValidateAddress() || !AddressValidation.IsWebVerified(Parent.ValidationStatus))
			{
				MandatoryValidation.CheckEntered(Parent.GC_CityInfo);
			}
		}

		bool ShouldValidateAddress()
		{
			return
				Parent.GC_IsActive &&
				Parent.Country != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Parent.Country.PK.ToGuid(), Parent.ValidationSection);
		}

		#endregion

		#region GC_Postcode

		protected override void CheckGC_PostCode()
		{
			RefCountry country = Parent.Country;
			AddressValidation.CheckPostCode(Parent.GC_PostCodeInfo, country, Parent.ValidationSection, Parent.ValidationStatus);
		}

		#endregion

		#region GC_State

		protected override void CheckGC_State()
		{
			AddressValidation.CheckState(Parent.GC_StateInfo, Parent.Country, Parent.ValidationSection, Parent.ValidationStatus);
		}

		#endregion

		#region GC_Email

		protected override void CheckGC_Email()
		{
			if (!Parent.GC_Email.IsEmpty)
			{
				AddressValidation.CheckEmail(Parent.GC_EmailInfo);
			}
		}

		#endregion

		#region GC_WebAddress

		protected override void CheckGC_WebAddress()
		{
			var branch = Parent.Factory.Load<GlbBranch>(Env.Registry.WebBranch);
			if (branch != null)
			{
				if (Parent.GC_Code == branch.Company.GC_Code)
				{
					MandatoryValidation.CheckEntered(Parent.GC_WebAddressInfo);
				}
			}
			if (!Parent.GC_WebAddress.IsEmpty && !Parent.GC_WebAddressInfo.HasErrors() && !UrlValidation.IsValidUrl(Parent.GC_WebAddress))
			{
				Parent.GC_WebAddressInfo.AddError(Res.GetString("02093877-0be0-45d7-8262-d169fd19c01c", "Please enter a valid website address (URL).\r\n\r\nA valid address is commonly found in the format \"{0}\" or \"{1}\"", "http://", "www."));
			}
		}

		#endregion

		#region GC_BusinessRegNo

		protected override void CheckGC_BusinessRegNo()
		{
			base.CheckGC_BusinessRegNo();
			if (Parent.IsInDatabase && Parent.GC_BusinessRegNoInfo.HasChanges)
			{
				Parent.GC_BusinessRegNoInfo.AddWarning(Res.GetString("16a80d7b-8257-4b44-a14e-8ed80e572992", "Previously issued invoices will need to be reprinted."));
			}
			if (Parent.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Spain && Parent.GC_BusinessRegNo.IsEmpty)
			{
				Parent.GC_BusinessRegNoInfo.AddWarning(Res.GetString("d37ebde7-4afc-4522-8ed0-0c8e7c6cb1ac", "For Spain login companies, you must record your companies NIF."));
			}
		}

		#endregion

		#region GC_BusinessRegNo2

		protected override void CheckGC_BusinessRegNo2()
		{
			base.CheckGC_BusinessRegNo2();
			if (Parent.IsInDatabase && Parent.GC_BusinessRegNo2Info.HasChanges)
			{
				Parent.GC_BusinessRegNo2Info.AddWarning(Res.GetString("16a80d7b-8257-4b44-a14e-8ed80e572992", "Previously issued invoices will need to be reprinted."));
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGC_Phone_Formatted();
			ValidateGC_Fax_Formatted();
		}

		protected override void CheckGC_ValidationStatus()
		{
			base.CheckGC_ValidationStatus();

			if (ShouldValidateAddress() && Parent.GC_ValidationStatus == AddressValidationStatus.Invalid && !Parent.IgnoreValidationStatusError)
			{
				var shouldSuppressError = OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled;

				var message = Res.GetString(
					"8315d38d-83d4-4ed9-9678-2c7354225bc4",
					"The address is invalid. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				if (shouldSuppressError)
				{
					Parent.GC_ValidationStatusInfo.AddWarning(message);
				}
				else
				{
					Parent.GC_ValidationStatusInfo.AddError(message);
				}
			}
		}

		#endregion
	}
}
