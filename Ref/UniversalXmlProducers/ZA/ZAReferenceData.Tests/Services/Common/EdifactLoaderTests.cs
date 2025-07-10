using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common
{
	[TestFixture]
	public class EdifactLoaderTests
	{
		[Test]
		public void LoadProdatMessage_Valid()
		{
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestFiles.Input.D96B_Valid_01.txt");
			var msg = EdifactLoader.LoadProdatMessage(msgContent);

			Assert.That(msg, Is.Not.Null);
			Assert.That(msg.Group8, Is.Not.Null);
		}

		[Test]
		public void LoadProdatMessage_Invalid()
		{
			var msgContent = "This is not an Edifact Message";
			var msg = EdifactLoader.LoadProdatMessage(msgContent);
			Assert.That(msg, Is.Null);
		}

		[Test]
		public void LoadGesmesMessage_Valid()
		{
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates.TestFiles.Input.Gesmes_Valid_01.txt");
			var msg = EdifactLoader.LoadGesmesMessage(msgContent);

			Assert.That(msg, Is.Not.Null);
			Assert.That(msg.Group8, Is.Not.Null);
		}

		[Test]
		public void LoadGesmesMessage_Invalid()
		{
			var msgContent = "This is not an Edifact Message";
			var msg = EdifactLoader.LoadGesmesMessage(msgContent);
			Assert.That(msg, Is.Null);
		}
	}
}
