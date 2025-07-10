using CargoWise.RefDbRepo.CAReferenceData.Business.CAOfficeCode;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CAOfficeCode
{
	[TestFixture]
	public class CAProvinceLookupTest
	{
		[Test]
		public void TestGetProvince()
		{
			Assert.AreEqual("NS", CAProvinceLookup.GetProvince("0000"));
			Assert.AreEqual("PE", CAProvinceLookup.GetProvince("0100"));
			Assert.AreEqual("NB", CAProvinceLookup.GetProvince("0200"));
			Assert.AreEqual("QC", CAProvinceLookup.GetProvince("0300"));
			Assert.AreEqual("ON", CAProvinceLookup.GetProvince("0400"));
			Assert.AreEqual("MB", CAProvinceLookup.GetProvince("0500"));
			Assert.AreEqual("SK", CAProvinceLookup.GetProvince("0600"));
			Assert.AreEqual("AB", CAProvinceLookup.GetProvince("0700"));
			Assert.AreEqual("BC", CAProvinceLookup.GetProvince("0800"));
			Assert.AreEqual("NL", CAProvinceLookup.GetProvince("0900"));

			Assert.AreEqual("MB", CAProvinceLookup.GetProvince("0511"));
			Assert.AreEqual("NT", CAProvinceLookup.GetProvince("0512"));
			Assert.AreEqual("NT", CAProvinceLookup.GetProvince("0513"));
			Assert.AreEqual("NT", CAProvinceLookup.GetProvince("0514"));
			Assert.AreEqual("NT", CAProvinceLookup.GetProvince("0515"));
			Assert.AreEqual("MB", CAProvinceLookup.GetProvince("0516"));

			Assert.AreEqual("YT", CAProvinceLookup.GetProvince("0890"));
			Assert.AreEqual("YT", CAProvinceLookup.GetProvince("0892"));
			Assert.AreEqual("YT", CAProvinceLookup.GetProvince("0894"));
		}
	}
}
