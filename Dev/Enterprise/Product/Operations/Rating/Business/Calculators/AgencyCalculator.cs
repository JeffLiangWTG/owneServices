using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(AgencyCalculator.Items.AgencyRate, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal1", RelatedTo = "AgencyRate")]
	[CalculatorProperty(AgencyCalculator.Items.CostPerAdditionalLine, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal2", RelatedTo = "PerAdditionalLine")]
	[CalculatorProperty(AgencyCalculator.Items.IncludedLines, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Int1", RelatedTo = "IncludedLines")]
	[CalculatorProperty(AgencyCalculator.Items.MaximumLines, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Int2", RelatedTo = "MaximumLines")]
	[CalculatorProperty(AgencyCalculator.Items.AdditionalRate, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal5", RelatedTo = "AdditionalRate")]
	[CalculatorProperty(Calculator.Items.Operator.MAX, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal6", ShowWarningIfEmpty = false, RelatedTo = "Maximum")]
	[CalculatorProperty(AgencyCalculator.Items.IncludedHeaders, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Int3", ShowWarningIfEmpty = false, RelatedTo = "IncludedHeaders")]
	[CalculatorProperty(AgencyCalculator.Items.AgencyFeeType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String1", RelatedTo = "AgencyFeeType")]
	[CalculatorProperty(AgencyCalculator.Items.AgencyLineType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String2", RelatedTo = "AgencyLineType")]
	[CalculatorProperty(AgencyCalculator.Items.HideFeeLineTypeOnQuote, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool1", RelatedTo = "HideFeeLineTypeOnQuote")]
	[CalculatorProperty(AgencyCalculator.Items.MessageType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String3", RelatedTo = "MessageType")]
	[CalculatorProperty(AgencyCalculator.Items.MessageSubType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String4", RelatedTo = "MessageSubType")]
	[CalculatorProperty(AgencyCalculator.Items.HideMessageTypeOnQuote, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool2", RelatedTo = "HideMessageTypeOnQuote")]
	public class AgencyCalculator : Calculator
	{
		public AgencyCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.Agency;

		#region Calculator Item Codes

		public new abstract class Items
		{
			public const string AgencyFeeType = "AFT";

			public const string AgencyLineType = "ALT";

			public const string MessageType = "TYP";
			public const string MessageSubType = "SUB";

			public const string AgencyRate = "RAT";
			public const string IncludedHeaders = "INH";
			public const string AdditionalRate = "ADR";
			public const string CostPerAdditionalLine = "CAL";
			public const string IncludedLines = "INC";
			public const string MaximumLines = "MLI";
			public const string HideFeeLineTypeOnQuote = "HID";
			public const string HideMessageTypeOnQuote = "HMT";
		}

		#endregion

		#region Initialisation

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var feeTypeItem = FindRateLineItem(Items.AgencyFeeType);
			var feeTypeNeedToBeSet = feeTypeItem == null || feeTypeItem.TM_Text.IsEmpty;
			var lineTypeItem = FindRateLineItem(Items.AgencyLineType);
			var lineTypeNeedToBeSet = lineTypeItem == null || lineTypeItem.TM_Text.IsEmpty;

			var itemList = base.CheckOrCreateItems();

			if (feeTypeNeedToBeSet)
			{
				var feeTypeLine = itemList.FirstOrDefault(item => item.TM_Type == Items.AgencyFeeType) as RateLineItem;

				if (feeTypeLine != null)
				{
					using (feeTypeLine.SuspendSettingHasChanges())
					using (feeTypeLine.GetValidationSuspender())
					{
						AgencyFeeType = RatingDataRegistry.Instance.DefaultFeeType.Value;
					}
				}
			}

			if (lineTypeNeedToBeSet)
			{
				var lineTypeLine = itemList.FirstOrDefault(item => item.TM_Type == Items.AgencyLineType) as RateLineItem;

				if (lineTypeLine != null)
				{
					using (lineTypeLine.SuspendSettingHasChanges())
					using (lineTypeLine.GetValidationSuspender())
					{
						AgencyLineType = RatingDataRegistry.Instance.DefaultLineType.Value;
					}
				}
			}

			return itemList;
		}

		#endregion

		#region Properties

		#region AgencyFeeType

		public ZString AgencyFeeType
		{
			get => (ZString)this[AgencyCalculator.Items.AgencyFeeType];
			set
			{
				this[AgencyCalculator.Items.AgencyFeeType] = value;
				DefaultIncludedHeadersIfNecessary();
			}
		}

		public ZString AgencyFeeTypeDescription => RatingDataRegistry.Instance.FeeTypeList.GetDescriptionFromCode(AgencyFeeType);

		//Fee Type
		public override ZString String1
		{
			get => base.String1;
			set
			{
				base.String1 = value;
				DefaultIncludedHeadersIfNecessary();
			}
		}

		void DefaultIncludedHeadersIfNecessary()
		{
			if (AgencyFeeType == RateFeeTypeList.Codes.PerShipment)
			{
				IncludedHeaders = 0;
			}
			else if (IncludedHeaders == 0)
			{
				IncludedHeaders = 1;
			}
		}

		#endregion

		#region AgencyLineType

		public ZString AgencyLineType
		{
			get => (ZString)this[AgencyCalculator.Items.AgencyLineType];
			set => this[AgencyCalculator.Items.AgencyLineType] = value;
		}

		public ZString AgencyLineTypeDescription => RatingDataRegistry.Instance.LineTypeList.GetDescriptionFromCode(AgencyLineType);

		#endregion

		#region Agency Rate

		public ZDecimal AgencyRate
		{
			get => (ZDecimal)this[AgencyCalculator.Items.AgencyRate];
			set => this[AgencyCalculator.Items.AgencyRate] = value;
		}

		ZDecimal BaseRate
		{
			get
			{
				if (AgencyFeeType != RateFeeTypeList.Codes.PerEntryPage)
				{
					return AgencyRate;
				}
				return 0m;
			}
		}

		#endregion

		#region IncludedHeaders

		public ZInt IncludedHeaders
		{
			get => (ZInt)this[AgencyCalculator.Items.IncludedHeaders];
			set => this[AgencyCalculator.Items.IncludedHeaders] = value;
		}

		public ZPropertyInfo IncludedHeadersInfo
		{
			get { return ((IGetZPropertyInfo)RateLineBizO).GetZPropertyInfo("Calculator+Int3"); }
		}

		#endregion

		#region Additional Rate

		public ZDecimal AdditionalRate
		{
			get
			{
				if (AgencyFeeType == RateFeeTypeList.Codes.PerShipment)
				{
					return 0m;
				}
				else
				{
					return (ZDecimal)this[AgencyCalculator.Items.AdditionalRate];
				}
			}
			set { this[AgencyCalculator.Items.AdditionalRate] = value; }
		}

		public ZPropertyInfo AdditionalRateInfo
		{
			get { return ((IGetZPropertyInfo)RateLineBizO).GetZPropertyInfo("Calculator+Decimal5"); }
		}

		#endregion

		#region Included Lines

		public ZInt IncludedLines
		{
			get
			{
				if (AgencyLineType == RateLineTypeList.Codes.FLAT)
				{
					return 0;
				}
				else
				{
					return (ZInt)this[AgencyCalculator.Items.IncludedLines];
				}
			}
			set { this[AgencyCalculator.Items.IncludedLines] = (ZDecimal)(decimal)value; }
		}

		public ZPropertyInfo IncludedLinesInfo
		{
			get { return ((IGetZPropertyInfo)RateLineBizO).GetZPropertyInfo("Calculator+Int1"); }
		}

		#endregion

		#region Maximum Lines

		public ZInt MaximumLines
		{
			get
			{
				if (AgencyLineType == RateLineTypeList.Codes.FLAT)
				{
					return 0;
				}
				else
				{
					return (ZInt)this[AgencyCalculator.Items.MaximumLines];
				}
			}
			set { this[AgencyCalculator.Items.MaximumLines] = (ZDecimal)(decimal)value; }
		}

		public ZPropertyInfo MaximumLinesInfo
		{
			get { return ((IGetZPropertyInfo)RateLineBizO).GetZPropertyInfo("Calculator+Int2"); }
		}

		#endregion

		#region Per Additional Line

		public ZDecimal PerAdditionalLine
		{
			get
			{
				if (AgencyLineType == RateLineTypeList.Codes.FLAT)
				{
					return 0m;
				}
				else
				{
					return (ZDecimal)this[AgencyCalculator.Items.CostPerAdditionalLine];
				}
			}
			set { this[AgencyCalculator.Items.CostPerAdditionalLine] = value; }
		}

		public ZPropertyInfo PerAdditionalLineInfo
		{
			get { return ((IGetZPropertyInfo)RateLineBizO).GetZPropertyInfo("Calculator+Decimal2"); }
		}

		#endregion

		#region HideFeeLineTypeOnQuote

		public ZBool HideFeeLineTypeOnQuote
		{
			get => (ZBool)this[AgencyCalculator.Items.HideFeeLineTypeOnQuote];
			set => this[AgencyCalculator.Items.HideFeeLineTypeOnQuote] = value;
		}

		#endregion

		#region Maximum

		public ZDecimal Maximum
		{
			get => (ZDecimal)this[Calculator.Items.Operator.MAX];
			set => this[Calculator.Items.Operator.MAX] = value;
		}

		#endregion

		#region MessageType

		public override ZString MessageType
		{
			get => (ZString)this[AgencyCalculator.Items.MessageType];
			set
			{
				this[AgencyCalculator.Items.MessageType] = value;
				RateLineItems.MarkAsNeedingValidation();
			}
		}

		public ZString MessageTypeDescription
		{
			get { return RateLineBizO.Lookups.MessageTypeList.GetDescriptionFromCode(MessageType); }
		}

		#endregion

		#region MessageSubType

		public override ZString MessageSubType
		{
			get => (ZString)this[AgencyCalculator.Items.MessageSubType];
			set
			{
				this[AgencyCalculator.Items.MessageSubType] = value;
				RateLineItems.MarkAsNeedingValidation();
			}
		}

		public ZString MessageSubTypeDescription => RateLineBizO.Lookups.MessageSubTypeList.GetDescriptionFromCode(MessageSubType);

		protected override bool ShowMessageTypeSubTypeInternal => true;

		#endregion

		#region HideFeeLineTypeOnQuote

		public ZBool HideMessageTypeOnQuote
		{
			get => (ZBool)this[AgencyCalculator.Items.HideMessageTypeOnQuote];
			set => this[AgencyCalculator.Items.HideMessageTypeOnQuote] = value;
		}

		#endregion

		protected internal override bool ValueIsReadOnly(RateLineItem item)
		{
			var result = base.ValueIsReadOnly(item);

			if (!result)
			{
				switch (item.TM_Type)
				{
					case AgencyCalculator.Items.AdditionalRate:
					case AgencyCalculator.Items.IncludedHeaders:
						result = AgencyFeeType == RateFeeTypeList.Codes.PerShipment;
						break;

					case AgencyCalculator.Items.IncludedLines:
					case AgencyCalculator.Items.MaximumLines:
					case AgencyCalculator.Items.CostPerAdditionalLine:
						result = AgencyLineType == RateLineTypeList.Codes.FLAT;
						break;
				}
			}

			return result;
		}

		#endregion

		#region Validation

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			base.ValidateTM_Text(lineItem);

			switch (lineItem.TM_Type)
			{
				case AgencyCalculator.Items.AgencyFeeType:
					MandatoryValidation.CheckEntered(lineItem.TM_TextInfo);
					ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, RatingDataRegistry.Instance.FeeTypeList);
					break;

				case AgencyCalculator.Items.AgencyLineType:
					MandatoryValidation.CheckEntered(lineItem.TM_TextInfo);
					ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, RatingDataRegistry.Instance.LineTypeList);
					break;
			}
		}

		public override void ValidateTM_Value(RateLineItem lineItem)
		{
			base.ValidateTM_Value(lineItem);

			if (lineItem.TM_Type == Items.MaximumLines || lineItem.TM_Type == Items.IncludedLines || lineItem.TM_Type == Items.IncludedHeaders)
			{
				if (lineItem.TM_Value < 0 || lineItem.TM_Value > int.MaxValue)
				{
					lineItem.TM_ValueInfo.AddError(Res.GetString("48c19421-b81e-b5af-4d3d-b8e6eff6b140", "The value must be between 0 and {0}.", int.MaxValue));
				}
			}
		}

		#endregion

		#region Lists

		public override CodeDescriptionPairList List1
		{
			get { return RatingDataRegistry.Instance.FeeTypeList; }
		}

		public override CodeDescriptionPairList List2
		{
			get
			{
				var result = RatingDataRegistry.Instance.LineTypeList;

				if (AllowInvoiceLinesFromDifferentInvoicesToBeMergedIntoSameTariffLines)
				{
					result.RemoveCode(RateLineTypeList.Codes.PerTariffLinePerInvoice);
				}

				return result;
			}
		}

		bool AllowInvoiceLinesFromDifferentInvoicesToBeMergedIntoSameTariffLines
		{
			get
			{
				var company = ParentRatingHeader?.Company ?? GlbCompany.CurrentCompany;

				return company.GC_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedStates;
			}
		}

		public override CodeDescriptionPairList List3 => RateLineBizO.Lookups.MessageTypeList;

		public override CodeDescriptionPairList List4 => RateLineBizO.Lookups.MessageSubTypeList;

		#endregion

		#region Quotation Lines

		/// <summary>
		/// Agency Charge Code for Import - Per Entry, Per Tariff Line Per Entry
		///		First 1 Entry, First 5 Tariff Lines for each entry			AUD 100
		///		Additional Entries											USD 10
		///		Thereafter													USD 5 per line
		///		Maximum Lines												50 lines
		/// </summary>
		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			var description = GetDescription();

			if (AgencyFeeType == RateFeeTypeList.Codes.PerShipment && AgencyLineType == RateLineTypeList.Codes.FLAT)
			{
				var type = RateDescriptionFlags(flags);

				if (description.Length > 0)
				{
					type |= QuotationLineType.MergeWithRateLineDescription;
				}

				result.Add(QuotationLine.NewWithValue(Line, type, AgencyCalculator.Items.AgencyRate, description.ToString(), (NoResString)ZString.Empty));
			}
			else
			{
				if (description.Length == 0)
				{
					result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
				}
				else
				{
					result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags), description.ToString()));
				}

				var lineTypeHeaderText = RateLineTypeList.GetDescriptiveHeaderText(AgencyLineType);
				var lineType = RateLineTypeList.GetTypeOfLine(AgencyLineType, IncludedLines > 1);

				if (AgencyFeeType == RateFeeTypeList.Codes.PerShipment)
				{
					var lineDesc = BaseRateIncluded(IncludedLines, lineType, lineTypeHeaderText);

					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, AgencyCalculator.Items.AgencyRate, lineDesc, (NoResString)ZString.Empty));
				}
				else
				{
					var headerType = RateFeeTypeList.GetHeaderType(AgencyFeeType, IncludedHeaders > 1);

					//i.e. First 1 Supplier
					var agencyRateDesc = FirstText(IncludedHeaders, headerType);

					if (AgencyLineType != RateLineTypeList.Codes.FLAT)
					{
						//i.e. First 5 tariff lines for each entry
						var headerTypeForLine = RateLineTypeList.GetDescriptiveHeaderText(AgencyLineType);

						var agencyRateAddDesc = FirstText(IncludedLines, lineType, headerTypeForLine);

						agencyRateDesc += agencyRateAddDesc;
					}

					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, AgencyCalculator.Items.AgencyRate, agencyRateDesc, (NoResString)ZString.Empty));

					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, AgencyCalculator.Items.AdditionalRate, AdditionalText(headerType), (NoResString)ZString.Empty));
				}

				var perAdditionalLineLine = PerAdditionalLine.IsEmpty ? null : QuotationLine.NewWithValue(Line, 0, AgencyCalculator.Items.CostPerAdditionalLine, ThereAfterText, PerLineText);

				if (perAdditionalLineLine != null)
				{
					result.Add(perAdditionalLineLine);
					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.f0 | QuotationLineType.NoCurrency, AgencyCalculator.Items.MaximumLines, MaximumLinesText, LinesText));
				}
			}

			result.Add(QuotationLine.Maximum(Line));

			return result;
		}

		StringBuilder GetDescription()
		{
			var builder = new StringBuilder();

			if (!HideMessageTypeOnQuote && !(MessageType.IsEmpty && MessageSubType.IsEmpty))
			{
				builder.Append(ForText);

				if (!MessageType.IsEmpty)
				{
					builder.Append(' ');
					builder.Append(MessageTypeDescription);
				}

				if (!MessageSubType.IsEmpty)
				{
					builder.Append(' ');
					builder.Append(MessageSubTypeDescription);
				}
			}

			if (!HideFeeLineTypeOnQuote)
			{
				if (builder.Length > 0)
				{
					builder.Append(' ');
				}

				builder.Append("- ");
				builder.Append(AgencyFeeTypeDescription);
				builder.Append(", ");
				builder.Append(AgencyLineTypeDescription);
			}

			return builder;
		}

		static string BaseRateIncluded(int includedLines, string lineType, string lineTypeHeaderText) => Res.GetString("b75d5fb3-fc8d-444b-9265-cfae425fc23c", "Base Rate ({0} {1} {2} Included)", includedLines, lineType, lineTypeHeaderText);

		static string FirstText(int includedHeaders, string headerType) => Res.GetString("2b28e550-6030-4bc5-ac90-ca6d56d7e6bb", "First {0} {1}", includedHeaders, headerType);

		static string FirstText(int includedLines, string lineType, string headerTypeForLine) => Res.GetString("2313b71a-06ee-496c-bc4e-c085461c41bc", ", First {0} {1} {2}", includedLines, lineType, headerTypeForLine);

		static string AdditionalText(string headerType) => Res.GetString("b7a636e9-3eba-43d0-9881-511f65dfdc97", "Additional {0}", headerType);

		static string ForText => Res.GetString("48c19421-b81e-4d3d-b5af-eff6b140b8e6", "for");

		static string MaximumLinesText => Res.GetString("99b138ca-79c0-44bd-85b3-ccaa48fe3c6b", "Maximum Lines");

		static ResourceString LinesText => ResString.GetMultilingualString("e1300795-3f7a-43ce-a7e0-e5a7f5db8bb3", "lines");

		static ResourceString PerLineText => ResString.GetMultilingualString("9b1b76df-ca9e-44c7-b601-fde9e23b5c95", "per line");

		static string ThereAfterText => Res.GetString("80d476c1-15f9-4e83-a356-b73762d6163b", "Thereafter");

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			var description = GetDescription();

			if (AgencyFeeType == RateFeeTypeList.Codes.PerShipment && AgencyLineType == RateLineTypeList.Codes.FLAT)
			{
				var flat = QuotationLine.GetValue(AgencyCalculator.Items.AgencyRate, Line.ChildRateLineItems);
				result.SetFlat(Line.TL_RX_NKCurrency, flat, description.ToString());
			}
			else
			{
				var lineTypeHeaderText = RateLineTypeList.GetDescriptiveHeaderText(AgencyLineType);
				var lineType = RateLineTypeList.GetTypeOfLine(AgencyLineType, IncludedLines > 1);

				var agencyRate = 0m;
				var agencyRateDescription = "";

				var additionalRate = 0m;
				var additionalRateDescription = "";

				var costPerAdditionalRate = 0m;
				var maxLines = 0m;
				var maxLinesDescription = "";
				var maxUnit = "";

				if (AgencyFeeType == RateFeeTypeList.Codes.PerShipment)
				{
					agencyRateDescription = BaseRateIncluded(IncludedLines, lineType, lineTypeHeaderText);
					agencyRate = QuotationLine.GetValue(AgencyCalculator.Items.AgencyRate, Line.ChildRateLineItems);
				}
				else
				{
					var headerType = RateFeeTypeList.GetHeaderType(AgencyFeeType, IncludedHeaders > 1);

					//i.e. First 1 Supplier
					var agencyRateDesc = FirstText(IncludedHeaders, headerType);
					if (AgencyLineType != RateLineTypeList.Codes.FLAT)
					{
						//i.e. First 5 tariff lines for each entry
						var headerTypeForLine = RateLineTypeList.GetDescriptiveHeaderText(AgencyLineType);
						var agencyRateAddDesc = FirstText(IncludedLines, lineType, headerTypeForLine);
						agencyRateDesc += agencyRateAddDesc;
					}

					agencyRate = QuotationLine.GetValue(AgencyCalculator.Items.AgencyRate, Line.ChildRateLineItems);
					agencyRateDescription = agencyRateDesc;

					additionalRate = QuotationLine.GetValue(AgencyCalculator.Items.AdditionalRate, Line.ChildRateLineItems);
					additionalRateDescription = AdditionalText(headerType);
				}

				var perAdditionalLineLine = PerAdditionalLine.IsEmpty
					? 0
					: QuotationLine.GetValue(AgencyCalculator.Items.CostPerAdditionalLine, Line.ChildRateLineItems);
				if (perAdditionalLineLine != 0)
				{
					costPerAdditionalRate = perAdditionalLineLine;
					maxLines = QuotationLine.GetValue(AgencyCalculator.Items.MaximumLines, Line.ChildRateLineItems);
					maxLinesDescription = MaximumLinesText;
					maxUnit = LinesText;
				}

				result.SetAgency(Line.TL_RX_NKCurrency, description.ToString(), agencyRate, agencyRateDescription, additionalRate, additionalRateDescription, costPerAdditionalRate, ThereAfterText, PerLineText);
			}

			var max = QuotationLine.GetValue(Calculator.Items.Operator.MAX, Line.ChildRateLineItems);
			result.SetMax(Line.TL_RX_NKCurrency, max);

			return result;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			if (!ValidateRateLineItemsIfExists(Items.MaximumLines, Items.IncludedLines, Items.IncludedHeaders))
			{
				// should ideally pass validation errors as a failure message
				calcOutput.IsEmpty = true;
				return;
			}

			var results = new List<PaymentBasis>();
			var parameters = calcOutput.Parameters;
			var criteria = parameters.Criteria;
			TotalAdditionalLinesForCalculation = 0;

			if (!AdditionalRate.IsEmpty && AgencyFeeType != RateFeeTypeList.Codes.PerShipment)
			{
				results.AddRange(CalculateHeaders(criteria));
			}

			void SetCalcOutputResults()
			{
				calcOutput.Add(results);
				calcOutput.BaseRate = BaseRate;
				calcOutput.Maximum = Maximum;
			}

			if (AgencyLineType == RateLineTypeList.Codes.FLAT)
			{
				// Doesn't need more calculations. Just add existing ones to calculation output and return.
				SetCalcOutputResults();
				return;
			}

			switch (AgencyLineType)
			{
				case RateLineTypeList.Codes.PerInvoiceLinePerEntry:
				case RateLineTypeList.Codes.PerTariffLinePerEntry:
					results.Add(GetAdditionalLineCharge(criteria, criteria.Entries));
					break;

				case "":
					ReportUnexpectedBlankAgencyLineType(criteria, calcOutput);
					return;

				default:
					results.Add(GetAdditionalLineCharge(criteria, null));
					break;
			}

			if (results.Any())
			{
				SetCalcOutputResults();
			}
		}

		/// <summary>
		/// For CS01210355 - WI00568372, we could not find exact causes.
		/// Data fix has also been applied but just in case it will happen again, we report the calculator data for further investigation.
		/// </summary>
		void ReportUnexpectedBlankAgencyLineType(RatingCriteria criteria, CalculatorOutput calcOutput = null)
		{
			#region SuppressResourceStringsCheckRegion

			var ratingHeader = Line.ParentRateEntry.ParentRatingHeader;
			var rateType = ratingHeader.DisplayInfo();
			if (rateType.IsEmpty)
			{
				rateType = "client rates / costings / company tariffs";
			}

			var inspectionMessage = new ZStringBuilder("AgencyLineType should not be empty but it is.");
			inspectionMessage.AppendLine($"RateLine: {Line.DisplayInfo()}.");
			foreach (var item in Line.ChildRateLineItems)
			{
				inspectionMessage.Append($"RateLineItems: {item.TM_Type}|{item.TM_Text}|{item.TM_Value}|{item.TM_RelevantValue}.");
			}

			var jobMeasures = criteria.RateableMeasures;
			foreach (var measureType in Enum.GetValues(typeof(MeasureType)).Cast<MeasureType>())
			{
				if (jobMeasures.HasMeasureType(measureType))
				{
					inspectionMessage.Append($"Measure {measureType.ToString()}: {jobMeasures.ValueAndUnitString(measureType)}.");
				}
			}

			foreach (var jobService in criteria.JobServices)
			{
				if (jobService.IsEnabled)
				{
					inspectionMessage.Append($"{jobService}.");
				}
			}

			if (_Rating.IsOn && _Rating.Interactor != null)
			{
				_Rating.Interactor.Error($"Could not calculate with AGY calculator due to missing Agency Line Type. Please check {rateType} {ratingHeader.Header?.OH_Code}.");
			}

			var nameOfAgencyLineType = nameof(AgencyLineType);
			ErrorReporter.ReportOnce(
				"AgencyCalculator|CalculateInternal|AgencyLineType.IsEmpty",
				inspectionMessage.ToStringWithNewLineBetweenAppends(),
				new ArgumentException("Agency Line Type must not be empty", nameOfAgencyLineType)
			);

			// Should not return any calculation.
			if (calcOutput != null)
			{
				calcOutput.IsEmpty = true;
			}

			#endregion
		}

		List<PaymentBasis> CalculateHeaders(RatingCriteria criteria)
		{
			var results = new List<PaymentBasis>();
			var unit = AgencyLineType;
			int additionalCountToShow;

			if (AgencyFeeType == RateFeeTypeList.Codes.PerEntryPage)
			{
				if (unit.IsEmpty)
				{
					// Unable to calculate if the unit is blank. Add a test for this case.
					ReportUnexpectedBlankAgencyLineType(criteria);
					return results;
				}

				var chargeable = new Quantity(criteria.Entries.Count, unit);
				var rateInfo = RateInfo.CreateUNT(AgencyRate, unit, Line.TL_RX_NKCurrency, RateFeeTypeList.GetHeaderType(AgencyFeeType, false), unitMultiplier: UnitMultiplier);

				results.Add(criteria.CreatePaymentBasis(rateInfo, chargeable, RateFeeTypeList.GetHeaderType(AgencyFeeType, criteria.Entries.Count > 1).ToLower(CultureInfo.CurrentCulture)));

				var numLinesFirstPage = criteria.IsImport() ? Env.Registry.Rating.AgencyCalcLinesFirstPageImport : Env.Registry.Rating.AgencyCalcLinesFirstPageExport;
				var numLinesAdditionalPage = criteria.IsImport() ? Env.Registry.Rating.AgencyCalcLinesAdditionalPageImport : Env.Registry.Rating.AgencyCalcLinesAdditionalPageExport;

				var totalAdditionalPages = 0;
				foreach (var entry in criteria.Entries)
				{
					var additionalPages = (entry.EntryLines - numLinesFirstPage) / (decimal)numLinesAdditionalPage;
					var numAdditionalPages = GetAdditionalHeaders(1 + (int)Math.Ceiling(additionalPages));// first page + additional pages - included pages
					totalAdditionalPages += numAdditionalPages;
				}

				var additionalChargeable = new Quantity(totalAdditionalPages, unit);
				var additionalRateInfo = RateInfo.CreateUNT(AdditionalRate, unit, Line.TL_RX_NKCurrency, RateFeeTypeList.GetHeaderType(AgencyFeeType, false), unitMultiplier: UnitMultiplier);

				additionalCountToShow = totalAdditionalPages;
				var additionalHeaderType = RateFeeTypeList.GetHeaderType(AgencyFeeType, additionalCountToShow > 1) ?? "";
				var includedHeaderDesc = IncludedHeaders > 1 ? " " + Res.GetString("edab513a-a4cb-46ee-8545-d9676f23c09a", "above {0} (incl)", IncludedHeaders) : "";
				var chargeableDescription = Res.GetString("e0e2f36d-32b3-4183-a45d-a45938f264af", "additional {0}{1}", additionalHeaderType.ToLower(Culture.CurrentCompanyCountryCulture), includedHeaderDesc);
				results.Add(criteria.CreatePaymentBasis(additionalRateInfo, additionalChargeable, chargeableDescription));
			}
			else
			{
				var additionalCount = 0;

				switch (AgencyFeeType)
				{
					case RateFeeTypeList.Codes.PerEntry:
						additionalCount = GetAdditionalHeaders(criteria.Entries.Count);
						break;

					case RateFeeTypeList.Codes.PerSupplier:
						additionalCount = GetAdditionalHeaders(criteria.Invoices.UniqueSuppliers.Count);
						break;

					case RateFeeTypeList.Codes.PerInvoice:
						additionalCount = GetAdditionalHeaders(criteria.Invoices.Count);
						break;

					case RateFeeTypeList.Codes.PerSubHeader:
						additionalCount = GetAdditionalHeaders(criteria.SubHeaderCount);
						break;
				}

				additionalCount = Math.Max(additionalCount, 0);
				additionalCountToShow = additionalCount;

				var chargeable = new Quantity(additionalCount, AgencyFeeType);
				var rateInfo = RateInfo.CreateUNT(AdditionalRate, AgencyFeeType, Line.TL_RX_NKCurrency, RateFeeTypeList.GetHeaderType(AgencyFeeType, false), unitMultiplier: UnitMultiplier);
				results.Add(criteria.CreatePaymentBasis(rateInfo, chargeable, RateFeeTypeList.GetHeaderType(AgencyFeeType, additionalCountToShow > 1)));
			}

			return results;
		}

		int GetAdditionalHeaders(int count)
		{
			return count - IncludedHeaders;
		}

		int TotalAdditionalLinesForCalculation;

		PaymentBasis GetAdditionalLineCharge(RatingCriteria criteria, EntryInfoCollection entryInfos)
		{
			var additionalLines = 0;

			if (entryInfos != null)//per entry
			{
				foreach (var entryInfo in entryInfos)
				{
					additionalLines += GetAdditionalLines(GetLineCount(entryInfo));
				}
			}
			else if (AgencyLineType != RateLineTypeList.Codes.FLAT)
			{
				switch (AgencyLineType)
				{
					case RateLineTypeList.Codes.PerInvoiceLinePerInvoice:
					case RateLineTypeList.Codes.PerTariffLinePerInvoice:
						additionalLines += criteria.Invoices.Sum(x => GetAdditionalLines(GetLineCount(x)));
						break;

					case RateLineTypeList.Codes.PerHTSCodePerInvoice:
						additionalLines += criteria.TariffsPerInvoice.Sum(x => GetAdditionalLines(GetLineCount(x)));
						break;

					case RateLineTypeList.Codes.PerHTSCodePerShipment:
					case RateLineTypeList.Codes.TotalHTSCountPerDeclaration:
						additionalLines += criteria.TariffsPerShipment.Sum(x => GetAdditionalLines(GetLineCount(x)));
						break;

					default:
						additionalLines = GetAdditionalLines(GetTotalLineCount(criteria.Entries));
						break;
				}
			}

			TotalAdditionalLinesForCalculation += additionalLines;

			var unit = AgencyLineType;

			var rateInfo = RateInfo.CreateUNT(PerAdditionalLine, unit, Line.TL_RX_NKCurrency, RateLineTypeList.GetTypeOfLine(AgencyLineType) ?? AgencyLineType, unitMultiplier: UnitMultiplier);
			var chargeable = new Quantity(additionalLines, unit);
			return criteria.CreatePaymentBasis(rateInfo, chargeable, GetAdditionalLinesDescription());
		}

		#region Calculation Implementation

		int GetLineCount<T>(T info) where T : ILineCountInfo
		{
			switch (AgencyLineType)
			{
				case RateLineTypeList.Codes.PerInvoiceLinePerEntry:
				case RateLineTypeList.Codes.PerInvoiceLinePerInvoice:
				case RateLineTypeList.Codes.PerInvoiceLinePerShipment:
				case RateLineTypeList.Codes.PerHTSCodePerInvoice:
				case RateLineTypeList.Codes.PerHTSCodePerShipment:
					return info.InvoiceLines;

				case RateLineTypeList.Codes.PerTariffLinePerEntry:
				case RateLineTypeList.Codes.PerTariffLinePerInvoice:
				case RateLineTypeList.Codes.PerTariffLinePerShipment:
				case RateLineTypeList.Codes.TotalHTSCountPerDeclaration:
					return info.EntryLines;

				default:
					return 0;
			}
		}

		int GetTotalLineCount<T>(List<T> infoList) where T : ILineCountInfo
		{
			var result = 0;

			foreach (ILineCountInfo info in infoList)
			{
				result += GetLineCount(info);
			}

			return result;
		}

		int GetAdditionalLines(int lineCount)
		{
			var result = 0;
			if (lineCount > IncludedLines)
			{
				result = (MaximumLines == 0 || lineCount <= MaximumLines) ? lineCount - IncludedLines : MaximumLines - IncludedLines;
			}
			return result;
		}

		#endregion

		#region Description

		ZString GetAdditionalLinesDescription()
		{
			var typeOfLine = GetLineTypeDescription().ToLower();

			var sb = new ZStringBuilder();

			if (IncludedLines != 0 && MaximumLines != 0)
			{
				sb.Append(Res.GetString("75e90709-0eed-4383-9f45-a1dd8c34681e", "additional {0} between {1} (incl) and {2} (max)", typeOfLine, IncludedLines, MaximumLines));
			}
			else if (IncludedLines != 0)
			{
				sb.Append(Res.GetString("c043274b-9cd9-483d-9561-551331867ffc", "additional {0} above {1} (incl)", typeOfLine, IncludedLines));
			}
			else if (MaximumLines != 0)
			{
				sb.Append(Res.GetString("1ec21411-b879-45ca-8797-fcf9e578ffb5", "{0} up to {1} (max)", typeOfLine, MaximumLines));
			}
			else
			{
				sb.Append(typeOfLine);
			}

			var headerText = RateLineTypeList.GetDescriptiveHeaderText(AgencyLineType);

			if (!string.IsNullOrEmpty(headerText))
			{
				sb.Append(" " + headerText);
			}

			return sb.ToString();
		}

		ZString GetLineTypeDescription()
		{
			var typeOfLine = RateLineTypeList.GetTypeOfLine(AgencyLineType, TotalAdditionalLinesForCalculation > 1) ?? "";
			if (string.IsNullOrEmpty(typeOfLine))
			{
				return AgencyLineType;
			}

			return typeOfLine;
		}

		#endregion

		#endregion

		#region SetCloneLineItems

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			clone.RateLineItems.RemoveAndDeleteAll();
			foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
			{
				var newItem = clone.RateLineItems.CloneItem(item);
				if (ShouldValueBeDiscounted(item))
				{
					newItem.UpdateRateValue(x => x * (ctbCalc.Percent + 100) / 100, rateTypeToUpdate);

					if (item.TM_Type == AgencyCalculator.Items.AgencyRate)
					{
						newItem.UpdateRateValue(x => x + ctbCalc.BaseRateWithApplicableIncrease, rateTypeToUpdate);
					}

					newItem.UpdateRateValue(x => Utilities.Round(x, 2), rateTypeToUpdate);
				}
			}
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool IsMeasureTypeMatchApplicable => false;
	}
}

