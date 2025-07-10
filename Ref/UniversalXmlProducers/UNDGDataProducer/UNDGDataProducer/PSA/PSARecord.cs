using System;
namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class PSARecord : IEquatable<PSARecord>
	{
		public string UNNO { get; set; }
		public string IMOClass { get; set; }
		public string PSN { get; set; }
		public string PackagingGroup { get; set; }
		public string PSAGroup { get; set; }
		public string FlashPointLower { get; set; }
		public string FlashPointUpper { get; set; }

		public bool Equals(PSARecord other)
		{
			if (Object.ReferenceEquals(other, null))
			{
				return false;
			}

			if (Object.ReferenceEquals(this, other))
			{
				return false;
			}

			return UNNO.Equals(other.UNNO, StringComparison.Ordinal) && IMOClass.Equals(other.IMOClass, StringComparison.Ordinal) && PSN.Equals(other.PSN, StringComparison.Ordinal) && PackagingGroup.Equals(other.PackagingGroup, StringComparison.Ordinal) && FlashPointUpper.Equals(other.FlashPointUpper, StringComparison.Ordinal) && FlashPointLower.Equals(other.FlashPointLower, StringComparison.Ordinal) && PSAGroup.Equals(other.PSAGroup, StringComparison.Ordinal);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as PSARecord);
		}

		public override int GetHashCode()
		{
			int hashUNNO = UNNO == null ? 0 : UNNO.GetHashCode();
			int hashIMOClass = IMOClass == null ? 0 : IMOClass.GetHashCode();
			int hashPSN = PSN == null ? 0 : PSN.GetHashCode();
			int hashPackagingGroup = PackagingGroup == null ? 0 : PackagingGroup.GetHashCode();
			int hashPSAGroup = PSAGroup == null ? 0 : PSAGroup.GetHashCode();
			int hashFlashPointLower = FlashPointLower == null ? 0 : FlashPointLower.GetHashCode();
			int hashFlashPointUpper = FlashPointUpper == null ? 0 : FlashPointUpper.GetHashCode();

			return hashUNNO ^ hashIMOClass ^ hashPSN ^ hashPackagingGroup ^ hashPSAGroup ^ hashFlashPointLower ^ hashFlashPointUpper;
		}
	}
}
