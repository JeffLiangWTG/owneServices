using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Core.Environment;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.US;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.MasterFiles.Business
{
	[UniversalDataContext(DataContextType.Organization)]
	[UniversalCopyWithExtendedEntities]
	[UniversalCopyIgnoreElement("Addresses")]
	[UniversalCopyAssociateElement("OrgAddresses", "AddressesNoAutoCreate")]
	[UserDefinedValues]
	[DescriptionProperty(AutoOrgHeader.Schema.OH_FullName)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[DebuggerDisplay("PK = {PK}, FullName = {OH_FullName}, Code = {OH_Code}")]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class OrgHeader : AutoOrgHeader,
		IOrgHeader,
		IJobNumber,
		IContactable,
		INoteSource,
		ISendEmailSource,
		ICustomLabelsProvider,
		ICustomLabelsConfigOrgProvider,
		ICustomFieldProvider,
		IDocumentSupportable,
		IAddressDetails,
		IHaveRequiredDocumentsWithAttributes,
		IScreeningPartyProvider,
		ICodeDescription,
		IOrgHeaderForMatching,
		IOrgHeaderWithAddressesNoAutoCreate,
		IWorkflowProviderIncludingRelated,
		ICanBeExcludedFromOperationalActions,
		ICustomPropertyContainer,
		ICusAddInfoTypeSupporter,
		ICusCodeDataTypeSupporter,
		ISalesValueAssociatedEntity,
		IConversationParticipant,
		IAuditParent,
		IDeduplicatable,
		IPatternMatchingRegenerationEntities<OrgHeader>,
		IEnrichmentDataProvider,
		IEInvoicingEligibilityLiteOrgHeader,
		IDocAddresses,
		IDpsEntityProvider,
		IAutoRateDateByChargeGroupConfiguration,
		IRegisterStatusChangeContext,
		IViewStmNumsOwner,
		IUSOrgHeader
	{
		#region Schema

		public new class Schema : AutoOrgHeader.Schema
		{
			public const string CustomsClientID = "CustomsClientID";
			public const string OpportunityDateFrom = "OpportunityDateFrom";
			public const string OpportunityDateTo = "OpportunityDateTo";
			public const string OpportunitiesDateTypeToFilter = "OpportunitiesDateTypeToFilter";
			public const string DateOfCallFrom = "DateOfCallFrom";
			public const string DateOfCallNoteFrom = "DateOfCallNoteFrom";
			public const string DateOfCallTo = "DateOfCallTo";
			public const string DateOfCallNoteTo = "DateOfCallNoteTo";
			public const string DateNextCallFrom = "DateNextCallFrom";
			public const string DateFollowUpFrom = "DateFollowUpFrom";
			public const string DateNextCallTo = "DateNextCallTo";
			public const string DateFollowUpTo = "DateFollowUpTo";
			public const string CallContact = "CallContact";
			public const string CallNoteContact = "CallNoteContact";
			public const string CallSalesRep = "CallSalesRep";
			public const string CallingStaff = "CallingStaff";
			public const string CallDirection = "CallDirection";
			public const string CallLocation = "CallLocation";
			public const string CallStatus = "CallStatus";
			public const string CollectionCallStatus = "CollectionCallStatus";
			public const string CallDisposition = "CallDisposition";
			public const string CollectionCallDisposition = "CollectionCallDisposition";
			public const string CompetitorDetailPrefix = "CompetitorDetail";
			public const string SourceOfLeadPK = "SourceOfLeadPK";
			public const string ControllingAgentPK = "ControllingAgentPK";
			public const string APSettlementGroupPK = "APSettlementGroupPK";
			public const string ARSettlementGroupPK = "ARSettlementGroupPK";
			public const string ExternalValidationStatus = "ExternalValidationStatus";
			public const string CAAccountSecurityNumber = "CAAccountSecurityNumber";
			public const string ImporterBondQueryDate = "ImporterBondQueryDate";
			public const int OH_FullNameTruncatedLength = 50;
			public const string ExportersBankName = "ExportersBankName";
			public const string ExportersBankAccount = "ExportersBankAccount";
			public const string ExportersSwiftCode = "ExportersSwiftCode";
			public const string MethodOfPayment = "MethodOfPayment";
			public const string AdditionalInformation = "AdditionalInformation";
			public const string EmployerIdentificationNumber = "EmployerIdentificationNumber";
			public const string PowerOfAttorneyValidToDate = "PowerOfAttorneyValidToDate";
		}

		public static class OHConstants
		{
			public const string OH_IsDebtor = "OH_IsDebtor";
			public const string OH_IsCreditor = "OH_IsCreditor";
		}

		#endregion

		public OrgHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Required Fields

		CachedProperty<OrgRequiredFields> requiredFieldsForOrgCache;

		internal OrgRequiredFields RequiredFieldsForOrg
		{
			get
			{
				if (requiredFieldsForOrgCache == null)
				{
					requiredFieldsForOrgCache = new CachedProperty<OrgRequiredFields>(Factory, GetRequiredFields);
				}

				return requiredFieldsForOrgCache.Value;
			}
		}

		OrgRequiredFields GetRequiredFields()
		{
			OrgRequiredFields fields = new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, false);
			if (!IsDeleted)
			{
				if (OH_IsTempAccount)
				{
					if (OH_IsConsignee)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgConsigneeRequiredFields());
					}

					if (OH_IsConsignor)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgConsignorRequiredFields());
					}

					if (OH_IsCreditor)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgCreditorRequiredFields());
					}

					if (OH_IsDebtor)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgDebtorRequiredFields());
					}

					if (OH_IsSalesLead)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetTempOrgSalesRequiredFields());
					}
				}
				else
				{
					if (OH_IsBroker)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgBrokerRequiredFields());
					}

					if (OH_IsShippingProvider)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgCarrierRequiredFields());
					}

					if (OH_IsCompetitor)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgCompetitorRequiredFields());
					}

					if (OH_IsConsignee)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgConsigneeRequiredFields());
					}

					if (OH_IsConsignor)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgConsignorRequiredFields());
					}

					if (OH_IsContainerYard)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgContainerYardRequiredFields());
					}

					if (OH_IsCreditor)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgCreditorRequiredFields());
					}

					if (OH_IsAirCTO || OH_IsSeaCTO)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgCTORequiredFields());
					}

					if (OH_IsDebtor)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgDebtorRequiredFields());
					}

					if (OH_IsForwarder)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgForwarderRequiredFields());
					}

					if (OH_IsPackDepot)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgPackDepotRequiredFields());
					}

					if (OH_IsSalesLead)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgSalesLeadRequiredFields());
					}

					if (OH_IsTransportClient)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgTransportClientRequiredFields());
					}

					if (OH_IsWarehouseClient)
					{
						fields = MergeRequiredFields(fields, Env.Registry.GetOrgWarehouseRequiredFields());
					}
				}
			}

			return fields;
		}

		OrgRequiredFields MergeRequiredFields(OrgRequiredFields fields, OrgRequiredFields fieldsToMerge)
		{
			bool address2 = fields.RequireAddress2 || fieldsToMerge.RequireAddress2;
			bool branch = fields.RequireBranch || fieldsToMerge.RequireBranch;
			bool city = fields.RequireCity || fieldsToMerge.RequireCity;
			bool phone = fields.RequirePhoneNumber || fieldsToMerge.RequirePhoneNumber;
			bool gst = fields.RequireBusinessNumber || fieldsToMerge.RequireBusinessNumber;
			bool gstOrPhone = fields.RequirePhoneOrBusinessNumber || fieldsToMerge.RequirePhoneOrBusinessNumber;
			bool fax = fields.RequireFaxNumber || fieldsToMerge.RequireFaxNumber;
			bool email = fields.RequireEmailAddress || fieldsToMerge.RequireEmailAddress;
			bool web = fields.RequireWebAddress || fieldsToMerge.RequireWebAddress;
			bool faxEmailOrWeb = fields.RequireFaxEmailOrWeb || fieldsToMerge.RequireFaxEmailOrWeb;
			bool requireARContact = fields.RequireARContact || fieldsToMerge.RequireARContact;

			return new OrgRequiredFields(address2, branch, city, phone, gst, gstOrPhone, fax, email, web, faxEmailOrWeb, requireARContact);
		}

		#endregion

		#region Load / Find
		public static OrgHeader DefaultOrg
		{
			get
			{
				//Use GetDefaultOrg() instead when under multithreading environment. Global variable GlbCompany.CurrentCompany cannot be used share between different threads
				return GetDefaultOrgFromCompany(GlbCompany.CurrentCompany);
			}
		}

		public static OrgHeader GetDefaultOrg(BusinessObjectFactory factory)
		{
			return GetDefaultOrgFromCompany(GlbCompany.GetCurrentCompany(factory));
		}

		static OrgHeader GetDefaultOrgFromCompany(GlbCompany company)
		{
			if (company == null)
			{
				return null;
			}
			return company.OrgProxy;
		}

		public static OrgHeader GetCompanyOrgProxyFromCompanyCode(BusinessObjectFactory factory, ZString companyCode)
		{
			ZDBOnlyQuery orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.GC_OH_OrgProxy);
			companyQuery.AddToFilter(GlbCompanySchema.GC_Code, companyCode);
			orgQuery.AddSubQuery(OrgHeaderSchema.PK, companyQuery, JoinCondition.And);

			var organizations = factory.Load<OrgHeader>(orgQuery);
			return organizations.FirstOrDefault();
		}

		public static OrgHeader New(BusinessObjectFactory factory)
		{
			return factory.New<OrgHeader>();
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public OrgHeader LoadFromLegacyCode(ZString countryCode, ZString legacyCode)
			{
				OrgCusCode orgCusCode = new OrgCusCode.Loader(Factory).LoadFromLegacyCode(countryCode, legacyCode);
				return orgCusCode != null ? orgCusCode.Header : null;
			}

			public OrgHeader[] LoadDBOrganisations(ZString countryCode, ZString codeType, ZString regoNumber)
			{
				var subquery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				subquery.AddToFilter(OrgCusCode.Loader.GetQuery(countryCode, codeType, regoNumber));
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddSubQuery(subquery, JoinCondition.And);

				return Factory.Load<OrgHeader>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(OrgHeader);
			}
		}

		public static OrgHeader LoadFromCode(BusinessObjectFactory factory, ZString orgCode)
		{
			return (OrgHeader)factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, orgCode);
		}

		public static OrgHeader LoadFromForeignCode(BusinessObjectFactory factory, ZString foreignCode, OrgHeader org)
		{
			OrgHeader result = null;

			ZQuery orgFilter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, org.PK);
			orgFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Organisation);
			orgFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignCode);

			OrgPatternMatchOverride[] orgPatternMatchOverrides = factory.Load<OrgPatternMatchOverride>(orgFilter);
			if (orgPatternMatchOverrides.Length == 1)
			{
				OrgPatternMatchOverride orgPatternMatch = orgPatternMatchOverrides[0];
				result = factory.Load<OrgHeader>(orgPatternMatch.OO_LocalGuid);
			}

			return result;
		}

		public static OrgHeader Find(BusinessObjectFactory factory, string exactName, string port, string address)
		{
			OrgHeader result = null;

			ZQuery orgFilter = new ZQuery(OrgHeaderSchema.OH_FullName, exactName);
			orgFilter.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, port);
			OrgHeader[] organisations = (OrgHeader[])factory.Load(typeof(OrgHeader), orgFilter);

			foreach (OrgHeader organisation in organisations)
			{
				ZQuery checkAddressTooFilter = new ZQuery(OrgAddressSchema.OA_OH, organisation.PK);
				checkAddressTooFilter.AddToFilter(OrgAddressSchema.OA_Address1, address);
				OrgAddress organisationAddress = (OrgAddress)factory.LoadTop1(typeof(OrgAddress), checkAddressTooFilter);

				if (organisationAddress != null)
				{
					result = organisation;
					break;
				}
			}

			return result;
		}

		public static OrgHeader FindByAccountID(BusinessObjectFactory factory, string accountID)
		{
			return FindByOrgCusCode(factory, OrgCusCode.CodeTypes.LegacySystemCode, accountID);
		}

		public static OrgHeader FindByOrgCusCode(BusinessObjectFactory factory, string externalCodeType, string externalCode)
		{
			return FindByOrgCusCode(factory, externalCodeType, externalCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static OrgHeader FindByOrgCusCode(BusinessObjectFactory factory, string externalCodeType, string externalCode, string country)
		{
			OrgHeader result = null;

			ZQuery accountFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, externalCodeType);
			accountFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, country);
			accountFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, externalCode);
			OrgCusCode externalCodeRef = (OrgCusCode)factory.LoadTop1(typeof(OrgCusCode), accountFilter);

			if (externalCodeRef != null)
			{
				result = (OrgHeader)factory.Load(typeof(OrgHeader), externalCodeRef.OK_OH);
			}

			return result;
		}

		public static OrgHeader FindBy3CharAirlineCode(BusinessObjectFactory factory, ZString airlineCode)
		{
			var subQueryRefAirline = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
			subQueryRefAirline.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, airlineCode);

			var queryOrgMiscServ = new ZDBOnlyQuery(typeof(OrgMiscServ));
			queryOrgMiscServ.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, subQueryRefAirline, JoinCondition.And);

			var miscServ = factory.LoadTop1<OrgMiscServ>(queryOrgMiscServ);
			return miscServ?.Header;
		}

		#endregion

		#region Validation

		protected override OrgHeaderValidation GetNewValidation()
		{
			return new OrgHeaderValidationReal(this);
		}

		public new OrgHeaderValidationReal Validation
		{
			get { return (OrgHeaderValidationReal)base.Validation; }
		}

		internal void SetOrgTypeAsExpectedAndValidate(params ZPropertyInfo[] infos)
		{
			ExpectedOrgTypeInfos.AddRange(infos);
			foreach (ZPropertyInfo info in infos)
			{
				((IBusinessObjectInternals)this).Validate(info);
			}
			ExpectedOrgTypeInfos.Clear();
		}

		internal List<ZPropertyInfo> ExpectedOrgTypeInfos = new List<ZPropertyInfo>();

		internal void SetAtleastOneOrgTypeAsExpectedAndValidate(params ZPropertyInfo[] infos)
		{
			ExpectedAtleastOneOrgTypeInfos.AddRange(infos);
			foreach (ZPropertyInfoBool info in infos)
			{
				((IBusinessObjectInternals)this).Validate(info);
			}
			ExpectedAtleastOneOrgTypeInfos.Clear();
		}

		internal List<ZPropertyInfo> ExpectedAtleastOneOrgTypeInfos = new List<ZPropertyInfo>();

		protected override void RunPreSaveValidationCore()
		{
			if (shouldSetDefaultRelatedParties)
			{
				SetDefaultRelatedParties();
			}
			if (shouldSetDefaultOrgSecurities)
			{
				SetDefaultOrgSecurities();
			}

			if (!IsLoadedFromCollectionCall && !ValidationSuspendedForOrgHeader)
			{
				this.ClearValueCachedForValidation();
				base.RunPreSaveValidationCore();
			}

			ClearCollectionNotesFilterValues();
			EnsureSalesRepAddedOnOrg();
		}

		public void EnsureSalesRepAddedOnOrg()
		{
			var salesRepMandatory = (OH_IsTempAccount && RawDataRegistry.Instance.MakeSalesRepMandatoryForTempOrganization.Value)
				|| (!OH_IsTempAccount && OrganisationsDataRegistry.Instance.MakeSalesRepMandatory.Value);
			if (salesRepMandatory && (OH_IsSalesLead || OH_IsDebtor))
			{
				bool hasSalesRep = false;
				OrgStaffAssignmentsCollection collectionWithGlobal = new OrgStaffAssignmentsCollection(this);
				collectionWithGlobal.CompanySpecific = false;
				foreach (OrgStaffAssignments assignment in collectionWithGlobal)
				{
					if (assignment.O8_Role == StaffAssignmentRoles.Codes.SalesRep && (assignment.O8_GC == GlbCompany.CurrentCompany.PK || assignment.O8_GC == ZGuid.Empty))
					{
						hasSalesRep = true;
						break;
					}
				}

				if (!hasSalesRep)
				{
					OrgStaffAssignments newAssignment = StaffAssignments.AddNew();
					newAssignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
					newAssignment.ValidateResponsiblePerson();
				}
			}
		}

		public void PreValidateMainAddressForRegistry()
		{
			MainAddress.Validation.ValidateOA_Email();
			MainAddress.Validation.ValidateOA_Address2();
			MainAddress.Validation.ValidateOA_City();
			MainAddress.Validation.ValidateOA_Fax_Formatted();
			MainAddress.Validation.ValidateOA_Phone_Formatted();
			MainAddress.Validation.ValidateOA_PostCode();
			MainAddress.Validation.ValidateOA_State();
			MainWebURL.Validation.ValidatePU_URL();
			CompanyData.Validation.ValidateOB_GB_ControllingBranch();
			PrimaryRegistrationNumber.Validation.ValidateNumber();
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgHeaderFetchStrategy(this);
		}

		#endregion

		public ZString OH_CreatedUnderBranch
		{
			get
			{
				if (createdLog == null)
				{
					createdLog = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, PK)
						.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.AddedARecordToTheSystemCode)).FirstOrDefault();
				}

				return createdLog?.SL_GB_NKBranch ?? ZString.Empty;
			}
		}

		public ZString OH_CreatedUnderCompany
		{
			get
			{
				var result = ZString.Empty;
				if (!OH_CreatedUnderBranch.IsEmpty)
				{
					var glbBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, OH_CreatedUnderBranch);
					if (glbBranch != null)
					{
						result = glbBranch.Company.GC_Code;
					}
				}

				return result;
			}
		}

		StmALog createdLog;

		public static bool ReDefaultARAPTaxConfigurations(BusinessObjectFactory factory)
		{
			var result = false;
			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			var query = new ZQuery();
			query.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, true);
			query.AddToFilter(JoinCondition.Or, OrgCompanyDataSchema.OB_IsCreditor, true);
			orgCompanyDataQuery.AddToFilter(query);
			orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);

			var orgHeaders = factory.Load<OrgHeader>(orgHeaderQuery);

			foreach (var orgHeader in orgHeaders)
			{
				orgHeader.RedefaultTaxRecognition();
				result = true;
			}
			return result;
		}

		public void RedefaultTaxRecognition()
		{
			if (OH_IsDebtor)
			{
				CompanyData.RedefaultTaxRecognitionForAR(UNLOCO);
			}
			if (OH_IsCreditor)
			{
				CompanyData.RedefaultTaxRecognitionForAP(UNLOCO);
			}
		}

		public static OrgHeader UnmatchOrg(BusinessObjectFactory factory)
		{
			return factory.Load<OrgHeader>(UnmatchedOrganisationPK);
		}

		public static ZString UnmatchedOrganisationCode
		{
			get { return Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Code; }
		}

		public static ZGuid UnmatchedOrganisationPK
		{
			get
			{
				return OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			}
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OH_IsActive = true;
			OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			OH_Language = Core.Constants.Languages.English;
			SetValidation();
			shouldSetDefaultRelatedParties = true;
			shouldSetDefaultOrgSecurities = true;
			InitialiseOpportunityFilter();
			OriginalCollections.SetDefaultValues();
			SetDefaultRelatedPartiesFilter();
		}

		bool shouldSetDefaultRelatedParties;
		bool shouldSetDefaultOrgSecurities;

		void SetDefaultRelatedParties()
		{
			shouldSetDefaultRelatedParties = false;
			AddRelatedParty(OrganisationsDataRegistry.Instance.ImportSeaBroker.GetValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty, GlbCompany.CurrentCompany);
			AddRelatedParty(OrganisationsDataRegistry.Instance.ImportAirBroker.GetValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty, GlbCompany.CurrentCompany);
			AddRelatedParty(OrganisationsDataRegistry.Instance.ExportSeaBroker.GetValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty, GlbCompany.CurrentCompany);
			AddRelatedParty(OrganisationsDataRegistry.Instance.ExportAirBroker.GetValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty, GlbCompany.CurrentCompany);
		}

		void SetDefaultOrgSecurities()
		{
			shouldSetDefaultOrgSecurities = false;
			var defaultValue = OrganisationRegistry.Instance.WebSecurityDefaultValues.Value.OfType<OrgSecurityProfile>().FirstOrDefault(x => x.Default);
			if (defaultValue != null)
			{
				SecurityRights.SetOrgSecurities(defaultValue);
			}
		}

		void SetDefaultRelatedPartiesFilter()
		{
			this.FilterFreightDirection = ZString.Empty;
			this.FilterPartyType = ZString.Empty;
		}

		public void SetDefaultValuesForTemporaryOrganisation()
		{
			using (SuspendSettingHasChanges())
			{
				OH_IsTempAccount = true;
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetValidation();
			InitialiseOpportunityFilter();
			SetDefaultRelatedPartiesFilter();
		}

		internal PatternMatchingOriginalCollections originalCollections;
		public PatternMatchingOriginalCollections OriginalCollections
		{
			get
			{
				if (originalCollections == null)
				{
					originalCollections = new PatternMatchingOriginalCollections();
				}

				return originalCollections;
			}
		}
		void SetValidation()
		{
			if ((OrganisationsDataRegistry.Instance.MakeSalesRepMandatory.Value || RawDataRegistry.Instance.MakeSalesRepMandatoryForTempOrganization.Value) && EnableLightValidationIfAvailable)
			{
				if (!IsMarkingAsNeedingValidationSuspended && LightValidationIsValid)
				{
					MarkAsNeedingValidation();
				}

				if (LightValidationEnabled)
				{
					disableLightValidation = true;
				}
			}
		}

		bool disableLightValidation;

		protected override bool EnableLightValidationIfAvailable
		{
			get
			{
				return base.EnableLightValidationIfAvailable && !disableLightValidation;
			}
		}

		void InitialiseOpportunityFilter()
		{
			dateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.None;
		}

		protected void SetMainAddressState()
		{
			if (!Globals.IsWeb && UNLOCO != null && UNLOCO.CountryStates != null && UNLOCO.Country.RN_StateProvinceValidationRule == CountryAddressValidationRuleList.Codes.MustBeEntered && MainAddress.OA_State != UNLOCO.CountryStates.RW_Code)
			{
				MainAddress.OA_State = UNLOCO.CountryStates.RW_Code;
			}
		}

		protected void SetBranch()
		{
			if (!CompanyData.OB_GB_ControllingBranch.IsValid)
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort == OH_RL_NKClosestPort)
				{
					CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
				}
				else
				{
					var branchFilter = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, OH_RL_NKClosestPort);
					branchFilter.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
					branchFilter.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					branchFilter.OrderBy = GlbBranchSchema.GB_Code.Name;
					var firstBranch = Factory.LoadTop1<GlbBranch>(branchFilter);
					if (firstBranch != null)
					{
						CompanyData.OB_GB_ControllingBranch = firstBranch.PK;
					}
				}
			}
		}

		protected void SetMiscServDefaults(RefUNLOCO previousUNLOCO)
		{
			var uNLOCO = this.UNLOCO;

			if (uNLOCO != null)
			{
				var companyData = this.CompanyData;
				var country = uNLOCO.Country;

				SetDefaultCartageProviders();

				if (previousUNLOCO == null || previousUNLOCO.Country == null || country == null || country.Code != previousUNLOCO.Country.Code)
				{
					companyData.SetTaxApplicable();
				}

				if (country != null)
				{
					if (country.RN_IsActive)
					{
						MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = country.RN_Code;
					}

					if (country.LocalCurrency != null && country.LocalCurrency.RX_IsActive)
					{
						RefCurrency apDefaultCurrency = GetCurrency(companyData.APDefltCurrency, previousUNLOCO);
						RefCurrency fwDefaultCurrency = GetCurrency(MiscServ.FWDefCurrency, previousUNLOCO);

						if (apDefaultCurrency != null)
						{
							companyData.OB_RX_NKAPDefltCurrency = apDefaultCurrency.RX_Code;
						}

						if (fwDefaultCurrency != null)
						{
							MiscServ.OM_RX_NKFWDefCurrency = fwDefaultCurrency.RX_Code;
						}
					}
				}
			}
		}

		protected void SetDefaultCartageProviders()
		{
			if (UNLOCO != null && UNLOCO.Country != null)
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == UNLOCO.RL_RN_NKCountryCode)
				{
					ZGuid branchPK = ZGuid.Empty;
					if (CompanyData.OB_GB_ControllingBranch.IsValid)
					{
						branchPK = CompanyData.OB_GB_ControllingBranch;
					}
					else if (BranchForCurrentUNLOCO != null)
					{
						branchPK = BranchForCurrentUNLOCO.PK;
					}

					if (branchPK.IsValid)
					{
						ZGuid defaultAIRCartageCompany = FreightDataRegistry.Instance.AIRCartageCompany.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), branchPK.ToGuid(), Guid.Empty);
						ZGuid defaultLCLCartageCompany = FreightDataRegistry.Instance.LCLCartageCompany.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), branchPK.ToGuid(), Guid.Empty);
						ZGuid defaultFCLCartageCompany = FreightDataRegistry.Instance.FCLCartageCompany.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), branchPK.ToGuid(), Guid.Empty);

						SetRelatedParty(defaultAIRCartageCompany, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
						SetRelatedParty(defaultLCLCartageCompany, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
						SetRelatedParty(defaultFCLCartageCompany, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

						SetRelatedParty(defaultAIRCartageCompany, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
						SetRelatedParty(defaultLCLCartageCompany, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
						SetRelatedParty(defaultFCLCartageCompany, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

						SetRelatedParty(defaultAIRCartageCompany, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty);
						SetRelatedParty(defaultLCLCartageCompany, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
						SetRelatedParty(defaultFCLCartageCompany, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
					}
				}
			}
		}

		#endregion

		#region Add Or Update DDR Log Info

		readonly List<Dictionary<string, string>> referencesForDDR = new List<Dictionary<string, string>>();
		internal ZDateTimeOffset dateTimeForDDR;

		public void AddOrUpdateDDRLogInfo(IDuplicationEventArgs e)
		{
			if (e != null && e.Results != null && e.TargetObjects != null && e.TargetObjects is IEnumerable<CargoWise.Glow.Model.Interfaces.IOrgHeader> targetGlows)
			{
				referencesForDDR.Clear();
				var existingRecord = IsInDatabase ? "EDT" : "ADD";
				dateTimeForDDR = ZDateTimeOffset.UtcNow;
				IEnumerable<(string Name, string Score, ConfidenceRating ConfidenceRating)> scoringResults = e.Results.OrderByDescending(u => u.Score).Select(score => (targetGlows.FirstOrDefault(target => target.OH_PK == score.TargetPK)?.OH_Code, (score.Score * 100).ToString(), score.ConfidenceRating));

				foreach (var scoringResult in scoringResults)
				{
					string threshold;

					switch (scoringResult.ConfidenceRating)
					{
						case ConfidenceRating.Exact:
						case ConfidenceRating.High:
							threshold = "HIGH";
							break;
						case ConfidenceRating.Medium:
							threshold = "MED";
							break;
						case ConfidenceRating.Low:
						case ConfidenceRating.None:
							threshold = "LOW";
							break;
						default:
							threshold = string.Empty;
							break;
					}

					referencesForDDR.Add(new Dictionary<string, string>
					{
						[Params.Codes.Reason] = threshold,
						[Params.Codes.Type] = existingRecord,
						[Params.Codes.Name] = scoringResult.Name,
						[Params.Codes.Score] = scoringResult.Score,
					});
				}
			}
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			GenerateFinalCode();

			if (!Globals.IsUserInteractive && IsInDatabase && string.IsNullOrWhiteSpace(OH_Code))
			{
				OH_Code = (ZString)OH_CodeInfo.OriginalValue;
			}

			if (OH_Code.IsEmpty)
			{
				throw new ZCannotSaveException($"Organization code is empty, and could not be calculated using the code generation algorithm setup in the registry under {OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.GetLocation()}. The algorithm may be setup incorrectly, or insufficient information was entered for a code to be generated.", "Cannot save");
			}

			var closestPortHasChanges = (OH_IsGlobalAccount && !isResolvingClosestPort && MainAddress != null) ? MainAddress.OA_RL_NKRelatedPortCodeHasChanges : OH_RL_NKClosestPortInfo.HasChanges;
			if (OH_FullNameInfo.HasChanges || closestPortHasChanges || !OH_IsActive)
			{
				InvalidateScreeningStatuses();
			}

			if (OH_IsNationalAccountInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("D0D8C3E0-152B-4E11-BF79-F2A0E03EB741", "National Account"), OH_IsNationalAccount);
			}

			if (OH_IsGlobalAccountInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("124909DD-9231-4B2A-8AD3-F66EF9458C73", "Global Supplier"), OH_IsGlobalAccount);
			}

			if (OH_IsTempAccountInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("0319E496-48A6-4500-97E5-00CB7907947D", "Temporary Account"), OH_IsTempAccount);
			}

			if (OH_IsConsigneeInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("2582D6A9-94B0-423B-B66D-A08873829884", "Consignee"), OH_IsConsignee);
			}

			if (OH_IsConsignorInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("465B5FEB-2E58-492A-8245-ABBC8C2E68E1", "Consignor"), OH_IsConsignor);
			}

			if (OH_IsTransportClientInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("CE72C5F5-ABE4-465D-9F69-09A63D75EFB4", "Transport Client"), OH_IsTransportClient);
			}

			if (OH_IsWarehouseClientInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("9538C6C0-6DC8-4A27-8230-4D9B11C9F097", "Warehouse"), OH_IsWarehouseClient);
			}

			if (OH_IsShippingProviderInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("93D4BFC5-4036-4A99-A1D1-25ECF6803EFC", "Carrier"), OH_IsShippingProvider);
			}

			if (OH_IsForwarderInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("F1287B13-50E2-40B9-8965-6121FC683987", "Forwarder/Agent"), OH_IsForwarder);
			}

			if (OH_IsBrokerInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("4449C6D8-4768-468F-8188-4EDD0A501F63", "Broker"), OH_IsBroker);
			}

			if (OH_IsMiscFreightServicesInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("5A595254-B521-4F58-B2D9-C747AFAB62E0", "Services"), OH_IsMiscFreightServices);
			}

			if (OH_IsCompetitorInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("8A9426CD-3912-4639-AACE-C9DEDEC971E4", "Competitor"), OH_IsCompetitor);
			}

			if (OH_IsSalesLeadInfo.HasChanges)
			{
				LogOrgTypeSet(Res.GetString("385C338F-B3BD-4E44-A3C2-3C25A4558CEE", "Sales"), OH_IsSalesLead);
			}

			AddDDREvent();
		}

		[SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		void LogOrgTypeSet(string companyType, bool tickValue)
		{
			string tickedMessage = Res.GetString("6CDD09F3-584B-4278-AD54-58CE6C957E6B", "{0} Flag Ticked", companyType);
			string untickedMessage = Res.GetString("41E9235E-3713-410D-901A-3AA41D1424AE", "{0} Flag Un-Ticked", companyType);
			var eventString = tickValue ? tickedMessage : untickedMessage;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(Events.EditedARecord, eventString);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		void AddDDREvent()
		{
			foreach (var referenceForDDR in referencesForDDR)
			{
				this.GetLogs().AddNew(AutoEvents.DuplicateDetectedForReview, dateTimeForDDR, referenceForDDR.ToArray());
			}

			referencesForDDR.Clear();
		}

		public void InvalidateScreeningStatuses()
		{
			ScreeningStatusUpdater.ProcessInvalidateLocalDataChanges(this, () =>
			{
				shouldDeactivateEntity = !HasDeactivatedEntity && !OH_IsActive;
			});
		}

		bool shouldDeactivateEntity;
		public ZBool ShouldUpdateRelatedJobs { get; set; }

		public override bool IsSavedByFactory
		{
			get { return fIsSavedByFactory && base.IsSavedByFactory; }
		}

		public void OverrideIsSavedByFactory(bool isSavedByFactory)
		{
			fIsSavedByFactory = isSavedByFactory;
		}

		bool fIsSavedByFactory = true;

		public event EventHandler OrgSaved;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				OrgSaved?.Invoke(this, EventArgs.Empty);

				if (shouldDeactivateEntity)
				{
					shouldDeactivateEntity = false;
				}
			}

			isSavingFactory = false;
		}

		bool isSavingFactory;
		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		protected override void OnFactorySaving()
		{
			isSavingFactory = true;

			if (shouldSetDefaultRelatedParties)
			{
				SetDefaultRelatedParties();
			}
			if (shouldSetDefaultOrgSecurities)
			{
				SetDefaultOrgSecurities();
			}

			if (PatternMatchRequiresRegen)
			{
				// Use temporary collection to prevent loading all pattern matches into memory
				OrgPatternMatchCollection patternMatches = new OrgPatternMatchCollection(Factory, new ZQuery(OrgPatternMatchSchema.OS_OH, PK) { FetchOnlyFromLocalCache = !IsInDatabase });
				patternMatches.GeneratePatternMatchesFromOrg(this);
				PatternMatchRequiresRegen = false;
			}

			if (IsInDatabase && OH_CategoryInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.CurrentCulture, "Organization category was changed from: {0} to: {1}", OH_CategoryInfo.OriginalValue, OH_Category));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			if (IsInDatabase && startValueOH_IsActive != null && startValueOH_IsActive != OH_IsActive)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, "Organization was marked as " + (OH_IsActive ? (NoResString)"active" : (NoResString)"inactive"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			startValueOH_IsActive = OH_IsActive;

			if (!OH_IsActive && OH_IsActiveInfo.HasChanges)
			{
				DeleteUnusedStmData();
			}

			base.OnFactorySaving();
		}

		void DeleteUnusedStmData()
		{
			var datas = new StmDataCollection(Factory, new ZQuery(StmDataSchema.SD_DepartmentGuid, this.PK));
			datas.DeleteAll();
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			OrgHeader result = (OrgHeader)base.CloneInternal(args);
			result.OH_Code = OH_Code; // OH_Code gets regenrated when clone touches the RL_NKClosestPort or FullName 

			return result;
		}

		#endregion

		#region Delete

		protected override void OnElementChanged()
		{
			if (!IsBeingDeleted)
			{
				base.OnElementChanged();
			}
		}

		public string DeletedStack
		{
			get
			{
				return (NoResString)"Delete Stack: " + System.Environment.NewLine + (deletedStack ?? (NoResString)"Delete stack never collected");
			}
		}
		string deletedStack;

		public override void Delete()
		{
			IsBeingDeleted = true;

			try
			{
				deletedStack = new StackTrace().ToString();
				if (!IsDeleted)
				{
					ObjectFactory.Get<IChildrenDeletionHelper>().DeleteChildren(this);
				}
				DeleteDependantObjects();
				DeleteUnusedStmData();
				base.Delete();
				NullAllDependantObjectReferences();
			}
			finally
			{
				IsBeingDeleted = false;
			}
		}

		protected override StringBuilder BuildRowDeletedReport(string columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, string message)
		{
			return base.BuildRowDeletedReport(columnName, ex, versionToUse, version, message).AppendLine(DeletedStack);
		}

		internal bool IsBeingDeleted
		{
			get;
			set;
		}

		void DeleteRelatedOrgPattenMatchOverride()
		{
			if (!IsDeleted)
			{
				var query = new ZQuery();
				query.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, PK);
				query.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Organisation);
				OrgPatternMatchOverride[] orgMatches = Factory.Load<OrgPatternMatchOverride>(query);

				for (int i = 0; i < orgMatches.Length; i++)
				{
					orgMatches[i].Delete();
				}
			}
		}

		void DeletePatternMatching()
		{
			PatternMatchingRemover.DeleteAll(this);
		}

		void DeleteRelatedCompetitors()
		{
			var relatedCompetitorsFilter = new ZQuery();
			relatedCompetitorsFilter.AddToFilter(JoinCondition.Or, OrgCompetitorSchema.OCP_OH_Parent, SQLComparisonOperator.Equal, PK);
			relatedCompetitorsFilter.AddToFilter(JoinCondition.Or, OrgCompetitorSchema.OCP_OH_Competitor, SQLComparisonOperator.Equal, PK);
			var relatedCompetitors = Factory.Load<OrgCompetitor>(relatedCompetitorsFilter);
			foreach (var competitor in relatedCompetitors)
			{
				competitor.Delete();
			}
		}

		void DeleteDependantObjects()
		{
			DeleteRelatedOrgPattenMatchOverride();
			OrgRelatedPartyDependentCollection relatedParties = new OrgRelatedPartyDependentCollection(this, this.Factory);
			relatedParties.Load();
			relatedParties.RemoveAndDeleteAll();

			DeleteRelatedCompetitors();
			Subscriptions.DeleteAll();
			OrgServiceLevels.DeleteAll();
			LandedCostingPreferences.RemoveAndDeleteAll();

			StaffAssignments.AllowDeleteOfOverallRepRegardlessOfSecurity = true;
			StaffAssignments.CompanySpecific = false;
			StaffAssignments.RemoveAndDeleteAll();

			SalesOpportunities.RemoveAndDeleteAll();
			BrandsOrRelatedNames.RemoveAndDeleteAll();
			PatternMatchesForThisOrg.RemoveAndDeleteAll();
			DeletePatternMatching();

			var orgSalesFilter = new ZQuery();
			orgSalesFilter.AddToFilter(JoinCondition.Or, OrgSalesSchema.OW_OH_Primary, SQLComparisonOperator.Equal, PK);
			orgSalesFilter.AddToFilter(JoinCondition.Or, OrgSalesSchema.OW_OH_Buyer, SQLComparisonOperator.Equal, PK);
			orgSalesFilter.AddToFilter(JoinCondition.Or, OrgSalesSchema.OW_OH_Supplier, SQLComparisonOperator.Equal, PK);
			var orgSales = Factory.Load<OrgSales>(orgSalesFilter);
			foreach (var sales in orgSales)
			{
				sales.Delete();
			}

			var tradePeriods = Factory.Load<OrgTradePeriod>(new ZQuery(OrgTradePeriodSchema.PAS_OH_Client, PK));
			foreach (var tradePeriod in tradePeriods)
			{
				if (!tradePeriod.IsDeleted)
				{
					tradePeriod.Delete();
				}
			}

			SalesCalls.DeleteAll();
			SupplierLinks.RemoveAndDeleteAll();
			BuyerLinks.RemoveAndDeleteAll();
			PatternMatchOverrides_ForBinding.RemoveAndDeleteAll();

			CustomsCodes.AllowDeleteRegardlessOfSecurity = true;
			CustomsCodes.RemoveAndDeleteAll();

			if (IsInDatabase || clients != null)
			{
				Clients.RemoveAndDeleteAll();
			}

			customLabels = null; // need to remove extra objects that are reloaded
			CustomLabels.RemoveAndDeleteAll();

			if (miscServ != null)
			{
				miscServ.Delete();
			}

			DeleteCompanyDataForAllCompanies();
			DeleteCountryDataForAllCountries();
			if (fCountryData != null)
			{
				fCountryData.Delete();
				fCountryData = null;
			}

			ZQuery enquiryQuery = new ZQuery(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, PK);
			SalesEnquiry[] enquiries = Factory.Load<SalesEnquiry>(enquiryQuery);
			foreach (var enquiry in enquiries)
			{
				enquiry.Delete();
			}

			AddressesNoAutoCreate.RemoveAndDeleteAllIncludingMainAddress();
			AllocatedContacts.RemoveAndDeleteAll();
			Contacts.RemoveAndDeleteAll();
			SecurityRights.RemoveAndDeleteAll();
			AppointedAgentPorts.RemoveAndDeleteAll();
			AppointedGatewayAgentPorts.RemoveAndDeleteAll();
			CarrierAppointedAgentPorts_Stevedore.RemoveAndDeleteAll();
			CarrierAppointedAgentPorts_AirCTO.RemoveAndDeleteAll();
			CarrierAppointedAgentPorts_RailHeadDepot.RemoveAndDeleteAll();
			CarrierAppointedAgentPorts_RoadDepotShed.RemoveAndDeleteAll();
			CarrierAppointedAgentPorts_ContainerYardPark.RemoveAndDeleteAll();
			CarrierAppointedAgentPorts_Agency.RemoveAndDeleteAll();
			ConsigneeContainerPenalties.DeleteAll();
			ConsignorContainerPenalties.DeleteAll();
			CarrierContainerPenalties.DeleteAll();
			EDICommunicationsModes.RemoveAndDeleteAll();
			AgentRelationships.RemoveAndDeleteAll();
			OrgWebURLs.RemoveAndDeleteAll();
			OrgFountains.DeleteAll();
			NumberRangeMatchingDetails.DeleteAll();
			OrgAirlineBranchAccounts.DeleteAll();

			WorkflowItems.RemoveAndDeleteAll();
			SalesValueAssociatedEntity.DeleteAllSalesValuePivots(this);

			RequiredDocuments.RemoveAndDeleteAll();
			SuppressedDocuments.RemoveAndDeleteAll();
		}

		// This is a temporary fix until the Factory gets refreshed properly after a delete fails.
		// At present, when a delete fails from the GUI, the dependent collections do not get refreshed properly, so you cannot access any any values.
		void NullAllDependantObjectReferences()
		{
			contacts = null;
			addresses = null;
			salesCollection = null;
			salesCalls = null;
			supplierLinks = null;
			buyerLinks = null;
			patternMatchOverrides = null;
			customLabels = null;
			fCustomsCodesCollection = null;
			clients = null;
			landedCostingPreferences = null;
			staffAssignments = null;
			securityRights = null;
			salesOpportunities = null;
			brandsOrRelatedNames = null;
			patternMatchesForThisOrg = null;
			appointedAgentPorts = null;
			appointedGatewayAgentPorts = null;
			carrierAppointedAgentPorts_Stevedore = null;
			carrierAppointedAgentPorts_AirCTO = null;
			carrierAppointedAgentPorts_RailHeadDepot = null;
			carrierAppointedAgentPorts_RoadDepotShed = null;
			carrierAppointedAgentPorts_ContainerYardPark = null;
			containerYardRelatedCarrierAppointedAgentPorts = null;
			consigneeContainerPenalties = null;
			consignorContainerPenalties = null;
			carrierContainerPenalties = null;
			ediCommunicationsModes = null;
			subscriptions = null;
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (propertyPath.Length > 0)
			{
				if ((kind & TestBusinessObjectKind.MinimumRequiredToSave) != 0)
				{
					kind = TestBusinessObjectKind.MinimumRequiredToSave;
				}
				else
				{
					kind = TestBusinessObjectKind.NoData;
				}
			}

			base.FillWithValidTestDataCore(kind, propertyPath);

			if (MainAddress != null)
			{
				MainAddress.FillWithValidTestData(kind, propertyPath);
			}

			for (int i = 0; i < CustomsCodes.Count; i++)
			{
				if (CustomsCodes[i].OK_CustomsRegNo.IsEmpty)
				{
					CustomsCodes[i].OK_CustomsRegNo = "RN" + i;
				}
			}

			// make the full name based on the property that it is navigated from
			if (propertyPath.Length > 0)
			{
				ZString fullName = propertyPath[propertyPath.Length - 1].Name;
				if (fullName == "Header" && propertyPath.Length >= 2)
				{
					fullName = propertyPath[propertyPath.Length - 2].Name;
				}

				OH_FullName = fullName.SubstringSafe(0, OH_FullNameInfo.MaxLength);
			}
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				if (!((IList)CollectionPropertyNamesToExclude).Contains(collectionProperty.Name))
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}

			[ThreadStatic]
			static string[] fCollectionPropertyNamesToExclude;

			static string[] CollectionPropertyNamesToExclude
			{
				get
				{
					if (fCollectionPropertyNamesToExclude == null)
					{
						string[] value = new string[]
						{
							"SupplierLinks",
							"BuyerLinks",
							"SalesCollection",
							"SalesCallContacts",
							"CustomFormLabels",
							"CustomDocumentLabels",
							"PatternMatchOverrides",
							"CustomLabels",
							"Clients",
							"SalesCalls"
						};
						foreach (string collectionPropertyName in value)
						{
							if (TypeDescriptor.GetProperties(typeof(OrgHeader))[collectionPropertyName] == null)
							{
								throw new InvalidOperationException("Missing collection property " + collectionPropertyName + ", please remove from the above list");
							}
						}

						fCollectionPropertyNamesToExclude = value;
					}

					return fCollectionPropertyNamesToExclude;
				}
			}
		}

#endif
		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("1aef60c5-d34f-470b-b4f0-c6821d190fd4", "Organization");
				if (!IsDeleted && !OH_Code.IsEmpty)
				{
					result += " (" + OH_Code + ")";
				}

				return result;
			}
		}

		protected override ZString HumanReadableShortcutNameCore => GetHumanReadableShortcutName();

		public ZString GetHumanReadableShortcutName()
		{
			var result = string.Format("{0} - {1}", OH_Code, OH_FullName);

			if (!OH_RL_NKClosestPort.IsEmpty)
			{
				result += " (" + OH_RL_NKClosestPort + ")";
			}

			return result;
		}

		#endregion

		#region Read Only Factory

		public virtual BusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (fReadOnlyFactory == null)
				{
					fReadOnlyFactory = new BusinessObjectFactory();
				}

				return fReadOnlyFactory;
			}
		}

		BusinessObjectFactory fReadOnlyFactory;

		#endregion

		#region Logging

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				if (IsDeleted || IsBeingDeleted)
				{
					return base.BusinessObjectsWithRelatedEventsCore;
				}

				ArrayList objects = new ArrayList();

				objects.Add(MiscServ);
				objects.Add(CompanyData);
				objects.AddRange(Addresses);
				objects.AddRange(Contacts);
				objects.AddRange(SupplierLinks);
				objects.AddRange(BuyerLinks);
				objects.AddRange(SalesCollection);
				objects.AddRange(PatternMatchOverrides_ForBinding);
				objects.AddRange(CustomsCodes);

				objects.AddRange(Clients);
				objects.AddRange(StaffAssignments);
				objects.AddRange(SalesOpportunities);
				objects.AddRange(AgentRelationships);
				objects.AddRange(LandedCostingPreferences);
				objects.AddRange(BrandsOrRelatedNames);
				objects.AddRange(CustomLabels);
				objects.AddRange(CustomDocumentLabels);
				objects.AddRange(CustomFormLabels);
				objects.AddRange(AppointedAgentPorts);
				objects.AddRange(AppointedGatewayAgentPorts);
				objects.AddRange(CarrierAppointedAgentPorts_Stevedore);
				objects.AddRange(CarrierAppointedAgentPorts_AirCTO);
				objects.AddRange(CarrierAppointedAgentPorts_RailHeadDepot);
				objects.AddRange(CarrierAppointedAgentPorts_RoadDepotShed);
				objects.AddRange(CarrierAppointedAgentPorts_ContainerYardPark);
				objects.AddRange(CarrierAppointedAgentPorts_Agency);
				objects.AddRange(ContainerYardRelatedCarrierAppointedAgentPorts);
				objects.AddRange(CompanyData.AccCFXConfigurations.Where(x => x.Level == AccCFXConfigurationLevelEnum.Organisation).ToArray());
				objects.AddRange(CompanyData.AccARExchangeRateConfigurations.Where(x => x.Level == AccExRateConfigurationLevelEnum.Debtor).ToArray());
				objects.AddRange(CompanyData.AccAPExchangeRateConfigurations.Where(x => x.Level == AccExRateConfigurationLevelEnum.Creditor).ToArray());

				foreach (OrgContact contact in Contacts)
				{
					contact.AddFetchHintsForBusinessObjectsWithRelatedEvents();
				}

				foreach (OrgContact contact in Contacts)
				{
					objects.AddRange(contact.BusinessObjectsWithRelatedEvents);
				}

				foreach (OrgOpportunity opportunity in SalesOpportunities)
				{
					objects.AddRange(opportunity.WorkflowItems);
				}

				foreach (OrgLandedCostingPrefs pref in LandedCostingPreferences)
				{
					objects.AddRange(pref.BusinessObjectsWithRelatedEvents);
				}

				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Related Business Objects

		#region Branch

		public GlbBranch Branch
		{
			get { return CompanyData.ControllingBranch; }
		}

		#endregion

		#region CountryData

		public RefCountry Country
		{
			get
			{
				RefUNLOCO port = ClosestPort;
				return (port != null) ? port.Country : null;
			}
		}

		public ZString CountryCode
		{
			get { return OH_RL_NKClosestPort.Left(2); }
		}

		public ZString MainAddressCountryCodes
		{
			get
			{
				var mainAddresses = Addresses.AddressesOfType(OrgAddressType.Office.ToString(), currentCompanyOnly: false).OfType<OrgAddress>().Where(x => x.AddressCapability.GetIsMainAddress(OrgAddressType.Office));
				return string.Join(", ", mainAddresses.Select(x => x.OA_RN_NKCountryCode).OrderBy(x => x));
			}
		}

		public MultilingualString CountryName
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;
				if (Addresses.MainAddressDontCreate != null)
				{
					result = Addresses.MainAddressDontCreate.CountryName;
				}
				else
				{
					RefCountry country = Country;
					if (country != null)
					{
						result = country.RN_DescMultilingual;
					}
				}
				return result;
			}
		}

		public ZString PortName
		{
			get
			{
				ZString result = ZString.Empty;
				if (Addresses.MainAddressDontCreate != null)
				{
					result = Addresses.MainAddressDontCreate.PortName;
				}
				else
				{
					RefUNLOCO port = ClosestPort;
					if (port != null)
					{
						result = port.RL_PortName;
					}
				}
				return result;
			}
		}

		public ZString CityName
		{
			get
			{
				if (Addresses.MainAddressDontCreate != null)
				{
					return Addresses.MainAddressDontCreate.City;
				}
				return ZString.Empty;
			}
		}

		void DeleteCountryDataForAllCompanies()
		{
			OrgCountryDataDependentCollection countryDatas = new OrgCountryDataDependentCollection(this);
			countryDatas.Load();
			countryDatas.RemoveAndDeleteAll();
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCountryDataDependentCollection CountryDataCollectionForThisCompany
		{
			get
			{
				if (fCountryDataCollectionForThisCompany == null)
				{
					fCountryDataCollectionForThisCompany = new OrgCountryDataDependentCollection(this);
					fCountryDataCollectionForThisCompany.Load();
					RegisterEditableChildObject(fCountryDataCollectionForThisCompany);
					fCountryDataCollectionForThisCompany.SetReadOnlyIncludingChildren(!Env.Security.OrgConsignorModifyExporterScheme.IsAllowed);
				}

				return fCountryDataCollectionForThisCompany;
			}
		}

		OrgCountryDataDependentCollection fCountryDataCollectionForThisCompany;

		public OrgCountryData GetCountryData(ZString countryCode)
		{
			OrgCountryData result;
			ZQuery query = new ZQuery(OrgCountryDataSchema.OV_OH_OrgHeader, PK);
			query.AddToFilter(OrgCountryDataSchema.OV_OA_ApprovedLocation, DBNull.Value);
			query.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, countryCode);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			result = Factory.LoadTop1<OrgCountryData>(query);

			if (result == null)
			{
				result = Factory.New<OrgCountryData>();
				using (result.SuspendSettingHasChanges())
				{
					result.OV_OH_OrgHeader = PK;
					result.OV_RN_NKClientCountryRelation = countryCode;
				}
			}

			return result;
		}

		public OrgCountryData GetCountryData(RefCountry country)
		{
			return GetCountryData(country.RN_Code);
		}

		/// <summary>
		/// Currently logged in country Country specific data
		/// </summary>
		[ActionFieldFollow(true)]
		public OrgCountryData CountryData
		{
			get
			{
				if (IsDeleted)
				{
					return Factory.GetNull<OrgCountryData>();
				}

				if (fCountryData == null || (fCountryData.IsDeleted))
				{
					fCountryData = GetCountryData(GlbCompany.CurrentCompany.Country);
					RegisterEditableChildObject(fCountryData);
				}

				return fCountryData;
			}
		}

		OrgCountryData fCountryData;

		void DeleteCountryDataForAllCountries()
		{
			OrgCountryDataDependentCollection countryDatas = new OrgCountryDataDependentCollection(this);
			countryDatas.Load();
			countryDatas.RemoveAndDeleteAll();
		}

		public bool RequiresSecurityOverrideToUpdateCountry
		{
			get
			{
				return !SecurityProvider.HasNewDetailsAllowCreationOutsideLoginCountry && Env.CurrentCompany.Country.Code != CountryCode
					&& (!IsInDatabase || (IsInDatabase && ((ZString)OH_RL_NKClosestPortInfo.OriginalValue != base.OH_RL_NKClosestPort)));
			}
		}

		#endregion

		#region CompanyData

		public OrgCompanyData GetCompanyDataForGlbCompany(GlbCompany company)
		{
			OrgCompanyData[] allCompaniesData = (OrgCompanyData[])CompanyDataCollection.Find(new ZQuery(OrgCompanyDataSchema.OB_GC, company.PK));

			OrgCompanyData result = null;
			if (allCompaniesData.Length == 0)
			{
				result = CompanyDataCollection.AddNew();
				result.OB_GC = company.PK;
				result.OB_OH = PK;
			}
			else
			{
				result = allCompaniesData[0];
			}

			return result;
		}

		public OrgCompanyDataDependentCollection CompanyDataCollection
		{
			get
			{
				if (fCompanyDataCollection == null)
				{
					fCompanyDataCollection = new OrgCompanyDataDependentCollection(this);
					fCompanyDataCollection.Load();
					fCompanyDataCollection.IsManagedForDataRefresh = true;
				}

				return fCompanyDataCollection;
			}
		}
		OrgCompanyDataDependentCollection fCompanyDataCollection;

		public OrgCompanyDataCollection CurrentCompanyDataAsCollection
		{
			get { return currentCompanyDataAsCollection ?? (currentCompanyDataAsCollection = new OrgCompanyDataCollection(Factory)); }
		}
		OrgCompanyDataCollection currentCompanyDataAsCollection;

		[ActionFieldFollow(true)]
		public OrgCompanyData CompanyData => CompanyDataCore(createIfNotExists: true);

		OrgCompanyData fCompanyData;

		[ActionFieldFollow(false)]
		public OrgCompanyData CompanyDataLoadOnly => CompanyDataCore(createIfNotExists: false);

		OrgCompanyData CompanyDataCore(bool createIfNotExists)
		{
			if (IsDeleted)
			{
				return Factory.GetNull<OrgCompanyData>();
			}

			if (fCompanyData == null || fCompanyData.IsDeleted ||
				(!fCompanyData.IsDeleted && fCompanyData.OB_GC != Env.CurrentCompany.PK))
			{
				ReloadCompanyData(createIfNotExists: createIfNotExists);
			}

			return fCompanyData;
		}

		void ReloadCompanyData(bool forceReloadFromDatabase = false, bool createIfNotExists = true)
		{
			var query = new ZQuery(OrgCompanyDataSchema.OB_OH, PK);
			query.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.ReLoadExistingRows = forceReloadFromDatabase;
			fCompanyData = Factory.LoadTop1<OrgCompanyData>(query);
			if (fCompanyData != null)
			{
				fCompanyData.Validation.ValidateOB_OH();
			}
			else if (createIfNotExists)
			{
				fCompanyData = Factory.New<OrgCompanyData>();
				using (fCompanyData.SuspendSettingHasChanges())
				{
					fCompanyData.OB_OH = PK;
					fCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
				}
				CompanyDataCollection.Add(fCompanyData);
			}

			if (fCompanyData != null)
			{
				RegisterEditableChildObject(fCompanyData);
				RegisterListChangedCalledRefreshBinding(fCompanyData);

				CurrentCompanyDataAsCollection.RemoveAll();
				CurrentCompanyDataAsCollection.Add(fCompanyData);
			}
		}

		public void DestroyAndReloadCompanyData(bool reloadFromDatabase = false)
		{
			if (fCompanyData != null)
			{
				fCompanyData.Delete();
				ReloadCompanyData(reloadFromDatabase);
				if (reloadFromDatabase)
				{
					RefreshBindingIncludingChildren();
				}
			}
		}

		void DeleteCompanyDataForAllCompanies()
		{
			var exRateConfigQuery = AccExchangeRateConfigurationsQueryProviderFactory.CreateOrganizationLevelProvider(PK, new ZQuery()).GetQuery();
			AccExchangeRateConfigurationsHelper.LoadAndDelete(Factory, exRateConfigQuery);
			OrgCompanyDataDependentCollection companyDatas = new OrgCompanyDataDependentCollection(this);
			companyDatas.Load();
			companyDatas.RemoveAndDeleteAll();
		}

		#endregion

		#region ServiceLevels

		public ZBool IsServiceLevelOverridden
		{
			get
			{
				return isServiceLevelOverridden;
			}
			set
			{
				if (orgServiceLevels != null)
				{
					foreach (OrgServiceLevel level in orgServiceLevels)
					{
						level.HasChanges = true;
					}
				}

				isServiceLevelOverridden = value;
				IsServiceLevelOverriddenInfo.RefreshBinding();
			}
		}

		ZBool isServiceLevelOverridden;

		public ZPropertyInfo IsServiceLevelOverriddenInfo
		{
			get { return GetZPropertyInfo(nameof(IsServiceLevelOverridden)); }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgServiceLevelCollection OrgServiceLevels
		{
			get
			{
				if (orgServiceLevels == null || orgServiceLevels.Count == 0)
				{
					orgServiceLevels = new OrgServiceLevelCollection(Factory, PK);
					isServiceLevelOverridden = orgServiceLevels.Count > 0;

					FillCollectionWithDefaultLevels();
					RegisterEditableChildObject(orgServiceLevels);

					Factory.Saving += Factory_Saving;
					Factory.Saved += Factory_Saved;
				}

				return orgServiceLevels;
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			FillCollectionWithDefaultLevels();
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			foreach (OrgServiceLevel level in orgServiceLevels.ToArray())
			{
				if (isServiceLevelOverridden)
				{
					level.HasChanges = true;
				}
				else
				{
					level.Delete();
				}
			}
		}

		OrgServiceLevelCollection orgServiceLevels;

		void FillCollectionWithDefaultLevels()
		{
			RegistryServiceLevelCollection registryServiceLevels = WebDataRegistry.Instance.ServiceLevelVisibility.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			foreach (RegistryServiceLevel level in registryServiceLevels)
			{
				if (orgServiceLevels.FindByRefServiceLevelCode(level.Code) == null)
				{
					OrgServiceLevel newLevel = orgServiceLevels.AddNew();
					using (newLevel.SuspendSettingHasChanges())
					{
						newLevel.PM_OH = PK;
						newLevel.PM_RS_NKSrvLvl = level.Code;
						newLevel.PM_IsPublished = level.Bool;
					}
				}
			}
		}

		#endregion

		#region MiscServ

		[ActionFieldFollow(true)]
		public virtual OrgMiscServ MiscServ
		{
			get
			{
				if (IsDeleted)
				{
					return Factory.GetNull<OrgMiscServ>();
				}

				if (miscServ == null)
				{
					ReloadOrgMiscServ();
				}

				if (miscServ.IsDeleted)
				{
					return null;
				}

				return miscServ;
			}
#if DEBUG
			set { miscServ = value; }
#endif
		}

		OrgMiscServ miscServ;

		protected virtual Type MiscServType
		{
			get { return typeof(OrgMiscServ); }
		}

		public void DestroyAndReloadOrgMiscServ(bool reloadFromDatabase = false)
		{
			if (miscServ != null)
			{
				miscServ.Delete();
				ReloadOrgMiscServ(reloadFromDatabase);
				if (reloadFromDatabase)
				{
					RefreshBindingIncludingChildren();
				}
			}
		}

		void ReloadOrgMiscServ(bool forceReloadFromDatabase = false)
		{
			var query = new ZQuery(OrgMiscServSchema.OM_OH, PK);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.ReLoadExistingRows = forceReloadFromDatabase;

			miscServ = (OrgMiscServ)Factory.LoadTop1(MiscServType, query);
			if (miscServ == null)
			{
				miscServ = (OrgMiscServ)Factory.New(MiscServType);
				using (miscServ.SuspendSettingHasChanges())
				{
					miscServ.OM_OH = PK;

					var unloco = UNLOCO;
					if (unloco?.Country?.RN_IsActive ?? false)
					{
						miscServ.OM_RN_NKEXDefaultCntryOfOrigin = unloco.RL_RN_NKCountryCode;
					}
				}
			}

			RegisterEditableChildObject(miscServ);
			RegisterListChangedCalledRefreshBinding(miscServ);
		}

		public OrgCompanyDataCollection GlobalCreditGroupChilds
		{
			get
			{
				if (globalCreditGroupChilds == null)
				{
					FillGlobalCreditGroupChilds();
				}
				return globalCreditGroupChilds;
			}
		}
		OrgCompanyDataCollection globalCreditGroupChilds;

		public void FillGlobalCreditGroupChilds()
		{
			if (globalCreditGroupChilds == null)
			{
				globalCreditGroupChilds = new OrgCompanyDataCollection(Factory);

				var sql = @"
SELECT 
	Org.OB_PK as OrgPK, 
	vw_AccOrgBalance.BalanceTotal as BalanceTotalPerOrg,
	vw_AccOrgBalance.ClaimTotal as ClaimTotalPerOrg,
	vw_AccOrgBalance.CompanyPK as CompanyPK,
	OrgRelatedParty.PR_OH_RelatedParty as ARSettlementGroup
FROM dbo.GetGlobalCreditLimitOrganizationPerCompany(@OrgPK) as OrgCompanyPairs
INNER JOIN dbo.OrgCompanyData as Org ON OrgCompanyPairs.OrganisationPK = Org.OB_OH 
	AND OrgCompanyPairs.CompanyPK = Org.OB_GC
LEFT JOIN dbo.vw_AccOrgBalance ON vw_AccOrgBalance.OrganizationPK = OrgCompanyPairs.OrganisationPK
	AND vw_AccOrgBalance.CompanyPK = OrgCompanyPairs.CompanyPK
	AND vw_AccOrgBalance.Ledger = 'AR'
LEFT JOIN dbo.OrgRelatedParty on OrgRelatedParty.PR_GC = Org.OB_GC
	AND OrgRelatedParty.PR_OH_Parent = Org.OB_OH
	AND OrgRelatedParty.PR_PartyType = 'ARS'";

				var orgPKAndBalanceTotal = new DynamicBusinessObjectCollection(Factory);
				orgPKAndBalanceTotal.Load(sql, new[]
				{
					ZSqlParameter.New("@OrgPK", PK, OrgHeaderSchema.PK)
				});

				if (orgPKAndBalanceTotal.Count > 0)
				{
					IsUsingGlobalCreditGroupChildren = true;

					var dict = new Dictionary<ZGuid, Tuple<ZDecimal, ZGuid>>();
					foreach (DynamicBusinessObject organizationPerCompany in orgPKAndBalanceTotal)
					{
						var claimAmount = 0m;
						if (Guid.TryParse(organizationPerCompany["CompanyPK"].ToString(), out Guid companyPK) && companyPK != Guid.Empty)
						{
							var excludeOpenClaimsAmounts = (ObjectFactory.Get<IAccounting>().Registry.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation as BooleanRegistryItem).GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
							claimAmount = excludeOpenClaimsAmounts ? (ZDecimal)organizationPerCompany["ClaimTotalPerOrg"] : ZDecimal.Zero;
						}
						ZDecimal balanceTotalPerOrg = (ZDecimal)organizationPerCompany["BalanceTotalPerOrg"] - claimAmount;
						dict[(ZGuid)organizationPerCompany["OrgPK"]] = Tuple.Create(balanceTotalPerOrg, (ZGuid)organizationPerCompany[nameof(ARSettlementGroup)]);
					}

					globalCreditGroupChilds.AddRange(Factory.Load<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.PK, dict.Keys)));
					foreach (OrgCompanyData companyData in globalCreditGroupChilds)
					{
						companyData.CreditOutStandingBalance = dict[companyData.PK].Item1;
						companyData.ARSettlementGroupPK = dict[companyData.PK].Item2;
					}
				}
			}
		}

		public ZBool IsUsingGlobalCreditGroupChildren;

		public string InvalidGlobalCreditCurrencyOrMissingExRateMessage => Res.GetString("36a237e9-ed31-4295-ac2e-47f31a2bb9f0", @"Global outstanding transactions balance cannot be calculated for the global credit group.
It requires valid exchange rates for today to be entered (using ‘GCB’ or ‘PER’ exchange rate type) in all system companies where the Global Credit Group has transactions and/or local credit limits.
For a list of system companies and currency codes, please refer to Organization ({0}) > A/R > Credit Control and Settlement > Global > Companies Local Credit Control and Settlement Details.", OH_Code.Trim());

		#endregion

		#region ExchangeRateCollection

		[ChildEditable(true)]
		public RefExchangeRateInOrgHeaderDependentCollection ExchangeRateCollection
		{
			get
			{
				if (fExchangeRateCollection == null)
				{
					fExchangeRateCollection = new RefExchangeRateInOrgHeaderDependentCollection(this);
					RegisterEditableChildObject(fExchangeRateCollection);
					fExchangeRateCollection.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsSecurity);
				}

				return fExchangeRateCollection;
			}
		}
		RefExchangeRateInOrgHeaderDependentCollection fExchangeRateCollection;

		#endregion

		#region OrgRelatedParties

		public OrgHeader DeliveryAirCustomsBroker
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty); }
		}

		public OrgHeader DeliverySeaCustomsBroker
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty); }
		}

		public OrgHeader DeliveryCustomsBillTo
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty) ?? this; }
		}

		public OrgHeader DeliveryFreightBillTo
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty) ?? this; }
		}

		public OrgHeader DeliveryAttributeRevenueTo
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyDirectionList.Codes.Delivery) ?? this; }
		}

		public OrgHeader PickupAirCustomsBroker
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty); }
		}

		public OrgHeader PickupSeaCustomsBroker
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty); }
		}

		public OrgHeader PickupCustomsBillTo
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty) ?? this; }
		}

		public OrgHeader PickupFreightBillTo
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty) ?? this; }
		}

		public OrgHeader PickupAttributeRevenueTo
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyDirectionList.Codes.Pickup) ?? this; }
		}

		public OrgHeader ManagementGrouping
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder) ?? this; }
		}

		public OrgHeader ARGrouping
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyDirectionList.Codes.Forwarder) ?? this; }
		}

		public OrgHeader APGrouping
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder) ?? this; }
		}

		public OrgHeader ForwarderGrouping
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderGroup, RelatedPartyDirectionList.Codes.Forwarder) ?? this; }
		}

		public OrgHeader WarehouseBillTo
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, ZString.Empty, Constants.TransportModes.All, ZString.Empty) ?? this; }
		}

		public OrgHeader GetCustomsBillTo(bool isDelivery, ZString transportMode, ZString containerMode)
		{
			return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, isDelivery ? RelatedPartyDirectionList.Codes.Delivery : RelatedPartyDirectionList.Codes.Pickup, GetBillToTransportMode(transportMode), containerMode) ?? this;
		}

		public OrgHeader GetFreightBillTo(bool isDelivery, ZString transportMode, ZString containerMode)
		{
			return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, isDelivery ? RelatedPartyDirectionList.Codes.Delivery : RelatedPartyDirectionList.Codes.Pickup, GetBillToTransportMode(transportMode), containerMode) ?? this;
		}

		public OrgHeader GetWarehouseBillTo(ZString transportMode, ZString containerMode)
		{
			return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, null, GetBillToTransportMode(transportMode), containerMode) ?? this;
		}

		public OrgHeader GetRelatedBillToParty(JobInvoicingConsumerType consumerType, bool? isDelivery, ZString transportMode = default, ZString containerMode = default)
		{
			if (!isDelivery.HasValue)
			{
				if (consumerType == JobInvoicingConsumerTypes.WarehouseAdHocServiceJob
					|| consumerType == JobInvoicingConsumerTypes.WarehouseInwards
					|| consumerType == JobInvoicingConsumerTypes.WarehouseOutwards
					|| consumerType == JobInvoicingConsumerTypes.WarehouseStocktake
					|| consumerType == JobInvoicingConsumerTypes.WarehouseStorage)
				{
					return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, ZString.Empty, GetBillToTransportMode(transportMode), containerMode) ?? this;
				}
			}
			else
			{
				var direction = isDelivery.Value ? RelatedPartyDirectionList.Codes.Delivery : RelatedPartyDirectionList.Codes.Pickup;

				if (consumerType == null || consumerType == JobInvoicingConsumerTypes.Shipment || consumerType == JobInvoicingConsumerTypes.QuotedBooking)
				{
					return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, direction, GetBillToTransportMode(transportMode), containerMode) ?? this;
				}
				else if (consumerType == JobInvoicingConsumerTypes.Brokerage)
				{
					return GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, direction, GetBillToTransportMode(transportMode), containerMode) ?? this;
				}
				else if (consumerType == JobInvoicingConsumerTypes.CFSShipment
						&& isDelivery.Value
						&& !CompanyData.OB_IsDebtor
						|| OH_IsTempAccount)
				{
					return Factory.Load<OrgHeader>(CFSDataRegistry.Instance.DepotCashSalesOrg.Value) ?? this;
				}
			}
			return this;
		}

		ZString GetBillToTransportMode(ZString transportMode)
		{
			if (transportMode.IsEmpty)
			{
				return transportMode;
			}

			var validTransportModesForBilling = new CodeDescriptionPairList(OLookUpEditType.DocumentTransportMode);

			return validTransportModesForBilling.ContainsCode(transportMode) ? transportMode : (ZString)Constants.TransportModes.Other;
		}

		#region SourceOfLead
		[List("MiscServ.Organisations")]
		public ZGuid SourceOfLeadPK
		{
			get
			{
				return SourceOfLead != null ? SourceOfLead.PK : ZGuid.Empty;
			}
			set
			{
				if (value.IsEmpty && SourceOfLead != null)
				{
					DeleteRelatedParty(RelatedPartyTypeList.Codes.SourceOfSalesLead, RelatedPartyDirectionList.Codes.Sales);
				}
				else
				{
					SetRelatedParty(value, RelatedPartyTypeList.Codes.SourceOfSalesLead, RelatedPartyDirectionList.Codes.Sales);
				}

				SourceOfLeadPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SourceOfLeadPKInfo
		{
			get { return GetZPropertyInfo(Schema.SourceOfLeadPK); }
		}

		public OrgHeader SourceOfLead
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.SourceOfSalesLead, RelatedPartyDirectionList.Codes.Sales); }
		}

		#endregion

		#region ControllingAgent

		[List("MiscServ.CMControllingAgents")]
		public ZGuid ControllingAgentPK
		{
			get
			{
				return ControllingAgent != null ? ControllingAgent.PK : ZGuid.Empty;
			}
			set
			{
				if (value.IsEmpty && ControllingAgent != null)
				{
					DeleteRelatedParty(RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales);
				}
				else
				{
					SetRelatedParty(value, RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales);
				}

				ControllingAgentPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ControllingAgentPKInfo
		{
			get { return GetZPropertyInfo(Schema.ControllingAgentPK); }
		}

		public OrgHeader ControllingAgent
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales); }
		}

		#endregion

		#region Controlling Customers list

		public ZString ControllingCustomersAsString
		{
			get
			{
				var controllingCustomerPartiesView = new OrgRelatedPartyCollectionView(AllRelatedParties);
				controllingCustomerPartiesView.FilterByPartyType(RelatedPartyTypeList.Codes.ControllingCustomer, string.Empty);
				return string.Join(", ", controllingCustomerPartiesView.Cast<OrgRelatedParty>().Where(party => party.RelatedParty != null).Select(party => party.PR_FreightDirection + " - " + party.RelatedParty.OH_Code).OrderBy(s => s));
			}
		}

		#endregion

		#region APSettlementGroup
		[List("MiscServ.APSettlementGroups")]
		public ZGuid APSettlementGroupPK
		{
			get
			{
				return APSettlementGroup != null ? APSettlementGroup.PK : ZGuid.Empty;
			}
			set
			{
				if (value.IsEmpty && APSettlementGroup != null)
				{
					DeleteRelatedParty(RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);
				}
				else
				{
					SetRelatedParty(value, RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAPSettlementGroupPK();
				}

				APSettlementGroupPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo APSettlementGroupPKInfo
		{
			get { return GetZPropertyInfo(Schema.APSettlementGroupPK); }
		}

		public OrgHeader APSettlementGroup
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP); }
		}

		#endregion

		#region ARSettlementGroup

		[List("MiscServ.ARSettlementGroups")]
		public ZGuid ARSettlementGroupPK
		{
			get
			{
				return ARSettlementGroup != null ? ARSettlementGroup.PK : ZGuid.Empty;
			}
			set
			{
				if (value.IsEmpty)
				{
					if (ARSettlementGroup != null)
					{
						DeleteRelatedParty(RelatedPartyTypeList.Codes.ARSettlementGroup, RelatedPartyDirectionList.Codes.AR);
					}
					CompanyData.OB_ARUseSettlementGroupCreditLimit = false;
				}
				else
				{
					SetRelatedParty(value, RelatedPartyTypeList.Codes.ARSettlementGroup, RelatedPartyDirectionList.Codes.AR);
					if (PK == value)
					{
						CompanyData.OB_ARUseSettlementGroupCreditLimit = false;
					}
					CompanyData.Validation.ValidateOB_ARUseSettlementGroupCreditLimit();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateARSettlementGroupPK();
				}

				ARSettlementGroupPKInfo.RefreshBinding();
				CompanyData.OB_ARUseSettlementGroupCreditLimitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ARSettlementGroupPKInfo
		{
			get { return GetZPropertyInfo(Schema.ARSettlementGroupPK); }
		}

		public OrgHeader ARSettlementGroup
		{
			get { return GetRelatedParty(RelatedPartyTypeList.Codes.ARSettlementGroup, RelatedPartyDirectionList.Codes.AR); }
		}

		#endregion

		internal void RefreshRelatedPartyProxyFields()
		{
			APSettlementGroupPKInfo.RefreshBinding();
			ARSettlementGroupPKInfo.RefreshBinding();
			SourceOfLeadPKInfo.RefreshBinding();
			ControllingAgentPKInfo.RefreshBinding();
		}

		public OrgHeader GetRelatedParty(ZString partyType, ZString direction)
		{
			return GetRelatedParty(partyType, direction, ZString.Empty, ZString.Empty);
		}

		public OrgHeader GetRelatedParty(ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			return GetRelatedParty(ZGuid.Empty, partyType, direction, transportMode, containerMode);
		}

		public OrgHeader GetRelatedParty(ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			return GetRelatedParty(ZGuid.Empty, partyType, direction, transportMode, containerMode, location);
		}

		public OrgHeader GetRelatedParty(ZGuid addressPK, ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			return GetRelatedParty(addressPK, partyType, direction, transportMode, containerMode, ZString.Empty);
		}

		public OrgHeader GetRelatedPartyWithAddressFallback(ZGuid addressPK, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			var result = GetRelatedParty(addressPK, partyType, direction, transportMode, containerMode, location);
			return result ?? GetRelatedParty(null, partyType, direction, transportMode, containerMode, location);
		}

		public OrgHeader GetRelatedParty(ZGuid? addressPK, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			OrgHeader result = null;
			var relatedPartyRecord = AllRelatedParties.GetRelatedParty(addressPK, partyType, direction, transportMode, containerMode, location);
			if (relatedPartyRecord != null)
			{
				result = relatedPartyRecord.RelatedParty;
			}

			return result;
		}

		public void AddRelatedParty(ZGuid relatedOrgPK, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, GlbCompany company)
		{
			if (Factory.Load<OrgHeader>(relatedOrgPK) != null)
			{
				var orgRelatedParty = AllRelatedParties.AddNew();
				using (orgRelatedParty.GetValidationSuspender())
				{
					orgRelatedParty.PR_PartyType = partyType;
					orgRelatedParty.PR_FreightDirection = direction;
					orgRelatedParty.PR_FreightTransportMode = transportMode;
					orgRelatedParty.PR_FreightContainerMode = containerMode;
					orgRelatedParty.PR_OA = ZGuid.Empty;
					orgRelatedParty.PR_GC = company != null ? company.PK : ZGuid.Empty;
					orgRelatedParty.PR_OH_RelatedParty = relatedOrgPK;
				}
				orgRelatedParty.Validation.ValidateAll();
			}
		}

		public void SetRelatedParty(ZGuid relatedPartyPK, ZString partyType, ZString direction)
		{
			SetRelatedParty(Factory.Load<OrgHeader>(relatedPartyPK), partyType, direction, ZString.Empty, ZString.Empty);
		}

		public void SetRelatedParty(ZGuid relatedPartyPK, ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			SetRelatedParty(Factory.Load<OrgHeader>(relatedPartyPK), partyType, direction, transportMode, containerMode);
		}

		public void SetRelatedParty(ZGuid relatedPartyPK, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			SetRelatedParty(Factory.Load<OrgHeader>(relatedPartyPK), partyType, direction, transportMode, containerMode, location);
		}

		public void SetRelatedParty(OrgHeader relatedParty, ZString partyType, ZString direction)
		{
			SetRelatedParty(relatedParty, partyType, direction, ZString.Empty, ZString.Empty);
		}

		public void SetRelatedParty(OrgHeader relatedParty, ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			SetRelatedParty(ZGuid.Empty, relatedParty, partyType, direction, transportMode, containerMode);
		}

		public void SetRelatedParty(OrgHeader relatedParty, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			SetRelatedParty(ZGuid.Empty, relatedParty, partyType, direction, transportMode, containerMode, location);
		}

		public void SetRelatedParty(ZGuid addressPK, OrgHeader relatedParty, ZString partyType, ZString direction, ZString transportMode, ZString containerMode)
		{
			SetRelatedParty(addressPK, relatedParty, partyType, direction, transportMode, containerMode, ZString.Empty);
		}

		public void SetRelatedParty(ZGuid addressPK, OrgHeader relatedParty, ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZString location)
		{
			if (relatedParty != null)
			{
				AllRelatedParties.SetRelatedParty(addressPK, relatedParty, partyType, direction, transportMode, containerMode, location);
			}
		}

		public void DeleteRelatedParty(ZString partyType, ZString direction)
		{
			AllRelatedParties.RemoveRelatedParty(partyType, direction);
		}

		#region Related Parties

		[List("Lookups.FilterPartyTypeList")]
		public virtual ZString FilterPartyType
		{
			get { return filterPartyType; }
			set { filterPartyType = value; }
		}
		ZString filterPartyType;

		[List("Lookups.FilterFreightDirectionList")]
		public ZString FilterFreightDirection
		{
			get { return filterFreightDirection; }
			set { filterFreightDirection = value; }
		}
		ZString filterFreightDirection;

		#region AllRelatedParties

		[ChildEditable]
		[ActionFieldFollow(true)]
		public OrgRelatedPartyCompanySpecificCollection AllRelatedParties
		{
			get
			{
				if (allRelatedParties == null)
				{
					allRelatedParties = new OrgRelatedPartyCompanySpecificCollection(this, Factory);
					allRelatedParties.Load();
					RegisterEditableChildObject(allRelatedParties);
					allRelatedParties.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsRelatedPartiesSecurity);
					allRelatedParties.CountChanged += AllRelatedParties_CountChanged;
					if (shouldSetDefaultRelatedParties)
					{
						SetDefaultRelatedParties();
					}
				}

				return allRelatedParties;
			}
		}

		void AllRelatedParties_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved)
			{
				Validation.ValidateOH_IsControllingCustomer();
			}
		}

		OrgRelatedPartyCompanySpecificCollection allRelatedParties;

		public OrgRelatedPartyCollectionView AllRelatedPartiesView
		{
			get
			{
				if (allRelatedPartiesView == null)
				{
					allRelatedPartiesView = new OrgRelatedPartyCollectionView(AllRelatedParties);
					allRelatedPartiesView.FilterByPartyType(filterPartyType, filterFreightDirection);
				}

				return allRelatedPartiesView;
			}
		}

		OrgRelatedPartyCollectionView allRelatedPartiesView;

		#endregion

		#region Org Management Grouping

		public OrgManagementRelatedParentCollection RelatedManagementParentRelations
		{
			get
			{
				if (relatedManagementParentRelations == null)
				{
					relatedManagementParentRelations = new OrgManagementRelatedParentCollection(this);
				}
				return relatedManagementParentRelations;
			}
		}
		OrgManagementRelatedParentCollection relatedManagementParentRelations;

		public OrgManagementRelatedSubsidiaryCollection RelatedManagementSubsidiaryRelations
		{
			get
			{
				if (relatedManagementSubsidiaryRelations == null)
				{
					relatedManagementSubsidiaryRelations = new OrgManagementRelatedSubsidiaryCollection(this);
				}
				return relatedManagementSubsidiaryRelations;
			}
		}
		OrgManagementRelatedSubsidiaryCollection relatedManagementSubsidiaryRelations;

		public OrgManagementGroupingModel OrgManagementGroupingModel
		{
			get
			{
				if (orgManagementGroupingModel == null)
				{
					orgManagementGroupingModel = new OrgManagementGroupingModel(this);
					RegisterEditableChildObject(orgManagementGroupingModel);
				}

				return orgManagementGroupingModel;
			}
		}
		OrgManagementGroupingModel orgManagementGroupingModel;

		#endregion

		#region AllParentParties

		public OrgRelatedPartyCollection AllParentParties
		{
			get
			{
				if (allParentParties == null)
				{
					var localAllParentParties = new OrgRelatedPartyCollection(Factory, new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, this.PK));
					localAllParentParties.Load();
					allParentParties = localAllParentParties;
					allParentParties.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsRelatedPartiesSecurity);
					allParentParties.IsManagedForDataRefresh = true;
				}

				return allParentParties;
			}
		}

		OrgRelatedPartyCollection allParentParties;

		public OrgRelatedPartyCollectionView AllParentPartiesView
		{
			get
			{
				if (allParentPartiesView == null)
				{
					allParentPartiesView = new OrgRelatedPartyCollectionView(AllParentParties);
					allParentPartiesView.FilterByPartyType(filterPartyType, filterFreightDirection);
				}

				return allParentPartiesView;
			}
		}

		OrgRelatedPartyCollectionView allParentPartiesView;

		#endregion

		#endregion

		#region ConsigneeOrgRelatedParties

		[ChildEditable]
		[ActionFieldFollow(true)]
		public ConsigneeOrgRelatedPartySubsetCollection ConsigneeRelatedParties
		{
			get
			{
				if (consigneeRelatedParties == null)
				{
					consigneeRelatedParties = new ConsigneeOrgRelatedPartySubsetCollection(AllRelatedParties);
					RegisterEditableChildObject(consigneeRelatedParties);
					consigneeRelatedParties.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsRelatedPartiesSecurity);
				}

				return consigneeRelatedParties;
			}
		}

		ConsigneeOrgRelatedPartySubsetCollection consigneeRelatedParties;

		#endregion

		#region ConsignorRelatedParties

		[ChildEditable]
		[ActionFieldFollow(true)]
		public ConsignorOrgRelatedPartySubsetCollection ConsignorRelatedParties
		{
			get
			{
				if (consignorRelatedParties == null)
				{
					consignorRelatedParties = new ConsignorOrgRelatedPartySubsetCollection(AllRelatedParties);
					RegisterEditableChildObject(consignorRelatedParties);
					consignorRelatedParties.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsRelatedPartiesSecurity);
				}

				return consignorRelatedParties;
			}
		}

		ConsignorOrgRelatedPartySubsetCollection consignorRelatedParties;

		#endregion

		#region ForwarderRelatedParties

		[ChildEditable]
		[ActionFieldFollow(true)]
		public ForwarderOrgRelatedPartySubsetCollection ForwarderRelatedParties
		{
			get
			{
				if (forwarderRelatedParties == null)
				{
					forwarderRelatedParties = new ForwarderOrgRelatedPartySubsetCollection(AllRelatedParties);
					RegisterEditableChildObject(forwarderRelatedParties);
					forwarderRelatedParties.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsRelatedPartiesSecurity);
				}

				return forwarderRelatedParties;
			}
		}

		ForwarderOrgRelatedPartySubsetCollection forwarderRelatedParties;

		#endregion

		#region ServiceRelatedParties

		[ChildEditable]
		[ActionFieldFollow(true)]
		public ServiceOrgRelatedPartySubsetCollection ServiceRelatedParties
		{
			get
			{
				if (serviceRelatedParties == null)
				{
					serviceRelatedParties = new ServiceOrgRelatedPartySubsetCollection(AllRelatedParties);
					RegisterEditableChildObject(serviceRelatedParties);
					serviceRelatedParties.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsRelatedPartiesSecurity);
				}

				return serviceRelatedParties;
			}
		}

		ServiceOrgRelatedPartySubsetCollection serviceRelatedParties;

		#endregion

		#region OrgRefFacilities

		[ChildEditable]
		[ActionFieldFollow(true)]
		public OrgRefFacilityCollection OrgRefFacilities
		{
			get
			{
				if (orgRefFacilities == null)
				{
					orgRefFacilities = new OrgRefFacilityCollection(this, Factory);
					orgRefFacilities.Load();
					RegisterEditableChildObject(orgRefFacilities);
					orgRefFacilities.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsSecurity);
				}

				return orgRefFacilities;
			}
		}

		OrgRefFacilityCollection orgRefFacilities;

		#endregion

		#endregion

		#region CustomsCodes

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		[UniversalCopyCollectionEntity(OrgCusCodeSchema.Constants.TableName, OrgCusCodeSchema.Constants.OK_OH)]
		public OrgCusCodeCollection CustomsCodes
		{
			get
			{
				if (fCustomsCodesCollection == null)
				{
					fCustomsCodesCollection = new OrgCusCodeCollection(this, Factory);
					fCustomsCodesCollection.Load();
					RegisterEditableChildObject(fCustomsCodesCollection);
					fCustomsCodesCollection.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyConfigFinancialARAPRegistrationNumbersSecurity &&
																		 !SecurityProvider.HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity &&
																		 !SecurityProvider.HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity &&
																		 !SecurityProvider.HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity);

					List<IMatchingCusCode> list = new List<IMatchingCusCode>();
					fCustomsCodesCollection.CopyToList(list);
					OriginalCollections.CustomsCodes = list;
				}
				return fCustomsCodesCollection;
			}
		}

		OrgCusCodeCollection fCustomsCodesCollection;

		public OrgCusCode LocalBusinessRegNoObject
		{
			get { return CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType, GlbCompany.CurrentCompany.Country); }
		}

		public ZString LocalBusinessRegNo
		{
			get { return GetLocalCusCodeFromCollection(GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType); }
			set { SetLocalCustomsCode(GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType, value); }
		}

		/// <summary>
		/// Corresponding code is LegacyCode = LCS
		/// </summary>
		[MaxLength(OrgCusCode.Schema.OK_CustomsRegNoMaxLength)]
		public ZString LegacyCode
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.LegacySystemCode); }
		}

		public ZPropertyInfo LegacyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(LegacyCode)); }
		}

		/// <summary>
		/// Corresponding code is CarrierCode = CCC
		/// </summary>
		public ZString LocalCustomsCarrierCode
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.CarrierCode); }
		}

		/// <summary>
		/// Corresponding code is BondHolder = BHR
		/// </summary>
		public ZString BondHolderLocalCustomsCarrierCode
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.BondHolderCode); }
		}

		public ZString LocalReleaseAgentCode
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.ReleaseAgentCode); }
#if DEBUG
			set { SetLocalCustomsCode(OrgCusCode.CodeTypes.ReleaseAgentCode, value); }
#endif
		}

		public ZString LocalVATCode
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.VATCode); }
		}

		public ZString LocalRebateUserCode
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.RebateUserCode); }
		}

		/// <summary>
		/// Corresponding code is CCD
		/// </summary>
		public ZString LocalCustomsClientCode
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.CustomsClientCode); }
			set { SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, value); }
		}

		public ZString LocalCustomsSupplierCode
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.SupplierCode); }
			set { SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, value); }
		}

		[MaxLength(OrgCusCode.Schema.OK_CustomsRegNoMaxLength)]
		public ZString CustomsClientID
		{
			get
			{
				return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.CustomsClientID);
			}
			set
			{
				SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, value);
				CustomsClientIDInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomsClientIDInfo
		{
			get { return GetZPropertyInfo(OrgHeader.Schema.CustomsClientID); }
		}

		public ZString GetAgentCode(RefCountry country)
		{
			return GetCusCodeForCodeFromCollection(OrgCusCode.CodeTypes.AgentCode, country);
		}

		public void SetAgentCode(RefCountry country, string customsCode)
		{
			SetCustomsCode(OrgCusCode.CodeTypes.AgentCode, country, customsCode);
		}

		public ZString GetBuyerCode(RefCountry country)
		{
			return GetCusCodeForCodeFromCollection(OrgCusCode.CodeTypes.BuyerCode, country);
		}

		public ZString UINCodeForIndia => GetCusCodeForCodeFromCollection(IndiaOrgCusCodeInfo.OrgCusCodes.UIN, Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.India));

		public ZString LocalManifestID
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.ManifestProviderID); }
			set { SetLocalCustomsCode(OrgCusCode.CodeTypes.ManifestProviderID, value); }
		}

		public ZString LocalPrincipalID
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.CarrierCode); }
			set { SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, value); }
		}

		#region Road Carrier Registration Number

		public ZString RoadCarrierRegistrationNumber
		{
			get { return GetLocalCusCodeFromCollection(OrgCusCode.CodeTypes.RoadCarrierRegistration); }
		}

		#endregion

		public ZString SCACCode
		{
			get
			{
				RefCountry usCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates);
				OrgCusCode cusCode = CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, usCountry);

				if (cusCode != null)
				{
					return cusCode.OK_CustomsRegNo;
				}

				return ZString.Empty;
			}
		}

		public ZString C1CCode
		{
			get
			{
				RefCountry usCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates);
				OrgCusCode cusCode = CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, usCountry);

				if (cusCode != null)
				{
					return cusCode.OK_CustomsRegNo;
				}

				return ZString.Empty;
			}
		}

		protected ZString GetLocalCusCodeFromCollection(ZString code)
		{
			return GetCusCodeForCodeFromCollection(code, GlbCompany.CurrentCompany.Country);
		}

		protected ZString GetCusCodeForCodeFromCollection(ZString code, RefCountry country)
		{
			OrgCusCode cusCode = CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(code, country);
			return cusCode == null ? ZString.Empty : cusCode.OK_CustomsRegNo;
		}

		public (ISecurityCheckpoint FailingCheckpoint, string CompanyARAPInfo) GetFailingCheckpointAndCompanyARAPInfoWhenModifyRegistrationNumber(bool isPrimaryCodeType)
		{
			var isARAP = IsARAPForAny(true, out var companyARAPInfo);

			return (GetFailingCheckpointWhenModifyRegistrationNumberCore(isARAP, isPrimaryCodeType), companyARAPInfo);
		}

		public ISecurityCheckpoint GetFailingCheckpointWhenModifyRegistrationNumber(bool isPrimaryCodeType = false)
		{
			var isARAP = IsARAPForAny(false, out _);

			return GetFailingCheckpointWhenModifyRegistrationNumberCore(isARAP, isPrimaryCodeType);
		}

		ISecurityCheckpoint GetFailingCheckpointWhenModifyRegistrationNumberCore(bool isARAP, bool isPrimaryCodeType)
		{
			ISecurityCheckpoint failingCheckpoint = null;
			if (isPrimaryCodeType)
			{
				if (isARAP && !SecurityProvider.HasModifyConfigFinancialARAPRegistrationNumbersSecurity)
				{
					failingCheckpoint = Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers;
				}
				else if (!isARAP && !SecurityProvider.HasModifyConfigFinancialNonARAPRegistrationNumbersSecurity)
				{
					failingCheckpoint = Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers;
				}
			}
			else
			{
				if (isARAP && !SecurityProvider.HasModifyConfigNonFinancialARAPRegistrationNumbersSecurity)
				{
					failingCheckpoint = Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers;
				}
				else if (!isARAP && !SecurityProvider.HasModifyConfigNonFinancialNonARAPRegistrationNumbersSecurity)
				{
					failingCheckpoint = Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers;
				}
			}

			return failingCheckpoint;
		}

		bool IsARAPForAny(bool needCompanyARAPInfo, out string companyARAPInfo)
		{
			var result = false;
			if (OH_IsDebtor || OH_IsCreditor)
			{
				result = true;
				companyARAPInfo = GetCompanyARAPInfo(needCompanyARAPInfo, CompanyData);
			}
			else
			{
				var query = new ZQuery(OrgCompanyDataSchema.OB_OH, PK);
				var subQuery = new ZQuery();
				subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
				subQuery.AddToFilter(JoinCondition.Or, OrgCompanyDataSchema.OB_IsDebtor, true);
				query.AddToFilter(subQuery);
				var companyData = Factory.LoadTop1<OrgCompanyData>(query);
				result = companyData != null;
				companyARAPInfo = GetCompanyARAPInfo(needCompanyARAPInfo, companyData);
			}

			return result;
		}

		string GetCompanyARAPInfo(bool needCompanyARAPInfo, OrgCompanyData companyData)
		{
			var result = string.Empty;

			if (needCompanyARAPInfo && companyData != null && companyData.Company != null)
			{
				var companyCode = companyData.Company.GC_Code;

				if (companyData.OB_IsDebtor)
				{
					if (companyData.OB_IsCreditor)
					{
						result = Res.GetString("DEAD0344-3AEE-4DB2-AB0E-F19F79E7E3BF", "This organization is marked as A/R and A/P under Company {0}.", companyCode);
					}
					else
					{
						result = Res.GetString("0EE17657-566B-42D0-A1F7-0B91FAFE701D", "This organization is marked as A/R under Company {0}.", companyCode);
					}
				}
				else if (companyData.OB_IsCreditor)
				{
					result = Res.GetString("62AA7746-49D4-4BB6-9AF1-C234D13D504A", "This organization is marked as A/P under Company {0}.", companyCode);
				}
			}

			return result;
		}

		#endregion

		#region Contacts

		public ZBool ShowSystemGeneratedContacts
		{
			get
			{
				return showSystemGeneratedContacts;
			}
			set
			{
				if (showSystemGeneratedContacts != value)
				{
					using (SuspendSettingHasChanges())
					{
						SetNonPersistentPropertyValue(ShowSystemGeneratedContactsInfo, ref showSystemGeneratedContacts, value);

						if (contacts != null)
						{
							contacts.Load(GetContactsFilter());
							contacts.RefreshBindingIncludingChildren();
						}
					}
				}
			}
		}

		ZBool showSystemGeneratedContacts = true;

		public IDisposable ToggleShowSystemGeneratedContactsFlagTemporarily(bool flagValue)
		{
			bool originalShowSystemGeneratedContacts = ShowSystemGeneratedContacts;
			ShowSystemGeneratedContacts = flagValue;
			return new DisposableAction(() => ShowSystemGeneratedContacts = originalShowSystemGeneratedContacts);
		}

		public ZPropertyInfo ShowSystemGeneratedContactsInfo
		{
			get { return GetZPropertyInfo(nameof(ShowSystemGeneratedContacts)); }
		}

		ZQuery GetContactsFilter(bool onlyShowActive = false)
		{
			ZQuery filter;
			if (ShowSystemGeneratedContacts)
			{
				filter = new ZQuery();
			}
			else
			{
				var dbFilter = new ZDBOnlyQuery(typeof(OrgContact));
				dbFilter.AddToFilter(OrgContactSchema.OC_SystemCreateUser, SQLComparisonOperator.NotEqual, User.ServiceUserCode);
				filter = dbFilter;
			}

			if (onlyShowActive)
			{
				filter.AddToFilter(OrgContactSchema.OC_IsActive, SQLComparisonOperator.Equal, true);
			}

			return filter;
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContactDependentCollection Contacts
		{
			get
			{
				if (contacts == null)
				{
					ZQuery filter = GetContactsFilter();
					contacts = new OrgContactDependentCollection(this, filter);
					contacts.Load();
					RegisterEditableChildObject(contacts);
					contacts.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyContactSecurity);
					contacts.CountChanged += Contacts_CountChanged;
				}

				return contacts;
			}
		}

		void Contacts_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			InvalidateContactNamesLookupCache();
		}

		OrgContactDependentCollection contacts;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public virtual OrgSuppressedDocumentCollection SuppressedDocuments
		{
			get
			{
				if (suppressedDocuments == null)
				{
					suppressedDocuments = new OrgSuppressedDocumentCollection(this, Factory);
					suppressedDocuments.Load();
					RegisterEditableChildObject(suppressedDocuments);
					suppressedDocuments.SetReadOnlyIncludingChildren(!this.SecurityProvider.HasModifyContactDocDeliveryDetailsSecurity);
				}
				return suppressedDocuments;
			}
		}

		OrgSuppressedDocumentCollection suppressedDocuments;

		OrgDocumentCollection GetDummyContactToSuppressDocs()
		{
			var filter = new ZDBOnlyQuery(typeof(OrgContact)); // Not load the contact with dummy name created by user
			filter.AddToFilter(OrgContactSchema.OC_ContactName, "DUMMY CONTACT TO SUPPRESS DOCS");
			filter.AddToFilter(OrgContactSchema.OC_OH, PK);
			var contact = Factory.LoadTop1(typeof(OrgContact), filter) as OrgContact;
			return contact?.Documents ?? new OrgDocumentCollection(Factory);
		}

		public List<OrgDocument> SuppressedDocumentsIncludeDummyContact
		{
			get
			{
				if (suppressedDocumentsIncludeDummyContact == null)
				{
					suppressedDocumentsIncludeDummyContact = new List<OrgDocument>(GetDummyContactToSuppressDocs().OfType<OrgDocument>());
					suppressedDocumentsIncludeDummyContact.AddRange(SuppressedDocuments.OfType<OrgDocument>());
				}
				return suppressedDocumentsIncludeDummyContact;
			}
		}
		List<OrgDocument> suppressedDocumentsIncludeDummyContact;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContactDependentCollection ContactsActive
		{
			get
			{
				if (activeContacts == null)
				{
					var filter = new ZQuery(OrgContactSchema.OC_IsActive, ZBool.True);
					activeContacts = new OrgContactDependentCollection(this, filter);
					activeContacts.Load();
					RegisterEditableChildObject(activeContacts);
				}

				return activeContacts;
			}
		}

		OrgContactDependentCollection activeContacts;

		public OrgContactDependentCollection GetActiveContacts()
		{
			var activeContacts = new OrgContactDependentCollection(this, GetContactsFilter(true));
			activeContacts.Load();
			return activeContacts;
		}

		public bool IncludeInactiveContacts
		{
			get { return FilteredContacts.IncludeInactiveContacts; }
			set { FilteredContacts.IncludeInactiveContacts = value; }
		}

		public bool OnlyShowWebAccessEnabledContacts
		{
			get { return FilteredContacts.OnlyShowWebAccessEnabledContacts; }
			set { FilteredContacts.OnlyShowWebAccessEnabledContacts = value; }
		}

		public string ContactsFilterString
		{
			get { return FilteredContacts.FilterString; }
			set { FilteredContacts.FilterString = value; }
		}

		[List("Lookups.ContactsFilterOptionList")]
		public string ContactsFilterOption
		{
			get { return FilteredContacts.FilterOption; }
			set { FilteredContacts.FilterOption = value; }
		}

		public FilteredContactsCollection FilteredContacts
		{
			get
			{
				return filteredContacts ?? (filteredContacts = CreateFilteredContactsCollection());
			}
		}

		protected virtual FilteredContactsCollection CreateFilteredContactsCollection() => new FilteredContactsCollection(Contacts);

		FilteredContactsCollection filteredContacts;

		public OrgContactDependentCollection SalesCallContacts
		{
			get
			{
				if (salesCallContacts == null)
				{
					salesCallContacts = new OrgContactDependentCollection(this, Factory);
					salesCallContacts.IsManagedForDataRefresh = true;
				}

				return salesCallContacts;
			}
		}

		OrgContactDependentCollection salesCallContacts;

		internal IEnumerable<OrgContact> ContactsLoaded => (contacts ?? activeContacts)?.OfType<OrgContact>() ?? Enumerable.Empty<OrgContact>();

		#endregion

		#region Contact Allocations

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgAllocatedContactCollection AllocatedContacts
		{
			get
			{
				if (allocatedContactsCollection == null)
				{
					allocatedContactsCollection = new OrgAllocatedContactCollection(this);
					allocatedContactsCollection.Load();
					RegisterEditableChildObject(allocatedContactsCollection);
					allocatedContactsCollection.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyContactSecurity);
				}

				return allocatedContactsCollection;
			}
		}

		OrgAllocatedContactCollection allocatedContactsCollection;

		#endregion

		#region IEnrichmentDataProvider

		OrgAddressDependentCollection enrichmentAddressesCollection;

		OrgAddressDependentCollection IEnrichmentDataProvider.Addresses
		{
			get
			{
				if (enrichmentAddressesCollection == null)
				{
					enrichmentAddressesCollection = new OrgAddressDependentCollection(this);
					enrichmentAddressesCollection.Load();
				}

				return enrichmentAddressesCollection;
			}
		}

		OrgBrandOrRelatedNameCollection IEnrichmentDataProvider.BrandsOrRelatedNames
		{
			get
			{
				var brandsOrRelatedNamesCollection = new OrgBrandOrRelatedNameCollection(this);
				brandsOrRelatedNamesCollection.Load();

				return brandsOrRelatedNamesCollection;
			}
		}

		OrgContactDependentCollection IEnrichmentDataProvider.Contacts
		{
			get
			{
				var filter = GetContactsFilter();
				var contactsCollection = new OrgContactDependentCollection(this, filter);
				contactsCollection.Load();

				return contactsCollection;
			}
		}

		OrgCusCodeCollection IEnrichmentDataProvider.CustomsCodes
		{
			get
			{
				var customsCodesCollection = new OrgCusCodeCollection(this, Factory);
				customsCodesCollection.Load();

				return customsCodesCollection;
			}
		}

		OrgWebURLDependentCollection IEnrichmentDataProvider.OrgWebURLs
		{
			get
			{
				var orgWebURLsCollection = new OrgWebURLDependentCollection(this);
				orgWebURLsCollection.Load();

				return orgWebURLsCollection;
			}
		}

		ZString IEnrichmentDataProvider.OH_RL_NKClosestPort
		{
			get
			{
				var result = ZString.Empty;

				if (OH_IsGlobalAccount)
				{
					var addressResult = GetMainAddressForEnrichment(Core.Constants.Languages.English) ?? GetMainAddressForEnrichment(null);

					if (addressResult != null)
					{
						result = ((INeedRow)addressResult).Row[OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode].ToString();
					}

					if (result.IsEmpty)
					{
						result = base.OH_RL_NKClosestPort;
					}
				}
				else
				{
					result = base.OH_RL_NKClosestPort;
				}

				return result;
			}
		}

		OrgAddress GetMainAddressForEnrichment(string preferedLanguage)
		{
			OrgAddress addressResult = null;

			foreach (OrgAddress address in ((IEnrichmentDataProvider)this).Addresses)
			{
				if (!address.IsDeleted &&
					address.AddressCapability.GetCapabilityEnabled(OrgAddressType.Office) &&
					address.AddressCapability.GetIsMainAddress(OrgAddressType.Office) &&
					(string.IsNullOrEmpty(preferedLanguage) || address.OA_Language == preferedLanguage))
				{
					addressResult = address;
					break;
				}
			}

			return addressResult;
		}

		int IEnrichmentDataProvider.OrganisationTypes
		{
			get
			{
				var result = OrganisationTypes.None;

				// Skip Company Data Related Organization Types (Debtor, Creditor), it connect with current company
				if (OH_IsConsignor)
				{
					result |= OrganisationTypes.Consignor;
				}

				if (OH_IsConsignee)
				{
					result |= OrganisationTypes.Consignee;
				}

				if (OH_IsTransportClient)
				{
					result |= OrganisationTypes.TransportClient;
				}

				if (OH_IsShippingProvider)
				{
					result |= OrganisationTypes.Carrier;
				}

				if (OH_IsForwarder)
				{
					result |= OrganisationTypes.Forwarder;
				}

				if (OH_IsBroker)
				{
					result |= OrganisationTypes.Broker;
				}

				if (OH_IsMiscFreightServices)
				{
					result |= OrganisationTypes.Services;
				}

				if (OH_IsCompetitor)
				{
					result |= OrganisationTypes.Competitor;
				}

				if (OH_IsSalesLead)
				{
					result |= OrganisationTypes.Sales;
				}

				if (OH_IsWarehouseClient)
				{
					result |= OrganisationTypes.WarehouseClient;
				}

				if (OH_IsDistributionCentre)
				{
					result |= OrganisationTypes.DistributionCentre;
				}

				if (OH_IsControllingAgent)
				{
					result |= OrganisationTypes.ControllingAgent;
				}

				if (OH_IsControllingCustomer)
				{
					result |= OrganisationTypes.ControllingCustomer;
				}

				return (int)result;
			}
		}

		#endregion

		#region Addresses

		public bool CanAddNewAddress
		{
			get { return !IsBeingDeleted && !IsDeleted && !IsNull; }
		}

		[ChildEditable(false)]
		public OrgAddressDependentCollection Addresses
		{
			get
			{
				OrgAddressDependentCollection result = AddressesNoAutoCreate;
				if (result.Count == 0 && CanAddNewAddress)
				{
					OrgAddress address = result.AddNewMainAddress();
					result.HasChanges = false;
				}

				return result;
			}
		}

		List<IMatchingAddress> IOrgHeaderWithAddressesNoAutoCreate.AddressesNoAutoCreate
		{
			get { return new List<IMatchingAddress>(AddressesNoAutoCreate.Cast<OrgAddress>()); }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgAddressDependentCollection AddressesNoAutoCreate
		{
			get
			{
				if (addresses == null)
				{
					var tempAddresses = new OrgAddressDependentCollection(this);
					tempAddresses.Load();
					addresses = tempAddresses;
					RegisterEditableChildObject(addresses);
					List<IMatchingAddress> list = new List<IMatchingAddress>();
					addresses.CopyToList(list);
					OriginalCollections.Addresses = list;
					if (OriginalCollections.OrganisationNamesExceptBrands == null && !IsDeleted)
					{
						OriginalCollections.OrganisationNamesExceptBrands = OrganisationNamesExceptBrands;
					}
					addresses.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyAddressSecurity);
				}

				return addresses;
			}
		}

		ActiveBusinessObjectCollection<OrgAddress> addressesActive;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ActiveBusinessObjectCollection<OrgAddress> AddressesActive
		{
			get
			{
				if (addressesActive == null)
				{
					addressesActive = new ActiveBusinessObjectCollection<OrgAddress>(this, new ZQuery(OrgAddressSchema.OA_IsActive, ZBool.True));
					RegisterEditableChildObject(addressesActive);
				}

				return addressesActive;
			}
		}

		/// <summary>
		/// This Addresses collection changes dynamically to:
		///   - show all Addresses
		///   - show only active Addresses
		/// Based on the IncludeInactiveAddresses property.
		/// </summary>
		ActiveOrAllAddressesCollection activeOrAllAddresses;
		public ActiveOrAllAddressesCollection ActiveOrAllAddresses
		{
			get
			{
				return activeOrAllAddresses ?? (activeOrAllAddresses = new ActiveOrAllAddressesCollection(Addresses));
			}
		}

		public ZBool IncludeInactiveAddresses
		{
			get { return ActiveOrAllAddresses.IncludeInactiveAddresses; }
			set
			{
				ActiveOrAllAddresses.IncludeInactiveAddresses = value;
			}
		}

		public OrgAddress CustomsAddress
		{
			get
			{
				if (customsAddressCache == null || (customsAddressCache.Value != null && customsAddressCache.Value.IsDeleted))
				{
					customsAddressCache = new CachedValue<OrgAddress>(GetCustomsAddressCore);
				}
				return customsAddressCache != null ? customsAddressCache.Value : null;
			}
		}

		OrgAddress GetCustomsAddressCore()
		{
			return Addresses.CustomsAddress;
		}

		CachedValue<OrgAddress> customsAddressCache;

		public OrgAddress MainAddress
		{
			get
			{
				var mainAddress = (mainAddressCache != null && mainAddressCache.Value != null && !mainAddressCache.Value.IsDeleted) ? mainAddressCache.Value : GetMainAddressCore();
				return MainAddressLocalizedForDocument ?? mainAddress;
			}
		}

		public bool HasEnglishMainAddress
		{
			get { return Res.IsEnglish(MainAddress.OA_Language) || MainAddress.TranslatedAddresses.Any(a => Res.IsEnglish(a.OTA_Language)); }
		}

		bool NoNeedMainAddressLocalizedForDocument { get; set; }
		bool isLocalizingMainAddressForDocument;
		OrgAddress MainAddressLocalizedForDocument
		{
			get
			{
				OrgAddress result = null;
				if (!isLocalizingMainAddressForDocument && !NoNeedMainAddressLocalizedForDocument)
				{
					isLocalizingMainAddressForDocument = true;
					try
					{
						if (DocumentGenerationHelper.IsGeneratingDocument && Addresses.MainAddress != null)
						{
							if (Addresses.MainAddress.Language == Res.CurrentLanguage)
							{
								return Addresses.MainAddress;
							}

							var translatedAddress = Addresses.MainAddress.GetTranslatedAddressInSpecificLanguage(Res.CurrentLanguage);
							if (translatedAddress != null)
							{
								result = CreateTempOrgAddressFromOrgTranslatedAddress(translatedAddress, Addresses.MainAddress, Res.CurrentLanguage);
							}
							else
							{
								result = Addresses.DefaultAddressOfType(OrgAddressType.Office, activeOnly: true, preferredLanguage: Res.CurrentLanguage);
							}
						}
					}
					finally
					{
						isLocalizingMainAddressForDocument = false;
					}
				}
				return result;
			}
		}

		OrgAddress tempMainAddressLocalizedForDocument;

		protected virtual OrgAddress CreateTempOrgAddressFromOrgTranslatedAddress(OrgTranslatedAddress translatedAddress, OrgAddress originalAddress, string language)
		{
			if (tempMainAddressLocalizedForDocument == null || tempMainAddressLocalizedForDocument.IsDeleted)
			{
				tempMainAddressLocalizedForDocument = ReadOnlyFactory.New<OrgAddress>();
				tempMainAddressLocalizedForDocument.SuspendValidation();

				tempMainAddressLocalizedForDocument.OA_OH = originalAddress.OA_OH;
				if (tempMainAddressLocalizedForDocument.Header != null)
				{
					tempMainAddressLocalizedForDocument.Header.NoNeedMainAddressLocalizedForDocument = true;
				}
			}

			var addressCopyArgs = new BusinessObjectCloneArgs(new string[] { OrgAddressSchema.Constants.OA_RL_NKRelatedPortCode });
			tempMainAddressLocalizedForDocument.CopyPersistentValuesFrom(originalAddress, addressCopyArgs);
			tempMainAddressLocalizedForDocument.OA_RL_NKRelatedPortCode = originalAddress.OA_RL_NKRelatedPortCode;

			tempMainAddressLocalizedForDocument.Language = language;
			tempMainAddressLocalizedForDocument.Address1 = translatedAddress.Address1;
			tempMainAddressLocalizedForDocument.Address2 = translatedAddress.Address2;
			tempMainAddressLocalizedForDocument.City = translatedAddress.City.Substring(0, tempMainAddressLocalizedForDocument.City_MaxLength);
			tempMainAddressLocalizedForDocument.Postcode = translatedAddress.Postcode.Substring(0, tempMainAddressLocalizedForDocument.PostCode_MaxLength);
			tempMainAddressLocalizedForDocument.CompanyName = translatedAddress.CompanyName.Substring(0, tempMainAddressLocalizedForDocument.CompanyName_MaxLength);
			return tempMainAddressLocalizedForDocument;
		}

		IOrgAddress IOrgHeader.MainAddress
		{
			get { return MainAddress; }
		}

		OrgAddress GetMainAddressCore()
		{
			return Addresses.MainAddress;
		}

		IDisposable IMatchingOrganisation.CacheMainAddress()
		{
			mainAddressCache = new CachedValue<OrgAddress>(GetMainAddressCore);
			return new DisposableAction(() => mainAddressCache = null);
		}

		CachedValue<OrgAddress> mainAddressCache;

		// We need this as binding to the MainAddress+OA_Phone (for example) causes problems
		// where values are not refreshed correctly on the GUI.
		// However binding to the below collection is a work around for this problem.
		OrgAddressCollection mainAddressCollection;
		public OrgAddressCollection MainAddressCollection
		{
			get
			{
				if (mainAddressCollection == null)
				{
					mainAddressCollection = new OrgAddressCollection(Factory);
					if (MainAddress != null)
					{
						mainAddressCollection.Add(MainAddress);
					}
				}
				return mainAddressCollection;
			}
		}

		OrgAddressDependentCollection addresses;

		[BusinessObjectTestExclude]
		protected OrgAddressDependentCollection AddressesInternal
		{
			get { return addresses; }
			set { addresses = value; }
		}

		public OrgAddressHelper AddressHelper
		{
			get { return addressHelper ?? (addressHelper = OrgAddressHelper.New(this)); }
		}

		OrgAddressHelper addressHelper;

		#endregion

		#region MainWebURL

		public OrgWebURL MainWebURL
		{
			get { return OrgWebURLs.MainURL; }
		}

		public ZString CartageTransportWebSite
		{
			get
			{
				OrgWebURL[] urls = OrgWebURLs.FindByUrlType(OrgWebUrlList.Codes.CartageTracking);
				return urls.Length > 0 ? urls[0].PU_URL : (ZString)string.Empty;
			}
		}

		#endregion

		#region Pattern Matching

		public bool IsLikelyDuplicate()
		{
			return IsLikelyDuplicate(true);
		}

		public bool IsLikelyDuplicate(bool shouldIncludeUnmatchedOrgInList)
		{
			bool result = false;

			if (MustPerformCheckForDuplicateOrganisations && !IsInDatabase)
			{
				if (PatternMatchRequiresRegen)
				{
					OrgPatternMatchCollection patternMatches = PatternMatchesForThisOrg;
					if (patternMatches != null && PatternMatchRequiresRegen)
					{
						patternMatches.GeneratePatternMatchesFromOrg(this);
						PatternMatchRequiresRegen = false;
					}
				}

				SimilarOrgFinder.FindSimilarOrganisations(shouldIncludeUnmatchedOrgInList);
				result = SimilarOrgMatches.LikelyMatchesExist;
			}

			return result;
		}

		public bool MustPerformCheckForDuplicateOrganisations { get; set; } = true;

		#region Pattern Match Overrides

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		[UniversalCopyCollectionEntity(OrgPatternMatchOverrideSchema.Constants.TableName, OrgPatternMatchOverrideSchema.Constants.OO_OH)]
		public OrgPatternMatchOverrideCollection PatternMatchOverrides_ForBinding
		{
			get
			{
				if (patternMatchOverrides == null)
				{
					patternMatchOverrides = new OrgPatternMatchOverrideCollection(this, Factory);
					patternMatchOverrides.Load();
					RegisterEditableChildObject(patternMatchOverrides);
					patternMatchOverrides.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyConfigEDICodeMappingSecurity);
				}

				return patternMatchOverrides;
			}
		}

		OrgPatternMatchOverrideCollection patternMatchOverrides;

		#endregion

		#region Pattern Matches For This Org

		public OrgPatternMatchCollection PatternMatchesForThisOrg
		{
			get
			{
				if (patternMatchesForThisOrg == null || patternMatchRegeneratedSinceLastLoad)
				{
					ZQuery query = new ZQuery(OrgPatternMatchSchema.OS_OH, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					var localPatternMatchesForThisOrg = GetNewPatternMatchesForThisOrgCollection(Factory, query);
					localPatternMatchesForThisOrg.Load();
					patternMatchesForThisOrg = localPatternMatchesForThisOrg;

					if (!IsInDatabase && !IsBeingDeleted && !patternMatchRegeneratedSinceLastLoad)
					{
						patternMatchesForThisOrg.GeneratePatternMatchesFromOrg(this);
						PatternMatchRequiresRegen = false;
					}
					patternMatchRegeneratedSinceLastLoad = false;
				}

				return patternMatchesForThisOrg;
			}
		}
		OrgPatternMatchCollection patternMatchesForThisOrg;

		protected virtual OrgPatternMatchCollection GetNewPatternMatchesForThisOrgCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
		{
			return new OrgPatternMatchCollection(factory, additionalFilter);
		}

		#endregion

		#endregion

		#region Trade Lanes

		public OrgSalesCollection SalesCollection
		{
			get
			{
				if (salesCollection == null)
				{
					var lsalesCollection = new OrgSalesCollection(this);
					lsalesCollection.Load();
					salesCollection = lsalesCollection;
					salesCollection.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifySalesTradeProfileSecurity);
				}
				return salesCollection;
			}
		}
		OrgSalesCollection salesCollection;

		#endregion

		#region ConsigneeContainerPenalties

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContainerDetentionCollection ConsigneeContainerPenalties
		{
			get
			{
				if (consigneeContainerPenalties == null)
				{
					consigneeContainerPenalties = new OrgContainerDetentionCollection(this, OrgContainerDetentionCollection.ParentType.Consignee, ZString.Empty, ZString.Empty);
					RegisterEditableChildObject(consigneeContainerPenalties);
					consigneeContainerPenalties.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsSecurity);
				}

				return consigneeContainerPenalties;
			}
		}

		OrgContainerDetentionCollection consigneeContainerPenalties;

		#endregion

		#region ConsignorContainerPenalties

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContainerDetentionCollection ConsignorContainerPenalties
		{
			get
			{
				if (consignorContainerPenalties == null)
				{
					consignorContainerPenalties = new OrgContainerDetentionCollection(this, OrgContainerDetentionCollection.ParentType.Consignor, ZString.Empty, ZString.Empty);
					RegisterEditableChildObject(consignorContainerPenalties);
					consignorContainerPenalties.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsSecurity);
				}

				return consignorContainerPenalties;
			}
		}

		OrgContainerDetentionCollection consignorContainerPenalties;

		#endregion

		#region ConsigneeCTOStorage

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContainerDetentionCollection ConsigneeCTOStorages
		{
			get
			{
				if (consigneeCTOStorages == null)
				{
					consigneeCTOStorages = new OrgContainerDetentionCollection(this, OrgContainerDetentionCollection.ParentType.Consignee, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
					RegisterEditableChildObject(consigneeCTOStorages);
					consigneeCTOStorages.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsSecurity);
				}

				return consigneeCTOStorages;
			}
		}

		OrgContainerDetentionCollection consigneeCTOStorages;

		#endregion

		#region ConsignorCTOStorage

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContainerDetentionCollection ConsignorCTOStorages
		{
			get
			{
				if (consignorCTOStorages == null)
				{
					consignorCTOStorages = new OrgContainerDetentionCollection(this, OrgContainerDetentionCollection.ParentType.Consignor, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
					RegisterEditableChildObject(consignorCTOStorages);
					consignorCTOStorages.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsSecurity);
				}

				return consignorCTOStorages;
			}
		}

		OrgContainerDetentionCollection consignorCTOStorages;

		#endregion

		#region ServiceImportCTOStorages

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContainerDetentionCollection ServiceImportCTOStorages
		{
			get
			{
				if (serviceImportCTOStorages == null)
				{
					serviceImportCTOStorages = new OrgContainerDetentionCollection(this, OrgContainerDetentionCollection.ParentType.ServiceIMP, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
					RegisterEditableChildObject(serviceImportCTOStorages);
					serviceImportCTOStorages.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsSecurity);
				}

				return serviceImportCTOStorages;
			}
		}

		OrgContainerDetentionCollection serviceImportCTOStorages;

		#endregion

		#region ServiceExportCTOStorages

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContainerDetentionCollection ServiceExportCTOStorages
		{
			get
			{
				if (serviceExportCTOStorages == null)
				{
					serviceExportCTOStorages = new OrgContainerDetentionCollection(this, OrgContainerDetentionCollection.ParentType.ServiceEXP, Constants.ContainerDetentionPenaltyType.STO, Constants.ContainerPenaltyCreditorType.Codes.CTO);
					RegisterEditableChildObject(serviceExportCTOStorages);
					serviceExportCTOStorages.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsSecurity);
				}

				return serviceExportCTOStorages;
			}
		}

		OrgContainerDetentionCollection serviceExportCTOStorages;

		#endregion

		#region CarrierContainerPenalties

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContainerDetentionCollection CarrierContainerPenalties
		{
			get
			{
				if (carrierContainerPenalties == null)
				{
					carrierContainerPenalties = new OrgContainerDetentionCollection(this, OrgContainerDetentionCollection.ParentType.Carrier, ZString.Empty, Constants.ContainerPenaltyCreditorType.Codes.Carrier);
					RegisterEditableChildObject(carrierContainerPenalties);
					carrierContainerPenalties.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsSecurity);
				}

				return carrierContainerPenalties;
			}
		}

		OrgContainerDetentionCollection carrierContainerPenalties;

		#endregion

		#region Air Clients

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgTradeProspectByCompetitorCollection Clients
		{
			get
			{
				if (clients == null)
				{
					clients = new OrgTradeProspectByCompetitorCollection(this);
					clients.Load();
					RegisterEditableChildObject(clients);
					clients.SetReadOnlyIncludingChildren(true);
				}

				return clients;
			}
		}

		OrgTradeProspectByCompetitorCollection clients;

		#endregion

		#region OrgUserFlags

		public OrgUserFlagCollection OrgUserFlags
		{
			get
			{
				if (orgUserFlags == null)
				{
					orgUserFlags = new OrgUserFlagCollection(this);
					orgUserFlags.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifySalesClientSummarySecurity);
				}
				return orgUserFlags;
			}
		}
		OrgUserFlagCollection orgUserFlags;

		#endregion

		#region Competitors

		[ChildEditable]
		public OrgCompetitorCollection Competitors
		{
			get
			{
				if (competitors == null)
				{
					competitors = new OrgCompetitorCollection(this);
					RegisterEditableChildObject(competitors);
					competitors.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifySalesClientSummarySecurity);
				}
				return competitors;
			}
		}
		OrgCompetitorCollection competitors;

		#endregion

		#region Sales Calls

		public OrgSalesCallCollection SalesCalls
		{
			get
			{
				if (salesCalls == null)
				{
					salesCalls = new OrgSalesCallCollection(this);
				}
				return salesCalls;
			}
		}
		OrgSalesCallCollection salesCalls;

		#endregion

		#region Collection Notes

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCollectionNoteCollection CollectionNotes
		{
			get
			{
				if (collectionNotes == null)
				{
					var localCollectionNotes = new OrgCollectionNoteCollection(this);
					localCollectionNotes.Load(GetCollectionCallsQuery());
					collectionNotes = localCollectionNotes;
					RegisterEditableChildObject(collectionNotes);
					SortInfo sortInfo = new SortInfo(OrgCollectionNote.Schema.PN_SystemCreateTimeUtc, ListSortDirection.Descending);
					collectionNotes.Sort(sortInfo);
					collectionNotes.SetReadOnlyIncludingChildren(!(SecurityProvider.HasModifyDetailsSecurity || Env.Security.ReceivablesCollectionCallsEdit.IsAllowed));
				}

				return collectionNotes;
			}
		}

		OrgCollectionNoteCollection collectionNotes;

		public ZBool IsLoadedFromCollectionCall { get; set; }

		#endregion

		#region Supplier Links

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgSupplierLinkCollection SupplierLinks
		{
			get
			{
				if (supplierLinks == null)
				{
					supplierLinks = new OrgSupplierLinkCollection(this, Factory);
					supplierLinks.Load();
					RegisterEditableChildObject(supplierLinks);
					supplierLinks.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyConsigneeRelationshipsSecurity);
				}

				return supplierLinks;
			}
		}

		OrgSupplierLinkCollection supplierLinks;

		#endregion

		#region Buyer Links

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgBuyerLinkCollection BuyerLinks
		{
			get
			{
				if (buyerLinks == null)
				{
					buyerLinks = new OrgBuyerLinkCollection(this, Factory);
					buyerLinks.Load();
					RegisterEditableChildObject(buyerLinks);
					buyerLinks.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyConsignorRelationshipsSecurity);
				}

				return buyerLinks;
			}
		}

		OrgBuyerLinkCollection buyerLinks;

		#endregion

		#region CustomForms

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public CustomFormsCollection CustomFormLabels
		{
			get
			{
				if (customFormLabels == null)
				{
					customFormLabels = new CustomFormsCollection(this, Factory);
					customFormLabels.Load();
					RegisterEditableChildObject(customFormLabels);
					customFormLabels.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCustomSecurity);
				}

				return customFormLabels;
			}
		}

		CustomFormsCollection customFormLabels;

		#endregion

		#region CustomDocuments

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public CustomDocumentsCollection CustomDocumentLabels
		{
			get
			{
				if (customDocumentLabels == null)
				{
					customDocumentLabels = new CustomDocumentsCollection(this, Factory);
					customDocumentLabels.Load();
					RegisterEditableChildObject(customDocumentLabels);
					customDocumentLabels.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCustomSecurity);
				}

				return customDocumentLabels;
			}
		}

		CustomDocumentsCollection customDocumentLabels;

		#endregion

		#region Custom Labels

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCustomLabelsCollection CustomLabels
		{
			get
			{
				if (customLabels == null)
				{
					customLabels = new OrgCustomLabelsCollection(this, Factory);
					customLabels.Load();
					RegisterEditableChildObject(customLabels);
					customLabels.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCustomSecurity);
				}

				return customLabels;
			}
		}

		OrgCustomLabelsCollection customLabels;

		#endregion

		#region UNLOCO

		public RefUNLOCO UNLOCO
		{
			get { return ClosestPort; }
		}

		#endregion

		#region Communication Modes

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		[UniversalCopyCollectionEntity(EDICommunicationsModeSchema.Constants.TableName, EDICommunicationsModeSchema.Constants.EK_ParentID, EDICommunicationsModeSchema.Constants.EK_ParentTableCode)]
		public EDICommunicationsModeDependentCollection EDICommunicationsModes
		{
			get
			{
				if (ediCommunicationsModes == null)
				{
					ediCommunicationsModes = new EDICommunicationsModeDependentCollection(this);
					ediCommunicationsModes.Load();
					RegisterEditableChildObject(ediCommunicationsModes);
					ediCommunicationsModes.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyConfigGeneralSecurity);
				}

				return ediCommunicationsModes;
			}
		}

		EDICommunicationsModeDependentCollection ediCommunicationsModes;

		#endregion

		#region Carrier Related Organisations/Ports

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCarrierAppointedAgentPortsDependentCollection CarrierAppointedAgentPorts_Stevedore
		{
			get
			{
				if (carrierAppointedAgentPorts_Stevedore == null)
				{
					carrierAppointedAgentPorts_Stevedore = new OrgCarrierAppointedAgentPortsDependentCollection(this, CarrierOrForwarderType.Codes.Stevedore);
					carrierAppointedAgentPorts_Stevedore.Load();
					RegisterEditableChildObject(carrierAppointedAgentPorts_Stevedore);
					carrierAppointedAgentPorts_Stevedore.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCarrierSecurity);
				}

				return carrierAppointedAgentPorts_Stevedore;
			}
		}

		OrgCarrierAppointedAgentPortsDependentCollection carrierAppointedAgentPorts_Stevedore;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCarrierAppointedAgentPortsDependentCollection CarrierAppointedAgentPorts_AirCTO
		{
			get
			{
				if (carrierAppointedAgentPorts_AirCTO == null)
				{
					carrierAppointedAgentPorts_AirCTO = new OrgCarrierAppointedAgentPortsDependentCollection(this, CarrierOrForwarderType.Codes.AirCTO);
					carrierAppointedAgentPorts_AirCTO.Load();
					RegisterEditableChildObject(carrierAppointedAgentPorts_AirCTO);
					carrierAppointedAgentPorts_AirCTO.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCarrierSecurity);
				}

				return carrierAppointedAgentPorts_AirCTO;
			}
		}

		OrgCarrierAppointedAgentPortsDependentCollection carrierAppointedAgentPorts_AirCTO;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCarrierAppointedAgentPortsDependentCollection CarrierAppointedAgentPorts_RailHeadDepot
		{
			get
			{
				if (carrierAppointedAgentPorts_RailHeadDepot == null)
				{
					carrierAppointedAgentPorts_RailHeadDepot = new OrgCarrierAppointedAgentPortsDependentCollection(this, CarrierOrForwarderType.Codes.RailHeadDepot);
					carrierAppointedAgentPorts_RailHeadDepot.Load();
					RegisterEditableChildObject(carrierAppointedAgentPorts_RailHeadDepot);
					carrierAppointedAgentPorts_RailHeadDepot.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCarrierSecurity);
				}

				return carrierAppointedAgentPorts_RailHeadDepot;
			}
		}

		OrgCarrierAppointedAgentPortsDependentCollection carrierAppointedAgentPorts_RailHeadDepot;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCarrierAppointedAgentPortsDependentCollection CarrierAppointedAgentPorts_RoadDepotShed
		{
			get
			{
				if (carrierAppointedAgentPorts_RoadDepotShed == null)
				{
					carrierAppointedAgentPorts_RoadDepotShed = new OrgCarrierAppointedAgentPortsDependentCollection(this, CarrierOrForwarderType.Codes.RoadDepotShed);
					carrierAppointedAgentPorts_RoadDepotShed.Load();
					RegisterEditableChildObject(carrierAppointedAgentPorts_RoadDepotShed);
					carrierAppointedAgentPorts_RoadDepotShed.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCarrierSecurity);
				}

				return carrierAppointedAgentPorts_RoadDepotShed;
			}
		}

		OrgCarrierAppointedAgentPortsDependentCollection carrierAppointedAgentPorts_RoadDepotShed;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCarrierAppointedAgentPortsDependentCollection CarrierAppointedAgentPorts_ContainerYardPark
		{
			get
			{
				if (carrierAppointedAgentPorts_ContainerYardPark == null)
				{
					carrierAppointedAgentPorts_ContainerYardPark = new OrgCarrierAppointedAgentPortsDependentCollection(this, CarrierOrForwarderType.Codes.ContainerYard);
					carrierAppointedAgentPorts_ContainerYardPark.Load();
					RegisterEditableChildObject(carrierAppointedAgentPorts_ContainerYardPark);
					carrierAppointedAgentPorts_ContainerYardPark.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCarrierSecurity);
				}

				return carrierAppointedAgentPorts_ContainerYardPark;
			}
		}

		OrgCarrierAppointedAgentPortsDependentCollection carrierAppointedAgentPorts_ContainerYardPark;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCarrierAppointedAgentPortsDependentCollection CarrierAppointedAgentPorts_Agency
		{
			get
			{
				if (carrierAppointedAgentPorts_Agency == null)
				{
					carrierAppointedAgentPorts_Agency = new OrgCarrierAppointedAgentPortsDependentCollection(this, CarrierOrForwarderType.Codes.Agency);
					carrierAppointedAgentPorts_Agency.Load();
					RegisterEditableChildObject(carrierAppointedAgentPorts_Agency);
					carrierAppointedAgentPorts_Agency.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCarrierSecurity);
				}

				return carrierAppointedAgentPorts_Agency;
			}
		}
		OrgCarrierAppointedAgentPortsDependentCollection carrierAppointedAgentPorts_Agency;

		#endregion

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContainerYardRelatedCarrierAppointedAgentPortsCollection ContainerYardRelatedCarrierAppointedAgentPorts
		{
			get
			{
				if (containerYardRelatedCarrierAppointedAgentPorts == null)
				{
					containerYardRelatedCarrierAppointedAgentPorts = new OrgContainerYardRelatedCarrierAppointedAgentPortsCollection(this, CarrierOrForwarderType.Codes.ContainerYard);
					RegisterEditableChildObject(containerYardRelatedCarrierAppointedAgentPorts);
					containerYardRelatedCarrierAppointedAgentPorts.SetReadOnlyIncludingChildren(true);
				}

				return containerYardRelatedCarrierAppointedAgentPorts;
			}
		}

		OrgContainerYardRelatedCarrierAppointedAgentPortsCollection containerYardRelatedCarrierAppointedAgentPorts;

		#region Forwarder Agent Ports

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgAppointedAgentPortsDependentCollection AppointedAgentPorts
		{
			get
			{
				if (appointedAgentPorts == null)
				{
					appointedAgentPorts = new OrgAppointedAgentPortsDependentCollection(this, OrgAppointedAgentPorts.Forwarder);
					appointedAgentPorts.Load();
					RegisterEditableChildObject(appointedAgentPorts);
					appointedAgentPorts.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyForwarderDetailsSecurity);
				}

				return appointedAgentPorts;
			}
		}

		OrgAppointedAgentPortsDependentCollection appointedAgentPorts;

		#endregion

		#region Gateway Agent Ports

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgAppointedAgentPortsDependentCollection AppointedGatewayAgentPorts
		{
			get
			{
				if (appointedGatewayAgentPorts == null)
				{
					appointedGatewayAgentPorts = new OrgAppointedAgentPortsDependentCollection(this, OrgAppointedAgentPorts.GatewayAgent);
					appointedGatewayAgentPorts.Load();
					RegisterEditableChildObject(appointedGatewayAgentPorts);
					appointedGatewayAgentPorts.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyForwarderDetailsSecurity);
				}

				return appointedGatewayAgentPorts;
			}
		}

		OrgAppointedAgentPortsDependentCollection appointedGatewayAgentPorts;

		#endregion

		#region Agent Relationships

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgAgentRelationshipCollection AgentRelationships
		{
			get
			{
				if (agentRelationships == null)
				{
					ZQuery filter = new ZQuery(OrgAgentRelationshipSchema.O3_OH_SendingAgent, PK);
					filter.AddToFilter(JoinCondition.Or, OrgAgentRelationshipSchema.O3_OH_ReceivingAgent, SQLComparisonOperator.Equal, PK);
					var localAgentRelationships = new OrgAgentRelationshipCollection(Factory, filter);
					localAgentRelationships.Load();
					agentRelationships = localAgentRelationships;
					agentRelationships.SetOrganisationReadOnly(this);
					RegisterEditableChildObject(agentRelationships);
					agentRelationships.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyForwarderProfitShareSecurity);
				}

				return agentRelationships;
			}
		}

		OrgAgentRelationshipCollection agentRelationships;

		#endregion

		#region Landed Costing Preferences

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgLandedCostingPrefsCollection LandedCostingPreferences
		{
			get
			{
				if (landedCostingPreferences == null)
				{
					landedCostingPreferences = new OrgLandedCostingPrefsCollection(this);
					landedCostingPreferences.Load();
					RegisterEditableChildObject(landedCostingPreferences);
					landedCostingPreferences.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyConsigneeLandedCostingSecurity);
				}

				return landedCostingPreferences;
			}
		}

		OrgLandedCostingPrefsCollection landedCostingPreferences;

		#endregion

		#region Staff Assignments

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public virtual OrgStaffAssignmentsCollection StaffAssignments
		{
			get
			{
				if (staffAssignments == null)
				{
					staffAssignments = GetStaffAssignments();
					staffAssignments.Load();
					RegisterEditableChildObject(staffAssignments);
					staffAssignments.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsStaffAssignmentsSecurity && !SecurityProvider.HasModifyDetailsStaffAssignmentsAnyRoleSecurity);
				}
				return staffAssignments;
			}
		}
		OrgStaffAssignmentsCollection staffAssignments;

		public OrgStaffAssignmentsCollection StaffAssignmentsNotCompanySpecific
		{
			get
			{
				var result = GetStaffAssignments();
				result.CompanySpecific = false;
				return result;
			}
		}

		protected virtual OrgStaffAssignmentsCollection GetStaffAssignments(GlbCompany company = null)
		{
			return new OrgStaffAssignmentsCollection(this, company);
		}

		public OrgStaffAssignmentsCollection GetStaffAssignmentsForGlbCompany(GlbCompany company)
		{
			var result = GetStaffAssignments(company);
			result.Load();
			return result;
		}

		#endregion

		#region Security

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgSecurityCollection SecurityRights
		{
			get
			{
				if (securityRights == null)
				{
					securityRights = new OrgSecurityCollection(this);
					securityRights.Load();
					RegisterEditableChildObject(securityRights);
					securityRights.SetReadOnlyIncludingChildren(!(IsInDatabase ? SecurityProvider.HasModifyDetailsWebSecurity : SecurityProvider.HasNewDetailsWebSecurity));
					if (shouldSetDefaultOrgSecurities)
					{
						SetDefaultOrgSecurities();
					}
				}

				return securityRights;
			}
		}

		OrgSecurityCollection securityRights;

		public void RefreshSecurityRights()
		{
			if (securityRights != null)
			{
				securityRights.Refresh();
			}
		}

		public OrgSecurityCollectionView SecurityRightsView
		{
			get
			{
				if (securityRightsView == null)
				{
					securityRightsView = new OrgSecurityCollectionView(SecurityRights);
				}
				return securityRightsView;
			}
		}
		OrgSecurityCollectionView securityRightsView;

		#region Neo

		public virtual bool IsWebSecuritySyncToNeoSupported => true;

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Procedure in db")]
		public void SyncWebSecurityToNeoGroup()
		{
			using (var cmd = Db.Connection.Command("EXEC SyncOrgWebSecurityToNeoGroup @OrgPK, @UserCode"))
			{
				cmd.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, PK.ToGuid());
				cmd.AddParameter("@UserCode", SqlDbType.VarChar, 3, Env.CurrentUser.Initials);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#endregion

		#region Sales Opportunities

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgOpportunityDependentCollection SalesOpportunities
		{
			get
			{
				if (salesOpportunities == null)
				{
					salesOpportunities = new OrgOpportunityDependentCollection(this);
					salesOpportunities.Load();
					RegisterEditableChildObject(salesOpportunities);
					salesOpportunities.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifySalesOpportunityManagementSecurity);
				}

				return salesOpportunities;
			}
		}

		OrgOpportunityDependentCollection salesOpportunities;

		public OrgOpportunity MostRecentSalesOpportunity
		{
			get
			{
				if (SalesOpportunities.Count == 0)
				{
					return null;
				}
				return SalesOpportunities.Cast<OrgOpportunity>().Aggregate((x, y) => (x.P8_OpportunityID > y.P8_OpportunityID) ? x : y);
			}
		}

		#endregion

		#region Organisation Names

		public IEnumerable<OrganisationName> OrganisationNamesExceptBrands
		{
			get
			{
				var result = new List<OrganisationName>();
				result.Add(new OrganisationName(OH_FullName, OH_Language)); // Add Org's Main Name

				foreach (OrgAddress addr in AddressesNoAutoCreate)
				{
					// Add company name overrides
					if (!addr.OA_CompanyNameOverride.IsEmpty && addr.OA_IsActive)
					{
						result.Add(new OrganisationName(addr.OA_CompanyNameOverride, addr.OA_Language) { OrgAddressPK = addr.PK });
					}
				}

				if (OriginalCollections.OrganisationNamesExceptBrands == null)
				{
					OriginalCollections.OrganisationNamesExceptBrands = result;
				}

				return result;
			}
		}

		public IEnumerable<OrganisationName> OrganisationNamesFromBrands
		{
			get
			{
				var result = new List<OrganisationName>();
				foreach (OrgBrandOrRelatedName brandName in BrandsOrRelatedNames)
				{
					result.Add(new OrganisationName(brandName.P1_RelatedName, OH_Language));
				}

				if (OriginalCollections.OrganisationNamesFromBrands == null)
				{
					OriginalCollections.OrganisationNamesFromBrands = result;
				}

				return result;
			}
		}

		#endregion

		#region DefermentAccountNumberCollection

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCusAccountCollection DefermentAccountNumberCollection
		{
			get
			{
				if (defermentAccountNumberCollection == null)
				{
					defermentAccountNumberCollection = new OrgCusAccountCollection(this, Constants.CountryCodes.Germany);
					defermentAccountNumberCollection.Load();
					RegisterEditableChildObject(defermentAccountNumberCollection);
				}

				return defermentAccountNumberCollection;
			}
		}
		OrgCusAccountCollection defermentAccountNumberCollection;

		#endregion

		#region DeltaAgreementNumberCollection

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCusAccountCollection DeltaAgreementNumberCollection
		{
			get
			{
				if (deltaAgreementNumberCollection == null)
				{
					deltaAgreementNumberCollection = new OrgCusAccountCollection(this, Constants.CountryCodes.France);
					deltaAgreementNumberCollection.Load();
					RegisterEditableChildObject(deltaAgreementNumberCollection);
				}

				return deltaAgreementNumberCollection;
			}
		}
		OrgCusAccountCollection deltaAgreementNumberCollection;

		#endregion

		#region CusBondDetailCollection

		[ChildEditable(true)]
		public CusBondDetailCollection CusBondDetails
		{
			get
			{
				if (cusBondDetails == null)
				{
					cusBondDetails = new CusBondDetailCollection(this);
					cusBondDetails.Load();
					RegisterEditableChildObject(cusBondDetails);
				}
				return cusBondDetails;
			}
		}

		CusBondDetailCollection cusBondDetails;

		#endregion

		#region Brands Or Related Names

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		[UniversalCopyCollectionEntity(OrgBrandOrRelatedNameSchema.Constants.TableName, OrgBrandOrRelatedNameSchema.Constants.P1_OH)]
		public OrgBrandOrRelatedNameCollection BrandsOrRelatedNames
		{
			get
			{
				if (brandsOrRelatedNames == null)
				{
					brandsOrRelatedNames = new OrgBrandOrRelatedNameCollection(this);
					brandsOrRelatedNames.Load();
					RegisterEditableChildObject(brandsOrRelatedNames);
					brandsOrRelatedNames.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyConfigBrandsAndCompanyNamesSecurity);

					if (OriginalCollections.OrganisationNamesFromBrands == null && !IsDeleted)
					{
						OriginalCollections.OrganisationNamesFromBrands = OrganisationNamesFromBrands;
					}
				}

				return brandsOrRelatedNames;
			}
		}

		OrgBrandOrRelatedNameCollection brandsOrRelatedNames;

		#endregion

		#region Registration Number

		OrgRegistrationNumber primaryRegistrationNumber;

		public OrgRegistrationNumber PrimaryRegistrationNumber
		{
			get
			{
				if (primaryRegistrationNumber == null)
				{
					primaryRegistrationNumber = new OrgRegistrationNumber(this);
					RegisterEditableChildObject(primaryRegistrationNumber);
				}

				return primaryRegistrationNumber;
			}
		}

		#endregion

		#region Invoice Orders

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public AccClientInvoiceOrderCollection InvoiceOrders
		{
			get
			{
				if (invoiceOrders == null)
				{
					invoiceOrders = new AccClientInvoiceOrderCollection(Factory, this);
					RegisterEditableChildObject(invoiceOrders);
					invoiceOrders.SetCountedReadOnlyIncludingChildren(!SecurityProvider.HasModifyReceivablesSecurity);
					invoiceOrders.MarkAsNeedingValidation();
					((IBindingList)invoiceOrders).ListChanged += new ListChangedEventHandler(OrgHeader_ListChanged);
				}

				return invoiceOrders;
			}
		}

		AccClientInvoiceOrderCollection invoiceOrders;

		void OrgHeader_ListChanged(object sender, ListChangedEventArgs e)
		{
			CheckForInvoiceOrdersDuplicates();
		}

		bool isCheckingForInvoiceOrdersDuplicates;

		void CheckForInvoiceOrdersDuplicates()
		{
			if (!isCheckingForInvoiceOrdersDuplicates)
			{
				isCheckingForInvoiceOrdersDuplicates = true;
				for (int i = 0; i < InvoiceOrders.Count; i++)
				{
					InvoiceOrders[i].RemoveRowError(Res.GetString("53088c86-61ac-4405-a138-b41c1223e78d", "Duplicate entries are not allowed."));
					InvoiceOrders[i].RemoveRowError(Res.GetString("162cf1be-55ec-4a45-9a4a-a61d5311d230", "Duplicate values of Print Order are not allowed."));
				}

				bool[] rowErrors = new bool[InvoiceOrders.Count];
				bool hasError;
				string errorMessage = string.Empty;

				for (int i = 0; i < InvoiceOrders.Count; i++)
				{
					for (int j = i + 1; j < InvoiceOrders.Count; j++)
					{
						hasError = false;
						if (InvoiceOrders[i].AI_AC == InvoiceOrders[j].AI_AC && InvoiceOrders[i].AI_InvoiceType == InvoiceOrders[j].AI_InvoiceType)
						{
							hasError = true;
							errorMessage = Res.GetString("7daabb0d-4085-4fe0-b81f-f959309fb7a0", "Duplicate entries are not allowed.");
						}
						else if (InvoiceOrders[i].AI_PrintOrder == InvoiceOrders[j].AI_PrintOrder)
						{
							hasError = true;
							errorMessage = Res.GetString("a34fbbd4-64cb-4738-988f-17db8aff4495", "Duplicate values of Print Order are not allowed.");
						}

						if (hasError)
						{
							if (!rowErrors[i] && !InvoiceOrders[i].IsValidationSuspended)
							{
								InvoiceOrders[i].AddRowError(errorMessage);
								rowErrors[i] = true;
							}

							if (!rowErrors[j] && !InvoiceOrders[j].IsValidationSuspended)
							{
								InvoiceOrders[j].AddRowError(errorMessage);
								rowErrors[j] = true;
							}
						}
					}
				}

				isCheckingForInvoiceOrdersDuplicates = false;
			}
		}

		#endregion

		#region Rating Documents Charge Order

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public RatingDocumentsChargeOrderCollection RatingDocumentsChargeOrders
		{
			get
			{
				if (ratingDocumentsChargeOrders == null)
				{
					ratingDocumentsChargeOrders = new RatingDocumentsChargeOrderCollection(Factory, this);
					((IBindingList)ratingDocumentsChargeOrders).ListChanged += new ListChangedEventHandler(TariffsAndRates_ListChanged);
					RegisterEditableChildObject(ratingDocumentsChargeOrders);
				}

				return ratingDocumentsChargeOrders;
			}
		}

		RatingDocumentsChargeOrderCollection ratingDocumentsChargeOrders;

		void TariffsAndRates_ListChanged(object sender, ListChangedEventArgs e)
		{
			CheckForRatingDocumentsOrdersDuplicates();
		}

		bool isCheckingForRatingDocumentsOrdersDuplicates;

		void CheckForRatingDocumentsOrdersDuplicates()
		{
			if (!isCheckingForRatingDocumentsOrdersDuplicates)
			{
				isCheckingForRatingDocumentsOrdersDuplicates = true;
				for (int i = 0; i < RatingDocumentsChargeOrders.Count; i++)
				{
					RatingDocumentsChargeOrders[i].RemoveRowError(Res.GetString("B59D9FBB-B0AA-4694-A2AD-41B001E7ABCD", "Duplicate entries are not allowed."));
					RatingDocumentsChargeOrders[i].RemoveRowError(Res.GetString("37E07C1B-47C8-4E93-8450-981B9FCDC77C", "Duplicate values of Print Order are not allowed."));
				}

				bool[] rowErrors = new bool[RatingDocumentsChargeOrders.Count];
				bool hasError;
				string errorMessage = string.Empty;

				for (int i = 0; i < RatingDocumentsChargeOrders.Count; i++)
				{
					for (int j = i + 1; j < RatingDocumentsChargeOrders.Count; j++)
					{
						hasError = false;
						if (RatingDocumentsChargeOrders[i].RCO_AC_ChargeCode == RatingDocumentsChargeOrders[j].RCO_AC_ChargeCode && RatingDocumentsChargeOrders[i].RCO_DocumentType == RatingDocumentsChargeOrders[j].RCO_DocumentType)
						{
							hasError = true;
							errorMessage = Res.GetString("23D696BF-5989-47AB-A86D-A7CD7FE4D08D", "Duplicate entries are not allowed.");
						}
						else if (RatingDocumentsChargeOrders[i].RCO_PrintOrder == RatingDocumentsChargeOrders[j].RCO_PrintOrder)
						{
							hasError = true;
							errorMessage = Res.GetString("56F98763-D915-4E76-B06C-CAAFED078CBD", "Duplicate values of Print Order are not allowed.");
						}

						if (hasError)
						{
							if (!rowErrors[i] && !RatingDocumentsChargeOrders[i].IsValidationSuspended)
							{
								RatingDocumentsChargeOrders[i].AddRowError(errorMessage);
								rowErrors[i] = true;
							}

							if (!rowErrors[j] && !RatingDocumentsChargeOrders[j].IsValidationSuspended)
							{
								RatingDocumentsChargeOrders[j].AddRowError(errorMessage);
								rowErrors[j] = true;
							}
						}
					}
				}

				isCheckingForRatingDocumentsOrdersDuplicates = false;
			}
		}

		#endregion

		#region Screening Log Collection

		[ChildEditable]
		[ChildEditableTestExclude]
		public IStmEntityScreeningLogCollection ScreeningLogCollection
		{
			get
			{
				if (screeningLogCollection == null)
				{
					screeningLogCollection = (IStmEntityScreeningLogCollection)Activator.CreateInstance(ObjectFactory.GetType<IStmEntityScreeningLogCollection>(), this);
					RegisterEditableChildObject((IBusiness)screeningLogCollection);
				}
				return screeningLogCollection;
			}
		}
		IStmEntityScreeningLogCollection screeningLogCollection;

		#endregion

		#region Number Fountains

		[ChildEditable()]
		public IViewStmNumsCollection<ViewStmNums> Fountains => OrgFountains;

		[ChildEditable()]
		public OrganisationViewStmNumsCollection OrgFountains
		{
			get
			{
				if (orgFountains == null)
				{
					orgFountains = new OrganisationViewStmNumsCollection(this);
					orgFountains.AdditionalFilter.AddToFilter(ViewStmNumsSchema.SN_ID, SQLComparisonOperator.NotEqual, 0);
					orgFountains.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(orgFountains);
				}

				return orgFountains;
			}
		}
		OrganisationViewStmNumsCollection orgFountains;

		#endregion

		#region Matching Number

		[ChildEditable()]
		public StmNumberRangeMatchingDetailsCollection NumberRangeMatchingDetails
		{
			get
			{
				if (stmNumberRangeMatchingDetailsCollection == null)
				{
					stmNumberRangeMatchingDetailsCollection = new StmNumberRangeMatchingDetailsCollection(this);
					RegisterEditableChildObject(stmNumberRangeMatchingDetailsCollection);
					stmNumberRangeMatchingDetailsCollection.SetReadOnlyIncludingChildren(!Env.Security.OrgConfigModify.IsAllowed);
				}

				return stmNumberRangeMatchingDetailsCollection;
			}
		}
		StmNumberRangeMatchingDetailsCollection stmNumberRangeMatchingDetailsCollection;

		#endregion

		#region GetMatchingNumberRanges

		public StmNumberRangeMatchingDetail GetMatchingNumberRange(string orgNumberFountainCode, IReadOnlyList<ColumnValueRanker.ColumnValuesPair> rankConditions = null)
		{
			return StmNumberRangeMatchingDetail.GetMatchingNumberRange(this, orgNumberFountainCode, rankConditions);
		}

		#endregion

		#region Subscriptions

		[ChildEditable(true)]
		public IGlbCompanyCampaignSubscriptionForOrganisationCollection Subscriptions => subscriptions ?? (subscriptions = GetOrgSubscriptionCollection());

		IGlbCompanyCampaignSubscriptionForOrganisationCollection subscriptions;

		IGlbCompanyCampaignSubscriptionForOrganisationCollection GetOrgSubscriptionCollection()
		{
			var glbCompanyCampaignSubscriptionsForOrganisation = ObjectFactory.Get<IGlbCompanyCampaignSubscriptionForOrganisationCollection>(nameof(IGlbCompanyCampaignSubscriptionForOrganisationCollection), this);
			RegisterEditableChildObject(glbCompanyCampaignSubscriptionsForOrganisation);
			return glbCompanyCampaignSubscriptionsForOrganisation;
		}

		#endregion

		#region Carrier Account Numbers

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCarrierAccountCollection CarrierAccounts
		{
			get
			{
				if (carrierAccounts == null)
				{
					carrierAccounts = new OrgCarrierAccountCollection(this);
					RegisterEditableChildObject(carrierAccounts);
					carrierAccounts.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCarrierSecurity);
				}
				return carrierAccounts;
			}
		}

		OrgCarrierAccountCollection carrierAccounts;

		#endregion

		#region Carrier Named Accounts

		// This property represents a collection of mapped named accounts under this carrier organisation.
		// 
		// Named accounts are a text-based identifier representing a business/entity (e.g., Nike & Nike, Inc)
		//
		// Organisations in CW1 are the entitys these named accounts are representing (e.g, Nike)
		//
		// Carriers are organisations that specifically provide transportation services. Carriers organisations
		// provide the named accounts and thus why named accounts can be mapped under a carrier.
		//
		// Example:
		//   Organisation Carrier     Named Account
		// 1 Nike         Maersk      "Nike"
		// 2 Nike         Hapag-Lloyd "Nike, Inc."
		// 3 Adidas       Maersk      "Adidas"
		//
		// In this case both "Nike" and "Nike, Inc." are mapped to the same entity "Nike", and if this OrgHeader
		// is representing the Carrier "Maersk" then this collection would only consist of mapped named accounts
		// 1 and 3.
		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCarrierNamedAccountCollection CarrierNamedAccounts
		{
			get
			{
				if (carrierNamedAccounts == null)
				{
					var tempCarrierNamedAccounts = new OrgCarrierNamedAccountCollection(this, OrgCarrierNamedAccountSchema.ONA_OH_Carrier);
					tempCarrierNamedAccounts.Load();
					carrierNamedAccounts = tempCarrierNamedAccounts;
					RegisterEditableChildObject(carrierNamedAccounts);
					carrierNamedAccounts.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyCarrierSecurity);
				}
				return carrierNamedAccounts;
			}
		}

		OrgCarrierNamedAccountCollection carrierNamedAccounts;

		#endregion

		#region Mapped Named Accounts

		// This property represents a collection of named accounts mapped to this organisation.
		//
		// Please refer to the comment for "CarrierNamedAccounts" above for more explanation.
		//
		// Example:
		//   Organisation Carrier     Named Account
		// 1 Nike         Maersk      "Nike"
		// 2 Nike         Hapag-Lloyd "Nike, Inc."
		// 3 Adidas       Maersk      "Adidas"
		//
		// In this case lets say this OrgHeader is representing organisation "Nike", then
		// this collection would return both mapped named accounts 1 and 2 but not 3.
		[ActionFieldFollow(true)]
		public OrgCarrierNamedAccountCollection MappedNamedAccounts
		{
			get
			{
				if (mappedNamedAccounts == null)
				{
					var tempMappedNamedAccounts = new OrgCarrierNamedAccountCollection(this, OrgCarrierNamedAccountSchema.ONA_OH_Organization);
					tempMappedNamedAccounts.Load();
					mappedNamedAccounts = tempMappedNamedAccounts;
					mappedNamedAccounts.SetReadOnlyIncludingChildren(true);
				}
				return mappedNamedAccounts;
			}
		}

		OrgCarrierNamedAccountCollection mappedNamedAccounts;

		#endregion

		#region Warehouse Client Account Associations

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgWhsClientAccountAssociationCollection OrgWhsClientAccountAssociations
		{
			get
			{
				if (whsCarrierAccountNumbers == null)
				{
					whsCarrierAccountNumbers = new OrgWhsClientAccountAssociationCollection(this);
					RegisterEditableChildObject(whsCarrierAccountNumbers);
					whsCarrierAccountNumbers.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyWarehouseSecurity);
				}
				return whsCarrierAccountNumbers;
			}
		}

		OrgWhsClientAccountAssociationCollection whsCarrierAccountNumbers;

		#endregion

		#region OrgAirlineMAWBStockManagementCollection

		[ChildEditable(true)]
		public OrgAirlineMAWBStockManagementCollection OrgAirlineMAWBStockManagementCollection
		{
			get
			{
				if (orgAirlineMAWBStockManagementCollection == null)
				{
					orgAirlineMAWBStockManagementCollection = new OrgAirlineMAWBStockManagementCollection(Factory, this);

					RegisterEditableChildObject(orgAirlineMAWBStockManagementCollection);
					SetOrgAirlineMAWBStockManagementCollectionEditableState();
				}

				return orgAirlineMAWBStockManagementCollection;
			}
		}

		OrgAirlineMAWBStockManagementCollection orgAirlineMAWBStockManagementCollection;

		void SetOrgAirlineMAWBStockManagementCollectionEditableState()
		{
			var editable = OH_IsAirLine || OH_IsAirWholesaler;
			if (!editable)
			{
				OrgAirlineMAWBStockManagementCollection.DeleteAll();
			}

			var hasPermission = SecurityProvider.HasModifyCarrierSecurityAir;

			OrgAirlineMAWBStockManagementCollection.SetReadOnlyIncludingChildren(!editable || !hasPermission);
		}

		#endregion OrgAirlineMAWBStockManagementCollection

		#region CYDYardStorageFreeDays

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public CYDYardStorageFreeDaysCollection YardStorageFreeDays
		{
			get
			{
				if (yardStorageFreeDays == null)
				{
					yardStorageFreeDays = new CYDYardStorageFreeDaysCollection(this);
					RegisterEditableChildObject(yardStorageFreeDays);
					yardStorageFreeDays.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyWarehouseSecurity);
				}
				return yardStorageFreeDays;
			}
		}

		CYDYardStorageFreeDaysCollection yardStorageFreeDays;

		#endregion CYDYardStorageFreeDays

		#endregion

		#region Recalculate PatternMatchingTables

		PatternMatchingRecalculator<OrgHeader> patternMatchingRecalculator;
		public PatternMatchingRecalculator<OrgHeader> PatternMatchingRecalculator
		{
			get
			{
				return patternMatchingRecalculator ?? (patternMatchingRecalculator = new PatternMatchingRecalculator<OrgHeader>(this));
			}
		}

		public async Task RegeneratePatternTables()
		{
			await PatternMatchingRecalculator.RegenerateAsync(ObjectFactory.Get<IMasterDataProvider>().GetDeduplicationOrgHeader(this));
		}

		#endregion

		#region Properties

		public bool IsMiscellaneous
		{
			get { return PK == OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation; }
		}

		public string NameAndCode => FormattableString.Invariant($"{OH_FullName} ({OH_Code})");

		#region OH_IsActive

		bool? startValueOH_IsActive;

		internal bool HasDeactivatedEntity { get; set; }

		[ReadOnly(true)]
		public override ZBool OH_IsActive
		{
			get
			{
				return base.OH_IsActive;
			}
			set
			{
				if (startValueOH_IsActive == null)
				{
					startValueOH_IsActive = base.OH_IsActive;
				}

				base.OH_IsActive = value;
				PatternMatchRequiresRegen = true;
				PatternMatchRequiresFullRegen = true;
			}
		}

		#endregion

		#region OH_IsTempAccount

		public override ZBool OH_IsTempAccount
		{
			get
			{
				return base.OH_IsTempAccount;
			}
			set
			{
				if (OH_IsTempAccount != value)
				{
					ResetFlagsForSecurity(value);
				}

				base.OH_IsTempAccount = value;
				Addresses.MarkAsNeedingValidation();
				CompanyData.MarkAsNeedingValidation();
			}
		}

		void ResetFlagsForSecurity(ZBool newValue)
		{
			if (!newValue)
			{
				if (IsInDatabase)
				{
					CheckAndResetFlagsForModify();
				}
				else
				{
					CheckAndResetFlagsForNew();
				}
			}
			else
			{
				if (IsInDatabase)
				{
					CheckAndResetFlagsForModifyTemp();
				}
				else
				{
					CheckAndResetFlagsForNewTemp();
				}
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CheckAndResetFlagsForModify()
		{
			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagAR && OH_IsDebtor != (ZBool)OH_IsDebtorInfo.OriginalValue)
			{
				OH_IsDebtor = (ZBool)OH_IsDebtorInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagAP && OH_IsCreditor != (ZBool)OH_IsCreditorInfo.OriginalValue)
			{
				OH_IsCreditor = (ZBool)OH_IsCreditorInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagSP && OH_IsConsignor != (ZBool)OH_IsConsignorInfo.OriginalValue)
			{
				OH_IsConsignor = (ZBool)OH_IsConsignorInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagCon && OH_IsConsignee != (ZBool)OH_IsConsigneeInfo.OriginalValue)
			{
				OH_IsConsignee = (ZBool)OH_IsConsigneeInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagTC && OH_IsTransportClient != (ZBool)OH_IsTransportClientInfo.OriginalValue)
			{
				OH_IsTransportClient = (ZBool)OH_IsTransportClientInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagWH && OH_IsWarehouseClient != (ZBool)OH_IsWarehouseClientInfo.OriginalValue)
			{
				OH_IsWarehouseClient = (ZBool)OH_IsWarehouseClientInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagCrr && OH_IsShippingProvider != (ZBool)OH_IsShippingProviderInfo.OriginalValue)
			{
				OH_IsShippingProvider = (ZBool)OH_IsShippingProviderInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagFA && OH_IsForwarder != (ZBool)OH_IsForwarderInfo.OriginalValue)
			{
				OH_IsForwarder = (ZBool)OH_IsForwarderInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagBR && OH_IsBroker != (ZBool)OH_IsBrokerInfo.OriginalValue)
			{
				OH_IsBroker = (ZBool)OH_IsBrokerInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagSV && OH_IsMiscFreightServices != (ZBool)OH_IsMiscFreightServicesInfo.OriginalValue)
			{
				OH_IsMiscFreightServices = (ZBool)OH_IsMiscFreightServicesInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagCM && OH_IsCompetitor != (ZBool)OH_IsCompetitorInfo.OriginalValue)
			{
				OH_IsCompetitor = (ZBool)OH_IsCompetitorInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagSal && OH_IsSalesLead != (ZBool)OH_IsSalesLeadInfo.OriginalValue)
			{
				OH_IsSalesLead = (ZBool)OH_IsSalesLeadInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagCtrlAgent && OH_IsControllingAgent != (ZBool)OH_IsControllingAgentInfo.OriginalValue)
			{
				OH_IsControllingAgent = (ZBool)OH_IsControllingAgentInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomer && OH_IsControllingCustomer != (ZBool)OH_IsControllingCustomerInfo.OriginalValue)
			{
				OH_IsControllingCustomer = (ZBool)OH_IsControllingCustomerInfo.OriginalValue;
			}
		}

		void CheckAndResetFlagsForModifyTemp()
		{
			if (!SecurityProvider.HasModifyDetailsOrgTypeTempARFlag && OH_IsDebtor != (ZBool)OH_IsDebtorInfo.OriginalValue)
			{
				OH_IsDebtor = (ZBool)OH_IsDebtorInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempAPFlag && OH_IsCreditor != (ZBool)OH_IsCreditorInfo.OriginalValue)
			{
				OH_IsCreditor = (ZBool)OH_IsCreditorInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempSPFlag && OH_IsConsignor != (ZBool)OH_IsConsignorInfo.OriginalValue)
			{
				OH_IsConsignor = (ZBool)OH_IsConsignorInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempConFlag && OH_IsConsignee != (ZBool)OH_IsConsigneeInfo.OriginalValue)
			{
				OH_IsConsignee = (ZBool)OH_IsConsigneeInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempTCFlag && OH_IsTransportClient != (ZBool)OH_IsTransportClientInfo.OriginalValue)
			{
				OH_IsTransportClient = (ZBool)OH_IsTransportClientInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempWHFlag && OH_IsWarehouseClient != (ZBool)OH_IsWarehouseClientInfo.OriginalValue)
			{
				OH_IsWarehouseClient = (ZBool)OH_IsWarehouseClientInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempFAFlag && OH_IsForwarder != (ZBool)OH_IsForwarderInfo.OriginalValue)
			{
				OH_IsForwarder = (ZBool)OH_IsForwarderInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempBRFlag && OH_IsBroker != (ZBool)OH_IsBrokerInfo.OriginalValue)
			{
				OH_IsBroker = (ZBool)OH_IsBrokerInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempSVFlag && OH_IsMiscFreightServices != (ZBool)OH_IsMiscFreightServicesInfo.OriginalValue)
			{
				OH_IsMiscFreightServices = (ZBool)OH_IsMiscFreightServicesInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempCMFlag && OH_IsCompetitor != (ZBool)OH_IsCompetitorInfo.OriginalValue)
			{
				OH_IsCompetitor = (ZBool)OH_IsCompetitorInfo.OriginalValue;
			}

			if (!SecurityProvider.HasModifyDetailsOrgTypeTempSalFlag && OH_IsSalesLead != (ZBool)OH_IsSalesLeadInfo.OriginalValue)
			{
				OH_IsSalesLead = (ZBool)OH_IsSalesLeadInfo.OriginalValue;
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CheckAndResetFlagsForNew()
		{
			if (!SecurityProvider.HasNewDetailsOrgTypeFlagAR && OH_IsDebtor)
			{
				OH_IsDebtor = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagAP && OH_IsCreditor)
			{
				OH_IsCreditor = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagSP && OH_IsConsignor)
			{
				OH_IsConsignor = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagCon && OH_IsConsignee)
			{
				OH_IsConsignee = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagTC && OH_IsTransportClient)
			{
				OH_IsTransportClient = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagWH && OH_IsWarehouseClient)
			{
				OH_IsWarehouseClient = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagCrr && OH_IsShippingProvider)
			{
				OH_IsShippingProvider = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagFA && OH_IsForwarder)
			{
				OH_IsForwarder = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagBR && OH_IsBroker)
			{
				OH_IsBroker = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagSV && OH_IsMiscFreightServices)
			{
				OH_IsMiscFreightServices = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagCM && OH_IsCompetitor)
			{
				OH_IsCompetitor = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagSal && OH_IsSalesLead)
			{
				OH_IsSalesLead = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagCtrlAgent && OH_IsControllingAgent)
			{
				OH_IsControllingAgent = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeFlagCtrlCustomer && OH_IsControllingCustomer)
			{
				OH_IsControllingCustomer = ZBool.False;
			}
		}

		void CheckAndResetFlagsForNewTemp()
		{
			if (!SecurityProvider.HasNewDetailsOrgTypeTempARFlag && OH_IsDebtor)
			{
				OH_IsDebtor = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempAPFlag && OH_IsCreditor)
			{
				OH_IsCreditor = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempSPFlag && OH_IsConsignor)
			{
				OH_IsConsignor = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempConFlag && OH_IsConsignee)
			{
				OH_IsConsignee = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempTCFlag && OH_IsTransportClient)
			{
				OH_IsTransportClient = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempWHFlag && OH_IsWarehouseClient)
			{
				OH_IsWarehouseClient = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempFAFlag && OH_IsForwarder)
			{
				OH_IsForwarder = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempBRFlag && OH_IsBroker)
			{
				OH_IsBroker = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempSVFlag && OH_IsMiscFreightServices)
			{
				OH_IsMiscFreightServices = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempCMFlag && OH_IsCompetitor)
			{
				OH_IsCompetitor = ZBool.False;
			}

			if (!SecurityProvider.HasNewDetailsOrgTypeTempSalFlag && OH_IsSalesLead)
			{
				OH_IsSalesLead = ZBool.False;
			}
		}

		#endregion

		#region OH_IsAirCTO

		public override ZBool OH_IsAirCTO
		{
			get
			{
				return base.OH_IsAirCTO;
			}
			set
			{
				base.OH_IsAirCTO = value;
				Addresses.MarkAsNeedingValidation();
				CompanyData.MarkAsNeedingValidation();
				CheckOrgRefFacility();
			}
		}

		#endregion

		#region OH_IsSeaCTO

		public override ZBool OH_IsSeaCTO
		{
			get
			{
				return base.OH_IsSeaCTO;
			}
			set
			{
				base.OH_IsSeaCTO = value;
				Addresses.MarkAsNeedingValidation();
				CompanyData.MarkAsNeedingValidation();
				CheckOrgRefFacility();
			}
		}

		#endregion

		#region OH_IsPackDepot

		public override ZBool OH_IsPackDepot
		{
			get
			{
				return base.OH_IsPackDepot;
			}
			set
			{
				base.OH_IsPackDepot = value;
				Addresses.MarkAsNeedingValidation();
				CompanyData.MarkAsNeedingValidation();
				CheckOrgRefFacility();
			}
		}

		#endregion

		#region OH_IsBroker

		public override ZBool OH_IsBroker
		{
			get
			{
				return base.OH_IsBroker;
			}
			set
			{
				var oldGeneratedCode = GetOldGeneratedCode(true);
				base.OH_IsBroker = value;
				RegenerateCode(oldGeneratedCode);
				Addresses.MarkAsNeedingValidation();
				CompanyData.MarkAsNeedingValidation();
			}
		}

		protected bool OH_IsBroker_ReadOnly
		{
			get
			{
				bool readOnly = false;
				if (OH_IsTempAccount)
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeTempBRFlag.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeTempBRFlag;
				}
				else
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagBR.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagBR;
				}
				return readOnly;
			}
		}

		#endregion

		#region OH_IsTransportClient

		public override ZBool OH_IsTransportClient
		{
			get
			{
				return base.OH_IsTransportClient;
			}
			set
			{
				var oldGeneratedCode = GetOldGeneratedCode(true);
				base.OH_IsTransportClient = value;
				RegenerateCode(oldGeneratedCode);
				Addresses.MarkAsNeedingValidation();
				CompanyData.MarkAsNeedingValidation();
			}
		}

		protected bool OH_IsTransportClient_ReadOnly
		{
			get
			{
				bool readOnly = false;
				if (OH_IsTempAccount)
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeTemlTCFlag.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeTempTCFlag;
				}
				else
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagTC.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagTC;
				}
				return readOnly;
			}
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region OH_RL_NKClosestPort

		bool isResolvingClosestPort;
		[List("Lookups.ClosestPorts")]
		public override sealed ZString OH_RL_NKClosestPort
		{
			get
			{
				ZString result = ZString.Empty;
				// This is to stop stack overflow between this method and OA_RL_NKRelatedPortCode (which calls back to this method)
				if (OH_IsGlobalAccount)
				{
					if (!isResolvingClosestPort)
					{
						isResolvingClosestPort = true;

						try
						{
							OrgAddress address = MainAddress;
							if (address != null)
							{
								result = address.OA_RL_NKRelatedPortCode;
							}

							if (result.IsEmpty)
							{
								result = base.OH_RL_NKClosestPort;
							}
						}
						finally
						{
							isResolvingClosestPort = false;
						}
					}
				}
				else
				{
					result = base.OH_RL_NKClosestPort;
				}
				return result;
			}
			set
			{
				if (!isSettingClosestPort)
				{
					isSettingClosestPort = true;
					try
					{
						var oldGeneratedCode = GetOldGeneratedCode(false);
						RefUNLOCO previousUnloco = UNLOCO;

						base.OH_RL_NKClosestPort = value;
						MainAddress.OA_RL_NKRelatedPortCode = value;
						Validation.ValidateOH_RL_NKClosestPort();

						var newUnloco = UNLOCO;
						if (string.IsNullOrEmpty(OH_Code) || newUnloco != null)
						{
							RegenerateCode(oldGeneratedCode, OrgHeaderSchema.Constants.OH_RL_NKClosestPort);
						}
						if (newUnloco == null)
						{
							codeRegenerator = () => RegenerateCode(oldGeneratedCode, OrgHeaderSchema.Constants.OH_RL_NKClosestPort);
						}

						if (!Env.Registry.EnableAddressValidationWebService)
						{
							SetMainAddressState();
						}

						SetBranch();

						SetMiscServDefaults(previousUnloco);
						MarkMiscServNeedValidationIfRequired(previousUnloco, newUnloco);

						SetCountryHasStateListForAddresses();
						Contacts.MarkAsNeedingValidation();
						Addresses.MarkAsNeedingValidation();
						AddressHelper.UpdateOtherAddressesUNLOCOFromMainAddress(value);

						PatternMatchRequiresRegen = true;
					}
					finally
					{
						isSettingClosestPort = false;
					}
				}
			}
		}
		bool isSettingClosestPort;

		internal void RegenerateCodeIfRequired()
		{
			if (codeRegenerator != null)
			{
				codeRegenerator();
				codeRegenerator = null;
			}
		}
		Action codeRegenerator;

		void MarkMiscServNeedValidationIfRequired(RefUNLOCO previousUnloco, RefUNLOCO newUnloco)
		{
			if (!IsValidationSuspended
				&& OH_IsAirLine
				&& OH_IsShippingProvider
				&& previousUnloco != null
				&& newUnloco != null
				&& MiscServ != null
				&& previousUnloco.RL_RN_NKCountryCode != newUnloco.RL_RN_NKCountryCode)
			{
				MiscServ.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region OH_Code

		[NotDefaultingPropertyValue("UNMATCHED")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString OH_Code
		{
			get { return base.OH_Code; }
			set
			{
				base.OH_Code = value;
				RegeneratingCodeAfterPossibleUserEdit = false;
				InvalidCodeGenAlgorithm = false;
				isCodeWithUniqueNumberGenerationPending = false;
			}
		}

		protected bool OH_Code_ReadOnly
		{
			get { return !Env.Registry.CanUserEditOrganisationCode; }
		}

		OrgWebURLDependentCollection orgWebURLs;
		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgWebURLDependentCollection OrgWebURLs
		{
			get
			{
				if (orgWebURLs == null)
				{
					orgWebURLs = new OrgWebURLDependentCollection(this);
					orgWebURLs.Load();
					RegisterEditableChildObject(orgWebURLs);
					orgWebURLs.SetReadOnlyIncludingChildren(!SecurityProvider.HasModifyDetailsPhFaxWebDetailsSecurity);
				}

				return orgWebURLs;
			}
		}

		#endregion

		#region OH_Category

		[List("Lookups.Categories")]
		public override ZString OH_Category
		{
			get
			{
				return base.OH_Category;
			}
			set
			{
				base.OH_Category = value;
			}
		}

		#endregion

		#region OH_FullName
		/// <summary>
		/// Use this property if only we want to show the full company name.
		/// Otherwise use 'OH_FullNameTruncated' as in most of places we only want to show first 50 characters of a company name
		/// </summary>
		public override ZString OH_FullName
		{
			get
			{
				return base.OH_FullName;
			}
			set
			{
				if (base.OH_FullName != value)
				{
					var oldGeneratedCode = GetOldGeneratedCode(false);
					base.OH_FullName = value.TrimStart();
					RegenerateCode(oldGeneratedCode, OrgHeaderSchema.Constants.OH_FullName);
					PatternMatchRequiresRegen = true;
					FindDuplicates();
				}
			}
		}

		public bool IsDeduplicationAllowed => !ReadOnly && OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value && Env.Security.OrganisationModify.IsAllowed;

#if DEBUG
		public
#endif
		ISupportDuplicationFinder CurrentDuplicationFinder
		{ get; set; }

		public void FindDuplicates()
		{
			Factory.ThreadSentry.EnsureCurrentThreadIsOwner();

			if (IsDeduplicationAllowed && shouldRunDeduplication)
			{
				var maximumountForAddress = OrganisationsDataRegistry.Instance.MaximumAddressCountForUserDrivenDeduplication.Value;
				var maximumCountForContact = OrganisationsDataRegistry.Instance.MaximumContactCountForUserDrivenDeduplication.Value;
				var contactCount = Contacts.Count;
				var addressCount = Addresses.Count;
				if (contactCount <= maximumCountForContact && addressCount < maximumountForAddress)
				{
					((IDeduplicatable)this).PropagateDeduplicationStarted();
					CurrentDuplicationFinder?.RequestToCancel();
					CurrentDuplicationFinder = ObjectFactory.Get<IMasterDataProvider>().CreateOrgDuplicationFinder(this);
					CurrentDuplicationFinder.FindDuplicates();
				}
				else
				{
					var exclusionManager = new DeduplicationExclusionManager<OrgHeader>();
					exclusionManager.Builder.BuildExclusion(
						new OrgHeader[] { this }.AsQueryable(),
						ResString.GetMultilingualString("2E64C1FE-D121-47FC-9023-185E90F4EAC2", "This Organization has {0} addresses and {1} contacts, The system excludes record with more than {2} addresses/{3} contacts. Consider if this organization needs to be split using management groups.", addressCount, contactCount, maximumountForAddress, maximumCountForContact),
						org => true);
					((IDeduplicatable)this).PropagateDeduplicationEnded(null, null, null, DuplicationStatus.OK, exclusionManager);
				}
			}
		}

		void IDeduplicatable.FindDuplicatesBypassErrorChecking(bool isForAdminPanel)
		{
			FindDuplicates();
		}

		public static SecurityCheckpoint ExportExcelCheckPoint => Env.Security.FindOrCreateExportCheckPoint(Env.Security.Organisation);

		#region IDeduplicationPropagation Members

		public bool IsDuplicateFound { get; private set; }

		public event EventHandler DeduplicationStarted;

		public event EventHandler<IDuplicationEventArgs> DeduplicationActionOccurred;

		public event EventHandler<IDuplicationEventArgs> DeduplicationEnded;

		public event EventHandler<IDuplicationEventArgs> DuplicationDetected;

		void IDeduplicatable.PropagateDeduplicationStarted()
		{
			DeduplicationStarted?.Invoke(this, EventArgs.Empty);
		}

		bool IDeduplicatable.IsDeduplicationStarted { get; set; }

		public void PropagateDeduplicationActionOccurred(DeduplicationAction userAction, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModels, object targetList, bool isExcludingInactiveFromResults = false, bool isExcludingOtherCountriesFromResults = false, bool isShowIgnoredFromResults = true)
		{
			if (!ReadOnly && OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value)
			{
				var eventArgs = ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", this, targetList, results, resultsModels);
				eventArgs.InvokedAction = userAction;
				eventArgs.IsExcludingInactiveFromResults = isExcludingInactiveFromResults;
				eventArgs.IsExcludingOtherCountriesFromResults = isExcludingOtherCountriesFromResults;
				eventArgs.IsShowIgnoredFromResults = isShowIgnoredFromResults;

				DeduplicationActionOccurred?.Invoke(this, eventArgs);
			}
		}

		public void PropagateDeduplicationEnded<TBizo>(IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModel, object targetObjects, DuplicationStatus lastRunStatus, DeduplicationExclusionManager<TBizo> exclusionManager)
			where TBizo : BusinessObject, IDeduplicatable
		{
			var manager = new DeduplicationExclusionManager<OrgHeader>();

			exclusionManager.BuildDisplay();
			manager.ItemsCount = exclusionManager.ItemsCount;
			manager.DisplayInfo = exclusionManager.DisplayInfo;

			DeduplicationEnded?.Invoke(this, ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", this, targetObjects, results, resultsModel, lastRunStatus, manager));
		}

		public void PropagateDeduplication(IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModel, object targetList)
		{
			DuplicationDetected?.Invoke(this, ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", this, targetList, results, resultsModel));
		}

		public void ValidateDuplicationResult(bool isDuplicatesFound)
		{
			IsDuplicateFound = isDuplicatesFound;

			if (!IsDeleted)
			{
				lock (this)
				{
					Validation.ValidateOH_FullName();
				}
			}
		}

		bool shouldRunDeduplication;
		bool IDeduplicatable.ShouldRunDeduplication
		{
			get
			{
				return shouldRunDeduplication;
			}
			set
			{
				shouldRunDeduplication = value;
			}
		}

		public ZString DeduplicationCountryCode => CountryCode;

		bool IDeduplicatable.IsExcludedFromDeduplication
		{
			get
			{
				var dedupOrg = Factory.Load<DeduplicationOrganisation>(PK);
				dedupOrg.Reload();
				return dedupOrg.DOH_Status == DeduplicationHelper.StatusConstants.Excluded;
			}
			set
			{
				ObjectFactory.Get<IMasterDataProvider>().ComputeIsExcludedFromDeduplication(this, value);
			}
		}

		string IDeduplicatable.Info => OH_Code;
		string IDeduplicatable.FullName => OH_FullName;
		bool IDeduplicatable.IsActive => OH_IsActive;
		Type IDeduplicatable.BizoType => typeof(OrgHeader);
		ZString IDeduplicatable.NaturalKey => OH_Code;

		#endregion

		/// <summary>
		/// Use this property to diplay first 50 characters for company name.
		/// In most cases the system is best suited to show 50 characters for company name.
		/// Use the property 'OH_FullName' if only we want to show the full company name.
		/// </summary>
		public ZString OH_FullNameTruncated
		{
			get
			{
				return OH_FullName.SubstringSafe(0, OrgHeader.Schema.OH_FullNameTruncatedLength);
			}
		}

		#endregion

		#region OH_IsNationalAccount

		public override ZBool OH_IsNationalAccount
		{
			get
			{
				return base.OH_IsNationalAccount;
			}
			set
			{
				if (base.OH_IsNationalAccount != value)
				{
					var oldGeneratedCode = GetOldGeneratedCode(false);
					base.OH_IsNationalAccount = value;
					RegenerateCode(oldGeneratedCode, OrgHeaderSchema.Constants.OH_IsNationalAccount);
					if (!IsValidationSuspended)
					{
						Validation.ValidateOH_IsGlobalAccount();
					}
				}
			}
		}

		protected bool OH_IsNationalAccount_ReadOnly
		{
			get
			{
				bool readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyIsNationalOrg.IsAllowed : !SecurityProvider.HasNewDetailsIsNationalOrgSecurity;
				return readOnly;
			}
		}

		#endregion

		#region IsOrgTypeInvolvedInOverseasTransactions

		public bool IsOrgTypeInvolvedInOverseasTransactions
		{
			get
			{
				return OH_IsConsignee ||
					OH_IsConsignor ||
					OH_IsShippingProvider ||
					OH_IsForwarder;
			}
		}

		#endregion

		#region OH_IsDebtor

		public ZBool OH_IsDebtor
		{
			get
			{
				return CompanyData.OB_IsDebtor;
			}
			set
			{
				if (OH_IsTempAccount)
				{
					if ((Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed && !IsInDatabase) || (Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed && IsInDatabase))
					{
						SetIsDebtorValue(value);
						CompanyData.Validation.ValidateOB_OJ_ARDebtorGroup();
					}
					else if (!refreshingBinding)
					{
						refreshingBinding = true;
						SecurityAccessDenied();
						OH_IsDebtorInfo.RefreshBinding();
						refreshingBinding = false;
					}
				}
				else
				{
					SetIsDebtorValue(value);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateOH_IsTempAccount();
				}
			}
		}

		public bool IsDebtorForCompany(GlbCompany company)
		{
			return company != null && IsDebtorForCompany(company.PK);
		}

		public bool IsDebtorForCompany(ZGuid companyPK)
		{
			bool result = false;

			var companyData = OrgCompanyData.Load(Factory, PK, companyPK);
			result = companyData != null && companyData.OB_IsDebtor;

			return result;
		}

		public bool IsCreditorForCompany(ZGuid companyPK)
		{
			bool result = false;

			var companyData = OrgCompanyData.Load(Factory, PK, companyPK);
			result = companyData != null && companyData.OB_IsCreditor;

			return result;
		}

		public bool IsCreditorForCompany(GlbCompany company)
		{
			return company != null && IsCreditorForCompany(company.PK);
		}

		bool refreshingBinding;

		void SetIsDebtorValue(bool isDebtor)
		{
			var oldGeneratedCode = GetOldGeneratedCode(true);
			CompanyData.OB_IsDebtor = isDebtor;
			RegenerateCode(oldGeneratedCode);
			CheckRegisteredChildObjects();
		}

		public ZPropertyInfo OH_IsDebtorInfo
		{
			get { return GetWrappedZPropertyInfo(OHConstants.OH_IsDebtor, x => CompanyData.OB_IsDebtorInfo); }
		}

		#endregion

		#region OH_IsCreditor

		public ZBool OH_IsCreditor
		{
			get
			{
				return CompanyData.OB_IsCreditor;
			}
			set
			{
				if (OH_IsTempAccount)
				{
					if ((Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed && !IsInDatabase) || (Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed && IsInDatabase))
					{
						SetIsCreditorValue(value);
						if (value)
						{
							CompanyData.SetDefaultPayablesAccount();
						}
						CompanyData.Validation.ValidateOB_OG_APCreditorGroup();
					}
					else if (!refreshingBinding)
					{
						refreshingBinding = true;
						SecurityAccessDenied();
						OH_IsCreditorInfo.RefreshBinding();
						refreshingBinding = false;
					}
				}
				else
				{
					SetIsCreditorValue(value);
					if (value)
					{
						CompanyData.SetDefaultPayablesAccount();
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateOH_IsTempAccount();
				}
			}
		}

		void SetIsCreditorValue(bool isCreditor)
		{
			var oldGeneratedCode = GetOldGeneratedCode(true);
			CompanyData.OB_IsCreditor = isCreditor;
			RegenerateCode(oldGeneratedCode);
		}

		public ZPropertyInfo OH_IsCreditorInfo
		{
			get { return GetWrappedZPropertyInfo(OHConstants.OH_IsCreditor, x => CompanyData.OB_IsCreditorInfo); }
		}

		#endregion

		#region OH_IsConsignee

		public override ZBool OH_IsConsignee
		{
			get
			{
				return base.OH_IsConsignee;
			}
			set
			{
				if (OH_IsConsignee != value)
				{
					var oldGeneratedCode = GetOldGeneratedCode(true);
					base.OH_IsConsignee = value;
					RegenerateCode(oldGeneratedCode);

					MiscServ.MarkAsNeedingValidation();
					Addresses.MarkAsNeedingValidation();
					CompanyData.MarkAsNeedingValidation();
					SupplierLinks.MarkAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateOH_IsTempAccount();
					}

					if (!OH_IsConsignee)
					{
						EmptyConsigneeRelatedCollections();
						RemoveRelatedInvalidZGuidValues("_IM");
					}

					CheckRegisteredChildObjects();
				}
			}
		}

		protected void EmptyConsigneeRelatedCollections()
		{
			ArrayList collections = new ArrayList();
			collections.Add(SupplierLinks);
			collections.Add(CustomLabels);
			collections.Add(ConsigneeContainerPenalties);
			collections.Add(LandedCostingPreferences);
			CheckIfNeedToEmptyRelatedCollections(collections);
			if (EmptyRelatedCollections)
			{
				SupplierLinks.RemoveAndDeleteAll();
				CustomLabels.RemoveAndDeleteAll();
				ConsigneeContainerPenalties.DeleteAll();
				LandedCostingPreferences.RemoveAndDeleteAll();
			}
			else
			{
				OH_IsConsignee = true;
			}
		}

		protected bool OH_IsConsignee_ReadOnly
		{
			get
			{
				bool readOnly = false;
				if (OH_IsTempAccount)
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeTempConFlag.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeTempConFlag;
				}
				else
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagCon.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagCon;
				}
				return readOnly;
			}
		}

		#endregion

		#region OH_IsConsignor

		public override ZBool OH_IsConsignor
		{
			get
			{
				return base.OH_IsConsignor;
			}
			set
			{
				if (OH_IsConsignor != value)
				{
					var oldGeneratedCode = GetOldGeneratedCode(true);
					base.OH_IsConsignor = value;
					RegenerateCode(oldGeneratedCode);

					MiscServ.MarkAsNeedingValidation();
					Addresses.MarkAsNeedingValidation();
					CompanyData.MarkAsNeedingValidation();
					BuyerLinks.MarkAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateOH_IsTempAccount();
					}

					if (!OH_IsConsignor)
					{
						EmptyConsignorRelatedCollections();
						RemoveRelatedInvalidZGuidValues("_EX");
					}

					CheckRegisteredChildObjects();
				}
			}
		}

		protected void EmptyConsignorRelatedCollections()
		{
			ArrayList collections = new ArrayList();
			collections.Add(BuyerLinks);
			CheckIfNeedToEmptyRelatedCollections(collections);
			if (EmptyRelatedCollections)
			{
				BuyerLinks.RemoveAndDeleteAll();
				ConsignorContainerPenalties.DeleteAll();
			}
			else
			{
				OH_IsConsignor = true;
			}
		}

		protected bool OH_IsConsignor_ReadOnly
		{
			get
			{
				bool readOnly = false;
				if (OH_IsTempAccount)
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeTempSPFlag.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeTempSPFlag;
				}
				else
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagSP.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagSP;
				}
				return readOnly;
			}
		}

		#endregion

		#region OH_IsForwarder

		public override ZBool OH_IsForwarder
		{
			get
			{
				return base.OH_IsForwarder;
			}
			set
			{
				if (OH_IsForwarder != value)
				{
					var oldGeneratedCode = GetOldGeneratedCode(true);
					base.OH_IsForwarder = value;
					RegenerateCode(oldGeneratedCode);

					MiscServ.MarkAsNeedingValidation();
					Addresses.MarkAsNeedingValidation();
					CompanyData.MarkAsNeedingValidation();

					if (!OH_IsForwarder)
					{
						var shoulEmptyControllingAgentCollections = OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value;
						EmptyForwarderRelatedCollections(shoulEmptyControllingAgentCollections);
						RemoveRelatedInvalidZGuidValues("_FW");
					}
				}
				CompanyData.DefaultPayablesGSTApplicableIfNecessary();
			}
		}

		void EmptyForwarderRelatedCollections(bool includeControllingAgentCollections)
		{
			var collections = new ArrayList();
			collections.Add(AppointedAgentPorts);
			collections.Add(AppointedGatewayAgentPorts);
			collections.Add(AgentRelationships);

			OrgRelatedPartyCollectionView controllingAgentParentPartiesView = null;

			if (includeControllingAgentCollections)
			{
				controllingAgentParentPartiesView = new OrgRelatedPartyCollectionView(AllParentParties);
				controllingAgentParentPartiesView.FilterByPartyType(RelatedPartyTypeList.Codes.ControllingAgent, string.Empty);
				collections.Add(controllingAgentParentPartiesView);
			}

			CheckIfNeedToEmptyRelatedCollections(collections);

			if (EmptyRelatedCollections)
			{
				AppointedAgentPorts.RemoveAndDeleteAll();
				AppointedGatewayAgentPorts.RemoveAndDeleteAll();
				AgentRelationships.RemoveAndDeleteAll();

				if (includeControllingAgentCollections && controllingAgentParentPartiesView != null)
				{
					controllingAgentParentPartiesView.RemoveAndDeleteAll();
				}
			}
			else
			{
				OH_IsForwarder = true;
			}
		}

		protected bool OH_IsForwarder_ReadOnly
		{
			get
			{
				bool readOnly = false;
				if (OH_IsTempAccount)
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeTempFAFlag.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeTempFAFlag;
				}
				else
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagFA.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagFA;
				}
				return readOnly;
			}
		}

		#endregion

		#region OH_IsGlobalAccount

		public override ZBool OH_IsGlobalAccount
		{
			get
			{
				return base.OH_IsGlobalAccount;
			}
			set
			{
				if (OH_IsGlobalAccount != value)
				{
					var oldGeneratedCode = GetOldGeneratedCode(false);
					base.OH_IsGlobalAccount = value;
					RegenerateCode(oldGeneratedCode, OrgHeaderSchema.Constants.OH_IsGlobalAccount);
					Addresses.MarkAsNeedingValidation();
					Contacts.MarkAsNeedingValidation();
					if (!IsValidationSuspended)
					{
						Validation.ValidateOH_IsNationalAccount();
					}
				}
			}
		}

		protected bool OH_IsGlobalAccount_ReadOnly
		{
			get
			{
				bool readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyIsGlobalOrg.IsAllowed : !SecurityProvider.HasNewDetailsIsGlobalOrgSecurity;
				return readOnly;
			}
		}

		#endregion

		#region OH_IsSalesLead

		public override ZBool OH_IsSalesLead
		{
			get
			{
				return base.OH_IsSalesLead;
			}
			set
			{
				if (OH_IsSalesLead != value)
				{
					var oldGeneratedCode = GetOldGeneratedCode(true);
					base.OH_IsSalesLead = value;
					RegenerateCode(oldGeneratedCode);

					if (OH_IsSalesLead && MiscServ.OM_CMPeriodOfActivity.IsEmpty)
					{
						var defaultPeriodOfActivity = OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.Value.Default;
						if (defaultPeriodOfActivity != null)
						{
							miscServ.OM_CMPeriodOfActivity = defaultPeriodOfActivity.Code;
						}
					}

					MiscServ.MarkAsNeedingValidation();
					CompanyData.MarkAsNeedingValidation();
					Addresses.MarkAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateOH_IsTempAccount();
					}

					if (!OH_IsSalesLead)
					{
						RemoveRelatedInvalidZGuidValues("_CM");
					}

					CheckRegisteredChildObjects();
				}
			}
		}

		protected bool OH_IsSalesLead_ReadOnly
		{
			get
			{
				bool readOnly = false;
				if (OH_IsTempAccount)
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeTempSalFlag.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeTempSalFlag;
				}
				else
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagSal.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagSal;
				}
				return readOnly;
			}
		}

		#endregion

		#region OH_IsCompetitor

		public override ZBool OH_IsCompetitor
		{
			get
			{
				return base.OH_IsCompetitor;
			}
			set
			{
				var oldGeneratedCode = GetOldGeneratedCode(true);
				base.OH_IsCompetitor = value;
				RegenerateCode(oldGeneratedCode);

				MiscServ.MarkAsNeedingValidation();
				Addresses.MarkAsNeedingValidation();
				CompanyData.MarkAsNeedingValidation();

				if (!OH_IsCompetitor)
				{
					ArrayList collections = new ArrayList();
					collections.Add(Clients);
					CheckIfNeedToEmptyRelatedCollections(collections);
					if (EmptyRelatedCollections)
					{
						Clients.RemoveAndDeleteAll();
					}
					else
					{
						OH_IsCompetitor = true;
					}
				}
			}
		}

		protected bool OH_IsCompetitor_ReadOnly
		{
			get
			{
				bool readOnly = false;
				if (OH_IsTempAccount)
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeTempCMFlag.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeTempCMFlag;
				}
				else
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagCM.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagCM;
				}
				return readOnly;
			}
		}

		#endregion

		#region OH_IsAirLine

		public override ZBool OH_IsAirLine
		{
			get
			{
				return base.OH_IsAirLine;
			}
			set
			{
				if (OH_IsAirLine != value)
				{
					base.OH_IsAirLine = value;
					if (!OH_IsAirLine)
					{
						MiscServ.OM_RM_Airline = ZGuid.Empty;
						CompanyData.OB_APAirlineAccountNumber = string.Empty;

						CompanyData.MarkAsNeedingValidation();
					}

					SetOrgAirlineMAWBStockManagementCollectionEditableState();
					SetOrgAirlineBranchAccountsEditableState();
					MiscServ.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region OH_IsAirWholesaler

		public override ZBool OH_IsAirWholesaler
		{
			get => base.OH_IsAirWholesaler;
			set
			{
				if (OH_IsAirWholesaler != value)
				{
					base.OH_IsAirWholesaler = value;

					SetOrgAirlineMAWBStockManagementCollectionEditableState();
					SetOrgAirlineBranchAccountsEditableState();
				}
			}
		}

		#endregion

		#region OH_IsShippingProvider

		public override ZBool OH_IsShippingProvider
		{
			get
			{
				return base.OH_IsShippingProvider;
			}
			set
			{
				if (OH_IsShippingProvider != value)
				{
					var oldGeneratedCode = GetOldGeneratedCode(true);
					base.OH_IsShippingProvider = value;
					RegenerateCode(oldGeneratedCode);

					Addresses.MarkAsNeedingValidation();
					MiscServ.MarkAsNeedingValidation();
					CompanyData.MarkAsNeedingValidation();

					if (!OH_IsShippingProvider)
					{
						OH_IsAirLine = false;
						EmptyCarrierRelatedCollections();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateOH_IsGlobalAccount();
					}
				}
			}
		}

		void EmptyCarrierRelatedCollections()
		{
			ArrayList collections = new ArrayList();
			collections.Add(CarrierAppointedAgentPorts_Stevedore);
			collections.Add(CarrierAppointedAgentPorts_AirCTO);
			collections.Add(CarrierAppointedAgentPorts_RailHeadDepot);
			collections.Add(CarrierAppointedAgentPorts_RoadDepotShed);
			collections.Add(CarrierAppointedAgentPorts_ContainerYardPark);
			collections.Add(CarrierAppointedAgentPorts_Agency);
			collections.Add(CarrierContainerPenalties);
			collections.Add(ContainerYardRelatedCarrierAppointedAgentPorts);
			collections.Add(MiscServ.CarrierServiceLevels);
			CheckIfNeedToEmptyRelatedCollections(collections);

			if (EmptyRelatedCollections)
			{
				CarrierAppointedAgentPorts_Stevedore.RemoveAndDeleteAll();
				CarrierAppointedAgentPorts_AirCTO.RemoveAndDeleteAll();
				CarrierAppointedAgentPorts_RailHeadDepot.RemoveAndDeleteAll();
				CarrierAppointedAgentPorts_RoadDepotShed.RemoveAndDeleteAll();
				CarrierAppointedAgentPorts_ContainerYardPark.RemoveAndDeleteAll();
				CarrierAppointedAgentPorts_Agency.RemoveAndDeleteAll();
				CarrierContainerPenalties.DeleteAll();
				ContainerYardRelatedCarrierAppointedAgentPorts.DeleteAll();
				MiscServ.CarrierServiceLevels.RemoveAndDeleteAll();
			}
			else
			{
				OH_IsShippingProvider = true;
			}
		}

		protected bool OH_IsShippingProvider_ReadOnly
		{
			get
			{
				return IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagCrr.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagCrr;
			}
		}

		#endregion

		#region OH_IsMiscFreightServices

		public override ZBool OH_IsMiscFreightServices
		{
			get
			{
				return base.OH_IsMiscFreightServices;
			}
			set
			{
				var oldGeneratedCode = GetOldGeneratedCode(true);
				base.OH_IsMiscFreightServices = value;
				RegenerateCode(oldGeneratedCode);
				MiscServ.MarkAsNeedingValidation();
			}
		}

		protected bool OH_IsMiscFreightServices_ReadOnly
		{
			get
			{
				bool readOnly = false;
				if (OH_IsTempAccount)
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeTempSVFlag.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeTempSVFlag;
				}
				else
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagSV.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagSV;
				}
				return readOnly;
			}
		}

		#endregion

		#region OH_IsContainerYard

		public override ZBool OH_IsContainerYard
		{
			get
			{
				return base.OH_IsContainerYard;
			}
			set
			{
				base.OH_IsContainerYard = value;
				Addresses.MarkAsNeedingValidation();
				CompanyData.MarkAsNeedingValidation();
				CheckOrgRefFacility();
			}
		}

		#endregion

		#region OH_IsUnpackDepot

		public override ZBool OH_IsUnpackDepot
		{
			get
			{
				return base.OH_IsUnpackDepot;
			}
			set
			{
				base.OH_IsUnpackDepot = value;
				CheckOrgRefFacility();
			}
		}

		#endregion

		#region OH_IsRailHead

		public override ZBool OH_IsRailHead
		{
			get
			{
				return base.OH_IsRailHead;
			}
			set
			{
				base.OH_IsRailHead = value;
				CheckOrgRefFacility();
			}
		}

		#endregion

		#region OH_IsRoadFreightDepot

		public override ZBool OH_IsRoadFreightDepot
		{
			get
			{
				return base.OH_IsRoadFreightDepot;
			}
			set
			{
				base.OH_IsRoadFreightDepot = value;
				CheckOrgRefFacility();
			}
		}

		#endregion

		#region OH_IsWarehouseClient

		public override ZBool OH_IsWarehouseClient
		{
			get
			{
				return base.OH_IsWarehouseClient;
			}
			set
			{
				var oldGeneratedCode = GetOldGeneratedCode(true);
				base.OH_IsWarehouseClient = value;
				RegenerateCode(oldGeneratedCode);
				Addresses.MarkAsNeedingValidation();
				CompanyData.MarkAsNeedingValidation();
				RefreshOrganisationAndContactsSecurityRights();
			}
		}

		void RefreshOrganisationAndContactsSecurityRights()
		{
			RefreshSecurityRights();
			FilteredContacts.ForEach(s => ((OrgContact)s).RefreshSecurityRights());
		}

		protected bool OH_IsWarehouseClient_ReadOnly
		{
			get
			{
				bool readOnly = false;
				if (OH_IsTempAccount)
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeTempWHFlag.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeTempWHFlag;
				}
				else
				{
					readOnly = IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagWH.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagWH;
				}
				return readOnly;
			}
		}

		#endregion

		#region OH_IsControllingAgent

		public override ZBool OH_IsControllingAgent
		{
			get
			{
				return base.OH_IsControllingAgent;
			}
			set
			{
				if (base.OH_IsControllingAgent != value)
				{
					base.OH_IsControllingAgent = value;

					if (!OH_IsControllingAgent && OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value)
					{
						EmptyControllingAgentRelatedCollections();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateOH_IsControllingCustomer();
					}
				}
			}
		}

		protected bool OH_IsControllingAgent_ReadOnly
		{
			get
			{
				return IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagCtrlAgent.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagCtrlAgent;
			}
		}

		void EmptyControllingAgentRelatedCollections()
		{
			var controllingAgentParentPartiesView = new OrgRelatedPartyCollectionView(AllParentParties);
			controllingAgentParentPartiesView.FilterByPartyType(RelatedPartyTypeList.Codes.ControllingAgent, string.Empty);

			var collections = new ArrayList();
			collections.Add(controllingAgentParentPartiesView);

			CheckIfNeedToEmptyRelatedCollections(collections);

			if (EmptyRelatedCollections)
			{
				controllingAgentParentPartiesView.RemoveAndDeleteAll();
			}
			else
			{
				OH_IsControllingAgent = true;
			}
		}

		#endregion

		#region OH_IsControllingCustomer

		public override ZBool OH_IsControllingCustomer
		{
			get
			{
				return base.OH_IsControllingCustomer;
			}
			set
			{
				if (base.OH_IsControllingCustomer != value)
				{
					base.OH_IsControllingCustomer = value;

					if (!OH_IsControllingCustomer && OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value)
					{
						EmptyControllingCustomerRelatedCollections();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateOH_IsControllingAgent();
					}
				}
			}
		}

		protected bool OH_IsControllingCustomer_ReadOnly
		{
			get
			{
				return IsInDatabase ? !Env.Security.OrgDetailsModifyOrgTypeFlagCtrlCustomer.IsAllowed : !SecurityProvider.HasNewDetailsOrgTypeFlagCtrlCustomer;
			}
		}

		void EmptyControllingCustomerRelatedCollections()
		{
			var controllingCustomerParentPartiesView = new OrgRelatedPartyCollectionView(AllParentParties);
			controllingCustomerParentPartiesView.FilterByPartyType(RelatedPartyTypeList.Codes.ControllingCustomer, string.Empty);

			var supplierBuyerLinks = new OrgSupplierBuyerLinkCollection(Factory, new ZQuery(OrgSupplierBuyerLinkSchema.OL_OH_ControllingCustomer, this.PK));
			supplierBuyerLinks.Load();

			var supBuyLinkTrnModes = new OrgSupBuyLinkTrnModeCollection(Factory, new ZQuery(OrgSupBuyLinkTrnModeSchema.PF_OH_ControllingCustomer, this.PK));
			supBuyLinkTrnModes.Load();

			var collections = new ArrayList();
			collections.Add(controllingCustomerParentPartiesView);
			collections.Add(supplierBuyerLinks);
			collections.Add(supBuyLinkTrnModes);

			CheckIfNeedToEmptyRelatedCollections(collections);

			if (EmptyRelatedCollections)
			{
				controllingCustomerParentPartiesView.RemoveAndDeleteAll();
				foreach (OrgSupplierBuyerLink link in supplierBuyerLinks)
				{
					link.OL_OH_ControllingCustomer = ZGuid.Empty;
				}
				foreach (OrgSupBuyLinkTrnMode mode in supBuyLinkTrnModes)
				{
					mode.PF_OH_ControllingCustomer = ZGuid.Empty;
				}
			}
			else
			{
				OH_IsControllingCustomer = true;
			}
		}

		#endregion

		#region OH_Language
		[List("Lookups.Languages")]
		public override ZString OH_Language
		{
			get
			{
				return base.OH_Language;
			}
			set
			{
				if (base.OH_Language != value)
				{
					if (Culture.LanguageCodeMapping.TryGetValue(value, out string newCode))
					{
						value = newCode;
					}
					base.OH_Language = value;

					if (value != Constants.Languages.English)
					{
						foreach (OrgAddress address in Addresses)
						{
							if (address.OA_Language == Constants.Languages.English && !address.OA_Address1.IsEnglishOnlyOrEmpty)
							{
								address.OA_Language = value;
							}
						}
					}
				}
			}
		}
		#endregion

		#region OH_RSL_ShippingLine

		[ReadOnlyMember(nameof(CanNotBeLinkedToShippingLine))]
		[RelatedBusinessObject("ShippingLine")]
		[List("Lookups.ShippingLines")]
		public override ZGuid OH_RSL_ShippingLine
		{
			get { return base.OH_RSL_ShippingLine; }
			set
			{
				if (base.OH_RSL_ShippingLine != value)
				{
					base.OH_RSL_ShippingLine = value;
					ShippingLineCarrierNameInfo.RefreshBinding();
					ShippingLineSCACInfo.RefreshBinding();
					ShippingLineIntegrationsInfo.RefreshBinding();
					SyncRefShippingLineToCusCodeIfNeeded();
				}
			}
		}

		void SyncRefShippingLineToCusCodeIfNeeded()
		{
			if (ShippingLine != null)
			{
				var c1cCode = CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);

				if (c1cCode != null)
				{
					c1cCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
					c1cCode.OK_CustomsRegNo = ShippingLine.RSL_CargoWiseOneCode;
				}
				else
				{
					var newC1C = CustomsCodes.AddNew();
					newC1C.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
					newC1C.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
					newC1C.OK_CustomsRegNo = ShippingLine.RSL_CargoWiseOneCode;
				}
			}
		}

		public bool CanNotBeLinkedToShippingLine => !OH_IsShippingLine && !OH_IsSeaWholesaler;

		#endregion

		#region ShippingLineCarrierName

		public ZString ShippingLineCarrierName
		{
			get { return ShippingLine != null ? ShippingLine.RSL_CarrierName : ZString.Empty; }
		}

		public ZPropertyInfo ShippingLineCarrierNameInfo
		{
			get { return GetZPropertyInfo(nameof(ShippingLineCarrierName)); }
		}

		#endregion

		#region ShippingLineSCAC

		public ZString ShippingLineSCAC
		{
			get { return ShippingLine != null ? ShippingLine.RSL_StandardCarrierAlphaCode : ZString.Empty; }
		}

		public ZPropertyInfo ShippingLineSCACInfo
		{
			get { return GetZPropertyInfo(nameof(ShippingLineSCAC)); }
		}

		#endregion

		#region ShippingLineIntegrations

		public ZString ShippingLineIntegrations
		{
			get { return ShippingLine != null ? ShippingLine.ShippingLineIntegration : string.Empty; }
		}

		public ZPropertyInfo ShippingLineIntegrationsInfo
		{
			get { return GetZPropertyInfo(nameof(ShippingLineIntegrations)); }
		}

		#endregion

		#region OH_ScreeningStatus

		[ReadOnly(true)]
		[List("Lookups.ScreeningStatusesList")]
		public override ZString OH_ScreeningStatus
		{
			get { return base.OH_ScreeningStatus; }
			set
			{
				base.OH_ScreeningStatus = value;
			}
		}

		#endregion

		#region Custom Properties

		protected GlbBranch BranchForCurrentUNLOCO
		{
			get
			{
				if (branchForCurrentUNLOCO == null || branchForCurrentUNLOCO.IsDeleted || branchForCurrentUNLOCO.GB_RL_NKHomePort != OH_RL_NKClosestPort)
				{
					branchForCurrentUNLOCO = GetBranchForUNLOCO(OH_RL_NKClosestPort);
				}

				return branchForCurrentUNLOCO;
			}
		}

		GlbBranch branchForCurrentUNLOCO;

		protected bool ValidUNLOCOCurrency
		{
			get { return UNLOCO != null && UNLOCO.Country != null && UNLOCO.Country.LocalCurrency != null; }
		}

		#region Overall Sales Rep Staff

		[List("StaffAssignments.OverallSalesRepStaff")]
		[MaxLength(GlbStaff.Schema.GS_FullNameMaxLength)]
		public ZString OverallSalesRepStaff
		{
			get { return (StaffAssignments.OverallSalesRepStaff != null) ? StaffAssignments.OverallSalesRepStaff.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo OverallSalesRepStaffInfo
		{
			get { return GetZPropertyInfo(nameof(OverallSalesRepStaff)); }
		}

		#endregion

		#region Overall Account Manager Staff

		[MaxLength(GlbStaff.Schema.GS_FullNameMaxLength)]
		public ZString OverallAccountManagerStaff
		{
			get { return (StaffAssignments.OverallAccountManagerStaff != null) ? StaffAssignments.OverallAccountManagerStaff.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo OverallAccountManagerStaffInfo
		{
			get { return GetZPropertyInfo(nameof(OverallAccountManagerStaff)); }
		}

		#endregion

		#region Overall Customer Service Rep Staff

		[MaxLength(GlbStaff.Schema.GS_FullNameMaxLength)]
		public ZString OverallCustomerServiceRepStaff
		{
			get { return (StaffAssignments.OverallCustomerServiceRepStaff != null) ? StaffAssignments.OverallCustomerServiceRepStaff.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo OverallCustomerServiceRepStaffInfo
		{
			get { return GetZPropertyInfo(nameof(OverallCustomerServiceRepStaff)); }
		}

		#endregion

		#region Overall Controller Staff

		[MaxLength(GlbStaff.Schema.GS_FullNameMaxLength)]
		public ZString OverallControllerStaff
		{
			get { return (StaffAssignments.OverallControllerStaff != null) ? StaffAssignments.OverallControllerStaff.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo OverallControllerStaffInfo
		{
			get { return GetZPropertyInfo(nameof(OverallControllerStaff)); }
		}

		#endregion

		public bool IsValidABN
		{
			get { return ABNValidation.CheckValidABN(PrimaryRegistrationNumber.Number); }
		}

		public ZString CityFallback
		{
			get
			{
				if (MainAddress != null)
				{
					return MainAddress.CityFallback;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZBool EmptyRelatedCollections
		{
			get { return emptyRelatedCollections; }
			set { emptyRelatedCollections = value; }
		}

		ZBool emptyRelatedCollections;

		public ZBool OrganisationTypeIsSelected
		{
			get
			{
				return OH_IsDebtor
						 || OH_IsCreditor
						 || OH_IsConsignee
						 || OH_IsConsignor
						 || OH_IsTransportClient
						 || OH_IsWarehouseClient
						 || OH_IsShippingProvider
						 || OH_IsForwarder
						 || OH_IsBroker
						 || OH_IsMiscFreightServices
						 || OH_IsCompetitor
						 || OH_IsSalesLead
						 || OH_IsControllingAgent
						 || OH_IsControllingCustomer;
			}
		}

		public ZBool PatternMatchRequiresRegen
		{
			get
			{
				return patternMatchRequiresRegen;
			}
			set
			{
				if (patternMatchRequiresRegen && !value)
				{
					// If we are going from True to False when generating the pattern match (IE: Pattern Matches have been generated)
					patternMatchRegeneratedSinceLastLoad = true;
				}

				patternMatchRequiresRegen = value;
			}
		}

		ZBool patternMatchRequiresRegen;

		ZBool patternMatchRegeneratedSinceLastLoad;

		public ZBool PatternMatchRequiresFullRegen { get; set; }

		public ZBool CountryHasStateList
		{
			get { return UNLOCO != null && UNLOCO.CountryStates != null; }
		}

		bool FilterHasErrors
		{
			get
			{
				return DateOfCallFromInfo.HasErrors() ||
					DateOfCallToInfo.HasErrors() ||
					DateNextCallFromInfo.HasErrors() ||
					DateNextCallToInfo.HasErrors() ||
					CallSalesRepInfo.HasErrors() ||
					CallContactInfo.HasErrors() ||
					CallDirectionInfo.HasErrors() ||
					CallLocationInfo.HasErrors();
			}
		}

		bool CollectionCallsFilterHasErrors
		{
			get
			{
				return DateOfCallNoteFromInfo.HasErrors() ||
					DateOfCallNoteToInfo.HasErrors() ||
					DateFollowUpFromInfo.HasErrors() ||
					DateFollowUpToInfo.HasErrors() ||
					CallingStaffInfo.HasErrors() ||
					CallNoteContactInfo.HasErrors() ||
					CollectionCallStatusInfo.HasErrors() ||
					CollectionCallDispositionInfo.HasErrors();
			}
		}

		public bool PhoneOrBusinessNumberRequiredForOrg
		{
			get
			{
				RefCountry country;
				return RequiredFieldsForOrg.RequirePhoneOrBusinessNumber && ((country = Country) != null) && (country.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
		}

		#region Part Attribute Manager

		public PartAttributeManager PartAttributeManager
		{
			get
			{
				if (partAttributeManager == null)
				{
					partAttributeManager = new PartAttributeManager(this);
				}

				return partAttributeManager;
			}
		}

		PartAttributeManager partAttributeManager;

		#endregion

		#region Credit Checking

		public CreditChecker CreditChecker
		{
			get
			{
				if (creditChecker == null)
				{
					creditChecker = new CreditChecker(this);
				}

				return creditChecker;
			}
		}

		internal CreditChecker creditChecker;

		#endregion

		#region Call Reporting Filter

		#region Date Of Call

		ZDateTime dateOfCallFrom;

		public ZDateTime DateOfCallFrom
		{
			get
			{
				return dateOfCallFrom;
			}
			set
			{
				dateOfCallFrom = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateOfCallFrom();
				}

				DateOfCallFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateOfCallFromInfo
		{
			get { return GetZPropertyInfo(Schema.DateOfCallFrom); }
		}

		ZDateTime dateOfCallTo;

		public ZDateTime DateOfCallTo
		{
			get
			{
				return dateOfCallTo;
			}
			set
			{
				dateOfCallTo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateOfCallTo();
				}

				DateOfCallToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateOfCallToInfo
		{
			get { return GetZPropertyInfo(Schema.DateOfCallTo); }
		}

		#endregion

		#region Date Next Call

		ZDateTime dateNextCallFrom;

		public ZDateTime DateNextCallFrom
		{
			get
			{
				return dateNextCallFrom;
			}
			set
			{
				dateNextCallFrom = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateNextCallFrom();
				}

				DateNextCallFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateNextCallFromInfo
		{
			get { return GetZPropertyInfo(Schema.DateNextCallFrom); }
		}

		ZDateTime dateNextCallTo;

		public ZDateTime DateNextCallTo
		{
			get
			{
				return dateNextCallTo;
			}
			set
			{
				dateNextCallTo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateNextCallTo();
				}

				DateNextCallToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateNextCallToInfo
		{
			get { return GetZPropertyInfo(Schema.DateNextCallTo); }
		}

		#endregion

		#region CallContact

		ZGuid callContact;
		[List("SalesCallContacts")]
		public ZGuid CallContact
		{
			get
			{
				return callContact;
			}
			set
			{
				callContact = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCallContact();
				}

				CallContactInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CallContactInfo
		{
			get { return GetZPropertyInfo(Schema.CallContact); }
		}

		#endregion

		#region CallSalesRep

		[List("Lookups.Staff")]
		[MaxLength(GlbStaff.Schema.GS_CodeMaxLength)]
		public ZString CallSalesRep
		{
			get { return callSalesRep; }
			set
			{
				if (callSalesRep != value)
				{
					CheckMaximumLength(CallSalesRepInfo, value);
					callSalesRep = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateCallSalesRep();
					}

					CallSalesRepInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CallSalesRepInfo
		{
			get { return GetZPropertyInfo(Schema.CallSalesRep); }
		}

		ZString callSalesRep;

		#endregion

		#region CallDirection

		[List("Lookups.DirectionList")]
		[MaxLength(6)]
		public ZString CallDirection
		{
			get
			{
				return callDirection;
			}
			set
			{
				CheckMaximumLength(CallDirectionInfo, value);
				callDirection = value;
				CallLocation = callDirection == CallDirectionNone ? ZString.Empty : CallLocation;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCallDirection();
				}

				CallDirectionInfo.RefreshBinding();
			}
		}

		ZString callDirection = CallDirectionNone;

		const string CallDirectionNone = "NONE";

		public ZPropertyInfo CallDirectionInfo
		{
			get { return GetZPropertyInfo(Schema.CallDirection); }
		}

		#endregion

		#region CallLocation

		ZString callLocation;
		[List("Lookups.Locations")]
		[MaxLength(6)]
		public ZString CallLocation
		{
			get
			{
				return callLocation;
			}
			set
			{
				CheckMaximumLength(CallLocationInfo, value);
				callLocation = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCallLocation();
				}

				CallLocationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CallLocationInfo
		{
			get { return GetZPropertyInfo(Schema.CallLocation); }
		}

		#endregion

		#endregion

		#region Collection Calls Notes Filter

		#region Date Of Call

		ZDateTime dateOfCallNoteFrom;

		public ZDateTime DateOfCallNoteFrom
		{
			get
			{
				return dateOfCallNoteFrom;
			}
			set
			{
				dateOfCallNoteFrom = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateOfCallNoteFrom();
				}

				DateOfCallNoteFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateOfCallNoteFromInfo
		{
			get { return GetZPropertyInfo(Schema.DateOfCallNoteFrom); }
		}

		ZDateTime dateOfCallNoteTo;

		public ZDateTime DateOfCallNoteTo
		{
			get
			{
				return dateOfCallNoteTo;
			}
			set
			{
				dateOfCallNoteTo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateOfCallNoteTo();
				}

				DateOfCallNoteToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateOfCallNoteToInfo
		{
			get { return GetZPropertyInfo(Schema.DateOfCallNoteTo); }
		}

		#endregion

		#region Date Follow Up Call

		ZDateTime dateFollowUpFrom;

		public ZDateTime DateFollowUpFrom
		{
			get
			{
				return dateFollowUpFrom;
			}
			set
			{
				dateFollowUpFrom = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateFollowUpFrom();
				}

				DateFollowUpFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateFollowUpFromInfo
		{
			get { return GetZPropertyInfo(Schema.DateFollowUpFrom); }
		}

		ZDateTime dateFollowUpTo;

		public ZDateTime DateFollowUpTo
		{
			get
			{
				return dateFollowUpTo;
			}
			set
			{
				dateFollowUpTo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateFollowUpTo();
				}

				DateFollowUpToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateFollowUpToInfo
		{
			get { return GetZPropertyInfo(Schema.DateFollowUpTo); }
		}

		#endregion

		#region CallNoteContact

		ZGuid callNoteContact;
		[List("SalesCallContacts")]
		public ZGuid CallNoteContact
		{
			get
			{
				return callNoteContact;
			}
			set
			{
				callNoteContact = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCallNoteContact();
				}

				CallNoteContactInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CallNoteContactInfo
		{
			get { return GetZPropertyInfo(Schema.CallNoteContact); }
		}

		#endregion

		#region CallingStaff

		ZString callingStaff;

		[List("Lookups.Staff")]
		[MaxLength(OrgCollectionNote.Schema.PN_SystemCreateUserMaxLength)]
		public ZString CallingStaff
		{
			get
			{
				return callingStaff;
			}
			set
			{
				CheckMaximumLength(CallingStaffInfo, value);
				callingStaff = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCallingStaff();
				}

				CallingStaffInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CallingStaffInfo
		{
			get { return GetZPropertyInfo(Schema.CallingStaff); }
		}

		#endregion

		#region CollectionCallStatus

		ZString collectionCallStatus;
		[List("CollectionCallStatus_List")]
		[MaxLength(OrgCollectionNote.Schema.PN_StatusMaxLength)]
		public ZString CollectionCallStatus
		{
			get
			{
				return collectionCallStatus;
			}
			set
			{
				CheckMaximumLength(CollectionCallStatusInfo, value);
				collectionCallStatus = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCollectionCallStatus();
				}

				CollectionCallStatusInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CollectionCallStatusInfo
		{
			get { return GetZPropertyInfo(Schema.CollectionCallStatus); }
		}

		public CodeDescriptionPairList CollectionCallStatus_List
		{
			get
			{
				if (collectionCallStatus_List == null)
				{
					collectionCallStatus_List = new CodeDescriptionPairList();
					collectionCallStatus_List.AddPair("ALL", Res.GetString("75a437ce-fe35-4e81-abb6-99298bf6ddb5", "All Calls"));
					collectionCallStatus_List.AddPair("NCL", Res.GetString("03ce6c3b-f114-4934-96fb-7e421aa4687f", "Not Closed"));
					collectionCallStatus_List.AddPair(CollectionNoteStatusList.Codes.Open, CollectionNoteStatusList.Descriptions.Open);
					collectionCallStatus_List.AddPair(CollectionNoteStatusList.Codes.Working, CollectionNoteStatusList.Descriptions.Working);
					collectionCallStatus_List.AddPair(CollectionNoteStatusList.Codes.Closed, CollectionNoteStatusList.Descriptions.Closed);
				}

				return collectionCallStatus_List;
			}
		}

		CodeDescriptionPairList collectionCallStatus_List;

		#endregion

		#region CollectionCallDisposition

		ZString collectionCallDisposition;
		[List("CollectionCallDisposition_List")]
		[MaxLength(OrgCollectionNote.Schema.PN_CallDispositionMaxLength)]
		public ZString CollectionCallDisposition
		{
			get
			{
				return collectionCallDisposition;
			}
			set
			{
				CheckMaximumLength(CollectionCallDispositionInfo, value);
				collectionCallDisposition = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCollectionCallDisposition();
				}

				CollectionCallDispositionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CollectionCallDispositionInfo
		{
			get { return GetZPropertyInfo(Schema.CollectionCallDisposition); }
		}

		public CodeDescriptionPairList CollectionCallDisposition_List
		{
			get
			{
				if (collectionCallDisposition_List == null)
				{
					collectionCallDisposition_List = new CollectionNoteDispositionList();
				}

				return collectionCallDisposition_List;
			}
		}

		CodeDescriptionPairList collectionCallDisposition_List;

		#endregion

		#endregion

		#region Opportunities Filter Properties

		#region OpportunityDateFrom

		[ReadOnlyMember(nameof(IsOpportunitiesDateTypeToFilterNotSpecified))]
		public ZDateTime OpportunityDateFrom
		{
			get
			{
				return opportunityDateFrom;
			}
			set
			{
				opportunityDateFrom = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOpportunityDateFrom();
				}

				OpportunityDateFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OpportunityDateFromInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OpportunityDateFrom);
			}
		}

		ZDateTime opportunityDateFrom;

		#endregion

		#region OpportunityDateTo

		[ReadOnlyMember(nameof(IsOpportunitiesDateTypeToFilterNotSpecified))]
		public ZDateTime OpportunityDateTo
		{
			get
			{
				return opportunityDateTo;
			}
			set
			{
				opportunityDateTo = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOpportunityDateTo();
				}

				OpportunityDateToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OpportunityDateToInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OpportunityDateTo);
			}
		}

		ZDateTime opportunityDateTo;

		#endregion

		#region OpportunitiesDateTypeToFilter
		[List("Lookups.DateFilterList")]
		[MaxLength(11)]
		public ZString OpportunitiesDateTypeToFilter
		{
			get
			{
				return dateTypeToFilter;
			}
			set
			{
				CheckMaximumLength(OpportunitiesDateTypeToFilterInfo, value);
				dateTypeToFilter = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOpportunitiesDateTypeToFilter();

					if (value == OrgHeaderLookups.LookupConstants.DateFilterListConstants.None)
					{
						OpportunityDateFrom = ZDateTime.Empty;
						OpportunityDateTo = ZDateTime.Empty;
					}
				}

				OpportunitiesDateTypeToFilterInfo.RefreshBinding();
				OpportunityDateFromInfo.RefreshBinding();
				OpportunityDateToInfo.RefreshBinding();
			}
		}

		ZString dateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.None;

		public ZPropertyInfo OpportunitiesDateTypeToFilterInfo
		{
			get { return GetZPropertyInfo(Schema.OpportunitiesDateTypeToFilter); }
		}

		protected bool IsOpportunitiesDateTypeToFilterNotSpecified
		{
			get { return OpportunitiesDateTypeToFilter == OrgHeaderLookups.LookupConstants.DateFilterListConstants.None; }
		}

		#endregion

		#endregion

		#region Disbursement Terms

		public ZString DisbursmentTerms
		{
			get
			{
				ZString result = "";

				if (CompanyData != null)
				{
					InvoiceTerm disbursmentTerm;
					if (CompanyData.TryToGetTheOnlyOrgARTerm(out disbursmentTerm, true))
					{
						if (!disbursmentTerm.IsTermWithoutDays)
						{
							result = disbursmentTerm.Days.ToString() +
								(disbursmentTerm.Term != Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle ? " " + (disbursmentTerm.IsTermWithMonths ? Res.GetString("abda7af3-b44c-4a04-9d1b-8ddb4b8cc8bf", "MONTHS") : Res.GetString("34a5b61a-ccd5-4332-aeea-3d8209e4e85d", "DAYS")) + " " : " ");
						}
						result += disbursmentTerm.TermDescription.Trim().ToUpper();
					}
					else
					{
						result += CompanyData.GetOrgARTermsAsCSV(", ", x => x.IsDisbursementTerm || x.IsDefaultTerm);
					}
				}
				else
				{
					InvoiceTermsList termList = new ARInvoiceTermsList();
					string termDescription = termList.GetDescriptionFromCode(Constants.InvoiceTerms.CashOnDelivery);
					if (termDescription != null)
					{
						result += termDescription.Trim().ToUpper();
					}
				}

				return result;
			}
		}

		public ZString ShortDisbursementTerms
		{
			get
			{
				ZString result = "";
				if (CompanyData != null)
				{
					InvoiceTerm disbursmentTerm;
					if (CompanyData.TryToGetTheOnlyOrgARTerm(out disbursmentTerm, true))
					{
						result = GetShortDescriptionInvoiceTerms(disbursmentTerm);
					}
					else
					{
						result = CompanyData.GetOrgARTermsAsCSV(", ", x => x.IsDisbursementTerm || x.IsDefaultTerm);
					}
				}
				return result;
			}
		}

		public ZString ShortInvoiceTerms
		{
			get
			{
				ZString result = "";
				if (CompanyData != null)
				{
					InvoiceTerm stdTerm;
					if (CompanyData.TryToGetTheOnlyOrgARTerm(out stdTerm, false))
					{
						result = GetShortDescriptionInvoiceTerms(stdTerm);
					}
					else
					{
						result = CompanyData.GetOrgARTermsAsCSV(", ", x => !x.IsDisbursementTerm || x.IsDefaultTerm);
					}
				}
				return result;
			}
		}

		string GetShortDescriptionInvoiceTerms(InvoiceTerm term)
		{
			ZString result = term.TermDescription;
			if (!term.IsTermWithoutDays)
			{
				if (term.Days.IsValid)
				{
					result = term.Days.ToString() + " " + (term.IsTermWithMonths ? Res.GetString("abda7af3-b44c-4a04-9d1b-8ddb4b8cc8bf", "MONTHS") : Res.GetString("34a5b61a-ccd5-4332-aeea-3d8209e4e85d", "DAYS"));
				}
			}
			return result.ToUpper();
		}

		#endregion

		#region Last Quoted Date

		public ZDateTime LastQuotedDate
		{
			get
			{
				var result = ZDate.Empty;

				var query = new ZDBOnlyQuery(ObjectFactory.Get<IRating>().QuoteType);
				var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, PK);
				jobDocAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				query.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
				query.AddToFilter(RatingHeaderSchema.TH_QuoteDate, SQLComparisonOperator.NotEqual, null);
				query.OrderBy = RatingHeaderSchema.Constants.TH_QuoteDate + OrderByClause.Descending;

				var quote = Factory.LoadTop1(ObjectFactory.Get<IRating>().QuoteType, query);
				if (quote != null)
				{
					result = (ZDate)quote[RatingHeaderSchema.TH_QuoteDate];
				}

				return result;
			}
		}

		#endregion

		#region External Validation Status

		public ZString ExternalValidationStatus
		{
			get
			{
				var status = "";
				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_Parent, PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.ExternalValidationPassedCode, AutoEvents.ExternalValidationFailedCode, AutoEvents.ExternalValidationNotCompletedCode });
				query.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC";

				var latestLog = Factory.LoadTop1<StmALog>(query);

				if (latestLog != null)
				{
					if (latestLog.SL_SE_NKEvent == AutoEvents.ExternalValidationPassedCode)
					{
						status = OrgConstants.FilterControl.ExternalValidationStatus.Description.Passed;
					}
					else if (latestLog.SL_SE_NKEvent == AutoEvents.ExternalValidationFailedCode)
					{
						status = OrgConstants.FilterControl.ExternalValidationStatus.Description.Failed;
					}
					else if (latestLog.SL_SE_NKEvent == AutoEvents.ExternalValidationNotCompletedCode)
					{
						status = OrgConstants.FilterControl.ExternalValidationStatus.Description.NotCompleted;
					}
				}
				else
				{
					status = OrgConstants.FilterControl.ExternalValidationStatus.Description.NotRun;
				}

				return status;
			}
		}

		public ZPropertyInfo ExternalValidationStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ExternalValidationStatus); }
		}

		#endregion

		#endregion

		#region IsDepot

		public ZBool IsDepot
		{
			get { return OH_IsMiscFreightServices && (OH_IsPackDepot || OH_IsUnpackDepot || OH_IsRoadFreightDepot || OH_IsRailHead); }
		}

		#endregion

		#region Is System Defined Organisation

		public ZBool IsSystemDefinedOrganisation
		{
			get
			{
				return PK == OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation ||
							PK == OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			}
		}

		bool ICanBeExcludedFromOperationalActions.ShouldExclude
		{
			get
			{
				return IsSystemDefinedOrganisation;
			}
		}

		string ICanBeExcludedFromOperationalActions.ReasonForExclusion
		{
			get
			{
				return Res.GetString("9bc28ca6-ec77-475c-a855-cef91802aaf5", "This organization is a special, system defined organization and cannot be modified.");
			}
		}

		#endregion

		#region Is NMFC Participant

		public ZBool IsNMFCParticipant
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (OrgCusCode customCode in CustomsCodes)
				{
					if (customCode.OK_CodeType == OrgCusCode.USACodeTypes.NMFCParticipant && customCode.OK_CustomsRegNo == OrgConstants.NMFCParticipantCodes.Code.Yes)
					{
						result = ZBool.True;
						break;
					}
				}

				return result;
			}
		}

		#endregion

		#region ENettRegistrationNumber

		public ZString ENettRegistrationNumber
		{
			get { return CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.eNettRegistrationNumber); }
		}

		#endregion

		#region DefaultCountryCodeForPhoneNumbers

		public ZString DefaultCountryCodeForPhoneNumbers
		{
			get
			{
				var result = ZString.Empty;
				var headerAddressCountryCodes = Addresses.Select(bo => bo.OA_RN_NKCountryCode).Distinct().ToArray();
				if (headerAddressCountryCodes.Length == 1)
				{
					result = headerAddressCountryCodes[0];
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region LookupsOverride

		protected override OrgHeaderLookups GetNewLookups()
		{
			return new OrgHeaderLookupsWithUserFilters(this);
		}

		#endregion

		#region Cache Related

		#region HasUniqueAddressCode

		public bool HasUniqueAddressCode(OrgAddress address)
		{
			//Cannot use CachedHashOfAddressCodes here. If currently edited address is new, it won't have hit the cache yet
			return !Addresses.Cast<OrgAddress>().Any(a => string.Equals(a.OA_Code, address.OA_Code, StringComparison.OrdinalIgnoreCase) && a.PK != address.PK);
		}

		#endregion

		#region HasDefaultOfficeAddress

		public bool HasDefaultOfficeAddress()
		{
			return HashCountGreaterThan(CachedHashOfDefaultAddressTypeCounts, OrgAddressType.Office.Code, 0);
		}

		#endregion

		#region HasOneDefaultForAddressType

		public bool HasOneDefaultForAddressType(ZString type)
		{
			return HashCountEquals(CachedHashOfDefaultAddressTypeCounts, type, 1);
		}

		#endregion

		#region HasMoreThanOneDefaultForAddressType

		public bool HasMoreThanOneDefaultForAddressType(ZString type)
		{
			return HashCountGreaterThan(CachedHashOfDefaultAddressTypeCounts, type, 1);
		}

		#endregion

		#region HasUniqueContactName

		public bool HasUniqueContactName(OrgContact contact)
		{
			HashSet<ZGuid> contactPksWithSameName;
			if (CachedContactNamesLookup.TryGetValue(contact.OC_ContactName, out contactPksWithSameName))
			{
				return !contactPksWithSameName.Any(x => x != contact.PK);
			}

			return true;
		}

		#endregion

		#region HasMoreThanOneDefaultForDocumentGroup

		public bool HasMoreThanOneDefaultForDocumentGroup(ZString group)
		{
			return HashCountGreaterThan(CachedHashOfDefaultDocGroupCounts, group, 1);
		}

		#endregion

		#region HasMoreThanOneDefaultForDocumentMenuItem

		public bool HasMoreThanOneDefaultForDocumentMenuItem(ZGuid menuItemPK)
		{
			string documentId = string.Empty;
			StmMenuItem menuItem = Factory.Load<StmMenuItem>(menuItemPK);

			if (menuItem != null)
			{
				documentId = menuItem.DocumentId;
				return HashCountGreaterThan(CachedHashOfDefaultDocMenuItemCounts, documentId, 1);
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region HasMoreThanOneNotifyParty

		public bool HasMoreThanOneNotifyParty()
		{
			return CachedNotifyPartyCount.Value > 1;
		}

		#endregion

		#region Cached Properties

		#region CachedContactNamesLookup

		Dictionary<string, HashSet<ZGuid>> CachedContactNamesLookup
		{
			get
			{
				if (cachedContactNamesLookup == null)
				{
					cachedContactNamesLookup = GetContactNamesLookup();
					HookContactNameChangeService();
				}

				return cachedContactNamesLookup;
			}
		}
		Dictionary<string, HashSet<ZGuid>> cachedContactNamesLookup;

		void InvalidateContactNamesLookupCache()
		{
			cachedContactNamesLookup = null;
		}

		Dictionary<string, HashSet<ZGuid>> GetContactNamesLookup()
		{
			var result = new Dictionary<string, HashSet<ZGuid>>(StringComparer.OrdinalIgnoreCase);

			foreach (OrgContact contact in Contacts)
			{
				HashSet<ZGuid> list;
				if (!result.TryGetValue(contact.OC_ContactName, out list))
				{
					list = new HashSet<ZGuid>();
					result.Add(contact.OC_ContactName, list);
				}

				list.Add(contact.PK);
			}

			return result;
		}

		void HookContactNameChangeService()
		{
			if (!contactNameChangeServiceHooked)
			{
				contactNameChangeServiceHooked = true;
				OrgContactNameChangeService.GetInstance(Factory).ContactNameChanged += OnContactNameChanged;
			}
		}
		bool contactNameChangeServiceHooked;

		void OnContactNameChanged(object sender, EventArgs e)
		{
			if (((OrgContact)sender).OC_OH == PK)
			{
				InvalidateContactNamesLookupCache();
			}
		}

		#endregion

		#region CachedHashOfDefaultDocGroupCounts

		CachedProperty<Hashtable> CachedHashOfDefaultDocGroupCounts
		{
			get
			{
				if (cachedHashOfDefaultDocGroupCounts == null)
				{
					cachedHashOfDefaultDocGroupCounts = new CachedProperty<Hashtable>(Factory, new GetValueDelegate<Hashtable>(GetHashOfDefaultDocGroupCounts));
				}

				return cachedHashOfDefaultDocGroupCounts;
			}
		}

		CachedProperty<Hashtable> cachedHashOfDefaultDocGroupCounts;

		Hashtable GetHashOfDefaultDocGroupCounts()
		{
			Hashtable result = new Hashtable();
			foreach (OrgContact contact in Contacts)
			{
				if (!contact.OC_IsActive)
				{
					continue;
				}

				foreach (OrgDocument doc in contact.Documents)
				{
					if (doc.OD_DefaultContact)
					{
						AddObjectToHashCounter(result, doc.OD_DocumentGroup);
					}
				}
			}

			return result;
		}

		#endregion

		#region CachedHashOfDefaultDocMenuItemCounts

		CachedProperty<Hashtable> CachedHashOfDefaultDocMenuItemCounts
		{
			get
			{
				if (cachedHashOfDefaultDocMenuItemCounts == null)
				{
					cachedHashOfDefaultDocMenuItemCounts = new CachedProperty<Hashtable>(Factory, new GetValueDelegate<Hashtable>(GetHashOfDefaultDocMenuItemCounts));
				}

				return cachedHashOfDefaultDocMenuItemCounts;
			}
		}

		CachedProperty<Hashtable> cachedHashOfDefaultDocMenuItemCounts;

		Hashtable GetHashOfDefaultDocMenuItemCounts()
		{
			Hashtable result = new Hashtable();
			foreach (OrgContact contact in Contacts)
			{
				if (!contact.OC_IsActive)
				{
					continue;
				}

				foreach (OrgDocument doc in contact.Documents)
				{
					if (doc.OD_DefaultContact)
					{
						StmMenuItem menuItem = Factory.Load<StmMenuItem>(doc.OD_SU_MenuItem);
						string documentId = menuItem != null ? menuItem.DocumentId.ToString() : doc.OD_SU_MenuItem.ToString();
						AddObjectToHashCounter(result, documentId);
					}
				}
			}

			return result;
		}

		#endregion

		#region CachedHashOfAddressCodes

		CachedProperty<Hashtable> CachedHashOfAddressCodes
		{
			get
			{
				if (cachedHashOfAddressCodes == null)
				{
					cachedHashOfAddressCodes = new CachedProperty<Hashtable>(Factory, new GetValueDelegate<Hashtable>(GetHashOfAddressCodes));
				}

				return cachedHashOfAddressCodes;
			}
		}

		CachedProperty<Hashtable> cachedHashOfAddressCodes;

		Hashtable GetHashOfAddressCodes()
		{
			Hashtable result = new Hashtable();
			foreach (OrgAddress address in Addresses)
			{
				AddObjectToHashCounter(result, address.OA_Code.ToUpper());
			}

			return result;
		}

		#endregion

		#region CachedHashOfDefaultAddressTypeCounts

		CachedProperty<Hashtable> CachedHashOfDefaultAddressTypeCounts
		{
			get
			{
				if (cachedHashOfDefaultAddressTypeCounts == null)
				{
					cachedHashOfDefaultAddressTypeCounts = new CachedProperty<Hashtable>(Factory, new GetValueDelegate<Hashtable>(GetHashOfDefaultAddressTypes));
				}

				return cachedHashOfDefaultAddressTypeCounts;
			}
		}

		CachedProperty<Hashtable> cachedHashOfDefaultAddressTypeCounts;

		Hashtable GetHashOfDefaultAddressTypes()
		{
			Hashtable result = new Hashtable();
			foreach (OrgAddress address in Addresses)
			{
				foreach (OrgAddressCapabilityWrapper oac in address.AddressCapability)
				{
					if (oac.Enabled && oac.Main)
					{
						AddObjectToHashCounter(result, oac.AddressCapabilityType);
					}
				}
			}

			return result;
		}

		#endregion

		#region CachedNotifyPartyCount

		CachedProperty<ZDecimal> CachedNotifyPartyCount
		{
			get
			{
				if (cachedNotifyPartyCount == null)
				{
					cachedNotifyPartyCount = new CachedProperty<ZDecimal>(Factory, new GetValueDelegate<ZDecimal>(GetNotifyPartyCount));
				}

				return cachedNotifyPartyCount;
			}
		}

		CachedProperty<ZDecimal> cachedNotifyPartyCount;

		ZDecimal GetNotifyPartyCount()
		{
			ZDecimal count = 0;
			foreach (OrgContact contact in Contacts)
			{
				foreach (OrgDocument doc in contact.Documents)
				{
					if (doc.OD_DocumentGroup == ContactType.NotifyParty.Code)
					{
						count++;
					}
				}
			}

			return count;
		}

		#endregion

		#endregion

		#region Cache Helper Methods

		void AddObjectToHashCounter(Hashtable hash, object obj)
		{
			object count = hash[obj];
			if (count != null)
			{
				hash[obj] = ((int)count) + 1;
			}
			else
			{
				hash[obj] = 1;
			}
		}

		bool HashCountEquals(CachedProperty<Hashtable> property, object key, int value)
		{
			Hashtable counts = property.Value;
			object qty = counts[key];
			return qty != null && (int)qty == value;
		}

		bool HashCountGreaterThan(CachedProperty<Hashtable> property, object key, int value)
		{
			Hashtable counts = property.Value;
			object qty = counts[key];
			return qty != null && (int)qty > value;
		}

		#endregion

		#endregion

		#region Organisation Types

		public OrganisationTypes OrganisationTypes
		{
			get
			{
				OrganisationTypes result = OrganisationTypes.None;
				if (OH_IsDebtor)
				{
					result |= OrganisationTypes.Debtor;
				}

				if (OH_IsCreditor)
				{
					result |= OrganisationTypes.Creditor;
				}

				if (OH_IsConsignor)
				{
					result |= OrganisationTypes.Consignor;
				}

				if (OH_IsConsignee)
				{
					result |= OrganisationTypes.Consignee;
				}

				if (OH_IsTransportClient)
				{
					result |= OrganisationTypes.TransportClient;
				}

				if (OH_IsShippingProvider)
				{
					result |= OrganisationTypes.Carrier;
				}

				if (OH_IsForwarder)
				{
					result |= OrganisationTypes.Forwarder;
				}

				if (OH_IsBroker)
				{
					result |= OrganisationTypes.Broker;
				}

				if (OH_IsMiscFreightServices)
				{
					result |= OrganisationTypes.Services;
				}

				if (OH_IsCompetitor)
				{
					result |= OrganisationTypes.Competitor;
				}

				if (OH_IsSalesLead)
				{
					result |= OrganisationTypes.Sales;
				}

				if (OH_IsWarehouseClient)
				{
					result |= OrganisationTypes.WarehouseClient;
				}

				if (OH_IsDistributionCentre)
				{
					result |= OrganisationTypes.DistributionCentre;
				}

				if (OH_IsControllingAgent)
				{
					result |= OrganisationTypes.ControllingAgent;
				}

				if (OH_IsControllingCustomer)
				{
					result |= OrganisationTypes.ControllingCustomer;
				}
				return result;
			}
			set
			{
				OH_IsDebtor = (value & OrganisationTypes.Debtor) != 0;
				OH_IsCreditor = (value & OrganisationTypes.Creditor) != 0;
				OH_IsConsignor = (value & OrganisationTypes.Consignor) != 0;
				OH_IsConsignee = (value & OrganisationTypes.Consignee) != 0;
				OH_IsTransportClient = (value & OrganisationTypes.TransportClient) != 0;
				OH_IsShippingProvider = (value & OrganisationTypes.Carrier) != 0;
				OH_IsForwarder = (value & OrganisationTypes.Forwarder) != 0;
				OH_IsBroker = (value & OrganisationTypes.Broker) != 0;
				OH_IsMiscFreightServices = (value & OrganisationTypes.Services) != 0;
				OH_IsCompetitor = (value & OrganisationTypes.Competitor) != 0;
				OH_IsSalesLead = (value & OrganisationTypes.Sales) != 0;
				OH_IsWarehouseClient = (value & OrganisationTypes.WarehouseClient) != 0;
				OH_IsDistributionCentre = (value & OrganisationTypes.DistributionCentre) != 0;
				OH_IsControllingAgent = (value & OrganisationTypes.ControllingAgent) != 0;
				OH_IsControllingCustomer = (value & OrganisationTypes.ControllingCustomer) != 0;
			}
		}

		#region OrganisationTypesAsString

		public ZString OrganisationTypesAsString
		{
			get
			{
				return OrganisationTypeDescriptor.GetString(this.OrganisationTypes);
			}
		}

		public ZPropertyInfo OrganisationTypesAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationTypesAsString)); }
		}

		#endregion

		#region DebtorCompany

		public ZString DebtorCompany => string.Join(", ", CompanyDataCollection.Cast<OrgCompanyData>().Where(d => d.OB_IsDebtor && d.Company != null).Select(o => o.Company.CompanyName));

		#endregion

		#region CreditorCompany

		public ZString CreditorCompany => string.Join(", ", CompanyDataCollection.Cast<OrgCompanyData>().Where(d => d.OB_IsCreditor && d.Company != null).Select(o => o.Company.CompanyName));

		#endregion

		#endregion

		#region Notes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection noteTypes = new NoteTypeCollection();

				noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
				noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
				noteTypes.Add(PredefinedNoteTypes.Instance.PickupInstructionsNote);
				noteTypes.Add(PredefinedNoteTypes.Instance.OrderManagementNote);
				noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
				noteTypes.Add(PredefinedNoteTypes.Instance.InvoicingPreferences);
				noteTypes.Add(PredefinedNoteTypes.Instance.ImportCustomsHandlingNotes);
				noteTypes.Add(PredefinedNoteTypes.Instance.ExportCustomsHandlingNotes);
				noteTypes.Add(PredefinedNoteTypes.Instance.AccountsReceivableAccountManagementNotes);
				noteTypes.Add(PredefinedNoteTypes.Instance.AccountsReceivableCreditManagementNote);
				noteTypes.Add(PredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes);
				noteTypes.Add(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog);
				noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
				noteTypes.Add(PredefinedNoteTypes.Instance.SpecialInstructions);
				noteTypes.Add(PredefinedNoteTypes.Instance.AdditionalInformation);
				noteTypes.Add(PredefinedNoteTypes.Instance.ExportersBankName);
				noteTypes.Add(PredefinedNoteTypes.Instance.ExportersBankAccountNo);
				noteTypes.Add(PredefinedNoteTypes.Instance.ExportersBankSWIFTCode);
				noteTypes.Add(PredefinedNoteTypes.Instance.MethodOfPayment);

				if (!OH_IsActive)
				{
					noteTypes.Add(PredefinedNoteTypes.Instance.InactiveRecordDetails);
				}

				return noteTypes;
			}
		}

		#endregion

		#region Multiple language Address

		public bool ShouldAskToCreateEnglishMainAddress
		{
			get
			{
				return !MainAddress.IsEnglishOnlyOrEmpty &&
						 (!MainAddress.IsInDatabase || (MainAddress.IsOriginalValueEnglishOnly && !HasEnglishMainAddress));
			}
		}

		public OrgTranslatedAddress CreateEnglishEquivalentAddress(OrgAddress nonEnglishAddress)
		{
			var englishAddress = nonEnglishAddress.TranslatedAddresses.AddNew();
			nonEnglishAddress.AddressLanguagePack.Add(englishAddress);

			englishAddress.OTA_Language = Constants.Languages.English;
			englishAddress.OTA_Address1 = nonEnglishAddress.OA_Address1.IsEnglishOnlyOrEmpty ? nonEnglishAddress.OA_Address1 : (ZString)WesternLanguageTransliterationHelper.TransliterateToEnglish(nonEnglishAddress.OA_Address1);
			englishAddress.OTA_Address2 = nonEnglishAddress.OA_Address2.IsEnglishOnlyOrEmpty ? nonEnglishAddress.OA_Address2 : (ZString)WesternLanguageTransliterationHelper.TransliterateToEnglish(nonEnglishAddress.OA_Address2);
			englishAddress.OTA_City = nonEnglishAddress.OA_City.IsEnglishOnlyOrEmpty ? nonEnglishAddress.OA_City : (ZString)WesternLanguageTransliterationHelper.TransliterateToEnglish(nonEnglishAddress.OA_City);
			englishAddress.OTA_PostCode = nonEnglishAddress.OA_PostCode.IsEnglishOnlyOrEmpty ? nonEnglishAddress.OA_PostCode : (ZString)WesternLanguageTransliterationHelper.TransliterateToEnglish(nonEnglishAddress.OA_PostCode);

			return englishAddress;
		}

		#endregion

		#region AR/AP document address

		public OrgAddress AddressForSendingARDocuments
		{
			get
			{
				var language = ExpectedAddressLanguage;

				return Addresses.DefaultAddressOfType(OrgAddressType.Receivables, preferredLanguage: language)
					?? Addresses.DefaultAddressOfType(OrgAddressType.Postal, preferredLanguage: language)
					?? Addresses.DefaultAddressOfType(OrgAddressType.Office, preferredLanguage: language);
			}
		}

		public OrgAddress AddressForSendingAPDocuments
		{
			get
			{
				var language = ExpectedAddressLanguage;

				return Addresses.DefaultAddressOfType(OrgAddressType.Payables, preferredLanguage: language)
					?? Addresses.DefaultAddressOfType(OrgAddressType.Postal, preferredLanguage: language)
					?? Addresses.DefaultAddressOfType(OrgAddressType.Office, preferredLanguage: language);
			}
		}

		string ExpectedAddressLanguage
		{
			get
			{
				var expectedAddressLanguage = Constants.Languages.English;

				var currentCompanyOrgProxy = GlbCompany.CurrentCompany.OrgProxy;
				if (currentCompanyOrgProxy != null && OH_Language == currentCompanyOrgProxy.OH_Language)
				{
					expectedAddressLanguage = OH_Language;
				}

				return expectedAddressLanguage;
			}
		}

		#endregion

		#region Finding Addresses By Type

		public OrgAddress GetAddressWithFallback(AddressType addressType)
		{
			ZGuid orgAddressPK = ZGuid.Empty;
			switch (addressType)
			{
				case AddressType.NoDefault:
					orgAddressPK = ZGuid.Empty;
					break;
				case AddressType.APM:
					orgAddressPK = Address_List.APMAddressOrFallback;
					break;
				case AddressType.ARM:
					orgAddressPK = Address_List.ARMAddressOrFallback;
					break;
				case AddressType.DLV:
					orgAddressPK = Address_List.DLVAddressOrFallback;
					break;
				case AddressType.PIC:
					orgAddressPK = Address_List.PICAddressOrFallback;
					break;
				case AddressType.SQM:
					orgAddressPK = Address_List.SQMAddressOrFallback;
					break;
				case AddressType.OFC:
					orgAddressPK = Address_List.OFCAddressOrFallback;
					break;
				case AddressType.PST:
					orgAddressPK = Address_List.PSTAddressOrFallback;
					break;
				case AddressType.CST:
					orgAddressPK = Address_List.CSTAddressOrFallback;
					break;
				case AddressType.ECA:
					orgAddressPK = Address_List.ECAAddressOrFallback;
					break;
			}

			if (orgAddressPK == ZGuid.Empty)
			{
				return MainAddress;
			}
			else
			{
				return (OrgAddress)Factory.Load(typeof(OrgAddress), orgAddressPK);
			}
		}

		#endregion

		#region Code Calculation

		OrgCodeGenerator orgCodeGen;
		bool regenerateCodeWhenOrgTypesChange;

		public bool CanGenerateCode
		{
			get
			{
				bool result = !OH_FullName.IsEmpty;

				if (result)
				{
					if (OrgCodeGen.GetAlgorithm(this).NeedsUNLOCO())
					{
						result = !OH_RL_NKClosestPort.IsEmpty;
					}
				}

				return result;
			}
		}

		internal OrgCodeGenerator OrgCodeGen
		{
			get { return orgCodeGen ?? (orgCodeGen = new OrgCodeGenerator()); }
		}

		public bool RegenerateCodeWhenOrgTypesChange
		{
			get { return regenerateCodeWhenOrgTypesChange; }
			set { regenerateCodeWhenOrgTypesChange = value; }
		}

		void GenerateFinalCode()
		{
			var code = OrgCodeGen.GenerateCode(this);
			if (ShouldGenerateCode(code)
				&& (OH_Code.IsEmpty || ((!code.HasChanged(OH_Code) || (!code.HasUniqueNumber && code.NumberPartCanBeReduced(code.GetProposedCode(), OH_Code)))
						&& (!IsInDatabase || code.HasChanged(OH_CodeInfo.OriginalValue.ToString())))))
			{
				OH_Code = code.GetFinalCode();
			}
			else if (isCodeWithUniqueNumberGenerationPending)
			{
				OH_Code = code.GetFinalCode();
			}
			else if (!OH_Code.IsEmpty)
			{
#if DEBUG
				if (IsCodeDuplicatedCheckDisabledForOnce)
				{
					IsCodeDuplicatedCheckDisabledForOnce = false;
					return;
				}
#endif
				if (CheckCodeIsDuplicated())
				{
					RegeneratingCodeAfterPossibleUserEdit = RegeneratingCodeAfterPossibleUserEdit || Env.Registry.CanUserEditOrganisationCode;
					OH_Code = code.GetFinalCode();
				}
			}
		}

		public void GenerateProposedCode(bool generateByPropertyChanges = true)
		{
			bool calculateCondition = generateByPropertyChanges ?
				(!IsInDatabase || OrgCodeGen.GetAlgorithm(this).RegenerateOrgCodeOnChanges) :
				(bool)OrgCodeGen.GetAlgorithm(this).AllowRecalculatedOrgCodeByUser;

			if (CanGenerateCode)
			{
				var code = OrgCodeGen.GenerateCode(this);

				if (code.HasChanged(OH_Code) && calculateCondition)
				{
					string originalCode = OH_CodeInfo.OriginalValue.ToString();

					if (code.HasChanged(originalCode))
					{
						if (code.HasUniqueNumber && !Env.Registry.CanUserEditOrganisationCode)
						{
							isCodeWithUniqueNumberGenerationPending = true;
						}
						else
						{
							OH_Code = code.GetProposedCode();
						}
					}
					else
					{
						OH_Code = originalCode;
					}
				}
			}
		}

		public bool CheckCodeIsDuplicated()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, OH_Code);
			filter.AddToFilter(JoinCondition.And, OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
			return Factory.LoadTop1<OrgHeader>(filter) != null;
		}

#if DEBUG
		public bool IsCodeDuplicatedCheckDisabledForOnce;
#endif

		IOrgCode GetOldGeneratedCode(bool orgTypeChange)
		{
			return !orgTypeChange || RegenerateCodeWhenOrgTypesChange ? OrgCodeGen.GenerateCode(this) : null;
		}

		bool ShouldGenerateCode(IOrgCode code, string triggeringProperty = null)
		{
			bool result = false;

			if (OH_Code.IsEmpty)
			{
				result = true;
			}
			else if (!IsInDatabase)
			{
				result = !Env.Registry.CanUserEditOrganisationCode || !code.HasChanged(OH_Code);
			}
			else if (OrgCodeGen.GetAlgorithm(this).RegenerateOrgCodeOnChanges)
			{
				var algorithm = OrgCodeGen.GetAlgorithm(this);
				algorithm.RunPreSaveValidation();
				if (algorithm.HasErrors)
				{
					InvalidCodeGenAlgorithm = true;
					Validation.ValidateOH_Code();
				}
				else if (!string.IsNullOrEmpty(triggeringProperty))
				{
					if (triggeringProperty == OrgHeaderSchema.Constants.OH_IsNationalAccount || triggeringProperty == OrgHeaderSchema.Constants.OH_IsGlobalAccount)
					{
						result = true;
						RegeneratingCodeAfterPossibleUserEdit = Env.Registry.CanUserEditOrganisationCode && code.HasChanged(OH_Code, false);
					}
					else
					{
						foreach (OrgCodeElement element in OrgCodeGen.GetAlgorithm(this).Elements)
						{
							switch (element.Description)
							{
								case OrgCodeElementDescription.UnlocoCode:
								case OrgCodeElementDescription.IataCode:
								case OrgCodeElementDescription.CountryCode:
									{
										if (triggeringProperty == OrgHeaderSchema.Constants.OH_RL_NKClosestPort)
										{
											result = true;
										}

										break;
									}
								case OrgCodeElementDescription.FirstName:
								case OrgCodeElementDescription.LastName:
								case OrgCodeElementDescription.SecondName:
									{
										if (triggeringProperty == OrgHeaderSchema.Constants.OH_FullName)
										{
											result = true;
										}

										break;
									}
							}
							if (result)
							{
								RegeneratingCodeAfterPossibleUserEdit = Env.Registry.CanUserEditOrganisationCode && code.HasChanged(OH_Code, false);
								break;
							}
						}
					}
				}
			}

			return result;
		}

		public bool RegeneratingCodeAfterPossibleUserEdit
		{
			get;
			private set;
		}

		public bool InvalidCodeGenAlgorithm
		{
			get;
			private set;
		}

		void RegenerateCode(IOrgCode oldGeneratedCode, string triggeringProperty = null)
		{
			// If there is already a code in the field and the user can edit org codes, then do not override the code with a new value.
			if (!IsCopying && (oldGeneratedCode != null) && ShouldGenerateCode(oldGeneratedCode, triggeringProperty))
			{
				GenerateProposedCode();
			}
		}

		bool isCodeWithUniqueNumberGenerationPending;

		#endregion

		#region Retrieval Methods

		protected GlbBranch GetBranchForUNLOCO(string unloco)
		{
			ZQuery filter = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, unloco);
			filter.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			return (GlbBranch)Factory.LoadTop1(typeof(GlbBranch), filter);
		}

		RefCurrency GetCurrency(RefCurrency currencyToSet, RefUNLOCO prevUNLOCO)
		{
			if (currencyToSet == null || currencyToSet == GetCurrencyFromUNLOCO(prevUNLOCO))
			{
				return UNLOCO.Country.LocalCurrency;
			}
			else
			{
				return currencyToSet;
			}
		}

		protected ZString GetCurrency(ZString currencyToSet, RefUNLOCO prevUNLOCO)
		{
			if (currencyToSet.IsEmpty)
			{
				return UNLOCO.Country.RN_RX_NKLocalCurrency;
			}

			RefCurrency currency = GetCurrencyFromUNLOCO(prevUNLOCO);
			if (currency != null && currencyToSet == currency.RX_Code)
			{
				return UNLOCO.Country.RN_RX_NKLocalCurrency;
			}

			return currencyToSet;
		}

		protected RefCurrency GetCurrencyFromUNLOCO(RefUNLOCO unloco)
		{
			if (unloco != null && unloco.Country != null)
			{
				return unloco.Country.LocalCurrency;
			}

			return null;
		}

		protected RefCountry GetCountryBusinessObject(ZString countryCode)
		{
			RefCountry country = (RefCountry)Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, countryCode);
			return country;
		}

		OrgCusCode CodeForTaxRegistration
		{
			get
			{
				if (codeForTaxRegistration == null || codeForTaxRegistration.IsDeleted)
				{
					codeForTaxRegistration = null;
					ZString currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					ZString orgCountryCode = UNLOCO != null ? UNLOCO.RL_RN_NKCountryCode : ZString.Empty;
					var orgCusCodePredicateProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIOrgCusCodePredicateProvider(currentCountryCode);

					foreach (OrgCusCode customCode in CustomsCodes)
					{
						if (orgCusCodePredicateProvider != null && !orgCusCodePredicateProvider.IncludeForOrgHeaderTaxRegsistration(customCode))
						{
							continue;
						}
						if (customCode.CodeCountry != null && customCode.OK_CodeType == ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(customCode.OK_RN_NKCodeCountry))
						{
							if (currentCountryCode == customCode.OK_RN_NKCodeCountry)
							{
								codeForTaxRegistration = customCode;
								break;
							}
							else if (!orgCountryCode.IsEmpty && orgCountryCode == customCode.OK_RN_NKCodeCountry)
							{
								codeForTaxRegistration = customCode;
							}
						}
					}

					if (codeForTaxRegistration == null)
					{
						codeForTaxRegistration = new BusinessObjectFactory().New<OrgCusCode>();
					}
				}

				return codeForTaxRegistration;
			}
		}

#if DEBUG
		public void ResetCodeForTaxRegistration_ForTestOnly()
		{
			codeForTaxRegistration = null;
		}
#endif

		OrgCusCode codeForTaxRegistration;

		public RefCountry CountryOfTaxRegistration
		{
			get { return CodeForTaxRegistration != null ? CodeForTaxRegistration.CodeCountry : null; }
		}

		public ZString RawTaxRegistrationNumber => GetRawTaxRegistrationNumber(CodeForTaxRegistration);

		public ZString TaxRegistrationNumber => GetTaxRegistrationNumber(CodeForTaxRegistration);

		ZString GetRawTaxRegistrationNumber(OrgCusCode orgCusCode)
		{
			return orgCusCode != null ? orgCusCode.OK_CustomsRegNo : ZString.Empty;
		}

		ZString GetTaxRegistrationNumber(OrgCusCode orgCusCode, bool usePrefix = true)
		{
			var result = ZString.Empty;

			if (orgCusCode != null)
			{
				result = orgCusCode.OK_CustomsRegNo;
				var prefixCode = RefCountry.GetPrefixForTaxRegistrationCode(orgCusCode.OK_RN_NKCodeCountry);
				if (usePrefix && !result.StartsWith(prefixCode, StringComparison.OrdinalIgnoreCase))
				{
					result = prefixCode + result;
				}
			}

			return result;
		}

		#endregion

		#region GetCodeForTaxRegistrationInOrgCountry

		public ZString GetCodeForTaxRegistrationInOrgCountry(OrgAddress address)
		{
			return GetCodeForTaxRegistrationInOrgCountryCore(address?.Country).registrationNumber;
		}

		public (ZString countryCode, ZString registrationNumber) GetCountryCodeAndTaxRegistrationWithoutPrefix(OrgAddress address)
		{
			return GetCodeForTaxRegistrationInOrgCountryCore(address?.Country, usePrefix: false);
		}

		public (ZString countryCode, ZString registrationNumber) GetCountryCodeAndTaxRegistrationWithoutPrefix(ZString? addressCountryCode)
		{
			var addressCountry = (!addressCountryCode.HasValue || (addressCountryCode == Country?.RN_Code)) ? Country : RefCountry.LoadFromCountryCode(Factory, addressCountryCode.Value);
			return GetCodeForTaxRegistrationInOrgCountryCore(addressCountry, usePrefix: false);
		}

		(ZString countryCode, ZString registrationNumber) GetCodeForTaxRegistrationInOrgCountryCore(RefCountry addressCountry, bool usePrefix = true)
		{
			var registrationNumber = ZString.Empty;
			var countryCode = ZString.Empty;

			var homeCountry = OH_IsGlobalAccount && addressCountry != null ? addressCountry : Country;
			if (homeCountry != null)
			{
				var taxCodes = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationCodesForOrgCountry(homeCountry.RN_Code);
				var matchedCountryCustomsCodes = CustomsCodes.Cast<OrgCusCode>().Where(c => c.OK_RN_NKCodeCountry == homeCountry.RN_Code).ToArray();

				OrgCusCode code = null;
				var taxCode = taxCodes.FirstOrDefault(x => matchedCountryCustomsCodes.FirstOrDefault(y => y.OK_CodeType == x) != null);
				if (!string.IsNullOrEmpty(taxCode))
				{
					code = matchedCountryCustomsCodes.FirstOrDefault(m => m.OK_CodeType == taxCode);
				}

				countryCode = homeCountry.RN_Code;
				registrationNumber = homeCountry.IsPartOfEuropeanUnion || countryCode == Constants.CountryCodes.UnitedKingdom ? GetTaxRegistrationNumber(code, usePrefix) : GetRawTaxRegistrationNumber(code);
			}

			return (countryCode, registrationNumber);
		}

		#endregion

		#region Helper Methods

		#region Opportunities Filter

		ZQuery DateTypeFilter
		{
			get
			{
				ZQuery result = new ZQuery();
				if (OpportunitiesDateTypeToFilter != OrgHeaderLookups.LookupConstants.DateFilterListConstants.None)
				{
					if (!OpportunityDateFrom.IsEmpty)
					{
						var utcOpportunityDateFrom = Env.Time.GetUtcFromLocalTime(OpportunityDateFrom.ToDateTime());
						if (OpportunitiesDateTypeToFilter == OrgHeaderLookups.LookupConstants.DateFilterListConstants.ClosedDate)
						{
							result.AddToFilter(OrgOpportunitySchema.P8_ClosedDate, SQLComparisonOperator.GreaterThanOrEqualTo, utcOpportunityDateFrom);
						}
						else
						{
							result.AddToFilter(OrgOpportunitySchema.P8_RecallDate, SQLComparisonOperator.GreaterThanOrEqualTo, utcOpportunityDateFrom);
						}
					}

					if (!OpportunityDateTo.IsEmpty)
					{
						var utcOpportunityDateTo = Env.Time.GetUtcFromLocalTime(OpportunityDateTo.ToDateTime());
						if (OpportunitiesDateTypeToFilter == OrgHeaderLookups.LookupConstants.DateFilterListConstants.ClosedDate)
						{
							result.AddToFilter(OrgOpportunitySchema.P8_ClosedDate, SQLComparisonOperator.LessThanOrEqualTo, utcOpportunityDateTo);
						}
						else
						{
							result.AddToFilter(OrgOpportunitySchema.P8_RecallDate, SQLComparisonOperator.LessThanOrEqualTo, utcOpportunityDateTo);
						}
					}
				}

				return result;
			}
		}

		public void LoadOpportunitiesWithFiltering()
		{
			if (!OpportunityDateFromInfo.HasErrors() && !OpportunityDateToInfo.HasErrors() && !OpportunitiesDateTypeToFilterInfo.HasErrors())
			{
				SalesOpportunities.Load(DateTypeFilter);
			}
			else
			{
				OnFilterValidationError();
			}
		}

		public void ClearOpportunitiesFilterValues()
		{
			OpportunitiesDateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.None;
			OpportunityDateFrom = ZDateTime.Empty;
			OpportunityDateTo = ZDateTime.Empty;

			LoadOpportunitiesWithFiltering();
		}

		#endregion

		#region FilterValidationError

		public event EventHandler FilterValidationError;

		void OnFilterValidationError()
		{
			if (FilterValidationError != null)
			{
				FilterValidationError(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Collection Calls Notes Filtering

		#region Custom Filters

		ZQuery DateOfCallNoteFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				if (!DateOfCallNoteFrom.IsEmpty)
				{
					result.AddToFilter(OrgCollectionNoteSchema.PN_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, DateOfCallNoteFrom);
				}

				if (!DateOfCallNoteTo.IsEmpty)
				{
					result.AddToFilter(OrgCollectionNoteSchema.PN_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, DateOfCallNoteTo);
				}

				return result;
			}
		}

		ZQuery DateFollowUpFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				if (!DateFollowUpFrom.IsEmpty)
				{
					result.AddToFilter(OrgCollectionNoteSchema.PN_CallBackDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, DateFollowUpFrom);
				}

				if (!DateFollowUpTo.IsEmpty)
				{
					result.AddToFilter(OrgCollectionNoteSchema.PN_CallBackDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, DateFollowUpTo);
				}

				return result;
			}
		}

		ZQuery NoteContactFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				if (!CallNoteContact.IsEmpty)
				{
					result.AddToFilter(OrgCollectionNoteSchema.PN_OC, CallNoteContact);
				}

				return result;
			}
		}

		ZQuery CallingStaffFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				if (!CallingStaff.IsEmpty)
				{
					result.AddToFilter(OrgCollectionNoteSchema.PN_SystemCreateUser, CallingStaff);
				}

				return result;
			}
		}

		ZQuery CollectionCallStatusFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				if (!CollectionCallStatus.IsEmpty && CollectionCallStatus != "ALL")
				{
					if (CollectionCallStatus == "NCL")
					{
						result.AddToFilter(OrgCollectionNoteSchema.PN_Status, SQLComparisonOperator.NotEqual, CollectionNoteStatusList.Codes.Closed);
					}
					else
					{
						result.AddToFilter(OrgCollectionNoteSchema.PN_Status, CollectionCallStatus);
					}
				}

				return result;
			}
		}

		ZQuery CollectionCallDispositionFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				if (!CollectionCallDisposition.IsEmpty)
				{
					result.AddToFilter(OrgCollectionNoteSchema.PN_CallDisposition, CollectionCallDisposition);
				}

				return result;
			}
		}

		#endregion
		#region LoadCollectionNotesWithFiltering

		public void LoadCollectionNotesWithFiltering()
		{
			if (!CollectionCallsFilterHasErrors)
			{
				CollectionNotes.Load(GetCollectionCallsQuery());
			}
		}

		ZQuery GetCollectionCallsQuery()
		{
			ZQuery collectionCallsQuery = new ZQuery();

			collectionCallsQuery.AddToFilter(CollectionCallStatusFilter);
			collectionCallsQuery.AddToFilter(CollectionCallDispositionFilter);
			collectionCallsQuery.AddToFilter(DateOfCallNoteFilter);
			collectionCallsQuery.AddToFilter(DateFollowUpFilter);
			collectionCallsQuery.AddToFilter(NoteContactFilter);
			collectionCallsQuery.AddToFilter(CallingStaffFilter);

			return collectionCallsQuery;
		}

		#endregion

		public void ClearCollectionNotesFilterValues()
		{
			DateOfCallNoteFrom = ZDateTime.Empty;
			DateOfCallNoteTo = ZDateTime.Empty;
			DateFollowUpFrom = ZDateTime.Empty;
			DateFollowUpTo = ZDateTime.Empty;
			CallNoteContact = ZGuid.Empty;
			CallingStaff = ZString.Empty;

			LoadCollectionNotesWithFiltering();
		}

		#endregion

		void CheckOrgRefFacility()
		{
			foreach (OrgRefFacility orgRefFacility in OrgRefFacilities)
			{
				orgRefFacility.Validation.ValidateOFC_RFT_Facility();
			}
		}

		public void SetCustomsCode(string codeType, RefCountry country, string customsCode)
		{
			OrgCusCode cusCode = CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(codeType, country);
			if (cusCode == null)
			{
				cusCode = CustomsCodes.AddNew();
				cusCode.OK_RN_NKCodeCountry = country.Code;
				cusCode.OK_CodeType = codeType;
			}

			cusCode.OK_CustomsRegNo = customsCode;
		}

		public void SetLocalCustomsCode(string codeType, string customsCode)
		{
			SetCustomsCode(codeType, GlbCompany.CurrentCompany.Country, customsCode);
		}

		public ZBool IsProxyOrgOfAnyCompany()
		{
			return IsProxyOrgOfAnyCompany(false);
		}

		public ZBool IsProxyOrgOfAnyCompany(bool excludeCurrentLoginCompany)
		{
			return IsProxyOrgOfAnyCompanyOnly(excludeCurrentLoginCompany) || IsProxyOrgOfAnyBranchOnly(excludeCurrentLoginCompany);
		}

		public ZBool IsProxyOrgOfAnyCompanyOnly(bool excludeCurrentLoginCompany)
		{
			var result = ZBool.False;

			var companyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, PK);
			companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);

			if (excludeCurrentLoginCompany)
			{
				companyQuery.AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			}

			if (Factory.LoadTop1<GlbCompany>(companyQuery) != null)
			{
				result = ZBool.True;
			}
			return result;
		}

		public ZBool IsProxyOrgOfAnyBranchOnly(bool excludeCurrentLoginCompany)
		{
			var result = ZBool.False;

			var branchQuery = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, PK);
			branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);

			if (excludeCurrentLoginCompany)
			{
				branchQuery.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			}

			var branches = Factory.Load<GlbBranch>(branchQuery);
			if (branches.Length != 0)
			{
				result = branches.Any(branch => branch.Company?.GC_IsActive ?? false);
			}
			return result;
		}

		public ZBool IsProxyOrg(GlbCompany company)
		{
			ZBool orgProxyMatch = ZBool.False;
			if (company != null && company.GC_IsActive)//if company is inactive, none of its branches should be active.
			{
				if (company.GC_OH_OrgProxy == PK)
				{
					orgProxyMatch = ZBool.True;
				}
				else
				{
					var branchQuery = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, PK);
					branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
					branchQuery.AddToFilter(GlbBranchSchema.GB_GC, company.PK);
					orgProxyMatch = company.Factory.LoadTop1<GlbBranch>(branchQuery) != null;
				}
			}

			return orgProxyMatch;
		}

		public List<GlbCompany> CompanyProxies(bool excludeCurrentLoginCompany)
		{
			List<GlbCompany> result = new List<GlbCompany>();
			ZQuery query = new ZQuery(GlbCompanySchema.GC_IsActive, true);
			if (excludeCurrentLoginCompany)
			{
				query.AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			}

			var companies = Factory.Load<GlbCompany>(query);

			foreach (GlbCompany company in companies)
			{
				if (IsProxyOrg(company))
				{
					result.Add(company);
				}
			}

			return result;
		}

		public bool IsInterOfficeBillingOrgNotReportableForTax
		{
			get
			{
				bool isProxyOrgOrSameRelatedParty() => IsProxyOrg(GlbCompany.CurrentCompany) || IsSameRelatedParty(GlbCompany.CurrentCompany);
				return AreTaxRegDetailsSameAsLoginCompany || Factory.GetCachedValue(nameof(IsInterOfficeBillingOrgNotReportableForTax) + PK, isProxyOrgOrSameRelatedParty, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		bool AreTaxRegDetailsSameAsLoginCompany
		{
			get
			{
				bool result = false;
				if (!RawTaxRegistrationNumber.IsEmpty && !GlbCompany.CurrentCompany.GC_BusinessRegNo.IsEmpty)
				{
					result = new Regex(@"\s").Replace(RawTaxRegistrationNumber, string.Empty) ==
						new Regex(@"\s").Replace(GlbCompany.CurrentCompany.GC_BusinessRegNo, string.Empty);
				}

				return result;
			}
		}

		ZBool IsSameRelatedParty(GlbCompany company)
		{
			bool result = false;
			if (company != null && company.GC_IsActive)//if company is inactive, none of its branches should be active.
			{
				var relatedParty = AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.AccountingVATGSTGroup, ZString.Empty);
				if (relatedParty != null)
				{
					if (company.OrgProxy != null && company.OrgProxy.PK == relatedParty.PR_OH_RelatedParty)
					{
						result = true;
					}
					if (!result && GlbBranch.CurrentBranch.OrgProxy != null && GlbBranch.CurrentBranch.OrgProxy.PK == relatedParty.PR_OH_RelatedParty)
					{
						result = true;
					}
				}
			}
			return result;
		}

		public event EventHandler OnCheckIfNeedToEmptyRelatedCollections;

		protected void CheckIfNeedToEmptyRelatedCollections(ArrayList collections)
		{
			if (Globals.IsTest || OnCheckIfNeedToEmptyRelatedCollections == null)
			{
				//This means no GUI layer is attached, ie. we are in a ServiceTask or OperationalAction, bailing out positively as we can't get confirmation from user
				EmptyRelatedCollections = true;
				return;
			}

			bool allEmpty = true;
			foreach (IBusinessObjectCollection collection in collections)
			{
				allEmpty &= collection.Count == 0;
			}

			if (!allEmpty)
			{
				OnCheckIfNeedToEmptyRelatedCollections(this, EventArgs.Empty);
			}
			else
			{
				EmptyRelatedCollections = true;
			}
		}

		void CheckRegisteredChildObjects()
		{
			if (OH_IsDebtor)
			{
				CompanyData.RegisterEditableChildObject(CompanyData.InvoiceTypes);
				if (CompanyData.InvoiceRollupOrGroupsInitialised)
				{
					CompanyData.RegisterEditableChildObject(CompanyData.InvoiceRollupOrGroups);
				}
			}
			else
			{
				CompanyData.UnRegisterEditableChildObject(CompanyData.InvoiceTypes);
				if (CompanyData.InvoiceRollupOrGroupsInitialised)
				{
					CompanyData.UnRegisterEditableChildObject(CompanyData.InvoiceRollupOrGroups);
				}
			}

			if (OH_IsSalesLead)
			{
				RegisterEditableChildObject(SalesOpportunities);
			}
			else
			{
				ClearHasChangesOnUnRegisteredCollection(SalesCalls);
				ClearHasChangesOnUnRegisteredCollection(SalesOpportunities);
				UnRegisterEditableChildObject(SalesOpportunities);
			}

			if (OH_IsDebtor || OH_IsConsignee || OH_IsConsignor || OH_IsSalesLead)
			{
				CompanyData.RegisterEditableChildObject(CompanyData.RateTariffLevels);
			}
			else
			{
				CompanyData.UnRegisterEditableChildObject(CompanyData.RateTariffLevels);
			}
		}

		void ClearHasChangesOnUnRegisteredCollection(IEnumerable<BusinessObject> collection)
		{
			foreach (BusinessObject bizObject in collection)
			{
				bizObject.ClearHasChanges();
			}
		}

		protected void RemoveRelatedInvalidZGuidValues(ZString orgTypeDescriptor)
		{
			if (miscServ != null)
			{
				foreach (ZPropertyInfo info in MiscServ.ZPropertyInfoHash)
				{
					if (info.Name.IndexOf(orgTypeDescriptor) != -1 && ValueIsInvalidZGuid(info))
					{
						MiscServ[info.Name] = ZGuid.Empty;
					}
				}
			}
		}

		protected bool ValueIsInvalidZGuid(ZPropertyInfo info)
		{
			return MiscServ[info.Name].GetType() == typeof(ZGuid) && !((ZGuid)MiscServ[info.Name]).IsValid;
		}

		protected void SetCountryHasStateListForAddresses()
		{
			foreach (OrgAddress address in Addresses)
			{
				address.StateListHasMembersInfo.RefreshBinding();
				address.StateListDoesNotHaveMembersInfo.RefreshBinding();
			}
		}

		public event EventHandler OnSecurityAccessDenied;

		protected void SecurityAccessDenied()
		{
			if (OnSecurityAccessDenied != null)
			{
				OnSecurityAccessDenied(this, EventArgs.Empty);
			}
		}

		public LCMarginPercentages GetLCMarginPercentagesForFallBack()
		{
			LCMarginPercentages result = new LCMarginPercentages();
			result.LCMarginPercentage1 = MiscServ.OM_LandedCostMarginPercent1;
			result.LCMarginPercentage2 = MiscServ.OM_LandedCostMarginPercent2;
			result.LCMarginPercentage3 = MiscServ.OM_LandedCostMarginPercent3;
			return result;
		}

		#region IsBoundToOrganisationForm

		internal bool IsBoundToOrganisationForm
		{
			// Is the ShowMessage event hooked - if it is, then we are showing through a form.
			get
			{
				return isBoundToOrganisationFormSet ? isBoundToOrganisationForm : (ShowMessage != null);
			}
			set
			{
				isBoundToOrganisationForm = value;
				isBoundToOrganisationFormSet = true;
			}
		}

		bool isBoundToOrganisationForm;
		bool isBoundToOrganisationFormSet;

		#endregion

		#region IsSettlementGroup

		public bool IsSettlementGroup(ZString ledger)
		{
			string cacheKey = PK.ToString() + ledger + GlbCompany.CurrentCompany.PK;
			bool result = Factory.GetCachedValue(cacheKey, () => LoadSqlResult_ForIsSettlementGroup(ledger));
			return result;
		}

		bool LoadSqlResult_ForIsSettlementGroup(ZString ledger)
		{
			string sql = @"
SELECT TOP 1 NULL AS Value
FROM dbo.OrgRelatedParty 
INNER JOIN dbo.OrgHeader 
	ON OH_PK = PR_OH_Parent
INNER JOIN dbo.OrgCompanyData 
	ON OB_OH = OH_PK
WHERE PR_OH_RelatedParty = @OrgPK AND PR_GC = @CompanyPK
AND ((@Ledger = @EmptyString AND PR_PartyType IN (@ARS, @APS)) OR
	(@Ledger = @AR AND PR_PartyType = @ARS) OR
	(@Ledger = @AP AND PR_PartyType = @APS))
AND OH_IsActive = 1
AND OB_GC = @CompanyPK";

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@OrgPK", PK, OrgHeaderSchema.PK);
			parameters.Add("@CompanyPK", GlbCompany.CurrentCompany.PK, GlbCompanySchema.PK);
			parameters.Add("@Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@EmptyString", "", AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@AR", "AR", AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@AP", "AP", AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@ARS", "ARS", OrgRelatedPartySchema.PR_PartyType);
			parameters.Add("@APS", "APS", OrgRelatedPartySchema.PR_PartyType);
			collection.Load(sql, parameters);

			return collection.Count > 0;
		}

		#endregion

		#endregion

		#region Show Message Event

		public delegate void ShowMessageEventHandler(string caption, string message);

		public event ShowMessageEventHandler ShowMessage;

		public void OnShowMessage(string caption, string message)
		{
			if (ShowMessage != null)
			{
				ShowMessage(caption, message);
			}
		}

		#endregion

		#region Security

		public OrganisationSecurityProvider SecurityProvider
		{
			get
			{
				if (securityProvider == null)
				{
					securityProvider = GetNewSecurityProvider();
				}

				return securityProvider;
			}
		}

		OrganisationSecurityProvider securityProvider;

		protected virtual OrganisationSecurityProvider GetNewSecurityProvider()
		{
			return new OrganisationSecurityProvider(this);
		}

		#endregion

		#region ImporterBondQueryDate
		public ZDateTime ImporterBondQueryDate
		{
			get
			{
				var latestMessageDate = ZDateTime.Empty;
				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, this.PK);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, "TRX");
				query.AddToFilter(EDIMessageSchema.EM_MessageType, "KI");
				query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
				query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " desc";
				var importerBondQueryMessages = Factory.LoadTop1<Messaging.Integration.IEDIMessage>(query);
				if (importerBondQueryMessages != null)
				{
					latestMessageDate = importerBondQueryMessages.EM_SystemCreateTimeUtc;
				}
				return latestMessageDate;
			}
		}

		public ZPropertyInfo ImporterBondQueryDateInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(OrgHeader.Schema.ImporterBondQueryDate); }
		}
		#endregion

		#region Web Tracker

		public GlbBranch GetBranchForLogin()
		{
			var branchForLogin = GlbBranch.FindControllingBranchWithFallBackToAnyCompany(this);

			if (branchForLogin == null)
			{
				branchForLogin = GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, ClosestPort);

				if (branchForLogin == null && ClosestPort != null)
				{
					ICompany webCompany = null;

					if (Globals.IsWeb)
					{
						webCompany = EnvProxy.Instance.CurrentCompany;
					}
					else
					{
						var webBranch = Factory.Load<GlbBranch>(DataRegistry.Instance.WebBranch);
						if (webBranch != null)
						{
							webCompany = webBranch.Company;
						}
					}

					if ((webCompany != null && webCompany.Country.Code != ClosestPort.RL_RN_NKCountryCode) || webCompany == null)
					{
						branchForLogin = GlbBranch.FindAnyBranchInSameCountry(Factory, ClosestPort.Country);
					}
				}
			}

			return branchForLogin;
		}

		#endregion

		#region ISimilarOrganisationsFinder Members

		ISimilarOrganisationsFinder similarOrgFinder;
		public ISimilarOrganisationsFinder SimilarOrgFinder
		{
			get
			{
				if (similarOrgFinder == null)
				{
					similarOrgFinder = new SimilarOrganisationsFinder(this);
				}
				return similarOrgFinder;
			}
		}

		OrgPatternMatchCollection similarOrgMatches;

		/// <summary>
		/// Returns a collection of pattern match objects that match this Organisation.
		/// The collection is not automatically loaded - call FindSimilarOrganisations to populate it.
		/// </summary>
		public OrgPatternMatchCollection SimilarOrgMatches
		{
			get
			{
				if (similarOrgMatches == null)
				{
					similarOrgMatches = new OrgPatternMatchCollection(Factory);
					similarOrgMatches.SetReadOnlyIncludingChildren(true);
				}

				return similarOrgMatches;
			}
		}

		#region CodeLists

		[SuppressWeaklyTypedCollectionMessage]
		public IList OH_State_List
		{
			get { return MainAddress != null ? MainAddress.OA_State_List : null; }
		}

		#endregion

		#endregion

		#region IOrgHeader Members

		IOrgAddress IOrgHeader.CustomsAddress
		{
			get { return this.CustomsAddress; }
		}

		IOrgCompanyData IOrgHeader.CompanyData => CompanyData;

		ZString IOrgHeader.Phone_Formatted
		{
			get { return MainAddress.OA_Phone_Formatted; }
			set { MainAddress.OA_Phone_Formatted = value; }
		}

		ZString IOrgHeader.Mobile_Formatted
		{
			get { return MainAddress.OA_Mobile_Formatted; }
			set { MainAddress.OA_Mobile_Formatted = value; }
		}

		ZString IOrgHeader.Fax_Formatted
		{
			get { return MainAddress.OA_Fax_Formatted; }
			set { MainAddress.OA_Fax_Formatted = value; }
		}

		ZGuid IOrganisationData.PK
		{
			get { return this.PK; }
		}

		ZString IOrganisationData.Code
		{
			get { return OH_Code; }
			set { OH_Code = value; }
		}

		ZString IOrganisationData.FullName
		{
			get { return OH_FullNameTruncated; }
			set { OH_FullName = value; }
		}

		ZString IOrganisationData.Address1
		{
			get { return MainAddress.OA_Address1; }
			set { MainAddress.OA_Address1 = value; }
		}

		ZString IOrganisationData.Address2
		{
			get { return MainAddress.OA_Address2; }
			set { MainAddress.OA_Address2 = value; }
		}

		ZString IOrganisationData.City
		{
			get { return MainAddress.OA_City; }
			set { MainAddress.OA_City = value; }
		}

		ZString IOrganisationData.State
		{
			get { return MainAddress.OA_State; }
			set { MainAddress.OA_State = value; }
		}

		ZString IOrganisationData.Postcode
		{
			get { return MainAddress.OA_PostCode; }
			set { MainAddress.OA_PostCode = value; }
		}

		ZString IOrganisationData.UNLOCO
		{
			get { return OH_RL_NKClosestPort; }
			set { OH_RL_NKClosestPort = value; }
		}

		ZString IOrganisationData.Phone
		{
			get { return MainAddress.OA_Phone; }
			set { MainAddress.OA_Phone = value; }
		}

		ZString IOrganisationData.Mobile
		{
			get { return MainAddress.OA_Mobile; }
			set { MainAddress.OA_Mobile = value; }
		}

		ZString IOrganisationData.Fax
		{
			get { return MainAddress.OA_Fax; }
			set { MainAddress.OA_Fax = value; }
		}

		[EmailAddress]
		ZString IOrganisationData.Email
		{
			get { return MainAddress.OA_Email; }
			set { MainAddress.OA_Email = value; }
		}

		ZString IOrganisationData.Web
		{
			get { return MainWebURL != null ? MainWebURL.PU_URL : ZString.Empty; }
			set { MainWebURL.PU_URL = value; }
		}

		event EventHandler IOrgHeader.AddressChanged
		{
			add
			{
				OH_FullNameInfo.ValueChanged += value;
				OH_RL_NKClosestPortInfo.ValueChanged += value;

				subscribedAddress = MainAddress;
				if (subscribedAddress != null)
				{
					subscribedAddress.OA_Address1Info.ValueChanged += value;
					subscribedAddress.OA_Address2Info.ValueChanged += value;
					subscribedAddress.OA_PhoneInfo.ValueChanged += value;
					subscribedAddress.OA_MobileInfo.ValueChanged += value;
					subscribedAddress.OA_FaxInfo.ValueChanged += value;
					subscribedAddress.OA_EmailInfo.ValueChanged += value;
				}

				subscribedWebURL = MainWebURL;
				if (subscribedWebURL != null)
				{
					subscribedWebURL.PU_URLInfo.ValueChanged += value;
				}

				addressChangedSubscribed = true;
			}

			remove
			{
				if (addressChangedSubscribed)
				{
					OH_FullNameInfo.ValueChanged -= value;
					OH_RL_NKClosestPortInfo.ValueChanged -= value;

					if (subscribedAddress != null)
					{
						subscribedAddress.OA_Address1Info.ValueChanged -= value;
						subscribedAddress.OA_Address2Info.ValueChanged -= value;
						subscribedAddress.OA_PhoneInfo.ValueChanged -= value;
						subscribedAddress.OA_MobileInfo.ValueChanged -= value;
						subscribedAddress.OA_FaxInfo.ValueChanged -= value;
						subscribedAddress.OA_EmailInfo.ValueChanged -= value;
						subscribedAddress = null;
					}

					if (subscribedWebURL != null)
					{
						subscribedWebURL.PU_URLInfo.ValueChanged -= value;
						subscribedWebURL = null;
					}

					addressChangedSubscribed = false;
				}
			}
		}

		bool addressChangedSubscribed;
		OrgAddress subscribedAddress;
		OrgWebURL subscribedWebURL;

		#region Address List

		IList IOrgHeader.Address_List
		{
			get { return Address_List; }
		}

		public ZAddressList Address_List
		{
			get
			{
				ZAddressListHolder result = new ZAddressListHolder();
				result.Value = new ZAddressList(delegate
				{
					using (AddressesActive.SuspendEnumerationCheck())
					{
						foreach (OrgAddress addy in AddressesActive)
						{
							AddAddress(result.Value, addy, addy.AddressDescription);
						}
					}
				});
				return result.Value;
			}
		}

		class ZAddressListHolder
		{
			public ZAddressList Value { get; set; }
		}

		protected virtual void AddAddress(ZAddressList list, OrgAddress orgA, string addressDescription)
		{
			List<AddressCapabilityItem> capabilities = new List<AddressCapabilityItem>();
			foreach (OrgAddressCapabilityWrapper capability in orgA.AddressCapability)
			{
				if (capability.Enabled)
				{
					capabilities.Add(new AddressCapabilityItem() { Capability = capability.AddressCapabilityType, IsDefault = capability.Main });
				}
			}

			if (capabilities.Count > 0)
			{
				List<string> capabilityCodeList = capabilities.ConvertAll<string>(x => x.Capability);
				string capabilityString = string.Join(", ", capabilityCodeList.ToArray());
				list.AddAddress(orgA.PK, orgA.OA_Code, addressDescription + " (" + capabilityString + ")", capabilities.ToArray());
			}
		}

		#endregion

		#endregion

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Code
		{
			get { return OH_Code; }
		}

		string ICodeDescription.Description
		{
			get { return OH_FullName; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return OH_Code; }
		}

		#endregion

		#region IContactable Members

		string IContactBase.Name
		{
			get { return this.OH_FullName; }
		}

		[EmailAddress]
		string IContactBase.Email
		{
			get { return this.MainAddress.OA_Email; }
		}

		string IContactable.Mobile
		{
			get { return ((IOrgHeader)this).Mobile; }
		}

		bool IContactable.IsActive
		{
			get { return true; }
		}

		IContactable[] IContactable.GetNestedContacts(string parentContactDescription)
		{
			ArrayList result = new ArrayList();
			result.Add(GetNewMainContact(parentContactDescription));
			foreach (OrgContact contact in this.Contacts)
			{
				result.Add(contact);
			}

			return (IContactable[])result.ToArray(typeof(IContactable));
		}

		public MainContactableImpl GetNewMainContact(string parentContactDescription)
		{
			return new MainContactableImpl(this, parentContactDescription);
		}

		public class MainContactableImpl : IContactable
		{
			public MainContactableImpl(OrgHeader organisation, string organisationDescription)
			{
				this.Organisation = organisation;
				this.OrganisationDescription = organisationDescription;
			}

			public readonly OrgHeader Organisation;
			public readonly string OrganisationDescription;

			public ZGuid PK
			{
				get { return Organisation.PK; }
			}

			public string Name
			{
				get
				{
					string result = Res.GetString("fca718d4-c5db-48a5-81f3-7917874463b7", "Main Contact");
					return result;
				}
			}

			[EmailAddress]
			public string Email
			{
				get { return Organisation.MainAddress.OA_Email; }
			}

			public string Mobile
			{
				get { return Organisation.MainAddress.OA_Mobile; }
			}

			public bool IsActive
			{
				get { return Organisation.MainAddress.OA_IsActive; }
			}

			public IContactable[] GetNestedContacts(string parentContactDescription)
			{
				return Array.Empty<IContactable>();
			}
		}

		#endregion

		#region INoteSource Members

		public ZString NoteSourceName
		{
			get { return OH_FullName; }
		}

		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();
			result.AddRecipient(this);
			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return string.Empty; }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.Organizations; }
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return null; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return ObjectFactory.GetType<DocumentWrappers.IDocOrganisation>(); }
		}

		#endregion

		#region ICustomLabelsProvider / ICustomLabelsConfigOrgProvider Members

		public virtual CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
		{
			var result = new CustomLabelInfoList(typeof(OrgMiscServ), configOrg, ResString.GetMultilingualString("6553af15-7b9d-4acf-a8ef-87577a8819f9", "this organization"), factory);
			result.Add(Constants.CustomLabels.Organisation.CustomAttribute1, OrgMiscServ.Schema.OM_CustomAttrib1, Constants.CustomLabels.Descriptions.CustomAttribute(1));
			result.Add(Constants.CustomLabels.Organisation.CustomAttribute2, OrgMiscServ.Schema.OM_CustomAttrib2, Constants.CustomLabels.Descriptions.CustomAttribute(2));
			result.Add(Constants.CustomLabels.Organisation.CustomAttribute3, OrgMiscServ.Schema.OM_CustomAttrib3, Constants.CustomLabels.Descriptions.CustomAttribute(3));
			result.Add(Constants.CustomLabels.Organisation.CustomFlag1, OrgMiscServ.Schema.OM_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1));
			result.Add(Constants.CustomLabels.Organisation.CustomFlag2, OrgMiscServ.Schema.OM_CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2));
			result.Add(Constants.CustomLabels.Organisation.CustomFlag3, OrgMiscServ.Schema.OM_CustomFlag3, Constants.CustomLabels.Descriptions.CustomFlag(3));
			result.Add(Constants.CustomLabels.Organisation.CustomFlag4, OrgMiscServ.Schema.OM_CustomFlag4, Constants.CustomLabels.Descriptions.CustomFlag(4));
			result.Add(Constants.CustomLabels.Organisation.CustomDate1, OrgMiscServ.Schema.OM_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1));
			result.Add(Constants.CustomLabels.Organisation.CustomDate2, OrgMiscServ.Schema.OM_CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2));
			result.Add(Constants.CustomLabels.Organisation.CustomDate3, OrgMiscServ.Schema.OM_CustomDate3, Constants.CustomLabels.Descriptions.CustomDate(3));
			result.Add(Constants.CustomLabels.Organisation.CustomDecimal1, OrgMiscServ.Schema.OM_CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1));
			result.Add(Constants.CustomLabels.Organisation.CustomDecimal2, OrgMiscServ.Schema.OM_CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2));
			result.Add(Constants.CustomLabels.Organisation.CustomDecimal3, OrgMiscServ.Schema.OM_CustomDecimal3, Constants.CustomLabels.Descriptions.CustomNumber(3));
			return result;
		}

		public ICustomLabelsConfigOrgProvider ConfigOrgProvider
		{
			get { return this; }
		}

		public OrgHeader ConfigOrg
		{
			get { return this; }
		}

		public event EventHandler ConfigOrgChanged // wtf?
		{
			add { }
			remove { }
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			customBusinessObject = shouldRefresh ? null : customBusinessObject;
			return CustomBusinessObject;
		}

		CustomBusinessObject CustomBusinessObject
		{
			get
			{
				var customBusinessObjectProcessTaskTemplateLoader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), Factory);
				var matches = customBusinessObjectProcessTaskTemplateLoader.FindMatches(this);

				var templateKeys = matches.Matches.Select(t => t.Identifier).ToArray();

				if (lastCustomBizoTemplates == null || !templateKeys.SequenceEqual(lastCustomBizoTemplates))
				{
					customBusinessObject = null;
					lastCustomBizoTemplates = templateKeys;
				}

				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this);

					properties.Add(matches);

					customBusinessObject = new CustomBusinessObject(Factory, this, properties);

					if (!SecurityProvider.HasModifyDetailsCustomFieldsSecurity)
					{
						customBusinessObject.SetReadOnlyIncludingChildren(true);
					}
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;
		ZGuid[] lastCustomBizoTemplates;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new OrgHeaderDocManagerInfo(this, "ORG");

					if (SupplyChainSecurityConfiguration.IsEnabled)
					{
						HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(EDocsHavePotentiallyBeenUpdated);
					}
				}

				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		void EDocsHavePotentiallyBeenUpdated(object sender, EventArgs args)
		{
			if (isSavingFactory)
			{
				return;
			}

			if ((!SupplyChainSecurityConfiguration.IsAddressLevelScheme || Addresses.All(a => !a.LightValidationIsValid)) && CountryDataCollectionForThisCompany.All(c => !c.LightValidationIsValid))
			{
				return;
			}

			bool hasChanges = HasChanges;

			if (hasChanges != previousHasChanges)
			{
				if (hasChanges)
				{
					if (SupplyChainSecurityConfiguration.IsAddressLevelScheme)
					{
						foreach (OrgAddress address in Addresses)
						{
							address.KnownShipperDetails.MarkAsNeedingValidation();
						}
					}

					CountryDataCollectionForThisCompany.MarkAsNeedingValidation();
				}

				previousHasChanges = hasChanges;
			}
		}
		bool previousHasChanges;

		#endregion

		#region IDocumentSupportable Members

		OrgHeaderDocumentSupporter documentSupporter;

		public virtual DocumentSupporter DocumentSupporter
		{
			get
			{
				if (documentSupporter == null)
				{
					documentSupporter = new OrgHeaderDocumentSupporter(this);
				}

				return documentSupporter;
			}
		}

		#endregion

		#region IReadOnlySecurity Members

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			string propertyName = property.Name;
			if (propertyName == "FilterDescriptionLocalized")
			{
			}
			if (propertyName == OrgHeaderSchema.OH_Code.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyDetailsCodeSecurity;
			}
			else if (propertyName == OrgHeaderSchema.OH_FullName.Name ||
					 propertyName == OrgHeaderSchema.OH_RL_NKClosestPort.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyDetailsNameAndAddressSecurity;
			}
			else if (propertyName == OrgHeaderSchema.OH_Category.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyDetailsCategorySecurity;
			}
			else if (propertyName == OrgHeaderSchema.OH_Language.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyDetailsPhFaxWebDetailsSecurity;
			}
			else if (propertyName == OrgHeaderSchema.OH_IsConsignee.Name ||
						propertyName == OrgHeaderSchema.OH_IsConsignor.Name ||
						propertyName == OrgHeaderSchema.OH_IsTransportClient.Name ||
						propertyName == OrgHeaderSchema.OH_IsWarehouseClient.Name ||
						propertyName == OrgHeaderSchema.OH_IsShippingProvider.Name ||
						propertyName == OrgHeaderSchema.OH_IsForwarder.Name ||
						propertyName == OrgHeaderSchema.OH_IsBroker.Name ||
						propertyName == OrgHeaderSchema.OH_IsMiscFreightServices.Name ||
						propertyName == OrgHeaderSchema.OH_IsCompetitor.Name ||
						propertyName == OrgHeaderSchema.OH_IsSalesLead.Name ||
						propertyName == nameof(OH_IsCreditor) ||
						propertyName == nameof(OH_IsDebtor))
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyDetailsOrganisationTypeSecurity;
			}
			else if (propertyName == OrgHeaderSchema.OH_IsLocalTransport.Name ||
						propertyName == OrgHeaderSchema.OH_IsRailProvider.Name ||
						propertyName == OrgHeaderSchema.OH_IsLineHaulProvider.Name ||
						propertyName == OrgHeaderSchema.OH_IsInlandWaterwayProvider.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyCarrierSecurityLand;
			}
			else if (propertyName == OrgHeaderSchema.OH_IsPackDepot.Name ||
						propertyName == OrgHeaderSchema.OH_IsUnpackDepot.Name ||
						propertyName == OrgHeaderSchema.OH_IsAirCTO.Name ||
						propertyName == OrgHeaderSchema.OH_IsSeaCTO.Name ||
						propertyName == OrgHeaderSchema.OH_IsRoadFreightDepot.Name ||
						propertyName == OrgHeaderSchema.OH_IsContainerYard.Name ||
						propertyName == OrgHeaderSchema.OH_IsFumigationContractor.Name ||
						propertyName == OrgHeaderSchema.OH_IsVGMContractor.Name ||
						propertyName == OrgHeaderSchema.OH_IsRailHead.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyServicesSecurity;
			}
			else if (propertyName == OrgHeaderSchema.OH_IsUserFlag1.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag2.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag3.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag4.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag5.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag6.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag7.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag8.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag9.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag10.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag11.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag12.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag13.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag14.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag15.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag16.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag17.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag18.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag19.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag20.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag21.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag22.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag23.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag24.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag25.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag26.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag27.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag28.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag29.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag30.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag31.Name ||
						propertyName == OrgHeaderSchema.OH_IsUserFlag32.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifySalesClientSummarySecurity;
			}
			else if (propertyName == Schema.OpportunitiesDateTypeToFilter ||
						propertyName == Schema.OpportunityDateFrom ||
						propertyName == Schema.OpportunityDateTo ||
						propertyName == Schema.DateOfCallFrom ||
						propertyName == Schema.DateOfCallTo ||
						propertyName == Schema.DateNextCallFrom ||
						propertyName == Schema.DateNextCallTo ||
						propertyName == Schema.CallContact ||
						propertyName == Schema.CallSalesRep ||
						propertyName == Schema.CallDirection ||
						propertyName == Schema.CallLocation)
			{
				shouldBeReadOnly = false; // These fields are filters on the Sales section - allow users to filter.
			}
			else if (propertyName == OrgHeaderSchema.OH_IsTempAccount.Name)
			{
				shouldBeReadOnly = IsInDatabase ? !SecurityProvider.HasModifyDetailsIsTemporaryOrgSecurity : !SecurityProvider.HasNewDetailsIsTemporaryOrgSecurity;
			}
			else if (propertyName == OrgHeaderSchema.OH_IsGlobalAccount.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyDetailsIsGlobalOrgSecurity;
			}
			else if (propertyName == OrgHeaderSchema.OH_IsNationalAccount.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyDetailsIsNationalOrgSecurity;
			}
			else if (propertyName == Schema.ARSettlementGroupPK)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyReceivablesSettlementGroupSecurity && CompanyData.CreditDetails_ReadOnly;
			}
			else if (propertyName == Schema.APSettlementGroupPK)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyPayablesDefaultsSecurity;
			}
			else if (propertyName == Schema.DateOfCallNoteFrom ||
						propertyName == Schema.DateOfCallNoteTo ||
						propertyName == Schema.DateFollowUpFrom ||
						propertyName == Schema.DateFollowUpTo ||
						propertyName == Schema.CallNoteContact ||
						propertyName == Schema.CallingStaff ||
						propertyName == Schema.CollectionCallStatus ||
						propertyName == Schema.CollectionCallDisposition)
			{
				shouldBeReadOnly = !(SecurityProvider.HasModifyDetailsSecurity || Env.Security.ReceivablesCollectionCallsEdit.IsAllowed);
			}
			else if (propertyName == OrgHeaderSchema.OH_IsAirLine.Name ||
						propertyName == OrgHeaderSchema.OH_IsAirWholesaler.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyCarrierSecurityAir;
			}
			else if (propertyName == OrgHeaderSchema.OH_IsShippingLine.Name ||
						propertyName == OrgHeaderSchema.OH_IsSeaWholesaler.Name ||
						propertyName == OrgHeaderSchema.OH_IsShippingConsortium.Name ||
						propertyName == OrgHeaderSchema.OH_RSL_ShippingLine.Name)
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyCarrierSecuritySea
					|| (propertyName == OrgHeaderSchema.OH_IsShippingLine.Name || propertyName == OrgHeaderSchema.OH_IsSeaWholesaler.Name) && ShippingLine != null && (OH_IsSeaWholesaler || OH_IsShippingLine);
			}
			else if (propertyName.StartsWith("MiscServ+"))
			{
				shouldBeReadOnly = false;
			}
			else
			{
				shouldBeReadOnly = !SecurityProvider.HasModifyDetailsSecurity;
			}

			return shouldBeReadOnly || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region IAddressDetails Members

		ZString IAddressDetails.CompanyName
		{
			get { return OH_FullName; }
		}

		ZString IAddressDetails.ContactName
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).ContactName : ZString.Empty; }
		}

		ZString IAddressDetails.Phone
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).Phone : ZString.Empty; }
		}

		ZString IAddressDetails.Fax
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).Fax : ZString.Empty; }
		}

		ZString IAddressDetails.Email
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).Email : ZString.Empty; }
		}

		ZString IAddressDetails.AddressLine1
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).AddressLine1 : ZString.Empty; }
		}

		ZString IAddressDetails.AddressLine2
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).AddressLine2 : ZString.Empty; }
		}

		ZString IAddressDetails.City
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).City : ZString.Empty; }
		}

		ZString IAddressDetails.State
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).State : ZString.Empty; }
		}

		ZString IAddressDetails.PostCode
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).PostCode : ZString.Empty; }
		}

		ZString IAddressDetails.Country
		{
			get { return MainAddress != null ? ((IAddressDetails)MainAddress).Country : OH_RL_NKClosestPort.Left(2); }
		}

		#endregion

		#region IHaveRequiredDocuments Members

		public ZString UniqueConsignRef
		{
			get { return OH_Code; }
		}

		public ZString HouseBill
		{
			get { return null; }
		}

		public ZString MasterBill
		{
			get { return null; }
		}

		public OrgHeader ExportBroker
		{
			get { return null; }
		}

		public ZString TableCode
		{
			get { return OrgHeaderSchema.Constants.Prefix; }
		}

		public IReadOnlyList<ZString> AdditionalRefTypes
		{
			get { return new ZString[] { Constants.ReferenceTypes.SupplyChainLogistics }; }
		}

		public void PreLogAllDocumentsReceivedEvents()
		{
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					requiredDocuments.Load();
					RegisterEditableChildObject(requiredDocuments);
				}

				return requiredDocuments;
			}
		}

		JobRequiredDocumentDependentCollection requiredDocuments;

		public BusinessObject UltimateDocumentParent
		{
			get { return this; }
		}

		#endregion

		#region IDeniedPartyScreeningPartyProvider Members

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties
		{
			get { return new ScreeningParty[] { new ScreeningParty(this, Res.GetString("0aa39a2f-7300-480c-868c-4d6957d36de4", "This organization"), this) }; }
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			return this.OH_ScreeningStatus;
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			return (this as IScreeningPartyProvider).GetWorstScreeningStatus();
		}

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get { return OH_ScreeningStatus; }
			set { OH_ScreeningStatus = value; }
		}

		ZBool IDpsEntityProvider.ShouldUpdateRelatedJobs
		{
			get { return ShouldUpdateRelatedJobs; }
			set { ShouldUpdateRelatedJobs = value; }
		}

		ZString IDpsEntityProvider.OriginalScreeningStatus => OH_ScreeningStatusInfo.OriginalValue.ToString();

		ZBool IDpsEntityProvider.NeedsScreening => OH_ScreeningStatus != ScreeningStatusesList.Codes.NotScreened && OH_ScreeningStatus != ScreeningStatusesList.Codes.Unknown;

		public void InvalidateByLocalDataChanges() => ScreeningLogCollection.InvalidateByLocalDataChanges();

		#endregion

		#region IAutoRateDateByChargeGroupConfiguration

		ZString IAutoRateDateByChargeGroupConfiguration.FilterType => MiscServ.OM_AutoratingDateFiltering;

		IEnumerable<IAutoRateDate> IAutoRateDateByChargeGroupConfiguration.GetAutoRateDates(string chargeGroup)
		{
			var query = new ZQuery(RatingDateConfigSchema.RDT_ParentTableCode, OrgHeaderSchema.Constants.Prefix);
			query.AddToFilter(RatingDateConfigSchema.RDT_ParentID, PK);
			query.AddToFilter(RatingDateConfigSchema.RDT_ChargeGroup, chargeGroup);

			var ratingDateConfigs = Factory.Load<RatingDateConfig>(query);

			return ratingDateConfigs;
		}

		#endregion

		#region IOrgHeaderForMatching Members

		IEnumerable<IMatchingAddress> IMatchingOrganisationCollections.Addresses
		{
			get { return new List<IMatchingAddress>(AddressesNoAutoCreate.Cast<OrgAddress>()); }
		}

		List<IOrgCusCodeForMatching> IOrgHeaderForMatching.CustomsCodes
		{
			get { return new List<IOrgCusCodeForMatching>(CustomsCodes.Cast<OrgCusCode>()); }
		}

		IEnumerable<IMatchingCusCode> IMatchingOrganisationCollections.CustomsCodes
		{
			get { return new List<IMatchingCusCode>(CustomsCodes.Cast<OrgCusCode>()); }
		}

		IMatchingAddress IOrgHeaderForMatching.MainAddress
		{
			get { return MainAddress; }
		}

		MultilingualString IMatchingOrganisation.CountryName
		{
			get { return MainAddress != null ? MainAddress.CountryName : (NoResString)string.Empty; }
		}

		ZString IMatchingOrganisation.PortName
		{
			get { return MainAddress != null ? MainAddress.PortName : ZString.Empty; }
		}

		ZBool IOrgHeaderForMatching.HasAddress(IMatchingAddress adr)
		{
			OrgAddress address = adr as OrgAddress;
			if (address != null)
			{
				return AddressesNoAutoCreate.Contains(address);
			}
			return false;
		}

		ZBool IMatchingOrganisation.OH_RL_NKClosestPortInfoHasChanges
		{
			get { return OH_RL_NKClosestPortInfo.HasChanges; }
		}

		void IOrgHeaderForMatching.ResetHasChanges()
		{
			HasChanges = false;
			MiscServ.HasChanges = false;
			Addresses.HasChanges = false;
		}

		IMatchingAddress IOrgHeaderForMatching.AddNewAddress()
		{
			return Addresses.AddNew();
		}

		void IOrgHeaderForMatching.OnAfterImport()
		{
		}

		#endregion

		#region ISalesAssociatedEntity
		ZString ISalesValueAssociatedEntity.ID => OH_Code;
		ZString ISalesValueAssociatedEntity.EntityType => "ORG";
		ControllerID ISalesValueAssociatedEntity.ControllerID => ControllerIDs.Organisation;
		ZGuid? ISalesValueAssociatedEntity.CompanyPk => null;
		ZString ISalesValueAssociatedEntity.CompanyCode => ZString.Empty;
		ZPropertyInfo ISalesValueAssociatedEntity.OrgPkInfo => null;
		ZString ISalesValueAssociatedEntity.Summary => OH_Code;
		ZString ISalesValueAssociatedEntity.ValueCurrency => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		public ZDateTime DateForExchangeRate => ZDateTime.Now;

		ZDateTime ISalesValueAssociatedEntity.GetDateAssociatedToSalesValue(ISalesValue salesValue)
		{
			return salesValue.SystemCreateTimeUtc.ToLocalBranchTime();
		}

		ZString ISalesValueAssociatedEntity.GetUserThatAssociatedToSalesValue(ISalesValue salesValue)
		{
			return salesValue.SystemCreateUser;
		}

		public ISalesHeaderCollection ActualAndProspectiveSalesHeaderCollection
		{
			get
			{
				if (salesHeaderCollection == null)
				{
					salesHeaderCollection = ObjectFactory.Get<ISalesHeaderCollectionBuilder>().New(this, true);
				}
				return salesHeaderCollection;
			}
		}
		ISalesHeaderCollection salesHeaderCollection;

		public ISalesHeaderCollection ProspectiveSalesHeaderCollection
		{
			get
			{
				if (prospectiveSalesHeaderCollection == null)
				{
					prospectiveSalesHeaderCollection = ObjectFactory.Get<ISalesHeaderCollectionBuilder>().New(this, false);
				}
				return prospectiveSalesHeaderCollection;
			}
		}
		ISalesHeaderCollection prospectiveSalesHeaderCollection;

		#endregion

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new OrgHeaderProcessTasksCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public virtual IEnumerable<IWorkflowProvider> RelatedIWorkflowProviders
		{
			get
			{
				var result = new List<IWorkflowProvider>();
				result.AddRange(SalesOpportunities.OfType<IWorkflowProvider>());

				if (!OH_IsSalesLead && IsRegisteredEditableChildObject(SalesOpportunities))
				{
					ClearHasChangesOnUnRegisteredCollection(SalesOpportunities);
					UnRegisterEditableChildObject(SalesOpportunities);
				}

				return result;
			}
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZGuid IWorkflowProviderCore.PK
		{
			get { return PK; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return new OrgHeaderWorkflowDescriptor().Code; }
		}

		#endregion

		#region ICustomPropertyContainer

		IEnumerable<ICustomProperty> ICustomPropertyContainer.CustomProperties
		{
			get
			{
				var customProperties = new List<ICustomProperty>();
				customProperties.AddRange(GetStaffAssignmentsCustomProperties());
				customProperties.AddRange(GetCompetitorTypesCustomProperties());
				return customProperties;
			}
		}

		IEnumerable<ICustomProperty> GetStaffAssignmentsCustomProperties()
		{
			var properties = new CustomPropertyCollectionImpl(s =>
			{
				var staffAssignment = StaffAssignments.Cast<OrgStaffAssignments>().FirstOrDefault(sa => GetStaffAssignmentRoleCodeDescription(sa.RoleDescription) == s);
				if (staffAssignment != null)
				{
					var personResponsible = staffAssignment.PersonResponsible;
					return personResponsible == null ? ZString.Empty : personResponsible.GS_Code;
				}
				else
				{
					return ZString.Empty;
				}
			}, (s, v) => false);
			PopulatePropertiesForStaffAssignmentRoles(properties);
			return properties;
		}

		IEnumerable<ICustomProperty> GetCompetitorTypesCustomProperties()
		{
			var properties = new CustomPropertyCollectionImpl(s =>
			{
				var competitors = Competitors.Where(c => GetCompetitorTypeCodeDescription(c.Description) == s);
				var competitorCodes = string.Join(", ", competitors.Select(c => c.Competitor.OH_Code).OrderBy(c => c));
				return competitorCodes;
			}, (s, v) => false);
			PopulatePropertiesForCompetitorTypes(properties);
			return properties;
		}

		public static void PopulateProperties(CustomPropertyCollection properties)
		{
			PopulatePropertiesForStaffAssignmentRoles(properties);
			PopulatePropertiesForCompetitorTypes(properties);
		}

		static void PopulatePropertiesForStaffAssignmentRoles(CustomPropertyCollection properties)
		{
			foreach (CodeDescriptionPair codeDescriptionPair in Env.Registry.OrgStaffMemberAssignmentRoles)
			{
				if (!string.IsNullOrEmpty(codeDescriptionPair.Description))
				{
					properties.Add(typeof(ZString), GetStaffAssignmentRoleCodeDescription(codeDescriptionPair.Description));
				}
			}
		}

		static ZString GetStaffAssignmentRoleCodeDescription(string description)
		{
			return Res.GetString("9d912696-b903-44c3-b735-3ec742c9e4af", "Staff - {0}", description);
		}

		static void PopulatePropertiesForCompetitorTypes(CustomPropertyCollection properties)
		{
			foreach (CodeDescriptionPair codeDescriptionPair in OrganisationsDataRegistry.Instance.CompetitorType.Value.GetActiveCodeDescriptionPairList())
			{
				if (!string.IsNullOrEmpty(codeDescriptionPair.Description))
				{
					properties.Add(typeof(ZString), GetCompetitorTypeCodeDescription(codeDescriptionPair.Description));
				}
			}
		}

		static ZString GetCompetitorTypeCodeDescription(string description)
		{
			return Res.GetString("b32499fa-b76e-48c8-b826-aed7aa306ff7", "Competitor on {0}", description);
		}

		#endregion

		#region ICusAddInfoTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusAddInfoTypeSupporterFetchStrategy>(), this);
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusCodeDataTypeSupporterFetchStrategy>(), this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.AUREG, ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICLREGInfoProvider>());
			result.Add(CusAddInfoTypeAttribute.Codes.CACSAMessage, ObjectFactory.GetType<CA.ITradeChainPartner>());
			return result;
		}

		public static class CusAddInfoTypeAttribute
		{
			public static class Codes
			{
				public const string AUREG = "REG";
				public const string CACSAMessage = "CSM";
			}
		}

		#endregion

		#region ASN Refresh Defaults

		public IEnumerable<ZString> GetASNRefreshOptions(Guid companyPK)
		{
			var result = new List<ZString>();

			var companyData = CompanyData;
			if (companyData != null && companyData.ImporterOverride)
			{
				result.AddRange(companyData.DefaultOptions.Cast<IMProductValueDefaultOption>().Select(x => x.FieldType));
			}
			else
			{
				var refreshOptions = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().ASNRefreshOptionsFieldTypes(companyPK);
				if (refreshOptions.Count > 0)
				{
					result.AddRange(refreshOptions);
				}
			}
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.FreightPercentage, ObjectFactory.GetType<CA.IFreightPercentage>());
			result.Add(CusCodeDataTypeList.Codes.AdditionalIdentification, ObjectFactory.GetType<BR.IAdditionalIdentification>());
			result.Add(CusCodeDataTypeList.Codes.SafeFoodLicense, ObjectFactory.GetType<CA.ISafeFoodLicense>());
			return result;
		}

		public static class CusCodeDataTypeList
		{
			public static class Codes
			{
				public const string FreightPercentage = "FP";
				public const string AdditionalIdentification = "AID";
				public const string SafeFoodLicense = "SFL";
			}
		}

		#endregion

		#region EInvoicingTemplateConfigurations

		public AccTemplateFileStorage GetValidTemplateFileForEInvoice(ZString jobType, ZString transportMode)
		{
			if (EInvoicingTemplateConfigurations.Any())
			{
				var ranker = new StringColumnValueRanker();
				ranker.Add(nameof(AccEInvoicingTemplateFileView.ETF_TemplateConfigLevel),
					new IZType[]
					{
						(ZString)nameof(AccEInvoicingTemplateFileLevelEnum.Organisation),
						(ZString)nameof(AccEInvoicingTemplateFileLevelEnum.Company)
					});
				ranker.Add(nameof(AccEInvoicingTemplateFileView.ETF_JobType), new IZType[] { jobType, (ZString)"ALL" });
				ranker.Add(nameof(AccEInvoicingTemplateFileView.ETF_TransportMode), new IZType[] { transportMode, (ZString)"ALL" });
				var templateConfiguration = ranker.GetBestMatch(EInvoicingTemplateConfigurations)?.FirstOrDefault();

				if (templateConfiguration != null)
				{
					return CompanyData
						.Company
						.TemplateFiles
						.OfType<AccTemplateFileStorage>().FirstOrDefault(x => x.TFS_Code == templateConfiguration.ETF_TemplateCode);
				}
			}
			return null;
		}

		IEnumerable<AccEInvoicingTemplateFileView> EInvoicingTemplateConfigurations => CompanyData.EInvoicingTemplateFileConfigurations.OfType<AccEInvoicingTemplateFileView>();

		#endregion

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZString GetAdditionalCompanyName(OrgAddressType addressType)
		{
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var currentLanguage = GlbCompany.CurrentCompany.OrgProxy?.OH_Language ?? GlbBranch.CurrentBranch.OrgProxy?.OH_Language ?? ZString.Empty;

			var capabilityAddresses = Addresses.Cast<OrgAddress>().Where(x =>
				x.AddressCapability.GetCapabilityEnabled(addressType) && x.OA_RN_NKCountryCode == currentCountry);

			var mainAddresses = capabilityAddresses.Where(x => x.AddressCapability.GetIsMainAddress(addressType));

			string result = mainAddresses.FirstOrDefault(x => x.OA_Language == currentLanguage)?.CompanyName;

			if (string.IsNullOrEmpty(result))
			{
				result = mainAddresses.SelectMany(x => x.TranslatedAddresses).FirstOrDefault(x => x.OTA_Language == currentLanguage)?.CompanyName;

				if (string.IsNullOrEmpty(result))
				{
					var nonMainAddresses = capabilityAddresses.Where(x => !x.AddressCapability.GetIsMainAddress(addressType));

					result = nonMainAddresses.FirstOrDefault(x => x.OA_Language == currentLanguage)?.CompanyName;

					if (string.IsNullOrEmpty(result))
					{
						result = nonMainAddresses.SelectMany(x => x.TranslatedAddresses).FirstOrDefault(x => x.OTA_Language == currentLanguage)?.CompanyName;
					}
				}
			}

			return result ?? string.Empty;
		}

		public ZString CAAccountSecurityNumber
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.Canada)
				{
					var impAddInfo = CountryData.ImpAddInfo as CA.IOrgImpAddInfo;
					return impAddInfo?.CAAccountSecurityNumber ?? ZString.Empty;
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo CAAccountSecurityNumberInfo
		{
			get { return GetZPropertyInfo(OrgHeader.Schema.CAAccountSecurityNumber); }
		}

		#region AUIsDutyDeferred

		public ZBool AUIsDutyDeferred
		{
			get => AUOrgImpAddInfo?.IsDutyDeferred ?? false;
			set
			{
				var ayOrgImpAddInfo = AUOrgImpAddInfo;
				if (ayOrgImpAddInfo != null)
				{
					ayOrgImpAddInfo.IsDutyDeferred = value;
				}
			}
		}

		Enterprise.Integration.Customs.AU.IOrgImpAddInfo AUOrgImpAddInfo
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
				{
					return (Enterprise.Integration.Customs.AU.IOrgImpAddInfo)CountryData.ImpAddInfo;
				}
				return null;
			}
		}

		#endregion

		public ZString ExportersBankName
		{
			get { return GetNoteValue(PredefinedNoteTypes.Instance.ExportersBankName.Description); }
			set
			{
				SetNoteValue(PredefinedNoteTypes.Instance.ExportersBankName.Description, value);
				HasChanges = true;
				ExportersBankNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExportersBankNameInfo
		{
			get { return GetZPropertyInfo(OrgHeader.Schema.ExportersBankName); }
		}

		ZString GetNoteValue(ZString description)
		{
			var note = Notes.FindByDescription(description).FirstOrDefault();
			if (note == null)
			{
				return ZString.Empty;
			}
			else
			{
				return note.ST_NoteDataAsText;
			}
		}

		void SetNoteValue(ZString description, ZString value)
		{
			var note = Notes.FindByDescription(description).FirstOrDefault();
			if (value.IsEmpty)
			{
				if (note != null)
				{
					note.Delete();
				}
			}
			else
			{
				if (note == null)
				{
					note = Notes.AddNew(false, description, ZString.Empty);
				}
				note.ST_NoteDataAsText = value;
			}
		}

		public ZString ExportersBankAccount
		{
			get { return GetNoteValue(PredefinedNoteTypes.Instance.ExportersBankAccountNo.Description); }
			set
			{
				SetNoteValue(PredefinedNoteTypes.Instance.ExportersBankAccountNo.Description, value);
				HasChanges = true;
				ExportersBankAccountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExportersBankAccountInfo
		{
			get { return GetZPropertyInfo(OrgHeader.Schema.ExportersBankAccount); }
		}

		public ZString ExportersSwiftCode
		{
			get { return GetNoteValue(PredefinedNoteTypes.Instance.ExportersBankSWIFTCode.Description); }
			set
			{
				SetNoteValue(PredefinedNoteTypes.Instance.ExportersBankSWIFTCode.Description, value);
				HasChanges = true;
				ExportersSwiftCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExportersSwiftCodeInfo
		{
			get { return GetZPropertyInfo(OrgHeader.Schema.ExportersSwiftCode); }
		}

		public ZString MethodOfPayment
		{
			get { return GetNoteValue(PredefinedNoteTypes.Instance.MethodOfPayment.Description); }
			set
			{
				SetNoteValue(PredefinedNoteTypes.Instance.MethodOfPayment.Description, value);
				HasChanges = true;
				MethodOfPaymentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MethodOfPaymentInfo
		{
			get { return GetZPropertyInfo(OrgHeader.Schema.MethodOfPayment); }
		}

		public ZString AdditionalInformation
		{
			get { return GetNoteValue(PredefinedNoteTypes.Instance.AdditionalInformation.Description); }
			set
			{
				SetNoteValue(PredefinedNoteTypes.Instance.AdditionalInformation.Description, value);
				HasChanges = true;
				AdditionalInformationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AdditionalInformationInfo
		{
			get { return GetZPropertyInfo(OrgHeader.Schema.AdditionalInformation); }
		}

		public ZString EmployerIdentificationNumber
		{
			get
			{
				var customsCode = CustomsCodes.Cast<OrgCusCode>()
					.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
				return customsCode != null ? customsCode.OK_CustomsRegNo : ZString.Empty;
			}
		}

		public ZPropertyInfo EmployerIdentificationNumberInfo
		{
			get { return GetZPropertyInfo(OrgHeader.Schema.EmployerIdentificationNumber); }
		}

		public ZDateTime PowerOfAttorneyValidToDate
		{
			get
			{
				var document = RequiredDocuments.Cast<JobRequiredDocument>()
					.Where(x => x.EQ_DocType == Constants.RefDocTypes.PowerOfAttorney)
					.OrderBy(x => x.EQ_ValidToDate)
					.FirstOrDefault();
				return document != null ? document.EQ_ValidToDate : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo PowerOfAttorneyValidToDateInfo
		{
			get { return GetZPropertyInfo(OrgHeader.Schema.PowerOfAttorneyValidToDate); }
		}

		public bool ValidationSuspendedForOrgHeader { get; set; }

		#region IConversationProvider

		ZString IConversationParticipant.Language => OH_Language;
		ZString IConversationParticipant.Email => MainAddress?.OA_Email ?? ZString.Empty;
		ZString IConversationParticipant.Code => OH_Code;
		ZString IConversationParticipant.Name => OH_FullName;
		ZString IConversationParticipant.OrganisationName => OH_FullName;
		ZString IConversationParticipant.Location => OH_RL_NKClosestPort;
		ZString IConversationParticipant.JobTitle => ZString.Empty;
		ZBool IConversationParticipant.IsActive => OH_IsActive;
		ZBool IConversationParticipant.IsInternal => false;
		ZString IConversationParticipant.DisplayText => NameAndCode;

		void IConversationParticipant.CheckCanParticipate(INotifications notifications)
		{
		}

		#endregion

		#region IAuditParent Members

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(OrgAddressSchema.OA_OH, OrgAddressSchema.OA_Code);
				yield return new AuditChildInfo(OrgBrandOrRelatedNameSchema.P1_OH, OrgBrandOrRelatedNameSchema.P1_RelatedName);
				yield return new AuditChildInfo(OrgContactSchema.OC_OH, OrgContactSchema.OC_ContactName);
				yield return new AuditChildInfo(OrgCusCodeSchema.OK_OH, null);
				yield return new AuditChildInfo(OrgMiscServSchema.OM_OH, null);
				yield return new AuditChildInfo(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, null);
				yield return new AuditChildInfo(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, null);
			}
		}

		#endregion

		#region SupplyChainSecurityConfiguration

		public ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfiguration()); }
		}
		ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#endregion

		public bool TSAKnownAddressChanged { set; get; }
		public bool MIDAddressChanged { get; set; }

		public List<ISupportDuplicationFinder> GetSupportedDuplicationFinders(Type targetType, DeduplicationProxyConfig config)
		{
			return ObjectFactory.Get<IMasterDataProvider>().GetOrganisationSupportedDuplicationFinders(this, targetType, config);
		}

		IPatternMatchingRegenerator<OrgHeader>[] IPatternMatchingRegenerationEntities<OrgHeader>.RegenerationEntities(PatternMatchingRecalculator<OrgHeader> recalculator)
		{
			return ObjectFactory.Get<IMasterDataProvider>().GetOrganisationPatternMatchingRegenerationEntities(recalculator);
		}

		#region Associated Fields

		public List<ZGuid> AssociatedDeclarations => GetAssociatedDeclarations(PK);

		public List<ZGuid> AssociatedConsols => GetAssociatedConsols(PK);

		public List<ZGuid> AssociatedShipments => GetAssociatedShipments(PK);

		public static List<ZGuid> GetAssociatedDeclarations(ZGuid orgPK) => GetAssociatedJobs("DeclarationsAssociatedWithOrgOrDocAddress", orgPK);

		public static List<ZGuid> GetAssociatedConsols(ZGuid orgPK) => GetAssociatedJobs("ConsolsAssociatedWithOrgOrDocAddress", orgPK);

		public static List<ZGuid> GetAssociatedShipments(ZGuid orgPK) => GetAssociatedJobs("ShipmentsAssociatedWithOrgOrDocAddress", orgPK);

		static List<ZGuid> GetAssociatedJobs(string functionName, ZGuid orgPK)
		{
			var list = new List<ZGuid>();

			if (orgPK.IsValid)
			{
				var query = functionName == "ShipmentsAssociatedWithOrgOrDocAddress" ? string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0}(@entityPK, @companyPK, default)", functionName) : string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0}(@entityPK, @companyPK)", functionName);
				var cmd = CargoWise.Data.Db.Connection.Command(query);
				cmd.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, orgPK.ToGuid());
				cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						list.Add(new ZGuid(reader[0]));
					}
				}
			}

			return list;
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				yield return new OrgHeaderUniqueIndexFailureHandler();
			}
		}

		class OrgHeaderUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public OrgHeaderUniqueIndexFailureHandler()
			{
			}

			#region IUniqueIndexFailureHandler Members

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportInformation(
					Res.GetString("7149D27B-7E4A-4B17-A4B7-37F95EFFD414", "Organization code is duplicated and will be regenerated, please attempt to save again."),
					Res.GetString("1B615B31-E8AF-4D4A-8421-9BE6D6709EC1", "Organization Update Required"));
			}

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return OrgHeaderSchema.Constants.Indexes.NR_UX__OH_Code; }
			}

			#endregion
		}

		#endregion

		public bool IsLocalClosestPort
		{
			get
			{
				var closestPort = ClosestPort;
				return closestPort != null
					&& closestPort.Country != null
					&& closestPort.RL_RN_NKCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
		}

		public bool IsLocalCountry
		{
			get
			{
				var country = Country;
				return country != null
					&& country.Code == GlbCompany.CurrentCompany.Country.Code;
			}
		}

		#region OrgAirlineBranchAccounts

		[ChildEditable(true)]
		public OrgAirlineBranchAccountCollection OrgAirlineBranchAccounts
		{
			get
			{
				if (orgAirlineBranchAccounts == null)
				{
					orgAirlineBranchAccounts = new OrgAirlineBranchAccountCollection(this);
					RegisterEditableChildObject(orgAirlineBranchAccounts);
					SetOrgAirlineBranchAccountsEditableState();
				}

				return orgAirlineBranchAccounts;
			}
		}
		OrgAirlineBranchAccountCollection orgAirlineBranchAccounts;

		void SetOrgAirlineBranchAccountsEditableState()
		{
			if (!IsDeleted && !IsDeleting)
			{
				var editable = OH_IsAirLine || OH_IsAirWholesaler;
				if (!editable)
				{
					OrgAirlineBranchAccounts.DeleteAll();
				}

				var hasPermission = SecurityProvider.HasModifyCarrierSecurityAir;

				OrgAirlineBranchAccounts.SetReadOnlyIncludingChildren(!editable || !hasPermission);
			}
		}

		#endregion

		#region IEInvoicingEligibilityLiteOrgHeader

		ZString IEInvoicingEligibilityLiteOrgHeader.Category => OH_Category;

		IReadOnlyCollection<IEInvoicingEligibilityLiteRegistrationCode> IEInvoicingEligibilityLiteOrgHeader.RegistrationCodes
			=> CustomsCodes?.Cast<IEInvoicingEligibilityLiteRegistrationCode>()?.ToArray() ?? Array.Empty<IEInvoicingEligibilityLiteRegistrationCode>();

		IEInvoicingEligibilityLiteOrgAddress IEInvoicingEligibilityLiteOrgHeader.MainAddress
			=> (IEInvoicingEligibilityLiteOrgAddress)MainAddress ?? OrgAddress.EmptyIEInvoicingEligibilityLiteOrgAddress.Value;

		[WTG.StaticAnalysis.Annotation.Immutable]
		internal class EmptyIEInvoicingEligibilityLiteOrgHeader : IEInvoicingEligibilityLiteOrgHeader
		{
			public static readonly EmptyIEInvoicingEligibilityLiteOrgHeader Value = new EmptyIEInvoicingEligibilityLiteOrgHeader();

			ZString IEInvoicingEligibilityLiteOrgHeader.Category => ZString.Empty;

			IReadOnlyCollection<IEInvoicingEligibilityLiteRegistrationCode> IEInvoicingEligibilityLiteOrgHeader.RegistrationCodes
				=> Array.Empty<IEInvoicingEligibilityLiteRegistrationCode>();

			IEInvoicingEligibilityLiteOrgAddress IEInvoicingEligibilityLiteOrgHeader.MainAddress
				=> OrgAddress.EmptyIEInvoicingEligibilityLiteOrgAddress.Value;
		}

		#endregion

		#region IDocAddresses Members

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		public SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.NotifyParty:
					return NotifyPartyDocAddressRequirement;
				case DocAddressType.MasterBillShipperOverride:
					return MasterBillShipperOverrideDocAddressRequirement;
				case DocAddressType.MasterBillConsigneeOverride:
					return MasterBillConsigneeOverrideDocAddressRequirement;
				default:
					return null;
			}
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return SupportedAddressTypes; }
		}

		protected virtual IReadOnlyList<DocAddressType> SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.NotifyParty,
					DocAddressType.MasterBillConsigneeOverride,
					DocAddressType.MasterBillShipperOverride
				};
			}
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region MasterBillConsigneeOverride

		public OrgHeader MasterBillConsigneeOverride
		{
			get { return (MasterBillConsigneeOverrideDocumentaryAddress == null || !MasterBillConsigneeOverrideDocumentaryAddress.HasRealOrganisation) ? null : MasterBillConsigneeOverrideDocumentaryAddress.Organisation; }
		}

		public JobDocAddress MasterBillConsigneeOverrideDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(masterBillConsigneeOverrideDocumentaryAddress))
				{
					masterBillConsigneeOverrideDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(MasterBillConsigneeOverrideDocAddressRequirement);
				}
				return masterBillConsigneeOverrideDocumentaryAddress;
			}
		}
		JobDocAddress masterBillConsigneeOverrideDocumentaryAddress;

		JobDocAddressRequirement MasterBillConsigneeOverrideDocAddressRequirement
		{
			get { return masterBillConsigneeOverrideDocAddressRequirement ?? (masterBillConsigneeOverrideDocAddressRequirement = GetMasterBillConsigneeOverrideDocAddressRequirement()); }
		}
		JobDocAddressRequirement masterBillConsigneeOverrideDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetMasterBillConsigneeOverrideDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.MasterBillConsigneeOverride);
		}

		#endregion

		#region MasterBillShipperOverride

		public OrgHeader MasterBillShipperOverride
		{
			get { return (MasterBillShipperOverrideDocumentaryAddress == null || !MasterBillShipperOverrideDocumentaryAddress.HasRealOrganisation) ? null : MasterBillShipperOverrideDocumentaryAddress.Organisation; }
		}

		public JobDocAddress MasterBillShipperOverrideDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(masterBillShipperOverrideDocumentaryAddress))
				{
					masterBillShipperOverrideDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(MasterBillShipperOverrideDocAddressRequirement);
				}
				return masterBillShipperOverrideDocumentaryAddress;
			}
		}
		JobDocAddress masterBillShipperOverrideDocumentaryAddress;

		JobDocAddressRequirement MasterBillShipperOverrideDocAddressRequirement
		{
			get { return masterBillShipperOverrideDocAddressRequirement ?? (masterBillShipperOverrideDocAddressRequirement = GetMasterBillShipperOverrideDocAddressRequirement()); }
		}
		JobDocAddressRequirement masterBillShipperOverrideDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetMasterBillShipperOverrideDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.MasterBillShipperOverride);
		}

		#endregion

		#region NotifyParty

		public OrgHeader NotifyParty
		{
			get { return (NotifyPartyDocumentaryAddress == null || !NotifyPartyDocumentaryAddress.HasRealOrganisation) ? null : NotifyPartyDocumentaryAddress.Organisation; }
		}

		public JobDocAddress NotifyPartyDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(notifyPartyDocumentaryAddress))
				{
					notifyPartyDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(NotifyPartyDocAddressRequirement);
				}
				return notifyPartyDocumentaryAddress;
			}
		}
		JobDocAddress notifyPartyDocumentaryAddress;

		JobDocAddressRequirement NotifyPartyDocAddressRequirement
		{
			get { return notifyPartyDocAddressRequirement ?? (notifyPartyDocAddressRequirement = GetNotifyPartyDocAddressRequirement()); }
		}
		JobDocAddressRequirement notifyPartyDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetNotifyPartyDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.NotifyParty);
		}

		#endregion

		#region IRegisterStatusChangeContext

		public bool IsChangingByWorkflowTrigger { get; private set; }

		IDisposable IRegisterStatusChangeContext.TemporarilySetStatusChangedByTriggerEvent(IStmALog triggeringEvent)
		{
			IsChangingByWorkflowTrigger = true;

			return new DisposableAction(() =>
			{
				IsChangingByWorkflowTrigger = false;
			});
		}

		#endregion
	}
}
