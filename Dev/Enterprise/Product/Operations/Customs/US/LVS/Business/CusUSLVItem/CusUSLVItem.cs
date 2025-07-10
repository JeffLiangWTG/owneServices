using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using OrgSupplierPart = Enterprise.Customs.Business.OrgSupplierPart;

namespace Enterprise.Customs.US.LVS.Business
{
	[DependentBusinessObject(typeof(CusUSLVConsignment), "CusUSLVItems")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusUSLVItem : AutoCusUSLVItem,
		IHaveAdditionalDataForBorderWise,
		ICargoReleaseCusEntryLine,
		ISimplifiedEntryLine,
		IADDCVDLiability,
		IInvoiceLinePartDetails,
		IInvoiceHeaderForProductCreation
	{
		public CusUSLVItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			RequirementsProvider = new CusUSLVItemPGAAgencyRequirementsProvider(this);
		}

		#region Constants

		public new class Schema : AutoCusUSLVItem.Schema
		{
			public const string ULI_RX_NKCurrEXRate = "ULI_RX_NKCurrEXRate";
			public const string ULI_TariffFormatted = "ULI_TariffFormatted";

			public const int ULI_TariffFormattedMaxLength = 12;
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ULI_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
		}

		CurrencyConverter CurrencyConverter
		{
			get
			{
				if (currencyConverter == null)
				{
					currencyConverter = CurrencyConverter.New(Factory, Consignment?.Shipment?.ULH_DepartureDate ?? ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, 0);
				}
				return currencyConverter;
			}
		}
		CurrencyConverter currencyConverter;

		#region CusUSLVItemPGAs

		[ChildEditable]
		public CusUSLVItemPGACollection CusUSLVItemPGAs
		{
			get
			{
				if (cusUSLVItemPGAs == null)
				{
					cusUSLVItemPGAs = GetNewCusUSLVItemPGACollection();
					cusUSLVItemPGAs.Load();
					RegisterEditableChildObject(cusUSLVItemPGAs);
				}
				return cusUSLVItemPGAs;
			}
		}
		CusUSLVItemPGACollection cusUSLVItemPGAs;

		CusUSLVItemPGACollection GetNewCusUSLVItemPGACollection()
		{
			var collection = new CusUSLVItemPGACollection(this);
			collection.Load();
			return collection;
		}

		[ChildEditable]
		public CusUSLVItemPGAWrapperCollection ItemPGAWrapperCollection
		{
			get
			{
				if (itemPGAWrapperCollection == null)
				{
					itemPGAWrapperCollection = new CusUSLVItemPGAWrapperCollection(RequirementsProvider);
					itemPGAWrapperCollection.Populate();
					RegisterEditableChildObject(itemPGAWrapperCollection);
				}
				return itemPGAWrapperCollection;
			}
		}
		CusUSLVItemPGAWrapperCollection itemPGAWrapperCollection;

		public CusUSLVItemPGAAgencyRequirementsProvider RequirementsProvider { get; }

		#endregion

		#region Product

		internal OrgSupplierPart Product => PartSyncManager.Part;

		void UpdateProductAndPivot()
		{
			if (Product != null && !Product.IsDeleted)
			{
				ULI_GoodsDescription = Product.OP_Desc.Left(Schema.ULI_GoodsDescriptionMaxLength);

				var pivot = Product?.PivotsForBinding.GetImportMatch(Consignment?.ConsigneeOrgPK ?? ZGuid.Empty, Consignment?.SellerOrgPK ?? ZGuid.Empty, ZDate.Today) as CusClassPartPivot;

				if (pivot != null)
				{
					ULI_AntiDumping = pivot.CD_ADDApplicable;
					ULI_Countervailing = pivot.CD_CVDApplicable;
					ULI_RN_NKCountryOfOrigin = pivot.CD_UC_NKCountryOfOrigin;
					ULI_Tariff = pivot.CI_TariffNum.IsEmpty ? pivot.Classification.CC_TariffNum : pivot.CI_TariffNum;

					UpdateProductPGADetailsFromPivot(pivot);
				}
			}
		}

		void UpdateProductPGADetailsFromPivot(CusClassPartPivot pivot)
		{
			ACEFDAWrapper.DisclaimReason = pivot.CD_ACEFDADisclaimReason;
			AMSWrapper.DisclaimReason = pivot.CD_AMSDisclaimReason;
			NOPWrapper.DisclaimReason = pivot.CD_NOPDisclaimReason;
			APHISWrapper.DisclaimReason = pivot.CD_APHISDisclaimReason;
			CPSCWrapper.DisclaimReason = pivot.CD_CPSCDisclaimReason;
			DEAWrapper.DisclaimReason = pivot.CD_DEADisclaimReason;
			FWSWrapper.DisclaimReason = pivot.CD_FWSDisclaimReason;
			LaceyActWrapper.DisclaimReason = pivot.CD_LaceyActDisclaimReason;
			NHTSAWrapper.DisclaimReason = pivot.CD_NHTSADisclaimReason;
			NMFS370Wrapper.DisclaimReason = pivot.CD_NMFS370DisclaimReason;
			NMFSAMRWrapper.DisclaimReason = pivot.CD_NMFSAMRDisclaimReason;
			NMFSHMSWrapper.DisclaimReason = pivot.CD_NMFSHMSDisclaimReason;
			ODSWrapper.DisclaimReason = pivot.CD_ODSDisclaimReason;
			OMCWrapper.DisclaimReason = pivot.CD_OMCDisclaimReason;
			PSTWrapper.DisclaimReason = pivot.CD_PSTDisclaimReason;
			TSCAWrapper.DisclaimReason = pivot.CD_TSCADisclaimReason;
			TTBWrapper.DisclaimReason = pivot.CD_TTBDisclaimReason;
			VNEWrapper.DisclaimReason = pivot.CD_VNEDisclaimReason;
		}

		#endregion

		internal OGARequirementCalculator OGARequirementCalculator
		{
			get
			{
				var today = ZDate.Today;
				return ogaRequirementCalculator ??
					(ogaRequirementCalculator = new OGARequirementCalculator(
						Factory,
						() => Tariff,
						() => null,
						() => today,
						() => ZString.Empty));
			}
		}
		OGARequirementCalculator ogaRequirementCalculator;

		public PGARequirementIndicator PGARequirementIndicator
		{
			get
			{
				var today = ZDate.Today;
				return pgaRequirementIndicator ??
					(pgaRequirementIndicator = new PGARequirementIndicator(
						() => Tariff,
						() => null,
						() => today,
						(x) => true,
						() => ZString.Empty));
			}
		}
		PGARequirementIndicator pgaRequirementIndicator;

		#region Overrides

		#region ULI_ULB

		[RelatedBusinessObject("Consignment")]
		public override ZGuid ULI_ULB
		{
			get => base.ULI_ULB;
			set
			{
				var oldValue = ULI_ULB;
				base.ULI_ULB = value;
				if (Consignment is CusUSLVConsignment consignment)
				{
					ULI_ClusterKey = consignment.ULB_ClusterKey;
				}

				if (!IsCopying && oldValue != ULI_ULB)
				{
					RefreshPartSyncManagerActiveDeciderPK();
				}
			}
		}

		#endregion

		#region ULI_Tariff

		[BusinessObjectTestExclude]
		public override ZString ULI_Tariff
		{
			get => base.ULI_Tariff;
			set
			{
				var oldValue = ULI_Tariff;
				value = TariffFormatter.Format(value);
				var hasChanges = oldValue != value;
				if (hasChanges)
				{
					var consignment = IsCopying ? null : Consignment;
					var hasAtLeastOnePGARequirementOnAnyItemLine = consignment?.HasAtLeastOnePGARequirementOnAnyItemLine ?? ZBool.False;
					var oldTariffFormatted = ULI_TariffFormatted;

					base.ULI_Tariff = value;

					if (!IsCopying)
					{
						if (!oldValue.IsEmpty)
						{
							foreach (CusUSLVItemPGAWrapper pgaWrapper in ItemPGAWrapperCollection)
							{
								pgaWrapper.DisclaimReason = ZString.Empty;
								pgaWrapper.Indicator = ZString.Empty;
							}
						}

						OGARequirementCalculator.Initialise();
					}

					if (consignment != null)
					{
						if (consignment.HasAtLeastOnePGARequirementOnAnyItemLine != hasAtLeastOnePGARequirementOnAnyItemLine)
						{
							consignment.HasAtLeastOnePGARequirementOnAnyItemLineInfo.RefreshBinding(hasAtLeastOnePGARequirementOnAnyItemLine);
						}

						var item = consignment.FirstCusUSLVItem;
						if (object.ReferenceEquals(item, this))
						{
							consignment.FirstCusUSLVItemTariffInfo.RefreshBinding(oldTariffFormatted);
							consignment.Validation.ValidateFirstCusUSLVItemTariff();
						}
					}
				}

				ItemPGAWrapperCollection.RefreshBinding();
			}
		}

		#endregion

		public USCTariff Tariff
		{
			get
			{
				var today = ZDate.Today;
				return Factory.GetCachedValue(ULI_Tariff.PadRight(AutoUSCTariff.Schema.UE_TariffMaxLength) + today.ToString(), () => new USCTariff.Loader(Factory).LoadBestMatch(ULI_Tariff, today));
			}
		}

		#region ULI_TariffFormatted

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusUSLVItemLookups.Tariffs))]
		[MaxLength(Schema.ULI_TariffFormattedMaxLength)]
		public ZString ULI_TariffFormatted
		{
			get => TariffFormatter.DisplayFormat(ULI_Tariff);
			set
			{
				var oldValue = ULI_TariffFormatted;
				ULI_Tariff = value;
				ULI_TariffFormattedInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ULI_TariffFormattedInfo => GetWrappedZPropertyInfo(CusUSLVItem.Schema.ULI_TariffFormatted, (x) => ULI_TariffInfo);

		#endregion

		#region ULI_RX_NKCurrEXRate

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal ULI_RX_NKCurrEXRate => Currency?.GetCustomsRate(Consignment?.Shipment?.ULH_DepartureDate ?? ZDateTime.Empty) ?? ZDecimal.Zero;

		public virtual ZPropertyInfo ULI_RX_NKCurrEXRateInfo => GetZPropertyInfo(CusUSLVItem.Schema.ULI_RX_NKCurrEXRate);

		public int ExchangeRateDecimals => 6;

		#endregion

		#region ULI_RX_NKCurrency

		public override ZString ULI_RX_NKCurrency
		{
			get => base.ULI_RX_NKCurrency;
			set
			{
				var oldValue = ULI_RX_NKCurrency;
				base.ULI_RX_NKCurrency = value;
				if (!IsCopying && oldValue != ULI_RX_NKCurrency)
				{
					ULI_RX_NKCurrEXRateInfo.RefreshBinding();
					if (Consignment is CusUSLVConsignment consignment)
					{
						consignment.ULB_GoodsValueInfo.RefreshBinding();
						var item = consignment.FirstCusUSLVItem;
						if (object.ReferenceEquals(item, this))
						{
							consignment.FirstCusUSLVItemCurrencyInfo.RefreshBinding(oldValue);
							consignment.Validation.ValidateFirstCusUSLVItemCurrency();
						}
					}
				}
			}
		}

		#endregion

		public bool HasForeignCurrency => ULI_RX_NKCurrency != Consignment.ULB_Currency;

		#region ULI_ClusterKey

		public override ZInt ULI_ClusterKey
		{
			get => base.ULI_ClusterKey;
			set
			{
				base.ULI_ClusterKey = value;
				CusUSLVItemPGAs.OfType<CusUSLVItemPGA>().ForEach(x => x.ULP_ClusterKey = value);
			}
		}

		#endregion

		#region ULI_PartNo

		[List(nameof(Lookups) + "." + nameof(CusUSLVItemLookups.Products))]
		public override ZString ULI_PartNo
		{
			get => base.ULI_PartNo;
			set
			{
				using (GetValidationSuspender())
				{
					var oldValue = ULI_PartNo;
					if (!IsCopying && oldValue != value)
					{
						base.ULI_PartNo = value;
						PartSyncManager.Refresh();
						RefreshBindingOfConsignmentFirstCusUSLVItemPropertyIfNeeded(oldValue, () => ULI_PartNo, CusUSLVConsignment.Schema.FirstCusUSLVItemProductCode);
						Consignment?.Validation.ValidateFirstCusUSLVItemProductCode();
					}
				}
				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateULI_PartNo();
				}
			}
		}

		#endregion

		#region ULI_GoodsValue

		[DecimalPlaces(nameof(GoodsValueDecimals))]
		public override ZDecimal ULI_GoodsValue
		{
			get => base.ULI_GoodsValue;
			set
			{
				var oldValue = ULI_GoodsValue;
				base.ULI_GoodsValue = value;
				RefreshBindingOfConsignmentFirstCusUSLVItemPropertyIfNeeded(oldValue, () => ULI_GoodsValue, CusUSLVConsignment.Schema.FirstCusUSLVItemLineValue);
			}
		}

		public int GoodsValueDecimals => Currency != null ? Currency.Decimals : 2;

		#endregion

		#region ULI_GoodsDescription

		public override ZString ULI_GoodsDescription
		{
			get => base.ULI_GoodsDescription;
			set
			{
				var oldValue = ULI_GoodsDescription;
				base.ULI_GoodsDescription = value;
				RefreshBindingOfConsignmentFirstCusUSLVItemPropertyIfNeeded(oldValue, () => ULI_GoodsDescription, CusUSLVConsignment.Schema.FirstCusUSLVItemGoodsDescription);
				Consignment?.Validation.ValidateFirstCusUSLVItemGoodsDescription();
			}
		}

		#endregion

		#region ULI_RN_NKCountryOfOrigin

		public override ZString ULI_RN_NKCountryOfOrigin
		{
			get => base.ULI_RN_NKCountryOfOrigin;
			set
			{
				var oldValue = ULI_RN_NKCountryOfOrigin;
				base.ULI_RN_NKCountryOfOrigin = value;
				RefreshBindingOfConsignmentFirstCusUSLVItemPropertyIfNeeded(oldValue, () => ULI_RN_NKCountryOfOrigin, CusUSLVConsignment.Schema.FirstCusUSLVItemCountryOfOrigin);
				Consignment?.Validation.ValidateFirstCusUSLVItemCountryOfOrigin();
			}
		}

		#endregion

		#region ULI_AntiDumping

		public override ZBool ULI_AntiDumping
		{
			get => base.ULI_AntiDumping;
			set
			{
				var oldValue = ULI_AntiDumping;
				base.ULI_AntiDumping = value;
				RefreshBindingOfConsignmentFirstCusUSLVItemPropertyIfNeeded(oldValue, () => ULI_AntiDumping, CusUSLVConsignment.Schema.FirstCusUSLVItemAntiDumping);
				Consignment?.Validation.ValidateFirstCusUSLVItemAntiDumping();
			}
		}

		#endregion

		#region ULI_Countervailing

		public override ZBool ULI_Countervailing
		{
			get => base.ULI_Countervailing;
			set
			{
				var oldValue = ULI_Countervailing;
				base.ULI_Countervailing = value;
				RefreshBindingOfConsignmentFirstCusUSLVItemPropertyIfNeeded(oldValue, () => ULI_Countervailing, CusUSLVConsignment.Schema.FirstCusUSLVItemCountervailing);
				Consignment?.Validation.ValidateFirstCusUSLVItemCountervailing();
			}
		}

		#endregion

		internal bool ItemIsNotApplicable => !ULI_AntiDumping
					|| !ULI_Countervailing
					|| HasNonDisclaimableTariffsOrEmptyDisclaimReason
					|| TariffsHasTaxFeeCode;

		bool HasNonDisclaimableTariffsOrEmptyDisclaimReason => ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>()
			.Any(wrapper =>
				PGARequirementIndicator.IsPGAProgramRequired(wrapper.AgencyCode) ||
				(wrapper.DisclaimReason.IsEmpty && PGARequirementIndicator.IsPGAProgramMayRequired(wrapper.AgencyCode)));

		bool TariffsHasTaxFeeCode => !string.IsNullOrEmpty(Tariff?.TaxFeeCode);

		void RefreshBindingOfConsignmentFirstCusUSLVItemPropertyIfNeeded(IZType oldValue, Func<IZType> getNewValue, string propertyName)
		{
			if (!IsCopying)
			{
				var newValue = getNewValue();
				if (oldValue != newValue && Consignment is CusUSLVConsignment consignment)
				{
					var item = consignment.FirstCusUSLVItem;
					if (object.ReferenceEquals(item, this))
					{
						var info = consignment.ZPropertyInfoHash.GetPropertySafe(propertyName);
						info?.RefreshBinding(oldValue);
					}
				}
			}
		}

		public CusUSLVConsignment Consignment
		{
			get { return Factory.Load<CusUSLVConsignment>(ULI_ULB); }
		}

		public override void Delete()
		{
			CusUSLVItemPGAs.RemoveAndDeleteAll();
			// Caution - two base.Delete() in here...
			if (PartSyncManager != null)
			{
				bool originalEnabledState = PartSyncManager.Enabled;
				PartSyncManager.Enabled = false;
				base.Delete();
				if (!IsDeleted)
				{
					PartSyncManager.Enabled = originalEnabledState;
				}
			}
			else
			{
				base.Delete();
			}
		}

		#endregion

		public ZShort LineNumber { get; set; }

		#region IHaveAdditionalDataForBorderWise

		public AdditionalDataForBorderWise GetAdditionalDataForBorderWise(string bindingProperty)
		{
			return new AdditionalDataForBorderWise("I", ZDate.Today, x => TariffFormatter.DisplayFormat(x));
		}

		US.Business.TariffFormatter TariffFormatter => tariffFormatter ?? (tariffFormatter = new US.Business.TariffFormatter());
		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList => Lookups.Tariffs.TypeOfElements;

		US.Business.TariffFormatter tariffFormatter;

		#endregion

		#region ICargoReleaseCusEntryLine

		ZString ICargoReleaseCusEntryLine.UltimateConsigneeNumber => ZString.Empty;

		#endregion

		#region ICusEntryLine

		ZString US.Business.MessageBuilders.ICusEntryLine.GetRelevantTariffForFee(string feeType) => ZString.Empty;

		US.Business.MessageBuilders.ICusEntryLine US.Business.MessageBuilders.ICusEntryLine.ParentLine => null;

		ZString US.Business.MessageBuilders.ICusEntryLine.ImportTariffCode => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.ExportTariffCode => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.NAFTATariff => ZString.Empty;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.NAFTADutyRate => 0;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.NAFTADutyFGN => 0;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.NAFTADutyUS => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.ImportFTZNumber => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.CountryOfOrigin => ULI_RN_NKCountryOfOrigin;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.GrossWeightInKilograms => 0;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.ADDSpecificDepositValue => 0;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.CVDSpecificDepositValue => 0;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.Charges => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.PortOfLading => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.ZoneStatus => ZString.Empty;

		ZDate US.Business.MessageBuilders.ICusEntryLine.PrivilegedStatusFilingDate => ZDate.Empty;

		ZBool US.Business.MessageBuilders.ICusEntryLine.NAFTANetCostIndicator => false;

		ZInt US.Business.MessageBuilders.ICusEntryLine.FTZLineItemQuantity => 0;

		ZShort US.Business.MessageBuilders.ICusEntryLine.InvDelimter => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.PreImportationReviewProgramRulingsType => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.PreImportationReviewProgramRulingsNumber => ZString.Empty;

		IEnumerable<ZString> US.Business.MessageBuilders.ICusEntryLine.CommercialDescriptions => null;

		ZString US.Business.MessageBuilders.ICusEntryLine.SpecialProgramsIndicatorPrimary => ZString.Empty;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.Quantity1 => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.UnitOfMeasure1 => ZString.Empty;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.Quantity2 => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.UnitOfMeasure2 => ZString.Empty;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.Quantity3 => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.UnitOfMeasure3 => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.CountryOfExport => ZString.Empty;

		ZDate US.Business.MessageBuilders.ICusEntryLine.DateOfExportation => ZDate.Empty;

		ZBool US.Business.MessageBuilders.ICusEntryLine.RelatedPartyIndicator => false;

		ZString US.Business.MessageBuilders.ICusEntryLine.SpecialProgramsIndicatorCountry => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.SpecialProgramsIndicatorSecondary => ZString.Empty;

		ZDate US.Business.MessageBuilders.ICusEntryLine.DateOfExportationFromCountryOfOrigin => ZDate.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.VisaNumber => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.TextileCategoryNumber => ZString.Empty;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.VisaQuantity => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.VisaUnitOfMeasure => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.AgricultureLicenseNumber => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.CottonCertificateNumberOrganicExemptionCertificateNumber => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.ChinaHongKongSWPMIndicator => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.CanadianExportCertificateSugar => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.WoolLicense => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.CBTPACertificationNumber => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.MiscellaneousPermitLicenseNumber => ZString.Empty;

		ZBool US.Business.MessageBuilders.ICusEntryLine.IsSoftwoodLumberLine => false;

		ZBool US.Business.MessageBuilders.ICusEntryLine.IsSupLine => false;

		ZBool US.Business.MessageBuilders.ICusEntryLine.IsSoftwoodLumberImporterDeclaration => false;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.SoftwoodLumberExportPrice => 0;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.SoftwoodLumberExportCharges => 0;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.CountervailingDuty => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.CountervailingCaseNumber => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.AntidumpingCaseNumber => ZString.Empty;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.AntidumpingDuty => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.ManufacturerSupplierCode => ZString.Empty;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.ExciseTax => 0;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.CVDDepositRate => 0;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.ADDDepositRate => 0;

		ZBool US.Business.MessageBuilders.ICusEntryLine.BondedCountervailingDuty => false;

		ZBool US.Business.MessageBuilders.ICusEntryLine.BondedAntidumpingDuty => false;

		ZString US.Business.MessageBuilders.ICusEntryLine.ADDCaseRateTypeQualifier => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.CVDCaseRateTypeQualifier => ZString.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.FTZCurrentTariff => ZString.Empty;

		IEnumerable<IFee> US.Business.MessageBuilders.ICusEntryLine.Fees => null;

		IEnumerable<ISecondaryTariffLine> US.Business.MessageBuilders.ICusEntryLine.SecondaryTariffLines => new List<ISecondaryTariffLine>();

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.SecondCustomsQuantity => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.SecondCustomsUnitQty => ZString.Empty;

		ZDecimal US.Business.MessageBuilders.ICusEntryLine.ThirdCustomsQuantity => 0;

		ZString US.Business.MessageBuilders.ICusEntryLine.ThirdCustomsUnitQty => ZString.Empty;

		ZDate US.Business.MessageBuilders.ICusEntryLine.DateForDutyCalculation => ZDate.Empty;

		ZString US.Business.MessageBuilders.ICusEntryLine.SelectedRateType => ZString.Empty;

		ZBool US.Business.MessageBuilders.ICusEntryLine.IsDisclaimSanction => false;

		IEnumerable<ISanctionsAdditionalInfo> US.Business.MessageBuilders.ICusEntryLine.SanctionsAdditionalInfos => Enumerable.Empty<ISanctionsAdditionalInfo>();

		ZDecimal Customs.Business.ICusEntryLine.CL_CustomsValue
		{
			get
			{
				var result = ULI_GoodsValueInUSD;

				if (0m < result && result < 0.5m)
				{
					return 1m;
				}
				else
				{
					return result.Round(0);
				}
			}
		}

		internal ZDecimal ULI_GoodsValueInUSD => Currency is null ? ULI_GoodsValue : CurrencyConverter.ConvertExact(new Money(ULI_GoodsValue, Currency), Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates)).Amount;

		ZShort Customs.Business.ICusEntryLine.CL_LineNumber => LineNumber;

		ZString Customs.Business.ICusEntryLine.Tariff => ULI_Tariff;

		ZDecimal Customs.Business.ICusEntryLine.CustomsQuantity => 0;

		ZString Customs.Business.ICusEntryLine.CustomsUnitQty => ZString.Empty;

		ZString Customs.Business.ICusEntryLine.Description => ULI_GoodsDescription;

		ZString Customs.Business.ICusEntryLine.ExtendedCommercialDescription => ZString.Empty;

		ZDecimal Customs.Business.ICusEntryLine.BondedWarehouseQuantity => 0;

		ZString Customs.Business.ICusEntryLine.BondedWarehouseUnitQuantity => ZString.Empty;

		ZString Customs.Business.ICusEntryLine.FormattedTariff => ZString.Empty;

		ZDecimal Customs.Business.ICusEntryLine.TotalLinePriceInLocalCurrency => 0;

		ZDecimal Customs.Business.ICusEntryLine.CL_DutyPercent => 0;

		ZString Customs.Business.ICusEntryLine.DutyRateDescription => ZString.Empty;

		ZDecimal Customs.Business.ICusEntryLine.CL_WarehouseUnitValue => 0;

		Money Customs.Business.ICusEntryLine.CustomsValue => Money.Empty;

		ZDecimal Customs.Business.ICusEntryLine.DutyAmount => 0;

		ZDecimal Customs.Business.ICusEntryLine.GSTVATAmount => 0;

		ZDecimal Customs.Business.ICusEntryLine.GSTVATDeferred => 0;

		Money Customs.Business.ICusEntryLine.TotalLinePrice => Money.Empty;

		ZString Customs.Business.ICusEntryLine.CL_ParentTrailer => ZString.Empty;

		Customs.Business.InvoiceLinesForEntryLineCollection Customs.Business.ICusEntryLine.InvoiceLines => null;

		BaseJobComInvoiceLine Customs.Business.ICusEntryLine.RandomLine => null;

		#endregion

		#region IGovernmentAgencies

		ZString IGovernmentAgencies.CommercialDescription => ULI_GoodsDescription;

		IEnumerable<IFSISLine> IGovernmentAgencies.FSISLines => new List<IFSISLine>();

		IEnumerable<IVNEData> IGovernmentAgencies.EPA_VNELines => new List<IVNEData>();

		IEnumerable<IPSTData> IGovernmentAgencies.EPA_PSTLines => new List<IPSTData>();

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFS370Lines => new List<INMFSLine>();

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSAMRLines => new List<INMFSLine>();

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSHMSLines => new List<INMFSLine>();

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSSIMLines => new List<INMFSLine>();

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSCOALines => new List<INMFSLine>();

		IEnumerable<INHTSAHeader> IGovernmentAgencies.NHTSALines => new List<INHTSAHeader>();

		IEnumerable<IAPHISHeader> IGovernmentAgencies.APHISHeaders => new List<IAPHISHeader>();

		IDDTCData IGovernmentAgencies.DDTCData => null;

		IEnumerable<IFDAData> IGovernmentAgencies.FDALines => new List<IFDAData>();

		IEnumerable<IAMSData> IGovernmentAgencies.AMSLines => new List<IAMSData>();

		ITSCAData IGovernmentAgencies.EPA_TSCAData => null;

		IPGADataCorrection IGovernmentAgencies.TSCADataCorrection => null;

		IPGADataCorrection IGovernmentAgencies.ODSDataCorrection => null;

		IEnumerable<ILaceyActCommon> IGovernmentAgencies.LaceyActData => new List<ILaceyActCommon>();

		IEnumerable<IATFData> IGovernmentAgencies.ATFLines => new List<IATFData>();

		IEnumerable<IOMCHeader> IGovernmentAgencies.OMCHeaders => new List<IOMCHeader>();

		IEnumerable<IFWSHeader> IGovernmentAgencies.FWSHeaders => new List<IFWSHeader>();

		IEnumerable<ITTBLine> IGovernmentAgencies.TTBLines => new List<ITTBLine>();

		IEnumerable<ICPSCHeader> IGovernmentAgencies.CPSCHeaders => new List<ICPSCHeader>();

		IEnumerable<IDEAHeader> IGovernmentAgencies.DEAHeaders => new List<IDEAHeader>();

		IEnumerable<IHFCHeader> IGovernmentAgencies.EPA_HFCHeaders => new List<IHFCHeader>();

		ZBool IGovernmentAgencies.ShouldIncludePGAInMessage(ZBool isCertified, ZString pgaCode)
		{
			return GovernmentAgencyProgramCodeList.IsPGAAllowed(false, true, isCertified, false, false, Consignment.ULB_EntryType, pgaCode);
		}

		#endregion

		#region IGovernmentAgenciesIndicators

		public CusUSLVItemPGAWrapper ODSWrapper => odsWrapper ?? (odsWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.ODS).Program));
		CusUSLVItemPGAWrapper odsWrapper;

		ZString IGovernmentAgenciesIndicators.ODSIndicator => ODSWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.ODSDisclaimReason => ODSWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper FSISWrapper => fsisWrapper ?? (fsisWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FSIS).Program));
		CusUSLVItemPGAWrapper fsisWrapper;

		ZString IGovernmentAgenciesIndicators.FSISIndicator => FSISWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.FSISDisclaimReason => FSISWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper VNEWrapper => vneWrapper ?? (vneWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.VNE).Program));
		CusUSLVItemPGAWrapper vneWrapper;

		ZString IGovernmentAgenciesIndicators.VNEIndicator => VNEWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.VNEDisclaimReason => VNEWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper PSTWrapper => pstWrapper ?? (pstWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.PST).Program));
		CusUSLVItemPGAWrapper pstWrapper;

		ZString IGovernmentAgenciesIndicators.PSTIndicator => PSTWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.PSTDisclaimReason => PSTWrapper.DisclaimReason;

		ZString IGovernmentAgenciesIndicators.PSTDisclaimProgram => PSTWrapper.AgencyProgram;

		public CusUSLVItemPGAWrapper NMFS370Wrapper => nmfs370Wrapper ?? (nmfs370Wrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes._370).Program));
		CusUSLVItemPGAWrapper nmfs370Wrapper;

		ZString IGovernmentAgenciesIndicators.NMFS370Indicator => NMFS370Wrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.NMFS370DisclaimReason => NMFS370Wrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper NMFSAMRWrapper => nmfsAMRWrapper ?? (nmfsAMRWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.AMR).Program));
		CusUSLVItemPGAWrapper nmfsAMRWrapper;

		ZString IGovernmentAgenciesIndicators.NMFSAMRIndicator => NMFSAMRWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.NMFSAMRDisclaimReason => NMFSAMRWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper NMFSHMSWrapper => nmfsHMSWrapper ?? (nmfsHMSWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.HMS).Program));
		CusUSLVItemPGAWrapper nmfsHMSWrapper;

		ZString IGovernmentAgenciesIndicators.NMFSHMSIndicator => NMFSHMSWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.NMFSHMSDisclaimReason => NMFSHMSWrapper.DisclaimReason;

		ZString IGovernmentAgenciesIndicators.NMFSSIMIndicator => ZString.Empty;

		ZString IGovernmentAgenciesIndicators.NMFSCOAIndicator => ZString.Empty;

		public CusUSLVItemPGAWrapper ACEFDAWrapper => aceFDAWrapper ?? (aceFDAWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FDA).Program));
		CusUSLVItemPGAWrapper aceFDAWrapper;

		ZString IGovernmentAgenciesIndicators.ACEFDAIndicator => ACEFDAWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.ACEFDADisclaimReason => ACEFDAWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper TSCAWrapper => tscaWrapper ?? (tscaWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.TSCA).Program));
		CusUSLVItemPGAWrapper tscaWrapper;

		ZString IGovernmentAgenciesIndicators.TSCAIndicator => TSCAWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.TSCADisclaimReason => TSCAWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper AMSWrapper => amsWrapper ?? (amsWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.AMS).Program));
		CusUSLVItemPGAWrapper amsWrapper;

		public CusUSLVItemPGAWrapper NOPWrapper => nopWrapper ?? (nopWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.NOP).Program));
		CusUSLVItemPGAWrapper nopWrapper;

		ZString IGovernmentAgenciesIndicators.AMSIndicator => AMSWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.AMSDisclaimReason => AMSWrapper.DisclaimReason;

		ZString IGovernmentAgenciesIndicators.AMSDisclaimProgram => AMSWrapper.AgencyProgram;

		ZString IGovernmentAgenciesIndicators.NOPIndicator => NOPWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.NOPDisclaimReason => NOPWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper NHTSAWrapper => nhtsaWrapper ?? (nhtsaWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.NHTSA).Program));
		CusUSLVItemPGAWrapper nhtsaWrapper;

		ZString IGovernmentAgenciesIndicators.NHTSAIndicator => NHTSAWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.NHTSADisclaimReason => NHTSAWrapper.DisclaimReason;

		ZString IGovernmentAgenciesIndicators.ATFIndicator => ZString.Empty;

		public CusUSLVItemPGAWrapper TTBWrapper => ttbWrapper ?? (ttbWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.TTB).Program));
		CusUSLVItemPGAWrapper ttbWrapper;

		ZString IGovernmentAgenciesIndicators.TTBIndicator => TTBWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.TTBDisclaimReason => TTBWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper OMCWrapper => omcWrapper ?? (omcWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.OMC).Program));
		CusUSLVItemPGAWrapper omcWrapper;

		ZString IGovernmentAgenciesIndicators.OMCIndicator => OMCWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.OMCDisclaimReason => OMCWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper LaceyActWrapper => laceyActWrapper ?? (laceyActWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.Lacey).Program));
		CusUSLVItemPGAWrapper laceyActWrapper;

		ZString IGovernmentAgenciesIndicators.LaceyActIndicator => LaceyActWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.LaceyActDisclaimReason => LaceyActWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper APHISWrapper => aphisWrapper ?? (aphisWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.APHIS).Program));
		CusUSLVItemPGAWrapper aphisWrapper;

		ZString IGovernmentAgenciesIndicators.APHISIndicator => APHISWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.APHISDisclaimReason => APHISWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper FWSWrapper => fwsWrapper ?? (fwsWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FWS).Program));
		CusUSLVItemPGAWrapper fwsWrapper;

		ZString IGovernmentAgenciesIndicators.FWSIndicator => FWSWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.FWSDisclaimReason => FWSWrapper.DisclaimReason;

		ZString IGovernmentAgenciesIndicators.DDTCIndicator => ZString.Empty;

		public CusUSLVItemPGAWrapper CPSCWrapper => cpscWrapper ?? (cpscWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.CPSC).Program));
		CusUSLVItemPGAWrapper cpscWrapper;

		ZString IGovernmentAgenciesIndicators.CPSCIndicator => CPSCWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.CPSCDisclaimReason => CPSCWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper DEAWrapper => deaWrapper ?? (deaWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.DEA).Program));
		CusUSLVItemPGAWrapper deaWrapper;

		ZString IGovernmentAgenciesIndicators.DEAIndicator => DEAWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.DEADisclaimReason => DEAWrapper.DisclaimReason;

		public CusUSLVItemPGAWrapper HFCWrapper => hfcWrapper ?? (hfcWrapper = (CusUSLVItemPGAWrapper)ItemPGAWrapperCollection.FirstOrDefault(x => ((CusUSLVItemPGAWrapper)x).AgencyProgram == RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.HFC).Program));
		CusUSLVItemPGAWrapper hfcWrapper;

		ZString IGovernmentAgenciesIndicators.HFCIndicator => HFCWrapper.Indicator;

		ZString IGovernmentAgenciesIndicators.HFCDisclaimReason => HFCWrapper.DisclaimReason;

		#endregion

		#region IOGA

		IList<IPriorNoticeLine> IOGA.FDA => new List<IPriorNoticeLine>();

		ZString IOGA.FDAIndicator => ZString.Empty;

		IEnumerable<IDOT> IOGA.DOT => new List<IDOT>();

		ZString IOGA.DOTIndicator => ZString.Empty;

		#endregion

		#region IPGALineNumbers

		void IPGALineNumbers.ClearPGALineNumbers()
		{
			((IPGALineNumbers)this).EPAStartLineNumber = 0;
			((IPGALineNumbers)this).FSISStartLineNumber = 0;
			((IPGALineNumbers)this).NMFSStartLineNumber = 0;
			((IPGALineNumbers)this).FDAStartLineNumber = 0;
			((IPGALineNumbers)this).TTBStartLineNumber = 0;
			((IPGALineNumbers)this).NHTSAStartLineNumber = 0;
			((IPGALineNumbers)this).AMSStartLineNumber = 0;
			((IPGALineNumbers)this).APHStartLineNumber = 0;
			((IPGALineNumbers)this).FWSStartLineNumber = 0;
			((IPGALineNumbers)this).ATFStartLineNumber = 0;
			((IPGALineNumbers)this).CPSCStartLineNumber = 0;
			((IPGALineNumbers)this).OMCStartLineNumber = 0;
			((IPGALineNumbers)this).DEAStartLineNumber = 0;
		}

		ZInt IPGALineNumbers.EPAStartLineNumber { get; set; }
		ZInt IPGALineNumbers.FSISStartLineNumber { get; set; }
		ZInt IPGALineNumbers.NMFSStartLineNumber { get; set; }
		ZInt IPGALineNumbers.FDAStartLineNumber { get; set; }
		ZInt IPGALineNumbers.TTBStartLineNumber { get; set; }
		ZInt IPGALineNumbers.NHTSAStartLineNumber { get; set; }
		ZInt IPGALineNumbers.AMSStartLineNumber { get; set; }
		ZInt IPGALineNumbers.APHStartLineNumber { get; set; }
		ZInt IPGALineNumbers.FWSStartLineNumber { get; set; }
		ZInt IPGALineNumbers.ATFStartLineNumber { get; set; }
		ZInt IPGALineNumbers.CPSCStartLineNumber { get; set; }
		ZInt IPGALineNumbers.OMCStartLineNumber { get; set; }
		ZInt IPGALineNumbers.DEAStartLineNumber { get; set; }

		#endregion

		#region ISimplifiedEntryLine

		IEnumerable<ISimplifiedEntryOrganisationDetails> ISimplifiedEntryLine.Entities => new List<ISimplifiedEntryOrganisationDetails>();

		#endregion

		#region IADDCVDLiability

		ZBool IADDCVDLiability.IsSetXLine => false;

		ZBool IADDCVDLiability.US_ADD_NA { get => false; }
		ZBool IADDCVDLiability.US_CVD_NA { get => false; }

		bool IADDCVDLiability.IsCountryOfOriginCanada
		{
			get
			{
				var origin = ULI_RN_NKCountryOfOrigin;
				return CanadaProvinceTerritoryCodes.IsCanadianProvince(origin) || CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(origin) || origin == "CA";
			}
		}

		ZString IADDCVDLiability.US_UC_NKCountryOfOrigin { get => ULI_RN_NKCountryOfOrigin; }

		bool IADDCVDLiability.IsACE => true;

		ZDate IADDCVDLiability.EffectiveDateForDutyRate => ZDate.Today;

		bool IADDCVDLiability.IsEntrySummaryValidationMode => false;

		bool IADDCVDLiability.IsLVS => true;

		#endregion

		#region PartSynchronisationManager

		public JobComInvoiceLinePartSynchronisationManager PartSyncManager => fPartSyncManager ?? (fPartSyncManager = new JobComInvoiceLinePartSynchronisationManager(this));
		JobComInvoiceLinePartSynchronisationManager fPartSyncManager;

		#endregion

		#region IInvoiceLinePartDetails
		ZString IInvoiceLinePartDetails.CustomsCountryCode => Core.Constants.CountryCodes.UnitedStates;
		internal void RefreshPartSyncManagerActiveDeciderPK()
		{
			partSyncManagerActiveDeciderPK = null;
		}

		ZGuid IInvoiceLinePartDetails.PartSyncManagerActiveDeciderPK
		{
			get
			{
				if (!partSyncManagerActiveDeciderPK.HasValue)
				{
					partSyncManagerActiveDeciderPK = Consignment?.Shipment?.PK ?? ZGuid.Empty;
				}
				return partSyncManagerActiveDeciderPK.Value;
			}
		}
		ZGuid? partSyncManagerActiveDeciderPK;

		void IInvoiceLinePartDetails.UpdateDetailsOnPartChange()
		{
			if (!IsCopying && !IsDeleted && !IsNull)
			{
				UpdateProductAndPivot();
			}
		}

		OrgHeader IInvoiceLinePartDetails.Importer => Consignment?.Consignee?.Header;

		OrgHeader IInvoiceLinePartDetails.Supplier => Consignment?.Seller?.Header;

		BaseJobComInvoiceHeader IInvoiceLinePartDetails.Header => null;

		bool IInvoiceLinePartDetails.IsForImportSectionOfDrawback => false;

		bool IInvoiceLinePartDetails.IsForExportSectionOfDrawback => false;

		bool IInvoiceLinePartDetails.IsDrawback => false;

		bool IInvoiceLinePartDetails.Enabled => true;

		ZGuid IInvoiceLinePartDetails.PartPK { get; set; }

		ZString IInvoiceLinePartDetails.PartNo => ULI_PartNo;

		RefCountry IInvoiceLinePartDetails.InvoiceCountry => GlbCompany.CurrentCompany.Country;

		bool IInvoiceLinePartDetails.JustUpdatedByDataRefresh => false;

		Type IInvoiceLinePartDetails.TypeOfPartUsed => MasterFiles.Business.OrgSupplierPartTypeDecider.GetOrgSupplierPartType(Core.Constants.CountryCodes.UnitedStates);

		#endregion

		#region IInvoiceHeaderForProductCreation
		bool IInvoiceHeaderForProductCreation.HasImportDeclaration => true;

		bool IInvoiceHeaderForProductCreation.HasExportDeclration => false;

		bool IInvoiceHeaderForProductCreation.InvoicesContainNotPersistentActiveProductWithMissingInvoiceUQ => false;

		bool IInvoiceHeaderForProductCreation.IsInwardBondedWarehousingEnabled => false;

		OrgHeader IInvoiceHeaderForProductCreation.Importer_Effective => Consignment?.Consignee?.Header;

		OrgHeader IInvoiceHeaderForProductCreation.Supplier_Effective => Consignment?.Seller?.Header;

		ZString IInvoiceHeaderForProductCreation.InvoiceNumber => ZString.Empty;

		ZString IInvoiceHeaderForProductCreation.FinalDestinationCountryCode => Consignment?.Shipment?.ULH_PortOfDischarge.Left(2) ?? ZString.Empty;
		ZString IInvoiceHeaderForProductCreation.BranchCompanyCountryCode => Consignment?.Shipment?.Branch?.Company.GC_RN_NKCountryCode ?? ZString.Empty;

		#endregion

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			var result = false;
			if (!property.IsReadOnly && Consignment != null && !Consignment.CE_EntryLineReference.IsEmpty)
			{
				result = true;
			}

			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}
	}
}
