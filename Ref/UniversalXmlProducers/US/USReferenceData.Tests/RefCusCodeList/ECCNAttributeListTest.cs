using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class ECCNAttributeListTest
	{
		static readonly ICsvParser Parser = new CsvParser();

		[Test]
		public void TestGetMEU()
		{
			var meus = ECCNAttributeList.GetMEU(Parser);
			Assert.AreEqual(47, meus.Count);
		}

		[Test]
		public void TestGetLicenseType()
		{
			var licenseTypes = ECCNAttributeList.GetLicenseType(Parser);
			Assert.AreEqual(795, licenseTypes.Count);
		}
	}
}
