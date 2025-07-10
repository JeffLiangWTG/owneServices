using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[CodeProperty(Schema.CPH_Number), DescriptionProperty(nameof(AuthorizationTypeDescription))]
	[RestrictedFilteredItem]
	public class CusAuthorisationHeader : CommonCusPermitHeader, Integration.Customs.ICusAuthorisationHeader, ICustomsNumberViewStmNumsParent
	{
		public CusAuthorisationHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusPermitHeader.Schema
		{
			public const string IsCurrent = nameof(CusAuthorisationHeader.IsCurrent);
			public const string AuthorizationAddress = nameof(CusAuthorisationHeader.AuthorizationAddress);
			public const string CusAuthorisationRuleCodes = nameof(CusAuthorisationHeader.CusAuthorisationRuleCodes);
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static ZString GetAuthorisationNumber(BusinessObjectFactory factory, ZString countryCode, ZString type, ZDateTime transactionDate, ZGuid permitHolder)
			{
				return GetAuthorisation(factory, countryCode, type, transactionDate, permitHolder)?.CPH_Number ?? ZString.Empty;
			}

			public static CusAuthorisationHeader GetAuthorisation(BusinessObjectFactory factory, ZString countryCode, ZString type, ZDateTime transactionDate, ZGuid permitHolder)
			{
				var permits = GetAuthorisations(factory, countryCode, new[] { type }, transactionDate, permitHolder);
				return permits.OrderByDescending(x => x.CPH_StartDate).FirstOrDefault();
			}

			public static CusAuthorisationHeader[] GetAuthorisations(BusinessObjectFactory factory, ZString countryCode, ZString[] types, ZDateTime transactionDate, params ZGuid[] permitHolders) => GetAuthorisations(factory, countryCode, types, transactionDate, permitHolders, null);

			public static CusAuthorisationHeader[] GetAuthorisationsForAddresses(BusinessObjectFactory factory, ZString countryCode, ZString[] types, ZDateTime transactionDate, ZGuid[] permitAddresses) => GetAuthorisations(factory, countryCode, types, transactionDate, null, permitAddresses);

			public static CusAuthorisationHeader[] GetAuthorisationsForAddressesAndPermitHolder(BusinessObjectFactory factory, ZString countryCode, ZString[] types, ZDateTime transactionDate, ZGuid[] permitHolders, ZGuid[] permitAddresses) => GetAuthorisations(factory, countryCode, types, transactionDate, permitHolders, permitAddresses);

			public static CusAuthorisationHeader[] GetAuthorisationsWithSpecificRule(BusinessObjectFactory factory, ZString[] types, ZGuid[] permitHolders, ZString countryCode, ZDateTime transactionDate, ZString ruleCode, params ZString[] ruleValues)
			{
				Argument.NotNull(factory, nameof(factory));

				if (countryCode.IsEmpty || ruleCode.IsEmpty || ruleValues == null || types == null || permitHolders == null || !transactionDate.IsValid)
				{
					return EmptyAuthorisationHeaders;
				}

				var filter = GetCusAuthorisationHeaderQueryBuilder(countryCode)
					.AddTransactionDateFilter(transactionDate)
					.AddTypeFilter(types)
					.AddPermitHoldersFilter(permitHolders);

				var query = filter.AddRuleFilter(filter.Build(), ruleCode, ruleValues);

				return factory.Load<CusAuthorisationHeader>(query);
			}

			public static CusAuthorisationHeader[] GetAuthorisationsWithSpecificNumberTypeAndCountryCode(BusinessObjectFactory factory, ZString number, ZString type, ZString countryCode)
			{
				Argument.NotNull(factory, nameof(factory));

				if (number.IsEmpty || type.IsEmpty || countryCode.IsEmpty)
				{
					return EmptyAuthorisationHeaders;
				}

				var filter = GetCusAuthorisationHeaderQueryBuilder(countryCode)
					.AddAuthorisationNumberFilter(number)
					.AddTypeFilter(type);

				var query = filter.Build();

				return factory.Load<CusAuthorisationHeader>(query);
			}

			static CusAuthorisationHeader[] GetAuthorisations(BusinessObjectFactory factory, ZString countryCode, ZString[] types, ZDateTime transactionDate, ZGuid[] permitHolders, ZGuid[] permitAddresses)
			{
				Argument.NotNull(factory, nameof(factory));

				if (countryCode.IsEmpty || !transactionDate.IsValid || (permitHolders == null && permitAddresses == null))
				{
					return EmptyAuthorisationHeaders;
				}

				var query = GetCusAuthorisationHeaderQueryBuilder(countryCode)
					.AddTypeFilter(types)
					.AddTransactionDateFilter(transactionDate)
					.AddPermitHoldersFilter(permitHolders)
					.AddAppliesToFilter(permitAddresses)
					.Build();

				return factory.Load<CusAuthorisationHeader>(query);
			}

			public static bool IsDuplicateAuthorisation(CusAuthorisationHeader authorisationToCheck)
			{
				Argument.NotNull(authorisationToCheck, nameof(authorisationToCheck));

				var result = false;
				if (!authorisationToCheck.CPH_IsAdHoc)
				{
					var query = GetCusAuthorisationHeaderQueryBuilder(authorisationToCheck.CPH_RN_NKCountryCode)
						.AddTypeFilter(authorisationToCheck.CPH_Type)
						.AddAuthorisationNumberFilter(authorisationToCheck.CPH_Number)
						.AddPermitHoldersFilter(authorisationToCheck.CPH_OH_PermitHolder)
						.ExcludePk(authorisationToCheck.PK)
						.ExcludeIsAdHoc()
						.Build();

					result = authorisationToCheck.Factory.Exists(typeof(CusAuthorisationHeader), query);
				}

				return result;
			}

			public static bool HolderHasValidAuthorisationWithSpecificRule(BusinessObjectFactory factory, ZGuid permitHolder, ZString countryCode, ZDateTime transactionDate, ZString ruleCode, ZString valueFrom)
			{
				Argument.NotNull(factory, nameof(factory));

				if (permitHolder.IsEmpty || countryCode.IsEmpty || ruleCode.IsEmpty || valueFrom.IsEmpty || !transactionDate.IsValid)
				{
					return false;
				}

				var filter = GetCusAuthorisationHeaderQueryBuilder(countryCode)
					.AddTransactionDateFilter(transactionDate)
					.AddPermitHoldersFilter(permitHolder);

				var query = filter.AddRuleFilter(filter.Build(), ruleCode, valueFrom);

				return factory.Exists(typeof(CusAuthorisationHeader), query);
			}

			public static bool HolderHasSpecificAuthorisationWithRule(BusinessObjectFactory factory, ZGuid permitHolder, ZString authorisationType, ZString authorisationNumber, ZString countryCode, ZDateTime transactionDate, ZString ruleCode, ZString valueFrom)
			{
				Argument.NotNull(factory, nameof(factory));

				if (permitHolder.IsEmpty || authorisationType.IsEmpty || authorisationNumber.IsEmpty || countryCode.IsEmpty || ruleCode.IsEmpty || valueFrom.IsEmpty || !transactionDate.IsValid)
				{
					return false;
				}

				var filter = GetCusAuthorisationHeaderQueryBuilder(countryCode)
					.AddTransactionDateFilter(transactionDate)
					.AddPermitHoldersFilter(permitHolder)
					.AddTypeFilter(authorisationType)
					.AddAuthorisationNumberFilter(authorisationNumber);

				var query = filter.AddRuleFilter(filter.Build(), ruleCode, valueFrom);

				return factory.Exists(typeof(CusAuthorisationHeader), query);
			}

			static CusAuthorisationHeaderQueryBuilder GetCusAuthorisationHeaderQueryBuilder(ZString countryCode) => new CusAuthorisationHeaderQueryBuilder(countryCode);

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CusAuthorisationHeader);
			static CusAuthorisationHeader[] EmptyAuthorisationHeaders => Array.Empty<CusAuthorisationHeader>();
		}

		#endregion

		#region Query Builder
		public class CusAuthorisationHeaderQueryBuilder
		{
			public CusAuthorisationHeaderQueryBuilder(ZString countryCode)
			{
				query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCode);
				query.AddToFilter(CusPermitHeaderSchema.CPH_IsActive, true);
			}
			ZQuery query;

			public CusAuthorisationHeaderQueryBuilder AddCountryCodeFilter(ZString countryCode)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCode);
				return this;
			}

			public CusAuthorisationHeaderQueryBuilder AddTypeFilter(params ZString[] types)
			{
				if (types != null)
				{
					query.AddToFilter(CusPermitHeaderSchema.CPH_Type, types);
				}
				return this;
			}

			public CusAuthorisationHeaderQueryBuilder AddPermitHoldersFilter(params ZGuid[] permitHolders)
			{
				var validHolders = permitHolders?.Where(x => x.IsValid) ?? Enumerable.Empty<ZGuid>();
				if (validHolders.Any())
				{
					query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, validHolders);
				}

				return this;
			}

			public CusAuthorisationHeaderQueryBuilder AddTransactionDateFilter(ZDateTime transactionDate)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, transactionDate);

				var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, transactionDate);
				endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
				query.AddToFilter(endDateQuery);

				return this;
			}

			public CusAuthorisationHeaderQueryBuilder AddAppliesToFilter(params ZGuid[] appliesTo)
			{
				if (appliesTo != null)
				{
					query.AddToFilter(CusPermitHeaderSchema.CPH_OA_AppliesTo, appliesTo);
				}
				return this;
			}

			public CusAuthorisationHeaderQueryBuilder AddAuthorisationNumberFilter(ZString authorisationNumber)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_Number, authorisationNumber);
				return this;
			}

			public CusAuthorisationHeaderQueryBuilder ExcludePk(params ZGuid[] authorisationHeaderPks)
			{
				if (authorisationHeaderPks != null && authorisationHeaderPks.Any())
				{
					query.AddToFilter(CusPermitHeaderSchema.PK, SQLComparisonOperator.NotEqual, authorisationHeaderPks);
				}
				return this;
			}

			public CusAuthorisationHeaderQueryBuilder ExcludeIsAdHoc()
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_IsAdHoc, SQLComparisonOperator.NotEqual, true);
				return this;
			}

			public ZQuery AddRuleFilter(ZQuery filter, ZString ruleCode, params ZString[] ruleValues)
			{
				var query = new ZDBOnlyQuery(typeof(CusAuthorisationHeader));
				query.AddToFilter(filter);

				var ruleSubQuery = new ZDBOnlySubQuery(typeof(CusAuthorisationRule), CusPermitRuleSchema.CPR_CPH_PermitHeader);
				ruleSubQuery.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, ruleCode);
				if (ruleValues != null)
				{
					ruleSubQuery.AddToFilter(CusPermitRuleSchema.CPR_ValueFrom, ruleValues);
				}
				query.AddSubQuery(CusPermitHeaderSchema.PK, ruleSubQuery, JoinCondition.And);
				return query;
			}

			public ZQuery Build()
			{
				var resultQuery = query;
				ResetBuilder();
				return resultQuery;
			}

			void ResetBuilder() => query = new ZQuery();
		}

		#endregion

		#region Properties

		[ResourceStringData("C8C82BC6-0C84-4EA8-BCE1-2D5D4AD6EE2A", Caption = "Authorization Type")]
		[List(nameof(Lookups) + "." + nameof(CusAuthorisationHeaderLookups.AuthorisationTypeList))]
		public override ZString CPH_Type
		{
			get => base.CPH_Type;
			set => base.CPH_Type = value;
		}

		public ZString AuthorizationTypeDescription
		{
			get
			{
				var result = CPH_PermitDescription;
				if (result.IsEmpty)
				{
					result = Lookups.AuthorisationTypeList.GetDescriptionFromCode(CPH_Type);
				}
				return result;
			}
		}

		[ResourceStringData("8E118925-C636-4A93-93FC-292D56A64A19", Caption = "Authorization Holder")]
		public override ZGuid CPH_OH_PermitHolder
		{
			get => base.CPH_OH_PermitHolder;
			set => base.CPH_OH_PermitHolder = value;
		}

		[ResourceStringData("502B1D9A-E8C2-4AC7-9781-B6CE07BF607E", Caption = "Authorization Address")]
		[List(nameof(Lookups) + "." + nameof(CusAuthorisationHeaderLookups.AppliesToList))]
		public override ZGuid CPH_OA_AppliesTo
		{
			get => base.CPH_OA_AppliesTo;
			set => base.CPH_OA_AppliesTo = value;
		}

		[ResourceStringData("A41C8670-5C22-4897-B994-1553E8323F6F", Caption = "Authorization Number")]
		public override ZString CPH_Number
		{
			get => base.CPH_Number;
			set => base.CPH_Number = value;
		}

		[ResourceStringData("DDBBE5C2-25B2-454B-9AB8-7D4CCA4C1099", Caption = "Start Date")]
		public override ZDate CPH_StartDate
		{
			get => base.CPH_StartDate;
			set => base.CPH_StartDate = value;
		}

		[ResourceStringData("38459DD0-0A6C-4BE9-9668-353A03AE316C", Caption = "End Date")]
		public override ZDate CPH_EndDate
		{
			get => base.CPH_EndDate;
			set => base.CPH_EndDate = value;
		}

		[ResourceStringData("EBBA3854-6B63-46A1-8F02-2DCAB3D94178", Caption = "Description")]
		public override ZString CPH_PermitDescription
		{
			get => base.CPH_PermitDescription;
			set => base.CPH_PermitDescription = value;
		}

		public override ZString CPH_RN_NKCountryCode
		{
			get => base.CPH_RN_NKCountryCode;
			set
			{
				if (CPH_RN_NKCountryCode != value)
				{
					base.CPH_RN_NKCountryCode = value;
					provider = null;
				}
			}
		}

		[ReadOnly(true)]
		public override ZBool CPH_IsAdHoc
		{
			get => base.CPH_IsAdHoc;
			set
			{
				base.CPH_IsAdHoc = value;
			}
		}

		[ResourceStringData("AFBB7D1A-64A7-4DA0-922C-02E38D011ECA", Caption = "Is Current")]
		public ZBool IsCurrent => CPH_StartDate <= ZDate.Today && (CPH_EndDate.IsEmpty || CPH_EndDate >= ZDate.Today);

		public override ZString ShortName => Res.GetString("883FDADC-B0C5-4E1C-AA43-CCD3A0862B26", "Authorization");

		[ResourceStringData("8B12ED22-9E3B-4EDA-8065-F0D1F669E0A2", Caption = "Authorization Address")]
		public ZString AuthorizationAddress => AppliesTo?.Address1 ?? ZString.Empty;

		[ResourceStringData("A4E12277-272B-4C86-B45A-40522BC459E9", Caption = "Rule Codes")]
		public ZString CusAuthorisationRuleCodes => string.Join(", ", CusAuthorisationRules.Select(x => x.CPR_RuleCode).OrderBy(x => x));

		public IWhsWarehouse Warehouse => AppliesTo?.GetWhsWarehouse();
		#endregion

		CusAuthorisationRuleCollection cusAuthorisationRules;
		[ChildEditable]
		public CusAuthorisationRuleCollection CusAuthorisationRules
		{
			get
			{
				if (cusAuthorisationRules == null)
				{
					cusAuthorisationRules = new CusAuthorisationRuleCollection(this);
					RegisterEditableChildObject(cusAuthorisationRules);
				}
				return cusAuthorisationRules;
			}
		}

		public CusAuthorisationHeaderProvider Provider => GetProviderCore();

		protected virtual CusAuthorisationHeaderProvider GetProviderCore()
		{
			if (provider == null || provider.CountryCode != CPH_RN_NKCountryCode)
			{
				provider = CusAuthorisationHeaderProvider.GetByCountryCode(CPH_RN_NKCountryCode);
			}
			return provider;
		}
		CusAuthorisationHeaderProvider provider;

		public new CusAuthorisationHeaderValidation Validation => (CusAuthorisationHeaderValidation)base.Validation;

		protected sealed override CusPermitHeaderValidation GetNewValidation() => Provider.GetNewValidation(this);

		public new CusAuthorisationHeaderLookups Lookups => (CusAuthorisationHeaderLookups)base.Lookups;

		protected sealed override CusPermitHeaderLookups GetNewLookups() => Provider.GetNewLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CPH_StartDate = ZDate.Today;
			CPH_EndDate = ZDateTime.MaxSmallDateTimeValue.Date;
		}

		public override void Delete()
		{
			this.DeleteChildren<CusAuthorisationRule>(CusPermitRuleSchema.CPR_CPH_PermitHeader);
			base.Delete();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("253EB362-3F8B-42F7-B92A-106765F325D5", "Authorization");

		protected override bool IsLookupsCachedInBase => false;

		public CustomsNumberViewStmNumsBusinessProvider CustomsNumberProvider
		{
			get
			{
				var providerKey = CustomsNumberProviderKey;
				if (customsNumberProvider == null || customsNumberProvider.ProviderKey != providerKey)
				{
					customsNumberProvider = (CustomsNumberViewStmNumsAuthorisationProvider)CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, providerKey, PK);
				}
				return customsNumberProvider;
			}
		}
		CustomsNumberViewStmNumsAuthorisationProvider customsNumberProvider;

		public string CustomsNumberProviderKey => Provider.GetCustomsNumberProviderKey(CPH_Type);
	}
}
