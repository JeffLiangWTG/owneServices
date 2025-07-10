using System.Collections.Generic;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers
{
	[TestFixture]
	internal class CarrierDataTests
	{
		[Test]
		public void Constructor()
		{
			var cd = new CarrierData(" C0123456 ", " Carrier Name ", true, true, new List<string> { "ABC", "DEF" });

			Assert.That(cd.CarrierCode, Is.EqualTo("C0123456"));
			Assert.That(cd.CarrierName, Is.EqualTo("Carrier Name"));
			Assert.That(cd.IsAir, Is.EqualTo(true));
			Assert.That(cd.IsSea, Is.EqualTo(true));
			Assert.That(cd.Attributes.Count, Is.EqualTo(2));
		}

		[Test]
		public void MergeAttributes()
		{
			var cd = new CarrierData("C001", "Name", false, false, new List<string> { "ABC", "DEF" });
			Assert.That(cd.Attributes.Count, Is.EqualTo(2));

			cd.MergeAttributes(new[] { "DEF", "GHI", "JKL" });

			Assert.That(cd.Attributes.Count, Is.EqualTo(4));
		}
	}
}
