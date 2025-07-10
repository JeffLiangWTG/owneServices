using System;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests
{
	class TemporaryOutputFile : IDisposable
	{
		public TemporaryOutputFile(string outputFileName)
		{
			FullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CH\TestFiles\" + outputFileName);
		}
		public readonly string FullPath;

		public void Dispose()
		{
			if (File.Exists(FullPath))
			{
				File.Delete(FullPath);
			}
		}
	}
}
