using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsSLAC : NatureAndQtyOfGoods
	{
		public NatureAndQtyOfGoodsSLAC(ExportAWBRateLine parentRateLine)
			: base(parentRateLine)
		{
		}

		public static new class Schema
		{
			public const string Count = "Count";
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

		static readonly Lazy<Regex> regex = new Lazy<Regex>(() => new Regex(@"^(?<Count>\d{1,5}) SLAC$", RegexOptions.Compiled | RegexOptions.IgnoreCase));

		protected ZString Serialize()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} SLAC", Count); // SLAC Serialization
		}

		protected void Deserialize(ZString value)
		{
			var match = regex.Value.Match(value);
			Count = match.Success ? Convert.ToInt32(match.Groups["Count"].Value, CultureInfo.InvariantCulture) : 0;
		}

		public static bool IsValidSLAC(ZString text)
		{
			var match = regex.Value.Match(text);
			return match.Success && Convert.ToInt32(match.Groups["Count"].Value, CultureInfo.InvariantCulture) > 0;
		}

		public new NatureAndQtyOfGoodsSLACValidation Validation
		{
			get { return (NatureAndQtyOfGoodsSLACValidation)base.Validation; }
		}

		protected override NatureAndQtyOfGoodsValidation GetValidation()
		{
			return new NatureAndQtyOfGoodsSLACValidation(this);
		}
	}
}
