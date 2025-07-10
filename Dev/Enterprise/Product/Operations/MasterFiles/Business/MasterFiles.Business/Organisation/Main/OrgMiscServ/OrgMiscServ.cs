using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgMiscServ : AutoOrgMiscServ
	{
		public OrgMiscServ(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema Constants for OrgCompanyData Columns

		static class OMConstants
		{
			public const string OM_IMPaymentMethod = "OM_IMPaymentMethod";
			public const string OM_ARAutoUpdateRates = "OM_ARAutoUpdateRates";
			public const string OM_ARCategory = "OM_ARCategory";
			public const string OM_ARCombinedStatementInvoice = "OM_ARCombinedStatementInvoice";
			public const string OM_ARConsolidatedAccountingCategory = "OM_ARConsolidatedAccountingCategory";
			public const string OM_APConsolidatedAccountingCategory = "OM_APConsolidatedAccountingCategory";
			public const string OM_ARCreditLimit = "OM_ARCreditLimit";
			public const string OM_ARCreditRating = "OM_ARCreditRating";
			public const string OM_ARDontShowTaxOnDocs = "OM_ARDontShowTaxOnDocs";
			public const string OM_ARExportAirCollectUplift = "OM_ARExportAirCollectUplift";
			public const string OM_ARExportSeaCollectUplift = "OM_ARExportSeaCollectUplift";
			public const string OM_ARImportAirCollectUplift = "OM_ARImportAirCollectUplift";
			public const string OM_ARImportSeaCollectUplift = "OM_ARImportSeaCollectUplift";
			public const string OM_AROnCreditHold = "OM_AROnCreditHold";
			public const string OM_ARPreviousChequeDrawer = "OM_ARPreviousChequeDrawer";
			public const string OM_ARPreviousChequeDrawerBank = "OM_ARPreviousChequeDrawerBank";
			public const string OM_ARPreviousChequeDrawerBankBranch = "OM_ARPreviousChequeDrawerBankBranch";
			public const string OM_ARReceiptInvoiceAfterPostingDefault = "OM_ARReceiptInvoiceAfterPostingDefault";
			public const string OM_ARTreatDisbursementsAsStandardValue = "OM_ARTreatDisbursementsAsStandardValue";
			public const string OM_ARUseSystemDefaultUplifts = "OM_ARUseSystemDefaultUplifts";
			public const string OM_ARVATSplitPaymentApplicable = "OM_ARVATSplitPaymentApplicable";
			public const string OM_ARWHTApplicable = "OM_ARWHTApplicable";
			public const string OM_OJ_ARDebtorGroup = "OM_OJ_ARDebtorGroup";
			public const string OM_APAccountName = "OM_APAccountName";
			public const string OM_APAutoDirectDebit = "OM_APAutoDirectDebit";
			public const string OM_APBankAccount = "OM_APBankAccount";
			public const string OM_APBankBsb = "OM_APBankBsb";
			public const string OM_APBankName = "OM_APBankName";
			public const string OM_APBankSwift = "OM_APBankSwift";
			public const string OM_APCategory = "OM_APCategory";
			public const string OM_APCreditLimit = "OM_APCreditLimit";
			public const string OM_APPayInvoiceAfterPostingDefault = "OM_APPayInvoiceAfterPostingDefault";
			public const string OM_APWHTApplicable = "OM_APWHTApplicable";
			public const string OM_AB_APDefaultBankAccount = "OM_AB_APDefaultBankAccount";
			public const string OM_AC_APDefaultChargeCode = "OM_AC_APDefaultChargeCode";
			public const string OM_OG_APCreditorGroup = "OM_OG_APCreditorGroup";
			public const string IsImportAirUpliftValid = "IsImportAirUpliftValid";
			public const string IsExportAirUpliftValid = "IsExportAirUpliftValid";
			public const string IsImportSeaUpliftValid = "IsImportSeaUpliftValid";
			public const string IsExportSeaUpliftValid = "IsExportSeaUpliftValid";
			public const string OM_ARGlobalRateBaseText = "OM_ARGlobalRateBaseText";
			public const string ARTemporaryCreditLimitIncrease = "ARTemporaryCreditLimitIncrease";
			public const string ARTemporaryCreditLimit = "ARTemporaryCreditLimit";
			public const string ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime = "ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime";
			public const string VoyageRecyclingPeriodCode = "VoyageRecyclingPeriodCode";
			public const string OM_IMDisallowOrders = "OM_IMDisallowOrders";
		}

		#endregion

		#region Default Values

		public override void OnLoaded()
		{
			base.OnLoaded();

			OriginalOM_CMEstimatedDateToClose = OM_CMEstimatedDateToClose;
			useTransactionCompanyAsPreferredPayment = OM_GC_CMPreferredPaymentCompany.IsEmpty;
		}

		ZDateTime OriginalOM_CMEstimatedDateToClose;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			OM_EXExporterCategory = Constants.AccountsCategory.Standard;

			OM_IMImporterCategory = Constants.AccountsCategory.Standard;
			OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.Default;
			OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Importer;
			OM_IMSendSeaImportDocsTo = OrgConstants.SendDocsTo.Importer;
			OM_IMOriginalSeaBills = (byte)FreightDataRegistry.Instance.ReleaseTypes.Value.OriginalsNumber;
			OM_IMCopySeaBills = (byte)FreightDataRegistry.Instance.ReleaseTypes.Value.CopiesNumber;
			OM_IMOrderStatusCodePairList = new ZBlob(Array.Empty<byte>());
			OM_IMSeaDepotFreeDays = Env.Registry.CFSSeaFreightLCLStorageFreeDays;
			OM_IMAirDepotFreeDays = Env.Registry.CFSAirFreightLCLStorageFreeDays;
			OM_IMDefaultWarehousePickOption = WhsPickOption.Codes.Auto;

			OM_FWAgentCategory = Constants.AccountsCategory.Standard;

			OM_CMClientSize = "";
			OM_CMGrowthOutlook = "";
			OM_CMOverallEffectOfClientOnAirfreightCosts = "";
			OM_CMOverallEffectOfClientOnLCLCosts = "";
			OM_CMOverallEffectOfClientOnTEUCosts = "";
			OM_CMOverallEffectOfClientOnOtherCosts = "";
			OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.DefaultFromRegistry;

			OM_CITypeOfService = "";
			OM_CISellingStyle = "";

			OM_IMDefaultINCOTerm = OrganisationsDataRegistry.Instance.ConsigneeIncoTerm.Value;
			OM_EXDefaultIncoTerm = OrganisationsDataRegistry.Instance.ConsignorIncoTerm.Value;
			OM_EXMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.Default;

			UseTransactionCompanyAsPreferredPayment = true;

			var defaultOSMG = OrganisationRegistry.Instance.OrgSecurityManagementGroupDefault.Value.OrgSecurityGroup;
			if (defaultOSMG != Guid.Empty && null != Factory.Load<GlbGroup>(defaultOSMG))
			{
				OM_GG_OrgSecurityGroup = defaultOSMG;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return (NoResString)"Organisation"; }
		}

		#endregion

		#region Parent Organisation

		OrgHeader fHeader;

		public new OrgHeader Header
		{
			get
			{
				if (fHeader != null && !fHeader.IsDeleted)
				{
					return fHeader;
				}

				var result = ((IBusinessObjectInternals)this).ParentCollections
					.Where(c => c is OrgMiscServCollection)
					.Cast<OrgMiscServCollection>()
					.Select(c => c.Master)
					.FirstOrDefault()
					?? base.Header;

				if (fHeader == null || (result != null && fHeader != result))
				{
					UnRegisterListChangedCalledRefreshBinding(fHeader);
					fHeader = result;
					RegisterListChangedCalledRefreshBinding(fHeader);
				}

				return fHeader;
			}
			set
			{
				if (fHeader == null || fHeader != value)
				{
					UnRegisterListChangedCalledRefreshBinding(fHeader);
					fHeader = value;
					RegisterListChangedCalledRefreshBinding(fHeader);
				}
			}
		}

		#endregion

		#region Related Business Objects

		#region abc

		[List("Lookups.AutoratingDateFilteringList")]
		public override ZString OM_AutoratingDateFiltering
		{
			get { return base.OM_AutoratingDateFiltering; }
			set { base.OM_AutoratingDateFiltering = value; }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public RatingDateConfigCollection RatingDateConfigs
		{
			get
			{
				if (ratingDateConfigs == null)
				{
					if (Header != null)
					{
						ratingDateConfigs = new RatingDateConfigCollection(Header);
						ratingDateConfigs.Load();
						RegisterEditableChildObject(ratingDateConfigs);
					}
				}
				return ratingDateConfigs;
			}
		}
		RatingDateConfigCollection ratingDateConfigs;

		public RatingDateConfigByChargeGroupConfiguration RatingDateConfigByChargeGroupConfiguration => ratingDateConfigByChargeGroupConfiguration ??= new RatingDateConfigByChargeGroupConfiguration(RatingDateConfigs);
		RatingDateConfigByChargeGroupConfiguration ratingDateConfigByChargeGroupConfiguration;

		#endregion

		#region UNDGContact

		public OrgContact UNDGContact
		{
			get { return Factory.Load<OrgContact>(OM_OC_EXDefaultDGContact); }
		}

		#endregion

		#region OrderStatusTypes user defined CodeDescriptionPairList

		public ZBlob OrderStatusList
		{
			get { return OrderStatusTypes.ST_NoteData; }
			set { OrderStatusTypes.ST_NoteData = value; }
		}

		HiddenStmNote OrderStatusTypes
		{
			get
			{
				if (fOrderStatusTypes == null)
				{
					ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, "OrderStatusTypes");
					filter.AddToFilter(StmNoteSchema.ST_ParentID, this.PK);
					//Filter.AddToFilter(StmNoteSchema.ST_Table, this.TableName);
					fOrderStatusTypes = (HiddenStmNote)Factory.LoadTop1(typeof(HiddenStmNote), filter);

					if (fOrderStatusTypes == null)
					{
						fOrderStatusTypes = Factory.New<HiddenStmNote>();

						fOrderStatusTypes.ST_ParentID = this.PK;
						fOrderStatusTypes.ST_Table = this.TableName;
						fOrderStatusTypes.ST_IsCustomDescription = true;
						fOrderStatusTypes.ST_Description = "OrderStatusTypes";
					}

					RegisterEditableChildObject(fOrderStatusTypes);
				}

				return fOrderStatusTypes;
			}
		}

		HiddenStmNote fOrderStatusTypes;

		#endregion

		#region OrderLineStatusTypes user defined CodeDescriptionPairList

		public ZBlob OrderLineStatusList
		{
			get { return OrderLineStatusTypes.ST_NoteData; }
			set { OrderLineStatusTypes.ST_NoteData = value; }
		}

		protected HiddenStmNote OrderLineStatusTypes
		{
			get
			{
				if (fOrderLineStatusTypes == null)
				{
					ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, "OrderLineStatusTypes");
					filter.AddToFilter(StmNoteSchema.ST_ParentID, this.PK);
					//Filter.AddToFilter(StmNoteSchema.ST_Table, this.TableName);
					fOrderLineStatusTypes = (HiddenStmNote)Factory.LoadTop1(typeof(HiddenStmNote), filter);

					if (fOrderLineStatusTypes == null)
					{
						fOrderLineStatusTypes = Factory.New<HiddenStmNote>();

						fOrderLineStatusTypes.ST_ParentID = this.PK;
						fOrderLineStatusTypes.ST_Table = this.TableName;
						fOrderLineStatusTypes.ST_IsCustomDescription = true;
						fOrderLineStatusTypes.ST_Description = "OrderLineStatusTypes";
					}

					RegisterEditableChildObject(fOrderLineStatusTypes);
				}

				return fOrderLineStatusTypes;
			}
		}

		HiddenStmNote fOrderLineStatusTypes;

		#endregion

		#region ClientDocumentLogo

		public ZBlob ClientDocumentLogo
		{
			get { return (ClientDocumentLogoNote != null && !ClientDocumentLogoNote.IsDeleted) ? ClientDocumentLogoNote.ST_NoteData : ZBlob.Empty; }
			set
			{
				if (value.IsEmpty && ClientDocumentLogoNote != null && !ClientDocumentLogoNote.IsDeleted)
				{
					ClientDocumentLogoNote.Delete();
					ClientDocumentLogoNote = null;
				}
				else if (!value.IsEmpty)
				{
					if (ClientDocumentLogoNote == null || ClientDocumentLogoNote.IsDeleted)
					{
						ClientDocumentLogoNote = CreateNewClientDocumentLogoNote();
					}

					// Final check to ensure we are playing with valid image data before saving it
					try
					{
						Image testImage = Image.FromStream(new System.IO.MemoryStream(value));
					}
					catch (Exception exception)
					{
						if (exception.IsCriticalException())
						{ throw; }
						else
						{ throw new ArgumentException("Invalid image data"); }
					}
					ClientDocumentLogoNote.ST_NoteData = value;
				}
			}
		}

		HiddenStmNote CreateNewClientDocumentLogoNote()
		{
			var logoNote = Factory.New<HiddenStmNote>();
			RegisterEditableChildObject(logoNote);
			logoNote.ST_ParentID = this.PK;
			logoNote.ST_Table = this.TableName;
			logoNote.ST_IsCustomDescription = true;
			logoNote.ST_Description = "ClientDocumentLogo";
			return logoNote;
		}

		HiddenStmNote ClientDocumentLogoNote
		{
			set { clientDocumentLogoNote = value; }
			get
			{
				if (clientDocumentLogoNote == null)
				{
					ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, "ClientDocumentLogo");
					filter.AddToFilter(StmNoteSchema.ST_ParentID, this.PK);
					//Filter.AddToFilter(StmNoteSchema.ST_Table, this.TableName);
					clientDocumentLogoNote = Factory.LoadTop1<HiddenStmNote>(filter);
					RegisterEditableChildObject(clientDocumentLogoNote);
				}
				return clientDocumentLogoNote;
			}
		}

		HiddenStmNote clientDocumentLogoNote;

		#endregion

		#region Carrier Service Levels

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get
			{
				if (fCarrierServiceLevels == null)
				{
					var localCarrierServiceLevels = new OrgCarrierServiceLevelCollection(this);
					localCarrierServiceLevels.Load();
					fCarrierServiceLevels = localCarrierServiceLevels;
					RegisterEditableChildObject(fCarrierServiceLevels);
					if (Header != null)
					{
						fCarrierServiceLevels.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyCarrierSecurity);
					}
				}

				return fCarrierServiceLevels;
			}
		}
		OrgCarrierServiceLevelCollection fCarrierServiceLevels;

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			CarrierServiceLevels.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Lookups

		#region BankAccounts

		public AccBankAccountCollection BankAccounts
		{
			get { return Header.CompanyData.Lookups.APDefaultBankAccounts; }
		}

		#endregion

		#region CreditorGroups

		public OrgCreditorGroupCollection CreditorGroups
		{
			get { return Header.CompanyData.Lookups.APCreditorGroups; }
		}

		#endregion

		#region Brokers

		public BrokerCollection Brokers
		{
			get
			{
				if (fBrokers == null)
				{
					fBrokers = new BrokerCollection(Factory);
				}
				return fBrokers;
			}
		}

		BrokerCollection fBrokers;

		#endregion

		#region ChargeCodes

		public AccChargeCodeCollection ChargeCodes
		{
			get { return Header.CompanyData.Lookups.APDefaultChargeCodes; }
		}

		#endregion

		#region Countries

		public RefCountryCollection Countries
		{
			get
			{
				if (fCountries == null)
				{
					fCountries = new RefCountryCollection(Factory);
				}
				return fCountries;
			}
		}

		RefCountryCollection fCountries;

		#endregion

		#region CommodityCodes

		public RefCommodityCodeCollection CommodityCodes
		{
			get
			{
				if (fCommodityCodes == null)
				{
					fCommodityCodes = new RefCommodityCodeCollection(Factory);
				}
				return fCommodityCodes;
			}
		}

		RefCommodityCodeCollection fCommodityCodes;

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}
				return fCurrencies;
			}
		}

		RefCurrencyCollection fCurrencies;

		#endregion

		#region DebtorGroups

		public OrgDebtorGroupCollection DebtorGroups
		{
			get
			{
				if (fDebtorGroups == null)
				{
					fDebtorGroups = new OrgDebtorGroupCollection(Factory);
				}
				return fDebtorGroups;
			}
		}

		OrgDebtorGroupCollection fDebtorGroups;

		#endregion

		#region Debtors

		public DebtorCollection Debtors
		{
			get
			{
				if (fDebtors == null)
				{
					fDebtors = new DebtorCollection(Header.ReadOnlyFactory);
				}
				return fDebtors;
			}
		}

		DebtorCollection fDebtors;

		#endregion

		#region Currency

		public RefCurrency GlobalCreditCurrency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, ARGlobalCreditCurrencyCode);

		public int GlobalCreditCurrencyDecimals => GlobalCreditCurrency?.Decimals ?? LocalCurrencyDecimals;

		public int LocalCurrencyDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		#region LocalTransports

		public LocalTransportCollection LocalTransports
		{
			get
			{
				if (fLocalTransports == null)
				{
					fLocalTransports = new LocalTransportCollection(Factory);
				}
				return fLocalTransports;
			}
		}

		LocalTransportCollection fLocalTransports;

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region Organisations

		public OrganisationsFindBoxCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrganisationsFindBoxCollection(Factory);
				}
				return fOrganisations;
			}
		}

		OrganisationsFindBoxCollection fOrganisations;

		#endregion

		#region SettlementGroups

		#region ARSettlementGroups

		public OrganisationsFindBoxCollection ARSettlementGroups
		{
			get
			{
				if (fARSettlementGroups == null)
				{
					fARSettlementGroups = new OrganisationsFindBoxCollection(Factory);
				}
				return fARSettlementGroups;
			}
		}

		OrganisationsFindBoxCollection fARSettlementGroups;

		#endregion

		#region APSettlementGroups

		public OrganisationsFindBoxCollection APSettlementGroups
		{
			get
			{
				if (fAPSettlementGroups == null)
				{
					fAPSettlementGroups = new OrganisationsFindBoxCollection(Factory);
				}
				return fAPSettlementGroups;
			}
		}

		OrganisationsFindBoxCollection fAPSettlementGroups;

		#endregion

		#endregion

		#region Forwarder Groupings

		#region FWManagementGroupings

		public OrganisationsFindBoxCollection FWManagementGroupings
		{
			get
			{
				if (fFWManagementGroupings == null)
				{
					fFWManagementGroupings = new OrganisationsFindBoxCollection(Factory);
				}
				return fFWManagementGroupings;
			}
		}

		OrganisationsFindBoxCollection fFWManagementGroupings;

		#endregion

		#region FWARGroupings

		public DebtorCollection FWARGroupings
		{
			get
			{
				if (fFWARGroupings == null)
				{
					fFWARGroupings = new DebtorCollection(Header.ReadOnlyFactory);
				}
				return fFWARGroupings;
			}
		}

		DebtorCollection fFWARGroupings;

		#endregion

		#region FWAPGroupings

		public CreditorCollection FWAPGroupings
		{
			get
			{
				if (fFWAPGroupings == null)
				{
					fFWAPGroupings = new CreditorCollection(Header.ReadOnlyFactory);
				}
				return fFWAPGroupings;
			}
		}

		CreditorCollection fFWAPGroupings;

		#endregion

		#endregion

		#region Depots

		#region AirDepots

		public DepotCollection AirDepots
		{
			get
			{
				if (fAirDepots == null)
				{
					fAirDepots = new DepotCollection(Factory);
				}
				return fAirDepots;
			}
		}

		DepotCollection fAirDepots;

		#endregion

		#region SeaDepots

		public DepotCollection SeaDepots
		{
			get
			{
				if (fSeaDepots == null)
				{
					fSeaDepots = new DepotCollection(Factory);
				}
				return fSeaDepots;
			}
		}

		DepotCollection fSeaDepots;

		#endregion

		#endregion

		#region ControllingAgents

		public ForwarderCollection CMControllingAgents
		{
			get
			{
				if (fCMControllingAgents == null)
				{
					fCMControllingAgents = new ForwarderCollection(Factory);
				}
				return fCMControllingAgents;
			}
		}

		ForwarderCollection fCMControllingAgents;

		#endregion

		#region CompetitorsBrokersAndForwarders

		public OrganisationsFindBoxCollection CompetitorsBrokersAndForwarders
		{
			get
			{
				if (fCompetitorsBrokersAndForwarders == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsCompetitor, ZBool.True);
					filter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsForwarder, SQLComparisonOperator.Equal, ZBool.True);
					filter.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsBroker, SQLComparisonOperator.Equal, ZBool.True);
					fCompetitorsBrokersAndForwarders = new OrganisationsFindBoxCollection(Factory, filter);

					fCompetitorsBrokersAndForwarders.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property10", ZBool.True));
					fCompetitorsBrokersAndForwarders.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
					fCompetitorsBrokersAndForwarders.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property8", ZBool.True));

					fCompetitorsBrokersAndForwarders.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("bbda2c1c-1fff-43c6-83bc-cb8a477ffbd5", "An organization chosen here must have an organization type of Competitor, Broker or Forwarder selected."));
				}

				return fCompetitorsBrokersAndForwarders;
			}
		}

		OrganisationsFindBoxCollection fCompetitorsBrokersAndForwarders;

		#endregion

		#region Warehouse

		protected virtual ZQuery WarehouseFilterQuery()
		{
			return new ZQuery(OrgHeaderSchema.OH_IsWarehouseClient, ZBool.True);
		}

		public OrganisationsFindBoxCollection Warehouses
		{
			get
			{
				if (warehouses == null)
				{
					ZQuery filter = WarehouseFilterQuery();
					warehouses = new OrganisationsFindBoxCollection(Factory, filter);
					warehouses.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property7", ZBool.True));
					warehouses.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("8DDD85CB-1EBC-446D-9700-E66CBC99D413", "An Organization selected from here must have an Organization Type of Warehouse selected."));
				}
				return warehouses;
			}
		}
		OrganisationsFindBoxCollection warehouses;

		#endregion

		#region Staff

		public GlbStaffCollection Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = new GlbStaffCollection(Factory);
				}
				return fStaff;
			}
		}

		GlbStaffCollection fStaff;

		#endregion

		#region ServiceLevels

		public RefServiceLevelCollection ServiceLevels
		{
			get
			{
				if (fServiceLevels == null)
				{
					fServiceLevels = new RefServiceLevelCollection(Factory);
				}
				return fServiceLevels;
			}
		}

		RefServiceLevelCollection fServiceLevels;

		#endregion

		#region Receivables (from CompanyData)

		#region OM_ARConsolidatedAccountingCategory_List

		public ReadOnlyCodeDescriptionPairList OM_ARConsolidatedAccountingCategory_List
		{
			get { return Header.CompanyData.Lookups.OB_ARConsolidatedAccountingCategory_List; }
		}

		#endregion

		#region OM_ARCategory_List

		public ReadOnlyCodeDescriptionPairList OM_ARCategory_List
		{
			get { return Header.CompanyData.Lookups.OB_ARCategory_List; }
		}

		#endregion

		#region OM_ARCreditRating_List

		public ReadOnlyCodeDescriptionPairList OM_ARCreditRating_List
		{
			get { return Header.CompanyData.Lookups.OB_ARCreditRating_List; }
		}

		#endregion

		#endregion

		#region Payables (from CompanyData)

		public ReadOnlyCodeDescriptionPairList OM_APCategory_List
		{
			get { return Header.CompanyData.Lookups.OB_APCategory_List; }
		}

		#endregion

		#region Consignee

		public CodeDescriptionPairList OM_IMMergeCustomsInvoiceLinesBy_List
		{
			get
			{
				var countryCode = Header?.MainAddress?.OA_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var cacheKey = ZString.Format("IMMergeCustomsInvoiceLinesBy_List_{0}", countryCode);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					return OrgCodeLists.GetMergeInvoiceLinesByListByCountryCode(countryCode);
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList OM_IMImporterCategory_List
		{
			get
			{
				if (fOM_IMImporterCategory_List == null)
				{
					fOM_IMImporterCategory_List = Env.Registry.ImporterCategoryList;
				}
				return fOM_IMImporterCategory_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fOM_IMImporterCategory_List;

		public CodeDescriptionPairList OM_IMPaymentMethod_List
		{
			get { return OrgCodeLists.PaymentMethod_List; }
		}

		public CodeDescriptionPairList OM_IMSendImportDocsTo_List
		{
			get { return OrgCodeLists.SendImportDocsTo_List; }
		}

		public CodeDescriptionPairList OM_IMSendSeaImportDocsTo_List
		{
			get { return OrgCodeLists.SendImportDocsTo_List; }
		}

		public CodeDescriptionPairList OM_IMAutoPopulateOwnerRefList
		{
			get
			{
				if (fOM_IMAutoPopulateOwnerRefList == null)
				{
					fOM_IMAutoPopulateOwnerRefList = new PopulateOwnerRefList();
				}
				return fOM_IMAutoPopulateOwnerRefList;
			}
		}
		PopulateOwnerRefList fOM_IMAutoPopulateOwnerRefList;

		FCLEquipmentNeededList fFCLEquipmentNeeded_List;
		public CodeDescriptionPairList OM_IMFCLEquipmentNeeded_List
		{
			get
			{
				if (fFCLEquipmentNeeded_List == null)
				{
					fFCLEquipmentNeeded_List = new FCLEquipmentNeededList();
				}
				return fFCLEquipmentNeeded_List;
			}
		}

		LCLAIREquipmentNeededList fLCLAIREquipmentNeeded_List;
		public CodeDescriptionPairList OM_IMLCLEquipmentNeeded_List
		{
			get
			{
				if (fLCLAIREquipmentNeeded_List == null)
				{
					fLCLAIREquipmentNeeded_List = new LCLAIREquipmentNeededList();
				}
				return fLCLAIREquipmentNeeded_List;
			}
		}

		public CodeDescriptionPairList OM_IMAirEquipmentNeeded_List
		{
			get
			{
				if (fLCLAIREquipmentNeeded_List == null)
				{
					fLCLAIREquipmentNeeded_List = new LCLAIREquipmentNeededList();
				}
				return fLCLAIREquipmentNeeded_List;
			}
		}

		public CodeDescriptionPairList OM_IMDocumentAddressPreference_List
		{
			get
			{
				if (fOM_IMDocumentAddressPreference_List == null)
				{
					fOM_IMDocumentAddressPreference_List = new IMDocumentAddressPreferenceCodeDescriptionPairList();
				}
				return fOM_IMDocumentAddressPreference_List;
			}
		}
		CodeDescriptionPairList fOM_IMDocumentAddressPreference_List;

		public CodeDescriptionPairList OM_IMDefaultINCOTerm_List
		{
			get
			{
				if (fOM_IMDefaultINCOTerm_List == null)
				{
					fOM_IMDefaultINCOTerm_List = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
					fOM_IMDefaultINCOTerm_List.AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));
				}
				return fOM_IMDefaultINCOTerm_List;
			}
		}
		CodeDescriptionPairList fOM_IMDefaultINCOTerm_List;

		#endregion

		#region Consignor

		public CodeDescriptionPairList OM_EXDocumentAddressPreference_List
		{
			get
			{
				if (fOM_EXDocumentAddressPreference_List == null)
				{
					fOM_EXDocumentAddressPreference_List = new EXDocumentAddressPreferenceCodeDescriptionPairList();
				}
				return fOM_EXDocumentAddressPreference_List;
			}
		}
		CodeDescriptionPairList fOM_EXDocumentAddressPreference_List;

		CodeDescriptionPairList fOM_EXDefaultIncoTerm_List;
		public CodeDescriptionPairList OM_EXDefaultIncoTerm_List
		{
			get
			{
				if (fOM_EXDefaultIncoTerm_List == null)
				{
					fOM_EXDefaultIncoTerm_List = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
					fOM_EXDefaultIncoTerm_List.AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));
				}
				return fOM_EXDefaultIncoTerm_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fOM_EXExporterCategory_List;
		public ReadOnlyCodeDescriptionPairList OM_EXExporterCategory_List
		{
			get
			{
				if (fOM_EXExporterCategory_List == null)
				{
					fOM_EXExporterCategory_List = Env.Registry.ExporterCategoryList;
				}
				return fOM_EXExporterCategory_List;
			}
		}

		public CodeDescriptionPairList OM_EXFCLEquipmentNeeded_List
		{
			get { return OM_IMFCLEquipmentNeeded_List; }
		}

		public CodeDescriptionPairList OM_EXLCLEquipmentNeeded_List
		{
			get { return OM_IMLCLEquipmentNeeded_List; }
		}

		public CodeDescriptionPairList OM_EXAirEquipmentNeeded_List
		{
			get { return OM_IMAirEquipmentNeeded_List; }
		}

		public CodeDescriptionPairList OM_EXInvPriceFromLastCost_List
		{
			get
			{
				if (fOM_EXInvPriceFromLastCost_List == null)
				{
					fOM_EXInvPriceFromLastCost_List = new PopulateInvPriceFromLastCostList();
				}
				return fOM_EXInvPriceFromLastCost_List;
			}
		}
		PopulateInvPriceFromLastCostList fOM_EXInvPriceFromLastCost_List;

		#endregion

		#region Forwarder

		ReadOnlyCodeDescriptionPairList fOM_FWAgentCategory_List;
		public ReadOnlyCodeDescriptionPairList OM_FWAgentCategory_List
		{
			get
			{
				if (fOM_FWAgentCategory_List == null)
				{
					fOM_FWAgentCategory_List = Env.Registry.AgentCategoryList;
				}
				return fOM_FWAgentCategory_List;
			}
		}

		#endregion

		#region Carrier

		ReadOnlyCodeDescriptionPairList fOM_CRCarrierCategory_List;
		public ReadOnlyCodeDescriptionPairList OM_CRCarrierCategory_List
		{
			get
			{
				if (fOM_CRCarrierCategory_List == null)
				{
					fOM_CRCarrierCategory_List = Env.Registry.CarrierCategoryList;
				}
				return fOM_CRCarrierCategory_List;
			}
		}

		#endregion

		#region Services

		ReadOnlyCodeDescriptionPairList fOM_SVServicesCategory_List;
		public ReadOnlyCodeDescriptionPairList OM_SVServicesCategory_List
		{
			get
			{
				if (fOM_SVServicesCategory_List == null)
				{
					fOM_SVServicesCategory_List = Env.Registry.ServicesCategoryList;
				}
				return fOM_SVServicesCategory_List;
			}
		}

		#endregion

		#region Sales

		#region OM_CMSalesCategory_List

		ReadOnlyCodeDescriptionPairList fOM_CMSalesCategory_List;
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

		#endregion

		#region OM_CMClientSize_List

		ReadOnlyCodeDescriptionPairList fOM_CMClientSize_List;
		public ReadOnlyCodeDescriptionPairList OM_CMClientSize_List
		{
			get
			{
				if (fOM_CMClientSize_List == null)
				{
					fOM_CMClientSize_List = OrganisationsDataRegistry.Instance.ClientSizeList.Value;
				}
				return fOM_CMClientSize_List;
			}
		}

		#endregion

		#region OM_CMCompetitorActivity_List

		ReadOnlyCodeDescriptionPairList fOM_CMCompetitorActivity_List;
		public ReadOnlyCodeDescriptionPairList OM_CMCompetitorActivity_List
		{
			get
			{
				if (fOM_CMCompetitorActivity_List == null)
				{
					fOM_CMCompetitorActivity_List = Env.Registry.CompetitorActivityList;
				}
				return fOM_CMCompetitorActivity_List;
			}
		}

		#endregion

		#region OM_CMGrowthOutlook_List

		ReadOnlyCodeDescriptionPairList fOM_CMGrowthOutlook_List;
		public ReadOnlyCodeDescriptionPairList OM_CMGrowthOutlook_List
		{
			get
			{
				if (fOM_CMGrowthOutlook_List == null)
				{
					fOM_CMGrowthOutlook_List = Env.Registry.SalesGrowthOutlookList;
				}
				return fOM_CMGrowthOutlook_List;
			}
		}

		#endregion

		#region OM_CMOverallEffectOfClient_List

		ReadOnlyCodeDescriptionPairList fOM_CMOverallEffectOfClient_List;
		public ReadOnlyCodeDescriptionPairList OM_CMOverallEffectOfClient_List
		{
			get
			{
				if (fOM_CMOverallEffectOfClient_List == null)
				{
					fOM_CMOverallEffectOfClient_List = Env.Registry.SalesEffectOnCostList;
				}
				return fOM_CMOverallEffectOfClient_List;
			}
		}

		#endregion

		#region OM_CMClientRanking_List

		CodeDescriptionPairList fOM_CMClientRanking_List;
		public CodeDescriptionPairList OM_CMClientRanking_List
		{
			get
			{
				if (fOM_CMClientRanking_List == null)
				{
					fOM_CMClientRanking_List = new CodeDescriptionPairList(OLookUpEditType.Numbers1To10);
				}
				return fOM_CMClientRanking_List;
			}
		}

		#endregion

		#region OM_CMOverallClientRelation_List

		public CodeDescriptionPairList OM_CMOverallClientRelation_List
		{
			get { return OrgCodeLists.ZeroToTen_List; }
		}

		#endregion

		#region OM_CMClientsDesireToRemain_List

		public CodeDescriptionPairList OM_CMClientsDesireToRemain_List
		{
			get { return OrgCodeLists.ZeroToTen_List; }
		}

		#endregion

		#region OM_CMEaseClientCanBePoached_List

		public CodeDescriptionPairList OM_CMEaseClientCanBePoached_List
		{
			get { return OrgCodeLists.ZeroToTen_List; }
		}

		#endregion

		#region OM_CMAmountOfElectronicIntegration_List

		public CodeDescriptionPairList OM_CMAmountOfElectronicIntegration_List
		{
			get { return OrgCodeLists.ZeroToTen_List; }
		}

		#endregion

		#region OM_CMSalesTerritory_List

		ReadOnlyCodeDescriptionPairList fOM_CMSalesTerritory_List;
		public ReadOnlyCodeDescriptionPairList OM_CMSalesTerritory_List
		{
			get
			{
				if (fOM_CMSalesTerritory_List == null)
				{
					fOM_CMSalesTerritory_List = Env.Registry.SalesTerritoryList;
				}
				return fOM_CMSalesTerritory_List;
			}
		}

		#endregion

		#region

		public CodeDescriptionPairList OM_CMIndustryVertical_List
		{
			get
			{
				return Factory.GetCachedValue("OrgMiscServ.OM_CMIndustryVertical_List", () =>
				{
					return OrganisationsDataRegistry.Instance.IndustryVerticalTypes.Value.GetCodeDescriptionPairList();
				});
			}
		}

		public CodeDescriptionPairList OM_CMIndustryVertical_ActiveList
		{
			get
			{
				return Factory.GetCachedValue("OrgMiscServ.OM_CMIndustryVertical_ActiveList", () =>
				{
					var result = OrganisationsDataRegistry.Instance.IndustryVerticalTypes.Value.GetActiveCodeDescriptionPairList();
					result.SortByDescription();
					return result;
				});
			}
		}

		#endregion

		#region OM_CMPeriodOfActivity_List

		CodeDescriptionPairList fOM_CMPeriodOfActivity_List;
		public CodeDescriptionPairList OM_CMPeriodOfActivity_List
		{
			get
			{
				if (fOM_CMPeriodOfActivity_List == null)
				{
					fOM_CMPeriodOfActivity_List = OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.Value.GetCodeDescriptionPairList();
				}
				return fOM_CMPeriodOfActivity_List;
			}
		}

		CodeDescriptionPairList fOM_CMPeriodOfActivity_ActiveList;
		public CodeDescriptionPairList OM_CMPeriodOfActivity_ActiveList
		{
			get
			{
				if (fOM_CMPeriodOfActivity_ActiveList == null)
				{
					fOM_CMPeriodOfActivity_ActiveList = OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.Value.GetActiveCodeDescriptionPairList();
					fOM_CMPeriodOfActivity_ActiveList.Sort();
				}
				return fOM_CMPeriodOfActivity_ActiveList;
			}
		}

		#endregion

		#endregion

		#region Competitor

		#region Type of Service

		ReadOnlyCodeDescriptionPairList fOM_CITypeOfService_List;
		public ReadOnlyCodeDescriptionPairList OM_CITypeOfService_List
		{
			get
			{
				if (fOM_CITypeOfService_List == null)
				{
					fOM_CITypeOfService_List = Env.Registry.CompetitorActivityList;
				}
				return fOM_CITypeOfService_List;
			}
		}

		#endregion

		#region Selling Style

		ReadOnlyCodeDescriptionPairList fOM_CISellingStyle_List;
		public ReadOnlyCodeDescriptionPairList OM_CISellingStyle_List
		{
			get
			{
				if (fOM_CISellingStyle_List == null)
				{
					fOM_CISellingStyle_List = Env.Registry.SalesStyleList;
				}
				return fOM_CISellingStyle_List;
			}
		}

		#endregion

		#region Category

		ReadOnlyCodeDescriptionPairList fOM_CICompetitorCategory_List;
		public ReadOnlyCodeDescriptionPairList OM_CICompetitorCategory_List
		{
			get
			{
				if (fOM_CICompetitorCategory_List == null)
				{
					fOM_CICompetitorCategory_List = Env.Registry.CompetitorCategoryList;
				}
				return fOM_CICompetitorCategory_List;
			}
		}

		#endregion

		#region OM_CICompetitiveRanking_List

		public CodeDescriptionPairList OM_CICompetitiveRanking_List
		{
			get { return OrgCodeLists.ZeroToTen_List; }
		}

		#endregion

		#endregion

		#region EDI Export Mapping Address Type List

		CodeDescriptionPairList fEDIExportMappingAddressConfigurationList;
		[Obsolete("Use EDICommunicationsModeLookupsList")]
		public CodeDescriptionPairList EDIExportMappingAddressConfigurationList
		{
			get
			{
				if (fEDIExportMappingAddressConfigurationList == null)
				{
					fEDIExportMappingAddressConfigurationList = new EDICommunicationsModeCommunicationsTransportList();
				}
				return fEDIExportMappingAddressConfigurationList;
			}
		}

		#endregion

		#region WarehousePickOptionList

		public WhsPickOption WarehousePickOptionList
		{
			get { return warehousePickOptionList ?? (warehousePickOptionList = new WhsPickOption()); }
		}
		WhsPickOption warehousePickOptionList;

		#endregion

		#region WarehousePickModeList

		public WhsPickMode WarehousePickModeList
		{
			get { return warehousePickModeList ?? (warehousePickModeList = new WhsPickMode()); }
		}
		WhsPickMode warehousePickModeList;

		#endregion

		#region DistanceCalculation

		public CodeDescriptionPairList OM_CMDistanceCalculationProvider_List
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				string codeFromReg = DistanceCalculationRegistry.Instance.DistanceCalculationProviderConfigurationItem.Value.Provider;
				string defaultDescriptionFromReg = DistanceCalculationLists.Instance.Providers.GetDescriptionFromCode(codeFromReg);
				result.AddPair(DistanceCalculationConstants.Providers.DefaultFromRegistry, Res.GetString("2141498a-6570-4f32-96c0-635f06667411", "Default from Registry - {0}", defaultDescriptionFromReg));
				result.AddRange(DistanceCalculationLists.Instance.Providers);

				return result;
			}
		}

		public CodeDescriptionPairList OM_CMDistanceCalculationVersion_List
		{
			get { return DistanceCalculationLists.Instance.Versions(OM_CMDistanceCalculationProvider); }
		}

		public CodeDescriptionPairList OM_CMDistanceCalculationMethod_List
		{
			get { return DistanceCalculationLists.Instance.CalculationMethods(OM_CMDistanceCalculationProvider); }
		}

		#endregion

		#region VoyageRecyclingPeriodList

		public VoyageRecyclingPeriodList VoyageRecyclingPeriodList
		{
			get { return voyageRecyclingPeriodList ?? (voyageRecyclingPeriodList = new VoyageRecyclingPeriodList(true)); }
		}

		VoyageRecyclingPeriodList voyageRecyclingPeriodList;

		#endregion

		#region OM_CarrierPackageGrouping_List

		public CarrierPackageGroupingList OM_CarrierPackageGrouping_List
		{
			get { return fOM_CarrierPackageGrouping ?? (fOM_CarrierPackageGrouping = new CarrierPackageGroupingList()); }
		}

		CarrierPackageGroupingList fOM_CarrierPackageGrouping;

		#endregion

		#region OM_FWAgentPackageGrouping_List

		public FWAgentPackageGroupingList OM_FWAgentPackageGrouping_List
		{
			get { return fOM_FWAgentPackageGrouping_List ?? (fOM_FWAgentPackageGrouping_List = new FWAgentPackageGroupingList()); }
		}

		FWAgentPackageGroupingList fOM_FWAgentPackageGrouping_List;

		#endregion

		#endregion

		#region Warehouse Part Attribute Rules

		PartAttributeTypeList fPartAttributeRuleList;
		public PartAttributeTypeList PartAttributeRuleList
		{
			get
			{
				if (fPartAttributeRuleList == null)
				{
					fPartAttributeRuleList = new PartAttributeTypeList();
				}
				return fPartAttributeRuleList;
			}
		}

		#endregion

		#region UNDGContacts

		public OrgContactDependentCollection UNDGContactList
		{
			get { return new OrgContactDependentCollection(Header, Factory); }
		}

		#endregion

		#region Warehouse Unique Ref Strategy Types

		CodeDescriptionPairList fWhsUniqueRefStrategyTypeList;

		public CodeDescriptionPairList WhsUniqueRefStrategyTypesList
		{
			get
			{
				if (fWhsUniqueRefStrategyTypeList == null)
				{
					fWhsUniqueRefStrategyTypeList = new WhsUniqueRefStrategyTypeList();
				}
				return fWhsUniqueRefStrategyTypeList;
			}
		}

		#endregion

		#region PhoneTypes

		CodeDescriptionPairList fPhoneTypeList;
		public CodeDescriptionPairList PhoneTypesList
		{
			get
			{
				if (fPhoneTypeList == null)
				{
					fPhoneTypeList = new PhoneTypeList();
				}
				return fPhoneTypeList;
			}
		}

		#endregion

		#region Putaway Algorithms

		CodeDescriptionPairList putawayAlgorithmList;
		public CodeDescriptionPairList PutawayAlgorithmList
		{
			get { return putawayAlgorithmList ?? (putawayAlgorithmList = new WhsPutawayAlgorithmList()); }
		}

		#endregion

		#region Pick Algorithms

		CodeDescriptionPairList pickAlgorithmList;
		public CodeDescriptionPairList PickAlgorithmList
		{
			get { return pickAlgorithmList ?? (pickAlgorithmList = new WhsPickAlgorithmList()); }
		}

		#endregion

		#region New Properties

		public ZBool IsAirline
		{
			get { return Header != null && Header.OH_IsShippingProvider && Header.OH_IsAirLine; }
		}

		public ZBool IsCompetitor
		{
			get { return Header != null && Header.OH_IsCompetitor; }
		}

		public ZBool IsSalesLead
		{
			get { return Header != null && Header.OH_IsSalesLead; }
		}

		public ZBool IsGSTVATDeferred
		{
			get { return OM_IMIsGSTDeferred; }
		}

		public OrgDebtorGroupCollection SingleOrgDebtor
		{
			get { return Header.CompanyData.SingleOrgDebtor; }
		}

		#region VoyageRecyclingPeriodCode

		[List("VoyageRecyclingPeriodList")]
		[ReadOnlyMember(nameof(VoyageRecyclingIsReadOnly))]
		public ZString VoyageRecyclingPeriodCode
		{
			get
			{
				if (!voyageRecyclingPeriodCode.HasValue)
				{
					voyageRecyclingPeriodCode = VoyageRecyclingPeriodList.GetCodeFromAmount(OM_CRVoyageRecyclingPeriodInMonths);
				}

				return voyageRecyclingPeriodCode.Value;
			}
			set
			{
				voyageRecyclingPeriodCode = value;
				OM_CRVoyageRecyclingPeriodInMonths = VoyageRecyclingPeriodList.GetAmountFromCode(value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateVoyageRecyclingPeriodCode();
				}

				VoyageRecyclingPeriodCodeInfo.RefreshBinding();
			}
		}

		ZString? voyageRecyclingPeriodCode;

		public ZPropertyInfo VoyageRecyclingPeriodCodeInfo
		{
			get { return GetZPropertyInfo(OMConstants.VoyageRecyclingPeriodCode); }
		}

		protected bool VoyageRecyclingIsReadOnly
		{
			get { return Header == null || Header.CanNotBeLinkedToShippingLine; }
		}

		#endregion

		#region DGPhoneNumber

		public ZString DGPhoneNumber
		{
			get
			{
				var undgContact = UNDGContact;
				return undgContact != null
					? undgContact.GetNumberForPhoneType(OM_EXDefaultDGContactPhoneUsed)
					: ZString.Empty;
			}
		}

		public ZPropertyInfo DGPhoneNumberInfo => GetZPropertyInfo(nameof(DGPhoneNumber));

		public ZString DGPhoneNumber_Formatted
		{
			get
			{
				var undgContact = UNDGContact;
				return undgContact != null
					? undgContact.GetNumberForPhoneType(OM_EXDefaultDGContactPhoneUsed, true)
					: ZString.Empty;
			}
		}

		public ZPropertyInfo DGPhoneNumber_FormattedInfo => GetZPropertyInfo(nameof(DGPhoneNumber_Formatted));

		#endregion

		#region Properties that are now on OrgCompanyData

		#region IsImportAirUpliftValid

		[BusinessObjectTestExclude()]
		public ZBool IsImportAirUpliftValid
		{
			get { return Header.CompanyData.IsImportAirUpliftValid; }
			set { Header.CompanyData.IsImportAirUpliftValid = value; }
		}

		public ZPropertyInfo IsImportAirUpliftValidInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsImportAirUpliftValid), x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.IsImportAirUpliftValidInfo)); }
		}

		#endregion

		#region IsExportAirUpliftValid

		[BusinessObjectTestExclude()]
		public ZBool IsExportAirUpliftValid
		{
			get { return Header.CompanyData.IsExportAirUpliftValid; }
			set { Header.CompanyData.IsExportAirUpliftValid = value; }
		}

		public ZPropertyInfo IsExportAirUpliftValidInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsExportAirUpliftValid), x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.IsExportAirUpliftValidInfo)); }
		}

		#endregion

		#region IsImportSeaUpliftValid

		[BusinessObjectTestExclude()]
		public ZBool IsImportSeaUpliftValid
		{
			get { return Header.CompanyData.IsImportSeaUpliftValid; }
			set { Header.CompanyData.IsImportSeaUpliftValid = value; }
		}

		public ZPropertyInfo IsImportSeaUpliftValidInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsImportSeaUpliftValid), x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.IsImportSeaUpliftValidInfo)); }
		}

		#endregion

		#region IsExportSeaUpliftValid

		[BusinessObjectTestExclude()]
		public ZBool IsExportSeaUpliftValid
		{
			get { return Header.CompanyData.IsExportSeaUpliftValid; }
			set { Header.CompanyData.IsExportSeaUpliftValid = value; }
		}

		public ZPropertyInfo IsExportSeaUpliftValidInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsExportSeaUpliftValid), x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.IsExportSeaUpliftValidInfo)); }
		}

		#endregion

		#endregion

		#region Temporary Field

		[List("OM_WhsPackingSlipOrderBy_List")]
		public override ZString OM_WhsPackingSlipOrderBy
		{
			get
			{
				return base.OM_WhsPackingSlipOrderBy;
			}
			set
			{
				base.OM_WhsPackingSlipOrderBy = value;
			}
		}

		public WhsPackingSlipOrderByList OM_WhsPackingSlipOrderBy_List
		{
			get
			{
				if (fOM_WhsPackingSlipOrderBy_List == null)
				{
					fOM_WhsPackingSlipOrderBy_List = new WhsPackingSlipOrderByList();
					fOM_WhsPackingSlipOrderBy_List.Insert(0, new CodeDescriptionPair("DEF", Res.GetString("CEFED6E6-DB96-4FD9-8C29-DAFC6B197B69", "Default From Registry")));
				}
				return fOM_WhsPackingSlipOrderBy_List;
			}
		}
		WhsPackingSlipOrderByList fOM_WhsPackingSlipOrderBy_List;

		#endregion

		#region OM_IMValidationForUnauditedClassification

		[BusinessObjectTestExclude]
		[List("Lookups.ProductAuditActions")]
		public override ZString OM_IMValidationForUnauditedClassification
		{
			get => base.OM_IMValidationForUnauditedClassification.IsEmpty ? new ZString(ProductAuditActions.Codes.RegistryDefault) : base.OM_IMValidationForUnauditedClassification;
			set => base.OM_IMValidationForUnauditedClassification = value;
		}

		#endregion

		#region OM_EXValidationForUnauditedClassification

		[BusinessObjectTestExclude]
		[List("Lookups.ProductAuditActions")]
		public override ZString OM_EXValidationForUnauditedClassification
		{
			get => base.OM_EXValidationForUnauditedClassification.IsEmpty ? new ZString(ProductAuditActions.Codes.RegistryDefault) : base.OM_EXValidationForUnauditedClassification;
			set => base.OM_EXValidationForUnauditedClassification = value;
		}

		#endregion

		#endregion

		#region Warehouse Order Fulfillment Rule

		[List("OM_WhsOrderFulfillmentRuleList")]
		public override ZString OM_WhsOrderFulfillmentRule
		{
			get { return base.OM_WhsOrderFulfillmentRule; }
			set { base.OM_WhsOrderFulfillmentRule = value; }
		}

		public WhsOrderFulfillmentRuleList OM_WhsOrderFulfillmentRuleList
		{
			get { return oM_WhsOrderFulfillmentRuleList ?? (oM_WhsOrderFulfillmentRuleList = new WhsOrderFulfillmentRuleList()); }
		}

		WhsOrderFulfillmentRuleList oM_WhsOrderFulfillmentRuleList;

		#endregion

		#region FW As Agent Option

		[List("Lookups.AsAgentOptions")]
		public override ZString OM_FWAsAgentOption
		{
			get => base.OM_FWAsAgentOption;
			set
			{
				if (base.OM_FWAsAgentOption != value)
				{
					base.OM_FWAsAgentOption = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateOM_FWAsAgentName();
					}
				}
			}
		}

		#endregion

		#region ProductReceiveWeightOrDimsCheckTypeList

		[List(nameof(Lookups) + "." + nameof(OrgMiscServLookups.ProductReceiveWeightOrDimsCheckTypeList))]
		public override ZString OM_WhsCheckPartWeightOrDimsOnReceive
		{
			get => base.OM_WhsCheckPartWeightOrDimsOnReceive;
			set => base.OM_WhsCheckPartWeightOrDimsOnReceive = value;
		}

		#endregion

		#region FW As Agent Name

		[ReadOnlyMember(nameof(FWAsAgentName_ReadOnly))]
		public override ZString OM_FWAsAgentName
		{
			get => base.OM_FWAsAgentName;
			set
			{
				if (base.OM_FWAsAgentName != value)
				{
					base.OM_FWAsAgentName = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateOM_FWAsAgentOption();
					}
				}
			}
		}

		bool FWAsAgentName_ReadOnly => OM_FWAsAgentOption.IsEmpty;

		#endregion

		#region Sales Calls

		public OrgSalesCall GetFollowUpDateSalesCall()
		{
			var closeStatusList = OrganisationsDataRegistry.Instance.CommunicationStatusList.Value.Cast<CommunicationStatus>().Where(x => x.Closed).Select(x => x.Code);
			ZQuery query = new ZQuery(OrgSalesCallSchema.OQ_OH, Header.PK);
			query.AddToFilter(OrgSalesCallSchema.OQ_NextCall, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow);
			query.AddToFilter(OrgSalesCallSchema.OQ_NextCall, SQLComparisonOperator.NotEqual, null);
			query.AddToFilter(OrgSalesCallSchema.OQ_CallDate, SQLComparisonOperator.Equal, null);
			query.AddToFilter(OrgSalesCallSchema.OQ_Status, SQLComparisonOperator.NotEqual, closeStatusList);

			query.OrderBy = OrgSalesCallSchema.Constants.OQ_NextCall;
			return Factory.LoadTop1<OrgSalesCall>(query);
		}

		public OrgSalesCall GetLastCallDateSalesCall()
		{
			var query = new ZQuery(OrgSalesCallSchema.OQ_OH, Header.PK);
			query.AddToFilter(OrgSalesCallSchema.OQ_CallDate, SQLComparisonOperator.NotEqual, null);
			query.OrderBy = OrgSalesCallSchema.Constants.OQ_CallDate + OrderByClause.Descending;
			return Factory.LoadTop1<OrgSalesCall>(query);
		}

		public void UpdateLastCallDateFromSalesCalls()
		{
			var lastCall = GetLastCallDateSalesCall();
			if (lastCall != null)
			{
				OM_CMLastCallDate = lastCall.OQ_CallDate;
			}
			else
			{
				OM_CMLastCallDate = ZDateTime.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Not required with a query where the only parameters are values")]
		public OrgSalesCall GetLastUnactionedCallDateSalesCall()
		{
			var closeStatusList = OrganisationsDataRegistry.Instance.CommunicationStatusList.Value.Cast<CommunicationStatus>().Where(x => x.Closed).Select(x => x.Code);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgSalesCall));
			query.AddToFilter(OrgSalesCallSchema.OQ_OH, Header.PK);
			query.AddToFilter(OrgSalesCallSchema.OQ_CallDate, SQLComparisonOperator.Equal, null);
			query.AddFilterAndZSQLParameterCollection(
				$"{OrgSalesCallSchema.Constants.OQ_NextCall} >= (SELECT ISNULL(MAX({OrgSalesCallSchema.Constants.OQ_CallDate}), '{ZDateTime.MinSmallDateTimeValue.SqlFormat}') FROM {OrgSalesCallSchema.Constants.SqlSchemaName}.{OrgSalesCallSchema.Constants.TableName} WHERE {OrgSalesCallSchema.Constants.OQ_OH} = @orgPK)",
				new ZSqlParameterCollection(ZSqlParameter.New("@orgPK", OM_OH, OrgSalesCallSchema.OQ_OH))
			);
			query.AddToFilter(OrgSalesCallSchema.OQ_NextCall, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			query.AddToFilter(OrgSalesCallSchema.OQ_Status, SQLComparisonOperator.NotEqual, closeStatusList);

			query.OrderBy = OrgSalesCallSchema.Constants.OQ_NextCall + OrderByClause.Descending;
			return Factory.LoadTop1<OrgSalesCall>(query);
		}

		#endregion

		#region OM_GC_CMPreferredPaymentCompany

		protected bool OM_GC_CMPreferredPaymentCompany_ReadOnly
		{
			get { return UseTransactionCompanyAsPreferredPayment; }
		}

		#region UseTransactionCompanyAsPreferredPayment

		public ZBool UseTransactionCompanyAsPreferredPayment
		{
			get { return useTransactionCompanyAsPreferredPayment; }
			set
			{
				if (useTransactionCompanyAsPreferredPayment != value)
				{
					SetNonPersistentPropertyValue(UseTransactionCompanyAsPreferredPaymentInfo, ref useTransactionCompanyAsPreferredPayment, value);
					if (value)
					{
						OM_GC_CMPreferredPaymentCompany = ZGuid.Empty;
					}
					else if (!IsValidationSuspended)
					{
						Validation.ValidateOM_GC_CMPreferredPaymentCompany();
					}
				}
			}
		}
		ZBool useTransactionCompanyAsPreferredPayment;

		public ZPropertyInfo UseTransactionCompanyAsPreferredPaymentInfo
		{
			get { return GetZPropertyInfo(nameof(UseTransactionCompanyAsPreferredPayment)); }
		}

		#endregion

		#endregion

		#region Property Overrides

		[BusinessObjectTestExclude]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid OM_OH
		{
			get { return base.OM_OH; }
			set { base.OM_OH = value; }
		}

		public ZDateTime FollowUpDate
		{
			get
			{
				var followUp = GetFollowUpDateSalesCall();
				return (followUp != null) ? followUp.OQ_NextCall : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo FollowUpDateInfo
		{
			get { return GetZPropertyInfo(nameof(FollowUpDate)); }
		}

		public ZDateTime FollowUpDateLocal
		{
			get
			{
				var followUpDate = FollowUpDate;
				return followUpDate.IsValid ? Env.Time.GetLocalTimeFromUtc(followUpDate.ToDateTime()) : followUpDate;
			}
		}

		public ZDateTime OM_CMLastCallDateLocal
		{
			get
			{
				return OM_CMLastCallDate.IsValid ? Env.Time.GetLocalTimeFromUtc(OM_CMLastCallDate.ToDateTime()) : OM_CMLastCallDate;
			}
		}

		public ZDateTime LastUnactionedCallDate
		{
			get
			{
				var lastUnactioned = GetLastUnactionedCallDateSalesCall();
				return (lastUnactioned != null) ? lastUnactioned.OQ_NextCall : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo LastUnactionedCallDateInfo
		{
			get { return GetZPropertyInfo(nameof(LastUnactionedCallDate)); }
		}

		public ZDateTime LastUnactionedCallDateLocal
		{
			get
			{
				var lastUnactionedCallDate = LastUnactionedCallDate;
				return lastUnactionedCallDate.IsValid ? Env.Time.GetLocalTimeFromUtc(lastUnactionedCallDate.ToDateTime()) : lastUnactionedCallDate;
			}
		}

		#region GlobalCreditLimit properties

		[ReadOnlyMember(nameof(IsMemberOfGlobalCreditGroup))]
		public override ZBool OM_ARGlobalCreditApproved
		{
			get => base.OM_ARGlobalCreditApproved;
			set => base.OM_ARGlobalCreditApproved = value;
		}

		[ReadOnlyMember(nameof(IsMemberOfGlobalCreditGroup))]
		public override ZBool OM_ARGlobalOnCreditHold
		{
			get => base.OM_ARGlobalOnCreditHold;
			set => base.OM_ARGlobalOnCreditHold = value;
		}

		public override ZGuid OM_OH_ARGlobalCreditGroup
		{
			get => base.OM_OH_ARGlobalCreditGroup;
			set
			{
				var oldValue = OM_OH_ARGlobalCreditGroup;
				base.OM_OH_ARGlobalCreditGroup = value;
				if (!oldValue.IsValid && IsMemberOfGlobalCreditGroup)
				{
					OM_ARGlobalCreditLimit = ZDecimal.Zero;
					OM_RX_NKARGlobalCreditCurrency = ZString.Empty;
					OM_ARGlobalOnCreditHold = false;
					OM_ARGlobalCreditApproved = false;
				}
			}
		}

		[List("Lookups.ARGlobalCreditGroups")]
		public ZGuid ARGlobalCreditGroupForDisplayPK => IsMemberOfGlobalCreditGroup ? OM_OH_ARGlobalCreditGroup : OM_OH;

		[ReadOnlyMember(nameof(IsMemberOfGlobalCreditGroup))]
		[DecimalPlaces(nameof(GlobalCreditCurrencyDecimals))]
		public override ZDecimal OM_ARGlobalCreditLimit
		{
			get => base.OM_ARGlobalCreditLimit;
			set => base.OM_ARGlobalCreditLimit = value;
		}

		[DecimalPlaces(nameof(GlobalCreditCurrencyDecimals))]
		public ZDecimal ARGlobalCreditLimit
		{
			get
			{
				var result = OM_ARGlobalCreditApproved ? OM_ARGlobalCreditLimit : ZDecimal.Zero;

				if (IsMemberOfGlobalCreditGroup)
				{
					var globalCreditGroupMiscServ = ARGlobalCreditGroup?.MiscServ;
					result = globalCreditGroupMiscServ != null && globalCreditGroupMiscServ.OM_ARGlobalCreditApproved ? globalCreditGroupMiscServ.OM_ARGlobalCreditLimit : ZDecimal.Zero;
				}

				return result;
			}
		}

		public ZString ARGlobalCreditCurrencyCode
		{
			get
			{
				var result = OM_RX_NKARGlobalCreditCurrency;

				if (IsMemberOfGlobalCreditGroup)
				{
					var globalCreditGroupMiscServ = ARGlobalCreditGroup?.MiscServ;
					if (globalCreditGroupMiscServ != null)
					{
						result = globalCreditGroupMiscServ.OM_RX_NKARGlobalCreditCurrency;
					}
				}

				return result;
			}
		}

		public ZBool ARGlobalCreditApproved
		{
			get
			{
				var result = OM_ARGlobalCreditApproved;

				if (IsMemberOfGlobalCreditGroup)
				{
					var globalCreditGroupMiscServ = ARGlobalCreditGroup?.MiscServ;
					if (globalCreditGroupMiscServ != null)
					{
						result = globalCreditGroupMiscServ.OM_ARGlobalCreditApproved;
					}
				}

				return result;
			}
		}

		public ZBool ARGlobalOnCreditHold
		{
			get
			{
				var result = OM_ARGlobalOnCreditHold;

				if (IsMemberOfGlobalCreditGroup)
				{
					var globalCreditGroupMiscServ = ARGlobalCreditGroup?.MiscServ;
					if (globalCreditGroupMiscServ != null)
					{
						result = globalCreditGroupMiscServ.OM_ARGlobalOnCreditHold;
					}
				}

				return result;
			}
		}

		public ZDecimal LocalCreditLimitTotal
		{
			get
			{
				if (!otherCompaniesLocalCreditLimitTotal.HasValue)
				{
					LoadOtherCompaniesLocalCreditLimitTotal();
				}
				var currentCompanyLocalCreditLimit = Header.CompanyData.OB_ARCreditApproved ? LocalCreditLimitInGlobalCreditCurrency : ZDecimal.Zero;
				return otherCompaniesLocalCreditLimitTotal.Value + currentCompanyLocalCreditLimit;
			}
		}
		ZDecimal? otherCompaniesLocalCreditLimitTotal;

		public ZBool IsInvalidGlobalCreditCurrencyOrMissingExRate
		{
			get
			{
				if (!isInvalidGlobalCreditCurrencyOrMissingExRate.HasValue)
				{
					LoadOtherCompaniesLocalCreditLimitTotal();
				}
				return isInvalidGlobalCreditCurrencyOrMissingExRate.Value || (OM_ARGlobalCreditApproved && Header.CompanyData.ARTemporaryCreditLimit > 0 && GlobalCreditGroupHelper.GetGlobalExchangeRate(Factory, ARGlobalCreditCurrencyCode) == 0);
			}
		}
		ZBool? isInvalidGlobalCreditCurrencyOrMissingExRate;

		void LoadOtherCompaniesLocalCreditLimitTotal()
		{
			var result = ZDecimal.Zero;
			isInvalidGlobalCreditCurrencyOrMissingExRate = !TryGetOtherCompaniesLocalCreditLimitTotal(out result);
			otherCompaniesLocalCreditLimitTotal = result;
		}

		void ResetInvalidGlobalCreditCurrencyOrMissingExRateCache()
		{
			isInvalidGlobalCreditCurrencyOrMissingExRate = null;
			otherCompaniesLocalCreditLimitTotal = null;
		}

		bool TryGetOtherCompaniesLocalCreditLimitTotal(out ZDecimal otherCompaniesLocalCreditLimitTotalValue)
		{
			var success = false;
			otherCompaniesLocalCreditLimitTotalValue = ZDecimal.Zero;

			var result = new DynamicBusinessObjectCollection(Factory);

			result.Load("EXEC OtherCompaniesLocalCreditLimitTotal @OrgPK, @CompanyPK, @GlobalCreditCurrencyCode", new ZSqlParameterCollection()
			{
				ZSqlParameter.New("@OrgPK", Header.PK.ToGuid(), OrgHeaderSchema.PK),
				ZSqlParameter.New("@CompanyPK", Header.CompanyData.OB_GC.ToGuid(), OrgCompanyDataSchema.OB_GC),
				ZSqlParameter.New("@GlobalCreditCurrencyCode", ARGlobalCreditCurrencyCode, RefCurrencySchema.RX_Code)
			});

			if (result.Count == 1)
			{
				success = !new ZBool(result[0]["InvalidGlobalCreditCurrencyOrMissingExRate"]);
				otherCompaniesLocalCreditLimitTotalValue = new ZDecimal(result[0]["OtherCompaniesLocalCreditLimitTotal"]);
			}

			return success;
		}

		ZDecimal LocalCreditLimitInGlobalCreditCurrency
		{
			get
			{
				var companyData = Header.CompanyData;
				var currentCompany = companyData.Company;
				var localCreditLimit = companyData.ARTemporaryCreditLimit;
				var globalCreditCurrencyCode = ARGlobalCreditCurrencyCode;

				if (currentCompany.GC_RX_NKLocalCurrency != globalCreditCurrencyCode)
				{
					var globalRate = GlobalCreditGroupHelper.GetGlobalExchangeRate(Factory, globalCreditCurrencyCode);
					if (globalRate != 1)
					{
						localCreditLimit = currentCompany.GetExchangeRate().LocalToForeign(localCreditLimit, globalRate, globalCreditCurrencyCode);
					}
				}

				return localCreditLimit;
			}
		}

		[ReadOnlyMember(nameof(IsMemberOfGlobalCreditGroup))]
		public override ZString OM_RX_NKARGlobalCreditCurrency
		{
			get => base.OM_RX_NKARGlobalCreditCurrency;
			set
			{
				base.OM_RX_NKARGlobalCreditCurrency = value;
				ResetInvalidGlobalCreditCurrencyOrMissingExRateCache();
			}
		}

		public bool IsMemberOfGlobalCreditGroup => OM_OH_ARGlobalCreditGroup.IsValid;

		#endregion

		#region OM_CarrierPackageGrouping

		[List("OM_CarrierPackageGrouping_List")]
		public override ZString OM_CarrierPackageGrouping
		{
			get
			{
				return base.OM_CarrierPackageGrouping;
			}
			set
			{
				base.OM_CarrierPackageGrouping = value;
			}
		}

		#endregion

		#region OM_FWAgentPackageGrouping

		[List("OM_FWAgentPackageGrouping_List")]
		public override ZString OM_FWAgentPackageGrouping
		{
			get
			{
				return base.OM_FWAgentPackageGrouping;
			}
			set
			{
				base.OM_FWAgentPackageGrouping = value;
			}
		}

		#endregion

		#region OM_CMOverallEffectOfClientOnWarehousingCosts
		[List("OM_CMOverallEffectOfClient_List")]
		public override ZString OM_CMOverallEffectOfClientOnWarehousingCosts
		{
			get
			{
				return base.OM_CMOverallEffectOfClientOnWarehousingCosts;
			}
			set
			{
				base.OM_CMOverallEffectOfClientOnWarehousingCosts = value;
			}
		}

		#endregion

		#region OM_CMOverallEffectOfClientOnOtherCosts
		[List("OM_CMOverallEffectOfClient_List")]
		public override ZString OM_CMOverallEffectOfClientOnOtherCosts
		{
			get
			{
				return base.OM_CMOverallEffectOfClientOnOtherCosts;
			}
			set
			{
				base.OM_CMOverallEffectOfClientOnOtherCosts = value;
			}
		}
		#endregion

		#region OM_CMOverallEffectOfClientOnTEUCosts
		[List("OM_CMOverallEffectOfClient_List")]
		public override ZString OM_CMOverallEffectOfClientOnTEUCosts
		{
			get
			{
				return base.OM_CMOverallEffectOfClientOnTEUCosts;
			}
			set
			{
				base.OM_CMOverallEffectOfClientOnTEUCosts = value;
			}
		}
		#endregion

		#region OM_CMOverallEffectOfClientOnLCLCosts
		[List("OM_CMOverallEffectOfClient_List")]
		public override ZString OM_CMOverallEffectOfClientOnLCLCosts
		{
			get
			{
				return base.OM_CMOverallEffectOfClientOnLCLCosts;
			}
			set
			{
				base.OM_CMOverallEffectOfClientOnLCLCosts = value;
			}
		}

		#endregion

		#region OM_CMOverallEffectOfClientOnAirfreightCosts
		[List("OM_CMOverallEffectOfClient_List")]
		public override ZString OM_CMOverallEffectOfClientOnAirfreightCosts
		{
			get
			{
				return base.OM_CMOverallEffectOfClientOnAirfreightCosts;
			}
			set
			{
				base.OM_CMOverallEffectOfClientOnAirfreightCosts = value;
			}
		}
		#endregion

		#region OM_CMOverallClientRelation

		[List("OM_CMOverallClientRelation_List")]
		public override ZByte OM_CMOverallClientRelation
		{
			get
			{
				return base.OM_CMOverallClientRelation;
			}
			set
			{
				base.OM_CMOverallClientRelation = value;
			}
		}

		#endregion

		#region OM_CMClientsDesireToRemain

		[List("OM_CMClientsDesireToRemain_List")]
		public override ZByte OM_CMClientsDesireToRemain
		{
			get
			{
				return base.OM_CMClientsDesireToRemain;
			}
			set
			{
				base.OM_CMClientsDesireToRemain = value;
			}
		}

		#endregion

		#region OM_CMEaseClientCanBePoached

		[List("OM_CMEaseClientCanBePoached_List")]
		public override ZByte OM_CMEaseClientCanBePoached
		{
			get
			{
				return base.OM_CMEaseClientCanBePoached;
			}
			set
			{
				base.OM_CMEaseClientCanBePoached = value;
			}
		}

		#endregion

		#region OM_CMAmountOfElectronicIntegration

		[List("OM_CMAmountOfElectronicIntegration_List")]
		public override ZByte OM_CMAmountOfElectronicIntegration
		{
			get
			{
				return base.OM_CMAmountOfElectronicIntegration;
			}
			set
			{
				base.OM_CMAmountOfElectronicIntegration = value;
			}
		}

		#endregion

		#region OM_CMSalesTerritory
		[List("OM_CMSalesTerritory_List")]
		public override ZString OM_CMSalesTerritory
		{
			get
			{
				return base.OM_CMSalesTerritory;
			}
			set
			{
				base.OM_CMSalesTerritory = value;
			}
		}
		#endregion

		#region OM_CMSalesCategory
		[List("OM_CMSalesCategory_List")]
		public override ZString OM_CMSalesCategory
		{
			get
			{
				return base.OM_CMSalesCategory;
			}
			set
			{
				base.OM_CMSalesCategory = value;
			}
		}
		#endregion

		#region OM_CMClientSize
		[List("OM_CMClientSize_List")]
		public override ZString OM_CMClientSize
		{
			get
			{
				return base.OM_CMClientSize;
			}
			set
			{
				base.OM_CMClientSize = value;
			}
		}
		#endregion

		#region OM_CMGrowthOutlook
		[List("OM_CMGrowthOutlook_List")]
		public override ZString OM_CMGrowthOutlook
		{
			get
			{
				return base.OM_CMGrowthOutlook;
			}
			set
			{
				base.OM_CMGrowthOutlook = value;
			}
		}
		#endregion

		#region OM_CMCompetitorActivity
		[List("OM_CMCompetitorActivity_List")]
		public override ZString OM_CMCompetitorActivity
		{
			get
			{
				return base.OM_CMCompetitorActivity;
			}
			set
			{
				base.OM_CMCompetitorActivity = value;
			}
		}
		#endregion

		#region OM_CMIndustryVertical

		[List("OM_CMIndustryVertical_ActiveList")]
		public override ZString OM_CMIndustryVertical
		{
			get { return base.OM_CMIndustryVertical; }
			set { base.OM_CMIndustryVertical = value; }
		}

		public ZString OM_CMIndustryVerticalDescription
		{
			get { return OM_CMIndustryVertical_List.GetDescriptionFromCode(OM_CMIndustryVertical); }
		}

		#endregion

		#region OM_CMPeriodOfActivity

		[List("OM_CMPeriodOfActivity_ActiveList")]
		public override ZString OM_CMPeriodOfActivity
		{
			get { return base.OM_CMPeriodOfActivity; }
			set { base.OM_CMPeriodOfActivity = value; }
		}

		public ZString OM_CMPeriodOfActivityDescription
		{
			get { return OM_CMPeriodOfActivity_List.GetDescriptionFromCode(OM_CMPeriodOfActivity); }
		}

		#endregion

		#region OM_CICompetitorCategory

		[List("OM_CICompetitorCategory_List")]
		public override ZString OM_CICompetitorCategory
		{
			get
			{
				return base.OM_CICompetitorCategory;
			}
			set
			{
				base.OM_CICompetitorCategory = value;
			}
		}

		#endregion

		#region OM_CICompetitiveRanking

		[List("OM_CICompetitiveRanking_List")]
		public override ZByte OM_CICompetitiveRanking
		{
			get
			{
				return base.OM_CICompetitiveRanking;
			}
			set
			{
				base.OM_CICompetitiveRanking = value;
			}
		}

		#endregion

		#region OM_RH_NKCMMainExportCmdty
		[List("CommodityCodes")]
		public override ZString OM_RH_NKCMMainExportCmdty
		{
			get
			{
				return base.OM_RH_NKCMMainExportCmdty;
			}
			set
			{
				base.OM_RH_NKCMMainExportCmdty = value;
			}
		}
		#endregion

		#region OM_RH_NKCMMainImportCmdty
		[List("CommodityCodes")]
		public override ZString OM_RH_NKCMMainImportCmdty
		{
			get
			{
				return base.OM_RH_NKCMMainImportCmdty;
			}
			set
			{
				base.OM_RH_NKCMMainImportCmdty = value;
			}
		}
		#endregion

		#region OM_SVServicesCategory
		[List("OM_SVServicesCategory_List")]
		public override ZString OM_SVServicesCategory
		{
			get
			{
				return base.OM_SVServicesCategory;
			}
			set
			{
				base.OM_SVServicesCategory = value;
			}
		}
		#endregion

		#region OM_OC_EXDefaultDGContact
		[List("UNDGContactList")]
		public override ZGuid OM_OC_EXDefaultDGContact
		{
			get
			{
				return base.OM_OC_EXDefaultDGContact;
			}
			set
			{
				base.OM_OC_EXDefaultDGContact = value;
			}
		}
		#endregion

		#region OM_EXDefaultDGContactPhoneUsed
		[List("PhoneTypesList")]
		public override ZString OM_EXDefaultDGContactPhoneUsed
		{
			get
			{
				return base.OM_EXDefaultDGContactPhoneUsed;
			}
			set
			{
				base.OM_EXDefaultDGContactPhoneUsed = value;
			}
		}
		#endregion

		#region OM_WCG_CartonGroup

		[List("Lookups.CartonGroups")]
		public override ZGuid OM_WCG_CartonGroup
		{
			get { return base.OM_WCG_CartonGroup; }
			set { base.OM_WCG_CartonGroup = value; }
		}

		#endregion

		#region OM_WhsOrderNumberUniquenessStrategy
		[List("WhsUniqueRefStrategyTypesList")]
		public override ZString OM_WhsOrderNumberUniquenessStrategy
		{
			get
			{
				return base.OM_WhsOrderNumberUniquenessStrategy;
			}
			set
			{
				base.OM_WhsOrderNumberUniquenessStrategy = value;
			}
		}
		#endregion

		#region OM_WhsABCAnalysisMethod

		[List("Lookups.ABCAnalysisMethodsList")]
		public override ZString OM_WhsABCAnalysisMethod
		{
			get { return base.OM_WhsABCAnalysisMethod; }
			set { base.OM_WhsABCAnalysisMethod = value; }
		}

		#endregion

		#region OM_WhsABCAnalysisPeriod

		[List("Lookups.ABCAnalysisPeriodsList")]
		public override ZString OM_WhsABCAnalysisPeriod
		{
			get { return base.OM_WhsABCAnalysisPeriod; }
			set { base.OM_WhsABCAnalysisPeriod = value; }
		}

		#endregion

		#region OM_WhsIncludeClientInABCAnalysis

		public override ZBool OM_WhsABCAnalysisEnabled
		{
			get { return base.OM_WhsABCAnalysisEnabled; }
			set
			{
				base.OM_WhsABCAnalysisEnabled = value;

				if (!value)
				{
					OM_WhsABCAnalysisMethod = WhsABCAnalysisMethodCodeList.Codes.Default;
					OM_WhsABCAnalysisPeriod = WhsABCAnalysisPeriodCodeList.Codes.Default;
				}
			}
		}

		#endregion

		#region OM_IMAllowOrders

		public ZBool OM_IMDisallowOrders
		{
			get { return !base.OM_IMAllowOrders; }
			set { base.OM_IMAllowOrders = !value; }
		}

		public ZPropertyInfo OM_IMDisallowOrdersInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_IMDisallowOrders, x => OM_IMAllowOrdersInfo); }
		}

		#endregion

		#region OM_IMAirCargoReportDefaultConsignee

		[List(nameof(Lookups) + "." + nameof(OrgMiscServLookups.ConsigneeDefaultOptionList))]
		[ResourceStringData("OrgMiscServ|01B9267D-B171-49A8-B8B2-DF5E08679598", Caption = "ACR Consignee Default")]
		public override ZString OM_IMAirCargoReportDefaultConsignee
		{
			get => base.OM_IMAirCargoReportDefaultConsignee;
			set => base.OM_IMAirCargoReportDefaultConsignee = value;
		}

		#endregion

		#region OM_IMSeaCargoReportDefaultConsignee

		[List(nameof(Lookups) + "." + nameof(OrgMiscServLookups.ConsigneeDefaultOptionList))]
		[ResourceStringData("OrgMiscServ|B0FBB8ED-BD7C-4E14-963B-63ACBB5932A8", Caption = "SCR Consignee Default")]
		public override ZString OM_IMSeaCargoReportDefaultConsignee
		{
			get => base.OM_IMSeaCargoReportDefaultConsignee;
			set => base.OM_IMSeaCargoReportDefaultConsignee = value;
		}

		#endregion

		#region OM_IMPartAttrib1Name

		[BusinessObjectTestExclude]
		[TranslatableDataField(Schema.TableName, Schema.OM_IMPartAttrib1Name, MaxLength = Schema.OM_IMPartAttrib1NameMaxLength, Type = typeof(OrgMiscServ), Asmid = ResString.AssemblyId)]
		public override ZString OM_IMPartAttrib1Name
		{
			get { return base.OM_IMPartAttrib1Name; }
			set { base.OM_IMPartAttrib1Name = value; }
		}

		public MultilingualString OM_IMPartAttrib1NameMultilingual
		{
			get { return GetMultilingual(OM_IMPartAttrib1NameInfo); }
		}

		#endregion

		#region OM_IMPartAttrib2Name

		[BusinessObjectTestExclude]
		[TranslatableDataField(Schema.TableName, Schema.OM_IMPartAttrib2Name, MaxLength = Schema.OM_IMPartAttrib2NameMaxLength, Type = typeof(OrgMiscServ), Asmid = ResString.AssemblyId)]
		public override ZString OM_IMPartAttrib2Name
		{
			get { return base.OM_IMPartAttrib2Name; }
			set { base.OM_IMPartAttrib2Name = value; }
		}

		public MultilingualString OM_IMPartAttrib2NameMultilingual
		{
			get { return GetMultilingual(OM_IMPartAttrib2NameInfo); }
		}

		#endregion

		#region OM_IMPartAttrib3Name

		[BusinessObjectTestExclude]
		[TranslatableDataField(Schema.TableName, Schema.OM_IMPartAttrib3Name, MaxLength = Schema.OM_IMPartAttrib3NameMaxLength, Type = typeof(OrgMiscServ), Asmid = ResString.AssemblyId)]
		public override ZString OM_IMPartAttrib3Name
		{
			get { return base.OM_IMPartAttrib3Name; }
			set { base.OM_IMPartAttrib3Name = value; }
		}

		public MultilingualString OM_IMPartAttrib3NameMultilingual
		{
			get { return GetMultilingual(OM_IMPartAttrib3NameInfo); }
		}

		#endregion

		#region OM_WhsPackageWeightTolerancePercent

		[ReadOnlyMember(nameof(OM_WhsPackageWeightTolerancePercent_ReadOnly))]
		public override ZDecimal OM_WhsPackageWeightTolerancePercent => base.OM_WhsPackageWeightTolerancePercent;

		bool OM_WhsPackageWeightTolerancePercent_ReadOnly => !OM_WhsPackageToleranceEnabled;

		#endregion

		#region OM_CIFinancialDetailsApplicableDates

		public override ZDate OM_CIFinancialDetailsApplicableFromDate
		{
			get { return base.OM_CIFinancialDetailsApplicableFromDate; }

			set
			{
				base.OM_CIFinancialDetailsApplicableFromDate = value;
				OM_CIFinancialDetailsApplicableToDateInfo.RefreshBinding();
			}
		}

		[ReadOnly(true)]
		public ZDate OM_CIFinancialDetailsApplicableToDate
		{
			get
			{
				var toDate = ZDate.Empty;

				if (OM_CIFinancialDetailsApplicableFromDate.IsValid && !OM_CIFinancialDetailsApplicableFromDateInfo.HasErrors())
				{
					toDate = OM_CIFinancialDetailsApplicableFromDate.AddYears(1).AddDays(-1);
				}

				return toDate;
			}
		}

		public ZPropertyInfo OM_CIFinancialDetailsApplicableToDateInfo
		{
			get { return GetZPropertyInfo(nameof(OM_CIFinancialDetailsApplicableToDate), Res.GetString("95118573-19B3-4849-8F05-DA3FEF44AA8C", "Applicable To Date")); }
		}

		#endregion

		#region Receivables (all overridden for OrgCompanyData)

		#region OM_IMAutoPopulateOwnerRefWithOrderNums
		[List("OM_IMAutoPopulateOwnerRefList")]
		public override ZString OM_IMAutoPopulateOwnerRefWithOrderNums
		{
			get
			{
				return base.OM_IMAutoPopulateOwnerRefWithOrderNums;
			}
			set
			{
				base.OM_IMAutoPopulateOwnerRefWithOrderNums = value;
			}
		}

		#endregion

		#region OM_IMPaymentMethod
		[List("OM_IMPaymentMethod_List")]
		public ZString OM_IMPaymentMethod
		{
			get { return Header.CompanyData.OB_AREftCustomsPaymentMethod; }
			set { Header.CompanyData.OB_AREftCustomsPaymentMethod = value; }
		}

		public ZPropertyInfo OM_IMPaymentMethodInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_IMPaymentMethod, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_AREftCustomsPaymentMethodInfo)); }
		}

		#endregion

		#region OM_IMPartAttrib3Type

		[List("PartAttributeRuleList")]
		public override ZString OM_IMPartAttrib3Type
		{
			get { return base.OM_IMPartAttrib3Type; }
			set
			{
				base.OM_IMPartAttrib3Type = value;
				if (OM_IMPartAttrib3Type.EqualsIgnoringCase(PartAttributeTypeList.Codes.JulianBatchNumber))
				{
					OM_IMUsePackingDate = true;
					OM_IMUseExpiryDate = true;
				}
			}
		}

		#endregion

		#region OM_IMPartAttrib2Type

		[List("PartAttributeRuleList")]
		public override ZString OM_IMPartAttrib2Type
		{
			get { return base.OM_IMPartAttrib2Type; }
			set
			{
				base.OM_IMPartAttrib2Type = value;
				if (OM_IMPartAttrib2Type.EqualsIgnoringCase(PartAttributeTypeList.Codes.JulianBatchNumber))
				{
					OM_IMUsePackingDate = true;
					OM_IMUseExpiryDate = true;
				}
			}
		}

		#endregion

		#region OM_IMPartAttrib1Type

		[List("PartAttributeRuleList")]
		public override ZString OM_IMPartAttrib1Type
		{
			get { return base.OM_IMPartAttrib1Type; }
			set
			{
				base.OM_IMPartAttrib1Type = value;
				if (OM_IMPartAttrib1Type.EqualsIgnoringCase(PartAttributeTypeList.Codes.JulianBatchNumber))
				{
					OM_IMUsePackingDate = true;
					OM_IMUseExpiryDate = true;
				}
			}
		}

		#endregion

		#region OM_IMSendSeaImportDocsTo
		[List("OM_IMSendSeaImportDocsTo_List")]
		public override ZString OM_IMSendSeaImportDocsTo
		{
			get
			{
				return base.OM_IMSendSeaImportDocsTo;
			}
			set
			{
				base.OM_IMSendSeaImportDocsTo = value;

				if (Header != null)
				{
					Header.AllRelatedParties.Load();
				}
			}
		}

		#endregion

		#region OM_IMSendImportDocsTo
		[List("OM_IMSendImportDocsTo_List")]
		public override ZString OM_IMSendImportDocsTo
		{
			get
			{
				return base.OM_IMSendImportDocsTo;
			}
			set
			{
				base.OM_IMSendImportDocsTo = value;
			}
		}
		#endregion

		#region OM_IMDocumentAddressPreference

		[List("OM_IMDocumentAddressPreference_List")]
		public override ZString OM_IMDocumentAddressPreference
		{
			get
			{
				return base.OM_IMDocumentAddressPreference;
			}
			set
			{
				base.OM_IMDocumentAddressPreference = value;
			}
		}

		#endregion

		#region OM_IMDefaultWarehousePickOption

		[List("WarehousePickOptionList")]
		public override ZString OM_IMDefaultWarehousePickOption
		{
			get { return base.OM_IMDefaultWarehousePickOption; }
			set { base.OM_IMDefaultWarehousePickOption = value; }
		}

		#endregion

		#region OM_WhsDefaultWarehousePickMode

		[List("WarehousePickModeList")]
		public override ZString OM_WhsDefaultWarehousePickMode
		{
			get { return base.OM_WhsDefaultWarehousePickMode; }
			set
			{
				base.OM_WhsDefaultWarehousePickMode = value;
				if (OM_WhsDefaultWarehousePickMode == WhsPickMode.Codes.AttributeSpecified)
				{
					OM_WhsDefaultWarehouseRollUp = false;
				}
			}
		}

		#endregion

		#region OM_IMDefaultINCOTerm

		[List("OM_IMDefaultINCOTerm_List")]
		public override ZString OM_IMDefaultINCOTerm
		{
			get
			{
				return base.OM_IMDefaultINCOTerm;
			}
			set
			{
				base.OM_IMDefaultINCOTerm = value;
			}
		}

		#endregion

		#region	OM_IMImporterCategory

		[List("OM_IMImporterCategory_List")]
		public override ZString OM_IMImporterCategory
		{
			get
			{
				return base.OM_IMImporterCategory;
			}
			set
			{
				base.OM_IMImporterCategory = value;
			}
		}

		#endregion

		#region OM_IMMergeCustomsInvoiceLinesBy

		[List("OM_IMMergeCustomsInvoiceLinesBy_List")]
		public override ZString OM_IMMergeCustomsInvoiceLinesBy
		{
			get
			{
				return base.OM_IMMergeCustomsInvoiceLinesBy;
			}
			set
			{
				base.OM_IMMergeCustomsInvoiceLinesBy = value;
			}
		}

		#endregion

		#region OM_EXMergeCustomsInvoiceLinesBy

		[List(nameof(Lookups) + "." + nameof(OrgMiscServLookups.OM_EXMergeCustomsInvoiceLinesBy_List))]
		[ResourceStringData("OrgMiscServ|OM_EXMergeCustomsInvoiceLinesBy", Caption = "Merge Customs Invoice Lines By")]
		public override ZString OM_EXMergeCustomsInvoiceLinesBy
		{
			get => base.OM_EXMergeCustomsInvoiceLinesBy;
			set => base.OM_EXMergeCustomsInvoiceLinesBy = value;
		}

		#endregion

		#region OM_IMInvoiceDetailReportSort3
		[List("Lookups.InvoiceDetailReportSortList3")]
		public override ZString OM_IMInvoiceDetailReportSort3
		{
			get
			{
				return base.OM_IMInvoiceDetailReportSort3;
			}
			set
			{
				base.OM_IMInvoiceDetailReportSort3 = value;
			}
		}
		#endregion

		#region OM_IMInvoiceDetailReportSort2
		[List("Lookups.InvoiceDetailReportSortList2")]
		public override ZString OM_IMInvoiceDetailReportSort2
		{
			get
			{
				return base.OM_IMInvoiceDetailReportSort2;
			}
			set
			{
				base.OM_IMInvoiceDetailReportSort2 = value;
			}
		}
		#endregion

		#region OM_IMInvoiceDetailReportSort
		[List("Lookups.InvoiceDetailReportSortList1")]
		public override ZString OM_IMInvoiceDetailReportSort
		{
			get
			{
				return base.OM_IMInvoiceDetailReportSort;
			}
			set
			{
				base.OM_IMInvoiceDetailReportSort = value;
			}
		}

		#endregion

		#region OM_ARAutoUpdateRates

		public ZBool OM_ARAutoUpdateRates
		{
			get { return Header.CompanyData.OB_ARAutoUpdateRates; }
			set { Header.CompanyData.OB_ARAutoUpdateRates = value; }
		}

		public ZPropertyInfo OM_ARAutoUpdateRatesInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARAutoUpdateRates, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARAutoUpdateRatesInfo)); }
		}

		#endregion

		#region OM_ARCategory
		[List("OM_ARCategory_List")]
		public ZString OM_ARCategory
		{
			get { return Header.CompanyData.OB_ARCategory; }
			set { Header.CompanyData.OB_ARCategory = value; }
		}

		public ZPropertyInfo OM_ARCategoryInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARCategory, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARCategoryInfo)); }
		}

		#endregion

		#region OM_ARCombinedStatementInvoice

		public ZBool OM_ARCombinedStatementInvoice
		{
			get { return Header.CompanyData.OB_ARCombinedStatementInvoice; }
			set { Header.CompanyData.OB_ARCombinedStatementInvoice = value; }
		}

		public ZPropertyInfo OM_ARCombinedStatementInvoiceInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARCombinedStatementInvoice, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARCombinedStatementInvoiceInfo)); }
		}

		#endregion

		#region OM_ARConsolidatedAccountingCategory
		[List("OM_ARConsolidatedAccountingCategory_List")]
		public ZString OM_ARConsolidatedAccountingCategory
		{
			get { return Header.CompanyData.OB_ARConsolidatedAccountingCategory; }
			set { Header.CompanyData.OB_ARConsolidatedAccountingCategory = value; }
		}

		public ZPropertyInfo OM_ARConsolidatedAccountingCategoryInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARConsolidatedAccountingCategory, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARConsolidatedAccountingCategoryInfo)); }
		}

		public bool OM_ARConsolidatedAccountingCategory_ReadOnly
		{
			get
			{
				bool hasSecurity = Header.SecurityProvider.HasModifyReceivablesSecurity && Header.SecurityProvider.HasModifyConsolidationCategory;
				return !hasSecurity;
			}
		}

		public string ConsolidatedAccountingCategoryClass => ConsolidatedAccountingCategoryListWithGroup.GetGroupFromCode(OM_ARConsolidatedAccountingCategory);

		CodeDescriptionWithGroupCollection ConsolidatedAccountingCategoryListWithGroup => consolidatedAccountingCategoryListWithGroup ?? (consolidatedAccountingCategoryListWithGroup = AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value);
		CodeDescriptionWithGroupCollection consolidatedAccountingCategoryListWithGroup;

		#endregion

		#region OM_ARCreditLimit

		public ZDecimal OM_ARCreditLimit
		{
			get { return Header.CompanyData.OB_ARCreditLimit; }
			set { Header.CompanyData.OB_ARCreditLimit = value; }
		}

		public ZPropertyInfo OM_ARCreditLimitInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARCreditLimit, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARCreditLimitInfo)); }
		}

		#endregion

		#region ARTemporaryCreditLimitIncrease

		public ZDecimal ARTemporaryCreditLimitIncrease
		{
			get { return Header.CompanyData.OB_ARTemporaryCreditLimitIncrease; }
			set { Header.CompanyData.OB_ARTemporaryCreditLimitIncrease = value; }
		}

		public ZPropertyInfo ARTemporaryCreditLimitIncreaseInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.ARTemporaryCreditLimitIncrease, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARTemporaryCreditLimitIncreaseInfo)); }
		}

		#endregion

		#region ARTemporaryCreditLimit

		public ZDecimal ARTemporaryCreditLimit
		{
			get { return Header.CompanyData.ARTemporaryCreditLimit; }
		}

		public ZPropertyInfo ARTemporaryCreditLimitInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.ARTemporaryCreditLimit, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.ARTemporaryCreditLimitInfo)); }
		}

		#endregion

		#region ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime

		public ZDateTime ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime
		{
			get { return Header.CompanyData.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime; }
		}

		public ZPropertyInfo ARTemporaryCreditLimitIncreaseExpiryLocalBranchTimeInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTimeInfo)); }
		}

		#endregion

		#region OM_ARCreditRating
		[List("OM_ARCreditRating_List")]
		public ZString OM_ARCreditRating
		{
			get { return Header.CompanyData.OB_ARCreditRating; }
			set { Header.CompanyData.OB_ARCreditRating = value; }
		}

		public ZPropertyInfo OM_ARCreditRatingInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARCreditRating, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARCreditRatingInfo)); }
		}

		#endregion

		#region OM_ARDontShowTaxOnDocs

		public ZBool OM_ARDontShowTaxOnDocs
		{
			get { return Header.CompanyData.OB_ARDontShowTaxOnDocs; }
			set { Header.CompanyData.OB_ARDontShowTaxOnDocs = value; }
		}

		public ZPropertyInfo OM_ARDontShowTaxOnDocsInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARDontShowTaxOnDocs, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARDontShowTaxOnDocsInfo)); }
		}

		#endregion

		#region OM_AROnCreditHold

		public ZBool OM_AROnCreditHold
		{
			get { return Header.CompanyData.OB_AROnCreditHold; }
			set { Header.CompanyData.OB_AROnCreditHold = value; }
		}

		public ZPropertyInfo OM_AROnCreditHoldInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_AROnCreditHold, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_AROnCreditHoldInfo)); }
		}

		#endregion

		#region OM_ARPreviousChequeDrawer

		public ZString OM_ARPreviousChequeDrawer
		{
			get { return Header.CompanyData.OB_ARPreviousChequeDrawer; }
			set { Header.CompanyData.OB_ARPreviousChequeDrawer = value; }
		}

		public ZPropertyInfo OM_ARPreviousChequeDrawerInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARPreviousChequeDrawer, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARPreviousChequeDrawerInfo)); }
		}

		#endregion

		#region OM_ARPreviousChequeDrawerBank

		public ZString OM_ARPreviousChequeDrawerBank
		{
			get { return Header.CompanyData.OB_ARPreviousChequeDrawerBank; }
			set { Header.CompanyData.OB_ARPreviousChequeDrawerBank = value; }
		}

		public ZPropertyInfo OM_ARPreviousChequeDrawerBankInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARPreviousChequeDrawerBank, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARPreviousChequeDrawerBankInfo)); }
		}

		#endregion

		#region OM_ARPreviousChequeDrawerBankBranch

		public ZString OM_ARPreviousChequeDrawerBankBranch
		{
			get { return Header.CompanyData.OB_ARPreviousChequeDrawerBankBranch; }
			set { Header.CompanyData.OB_ARPreviousChequeDrawerBankBranch = value; }
		}

		public ZPropertyInfo OM_ARPreviousChequeDrawerBankBranchInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARPreviousChequeDrawerBankBranch, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARPreviousChequeDrawerBankBranchInfo)); }
		}

		#endregion

		#region OM_ARReceiptInvoiceAfterPostingDefault

		public ZBool OM_ARReceiptInvoiceAfterPostingDefault
		{
			get { return Header.CompanyData.OB_ARReceiptInvoiceAfterPostingDefault; }
			set { Header.CompanyData.OB_ARReceiptInvoiceAfterPostingDefault = value; }
		}

		public ZPropertyInfo OM_ARReceiptInvoiceAfterPostingDefaultInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARReceiptInvoiceAfterPostingDefault, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARReceiptInvoiceAfterPostingDefaultInfo)); }
		}

		#endregion

		#region OM_ARTreatDisbursementsAsStandardValue

		public ZDecimal OM_ARTreatDisbursementsAsStandardValue
		{
			get { return Header.CompanyData.OB_ARTreatDisbursementsAsStandardValue; }
			set { Header.CompanyData.OB_ARTreatDisbursementsAsStandardValue = value; }
		}

		public ZPropertyInfo OM_ARTreatDisbursementsAsStandardValueInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARTreatDisbursementsAsStandardValue, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo)); }
		}

		#endregion

		#region OM_ARVATSplitPaymentApplicable

		public ZBool OM_ARVATSplitPaymentApplicable
		{
			get { return Header.CompanyData.OB_ARVATSplitPaymentApplicable; }
			set { Header.CompanyData.OB_ARVATSplitPaymentApplicable = value; }
		}

		public ZPropertyInfo OM_ARVATSplitPaymentApplicableInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARVATSplitPaymentApplicable, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARVATSplitPaymentApplicableInfo)); }
		}

		#endregion

		#region OM_ARWHTApplicable

		public ZBool OM_ARWHTApplicable
		{
			get { return Header.CompanyData.OB_ARWHTApplicable; }
			set { Header.CompanyData.OB_ARWHTApplicable = value; }
		}

		public ZPropertyInfo OM_ARWHTApplicableInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_ARWHTApplicable, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARWHTApplicableInfo)); }
		}

		#endregion

		#region OM_CMAuthorityToLeave

		[List("Lookups.AuthorityToLeaveOptions")]
		public override ZString OM_CMAuthorityToLeave
		{
			get { return base.OM_CMAuthorityToLeave; }
			set { base.OM_CMAuthorityToLeave = value; }
		}

		public ZBool CMisAuthorisedToLeaveWithFallback
		{
			get { return IsAuthorisedToLeaveWithFallback(OM_CMAuthorityToLeave); }
		}

#if DEBUG
		internal
#endif
		ZBool IsAuthorisedToLeaveWithFallback(ZString atlOption)
		{
			switch (atlOption)
			{
				case AuthorityToLeaveOptions.Codes.YES:
					return true;
				case AuthorityToLeaveOptions.Codes.NO:
					return false;
				default:
					return ObjectFactory.Get<TransportCommon.Integration.ITransportRegistry>().AuthorityToLeave.Value;
			}
		}

		#endregion

		#region OM_ConsignorAuthorityToLeave

		[List("Lookups.AuthorityToLeaveOptions")]
		public override ZString OM_ConsignorAuthorityToLeave
		{
			get { return base.OM_ConsignorAuthorityToLeave; }
			set { base.OM_ConsignorAuthorityToLeave = value; }
		}

		public ZBool CNRisAuthorisedToLeaveWithFallback
		{
			get { return IsAuthorisedToLeaveWithFallback(OM_ConsignorAuthorityToLeave); }
		}

		#endregion

		#region OM_ConsigneeAuthorityToLeave

		[List("Lookups.AuthorityToLeaveOptions")]
		public override ZString OM_ConsigneeAuthorityToLeave
		{
			get { return base.OM_ConsigneeAuthorityToLeave; }
			set { base.OM_ConsigneeAuthorityToLeave = value; }
		}

		public ZBool CNEisAuthorisedToLeaveWithFallback
		{
			get { return IsAuthorisedToLeaveWithFallback(OM_ConsigneeAuthorityToLeave); }
		}

		#endregion

		#region OM_CRCarrierCategory

		[List("OM_CRCarrierCategory_List")]
		public override ZString OM_CRCarrierCategory
		{
			get
			{
				return base.OM_CRCarrierCategory;
			}
			set
			{
				base.OM_CRCarrierCategory = value;
			}
		}

		#endregion

		#region OM_CISellingStyle

		[List("OM_CISellingStyle_List")]
		public override ZString OM_CISellingStyle
		{
			get
			{
				return base.OM_CISellingStyle;
			}
			set
			{
				base.OM_CISellingStyle = value;
			}
		}

		#endregion

		#region OM_CITypeOfService

		[List("OM_CITypeOfService_List")]
		public override ZString OM_CITypeOfService
		{
			get
			{
				return base.OM_CITypeOfService;
			}
			set
			{
				base.OM_CITypeOfService = value;
			}
		}

		#endregion

		#region OM_OJ_ARDebtorGroup
		[List("DebtorGroups")]
		public ZGuid OM_OJ_ARDebtorGroup
		{
			get { return Header.CompanyData.OB_OJ_ARDebtorGroup; }
			set { Header.CompanyData.OB_OJ_ARDebtorGroup = value; }
		}

		public ZPropertyInfo OM_OJ_ARDebtorGroupInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_OJ_ARDebtorGroup, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_OJ_ARDebtorGroupInfo)); }
		}

		protected bool OM_OJ_ARDebtorGroup_ReadOnly
		{
			get { return Header.CompanyData.OB_OJ_ARDebtorGroupInfo.ReadOnly; }
		}

		#endregion

		#region OM_RS_NKIMDefaultServiceLevel

		[List("ServiceLevels")]
		public override ZString OM_RS_NKIMDefaultServiceLevel
		{
			get
			{
				return base.OM_RS_NKIMDefaultServiceLevel;
			}
			set
			{
				base.OM_RS_NKIMDefaultServiceLevel = value;
			}
		}

		#endregion

		#region OM_RS_NKEXDefaultServiceLevel
		[List("ServiceLevels")]
		public override ZString OM_RS_NKEXDefaultServiceLevel
		{
			get
			{
				return base.OM_RS_NKEXDefaultServiceLevel;
			}
			set
			{
				base.OM_RS_NKEXDefaultServiceLevel = value;
			}
		}

		#endregion

		#region OM_RN_NKEXDefaultCntryOfOrigin
		[List("Countries")]
		public override ZString OM_RN_NKEXDefaultCntryOfOrigin
		{
			get
			{
				return base.OM_RN_NKEXDefaultCntryOfOrigin;
			}
			set
			{
				base.OM_RN_NKEXDefaultCntryOfOrigin = value;
			}
		}
		#endregion

		#region OM_RX_NKEXDefCurrency
		[List("Currencies")]
		public override ZString OM_RX_NKEXDefCurrency
		{
			get
			{
				return base.OM_RX_NKEXDefCurrency;
			}
			set
			{
				base.OM_RX_NKEXDefCurrency = value;
			}
		}
		#endregion

		#region OM_EXDefaultInvoicePriceFromProductLastCost
		[List("OM_EXInvPriceFromLastCost_List")]
		public override ZString OM_EXDefaultInvoicePriceFromProductLastCost
		{
			get
			{
				return base.OM_EXDefaultInvoicePriceFromProductLastCost;
			}
			set
			{
				base.OM_EXDefaultInvoicePriceFromProductLastCost = value;
			}
		}
		#endregion

		#region OM_EXDocumentAddressPreference
		[List("OM_EXDocumentAddressPreference_List")]
		public override ZString OM_EXDocumentAddressPreference
		{
			get
			{
				return base.OM_EXDocumentAddressPreference;
			}
			set
			{
				base.OM_EXDocumentAddressPreference = value;
			}
		}
		#endregion

		#region OM_EXExporterCategory
		[List("OM_EXExporterCategory_List")]
		public override ZString OM_EXExporterCategory
		{
			get
			{
				return base.OM_EXExporterCategory;
			}
			set
			{
				base.OM_EXExporterCategory = value;
			}
		}
		#endregion

		#region OM_EXDefaultIncoTerm
		[List("OM_EXDefaultIncoTerm_List")]
		public override ZString OM_EXDefaultIncoTerm
		{
			get
			{
				return base.OM_EXDefaultIncoTerm;
			}
			set
			{
				base.OM_EXDefaultIncoTerm = value;
			}
		}
		#endregion

		#region OM_IMEnablePromptToCreateProducts
		[List("Lookups.EnablePromptToCreateProductsList")]
		public override ZString OM_IMEnablePromptToCreateProducts
		{
			get
			{
				return base.OM_IMEnablePromptToCreateProducts;
			}
			set
			{
				base.OM_IMEnablePromptToCreateProducts = value;
			}
		}

		#endregion

		#endregion

		#region Payables (all overridden for OrgCompanyData)

		#region OM_APCategory
		[List("OM_APCategory_List")]
		public ZString OM_APCategory
		{
			get { return Header.CompanyData.OB_APCategory; }
			set { Header.CompanyData.OB_APCategory = value; }
		}

		public ZPropertyInfo OM_APCategoryInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_APCategory, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_APCategoryInfo)); }
		}

		#endregion

		#region OM_APCreditLimit

		public ZDecimal OM_APCreditLimit
		{
			get { return Header.CompanyData.OB_APCreditLimit; }
			set { Header.CompanyData.OB_APCreditLimit = value; }
		}

		public ZPropertyInfo OM_APCreditLimitInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_APCreditLimit, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_APCreditLimitInfo)); }
		}

		#endregion

		#region OM_APPayInvoiceAfterPostingDefault

		public ZBool OM_APPayInvoiceAfterPostingDefault
		{
			get { return Header.CompanyData.OB_APPayInvoiceAfterPostingDefault; }
			set { Header.CompanyData.OB_APPayInvoiceAfterPostingDefault = value; }
		}

		public ZPropertyInfo OM_APPayInvoiceAfterPostingDefaultInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_APPayInvoiceAfterPostingDefault, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_APPayInvoiceAfterPostingDefaultInfo)); }
		}

		#endregion

		#region OM_APWHTApplicable

		public ZBool OM_APWHTApplicable
		{
			get { return Header.CompanyData.OB_APWHTApplicable; }
			set { Header.CompanyData.OB_APWHTApplicable = value; }
		}

		public ZPropertyInfo OM_APWHTApplicableInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_APWHTApplicable, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_APWHTApplicableInfo)); }
		}

		#endregion

		#region OM_AB_APDefaultBankAccount
		[List("BankAccounts")]
		public ZGuid OM_AB_APDefaultBankAccount
		{
			get { return Header.CompanyData.OB_AB_APDefaultBankAccount; }
			set { Header.CompanyData.OB_AB_APDefaultBankAccount = value; }
		}

		public ZPropertyInfo OM_AB_APDefaultBankAccountInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_AB_APDefaultBankAccount, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_AB_APDefaultBankAccountInfo)); }
		}

		#endregion

		#region OM_AC_APDefaultChargeCode
		[List("ChargeCodes")]
		public ZGuid OM_AC_APDefaultChargeCode
		{
			get { return Header.CompanyData.OB_AC_APDefaultChargeCode; }
			set { Header.CompanyData.OB_AC_APDefaultChargeCode = value; }
		}

		public ZPropertyInfo OM_AC_APDefaultChargeCodeInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_AC_APDefaultChargeCode, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_AC_APDefaultChargeCodeInfo)); }
		}

		#endregion

		#region OM_OG_APCreditorGroup
		[List("CreditorGroups")]
		public ZGuid OM_OG_APCreditorGroup
		{
			get { return Header.CompanyData.OB_OG_APCreditorGroup; }
			set { Header.CompanyData.OB_OG_APCreditorGroup = value; }
		}

		public ZPropertyInfo OM_OG_APCreditorGroupInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_OG_APCreditorGroup, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_OG_APCreditorGroupInfo)); }
		}

		#endregion

		#region OM_APConsolidatedAccountingCategory

		[List("OM_ARConsolidatedAccountingCategory_List")]
		public ZString OM_APConsolidatedAccountingCategory
		{
			get { return Header.CompanyData.OB_ARConsolidatedAccountingCategory; }
			set { Header.CompanyData.OB_ARConsolidatedAccountingCategory = value; }
		}

		public ZPropertyInfo OM_APConsolidatedAccountingCategoryInfo
		{
			get { return GetWrappedZPropertyInfo(OMConstants.OM_APConsolidatedAccountingCategory, x => (Header == null || Header.CompanyData == null ? null : Header.CompanyData.OB_ARConsolidatedAccountingCategoryInfo)); }
		}

		public bool OM_APConsolidatedAccountingCategory_ReadOnly
		{
			get
			{
				bool hasSecurity = Header.SecurityProvider.HasModifyPayablesSecurity && Header.SecurityProvider.HasModifyConsolidationCategory;
				return !hasSecurity;
			}
		}

		#endregion

		#endregion

		#region Forwarder

		#region OM_FWAgentCategory

		[List("OM_FWAgentCategory_List")]
		public override ZString OM_FWAgentCategory
		{
			get
			{
				return base.OM_FWAgentCategory;
			}
			set
			{
				base.OM_FWAgentCategory = value;
			}
		}

		#endregion

		#region OM_RX_NKFWDefCurrency
		[List("Currencies")]
		public override ZString OM_RX_NKFWDefCurrency
		{
			get
			{
				return base.OM_RX_NKFWDefCurrency;
			}
			set
			{
				base.OM_RX_NKFWDefCurrency = value;
			}
		}
		#endregion

		#endregion

		#region Carrier

		#region OM_RM_Airline

		[RelatedBusinessObject("Airline")]
		[List("Lookups.NumericCodeAirlines")]
		public override ZGuid OM_RM_Airline
		{
			get { return base.OM_RM_Airline; }
			set
			{
				if (base.OM_RM_Airline != value)
				{
					base.OM_RM_Airline = value;
					RefreshAirlineDetails();
				}
			}
		}

		protected bool OM_RM_Airline_ReadOnly
		{
			get { return (Header != null && !Header.OH_IsAirLine); }
		}

		#region Airline Details

		void RefreshAirlineDetails()
		{
			AirlineThreeLetterCodeInfo.RefreshBinding();
			AirlineTwoCharacterCodeInfo.RefreshBinding();
			AirlineName1Info.RefreshBinding();
			AirlineName2Info.RefreshBinding();
			AirlineAddressLine1Info.RefreshBinding();
			AirlineAddressLine2Info.RefreshBinding();
			AirlineCityInfo.RefreshBinding();
			AirlineStateInfo.RefreshBinding();
			AirlinePostalCodeInfo.RefreshBinding();
			AirlineCountryInfo.RefreshBinding();
		}

		#region AirlineThreeLetterCode

		public ZString AirlineThreeLetterCode
		{
			get { return Airline != null ? Airline.RM_ThreeLetterCode : ZString.Empty; }
		}

		public ZPropertyInfo AirlineThreeLetterCodeInfo
		{
			get { return GetZPropertyInfo(nameof(AirlineThreeLetterCode)); }
		}

		#endregion

		#region AirlineTwoCharacterCode

		public ZString AirlineTwoCharacterCode
		{
			get { return Airline != null ? Airline.RM_TwoCharacterCode : ZString.Empty; }
		}

		public ZPropertyInfo AirlineTwoCharacterCodeInfo
		{
			get { return GetZPropertyInfo(nameof(AirlineTwoCharacterCode)); }
		}

		#endregion

		#region AirlineName1

		public ZString AirlineName1
		{
			get { return Airline != null ? Airline.RM_AirlineName1 : ZString.Empty; }
		}

		public ZPropertyInfo AirlineName1Info
		{
			get { return GetZPropertyInfo(nameof(AirlineName1)); }
		}

		#endregion

		#region AirlineName2

		public ZString AirlineName2
		{
			get { return Airline != null ? Airline.RM_AirlineName2 : ZString.Empty; }
		}

		public ZPropertyInfo AirlineName2Info
		{
			get { return GetZPropertyInfo(nameof(AirlineName2)); }
		}

		#endregion

		#region AirlineAddressLine1

		public ZString AirlineAddressLine1
		{
			get { return Airline != null ? Airline.RM_AddressLine1 : ZString.Empty; }
		}

		public ZPropertyInfo AirlineAddressLine1Info
		{
			get { return GetZPropertyInfo(nameof(AirlineAddressLine1)); }
		}

		#endregion

		#region AirlineAddressLine2

		public ZString AirlineAddressLine2
		{
			get { return Airline != null ? Airline.RM_AddressLine2 : ZString.Empty; }
		}

		public ZPropertyInfo AirlineAddressLine2Info
		{
			get { return GetZPropertyInfo(nameof(AirlineAddressLine2)); }
		}

		#endregion

		#region AirlineCity

		public ZString AirlineCity
		{
			get { return Airline != null ? Airline.RM_AirlineCity : ZString.Empty; }
		}

		public ZPropertyInfo AirlineCityInfo
		{
			get { return GetZPropertyInfo(nameof(AirlineCity)); }
		}

		#endregion

		#region AirlineState

		public ZString AirlineState
		{
			get { return Airline != null ? Airline.RM_AirlineState : ZString.Empty; }
		}

		public ZPropertyInfo AirlineStateInfo
		{
			get { return GetZPropertyInfo(nameof(AirlineState)); }
		}

		#endregion

		#region AirlinePostalCode

		public ZString AirlinePostalCode
		{
			get { return Airline != null ? Airline.RM_AirlinePostalCode : ZString.Empty; }
		}

		public ZPropertyInfo AirlinePostalCodeInfo
		{
			get { return GetZPropertyInfo(nameof(AirlinePostalCode)); }
		}

		#endregion

		#region AirlineCountry

		public ZString AirlineCountry
		{
			get { return Airline != null ? Airline.RM_AirlineCountry : ZString.Empty; }
		}

		public ZPropertyInfo AirlineCountryInfo
		{
			get { return GetZPropertyInfo(nameof(AirlineCountry)); }
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region DistanceCalculation

		[List("OM_CMDistanceCalculationProvider_List")]
		public override ZString OM_CMDistanceCalculationProvider
		{
			get { return base.OM_CMDistanceCalculationProvider; }
			set
			{
				base.OM_CMDistanceCalculationProvider = value;
				if (OM_CMDistanceCalculationVersion_List.DefaultCode != null)
				{
					OM_CMDistanceCalculationVersion = OM_CMDistanceCalculationVersion_List.DefaultCode;
				}
				else
				{
					OM_CMDistanceCalculationVersion = "";
				}
				if (OM_CMDistanceCalculationMethod_List.DefaultCode != null)
				{
					OM_CMDistanceCalculationMethod = OM_CMDistanceCalculationMethod_List.DefaultCode;
				}
				else
				{
					OM_CMDistanceCalculationMethod = "";
				}
			}
		}

		[List("OM_CMDistanceCalculationVersion_List")]
		public override ZString OM_CMDistanceCalculationVersion
		{
			get { return base.OM_CMDistanceCalculationVersion; }
			set { base.OM_CMDistanceCalculationVersion = value; }
		}

		protected bool OM_CMDistanceCalculationVersion_ReadOnly
		{
			get { return OM_CMDistanceCalculationProvider != DistanceCalculationConstants.Providers.PCMiler; }
		}

		[List("OM_CMDistanceCalculationMethod_List")]
		public override ZString OM_CMDistanceCalculationMethod
		{
			get { return base.OM_CMDistanceCalculationMethod; }
			set { base.OM_CMDistanceCalculationMethod = value; }
		}

		protected bool OM_CMDistanceCalculationMethod_ReadOnly
		{
			get { return OM_CMDistanceCalculationProvider != DistanceCalculationConstants.Providers.PCMiler; }
		}

		#endregion

		#region ReadOnly Properties

		protected bool OM_WhsDefaultWarehouseRollUp_ReadOnly
		{
			get
			{
				return OM_WhsDefaultWarehousePickMode != WhsPickMode.Codes.AttributeNeutral;
			}
		}

		protected bool OM_GG_OrgSecurityGroup_ReadOnly
		{
			get
			{
				return !Env.Security.GroupsManageOSMG.IsAllowed;
			}
		}

		#endregion

		#endregion

		#region Calendar Reminders

		internal bool ShouldCreateEstimatedCloseDateReminder
		{
			get { return OM_CMEstimatedDateToClose != OriginalOM_CMEstimatedDateToClose && Header.StaffAssignments.OverallSalesRepStaff != null; }
		}

		internal Reminder EstimatedCloseDateReminder
		{
			get
			{
				ZStringBuilder commonBodyBuilder = new ZStringBuilder();
				SalesReminderBuilder.AddAddress(commonBodyBuilder, Header.Addresses.MainAddress, false);
				string commonBody = commonBodyBuilder.ToString();

				string body = Res.GetString("e2f5ded8-3bf4-465d-ae7a-d1e8e99a39ef", "This is a reminder that the Estimated Sales Close Date is approaching for the sales lead {0} ({1})", Header.OH_FullNameTruncated, Header.OH_Code) + "\r\n\r\n";
				body += commonBody;

				string orgUrl = string.Format(@"<a href=""{0}"">{1}</a>",
					ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, Header.PK.ToGuid()),
					Header.OH_Code);
				string htmlBody = Res.GetString("e2f5ded8-3bf4-465d-ae7a-d1e8e99a39ef", "This is a reminder that the Estimated Sales Close Date is approaching for the sales lead {0} ({1})", Header.OH_FullNameTruncated, orgUrl) + "\r\n\r\n";
				htmlBody += commonBody;
				htmlBody = string.Format("<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{0}</BODY></HTML>", htmlBody);

				ZDateTime reminderDate = OM_CMEstimatedDateToClose.IsEmpty ? OriginalOM_CMEstimatedDateToClose : OM_CMEstimatedDateToClose;
				ReminderType type = OM_CMEstimatedDateToClose.IsEmpty ? ReminderType.Cancellation : ReminderType.Confirmed;
				Reminder result = new Reminder(Header.PK.ToString() + "EstimatedCloseDate", Header.PK, new ZString(Header.TableName), DateTimeKind.Local, reminderDate, reminderDate, Res.GetString("64e700ec-81b5-459a-95d9-07c15ad5e724", "Reminder of Close Date for Sales lead {0}", Header.OH_FullNameTruncated), body, htmlBody);
				result.ReminderType = type;

				if (Header.StaffAssignments.OverallSalesRepStaff != null)
				{
					result.Recipients.Add(Header.StaffAssignments.OverallSalesRepStaff.GS_FullName, Header.StaffAssignments.OverallSalesRepStaff.GS_EmailAddress);
				}

				return result;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (ShouldCreateEstimatedCloseDateReminder)
				{
					EstimatedCloseDateReminder.CreateAppointment();
				}

				OriginalOM_CMEstimatedDateToClose = OM_CMEstimatedDateToClose;
			}
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			AuthorisedToLeaveLogger.Log(this, OM_CMAuthorityToLeaveInfo, CM);
			AuthorisedToLeaveLogger.Log(this, OM_ConsigneeAuthorityToLeaveInfo, CNE);
			AuthorisedToLeaveLogger.Log(this, OM_ConsignorAuthorityToLeaveInfo, CNR);

			if (OM_ARGlobalOnCreditHoldInfo.HasChanges || (Header != null && !Header.IsInDatabase && OM_ARGlobalOnCreditHold))
			{
				var typeParameter = new KeyValuePair<string, string>(OrgCompanyData.LogParameterKeys.Type, OrgCompanyData.LogTypes.CreditOnHold);
				Logs.AddNew(Events.CreditControlsModified, String.Format(CultureInfo.InvariantCulture, "Credit Control and Settlement - {0}: {1}", OrgCompanyData.LogReferenceKeys.GlobalCreditOnHold, OM_ARGlobalOnCreditHold), typeParameter);
			}

			if ((OM_CMClientCommenced.IsValid && !IsInDatabase) || OM_CMClientCommencedInfo.HasChanges)
			{
				var query = new ZQuery(OrgCommissionAgreementSchema.CA0_OH_Customer, OM_OH);
				query.AddToFilter(OrgCommissionAgreementSchema.CA0_CommissionTriggerType, CommissionTriggerTypes.Codes.ClientCommencedDate);
				query.AddToFilter(OrgCommissionAgreementSchema.CA0_CA0_ParentVersion, DBNull.Value);
				query.AddToFilter(OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, SQLComparisonOperator.NotEqual, DBNull.Value);
				query.AddToFilter(OrgCommissionAgreementSchema.CA0_ReversedDateUtc, DBNull.Value);

				var relatedAgreements = Factory.Load<OrgCommissionAgreement>(query);
				relatedAgreements.ForEach(a => a.SendToCalculationQueue(ZDateTime.MinSmallDateTimeValue, ZBool.True));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Consignor text")]
		public const string CNR = "Consignor";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Consignee text")]
		public const string CNE = "Consignee";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Client text")]
		public const string CM = "Client";

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			string propertyName = property.Name;

			if (Header != null)
			{
				if (propertyName == OrgMiscServSchema.OM_EXDocumentAddressPreference.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyConsignorDetailsSecurity && !Header.SecurityProvider.HasModifyForwarderDetailsSecurity;
				}
				else if (propertyName == OrgMiscServSchema.OM_IMPartAttrib1Name.Name ||
							propertyName == OrgMiscServSchema.OM_IMPartAttrib2Name.Name ||
							propertyName == OrgMiscServSchema.OM_IMPartAttrib3Name.Name ||
							propertyName == OrgMiscServSchema.OM_IMPartAttrib1Type.Name ||
							propertyName == OrgMiscServSchema.OM_IMPartAttrib2Type.Name ||
							propertyName == OrgMiscServSchema.OM_IMPartAttrib3Type.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyWarehouseSecurity && !Header.SecurityProvider.HasModifyConsigneeDetailsSecurity;
				}
				else if (propertyName == OrgMiscServSchema.OM_OC_EXDefaultDGContact.Name ||
						propertyName == OrgMiscServSchema.OM_EXDefaultDGContactPhoneUsed.Name ||
						propertyName == "DGPhoneNumber" ||
						propertyName == OrgMiscServSchema.OM_IMUseExpiryDate.Name ||
						propertyName == OrgMiscServSchema.OM_IMUsePackingDate.Name ||
						propertyName == OrgMiscServSchema.OM_IMUseSerialNumber.Name ||
						propertyName == OrgMiscServSchema.OM_IMDefaultWarehousePickOption.Name ||
						propertyName == OrgMiscServSchema.OM_WhsDefaultWarehousePickMode.Name ||
						propertyName == OrgMiscServSchema.OM_WhsPackingSlipOrderBy.Name ||
						propertyName == OrgMiscServSchema.OM_WhsOrderFulfillmentRule.Name ||
						propertyName == "FIFOOption_WithRegistryFallback" ||
						propertyName == OrgMiscServSchema.OM_WhsOrderNumberUniquenessStrategy.Name ||
						propertyName == OrgMiscServSchema.OM_IMAttrib1IsKey.Name ||
						propertyName == OrgMiscServSchema.OM_IMAttrib2IsKey.Name ||
						propertyName == OrgMiscServSchema.OM_IMAttrib3IsKey.Name ||
						propertyName == OrgMiscServSchema.OM_IMSerialNumberIsKey.Name ||
						propertyName == OrgMiscServSchema.OM_IMInvoiceDetailReportSort.Name ||
						propertyName == OrgMiscServSchema.OM_WhsOrderDefaultPickPriority.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyWarehouseSecurity;
				}
				else if (propertyName == OrgMiscServSchema.OM_IMDocumentAddressPreference.Name ||
						propertyName == OrgMiscServSchema.OM_IMAirDepotFreeDays.Name ||
						propertyName == OrgMiscServSchema.OM_IMSeaDepotFreeDays.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyConsigneeDetailsSecurity && !Header.SecurityProvider.HasModifyForwarderDetailsSecurity;
				}
				else if (propertyName.Contains("_EX") || propertyName.Contains("_NKEX"))
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyConsignorDetailsSecurity;
				}
				else if (propertyName.Contains("_SV"))
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyServicesSecurity;
				}
				else if (propertyName.Contains("_FW") || propertyName.Contains("_NKFW"))
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyForwarderDetailsSecurity;
				}
				else if (propertyName.Contains("_CR"))
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyCarrierSecurity;
				}
				else if (propertyName.Contains("_IM") || propertyName.Contains("_NKIM") ||
						propertyName == OrgMiscServSchema.OM_MinimumShelfLifeAccepted.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyConsigneeDetailsSecurity;
				}
				else if (propertyName == OrgMiscServSchema.OM_LandedCostMarginPercent1.Name ||
						propertyName == OrgMiscServSchema.OM_LandedCostMarginPercent2.Name ||
						propertyName == OrgMiscServSchema.OM_LandedCostMarginPercent3.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyConsigneeLandedCostingSecurity;
				}
				else if (propertyName == OrgMiscServSchema.OM_CMClientPortalHomePage.Name)
				{
					shouldBeReadOnly = !(Header.IsInDatabase ? Header.SecurityProvider.HasModifyDetailsWebSecurity : Header.SecurityProvider.HasNewDetailsWebSecurity);
				}
				else if (propertyName == OrgMiscServSchema.OM_OH_ARGlobalCreditGroup.Name ||
						propertyName == OrgMiscServSchema.OM_RX_NKARGlobalCreditCurrency.Name ||
						propertyName == OrgMiscServSchema.OM_ARGlobalCreditLimit.Name ||
						propertyName == OrgMiscServSchema.OM_ARGlobalCreditApproved.Name ||
						propertyName == OrgMiscServSchema.OM_ARGlobalOnCreditHold.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasReceivablesGlobalCreditControlSecurity;
				}
				else if (propertyName.StartsWith("OM_CMDistanceCalculation"))
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyDetailsRatingAndTariffsSecurity;
				}
				else if (propertyName.StartsWith("OM_ConsigneeAuthorityToLeave"))
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyConsigneeDetailsSecurity;
				}
				else if (propertyName.StartsWith("OM_ConsignorAuthorityToLeave"))
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyConsignorDetailsSecurity;
				}
				else if (propertyName.Contains("_CM") || propertyName.Contains("_NKCM"))
				{
					if (propertyName == OrgMiscServSchema.OM_CMClientCommenced.Name ||
						propertyName == OrgMiscServSchema.OM_CMOverallEffectOfClientOnAirfreightCosts.Name ||
						propertyName == OrgMiscServSchema.OM_CMOverallEffectOfClientOnLCLCosts.Name ||
						propertyName == OrgMiscServSchema.OM_CMOverallEffectOfClientOnOtherCosts.Name ||
						propertyName == OrgMiscServSchema.OM_CMOverallEffectOfClientOnTEUCosts.Name ||
						propertyName == OrgMiscServSchema.OM_CMOverallEffectOfClientOnWarehousingCosts.Name ||
						propertyName == OrgMiscServSchema.OM_CMOverallClientRelation.Name ||
						propertyName == OrgMiscServSchema.OM_CMClientsDesireToRemain.Name ||
						propertyName == OrgMiscServSchema.OM_CMEaseClientCanBePoached.Name ||
						propertyName == OrgMiscServSchema.OM_CMAmountOfElectronicIntegration.Name ||
						propertyName == OrgMiscServSchema.OM_CMOverallClientRelation.Name)
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifySalesClientRelationshipSecurity;
					}
					else
					{
						shouldBeReadOnly = !Header.SecurityProvider.HasModifySalesClientSummarySecurity;
					}
				}
				else if (propertyName.Contains("_CI"))
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyCompetitorSecurity;
				}
				else if (propertyName == OrgMiscServSchema.OM_IsScanPackQtyAllowed.Name ||
						propertyName == OrgMiscServSchema.OM_IsLabelPrintedOnClosePackage.Name ||
						propertyName == OrgMiscServSchema.OM_IsAutoPackAllowed.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyConsigneeDetailsSecurity;
				}
				else if (propertyName == OrgMiscServSchema.OM_RM_Airline.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyCarrierSecurityAir;
				}
				else if (propertyName == OMConstants.VoyageRecyclingPeriodCode)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyCarrierSecuritySea;
				}
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				yield return new OrgMiscServUniqueIndexFailureHandler(this);
			}
		}

		class OrgMiscServUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public OrgMiscServUniqueIndexFailureHandler(OrgMiscServ orgMiscServ)
			{
				this.orgMiscServ = orgMiscServ;
			}

			readonly OrgMiscServ orgMiscServ;

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return OrgMiscServSchema.Constants.Indexes.FK_UC__OM_OH; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var query = new ZQuery(OrgMiscServSchema.OM_OH, orgMiscServ.Header.PK);
				query.AddToFilter(OrgMiscServSchema.PK, SQLComparisonOperator.NotEqual, orgMiscServ.PK);
				var orgMiscServInDB = orgMiscServ.Factory.LoadTop1<OrgMiscServ>(query);

				if (orgMiscServInDB == null)
				{
					return;
				}

				orgMiscServ.Header.DestroyAndReloadOrgMiscServ(true);

				notifier.ReportInformation(
					Res.GetString("dc0345de-99d7-4cfc-8a66-10bfc2d2a030", "While you were working, a constraint has been detected and corrected, please attempt to save again."),
					Res.GetString("f12945fa-2a95-416b-b4fc-b54713b1f785", "Organization Update Required"));
			}

			#endregion
		}

		#endregion
	}
}
