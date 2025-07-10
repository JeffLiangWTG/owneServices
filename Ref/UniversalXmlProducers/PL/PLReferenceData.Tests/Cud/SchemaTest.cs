using System;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Cud;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Cud
{
	[TestFixture]
	sealed class SchemaTest : CudExchangeRate
	{
		[Test]
		public void TestDeserializeMessage()
		{
			var dataToParse = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Cud.TestFiles.Input.eurofxref-hist-90d.xml"));
			var result = XmlParser.Deserialize<Envelope>(dataToParse);
			var cubeCollection = result.Cube;

			Assert.NotNull(cubeCollection);
			Assert.AreEqual(64, cubeCollection.Length);

			var first_date_cube = cubeCollection[0];

			Assert.AreEqual(32, first_date_cube.Cube.Length);
			Assert.AreEqual(new DateTime(2020, 10, 06), first_date_cube.time);

			var first_currency_cube = first_date_cube.Cube[0];

			Assert.AreEqual("USD", first_currency_cube.currency);
			Assert.AreEqual(1.1795, first_currency_cube.rate);
		}
	}
}
