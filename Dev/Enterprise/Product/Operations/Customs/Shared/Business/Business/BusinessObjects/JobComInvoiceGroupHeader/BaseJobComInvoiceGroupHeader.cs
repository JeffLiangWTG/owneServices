using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(BaseJobComInvoiceGroupHeader.Schema.JZ_InvoiceNumber), DescriptionProperty(BaseJobComInvoiceGroupHeader.Schema.JZ_InvoiceNumber)]
	[DependentBusinessObject(typeof(BaseJobDeclaration), "JobComInvoiceGroupHeaders")]
	[UniversalCopyInstanceType(CreationMethod = "NewForUniversalCopy")]
	[UniversalCopyMappingKeys(BaseJobComInvoiceGroupHeader.Schema.JZ_JZ_GroupInvoiceFK)]
	public class BaseJobComInvoiceGroupHeader
		: CommonJobComInvoiceHeader,
		Integration.Customs.Shared.IBaseJobComInvoiceGroupHeader,
		IExternalFactoryRefreshable,
		IInvoiceParent,
		ICommonInvoice,
		ILandedCostDistributeTo,
		ILandedCostChargeHolder,
		IDeclarationProvider,
		ICurrencyConverterDataProviderWithFixedExRates,
		ICurrencyConverterProvider,
		IGroupInvoiceOrInvoice,
		IChargeHolder,
		ITypeDeciderContext,
		ICommonNonApportionedChargeProvider<BaseGroupInvoiceCharge>
	{
		public BaseJobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly new TypeDecider TypeDecider = new BaseJobComInvoiceGroupHeaderTypeDecider();

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string AllInvoices = "All Invoices";

		public new class Schema : AutoJobComInvoiceHeader.Schema
		{
			public const string JZ_Calc_TNI = "JZ_Calc_TNI";
			public const string JZ_Calc_FOBAmount = "JZ_Calc_FOBAmount";
			public const string JZ_Calc_FOBCurrency = "JZ_Calc_FOBCurrency";
			public const string JZ_Calc_CIFAmount = "JZ_Calc_CIFAmount";
			public const string JZ_Calc_CIFCurrency = "JZ_Calc_CIFCurrency";
			public const string JZ_RX_LocalCurrency = "JZ_RX_LocalCurrency";
		}

		#endregion

		public new JobComInvoiceGroupHeaderValidation Validation
		{
			get { return (JobComInvoiceGroupHeaderValidation)base.Validation; }
		}

		protected override bool IsLookupsCachedInBase
		{
			get { return false; }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.JobComInvoiceGroupHeaderFetchStrategy(this);
		}

		#region Related BusinessObjects

		//Bind to TreeView
		[ChildEditable(false)]
		public IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> JobComInvoiceGroupHeaders
		{
			get
			{
				if (fJobComInvoiceGroupHeaders == null)
				{
					fJobComInvoiceGroupHeaders = CreateNewJobComInvoiceGroupHeaderCollection();
					RegisterEditableChildObject(fJobComInvoiceGroupHeaders);
				}
				return fJobComInvoiceGroupHeaders;
			}
		}

		public InvoiceHeaderActiveCollection JobComInvoiceHeaders
		{
			get
			{
				if (fJobComInvoiceHeaders == null)
				{
					fJobComInvoiceHeaders = CreateNewDirectInvoiceHeaderCollection();
				}
				return fJobComInvoiceHeaders;
			}
		}

		public bool IsThisInvoicePartOfThisGroup(BaseJobComInvoiceHeader invoice)
		{
			BaseJobComInvoiceGroupHeader groupHeader = invoice.GroupHeader;

			bool result = false;

			while (!result && groupHeader != null)
			{
				result = groupHeader == this;
				if (!result)
				{
					groupHeader = groupHeader.GroupHeader;
				}
			}

			return result;
		}

		/// <summary>
		/// This is not exposed in GUI. RegisterEditibleChild or SetReadOnly not necessary
		/// </summary>
		public InvoiceHeaderActiveCollection AllJobComInvoiceHeaders
		{
			get
			{
				if (fAllJobComInvoiceHeaders == null)
				{
					fAllJobComInvoiceHeaders = GetNewAllInvoiceHeaderCollection();
				}
				return fAllJobComInvoiceHeaders;
			}
		}

		protected virtual InvoiceHeaderActiveCollection GetNewAllInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this, false);
		}

		protected BaseGroupHeaderInvoiceLineViewCollection fAllJobComInvoiceLines;
		public BaseGroupHeaderInvoiceLineViewCollection AllJobComInvoiceLines
		{
			get
			{
				if (fAllJobComInvoiceLines == null && !IsDeleted)
				{
					fAllJobComInvoiceLines = new BaseGroupHeaderInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
				}
				return fAllJobComInvoiceLines;
			}
		}

		protected virtual BaseJobDeclaration GetJobDeclaration(ZGuid guidToLoad)
		{
			return Factory.Load<BaseJobDeclaration>(guidToLoad);
		}

		[BusinessObjectTestExclude()]
		public override ZBool JZ_GroupInvoice
		{
			get { return base.JZ_GroupInvoice; }
			set
			{
				if (!value)
				{
					throw new NotSupportedException("Cannot convert a group header into an invoice header");
				}
				base.JZ_GroupInvoice = value;
			}
		}

		[BusinessObjectTestExclude()]
		public override ZString JZ_InvoiceNumber
		{
			get => base.JZ_InvoiceNumber;
			set
			{
				var oldValue = JZ_InvoiceNumber;
				base.JZ_InvoiceNumber = value;
				var newValue = JZ_InvoiceNumber;
				if (!IsCopying && oldValue != newValue)
				{
					ReportIfTopGroupInvoiceIsSetIncorrectly(oldValue, newValue);
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				if (base.JZ_JE != value)
				{
					base.JZ_JE = value;
				}
			}
		}

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

		protected override JobComInvoiceHeaderValidation GetNewValidation()
		{
			return new JobComInvoiceGroupHeaderValidation(this);
		}

		internal JobComInvoiceGroupHeaderValidation GroupValidation
		{
			get { return Validation; }
		}

		[ChildEditable(true)]
		public IJobComInvChargeCollection<BaseGroupInvoiceCharge> Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = CreateGroupInvoiceChargeCollection();
					RegisterEditableChildObject(fCharges);
				}
				return fCharges;
			}
		}
		protected IJobComInvChargeCollection<BaseGroupInvoiceCharge> fCharges;

		protected virtual IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection()
		{
			return new JobComInvChargeCollection<BaseGroupInvoiceCharge>(this);
		}

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

		#endregion

		#region overriden Properties

		public ZString JZ_Calc_EffectiveInvoiceNumber
		{
			get
			{
				ZString result = JZ_InvoiceNumber;
				if (result.IsEmpty)
				{
					result = (NoResString)"All Invoices";
				}
				return result;
			}
		}

		protected bool JZ_InvoiceNumber_ReadOnly
		{
			get { return JobDeclaration != null && JobDeclaration.JobComInvoiceGroupHeaders[0].PK == PK; }
		}

		[BusinessObjectTestExclude]
		[LightValidationTestExempt]
		public override ZString JZ_AddInfo
		{
			get { return base.JZ_AddInfo; }
			set
			{
				base.JZ_AddInfo = value;
			}
		}

		public ZDecimal JZ_Calc_FOBAmount
		{
			get
			{
				Money result = new Money(0, null);

				foreach (BaseJobComInvoiceHeader aHeader in AllJobComInvoiceHeaders)
				{
					result = aHeader.CurrencyConverter.Add(result, new Money(aHeader.JZ_Calc_FOBAmount, aHeader.CalcFOBCurrency));
				}
				return result.Amount;
			}
		}

		public ZPropertyInfo JZ_Calc_FOBAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_FOBAmount); }
		}

		public ZGuid JZ_Calc_FOBCurrency
		{
			get
			{
				Money result = new Money(0, null);

				foreach (BaseJobComInvoiceHeader aHeader in AllJobComInvoiceHeaders)
				{
					result = aHeader.CurrencyConverter.Add(result, new Money(aHeader.JZ_Calc_FOBAmount, aHeader.CalcFOBCurrency));
				}

				return result.Currency == null ? ZGuid.Empty : result.Currency.PK;
			}
		}

		public ZPropertyInfo JZ_Calc_FOBCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_FOBCurrency); }
		}

		public ZDecimal JZ_Calc_CIFAmount
		{
			get
			{
				Money result = new Money(0, null);

				foreach (BaseJobComInvoiceHeader aHeader in AllJobComInvoiceHeaders)
				{
					result = aHeader.CurrencyConverter.Add(result, new Money(aHeader.JZ_Calc_CIFAmount, aHeader.CalcCIFCurrency));
				}
				return result.Amount;
			}
		}

		public ZPropertyInfo JZ_Calc_CIFAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_CIFAmount); }
		}

		public ZGuid JZ_Calc_CIFCurrency
		{
			get
			{
				Money result = new Money(0, null);

				foreach (BaseJobComInvoiceHeader aHeader in AllJobComInvoiceHeaders)
				{
					result = aHeader.CurrencyConverter.Add(result, new Money(aHeader.JZ_Calc_CIFAmount, aHeader.CalcCIFCurrency));
				}

				return result.Currency == null ? ZGuid.Empty : result.Currency.PK;
			}
		}

		public ZPropertyInfo JZ_Calc_CIFCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_CIFCurrency); }
		}

		#endregion

		#region Override Methods

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JZ_InvoiceDate = ZDateTime.Today;
			JZ_GroupInvoice = true;
			IsDefaultTopGroupHeader = false;
		}

		protected override IBusiness[] OtherChildrenToLoad()
		{
			var result = new List<IBusiness>();
			if (this.JobDeclaration != null)
			{
				result.Add(JobComInvoiceHeaders);
				result.Add(JobComInvoiceGroupHeaders);
			}
			return result.ToArray();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!JZ_JZ_GroupInvoiceFK.IsValid)
			{
				JZ_JZ_GroupInvoiceFK = Guid.Empty;
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();

				Charges.RemoveAndDeleteAll();

				if (JobDeclaration != null)
				{
					LoadChildEditableObjectsForChild(new IBusiness[] { JobComInvoiceHeaders });
					var invoices = JobComInvoiceHeaders.Cast<BaseJobComInvoiceHeader>().ToArray();
					foreach (var invoice in invoices)
					{
						invoice.FetchStrategy.FetchForDelete();
					}
					foreach (var invoiceHeader in invoices)
					{
						invoiceHeader.Delete();
					}

					JobComInvoiceGroupHeaders.RemoveAndDeleteAll();
					JobDeclaration.MarkApportionmentDirty();
				}
				base.Delete();
			}
		}

		protected override JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceGroupHeaderLookups(this);
		}

		public override bool IsSavedByFactory
		{
			get { return fIsPersistent && base.IsSavedByFactory; }
		}
		bool fIsPersistent = true;

		public void MakeNonPersistent()
		{
			fIsPersistent = false;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region public methods

		public BaseJobComInvoiceGroupHeader GetGroupHeaderWithThisCharge(ChargeCodeChargeKey chargeKey)
		{
			return GetGroupHeaderWithThisCharge(chargeKey, false);
		}

		public BaseJobComInvoiceGroupHeader GetGroupHeaderWithThisCharge(ChargeCodeChargeKey chargeKey, bool shouldTakePercentageValue)
		{
			BaseJobComInvoiceGroupHeader result = null;
			BaseJobComInvoiceGroupHeader current = this;

			do
			{
				if (shouldTakePercentageValue && current.Charges.HasChargeWithPercentage(chargeKey)
					|| current.Charges.HasChargeWithCurrency(chargeKey))
				{
					result = current;
					break;
				}

				current = current.GroupHeader;
			}
			while (current != null);

			return result;
		}

		#endregion

		#region New Properties

		#region JZ_Calc_TNI

		public ZDecimal JZ_Calc_TNI
		{
			get
			{
				decimal result = 0.0m;
				foreach (BaseJobComInvoiceHeader aHeader in AllJobComInvoiceHeaders)
				{
					result += aHeader.JZ_Calc_TNI;
				}
				return result;
			}
		}

		public ZPropertyInfo JZ_Calc_TNIInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Calc_TNI); }
		}

		#endregion JZ_Calc_TNI

		#endregion

		#region JZ_LocalCurrency

		public ZGuid JZ_RX_LocalCurrency
		{
			get
			{
				var localCurrency = LocalCurrency;
				return localCurrency != null ? localCurrency.PK : ZGuid.Empty;
			}
		}

		public ZPropertyInfo JZ_RX_LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_RX_LocalCurrency); }
		}

		public RefCurrency LocalCurrency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, BaseJobDeclaration.GetLocalCurrencyCodeFor(JobDeclaration)); }
		}

		#endregion

		#region Implementation

		void ReportIfTopGroupInvoiceIsSetIncorrectly(ZString oldInvoiceNumber, ZString newInvoiceNumber)
		{
			if (!oldInvoiceNumber.IsEmpty && JZ_JZ_GroupInvoiceFK.IsEmpty && !newInvoiceNumber.EqualsIgnoringCase(AllInvoices))
			{
				ErrorReporter.ReportOnce("Incorrectly Top Group Invoice JZ_InvoiceNumber", $"Top Group Invoice number set to invalid value (Old:{oldInvoiceNumber}, New:{newInvoiceNumber})");
			}
		}

		#region Collection Management

		IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> fJobComInvoiceGroupHeaders;
		InvoiceHeaderActiveCollection fJobComInvoiceHeaders;
		InvoiceHeaderActiveCollection fAllJobComInvoiceHeaders;

		protected virtual InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this, true);
		}

		protected virtual IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection()
		{
			return new BaseJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader>(this);
		}

		#endregion

		#endregion

		public BusinessObject Master
		{
			get
			{
				if (GroupHeader != null)
				{
					return GroupHeader;
				}
				else
				{
					return JobDeclaration;
				}
			}
		}

		protected override void MarkAsNeedingValidationForMajorDataChangeCore()
		{
			base.MarkAsNeedingValidationForMajorDataChangeCore();

			if (!IsCopying && JobDeclaration != null)
			{
				ZQuery query = new ZQuery(JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK, PK);
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				BaseJobComInvoiceHeader[] invoices = Factory.Load<BaseJobComInvoiceHeader>(query);

				foreach (BaseJobComInvoiceHeader header in invoices)
				{
					header.MarkAsNeedingValidationForMajorDataChange();
				}
			}

			Charges.MarkAsNeedingValidation();
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
			JobComInvoiceGroupHeaders.ExternalFactoryRefreshEnabled = ExternalFactoryRefreshEnabled;
			JobComInvoiceHeaders.ExternalFactoryRefreshEnabled = ExternalFactoryRefreshEnabled;
		}

		#endregion

		#region Calculated Charges for Document Wrapper

		public ZDecimal CalcOverseasFreight
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight)); }
		}

		public ZGuid CalcOverseasFreightCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight)); }
		}

		public ZDecimal CalcOverseasInsurance
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance)); }
		}

		public ZGuid CalcOverseasInsuranceCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance)); }
		}

		public ZDecimal CalcExWorks
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.ExWorks)); }
		}

		public ZGuid CalcExWorksCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.ExWorks)); }
		}

		public ZDecimal CalcForeignInlandFreight
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight)); }
		}

		public ZGuid CalcForeignInlandFreightCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight)); }
		}

		public ZDecimal CalcPackingCosts
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.PackingCost)); }
		}

		public ZGuid CalcPackingCostsCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.PackingCost)); }
		}

		public ZDecimal CalcLandingCharges
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.LandingCharges)); }
		}

		public ZGuid CalcLandingChargesCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.LandingCharges)); }
		}

		public ZDecimal CalcOtherCharges1
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OtherCharges)); }
		}

		public ZGuid CalcOtherCharges1Currency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OtherCharges)); }
		}

		public ZDecimal CalcOtherCharges2
		{
			get { return GetAmountWithThisKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, false)); }
		}

		public ZGuid CalcOtherCharges2Currency
		{
			get { return GetCurrencyWithThisKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, false)); }
		}

		public ZDecimal CalcDiscount
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.Discount)); }
		}

		public ZGuid CalcDiscountCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.Discount)); }
		}

		public ZDecimal CalcCommission
		{
			get { return GetAmountWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.Commission)); }
		}

		public ZGuid CalcCommissionCurrency
		{
			get { return GetCurrencyWithThisKey(IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.Commission)); }
		}

		protected ZDecimal GetAmountWithThisKey(ICustomsChargeCode chargeCode)
		{
			return chargeCode == null ? ZDecimal.Zero : GetAmountWithThisKey(chargeCode.ChargeCodeChargeKey);
		}

		protected ZDecimal GetAmountWithThisKey(ChargeCodeChargeKey chargeKey)
		{
			return Charges.GetCharge(chargeKey).Amount;
		}

		protected ZGuid GetCurrencyWithThisKey(ICustomsChargeCode chargeCode)
		{
			return chargeCode == null ? ZGuid.Empty : GetCurrencyWithThisKey(chargeCode.ChargeCodeChargeKey);
		}

		protected ZGuid GetCurrencyWithThisKey(ChargeCodeChargeKey chargeKey)
		{
			Money result = Charges.GetCharge(chargeKey);
			return result.Currency == null ? Guid.Empty : result.Currency.PK;
		}
		#endregion

		#region IInvoiceParent Members

		BaseJobDeclaration IDeclarationProvider.Declaration
		{
			get { return JobDeclaration; }
		}

		SchemaGuidColumn IInvoiceParent.ForeignKeyInInvoiceToParent
		{
			get { return JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK; }
		}

		#endregion

		#region ICommonInvoice Members

		public virtual CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null)
				{
					fCurrencyConverter = new CurrencyConverterWithDataProvider(Factory, this);
				}
				return fCurrencyConverter;
			}
		}
		CurrencyConverter fCurrencyConverter;

		public ZDateTime EffectiveValuationDate
		{
			get { return AllJobComInvoiceHeaders.EffectiveValuationDateForParentGroup; }
		}

		public bool IsGroup
		{
			get { return true; }
		}

		public ZString IncoTerm
		{
			get { return ZString.Empty; }
		}

		IAllInvoiceLines ICommonInvoice.InvoiceLines
		{
			get { return AllJobComInvoiceLines; }
		}

		Common.ICommonInvoice Common.ICommonInvoice.ImmediateCommonInvoiceParent
		{
			get { return GroupHeader; }
		}

		ZString ICommonInvoice.UserFriendlyCode
		{
			get { return JZ_Calc_EffectiveInvoiceNumber; }
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
			get { return BaseJobDeclaration.GetLocalCurrencyCodeFor(JobDeclaration); }
		}

		CodeDescriptionPairList ICommonInvoice.ChargeTypeList
		{
			get { return GetCustomsChargeTypeList(JZ_GroupInvoice ? ChargeParentTypes.GroupInvoice : ChargeParentTypes.Invoice); }
		}

		CodeDescriptionPairList ICommonInvoice.AllChargeTypeList
		{
			get { return GetCustomsChargeTypeList(ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine); }
		}

		bool Common.ICommonInvoice.HasMultipleInvoiceUQs
		{
			get { return JobComInvoiceHeaders.Any(invoice => invoice.HasMultipleInvoiceUQs); }
		}

		#endregion

		#region ILandedCostDistributeTo Members

		ZGuid ILandedCostDistributeTo.PK
		{
			get { return PK; }
		}

		ZString ILandedCostDistributeTo.TableCode
		{
			get { return JobComInvoiceHeaderSchema.Constants.Prefix; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public const string GroupInvoiceConstString = "GROUP INVOICE ";

		ZString ILandedCostDistributeTo.UniqueCode
		{
			get { return GroupInvoiceConstString + JZ_Calc_EffectiveInvoiceNumber; }
		}

		ZString ILandedCostDistributeTo.Description
		{
			get { return ((ILandedCostDistributeTo)this).UniqueCode; }
		}

		IEnumerable<IUltimateDistributee> ILandedCostDistributeTo.UltimateDistributees
		{
			get { return new TypedEnumerable<IUltimateDistributee>(AllJobComInvoiceLines); }
		}

		#endregion

		#region ILandedCostChargeHolder Members

		IEnumerable<IDefaultLandedCostInput> ILandedCostChargeHolder.ChargesToImportForLandedCosting
		{
			get
			{
				if (AllJobComInvoiceHeaders.Count > 0 && JobDeclaration != null && !JobDeclaration.ApportionmentDirty)
				{
					foreach (BaseGroupInvoiceCharge groupCharge in Charges)
					{
						if (((IDefaultLandedCostInput)groupCharge).IsValidToImport)
						{
							yield return groupCharge;
						}
					}
				}
			}
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
			get { return ((ICurrencyConverterDataProvider)JobDeclaration).Company; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return JobDeclaration.LocalCurrencyCode; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return JobDeclaration.IsReciprocalRates; }
		}

		#endregion

		#region ICurrencyConverterDataProviderWithFixedExRates Members

		string ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRateCurrencyCode
		{
			get { return ""; }
		}

		decimal ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRate
		{
			get { return decimal.Zero; }
		}

		#endregion

		#region IGroupInvoiceOrInvoice Members

		void IGroupInvoiceOrInvoice.Move(BaseJobComInvoiceGroupHeader origin, BaseJobComInvoiceGroupHeader destination)
		{
			JZ_JZ_GroupInvoiceFK = destination.PK;
			ActiveBusinessObjectCollection<BaseJobComInvoiceHeader>.RefreshAll(Factory);
		}

		BaseJobComInvoiceGroupHeader IGroupInvoiceOrInvoice.GroupInvoiceOfInvoiceOrGroupInvoiceItself
		{
			get { return this; }
		}

		BaseJobComInvoiceGroupHeader IGroupInvoiceOrInvoice.ParentGroupInvoice
		{
			get { return GroupHeader; }
		}

		bool IGroupInvoiceOrInvoice.IsGroupInvoice
		{
			get { return true; }
		}

		IGroupInvoiceOrInvoice[] IGroupInvoiceOrInvoice.ChildGroupInvoices
		{
			get
			{
				ArrayList result = new ArrayList();
				result.AddRange(JobComInvoiceGroupHeaders);
				return (IGroupInvoiceOrInvoice[])result.ToArray(typeof(IGroupInvoiceOrInvoice));
			}
		}

		IGroupInvoiceOrInvoice[] IGroupInvoiceOrInvoice.ChildInvoices
		{
			get
			{
				ArrayList result = new ArrayList();
				result.AddRange(JobComInvoiceHeaders);
				return (IGroupInvoiceOrInvoice[])result.ToArray(typeof(IGroupInvoiceOrInvoice));
			}
		}

		#endregion

		#region IChargeHolder Members

		IJobComInvChargeCollection<JobComInvCharge> IChargeHolder.Charges
		{
			get { return Charges; }
		}

		IChargeApportionee[] IChargeHolder.AllApportionees
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (BaseJobComInvoiceHeader invoice in AllJobComInvoiceHeaders)
				{
					result.Add(invoice);
					result.AddRange(invoice.JobComInvoiceLines);
				}
				return (IChargeApportionee[])result.ToArray(typeof(IChargeApportionee));
			}
		}

		IChargeHolder[] IChargeHolder.ImmediateChargeHolderChildren
		{
			get
			{
				ArrayList result = new ArrayList();
				result.AddRange(JobComInvoiceGroupHeaders);
				result.AddRange(JobComInvoiceHeaders);
				return (IChargeHolder[])result.ToArray(typeof(IChargeHolder));
			}
		}

		IChargeHolder IChargeHolder.ImmediateChargeHolderParent
		{
			get { return GroupHeader; }
		}

		bool IChargeHolder.IsGroupInvoice
		{
			get { return true; }
		}

		ZString IChargeHolder.GetDefaultCurrencyCode(ICustomsChargeCode chargeCode, JobComInvCharge charge)
		{
			return new GroupChargeCurrencyCalculator(this).GetDefaultCurrency(chargeCode);
		}

		ZString IChargeHolder.GetDefaultDistributeBy()
		{
			var result = ZString.Empty;
			if (JobDeclaration is BaseJobDeclaration declaration)
			{
				var companyPk = Guid.Empty;
				var branchPk = Guid.Empty;

				if (declaration.JE_GC is { IsEmpty: false, IsValid: true })
				{
					companyPk = declaration.JE_GC.ToGuid();
				}

				if (declaration.JE_GB is { IsEmpty: false, IsValid: true })
				{
					branchPk = declaration.JE_GB.ToGuid();
				}

				if (declaration.IsExport)
				{
					result = CustomsDataRegistry.Instance.InvoiceChargesForExport.GetFallBackValueAtAllLevels(companyPk, branchPk, Guid.Empty);
				}
				if (declaration.IsImport)
				{
					result = CustomsDataRegistry.Instance.InvoiceChargesForImport.GetFallBackValueAtAllLevels(companyPk, branchPk, Guid.Empty);
				}
			}
			return result;
		}

		#endregion

		#region ICommonNonApportionedChargeProvider<BaseGroupInvoiceCharge> Members

		BaseGroupInvoiceCharge ICommonNonApportionedChargeProvider<BaseGroupInvoiceCharge>.CreateNew()
		{
			return Charges.AddNew();
		}

		BaseGroupInvoiceCharge ICommonNonApportionedChargeProvider<BaseGroupInvoiceCharge>.GetChargeWithZeroAmount(ZString chargeCode)
		{
			return Charges.GetCharge(chargeCode).FirstOrDefault(x => x.J7_Amount.IsEmpty);
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZQuery GetQuery(BaseJobDeclaration declaration)
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_GroupInvoice, SQLComparisonOperator.Equal, ZBool.True);
				result.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_JE, SQLComparisonOperator.Equal, declaration.PK);
				result.FetchOnlyFromLocalCache = !declaration.IsInDatabase;
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(BaseJobComInvoiceHeader);
			}
		}

		#endregion

		#region Additional Declarations Support

		protected override IRelatedDeclarationGenPivotCollection GetNewRelatedDeclarationGenPivots()
		{
			return new GroupRelatedDeclarationGenPivotCollection(this);
		}

		protected override void OnAttachedToAdditionalDeclaration(BaseJobDeclaration declaration)
		{
			declaration.AllGroupHeaders.Load();
		}

		protected override void OnDetachedFromAdditionalDeclaration(BaseJobDeclaration declaration)
		{
			declaration.AllGroupHeaders.Load();
		}

		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => Branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region Universal Copy

		internal bool IsDefaultTopGroupHeader { get; set; }

		protected object NewForUniversalCopy(BusinessObjectFactory factory, object parentEntity)
		{
			BaseJobComInvoiceGroupHeader result = null;

			var declaration = parentEntity as BaseJobDeclaration ?? (parentEntity as BaseJobComInvoiceHeader)?.JobDeclaration;
			if (declaration != null)
			{
				var isTopGroupInvoice = JZ_JZ_GroupInvoiceFK.IsEmpty;
				if (isTopGroupInvoice && JobDeclaration.TopGroupInvoice == this)
				{
					result = declaration.JobComInvoiceGroupHeaders
						.Cast<BaseJobComInvoiceGroupHeader>()
						.FirstOrDefault(c => c.IsDefaultTopGroupHeader && c.JZ_InvoiceNumber.EqualsIgnoringCase(AllInvoices));
				}
				else if (isTopGroupInvoice && declaration.SkipDuplicateTopGroupInvoiceOnUniversalCopy)
				{
					result = declaration.TopGroupInvoice;
				}
				else
				{
					result = factory.New<BaseJobComInvoiceGroupHeader>();
					result.HasChanges = true;
					if (isTopGroupInvoice && declaration.TopGroupInvoice != null)
					{
						result.JZ_JZ_GroupInvoiceFK = declaration.TopGroupInvoice.PK;
					}
				}
			}

			return result;
		}

		#endregion

		#region Test Helpers
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			JZ_RelatedIndicator = "Y";
			if (IsDefaultTopGroupHeader)
			{
				base.JZ_InvoiceNumber = AllInvoices;
			}
		}

#endif
		#endregion
	}
}
