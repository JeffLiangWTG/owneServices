using System.IO;
using NUnit.Framework;

namespace Enterprise.eManifest.DataTransfer.Testing
{
	static class TestFiles
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public static string GetPathFor(string fileName)
		{
			return Path.Combine(TestCase.BaseSourcePath + @"\Enterprise\Product\Operations\eManifest\Testing\DataTransfer\TestFiles\", fileName);
		}
	}
}
