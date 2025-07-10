using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsDimensions : NatureAndQtyOfGoods
	{
		public NatureAndQtyOfGoodsDimensions(ExportAWBRateLine parentRateLine)
			: base(parentRateLine)
		{
		}

		public static new class Schema
		{
			public const string Length = "Length";
			public const string Width = "Width";
			public const string Height = "Height";
			public const string Count = "Count";
			public const string Unit = "Unit";
		}

		public ZInt Length
		{
			get { return length; }
			set
			{
				if (SetNonPersistentPropertyValue(LengthInfo, ref length, value) && !IsValidationSuspended)
				{
					Validation.ValidateLength();
				}
			}
		}
		ZInt length;

		public ZPropertyInfo LengthInfo
		{
			get { return GetZPropertyInfo(Schema.Length); }
		}

		public ZInt Width
		{
			get { return width; }
			set
			{
				if (SetNonPersistentPropertyValue(WidthInfo, ref width, value) && !IsValidationSuspended)
				{
					Validation.ValidateWidth();
				}
			}
		}
		ZInt width;

		public ZPropertyInfo WidthInfo
		{
			get { return GetZPropertyInfo(Schema.Width); }
		}

		public ZInt Height
		{
			get { return height; }
			set
			{
				if (SetNonPersistentPropertyValue(HeightInfo, ref height, value) && !IsValidationSuspended)
				{
					Validation.ValidateHeight();
				}
			}
		}
		ZInt height;

		public ZPropertyInfo HeightInfo
		{
			get { return GetZPropertyInfo(Schema.Height); }
		}

		[List("UnitList")]
		[MaxLength(2)]
		public ZString Unit
		{
			get { return unit; }
			set
			{
				if (SetNonPersistentPropertyValue(UnitInfo, ref unit, value) && !IsValidationSuspended)
				{
					Validation.ValidateUnit();
				}
			}
		}
		ZString unit;

		public CodeDescriptionPairList UnitList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}

		public ZPropertyInfo UnitInfo
		{
			get { return GetZPropertyInfo(Schema.Unit); }
		}

		public ZInt Count
		{
			get { return count; }
			set
			{
				if (SetNonPersistentPropertyValue(CountInfo, ref count, value) && !IsValidationSuspended)
				{
					Validation.ValidateCount();
				}
			}
		}
		ZInt count;

		public ZPropertyInfo CountInfo
		{
			get { return GetZPropertyInfo(Schema.Count); }
		}

		protected override ZString TextCore
		{
			get { return Serialize(); }
			set { Deserialize(value); }
		}

		protected ZString Serialize()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"DIMS {0}x{1}x{2} {3} x {4}", Length, Width, Height, Unit, Count); // Dimensions Serialization
		}

		protected void Deserialize(ZString value)
		{
			var match = regex.Value.Match(value);

			Length = match.Success ? Convert.ToInt32(match.Groups["Length"].Value, CultureInfo.InvariantCulture) : 0;
			Width = match.Success ? Convert.ToInt32(match.Groups["Width"].Value, CultureInfo.InvariantCulture) : 0;
			Height = match.Success ? Convert.ToInt32(match.Groups["Height"].Value, CultureInfo.InvariantCulture) : 0;
			Unit = match.Success ? match.Groups["Unit"].Value : string.Empty;
			Count = match.Success ? Convert.ToInt32(match.Groups["Count"].Value, CultureInfo.InvariantCulture) : 0;
		}

		static readonly Lazy<Regex> regex = new Lazy<Regex>(() => new Regex(@"^DIMS (?<Length>\d{1,5})x(?<Width>\d{1,5})x(?<Height>\d{1,5})(?: (?<Unit>[0-9a-zA-Z]{1,2}))(?: x (?<Count>\d{1,4}))$", RegexOptions.Compiled | RegexOptions.IgnoreCase));

		public static bool IsValidDimension(ZString text)
		{
			return text != ZString.Empty && regex.Value.IsMatch(text);
		}

		public new NatureAndQtyOfGoodsDimensionsValidation Validation
		{
			get { return (NatureAndQtyOfGoodsDimensionsValidation)base.Validation; }
		}

		protected override NatureAndQtyOfGoodsValidation GetValidation()
		{
			return new NatureAndQtyOfGoodsDimensionsValidation(this);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("da99b7bd-296f-449d-8b41-c84e37fdcd07", "Nature and Quantity Of Goods Dimensions");
	}
}
