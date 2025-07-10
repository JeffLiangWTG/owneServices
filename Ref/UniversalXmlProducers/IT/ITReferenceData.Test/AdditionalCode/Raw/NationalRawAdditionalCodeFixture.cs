using System;
using System.IO;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.AdditionalCode
{
	[TestFixture]
	class NationalRawAdditionalCodeFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new NationalRawAdditionalCode(rawHtml: null), "When html is null");
			Assert.Throws<ArgumentException>(() => new NationalRawAdditionalCode(rawHtml: ""), "When html is empty");
		}

		[Test]
		public void Parse()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AdditionalCode\TestFiles\NationalAdditionalCode_Q001.html");
			var rawAdditionalCode = (IRawAdditionalCode)new NationalRawAdditionalCode(File.ReadAllText(htmlTestFile));
			Assert.Multiple(() =>
			{
				Assert.AreEqual("Q001", rawAdditionalCode.Code, nameof(rawAdditionalCode.Code));
				Assert.AreEqual("Destinati all'alimentazione umana(Tabella A,punto 7,parte 111, DPR 633/72).", rawAdditionalCode.Description, nameof(rawAdditionalCode.Description));
				Assert.AreEqual(new DateTime(2003, 07, 01), rawAdditionalCode.StartDate, nameof(rawAdditionalCode.StartDate));
				Assert.AreEqual(new DateTime(9999, 12, 31), rawAdditionalCode.EndDate, nameof(rawAdditionalCode.EndDate));
			});
		}

		[Test]
		public void ParseDescriptionMultipleLinesAreConvertedToSpace()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AdditionalCode\TestFiles\NationalAdditionalCode_R024.html");
			var rawAdditionalCode = (IRawAdditionalCode)new NationalRawAdditionalCode(File.ReadAllText(htmlTestFile));
			Assert.AreEqual("Eucaliptolo o cineolo  Anetolo Eugenolo e isoeugenolo Safrolo e isosafrolo", rawAdditionalCode.Description, nameof(rawAdditionalCode.Description));
		}

		[Test]
		public void ParseWithInvalidHtml()
		{
			var rawAdditionalCode = (IRawAdditionalCode)new NationalRawAdditionalCode("<html></html>");
			Assert.Multiple(() =>
			{
				Assert.AreEqual("", rawAdditionalCode.Code, nameof(rawAdditionalCode.Code));
				Assert.AreEqual("", rawAdditionalCode.Description, nameof(rawAdditionalCode.Description));
				Assert.IsNull(rawAdditionalCode.StartDate, nameof(rawAdditionalCode.StartDate));
				Assert.IsNull(rawAdditionalCode.EndDate, nameof(rawAdditionalCode.EndDate));
			});
		}
	}
}
