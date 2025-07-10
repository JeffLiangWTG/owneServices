using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Rating.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[DebuggerDisplay("{ChargeCode?.AC_Code}-{" + RateLine.Schema.TL_RateCalculator + "}-{Parent?.Parent.TH_RateType}-{Parent?.Container?.RC_Code}-{Parent?.TI_RH_NKCommodityCode}")]
	[ProvideMetaDataProperty("PropertyReadOnly", MetaDataTypes.ReadOnly)]
	[SystemDefinedValues]
	public class RateLine : AutoRateLines, IBusinessObjectState, ISupportDataImporting, IHaveExpressionDescription, IRateLine, ICompanyFilterProviderContext
	{
		#region Schema

		public new abstract class Schema : AutoRateLines.Schema
		{
			public const string UseOnlyActualWeightMeasure = "UseOnlyActualWeightMeasure";
			public const string TL_ConversionFactorString = "TL_ConversionFactorString";
			public const string TL_ConversionFactorForCalculation = "TL_ConversionFactorForCalculation";
			public const string ChargeInformationNoteText = "ChargeInformationNoteText";
			public const string ChargeInternalNoteText = "ChargeInternalNoteText";
			public const string TL_OP_ProductNumber = "TL_OP_ProductNumber";
		}

		#endregion

		public RateLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!factory.IsLoading && !factory.IsConstructingNullBusinessObject)
			{
				EnsureCreatedByRateLineCollection(row);
			}
		}

		void EnsureCreatedByRateLineCollection(DataRow row)
		{
			if (!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(Factory) && !row.IsInConstruction(RateLinesSchema.PK, Factory))
			{
				ErrorReporter.ReportOnce("RateLine.EnsureCreatedByRateLineCollection", "RateLine is not being created by parent collection");
			}
		}

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TL_Rounding = RatingRoundingTypes.DefaultFromRegistry;
			overrideChargeDescription = false;
		}

		#endregion

		#region OnLoaded

		public override void OnLoaded()
		{
			base.OnLoaded();
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				CalculateOverrideChargeDescription();
				ResetConversionFactor(defaultWhenEmpty: false);
			}
		}

		internal void CalculateOverrideChargeDescription()
		{
			// Only TL_RateDesc indicate overrideChargeDescription because it has validation for empty string, while TL_RateDescLocal has no validation for empty string
			overrideChargeDescription = !base.TL_RateDesc.IsEmpty;
		}

		#endregion

		#region Company Tariff

		public virtual bool IsTariffLineInherited
		{
			get { return Parent != null && Parent.Parent != null && Parent.Parent.IsAdditionalTariff() && TL_CompanyTariffLevel != Parent.Parent.TH_GlobalRateLevel; }
		}

		public virtual ZDecimal CompanyTariffDiscount
		{
			get
			{
				return TL_FeeChargeType.IsEmpty && Header != null
					? Parent.GetCompanyTariffDiscount(Header)
					: (ZDecimal)0m;
			}
		}

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				if (IsTariffLineInherited)
				{
					value = true;
				}

				if (this is RelatedRateLine || fRateLineItems != null)
				{
					RateLineItems.SetReadOnlyIncludingChildren(value);
				}
				else
				{
					RateLineItemsReadOnlyOnLoad = value;
				}

				base.ReadOnly = value;
			}
		}

		#region IBusinessObjectState Members

		int readOnlyIndex;

		/// <summary>
		/// We are doing this as the method in BusinessObject is not virtual.
		/// We don't want this method to load all child objects and set their readonly status - this is expensive.
		/// Instead, we do it when the child collections are lazy loaded.
		/// </summary>
		void IBusinessObjectState.IncrementReadOnlyIncludingChildren()
		{
			if (readOnlyIndex == 0)
			{
				ReadOnly = true;
			}
			readOnlyIndex++;
		}

		void IBusinessObjectState.DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
			DecrementReadOnlyIncludingChildren(decrementToZero);
		}

		void DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
			if (readOnlyIndex > 0)
			{
				readOnlyIndex--;
			}
			if (decrementToZero || readOnlyIndex == 0)
			{
				ReadOnly = false;
			}
			if (decrementToZero && readOnlyIndex > 0)
			{
				DecrementReadOnlyIncludingChildren(true);
			}
		}

		#endregion

		public void SetReadOnlyExcludingChargeCode(bool value)
		{
			readOnlyExcludingChargeCode = value;
			SetReadOnlyExcludingSpecificProperty(RateLinesSchema.TL_AC.Name, value);
		}

		public void SetReadOnlyExcludingViewAgentRates(bool value)
		{
			readOnlyExcludingViewAgentRates = value;
			SetReadOnlyExcludingSpecificProperty(nameof(ViewAgentRates), value);
		}

		void SetReadOnlyExcludingSpecificProperty(string propertyName, bool value)
		{
			if (ReadOnlyProperties == null)
			{
				ReadOnlyProperties = new Dictionary<string, bool>();
			}

			foreach (ZPropertyInfo info in ZPropertyInfoHash)
			{
				if (info.HasSetter && info.Name != propertyName)
				{
					ReadOnlyProperties[info.Name] = value;
				}
			}
			RefreshBinding();
		}

		protected bool GetPropertyReadOnly(PropertyDescriptor property)
		{
			var result = false;

			if (readOnlyExcludingChargeCode)
			{
				result = property.Name != RateLinesSchema.TL_AC.Name ? !(property is WrappingPropertyDescriptor) : property.IsReadOnly;
			}
			else if (readOnlyExcludingViewAgentRates)
			{
				result = property.Name != "ViewAgentRates" ? !(property is WrappingPropertyDescriptor) : property.IsReadOnly;
			}

			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		Dictionary<string, bool> ReadOnlyProperties;
		bool readOnlyExcludingChargeCode;
		bool readOnlyExcludingViewAgentRates;

		bool IsPropertyReadOnly(string name)
		{
			bool result;
			return ReadOnlyProperties != null && ReadOnlyProperties.TryGetValue(name, out result) && ReadOnlyProperties[name];
		}

		#endregion

		#region Helper Properties / Methods

		public bool IsCartageCalculatorCode(ZString code)
		{
			return code == CartageCalculator.Code || code == CartageZoneDistanceCalculator.Code;
		}

		public bool IsFreightChargeCodeLine
		{
			get { return ChargeCode != null && ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight; }
		}

		internal bool IsFirstLineInCollection
		{
			get { return TL_LineOrder == 0; }
		}

		public bool IsJobLevelAvailable
		{
			get { return !IsCalculatorInitialized || Calculator.IsJobLevelAvailable; }
		}

		public bool IsAccumulated
		{
			get { return IsCalculatorInitialized && (bool)Calculator.IsAccumulated; }
		}

		public string DefaultWeightVolume
		{
			get { return IsCalculatorInitialized ? Calculator.DefaultWeightVolume : ""; }
		}

		public bool IsProductAllowed
		{
			get { return !IsCalculatorInitialized || Calculator.IsProductAllowed; }
		}

		#endregion

		#region Properties

		#region TL_ContainerOwnership

		[List("Lookups.ContainerOwnerships")]
		[ResourceStringData("RateLine|ContainerOwnership", Caption = "Container Ownership", ShortCaption = "Ownership", FullDescription = "Container Ownership. Can only be specified when units are CN")]
		public override ZString TL_ContainerOwnership
		{
			get => GetResultsRateLineValue(TL_ContainerOwnershipInfo, ZString.Empty, base.TL_ContainerOwnership);
			set
			{
				base.TL_ContainerOwnership = value;
				TL_ContainerOwnershipInfo.RefreshBinding();
			}
		}

		#endregion

		#region TL_Condition

		[List("Lookups.RateLineConditions")]
		public override ZString TL_Condition
		{
			get { return base.TL_Condition; }
			set
			{
				if (base.TL_Condition != value)
				{
					if (value != RateLineConditions.UserDefined)
					{
						TL_ConditionalExpression = ZString.Empty;
						base.TL_ConditionalExpressionDescription = ZString.Empty;
					}

					base.TL_Condition = value;

					Validation.ValidateTL_ConditionalExpression();
				}
			}
		}

		// It is readonly if it is not user defined, unless it already has a value.
		// This can occur during ADAW importing if the TL_ConditionalExpression is set before the
		// TL_Condition. Generally, the TL_Condition will also be set after which will either
		// clear the TL_ConditionalExpression or keep it (If it's non-UserDefined or is).
		// However, in the case where the user did not import the TL_Condition then a validation error occurs
		// and the user can edit the TL_ConditionalExpression to remove the text and pass the validation.
		protected bool TL_ConditionalExpression_ReadOnly
		{
			get { return TL_Condition != RateLineConditions.UserDefined && TL_ConditionalExpression.IsEmpty; }
		}

		void IHaveExpressionDescription.SetExpressionDescription(ZString description)
		{
			TL_ConditionalExpressionDescription = description;
		}

		protected bool TL_ConditionalExpressionDescription_ReadOnly
		{
			get { return TL_ConditionalExpression_ReadOnly || TL_ConditionalExpression.IsEmpty; }
		}

		#endregion

		#region TL_CompanyTariffLevel

		public override ZByte TL_CompanyTariffLevel
		{
			get
			{
				return Parent != null && Parent.IsCompanyTariff() && base.TL_CompanyTariffLevel == 0
					? (ZByte)1
					: base.TL_CompanyTariffLevel;
			}
		}

		#endregion

		#region TL_AC

		[List("Lookups.ChargeCodes")]
		public override ZGuid TL_AC
		{
			get { return base.TL_AC; }
			set
			{
				var newChargeCode = Factory.Load<AccChargeCode>(value);

				// When importing, typically through ADAW, there is no control in the order that
				// values are set. So, sometimes, it'll set the calculator for the RateLine
				// and before the ChargeCode. Then the ChargeCode's default calculator overrides
				// the previously set one. With this, we can avoid that from happening.
				var isImporting = (this as ISupportDataImporting).IsImportingData;

				if (newChargeCode != null && !LockCalculator && !isImporting)
				{
					TL_RateCalculator = newChargeCode.AC_RateCalculator;
				}

				if (Parent != null && base.TL_AC != value)
				{
					Parent.InvalidateGroupValidation();
				}

				base.TL_AC = value;

				if (this.Uses(CalculatorType.CombinedWithIncrement) && newChargeCode != null && PreviousChargeCodePK != newChargeCode.PK && !isImporting)
				{
					Calculator.NotifyChanged(this);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateTL_RateDesc();
				}

				SetActualPercentage(newChargeCode, PreviousChargeCodePK);
				PreviousChargeCodePK = TL_AC;
			}
		}

		void SetActualPercentage(AccChargeCode newChargeCode, ZGuid previousChargeCodePK)
		{
			if (newChargeCode != null && newChargeCode.PK != previousChargeCodePK)
			{
				if (newChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSStorage)
				{
					UseOnlyActualWeightMeasure = true;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateUseOnlyActualWeightMeasure();
				}
				UseOnlyActualWeightMeasureInfo.RefreshBinding();
			}
		}

		ZGuid PreviousChargeCodePK;

		#endregion

		#region TL_RateCalculator

		public bool RateCalculatorChanged
		{
			get { return fRateCalculatorChanged; }
			set { fRateCalculatorChanged = value; }
		}

		bool fRateCalculatorChanged;

		public CalculatorType RateCalculatorType
		{
			get
			{
				if (!rateCalculatorTypeIsInitialized)
				{
					rateCalculatorType = CalculatorTypeConverter.CodeToEnum(TL_RateCalculator);
					rateCalculatorTypeIsInitialized = true;
				}
				return rateCalculatorType;
			}
			set
			{
				if (rateCalculatorType != value)
				{
					rateCalculatorType = value;
					rateCalculatorTypeIsInitialized = true;
					var code = CalculatorTypeConverter.EnumToCode(value);
					SetRateCalculator(code);
				}
			}
		}
		CalculatorType rateCalculatorType;
		bool rateCalculatorTypeIsInitialized;

		[List("Lookups.RateCalculators")]
		public override ZString TL_RateCalculator
		{
			get { return base.TL_RateCalculator; }
			set
			{
				if (base.TL_RateCalculator != value)
				{
					rateCalculatorTypeIsInitialized = false;
					SetRateCalculator(value);

					if (Parent != null)
					{
						Parent.InvalidateGroupValidation();
					}
				}
			}
		}

		void SetRateCalculator(ZString value)
		{
			RateCalculatorChanged = true;
			try
			{
				base.TL_RateCalculator = value;
				calculator = CalculatorFactory.GetCalculator(this);
			}
			finally
			{
				RateCalculatorChanged = false;
			}

			ResetWeightVolumeIfNeeded();

			if (!((ISupportDataImporting)this).IsImportingData)
			{
				if (this.Uses(CalculatorType.Percentage))
				{
					TL_RX_NKCurrency = Parent.TI_RX_NKCurrency;
				}
				else if (this.UsesCompanyTariffOrCostBasedCalculator() && ResultsRateLine != null)
				{
					TL_RX_NKCurrency = ResultsRateLine.TL_RX_NKCurrency;
				}
				else if (this.Uses(CalculatorType.CartageZoneDistance))
				{
					Calculator.ReloadCartageZones();
				}

				ResetUnitFactorIfNeeded();

				if (!IsJobLevelAvailable)
				{
					TL_IsWhsJobLevelCharge = false;
				}

				if (!string.IsNullOrEmpty(DefaultWeightVolume))
				{
					TL_WeightVolume = DefaultWeightVolume;
				}

				if (!IsProductAllowed)
				{
					TL_OP_ProductNumber = ZGuid.Empty;
				}
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateTL_AC();
			}

			RateLineItems.MarkAsNeedingValidationIncludingChildren();

			if (Parent != null)
			{
				Parent.MarkAsNeedingValidation();
			}
		}

		internal void ResetWeightVolumeIfNeeded()
		{
			if (!this.RequiresWeightVolume())
			{
				UseOnlyActualWeightMeasure = false;
				TL_WeightVolumeMultiple = 0m;
				TL_WeightVolume = ZString.Empty;
				TL_Rounding = RatingRoundingTypes.DefaultFromRegistry;
			}
			else
			{
				TL_ActualPercentageInfo.RefreshBinding();
				UseOnlyActualWeightMeasureInfo.RefreshBinding();
			}

			ResetConversionFactor();
		}

		protected bool TL_RateCalculator_ReadOnly
		{
			get { return ViewResults || IsPropertyReadOnly(RateLinesSchema.TL_RateCalculator.Name) || !IsRateCalculatorOverrideAllowed; }
		}

		bool IsRateCalculatorOverrideAllowed
		{
			get { return Header == null || Header.RateCalculatorOverrideAllowed; }
		}

		public bool LockCalculator
		{
			get { return fLockCalculator; }
			set { fLockCalculator = value; }
		}

		bool fLockCalculator;

		#endregion

		#region TL_Rounding

		[List("Lookups.Roundings")]
		public override ZString TL_Rounding
		{
			get { return base.TL_Rounding; }
			set
			{
				var oldValue = base.TL_Rounding;
				base.TL_Rounding = value;

				if (oldValue != value)
				{
					if (RateLineHelper.RequiresChargeableFromJob(this) && !Calculator.BreaksPer.IsEmpty)
					{
						Calculator.BreaksPer = ZString.Empty;
						Calculator.BreaksPerInfo.RefreshBinding();
					}
				}

				TL_RoundingInfo.RefreshBinding();
				TL_RoundingFactorInfo.RefreshBinding();
				RoundingFactorInfo.RefreshBinding();
			}
		}

		#endregion

		#region ConversionFactor

		bool isConversionFactorPendingUpdate;

		[Obsolete("The property is used only for DB purposes. Use ConversionFactor instead.")]
		public new ZDecimal TL_ConversionFactor
		{
			get { return base.TL_ConversionFactor; }
			set {
				isConversionFactorPendingUpdate = true;
				base.TL_ConversionFactor = value;
			}
		}

		[Obsolete("The property is used only for DB purposes. Use ConversionFactor instead.")]
		public new ZString TL_FactorNumerator
		{
			get { return base.TL_FactorNumerator; }
			set {
				isConversionFactorPendingUpdate = true;
				base.TL_FactorNumerator = value;
			}
		}

		[Obsolete("The property is used only for DB purposes. Use ConversionFactor instead.")]
		public new ZString TL_FactorDenominator
		{
			get { return base.TL_FactorDenominator; }
			set {
				isConversionFactorPendingUpdate = true;
				base.TL_FactorDenominator = value;
			}
		}

		/// <summary>
		///		Used by calculators as the don't support nested binding.
		/// </summary>
		[BusinessObjectTestExclude]
		[List("ConversionFactorForBinding.Lookups.ConversionFactors")]
		public virtual ZString TL_ConversionFactorString
		{
			get
			{
				return ConversionFactorForBinding.ConversionFactorString;
			}
			set
			{
				ConversionFactorForBinding.ConversionFactorString = value;
			}
		}

		public ZPropertyInfo TL_ConversionFactorStringInfo
		{
			[DebuggerStepThrough]
			get { return GetWrappedZPropertyInfo(Schema.TL_ConversionFactorString, (s) => ConversionFactorForBinding.ConversionFactorStringInfo); }
		}

		public ConversionFactorViewModel ConversionFactorForBinding
		{
			get
			{
				if (conversionFactorForBinding == null)
				{
					conversionFactorForBinding = new ConversionFactorViewModel(
						p => new RateLineConversionFactorLookups(p, this),
						p => new RateLineConversionFactorValidation(p, this));

					conversionFactorForBinding.ConversionFactor = new ConversionFactor(base.TL_ConversionFactor, base.TL_FactorNumerator, base.TL_FactorDenominator);

					conversionFactorForBinding.ConversionFactorStringInfo.ValueChanged += ConversionFactorChanged;
				}

				return conversionFactorForBinding;
			}
		}

		void ConversionFactorChanged(object sender, EventArgs e)
		{
			if (!TL_ConversionFactorStringInfo.HasErrors() && !isConversionFactorPendingUpdate)
			{
				base.TL_ConversionFactor = ConversionFactor.Factor;
				base.TL_FactorNumerator = ConversionFactor.NumeratorUnit;
				base.TL_FactorDenominator = ConversionFactor.DenominatorUnit;
			}
		}

		public ConversionFactor ConversionFactorForDocumentPrintingOnly
		{
			get
			{
				if (!ConversionFactor.IsEmpty)
				{
					return ConversionFactor;
				}

				var isDomestic = !ParentRateEntry.IsFreightEntry();
				string transportMode = null;
				if (ParentRateEntry.IsAir())
				{
					transportMode = Core.Constants.TransportModes.Air;
				}
				else if (ParentRateEntry.IsSea())
				{
					transportMode = Core.Constants.TransportModes.Sea;
				}
				else if (ParentRateEntry.IsRoad())
				{
					transportMode = Core.Constants.TransportModes.Road;
				}
				else if (ParentRateEntry.IsRail())
				{
					transportMode = Core.Constants.TransportModes.Rail;
				}
				else if (ParentRateEntry.IsCourier())
				{
					transportMode = Core.Constants.TransportModes.Courier;
				}

				if (ConversionFactorForBinding.ReadOnly || transportMode == null || TL_WeightVolume.IsEmpty)
				{
					return ConversionFactor.Empty;
				}

				var factors = ChargeableAmountCalculator.GetDefaultConversionFactors(isDomestic, transportMode, TL_WeightVolume);
				return factors.FirstOrDefault();
			}
		}

		public ConversionFactor ConversionFactor
		{
			get
			{
				return ConversionFactorForBinding.ConversionFactor;
			}
			set
			{
				ConversionFactorForBinding.ConversionFactor = value;
			}
		}

		void ResetConversionFactor(bool defaultWhenEmpty = false, bool defaultWhenNonEmpty = false)
		{
			var isReadOnly = UseOnlyActualWeightMeasure || !this.RequiresWeightVolume();

			if (isReadOnly)
			{
				ConversionFactor = ConversionFactor.Empty;
			}
			else if (!TL_WeightVolume.IsEmpty)
			{
				if ((ConversionFactor.IsEmpty && defaultWhenEmpty) || (!ConversionFactor.IsEmpty && defaultWhenNonEmpty))
				{
					ConversionFactor = this.GetDefaultConversionFactor(TL_WeightVolume);
				}
			}

			ConversionFactorForBinding.ReadOnly = isReadOnly;

			if (!IsValidationSuspended)
			{
				ConversionFactorForBinding.Validation.ValidateConversionFactorString();
			}
		}

		ConversionFactorViewModel conversionFactorForBinding;

		#endregion

		#region TL_WeightVolume

		[List("Lookups.WeightVolumes")]
		public override ZString TL_WeightVolume
		{
			get { return GetResultsRateLineValue(TL_WeightVolumeInfo, ZString.Empty, base.TL_WeightVolume); }
			set
			{
				if (base.TL_WeightVolume != value)
				{
					var oldValue = base.TL_WeightVolume;

					base.TL_WeightVolume = value;

					if (!(TL_WeightVolume == RatingConstants.Units.CN))
					{
						TL_ContainerOwnership = string.Empty;
					}

					if (!QuantityUnit.IsWeight(value) && !Calculator.BreaksPer.IsEmpty)
					{
						Calculator.BreaksPer = ZString.Empty;
						Calculator.BreaksPerInfo.RefreshBinding();
					}

					var isConversionFactorOverriden = ConversionFactor != this.GetDefaultConversionFactor(oldValue);
					ResetConversionFactor(defaultWhenNonEmpty: !isConversionFactorOverriden);

					RateLineItems.MarkAsNeedingValidationIncludingChildren();

					if (IsCalculatorInitialized && Calculator != null)
					{
						Calculator.NotifyChanged(this);
					}
				}
			}
		}

		protected bool TL_WeightVolume_ReadOnly => !this.RequiresWeightVolume()
			|| IsPropertyReadOnly(RateLinesSchema.TL_WeightVolume.Name)
			|| (this.Uses(CalculatorType.Equalization) && Parent != null && Parent.TI_RateCategory.ToString().In(RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.CAI));

		public override ZBool TL_IsWhsJobLevelCharge
		{
			get { return base.TL_IsWhsJobLevelCharge; }
			set
			{
				if (Parent != null && base.TL_IsWhsJobLevelCharge != value)
				{
					Parent.InvalidateGroupValidation();
				}
				base.TL_IsWhsJobLevelCharge = value;
			}
		}

		protected bool TL_IsWhsJobLevelCharge_ReadOnly
		{
			get { return !IsJobLevelAvailable || IsPropertyReadOnly(RateLinesSchema.TL_IsWhsJobLevelCharge.Name); }
		}

		public bool IsBreakWeightVolumeAvailable
		{
			get { return Calculator.IsBreakUnitAvailable; }
		}

		#endregion

		#region View Results

		public RateLine ResultsRateLine { get; private set; }

		[BusinessObjectTestExclude]
		public ZBool ViewResults
		{
			get { return fViewResults; }
			set
			{
				if (fViewResults != value)
				{
					fShowViewResultsWarning = false;
					if (value && this.UsesCompanyTariffOrCostBasedCalculator())
					{
						ResultsRateLine = new CompanyTariffOrCostLineCloneHelper(this).GetClone();
						if (ResultsRateLine != null)
						{
							fViewResults = value;
							fViewCalculator = null;
							ResultsRateLine.RateCalculatorChanged = false;
							ResultsRateLine.SetReadOnlyIncludingChildren(true);
						}
						else
						{
							fShowViewResultsWarning = true;
						}
					}
					else
					{
						fViewResults = value;
						fViewCalculator = null;
						ResultsRateLine = null;
					}

					if (fViewResults == value && Parent != null)
					{
						ResetParentCollection();
					}

					ViewResultsInfo.RefreshBinding();

					if (!(this is RelatedRateLine))
					{
						((ActualRateLinesValidation)Validation).ValidateViewResults();
					}
				}
			}
		}

		ZBool fViewResults;

		internal bool ShowViewResultsWarning
		{
			get { return fShowViewResultsWarning; }
		}

		bool fShowViewResultsWarning;

		public ZPropertyInfo ViewResultsInfo
		{
			[DebuggerStepThrough]
			get { return GetZPropertyInfo(nameof(ViewResults)); }
		}

		protected virtual void ResetParentCollection()
		{
			((IBusinessObjectCollectionInternals)Parent.RateLines).FireListResetEvent();
		}

		T GetResultsRateLineValue<T>(ZPropertyInfo propertyInfo, T emptyValue, T baseValue) where T : IZType
		{
			if (!IsCalculatorInitialized)
			{
				return baseValue;
			}

			if (this.UsesCompanyTariffOrCostBasedCalculator())
			{
				return ViewResults ? (T)ResultsRateLine[propertyInfo.Name] : emptyValue;
			}

			return baseValue;
		}

		public bool ViewResultsVisible
		{
			get { return !IsBulkRateUpdateActionLine && (this.UsesCompanyTariffOrCostBasedCalculator() || ViewResults); }
		}

		#endregion

		#region UseOnlyActualWeightMeasure

		public ZBool UseOnlyActualWeightMeasure
		{
			get
			{
				return this.UseOnlyActualWeightMeasure();
			}
			set
			{
				TL_ActualPercentage = (ZByte)(value ? 100 : 0);

				if (!IsValidationSuspended)
				{
					Validation.ValidateUseOnlyActualWeightMeasure();
				}

				TL_ActualPercentageInfo.RefreshBinding();
				UseOnlyActualWeightMeasureInfo.RefreshBinding();
				ResetConversionFactor(defaultWhenEmpty: true);
			}
		}

		public ZPropertyInfo UseOnlyActualWeightMeasureInfo
		{
			get { return GetZPropertyInfo(Schema.UseOnlyActualWeightMeasure); }
		}

		protected bool UseOnlyActualWeightMeasure_ReadOnly
			=> !this.RequiresWeightVolume()
				|| this.Uses(CalculatorType.Equalization)
				|| TL_UnitFactor == UnitFactorList.Codes.PacksWeight;

		#endregion

		#region TL_ActualPercentage

		public override ZByte TL_ActualPercentage
		{
			get { return GetResultsRateLineValue(TL_ActualPercentageInfo, ZByte.Zero, base.TL_ActualPercentage); }
			set
			{
				if (TL_ActualPercentage != value)
				{
					base.TL_ActualPercentage = value;
					ResetConversionFactor(defaultWhenEmpty: true);
				}
			}
		}

		protected bool TL_ActualPercentage_ReadOnly
			=> !this.RequiresWeightVolume()
				|| TL_UnitFactor == UnitFactorList.Codes.PacksWeight;

		#endregion

		#region TL_RX_NKCurrency

		[List("Lookups.Currencies")]
		public override ZString TL_RX_NKCurrency
		{
			get { return GetResultsRateLineValue(TL_RX_NKCurrencyInfo, base.TL_RX_NKCurrency, base.TL_RX_NKCurrency); }
			set
			{
				if (Parent != null && base.TL_RX_NKCurrency != value)
				{
					Parent.InvalidateGroupValidation();
				}
				base.TL_RX_NKCurrency = value;
			}
		}

		protected bool TL_RX_NKCurrency_ReadOnly
		{
			get { return (this.UsesCompanyTariffOrCostBasedCalculator() && ViewResults) || IsPropertyReadOnly(RateLinesSchema.TL_RX_NKCurrency.Name); }
		}

		#endregion

		#region TL_WeightVolumeMultiple

		public override ZDecimal TL_WeightVolumeMultiple
		{
			get { return GetResultsRateLineValue(TL_WeightVolumeMultipleInfo, (ZDecimal)0m, base.TL_WeightVolumeMultiple); }
			set
			{
				base.TL_WeightVolumeMultiple = value;

				if (IsCalculatorInitialized && Calculator != null)
				{
					Calculator.NotifyChanged(this);
				}
			}
		}

		[List("Lookups.UnitMultiples")]
		[MaxLength(8)]
		public ZString UnitMultipleAsString
		{
			get
			{
				if (!unitMultipleAsString.IsEmpty)
				{
					return unitMultipleAsString;
				}

				return RateLineHelper.GetUnitMultipleAsString(TL_WeightVolumeMultiple);
			}
			set
			{
				CheckMaximumLength(UnitMultipleAsStringInfo, value);
				unitMultipleAsString = value;
				try
				{
					TL_WeightVolumeMultiple = decimal.Parse(value, CultureInfo.CurrentCulture);
					unitMultipleAsString = ZString.Empty;
				}
				catch (OverflowException)
				{
					TL_WeightVolumeMultiple = 0m;
				}
				catch (FormatException)
				{
					TL_WeightVolumeMultiple = 0m;
				}

				if (!(this is RelatedRateLine))
				{
					((ActualRateLinesValidation)Validation).ValidateUnitMultipleAsString();
				}
				UnitMultipleAsStringInfo.RefreshBinding();
			}
		}

		ZString unitMultipleAsString;

		public ZPropertyInfo UnitMultipleAsStringInfo
		{
			[DebuggerStepThrough]
			get { return GetZPropertyInfo(nameof(UnitMultipleAsString)); }
		}

		protected bool UnitMultipleAsString_ReadOnly
		{
			get { return !this.RequiresWeightVolume() || IsPropertyReadOnly(RateLinesSchema.TL_WeightVolumeMultiple.Name); }
		}

		internal bool ShowUnitMultipleAsStringInfoParsingError
		{
			get { return !unitMultipleAsString.IsEmpty; }
		}

		#endregion

		#region TL_TI

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("Parent")]
		public override ZGuid TL_TI
		{
			get { return base.TL_TI; }
			set
			{
				if (!IsRemovingFromRelationship && value.IsEmpty)
				{
					ErrorReporter.ReportOnce("RateLine_TL_TI", string.Format(CultureInfo.CurrentCulture, "value:{0},RemoveRelationship:{1}", value, IsRemovingFromRelationship)); // To display the Column name in the key
				}
				base.TL_TI = value;
				fParent = null;
			}
		}

		#endregion

		#region TL_UnitFactor

		[List("Lookups.UnitFactors")]
		[ResourceStringData("RateLine|UnitFactor", Caption = "Unit Factor")]
		public override ZString TL_UnitFactor
		{
			get { return base.TL_UnitFactor; }
			set
			{
				base.TL_UnitFactor = value;
				if (base.TL_UnitFactor.ToString().In(UnitFactorList.Codes.PacksWeight))
				{
					UseOnlyActualWeightMeasure = true;
				}
			}
		}

		protected bool TL_UnitFactor_ReadOnly => (IsBulkRateUpdateActionLine && !this.IsIntercompanyTariff()) || IsPropertyReadOnly(RateLinesSchema.TL_UnitFactor.Name);

		void ResetUnitFactorIfNeeded()
		{
			if (TL_UnitFactor == UnitFactorList.Codes.LoadedPackagesOnly && TL_RateCalculator != WarehousePackCalculator.Code)
			{
				TL_UnitFactor = "";
			}
		}

		#endregion

		#region TL_Rounding

		protected bool TL_Rounding_ReadOnly
		{
			get { return !this.Uses(CalculatorType.WarehousePack) && !this.RequiresWeightVolume() || IsPropertyReadOnly(RateLinesSchema.TL_Rounding.Name); }
		}

		#endregion

		#region Calculated RoundingFactor

		public (ZString RoudingType, ZDecimal RoudingFactor) CalculatedRounding
		{
			get
			{
				if (TL_Rounding == RatingRoundingTypes.Custom)
				{
					return (RatingRoundingTypes.Custom, TL_RoundingFactor);
				}

				DefaultRoundings defaultRoundings;
				if (TL_Rounding == RatingRoundingTypes.DefaultFromRegistry)
				{
					defaultRoundings = DataRegistryRating.Instance.DefaultRounding.GetDefaultRounding(ParentRateEntry.TI_RateCategory);
					if (defaultRoundings.RoundingType == RatingRoundingTypes.Custom && defaultRoundings.RoundingFactor.IsEmpty)
					{
						return (RatingRoundingTypes.Custom, TL_RoundingFactor);
					}

					return (defaultRoundings.RoundingType, defaultRoundings.RoundingFactor);
				}

				defaultRoundings = new DefaultRoundings
				{
					RoundingType = TL_Rounding
				};
				return (defaultRoundings.RoundingType, defaultRoundings.RoundingFactor);
			}
		}

		public ZDecimal RoundingFactor
		{
			get => CalculatedRounding.RoudingFactor;
			set
			{
				if (TL_Rounding == RatingRoundingTypes.Custom)
				{
					TL_RoundingFactor = value;
				}
				RoundingFactorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RoundingFactorInfo => GetZPropertyInfo(nameof(RoundingFactor));

		protected bool RoundingFactor_ReadOnly
		{
			get { return TL_Rounding != RatingRoundingTypes.Custom; }
		}

		#endregion

		#region TL_IsOnPallets

		protected bool TL_IsOnPallets_ReadOnly
		{
			get { return ContainerParameterColumnReadOnly(RateLinesSchema.TL_IsOnPallets.Name); }
		}

		protected bool TL_ContainerOwnership_ReadOnly =>
			ContainerParameterColumnReadOnly(Schema.TL_ContainerOwnership);

		bool ContainerParameterColumnReadOnly(string columnName)
		{
			return this.UsesCompanyTariffOrCostBasedCalculator() || TL_WeightVolume != RatingConstants.Units.CN || IsPropertyReadOnly(columnName);
		}

		#endregion

		#region TL_OP_ProductNumber - proxy to TL_ParentID

		[RelatedBusinessObject("ProductNumber")]
		[List("Lookups.ProductNumbers")]
		public ZGuid TL_OP_ProductNumber
		{
			get { return base.TL_ParentID; }
			set
			{
				base.TL_ParentID = value;
				base.TL_ParentTableCode = OrgSupplierPartSchema.Constants.Prefix;
			}
		}

		public virtual OrgSupplierPart ProductNumber
		{
			get { return Factory.Load<OrgSupplierPart>(TL_OP_ProductNumber); }
		}

		public ZPropertyInfo TL_OP_ProductNumberInfo => GetWrappedZPropertyInfo(Schema.TL_OP_ProductNumber, x => TL_ParentIDInfo);

		protected bool TL_ParentID_ReadOnly => this.UsesCompanyTariffOrCostBasedCalculator() || !IsProductAllowed || IsPropertyReadOnly(Schema.TL_OP_ProductNumber);

		protected bool TL_OP_ProductNumber_ReadOnly => TL_ParentID_ReadOnly;

		#endregion

		#region BulkRateUpdate ActionLine

		public bool IsBulkRateUpdateActionLine
		{
			get { return fIsBulkRateUpdateActionLine; }
			set { fIsBulkRateUpdateActionLine = value; }
		}

		bool fIsBulkRateUpdateActionLine;

		#endregion

		#region View Agent Rates

		public void SetViewAgentRatesWithoutRefreshBinding(bool newValue) => viewAgentRates = newValue;

		public ZBool ViewAgentRates
		{
			get { return viewAgentRates; }
			set
			{
				if (viewAgentRates != value)
				{
					viewAgentRates = value;

					ViewAgentRatesInfo.RefreshBinding();

					foreach (var lineItem in RateLineItems.Cast<RateLineItem>())
					{
						lineItem.TM_RelevantValueInfo.RefreshBinding();
					}
				}
			}
		}

		ZBool viewAgentRates;

		public virtual ZPropertyInfo ViewAgentRatesInfo
		{
			[DebuggerStepThrough]
			get { return GetZPropertyInfo(nameof(ViewAgentRates)); }
		}

		#endregion

		#region Fees and Charges

		[List("Lookups.FeeChargeLevels")]
		public override ZString TL_FeeChargeLevel
		{
			get { return base.TL_FeeChargeLevel; }
			set { base.TL_FeeChargeLevel = value; }
		}

		protected bool TL_FeeChargeLevel_ReadOnly
		{
			get { return TL_FeeChargeType.IsEmpty || TL_FeeChargeType_ReadOnly; }
		}

		[List("Lookups.FeeChargeTypes")]
		public override ZString TL_FeeChargeType
		{
			get { return base.TL_FeeChargeType; }
			set
			{
				if (base.TL_FeeChargeType != value)
				{
					if (!TL_FeeChargeLevel.IsEmpty
						&& !IsCopying
						&& !((ISupportDataImporting)this).IsImportingData)
					{
						TL_FeeChargeLevel = ZString.Empty;
					}

					base.TL_FeeChargeType = value;
				}
			}
		}

		protected bool TL_FeeChargeType_ReadOnly
		{
			get { return Header == null || (!Header.IsLevelOneTariff() && !Header.IsStandardCostRate()); }
		}

		#endregion

		#region Calc_RateDescOrRateDescLocal

		public ZString GetRateDescOrRateDescLocal(bool? isLocalClient = null)
		{
			bool currentIsLocalClient;

			if (isLocalClient == null)
			{
				currentIsLocalClient = ParentRateEntry?.ParentRatingHeader?.HasLocalClient() ?? false;
			}
			else
			{
				currentIsLocalClient = isLocalClient.Value;
			}

			return (currentIsLocalClient || ObjectFactory.Get<IAccounting>().ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors)
				&& ObjectFactory.Get<IAccounting>().EnableLocalChargeCodeDescriptionDefault
				&& !TL_RateDescLocal.IsEmpty
				? TL_RateDescLocal
				: TL_RateDesc;
		}

		public ZString GetMultilingualRateDesc(bool? isLocalClient = null)
		{
			var description = GetRateDescOrRateDescLocal(isLocalClient);

			return ChargeCode?.GetMultilingualOrDefaultDescription(description)
				?? TL_RateDesc;
		}

		#endregion

		#region TL_RateDesc

		protected bool TL_RateDesc_ReadOnly => !HasOverrideChargeDescriptionPermission || !OverrideChargeDescription;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public override ZString TL_RateDesc
		{
			get
			{
				return !overrideChargeDescription && base.TL_RateDesc.IsEmpty && ChargeCode != null
					? ChargeCode.AC_Desc
					: base.TL_RateDesc;
			}
			set
			{
				base.TL_RateDesc = value;
				if (!IsValidationSuspended && Validation is ActualRateLinesValidation)
				{
					((ActualRateLinesValidation)Validation).ValidateOverrideChargeDescription();
				}
			}
		}

		#endregion

		#region TL_RateDescLocal

		protected bool TL_RateDescLocal_ReadOnly => !HasOverrideChargeDescriptionPermission || !OverrideChargeDescription;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "If Local description is empty, then show description.")]
		public override ZString TL_RateDescLocal
		{
			get
			{
				if (!overrideChargeDescription && base.TL_RateDescLocal.IsEmpty && ChargeCode != null)
				{
					return !ChargeCode.AC_LocalLanguageDescription.IsEmpty
						? ChargeCode.AC_LocalLanguageDescription
						: ChargeCode.AC_Desc;
				}

				return base.TL_RateDescLocal;
			}
			set
			{
				base.TL_RateDescLocal = value;
			}
		}

#if DEBUG

		internal ZString BaseRateDesc => base.TL_RateDesc;

		internal ZString BaseRateDescLocal => base.TL_RateDescLocal;

#endif

		#endregion

		#region Override Charge Description

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZBool OverrideChargeDescription
		{
			get { return overrideChargeDescription; }
			set
			{
				SetNonPersistentPropertyValue(OverrideChargeDescriptionInfo, ref overrideChargeDescription, value);
				if (overrideChargeDescription && ChargeCode != null)
				{
					TL_RateDesc = ChargeCode.AC_Desc;
					TL_RateDescLocal = !ChargeCode.AC_LocalLanguageDescription.IsEmpty
						? ChargeCode.AC_LocalLanguageDescription
						: ChargeCode.AC_Desc;
				}
				else
				{
					TL_RateDesc = ZString.Empty;
					TL_RateDescLocal = ZString.Empty;
				}
			}
		}
		ZBool overrideChargeDescription;

		public ZPropertyInfo OverrideChargeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(OverrideChargeDescription)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		bool OverrideChargeDescription_ReadOnly => !HasOverrideChargeDescriptionPermission;

		bool HasOverrideChargeDescriptionPermission => SecurityCheckpointForOverrideChargeDescription?.IsAllowed ?? true;

		SecurityCheckpoint SecurityCheckpointForOverrideChargeDescription
		{
			get
			{
				SecurityCheckpoint result = null;

				var ratingHeader = Parent?.Parent;
				if (!IsBulkRateUpdateActionLine && ratingHeader != null)
				{
					if (ratingHeader.IsTariff())
					{
						result = Env.Security.CompanyTariffRatesChargeDescriptionOverride;
					}
					else if (ratingHeader.IsCosting())
					{
						result = Env.Security.CostingRatesChargeDescriptionOverride;
					}
					else if (ratingHeader.IsClientRate())
					{
						result = Env.Security.ClientRatesChargeDescriptionOverride;
					}
					else if (ratingHeader.IsQuote())
					{
						result = Env.Security.QuotationChargeDescriptionOverride;
					}
					else if (ratingHeader.IsIntercompanyTariff())
					{
						result = Env.Security.IntercompanyTariffsChargeDescriptionOverride;
					}
				}

				return result;
			}
		}

		#endregion

		#region Trade Lane Charge Notes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;

				types.Add(PredefinedNoteTypes.Instance.TradeLaneChargeInformation);
				types.Add(PredefinedNoteTypes.Instance.TradeLaneChargeInternalNote);

				return types;
			}
		}

		#region Information Note

		readonly string informationNote = PredefinedNoteTypes.Instance.TradeLaneChargeInformation.Description;

		string InformationNoteCacheKey => PK + "|InformationNote"; // CacheKey

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString ChargeInformationNoteText
		{
			get => GetChargeNote(InformationNoteCacheKey, informationNote);
			set => SetChargeNote(ChargeInformationNoteTextInfo, InformationNoteCacheKey, informationNote, value);
		}

		ZPropertyInfo ChargeInformationNoteTextInfo => GetZPropertyInfo(Schema.ChargeInformationNoteText);

		#endregion

		#region Internal Note

		readonly string internalNote = PredefinedNoteTypes.Instance.TradeLaneChargeInternalNote.Description;

		string InternalNoteCacheKey => PK + "|InternalNote"; // CacheKey

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString ChargeInternalNoteText
		{
			get => GetChargeNote(InternalNoteCacheKey, internalNote);
			set => SetChargeNote(ChargeInternalNoteTextInfo, InternalNoteCacheKey, internalNote, value);
		}

		ZPropertyInfo ChargeInternalNoteTextInfo => GetZPropertyInfo(Schema.ChargeInternalNoteText);

		#endregion

		ZString GetChargeNote(string noteCacheKey, string noteKey)
		{
			return Factory.GetCachedValue(noteCacheKey, () =>
			{
				var note = GetChargeNote(noteKey);
				return note?.ST_NoteText ?? ZString.Empty;
			});
		}

		void SetChargeNote(ZPropertyInfo propertyInfo, string noteCacheKey, string noteKey, string value)
		{
			SetChargeNote(noteKey, value);
			Factory.ClearCachedValue<ZString>(noteCacheKey);
			propertyInfo.RefreshBinding();
		}

		StmNote GetChargeNote(ZString description)
		{
			return Notes.FindByDescription(description).FirstOrDefault();
		}

		void SetChargeNote(ZString description, ZString value)
		{
			var note = GetChargeNote(description);
			if (value.IsEmpty)
			{
				if (note != null)
				{
					note.Delete();
				}
			}
			else
			{
				if (note == null)
				{
					Notes.AddNew(false, description, value);
				}
				else
				{
					note.ST_NoteText = value;
				}
			}
		}

		#endregion

		#region Contract Number

		public ZString ContractNumber
		{
			get { return Parent != null ? Parent.TI_ContractNumber : ZString.Empty; }
		}

		#endregion

		#region Spot Rate Description

		internal ZString SpotRateDescription { get; set; }

		#endregion

		#region DIN Proxy Properties For PropertyInfo Binding

		internal ZDecimal EffectiveRate
		{
			get { return 0m; }
		}

		internal ZPropertyInfo EffectiveRateInfo
		{
			get { return GetZPropertyInfo(nameof(EffectiveRate)); }
		}

		internal ZDecimal CurrentPrimeRate
		{
			get { return 0m; }
		}

		internal ZPropertyInfo CurrentPrimeRateInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentPrimeRate)); }
		}

		#endregion

		#endregion

		#region Clone

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(RateLinesSchema.Constants.TL_TI);

			return result;
		}

		public bool IsCloneForViewResults { get; private set; }

		public RateLine Clone(RateLinesCollection destinationRateLinesCollection)
		{
			var clone = destinationRateLinesCollection.AddNew();
			clone.IsCloneForViewResults = true;

			return CopyTo(clone);
		}

		public RateLine Clone(BusinessObjectFactory newFactory)
		{
			var clonedHeader = newFactory.New(Parent.Parent.GetType()) as RatingHeader;

			using (clonedHeader.SuspendSettingHasChanges())
			{
				clonedHeader.CopyPersistentValuesFrom(Parent.Parent);
				clonedHeader.TH_OH = Parent.Parent.TH_OH;
			}

			var entryCollection = clonedHeader.EntryCollections[Parent.TI_RateCategory];
			var clonedEntry = Parent.Clone(entryCollection);
			var clone = clonedEntry.RateLines.AddNew();
			clone.IsCloneForViewResults = true;

			return CopyTo(clone);
		}

		public RateLine Clone(RowFactory rowFactory, RateEntry parentRateEntry)
		{
			var clone = rowFactory.New(TableName);
			RateLine clonedObject;

			using (clone.MarkAsInConstruction(RateLinesSchema.PK, Factory))
			{
				this.CopyToDataRow(rowFactory, clone, GetPropertiesThatShouldNotBeCopied());
				clone["TL_TI"] = parentRateEntry.PK.ToGuid();
				clonedObject = new RateLine(Factory, clone);
			}

			clonedObject.IsCloneForViewResults = true;

			return CopyTo(clonedObject, false);
		}

		public RateLine CopyTo(RateLine copyToRateLine, bool copyPersistentValues = true)
		{
			using (copyToRateLine.SuspendSettingHasChanges())
			using (copyToRateLine.GetValidationSuspender())
			{
				if (copyPersistentValues)
				{
					copyToRateLine.CopyPersistentValuesFrom(this);
				}

				// order is important
				if (!copyToRateLine.TL_ConditionalExpression_ReadOnly)
				{
					copyToRateLine.TL_ConditionalExpression = TL_ConditionalExpression;
					copyToRateLine.TL_ConditionalExpressionDescription = TL_ConditionalExpressionDescription;
				}

				if (ParentRateEntry.IsGlobal() != copyToRateLine.ParentRateEntry.IsGlobal() || Parent.IsPublished != copyToRateLine.Parent.IsPublished)
				{
					SetLinkedChargeCode(copyToRateLine);
				}

				copyToRateLine.TL_RateCalculator = TL_RateCalculator;

				copyToRateLine.OverrideChargeDescription = OverrideChargeDescription;
				copyToRateLine.TL_RateDesc = base.TL_RateDesc;
				copyToRateLine.TL_RateDescLocal = base.TL_RateDescLocal;

				copyToRateLine.TL_WeightVolumeMultiple = TL_WeightVolumeMultiple;
				// avoid setting ActualPercentage to default value because setting WeightVolume triggers HighestRateCalculator.NotifyChanged that calls
				// SetDefaultRateLineValues then set RateLine.UseOnlyActualWeightMeasure and then set TL_ActualPercentage=100
				copyToRateLine.TL_WeightVolume = TL_WeightVolume;
				copyToRateLine.TL_ActualPercentage = TL_ActualPercentage;
				copyToRateLine.TL_ContainerOwnership = TL_ContainerOwnership;
				copyToRateLine.ChargeInformationNoteText = ChargeInformationNoteText;
				copyToRateLine.ChargeInternalNoteText = ChargeInternalNoteText;
				copyToRateLine.ConversionFactor = ConversionFactor;
			}
			return copyToRateLine;
		}

		void SetLinkedChargeCode(RateLine copyToRateLine)
		{
			var chargeCode = copyToRateLine.ChargeCode;
			if (chargeCode != null)
			{
				var ledgerType = copyToRateLine.IsCostRate() ? ZArchitecture.Core.LedgerTypes.AccountsPayable : ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				AccChargeCode linkedChargeCode = null;
				if (!chargeCode.IsGlobal && copyToRateLine.ParentRateEntry.IsGlobal())
				{
					linkedChargeCode = chargeCode.GetGlobalChargeCode(ledgerType, null);
				}
				else if (chargeCode.IsGlobal && !copyToRateLine.ParentRateEntry.IsGlobal())
				{
					linkedChargeCode = chargeCode.GetLocalChargeCode(ledgerType, null);
				}

				if (linkedChargeCode != null && linkedChargeCode.PK != copyToRateLine.TL_AC)
				{
					copyToRateLine.TL_AC = linkedChargeCode.PK;
				}
			}
		}

		#endregion

		#region Validation

		protected override RateLinesValidation GetNewValidation()
		{
			return new ActualRateLinesValidation(this);
		}

		#endregion

		#region Rate Line Items

		protected RateLineItemsCollection fRateLineItems;

		ZDecimal InheritedDiscount;

		bool RateLineItemsReadOnlyOnLoad;

		/// <summary>
		/// These are the RateLineItems that belong to the RateLine.
		/// However, there's another set of RateLineItems in each calculator.
		/// Those are the subset that belongs in the grid view of the calculator.
		/// </summary>
		[ChildEditable(false)]
		public virtual RateLineItemsCollection RateLineItems
		{
			get
			{
				var needToReload = fRateLineItems == null || (IsTariffLineInherited && InheritedDiscount != CompanyTariffDiscount);

				bool currentReadOnly;
				if (fRateLineItems == null)
				{
					fRateLineItems = new RateLineItemsCollection(this);
					currentReadOnly = RateLineItemsReadOnlyOnLoad;
				}
				else
				{
					currentReadOnly = fRateLineItems.ReadOnly;
				}

				if (needToReload)
				{
					if (IsTariffLineInherited)
					{
						InheritedDiscount = CompanyTariffDiscount;
					}

					fRateLineItems = new RateLineItemsCollection(this);
					fRateLineItems.Load();
					fRateLineItems.SetReadOnlyIncludingChildren(currentReadOnly);
					RegisterEditableChildObject(RateLineItems);
				}

				return fRateLineItems;
			}
		}

		public virtual bool IsRateLineItemsLoaded => fRateLineItems != null;

		#region Delete Rate Line and Children

		/// <summary>
		/// Delete this Rate Line and all child Rate Line Items
		/// </summary>
		public override void Delete()
		{
			if (IsDeleted)
			{
				return; // to handle datarefreshbus trying to delete our object even though it's already deleted
			}

			var inMemoryQuery = new ZQuery { FetchOnlyFromLocalCache = true };
			inMemoryQuery.AddToFilter(RateLineItemsSchema.TM_TL, PK);
			var inMemoryLineItems = Factory.Load<RateLineItem>(inMemoryQuery);

			var queryNotes = new ZQuery { FetchOnlyFromLocalCache = false };
			queryNotes.AddToFilter(StmNoteSchema.ST_Table, RateLinesSchema.Constants.TableName);
			queryNotes.AddToFilter(StmNoteSchema.ST_ParentID, PK);
			var inMemoryNotesItems = Factory.Load<StmNote>(queryNotes);

			Factory.ClearCachedValue<ZString>(InternalNoteCacheKey);
			Factory.ClearCachedValue<ZString>(InformationNoteCacheKey);

			base.Delete();

			foreach (var lineItem in inMemoryLineItems)
			{
				lineItem.Delete();
			}

			foreach (var note in inMemoryNotesItems)
			{
				note.Delete();
			}
		}

		#endregion

		#endregion

		#region Calculator

		public Calculator Calculator
		{
			get
			{
				InitializeCalculator();
				return calculator;
			}
		}

		Calculator calculator;

		public Calculator ViewCalculator
		{
			get
			{
				if (ViewResults && ResultsRateLine != null)
				{
					if (fViewCalculator == null)
					{
						fViewCalculator = CalculatorFactory.GetCalculator(ResultsRateLine);
					}
					return fViewCalculator;
				}
				else
				{
					return Calculator;
				}
			}
		}

		Calculator fViewCalculator;

		public bool IsCalculatorInitialized
		{
			get { return !(calculator == null || RateCalculatorChanged); }
		}

		public void InitializeCalculator()
		{
			if (!IsCalculatorInitialized)
			{
				calculator = CalculatorFactory.GetCalculator(this);
				RateCalculatorChanged = false;
			}
		}

		#region ViewCalculatorForBinding

		public CalculatorWrapper ViewCalculatorForBinding
		{
			get
			{
				if (viewCalculatorForBinding.Calculator != ViewCalculator)
				{
					viewCalculatorForBinding = new CalculatorWrapper(ViewCalculator);
				}

				return viewCalculatorForBinding;
			}
		}

		CalculatorWrapper viewCalculatorForBinding;

		public struct CalculatorWrapper
		{
			public CalculatorWrapper(Calculator calculator)
			{
				calculatorRef = new WeakReference(calculator);
				IsWiseRatesView = calculator?.RateLineBizO == null;
				IsRelatedRateLine = calculator.RateLineBizO is RelatedRateLine;
			}

			public Calculator Calculator
			{
				get { return calculatorRef != null ? calculatorRef.Target as Calculator : null; }
			}

			public bool IsWiseRatesView { get; }

			public bool IsRelatedRateLine { get; }

			readonly WeakReference calculatorRef;
		}

		#endregion

		#endregion

		#region Is Cloned

		public RateLine ClonedLineMaster
		{
			get { return fClonedLineMaster; }
			internal set { fClonedLineMaster = value; }
		}

		RateLine fClonedLineMaster;

		public bool IsCloned
		{
			get { return fClonedLineMaster != null; }
		}

		#endregion

		#region Parent Rate Entry

		public RatingHeader Header
		{
			get { return Parent?.Parent; }
		}

		public RateEntry Parent
		{
			get { return fParent ?? (fParent = GetParentFromCollections() ?? Factory.Load<RateEntry>(TL_TI)); }
		}

		RateEntry fParent;

		protected virtual RateEntry GetParentFromCollections()
		{
			RateEntry result = null;
			var rateLinesCollection = ((IBusinessObjectInternals)this).ParentCollections.Where(x => x is IRateLinesWithParentEntry);
			foreach (IRateLinesWithParentEntry parentCollection in rateLinesCollection)
			{
				if (parentCollection.Master != null)
				{
					result = parentCollection.Master;
				}
			}

			return result;
		}

		#endregion

		#region Event Logging

		public override void OnSaving()
		{
			base.OnSaving();
			if (Parent != null)
			{
				Parent.AddLog();
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			AdditionalActionsOnMasterSaving();
		}

		protected virtual void AdditionalActionsOnMasterSaving()
		{
			if (calculator != null)
			{
				calculator.RunActionsOnMasterSaving();
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			if (IsDeleted)
			{
				return;
			}

			base.BeforeSuccessfulDelete();
			if (IsInDatabase && Parent != null)
			{
				Parent.AddLog();
			}
		}

		#endregion

		#region Pre-Fetch

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new RateLineFetchStrategy(this);
		}

		#endregion

		#region IGetZPropertyInfo Members

		ZPropertyInfo IGetZPropertyInfo.GetZPropertyInfo(string propertyName)
		{
			return GetZPropertyInfo(propertyName);
		}

		#endregion

		#region Data Import

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData || (Parent != null && ((ISupportDataImporting)Parent).IsImportingData); }
			set {
				if (isConversionFactorPendingUpdate)
				{
					ConversionFactor = new ConversionFactor(base.TL_ConversionFactor, base.TL_FactorNumerator, base.TL_FactorDenominator);
					isConversionFactorPendingUpdate = false;
				}
				fIsImportingData = value;
			}
		}
		bool fIsImportingData;

		#endregion

		#region ErrorReporting

		public void SendErrorReporterIfParentIsNull(string errorReporterKey)
		{
			if (Parent == null)
			{
				var errorMessageBuilder = new ZStringBuilder();

				#region SuppressResourceStringsCheckRegion
				errorMessageBuilder.Append("Inform IL/Rating Team. Orphan line has been spotted.");
				errorMessageBuilder.Append("PK: " + PK);
				errorMessageBuilder.Append("Is In Database:" + IsInDatabase);
				errorMessageBuilder.Append("Is Deleted: " + IsDeleted);
				errorMessageBuilder.Append("fParent: " + (fParent == null ? "null" : fParent.PK.ToString()));
				errorMessageBuilder.Append("TL_TI: " + TL_TI); // Just To Report an Error
				errorMessageBuilder.Append("--------- Parent Collections ----------");
				for (var i = 0; i < ((IBusinessObjectInternals)this).ParentCollections.Length; i++)
				{
					var parentCollection = ((IBusinessObjectInternals)this).ParentCollections[i];
					var collection = parentCollection as RateLinesCollection;
					var parentCollectionMaster = collection != null ? collection.Master : null;
					errorMessageBuilder.Append("ParentCollection[" + i + "]");
					errorMessageBuilder.Append("Type: " + parentCollection.GetType());
					errorMessageBuilder.Append("Master: " + (parentCollectionMaster == null ? "null" : parentCollectionMaster.PK.ToString()));
				}
				errorMessageBuilder.Append("------------------------------------------");
				#endregion

				ErrorReporter.ReportOnce(errorReporterKey, errorMessageBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		protected override IAdditionalNoteProvider GetAdditionalNoteProvider() => null;

		public IEnumerable<IRateLineItem> ChildRateLineItems => RateLineItems.Cast<IRateLineItem>();
		public IList<IRateLine> IncludedLines { get; } = new List<IRateLine>();

		public IRateEntry ParentRateEntry
		{
			get { return Parent; }
		}

		public string InvalidReason => GetInvalidReason();

		string GetInvalidReason()
		{
			var errors = new StringBuilder();
			Validation.ValidateTL_WeightVolume();
			if (TL_WeightVolumeInfo.HasErrors())
			{
				foreach (var notification in TL_WeightVolumeInfo.Notifications)
				{
					errors.AppendLine(notification.Message);
				}
			}

			return errors.ToString();
		}

		public ZString Comment => string.Empty;

		public ZString CarrierChargeCode => string.Empty;
		public ZString CarrierChargeCodeDescription => string.Empty;

		public ZString UniversalChargeCodes => this.GetUniversalChargeCodes();

		public static RateLine GetBO(IRateLine line)
		{
			return line as RateLine;        //ToDo: fix this
		}

		public ZString ChargeType
		{
			get
			{
				if (Calculator is FreightInclusiveCalculator freightInclusiveCalculator)
				{
					return freightInclusiveCalculator.FreightCalcTypeDescription;
				}

				return null;
			}
		}

		bool ICompanyFilterProviderContext.IsLocalFirst => !Parent.IsGlobal();
	}
}

