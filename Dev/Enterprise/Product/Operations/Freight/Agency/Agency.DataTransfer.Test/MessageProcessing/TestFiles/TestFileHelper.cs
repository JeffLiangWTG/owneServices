using System.Globalization;
using System.IO;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.TestFiles
{
	static class TestFileHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public static string GetText(string filename)
		{
			var absolutePath = Path.Combine(Path.Combine(TestCase.BaseSourcePath, "Enterprise", "Product", "Operations", "Freight", "Agency", "Agency.DataTransfer.Test", "MessageProcessing", "TestFiles"), filename);
			if (!File.Exists(absolutePath))
			{
				throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "cannot find the test file '{0}'", absolutePath), absolutePath);
			}

			using (var reader = new StreamReader(absolutePath))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
