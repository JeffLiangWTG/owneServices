using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty("DisplayText"), DescriptionProperty("AddressDescription")]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgTranslatedAddress : AutoOrgTranslatedAddress, ISupportWebAddressValidation
	{
		public OrgTranslatedAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("LanguageList")]
		public override ZString OTA_Language
		{
			get { return base.OTA_Language; }
			set
			{
				var oldValue = base.OTA_Language;
				base.OTA_Language = value;
				if (ParentAddress.LocalAddressDisplayText.EndsWith(oldValue, StringComparison.CurrentCulture))
				{
					ParentAddress.LocalAddressDisplayText = ConstructDisplayText(value);
				}

				if (oldValue != base.OTA_Language && TranslatedAdditionalInfoCollection != null)
				{
					var targetTranslatedAdditionalInfo = TranslatedAdditionalInfoCollection.Where(o =>
						o.OTI_AdditionalInfo == base.OTA_AdditionalAddressInformation
						&& o.OTI_Language == oldValue).ToList();

					if (string.IsNullOrWhiteSpace(OTA_AdditionalAddressInformation))
					{
						targetTranslatedAdditionalInfo.DeleteAll();
					}
					else
					{
						if (targetTranslatedAdditionalInfo.Any())
						{
							targetTranslatedAdditionalInfo.ForEach(t => t.OTI_Language = base.OTA_Language);
						}
						else if (!TranslatedAdditionalInfoCollection.Any(t => t.OTI_Language == OTA_Language && t.OTI_AdditionalInfo == OTA_AdditionalAddressInformation))
						{
							var newTranslatedAdditionalInfo = TranslatedAdditionalInfoCollection.AddNew();
							newTranslatedAdditionalInfo.OTI_Language = OTA_Language;
							newTranslatedAdditionalInfo.OTI_AdditionalInfo = OTA_AdditionalAddressInformation;
						}
					}
				}

				//Revalidate fields whose validation is language dependent
				if (!IsValidationSuspended)
				{
					Validation.ValidateOTA_CompanyName();
					Validation.ValidateOTA_Address1();
					Validation.ValidateOTA_Address2();
					Validation.ValidateOTA_City();
					Validation.ValidateOTA_State();
				}
			}
		}

		public override ZString OTA_AdditionalAddressInformation
		{
			get { return base.OTA_AdditionalAddressInformation; }
			set
			{
				var oldValue = base.OTA_AdditionalAddressInformation;
				base.OTA_AdditionalAddressInformation = value;
				unrestrictedAdditionalAddressInformation = value;
				if (oldValue != base.OTA_AdditionalAddressInformation && TranslatedAdditionalInfoCollection != null)
				{
					var targetTranslatedAdditionalInfo = TranslatedAdditionalInfoCollection.Where(o =>
						o.OTI_AdditionalInfo == oldValue
						&& o.OTI_Language == base.OTA_Language).ToList();

					if (string.IsNullOrWhiteSpace(base.OTA_AdditionalAddressInformation))
					{
						targetTranslatedAdditionalInfo.DeleteAll();
					}
					else
					{
						if (targetTranslatedAdditionalInfo.Any())
						{
							targetTranslatedAdditionalInfo.ForEach(t => t.OTI_AdditionalInfo = base.OTA_AdditionalAddressInformation);
						}
						else if (!TranslatedAdditionalInfoCollection.Any(t => t.OTI_Language == OTA_Language && t.OTI_AdditionalInfo == OTA_AdditionalAddressInformation))
						{
							var newTranslatedAdditionalInfo = TranslatedAdditionalInfoCollection.AddNew();
							newTranslatedAdditionalInfo.OTI_Language = OTA_Language;
							newTranslatedAdditionalInfo.OTI_AdditionalInfo = OTA_AdditionalAddressInformation;
						}
					}
				}
			}
		}

		OrgTranslatedAddressAdditionalInfoCollection TranslatedAdditionalInfoCollection => Address.AdditionalInfos.FirstOrDefault(a => a.OAI_IsPrimary)?.TranslatedInfos;

		#region Translated Additional Address Information Wrapper

		[ChildEditable(true)]
		public OrgTranslatedAddressAdditionalInfoWrapperCollection AdditionalInfoWrapperCollection
		{
			get
			{
				if (additionalInfoWrapperCollection == null)
				{
					additionalInfoWrapperCollection = new OrgTranslatedAddressAdditionalInfoWrapperCollection(this);
					RegisterEditableChildObject(additionalInfoWrapperCollection);
				}
				else
				{
					additionalInfoWrapperCollection.ReBuild();
				}
				return additionalInfoWrapperCollection;
			}
		}

		OrgTranslatedAddressAdditionalInfoWrapperCollection additionalInfoWrapperCollection;

		#endregion

		public override void Delete()
		{
			additionalInfoWrapperCollection?.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			// if save failed and this OrgTranslatedAddress has been deleted in other factory or client, ParentAddress.AddressLanguagePack need to be refreshed
			if (!saveSucceeded)
			{
				var addressFactory = new BusinessObjectFactory();
				if (addressFactory.Load<OrgTranslatedAddress>(this.PK) == null)
				{
					ParentAddress.DeleteTranslatedAddress(this);
				}
			}
		}

		public override ZString OTA_Address1
		{
			get { return base.OTA_Address1; }
			set
			{
				if (base.OTA_Address1 != value)
				{
					base.OTA_Address1 = value;
					ResetValidationStatus(OTA_Address1Info);
				}
			}
		}

		public override ZString OTA_Address2
		{
			get { return base.OTA_Address2; }
			set
			{
				if (base.OTA_Address2 != value)
				{
					base.OTA_Address2 = value;
					ResetValidationStatus(OTA_Address2Info);
				}
			}
		}

		public override ZString OTA_City
		{
			get { return base.OTA_City; }
			set
			{
				if (base.OTA_City != value)
				{
					base.OTA_City = value;
					ResetValidationStatus(OTA_CityInfo);
				}
			}
		}

		public override ZString OTA_PostCode
		{
			get { return base.OTA_PostCode; }
			set
			{
				if (base.OTA_PostCode != value)
				{
					base.OTA_PostCode = value;
					ResetValidationStatus(OTA_PostCodeInfo);
				}
			}
		}

		[List("StateCodeList")]
		public override ZString OTA_State
		{
			get { return base.OTA_State; }
			set
			{
				if (OTA_State != value)
				{
					base.OTA_State = value;
					ResetValidationStatus(OTA_StateInfo);
				}
			}
		}

		public override ZString OTA_ValidationStatus
		{
			get { return base.OTA_ValidationStatus; }
			set
			{
				base.OTA_ValidationStatus = value;
				RaiseAddressValidationStatusChanged();
				isManuallyVerifiedByUser = value == AddressValidationStatus.ManuallyVerified;
			}
		}

		public CodeDescriptionPairList LanguageList
		{
			get
			{
				return Factory.GetCachedValue("LanguageList",
					delegate
					{ return new CodeDescriptionPairList(OLookUpEditType.Language); });
			}
		}

		public ZString LanguageDescription => LanguageList.GetDescriptionFromCode(Language);

		public OrgAddress ParentAddress
		{
			get
			{
				return Factory.Load<OrgAddress>(OTA_OA);
			}
		}

		public bool IsEnglish
		{
			get { return Res.IsEnglish(OTA_Language); }
		}

		public bool IsEnglishOnlyOrEmpty
		{
			get { return OTA_Address1.IsEnglishOnlyOrEmpty && OTA_Address2.IsEnglishOnlyOrEmpty && OTA_City.IsEnglishOnlyOrEmpty; }
		}

		#region AddressDescription

		public ZString AddressDescription
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				if (!OTA_City.IsEmpty)
				{
					sb.Append(OTA_City);
					sb.Append(' ');
				}
				if (!OTA_State.IsEmpty)
				{
					sb.Append(OTA_State);
					sb.Append(' ');
				}
				if (!ParentAddress.OA_RL_NKRelatedPortCode.IsEmpty)
				{
					sb.Append(ParentAddress.OA_RL_NKRelatedPortCode);
					sb.Append(' ');
				}
				if (!OTA_Address1.IsEmpty)
				{
					sb.Append(OTA_Address1);
				}
				return sb.ToString().Trim();
			}
		}

		public ZPropertyInfo AddressDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(AddressDescription)); }
		}
		#endregion

		#endregion

		#region ISupportWebAddressValidation

		public ZPropertyInfo PostcodeInfo
		{
			get { return OTA_PostCodeInfo; }
		}

		[List("LanguageList")]
		public ZString Language
		{
			get { return OTA_Language; }
			set
			{
				OTA_Language = value;
			}
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return OTA_LanguageInfo; }
		}

		[List("AdditionalAddressInfoList")]
		[DocumentFieldExcludeFromMap]
		public ZString UnrestrictedAdditionalAddressInformation
		{
			get { return unrestrictedAdditionalAddressInformation; }

			set
			{
				value = value.TrimEndSpaceTab();
				OTA_AdditionalAddressInformation = value;
				unrestrictedAdditionalAddressInformation = value;
			}
		}

		public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo
		{
			get { return OTA_AdditionalAddressInformationInfo; }
		}

		ZString unrestrictedAdditionalAddressInformation;

		public CodeDescriptionPairList AdditionalAddressInfoList => new CodeDescriptionPairList();

		public ZString AddressCode { get; set; }

		public ZString Address1
		{
			get { return OTA_Address1; }
			set
			{
				OTA_Address1 = value;
			}
		}

		public ZPropertyInfo Address1Info
		{
			get { return OTA_Address1Info; }
		}

		public ZString Address2
		{
			get { return OTA_Address2; }
			set { OTA_Address2 = value; }
		}

		public ZPropertyInfo Address2Info
		{
			get { return OTA_Address2Info; }
		}

		public ZString City
		{
			get { return OTA_City; }
			set { OTA_City = value; }
		}

		public ZPropertyInfo CityInfo
		{
			get { return OTA_CityInfo; }
		}

		public ZString CompanyName
		{
			get { return OTA_CompanyName; }
			set { OTA_CompanyName = value; }
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return OTA_CompanyNameInfo; }
		}

		public int Language_MaxLength
		{
			get { return Schema.OTA_LanguageMaxLength; }
		}

		public int Address1_MaxLength
		{
			get { return Schema.OTA_Address1MaxLength; }
		}

		public int Address2_MaxLength
		{
			get { return Schema.OTA_Address2MaxLength; }
		}

		public int City_MaxLength
		{
			get { return Schema.OTA_CityMaxLength; }
		}

		public int Postcode_MaxLength
		{
			get { return Schema.OTA_PostCodeMaxLength; }
		}

		public int CompanyName_MaxLength
		{
			get { return Schema.OTA_CompanyNameMaxLength; }
		}

		public ZString DisplayText
		{
			get { return ConstructDisplayText(OTA_Language); }
			set
			{
				OTA_Language = value.Replace(DisplayTextHeader, string.Empty);
			}
		}

		string DisplayTextHeader
		{
			get { return Res.GetString("5D88C191-D405-40DC-A68D-CF422BCBD64C", "TRANSLATED:"); }
		}

		string ConstructDisplayText(string languageCode)
		{
			return DisplayTextHeader + languageCode;
		}

		public ZGuid EntityPK
		{
			get { return PK; }
		}

		public ZString AddressMap
		{
			get { return OTA_AddressMap; }
			set { OTA_AddressMap = value; }
		}

		public ZString AddressRecordGUID
		{
			get { return PK.ToString(); }
		}

		public ZString AddressSourceTable
		{
			get { return OrgTranslatedAddressSchema.Constants.Prefix; }
		}

		[BusinessObjectTestExclude]
		public ZString State
		{
			get
			{
				if (string.IsNullOrWhiteSpace(StateCodeList.GetDescriptionFromCode(OTA_State)))
				{
					return OTA_State;
				}
				return StateCodeList.GetDescriptionFromCode(OTA_State);
			}
			set
			{
				var code = (ZString)StateCodeList.GetCodeFromDescription(value);
				OTA_State = string.IsNullOrEmpty(code) ? value : code;
				StateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		public int State_MaxLength
		{
			get { return Schema.OTA_StateMaxLength; }
		}

		public ZString StateCode
		{
			get { return OTA_State; }
			set
			{
				OTA_State = value;
				StateCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StateCode)); }
		}

		public int StateCode_MaxLength
		{
			get { return Schema.OTA_StateMaxLength; }
		}

		public CodeDescriptionPairList StateCodeList
		{
			get { return ParentAddress.StateCodeList; }
		}

		public ZString Postcode
		{
			get { return OTA_PostCode; }
			set { OTA_PostCode = value; }
		}

		public ZString CountryCodeISO2
		{
			get { return ParentAddress.OA_RN_NKCountryCode; }
			set { ParentAddress.OA_RN_NKCountryCode = value; }
		}

		public int CountryCodeISO2_MaxLength => ParentAddress.CountryCodeISO2_MaxLength;

		public RefCountry Country
		{
			get { return ParentAddress.Country; }
		}

		public RefCountryCollection CountryCodeList
		{
			get { return ParentAddress.CountryCodeList; }
		}

		public ZString ValidationStatus
		{
			get
			{
				return OTA_ValidationStatus;
			}
			set
			{
				OTA_ValidationStatus = value;
				RaiseAddressValidationStatusChanged();
			}
		}

		public ZString Addressee
		{
			get { return OTA_CompanyName; }
		}

		public ZGeography GeoLocation
		{
			get => ParentAddress.OA_GeoLocation;
			set => ParentAddress.OA_GeoLocation = value;
		}

		public ZString ClosestPort { get; set; }

		bool isManuallyVerifiedByUser;

		public AddressValidationSection ValidationSection { get; } = AddressValidationSection.OrganizationAddress;

		public void ResetValidationStatus(ZPropertyInfo propertyInfo)
		{
			if (isManuallyVerifiedByUser)
			{
				return;
			}

			if (ValidationStatus != AddressValidationStatus.CountryNotAvailable && Country != null && OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Country.PK.ToGuid(), ValidationSection))
			{
				OTA_ValidationStatus = AddressValidationStatus.ToBeVerified;
				OTA_AddressMap = "";
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				if (!string.IsNullOrEmpty(registrationKey.SystemId))
				{
					RaiseWebServices(propertyInfo);
				}
			}
		}

		public async Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, WTG.AddressCleansing.Common.CleanseAction cleanseAction = WTG.AddressCleansing.Common.CleanseAction.ValidateAndSuggest)
		{
			return await AddressValidationService.ValidateAddressAsync(this, cancellationToken, false, cleanseAction);
		}

		public async Task<WTG.AddressCleansing.Common.CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken)
		{
			return await AddressValidationService.GetCityTownAsync(this, cancellationToken);
		}

		public event EventHandler AddressValidationStatusChanged;
		public bool IsErrorSuppressed => false;
		public bool IsJobDocAddress => false;

		public event EventHandler TriggerWebAddressValidation;
		public event EventHandler TriggerWebGetCityTown;

		public void ClearWebAddressValidationHandler()
		{
			if (TriggerWebAddressValidation != null)
			{
				foreach (EventHandler item in TriggerWebAddressValidation.GetInvocationList())
				{
					TriggerWebAddressValidation -= item;
				}
			}
		}

		public void ClearWebGetCityTownHandler()
		{
			if (TriggerWebGetCityTown != null)
			{
				foreach (EventHandler item in TriggerWebGetCityTown.GetInvocationList())
				{
					TriggerWebGetCityTown -= item;
				}
			}
		}

		void RaiseWebServices(ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(CountryCodeISO2))
			{
				if ((string.IsNullOrEmpty(OTA_City) || string.IsNullOrEmpty(OTA_State) || string.IsNullOrEmpty(OTA_PostCode)) && (propertyInfo == OTA_CityInfo || propertyInfo == OTA_PostCodeInfo))
				{
					RaiseTriggerWebGetCityTown(propertyInfo);
				}
				else
				{
					RaiseTriggerWebAddressValidation(propertyInfo);
				}
			}
		}

		void RaiseAddressValidationStatusChanged()
		{
			if (AddressValidationStatusChanged != null)
			{
				AddressValidationStatusChanged(this, EventArgs.Empty);
			}
		}

		void RaiseTriggerWebAddressValidation(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebAddressValidation != null)
			{
				TriggerWebAddressValidation(this, new InfoEventArgs(propertyInfo));
			}
		}

		void RaiseTriggerWebGetCityTown(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebGetCityTown != null)
			{
				TriggerWebGetCityTown(this, new InfoEventArgs(propertyInfo));
			}
		}

		public bool NeedValidation
		{
			get
			{
				var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, ParentAddress.OA_RN_NKCountryCode);
				if (country != null)
				{
					if (!OTA_Address1.IsEmpty && !OTA_PostCode.IsEmpty && !OTA_City.IsEmpty && !OTA_State.IsEmpty)
					{
						if (!IsInDatabase || (OTA_Address1Info.HasChanges || OTA_Address2Info.HasChanges || OTA_PostCodeInfo.HasChanges ||
												OTA_CityInfo.HasChanges || OTA_StateInfo.HasChanges || ParentAddress.OA_RN_NKCountryCodeInfo.HasChanges))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public void PreValidationForAddressValidationService()
		{
			Validation.ValidateOTA_Address1();
			Validation.ValidateOTA_City();
			ValidatePostcodeAndStateForAddress();
			ParentAddress.Validation.ValidateOA_RN_NKCountryCode();
		}

		public void ValidatePostcodeAndStateForAddress()
		{
			Validation.ValidateOTA_PostCode();
			Validation.ValidateOTA_State();
		}

		public bool IsUpdatingCityTown { get; set; }
		public bool IsValidatingAddress { get; set; }
		public bool IsExactPointFound { get; set; }
		public bool IsValidatedByBackgroundService { get; set; }
		public bool IsTSAKnownAddress => ParentAddress.IsTSAKnownAddress;
		public bool IsMIDAddress => ParentAddress.IsMIDAddress;

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		bool ISupportWebAddressValidation.GetReadOnlySecurity(PropertyDescriptor property)
		{
			return GetReadOnlySecurity(property);
		}

		#endregion

		#region Test Data

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (string.IsNullOrEmpty(OTA_Address1))
			{
				OTA_Address1 = "#1";
			}
		}

#endif

		#endregion
	}
}
