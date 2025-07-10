using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[DebuggerDisplay("{TM_Type}-{TM_RelevantValue}")]
	[ProvideMetaDataProperty("PropertyReadOnly", MetaDataTypes.ReadOnly)]
	public class RateLineItem : AutoRateLineItems, IGetZPropertyInfo, ISupportDataImporting, INotifyPropertyUpdated, IRateLineItem
	{
		public RateLineItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!factory.IsLoading && !(this is DummyRateLineItem) && !factory.IsConstructingNullBusinessObject)
			{
				EnsureCreatedByRateLineItemsCollection(row);
			}
		}

		void EnsureCreatedByRateLineItemsCollection(DataRow row)
		{
			if (!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(Factory) && !row.IsInConstruction(RateLineItemsSchema.PK, Factory))
			{
				ErrorReporter.ReportOnce("RateLineItem.EnsureCreatedByRateLineItemsCollection", "RateLineItem is not being created by parent collection");
			}
		}

		#region Schema

		new public abstract class Schema : AutoRateLineItems.Schema
		{
			public const string TM_RelevantValue = "TM_RelevantValue";
			public const string CalculationOrderOrPercentOf = "CalculationOrderOrPercentOf";
			public const string CalculationOrderOrPercentOfFieldType = "CalculationOrderOrPercentOfFieldType";
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return false;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(RateLineItemsSchema.Constants.TM_TL);
			return result;
		}

		public RateLineItem Clone(RowFactory rowFactory)
		{
			var clone = rowFactory.New(TableName);
			RateLineItem clonedObject;

			using (clone.MarkAsInConstruction(RateLineItemsSchema.PK, Factory))
			{
				this.CopyToDataRow(rowFactory, clone, GetPropertiesThatShouldNotBeCopied());
				clonedObject = new RateLineItem(Factory, clone);
			}

			return clonedObject;
		}

		#endregion

		#region Properties

		#region TM_AC

		[List("Lookups.ChargeCodes")]
		public override ZGuid TM_AC
		{
			get { return base.TM_AC; }
			set
			{
				if (Parent?.Parent != null && base.TM_AC != value)
				{
					Parent.Parent.InvalidateGroupValidation();
				}
				base.TM_AC = value;
			}
		}

		#endregion

		#region ChargeCodeDescription

		public ZString ChargeCodeDescription
		{
			get => ChargeCode?.AC_DescMultilingual ?? string.Empty;
		}

		public ZPropertyInfo ChargeCodeDescriptionInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ChargeCodeDescription)); }
		}

		#endregion

		#region CalculationOrder Or PercentOf

		[List("Lookups+ChargeCodes")]
		public ZString CalculationOrderOrPercentOf
		{
			get
			{
				if (this.RateOperatorIsApplyTo() && TM_Text == CalculatorConstants.Text.ChargeCode)
				{
					return TM_AC.ToString();
				}

				if (this.RateOperatorIsCalculationOrder())
				{
					return TM_Value.ToZInt().ToString();
				}

				return calculationOrderOrPercentOfInvalid;
			}
			set
			{
				CheckMaximumLength(CalculationOrderOrPercentOfInfo, value);

				if (this.RateOperatorIsApplyTo() && TM_Text == CalculatorConstants.Text.ChargeCode)
				{
					Guid calcPercent;
					if (Guid.TryParse(value, out calcPercent))
					{
						TM_AC = calcPercent;
					}
					else
					{
						calculationOrderOrPercentOfInvalid = value;
					}
				}
				else if (this.RateOperatorIsCalculationOrder())
				{
					if (value.IsEmpty)
					{
						TM_Value = ZDecimal.Zero;
					}
					else
					{
						ZInt calcOrder;
						if (ZInt.TryParse(value, out calcOrder))
						{
							TM_Value = (ZDecimal)calcOrder;
						}
						else
						{
							calculationOrderOrPercentOfInvalid = value;
						}
					}
				}
				else
				{
					calculationOrderOrPercentOfInvalid = value;
				}

				CalculationOrderOrPercentOfInfo.RefreshBinding();
			}
		}
		ZString calculationOrderOrPercentOfInvalid;

		public ZPropertyInfo CalculationOrderOrPercentOfInfo
		{
			get { return GetZPropertyInfo(nameof(CalculationOrderOrPercentOf)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required property for UI Components")]
		int CalculationOrderOrPercentOf_MaxLength
		{
			get { return this.RateOperatorIsApplyTo() && TM_Text == CalculatorConstants.Text.ChargeCode ? AccChargeCodeSchema.PK.MaxLength : 2; }
		}

		public ZString CalculationOrderOrPercentOfFieldType
		{
			get { return this.RateOperatorIsApplyTo() && TM_Text == CalculatorConstants.Text.ChargeCode ? nameof(FieldType.Guid) : nameof(FieldType.Text); }
		}

		#endregion

		#region TM_BreakWeightVolume

		[List("Lookups.WeightVolumes")]
		public override ZString TM_BreakWeightVolume
		{
			get { return base.TM_BreakWeightVolume; }
			set
			{
				base.TM_BreakWeightVolume = value;

				if (this.Calculator() != null)
				{
					this.Calculator().NotifyChanged(this);
				}
			}
		}

		#endregion

		#region TM_UnitMultiple

		public override ZInt TM_UnitMultiple
		{
			get { return base.TM_UnitMultiple; }
			set
			{
				base.TM_UnitMultiple = value;

				if (this.Calculator() != null)
				{
					this.Calculator().NotifyChanged(this);
				}
			}
		}

		[List("Lookups.UnitMultiples")]
		[MaxLength(8)]
		public ZString UnitMultipleString
		{
			get => this.GetUnitMultipleString(unitMultipleString);
			set
			{
				CheckMaximumLength(UnitMultipleStringInfo, value);
				unitMultipleString = value;

				try
				{
					TM_UnitMultiple = int.Parse(value);
					unitMultipleString = ZString.Empty;
				}
				catch (OverflowException)
				{
					TM_UnitMultiple = 1;
				}
				catch (FormatException)
				{
					TM_UnitMultiple = 1;
				}

				UnitMultipleStringInfo.RefreshBinding();
			}
		}
		ZString unitMultipleString;

		public ZPropertyInfo UnitMultipleStringInfo
		{
			[DebuggerStepThrough]
			get { return GetZPropertyInfo(nameof(UnitMultipleString)); }
		}

		#endregion

		#region TM_Type

		[List("Lookups.WeightBreaks")]
		public override ZString TM_Type
		{
			get { return base.TM_Type; }
			set
			{
				if (base.TM_Type != value)
				{
					base.TM_Type = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateTM_Break();
					}

					if (!IsImporting)
					{
						ClearReadOnlyFields();
						NotifyPropertyUpdated(RateLineItem.Schema.CalculationOrderOrPercentOfFieldType);

						if (this.Calculator() != null)
						{
							this.Calculator().NotifyChanged(this);
						}
					}
				}
			}
		}

		public bool HasBeenFoundAsTM_TypeDuplicateByAnotherValidation { get; set; }

		#endregion

		#region TM_FlatAmount

		[DecimalPlaces("NumberOfDecimalPlacesForRates")]
		public override ZDecimal TM_FlatAmount
		{
			get { return GetRoundedAndDiscounted(base.TM_FlatAmount); }
			set { base.TM_FlatAmount = value; }
		}

		public int NumberOfDecimalPlacesForRates
		{
			get { return Parent?.DecimalPlaces() ?? 4; }
		}

		#endregion

		#region TM_Value

		[BusinessObjectTestExclude]
		[DecimalPlaces("NumberOfDecimalPlacesForRates")]
		public override ZDecimal TM_Value
		{
			get { return GetRoundedAndDiscounted(base.TM_Value); }
			set
			{
				base.TM_Value = value;
				CascadeBreakAmounts(false);
			}
		}

		ZDecimal GetRoundedAndDiscounted(ZDecimal value)
		{
			var canBeDiscounted = !value.IsEmpty
				&& Parent != null
				&& Parent.TL_CompanyTariffLevel <= 1
				&& Parent.TL_FeeChargeType.IsEmpty
				&& Parent.Header != null
				&& Parent.Header.IsTariff()
				&& Parent.Header.TH_GlobalRateLevel > 1;

			value = GetRoundedValue(value);

			if (canBeDiscounted)
			{
				if (this.Calculator() != null && !(this.Calculator() is NullCalculator) && this.Calculator().ShouldValueBeDiscounted(this))
				{
					ZDecimal discount = 1 - Parent.CompanyTariffDiscount / 100;

					if (discount != 1m)
					{
						return value * discount;
					}
				}
			}

			return value;
		}

		ZDecimal GetRoundedValue(ZDecimal value)
		{
			var roundingMode = RatingRoundingTypes.DefaultFromRegistry;
			return DefaultNumberOfDecimals.GetRoundedValue(value, roundingMode, NumberOfDecimalPlacesForRates);
		}

		bool ShouldCascadeBreakAmounts
		{
			get
			{
				var result = this.RequiresWeightBreak();
				result = result && !Parent.ViewResults;
				result = result && !IsInDatabase;
				result = result && !IsCopying;
				result = result && !((ISupportDataImporting)this).IsImportingData;
				result = result && !(Parent.Parent != null && Parent.Parent.Parent is CompanyTariff);
				result = result && !Parent.IsCloneForViewResults;
				result = result && !Parent.Uses(CalculatorType.Equalization);

				return result;
			}
		}

		void CascadeBreakAmounts(bool isAgentAmount)
		{
			if (!ShouldCascadeBreakAmounts)
			{
				return;
			}

			var filter = new ZQuery();

			if (Parent.RateCalculatorType == CalculatorType.CartageZoneDistance)
			{
				if (Parent.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones)
				{
					filter.AddToFilter(RateLineItemsSchema.TM_F1Zone, SQLComparisonOperator.Equal, TM_F1Zone);
				}
				else
				{
					filter.AddToFilter(RateLineItemsSchema.TM_TZ_DomesticZone, SQLComparisonOperator.Equal, TM_TZ_DomesticZone);
				}
			}

			var lineItems = Parent.RateLineItems.Find(filter);

			foreach (RateLineItem lineItem in lineItems)
			{
				if (lineItem.LocalLineOrder > LocalLineOrder && lineItem.RateOperatorIsPlus())
				{
					if (isAgentAmount)
					{
						lineItem.TM_AgentDeclaredRate = TM_AgentDeclaredRate;
					}
					else
					{
						lineItem.TM_Value = TM_Value;
					}
				}
			}
		}

		#endregion

		#region  TM_Break

		/// <summary>
		/// If the RateLineItem is a MIN, you cannot set the break to non-zero
		/// unless there's a Minus or Plus RateLineItem existing for the RateLine
		/// already
		/// </summary>
		public override ZDecimal TM_Break
		{
			get { return TM_BreakOverride > 0 ? TM_BreakOverride : base.TM_Break; }
			set
			{
				if (TM_Break != value)
				{
					if (IsImporting)
					{
						base.TM_Break = value;
					}
					else
					{
						var wasLowestPlus = this.IsLowestPlus();

						base.TM_Break = value;

						if (isSetWeightBreakEnabled)
						{
							isSetWeightBreakEnabled = false;
							SetWeightBreakOnMinusOrPlus(TM_Break, wasLowestPlus);
						}
						isSetWeightBreakEnabled = true;
					}
				}
			}
		}

		bool isSetWeightBreakEnabled = true;

		void SetWeightBreakOnMinusOrPlus(ZDecimal breakValue, bool shouldCheckForLowestPlus)
		{
			if (Parent == null)
			{
				return;
			}

			IRateLineItem item = null;

			if (this.RateOperatorIsMinus())
			{
				item = RateLineItemsFromSameGroup.FirstOrDefault(x => x.IsLowestPlus());
			}
			else if (this.IsLowestPlus() || shouldCheckForLowestPlus)
			{
				item = RateLineItemsFromSameGroup.FirstOrDefault(x => x.RateOperatorIsMinus());
			}

			if (shouldCheckForLowestPlus && this.RateOperatorIsPlus())
			{
				IRateLineItem newLowestPlus = null;
				if (!this.IsLowestPlus())
				{
					newLowestPlus = RateLineItemsFromSameGroup.FirstOrDefault(x => x.IsLowestPlus());
				}
				else if (breakValue == -1m)
				{
					newLowestPlus = RateLineItemsFromSameGroup.FirstOrDefault(x => x.TM_Break > TM_Break && x.RateOperatorIsPlus());
				}

				if (newLowestPlus != null)
				{
					breakValue = newLowestPlus.TM_Break;
				}
			}

			var rateLineItem = item as RateLineItem;

			if (rateLineItem != null && breakValue != -1m)
			{
				rateLineItem.TM_Break = breakValue;
			}
		}

		#endregion

		#region TM_BreakMinimum

		[DecimalPlaces("NumberOfDecimalPlacesForRates")]
		public override ZDecimal TM_BreakMinimum
		{
			get
			{
				return GetRoundedValue(base.TM_BreakMinimum);
			}
			set
			{
				base.TM_BreakMinimum = value;
			}
		}

		#endregion

		#region TM_CallForPricing

		public override ZBool TM_CallForPricing
		{
			get { return base.TM_CallForPricing; }
			set
			{
				base.TM_CallForPricing = value;
				ClearReadOnlyFields();
			}
		}

		#endregion

		#region TM_Text

		[List("Lookups.EquipmentTypes")]
		public override ZString TM_Text
		{
			get { return base.TM_Text; }
			set
			{
				base.TM_Text = value;

				switch (TM_Type)
				{
					case CalculatorConstants.Type.ApplyTo:
						ClearReadOnlyFields();
						break;

					case CombinedCalculator.Items.UseAccumulated:
						ClearAllReadOnlyFields(Parent.RateLineItems);
						break;
					case AgencyCalculator.Items.AgencyFeeType:
						ClearAllReadOnlyFields(Parent.RateLineItems);
						Parent.GetCalculator<AgencyCalculator>()?.AdditionalRateInfo.RefreshBinding();
						Parent.GetCalculator<AgencyCalculator>()?.IncludedHeadersInfo.RefreshBinding();
						break;
					case AgencyCalculator.Items.AgencyLineType:
						ClearAllReadOnlyFields(Parent.RateLineItems);
						Parent.GetCalculator<AgencyCalculator>()?.IncludedLinesInfo.RefreshBinding();
						Parent.GetCalculator<AgencyCalculator>()?.MaximumLinesInfo.RefreshBinding();
						Parent.GetCalculator<AgencyCalculator>()?.PerAdditionalLineInfo.RefreshBinding();
						break;
				}

				if (!IsValidationSuspended && Parent != null && Parent.Parent != null)
				{
					Parent.Parent.MarkAsNeedingValidationForRateLineChange();
				}

				NotifyPropertyUpdated(RateLineItem.Schema.CalculationOrderOrPercentOfFieldType);
			}
		}

		void ClearAllReadOnlyFields(RateLineItemsCollection items)
		{
			foreach (var item in items.Cast<RateLineItem>())
			{
				item.ClearReadOnlyFields();
			}
		}

		#endregion

		#region TM_TL

		[RelatedBusinessObject("Parent")]
		public override ZGuid TM_TL
		{
			get { return base.TM_TL; }
			set
			{
				if (!this.IsRemovingFromRelationship && value.IsEmpty && !this.IsInDeletion() && !(this is DummyRateLineItem))
				{
					ErrorReporter.ReportOnce("RateLineItem_TM_TL", string.Format(CultureInfo.CurrentCulture, "value:{0},RemoveRelationship:{1}", value, IsRemovingFromRelationship)); // To display the Column name in the key
				}
				parent = null;
				base.TM_TL = value;
			}
		}

		#endregion

		#region TM_AgentDeclaredRate

		[BusinessObjectTestExclude()]
		[DecimalPlaces("NumberOfDecimalPlacesForRates")]
		public override ZDecimal TM_AgentDeclaredRate
		{
			get { return GetRoundedAndDiscounted(base.TM_AgentDeclaredRate); }
			set
			{
				base.TM_AgentDeclaredRate = value;
				CascadeBreakAmounts(true);
			}
		}

		#endregion

		#region TM_RelevantValue

		[DecimalPlaces("NumberOfDecimalPlacesForRates")]
		public ZDecimal TM_RelevantValue
		{
			get { return ViewAgentRates ? TM_AgentDeclaredRate : TM_Value; }
			set
			{
				if (ViewAgentRates)
				{
					TM_AgentDeclaredRate = value;
				}
				else
				{
					TM_Value = value;
				}
			}
		}

		public ZWrappedPropertyInfo TM_RelevantValueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.TM_RelevantValue, x => (ViewAgentRates ? TM_AgentDeclaredRateInfo : TM_ValueInfo)); }
		}

		bool ViewAgentRates
		{
			get { return Parent != null && Parent.ViewAgentRates; }
		}

		#endregion

		#region Apply To Description

		[MaxLength(30)]
		public ZString ApplyToDescription
		{
			get
			{
				ZString result = "";
				if (Parent.Uses(CalculatorType.Percentage) || Parent.Uses(CalculatorType.PercentageBreaks) || Parent.Uses(CalculatorType.DisbursementInterest))
				{
					if (Lookups.PercentageApplyToList.ContainsCode(TM_Text))
					{
						result = Lookups.PercentageApplyToList.GetDescriptionFromCode(TM_Text);
					}
				}
				return result;
			}
		}

		public ZPropertyInfo ApplyToDescriptionInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ApplyToDescription)); }
		}

		#endregion

		#region Profit Share / Rebate Description

		[MaxLength(30)]
		public ZString ProfitShareRebateApplyToDescription
		{
			get
			{
				var description = Parent.Uses(CalculatorType.ProfitShareRebate) && Lookups.ProfitShareRebateApplyToList.ContainsCode(TM_Text)
					? Lookups.ProfitShareRebateApplyToList.GetDescriptionFromCode(TM_Text)
					: "";

				return description;
			}
		}

		public ZPropertyInfo ProfitShareRebateApplyToDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ProfitShareRebateApplyToDescription)); }
		}

		#endregion

		#region HousebillReleaseTypeDesc

		[MaxLength(1024)]
		public ZString HousebillReleaseTypeDesc
		{
			get
			{
				ZString result = "";
				if (Parent != null && Parent.Uses(CalculatorType.HousebillReleaseType))
				{
					if (Lookups.HousebillReleaseTypes.ContainsCode(TM_Type))
					{
						result = Lookups.HousebillReleaseTypes.GetDescriptionFromCode(TM_Type);
					}
				}
				return result;
			}
		}

		public ZPropertyInfo HousebillReleaseTypeDescInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(HousebillReleaseTypeDesc)); }
		}

		#endregion

		#region WarehousePackageTypeDesc

		[MaxLength(1024)]
		public ZString WarehousePackageTypeDesc
		{
			get
			{
				ZString result = "";
				if (Parent != null && Parent.Uses(CalculatorType.WarehousePack))
				{
					if (Lookups.WarehousePackagesTypes.ContainsCode(TM_Type))
					{
						result = Lookups.WarehousePackagesTypes.GetDescriptionFromCode(TM_Type);
					}
				}
				return result;
			}
		}

		public ZPropertyInfo WarehousePackageTypeDescInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(WarehousePackageTypeDesc)); }
		}

		#endregion

		#region WarehouseLocationTypeDesc

		[MaxLength(1024)]
		public ZString WarehouseLocationTypeDesc
		{
			get
			{
				ZString result = "";
				if (Parent != null && Parent.Uses(CalculatorType.WarehouseLocationType))
				{
					foreach (BusinessObject locationType in (IEnumerable<BusinessObject>)Lookups.WarehouseLocationTypes)
					{
						if (locationType[WhsLocationTypeSchema.WLT_Code].ToString() == TM_Type.ToString())
						{
							result = locationType[WhsLocationTypeSchema.WLT_Description].ToString();
						}
					}
				}
				return result;
			}
		}

		public ZPropertyInfo WarehouseLocationTypeDescInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(WarehouseLocationTypeDesc)); }
		}

		#endregion

		#region EquipmentTypeDesc

		[MaxLength(1024)]
		public virtual ZString EquipmentTypeDesc
		{
			get
			{
				ZString result = "";
				if (Parent != null && Parent.Uses(CalculatorType.EquipmentHire))
				{
					if (Lookups.EquipmentTypes.ContainsCode(TM_Text))
					{
						result = Lookups.EquipmentTypes.GetDescriptionFromCode(TM_Text);
					}
				}
				return result;
			}
		}

		public ZPropertyInfo EquipmentTypeDescInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(EquipmentTypeDesc)); }
		}

		#endregion

		#region GetPropertyReadOnly / ClearFields

		protected bool GetPropertyReadOnly(PropertyDescriptor property)
		{
			var result = false;
			if (!IsDeleted && Parent != null && this.Calculator() != null)
			{
				switch (property.Name)
				{
					case Schema.CalculationOrderOrPercentOf:
						result = !(this.RateOperatorIsApplyTo() && TM_Text == CalculatorConstants.Text.ChargeCode || this.RateOperatorIsCalculationOrder());
						break;

					case Schema.TM_Value:
					case Schema.TM_AgentDeclaredRate:
						result = TM_CallForPricing || this.Calculator().ValueIsReadOnly(this);
						break;

					case Schema.TM_RelevantValue:
						result = TM_CallForPricing || this.Calculator().ValueIsReadOnly(this);
						if (Parent.Uses(CalculatorType.PercentageBreaks))
						{
							result = result || this.RequiresWeightBreak();
						}
						break;

					case Schema.TM_FlatAmount:
						if (Parent.Uses(CalculatorType.PercentageBreaks))
						{
							result = !this.RequiresWeightBreak();
						}
						else if (Parent.Uses(CalculatorType.HighestRate))
						{
							result = this.RateOperatorIsMIN();
						}
						else
						{
							result = !this.RequiresWeightBreak() || TM_CallForPricing;
						}
						break;

					case Schema.TM_BreakHourRate:
						result = Parent.Uses(CalculatorType.CombinedWithIncrement) && !this.IsLowestBreak();
						break;

					case Schema.TM_Break:
						result = !Parent.Calculator.TM_BreakEditable(this);
						break;

					case Schema.TM_BreakMinimum:
						if (Parent.Uses(CalculatorType.PercentageBreaks))
						{
							result = !this.RequiresWeightBreak();
						}
						else
						{
							result = !this.RequiresWeightBreak() || TM_CallForPricing || Parent.IsAccumulated;
						}
						break;

					case Schema.TM_BreakWeightVolume:
						result = IsBreakWeightVolumeReadOnly();
						break;

					case Schema.TM_CallForPricing:
						if (CalculatorHasCallForPricing)
						{
							result = !this.RateOperatorIsPlus() || this.IsLowestBreak();
						}
						break;

					case Schema.TM_Text:
						if (this.RateOperatorIsEquipmentType())
						{
							result = !this.Calculator().ShowEquipmentType;
						}
						else if ((this.RateOperatorIsUseAccumulated() || this.RateOperatorIsHigherChargeableLowerRate()) && Parent.IsBreaksPerApplicable() && !Parent.Calculator.BreaksPer.IsEmpty)
						{
							//We need to make Cumulative Breaks and Higher Break Lower Rate checkboxes readonly if Break Per value is not empty
							result = true;
						}
						else if (!this.RateOperatorIsNonPrintedFlag() && IsSlidingOperatorType && CalculatorHasCallForPricing)
						{
							result = !(this.RateOperatorIsPlus() && TM_CallForPricing);
						}
						else if (this.RateOperatorIsBreaksPer())
						{
							result = !Parent.IsBreaksPerApplicable();
						}
						break;

					case Schema.TM_AC:
						if (this.RateOperatorIsApplyTo())
						{
							result = TM_Text != CalculatorConstants.Text.ChargeCode;
						}
						break;
				}
			}

			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		bool IsSlidingOperatorType
		{
			get
			{
				return TM_Type.IsEmpty
					|| TM_Type == Calculator.Items.Operator.Plus
					|| TM_Type == Calculator.Items.Operator.Minus
					|| TM_Type == Calculator.Items.Operator.BAS
					|| TM_Type == Calculator.Items.Operator.MAX
					|| TM_Type == Calculator.Items.Operator.MIN
					|| TM_Type == Calculator.Items.Operator.UNT;
			}
		}

		bool CalculatorHasCallForPricing
		{
			get
			{
				return Parent.Uses(CalculatorType.Combined)
					|| Parent.Uses(CalculatorType.Cartage)
					|| (Parent.Uses(CalculatorType.CartageZoneDistance) && !((CartageZoneDistanceCalculator)Parent.Calculator).UseACIZones)
					|| (Parent.UsesCompanyTariffOrCostBasedCalculator() && Parent.Calculator.IsSliding())
					|| Parent.Uses(CalculatorType.PercentageBreaks)
					|| Parent.Uses(CalculatorType.Time)
					|| Parent.Uses(CalculatorType.Equalization)
					|| Parent.Uses(CalculatorType.ValueRange)
					|| Parent.Uses(CalculatorType.SplitMonthBilling);
			}
		}

		bool IsBreakWeightVolumeReadOnly()
		{
			var result = false;
			var calculator = this.Calculator();

			if (!calculator.IsBreakUnitAvailable)
			{
				result = true;
			}
			else if (Parent.Uses(CalculatorType.Time))
			{
				result = !this.RateOperatorIsUNT() && !this.IsLowestBreak();
			}
			else if (calculator is BaseCombinedCalculator)
			{
				result = !this.IsLowestBreak();
			}
			else if (Parent.Uses(CalculatorType.HighestRate))
			{
				result = !this.RateOperatorIsUNT();
			}

			if (result)
			{
				TM_BreakWeightVolumeInfo.ClearValue();
			}

			return result;
		}

		internal void ClearReadOnlyFields()
		{
			if (Parent != null && !Parent.IsCloneForViewResults && !Parent.IsTariffLineInherited)
			{
				foreach (var column in RateLineItemsSchema.All)
				{
					var columnName = column.Name;
					if (ZPropertyInfoHash.ContainsKey(columnName) && ZPropertyInfoHash[columnName].ReadOnly)
					{
						if (column != RateLineItemsSchema.TM_TL)
						{
							var info = GetZPropertyInfo(columnName);
							if (info.PropertyType == typeof(ZBool))
							{
								if ((ZBool)info.Value)
								{
									info.Value = ZBool.False;
								}
							}
							else if (!info.Value.IsEmpty && !IsPivotForEqualization(columnName))
							{
								info.ClearValue();
							}
						}
						else
						{
							ErrorReporter.ReportOnce("RateLineItem.ClearReadOnlyFields", "TM_TL is not supposed to be ReadOnly but it is");
						}
					}
				}
			}
		}

		bool IsPivotForEqualization(ZString columnName)
		{
			return Parent != null && Parent.Uses(CalculatorType.Equalization) && columnName == RateLineItemsSchema.Constants.TM_Break;
		}

		#endregion

		#region LocalLineOrder

		public ZByte LocalLineOrder { get; set; }

		#endregion

		#endregion

		#region Events

		public event EventHandler<PropertyUpdatedEventArgs> PropertyUpdated;

		#endregion

		#region Update Standard / Agent Rate Value

		public delegate ZDecimal GetUpdatedValue(ZDecimal originalValue);

		public enum RateTypeToUpdate
		{
			Relevant,
			Standard,
			Agent,
			StandardAndAgent
		}

		public void UpdateRateValue(GetUpdatedValue getUpdatedValue, RateTypeToUpdate rateTypeToUpdate)
		{
			if (rateTypeToUpdate == RateTypeToUpdate.Relevant)
			{
				TM_RelevantValue = getUpdatedValue(TM_RelevantValue);
			}

			if (rateTypeToUpdate == RateTypeToUpdate.Standard || rateTypeToUpdate == RateTypeToUpdate.StandardAndAgent)
			{
				TM_Value = getUpdatedValue(TM_Value);
			}

			if (rateTypeToUpdate == RateTypeToUpdate.Agent || rateTypeToUpdate == RateTypeToUpdate.StandardAndAgent)
			{
				TM_AgentDeclaredRate = getUpdatedValue(TM_AgentDeclaredRate);
			}
		}

		#endregion

		#region Parent Rate Line

		public virtual RateLine Parent
		{
			get { return parent ?? (parent = Factory.Load<RateLine>(TM_TL)); }
		}
		RateLine parent;

		#endregion

		#region ParentZone / ParentZoneItems

		public CartageZone ParentZone
		{
			get
			{
				if (fParentZone == null)
				{
					foreach (var parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						var mapperView = parentCollection as RateLineItemsView;
						if (mapperView != null && mapperView.CartageZone != null)
						{
							fParentZone = mapperView.CartageZone;
							break;
						}
					}
				}

				return fParentZone;
			}
		}

		CartageZone fParentZone;

		public IEnumerable<IRateLineItem> RateLineItemsFromSameGroup => ParentZone != null
			? ParentZone.ZoneRateLineItems.Cast<RateLineItem>()
			: ParentRateLine.ChildRateLineItems;

		#endregion

		#region Event Logging

		public override void OnSaving()
		{
			base.OnSaving();
			if (Parent != null && Parent.Parent != null)
			{
				Parent.Parent.AddLog();
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			if (IsDeleted)
			{
				return;
			}

			base.BeforeSuccessfulDelete();

			if (IsInDatabase && Parent != null && !Parent.IsDeleted && Parent.Parent != null && !Parent.Parent.IsDeleted)
			{
				Parent.Parent.AddLog();
			}
		}

		public override void Delete()
		{
			if (IsDeleted)
			{
				return;
			}

			if (Parent != null
				&& !Parent.IsDeleted
				&& this.Calculator() != null
				&& this.IsLowestPlus()
				&& RateLineItemsFromSameGroup.Any(x => x.RateOperatorIsMinus()))
			{
				SetWeightBreakOnMinusOrPlus(-1m, true);
			}

			base.Delete();
		}

		#endregion

		#region IGetZPropertyInfo Members

		ZPropertyInfo IGetZPropertyInfo.GetZPropertyInfo(string propertyName)
		{
			switch (propertyName)
			{
				case Schema.TM_Value:
					return TM_ValueInfo;

				case Schema.TM_AgentDeclaredRate:
					return TM_AgentDeclaredRateInfo;

				case Schema.TM_RelevantValue:
					return TM_RelevantValueInfo;

				case Schema.TM_FlatAmount:
					return TM_FlatAmountInfo;

				case Schema.TM_Break:
					return TM_BreakInfo;

				case Schema.TM_BreakMinimum:
					return TM_BreakMinimumInfo;

				case Schema.TM_BreakWeightVolume:
					return TM_BreakWeightVolumeInfo;

				case Schema.TM_CallForPricing:
					return TM_CallForPricingInfo;

				case Schema.TM_Text:
					return TM_TextInfo;

				default:
					return GetZPropertyInfo(propertyName);
			}
		}

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData || (Parent != null && ((ISupportDataImporting)Parent).IsImportingData); }
			set { fIsImportingData = value; }
		}

		bool fIsImportingData;

		bool IsImporting
		{
			get { return ((ISupportDataImporting)this).IsImportingData; }
		}

		#endregion

		#region INotifyPropertyUpdated

		void NotifyPropertyUpdated(string propertyName)
		{
			var handler = PropertyUpdated;
			if (handler != null)
			{
				handler(this, new PropertyUpdatedEventArgs(propertyName));
			}
		}

		#endregion

		#region Overrides

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#endregion

		public string InvalidReason => string.Empty;
		public IRateLine ParentRateLine
		{
			get { return Parent; }
		}

		public void OverrideBreak(ZDecimal breakValue)
		{
			TM_BreakOverride = breakValue;
		}

		public void ResetBreakToOriginalValue()
		{
			TM_BreakOverride = 0;
		}

		ZDecimal TM_BreakOverride;
	}
}

