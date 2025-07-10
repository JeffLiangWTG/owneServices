using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	[DebuggerDisplay("Code: {Code}, Type: {Type}")]
	public class HsnTariffDTYRateTradingPartner : IEquatable<HsnTariffDTYRateTradingPartner>
	{
		public HsnTariffDTYRateTradingPartner(string code)
		{
			Code = code.Trim();

			if (TypeOverrides.TryGetValue(Code, out var type))
			{
				Type = type;
			}
			else
			{
				Type = Code.Length > 2 ? HsnTariffDTYRateTradingPartnerType.TradeGroup : HsnTariffDTYRateTradingPartnerType.Country;
			}
		}

		public static IReadOnlyCollection<HsnTariffDTYRateTradingPartner> From(string cellValue)
			=> cellValue.Trim().SplitBy(DtyRateConstants.TradingPartnerSeparator).Select(x => new HsnTariffDTYRateTradingPartner(x)).ToArray();

		public string Code { get; }

		public HsnTariffDTYRateTradingPartnerType Type { get; }

		#region IEquatable Support

		public bool Equals(HsnTariffDTYRateTradingPartner other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Code == other.Code && Type == other.Type;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((HsnTariffDTYRateTradingPartner)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Code, (int)Type);
		}

		#endregion

		public override string ToString()
		{
			return $"Code: {Code}, Type: {Type}";
		}

		public enum HsnTariffDTYRateTradingPartnerType
		{
			Country,
			TradeGroup
		}

		static Dictionary<string, HsnTariffDTYRateTradingPartnerType> TypeOverrides { get; } =
			new Dictionary<string, HsnTariffDTYRateTradingPartnerType>
			{
				{ "AT", HsnTariffDTYRateTradingPartnerType.TradeGroup },
				{ "BK", HsnTariffDTYRateTradingPartnerType.TradeGroup },
				{ "D8", HsnTariffDTYRateTradingPartnerType.TradeGroup },
			};
	}
}
