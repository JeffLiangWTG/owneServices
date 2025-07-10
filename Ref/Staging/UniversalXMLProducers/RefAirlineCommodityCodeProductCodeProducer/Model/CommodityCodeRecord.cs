using System;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer
{
	public class CommodityCodeRecord : IEquatable<CommodityCodeRecord>
	{
		public CommodityCodeRecord(string airlineId, string commodityCode, string commodityDescription, string specialHandlingCode)
		{
			AirlineID = airlineId;
			CommodityCode = commodityCode;
			CommodityDescription = commodityDescription;
			SpecialHandlingCodes = specialHandlingCode?.Replace(" ", "").ToUpperInvariant();
		}

		public string AirlineID { get; }

		public string CommodityCode { get; }

		public string CommodityDescription { get; }

		public string SpecialHandlingCodes { get; set; }

		public static bool operator ==(CommodityCodeRecord left, CommodityCodeRecord right)
		{
			if (ReferenceEquals(left, right))
			{
				return true;
			}
			if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
			{
				return false;
			}
			return left.AirlineID == right.AirlineID
				   && left.CommodityCode == right.CommodityCode
				   && left.CommodityDescription == right.CommodityDescription;
		}

		public static bool operator !=(CommodityCodeRecord left, CommodityCodeRecord right)
		{
			return !(left == right);
		}

		bool IEquatable<CommodityCodeRecord>.Equals(CommodityCodeRecord other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			return this == obj as CommodityCodeRecord;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + CommodityCode.GetHashCode();
				hash = hash * 23 + CommodityDescription.GetHashCode();
				hash = hash * 23 + AirlineID.GetHashCode();

				return hash;
			}
		}

		public void MergeSpecialHandlingCodes(CommodityCodeRecord commodityCodeRecord)
		{
			if (commodityCodeRecord == null)
			{
				return;
			}

			var currentSpecialHandlingCodes = SpecialHandlingCodes.Replace(" ", "").ToUpperInvariant().Split(',');
			var combined = currentSpecialHandlingCodes.Union(commodityCodeRecord.SpecialHandlingCodes.Replace(" ", "").ToUpperInvariant().Split(',')).ToList();

			SpecialHandlingCodes = string.Join(",", combined);
		}
	}
}
