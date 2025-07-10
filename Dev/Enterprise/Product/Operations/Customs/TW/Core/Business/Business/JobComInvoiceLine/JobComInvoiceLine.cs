using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.TW.Business.Constants;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobComInvoiceLine : AutoTWJobComInvoiceLine,
		Integration.Customs.ICusSupportingInfoTypeSupporter,
		Integration.Customs.ICusAddInfoTypeSupporter,
		Integration.Customs.TW.IJobComInvoiceLine,
		IJobComInvLineRefsTypeSupporter,
		Integration.Customs.ICusCodeDataTypeSupporter,
		ICurrencyConverterDataProviderWithFixedExRates,
		IReservedFieldSupporter,
		IReconcileCandidate,
		ISequenceNumberHeader,
		IDocAddresses,
		ISupportMultipleResourceStringData
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Light Validation

		protected override void MarkAsNeedingValidationCore()
		{
			base.MarkAsNeedingValidationCore();
			ChassisJobComInvLineRefsCollection.MarkAsNeedingValidation();
			AssignedJobComInvLineRefsCollection.MarkAsNeedingValidation();
			StorageAndShippingConditionJobComInvLineRefsCollection.MarkAsNeedingValidation();
		}

		#endregion

		public new class Schema : AutoTWJobComInvoiceLine.Schema
		{
			public const int PreviousPermitNoInfoMaxLength = 14;
			public const string JI_CEI_Description = "JI_CEI_Description";
			public new const int JI_DescriptionMaxLength = 512;
			public new const int JI_NDescriptionMaxLength = 512;
			public const string JI_TariffDescription = "JI_TariffDescription";
			public const string JI_DeclarationGoodsDescription = "JI_DeclarationGoodsDescription";
			public const string JI_Calc_RAPRORUnitPrice = "JI_Calc_RAPRORUnitPrice";
			public const string JI_Calc_RAPRORUnitCurr = "JI_Calc_RAPRORUnitCurr";
			public const int JI_Calc_RAPRORUnitCurrMaxLength = 3;
			public const string InvoiceHeaderDisplaySequence = "InvoiceHeader+JZ_InvoiceDisplaySequence";
			public const string PermitCusSupportingNo1 = "PermitCusSupportingNo1";
			public const string PermitCusSupportingLineNo1 = "PermitCusSupportingLineNo1";
			public const string PermitCusSupportingNo2 = "PermitCusSupportingNo2";
			public const string PermitCusSupportingLineNo2 = "PermitCusSupportingLineNo2";
			public const string PermitCusSupportingNo3 = "PermitCusSupportingNo3";
			public const string PermitCusSupportingLineNo3 = "PermitCusSupportingLineNo3";
			public const string PermitCusSupportingNo4 = "PermitCusSupportingNo4";
			public const string PermitCusSupportingLineNo4 = "PermitCusSupportingLineNo4";
			public const string PermitCusSupportingNo5 = "PermitCusSupportingNo5";
			public const string PermitCusSupportingLineNo5 = "PermitCusSupportingLineNo5";
			public const string AssignedNumber1 = "AssignedNumber1";
			public const string AssignedNumber2 = "AssignedNumber2";
			public const string PermitExemptionCode1 = "PermitExemptionCode1";
			public const string PermitExemptionCode2 = "PermitExemptionCode2";
			public const string PermitExemptionCode3 = "PermitExemptionCode3";
			public const string PermitExemptionCode4 = "PermitExemptionCode4";
			public const string PermitExemptionCode5 = "PermitExemptionCode5";
			public const string TWL_DocumentaryUQ = "AddInfoChild+TWL_DocumentaryUQ";
			public const string TWL_DocumentaryQty = "AddInfoChild+TWL_DocumentaryQty";
			public const string TWL_DocumentaryUnitPrice = "AddInfoChild+TWL_DocumentaryUnitPrice";
			public const string AlcoholTaxCashTariffCode = "AlcoholTaxCashTariffCode";
			public const string AlcoholTaxNonCashTariffCode = "AlcoholTaxNonCashTariffCode";
			public const string CommodityTaxCashTariffCode = "CommodityTaxCashTariffCode";
			public const string CommodityTaxNonCashTariffCode = "CommodityTaxNonCashTariffCode";
			public const string SpecialTaxCashTariffCode = "SpecialTaxCashTariffCode";
			public const string SpecialTaxNonCashTariffCode = "SpecialTaxNonCashTariffCode";
			public const string TobaccoTaxCashTariffCode = "TobaccoTaxCashTariffCode";
			public const string TobaccoTaxNonCashTariffCode = "TobaccoTaxNonCashTariffCode";
			public const string NX101ShippingMarks = "NX101ShippingMarks";
			public const string NX101PermitGoodsDescription = "NX101PermitGoodsDescription";
			public const string QuotaPermitNumber = "QuotaPermitNumber";
			public const string QuotaPermitNumberItemNumber = "QuotaPermitNumberItemNumber";
			public const string TWL_AircraftPartsCategory = "AddInfoChild+TWL_AircraftPartsCategory";
			public const string TWL_AircraftPartsCode = "AddInfoChild+TWL_AircraftPartsCode";
			public const string TWL_AircraftIPC = "AddInfoChild+TWL_AircraftIPC";
			public const string ReservedFieldCode1 = "ReservedFieldCode1";
			public const string ReservedFieldCode2 = "ReservedFieldCode2";
			public const string ReservedFieldValue1 = "ReservedFieldValue1";
			public const string ReservedFieldValue2 = "ReservedFieldValue2";
			public const string ManufacturerDocAddressOrgPK = "ManufacturerDocAddressOrgPK";
			public const string PreviousPermitNo = "PreviousPermitNo";
			public const string PartyIdentifier = nameof(JobComInvoiceLine.PartyIdentifier);
			public const string CertificateNo = nameof(JobComInvoiceLine.CertificateNo);
			public const string AuthorizedPerson = nameof(JobComInvoiceLine.AuthorizedPerson);
			public const string JI_PHValueNumeric = nameof(JobComInvoiceLine.JI_PHValueNumeric);
			public const string JI_SterilizationValueNumeric = nameof(JobComInvoiceLine.JI_SterilizationValueNumeric);
		}

		public JobTWComInvoiceLine AddInfoChild => this.LoadOrCreateAddInfoChild(ref addInfoChild);
		JobTWComInvoiceLine addInfoChild;

		#region IAddInfoChildSupporter
		protected override BusinessObject GetAddInfoChild() => AddInfoChild;
		protected override SchemaGuidColumn GetChildForeignKeyColumn() => JobTWComInvoiceLineSchema.TWL_JI;
		#endregion

		#region ISequenceNumberHeader Members

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(PermitCusSupportingCollection);

		HugeSequenceNumberGenerator permitItemNumberGenerator;
		internal HugeSequenceNumberGenerator PermitItemNumberGenerator => permitItemNumberGenerator ?? (permitItemNumberGenerator = new HugeSequenceNumberGenerator(this));

		#endregion

		#region JI_RAPPrice
		[ReadOnlyMember(nameof(JI_RAPRORPriceReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_RAPPrice", Caption = "RAP/ROR Price")]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|RAP|JI_RAPPrice", Caption = "RAP Price", MultipleKey = RapCaptionKey)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|ROR|JI_RAPPrice", Caption = "ROR Price", MultipleKey = RorCaptionKey)]
		public override ZDecimal JI_RAPPrice { get => base.JI_RAPPrice; set => base.JI_RAPPrice = value; }

		public bool IsRAP => Factory.GetValue(ref isRAP, () => IsImport && JI_Procedure.ToString() switch
		{
			ProcedureCodes._37 or ProcedureCodes._39 or ProcedureCodes._3F => true,
			_ => false,
		});
		CachedProperty<bool> isRAP;

		public bool IsROR => Factory.GetValue(ref isROR, () => IsImport && JI_Procedure.ToString() switch
		{
			ProcedureCodes._38 or ProcedureCodes._3E => true,
			_ => false,
		});
		CachedProperty<bool> isROR;

		public bool IsRAPOrROR => Factory.GetValue(ref isRepairOrRentalRoyaltyVisible, () => IsRAP || IsROR);
		CachedProperty<bool> isRepairOrRentalRoyaltyVisible;

		public ZBool JI_RAPRORPriceReadOnly => JI_UseOneTenthCV;

		public ZDecimal JI_Calc_RAPRORPriceLocalAmount => CurrencyConverter.ConvertExact(JI_RAPRORPriceMoney, LocalCurrency).Amount;

		Money JI_RAPRORPriceMoney
		{
			get
			{
				Money result = null;
				var currency = RAPRORPriceRefCurrency;
				if (currency != null && !JI_RAPPrice.IsEmpty)
				{
					result = new Money(JI_RAPPrice, currency);
				}
				else
				{
					result = Money.Empty;
				}
				return result;
			}
		}
		#endregion

		#region JI_RAPCurr
		[RelatedBusinessObject("RAPRORPriceRefCurrency")]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_RAPCurr", Caption = "Curr.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RAPRORCurrencyList))]
		[ReadOnlyMember(nameof(JI_RAPRORCurrReadOnly))]
		public override ZString JI_RAPCurr { get => base.JI_RAPCurr; set => base.JI_RAPCurr = value; }

		public RefCurrency RAPRORPriceRefCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, JI_RAPCurr); }
		}

		public ZBool JI_RAPRORCurrReadOnly => JI_UseOneTenthCV || !IsRAPOrROR;

		void SetDefaultUseOneTenthCV()
		{
			if (JI_Procedure.IsEmpty || !ShouldAllowOneTenthCVasRAPRORPrice)
			{
				JI_UseOneTenthCV = ZBool.False;
			}
			else if (IsROR)
			{
				JI_UseOneTenthCV = ZBool.True;
			}
		}

		public void SetDefaultRAPRORValuesIfNeeded()
		{
			JI_RAPCurr = IsRAPOrROR ? InvoiceHeader?.JZ_RX_NKInvoice_Currency ?? ZString.Empty : ZString.Empty;
			JI_RAPPrice = ZDecimal.Zero;
		}
		#endregion

		#region JI_Calc_RAPRORUnitPrice
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Calc_RAPRORUnitPrice", Caption = "RAP/ROR Unit Price", ShortCaption = "RAP/ROR U.P.")]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|RAP|JI_Calc_RAPRORUnitPrice", Caption = "RAP Unit Price", ShortCaption = "RAP U.P.", MultipleKey = RapCaptionKey)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|ROR|JI_Calc_RAPRORUnitPrice", Caption = "ROR Unit Price", ShortCaption = "ROR U.P.", MultipleKey = RorCaptionKey)]
		[ReadOnlyMember(nameof(JI_Calc_RAPRORUnitPriceReadOnly))]
		[DecimalPrecision(18)]
		[DecimalPlaces(6)]
		public ZDecimal JI_Calc_RAPRORUnitPrice
		{
			get
			{
				return JI_InvoiceQuantity.IsEmpty ? decimal.Zero : (JI_RAPPrice / JI_InvoiceQuantity);
			}
			set
			{
				var oldValue = JI_Calc_RAPRORUnitPrice;
				if (!IsCopying && oldValue != value)
				{
					JI_RAPPrice = value * JI_InvoiceQuantity;
					JI_Calc_RAPRORUnitPriceInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateJI_Calc_RAPRORUnitPrice();
				}
			}
		}

		public ZPropertyInfo JI_Calc_RAPRORUnitPriceInfo => GetZPropertyInfo(Schema.JI_Calc_RAPRORUnitPrice);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Calc_RAPRORUnitCurr", Caption = "Curr.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RAPRORCurrencyList))]
		public ZString JI_Calc_RAPRORUnitCurr => JI_RAPCurr;

		public ZPropertyInfo JI_Calc_RAPRORUnitCurrInfo => GetWrappedZPropertyInfo(nameof(JI_Calc_RAPRORUnitCurr), x => JI_RAPCurrInfo);

		public ZBool JI_Calc_RAPRORUnitPriceReadOnly => JI_UseOneTenthCV || !IsRAPOrROR;
		#endregion

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Calc_Invoice", Caption = "Inv. # for Line", ShortCaption = "Inv. No.", MediumCaption = "Invoice Number", FullDescription = "The invoice number of the invoice line.")]
		public override ZString JI_Calc_Invoice
		{
			get => base.JI_Calc_Invoice;
			set => base.JI_Calc_Invoice = value;
		}

		#region JI_UseOneTenthCV
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_UseOneTenthCV", Caption = "Use 10% CV", ShortCaption = "10% CV")]
		[ReadOnlyMember(nameof(JI_UseOneTenthCVReadOnly))]
		public override ZBool JI_UseOneTenthCV
		{
			get => base.JI_UseOneTenthCV;
			set
			{
				var oldValue = JI_UseOneTenthCV;
				base.JI_UseOneTenthCV = value;
				if (!IsCopying && oldValue != JI_UseOneTenthCV)
				{
					if (JI_UseOneTenthCV)
					{
						JI_RAPPrice = ZDecimal.Zero;
					}
					else
					{
						Validation.ValidateJI_RAPPrice();
					}
				}
			}
		}

		public ZBool JI_UseOneTenthCVReadOnly => !ShouldAllowOneTenthCVasRAPRORPrice;

		public ZBool ShouldAllowOneTenthCVasRAPRORPrice
		{
			get
			{
				var fJI_Procedure = JI_Procedure;
				return IsImport && (fJI_Procedure == Constants.ProcedureCodes._39 || fJI_Procedure == Constants.ProcedureCodes._37 || fJI_Procedure == Constants.ProcedureCodes._38 || fJI_Procedure == Constants.ProcedureCodes._3E);
			}
		}
		#endregion

		public override bool IsContainerLinkMandatory => ZBool.False;

		#region JI_TariffDescription
		public ZString JI_TariffDescription => GetJI_TariffDescription();

		ZString GetJI_TariffDescription()
		{
			var result = ZString.Empty;
			if (UniversalTariff != null)
			{
				var stringBuilder = new ZStringBuilder();
				AppendTariffAttributeValues(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, stringBuilder);
				AppendTariffAttributeValues(ImportExportRegulationsAttributeName, stringBuilder);
				result = stringBuilder.ToStringWithDelimiterBetweenAppends(", ");
			}
			return result;
		}

		void AppendTariffAttributeValues(ZString key, ZStringBuilder stringBuilder)
		{
			GetRegulationsAttributesByKey(key).ForEach(x => stringBuilder.Append(x.Code));
		}
		#endregion

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_TextileWidth", Caption = "Textile Width", FullDescription = "When the unit of the Customs Quantity is set to MTK - Square Meter, you need to enter the textile length in the Invoice Qty field and enter the textile width under the Other Details tab. The system will convert the unit of length and width into meter and then calculate the Customs Quantity automatically.")]
		public override ZDecimal JI_TextileWidth
		{
			get => base.JI_TextileWidth;
			set
			{
				var oldValue = JI_TextileWidth;
				base.JI_TextileWidth = value;
				if (!IsCopying && oldValue != JI_TextileWidth)
				{
					CustomsQuantity2Converter.CalculateCustomsFactorAndQty();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TextileWidthUQList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_TextileWidthUQ", Caption = "Textile Width UQ")]
		public override ZString JI_TextileWidthUQ
		{
			get => base.JI_TextileWidthUQ;
			set
			{
				var oldValue = JI_TextileWidthUQ;
				base.JI_TextileWidthUQ = value;
				if (!IsCopying && oldValue != JI_TextileWidthUQ)
				{
					CustomsQuantity2Converter.CalculateCustomsFactorAndQty();
				}
			}
		}

		#region Implementation

		#region New Properties

		internal bool IsL1Declaration => (EntryInstruction?.CEI_Style ?? ZString.Empty) == Constants.DeclarationTypes.Import.L1;

		internal ZString RORPaymentMethod => EntryInstruction?.CEI_RORPaymentMethod ?? ZString.Empty;

		#region JI_CEI_Description

		public ZString JI_CEI_Description => EntryInstruction?.CEI_Description ?? ZString.Empty;

		public ZPropertyInfo JI_CEI_DescriptionInfo => GetZPropertyInfo(Schema.JI_CEI_Description);

		#endregion

		public ZBool ShouldDisplayLabelGroupBox => IsForCAHeader20 || IsForCAHeaderCI || IsForCAHeader2Q;

		public ZBool IsShippingFromFactoryToDutyLevyingArea
		{
			get
			{
				var fJI_Procedure = JI_Procedure;
				return fJI_Procedure == Constants.ProcedureCodes._31 || fJI_Procedure == Constants.ProcedureCodes._35 || fJI_Procedure == Constants.ProcedureCodes._50;
			}
		}

		ZString GetInvoiceLineDescriptionFromCarInfo()
		{
			void AppendLine(ZStringBuilder build, List<Tuple<ZString, ZString>> list)
			{
				var shouldAppendLine = false;
				foreach (var tuple in list)
				{
					if (!tuple.Item2.IsEmpty)
					{
						if (!build.IsEmpty && shouldAppendLine)
						{
							build.Append(" ");
						}

						if (!shouldAppendLine)
						{
							shouldAppendLine = true;
						}

						build.AppendFormat("{0}: {1}", tuple.Item1, tuple.Item2);
					}
				}

				if (shouldAppendLine)
				{
					build.AppendLine();
				}
			}

			var result = new ZStringBuilder();
			var lineList = new List<Tuple<ZString, ZString>>()
			{
				new Tuple<ZString, ZString>((NoResString)"MODEL YEAR", JI_ModelYearStringFormatting),
				new Tuple<ZString, ZString>((NoResString)"CAR TYPE", CarTypeCodeListForPrinting.Instance.GetDescriptionFromCode(JI_CarType) ?? ZString.Empty),
				new Tuple<ZString, ZString>("DOOR", JI_NumberOfDoorStringFormatting)
			};
			AppendLine(result, lineList);

			lineList = new List<Tuple<ZString, ZString>>()
			{
				new Tuple<ZString, ZString>("BRAND", JI_BrandName),
				new Tuple<ZString, ZString>("MODEL", JI_Model)
			};
			AppendLine(result, lineList);

			lineList = new List<Tuple<ZString, ZString>>()
			{
				new Tuple<ZString, ZString>("DISPLACEMENT", JI_Displacement),
				new Tuple<ZString, ZString>("CYLINDER", JI_CylindersStringFormatting),
				new Tuple<ZString, ZString>("SEAT", JI_SeatsStringFormatting)
			};
			AppendLine(result, lineList);

			var leftSideSteering = ZString.Empty;
			switch (JI_LHD)
			{
				case YesNoList.Codes.Yes:
					leftSideSteering = YesNoList.Descriptions.Yes.ToUpper(CultureInfo.InvariantCulture);
					break;
				case YesNoList.Codes.No:
					leftSideSteering = YesNoList.Descriptions.No.ToUpper(CultureInfo.InvariantCulture);
					break;
			}

			if (!leftSideSteering.IsEmpty)
			{
				lineList = new List<Tuple<ZString, ZString>>() { new Tuple<ZString, ZString>((NoResString)"LEFT SIDE STEERING", leftSideSteering) };
				AppendLine(result, lineList);
			}

			lineList = new List<Tuple<ZString, ZString>>() { new Tuple<ZString, ZString>((NoResString)"ENGINE TYPE", EngineTypeCodeListForPrinting.Instance.GetDescriptionFromCode(JI_EngineType) ?? ZString.Empty) };
			AppendLine(result, lineList);

			lineList = new List<Tuple<ZString, ZString>>() { new Tuple<ZString, ZString>("TRANSMISSION", TransmissionCodeListForPrinting.Instance.GetDescriptionFromCode(JI_Transmission) ?? ZString.Empty) };
			AppendLine(result, lineList);

			var equipmentPrintMode = Lookups.EquipmentPrintModeList.GetDescriptionFromCode(JI_EquipmentPrintMode);
			if (!string.IsNullOrEmpty(equipmentPrintMode))
			{
				result.AppendLine(ZString.Format((NoResString)"STANDARD EQUIPMENT WITH {0}", equipmentPrintMode));
			}

			var chassisArray = ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>().Select(x => x.JG_ReferenceNumber).ToArray();
			lineList = new List<Tuple<ZString, ZString>>() { new Tuple<ZString, ZString>((NoResString)"CHASSIS NO", ZString.Join(";", chassisArray)) };
			AppendLine(result, lineList);

			return result.ToString();
		}

		public ZBool HasTariffCustomsRequirementsAttribute(ZString attributeCode)
		{
			return UniversalTariff.HasTariffCustomsRequirementsAttribute(attributeCode);
		}

		public ZString JI_Calc_EnvironmentalProtectionCode
		{
			get
			{
				var result = ZString.Empty;
				if (!JI_EPTDigit1.IsEmpty && !JI_EPTDigit2.IsEmpty && !JI_EPTDigit3.IsEmpty)
				{
					result = new ZStringBuilder(new ZString[] { JI_EPTDigit1, JI_EPTDigit2, JI_EPTDigit3 }).ToString();
				}
				return result;
			}
		}
		#endregion

		#region override Properties

		[MaxLength(50)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_BrandName", Caption = "Brand Name", FullDescription = "The trademark, brand name and other identification logo of the product. Mandatory for vehicles.")]
		public override ZString JI_BrandName { get => base.JI_BrandName; set => base.JI_BrandName = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Model", Caption = "Model", FullDescription = "The model of the product. Mandatory for vehicles and food containers.")]
		public override ZString JI_Model { get => base.JI_Model; set => base.JI_Model = value; }

		#region JI_ModelYear
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_ModelYear", Caption = "Model Year", FullDescription = "The model year of the car.")]
		public override ZShort JI_ModelYear { get => base.JI_ModelYear; set => base.JI_ModelYear = value; }

		ZString JI_ModelYearStringFormatting => CommonHelper.ZeroConvertToEmptyString(JI_ModelYear);
		#endregion

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CarTypeCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CarType", Caption = "Car Type", FullDescription = "The type of car, including the model number.")]
		public override ZString JI_CarType
		{
			get => base.JI_CarType;
			set
			{
				var oldValue = JI_CarType;
				base.JI_CarType = value;
				if (!IsCopying && oldValue != JI_CarType)
				{
					EmptyFieldsIfCarTypeIsH1();
					EmptyFieldsIfCarTypeIsJ1();
				}
			}
		}

		void EmptyFieldsIfCarTypeIsJ1()
		{
			if (IsJ1CarType)
			{
				JI_NumberOfDoor = ZShort.Zero;
				JI_Displacement = ZString.Empty;
				JI_Cylinders = ZShort.Zero;
				JI_Seats = ZShort.Zero;
				JI_Transmission = ZString.Empty;
				JI_HasCatalystConverter = ZString.Empty;
				JI_LHD = ZString.Empty;
			}
		}

		void EmptyFieldsIfCarTypeIsH1()
		{
			if (IsH1CarType)
			{
				JI_NumberOfDoor = ZShort.Zero;
				JI_LHD = ZString.Empty;
			}
		}

		public bool IsJ1CarType
		{
			get { return JI_CarType == CarTypeCodeList.Codes.J1; }
		}
		public bool IsH1CarType
		{
			get { return JI_CarType == CarTypeCodeList.Codes.H1; }
		}

		public bool IsJ1OrH1CarType
		{
			get { return IsJ1CarType || IsH1CarType; }
		}

		#region
		[ReadOnlyMember(nameof(JI_NumberOfDoorReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_NumberOfDoor", Caption = "Number of Door(s)", FullDescription = "The number of doors of the car.")]
		public override ZShort JI_NumberOfDoor { get => base.JI_NumberOfDoor; set => base.JI_NumberOfDoor = value; }

		ZString JI_NumberOfDoorStringFormatting => CommonHelper.ZeroConvertToEmptyString(JI_NumberOfDoor);

		public ZBool JI_NumberOfDoorReadOnly { get { return IsJ1OrH1CarType; } }
		#endregion

		#region JI_DisplacementReadOnly
		[ReadOnlyMember(nameof(JI_DisplacementReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Displacement", Caption = "Displacement(cc)", FullDescription = "Indicates the capacity of a car engine cylinder, measured in cc's.")]
		public override ZString JI_Displacement { get => base.JI_Displacement; set => base.JI_Displacement = value; }

		public ZBool JI_DisplacementReadOnly { get { return IsJ1CarType; } }
		#endregion

		#region JI_Cylinders
		[ReadOnlyMember(nameof(JI_CylindersReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Cylinders", Caption = "Number of Cylinder(s)", FullDescription = "The number of cylinders of the engine of the car.")]
		public override ZShort JI_Cylinders { get => base.JI_Cylinders; set => base.JI_Cylinders = value; }

		public ZBool JI_CylindersReadOnly { get { return IsJ1CarType; } }

		ZString JI_CylindersStringFormatting => CommonHelper.ZeroConvertToEmptyString(JI_Cylinders);
		#endregion

		#region JI_Cylinders
		[ReadOnlyMember(nameof(JI_SeatsReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Seats", Caption = "Number of Seat(s)", FullDescription = "The number of seats of the car.")]
		public override ZShort JI_Seats { get => base.JI_Seats; set => base.JI_Seats = value; }

		public bool JI_SeatsReadOnly { get { return IsJ1CarType; } }

		ZString JI_SeatsStringFormatting => CommonHelper.ZeroConvertToEmptyString(JI_Seats);
		#endregion

		#region JI_LHD
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.LeftSideSteeringCodeList))]
		[ReadOnlyMember(nameof(JI_LHDReadonly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_LHD", Caption = "Left Side Steering", FullDescription = "Indicates whether the car is left side steering.")]
		public override ZString JI_LHD { get => base.JI_LHD; set => base.JI_LHD = value; }

		public ZBool JI_LHDReadonly { get { return IsJ1OrH1CarType; } }
		#endregion

		#region JI_Transmission
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TransmissionCodeList))]
		[ReadOnlyMember(nameof(JI_TransmissionReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Transmission", Caption = "Transmission", FullDescription = "The transmission shifting mode of the car.")]
		public override ZString JI_Transmission
		{
			get => base.JI_Transmission;
			set
			{
				var oldValue = JI_Transmission;
				base.JI_Transmission = value;
				if (!IsCopying && oldValue != JI_Transmission)
				{
					if (JI_Transmission == TransmissionCodeList.Codes.CVT)
					{
						JI_Gears = ZShort.Zero;
					}
				}
			}
		}

		public ZBool JI_TransmissionReadOnly { get { return IsJ1CarType; } }
		#endregion

		#region JI_Gears
		[ReadOnlyMember(nameof(JI_GearsReadonly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Gears", Caption = "Number of Gear(s)", FullDescription = "The number of gears of the car.")]
		public override ZShort JI_Gears { get => base.JI_Gears; set => base.JI_Gears = value; }

		public bool JI_GearsReadonly { get { return JI_Transmission == TransmissionCodeList.Codes.CVT; } }

		internal ZString JI_GearsStringFormatting => CommonHelper.ZeroConvertToEmptyString(JI_Gears);
		#endregion

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.EngineTypeCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_EngineType", Caption = "Engine Type", FullDescription = "The engine type of the car.")]
		public override ZString JI_EngineType { get => base.JI_EngineType; set => base.JI_EngineType = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CatalystConverterPrintModeList))]
		[ReadOnlyMember(nameof(JI_HasCatalystConverterReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_HasCatalystConverter", Caption = "Catalytic Converter?", FullDescription = "Indicates whether a vehicle using unleaded gasoline is equipped with a catalyst conversion device in the exhaust pipe.")]
		public override ZString JI_HasCatalystConverter
		{
			get => base.JI_HasCatalystConverter;
			set
			{
				base.JI_HasCatalystConverter = value;

				if (!IsCopying && EquipmentPrintModeReadOnly && !JI_EquipmentPrintMode.IsEmpty)
				{
					JI_EquipmentPrintMode = ZString.Empty;
				}
			}
		}

		public bool JI_HasCatalystConverterReadOnly { get { return IsJ1CarType; } }

		bool EquipmentPrintModeReadOnly { get => CatalystConverterPrintModeList.Codes.No.Equals(JI_HasCatalystConverter); }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.EquipmentPrintModeList))]
		[ReadOnlyMember(nameof(EquipmentPrintModeReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_EquipmentPrintMode", Caption = "Standard Equipment", FullDescription = "Type of Catalyst Converter")]
		public override ZString JI_EquipmentPrintMode { get => base.JI_EquipmentPrintMode; set => base.JI_EquipmentPrintMode = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CarConditionCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CarCondition", Caption = "Condition", FullDescription = "The condition of the car.")]
		public override ZString JI_CarCondition { get => base.JI_CarCondition; set => base.JI_CarCondition = value; }

		bool AlcoholReadOnly => !IsForCAHeaderDN;

		[ReadOnlyMember(nameof(AlcoholReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_ExpirationDate", Caption = "Expiration Date", FullDescription = "The expiry date of the commodity.")]
		public override ZDateTime JI_ExpirationDate { get => base.JI_ExpirationDate; set => base.JI_ExpirationDate = value; }

		[ReadOnlyMember(nameof(AlcoholReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_BottledDate", Caption = "Bottled Date", FullDescription = "The date of the wine applying for inspection was bottled.")]
		public override ZDateTime JI_BottledDate { get => base.JI_BottledDate; set => base.JI_BottledDate = value; }

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PrimaryPreference", Caption = "Preference", FullDescription = "The duty rates for the first, second and third columns of the tariff.")]
		public override ZString JI_PrimaryPreference
		{
			get
			{
				return base.JI_PrimaryPreference;
			}
			set
			{
				var oldValue = base.JI_PrimaryPreference;
				base.JI_PrimaryPreference = value;
				if (!IsCopying && oldValue != value)
				{
					JI_PrimaryPreferenceInfo.RefreshBinding(oldValue);
					ClearOrSetDefaultConcessionOrder();
					if (InvoiceHeader != null)
					{
						InvoiceHeader.MarkAsNeedingValidation();
					}
					SetDefaultProcedure();
				}
			}
		}

		[DecimalPlaces(6)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_EnteredUnitPrice", Caption = "Unit Price", FullDescription = "The unit price of product.")]
		public override ZDecimal JI_EnteredUnitPrice
		{
			get => base.JI_EnteredUnitPrice;
			set
			{
				using (GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.UnitPrice))
				{
					var oldValue = JI_EnteredUnitPrice;
					base.JI_EnteredUnitPrice = value;
					if (!IsCopying && oldValue != JI_EnteredUnitPrice)
					{
						UpdateLinePriceIfChangedFromUnitPriceChanges();
						UpdateDocumentaryUnitPriceIfChangedFromUnitPriceChanges();
						Validation.ValidateJI_LinePrice();
					}
					JI_EnteredUnitPriceInfo.RefreshBinding(oldValue);
				}
			}
		}

		public override ZDecimal JI_LinePrice
		{
			get { return base.JI_LinePrice; }
			set
			{
				var oldValue = JI_LinePrice;
				base.JI_LinePrice = value;
				if (!IsCopying && JI_LinePrice != oldValue)
				{
					AddInfoChild.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.BondedGoodsCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_BondedGoodsCode", Caption = "Bonded Goods Code", FullDescription = "The code to identify bonded goods.")]
		public override ZString JI_BondedGoodsCode
		{
			get => base.JI_BondedGoodsCode;
			set
			{
				var oldValue = JI_BondedGoodsCode;
				base.JI_BondedGoodsCode = value;

				if (!IsCopying && JI_BondedGoodsCode != oldValue)
				{
					Validation.ValidateJI_CustomsSupplierPartNo();
				}
			}
		}

		[ReadOnlyMember(nameof(JI_ProductGradeReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_ProductGrade", Caption = "Grade", FullDescription = "The simplified description of the grading information of the commodity applying for inspection.")]
		public override ZString JI_ProductGrade { get => base.JI_ProductGrade; set => base.JI_ProductGrade = value; }

		ZBool JI_ProductGradeReadOnly => !EditableMessageTypeNX301AndNX603;

		[ReadOnlyMember(nameof(JI_ProductThicknessReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_ProductThickness", Caption = "Thickness", FullDescription = "The simplified description of the thickness of the commodity applying for inspection.")]
		public override ZString JI_ProductThickness { get => base.JI_ProductThickness; set => base.JI_ProductThickness = value; }

		ZBool JI_ProductThicknessReadOnly => !EditableMessageTypeNX301AndNX603;

		ZBool EditableMessageTypeNX301AndNX603 => Factory.GetValue(ref editableMessageTypeNX301AndNX603Cached, () => IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301, ControllingMessageTypeList.Codes.NX603));
		CachedProperty<ZBool> editableMessageTypeNX301AndNX603Cached;

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Group", Caption = "Grouping", FullDescription = "The grouping of products. It will be sent with the goods description for customs declaration.")]
		public override ZString JI_Group { get => base.JI_Group; set => base.JI_Group = value; }

		[ReadOnlyMember(nameof(AlcoholReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AlcoholAge", Caption = "Age", FullDescription = "The number of years for which the alcohol has been stored.")]
		public override ZInt JI_AlcoholAge { get => base.JI_AlcoholAge; set => base.JI_AlcoholAge = value; }

		[ReadOnlyMember(nameof(AlcoholReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AlcoholYear", Caption = "Year", FullDescription = "The year of the alcohol.")]
		public override ZInt JI_AlcoholYear { get => base.JI_AlcoholYear; set => base.JI_AlcoholYear = value; }

		[ReadOnlyMember(nameof(AlcoholReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AlcoholEndOfShelfLife", Caption = "End of Shelf Life", FullDescription = "The end of shelf life for the commodity applying for inspection.")]
		public override ZDateTime JI_AlcoholEndOfShelfLife { get => base.JI_AlcoholEndOfShelfLife; set => base.JI_AlcoholEndOfShelfLife = value; }

		[ReadOnlyMember(nameof(AlcoholReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AlteredLotNoAmt", Caption = "With Lot Number Altered", FullDescription = "The volume of the product with which the manufacturing lot number has been altered.")]
		public override ZDecimal JI_AlteredLotNoAmt { get => base.JI_AlteredLotNoAmt; set => base.JI_AlteredLotNoAmt = value; }

		[ReadOnlyMember(nameof(AlcoholReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_NoOriginalLotNoAmt", Caption = "Without Original Lot Number", FullDescription = "The volume of the product without manufacturing lot number at the factory.")]
		public override ZDecimal JI_NoOriginalLotNoAmt { get => base.JI_NoOriginalLotNoAmt; set => base.JI_NoOriginalLotNoAmt = value; }

		[ReadOnlyMember(nameof(AlcoholReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_RemovedLotNoAmt", Caption = "With Lot Number Removed", FullDescription = "The volume of the product with which the manufacturing lot number has been removed.")]
		public override ZDecimal JI_RemovedLotNoAmt { get => base.JI_RemovedLotNoAmt; set => base.JI_RemovedLotNoAmt = value; }

		[ReadOnlyMember(nameof(AlcoholReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AlcoholCountryRegion", Caption = "Country / Region", FullDescription = "The geographical indication of the alcohol.")]
		public override ZString JI_AlcoholCountryRegion { get => base.JI_AlcoholCountryRegion; set => base.JI_AlcoholCountryRegion = value; }

		protected override bool JI_ExtraInfoForClassification_ReadOnly => false;

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_TariffAdditionalCode", Caption = "Tariff Additional Code", FullDescription = "The code of the product subject to duty deduction or exemption in accordance with Import Tariff Additional Code.")]
		public override ZString JI_TariffAdditionalCode { get => base.JI_TariffAdditionalCode; set => base.JI_TariffAdditionalCode = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_FormattedTariff", Caption = "Tariff", FullDescription = "The tariff code of the product.")]
		public override ZString JI_FormattedTariff { get => base.JI_FormattedTariff; set => base.JI_FormattedTariff = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CusValueConvRatio", Caption = "Customs Value Conversion Ratio", FullDescription = "The conversion ratio used to calculate the customs value, special duty rate, specific duty amount and other taxes and fees.")]
		[ReadOnlyMember(nameof(JI_CusValueConvRatio_ReadOnly))]
		public override ZDecimal JI_CusValueConvRatio { get => base.JI_CusValueConvRatio; set => base.JI_CusValueConvRatio = value; }

		[DecimalPlaces(0)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CustomsValue", Caption = "Customs Value", FullDescription = "The price of the dutiable imported goods, used in calculating Ad-valorem Duty.")]
		public override ZDecimal JI_CustomsValue => base.JI_CustomsValue;

		public bool JI_CusValueConvRatio_ReadOnly => CusValueConvRatioShouldBeEmpty;

		public bool CusValueConvRatioShouldBeEmpty
		{
			get
			{
				var entryInstruction = EntryInstruction;
				string style = entryInstruction?.CEI_Style;
				string reasonForDuty = entryInstruction?.CEI_ReasonForDuty;
				string customsOffice = entryInstruction?.CEI_CustomsOffice;

				return style == Constants.DeclarationTypes.Import.F2 || style == Constants.DeclarationTypes.Import.G2
					|| reasonForDuty == ReasonforDutyList.Codes.FinishedProductDomesticSales || reasonForDuty == ReasonforDutyList.Codes.GiftSupplementary
					|| customsOffice == "CB" || customsOffice == "CS" || customsOffice == "DB" || customsOffice == "BB";
			}
		}

		public void SetDefaultCusValueConvRatioIfNeeded()
		{
			if (CusValueConvRatioShouldBeEmpty)
			{
				JI_CusValueConvRatio = ZDecimal.Zero;
			}
		}

		public override ZGuid JI_CEI
		{
			get { return base.JI_CEI; }
			set
			{
				var oldValue = JI_CEI;
				base.JI_CEI = value;
				if (InvoiceHeader != null)
				{
					InvoiceHeader.MarkAsNeedingValidation();
				}
				if (!IsCopying && JI_CEI != oldValue)
				{
					JI_CEI_DescriptionInfo.RefreshBinding();
					SetDefaultCusValueConvRatioIfNeeded();
					InvoiceLineLinkControllingMsgHeaders.ClearAndBuildElements();
				}
			}
		}

		public override ZGuid JI_JZ
		{
			get { return base.JI_JZ; }
			set
			{
				var oldValue = JI_JZ;
				base.JI_JZ = value;
				if (!IsCopying && JI_JZ != oldValue)
				{
					SetDefaultPrimaryPreferenceValue();
					SetDefaultEntryInstruction();
					if (InvoiceHeader != null)
					{
						SetDefaultOrderNumber();
						SetDefaultProcedureByRelatedIndicatorIfRequired();
					}
					AssignedJobComInvLineRefsCollection.MarkAsNeedingValidation();
					AddInfoChild.MarkAsNeedingValidation();
					RebuildInvoiceQuantityAndUnitQtyResultIfNeeded();
				}
			}
		}

		void SetDefaultProcedureByRelatedIndicatorIfRequired()
		{
			if (JI_Procedure.IsEmpty && InvoiceHeader.JZ_RelatedIndicator == RelationshipIndicatorList.Codes.RelationshipIndicator138)
			{
				JI_Procedure = Constants.ProcedureCodes._65;
			}
		}

		internal void SetDefaultEntryInstruction()
		{
			var jobDeclaration = Declaration;
			if (jobDeclaration != null && jobDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Count <= 1)
			{
				JI_CEI = jobDeclaration.IsPersistent ? (jobDeclaration.CusEntryInstruction?.PK ?? ZGuid.Empty) : ZGuid.Empty;
			}
		}

		public void SetDefaultPrimaryPreferenceValue()
		{
			if (IsImport && JI_PrimaryPreference.IsEmpty && !JI_Tariff.IsEmpty && !JI_CountryOfOrigin.IsEmpty)
			{
				var codeList = Lookups.PrimaryPreferenceList;
				var codesToLookFor = new[] { PreferenceCodes.ProvisionalPreference2, PreferenceCodes.ProvisionalPreference1, PreferenceCodes.ProvisionalPreference3, PreferenceCodes.Preference2, PreferenceCodes.Preference1, PreferenceCodes.Standard };
				codesToLookFor.FirstOrDefault(x =>
				{
					if (codeList.ContainsCode(x))
					{
						JI_PrimaryPreference = x;
						return true;
					}
					return false;
				});
			}
		}

		void SetDefaultOrderNumber()
		{
			if (!HasValidOrder && !IsDeclarationCloning)
			{
				JI_OrderNumber = InvoiceHeader.JZ_OrderNumber.Left(JI_OrderNumberInfo.MaxLength);
			}
		}

		bool IsDeclarationCloning => Declaration?.IsCloning ?? false;

		public ZDecimal TotalQuantityForPackagesPivot => PackagesPivot?.Cast<InvoiceLinePackagePivot>()?.Sum(x => x.CHC_Quantity) ?? ZDecimal.Zero;

		[MaxLength(Schema.JI_NDescriptionMaxLength)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_NDescription", ShortCaption = "Chinese Desc.", Caption = "Chinese Description", FullDescription = "The description of product. Both Chinese and Western European languages characters are accepted in this field.")]
		public override ZString JI_NDescription { get => base.JI_NDescription; set => base.JI_NDescription = value; }

		[MaxLength(Schema.JI_DescriptionMaxLength)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|EnglishDescription", ShortCaption = "English Desc.", Caption = "English Description", FullDescription = "The description of product. Only Western European languages characters are accepted in this field.")]
		public override ZString JI_Description { get => base.JI_Description; set => base.JI_Description = value; }

		[ReadOnlyMember(nameof(JI_TariffExtensionCodeReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_TariffExtensionCode", Caption = "Tariff Extension Code", FullDescription = "The tariff extension code of the commodity applying for inspection.")]
		public override ZString JI_TariffExtensionCode { get => base.JI_TariffExtensionCode; set => base.JI_TariffExtensionCode = value; }

		ZBool JI_TariffExtensionCodeReadOnly => Factory.GetValue(ref tariffExtensionCodeReadOnlyCached, () => !IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301, ControllingMessageTypeList.Codes.NX301_AX, ControllingMessageTypeList.Codes.NX601, ControllingMessageTypeList.Codes.NX603));
		CachedProperty<ZBool> tariffExtensionCodeReadOnlyCached;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsTypeList))]
		[ReadOnlyMember(nameof(GoodsTypeReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_GoodsType", Caption = "Goods Type", FullDescription = "The type of the goods.")]
		public override ZString JI_GoodsType { get => base.JI_GoodsType; set => base.JI_GoodsType = value; }

		ZBool GoodsTypeReadOnly => Factory.GetValue(ref goodsTypeReadOnlyCached, () => !IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX601));
		CachedProperty<ZBool> goodsTypeReadOnlyCached;

		[ReadOnlyMember(nameof(JI_QuarantineFeaturesReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_QuarantineFeatures", Caption = "Features", FullDescription = "The color and characteristics of the imported animal or the botanical nomenclature of the plant.")]
		public override ZString JI_QuarantineFeatures { get => base.JI_QuarantineFeatures; set => base.JI_QuarantineFeatures = value; }

		[ReadOnlyMember(nameof(JI_QuarantineTreatmentReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_QuarantineTreatment", Caption = "Treatment", FullDescription = "The description of the quarantine treatment completed for the declared quarantine goods.")]
		public override ZString JI_QuarantineTreatment { get => base.JI_QuarantineTreatment; set => base.JI_QuarantineTreatment = value; }

		[ReadOnlyMember(nameof(JI_AnimalAgeMonthReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AnimalAgeMonth", Caption = "Age (Month)", FullDescription = "The age of the imported/exported animal (enter age - month). If the age of each animal differs, fill in the oldest of all.")]
		public override ZInt JI_AnimalAgeMonth { get => base.JI_AnimalAgeMonth; set => base.JI_AnimalAgeMonth = value; }

		[ReadOnlyMember(nameof(JI_AnimalAgeYearReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AnimalAgeYear", Caption = "Age (Year)", FullDescription = "The age of the imported/exported animal (enter age - year). If the age of each animal differs, fill in the oldest of all.")]
		public override ZInt JI_AnimalAgeYear { get => base.JI_AnimalAgeYear; set => base.JI_AnimalAgeYear = value; }

		[ReadOnlyMember(nameof(JI_AnimalFemaleQtyReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AnimalFemaleQty", Caption = "Female Quantity", FullDescription = "The number of females of the imported/exported animals.")]
		public override ZInt JI_AnimalFemaleQty { get => base.JI_AnimalFemaleQty; set => base.JI_AnimalFemaleQty = value; }

		[ReadOnlyMember(nameof(JI_AnimalMaleQtyReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AnimalMaleQty", Caption = "Male Quantity", FullDescription = "The number of males of the imported/exported animals.")]
		public override ZInt JI_AnimalMaleQty { get => base.JI_AnimalMaleQty; set => base.JI_AnimalMaleQty = value; }

		[ReadOnlyMember(nameof(JI_MicrochipIDReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_MicrochipID", Caption = "Microchip Number", ShortCaption = "Microchip No.", FullDescription = "The microchip number implanted in the animal to identify the individual animal.")]
		public override ZString JI_MicrochipID { get => base.JI_MicrochipID; set => base.JI_MicrochipID = value; }

		[ReadOnlyMember(nameof(JI_VaccinationTypeDateReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_VaccinationTypeDate", Caption = "Vaccination Type/Date", FullDescription = "The vaccination type and date of the animal applying for quarantine (applicable to some live animals).")]
		public override ZString JI_VaccinationTypeDate { get => base.JI_VaccinationTypeDate; set => base.JI_VaccinationTypeDate = value; }

		ZBool EnableIfLinkedNX601 => !IsForCMHeaderMessageTypeNX601;

		ZBool EnableIfLinkedNX603 => !IsForCMHeaderMessageTypeNX603;

		[ReadOnlyMember(nameof(EnableIfLinkedNX601))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_BarCode", Caption = "Barcode", FullDescription = "The standard barcode of the product that can be read by the scanner.")]
		public override ZString JI_BarCode { get => base.JI_BarCode; set => base.JI_BarCode = value; }

		[ReadOnlyMember(nameof(EnableIfLinkedNX601))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PHValue", Caption = "pH Value", FullDescription = "The pH Value of the product at the final equilibrium state. When the goods type entered is \"2\" in accordance with Taiwan Food and Drug Administration, this field is mandatory.")]
		public override ZString JI_PHValue { get => base.JI_PHValue; set => base.JI_PHValue = value; }

		[DecimalPlaces(1), DecimalPrecision(4)]
		[ReadOnlyMember(nameof(EnableIfLinkedNX601))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PHValueNumeric", Caption = "pH Value", FullDescription = "The pH Value of the product at the final equilibrium state. When the goods type entered is \"2\" in accordance with Taiwan Food and Drug Administration, this field is mandatory.")]
		public ZDecimal JI_PHValueNumeric
		{
			get => ZDecimal.TryParse(JI_PHValue, out var number) ? number : ZDecimal.Zero;
			set
			{
				var oldValue = JI_PHValueNumeric;
				JI_PHValue = value.Round(1).ToString();
				JI_PHValueNumericInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JI_PHValueNumericInfo => GetZPropertyInfo(Schema.JI_PHValueNumeric);

		[ReadOnlyMember(nameof(EnableIfLinkedNX601))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_SterilizationValue", Caption = "Sterilization Fo Value", FullDescription = "The thermal death time required. When the goods type entered is \"1\" in accordance with Taiwan Food and Drug Administration, this field is mandatory.")]
		public override ZString JI_SterilizationValue { get => base.JI_SterilizationValue; set => base.JI_SterilizationValue = value; }

		[DecimalPlaces(1), DecimalPrecision(4)]
		[ReadOnlyMember(nameof(EnableIfLinkedNX601))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_SterilizationValueNumeric", Caption = "Sterilization Fo Value", FullDescription = "The thermal death time required. When the goods type entered is \"1\" in accordance with Taiwan Food and Drug Administration, this field is mandatory.")]
		public ZDecimal JI_SterilizationValueNumeric
		{
			get => ZDecimal.TryParse(JI_SterilizationValue, out var number) ? number : ZDecimal.Zero;
			set
			{
				var oldValue = JI_SterilizationValueNumeric;
				JI_SterilizationValue = value.Round(1).ToString();
				JI_SterilizationValueNumericInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JI_SterilizationValueNumericInfo => GetZPropertyInfo(Schema.JI_SterilizationValueNumeric);

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InnerPackageTypeList))]
		[ReadOnlyMember(nameof(JI_InnerPackTypeReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_InnerPackType", Caption = "Type", FullDescription = "The code of the package type.")]
		public override ZString JI_InnerPackType { get => base.JI_InnerPackType; set => base.JI_InnerPackType = value; }

		ZBool JI_InnerPackTypeReadOnly => Factory.GetValue(ref innerPackTypeReadOnlyCached, () => !IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301, ControllingMessageTypeList.Codes.NX301_AX, ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX601));
		CachedProperty<ZBool> innerPackTypeReadOnlyCached;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InnerPackingMaterialList))]
		[ReadOnlyMember(nameof(JI_InnerPackingMaterialReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_InnerPackingMaterial", Caption = "Material", FullDescription = "The code of the packaging material.")]
		public override ZString JI_InnerPackingMaterial { get => base.JI_InnerPackingMaterial; set => base.JI_InnerPackingMaterial = value; }

		ZBool JI_InnerPackingMaterialReadOnly => !IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX601);

		[ReadOnlyMember(nameof(JI_InnerPackDescriptionReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_InnerPackDescription", Caption = "Description", FullDescription = "The description of the package specification.")]
		public override ZString JI_InnerPackDescription { get => base.JI_InnerPackDescription; set => base.JI_InnerPackDescription = value; }

		ZBool JI_InnerPackDescriptionReadOnly => Factory.GetValue(ref innerPackDescriptionReadOnlyCached, () => !IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX601) && !IsLinkedNX101WithCertificateType(CertificateTypeList.Codes.Code15));
		CachedProperty<ZBool> innerPackDescriptionReadOnlyCached;

		#region JI_EPTDigit1
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_EPTDigit1", Caption = "Container Material", ShortCaption = "Material", FullDescription = "The first digit of environmental protection tariff indicating the material of container. The environmental protection tariff entered will be declared in the Assigned Number field.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ContainerMaterialList))]
		public override ZString JI_EPTDigit1
		{
			get { return base.JI_EPTDigit1; }
			set
			{
				var oldValue = JI_EPTDigit1;
				base.JI_EPTDigit1 = value;
				if (!IsCopying && JI_EPTDigit1 != oldValue)
				{
					if (JI_EPTDigit1 == ContainerMaterialList.Codes.Z)
					{
						JI_EPTDigit2 = ContainerCapacityList.Codes._0;
						JI_EPTDigit3 = ContainerMaterialNumberList.Codes._0;
					}
					else if (oldValue == ContainerMaterialList.Codes.Z)
					{
						JI_EPTDigit2 = ZString.Empty;
						JI_EPTDigit3 = ZString.Empty;
					}

					AssignedJobComInvLineRefsCollection.MarkAsNeedingValidation();
				}
			}
		}

		public ZBool ShouldJI_EPTDigit1IsZ => JI_EPTDigit1 == ContainerMaterialList.Codes.Z;

		ZBool JI_ConcessionOrderReadOnly => Lookups.OrderNumbersList.Count == 0;

		[ReadOnlyMember(nameof(JI_ConcessionOrderReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OrderNumbersList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_ConcessionOrder", Caption = "Quota", ShortCaption = "Quota")]
		public override ZString JI_ConcessionOrder { get => base.JI_ConcessionOrder; set => base.JI_ConcessionOrder = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CountryOfOrigin", Caption = "Goods Origin", FullDescription = "Country/region code of the origin of goods.")]
		public override ZString JI_CountryOfOrigin
		{
			get => base.JI_CountryOfOrigin;
			set
			{
				var oldValue = JI_CountryOfOrigin;
				base.JI_CountryOfOrigin = value;
				if (!IsCopying && JI_CountryOfOrigin != oldValue)
				{
					ClearOrSetDefaultConcessionOrder();
					SetDefaultPrimaryPreferenceValue();
					SetDefaultProcedure();
					DefaultTaxPaymentMethodIfNeeded();
				}
			}
		}

		void DefaultTaxPaymentMethodWhenProcedureChanged()
		{
			if (IsROR)
			{
				var rorPaymentMethod = RORPaymentMethod.IsEmpty ? DutyTaxPaymentMethodList.Codes.RorPayment : RORPaymentMethod.ToString();
				Taxes.Cast<JobComInvoiceLineTax>().Where(x => x.IsRorType).ForEach(x => x.JLT_MethodOfPayment = rorPaymentMethod);
			}
			else
			{
				DefaultTaxPaymentMethodIfNeeded();
			}
		}

		void DefaultTaxPaymentMethodIfNeeded()
		{
			Taxes.Cast<JobComInvoiceLineTax>().ForEach(x => x.SetDefaultPaymentMethod());
		}

		void ClearOrSetDefaultConcessionOrder()
		{
			var orderNumbersList = Lookups.OrderNumbersList;
			if (orderNumbersList.Count == 1 && orderNumbersList[0].Code.Equals(Constants.ConcessionOrder.Quota, StringComparison.InvariantCultureIgnoreCase)
				&& UniversalTariff is TariffView universalTariff && universalTariff.Rates.GetRatesFor(EffectiveAssessmentDate).Take(2).Count() == 1)
			{
				JI_ConcessionOrder = Constants.ConcessionOrder.Quota;
			}
			else
			{
				JI_ConcessionOrder = ZString.Empty;
			}
		}
		#endregion

		public ZBool IsEnvironmentalProtectionTariff => UniversalTariff?.HasAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.EnvironmentalProtectionTariff) ?? ZBool.False;

		#region JI_EPTDigit2
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_EPTDigit2", Caption = "Container Capacity (cc)", ShortCaption = "Capacity (cc)", FullDescription = "The second digit of environmental protection tariff indicating the capacity of container (cc). The environmental protection tariff entered will be declared in the Assigned Number field.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ContainerCapacityList))]
		[ReadOnlyMember(nameof(JI_EPTDigit2_ReadOnly))]
		public override ZString JI_EPTDigit2
		{
			get { return base.JI_EPTDigit2; }
			set
			{
				var oldValue = JI_EPTDigit2;
				base.JI_EPTDigit2 = value;
				if (!IsCopying && JI_EPTDigit2 != oldValue)
				{
					AssignedJobComInvLineRefsCollection.MarkAsNeedingValidation();
				}
			}
		}

		public ZBool JI_EPTDigit2_ReadOnly => ShouldJI_EPTDigit1IsZ;
		#endregion

		#region JI_EPTDigit3
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_EPTDigit3", Caption = "No. of Container Material(s)", ShortCaption = "No. of Material(s)", FullDescription = "The third digit of environmental protection tariff indicating the quantity of container. The environmental protection tariff entered will be declared in the Assigned Number field.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ContainerMaterialNumberList))]
		[ReadOnlyMember(nameof(JI_EPTDigit3_ReadOnly))]
		public override ZString JI_EPTDigit3
		{
			get { return base.JI_EPTDigit3; }
			set
			{
				var oldValue = JI_EPTDigit3;
				base.JI_EPTDigit3 = value;
				if (!IsCopying && JI_EPTDigit3 != oldValue)
				{
					AssignedJobComInvLineRefsCollection.MarkAsNeedingValidation();
				}
			}
		}

		public ZBool JI_EPTDigit3_ReadOnly => ShouldJI_EPTDigit1IsZ;
		#endregion

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CustomsUnitQty", Caption = "Statistical Weight Unit", ShortCaption = "UQ")]
		public override ZString JI_CustomsUnitQty
		{
			get { return base.JI_CustomsUnitQty; }
			set
			{
				var oldValue = base.JI_CustomsUnitQty;
				base.JI_CustomsUnitQty = value;
				if (!IsCopying && oldValue != JI_CustomsUnitQty)
				{
					SetInvoiceLineTaxDefaultQuantity();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AntiDumpingDutyRate", Caption = "Anti-Dumping Duty Rate", MediumCaption = "Anti-Dumping Rate", ShortCaption = "ADD Rate", FullDescription = "Rate of Anti-Dumping Duty")]
		public override ZDecimal JI_AntiDumpingDutyRate { get => base.JI_AntiDumpingDutyRate; set => base.JI_AntiDumpingDutyRate = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CountervailingDutyRate", Caption = "Countervailing Duty Rate", MediumCaption = "Countervailing Rate", ShortCaption = "CVD Rate", FullDescription = "Rate of Countervailing Duty")]
		public override ZDecimal JI_CountervailingDutyRate { get => base.JI_CountervailingDutyRate; set => base.JI_CountervailingDutyRate = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AdditionalDutyRate", Caption = "Additional Duty Rate", MediumCaption = "Additional Rate", ShortCaption = "ADT Rate", FullDescription = "Rate of Additional Duty")]
		public override ZDecimal JI_AdditionalDutyRate { get => base.JI_AdditionalDutyRate; set => base.JI_AdditionalDutyRate = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_RetaliatoryDutyRate", Caption = "Retaliatory Duty Rate", MediumCaption = "Retaliatory Rate", ShortCaption = "RTD Rate", FullDescription = "Rate of Retaliatory Duty")]
		public override ZDecimal JI_RetaliatoryDutyRate { get => base.JI_RetaliatoryDutyRate; set => base.JI_RetaliatoryDutyRate = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_IMPTariff", Caption = "Import Country's Tariff", MediumCaption = "Import Tariff", ShortCaption = "IMP Tariff", FullDescription = "Indicates the tariff of the Import country. Default the first 8 characters of Invoice Line Tariff when Certificate Type is '15'.")]
		public override ZString JI_IMPTariff { get => base.JI_IMPTariff; set => base.JI_IMPTariff = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TariffPrintLengthList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_TariffPrintLength", Caption = "Tariff Printing", MediumCaption = "Tariff Printing", ShortCaption = "Tariff Printing", FullDescription = "Indicates whether to print the tariff for printing the Certificate of Origin. Tariff Printing cannot be 'N' when the Certificate Type is '15'.")]
		public override ZString JI_TariffPrintLength { get => base.JI_TariffPrintLength; set => base.JI_TariffPrintLength = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CPT_124_OriginCriteriaCodeList_EN))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_OriginCriteria", Caption = "Origin Criteria")]
		public override ZString JI_OriginCriteria { get => base.JI_OriginCriteria; set => base.JI_OriginCriteria = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PTCriteriaCodes))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PTCriteria", Caption = "Preferential Treatment Criteria", MediumCaption = "Preferential Criteria", ShortCaption = "PT Criteria", FullDescription = "Indicates the standard of preferential tariff treatment. When the Certificate Type is  '09', '11', '13', '14', '15', '19', this column must be filled in.")]
		public override ZString JI_PTCriteria { get => base.JI_PTCriteria; set => base.JI_PTCriteria = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PTCriteria2List))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PTCriteria2", Caption = "Other Preferential Treatment Criteria", MediumCaption = "Other PT Criteria", ShortCaption = "Other Criteria", FullDescription = "Indicates the other standard of preferential tariff treatment. When the Certificate Type is  '09', '11', '13', '14', '19', this column must be filled in.")]
		public override ZString JI_PTCriteria2 { get => base.JI_PTCriteria2; set => base.JI_PTCriteria2 = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ManufacturerRelationshipCodes))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_ManufacturerRelationship", Caption = "Manufacturer Relationship", MediumCaption = "Manufacturer Relationship", ShortCaption = "Manufacturer Rel.", FullDescription = "Indicates the relationship between manufacturer and seller. When the Certificate Type is  '09', '11', '13', '14', '19', this column must be filled in.")]
		public override ZString JI_ManufacturerRelationship { get => base.JI_ManufacturerRelationship; set => base.JI_ManufacturerRelationship = value; }

		[DecimalPlaces(6)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PermitUnitPrice", Caption = "Permit Unit Price", FullDescription = "Indicates the unit price for Certificate of Origin. Default from the Documentary Unit Price of Invoice Line when Certificate Type is '15'.")]
		public override ZDecimal JI_PermitUnitPrice { get => base.JI_PermitUnitPrice; set => base.JI_PermitUnitPrice = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PermitUQList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PermitUQ", FullDescription = "Indicates the quantity unit for Certificate of Origin.")]
		public override ZString JI_PermitUQ { get => base.JI_PermitUQ; set => base.JI_PermitUQ = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PermitQty", Caption = "Permit Quantity", MediumCaption = "Permit QTY", ShortCaption = "Permit QTY", FullDescription = "Indicates the quantity for Certificate of Origin.")]
		public override ZDecimal JI_PermitQty { get => base.JI_PermitQty; set => base.JI_PermitQty = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CustomPermitUQ", Caption = "Permit Qty. Unit (For Printing)", ShortCaption = "Custom UQ")]
		public override ZString JI_CustomPermitUQ { get => base.JI_CustomPermitUQ; set => base.JI_CustomPermitUQ = value; }

		#endregion

		[ChildEditable(true)]
		public ChassisJobComInvLineRefsCollection ChassisJobComInvLineRefsCollection
		{
			get
			{
				if (fChassisJobComInvLineRefsCollection == null)
				{
					fChassisJobComInvLineRefsCollection = new ChassisJobComInvLineRefsCollection(this);
					fChassisJobComInvLineRefsCollection.Load();
					RegisterEditableChildObject(fChassisJobComInvLineRefsCollection);
				}
				return fChassisJobComInvLineRefsCollection;
			}
		}

		ChassisJobComInvLineRefsCollection fChassisJobComInvLineRefsCollection;

		public List<ZString> ChassisNumbers
		{
			get
			{
				return ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>().Where(x => !x.JG_ReferenceNumber.IsEmpty).Select(x => x.JG_ReferenceNumber).Distinct().ToList();
			}
		}

		#region PreviousPermitNoCusSupporting
		public PreviousPermitNoCusSupporting PrePermitNoCusSupporting
		{
			get
			{
				if (PreviousPermitNoCusSupportingCollection.Count == 0)
				{
					PreviousPermitNoCusSupportingCollection.AddNew();
				}
				return PreviousPermitNoCusSupportingCollection[0];
			}
		}

		PreviousPermitNoCusSupportingCollection PreviousPermitNoCusSupportingCollection
		{
			get
			{
				if (fPreviousPermitNoCusSupportingCollection == null)
				{
					fPreviousPermitNoCusSupportingCollection = new PreviousPermitNoCusSupportingCollection(this);
					fPreviousPermitNoCusSupportingCollection.Load();
					RegisterEditableChildObject(fPreviousPermitNoCusSupportingCollection);
				}
				return fPreviousPermitNoCusSupportingCollection;
			}
		}

		PreviousPermitNoCusSupportingCollection fPreviousPermitNoCusSupportingCollection;

		ZBool PreviousPermitNoReadOnly => !EditableMessageTypeNX301AndNX603;

		[UniversalCopyExtraProperty]
		[MaxLength(Schema.PreviousPermitNoInfoMaxLength)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PreviousPermitNo", Caption = "Previous Permit No", FullDescription = "The previous permit number that was issued before. The previous permit number of the commodity that failed the inspection conducted by the Bureau of Standards, Metrology and Inspection.")]
		[ReadOnlyMember(nameof(PreviousPermitNoReadOnly))]
		public ZString PreviousPermitNo
		{
			get
			{
				return PreviousPermitNoCusSupportingCollection.Count > 0 ? PrePermitNoCusSupporting.CSI_ReferenceNumber : ZString.Empty;
			}
			set
			{
				var oldValue = PreviousPermitNo;
				if (oldValue != value)
				{
					value = value.TrimEndSpaceTab();
					CheckMaximumLength(PreviousPermitNoInfo, value);
					PrePermitNoCusSupporting.CSI_ReferenceNumber = value;
					PreviousPermitNoInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidatePreviousPermitNo();
				}
			}
		}

		public ZPropertyInfo PreviousPermitNoInfo
		{
			get { return GetZPropertyInfo(nameof(PreviousPermitNo)); }
		}
		#endregion

		[ChildEditable(true)]
		public ShippingIdentificationDataCollection ShippingIdentificationDataCollection
		{
			get
			{
				if (fShippingIdentificationDataCollection == null)
				{
					fShippingIdentificationDataCollection = new ShippingIdentificationDataCollection(this);
					fShippingIdentificationDataCollection.Load();
					RegisterEditableChildObject(fShippingIdentificationDataCollection);
				}
				return fShippingIdentificationDataCollection;
			}
		}

		ShippingIdentificationDataCollection fShippingIdentificationDataCollection;
		public ZBool ShippingIdentificationDataCollectionReadOnly => Factory.GetValue(ref shippingIdentificationDataCollectionReadOnlyCached, () => !IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301, ControllingMessageTypeList.Codes.NX301_AX, ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX601, ControllingMessageTypeList.Codes.NX603));
		CachedProperty<ZBool> shippingIdentificationDataCollectionReadOnlyCached;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			var emptyAssignedNumberRows = AssignedJobComInvLineRefsCollection.Cast<AssignedJobComInvLineRefs>().Where(r => r.JG_ReferenceNumber.IsEmpty).ToArray();
			foreach (var row in emptyAssignedNumberRows)
			{
				AssignedJobComInvLineRefsCollection.RemoveAndDelete(row);
			}

			var emptyPermitNumberRows = PermitCusSupportingCollection.Cast<PermitCusSupporting>().Where(r => r.CSI_ReferenceNumber.IsEmpty && r.CSI_LineNo.IsEmpty).ToArray();
			foreach (var row in emptyPermitNumberRows)
			{
				PermitCusSupportingCollection.RemoveAndDelete(row);
			}

			var emptyControllingAgenciesRows = ExemptionOfControllingAgenciesCusSupportings.Cast<ExemptionOfControllingAgenciesCusSupporting>().Where(r => r.CSI_ReferenceNumber.IsEmpty).ToArray();
			foreach (var row in emptyControllingAgenciesRows)
			{
				ExemptionOfControllingAgenciesCusSupportings.RemoveAndDelete(row);
			}

			InvoiceLineLinkControllingMsgHeaders.SaveGenPivots();
		}

		public override void Delete()
		{
			InvoiceLineLinkControllingMsgHeaders.DeleteGenPivots();
			Taxes.RemoveAndDeleteAll();
			DeleteControllingMessageHeaderLinkInvoiceLine();
			base.Delete();

			RebuildInvoiceQuantityAndUnitQtyResultIfNeeded();
		}

		protected override void DeleteForDataRefresh()
		{
			DeleteControllingMessageHeaderLinkInvoiceLine();
			base.DeleteForDataRefresh();
		}

		void DeleteControllingMessageHeaderLinkInvoiceLine()
		{
			Declaration?.CusEntryInstruction.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().Where(x => x.IsControllingMessageHeaderLinkInvoiceLinesLoaded)
				.Select(x => x.ControllingMessageHeaderLinkInvoiceLines).ForEach(x => x.DeleteControllingMessageHeaderLinkInvoiceLineByInvoiceLine(this));
		}

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentID, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public PermitCusSupportingCollection PermitCusSupportingCollection
		{
			get
			{
				if (fPermitCusSupportingCollection == null)
				{
					fPermitCusSupportingCollection = new PermitCusSupportingCollection(this);
					fPermitCusSupportingCollection.Load();
					RegisterEditableChildObject(fPermitCusSupportingCollection);
				}
				return fPermitCusSupportingCollection;
			}
		}
		PermitCusSupportingCollection fPermitCusSupportingCollection;

		#region PermitCusSupportingNos PermitCusSupportingLineNos

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingNo1", Caption = "Permit No 1")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo1
		{
			get => GetPermitCusSupportingNo(0);
			set
			{
				var oldValue = PermitCusSupportingNo1;
				SetPermitCusSupportingNo(0, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo1))
				{
					PermitCusSupportingNo1Info.RefreshBinding(oldValue);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePermitCusSupportingNo1();
					}
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo1Info => GetPermitCusSupportingNoInfo(0, nameof(PermitCusSupportingNo1));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingLineNo1", Caption = "Permit Line No 1")]
		public ZInt PermitCusSupportingLineNo1
		{
			get => GetPermitCusSupportingLineNo(0);
			set => SetPermitCusSupportingLineNo(0, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo1Info => GetPermitCusSupportingLineNoInfo(0, nameof(PermitCusSupportingLineNo1));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingNo2", Caption = "Permit No 2")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo2
		{
			get => GetPermitCusSupportingNo(1);
			set
			{
				var oldValue = PermitCusSupportingNo2;
				SetPermitCusSupportingNo(1, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo2))
				{
					PermitCusSupportingNo2Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo2Info => GetPermitCusSupportingNoInfo(1, nameof(PermitCusSupportingNo2));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingLineNo2", Caption = "Permit Line No 2")]
		public ZInt PermitCusSupportingLineNo2
		{
			get => GetPermitCusSupportingLineNo(1);
			set => SetPermitCusSupportingLineNo(1, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo2Info => GetPermitCusSupportingLineNoInfo(1, nameof(PermitCusSupportingLineNo2));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingNo3", Caption = "Permit No 3")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo3
		{
			get => GetPermitCusSupportingNo(2);
			set
			{
				var oldValue = PermitCusSupportingNo3;
				SetPermitCusSupportingNo(2, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo3))
				{
					PermitCusSupportingNo3Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo3Info => GetPermitCusSupportingNoInfo(2, nameof(PermitCusSupportingNo3));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingLineNo3", Caption = "Permit Line No 3")]
		public ZInt PermitCusSupportingLineNo3
		{
			get => GetPermitCusSupportingLineNo(2);
			set => SetPermitCusSupportingLineNo(2, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo3Info => GetPermitCusSupportingLineNoInfo(2, nameof(PermitCusSupportingLineNo3));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingNo4", Caption = "Permit No 4")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo4
		{
			get => GetPermitCusSupportingNo(3);
			set
			{
				var oldValue = PermitCusSupportingNo4;
				SetPermitCusSupportingNo(3, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo4))
				{
					PermitCusSupportingNo4Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo4Info => GetPermitCusSupportingNoInfo(3, nameof(PermitCusSupportingNo4));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingLineNo4", Caption = "Permit Line No 4")]
		public ZInt PermitCusSupportingLineNo4
		{
			get => GetPermitCusSupportingLineNo(3);
			set => SetPermitCusSupportingLineNo(3, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo4Info => GetPermitCusSupportingLineNoInfo(3, nameof(PermitCusSupportingLineNo4));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingNo5", Caption = "Permit No 5")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo5
		{
			get => GetPermitCusSupportingNo(4);
			set
			{
				var oldValue = PermitCusSupportingNo5;
				SetPermitCusSupportingNo(4, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo5))
				{
					PermitCusSupportingNo5Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo5Info => GetPermitCusSupportingNoInfo(4, nameof(PermitCusSupportingNo5));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitCusSupportingLineNo5", Caption = "Permit Line No 5")]
		public ZInt PermitCusSupportingLineNo5
		{
			get => GetPermitCusSupportingLineNo(4);
			set => SetPermitCusSupportingLineNo(4, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo5Info => GetPermitCusSupportingLineNoInfo(4, nameof(PermitCusSupportingLineNo5));

		ZString GetPermitCusSupportingNo(int index) => PermitCusSupportingCollection.Count <= index ? ZString.Empty : SortedPermitCusSupportingCollection[index].CSI_ReferenceNumber;

		void SetPermitCusSupportingNo(int index, ZString value)
		{
			var permitCusSupporting = PermitCusSupportingCollection.Count <= index ? PermitCusSupportingCollection.AddNew() : SortedPermitCusSupportingCollection[index];
			permitCusSupporting.CSI_ReferenceNumber = value;
		}

		ZPropertyInfo GetPermitCusSupportingNoInfo(int index, string propertyName) => PermitCusSupportingCollection.Count <= index ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => SortedPermitCusSupportingCollection[index].CSI_ReferenceNumberInfo);

		ZInt GetPermitCusSupportingLineNo(int index) => PermitCusSupportingCollection.Count <= index ? ZInt.Zero : SortedPermitCusSupportingCollection[index].CSI_LineNo;

		void SetPermitCusSupportingLineNo(int index, ZInt value)
		{
			var permitCusSupporting = PermitCusSupportingCollection.Count <= index ? PermitCusSupportingCollection.AddNew() : SortedPermitCusSupportingCollection[index];
			permitCusSupporting.CSI_LineNo = value;
		}

		ZPropertyInfo GetPermitCusSupportingLineNoInfo(int index, string propertyName) => PermitCusSupportingCollection.Count <= index ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => SortedPermitCusSupportingCollection[index].CSI_LineNoInfo);

		internal List<PermitCusSupporting> SortedPermitCusSupportingCollection
		{
			get
			{
				var collection = PermitCusSupportingCollection.Cast<PermitCusSupporting>();
				return (IsCopying || ((ISupportDataImporting)this).IsImportingData) ? collection.ToList() : collection.OrderBy(c => c.CSI_ItemNumber).ToList();
			}
		}

		#endregion

		#region Assigned Numbers

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|AssignedNumber1", Caption = "Assigned Number 1", MediumCaption = "Assigned No. 1", ShortCaption = "AS NO. 1")]
		[MaxLength(35)]
		public ZString AssignedNumber1
		{
			get => GetAssignedNumber(0);
			set
			{
				var oldValue = AssignedNumber1;
				SetAssignedNumber(0, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber1))
				{
					AssignedNumber1Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber1Info => GetAssignedNumberInfo(0, nameof(AssignedNumber1));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|AssignedNumber2", Caption = "Assigned Number 2", MediumCaption = "Assigned No. 2", ShortCaption = "AS NO. 2")]
		[MaxLength(35)]
		public ZString AssignedNumber2
		{
			get => GetAssignedNumber(1);
			set
			{
				var oldValue = AssignedNumber2;
				SetAssignedNumber(1, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber2))
				{
					AssignedNumber2Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber2Info => GetAssignedNumberInfo(1, nameof(AssignedNumber2));

		ZString GetAssignedNumber(int index) => AssignedJobComInvLineRefsCollection.Count <= index ? ZString.Empty : AssignedJobComInvLineRefsCollection[index].JG_ReferenceNumber;

		void SetAssignedNumber(int index, ZString value)
		{
			var assignedJobComInvLineRef = AssignedJobComInvLineRefsCollection.Count <= index ? AssignedJobComInvLineRefsCollection.AddNew() : AssignedJobComInvLineRefsCollection[index];
			assignedJobComInvLineRef.JG_ReferenceNumber = value;
		}

		ZPropertyInfo GetAssignedNumberInfo(int index, string propertyName) => AssignedJobComInvLineRefsCollection.Count <= index ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => AssignedJobComInvLineRefsCollection[index].JG_ReferenceNumberInfo);

		#endregion

		#region Permit Exemption Codes

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SpecialCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitExemptionCode1", Caption = "Permit Exemption Code 1", MediumCaption = "Exemption Code 1", ShortCaption = "Exem. Code 1")]
		[MaxLength(100)]
		public ZString PermitExemptionCode1
		{
			get => GetPermitExemptionCode(0);
			set => SetPermitExemptionCode(0, value);
		}

		public ZPropertyInfo PermitExemptionCode1Info => GetPermitExemptionCodeInfo(0, nameof(PermitExemptionCode1));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SpecialCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitExemptionCode2", Caption = "Permit Exemption Code 2", MediumCaption = "Exemption Code 2", ShortCaption = "Exem. Code 2")]
		[MaxLength(100)]
		public ZString PermitExemptionCode2
		{
			get => GetPermitExemptionCode(1);
			set => SetPermitExemptionCode(1, value);
		}

		public ZPropertyInfo PermitExemptionCode2Info => GetPermitExemptionCodeInfo(1, nameof(PermitExemptionCode2));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SpecialCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitExemptionCode3", Caption = "Permit Exemption Code 3", MediumCaption = "Exemption Code 3", ShortCaption = "Exem. Code 3")]
		[MaxLength(100)]
		public ZString PermitExemptionCode3
		{
			get => GetPermitExemptionCode(2);
			set => SetPermitExemptionCode(2, value);
		}

		public ZPropertyInfo PermitExemptionCode3Info => GetPermitExemptionCodeInfo(2, nameof(PermitExemptionCode3));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SpecialCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitExemptionCode4", Caption = "Permit Exemption Code 4", MediumCaption = "Exemption Code 4", ShortCaption = "Exem. Code 4")]
		[MaxLength(100)]
		public ZString PermitExemptionCode4
		{
			get => GetPermitExemptionCode(3);
			set => SetPermitExemptionCode(3, value);
		}

		public ZPropertyInfo PermitExemptionCode4Info => GetPermitExemptionCodeInfo(3, nameof(PermitExemptionCode4));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SpecialCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PermitExemptionCode5", Caption = "Permit Exemption Code 5", MediumCaption = "Exemption Code 5", ShortCaption = "Exem. Code 5")]
		[MaxLength(100)]
		public ZString PermitExemptionCode5
		{
			get => GetPermitExemptionCode(4);
			set => SetPermitExemptionCode(4, value);
		}

		public ZPropertyInfo PermitExemptionCode5Info => GetPermitExemptionCodeInfo(4, nameof(PermitExemptionCode5));

		ZString GetPermitExemptionCode(int index) => ExemptionOfControllingAgenciesCusSupportings.Count <= index ? ZString.Empty : ExemptionOfControllingAgenciesCusSupportings[index].CSI_ReferenceNumber;

		void SetPermitExemptionCode(int index, ZString value)
		{
			var permitExemption = ExemptionOfControllingAgenciesCusSupportings.Count <= index ? ExemptionOfControllingAgenciesCusSupportings.AddNew() : ExemptionOfControllingAgenciesCusSupportings[index];
			permitExemption.CSI_ReferenceNumber = value;
		}

		ZPropertyInfo GetPermitExemptionCodeInfo(int index, string propertyName) => ExemptionOfControllingAgenciesCusSupportings.Count <= index ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => ExemptionOfControllingAgenciesCusSupportings[index].CSI_ReferenceNumberInfo);

		#endregion

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentID, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public ExemptionOfControllingAgenciesCusSupportingCollection ExemptionOfControllingAgenciesCusSupportings
		{
			get
			{
				if (fExemptionOfControllingAgenciesCusSupportings == null)
				{
					fExemptionOfControllingAgenciesCusSupportings = new ExemptionOfControllingAgenciesCusSupportingCollection(this);
					fExemptionOfControllingAgenciesCusSupportings.Load();
					RegisterEditableChildObject(fExemptionOfControllingAgenciesCusSupportings);
				}
				return fExemptionOfControllingAgenciesCusSupportings;
			}
		}
		ExemptionOfControllingAgenciesCusSupportingCollection fExemptionOfControllingAgenciesCusSupportings;

		[ChildEditable(true)]
		public AssignedJobComInvLineRefsCollection AssignedJobComInvLineRefsCollection
		{
			get
			{
				if (fAssignedJobComInvLineRefsCollection == null)
				{
					fAssignedJobComInvLineRefsCollection = new AssignedJobComInvLineRefsCollection(this);
					fAssignedJobComInvLineRefsCollection.Load();
					RegisterEditableChildObject(fAssignedJobComInvLineRefsCollection);
				}
				return fAssignedJobComInvLineRefsCollection;
			}
		}

		AssignedJobComInvLineRefsCollection fAssignedJobComInvLineRefsCollection;

		[ChildEditable(true)]
		public StorageAndShippingConditionJobComInvLineRefsCollection StorageAndShippingConditionJobComInvLineRefsCollection
		{
			get
			{
				if (storageAndShippingConditionJobComInvLineRefsCollection == null)
				{
					storageAndShippingConditionJobComInvLineRefsCollection = new StorageAndShippingConditionJobComInvLineRefsCollection(this);
					storageAndShippingConditionJobComInvLineRefsCollection.Load();
					RegisterEditableChildObject(storageAndShippingConditionJobComInvLineRefsCollection);
				}
				return storageAndShippingConditionJobComInvLineRefsCollection;
			}
		}
		StorageAndShippingConditionJobComInvLineRefsCollection storageAndShippingConditionJobComInvLineRefsCollection;

		#region TypeApprovalCertificateNumberCusSupporting
		public TypeApprovalCertificateNumberCusSupporting TypeApprovalCertificateNumbers
		{
			get
			{
				if (fTypeApprovalCertificateNumberCusSupporting?.IsDeleted ?? true)
				{
					fTypeApprovalCertificateNumberCusSupporting = TypeApprovalCertificateNumberCusSupportingCollection.FirstOrDefault() ?? TypeApprovalCertificateNumberCusSupportingCollection.AddNew();
				}
				return fTypeApprovalCertificateNumberCusSupporting;
			}
		}
		TypeApprovalCertificateNumberCusSupporting fTypeApprovalCertificateNumberCusSupporting;

		ZBool TypeApprovalCertificateNumbersReadOnly => !IsForCAHeader20 && !IsForCAHeaderCI && !IsForCAHeader2Q;

		[ChildEditable(true)]
		public TypeApprovalCertificateNumberCusSupportingCollection TypeApprovalCertificateNumberCusSupportingCollection
		{
			get { return fTypeApprovalCertificateNumberCusSupportingCollection ?? (fTypeApprovalCertificateNumberCusSupportingCollection = GetTypeApprovalCertificateNumberCusSupportingCollection()); }
		}
		TypeApprovalCertificateNumberCusSupportingCollection fTypeApprovalCertificateNumberCusSupportingCollection;

		TypeApprovalCertificateNumberCusSupportingCollection GetTypeApprovalCertificateNumberCusSupportingCollection()
		{
			var result = new TypeApprovalCertificateNumberCusSupportingCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[ReadOnlyMember(nameof(TypeApprovalCertificateNumbersReadOnly))]
		[UniversalCopyExtraProperty]
		[MaxLength(1)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|ExemptionCode", Caption = "Exemption Reason Code", FullDescription = "The reason of imported goods exempted from type approval.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ExemptionCodeList))]
		public ZString ExemptionCode
		{
			get => TypeApprovalCertificateNumbers?.CSI_Code ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ExemptionCodeInfo, value);
				TypeApprovalCertificateNumbers.CSI_Code = value;
				ExemptionCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExemptionCodeInfo => (fTypeApprovalCertificateNumberCusSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(ExemptionCode)) : GetWrappedZPropertyInfo(nameof(ExemptionCode), x => fTypeApprovalCertificateNumberCusSupporting.CSI_CodeInfo);

		[ReadOnlyMember(nameof(TypeApprovalCertificateNumbersReadOnly))]
		[UniversalCopyExtraProperty]
		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|TypeApprovalPartyIdentifier", Caption = "Authorized Person ID Type", FullDescription = "The ID type of VAT number, ID card number, foreign resident card number or passport number.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PartyIdentifierCodeList))]
		public ZString TypeApprovalPartyIdentifier
		{
			get => TypeApprovalCertificateNumbers?.CSI_Description ?? ZString.Empty;
			set
			{
				TypeApprovalCertificateNumbers.CSI_Description = value;
				TypeApprovalPartyIdentifierInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TypeApprovalPartyIdentifierInfo => (fTypeApprovalCertificateNumberCusSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(TypeApprovalPartyIdentifier)) : GetWrappedZPropertyInfo(nameof(TypeApprovalPartyIdentifier), x => fTypeApprovalCertificateNumberCusSupporting.CSI_DescriptionInfo);

		[ReadOnlyMember(nameof(TypeApprovalCertificateNumbersReadOnly))]
		[UniversalCopyExtraProperty]
		[MaxLength(14)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|TypeApprovalAuthorizedParty", Caption = "Authorized Person ID", FullDescription = "The VAT number, ID card number, foreign resident card number or passport number of the authorized person of type approval.")]
		public ZString TypeApprovalAuthorizedParty
		{
			get => TypeApprovalCertificateNumbers?.CSI_ReferenceNumber2 ?? ZString.Empty;
			set
			{
				TypeApprovalCertificateNumbers.CSI_ReferenceNumber2 = value;
				TypeApprovalAuthorizedPartyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TypeApprovalAuthorizedPartyInfo => (fTypeApprovalCertificateNumberCusSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(TypeApprovalAuthorizedParty)) : GetWrappedZPropertyInfo(nameof(TypeApprovalAuthorizedParty), x => fTypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumber2Info);

		[ReadOnlyMember(nameof(TypeApprovalCertificateNumbersReadOnly))]
		[UniversalCopyExtraProperty]
		[MaxLength(14)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|TypeApprovalCertificateNo", Caption = "Certificate", FullDescription = "The certificate number recognized by CCC code in accordance with the regulations of the Bureau of Standards, Metrology and Inspection.")]
		public ZString TypeApprovalCertificateNo
		{
			get => TypeApprovalCertificateNumbers?.CSI_ReferenceNumber ?? ZString.Empty;
			set
			{
				TypeApprovalCertificateNumbers.CSI_ReferenceNumber = value;
				TypeApprovalCertificateNoInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TypeApprovalCertificateNoInfo => (fTypeApprovalCertificateNumberCusSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(TypeApprovalCertificateNo)) : GetWrappedZPropertyInfo(nameof(TypeApprovalCertificateNo), x => fTypeApprovalCertificateNumberCusSupporting.CSI_ReferenceNumberInfo);

		#endregion

		#region PreviousBondedEntryNumberCusSupporting
		[UniversalCopyExtraProperty]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PreviousBondedEntryNumber", Caption = "Previous Bonded Entry Number", ShortCaption = "Pre. Bonded Entry No", FullDescription = "The previous bonded entry number of the bonded goods.")]
		[MaxLength(14)]
		public ZString PreviousBondedEntryNumber
		{
			get
			{
				return PreviousBondedCusSupporting?.CSI_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				var oldValue = PreviousBondedEntryNumber;
				if (oldValue != value)
				{
					CheckMaximumLength(PreviousBondedEntryNumberInfo, value);
					PreviousBondedCusSupporting.CSI_ReferenceNumber = value;
					PreviousBondedEntryNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PreviousBondedEntryNumberInfo
		{
			get { return (fPreviousBonded?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(PreviousBondedEntryNumber)) : GetWrappedZPropertyInfo(nameof(PreviousBondedEntryNumber), x => PreviousBondedCusSupporting.CSI_ReferenceNumberInfo); }
		}

		[UniversalCopyExtraProperty]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PreviousBondedEntryLineNumber", Caption = "Previous Bonded Entry Line Number", ShortCaption = "Pre. Bonded Entry LNO", FullDescription = "The previous bonded entry line number of the bonded goods.")]
		[MaxLength(4)]
		public ZInt PreviousBondedEntryLineNumber
		{
			get
			{
				return PreviousBondedCusSupporting.CSI_LineNo;
			}
			set
			{
				var oldValue = PreviousBondedEntryLineNumber;
				var newValue = value;
				if (oldValue != newValue)
				{
					PreviousBondedCusSupporting.CSI_LineNo = value;
					PreviousBondedEntryLineNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PreviousBondedEntryLineNumberInfo
		{
			get { return (fPreviousBonded?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(PreviousBondedEntryLineNumber)) : GetWrappedZPropertyInfo(nameof(PreviousBondedEntryLineNumber), x => PreviousBondedCusSupporting.CSI_LineNoInfo); }
		}

		public PreviousBondedCusSupporting PreviousBondedCusSupporting
		{
			get
			{
				if (fPreviousBonded?.IsDeleted ?? true)
				{
					fPreviousBonded = PreviousBondedCusSupportingCollection.FirstOrDefault() ?? PreviousBondedCusSupportingCollection.AddNew();
				}
				return fPreviousBonded;
			}
		}
		PreviousBondedCusSupporting fPreviousBonded;

		[ChildEditable(true)]
		public PreviousBondedCusSupportingCollection PreviousBondedCusSupportingCollection
		{
			get { return fPreviousBondedCusSupportingCollection ?? (fPreviousBondedCusSupportingCollection = GetPreviousBondedCusSupportingCollection()); }
		}
		PreviousBondedCusSupportingCollection fPreviousBondedCusSupportingCollection;

		PreviousBondedCusSupportingCollection GetPreviousBondedCusSupportingCollection()
		{
			var result = new PreviousBondedCusSupportingCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}
		#endregion

		#region MedicalInstrumentOrFoodCusSupporting
		public MedicalInstrumentOrFoodCusSupporting MedicalInstrumentOrFoodCusSupporting
		{
			get
			{
				if (fMedicalInstrumentOrFoodCusSupporting?.IsDeleted ?? true)
				{
					fMedicalInstrumentOrFoodCusSupporting = MedicalInstrumentOrFoodCusSupportingCollection.FirstOrDefault() ?? MedicalInstrumentOrFoodCusSupportingCollection.AddNew();
				}
				return fMedicalInstrumentOrFoodCusSupporting;
			}
		}

		MedicalInstrumentOrFoodCusSupporting fMedicalInstrumentOrFoodCusSupporting;

		MedicalInstrumentOrFoodCusSupportingCollection MedicalInstrumentOrFoodCusSupportingCollection
		{
			get { return fMedicalInstrumentOrFoodCusSupportingCollection ?? (fMedicalInstrumentOrFoodCusSupportingCollection = GetMedicalInstrumentOrFoodCusSupportingCollection()); }
		}
		MedicalInstrumentOrFoodCusSupportingCollection fMedicalInstrumentOrFoodCusSupportingCollection;

		MedicalInstrumentOrFoodCusSupportingCollection GetMedicalInstrumentOrFoodCusSupportingCollection()
		{
			var result = new MedicalInstrumentOrFoodCusSupportingCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[UniversalCopyExtraProperty]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(EnableIfLinkedNX603))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PartyIdentifierCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|PartyIdentifier", Caption = "Authorized Person ID Type", FullDescription = "The ID type of VAT number, ID card number, foreign resident card number or passport number.")]
		public ZString PartyIdentifier
		{
			get => MedicalInstrumentOrFoodCusSupporting?.CSI_Code ?? ZString.Empty;
			set
			{
				CheckMaximumLength(PartyIdentifierInfo, value);
				MedicalInstrumentOrFoodCusSupporting.CSI_Code = value;
				PartyIdentifierInfo.RefreshBinding();

				if (PartyIdentifier.IsEmpty)
				{
					AuthorizedPerson = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePartyIdentifier();
				}
			}
		}

		public ZPropertyInfo PartyIdentifierInfo => GetZPropertyInfo(Schema.PartyIdentifier);

		[UniversalCopyExtraProperty]
		[MaxLength(14)]
		[ReadOnlyMember(nameof(EnableIfLinkedNX603))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|CertificateNo", Caption = "Certificate", FullDescription = "The certificate number recognized by CCC code in accordance with the regulations of the Taiwan Food and Drug Administration.")]
		public ZString CertificateNo
		{
			get => MedicalInstrumentOrFoodCusSupporting?.CSI_ReferenceNumber ?? ZString.Empty;
			set
			{
				CheckMaximumLength(CertificateNoInfo, value);
				MedicalInstrumentOrFoodCusSupporting.CSI_ReferenceNumber = value;
				CertificateNoInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCertificateNo();
				}
			}
		}

		public ZPropertyInfo CertificateNoInfo => GetZPropertyInfo(Schema.CertificateNo);

		[UniversalCopyExtraProperty]
		[MaxLength(14)]
		[ReadOnlyMember(nameof(EnableIfLinkedNX603))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|AuthorizedPerson", Caption = "Authorized Person ID", FullDescription = "The VAT number, ID card number, foreign resident card number or passport number of the authorized person of medical instrument.")]
		public ZString AuthorizedPerson
		{
			get => MedicalInstrumentOrFoodCusSupporting?.CSI_ReferenceNumber2 ?? ZString.Empty;
			set
			{
				CheckMaximumLength(AuthorizedPersonInfo, value);
				MedicalInstrumentOrFoodCusSupporting.CSI_ReferenceNumber2 = value;
				AuthorizedPersonInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateAuthorizedPerson();
				}
			}
		}

		public ZPropertyInfo AuthorizedPersonInfo => GetZPropertyInfo(Schema.AuthorizedPerson);
		#endregion

		#region FoodDataCollection
		[ChildEditable(true)]
		public FoodDataCollection FoodDataCollection
		{
			get
			{
				if (fFoodDataCollection == null)
				{
					fFoodDataCollection = new FoodDataCollection(this);
					fFoodDataCollection.SetReadOnlyIncludingChildren(EnableIfLinkedNX601);
					fFoodDataCollection.Load();
					RegisterEditableChildObject(fFoodDataCollection);
				}
				return fFoodDataCollection;
			}
		}

		FoodDataCollection fFoodDataCollection;
		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (Declaration?.CustomsEntryInstructionProvider.CustomsEntryInstructions?.Count == 1)
			{
				JI_CEI = Declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions[0].PK;
			}

			JI_TariffPrintLength = PrintingTariffCodeList.Codes.N;
		}

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.Food, typeof(FoodData) },
				{ CusCodeDataTypeList.Codes.ReservedField, typeof(JobComInvoiceLineReservedField) },
				{ CusCodeDataTypeList.Codes.PackingHouses, typeof(PackingHouse) },
				{ CusCodeDataTypeList.Codes.PackingDates, typeof(PackingDate) },
				{ CusCodeDataTypeList.Codes.SlaughterDates, typeof(SlaughterDate) }
			};
			return result;
		}

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.PreviousPermitNumber, typeof(PreviousPermitNoCusSupporting) },
				{ CusSupportingInfoTypeList.Codes.PermitNumber, typeof(PermitCusSupporting) },
				{ CusSupportingInfoTypeList.Codes.TypeApprovalCertificateNumber, typeof(TypeApprovalCertificateNumberCusSupporting) },
				{ CusSupportingInfoTypeList.Codes.CertificateOfOriginNumber, typeof(CertificateOfOriginCusSupporting) },
				{ CusSupportingInfoTypeList.Codes.CitesImportPermit, typeof(CitesPermitCusSupporting) },
				{ CusSupportingInfoTypeList.Codes.ShtcImportPermit, typeof(HighTechLicenseCusSupporting) },
				{ CusSupportingInfoTypeList.Codes.MedicalInstrumentPartyIdentifier, typeof(MedicalInstrumentOrFoodCusSupporting) },
				{ CusSupportingInfoTypeList.Codes.TariffRateQuotaCertificate, typeof(QuotaPermitNumberCusSupporting) },
				{ CusSupportingInfoTypeList.Codes.PreviousBondedEntryNumber, typeof(PreviousBondedCusSupporting) },
				{ CusSupportingInfoTypeList.Codes.PermitExemptionCodes, typeof(ExemptionOfControllingAgenciesCusSupporting) }
			};
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> Integration.Customs.ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ CusAddInfoTypeAttribute.Codes.TWShippingIdentification, typeof(ShippingIdentificationData) }
			};
			return result;
		}

		public JobComInvoiceLinePartSynchronisationManager NewOwnerProductSyncManager { get; private set; }

		protected override void InitialisePartSyncManager()
		{
			base.InitialisePartSyncManager();
			if (NewOwnerProductSyncManager == null)
			{
				NewOwnerProductSyncManager = new JobComInvoiceLinePartSynchronisationManager(new InvoiceLineNewOwnerPartDetails(this));
			}
		}

		public override ZGuid JI_OwnerProduct
		{
			get { return base.JI_OwnerProduct; }
			set
			{
				var oldValue = JI_OwnerProduct;
				base.JI_OwnerProduct = value;
				if (!IsCopying && oldValue != JI_OwnerProduct)
				{
					if (NewOwnerProductSyncManager != null)
					{
						NewOwnerProductSyncManager.ReloadPart = true;
					}
				}
			}
		}

		public OrgSupplierPart NewOwnerProduct
		{
			get
			{
				OrgSupplierPart result = null;
				if (NewOwnerProductSyncManager == null)
				{
					ErrorReporter.ReportOnce("NewOwnerProductSyncManagerNullAccess", "NewOwnerProductSyncManager has not been initialised");
				}
				else
				{
					if (NewOwnerProductSyncManager.Part != null && !NewOwnerProductSyncManager.Part.IsDeleted)
					{
						result = (OrgSupplierPart)NewOwnerProductSyncManager.Part;
					}
				}
				return result;
			}
		}

		bool NewOwnerPartNoReadOnly => EntryInstruction?.Owner == null;

		[RelatedBusinessObject(nameof(NewOwnerProduct))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NewOwnerProducts))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_NewOwnerPartNo", Caption = "Owner Product")]
		[ReadOnlyMember(nameof(NewOwnerPartNoReadOnly))]
		public override ZString JI_NewOwnerPartNo
		{
			get { return base.JI_NewOwnerPartNo; }
			set
			{
				using (GetValidationSuspender())
				{
					bool reloadNeeded = (JI_NewOwnerPartNo != value);
					base.JI_NewOwnerPartNo = value;
					if (reloadNeeded && !IsCopying)
					{
						NewOwnerProductSyncManager.Refresh();
						if (JI_NewOwnerPartNo.IsEmpty)
						{
							JI_NewPartAttribute1 = ZString.Empty;
							JI_NewPartAttribute2 = ZString.Empty;
							JI_NewPartAttribute3 = ZString.Empty;
							JI_NewSerialNumber = ZString.Empty;
						}
					}
				}
				if (!IsCopying)
				{
					Validation.ValidateJI_NewOwnerPartNo();
				}
			}
		}

		bool NewPartAttributesReadOnly => JI_NewOwnerPartNo.IsEmpty;

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_NewPartAttribute1", Caption = "New Part Attribute 1", ShortCaption = "New Part Attrib. 1")]
		[ReadOnlyMember(nameof(NewPartAttributesReadOnly))]
		public override ZString JI_NewPartAttribute1
		{
			get { return base.JI_NewPartAttribute1; }
			set { base.JI_NewPartAttribute1 = value; }
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_NewPartAttribute2", Caption = "New Part Attribute 2", ShortCaption = "New Part Attrib. 2")]
		[ReadOnlyMember(nameof(NewPartAttributesReadOnly))]
		public override ZString JI_NewPartAttribute2
		{
			get { return base.JI_NewPartAttribute2; }
			set { base.JI_NewPartAttribute2 = value; }
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_NewPartAttribute3", Caption = "New Part Attribute 3", ShortCaption = "New Part Attrib. 3")]
		[ReadOnlyMember(nameof(NewPartAttributesReadOnly))]
		public override ZString JI_NewPartAttribute3
		{
			get { return base.JI_NewPartAttribute3; }
			set { base.JI_NewPartAttribute3 = value; }
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_NewSerialNumber", Caption = "New Serial Number", ShortCaption = "New Serial Num.")]
		[ReadOnlyMember(nameof(NewPartAttributesReadOnly))]
		public override ZString JI_NewSerialNumber
		{
			get { return base.JI_NewSerialNumber; }
			set { base.JI_NewSerialNumber = value; }
		}

		[ResourceStringData("AF0680E5-14C8-45D3-B26F-85DC303966F6", Caption = "Customs Supplier Part No.", ShortCaption = "Supplier Part No.", FullDescription = "The supplier's part number.")]
		public override ZString JI_CustomsSupplierPartNo
		{
			get => base.JI_CustomsSupplierPartNo;
			set => base.JI_CustomsSupplierPartNo = value;
		}

		[ResourceStringData("A1102F9B-0425-48B0-822E-324DD2AA68CB", Caption = "Customs Owner Part No.", ShortCaption = "Owner Part No.", FullDescription = "The owner's part number.")]
		public override ZString JI_CustomsOwnerPartNo
		{
			get => base.JI_CustomsOwnerPartNo;
			set => base.JI_CustomsOwnerPartNo = value;
		}

		public override ZString JI_PartNo
		{
			get { return base.JI_PartNo; }
			set
			{
				var oldValue = JI_PartNo;
				base.JI_PartNo = value;
				if (!IsCopying && oldValue != JI_PartNo)
				{
					DefaultOwnerProductDataIfNeeded();
					TrademarkStorageDocsGuid = Lookups.TrademarkEDocList.DefaultDocPk;
				}
			}
		}

		[DecimalPlaces(4)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_InvoiceQuantity", Caption = "Quantity", ShortCaption = "Qty", FullDescription = "The quantity of the dutiable imported goods, used in calculating Specific Duty.")]
		public override ZDecimal JI_InvoiceQuantity
		{
			get => base.JI_InvoiceQuantity;
			set
			{
				var oldValue = JI_InvoiceQuantity;
				base.JI_InvoiceQuantity = value;
				if (!IsCopying && oldValue != JI_InvoiceQuantity)
				{
					ChassisJobComInvLineRefsCollection.MarkAsNeedingValidationIncludingChildren();
					RebuildInvoiceQuantityAndUnitQtyResultIfNeeded();
					CalculateFromInvoiceQuantityToNetWeightIfRequired();
				}
			}
		}

		void CalculateFromInvoiceQuantityToNetWeightIfRequired()
		{
			if (JI_NetWeight.IsEmpty && CanConvertFromInvoiceQuantityToNetWeightUnit(JI_NetWeightUQ))
			{
				JI_NetWeight = Core.Constants.Weight.Convert(JI_InvoiceQuantity, UnitConverterHelper.ConvertToCW1StandardWeightUnit(JI_InvoiceUQ), JI_NetWeightUQ);
			}
		}

		bool CanConvertFromInvoiceQuantityToNetWeightUnit(ZString netWeightUnit)
		{
			return JI_InvoiceQuantity > 0m && Core.Constants.Weight.ContainsCode(UnitConverterHelper.ConvertToCW1StandardWeightUnit(JI_InvoiceUQ)) && Core.Constants.Weight.ContainsCode(netWeightUnit);
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_InvoiceUQ", Caption = "UQ", FullDescription = "The unit of the product quantity.")]
		public override ZString JI_InvoiceUQ
		{
			get => base.JI_InvoiceUQ;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_InvoiceUQ)))
				{
					var oldValue = JI_InvoiceUQ;
					base.JI_InvoiceUQ = value;
					if (!IsCopying && oldValue != JI_InvoiceUQ)
					{
						RebuildInvoiceQuantityAndUnitQtyResultIfNeeded();
						CalculateFromInvoiceQuantityToNetWeightIfRequired();
						CalculateFromNetWeightToInvoiceQuantityIfRequired();
					}
				}
			}
		}

		void RebuildInvoiceQuantityAndUnitQtyResultIfNeeded()
		{
			InvoiceHeader?.RebuildInvoiceQuantityAndUnitQtyResultIfNeeded();
		}

		public ZBool IsCarRelatedTariff => CommonHelper.CheckIsCarRelatedTariff(JI_Tariff);

		public ZBool IsCarRelatedDataEmpty
		{
			get
			{
				return JI_CarType.IsEmpty
					&& JI_CarCondition.IsEmpty
					&& JI_ModelYear.IsEmpty
					&& (!ChassisJobComInvLineRefsCollection.Any() || ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>().All(chassis => chassis.JG_ReferenceNumber.IsEmpty))
					&& JI_Transmission.IsEmpty
					&& JI_EngineType.IsEmpty
					&& JI_LHD.IsEmpty
					&& JI_HasCatalystConverter.IsEmpty
					&& JI_EquipmentPrintMode.IsEmpty
					&& JI_Displacement.IsEmpty
					&& JI_NumberOfDoor.IsEmpty
					&& JI_Seats.IsEmpty
					&& JI_Cylinders.IsEmpty
					&& JI_Gears.IsEmpty;
			}
		}

		public override ZString JI_Tariff
		{
			get => base.JI_Tariff;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
				{
					var oldValue = JI_Tariff;
					base.JI_Tariff = value;
					if (!IsCopying && oldValue != JI_Tariff)
					{
						ClearOrSetDefaultConcessionOrder();
						EmptyCarInfoRelatedFieldsIfNotCarRelatedTariff();
						if (UniversalTariff != null)
						{
							SetDefaultPrimaryPreferenceValue();
							SetDefaultProcedure();
						}

						if (!TariffUnitOfQuantity.IsEmpty)
						{
							CustomsQuantityConverter.CalculateFromNetWeightToCustomsQty();
						}
						else
						{
							ClearCustomsQuantityIfRequired();
						}

						if (IsImport)
						{
							ClearAndDefaultInvoiceLineTax();
						}
						if (!IsValidationSuspended)
						{
							var validation = Validation;
							validation.ValidateJI_Model();
							validation.ValidateJI_BrandName();
							validation.ValidateJI_Compositions();
							validation.ValidateJI_Procedure();
						}
					}
				}
			}
		}

		ZString TariffUnitOfQuantity => UniversalTariff?.ZZ1_ZZ8_UQ1 ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_NetWeight", Caption = "Net Weight", FullDescription = "The gross weight minus packaging (inner and outer packages) weight. System converts the weight in KGM automatically for customs declaration.")]
		public override ZDecimal JI_NetWeight
		{
			get => base.JI_NetWeight;
			set
			{
				var oldValue = JI_NetWeight;
				base.JI_NetWeight = value;
				if (oldValue != JI_NetWeight)
				{
					ClearCustomsQuantityIfRequired();
					CalculateFromNetWeightToInvoiceQuantityIfRequired();
				}
			}
		}

		void CalculateFromNetWeightToInvoiceQuantityIfRequired()
		{
			if (JI_InvoiceQuantity.IsEmpty && CanConvertFromNetWeightToCustomsUnit(JI_InvoiceUQ))
			{
				JI_InvoiceQuantity = Core.Constants.Weight.Convert(JI_NetWeight, JI_NetWeightUQ, UnitConverterHelper.ConvertToCW1StandardWeightUnit(JI_InvoiceUQ));
			}
		}

		public override void CalculateFromNetWeightToCustomsQty() => CustomsQuantityConverter.CalculateFromNetWeightToCustomsQty();

		public override ZString JI_NetWeightUQ
		{
			get => base.JI_NetWeightUQ;
			set
			{
				var oldValue = JI_NetWeightUQ;
				base.JI_NetWeightUQ = value;
				if (oldValue != JI_NetWeightUQ)
				{
					ClearCustomsQuantityIfRequired();
					CalculateFromNetWeightToInvoiceQuantityIfRequired();
					CalculateFromInvoiceQuantityToNetWeightIfRequired();
				}
			}
		}

		public ZDecimal CustomsQuantityInKG
		{
			get
			{
				if (JI_CustomsUnitQty == Constants.UnitOfQuantityCodes.Kilograms)
				{
					return JI_CustomsQuantity;
				}
				else if (JI_CustomsUnitQty == Constants.UnitOfQuantityCodes.Tonnes)
				{
					return JI_CustomsQuantity * 1000;
				}
				return 0m;
			}
		}

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new UniversalTariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>((true, true, false, true, true));

		[ReadOnlyMember(nameof(JI_CustomsSecondUnitQtyReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CustomsSecondUnitQty", Caption = "Statistical Quantity Unit", ShortCaption = "UQ", FullDescription = "Statistical Quantity Unit as indicated by the tariff item entered.")]
		[MaxLength(3)]
		public override ZString JI_CustomsSecondUnitQty
		{
			get => base.JI_CustomsSecondUnitQty;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsSecondUnitQty))
				{
					var oldValue = base.JI_CustomsSecondUnitQty;
					base.JI_CustomsSecondUnitQty = value;
					if (ClearCustomsSecondQuantityWhenUQSet)
					{
						JI_CustomsSecondQuantity = ZDecimal.Zero;
					}
					if (!IsCopying && oldValue != JI_CustomsSecondUnitQty)
					{
						SetInvoiceLineTaxDefaultQuantity();
					}
				}
			}
		}

		public override ZDecimal JI_CustomsQuantity
		{
			get => base.JI_CustomsQuantity;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsQuantity))
				{
					var oldValue = base.JI_CustomsQuantity;
					base.JI_CustomsQuantity = value;
					if (!IsCopying && oldValue != JI_CustomsQuantity)
					{
						SetInvoiceLineTaxDefaultQuantity();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(JI_CustomsSecondQuantityReadOnly))]
		[DecimalPlaces(4)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CustomsSecondQuantity", Caption = "Statistical Quantity", ShortCaption = "Stats. Qty", FullDescription = "Statistical Quantity as indicated by the Tariff item entered.")]
		public override ZDecimal JI_CustomsSecondQuantity
		{
			get => base.JI_CustomsSecondQuantity;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsSecondQuantity))
				{
					var oldValue = base.JI_CustomsSecondQuantity;
					base.JI_CustomsSecondQuantity = value;
					if (!IsCopying && oldValue != JI_CustomsSecondQuantity)
					{
						SetInvoiceLineTaxDefaultQuantity();
					}
				}
			}
		}

		void SetInvoiceLineTaxDefaultQuantity()
		{
			if (Taxes.Count > 0)
			{
				var unitOfMeasureValueList = EntryLineUniversalRate.GetUnitOfMeasureValueListByInvoiceLine(this);
				Taxes.Cast<JobComInvoiceLineTax>().ForEach(x => x.SetDefaultQuantity(unitOfMeasureValueList));
			}
		}

		protected bool ClearCustomsSecondQuantityWhenUQSet => JI_CustomsSecondQuantityReadOnly;

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly() => true;

		protected override bool GetJI_CustomsQuantityReadOnly() => true;

		protected override bool ClearCustomsQuantityWhenUQSet => false;

		ZBool JI_CustomsSecondUnitQtyReadOnly => tariffContainsCU2Unit;

		ZBool JI_CustomsSecondQuantityReadOnly => JI_CustomsSecondUnitQty.IsEmpty && tariffContainsCU2Unit;

		ZBool tariffContainsCU2Unit => Factory.GetValue(ref tariffContainsCU2UnitCached, () => !(UniversalTariff?.ZZ1_ZZ8_UQ2.IsEmpty ?? ZBool.True));
		CachedProperty<ZBool> tariffContainsCU2UnitCached;

		public bool IsModeOfStatisticsRequirePermitNumber => Factory.GetValue(ref isModeOfStatisticsRequirePermitNumber, () => IsExport && JI_Procedure.ToString() switch
		{
			ProcedureCodes._01 or ProcedureCodes._1A or ProcedureCodes._8A or ProcedureCodes._8D => true,
			_ => false,
		});
		CachedProperty<bool> isModeOfStatisticsRequirePermitNumber;

		[MaxLength(2)]
		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				var oldValue = JI_Procedure;
				base.JI_Procedure = value;
				if (!IsCopying && oldValue != JI_Procedure)
				{
					SetDefaultCusValueConvRatioIfNeeded();
					EntryInstruction?.Validation?.ValidateCEI_ReasonForDuty();
					SetDefaultUseOneTenthCV();
					SetDefaultRAPRORValuesIfNeeded();
					SetDefaultPaymentMethod();
					DefaultTaxPaymentMethodWhenProcedureChanged();
					if (!IsValidationSuspended)
					{
						Validation.ValidateJI_CustomsOwnerPartNo();
						Declaration?.FilteredInvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.Validation.ValidateJI_Procedure());
					}
				}
			}
		}

		void SetDefaultPaymentMethod()
		{
			if (IsImport)
			{
				if (IsROR)
				{
					var rorPaymentMethod = RORPaymentMethod.IsEmpty ? DutyTaxPaymentMethodList.Codes.RorPayment : RORPaymentMethod.ToString();
					JI_DtyPymntMthd = JI_VatPymntMthd = JI_TpfPymntMthd = rorPaymentMethod;
				}
				else
				{
					JI_DtyPymntMthd = GetDefaultPaymentMethod(UniversalReferenceConstants.RefCusProcedureAttributes.DTYPaymentMethod);
					JI_VatPymntMthd = GetDefaultPaymentMethod(UniversalReferenceConstants.RefCusProcedureAttributes.VATPaymentMethod);
					JI_TpfPymntMthd = GetDefaultPaymentMethod(UniversalReferenceConstants.RefCusProcedureAttributes.TPFPaymentMethod);
				}
			}
			else
			{
				JI_DtyPymntMthd = ZString.Empty;
				JI_VatPymntMthd = ZString.Empty;
				JI_TpfPymntMthd = ZString.Empty;
			}
		}

		ZString GetDefaultPaymentMethod(ZString name)
		{
			var result = ZString.Empty;
			var defaultDutyPaymentMethod = CusProcedure?.Attributes.FirstOrDefault(x => x.ZXB_Name == name)?.ZXB_Value ?? ZString.Empty;
			if (defaultDutyPaymentMethod == DutyTaxPaymentMethodList.Codes.NonCashPayment || defaultDutyPaymentMethod == UniversalReferenceConstants.PaymentMethods.OLDDEFERRED)
			{
				result = DutyTaxPaymentMethodList.Codes.NonCashPayment;
			}
			else if (defaultDutyPaymentMethod == DutyTaxPaymentMethodList.Codes.CashPayment || defaultDutyPaymentMethod == UniversalReferenceConstants.PaymentMethods.OLDCASH)
			{
				result = DutyTaxPaymentMethodList.Codes.CashPayment;
			}
			return result;
		}

		void EmptyCarInfoRelatedFieldsIfNotCarRelatedTariff()
		{
			if (!IsCarRelatedTariff)
			{
				JI_CarType = ZString.Empty;
				JI_Transmission = ZString.Empty;
				JI_EngineType = ZString.Empty;
				JI_LHD = ZString.Empty;
				JI_HasCatalystConverter = ZString.Empty;
				JI_EquipmentPrintMode = ZString.Empty;
				JI_CarCondition = ZString.Empty;
				JI_ModelYear = ZShort.Zero;
				JI_Displacement = ZString.Empty;
				JI_NumberOfDoor = ZShort.Zero;
				JI_Seats = ZShort.Zero;
				JI_Cylinders = ZShort.Zero;
				JI_Gears = ZShort.Zero;
				ChassisJobComInvLineRefsCollection.RemoveAndDeleteAll();
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_AlcoholPercentage", Caption = "Alcohol by Volume (%)", FullDescription = "The percentage of alcohol by volume.")]
		public override ZDecimal JI_AlcoholPercentage { get => base.JI_AlcoholPercentage; set => base.JI_AlcoholPercentage = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PreviousEntryNumber", Caption = "Previous Entry Number", FullDescription = "The previous entry number of the re-export/re-import.")]
		[MaxLength(14)]
		public override ZString JI_PreviousEntryNumber { get => base.JI_PreviousEntryNumber; set => base.JI_PreviousEntryNumber = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PreviousEntryLineNumber", Caption = "Previous Entry Line No.", FullDescription = "The previous entry line number of the re-export/re-import.")]
		[MaxLength(4)]
		public override ZShort JI_PreviousEntryLineNumber { get => base.JI_PreviousEntryLineNumber; set => base.JI_PreviousEntryLineNumber = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.UNDGSubs))]
		[MaxLength(4)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_HazMatCode", Caption = "DG Code", FullDescription = "The standard classification code for dangerous goods.")]
		public override ZString JI_HazMatCode { get => base.JI_HazMatCode; set => base.JI_HazMatCode = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_Compositions", Caption = "Specification", FullDescription = "The measurements, specification and component of the goods.")]
		public override ZString JI_Compositions { get => base.JI_Compositions; set => base.JI_Compositions = value; }

		[List(nameof(JI_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_OA_ManufacturerAddress", Caption = "Foreign Manufacturer", ShortCaption = "Foreign Manuf.")]
		public override ZGuid JI_OA_ManufacturerAddress { get => base.JI_OA_ManufacturerAddress; set => base.JI_OA_ManufacturerAddress = value; }

		protected override ZAddress GetNewJI_OA_ManufacturerAddress_ZAddress()
		{
			var result = base.GetNewJI_OA_ManufacturerAddress_ZAddress();
			result.GetDefaultAddress = (org) => org?.MainAddress?.PK ?? ZGuid.Empty;
			return result;
		}

		ZBool IsForCAHeaderByControllingAgency(string controllingAgency)
		{
			return InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.ControllingAgency == controllingAgency);
		}

		ZBool IsForCAHeaderByControllingAgency(string[] controllingAgencies)
		{
			return InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && controllingAgencies.Contains<string>(x.ControllingAgency));
		}

		public void AssignCMHeaderToInvoices(CusTWControllingMessageHeader cmHeader)
		{
			InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Where(x => x.ControllingMessageHeaderPK == cmHeader.PK && !x.IsLinkedCMHeader).ForEach(x => x.IsLinkedCMHeader = true);
		}

		#region HasLinkedCMHeader
		public ZBool HasLinkedCMHeader => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader);

		public ZPropertyInfo HasLinkedCMHeaderInfo => GetZPropertyInfo(nameof(HasLinkedCMHeader));

		ZBool IsForCMHeaderByMessageType(string messageType) => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.MessageType == messageType);

		public ZBool IsForCMHeaderByMessageTypes(params string[] messageTypes) => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && messageTypes.Contains<string>(x.MessageType));

		public InvoiceLineLinkControllingMsgHeader GetCMHeaderByMessageType(string messageType) => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.IsLinkedCMHeader && x.MessageType == messageType);

		public ZBool CertificateOfOriginSupported => IsForCMHeaderMessageTypeNX101;

		public ZPropertyInfo CertificateOfOriginSupportedInfo => GetZPropertyInfo(nameof(CertificateOfOriginSupported));

		public ZBool AlcoholSupported => IsForCMHeaderMessageTypeNX301_DN;

		public ZPropertyInfo AlcoholSupportedInfo => GetZPropertyInfo(nameof(AlcoholSupported));

		public ZBool TypeApprovalSupported => IsForCMHeaderMessageTypeNX301;

		public ZPropertyInfo TypeApprovalSupportedInfo => GetZPropertyInfo(nameof(TypeApprovalSupported));

		public ZBool AnimalAndPlantSupported => IsForCMHeaderMessageTypeNX401;

		public ZPropertyInfo AnimalAndPlantSupportedInfo => GetZPropertyInfo(nameof(AnimalAndPlantSupported));

		public ZBool FoodAndDrugSupported => IsForCMHeaderMessageTypeNX601 || IsForCMHeaderMessageTypeNX603;

		public ZPropertyInfo FoodAndDrugSupportedInfo => GetZPropertyInfo(nameof(FoodAndDrugSupported));

		public ZBool IsForCMHeaderMessageTypeNX101 => Factory.GetValue(ref isForCMHeaderMessageTypeNX101Cached, () => IsForCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101));
		CachedProperty<ZBool> isForCMHeaderMessageTypeNX101Cached;

		public ZBool IsForCMHeaderMessageTypeNX101CertificateType15 => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.MessageType == ControllingMessageTypeList.Codes.NX101 && x.CertificateType == CertificateTypeList.Codes.Code15);

		public ZBool IsForCMHeaderMessageTypeNX301 => Factory.GetValue(ref isForCMHeaderMessageTypeNX301Cached, () => IsForCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX301));
		CachedProperty<ZBool> isForCMHeaderMessageTypeNX301Cached;

		public ZBool IsForCMHeaderMessageTypeNX301_AX => Factory.GetValue(ref isForCMHeaderMessageTypeNX301_AXCached, () => IsForCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX301_AX));
		CachedProperty<ZBool> isForCMHeaderMessageTypeNX301_AXCached;

		public ZBool IsForCMHeaderMessageTypeNX301_DN => Factory.GetValue(ref isForCMHeaderMessageTypeNX301_DNCached, () => IsForCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX301_DN));
		CachedProperty<ZBool> isForCMHeaderMessageTypeNX301_DNCached;

		public ZBool IsForCMHeaderMessageTypeNX401 => Factory.GetValue(ref isForCMHeaderMessageTypeNX401Cached, () => IsForCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX401));
		CachedProperty<ZBool> isForCMHeaderMessageTypeNX401Cached;

		public ZBool IsForCMHeaderMessageTypeNX601 => Factory.GetValue(ref isForCMHeaderMessageTypeNX601Cached, () => IsForCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX601));
		CachedProperty<ZBool> isForCMHeaderMessageTypeNX601Cached;

		public ZBool IsForCMHeaderMessageTypeNX603 => Factory.GetValue(ref isForCMHeaderMessageTypeNX603Cached, () => IsForCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX603));
		CachedProperty<ZBool> isForCMHeaderMessageTypeNX603Cached;

		bool IsLinkedNX401CMHeaderWithAllBusinessType => Factory.GetValue(ref isLinkedNX401CMHeaderWithAllBusinessTypeCached, () => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.MessageType == ControllingMessageTypeList.Codes.NX401 && new[] { CPT_111_BusinessTypeList.Codes.QuarantineOfExportAnimal, CPT_111_BusinessTypeList.Codes.QuarantineOfImportAnimal, CPT_111_BusinessTypeList.Codes.QuarantineOfExportPlant, CPT_111_BusinessTypeList.Codes.QuarantineOfImportPlant }.Contains<string>(x.BusinessType)));
		CachedProperty<bool> isLinkedNX401CMHeaderWithAllBusinessTypeCached;

		bool IsLinkedNX401CMHeaderWithBusinessType3040 => Factory.GetValue(ref isLinkedNX401CMHeaderWithBusinessType3040Cached, () => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.MessageType == ControllingMessageTypeList.Codes.NX401 && new[] { CPT_111_BusinessTypeList.Codes.QuarantineOfExportAnimal, CPT_111_BusinessTypeList.Codes.QuarantineOfImportAnimal }.Contains<string>(x.BusinessType)));
		CachedProperty<bool> isLinkedNX401CMHeaderWithBusinessType3040Cached;

		bool IsLinkedNX401CMHeaderWithBusinessType40 => Factory.GetValue(ref isLinkedNX401CMHeaderWithBusinessType40Cached, () => IsImport && InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.MessageType == ControllingMessageTypeList.Codes.NX401 && x.BusinessType == CPT_111_BusinessTypeList.Codes.QuarantineOfImportAnimal));
		CachedProperty<bool> isLinkedNX401CMHeaderWithBusinessType40Cached;

		bool IsLinkedNX401CMHeaderWithBusinessType60 => Factory.GetValue(ref isLinkedNX401CMHeaderWithBusinessType60Cached, () => IsImport && InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.MessageType == ControllingMessageTypeList.Codes.NX401 && x.BusinessType == CPT_111_BusinessTypeList.Codes.QuarantineOfImportPlant));
		CachedProperty<bool> isLinkedNX401CMHeaderWithBusinessType60Cached;
		#endregion

		public ZBool HasLinkedToOtherSameTypeControllingMessageHeader(CusTWControllingMessageHeader header)
		{
			var result = false;
			if (header != null)
			{
				var controllingMessageType = header.TW1_ControllingMessageType;
				result = InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => !x.ControllingMessageHeaderPK.Equals(header.PK) && x.IsLinkedCMHeader && x.MessageType == controllingMessageType);
			}
			return result;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CA20", Caption = "20 - Bureau of Standards - Lot-by-Lot Inspection")]
		public ZBool IsForCAHeader20 => IsForCAHeaderByControllingAgency(ControllingAgencyList.Codes._20);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CACI", Caption = "CI - Bureau of Standards - Registration, IPO, Exemption from Inspection")]
		public ZBool IsForCAHeaderCI => IsForCAHeaderByControllingAgency(ControllingAgencyList.Codes.CI);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CA2Q", Caption = "2Q - Bureau of Standards - Self-Signed Certificate")]
		public ZBool IsForCAHeader2Q => IsForCAHeaderByControllingAgency(ControllingAgencyList.Codes._2Q);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CACD", Caption = "CD - Food and Drug Administration – Drug")]
		public ZBool IsForCAHeaderCD => IsForCAHeaderByControllingAgency(ControllingAgencyList.Codes.CD);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CAIF", Caption = "IF - Food and Drug Administration – Food")]
		public ZBool IsForCAHeaderIF => IsForCAHeaderByControllingAgency(ControllingAgencyList.Codes.IF);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CADH", Caption = "DH - Food and Drug Administration – Other")]
		public ZBool IsForCAHeaderDH => IsForCAHeaderByControllingAgency(ControllingAgencyList.Codes.DH);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CADN", Caption = "DN - National Treasury Administration")]
		public ZBool IsForCAHeaderDN => IsForCAHeaderByControllingAgency(ControllingAgencyList.Codes.DN);

		ZBool IsForCAHeaderFT => IsForCAHeaderByControllingAgency(ControllingAgencyList.Codes.FT);

		[ReadOnlyMember(nameof(CustomsThirdQuantityReadOnly))]
		[DecimalPlaces(4)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CustomsThirdQuantity", Caption = "Licensing Quantity", FullDescription = "The statistical quantity calculated based on the unit defined by the controlling agency.")]
		public override ZDecimal JI_CustomsThirdQuantity { get => base.JI_CustomsThirdQuantity; set => base.JI_CustomsThirdQuantity = value; }

		ZBool CustomsThirdQuantityReadOnly => Factory.GetValue(ref customsThirdQuantityReadOnlyCached, () => !IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX201_01, ControllingMessageTypeList.Codes.NX301, ControllingMessageTypeList.Codes.NX301_AX, ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX401, ControllingMessageTypeList.Codes.NX601, ControllingMessageTypeList.Codes.NX603));
		CachedProperty<ZBool> customsThirdQuantityReadOnlyCached;

		[ReadOnlyMember(nameof(CustomsThirdQuantityReadOnly))]
		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CustomsThirdUnitQty", Caption = "CA Customs UQ")]
		public override ZString JI_CustomsThirdUnitQty { get => base.JI_CustomsThirdUnitQty; set => base.JI_CustomsThirdUnitQty = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DutyOrTaxPaymentMethodList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_DtyPymntMthd", Caption = "Duty Payment Method", FullDescription = "The payment method of import duty.")]
		public override ZString JI_DtyPymntMthd { get => base.JI_DtyPymntMthd; set => base.JI_DtyPymntMthd = value; }

		public ZString JI_DtyPymntMthdDescription => GetPaymentMethodDescription(JI_DtyPymntMthd, FreePaymentMethodDescriptions.DutyFree);

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DutyOrTaxPaymentMethodList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_VatPymntMthd", Caption = "Business Tax Payment Method", FullDescription = "The payment method for business tax.")]
		public override ZString JI_VatPymntMthd { get => base.JI_VatPymntMthd; set => base.JI_VatPymntMthd = value; }

		public ZString JI_VatPymntMthdDescription => GetPaymentMethodDescription(JI_VatPymntMthd, FreePaymentMethodDescriptions.TaxFree);

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TpfPaymentMethodList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_TpfPymntMthd", Caption = "TPF Payment Method", FullDescription = "The payment method of Trade Promotion Fee.")]
		public override ZString JI_TpfPymntMthd { get => base.JI_TpfPymntMthd; set => base.JI_TpfPymntMthd = value; }

		public ZString JI_TpfPymntMthdDescription => GetPaymentMethodDescription(JI_TpfPymntMthd, FreePaymentMethodDescriptions.TaxFree);

		ZString GetPaymentMethodDescription(ZString paymentMethodCode, ZString descriptionWhenCodeIsEmpty)
		{
			var result = ZString.Empty;
			var paymentMethodList = Lookups.DutyOrTaxPaymentMethodList;
			if (paymentMethodList.ContainsCode(paymentMethodCode))
			{
				result = paymentMethodList.GetDescriptionFromCode(paymentMethodCode);
			}
			else if (paymentMethodCode.IsEmpty)
			{
				result = descriptionWhenCodeIsEmpty;
			}
			return result;
		}

		#region CertificateOfOriginCusSupporting

		public CertificateOfOriginCusSupporting CertificateOfOriginCusSupporting
		{
			get
			{
				if (certificateOfOriginCusSupporting?.IsDeleted ?? true)
				{
					certificateOfOriginCusSupporting = CertificateOfOriginCusSupportingCollection.FirstOrDefault() ?? CertificateOfOriginCusSupportingCollection.AddNew();
				}
				return certificateOfOriginCusSupporting;
			}
		}
		CertificateOfOriginCusSupporting certificateOfOriginCusSupporting;

		[ChildEditable(true)]
		public CertificateOfOriginCusSupportingCollection CertificateOfOriginCusSupportingCollection
		{
			get { return fCertificateOfOriginCusSupportingCollection ?? (fCertificateOfOriginCusSupportingCollection = GetCertificateOfOriginCusSupportingCollection()); }
		}
		CertificateOfOriginCusSupportingCollection fCertificateOfOriginCusSupportingCollection;

		CertificateOfOriginCusSupportingCollection GetCertificateOfOriginCusSupportingCollection()
		{
			var result = new CertificateOfOriginCusSupportingCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[UniversalCopyExtraProperty]
		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|CertificateOfOriginNumber", Caption = "Certificate of Origin", FullDescription = "The certificate of origin number requested by customs from the importer/exporter.")]
		public ZString CertificateOfOriginNumber
		{
			get
			{
				return CertificateOfOriginCusSupporting?.CSI_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(CertificateOfOriginNumberInfo, value);
				CertificateOfOriginCusSupporting.CSI_ReferenceNumber = value;
				CertificateOfOriginNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CertificateOfOriginNumberInfo
		{
			get { return (certificateOfOriginCusSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(CertificateOfOriginNumber)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginNumber), x => certificateOfOriginCusSupporting.CSI_ReferenceNumberInfo); }
		}

		[UniversalCopyExtraProperty]
		[MaxLength(4)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|CertificateOfOriginNumberItemNumber", Caption = "Certificate of Origin Line No.", FullDescription = "The line numbers of the certificate of origin requested by customs from the importer/exporter.")]
		public ZInt CertificateOfOriginNumberItemNumber
		{
			get
			{
				return CertificateOfOriginCusSupporting?.CSI_LineNo ?? ZShort.Zero;
			}
			set
			{
				CertificateOfOriginCusSupporting.CSI_LineNo = value;
				CertificateOfOriginNumberItemNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CertificateOfOriginNumberItemNumberInfo
		{
			get { return (certificateOfOriginCusSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(CertificateOfOriginNumberItemNumber)) : GetWrappedZPropertyInfo(nameof(CertificateOfOriginNumberItemNumber), x => certificateOfOriginCusSupporting.CSI_LineNoInfo); }
		}
		#endregion

		#region CitesPermitCusSupporting

		public CitesPermitCusSupporting CitesPermitCusSupporting
		{
			get
			{
				if (citesPermitCusSupporting?.IsDeleted ?? true)
				{
					citesPermitCusSupporting = CitesPermitCusSupportingCollection.FirstOrDefault() ?? CitesPermitCusSupportingCollection.AddNew();
				}
				return citesPermitCusSupporting;
			}
		}
		CitesPermitCusSupporting citesPermitCusSupporting;

		[ChildEditable(true)]
		public CitesPermitCusSupportingCollection CitesPermitCusSupportingCollection
		{
			get { return fCitesPermitCusSupportingCollection ?? (fCitesPermitCusSupportingCollection = GetCitesPermitCusSupportingCollection()); }
		}
		CitesPermitCusSupportingCollection fCitesPermitCusSupportingCollection;

		CitesPermitCusSupportingCollection GetCitesPermitCusSupportingCollection()
		{
			var result = new CitesPermitCusSupportingCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[UniversalCopyExtraProperty]
		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|CitesPermit", Caption = "CITES Import Permit", FullDescription = "The import permit number of the Convention on International Trade in Endangered Species.")]
		public ZString CitesPermit
		{
			get
			{
				return CitesPermitCusSupporting?.CSI_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(CitesPermitInfo, value);
				CitesPermitCusSupporting.CSI_ReferenceNumber = value;
				CitesPermitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CitesPermitInfo
		{
			get { return (citesPermitCusSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(CitesPermit)) : GetWrappedZPropertyInfo(nameof(CitesPermit), x => citesPermitCusSupporting.CSI_ReferenceNumberInfo); }
		}

		#endregion

		#region HighTechLicenseSupporting

		public HighTechLicenseCusSupporting HighTechLicenseSupporting
		{
			get
			{
				if (highTechLicenseSupporting?.IsDeleted ?? true)
				{
					highTechLicenseSupporting = HighTechLicenseCusSupportingCollection.FirstOrDefault() ?? HighTechLicenseCusSupportingCollection.AddNew();
				}
				return highTechLicenseSupporting;
			}
		}

		HighTechLicenseCusSupporting highTechLicenseSupporting;

		[ChildEditable(true)]
		public HighTechLicenseCusSupportingCollection HighTechLicenseCusSupportingCollection
		{
			get { return fHighTechLicenseCusSupportingCollection ?? (fHighTechLicenseCusSupportingCollection = GetHighTechLicenseCusSupportingCollection()); }
		}

		HighTechLicenseCusSupportingCollection fHighTechLicenseCusSupportingCollection;

		HighTechLicenseCusSupportingCollection GetHighTechLicenseCusSupportingCollection()
		{
			var result = new HighTechLicenseCusSupportingCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[UniversalCopyExtraProperty]
		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|HighTechLicense", Caption = "SHTC Import Permit", FullDescription = "The import permit number of the Strategic High Tech Commodity.")]
		public ZString HighTechLicense
		{
			get
			{
				return HighTechLicenseSupporting?.CSI_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(HighTechLicenseInfo, value);
				HighTechLicenseSupporting.CSI_ReferenceNumber = value;
				HighTechLicenseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo HighTechLicenseInfo
		{
			get { return (highTechLicenseSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(HighTechLicense)) : GetWrappedZPropertyInfo(nameof(HighTechLicense), x => highTechLicenseSupporting.CSI_ReferenceNumberInfo); }
		}

		#endregion

		#region Suspend JobComInvoiceLineTax Defaulting

		internal bool IsJobComInvoiceLineTaxDefaultingSuspended => jobComInvoiceLineTaxDefaultingSuspenderDefaultingSuspenderIndex > 0;

		internal IDisposable SuspendJobComInvoiceLineTaxDefaulting()
		{
			return new JobComInvoiceLineTaxDefaultingSuspender(this);
		}

		int jobComInvoiceLineTaxDefaultingSuspenderDefaultingSuspenderIndex;

		class JobComInvoiceLineTaxDefaultingSuspender : IDisposable
		{
			public JobComInvoiceLineTaxDefaultingSuspender(JobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
				this.invoiceLine.jobComInvoiceLineTaxDefaultingSuspenderDefaultingSuspenderIndex++;
			}

			readonly JobComInvoiceLine invoiceLine;

			#region IDisposable Members

			public void Dispose()
			{
				invoiceLine.jobComInvoiceLineTaxDefaultingSuspenderDefaultingSuspenderIndex--;
			}

			#endregion
		}
		#endregion

		#region JobComInvoiceLineTax Defaulting

		internal void ClearAndDefaultInvoiceLineTax()
		{
			using (SuspendUOMDefaulting())
			using (SuspendJobComInvoiceLineTaxDefaulting())
			{
				Taxes.RemoveAndDeleteAll();

				foreach (var pair in GetValidRefCusTariffSortedDictionary())
				{
					var invoiceLineTax = Taxes.AddNew();
					using (invoiceLineTax.SuspendInvoiceLineTaxDefaulting())
					{
						invoiceLineTax.JLT_Type = pair.Key.ZZI_TariffType;
						if (pair.Value.Count == 1)
						{
							invoiceLineTax.JLT_Tariff = pair.Value[0].ZZ1_TariffCode.Left(invoiceLineTax.JLT_TariffInfo.MaxLength);
						}
					}
				}
			}
		}

		internal IDictionary<RefCusTariffType, List<TariffView>> GetValidRefCusTariffSortedDictionary()
		{
			return TariffHelper.GetValidRefCusTariffSortedDictionary(Factory, UniversalTariff, JI_Tariff, EffectiveAssessmentDate);
		}
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void DefaultOwnerProductDataIfNeeded()
		{
			OrgSupplierPart newOwnerPart;
			var partNo = JI_PartNo;
			if (JI_NewOwnerPartNo.IsEmpty && !partNo.IsEmpty)
			{
				var owner = EntryInstruction?.Owner;
				if (owner != null)
				{
					var loadResults = JobComInvoiceLinePartSynchronisationManager.LoadResults(Factory, TypeOfPartUsed, partNo, owner, null, IsDrawback, IsForExportSectionOfDrawback, IsForImportSectionOfDrawback, InvoiceHeader.IsExport);
					newOwnerPart = (OrgSupplierPart)loadResults.BestMatchingProduct;
					if (newOwnerPart != null)
					{
						JI_NewOwnerPartNo = partNo;
					}
					if (!JI_NewOwnerPartNo.IsEmpty)
					{
						var importerMiscServ = Importer?.MiscServ;
						var ownerPartAttributeDetails = GetOwnerPartAttributeDetails(owner.MiscServ).ToArray();
						if (importerMiscServ != null && ownerPartAttributeDetails.Length > 0)
						{
							DefaultOwnerPartAttribIfNeeded(importerMiscServ.OM_IMPartAttrib1NameMultilingual, importerMiscServ.OM_IMPartAttrib1Type, JI_PartAttrib1, ownerPartAttributeDetails);
						}
					}
				}
			}
		}

		IEnumerable<OwnerPartAttributeDetail> GetOwnerPartAttributeDetails(OrgMiscServ miscServ)
		{
			if (miscServ != null)
			{
				var detail = GetOwnerPartAttributeDetail(miscServ, 1);
				if (detail != null)
				{
					yield return detail;
				}
				detail = GetOwnerPartAttributeDetail(miscServ, 2);
				if (detail != null)
				{
					yield return detail;
				}
				detail = GetOwnerPartAttributeDetail(miscServ, 3);
				if (detail != null)
				{
					yield return detail;
				}
			}
		}

		void DefaultOwnerPartAttribIfNeeded(ZString partAttribName, ZString partAttribType, ZString partAttribValue, OwnerPartAttributeDetail[] ownerPartAttributeDetails)
		{
			if (!partAttribName.IsEmpty && !partAttribType.IsEmpty)
			{
				var ownerPartAttributeDetail = ownerPartAttributeDetails.FirstOrDefault(x => x.Name.EqualsIgnoringCase(partAttribName) && x.Type.EqualsIgnoringCase(partAttribType));
				if (ownerPartAttributeDetail != null)
				{
					SetOwnerPartAttributeIfPossible(ownerPartAttributeDetail.Info, ownerPartAttributeDetail.Position, partAttribValue);
				}
			}
		}

		void SetOwnerPartAttributeIfPossible(ZPropertyInfo info, int position, ZString partAttribValue)
		{
			var partRelation = NewOwnerProduct?.RelatedOrganisations?.FindByOrganisationAndRelationship(EntryInstruction?.Owner, OrgPartRelation.RelationshipTypes.Owner);
			if (partRelation == null || UsePartAttrib(partRelation, position))
			{
				info.Value = partAttribValue;
			}
		}

		bool UsePartAttrib(OrgPartRelation partRelation, int position)
		{
			var info = partRelation.ZPropertyInfoHash.GetPropertySafe(OrgPartRelation.Schema.OU_UsePartAttrib1.Substring(0, OrgPartRelation.Schema.OU_UsePartAttrib1.Length - 1) + position.ToString(CultureInfo.InvariantCulture)) as ZPropertyInfoBool;
			return info != null && info.Value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "GetOwnerPartAttributeDetails no need to be translated")]
		OwnerPartAttributeDetail GetOwnerPartAttributeDetail(OrgMiscServ orgMiscServ, int attributeNo)
		{
			OwnerPartAttributeDetail result = null;

			if (attributeNo == 1)
			{
				result = OwnerPartAttributeDetail.New(orgMiscServ.OM_IMPartAttrib1NameMultilingual, orgMiscServ.OM_IMPartAttrib1Type, attributeNo, JI_NewPartAttribute1Info);
			}
			else if (attributeNo == 2)
			{
				result = OwnerPartAttributeDetail.New(orgMiscServ.OM_IMPartAttrib2NameMultilingual, orgMiscServ.OM_IMPartAttrib2Type, attributeNo, JI_NewPartAttribute2Info);
			}
			else if (attributeNo == 3)
			{
				result = OwnerPartAttributeDetail.New(orgMiscServ.OM_IMPartAttrib3NameMultilingual, orgMiscServ.OM_IMPartAttrib3Type, attributeNo, JI_NewPartAttribute3Info);
			}

			return result;
		}

		public IDictionary<ZString, Type> GetJobComInvLineRefsTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ JobComInvLineRefsType.Codes.AssignedNumber, typeof(AssignedJobComInvLineRefs) },
				{ JobComInvLineRefsType.Codes.Chassis, typeof(ChassisJobComInvLineRefs) },
				{ JobComInvLineRefsType.Codes.StorageAndShippingCondition, typeof(StorageAndShippingConditionJobComInvLineRefs) }
			};
			return result;
		}

		public override void UpdateDetailsFromProductOnPartChangeCore()
		{
			base.UpdateDetailsFromProductOnPartChangeCore();

			var part = Part;
			if (part != null)
			{
				JI_BrandName = part.OP_Brand.Left(50);
				JI_Model = part.OP_Model;
			}
		}

		void SetDeclarationGoodsDescription(CusClassPartPivot pivot)
		{
			var goodDescMode = pivot.CI_DeclGoodsDescMode;
			switch (goodDescMode)
			{
				case DeclarationGoodsDescriptionModeList.Codes.BTH:
				case DeclarationGoodsDescriptionModeList.Codes.ENG:
					var pivotDescription = pivot.CI_Description;
					JI_Description = pivotDescription.IsEmpty ? Part?.OP_Desc ?? ZString.Empty : pivotDescription;
					if (goodDescMode == DeclarationGoodsDescriptionModeList.Codes.BTH)
					{
						JI_NDescription = pivot.CI_NDescription;
					}
					break;
				case DeclarationGoodsDescriptionModeList.Codes.CHT:
					JI_NDescription = pivot.CI_NDescription;
					break;
			}
		}

		protected override bool ShouldSetDescriptionWhenClassificationChanges => false;

		protected override bool ShouldSetDescriptionWhenTariffChanges => false;

		public override bool ShouldSetDescriptionWhenPartNoChanges => false;

		public override void UpdateDetailsFromPivotOnPartChangeCore()
		{
			base.UpdateDetailsFromPivotOnPartChangeCore();
			var pivot = (CusClassPartPivot)Pivot;
			if (pivot != null)
			{
				JI_CountryOfOrigin = pivot.CI_RN_NKCountryOfOrigin;
				JI_Compositions = pivot.CI_Compositions;
				SetDeclarationGoodsDescription(pivot);
				JI_CustomsOwnerPartNo = pivot.CI_CustomsOwnerPartNo;
				JI_CustomsSupplierPartNo = pivot.CI_CustomsSupplierPartNo;

				if (IsImport)
				{
					JI_AlcoholPercentage = pivot.CI_AlcoholPercentage;
					JI_TariffAdditionalCode = pivot.CI_TariffAdditionalCode;
					JI_Procedure = pivot.CI_DutyTreatment;
				}
				else if (IsExport)
				{
					JI_Procedure = pivot.CI_ModeOfStatistics;
				}

				if (!pivot.CI_Price.IsEmpty)
				{
					if (JI_RX_NKLinePriceCurr.IsEmpty || JI_RX_NKLinePriceCurr == pivot.CI_PriceCurr)
					{
						JI_EnteredUnitPrice = pivot.CI_Price;
					}
				}

				if (pivot.ProductPermitCusSupportingCollection.Count > 0)
				{
					var existingData = PermitCusSupportingCollection.Cast<PermitCusSupporting>().ToList();
					foreach (var productPermitCusSupporting in pivot.ProductPermitCusSupportingCollection.Cast<ProductPermitCusSupporting>().Where(x => !x.CSI_ReferenceNumber.IsEmpty))
					{
						var existedElement = existingData.Any(x => x.CSI_LineNo == productPermitCusSupporting.CSI_LineNo && x.CSI_ReferenceNumber == productPermitCusSupporting.CSI_ReferenceNumber);
						if (!existedElement)
						{
							var permitCusSupporting = PermitCusSupportingCollection.AddNew();
							permitCusSupporting.CSI_ReferenceNumber = productPermitCusSupporting.CSI_ReferenceNumber;
							permitCusSupporting.CSI_LineNo = productPermitCusSupporting.CSI_LineNo;
							existingData.Add(permitCusSupporting);
						}
					}
				}

				if (pivot.AssignedCusClassPartPivotRefCollection.Count > 0)
				{
					var existingData = AssignedJobComInvLineRefsCollection.Cast<AssignedJobComInvLineRefs>().ToList();
					foreach (var assignedCusClassPartPivotRef in pivot.AssignedCusClassPartPivotRefCollection.Cast<AssignedCusClassPartPivotRef>().Where(x => !x.CIR_ReferenceNumber.IsEmpty))
					{
						var existedElement = existingData.Any(x => x.JG_ReferenceNumber == assignedCusClassPartPivotRef.CIR_ReferenceNumber);
						if (!existedElement)
						{
							var assignedJobComInvLineRefs = AssignedJobComInvLineRefsCollection.AddNew();
							assignedJobComInvLineRefs.JG_ReferenceNumber = assignedCusClassPartPivotRef.CIR_ReferenceNumber;
							existingData.Add(assignedJobComInvLineRefs);
						}
					}
				}
			}
		}

		protected override void SetTariffEtcDataFromProductsPivotCore(BaseCusClassPartPivot pivot)
		{
			base.SetTariffEtcDataFromProductsPivotCore(pivot);
			var cusClassPartPivot = (CusClassPartPivot)pivot;

			CommonHelper.UpdateIfNotEmpty(JI_CustomsOwnerPartNoInfo, cusClassPartPivot.CI_CustomsOwnerPartNo);
			CommonHelper.UpdateIfNotEmpty(JI_CustomsSupplierPartNoInfo, cusClassPartPivot.CI_CustomsSupplierPartNo);
			CommonHelper.UpdateIfNotEmpty(JI_InvoiceUQInfo, cusClassPartPivot.CI_PartPivotUOM);

			CopyValuesFromProductsPivotToCarInfo(cusClassPartPivot);
		}

		void CopyValuesFromProductsPivotToCarInfo(CusClassPartPivot pivot)
		{
			if (IsImport && IsCarRelatedTariff)
			{
				CommonHelper.UpdateIfNotEmpty(JI_CarTypeInfo, pivot.CI_CarType);
				CommonHelper.UpdateIfNotEmpty(JI_TransmissionInfo, pivot.CI_Transmission);
				CommonHelper.UpdateIfNotEmpty(JI_EngineTypeInfo, pivot.CI_EngineType);
				CommonHelper.UpdateIfNotEmpty(JI_LHDInfo, pivot.CI_LHD);
				CommonHelper.UpdateIfNotEmpty(JI_HasCatalystConverterInfo, pivot.CI_HasCatalystConverter);
				CommonHelper.UpdateIfNotEmpty(JI_EquipmentPrintModeInfo, pivot.CI_EquipmentPrintMode);
				CommonHelper.UpdateIfNotEmpty(JI_CarConditionInfo, pivot.CI_CarCondition);
				CommonHelper.UpdateIfNotEmpty(JI_ModelYearInfo, pivot.CI_ModelYear);
				CommonHelper.UpdateIfNotEmpty(JI_DisplacementInfo, pivot.CI_Displacement);
				CommonHelper.UpdateIfNotEmpty(JI_NumberOfDoorInfo, pivot.CI_NumberOfDoor);
				CommonHelper.UpdateIfNotEmpty(JI_SeatsInfo, pivot.CI_Seats);
				CommonHelper.UpdateIfNotEmpty(JI_CylindersInfo, pivot.CI_Cylinders);
				CommonHelper.UpdateIfNotEmpty(JI_GearsInfo, pivot.CI_Gears);
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceLineFetchStrategy(this);
		}

		class OwnerPartAttributeDetail
		{
			OwnerPartAttributeDetail() { }

			public static OwnerPartAttributeDetail New(ZString name, ZString type, int position, ZPropertyInfo info)
			{
				return !name.IsEmpty && !type.IsEmpty ? new OwnerPartAttributeDetail() { Name = name, Type = type, Info = info, Position = position } : null;
			}

			public ZString Name { get; private set; }
			public ZString Type { get; private set; }
			public ZPropertyInfo Info { get; private set; }

			public int Position { get; private set; }
		}

		protected override CurrencyConverter GetCurrencyConverter()
		{
			return InvoiceHeader == null
				? new RefCurrencyCurrencyConverter(Factory, false) { DateForRate = ZDateTime.Today, RateType = ExchangeRateType.Customs }
				: new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, this, false);
		}

		#region CustomsQuantityConverter
		protected override Customs.Business.BaseCustomsQuantityConverter GetCustomsQuantityConverter() => new CustomsQuantityConverter(this);

		public override bool CanConvertFromNetWeightToCustomsUnit(ZString customsUnit)
		{
			return UnitConverterHelper.CanConvertFromNetWeightToCustomsUnit(JI_NetWeight, JI_NetWeightUQ, customsUnit);
		}
		#endregion

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Taiwan;
		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		DocumentaryQuantityConverter documentaryQuantityConverter;
		public DocumentaryQuantityConverter DocumentaryQuantityConverter
		{
			get
			{
				if (documentaryQuantityConverter == null)
				{
					documentaryQuantityConverter = new DocumentaryQuantityConverter(this);
				}
				return documentaryQuantityConverter;
			}
		}
		#endregion

		#region ICurrencyConverterDataProviderWithFixedExRates
		string ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRateCurrencyCode => InvoiceHeaderCurrencyConverterDataProvider.FixedExchangeRateCurrencyCode;

		decimal ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRate => InvoiceHeaderCurrencyConverterDataProvider.FixedExchangeRate;

		public ZDateTime DateOfValuation
		{
			get
			{
				var dateOfValuation = ZDateTime.Empty;
				var entryInstruction = EntryInstruction;
				if (entryInstruction != null)
				{
					dateOfValuation = entryInstruction.CEI_DateForDuty;
				}
				if (!dateOfValuation.IsValid)
				{
					dateOfValuation = ZDateTime.Today;
				}
				return dateOfValuation;
			}
		}

		ExchangeRateType ICurrencyConverterDataProvider.RateType => InvoiceHeaderCurrencyConverterDataProvider.RateType;

		int ICurrencyConverterDataProvider.MaximumDaysToFallback => InvoiceHeaderCurrencyConverterDataProvider.MaximumDaysToFallback;

		GlbCompany ICurrencyConverterDataProvider.Company => InvoiceHeaderCurrencyConverterDataProvider.Company;

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride => InvoiceHeaderCurrencyConverterDataProvider.LocalCurrencyCodeOverride;

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride => InvoiceHeaderCurrencyConverterDataProvider.IsReciprocalOverride;

		ICurrencyConverterDataProviderWithFixedExRates InvoiceHeaderCurrencyConverterDataProvider => InvoiceHeader;
		#endregion

		[ChildEditable]
		public InvoiceLineLinkControllingMsgHeaderCollection InvoiceLineLinkControllingMsgHeaders
		{
			get
			{
				if (invoiceLineLinkControllingMsgHeaders == null)
				{
					invoiceLineLinkControllingMsgHeaders = new InvoiceLineLinkControllingMsgHeaderCollection(this, RefreshBindingAndClearValuesWhenReadOnly);
					invoiceLineLinkControllingMsgHeaders.Load();
					RegisterEditableChildObject(invoiceLineLinkControllingMsgHeaders);
				}
				return invoiceLineLinkControllingMsgHeaders;
			}
		}

		ZBool IsInvoiceLineLinkControllingMsgHeadersLoaded => invoiceLineLinkControllingMsgHeaders != null;

		public void RefreshInvoiceLineLinkControllingMsgHeaderLinkIfNeeded(ZGuid controllingMessageHeaderPK, ZBool link)
		{
			if (IsInvoiceLineLinkControllingMsgHeadersLoaded)
			{
				var invoiceLineLinkControllingMsgHeaders = InvoiceLineLinkControllingMsgHeaders;
				if (invoiceLineLinkControllingMsgHeaders.Count > 0)
				{
					var theRelatedInvoiceLineLinkControllingMsgHeader = invoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(c => c.Invoiceline.PK == PK && c.ControllingMessageHeaderPK == controllingMessageHeaderPK);
					if (theRelatedInvoiceLineLinkControllingMsgHeader != null)
					{
						theRelatedInvoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = link;
					}
				}
			}
		}

		internal bool IsLinkedNX101WithCertificateType(params string[] certificateTypes)
		{
			return InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.MessageType == ControllingMessageTypeList.Codes.NX101 && certificateTypes.Contains<string>(x.CertificateType));
		}

		public bool IsLinkedNX101WithCertificate19 => Factory.GetCached(ref isLinkedNX101WithCertificate19, () => IsLinkedNX101WithCertificateType(CertificateTypeList.Codes.Code19));
		CachedProperty<bool> isLinkedNX101WithCertificate19;

		public bool IsLinkedNX101WithCertificate15 => Factory.GetCached(ref isLinkedNX101WithCertificate15, () => IsLinkedNX101WithCertificateType(CertificateTypeList.Codes.Code15));
		CachedProperty<bool> isLinkedNX101WithCertificate15;

		internal ZString NX101CertificateType => Factory.GetValue(ref nx101CertificateTypeCached, () => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.MessageType == ControllingMessageTypeList.Codes.NX101 && x.IsLinkedCMHeader)?.CertificateType ?? ZString.Empty);
		CachedProperty<ZString> nx101CertificateTypeCached;

		internal bool IsInvoiceDateAndNumberRequired => Factory.GetCached(ref isInvoiceDateAndNumberRequired, () => IsLinkedNX101WithCertificateType(CertificateTypeList.Codes.Code9, CertificateTypeList.Codes.Code11, CertificateTypeList.Codes.Code13, CertificateTypeList.Codes.Code14, CertificateTypeList.Codes.Code15, CertificateTypeList.Codes.Code18, CertificateTypeList.Codes.Code19));
		CachedProperty<bool> isInvoiceDateAndNumberRequired;

		protected override void FinishUniversalCopyCore(BusinessObjectUniversalCopyFactoryService factoryCopyService)
		{
			base.FinishUniversalCopyCore(factoryCopyService);
			factoryCopyService?.AddOnCopyFinishedAction(() =>
			{
				if (JI_CEI.IsEmpty)
				{
					var entryInstructionPK = Declaration?.CusEntryInstruction.PK ?? ZGuid.Empty;
					if (!entryInstructionPK.IsEmpty)
					{
						var line = (IBusinessObjectInternals)this;
						line.IsCopying = true;
						try
						{
							JI_CEI = entryInstructionPK;
							InvoiceLineLinkControllingMsgHeaders.Load();
						}
						finally
						{
							line.IsCopying = false;
						}
					}
				}
			});
		}

		[UniversalCopyExtraProperty(CustomCopyTemplateNode = nameof(GetMsgHeadersGenPivotsCopyTemplateNode))]
		[ChildEditable]
		public InvoiceLineRelatedControllingMsgHeadersGenPivotCollection InvoiceLineRelatedControllingMsgHeadersGenPivots
		{
			get
			{
				if (invoiceLineRelatedControllingMsgHeadersGenPivots == null)
				{
					invoiceLineRelatedControllingMsgHeadersGenPivots = new InvoiceLineRelatedControllingMsgHeadersGenPivotCollection(this);
					invoiceLineRelatedControllingMsgHeadersGenPivots.Load();
					RegisterEditableChildObject(invoiceLineRelatedControllingMsgHeadersGenPivots);
				}
				return invoiceLineRelatedControllingMsgHeadersGenPivots;
			}
		}
		InvoiceLineRelatedControllingMsgHeadersGenPivotCollection invoiceLineRelatedControllingMsgHeadersGenPivots;

		CopyTemplateNode GetMsgHeadersGenPivotsCopyTemplateNode()
		{
			var innerNode = new EntityCopyTemplateNode { Name = "InvoiceLineRelatedControllingMsgHeadersGenPivots" };
			innerNode.Nodes.Add(new PropertyCopyTemplateNode { Name = GenPivotSchema.Constants.XX_Relation2ID, CopyMethod = CopyMethod.Copy });
			return new CollectionCopyTemplateNode { InnerNode = innerNode, Name = innerNode.Name, CopyMethod = CollectionCopyMethod.All, ItemPropertyName = GenPivotSchema.Constants.XX_Relation1ID };
		}

		void RefreshBindingAndClearValuesWhenReadOnly()
		{
			if (!IsDeleted)
			{
				using (GetValidationSuspender())
				{
					RefreshBindingAndClearValueWhenReadOnly(JI_BottledDateInfo, ZDateTime.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_ExpirationDateInfo, ZDateTime.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_AlcoholEndOfShelfLifeInfo, ZDateTime.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_CustomsThirdQuantityInfo, ZDecimal.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_CustomsThirdUnitQtyInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_TariffExtensionCodeInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_ProductThicknessInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_ProductGradeInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_InnerPackTypeInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_InnerPackingMaterialInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_InnerPackDescriptionInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_GoodsTypeInfo, ZString.Empty, null, () => !base.JI_GoodsType.IsEmpty && !Lookups.GoodsTypeList.ContainsCode(base.JI_GoodsType));
					RefreshBindingAndClearValueWhenReadOnly(JI_QuarantineFeaturesInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_QuarantineTreatmentInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_VaccinationTypeDateInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_MicrochipIDInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_AnimalAgeYearInfo, ZInt.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_AnimalAgeMonthInfo, ZInt.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_AnimalMaleQtyInfo, ZInt.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_AnimalFemaleQtyInfo, ZInt.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_AlcoholAgeInfo, ZInt.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_AlcoholYearInfo, ZInt.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_AlteredLotNoAmtInfo, ZDecimal.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_RemovedLotNoAmtInfo, ZDecimal.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_NoOriginalLotNoAmtInfo, ZDecimal.Zero);
					RefreshBindingAndClearValueWhenReadOnly(JI_AlcoholCountryRegionInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(PartyIdentifierInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(CertificateNoInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_PHValueInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(AuthorizedPersonInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_BarCodeInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(JI_SterilizationValueInfo, ZString.Empty);
					RefreshBindingAndClearValueWhenReadOnly(PreviousPermitNoInfo, ZString.Empty);

					var shippingIdentificationDataCollectionReadOnly = ShippingIdentificationDataCollectionReadOnly;
					ShippingIdentificationDataCollection.SetReadOnlyIncludingChildren(shippingIdentificationDataCollectionReadOnly);
					if (shippingIdentificationDataCollectionReadOnly)
					{
						ShippingIdentificationDataCollection.RemoveAndDeleteAll();
					}
					else
					{
						foreach (ShippingIdentificationData shippingIdentificationData in ShippingIdentificationDataCollection)
						{
							RefreshBindingAndClearValueWhenReadOnly(shippingIdentificationData.TW_ExpirationDateInfo, ZDateTime.Empty);
						}
					}

					var typeApprovalCertificateNumbersReadOnly = TypeApprovalCertificateNumbersReadOnly;
					TypeApprovalCertificateNumberCusSupportingCollection.SetReadOnlyIncludingChildren(typeApprovalCertificateNumbersReadOnly);
					if (typeApprovalCertificateNumbersReadOnly)
					{
						TypeApprovalCertificateNumberCusSupportingCollection.RemoveAndDeleteAll();
					}

					var foodDrugReadOnly = EnableIfLinkedNX601;
					StorageAndShippingConditionJobComInvLineRefsCollection.SetReadOnlyIncludingChildren(foodDrugReadOnly);
					if (foodDrugReadOnly)
					{
						StorageAndShippingConditionJobComInvLineRefsCollection.RemoveAndDeleteAll();
					}

					var packingDateSlaughterDateReadOnly = PackingDateSlaughterDateCollectionReadOnly;
					PackingDateCollection.SetReadOnlyIncludingChildren(packingDateSlaughterDateReadOnly);
					if (packingDateSlaughterDateReadOnly)
					{
						PackingDateCollection.RemoveAndDeleteAll();
					}

					SlaughterDateCollection.SetReadOnlyIncludingChildren(packingDateSlaughterDateReadOnly);
					if (packingDateSlaughterDateReadOnly)
					{
						SlaughterDateCollection.RemoveAndDeleteAll();
					}

					var packingHouseCollectionReadOnly = PackingHouseCollectionReadOnly;
					PackingHouseCollection.SetReadOnlyIncludingChildren(packingHouseCollectionReadOnly);
					if (packingHouseCollectionReadOnly)
					{
						PackingHouseCollection.RemoveAndDeleteAll();
					}
				}

				SetFoodDrugsReadOnly();

				if (!IsValidationSuspended)
				{
					var validation = Validation;
					validation.ValidateJI_BottledDate();
					validation.ValidateJI_ExpirationDate();
					validation.ValidateJI_AlcoholEndOfShelfLife();
					validation.ValidateJI_GoodsType();
					validation.ValidateJI_PHValue();
					validation.ValidateJI_BarCode();
					validation.ValidateJI_SterilizationValue();
					validation.ValidateJI_CustomsThirdQuantity();
					validation.ValidateJI_CustomsThirdUnitQty();
					validation.ValidateJI_QuarantineFeatures();
					validation.ValidateJI_QuarantineTreatment();
					validation.ValidateJI_VaccinationTypeDate();
					validation.ValidateJI_MicrochipID();
					validation.ValidateJI_AnimalAgeYear();
					validation.ValidateJI_AnimalAgeMonth();
					validation.ValidateJI_AnimalMaleQty();
					validation.ValidateJI_AnimalFemaleQty();
					validation.ValidateJI_AlcoholAge();
					validation.ValidateJI_AlcoholYear();
					validation.ValidateJI_AlteredLotNoAmt();
					validation.ValidateJI_RemovedLotNoAmt();
					validation.ValidateJI_NoOriginalLotNoAmt();
					validation.ValidateJI_AlcoholCountryRegion();
					validation.ValidatePartyIdentifier();
					validation.ValidateAuthorizedPerson();
					validation.ValidateCertificateNo();
					validation.ValidateJI_TariffExtensionCode();
					validation.ValidateJI_ProductThickness();
					validation.ValidateJI_ProductGrade();
					validation.ValidateJI_InnerPackType();
					validation.ValidateJI_InnerPackingMaterial();
					validation.ValidateJI_InnerPackDescription();
					TypeApprovalCertificateNumberCusSupportingCollection.MarkAsNeedingValidationIncludingChildren();
					StorageAndShippingConditionJobComInvLineRefsCollection.MarkAsNeedingValidationIncludingChildren();
				}

				HasLinkedCMHeaderInfo.RefreshBinding();
				CertificateOfOriginSupportedInfo.RefreshBinding();
				AlcoholSupportedInfo.RefreshBinding();
				TypeApprovalSupportedInfo.RefreshBinding();
				AnimalAndPlantSupportedInfo.RefreshBinding();
				FoodAndDrugSupportedInfo.RefreshBinding();
			}
		}

		void SetFoodDrugsReadOnly()
		{
			var isReadOnly = EnableIfLinkedNX601;
			var currentReadOnly = FoodDataCollection.ReadOnly;
			if (isReadOnly != currentReadOnly)
			{
				FoodDataCollection.SetReadOnlyIncludingChildren(isReadOnly);
			}
			if (isReadOnly && FoodDataCollection.Any())
			{
				FoodDataCollection.RemoveAndDeleteAll();
			}
		}

		void RefreshBindingAndClearValueWhenReadOnly(ZPropertyInfo propertyInfo, IZType value, Action clearDataAction = null, Func<bool> otherConditionAction = null)
		{
			propertyInfo.RefreshBinding();
			if ((propertyInfo.ReadOnly && !propertyInfo.Value.IsEmpty) || (otherConditionAction?.Invoke() ?? false))
			{
				propertyInfo.Value = value;
				clearDataAction?.Invoke();
			}
		}

		InvoiceLineLinkControllingMsgHeaderCollection invoiceLineLinkControllingMsgHeaders;

		public InvoiceLineLinkControllingMsgHeader[] GetRelatedControllingMsgHeaders() => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Where(link => link.IsLinkedCMHeader).ToArray();

		public void ClearDataByControllingAgency(ZGuid controllingMsgHeaderPK)
		{
			if (GetRelatedControllingMsgHeaders().Any(link => link.PK == controllingMsgHeaderPK))
			{
				RefreshBindingAndClearValuesWhenReadOnly();
			}
		}

		public ZGuid[] GetLinkControllingAgencyPKs() => InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Where(x => x.IsLinkedCMHeader).Select(x => x.ControllingMessageHeaderPK).ToArray();

		public bool IsLinkSameControllingAgencyAndSameData(Func<JobComInvoiceLine, JobComInvoiceLine, bool> isSameData)
		{
			var result = false;
			var linePK = PK;
			var jobDeclaration = Declaration;
			if (jobDeclaration != null)
			{
				var invoiceLines = jobDeclaration.InvoiceLines;
				var linkPKs = GetLinkControllingAgencyPKs();
				result = invoiceLines.Cast<JobComInvoiceLine>().Any(x => x.PK != linePK && x.GetLinkControllingAgencyPKs().Intersect(linkPKs).Any() && isSameData(x, this));
			}
			return result;
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var fNoteTypes = base.NoteTypesCore;
				fNoteTypes.Add(PredefinedNoteTypes.Instance.DeclarationGoodsDescription);
				return fNoteTypes;
			}
		}

		[ChildEditable(true)]
		public JobComInvoiceLineReservedFieldCollection ReservedFields
		{
			get
			{
				if (jobComInvoiceLineReservedFieldCollection == null)
				{
					jobComInvoiceLineReservedFieldCollection = new JobComInvoiceLineReservedFieldCollection(this);
					jobComInvoiceLineReservedFieldCollection.Load();
					RegisterEditableChildObject(jobComInvoiceLineReservedFieldCollection);
				}
				return jobComInvoiceLineReservedFieldCollection;
			}
		}
		JobComInvoiceLineReservedFieldCollection jobComInvoiceLineReservedFieldCollection;

		#region JI_DeclarationGoodsDescription

		[ChildEditable(true)]
		StmNote JI_DeclarationGoodsDescriptionNote
		{
			get
			{
				if (fJI_DeclarationGoodsDescriptionNote == null || fJI_DeclarationGoodsDescriptionNote.IsDeleted)
				{
					fJI_DeclarationGoodsDescriptionNote = Notes.FindByDescription(PredefinedNoteTypes.Instance.DeclarationGoodsDescription.Description).FirstOrDefault(x => !x.IsDeleted);
				}
				return fJI_DeclarationGoodsDescriptionNote;
			}
		}

		StmNote fJI_DeclarationGoodsDescriptionNote;

		[UniversalCopyExtraProperty]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|DeclarationGoodsDescription", Caption = "Declaration Goods Description")]
		public ZString DeclarationGoodsDescription
		{
			get { return OverrideDeclarationGoodsDescription ? JI_DeclarationGoodsDescription : ZString.Empty; }
			set
			{
				var newValue = value;
				if (!newValue.IsEmpty)
				{
					JI_DeclarationGoodsDescription = newValue;
				}
			}
		}

		[ReadOnlyMember(nameof(JI_DeclarationGoodsDescriptionReadOnly))]
		[MaxLength(20000)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_DeclarationGoodsDescription", Caption = "Declaration Goods Description", FullDescription = "The Chinese description and English description entered will populate to this field automatically. The content that exceeds the length limit (512 characters) imposed by customs is only for document printing.")]
		public ZString JI_DeclarationGoodsDescription
		{
			get
			{
				var declarationGoodsDescription = new ZStringBuilder();
				var note = JI_DeclarationGoodsDescriptionNote;
				if (note == null)
				{
					if (IsImport && AddInfoChild.ReportAircraftPartsIsNeeded)
					{
						var jobTWComInvoiceLine = AddInfoChild;
						declarationGoodsDescription.AppendFormat("{0}{1}[{2}|{3}|{4}||||]", JI_Description, JI_NDescription, jobTWComInvoiceLine.TWL_AircraftPartsCategory, jobTWComInvoiceLine.TWL_AircraftPartsCode, jobTWComInvoiceLine.TWL_AircraftIPC);
					}
					else
					{
						declarationGoodsDescription.AppendIfNotEmpty(JI_NDescription);
						declarationGoodsDescription.AppendIfNotEmpty(JI_Description);

						if (IsImport && IsCarRelatedTariff)
						{
							declarationGoodsDescription.AppendIfNotEmpty(GetInvoiceLineDescriptionFromCarInfo());
						}

						if (!JI_TextileWidth.IsEmpty && !JI_TextileWidthUQ.IsEmpty)
						{
							declarationGoodsDescription.AppendFormat("WIDTH: {0} {1}", JI_TextileWidth.ToString("0.######"), DimensionSymbolMap.TryGetValue(JI_TextileWidthUQ, out var unitSymbol) ? new ZString(unitSymbol) : JI_TextileWidthUQ.ToLower());
						}
					}
				}
				else
				{
					declarationGoodsDescription.Append(note.ST_NoteText);
				}
				return declarationGoodsDescription.ToStringWithNewLineBetweenAppends();
			}
			set
			{
				var oldValue = JI_DeclarationGoodsDescription;
				CheckMaximumLength(JI_DeclarationGoodsDescriptionInfo, value);
				if (oldValue != value)
				{
					var note = JI_DeclarationGoodsDescriptionNote;
					if (note == null || note.IsDeleting)
					{
						note = Notes.AddNew(true, PredefinedNoteTypes.Instance.DeclarationGoodsDescription.Description, string.Empty);
					}
					note.IgnoreValidationSuspended = false;
					note.ST_NoteText = value;
					HasChanges = true;
					JI_DeclarationGoodsDescriptionInfo.RefreshBinding();
					Validation.ValidateJI_DeclarationGoodsDescription();
				}
			}
		}

		HiddenTextNote JI_PermitGoodsDescriptionNote => fJI_PermitGoodsDescriptionNote ?? (fJI_PermitGoodsDescriptionNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.PermitGoodsDescription.Description));
		HiddenTextNote fJI_PermitGoodsDescriptionNote;

		public ZString JI_PermitGoodsDescription
		{
			get => JI_PermitGoodsDescriptionNote.Text;
			set => JI_PermitGoodsDescriptionNote.SetNoteText(this, JI_PermitGoodsDescriptionNoteInfo, value);
		}

		public ZPropertyInfo JI_PermitGoodsDescriptionNoteInfo => GetZPropertyInfo(nameof(JI_PermitGoodsDescription));

		static Dictionary<string, string> DimensionSymbolMap => dimensionSymbolMap ?? (dimensionSymbolMap = new Dictionary<string, string>()
				{
					{ "IN", "\"" }, { "FT",  "'" }, { "MM",  (NoResString)"mm" }, { "CM",  (NoResString)"cm" }, { "M",  (NoResString)"m" }, { "YD",  (NoResString)"yd" }
				});

		[ThreadStatic]
		static Dictionary<string, string> dimensionSymbolMap;

		ZBool JI_DeclarationGoodsDescriptionReadOnly => !OverrideDeclarationGoodsDescription;

		public ZPropertyInfo JI_DeclarationGoodsDescriptionInfo => GetZPropertyInfo(Schema.JI_DeclarationGoodsDescription);

		public ZBool OverrideDeclarationGoodsDescription
		{
			get
			{
				return JI_DeclarationGoodsDescriptionNote != null;
			}
			set
			{
				var note = JI_DeclarationGoodsDescriptionNote;
				if (!value && note != null)
				{
					note.Delete();
					HasChanges = true;
					JI_DeclarationGoodsDescriptionInfo.RefreshBinding();
				}
				else if (value && note == null)
				{
					Notes.AddNew(true, PredefinedNoteTypes.Instance.DeclarationGoodsDescription.Description, JI_DeclarationGoodsDescription);
					HasChanges = true;
					JI_DeclarationGoodsDescriptionInfo.RefreshBinding();
				}
			}
		}
		#endregion

		#region QuotaPermitNumberCusSupporting

		public QuotaPermitNumberCusSupporting QuotaPermitNumberCusSupporting
		{
			get
			{
				if (quotaPermitNumberCusSupporting?.IsDeleted ?? true)
				{
					quotaPermitNumberCusSupporting = QuotaPermitNumberCusSupportingCollection.FirstOrDefault() ?? QuotaPermitNumberCusSupportingCollection.AddNew();
				}
				return quotaPermitNumberCusSupporting;
			}
		}
		QuotaPermitNumberCusSupporting quotaPermitNumberCusSupporting;

		[ChildEditable(true)]
		public QuotaPermitNumberCusSupportingCollection QuotaPermitNumberCusSupportingCollection
		{
			get { return fQuotaPermitNumberCusSupportingCollection ?? (fQuotaPermitNumberCusSupportingCollection = GetQuotaPermitNumberCusSupportingCollection()); }
		}
		QuotaPermitNumberCusSupportingCollection fQuotaPermitNumberCusSupportingCollection;

		QuotaPermitNumberCusSupportingCollection GetQuotaPermitNumberCusSupportingCollection()
		{
			var result = new QuotaPermitNumberCusSupportingCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[UniversalCopyExtraProperty]
		[MaxLength(14)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|QuotaPermitNumber", Caption = "Tariff Rate Quota Certificate", MediumCaption = "Tariff Rate Quota Cert", ShortCaption = "Quota Cert", FullDescription = "Certificate number of Tariff Rate Quota")]
		public ZString QuotaPermitNumber
		{
			get
			{
				return QuotaPermitNumberCusSupporting?.CSI_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(QuotaPermitNumberInfo, value);
				QuotaPermitNumberCusSupporting.CSI_ReferenceNumber = value;
				QuotaPermitNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo QuotaPermitNumberInfo
		{
			get { return (quotaPermitNumberCusSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(QuotaPermitNumber)) : GetWrappedZPropertyInfo(nameof(QuotaPermitNumber), x => quotaPermitNumberCusSupporting.CSI_ReferenceNumberInfo); }
		}

		[UniversalCopyExtraProperty]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|QuotaPermitNumberItemNumber", ShortCaption = "Quota Permit Line No.", Caption = "Quota Permit Line Number")]
		public ZInt QuotaPermitNumberItemNumber
		{
			get
			{
				return QuotaPermitNumberCusSupporting?.CSI_LineNo ?? ZShort.Zero;
			}
			set
			{
				QuotaPermitNumberCusSupporting.CSI_LineNo = value;
				QuotaPermitNumberItemNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo QuotaPermitNumberItemNumberInfo
		{
			get { return (quotaPermitNumberCusSupporting?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(QuotaPermitNumberItemNumber)) : GetWrappedZPropertyInfo(nameof(QuotaPermitNumberItemNumber), x => quotaPermitNumberCusSupporting.CSI_LineNoInfo); }
		}
		#endregion

		IEnumerable<ReservedField> IReservedFieldSupporter.GetReservedFields()
		{
			return ReservedFields.OrderList;
		}

		protected override ZBool IsSupportEmptyPackType(BasePackage package) => true;

		ZBool JI_QuarantineFeaturesReadOnly => !IsLinkedNX401CMHeaderWithAllBusinessType;

		ZBool JI_QuarantineTreatmentReadOnly => !IsLinkedNX401CMHeaderWithAllBusinessType;

		ZBool JI_VaccinationTypeDateReadOnly => !IsLinkedNX401CMHeaderWithBusinessType3040;

		ZBool JI_MicrochipIDReadOnly => !IsLinkedNX401CMHeaderWithBusinessType3040;

		ZBool JI_AnimalAgeYearReadOnly => !IsLinkedNX401CMHeaderWithBusinessType3040;

		ZBool JI_AnimalAgeMonthReadOnly => !IsLinkedNX401CMHeaderWithBusinessType3040;

		ZBool JI_AnimalMaleQtyReadOnly => !IsLinkedNX401CMHeaderWithBusinessType3040;

		ZBool JI_AnimalFemaleQtyReadOnly => !IsLinkedNX401CMHeaderWithBusinessType3040;

		public InvoiceQuantityAndUnitQtyResultCollection InvoiceQuantityAndUnitQtyResultCollection
		{
			get
			{
				if (fInvoiceQuantityAndUnitQtyResultCollection == null)
				{
					fInvoiceQuantityAndUnitQtyResultCollection = new InvoiceQuantityAndUnitQtyResultCollection(this);
					fInvoiceQuantityAndUnitQtyResultCollection.ShouldRebuildElements();
				}
				return fInvoiceQuantityAndUnitQtyResultCollection;
			}
		}
		InvoiceQuantityAndUnitQtyResultCollection fInvoiceQuantityAndUnitQtyResultCollection;

		internal ZBool IsInvoiceQuantityAndUnitQtyResultCollectionLoaded => fInvoiceQuantityAndUnitQtyResultCollection != null;

		protected override TariffFormatter TariffFormatter => new TaiwanTariffFormatter();

		#region Trademark Image
		[UniversalCopyExtraProperty]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|TrademarkStorageDocsGuid", Caption = "Trademark Image", FullDescription = "Select the Trademark Image with the file type: [TDM Trademark Image] from eDocs. It's printed on the export/import entry.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TrademarkEDocList))]
		public ZGuid TrademarkStorageDocsGuid
		{
			get
			{
				if (IsInDatabase && (trademarkPivot == null || trademarkPivot.IsDeleted))
				{
					trademarkPivot = LoadTrademarkPivot();
				}
				return trademarkPivot?.XX_Relation2ID ?? ZGuid.Empty;
			}
			set
			{
				var oldValue = TrademarkStorageDocsGuid;
				if (value.IsEmpty)
				{
					if (trademarkPivot != null)
					{
						trademarkPivot.Delete();
						trademarkPivot = null;
					}
				}
				else
				{
					if (trademarkPivot == null || trademarkPivot.IsDeleted)
					{
						trademarkPivot = MakeOrFindTrademarkPivot();
					}
					trademarkPivot.XX_Relation2ID = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateTrademarkStorageDocsGuid();
					}
				}
				TrademarkStorageDocsGuidInfo.RefreshBinding();
				if (!IsCopying && oldValue != TrademarkStorageDocsGuid)
				{
					HasChanges = true;
					SetBrandNameIfTrademarkStorageDocsGuidSelected();
				}
			}
		}

		public ZPropertyInfo TrademarkStorageDocsGuidInfo => GetZPropertyInfo(nameof(TrademarkStorageDocsGuid));

		GenPivot MakeOrFindTrademarkPivot()
		{
			var pivot = LoadTrademarkPivot();
			if (pivot == null)
			{
				pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypeDecider.Types.InvoiceLineRelatedTrademarkImagePivot;
				pivot.XX_Relation1ID = PK;
				pivot.XX_Relation1TableCode = JobComInvoiceLineSchema.Constants.Prefix;
				pivot.XX_Relation2TableCode = StorageDocsSchema.Constants.Prefix;
			}
			return pivot;
		}

		GenPivot LoadTrademarkPivot()
		{
			var query = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.InvoiceLineRelatedTrademarkImagePivot);
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceLineSchema.Constants.Prefix);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, StorageDocsSchema.Constants.Prefix);
			var pivot = Factory.LoadTop1<GenPivot>(query);
			return pivot;
		}
		GenPivot trademarkPivot;

		public IeDoc TrademarkStorageDoc
		{
			get
			{
				var edocPK = TrademarkStorageDocsGuid;
				return edocPK.IsValid ? TrademarkStorageDocs.GetFromUniqueKey(edocPK.ToGuid()) : null;
			}
		}

		internal IStorageDocsBaseCollection[] TrademarkStorageDocs
		{
			get
			{
				var declaration = Declaration;
				var docManagerInfo = declaration?.TrademarkDocManagerInfo;
				var shipmentDocManagerInfo = declaration?.Shipment?.DocManagerInfo;
				var declarationDocManagerInfo = declaration?.DocManagerInfo;
				var productDocManagerInfo = Part?.DocManagerInfo();
				SetDocManagerInfoMasterFactory(productDocManagerInfo, docManagerInfo);
				SetDocManagerInfoMasterFactory(shipmentDocManagerInfo, docManagerInfo);
				SetDocManagerInfoMasterFactory(declarationDocManagerInfo, docManagerInfo);
				var invoiceHeader = InvoiceHeader;
				DocManagerInfo headerDocManagerInfo = null;
				if (invoiceHeader != null && !invoiceHeader.JustAddedByDataObjectReader)
				{
					headerDocManagerInfo = invoiceHeader.DocManagerInfo;
					if (invoiceHeader.IsAttachedToPersistentDeclaration)
					{
						SetDocManagerInfoMasterFactory(headerDocManagerInfo, docManagerInfo);
					}
				}

				return new IStorageDocsBaseCollection[] { productDocManagerInfo?.AllEDocs, declarationDocManagerInfo?.AllEDocs, shipmentDocManagerInfo?.AllEDocs, headerDocManagerInfo?.AllEDocs };
			}
		}

		void SetDocManagerInfoMasterFactory(DocManagerInfo docManagerInfo, DocManagerInfo mainDocManagerInfo)
		{
			if (docManagerInfo != null && mainDocManagerInfo != null && mainDocManagerInfo != docManagerInfo)
			{
				docManagerInfo.MasterFactory = mainDocManagerInfo.MasterFactory;
			}
		}

		public Image TrademarkImage
		{
			get
			{
				var blob = TrademarkStorageDoc?.ImageData ?? ZBlob.Empty;
				Image result = null;
				if (!blob.IsEmpty)
				{
					try
					{
						result = Image.FromStream(new System.IO.MemoryStream(blob));
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				}
				return result;
			}
		}
		#endregion

		protected override Customs.Business.BaseCustomsQuantityConverter GetCustomsQuantity2Converter() => new CustomsQuantity2Converter(this);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|FormattedAdValoremDutyRate", Caption = "Ad-Valorem Duty Rate", ShortCaption = "Ad-Valorem Duty", FullDescription = "The Ad-Valorem Duty rate for imported goods.")]
		public ZString FormattedAdValoremDutyRate => Factory.GetValue(ref formattedAdValoremDutyRate, delegate
					{
						var rateFormulaDerivedFrom = ApplicableRates.Where(x => x.RateCode == UniversalReferenceConstants.RefCusRateCodes.DTA).Select(x => x.ZZ2_RateFormulaDerivedFrom).FirstOrDefault();
						return CommonHelper.GetFormattedStringForRateFormulaDerivedFrom(rateFormulaDerivedFrom);
					});

		CachedProperty<ZString> formattedAdValoremDutyRate;

		public ZPropertyInfo FormattedAdValoremDutyRateInfo => GetZPropertyInfo(nameof(FormattedAdValoremDutyRate));

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|FormattedSpecificDutyRate", Caption = "Specific Duty Rate", ShortCaption = "Specific Duty", FullDescription = "The rate and unit of Ad-Valorem Duty for import goods per unit.")]
		public ZString FormattedSpecificDutyRate => Factory.GetValue(ref formattedSpecificDutyRate, delegate
					{
						var rateFormulaDerivedFrom = ApplicableRates.Where(x => x.RateCode == UniversalReferenceConstants.RefCusRateCodes.DTS).Select(x => x.ZZ2_RateFormulaDerivedFrom).FirstOrDefault();
						return CommonHelper.GetFormattedStringForRateFormulaDerivedFrom(rateFormulaDerivedFrom);
					});

		CachedProperty<ZString> formattedSpecificDutyRate;

		public ZPropertyInfo FormattedSpecificDutyRateInfo => GetZPropertyInfo(nameof(FormattedSpecificDutyRate));

		ZString TariffDutyRate => Factory.GetValue(ref tariffDutyRate, delegate
					{
						var result = ZString.Empty;
						var applicableRates = ApplicableRates;
						if (applicableRates.Any())
						{
							var rateWithHighestAmount = Declaration.DutyCalculatorStrategy.GetRateViewOfHighestDutyCalculationResult(CalcDataForConditionFormula, applicableRates);
							result = CommonHelper.GetFormattedStringForRateFormulaDerivedFrom(rateWithHighestAmount.ZZ2_RateFormulaDerivedFrom);
						}
						return result;
					});

		CachedProperty<ZString> tariffDutyRate;

		IEnumerable<RateView> ApplicableRates => UniversalTariff?.GetApplicableRates(DutyRateSelectionCriteria) ?? Enumerable.Empty<RateView>();

		internal bool IsTariffQuota => JI_Tariff.StartsWith(Constants.ProcedureCodes._98) || JI_ConcessionOrder == Constants.ConcessionOrder.Quota;

		void SetDefaultProcedure()
		{
			if (JI_Procedure.IsEmpty && !JI_PrimaryPreference.IsEmpty && !JI_CountryOfOrigin.IsEmpty && !JI_Tariff.IsEmpty)
			{
				var formattedDutyRate = TariffDutyRate;
				if (!formattedDutyRate.IsEmpty && formattedDutyRate != CommonHelper.GetFormattedStringForRateFormulaDerivedFrom("0"))
				{
					JI_Procedure = Constants.ProcedureCodes._31;
				}
				else if (!JI_TariffAdditionalCode.IsEmpty)
				{
					JI_Procedure = Constants.ProcedureCodes._51;
				}
				else
				{
					JI_Procedure = Constants.ProcedureCodes._50;
				}
			}
		}

		void ClearCustomsQuantityIfRequired()
		{
			if (TariffUnitOfQuantity.IsEmpty || !Lookups.WeightUQList.ContainsCode(JI_NetWeightUQ) || JI_NetWeight <= 0)
			{
				JI_CustomsQuantity = ZDecimal.Zero;
			}
		}

		void SetBrandNameIfTrademarkStorageDocsGuidSelected()
		{
			if (JI_BrandName.IsEmpty && !TrademarkStorageDocsGuid.IsEmpty && TrademarkStorageDocsGuid.IsValid)
			{
				JI_BrandName = Constants.Figure;
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_CVAfterRecon", Caption = "Customs Value")]
		public override ZDecimal JI_CVAfterRecon
		{
			get => base.JI_CVAfterRecon;
			set => base.JI_CVAfterRecon = value;
		}

		#region IReconcileCandidate
		ZDecimal IReconcileCandidate.ReconciledCustomsValue
		{
			get => JI_CVAfterRecon;
			set => JI_CVAfterRecon = value;
		}

		ZDecimal IReconcileCandidate.PreReconciledCustomsValue => JI_CVAfterRecon;

		ZInt IReconcileCandidate.SecondarySortingValue => JI_LineNo;
		#endregion

		public override ZString GetPackableItemGoodsDescription()
		{
			return JI_DeclarationGoodsDescription.Left(CusPackableItem.Schema.CUI_GoodsDescriptionMaxLength);
		}

		#region Taxes
		[BusinessObjectTestExclude]
		[UniversalCopyCollectionEntity(JobComInvoiceLineTaxSchema.Constants.TableName, JobComInvoiceLineTaxSchema.Constants.JLT_JI)]
		[ChildEditable(true)]
		public JobComInvoiceLineTaxCollection Taxes => taxes ?? (taxes = GetTaxes());
		JobComInvoiceLineTaxCollection taxes;

		JobComInvoiceLineTaxCollection GetTaxes()
		{
			var result = new JobComInvoiceLineTaxCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}
		#endregion

		public HashSet<ZString> F5FTZDestinationCountryCodes => Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "{0}_{1}_F5FTZDestinationCountryCodes", JI_Tariff, Constants.UniversalReferenceConstants.CusTariffAttributeName.F5FTZDestination), () => UniversalTariff?.GetAttributes(Constants.UniversalReferenceConstants.CusTariffAttributeName.F5FTZDestination).SelectMany(x => x.ZZ3_Value.Split(';').Select(v => v.Trim())).ToHashSet() ?? new HashSet<ZString>());

		#region Tariff Regulations Attributes
		IEnumerable<(ZString Code, ZString AdditionalDescription)> GetRegulationsAttributesByKey(ZString key)
		{
			var universalTariff = UniversalTariff;
			if (universalTariff != null)
			{
				return universalTariff.GetAttributes(key).Select(x => (Code: x.ZZ3_Value, AdditionalDescription: x.AdditionalDescription)).OrderBy(x => x.Code);
			}
			return Enumerable.Empty<(ZString Code, ZString AdditionalDescription)>();
		}

		ZString GetRegulationsDescriptionsByAttributeName(ZString attributeName)
		{
			var result = ZString.Empty;
			var universalTariff = UniversalTariff;
			if (universalTariff != null)
			{
				var stringBuilder = new ZStringBuilder();
				GetRegulationsAttributesByKey(attributeName).ForEach(x => stringBuilder.AppendFormat("{0}{1}{2}", x.Code, System.Environment.NewLine, x.AdditionalDescription));
				result = stringBuilder.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine + System.Environment.NewLine);
			}
			return result;
		}

		ZString ImportExportRegulationsAttributeName => IsImport ? Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations : Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations;

		public ZString ImportExportRegulations
		{
			get
			{
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_ImportExportRegulations", JI_Tariff, ImportExportRegulationsAttributeName);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					return GetRegulationsDescriptionsByAttributeName(ImportExportRegulationsAttributeName);
				});
			}
		}

		public ZString CustomsRegulations
		{
			get
			{
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_CustomsRegulations", JI_Tariff, Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					return GetRegulationsDescriptionsByAttributeName(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements);
				});
			}
		}

		public IEnumerable<ZString> GetImportExportRegulationCodes()
		{
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_GetImportExportRegulationCodes", JI_Tariff, ImportExportRegulationsAttributeName);
			return Factory.GetCachedValue(cacheKey, () =>
			{
				return GetRegulationsAttributesByKey(ImportExportRegulationsAttributeName).Select(x => x.Code).OrderBy(x => x);
			});
		}

		internal bool HasRegulationsCode581Or541
		{
			get
			{
				var importExportRegulationCodes = GetImportExportRegulationCodes();
				return importExportRegulationCodes.Contains(ImportExportRegulationCodes.RegulationsCode581) || importExportRegulationCodes.Contains(ImportExportRegulationCodes.RegulationsCode541);
			}
		}

		internal bool HasRegulationsCode602 => GetImportExportRegulationCodes().Contains(ImportExportRegulationCodes.RegulationsCode602);

		internal bool HasRegulationsCodeF01OrF02
		{
			get
			{
				var importExportRegulationCodes = GetImportExportRegulationCodes();
				return importExportRegulationCodes.Contains(ImportExportRegulationCodes.RegulationsCodeF01) || importExportRegulationCodes.Contains(ImportExportRegulationCodes.RegulationsCodeF02);
			}
		}
		#endregion

		protected override void CalculateCustomsFactorAndQtyCore()
		{
			if (CustomsQuantity2Converter != null)
			{
				CustomsQuantity2Converter.CalculateCustomsFactorAndQty();
			}
			DocumentaryQuantityConverter.CalculateCustomsFactorAndQty();
		}

		#region Interactions between Unit Price, Quantity and Line Price

		public string DocumentaryQuantityCalculationFieldSettingType => nameof(AddInfoChild.TWL_DocumentaryQty);
		public string DocumentaryUnitCalculationFieldSettingType => nameof(AddInfoChild.TWL_DocumentaryUQ);
		public string DocumentaryUnitPriceCalculationFieldSettingType => nameof(AddInfoChild.TWL_DocumentaryUnitPrice);

		public bool IsSettingDocumentaryQuantity => IsFieldSettingInProgress(DocumentaryQuantityCalculationFieldSettingType);
		public bool IsSettingDocumentaryUnit => IsFieldSettingInProgress(DocumentaryUnitCalculationFieldSettingType);
		public bool IsSettingDocumentaryUnitPrice => IsFieldSettingInProgress(DocumentaryUnitPriceCalculationFieldSettingType);

		public bool IsChangedFromQuantity => IsSettingInvoiceQuantity && !IsSettingUnitPrice && !IsSettingLinePrice && !IsSettingDocumentaryQuantity && !IsSettingDocumentaryUnit && !IsSettingDocumentaryUnitPrice;
		public bool IsChangedFromUnitPrice => IsSettingUnitPrice && !IsSettingInvoiceQuantity && !IsSettingLinePrice && !IsSettingDocumentaryQuantity && !IsSettingDocumentaryUnit && !IsSettingDocumentaryUnitPrice;
		public bool IsChangedFromLinePrice => IsSettingLinePrice && !IsSettingUnitPrice && !IsSettingInvoiceQuantity && !IsSettingDocumentaryQuantity && !IsSettingDocumentaryUnit && !IsSettingDocumentaryUnitPrice;
		public bool IsChangedFromDocumentaryQuantity => IsSettingDocumentaryQuantity && !IsSettingLinePrice && !IsSettingUnitPrice && !IsSettingInvoiceQuantity && !IsSettingDocumentaryUnit && !IsSettingDocumentaryUnitPrice;
		public bool IsChangedFromDocumentaryUnit => IsSettingDocumentaryUnit && !IsSettingDocumentaryUnitPrice && !IsSettingDocumentaryQuantity && !IsSettingLinePrice && !IsSettingUnitPrice && !IsSettingInvoiceQuantity;

		bool CanCalculateUnitPrice => !JI_InvoiceQuantity.IsEmpty && !JI_LinePrice.IsEmpty && JI_EnteredUnitPrice.IsEmpty;
		bool CanCalculateLinePrice => !JI_InvoiceQuantity.IsEmpty && !JI_EnteredUnitPrice.IsEmpty;

		public override void SetDefaultValuesForNewPackableItem(Customs.Business.CusPackableItem newItem)
		{
			base.SetDefaultValuesForNewPackableItem(newItem);
			newItem.CUI_PackableQty = AddInfoChild.TWL_DocumentaryQty;
			newItem.CUI_PackableUQ = AddInfoChild.TWL_DocumentaryUQ.Left(CusPackableItem.Schema.CUI_PackableUQMaxLength);
			newItem.Grouping = JI_Group;
		}

		protected override void UpdateUnitPriceIfChangedFromLinePriceChanges()
		{
			if (IsChangedFromLinePrice)
			{
				CalculateUnitPrice();
				AddInfoChild.CalculateDocumentaryUnitPriceUsingUnitPriceAndConversionFactor();
			}
		}
		protected override void CalculateLinePriceFromInvoiceQuantityChanges()
		{
			if (CanCalculateUnitPrice)
			{
				UpdateUnitPriceFromInvoiceQuantityChanges();
			}
			else if (IsChangedFromQuantity)
			{
				CalculateLinePrice();
			}
		}
		protected override void UpdateLinePriceIfChangedFromUnitPriceChanges()
		{
			if (IsChangedFromUnitPrice)
			{
				CalculateLinePrice();
			}
		}
		void UpdateUnitPriceFromInvoiceQuantityChanges()
		{
			if (IsChangedFromQuantity)
			{
				CalculateUnitPrice();
			}
		}
		void UpdateDocumentaryUnitPriceIfChangedFromUnitPriceChanges()
		{
			if (IsChangedFromUnitPrice)
			{
				AddInfoChild.CalculateDocumentaryUnitPriceUsingUnitPriceAndConversionFactor();
			}
		}
		new void CalculateUnitPrice()
		{
			if (CanCalculateUnitPrice)
			{
				JI_EnteredUnitPrice = JI_InvoiceQuantity.IsEmpty ? JI_EnteredUnitPrice : (ZDecimal)Utilities.Round(JI_LinePrice / JI_InvoiceQuantity, 6);
			}
		}
		void CalculateLinePrice()
		{
			if (CanCalculateLinePrice)
			{
				JI_LinePrice = Utilities.Round(ComponentPrice, 2);
			}
		}

		protected override ZDecimal ComponentPrice => JI_InvoiceQuantity * JI_EnteredUnitPrice;

		#endregion

		#region Taxes

		ZString GetTariffCode(ZString type, ZString paymentMethod)
		{
			return GetInvoiceLineTax(type, paymentMethod)?.JLT_Tariff ?? ZString.Empty;
		}

		ZPropertyInfo GetTariffCodeInfo(ZString propertyName, ZString type, ZString paymentMethod)
		{
			var lineTax = GetInvoiceLineTax(type, paymentMethod);
			return lineTax == null ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => lineTax.JLT_TariffInfo);
		}

		JobComInvoiceLineTax GetInvoiceLineTax(ZString type, ZString paymentMethod)
		{
			return Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault(x => x.JLT_Type == type && x.JLT_MethodOfPayment == paymentMethod);
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|AlcoholTaxCashTariffCode", Caption = "Alcohol Tax (Cash)")]
		public ZString AlcoholTaxCashTariffCode => GetTariffCode(Constants.UniversalReferenceConstants.RefCusRateTypes.AT, DutyTaxPaymentMethodList.Codes.CashPayment);

		public ZPropertyInfo AlcoholTaxCashTariffCodeInfo => GetTariffCodeInfo(nameof(AlcoholTaxCashTariffCode), Constants.UniversalReferenceConstants.RefCusRateTypes.AT, DutyTaxPaymentMethodList.Codes.CashPayment);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|AlcoholTaxNonCashTariffCode", Caption = "Alcohol Tax (Non-Cash)")]
		public ZString AlcoholTaxNonCashTariffCode => GetTariffCode(Constants.UniversalReferenceConstants.RefCusRateTypes.AT, DutyTaxPaymentMethodList.Codes.NonCashPayment);

		public ZPropertyInfo AlcoholTaxNonCashTariffCodeInfo => GetTariffCodeInfo(nameof(AlcoholTaxNonCashTariffCode), Constants.UniversalReferenceConstants.RefCusRateTypes.AT, DutyTaxPaymentMethodList.Codes.NonCashPayment);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|CommodityTaxCashTariffCode", Caption = "Commodity Tax (Cash)")]
		public ZString CommodityTaxCashTariffCode => GetTariffCode(Constants.UniversalReferenceConstants.RefCusRateTypes.CT, DutyTaxPaymentMethodList.Codes.CashPayment);

		public ZPropertyInfo CommodityTaxCashTariffCodeInfo => GetTariffCodeInfo(nameof(CommodityTaxCashTariffCode), Constants.UniversalReferenceConstants.RefCusRateTypes.CT, DutyTaxPaymentMethodList.Codes.CashPayment);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|CommodityTaxNonCashTariffCode", Caption = "Commodity Tax (Non-Cash)")]
		public ZString CommodityTaxNonCashTariffCode => GetTariffCode(Constants.UniversalReferenceConstants.RefCusRateTypes.CT, DutyTaxPaymentMethodList.Codes.NonCashPayment);

		public ZPropertyInfo CommodityTaxNonCashTariffCodeInfo => GetTariffCodeInfo(nameof(CommodityTaxNonCashTariffCode), Constants.UniversalReferenceConstants.RefCusRateTypes.CT, DutyTaxPaymentMethodList.Codes.NonCashPayment);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|SpecialTaxCashTariffCode", Caption = "Special Tax (Cash)")]
		public ZString SpecialTaxCashTariffCode => GetTariffCode(Constants.UniversalReferenceConstants.RefCusRateTypes.SS, DutyTaxPaymentMethodList.Codes.CashPayment);

		public ZPropertyInfo SpecialTaxCashTariffCodeInfo => GetTariffCodeInfo(nameof(SpecialTaxCashTariffCode), Constants.UniversalReferenceConstants.RefCusRateTypes.SS, DutyTaxPaymentMethodList.Codes.CashPayment);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|SpecialTaxNonCashTariffCode", Caption = "Special Tax (Non-Cash)")]
		public ZString SpecialTaxNonCashTariffCode => GetTariffCode(Constants.UniversalReferenceConstants.RefCusRateTypes.SS, DutyTaxPaymentMethodList.Codes.NonCashPayment);

		public ZPropertyInfo SpecialTaxNonCashTariffCodeInfo => GetTariffCodeInfo(nameof(SpecialTaxNonCashTariffCode), Constants.UniversalReferenceConstants.RefCusRateTypes.SS, DutyTaxPaymentMethodList.Codes.NonCashPayment);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|TobaccoTaxCashTariffCode", Caption = "Tobacco Tax (Cash)")]
		public ZString TobaccoTaxCashTariffCode => GetTariffCode(Constants.UniversalReferenceConstants.RefCusRateTypes.TT, DutyTaxPaymentMethodList.Codes.CashPayment);

		public ZPropertyInfo TobaccoTaxCashTariffCodeInfo => GetTariffCodeInfo(nameof(TobaccoTaxCashTariffCode), Constants.UniversalReferenceConstants.RefCusRateTypes.TT, DutyTaxPaymentMethodList.Codes.CashPayment);

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|TobaccoTaxNonCashTariffCode", Caption = "Tobacco Tax (Non-Cash)")]
		public ZString TobaccoTaxNonCashTariffCode => GetTariffCode(Constants.UniversalReferenceConstants.RefCusRateTypes.TT, DutyTaxPaymentMethodList.Codes.NonCashPayment);

		public ZPropertyInfo TobaccoTaxNonCashTariffCodeInfo => GetTariffCodeInfo(nameof(TobaccoTaxNonCashTariffCode), Constants.UniversalReferenceConstants.RefCusRateTypes.TT, DutyTaxPaymentMethodList.Codes.NonCashPayment);
		#endregion

		#region IDocAddresses

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;

		[ChildEditable]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new TWJobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new[]
		{
			DocAddressType.Manufacturer
		};
		#endregion

		public TWJobDocAddress ManufacturerDocAddress
		{
			get
			{
				if (manufacturerDocAddress == null || manufacturerDocAddress.IsDeleted)
				{
					var manufacturerAddressRequirement = new ManufacturerAddressRequirement(DocAddressType.Manufacturer);
					manufacturerDocAddress = (TWJobDocAddress)DocAddresses.FindOrCreateWithRequirement(manufacturerAddressRequirement);
				}
				return manufacturerDocAddress;
			}
		}
		TWJobDocAddress manufacturerDocAddress;

		bool PackingHouseCollectionReadOnly => !IsLinkedNX401CMHeaderWithBusinessType60;

		#region PackingHouseCollection
		[ChildEditable(true)]
		public PackingHouseCollection PackingHouseCollection
		{
			get
			{
				if (packingHouseCollection == null)
				{
					packingHouseCollection = new PackingHouseCollection(this);
					packingHouseCollection.SetReadOnlyIncludingChildren(PackingHouseCollectionReadOnly);
					packingHouseCollection.Load();
					RegisterEditableChildObject(packingHouseCollection);
				}
				return packingHouseCollection;
			}
		}

		PackingHouseCollection packingHouseCollection;
		#endregion

		#region PackingDateCollection
		bool PackingDateSlaughterDateCollectionReadOnly => !IsLinkedNX401CMHeaderWithBusinessType40;

		[ChildEditable(true)]
		public PackingDateCollection PackingDateCollection
		{
			get
			{
				if (packingDateCollection == null)
				{
					packingDateCollection = new PackingDateCollection(this);
					packingDateCollection.SetReadOnlyIncludingChildren(PackingDateSlaughterDateCollectionReadOnly);
					packingDateCollection.Load();
					RegisterEditableChildObject(packingDateCollection);
				}
				return packingDateCollection;
			}
		}

		PackingDateCollection packingDateCollection;
		#endregion

		#region SlaughterDateCollection
		[ChildEditable(true)]
		public SlaughterDateCollection SlaughterDateCollection
		{
			get
			{
				if (slaughterDateCollection == null)
				{
					slaughterDateCollection = new SlaughterDateCollection(this);
					slaughterDateCollection.SetReadOnlyIncludingChildren(PackingDateSlaughterDateCollectionReadOnly);
					slaughterDateCollection.Load();
					RegisterEditableChildObject(slaughterDateCollection);
				}
				return slaughterDateCollection;
			}
		}

		SlaughterDateCollection slaughterDateCollection;
		#endregion

		[MaxLength(512)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|NX101ShippingMarks", Caption = "Shipping Marks", MediumCaption = "Marks", ShortCaption = "Marks", FullDescription = "Indicates the shipping marks of Package.")]
		public ZString NX101ShippingMarks
		{
			get
			{
				if (shippingMarksNotes == null)
				{
					shippingMarksNotes = new HiddenTextNote(this, PredefinedNoteTypes.Instance.NX101ShippingMarks.Description);
				}

				return shippingMarksNotes.Text;
			}
			set
			{
				var oldValue = NX101ShippingMarks;
				CheckMaximumLength(NX101ShippingMarksInfo, value);
				if (oldValue != value)
				{
					if (shippingMarksNotes == null)
					{
						shippingMarksNotes = new HiddenTextNote(this, PredefinedNoteTypes.Instance.NX101ShippingMarks.Description);
					}
					shippingMarksNotes.SetNoteText(this, NX101ShippingMarksInfo, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateNX101ShippingMarks();
					}
				}
			}
		}

		HiddenTextNote shippingMarksNotes;

		public ZPropertyInfo NX101ShippingMarksInfo => GetZPropertyInfo(Schema.NX101ShippingMarks);

		[ChildEditable(true)]
		StmNote NX101PermitGoodsDescriptionNote
		{
			get
			{
				if (fNX101PermitGoodsDescriptionNote == null || fNX101PermitGoodsDescriptionNote.IsDeleted)
				{
					fNX101PermitGoodsDescriptionNote = Notes.FindByDescription(PredefinedNoteTypes.Instance.NX101PermitGoodsDescription.Description).FirstOrDefault(x => !x.IsDeleted);
				}
				return fNX101PermitGoodsDescriptionNote;
			}
		}
		StmNote fNX101PermitGoodsDescriptionNote;

		[ReadOnlyMember(nameof(NX101PermitGoodsDescriptionReadOnly))]
		[MaxLength(512)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|NX101PermitGoodsDescription", Caption = "Permit Goods Description", MediumCaption = "Goods Description", ShortCaption = "Description", FullDescription = "Indicates the Goods Description for Certificate of Origin. Invoice Line Declaration Goods Description will trim to 512 characters and default to Permit Goods Description.")]
		public ZString NX101PermitGoodsDescription
		{
			get
			{
				return OverrideNX101PermitGoodsDescription ? NX101PermitGoodsDescriptionNote?.ST_NoteText ?? ZString.Empty : JI_DeclarationGoodsDescription;
			}
			set
			{
				var oldValue = NX101PermitGoodsDescription;
				CheckMaximumLength(NX101PermitGoodsDescriptionInfo, value);
				if (oldValue != value)
				{
					var note = NX101PermitGoodsDescriptionNote;
					if (note == null || note.IsDeleting)
					{
						note = Notes.AddNew(false, PredefinedNoteTypes.Instance.NX101PermitGoodsDescription.Description, string.Empty);
					}
					note.IgnoreValidationSuspended = false;
					note.ST_NoteText = value;
					HasChanges = true;
					NX101PermitGoodsDescriptionInfo.RefreshBinding();
					Validation.ValidateNX101PermitGoodsDescription();
				}
			}
		}

		public ZPropertyInfo NX101PermitGoodsDescriptionInfo => GetZPropertyInfo(Schema.NX101PermitGoodsDescription);

		ZBool NX101PermitGoodsDescriptionReadOnly => !OverrideNX101PermitGoodsDescription;

		public ZBool OverrideNX101PermitGoodsDescription
		{
			get
			{
				return NX101PermitGoodsDescriptionNote != null;
			}
			set
			{
				var note = NX101PermitGoodsDescriptionNote;
				if (!value && note != null)
				{
					note.Delete();
					HasChanges = true;
					NX101PermitGoodsDescriptionInfo.RefreshBinding();
				}
				else if (value && note == null)
				{
					note = Notes.AddNew(false, PredefinedNoteTypes.Instance.NX101PermitGoodsDescription.Description, NX101PermitGoodsDescription);
					note.ST_NoteText = JI_DeclarationGoodsDescription;
					HasChanges = true;
					NX101PermitGoodsDescriptionInfo.RefreshBinding();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CurrencyList))]
		public ZString JI_PermitUnitPriceCurrency => InvoiceHeader?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;

		public ZPropertyInfo JI_PermitUnitPriceCurrencyInfo => GetZPropertyInfo(nameof(JI_PermitUnitPriceCurrency));

		public override ZShort JI_LineNo
		{
			get => base.JI_LineNo;
			set
			{
				var oldValue = base.JI_LineNo;
				base.JI_LineNo = value;
				if (!IsCopying && value != oldValue && !IsValidationSuspended)
				{
					Validation.ValidateJI_Procedure();
				}
			}
		}

		[DecimalPlaces(0)]
		[DecimalPrecision(8)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|JI_PackagingQTY", Caption = "Number of Package", ShortCaption = "Package", FullDescription = "Indicates the number of inner packages of goods.")]
		public override ZDecimal JI_PackagingQTY { get => base.JI_PackagingQTY; set => base.JI_PackagingQTY = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PackagingUQList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.AddInfoJobComInvoiceLine|JI_PackagingUQ", Caption = "Packaging UQ")]
		public override ZString JI_PackagingUQ { get => base.JI_PackagingUQ; set => base.JI_PackagingUQ = value; }

		#region ReservedFields

		[MaxLength(1)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|ReservedFieldCode1", Caption = "Reserved Field 1 Code", MediumCaption = "Reserved Code 1", ShortCaption = "RS Code 1")]
		public ZString ReservedFieldCode1
		{
			get => GetReservedField(0)?.CY_Code ?? ZString.Empty;
			set => SetReservedFieldCode(0, value);
		}

		public ZPropertyInfo ReservedFieldCode1Info => GetReservedFieldCodeInfo(0, nameof(ReservedFieldCode1));

		[MaxLength(1)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|ReservedFieldCode2", Caption = "Reserved Field 2 Code", MediumCaption = "Reserved Code 2", ShortCaption = "RS Code 2")]
		public ZString ReservedFieldCode2
		{
			get => GetReservedField(1)?.CY_Code ?? ZString.Empty;
			set => SetReservedFieldCode(1, value);
		}

		public ZPropertyInfo ReservedFieldCode2Info => GetReservedFieldCodeInfo(1, nameof(ReservedFieldCode2));

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|ReservedFieldValue1", Caption = "Reserved Field 1 Value", MediumCaption = "Reserved Value 1", ShortCaption = "RS Value 1")]
		public ZString ReservedFieldValue1
		{
			get => GetReservedField(0)?.CY_Data ?? ZString.Empty;
			set => SetReservedFieldValue(0, value);
		}

		public ZPropertyInfo ReservedFieldValue1Info => GetReservedFieldValueInfo(0, nameof(ReservedFieldValue1));

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|ReservedFieldValue2", Caption = "Reserved Field 2 Value", MediumCaption = "Reserved Value 2", ShortCaption = "RS Value 2")]
		public ZString ReservedFieldValue2
		{
			get => GetReservedField(1)?.CY_Data ?? ZString.Empty;
			set => SetReservedFieldValue(1, value);
		}

		public ZPropertyInfo ReservedFieldValue2Info => GetReservedFieldValueInfo(1, nameof(ReservedFieldValue2));

		ReservedField GetReservedField(int index) => ReservedFields.Count <= index ? null : ReservedFields[index];

		void SetReservedFieldCode(int index, ZString value)
		{
			var reservedField = ReservedFields.Count <= index ? ReservedFields.AddNew() : ReservedFields[index];
			reservedField.CY_Code = value;
		}

		void SetReservedFieldValue(int index, ZString value)
		{
			var reservedField = ReservedFields.Count <= index ? ReservedFields.AddNew() : ReservedFields[index];
			reservedField.CY_Data = value;
		}

		ZPropertyInfo GetReservedFieldCodeInfo(int index, string propertyName) => ReservedFields.Count <= index ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => ReservedFields[index].CY_CodeInfo);

		ZPropertyInfo GetReservedFieldValueInfo(int index, string propertyName) => ReservedFields.Count <= index ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => ReservedFields[index].CY_DataInfo);

		#endregion

		[ResourceStringData("Enterprise.Customs.TW.Business.JobComInvoiceLine|ManufacturerDocAddressOrgPK", Caption = "Foreign Manufacturer", MediumCaption = "Foreign Mfr.", ShortCaption = "FN Mfr.")]
		public ZGuid ManufacturerDocAddressOrgPK { get => ManufacturerDocAddress.OrganisationPK; set => ManufacturerDocAddress.OrganisationPK = value; }

		public ZPropertyInfo ManufacturerDocAddressOrgPKInfo => GetWrappedZPropertyInfo(nameof(ManufacturerDocAddressOrgPK), x => ManufacturerDocAddress.OrganisationPKInfo);

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse
		{
			get
			{
				if (IsRAP)
				{
					return [RapCaptionKey];
				}
				else if (IsROR)
				{
					return [RorCaptionKey];
				}
				else
				{
					return Array.Empty<string>();
				}
			}
		}

		public const string RapCaptionKey = "A58858AA-143B-4CCD-ACBF-C36D1AA00E85";

		public const string RorCaptionKey = "78269425-2009-4284-8D5D-E111CE8159C6";
	}
}
