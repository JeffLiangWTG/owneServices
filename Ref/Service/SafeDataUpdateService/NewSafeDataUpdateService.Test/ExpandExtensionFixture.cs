using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	public class ExpandExtensionFixture
	{
		[Test]
		public void GetChunk()
		{
			var repo = new Mock<IReferenceDataRepository>();
			var tariffPK1 = new Guid("00000000-0000-0000-0000-000000000001");
			var tariffPK2 = new Guid("00000000-0000-0000-0000-000000000002");
			var tariffPK3 = new Guid("00000000-0000-0000-0000-000000000003");
			var rate1 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK1, ZZ2_DataSetPK = tariffPK1 };
			var rate2 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK1, ZZ2_DataSetPK = tariffPK1 };
			var rate3 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK2, ZZ2_DataSetPK = tariffPK2 };
			var rate4 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK3, ZZ2_DataSetPK = tariffPK3 };
			repo.Setup(x => x.Get<RefCusRate>()).Returns(new[] { rate1, rate2, rate3, rate4 }.AsQueryable());

			var dataSetPKs = new List<Guid> { tariffPK1, tariffPK2 }.OrderBy(x => x).ToList();
			var ratesChunk = repo.Object.Get<RefCusRate>().GetChunk(dataSetPKs, new List<Type> { typeof(RefCusRate) });

			Assert.AreEqual(3, ratesChunk.Length);
			Assert.AreEqual(tariffPK1, ratesChunk[0].ZZ2_DataSetPK);
			Assert.AreEqual(tariffPK1, ratesChunk[1].ZZ2_DataSetPK);
			Assert.AreEqual(tariffPK2, ratesChunk[2].ZZ2_DataSetPK);
		}

		[Test]
		public void GetSetData()
		{
			var tariffPK1 = new Guid("00000000-0000-0000-0000-000000000001");
			var tariffPK2 = new Guid("00000000-0000-0000-0000-000000000002");
			var tariffPK3 = new Guid("00000000-0000-0000-0000-000000000003");
			var rate1 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK1, ZZ2_DataSetPK = tariffPK1 };
			var rate2 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK2, ZZ2_DataSetPK = tariffPK2 };
			var rate3 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK2, ZZ2_DataSetPK = tariffPK2 };
			var rate4 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK3, ZZ2_DataSetPK = tariffPK3 };
			var rate5 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK3, ZZ2_DataSetPK = tariffPK3 };
			var rate6 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK3, ZZ2_DataSetPK = tariffPK3 };
			var ratesChunk = new RefCusRate[] { rate1, rate2, rate3, rate4, rate5, rate6 };
			var rateIdx = 0;

			var rates1 = ratesChunk.GetSetData(x => x.ZZ2_DataSetPK, tariffPK1, ref rateIdx);
			Assert.AreEqual(1, rates1.Count());
			Assert.AreEqual(1, rateIdx);

			var rates2 = ratesChunk.GetSetData(x => x.ZZ2_DataSetPK, tariffPK2, ref rateIdx);
			Assert.AreEqual(2, rates2.Count());
			Assert.AreEqual(3, rateIdx);

			var rates3 = ratesChunk.GetSetData(x => x.ZZ2_DataSetPK, tariffPK3, ref rateIdx);
			Assert.AreEqual(3, rates3.Count());
			Assert.AreEqual(6, rateIdx);
		}

		[Test]
		public void FilterToArray()
		{
			var tariffPK1 = new Guid("00000000-0000-0000-0000-000000000001");
			var tariffPK2 = new Guid("00000000-0000-0000-0000-000000000002");
			var tariffPK3 = new Guid("00000000-0000-0000-0000-000000000003");
			var rate1 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK1, ZZ2_DataSetPK = tariffPK1 };
			var rate2 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK2, ZZ2_DataSetPK = tariffPK2 };
			var rate3 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK2, ZZ2_DataSetPK = tariffPK2 };
			var rate4 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK3, ZZ2_DataSetPK = tariffPK3 };
			var rate5 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK3, ZZ2_DataSetPK = tariffPK3 };
			var rate6 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariffPK3, ZZ2_DataSetPK = tariffPK3 };
			var ratesChunk = new RefCusRate[] { rate1, rate2, rate3, rate4, rate5, rate6 };
			var rates = ratesChunk.GroupBy(x => x.ZZ2_DataSetPK);

			var rates1 = rates.FilterToArray(tariffPK1);
			Assert.AreEqual(1, rates1.Length);
			var rates2 = rates.FilterToArray(tariffPK2);
			Assert.AreEqual(2, rates2.Length);
			var rates3 = rates.FilterToArray(tariffPK3);
			Assert.AreEqual(3, rates3.Length);
		}
	}
}
