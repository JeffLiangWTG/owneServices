using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	public class DataProviderHelperFixture
	{
		[Test]
		public void OrderIncludeAscending()
		{
			var includes = new[] { "Level1.Level2.Level3", "Level1a", "Level2b.Level3b", "Level5" }.AsEnumerable();
			var previousIncludeLenght = 0;
			foreach (var include in DataProviderHelper.OrderInclude(includes))
			{
				var currentIncludeLenght = include.Split('.').Length;
				Assert.IsTrue(currentIncludeLenght >= previousIncludeLenght);
				previousIncludeLenght = currentIncludeLenght;
			}
			previousIncludeLenght = int.MaxValue;
			foreach (var include in DataProviderHelper.OrderInclude(includes, "DESC"))
			{
				var currentIncludeLenght = include.Split('.').Length;
				Assert.IsTrue(currentIncludeLenght <= previousIncludeLenght);
				previousIncludeLenght = currentIncludeLenght;
			}
		}

		[Test]
		public void IsExpirableType()
		{
			Assert.True(DataProviderHelper.IsExpirableType(typeof(RefCusTariff)));
			Assert.False(DataProviderHelper.IsExpirableType(typeof(RefCusTariffAttribute)));
			Assert.True(DataProviderHelper.IsExpirableType(typeof(RefCusCodeList)));
			Assert.True(DataProviderHelper.IsExpirableType(typeof(RefCusTariffNationalCode)));
			Assert.Throws<ArgumentNullException>(() => DataProviderHelper.IsExpirableType(typeof(RefCusTariffUOM)));
			Assert.Throws<ArgumentNullException>(() => DataProviderHelper.IsExpirableType(typeof(RefCusCodeListAttribute)));

			DataProviderHelper.SetEnableExpirable(false);
			Assert.False(DataProviderHelper.IsExpirableType(typeof(RefCusTariffUOM)));
			Assert.False(DataProviderHelper.IsExpirableType(typeof(RefCusCodeListAttribute)));
			Assert.False(DataProviderHelper.IsExpirableType(typeof(RefCusTariffUom)));
			Assert.False(DataProviderHelper.IsExpirableType(typeof(RefCusCodeListAttribute)));
			DataProviderHelper.SetEnableExpirable(true);
			Assert.True(DataProviderHelper.IsExpirableType(typeof(RefCusCodeListAttribute)));
			Assert.True(DataProviderHelper.IsExpirableType(typeof(RefCusTariffUom)));
			Assert.True(DataProviderHelper.IsExpirableType(typeof(RefCusCodeListAttribute)));
		}

		[Test]
		public void AddUpdaterResult()
		{
			var results = new Dictionary<Guid, SafeObjectUpdaterResult>();
			var ratePK1 = Guid.NewGuid();
			var ratePK2 = Guid.NewGuid();
			var tariffPK = Guid.NewGuid();
			var newPK = Guid.NewGuid();
			var rate1 = new RefCusRate { ZZ2_PK = ratePK1, ZZ2_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			var rate2 = new RefCusRate { ZZ2_PK = ratePK2, ZZ2_EndDate = new DateTime(2079, 06, 06).ToUTCDateTimeOffset() };
			DataProviderHelper.AddUpdaterResult(results, rate1, "ZZ2", ResultAction.Expire, null, null, ratePK1);
			DataProviderHelper.AddUpdaterResult(results, rate2, "ZZ2", ResultAction.Expire, tariffPK, newPK, ratePK2);
			Assert.AreEqual(results.Count, 2);
			Assert.AreEqual(results[ratePK1].ParentPK, ratePK1);
			Assert.AreEqual(results[ratePK1].ParentCode, "ZZ2");
			Assert.AreEqual(results[ratePK1].Action, ResultAction.Expire);
			Assert.IsNull(results[ratePK1].ExpirableAncestorPK);
			Assert.IsNull(results[ratePK1].NewRecordForCloneActionPK);

			Assert.AreEqual(tariffPK, results[ratePK2].ExpirableAncestorPK);
			Assert.AreEqual(newPK, results[ratePK2].NewRecordForCloneActionPK);
		}
	}

	#region Expirable classes to remove later

	class RefCusTariffUom
	{
		public Guid ZZ8_PK { get; set; }
		public DateTime? ZZ8_StartDate { get; set; }
		public DateTime? ZZ8_EndDate { get; set; }
	}

	#endregion
}
