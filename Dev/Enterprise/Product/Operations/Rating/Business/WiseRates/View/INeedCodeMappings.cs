using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public interface INeedCodeMappings
	{
		List<UnmappedForeignCode> UnmappedCodes { get; }
		bool NeedsCodeMapping { get; }
		ZGuid CarrierOrgHeaderPK { get; }
		void SetUnmappedCodes(IEnumerable<UnmappedForeignCode> unmappedCodes);
	}

	public struct UnmappedForeignCode
	{
		public UnmappedForeignCode(string foreignCode, string relationship)
		{
			this.ForeignCode = foreignCode;
			this.Relationship = relationship;
		}

		public string ForeignCode { get; }
		public string Relationship { get; }

		public override bool Equals(object obj)
		{
			var otherUnmappedForeignCode = (UnmappedForeignCode)obj;
			return ForeignCode == otherUnmappedForeignCode.ForeignCode && Relationship == otherUnmappedForeignCode.Relationship;
		}

		public override int GetHashCode()
		{
			return ForeignCode.GetHashCode() ^ Relationship.GetHashCode();
		}

		public override string ToString()
		{
			return ZString.Format("{0} {1}", ForeignCode, Relationship);
		}

		public static bool operator ==(UnmappedForeignCode obj1, UnmappedForeignCode obj2) => obj1.Equals(obj2);
		public static bool operator !=(UnmappedForeignCode obj1, UnmappedForeignCode obj2) => !obj1.Equals(obj2);
	}
}
