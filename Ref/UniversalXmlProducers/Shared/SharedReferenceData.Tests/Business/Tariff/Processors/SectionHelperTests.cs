using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors.Tests
{
	[TestFixture]
	class SectionHelperTests
	{
		[Test]
		public void GetSectionNumber()
		{
			var sectionHelper = new SectionHelper();
			Assert.That(sectionHelper.GetSectionNumber(6), Is.EqualTo(2), "Lower boundry");
			Assert.That(sectionHelper.GetSectionNumber(14), Is.EqualTo(2), "Upper boundary");
			Assert.That(sectionHelper.GetSectionNumber(71), Is.EqualTo(14), "Single chapter section");
			Assert.That(sectionHelper.GetSectionNumber(0), Is.EqualTo(0), "Below minimum");
			Assert.That(sectionHelper.GetSectionNumber(911), Is.EqualTo(0), "Above maximum");
		}

		[Test]
		public void SectionsByChapter()
		{
			var sectionHelper = new SectionHelper();
			var sections = sectionHelper.GetSectionsByChapter("0"); //Chapter Filter "0" equals Chapter 01 to 09
			Assert.That(sections, Is.Not.Null);
			Assert.That(sections.Count, Is.EqualTo(2));

			sections = sectionHelper.GetSectionsByChapter("1"); //Chapter Filter "1" equals Chapter 10 to 19
			Assert.That(sections, Is.Not.Null);
			Assert.That(sections.Count, Is.EqualTo(3));

			sections = sectionHelper.GetSectionsByChapter("37");
			Assert.That(sections, Is.Not.Null);
			Assert.That(sections.Count, Is.EqualTo(1));

			sections = sectionHelper.GetSectionsByChapter("00");
			Assert.That(sections, Is.Not.Null);
			Assert.That(sections.Count, Is.EqualTo(0));

			sections = sectionHelper.GetSectionsByChapter("99");
			Assert.That(sections, Is.Not.Null);
			Assert.That(sections.Count, Is.EqualTo(1));

			sections = sectionHelper.GetSectionsByChapter("100");
			Assert.That(sections, Is.Not.Null);
			Assert.That(sections.Count, Is.EqualTo(0));
		}

		[Test]
		public void Sections()
		{
			var sectionHelper = new SectionHelper();
			var sections = sectionHelper.Sections;

			Assert.That(sections.Count, Is.EqualTo(22), "Expecting 22 sections");
			Assert.That(sections.Select(x => x.SectionNumber).Distinct().Count, Is.EqualTo(22), "Section numbers should be unique");
			Assert.That(sections.OrderBy(x => x.SectionNumber).First().SectionNumber, Is.EqualTo(1), "Section numbers should start at 1");
			Assert.That(sections.OrderByDescending(x => x.SectionNumber).First().SectionNumber, Is.EqualTo(22), "Section numbers should end at 22");

			foreach (var sec in sections)
			{
				var overlap = sections.FirstOrDefault(x => x != sec && x.MinimumChapter <= sec.MinimumChapter && x.MaximumChapter >= sec.MinimumChapter);
				Assert.That(overlap, Is.Null, $"Section {sec.SectionNumber} overlaps with section {overlap?.SectionNumber}");
			}
		}
	}
}
