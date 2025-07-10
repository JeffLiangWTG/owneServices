using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	class KeyStructureEdecImportTest
	{
		[Test]
		public void TestImportDescriptions() => Assert.Multiple(() =>
		{
			DownloadResult keyStructureDownload = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.key_structure_import_latest_format.xlsx"),
			};

			var keyStructure = new KeyStructureImport(keyStructureDownload, false);
			Description description;

			var tariff = $"0101.2110{KeyStructure.KeySeparator}911";
			Assert.IsTrue(keyStructure.TryGetValue(tariff, out description), $"Contains {tariff} (first tariff)");
			Assert.AreEqual(@"Fohlen bei Fuss der Mutter (gemäss ""Erläuterungen"")", description.TextD, $"TextD {tariff}");
			Assert.AreEqual(@"poulains sous la mère (conformément aux ""Notes explicatives"")", description.TextF, $"TextF {tariff}");
			Assert.AreEqual(@"puledri accompagnati dalla madre (secondo le ""Note esplicative"")", description.TextI, $"TextI {tariff}");
			Assert.AreEqual(@"foals at the mother's foot (according to the ""Explanatory notes"")", description.TextE, $"TextE {tariff}");

			tariff = $"9706.9000{KeyStructure.KeySeparator}999";
			Assert.IsTrue(keyStructure.TryGetValue(tariff, out description), $"Contains {tariff} (last tariff)");
			Assert.AreEqual("andere", description.TextD, $"TextD {tariff}");
			Assert.AreEqual("autres", description.TextF, $"TextF {tariff}");
			Assert.AreEqual("altri", description.TextI, $"TextI{tariff}");
			Assert.AreEqual("other", description.TextE, $"TextE {tariff}");

			Assert.IsFalse(keyStructure.TryGetValue($"2009.9069{KeyStructure.KeySeparator}", out _), "Does not provide VLS row (empty key)");
			Assert.IsFalse(keyStructure.TryGetValue($"2009.9069{KeyStructure.KeySeparator}0", out _), "Does not provide VLS row - (key without leading zeros)");
			Assert.IsFalse(keyStructure.TryGetValue($"2009.9069{KeyStructure.KeySeparator}000", out _), "Does not provide VLS row - (key with leading zeros)");
		});
	}
}
