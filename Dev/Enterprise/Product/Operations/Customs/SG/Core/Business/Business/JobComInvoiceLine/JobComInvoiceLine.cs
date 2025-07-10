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
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceLine : TypeSafeJobComInvoiceLine
		, Integration.Customs.SG.IJobComInvoiceLine
		, IChargeApportionee
		, ICusCodeDataTypeSupporter
		, IAdditionalLineTariffDetailParent
	{
		public new class Schema : BaseJobComInvoiceLine.Schema
		{
			public const string JI_Calc_ExciseAmount = "JI_Calc_ExciseAmount";
			public const string JI_Calc_OtherTaxAmount = "JI_Calc_OtherTaxAmount";
			public const string JI_Calc_OtherAmount = "JI_Calc_OtherAmount";
			public const string CertItemDescription = "CertItemDescription";
			public const string MarksAndNumbers = "MarksAndNumbers";
			public const int CertItemDescriptionMaxLength = 1750;
			public const int OutwardMAWBMaxLength = 35;
		}

		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>(invoiceLine => invoiceLine.UniversalTariff, true);

		public override void OnSaving()
		{
			base.OnSaving();

			var declaration = Declaration;
			if (declaration != null)
			{
				var shipment = declaration.Shipment;
				if (shipment != null && shipment.OuterPackLines != null && shipment.OuterPackLines.Cast<ForwardingPackLine>().Any(x => x.JL_HarmonisedCode.IsEmpty))
				{
					if (declaration.InvoiceLines.Count > 0)
					{
						if (declaration.InvoiceLines.Cast<JobComInvoiceLine>().AllSame(x => x.JI_Tariff))
						{
							shipment.OuterPackLines.Cast<ForwardingPackLine>().Where(x => x.JL_HarmonisedCode.IsEmpty).ForEach(x => x.JL_HarmonisedCode = declaration.InvoiceLines[0].JI_Tariff.Left(ForwardingPackLine.Schema.JL_HarmonisedCodeMaxLength));
						}
						else if (declaration.InvoiceLines.Cast<JobComInvoiceLine>().AllSame(x => x.JI_Tariff.Left(6)))
						{
							shipment.OuterPackLines.Cast<ForwardingPackLine>().Where(x => x.JL_HarmonisedCode.IsEmpty).ForEach(x => x.JL_HarmonisedCode = declaration.InvoiceLines[0].JI_Tariff.Left(6));
						}
					}
				}
			}
		}

		public override void Delete()
		{
			base.Delete();
			this.DeleteHiddenNotes();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.STD;
		}

		public override ZString JI_Description
		{
			get
			{
				return base.JI_Description;
			}
			set
			{
				if (base.JI_Description != value)
				{
					base.JI_Description = value;
					if (!IsCopying)
					{
						if (Declaration != null && Declaration.HasCofO && CertItemDescription.IsEmpty)
						{
							CertItemDescription = JI_Description;
						}
					}
				}
			}
		}

		public override ZPropertyInfo JI_DescriptionInfo
		{
			get
			{
				ZPropertyInfo result = base.JI_DescriptionInfo;

				return result;
			}
		}

		public int JI_Description_MaxLength
		{
			get
			{
				int maxLength = Schema.JI_DescriptionMaxLength;
				if (!IsCopying)
				{
					if (Declaration != null && Declaration.IsTradeNet4Point1)
					{
						maxLength = 512;
					}
					else
					{
						maxLength = 175;
					}
				}
				return maxLength;
			}
		}

		public override ZBool SG_IsStrategic
		{
			get { return base.SG_IsStrategic; }
			set
			{
				if (base.SG_IsStrategic != value)
				{
					base.SG_IsStrategic = value;

					if (SG_IsStrategic)
					{
						SG_EndUseCode1 = CA_SC1CodeList.Codes.NMU;
						SG_EndUseCode2 = CA_SC2CodeList.Codes.NGU;
						SG_EndUseCode3 = CA_SC3CodeList.Codes.NMD;
					}
					else
					{
						SG_StrategicGoodsCategory = "";
						SG_CategoryCode = "";
						SG_StrategicGoodsProductCodeQuantity = 0;
						SG_StrategicGoodsProductCodeQuantityUnit = "";
						SG_EndUseCode1 = "";
						SG_EndUseCode2 = "";
						SG_EndUseCode3 = "";
						SG_EndUseDescription = "";
					}
				}
			}
		}

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PreferenceList))]
		[ResourceStringData("SGInvoiceLineUserControl|4ec5257e-da6b-4d25-88fd-d5361714add6", Caption = "Preference")]
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
					if (InvoiceHeader != null)
					{
						InvoiceHeader.MarkAsNeedingValidation();
					}
					if (UniversalTariff != null)
					{
						SetDutyRateExciseRateAndSG_UnitDutiableWGTVOLQTYUnit();
					}
				}
				SG_DutyUnitRateInfo.RefreshBinding();
			}
		}

		[MaxLength(35)]
		public override ZString JI_BrandName
		{
			get => base.JI_BrandName;
			set => base.JI_BrandName = value;
		}

		[MaxLength(35)]
		public override ZString JI_Model
		{
			get => base.JI_Model;
			set => base.JI_Model = value;
		}

		[MaxLength(Schema.OutwardMAWBMaxLength)]
		public ZString JI_OutwardMAWB { get => base.SG_OutwardMAWB; set => base.SG_OutwardMAWB = value; }

		public ZWrappedPropertyInfo JI_OutwardMAWBInfo => GetWrappedZPropertyInfo(nameof(JI_OutwardMAWB), (x) => base.SG_OutwardMAWBInfo);

		#region Customs Amounts

		public override ZDecimal JI_CustomsValue
		{
			get { return SG_LastSellingPrice > 0 ? SG_LastSellingPrice : JI_Calc_CIF_InLocalCurrency; }
		}

		protected override bool ShouldRecalculatePercentageChargeAmountBasedOnLinePrice
		{
			get { return false; }
		}

		#region JI_Calc_ExciseAmount

		public ZDecimal JI_Calc_ExciseAmount
		{
			get { return (CusEntryLine == null) ? ZDecimal.Zero : GetAmountApportionedFromCusEntryLine(((CusEntryLine)CusEntryLine).ExciseAmount).Amount; }
		}

		public ZPropertyInfo JI_Calc_ExciseAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_ExciseAmount); }
		}

		public override ZString JI_AddInfo
		{
			get { return base.JI_AddInfo; }
			set
			{
				if (base.JI_AddInfo != value)
				{
					base.JI_AddInfo = value;

					if (InvoiceHeader != null)
					{
						InvoiceHeader.MarkAsNeedingValidation();
					}
					if (Declaration != null)
					{
						Declaration.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString JI_NAddInfo
		{
			get { return base.JI_NAddInfo; }
			set
			{
				if (base.JI_NAddInfo != value)
				{
					base.JI_NAddInfo = value;

					if (InvoiceHeader != null)
					{
						InvoiceHeader.MarkAsNeedingValidation();
					}
					if (Declaration != null)
					{
						Declaration.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region JI_Calc_OtherTaxAmount

		public ZDecimal JI_Calc_OtherTaxAmount
		{
			get { return (CusEntryLine == null) ? ZDecimal.Zero : GetAmountApportionedFromCusEntryLine(((CusEntryLine)CusEntryLine).OtherTaxAmount).Amount; }
		}

		public ZPropertyInfo JI_Calc_OtherTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_OtherTaxAmount); }
		}

		#endregion

		#region JI_Calc_OtherCharges

		public ZDecimal JI_Calc_OtherAmount
		{
			get
			{
				ZDecimal result = 0m;
				JobComInvCharge[] otherCharges = ApportionedCharges.GetCharge(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges);

				foreach (JobComInvCharge otherCharge in otherCharges)
				{
					if (InvoiceHeader != null && InvoiceHeader.Invoice_Currency != null)
					{
						result += CurrencyConverter.ConvertExact(otherCharge.Money, InvoiceHeader.Invoice_Currency).Amount;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo JI_Calc_OtherAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_OtherAmount); }
		}

		#endregion

		#endregion

		#region Quanities and Units

		public override ZInt SG_OuterPackQuantity
		{
			get { return base.SG_OuterPackQuantity; }
			set
			{
				if (base.SG_OuterPackQuantity != value)
				{
					base.SG_OuterPackQuantity = value;
					TotalQuantityInfo.RefreshBinding();
					SetTotalDutiableQty();
				}
			}
		}

		public override ZString SG_OuterPackQuantityUnit
		{
			get { return base.SG_OuterPackQuantityUnit; }
			set
			{
				if (base.SG_OuterPackQuantityUnit != value)
				{
					base.SG_OuterPackQuantityUnit = value;
					SetTotalDutiableQty();
				}
			}
		}

		public override ZInt SG_InPackQuantity
		{
			get { return base.SG_InPackQuantity; }
			set
			{
				if (base.SG_InPackQuantity != value)
				{
					base.SG_InPackQuantity = value;
					TotalQuantityInfo.RefreshBinding();
					SetTotalDutiableQty();
				}
			}
		}

		public override ZString SG_InPackQuantityUnit
		{
			get { return base.SG_InPackQuantityUnit; }
			set
			{
				if (base.SG_InPackQuantityUnit != value)
				{
					base.SG_InPackQuantityUnit = value;
					SetTotalDutiableQty();
				}
			}
		}

		public override ZInt SG_InnerPackQuantity
		{
			get { return base.SG_InnerPackQuantity; }
			set
			{
				if (base.SG_InnerPackQuantity != value)
				{
					base.SG_InnerPackQuantity = value;
					TotalQuantityInfo.RefreshBinding();
					SetTotalDutiableQty();
				}
			}
		}

		public override ZString SG_InnerPackQuantityUnit
		{
			get { return base.SG_InnerPackQuantityUnit; }
			set
			{
				if (base.SG_InnerPackQuantityUnit != value)
				{
					base.SG_InnerPackQuantityUnit = value;
					SetTotalDutiableQty();
				}
			}
		}

		public override ZInt SG_InmostPackQuantity
		{
			get { return base.SG_InmostPackQuantity; }
			set
			{
				if (base.SG_InmostPackQuantity != value)
				{
					base.SG_InmostPackQuantity = value;
					TotalQuantityInfo.RefreshBinding();
					SetTotalDutiableQty();
				}
			}
		}

		public override ZString SG_InmostPackQuantityUnit
		{
			get { return base.SG_InmostPackQuantityUnit; }
			set
			{
				if (base.SG_InmostPackQuantityUnit != value)
				{
					base.SG_InmostPackQuantityUnit = value;
					SetTotalDutiableQty();
				}
			}
		}

		[MaxLength(100)]
		public ZString TotalQuantity
		{
			get
			{
				decimal value = !(SG_OuterPackQuantity == 0 && SG_InPackQuantity == 0 && SG_InnerPackQuantity == 0 && SG_InmostPackQuantity == 0)
					? (decimal)(SG_OuterPackQuantity == 0 ? (ZInt)1 : SG_OuterPackQuantity) * (decimal)(SG_InPackQuantity == 0 ? (ZInt)1 : SG_InPackQuantity) * (decimal)(SG_InnerPackQuantity == 0 ? (ZInt)1 : SG_InnerPackQuantity) * (decimal)(SG_InmostPackQuantity == 0 ? (ZInt)1 : SG_InmostPackQuantity)
					: 0;

				return Utilities.FormatNumberNationalWithGroupSeparators(value, 0);
			}
		}

		public ZPropertyInfo TotalQuantityInfo
		{
			get { return GetZPropertyInfo(nameof(TotalQuantity)); }
		}

		public override ZDecimal SG_UnitDutiableWGTVOLQTY
		{
			get { return base.SG_UnitDutiableWGTVOLQTY; }
			set
			{
				if (base.SG_UnitDutiableWGTVOLQTY != value)
				{
					base.SG_UnitDutiableWGTVOLQTY = value;
					SetTobaccoMultiplier();
					SetTotalDutiableQty();
				}
			}
		}

		public override ZString SG_UnitDutiableWGTVOLQTYUnit
		{
			get { return base.SG_UnitDutiableWGTVOLQTYUnit; }
			set
			{
				if (base.SG_UnitDutiableWGTVOLQTYUnit != value)
				{
					base.SG_UnitDutiableWGTVOLQTYUnit = value;
					if (!((ISupportDataImporting)this).IsImportingData && !IsCopying)
					{
						SG_TotalDutiableWGTVOLQTYUnit = SG_UnitDutiableWGTVOLQTYUnit;
					}
				}
			}
		}

		public override ZInt SG_TobaccoMultiplier
		{
			get { return base.SG_TobaccoMultiplier; }
			set
			{
				if (base.SG_TobaccoMultiplier != value)
				{
					base.SG_TobaccoMultiplier = value;
					SetTotalDutiableQty();
				}
			}
		}

		void SetTobaccoMultiplier()
		{
			if (!((ISupportDataImporting)this).IsImportingData && !IsCopying)
			{
				SG_TobaccoMultiplier = (JI_Tariff == "24022090" || JI_Tariff == "24029020" || JI_Tariff == "24022020") ? (ZInt)Math.Ceiling(SG_UnitDutiableWGTVOLQTY) : ZInt.Zero;
			}
		}

		void SetTotalDutiableQty()
		{
			if (!((ISupportDataImporting)this).IsImportingData && !IsCopying)
			{
				if (UniversalTariff != null)
				{
					SG_TotalDutiableWGTVOLQTY =
						!(SG_OuterPackQuantity == 0 && SG_InPackQuantity == 0 && SG_InnerPackQuantity == 0 && SG_InmostPackQuantity == 0)
						? (decimal)(SG_OuterPackQuantity == 0 ? (ZInt)1 : SG_OuterPackQuantity) * (decimal)(SG_InPackQuantity == 0 ? (ZInt)1 : SG_InPackQuantity) * (decimal)(SG_InnerPackQuantity == 0 ? (ZInt)1 : SG_InnerPackQuantity) * (decimal)(SG_InmostPackQuantity == 0 ? (ZInt)1 : SG_InmostPackQuantity)
						: 0;

					if (SG_TobaccoMultiplier == 0)
					{
						SG_TotalDutiableWGTVOLQTY *= SG_UnitDutiableWGTVOLQTY == 0 ? (ZDecimal)1 : SG_UnitDutiableWGTVOLQTY;
					}
					else
					{
						SG_TotalDutiableWGTVOLQTY *= SG_TobaccoMultiplier;
					}

					if (JI_Tariff.StartsWith("2402"))
					{
						if (SG_InmostPackQuantityUnit == UnitOfQuantityCodeList.Codes.STK || SG_InnerPackQuantityUnit == UnitOfQuantityCodeList.Codes.STK || SG_InPackQuantityUnit == UnitOfQuantityCodeList.Codes.STK || SG_OuterPackQuantityUnit == UnitOfQuantityCodeList.Codes.STK)
						{
							if (JI_Tariff != "24022090" && JI_Tariff != "24029020" && JI_Tariff != "24022020")
							{
								SG_TotalDutiableWGTVOLQTY /= 1000;
							}
						}
					}
				}
			}
		}

		#endregion

		#region Part
		public new OrgSupplierPart Part
		{
			get { return (OrgSupplierPart)base.Part; }
		}

		public override void UpdateDetailsFromProductOnPartChangeCore()
		{
			base.UpdateDetailsFromProductOnPartChangeCore();
			if (Part != null)
			{
				JI_BrandName = Part.OP_Brand;
				JI_Model = Part.OP_Model;
				JI_BrandNameInfo.RefreshBinding();
				JI_ModelInfo.RefreshBinding();
			}
		}

		public override void UpdateDetailsFromPivotOnPartChangeCore()
		{
			base.UpdateDetailsFromPivotOnPartChangeCore();
			var pivot = (CusClassPartPivot)Pivot;
			var classification = pivot?.Classification;
			if (classification != null)
			{
				JI_CC = classification.PK;
			}
			else
			{
				UpdateDetailsFromTariffData(pivot);
			}
		}

		void UpdateDetailsFromTariffData(ITariffData tariffData)
		{
			if (tariffData != null)
			{
				JI_Tariff = tariffData.TariffNum;
				SG_PercAlcohol = tariffData.PercAlcohol;
				ProductCodes.RemoveAndDeleteAll();
				foreach (ProductCode productCode in tariffData.ProductCodes)
				{
					var lineProduct = ProductCodes.AddNew();
					lineProduct.BZ_Tariff = productCode.CY_Data.Left(lineProduct.BZ_TariffInfo.MaxLength);
				}
			}
		}

		#endregion

		#region Classification

		public override ZGuid JI_CC
		{
			get { return base.JI_CC; }
			set
			{
				if (base.JI_CC != value)
				{
					base.JI_CC = value;
					if (!IsCopying)
					{
						UpdateDetailsFromTariffData(Classification);
					}
				}
			}
		}

		public new Classification Classification
		{
			get { return base.Classification != null ? Factory.Load<Classification>(base.Classification.PK) : null; }
		}

		#endregion

		#region Tariff and Customs UQ

		public override ZString JI_Tariff
		{
			get { return base.JI_Tariff; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
				{
					if (base.JI_Tariff != value)
					{
						base.JI_Tariff = value;

						if (!IsCopying)
						{
							SetTobaccoMultiplier();
							SG_TariffCommodityType = "";
							SG_ESNDPIndicator = "";

							if (UniversalTariff != null)
							{
								SG_TariffCommodityType = UniversalTariff.GetAttribute(SGConstants.Attributes.Names.CommodityType)?.ZZ3_Value ?? string.Empty;
								SetTotalDutiableQty();
								SetDutyRateExciseRateAndSG_UnitDutiableWGTVOLQTYUnit();
							}
							else
							{
								SG_TotalDutiableWGTVOLQTYUnit = "";
								SG_UnitDutiableWGTVOLQTYUnit = SG_TotalDutiableWGTVOLQTYUnit;
								SG_DutyUnitRate = 0m;
								SG_DutyPercentageRate = 0m;
								SG_ExciseUnitRate = 0m;
								SG_ExcisePercentageRate = 0m;
							}
						}
					}

					SG_TotalDutiableWGTVOLQTYUnitInfo.RefreshBinding();
					RefreshDutyRateExciseRateAndSG_UnitDutiableWGTVOLQTYUnitInfo();
				}
			}
		}

		void RefreshDutyRateExciseRateAndSG_UnitDutiableWGTVOLQTYUnitInfo()
		{
			SG_UnitDutiableWGTVOLQTYUnitInfo.RefreshBinding();
			SG_DutyUnitRateInfo.RefreshBinding();
			SG_DutyPercentageRateInfo.RefreshBinding();
			SG_ExciseUnitRateInfo.RefreshBinding();
			SG_ExcisePercentageRateInfo.RefreshBinding();
		}

		void SetDutyRateExciseRateAndSG_UnitDutiableWGTVOLQTYUnit()
		{
			var dutyRate = UniversalTariff.GetDutyRate(this);
			var exciseRate = UniversalTariff.GetExciseRate(EffectiveDateForDutyRate);

			var unitQty = string.IsNullOrEmpty(dutyRate.UnitQty) ? exciseRate.UnitQty : dutyRate.UnitQty;
			SG_DutyPercentageRate = dutyRate.PercentageRate;
			SG_DutyUnitRate = dutyRate.UnitRate;

			SG_ExcisePercentageRate = exciseRate.PercentageRate;
			SG_ExciseUnitRate = exciseRate.UnitRate;

			SG_UnitDutiableWGTVOLQTYUnit = unitQty == SGConstants.LPA ? UnitOfQuantityCodeList.Codes.LTR : unitQty.ToString();
		}

		public override ZString SG_TariffCommodityType
		{
			get { return base.SG_TariffCommodityType; }
			set
			{
				base.SG_TariffCommodityType = value;
				if (SG_TariffCommodityType == CommodityTypeList.Codes.Tobacco)
				{
					SG_ESNDPIndicator = MarkingCodeList.Codes.HW;
				}
			}
		}

		public override ZString JI_CountryOfOrigin
		{
			get { return base.JI_CountryOfOrigin; }
			set
			{
				var oldValue = base.JI_CountryOfOrigin;
				base.JI_CountryOfOrigin = value;
				if (oldValue != value && !IsCopying)
				{
					if (UniversalTariff != null)
					{
						SetDutyRateExciseRateAndSG_UnitDutiableWGTVOLQTYUnit();
						RefreshDutyRateExciseRateAndSG_UnitDutiableWGTVOLQTYUnitInfo();
					}
				}
			}
		}

		protected void SetDefaultSG_ESNDPIndicator()
		{
			if (SG_ESNDPIndicator == "")
			{
				SG_ESNDPIndicator = MarkingCodeList.Codes.HW;
			}
		}

		public override bool ShouldWipeNKTaxType => false;

		public override ZString JI_InvoiceUQ
		{
			get { return base.JI_InvoiceUQ; }
			set
			{
				if (JI_InvoiceUQ != value && !SetterSuspender.IsSetterSuspended(nameof(JI_InvoiceUQ)))
				{
					base.JI_InvoiceUQ = value;
				}
			}
		}

		protected override BaseCustomsQuantityConverter GetCustomsQuantityConverter()
		{
			return new CustomsQuantityConverter(this, (ZPropertyInfoDecimal)JI_CustomsQuantityInfo, (ZPropertyInfoString)JI_CustomsUnitQtyInfo);
		}

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			return UniversalTariff?.ZZ1_Description ?? ZString.Empty;
		}

		#endregion

		#region Origin Criterion

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OriginCriterionCodeList))]
		public override ZString SG_CertOriginCriterion1
		{
			get { return base.SG_CertOriginCriterion1; }
			set
			{
				ZString[] originCriterion = value.Split(',');
				if (originCriterion.Length > 0)
				{
					base.SG_CertOriginCriterion1 = originCriterion[0];
				}

				base.SG_CertOriginCriterion2 = originCriterion.Length > 1 ? originCriterion[1].TrimStart() : ZString.Empty;
				base.SG_CertOriginCriterion3 = originCriterion.Length > 2 ? originCriterion[2].TrimStart() : ZString.Empty;
				SG_CertOriginCriterion2Info.RefreshBinding();
				SG_CertOriginCriterion3Info.RefreshBinding();
			}
		}

		#endregion

		protected override ZString CustomsCountryCodeCore
		{
			get { return Core.Constants.CountryCodes.Singapore; }
		}
		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		#endregion

		#region CASC Codes

		#region CASC Code 1

		[ChildEditable(true)]
		public CASCCode1Collection CASCCode1s
		{
			get
			{
				if (cascCode1s == null)
				{
					cascCode1s = new CASCCode1Collection(this);
					RegisterEditableChildObject(cascCode1s);
					cascCode1s.Load();
				}

				return cascCode1s;
			}
		}
		CASCCode1Collection cascCode1s;

		#endregion

		#region CASC Code 2

		[ChildEditable(true)]
		public CASCCode2Collection CASCCode2s
		{
			get
			{
				if (cascCode2s == null)
				{
					cascCode2s = new CASCCode2Collection(this);
					RegisterEditableChildObject(cascCode2s);
					cascCode2s.Load();
				}

				return cascCode2s;
			}
		}
		CASCCode2Collection cascCode2s;

		#endregion

		#region CASC Code 3

		[ChildEditable(true)]
		public CASCCode3Collection CASCCode3s
		{
			get
			{
				if (cascCode3s == null)
				{
					cascCode3s = new CASCCode3Collection(this);
					RegisterEditableChildObject(cascCode3s);
					cascCode3s.Load();
				}

				return cascCode3s;
			}
		}
		CASCCode3Collection cascCode3s;

		#endregion

		#endregion

		#region Product Codes

		[ChildEditable(true)]
		public CusLineTariffDetailCollection<CusLineTariffDetail> ProductCodes
		{
			get
			{
				if (productCodes == null)
				{
					productCodes = new CusLineTariffDetailCollection<CusLineTariffDetail>(this);
					productCodes.Load();
					RegisterEditableChildObject(productCodes);
				}
				return productCodes;
			}
		}
		CusLineTariffDetailCollection<CusLineTariffDetail> productCodes;

		#endregion

		#region Notes

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get => MarksAndNumbersNote.Text;
			set => MarksAndNumbersNote.SetNoteText(this, MarksAndNumbersInfo, value);
		}

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(Schema.MarksAndNumbers);

		public int MarksAndNumbers_MaxLength
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && (declaration.JE_MessageType == MessageTypeCodeList.Codes.COO || !declaration.SG_ApplicationProductType.IsEmpty)
					? 170
					: 512;
			}
		}

		HiddenTextNote MarksAndNumbersNote => marksAndNumbersNote ?? (marksAndNumbersNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.SGMarksAndNumbers.Description));
		HiddenTextNote marksAndNumbersNote;

		#endregion

		#region CertItemDescription

		[MaxLength(Schema.CertItemDescriptionMaxLength)]
		public ZString CertItemDescription
		{
			get => CertItemDescriptionNote.Text;
			set => CertItemDescriptionNote.SetNoteText(this, CertItemDescriptionInfo, value, () => Validation.ValidateCertItemDescription());
		}

		public ZPropertyInfo CertItemDescriptionInfo => GetZPropertyInfo(Schema.CertItemDescription);

		HiddenTextNote CertItemDescriptionNote => certItemDescriptionNote ?? (certItemDescriptionNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.SGCertificateItemDesc.Description));
		HiddenTextNote certItemDescriptionNote;

		#endregion

		#endregion

		#region Validation

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			Customs.Business.JobComInvoiceLineValidation result = null;

			if (Declaration != null)
			{
				if (Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT)
				{
					result = new JobComInvoiceLineValidation_IPT(this);
				}
				else if (Declaration.JE_MessageType == MessageTypeCodeList.Codes.INP)
				{
					result = new JobComInvoiceLineValidation_INP(this);
				}
				else if (Declaration.JE_MessageType == MessageTypeCodeList.Codes.TNP)
				{
					result = new JobComInvoiceLineValidation_TNP(this);
				}
				else if (Declaration.IsOUTDEC)
				{
					result = this.Declaration.SG_ApplicationProductType != "" ? new JobComInvoiceLineValidation_OUTwCO(this) : new JobComInvoiceLineValidation_OUT(this);
				}
				else if (Declaration.JE_MessageType == MessageTypeCodeList.Codes.COO)
				{
					result = new JobComInvoiceLineCOValidation(this);
				}
				else
				{
					result = new JobComInvoiceLineValidation(this);
				}
			}
			else
			{
				result = new JobComInvoiceLineValidation(this);
			}

			return result;
		}

		#endregion

		#region Copy/Clone

		public IBusiness TemplateCopy()
		{
			return (JobComInvoiceLine)Clone();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			JobComInvoiceLine result = (JobComInvoiceLine)base.CloneInternal(args);
			using (GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				result.MarksAndNumbers = MarksAndNumbers;
				result.CertItemDescription = CertItemDescription;

				foreach (CusLineTariffDetail tariffDetail in ProductCodes)
				{
					result.ProductCodes.Add(tariffDetail.Clone());
				}

				foreach (CASCCode cascCode in CASCCode1s)
				{
					result.CASCCode1s.Add(cascCode.Clone());
				}

				foreach (CASCCode cascCode in CASCCode2s)
				{
					result.CASCCode2s.Add(cascCode.Clone());
				}

				foreach (CASCCode cascCode in CASCCode3s)
				{
					result.CASCCode3s.Add(cascCode.Clone());
				}

				ResetValuesOnClone(result);
			}

			return result;
		}

		protected void ResetValuesOnClone(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.SG_InwardHAWB = string.Empty;
			invoiceLine.SG_InwardMAWB = string.Empty;
			invoiceLine.SG_OutwardHAWB = string.Empty;
			invoiceLine.JI_OutwardMAWB = string.Empty;

			invoiceLine.SG_RefundForItemCustomsDutyAmount = 0m;
			invoiceLine.SG_RefundForItemExciseAmount = 0m;
			invoiceLine.SG_RefundForItemGSTAmount = 0m;
		}

		#endregion

		#region Fetch Hints

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceLineFetchStrategy(this);

		class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
		{
			public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, BusinessObject.PK);

				if (!BusinessObject.JI_Tariff.IsEmpty)
				{
					var tariffQuery = Universal.TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem, BusinessObject.JI_Tariff, BusinessObject.EffectiveDateForDutyRate);
					Factory.AddFetchHint(typeof(TariffView), tariffQuery);
				}
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);

				if (!BusinessObject.JI_Tariff.IsEmpty)
				{
					var tariffQuery = Universal.TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem, BusinessObject.JI_Tariff, BusinessObject.EffectiveDateForDutyRate);
					Factory.AddFetchHint(typeof(TariffView), tariffQuery);
				}
			}
		}

		#endregion

		#region IChargeApportionee members

		void IChargeApportionee.CalculateAmountBasedOnPercentage(JobComInvCharge charge)
		{
			if (LinePriceRefCurrency != null)
			{
				ZDecimal amountCalculated = 0m;
				if (charge.J7_ChargeType == Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance)
				{
					var invoice = this.InvoiceHeader;
					var incoTermAndChargeFactory = invoice?.IncoTermAndChargeFactory;

					ZDecimal cFRAmount = 0m;
					if (incoTermAndChargeFactory != null && !invoice.IncoTerm.IsEmpty && incoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoice.IncoTerm, charge.ChargeCode))
					{
						cFRAmount = (JI_Calc_CIF / (100 + charge.J7_Percentage)) * 100;
					}
					else
					{
						cFRAmount = JI_Calc_FOB + JI_Calc_FreightInInvoiceCurr;
					}

					amountCalculated = charge.J7_Percentage > 0m ? cFRAmount * charge.J7_Percentage / 100m : 0m;
				}
				else if (charge.J7_ChargeType == Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight ||
						 charge.J7_ChargeType == Customs.Business.CustomsChargeTypeList.Codes.OtherCharges)
				{
					amountCalculated = charge.J7_Percentage > 0m ? JI_Calc_FOB * charge.J7_Percentage / 100m : 0m;
				}

				charge.J7_Amount = amountCalculated.Round(2);

				if (amountCalculated > 0m)
				{
					charge.J7_RX_NKCurrency = LinePriceRefCurrency.RX_Code;
				}
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.CASCode1, typeof(CASCCode1));
			result.Add(CusCodeDataTypeList.Codes.CASCode2, typeof(CASCCode2));
			result.Add(CusCodeDataTypeList.Codes.CASCode3, typeof(CASCCode3));
			return result;
		}

		#endregion

		public bool PreferenceRateApplies => JI_PrimaryPreference == PreferentialIndicatorCodeList.Codes.PRF || JI_PrimaryPreference == PreferentialIndicatorCodeList.Codes.PRI;

		ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> IAdditionalLineTariffDetailParent.CusLineTariffDetails => ProductCodes;

		protected override IZZRateSelectionCriteria GetDutyRateSelectionCriteriaCore() => new RateSelectionCriteria(this, Constants.RateTypes.Duty, Constants.RateTypes.Duty);

		public class RateSelectionCriteria : RateSelectionCriteria<JobComInvoiceLine>
		{
			public RateSelectionCriteria(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode)
				: base(invoiceLine, rateType, rateCode)
			{
			}

			protected override ZDateTime GetEffectiveDate(JobComInvoiceLine invoiceLine) => invoiceLine.EffectiveDateForDutyRate;
		}
	}
}
