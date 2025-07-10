using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class EntityMatcherFixture
	{
		[Test]
		public void GetBestMatchingCodes()
		{
			var repo = new Mock<IStagingRepository>();
			var entity1 = new NamedEntityClassification
			{
				NEC_Class = EntityClass.COUNTRY.ToString(),
				NEC_Code = "CN",
				NEC_Language = "EN",
				NEC_Name = "China"
			};
			var entity2 = new NamedEntityClassification
			{
				NEC_Class = EntityClass.CURRENCY.ToString(),
				NEC_Code = "CNY",
				NEC_Language = "EN",
				NEC_Name = "Yuan Renminbi"
			};
			var entity3 = new NamedEntityClassification
			{
				NEC_Class = EntityClass.COUNTRY.ToString(),
				NEC_Code = "US",
				NEC_Language = "EN",
				NEC_Name = "United States"
			};
			var entity4 = new NamedEntityClassification
			{
				NEC_Class = EntityClass.COUNTRY.ToString(),
				NEC_Code = "KN",
				NEC_Language = "EN",
				NEC_Name = "Saint Kitts and Nevis"
			};
			var entity5 = new NamedEntityClassification
			{
				NEC_Class = EntityClass.COUNTRY.ToString(),
				NEC_Code = "XX",
				NEC_Language = "EN",
				NEC_Name = "Nevis"
			};
			var entity6 = new NamedEntityClassification
			{
				NEC_Class = EntityClass.CACUSTOMUOM.ToString(),
				NEC_Code = "KGM",
				NEC_Language = "EN",
				NEC_Name = "kilogram"
			};
			repo.Setup(x => x.Get<NamedEntityClassification>())
				.Returns(new[] { entity1, entity2, entity3, entity4, entity5, entity6 }.AsQueryable());
			var recoginition = new Mock<IEntityRecognition>();
			recoginition.Setup(x => x.GetNamedEntities(It.IsAny<string>())).Returns<string>(x => new[] { x });
			var matcher = new EntityMatcher(repo.Object, recoginition.Object);
			Assert.AreEqual("US", matcher.GetBestMatchingCodes(EntityClass.COUNTRY, "EN", true, true, "United-States").FirstOrDefault().Result);
			Assert.AreEqual("CN", matcher.GetBestMatchingCodes(EntityClass.COUNTRY, "EN", true, true, "China").FirstOrDefault().Result);
			Assert.AreEqual("KN", matcher.GetBestMatchingCodes(EntityClass.COUNTRY, "EN", true, true, "Saint Kitts and Nevis").FirstOrDefault().Result);
			Assert.AreEqual("CNY", matcher.GetBestMatchingCodes(EntityClass.CURRENCY, "EN", true, true, "Yuan Renminbi").FirstOrDefault().Result);
			Assert.AreEqual("KGM", matcher.GetBestMatchingCodes(EntityClass.CACUSTOMUOM, "EN", true, true, "kilogram").FirstOrDefault().Result);
		}

		[Test]
		public void GetBestMatchingCodes_ExactMatchExistsInDB()
		{
			var repo = new Mock<IStagingRepository>();
			var entity1 = new NamedEntityClassification
			{
				NEC_Class = EntityClass.COUNTRY.ToString(),
				NEC_Code = "SD",
				NEC_Language = "EN",
				NEC_Name = "Sudan"
			};
			var entity2 = new NamedEntityClassification
			{
				NEC_Class = EntityClass.COUNTRY.ToString(),
				NEC_Code = "SS",
				NEC_Language = "EN",
				NEC_Name = "South Sudan"
			};
			repo.Setup(x => x.Get<NamedEntityClassification>())
				.Returns(new[] { entity1, entity2 }.AsQueryable());
			var recoginition = new Mock<IEntityRecognition>();
			recoginition.Setup(x => x.GetNamedEntities(It.IsAny<string>())).Returns(new[] { "Sudan" });
			var matcher = new EntityMatcher(repo.Object, recoginition.Object);
			Assert.AreEqual("SS", matcher.GetBestMatchingCodes(EntityClass.COUNTRY, "EN", true, true, "South Sudan").FirstOrDefault().Result);
		}
	}
}
