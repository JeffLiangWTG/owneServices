using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	[TestFixture]
	public class NomenclatureHelperTest
	{
		[Test]
		public void Sections()
		{
			var sections = helper.Sections;
			Assert.AreEqual(21, sections.Count());

			var first = sections.First();
			Assert.AreEqual("01", first.Code);
			Assert.AreEqual("CANLI HAYVANLAR VE HAYVANSAL ÜRÜNLER", first.Description);

			var chaptersOfLastSecion = sections.Last().Chapters;
			Assert.AreEqual(3, chaptersOfLastSecion.Length);
			Assert.AreEqual("97,98,99", string.Join(",", chaptersOfLastSecion));
		}

		[Test]
		public void GetSection()
		{
			Assert.AreEqual("01", helper.GetSection("05"));
			Assert.AreEqual("06", helper.GetSection("28"));
			Assert.AreEqual("11", helper.GetSection("53"));
			Assert.AreEqual("15", helper.GetSection("72"));
			Assert.AreEqual("21", helper.GetSection("97"));
		}

		[Test]
		public void ChapterCodes()
		{
			var chapterCodes = helper.ChapterCodes;
			Assert.AreEqual(99, chapterCodes.Count());

			var first = chapterCodes.First();
			Assert.AreEqual("01", first);

			var last = chapterCodes.Last();
			Assert.AreEqual("99", last);
		}

		[Test]
		public void GetChapterTitle()
		{
			Assert.AreEqual("Canlı hayvanlar", helper.GetChapterTitle("01"));
			Assert.AreEqual("Gübreler", helper.GetChapterTitle("31"));
			Assert.AreEqual("Sentetik ve suni filamentler, şeritler ve benzeri sentetik ve suni dokumaya elverişli maddeler.......", helper.GetChapterTitle("54"));
			Assert.AreEqual("Demir ve çelik", helper.GetChapterTitle("72"));
			Assert.AreEqual("Özel amaçlı gümrük tarife istatistik pozisyonları", helper.GetChapterTitle("99"));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			helper = new NomenclatureHelper();
		}

		NomenclatureHelper helper;
	}
}
