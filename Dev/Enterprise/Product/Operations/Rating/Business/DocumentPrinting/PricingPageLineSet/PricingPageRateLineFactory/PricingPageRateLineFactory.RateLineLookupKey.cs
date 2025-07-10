using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public partial class PricingPageRateLineFactory
	{
		sealed class RateLineLookupKey : IEquatable<RateLineLookupKey>
		{
			public RateLineLookupKey(RateEntry parentEntry, RateLine line)
			{
				chargeCodePK = line.ChargeCode.PK;
				chargeCodeDescription = line.GetMultilingualRateDesc();
				port = GetPort(parentEntry, line);
				mode = GetMode(parentEntry);
				messageType = line.Calculator.MessageType;
				messageSubType = line.Calculator.MessageSubType;

				if (parentEntry.IsWHS())
				{
					warehouse = line.Parent.TI_WW_Warehouse;
					product = line.TL_OP_ProductNumber;
				}

				if (parentEntry.IsTRW() || parentEntry.IsTWU())
				{
					warehouse = line.Parent.TI_WW_Warehouse;
				}

				unchecked
				{
					var calc = (uint)chargeCodePK.GetHashCode();
					calc = ((calc << 27) | (calc >> 5)) ^ (uint)chargeCodeDescription.GetHashCode();
					calc = ((calc << 27) | (calc >> 5)) ^ (uint)port.GetHashCode();
					calc = ((calc << 27) | (calc >> 5)) ^ (uint)mode.GetHashCode();
					calc = ((calc << 27) | (calc >> 5)) ^ (uint)messageType.GetHashCode();
					calc = ((calc << 27) | (calc >> 5)) ^ (uint)messageSubType.GetHashCode();
					calc = ((calc << 27) | (calc >> 5)) ^ (uint)warehouse.GetHashCode();
					calc = ((calc << 27) | (calc >> 5)) ^ (uint)product.GetHashCode();
					hashCode = (int)calc;
				}
			}

			public override bool Equals(object obj) => Equals(obj as RateLineLookupKey);

			public bool Equals(RateLineLookupKey other)
				=> hashCode == other.hashCode
					&& chargeCodePK.Equals(other.chargeCodePK)
					&& chargeCodeDescription.Equals(other.chargeCodeDescription)
					&& port.Equals(other.port)
					&& mode.Equals(other.mode)
					&& messageType.Equals(other.messageType)
					&& messageSubType.Equals(other.messageSubType)
					&& warehouse.Equals(other.warehouse)
					&& product.Equals(other.product);

			public override int GetHashCode() => hashCode;

			static ZString GetPort(RateEntry parentEntry, RateLine line)
			{
				if (line.Parent.IsOriginEntry() && parentEntry.Origin() != null)
				{
					if (parentEntry.Origin().CompletelyCovers(line.Parent.Origin()))
					{
						return line.Parent.TI_OriginLRC;
					}
					else
					{
						return parentEntry.TI_OriginLRC;
					}
				}
				else if (line.Parent.IsDestinationEntry() && parentEntry.Destination() != null)
				{
					if (parentEntry.Destination().CompletelyCovers(line.Parent.Destination()))
					{
						return line.Parent.TI_DestinationLRC;
					}
					else
					{
						return parentEntry.TI_DestinationLRC;
					}
				}
				else if (line.Parent.IsFreightEntry())
				{
					return parentEntry.TI_OriginLRC + "|" + parentEntry.TI_DestinationLRC;
				}
				else
				{
					return line.Parent.TI_OriginLRC + "|" + line.Parent.TI_DestinationLRC;
				}
			}

			static ZString GetMode(RateEntry parentEntry)
			{
				if (parentEntry.TI_RateCategory == RatingConstants.RateCategory.FCL)
				{
					switch (parentEntry.TI_Mode)
					{
						case Core.Constants.RateMode.SEA:
							return Core.Constants.RateMode.FCL;
						case Core.Constants.RateMode.ROA:
							return Core.Constants.RateMode.FRO;
						case Core.Constants.RateMode.RAI:
							return Core.Constants.RateMode.FRA;
						default:
							return ZString.Empty;
					}
				}
				else
				{
					return parentEntry.TI_Mode;
				}
			}

			readonly int hashCode;
			readonly ZGuid chargeCodePK;
			readonly ZString chargeCodeDescription;
			readonly ZString port;
			readonly ZString mode;
			readonly ZString messageType;
			readonly ZString messageSubType;
			readonly ZGuid warehouse;
			readonly ZGuid product;
		}
	}
}
