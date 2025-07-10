using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoGlbBranch.Schema.GB_BranchName)]
	[DebuggerDisplay("Branch ({Company." + GlbCompany.Schema.GC_Code + "}->{" + GlbBranch.Schema.GB_Code + "})")]
	public class GlbBranch : AutoGlbBranch,
		IGlbBranch,
		IDocManagerSupport,
		IBranch,
		ILocationReference,
		IAddressDetails,
		ISupportWebAddressValidation,
		ICombinedEInvoicingCredentials
	{
		public GlbBranch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoGlbBranch.Schema
		{
			public const string GB_Phone_Formatted = "GB_Phone_Formatted";
			public const string GB_Phone_FormattedLocalNumberIfLoggedInSameCountry = "GB_Phone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GB_Phone_IsManuallyVerified = "GB_Phone_IsManuallyVerified";
			public const string GB_Fax_Formatted = "GB_Fax_Formatted";
			public const string GB_Fax_IsManuallyVerified = "GB_Fax_IsManuallyVerified";
			public const string GB_Fax_FormattedLocalNumberIfLoggedInSameCountry = "GB_Fax_FormattedLocalNumberIfLoggedInSameCountry";
			public const int LanguageMaxLength = 3;
		}

		#endregion

		#region Static

		public static GlbBranch[] GetOneActiveBranchPerCompany(string countryCode = null, BusinessObjectFactory factory = null)
		{
			return GetOneActiveBranchPerCompanyDeferred(countryCode, factory).ToArray();
		}

		public static GlbBranch GetFirstActiveBranch(string countryCode = null, BusinessObjectFactory factory = null)
		{
			return GetOneActiveBranchPerCompanyDeferred(countryCode, factory).FirstOrDefault();
		}

		static IEnumerable<GlbBranch> GetOneActiveBranchPerCompanyDeferred(string countryCode = null, BusinessObjectFactory factory = null)
		{
			var companyLoader = new GlbCompany.Loader(factory ?? new BusinessObjectFactory());
			foreach (var company in companyLoader.LoadCompanies(countryCode, activeCompaniesOnly: true, excludeDemoCompany: true))
			{
				var activeBranch = company.FirstActiveBranch;
				if (activeBranch != null)
				{
					yield return activeBranch;
				}
			}
		}

		public static GlbBranch FindControllingBranchWithFallBackToAnyCompanyIfOnlyOne(OrgHeader organisation)
		{
			return FindControllingBranchWithFallBackToAnyCompany(organisation, true);
		}

		public static GlbBranch FindControllingBranchWithFallBackToAnyCompany(OrgHeader organisation)
		{
			return FindControllingBranchWithFallBackToAnyCompany(organisation, false);
		}

		static GlbBranch FindControllingBranchWithFallBackToAnyCompany(OrgHeader organisation, bool ifOnlyOne)
		{
			GlbBranch result = null;

			if (organisation != null)
			{
				using (var command = Db.Connection.Command("GetActiveControllingBranches"))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.AddParameter("@organizationPK", SqlDbType.UniqueIdentifier, organisation.PK.ToGuid());
					var pk = ZGuid.Empty;
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							pk = new ZGuid(reader[0]);
						}
						if (ifOnlyOne && reader.Read())
						{
							pk = ZGuid.Empty;
						}
					}
					if (pk != ZGuid.Empty)
					{
						result = organisation.Factory.Load<GlbBranch>(pk);
					}
				}
			}

			return result;
		}

		public static GlbBranch FindByOrgProxy(BusinessObjectFactory factory, OrgHeader orgHeader, bool includeInactive = false, bool includeOtherCompanies = false)
		{
			if (factory == null || orgHeader == null)
			{
				return null;
			}

			var query = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, orgHeader.PK);
			if (!includeInactive)
			{
				query.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);
			}
			if (!includeOtherCompanies)
			{
				query.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			}
			query.OrderBy = GlbBranchSchema.GB_Code.Name;

			return factory.LoadTop1<GlbBranch>(query);
		}

		public static GlbBranch FindByHomePortWithFallBackToRelatedPort(BusinessObjectFactory factory, RefUNLOCO port)
		{
			return FindByHomePort(factory, port) ?? FindByRelatedPort(factory, port);
		}

		public static GlbBranch FindByHomePortWithFallBackToRelatedPort(BusinessObjectFactory factory, RefUNLOCO port, GlbCompany company)
		{
			return FindByHomePort(factory, port, company) ?? FindByRelatedPort(factory, port, company);
		}

		public static GlbBranch FindByHomePort(BusinessObjectFactory factory, RefUNLOCO homePort, GlbCompany company)
		{
			GlbBranch result = null;

			if (homePort != null)
			{
				ZQuery branchesQuery = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, homePort.RL_Code);
				branchesQuery.OrderBy = GlbBranchSchema.GB_Code.Name;
				branchesQuery.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);
				if (company != null)
				{
					branchesQuery.AddToFilter(GlbBranchSchema.GB_GC, company.PK);
				}
				result = factory.LoadTop1<GlbBranch>(branchesQuery);
			}

			return result;
		}

		public static GlbBranch FindByHomePort(BusinessObjectFactory factory, RefUNLOCO homePort)
		{
			return FindByHomePort(factory, homePort, null);
		}

		public static GlbBranch FindByRelatedPort(BusinessObjectFactory factory, RefUNLOCO relatedPort)
		{
			return FindByRelatedPort(factory, relatedPort, null);
		}

		public static GlbBranch FindByRelatedPort(BusinessObjectFactory factory, RefUNLOCO relatedPort, GlbCompany relatedCompany)
		{
			GlbBranch result = null;

			if (relatedPort != null)
			{
				ZDBOnlyQuery branchesQuery = new ZDBOnlyQuery(typeof(GlbBranch));
				branchesQuery.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);

				ZDBOnlySubQuery extraPortsSubQuery = new ZDBOnlySubQuery(typeof(GlbBranchExtraPorts), GlbBranchExtraPortsSchema.GY_GB);
				extraPortsSubQuery.AddToFilter(GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, relatedPort.RL_Code);
				branchesQuery.AddSubQuery(extraPortsSubQuery, JoinCondition.And);

				var branches = factory.Load<GlbBranch>(branchesQuery);
				var company = relatedCompany ?? GlbCompany.CurrentCompany;
				if (company != null)
				{
					foreach (var branch in branches)
					{
						if (branch.GB_GC == company.PK)
						{
							result = branch;
							break;
						}
					}
				}
				if (result == null && branches.Length > 0)
				{
					result = branches[0];
				}
			}

			return result;
		}

		public static GlbBranch Find(OrgCompanyDataDependentCollection companyDataCollection)
		{
			if (companyDataCollection != null)
			{
				foreach (OrgCompanyData companyData in companyDataCollection)
				{
					if (companyData.ControllingBranch != null)
					{
						return companyData.ControllingBranch;
					}
				}
			}

			return null;
		}

		public static GlbBranch FindAnyBranchInSameCountry(BusinessObjectFactory factory, RefCountry country)
		{
			GlbBranch result = null;

			if (country != null)
			{
				ZQuery branchesQuery = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.StartsWith, country.Code);
				branchesQuery.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);

				result = factory.LoadTop1<GlbBranch>(branchesQuery);
			}

			return result;
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.GlbBranchFetchStrategy(this);
		}

		#endregion

		#region Testing Only
#if DEBUG

		#region SetCountry

		public void SetCountryIncludingCompanyWithoutCreatingAccountingData(string countryCode)
		{
			Argument.NotNullOrEmpty(countryCode, nameof(countryCode));
			var refCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);

			SetCountry(countryCode);
			var company = Company;
			using (company.SuspendMarkingAsNeedingValidation())
			{
				company.GC_IsGSTRegistered = false;
				company.GC_RN_NKCountryCode = refCountry.RN_Code;
				company.GC_RX_NKLocalCurrency = refCountry.RN_RX_NKLocalCurrency;
				if (string.IsNullOrEmpty(company.GC_RX_NKLocalCurrency))
				{
					company.GC_RX_NKLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
			}
		}

		public void SetCountry(ZString countryCode)
		{
			if (Country == null || countryCode != Country.Code)
			{
				var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode)
					.AddToFilter(RefUNLOCOSchema.RL_IsActive, SQLComparisonOperator.Equal, true);
				query.OrderBy = RefUNLOCOSchema.Constants.RL_Code;
				RefUNLOCO bizO = Factory.LoadTop1<RefUNLOCO>(query);
				if (bizO != null)
				{
					GB_RL_NKHomePort = bizO.RL_Code;
				}
			}
			// Currency can be set to match country as license now forces this to be true for systems with Customs
		}
		#endregion

		#region FillWithValidTestData
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

			if (GB_WebAddress.IsEmpty)
			{
				GB_WebAddress = "http://www.wisetechglobal.com/";
			}

			if (GB_BranchName.IsEmpty)
			{
				GB_BranchName = "Test Branch";
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion

#endif
		#endregion

		#region BusinessObjectsWithRelatedEventsCore

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(AccCFXConfigurations.Where(x => x.Level == AccCFXConfigurationLevelEnum.Branch));
				return result.ToArray();
			}
		}

		#endregion

		#region Related Business Objects

		#region GlbHolidays

		[ChildEditable(true)]
		public GlbHolidayDependentCollection GlbHolidays
		{
			get
			{
				if (fGlbHolidays == null)
				{
					fGlbHolidays = new GlbHolidayDependentCollection(this, Factory);
					RegisterEditableChildObject(fGlbHolidays);
				}
				return fGlbHolidays;
			}
		}
		GlbHolidayDependentCollection fGlbHolidays;

		#endregion

		#region AllowedDepartments

		[ChildEditable(true)]
		public AccAllowedBranchDepartmentComboCollection AllowedDepartments
		{
			get
			{
				if (allowedDepartments == null)
				{
					allowedDepartments = new AccAllowedBranchDepartmentComboCollection(this);
					RegisterEditableChildObject(allowedDepartments);
				}
				return allowedDepartments;
			}
		}

		AccAllowedBranchDepartmentComboCollection allowedDepartments;

		public bool IsDepartmentAllowed(ZGuid departmentPK)
		{
			return AllowedDepartments.Count == 0 || AllowedDepartments.Any(x => x.AAB_GE_Department == departmentPK);
		}

		#endregion

		#region Country

		/// <summary>
		/// This should really be called CountryFromHomePort.
		/// However, there is a lot of functionality (353 unit tests) relying on GlbBranch.Country returning the Country from GB_RL_NKHomePort instead of the one from GB_RN_NKCountryCode.
		/// Do we really required both NK fields? RefCountry's NK seems redundant.
		/// </summary>
		public sealed override RefCountry Country => HomePort?.Country;

		/// <summary>
		/// Remove these once Country has been renamed to CountryFromHomePort.
		/// </summary>
		public RefCountry BaseCountry => base.Country;
		RefCountry ISupportWebAddressValidation.Country => BaseCountry;

		[Obsolete("Please use BaseCountry property. This will be removed once references in client-defined macros (Documents, Workflows, Notes, etc) are transformed.")]
		[CargoWise.Macros.MacroIgnore]
		public RefCountry CountryCode => BaseCountry;

		#endregion

		#region GlbBranchExtraPorts

		[ChildEditable(true)]
		public GlbBranchExtraPortsDependentCollection ExtraPorts
		{
			get
			{
				if (fExtraPorts == null)
				{
					fExtraPorts = new GlbBranchExtraPortsDependentCollection(this);
					fExtraPorts.Load();
					RegisterEditableChildObject(fExtraPorts);
				}
				return fExtraPorts;
			}
		}
		GlbBranchExtraPortsDependentCollection fExtraPorts;

		#endregion

		#region GlbBranchDefaultPorts

		[ChildEditable(true)]
		public GlbBranchDefaultPortDependentCollection DefaultPorts
		{
			get
			{
				if (fDefaultPorts == null)
				{
					fDefaultPorts = new GlbBranchDefaultPortDependentCollection(this);
					fDefaultPorts.Load();
					RegisterEditableChildObject(fDefaultPorts);
				}

				return fDefaultPorts;
			}
		}

		GlbBranchDefaultPortDependentCollection fDefaultPorts;

		#endregion

		#region EInvoicingCredentials

		IEInvoicingCredentialSettings EInvoicingCredentialSettings
		{
			get
			{
				if (eInvoicingCredentialSettingsValue == null)
				{
					var objectFactory = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>();
					var settings = objectFactory.GetCountryEInvoicingObjectFactorySettings(Company?.GC_RN_NKCountryCode ?? ZString.Empty);
					eInvoicingCredentialSettingsValue = settings?.Credentials;
				}
				return eInvoicingCredentialSettingsValue;
			}
		}
		IEInvoicingCredentialSettings eInvoicingCredentialSettingsValue;

		[ChildEditable(true)]
		public CombinedEInvoicingCertificateCollection EInvoicingCertificateCredentials => GetLazyEInvoicingCertificateCredentials().Value;

		Lazy<CombinedEInvoicingCertificateCollection> GetLazyEInvoicingCertificateCredentials()
		{
			CombinedEInvoicingCertificateCollection GetCollection()
			{
				var credentialSettings = EInvoicingCredentialSettings as IEInvoicingCertificateCredentialSettings;
				var collection = new CombinedEInvoicingCertificateCollection(new CombinedEInvoicingMaster(this), credentialSettings);
				RegisterEditableChildObject(collection);
				if (collection.IsEnabled && collection.IsEditAllowed)
				{
					collection.Load();
				}
				return collection;
			}

			return lazyEInvoicingCertificateCredentials ?? (lazyEInvoicingCertificateCredentials = new Lazy<CombinedEInvoicingCertificateCollection>(GetCollection));
		}
		Lazy<CombinedEInvoicingCertificateCollection> lazyEInvoicingCertificateCredentials;

		[ChildEditable(true)]
		public CombinedEInvoicingPasswordCollection EInvoicingPasswordCredentials => GetLazyEInvoicingPasswordCredentials().Value;

		Lazy<CombinedEInvoicingPasswordCollection> GetLazyEInvoicingPasswordCredentials()
		{
			CombinedEInvoicingPasswordCollection GetCollection()
			{
				var credentialSettings = EInvoicingCredentialSettings as IEInvoicingPasswordCredentialSettings;
				var collection = new CombinedEInvoicingPasswordCollection(new CombinedEInvoicingMaster(this), credentialSettings);
				RegisterEditableChildObject(collection);
				if (collection.IsEnabled && collection.IsEditAllowed)
				{
					collection.Load();
				}
				return collection;
			}

			return lazyEInvoicingPasswordCredentials ?? (lazyEInvoicingPasswordCredentials = new Lazy<CombinedEInvoicingPasswordCollection>(GetCollection));
		}
		Lazy<CombinedEInvoicingPasswordCollection> lazyEInvoicingPasswordCredentials;

		#endregion

		#region CertificateCredentialsTaxCore

		[ChildEditable(true)]
		public EInvoicingCertificateCredentialCollectionForTaxCore CertificateCredentialsTaxCore
		{
			get
			{
				if (certificateCredentialsTaxCore == null)
				{
					certificateCredentialsTaxCore = new EInvoicingCertificateCredentialCollectionForTaxCore(this);
					RegisterEditableChildObject(certificateCredentialsTaxCore);
					certificateCredentialsTaxCore.Load();
				}
				return certificateCredentialsTaxCore;
			}
		}
		EInvoicingCertificateCredentialCollectionForTaxCore certificateCredentialsTaxCore;

		#endregion

		#region BranchCredentialsIndia

		public GlbBranchCredentialsForIndia BranchCredentialsIndia
		{
			get
			{
				if (branchCredentialsIndia == null && GlbBranchCredentialsForIndia.IsAllowed(this))
				{
					branchCredentialsIndia = GlbBranchCredentialsForIndia.New(this);
					RegisterEditableChildObject(branchCredentialsIndia);
				}
				return branchCredentialsIndia;
			}
		}
		GlbBranchCredentialsForIndia branchCredentialsIndia;

		#endregion

		#region HumanReadableNameForRegistry

		public string HumanReadableNameForRegistry => Env.Registry.ShowCodeAtCompanyAndBranchName ? HumanReadableShortcutName : GB_BranchName;

		#endregion

		internal IEnumerable<IStmScheduleTask> RelatedScheduleTasks
		{
			get
			{
				if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
				{
					var scheduleTasks = Factory.Load<IStmScheduleTask>(new ZQuery(
						new ZQuery(StmScheduleTaskSchema.S5_ParentTableCode, SQLComparisonOperator.NotEqual, Constants.ServiceTask.ParentTableCode),
						JoinCondition.And,
						new ZQuery(StmScheduleTaskSchema.S5_GB, PK)));

					var stmServiceTasks = Factory.Load<IStmServiceTaskBranchValidationAdapter>(new ZQuery(StmServiceTaskSchema.SST_GB_Branch, PK));

					return scheduleTasks.Concat(stmServiceTasks);
				}

				return Factory.Load<IStmScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_GB, PK));
			}
		}

		internal IEnumerable<IStmScheduleTask> RelatedActiveScheduleTasks => RelatedScheduleTasks.Where(t => t.S5_IsActive);

		internal IEnumerable<IGlbStaff> RelatedStaff => Factory.Load<IGlbStaff>(new ZQuery(GlbStaffSchema.GS_GB_HomeBranch, PK));

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var envCompany = GlbCompany.CurrentCompany;
			if (envCompany != null)
			{
				GB_RN_NKCountryCode = envCompany.GC_RN_NKCountryCode;
			}
			//Make sure to also set 'empty', as the row might still be null (as we read null as empty)
			if (!GB_GeoLocation.IsValid || GB_GeoLocation.IsEmpty)
			{
				GB_GeoLocation = ZGeography.Empty;
			}
		}

		#endregion

		#region Properties

		public override ZString GB_Code
		{
			get => base.GB_Code;
			set
			{
				base.GB_Code = value;

				foreach (var accTaxConfiguration in AccTaxConfigurations)
				{
					accTaxConfiguration.OverrideCodeValue();
				}
			}
		}

		#region AccCFXConfigurations

		AccCFXUpliftConfigurationCollection accCFXConfigurations;
		[ChildEditable(true)]
		public AccCFXUpliftConfigurationCollection AccCFXConfigurations
		{
			get
			{
				if (accCFXConfigurations == null)
				{
					var localAccCFXConfigurations = new AccCFXUpliftConfigurationCollection(Factory, GB_GC, PK);
					localAccCFXConfigurations.Load();
					accCFXConfigurations = localAccCFXConfigurations;
					RegisterEditableChildObject(accCFXConfigurations);
				}
				return accCFXConfigurations;
			}
		}

		#endregion

		#region AccTaxConfigurations

		[ChildEditable(true)]
		public AccTaxConfigurationCollection AccTaxConfigurations
		{
			get
			{
				if (accTaxConfigurations == null)
				{
					accTaxConfigurations = new AccTaxConfigurationCollectionForCompanyOrBranch(this);
					RegisterEditableChildObject(accTaxConfigurations);
				}

				return accTaxConfigurations;
			}
		}
		AccTaxConfigurationCollection accTaxConfigurations;

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region GB_GC

		[List("Lookups.Companies")]
		[ReadOnlyMember(nameof(IsInDatabase))]
		public override ZGuid GB_GC
		{
			get { return base.GB_GC; }
			set
			{
				if (GB_GC != value)
				{
					base.GB_GC = value;
					GlbCompany company;
					using (DoNotReportMissingCompany()) // There is FK constraint in db for this column, no need to worry about wrong value here
					{
						company = Company;
					}
					if (company != null && company.GC_OH_OrgProxy.IsValid)
					{
						GB_OH_OrgProxy = company.GC_OH_OrgProxy;
					}
				}
			}
		}

		#endregion

		#region GB_AccountingGroupCode

		[List("Lookups.AccountingGroupCodes")]
		public override ZString GB_AccountingGroupCode
		{
			get { return base.GB_AccountingGroupCode; }
			set { base.GB_AccountingGroupCode = value; }
		}

		#endregion

		#region GB_Email

		[EmailAddress]
		public override ZString GB_Email
		{
			get
			{
				return base.GB_Email;
			}

			set
			{
				base.GB_Email = value;
			}
		}

		#endregion

		#region GB_IsActive

		public override ZBool GB_IsActive
		{
			get
			{
				return base.GB_IsActive;
			}
			set
			{
				base.GB_IsActive = value;
				Validation.ValidateAll();
				GB_Phone_Wrapper.FormattedForBindingInfo.RefreshBinding();
				if (Company != null)
				{
					Company.Validation.ValidateGC_IsActive();
					Company.GC_IsActiveInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region GB_OH_OrgProxy
		[List("Lookups.OrgProxies")]
		public override ZGuid GB_OH_OrgProxy
		{
			get { return base.GB_OH_OrgProxy; }
			set
			{
				base.GB_OH_OrgProxy = value;
			}
		}

		#endregion

		#region Company

		public override GlbCompany Company
		{
			get
			{
				var company = base.Company;
				if (company == null && GB_GC.IsValid)
				{
					if (PK == Env.CurrentBranch.PK)
					{
						Factory.ClearQueryCache(GlbCompanySchema.Constants.TableName);
						company = base.Company;
					}

					if (company == null)
					{
						RowFactory.ClearSpecificTableFromUberFactory(GlbCompanySchema.Constants.TableName);

						var query = new ZQuery(GlbCompanySchema.PK, GB_GC)
						{
							ReLoadExistingRows = true
						};
						company = Factory.LoadTop1<GlbCompany>(query);
					}

					if (company == null && !doNotReportMissingCompany)
					{
						var errorMessageBuilder = new StringBuilder();
						errorMessageBuilder.AppendLine(string.Format((NoResString)"Cannot load Company for PK '{0}' for branch '{1}'.", GB_GC.ToString(), PK.ToString()));
						errorMessageBuilder.AppendLine(this.GetAllPropertyValues());

						var exceptionKey = "GlbBranchCompany_CannotLoadCompany_NewBranch";

						if (IsInDatabase)
						{
							var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
							var branchInDB = newFactory.Load<GlbBranch>(PK);

							if (branchInDB != null)
							{
								errorMessageBuilder.AppendLine((NoResString)"Branch in Database:");
								errorMessageBuilder.AppendLine(branchInDB.GetAllPropertyValues());
								exceptionKey = "GlbBranchCompany_CannotLoadCompany_PersistedBranch";
							}
							else
							{
								exceptionKey = "GlbBranchCompany_CannotLoadCompany_PersistedBranchIsNotInDatabase";
							}
						}

						errorMessageBuilder.AppendLine($"Factory information: {Factory.GetDebugInformation("GlbCompany", GB_GC)}");

						ErrorReporter.ReportOnce(exceptionKey, errorMessageBuilder.ToString());
					}
				}
				return company;
			}
		}

		IDisposable DoNotReportMissingCompany()
		{
			if (doNotReportMissingCompany)
			{ return null; }
			doNotReportMissingCompany = true;
			return new DisposableAction(() => { doNotReportMissingCompany = false; });
		}
		bool doNotReportMissingCompany;

		#endregion

		#region CurrentBranch

		[TestExcludeDetectStaticBusinessObjectsCollectionsAndFactories]
		public static GlbBranch CurrentBranch
		{
			get { return (GlbBranch)Env.CurrentBranch; }
		}

		public static GlbBranch GetCurrentBranch(BusinessObjectFactory factory)
		{
			var branch = CurrentBranch;
			return branch != null && factory != branch.Factory ? factory.Load<GlbBranch>(branch.PK) : branch;
		}

		#endregion

		#region GB_RL_NKHomePort

		[List("Lookups.HomePorts")]
		public override ZString GB_RL_NKHomePort
		{
			get
			{
				return base.GB_RL_NKHomePort;
			}
			set
			{
				if (base.GB_RL_NKHomePort != value && !base.GB_RL_NKHomePort.IsEmpty)
				{
					if (!Env.Security.BranchModify.IsAllowed)
					{
						ErrorReporter.ReportOnce("ModifyingHomePortWithoutSecurityRight",
							string.Format(CultureInfo.InvariantCulture, "Home Port should only be edited through the form and only by people with the security access. Assign this issue to Timothy Stiles (TST) please. New Value = {0}",
							value));
					}
				}
				base.GB_RL_NKHomePort = value;
			}
		}

		#endregion

		#region GB_RN_NKCountryCode

		[List("Lookups.Countries")]
		public override ZString GB_RN_NKCountryCode
		{
			get { return base.GB_RN_NKCountryCode; }
			set
			{
				if (GB_RN_NKCountryCode != value)
				{
					base.GB_RN_NKCountryCode = value;
					GB_Phone_Wrapper.RefreshFormat();
					GB_Fax_Wrapper.RefreshFormat();
					ResetValidationStatus(GB_RN_NKCountryCodeInfo);
				}
			}
		}

		#endregion

		#region IsRegisteredVATInChina

		public ZBool IsRegisteredVATInChina
		{
			get
			{
				if (OrgProxy == null)
				{
					return false;
				}

				return OrgProxy.CustomsCodes.Cast<OrgCusCode>().Any(c =>
								 c.OK_CodeType == OrgCusCode.CodeTypes.VATCode &&
								 c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.China &&
								 !string.IsNullOrEmpty(c.OK_CustomsRegNo));
			}
		}

		#endregion

		#region GB_ValidationStatus

		public override ZString GB_ValidationStatus
		{
			get => base.GB_ValidationStatus;

			set
			{
				base.GB_ValidationStatus = value;
				isManuallyVerifiedByUser = value == AddressValidationStatus.ManuallyVerified;
			}
		}

		#endregion

		#endregion

		#region Phone Numbers

		#region GB_Phone

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString GB_Phone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(GB_PhoneInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(GB_PhoneInfo, GB_Phone_FormattedInfo, value, Validation.ValidateGB_Phone_Formatted, GB_Phone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo GB_Phone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GB_Phone_Formatted); }
		}

		public ZString GB_Phone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GB_PhoneInfo); }
		}

		public ZPropertyInfo GB_Phone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GB_Phone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GB_Phone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbBranchSchema.Constants.GB_Phone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GB_Phone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbBranchSchema.Constants.GB_Phone, Validation.ValidateGB_Phone_Formatted, GB_Phone_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GB_Phone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GB_Phone_IsManuallyVerified); }
		}

		public PhoneNumber GB_Phone_Wrapper
		{
			get { return gbPhoneWrapper ?? (gbPhoneWrapper = new PhoneNumber(GB_Phone_FormattedInfo, null, GB_Phone_FormattedLocalNumberIfLoggedInSameCountryInfo, GB_Phone_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gbPhoneWrapper;

		#endregion

		#region GB_InternalExtension

		public PhoneNumber GB_InternalExtension_Wrapper
		{
			get
			{
				if (gbInternalExtensionWrapper == null)
				{
					gbInternalExtensionWrapper = new PhoneNumber(GB_InternalExtensionInfo, null, null);
				}
				return gbInternalExtensionWrapper;
			}
		}
		PhoneNumber gbInternalExtensionWrapper;

		#endregion

		#region GB_Fax

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString GB_Fax_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(GB_FaxInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(GB_FaxInfo, GB_Fax_FormattedInfo, value, Validation.ValidateGB_Fax_Formatted, GB_Fax_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo GB_Fax_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GB_Fax_Formatted); }
		}

		public ZString GB_Fax_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GB_FaxInfo); }
		}

		public ZPropertyInfo GB_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GB_Fax_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GB_Fax_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbBranchSchema.Constants.GB_Fax, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GB_Fax_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbBranchSchema.Constants.GB_Fax, Validation.ValidateGB_Fax_Formatted, GB_Fax_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GB_Fax_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GB_Fax_IsManuallyVerified); }
		}

		public PhoneNumber GB_Fax_Wrapper
		{
			get { return gbFaxWrapper ?? (gbFaxWrapper = new PhoneNumber(GB_Fax_FormattedInfo, null, GB_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo, GB_Fax_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gbFaxWrapper;

		#endregion

		#region AddOnRuleAcks

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

		#region DefaultCountryCodeForPhoneNumbers

		internal ZString DefaultCountryCodeForPhoneNumbers
		{
			get { return !GB_RN_NKCountryCode.IsEmpty ? GB_RN_NKCountryCode : Company?.DefaultCountryCodeForPhoneNumbers ?? ZString.Empty; }
		}

		#endregion

		#region Implementations

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper
		{
			get { return phoneNumberPropertyHelper ?? (phoneNumberPropertyHelper = new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbers)); }
		}

		PhoneNumberPropertyHelper phoneNumberPropertyHelper;

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			if (PK != CurrentBranch.PK)
			{
				AccTaxConfigurations.DeleteAll();
				GlbHolidays.DeleteAll();
				AllowedDepartments.DeleteAll();
				ExtraPorts.RemoveAndDeleteAll();
				DefaultPorts.RemoveAndDeleteAll();
				AddOnRuleAcks.DeleteAll();
				DeleteReferencingStaff();
				base.Delete();
			}
		}

		void DeleteReferencingStaff()
		{
			var query = new ZQuery();
			query.AddToFilter(JoinCondition.Or, GlbStaffSchema.GS_GB_HomeBranch, this.PK);
			query.AddToFilter(JoinCondition.Or, GlbStaffSchema.GS_GB_LastLogonBranch, this.PK);
			var staffs = new GlbStaffCollection(Factory, query);
			foreach (var staff in staffs.ToArray())
			{
				if (staff.GS_GB_HomeBranch == this.PK)
				{
					staff.GS_GB_HomeBranch = ZGuid.Empty;
				}
				if (staff.GS_GB_LastLogonBranch == this.PK)
				{
					staff.GS_GB_LastLogonBranch = ZGuid.Empty;
				}
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString reason = base.ReasonForNotAbleToDelete;
				if (IsLastActiveBranch)
				{
					reason = ResString.GetMultilingualString("fe550d94-f6c6-4629-86cc-2f3b4f1bcda1", "Cannot delete Branch. This is the last active Branch and cannot be deleted.");
				}
				return reason;
			}
		}

		public override bool CanDelete
		{
			get
			{
				return !IsLastActiveBranch;
			}
		}

		public bool IsLastActiveBranch
		{
			get
			{
				//We have to do two separate queries because of the following case:
				//1) We have have company form open, and have unticked 'is active' on all branches.
				//2) Query 'are there any inactive branches' goes to the database, returns a row that we have set to inactive in this factory.
				//3) As a result, in this factory we select over only this company's branches (which are all inactive).
				//4) Unable to find a branch that is active (even though many are in the database), false is erroneously returned.

				ZQuery activeBranchesInCompanyQuery = new ZQuery(GlbBranchSchema.GB_IsActive, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueString);
				activeBranchesInCompanyQuery.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, PK);
				activeBranchesInCompanyQuery.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GB_GC);

				ZQuery activeBranchesOutOfCompanyQuery = new ZQuery(GlbBranchSchema.GB_IsActive, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueString);
				activeBranchesOutOfCompanyQuery.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, PK);
				activeBranchesOutOfCompanyQuery.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GB_GC);

				return Factory.LoadTop1<GlbBranch>(activeBranchesInCompanyQuery) == null && Factory.LoadTop1<GlbBranch>(activeBranchesOutOfCompanyQuery) == null;
			}
		}

		public bool IgnoreValidationStatusError { get; set; }

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Branch);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IBranch Members

		string IBranch.Code
		{
			get { return GB_Code; }
		}

		Guid IBranch.CompanyPK
		{
			get { return GB_GC.ToGuid(); }
		}

		ICompany IBranch.Company
		{
			get { return Company; }
		}

		string IBranch.Name
		{
			get { return GB_BranchName; }
		}

		string IBranch.NKUNLOCO
		{
			get { return GB_RL_NKHomePort; }
		}

		Guid IBranch.OrganisationPK
		{
			get { return !GB_OH_OrgProxy.IsValid || GB_OH_OrgProxy.IsEmpty ? Guid.Empty : GB_OH_OrgProxy.ToGuid(); }
		}

		string IBranch.Phone
		{
			get { return GB_Phone; }
		}

		Guid IBranch.PK
		{
			get { return PK.ToGuid(); }
		}

		string IBranch.State
		{
			get { return GB_State; }
		}

		string IBranch.HumanReadableNameForRegistry => HumanReadableNameForRegistry;
		bool IBranch.IsActive => GB_IsActive;

		#endregion

		#region IGlbBranch Members

		IGlbCompany IGlbBranch.Company
		{
			get { return Company; }
		}

		string IGlbBranch.NKUNLOCO
		{
			get { return this.GB_RL_NKHomePort; }
		}

		ITimeZone IGlbBranch.HomeTimeZone
		{
			get => HomePort?.TimeZoneSet?.GetCalculationTimeZone();
		}

		#endregion

		#region ILocationReference Members

		bool ILocationReference.IsLocalInRelationTo(ZString code)
		{
			bool result = false;

			if (!code.IsEmpty)
			{
				ILocationReference locationReference = HomePort;

				result = locationReference != null && locationReference.IsLocalInRelationTo(code)
					|| ExtraPorts.Cast<GlbBranchExtraPorts>().Any(extraPort => extraPort.GY_RL_NKAdditionalBranchRelatedPort == code);
			}

			return result;
		}

		#endregion

		#region IAddressDetails Members

		ZString IAddressDetails.AddressLine1
		{
			get { return GB_Address1; }
		}

		ZString IAddressDetails.AddressLine2
		{
			get { return GB_Address2; }
		}

		ZString IAddressDetails.City
		{
			get { return GB_City; }
		}

		ZString IAddressDetails.CompanyName
		{
			get { return OrgProxy?.OH_FullNameTruncated ?? GB_BranchName; }
		}

		ZString IAddressDetails.ContactName
		{
			get { return ""; }
		}

		ZString IAddressDetails.Country
		{
			get { return Country?.RN_Code ?? ZString.Empty; }
		}

		ZString IAddressDetails.Email
		{
			get { return GB_Email; }
		}

		ZString IAddressDetails.Fax
		{
			get { return GB_Fax; }
		}

		ZString IAddressDetails.Phone
		{
			get { return GB_Phone; }
		}

		ZString IAddressDetails.PostCode
		{
			get { return GB_PostCode; }
		}

		ZString IAddressDetails.State
		{
			get { return GB_State; }
		}

		#endregion

		#region ISupportWebAddressValidation

		public bool IsErrorSuppressed => false;
		public bool IsJobDocAddress => false;

		public event EventHandler TriggerWebAddressValidation;
		public event EventHandler AddressValidationStatusChanged;
		public event EventHandler TriggerWebGetCityTown;

		void RaiseAddressValidationStatusChanged()
		{
			if (AddressValidationStatusChanged != null)
			{
				AddressValidationStatusChanged(this, EventArgs.Empty);
			}
		}

		public bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return false;
		}

		public async Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			return await AddressValidationService.ValidateAddressAsync(this, cancellationToken, false, cleanseAction);
		}

		public async Task<CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken)
		{
			return await AddressValidationService.GetCityTownAsync(this, cancellationToken);
		}

		bool isManuallyVerifiedByUser;

		public AddressValidationSection ValidationSection { get; } = AddressValidationSection.Branch;

		//helper
		public void ResetValidationStatus(ZPropertyInfo propertyInfo)
		{
			if (isManuallyVerifiedByUser)
			{
				return;
			}

			if (Env.Registry.EnableAddressValidationWebService)
			{
				if (ValidationStatus != AddressValidationStatus.CountryNotAvailable && BaseCountry != null && OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(BaseCountry.PK.ToGuid(), ValidationSection))
				{
					ValidationStatus = AddressValidationStatus.ToBeVerified;
					AddressMap = string.Empty;
					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					if (!string.IsNullOrEmpty(registrationKey.SystemId))
					{
						RaiseWebServices(propertyInfo);
					}
				}
				else if (propertyInfo.Name == nameof(GB_RN_NKCountryCode))
				{
					ValidationStatus = AddressValidationStatus.ToBeVerified;
					AddressMap = string.Empty;
				}
			}
		}

		//helper
		void RaiseWebServices(ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(GB_RN_NKCountryCode))
			{
				if ((string.IsNullOrEmpty(GB_City) || string.IsNullOrEmpty(GB_State) || string.IsNullOrEmpty(GB_PostCode)) && (propertyInfo == GB_CityInfo || propertyInfo == GB_StateInfo || propertyInfo == GB_PostCodeInfo))
				{
					RaiseTriggerWebGetCityTown(propertyInfo);
				}
				else
				{
					RaiseTriggerWebAddressValidation(propertyInfo);
				}
			}
		}

		//helper
		void RaiseTriggerWebGetCityTown(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebGetCityTown != null)
			{
				TriggerWebGetCityTown(this, new InfoEventArgs(propertyInfo));
			}
		}

		//helper
		void RaiseTriggerWebAddressValidation(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebAddressValidation != null)
			{
				TriggerWebAddressValidation(this, new InfoEventArgs(propertyInfo));
			}
		}

		public void PreValidationForAddressValidationService()
		{
			Validation.ValidateGB_Address1();
			Validation.ValidateGB_City();
			Validation.ValidateGB_RN_NKCountryCode();
			ValidatePostcodeAndStateForAddress();
		}

		public void ValidatePostcodeAndStateForAddress()
		{
			Validation.ValidateGB_PostCode();
			Validation.ValidateGB_State();
		}

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

		public override ZString GB_Address1
		{
			get { return base.GB_Address1; }
			set
			{
				if (value != base.GB_Address1)
				{
					base.GB_Address1 = value;
					ResetValidationStatus(GB_Address1Info);
				}
			}
		}

		public override ZString GB_Address2
		{
			get { return base.GB_Address2; }
			set
			{
				if (value != base.GB_Address2)
				{
					base.GB_Address2 = value;
					ResetValidationStatus(GB_Address2Info);
				}
			}
		}

		public override ZString GB_City
		{
			get { return base.GB_City; }
			set
			{
				if (value != base.GB_City)
				{
					base.GB_City = value;
					ResetValidationStatus(GB_CityInfo);
				}
			}
		}

		[List("Lookups.StateList")]
		public override ZString GB_State
		{
			get { return base.GB_State; }
			set
			{
				if (value != base.GB_State)
				{
					base.GB_State = value;
					ResetValidationStatus(GB_StateInfo);
				}
			}
		}

		public override ZString GB_PostCode
		{
			get { return base.GB_PostCode; }
			set
			{
				if (value != base.GB_PostCode)
				{
					base.GB_PostCode = value;
					ResetValidationStatus(GB_PostCodeInfo);
				}
			}
		}

		[BusinessObjectTestExclude] // Language is English only
		public ZString Language
		{
			get { return Constants.Languages.English; }
			set { }
		}

		public ZPropertyInfo LanguageInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Language));
			}
		}

		public int Language_MaxLength
		{
			get
			{
				return Schema.LanguageMaxLength;
			}
		}

		public CodeDescriptionPairList LanguageList { get { return null; } }

		public ZString Address1
		{
			get { return GB_Address1; }
			set { GB_Address1 = value; }
		}

		public ZPropertyInfo Address1Info
		{
			get { return GB_Address1Info; }
		}

		public int Address1_MaxLength
		{
			get { return AutoGlbBranch.Schema.GB_Address1MaxLength; }
		}

		public ZString Address2
		{
			get { return GB_Address2; }
			set { GB_Address2 = value; }
		}

		public ZPropertyInfo Address2Info
		{
			get { return GB_Address2Info; }
		}

		public int Address2_MaxLength
		{
			get { return AutoGlbBranch.Schema.GB_Address2MaxLength; }
		}

		public ZString City
		{
			get { return GB_City; }
			set { GB_City = value; }
		}

		public ZPropertyInfo CityInfo
		{
			get { return GB_CityInfo; }
		}

		public int City_MaxLength
		{
			get { return AutoGlbBranch.Schema.GB_CityMaxLength; }
		}

		public ZString Postcode
		{
			get { return GB_PostCode; }
			set { GB_PostCode = value; }
		}

		public ZPropertyInfo PostcodeInfo
		{
			get { return GB_PostCodeInfo; }
		}

		public int Postcode_MaxLength
		{
			get { return AutoGlbBranch.Schema.GB_PostCodeMaxLength; }
		}

		[BusinessObjectTestExclude] // Company of a branch cannot be changed
		public ZString CompanyName
		{
			get { return Env.CurrentCompany.Name; }
			set { }
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyName)); }
		}

		public int CompanyName_MaxLength
		{
			get { return 0; }
		}

		public ZString StateCode
		{
			get { return GB_State; }
			set
			{
				GB_State = value;
				StateCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StateCode)); }
		}

		public int StateCode_MaxLength
		{
			get { return AutoGlbBranch.Schema.GB_StateMaxLength; }
		}

		public CodeDescriptionPairList StateCodeList
		{
			get
			{
				return Lookups.StateList;
			}
		}

		[BusinessObjectTestExclude] // State has to be valid
		public ZString State
		{
			get { return StateCodeList.GetDescriptionFromCode(GB_State); }
			set
			{
				var code = (ZString)StateCodeList.GetCodeFromDescription(value);
				GB_State = string.IsNullOrEmpty(code) ? value : code;
				StateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		public int State_MaxLength
		{
			get { return AutoGlbBranch.Schema.GB_StateMaxLength; }
		}

		[List("CountryCodeList")]
		ZString ISupportWebAddressValidation.CountryCodeISO2
		{
			get { return GB_RN_NKCountryCode; }
			set { GB_RN_NKCountryCode = value; }
		}

		public int CountryCodeISO2_MaxLength => Schema.GB_RN_NKCountryCodeMaxLength;

		public RefCountryCollection CountryCodeList
		{
			get { return Lookups.Countries; }
		}

		public ZString DisplayText
		{
			get { return GB_Code; }
			set { GB_Code = value; }
		}

		public ZGuid EntityPK
		{
			get { return PK; }
		}

		public ZString AddressRecordGUID
		{
			get { return PK.ToString(); }
		}

		public ZString AddressSourceTable
		{
			get { return GlbBranchSchema.Constants.Prefix; }
		}

		public ZString ValidationStatus
		{
			get
			{
				return GB_ValidationStatus;
			}
			set
			{
				GB_ValidationStatus = value;
				RaiseAddressValidationStatusChanged();
			}
		}

		public ZString AddressMap
		{
			get { return GB_AddressMap; }
			set { GB_AddressMap = value; }
		}

		public ZString Addressee
		{
			get { return GB_BranchName; }
		}

		public ZGeography GeoLocation
		{
			get { return GB_GeoLocation; }
			set { GB_GeoLocation = value; }
		}

		public bool NeedValidation
		{
			get
			{
				var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, GB_RN_NKCountryCode);
				if (country != null)
				{
					if (!GB_Address1.IsEmpty && !GB_PostCode.IsEmpty && !GB_City.IsEmpty && !GB_State.IsEmpty)
					{
						if (!IsInDatabase || (GB_Address1Info.HasChanges || GB_Address2Info.HasChanges || GB_PostCodeInfo.HasChanges ||
												GB_CityInfo.HasChanges || GB_StateInfo.HasChanges || GB_RN_NKCountryCodeInfo.HasChanges))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool IsUpdatingCityTown { get; set; }
		public bool IsValidatingAddress { get; set; }
		public bool IsExactPointFound { get; set; }
		public bool IsValidatedByBackgroundService { get; set; }
		public bool IsTSAKnownAddress => false;
		public bool IsMIDAddress => false;

		[DocumentFieldExcludeFromMap]
		[BusinessObjectTestExclude]
		public ZString UnrestrictedAdditionalAddressInformation { get; set; }

		public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo => GetZPropertyInfo(nameof(UnrestrictedAdditionalAddressInformation));

		[BusinessObjectTestExclude]
		public CodeDescriptionPairList AdditionalAddressInfoList { get; }

		public ZString AddressCode { get; set; }

		public ZString ClosestPort { get; set; }

		#endregion

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{ }

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(GlbBranch);
			}

			/// <summary>
			/// Gets active branch in the specified country whose Org Proxy (or Org Proxy of its company) matches the specified OrgHeader
			/// </summary>
			/// <param name="orgHeader">The OrgHeader to match</param>
			/// <param name="countryCode">The country to search branch in</param>
			/// <returns>The matching branch if found; null otherwise</returns>
			public GlbBranch LoadActiveMatchingBranchInThisCountry(OrgHeader orgHeader, string countryCode)
			{
				if (orgHeader != null)
				{
					var companyLoader = new GlbCompany.Loader(Factory);
					var companies = companyLoader.LoadCompanies(countryCode);
					foreach (var company in companies)
					{
						if (company.GC_OH_OrgProxy == orgHeader.PK)
						{
							return company.FirstActiveBranch;
						}
						foreach (var branch in company.Branches)
						{
							if (branch.GB_IsActive && branch.GB_OH_OrgProxy == orgHeader.PK)
							{
								return branch;
							}
						}
					}
				}
				return null;
			}

			public GlbBranch[] LoadAllBranchesInThisCountryActiveOnly(string countryCode)
			{
				return LoadAllBranchesInThisCountry(countryCode, true);
			}

			public GlbBranch[] LoadAllBranchesInThisCountry(string countryCode, bool onlyShowActiveBranchesOnActiveCompanies)
			{
				var companyLoader = new GlbCompany.Loader(this.Factory);
				var companies = companyLoader.LoadCompanies(countryCode, onlyShowActiveBranchesOnActiveCompanies);
				var branches = new List<GlbBranch>(companies.Length);
				foreach (var company in companies)
				{
					foreach (var branch in company.Branches)
					{
						if (onlyShowActiveBranchesOnActiveCompanies)
						{
							if (branch.GB_IsActive)
							{
								branches.Add(branch);
							}
						}
						else
						{
							branches.Add(branch);
						}
					}
				}
				return branches.ToArray();
			}
		}
	}
}
