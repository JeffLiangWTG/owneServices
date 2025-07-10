using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public static class CalculatorConstants
	{
		public static class Type
		{
			public const string PER = "PER";
			public const string PRU = "PRU";
			public const string ApplyTo = "APP";
			public const string ValueOrPartThereOf = "VPT";
			public const string Rate = "RAT";
		}

		public static class Text
		{
			public static class ApplyTo
			{
				public static class Value
				{
					public const string SingleTransactionBondAmount = "BND";
					public const string InsuranceValue = "INS";
					public const string ValueOfGoods = "VAL";
					public const string CustomsValue = "CUV";
					public const string InvoiceValue = "INV";

					public static class Descriptions
					{
						public static string ValueOfGoods
							=> Res.GetString("975cae49-20f5-4eb3-9529-0b9ea62b1626", "Value of Goods");

						public static string InsuranceValue
							=> Res.GetString("cd1ed8ad-89f0-4dbb-ab3a-8950eb9b7fcd", "Insurance Value");

						public static string CustomsValue
							=> Res.GetString("23157d32-89f0-41ec-8a4f-d946695f3642", "Customs Value");

						public static string InvoiceValue
							=> Res.GetString("78723234-4f36-44a7-9c29-b5fda7e2a7b6", "Invoice Value");

						public static string BondAmount
							=> Res.GetString("3505e7a3-d819-4ecc-87d7-42e338546b93", "Single Transaction Bond Amount");
					}
				}

				public static class Charges
				{
					public const string ChargeCode = "COD";
					public const string AllCharges = "ALL";
					public const string FreightCharges = "FRT";
					public const string OriginCharges = "ORG";
					public const string DestinationCharges = "DST";
					public const string LoadingCharges = "LOD";
					public const string OriginCustomsBrokerageCharges = "OBR";
					public const string CustomsBrokerageCharges = "BRK";
					public const string UnloadingCharges = "UNL";

					public static IEnumerable<string> All => new[]
					{
						ChargeCode,
						AllCharges,
						FreightCharges,
						OriginCharges,
						DestinationCharges,
						LoadingCharges,
						OriginCustomsBrokerageCharges,
						CustomsBrokerageCharges,
						UnloadingCharges,
					};
				}
			}

			public const string IncludeGST = "IGS";
			public const string GreaterCharge = "GRT";

			// TODO: Get rid of it and use ApplyTo.Charge.* constants instead
			public const string ChargeCode = ApplyTo.Charges.ChargeCode;
			public const string AllCharges = ApplyTo.Charges.AllCharges;
			public const string FreightCharges = ApplyTo.Charges.FreightCharges;
			public const string OriginCharges = ApplyTo.Charges.OriginCharges;
			public const string DestinationCharges = ApplyTo.Charges.DestinationCharges;
			public const string LoadingCharges = ApplyTo.Charges.LoadingCharges;
			public const string OriginCustomsBrokerageCharges = ApplyTo.Charges.OriginCustomsBrokerageCharges;
			public const string CustomsBrokerageCharges = ApplyTo.Charges.CustomsBrokerageCharges;
			public const string UnloadingCharges = ApplyTo.Charges.UnloadingCharges;

			//MinimumCalculator
			public const string MIN_Job = "JOB";
			public const string MIN_ChargeCode = "CHC";

			//Percentage
			public const string PER_PartThereof = "PTO";

			//ProfitShareRebateCalculator
			public const string ZeroWhenLoss = "ZWL";
		}

		public static class MapTo
		{
			public const string ChargeCode = nameof(ChargeCode);
		}
	}

	[PropertyDescriptorCollection(typeof(BusinessObjectPropertyDescriptorCollection))]
	public abstract partial class Calculator : IEquatable<Calculator>
	{
		protected Calculator(IRateLine master)
		{
			this.master = Argument.NotNull(master, "master");
			CalculatorFactory.EnsureIsCreatedByFactory(master);
		}

		#region Construction

		public void PerformPostConstructionActions()
		{
			if (RateLineBizO != null)
			{
				using (RateLineBizO.GetValidationSuspender())
				{
					if (RateLineBizO.RateCalculatorChanged)
					{
						RateLineBizO.RateLineItems.RemoveAndDeleteAll();
					}

					if (!RateLineBizO.ViewResults)
					{
						CheckOrCreateItems();
					}
				}
			}
			else
			{
				CheckOrCreateItems();
			}

			PerformPostItemCreationActions();
		}

		protected virtual void PerformPostItemCreationActions() { }

		protected virtual IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var itemList = new List<IRateLineItem>();
			foreach (var attribute in Attributes)
			{
				if (attribute.IsMandatory)
				{
					var item = FindOrAddRateLineItem(attribute);
					if (item != null)
					{
						itemList.Add(item);
					}
				}
			}

			return itemList;
		}

		#endregion
		internal RateLine RateLineBizO { get { return master as RateLine; } }
		public IRateLine Line { get { return master; } }
		readonly IRateLine master;

		protected IRateEntry ParentRateEntry => Line.ParentRateEntry;
		protected IRatingHeader ParentRatingHeader => Line.ParentRateEntry?.ParentRatingHeader;

		ChargeableValueProvider ChargeableValueProvider
		{
			get { return chargeableValueProvider ?? (chargeableValueProvider = new ChargeableValueProvider(this)); }
		}
		ChargeableValueProvider chargeableValueProvider;

		#region Calculator Item Codes

		public static class Items
		{
			//ToDo: Move all those codes to the CalculatorConstants
			public const string UseAccumulated = "ACC";
			public const string HigherChargeableLowerRate = "HCL";
			public const string UseInclusiveBreaks = "INB";
			public const string EquipmentType = "EQU";
			public const string BreaksPer = "BRP";
			public const string BreaksPerContainerTypeOrClass = "CTT";
			public const string BreaksPerContainer = "CTN";

			public const string MultipleEquipmentsOverMaxWeightVolume = "MEQ";

			public const string RatePickRule = "RPR";
			public const string HighestRate = "";
			public const string AsFreightedHighestRateWhenMin = "HRM";
			public const string AsFreightedDontApplyWhenMin = "DAM";
			public const string ACIZoneData = "ACI";

			public const string CalculationOrder = "ORD";
			public const string PercentFirst = "PER";
			public const string IncreaseFirst = "FIX";

			public const string GreaterCharge = "GRT";
			public const string BreaksBasedOnValues = "BBV";

			public const string ShowOnBillingWithoutPrefix = "SHW";

			public const string Uplift = "UPL";
			public const string AdjustmentDays = "DAY";
			public const string OutstandingDays = "OUT";

			public const string FirstPackageRate = "FPR";
			public const string AddtionalPackageRate = "APR";

			public const string ExcludeHolidays = "EXH";
			public const string ExcludeWeekendsAndPublicHolidays = "WEH";
			public const string ExcludeSundaysAndPublicHolidays = "SUH";

			public static string ExcludeWeekendsAndPublicHolidaysDescription
			{
				get { return Res.GetString("a1ea08bf-23a5-4320-a63b-6b058825199f", "Weekends and Public Holidays"); }
			}
			public static string ExcludeSundaysAndPublicHolidaysDescription
			{
				get { return Res.GetString("d7e8f7b6-e54b-4df1-ae1a-5ffffac6dd18", "Sundays and Public Holidays"); }
			}

			public static string PercentFirstDescription
			{
				get { return Res.GetString("69598ec0-0116-4f0d-955c-a013687e56f8", "Apply Percentage change first"); }
			}
			public static string IncreaseFirstDescription
			{
				get { return Res.GetString("ce6e38c1-589e-45db-805d-a9265c4af97b", "Apply Fixed changes first"); }
			}

			public static string AsFreightedHighestRateWhenMinDescription
			{
				get { return Res.GetString("c03beff5-ecbc-4cec-ae75-0c2215afdad4", "Highest Rate when Freight Charge is Minimum"); }
			}

			public static string AsFreightedDontApplyWhenMinDescription
			{
				get { return Res.GetString("274b7018-528d-4c0d-9955-4d30bc58ced9", "Don't apply when Freight Charge is Minimum"); }
			}

			public static string ZeroWhenLossDescription
			{
				get { return Res.GetString("eea1d50b-3ef7-4ed2-909d-599c826b548d", "Calculate as zero rebate if Job Total is a loss."); }
			}

			public static class Operator
			{
				public const string MIN = "MIN";
				public const string MAX = "MAX";
				public const string BAS = "BAS";
				public const string UNT = "UNT";
				public const string Minus = "-";
				public const string Plus = "+";
			}

			#region Value Calculators Mapper Codes

			public static class Value
			{
				#region DisbursementApplyToTypes
				public static class DisbursementApplyToTypes
				{
					public const string Disbursements = "DSB";
					public const string CustomsDisbursement = "CUD";
				}

				public static class DisbursementApplyToTypesDescription
				{
					public static string DisbursementsDescription
					{
						get { return Res.GetString("01815f0f-d601-426b-9ec1-59b337f0f236", "Disbursements"); }
					}
					public static string CustomsDisbursementDescription
					{
						get { return Res.GetString("c2c89883-4f58-4fb1-8f79-267479c94d1c", "Customs Disbursement"); }
					}
				}
				#endregion

				#region ChargesApplyToTypes
				public static class ChargesApplyToTypesDescription
				{
					public static string ChargeCodeDescription
					{
						get { return Res.GetString("2f05d902-e0f4-4422-b42b-2aecc51cde57", "Charge Code"); }
					}
					public static string AllChargesDescription
					{
						get { return Res.GetString("bdb4f3cb-b080-41c3-bd7d-490cf58c7faf", "All Charge Codes"); }
					}
					public static string FreightChargesDescription
					{
						get { return Res.GetString("271e71a9-1dee-47c8-b4ef-d08a2571bf09", "Freight Charges"); }
					}
					public static string OriginChargesDescription
					{
						get { return Res.GetString("dd4aea77-27dc-483a-8e86-db75ed6f9b9d", "Origin Charges"); }
					}
					public static string DestinationChargesDescription
					{
						get { return Res.GetString("64001525-df51-4ce9-b042-e1a61e5af5a4", "Destination Charges"); }
					}

					public static string LoadingChargesDescription => Res.GetString("0db2b62a-9918-4ef7-bf00-0d6c3f089384", "Loading Charges");

					public static string OriginCustomsBrokerageChargesDescription => Res.GetString("8369af06-99f3-4cba-b507-07bb9dc58901", "Origin Customs Brokerage Charges");

					public static string CustomsBrokerageChargesDescription => Res.GetString("e3fec9f2-d600-46cf-a159-56d6ba240c48", "Customs Brokerage Charges");

					public static string UnloadingChargesDescription => Res.GetString("94328a45-30a5-4ead-8183-37970705ef6d", "Unloading Charges");
				}

				#endregion

				public const string CalculationOrder = "SEQ";
				public static string CalculationOrderDesc
				{
					get { return Res.GetString("4db31f92-dfbb-42e9-8e0e-b79a77817941", "Calculation Priority"); }
				}

				public const string Multiple = "MUL";
			}

			#endregion
		}

		#endregion

		#region Quotation Landscape Supportable

		public abstract bool CanBePrintedUsing6StandardOperatorColumnHeaders { get; }

		#endregion

		#region Generic Properties

		public virtual ZBool IsAccumulated
		{
			get { return false; }
			set { }
		}

		public virtual ZBool UseInclusiveBreaks
		{
			get { return false; }
			set { }
		}

		/// <summary>
		///		A flag indicating how per equipment (per container) rate has to be calculated.
		///		A user can specify the number of containers on a container in the grid. But, the actual number
		///		of containers required for the weight/volume specified on a container can be different.
		///
		///		It depends on the container payload and the weight/volume specified by the user. For example, a user can
		///		specify the following container:
		///		5 x 20GP Containers, 5000 KG
		///
		///		But, if 20GP container has 800 KG payload, it will require at least 7 containers to move 5000 KG:
		///		5000 / 800 = 6.25 => 7
		///
		///		If the flag is set, it means we want to calculate based on the calculated number of containers rather than
		///		the actual specified by the user.
		/// </summary>
		public virtual ZBool MultipleEquipmentsOverMaxWeightVolume
		{
			get { return false; }
			set { }
		}

		public virtual bool IsJobLevelAvailable
		{
			get { return true; }
		}

		public static bool IsSliding(IEnumerable<IRateLineItem> items)
		{
			return items.Any(x => x.TM_Type == Items.Operator.Minus || x.TM_Type == Items.Operator.Plus);
		}

		public bool IsSliding()
		{
			return IsSliding(Line.ChildRateLineItems);
		}

		public bool IsSlidingForBinding
		{
			get { return isSlidingForBinding ?? IsSliding(); }
			set { isSlidingForBinding = value; }
		}

		bool? isSlidingForBinding;

		public virtual bool IsMeasureTypeMatchApplicable => true;

		public virtual string DefaultWeightVolume => "";

		public virtual bool IsProductAllowed => true;

		public virtual TimeInfo.Exclusion ExcludedHolidays => 0;

		public virtual ZString BreaksPer { get; set; }

		internal ZPropertyInfo BreaksPerInfo
		{
			get { return ((IGetZPropertyInfo)RateLineBizO).GetZPropertyInfo("Calculator+String3"); }
		}

		#region Equipment Type

		public bool ShowEquipmentType
		{
			get { return ShowEquipmentTypeInternal && (ParentRateEntry == null || (!ParentRateEntry.IsWHS() && !ParentRateEntry.IsTRW() && !ParentRateEntry.IsTWU())); }
		}

		protected virtual bool ShowEquipmentTypeInternal
		{
			get { return false; }
		}

		protected virtual bool HasDuplicateEquipmentType(RateLine line)
		{
			return line.PK != Line.PK && line.TL_AC == Line.TL_AC && line.Calculator.ShowEquipmentType &&
					line.Calculator.EquipmentType == EquipmentType;
		}

		public virtual ZString EquipmentType
		{
			get { return ZString.Empty; }
			set { }
		}

		public bool HasDuplicateEquipmentTypes => RateLineBizO.Parent.RateLines.Cast<RateLine>().Any(HasDuplicateEquipmentType);

		#endregion

		#region Message Type/SubType

		public bool ShowMessageTypeSubType
		{
			get { return ShowMessageTypeSubTypeInternal; }
		}

		protected virtual bool ShowMessageTypeSubTypeInternal
		{
			get { return false; }
		}

		public virtual ZString MessageType
		{
			get { return ZString.Empty; }
			set { }
		}

		public virtual ZString MessageSubType
		{
			get { return ZString.Empty; }
			set { }
		}

		#endregion

		#endregion

		#region Property Indexer

		public IZType this[string propertyName]
		{
			get
			{
				var attribute = GetAttribute(propertyName);
				return attribute != null ? attribute.GetValue(this, GetBreak(propertyName)) : null;
			}
			set
			{
				var attribute = GetAttribute(propertyName);
				if (attribute != null)
				{
					attribute.SetValue(this, GetBreak(propertyName), value);
				}
			}
		}

		#endregion

		#region Validation

		#region TM_Type

		public void ValidateTM_Type(RateLineItem item)
		{
			ValidateTM_TypeCore(item);
		}

		protected virtual void ValidateTM_TypeCore(RateLineItem item)
		{
		}

		#endregion

		#region TM_Value

		public virtual void ValidateTM_Value(RateLineItem lineItem)
		{
			if (ShowWarningIfEmpty(lineItem))
			{
				lineItem.TM_ValueInfo.AddWarning(ErrorMessages.RatePriceIsZero);
			}
		}

		#endregion

		#region TM_FlatAmount

		public virtual void ValidateTM_FlatAmount(RateLineItem lineItem)
		{
		}

		#endregion

		#region TM_AgentDeclaredRate

		public virtual void ValidateTM_AgentDeclaredRate(RateLineItem lineItem)
		{
		}

		#endregion

		#region TM_Break

		public void ValidateTM_Break(RateLineItem item)
		{
			ValidateTM_BreakCore(item);
		}

		protected virtual void ValidateTM_BreakCore(RateLineItem item)
		{
			new WeightBreakValidator(RateLineBizO.RateLineItems, item).CheckWeightBreak();
		}

		internal virtual bool TM_BreakEditable(RateLineItem item)
		{
			return item.RequiresWeightBreak();
		}

		#endregion

		#region TM_BreakWeightVolume

		public virtual void ValidateTM_BreakWeightVolume(RateLineItem lineItem)
		{
		}

		#endregion

		#region TM_BreakMinimum

		public virtual void ValidateTM_BreakMinimum(RateLineItem lineItem)
		{
		}

		#endregion

		#region TM_Text

		public virtual void ValidateTM_Text(RateLineItem lineItem)
		{
			if (!RateLineBizO.IsBulkRateUpdateActionLine)
			{
				ValidateEquipmentType(lineItem);
				ValidateMessageTypeSubType(lineItem);
			}
		}

		void ValidateEquipmentType(RateLineItem lineItem)
		{
			if (ShowEquipmentType && lineItem.RateOperatorIsEquipmentType())
			{
				if (!RateLineBizO.UsesCompanyTariffOrCostBasedCalculator())
				{
					MandatoryValidation.CheckEntered(lineItem.TM_TextInfo);
				}

				if (lineItem.TM_Text.IsEmpty)
				{
					return;
				}

				ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, RateLineBizO.Lookups.EquipmentTypes);

				if (HasDuplicateEquipmentTypes)
				{
					lineItem.TM_TextInfo.AddError(ErrorMessages.EquipmentTypeDuplicate);
				}
			}
		}

		void ValidateMessageTypeSubType(RateLineItem lineItem)
		{
			if (ShowMessageTypeSubType && (
				lineItem.TM_Type == AgencyCalculator.Items.MessageType ||
				lineItem.TM_Type == AgencyCalculator.Items.MessageSubType))
			{
				switch (lineItem.TM_Type)
				{
					case AgencyCalculator.Items.MessageType:
						ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, RateLineBizO.Lookups.MessageTypeList);
						break;

					case AgencyCalculator.Items.MessageSubType:
						ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, RateLineBizO.Lookups.MessageSubTypeList);
						break;
				}

				if (!RateLineBizO.UsesCompanyTariffOrCostBasedCalculator())
				{
					foreach (var rateLine in ParentRateEntry.ChildRateLines.Cast<RateLine>())
					{
						if (rateLine.PK != Line.PK && rateLine.TL_AC == Line.TL_AC && rateLine.Calculator.ShowMessageTypeSubType)
						{
							if (MessageType == rateLine.Calculator.MessageType && MessageSubType == rateLine.Calculator.MessageSubType)
							{
								lineItem.TM_TextInfo.AddError(ErrorMessages.MessageTypeOrSubTypeDuplicate);
								break;
							}
						}
					}
				}
			}
		}

		#endregion

		#region TM_AC

		public virtual void ValidateTM_AC(RateLineItem lineItem)
		{
		}

		#endregion

		#region Grid Validation

		protected virtual IEnumerable<RateLineItem> GetItemsForGridValidation(RateLineItem lineItem)
		{
			return RateLineBizO.RateLineItems.Cast<RateLineItem>();
		}

		protected void ValidateGridCalculator(RateLineItem currentItem, bool enableUNT, bool enableBAS, bool enableMAX)
		{
			if (currentItem.RateOperatorIsNonPrintedFlag() || currentItem.TM_TypeInfo.HasErrors())
			{
				return;
			}

			var currentItemIndex = RateLineBizO.RateLineItems.IndexOf(currentItem);
			if (currentItemIndex < 0)
			{
				return;
			}

			var currentItemType = currentItem.TM_Type;
			var uniqueOperators = new List<string> { Items.Operator.MIN, Items.Operator.MAX, Items.Operator.BAS, Items.Operator.Minus, Items.Operator.UNT };

			if (uniqueOperators.Contains(currentItemType))
			{
				ValidateTM_TypeDuplicates(currentItem, currentItemType, ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed);
			}

			switch (currentItemType)
			{
				case Items.Operator.MAX:
					if (!enableMAX)
					{
						currentItem.TM_TypeInfo.AddError(ErrorMessages.MAXNotAllowed);
					}
					break;

				case Items.Operator.UNT:
					if (!enableUNT)
					{
						currentItem.TM_TypeInfo.AddError(ErrorMessages.UNTNotAllowed);
					}

					var allGridItems = GetItemsForGridValidation(currentItem);
					if (allGridItems.Any(r => r.RateOperatorIsMinus() || r.RateOperatorIsPlus()))
					{
						currentItem.TM_TypeInfo.AddError(ErrorMessages.UNTIncompatibleWithSlidingItems);
					}
					break;

				case Items.Operator.BAS:
					if (!enableBAS)
					{
						currentItem.TM_TypeInfo.AddError(ErrorMessages.BASNotAllowed);
					}
					break;

				case Items.Operator.Minus:
					allGridItems = GetItemsForGridValidation(currentItem);
					if (!allGridItems.Any(r => r.RateOperatorIsPlus()))
					{
						currentItem.TM_TypeInfo.AddError(ErrorMessages.MoreLinesRequired);
					}

					if (allGridItems.Any(r => r.RateOperatorIsUNT()))
					{
						currentItem.TM_TypeInfo.AddError(ErrorMessages.UNTIncompatibleWithSlidingItems);
					}
					break;

				case Items.Operator.Plus:
					allGridItems = GetItemsForGridValidation(currentItem);
					if (allGridItems.Any(r => r.RateOperatorIsUNT()))
					{
						currentItem.TM_TypeInfo.AddError(ErrorMessages.UNTIncompatibleWithSlidingItems);
					}
					break;

				case Items.Operator.MIN:
					break;

				default:
					currentItem.TM_TypeInfo.AddError(ErrorMessages.InvalidRateOperator);
					break;
			}
		}

		protected void ValidateTM_TypeDuplicates(RateLineItem item, ZString itemType, string errorMessage)
		{
			if (!item.TM_TypeInfo.HasErrors())
			{
				var hasDuplicates = false;

				if (item.HasBeenFoundAsTM_TypeDuplicateByAnotherValidation)
				{
					hasDuplicates = true;
				}
				else
				{
					var allGridItems = GetItemsForGridValidation(item);
					var duplicates = allGridItems.Where(x => x != item && x.TM_Type == itemType);

					foreach (var duplicate in duplicates)
					{
						hasDuplicates = true;
						duplicate.HasBeenFoundAsTM_TypeDuplicateByAnotherValidation = true;
						duplicate.Validation.ValidateTM_Type();
						duplicate.HasBeenFoundAsTM_TypeDuplicateByAnotherValidation = false;
					}
				}

				if (hasDuplicates)
				{
					item.TM_TypeInfo.AddError(errorMessage);
				}
			}
		}

		#endregion

		#region Empty Value Warning

		bool ShowWarningIfEmpty(RateLineItem item)
		{
			if (item.RateOperatorIsNonPrintedFlag())
			{
				return false;
			}

			CalculatorPropertyAttribute attribute;
			if (ItemTypeOrFieldNameToAttribute.TryGetValue(item.TM_Type, out attribute) &&
				(!attribute.ShowWarningIfEmpty || (!string.IsNullOrEmpty(attribute.MapTo) && (!attribute.MapTo.StartsWith((NoResString)"Decimal", StringComparison.OrdinalIgnoreCase) || !attribute.MapTo.StartsWith((NoResString)"Int", StringComparison.OrdinalIgnoreCase))))) // Hard-coded header constant
			{
				return false;
			}

			return item.TM_Value.IsEmpty && item.TM_FlatAmount.IsEmpty && !item.TM_ValueInfo.ReadOnly && !(item.Parent != null && item.Parent.ReadOnly);
		}

		#endregion

		#region Validate Rate Line Items If Exists

		protected bool ValidateRateLineItemsIfExists(params ZString[] itemTypes)
		{
			var rateLineItems = itemTypes.Select(FindRateLineItem)
				.Where(x => x != null)
				.ToList();

			foreach (var rateLineItem in rateLineItems)
			{
				rateLineItem.Validation.ValidateAll();
			}

			return rateLineItems.All(x => !x.HasErrors());
		}

		#endregion

		#endregion

		#region Quotation / Calculation Description

		#region Quotation Lines

		[Flags]
		public enum GetQuotationLinesParam
		{
			None = 0x00,
			ShowEquipmentType = 0x01,
			ShowStartEndDates = 0x02,
			IncludeContractNumbers = 0x04,
			IncludeNotes = 0x08,
			AlternativeFormat = 0x10,
			UseChargeDescription = 0x20,
		}

		public static QuotationLineType RateDescriptionFlags(GetQuotationLinesParam parameters)
		{
			var result = QuotationLineType.Mandatory;

			if ((parameters & Calculator.GetQuotationLinesParam.UseChargeDescription) != 0)
			{
				result |= QuotationLineType.UseChargeDescription;
			}
			else
			{
				result |= QuotationLineType.UseRateLineDescription;
			}

			return result;
		}

		public static bool ShouldAddContainerCode(QuotationLineType type) => ((type & QuotationLineType.AlternativeFormat) != 0)
			|| DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;

		bool gettingQuotationLines;

		public QuotationLineList GetQuotationLines(GetQuotationLinesParam flags, IRateEntry parentEntry)
		{
			gettingQuotationLines = true;

			try
			{
				var result = GetQuotationLinesInternal(flags);
				var descriptionLine = result.FirstOrDefault(line => (line.Type & QuotationLineType.UseDescriptionMask) != 0);

				if (descriptionLine != null)
				{
					descriptionLine.MergeDescriptions(GetAdditionalDescription(flags, parentEntry));

					if ((flags & GetQuotationLinesParam.ShowStartEndDates) != 0)
					{
						descriptionLine.AddDescription(GetStartEndDateString());
					}
				}

				if (result.Count > 0 && (flags & GetQuotationLinesParam.IncludeContractNumbers) != 0)
				{
					var line = QuotationLine.ContractNumber(Line);

					if (line != null)
					{
						result.Add(line);
					}
				}

				var hasNote = false;

				if (result.Count > 0 && (flags & GetQuotationLinesParam.IncludeNotes) != 0)
				{
					var line = QuotationLine.RateNote(Line);

					if (line != null)
					{
						hasNote = true;
						result.Add(line);
					}
				}

				if (!hasNote && result.Count > 0 && !Line.TL_Condition.IsEmpty)
				{
					var line = QuotationLine.ConditionsApply(Line);
					if (line != null)
					{
						result.Add(line);
					}
				}

				return result;
			}
			finally
			{
				gettingQuotationLines = false;
			}
		}

		protected virtual QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			return new QuotationLineList();
		}

		public virtual DocLineAmount GetDocLineAmount()
		{
			return new DocLineAmount();
		}

		public ZString GetAdditionalDescription(GetQuotationLinesParam flags, IRateEntry parentEntry)
		{
			var builder = new StringBuilder();

			var equipment = GetEquipmentTypeString((flags & GetQuotationLinesParam.ShowEquipmentType) != 0);
			if (!equipment.IsEmpty)
			{
				AppendDescription(builder, equipment, equipment, true);
			}

			if (parentEntry != null && parentEntry.IsFreightEntry() && ParentRateEntry != null && !ParentRateEntry.IsFreightEntry())
			{
				if (ParentRateEntry.TI_RS_NKServiceLevel_NI != parentEntry.TI_RS_NKServiceLevel_NI)
				{
					AppendDescription(builder, ParentRateEntry.TI_RS_NKServiceLevel_NI, ParentRateEntry.TI_RS_NKServiceLevel_NI, false);
				}
				if (ParentRateEntry.TI_RH_NKCommodityCode != parentEntry.TI_RH_NKCommodityCode)
				{
					AppendDescription(builder, ParentRateEntry.TI_RH_NKCommodityCode, ParentRateEntry.TI_RH_NKCommodityCode, false);
				}

				if (ParentRateEntry.TransportProvider != null)
				{
					if ((DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.Value && parentEntry.IsAirFreight())
						|| (DocumentsDataRegistry.Instance.SeaFreightIncludeTransportProviderOnQuotation.Value && parentEntry.IsSeaFreight()))
					{
						AppendDescription(builder, ParentRateEntry.TransportProvider.OH_FullNameTruncated,
							Res.GetString("c9d79bbc-c301-4c8f-96b9-1074f4bb8c98", "Transport Provider: {0}", ParentRateEntry.TransportProvider.OH_FullNameTruncated), true);
					}
				}
			}

			var cartageAddress = GetCartageAddressString();
			AppendDescription(builder, cartageAddress, cartageAddress, true);

			var part = GetPartString();
			AppendDescription(builder, part, Res.GetString("90a6be12-16e6-43a0-9687-86f87142aff3", "for {0}", part), true);

			if (ParentRateEntry != null)
			{
				AppendDescription(builder, ParentRateEntry.TI_PL_NKCarrierServiceLevel,
					Res.GetString("7d2f65a6-9f0f-4d91-ada9-e1090ad8122c", "Carrier Service Level: {0}", ParentRateEntry.TI_PL_NKCarrierServiceLevel), true);

				if (ParentRateEntry.Warehouse() != null)
				{
					var warehouseName = ParentRateEntry.Warehouse().WW_WarehouseName.ToString();
					AppendDescription(builder, warehouseName, Res.GetString("7ca87202-d3ea-41a4-8799-76c5e251567f", "Warehouse: {0}", warehouseName), true);
				}
			}

			return builder.ToString();
		}

		static void AppendDescription(StringBuilder builder, ZString additionalInfo, string appendText, bool startNewLine)
		{
			if (!additionalInfo.IsEmpty)
			{
				if (startNewLine)
				{
					builder.Append(System.Environment.NewLine + "-  ");
				}
				else
				{
					builder.Append(' ');
				}

				builder.Append(appendText ?? additionalInfo);
			}
		}

		ZString GetPartString()
		{
			var part = Line.ProductNumber();

			if (part != null)
			{
				return part.OP_PartNum + " (" + part.OP_Desc + ")";
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected ZString GetCartageAddressString()
		{
			if (ParentRatingHeader?.TH_OneTimeQuote ?? false)
			{
				if (ParentRateEntry.IsOriginEntry())
				{
					return GetCartageAddressString(RateEntrySchema.TI_OH_Consignor);
				}
				else if (ParentRateEntry.IsDestinationEntry())
				{
					return GetCartageAddressString(RateEntrySchema.TI_OH_Consignee);
				}
			}

			var consignorPart = GetCartageAddressString(RateEntrySchema.TI_OH_Consignor);
			var consigneePart = GetCartageAddressString(RateEntrySchema.TI_OH_Consignee);

			if (consignorPart.IsEmpty)
			{
				return consigneePart;
			}
			if (consigneePart.IsEmpty)
			{
				return consignorPart;
			}
			else
			{
				return consignorPart + " " + consigneePart;
			}
		}

		ZString GetCartageAddressString(SchemaColumn orgColumn)
		{
			if (ParentRateEntry == null || !(ParentRateEntry is RateEntry parentRateEntry))
			{
				return ZString.Empty;
			}
			else
			{
				OrgHeader org;
				OrgAddress cartageAddress = null;
				SchemaColumn postcodeColumn;
				SchemaColumn zoneColumn;
				ZGuid suburbPK;
				ZString direction;

				if (orgColumn == RateEntrySchema.TI_OH_Consignor)
				{
					org = parentRateEntry.Consignor;

					if (ParentRateEntry != null && ParentRatingHeader.TH_OneTimeQuote && ParentRatingHeader is Quote oneOffQuote)
					{
						cartageAddress = oneOffQuote.CurrentOneOffQuote.PickUpDocAddress.Address;
					}
					else if (parentRateEntry.Lookups.CartagePickupAddressOverrides.Count > 1 || parentRateEntry.CartagePickupAddressOverride != null)
					{
						cartageAddress = parentRateEntry.CartagePickupAddressOverride;
					}

					postcodeColumn = RateEntrySchema.TI_CartagePickupAddressPostCode;
					zoneColumn = RateEntrySchema.TI_TZ_OriginZone;
					suburbPK = ParentRateEntry.OriginSuburbPK;
					direction = Res.GetString("916c1060-fc18-4f81-9b60-a3aaeab5fd5b", "From");
				}
				else if (orgColumn == RateEntrySchema.TI_OH_Consignee)
				{
					org = parentRateEntry.Consignee;

					if (ParentRatingHeader != null && ParentRatingHeader.TH_OneTimeQuote && ParentRatingHeader is Quote oneOffQuote)
					{
						cartageAddress = oneOffQuote.CurrentOneOffQuote.DeliveryDocAddress.Address;
					}
					else if (parentRateEntry.Lookups.CartageDeliveryAddressOverrides.Count > 1 || parentRateEntry.CartagePickupAddressOverride != null)
					{
						cartageAddress = parentRateEntry.CartageDeliveryAddressOverride;
					}

					postcodeColumn = RateEntrySchema.TI_CartageDeliveryAddressPostCode;
					zoneColumn = RateEntrySchema.TI_TZ_DestinationZone;
					suburbPK = Line.ParentRateEntry.DestinationSuburbPK;
					direction = Res.GetString("6c650bef-537d-4796-93ab-96069880a4f5", "To");
				}
				else
				{
					return ZString.Empty;
				}

				var builder = new StringBuilder();

				string addressString = GetAddressString(cartageAddress, postcodeColumn, zoneColumn, suburbPK);

				if (!string.IsNullOrEmpty(addressString))
				{
					builder.Append(addressString);
				}

				if (org != null && ParentRatingHeader?.TH_OH != org.PK)
				{
					if (builder.Length > 0)
					{
						builder.Append(' ');
						builder.Append(Res.GetString("e1c4aa2e-106d-4583-b1bc-7513762f3e8e", "for"));
						builder.Append(' ');
					}

					builder.Append(org.OH_FullNameTruncated);
				}

				if (builder.Length > 0)
				{
					return direction + " " + builder;
				}

				return ZString.Empty;
			}
		}

		ZString GetAddressString(OrgAddress address, SchemaColumn postcodeColumn, SchemaColumn zoneColumn, ZGuid suburbPK)
		{
			var builder = new StringBuilder();
			var postcode = ZString.Empty;
			var zone = ZString.Empty;
			var suburb = ZString.Empty;

			if (address != null)
			{
				builder.Append(address.OA_City);

				if (!address.OA_City.IsEmpty && !address.OA_State.IsEmpty)
				{
					builder.Append(", ");
				}

				builder.Append(address.OA_State);
				postcode = address.OA_PostCode;
			}
			else
			{
				postcode = (ZString)RateLineBizO.Parent[postcodeColumn.Name];

				var zonePK = (ZGuid)RateLineBizO.Parent[zoneColumn.Name];
				if (zonePK.IsValid)
				{
					zone = RateLineBizO.Factory.Load<RateTransportZone>(zonePK).TZ_ZoneName;
				}

				if (suburbPK.IsValid)
				{
					suburb = RateLineBizO.Factory.Load<RefCityTown>(suburbPK).Description;
				}
			}

			if (!zone.IsEmpty)
			{
				if (builder.Length > 0)
				{
					builder.Append(' ');
				}

				builder.Append(zone);
			}

			if (!suburb.IsEmpty)
			{
				if (builder.Length > 0)
				{
					builder.Append(' ');
				}

				builder.Append(suburb);
			}

			if (!postcode.IsEmpty)
			{
				if (builder.Length > 0)
				{
					builder.Append(' ');
				}

				if (zone.IsEmpty && suburb.IsEmpty)
				{
					builder.Append(Res.GetString("9af64f93-a475-4d3c-875b-1d99c918c263", "Postcode {0}", postcode));
				}
				else
				{
					builder.Append(postcode);
				}
			}

			return builder.ToString();
		}

		ZString GetEquipmentTypeString(bool forceShowEquipmentType)
		{
			if (ParentRateEntry != null && !EquipmentType.IsEmpty && (forceShowEquipmentType || ShouldShowEquipmentType))
			{
				return RateLineBizO.Lookups.EquipmentTypes.GetDescriptionFromCode(EquipmentType);
			}
			else
			{
				return ZString.Empty;
			}
		}

		bool ShouldShowEquipmentType
		{
			get
			{
				var defaultEquipment = ParentRateEntry.IsFCL() ? Constants.FCLEquipmentNeeded.WaitForUnpack : Constants.LCLAIREquipmentNeeded.Premise;
				if (EquipmentType == defaultEquipment)
				{
					foreach (var rateLine in ParentRateEntry.ChildRateLines)
					{
						if (rateLine.PK != Line.PK && rateLine.Calculator.ShowEquipmentType && EquipmentType != rateLine.Calculator.EquipmentType)
						{
							return true;
						}
					}

					return false;
				}

				return true;
			}
		}

		ZString GetStartEndDateString()
		{
			if (ParentRateEntry != null && !ParentRateEntry.TI_RateStartDate.IsEmpty)
			{
				if (!ParentRateEntry.TI_RateEndDate.IsEmpty)
				{
					return string.Format(CultureInfo.InvariantCulture, "({0} - {1})", ParentRateEntry.TI_RateStartDate.ToShortDateString(), ParentRateEntry.TI_RateEndDate.ToShortDateString());
				}

				return Res.GetString("0bc9ceba-e342-4ac9-bc4d-e2da53f5e576", "(from {0})", ParentRateEntry.TI_RateStartDate.ToShortDateString());
			}

			return ZString.Empty;
		}

		#endregion

		#region Unit Description

		protected internal virtual MultilingualString UnitDescription(bool showConversionFactor, ZString unit = default, bool addContainerCode = false)
		{
			var currentUnit = string.IsNullOrEmpty(unit) ? Unit : unit;
			return UnitDescriptionInternal(currentUnit, UnitMultiplier, showConversionFactor, false, addContainerCode);
		}

		public MultilingualString UnitDescriptionInternal(ZString unit, Decimal? unitMultiplier = null, bool showConversionFactor = false, bool addPlural = false, bool addContainerCode = false)
		{
			MultilingualString result = (NoResString)ZString.Empty;
			if (unitMultiplier.HasValue && unitMultiplier.Value != 0 && unitMultiplier.Value != 1)
			{
				result = (NoResString)(unitMultiplier.Value.ToString("f0", Culture.CurrentCompanyCountryCulture) + " ");
			}

			switch (unit)
			{
				case QuantityUnit.SV:
					var chargeCode = Line.ChargeCode;
					var group = chargeCode.Lookups.ChargeGroupList.GetMultilingualDescriptionFromCode(chargeCode.AC_ChargeGroup);
					var subGroupOrService = chargeCode.Lookups.ChargeSubGroupList.GetMultilingualDescriptionFromCode(chargeCode.AC_ChargeSubGroup);
					if (string.IsNullOrEmpty(subGroupOrService))
					{
						subGroupOrService = addPlural ? ResString.GetMultilingualString("e2f8be7c-0381-4eea-8a31-f1f31e24198f", "Service(s)") : ResString.GetMultilingualString("9e48d793-310b-46d1-b3a5-264aa18bf3b3", "Service");
					}
					result = MultilingualString.Join("", result, group, (NoResString)" ", subGroupOrService);
					break;

				case QuantityUnit.CN:
					var containerCode = addContainerCode ? GetContainerCodeForUnitDescription() : ZString.Empty;
					var unitDescription = GetContainerUnitDescription(containerCode, addPlural);
					result = MultilingualString.Join("", result, unitDescription);
					break;

				case QuantityUnit.PL:
					result = GetUnitDescription(result, Constants.PkgUnit.Pallet, showConversionFactor, addPlural);
					break;

				default:
					result = GetUnitDescription(result, unit, showConversionFactor, addPlural);
					break;
			}

			return result;
		}

		protected MultilingualString GetContainerUnitDescription(string containerCode, bool isPlural)
		{
			var containerText = isPlural
				? ResString.GetMultilingualString("e086e1f5-f6a2-4fd5-95cf-6e4eeb5f940b", "Container(s)")
				: ResString.GetMultilingualString("d49c9398-494f-4a92-b352-7004bff86410", "Container");

			var result = string.IsNullOrEmpty(containerCode)
				? containerText
				: MultilingualString.Join(" ", (NoResString)containerCode, containerText);

			return result;
		}

		#region Get Container Code For Description

		ZString GetContainerCodeForUnitDescription()
		{
			var container = ParentRateEntry?.Container;
			if (container == null)
			{
				return ZString.Empty;
			}

			var containerOwnership = string.Empty;
			if (Line.TL_WeightVolume == QuantityUnit.CN && !Line.TL_ContainerOwnership.IsEmpty)
			{
				containerOwnership = RateLinesLookups.GetContainerOwnershipsDescription(Line);
			}

			var code = string.IsNullOrEmpty(containerOwnership)
				? container.RC_Code
				: (ZString)FormattableString.Invariant($"{container.RC_Code} {containerOwnership}");

			return code;
		}

		#endregion

		#region Get Unit Description

		MultilingualString GetUnitDescription(MultilingualString result, ZString unit, bool showConversionFactor, bool addPlural)
		{
			var isWeightOrVolume = Constants.Volume.ContainsCode(unit) || Constants.Weight.ContainsCode(unit);
			if (isWeightOrVolume && gettingQuotationLines && DocumentsDataRegistry.Instance.FreightChargesConversionFactorDisplayOption.Value == "W/M")
			{
				return (NoResString)"W/M";
			}

			result = MultilingualString.Join("", result, ConvertUnitToDescriptionIfNecessary(unit, addPlural, isWeightOrVolume));

			if (showConversionFactor && isWeightOrVolume && ParentRateEntry != null && !ParentRateEntry.IsFCL() && !Line.UseOnlyActualWeightMeasure())
			{
				result = MultilingualString.Join("", result, (NoResString)GetConversionFactorString(unit, showConversionFactor));
			}

			return result;
		}

		MultilingualString ConvertUnitToDescriptionIfNecessary(ZString unit, bool addPlural, bool isWeightOrVolume)
		{
			var getUnitDescription = !isWeightOrVolume || (addPlural && !gettingQuotationLines);
			if (getUnitDescription)
			{
				var pluralState = addPlural ? Constants.PluralState.PluralOrNonPlural : Constants.PluralState.NonPlural;
				var result = Constants.PkgUnit.GetDescription(unit, pluralState);
				if (!string.IsNullOrEmpty(result))
				{
					return result;
				}

				result = QuantityUnit.GetDescription(unit, ParentRateEntry.RateType(), pluralState);
				if (!string.IsNullOrEmpty(result))
				{
					return result;
				}

				var units = UnitHelper.GetUnits(ParentRateEntry, ParentRateEntry.Country().RN_Code, ParentRateEntry.Factory, pluralState);
				if (units.ContainsCode(unit))
				{
					return units.GetMultilingualDescriptionFromCode(unit);
				}
			}

			return (NoResString)unit;
		}

		#endregion

		#region Conversion Factor Description

		#region SuppressResourceStringsCheckRegion

		ZString GetConversionFactorString(ZString unit, bool showConversionFactor = false)
		{
			ConversionFactor conversionFactor;
			if (Line is RateLine rateline)
			{
				conversionFactor = showConversionFactor ? rateline.ConversionFactorForDocumentPrintingOnly : rateline.ConversionFactor;
			}
			else
			{
				conversionFactor = Line.ConversionFactor;
			}

			if (conversionFactor.IsEmpty)
			{
				return ZString.Empty;
			}

			if (conversionFactor.Factor != 0)
			{
				if (conversionFactor.DenominatorUnit == unit)
				{
					return string.Format(Culture.CurrentCompanyCountryCulture, " / {0:f0} {1}", conversionFactor.Factor, conversionFactor.NumeratorUnit);
				}
				else
				{
					return string.Format(Culture.CurrentCompanyCountryCulture, " ({0})", conversionFactor.ToLongString());
				}
			}

			return string.Empty;
		}

		#endregion

		#endregion

		#endregion

		#region Item Break Description

		protected ZString ItemBreakDescription(IRateLineItem item)
		{
			if (item.RateOperatorIsMinus())
			{
				return Res.GetString("ad21cede-a4c9-4233-b6aa-0409dd23fc7f", "{0} {1}",
						UseInclusiveBreaks
							? (ZString)Res.GetString("5e680e36-6e52-44c1-a9f8-897fe068a99f", "Up to")
							: LessThanText,
						CurrencyAndBreakUnitDescription(item.TM_Break));
			}

			if (item.IsOverPivotRate())
			{
				return Res.GetString("4680e27c-65f6-4cba-8506-56567a45f96f", "Over Pivot Rate");
			}

			if (item.RateOperatorIsPlus())
			{
				var breakItemDescription = CurrencyAndBreakUnitDescription(item.TM_Break);
				var nextbreakItemDescription = CurrencyAndBreakUnitDescription(item.NextWeightBreak());

				return UseInclusiveBreaks
					? item.IsHighestPlus()
						? Res.GetString("2c1cf4d3-29cd-4911-9ff2-79cb109cad4c", "{0} {1}", MoreThanText, breakItemDescription)
						: Res.GetString("157549a5-f4c3-477d-bd7c-53a32de13c17", "{0} {1} to {2}", MoreThanText, breakItemDescription, nextbreakItemDescription)
					: item.IsHighestPlus()
						? Res.GetString("e2269438-4d00-4585-b62a-e9097639bae5", "{0} and above", breakItemDescription)
						: Res.GetString("f9a9c0ee-298f-4e91-b2dd-5a1912aa1405", "{0} to {1} {2}", breakItemDescription, LessThanText.ToLower(), nextbreakItemDescription);
			}

			return string.Empty;
		}

		string CurrencyAndBreakUnitDescription(ZDecimal unitBreakAmount)
		{
			var currencySymbol = ZString.Empty;
			var extraDescription = ZString.Empty;

			var breakUnitDescription = BreakUnitDescription;
			if (breakUnitDescription == Line.TL_RX_NKCurrency)
			{
				currencySymbol = CurrencySymbol(Line.TL_RX_NKCurrency);
			}
			else
			{
				extraDescription = " " + breakUnitDescription;
			}

			return string.Format(Culture.CurrentCompanyCountryCulture, "{0}{1}{2}", currencySymbol, unitBreakAmount.ToStringTrimZeros(), extraDescription);
		}

		protected virtual ZString BreakUnitDescription
		{
			get { return UnitDescriptionInternal(BreakUnit, addPlural: true); }
		}

		protected virtual ZString LessThanText
		{
			get { return Res.GetString("206e6d47-d798-4849-b570-d8871645e32b", "Less than"); }
		}

		protected virtual ZString MoreThanText
		{
			get { return Res.GetString("e9aa3f66-8805-4d16-ada4-e8a8b19b0130", "More than"); }
		}

		#endregion

		#region Percentage Unit

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Strings selection, it is not that complex")]
		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected MultilingualString GetPercentageUnit(IRateLineItem item)
		{
			switch (item.TM_Text)
			{
				case CalculatorConstants.Text.ChargeCode:
					if (item.TM_AC == FreightChargeCode)
					{
						return ResString.GetMultilingualString("e58d64fa-8713-4094-a001-06b7594a2b7b", "% of Freight");
					}

					var percentOf = item.ParentRateLine.Factory.Load<AccChargeCode>(item.TM_AC);

					if (percentOf != null)
					{
						return ResString.GetMultilingualString("91d37466-7da4-496b-9826-7b68317b6ff1", "% of {0}", GetChargeDescOrDescLocalOrMultilingual(percentOf));
					}
					break;

				case CalculatorConstants.Text.AllCharges:
					return ResString.GetMultilingualString("aaa16458-49f9-455d-a854-e643de1935a9", "% of all charges");

				case CalculatorConstants.Text.FreightCharges:
					return ResString.GetMultilingualString("70c7fe4d-48bb-4420-97c0-c3e55da92a02", "% of freight charges");

				case CalculatorConstants.Text.OriginCharges:
					return ResString.GetMultilingualString("f3f29446-5bad-4666-a485-f70dc2228162", "% of origin charges");

				case CalculatorConstants.Text.DestinationCharges:
					return ResString.GetMultilingualString("51dfd7b6-6dd4-4902-b8e0-d89cba88f394", "% of destination charges");

				case CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods:
					return ResString.GetMultilingualString("71558dc9-97ae-4d9b-9449-11b508a34472", "% of value of goods");

				case CalculatorConstants.Text.ApplyTo.Value.InsuranceValue:
					return ResString.GetMultilingualString("4eb44d4c-4553-49f1-a929-d572e214dc45", "% of insurance value");

				case CalculatorConstants.Text.ApplyTo.Value.CustomsValue:
					return ResString.GetMultilingualString("d883218a-d5ce-4c73-ab13-7f8362af4725", "% of customs value");

				case CalculatorConstants.Text.ApplyTo.Value.InvoiceValue:
					return ResString.GetMultilingualString("ca3cb4c5-484e-498a-b788-7c64e5b6d64a", "% of invoice value");

				case Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement:
					return ResString.GetMultilingualString("72a4c2a6-a843-4da8-95c7-89ce8cebe87e", "% of customs disbursement");

				case Calculator.Items.Value.DisbursementApplyToTypes.Disbursements:
					return ResString.GetMultilingualString("1e0775bc-0ac8-4e80-9a21-83c2e85812e5", "% of disbursements");

				case CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount:
					return ResString.GetMultilingualString("64b99e24-304a-483e-8000-bbd88647746d", "% of single transaction bond amount");

				case CalculatorConstants.Text.LoadingCharges:
					return ResString.GetMultilingualString("865514BC-8089-4078-B60C-C7E199B47980", "% of loading charges");

				case CalculatorConstants.Text.OriginCustomsBrokerageCharges:
					return ResString.GetMultilingualString("5772E8FA-1098-445F-8682-7313DAA19BBD", "% of origin customs brokerage charges");

				case CalculatorConstants.Text.CustomsBrokerageCharges:
					return ResString.GetMultilingualString("E1CAECD4-1D6A-4727-862E-1335CDD4081B", "% of customs brokerage charges");

				case CalculatorConstants.Text.UnloadingCharges:
					return ResString.GetMultilingualString("ED8D099D-162E-4C88-9A16-BD237BCB58D1", "% of unloading charges");
			}

			return (NoResString)ZString.Empty;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Part of multilingual logic")]
		string GetChargeDescOrDescLocalOrMultilingual(AccChargeCode accChargeCode)
		{
			var hasLocalClient = ParentRatingHeader?.HasLocalClient() ?? false;
			var description = accChargeCode.GetDescriptionOrLocalDescription(accChargeCode.AC_Desc, hasLocalClient);
			return accChargeCode.GetMultilingualOrDefaultDescription(description);
		}

		#endregion

		#region Autorating Description

		public virtual ZString AutoRateDescription(AutoRatingCalculatorParameters parameters, bool isLocalDescription = false)
		{
			var rateLineDescription = isLocalDescription && !string.IsNullOrEmpty(Line.TL_RateDescLocal)
				? Line.TL_RateDescLocal
				: Line.TL_RateDesc;

			var result = new StringBuilder(ExpandMacros(parameters.Criteria, rateLineDescription));

			var chargeCode = Line.ChargeCode;
			if (chargeCode != null)
			{
				var product = parameters.ProductFilter;
				var productAttributes = parameters.ProductAttributesFilter;
				var docketReference = parameters.DocketReferenceFilter;

				if (!docketReference.IsEmpty && chargeCode.AC_ChargeGroup != ChargeCodeGroupList.Codes.WHSStorage)
				{
					result.AppendFormat(" {0}", docketReference);
				}

				if (product != null)
				{
					result.AppendFormat(" - {0} ({1})", product.OP_PartNum, product.OP_Desc);

					for (var i = 0; i < productAttributes.Length; i++)
					{
						if (productAttributes[i] != null && productAttributes[i].Length > 0)
						{
							result.AppendFormat(" {0}", productAttributes[i]);
						}
					}
				}

				try
				{
					if (((chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.CFSShipment &&
						chargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.Storage) ||
						chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSStorage)
						)
					{
						var timeInfo = parameters.Criteria.Time(chargeCode);
						if (timeInfo.SpanExcluding(ExcludedHolidays).TotalDays > 0)
						{
							var errorString = " " + Res.GetString("55c12945-0ff0-4143-8590-c98866a3def0", "for {0}", timeInfo.ToString(ExcludedHolidays));
							result.Append(errorString);
						}
					}
				}
				catch (CalculationException)
				{ }
			}

			return result.ToString();
		}

		static string ExpandMacros(IAutoRatingDescriptionMacroExpander expander, ZString inString)
		{
			if (!expander.CanExpandMacros)
			{
				return inString;
			}
			else
			{
				var regex = new Regex("\\{[a-z]*\\}", RegexOptions.IgnoreCase);
				return regex.Replace(inString, delegate(Match m)
				{
					string macro = inString.Substring(m.Index + 1, m.Length - 2).ToLower();
					return expander.ExpandMacro(macro) ?? m.ToString();
				});
			}
		}

		#endregion

		#region Currency Symbol

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Universal symbol")]
		const string EuroSymbol = "€";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Universal symbol")]
		const string PoundSymbol = "£";

		protected ZString CurrencySymbol(ZString currency)
		{
			switch (currency)
			{
				case Core.Constants.CurrencyCodes.Australia:
				case Core.Constants.CurrencyCodes.UnitedStates:
				case Core.Constants.CurrencyCodes.NewZealand:
					return "$";

				case Constants.CurrencyCodes.EuropeanUnion:
					return EuroSymbol;

				case Core.Constants.CurrencyCodes.UnitedKingdom:
					return PoundSymbol;

				default:
					return Line.TL_RX_NKCurrency + " ";
			}
		}

		#endregion

		#endregion

		#region Calculation

		#region Calculate

		public (IEnumerable<CalculationResult> results, string error) Calculate(AutoRatingCalculatorParameters parameters)
		{
			try
			{
				return CalculateResult(parameters);
			}
			catch (AutoRater.AutoRaterCalculationException ex1) when (parameters.Criteria.IsManualCostSelectMode)
			{
				return (Enumerable.Empty<CalculationResult>(), ex1.MessageForManualSelect);
			}
			catch (CalculationException ex2)
			{
				return (Enumerable.Empty<CalculationResult>(), ex2.Message);
			}
		}

		protected virtual (IEnumerable<CalculationResult> results, string error) CalculateResult(AutoRatingCalculatorParameters parameters)
		{
			if (Line.TL_RX_NKCurrency.IsEmpty && Line.TL_RateCalculator != FreightInclusiveCalculator.Code)
			{
				return (Enumerable.Empty<CalculationResult>(), (ZString)Res.GetString("12737fc1-b21a-4c02-97c5-ae14e2da8ed1", "the rate has no currency"));
			}

			var calcLog = new CalculationLog();

			var unit = GetUnit(parameters);
			InitializeCalculationLog(this, calcLog, parameters, Line, unit);
			var calculation = new CalculatorOutput(parameters, calcLog);

			CalculateInternal(calculation);

			if (calculation.IsEmpty || !string.IsNullOrEmpty(calculation.FailureMessage))
			{
				var error = calculation.FailureMessage.IsEmpty
					? (ZString)Res.GetString("2aae28bb-f1e6-427e-af5a-8a03e1b9792f", "incorrect or missing data")
					: calculation.FailureMessage;

				return (Enumerable.Empty<CalculationResult>(), error);
			}

			// Some calculators, such as HighestRateCalculator, set the unit as part of the calculation (yuk).
			// Should make it pass the unit in the CalculatorOuput one day.
			// For now, fetch the unit again.
			unit = GetUnit(parameters);

			AddBaseMinMax(calculation);

			if (!calculation.PaymentBases.Any())
			{
				return (Enumerable.Empty<CalculationResult>(), string.Empty);
			}

			if (Line.TL_UnitFactor.ToString().In(UnitFactorList.Codes.ProductLine, UnitFactorList.Codes.PackageLine))
			{
				var warehouseDescription = parameters.GetWarehouseDescription(Line);
				if (!string.IsNullOrEmpty(warehouseDescription))
				{
					var bases = new List<PaymentBasis>();
					foreach (var basis in calculation.PaymentBases)
					{
						var updatedBasis = basis.UpdateRateInfo(RateInfo.GetWithNewRateInfoDescription(basis.RateInfo, warehouseDescription));
						bases.Add(updatedBasis);
					}
					calculation.Set(bases);
				}
			}

			var calculationResult = new CalculationResult(Line, calculation, unit);
			SetAttributes(calculationResult, parameters, Line, unit);

			return (new[] { calculationResult }, string.Empty);
		}

		protected void AddBaseMinMax(CalculatorOutput calculation)
		{
			var parameters = calculation.Parameters;
			var bases = calculation.PaymentBases.ToList();

			AddBaseMinMaxPaymentBasis(parameters.Criteria, bases,
				currencyCode: Line.TL_RX_NKCurrency,
				isDependentCalculator: this is IDependentCalculator,
				baseRate: calculation.BaseRate,
				shouldAddBaseRateDescriptionEvenIfEmpty: ShouldAddBaseRateDescriptionEvenIfEmpty,
				minRate: calculation.Minimum,
				maxRate: calculation.Maximum);

			AddBaseMinMaxLog(calculation.CalculationLog,
				bases,
				baseRate: calculation.BaseRate,
				shouldAddBaseRateDescriptionEvenIfEmpty: ShouldAddBaseRateDescriptionEvenIfEmpty);

			calculation.Set(bases);
		}

		static void AddBaseMinMaxPaymentBasis(
			RatingCriteria criteria,
			List<PaymentBasis> bases,
			string currencyCode,
			bool isDependentCalculator,
			decimal baseRate,
			bool shouldAddBaseRateDescriptionEvenIfEmpty,
			decimal minRate,
			decimal maxRate
			)
		{
			if (bases.Calculate().amount != 0 || !isDependentCalculator)
			{
				if (baseRate != 0 || shouldAddBaseRateDescriptionEvenIfEmpty)
				{
					var baseRateInfo = RateInfo.CreateFLT(baseRate, currencyCode);
					bases.Add(criteria.CreatePaymentBasis(baseRateInfo, default, chargeableUnitDescription: RatingDataRegistry.Instance.BaseRateText.Value));
				}

				if (minRate > 0)
				{
					bases.Add(criteria.CreatePaymentBasis(RateInfo.CreateMIN(minRate, currencyCode), default));
				}

				if (maxRate != 0)
				{
					bases.Add(criteria.CreatePaymentBasis(RateInfo.CreateMAX(maxRate, currencyCode), default));
				}
			}
		}

		static void AddBaseMinMaxLog(CalculationLog calculationLog,
			List<PaymentBasis> bases,
			decimal baseRate,
			bool shouldAddBaseRateDescriptionEvenIfEmpty)
		{
			if (bases.Any())
			{
				var (amount, min, max) = bases.Calculate();
				if (baseRate != 0 || shouldAddBaseRateDescriptionEvenIfEmpty)
				{
					calculationLog.BaseRate = baseRate;
				}

				if (amount >= max)
				{
					calculationLog.Maximum = max;
				}

				if (amount <= min)
				{
					calculationLog.Minimum = min;
				}
			}
		}

		protected static CalculationLog InitializeCalculationLog(
			Calculator calculator,
			CalculationLog calculationLog,
			AutoRatingCalculatorParameters parameters,
			IRateLine line,
			ZString unit)
		{
			calculationLog.IsCosting = line.IsCostRate();
			calculationLog.CalculatorCode = line.TL_RateCalculator;
			calculationLog.ChargeCode = line.ChargeCode.AC_Code;
			calculationLog.Currency = line.TL_RX_NKCurrency;
			var entry = line.ParentRateEntry;
			if (entry != null)
			{
				calculationLog.CommodityCode = entry.TI_RH_NKCommodityCode;
				calculationLog.RateMode = entry.TI_Mode;
				calculationLog.ContainerCode = entry.Container?.RC_Code ?? ZString.Empty;
			}

			calculationLog.Unit = unit;
			if (line.TL_AC == Env.Registry.FreightChargeCode)
			{
				var weightQty = parameters.GetWeight(line).Amount;
				calculationLog.Weight = QuantityUnit.IsWeight(unit) ? weightQty.AmountFor(unit).Amount : weightQty.Amount;
				if (unit != QuantityUnit.CN)
				{
					var chargeableQty = calculator.ChargeableAmount(parameters);
					calculationLog.Chargeable = chargeableQty.Amount;
					calculationLog.ChargeableUnit = chargeableQty.Unit;
					if (unit.IsEmpty)
					{
						calculationLog.Unit = weightQty.Unit;
					}
				}
			}

			return calculationLog;
		}

		protected virtual bool ShouldAddBaseRateDescriptionEvenIfEmpty => false;

		string CreateTeaChestSpecificDescription(AutoRatingCalculatorParameters parameters)
		{
			// hacky but we have very specific requirements for Tea Chest rating descriptions
			var sb = new ZStringBuilder();

			sb.Append(UnitDescriptionInternal(GetUnit(parameters), addPlural: true) + " ");

			if (parameters.Criteria.PackageInformation != null)
			{
				var teaChestLines = new List<string>();
				foreach (var info in parameters.Criteria.PackageInformation)
				{
					var unitDescription = UnitDescriptionInternal(info.Description, addPlural: true, addContainerCode: true);
					var teachestLineDescription = Res.GetString(
						"12b0e7d1-da70-4a9b-be6a-3b9bd2e7bf40",
						"{0} {1} ({2} Tea Chest volume)",
						info.Count.ToString(Culture.CurrentCompanyCountryCulture),
						unitDescription,
						Constants.Volume.Convert(info.Volume, info.VolumeUnit, Constants.Volume.TeaChest).ToString(Culture.CurrentCompanyCountryCulture));

					teaChestLines.Add(teachestLineDescription);
				}
				if (teaChestLines.Any())
				{
					sb.Append("- ");
				}
				sb.Append(string.Join(" + ", teaChestLines));
			}

			return sb.ToString();
		}

		protected virtual void CalculateInternal(CalculatorOutput output) { }

		protected void CalculatePerUnit(CalculatorOutput calcOutput, Quantity perUnit, Quantity? chargeableAmountOverride = null)
		{
			var parameters = calcOutput.Parameters;
			var chargeableUnitDescription = perUnit.Unit == Constants.Volume.TeaChest
				? CreateTeaChestSpecificDescription(parameters).Trim()
				: UnitDescriptionInternal(perUnit.Unit, addPlural: true, addContainerCode: true);

			var chargeableAmount = chargeableAmountOverride == null
				? GetPerUnitChargeable(parameters, perUnit.Unit)
				: new[] { chargeableAmountOverride.Value };

			foreach (var chargeable in chargeableAmount.Where(x => !x.IsEmpty))
			{
				var roundedChargeable = RoundAmount(chargeable);
				var rateInfo = RateInfo.CreateUNT(
					perUnit.Amount / UnitMultiplier,
					perUnit.Unit,
					Line.TL_RX_NKCurrency,
					unitDescription: UnitDescription(false, perUnit.Unit),
					unitMultiplier: UnitMultiplier,
					rateInfoDescription: perUnit.Description);

				var chargeableDescription = chargeable.Description;

				if (perUnit.Unit == QuantityUnit.SV)
				{
					var faultyServices = parameters.Criteria.JobServices
						.FindServices(Line.ChargeCode)
						.Where(x => !string.IsNullOrEmpty(x.FaultMessage))
						.ToArray();

					if (faultyServices.Any())
					{
						chargeableDescription = string.Join(", ", faultyServices.Select(x => x.FaultMessage));
					}
				}

				var basis = calcOutput.Criteria.CreatePaymentBasis(rateInfo, roundedChargeable, chargeableUnitDescription, chargeableDescription);
				basis.UnroundedChargeable = chargeable;

				calcOutput.Add(basis);

				if (perUnit.Unit == QuantityUnit.CN)
				{
					calcOutput.CalculationLog.AddPerContainerUnitCalculation(chargeable.Amount, perUnit.Amount / UnitMultiplier);
				}
				else
				{
					calcOutput.CalculationLog.AddPerUnitCalculation(chargeable.Amount, perUnit.Unit, perUnit.Amount / UnitMultiplier);
				}
			}
		}

		protected void AddFlatAmountToLastCalculation(CalculatorOutput calcOutput, ZDecimal flatRate, ZString? rateInfoDescription = null)
		{
			if (flatRate == 0)
			{
				return;
			}

			calcOutput.CalculationLog.AddFlatAmountToLastCalculation(flatRate);

			var basis = calcOutput.Parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(flatRate, Line.TL_RX_NKCurrency), default, null, rateInfoDescription);
			calcOutput.Add(basis);
		}

		public virtual bool SupportsPacksWeightUnitFactor => false;

		public virtual bool SupportsProductLineUnitFactor => false;

		public virtual bool SupportsPackageLineUnitFactor => false;

		#endregion

		#region Merge Payment Bases

		protected IEnumerable<PaymentBasis> MergePaymentBases(IEnumerable<PaymentBasis> bases)
		{
			var result = new List<PaymentBasis>();

			var mergeableGroups = bases
				.GroupBy(x => (x.AdapterType, x.AdapterID, x.ChargeableUnitDescription, x.ChargeableDescription, x.PercentageBaseDescription))
				.ToList();

			foreach (var group in mergeableGroups)
			{
				var rateTypeGroups = group.GroupBy(g => g.RateInfo.Type).ToList();

				foreach (var basesByType in rateTypeGroups)
				{
					if (basesByType.Key == RateInfo.RateInfoType.UNT)
					{
						var perUnitMergedBases = MergePerUnitBases(basesByType);
						result.AddRange(perUnitMergedBases);
					}
					else
					{
						// We only merge Per Unit Bases for now
						result.AddRange(basesByType.ToArray());
					}
				}
			}

			return result;
		}

		IEnumerable<PaymentBasis> MergePerUnitBases(IEnumerable<PaymentBasis> perUnitBases)
		{
			var result = new List<PaymentBasis>();
			var basesToMerge = new List<PaymentBasis>();

			foreach (var basis in perUnitBases)
			{
				// These ones are not actually per unit charges even though recorded as per unit. So,
				// we don't merge them.
				if (basis.RateInfo.IsPartThereof || basis.RateInfo.IsPercentage)
				{
					result.Add(basis);
				}
				else
				{
					basesToMerge.Add(basis);
				}
			}

			if (!basesToMerge.Any())
			{
				return result;
			}

			// First, merge by the same rate, i.e.
			// 50 KG x $2 @ KG
			// 200 KG x $4 @ KG
			// 100 KG x $2 @ KG
			// 300 KG x $4 @ KG
			// 150 KG x $8 @ KG
			//
			// Must be merged to
			// 150 KG x $2 @ KG
			// 500 KG x $4 @ KG
			// 150 KG x $8 @ KG
			var mergedBases = basesToMerge
				.GroupBy(x => (x.RateInfo, x.Chargeable.Unit, x.Chargeable.Reference))
				.Select(bases =>
				{
					var basis = bases.First();
					var totalChargeable = new Quantity(bases.Sum(x => x.Chargeable.Amount), bases.Key.Unit, reference: bases.Key.Reference);
					var mergedBasis = new PaymentBasis(
						totalChargeable,
						bases.Key.RateInfo,
						basis.AdapterType,
						basis.AdapterID,
						basis.ChargeableUnitDescription,
						basis.ChargeableDescription);

					return mergedBasis;
				})
				.ToList();

			// Then merge by chargeable, i.e.
			//  50 KG x $3 @ KG
			//  50 KG x $4 @ KG
			// 100 KG x $2 @ KG
			// 300 KG x $4 @ KG
			// 100 KG x $8 @ KG
			//
			// Must be merged to
			//  50 KG x $7 @ KG
			// 100 KG x $10 @ KG
			// 300 KG x $4 @ KG
			mergedBases = mergedBases
				.GroupBy(x => (x.Chargeable, x.RateInfo.Unit, x.RateInfo.UnitDescription, x.RateInfo.UnitMultiplier, x.RateInfo.Currency, x.RateInfo.RateInfoDescription))
				.Select(bases =>
				{
					var basis = bases.First();
					var totalPrice = bases.Sum(x => x.RateInfo.PerUnitRate ?? 0);
					var totalRate = RateInfo.CreateUNT(
						totalPrice,
						bases.Key.Unit.Value,
						bases.Key.Currency,
						unitMultiplier: bases.Key.UnitMultiplier,
						unitDescription: bases.Key.UnitDescription,
						rateInfoDescription: bases.Key.RateInfoDescription);
					var mergedBasis = new PaymentBasis(
						bases.Key.Chargeable,
						totalRate,
						basis.AdapterType,
						basis.AdapterID,
						basis.ChargeableUnitDescription,
						basis.ChargeableDescription);

					return mergedBasis;
				})
				.ToList();

			if (mergedBases.Any())
			{
				result.AddRange(mergedBases);
			}

			return result;
		}

		#endregion

		#region SetAttributes

		protected static void SetAttributes(
			CalculationResult result,
			AutoRatingCalculatorParameters parameters,
			IRateLine calculatorLine,
			string unit)
		{
			var product = parameters.ProductFilter;
			var productAttributes = parameters.ProductAttributesFilter;
			if (product != null)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.Product, product.OP_PartNum);
				result.AddAttribute(JobChargeAttribTypeList.Codes.Commodity, product.OP_RH_NKCommodityCode);

				if (productAttributes.Length > 0)
				{
					result.AddAttribute(JobChargeAttribTypeList.Codes.Attrib1, productAttributes[0] ?? "");
				}
				if (productAttributes.Length > 1)
				{
					result.AddAttribute(JobChargeAttribTypeList.Codes.Attrib2, productAttributes[1] ?? "");
				}
				if (productAttributes.Length > 2)
				{
					result.AddAttribute(JobChargeAttribTypeList.Codes.Attrib3, productAttributes[2] ?? "");
				}
				if (productAttributes.Length > 3)
				{
					result.AddAttribute(JobChargeAttribTypeList.Codes.SerialNumber, productAttributes[3] ?? "");
				}
			}

			var entry = calculatorLine.ParentRateEntry;

			if (result.Attributes.Attributes.All(a => a.Code != JobChargeAttribTypeList.Codes.Commodity) &&
				!entry.TI_RH_NKCommodityCode.IsEmpty)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.Commodity, entry.TI_RH_NKCommodityCode);
			}

			if (result.Attributes.Attributes.All(a => a.Code != JobChargeAttribTypeList.Codes.FMCTariffID) &&
				!entry.TI_FMCTariffID.IsEmpty)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.FMCTariffID, entry.TI_FMCTariffID);
			}

			var addedContainerCodes = new HashSet<ZString>();
			var entryContainer = entry.Container;
			if (entryContainer != null)
			{
				addedContainerCodes.Add(entryContainer.RC_Code);
			}

			var containerCodesFromCalculations = result.PaymentBases
				.Where(p => p.RateInfo.Unit.HasValue && p.RateInfo.Unit.Value == QuantityUnit.CN && p.Chargeable.Unit != QuantityUnit.CN)
				.Select(paymentBasis => paymentBasis.Chargeable.Unit);

			addedContainerCodes.UnionWith(containerCodesFromCalculations);

			foreach (var containerCode in addedContainerCodes)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.ContainerCode, containerCode);
			}

			var location = parameters.LocationFilter;
			if (!location.IsEmpty)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.LocationType, location.LocationType);
				result.AddAttribute(JobChargeAttribTypeList.Codes.LocationDesc, location.Description);
			}

			var docketReference = parameters.DocketReferenceFilter;
			if (!docketReference.IsEmpty)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.DocketReference, docketReference);
			}

			var cartageLegPK = parameters.CartageLegPKFilter;
			if (cartageLegPK.IsValid)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.CartageLegPK, cartageLegPK.ToString());
			}

			// TODO remove the usage of this attribute in 2 years time in WI00617632
			if (result.PaymentBases.Calculate().minimum > decimal.MinValue)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.MinimumRateUsed, ZBool.True.ToString());
			}

			if (!string.IsNullOrWhiteSpace(result.CartageZoneDescription))
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.CartageZoneDescription, result.CartageZoneDescription);
			}

			result.AddAttribute(JobChargeAttribTypeList.Codes.ItemsToRate, result.PaymentBases.Sum(x => x.Chargeable.Amount).ToString(CultureInfo.InvariantCulture));
			result.AddAttribute(JobChargeAttribTypeList.Codes.ItemsToRateUnit, unit);
			result.AddAttribute(JobChargeAttribTypeList.Codes.UnroundedItemsToRate, result.PaymentBases.Sum(x => x.UnroundedChargeable.Amount).ToString(CultureInfo.InvariantCulture));

			// Container number attribute is only added for jobs that set a filter of MeasureDimension.ContainerNumber (see GetMeasureDimensionsForLine).
			// This is currently only Local Transport jobs.
			var containerNumber = parameters.ContainerNumberFilter;
			if (containerNumber.HasValue && !containerNumber.Value.IsEmpty)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.ContainerNumber, containerNumber, 0);
			}

			// For BCN shipments, cost and revenue charges can be created from co-load shipments but we can save them to lead shipment's job only.
			// Without the references, identical charges (usually apportioned ORG charge type) created for different shipments cause confusion when merging.
			// An example:
			// - Auto cost lead shipment having 2 sub shipments.
			// - 3 cost charges created with same charge code from 3 shipments but stored with lead shipment's job.
			// Because of this reason, we cannot use just JobNumber from the main job for all charges.
			// - Auto revenue the lead shipment.
			// - 3 revenue charges created with same charge code. JobNumber(s) from AutoRatedFor collection as charge's attributes
			// can help merging them to cost charges.
			var jobNumbersFromConsolLeadShipments = new ZStringBuilder(parameters.Criteria.AutoRatedFor
				.Where(bizO => bizO.IsConsolLeadShipment())
				.OfType<IJobNumber>()
				.Select(x => x.JobNumber)
				.OrderBy(x => x)).ToStringWithDelimiterBetweenAppends(", ");
			if (!string.IsNullOrEmpty(jobNumbersFromConsolLeadShipments))
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.JobNumbersReference, jobNumbersFromConsolLeadShipments);
			}

			if (!entry.TI_OH_TransportProvider.IsEmpty)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.TransportProviderPK, entry.TI_OH_TransportProvider.ToString());
			}

			if (entry.ParentRatingHeader is Costing costing)
			{
				result.AddAttribute(JobChargeAttribTypeList.Codes.ServiceProviderPK, costing.TH_OH.ToString());
			}

			if (parameters.ServiceRater.IsEnabled)
			{
				var services = parameters.ServiceRater.GetServicesBeingCalculated(calculatorLine);
				var serviceId = services.Select(s => s.ServiceId).FirstOrDefault(s => !s.IsEmpty);
				if (!serviceId.IsEmpty)
				{
					result.AddAttribute(JobChargeAttribTypeList.Codes.ServiceID, serviceId);
				}
			}

			var rateId = entry.RateId;

			if (!string.IsNullOrEmpty(rateId))
			{
				if (rateId.Length > AutoJobChargeAttrib.Schema.EC_ValueMaxLength)
				{
					rateId = rateId.Substring(0, AutoJobChargeAttrib.Schema.EC_ValueMaxLength);
				}

				result.AddAttribute(JobChargeAttribTypeList.Codes.RateId, rateId);
			}

			if (calculatorLine.Calculator.IsCalculatorDescriptionSupported)
			{
				var calculationDescription = calculatorLine.Calculator.ConvertCalculatorDescription();
				result.AddAttribute(JobChargeAttribTypeList.Codes.CalculatorDescription, calculationDescription);
			}

			if (parameters.Criteria.ConsumerType == JobInvoicingConsumerTypes.MNRWorkOrderHeader)
			{
				var partLists = parameters.Criteria.JobMeasures.GetJobRefContainerInfo();
				foreach (var list in partLists)
				{
					if (list.WorkOrderLinePK != Guid.Empty)
					{
						result.AddAttribute(JobChargeAttribTypeList.Codes.WorkOrderLine, list.WorkOrderLinePK.ToString());
					}
				}
			}
		}

		#endregion

		#region Calculator Description Conversion

		protected virtual bool IsCalculatorDescriptionSupported => false;

		protected virtual string ConvertCalculatorDescription() => string.Empty;

		#endregion

		#region Chargeable Amount

		protected internal virtual ZString Unit => Line.TL_WeightVolume;

		protected internal virtual IEnumerable<ZString> GetUnits(AutoRatingCalculatorParameters parameters)
		{
			return new[] { Unit };
		}

		protected Quantity UnroundedChargeable { get; set; }

		public ZString GetUnit(AutoRatingCalculatorParameters parameters)
		{
			return GetUnits(parameters).FirstOrDefault();
		}

		protected virtual bool HasUnitMultiple
		{
			get { return Line.TL_WeightVolumeMultiple != 0m; }
		}

		protected virtual ZDecimal UnitMultiplier => HasUnitMultiple ? Line.TL_WeightVolumeMultiple : 1;

		public Quantity ChargeableAmount(AutoRatingCalculatorParameters parameters)
		{
			try
			{
				var result = ChargeableAmountInternal(parameters, GetUnit(parameters));
				var roundedResult = RoundAmount(result);
				UnroundedChargeable = result;

				return roundedResult;
			}
			catch (UnitConversionException ex)
			{
				throw new CalculationException(ex.Message);
			}
		}

		IEnumerable<Quantity> GetPerUnitChargeable(AutoRatingCalculatorParameters parameters, string unit)
		{
			// Chargeable here is total amount in units with reference including references of all chargeable objects.
			//
			// For example, for the following container configuration
			// 1 20GP REF1111111
			// 1 20GP REF2222222
			// 4 20GP
			//
			// The chargeable will be:
			// 6 20GP REF1111111,REF2222222
			//
			// But, for proper calculation and payment bases creation we need to disassemble it back to what it was originally, i.e. we want
			// to see the following chargeable:
			// 1 20GP REF1111111
			// 1 20GP REF2222222
			// 4 20GP
			//
			// The logic below does this.

			var chargeable = ChargeableAmountInternal(parameters, unit);

			// You may ask why do we pass unit in the method if we have chargeable.Unit which is the same?
			// Well, it is not the same. For containers, chargeable.Unit will contain concrete container type (i.e. 20GP) rather than CN.
			// So, for the below conditions, we still need to use unit rather than chargeable.Unit.

			if (unit == QuantityUnit.CN || unit == QuantityUnit.TU)
			{
				// Due to some strange logic, we have inconsistency in charges created depending on the unit.
				// If the unit is CN (container) then we won't calculate and create any charge if the job has no containers (which is logical)
				// If the unit is other than CN (for exanple weight, volume, pack types like Box, Pallets, etc.) then
				// we will calculate and create a charge if the job has 0 amounts for those unit (for example, doens't have boxes and the rate is per box).
				// This is not logical, but, product manager believes that some clients may want these charges to be created. So, I keep these logic
				// for all units but containers (basically as it was before refactoring).
				//
				// For containers, I return empty chargeable collection which means nothing to calculate and thus no payment bases will be created.
				if (chargeable.Amount == 0)
				{
					return Enumerable.Empty<Quantity>();
				}

				if (Line.UseOnlyActualWeightMeasure())
				{
					// If there are 5 containers and shipment share in these containers is 20%, it means we can pack the shipment
					// in 1 container with share 100% (20% * 5 containers) and effectively we will need just 1 container for the shipment.
					var shipmentShares = parameters.GetChargeableContainers(Line).Sum(x => x.ShipmentShare * x.ContainerCount);
					var newChargeable = shipmentShares < chargeable.Amount
						? new Quantity(shipmentShares, chargeable.Unit, reference: chargeable.Reference)
						: chargeable;

					return new[] { newChargeable };
				}

				if (unit == QuantityUnit.CN && !Line.UseOnlyActualWeightMeasure() && !HasChargeableContainersForServices(parameters))
				{
					var newChargeables = new List<Quantity>();

					var references = chargeable.Reference.Split(',')
						.Select(r => r.Trim())
						.Where(r => !r.IsEmpty)
						.ToList();

					foreach (var reference in references)
					{
						var newChargeable = new Quantity(1m, chargeable.Unit, reference: reference);
						newChargeables.Add(newChargeable);
					}

					var otherContainersCount = chargeable.Amount - newChargeables.Count;
					if (otherContainersCount > 0)
					{
						var newChargeable = new Quantity(otherContainersCount, chargeable.Unit);
						newChargeables.Add(newChargeable);
					}

					return newChargeables;
				}
			}

			return new[] { chargeable };
		}

		bool HasChargeableContainersForServices(AutoRatingCalculatorParameters parameters)
		{
			var serviceCode = Line.ChargeCode.AC_ChargeSubGroup;
			if (serviceCode.IsEmpty || !parameters.ServiceRater.IsEnabled)
			{
				return false;
			}

			var (containers, _) = parameters.ServiceRater.GetContainersForContainerServices(Line);
			return containers.Any();
		}

		protected virtual Quantity ChargeableAmountForBreakSearch(AutoRatingCalculatorParameters parameters)
		{
			return ChargeableAmount(parameters);
		}

		protected virtual Quantity ChargeableAmountInternal(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			var result = ChargeableValueProvider.CalculateChargeable(parameters, unit);
			return result;
		}

		internal Quantity RoundAmount(Quantity chargeable)
		{
			if (chargeable.IsEmpty)
			{
				return chargeable;
			}

			var amount = chargeable.Amount;

			if (HasUnitMultiple && RatingDataRegistry.Instance.RoundingUsesWeightVolumeMultiple.Value)
			{
				amount /= UnitMultiplier;
			}

			var (rounding, roundingFactor) = RateLineBizO?.CalculatedRounding ?? (Line.TL_Rounding, Line.TL_RoundingFactor);

			var result = Utilities.Round(amount, rounding, roundingFactor);

			if (HasUnitMultiple && RatingDataRegistry.Instance.RoundingUsesWeightVolumeMultiple.Value)
			{
				result *= UnitMultiplier;
			}

			return new Quantity(result, chargeable.Unit, reference: chargeable.Reference, description: chargeable.Description, label: chargeable.Label);
		}

		protected ZString GetShipmentSharesDescription(ZDecimal fullAmount, ZString unitDescription)
		{
			return Res.GetString("9e0a518c-58c4-427a-ad0f-e00c0ef82c6f", "of {0} {1}", fullAmount.ToString("G26", Culture.CurrentCompanyCountryCulture), unitDescription);
		}

		#endregion

		#region Break Amount

		public virtual ZString BreakUnit => Line.TL_WeightVolume;

		internal bool IsBaseCombinedCalculatorAndHasBreakUnit => (Line.Uses(CalculatorType.Combined) || Line.Uses(CalculatorType.Cartage) || Line.Uses(CalculatorType.CartageZoneDistance))
			&& !string.IsNullOrEmpty(BreakUnit);

		protected virtual ZString BreakItems(ZDecimal value, bool isHigherRange)
		{
			var unitDescription = UnitDescriptionInternal(BreakUnit, addPlural: true);
			return value.ToString("G29", Culture.CurrentCompanyCountryCulture) + " " + unitDescription;
		}

		#endregion

		#region Time Amount

		protected Quantity TimeAmountInternal(AutoRatingCalculatorParameters parameters, ZString timeUnit)
		{
			return new TimeChargeableCalculationStrategy(this).GetChargeableAmount(parameters, timeUnit);
		}

		#endregion

		#region Find Break RateLineItem

		protected internal IRateLineItem GetBreakItem(IEnumerable<IRateLineItem> items, AutoRatingCalculatorParameters parameters, Quantity? breakAmountOverride = null)
		{
			var breakAmount = breakAmountOverride ?? ChargeableAmountForBreakSearch(parameters);
			var item = GetBreakItemByAmount(breakAmount, items);
			if (item != null && item.TM_CallForPricing)
			{
				var reasonDescription = Res.GetString("17c211c2-2fd0-4952-a4db-56417e1aad87", "Greater than {0} {1}", CurrencyAndBreakUnitDescription(item.TM_Break), CallForPricingReason(item));

				throw new AutoRater.CallForPriceException(item.ParentRateLine, parameters.Criteria, reasonDescription);
			}

			return item;
		}

		internal IRateLineItem GetBreakItemByAmount(Quantity chargeableAmount, IEnumerable<IRateLineItem> items)
			=> GetBreakItemByAmount(chargeableAmount, items, UseInclusiveBreaks);

		static internal IRateLineItem GetBreakItemByAmount(Quantity chargeableAmount, IEnumerable<IRateLineItem> items, bool useInclusiveBreaks)
		{
			if (items == null)
			{
				ErrorReporter.ReportOnce("14de56af-c4ee-465b-acfa-8bbfd22a1d67", "WI00234641 - GetBreakItemByAmount: items parameter should not be null but it is.");
				return null;
			}

			var sortedItems = GetSortedBreaks(items).ToList();
			if (!sortedItems.Any())
			{
				return null;
			}

			bool isChargeableLessThanBreak(IRateLineItem item) => useInclusiveBreaks ? chargeableAmount.Amount <= item.TM_Break : chargeableAmount.Amount < item.TM_Break;

			int foundIndex = isChargeableLessThanBreak(sortedItems[0])
				? 0
				: sortedItems.FindLastIndex(item => item.RateOperatorIsPlus() && !isChargeableLessThanBreak(item));

			IRateLineItem result = null;
			if (foundIndex >= 0)
			{
				result = sortedItems[foundIndex];

				bool isDuplicateBreak(IRateLineItem item) => item.TM_Type == result.TM_Type && item.TM_Break == result.TM_Break;
				int increment = foundIndex == 0 ? 1 : -1;
				int searchIndex = foundIndex + increment;
				while (searchIndex >= 0 && searchIndex < sortedItems.Count && isDuplicateBreak(sortedItems[searchIndex]))
				{
					if (sortedItems[searchIndex].TM_RelevantValue != result.TM_RelevantValue)
					{
						throw new CalculationException(ErrorMessages.CalculationHasDuplicateBreaksWithDifferentValues(result.TM_Break));
					}
					searchIndex += increment;
				}
			}

			return result;
		}

		protected static MultilingualString CallForPricingReason(IRateLineItem item)
		{
			if (item.TM_Text.IsEmpty)
			{
				return ResString.GetMultilingualString("c55a08f2-756c-4ecf-b0e3-a5a0a9486d94", "Call for Price");
			}
			else
			{
				return (NoResString)item.TM_Text;
			}
		}

		internal IEnumerable<IRateLineItem> SortedBreaks
		{
			get { return GetSortedBreaks(Line.ChildRateLineItems.Cast<IRateLineItem>()); }
		}

		internal static IEnumerable<IRateLineItem> GetSortedBreaks(IEnumerable<IRateLineItem> items)
		{
			var result = items
				.Where(item => item.RateOperatorIsMinus() || item.RateOperatorIsPlus())
				.OrderBy(x => x.RateOperatorIsPlus())
				.ThenBy(x => x.TM_Break);

			return result;
		}

		#endregion

		public virtual Calculator GetBaseCalculator(AutoRatingCalculatorParameters parameters)
		{
			return this;
		}

		#endregion

		#region Company Tariff / Cost Based Clone

		internal virtual ZString GetCloneCode(CompanyTariffOrCostBasedCalculator source)
		{
			return Line.TL_RateCalculator;
		}

		internal virtual void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			if (!GetCloneCode(ctbCalc).IsEmpty)
			{
				clone.RateLineItems.RemoveAndDeleteAll();
				CloneLineItemsUpdatingRateValuesWithZones(ctbCalc, clone, RateLineBizO.RateLineItems, rateTypeToUpdate, ZString.Empty);
			}
		}

		#region Default Implementation

		protected void CloneLineItemsUpdatingRateValuesWithZones(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, IList items, RateLineItem.RateTypeToUpdate rateTypeToUpdate, ZString? zoneName = null, ZGuid? zonePK = null)
		{
			SetClonedMinimum(ctbCalc, clone, items, rateTypeToUpdate);
			var clonedLineHasBAS = SetClonedBaseRate(ctbCalc, clone, items, rateTypeToUpdate, zoneName, zonePK);
			SetClonedOtherRates(ctbCalc, clone, items, clonedLineHasBAS, rateTypeToUpdate);
		}

		protected bool SetClonedBaseRate(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, IList items, RateLineItem.RateTypeToUpdate rateTypeToUpdate, ZString? zoneName = null, ZGuid? zonePK = null)
		{
			var baseRate = ctbCalc.BaseRateWithApplicableIncrease;
			var oldBasItem = items.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsBAS());
			RateLineItem newBasItem = null;
			if (oldBasItem != null)
			{
				newBasItem = clone.RateLineItems.CloneItem(oldBasItem);
				newBasItem.UpdateRateValue(x => x * ((ctbCalc.Percent + 100) / 100) + baseRate, rateTypeToUpdate);
			}
			else if (baseRate != 0 && items.Cast<RateLineItem>().All(x => x.TM_FlatAmount.IsEmpty))
			{
				newBasItem = clone.RateLineItems.AddNew();
				newBasItem.TM_Type = Items.Operator.BAS;
				newBasItem.UpdateRateValue(x => baseRate, rateTypeToUpdate);

				if (zoneName.HasValue && !zoneName.Value.IsEmpty)
				{
					newBasItem.TM_F1Zone = zoneName.Value;
				}

				if (zonePK.HasValue && !zonePK.Value.IsEmpty)
				{
					newBasItem.TM_TZ_DomesticZone = zonePK.Value;
				}
			}

			if (newBasItem != null)
			{
				newBasItem.UpdateRateValue(x => Utilities.Round(x, newBasItem.NumberOfDecimalPlacesForRates), rateTypeToUpdate);

				return true;
			}

			return false;
		}

		protected void SetClonedMinimum(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, IList items, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			var oldMinItem = items.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsMIN());
			if (oldMinItem != null)
			{
				var newMinItem = clone.RateLineItems.CloneItem(oldMinItem);

				newMinItem.UpdateRateValue(
					x =>
					{
						ZDecimal result = x * ((ctbCalc.Percent + 100) / 100) + ctbCalc.MinimumWithApplicableIncrease;
						return Utilities.Round(result, newMinItem.NumberOfDecimalPlacesForRates);
					},
					rateTypeToUpdate);
			}
		}

		protected void SetClonedOtherRates(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, IList items, bool clonedLineHasBAS, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			var baseRateWithApplicableIncrease = ctbCalc.BaseRateWithApplicableIncrease;
			var itemClones = ctbCalc.CloneNonBASAndMINItems(ctbCalc, items, clone);

			foreach (var itemClone in itemClones)
			{
				if (!itemClone.RateOperatorIsNonPrintedFlag())
				{
					if (itemClone.RateOperatorIsUNT())
					{
						itemClone.UpdateRateValue(x => x * ((ctbCalc.PerUnitPercent + 100) / 100) + ctbCalc.PerUnitWithApplicableIncrease, rateTypeToUpdate);
					}
					else
					{
						itemClone.UpdateRateValue(x => x * ((ctbCalc.PerUnitPercent + 100) / 100), rateTypeToUpdate);
					}

					if (itemClone.RequiresWeightBreak())
					{
						var perUnitIncrease = GetPerUnitIncrease(ctbCalc, itemClone);

						// For IncreaseFirst, apply Percent to perUnitIncrease
						if (ctbCalc.CalculationOrder == Items.IncreaseFirst)
						{
							perUnitIncrease *= GetIncreasedPercentageDecimal(ctbCalc.PerUnitPercent);
						}

						itemClone.UpdateRateValue(x => x + perUnitIncrease, rateTypeToUpdate);
						itemClone.TM_FlatAmount *= (ctbCalc.Percent + 100) / 100;

						if (baseRateWithApplicableIncrease != 0 && !clonedLineHasBAS)
						{
							itemClone.TM_FlatAmount += baseRateWithApplicableIncrease;
						}
					}
					else if (itemClone.RateOperatorIsMAX())
					{
						itemClone.UpdateRateValue(x => x + baseRateWithApplicableIncrease, rateTypeToUpdate);
					}

					var roundingScale = itemClone.NumberOfDecimalPlacesForRates;
					itemClone.UpdateRateValue(x => Utilities.Round(x, roundingScale), rateTypeToUpdate);

					if (itemClone.TM_BreakMinimum != 0)
					{
						itemClone.TM_BreakMinimum = ctbCalc.Minimum.IsEmpty
							? Utilities.Round(itemClone.TM_BreakMinimum * (ctbCalc.Percent + 100) / 100 + baseRateWithApplicableIncrease, itemClone.NumberOfDecimalPlacesForRates)
							: itemClone.TM_BreakMinimum + ctbCalc.Minimum;
					}
				}
			}
		}

		static ZDecimal GetIncreasedPercentageDecimal(ZDecimal increasedPercentage) => (increasedPercentage + 100) / 100;

		#endregion

		#region Clone Non Base and Minimum Items

		IEnumerable<RateLineItem> CloneNonBASAndMINItems(CompanyTariffOrCostBasedCalculator ctbCalc, IList itemsToClone, RateLine clone)
		{
			var clonedItems = ctbCalc.IsSliding()
				? CloneItemsForSlidingCTBCalc(ctbCalc, itemsToClone, clone)
				: itemsToClone.Cast<RateLineItem>()
					.Where(x => !x.RateOperatorIsBAS() && !x.RateOperatorIsMIN())
					.Select(x => clone.RateLineItems.CloneItem(x));

			return clonedItems;
		}

		IEnumerable<RateLineItem> CloneItemsForSlidingCTBCalc(CompanyTariffOrCostBasedCalculator ctbCalc, IList items, RateLine clone)
		{
			var itemsToClone = items.Cast<RateLineItem>().ToList();
			var originalBreaks = new HashSet<ZDecimal>(itemsToClone.Where(x => !x.TM_Break.IsEmpty).Select(x => x.TM_Break));
			var addtionalBreaks = ctbCalc.RateLineBizO.RateLineItems.Cast<RateLineItem>().Select(x => x.TM_Break).Where(x => !x.IsEmpty && !originalBreaks.Contains(x)).Distinct().ToList();

			var result = new List<RateLineItem>();
			RateLineItem breakItem = null;
			foreach (var itemToClone in itemsToClone)
			{
				if (itemToClone.TM_Break.IsEmpty && !itemToClone.RateOperatorIsUNT())
				{
					AddCloneIfNotBASOrMIN(result, clone, itemToClone);
				}
				else
				{
					breakItem = itemToClone;
					if (breakItem.RateOperatorIsUNT())
					{
						AddItemClone(result, breakItem, clone, Items.Operator.Minus, addtionalBreaks[0]);
					}
					else if (breakItem.RateOperatorIsMinus())
					{
						var hasMinus = false;
						while (addtionalBreaks.Count > 0 && addtionalBreaks[0] < breakItem.TM_Break)
						{
							if (!hasMinus)
							{
								AddItemClone(result, breakItem, clone, Items.Operator.Minus, addtionalBreaks[0]);
								hasMinus = true;
							}
							AddItemClone(result, breakItem, clone, Items.Operator.Plus, addtionalBreaks[0]);
							addtionalBreaks.RemoveAt(0);
						}
						if (!hasMinus)
						{
							AddItemClone(result, breakItem, clone, Items.Operator.Minus, breakItem.TM_Break);
						}
					}
					else if (breakItem.RateOperatorIsPlus())
					{
						AddItemClone(result, breakItem, clone, Items.Operator.Plus, breakItem.TM_Break);
						var nextPlus = breakItem.NextRateLineItem(items.Cast<RateLineItem>());
						if (nextPlus != null)
						{
							while (addtionalBreaks.Count > 0 && addtionalBreaks[0] < nextPlus.TM_Break)
							{
								AddItemClone(result, breakItem, clone, Items.Operator.Plus, addtionalBreaks[0]);
								addtionalBreaks.RemoveAt(0);
							}
						}
					}
				}
			}

			if (breakItem != null)
			{
				foreach (var addtionalBreak in addtionalBreaks)
				{
					AddItemClone(result, breakItem, clone, Items.Operator.Plus, addtionalBreak);
				}
			}

			byte order = 0;
			foreach (var item in result)
			{
				item.LocalLineOrder = order++;
			}

			return result;
		}

		void AddItemClone(List<RateLineItem> list, RateLineItem itemToClone, RateLine line, ZString newType, ZDecimal newBreak)
		{
			var newItem = AddCloneIfNotBASOrMIN(list, line, itemToClone);

			if (newItem != null)
			{
				newItem.TM_Type = newType;
				newItem.TM_Break = newBreak;
			}
		}

		RateLineItem AddCloneIfNotBASOrMIN(List<RateLineItem> list, RateLine line, RateLineItem itemToClone)
		{
			RateLineItem result = null;

			if (!itemToClone.RateOperatorIsBAS() && !itemToClone.RateOperatorIsMIN())
			{
				result = line.RateLineItems.CloneItem(itemToClone);
				list.Add(result);
			}

			return result;
		}

		#endregion

		ZDecimal GetPerUnitIncrease(CompanyTariffOrCostBasedCalculator ctbCalc, IRateLineItem newItem)
		{
			if (ctbCalc.IsSliding())
			{
				var minusItem = ctbCalc.FindRateLineItem(Items.Operator.Minus);
				if (minusItem != null && newItem.RateOperatorIsMinus())
				{
					return minusItem.TM_RelevantValue;
				}

				if (newItem.RateOperatorIsPlus())
				{
					var previousItem = minusItem;
					foreach (var item in ctbCalc.Line.ChildRateLineItems.Cast<RateLineItem>())
					{
						if (item.RateOperatorIsPlus())
						{
							if (newItem.TM_Break < item.TM_Break)
							{
								return previousItem != null ? previousItem.TM_RelevantValue : ctbCalc.PerUnit;
							}
							previousItem = item;
						}
					}

					return previousItem != null ? previousItem.TM_RelevantValue : ctbCalc.PerUnit;
				}
			}

			return ctbCalc.PerUnit;
		}

		public bool ShouldValueBeDiscounted(RateLineItem item)
		{
			return !Line.Uses(CalculatorType.Agency)
					|| (item.TM_Type == AgencyCalculator.Items.AgencyRate ||
						item.TM_Type == AgencyCalculator.Items.AdditionalRate ||
						item.TM_Type == AgencyCalculator.Items.CostPerAdditionalLine);
		}

		#endregion

		#region Costs Comparer Charges Summary

		public IList<ChargesSummaryItem> GetCostsComparerChargesSummary(List<RateLine> lines)
		{
			return GetCostsComparerChargesSummaryCore(lines);
		}

		protected virtual IList<ChargesSummaryItem> GetCostsComparerChargesSummaryCore(List<RateLine> lines)
		{
			return null;
		}

		#endregion

		#region CartageZones

		public CartageZoneCollection CartageZones
		{
			get
			{
				if (fCartageZones == null)
				{
					fCartageZones = new CartageZoneCollection(RateLineBizO);

					if (RateLineBizO.Uses(CalculatorType.CartageZoneDistance))
					{
						fCartageZones.Load();
						RateLineBizO.RegisterEditableChildObject(fCartageZones);
					}
				}

				return fCartageZones;
			}
		}
		protected CartageZoneCollection fCartageZones;

		/// <summary>
		/// Loads the cartage zones (either Transport Zone set or ACI Zone)
		/// which are used to display on the zone-picker list in the UI.
		/// </summary>
		public void ReloadCartageZones()
		{
			fCartageZones?.RemoveAll();
			fCartageZones?.Load();
		}

		#endregion

		#region Implementation

		public bool IsBreakUnitAvailable
		{
			get { return IsBreakUnitAvailableCore(); }
		}

		protected virtual bool IsBreakUnitAvailableCore()
		{
			return false;
		}

		#region Attributes

		internal CalculatorPropertyAttribute[] Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = (CalculatorPropertyAttribute[])GetType().GetCustomAttributes(typeof(CalculatorPropertyAttribute), true);

					foreach (var attribute in attributes)
					{
						if (!string.IsNullOrEmpty(attribute.MapTo))
						{
							MapToAttribute.Add(attribute.MapTo, attribute);
						}

						ItemTypeOrFieldNameToAttribute.Add(string.IsNullOrEmpty(attribute.ItemType) ? attribute.FieldName : attribute.ItemType, attribute);

						if (!string.IsNullOrEmpty(attribute.RelatedTo))
						{
							RelatedToToAttribute.Add(attribute.RelatedTo, attribute);
						}
					}
				}
				return attributes;
			}
		}
		CalculatorPropertyAttribute[] attributes;

		CalculatorPropertyAttribute GetAttribute(string propertyName)
		{
			if (propertyName.StartsWith("-", StringComparison.OrdinalIgnoreCase) || propertyName.StartsWith("+", StringComparison.OrdinalIgnoreCase))
			{
				propertyName = propertyName.Substring(0, 1);
			}
			CalculatorPropertyAttribute result;
			ItemTypeOrFieldNameToAttribute.TryGetValue(propertyName, out result);

			return result;
		}

		public int DecimalPlaces
		{
			get { return Line.DecimalPlaces(); }
		}

		static ZDecimal GetBreak(string propertyName)
		{
			var result = ZDecimal.Zero;
			if ((propertyName.StartsWith("-", StringComparison.OrdinalIgnoreCase) || propertyName.StartsWith("+", StringComparison.OrdinalIgnoreCase))
				&& ZDecimal.TryParse(propertyName.Substring(1), out result))
			{
				return result;
			}

			return result;
		}

		readonly Dictionary<string, CalculatorPropertyAttribute> MapToAttribute = new Dictionary<string, CalculatorPropertyAttribute>();
		readonly Dictionary<string, CalculatorPropertyAttribute> RelatedToToAttribute = new Dictionary<string, CalculatorPropertyAttribute>();
		readonly Dictionary<string, CalculatorPropertyAttribute> ItemTypeOrFieldNameToAttribute = new Dictionary<string, CalculatorPropertyAttribute>();

		#endregion

		#region Check, Create, Find, Add Items

		internal IRateLineItem FindOrAddRateLineItem(CalculatorPropertyAttribute attribute, ZDecimal @break)
		{
			if (@break.IsEmpty)
			{
				return FindOrAddRateLineItem(attribute);
			}

			var iItem = FindRateLineItem(attribute.ItemType, @break);
			if (iItem != null)
			{
				return iItem;
			}

			var item = AddRateLineItem(attribute.ItemType);
			using (item.SuspendSettingHasChanges())
			using (item.GetValidationSuspender())
			{
				item.TM_Break = @break;
			}
			return item;
		}

		IRateLineItem FindOrAddRateLineItem(CalculatorPropertyAttribute attribute)
		{
			var iItem = Line.FindRateLineItem(attribute.ItemType);

			if (iItem != null || RateLineBizO.ReadOnly || (RateLineBizO.Parent?.Parent?.IsFormDelete ?? false))
			{
				return iItem;
			}

			var item = AddRateLineItem(attribute.ItemType);
			if (attribute.InitialValue != null)
			{
				IZType initialValue = null;
				var stringValue = attribute.InitialValue as string;
				if (stringValue != null)
				{
					initialValue = (ZString)stringValue;
				}
				else if (attribute.InitialValue is bool)
				{
					initialValue = (ZBool)(bool)attribute.InitialValue;
				}

				if (initialValue != null)
				{
					using (item.SuspendSettingHasChanges())
					using (item.GetValidationSuspender())
					{
						attribute.SetValue(this, 0m, initialValue);
					}
				}
			}

			return item;
		}

		internal RateLineItem FindRateLineItem(ZString itemType)
		{
			return Line.FindRateLineItem(itemType) as RateLineItem;
		}

		internal IRateLineItem FindRateLineItem(ZString itemType, ZDecimal @break)
		{
			return @break.IsEmpty ? Line.FindRateLineItem(itemType) : Line.ChildRateLineItems.FirstOrDefault(o => o.TM_Type == itemType && o.TM_Break == @break);
		}

		RateLineItem AddRateLineItem(ZString itemType)
		{
			var newItem = RateLineBizO.RateLineItems.AddNew();

			using (newItem.SuspendSettingHasChanges())
			using (newItem.GetValidationSuspender())
			{
				((ISupportDataImporting)newItem).IsImportingData = ((ISupportDataImporting)RateLineBizO).IsImportingData;
				newItem.TM_Type = itemType;
			}

			return newItem;
		}

		#endregion

		#region Mapping

		/// <summary>
		/// Given the name of a calculator property, returns the ZPropertyInfo of its RateLineItem
		/// mapped-to field; or null if not found.
		///
		/// The world 'relatedTo' refers to the CalculatorAttribute RelatedTo property, that
		/// corresponds to a property on the calculator.
		/// </summary>
		internal ZPropertyInfo GetRelatedToInfo(string relatedTo) =>
			RelatedToToAttribute.ContainsKey(relatedTo)
				? RelatedToToAttribute[relatedTo].GetMapToInfo(this)
				: null;

		public bool ContainsMapToProperty(string mapTo)
		{
			return MapToAttribute.ContainsKey(mapTo);
		}

		IZType GetMapToValue(string mapTo)
		{
			CalculatorPropertyAttribute attribute;
			MapToAttribute.TryGetValue(mapTo, out attribute);
			if (attribute != null)
			{
				return attribute.GetValue(this, 0);
			}

			if (mapTo.StartsWith((NoResString)"Decimal", StringComparison.OrdinalIgnoreCase)) // Hard-coded constant
			{
				return (ZDecimal)0m;
			}

			if (mapTo.StartsWith((NoResString)"Bool", StringComparison.OrdinalIgnoreCase)) // Hard-coded constant
			{
				return ZBool.False;
			}

			if (mapTo.StartsWith((NoResString)"Int", StringComparison.OrdinalIgnoreCase)) // Hard-coded constant
			{
				return (ZInt)0;
			}

			if (mapTo.Equals(CalculatorConstants.MapTo.ChargeCode, StringComparison.OrdinalIgnoreCase))
			{
				return ZGuid.Empty;
			}

			return ZString.Empty;
		}

		void SetMapToValue(string mapTo, IZType value)
		{
			CalculatorPropertyAttribute attribute;
			if (MapToAttribute.TryGetValue(mapTo, out attribute))
			{
				attribute.SetValue(this, 0m, value);
			}
			else
			{
				ReportIncorrectMapperPropertyAccessed(mapTo);
			}
		}

		void ReportIncorrectMapperPropertyAccessed(ZString propertyName)
		{
			string message = "Property: [{0}] should not be accessed from this calculator. Calculator: [{1}]. MapToAttribute:[{2}]. TL_RateCalculator:[{3}].";// Log RateCalculator, MapToAttribute, Calculator and property
			string mapToAttr = string.Join(";", MapToAttribute.Keys.OrderBy(x => x).ToArray());

			ErrorReporter.ReportOnce("IncorrectRateLineItemMapper" + propertyName, string.Format(CultureInfo.InvariantCulture, message, propertyName, this.GetType().FullName, mapToAttr, this.master == null ? "" : this.master.TL_RateCalculator.ToString()));
		}

		ZPropertyInfo GetMapToInfo(string mapTo)
		{
			ZPropertyInfo result;
			CalculatorPropertyAttribute attribute;

			MapToAttribute.TryGetValue(mapTo, out attribute);
			if (attribute != null)
			{
				result = attribute.GetMapToInfo(this);
				if (result != null)
				{
					return result;
				}
			}

			if (mapTo.StartsWith((NoResString)"Decimal", StringComparison.OrdinalIgnoreCase) || mapTo.StartsWith((NoResString)"Int", StringComparison.OrdinalIgnoreCase)) // Hard-coded constant
			{
				return DummyRateLineItem.TM_RelevantValueInfo;
			}

			if (mapTo.Equals(CalculatorConstants.MapTo.ChargeCode, StringComparison.OrdinalIgnoreCase))
			{
				return DummyRateLineItem.TM_ACInfo;
			}

			return DummyRateLineItem.TM_TextInfo;
		}

		RateLineItem fDummyRateLineItem;
		protected RateLineItem DummyRateLineItem
		{
			get { return fDummyRateLineItem ?? (fDummyRateLineItem = Line.Factory.New<DummyRateLineItem>()); }
		}

		#region Mapper Properties

		#region Decimals

		[BusinessObjectTestExclude]
		public ZDecimal Decimal1
		{
			get { return (ZDecimal)GetMapToValue("Decimal1"); }
			set { SetMapToValue("Decimal1", value); }
		}

		public ZPropertyInfo Decimal1Info
		{
			get { return GetMapToInfo("Decimal1"); }
		}

		[BusinessObjectTestExclude]
		public ZDecimal Decimal2
		{
			get { return (ZDecimal)GetMapToValue("Decimal2"); }
			set { SetMapToValue("Decimal2", value); }
		}

		public ZPropertyInfo Decimal2Info
		{
			get { return GetMapToInfo("Decimal2"); }
		}

		[BusinessObjectTestExclude]
		public ZDecimal Decimal3
		{
			get { return (ZDecimal)GetMapToValue("Decimal3"); }
			set { SetMapToValue("Decimal3", value); }
		}

		public ZPropertyInfo Decimal3Info
		{
			get { return GetMapToInfo("Decimal3"); }
		}

		[BusinessObjectTestExclude]
		public ZDecimal Decimal4
		{
			get { return (ZDecimal)GetMapToValue("Decimal4"); }
			set { SetMapToValue("Decimal4", value); }
		}

		public ZPropertyInfo Decimal4Info
		{
			get { return GetMapToInfo("Decimal4"); }
		}

		[BusinessObjectTestExclude]
		public ZDecimal Decimal5
		{
			get { return (ZDecimal)GetMapToValue("Decimal5"); }
			set { SetMapToValue("Decimal5", value); }
		}

		public ZPropertyInfo Decimal5Info
		{
			get { return GetMapToInfo("Decimal5"); }
		}

		[BusinessObjectTestExclude]
		public ZDecimal Decimal6
		{
			get { return (ZDecimal)GetMapToValue("Decimal6"); }
			set { SetMapToValue("Decimal6", value); }
		}

		public ZPropertyInfo Decimal6Info
		{
			get { return GetMapToInfo("Decimal6"); }
		}

		[BusinessObjectTestExclude]
		public ZDecimal Decimal7
		{
			get { return (ZDecimal)GetMapToValue("Decimal7"); }
			set { SetMapToValue("Decimal7", value); }
		}

		public ZPropertyInfo Decimal7Info
		{
			get { return GetMapToInfo("Decimal7"); }
		}

		#endregion

		#region Strings

		[BusinessObjectTestExclude]
		public virtual ZString String1
		{
			get { return (ZString)GetMapToValue("String1"); }
			set { SetMapToValue("String1", value); }
		}

		public ZPropertyInfo String1Info
		{
			get { return GetMapToInfo("String1"); }
		}

		[BusinessObjectTestExclude]
		public virtual ZString String2
		{
			get { return (ZString)GetMapToValue("String2"); }
			set { SetMapToValue("String2", value); }
		}

		public ZPropertyInfo String2Info
		{
			get { return GetMapToInfo("String2"); }
		}

		[BusinessObjectTestExclude]
		public virtual ZString String3
		{
			get { return (ZString)GetMapToValue("String3"); }
			set { SetMapToValue("String3", value); }
		}

		public ZPropertyInfo String3Info
		{
			get { return GetMapToInfo("String3"); }
		}

		[BusinessObjectTestExclude]
		public ZString String4
		{
			get { return (ZString)GetMapToValue("String4"); }
			set { SetMapToValue("String4", value); }
		}

		public ZPropertyInfo String4Info
		{
			get { return GetMapToInfo("String4"); }
		}

		#endregion

		#region Bools

		[BusinessObjectTestExclude]
		public virtual ZBool Bool1
		{
			get { return (ZBool)GetMapToValue("Bool1"); }
			set { SetMapToValue("Bool1", value); }
		}

		public ZPropertyInfo Bool1Info
		{
			get { return GetMapToInfo("Bool1"); }
		}

		[BusinessObjectTestExclude]
		public virtual ZBool Bool2
		{
			get { return (ZBool)GetMapToValue("Bool2"); }
			set { SetMapToValue("Bool2", value); }
		}

		public ZPropertyInfo Bool2Info
		{
			get { return GetMapToInfo("Bool2"); }
		}

		[BusinessObjectTestExclude]
		public ZBool Bool3
		{
			get { return (ZBool)GetMapToValue("Bool3"); }
			set { SetMapToValue("Bool3", value); }
		}

		public ZPropertyInfo Bool3Info
		{
			get { return GetMapToInfo("Bool3"); }
		}

		[BusinessObjectTestExclude]
		public ZBool Bool4
		{
			get { return (ZBool)GetMapToValue("Bool4"); }
			set { SetMapToValue("Bool4", value); }
		}

		public ZPropertyInfo Bool4Info
		{
			get { return GetMapToInfo("Bool4"); }
		}

		[BusinessObjectTestExclude]
		public ZBool Bool5
		{
			get { return (ZBool)GetMapToValue("Bool5"); }
			set { SetMapToValue("Bool5", value); }
		}

		public ZPropertyInfo Bool5Info
		{
			get { return GetMapToInfo("Bool5"); }
		}

		[BusinessObjectTestExclude]
		public ZBool Bool6
		{
			get { return (ZBool)GetMapToValue("Bool6"); }
			set { SetMapToValue("Bool6", value); }
		}

		public ZPropertyInfo Bool6Info
		{
			get { return GetMapToInfo("Bool6"); }
		}

		#endregion

		#region Lists

		public virtual CodeDescriptionPairList List1
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList List2
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList List3
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList List4
		{
			get { return new CodeDescriptionPairList(); }
		}

		#endregion

		#region RateLineItems View

		RateLineItemsView fRateLineItemsView;
		public RateLineItemsView RateLineItems => fRateLineItemsView ?? (fRateLineItemsView = new RateLineItemsView(RateLineBizO.RateLineItems));

		RateLineItemsView fApplyToRateLineItems;
		public RateLineItemsView ApplyToRateLineItems => fApplyToRateLineItems ?? (fApplyToRateLineItems = new RateLineItemsView(RateLineBizO.RateLineItems, true));

		IRateLineItemsView fIRateLineItems;
		public IRateLineItemsView IRateLineItems => fIRateLineItems ?? (fIRateLineItems = new IRateLineItemsView(((WiseLineView)Line).ChildWiseRateLineItemViews, Line));

		IRateLineItemsView fApplyToIRateLineItems;
		public IRateLineItemsView ApplyToIRateLineItems => fApplyToIRateLineItems ?? (fApplyToIRateLineItems = new IRateLineItemsView(((WiseLineView)Line).ChildWiseRateLineItemViews, Line));

		#endregion

		#region Ints

		[BusinessObjectTestExclude]
		public ZInt Int1
		{
			get { return (ZInt)GetMapToValue("Int1"); }
			set { SetMapToValue("Int1", value); }
		}

		public ZPropertyInfo Int1Info
		{
			get { return GetMapToInfo("Int1"); }
		}

		[BusinessObjectTestExclude]
		public ZInt Int2
		{
			get { return (ZInt)GetMapToValue("Int2"); }
			set { SetMapToValue("Int2", value); }
		}

		public ZPropertyInfo Int2Info
		{
			get { return GetMapToInfo("Int2"); }
		}

		[BusinessObjectTestExclude]
		public ZInt Int3
		{
			get { return (ZInt)GetMapToValue("Int3"); }
			set { SetMapToValue("Int3", value); }
		}

		public ZPropertyInfo Int3Info
		{
			get { return GetMapToInfo("Int3"); }
		}

		#endregion

		#region ChargeCodes

		[List("ChargeCodes")]
		public ZGuid ChargeCode
		{
			get => (ZGuid)GetMapToValue(CalculatorConstants.MapTo.ChargeCode);
			set => SetMapToValue(CalculatorConstants.MapTo.ChargeCode, value);
		}

		public AccChargeCodeCollection ChargeCodes
		{
			get
			{
				var rateLineLookups = Line is WiseLineView line ? line.Lookups : Line.Lookups();
				var filteredChargeCodes = rateLineLookups.GetChargeCodes(true);
				return filteredChargeCodes;
			}
		}
		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required property for UI Components")]
		int ChargeCode_MaxLength => AccChargeCodeSchema.PK.MaxLength;

		public ZPropertyInfo ChargeCodeInfo => GetMapToInfo(CalculatorConstants.MapTo.ChargeCode);

		#endregion

		#endregion

		#endregion

		protected ZGuid FreightChargeCode
		{
			get
			{
				ZGuid chargeCodePK;

				var company = ParentRateEntry != null ? ParentRateEntry.Company() : GlbCompany.CurrentCompany;
				var companyPK = company == null ? Guid.Empty : company.PK.ToGuid();
				if (!CompanyFreightChargeCodeDictionary.TryGetValue(companyPK, out chargeCodePK))
				{
					chargeCodePK = Env.Registry.GetFreightChargeCode(companyPK);
					CompanyFreightChargeCodeDictionary.Add(companyPK, chargeCodePK);
				}

				return chargeCodePK;
			}
		}

		static Dictionary<ZGuid, ZGuid> CompanyFreightChargeCodeDictionary
		{
			get { return fCompanyFreightChargeCodeDictionary ?? (fCompanyFreightChargeCodeDictionary = new Dictionary<ZGuid, ZGuid>()); }
		}

		[ThreadStatic]
		static Dictionary<ZGuid, ZGuid> fCompanyFreightChargeCodeDictionary;

		protected internal virtual bool ValueIsReadOnly(RateLineItem item)
		{
			return false;
		}

		public AutoRateInfo GetFirstFreightRateInfo(AutoRatingCalculatorParameters parameters)
		{
			return parameters.Results.FirstOrDefault(IsFreightChargeInfo);
		}

		bool IsFreightChargeInfo(AutoRateInfo item)
		{
			return item.IsCost == ParentRatingHeader.IsCosting() && item.ChargeCode.PK == Env.Registry.FreightChargeCode;
		}

		#endregion

		#region IEquatable<Calculator> Members

		public bool Equals(Calculator other)
		{
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			var rateLineItems = Line.ChildRateLineItems.ToList();
			var otherRateLineItems = other.Line.ChildRateLineItems.ToList();

			if (GetType() == other.GetType() &&
				GetRateLineCompareString(Line) == GetRateLineCompareString(other.Line) &&
				rateLineItems.Count == otherRateLineItems.Count)
			{
				var compareStrings1 = new string[rateLineItems.Count];
				var compareStrings2 = new string[rateLineItems.Count];
				for (var i = 0; i < rateLineItems.Count; i++)
				{
					compareStrings1[i] = GetRateLineItemCompareString(rateLineItems[i]);
					compareStrings2[i] = GetRateLineItemCompareString(otherRateLineItems[i]);
				}

				Array.Sort(compareStrings1);
				Array.Sort(compareStrings2);

				for (var i = 0; i < rateLineItems.Count; i++)
				{
					if (compareStrings1[i] != compareStrings2[i])
					{
						return false;
					}
				}

				return true;
			}

			return false;
		}

		string GetRateLineCompareString(IRateLine line)
		{
			var compareString = new StringBuilder();
			compareString.AppendFormat("{0}|", line.TL_RX_NKCurrency);
			compareString.AppendFormat("{0}|", line.TL_WeightVolume);
			compareString.AppendFormat("{0}|", line.TL_WeightVolumeMultiple);
			compareString.AppendFormat("{0}|", line.ConversionFactor.ToShortString());
			compareString.AppendFormat("{0}|", line.UseOnlyActualWeightMeasure());
			compareString.AppendFormat("{0}|", line.TL_Rounding);

			return compareString.ToString();
		}

		string GetRateLineItemCompareString(IRateLineItem lineItem)
		{
			var compareString = new StringBuilder();
			compareString.AppendFormat("{0}|", lineItem.TM_Type);
			compareString.AppendFormat("{0}|", lineItem.TM_RelevantValue);
			compareString.AppendFormat("{0}|", lineItem.TM_FlatAmount);
			compareString.AppendFormat("{0}|", lineItem.TM_Break);
			compareString.AppendFormat("{0}|", lineItem.TM_BreakMinimum);
			compareString.AppendFormat("{0}|", lineItem.TM_BreakWeightVolume);
			compareString.AppendFormat("{0}|", lineItem.TM_Text);
			compareString.AppendFormat("{0}|", lineItem.TM_F1Zone);
			compareString.AppendFormat("{0}|", lineItem.TM_TZ_DomesticZone);
			compareString.AppendFormat("{0}|", lineItem.TM_CallForPricing);
			compareString.AppendFormat("{0}|", lineItem.TM_AgentDeclaredRate);
			compareString.AppendFormat("{0}|", lineItem.TM_AC);

			return compareString.ToString();
		}

		#endregion

		#region NotifyChanged

		public virtual void NotifyChanged(object changedBusinessObject)
		{
		}

		#endregion

		#region RunActionsOnMasterSaving

		public void RunActionsOnMasterSaving()
		{
			RunActionsOnMasterSavingCore();
		}

		protected virtual void RunActionsOnMasterSavingCore()
		{
		}

		#endregion

		#region Types

		/// <summary>
		/// A calculator exception is one that occurs during the calculation of the charges.
		/// It is caught by the calculator code and then displayed as an error in the
		/// charge itself. It does not stop auto rating.
		/// </summary>
		[Serializable]
		public class CalculationException : ZException
		{
			public CalculationException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected CalculationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion

		#region PricePerSingleChargeable

		public List<PaymentBasis> PricePerSingleChargeable
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(Line.TL_RX_NKCurrency))
				{
					return GetPricePerSingleChargeable();
				}

				return Enumerable.Empty<PaymentBasis>().ToList();
			}
		}

		public virtual List<PaymentBasis> GetPricePerSingleChargeable()
		{
			return new List<PaymentBasis>();
		}

		#endregion

#if DEBUG
		public IEnumerable<IRateLineItem> CheckOrCreateItems_ForTest()
			=> CheckOrCreateItems();

		public Quantity ChargeableAmountForBreakSearch_ForTest(AutoRatingCalculatorParameters parameters)
			=> ChargeableAmountForBreakSearch(parameters);
#endif
	}

#if DEBUG
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class TestOnlyCalculatorAttribute : Attribute
	{
	}

	#region Test Calculator

	[TestOnlyCalculator]
	[CalculatorProperty("IT1", RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String1")]
	[CalculatorProperty("IT2", RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool1")]
	[CalculatorProperty("IT3", RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1")]
	[CalculatorProperty("IT4", RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Int1")]
	public class CalculatorToTestCalculatorPropertyAttribute : Calculator
	{
		public CalculatorToTestCalculatorPropertyAttribute(IRateLine master)
			: base(master)
		{
		}

		public const string Code = "TT2";

		public CalculatorPropertyAttribute GetCalculatorPropertyAttribute(string itemType)
		{
			return GetType().GetCustomAttributes(typeof(CalculatorPropertyAttribute), true).Cast<CalculatorPropertyAttribute>().Single(o => o.ItemType == itemType);
		}

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders
		{
			get { return false; }
		}
	}

	[TestOnlyCalculator]
	public class CalculatorForTest : Calculator
	{
		public CalculatorForTest(IRateLine master)
			: base(master)
		{ }

		public const string Code = "CFT";

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders
		{
			get { return false; }
		}

		public Converter<GetQuotationLinesParam, QuotationLineList> GetQuotationLinesInternalOverride { get; set; }

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			if (GetQuotationLinesInternalOverride != null)
			{
				return GetQuotationLinesInternalOverride(flags);
			}
			else
			{
				return base.GetQuotationLinesInternal(flags);
			}
		}

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			calcOutput.Add(calcOutput.Parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(ExpectedAmount, "AUD"), default));
			calcOutput.Minimum = Minimum;
		}

		public ZDecimal ExpectedAmount { get; set; }

		public ZDecimal Minimum
		{
			get { return 2.0M; }
		}
	}

	[TestOnlyCalculator]
	public class CalculatorForCalculationLogTest : Calculator
	{
		public CalculatorForCalculationLogTest(IRateLine master)
			: base(master) { }

		public const string Code = "CLT";

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders
		{
			get { return false; }
		}

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			if (!ForceCalculationResult.IsEmpty)
			{
				calcOutput.Add(calcOutput.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(ForceCalculationResult, Line.TL_RX_NKCurrency), default));
			}
			else
			{
				CalculatePerUnit(calcOutput, new Quantity(PerUnit, Unit));
				AddFlatAmountToLastCalculation(calcOutput, 8m);
			}
			calcOutput.Minimum = Minimum;
			calcOutput.Maximum = Maximum;
			calcOutput.BaseRate = BaseRate;
		}

		public ZDecimal ForceCalculationResult { get; set; }

		public ZDecimal PerUnit
		{
			get { return 4m; }
		}

		public ZDecimal BaseRate
		{
			get { return 16m; }
		}

		public ZDecimal Minimum
		{
			get { return 32m; }
		}

		public ZDecimal Maximum
		{
			get { return 128m; }
		}
	}

	#endregion
#endif
}
