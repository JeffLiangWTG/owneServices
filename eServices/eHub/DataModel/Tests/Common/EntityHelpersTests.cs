using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.DataModel.Common;

namespace CargoWise.eHub.DataModel.Tests.Common
{
	[TestClass]
	public class EntityHelpersTests
	{
		[TestMethod]
		public void EntityHelpers_ReorderEntities()
		{
			var testData1 = new List<eHubRoutingRule>
			{
				new eHubRoutingRule { RR_Condition_Expression = "AAA" },
				new eHubRoutingRule { RR_Condition_Expression = "BBB" },
				new eHubRoutingRule { RR_Condition_Expression = "CCC" }
			};
			EntityHelpers.ReorderEntities(testData1, "RR_Group_Ordering");
			var result1 = testData1.Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{ 
				new Tuple<string, int?>("AAA", 1000),
				new Tuple<string, int?>("BBB", 2000), 
				new Tuple<string, int?>("CCC", 3000)
			}, result1);

			var testData2 = new List<eHubRoutingRule>
			{
				new eHubRoutingRule { RR_Condition_Expression = "AAA", RR_Group_Ordering = 1000 },
				new eHubRoutingRule { RR_Condition_Expression = "DDD" },
				new eHubRoutingRule { RR_Condition_Expression = "EEE" },
				new eHubRoutingRule { RR_Condition_Expression = "FFF" },
				new eHubRoutingRule { RR_Condition_Expression = "BBB", RR_Group_Ordering = 2000 },
				new eHubRoutingRule { RR_Condition_Expression = "CCC", RR_Group_Ordering = 3000 },
				new eHubRoutingRule { RR_Condition_Expression = "GGG" }
			};
			EntityHelpers.ReorderEntities(testData2, "RR_Group_Ordering");
			var result2 = testData2.Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{ 
				new Tuple<string, int?>("AAA", 1000),
				new Tuple<string, int?>("DDD", 1250),
				new Tuple<string, int?>("EEE", 1500),
				new Tuple<string, int?>("FFF", 1750),
				new Tuple<string, int?>("BBB", 2000), 
				new Tuple<string, int?>("CCC", 3000),
				new Tuple<string, int?>("GGG", 4000)
			}, result2);

			var testData3 = new List<eHubRoutingRule>
			{
				new eHubRoutingRule { RR_Condition_Expression = "GGG" },
				new eHubRoutingRule { RR_Condition_Expression = "AAA", RR_Group_Ordering = 1000 },
				new eHubRoutingRule { RR_Condition_Expression = "DDD", RR_Group_Ordering = 1250 },
				new eHubRoutingRule { RR_Condition_Expression = "HHH" },
				new eHubRoutingRule { RR_Condition_Expression = "III" },
				new eHubRoutingRule { RR_Condition_Expression = "BBB", RR_Group_Ordering = 2000 },
				new eHubRoutingRule { RR_Condition_Expression = "CCC", RR_Group_Ordering = 3000 }
			};
			EntityHelpers.ReorderEntities(testData3, "RR_Group_Ordering");
			var result3 = testData3.Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{ 
				new Tuple<string, int?>("GGG", 500), 
				new Tuple<string, int?>("AAA", 1000),
				new Tuple<string, int?>("DDD", 1250),
				new Tuple<string, int?>("HHH", 1500),
				new Tuple<string, int?>("III", 1750),
				new Tuple<string, int?>("BBB", 2000),
				new Tuple<string, int?>("CCC", 3000)
			}, result3);

			var testData4 = new List<eHubRoutingRule>
			{
				new eHubRoutingRule { RR_Condition_Expression = "HHH" },
				new eHubRoutingRule { RR_Condition_Expression = "DDD", RR_Group_Ordering = 2 },
				new eHubRoutingRule { RR_Condition_Expression = "XXX" },
				new eHubRoutingRule { RR_Condition_Expression = "YYY" },
				new eHubRoutingRule { RR_Condition_Expression = "EEE", RR_Group_Ordering = 5 },
				new eHubRoutingRule { RR_Condition_Expression = "FFF", RR_Group_Ordering = 6 },
				new eHubRoutingRule { RR_Condition_Expression = "GGG", RR_Group_Ordering = 7 },
				new eHubRoutingRule { RR_Condition_Expression = "BBB" },
				new eHubRoutingRule { RR_Condition_Expression = "CCC" },
				new eHubRoutingRule { RR_Condition_Expression = "AAA" },
			};
			EntityHelpers.ReorderEntities(testData4, "RR_Group_Ordering", 10);
			var result4 = testData4.Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{ 
				new Tuple<string, int?>("HHH", 1),
				new Tuple<string, int?>("DDD", 2),
				new Tuple<string, int?>("XXX", 3),
				new Tuple<string, int?>("YYY", 4),
				new Tuple<string, int?>("EEE", 5), 
				new Tuple<string, int?>("FFF", 6), 
				new Tuple<string, int?>("GGG", 7), 
				new Tuple<string, int?>("BBB", 10),
				new Tuple<string, int?>("CCC", 20),
				new Tuple<string, int?>("AAA", 30)
			}, result4);

			var testData5 = new List<eHubRoutingRule>
			{
				new eHubRoutingRule { RR_Condition_Expression = "AAA", RR_Group_Ordering = 30 },
				new eHubRoutingRule { RR_Condition_Expression = "DDD", RR_Group_Ordering = 1 },
				new eHubRoutingRule { RR_Condition_Expression = "XXX", RR_Group_Ordering = 2 },
				new eHubRoutingRule { RR_Condition_Expression = "YYY", RR_Group_Ordering = 3 },
				new eHubRoutingRule { RR_Condition_Expression = "BBB", RR_Group_Ordering = 10 },
				new eHubRoutingRule { RR_Condition_Expression = "CCC", RR_Group_Ordering = 20 },
			};
			EntityHelpers.ReorderEntities(testData5, "RR_Group_Ordering");
			var result5 = testData5.Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{
				new Tuple<string, int?>("AAA", 2),
				new Tuple<string, int?>("DDD", 4),
				new Tuple<string, int?>("XXX", 6),
				new Tuple<string, int?>("YYY", 8),
				new Tuple<string, int?>("BBB", 10),
				new Tuple<string, int?>("CCC", 20),
			}, result5);

			var testData6 = new List<eHubRoutingRule>
			{
				new eHubRoutingRule { RR_Condition_Expression = "AAA", RR_Group_Ordering = 39 },
				new eHubRoutingRule { RR_Condition_Expression = "BBB", RR_Group_Ordering = 78 },
				new eHubRoutingRule { RR_Condition_Expression = "DDD", RR_Group_Ordering = 5000 },
				new eHubRoutingRule { RR_Condition_Expression = "CCC", RR_Group_Ordering = 3541 },
				new eHubRoutingRule { RR_Condition_Expression = "EEE", RR_Group_Ordering = 8500 },
				new eHubRoutingRule { RR_Condition_Expression = "FFF", RR_Group_Ordering = 12000 },
				new eHubRoutingRule { RR_Condition_Expression = "GGG", RR_Group_Ordering = 15500 },
				new eHubRoutingRule { RR_Condition_Expression = "HHH", RR_Group_Ordering = 19000 },
				new eHubRoutingRule { RR_Condition_Expression = "III", RR_Group_Ordering = 22500 },
			};
			EntityHelpers.ReorderEntities(testData6, "RR_Group_Ordering");
			var result6 = testData6.Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{
				new Tuple<string, int?>("AAA", 39),
				new Tuple<string, int?>("BBB", 78),
				new Tuple<string, int?>("DDD", 1809),
				new Tuple<string, int?>("CCC", 3541),
				new Tuple<string, int?>("EEE", 8500),
				new Tuple<string, int?>("FFF", 12000),
				new Tuple<string, int?>("GGG", 15500),
				new Tuple<string, int?>("HHH", 19000),
				new Tuple<string, int?>("III", 22500),
			}, result6);
		}
		
		[TestMethod]
		public void EntityHelpers_ReorderEntities_Exceptions()
		{
			List<Tuple<string, int>> testEntities = null;

			try { EntityHelpers.ReorderEntities(testEntities, null, 0); }
			catch (Exception ex) { Assert.IsInstanceOfType(ex, typeof(ArgumentNullException)); }

			testEntities = new List<Tuple<string, int>>();
			try { EntityHelpers.ReorderEntities(testEntities, null, 0); }
			catch (Exception ex) { Assert.IsInstanceOfType(ex, typeof(ArgumentNullException)); }

			try { EntityHelpers.ReorderEntities(testEntities, "", 0); }
			catch (Exception ex) { Assert.IsInstanceOfType(ex, typeof(ArgumentOutOfRangeException)); }

			try { EntityHelpers.ReorderEntities(testEntities, "", 1); }
			catch (Exception ex) { Assert.IsInstanceOfType(ex, typeof(ArgumentException)); }

			try { EntityHelpers.ReorderEntities(testEntities, "Item2", 1); }
			catch (Exception ex) { Assert.IsInstanceOfType(ex, typeof(ArgumentException)); }

			EntityHelpers.ReorderEntities(new List<Tuple<string,int?>>(), "Item2");
		}
	}
}
