using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.Registry.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CodeStringFinderSupportedReturnType]
	public class QuotationLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructor

		QuotationLine(IRateLine rateLine, QuotationLineType type, ZString description, MultilingualString amountAsString, ZString currency, MultilingualString unit)
			: this(rateLine, type, description, DocAmount.Create(amountAsString), currency, unit)
		{
		}

		QuotationLine(IRateLine rateLine, QuotationLineType type, ZString description, ZDecimal amount, ZString currency, MultilingualString unit)
			: this(rateLine, type, description, DocAmount.Create(amount, type), currency, unit)
		{
		}

		QuotationLine(IRateLine rateLine, QuotationLineType type, ZString description, DocAmount docAmount, ZString currency, MultilingualString unit)
		{
			//todo: QuotationLine should not coupled to RateLine i.e. DocRollUpSort doesn't need it
			this.Master = rateLine as RateLine;

			if (Master == null)
			{
				throw new ArgumentNullException(nameof(rateLine));
			}

			this.Type = type;
			this.description = description;
			this.DocAmount = docAmount;
			this.Currency = (type & QuotationLineType.NoCurrency) == 0 ? currency : ZString.Empty;
			this.Unit = unit;
			if ((type & QuotationLineType.UseOverrideDescription) != 0)
			{
				this.NumberOfTabs = 0;
			}
			else
			{
				this.NumberOfTabs = (type & QuotationLineType.UseDescriptionMask) == 0 ? 1 : 0;
			}
		}

		public readonly RateLine Master;
		public QuotationLineType Type { get; private set; }

		#endregion

		#region Common Calculator Properties

		public static QuotationLine BaseRate(IRateLine rateLine, QuotationLineType type)
			=> BaseRate(rateLine, type, null, ZString.Empty);

		public static QuotationLine BaseRate(IRateLine rateLine, QuotationLineType type, IEnumerable<IRateLineItem> items, ZString description)
			=> NewWithValue(rateLine, type, items, Calculator.Items.Operator.BAS, description.IsEmpty ? RatingDataRegistry.Instance.BaseRateText.Value : description, (NoResString)ZString.Empty);

		public static QuotationLine PerUnit(IRateLine rateLine, QuotationLineType type)
			=> PerUnit(rateLine, type, null, ZString.Empty);

		public static QuotationLine PerUnit(IRateLine rateLine, QuotationLineType type, IEnumerable<IRateLineItem> items, ZString description)
		{
			if (DocumentsDataRegistry.Instance.AlternativeRateFormat.Value)
			{
				type |= QuotationLineType.AlternativeFormat;
			}

			var addContainerCode = Calculator.ShouldAddContainerCode(type);
			var weightVolumeDescription = rateLine.Calculator.UnitDescription(true, addContainerCode: addContainerCode);

			return NewWithValue
			(
				rateLine,
				type,
				items,
				Calculator.Items.Operator.UNT,
				description.IsEmpty ? (ZString)Res.GetString("2a2b39f6-b63b-490b-bc89-64a2ed7e6bfd", "Per Unit") : description,
				ResString.GetMultilingualString("0829f9ca-14cd-4357-97de-83530144cb01", "per {0}", weightVolumeDescription)
			);
		}

		public static QuotationLine Minimum(IRateLine rateLine, QuotationLineType type)
			=> Minimum(rateLine, type, null, ZString.Empty);

		public static QuotationLine Minimum(IRateLine rateLine, QuotationLineType type, IEnumerable<IRateLineItem> items, ZString description)
			=> NewWithValue(rateLine, type, items, Calculator.Items.Operator.MIN, description.IsEmpty ? (ZString)Res.GetString("26410fbe-d0e3-4b4d-bd71-2f9c9534ada2", "Minimum") : description, (NoResString)ZString.Empty);

		public static QuotationLine Maximum(IRateLine rateLine)
			=> Maximum(rateLine, null, ZString.Empty);

		public static QuotationLine Maximum(IRateLine rateLine, IEnumerable<IRateLineItem> items, ZString description)
			=> NewWithValue(rateLine, 0, items, Calculator.Items.Operator.MAX, description.IsEmpty ? (ZString)Res.GetString("6272263b-97bd-4d55-9e81-cf3bd2124f8b", "Maximum") : description, (NoResString)ZString.Empty);

		public static QuotationLine Header(IRateLine rateLine, QuotationLineType type)
			=> new QuotationLine(rateLine, type, ZString.Empty, (NoResString)ZString.Empty, ZString.Empty, (NoResString)ZString.Empty);

		public static QuotationLine Header(IRateLine rateLine, QuotationLineType type, ZString additionalDescription)
			=> new QuotationLine(rateLine, type | QuotationLineType.MergeWithRateLineDescription, additionalDescription, (NoResString)ZString.Empty, ZString.Empty, (NoResString)ZString.Empty);

		public static QuotationLine HeaderWithDescription(IRateLine rateLine, QuotationLineType type, ZString description)
			=> new QuotationLine(rateLine, type , description, (NoResString)ZString.Empty, ZString.Empty, (NoResString)ZString.Empty);

		public static QuotationLine HeaderWithDescriptionCurrencyAmountUnits(IRateLine rateLine, QuotationLineType type, ZString description, ZString currency, MultilingualString amount, MultilingualString unit)
			=> new QuotationLine(rateLine, type, description, DocAmount.Create(amount), currency, unit);

		public static QuotationLine ContractNumber(IRateLine rateLine)
		{
			if (rateLine == null)
			{
				throw new ArgumentNullException(nameof(rateLine));
			}

			var entry = rateLine.ParentRateEntry;

			if (entry == null || entry.TI_ContractNumber.IsEmpty)
			{
				return null;
			}
			else
			{
				return new QuotationLine(rateLine, QuotationLineType.NoCurrency, Res.GetString("4e737769-251a-43dc-8185-87370987e114", "Contract Number: {0}", entry.TI_ContractNumber), (NoResString)ZString.Empty, ZString.Empty, (NoResString)ZString.Empty);
			}
		}

		public static QuotationLine RateNote(IRateLine rateLine)
		{
			if (rateLine == null)
			{
				throw new ArgumentNullException(nameof(rateLine));
			}

			var text = rateLine.ChargeInformationNoteText;

			if (text.IsEmpty)
			{
				return null;
			}
			else
			{
				return new QuotationLine(rateLine, QuotationLineType.NoCurrency, Res.GetString("d6114464-a79d-4572-9343-55cc76a60fe9", "Note: {0}", text), (NoResString)ZString.Empty, ZString.Empty, (NoResString)ZString.Empty);
			}
		}

		public static QuotationLine ConditionsApply(IRateLine rateLine)
		{
			if (rateLine == null)
			{
				throw new ArgumentNullException(nameof(rateLine));
			}

			if (!rateLine.TL_Condition.IsEmpty)
			{
				return new QuotationLine(rateLine, QuotationLineType.NoCurrency, Res.GetString("d1974ede-cd52-4bed-87f5-93b606e5ba83", "*conditions apply"), (NoResString)ZString.Empty, ZString.Empty, (NoResString)ZString.Empty);
			}

			return null;
		}

		#region New With Value

		public static QuotationLine New(IRateLine rateLine, ZString description)
			=> new QuotationLine(rateLine, 0, description, (NoResString)ZString.Empty, ZString.Empty, (NoResString)ZString.Empty);

		public static QuotationLine NewWithValue(IRateLine rateLine, QuotationLineType type, string itemType, ZString description, MultilingualString unit)
			=> NewWithValue(rateLine, type, null, itemType, description, unit);

		public static QuotationLine NewWithValue(IRateLine rateLine, QuotationLineType type, IEnumerable<IRateLineItem> items, string itemType, ZString description, MultilingualString unit)
			=> NewWithValue(rateLine, type, GetValue(itemType, items ?? rateLine.ChildRateLineItems), description, unit);

		public static QuotationLine NewWithValue(IRateLine rateLine, QuotationLineType type, IRateLineItem item, ZString description, MultilingualString unit)
			=> NewWithValue(rateLine, type, item.TM_RelevantValue, description, unit);

		public static QuotationLine New(IRateLine rateLine, QuotationLineType type, ZString description, MultilingualString value, MultilingualString unit)
			=> new QuotationLine(rateLine, type, description, value, ZString.Empty, unit);

		public static QuotationLine NewWithValue(IRateLine rateLine, QuotationLineType type, ZString description, ZDecimal value, MultilingualString unit)
			=> new QuotationLine(rateLine, type, description, value, ZString.Empty, unit);

		public static QuotationLine NewWithValue(IRateLine rateLine, QuotationLineType type, ZDecimal value, ZString description, MultilingualString unit, string currency = default)
		{
			if (value.IsEmpty)
			{
				if ((type & QuotationLineType.Mandatory) != 0)
				{
					return new QuotationLine(rateLine, type, description, Env.Registry.NotChargedMultilingualText, ZString.Empty, (NoResString)ZString.Empty);
				}
				else
				{
					return null;
				}
			}
			else
			{
				return new QuotationLine(rateLine, type, description, value, !string.IsNullOrEmpty(currency) ? currency : rateLine.TL_RX_NKCurrency, unit);
			}
		}

		#endregion

		public static QuotationLine Spacing(IRateLine rateLine) => new QuotationLine(rateLine, 0, ZString.Empty, (NoResString)ZString.Empty, ZString.Empty, (NoResString)ZString.Empty);

		#endregion

		public void AddDescription(ZString additionalDescription)
		{
			if ((Type & QuotationLineType.UseDescriptionMask) != 0)
			{
				this.Type |= QuotationLineType.MergeWithRateLineDescription;

				if (description.IsEmpty)
				{
					description = additionalDescription;
				}
				else if (!additionalDescription.IsEmpty)
				{
					description += " " + additionalDescription;
				}
			}
		}

		public ZString GetDescription(ZString incoTerm, bool? hasLocalClientForRelatedRates = null)
		{
			var builder = new StringBuilder();

			if ((Type & QuotationLineType.UseDescriptionMask) != 0)
			{
				if ((Type & QuotationLineType.UseDescriptionMask) == QuotationLineType.UseChargeDescription)
				{
					builder.Append(ChargeCodeDescription);
				}
				else
				{
					builder.Append(Master.GetMultilingualRateDesc(hasLocalClientForRelatedRates));
				}

				if (Master.MayGSTBeApplicable(incoTerm))
				{
					builder.Append(" *");
				}

				if ((Type & QuotationLineType.MergeWithRateLineDescription) != 0 && !description.IsEmpty)
				{
					builder.Append(' ');
					builder.Append(description);
				}
			}
			else
			{
				builder.Append(description);
			}

			return builder.ToString();
		}

		public MultilingualString Amount => DocAmount.AmountAsString;

		public ZString Currency { get; private set; }

		public ZInt DecimalPlaces => Master.DecimalPlaces();

		DocAmount DocAmount { get; set; }

		public MultilingualString Unit { get; private set; }

		public ZString Mode
		{
			get
			{
				var parent = Master.ParentRateEntry;

				if (parent == null)
				{
					return ZString.Empty;
				}
				else if (parent.TI_RateCategory == RatingConstants.RateCategory.SCO)
				{
					return Core.Constants.RateMode.FCL;
				}
				else if (parent.TI_RateCategory == RatingConstants.RateCategory.FCL)
				{
					if (parent.IsSea())
					{
						return Core.Constants.RateMode.FCL;
					}
					else if (parent.IsRoad())
					{
						return Core.Constants.RateMode.FRO;
					}
					else if (parent.IsRail())
					{
						return Core.Constants.RateMode.FRA;
					}
				}

				return parent.TI_Mode;
			}
		}

		public ZString Container => Master.ParentRateEntry?.Container?.RC_Code ?? ZString.Empty;

		public ZString Validity
		{
			get
			{
				if ((Type & QuotationLineType.UseDescriptionMask) != 0)
				{
					ZString validFrom = Master.ParentRateEntry.TI_RateStartDate.ToString("d");
					ZString validUntil = Master.ParentRateEntry.TI_RateEndDate.ToString("d");

					if (!validFrom.IsEmpty || !validUntil.IsEmpty)
					{
						return validFrom + "-" + validUntil;
					}
				}

				return ZString.Empty;
			}
		}

		public void Shift()
		{
			NumberOfTabs++;
		}

		public void Shift(ZString newRateDescription)
		{
			if ((Type & QuotationLineType.UseDescriptionMask) != 0)
			{
				Type &= ~QuotationLineType.UseDescriptionMask;

				if ((Type & QuotationLineType.MergeWithRateLineDescription) != 0)
				{
					Type &= ~QuotationLineType.MergeWithRateLineDescription;

					if (description.IsEmpty)
					{
						description = newRateDescription;
					}
					else if (!newRateDescription.IsEmpty)
					{
						description = newRateDescription + " " + description;
					}
				}
				else
				{
					description = newRateDescription;
				}
			}

			Shift();
		}

		public void MergeDescriptions(ZString additionalDescription)
		{
			if (!additionalDescription.IsEmpty)
			{
				if ((Type & QuotationLineType.UseDescriptionMask) != 0 && (Type & QuotationLineType.MergeWithRateLineDescription) == 0)
				{
					description = additionalDescription;
					Type |= QuotationLineType.MergeWithRateLineDescription;
				}
				else if (description.IsEmpty)
				{
					description = additionalDescription;
				}
				else
				{
					description += " " + additionalDescription;
				}
			}
		}

		public void MakeEmpty()
		{
			DocAmount = DocAmount.Create(Env.Registry.NotChargedMultilingualText);
			Currency = ZString.Empty;
			Unit = (NoResString)ZString.Empty;
		}

		public override string ToString() => GetDescription("ALL") + "|" + Currency + "|" + Amount + "|" + Unit;

		public override bool Equals(object obj) => ToString().Equals(obj.ToString());

		public override int GetHashCode() => ToString().GetHashCode();

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		ZString ChargeCodeDescription
		{
			get
			{
				var code = Master.ChargeCode;

				if (code == null)
				{
					return "";
				}
				else if (!code.AC_LocalLanguageDescription.IsEmpty && ObjectFactory.Get<IAccounting>().EnableLocalChargeCodeDescriptionDefault && (IsClientInSameCountry || ObjectFactory.Get<IAccounting>().ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors))
				{
					return code.AC_LocalLanguageDescription;
				}
				else
				{
					return code.AC_Desc;
				}
			}
		}

		ZString description;

		bool IsClientInSameCountry => Master?.ParentRateEntry?.ParentRatingHeader?.Header != null
			&& Master.ParentRateEntry.ParentRatingHeader.Header.IsLocalCountry;

		public ZInt NumberOfTabs { get; set; }

		public static ZDecimal GetValue(string itemType, IEnumerable<IRateLineItem> items) => items.FirstOrDefault(i => i.TM_Type == itemType)?.TM_RelevantValue ?? 0m;

		#endregion
	}
}

