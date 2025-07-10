using System;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Messaging;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsValidation : ZValidation
	{
		public NatureAndQtyOfGoodsValidation(NatureAndQtyOfGoods parent)
			: base(parent)
		{
			Argument.NotNull(parent, "parent");
			Parent = parent;
		}

		protected NatureAndQtyOfGoods Parent { get; private set; }

		protected virtual bool IsValidationApplicable
		{
			get { return true; }
		}

		public override Type AutoValidationType
		{
			get { return typeof(NatureAndQtyOfGoodsValidation); }
		}

		public override void ValidateAll()
		{
			ValidateText();
		}

		public void ValidateText()
		{
			ValidateCalculatedProperty(Parent.TextInfo);
		}

		const int MaxTextWidthInMillimeters = 56;

		protected virtual void CheckText()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.TextInfo);

			if (Parent.HasErrors)
			{
				return;
			}
			if (Parent.Text.Length > 15 && Parent.TextSize > MaxTextWidthInMillimeters)
			{
				Parent.TextInfo.AddWarning(Res.GetString("c596113f-0647-4c6e-bc93-2308b4b29862", "The text may be too long to print correctly on the AWB"));
			}

			if (Parent.ParentRateLine.IsDeleted)
			{
				return;
			}

			const int lineMaxLength = FWB.NatureAndQtyMaxLength;

			if (IsAttachedToMAWB
				&& IsNatureAndQtyOfGoodsTypeWithText(Parent.ParentRateLine.ER_NatureAndQtyOfGoodsType)
				&& Parent.Text.Length > lineMaxLength)
			{
				var error = Res.GetString("f147e342-731b-4791-9346-925b76bb2ba7",
					"This field is longer than {0} characters and will be wrapped in the message.",
					lineMaxLength);
				Parent.TextInfo.AddWarning(error);
			}

			if (Parent.ParentRateLine.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber && !IsValidULD(Parent.Text))
			{
				Parent.TextInfo.AddMessageError(Res.GetString("8140204f-8241-4e3f-8710-8dd651000abe", "This is not a valid ULD number."));
			}

			if (Parent.ParentRateLine.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode && !IsValidHCC(Parent.Text))
			{
				Parent.TextInfo.AddMessageError(Res.GetString("603b31e7-1bc3-4d7a-b9cd-de33056615ce", "Harmonized Commodity Code must be between 6 and 18 characters long."));
			}

			if (!AreRateLinesOverridden && Parent.ParentRateLine.IsHSCodeLine && IsLastHSCodeAndHasExtraHSCodes)
			{
				Parent.TextInfo.AddWarning(Res.GetString("a42d0bad-1869-4ef3-99db-71b7df58740a", "More HS Codes exist but cannot be shown due to lack of space."));
			}
		}

		protected bool IsAttachedToMAWB
		{
			get
			{
				var header = Parent.ParentRateLine?.Master;
				return header != null && !header.IsHouseAirWayBill;
			}
		}

		bool IsLastHSCodeAndHasExtraHSCodes
		{
			get
			{
				var master = Parent.ParentRateLine?.Master;
				return master != null
					&& master.LineCountOfLastHSCode == Parent.ParentRateLine.ER_LineCount
					&& master.HasExtraHSCodes;
			}
		}

		bool AreRateLinesOverridden
		{
			get
			{
				var master = Parent.ParentRateLine?.Master;
				return master != null && master.EH_AreRateLinesOverridden;
			}
		}

		bool IsNatureAndQtyOfGoodsTypeWithText(ZString type)
		{
			return type == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription
				|| type == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
		}

		Regex ULDRegex
		{
			get { return uldRegex ?? (uldRegex = new Regex(@"^(?:[a-zA-Z]){1}(?:[a-zA-Z0-9]){3}\d{3,4}(?:(?!\d{2})([a-zA-Z0-9]){2})$", RegexOptions.Compiled | RegexOptions.IgnoreCase)); }
		}
		Regex uldRegex;

		bool IsValidULD(ZString text)
		{
			return ULDRegex.IsMatch(text);
		}

		Regex HCCRegex
		{
			get { return hccRegex ?? (hccRegex = new Regex(@"^(([a-zA-Z0-9]){6,18})$", RegexOptions.Compiled | RegexOptions.IgnoreCase)); }
		}
		Regex hccRegex;

		bool IsValidHCC(ZString text)
		{
			return HCCRegex.IsMatch(text.Replace("HS Code: ", string.Empty));
		}
	}
}
