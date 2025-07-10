using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.Business.EventProcessors;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business
{
	[SystemDefinedValues]
	[UniversalCopyIgnoreElement("JobComInvoiceHeaders")]
	[VisualizableDocumentsSupportable("JobDeclarationZAVisualizableDocumentSupporter")]
	public class JobDeclaration : TypeSafeJobDeclaration, ITemplateCopyable
		, ILandedCostHeader
		, IMessageManageableBizObj
		, Integration.Customs.ZA.IJobDeclaration
		, ICustomsJobInfo, IDA63ValueRecalculationParent
		, ICurrencyConverterDataProvider
		, Customs.Business.Interfaces.IOnUniversalEventAddedHandler
		, IApportionInvoiceHolder
		, IZASupportingDocSendingObject
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			//would have done this based on the branch on the declaration, but since department is also valid - it makes this only possible to do based on the current context
			var customsOfficeGuid = (ZGuid)ZACustomsRegistry.Instance.CustomsOfficeCode.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()); // TODO: replace it with new registry
			var cusCodeList = customsOfficeGuid.IsEmpty ? null : Factory.Load<ZZRefCusCodeListCombined>(customsOfficeGuid);
			JE_CustomsOffice = cusCodeList != null ? cusCodeList.ZZD_Code : ZString.Empty;

			JE_PaymentMethod = PaidByCodeList.Codes.BRK;
		}

		public new class Schema : TypeSafeJobDeclaration.Schema
		{
			public const string AgentCode = "AgentCode";
			public const string CaseNumbers = "CaseNumbers";
			public const string CombinedUCREntryNumbers = "CombinedUCREntryNumbers";
			public const string CombinedReleasePrintIndicator = "CombinedReleasePrintIndicator";
			public const string EffectiveEntryStatus = "EffectiveEntryStatus";
			public const string JE_OH_AgentOverride = "JE_OH_AgentOverride";
			public const string PortDirection = "PortDirection";
			public const string SupportingDocumentStatus = "SupportingDocumentStatus";
			public new const int JE_CustomsOfficeMaxLength = 3;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategy.JobDeclarationFetchStrategy(this);
		}

		protected override bool IsEntryClearCore
		{
			get { return true; }
		}

		protected override WarehouseInvoiceLink GetNewWarehouseInvoiceLink()
		{
			return new WarehouseInvoiceLink(this);
		}

		protected override Customs.Business.BondedWarehouseTransaction GetNewBondedWarehouseTransaction()
		{
			return new BondedWarehouseTransaction(this);
		}

		protected override void WarehouseDocAddress_ValueChanged(object sender, EventArgs e)
		{
			base.WarehouseDocAddress_ValueChanged(sender, e);
			if (IsExport)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
			var warehouseOAAddressPK = WarehouseDocAddress?.E2_OA_Address ?? ZGuid.Empty;
			var originalWarehousePK = (ZGuid)(WarehouseDocAddress?.E2_OA_AddressInfo?.OriginalValue ?? ZGuid.Empty);
			if (!warehouseOAAddressPK.IsEmpty || !originalWarehousePK.IsEmpty)
			{
				foreach (CusEntryInstruction inst in CustomsEntryInstructionProvider.CustomsEntryInstructions)
				{
					if (originalWarehousePK == inst.CEI_OA_Warehouse)
					{
						inst.CEI_OA_Warehouse = warehouseOAAddressPK;
					}
					if (originalWarehousePK == inst.CEI_OA_Warehouse2)
					{
						inst.CEI_OA_Warehouse2 = warehouseOAAddressPK;
					}
				}
			}
		}

		protected override string GetAutoCreatedExBondInvoiceNumber()   // Discuss with Brett & Reza when they return from holidays. This is overriden in ZA to stop a message test from failing. Should the invoice number be sent in the message in ex-warehouse, or is that bug?
		{
			return "";
		}

		#region Overridden Object Getters

		protected override Customs.Business.MergeManager GetMergeManager()
		{
			return new MergeManager(this);
		}

		protected override ZQuery GetDiscardedMessagesFilter()
		{
			return new ZQuery(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.NotEqual, Messaging.Integration.EDIMessageSubTypeList.Codes.XmlUniversalEvent);
		}

		public override ZDateTime DateOfValuation
		{
			get
			{
				var result = ZDateTime.Empty;
				if (JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled)
				{
					result = JE_ValuationDate;
					if (!result.IsValid)
					{
						result = GetDefaultExchangeRateDate();
					}
				}
				else
				{
					if (IsExport)
					{
						result = ZDateTime.Today.AddDays(-1);
					}
					else if (IsImport)
					{
						if (JE_MasterBillIssuedDate.IsValid)
						{
							result = JE_MasterBillIssuedDate;
						}
					}
					else
					{
						result = ZDateTime.Today;
					}
				}
				return result;
			}
		}

		#endregion

		#region Overrides

		protected override ZString GetContainerModeForDeclarationCore(ZString transportMode, ZString shipmentPackingMode)
		{
			var result = ZString.Empty;
			switch (shipmentPackingMode)
			{
				case Enterprise.Core.Constants.ContainerModes.FCL:
				case Enterprise.Core.Constants.ContainerModes.LCL:
				case Enterprise.Core.Constants.ContainerModes.BuyersConsol:
					result = Core.Constants.ContainerModes.Containerised;
					break;
				case Enterprise.Core.Constants.ContainerModes.Bulk:
					result = Core.Constants.ContainerModes.Bulk;
					break;
				case Enterprise.Core.Constants.ContainerModes.Liquid:
					result = Core.Constants.ContainerModes.Liquid;
					break;
				case Enterprise.Core.Constants.ContainerModes.BreakBulk:
				case Enterprise.Core.Constants.ContainerModes.RollOnRollOff:
					result = Core.Constants.ContainerModes.BreakBulk;
					break;
			}
			return result;
		}

		protected override bool IsIntegrationWithAccountingSupported
		{
			get { return true; }
		}

		protected override JobDeclarationIAccIntegrationDataProvider GetJobDeclarationIAccIntegrationDataProvider()
		{
			return new ZAJobDeclarationIAccIntegrationDataProvider(this);
		}

		internal Func<IEnumerable<CusEntryHeader>> GetEntryHeadersForPreCreditCheck { get; set; }

		protected override IEnumerable<Customs.Business.CusEntryHeader> EntryHeadersForPreCreditCheck
		{
			get => GetEntryHeadersForPreCreditCheck == null ? base.EntryHeadersForPreCreditCheck : GetEntryHeadersForPreCreditCheck();
		}

		protected override ZBool IsReciprocalRatesCore
		{
			get { return IsReciprocalRatesConstant; }
		}

		internal static bool IsReciprocalRatesConstant
		{
			get { return false; }
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return LocalCurrencyConstantCode; }
		}

		internal static ZString LocalCurrencyConstantCode
		{
			get { return Enterprise.Core.Constants.CurrencyCodes.SouthAfrica; }
		}

		internal static RefCurrency GetLocalCurrency()
		{
			return RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, LocalCurrencyConstantCode);
		}

		[RelatedBusinessObject("AgentOverride")]
		[ResourceStringData("Enterprise.Customs.ZA.Business.JobDeclaration|JE_OH_AgentOverride", Caption = "Agent")]
		public ZGuid JE_OH_AgentOverride
		{
			get { return DeclarantAddress?.Header?.PK ?? ZGuid.Empty; }
			set
			{
				var oldValue = JE_OH_AgentOverride;
				var addressToSet = Factory.Load<OrgHeader>(value);
				JE_OA_DeclarantAddress = addressToSet?.MainAddress?.PK ?? ZGuid.Empty;
				if (!IsCopying && oldValue != JE_OH_AgentOverride)
				{
					if (!JE_OH_AgentOverride_ReadOnly)
					{
						JE_AGTCode = ZString.Empty;
					}
					ActiveEntryHeaders.Cast<CusEntryHeader>().ToList().ForEach(x => x.NeedsNewBGMReference = true);
				}
				Validation.ValidateJE_OH_AgentOverride();
				JE_OH_AgentOverrideInfo.RefreshBinding();
			}
		}

		public bool JE_OH_AgentOverride_ReadOnly => !IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected;

		public ZPropertyInfo JE_OH_AgentOverrideInfo
		{
			get { return GetZPropertyInfo(Schema.JE_OH_AgentOverride); }
		}

		public virtual OrgHeader AgentOverride
		{
			get { return Factory.Load<OrgHeader>(JE_OH_AgentOverride); }
		}

		public override ZString JE_AGTCode
		{
			get => base.JE_AGTCode;
			set
			{
				var oldValue = JE_AGTCode;
				base.JE_AGTCode = value;
				if (!IsCopying && JE_AGTCode != oldValue)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("75FFF116-25B7-487F-91FB-7501852132D0", Caption = "Document No.")]
		[ResourceStringData("C550A46D-4B0C-4CAF-97BB-B344294E3110", Caption = "Bill of Lading", IsApplicableMember = nameof(IsSea))]
		[ResourceStringData("07724F4A-B073-4BFE-81E7-03AE7A48A8F3", Caption = "Air Waybill", IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("67AA33AE-3235-4B9F-960A-122AB5210510", Caption = "Road Manifest", IsApplicableMember = nameof(IsRoad))]
		[ResourceStringData("350E67D2-3CD6-4A34-9360-7351E4894F23", Caption = "Rail Consignment Note", IsApplicableMember = nameof(IsRail))]
		[ResourceStringData("C8EDC509-9BC2-4742-9E1E-B06E8B856659", Caption = "Parcel Advice No.", IsApplicableMember = nameof(IsPost))]
		public override ZString JE_MasterBill
		{
			get { return base.JE_MasterBill; }
			set { base.JE_MasterBill = value; }
		}

		public override ZDateTime JE_MasterBillIssuedDate
		{
			get { return base.JE_MasterBillIssuedDate; }
			set
			{
				var oldValue = JE_MasterBillIssuedDate;
				base.JE_MasterBillIssuedDate = value;
				if (oldValue != JE_MasterBillIssuedDate)
				{
					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();

					if (IsImport)
					{
						if (JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled)
						{
							if (HouseBillIssuedDate.IsEmpty)
							{
								JE_ValuationDate = GetDefaultExchangeRateDate();
							}
						}
						else
						{
							UpdateInvoiceExchangeRates();
						}
					}
				}
			}
		}

		void UpdateInvoiceExchangeRates()
		{
			foreach (var invoice in Invoices)
			{
				invoice.SetExchangeRateIfNotUserEntered();
				invoice.Validation.ValidateJZ_RX_NKInvoice_Currency();
			}
			MarkApportionmentDirty();
		}

		public override ZDate JE_ValuationDate
		{
			get => base.JE_ValuationDate;
			set
			{
				var oldValue = JE_ValuationDate;
				base.JE_ValuationDate = value;
				if (oldValue != JE_ValuationDate && IsImport)
				{
					UpdateInvoiceExchangeRates();
				}
			}
		}

		protected override bool IsPackingInformationRelevantCore
		{
			get { return false; }
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement
		{
			get { return false; }
		}

		protected override bool IsCustomsLineAmendmentATotalReplacement
		{
			get { return false; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationOfGoodsCollection))]
		[RelatedBusinessObject("LocationOfGoods")]
		[MaxLength(2)]
		[ResourceStringData("Enterprise.Customs.ZA.Business.JobDeclaration|JE_LocationOfGoods", Caption = "Location Of Goods")]
		public override ZString JE_LocationOfGoods
		{
			get { return base.JE_LocationOfGoods; }
			set { base.JE_LocationOfGoods = value; }
		}

		public ZZRefCusCodeListCombined LocationOfGoods =>
			ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, JE_LocationOfGoods, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today, attributeNames: new ZString[] { RefCusCodeListAttributeTypes.Codes.DistrictOffices });

		public override ZInt JE_TotalNoOfPacks
		{
			get { return base.JE_TotalNoOfPacks; }
			set
			{
				bool hasChanges = JE_TotalNoOfPacks != value;
				base.JE_TotalNoOfPacks = value;
				if (hasChanges)
				{
					CustomsEntryHeaders.ClearPackages();
				}
			}
		}

		public override ZDecimal JE_TotalWeight
		{
			get { return base.JE_TotalWeight; }
			set
			{
				var oldValue = JE_TotalWeight;
				base.JE_TotalWeight = value;
				if (!IsCopying && oldValue != JE_TotalWeight)
				{
					CustomsEntryHeaders.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_TotalWeightUnit
		{
			get { return base.JE_TotalWeightUnit; }
			set
			{
				var oldValue = JE_TotalWeightUnit;
				base.JE_TotalWeightUnit = value;
				if (!IsCopying && oldValue != JE_TotalWeightUnit)
				{
					CustomsEntryHeaders.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_ApplicationCode
		{
			get => base.JE_ApplicationCode;
			set
			{
				var oldValue = JE_ApplicationCode;
				base.JE_ApplicationCode = value;
				if (!IsCopying && oldValue != JE_ApplicationCode)
				{
					InvoiceLines.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		public override ZGuid JE_GB
		{
			get { return base.JE_GB; }
			set
			{
				var oldValue = JE_GB;
				base.JE_GB = value;
				if (oldValue != JE_GB)
				{
					CustomsEntryHeaders.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_GC
		{
			get { return base.JE_GC; }
			set
			{
				var oldValue = JE_GC;
				base.JE_GC = value;
				if (oldValue != JE_GC)
				{
					InvoiceLines.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		public override ZString JE_RL_NKOrigin
		{
			get { return base.JE_RL_NKOrigin; }
			set
			{
				var oldValue = JE_RL_NKOrigin;
				base.JE_RL_NKOrigin = value;
				if (oldValue != JE_RL_NKOrigin)
				{
					InvoiceLines.MarkAsNeedingValidation();
					Bills.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("ZA|JobDeclaration|JE_GoodsOrigin", Caption = "Goods Origin")]
		public override ZString JE_GoodsOrigin
		{
			get { return base.JE_GoodsOrigin; }
			set
			{
				var hasChanged = value != JE_GoodsOrigin;
				if (hasChanged)
				{
					base.JE_GoodsOrigin = value;
					if (!IsCopying)
					{
						ClearInvoiceValuesIfSame(value, JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin);
						Invoices.MarkAsNeedingValidation();
						InvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid JE_OH_Importer
		{
			get { return base.JE_OH_Importer; }
			set
			{
				var hasChanged = value != JE_OH_Importer;
				if (!IsCopying && hasChanged)
				{
					base.JE_OH_Importer = value;
					Invoices.MarkAsNeedingValidation();
					if (!IsExport)
					{
						DefaultAgent();
						DefaultVATClaimBack(Importer);
					}
				}
			}
		}

		public override ZGuid JE_OH_Supplier
		{
			get { return base.JE_OH_Supplier; }
			set
			{
				var oldValue = JE_OH_Supplier;
				base.JE_OH_Supplier = value;
				if (oldValue != JE_OH_Supplier)
				{
					supplierCustomsApprovedExporterCode = null;
					CusContainers.MarkAsNeedingValidation();
					if (IsExport)
					{
						DefaultVATClaimBack(Supplier);
					}
				}
			}
		}

		public override bool ShouldCopyProcedureFromPreviousInvoiceLine => true;

		public override ZBool ContainersRequired
		{
			get { return ((!IsNonTransportDeclarationType && !IsAir && Enterprise.Core.Constants.ContainerModes.IsContainerised(JE_ContainerMode)) || ContainersAlwaysRequired) || IsRoad || IsRail; }
		}

		public override ZString JE_ContainerMode
		{
			get { return base.JE_ContainerMode; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_ContainerMode))
				{
					var oldValue = JE_ContainerMode;
					base.JE_ContainerMode = value;
					if (!IsCopying && oldValue != JE_ContainerMode)
					{
						CusContainers.MarkAsNeedingValidation();
						foreach (CusContainer cusContainer in CusContainers)
						{
							cusContainer.Validation.ValidateCO_FCL_LCL_AIR();
						}
					}
				}
			}
		}

		public override ZString JE_VesselName
		{
			get { return base.JE_VesselName; }
			set
			{
				var oldValue = JE_VesselName;
				base.JE_VesselName = value;
				if (!IsCopying && oldValue != JE_VesselName)
				{
					UpdateRadioCallSignFromSystemDataFirst();
					UpdateCarrierCodeFromSystemDataFirst();
				}
			}
		}

		void UpdateRadioCallSignFromSystemDataFirst()
		{
			if (JE_VesselName.IsEmpty)
			{
				JE_RadioCallSign = ZString.Empty;
			}
			else
			{
				var vessel = Vessel;
				if (vessel != null)
				{
					if (vessel.HasSystemVessel)
					{
						var relatedSystemVessel = vessel.RelatedSystemVessels;
						var systemVessel = vessel.SystemVessel;
						if (relatedSystemVessel != null && relatedSystemVessel.Length < 2)
						{
							if (systemVessel != null && !systemVessel.ZZO_RadioCallSign.IsEmpty)
							{
								JE_RadioCallSign = systemVessel.ZZO_RadioCallSign;
							}
							else
							{
								JE_RadioCallSign = vessel.RV_RadioCallSign;
							}
						}
					}
					else
					{
						JE_RadioCallSign = vessel.RV_RadioCallSign;
					}
				}
			}
		}

		void UpdateCarrierCodeFromSystemDataFirst()
		{
			var carrierCode = ZString.Empty;

			var vessel = Vessel;
			if (vessel != null)
			{
				if (vessel.HasSystemVessel)
				{
					var systemVessel = vessel.SystemVessel;
					if (systemVessel != null)
					{
						var pivot = Factory.LoadTop1<RefCarrierVesselPivot>(new ZQuery(RefCarrierVesselPivotSchema.ZZQ_ZZO, systemVessel.PK));
						carrierCode = pivot?.Carrier?.ZZ4_Code.Left(4) ?? vessel.RV_CarrierCode;
					}
					else
					{
						carrierCode = vessel.RV_CarrierCode;
					}
				}
				else
				{
					carrierCode = vessel.RV_CarrierCode;
				}
			}

			JE_Carrier = carrierCode;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.RadioCallSignVessels))]
		public override ZString JE_RadioCallSign
		{
			get { return base.JE_RadioCallSign; }
			set { base.JE_RadioCallSign = value; }
		}

		protected override void OnFactorySaving()
		{
			if (!IsAir && !IsSea && !IsRoad)
			{
				JE_VoyageFlightNo = ZString.Empty;
			}
			DeriveDeclarationStatus();
			base.OnFactorySaving();
		}

		protected override bool DoMergeCore(ISendsMessagesToCustoms notifier)
		{
			CusEntryInstruction.ResetDefaultAssessmentDate(Factory);
			RefreshExRateToLatestRateAvailableIfNeeded();
			return base.DoMergeCore(notifier);
		}

		protected override void AfterExRatesRefreshed(IEnumerable<ICurrencyProvider> currencyProviders)
		{
			base.AfterExRatesRefreshed(currencyProviders);
			foreach (ICurrencyProvider currencyProvider in currencyProviders)
			{
				currencyProvider.ValidateCurrencyCode();
			}
		}

		#region Lookups

		protected override bool IsLookupsCachedInBase
		{
			get { return false; }
		}

		protected override Customs.Business.JobDeclarationLookups GetNewLookups()
		{
			JobDeclarationLookups result;

			if (IsExport)
			{
				result = ExportLookups;
			}
			else
			{
				result = ImportLookups;
			}

			return result;
		}

		ExportJobDeclarationLookups ExportLookups
		{
			get
			{
				if (fExportLookups == null)
				{
					fExportLookups = new ExportJobDeclarationLookups(this);
				}

				return fExportLookups;
			}
		}

		ImportJobDeclarationLookups ImportLookups
		{
			get
			{
				if (fImportLookups == null)
				{
					fImportLookups = new ImportJobDeclarationLookups(this);
				}

				return fImportLookups;
			}
		}

		ExportJobDeclarationLookups fExportLookups;
		ImportJobDeclarationLookups fImportLookups;

		#endregion

		public override ZBool BondedWarehouseEditable
		{
			get { return base.BondedWarehouseEditable || IsExport; }
		}

		public override ZBool IsImport
		{
			get { return (ZBool)(base.IsImport || IsExWarehouse); }
		}

		public override ZBool IsExWarehouse
		{
			get { return JE_MessageType == ZAJobMessageTypeList.Codes.ExBond; }
		}

		public override bool IsImportByExternalBroker => JE_MessageType == ZAJobMessageTypeList.Codes.ImportByExternalBroker;

		public override ZString JE_MessageType
		{
			get { return base.JE_MessageType; }
			set
			{
				var hasChanged = value != JE_MessageType;
				if (hasChanged)
				{
					base.JE_MessageType = value;
					if (!IsCopying)
					{
						RefreshIncotermAndChargeFactory();
						DefaultAgent();
						if (JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled)
						{
							JE_ValuationDate = GetDefaultExchangeRateDate();
						}
						RefreshInvoiceLineMaxCountValidation();
						RefreshInvoiceLineInfo();
						if (JE_MessageType == ZAJobMessageTypeList.Codes.ExBond)
						{
							foreach (var invoice in Invoices)
							{
								invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
							}
						}
					}
				}
			}
		}

		[MaxLength(Schema.JE_CustomsOfficeMaxLength)]
		[ResourceStringData("Enterprise.Customs.ZA.Business.JobDeclaration|JE_CustomsOffice", Caption = "Customs Office", ShortCaption = "Office")]
		public override ZString JE_CustomsOffice
		{
			get { return base.JE_CustomsOffice; }
			set
			{
				var oldValue = JE_CustomsOffice;
				base.JE_CustomsOffice = value.Length > Schema.JE_CustomsOfficeMaxLength ? value.Left(Schema.JE_CustomsOfficeMaxLength) : value;
				if (!IsCopying && oldValue != JE_CustomsOffice)
				{
					if (!IsExport)
					{
						DefaultAgent();
					}
					ActiveEntryHeaders.Cast<CusEntryHeader>().ToList().ForEach(x => x.NeedsNewBGMReference = true);
					ActiveEntryHeaders.MarkAsNeedingValidation();
				}
			}
		}

		public bool JE_CustomsOffice_ReadOnly => !IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected;

		public override ZGuid JE_OA_DeclarantAddress
		{
			get { return base.JE_OA_DeclarantAddress; }
			set
			{
				var oldValue = JE_OA_DeclarantAddress;
				base.JE_OA_DeclarantAddress = value;
				if (!IsCopying && oldValue != JE_OA_DeclarantAddress)
				{
					ActiveEntryHeaders.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.JobDeclaration|JE_CarrierCode", Caption = "Master Cargo Carrier")]
		public override ZString JE_CarrierCode
		{
			get { return base.JE_CarrierCode; }
			set { base.JE_CarrierCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CargoCarrierCodeList))]
		public override ZString JE_CargoCarrier
		{
			get { return base.JE_CargoCarrier; }
			set
			{
				var oldValue = JE_CargoCarrier;
				base.JE_CargoCarrier = value;
				if (oldValue != JE_CargoCarrier)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CarrierCodeList))]
		public override ZString JE_Carrier
		{
			get { return base.JE_Carrier; }
			set
			{
				var oldValue = JE_Carrier;
				base.JE_Carrier = value;
				if (!IsCopying && JE_Carrier != oldValue)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool JE_IsNonIATAFormatAirWayBill
		{
			get { return base.JE_IsNonIATAFormatAirWayBill; }
			set
			{
				var oldValue = JE_IsNonIATAFormatAirWayBill;
				base.JE_IsNonIATAFormatAirWayBill = value;
				if (!IsCopying && JE_IsNonIATAFormatAirWayBill != oldValue)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_RL_NKFinalDestination
		{
			get { return base.JE_RL_NKFinalDestination; }
			set
			{
				var oldValue = JE_RL_NKFinalDestination;
				base.JE_RL_NKFinalDestination = value;
				if (!IsCopying && oldValue != JE_RL_NKFinalDestination)
				{
					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
					Bills.MarkAsNeedingValidation();
					Invoices.ForEach(x => ((JobComInvoiceHeader)x).DefaultRooCertWithApprovedExporter());
				}
			}
		}

		public override ZString JE_ExportGoodsType
		{
			get { return base.JE_ExportGoodsType; }
			set
			{
				var oldValue = JE_ExportGoodsType;
				base.JE_ExportGoodsType = value;
				if (oldValue != JE_ExportGoodsType)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JE_MergeBy
		{
			get => base.JE_MergeBy;
			set
			{
				base.JE_MergeBy = value;
				InvoiceLines.MarkAsNeedingValidation();
			}
		}

		protected override void DefaultValuesFromLocalPartyWhenEnteredCore(OrgHeader party)
		{
			base.DefaultValuesFromLocalPartyWhenEnteredCore(party);
			DefaultJE_PaymentMethodFromLocalParty(party);
		}

		protected void DefaultJE_PaymentMethodFromLocalParty(OrgHeader localParty)
		{
			var miscServ = localParty.MiscServ;
			if (miscServ != null && !miscServ.OM_IMPaymentMethod.IsEmpty && Lookups.PaymentPartyList.ContainsCode(miscServ.OM_IMPaymentMethod))
			{
				JE_PaymentMethod = miscServ.OM_IMPaymentMethod;
			}
		}

		public override bool IsNonTransportDeclarationType
		{
			get { return IsExWarehouse; }
		}

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		protected override ZBool IsBillIssueDateVisibleCore
		{
			get { return true; }
		}

		protected override INotificationType ContainerNotLinkedSeverityCore
		{
			get { return HasMultiCustomsEntryInstructions ? CargoWise.EntityFramework.NotificationType.MessageError : base.ContainerNotLinkedSeverityCore; }
		}

		public ZBool HasMultiCustomsEntryInstructions
		{
			get { return CustomsEntryInstructionProvider.CustomsEntryInstructions.Count > 1; }
		}

		#endregion

		#region New Properties

		public bool IsMergeByValidForPreviousProcedureCode
		{
			get
			{
				var mergeByValidForPreviousProcedureCode = true;

				if (IsAnyInvoiceLinePreviousProcedureCodeNot00
						&& JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge
						&& JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription)
				{
					mergeByValidForPreviousProcedureCode = false;
				}

				return mergeByValidForPreviousProcedureCode;
			}
		}

		bool IsAnyInvoiceLinePreviousProcedureCodeNot00 => Factory.GetValue(ref _isAnyInvoiceLinePreviousProcedureCodeNot00Cached, delegate
		{
			return InvoiceLines?.Cast<JobComInvoiceLine>().Any(x => x.JI_Calc_PreviousProcedure != string.Empty && x.JI_Calc_PreviousProcedure != UniversalReferenceConstants.ProcedureCodes._00) ?? false;
		});

		CachedProperty<bool> _isAnyInvoiceLinePreviousProcedureCodeNot00Cached;

		public ZBool HasEntryLineNumberExceedingMax => Factory.GetValue(ref hasEntryLineNumberExceedingMaxCached, delegate
					{
						var result = ZBool.False;
						foreach (CusEntryHeader entryHeader in ActiveEntryHeaders)
						{
							foreach (CusEntryLine entryLine in entryHeader.MergedLines)
							{
								if (entryLine.CL_LineNumber > 9999)
								{
									return ZBool.True;
								}
							}
						}
						return result;
					});

		CachedProperty<ZBool> hasEntryLineNumberExceedingMaxCached;

		public ZString GetPreviousEntryLineNumbersMinMaxValueMessageError()
		{
			var strBuilder = new ZStringBuilder();
			foreach (JobComInvoiceHeader invoiceHeader in Invoices)
			{
				invoiceHeader.InvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo);
				foreach (JobComInvoiceLine invoiceLine in invoiceHeader.InvoiceLines)
				{
					var lineNumber = invoiceLine.JI_PreviousEntryLineNumber;
					if (lineNumber < 0 || lineNumber > 9999)
					{
						strBuilder.Append("\r\nInvoice: " + invoiceHeader.JZ_InvoiceNumber + " Line: " + invoiceLine.JI_LineNo);
					}
				}
			}
			if (!strBuilder.IsEmpty)
			{
				strBuilder.Prepend(ValidationConstants.Shared.MessageCannotBeSent + "\r\n\r\n" + ValidationConstants.InvoiceLine.PreviousMRNLineNumberMinMaxValue + " on");
			}
			return strBuilder.ToString();
		}

		#region ROO Certificate

		[ResourceStringData("Enterprise.Customs.ZA.Business.JobDeclaration|JE_ROOCert", Caption = "Rules Of Origin Certificate", ShortCaption = "ROO Certificate")]
		public override ZString JE_ROOCert
		{
			get { return base.JE_ROOCert; }
			set
			{
				var oldValue = base.JE_ROOCert;
				base.JE_ROOCert = value;
				if (!IsCopying && oldValue != JE_ROOCert)
				{
					ClearInvoiceValuesIfSame(JE_ROOCert, JobComInvoiceHeader.Schema.JZ_ROOCert);
					Invoices.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		#endregion

		#region ROOType

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ROOTypesList))]
		[MaxLength(3)]
		public override ZString JE_ROOType
		{
			get { return base.JE_ROOType; }
			set
			{
				var oldValue = JE_ROOType;
				base.JE_ROOType = value;
				if (!IsCopying && oldValue != JE_ROOType)
				{
					ClearInvoiceValuesIfSame(value, JobComInvoiceHeader.Schema.JZ_ROOType);
					Invoices.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.RemovalTransportCodeList))]
		public override ZString JE_RemovalTransportCode
		{
			get { return base.JE_RemovalTransportCode; }
			set
			{
				var oldValue = JE_RemovalTransportCode;
				base.JE_RemovalTransportCode = value;
				if (!IsCopying && oldValue != JE_RemovalTransportCode)
				{
					MarkAsNeedingValidation();
					Bills.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.VATClaimBackIndicator))]
		public override ZString JE_VATClaimBackIndicator
		{
			get => base.JE_VATClaimBackIndicator;
			set => base.JE_VATClaimBackIndicator = value;
		}

		[ResourceStringData("ZA|JobDeclaration|JE_VesselAgent", Caption = "Vessel Agent")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.VesselAgentList))]
		[MaxLength(4)]
		public override ZString JE_VesselAgent
		{
			get => base.JE_VesselAgent;
			set => base.JE_VesselAgent = value;
		}

		[ResourceStringData("FD2DB373-D68E-4C77-8739-DD4615E71495", Caption = "Estimated Arrival Date", IsApplicableMember = nameof(IsExBondAndAutomaticDeferred))]
		public override ZDateTime JE_DateOfArrival { get => base.JE_DateOfArrival; set => base.JE_DateOfArrival = value; }

		#region Agent Related Information

		#region AgentCode

		[ResourceStringData("Enterprise.Customs.ZA.Business.JobDeclaration|AgentCode", Caption = "Agent Code")]
		[MaxLength(35)]
		[ReadOnly(true)]
		public ZString AgentCode
		{
			get
			{
				var result = base.JE_AGTCode;
				if (result.IsEmpty)
				{
					result = FullAgentCode;
				}
				if (result.Contains("/", StringComparison.CurrentCulture))
				{
					result = result.Left(result.IndexOf("/", StringComparison.CurrentCulture));
				}
				return result;
			}
			set
			{
				CheckMaximumLength(AgentCodeInfo, value);
				base.JE_AGTCode = value;
				AgentCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AgentCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AgentCode); }
		}

		#endregion

		public ZString SupplierCustomsApprovedExporterCode
		{
			get { return supplierCustomsApprovedExporterCode ?? (supplierCustomsApprovedExporterCode = Supplier?.CustomsCodes?.GetOrgCusCode(OrgCusCode.SouthAfricaCodeTypes.CustomsApprovedExporter, Country)?.OK_CustomsRegNo ?? ZString.Empty).Value; }
		}
		ZString? supplierCustomsApprovedExporterCode;

		public ZBool ShouldDefaultRooCertForApprovedExporter(ZString supplierCustomsApprovedExporterCode)
		{
			return IsExport
					&& !supplierCustomsApprovedExporterCode.IsEmpty
					&& HasTradeAgreement(FinalDestinationCountryCode);
		}

		bool HasTradeAgreement(ZString finalDestinationCountryCode)
		{
			var query = new ZDBOnlyQuery(typeof(RefCusTradeGroup));
			query.AddToFilter(RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
			query.AddToFilter(RefCusTradeGroupSchema.ZZA_TradeGroup, SQLComparisonOperator.NotEqual, TradeAgreement.Standard);
			query.AddToFilter(RefCusTradeGroupSchema.ZZA_TradeGroup, SQLComparisonOperator.NotEqual, finalDestinationCountryCode);

			var subQuery = new ZDBOnlySubQuery(typeof(RefCusTradeGroupCountry), RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup);
			subQuery.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode, finalDestinationCountryCode);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return Factory.LoadTop1<RefCusTradeGroup>(query) != null;
		}

		public ZString AgentDualProfileCode
		{
			get
			{
				return string.IsNullOrEmpty(JE_AGTCode) ? AgentOverride?.CustomsCodes.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty
															: OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.AgentCode, AgentCode, CountryCode)?.CustomsCodes.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;
			}
		}

		public ZString TradingPartyID => AgentCode;

		public ZString AgentName
		{
			get
			{
				var agent = AgentOverride;
				return agent != null ? agent.OH_FullNameTruncated : ZString.Empty;
			}
		}

		ZString FullAgentCode
		{
			get
			{
				var effectiveBranch = Branch ?? GlbBranch.CurrentBranch;
				var agent = AgentOverride;
				return agent != null ? agent.GetAgentCode(effectiveBranch.Company.Country) : ZString.Empty;
			}
		}

		#endregion

		public ZString ExitEntryType;
		public ZDateTime ActualDateOfArrival;
		public ZBool IsCCA;

		internal bool DoAgentHaveAValue => !AgentCode.IsEmpty;

		public ZString LocalReferenceNumber(string ediNumber, ZString customsOffice)
		{
			var strBuilder = new StringBuilder();
			if (DoAgentHaveAValue && !customsOffice.IsEmpty)
			{
				strBuilder.Append(AgentCode);
				strBuilder.Append(customsOffice);
				strBuilder.Append(ZDateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
				strBuilder.Append(ediNumber);
			}
			return strBuilder.ToString();
		}

		public bool IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected => ActiveEntryHeaders.Cast<CusEntryHeader>().All(x => x.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected);

		CachedProperty<OrgHeader[]> fSuppliersRequiringROOCertificate;
		public OrgHeader[] SuppliersRequiringROOCertificate => Factory.GetValue(ref fSuppliersRequiringROOCertificate, new SupplierROOCalculator(Invoices).Execute);

		[BusinessObjectTestExclude]
		public override ZString JE_RL_NKPortOfFirstArrival
		{
			get { return ZString.Empty; }
			set { } // Do Nothing
		}

		[BusinessObjectTestExclude]
		public override ZDateTime JE_DateOfFirstArrival
		{
			get { return ZDateTime.Empty; }
			set { } // Do Nothing
		}

		public ZString TransportDocumentNumber
		{
			get
			{
				if (IsSea && !IsExWarehouse)
				{
					return TDTSegmentSplitHelper.IsTDTSegementSplitApplicable(this)
						? JE_MasterBill
						: new ZString(JE_CarrierCode.PadRight(4) + JE_MasterBill);
				}
				else if (IsAir)
				{
					return JE_MasterBill.InsertSafe(3, "-");
				}
				else
				{
					return JE_MasterBill;
				}
			}
		}

		public bool CanPrintDA65
		{
			get
			{
				foreach (CusEntryInstruction intruction in CustomsEntryInstructions)
				{
					if (intruction.CEI_Style == UniversalReferenceConstants.ProcedureCodes._75 || intruction.CEI_Style == UniversalReferenceConstants.ProcedureCodes._76)
					{
						return true;
					}
				}

				return false;
			}
		}

		public RefUNLOCOCollection JE_RL_NKMasterBillIssuedAtList
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public ZDateTime EffectiveSubmittedDate
		{
			get { return JE_EntrySubmittedDate.IsEmpty ? ZDateTime.Now : JE_EntrySubmittedDate; }
		}

		public FinancialAccountNumberPortMapCollection FinancialAccountNumberPortMappings
		{
			get { return financialAccountNumberPortMappings ?? (financialAccountNumberPortMappings = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty)); }
		}
		FinancialAccountNumberPortMapCollection financialAccountNumberPortMappings;

		internal List<CusEntryHeader> ClearanceParts => Factory.GetValue(ref clearancePartsCached, () =>
					{
						var result = new List<CusEntryHeader>();
						if (ActiveEntryHeaders.Count > 0)
						{
							var activeEntryHeaders = ActiveEntryHeaders.Cast<CusEntryHeader>();
							var replacedMRNNumbers = activeEntryHeaders.Select(x => x.EntryInstruction?.CEI_MRNToBeReplaced ?? ZString.Empty).Where(x => !x.IsEmpty);
							result = activeEntryHeaders.Where(x => !replacedMRNNumbers.Contains(x.MovementReferenceNumber)).OrderBy(x => x.CH_BGMReference).ToList();
						}
						return result;
					});

		CachedProperty<List<CusEntryHeader>> clearancePartsCached;

		public ZString SupportingDocumentStatus
		{
			get
			{
				var result = ZString.Empty;
				var caseNumbers = CustomsEntryInstructions.Cast<CusEntryInstruction>().SelectMany(x => x.CaseNumbers.Cast<CaseNumber>());
				var defaultCaseNumber = caseNumbers.FirstOrDefault(x => x.Document_Status != ZString.Empty);

				if (defaultCaseNumber != null)
				{
					if (!caseNumbers.Any(x => x.Document_Status != defaultCaseNumber.Document_Status))
					{
						result = defaultCaseNumber.Document_Status;
					}
					else if (caseNumbers.Any(x => x.Document_Status == DocumentStatusCodes.Codes.PND))
					{
						result = DocumentStatusCodes.Codes.PND;
					}
					else if (caseNumbers.Any(x => x.Document_Status == DocumentStatusCodes.Codes.FAL))
					{
						result = DocumentStatusCodes.Codes.FAL;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo SupportingDocumentStatusInfo
		{
			get { return GetZPropertyInfo(Schema.SupportingDocumentStatus); }
		}

		public ZString CaseNumbers
		{
			get
			{
				var result = new ZStringBuilder();
				var caseNumbers = CustomsEntryInstructions.Cast<CusEntryInstruction>().SelectMany(x => x.CaseNumbers.Cast<CaseNumber>());
				foreach (var caseNumber in caseNumbers)
				{
					if (!caseNumber.CY_Data.IsEmpty)
					{
						result.Append(caseNumber.CY_Data);
					}
				}
				return result.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public ZPropertyInfo CaseNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.CaseNumbers); }
		}

		public ZString CombinedUCREntryNumbers => Factory.GetValue(ref combinedUCREntryNumbers, delegate
					{
						var ucrEntryNumbers = new List<ZString>();
						var entryNumbers = Factory.Load<CusEntryNumber>(EntryNumberQueryGenerator.GetEntryNumberByEntryTypeQuery(PK, CountryCode, CusEntryNumberTypes.Standard.UniqueConsignementReference, IsInDatabase));
						entryNumbers.ForEach(x => ucrEntryNumbers.Add(x.CE_EntryNum));

						foreach (CusEntryHeader header in CustomsEntryHeaders)
						{
							foreach (var entryNumberUCR in header.UCREntryNumbers)
							{
								if (!ucrEntryNumbers.Contains(entryNumberUCR))
								{
									ucrEntryNumbers.Add(entryNumberUCR);
								}
							}
						}
						return ZString.Join(", ", ucrEntryNumbers.ToArray());
					});

		CachedProperty<ZString> combinedUCREntryNumbers;

		public ZPropertyInfo CombinedUCREntryNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.CombinedUCREntryNumbers); }
		}

		#endregion

		#region CombinedReleasePrintIndicator
		CachedProperty<ZString> combinedReleasePrintIndicator;

		public ZString CombinedReleasePrintIndicator => Factory.GetValue(ref combinedReleasePrintIndicator, delegate
					{
						var indicators = CustomsEntryHeaders.Select(x => x.CH_RelPrintInd).Distinct().Take(2).ToArray();
						switch (indicators.Length)
						{
							case 2:
								return "MULTI";
							case 1:
								return indicators[0];
							default:
								return ZString.Empty;
						}
					});

		public ZPropertyInfo CombinedReleasePrintIndicatorsInfo
		{
			get { return GetZPropertyInfo(Schema.CombinedReleasePrintIndicator); }
		}
		#endregion

		#region Bonded Warehouse

		protected override Customs.Business.BondedWarehousingHelper GetNewBondedWarehousingHelper()
		{
			return new BondedWarehousingHelper(this);
		}

		public bool HasInwardEntryInstruction
		{
			get { return CustomsEntryInstructions.OfType<CusEntryInstruction>().Any(x => x.IsIntoWarehouseWarehousing); }
		}

		public bool HasOutwardEntryInstruction
		{
			get { return CustomsEntryInstructions.OfType<CusEntryInstruction>().Any(x => x.IsOutOfWarehouseWarehousing); }
		}

		protected override bool SupportsBondedWarehousingCore
		{
			get { return false; }
		}

		protected override bool IsInwardBondedWarehousingEnabledCore
		{
			get { return IsWHSUniversalXMLActive && HasInwardEntryInstruction; }
		}

		protected override bool IsOutwardBondedWarehousingEnabledCore
		{
			get { return IsWHSUniversalXMLActive && HasOutwardEntryInstruction; }
		}

		protected override bool ShouldUpdateOutwardLinesWithInventoryDetailsCore
		{
			get { return IsInventorySelectionEnabled; }
		}

		protected override bool IsInventorySelectionEnabledCore
		{
			get { return IsExWarehouse || IsExport; }
		}

		protected override Customs.Business.DeclarationInventorySelectionHeader GetNewInventorySelectionHeader()
		{
			return new DeclarationInventorySelectionHeader(this);
		}

		protected override bool SupportMultipleWarehouseEntryCore
		{
			get { return true; }
		}

		#endregion

		#region IDocumentSupport Members
		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobDeclarationDocumentSupporter(this);
		}
		#endregion

		#region New Methods
		#endregion

		#region Messaging

		public void SetEntrySubmittedDate(ZDateTime date)
		{
			PopulateEntrySubmittedDate(date);
		}

		#endregion

		#region PortDirection
		[MaxLength(20)]
		public ZString PortDirection
		{
			get
			{
				bool shouldShowPortOfExit = IsExport;
				if (!shouldShowPortOfExit && !JE_RL_NKFinalDestination.IsEmpty)
				{
					var finalDestination = FinalDestination?.Country;
					shouldShowPortOfExit = (finalDestination == null ||
						(finalDestination.RN_Code != Core.Constants.CountryCodes.SouthAfrica &&
						!finalDestination.IsBLNS));
				}
				return shouldShowPortOfExit ? "Port of Exit" : "Port of Destination";
			}
		}

		public ZPropertyInfo PortDirectionInfo
		{
			get { return GetZPropertyInfo(Schema.PortDirection); }
		}

		#endregion

		#region ILandedCostHeader Members

		DutyTaxEntryFee ILandedCostHeader.TotalDutyTaxEntryFeeItems => Factory.GetValue(ref totalDutyTaxEntryFeeItemsCached, GetTotalDutyTaxEntryFeeItems);

		CachedProperty<DutyTaxEntryFee> totalDutyTaxEntryFeeItemsCached;

		// TODO: Will be replaced with GenericLandedCostingConfig
		DutyTaxEntryFee GetTotalDutyTaxEntryFeeItems()
		{
			var result = new DutyTaxEntryFee();
			foreach (CusEntryHeader entryHeader in ActiveEntryHeaders)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					result[CustomsDisbursementChargeCode.TotalDuty] += entryLine.GetDutyAmountForLandedCosting();
					result[CustomsDisbursementChargeCode.SpecialTax1] += entryLine.GetDutySch1P2B(null);
				}
			}
			return result;
		}

		ZBool ILandedCostHeader.SupportsNoCostApportionmentItem => true;

		#endregion

		#region IDocAddresses Members

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new DeclarationJobDocAddressValidation(addressToValidate, this);
		}

		#endregion

		JobDeclaration[] GetRelatedImportJobDeclarations()
		{
			Hashtable decs = new Hashtable();
			if (decs.Values.Count > 0)
			{
				JobDeclaration[] result = new JobDeclaration[decs.Values.Count];
				decs.Values.CopyTo(result, 0);
				return result;
			}
			else
			{
				return Array.Empty<JobDeclaration>();
			}
		}

		#region IDocManagerSupport Members
		protected override DeclarationDocManagerInfo GetNewDocManagerInfo()
		{
			return new ZADeclarationDocManagerInfo(this);
		}

		internal class ZADeclarationDocManagerInfo : DeclarationDocManagerInfo
		{
			public ZADeclarationDocManagerInfo(JobDeclaration parent)
				: base(parent)
			{
			}

			protected override BusinessObject[] GetRelatedObjects()
			{
				ArrayList businessObjects = new ArrayList();
				businessObjects.AddRange(base.GetRelatedObjects());

				JobDeclaration declaration = (JobDeclaration)BusinessEntity;
				BusinessObject[] relatedDecs = declaration.GetRelatedImportJobDeclarations();
				if (relatedDecs.Length > 0)
				{
					businessObjects.AddRange(relatedDecs);
				}

				return (BusinessObject[])businessObjects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region HouseBills

		[ChildEditable(true)]
		public new BillCollection<Bill, JobDeclaration> Bills => (BillCollection<Bill, JobDeclaration>)base.Bills;

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection<Bill, JobDeclaration>(this, Factory);

		#endregion

		[ResourceStringData("C77A0193-BC97-497C-AF66-AA24A1481DE7", Caption = "Vehicle Reg No", IsApplicableMember = nameof(IsRoad))]
		public override ZString JE_VoyageFlightNo { get => base.JE_VoyageFlightNo; set => base.JE_VoyageFlightNo = value; }

		#region IMessageManageableBizObj Members
		//ZA does not have messaging capabilities. This is an expensive operation. When put back in, please consider full profile. CS00184125
		//Currenty, AmendmentDetection functionality is not implemented, the interface is just for MessageManger Compliance #Victor 20160422

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable => false;

		#endregion

		#region ICustomsJobInfo members

		ZGuid ICustomsJobInfo.CreditorPK
		{
			get
			{
				var result = ZGuid.Empty;

				if (IsDeclarationIntegrated)
				{
					var creditors = DataRegistry.Business.ZACustomsRegistry.Instance.DSBCreditors.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty);
					result = creditors.GetCreditorFor(JE_CustomsOffice);
				}
				else
				{
					var agentOrgHeader = OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.AgentCode, AgentCode, CountryCode);
					if (agentOrgHeader != null)
					{
						result = FinancialAccountNumberPortMappings.GetCreditorFor(agentOrgHeader.PK, JE_CustomsOffice);
					}
				}

				if (result.IsEmpty)
				{
					result = Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty);
				}

				return result;
			}
		}

		#endregion

		public override ZString EntryDetailsInARInvoice
		{
			get
			{
				ZStringBuilder mrn = new ZStringBuilder();
				ZStringBuilder lrn = new ZStringBuilder();
				ZStringBuilder icn_ucr = new ZStringBuilder();

				foreach (var entry in ActiveEntryHeaders)
				{
					var entryZA = (CusEntryHeader)entry;
					mrn.AppendIfNotEmpty(entryZA.MovementReferenceNumber);
					lrn.AppendIfNotEmpty(entryZA.CH_BGMReference);

					if (IsImport)
					{
						icn_ucr.AppendIfNotEmpty(entryZA.ImportControlNumber);
					}
					else
					{
						icn_ucr.AppendIfNotEmpty(entryZA.UniqueConsignmentReference);
					}
				}

				return "<ExpandToFit><B>MRN:</B> " + mrn.ToStringWithDelimiterBetweenAppends(", ") + "<BR><B>LRN:</B> " + lrn.ToStringWithDelimiterBetweenAppends(", ") + "<BR><B>" + (IsImport ? "ICN" : "UCR") + ":</B> " + icn_ucr.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		#region GetTemplateCopyStrategy
		protected override JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new ZAJobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		#endregion

		protected override void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration declaration, CloneType cloneType)
		{
			base.ResetValuesOnTemplateCopyAfterClone(declaration, cloneType);

			var jobDeclaration = declaration as JobDeclaration;
			jobDeclaration.JE_BOESightNumber = "";
			foreach (JobComInvoiceGroupHeader groupInvoice in declaration.TopGroupInvoice)
			{
				ResetValuesOnGroupInvoiceForTemplateCopy(groupInvoice);
			}

			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				ResetValuesOnInvoiceForTemplateCopy(invoice);
			}

			foreach (JobComInvoiceLine jobComInvoiceLine in declaration.InvoiceLines)
			{
				ResetValuesOnJobComInvoiceLinesForTemplateCopy(jobComInvoiceLine);
			}
		}

		void ResetValuesOnJobComInvoiceLinesForTemplateCopy(JobComInvoiceLine jobComInvoiceLine)
		{
			using (jobComInvoiceLine.GetValidationSuspender())
			{
				if (jobComInvoiceLine.JI_TargetEntryLineNumber != 0)
				{
					jobComInvoiceLine.JI_TargetEntryLineNumber = 0;
				}
			}
		}

		void ResetValuesOnInvoiceForTemplateCopy(JobComInvoiceHeader invoice)
		{
			using (invoice.GetValidationSuspender())
			using (invoice.SuspendSettingHasChanges())
			{
				invoice.JZ_InvoiceDate = ZDateTime.Empty;
			}
			invoice.HasChanges = false;
		}

		void ResetValuesOnGroupInvoiceForTemplateCopy(JobComInvoiceGroupHeader groupInvoice)
		{
			using (groupInvoice.GetValidationSuspender())
			{
				groupInvoice.JZ_InvoiceDate = ZDateTime.Empty;
			}
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
		public new CusEntryInstructionCollection CustomsEntryInstructions => (CusEntryInstructionCollection)base.CustomsEntryInstructions;

		#region Implementation
		protected override bool ClientIsInventoryManagementOnCore => ClientIsBondedWarehousing;
		protected override bool IsMessageStatusAwaiting(string status) => ZAMessageStatusList.IsAwaiting(status);
		protected override bool IsMessageStatusError(string status) => ZAMessageStatusList.IsError(status);

		string IApportionInvoiceHolder.CountryContext
		{
			get { return CountryCode + this.GetIncoTermChargeFactoryCacheKey(); }
		}

		protected override bool SupportDeclarationRefs
		{
			get { return true; }
		}

		protected delegate bool ShouldClearInvoiceValue(JobComInvoiceHeader h, IZType invoiceValue);

		void ClearInvoiceValuesIfSame(IZType decValue, string invoiceFieldName)
		{
			ClearInvoiceValuesIfSame(decValue, invoiceFieldName, (JobComInvoiceHeader h, IZType invoiceValue) =>
			{ return invoiceValue.Equals(decValue); });
		}

		void ClearInvoiceValuesIfSame(IZType decValue, string invoiceFieldName, ShouldClearInvoiceValue shouldClearInvoiceValue)
		{
			if (IsPersistent)
			{
				foreach (JobComInvoiceHeader invoice in Invoices)
				{
					IZType invoiceValue = (IZType)invoice[invoiceFieldName];

					if (shouldClearInvoiceValue(invoice, invoiceValue))
					{
						using (invoice.SuspendEffectiveValue(invoiceFieldName, decValue))
						{
							invoice[invoiceFieldName] = invoiceValue.Default;
						}

						ZPropertyInfo infoToRefresh = invoice.ZPropertyInfoHash[invoiceFieldName];
						if (infoToRefresh != null)
						{
							infoToRefresh.RefreshBinding();
						}
					}
				}
			}
		}

		void DefaultAgent()
		{
			ZGuid? agentPK = null;
			var importerPK = JE_OH_Importer;
			var customsOffice = JE_CustomsOffice;
			if (!IsExport && importerPK.IsValid && !customsOffice.IsEmpty)
			{
				if (!FinancialAccountNumberPortMappings.GetFinancialAccountNumberFor(importerPK, customsOffice).IsEmpty)
				{
					agentPK = importerPK;
				}
			}

			if (!agentPK.HasValue)
			{
				agentPK = ZGuid.Empty;
				var company = Company;
				if (company != null)
				{
					agentPK = company.GC_OH_OrgProxy;
				}
			}
			JE_OH_AgentOverride = agentPK.Value;
			AgentCodeInfo.RefreshBinding();
			RefreshAgentDefaulting();
		}

		void RefreshAgentDefaulting()
		{
			if (IsExport)
			{
				if (Supplier != null)
				{
					DefaultVATClaimBack(Supplier);
				}
			}
			else if (IsImport)
			{
				if (Importer != null)
				{
					DefaultVATClaimBack(Importer);
				}
			}
		}

		void DefaultVATClaimBack(OrgHeader org)
		{
			JE_VATClaimBackIndicator = org == null ? "" : (org.LocalVATCode.IsEmpty ? "N" : "Y");
		}

		ZDate GetDefaultExchangeRateDate()
		{
			switch (JE_MessageType)
			{
				case ZAJobMessageTypeList.Codes.Import:
					if (HouseBillIssuedDate != ZDateTime.Empty)
					{
						return HouseBillIssuedDate.Date;
					}
					if (JE_MasterBillIssuedDate != ZDateTime.Empty)
					{
						return JE_MasterBillIssuedDate.Date;
					}
					break;
				case ZAJobMessageTypeList.Codes.Export:
					var header1 = ((IEnumerable<CusEntryHeader>)CustomsEntryHeaders).OrderByDescending(x => x.CH_EntrySubmittedDate).FirstOrDefault();
					var submittedDate = header1?.CH_EntrySubmittedDate ?? ZDateTime.Empty;
					if (submittedDate.IsEmpty)
					{
						return ZDate.Today.AddDays(-1);
					}
					else if (submittedDate.IsValid && submittedDate.Date > ZDateTime.MinSmallDateTimeValue)
					{
						return submittedDate.AddDays(-1).Date;
					}
					break;
				case ZAJobMessageTypeList.Codes.ExBond:
					var header2 = CustomsEntryHeaders
						.Where(x => (string)x.EntryInstruction?.CEI_Style == UniversalReferenceConstants.ProcedureCodes._46 || (string)x.EntryInstruction?.CEI_Style == UniversalReferenceConstants.ProcedureCodes._47)
						.OrderByDescending(x => x.CH_EntrySubmittedDate)
						.FirstOrDefault();
					if (header2 != null)
					{
						var result = header2.CH_EntrySubmittedDate;
						return (result.IsEmpty || !result.IsValid) ? ZDate.Today : result.Date;
					}
					break;
			}
			return ZDate.Empty;
		}

		void RefreshInvoiceLineInfo()
		{
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				invoiceLine.DefaultPreference();
			}
		}

		void RefreshInvoiceLineMaxCountValidation() => InvoiceLines.UpdateMaxCountValidation();
		#endregion

		#region ICurrencyConverterDataProvider

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return 0; }
		}

		#endregion

		#region IDA63ValueRecalculationParent

		public ZBool DA63NeedsRecalculation
		{
			get { return Invoices?.OfType<IDA63ValueRecalculationParent>()?.Any(x => x.DA63NeedsRecalculation) ?? false; }
		}

		public void RecalculateDA63Values()
		{
			foreach (var invoice in Invoices.OfType<IDA63ValueRecalculationParent>().Where(x => x.DA63NeedsRecalculation))
			{
				invoice.RecalculateDA63Values();
			}
		}

		#endregion

		#region IOnUniversalEventAddedHandler

		public void OnUniversalEventAdded(IXmlSessionTracker logger, UniversalDataBuss.DataObjects.Universal.Event eventAdded)
		{
			IEventProcessor processor = null;
			switch (eventAdded.EventType)
			{
				case Events.DocumentNotDeliveredCode:
					processor = new DocumentNotDeliveredProcessor(logger);
					break;
				case Events.DocumentDeliveredCode:
					processor = new DocumentEventProcessor(logger);
					break;
			}
			if (processor != null)
			{
				processor.Process(this, eventAdded);
			}
		}

		#endregion

		internal bool IsInPreSaveValidation => ((IBusinessObjectInternals)this).IsInPreSaveValidation;

		protected override IDisposable GetValidationDataSuspender()
		{
			var baseSuspender = base.GetValidationDataSuspender();
			CreateDictionaryForDuplicatedVINsCheck();

			return new DisposableAction(() =>
			{
				baseSuspender.Dispose();
				DuplicateVINWarningDictionary = null;
			});
		}

		void CreateDictionaryForDuplicatedVINsCheck()
		{
			DuplicateVINWarningDictionary = new Dictionary<ZString, ZStringBuilder>();
			var vinList = Invoices.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>().Select(line => line.JI_VIN).Where(vin => !vin.IsEmpty)).Distinct().ToArray();
			var duplicateVINInvoiceLines = ValidationHelper.GetDuplicatedVINInvoiceLines(this, vinList);
			if (duplicateVINInvoiceLines.Any())
			{
				foreach (DynamicBusinessObject invoiceLineDetail in duplicateVINInvoiceLines)
				{
					var vin = invoiceLineDetail[CusVehicleSchema.Constants.CVH_VehicleIdentificationNumber].ToString();
					if (DuplicateVINWarningDictionary.ContainsKey(vin))
					{
						var warningMessage = DuplicateVINWarningDictionary[vin];
						warningMessage.Append(ValidationHelper.GetduplicateVINWarningMessage(invoiceLineDetail));
					}
					else
					{
						var warningMessage = new ZStringBuilder();
						warningMessage.Append(JobComInvoiceLineValidation.DuplicatedVINNumberOnAnotherDeclaration);
						warningMessage.Append(ValidationHelper.GetduplicateVINWarningMessage(invoiceLineDetail));
						DuplicateVINWarningDictionary.Add(vin, warningMessage);
					}
				}
			}
		}

		internal Dictionary<ZString, ZStringBuilder> DuplicateVINWarningDictionary { get; set; }

		public Dictionary<ZString, HashSet<ZGuid>> VINLookup => Factory.GetValue(ref vinLookup, () =>
			{
				return InvoiceLines.Cast<JobComInvoiceLine>().GroupBy(invoiceLine => invoiceLine.JI_VIN)
					.ToDictionary(x => x.Key, x => x.Select(l => l.PK).ToHashSet());
			});

		CachedProperty<Dictionary<ZString, HashSet<ZGuid>>> vinLookup;

		protected override BaseJobDeclarationInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new JobDeclarationInvoicingSupporter(this);
		}

		public override Customs.Business.SupportingDocSendingObject GetSupportingDocSendingObject()
		{
			return new SupportingDocSendingObject(this);
		}

		public bool IsBLNSValidationRequired
		{
			get
			{
				switch (JE_MessageType)
				{
					case ZAJobMessageTypeList.Codes.Import:
					case ZAJobMessageTypeList.Codes.ImportByExternalBroker:
						return IsBLNSCountryInList(new string[] { JE_RL_NKPortOfLoading, JE_RL_NKOrigin, JE_GoodsOrigin });

					case ZAJobMessageTypeList.Codes.ExBond:
						return IsBLNSCountryInList(new string[] { JE_RL_NKPortOfArrival, JE_RL_NKFinalDestination });

					default:
						return true;
				}
			}
		}

		bool IsBLNSCountryInList(string[] countryList)
		{
			var cleanList = countryList.Where(x => !string.IsNullOrWhiteSpace(x) && x.Length >= 2).Select(x => x.Substring(0, 2)).Distinct();

			foreach (var code in cleanList)
			{
				var isBLNS = RefCountry.LoadFromCountryCode(Factory, code)?.IsBLNS ?? false;

				if (isBLNS)
				{
					return true;
				}
			}

			return false;
		}

		protected override ZBool SupportValidateCustomsMessagingCore => true;

		public ZAAutoSendCustomsMessageProcessor GetEntryDeclarationMessageProcessor(JobDeclaration declaration)
		{
			return new ZAAutoSendCustomsMessageProcessor(declaration);
		}

		#region Override IJobDeclarationMessageSupporter Members

		protected override ZBool SupportEntryDeclarationMessageCore => true;

		public override bool SupportInvoiceLineRefs => true;

		protected override IProcessor GetEntryDeclarationMessageProcessorCore()
		{
			return new ZAAutoSendCustomsMessageProcessor(this);
		}

		#endregion

		protected override ZQuery GetValidCusEntryNumFilter()
		{
			var result = base.GetValidCusEntryNumFilter();

			result.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.NotEqual, CusEntryNumberTypes.Standard.UniqueConsignementReference);

			return result;
		}

		public bool IsExBondAndAutomaticDeferred => JE_MessageType == ZAJobMessageTypeList.Codes.ExBond && ZACustomsRegistry.Instance.AutomaticDeferredSelection.Value.AllowAutomaticDeferredSelection;

		public ZBool IsUnknownOrNotApplicable => new ZString[] { ZString.Empty, Core.Constants.TransportModes.Other }.Contains(TransportMode);

		#region Auto-allocate Invoice Lines to Entry Instructions

		public bool HasAnyInvoiceLinesLinkedToEntryInstructions => InvoiceLines.OfType<JobComInvoiceLine>().Any(line => line.JI_CEI.IsValid);

		public void AutoCreateEntryInstructions(bool overwriteExistingLinks)
		{
			CusEntryInstruction entryInstruction11 = null;
			CusEntryInstruction entryInstruction40 = null;

			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				if ((!line.JI_CEI.IsValid || overwriteExistingLinks) && line.UniversalTariff != null)
				{
					var entry = TariffCodeHasDuty(line.UniversalTariff.PK, line.EffectiveAssessmentDate, line.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff))
						? GetOrCreateEntryInstructionForCpc(ref entryInstruction40, UniversalReferenceConstants.ProcedureCodes._40)
						: GetOrCreateEntryInstructionForCpc(ref entryInstruction11, UniversalReferenceConstants.ProcedureCodes._11);
					if (entry != null)
					{
						line.JI_CEI = entry.PK;
					}
				}
			}
		}

		internal bool TariffCodeHasDuty(ZGuid tariffPK, ZDateTime effectiveDate, ZString dataGroupingCode)
		{
			var queryPref = new ZDBOnlySubQuery(typeof(CusRefPreferenceView), CusRefPreferenceViewSchema.PK);
			queryPref.AddToFilter(CusRefPreferenceViewSchema.ZZS_ZZZ_NKDataGrouping, dataGroupingCode);
			queryPref.AddToFilter(CusRefPreferenceViewSchema.ZZS_Preference, UniversalReferenceConstants.PrimaryPreference.Standard);

			var queryRateType = new ZDBOnlySubQuery(typeof(CusRefRateCodeView), CusRefRateCodeViewSchema.PK);
			queryRateType.AddToFilter(CusRefRateCodeViewSchema.ZY1_ZZZ_NKDataGrouping, dataGroupingCode);
			queryRateType.AddToFilter(CusRefRateCodeViewSchema.ZY1_RateType, Constants.RateTypes.Duty);

			var queryRate = new ZDBOnlyQuery(typeof(RateView));
			queryRate.AddToFilter(RateViewSchema.ZZ2_ZZZ_NKDataGrouping, dataGroupingCode);
			queryRate.AddSubQuery(RateViewSchema.ZZ2_ZY1_RateCode, CusRefRateCodeViewSchema.PK, queryRateType, JoinCondition.And);
			queryRate.AddSubQuery(RateViewSchema.ZZ2_ZZS_Preference, CusRefPreferenceViewSchema.PK, queryPref, JoinCondition.And);
			queryRate.AddToFilter(RateViewSchema.ZZ2_ZZ1_ParentTariffOrNationalCode, tariffPK);
			queryRate.AddToFilter(RateViewSchema.ZZ2_StartDate, SQLComparisonOperator.LessThanOrEqualTo, effectiveDate);
			queryRate.AddToFilter(RateViewSchema.ZZ2_EndDate, SQLComparisonOperator.GreaterThan, effectiveDate);

			var rate = Factory.LoadTop1<RateView>(queryRate);
			return rate != null && rate.ZZ2_RateFormula != "0";
		}

		CusEntryInstruction GetOrCreateEntryInstructionForCpc(ref CusEntryInstruction entryLine, string cpc)
		{
			if (entryLine == null)
			{
				entryLine = CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault(line => line.CEI_Style == cpc);
			}
			if (entryLine == null)
			{
				entryLine = CustomsEntryInstructions.AddNew();
				entryLine.CEI_Style = cpc;
			}
			return entryLine;
		}

		#endregion

		public ZBool HasSplitByBondAmount = ZBool.False;

		public ZBool HasSplitByMaxEntryLines = ZBool.False;

		protected override bool CalculateJE_ApplicationCode_ReadOnly()
		{
			switch (GetInterfaceSubmissionType())
			{
				case "":
				case DeclarationApplicationCodeList.Codes.Builtin:
				case DeclarationApplicationCodeList.Codes.Interfaced:
					return true;
				default:
					return false;
			}
		}

		protected override void FlushImporterDocumentaryAddressIfBlank(ZGuid importer) { }

		protected override TransportSupporter GetNewTransportSupporter() => new JobDeclarationTransportSupporter(this);
	}
}
