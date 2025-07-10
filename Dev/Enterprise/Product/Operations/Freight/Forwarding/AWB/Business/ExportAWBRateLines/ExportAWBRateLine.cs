using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[DependentBusinessObject(typeof(ExportAWBHeader), "AWBRateLines")]
	[ProvideMetaDataProperty("NumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	[DebuggerDisplay("RateClass: {ER_RateClass} NatureAndQtyOfGoods: {ER_NatureAndQtyOfGoodsType} {NatureAndQtyOfGoods.Text}")]
	public class ExportAWBRateLine : AutoExportAWBRateLine, IExportAWBRateLine, IAWBRateLineMessageDetailsProvider
	{
		public ExportAWBRateLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CalculatingRateCharge = false;
			CalculatingTotal = false;
		}

		protected new class Schema : AutoExportAWBRateLine.Schema
		{
			public const string NatureAndQtyOfGoodsType = "NatureAndQtyOfGoodsType";
			public const string NatureAndQtyOfGoodsDescription = "NatureAndQtyOfGoodsDescription";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ER_WeightInLBsOrKGs = "";
		}

		#region IExportAWBRateLine members

		public ZString NatureAndQtyOfGoodsDescription
		{
			get { return NatureAndQtyOfGoods.Text; }
			set
			{
				ER_NatureAndQtyOfGoodsType = Master != null && Master.AllowRecogniseAndUpdateNatureAndQtyOfGoodsTypeFromText ?
					GetNatureAndQtyOfGoodsTypeFromText(value) : Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
				NatureAndQtyOfGoods.Text = value;
			}
		}

		public ZPropertyInfo NatureAndQtyOfGoodsDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.NatureAndQtyOfGoodsDescription, (obj) => NatureAndQtyOfGoods.TextInfo); }
		}

		[List("NatureAndQtyOfGoodsTypeList")]
		public ZString NatureAndQtyOfGoodsType
		{
			get { return ER_NatureAndQtyOfGoodsType; }
			set { ER_NatureAndQtyOfGoodsType = value; }
		}

		public ZPropertyInfo NatureAndQtyOfGoodsTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.NatureAndQtyOfGoodsType, (obj) => ER_NatureAndQtyOfGoodsTypeInfo); }
		}

		#endregion

		string GetNatureAndQtyOfGoodsTypeFromText(ZString text)
		{
			ZString result = ZString.Empty;

			if (NatureAndQtyOfGoodsVolume.IsValidVolume(text))
			{
				result = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			}
			else if (NatureAndQtyOfGoodsDimensions.IsValidDimension(text))
			{
				result = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
			}
			else if (NatureAndQtyOfGoodsSLAC.IsValidSLAC(text))
			{
				result = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount;
			}
			else if (NatureAndQtyOfGoodsOrigin.IsValidOrigin(text, Factory))
			{
				result = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin;
			}
			else if (NatureAndQtyOfGoodsLithiumBattery.IsValidLithiumBatteryType(text))
			{
				result = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			}
			else if (Constants.AWB.NatureAndQtyOfGoodsDetails.ConsolAsPerList == text)
			{
				result = Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			}
			else
			{
				result = (ZString)Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			}

			return result;
		}

		[List("NatureAndQtyOfGoodsTypeList")]
		public override ZString ER_NatureAndQtyOfGoodsType
		{
			get { return base.ER_NatureAndQtyOfGoodsType; }
			set
			{
				if (ER_NatureAndQtyOfGoodsType != value)
				{
					NatureAndQtyOfGoods oldNatureAndQtyOfGoods = NatureAndQtyOfGoods;

					base.ER_NatureAndQtyOfGoodsType = value;

					NatureAndQtyOfGoods currentNatureAndQtyOfGoods = NatureAndQtyOfGoods;

					if (oldNatureAndQtyOfGoods != currentNatureAndQtyOfGoods)
					{
						currentNatureAndQtyOfGoods.Text = oldNatureAndQtyOfGoods.Text;
						MarkAsNeedingValidationIncludingChildren();
					}
				}
			}
		}

		public CodeDescriptionPairList NatureAndQtyOfGoodsTypeList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AWBNatureAndQtyOfGoodsType); }
		}

		public NatureAndQtyOfGoods NatureAndQtyOfGoods
		{
			get
			{
				switch (ER_NatureAndQtyOfGoodsType)
				{
					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions:
						return NatureAndQtyOfGoodsDimensions;

					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume:
						return NatureAndQtyOfGoodsVolume;

					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount:
						return NatureAndQtyOfGoodsSLAC;

					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin:
						return NatureAndQtyOfGoodsOrigin;

					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery:
						return NatureAndQtyOfGoodsLithiumBattery;

					default:
						return NatureAndQtyOfGoodsText;
				}
			}
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public NatureAndQtyOfGoods NatureAndQtyOfGoodsText
		{
			get
			{
				if (natureAndQtyOfGoodsText == null)
				{
					natureAndQtyOfGoodsText = GetNewNatureAndQtyOfGoodsText();
					RegisterEditableChildObject(natureAndQtyOfGoodsText);
				}

				return natureAndQtyOfGoodsText;
			}
		}
		NatureAndQtyOfGoods natureAndQtyOfGoodsText;

		protected virtual NatureAndQtyOfGoods GetNewNatureAndQtyOfGoodsText()
		{
			return new NatureAndQtyOfGoods(this);
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public NatureAndQtyOfGoodsVolume NatureAndQtyOfGoodsVolume
		{
			get
			{
				if (natureAndQtyOfGoodsVolume == null)
				{
					natureAndQtyOfGoodsVolume = GetNatureAndQtyOfGoodsVolume();
					RegisterEditableChildObject(natureAndQtyOfGoodsVolume);
				}

				return natureAndQtyOfGoodsVolume;
			}
		}
		NatureAndQtyOfGoodsVolume natureAndQtyOfGoodsVolume;

		protected virtual NatureAndQtyOfGoodsVolume GetNatureAndQtyOfGoodsVolume()
		{
			return new NatureAndQtyOfGoodsVolume(this);
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public NatureAndQtyOfGoodsDimensions NatureAndQtyOfGoodsDimensions
		{
			get
			{
				if (natureAndQtyOfGoodsDimensions == null)
				{
					natureAndQtyOfGoodsDimensions = new NatureAndQtyOfGoodsDimensions(this);
					RegisterEditableChildObject(natureAndQtyOfGoodsDimensions);
				}

				return natureAndQtyOfGoodsDimensions;
			}
		}
		NatureAndQtyOfGoodsDimensions natureAndQtyOfGoodsDimensions;

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public NatureAndQtyOfGoodsLithiumBattery NatureAndQtyOfGoodsLithiumBattery
		{
			get
			{
				if (natureAndQtyOfGoodsLithiumBattery == null)
				{
					natureAndQtyOfGoodsLithiumBattery = GetNatureAndQtyOfGoodsLithiumBattery();
					RegisterEditableChildObject(natureAndQtyOfGoodsLithiumBattery);
				}

				return natureAndQtyOfGoodsLithiumBattery;
			}
		}
		NatureAndQtyOfGoodsLithiumBattery natureAndQtyOfGoodsLithiumBattery;

		protected virtual NatureAndQtyOfGoodsLithiumBattery GetNatureAndQtyOfGoodsLithiumBattery()
		{
			return new NatureAndQtyOfGoodsLithiumBattery(this);
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public NatureAndQtyOfGoodsSLAC NatureAndQtyOfGoodsSLAC
		{
			get
			{
				if (natureAndQtyOfGoodsSLAC == null)
				{
					natureAndQtyOfGoodsSLAC = new NatureAndQtyOfGoodsSLAC(this);
					RegisterEditableChildObject(natureAndQtyOfGoodsSLAC);
				}

				return natureAndQtyOfGoodsSLAC;
			}
		}
		NatureAndQtyOfGoodsSLAC natureAndQtyOfGoodsSLAC;

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public NatureAndQtyOfGoodsOrigin NatureAndQtyOfGoodsOrigin
		{
			get
			{
				if (natureAndQtyOfGoodsOrigin == null)
				{
					natureAndQtyOfGoodsOrigin = new NatureAndQtyOfGoodsOrigin(this);
					RegisterEditableChildObject(natureAndQtyOfGoodsOrigin);
				}

				return natureAndQtyOfGoodsOrigin;
			}
		}
		NatureAndQtyOfGoodsOrigin natureAndQtyOfGoodsOrigin;

		protected override IDisposable SuspendSettingHasChangesCore()
		{
			List<IDisposable> disposables = new List<IDisposable>
			{
				base.SuspendSettingHasChangesCore(),
				NatureAndQtyOfGoodsText.SuspendSettingHasChanges(),
				NatureAndQtyOfGoodsVolume.SuspendSettingHasChanges(),
				NatureAndQtyOfGoodsDimensions.SuspendSettingHasChanges(),
				NatureAndQtyOfGoodsLithiumBattery.SuspendSettingHasChanges(),
				NatureAndQtyOfGoodsSLAC.SuspendSettingHasChanges(),
				NatureAndQtyOfGoodsOrigin.SuspendSettingHasChanges()
			};

			return new DisposableAction(() => disposables.ForEach((disposable) => disposable.Dispose()));
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			ER_NatureAndQtyOfGoods = NatureAndQtyOfGoods.Text;
		}

		public void Clear()
		{
			ER_NoOfPiecesOrRCP = "0";
			ER_WeightInLBsOrKGs = "";
			ER_GrossWeight = 0;
			ER_ChargeableWeight = 0;
			ER_RateChargeOrDiscount = 0;
			ER_CommodityItemNumber = "";
			ER_RateClass = "";
		}

		public ZBool IsEmpty
		{
			get { return (IsRateDescriptionEmpty && IsNatureAndQtyOfGoodsEmpty); }
		}

		public ZBool IsRateDescriptionEmpty
		{
			get
			{
				return (ER_NoOfPiecesOrRCP == "0" || ER_NoOfPiecesOrRCP.IsEmpty)
					&& ER_GrossWeight == 0M
					&& ER_CommodityItemNumber == ""
					&& ER_RateChargeOrDiscount == 0M
					&& ER_RateClass.Trim() == ""
					&& ER_ChargeableWeight == 0M;
			}
		}

		public ZBool IsNatureAndQtyOfGoodsEmpty
		{
			get { return (ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription && NatureAndQtyOfGoods.Text.IsEmpty); }
		}

		public ZInt ER_NoOfPiecesOrRCPAsInt
		{
			get
			{
				ZInt result = 0;

				ZInt parseResult = 0;

				if (ZInt.TryParse(ER_NoOfPiecesOrRCP, out parseResult))
				{
					result = parseResult;
				}

				return result;
			}
		}

		public override ZString ER_NoOfPiecesOrRCP
		{
			get
			{
				return base.ER_NoOfPiecesOrRCP;
			}
			set
			{
				base.ER_NoOfPiecesOrRCP = value;
				if (Master != null)
				{
					Master.EH_TotalNoOfPiecesInfo.RefreshBinding();
					Master.EH_TotalLineTotalsInfo.RefreshBinding();
					Master.EH_TotalWeightCOLInfo.RefreshBinding();
					Master.EH_TotalWeightPPDInfo.RefreshBinding();

					if (!Master.IsValidationSuspended)
					{
						Master.Validation.ValidateEH_TotalNoOfPieces();
					}
				}
			}
		}

		[DecimalPlaces(1)]
		public override ZDecimal ER_GrossWeight
		{
			get { return base.ER_GrossWeight; }
			set
			{
				ZDecimal newValue = value;
				if (base.ER_GrossWeight != newValue)
				{
					ER_GrossWeightInfo.RefreshBinding();
				}

				newValue = this.GetRoundedGrossWeight(ExportAWBRateLineSchema.ER_GrossWeight, newValue);

				if (base.ER_GrossWeight != newValue)
				{
					base.ER_GrossWeight = newValue;

					if (Master != null)
					{
						Master.EH_TotalGrossWeightInfo.RefreshBinding();
						Master.MarkAsNeedingValidation();

						if (!Master.IsValidationSuspended)
						{
							Master.Validation.ValidateEH_TotalGrossWeight();
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateER_WeightInLBsOrKGs();
					}
				}
			}
		}

		[DecimalPlaces(1)]
		public override ZDecimal ER_ChargeableWeight
		{
			get { return base.ER_ChargeableWeight; }
			set
			{
				ZDecimal newValue = value;

				if (ER_ChargeableWeight != newValue)
				{
					ER_ChargeableWeightInfo.RefreshBinding();
				}

				if (Master != null)
				{
					newValue = AWBRoundingHelper.GetAWBRoundedValue(Master.AWBType, newValue);
				}

				if (ER_ChargeableWeight != newValue)
				{
					base.ER_ChargeableWeight = newValue;
					CalculateTotal();
					ER_TotalInfo.RefreshBinding();
				}

				if (Master != null)
				{
					Master.EH_TotalLineTotalsInfo.RefreshBinding();
					Master.EH_TotalWeightCOLInfo.RefreshBinding();
					Master.EH_TotalWeightPPDInfo.RefreshBinding();
				}
			}
		}

		public int GetNumberOfDecimals(PropertyDescriptor property)
		{
			return NumberOfDecimalsHelper.GetNumberOfDecimals(this, property, GetNumberOfDecimalsCore);
		}

		protected virtual int GetNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return -1;
		}

		public virtual ZDecimal GetRoundedGrossWeight(SchemaColumn column, ZDecimal value)
		{
			var result = new ZDecimal(decimal.Ceiling(value * 10) / 10);

			if (column is SchemaDecimalColumn decimalColumn
				&& !result.IsWithinSqlPrecisionAndScale(decimalColumn.Precision, decimalColumn.Scale)
				&& value.IsWithinSqlPrecisionAndScale(decimalColumn.Precision, decimalColumn.Scale))
			{
				result = DefaultNumberOfDecimals.GetRoundedValue(value, RoundingModes.Down, GetNumberOfDecimals(ER_GrossWeightInfo));
			}

			return result;
		}

		[DecimalPlaces(2)]
		public override ZDecimal ER_RateChargeOrDiscount
		{
			get { return base.ER_RateChargeOrDiscount; }
			set
			{
				if (ER_RateChargeOrDiscount != value)
				{
					base.ER_RateChargeOrDiscount = value;
					CalculateTotal();
				}
				ER_TotalInfo.RefreshBinding();
				if (Master != null)
				{
					Master.EH_TotalLineTotalsInfo.RefreshBinding();
					Master.EH_TotalWeightCOLInfo.RefreshBinding();
					Master.EH_TotalWeightPPDInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal ER_Total
		{
			get { return base.ER_Total; }
			set
			{
				if (ER_Total != value)
				{
					base.ER_Total = value;
					CalculateRateCharge();
					ER_RateChargeOrDiscountInfo.RefreshBinding();
					if (Master != null)
					{
						Master.MarkAsNeedingValidation();
					}
				}
			}
		}

		[List("RateClassList")]
		public override ZString ER_RateClass
		{
			get { return base.ER_RateClass; }
			set
			{
				if (ER_RateClass != value)
				{
					base.ER_RateClass = value;
					CalculateTotal();
					ER_TotalInfo.RefreshBinding();
				}
			}
		}

		[List("RateUQList")]
		public override ZString ER_WeightInLBsOrKGs
		{
			get { return base.ER_WeightInLBsOrKGs; }
			set
			{
				base.ER_WeightInLBsOrKGs = value;

				if (Master != null)
				{
					base.ER_GrossWeight = this.GetRoundedGrossWeight(ExportAWBRateLineSchema.ER_GrossWeight, ER_GrossWeight);
					Master.AWBRateLines.RefreshBindingIncludingChildren();
					Master.EH_TotalGrossWeightInfo.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateER_GrossWeight();
				}
			}
		}

		public ExportAWBRateLine GetNextRateLine()
		{
			if (Master != null)
			{
				return Master.GetRateLineAt(ER_LineCount + 1) as ExportAWBRateLine;
			}
			return null;
		}

		public override ZString ER_CommodityItemNumber
		{
			get { return base.ER_CommodityItemNumber; }
			set
			{
				if (ER_CommodityItemNumber != value)
				{
					base.ER_CommodityItemNumber = value;
					CalculateTotal();
					ER_TotalInfo.RefreshBinding();
				}
			}
		}

		public int ER_CommodityItemNumber_MaxLength
		{
			get { return GetER_CommodityItemNumber_MaxLength(); }
		}

		protected virtual int GetER_CommodityItemNumber_MaxLength()
		{
			return ExportAWBRateLine.Schema.ER_CommodityItemNumberMaxLength;
		}

		public CodeDescriptionPairList RateUQList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AWBRateUQ); }
		}

		public CodeDescriptionPairList RateClassList
		{
			get { return RateClassCoreList; }
		}

		protected virtual CodeDescriptionPairList RateClassCoreList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AWBRateClass); }
		}

		public ExportAWBHeader Master
		{
			get { return Factory.Load<ExportAWBHeader>(ER_EH); }
		}

		public int CurrencyDecimals
		{
			get
			{
				const int DefaultDecimals = 2;

				if (Master != null)
				{
					RefCurrency refCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Master.EH_Currency));
					return refCurrency != null ? refCurrency.Decimals : DefaultDecimals;
				}

				return DefaultDecimals;
			}
		}

		bool CalculatingTotal;

		public void CalculateTotal()
		{
			if (CalculatingRateCharge || IsCalculationSuppressed)
			{
				return;
			}

			string rateClass = ER_RateClass;
			if (ER_RateClass == Constants.AWB.RateClass.ClassRateSurcharge || ER_RateClass == Constants.AWB.RateClass.ClassRateReduction)
			{
				if (ER_CommodityItemNumber.Length > 0)
				{
					rateClass = ER_CommodityItemNumber.Left(1);
				}
			}

			ZDecimal calculatedTotal = 0M;
			switch (rateClass)
			{
				case Constants.AWB.RateClass.MinimumCharge:
				case Constants.AWB.RateClass.ClassRateSurcharge:
				case Constants.AWB.RateClass.BasicCharge:
					calculatedTotal = ZArchitecture.Core.Utilities.Round(ER_RateChargeOrDiscount, CurrencyDecimals);
					break;
				case Constants.AWB.RateClass.UnitLoadDeviceBasicCharge:
					calculatedTotal = ZArchitecture.Core.Utilities.Round(ER_NoOfPiecesOrRCPAsInt * ER_RateChargeOrDiscount, CurrencyDecimals);
					break;
				case "":
					calculatedTotal = 0M;
					break;
				default:
					calculatedTotal = ZArchitecture.Core.Utilities.Round(ER_ChargeableWeight * ER_RateChargeOrDiscount, CurrencyDecimals);
					break;
			}

			if (calculatedTotal != ER_Total)
			{
				CalculatingTotal = true;
				ER_Total = calculatedTotal;
				CalculatingTotal = false;
			}
		}

		bool CalculatingRateCharge;

		void CalculateRateCharge()
		{
			if (CalculatingTotal || IsCalculationSuppressed)
			{
				return;
			}

			ZDecimal calculatedTotal = 0M;

			string rateClass = ER_RateClass;
			if (ER_RateClass == Constants.AWB.RateClass.ClassRateSurcharge || ER_RateClass == Constants.AWB.RateClass.ClassRateReduction)
			{
				if (ER_CommodityItemNumber.Length > 0)
				{
					rateClass = ER_CommodityItemNumber.Left(1);
				}
			}

			switch (rateClass)
			{
				case Constants.AWB.RateClass.MinimumCharge:
				case Constants.AWB.RateClass.BasicCharge:
					calculatedTotal = ZArchitecture.Core.Utilities.Round(ER_Total, CurrencyDecimals);
					break;
				case Constants.AWB.RateClass.UnitLoadDeviceBasicCharge:
					if (ER_NoOfPiecesOrRCPAsInt != 0M)
					{
						calculatedTotal = ZArchitecture.Core.Utilities.Round(ER_Total / ER_NoOfPiecesOrRCPAsInt, CurrencyDecimals);
					}
					break;
				case "":
					calculatedTotal = 0M;
					break;
				default:
					if (ER_ChargeableWeight != 0M)
					{
						calculatedTotal = ZArchitecture.Core.Utilities.Round(ER_Total / ER_ChargeableWeight, CurrencyDecimals);
					}
					break;
			}

			if (calculatedTotal != ER_RateChargeOrDiscount)
			{
				CalculatingRateCharge = true;
				ER_RateChargeOrDiscount = calculatedTotal;
				CalculatingRateCharge = false;
			}
		}

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return Master != null && !Master.EH_AreRateLinesOverridden;
		}

		public void RefreshBindingForOverride()
		{
			ER_NoOfPiecesOrRCPInfo.RefreshBinding();
			ER_GrossWeightInfo.RefreshBinding();
			ER_WeightInLBsOrKGsInfo.RefreshBinding();
			ER_RateClassInfo.RefreshBinding();
			ER_CommodityItemNumberInfo.RefreshBinding();
			ER_ChargeableWeightInfo.RefreshBinding();
			ER_RateChargeOrDiscountInfo.RefreshBinding();
			ER_TotalInfo.RefreshBinding();
			NatureAndQtyOfGoodsText?.TextInfo.RefreshBinding();
			NatureAndQtyOfGoodsDimensions?.LengthInfo.RefreshBinding();
			NatureAndQtyOfGoodsVolume?.VolumeInfo.RefreshBinding();
			NatureAndQtyOfGoodsSLAC?.CountInfo.RefreshBinding();
			NatureAndQtyOfGoodsOrigin?.CountryInfo.RefreshBinding();
			NatureAndQtyOfGoodsLithiumBattery?.LithiumBatteryTypeInfo.RefreshBinding();
		}

		#region Calculation Suppresser

		public IDisposable SuppressTotalAndRateChargeCalculation()
		{
			return new CalculationSuppresser(this);
		}

		sealed class CalculationSuppresser : IDisposable
		{
			public CalculationSuppresser(ExportAWBRateLine exportAWBRateLine)
			{
				this.rateLine = exportAWBRateLine;
				rateLine.calculationSuppressedIndex++;
			}

			public void Dispose()
			{
				if (!isDisposed)
				{
					rateLine.calculationSuppressedIndex--;
					isDisposed = true;
				}
			}

			readonly ExportAWBRateLine rateLine;
			bool isDisposed;
		}

		bool IsCalculationSuppressed
		{
			get { return calculationSuppressedIndex > 0; }
		}

		int calculationSuppressedIndex;

		public virtual bool RequireHSCode => false;

		public virtual bool IsHSCodeLine => false;

		#endregion

		#region IAWBRateLineMessageDetailsProvider Members

		public ZString WeightInLBsOrKGs => ER_WeightInLBsOrKGs;

		#endregion
	}
}
