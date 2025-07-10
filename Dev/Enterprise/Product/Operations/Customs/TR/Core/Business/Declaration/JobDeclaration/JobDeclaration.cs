using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class JobDeclaration : AutoTRJobDeclaration
		, Integration.Customs.TR.IJobDeclaration, IApportionInvoiceHolder, IManifestToOpenProvider
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JE_ExportGoodsType = OrderTypesOfGoodsList.Codes._3;
			JE_ValuationDate = ZDate.Today;
			ZG_TradeType = TradeTypeList.Codes.ETD;
		}

		#endregion

		[ChildEditable(true)]
		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		protected override bool IsLookupsCachedInBase => false;

		protected override Customs.Business.JobDeclarationLookups GetNewLookups()
		{
			JobDeclarationLookups result;
			if (IsImport)
			{
				result = new ImportJobDeclarationLookups(this);
			}
			else if (IsExport)
			{
				result = new ExportJobDeclarationLookups(this);
			}
			else
			{
				result = new JobDeclarationLookups(this);
			}
			return result;
		}

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			JobDeclarationValidation result;
			if (IsImport)
			{
				result = new ImportJobDeclarationValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobDeclarationValidation(this);
			}
			else
			{
				result = new JobDeclarationValidation(this);
			}
			return result;
		}

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new DeclarationJobDocAddressValidation(addressToValidate, this);

		protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		#region DV1Details

		[ChildEditable(true)]
		public new CusDV1DetailCollection DV1Details => (CusDV1DetailCollection)base.DV1Details;
		protected override EU.Business.Declaration.CusDV1DetailCollection CreateNewDV1DetailsCollection() => new CusDV1DetailCollection(this);

		#endregion

		#region CusEntryInstruction

		[MaxLength(CusEntryInstruction.Schema.CEI_SubStyleMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EntrySubStyleList))]
		[ResourceStringData("A7B6AED1-33CC-483D-AAA0-5DD14A918390", Caption = "Simplified Procedure Code", ShortCaption = "Simp. Procedure")]
		public ZString JE_EntrySubStyle
		{
			get
			{
				return CusEntryInstruction.CEI_SubStyle;
			}
			set
			{
				CheckMaximumLength(JE_EntrySubStyleInfo, value);
				CusEntryInstruction.CEI_SubStyle = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEntrySubStyle();
				}

				JE_EntrySubStyleInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_EntrySubStyleInfo => GetZPropertyInfo(nameof(JE_EntrySubStyle));

		[BusinessObjectTestExclude]
		[ResourceStringData("E4C22F5F-2601-4D05-BF97-DD4427899535", Caption = "Reference Date", ShortCaption = "Ref. Date")]
		public ZDateTime JE_EntryDateForDuty
		{
			get
			{
				return CusEntryInstruction.CEI_DateForDuty;
			}
			set
			{
				CusEntryInstruction.CEI_DateForDuty = value;
				JE_EntryDateForDutyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_EntryDateForDutyInfo => GetZPropertyInfo(nameof(JE_EntryDateForDuty));

		public override ZGuid JE_OA_ManufacturerAddress
		{
			get => base.JE_OA_ManufacturerAddress;
			set
			{
				var oldValue = base.JE_OA_ManufacturerAddress;
				base.JE_OA_ManufacturerAddress = value;
				var newVaule = JE_OA_ManufacturerAddress;

				if (oldValue != newVaule && !IsCopying)
				{
					InvoiceLines?.MarkAsNeedingValidation();

					foreach (JobComInvoiceLine line in InvoiceLines)
					{
						if (newVaule == line.JI_OA_ManufacturerAddress)
						{
							line.JI_OA_ManufacturerAddress = ZGuid.Empty;
						}
						line.JI_OA_ManufacturerAddressInfo.RefreshBinding();
					}
				}
			}
		}

		#region WarehouseAddresses

		[ResourceStringData("TR.JobDeclaration.JE_EntryFromWarehouse", Caption = "From Warehouse", ShortCaption = "From Whs.")]
		public ZGuid JE_EntryFromWarehouse
		{
			get
			{
				return CusEntryInstruction.CEI_OA_Warehouse;
			}
			set
			{
				CusEntryInstruction.CEI_OA_Warehouse = value;
			}
		}

		public ZAddress JE_EntryFromWarehouse_ZAddress
		{
			get
			{
				if (fJE_EntryFromWarehouse_ZAddress == null)
				{
					fJE_EntryFromWarehouse_ZAddress = GetNewJE_EntryFromWarehouse_ZAddress();
				}
				return fJE_EntryFromWarehouse_ZAddress;
			}
		}
		ZAddress fJE_EntryFromWarehouse_ZAddress;

		ZAddress GetNewJE_EntryFromWarehouse_ZAddress()
		{
			return new ZAddress(CusEntryInstruction.CEI_OA_WarehouseInfo);
		}

		[ResourceStringData("TR.JobDeclaration.JE_EntryToWarehouse", Caption = "To Warehouse", ShortCaption = "To Whs.")]
		public ZGuid JE_EntryToWarehouse
		{
			get
			{
				return CusEntryInstruction.CEI_OA_Warehouse2;
			}
			set
			{
				CusEntryInstruction.CEI_OA_Warehouse2 = value;
			}
		}

		public ZAddress JE_EntryToWarehouse_ZAddress
		{
			get
			{
				if (fJE_EntryToWarehouse_ZAddress == null)
				{
					fJE_EntryToWarehouse_ZAddress = GetNewJE_EntryToWarehouse_ZAddress();
				}
				return fJE_EntryToWarehouse_ZAddress;
			}
		}
		ZAddress fJE_EntryToWarehouse_ZAddress;

		ZAddress GetNewJE_EntryToWarehouse_ZAddress()
		{
			return new ZAddress(CusEntryInstruction.CEI_OA_Warehouse2Info);
		}
		#endregion

		public CusEntryInstruction CusEntryInstruction
		{
			get
			{
				if (!IsDeleted && (cusEntryInstruction == null || cusEntryInstruction.IsDeleted))
				{
					cusEntryInstruction = CustomsEntryInstructionProvider.CustomsEntryInstructions.OfType<CusEntryInstruction>().OrderBy(x => x.PK).FirstOrDefault();
					if (cusEntryInstruction == null)
					{
						cusEntryInstruction = ((EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>)CustomsEntryInstructionProvider.CustomsEntryInstructions).AddNew();
					}

					if (cusEntryInstruction != null)
					{
						RegisterEditableChildObject(cusEntryInstruction);
						if (!IsPersistent)
						{
							cusEntryInstruction.MakeNonPersistent();
						}
					}
				}
				return cusEntryInstruction;
			}
		}
		CusEntryInstruction cusEntryInstruction;

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);

		#endregion

		#region New Properties

		[ChildEditable(true)]
		public ManifestToOpenHeaderCollection ManifestToOpenHeaders
		{
			get
			{
				if (manifestToOpenHeaders == null)
				{
					manifestToOpenHeaders = new ManifestToOpenHeaderCollection(this);
					RegisterEditableChildObject(manifestToOpenHeaders);
				}
				return manifestToOpenHeaders;
			}
		}
		ManifestToOpenHeaderCollection manifestToOpenHeaders;

		[ChildEditable(true)]
		public TraderCollection Traders
		{
			get
			{
				if (traders == null)
				{
					traders = new TraderCollection(this);
					RegisterEditableChildObject(traders);
				}
				return traders;
			}
		}
		TraderCollection traders;

		[MaxLength(10)]
		[ResourceStringData("5205DD7F-A35E-49EE-A19C-AB38E0134B15", Caption = "[27] Discharge Office")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TRCustomsOfficeList))]
		public ZString DischargeOffice
		{
			get
			{
				if (dischargeOffice == null)
				{
					dischargeOffice = CustomsOffices.Cast<OfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented);
				}

				return dischargeOffice?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(DischargeOfficeInfo, value);
				var oldValue = DischargeOffice;
				if (value != oldValue)
				{
					if (dischargeOffice == null)
					{
						dischargeOffice = CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented);
					}

					dischargeOffice.CY_Data = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDischargeOffice();
				}

				DischargeOfficeInfo.RefreshBinding(oldValue);
			}
		}
		OfficeCode dischargeOffice;

		public ZPropertyInfo DischargeOfficeInfo => GetZPropertyInfo(nameof(DischargeOffice));

		ZString OfficeDescription => DischargeOffice.IsEmpty ? ZString.Empty : dischargeOffice?.CY_OfficeDescription ?? ZString.Empty;

		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		[ResourceStringData("A6F4DC3E-48ED-4865-BB71-92EAFAB028E7", Caption = "Discharge Place")]
		public ZString DischargePlace
		{
			get => GetEffectiveValueToReturn(this.GetSystemDefinedValue<ZString>(nameof(DischargePlace)), OfficeDescription);
			set
			{
				var oldValue = DischargePlace;
				CheckMaximumLength(DischargePlaceInfo, value);
				this.SetSystemDefinedValue(nameof(DischargePlace), GetEffectiveValueToSet(value, OfficeDescription));
				if (!IsValidationSuspended)
				{
					Validation.ValidateDischargePlace();
				}
				DischargePlaceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DischargePlaceInfo => GetZPropertyInfo(nameof(DischargePlace));

		[MaxLength(10)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TRCustomsOfficeList))]
		public ZString EntryOffice
		{
			get
			{
				if (entryOffice == null)
				{
					entryOffice = CustomsOffices.Cast<OfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented);
				}

				return entryOffice?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(EntryOfficeInfo, value);
				var oldValue = EntryOffice;
				if (value != oldValue)
				{
					if (entryOffice == null)
					{
						entryOffice = CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented);
					}

					entryOffice.CY_Data = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateEntryOffice();
				}

				EntryOfficeInfo.RefreshBinding(oldValue);
			}
		}
		OfficeCode entryOffice;

		public ZPropertyInfo EntryOfficeInfo => GetZPropertyInfo(nameof(EntryOffice));

		public ZBool IsActualImport => IsImport && InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.ProcedureCode.EndsWith("00"));

		public ZPropertyInfo IsActualImportInfo => GetZPropertyInfo(nameof(IsActualImport));

		#endregion

		ZString GetEffectiveValueToReturn(ZString originalValue, ZString backupValue)
		{
			return originalValue.IsEmpty ? backupValue : originalValue;
		}

		ZString GetEffectiveValueToSet(ZString valuePassed, ZString backupValue)
		{
			var result = valuePassed;
			if (valuePassed == backupValue)
			{
				result = ZString.Empty;
			}
			return result;
		}

		protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = base.GetCusCodeDataTypesCore();
			result[CusCodeDataTypeList.Codes.OfficeCode] = typeof(OfficeCode);
			return result;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobDeclarationFetchStrategy(this);

		protected override EuOfficeCodeCollection GetCustomsOffices()
		{
			var customsOffices = new OfficeCodeCollection(this);
			customsOffices.DefaultPurposeCode = EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented;
			return customsOffices;
		}

		public new OfficeCodeCollection CustomsOffices => (OfficeCodeCollection)base.CustomsOffices;

		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				var oldValue = JE_MessageType;
				base.JE_MessageType = value;
				if (oldValue != JE_MessageType)
				{
					CustomsOffices.MarkAsNeedingValidation();
					RefreshIncotermAndChargeFactory();
					RefreshExRateToLatestRateAvailableIfNeeded();
					ManifestToOpenHeaders.MarkAsNeedingValidation();
					ZG_IsHighValueOvrd = IsImport;
					IsActualImportInfo.RefreshBinding();
				}
			}
		}

		public override ZGuid JE_GC
		{
			get => base.JE_GC; set
			{
				var oldValue = base.JE_GC;
				base.JE_GC = value;
				if (!IsCopying && oldValue != JE_GC)
				{
					ManifestToOpenHeaders.MarkAsNeedingValidation();
				}
			}
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Turkey;

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("TR"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection()
		{
			return new EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>(this);
		}

		string IApportionInvoiceHolder.CountryContext
		{
			get { return CountryCode + this.GetIncoTermChargeFactoryCacheKey(); }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportModeInland))]
		[ResourceStringData("TR.JobDeclaration.JE_TransportModeInland", Caption = "[25] Border M.O.T.")]
		public override ZString JE_TransportModeInland
		{
			get => base.JE_TransportModeInland;
			set
			{
				var oldValue = JE_TransportModeInland;
				base.JE_TransportModeInland = value;
				if (CusEntryInstruction.ZG_InlandTransportType == oldValue)
				{
					CusEntryInstruction.ZG_InlandTransportType = value;
				}
			}
		}

		[ResourceStringData("TR.JobDeclaration.JE_TransportMode", Caption = "Transport")]
		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set => base.JE_TransportMode = value;
		}

		public override ZString JE_LocationOfGoods
		{
			get => base.JE_LocationOfGoods;
			set
			{
				var oldValue = JE_LocationOfGoods;
				base.JE_LocationOfGoods = value;
				if (oldValue != JE_LocationOfGoods)
				{
					CustomsOffices.MarkAsNeedingValidation();
					SetJE_SubLocationOfGoods();
				}
			}
		}

		public override ZString JE_GoodsDestination
		{
			get => base.JE_GoodsDestination;
			set
			{
				var oldValue = JE_GoodsDestination;
				base.JE_GoodsDestination = value;
				if (CusEntryInstruction.ZG_ExportUnionCountryCode == ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Enterprise.Core.Constants.CountryCodes.Turkey, TRMessageConstants.CountryMapType, oldValue, ZDateTime.Today))
				{
					CusEntryInstruction.ZG_ExportUnionCountryCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Enterprise.Core.Constants.CountryCodes.Turkey, TRMessageConstants.CountryMapType, value, ZDateTime.Today);
				}
			}
		}

		void SetJE_SubLocationOfGoods()
		{
			JE_SubLocationOfGoods = ((CodeDescriptionPairList)Lookups.Locations).GetDescriptionFromCode(JE_LocationOfGoods);
		}

		[MaxLength(35)]
		public override ZString JE_SubLocationOfGoods { get => base.JE_SubLocationOfGoods; set => base.JE_SubLocationOfGoods = value; }

		#region CusEntryHeader

		public CusEntryHeader CusEntryHeader => ActiveEntryHeaders.Cast<CusEntryHeader>().OrderBy(x => x.CH_EntrySubmittedDate).FirstOrDefault();
		public override bool WillThereBeMultipleEntryHeaders => false;

		[ResourceStringData("8DD5C31E-80EC-45AD-834D-778ECBDECC64", Caption = "Goods are at Customs Area")]
		public ZBool GoodsAtCustomsArea => CusEntryHeader?.EUH_AreGoodsAtCustomsArea ?? ZBool.False;
		public ZPropertyInfo GoodsAtCustomsAreaInfo => GetZPropertyInfo(nameof(GoodsAtCustomsArea));

		[ResourceStringData("12679AB1-4965-4F93-B7AA-668BA62A14C8", Caption = "Overtime Time Procedure Completed")]
		public ZBool OverTimePaymentCompleted => CusEntryHeader?.EUH_IsOverTimePaymentCompleted ?? ZBool.False;
		public ZPropertyInfo OverTimePaymentCompletedInfo => GetZPropertyInfo(nameof(OverTimePaymentCompleted));

		[ResourceStringData("C7AF4912-52AF-4C88-A192-5B86D6AE1C20", Caption = "Inspection Clerk")]
		public ZString InspectionClerk => CusEntryHeader?.EUH_InspectionClerk ?? ZString.Empty;
		public ZPropertyInfo InspectionClerkInfo => GetZPropertyInfo(nameof(InspectionClerk));

		[ResourceStringData("8E7FBEE5-2577-4280-927D-1AC0DAF007CA", ShortCaption = "Dec. Date", Caption = "Declaration Date")]
		public ZDateTime JE_DeclarationDate => CusEntryHeader?.CH_EntrySubmittedDate ?? ZDateTime.Empty;
		public ZPropertyInfo JE_DeclarationDateInfo => GetZPropertyInfo(nameof(JE_DeclarationDate));

		#endregion

		[ResourceStringData("B0A26AB2-D45F-4A81-A9BD-AE5779C40418", Caption = "Dec.Exchange Rate")]
		public ZDecimal JE_DeclarationExchangeRate => DefaultInvoice?.JZ_InvoiceCurrExRate ?? ZDecimal.Zero;

		public ZPropertyInfo JE_DeclarationExchangeRateInfo => GetZPropertyInfo(nameof(JE_DeclarationExchangeRate));

		[ResourceStringData("951058C0-8749-44FE-A377-8FD0F4C7ED6C", Caption = "Total Net Weight")]
		[DecimalPlaces(3)]
		public ZDecimal TotalCustomsQuantity => InvoiceLines.OfType<JobComInvoiceLine>().Sum(x => x.JI_CustomsQuantity);

		public ZPropertyInfo TotalCustomsQuantityInfo => GetZPropertyInfo(nameof(TotalCustomsQuantity));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.WeightUnitList))]
		public ZString TotalCustomsQuantityUnit => Core.Constants.Weight.Kilograms;

		[ResourceStringData("B9D5B028-32DA-4FB6-A0E6-494C312219F4", Caption = "Customs Discharge Port", ShortCaption = "Cus.Disc.Port")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TRCustomsPortList))]
		public override ZString JE_CustomsDischargePort { get => base.JE_CustomsDischargePort; set => base.JE_CustomsDischargePort = value; }

		[ResourceStringData("48A24FC7-F4F0-4C8E-BC6E-D0989E8674D6", Caption = "Customs Load Port", ShortCaption = "Cus.Load Port")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TRCustomsPortList))]
		public override ZString JE_CustomsLoadPort { get => base.JE_CustomsLoadPort; set => base.JE_CustomsLoadPort = value; }

		[ResourceStringData("72377F41-DC25-453F-912C-CEF7743EA2B4", Caption = "Order Type")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.OrderTypesOfGoodsList))]
		public override ZString JE_ExportGoodsType { get => base.JE_ExportGoodsType; set => base.JE_ExportGoodsType = value; }

		#region Calculated Total Amounts

		CachedProperty<ZInt> invoiceCountCache;
		[ResourceStringData("A3796225-FB93-4F58-B134-8862BDC57085", Caption = "Total Dec. Lines")]
		public ZInt InvoiceCount => Factory.GetValue(ref invoiceCountCache, () => Invoices.Count);

		public ZPropertyInfo InvoiceCountInfo => GetZPropertyInfo(nameof(InvoiceCount));

		CachedProperty<ZDecimal> totalInvoiceAmountCache;
		[ResourceStringData("6A65D918-2DE0-43E4-ADC8-2911320033CA", Caption = "Invoice Amount")]
		public new ZDecimal TotalInvoiceAmount => Factory.GetValue(ref totalInvoiceAmountCache, () => base.TotalInvoiceAmount.Amount);

		public ZPropertyInfo TotalInvoiceAmountInfo => GetZPropertyInfo(nameof(TotalInvoiceAmount));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CurrencyList))]
		[ResourceStringData("549961BF-3952-4AB3-BC4A-2CB8F87831FE", Caption = "Currency", FullDescription = "Invoice Amount Currency", ShortCaption = "Curr.")]
		public ZGuid TotalInvoiceCurrency => DefaultInvoiceCurrency;
		public ZPropertyInfo TotalInvoiceCurrencyInfo => GetZPropertyInfo(nameof(TotalInvoiceCurrency));

		CachedProperty<ZDecimal> totalFreeOnBoardAmountCache;
		[ResourceStringData("A69844BB-706F-4644-9337-6916328C012E", Caption = "[42] FOB Amount")]
		public ZDecimal TotalFreeOnBoardAmount => Factory.GetValue(ref totalFreeOnBoardAmountCache, () => InvoicesEnum.Sum(invoice => invoice.JZ_Calc_FOBAmount));

		public ZPropertyInfo TotalFreeOnBoardAmountInfo => GetZPropertyInfo(nameof(TotalFreeOnBoardAmount));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CurrencyList))]
		[ResourceStringData("710F2A5F-C29C-493B-A476-46A7F8B899E3", Caption = "Currency", FullDescription = "FOB Amount Currency", ShortCaption = "Curr.")]
		public ZGuid TotalFreeOnBoardCurrency => DefaultInvoiceCurrency;
		public ZPropertyInfo TotalFreeOnBoardCurrencyInfo => GetZPropertyInfo(nameof(TotalFreeOnBoardCurrency));

		CachedProperty<ZDecimal> totalFreightAmountCache;
		[ResourceStringData("DC95553D-F308-4D4E-9BA2-17D82C8485A5", Caption = "Freight Amount")]
		public ZDecimal TotalFreightAmount => Factory.GetValue(ref totalFreightAmountCache, () => InvoicesEnum.Sum(invoice => invoice.OverseasFreight.Amount));

		public ZPropertyInfo TotalFreightAmountInfo => GetZPropertyInfo(nameof(TotalFreightAmount));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CurrencyList))]
		[ResourceStringData("63298C4F-7D6E-4BE9-AB61-3E1E9B96C9AF", Caption = "Currency", FullDescription = "Freight Amount Currency", ShortCaption = "Curr.")]
		public ZGuid TotalFreightCurrency => InvoicesEnum.FirstOrDefault()?.OverseasFreight.Currency.PK ?? ZGuid.Empty;
		public ZPropertyInfo TotalFreightCurrencyInfo => GetZPropertyInfo(nameof(TotalFreightCurrency));

		CachedProperty<ZDecimal> totalInsuranceAmountCache;
		[ResourceStringData("210D9C8D-0344-4F8C-8ED5-7A8BA6DFF007", Caption = "Insurance Amount")]
		public ZDecimal TotalInsuranceAmount => Factory.GetValue(ref totalInsuranceAmountCache, () => InvoicesEnum.Sum(invoice => invoice.OverseasInsurance.Amount));

		public ZPropertyInfo TotalInsuranceAmountInfo => GetZPropertyInfo(nameof(TotalInsuranceAmount));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CurrencyList))]
		[ResourceStringData("B7EB0088-4E54-4857-8894-22BCBF40A36B", Caption = "Currency", FullDescription = "Insurance Amount Currency", ShortCaption = "Curr.")]
		public ZGuid TotalInsuranceCurrency => InvoicesEnum.FirstOrDefault()?.OverseasInsurance.Currency.PK ?? ZGuid.Empty;
		public ZPropertyInfo TotalInsuranceCurrencyInfo => GetZPropertyInfo(nameof(TotalInsuranceCurrency));

		CachedProperty<ZDecimal> totalOverseasAmountCache;
		[ResourceStringData("A134AAB1-DB6B-4AE5-85B5-AC4C48040F9F", Caption = "Overseas Amount")]
		public ZDecimal TotalOverseasAmount => Factory.GetValue(ref totalOverseasAmountCache, () => SumChargeAmount(TRIncotermChargeCodeList.Codes.TotalForeignCharges));

		public ZPropertyInfo TotalOverseasAmountInfo => GetZPropertyInfo(nameof(TotalOverseasAmount));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CurrencyList))]
		[ResourceStringData("DCED4349-2A27-428A-A9E8-DF2AFB7F4BF2", Caption = "Currency", FullDescription = "Overseas Amount Currency", ShortCaption = "Curr.")]
		public ZGuid TotalOverseasCurrency => GetDefaultCurrency(TRIncotermChargeCodeList.Codes.TotalForeignCharges);
		public ZPropertyInfo TotalOverseasCurrencyInfo => GetZPropertyInfo(nameof(TotalOverseasCurrency));

		CachedProperty<ZDecimal> localTotalChargesCache;
		[ResourceStringData("E3B50DF5-295F-429A-BD78-20883890D22A", Caption = "Domestic Amount")]
		public ZDecimal LocalTotalChargesAmount => Factory.GetValue(ref localTotalChargesCache, () => SumChargeAmount(TRIncotermChargeCodeList.Codes.LocalTotalCharges));

		public ZPropertyInfo LocalTotalChargesAmountInfo => GetZPropertyInfo(nameof(LocalTotalChargesAmount));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CurrencyList))]
		[ResourceStringData("DB9F40D8-2995-4494-961F-A0E76A244837", Caption = "Currency", FullDescription = "Domestic Amount Currency", ShortCaption = "Curr.")]
		public ZGuid LocalTotalChargesCurrency => GetDefaultCurrency(TRIncotermChargeCodeList.Codes.LocalTotalCharges);
		public ZPropertyInfo LocalTotalChargesCurrencyInfo => GetZPropertyInfo(nameof(LocalTotalChargesCurrency));

		#endregion

		#region Bonded Warehouse Code

		[MaxLength(9)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BondedWarehouseCodeList))]
		[ResourceStringData("1090FDF3-0215-4775-9B07-16F39A90B927", Caption = "[49] Bonded Warehouse Code", ShortCaption = "[49] BWH Code")]
		public ZString BondedWarehouseCode
		{
			get { return BondedWarehouseCustomsSupportingInfo?.CSI_CustomsOffice ?? ZString.Empty; }
			set
			{
				var oldValue = BondedWarehouseCode;
				if (oldValue != value)
				{
					var warehouseCode = BondedWarehouseCustomsSupportingInfo ?? CreateBondedWarehouseCodeInfo();
					CheckMaximumLength(BondedWarehouseCodeInfo, value);
					warehouseCode.CSI_CustomsOffice = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateBondedWarehouseCode();
					}
					BondedWarehouseCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo BondedWarehouseCodeInfo => GetZPropertyInfo(nameof(BondedWarehouseCode));

		BondedWarehouseCodeInfo BondedWarehouseCustomsSupportingInfo
		{
			get
			{
				if (cusSupportingInfo == null || cusSupportingInfo.IsDeleted)
				{
					cusSupportingInfo = CusSupportingInfoList.Cast<BondedWarehouseCodeInfo>().FirstOrDefault(x => !x.IsDeleted);
				}
				return cusSupportingInfo;
			}
		}
		BondedWarehouseCodeInfo cusSupportingInfo;

		BondedWarehouseCodeInfo CreateBondedWarehouseCodeInfo()
		{
			using (SuspendSettingHasChanges())
			using (SuspendMarkingAsNeedingValidation())
			{
				return CusSupportingInfoList.AddNew();
			}
		}

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public BondedWarehouseCodeInfoCollection CusSupportingInfoList
		{
			get
			{
				if (cusSupportingInfoList == null)
				{
					cusSupportingInfoList = new BondedWarehouseCodeInfoCollection(this);
					cusSupportingInfoList.Load();
					RegisterEditableChildObject(cusSupportingInfoList);
				}
				return cusSupportingInfoList;
			}
		}
		BondedWarehouseCodeInfoCollection cusSupportingInfoList;

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[CusSupportingInfoTypeList.Codes.BondedWarehouse] = typeof(BondedWarehouseCodeInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			return result;
		}

		#endregion

		ZGuid DefaultInvoiceCurrency => DefaultInvoice?.Invoice_Currency?.PK ?? ZGuid.Empty;

		IEnumerable<JobComInvoiceHeader> InvoicesEnum => Invoices.Cast<JobComInvoiceHeader>();

		JobComInvoiceHeader DefaultInvoice => InvoicesEnum.FirstOrDefault();

		IEnumerable<InvoiceCharge> GetChargesByType(string chargeType) =>
			InvoicesEnum.SelectMany(invoice => invoice.Charges.Cast<InvoiceCharge>())
			.Where(charge => charge.J7_ChargeType == chargeType);

		internal protected virtual ZDecimal SumChargeAmount(string chargeType) => GetChargesByType(chargeType).Sum(charge => charge.J7_Amount);

		internal ZDecimal SumChargeAmountByLocal(string chargeType) => GetChargesByType(chargeType).Sum(charge => charge.MoneyInLocalCurrency.Amount);

		internal ZGuid GetDefaultCurrency(string chargeType) => GetChargesByType(chargeType).Select(charge => charge.Currency).WhereNotNull().FirstOrDefault()?.PK ?? ZGuid.Empty;

		protected override void DeriveImportDeclarationStatus()
		{
			base.DeriveImportDeclarationStatus();
			var header = CusEntryHeader;
			JE_EntryStatus = header?.CH_EntryStatus ?? ZString.Empty;
		}

		public override EU.Business.Declaration.EntryCreationStrategy CreateEntryCreationStrategy() => new EntryCreationStrategy(this);

		public new ICusContainerCollection<CusContainer> CusContainers => (ICusContainerCollection<CusContainer>)base.CusContainers;

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new CusContainerCollection(this, Factory);

		public override ZDate JE_ValuationDate
		{
			get => base.JE_ValuationDate;
			set
			{
				base.JE_ValuationDate = value;
				RefreshExRateToLatestRateAvailableIfNeeded();
				JE_DeclarationExchangeRateInfo.RefreshBinding();
			} 
		}
	}
}
