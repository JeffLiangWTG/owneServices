using System.Linq;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class TariffAttributeFixture
	{
		[Test]
		public void TestDefaultPropertyValues()
		{
			var tariffAttribute = new TariffAttribute();
			Assert.Null(tariffAttribute.TariffCode);
			Assert.Null(tariffAttribute.AttributeName);
			Assert.Null(tariffAttribute.AttributeValue);
		}

		[Test]
		public void TestGetAll()
		{
			var result = TariffAttributeRepository.GetAll();
			Assert.AreEqual(1059, result.Count);
		}

		[Test]
		public void TestAllAttributes()
		{
			var all = TariffAttributeRepository.GetAll();
			var repository = new TariffAttributeRepository();
			var attributes = repository.AllAttributes;
			Assert.AreEqual(all.Count, attributes.Count);
		}

		[Test]
		public void TestGetByTariffCode()
		{
			var repository = new TariffAttributeRepository();
			var result = repository.GetByTariffCode("3005101000");
			Assert.AreEqual(1, result.Count());
			var item = result.FirstOrDefault();
			Assert.AreEqual("COMMODITYTYPE", item.AttributeName);
			Assert.AreEqual("MED", item.AttributeValue);

			result = repository.GetByTariffCode("8414301400999");
			Assert.AreEqual(1, result.Count());
			item = result.FirstOrDefault();
			Assert.AreEqual("COMMODITYTYPE", item.AttributeName);
			Assert.AreEqual("CFCS", item.AttributeValue);

			result = repository.GetByTariffCode("0101210010");
			Assert.AreEqual(0, result.Count());
		}
	}
}
