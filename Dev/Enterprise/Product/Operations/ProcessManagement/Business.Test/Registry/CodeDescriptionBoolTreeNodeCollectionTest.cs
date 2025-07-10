using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public abstract class CodeDescriptionBoolTreeNodeCollectionTest<T> : CodeDescriptionBoolCollectionAbstractTest<T> where T : CodeDescriptionBoolTreeNodeCollection
	{
		const string ALL = CodeDescriptionBoolTreeNode.AllCode;

		public void TestAdd_CodeParent()
		{
			var node1 = Collection.Add("1AA");
			var node1a = Collection.Add("2AA", node1);
			var node1a1 = Collection.Add("3AA", node1a);
			var node2 = Collection.Add("BBB");

			AssertEquals("1AA", node1.Code);
			AssertEquals(true, node1.ID.IsValid);
			AssertEquals("ParentID.IsEmpty", true, node1.ParentID.IsEmpty);

			AssertEquals("2AA", node1a.Code);
			AssertEquals(true, node1a.ID.IsValid);
			AssertEquals("ParentID", node1.ID, node1a.ParentID);
		}

		public void TestGetParents()
		{
			var node1 = Collection.Add("1AA");
			var node1a = Collection.Add("211", node1);
			var node1a1 = Collection.Add("31A", node1a);
			var node2 = Collection.Add("1BB");

			CodeDescriptionPairList expected = new CodeDescriptionPairList();
			expected.AddPair("1AA");
			expected.AddPair("1BB");

			var actual = Collection.GetParents(true);
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestGetParents_SortedByDescription()
		{
			var node1 = Collection.Add("1AA", (NoResString)"ZZZ Description");
			var node2 = Collection.Add("1BB", (NoResString)"AAA Description");
			var node3 = Collection.Add("1CC", (NoResString)"MMM Description");

			var expected = new CodeDescriptionPairList();
			expected.AddPair(node2.Code, node2.Description);
			expected.AddPair(node3.Code, node3.Description);
			expected.AddPair(node1.Code, node1.Description);

			var actual = Collection.GetParents(true);
			AssertContainsExactElementsInExactOrder(expected, actual);
		}

		void AssertContainsExactElementsInExactOrder(CodeDescriptionPairList expected, CodeDescriptionPairList actual)
		{
			AssertEquals("Count", expected.Count, actual.Count);
			for (int i = 0; i < expected.Count; i++)
			{
				AssertEquals(expected[i].Code, actual[i].Code);
				AssertEquals(expected[i].Description, actual[i].Description);
			}
		}

		public void TestFind()
		{
			var node1 = Collection.Add("1AA");
			var node1a = Collection.Add("2AA", node1);
			var node1a1 = Collection.Add("3AA", node1a);
			var node2 = Collection.Add("1BB");

			var actual1 = Collection.Find("1AA", "2AA", "3AA");
			var actual2 = Collection.Find("1AA", "2BB", "3AA");
			var actual3 = Collection.Find("1AA", "2AA");
			var actual4 = Collection.Find("1BB", "2BB");
			var actual5 = Collection.Find("1BB");

			AssertEquals(node1a1, actual1);
			AssertNull(actual2);
			AssertEquals(node1a, actual3);
			AssertNull(actual4);
			AssertEquals(node2, actual5);
		}

		public void TestRemoveAndDelete()
		{
			var node1 = Collection.Add("1AA");
			var node1a = Collection.Add("211", node1);
			var node1a1 = Collection.Add("31A", node1a);
			var node2 = Collection.Add("1BB");
			var node2a = Collection.Add("2BB", node2);

			var node3 = Collection.Add("1CC");
			var node3a = Collection.Add("2CC", node3);
			var node3a1 = Collection.Add("3CC", node3a);
			var node3a1a = Collection.Add("4CC", node3a1);

			CodeDescriptionPairList expected = new CodeDescriptionPairList();
			expected.AddPair("1BB");
			expected.AddPair("2BB");
			expected.AddPair("1CC");
			expected.AddPair("2CC");

			Collection.RemoveAndDelete(node1);
			Collection.RemoveAndDelete(node3a1);

			AssertContainsExactElementsInAnyOrder("descendants also deleted", expected, Collection);
		}

		public void TestGetDepth()
		{
			var node1 = Collection.Add("1AA");
			var node1a = Collection.Add("2AA", node1);
			var node1a1 = Collection.Add("3AA", node1a);
			var node2 = Collection.Add("1BB");
			var node3 = Collection.Add("1CC");
			var node3a = Collection.Add("2CC", node3);
			var node3a1 = Collection.Add("3CC", node3a);
			var node3a1a = Collection.Add("4CA", node3a1);
			var node3a1b = Collection.Add("4CB", node3a1);

			AssertEquals(1, Collection.GetDepth(node1));
			AssertEquals(2, Collection.GetDepth(node1a));
			AssertEquals(3, Collection.GetDepth(node1a1));
			AssertEquals(1, Collection.GetDepth(node3));
			AssertEquals(2, Collection.GetDepth(node3a));
			AssertEquals(4, Collection.GetDepth(node3a1a));
			AssertEquals(4, Collection.GetDepth(node3a1b));
		}

		public override void TestDefaultBoolForNewChild()
		{
			var item = Collection.AddNew();
			AssertEquals("AddNew().Bool", true, item.Bool);
		}

		public void TestGetChildren1()
		{
			var tree = CreateTestTree4();
			var actual1 = tree.GetChildren("1AA", true);
			var actual2 = tree.GetChildren("1BB", true);

			var expected1 = new CodeDescriptionPairList();
			expected1.Add(tree.Find("1AA", "2AA"));
			expected1.Add(tree.Find("1AA", "2AB"));
			expected1.Add(tree.Find(ALL, "2CC"));
			expected1.Add(tree.Find(ALL, "2DD"));

			var expected2 = new CodeDescriptionPairList();
			expected2.Add(tree.Find("1BB", "2BB"));
			expected2.Add(tree.Find(ALL, "2CC"));
			expected2.Add(tree.Find(ALL, "2DD"));

			AssertContainsExactElementsInAnyOrder(expected1, actual1);
			AssertContainsExactElementsInAnyOrder(expected2, actual2);
		}

		public void TestGetChildren1_SortedByCombinedDescription()
		{
			Collection.AddSystemChildren();
			var nodeALL = Collection.Find(ALL);

			var node100 = Collection.Add("100");
			var node200 = Collection.Add("200", (NoResString)"BBB Level 2 200", node100);
			Collection.Add("201", (NoResString)"AAA Level 2 201", node100);
			Collection.Add("202", (NoResString)"CCC Level 2 202", node100);
			Collection.Add("DUP", (NoResString)"La Dup Level 2", node100);
			Collection.Add("300", (NoResString)"ZZZ Level 3 300", node200);
			Collection.Add("301", (NoResString)"DDD Level 3 301", node200);

			var nodeA00 = Collection.Add("A00");
			var nodeAA0 = Collection.Add("AA0", (NoResString)"DDD Level 2 AA0", nodeA00);
			var nodeAA1 = Collection.Add("AA1", (NoResString)"BBB Level 2 AA1", nodeA00);
			var nodeAA2 = Collection.Add("AA2", (NoResString)"AAA Level 2 AA2", nodeA00);
			var nodeAA3 = Collection.Add("AA3", (NoResString)"CCC Level 2 AA3", nodeA00);
			var nodeDup2 = Collection.Add("DUP", (NoResString)"A Dup Level 2", nodeA00);
			var nodeAAA = Collection.Add("AAA", (NoResString)"AAA Level 3 AAA", nodeAA0);

			var nodeDup3 = Collection.Add("DUP", (NoResString)"The Dup Level 2", nodeALL);

			var actual1 = Collection.GetChildren("100", true);
			var actual2 = Collection.GetChildren("A00", true);

			var expected1 = new CodeDescriptionPairList();
			expected1.AddPair("201", "AAA Level 2 201");
			expected1.AddPair("200", "BBB Level 2 200");
			expected1.AddPair("202", "CCC Level 2 202");
			expected1.AddPair("DUP", "La Dup Level 2, The Dup Level 2");

			var expected2 = new CodeDescriptionPairList();
			expected2.AddPair("DUP", "A Dup Level 2, The Dup Level 2");
			expected2.AddPair("AA2", "AAA Level 2 AA2");
			expected2.AddPair("AA1", "BBB Level 2 AA1");
			expected2.AddPair("AA3", "CCC Level 2 AA3");
			expected2.AddPair("AA0", "DDD Level 2 AA0");

			AssertContainsExactElementsInExactOrder(expected1, actual1);
			AssertContainsExactElementsInExactOrder(expected2, actual2);
		}

		public void TestGetChildren3()
		{
			var tree = CreateTestTree4();
			var actual1 = tree.GetChildren(true, "1AA", "2AA", "3AA");
			var actual2 = tree.GetChildren(true, "1BB", "2BB", "3S1");

			var expected1 = new CodeDescriptionPairList();
			expected1.Add(tree.Find("1AA", "2AA", "3AA", "4AA"));
			expected1.Add(tree.Find("1AA", "2AA", ALL, "4AX"));
			expected1.Add(tree.Find("1AA", ALL, ALL, "4XX"));
			expected1.Add(tree.Find(ALL, ALL, ALL, "ANY"));

			var expected2 = new CodeDescriptionPairList();
			expected2.Add(tree.Find(ALL, ALL, "3S1", "4S1"));
			expected2.Add(tree.Find(ALL, ALL, "3S1", "4S2"));
			expected2.Add(tree.Find(ALL, ALL, ALL, "ANY"));

			AssertContainsExactElementsInAnyOrder(expected1, actual1);
			AssertContainsExactElementsInAnyOrder(expected2, actual2);
		}

		public void TestGetChildren3_SortedByCombinedDescription()
		{
			Collection.AddSystemChildren();
			var nodeALL3 = Collection.Find(ALL, ALL, ALL);

			var node100 = Collection.Add("100");
			var node200 = Collection.Add("200", (NoResString)"200 Level 2", node100);
			var node300 = Collection.Add("300", (NoResString)"300 Level 3", node200);
			Collection.Add("DUP", (NoResString)"La Dup Level 4", node300);
			Collection.Add("400", (NoResString)"AAA Level 4 400", node300);
			Collection.Add("401", (NoResString)"DDD Level 4 401", node300);
			Collection.Add("402", (NoResString)"CCC Level 4 402", node300);
			Collection.Add("403", (NoResString)"BBB Level 4 403", node300);
			var node301 = Collection.Add("301", (NoResString)"300 Level 3", node200);
			Collection.Add("DUP", (NoResString)"La Absolute Dup Level 4", node301);

			Collection.Add("DUP", (NoResString)"And this is an duplicate @ level 4", nodeALL3);

			var actual1 = Collection.GetChildren(true, "100", "200", "300");
			var actual2 = Collection.GetChildren(true, "100", "200", "301");

			var expected1 = new CodeDescriptionPairList();
			expected1.AddPair("400", "AAA Level 4 400");
			expected1.AddPair("DUP", "And this is an duplicate @ level 4, La Dup Level 4");
			expected1.AddPair("403", "BBB Level 4 403");
			expected1.AddPair("402", "CCC Level 4 402");
			expected1.AddPair("401", "DDD Level 4 401");

			var expected2 = new CodeDescriptionPairList();
			expected2.AddPair("DUP", "And this is an duplicate @ level 4, ...");

			AssertContainsExactElementsInExactOrder(expected1, actual1);
			AssertContainsExactElementsInExactOrder(expected2, actual2);
		}

		public void TestGetChildrenExactMatchOnly()
		{
			var tree = CreateTestTree4();
			var actual1 = tree.GetChildrenExactMatchOnly(true, "1AA", "2AA", "3AA");
			var actual2 = tree.GetChildrenExactMatchOnly(true, "1BB", "2BB", "3S1");

			var expected1 = new CodeDescriptionPairList();
			expected1.Add(tree.Find("1AA", "2AA", "3AA", "4AA"));

			var expected2 = new CodeDescriptionPairList();

			AssertContainsExactElementsInAnyOrder(expected1, actual1);
			AssertContainsExactElementsInAnyOrder(expected2, actual2);
		}

		CodeDescriptionBoolTreeNodeCollection CreateTestTree4()
		{
			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4);
			tree.AddSystemChildren();

			CodeDescriptionBoolTreeTestHelper.Add(tree, "1AA");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1AA", "2AA");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1AA", "2AB");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1AA", "2AA", "3AA");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1AA", "2AA", "3AA", "4AA");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1AA", "2AA", ALL, "4AX");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1AA", ALL, ALL, "4XX");
			CodeDescriptionBoolTreeTestHelper.Add(tree, ALL, ALL, ALL, "ANY");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1AA", ALL, "3AX");

			CodeDescriptionBoolTreeTestHelper.Add(tree, "1BB");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1BB", "2BB");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "1BB", "2BB", "3BB");

			CodeDescriptionBoolTreeTestHelper.Add(tree, ALL, "2CC");
			CodeDescriptionBoolTreeTestHelper.Add(tree, ALL, "2DD");

			CodeDescriptionBoolTreeTestHelper.Add(tree, ALL, "2CC", "3CC");

			CodeDescriptionBoolTreeTestHelper.Add(tree, ALL, ALL, "3S1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, ALL, ALL, "3S2");

			CodeDescriptionBoolTreeTestHelper.Add(tree, ALL, ALL, "3S1", "4S1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, ALL, ALL, "3S1", "4S2");

			return tree;
		}

		public void TestAddSystemChildren()
		{
			var allDescriptions = new MultilingualString[]
				{
					(NoResString)"All 1",
					(NoResString)"All 2",
					(NoResString)"All 3",
					(NoResString)"All 4"
				};

			var collection = new CodeDescriptionBoolTreeNodeCollection(true, 3, 5, allDescriptions);
			collection.AddSystemChildren(null);
			AssertEquals(4, collection.Count);
			AssertEquals(allDescriptions[0], collection.Find(ALL).Description);
			AssertEquals(allDescriptions[1], collection.Find(ALL, ALL).Description);
			AssertEquals(allDescriptions[2], collection.Find(ALL, ALL, ALL).Description);
			AssertEquals(allDescriptions[3], collection.Find(ALL, ALL, ALL, ALL).Description);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionBoolTreeNode();
		}

		#endregion
	}

	[TestedType(typeof(CodeDescriptionBoolTreeNodeCollection))]
	public class CodeDescriptionBoolTreeNodeCollectionTest : CodeDescriptionBoolTreeNodeCollectionTest<CodeDescriptionBoolTreeNodeCollection>
	{
		protected override CodeDescriptionBoolTreeNodeCollection GetCollectionToTest()
		{
			var result = new CodeDescriptionBoolTreeNodeCollection();
			result.MaxDepth = 4;
			return result;
		}
	}
}
