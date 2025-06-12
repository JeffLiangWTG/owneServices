using System;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	public struct WinScpFileInfo
	{
		public string Name { get; set; }
		public string FullName { get; set; }
		public long Length { get; set; }
		public DateTime LastWriteTime { get; set; }

		public WinScpFileInfo(string name, string fullName, long length, DateTime lastWriteTime)
		{
			Name = name;
			FullName = fullName;
			Length = length;
			LastWriteTime = lastWriteTime;
		}
	}
}
