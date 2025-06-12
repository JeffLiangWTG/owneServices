using System;
using System.Diagnostics.CodeAnalysis;

namespace Hawking.CSI.Plugins.FTP
{
	public struct FtpFileInfo : IEquatable<FtpFileInfo>
    {
        public string Name { get; set; }
		public string Folder { get; set; }
		public Int64 Size { get; set; }
        public DateTime Timestamp { get; set; }

		public bool Equals(FtpFileInfo other)
		{
			return this.Folder == other.Folder && this.Name == other.Name;
		}

		public override bool Equals(object obj)
		{
			if (obj is FtpFileInfo)
				return Equals((FtpFileInfo)obj);
			else
				return false;
		}

		public override int GetHashCode()
		{
			return (Folder + Name).GetHashCode();
		}

		public static bool operator ==(FtpFileInfo t1, FtpFileInfo t2)
		{
			return t1.Equals(t2);
		}

		public static bool operator !=(FtpFileInfo t1, FtpFileInfo t2)
		{
			return !t1.Equals(t2);
		}
	}
}
