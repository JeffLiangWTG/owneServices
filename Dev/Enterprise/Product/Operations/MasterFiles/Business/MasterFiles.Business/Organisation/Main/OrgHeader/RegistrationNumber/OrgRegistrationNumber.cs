using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgRegistrationNumber : NonPersistentBusinessObject
	{
		ZString? lastCalculatedNumberType;
		OrgRegistrationNumberLookups lookups;
		readonly OrgHeader organization;
		ZString numberTypeForDisplay;

		public OrgRegistrationNumber(OrgHeader organization)
			: base(organization.Factory)
		{
			this.organization = organization;
		}

		public ZString CountryCode
		{
			get { return Lookups.NumberTypes.GetCountryCode(NumberTypeForDisplay); }
		}

		public OrgCusCode CusCode
		{
			get { return GetCusCode(NumberType, CountryCode); }
		}

		public OrgRegistrationNumberLookups Lookups
		{
			get { return lookups ?? (lookups = new OrgRegistrationNumberLookups(this)); }
		}

		public OrgHeader Organization
		{
			get { return organization; }
		}

		[BusinessObjectTestExclude]
		[MaxLength(OrgCusCode.Schema.OK_CustomsRegNoMaxLength)]
		public ZString Number
		{
			get
			{
				OrgCusCode cusCode = CusCode;
				return (cusCode == null) ? ZString.Empty : cusCode.OK_CustomsRegNo;
			}
			set
			{
				if ((Number != value) && NumberTypeForDisplay.IsValid && !NumberTypeForDisplayInfo.HasErrors())
				{
					OrgCusCode cusCode = CusCode;
					Organization.PatternMatchRequiresRegen = true;
					organization.FindDuplicates();

					if (value.IsEmpty)
					{
						if (cusCode != null)
						{
							organization.CustomsCodes.RemoveAndDelete(cusCode);
						}
					}
					else
					{
						if (cusCode == null)
						{
							cusCode = organization.CustomsCodes.AddNew();
						}
						cusCode.OK_RN_NKCodeCountry = CountryCode;
						cusCode.OK_CodeType = NumberType;
						cusCode.OK_CustomsRegNo = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateNumber();
					}

					if (!NumberInfo.HasErrors())
					{
						Organization?.FindDuplicates();
					}
				}
				NumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NumberInfo
		{
			get { return GetZPropertyInfo(nameof(Number), "Registration Number"); }
		}

		public bool Number_ReadOnly => CusCode?.IsRestricted ?? false;

		[MaxLength(OrgCusCode.Schema.OK_CustomsRegNoMaxLength)]
		public ZString NumberForDisplay
		{
			get
			{
				var cusCode = CusCode;
				return (cusCode == null) ? Number : cusCode.SecuredCustomsRegNo;
			}
			set
			{
				Number = value;
				NumberForDisplayInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NumberForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(NumberForDisplay), x => NumberInfo); }
		}

		public bool NumberForDisplay_ReadOnly => !OrgCusCode.AllowViewSocialSecurityNumber(NumberType) || Number_ReadOnly;

		public ZString NumberType
		{
			get { return Lookups.NumberTypes.GetActualCode(NumberTypeForDisplay); }
		}

		public ZString NumberTypeDescription
		{
			get
			{
				ZString result = Lookups.NumberTypes.GetDescriptionFromCode(NumberTypeForDisplay);
				int openBracketIndex = result.IndexOf('(');
				int closeBraketIndex = result.LastIndexOf(')');
				if (openBracketIndex >= 0 && closeBraketIndex > openBracketIndex)
				{
					result = result.Remove(openBracketIndex, closeBraketIndex - openBracketIndex + 1).TrimEnd();
				}
				return result;
			}
		}

		public ZPropertyInfo NumberTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(NumberTypeDescription)); }
		}

		[List("Lookups.NumberTypes")]
		[MaxLength(6)]
		public ZString NumberTypeForDisplay
		{
			get
			{
				OrgCusCode cusCode = GetPrimaryCusCodeForOrganization();
				ZString calculatedRegistrationNumberType = (cusCode == null) ? (ZString)GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType : cusCode.OK_CodeType;
				if (!lastCalculatedNumberType.HasValue || (lastCalculatedNumberType.Value != calculatedRegistrationNumberType))
				{
					RefCountry country = (cusCode == null) ? GlbCompany.CurrentCompany.Country : cusCode.CodeCountry;
					numberTypeForDisplay = Lookups.NumberTypes.GetDisplayCode(calculatedRegistrationNumberType, country);
				}
				lastCalculatedNumberType = calculatedRegistrationNumberType;
				return numberTypeForDisplay;
			}
			set
			{
				CheckMaximumLength(NumberTypeForDisplayInfo, value);
				SetNonPersistentPropertyValue(NumberTypeForDisplayInfo, ref numberTypeForDisplay, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNumberTypeForDisplay();
				}
			}
		}

		public ZPropertyInfo NumberTypeForDisplayInfo
		{
			get { return GetZPropertyInfo(nameof(NumberTypeForDisplay), "Registration Number Type"); }
		}

		public OrgRegistrationNumberValidation Validation
		{
			get { return new OrgRegistrationNumberValidation(this); }
		}

		OrgCusCode GetCusCode(string numberType, ZString countryCode)
		{
			ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, numberType);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			BusinessObject[] cusCodes = Organization.CustomsCodes.Find(query);
			return (cusCodes.Length > 0) ? (OrgCusCode)cusCodes[0] : null;
		}

		OrgCusCode GetPrimaryCusCodeForCountry(RefCountry country)
		{
			foreach (string numberType in OrgRegistrationNumberTypeList.GetApplicableNumberTypes(country))
			{
				OrgCusCode cusCode = GetCusCode(numberType, country.Code);
				if (cusCode != null)
				{
					return cusCode;
				}
			}
			return null;
		}

		OrgCusCode GetPrimaryCusCodeForOrganization()
		{
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			OrgCusCode result = GetPrimaryCusCodeForCountry(currentCountry);
			if (result == null)
			{
				RefCountry orgCountry = Organization.Country;
				if ((orgCountry != null) && (orgCountry.PK != currentCountry.PK))
				{
					result = GetPrimaryCusCodeForCountry(orgCountry);
				}
			}
			return result;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (Organization.IsInDatabase && Organization.GetFailingCheckpointWhenModifyRegistrationNumber(true) != null)
				|| (!Organization.IsInDatabase && !Organization.SecurityProvider.HasNewConfigModifyFinancialRegistrationNosSecurity)
				|| MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
