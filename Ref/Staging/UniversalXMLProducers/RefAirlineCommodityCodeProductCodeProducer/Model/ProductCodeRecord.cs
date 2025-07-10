using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer
{
	public class ProductCodeRecord : IEquatable<ProductCodeRecord>
	{
		public ProductCodeRecord(string airlineId, string productCode, string productDescription)
		{
			AirlineID = airlineId;
			ProductCode = productCode;
			ProductDescription = productDescription;
		}

		public string AirlineID { get; }

		public string ProductCode { get; }

		public string ProductDescription { get; }

		public static bool operator ==(ProductCodeRecord left, ProductCodeRecord right)
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
				   && left.ProductCode == right.ProductCode
				   && left.ProductDescription == right.ProductDescription;
		}

		public static bool operator !=(ProductCodeRecord left, ProductCodeRecord right)
		{
			return !(left == right);
		}

		bool IEquatable<ProductCodeRecord>.Equals(ProductCodeRecord other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			return this == obj as ProductCodeRecord;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + ProductCode.GetHashCode();
				hash = hash * 23 + ProductDescription.GetHashCode();
				hash = hash * 23 + AirlineID.GetHashCode();

				return hash;
			}
		}
	}
}
