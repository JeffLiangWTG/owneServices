using System.Linq;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class CustomsOfficesDataParserTest
	{
		[Test]
		public void TestParseXML()
		{
			var records = new[]
			{
				"",
				"税関官署コード,,,,,,",
				",,,,,,",
				",,,,,,",
				"東京,1,A,○,東京税関（本関）,1000,TOKYO",
				",,4,○,東京税関（本関）埼玉方面,1991,HONKAN(S)",
				",,5,○,東京税関前橋出張所太田派出所,1009,MAEBASHI-O",
				",,I,○,東京税関立川出張所,1005,TACHIKAWA",
				",,M,○,成田航空貨物出張所,1040,NARIKOH BC",
				",,,,,,",
				"横浜,2,A,○,横浜税関（本関）,2000,YOKOHAMA",
				",,J,○,横浜税関川崎外郵出張所,2001,GAIYU KWS",
				",,,,,,"
			};
			var parser = new CustomsOfficesDataParser();
			var result = parser.ParseToRefCusCodeLists(records);
			Assert.That(result.Count(), Is.EqualTo(7));
			Assert.That(parser.ErrorStr, Is.Empty);
		}

		[Test]
		public void TestCanNotFindAnyErrorMessage()
		{
			var records = Enumerable.Empty<string>();
			var parser = new CustomsOfficesDataParser();
			var result = parser.ParseToRefCusCodeLists(records);
			string errorMsg = "Can not find any Customs Office Codes!";
			Assert.That(result.Count(), Is.EqualTo(0));
			Assert.That(parser.ErrorStr, Is.EqualTo(errorMsg));

			records = new[]
			{
				"",
				"   ",
				"税関官署コード,,,,,,",
				",,,",
				"  ,  ,  ,"
			};
			result = parser.ParseToRefCusCodeLists(records);
			Assert.That(result.Count(), Is.EqualTo(0));
			Assert.That(parser.ErrorStr, Is.EqualTo(errorMsg));
		}
	}
}
