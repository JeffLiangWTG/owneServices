using System;
using CargoWise.RefDbRepo.Common.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class DataBlockKeyFixture
	{
		[Test]
		public void GetKey()
		{
			var key = new DataBlockKey<int>(new DateTime(2016, 10, 31), new DateTime(2016, 10, 31), "CheckpointString", "dummy");
			Assert.That(key.GetKey(), Is.EqualTo(@"{""LowerTimestamp"":""2016-10-31T00:00:00"",""UpperTimestamp"":""2016-10-31T00:00:00"",""Checkpoint"":""CheckpointString"",""DataSet"":""dummy"",""Version"":2}"));
		}

		[Test]
		public void CaculateNextKey()
		{
			var dataSet = new RefDataSet { Checkpoint = "CheckpointString" };
			var key = new DataBlockKey<RefDataSet>(new DateTime(2016, 10, 31), new DateTime(2016, 11, 1), "BB", null);
			var nextKey = (DataBlockKey<RefDataSet>)key.CaculateNextKey(dataSet.Checkpoint);
			Assert.That(nextKey.Checkpoint, Is.EqualTo("CheckpointString"));
			Assert.That(nextKey.LowerTimestamp, Is.EqualTo(new DateTime(2016, 10, 31)));
			Assert.That(nextKey.UpperTimestamp, Is.EqualTo(new DateTime(2016, 11, 1)));
		}

		[Test]
		public void CaculateNextKeyNotSuccessful()
		{
			var dataSet = new RefDataSet();
			var key = new DataBlockKey<RefDataSet>(new DateTime(2016, 10, 31), new DateTime(2016, 11, 1), "AAA", null);
			Assert.Null((DataBlockKey<RefDataSet>)key.CaculateNextKey(dataSet.Checkpoint));
		}
	}
}
