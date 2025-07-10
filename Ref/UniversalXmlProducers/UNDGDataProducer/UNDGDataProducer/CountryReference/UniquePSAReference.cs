using System;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public struct UniquePSAReference : IEquatable<UniquePSAReference>
	{
		public UniquePSAReference(string code, bool hasFlashPointLower, decimal flashPointLowerCentigrade)
		{
			Code = code;
			HasFlashPointLower = hasFlashPointLower;
			FlashPointLowerCentigrade = flashPointLowerCentigrade;
		}

		public string Code { get; }
		public bool HasFlashPointLower { get; }
		public decimal FlashPointLowerCentigrade { get; }

		public static bool operator ==(UniquePSAReference left, UniquePSAReference right)
		{
			return left.Code == right.Code &&
				left.HasFlashPointLower == right.HasFlashPointLower &&
				left.FlashPointLowerCentigrade == right.FlashPointLowerCentigrade;
		}

		public static bool operator !=(UniquePSAReference left, UniquePSAReference right) => !(left == right);

		public override bool Equals(object obj)
		{
			if (obj is UniquePSAReference reference)
			{
				return this == reference;
			}

			return false;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + Code.GetHashCode();
				hash = hash * 23 + HasFlashPointLower.GetHashCode();
				hash = hash * 23 + FlashPointLowerCentigrade.GetHashCode();

				return hash;
			}
		}

		bool IEquatable<UniquePSAReference>.Equals(UniquePSAReference other)
		{
			return other == this;
		}
	}
}
