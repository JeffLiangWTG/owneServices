using System;
using System.Collections.Generic;
using System.Text.Json;
using CargoWise.RefDbRepo.INReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	[TestFixture]
	public class LocalFileStorageTest
	{
		[Test]
		public void TestSaveAndLoad()
		{
			Storage.ClearData();
			Assert.Null(Storage.Load());

			var data = "test";
			Storage.Save(data);
			Assert.AreEqual(data, Storage.Load());

			var data2 = new List<DateTime> { DateTime.UtcNow };
			Storage.Save(data2);
			Assert.AreEqual(JsonSerializer.Serialize(data2), Storage.Load());
			Assert.AreEqual(data2, Storage.Load<List<DateTime>>());

			Storage.Save(null);
			Assert.AreEqual(string.Empty, Storage.Load());
			Assert.Null(Storage.Load<object>());
		}

		[Test]
		public void TestClearData()
		{
			var filePath = Storage.FileLocation;

			Assert.False(System.IO.File.Exists(filePath));
			Storage.Save("test");
			Assert.True(System.IO.File.Exists(filePath));
			Storage.ClearData();
			Assert.False(System.IO.File.Exists(filePath));

		}

		[SetUp]
		public void SetUp()
		{
			Storage = new LocalFileStorage("TST");
		}

		[TearDown]
		public void TearDown()
		{
			Storage.ClearData();
		}

		LocalFileStorage Storage;
	}
}
