using System.Collections.Generic;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestClasses;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers
{
	[TestFixture]
	internal class VesselDataTests
	{
		[Test]
		public void Constructor()
		{
			var cd = new CarrierData("C001", "Name", false, true, new List<string> { "ABC", "DEF" });
			var vd = new VesselData(" Rad-call-sign ", " Vessel Name ", cd);

			Assert.That(vd.RadioCallSign, Is.EqualTo("Rad-call-sign"));
			Assert.That(vd.VesselName, Is.EqualTo("Vessel Name"));
			Assert.That(vd.Carrier, Is.EqualTo(cd));
		}

		[Test]
		public void NormalizeName()
		{
			var vd = new VesselDataForTest("R01", " Vessel Name ", null);

			var result = vd.NormalizeNameExposed("  Yiğit buradaydı  ");

			Assert.That(result, Is.EqualTo("Yigit buradaydı"));
		}
	}
}
