using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class DictionaryExtensionsFixture
	{
		[Test]
		public void AddIfNotExists()
		{
			var dict = new Dictionary<string, int>();
			dict.AddIfNotExists("001", 4);
			dict.AddIfNotExists("001", 5);
			Assert.AreEqual(4, dict["001"]);
		}

		[Test]
		public void MergeIfNotExists()
		{
			var dict1 = new Dictionary<string, int>();
			var dict2 = new Dictionary<string, int>();
			dict1.Add("001", 1);
			dict1.Add("002", 1);
			dict2.Add("002", 3);
			dict2.Add("003", 3);
			dict1.MergeIfNotExists(dict2);
			Assert.AreEqual(3, dict1.Keys.Count);
			Assert.AreEqual(1, dict1["001"]);
			Assert.AreEqual(1, dict1["002"]);
			Assert.AreEqual(3, dict1["003"]);
		}
	}
}
