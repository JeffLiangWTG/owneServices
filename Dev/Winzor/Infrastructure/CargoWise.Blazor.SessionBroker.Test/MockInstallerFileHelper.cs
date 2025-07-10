using System;
using System.IO;
using System.Threading.Tasks;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public static class MockInstallerFileHelper
	{
		public static async Task<string> CreateMockInstallerFile(string filepath, string expectedContent = null)
		{
			expectedContent ??= Guid.NewGuid().ToString();
			await using var sw = File.CreateText(filepath);
			await sw.WriteAsync(expectedContent);

			return expectedContent;
		}
	}
}
