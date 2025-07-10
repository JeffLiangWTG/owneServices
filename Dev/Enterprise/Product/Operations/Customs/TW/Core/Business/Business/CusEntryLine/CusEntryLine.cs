using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusEntryLine : IReconcileCandidate
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : Customs.Business.CusEntryLine.Schema
		{
			public const string CL_Procedure = nameof(CusEntryLine.CL_Procedure);
			public const string CL_TariffExtensionCode = nameof(CusEntryLine.CL_TariffExtensionCode);
			public const string CL_EntryLineQty = nameof(CusEntryLine.CL_EntryLineQty);
			public const string CL_EntryLineUQDescription = nameof(CusEntryLine.CL_EntryLineUQDescription);
			public const string CL_EntryLineUnitPrice = nameof(CusEntryLine.CL_EntryLineUnitPrice);
			public const string CL_EntryLineUQ = nameof(CusEntryLine.CL_EntryLineUQ);
			public const string CL_Calc_NetWeightInKG = nameof(CusEntryLine.CL_Calc_NetWeightInKG);
			public const string CL_Calc_CustomsSecondQuantity = nameof(CusEntryLine.CL_Calc_CustomsSecondQuantity);
			public const string CL_CustomsSecondUnitQty = nameof(CusEntryLine.CL_CustomsSecondUnitQty);
			public const string CL_BondedGoodsCode = nameof(CusEntryLine.CL_BondedGoodsCode);
			public const string CL_EnvironmentalProtectionCode = nameof(CusEntryLine.CL_EnvironmentalProtectionCode);
			public const string CL_Calc_GoodsDescription = nameof(CusEntryLine.CL_Calc_GoodsDescription);
			public const string CL_Calc_EntryLineGroup = nameof(CusEntryLine.CL_Calc_EntryLineGroup);
			public const string CL_Calc_AdditionalDutyAmount = nameof(CusEntryLine.CL_Calc_AdditionalDutyAmount);
			public const string CL_Calc_AntiDumpingDutyAmount = nameof(CusEntryLine.CL_Calc_AntiDumpingDutyAmount);
			public const string CL_Calc_BusinessTaxAmount = nameof(CusEntryLine.CL_Calc_BusinessTaxAmount);
			public const string CL_Calc_CommodityTaxCashAmount = nameof(CusEntryLine.CL_Calc_CommodityTaxCashAmount);
			public const string CL_Calc_CommodityTaxNonCashAmount = nameof(CusEntryLine.CL_Calc_CommodityTaxNonCashAmount);
			public const string CL_Calc_CountervailingDutyAmount = nameof(CusEntryLine.CL_Calc_CountervailingDutyAmount);
			public const string CL_Calc_HealthAndWelfareSurchargeAmount = nameof(CusEntryLine.CL_Calc_HealthAndWelfareSurchargeAmount);
			public const string CL_Calc_ImportDutyCashAmount = nameof(CusEntryLine.CL_Calc_ImportDutyCashAmount);
			public const string CL_Calc_ImportDutyNonCashAmount = nameof(CusEntryLine.CL_Calc_ImportDutyNonCashAmount);
			public const string CL_Calc_RetaliatoryDutyAmount = nameof(CusEntryLine.CL_Calc_RetaliatoryDutyAmount);
			public const string CL_Calc_SpecificallySelectedGoodsAndServicesTaxAmount = nameof(CusEntryLine.CL_Calc_SpecificallySelectedGoodsAndServicesTaxAmount);
			public const string CL_Calc_TobaccoAndAlcoholTaxAmount = nameof(CusEntryLine.CL_Calc_TobaccoAndAlcoholTaxAmount);
			public const string CL_Calc_TradePromotionFeeAmount = nameof(CusEntryLine.CL_Calc_TradePromotionFeeAmount);
			public const string CL_AssignedNumber = nameof(CusEntryLine.CL_AssignedNumber);
			public const string CL_Model = nameof(CusEntryLine.CL_Model);
			public const string CL_BrandName = nameof(CusEntryLine.CL_BrandName);
			public const string CL_Permit = nameof(CusEntryLine.CL_Permit);
			public const string CL_DGCode = nameof(CusEntryLine.CL_DGCode);
			public const string CL_Specification = nameof(CusEntryLine.CL_Specification);
			public const string CL_SupplierPartNumber = nameof(CusEntryLine.CL_SupplierPartNumber);
			public const string CL_CustomsOwnerPartNo = nameof(CusEntryLine.CL_CustomsOwnerPartNo);
			public const string CL_GoodsOrigin = nameof(CusEntryLine.CL_GoodsOrigin);
			public const string CL_CertificateOfOriginNumber = nameof(CusEntryLine.CL_CertificateOfOriginNumber);
			public const string CL_PreviousEntryNumber = nameof(CusEntryLine.CL_PreviousEntryNumber);
			public const string CL_PreviousBondedEntryNumber = nameof(CusEntryLine.CL_PreviousBondedEntryNumber);
			public const string CL_ChineseDescription = nameof(CusEntryLine.CL_ChineseDescription);
			public const string CL_EnglishDescription = nameof(CusEntryLine.CL_EnglishDescription);
			public const string CL_SHTCImportPermit = nameof(CusEntryLine.CL_SHTCImportPermit);
			public const string CL_CITESImportPermit = nameof(CusEntryLine.CL_CITESImportPermit);
			public const string CL_Preference = nameof(CusEntryLine.CL_Preference);
			public const string CL_CatalyticConverter = nameof(CusEntryLine.CL_CatalyticConverter);
			public const string CL_StandardEquipment = nameof(CusEntryLine.CL_StandardEquipment);
			public const string CL_CarType = nameof(CusEntryLine.CL_CarType);
			public const string CL_Transmission = nameof(CusEntryLine.CL_Transmission);
			public const string CL_EngineType = nameof(CusEntryLine.CL_EngineType);
			public const string CL_LeftSideSteering = nameof(CusEntryLine.CL_LeftSideSteering);
			public const string CL_CarCondition = nameof(CusEntryLine.CL_CarCondition);
			public const string CL_ModelYear = nameof(CusEntryLine.CL_ModelYear);
			public const string CL_Displacement = nameof(CusEntryLine.CL_Displacement);
			public const string CL_NumberOfDoor = nameof(CusEntryLine.CL_NumberOfDoor);
			public const string CL_NumberOfSeat = nameof(CusEntryLine.CL_NumberOfSeat);
			public const string CL_NumberOfCylinder = nameof(CusEntryLine.CL_NumberOfCylinder);
			public const string CL_NumberOfGear = nameof(CusEntryLine.CL_NumberOfGear);
		}

		#region Implementation

		public JobComInvoiceLine FirstInvoiceLine => Factory.GetValue(ref firstInvoiceLineCached, () => (JobComInvoiceLine)FirstLine);
		CachedProperty<JobComInvoiceLine> firstInvoiceLineCached;

		public ZString CL_Procedure => FirstInvoiceLine.JI_Procedure;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_TariffExtensionCode", Caption = "Tariff Extension Code", ShortCaption = "Tariff Ext. Code")]
		public ZString CL_TariffExtensionCode => FirstInvoiceLine.JI_TariffExtensionCode;

		public ZString CL_Grouping => FirstInvoiceLine.JI_Group;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_AssignedNumber", Caption = "Assigned Number")]
		public ZString CL_AssignedNumber => ZString.Join("|", FirstInvoiceLine.AssignedJobComInvLineRefsCollection.Select(x => x.JG_ReferenceNumber).ToArray());

		public ZPropertyInfo CL_AssignedNumberInfo => GetZPropertyInfo(nameof(CL_AssignedNumber));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Model", Caption = "Model")]
		public ZString CL_Model => FirstInvoiceLine.JI_Model;

		public ZPropertyInfo CL_ModelInfo => GetZPropertyInfo(nameof(CL_Model));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_BrandName", Caption = "Brand")]
		public ZString CL_BrandName => FirstInvoiceLine.JI_BrandName;

		public ZPropertyInfo CL_BrandNameInfo => GetZPropertyInfo(nameof(CL_BrandName));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Permit", Caption = "Permit")]
		public ZString CL_Permit => ZString.Join("|", InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(invoiceLine => invoiceLine.PermitCusSupportingCollection.Cast<PermitCusSupporting>().
					Where(x => !x.CSI_ReferenceNumber.IsEmpty).GroupBy(x => new { x.CSI_ReferenceNumber, x.CSI_LineNo }).Select(x => x.First()).Select(x => ZString.Format("{0}-{1}", x.CSI_ReferenceNumber, x.CSI_LineNo))).ToArray());

		public ZPropertyInfo CL_PermitInfo => GetZPropertyInfo(nameof(CL_Permit));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_DGCode", Caption = "DG Code")]
		public ZString CL_DGCode => FirstInvoiceLine.JI_HazMatCode;

		public ZPropertyInfo CL_DGCodeInfo => GetZPropertyInfo(nameof(CL_DGCode));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Specification", Caption = "Specification")]
		public ZString CL_Specification => FirstInvoiceLine.JI_Compositions;

		public ZPropertyInfo CL_SpecificationInfo => GetZPropertyInfo(nameof(CL_Specification));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_SupplierPartNumber", Caption = "Supplier Part Number")]
		public ZString CL_SupplierPartNumber => FirstInvoiceLine.JI_CustomsSupplierPartNo;

		public ZPropertyInfo CL_SupplierPartNumberInfo => GetZPropertyInfo(nameof(CL_SupplierPartNumber));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_CustomsOwnerPartNo", Caption = "Owner Part Number")]
		public ZString CL_CustomsOwnerPartNo => FirstInvoiceLine.JI_CustomsOwnerPartNo;

		public ZPropertyInfo CL_CustomsOwnerPartNoInfo => GetZPropertyInfo(nameof(CL_CustomsOwnerPartNo));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_GoodsOrigin", Caption = "Goods Origin")]
		public ZString CL_GoodsOrigin => FirstInvoiceLine.JI_CountryOfOrigin;

		public ZPropertyInfo CL_GoodsOriginInfo => GetZPropertyInfo(nameof(CL_GoodsOrigin));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_CertificateOfOriginNumber", Caption = "Certification of Origin")]
		public ZString CL_CertificateOfOriginNumber
		{
			get
			{
				var resultBuilder = new ZStringBuilder();
				resultBuilder.AppendIfNotEmpty(FirstInvoiceLine.CertificateOfOriginNumber);
				var lineNumber = FirstInvoiceLine.CertificateOfOriginNumberItemNumber;
				if (!lineNumber.IsEmpty)
				{
					resultBuilder.Append(lineNumber.ToString());
				}
				return resultBuilder.ToStringWithDelimiterBetweenAppends("-");
			}
		}

		public ZPropertyInfo CL_CertificateOfOriginNumberInfo => GetZPropertyInfo(nameof(CL_CertificateOfOriginNumber));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_PreviousEntryNumber", Caption = "Previous Entry Number")]
		public ZString CL_PreviousEntryNumber
		{
			get
			{
				var resultBuilder = new ZStringBuilder();
				resultBuilder.AppendIfNotEmpty(FirstInvoiceLine.JI_PreviousEntryNumber);
				var lineNumber = FirstInvoiceLine.JI_PreviousEntryLineNumber;
				if (!lineNumber.IsEmpty)
				{
					resultBuilder.Append(lineNumber.ToString());
				}
				return resultBuilder.ToStringWithDelimiterBetweenAppends("-");
			}
		}

		public ZPropertyInfo CL_PreviousEntryNumberInfo => GetZPropertyInfo(nameof(CL_PreviousEntryNumber));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_PreviousBondedEntryNumber", Caption = "Previous Bounded Entry Number")]
		public ZString CL_PreviousBondedEntryNumber
		{
			get
			{
				var resultBuilder = new ZStringBuilder();
				resultBuilder.AppendIfNotEmpty(FirstInvoiceLine.PreviousBondedEntryNumber);
				var lineNumber = FirstInvoiceLine.PreviousBondedEntryLineNumber;
				if (!lineNumber.IsEmpty)
				{
					resultBuilder.Append(lineNumber.ToString());
				}
				return resultBuilder.ToStringWithDelimiterBetweenAppends("-");
			}
		}

		public ZPropertyInfo CL_PreviousBondedEntryNumberInfo => GetZPropertyInfo(nameof(CL_PreviousBondedEntryNumber));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_ChineseDescription", Caption = "Chinese Description")]
		public ZString CL_ChineseDescription => FirstInvoiceLine.JI_NDescription;

		public ZPropertyInfo CL_ChineseDescriptionInfo => GetZPropertyInfo(nameof(CL_ChineseDescription));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_EnglishDescription", Caption = "English Description")]
		public ZString CL_EnglishDescription => FirstInvoiceLine.JI_Description;

		public ZPropertyInfo CL_EnglishDescriptionInfo => GetZPropertyInfo(nameof(CL_EnglishDescription));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_SHTCImportPermit", Caption = "SHTC Import Permit")]
		public ZString CL_SHTCImportPermit => FirstInvoiceLine.HighTechLicense;

		public ZPropertyInfo CL_SHTCImportPermitInfo => GetZPropertyInfo(nameof(CL_SHTCImportPermit));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_CITESImportPermit", Caption = "CITES Import Permit")]
		public ZString CL_CITESImportPermit => FirstInvoiceLine.CitesPermit;

		public ZPropertyInfo CL_CITESImportPermitInfo => GetZPropertyInfo(nameof(CL_CITESImportPermit));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Preference", Caption = "Preference")]
		public ZString CL_Preference => FirstInvoiceLine.JI_PrimaryPreference;

		public ZPropertyInfo CL_PreferenceInfo => GetZPropertyInfo(nameof(CL_Preference));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_CatalyticConverter", Caption = "Catalytic Converter?")]
		public ZString CL_CatalyticConverter => FirstInvoiceLine.JI_HasCatalystConverter;

		public ZPropertyInfo CL_CatalyticConverterInfo => GetZPropertyInfo(nameof(CL_CatalyticConverter));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_StandardEquipment", Caption = "Standard Equipment")]
		public ZString CL_StandardEquipment => FirstInvoiceLine.JI_EquipmentPrintMode;

		public ZPropertyInfo CL_StandardEquipmentInfo => GetZPropertyInfo(nameof(CL_StandardEquipment));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_CarType", Caption = "Car Type")]
		public ZString CL_CarType => FirstInvoiceLine.JI_CarType;

		public ZPropertyInfo CL_CarTypeInfo => GetZPropertyInfo(nameof(CL_CarType));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Transmission", Caption = "Transmission")]
		public ZString CL_Transmission => FirstInvoiceLine.JI_Transmission;

		public ZPropertyInfo CL_TransmissionInfo => GetZPropertyInfo(nameof(CL_Transmission));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_EngineType", Caption = "Engine Type")]
		public ZString CL_EngineType => FirstInvoiceLine.JI_EngineType;

		public ZPropertyInfo CL_EngineTypeInfo => GetZPropertyInfo(nameof(CL_EngineType));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_LeftSideSteering", Caption = "Left Side Steering")]
		public ZString CL_LeftSideSteering => FirstInvoiceLine.JI_LHD;

		public ZPropertyInfo CL_LeftSideSteeringInfo => GetZPropertyInfo(nameof(CL_LeftSideSteering));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_CarCondition", Caption = "Condition")]
		public ZString CL_CarCondition => FirstInvoiceLine.JI_CarCondition;

		public ZPropertyInfo CL_CarConditionInfo => GetZPropertyInfo(nameof(CL_CarCondition));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_ModelYear", Caption = "Model Year")]
		public ZShort CL_ModelYear => FirstInvoiceLine.JI_ModelYear;

		public ZPropertyInfo CL_ModelYearInfo => GetZPropertyInfo(nameof(CL_ModelYear));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Displacement", Caption = "Displacement(cc)")]
		public ZString CL_Displacement => FirstInvoiceLine.JI_Displacement;

		public ZPropertyInfo CL_DisplacementInfo => GetZPropertyInfo(nameof(CL_Displacement));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_NumberOfDoor", Caption = "Number of Door")]
		public ZShort CL_NumberOfDoor => FirstInvoiceLine.JI_NumberOfDoor;

		public ZPropertyInfo CL_NumberOfDoorInfo => GetZPropertyInfo(nameof(CL_NumberOfDoor));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_NumberOfSeat", Caption = "Number of Seat")]
		public ZShort CL_NumberOfSeat => FirstInvoiceLine.JI_Seats;

		public ZPropertyInfo CL_NumberOfSeatInfo => GetZPropertyInfo(nameof(CL_NumberOfSeat));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_NumberOfCylinder", Caption = "Number of Cylinder")]
		public ZShort CL_NumberOfCylinder => FirstInvoiceLine.JI_Cylinders;

		public ZPropertyInfo CL_NumberOfCylinderInfo => GetZPropertyInfo(nameof(CL_NumberOfCylinder));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_NumberOfGear", Caption = "Number of Gear")]
		public ZShort CL_NumberOfGear => FirstInvoiceLine.JI_Gears;

		public ZPropertyInfo CL_NumberOfGearInfo => GetZPropertyInfo(nameof(CL_NumberOfGear));

		#endregion

		#region Properties
		public ZDecimal UnitCommodityTax
		{
			get
			{
				var ratesCodes = new List<ZString> { UniversalReferenceConstants.RefCusRateCodes.CTA, UniversalReferenceConstants.RefCusRateCodes.CTS };
				var result = Fees.Cast<CusEntryLineFee>().Where(x => ratesCodes.Contains(x.CF_ChargeType)).Sum(x => x.CF_ChargeAmount);
				var tariffQuantity = InvoiceQuantity;
				if (tariffQuantity.IsEmpty)
				{
					result = ZDecimal.Zero;
				}
				else
				{
					result /= tariffQuantity;
				}
				return result;
			}
		}

		public ZDecimal UnitCustomsQuantityInKG
		{
			get
			{
				var result = CustomsQuantityInKG;
				var tariffQuantity = InvoiceQuantity;
				if (tariffQuantity.IsEmpty)
				{
					result = ZDecimal.Zero;
				}
				else
				{
					result /= tariffQuantity;
				}
				return result;
			}
		}

		public List<string> AdditionalDocuments
		{
			get
			{
				var documentlist = new List<string>();
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (var document in invoiceLine.PermitCusSupportingCollection.Cast<PermitCusSupporting>().Where(x => !x.CSI_ReferenceNumber.IsEmpty || !x.CSI_LineNo.IsEmpty))
					{
						var documentId = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", document.CSI_ReferenceNumber, document.CSI_LineNo);
						if (!documentlist.Contains(documentId))
						{
							documentlist.Add(documentId);
						}
					}
				}
				return documentlist;
			}
		}

		ZBool MergeByForCondensedDeclaration => (Declaration?.JE_MergeBy ?? ZString.Empty) == MergeByCodeList.Codes.CondensedDeclaration && InvoiceLines.Count > 1;

		#region CL_EntryLineQty

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_EntryLineQty", Caption = "Quantity")]
		public ZDecimal CL_EntryLineQty => Factory.GetValue(ref entryLineQtyCached, () => MergeByForCondensedDeclaration ? 1m : InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_InvoiceQuantity));

		CachedProperty<ZDecimal> entryLineQtyCached;

		public ZPropertyInfo CL_EntryLineQtyInfo => GetZPropertyInfo(Schema.CL_EntryLineQty);
		#endregion

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_EntryLineUQ", Caption = "Unit of Quantity", ShortCaption = "UQ")]
		public ZString CL_EntryLineUQ
		{
			get
			{
				ZString unitPriceUQ;
				if (MergeByForCondensedDeclaration)
				{
					unitPriceUQ = Constants.UnitOfQuantityCodes.LOT;
				}
				else
				{
					var unitPriceUQCW1Code = FirstLine?.JI_InvoiceUQ ?? ZString.Empty;
					var result = TWRefCusMapper.MapCW1UnitPriceUQToCustomsCode(Factory, unitPriceUQCW1Code);
					unitPriceUQ = result.IsEmpty ? unitPriceUQCW1Code : result;
				}
				return unitPriceUQ;
			}
		}

		public ZPropertyInfo CL_EntryLineUQInfo => GetZPropertyInfo(Schema.CL_EntryLineUQ);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_EntryLineUQDescription", Caption = "UQ Description", ShortCaption = "UQ Desc.")]
		public ZString CL_EntryLineUQDescription => Lookups.InvoiceUQList.GetDescriptionFromCode(CL_EntryLineUQ);

		public ZPropertyInfo CL_EntryLineUQDescriptionInfo => GetZPropertyInfo(Schema.CL_EntryLineUQDescription);

		#region CL_Calc_UnitPriceForDeclaration

		[DecimalPlaces(6)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_EntryLineUnitPrice", Caption = "Unit Price")]
		public ZDecimal CL_EntryLineUnitPrice => Factory.GetValue(ref entryLineUnitPriceCached, GetEntryLineUnitPrice);

		CachedProperty<ZDecimal> entryLineUnitPriceCached;

		ZDecimal GetEntryLineUnitPrice()
		{
			ZDecimal result;
			if (MergeByForCondensedDeclaration)
			{
				var totalLinePriceMoney = Money.Empty;
				var entryHeader = Header;
				if (entryHeader != null)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						if (entryHeader.IsCurrencySameAsFirstInvoiceCurrency(invoiceLine.JI_RX_NKLinePriceCurr))
						{
							totalLinePriceMoney = CurrencyConverter.Add(totalLinePriceMoney, invoiceLine.JI_LinePriceMoney);
						}
						else
						{
							var linePriceInFirstInvoiceCurrency = CurrencyConverter.ConvertExact(invoiceLine.JI_LinePriceInLocalCurrencyMoney, entryHeader.FirstInvoiceCurrency);
							totalLinePriceMoney = CurrencyConverter.Add(totalLinePriceMoney, linePriceInFirstInvoiceCurrency);
						}
					}
				}
				result = totalLinePriceMoney.Amount;
			}
			else
			{
				result = FirstInvoiceLine.JI_EnteredUnitPrice;
			}
			return result;
		}

		public ZPropertyInfo CL_EntryLineUnitPriceInfo => GetZPropertyInfo(Schema.CL_EntryLineUnitPrice);

		#endregion

		public ZDecimal CustomsQuantityInKG => Factory.GetValue(ref customsQuantityInKGCached, GetCustomsQuantityInKG);

		CachedProperty<ZDecimal> customsQuantityInKGCached;

		ZDecimal GetCustomsQuantityInKG()
		{
			return InvoiceLines.Cast<JobComInvoiceLine>().Sum(invoiceLine => invoiceLine.CustomsQuantityInKG);
		}

		#region CL_Calc_NetWeightInKG
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_NetWeightInKG", Caption = "Net Weight (KG)", ShortCaption = "Net Wgt. (KG)")]
		public ZDecimal CL_Calc_NetWeightInKG => Factory.GetValue(ref netWeightInKGCached, () => EffectiveNetWeight.InKilogramsSafe);

		CachedProperty<ZDecimal> netWeightInKGCached;

		public ZPropertyInfo CL_Calc_NetWeightInKGInfo => GetZPropertyInfo(Schema.CL_Calc_NetWeightInKG);
		#endregion

		#region CL_CustomsSecondUnitQty

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_CustomsSecondUnitQty", Caption = "Statistical Quantity Unit", ShortCaption = "UQ")]
		public ZString CL_CustomsSecondUnitQty => FirstInvoiceLine.JI_CustomsSecondUnitQty;

		public ZPropertyInfo CL_CustomsSecondUnitQtyInfo => GetZPropertyInfo(Schema.CL_CustomsSecondUnitQty);

		#endregion

		#region CL_Calc_CustomsSecondQuantity

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_CustomsSecondQuantity", Caption = "Statistical Quantity", ShortCaption = "Stats. Qty")]
		public ZDecimal CL_Calc_CustomsSecondQuantity => Factory.GetValue(ref fCL_Calc_CustomsSecondQuantityCached, () => InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_CustomsSecondQuantity));

		CachedProperty<ZDecimal> fCL_Calc_CustomsSecondQuantityCached;

		public ZPropertyInfo CL_Calc_CustomsSecondQuantityInfo => GetZPropertyInfo(Schema.CL_Calc_CustomsSecondQuantity);

		#endregion

		#region CL_BondedGoodsCode

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_BondedGoodsCode", Caption = "Bonded Goods Code")]
		public ZString CL_BondedGoodsCode => FirstInvoiceLine.JI_BondedGoodsCode;

		public ZPropertyInfo CL_BondedGoodsCodeInfo => GetZPropertyInfo(Schema.CL_BondedGoodsCode);

		#endregion

		#region CL_EnvironmentalProtectionCode

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_EnvironmentalProtectionCode", Caption = "Environmental Protection Code", ShortCaption = "Env. Pro. Code")]
		public ZString CL_EnvironmentalProtectionCode => FirstInvoiceLine.JI_Calc_EnvironmentalProtectionCode;

		#endregion

		public override void RoundCustomsValue()
		{
			CL_CustomsValue = CL_CustomsValue.Round(0);
		}

		public ZString CL_Calc_EntryLineGroup
		{
			get
			{
				var builder = new ZStringBuilder();
				InvoiceLines.Cast<JobComInvoiceLine>().Select(line => line.JI_Group).Distinct().Where(group => !group.IsEmpty).ForEach(group => builder.Append(group));
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZPropertyInfo CL_Calc_EntryLineGroupInfo => GetZPropertyInfo(Schema.CL_Calc_EntryLineGroup);

		#region RAP ROR
		public ZBool IsRAPOrROR => FirstInvoiceLine.IsRAPOrROR;
		public ZBool IsRAP => FirstInvoiceLine.IsRAP;
		public ZBool IsROR => FirstInvoiceLine.IsROR;
		ZBool UseOneTenthCV => FirstInvoiceLine.JI_UseOneTenthCV;
		ZDecimal TenthCustomsValue => CL_CustomsValue * 0.1m;

		public ZDecimal CL_Calc_RAPRORPriceLocalAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (IsRAPOrROR)
				{
					result = UseOneTenthCV ? TenthCustomsValue : FirstInvoiceLine.JI_Calc_RAPRORPriceLocalAmount * (IsRAP ? 1m : Header.CH_RorCustomsFactor);
				}
				return result;
			}
		}

		public ZString CL_Calc_RAPRORUnitCurr => FirstInvoiceLine.JI_Calc_RAPRORUnitCurr;

		public ZDecimal CL_Calc_RAPRORUnitPrice
		{
			get
			{
				var result = ZDecimal.Zero;
				if (IsRAPOrROR)
				{
					var invoiceLine = FirstInvoiceLine;
					if (UseOneTenthCV)
					{
						result = TenthCustomsValue / InvoiceQuantity;
						if (CL_Calc_RAPRORUnitCurr != invoiceLine.LocalCurrency.Code)
						{
							var currencyRate = invoiceLine.InvoiceHeader?.JZ_InvoiceCurrExRate ?? ZDecimal.Zero;
							if (!currencyRate.IsEmpty)
							{
								result /= currencyRate;
							}
						}
					}
					else
					{
						result = invoiceLine.JI_Calc_RAPRORUnitPrice;
					}
				}
				return result;
			}
		}
		#endregion

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Description", Caption = "Goods Description", ShortCaption = "Goods Desc.")]
		public override ZString CL_Description { get => base.CL_Description; set => base.CL_Description = value; }

		public ZDecimal ReconciledCustomsValue
		{
			get => CL_CustomsValue;
			set => CL_CustomsValue = value;
		}

		public ZDecimal PreReconciledCustomsValue => CL_CustomsValue;

		public ZInt SecondarySortingValue => CL_LineNumber;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_GoodsDescription", Caption = "Goods Description", ShortCaption = "Goods Desc.")]
		public ZString CL_Calc_GoodsDescription
		{
			get
			{
				var builder = new ZStringBuilder();
				builder.Append(CL_Calc_EntryLineGroup);
				builder.Append(ZString.Empty);
				builder.Append(CL_Calc_GoodsDescriptionWithoutGrouping);
				return builder.ToStringWithNewLineBetweenAppends().Trim();
			}
		}

		public ZString CL_Calc_GoodsDescriptionWithoutGrouping => CL_Description.IsEmpty ? FirstInvoiceLine.JI_DeclarationGoodsDescription : CL_Description;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_AdditionalDutyAmount", Caption = "Additional Duty Amount", ShortCaption = "ADT Amt.")]
		public ZDecimal CL_Calc_AdditionalDutyAmount => Factory.GetValue(ref additionalDutyAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.ADT).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> additionalDutyAmountCached;

		public ZPropertyInfo CL_Calc_AdditionalDutyAmountInfo => GetZPropertyInfo(Schema.CL_Calc_AdditionalDutyAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_AntiDumpingDutyAmount", Caption = "Anti-Dumping Duty Amount", ShortCaption = "ADD Amt.")]
		public ZDecimal CL_Calc_AntiDumpingDutyAmount => Factory.GetValue(ref antiDumpingDutyAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.ADD).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> antiDumpingDutyAmountCached;

		public ZPropertyInfo CL_Calc_AntiDumpingDutyAmountInfo => GetZPropertyInfo(Schema.CL_Calc_AntiDumpingDutyAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_BusinessTaxAmount", Caption = "Business Tax", ShortCaption = "VAT Amt.")]
		public ZDecimal CL_Calc_BusinessTaxAmount => Factory.GetValue(ref businessTaxAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == RefCusTaxOrFeeCodes.VAT).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> businessTaxAmountCached;

		public ZPropertyInfo CL_Calc_BusinessTaxAmountInfo => GetZPropertyInfo(Schema.CL_Calc_BusinessTaxAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_CommodityTaxCashAmount", Caption = "Commodity Tax (CASH)", ShortCaption = "CT Amt. (CASH)")]
		public ZDecimal CL_Calc_CommodityTaxCashAmount => Factory.GetValue(ref commodityTaxCashAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => (x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.CTA || x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.CTS) && x.CF_MethodOfPayment == DutyTaxPaymentMethodList.Codes.CashPayment).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> commodityTaxCashAmountCached;

		public ZPropertyInfo CL_Calc_CommodityTaxCashAmountInfo => GetZPropertyInfo(Schema.CL_Calc_CommodityTaxCashAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_CommodityTaxNonCashAmount", Caption = "Commodity Tax (NON-CASH)", ShortCaption = "CT Amt. (NON-CASH)")]
		public ZDecimal CL_Calc_CommodityTaxNonCashAmount => Factory.GetValue(ref commodityTaxNonCashAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => (x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.CTA || x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.CTS) && x.CF_MethodOfPayment == DutyTaxPaymentMethodList.Codes.NonCashPayment).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> commodityTaxNonCashAmountCached;

		public ZPropertyInfo CL_Calc_CommodityTaxNonCashAmountInfo => GetZPropertyInfo(Schema.CL_Calc_CommodityTaxNonCashAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_CountervailingDutyAmount", Caption = "Countervailing Duty", ShortCaption = "CVD Amt.")]
		public ZDecimal CL_Calc_CountervailingDutyAmount => Factory.GetValue(ref countervailingDutyAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.CVD).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> countervailingDutyAmountCached;

		public ZPropertyInfo CL_Calc_CountervailingDutyAmountInfo => GetZPropertyInfo(Schema.CL_Calc_CountervailingDutyAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_HealthAndWelfareSurchargeAmount", Caption = "Health and Welfare Surcharge", ShortCaption = "HWS Amt.")]
		public ZDecimal CL_Calc_HealthAndWelfareSurchargeAmount => Factory.GetValue(ref healthAndWelfareSurchargeAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.HWS).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> healthAndWelfareSurchargeAmountCached;

		public ZPropertyInfo CL_Calc_HealthAndWelfareSurchargeAmountInfo => GetZPropertyInfo(Schema.CL_Calc_HealthAndWelfareSurchargeAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_ImportDutyCashAmount", Caption = "Import Duty (CASH)", ShortCaption = "DT Amt. (CASH)")]
		public ZDecimal CL_Calc_ImportDutyCashAmount => Factory.GetValue(ref importDutyCashAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => (x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.DTA || x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.DTS) && x.CF_MethodOfPayment == DutyTaxPaymentMethodList.Codes.CashPayment).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> importDutyCashAmountCached;

		public ZPropertyInfo CL_Calc_ImportDutyCashAmountInfo => GetZPropertyInfo(Schema.CL_Calc_ImportDutyCashAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_ImportDutyNonCashAmount", Caption = "Import Duty (NON-CASH)", ShortCaption = "DT Amt. (NON-CASH)")]
		public ZDecimal CL_Calc_ImportDutyNonCashAmount => Factory.GetValue(ref importDutyNonCashAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => (x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.DTA || x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.DTS) && x.CF_MethodOfPayment == DutyTaxPaymentMethodList.Codes.NonCashPayment).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> importDutyNonCashAmountCached;

		public ZPropertyInfo CL_Calc_ImportDutyNonCashAmountInfo => GetZPropertyInfo(Schema.CL_Calc_ImportDutyNonCashAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_RetaliatoryDutyAmount", Caption = "Retaliatory Duty", ShortCaption = "RTD Amt.")]
		public ZDecimal CL_Calc_RetaliatoryDutyAmount => Factory.GetValue(ref retaliatoryDutyAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.RTD).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> retaliatoryDutyAmountCached;

		public ZPropertyInfo CL_Calc_RetaliatoryDutyAmountInfo => GetZPropertyInfo(Schema.CL_Calc_RetaliatoryDutyAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_SpecificallySelectedGoodsAndServicesTaxAmount", Caption = "Specifically Selected Goods and Services Tax", ShortCaption = "SSG Amt.")]
		public ZDecimal CL_Calc_SpecificallySelectedGoodsAndServicesTaxAmount => Factory.GetValue(ref specificallySelectedGoodsAndServicesTaxAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.SSG).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> specificallySelectedGoodsAndServicesTaxAmountCached;

		public ZPropertyInfo CL_Calc_SpecificallySelectedGoodsAndServicesTaxAmountInfo => GetZPropertyInfo(Schema.CL_Calc_SpecificallySelectedGoodsAndServicesTaxAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_TobaccoAndAlcoholTaxAmount", Caption = "Tobacco and Alcohol Tax", ShortCaption = "TAT Amt.")]
		public ZDecimal CL_Calc_TobaccoAndAlcoholTaxAmount => Factory.GetValue(ref tobaccoAndAlcoholTaxAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.TAT).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> tobaccoAndAlcoholTaxAmountCached;

		public ZPropertyInfo CL_Calc_TobaccoAndAlcoholTaxAmountInfo => GetZPropertyInfo(Schema.CL_Calc_TobaccoAndAlcoholTaxAmount);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_Calc_TradePromotionFeeAmount", Caption = "Trade Promotion Fee", ShortCaption = "TPF Amt.")]
		public ZDecimal CL_Calc_TradePromotionFeeAmount => Factory.GetValue(ref tradePromotionFeeAmountCached, () => Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == RefCusTaxOrFeeCodes.TPF).Sum(x => x.CF_ChargeAmount));

		CachedProperty<ZDecimal> tradePromotionFeeAmountCached;

		public ZPropertyInfo CL_Calc_TradePromotionFeeAmountInfo => GetZPropertyInfo(Schema.CL_Calc_TradePromotionFeeAmount);
		#endregion

		protected override TariffFormatter GetTariffFormatter() => new TaiwanTariffFormatter();

		public ZDecimal CL_CustomsValueForCustomsValuation
		{
			get
			{
				var entryHeader = Header;
				var customsValueForCalculationInInvoiceCurrency = entryHeader.CH_CustomsFactor * CL_EntryLineUnitPrice * CL_EntryLineQty;
				var customsValueForCalculationInInvoiceCurrencyMoney = ConvertToLocalAmountRounded(new Money(customsValueForCalculationInInvoiceCurrency, entryHeader.FirstInvoiceCurrency));
				var result = customsValueForCalculationInInvoiceCurrencyMoney.Amount;
				return result < 1m ? 1 : result;
			}
		}

		[DecimalPlaces(0)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLine|CL_RAPOrRORCustomsValue", Caption = "ROR/RAP Customs Value", ShortCaption = "ROR/RAP CV")]
		public ZDecimal CL_RAPOrRORCustomsValue => CL_Calc_RAPRORPriceLocalAmount.Round(0);

		public ZDecimal CL_CustomsValueForDutyCalculation => IsRAPOrROR ? CL_RAPOrRORCustomsValue : CL_CustomsValueForCustomsValuation;

		public override ZGuid CL_CH
		{
			get => base.CL_CH;
			set
			{
				var oldValue = CL_CH;
				base.CL_CH = value;
				if (oldValue != CL_CH)
				{
					Fees.MarkAsNeedingValidation();
					Header.MarkAsNeedingValidation();
				}
			}
		}
	}
}
