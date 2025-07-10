using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	class KeyStructurePassarExportTest
	{
		[Test]
		public void TestExportDescriptions()
		{
			DownloadResult keyStructureDownload = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.key_structure_export_latest_format.xlsx"),
			};

			var keyStructure = new KeyStructureExport(keyStructureDownload, false);
			Description description;

			var tariff = $"0102.2191{KeyStructure.KeySeparator}911";
			Assert.IsTrue(keyStructure.TryGetValue(tariff, out description), $"Contains {tariff} (first tariff)");
			Assert.AreEqual("Braunvieh", description.TextD, $"TextD {tariff}");
			Assert.AreEqual("brune", description.TextF, $"TextF {tariff}");
			Assert.AreEqual("di razza bruna", description.TextI, $"TextI {tariff}");
			Assert.AreEqual("brown races", description.TextE, $"TextE {tariff}");

			tariff = $"9706.9000{KeyStructure.KeySeparator}999";
			Assert.IsTrue(keyStructure.TryGetValue(tariff, out description), $"Contains {tariff} (last tariff)");
			Assert.AreEqual("andere", description.TextD, $"TextD {tariff}");
			Assert.AreEqual("autres", description.TextF, $"TextF {tariff}");
			Assert.AreEqual("altri", description.TextI, $"TextI {tariff}");
			Assert.AreEqual("other", description.TextE, $"TextE {tariff}");

			Assert.IsFalse(keyStructure.TryGetValue($"2009.8999{KeyStructure.KeySeparator}", out _), "Does not provide VLS row (no key)");
			Assert.IsFalse(keyStructure.TryGetValue($"2009.8999{KeyStructure.KeySeparator}0", out _), "Does not provide VLS row (key without leading zeries)");
			Assert.IsFalse(keyStructure.TryGetValue($"2009.8999{KeyStructure.KeySeparator}000", out _), "Does not provide VLS row (key with leading zeros)");
		}
	}
}
