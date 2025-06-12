using System;
using System.Collections.Generic;
using System.Linq;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace eServices.eHubRoutingRuleEngine.Tests
{
	[TestClass]
	public class SubRuleCollectionTests
	{
		[TestMethod]
		public void SubRuleCollection_Load()
		{
			Guid parentPK1 = new Guid("11111111-1111-1111-1111-111111111111");
			var parentRule1 = new eHubRoutingRule { RR_PK = parentPK1, RR_Group_MatchMultiple = false };
			var subRuleCollection1 = new SubRuleCollection(parentRule1);

			var loadTest1 = new List<Criterion>
			{
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "AAA" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "BBB" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "CCC" })
			};
			subRuleCollection1.Load(loadTest1);
			var result1 = parentRule1.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{ 
				new Tuple<string, int?>("AAA", 1000),
				new Tuple<string, int?>("BBB", 2000), 
				new Tuple<string, int?>("CCC", 3000)
			}, result1);

			var loadTest2 = new List<Criterion>
			{
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "AAA" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "DDD" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "EEE" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "FFF" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "BBB" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "CCC" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "GGG" })
			};
			subRuleCollection1.Load(loadTest2);
			var result2 = parentRule1.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
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

			var loadTest3 = new List<Criterion>
			{
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "GGG" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "AAA" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "DDD" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "HHH" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "III" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "BBB" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "CCC" })
			};
			subRuleCollection1.Load(loadTest3);
			var result3 = parentRule1.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
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

			Guid parentPK2 = new Guid("22222222-2222-2222-2222-222222222222");
			var parentRule2 = new eHubRoutingRule { RR_PK = parentPK2, RR_Group_MatchMultiple = false };
			parentRule2.eHubRoutingRules_Group = new[]
			{
				new eHubRoutingRule { RR_Condition_Expression = "AAA", RR_Group_Ordering = 1, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "BBB", RR_Group_Ordering = 2, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "CCC", RR_Group_Ordering = 3, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "DDD", RR_Group_Ordering = 4, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "EEE", RR_Group_Ordering = 5, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "FFF", RR_Group_Ordering = 6, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "GGG", RR_Group_Ordering = 7, eHubRoutingRule_Group = parentRule2 },
			}.ToList();
			var subRuleCollection2 = new SubRuleCollection(parentRule2);

			var loadTest4 = new List<Criterion>
			{
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "DDD" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "XXX" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "YYY" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "EEE" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "FFF" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "GGG" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "BBB" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "CCC" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "AAA" }),
			};
			subRuleCollection2.Load(loadTest4);
			var result4 = parentRule2.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{ 
				new Tuple<string, int?>("DDD", 1),
				new Tuple<string, int?>("XXX", 2),
				new Tuple<string, int?>("YYY", 3),
				new Tuple<string, int?>("EEE", 5), 
				new Tuple<string, int?>("FFF", 6), 
				new Tuple<string, int?>("GGG", 7), 
				new Tuple<string, int?>("BBB", 1000),
				new Tuple<string, int?>("CCC", 2000),
				new Tuple<string, int?>("AAA", 3000)
			}, result4);

			var loadTest5 = new List<Criterion>
			{
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "AAA" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "DDD" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "XXX" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "YYY" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "BBB" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "CCC" }),
			};
			subRuleCollection2.Load(loadTest5);
			var result5 = parentRule2.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{
				new Tuple<string, int?>("AAA", 200),
				new Tuple<string, int?>("DDD", 400),
				new Tuple<string, int?>("XXX", 600),
				new Tuple<string, int?>("YYY", 800),
				new Tuple<string, int?>("BBB", 1000),
				new Tuple<string, int?>("CCC", 2000),
			}, result5);

			var loadTest6 = new List<Criterion>
			{
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "AAA" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "MMM" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "DDD" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "NNN" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "XXX" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "YYY" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "BBB" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "CCC" }),
			};
			subRuleCollection2.Load(loadTest6);
			var result6 = parentRule2.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{
			    new Tuple<string, int?>("AAA", 200),
			    new Tuple<string, int?>("MMM", 300),
			    new Tuple<string, int?>("DDD", 400),
			    new Tuple<string, int?>("NNN", 500),
			    new Tuple<string, int?>("XXX", 600),
			    new Tuple<string, int?>("YYY", 800),
			    new Tuple<string, int?>("BBB", 1000),
			    new Tuple<string, int?>("CCC", 2000),
			}, result6);

			Guid parentPK3 = new Guid("33333333-3333-3333-3333-333333333333");
			var parentRule3 = new eHubRoutingRule { RR_PK = parentPK3, RR_Group_MatchMultiple = false };
			parentRule3.eHubRoutingRules_Group = new[]
			{
				new eHubRoutingRule { RR_Condition_Expression = "AAA", RR_Group_Ordering = 100, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "BBB", RR_Group_Ordering = 200, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "CCC", RR_Group_Ordering = 300, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "DDD", RR_Group_Ordering = 400, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "EEE", RR_Group_Ordering = 500, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "FFF", RR_Group_Ordering = 600, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "GGG", RR_Group_Ordering = 700, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "HHH", RR_Group_Ordering = 800, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "III", RR_Group_Ordering = 900, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "JJJ", RR_Group_Ordering = 1000, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "KKK", RR_Group_Ordering = 2000, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "LLL", RR_Group_Ordering = 3000, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "MMM", RR_Group_Ordering = 4000, eHubRoutingRule_Group = parentRule2 },
				new eHubRoutingRule { RR_Condition_Expression = "NNN", RR_Group_Ordering = 5000, eHubRoutingRule_Group = parentRule2 },
			}.ToList();
			var subRuleCollection3 = new SubRuleCollection(parentRule3);

			var loadTest7 = new List<Criterion>
			{
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "AAA" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "BBB" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "XXX" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "CCC" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "DDD" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "YYY" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "EEE" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "ZZZ" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "FFF" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "GGG" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "HHH" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "III" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "JJJ" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "KKK" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "LLL" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "MMM" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "NNN" }),
			};
			subRuleCollection3.Load(loadTest7);
			var result7 = parentRule3.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{
				new Tuple<string, int?>("AAA", 100),
				new Tuple<string, int?>("BBB", 200),
				new Tuple<string, int?>("XXX", 250),
				new Tuple<string, int?>("CCC", 300),
				new Tuple<string, int?>("DDD", 400),
				new Tuple<string, int?>("YYY", 450),
				new Tuple<string, int?>("EEE", 500),
				new Tuple<string, int?>("ZZZ", 550),
				new Tuple<string, int?>("FFF", 600),
				new Tuple<string, int?>("GGG", 700),
				new Tuple<string, int?>("HHH", 800),
				new Tuple<string, int?>("III", 900),
				new Tuple<string, int?>("JJJ", 1000),
				new Tuple<string, int?>("KKK", 2000),
				new Tuple<string, int?>("LLL", 3000),
				new Tuple<string, int?>("MMM", 4000),
				new Tuple<string, int?>("NNN", 5000),
			}, result7);

			try
			{
				var loadTest8 = new List<Criterion>
				{
					new Condition(new eHubRoutingRule {RR_Condition_Expression = "DDD"}),
					new Condition(new eHubRoutingRule {RR_Condition_Expression = "AAA"}),
					new Condition(new eHubRoutingRule {RR_Condition_Expression = "BBB"}),
					new Condition(new eHubRoutingRule {RR_Condition_Expression = "CCC"}),
					new Condition(new eHubRoutingRule {RR_Condition_Expression = "DDD"}),
				};
				subRuleCollection3.Load(loadTest8);
				Assert.Fail("Exception not thrown for duplicate rule.");
			}
			catch (RoutingRuleException ex)
			{
				Assert.AreEqual("Duplicate rules detected.", ex.Message);
			}
		}

		[TestMethod]
		public void SubRuleCollection_AddInsertRemove()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var testRR = new TestDbSet<eHubRoutingRule>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRR);

			Guid parentPK1 = new Guid("11111111-1111-1111-1111-111111111111");
			var parentRule1 = new eHubRoutingRule { RR_PK = parentPK1, RR_Group_MatchMultiple = false };
			var subRuleCollection1 = new SubRuleCollection(parentRule1);

			subRuleCollection1.Add(new Condition(new eHubRoutingRule { RR_Condition_Expression = "AAA" }));
			var result1 = parentRule1.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{ 
				new Tuple<string, int?>("AAA", 1000),
			}, result1);

			subRuleCollection1.AddRange(new[]
			{
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "BBB" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "CCC" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "DDD" }),
			});
			var result2 = parentRule1.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{
				new Tuple<string, int?>("AAA", 1000),
				new Tuple<string, int?>("BBB", 2000),
				new Tuple<string, int?>("CCC", 3000),
				new Tuple<string, int?>("DDD", 4000),
			}, result2);

			subRuleCollection1.Insert(0, new Condition(new eHubRoutingRule { RR_Condition_Expression = "EEE" }));
			var result3 = parentRule1.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{ 
				new Tuple<string, int?>("EEE", 500),
				new Tuple<string, int?>("AAA", 1000),
				new Tuple<string, int?>("BBB", 2000),
				new Tuple<string, int?>("CCC", 3000),
				new Tuple<string, int?>("DDD", 4000),
			}, result3);

			subRuleCollection1.InsertRange(1, new[]
			{
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "FFF" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "GGG" }),
				new Condition(new eHubRoutingRule { RR_Condition_Expression = "HHH" }),
			});
			var result4 = parentRule1.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{
				new Tuple<string, int?>("EEE", 500),
				new Tuple<string, int?>("FFF", 625),
				new Tuple<string, int?>("GGG", 750),
				new Tuple<string, int?>("HHH", 875),
				new Tuple<string, int?>("AAA", 1000),
				new Tuple<string, int?>("BBB", 2000),
				new Tuple<string, int?>("CCC", 3000),
				new Tuple<string, int?>("DDD", 4000),
			}, result4);

			subRuleCollection1.RemoveAt(4);
			var result5 = parentRule1.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{
				new Tuple<string, int?>("EEE", 500),
				new Tuple<string, int?>("FFF", 625),
				new Tuple<string, int?>("GGG", 750),
				new Tuple<string, int?>("HHH", 875),
				new Tuple<string, int?>("BBB", 2000),
				new Tuple<string, int?>("CCC", 3000),
				new Tuple<string, int?>("DDD", 4000),
			}, result5);

			subRuleCollection1.RemoveRange(0, 4);
			var result6 = parentRule1.eHubRoutingRules_Group.OrderBy(r => r.RR_Group_Ordering).Select(r => new Tuple<string, int?>(r.RR_Condition_Expression, r.RR_Group_Ordering)).ToArray();
			CollectionAssert.AreEqual(new Tuple<string, int?>[] 
			{
				new Tuple<string, int?>("BBB", 2000),
				new Tuple<string, int?>("CCC", 3000),
				new Tuple<string, int?>("DDD", 4000),
			}, result6);
		}
	}
}
