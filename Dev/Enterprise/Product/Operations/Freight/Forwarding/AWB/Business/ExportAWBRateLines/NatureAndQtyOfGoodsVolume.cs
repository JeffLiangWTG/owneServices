using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	[ProvideMetaDataProperty("NumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class NatureAndQtyOfGoodsVolume : NatureAndQtyOfGoods
	{
		public NatureAndQtyOfGoodsVolume(ExportAWBRateLine parentRateLine)
			: base(parentRateLine)
		{
		}

		public static new class Schema
		{
			public const string Volume = "Volume";
			public const string Unit = "Unit";
		}

		[DecimalPlaces(2)]
		[MeasureUnit(Schema.Unit, MeasureUnitType.Volume)]
		public ZDecimal Volume
		{
			get { return volume; }
			set
			{
				ZDecimal roundedValue = GetRoundedVolume(value);

				if (SetNonPersistentPropertyValue(VolumeInfo, ref volume, roundedValue) && !IsValidationSuspended)
				{
					Validation.ValidateVolume();
				}
			}
		}
		ZDecimal volume;

		protected virtual decimal GetRoundedVolume(decimal value)
		{
			return decimal.Ceiling(value * 100) / 100;
		}

		public ZPropertyInfo VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.Volume); }
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
					Volume = GetRoundedVolume(Volume);
				}
			}
		}
		ZString unit;

		public CodeDescriptionPairList UnitList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public ZPropertyInfo UnitInfo
		{
			get { return GetZPropertyInfo(Schema.Unit); }
		}

		protected override ZString TextCore
		{
			get { return Serialize(); }
			set { Deserialize(value); }
		}

		protected ZString Serialize()
		{
			int decimalPlaces = GetNumberOfDecimalsCore(VolumeInfo);
			decimalPlaces = decimalPlaces >= 0 ? decimalPlaces : 2;
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"VOL {0} {1}", Volume.ToString(decimalPlaces), Unit).TrimEnd(); // Volume Serialization
		}

		protected void Deserialize(ZString value)
		{
			var match = regex.Value.Match(value);

			var success = match.Success && match.Groups["Volume"].Value.Length < 10;

			Unit = success ? match.Groups["Unit"].Value : string.Empty;
			Volume = success ? Convert.ToDecimal(match.Groups["Volume"].Value, CultureInfo.InvariantCulture) : 0m;
		}

		public int GetNumberOfDecimals(PropertyDescriptor property)
		{
			return NumberOfDecimalsHelper.GetNumberOfDecimals(this, property, GetNumberOfDecimalsCore);
		}

		protected virtual int GetNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return -1;
		}

		static readonly Lazy<Regex> regex = new Lazy<Regex>(() => new Regex(@"^VOL (?<Volume>\d{1,6}(?:\.\d{1,3})?)(?: (?<Unit>[0-9a-zA-Z]{1,2}))$", RegexOptions.Compiled | RegexOptions.IgnoreCase));

		public static bool IsValidVolume(ZString text)
		{
			var match = regex.Value.Match(text);
			return match.Success && match.Groups["Volume"].Value.Length < 10;
		}

		public new NatureAndQtyOfGoodsVolumeValidation Validation
		{
			get { return (NatureAndQtyOfGoodsVolumeValidation)base.Validation; }
		}

		protected override NatureAndQtyOfGoodsValidation GetValidation()
		{
			return new NatureAndQtyOfGoodsVolumeValidation(this);
		}
	}
}
