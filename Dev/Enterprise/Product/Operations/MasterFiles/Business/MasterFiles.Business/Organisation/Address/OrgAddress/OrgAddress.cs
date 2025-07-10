using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Core.Environment;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ICodeDescription = CargoWise.Integration.ICodeDescription;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.MasterFiles.Business
{
	[SystemDefinedValues]
	[CodeProperty(OrgAddress.Schema.OA_Code), DescriptionProperty("CityStateCountryDescription")]
	[SingleObjectAroundARow]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = "FinishUniversalCopy")]
	public class OrgAddress : AutoOrgAddress, IOrgAddress, IDocAddress, IAddressDetails, IContactable, IMatchingAddress, ISupportDataImporting, ISupportWebAddressValidation, ILocation, IEInvoicingEligibilityLiteOrgAddress, IUSOrgAddress
	{
		public new class Schema : AutoOrgAddress.Schema
		{
			public const string CityStateCountryDescription = "CityStateCountryDescription";
			public const string OA_Fax_Formatted = "OA_Fax_Formatted";
			public const string OA_Fax_FormattedLocalNumberIfLoggedInSameCountry = "OA_Fax_FormattedLocalNumberIfLoggedInSameCountry";
			public const string OA_Fax_IsManuallyVerified = "OA_Fax_IsManuallyVerified";
			public const string OA_Mobile_Formatted = "OA_Mobile_Formatted";
			public const string OA_Mobile_FormattedLocalNumberIfLoggedInSameCountry = "OA_Mobile_FormattedLocalNumberIfLoggedInSameCountry";
			public const string OA_Mobile_IsManuallyVerified = "OA_Mobile_IsManuallyVerified";
			public const string OA_Phone_Formatted = "OA_Phone_Formatted";
			public const string OA_Phone_FormattedLocalNumberIfLoggedInSameCountry = "OA_Phone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string OA_Phone_IsManuallyVerified = "OA_Phone_IsManuallyVerified";
			public const int OA_CompanyNameOverrideTruncatedLength = 50;
			public const int OA_Address1MinimumLength = 4;
			public const string IsOverridenJobLoadingDuration = "IsOverridenJobLoadingDuration";
		}

		public OrgAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());
			phoneNumberPropertyHelperThunk = new Lazy<PhoneNumberPropertyHelper>(() => new PhoneNumberPropertyHelper(() => !DefaultCountryCodeForPhoneNumbers.IsEmpty ? DefaultCountryCodeForPhoneNumbers : (Header != null ? Header.CountryCode : ZString.Empty)));
		}

		public static OrgAddress New(BusinessObjectFactory factory)
		{
			return factory.New<OrgAddress>();
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			#region GetWarehouseAddressBasedOnCusCode

			public OrgAddress GetWarehouseAddressBasedOnCusCode(string cCPExternalCode)
			{
				OrgAddress result = null;
				var cusCodeFilter = GetRegistrationFilter(OrgCusCode.CodeTypes.ControlledPremisesID, cCPExternalCode);
				var cCPCode = Factory.LoadTop1<OrgCusCode>(cusCodeFilter);

				if (cCPCode != null)
				{
					if (!cCPCode.OK_OA_PremisesAddress.IsEmpty)
					{
						result = cCPCode.PremisesAddress;
					}
					else
					{
						var org = cCPCode.Header;
						result = org.Addresses.DefaultAddressOfType(OrgAddressType.Office);
					}
				}

				return result;
			}

			#endregion

			#region LoadAddressFromLocalCode

			public OrgAddress LoadAddressFromLocalCode(ZString codeType, ZString code)
			{
				var cusCodeFilter = GetRegistrationFilter(codeType, code);
				cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				var cusCode = Factory.LoadTop1<OrgCusCode>(cusCodeFilter);
				return cusCode?.PremisesAddress;
			}

			#endregion

			#region FromLocalPremiseID

			public OrgAddress FromLocalPremiseID(ZString code)
			{
				OrgAddress result = null;
				var filter = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, code);
				filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				filter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ControlledPremisesID);
				var orgCusCodes = Factory.Load<OrgCusCode>(filter);
				if (orgCusCodes.Length >= 1)
				{
					var cusCode = orgCusCodes[0];
					if (cusCode.PremisesAddress != null)
					{
						result = cusCode.PremisesAddress;
					}
					else if (cusCode.Lookups.PremisesAddresses.Count > 0)
					{
						if (cusCode.Header != null)
						{
							result = cusCode.Header.Addresses.DefaultAddressOfType(OrgAddressType.Office);
						}
					}
				}
				return result;
			}

			#endregion

			#region LoadFromCustomsRegisteredCodes

			public OrgAddress[] LoadDBAddresses(ZString countryCode, ZString codeType, ZString regoNumber)
			{
				var subquery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OA_PremisesAddress);
				subquery.AddToFilter(OrgCusCode.Loader.GetQuery(countryCode, codeType, regoNumber));
				var query = new ZDBOnlyQuery(typeof(OrgAddress));
				query.AddSubQuery(subquery, JoinCondition.And);

				return Factory.Load<OrgAddress>(query);
			}

			#endregion

			#region Implementation

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(OrgAddress);
			}

			ZQuery GetRegistrationFilter(ZString codeType, ZString customsRegNo)
			{
				ZQuery result = new ZQuery(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				result.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
				result.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, customsRegNo);
				return result;
			}

			#endregion
		}

		#endregion

		#region Test Data

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (propertyPath.Length > 0)
			{
				kind = (kind & ~TestBusinessObjectKind.PopulateRelationsDeeply & ~TestBusinessObjectKind.PopulateRelatedObjects);
			}
			base.FillWithValidTestDataCore(kind, propertyPath);

			var propertyPathAsString = GetPropertyPathAsString(propertyPath);
			var addressIndex = -1;
			if (Header != null)
			{
				for (int i = 0; i < Header.Addresses.Count; i++)
				{
					var address = Header.Addresses[i];
					if (address.PK == PK)
					{
						addressIndex = (i + 1);
						break;
					}
				}
			}
			var address1 = new ZStringBuilder();
			if (addressIndex != -1)
			{
				address1.Append("#" + addressIndex);
			}
			if (!string.IsNullOrEmpty(propertyPathAsString))
			{
				address1.Append(" Path=" + propertyPathAsString);
			}
			OA_Address1 = (new ZString(address1.ToString().Trim())).SubstringSafe(0, OA_Address1Info.MaxLength);
			base.OA_RN_NKCountryCode = ZString.Empty;

			if (OA_OH.IsEmpty)
			{
				var populatedWithTestData = Factory.New<OrgHeader>();
				populatedWithTestData.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave, propertyPath);
				OA_OH = populatedWithTestData.PK;
				populatedWithTestData.OH_FullName = "";
			}
		}

		string GetPropertyPathAsString(PropertyDescriptor[] propertyPath)
		{
			var result = new ZStringBuilder();
			if (propertyPath.Length > 1)
			{
				foreach (var property in propertyPath)
				{
					result.Append(property.Name);
				}
			}
			return result.ToStringWithDelimiterBetweenAppends(".");
		}

#endif

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString convertedLog = OA_Code.ConvertToWesternEuropeanCharacters();
				if (convertedLog != OA_Code)
				{
					convertedLog = AddressNotInWesternEuropeanCodePageLogMessage +
						convertedLog.Replace("?", "").Substring(0, StmALogSchema.SL_Reference.MaxLength - AutoLogPrefix.Length - AddressNotInWesternEuropeanCodePageLogMessage.Length);
				}
				return AutoLogPrefix + convertedLog;
			}
		}

		#endregion

		#region Constants

		internal static string CannotUntickDefaultOfficeAddressCaption
		{
			get { return Res.GetString("46c18128-0487-4b1f-9f01-4e18391b3bb8", "Cannot Change Default Office Address"); }
		}

		internal static string NoAccessToUntickDefaultMessage
		{
			get { return Res.GetString("58050124-6ae1-4e27-949c-cf7fa146d08b", "You do not have security rights to change the Main Address of this organization."); }
		}

		internal static string CannotUntickDefaultOfficeAddressText
		{
			get { return Res.GetString("cf749daf-e669-4f9c-9989-c28e0fa77e37", "You cannot make the Main Office Address non-default.\r\nTo change the Main Office Address, you should select the Default checkbox on another address."); }
		}

		static string AddressNotInWesternEuropeanCodePageLogMessage
		{
			get { return Res.GetString("6ac11d0b-8b94-43b0-933a-e88fdcbb7b34", "(Non Western-European Characters Removed)") + " "; }
		}

		static string AutoLogPrefix
		{
			get { return Res.GetString("52e5f53f-4345-4f05-84b2-47fccc701ebb", "Address") + " "; }
		}

		public static string AddressNotOnFile
		{
			get { return Res.GetString("757cea21-bb2e-4873-8b37-8cce12ab9b96", "***Address Not On File***"); }
		}

		#endregion

		#region Pre Fetch

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgAddressFetchStrategy(this);
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			SettingDefaults = true;
			try
			{
				base.SetDefaultValues();
				IsUpdatingCityTown = false;
				IsValidatingAddress = false;

				if (!Factory.IsConstructingNullBusinessObject)
				{
					base.OA_RN_NKCountryCode = EffectiveRelatedPortCode?.Country?.Code ?? Env.CurrentCompany.Country.Code;

					SetDefaultLanguage();
					OA_FCLEquipmentNeeded = OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value;
					OA_LCLEquipmentNeeded = OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.Value;
					OA_AIREquipmentNeeded = OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.Value;
				}
			}
			finally
			{
				SettingDefaults = false;
			}
		}

		public bool SettingDefaults;

		#endregion

		#region Lookups

		#region UNLOCOs

		public RefUNLOCOCollection RefUNLOCOs
		{
			get
			{
				if (fRefUNLOCOs == null)
				{
					fRefUNLOCOs = new RefUNLOCOCollection(Factory);
				}
				return fRefUNLOCOs;
			}
		}

		RefUNLOCOCollection fRefUNLOCOs;

		#endregion

		#region Sales Categories

		public ReadOnlyCodeDescriptionPairList OM_CMSalesCategory_List
		{
			get
			{
				if (fOM_CMSalesCategory_List == null)
				{
					fOM_CMSalesCategory_List = Env.Registry.SalesCategoryList;
				}
				return fOM_CMSalesCategory_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fOM_CMSalesCategory_List;

		#endregion

		#region Access Points

		public ReadOnlyCodeDescriptionPairList OA_AccessPoint_List
		{
			get
			{
				if (fOA_AccessPoint_List == null)
				{
					fOA_AccessPoint_List = Env.Registry.AddressAccessPointList;
				}
				return fOA_AccessPoint_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fOA_AccessPoint_List;

		#endregion

		#region Address Types

		#region Communication Required

		public ReadOnlyCodeDescriptionPairList OA_CommunicationRequired_List
		{
			get
			{
				if (fOA_CommunicationRequired_List == null)
				{
					fOA_CommunicationRequired_List = Env.Registry.AddressCommunicationRequiredList;
				}
				return fOA_CommunicationRequired_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fOA_CommunicationRequired_List;

		#endregion

		#region Container Handling

		public ReadOnlyCodeDescriptionPairList OA_ContainerHandling_List
		{
			get
			{
				if (fOA_ContainerHandling_List == null)
				{
					fOA_ContainerHandling_List = Env.Registry.AddressContainerHandlingList;
				}
				return fOA_ContainerHandling_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fOA_ContainerHandling_List;

		#endregion

		#region Dock Heights

		public ReadOnlyCodeDescriptionPairList OA_Dock_Height_List
		{
			get
			{
				if (fOA_Dock_Height_List == null)
				{
					fOA_Dock_Height_List = Env.Registry.AddressDockHeightList;
				}
				return fOA_Dock_Height_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fOA_Dock_Height_List;

		#endregion

		#region Labour Required

		public ReadOnlyCodeDescriptionPairList OA_LabourRequired_List
		{
			get
			{
				if (fOA_LabourRequired_List == null)
				{
					fOA_LabourRequired_List = Env.Registry.AddressLabourRequiredList;
				}
				return fOA_LabourRequired_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fOA_LabourRequired_List;

		#endregion

		#region Delivery Route

		public ReadOnlyCodeDescriptionPairList DeliveryRoutesList
		{
			get
			{
				if (deliveryRoutesList == null)
				{
					deliveryRoutesList = Env.Registry.DeliveryRoutesListSorted;
				}
				return deliveryRoutesList;
			}
		}

		ReadOnlyCodeDescriptionPairList deliveryRoutesList;

		#endregion

		#region Languages

		public CodeDescriptionPairList LanguageList
		{
			get
			{
				return Factory.GetCachedValue("LanguageList",
					delegate
					{ return new CodeDescriptionPairList(OLookUpEditType.Language); });
			}
		}

		#endregion

		#region States

		public CodeDescriptionPairList OA_State_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				if (Header != null && Header.UNLOCO != null)
				{
					list = new OrgCodeLists().State_List(Header.UNLOCO);
				}
				return list;
			}
		}

		public CodeDescriptionPairList OA_State_List_For_Current_Address
		{
			get { return (Country != null) ? new OrgCodeLists().State_List(Country) : OA_State_List; }
		}

		public ZString ValidateState()
		{
			var result = ZString.Empty;
			var codeOrDesc = OA_State;
			var countryCode = Country != null ? Country.RN_Code : ZString.Empty;
			result = ValidateState(Factory, codeOrDesc, countryCode);
			return result;
		}

		public static ZString ValidateState(BusinessObjectFactory factory, ZString stateCodeOrDesc, ZString countryCode)
		{
			var result = ZString.Empty;
			if (!countryCode.IsEmpty && !stateCodeOrDesc.IsEmpty)
			{
				var relatedState = new RefCountryStates.Loader(factory).LoadRefCountryStatesFromCodeOrDesc(stateCodeOrDesc, countryCode);
				if (relatedState == null)
				{
					result = ResString.GetMultilingualString("E87B80A1-1677-4073-9906-B5AF4ADA6B28", "The state is not a valid {0} state.", countryCode);
				}
			}
			return result;
		}

		#endregion

		#endregion

		[ChildEditable(true)]
		public GenCustomAddOnRuleAckCollection AddOnRuleAcks
		{
			get
			{
				if (addOnRuleAcks == null)
				{
					addOnRuleAcks = new GenCustomAddOnRuleAckCollection(this);
					RegisterEditableChildObject(addOnRuleAcks);
				}

				return addOnRuleAcks;
			}
		}
		GenCustomAddOnRuleAckCollection addOnRuleAcks;

		#endregion

		#region Properties

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = new ZStringBuilder(base.HumanReadableNameCore);
				if (!IsDeleted && !OA_Code.IsEmpty)
				{
					result.Append(" (" + OA_Code + ")");
				}

				return result.ToString();
			}
		}

		#endregion

		#region StateCityCountryDescription

		public ZString CityStateCountryDescription
		{
			get
			{
				var sb = new ZStringBuilder();
				if (!OA_City.IsEmpty)
				{
					sb.Append(OA_City);
				}
				if (!OA_State.IsEmpty)
				{
					sb.Append(OA_State);
				}
				if (!OA_RL_NKRelatedPortCode.IsEmpty)
				{
					sb.Append(OA_RL_NKRelatedPortCode);
				}
				if (!OA_Address1.IsEmpty)
				{
					sb.Append(OA_Address1);
				}
				return sb.ToStringWithDelimiterBetweenAppends(" ").Trim();
			}
		}

		public ZPropertyInfo CityStateCountryDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CityStateCountryDescription); }
		}

		public ZString AddressDescription
		{
			get
			{
				if (OA_Address2.IsEmpty && OA_CompanyNameOverride.IsEmpty && OA_City.IsEmpty && OA_State.IsEmpty && OA_PostCode.IsEmpty)
				{
					return (OA_Address1.IsEmpty) ? (ZString)OrgAddress.AddressNotOnFile : OA_Address1;
				}
				else
				{
					var result = new ZStringBuilder(OA_Address2);

					if (!EffectiveCompanyNameTruncated.IsEmpty)
					{
						result.Append(EffectiveCompanyNameTruncated);
					}

					if (!CityFallback.IsEmpty)
					{
						result.Append(CityFallback);
					}

					if (!OA_State.IsEmpty)
					{
						result.Append(OA_State);
					}

					if (!OA_PostCode.IsEmpty)
					{
						result.Append(OA_PostCode);
					}

					if (!OA_RL_NKRelatedPortCode.IsEmpty)
					{
						result.Append(OA_RL_NKRelatedPortCode);
					}

					return result.ToStringWithDelimiterBetweenAppends(" ").Trim();
				}
			}
		}

		#endregion

		#region Header Closest Port

		public RefUNLOCO HeaderClosestPort
		{
			get { return Header != null ? Header.ClosestPort : null; }
		}

		#endregion

		#region OA_Code

		public override ZString OA_Code
		{
			get { return base.OA_Code; }
			set
			{
				base.OA_Code = value;
				RunAddressCodeValidationForOrg();
				if (Header != null)
				{
					Header.Addresses.MarkAsNeedingValidation();
				}
				LocalAddressDisplayText = value;
			}
		}

		void RunAddressCodeValidationForOrg()
		{
			if (Header != null)
			{
				foreach (OrgAddress address in Header.Addresses)
				{
					address.Validation.ValidateOA_Code();
				}
			}
		}

		#endregion

		#region OA_CommunicationRequired

		[List("OA_CommunicationRequired_List")]
		public override ZString OA_CommunicationRequired
		{
			get { return base.OA_CommunicationRequired; }
			set { base.OA_CommunicationRequired = value; }
		}

		#endregion

		#region OA_AdditionalAddressInformation

		public override ZString OA_AdditionalAddressInformation
		{
			get
			{
				return base.OA_AdditionalAddressInformation;
			}
			set
			{
				if (base.OA_AdditionalAddressInformation != value)
				{
					if (PrimaryOrgAddressAdditionalInfoCount > 1)
					{
						AdditionalInfos.ForEach(a => a.OAI_IsPrimary = false);
					}

					base.OA_AdditionalAddressInformation = value;
				}

				if (PrimaryOrgAddressAdditionalInfoDetail != value)
				{
					PrimaryOrgAddressAdditionalInfoDetail = value;
				}

				if (UnrestrictedAdditionalAddressInformation != value)
				{
					UnrestrictedAdditionalAddressInformation = value;
				}
			}
		}

		#endregion

		#region OA_Address1

		public override ZString OA_Address1
		{
			get { return base.OA_Address1; }
			set
			{
				if (base.OA_Address1 != value)
				{
					base.OA_Address1 = value;
					if (!IsInDatabase)
					{
						SetDefaultUsageComment();
					}
					if (Header != null)
					{
						Header.PatternMatchRequiresRegen = true;

						if (!Res.IsEnglish(Header.OH_Language) && !value.IsWesternEuropeanOrEmpty && Res.IsEnglish(OA_Language))
						{
							OA_Language = Header.OH_Language;
						}
					}
					ResetValidationStatus(OA_Address1Info);
				}
			}
		}

		#endregion

		#region OA_Address2

		public override ZString OA_Address2
		{
			get { return base.OA_Address2; }
			set
			{
				if (OA_Address2 != value)
				{
					base.OA_Address2 = value;
					if (Header != null)
					{
						Header.PatternMatchRequiresRegen = true;
					}

					ResetValidationStatus(OA_Address2Info);
				}
			}
		}

		#endregion

		#region OA_AccessPoint

		[List("OA_AccessPoint_List")]
		public override ZString OA_AccessPoint
		{
			get { return base.OA_AccessPoint; }
			set { base.OA_AccessPoint = value; }
		}

		#endregion

		#region OA_AIREquipmentNeeded

		[List("Lookups.CartageEquipmentNeededAir")]
		public override ZString OA_AIREquipmentNeeded
		{
			get { return base.OA_AIREquipmentNeeded; }
			set { base.OA_AIREquipmentNeeded = value; }
		}

		#endregion

		#region OA_AuthorityToLeave

		[List("Lookups.AuthorityToLeaveOptions")]
		public override ZString OA_AuthorityToLeave
		{
			get { return base.OA_AuthorityToLeave; }
			set { base.OA_AuthorityToLeave = value; }
		}

		#endregion

		#region OA_City

		[MaxLength(OrgAddress.Schema.OA_CityMaxLength)]
		public override ZString OA_City
		{
			get { return base.OA_City; }
			set
			{
				if (OA_City != value)
				{
					base.OA_City = value;
					if (Header != null)
					{
						Header.PatternMatchRequiresRegen = true;
					}

					ResetValidationStatus(OA_CityInfo);
					FindDuplicatesWhenAddressValidationDisabled(OA_City);
				}
			}
		}

		public override ZPropertyInfo OA_CityInfo
		{
			get { return GetZPropertyInfo(OrgAddressSchema.OA_City.Name); }
		}

		#endregion

		#region OA_PostCode

		public override ZString OA_PostCode
		{
			get { return base.OA_PostCode; }
			set
			{
				if (OA_PostCode != value)
				{
					base.OA_PostCode = value;
					if (Header != null)
					{
						Header.PatternMatchRequiresRegen = true;
					}

					ResetValidationStatus(OA_PostCodeInfo);
					FindDuplicatesWhenAddressValidationDisabled(OA_PostCode);
				}
			}
		}

		#endregion

		#region OA_State

		[List("OA_State_List_For_Current_Address")]
		public override ZString OA_State
		{
			get { return base.OA_State; }
			set
			{
				if (OA_State != value)
				{
					base.OA_State = value;

					if (Header != null)
					{
						Header.PatternMatchRequiresRegen = true;
					}

					ResetValidationStatus(OA_StateInfo);
					FindDuplicatesWhenAddressValidationDisabled(OA_State);
				}
			}
		}

		public RefCountryStates RelatedState
		{
			get
			{
				RefCountryStates result = null;

				var relatedCountry = RelatedCountry;
				if (relatedCountry != null)
				{
					var stateQuery = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, RelatedCountry.RN_Code);
					stateQuery.AddToFilter(RefCountryStatesSchema.RW_Code, OA_State);
					var states = Factory.Load<RefCountryStates>(stateQuery);
					if (states.Length == 1)
					{
						result = states[0];
					}
				}

				return result;
			}
		}

		#endregion

		#region OA_ContainerHandling

		[List("OA_ContainerHandling_List")]
		public override ZString OA_ContainerHandling
		{
			get { return base.OA_ContainerHandling; }
			set { base.OA_ContainerHandling = value; }
		}

		#endregion

		#region OA_Dock_Height

		[List("OA_Dock_Height_List")]
		public override ZString OA_Dock_Height
		{
			get { return base.OA_Dock_Height; }
			set { base.OA_Dock_Height = value; }
		}

		#endregion

		#region OA_Email

		[EmailAddress]
		public override ZString OA_Email
		{
			get { return base.OA_Email; }
			set
			{
				if (OA_Email != value)
				{
					base.OA_Email = value;
					if (Header != null)
					{
						Header.PatternMatchRequiresRegen = true;
					}

					Validation.ValidateOA_Email();
					Header?.FindDuplicates();
				}
			}
		}

		#endregion

		#region Phone Numbers

		#region OA_Phone

		public override ZString OA_Phone
		{
			get { return base.OA_Phone; }
			set
			{
				if (OA_Phone != value)
				{
					base.OA_Phone = value;
					if (Header != null)
					{
						Header.PatternMatchRequiresRegen = true;
					}
				}
			}
		}

		#endregion

		#region OA_Phone_Formatted

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString OA_Phone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(OA_PhoneInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(OA_PhoneInfo, OA_Phone_FormattedInfo, value, Validation.ValidateOA_Phone_Formatted, OA_Phone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo OA_Phone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.OA_Phone_Formatted); }
		}

		#endregion

		#region OA_Phone_IsManuallyVerified

		public ZBool OA_Phone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgAddressSchema.Constants.OA_Phone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OA_Phone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgAddressSchema.Constants.OA_Phone, Validation.ValidateOA_Phone_Formatted, PhoneNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo OA_Phone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OA_Phone_IsManuallyVerified); }
		}

		#endregion

		#region OA_Phone_FormattedLocalNumberIfLoggedInSameCountry

		public ZString OA_Phone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(OA_PhoneInfo); }
		}

		public ZPropertyInfo OA_Phone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.OA_Phone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		#endregion

		#region OA_Fax

		public override ZString OA_Fax
		{
			get { return base.OA_Fax; }
			set
			{
				if (OA_Fax != value)
				{
					base.OA_Fax = value;
					if (Header != null)
					{
						Header.PatternMatchRequiresRegen = true;
						Header.Contacts.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region OA_Fax_Formatted

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString OA_Fax_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(OA_FaxInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(OA_FaxInfo, OA_Fax_FormattedInfo, value, Validation.ValidateOA_Fax_Formatted, OA_Fax_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo OA_Fax_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.OA_Fax_Formatted); }
		}

		#endregion

		#region OA_Fax_IsManuallyVerified

		public ZBool OA_Fax_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgAddressSchema.Constants.OA_Fax, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OA_Fax_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgAddressSchema.Constants.OA_Fax, Validation.ValidateOA_Fax_Formatted, FaxNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo OA_Fax_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OA_Fax_IsManuallyVerified); }
		}

		#endregion

		#region OA_Fax_FormattedLocalNumberIfLoggedInSameCountry

		public ZString OA_Fax_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(OA_FaxInfo); }
		}

		public ZPropertyInfo OA_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.OA_Fax_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		#endregion

		#region OA_Mobile_Formatted

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString OA_Mobile_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(OA_MobileInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(OA_MobileInfo, OA_Mobile_FormattedInfo, value, Validation.ValidateOA_Mobile_Formatted, OA_Mobile_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo OA_Mobile_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.OA_Mobile_Formatted); }
		}

		#endregion

		#region OA_Mobile_IsManuallyVerified

		public ZBool OA_Mobile_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgAddressSchema.Constants.OA_Mobile, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OA_Mobile_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgAddressSchema.Constants.OA_Mobile, Validation.ValidateOA_Mobile_Formatted, MobilePhoneNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo OA_Mobile_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OA_Mobile_IsManuallyVerified); }
		}

		#endregion

		#region OA_Mobile_FormattedLocalNumberIfLoggedInSameCountry

		public ZString OA_Mobile_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(OA_MobileInfo); }
		}

		public ZPropertyInfo OA_Mobile_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.OA_Mobile_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		#endregion

		#region DefaultCountryCodeForPhoneNumbers

		public ZString DefaultCountryCodeForPhoneNumbers
		{
			get
			{
				var result = OA_RN_NKCountryCode;
				if (result.IsEmpty && RelatedPortCode != null)
				{
					result = RelatedPortCode.RL_RN_NKCountryCode;
				}
				return result;
			}
		}

		#endregion

		#region Implementations

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper
		{
			get { return phoneNumberPropertyHelperThunk.Value; }
		}

		readonly Lazy<PhoneNumberPropertyHelper> phoneNumberPropertyHelperThunk;

		#endregion

		#endregion

		#region OA_FCLEquipmentNeeded

		[List("Lookups.CartageEquipmentNeededFCL")]
		public override ZString OA_FCLEquipmentNeeded
		{
			get { return base.OA_FCLEquipmentNeeded; }
			set { base.OA_FCLEquipmentNeeded = value; }
		}

		#endregion

		#region OA_Latitude - For deduplication only. Should be removed later.

		public ZDecimal OA_Latitude
		{
			get => (OA_GeoLocation.IsEmpty) ? 0m : (decimal)OA_GeoLocation.Latitude.GetValueOrDefault();
			set
			{
				if (OA_GeoLocation.IsEmpty)
				{
					if (value == 0)
					{
						return;
					}
					else
					{
						SetPropertyValue(OA_GeoLocationInfo, ZGeography.CreatePoint(0, (double)value));
					}
				}
				else
				{
					SetPropertyValue(OA_GeoLocationInfo, (OA_GeoLocation.Longitude == 0 && value == 0) ? ZGeography.Empty : ZGeography.CreatePoint(OA_GeoLocation.Longitude.GetValueOrDefault(), (double)value));
				}
				OA_LatitudeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateOA_GeoLocation();
				}
			}
		}

		public ZPropertyInfo OA_LatitudeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(OA_Latitude)); }
		}

		#endregion

		#region OA_Longitude - For deduplication only. Should be removed later.

		public ZDecimal OA_Longitude
		{
			get
			{
				return OA_GeoLocation.IsEmpty ? 0m : (decimal)OA_GeoLocation.Longitude.GetValueOrDefault();
			}
			set
			{
				if (OA_GeoLocation.IsEmpty)
				{
					if (value == 0)
					{
						return;
					}
					else
					{
						SetPropertyValue(OA_GeoLocationInfo, ZGeography.CreatePoint((double)value, 0));
					}
				}
				else
				{
					SetPropertyValue(OA_GeoLocationInfo, (OA_GeoLocation.Latitude == 0 && value == 0) ? ZGeography.Empty : ZGeography.CreatePoint((double)value, OA_GeoLocation.Latitude.GetValueOrDefault()));
				}
				OA_LongitudeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateOA_GeoLocation();
				}
			}
		}

		public ZPropertyInfo OA_LongitudeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(OA_Longitude)); }
		}

		#endregion

		#region OA_Language

		[List("LanguageList")]
		public override ZString OA_Language
		{
			get { return base.OA_Language; }
			set
			{
				if (Culture.LanguageCodeMapping.TryGetValue(value, out string newCode))
				{
					value = newCode;
				}
				base.OA_Language = value;

				if (!DocumentGenerationHelper.IsGeneratingDocument)
				{
					if (Header != null && Header.OH_IsGlobalAccount && IsDefaultOfficeAddress)
					{
						MergeMainAddresses();
					}
					if (Header != null && IsMainAddress)
					{
						Header.MarkAsNeedingValidation();
					}
					//Revalidate fields whose validation is language dependent
					if (!IsValidationSuspended)
					{
						Validation.ValidateOA_CompanyNameOverride();
						Validation.ValidateOA_Address1();
						Validation.ValidateOA_Address2();
						Validation.ValidateOA_City();
						Validation.ValidateOA_State();
						Validation.ValidateOA_Code();
					}
				}
			}
		}

		#endregion

		#region OA_LCLEquipmentNeeded

		[List("Lookups+CartageEquipmentNeededLCL")]
		public override ZString OA_LCLEquipmentNeeded
		{
			get { return base.OA_LCLEquipmentNeeded; }
			set { base.OA_LCLEquipmentNeeded = value; }
		}

		#endregion

		#region OA_LabourRequired

		[List("OA_LabourRequired_List")]
		public override ZString OA_LabourRequired
		{
			get { return base.OA_LabourRequired; }
			set { base.OA_LabourRequired = value; }
		}

		#endregion

		#region OA_DeliveryRoute

		[List("DeliveryRoutesList")]
		public override ZString OA_DeliveryRoute
		{
			get { return base.OA_DeliveryRoute; }
			set
			{
				base.OA_DeliveryRoute = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOA_DeliveryRouteSequence();
				}
			}
		}

		#endregion

		#region OA_DeliveryRouteSequence

		public override ZShort OA_DeliveryRouteSequence
		{
			get { return base.OA_DeliveryRouteSequence; }
			set
			{
				base.OA_DeliveryRouteSequence = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOA_DeliveryRoute();
				}
			}
		}

		#endregion

		#region OA_RN_NKCountryCode

		public override ZString OA_RN_NKCountryCode
		{
			get
			{
				return base.OA_RN_NKCountryCode;
			}
			set
			{
				if (OA_RN_NKCountryCode != value)
				{
					base.OA_RN_NKCountryCode = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateOA_RN_NKCountryCode();
						Validation.ValidateOA_State();
					}
					if (Header != null)
					{
						Header.Addresses.MarkAsNeedingValidation();
						Header.Contacts.MarkAsNeedingValidation();
					}
					PhoneNumber.RefreshFormat();
					FaxNumber.RefreshFormat();
					ResetValidationStatus(OA_RN_NKCountryCodeInfo);
					SetDefaultLanguage();
					RefreshDefaultTimetables();
				}
			}
		}

		void SetDefaultLanguage()
		{
			if (!IsInDatabase)
			{
				if (Country != null && OrganisationsDataRegistry.Instance.TryGetDefaultLanguage(Country.PK, out var defaultLanguage))
				{
					OA_Language = defaultLanguage;

					if (!SettingDefaults && Header != null && !Header.IsInDatabase && IsMainAddress)
					{
						Header.OH_Language = defaultLanguage;
					}
				}
				else if (SettingDefaults)
				{
					OA_Language = Constants.Languages.English;
				}
			}
		}

		void RefreshDefaultTimetables()
		{
			if (timetables != null && TimetablesRangeType == OrgTimeTableRangeType.Default)
			{
				Timetables.SetRangeType(OrgTimeTableRangeType.Default);
			}
		}

		[MacroIgnore]
		[Obsolete("Please use Country property. This will be removed once references in client-defined macros (Documents, Workflows, Notes, etc) are transformed.")]
		public RefCountry CountryCode => Country;

		#endregion

		#region Related Country

		public RefCountry RelatedCountry
		{
			get
			{
				RefCountry country = null;

				if (!string.IsNullOrEmpty(OA_RN_NKCountryCode))
				{
					return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, OA_RN_NKCountryCode);
				}

				if (base.OA_RL_NKRelatedPortCode.Length == 2 || base.OA_RL_NKRelatedPortCode.Length == 5)
				{
					country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, base.OA_RL_NKRelatedPortCode.Substring(0, 2));
				}
				else if (Header != null && (Header.OH_RL_NKClosestPort.Length == 2 || Header.OH_RL_NKClosestPort.Length == 5))
				{
					country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Header.OH_RL_NKClosestPort.Substring(0, 2));
				}

				return country;
			}
		}

		#endregion

		#region OA_RL_NKRelatedPortCode

		[List("RefUNLOCOs")]
		public override sealed ZString OA_RL_NKRelatedPortCode
		{
			get
			{
				ZString result;
				OrgHeader header = Header;
				if (header != null && !isSettingRelatedPort)
				{
					if (base.OA_RL_NKRelatedPortCode.IsEmpty && (header.OH_IsGlobalAccount || IsMainAddress))
					{
						result = header.OH_RL_NKClosestPort;
					}
					else
					{
						result = base.OA_RL_NKRelatedPortCode;
					}
				}
				else
				{
					result = base.OA_RL_NKRelatedPortCode;
				}
				return result;
			}
			set
			{
				if (!isSettingRelatedPort)
				{
					isSettingRelatedPort = true;
					try
					{
						if (DocumentGenerationHelper.IsGeneratingDocument)
						{
							base.OA_RL_NKRelatedPortCode = value;
							return;
						}

						OrgHeader header = Header;
						if (header != null)
						{
							if (IsMainAddress && !header.OH_IsGlobalAccount)
							{
								header.OH_RL_NKClosestPort = value;
								header.MarkAsNeedingValidation();
							}
							else
							{
								header.OH_RL_NKClosestPortInfo.RefreshBinding();
							}
						}

						if (base.OA_RL_NKRelatedPortCode != value)
						{
							base.OA_RL_NKRelatedPortCode = value;
							if (value.Length == 2 || value.Length == 5)
							{
								OA_RN_NKCountryCode = value.Substring(0, 2);
							}

							this.MarkAsNeedingValidation();
							if (header != null)
							{
								header.Validation.ValidateOH_RL_NKClosestPort();
								header.PatternMatchRequiresRegen = true;
								if (header.OH_IsGlobalAccount)
								{
									header.OH_RL_NKClosestPortInfo.RefreshBinding();
								}
							}
						}

						if ((header != null) && IsDefaultOfficeAddress && header.OH_IsGlobalAccount)
						{
							MergeMainAddresses();
							header.MarkAsNeedingValidation();
						}
					}
					finally
					{
						isSettingRelatedPort = false;
						OA_Phone_Formatted = OA_Phone_Formatted; // Refresh the property.
						OA_Mobile_Formatted = OA_Mobile_Formatted; // Refresh the property.
						OA_Fax_Formatted = OA_Fax_Formatted; // Refresh the property.
					}
				}
			}
		}

		bool isSettingRelatedPort;

		public ZString GetBaseOA_RL_NKRelatedPortCode()
		{
			return base.OA_RL_NKRelatedPortCode;
		}

		public void SetBaseOA_RL_NKRelatedPortCode(ZString value)
		{
			base.OA_RL_NKRelatedPortCode = value;
		}

		internal bool OA_RL_NKRelatedPortCodeHasChanges
		{
			get { return !base.OA_RL_NKRelatedPortCode.Equals(OA_RL_NKRelatedPortCodeInfo.OriginalValue); }
		}

		ZBool IMatchingAddress.OA_RL_NKRelatedPortCodeInfoHasChanges
		{
			get { return OA_RL_NKRelatedPortCodeInfo.HasChanges; }
		}

		void MergeMainAddresses()
		{
			if (Header != null && !IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code))
			{
				foreach (OrgAddress curAddress in Header.Addresses)
				{
					if (curAddress.PK != PK && curAddress.IsDefaultOfficeAddress)
					{
						OrgAddressCapabilityWrapper oAC = AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
						oAC.SetMainSilently(ZBool.False);
					}
				}
			}
		}

		#endregion

		#region OA_OH

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid OA_OH
		{
			get { return base.OA_OH; }
			set
			{
				if (base.OA_OH != value)
				{
					base.OA_OH = value;
					if (Header != null)
					{
						Header.MarkAsNeedingValidation();
					}

					if (AdditionalInfos != null)
					{
						AdditionalInfos.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region OA_CompanyNameOverride

		/// <summary>
		/// Use this property if only we want to show the full company name.
		/// Otherwise use 'OA_CompanyNameOverrideTruncated' as in most of places we only want to show first 50 characters of a company name
		/// </summary>
		public override ZString OA_CompanyNameOverride
		{
			get { return base.OA_CompanyNameOverride; }
			set
			{
				if (base.OA_CompanyNameOverride != value)
				{
					base.OA_CompanyNameOverride = value;
					if (Header != null)
					{
						Header.PatternMatchRequiresRegen = true;
					}
				}
			}
		}

		/// <summary>
		/// Use this property to display first 50 characters for company name.
		/// In most cases the system is best suited to show 50 characters for company name.
		/// Use the property 'OA_CompanyNameOverride' if only we want to show the full company name.
		/// </summary>
		public ZString OA_CompanyNameOverrideTruncated
		{
			get { return OA_CompanyNameOverride.Substring(0, OrgAddress.Schema.OA_CompanyNameOverrideTruncatedLength); }
		}

		#endregion

		#region OA_IsActive

		public override ZBool OA_IsActive
		{
			get { return base.OA_IsActive; }
			set
			{
				var previousValue = base.OA_IsActive;
				base.OA_IsActive = value;
				if (Header != null)
				{
					Header.Contacts?.OfType<OrgContact>().ForEach(c => c.Validation.ValidateOC_OA_OrgAddress());
					Header.AppointedAgentPorts?.OfType<OrgAppointedAgentPorts>()
						.ForEach(p => p.Validation.ValidateO5_OA_AgentOfficeAddress());
					Header.AppointedGatewayAgentPorts?.OfType<OrgAppointedAgentPorts>()
						.ForEach(p => p.Validation.ValidateO5_OA_AgentOfficeAddress());

					Header.PatternMatchRequiresRegen = true;
					if (!IsValidationSuspended)
					{
						foreach (OrgAddress address in Header.Addresses)
						{
							address.Validation.ValidateOA_IsActive();
						}
					}
				}

				if (value && !previousValue)
				{
					OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
					TranslatedAddresses.ForEach(ota => ota.OTA_ValidationStatus = AddressValidationStatus.ToBeVerified);
				}
			}
		}

		#endregion

		#region IsOverridenJobLoadingDuration

		public ZBool IsOverridenJobLoadingDuration
		{
			get
			{
				return (!isOverridenJobLoadingDuration) ? isOverridenJobLoadingDuration = base.OA_JobLoadingDuration != 0 : isOverridenJobLoadingDuration;
			}
			set
			{
				if (isOverridenJobLoadingDuration != value)
				{
					isOverridenJobLoadingDuration = value;
					if (!isOverridenJobLoadingDuration)
					{
						OA_JobLoadingDuration = 0;
					}
				}

				IsOverridenJobLoadingDurationInfo.RefreshBinding();
			}
		}

		bool isOverridenJobLoadingDuration;

		public ZPropertyInfo IsOverridenJobLoadingDurationInfo
		{
			get { return GetZPropertyInfo(Schema.IsOverridenJobLoadingDuration); }
		}

		#endregion

		#region OA_JobLoadingDuration

		public override ZInt OA_JobLoadingDuration
		{
			get
			{
				var result = 0;
				if (base.OA_JobLoadingDuration != 0)
				{
					result = base.OA_JobLoadingDuration;
				}
				else
				{
					result = new ZInt(ObjectFactory.Get<ILandTransportRegistry>().DefaultJobLoadingFixedDuration.Value);
				}

				return result;
			}
			set
			{
				base.OA_JobLoadingDuration = value;
				isOverridenJobLoadingDuration = default;
				IsOverridenJobLoadingDurationInfo.RefreshBinding();
			}
		}

		#endregion

		#region OA_SystemCreateTimeUtc

		public override ZDateTime OA_SystemCreateTimeUtc
		{
			get => base.OA_SystemCreateTimeUtc;
			set
			{
				if (base.OA_SystemCreateTimeUtc != value)
				{
					base.OA_SystemCreateTimeUtc = value;

					var header = Header;
					if (header != null)
					{
						header.MarkAsNeedingValidation();
						header.Addresses.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region

		[ResourceStringData("OrgAddress|OA_UseCumulativeFreeWaitingTime", Caption = "Use Cumulative")]
		public override ZBool OA_UseCumulativeFreeWaitingTime
		{
			get { return base.OA_UseCumulativeFreeWaitingTime; }
			set { base.OA_UseCumulativeFreeWaitingTime = value; }
		}

		#endregion

		#region IsCancelled

		public override bool IsCancelled
		{
			get { return base.IsCancelled || (Header?.IsCancelled ?? false); }
		}

		#endregion

		#region CityFallback

		public ZString CityFallback
		{
			get
			{
				if (!OA_City.IsEmpty)
				{
					return OA_City;
				}
				else if (Header != null && Header.UNLOCO != null && !Header.UNLOCO.RL_PortName.IsEmpty)
				{
					return Header.UNLOCO.RL_PortName;
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region Address Validation

		public string Apartment
		{
			get { return AddressValidationService.GetApartment(this); }
		}

		public string Street
		{
			get { return AddressValidationService.GetStreet(this); }
		}

		public string StreetNumber
		{
			get { return AddressValidationService.GetStreetNumber(this); }
		}

		public bool IsSuspendingDeduplication;

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString OA_ValidationStatus
		{
			get { return base.OA_ValidationStatus; }
			set
			{
				base.OA_ValidationStatus = value;
				RaiseAddressValidationStatusChanged();

				if (value == AddressValidationStatus.ManuallyVerified ||
					value == AddressValidationStatus.Verified ||
					value == AddressValidationStatus.VerifiedToStreet ||
					value == AddressValidationStatus.CountryNotAvailable)
				{
					if (!IsSuspendingDeduplication && Header != null)
					{
						Header.FindDuplicates();
					}
				}

				isManuallyVerifiedByUser = value == AddressValidationStatus.ManuallyVerified;
			}
		}

		public ZString EffectiveCompanyNameTruncated
		{
			get { return (!OA_CompanyNameOverride.IsEmpty ? OA_CompanyNameOverrideTruncated : (Header != null ? Header.OH_FullNameTruncated : ZString.Empty)); }
		}

		public ZString EffectiveCompanyName
		{
			get { return (!OA_CompanyNameOverride.IsEmpty ? OA_CompanyNameOverride : (Header != null ? Header.OH_FullName : ZString.Empty)); }
		}

		public RefUNLOCO EffectiveRelatedPortCode
		{
			get
			{
				RefUNLOCO result = RelatedPortCode;
				OrgHeader header;
				if (result == null && (header = Header) != null)
				{
					result = header.UNLOCO;
				}
				return result;
			}
		}

		public bool IsTemporaryOrgAddress { get; set; }

		#endregion

		#region MaxLengthOfAddressForUsageComment

		public int MaxLengthOfAddressForUsageComment
		{
			get { return OA_CodeInfo.MaxLength; }
		}

		#endregion

		#region LocalControlledPremisesID

		OrgCusCode GetLocalControlledPremises(string cCPType)
		{
			var cusCodes = Header?.CustomsCodes;
			OrgCusCode result = null;

			if (cusCodes != null)
			{
				var customsCodesOfTypeInCountry = cusCodes.Cast<OrgCusCode>().Where(x => x.OK_CodeType == cCPType && x.OK_RN_NKCodeCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode).OrderBy(x => x.PK);
				foreach (var customsCode in customsCodesOfTypeInCountry)
				{
					if (customsCode.PremisesAddress == this)
					{
						result = customsCode;
						break;
					}
					else if (result == null && customsCode.PremisesAddress == null)
					{
						result = customsCode;
					}
				}
			}
			return result;
		}

		OrgCusCode GetLocalControlledPremisesID()
		{
			return GetLocalControlledPremises(OrgCusCode.CodeTypes.ControlledPremisesID);
		}

		public ZString LocalControlledPremisesID
		{
			get
			{
				OrgCusCode orgCusCode = GetLocalControlledPremisesID();
				return orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
			}
			set
			{
				OrgCusCode orgCusCode = GetLocalControlledPremisesID();
				if (orgCusCode == null)
				{
					orgCusCode = Header.CustomsCodes.AddNew();
					orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
					orgCusCode.OK_OA_PremisesAddress = PK;
				}
				orgCusCode.OK_CustomsRegNo = value;
			}
		}

		#endregion

		#region Depot-LocalControlledPremisesID

		OrgCusCode GetDepotLocalControlledPremisesID()
		{
			return GetLocalControlledPremises(OrgCusCode.CodeTypes.DepotControlledPremisesID);
		}

		public ZString DepotLocalControlledPremisesID
		{
			get
			{
				OrgCusCode orgCusCode = GetDepotLocalControlledPremisesID();
				return orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
			}
			set
			{
				OrgCusCode orgCusCode = GetDepotLocalControlledPremisesID();
				if (orgCusCode == null)
				{
					orgCusCode = Header.CustomsCodes.AddNew();
					orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DepotControlledPremisesID;
					orgCusCode.OK_OA_PremisesAddress = PK;
				}
				orgCusCode.OK_CustomsRegNo = value;
			}
		}

		#endregion

		#region Warehouse-LocalControlledPremisesID

		OrgCusCode GetWarehouseLocalControlledPremisesID()
		{
			return GetLocalControlledPremises(OrgCusCode.CodeTypes.WarehouseControlledPremisesID);
		}

		public ZString WarehouseLocalControlledPremisesID
		{
			get
			{
				OrgCusCode orgCusCode = GetWarehouseLocalControlledPremisesID();
				return orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
			}
			set
			{
				OrgCusCode orgCusCode = GetWarehouseLocalControlledPremisesID();
				if (orgCusCode == null)
				{
					orgCusCode = Header.CustomsCodes.AddNew();
					orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
					orgCusCode.OK_OA_PremisesAddress = PK;
				}
				orgCusCode.OK_CustomsRegNo = value;
			}
		}

		#endregion

		#region StateListHasMembers

		public ZBool StateListHasMembers
		{
			get { return OA_State_List.Count > 0; }
		}

		public ZPropertyInfo StateListHasMembersInfo
		{
			get { return GetZPropertyInfo(nameof(StateListHasMembers)); }
		}

		#endregion

		#region StateListDoesNotHaveMembers

		public ZBool StateListDoesNotHaveMembers
		{
			get { return !StateListHasMembers; }
		}

		public ZPropertyInfo StateListDoesNotHaveMembersInfo
		{
			get { return GetZPropertyInfo(nameof(StateListDoesNotHaveMembers)); }
		}

		#endregion

		#region StateListForCurrentAddressHasMembers

		public ZBool StateListForCurrentAddressHasMembers
		{
			get { return OA_State_List_For_Current_Address.Count > 0; }
		}

		public ZPropertyInfo StateListForCurrentAddressHasMembersInfo
		{
			get { return GetZPropertyInfo(nameof(StateListForCurrentAddressHasMembers)); }
		}

		#endregion

		#region StateListForCurrentAddressDoesNotHaveMembers

		public ZBool StateListForCurrentAddressDoesNotHaveMembers
		{
			get { return !StateListForCurrentAddressHasMembers; }
		}

		public ZPropertyInfo StateListForCurrentAddressDoesNotHaveMembersInfo
		{
			get { return GetZPropertyInfo(nameof(StateListForCurrentAddressDoesNotHaveMembers)); }
		}

		#endregion

		#region IsEnglish

		public bool IsEnglish
		{
			get { return Res.IsEnglish(OA_Language); }
		}

		#endregion

		#region IsUniqueMainAddressForGlobalOrg

		public ZBool IsUniqueMainAddressForGlobalOrgOfType(ZString addressType)
		{
			var result = false;

			if (Header != null && Header.OH_IsGlobalAccount)
			{
				result = !Header
					.Addresses
					.Cast<OrgAddress>()
					.Any(address =>
						address.PK != PK &&
						address.AddressCapability.GetCapabilityEnabled(addressType) &&
						address.AddressCapability.GetIsMainAddress(addressType) &&
						address.RelatedCountry == RelatedCountry);
			}

			return result;
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region PhoneNumbers

		public PhoneNumber MobilePhoneNumber
		{
			get
			{
				if (mobilePhoneNumber == null)
				{
					mobilePhoneNumber = new PhoneNumber(this, OA_Mobile_FormattedInfo, null, OA_Mobile_FormattedLocalNumberIfLoggedInSameCountryInfo, OA_Mobile_IsManuallyVerifiedInfo);
				}
				return mobilePhoneNumber;
			}
		}
		PhoneNumber mobilePhoneNumber;

		public PhoneNumber PhoneNumber
		{
			get
			{
				if (phoneNumber == null)
				{
					phoneNumber = new PhoneNumber(this, OA_Phone_FormattedInfo, null, OA_Phone_FormattedLocalNumberIfLoggedInSameCountryInfo, OA_Phone_IsManuallyVerifiedInfo);
				}
				return phoneNumber;
			}
		}
		PhoneNumber phoneNumber;

		public PhoneNumber FaxNumber
		{
			get
			{
				if (faxNumber == null)
				{
					faxNumber = new PhoneNumber(this, OA_Fax_FormattedInfo, null, OA_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo, OA_Fax_IsManuallyVerifiedInfo);
				}
				return faxNumber;
			}
		}
		PhoneNumber faxNumber;

		#endregion

		#region Address Capabilities

		#region Collection

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public virtual OrgAddressCapabilityWrapperCollection AddressCapability
		{
			get
			{
				if (fAddressCapability == null)
				{
					fAddressCapability = new OrgAddressCapabilityWrapperCollection(this);
					RegisterEditableChildObject(fAddressCapability);
				}
				return fAddressCapability;
			}
		}

		OrgAddressCapabilityWrapperCollection fAddressCapability;

		public ActiveBusinessObjectCollection<OrgAddressCapability> CapabilitiesCollection
		{
			get
			{
				if (capabilitiesCollection == null)
				{
					capabilitiesCollection = new ActiveBusinessObjectCollection<OrgAddressCapability>(this);
					capabilitiesCollection.CollectionCountChange += capabilitiesCollection_CollectionCountChange;
				}
				return capabilitiesCollection;
			}
		}

		ActiveBusinessObjectCollection<OrgAddressCapability> capabilitiesCollection;

		void capabilitiesCollection_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (!SettingDefaults && fAddressCapability != null)
			{
				using (SuspendSettingHasChanges())
				{
					foreach (OrgAddressCapabilityWrapper capabilityWrapper in fAddressCapability)
					{
						var capability = GetAddressCapability(capabilityWrapper.AddressCapabilityType);
						capabilityWrapper.RefreshValues(capability);
					}
				}
			}
		}

		internal OrgAddressCapability GetAddressCapability(ZString capabilityType)
		{
			OrgAddressCapability result = null;
			foreach (OrgAddressCapability addressCapability in CapabilitiesCollection)
			{
				if (addressCapability.PZ_AddressType == capabilityType)
				{
					result = addressCapability;
					break;
				}
			}
			return result;
		}

		#endregion

		#region AddressCapability helpers

		public ZBool IsAddressOfType(OrgAddressType addressType)
		{
			return AddressCapability.GetCapabilityEnabled(addressType.ToString());
		}

		public ZBool IsMainAddressOfType(OrgAddressType addressType)
		{
			var result = false;

			if (fAddressCapability != null)
			{
				result = AddressCapability.GetIsMainAddress(addressType.ToString());
			}
			else
			{
				result = IsMainAddress_WithoutLoadingCapabilitiesCollection(addressType.ToString());
			}

			return result;
		}

		bool IsMainAddress_WithoutLoadingCapabilitiesCollection(string addressType)
		{
			var query = new ZQuery(OrgAddressCapabilitySchema.PZ_OA, PK);
			query.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, addressType);
			query.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, true);

			return Factory.LoadTop1<OrgAddressCapability>(query) != null;
		}

		public void AddAddressType(OrgAddressType addressType)
		{
			AddressCapability.SetCapabilityEnabled(addressType);
		}

		public void DeleteAddressType(OrgAddressType addressType)
		{
			AddressCapability.SetCapabilityDisabled(addressType);
		}

		#endregion

		#endregion

		#region OrgFreeWaitingTime

		[ChildEditable]
		public OrgFreeWaitingTimeCollection FreeWaitingCollection
		{
			get
			{
				if (fFreeWaitingCollection == null)
				{
					fFreeWaitingCollection = new OrgFreeWaitingTimeCollection(Factory, this);
					RegisterEditableChildObject(fFreeWaitingCollection);
				}
				return fFreeWaitingCollection;
			}
		}

		OrgFreeWaitingTimeCollection fFreeWaitingCollection;

		#endregion

		#region Pattern Matches

		public OrgPatternMatchCollection PatternMatches
		{
			get
			{
				if (fPatternMatches == null)
				{
					ZQuery filter = new ZQuery(OrgPatternMatchSchema.OS_OA, PK);
					if (!OA_CompanyNameOverride.IsEmpty)
					{
						ZQuery subquery = new ZQuery(OrgPatternMatchSchema.OS_FullCompanyName, OA_CompanyNameOverride);
						subquery.AddToFilter(OrgPatternMatchSchema.OS_OH, OA_OH);
						filter.AddToFilter(subquery, JoinCondition.Or);
					}
					var localPatternMatches = new OrgPatternMatchCollection(Factory, filter);
					localPatternMatches.Load();
					fPatternMatches = localPatternMatches;
				}

				return fPatternMatches;
			}
		}

		OrgPatternMatchCollection fPatternMatches;

		#endregion

		#region Known Shipper Details

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCountryDataAddressDependentCollection KnownShipperDetails
		{
			get
			{
				if (fKnownShipperDetails == null)
				{
					fKnownShipperDetails = new OrgCountryDataAddressDependentCollection(this);
					RegisterEditableChildObject(fKnownShipperDetails);
				}

				return fKnownShipperDetails;
			}
		}

		OrgCountryDataAddressDependentCollection fKnownShipperDetails;

		public OrgCountryData KnownShipper
		{
			get { return KnownShipperDetails.FirstOrDefault(); }
		}

		#endregion

		#region Customs Codes

		[BusinessObjectTestExclude]
		public OrgAddressCusCodeCollection CustomsCodes
		{
			get
			{
				if (customsCodes == null && Header != null)
				{
					customsCodes = new OrgAddressCusCodeCollection(this);
				}
				return customsCodes;
			}
		}

		OrgAddressCusCodeCollection customsCodes;

		#endregion

		#region Timetable

		[ChildEditable(true)]
		public OrgTimetableCollection Timetables
		{
			get
			{
				if (timetables == null)
				{
					using (SuspendSettingHasChangesIncludingChildren())
					{
						timetables = new OrgTimetableCollection(this);
						if (!this.IsDeleting && !this.IsDeleted)
						{
							System.Collections.IComparer comparer = new OrgTimetableComparer();
							timetables.ApplySort(comparer);
							RegisterEditableChildObject(timetables);
							timetables.InitializeRangeType();
						}
					}
				}
				return timetables;
			}
		}
		OrgTimetableCollection timetables;

		public void SetTimetablesRangeType(OrgTimeTableRangeType rangeType)
		{
			Timetables.SetRangeType(rangeType);
		}

		public bool TimetablesHasChanges => Timetables.HasChanges;

		public OrgTimeTableRangeType TimetablesRangeType => Timetables.RangeType;

		#endregion

		#region Additional Address Information

		[ChildEditable(true)]
		public OrgAddressAdditionalInfoCollection AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = new OrgAddressAdditionalInfoCollection(this);
					RegisterEditableChildObject(additionalInfos);
				}
				return additionalInfos;
			}
		}
		OrgAddressAdditionalInfoCollection additionalInfos;

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				PatternMatches.RemoveAndDeleteAll();
			}
			AddressCapability.RemoveAndDeleteAll();
			FreeWaitingCollection.DeleteAll();
			KnownShipperDetails.DeleteAll();
			Timetables.DeleteAllAndIgnoreDeleteEvent();
			AddOnRuleAcks.DeleteAll();
			TranslatedAddresses.DeleteAll();
			AdditionalInfos.DeleteAll();

			base.Delete();
			NullAllDependentObjectReferences();
		}

		// This is a temporary fix until the Factory gets refreshed properly after a delete fails.
		// Currently, when a delete fails from the GUI, the dependent collections do not get refreshed properly, so you cannot access any values.
		void NullAllDependentObjectReferences()
		{
			fPatternMatches = null;
			fAddressCapability = null;
			fFreeWaitingCollection = null;
			fKnownShipperDetails = null;
			timetables = null;
			addOnRuleAcks = null;
			translatedAddresses = null;
		}

		#endregion

		#region Implementation

		#region Phone Number Validator

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk;

		#endregion

		public bool IsMainAddress
		{
			get
			{
				OrgHeader header = Header;
				OrgAddress mainAddress = header != null ? header.MainAddress : null;
				return mainAddress != null && mainAddress.PK == PK;
			}
		}

		public bool AllowModify => IsMainAddress ?
									Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed :
									Env.Security.OrgAddressDetailsModify.IsAllowed;

		public bool IsReferencedByContact
		{
			get
			{
				return Header != null && Header.Contacts.OfType<OrgContact>().Any(c => c.OC_OA_OrgAddress == PK);
			}
		}

		bool IsDefaultOfficeAddress
		{
			get { return AddressCapability.GetCapabilityEnabled(OrgAddressType.Office.Code) && AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code); }
		}

		public bool IsCustomsAddress
		{
			get { return AddressCapability.GetCapabilityEnabled(OrgAddressType.CustomsAddressOfRecord.Code); }
		}

		public bool IsEUCustomsAddress
		{
			get { return AddressCapability.GetCapabilityEnabled(OrgAddressType.EUCustomsAddress.Code); }
		}

		protected virtual void SetDefaultUsageComment()
		{
			SetDefaultUsageComment(null);
		}

		public void SetDefaultUsageComment(IEnumerable<string> codesToCheckAgainst)
		{
			if ((this as ISupportDataImporting).IsImportingData && !OA_Code.IsEmpty)
			{
				return;
			}

			ZString addressPart = OA_Address1.Trim();
			if (addressPart.Length > MaxLengthOfAddressForUsageComment)
			{
				addressPart = addressPart.Substring(0, MaxLengthOfAddressForUsageComment).Trim();
			}
			OA_Code = GetUniqueUsageComment(addressPart, codesToCheckAgainst);
		}

		ZString GetUniqueUsageComment(ZString addressPart, IEnumerable<string> codes)
		{
			ZString proposedUsageComment = addressPart;
			if (Header != null)
			{
				int extension = 1;
				ZString newAddressPart = addressPart;
				while (!IsUniqueUsageComment(proposedUsageComment, codes ?? GetUsageComments()))
				{
					while (newAddressPart.Length > 0 && char.IsDigit(newAddressPart[newAddressPart.Length - 1]))
					{
						newAddressPart = newAddressPart.Substring(0, newAddressPart.Length - 1).Trim();
					}
					string exString = extension.ToString();
					if (newAddressPart.Length + exString.Length > MaxLengthOfAddressForUsageComment)
					{
						newAddressPart = newAddressPart.Substring(0, MaxLengthOfAddressForUsageComment - exString.Length).Trim();
					}
					proposedUsageComment = newAddressPart + exString;
					extension++;
				}
			}
			return proposedUsageComment.Left(25);
		}

		protected bool IsUniqueUsageComment(ZString usageComment)
		{
			return IsUniqueUsageComment(usageComment, GetUsageComments());
		}

		public List<string> GetUsageComments()
		{
			List<string> result = new List<string>();
			foreach (OrgAddress address in Header.Addresses)
			{
				if (address != this)
				{
					result.Add(address.OA_Code);
				}
			}
			return result;
		}

		public bool IsUniqueUsageComment(ZString usageComment, IEnumerable<string> codes)
		{
			if (Header != null)
			{
				return !codes.Any(c => string.Equals(c, usageComment, StringComparison.OrdinalIgnoreCase));
			}
			return true;
		}

		public ZString AddressAsASingleLine
		{
			get { return new AddressFormatter(Factory, this, GlbCompany.CurrentCompany, false).PostalAddressAsASingleLine(); }
		}

		public ZString AddressAsASingleLineWithoutCompanyName
		{
			get { return new AddressFormatter(Factory, this, GlbCompany.CurrentCompany, false).PostalAddressAsASingleLineWithoutCompanyName(); }
		}

		public ZString AddressAsASingleLineWithoutCompanyNameAndAdditionalAddressInfo
		{
			get { return new AddressFormatter(Factory, this, GlbCompany.CurrentCompany, false, true).PostalAddressAsASingleLineWithoutCompanyName(); }
		}

		public ZString AddressWithoutCompanyName
		{
			get { return new AddressFormatter(Factory, this, GlbCompany.CurrentCompany, false).PostalAddressWithoutCompanyName(); }
		}

		public bool IgnoreValidationStatusError { get; set; }

		public bool IgnorePhoneNumberError { get; set; }

		public bool IgnoreFaxNumberError { get; set; }

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			if (OA_Address1.IsEmpty && AddressCapability.GetIsMainAddress(OrgConstants.AddressType.Office) && AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office))
			{
				OA_Address1 = AddressNotOnFile;

				if (OrganisationRegistry.Instance.EnableAddressNotOnFileLogging.Value
					&& OrgAddressDependentCollection.LoadMainAddressDirectlyFromDatabase(Factory, OA_OH) is not null)
				{
					ErrorReporter.ReportDeveloperExceptionOnce("DuplicateAddressNotOnFile", "Duplicate AddressNotOnFile should not be saved.", new InvalidOperationException("Saving Duplicate AddressNotOnFile"));
				}
			}

			InvalidateScreeningStatuses();

			AuthorisedToLeaveLogger.Log(this, OA_AuthorityToLeaveInfo);

			this.AddAddressValidationEventLog(OA_ValidationStatusInfo.OriginalValue.ToString(), Factory);
		}

		protected override void OnSavingForDelete()
		{
			InvalidateScreeningStatuses(true);
			base.OnSavingForDelete();
		}

		void InvalidateScreeningStatuses(bool isBeingDeleted = false)
		{
			if (!(this as ISupportDataImporting).IsImportingData)
			{
				var ohPK = isBeingDeleted ? (ZGuid)OA_OHInfo.OriginalValue : OA_OH;
				var header = Factory.Load<OrgHeader>(ohPK);
				if (header != null && !header.IsDeleted && !header.IsBeingDeleted)
				{
					var shouldInvalidateScreeningStatuses = ((OA_Address1Info.HasChanges ||
															 OA_Address2Info.HasChanges ||
															 OA_CityInfo.HasChanges ||
															 OA_PostCodeInfo.HasChanges ||
															 OA_StateInfo.HasChanges ||
															 OA_RN_NKCountryCodeInfo.HasChanges ||
															 OA_CompanyNameOverrideInfo.HasChanges) &&
															 (HasBeenChangedByUser || !IsExactPointFound)) ||
															 (header.OH_ScreeningStatus != ScreeningStatusesList.Codes.Matched && ((OA_IsActiveInfo.HasChanges && OA_IsActive) || !IsInDatabase)) ||
															 (header.OH_ScreeningStatus != ScreeningStatusesList.Codes.Clear && ((OA_IsActiveInfo.HasChanges && !OA_IsActive) || isBeingDeleted));

					if (shouldInvalidateScreeningStatuses)
					{
						header.InvalidateScreeningStatuses();
						HasBeenChangedByUser = false;
					}
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (IsInDatabase && (OA_Address1Info.HasChanges || OA_Address2Info.HasChanges || OA_CityInfo.HasChanges ||
				OA_PostCodeInfo.HasChanges || OA_StateInfo.HasChanges || OA_RN_NKCountryCodeInfo.HasChanges))
			{
				if (IsTSAKnownAddress)
				{
					Header.TSAKnownAddressChanged = true;

					var tsaRecords = Factory.Load<OrgCountryData>(new ZQuery()
						.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, Constants.CountryCodes.UnitedStates)
						.AddToFilter(OrgCountryDataSchema.OV_OA_ApprovedLocation, PK)
						.AddToFilter(OrgCountryDataSchema.OV_EXApprovedOrMajorExporter, AviationSecuritySchemeMembershipEx.Codes.Yes));

					if (tsaRecords.Length > 0)
					{
						if (Header.GetLogs().LogsNotInDB.All(u => u.SL_SE_NKEvent != AutoEvents.TSAKnownShipperApprovedStatusChanged.Code))
						{
							Header.GetLogs().AddNew(AutoEvents.TSAKnownShipperApprovedStatusChanged, "TSA Known Shipper Approved Status Changed To No");
						}

						foreach (var tsa in tsaRecords)
						{
							tsa.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;
						}
					}
				}
				if (IsMIDAddress)
				{
					Header.MIDAddressChanged = true;
				}
			}
		}

		#endregion

		#region IOrgAddress Members

		ZGuid IOrgAddress.PK
		{
			get { return PK; }
		}

		ZString IOrgAddress.Address
		{
			get
			{
				var result = new ZStringBuilder();

				if (!OA_CompanyNameOverride.IsEmpty)
				{
					result.AppendLine(" " + OA_CompanyNameOverrideTruncated);
				}
				else if (Header != null)
				{
					result.AppendLine(" " + Header.OH_FullNameTruncated);
				}

				result.Append(OA_Address2);

				if (!OA_City.IsEmpty)
				{
					result.Append(" " + OA_City);
				}

				if (!OA_State.IsEmpty)
				{
					result.Append(" " + OA_State);
				}

				if (!OA_PostCode.IsEmpty)
				{
					result.Append(" " + OA_PostCode);
				}

				return result.ToString().Trim();
			}
		}

		public ZString AddressFull
		{
			get
			{
				var result = new ZStringBuilder();

				if (!OA_CompanyNameOverride.IsEmpty)
				{
					result.AppendLine(" " + OA_CompanyNameOverrideTruncated);
				}
				else if (Header != null)
				{
					result.AppendLine(" " + Header.OH_FullNameTruncated);
				}

				if (!OA_Address1.IsEmpty)
				{
					result.Append(OA_Address1);
					if (!OA_Address2.IsEmpty)
					{
						result.Append(", ");
					}
				}
				if (!OA_Address2.IsEmpty)
				{
					result.Append(OA_Address2);
				}

				if (!OA_City.IsEmpty)
				{
					result.Append(" " + OA_City);
				}

				if (!OA_State.IsEmpty)
				{
					result.Append(" " + OA_State);
				}

				if (!OA_PostCode.IsEmpty)
				{
					result.Append(" " + OA_PostCode);
				}

				if (!OA_RL_NKRelatedPortCode.IsEmpty)
				{
					result.Append(" " + OA_RL_NKRelatedPortCode);
				}

				return result.ToString().Trim();
			}
		}

		public ZString CountryDescription => Country?.Description ?? ZString.Empty;

		ZString IOrgAddress.AddressDetailed
		{
			get
			{
				var result = new ZStringBuilder();
				var cityStatePostCodeBuilder = new ZStringBuilder();

				if (!OA_City.IsEmpty)
				{
					cityStatePostCodeBuilder.Append(OA_City);
				}

				if (!OA_State.IsEmpty)
				{
					cityStatePostCodeBuilder.Append(OA_State);
				}

				if (!OA_PostCode.IsEmpty)
				{
					cityStatePostCodeBuilder.Append(OA_PostCode);
				}

				var cityStatePostCode = new ZString(cityStatePostCodeBuilder.ToStringWithDelimiterBetweenAppends(" ").Trim(' ', '\t'));

				if (!OA_CompanyNameOverride.IsEmpty)
				{
					result.AppendLine(OA_CompanyNameOverrideTruncated);
				}

				if (!OA_Address1.IsEmpty)
				{
					result.AppendLine(OA_Address1);
				}

				if (!OA_Address2.IsEmpty)
				{
					result.AppendLine(OA_Address2);
				}

				if (!cityStatePostCode.IsEmpty)
				{
					result.AppendLine(cityStatePostCode);
				}

				return result.ToString();
			}
		}

		ZString IOrgAddress.AddressDetailedOnSingleLine
		{
			get { return AddressDescription; }
		}

		IOrgHeader IOrgAddress.Header => Header;

		ZGuid IOrgAddress.OrganisationPK
		{
			get { return OA_OH; }
		}

		ZPropertyInfo IOrgAddress.UsageCommentInfo
		{
			get { return OA_CodeInfo; }
		}

		#endregion

		#region IDocAddress Members

		ZString IDocAddress.E2_AddressType
		{
			get { return ZString.Empty; }
		}

		ZBool IDocAddress.E2_AddressOverride
		{
			get { return false; }
		}

		ZGuid IDocAddress.E2_OA_Address
		{
			get { return PK; }
		}

		ZString IDocAddress.E2_CompanyName
		{
			get { return EffectiveCompanyName; }
		}

		ZString IDocAddress.E2_CompanyNameTruncated
		{
			get { return EffectiveCompanyNameTruncated; }
		}

		ZString IDocAddress.E2_AdditionalAddressInformation
		{
			get { return PrimaryOrgAddressAdditionalInfoDetail; }
		}

		ZString IDocAddress.E2_Address1
		{
			get { return OA_Address1; }
		}

		ZString IDocAddress.E2_Address2
		{
			get { return OA_Address2; }
		}

		ZString IDocAddress.E2_City
		{
			get { return OA_City; }
		}

		ZString IDocAddress.E2_State
		{
			get { return OA_State; }
		}

		ZString IDocAddress.E2_Postcode
		{
			get { return OA_PostCode; }
		}

		ZString IDocAddress.E2_GovRegNum
		{
			get { return ZString.Empty; }
		}

		ZString IDocAddress.E2_GovRegNumType
		{
			get { return ZString.Empty; }
		}

		ZString IDocAddress.E2_PassportID
		{
			get { return ZString.Empty; }
		}

		ZString IDocAddress.E2_PassportCountryOfIssue
		{
			get { return ZString.Empty; }
		}

		ZDateTime IDocAddress.E2_PassportDateOfBirth
		{
			get { return ZDateTime.Empty; }
		}

		ZString IDocAddress.E2_PortCode
		{
			get
			{
				var result = EffectiveRelatedPortCode;
				return result != null ? result.RL_Code : ZString.Empty;
			}
		}

		ZString IDocAddress.CountryCode
		{
			get
			{
				RefUNLOCO relatedPortCode = EffectiveRelatedPortCode;
				return relatedPortCode != null ? relatedPortCode.RL_RN_NKCountryCode : ZString.Empty;
			}
		}

		ZString IDocAddress.ParentDescription
		{
			get { return Header.OH_Code; }
		}

		ZString IDocAddress.AddressCaption
		{
			get { return OA_Code; }
		}

		ZString IDocAddress.E2_Fax
		{
			get { return OA_Fax; }
		}

		ZString IDocAddress.E2_Phone
		{
			get { return OA_Phone; }
		}

		ZString IDocAddress.E2_RN_NKCountryCode
		{
			get { return OA_RN_NKCountryCode; }
		}

		IOrgHeader IDocAddress.Organisation
		{
			get { return this.Header; }
		}

		#endregion

		#region IReadOnlySecurity Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			var shouldBeReadOnly = false;

			if (Header != null)
			{
				if (property.Name == OrgAddressSchema.OA_IsActive.Name ||
					property.Name == OrgAddressSchema.OA_AdditionalAddressInformation.Name ||
					property.Name == OrgAddressSchema.OA_Address1.Name ||
					property.Name == OrgAddressSchema.OA_Address2.Name ||
					property.Name == OrgAddressSchema.OA_City.Name ||
					property.Name == OrgAddressSchema.OA_PostCode.Name ||
					property.Name == OrgAddressSchema.OA_State.Name ||
					property.Name == OrgAddressSchema.OA_CompanyNameOverride.Name ||
					property.Name == OrgAddressSchema.OA_Language.Name ||
					property.Name == OrgAddressSchema.OA_RL_NKRelatedPortCode.Name ||
					property.Name == OrgAddressSchema.OA_RN_NKCountryCode.Name ||
					property.Name == nameof(UnrestrictedAdditionalAddressInformation))
				{
					shouldBeReadOnly = Header.SecurityProvider.ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities(this).Any() ||
										(IsCustomsAddress || IsEUCustomsAddress ?
										(IsCustomsAddress && !Header.SecurityProvider.HasModifyCustomsAddressSecurity) || (IsEUCustomsAddress && !Header.SecurityProvider.HasModifyEUCustomsAddressSecurity) :
										IsMainAddress ?
											!Header.SecurityProvider.HasModifyDetailsNameAndAddressSecurity :
											!Header.SecurityProvider.HasModifyAddressDetailsSecurity);
				}
				else if (property.Name == OrgAddressSchema.OA_Code.Name)
				{
					shouldBeReadOnly = Header.SecurityProvider.ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities(this).Any() ||
										(IsCustomsAddress || IsEUCustomsAddress ?
										(IsCustomsAddress && !Header.SecurityProvider.HasModifyCustomsAddressSecurity) || (IsEUCustomsAddress && !Header.SecurityProvider.HasModifyEUCustomsAddressSecurity) :
										IsMainAddress ?
											!Header.SecurityProvider.HasModifyDetailsAddressShortCodeSecurity :
											!Header.SecurityProvider.HasModifyAddressShortCodeSecurity);
				}
				else if (property.Name == OrgAddressSchema.OA_Phone.Name ||
							property.Name == OrgAddressSchema.OA_Mobile.Name ||
							property.Name == OrgAddressSchema.OA_Fax.Name ||
							property.Name == OrgAddressSchema.OA_Email.Name ||
							property.Name == Schema.OA_Fax_Formatted ||
							property.Name == Schema.OA_Mobile_Formatted ||
							property.Name == Schema.OA_Phone_Formatted
						)
				{
					shouldBeReadOnly = Header.SecurityProvider.ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities(this).Any() ||
										(IsCustomsAddress || IsEUCustomsAddress ?
										(IsCustomsAddress && !Header.SecurityProvider.HasModifyCustomsAddressSecurity) || (IsEUCustomsAddress && !Header.SecurityProvider.HasModifyEUCustomsAddressSecurity) :
										IsMainAddress ?
											!Header.SecurityProvider.HasModifyDetailsPhFaxWebDetailsSecurity :
											!Header.SecurityProvider.HasModifyAddressDetailsSecurity);
				}
				else
				{
					shouldBeReadOnly = IsCustomsAddress || IsEUCustomsAddress ?
										(IsCustomsAddress && !Header.SecurityProvider.HasModifyCustomsAddressSecurity) || (IsEUCustomsAddress && !Header.SecurityProvider.HasModifyEUCustomsAddressSecurity) :
										!Header.SecurityProvider.HasModifyAddressAdditionalDetailsSecurity;
				}
			}

			bool tSAKnownAddressReadOnly = IsTSAKnownAddress && !Env.Security.OrgAddressTSAKnownAddressModify.IsAllowed;
			bool mIDAddressReadOnly = IsMIDAddress && !Env.Security.OrgAddressMIDAddressModify.IsAllowed;
			return shouldBeReadOnly || tSAKnownAddressReadOnly || mIDAddressReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		bool ISupportWebAddressValidation.GetReadOnlySecurity(PropertyDescriptor property)
		{
			return GetReadOnlySecurity(property);
		}

		#endregion

		#region IAddressDetails Members

		ZString IAddressDetails.CompanyName
		{
			get { return EffectiveCompanyNameTruncated; }
		}

		ZString IAddressDetails.ContactName
		{
			get { return Header != null && Header.Contacts.Count > 0 ? Header.Contacts[0].OC_ContactName : ZString.Empty; }
		}

		ZString IAddressDetails.Phone
		{
			get { return OA_Phone; }
		}

		ZString IAddressDetails.Fax
		{
			get { return OA_Fax; }
		}

		ZString IAddressDetails.Email
		{
			get { return OA_Email; }
		}

		ZString IAddressDetails.AddressLine1
		{
			get { return OA_Address1; }
		}

		ZString IAddressDetails.AddressLine2
		{
			get { return OA_Address2; }
		}

		ZString IAddressDetails.City
		{
			get { return OA_City; }
		}

		ZString IAddressDetails.State
		{
			get
			{
				var stateCode = OA_State;
				var relatedCountry = RelatedCountry;
				if (relatedCountry != null)
				{
					var refCountryState = new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCodeOrDesc(stateCode, relatedCountry.Code);
					if (refCountryState != null)
					{
						stateCode = refCountryState.RW_Code;
					}
				}
				return stateCode;
			}
		}

		ZString IAddressDetails.PostCode
		{
			get { return OA_PostCode; }
		}

		ZString IAddressDetails.Country
		{
			get
			{
				var result = OA_RL_NKRelatedPortCode.Left(2);

				if (result.IsEmpty && !OA_RN_NKCountryCode.IsEmpty)
				{
					result = OA_RN_NKCountryCode;
				}

				if (result.IsEmpty && EffectiveRelatedPortCode != null)
				{
					result = EffectiveRelatedPortCode.RL_Code.Left(2);
				}

				return result;
			}
		}

		#endregion

		#region IContactable Members

		ZGuid IContactable.PK
		{
			get { return PK; }
		}

		string IContactBase.Name
		{
			get { return Header.OH_FullNameTruncated; }
		}

		string IContactBase.Email
		{
			get { return OA_Email; }
		}

		string IContactable.Mobile
		{
			get { return OA_Mobile; }
		}

		bool IContactable.IsActive
		{
			get { return OA_IsActive; }
		}

		IContactable[] IContactable.GetNestedContacts(string parentContactDescription)
		{
			return Array.Empty<IContactable>();
		}

		#endregion

		#region IOrganisationAddressForMatching Members

		void IMatchingAddress.SetMainAddress()
		{
		}

		#endregion

		#region IOrgAddressForMatching Members

		public ZString PortName
		{
			get { return portNameCache != null ? portNameCache.Value : GetPortNameCore(); }
		}

		ZString GetPortNameCore()
		{
			RefUNLOCO port = EffectiveRelatedPortCode;
			return (port != null) ? port.RL_PortName : ZString.Empty;
		}

		public MultilingualString CountryName
		{
			get { return countryNameCache != null ? countryNameCache.Value : GetCountryNameCore(); }
		}

		MultilingualString GetCountryNameCore()
		{
			RefUNLOCO port = EffectiveRelatedPortCode;
			return (port != null && port.Country != null) ? port.Country.RN_DescMultilingual : (NoResString)string.Empty;
		}

		ZString IMatchingAddress.Contact { get; set; }

		ZString IMatchingAddress.CountryCode { get; set; }

		IDisposable IMatchingAddress.CachePortAndCountryNames()
		{
			portNameCache = new CachedValue<ZString>(GetPortNameCore);
			countryNameCache = new CachedValue<MultilingualString>(GetCountryNameCore);

			return new DisposableAction(
				() =>
				{
					portNameCache = null;
					countryNameCache = null;
				});
		}

		CachedValue<ZString> portNameCache;
		CachedValue<MultilingualString> countryNameCache;

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get
			{
				return !IsMainAddress && !IsReferencedByContact;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsMainAddress)
				{
					return ResString.GetMultilingualString("bce60b24-5844-419b-ba72-1f593ff4b1cf", "Cannot delete main address.");
				}

				if (IsReferencedByContact)
				{
					var refContacts =
						Header.Contacts.OfType<OrgContact>()
							.Where(c => c.OC_OA_OrgAddress == PK)
							.Select(
								c =>
									ResString.GetMultilingualString("C2BD5107-8A91-46DE-8998-5BB481E79E4D", "{0}({1})", c.OC_ContactName,
										c.OC_IsActive
											? Res.GetString("73A6E277-A6E3-434E-85F0-4994885389FF", "Active")
											: Res.GetString("DD2DC800-D0F2-41FC-9406-521E60E14339", "Inactive")));

					return ResString.GetMultilingualString("98C603A5-8294-4623-AF7B-4ED394C0491E",
						"This address cannot be deleted as it is referenced by one or more contacts: \r\n{0}",
						ListFormatter.GetCommaSeparatedText(refContacts));
				}

				return (NoResString)string.Empty;
			}
		}

		#endregion

		#region Universal Copy

		protected void FinishUniversalCopy()
		{
			if (fAddressCapability != null)
			{
				UnRegisterEditableChildObject(fAddressCapability);
				fAddressCapability = null;
			}
		}

		#endregion

		#region OA_GeofencePolygon override WI00100952
		//DBNull is used for flagging invalid values for ZTypes, but we are moving towards non-null SqlGeography columns. For now, if it's invalid, it was likely a db null

		public override ZGeography OA_GeofencePolygon
		{
			get
			{
				if (!base.OA_GeofencePolygon.IsValid)
				{
					return ZGeography.Empty;
				}
				else
				{
					return base.OA_GeofencePolygon;
				}
			}
			set => base.OA_GeofencePolygon = value;
		}

		#endregion

		bool ISupportDataImporting.IsImportingData { get; set; }

		#region ISupportWebAddressValidation

		public ZString AddressRecordGUID
		{
			get { return PK.ToString(); }
		}

		public ZString AddressSourceTable
		{
			get { return OrgAddressSchema.Constants.Prefix; }
		}

		[List("AdditionalAddressInfoList")]
		[DocumentFieldExcludeFromMap]
		public ZString UnrestrictedAdditionalAddressInformation
		{
			get
			{
				if (string.IsNullOrEmpty(unrestrictedAdditionalAddressInformation))
				{
					unrestrictedAdditionalAddressInformation = PrimaryOrgAddressAdditionalInfoDetail;
				}

				return unrestrictedAdditionalAddressInformation;
			}

			set
			{
				value = value.TrimEndSpaceTab();

				if (unrestrictedAdditionalAddressInformation != value)
				{
					unrestrictedAdditionalAddressInformation = value;
					if (!HasChanges)
					{
						HasChanges = value != PrimaryOrgAddressAdditionalInfoDetail;
					}

					if (value.Length <= OrgAddressAdditionalInfoSchema.OAI_AdditionalInfo.MaxLength)
					{
						PrimaryOrgAddressAdditionalInfoDetail = value;
					}
				}

				UnrestrictedAdditionalAddressInformationInfo.RefreshBinding();
			}
		}

		ZString unrestrictedAdditionalAddressInformation;

		public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo => GetZPropertyInfo(nameof(UnrestrictedAdditionalAddressInformation));

		public OrgAddressAdditionalInfo PrimaryOrgAddressAdditionalInfo => AdditionalInfos.FirstOrDefault(a => a.OAI_IsPrimary);

		public ZString PrimaryOrgAddressAdditionalInfoDetail
		{
			get
			{
				return PrimaryOrgAddressAdditionalInfo?.OAI_AdditionalInfo ?? ZString.Empty;
			}
			set
			{
				if (PrimaryOrgAddressAdditionalInfoDetail != value)
				{
					if (string.IsNullOrEmpty(value))
					{
						PrimaryOrgAddressAdditionalInfo?.Delete();
						base.OA_AdditionalAddressInformation = ZString.Empty;
					}
					else if (PrimaryOrgAddressAdditionalInfo == null)
					{
						var newAdditionalInfo = AdditionalInfos.AddNew();
						newAdditionalInfo.OAI_AdditionalInfo = value;
						newAdditionalInfo.OAI_IsPrimary = true;
						OA_AdditionalAddressInformation = value;
					}
					else
					{
						var existingOrgAddress = additionalInfos?.FirstOrDefault(a => a.OAI_AdditionalInfo == value);

						if (existingOrgAddress != null)
						{
							AdditionalInfos.Except(existingOrgAddress).ForEach(a => a.OAI_IsPrimary = false);
							existingOrgAddress.OAI_IsPrimary = true;
						}
						else if (string.IsNullOrEmpty(PrimaryOrgAddressAdditionalInfo.OAI_AdditionalInfo))
						{
							PrimaryOrgAddressAdditionalInfo.OAI_AdditionalInfo = value;
						}
						else
						{
							PrimaryOrgAddressAdditionalInfo.OAI_IsPrimary = false;
							var newAdditionalInfo = AdditionalInfos.AddNew();
							newAdditionalInfo.OAI_AdditionalInfo = value;
							newAdditionalInfo.OAI_IsPrimary = true;
						}
						OA_AdditionalAddressInformation = value;
					}
				}
			}
		}

		public int PrimaryOrgAddressAdditionalInfoCount => AdditionalInfos.Count(a => a.OAI_IsPrimary);

		public CodeDescriptionPairList AdditionalAddressInfoList => AdditionalInfos.GetAsCodeDescriptionPair();

		public ZString AddressCode
		{
			get { return OA_Code; }
			set { OA_Code = value; }
		}

		public ZString Address1
		{
			get { return OA_Address1; }
			set { OA_Address1 = value; }
		}

		public int Address1MaxLength
		{
			get { return AutoOrgAddress.Schema.OA_Address1MaxLength; }
		}

		public ZString Address2
		{
			get { return OA_Address2; }
			set { OA_Address2 = value; }
		}

		public int Address2MaxLength
		{
			get { return AutoOrgAddress.Schema.OA_Address2MaxLength; }
		}

		public ZString City
		{
			get { return OA_City; }
			set { OA_City = value; }
		}

		public int CityMaxLength
		{
			get { return AutoOrgAddress.Schema.OA_CityMaxLength; }
		}

		[BusinessObjectTestExclude]
		public ZString State
		{
			get
			{
				if (string.IsNullOrWhiteSpace(OA_State_List_For_Current_Address.GetDescriptionFromCode(OA_State)))
				{
					return OA_State;
				}
				return OA_State_List_For_Current_Address.GetDescriptionFromCode(OA_State);
			}
			set
			{
				var code = (ZString)OA_State_List_For_Current_Address.GetCodeFromDescription(value);
				OA_State = string.IsNullOrEmpty(code) ? value.Length > State_MaxLength ? ZString.Empty : value : code;
				StateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		public int State_MaxLength
		{
			get { return AutoOrgAddress.Schema.OA_StateMaxLength; }
		}

		public ZString StateCode
		{
			get { return OA_State; }
			set
			{
				OA_State = value;
				StateCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateCodeInfo
		{
			get { return OA_StateInfo; }
		}

		public int StateCode_MaxLength
		{
			get { return AutoOrgAddress.Schema.OA_StateMaxLength; }
		}

		public CodeDescriptionPairList StateCodeList
		{
			get { return OA_State_List_For_Current_Address; }
		}

		public ZString Postcode
		{
			get { return OA_PostCode; }
			set { OA_PostCode = value; }
		}

		public int PostcodeMaxLength
		{
			get { return AutoOrgAddress.Schema.OA_PostCodeMaxLength; }
		}

		public ZString AddressMap
		{
			get { return OA_AddressMap; }
			set { OA_AddressMap = value; }
		}

		[List("CountryCodeList")]
		ZString ISupportWebAddressValidation.CountryCodeISO2
		{
			get { return OA_RN_NKCountryCode; }
			set { OA_RN_NKCountryCode = value; }
		}

		public int CountryCodeISO2_MaxLength => Schema.OA_RN_NKCountryCodeMaxLength;

		public RefCountryCollection CountryCodeList
		{
			get { return Lookups.Countries; }
		}

		[List("LanguageList")]
		public ZString Language
		{
			get { return OA_Language; }
			set { OA_Language = value; }
		}

		public ZString ValidationStatus
		{
			get
			{
				return OA_ValidationStatus;
			}
			set { OA_ValidationStatus = value; }
		}

		public ZString Addressee
		{
			get
			{
				ZString addressee;
				if (OA_CompanyNameOverride.IsEmpty && Header != null)
				{
					addressee = Header.OH_FullName;
				}
				else
				{
					addressee = OA_CompanyNameOverride;
				}

				return addressee;
			}
		}

		public ZGeography GeoLocation
		{
			get => OA_GeoLocation;
			set => OA_GeoLocation = value;
		}

		public ZDecimal Longitude
		{
			get { return OA_Longitude; }
		}

		public ZDecimal Latitude
		{
			get { return OA_Latitude; }
		}

		public ZString ClosestPort
		{
			get { return OA_RL_NKRelatedPortCode; }
			set { OA_RL_NKRelatedPortCode = value; }
		}

		bool isManuallyVerifiedByUser;

		public AddressValidationSection ValidationSection => IsInAdminPanel ? AddressValidationSection.AdminPanel : AddressValidationSection.OrganizationAddress;

		public bool IsInAdminPanel { get; set; }

		public void ResetValidationStatus(ZPropertyInfo propertyInfo)
		{
			if (isManuallyVerifiedByUser)
			{
				return;
			}

			if (Env.Registry.EnableAddressValidationWebService)
			{
				if (ValidationStatus != AddressValidationStatus.CountryNotAvailable && Country != null && OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Country.PK.ToGuid(), ValidationSection))
				{
					OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
					OA_AddressMap = string.Empty;
					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					if (!string.IsNullOrEmpty(registrationKey.SystemId))
					{
						RaiseWebServices(propertyInfo);
					}
				}
				else if (propertyInfo.Name == nameof(OA_RN_NKCountryCode))
				{
					OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
					OA_AddressMap = string.Empty;
				}
			}
		}

		public

#if DEBUG
		virtual
#endif

		async Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, WTG.AddressCleansing.Common.CleanseAction cleanseAction = WTG.AddressCleansing.Common.CleanseAction.ValidateAndSuggest)
		{
			return await AddressValidationService.ValidateAddressAsync(this, cancellationToken, false, cleanseAction);
		}

		public async Task<WTG.AddressCleansing.Common.CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken)
		{
			return await AddressValidationService.GetCityTownAsync(this, cancellationToken);
		}

		public bool IsErrorSuppressed => OA_SuppressAddressValidationError;
		public bool IsJobDocAddress => false;

		public event EventHandler AddressValidationStatusChanged;
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

		void RaiseAddressValidationStatusChanged()
		{
			if (AddressValidationStatusChanged != null)
			{
				AddressValidationStatusChanged(this, EventArgs.Empty);
			}
		}

		void RaiseWebServices(ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(OA_RN_NKCountryCode))
			{
				if ((string.IsNullOrEmpty(OA_City) || string.IsNullOrEmpty(OA_State) || string.IsNullOrEmpty(OA_PostCode)) && (propertyInfo == OA_CityInfo || propertyInfo == OA_StateInfo || propertyInfo == OA_PostCodeInfo))
				{
					RaiseTriggerWebGetCityTown(propertyInfo);
				}
				else
				{
					RaiseTriggerWebAddressValidation(propertyInfo);
				}
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
				var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, OA_RN_NKCountryCode);
				if (country != null)
				{
					if (!OA_Address1.IsEmpty && !OA_PostCode.IsEmpty && !OA_City.IsEmpty && !OA_State.IsEmpty)
					{
						if (!IsInDatabase || (OA_Address1Info.HasChanges || OA_Address2Info.HasChanges || OA_PostCodeInfo.HasChanges ||
												OA_CityInfo.HasChanges || OA_StateInfo.HasChanges || OA_RN_NKCountryCodeInfo.HasChanges))
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
			Validation.ValidateOA_Address1();
			Validation.ValidateOA_City();
			Validation.ValidateOA_RN_NKCountryCode();
			ValidatePostcodeAndStateForAddress();
		}

		public void ValidatePostcodeAndStateForAddress()
		{
			Validation.ValidateOA_PostCode();
			Validation.ValidateOA_State();
		}

		public bool IsUpdatingCityTown { get; set; }
		public bool IsValidatingAddress { get; set; }
		public bool IsExactPointFound { get; set; }
		public bool HasBeenChangedByUser { get; set; }
		public bool IsValidatedByBackgroundService { get; set; }

		#endregion

		#region ILocation Members

		ZString ILocation.Code
		{
			get { return ZString.Empty; }
		}

		ZString ILocation.Description
		{
			get { return ZString.Empty; }
		}

		ZBool ILocation.IsActive
		{
			get { return OA_IsActive; }
		}

		RefCityTown ILocation.CityTown
		{
			get { return this.GetCityTown(OA_City, Factory); }
		}

		RefCountry ILocation.Country
		{
			get { return Country; }
		}

		RefCountryStates ILocation.State
		{
			get { return RelatedState; }
		}

		RefUNLOCO ILocation.UNLOCO
		{
			get { return RelatedPortCode; }
		}

		IATACityCode ILocation.IATACityCode => RelatedPortCode?.IATACityCode;

		RefZoneHeader[] ILocation.Zones
		{
			get
			{
				return ((ILocation)RelatedPortCode)?.Zones ?? Array.Empty<RefZoneHeader>();
			}
		}

		bool ILocationReference.IsLocalInRelationTo(ZString code)
		{
			return false;
		}

		#endregion

		internal bool OA_JobLoadingDuration_ReadOnly => !IsOverridenJobLoadingDuration;

		public ZPropertyInfo PostcodeInfo
		{
			get { return OA_PostCodeInfo; }
		}

		public int Postcode_MaxLength
		{
			get { return Schema.OA_PostCodeMaxLength; }
		}

		public ZString CompanyName
		{
			get { return OA_CompanyNameOverride.IsEmpty && Header != null ? Header.OH_FullName : OA_CompanyNameOverride; }
			set { OA_CompanyNameOverride = value; }
		}

		public ZString RelatedPortCodeWithFallback
		{
			get { return OA_RL_NKRelatedPortCode.IsEmpty && Header != null ? Header.OH_RL_NKClosestPort : OA_RL_NKRelatedPortCode; }
			set { OA_RL_NKRelatedPortCode = value; }
		}

		public ZString DisplayText
		{
			get { return OA_Code; }
			set { OA_Code = value; }
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return OA_LanguageInfo; }
		}

		public ZPropertyInfo Address1Info
		{
			get { return OA_Address1Info; }
		}

		public ZPropertyInfo Address2Info
		{
			get { return OA_Address2Info; }
		}

		public ZPropertyInfo CityInfo
		{
			get { return OA_CityInfo; }
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return OA_CompanyNameOverrideInfo; }
		}

		public ZGuid EntityPK
		{
			get { return PK; }
		}

		public int Language_MaxLength
		{
			get { return Schema.OA_LanguageMaxLength; }
		}

		public int Address1_MaxLength
		{
			get { return Schema.OA_Address1MaxLength; }
		}

		public int Address2_MaxLength
		{
			get { return Schema.OA_Address2MaxLength; }
		}

		public int City_MaxLength
		{
			get { return Schema.OA_CityMaxLength; }
		}

		public int PostCode_MaxLength
		{
			get { return Schema.OA_PostCodeMaxLength; }
		}

		public int CompanyName_MaxLength
		{
			get { return Schema.OA_CompanyNameOverrideMaxLength; }
		}

		#region Multiple Language

		/// <summary>
		/// This property is only used for data binding purpose
		/// </summary>
		public List<ISupportWebAddressValidation> AddressLanguagePack
		{
			get
			{
				if (addressLanguagePack == null)
				{
					addressLanguagePack = new List<ISupportWebAddressValidation>();
					addressLanguagePack.Add(this);
					addressLanguagePack.AddRange(TranslatedAddresses);
				}
				return addressLanguagePack;
			}
		}

		List<ISupportWebAddressValidation> addressLanguagePack;

		internal void CheckNoDuplicateLocalAddress(ISupportWebAddressValidation address, ZPropertyInfo propertyInfo)
		{
			if (AddressLanguagePack.Exists(a => a.EntityPK != address.EntityPK && a.Language == address.Language))
			{
				propertyInfo.AddError(ResString.GetMultilingualString("93A03F57-8433-4329-9D7B-5A3F2D66DC4F", "Address already has another local translation in this language."));
			}
		}

		public ISupportWebAddressValidation SelectedTranslatedAddress
		{
			get
			{
				if (selectedTranslatedAddress == null)
				{
					SetSelectedTranslatedAddress(this);
				}
				return selectedTranslatedAddress;
			}
			set
			{
				if (selectedTranslatedAddress != value)
				{
					SetSelectedTranslatedAddress(value);
				}
			}
		}
		ISupportWebAddressValidation selectedTranslatedAddress;

		void SetSelectedTranslatedAddress(ISupportWebAddressValidation address)
		{
			selectedTranslatedAddress = address;
			LocalAddressDisplayText = address.DisplayText;
			RefreshBinding();
		}

		[BusinessObjectTestExclude] //Setter only works when value is valid
		public ZString LocalAddressDisplayText
		{
			get { return localAddressDisplayText; }
			set
			{
				if (localAddressDisplayText != value)
				{
					var localAddress = AddressLanguagePack.FirstOrDefault(a => a.DisplayText.EqualsIgnoringCase(value));
					if (localAddress != null)
					{
						localAddressDisplayText = value;
						SelectedTranslatedAddress = localAddress;
					}
				}
				LocalAddressDisplayTextInfo.RefreshBinding();
				RefreshBinding();
			}
		}
		ZString localAddressDisplayText;

		public ZPropertyInfo LocalAddressDisplayTextInfo
		{
			get { return GetZPropertyInfo(nameof(LocalAddressDisplayText)); }
		}

		public bool IsEnglishOnlyOrEmpty
		{
			get { return OA_Address1.IsEnglishOnlyOrEmpty && OA_Address2.IsEnglishOnlyOrEmpty && OA_City.IsEnglishOnlyOrEmpty; }
		}

		public bool IsOriginalValueEnglishOnly
		{
			get
			{
				return ((ZString)OA_Address1Info.OriginalValue).IsEnglishOnlyOrEmpty &&
						 ((ZString)OA_Address2Info.OriginalValue).IsEnglishOnlyOrEmpty && ((ZString)OA_CityInfo.OriginalValue).IsEnglishOnlyOrEmpty;
			}
		}

		[ChildEditable(true)]
		public ActiveBusinessObjectCollection<OrgTranslatedAddress> TranslatedAddresses
		{
			get
			{
				if (translatedAddresses == null)
				{
					translatedAddresses = new ActiveBusinessObjectCollection<OrgTranslatedAddress>(this);
					RegisterEditableChildObject(translatedAddresses);
					//Register this to make the notification refresh
					((IBindingList)translatedAddresses).ListChanged += OrgAddress_ListChanged;
				}
				return translatedAddresses;
			}
		}

		ActiveBusinessObjectCollection<OrgTranslatedAddress> translatedAddresses;

		void OrgAddress_ListChanged(object sender, ListChangedEventArgs e)
		{
			RefreshBinding();
		}

		public OrgTranslatedAddress CurrentTranslatedAddress => SelectedTranslatedAddress as OrgTranslatedAddress;

		public OrgTranslatedAddress GetTranslatedAddressInSpecificLanguage(string language) => TranslatedAddresses.FirstOrDefault(a => a.OTA_Language == language);

		public OrgTranslatedAddress AddNewTranslatedAddress()
		{
			var languageCode = LanguageForNewTranslatedAddress();
			var newAddress = TranslatedAddresses.AddNew();
			if (languageCode != null)
			{
				newAddress.OTA_Language = languageCode;
			}
			AddressLanguagePack.Add(newAddress);
			SelectedTranslatedAddress = newAddress;
			return newAddress;
		}

		string LanguageForNewTranslatedAddress()
		{
			if (AddressLanguagePack.Count < LanguageList.Count)
			{
				foreach (ICodeDescription language in LanguageList)
				{
					if (!AddressLanguagePack.Exists(a => a.Language == language.Code))
					{
						return language.Code;
					}
				}
			}
			return null;
		}

		public void DeleteTranslatedAddress(OrgTranslatedAddress translatedAddress)
		{
			bool shouldRemoveSelectedTranslatedAddress = translatedAddress == SelectedTranslatedAddress;
			var localAddress = translatedAddress;
			AddressLanguagePack.Remove(localAddress);

			if (shouldRemoveSelectedTranslatedAddress)
			{
				SelectedTranslatedAddress = this;
			}
			localAddress.Delete();
		}

		#endregion

		public bool IsTSAKnownAddress
		{
			get
			{
				return Factory.Exists(typeof(OrgCountryData), new ZQuery()
				.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, Constants.CountryCodes.UnitedStates)
				.AddToFilter(OrgCountryDataSchema.OV_OA_ApprovedLocation, PK));
			}
		}

		public bool IsMIDAddress
		{
			get
			{
				return Factory.Exists(typeof(OrgCusCode), new ZQuery()
							 .AddToFilter(OrgCusCodeSchema.OK_CodeType, "MID")
							 .AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Constants.CountryCodes.UnitedStates)
							 .AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, this.PK));
			}
		}

		public ZString AddressFullFormatted
		{
			get
			{
				return new AddressFormatter(Factory, this).PostalAddress();
			}
		}

		#region On concurrency exception, suspend validation

		protected override void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			base.OnConcurrencyExceptionAfterMergeCore(propertyRecords);

			if (!isHasChangesChangedHooked)
			{
				isHasChangesChangedHooked = true;
				HasChangesChanged += OrgAddress_HasChangesChanged;
			}

			if (!IsValidationSuspended)
			{
				isValidationSuspendedAfterMergeConcurrency = true;
				SuspendValidation();
			}

			if (!IsDeleted && OA_SystemLastEditUser.Equals(User.ServiceUserCode))
			{
				var geoLocation = propertyRecords.FirstOrDefault(record => record.ColumnName == OrgAddressSchema.OA_GeoLocation.Name);
				if (geoLocation != null)
				{
					OA_GeoLocation = new ZGeography(geoLocation.CurrentValue);
				}

				var validationStatus = propertyRecords.FirstOrDefault(record => record.ColumnName == OrgAddressSchema.OA_ValidationStatus.Name);
				if (validationStatus != null)
				{
					OA_ValidationStatus = validationStatus.CurrentValue.ToString();
				}
			}
		}

		void OrgAddress_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (isValidationSuspendedAfterMergeConcurrency && e.ObjectJustWasChanged)
			{
				isValidationSuspendedAfterMergeConcurrency = false;
				ResumeValidation();
			}
		}

		bool isHasChangesChangedHooked;
		bool isValidationSuspendedAfterMergeConcurrency;

		#endregion

		public static ZString ConvertAddressToAnalysisText(OrgAddress orgAddress)
		{
			if (orgAddress == null)
			{
				return ZString.Empty;
			}

			var result = new ZStringBuilder();
			result.AppendLine("InputAddress1: " + orgAddress.OA_Address1);
			result.AppendLine("InputAddress2: " + orgAddress.OA_Address2);
			result.AppendLine("InputCity: " + orgAddress.OA_City);
			result.AppendLine("InputPostcode: " + orgAddress.OA_PostCode);
			result.AppendLine("InputState: " + orgAddress.OA_State);
			result.AppendLine("InputCountryCode: " + orgAddress.OA_RN_NKCountryCode);

			return result.ToString();
		}

		#region Find Duplicates

		void FindDuplicatesWhenAddressValidationDisabled(ZString value)
		{
			if (!value.IsEmpty && !OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Country?.PK.ToGuid() ?? Guid.Empty, ValidationSection))
			{
				Header?.FindDuplicates();
			}
		}

		#endregion

		public ZString Address1AndAddress2
		{
			get
			{
				return new ZStringBuilder()
					.AppendIfNotEmpty(Address1)
					.AppendIfNotEmpty(Address2)
					.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		#region IEInvoicingEligibilityLiteOrgAddress

		ZString IEInvoicingEligibilityLiteOrgAddress.CountryCode => OA_RN_NKCountryCode;

		[WTG.StaticAnalysis.Annotation.Immutable]
		internal class EmptyIEInvoicingEligibilityLiteOrgAddress : IEInvoicingEligibilityLiteOrgAddress
		{
			public static readonly EmptyIEInvoicingEligibilityLiteOrgAddress Value = new EmptyIEInvoicingEligibilityLiteOrgAddress();

			ZString IEInvoicingEligibilityLiteOrgAddress.CountryCode => ZString.Empty;
		}

		#endregion
	}
}
