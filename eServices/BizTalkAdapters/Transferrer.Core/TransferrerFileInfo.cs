using System;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Core
{
	[ExcludeFromCodeCoverage]
	public struct TransferrerFileInfo : IEquatable<TransferrerFileInfo>
    {
        public string Name { get; set; }
		public string Folder { get; set; }
		public Int64 Size { get; set; }
        public DateTime Timestamp { get; set; }

		public bool Equals(TransferrerFileInfo other)
		{
			return this.Folder == other.Folder && this.Name == other.Name;
		}

		public override bool Equals(object obj)
		{
			if (obj is TransferrerFileInfo)
				return Equals((TransferrerFileInfo)obj);
			else
				return false;
		}

		public override int GetHashCode()
		{
			return (Folder + Name).GetHashCode();
		}

		public static bool operator ==(TransferrerFileInfo t1, TransferrerFileInfo t2)
		{
			return t1.Equals(t2);
		}

		public static bool operator !=(TransferrerFileInfo t1, TransferrerFileInfo t2)
		{
			return !t1.Equals(t2);
		}
	}
}
