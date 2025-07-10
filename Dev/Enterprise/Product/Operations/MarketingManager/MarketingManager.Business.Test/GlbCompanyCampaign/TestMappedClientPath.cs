using System;
using Enterprise.Integration.RemoteDesktopServices;

namespace Enterprise.MarketingManager.Business.Testing
{
	public sealed class TestMappedClientPath : IMappedClientPath
	{
		public bool FindMappedPath { get; set; }

		public string GetMappedPath(string unmappedPath)
		{
			return FindMappedPath ? unmappedPath : null;
		}

		public string GetUnmappedPath(string mappedPath)
		{
			throw new NotImplementedException();
		}
	}
}
