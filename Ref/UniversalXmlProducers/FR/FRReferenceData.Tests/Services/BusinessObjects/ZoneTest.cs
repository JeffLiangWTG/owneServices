using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.BusinessObjects
{
	[TestFixture]
	class ZoneTest
	{
		[Test]
		public void GetZoneType()
		{
			var zone = EmptyZone();
			Assert.AreEqual(typeof(Zone), zone.GetType());
		}

		[Test]
		public void GetPercentDomestic()
		{
			var zone = EmptyZone();
			zone.PercentInEu = "16";
			zone.PercentOutEu = "79";
			Assert.AreEqual("5", zone.PercentDomestic);
		}

		Zone EmptyZone()
		{
			return new Zone(string.Empty, string.Empty, string.Empty, string.Empty);
		}
	}
}
