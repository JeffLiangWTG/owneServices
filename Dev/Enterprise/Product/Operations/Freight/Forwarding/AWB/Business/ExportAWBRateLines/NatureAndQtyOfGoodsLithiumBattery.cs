using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsLithiumBattery : NatureAndQtyOfGoods
	{
		public NatureAndQtyOfGoodsLithiumBattery(ExportAWBRateLine parentRateLine)
			: base(parentRateLine)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static new class Schema
		{
			public const string LithiumBatteryType = "LithiumBatteryType";
		}

		[List("LithiumBatteryTypeList")]
		[MaxLength(5)]
		public ZString LithiumBatteryType
		{
			get { return lithiumBatteryType; }
			set
			{
				if (SetNonPersistentPropertyValue(LithiumBatteryTypeInfo, ref lithiumBatteryType, value) && !IsValidationSuspended)
				{
					Validation.ValidateLithiumBatteryType();
				}
				description = LithiumBatteryTypeList.GetDescriptionFromCode(value);
				wrappedDescriptions = Wrap(description, ExportAWBRateLine.Schema.ER_NatureAndQtyOfGoodsMaxLength);
			}
		}
		ZString lithiumBatteryType;

		public ZPropertyInfo LithiumBatteryTypeInfo
		{
			get { return GetZPropertyInfo(Schema.LithiumBatteryType); }
		}

		public CodeDescriptionPairList LithiumBatteryTypeList
		{
			get { return GetLithiumBatteryTypeList(); }
		}

		protected virtual CodeDescriptionPairList GetLithiumBatteryTypeList()
		{
			return new CodeDescriptionPairList(OLookUpEditType.AWBLithiumBatteryType);
		}

		ZString description;

		public List<ZString> WrappedDescriptions
		{
			get { return wrappedDescriptions ?? (wrappedDescriptions = new List<ZString>()); }
		}
		List<ZString> wrappedDescriptions;

		List<ZString> Wrap(ZString text, int maxLength)
		{
			var wrappedTexts = new List<ZString>();

			while (!string.IsNullOrEmpty(text))
			{
				var endWhitespacePtr = 0;
				for (var i = 0; i < maxLength && i < text.Length; i++)
				{
					if (i == text.Length - 1 || char.IsWhiteSpace(text[i]) || (text.Length > i + 1 && char.IsWhiteSpace(text[i + 1])))
					{
						endWhitespacePtr = i + 1;
					}
				}

				wrappedTexts.Add(text.SubstringSafe(0, endWhitespacePtr));
				text = text.SubstringSafe(endWhitespacePtr);
			}

			return wrappedTexts;
		}

		protected override ZString TextCore
		{
			get { return Serialize(); }
			set { Deserialize(value); }
		}

		static readonly Lazy<Regex> regex = new Lazy<Regex>(() => new Regex(@"^Lithium Battery: (?<LithiumBattery>[0-9a-zA-Z]{3,5})$", RegexOptions.Compiled | RegexOptions.IgnoreCase));

		protected ZString Serialize()
		{
			if (LithiumBatteryTypeList.ContainsCode(LithiumBatteryType))
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"Lithium Battery: {0}", LithiumBatteryType); // Lithium Battery Type Serialization
			}
			return ZString.Empty;
		}

		protected void Deserialize(ZString text)
		{
			var match = regex.Value.Match(text);
			LithiumBatteryType = match.Success ? new ZString(match.Groups["LithiumBattery"].Value) : ZString.Empty;
		}

		public static bool IsValidLithiumBatteryType(ZString text)
		{
			var match = regex.Value.Match(text);
			return match.Success;
		}

		public new NatureAndQtyOfGoodsLithiumBatteryValidation Validation
		{
			get { return (NatureAndQtyOfGoodsLithiumBatteryValidation)base.Validation; }
		}

		protected override NatureAndQtyOfGoodsValidation GetValidation()
		{
			return new NatureAndQtyOfGoodsLithiumBatteryValidation(this);
		}
	}
}
