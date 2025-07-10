using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgConstants;
using OrdersBusiniess = Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(BaseJobDeclaration), "Invoices")]
	[CodeProperty(BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumber)]
	[DescriptionProperty(BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumber)]
	[SingleObjectAroundARow]
	[UniversalDataContext(DataContextType.CustomsCommercialInvoice)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.BaseJobComInvoiceHeader)]
	[UniversalCopyWithExtendedEntities(StartCopyMethod = "StartUniversalCopy", FinishCopyMethod = "FinishUniversalCopy")]
	[UniversalCopyMappingKeys(BaseJobComInvoiceHeader.Schema.JZ_JZ_GroupInvoiceFK)]
	[UniversalCopyIgnoreElement("GroupInvoiceFK")]
	[UniversalCopyExtraCollection("CustomFields", "IAddOnValue", GenCustomAddOnValueSchema.Constants.TableName, GenCustomAddOnValueSchema.Constants.XV_ParentID, GenCustomAddOnValueSchema.Constants.XV_ParentTableCode)]
	[UserDefinedValues]
	[UniversalCopyAddInfo]
	public partial class BaseJobComInvoiceHeader
		: CommonJobComInvoiceHeader,
		Integration.Customs.Shared.IBaseJobComInvoiceHeader,
		ICodeDescription,
		IExternalFactoryRefreshable,
		ISupportDataImporting,
		IDocManagerSupport,
		ICommonInvoice,
		ITemplateCopyable,
		ILandedCostDistributeTo,
		ILandedCostChargeHolder,
		ILandedCostExchangeRateHolder,
		IDeclarationProvider,
		ICurrencyConverterDataProviderWithFixedExRates,
		ICurrencyConverterProvider,
		IGroupInvoiceOrInvoice,
		IChargeApportionee,
		IWeightApportionee,
		IWeightHolder,
		IDocumentSupportable,
		IRegistryAccessingSupporter,
		ISequenceNumberHeader,
		IShortSequenceNumberLine,
		IUnitConverterDataProvider,
		ICommonNonApportionedChargeProvider<BaseInvoiceCharge>,
		ICurrencyProvider,
		ICDArchive,
		IUniversalXMLNoteParent,
		IWorkflowProvider,
		ICustomFieldProvider,
		IWorkflowAffectedPropertyProvider,
		ICusLinkPackageSupporter,
		IBaseInvoiceHeader,
		IInvoiceHeaderForProductCreation,
		OrdersBusiniess.IAttachOrders,
		IAddInfoChildSupporter,
		ITypeDeciderContext,
		ITriggerActionProvider,
		ICustomsFileParent,
		IWorkflowTriggerEventSource
	{
		#region Constants

		public new class Schema : AutoJobComInvoiceHeader.Schema
		{
			public const string InvoiceLineTotal = "InvoiceLineTotal";
			public const string IncoTerm = "IncoTerm";
			public const string IsJZ_InvoiceCurrExRateUserEnterable = "IsJZ_InvoiceCurrExRateUserEnterable";
			public const string SupplierName = "SupplierName";
			public const string JZ_ITOTIncoTerm = "JZ_ITOTIncoTerm";
			public const string InvoiceLineTotalCurrency = "InvoiceLineTotalCurrency";
			public const string JZ_Calc_Balance = "JZ_Calc_Balance";
			public const string JZ_Calc_BalanceString = "JZ_Calc_BalanceString";
			public const string JZ_Calc_ChargesExcludedFromITOT = "JZ_Calc_ChargesExcludedFromITOT";
			public const string JZ_Calc_LinesEntered = "JZ_Calc_LinesEntered";
			public const string JZ_Calc_GroupInvoice = "JZ_Calc_GroupInvoice";
			public const string JZ_Calc_TNI = "JZ_Calc_TNI";
			public const string JZ_RX_Calc_TNICurrency = "JZ_RX_Calc_TNICurrency";
			public const string JZ_Calc_FOBAmount = "JZ_Calc_FOBAmount";
			public const string JZ_Calc_FOBCurrency = "JZ_Calc_FOBCurrency";
			public const string JZ_Calc_CIFAmount = "JZ_Calc_CIFAmount";
			public const string JZ_Calc_CIFCurrency = "JZ_Calc_CIFCurrency";
			public const string JZ_Calc_OFTInInvoiceCurrency = "JZ_Calc_OFTInInvoiceCurrency";
			public const string JZ_Calc_ONSInInvoiceCurrency = "JZ_Calc_ONSInInvoiceCurrency";
			public const string JZ_MessageType = "JZ_MessageType";
			public const string NoOfPacksPackType = "NoOfPacksPackType";
			public const string EffectiveValuationDate = "EffectiveValuationDate";
			public const string ShipToPartyOrgPK = "ShipToPartyOrgPK";
			public const string SellerOrgPK = "SellerOrgPK";
			public const string SoldToPartyOrgPK = "SoldToPartyOrgPK";
			public const string ManufacturerOrgPK = "ManufacturerOrgPK";
			public const string ConsigneeOrgPK = "ConsigneeOrgPK";
			public const string SupplierOrgPK = "SupplierOrgPK";
			public const string BuyerOrgPK = "BuyerOrgPK";
			public const string ExporterOrgPK = "ExporterOrgPK";
			public const string IntermediateConsigneeOrgPK = "IntermediateConsigneeOrgPK";
			public const string SellingAgentOrgPK = "SellingAgentOrgPK";
			public const string DistributorOrgPK = "DistributorOrgPK";
			public const string PackagerOrgPK = "PackagerOrgPK";
			public const string ShipperOrgPK = "ShipperOrgPK";
			public const string ConsigneeAddressOrgPK = "ConsigneeAddressOrgPK";
		}

		#endregion

		public BaseJobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ApportionmentDirtyChangedEventHandler += new EventHandler(MarkApportionmentDirty);
		}

		public static readonly new BaseJobComInvoiceHeaderTypeDecider TypeDecider = new BaseJobComInvoiceHeaderTypeDecider();

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZQuery GetQuery(BaseJobDeclaration declaration)
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_GroupInvoice, SQLComparisonOperator.Equal, ZBool.False);
				result.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_JE, SQLComparisonOperator.Equal, declaration.PK);
				result.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_ClusterKey, SQLComparisonOperator.Equal, declaration.JE_ClusterKey);
				result.FetchOnlyFromLocalCache = !declaration.IsInDatabase;
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(BaseJobComInvoiceHeader);
			}
		}

		/// <summary>
		/// When user right-clicks the list of invoice headers and chooses "Auto-create one-liner" fronm
		/// the context menu, this will be called.  This will be called once for each invoice that is selected on the grid.
		/// It will look at a since invoice header and see whether there are already and invoce lines.  If yes do nothing.
		/// If no, create an EMPTY invoice ine using the follwong data from the parent:  Invoice Number, Invoice Line value and Invoice Line Weight.
		/// This vaules will be copied into the blank invoice line.
		/// </summary>
		public virtual void AutoCreateOneInvoiceLineFromHeaderDetailsIfNoLinesAlreadyExist()
		{
			if (JobComInvoiceLines.Count == 0)
			{
				var invLine = JobComInvoiceLines.AddNew();

				invLine.JI_Weight = this.JZ_Weight;
				invLine.JI_NetWeight = this.JZ_NetWeight;
				invLine.JI_Volume = this.JZ_Volume;

				invLine.JI_VolumeUQ = this.JZ_VolumeUQ;
				invLine.JI_WeightUQ = this.JZ_WeightUQ;
				invLine.JI_NetWeightUQ = this.JZ_NetWeightUQ;

				invLine.JI_LinePrice = this.JZ_InvoiceAmount;  // "LinePrice" does NOT refer to the price of a single item on the line.  It refers to the subtotal!  For example, if you have an invoice line for 10 widgets at a price 2 dollars each, that's a subtotal for this line of 20 dollars. However in this structure, the LinePrice (what most accountants would call the subtotal) is 20. Take care!  It's a crazy nomencalture, but hey-ho!
			}
		}

		/// <summary>
		/// Link container to invoice
		/// </summary>
		/// <param name="ContainerToAssign">Container number will be linked</param>
		public virtual void AssignContainerToInvoiceLines(ZString containerNumberToAssign)
		{
			var declaration = JobDeclaration;
			var container = declaration != null ? declaration.CusContainers.Cast<BaseCusContainer>().FirstOrDefault(x => x.CO_ContainerNumber == containerNumberToAssign) : null;
			if (container != null)
			{
				foreach (BaseJobComInvoiceLine line in this.InvoiceLines)
				{
					line.ToggleLinkageWithContainer(container, true);
				}
			}
		}

		#region Related Business Objects

		public override GlbBranch Branch
		{
			get { return PersistentDeclaration is BaseJobDeclaration declaration ? declaration.Branch : Factory.Load<GlbBranch>(JZ_GB); }
		}

		protected override void MarkAsNeedingValidationForMajorDataChangeCore()
		{
			base.MarkAsNeedingValidationForMajorDataChangeCore();
			if (JobDeclaration != null)
			{
				JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}

		#region IAttachOrders

		[ChildEditable(true)]
		public OrdersBusiniess.OrderCollection AttachedOrders
		{
			get
			{
				if (fAttachOrders == null)
				{
					fAttachOrders = new OrdersBusiniess.OrderCollection(this);
					RegisterEditableChildObject(fAttachOrders);
					fAttachOrders.CollectionCountChange -= new CollectionCountChangedEventHandler(AttachedOrders_CountChanged);
					fAttachOrders.CollectionCountChange += new CollectionCountChangedEventHandler(AttachedOrders_CountChanged);
				}

				return fAttachOrders;
			}
		}
		OrdersBusiniess.OrderCollection fAttachOrders;

		void AttachedOrders_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is OrdersBusiniess.Order)
			{
				this.HasChanges = true;
			}
		}

		public OrdersBusiniess.OrderCollection PossibleOrdersForAttachment_List
		{
			get { return OrdersBusiniess.Order.GetPossibleOrdersForAttachment_List(this); }
		}

		void OrdersBusiniess.IAttachOrders.SetDefaultsOnOrder(OrdersBusiniess.Order newOrder)
		{
			((IBusinessObjectInternals)newOrder).IsCopying = true;
			try
			{
				newOrder.BuyerPK = JZ_OH_Buyer;
				newOrder.SupplierPK = JZ_OH_Supplier;
				newOrder.JD_IncoTerm = JZ_IncoTerm;
			}
			finally
			{
				((IBusinessObjectInternals)newOrder).IsCopying = false;
			}
		}

		void OrdersBusiniess.IAttachOrders.OnOrderAttached(OrdersBusiniess.Order attachedOrder)
		{
			if (attachedOrder != null)
			{
				attachedOrder.RequiredDocuments.CopyToOtherCollection(DocsAndCartage.RequiredDocuments);
			}
		}
		#endregion

		#region JobComInvoiceLines

		public BaseJobComInvoiceLineViewCollection JobComInvoiceLines
		{
			get
			{
				if (fJobComInvoiceLines == null)
				{
					fJobComInvoiceLines = CreateNewJobComInvoiceLineCollection();
					if (fJobComInvoiceLines == null)
					{
						fJobComInvoiceLines = CreateNewInvoiceLineCollectionWhenDeclarationIsNull();
					}
					fJobComInvoiceLines.CountChanged += JobComInvoiceLines_CountChanged;
					fIsInvoiceLinesLoaded = true;
					fJobComInvoiceLines.ExternalFactoryRefreshEnabled = ExternalFactoryRefreshEnabled;

					AdditionalDeclarations.ForEach(d => d.RegisterEditableChildObject(fJobComInvoiceLines));
				}

				return fJobComInvoiceLines;
			}
		}

		public BaseJobComInvoiceLineViewCollection InvoiceLines
		{
			get { return JobComInvoiceLines; }
		}

		public void ReloadInvoiceLines()
		{
			if (fJobComInvoiceLines != null)
			{
				fJobComInvoiceLines.CountChanged -= JobComInvoiceLines_CountChanged;
				fIsInvoiceLinesLoaded = false;
				AdditionalDeclarations.ForEach(d => d.UnRegisterEditableChildObject(fJobComInvoiceLines));
				fJobComInvoiceLines = null;
			}
		}

		protected override IBusiness[] OtherChildrenToLoad()
		{
			return new IBusiness[] { JobComInvoiceLines };
		}

		#endregion

		public void AddInvoicChargesFetchHintsIfNeeded()
		{
			if (!hasInvoiceChargesFetchHintBeingAdded)
			{
				hasInvoiceChargesFetchHintBeingAdded = true;
				if (IsInDatabase)
				{
					Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, PK);
				}
				foreach (BaseJobComInvoiceLine invoiceLine in JobComInvoiceLines.Where(x => x.IsInDatabase))
				{
					Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoiceLine.PK);
				}
			}
		}
		bool hasInvoiceChargesFetchHintBeingAdded;

		public bool HasFetchForLoadChildEditableObjectsBeenCalled { get; set; }

		#region GetFetchStrategy
		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy(this);
		}
		#endregion

		#region IsInvoiceLinesLoaded
		public bool IsInvoiceLinesLoaded()
		{
			return fIsInvoiceLinesLoaded;
		}
		bool fIsInvoiceLinesLoaded;
		#endregion

		#region Charges

		AllChargesCollection Common.ICommonInvoice.AllCharges
		{
			get
			{
				if (fAllCharges == null)
				{
					fAllCharges = new AllChargesCollection(this);
					fAllCharges.Load();
				}
				return fAllCharges;
			}
		}
		AllChargesCollection fAllCharges;

		[ChildEditable(true)]
		public IJobComInvChargeCollection<BaseInvoiceCharge> Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = CreateNewJobComInvHeaderCharges();
					RegisterEditableChildObject(fCharges);
				}
				return fCharges;
			}
		}
		protected IJobComInvChargeCollection<BaseInvoiceCharge> fCharges;

		protected virtual IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
		{
			return new JobComInvChargeCollection<BaseInvoiceCharge>(this);
		}

		#endregion

		BaseJobDeclaration IDeclarationProvider.Declaration
		{
			get { return JobDeclaration; }
		}

		#region Master
		public BaseJobComInvoiceGroupHeader Master
		{
			get { return GroupHeader; }
		}
		#endregion

		#region CurrencyConverter

		public CurrencyConverter CurrencyConverter => Factory.GetValue(ref currencyConverterCached, GetNewCurrencyConverter);
		CachedProperty<CurrencyConverter> currencyConverterCached;

		protected virtual CurrencyConverter GetNewCurrencyConverter()
		{
			return new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, this);
		}

		public ZBool IsReciprocalRates
		{
			get { return PersistentDeclaration is BaseJobDeclaration declaration ? declaration.IsReciprocalRates : IsReciprocalRatesCore; }
		}

		protected virtual ZBool IsReciprocalRatesCore
		{
			get { return GlbCompany.CurrentCompany.GC_IsReciprocal; }
		}

		public ZString LocalCurrencyCode
		{
			get
			{
				if (LocalCurrencyCodeCore.IsEmpty)
				{
					if (PersistentDeclaration is BaseJobDeclaration declaration)
					{
						return declaration.LocalCurrencyCode;
					}

					var refCountry = Branch?.Company?.Country ?? GlbCompany.CurrentCompany.Country;
					return refCountry.RN_RX_NKLocalCurrency;
				}
				return LocalCurrencyCodeCore;
			}
		}

		public static ZString GetLocalCurrencyCodeFor(BaseJobComInvoiceHeader invoice)
		{
			return invoice?.LocalCurrencyCode ?? GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
		}

		public RefCurrency LocalCurrency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, LocalCurrencyCode) ?? GlbCompany.CurrentCompany.Country.LocalCurrency; }
		}

		public static RefCurrency GetLocalCurrencyFor(BaseJobComInvoiceHeader invoice)
		{
			return invoice?.LocalCurrency ?? RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
		}

		protected virtual ZString LocalCurrencyCodeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PaymentCurrencyConverter
		public CurrencyConverter PaymentCurrencyConverter
		{
			get
			{
				if (fPaymentCurrencyConverter == null)
				{
					fPaymentCurrencyConverter = new CurrencyConverterWithDataProvider(Factory, new BuyCurrencyConverterDataProvider(this));
				}
				return fPaymentCurrencyConverter;
			}
		}
		CurrencyConverter fPaymentCurrencyConverter;

		class BuyCurrencyConverterDataProvider : ICurrencyConverterDataProvider
		{
			public BuyCurrencyConverterDataProvider(IDeclarationProvider declarationProvider)
			{
				this.declarationProvider = declarationProvider;
			}

			readonly IDeclarationProvider declarationProvider;

			public ExchangeRateType RateType
			{
				get { return ZArchitecture.Core.ExchangeRateType.Buy; }
			}

			public int MaximumDaysToFallback
			{
				get { return 7; }
			}

			ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
			{
				get { return ZString.Empty; }// brendon/reviewer: should this use customs override values or the company values?
			}

			ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
			{
				get { return null; }
			}

			public ZDateTime DateOfValuation
			{
				get
				{
					var declaration = declarationProvider.Declaration;
					return declaration != null ? declaration.JE_ExportDate : ZDateTime.Today;
				}
			}

			GlbCompany ICurrencyConverterDataProvider.Company
			{
				get
				{
					var declaration = declarationProvider.Declaration;
					return declaration != null ? ((ICurrencyConverterDataProvider)declaration).Company : GlbCompany.CurrentCompany;
				}
			}
		}

		#endregion

		#region UnitConverter
		public UnitConverter UnitConverter
		{
			get { return fUnitConverter ?? (fUnitConverter = new UnitConverter(this)); }
		}
		UnitConverter fUnitConverter;

		#endregion

		#region HouseBill
		public Bill Bill
		{
			get { return Factory.Load<Bill>(JZ_CU_RelatedHouseBill); }
		}
		#endregion

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (!IsDeleted)
				{
					foreach (BaseJobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						result.Add(invoiceLine);
					}
				}
				return result.ToArray();
			}
		}

		#endregion

		#region Invoice Header Refs Collection

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public InvoiceHeaderRefsCollection InvoiceHeaderRefs
		{
			get
			{
				if (invoiceHeaderRefs == null)
				{
					invoiceHeaderRefs = GetNewInvoiceHeaderRefsCollection();
					RegisterEditableChildObject(invoiceHeaderRefs);
				}
				return invoiceHeaderRefs;
			}
		}
		InvoiceHeaderRefsCollection invoiceHeaderRefs;

		protected virtual InvoiceHeaderRefsCollection GetNewInvoiceHeaderRefsCollection()
		{
			return new InvoiceHeaderRefsCollection(this);
		}

		#endregion

		#region Effective Supplier and Importer
		public OrgHeader Importer_Effective
		{
			get { return Factory.Load<OrgHeader>(JZ_OH_Buyer_Effective); }
		}

		public OrgHeader Supplier_Effective
		{
			get { return Factory.Load<OrgHeader>(JZ_OH_Supplier_Effective); }
		}

		public virtual ZGuid JZ_OH_Buyer_Effective
		{
			get
			{
				var result = JZ_OH_Buyer;
				if (result.IsEmpty)
				{
					var declaration = JobDeclaration;
					if (declaration != null)
					{
						result = declaration.JE_OH_Importer;
					}
				}
				return result;
			}
		}

		public virtual ZGuid JZ_OH_Supplier_Effective
		{
			get
			{
				var result = JZ_OH_Supplier;
				if (result.IsEmpty)
				{
					var declaration = JobDeclaration;
					if (declaration != null)
					{
						result = declaration.JE_OH_Supplier;
					}
				}
				return result;
			}
		}
		#endregion

		#region Effective Consigee Address
		public OrgAddress EffectiveConsigeeAddress
		{
			get { return GetEffectiveConsigeeAddress(); }
		}
		protected virtual OrgAddress GetEffectiveConsigeeAddress()
		{
			return ConsigneeAddress ?? Importer_Effective?.MainAddress;
		}
		#endregion

		#region Effective Supplier Address
		public OrgAddress EffectiveSupplierAddress
		{
			get { return GetEffectiveSupplierAddress(); }
		}
		protected virtual OrgAddress GetEffectiveSupplierAddress()
		{
			return SupplierAddress ?? Supplier_Effective?.MainAddress;
		}
		#endregion

		#region New Read Only Binding Properties

		public ZString EffectiveUCR
		{
			get
			{
				var result = JZ_UCR;
				if (result.IsEmpty && JobDeclaration is BaseJobDeclaration declaration)
				{
					result = declaration.JE_UCR;
				}
				return result;
			}
		}

		public ZString IncoTerm
		{
			get { return JZ_IncoTerm; }
		}

		public ZPropertyInfo IncoTermInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncoTerm); }
		}

		[BusinessObjectTestExclude]
		[ReadOnly(true)]
		public virtual ZString SupplierName
		{
			get { return Supplier != null ? Supplier.OH_FullNameTruncated : ZString.Empty; }
			set { throw new NotSupportedException("This is only here so it can be overridden, There's code in NZ that does this to handle MISC Suppliers if you need it."); }
		}

		public virtual ZPropertyInfo SupplierNameInfo
		{
			get { return GetZPropertyInfo(Schema.SupplierName); }
		}

		public ZString ITOTIncoTerm => IncoTermAndChargeFactory.ITOTIncoTerm(IncoTerm, this);

		public virtual ZDecimal InvoiceLineTotal => Factory.GetValue(ref cachedInvoiceLineTotal, () => JZ_InvoiceAmount - JZ_Calc_ChargesExcludedFromITOT);
		CachedProperty<ZDecimal> cachedInvoiceLineTotal;

		public ZPropertyInfo InvoiceLineTotalInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.InvoiceLineTotal); }
		}

		public ZGuid InvoiceLineTotalCurrency
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, JZ_RX_NKInvoice_Currency);
				if (currency != null)
				{
					return currency.PK;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
		}

		public ZPropertyInfo InvoiceLineTotalCurrencyInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.InvoiceLineTotalCurrency); }
		}

		public virtual ZDecimal JZ_Calc_TNI
		{
			get { return JZ_Calc_CIFAmount - JZ_Calc_FOBAmount; }
		}

		public ZPropertyInfo JZ_Calc_TNIInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.JZ_Calc_TNI);
				result.HumanReadableName = Res.GetString("5afce763-7ea9-4579-9b0d-24678c811eae", "Overseas Freight and Insurance");
				return result;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CurrencyList))]
		public virtual ZGuid JZ_RX_Calc_TNICurrency
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, JZ_RX_NKInvoice_Currency);
				if (currency != null)
				{
					return currency.PK;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
		}

		public ZPropertyInfo JZ_RX_Calc_TNICurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_RX_Calc_TNICurrency); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.NoOfPacksPackType_List))]
		public virtual ZString NoOfPacksPackType
		{
			get { return JobDeclaration is BaseJobDeclaration declaration ? declaration.JE_TotalNoOfPacksPackType : ZString.Empty; }
		}

		public ZPropertyInfo NoOfPacksPackTypeInfo
		{
			get { return GetZPropertyInfo(Schema.NoOfPacksPackType); }
		}

		#endregion

		#region ApportionedCharges

		[ChildEditable(true)]
		public IJobComInvApportionedChargeCollection<BaseApportionedCharge> GroupCharges
		{
			get
			{
				if (fGroupCharges == null)
				{
					fGroupCharges = CreateNewApportionedChargeCollection();
					fGroupCharges.CountChanged += new CollectionCountChangedEventHandler(GroupCharges_CountChanged);
					RegisterEditableChildObject(fGroupCharges);
				}
				return fGroupCharges;
			}
		}
		protected IJobComInvApportionedChargeCollection<BaseApportionedCharge> fGroupCharges;

		protected virtual IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<BaseApportionedCharge>(this);
		}

		public ZString JZ_Calc_BalanceString
		{
			get
			{
				var declaration = this.JobDeclaration;

				return declaration != null && declaration.ApportionmentDirty ? Res.GetString("fa5f888c-56ad-494e-985b-50ce8f68b59a", "Apportionment Pending") : JZ_Calc_Balance.ToString(2);
			}
		}

		public ZPropertyInfo JZ_Calc_BalanceStringInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_Calc_BalanceString, x => JZ_Calc_BalanceInfo); }
		}

		[DecimalPlaces(2)]
		[ResourceStringData("BaseJobComInvoiceHeader.JZ_Calc_Balance", Caption = "Balance")]
		public ZDecimal JZ_Calc_Balance
		{
			get
			{
				var declaration = JobDeclaration;

				return declaration != null && declaration.ApportionmentDirty ? decimal.Zero : InvoiceLineTotal - JZ_Calc_LinesEntered;
			}
		}

		public ZPropertyInfo JZ_Calc_BalanceInfo
		{
			get => GetZPropertyInfo(Schema.JZ_Calc_Balance);
		}

		/// <summary>
		/// Line Price Total	: 10,000$
		/// XXX Charge			:    500$
		/// YYY Charge			:	 200$
		/// -----------------------------
		/// Invoice Total		: 10,700$
		///
		/// It returns 700$. And also it adds up line level charges too.
		/// </summary>
		public ZDecimal JZ_Calc_ChargesExcludedFromITOT
		{
			get
			{
				if (jZ_Calc_ChargesExcludedFromITOTCached == null)
				{
					jZ_Calc_ChargesExcludedFromITOTCached = new CachedProperty<ZDecimal>(Factory, delegate
					{
						AddInvoicChargesFetchHintsIfNeeded();
						var result = Charges.AmountToAddForITOT(Invoice_Currency);

						var declaration = JobDeclaration;
						if (declaration != null)
						{
							foreach (BaseJobComInvoiceLine invoiceLine in JobComInvoiceLines)
							{
								if (invoiceLine.IsValidForLineTotalCalculation)
								{
									foreach (BaseInvoiceLineCharge lineCharge in invoiceLine.Charges)
									{
										if (lineCharge.J7_Calc_IsIncludedInInvoiceAmount &&
											!lineCharge.J7_IsIncludedInITOT &&
											!Charges.HasChargeWithThisKey(lineCharge.ApportionChargeKey) && //if an invoice does not have this charge!
											!lineCharge.J7_AdjustedCharge)
										{
											if (lineCharge.IsDiscount)
											{
												result -= lineCharge.MoneyInInvoiceCurrency.Amount;
											}
											else
											{
												result += lineCharge.MoneyInInvoiceCurrency.Amount;
											}
										}
									}
								}
							}
						}

						return result;
					}
					);
				}
				return jZ_Calc_ChargesExcludedFromITOTCached.Value;
			}
		}
		CachedProperty<ZDecimal> jZ_Calc_ChargesExcludedFromITOTCached;

		public ZPropertyInfo JZ_Calc_ChargesExcludedFromITOTInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_ChargesExcludedFromITOT); }
		}

		public ZDecimal JZ_Calc_LinesEntered => (linesEntered ?? (linesEntered = new RecalculableCachedValue<ZDecimal>(GetJZ_Calc_LinesEntered))).Value;
		RecalculableCachedValue<ZDecimal> linesEntered;

		public void InvalidateJZ_Calc_LinesEnteredCache() => linesEntered?.InvalidateCache();

		ZDecimal GetJZ_Calc_LinesEntered()
		{
			var linesEntered = ZDecimal.Zero;
			if (JobDeclaration != null)
			{
				foreach (BaseJobComInvoiceLine line in JobComInvoiceLines)
				{
					if (line.IsValidForLineTotalCalculation)
					{
						linesEntered += line.LinePriceForBalanceCalc;
					}
				}
			}
			return linesEntered;
		}

		public ZPropertyInfo JZ_Calc_LinesEnteredInfo => GetZPropertyInfo(nameof(JZ_Calc_LinesEntered));

		public virtual int PackagesFreeStore
		{
			get { return 0; }// throw new ApplicationException("Override in concrete class"); }
		}

		public virtual int PackagesBond
		{
			get { return 0; }// throw new ApplicationException("Override in concrete class"); }
		}

		#endregion

		#region ICusLinkPackageSupporter

		PivotLevel ICusLinkPackageSupporter.PivotLevel => PivotLevel.Invoice;

		ZBool ICusLinkPackageSupporter.IsSupportPivot => SupportsChzPivotBetweenInvoiceHeaderAndPacking;

		BaseJobDeclaration ICusLinkPackageSupporter.Declaration => JobDeclaration;

		IBasePackagePivotCollection ICusLinkPackageSupporter.CusPackPivots => PackagesPivot;

		BaseCusLinkPackageValidation ICusLinkPackageSupporter.GetNewLinkPackValidation(BaseCusLinkPackage linkPackage)
		{
			return new InvoiceHeaderPackageValidation(linkPackage, this);
		}

		public ICusPackagePivot ToggleLinkageWithPackage(BasePackage package, bool value)
		{
			var result = this.ToggleLinkageWithPackageCore(package, value);

			if (package != null)
			{
				foreach (var invoiceLine in InvoiceLines.Cast<BaseJobComInvoiceLine>().ToArray())
				{
					invoiceLine.SynchronizeContainersPivotWithPackagesPivot(package, string.Empty);
				}
			}

			return result;
		}

		ZBool ICusLinkPackageSupporter.IsSupportEmptyPackType(BasePackage package) => IsSupportEmptyPackType(package);

		protected virtual ZBool IsSupportEmptyPackType(BasePackage package) => false;

		HashSet<ZString> ICusLinkPackageSupporter.DistinctPackageTypes
		{
			get
			{
				if (distinctPackageTypes == null)
				{
					distinctPackageTypes = new CachedProperty<HashSet<ZString>>(Factory, GetDistinctPackageTypesCore);
				}
				return distinctPackageTypes.Value;
			}
		}
		CachedProperty<HashSet<ZString>> distinctPackageTypes;

		protected virtual HashSet<ZString> GetDistinctPackageTypesCore()
		{
			var result = new HashSet<ZString>();
			foreach (InvoiceHeaderPackagePivot pivot in PackagesPivot)
			{
				if (pivot.Package is BasePackage package)
				{
					if (package.CW_MarksAndNos.IsEmpty)
					{
						result.Add(package.CW_PackType);
					}
					else
					{
						result.Add(package.PK.ToString());
					}
				}
				else
				{
					result.Add(ZString.Empty);
				}
			}
			return result;
		}

		#endregion

		#region Packages

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public InvoiceHeaderPackagePivotCollection PackagesPivot
		{
			get
			{
				if (fPackagesPivot == null)
				{
					fPackagesPivot = GetNewPackagesPivotCore();
					if (SupportsChzPivotBetweenInvoiceHeaderAndPacking)
					{
						fPackagesPivot.Load();
						fPackagesPivot.IsManagedForDataRefresh = true;
						RegisterEditableChildObject(fPackagesPivot);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)fPackagesPivot).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						fPackagesPivot.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return fPackagesPivot;
			}
		}

		InvoiceHeaderPackagePivotCollection fPackagesPivot;

		protected virtual InvoiceHeaderPackagePivotCollection GetNewPackagesPivotCore()
		{
			return new InvoiceHeaderPackagePivotCollection(this);
		}

		public bool SupportsChzPivotBetweenInvoiceHeaderAndPacking
		{
			get
			{
				var declaration = JobDeclaration;
				return declaration == null || declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking;
			}
		}

		public BaseCusLinkPackageCollection PackagesForInvoicesForBindingOnly
		{
			get { return PackagesForInvoicesCore(); }
		}

		protected virtual BaseCusLinkPackageCollection PackagesForInvoicesCore()
		{
			return new BaseCusLinkPackageCollection(this);
		}

		#endregion

		#region GroupInvoice

		[BusinessObjectTestExclude()]
		[MaxLength(BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumberMaxLength)]
		public ZString JZ_Calc_GroupInvoice
		{
			get { return Master != null ? Master.JZ_InvoiceNumber : ZString.Empty; }
			set
			{
				if (JZ_Calc_GroupInvoice.ToUpper() != value.ToUpper())
				{
					GroupHeaderCollection groupHeaders = Lookups.JZ_JZ_GroupInvoiceFK_List;
					foreach (BaseJobComInvoiceGroupHeader groupHeader in groupHeaders)
					{
						if (groupHeader.JZ_InvoiceNumber.ToUpper() == value.ToUpper())
						{
							JZ_JZ_GroupInvoiceFK = groupHeader.PK;
							MarkApportionmentDirty(this, EventArgs.Empty);
							break;
						}
					}
				}
				JZ_Calc_GroupInvoiceInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude()]
		public override ZBool JZ_GroupInvoice
		{
			get { return base.JZ_GroupInvoice; }
			set
			{
				if (value)
				{
					throw new NotSupportedException("Cannot convert an invoice header into a group header");
				}
				base.JZ_GroupInvoice = value;
			}
		}

		protected override bool IsLookupsCachedInBase
		{
			get { return false; }
		}

		public ZPropertyInfo JZ_Calc_GroupInvoiceInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_GroupInvoice); }
		}

		/// <summary>
		/// Including grand-parents
		/// </summary>
		public ReadOnlyCollection<BaseJobComInvoiceGroupHeader> AllGroupInvoices
		{
			get
			{
				List<BaseJobComInvoiceGroupHeader> result = new List<BaseJobComInvoiceGroupHeader>();

				BaseJobComInvoiceGroupHeader currentGroup = GroupHeader;
				while (currentGroup != null)
				{
					result.Add(currentGroup);
					currentGroup = currentGroup.GroupHeader;
				}
				return new ReadOnlyCollection<BaseJobComInvoiceGroupHeader>(result);
			}
		}

		#endregion

		#region Message Type

		/// <summary>
		/// Use only when not on a persistent declaration.
		/// </summary>
		[BusinessObjectTestExclude()]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.MessageTypes))]
		[MaxLength(3)]
		public virtual ZString JZ_MessageType
		{
			get
			{
				if (PersistentDeclaration is BaseJobDeclaration declaration)
				{
					return declaration.JE_MessageType;
				}
				else
				{
					DeserialiseMessageTypeFromStandaloneMessageTypeIfRequired();
					return fJZMessageType;
				}
			}
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobComInvoiceHeader.Schema.JZ_MessageType))
				{
					CheckNotOnPersistentDeclaration();
					needToDeserialiseMessageTypeFromStandaloneMessageType = false;
					if (fJZMessageType != value)
					{
						var oldValue = JZ_MessageType;
						var originalEnabledStatus = !isDeserialiseMessageTypeFromStandaloneMessageTypeInProgress && !IsAdvanceShippingNotice;
						fJZMessageType = value;
						UpdateJobDeclarationMessageType(value);
						HasChanges = true;
						JZ_MessageTypeInfo.RefreshBinding(oldValue);
						var newEnabledStatus = !isDeserialiseMessageTypeFromStandaloneMessageTypeInProgress && !IsAdvanceShippingNotice;
						if (originalEnabledStatus != newEnabledStatus)
						{
							UpdatePartSyncManagerAndRefreshOnAllInvoiceLines(newEnabledStatus);
						}
					}
					if (!IsValidationSuspended)
					{
						InvoiceHeaderValidation?.ValidateJZ_MessageType();
					}
				}
			}
		}
		ZString fJZMessageType;

		public ZBool IsAdvanceShippingNotice
		{
			get { return JZ_MessageType == JobMessageTypeList.MoreCodes.AdvanceShippingNotice; }
		}

		public T GetPersistentDeclaration<T>(Func<T, bool> matchingData)
			where T : BaseJobDeclaration
		{
			return !JZ_JE.IsEmpty && JobDeclaration is T declaration && declaration.IsPersistent && matchingData(declaration) ? declaration : null;
		}

		public BaseJobDeclaration PersistentDeclaration => GetPersistentDeclaration<BaseJobDeclaration>((x) => true);

		public ZBool IsAttachedToPersistentDeclaration
		{
			get { return !JZ_JE.IsEmpty && (JobDeclaration?.IsPersistent ?? false); }
		}

		void UpdateJobDeclarationMessageType(ZString newMessageType)
		{
			if (!JZ_JE.IsEmpty && JobDeclaration is BaseJobDeclaration declaration)
			{
				using (declaration.SuspendSettingHasChanges())
				using (declaration.SuspendDataChangeByFakeDeclaration())
				{
					declaration.JE_MessageType = newMessageType;
				}
			}
		}

		void CheckNotOnPersistentDeclaration()
		{
			if (IsAttachedToPersistentDeclaration)
			{
				ErrorReporter.ReportOnce("MessageTypeUseDec", "Use Message type on the Declaration. The header is attached to a persistent Declaration.");
			}
		}

		public ZPropertyInfo JZ_MessageTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_MessageType); }
		}

		void ResetJZ_GB()
		{
			if (IsAttachedToPersistentDeclaration)
			{
				JZ_GB = ZGuid.Empty;
			}
			else
			{
				JZ_StandAloneInvoiceDirection = JZ_MessageType;
				if (JZ_GB.IsEmpty)
				{
					JZ_GB = GlbBranch.CurrentBranch.PK;
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			ResetJZ_GB();

			if (!IsAttachedToPersistentDeclaration)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (IsAttachedToPersistentDeclaration)
			{
				Transports.RemoveAndDeleteAll();
			}
			if (IsInDatabase)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JZ_JE), ConcurrencyPolicy.Strict);
			}
		}

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();
			oldDeclarationPK = JZ_JE;
		}

		ZGuid oldDeclarationPK;

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			if (oldDeclarationPK.IsValid && JZ_JE != oldDeclarationPK)
			{
				var oldDec = Factory.Load<BaseJobDeclaration>(oldDeclarationPK);
				if (oldDec != null)
				{
					foreach (var invoiceLine in JobComInvoiceLines.ToArray())
					{
						oldDec.InvoiceLines.Remove(invoiceLine);
					}

					if (oldDec.IsPersistent)
					{
						oldDec.MarkApportionmentDirty();

						oldDec.Invoices.CountChanged -= Invoices_CountChanged_OnUpdatedByDataRefresh;
						oldDec.Invoices.CountChanged += Invoices_CountChanged_OnUpdatedByDataRefresh;
					}

					oldDec.RefreshSortedInvoiceList();
					DetachInvoiceRoutings(oldDec);
				}
			}
			oldDeclarationPK = ZGuid.Empty;
		}

		void Invoices_CountChanged_OnUpdatedByDataRefresh(object sender, EventArgs e)
		{
			var invoices = sender as InvoiceHeaderActiveCollection;
			if (invoices != null)
			{
				var oldDec = invoices.declaration;
				if (oldDec != null)
				{
					oldDec.InvoiceNumberGenerator.ReCalculateAll();
					oldDec.ReApportionInvoiceWeightIfNeeded(true);
				}
				invoices.CountChanged -= Invoices_CountChanged_OnUpdatedByDataRefresh;
			}
		}

		void DeserialiseMessageTypeFromStandaloneMessageTypeIfRequired()
		{
			if (needToDeserialiseMessageTypeFromStandaloneMessageType)
			{
				using (SuspendSettingHasChanges())
				{
					needToDeserialiseMessageTypeFromStandaloneMessageType = false;
					try
					{
						isDeserialiseMessageTypeFromStandaloneMessageTypeInProgress = true;
						JZ_MessageType = JZ_StandAloneInvoiceDirection;
					}
					finally
					{
						isDeserialiseMessageTypeFromStandaloneMessageTypeInProgress = false;
					}
					if (JZ_MessageType.IsEmpty && JobDeclaration is BaseJobDeclaration declaration)
					{
						JZ_MessageType = GetMessageTypeFromDeclaration(declaration);
					}
				}
			}
		}
		bool needToDeserialiseMessageTypeFromStandaloneMessageType = true;
		bool isDeserialiseMessageTypeFromStandaloneMessageTypeInProgress;

		protected virtual ZString GetMessageTypeFromDeclaration(BaseJobDeclaration declaration) => declaration.JE_MessageType;

		public Directions JobDirection
		{
			get
			{
				if (IsImport)                           // State of an object should not be based on a set of Boolean properties
				{
					return Directions.Import;           // Instead state should be defined here in one place and Boolean properties should be based on it
				}

				if (IsExport)                           // Just like I've done it in all other solutions
				{
					return Directions.Export;           // If say attached JobDeclaration's direction is CrossTrade then this code will break returning Unknown while should be CrossTrade as well
				}

				return Directions.Unknown;              // However Joo insisted it to be this way
			}
		}

		public ZBool IsImport
		{
			get
			{
				if (isImportCached == null)
				{
					isImportCached = new CachedProperty<ZBool>(Factory, () => PersistentDeclaration is BaseJobDeclaration declaration
						? declaration.IsImport
						: IsImportForStandAloneInvoice);
				}

				return isImportCached.Value;
			}
		}
		CachedProperty<ZBool> isImportCached;

		protected virtual ZBool IsImportForStandAloneInvoice
		{
			get { return JZ_MessageType == Customs.Business.JobMessageTypeList.Codes.Import || IsAdvanceShippingNotice; }
		}

		public ZBool IsExport
		{
			get
			{
				if (isExportCached == null)
				{
					isExportCached = new CachedProperty<ZBool>(Factory, () => PersistentDeclaration is BaseJobDeclaration declaration
						? declaration.IsExport
						: IsExportForStandAloneInvoice);
				}

				return isExportCached.Value;
			}
		}
		CachedProperty<ZBool> isExportCached;

		protected virtual ZBool IsExportForStandAloneInvoice
		{
			get { return JZ_MessageType == Customs.Business.JobMessageTypeList.Codes.Export; }
		}

		public ZBool IsAttachedToPersistentExportDeclaration
		{
			get { return !JZ_JE.IsEmpty && JobDeclaration is BaseJobDeclaration declaration && declaration.IsPersistent && declaration.IsExport; }
		}

		#endregion

		#region ShipToPartyOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ShipToParties))]
		public ZGuid ShipToPartyOrgPK
		{
			get { return JZ_OA_ShipToPartyAddress_ZAddress.OrgPK; }
			set { JZ_OA_ShipToPartyAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ShipToPartyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShipToPartyOrgPK, x => JZ_OA_ShipToPartyAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region ManufacturerOrgPK

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SupplierList))]
		[ResourceStringData("0788FB93-C33E-4132-BE10-3620B604E133", Caption = "Manufacturer")]
		public virtual ZGuid ManufacturerOrgPK
		{
			get { return JZ_OA_ManufacturerAddress_ZAddress.OrgPK; }
			set { JZ_OA_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ManufacturerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerOrgPK, x => JZ_OA_ManufacturerAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region ExporterOrgPK

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.Exporters))]
		public virtual ZGuid ExporterOrgPK
		{
			get { return JZ_OA_ExporterAddress_ZAddress.OrgPK; }
			set { JZ_OA_ExporterAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ExporterOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ExporterOrgPK, x => JZ_OA_ExporterAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region SupplierOrgPK

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SupplierList))]
		public virtual ZGuid SupplierOrgPK
		{
			get { return JZ_OA_SupplierAddress_ZAddress.OrgPK; }
			set { JZ_OA_SupplierAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo SupplierOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SupplierOrgPK, x => JZ_OA_SupplierAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region BuyerOrgPK

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ImporterList))]
		public virtual ZGuid BuyerOrgPK
		{
			get { return JZ_OA_BuyerAddress_ZAddress.OrgPK; }
			set
			{
				JZ_OA_BuyerAddress_ZAddress.OrgPK = value;
				if (!IsValidationSuspended)
				{
					InvoiceHeaderValidation?.ValidateBuyerOrgPK();
				}
				BuyerOrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BuyerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.BuyerOrgPK, x => JZ_OA_BuyerAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region SellerOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SellerConsignors))]
		public virtual ZGuid SellerOrgPK
		{
			get { return JZ_OA_SellerAddress_ZAddress.OrgPK; }
			set
			{
				JZ_OA_SellerAddress_ZAddress.OrgPK = value;
				if (!IsValidationSuspended)
				{
					InvoiceHeaderValidation?.ValidateSellerOrgPK();
				}
				SellerOrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SellerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SellerOrgPK, x => JZ_OA_SellerAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader Seller
		{
			get { return Factory.Load<OrgHeader>(SellerOrgPK); }
		}

		#endregion

		#region SoldToPartyOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SoldToParties))]
		public ZGuid SoldToPartyOrgPK
		{
			get { return JZ_OA_SoldToPartyAddress_ZAddress.OrgPK; }
			set { JZ_OA_SoldToPartyAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo SoldToPartyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SoldToPartyOrgPK, x => JZ_OA_SoldToPartyAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader SoldToParty
		{
			get { return Factory.Load<OrgHeader>(SoldToPartyOrgPK); }
		}

		#endregion

		#region ConsigneeAddressOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.Consignees))]
		public ZGuid ConsigneeAddressOrgPK
		{
			get { return JZ_OA_ConsigneeAddress_ZAddress.OrgPK; }
			set { JZ_OA_ConsigneeAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ConsigneeAddressOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeAddressOrgPK, x => JZ_OA_ConsigneeAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader ConsigneeOrgAddress
		{
			get { return Factory.Load<OrgHeader>(ConsigneeAddressOrgPK); }
		}

		#endregion

		#region DistributorOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.Distributors))]
		public virtual ZGuid DistributorOrgPK
		{
			get { return JZ_OA_DistributorAddress_ZAddress.OrgPK; }
			set { JZ_OA_DistributorAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo DistributorOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DistributorOrgPK, x => JZ_OA_DistributorAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region PackagerOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.Packagers))]
		public ZGuid PackagerOrgPK
		{
			get { return JZ_OA_PackagerAddress_ZAddress.OrgPK; }
			set { JZ_OA_PackagerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo PackagerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.PackagerOrgPK, x => JZ_OA_PackagerAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region ShipperOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.Shippers))]
		[ResourceStringData("BaseJobComInvoiceHeader.ShipperOrgPK", Caption = "Shipper")]
		public ZGuid ShipperOrgPK
		{
			get { return JZ_OA_ShipperAddress_ZAddress.OrgPK; }
			set { JZ_OA_ShipperAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ShipperOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShipperOrgPK, x => JZ_OA_ShipperAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region IDocAddresses

		#region DocAddresses

		[ChildEditable(true)]
		JobDocAddressDependentCollection IDocAddresses.DocAddresses => DocAddresses;

		JobDocAddressDependentCollection IHaveInternalCartage.DocAddresses => DocAddresses;

		[ChildEditable(false)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get { return fDocAddresses ?? (fDocAddresses = GetDocAddressesCore()); }
		}
		JobDocAddressDependentCollection fDocAddresses;

		protected virtual JobDocAddressDependentCollection GetDocAddressesCore()
		{
			var docAddresses = new JobDocAddressDependentCollection(this);
			docAddresses.Load();
			RegisterEditableChildObject(docAddresses);
			return docAddresses;
		}

		#endregion

		#region DocAddressManager

		public JobDocAddressManager DocAddressManager
		{
			get { return fDocAddressManager ?? (fDocAddressManager = new JobDocAddressManager()); }
		}
		JobDocAddressManager fDocAddressManager;

		#endregion

		#region DocAddress Requirements

		public virtual ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return SupportedAddressTypesCore(); }
		}

		protected virtual DocAddressType[] SupportedAddressTypesCore()
		{
			return Array.Empty<DocAddressType>();
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress) => DocAddressChangedCore(docAddress);

		protected virtual void DocAddressChangedCore(JobDocAddress docAddress) { }

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return GetCanOverrideCheckpointCore(docAddress);
		}

		protected virtual SecurityCheckpoint GetCanOverrideCheckpointCore(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return CanDeleteAddressCore(docAddress);
		}

		protected virtual bool CanDeleteAddressCore(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#endregion

		#region IShipmentProvider

		CommonShipment IShipmentProvider.Shipment
		{
			get { return null; }
		}

		#endregion

		#region IDocsAndCartageParent

		public virtual Type DocsAndCartageType
		{
			get { return typeof(Freight.Forwarding.Business.ForwardingDocsAndCartage); }
		}

		public virtual Type DocsAndCartageParentType
		{
			get { return this.GetType(); }
		}

		IHaveRequiredDocuments IDocsAndCartageParent.RequiredDocumentsProvider
		{
			get { return DocsAndCartage; }
		}

		#endregion

		#region IShipmentWithDocsAndCartage

		public bool IsDocsAndCartageSet
		{
			get { return (fDocsAndCartage != null && !fDocsAndCartage.IsDeleted); }
		}

		protected void DeleteDocsAndCartage()
		{
			JobDocsAndCartage result = null;
			if (IsDocsAndCartageSet)
			{
				result = DocsAndCartage;
			}

			if (result != null)
			{
				UnRegisterEditableChildObject(result);
				result.Delete();
			}
			fDocsAndCartage = null;
		}

		JobDocsAndCartage IShipmentWithDocsAndCartage.DocsAndCartage => DocsAndCartage;

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "JP_ParentID,JP_ParentTableCode", DisableCopyMethodLink = true, DisableCopyMethodCopy = true)]

		public virtual JobDocsAndCartage DocsAndCartage
		{
			get
			{
				if (fDocsAndCartage == null)
				{
					fDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(this);
					RegisterListChangedCalledRefreshBinding(fDocsAndCartage);
				}
				else if (fDocsAndCartage.IsDeleted)
				{
					UnRegisterListChangedCalledRefreshBinding(fDocsAndCartage);
					fDocsAndCartage = null;
				}

				return fDocsAndCartage;
			}
		}

		JobDocsAndCartage fDocsAndCartage;

		JobDocAddress IShipmentWithDocsAndCartage.ConsignorDocumentaryAddress
		{
			get { return null; }
		}

		JobDocAddress IShipmentWithDocsAndCartage.ConsigneeDocumentaryAddress
		{
			get { return null; }
		}

		JobDocsAndCartageValidation IShipmentWithDocsAndCartage.PiggyBackedValidation
		{
			get { return null; }
		}

		string IShipmentWithDocsAndCartage.UniqueConsignRef
		{
			get { return null; }
		}

		string IShipmentWithDocsAndCartage.MasterBillNumber
		{
			get { return null; }
		}

		string IShipmentWithDocsAndCartage.HouseBillNumber
		{
			get { return null; }
		}

		OrgHeader IShipmentWithDocsAndCartage.ExportBroker
		{
			get { return null; }
		}

		event EventHandler IShipmentWithDocsAndCartage.TransportModeChanged
		{
			add { }
			remove { }
		}

		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryAndPickupCartageCoBeingSame()
		{
			return false;
		}

		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryCoPK()
		{
			return true;
		}

		bool IShipmentWithDocsAndCartage.RequiresOrderNumbersOnDocs()
		{
			return false;
		}

		bool IShipmentWithDocsAndCartage.RequiresOrderTrackLink()
		{
			return false;
		}

		#endregion

		#region IHaveInternalCartage

		Freight.Business.IContainer[] IHaveInternalCartage.GetContainers()
		{
			return null;
		}

		IPackLineInfo[] IHaveInternalCartage.GetPackLines()
		{
			return null;
		}

		public OrgHeader DeliveryCartageOrg
		{
			get { return null; }
		}

		public OrgHeader PickupCartageOrg
		{
			get { return null; }
		}

		ZString IHaveInternalCartage.ServiceLevel
		{
			get { return null; }
		}

		public ZBool DeliveryAndPickupCartageApplicable
		{
			get { return ZBool.False; }
		}

		public OrgAddress DepotOrCTOAddress
		{
			get { return null; }
		}

		ZGuid IHaveInternalCartage.CartagePickupDepotAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryDepotAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartagePickupCTOAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryCTOAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartagePickupContainerYardAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryContainerYardAddress
		{
			get { return ZGuid.Empty; }
		}

		JobDocAddress IHaveInternalCartage.CartageExporterDocAddress
		{
			get { return null; }
		}

		JobDocAddress IHaveInternalCartage.CartageImporterDocAddress
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupDepotAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryDepotAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupCTOAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryCTOAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupContainerYardAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryContainerYardAddressInfo
		{
			get { return null; }
		}

		ZString IHaveInternalCartage.TransportMode
		{
			get { return null; }
		}

		ZString IHaveInternalCartage.ContainerMode
		{
			get { return null; }
		}

		ZString IHaveInternalCartage.CartageTypeOverride
		{
			get { return ZString.Empty; }
		}

		bool IHaveInternalCartage.TypeSpecificPreCreationCheck()
		{
			return true;
		}

		ZPropertyInfo IHaveInternalCartage.DeliveryCartageAdvisedInfo
		{
			get { return DocsAndCartage.JP_DeliveryCartageAdvisedInfo; }
		}

		ZBool IHaveInternalCartage.PackedAtDepot
		{
			get { return false; }
		}

		public ZBool InternalCartageEnabled
		{
			get { return IsImport; }
		}

		bool IHaveInternalCartage.IsForPickupCartage
		{
			get { return IsExport; }
		}

		ZString IHaveInternalCartage.OwnerRef
		{
			get { return null; }
		}

		ZGuid IHaveInternalCartage.BranchPK
		{
			get { return JZ_GB; }
		}

		ZPropertyInfo IHaveInternalCartage.PickupCartageAdvisedInfo
		{
			get { return DocsAndCartage.JP_PickupCartageAdvisedInfo; }
		}

		bool IHaveInternalCartage.IsAllowedToUpdateAdviseDates
		{
			get { return true; }
		}
		#endregion

		#region ICartageExportSupport

		public string JobNumber
		{
			get { return JZ_InvoiceNumber; }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.Bills))]
		public override ZGuid JZ_CU_RelatedHouseBill
		{
			get { return base.JZ_CU_RelatedHouseBill; }
			set { base.JZ_CU_RelatedHouseBill = value; }
		}

		public override ZString JZ_RN_NKDefaultOrigin
		{
			get => base.JZ_RN_NKDefaultOrigin;
			set
			{
				base.JZ_RN_NKDefaultOrigin = value;
				InvoiceLines.MarkAsNeedingValidation();
			}
		}

		public ZDecimal InvoiceLineTotalInLocalCurrency
		{
			get { return ConvertToLocalAmountRounded(InvoiceLineTotal, Invoice_Currency).Amount; }
		}

		public virtual ZDecimal JZ_Calc_ConversionFactor
		{
			get
			{
				ZDecimal invoiceLineTotalCached = InvoiceLineTotal;
				if (invoiceLineTotalCached > 0)
				{
					return JZ_Calc_FOBAmountInLocalCurrency / invoiceLineTotalCached;
				}
				else
				{
					return 0m;
				}
			}
		}

		public LineWeightCalculator LineWeightCalculator
		{
			get { return new LineWeightCalculator(this); }
		}

		public LineVolumeCalculator LineVolumeCalculator
		{
			get { return new LineVolumeCalculator(this); }
		}

		public ZDate EffectiveDateForDutyRate
		{
			get { return EffectiveDateForDutyRateCore; }
		}

		protected virtual ZDate EffectiveDateForDutyRateCore
		{
			get { return JobDeclaration is BaseJobDeclaration declaration ? declaration.DateForDutyRate : ZDate.Today; }
		}

		public ZDateTime EffectiveValuationDate
		{
			get
			{
				var result = ZDateTime.Empty;

				var jobDeclaration = this.JobDeclaration;
				if (jobDeclaration != null && jobDeclaration.IsDrawback)
				{
					result = DrawbackValuationDate;
				}

				if (result.IsEmpty || !result.IsValid)
				{
					result = EffectiveValuationDateCore;
				}

				return result;
			}
		}

		public ZPropertyInfo EffectiveValuationDateInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveValuationDate); }
		}

		protected ZDateTime DrawbackValuationDate
		{
			get { return JZ_InvoiceDate; }
		}

		protected virtual ZDateTime EffectiveValuationDateCore
		{
			get
			{
				var jobDeclaration = this.JobDeclaration;
				return (JZ_ValuationDateOverride.IsEmpty || !JZ_ValuationDateOverride.IsValid) && jobDeclaration != null ? jobDeclaration.DateOfValuation : JZ_ValuationDateOverride;
			}
		}

		public CusEntryHeader FirstEntryHeader
		{
			get { return Entries.Length > 0 ? Entries[0] : null; }
		}

		#region Entries
		public CusEntryHeader[] Entries
		{
			get
			{
				if (fEntries == null)
				{
					if (isMergingInProgress)
					{
						string methodName = GetType().FullName + ".Entries";
						ErrorReporter.ReportOnce(methodName + " was accessed during merge", methodName + " should not be accessed during merge");
					}

					List<CusEntryHeader> result = new List<CusEntryHeader>();
					if (JobDeclaration is BaseJobDeclaration declaration)
					{
						foreach (CusEntryHeader entry in declaration.CustomsEntryHeaders)
						{
							if (entry.InvoiceHeaders.Contains(this))
							{
								result.Add(entry);
							}
						}
					}

					fEntries = result.ToArray();
				}
				return fEntries;
			}
		}
		CusEntryHeader[] fEntries;

		bool isMergingInProgress;

		internal void SetMergingInProgress(bool started)
		{
			isMergingInProgress = started;
			if (started)
			{
				RefreshEntries();
			}
		}

		internal void RefreshEntries()
		{
			fEntries = null;
		}
		#endregion

		public ISet<string> EntryInstructionProcedures => Factory.GetValue(ref entryInstructionProceduresCached, () => CusEntryInstructions.Select(instruction => instruction.CEI_Procedure.ToString()).Distinct().ToImmutableHashSet());
		CachedProperty<ISet<string>> entryInstructionProceduresCached;

		public bool IsABondedWarehousingInvoiceWithDifferentImporter
		{
			get
			{
				if (isABondedWarehousingInvoiceWithDifferentImporterCached == null)
				{
					isABondedWarehousingInvoiceWithDifferentImporterCached = new CachedProperty<bool>(Factory, GetIsABondedWarehousingInvoiceWithDifferentImporter);
				}
				return isABondedWarehousingInvoiceWithDifferentImporterCached.Value;
			}
		}
		CachedProperty<bool> isABondedWarehousingInvoiceWithDifferentImporterCached;

		bool GetIsABondedWarehousingInvoiceWithDifferentImporter()
		{
			var result = false;
			if (!JZ_JE.IsEmpty && !JZ_OH_Buyer.IsEmpty)
			{
				var declaration = JobDeclaration;
				if (declaration != null && declaration.IsPersistent && !declaration.IsExport && declaration.JE_OH_Importer != JZ_OH_Buyer && IsBondedWarehousingFieldValidationRequired)
				{
					result = JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(x));
				}
			}
			return result;
		}

		public bool IsABondedWarehousingInvoiceWithDifferentSupplier
		{
			get
			{
				if (isABondedWarehousingInvoiceWithDifferentSupplierCached == null)
				{
					isABondedWarehousingInvoiceWithDifferentSupplierCached = new CachedProperty<bool>(Factory, GetIsABondedWarehousingInvoiceWithDifferentSupplier);
				}
				return isABondedWarehousingInvoiceWithDifferentSupplierCached.Value;
			}
		}
		CachedProperty<bool> isABondedWarehousingInvoiceWithDifferentSupplierCached;

		bool GetIsABondedWarehousingInvoiceWithDifferentSupplier()
		{
			var result = false;
			if (!JZ_JE.IsEmpty && !JZ_OH_Supplier.IsEmpty)
			{
				var declaration = JobDeclaration;
				if (declaration != null && declaration.IsPersistent && declaration.IsExport && declaration.JE_OH_Supplier != JZ_OH_Supplier && IsBondedWarehousingFieldValidationRequired)
				{
					result = JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().Any(x => declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(x));
				}
			}
			return result;
		}

		public bool IsBondedWarehousingFieldValidationRequired
		{
			get
			{
				if (isBondedWarehousingFieldValidationRequiredCached == null)
				{
					isBondedWarehousingFieldValidationRequiredCached = new CachedProperty<bool>(Factory,
						() =>
						{
							var result = false;
							if (JobDeclaration is BaseJobDeclaration declaration)
							{
								if (declaration.SupportMultipleWarehouseEntry)
								{
									result = Entries.Any(x => x.IsBondedWarehousingFieldValidationRequired);
								}
								else
								{
									result = declaration.IsBondedWarehousingFieldValidationRequired;
								}
							}
							return result;
						});
				}
				return isBondedWarehousingFieldValidationRequiredCached.Value;
			}
		}
		CachedProperty<bool> isBondedWarehousingFieldValidationRequiredCached;

		public bool IsSplitInvoiceFor(ZString messageType)
		{
			int noOfEntries = 0;
			foreach (CusEntryHeader entry in Entries)
			{
				if (entry.CH_MessageType == messageType)
				{
					if (++noOfEntries > 1)
					{
						return true;
					}
				}
			}
			return false;
		}

		public
#if DEBUG  // for mock
 virtual
#endif
 ZDateTime DeclarationDate
		{
			get
			{
				if (declarationDate.IsEmpty)
				{
					CusEntryHeader entryHeader = FirstEntryHeader;
					declarationDate = entryHeader == null ? ZDateTime.Empty : entryHeader.DeclarationDate;
				}
				return declarationDate;
			}
		}
		ZDateTime declarationDate;

		public ZWeight EffectiveGrossWeight
		{
			get
			{
				ZWeight result = ZWeight.Empty;
				if (JZ_Weight != 0)
				{
					result = new ZWeight(JZ_Weight, JZ_WeightUQ);
				}
				else
				{
					foreach (BaseJobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						result += invoiceLine.EffectiveGrossWeight;
					}
				}
				return result;
			}
		}

		public ZDecimal EffectiveExchangeRateForInvoiceCurr =>
			Factory.GetValue(ref cachedEffectiveExchangeRateForInvoiceCurr, () => GetCurrencyConverterExchangeRate(CurrencyConverter, Invoice_Currency));
		CachedProperty<ZDecimal> cachedEffectiveExchangeRateForInvoiceCurr;

		public IEnumerable<ICurrencyProvider> GetCurrencyProvidersToRefreshExRatesFor()
		{
			var localCurrency = LocalCurrencyCode;

			if (!localCurrency.IsEmpty)
			{
				if (ShouldRefreshExRatesOnApportionment(this, localCurrency))
				{
					yield return this;
				}

				AddInvoicChargesFetchHintsIfNeeded();
				foreach (ICurrencyProvider charge in Charges)
				{
					if (ShouldRefreshExRatesOnApportionment(charge, localCurrency))
					{
						yield return charge;
					}
				}

				foreach (BaseJobComInvoiceLine invoiceLine in JobComInvoiceLines)
				{
					foreach (ICurrencyProvider charge in invoiceLine.Charges)
					{
						if (ShouldRefreshExRatesOnApportionment(charge, localCurrency))
						{
							yield return charge;
						}
					}
				}
			}
		}

		protected internal virtual bool ShouldRefreshExRatesOnApportionment(ICurrencyProvider currencyProvider, ZString localCurrency)
		{
			return currencyProvider.IsForeignCurrency(localCurrency);
		}

		[DecimalPlaces(6)]
		public override ZDecimal JZ_InvoiceCurrExRate
		{
			get { return base.JZ_InvoiceCurrExRate; }
			set
			{
				bool isDiff = base.JZ_InvoiceCurrExRate != value;
				base.JZ_InvoiceCurrExRate = value;
				if (isDiff && !IsCopying && IsJZ_InvoiceCurrExRateUserEnterable)
				{
					JobDeclaration?.ApportionInvoiceWeight(this);
					MarkApportionmentDirty(this, EventArgs.Empty);
				}
			}
		}

		[DecimalPlaces(6)]
		public override ZDecimal JZ_InvoiceCurrLandedCostExRate
		{
			get { return base.JZ_InvoiceCurrLandedCostExRate; }
			set { base.JZ_InvoiceCurrLandedCostExRate = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ExchangeRateTypeList))]
		public override ZString JZ_InvoiceCurrExRateType
		{
			get { return base.JZ_InvoiceCurrExRateType; }
			set
			{
				ZBool oldValue = IsJZ_InvoiceCurrExRateUserEnterable;
				base.JZ_InvoiceCurrExRateType = value;
				if (oldValue != IsJZ_InvoiceCurrExRateUserEnterable && !IsCopying)
				{
					JobDeclaration?.ApportionInvoiceWeight(this);
					IsJZ_InvoiceCurrExRateUserEnterableInfo.RefreshBinding(oldValue);
					MarkApportionmentDirty(this, EventArgs.Empty);
				}
			}
		}

		[BusinessObjectTestExclude]
		public ZBool IsJZ_InvoiceCurrExRateUserEnterable
		{
			get { return IsJZ_InvoiceCurrExRateUserEnterableCore; }
			set
			{
				ZBool oldValue = IsJZ_InvoiceCurrExRateUserEnterableCore;
				IsJZ_InvoiceCurrExRateUserEnterableCore = value;
				if (oldValue != IsJZ_InvoiceCurrExRateUserEnterableCore && !IsCopying)
				{
					SetExchangeRateIfNotUserEntered();
					IsJZ_InvoiceCurrExRateUserEnterableInfo.RefreshBinding(oldValue);
					MarkApportionmentDirty(this, EventArgs.Empty);
				}
			}
		}

		protected virtual void ResetToDefaultIsJZ_InvoiceCurrExRateUserEnterableIfPossible()
		{
			IsJZ_InvoiceCurrExRateUserEnterable = false;
		}

		protected virtual ZBool IsJZ_InvoiceCurrExRateUserEnterableCore
		{
			get { return JZ_InvoiceCurrExRateType == ChargeExchangeRateTypeList.Codes.FixedRate; }
			set
			{
				if (value)
				{
					JZ_InvoiceCurrExRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
				}
				else if (IsJZ_InvoiceCurrExRateUserEnterableCore)
				{
					JZ_InvoiceCurrExRateType = ZString.Empty;
				}
			}
		}

		public virtual ZPropertyInfo IsJZ_InvoiceCurrExRateUserEnterableInfo
		{
			get { return GetZPropertyInfo(Schema.IsJZ_InvoiceCurrExRateUserEnterable); }
		}

		protected virtual bool IsJZ_InvoiceCurrExRateUserEnterable_ReadOnly
		{
			get { return JZ_RX_NKInvoice_Currency.IsEmpty; }
		}

		public Money InvoiceAmount
		{
			get { return GetEffectiveMoney(new Money(JZ_InvoiceAmount, Invoice_Currency)); }
		}

		#region Overriden Properties

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (IsAttachedToPersistentDeclaration)
				{
					return base.HumanReadableNameCore;
				}
				else
				{
					var result = base.HumanReadableNameCore;
					if (!JZ_InvoiceNumber.IsEmpty)
					{
						result += " " + JZ_InvoiceNumber;
					}
					var parameters = new ZStringBuilder();
					var supplier = Supplier;
					if (supplier != null)
					{
						parameters.Append("SUP:" + supplier.OH_Code);
					}
					var buyer = Buyer;
					if (buyer != null)
					{
						parameters.Append("IMP:" + buyer.OH_Code);
					}
					if (!parameters.IsEmpty)
					{
						result += " (" + parameters.ToStringWithDelimiterBetweenAppends(" ") + ")";
					}
					return result;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.MessageStatusList))]
		public override ZString JZ_MessageStatus
		{
			get { return base.JZ_MessageStatus; }
			set { base.JZ_MessageStatus = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ImporterList))]
		public override ZGuid JZ_OH_Buyer
		{
			get { return base.JZ_OH_Buyer; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobComInvoiceHeader.Schema.JZ_OH_Buyer))
				{
					bool hasChanged = JZ_OH_Buyer != value;
					base.JZ_OH_Buyer = value;
					if (JobDeclaration is BaseJobDeclaration declaration && !declaration.IsPersistent && declaration.JE_OH_Importer != JZ_OH_Buyer)
					{
						declaration.JE_OH_Importer = JZ_OH_Buyer;
					}
					if (!IsCopying && hasChanged)
					{
						DefaultIncoTermAndCurrencyFromSupplier();
					}
					SetJZ_MessageTypeFromBuyerAndSupplier();
				}
			}
		}

		public override ZString JZ_InvoiceNumber
		{
			get { return base.JZ_InvoiceNumber; }
			set { base.JZ_InvoiceNumber = IsInvoiceNumberToUpper ? value.ToUpper() : value; }
		}

		protected virtual ZBool IsInvoiceNumberToUpper => ZBool.True;

		public override ZDateTime JZ_ValuationDateOverride
		{
			get
			{
				return base.JZ_ValuationDateOverride;
			}
			set
			{
				base.JZ_ValuationDateOverride = value;
				SetExchangeRateIfNotUserEntered();
			}
		}

		public virtual ZDecimal JZ_Calc_CIFAmount
			=> Factory.GetValue(ref cachedJZ_Calc_CIFAmount, () => InvoiceLineTotal + ValuationCalculator.GetAmountToAddToITOTForVatableGstable(Invoice_Currency));
		CachedProperty<ZDecimal> cachedJZ_Calc_CIFAmount;

		public ZPropertyInfo JZ_Calc_CIFAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_CIFAmount); }
		}

		public ZGuid JZ_Calc_CIFCurrency
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, JZ_RX_NKInvoice_Currency);
				if (currency != null)
				{
					return currency.PK;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
		}

		public ZDecimal JZ_Calc_CIFAmount_InLocalCurrency
		{
			get { return ConvertToLocalAmountRounded(JZ_Calc_CIFAmount, CalcCIFCurrency).Amount; }
		}

		public RefCurrency CalcCIFCurrency
		{
			get { return Invoice_Currency; }
		}

		public ZPropertyInfo JZ_Calc_CIFCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_CIFCurrency); }
		}

		#region JZ_FOBAmount
		public virtual ZDecimal JZ_Calc_FOBAmount //in invoice currency
			=> Factory.GetValue(ref cachedJZ_Calc_FOBAmount, () => InvoiceLineTotal + ValuationCalculator.GetAmountToAddToITOTForDutiable(Invoice_Currency));
		CachedProperty<ZDecimal> cachedJZ_Calc_FOBAmount;

		protected CustomsValuationCalculator ValuationCalculator => fValuationCalculator ?? (fValuationCalculator = GetValuationCalculatorCore());
		CustomsValuationCalculator fValuationCalculator;

		protected virtual CustomsValuationCalculator GetValuationCalculatorCore() => new CustomsValuationCalculator(this);

		public ZDecimal JZ_Calc_FOBAmountInLocalCurrency
		{
			get { return ConvertToLocalAmountRounded(JZ_Calc_FOBAmount, CalcFOBCurrency).Amount; }
		}

		public ZDecimal JZ_Calc_FOBAmountInLocalCurrencyRounded
		{
			get { return PerformLocalRounding(JZ_Calc_FOBAmountInLocalCurrency); }
		}

		public ZPropertyInfo JZ_Calc_FOBAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_FOBAmount); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CurrencyList))]
		public ZGuid JZ_Calc_FOBCurrency
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, JZ_RX_NKInvoice_Currency);
				if (currency != null)
				{
					return currency.PK;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
		}

		public RefCurrency CalcFOBCurrency
		{
			get { return Invoice_Currency; }
		}

		public ZPropertyInfo JZ_Calc_FOBCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_FOBCurrency); }
		}
		#endregion

		#region JZ_InvoiceAmount

		[DecimalPlaces(2)]
		public override ZDecimal JZ_InvoiceAmount
		{
			get
			{
				ZDecimal result = base.JZ_InvoiceAmount;
				if (result == 0 && JobDeclaration is BaseJobDeclaration declaration && declaration.IsExWarehouse)
				{
					result = JZ_Calc_LinesEntered;
				}
				return result;
			}
			set
			{
				ZDecimal oldValue = JZ_InvoiceAmount;
				base.JZ_InvoiceAmount = value;
				if (oldValue != JZ_InvoiceAmount)
				{
					if (IsSingleLine)
					{
						if (JobComInvoiceLines.Count == 1)
						{
							JobComInvoiceLines[0].JI_LinePrice = JZ_InvoiceAmount;
						}
					}

					if (!IsCopying && JobDeclaration is BaseJobDeclaration declaration)
					{
						declaration.ApportionInvoiceWeight(this);
					}
				}
			}
		}

		public Money JZ_InvoiceAmountInLocalCurrencyMoney => ConvertToLocalAmountRounded(InvoiceAmount);

		public ZDecimal JZ_InvoiceAmountInLocalCurrency => JZ_InvoiceAmountInLocalCurrencyMoney.Amount;

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get { return base.JZ_RX_NKInvoice_Currency; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency))
				{
					var oldValue = JZ_RX_NKInvoice_Currency;
					base.JZ_RX_NKInvoice_Currency = value;
					if (!IsCopying && oldValue != JZ_RX_NKInvoice_Currency)
					{
						SetChargeCurrency();

						if (JobDeclaration != null)
						{
							foreach (BaseJobComInvoiceLine invoiceline in JobComInvoiceLines)
							{
								invoiceline.SetChargeCurrencyToLinePriceRefCurrencyIfNeeded();
							}
						}

						ResetToDefaultIsJZ_InvoiceCurrExRateUserEnterableIfPossible();

						SetExchangeRateIfNotUserEntered();
						if (!JZ_InvoiceAmount.IsEmpty)
						{
							JobDeclaration?.ApportionInvoiceWeight(this);
						}
					}
				}
			}
		}
		#endregion

		[DecimalPlaces(2)]
		public override ZDecimal JZ_PaymentAmount
		{
			get { return base.JZ_PaymentAmount; }
			set { base.JZ_PaymentAmount = value; }
		}

		[DecimalPlaces(4)]
		public override ZDecimal JZ_PaymentExRate
		{
			get { return base.JZ_PaymentExRate; }
			set { base.JZ_PaymentExRate = value; }
		}

		#region Charges
		public ZString JZ_ITOTIncoTerm
		{
			get { return ITOTIncoTerm; }
		}

		public ZPropertyInfo JZ_ITOTIncoTermInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_ITOTIncoTerm); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.JZ_IncoTerm_List))]
		public override ZString JZ_IncoTerm
		{
			get { return base.JZ_IncoTerm; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_IncoTerm))
				{
					var oldValue = JZ_IncoTerm;
					base.JZ_IncoTerm = value;
					if (!IsCopying && oldValue != JZ_IncoTerm)
					{
						if (!((IBusinessObjectFactoryInternals)Factory).IsProcessingOnAllTransactionsCommitted)
						{
							new MandatoryChargesHandler(this).Execute();
						}

						GroupCharges.MarkAsNeedingValidation();
						SetIncludedInInvoiceFlagsForNonIncotermNeutralCharges();
					}
				}
			}
		}

		void SetIncludedInInvoiceFlagsForNonIncotermNeutralCharges()
		{
			if (!IncoTerm.IsEmpty)
			{
				Charges.SetIncludedInInvoiceFlagsForNonIncotermNeutralCharges();

				foreach (BaseJobComInvoiceLine invoiceLine in JobComInvoiceLines)
				{
					foreach (BaseInvoiceLineCharge lineCharge in invoiceLine.Charges)
					{
						lineCharge.ResetDefaultIsIncludedInAmountAndIsIncludedInITOTIfDetermined();
					}
				}
			}
		}

		[ResourceStringData("BaseJobComInvoiceHeader.JZ_Calc_OFTInInvoiceCurrency", Caption = "OFT In Invoice Currency")]
		public ZDecimal JZ_Calc_OFTInInvoiceCurrency
		{
			get { return ValuationCalculator.GetOverseasFreight(Invoice_Currency); }
		}

		public ZPropertyInfo JZ_Calc_OFTInInvoiceCurrencyInfo
		{
			get => GetZPropertyInfo(Schema.JZ_Calc_OFTInInvoiceCurrency);
		}

		[ResourceStringData("BaseJobComInvoiceHeader.JZ_Calc_ONSInInvoiceCurrency", Caption = "ONS In Invoice Currency")]
		public ZDecimal JZ_Calc_ONSInInvoiceCurrency
		{
			get { return ValuationCalculator.GetOverseasInsurance(Invoice_Currency); }
		}

		public ZPropertyInfo JZ_Calc_ONSInInvoiceCurrencyInfo
		{
			get => GetZPropertyInfo(Schema.JZ_Calc_ONSInInvoiceCurrency);
		}
		#endregion

		[DecimalPlaces(3)]
		public override ZDecimal JZ_Weight
		{
			get { return base.JZ_Weight; }
			set
			{
				var oldValue = JZ_Weight;
				base.JZ_Weight = value;
				if (!IsCopying && oldValue != JZ_Weight)
				{
					ApportionLineWeight(null);
				}
			}
		}

		public override ZString JZ_WeightUQ
		{
			get { return base.JZ_WeightUQ; }
			set
			{
				var oldValue = JZ_WeightUQ;
				base.JZ_WeightUQ = value;
				if (!IsCopying && oldValue != JZ_WeightUQ)
				{
					ApportionLineWeight(null);
				}
			}
		}

		[DecimalPlaces(3)]
		public override ZDecimal JZ_NetWeight
		{
			get { return base.JZ_NetWeight; }
			set
			{
				var oldValue = JZ_NetWeight;
				base.JZ_NetWeight = value;
				if (!IsCopying && oldValue != JZ_NetWeight)
				{
					ApportionLineWeight(null);
				}
			}
		}

		public override ZString JZ_NetWeightUQ
		{
			get { return base.JZ_NetWeightUQ; }
			set
			{
				var oldValue = JZ_NetWeightUQ;
				base.JZ_NetWeightUQ = value;
				if (!IsCopying && oldValue != JZ_NetWeightUQ)
				{
					ApportionLineWeight(null);
				}
			}
		}

		[DecimalPlaces(3)]
		public override ZDecimal JZ_Volume
		{
			get { return base.JZ_Volume; }
			set { base.JZ_Volume = value; }
		}

		public override ZGuid JZ_OH_Supplier
		{
			get { return base.JZ_OH_Supplier; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobComInvoiceHeader.Schema.JZ_OH_Supplier))
				{
					var oldValue = JZ_OH_Supplier;
					base.JZ_OH_Supplier = value;
					UpdateNonPersistentDeclarationWithInvoiceSupplier();
					if (!IsCopying && oldValue != JZ_OH_Supplier)
					{
						if (!IsSupplierAddressDefaultingSuspended && !SetterSuspender.IsSetterSuspended(BaseJobComInvoiceHeader.Schema.JZ_OA_SupplierAddress))
						{
							using (SuspendSupplierAddressDefaulting())
							{
								DefaultSupplierAddressFromSupplier(JZ_OH_Supplier);
							}
						}

						if (IsInDatabase || fJobComInvoiceLines != null)
						{
							JobComInvoiceLines.UpdateProductDetailsOnSupplierBuyerChange();
							JobComInvoiceLines.MarkAsNeedingValidation();
						}

						DefaultIncoTermAndCurrencyFromSupplier();
						DefaultResponsiblePartyFromSupplier();
					}
					SetJZ_MessageTypeFromBuyerAndSupplier();
					JZ_OH_SupplierInfo.RefreshBinding();
				}
			}
		}

		protected void DefaultResponsiblePartyFromSupplier()
		{
			if (InvoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs(InvoiceHeaderRefsTypeList.Codes.RP) == null)
			{
				var contact = Supplier?.Contacts.GetContactForAllocation(ContactAllocationType.CIV);
				if (contact != null)
				{
					InvoiceHeaderRefs.AddNew(InvoiceHeaderRefsTypeList.Codes.RP, contact.OC_ContactName);
				}
			}
		}

		protected virtual void DefaultSupplierAddressFromSupplier(ZGuid newSupplierPK)
		{
			if (!JZ_OA_SupplierAddress.IsEmpty && SupplierAddress?.OA_OH != newSupplierPK)
			{
				JZ_OA_SupplierAddress = ZGuid.Empty;
			}
		}

		void SetJZ_MessageTypeFromBuyerAndSupplier()
		{
			if (!IsAdvanceShippingNotice && JobDeclaration is BaseJobDeclaration declaration && !declaration.IsPersistent)// e.g. commerical invoice form
			{
				if (Buyer != null && Supplier != null)
				{
					if (ImportExportHelper.IsExport(Supplier.MainAddress.OA_RL_NKRelatedPortCode, Buyer.MainAddress.OA_RL_NKRelatedPortCode))
					{
						JZ_MessageType = JobMessageTypeList.Codes.Export;
					}
					else if (ImportExportHelper.IsImport(Supplier.MainAddress.OA_RL_NKRelatedPortCode, Buyer.MainAddress.OA_RL_NKRelatedPortCode))
					{
						JZ_MessageType = JobMessageTypeList.Codes.Import;
					}
				}
			}
		}

		bool IsSupplierAddressDefaultingSuspended => suspendSupplierAddressDefaulting != 0;

		IDisposable SuspendSupplierAddressDefaulting()
		{
			return new DisposableAction(() => suspendSupplierAddressDefaulting++, () => suspendSupplierAddressDefaulting--);
		}

		int suspendSupplierAddressDefaulting;

		public ZString BranchCompanyCountryCode => Branch?.Company?.GC_RN_NKCountryCode ?? ZString.Empty;

		public OrgSupplierBuyerLink SupplierBuyerLink
		{
			get
			{
				OrgSupplierBuyerLink result = null;
				var countryCode = PersistentDeclaration is BaseJobDeclaration declaration ? declaration.FinalDestinationCountryCode : ZString.Empty;
				result = GetSupplierBuyerLink(countryCode);
				if (result == null)
				{
					result = GetSupplierBuyerLink(BranchCompanyCountryCode);
				}
				return result;
			}
		}

		OrgSupplierBuyerLink GetSupplierBuyerLink(ZString countryCode)
		{
			return OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(SupplierForSuppplierLinkCalculation, BuyerForSuppplierLinkCalculation, countryCode);
		}

		protected virtual OrgHeader SupplierForSuppplierLinkCalculation
		{
			get { return IsAttachedToPersistentDeclaration ? Supplier_Effective : Supplier; }
		}

		protected virtual OrgHeader BuyerForSuppplierLinkCalculation
		{
			get { return IsAttachedToPersistentDeclaration ? Importer_Effective : Buyer; }
		}

		protected virtual ZString TransportModeForSuppplierLinkCalculation
		{
			get { return PersistentDeclaration is BaseJobDeclaration declaration ? declaration.JE_TransportMode : ZString.Empty; }
		}

		protected virtual ZString ContainerModeForSuppplierLinkCalculation
		{
			get { return PersistentDeclaration is BaseJobDeclaration declaration ? declaration.JE_ContainerMode : ZString.Empty; }
		}

		public DeclarationForProductCreationHelper GetProductCreationHelper()
		{
			return new DeclarationForProductCreationHelper(this);
		}

		protected void DefaultIncoTermAndCurrencyFromSupplier()
		{
			OrgHeader cachedSupplier = Supplier_Effective;
			var defaultIncoTerm = IsValidToDefaultIncoTermFromSupplier;
			var defaultCurrency = IsValidToDefaultCurrencyFromSupplier;

			if (cachedSupplier != null && cachedSupplier.PK != OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation)
			{
				ZBool isImport = IsImport;
				OrgMiscServ cachedMiscServ = cachedSupplier.MiscServ;
				if (cachedMiscServ != null)
				{
					if (!IsImportingData && defaultIncoTerm)
					{
						JZ_IncoTerm = IncoTermConverter.GetConvertedIncoTerm(cachedMiscServ.OM_EXDefaultIncoTerm, isImport);
					}

					if (!cachedMiscServ.OM_RX_NKEXDefCurrency.IsEmpty && defaultCurrency)
					{
						JZ_RX_NKInvoice_Currency = cachedMiscServ.OM_RX_NKEXDefCurrency;
					}
				}

				OrgSupplierBuyerLink myLink = SupplierBuyerLink;
				if (myLink != null)
				{
					RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, myLink.OL_RX_NKDefaultCurrency);

					if (currency != null && defaultCurrency)
					{
						JZ_RX_NKInvoice_Currency = currency.RX_Code;
					}

					if (!IsImportingData && defaultIncoTerm)
					{
						if (myLink.OrgSupBuyLinkTrnModes.Count > 0)
						{
							JZ_IncoTerm = IncoTermConverter.GetConvertedIncoTerm(myLink.OrgSupBuyLinkTrnModes[0].PF_IncoTerm, isImport);
						}
					}
				}

				if (!IsImportingData && myLink != null && defaultIncoTerm)
				{
					ZString incoTerm = OrgSupplierBuyerLink.GetDefaultINCO(myLink, cachedSupplier, BuyerForSuppplierLinkCalculation, TransportModeForSuppplierLinkCalculation, ContainerModeForSuppplierLinkCalculation);
					if (!incoTerm.IsEmpty)
					{
						JZ_IncoTerm = incoTerm;
					}
				}
			}
		}

		protected virtual bool IsValidToDefaultCurrencyFromSupplier
		{
			get { return true; }
		}

		protected virtual bool IsValidToDefaultIncoTermFromSupplier
		{
			get { return true; }
		}

		public void UpdateNonPersistentDeclarationWithInvoiceSupplier()
		{
			if (JobDeclaration is BaseJobDeclaration declaration && !declaration.IsPersistent)
			{
				declaration.JE_OH_Supplier = JZ_OH_Supplier;
			}
		}

		IncoTermConverter fIncoTermConverter;
		public IncoTermConverter IncoTermConverter
		{
			get
			{
				if (fIncoTermConverter == null)
				{
					fIncoTermConverter = CreateNewIncoTermConverter();
				}
				return fIncoTermConverter;
			}
		}

		protected virtual IncoTermConverter CreateNewIncoTermConverter()
		{
			return new IncoTermConverter();
		}

		[BusinessObjectTestExclude]
		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				var oldDec = JobDeclaration;
				var oldValue = JZ_JE;

				base.JZ_JE = value;

				if (!IsCopying && oldValue != JZ_JE)
				{
					InvalidateJZ_Calc_LinesEnteredCache();
					preDeclarationPk = oldValue;
					var newDec = JobDeclaration;
					NeedToGetNewIncoTermAndChargeFactory = true;
					var oldPartSyncManagerActiveDeciderPK = oldDec?.PK ?? PK;

					if (JZ_JE.IsEmpty)//detached
					{
						UpdateWhenAnInvoiceIsDetached(oldDec, oldPartSyncManagerActiveDeciderPK);

						if (oldDec != null)
						{
							if (oldDec.IsPersistent)
							{
								oldDec.MarkApportionmentDirty();

								oldDec.InvoiceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);

								foreach (OrdersBusiniess.Order anOrder in AttachedOrders)
								{
									anOrder.JD_JE = ZGuid.Empty;
								}
							}

							//the following action should not trigger another renumbering
							using (oldDec.GetInvoiceNumberRenumberingSuspender())
							{
								JZ_InvoiceDisplaySequence = 1;//on its own
							}
						}
					}
					else
					{
						UpdateWhenAnInvoiceIsLinkedToADeclaration(newDec, oldPartSyncManagerActiveDeciderPK);

						if (newDec != null)
						{
							newDec.MarkAsNeedingValidation();
							newDec.InvoiceNumberGenerator.RecalculateWhenAdded(this);
						}
					}

					if (!IsDataChangeSuspendedByFakeDeclaration)
					{
						if (oldDec != null)
						{
							oldDec.ReApportionInvoiceWeightIfNeeded(!JZ_InvoiceAmount.IsEmpty);
						}
						if (newDec != null)
						{
							newDec.ReApportionInvoiceWeightIfNeeded(!JZ_InvoiceAmount.IsEmpty);
						}
					}
				}
			}
		}

		public ZGuid PreDeclarationPk => preDeclarationPk;
		ZGuid preDeclarationPk;

		public override ZGuid JZ_GB
		{
			get { return base.JZ_GB; }
			set
			{
				var oldValue = JZ_GB;
				base.JZ_GB = value;
				if (!IsCopying && oldValue != JZ_GB)
				{
					NeedToGetNewIncoTermAndChargeFactory = true;
				}
			}
		}

		public ZBool IsASN => JZ_StandAloneInvoiceDirection == JobMessageTypeList.MoreCodes.AdvanceShippingNotice;

		void UpdateWhenAnInvoiceIsLinkedToADeclaration(BaseJobDeclaration declaration, ZGuid oldPartSyncManagerActiveDeciderPK)
		{
			if (declaration != null)
			{
				var invoiceLineFilter = new ZQuery(JobComInvoiceLineSchema.JI_JZ, PK);
				invoiceLineFilter.FetchOnlyFromLocalCache = !IsInDatabase;
				using (declaration.SuspendWeightApportionment())
				{
					var data = declaration.GetNewLinkedToDeclarationData();
					var updatePartSyncManagerAndRefresh = RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired && JobMessageTypeList.MoreCodes.RefreshDefaultWhenAttachedToDeclaration(JZ_StandAloneInvoiceDirection);
					UpdateWhenAnInvoiceIsLinkedToADeclaration(declaration, invoiceLineFilter, data, updatePartSyncManagerAndRefresh, oldPartSyncManagerActiveDeciderPK);
				}

				if (!IsDataChangeSuspendedByFakeDeclaration)
				{
					declaration.ReApportionInvoiceWeightIfNeeded(true);
					if (JZ_JZ_GroupInvoiceFK.IsEmpty)
					{
						var topInvoice = declaration.TopGroupInvoice;
						if (topInvoice != null)
						{
							JZ_JZ_GroupInvoiceFK = topInvoice.PK;
						}
					}

					declaration.MarkApportionmentDirty();
				}

				if (IsAttachedToPersistentDeclaration)
				{
					AddHeaderRefsToDeclaration();
					JZ_StandAloneInvoiceDirection = ZString.Empty;

					foreach (OrdersBusiniess.Order anOrder in AttachedOrders)
					{
						anOrder.JD_JE = JobDeclaration.PK;
					}
				}

				PackagesPivot.ToArray<InvoiceHeaderPackagePivot>().ForEach(x =>
				{
					if (x.CHZ_JE != declaration.PK)
					{
						PackagesPivot.RemoveAndDelete(x);
					}
				});
			}

			if (fJobComInvoiceLines != null)
			{
				fJobComInvoiceLines.CountChanged -= new CollectionCountChangedEventHandler(JobComInvoiceLines_CountChanged); // memory leak and performance issue
			}

			fJobComInvoiceLines = null;
			UpdateMessageTypeNotificationIfNeeded();
			if (!IsDataChangeSuspendedByFakeDeclaration)
			{
				SetSupplierFromDeclaration();
			}
			UpdateMessageTypeNotificationIfNeeded();

			if (RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired)
			{
				RefreshDefaultsWhenInvoiceAttachedToDeclaration();
			}
		}

		void UpdateWhenAnInvoiceIsLinkedToADeclaration(BaseJobDeclaration declaration, ZQuery invoiceLineFilter, LinkedToDeclarationData data, bool updatePartSyncManagerAndRefresh, ZGuid oldPartSyncManagerActiveDeciderPK)
		{
			var sortedInvoiceLines = Factory.Load<BaseJobComInvoiceLine>(invoiceLineFilter).Where(y => !y.IsDeleted).ToList();
			sortedInvoiceLines.Sort(new InvoiceLineNumberComparer());
			Dictionary<ZGuid, List<BaseJobComInvoiceLine>> childInvoiceLinesDictionary = null;
			IDisposable lineNumberRenumberingForDataImportSuspender = null;
			if (updatePartSyncManagerAndRefresh)
			{
				childInvoiceLinesDictionary = ExtraChildInvoiceLines(sortedInvoiceLines);
				lineNumberRenumberingForDataImportSuspender = SuspendLineNumberRenumberingForDataImport();
			}
			else
			{
				lineNumberRenumberingForDataImportSuspender = new DisposableObject();
			}
			using (lineNumberRenumberingForDataImportSuspender)
			{
				ZShort lineNo = 1;
				foreach (var invoiceLine in sortedInvoiceLines)
				{
					if (updatePartSyncManagerAndRefresh)
					{
						invoiceLine.JI_LineNo = lineNo++;
					}
					UpdateWhenAnInvoiceIsLinkedToADeclaration(declaration, data, updatePartSyncManagerAndRefresh, invoiceLine, oldPartSyncManagerActiveDeciderPK);
					if (updatePartSyncManagerAndRefresh)
					{
						List<BaseJobComInvoiceLine> existingChildInvoiceLines = null;
						if (childInvoiceLinesDictionary.TryGetValue(invoiceLine.PK, out existingChildInvoiceLines))
						{
							childInvoiceLinesDictionary.Remove(invoiceLine.PK);
						}
						var childInvoiceLines = GetChildInvoiceLines(invoiceLine);
						if (existingChildInvoiceLines != null)
						{
							foreach (var existingChildInvoiceLine in existingChildInvoiceLines)
							{
								existingChildInvoiceLine.JI_LineNo = lineNo++;
								UpdateWhenAnInvoiceIsLinkedToADeclaration(declaration, data, updatePartSyncManagerAndRefresh, existingChildInvoiceLine, oldPartSyncManagerActiveDeciderPK);
								childInvoiceLines.Remove(existingChildInvoiceLine);
							}
						}
						foreach (var childInvoiceLine in childInvoiceLines)
						{
							childInvoiceLine.JI_LineNo = lineNo++;
							UpdateWhenAnInvoiceIsLinkedToADeclaration(declaration, data, updatePartSyncManagerAndRefresh, childInvoiceLine, oldPartSyncManagerActiveDeciderPK);
						}
					}

					invoiceLine.PackagesPivot.ToArray<InvoiceLinePackagePivot>().ForEach(x =>
					{
						if (x.CHC_JE != declaration.PK)
						{
							invoiceLine.PackagesPivot.RemoveAndDelete(x);
						}
					});
				}
				if (childInvoiceLinesDictionary != null)
				{
					var unprocessedLines = new List<BaseJobComInvoiceLine>();
					foreach (var pair in childInvoiceLinesDictionary)
					{
						unprocessedLines.AddRange(pair.Value);
					}
					foreach (var unprocessedLine in unprocessedLines.OrderBy(x => x.JI_LineNo + "_" + x.PK))
					{
						unprocessedLine.JI_LineNo = lineNo++;
						UpdateWhenAnInvoiceIsLinkedToADeclaration(declaration, data, updatePartSyncManagerAndRefresh, unprocessedLine, oldPartSyncManagerActiveDeciderPK);
					}
				}
			}
		}

		void UpdateWhenAnInvoiceIsLinkedToADeclaration(BaseJobDeclaration declaration, LinkedToDeclarationData data, bool updatePartSyncManagerAndRefresh, BaseJobComInvoiceLine invoiceLine, ZGuid oldPartSyncManagerActiveDeciderPK)
		{
			declaration.InvoiceLines.Add(invoiceLine);
			invoiceLine.RefreshDeclaration();
			invoiceLine.RefreshPartSyncManagerActiveDeciderPK(oldPartSyncManagerActiveDeciderPK);
			UpdateWhenAnInvoiceIsLinkedToADeclaration(invoiceLine, data, updatePartSyncManagerAndRefresh);
		}

		Dictionary<ZGuid, List<BaseJobComInvoiceLine>> ExtraChildInvoiceLines(List<BaseJobComInvoiceLine> sortedInvoiceLines)
		{
			var result = new Dictionary<ZGuid, List<BaseJobComInvoiceLine>>();
			foreach (var sortedInvoiceLine in sortedInvoiceLines.ToArray())
			{
				var parentID = GetInvoiceLineParentID(sortedInvoiceLine);
				if (!parentID.IsEmpty)
				{
					List<BaseJobComInvoiceLine> list = null;
					if (!result.TryGetValue(parentID, out list))
					{
						list = new List<BaseJobComInvoiceLine>();
						result.Add(parentID, list);
					}
					list.Add(sortedInvoiceLine);
					sortedInvoiceLines.Remove(sortedInvoiceLine);
				}
			}

			return result;
		}

		protected virtual ZGuid GetInvoiceLineParentID(BaseJobComInvoiceLine sortedInvoiceLine)
		{
			var parentID = sortedInvoiceLine.JI_ParentID;
			return (!parentID.IsEmpty && sortedInvoiceLine.PK != parentID) ? parentID : ZGuid.Empty;
		}

		protected virtual List<BaseJobComInvoiceLine> GetChildInvoiceLines(BaseJobComInvoiceLine invoiceLine)
		{
			var query = new ZQuery(JobComInvoiceLineSchema.JI_ParentID, invoiceLine.PK);
			query.AddToFilter(JobComInvoiceLineSchema.JI_JZ, invoiceLine.JI_JZ);
			query.AddToFilter(JobComInvoiceLineSchema.PK, SQLComparisonOperator.NotEqual, invoiceLine.PK);
			query.FetchOnlyFromLocalCache = !invoiceLine.IsInDatabase;
			return invoiceLine.Factory.Load<BaseJobComInvoiceLine>(query).ToList();
		}

		protected virtual void UpdateWhenAnInvoiceIsLinkedToADeclaration(BaseJobComInvoiceLine invoiceLine, LinkedToDeclarationData linkedToDeclarationData, bool updatePartSyncManagerAndRefresh)
		{
			invoiceLine.ContainersPivot.RemoveAndDeleteAll();
			invoiceLine.JI_CL = ZGuid.Empty;

			if (updatePartSyncManagerAndRefresh)
			{
				UpdatePartSyncManagerAndRefresh(invoiceLine, true);
			}

			if (IsAttachedToPersistentDeclaration)
			{
				invoiceLine.AddNewOrderItemsToDocsAndCartage(invoiceLine.JI_OrderNumber);
			}
		}

		void AddHeaderRefsToDeclaration()
		{
			if (JobDeclaration is BaseJobDeclaration declaration)
			{
				var billTypeList = declaration.GetBillTypeList();
				var bills = new List<Bill>();
				foreach (var reference in InvoiceHeaderRefs)
				{
					if (reference.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN)
					{
						if (ValidToAddContainerRef)
						{
							var containerNumberToMatch = reference.J2_ReferenceNumber.Left(BaseCusContainer.Schema.CO_ContainerNumberMaxLength);
							if (declaration.CusContainers.Find(containerNumberToMatch) == null)
							{
								var container = declaration.CusContainers.AddNew();
								container.CO_ContainerNumber = containerNumberToMatch;
							}
						}
					}
					else
					{
						bool canAddToDeclarationBills = declaration.Bills.FindByBillNumberAndType(GetBillNumberFromReferenceNumber(reference.J2_ReferenceNumber), reference.J2_ReferenceType) == null
							&& billTypeList != null && billTypeList.ContainsCode(reference.J2_ReferenceType);

						if (canAddToDeclarationBills)
						{
							var bill = declaration.Bills.AddNew();
							bill.CU_BillType = reference.J2_ReferenceType;
							UpdateBillNumberFromReferenceNumber(bill, reference.J2_ReferenceNumber);
							bills.Add(bill);
						}
					}
				}

				AttachBillsToDeclarationFromStandaloneInvoice(declaration, bills);
			}
		}

		ZBool ValidToAddContainerRef => JobDeclaration.IsAir ? JobDeclaration.ContainersRequired : true;

		protected virtual ZString GetBillNumberFromReferenceNumber(ZString referenceNumber)
		{
			return referenceNumber;
		}

		protected virtual void UpdateBillNumberFromReferenceNumber(Bill bill, ZString referenceNumber)
		{
			bill.CU_BillNum = referenceNumber;
		}

		void AttachBillsToDeclarationFromStandaloneInvoice(BaseJobDeclaration declaration, List<Bill> bills)
		{
			List<Bill> masterBills = PossibleParentBills(BillTypeList.Codes.MasterBill, bills);
			List<Bill> houseBills = PossibleParentBills(BillTypeList.Codes.HouseBill, bills);

			foreach (Bill bill in bills)
			{
				if (bill.CU_BillType == BillTypeList.Codes.HouseBill)
				{
					if (masterBills.Count == 1)
					{
						bill.CU_CU_ParentBill = masterBills[0].PK;
					}
				}
				else if (bill.CU_BillType == BillTypeList.Codes.SubHouseBill)
				{
					if (houseBills.Count == 1)
					{
						bill.CU_CU_ParentBill = houseBills[0].PK;
					}
				}
			}
			UpdateBillsOnTransportDetails();
		}

		protected virtual void UpdateBillsOnTransportDetails() { }

		List<Bill> PossibleParentBills(ZString billType, List<Bill> bills)
		{
			return bills.FindAll(x => x.CU_BillType == billType);
		}

		void UpdateWhenAnInvoiceIsDetached(BaseJobDeclaration oldDeclaration, ZGuid oldPartSyncManagerActiveDeciderPK)
		{
			using (GetValidationSuspender())
			{
				JZ_JZ_GroupInvoiceFK = ZGuid.Empty;
				JZ_CU_RelatedHouseBill = ZGuid.Empty;

				if (oldDeclaration == null || oldDeclaration.IsPersistent)
				{
					needToDeserialiseMessageTypeFromStandaloneMessageType = true;
					var isImport = IsImport;
					var isAdvanceShippingNotice = IsAdvanceShippingNotice;
					foreach (BaseJobComInvoiceLine invoiceLine in JobComInvoiceLines.ToArray())
					{
						using (invoiceLine.GetValidationSuspender())
						{
							invoiceLine.RefreshPartSyncManagerActiveDeciderPK(oldPartSyncManagerActiveDeciderPK);
							UpdateWhenAnInvoiceIsDetached(invoiceLine, isImport, isAdvanceShippingNotice);
						}

						if (oldDeclaration != null)
						{
							oldDeclaration.InvoiceLines.Remove(invoiceLine);
						}
					}
				}
				DetachInvoiceRoutings(oldDeclaration);

				PackagesPivot.RemoveAndDeleteAll();
			}
		}

		protected virtual void UpdateWhenAnInvoiceIsDetached(BaseJobComInvoiceLine invoiceLine, bool isImport, bool isAdvanceShippingNotice)
		{
			invoiceLine.JI_CL = ZGuid.Empty;
			invoiceLine.JI_CO = ZGuid.Empty;
			invoiceLine.JI_JO = ZGuid.Empty;
			invoiceLine.JI_CEI = ZGuid.Empty;
			invoiceLine.ContainersPivot.RemoveAndDeleteAll();
			invoiceLine.PackagesPivot.RemoveAndDeleteAll();
			invoiceLine.AdditionalEntryLineLinks.DeleteAll();

			if (isAdvanceShippingNotice)
			{
				UpdatePartSyncManagerAndRefresh(invoiceLine, false);
			}
		}

		protected void DetachInvoiceRoutings(BaseJobDeclaration oldDeclaration)
		{
			if (oldDeclaration != null && oldDeclaration.HasChanges)
			{
				foreach (Transport transport in Transports)
				{
					Transport declarationTransport = oldDeclaration.Transports.FindTransportByLoadPort(transport.JW_RL_NKLoadPort);
					if (declarationTransport != null && declarationTransport.JW_RL_NKDiscPort == transport.JW_RL_NKDiscPort)
					{
						declarationTransport.Delete();
					}
				}
			}
		}

		internal void UpdateMessageTypeNotificationIfNeeded()
		{
			if (PersistentDeclaration is BaseJobDeclaration declaration) // don't check for validation suspended as JZ_JE is set from BaseInvoiceHeaderCollection.SetCollectionRelationships which suspend validation
			{
				fJZMessageType = declaration.JE_MessageType;
				JZ_MessageTypeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					InvoiceHeaderValidation?.ValidateJZ_MessageType(); // To remove any notification attached to JZ_MessageType
				}
			}
		}

		[BusinessObjectTestExclude]
		[LightValidationTestExempt]
		public override ZString JZ_AddInfo
		{
			get { return base.JZ_AddInfo; }
			set { base.JZ_AddInfo = value; }
		}

		public override ZShort JZ_InvoiceDisplaySequence
		{
			get { return base.JZ_InvoiceDisplaySequence; }
			set
			{
				if (value > 0)
				{
					ZShort oldValue = JZ_InvoiceDisplaySequence;
					var hasChanged = oldValue != value;
					base.JZ_InvoiceDisplaySequence = value;

					if (!IsCopying && JobDeclaration is BaseJobDeclaration declaration)
					{
						declaration.InvoiceNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
						if (hasChanged)
						{
							declaration.RefreshSortedInvoiceList();
						}
					}
				}
			}
		}

		public override ZDecimal JZ_NoOfPacks
		{
			get { return base.JZ_NoOfPacks; }
			set
			{
				base.JZ_NoOfPacks = value;
				JobDeclaration?.MarkAsNeedingValidation();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.RelatedIndicatorList))]
		public override ZString JZ_RelatedIndicator
		{
			get { return base.JZ_RelatedIndicator; }
			set { base.JZ_RelatedIndicator = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValuationCodeList))]
		public override ZString JZ_ValuationCode
		{
			get { return base.JZ_ValuationCode; }
			set { base.JZ_ValuationCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.PaymentMethodList))]
		public override ZString JZ_PaymentMethod
		{
			get { return base.JZ_PaymentMethod; }
			set { base.JZ_PaymentMethod = value; }
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			SetDefaultInvoiceDate();
			JZ_WeightUQ = "KG";
			JZ_NetWeightUQ = "KG";
			JZ_GroupInvoice = false;
			JZ_GB = GlbBranch.CurrentBranch.PK;
		}

		protected virtual void SetDefaultInvoiceDate()
		{
			JZ_InvoiceDate = ZDateTime.Today;
		}

		void RefreshDefaultsWhenInvoiceAttachedToDeclaration()
		{
			if (JobMessageTypeList.MoreCodes.RefreshDefaultWhenAttachedToDeclaration(JZ_StandAloneInvoiceDirection))
			{
				RefreshDefaultsWhenInvoiceAttachedToDeclarationCore();
			}
			RefreshDeclarationRoutingsWhenInvoiceAttachedToDecl();
			RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = false;
		}

		protected virtual void RefreshDefaultsWhenInvoiceAttachedToDeclarationCore()
		{
		}

		protected virtual void UpdatePartSyncManagerAndRefresh(BaseJobComInvoiceLine invoiceLine, bool enabledStatus)
		{
			invoiceLine.UpdatePartSyncManagerAndRefresh(enabledStatus);
		}

		void UpdatePartSyncManagerAndRefreshOnAllInvoiceLines(bool enabledStatus)
		{
			foreach (BaseJobComInvoiceLine invoiceLine in JobComInvoiceLines.ToArray())
			{
				if (!invoiceLine.IsDeleted)
				{
					UpdatePartSyncManagerAndRefresh(invoiceLine, enabledStatus);
				}
			}
		}

		public bool RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired
		{
			get { return refreshDefaultsWhenInvoiceAttachedToDeclarationRequired; }
			set { refreshDefaultsWhenInvoiceAttachedToDeclarationRequired = value; }
		}
		bool refreshDefaultsWhenInvoiceAttachedToDeclarationRequired;

		void RefreshDeclarationRoutingsWhenInvoiceAttachedToDecl()
		{
			var declaration = JobDeclaration;
			if (declaration != null && declaration.Transports.Count == 0)
			{
				foreach (Transport transport in Transports)
				{
					var decTransport = (Transport)transport.Clone();
					declaration.Transports.Add(decTransport);
				}

				if (declaration.Transports.Count == 1)
				{
					var declTransportSupporter = (IJobDeclarationTransportSupporter)((ITransportParent)declaration).TransportSupporter;
					declTransportSupporter.UpdateAllDeclarationTransportDataIfEmpty(declaration.Transports[0]);
				}
			}
		}

		public override void Delete()
		{
			BaseJobDeclaration jobDeclaration = null;
			ZDecimal invoiceAmount = ZDecimal.Zero;
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				invoiceAmount = JZ_InvoiceAmount;
				jobDeclaration = JobDeclaration;
				if (jobDeclaration != null)
				{
					using (GetLineNumberRenumberingSuspender())
					using (jobDeclaration.SuspendWeightApportionment())
					{
						JobComInvoiceLines.RemoveAndDeleteAll();
					}
				}
				else
				{
					JobComInvoiceLines.RemoveAndDeleteAll();
				}
				Charges.RemoveAndDeleteAll();
				GroupCharges.RemoveAndDeleteAll();
				foreach (var invoiceHeaderRef in InvoiceHeaderRefs)
				{
					invoiceHeaderRef.FetchStrategy.FetchForDelete();
				}
				InvoiceHeaderRefs.DeleteAll();

				if (jobDeclaration != null)
				{
					jobDeclaration.InvoiceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}

				if (SupportsChzPivotBetweenInvoiceHeaderAndPacking)
				{
					PackagesPivot.RemoveAndDeleteAll();
				}

				if (workflowItems != null)
				{
					workflowItems.RemoveAndDeleteAll();
				}

				AttachedOrders.RemoveAllFromRelationship();
				DeleteDocsAndCartage();

				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				Factory.Load<CusPackingList>(new ZQuery(CusPackingListSchema.CUL_JZ, PK)).DeleteAll();
			}
			base.Delete();
			//this should happen after Delete happens
			if (jobDeclaration != null)
			{
				jobDeclaration.MarkApportionmentDirty();
				jobDeclaration.ReApportionInvoiceWeightIfNeeded(!invoiceAmount.IsEmpty);
				jobDeclaration.RefreshSortedInvoiceList();
			}
			InvoiceDeleteEvent.OnInvoiceDeleted(Factory);
		}

		public bool IsSingleLine
		{
			get { return isSingleLine; }
			set { isSingleLine = value; }
		}
		bool isSingleLine;

		/// <summary>
		/// Sets the invoice price equal to the sum of the line prices
		/// </summary>
		public void BalancePrice()
		{
			JZ_InvoiceAmount = JZ_Calc_LinesEntered;
		}

		public BaseJobComInvoiceGroupHeader GetGroupHeaderWithThisCharge(ChargeCodeChargeKey chargeKey)
		{
			return GetGroupHeaderWithThisCharge(chargeKey, false);
		}

		public BaseJobComInvoiceGroupHeader GetGroupHeaderWithThisCharge(ChargeCodeChargeKey chargeKey, bool shouldTakePercentageValue)
		{
			if (GroupHeader != null)
			{
				return GroupHeader.GetGroupHeaderWithThisCharge(chargeKey, shouldTakePercentageValue);
			}

			return null;
		}

		public RefCountry InvoiceCountry
		{
			get
			{
				return Branch?.Company?.Country;
			}
		}

		public ZString CountryCode
		{
			get
			{
				var country = InvoiceCountry;
				return country != null ? country.RN_Code : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
		}

		#region Data Grouping
		public ZString GetDefaultDataGroupingCode(DefaultDataGroupingType dataGroupingType = DefaultDataGroupingType.None)
		{
			switch (dataGroupingType)
			{
				case DefaultDataGroupingType.Tariff:
					return DefaultDataGroupingForTariffsCore;
				case DefaultDataGroupingType.DutyRateCodes:
					return DefaultDataGroupingForDutyRateCodesCore;
				case DefaultDataGroupingType.CusProcedure:
					return DefaultDataGroupingForCusProcedureCore;
				case DefaultDataGroupingType.AdditionalDocumentCodes:
					return DefaultDataGroupingForAdditionalDocumentCodesCore;
				default:
					return DefaultDataGroupingCore;
			}
		}

		protected virtual ZString DefaultDataGroupingCore => PersistentDeclaration is BaseJobDeclaration declaration ? declaration.GetDefaultDataGroupingCode() : CountryCode;
		protected virtual ZString DefaultDataGroupingForTariffsCore => PersistentDeclaration is BaseJobDeclaration declaration ? declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) : CountryCode;
		protected virtual ZString DefaultDataGroupingForDutyRateCodesCore => PersistentDeclaration is BaseJobDeclaration declaration ? declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes) : CountryCode;
		protected virtual ZString DefaultDataGroupingForCusProcedureCore => PersistentDeclaration is BaseJobDeclaration declaration ? declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure) : CountryCode;
		protected virtual ZString DefaultDataGroupingForAdditionalDocumentCodesCore => PersistentDeclaration is BaseJobDeclaration declaration ? declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes) : CountryCode;
		#endregion

		public ZDecimal LandedCostingExRateFallBackToJobExRate
		{
			get
			{
				ZDecimal result = 1m;
				if (JZ_RX_NKInvoice_Currency != LocalCurrencyCode)
				{
					result = JZ_InvoiceCurrLandedCostExRate;
					if (result == 0m && Env.Registry.LandedCostingFallbackExRatesToJobInvoicing)
					{
						result = GetExRateFromJobInvoicing();
					}
				}
				return result;
			}
		}

		public ZDecimal GetExRateFromJobInvoicing()
		{
			ZDecimal result = 0m;
			if (Invoice_Currency != null)
			{
				ILandedCostExchangeRateProvider exRateProvider = JobDeclaration.JobInvoicing as ILandedCostExchangeRateProvider;
				if (exRateProvider != null)
				{
					result = exRateProvider.GetExRateFor(Invoice_Currency);
				}
			}
			return result;
		}

		public void ApportionLineWeight(BaseJobComInvoiceLine uncommittedLine)
		{
			if (!IsImportingData)
			{
				InvoiceDataProvider?.ApportionWeightIfNeeded(this, uncommittedLine);
			}
		}

		public virtual ZString AdditionalInformation => ZString.Empty;

		#region ICodeDescription Members
		public string Description
		{
			get { return JZ_InvoiceNumber; }
		}

		public string Code
		{
			get { return JZ_InvoiceNumber; }
		}

		object ICodeDescription.PK
		{
			get { return PK; }
		}
		#endregion

		#region Collections And Related Events

		BaseJobComInvoiceLineViewCollection fJobComInvoiceLines;

		protected virtual BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			BaseJobComInvoiceLineViewCollection result = null;
			if (JobDeclaration is BaseJobDeclaration declaration)
			{
				result = new BaseJobComInvoiceLineViewCollection(this, declaration.InvoiceLines);
			}

			return result;
		}

		protected virtual BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			InvoiceLineDependentCollection collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new BaseJobComInvoiceLineViewCollection(this, collection);
		}

		protected virtual void JobComInvoiceLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsValidationSuspended && !IsDeleted && e.ItemRemoved)
			{
				InvoiceHeaderValidation?.ValidateJZ_Calc_Balance();
			}
		}

		#endregion

		#region Property Changed Handlers

		protected void SetChargeCurrency()
		{
			if (!IsCopying)
			{
				ZString currencyCode = Invoice_Currency != null ? Invoice_Currency.RX_Code : ZString.Empty;
				Charges.SetCurrency(currencyCode);
			}
		}

		public void SetExchangeRateIfNotUserEntered()
		{
			if (!IsJZ_InvoiceCurrExRateUserEnterable)
			{
				SetExchangeRateCore(JZ_InvoiceCurrExRateInfo, CurrencyConverter);
			}

			SetExchangeRateCore(JZ_InvoiceCurrLandedCostExRateInfo, CurrencyConverter);
			SetExchangeRateCore(JZ_PaymentExRateInfo, PaymentCurrencyConverter);
		}

		void SetExchangeRateCore(ZPropertyInfo rateFieldInfo, CurrencyConverter currencyConverter)
		{
			var exchangeRate = ZDecimal.Zero;

			if (Invoice_Currency != null)
			{
				if (Invoice_Currency.RX_Code == LocalCurrencyCode)
				{
					exchangeRate = 1m;
				}
				else if (currencyConverter != null)
				{
					exchangeRate = GetCurrencyConverterExchangeRate(currencyConverter, Invoice_Currency);
				}
			}
			else
			{
				exchangeRate = 0m;
			}

			rateFieldInfo.Value = exchangeRate;
		}

		protected virtual ZDecimal GetCurrencyConverterExchangeRate(CurrencyConverter currencyConverter, RefCurrency invoiceCurrency) => currencyConverter.GetExchangeRate(invoiceCurrency);

		#endregion

		#region Apportion Charges

		void GroupCharges_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved)
			{
				RefreshBinding();
			}
		}

		internal bool HasMultipleInvoiceUQs
		{
			get
			{
				if (!hasMultipleInvoiceUQsCached.HasValue)
				{
					hasMultipleInvoiceUQsCached = InvoiceLines.Select(line => line.JI_InvoiceUQ).Distinct().Count() > 1;
				}
				return hasMultipleInvoiceUQsCached.Value;
			}
		}
		bool? hasMultipleInvoiceUQsCached;

		internal void ResetHasMultipleInvoiceUQs()
		{
			hasMultipleInvoiceUQsCached = null;
		}

		#endregion

		#region Validation

		protected override JobComInvoiceHeaderValidation GetNewValidation()
		{
			return new InvoiceHeaderValidation(this);
		}

		public InvoiceHeaderValidation InvoiceHeaderValidation => Validation as InvoiceHeaderValidation;

		public void AddFetchForLoadingInvoiceLines()
		{
			AddFetchForLoadingInvoiceLinesCore();
		}

		protected virtual void AddFetchForLoadingInvoiceLinesCore()
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			var declaration = JobDeclaration;
			if (declaration != null && !declaration.IsPersistent && declaration.ApportionmentDirty)
			{
				declaration.ResumeApportionment();
			}
		}
		#endregion

		#region CurrencyConversion

		public Money ConvertToLocalAmountRounded(ZDecimal amount, ICurrency currency)
		{
			Money result = Money.Invalid;

			if (currency != null)
			{
				Money moneyAmount = new Money(amount, currency);
				result = ConvertToLocalAmountRounded(moneyAmount);
			}

			return result;
		}

		protected Money GetEffectiveMoney(Money money)
		{
			return IsJZ_InvoiceCurrExRateUserEnterable ? ConvertToLocalAmountRounded(money) : money;
		}

		protected Money ConvertToLocalAmountRounded(Money amount)
		{
			return CurrencyConverter.ConvertRounded(amount, LocalCurrency);
		}

		#endregion

		protected virtual ZDecimal PerformLocalRounding(ZDecimal d)
		{
			return ZArchitecture.Core.Utilities.Round(d, 0);
		}

		#region IExternalFactoryRefreshable Members

		bool fExternalFactoryRefreshEnabled;
		public bool ExternalFactoryRefreshEnabled
		{
			get { return fExternalFactoryRefreshEnabled; }
			set
			{
				if (fExternalFactoryRefreshEnabled != value)
				{
					fExternalFactoryRefreshEnabled = value;
					OnExternalFactoryRefreshEnabledChanged();
				}
			}
		}

		protected void OnExternalFactoryRefreshEnabledChanged()
		{
			if (fJobComInvoiceLines != null)
			{
				JobComInvoiceLines.ExternalFactoryRefreshEnabled = ExternalFactoryRefreshEnabled;
			}
		}

		#endregion

		#region ISupportDataImporting Members

		bool fIsImportingData;
		public bool IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion

		#region Calculated Fields for Doc Wrapper

		public Money IncludedTotalInInvoiceCurr
		{
			get
			{
				Money result = Money.Empty;
				result = CurrencyConverter.Add(result, IncludedOverseasFreight);
				result = CurrencyConverter.Add(result, IncludedOverseasInsurance);
				result = CurrencyConverter.Add(result, IncludedExWorks);
				result = CurrencyConverter.Add(result, IncludedForeignInlandFreight);
				result = CurrencyConverter.Add(result, IncludedPackingCosts);
				result = CurrencyConverter.Add(result, IncludedLandingCharges);
				result = CurrencyConverter.Add(result, IncludedOtherCharges1);
				result = CurrencyConverter.Add(result, IncludedOtherCharges2);
				result = CurrencyConverter.Add(result, IncludedCommission);
				result = CurrencyConverter.Subtract(result, IncludedDiscount);
				result = CurrencyConverter.ConvertExact(result, Invoice_Currency);
				return result;
			}
		}

		public Money ExcludedTotalInInvoiceCurr
		{
			get
			{
				Money result = Money.Empty;
				result = CurrencyConverter.Add(result, ExcludedOverseasFreight);
				result = CurrencyConverter.Add(result, ExcludedOverseasInsurance);
				result = CurrencyConverter.Add(result, ExcludedExWorks);
				result = CurrencyConverter.Add(result, ExcludedForeignInlandFreight);
				result = CurrencyConverter.Add(result, ExcludedPackingCosts);
				result = CurrencyConverter.Add(result, ExcludedLandingCharges);
				result = CurrencyConverter.Add(result, ExcludedOtherCharges1);
				result = CurrencyConverter.Add(result, ExcludedOtherCharges2);
				result = CurrencyConverter.Add(result, ExcludedCommission);
				result = CurrencyConverter.Subtract(result, ExcludedDiscount);
				result = CurrencyConverter.ConvertExact(result, Invoice_Currency);
				return result;
			}
		}

		public Money OverseasFreight
		{
			get { return CurrencyConverter.Add(IncludedOverseasFreight, ExcludedOverseasFreight); }
		}

		public Money IncludedOverseasFreight
		{
			get { return GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.OverseasFreight); }
		}

		public Money ExcludedOverseasFreight
		{
			get { return GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.OverseasFreight); }
		}

		public Money OverseasFreightInLocalCurrency
		{
			get { return ConvertToLocalAmountRounded(OverseasFreight.Amount, OverseasFreight.Currency); }
		}

		public Money OverseasInsurance
		{
			get { return CurrencyConverter.Add(IncludedOverseasInsurance, ExcludedOverseasInsurance); }
		}

		public Money IncludedOverseasInsurance
		{
			get { return GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.OverseasInsurance); }
		}

		public Money ExcludedOverseasInsurance
		{
			get { return GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.OverseasInsurance); }
		}

		public Money OverseasInsuranceInLocalCurrency
		{
			get { return ConvertToLocalAmountRounded(OverseasInsurance.Amount, OverseasInsurance.Currency); }
		}

		public Money IncludedExWorks
		{
			get { return GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.ExWorks); }
		}

		public Money ExcludedExWorks
		{
			get { return GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.ExWorks); }
		}

		public Money IncludedForeignInlandFreight
		{
			get { return GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.ForeignInlandFreight); }
		}

		public Money ExcludedForeignInlandFreight
		{
			get { return GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.ForeignInlandFreight); }
		}

		public Money IncludedPackingCosts
		{
			get { return GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.PackingCost); }
		}

		public Money ExcludedPackingCosts
		{
			get { return GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.PackingCost); }
		}

		public Money IncludedLandingCharges
		{
			get { return GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.LandingCharges); }
		}

		public Money ExcludedLandingCharges
		{
			get { return GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.LandingCharges); }
		}

		public Money IncludedOtherCharges1
		{
			get
			{
				Money result = GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.OtherCharges);
				result = CurrencyConverter.Add(result, GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.AdditionCharge));
				return result;
			}
		}

		public Money ExcludedOtherCharges1
		{
			get
			{
				Money result = GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.OtherCharges);
				result = CurrencyConverter.Add(result, GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.AdditionCharge));
				return result;
			}
		}

		public Money IncludedOtherCharges2
		{
			get
			{
				Money result = GetIncludedAmountWithThisChargeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, false));
				result = CurrencyConverter.Add(result, GetIncludedAmountWithThisChargeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, true)));
				result = CurrencyConverter.Add(result, GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.DeductionCharge));
				return result;
			}
		}

		public Money ExcludedOtherCharges2
		{
			get
			{
				Money result = GetExcludedAmountWithThisChargeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, false));
				result = CurrencyConverter.Add(result, GetExcludedAmountWithThisChargeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, true)));
				result = CurrencyConverter.Add(result, GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.DeductionCharge));
				return result;
			}
		}

		public Money IncludedDiscount
		{
			get { return GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.Discount); }
		}

		public Money ExcludedDiscount
		{
			get { return GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.Discount); }
		}

		public Money IncludedCommission
		{
			get { return GetIncludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.Commission); }
		}

		public Money ExcludedCommission
		{
			get { return GetExcludedAmountWithThisChargeType(CustomsChargeTypeList.Codes.Commission); }
		}

		public Money DutiableChargesNotIncludedInLines
		{
			get
			{
				Money result = CurrencyConverter.Add(Charges.GetCharge(true, false), GroupCharges.GetCharge(true, false));
				result = CurrencyConverter.Subtract(result, Charges.GetCharge(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Discount, false, false)));
				return CurrencyConverter.Subtract(result, GroupCharges.GetCharge(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Discount, false, false)));
			}
		}

		public Money DutiableChargesNotIncludedInLinesInLocalCurrency
		{
			get { return ConvertToLocalAmountRounded(DutiableChargesNotIncludedInLines); }
		}

		public Money NonDutiableChargesNotIncludedInLines
		{
			get
			{
				Money result = CurrencyConverter.Add(Charges.GetCharge(false, false), GroupCharges.GetCharge(false, false));
				result = CurrencyConverter.Subtract(result, Charges.GetCharge(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Discount, false, false)));
				return CurrencyConverter.Subtract(result, GroupCharges.GetCharge(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Discount, false, false)));
			}
		}

		public Money NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance
		{
			get
			{
				Money result = CurrencyConverter.Add(Charges.GetCharge(false, false), GroupCharges.GetCharge(false, false));
				result = CurrencyConverter.Subtract(result, Charges.GetCharge(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Discount, false, false)));
				result = CurrencyConverter.Subtract(result, OverseasFreight);
				result = CurrencyConverter.Subtract(result, OverseasInsurance);
				return CurrencyConverter.Subtract(result, GroupCharges.GetCharge(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Discount, false, false)));
			}
		}

		public Money NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrency
		{
			get { return ConvertToLocalAmountRounded(NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance); }
		}

		public Money NonDutiableChargesNotIncludedInLinesInLocalCurrency
		{
			get { return ConvertToLocalAmountRounded(NonDutiableChargesNotIncludedInLines); }
		}

		protected Money GetIncludedAmountWithThisChargeType(string chargeCode)
		{
			return GetIncludedAmountWithThisChargeKey(GetChargeKey(chargeCode));
		}

		protected Money GetIncludedAmountWithThisChargeKey(ChargeCodeChargeKey chargeKey)
		{
			return GetAmountWithThisChargeKey(chargeKey, x => x.J7_Calc_IsIncludedInInvoiceAmount);
		}

		protected Money GetExcludedAmountWithThisChargeType(string chargeCode)
		{
			return GetExcludedAmountWithThisChargeKey(GetChargeKey(chargeCode));
		}

		protected Money GetExcludedAmountWithThisChargeKey(ChargeCodeChargeKey chargeKey)
		{
			return GetAmountWithThisChargeKey(chargeKey, x => !x.J7_Calc_IsIncludedInInvoiceAmount);
		}

		Money GetAmountWithThisChargeKey(ChargeCodeChargeKey chargeKey, Func<JobComInvCharge, bool> predicate)
		{
			var result = Money.Empty;
			if (chargeKey != null)
			{
				var allCharges = Charges.Cast<JobComInvCharge>();
				allCharges = allCharges.Append(GroupCharges.Cast<JobComInvCharge>().ToArray());

				foreach (JobComInvCharge charge in allCharges.Where(predicate))
				{
					if (charge.WithKey(chargeKey))
					{
						result = CurrencyConverter.Add(result, charge.Money);
					}
				}
			}
			return result;
		}

		ChargeCodeChargeKey GetChargeKey(string chargeCode)
		{
			var charge = IncoTermAndChargeFactory.GetCharge(chargeCode);
			return charge != null ? charge.ChargeCodeChargeKey : null;
		}

		public int GetInvoiceLineMaxDecimalPlaces(string fieldName)
		{
			return DecimalPlacesCalculator.GetInvoiceLineMaxDecimalPlaces(fieldName);
		}

		InvoiceLineDecimalPlacesCalculator DecimalPlacesCalculator
		{
			get { return decimalPlacesCalculator ?? (decimalPlacesCalculator = new InvoiceLineDecimalPlacesCalculator(this)); }
		}
		InvoiceLineDecimalPlacesCalculator decimalPlacesCalculator;

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = NewDocManager();
				}
				return docManagerInfo;
			}
		}

		protected virtual DocManagerInfo NewDocManager()
		{
			return new JobComInvoiceHeaderDocManagerInfo(this);
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region ICommonInvoice Members

		public bool IsGroup
		{
			get { return false; }
		}

		IAllInvoiceLines ICommonInvoice.InvoiceLines
		{
			get { return JobComInvoiceLines; }
		}

		Common.ICommonInvoice Common.ICommonInvoice.ImmediateCommonInvoiceParent
		{
			get { return GroupHeader; }
		}

		ZString ICommonInvoice.UserFriendlyCode
		{
			get { return JZ_InvoiceNumber; }
		}

		RefCurrencyCurrencyConverter Common.ICommonInvoice.CurrencyConverter
		{
			get { return (RefCurrencyCurrencyConverter)CurrencyConverter; }
		}

		IApportionInvoiceHolder Common.ICommonInvoice.InvoicesHolder
		{
			get { return JobDeclaration; }
		}

		ZString Common.ICommonInvoice.LocalCurrencyCode
		{
			get
			{
				return LocalCurrencyCode;
			}
		}

		bool Common.ICommonInvoice.HasMultipleInvoiceUQs
		{
			get { return HasMultipleInvoiceUQs; }
		}

		CodeDescriptionPairList ICommonInvoice.ChargeTypeList
		{
			get { return GetCustomsChargeTypeList(ChargeParentTypes.Invoice); }
		}

		CodeDescriptionPairList ICommonInvoice.AllChargeTypeList
		{
			get { return GetCustomsChargeTypeList(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine); }
		}

		#endregion

		#region ApportionmentEnabled

		protected virtual event EventHandler ApportionmentDirtyChangedEventHandler
		{
			add
			{
				JZ_InvoiceAmountInfo.ValueChanged += value;
				JZ_RX_NKInvoice_CurrencyInfo.ValueChanged += value;
				JZ_IncoTermInfo.ValueChanged += value;
				JZ_ValuationDateOverrideInfo.ValueChanged += value;
			}
			remove
			{
				throw new NotSupportedException();
			}
		}

		protected void MarkApportionmentDirty(object sender, EventArgs e)
		{
			if (!IsCopying && JobDeclaration is BaseJobDeclaration declaration)
			{
				declaration.MarkApportionmentDirty();
			}
		}

		#endregion

		#region ITemplateCopyable Members

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			if (JobDeclaration == null)
			{
				new FakeDeclarationCreatorForInvoice(this);
			}

			BaseJobComInvoiceHeader invoice = (BaseJobComInvoiceHeader)GetTemplateCopyStrategy(CloneType.TemplateCopy).Clone();

			invoice.JZ_InvoiceNumber = "*TBA*";
			return invoice;
		}

		protected virtual JobComInvoiceHeaderDeepCopyStrategy GetTemplateCopyStrategy(CloneType cloneType)
		{
			return new JobComInvoiceHeaderDeepCopyStrategy(this, cloneType);
		}

		#endregion

		#region ILandedCostDistributeTo Members

		ZString ILandedCostDistributeTo.TableCode
		{
			get { return JobComInvoiceHeaderSchema.Constants.Prefix; }
		}

		ZString ILandedCostDistributeTo.UniqueCode
		{
			get
			{
				ZString supplierCode = Supplier == null ? ZString.Empty : Supplier.OH_Code;
				return InvoiceConstString + JZ_InvoiceNumber + ":" + supplierCode;
			}
		}

		public const string InvoiceConstString = "INVOICE ";

		ZString ILandedCostDistributeTo.Description
		{
			get { return InvoiceConstString + JZ_InvoiceNumber + ":" + SupplierName; }
		}

		IEnumerable<IUltimateDistributee> ILandedCostDistributeTo.UltimateDistributees
		{
			get { return new TypedEnumerable<IUltimateDistributee>(JobComInvoiceLines); }
		}

		#endregion

		#region ILandedCostChargeHolder Members

		IEnumerable<IDefaultLandedCostInput> ILandedCostChargeHolder.ChargesToImportForLandedCosting
		{
			get
			{
				if (JobDeclaration is BaseJobDeclaration declaration && !declaration.ApportionmentDirty)
				{
					foreach (BaseInvoiceCharge invoiceCharge in Charges)
					{
						if (((IDefaultLandedCostInput)invoiceCharge).IsValidToImport)
						{
							yield return invoiceCharge;
						}
					}
				}
			}
		}

		#endregion

		#region ILandedCostExchangeRateHolder Members

		ZDecimal ILandedCostExchangeRateHolder.LandedCostExchangeRate
		{
			get { return JZ_InvoiceCurrLandedCostExRate; }
			set { JZ_InvoiceCurrLandedCostExRate = value; }
		}

		ZDecimal ILandedCostExchangeRateHolder.LandedCostExchangeRateDefault
		{
			get { return JZ_InvoiceCurrExRate; }
		}

		ZString ILandedCostExchangeRateHolder.ReferenceNumber
		{
			get { return ((ILandedCostDistributeTo)this).Description; }
		}

		ZString ILandedCostExchangeRateHolder.CurrencyCode
		{
			get { return Invoice_Currency == null ? ZString.Empty : Invoice_Currency.RX_Code; }
		}

		ZGuid ILandedCostExchangeRateHolder.CompanyPK
		{
			get { return Branch?.GB_GC ?? GlbCompany.CurrentCompany.PK; }
		}

		#endregion

		#region ICurrencyConverterDataProvider Members

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get { return EffectiveValuationDate; }
		}

		ExchangeRateType ICurrencyConverterDataProvider.RateType => RateTypeCore;

		protected virtual ExchangeRateType RateTypeCore => ZArchitecture.Core.ExchangeRateType.Customs;

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack; }
		}

		GlbCompany ICurrencyConverterDataProvider.Company
		{
			get { return Branch?.Company ?? GlbCompany.CurrentCompany; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return LocalCurrencyCode; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return IsReciprocalRates; }
		}

		#endregion

		#region ICurrencyConverterDataProviderWithFixedExRates Members

		string ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRateCurrencyCode
		{
			get { return IsJZ_InvoiceCurrExRateUserEnterable && Invoice_Currency != null ? Invoice_Currency.RX_Code : ZString.Empty; }
		}

		decimal ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRate
		{
			get { return JZ_InvoiceCurrExRate; }
		}

		#endregion

		#region IGroupInvoiceOrInvoice Members

		void IGroupInvoiceOrInvoice.Move(BaseJobComInvoiceGroupHeader origin, BaseJobComInvoiceGroupHeader destination)
		{
			JZ_JZ_GroupInvoiceFK = destination.PK;
		}

		BaseJobComInvoiceGroupHeader IGroupInvoiceOrInvoice.ParentGroupInvoice
		{
			get { return GroupHeader; }
		}

		BaseJobComInvoiceGroupHeader IGroupInvoiceOrInvoice.GroupInvoiceOfInvoiceOrGroupInvoiceItself
		{
			get { return GroupHeader; }
		}

		bool IGroupInvoiceOrInvoice.IsGroupInvoice
		{
			get { return false; }
		}

		IGroupInvoiceOrInvoice[] IGroupInvoiceOrInvoice.ChildGroupInvoices
		{
			get { return Array.Empty<IGroupInvoiceOrInvoice>(); }
		}

		IGroupInvoiceOrInvoice[] IGroupInvoiceOrInvoice.ChildInvoices
		{
			get { return Array.Empty<IGroupInvoiceOrInvoice>(); }
		}

		#endregion

		#region IChargeHolder

		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeType, JobComInvCharge charge)
		{
			return JZ_RX_NKInvoice_Currency;
		}

		ZString IChargeHolder.GetDefaultDistributeBy()
		{
			var result = ZString.Empty;
			var branch = Branch ?? JobDeclaration.Branch;

			var branchPk = Guid.Empty;
			var companyPk = Guid.Empty;

			if (branch != null && branch.PK is { IsEmpty: false, IsValid: true })
			{
				branchPk = branch.PK.ToGuid();

				if (branch.GB_GC is { IsEmpty: false, IsValid: true })
				{
					companyPk = branch.GB_GC.ToGuid();
				}
			}

			if (IsExport)
			{
				result = DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.GetFallBackValueAtAllLevels(companyPk, branchPk, Guid.Empty);
			}
			else if (IsImport)
			{
				result = DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForImport.GetFallBackValueAtAllLevels(companyPk, branchPk, Guid.Empty);
			}
			return result;
		}

		CurrencyConverter IChargeHolder.CurrencyConverter
		{
			get { return CurrencyConverter; }
		}

		IJobComInvChargeCollection<JobComInvCharge> IChargeHolder.Charges
		{
			get { return Charges; }
		}

		IChargeApportionee[] IChargeHolder.AllApportionees
		{
			get { return (IChargeApportionee[])new ArrayList(JobComInvoiceLines).ToArray(typeof(IChargeApportionee)); }
		}

		IChargeHolder[] IChargeHolder.ImmediateChargeHolderChildren
		{
			get { return (IChargeApportionee[])new ArrayList(JobComInvoiceLines).ToArray(typeof(IChargeApportionee)); }
		}

		IChargeHolder IChargeHolder.ImmediateChargeHolderParent
		{
			get { return GroupHeader; }
		}

		bool IChargeHolder.IsGroupInvoice
		{
			get { return false; }
		}

		#endregion

		#region IChargeApportionee

		bool IChargeApportionee.IsValidToApportionTo
		{
			get { return true; }
		}

		IJobComInvApportionedChargeCollection<JobComInvCharge> IChargeApportionee.ApportionedCharges
		{
			get { return GroupCharges; }
		}

		ZDecimal IChargeApportionee.GetBaseValueToApportionOn(CurrencyConverter currencyConverter, string distributeBy)
		{
			switch (distributeBy)
			{
				case ChargeDistributeByList.Codes.Value:
					return currencyConverter.ConvertExact(InvoiceAmount, LocalCurrency).Amount;
				case ChargeDistributeByList.Codes.Weight:
					return Core.Constants.Weight.ContainsCode(JZ_WeightUQ.ToUpper()) ? Core.Constants.Weight.Convert(JZ_Weight, JZ_WeightUQ.ToUpper(), Core.Constants.Weight.Kilograms) : 0m;
				case ChargeDistributeByList.Codes.Volume:
					return Core.Constants.Volume.ContainsCode(JZ_VolumeUQ.ToUpper()) ? Core.Constants.Volume.Convert(JZ_Volume, JZ_VolumeUQ.ToUpper(), Core.Constants.Volume.CubicMetres) : 0m;
				default:
					return 0m;
			}
		}

		bool IChargeApportionee.CanThisChargeBeApportionedBasedOnIncoterm(ApportionChargeKey apportionedChargeKey)
		{
			return IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(IncoTerm, IncoTermAndChargeFactory.GetCharge(apportionedChargeKey.ChargeKey.ChargeCode));
		}

		void IChargeApportionee.CalculateAmountBasedOnPercentage(JobComInvCharge charge)
		{
		}

		#endregion

		protected virtual bool JZ_InvoiceCurrExRate_ReadOnly
		{
			get { return !IsJZ_InvoiceCurrExRateUserEnterable; }
		}

#if DEBUG
		public virtual ZString IncotermEquivalentToCFRForTesting
		{
			get { return Core.Constants.IncoTerms.CostAndFreight; }
		}
#endif

		#region DocumentSupporter
		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = CreateNewDocumentSupporter()); }
		}

		DocumentSupporter documentSupporter;

		protected virtual DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobComInvoiceHeaderDocumentSupporter(this);
		}
		#endregion

		public virtual ZString JZ_SupplierMiscFields
		{
			get { return ZString.Empty; }
		}

		protected virtual void SetSupplierFromDeclaration()
		{
			BaseJobDeclaration declaration = JobDeclaration;
			if (JZ_OH_Supplier.IsEmpty && declaration != null)
			{
				JZ_OH_Supplier = declaration.JE_OH_Supplier;
				JZ_OH_SupplierInfo.RefreshBinding();
			}
		}

		[List(nameof(JZ_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_ManufacturerAddress
		{
			get { return base.JZ_OA_ManufacturerAddress; }
			set { base.JZ_OA_ManufacturerAddress = value; }
		}

		[List(nameof(JZ_OA_SupplierAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_SupplierAddress
		{
			get { return base.JZ_OA_SupplierAddress; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobComInvoiceHeader.Schema.JZ_OA_SupplierAddress))
				{
					var oldValue = JZ_OA_SupplierAddress;
					base.JZ_OA_SupplierAddress = value;
					if (!IsSupplierAddressDefaultingSuspended && !IsCopying && oldValue != JZ_OA_SupplierAddress && AllowDefaultSupplier)
					{
						using (SuspendSupplierAddressDefaulting())
						{
							JZ_OH_Supplier = !value.IsValid ? ZGuid.Empty : JZ_OA_SupplierAddress_ZAddress.OrgPK;
						}
					}
				}
			}
		}

		protected virtual bool AllowDefaultSupplier => true;

		[List(nameof(JZ_OA_BuyerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_BuyerAddress
		{
			get { return base.JZ_OA_BuyerAddress; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobComInvoiceHeader.Schema.JZ_OA_BuyerAddress))
				{
					base.JZ_OA_BuyerAddress = value;
				}
			}
		}

		[List(nameof(JZ_OA_DistributorAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_DistributorAddress
		{
			get { return base.JZ_OA_DistributorAddress; }
			set { base.JZ_OA_DistributorAddress = value; }
		}

		[List(nameof(JZ_OA_PackagerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_PackagerAddress
		{
			get { return base.JZ_OA_PackagerAddress; }
			set { base.JZ_OA_PackagerAddress = value; }
		}

		[List(nameof(JZ_OA_ShipperAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_ShipperAddress
		{
			get { return base.JZ_OA_ShipperAddress; }
			set { base.JZ_OA_ShipperAddress = value; }
		}

		#region IWeightApportionee Members

		ZDecimal IWeightApportionee.Amount
		{
			get { return CurrencyConverter.ConvertExact(new Money(JZ_InvoiceAmount, Invoice_Currency), LocalCurrency, false).Amount; }
		}

		ZDecimal IWeightApportionee.Weight
		{
			get { return JZ_Weight; }
			set { JZ_Weight = value; }
		}

		ZString IWeightApportionee.WeightUQ
		{
			get { return JZ_WeightUQ; }
			set { JZ_WeightUQ = value; }
		}

		ZDecimal IWeightApportionee.NetWeight
		{
			get { return JZ_NetWeight; }
			set { JZ_NetWeight = value; }
		}

		ZString IWeightApportionee.NetWeightUQ
		{
			get { return JZ_NetWeightUQ; }
			set { JZ_NetWeightUQ = value; }
		}

		public virtual bool NeedToApportionNetWeight
		{
			get { return true; }
		}

		public virtual ZDecimal MinimumReapportionedLineWeight
		{
			get { return 0m; }
		}

		#endregion

		#region IWeightHolder Members

		ZWeight IWeightHolder.TotalWeight
		{
			get { return new ZWeight(JZ_Weight, JZ_WeightUQ); }
		}

		ZWeight IWeightHolder.TotalNetWeight
		{
			get { return new ZWeight(JZ_NetWeight, JZ_NetWeightUQ); }
		}

		IWeightApportionee[] IWeightHolder.AllApportionees
		{
			get { return JobComInvoiceLines.Cast<IWeightApportionee>().ToArray(); }
		}

		public void ReApportionLineWeightIfNeeded(bool reapportion)
		{
			if (reapportion && (InvoiceDataProvider?.WeightApportionmentEnabled ?? false))
			{
				ApportionLineWeight(null);
			}
		}

		IWeightHolder[] IWeightHolder.WeightHolders
		{
			get { return null; }
		}

		#endregion

		public IDisposable SuspendLineNumberRenumberingForDataImport()
		{
			return new LineNumberRenumberingForDataImportSuspender(this);
		}

		public bool IsLineNumberRenumberingForDataImportSuspended
		{
			get { return lineNumberRenumberingForDataImportIndex > 0; }
		}

		int lineNumberRenumberingForDataImportIndex;
		class LineNumberRenumberingForDataImportSuspender : IDisposable
		{
			public LineNumberRenumberingForDataImportSuspender(BaseJobComInvoiceHeader invoice)
			{
				this.invoice = invoice;
				invoice.lineNumberRenumberingForDataImportIndex++;
				lineNumberRenumberingSuspender = invoice.GetLineNumberRenumberingSuspender();
			}
			readonly IDisposable lineNumberRenumberingSuspender;
			readonly BaseJobComInvoiceHeader invoice;

			public void Dispose()
			{
				lineNumberRenumberingSuspender.Dispose();
				invoice.lineNumberRenumberingForDataImportIndex--;
			}
		}

		#region ISequenceNumberHeader Members

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines
		{
			get { return new TypedEnumerable<ISequenceNumberLine>(JobComInvoiceLines); }
		}

		public IDisposable GetLineNumberRenumberingSuspender()
		{
			return InvoiceLineLineNumberGenerator.GetLineNumberSuspender();
		}

		public ShortSequenceNumberGenerator InvoiceLineLineNumberGenerator => invoiceLineLineNumberGenerator ?? (invoiceLineLineNumberGenerator = InvoiceLineLineNumberGeneratorCore);

		protected virtual ShortSequenceNumberGenerator InvoiceLineLineNumberGeneratorCore
		{
			get { return new ShortSequenceNumberGenerator(this); }
		}

		public void ClearInvoiceLineLineNumberGeneratorCache() => invoiceLineLineNumberGenerator = null;
		ShortSequenceNumberGenerator invoiceLineLineNumberGenerator;

		#endregion

		#region IShortSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => JZ_JE;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => JZ_InvoiceDisplaySequence;
			set => JZ_InvoiceDisplaySequence = value;
		}

		#endregion

		#region IRegistryAccessingSupporter Members

		public virtual Guid RegistryCompanyPK
		{
			get
			{
				Guid result;
				if (PersistentDeclaration is BaseJobDeclaration declaration)
				{
					result = ((IRegistryAccessingSupporter)declaration).RegistryCompanyPK;
				}
				else
				{
					var branch = Branch;
					result = (branch == null) ? GlbCompany.CurrentCompany.PK.ToGuid() : branch.GB_GC.ToGuid();
				}
				return result;
			}
		}

		public virtual Guid RegistryBranchPK
		{
			get
			{
				Guid result;
				if (PersistentDeclaration is BaseJobDeclaration declaration)
				{
					result = ((IRegistryAccessingSupporter)declaration).RegistryBranchPK;
				}
				else
				{
					var branch = Branch;
					result = branch == null ? GlbBranch.CurrentBranch.PK.ToGuid() : branch.PK.ToGuid();
				}
				return result;
			}
		}

		#endregion

		#region ICommonNonApportionedChargeProvider<BaseInvoiceCharge> Members

		BaseInvoiceCharge ICommonNonApportionedChargeProvider<BaseInvoiceCharge>.CreateNew()
		{
			return Charges.AddNew();
		}

		BaseInvoiceCharge ICommonNonApportionedChargeProvider<BaseInvoiceCharge>.GetChargeWithZeroAmount(ZString chargeCode)
		{
			return Charges.GetCharge(chargeCode).FirstOrDefault(x => x.J7_Amount.IsEmpty);
		}

		#endregion

		#region IUnitConverterDataProvider

		ZString IUnitConverterDataProvider.CountryCode
		{
			get { return CountryCode; }
		}

		BusinessObjectFactory IUnitConverterDataProvider.Factory
		{
			get { return Factory; }
		}

		IEnumerable<IUnitConverter> IUnitConverterDataProvider.GetUnitConversionFactorsFromProductUnits()
		{
			return Array.Empty<IUnitConverter>();
		}

		MasterFiles.Business.OrgSupplierPart IUnitConverterDataProvider.Product
		{
			get { return null; }
		}

		ZGuid IUnitConverterDataProvider.SupplierFK
		{
			get { return JZ_OH_Supplier; }
		}

		bool IUnitConverterDataProvider.ProductHasSpecificUnitConversions
		{
			get { return false; }
		}

		ZString IUnitConverterDataProvider.Type
		{
			get { return RPTypeList.Codes.CommercialInvoice; }
		}

		#endregion

		#region ICDArchive Members

		public CDArchiveInfo CDArchiveInfo
		{
			get { return new JobComInvoiceHeaderInfo(this); }
		}

		public class JobComInvoiceHeaderInfo : CDArchiveInfo
		{
			public JobComInvoiceHeaderInfo(BaseJobComInvoiceHeader invoice)
				: base(invoice)
			{
			}

			BaseJobComInvoiceHeader Invoice
			{
				get { return (BaseJobComInvoiceHeader)BusinessEntity; }
			}

			BaseJobDeclaration.DeclarationCDArchiveInfo DeclarationInfo
			{
				get { return (BaseJobDeclaration.DeclarationCDArchiveInfo)((ICDArchive)Invoice.JobDeclaration).CDArchiveInfo; }
			}

			public override ZString ConsigneeCode
			{
				get
				{
					var importer = Invoice.Importer_Effective;
					return importer != null ? importer.OH_Code : ZString.Empty;
				}
			}

			public override ZString ConsignorCode
			{
				get
				{
					var supplier = Invoice.Supplier_Effective;
					return supplier != null ? supplier.OH_Code : ZString.Empty;
				}
			}

			public override ZString[] ContainerNumbersList
			{
				get { return Invoice.JobDeclaration.ContainerNumbersListCore; }
			}

			public override ZString Destination
			{
				get { return DeclarationInfo.Destination; }
			}

			public override ZString[] EntryNumbersList
			{
				get { return DeclarationInfo.EntryNumbersList; }
			}

			public override ZDateTime ETA
			{
				get { return DeclarationInfo.ETA; }
			}

			public override ZDateTime ETD
			{
				get { return DeclarationInfo.ETD; }
			}

			public override ZString VoyageFlight
			{
				get { return DeclarationInfo.VoyageFlight; }
			}

			public override ZString HouseBill
			{
				get { return DeclarationInfo.HouseBill; }
			}

			public override ZString[] InvoiceNumbersList
			{
				get { return Invoice.JobDeclaration.InvoiceNumbersListCore; }
			}

			public override ZString JobNumber
			{
				get { return DeclarationInfo.JobNumber; }
			}

			public override ZString MasterBill
			{
				get { return DeclarationInfo.MasterBill; }
			}

			public override ZString[] OrderNumbersList
			{
				get { return DeclarationInfo.OrderNumbersList; }
			}

			public override ZString Origin
			{
				get { return DeclarationInfo.Origin; }
			}

			public override ZString Vessel
			{
				get { return DeclarationInfo.Vessel; }
			}
		}

		#endregion

		ZString ICurrencyProvider.CurrencyCode
		{
			get { return JZ_RX_NKInvoice_Currency; }
		}

		void ICurrencyProvider.SetExchangeRateIfNotUserOverridden()
		{
			if (!IsJZ_InvoiceCurrExRateUserEnterable)
			{
				SetExchangeRateCore(JZ_InvoiceCurrExRateInfo, CurrencyConverter);
			}
		}

		void ICurrencyProvider.ValidateCurrencyCode()
		{
			Validation.ValidateJZ_RX_NKInvoice_Currency();
		}

		public virtual bool SupportsRelatedBill
		{
			get { return true; }
		}

		protected override bool RegisterCustomBizoAsChild => true;

		#region IWorkflowProvider Members

		public virtual ZString WorkflowProviderCoreCode
		{
			get { return WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get
			{
				return WorkflowProviderCoreCode;
			}
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
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
		public ProcessTaskCollection<BaseJobComInvoiceHeaderProcessTask, BaseJobComInvoiceHeader> WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<BaseJobComInvoiceHeaderProcessTask, BaseJobComInvoiceHeader>(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}
		ProcessTaskCollection<BaseJobComInvoiceHeaderProcessTask, BaseJobComInvoiceHeader> workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return GetRankerForTemplate();
		}

		public virtual ColumnValueRanker GetRankerForTemplate()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClients());
			result.Add(ProcessTaskTemplateSchema.P0_GB, JZ_GB, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, IsAttachedToPersistentDeclaration ? JZ_MessageType : JZ_StandAloneInvoiceDirection, ZString.Empty);
			return result;
		}

		internal protected virtual IZType[] GetClients()
		{
			List<IZType> result = new List<IZType>();
			if (IsImport)
			{
				if (JZ_OH_Buyer.IsValid)
				{
					result.Add(JZ_OH_Buyer);
				}
				else if (PersistentDeclaration is BaseJobDeclaration declaration && declaration.JE_OH_Importer.IsValid)
				{
					result.Add(declaration.JE_OH_Importer);
				}
			}
			if (IsExport)
			{
				if (JZ_OH_Supplier.IsValid)
				{
					result.Add(JZ_OH_Supplier);
				}
				else if (PersistentDeclaration is BaseJobDeclaration declaration && declaration.JE_OH_Supplier.IsValid)
				{
					result.Add(declaration.JE_OH_Supplier);
				}
			}

			result.Add(ZGuid.Empty);
			return result.ToArray();
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, this, properties);
			}

			return customBusinessObject;
		}

		CustomBusinessObject customBusinessObject;

		#endregion

		#region IWorkflowAffectedPropertyProvider Members

		public ZPropertyInfo[] PropertyThatAffectWorkflowChanged
		{
			get
			{
				if (JobDeclaration is BaseJobDeclaration declaration)
				{
					return new ZPropertyInfo[]
					{
						declaration.JE_MessageTypeInfo,
						declaration.JE_OH_ImporterInfo,
						declaration.JE_OH_SupplierInfo
					};
				}

				return Array.Empty<ZPropertyInfo>();
			}
		}

		#endregion

		#region Additional Declarations Support

		protected override IRelatedDeclarationGenPivotCollection GetNewRelatedDeclarationGenPivots()
		{
			return new InvoiceRelatedDeclarationGenPivotCollection(this);
		}

		protected override void OnAttachedToAdditionalDeclaration(BaseJobDeclaration declaration)
		{
			if (!IsRefreshAdditionalInvoiceSuspended)
			{
				declaration.Invoices.RefreshAdditionalInvoices();
				declaration.InvoiceLines.Load();
				declaration.RegisterEditableChildObject(InvoiceLines);
			}
		}

		protected override void OnDetachedFromAdditionalDeclaration(BaseJobDeclaration declaration)
		{
			if (!IsRefreshAdditionalInvoiceSuspended)
			{
				declaration.Invoices.RefreshAdditionalInvoices();
				declaration.InvoiceLines.Load();
				declaration.UnRegisterEditableChildObject(InvoiceLines);
			}
		}

		public IDisposable SuspendRefreshAdditionalInvoice() => new DisposableAction(() => refreshAdditionalInvoiceCount++, () => refreshAdditionalInvoiceCount--);

		int refreshAdditionalInvoiceCount;

		internal bool IsRefreshAdditionalInvoiceSuspended => refreshAdditionalInvoiceCount > 0;

		#endregion

		#region Product Audit

		internal ZString ProductAuditAction()
		{
			return ProductAuditActionCore();
		}

		protected virtual ZString ProductAuditActionCore()
		{
			var orgAuditAction = IsImport ? Importer_Effective?.MiscServ.OM_IMValidationForUnauditedClassification : Supplier_Effective?.MiscServ.OM_EXValidationForUnauditedClassification;
			var orgAuditActionCode = orgAuditAction ?? (ZString)MasterFiles.Business.Customs.ProductAuditActions.Codes.RegistryDefault;
			return orgAuditActionCode == MasterFiles.Business.Customs.ProductAuditActions.Codes.RegistryDefault ? GetRegistryAuditAction() : orgAuditActionCode;
		}

		ZString GetRegistryAuditAction()
		{
			var branch = Branch;
			var branchPK = branch?.PK.ToGuid() ?? Guid.Empty;
			var companyPK = branch?.Company?.PK.ToGuid() ?? Guid.Empty;
			var auditActionRegistryItem = IsImport ? CustomsDataRegistry.Instance.ImportProductAuditAction : CustomsDataRegistry.Instance.ExportProductAuditAction;
			return auditActionRegistryItem.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		#endregion

		Integration.Customs.IBaseJobComInvoiceLine Integration.Customs.Shared.IBaseJobComInvoiceHeader.AddNewInvoiceLine()
		{
			return InvoiceLines.AddNew();
		}

		#region ConsigneeOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ImporterList))]
		[ResourceStringData("BaseJobComInvoiceHeader.ConsigneeOrgPK", Caption = "Consignee")]
		public ZGuid ConsigneeOrgPK { get => JZ_OA_ConsigneeAddress_ZAddress.OrgPK; set => JZ_OA_ConsigneeAddress_ZAddress.OrgPK = value; }

		public ZPropertyInfo ConsigneeOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeOrgPK, x => JZ_OA_ConsigneeAddress_ZAddress.OrgPKInfo); }
		}

		[List(nameof(JZ_OA_ConsigneeAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_ConsigneeAddress
		{
			get => base.JZ_OA_ConsigneeAddress;
			set
			{
				base.JZ_OA_ConsigneeAddress = value;
			}
		}
		#endregion

		#region IntermediateConsigneeOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ImporterList))]
		[ResourceStringData("BaseJobComInvoiceHeader.IntermediateConsigneeOrgPK", Caption = "Intermediate Consignee")]
		public ZGuid IntermediateConsigneeOrgPK { get => JZ_OA_IntermediateConsigneeAddress_ZAddress.OrgPK; set => JZ_OA_IntermediateConsigneeAddress_ZAddress.OrgPK = value; }

		public ZPropertyInfo IntermediateConsigneeOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.IntermediateConsigneeOrgPK, x => JZ_OA_IntermediateConsigneeAddress_ZAddress.OrgPKInfo); }
		}

		[List(nameof(JZ_OA_IntermediateConsigneeAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_IntermediateConsigneeAddress
		{
			get => base.JZ_OA_IntermediateConsigneeAddress;
			set
			{
				base.JZ_OA_IntermediateConsigneeAddress = value;
			}
		}
		#endregion

		#region SellingAgentOrgPK

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SellerConsignors))]
		public ZGuid SellingAgentOrgPK
		{
			get { return JZ_OH_SellingAgent; }
			set { JZ_OH_SellingAgent = value; }
		}

		#endregion

		[List(nameof(JZ_OA_SoldToPartyAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_SoldToPartyAddress
		{
			get => base.JZ_OA_SoldToPartyAddress;
			set
			{
				base.JZ_OA_SoldToPartyAddress = value;
			}
		}

		[List(nameof(JZ_OA_ShipToPartyAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_ShipToPartyAddress
		{
			get => base.JZ_OA_ShipToPartyAddress;
			set
			{
				base.JZ_OA_ShipToPartyAddress = value;
			}
		}

		[List(nameof(JZ_OA_SellerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_SellerAddress
		{
			get => base.JZ_OA_SellerAddress;
			set
			{
				base.JZ_OA_SellerAddress = value;
			}
		}

		[List(nameof(JZ_OA_ExporterAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid JZ_OA_ExporterAddress
		{
			get => base.JZ_OA_ExporterAddress;
			set
			{
				base.JZ_OA_ExporterAddress = value;
			}
		}

		public IEnumerable<CusEntryInstruction> CusEntryInstructions => Factory.GetValue(ref cusEntryInstructionsCached, () => InvoiceLines
																															.Cast<BaseJobComInvoiceLine>()
																															.Select(c => c.EntryInstruction)
																															.Where(c => c != null)
																															.Distinct()
																															.ToArray());
		CachedProperty<CusEntryInstruction[]> cusEntryInstructionsCached;

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion

		#region IInvoiceHeaderForProductCreation Members

		bool IInvoiceHeaderForProductCreation.HasImportDeclaration => JobDeclaration.IsImport;

		bool IInvoiceHeaderForProductCreation.HasExportDeclration => JobDeclaration.IsExport;

		bool IInvoiceHeaderForProductCreation.InvoicesContainNotPersistentActiveProductWithMissingInvoiceUQ => JobDeclaration.InvoicesContainNotPersistentActiveProductWithMissingInvoiceUQ;

		bool IInvoiceHeaderForProductCreation.IsInwardBondedWarehousingEnabled => JobDeclaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry;

		OrgHeader IInvoiceHeaderForProductCreation.Importer_Effective => Importer_Effective;

		OrgHeader IInvoiceHeaderForProductCreation.Supplier_Effective => Supplier_Effective;

		ZString IInvoiceHeaderForProductCreation.InvoiceNumber => JZ_InvoiceNumber;

		ZString IInvoiceHeaderForProductCreation.FinalDestinationCountryCode => JobDeclaration.FinalDestinationCountryCode;

		ZString IInvoiceHeaderForProductCreation.BranchCompanyCountryCode => JobDeclaration.BranchCompanyCountryCode;

		#endregion

		public virtual Type GetDeclarationTypeForFakeDeclarationCreatorForInvoice() => typeof(BaseJobDeclaration);

		public bool AllowNonWesternEuropeanCharacterForMarksAndNumbers => AllowNonWesternEuropeanCharacterForMarksAndNumbersCore;

		protected virtual bool AllowNonWesternEuropeanCharacterForMarksAndNumbersCore => false;

		public bool IsInvoiceUsedOverMultipleEntryInstructions
		{
			get
			{
				var result = false;

				if (JobDeclaration?.AreMultipleEntryInstructionsAllowed ?? false)
				{
					result = InvoiceLines.Cast<BaseJobComInvoiceLine>().Select(x => x.JI_CEI).Distinct().Count() > 1;
				}

				return result;
			}
		}
		public int GetChildPackageUsage(BasePackage parentPackage)
		{
			if (parentPackage == null)
			{
				return default;
			}
			var invoiceHeaderChildUsage = GetInvoiceHeaderChildPackageUsage(parentPackage);
			var invoiceLineChildUsage = 0;
			var invoiceLinesInHeader = InvoiceLines;
			if (null != invoiceLinesInHeader)
			{
				foreach (var invoiceHeaderInvoiceLine in invoiceLinesInHeader.Cast<BaseJobComInvoiceLine>())
				{
					invoiceLineChildUsage += invoiceHeaderInvoiceLine.GetChildPackageUsage(parentPackage);
				}
			}
			return invoiceHeaderChildUsage + invoiceLineChildUsage;
		}

		public void SyncParentPivotPackNum(BasePackage parentPackage)
		{
			var pivot = GetPivotByPackage(parentPackage);
			if (null != pivot)
			{
				pivot.CHZ_NumberOfPacks = new ZInt(GetChildPackageUsage(parentPackage));
			}
		}

		InvoiceHeaderPackagePivot GetPivotByPackage(BasePackage package)
		{
			return PackagesPivot?.Cast<InvoiceHeaderPackagePivot>().FirstOrDefault(x => package != null && x.CHZ_CW == package.PK);
		}

		int GetInvoiceHeaderChildPackageUsage(BasePackage package)
		{
			var subPackagePKs = package?.Children?.Select(x => x.PK);
			var subPackageUsage = PackagesPivot?.Where(x => subPackagePKs != null && subPackagePKs.Contains(x.Package.PK)).Sum(x => x.CHZ_NumberOfPacks) ?? 0;
			return subPackageUsage;
		}

		public ICommonInvoiceDataProvider InvoiceDataProvider => invoiceDataProvider ??= (UseNewStandaloneInvoiceData ? StandaloneCommonInvoiceProvider : JobDeclaration);
		ICommonInvoiceDataProvider invoiceDataProvider;

		protected override void ResetOnDeclarationChanged()
		{
			base.ResetOnDeclarationChanged();
			invoiceDataProvider = null;
		}

		public bool UseNewStandaloneInvoiceData => CustomsDataRegistry.Instance.EnableNewStandaloneInvoiceData.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty) && UseNewStandaloneInvoiceDataCore;

		protected virtual bool UseNewStandaloneInvoiceDataCore => false;

		public StandaloneCommonInvoiceProvider StandaloneCommonInvoiceProvider
		{
			get
			{
				if (standaloneCommonInvoiceProvider == null && UseNewStandaloneInvoiceData)
				{
					standaloneCommonInvoiceProvider = CreateStandaloneCommonInvoiceProvider();
				}
				return standaloneCommonInvoiceProvider;
			}
		}
		StandaloneCommonInvoiceProvider standaloneCommonInvoiceProvider;

		protected virtual StandaloneCommonInvoiceProvider CreateStandaloneCommonInvoiceProvider() => new StandaloneCommonInvoiceProvider(this);

		protected override Logs GetNewLogs()
		{
			var result = base.GetNewLogs();
			result.EventsThatCannotBeAdded.Add(AutoEvents.LockForEdit);
			result.EventsThatCannotBeAdded.Add(AutoEvents.UnlockForEdit);

			return result;
		}

		#region IAddInfoChildSupporter Members

		BusinessObject IAddInfoChildSupporter.AddInfoChild => GetAddInfoChild();
		protected virtual BusinessObject GetAddInfoChild() => null;

		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => GetChildForeignKeyColumn();

		IComparable IBaseInvoiceHeader.OrderByColumn => OrderByColumnCore;

		protected virtual IComparable OrderByColumnCore => JZ_InvoiceNumber;

		protected virtual SchemaGuidColumn GetChildForeignKeyColumn() => null;

		void IAddInfoChildSupporter.RegisterListChangedCalledRefreshBinding(IBindingList element) => RegisterListChangedCalledRefreshBinding(element);
		void IAddInfoChildSupporter.UnRegisterListChangedCalledRefreshBinding(IBindingList element) => UnRegisterListChangedCalledRefreshBinding(element);
		#endregion

		#region Universal Copy

		public bool IsUniversalCopying { get; private set; }

		protected void StartUniversalCopy()
		{
			IsUniversalCopying = true;
		}

		protected void FinishUniversalCopy()
		{
			if (Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>() is BusinessObjectUniversalCopyFactoryService factoryCopyService)
			{
				factoryCopyService.AddOnCopyFinishedAction(() =>
				{
					IsUniversalCopying = false;
				});
			}
		}

		#endregion

		#region Test Helpers
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			JZ_RelatedIndicator = "Y";
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateString(ZPropertyInfo property)
			{
				if (!property.Name.Equals(Schema.SupplierName) && !property.Name.Equals(Schema.JZ_MessageType))
				{
					base.PopulateString(property);
				}
			}
		}

#endif
		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => Branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region ITriggerActionProvider

		ZString ITriggerActionProvider.ReasonForDoNotTriggerAction
		{
			get
			{
				if (IsAttachedToPersistentDeclaration)
				{
					return Res.GetString("9193591E-C4D3-4F88-A09D-13062CA3259A", "Commercial Invoice {0} is attached to Customs Declaration {1}.", JZ_InvoiceNumber, JobDeclaration.JE_DeclarationReference);
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region ICustomsFileParent

		ZString ICustomsFileParent.DeclarationType
		{
			get { return JZ_StandAloneInvoiceDirection; }
		}

		ZPropertyInfo ICustomsFileParent.DeclarationTypeInfo
		{
			get { return JZ_StandAloneInvoiceDirectionInfo; }
		}

		ZGuid ICustomsFileParent.BranchPk
		{
			get { return JZ_GB; }
		}

		ZBool ICustomsFileParent.IsLocked
		{
			get { return Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit) != null; }
		}

		void ICustomsFileParent.LockFile(ZString reference)
		{
			this.AddLockEvent(reference);
		}

		void ICustomsFileParent.UnlockFile(ZString reference)
		{
			this.AddUnlockEvent(reference);
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get
			{
				return JobDeclaration?.Company ?? Branch?.Company ?? GlbCompany.CurrentCompany;
			}
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				if (JobDeclaration is IWorkflowProviderCore parent)
				{
					list.Add(parent);
				}
				return list;
			}
		}

		#endregion
	}
}
