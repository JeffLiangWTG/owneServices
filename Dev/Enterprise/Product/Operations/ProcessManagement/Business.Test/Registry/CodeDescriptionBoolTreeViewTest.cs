using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public abstract class CodeDescriptionBoolTreeViewTest<T> : NonPersistentBusinessObjectCollectionViewTestCase<T> where T : CodeDescriptionBoolTreeView
	{
		public void TestAddNewCore()
		{
			var types = (NoResString)"Types";
			var subtypes = (NoResString)"Sub Types";
			var areas = (NoResString)"Areas";

			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4, new MultilingualString[] { types, subtypes, areas });
			tree.AddSystemChildren(null);
			var all = CodeDescriptionBoolTreeNode.AllCode;
			tree.AddSystemChildren(tree.Add("UDF"));
			tree.AddSystemChildren(tree.Add("UDF", tree.Find(all)));
			tree.AddSystemChildren(tree.Add("UDF", tree.Find(all, all)));
			var node2 = tree.Find(all);
			var node3 = tree.Find(all, all);
			AssertEquals("Precondition", 9, tree.Count);

			var treeView1 = new CodeDescriptionBoolTreeView(tree, null);
			var newNode1 = treeView1.AddNew();
			newNode1.Code = "AAA";
			AssertEquals(ZGuid.Empty, newNode1.ParentID);
			AssertEquals("A new type, and its common sub types and common areas, are added", 12, tree.Count);
			AssertEquals(newNode1.Code, tree[9].Code);
			AssertEquals(subtypes, tree[10].Description);
			AssertEquals(areas, tree[11].Description);

			var treeView2 = new CodeDescriptionBoolTreeView(tree, node2);
			var newNode2 = treeView2.AddNew();
			newNode2.Code = "BBB";
			AssertEquals(node2.ID, newNode2.ParentID);
			AssertEquals("A new sub type, and its common areas, are added", 14, tree.Count);
			AssertEquals(newNode2.Code, tree[12].Code);
			AssertEquals(areas, tree[13].Description);

			var treeView3 = new CodeDescriptionBoolTreeView(tree, node3);
			var newNode3 = treeView3.AddNew();
			newNode3.Code = "CCC";
			AssertEquals(node3.ID, newNode3.ParentID);
			AssertEquals("A new area is added", 15, tree.Count);
			AssertEquals(newNode3.Code, tree[14].Code);
		}

		public void TestRemoveAndDelete()
		{
			// ImportWizard.GenerateReadOnlyWarnings creates a BizObj in the treeview and later deletes from it.
			// Ensure the bizobj is deleted from the collection if it's not in the treeview.
			var allDescriptions = new MultilingualString[] { (NoResString)"Any", (NoResString)"Description" };

			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 3, allDescriptions);
			var node1 = tree.Add("AAA");
			var node2 = tree.Add("AAA");

			var treeView1 = new CodeDescriptionBoolTreeView(tree, null);
			var treeViewNode1 = ((IBusinessObjectCollection)treeView1).AddNew() as CodeDescriptionBoolTreeNode;

			AssertNoExceptionThrown(() => { treeView1.RemoveAndDelete(treeViewNode1); });

			var treeView2 = new CodeDescriptionBoolTreeView(tree, node1);
			var treeViewNode2 = ((IBusinessObjectCollection)treeView2).AddNew() as CodeDescriptionBoolTreeNode;

			AssertNoExceptionThrown(() => { treeView2.RemoveAndDelete(treeViewNode2); });

			var treeView3 = new CodeDescriptionBoolTreeView(tree, node2);
			var treeViewNode3 = ((IBusinessObjectCollection)treeView3).AddNew() as CodeDescriptionBoolTreeNode;

			AssertNoExceptionThrown(() => { treeView3.RemoveAndDelete(treeViewNode3); });
		}

		public void TestSetDefaultsForNewChild()
		{
			var allDescriptions = new MultilingualString[] { (NoResString)"Any" };

			var codeList1 = new CodeDescriptionPairList();
			codeList1.AddPair("L1A", "Level 1 A Desc");
			codeList1.AddPair("L1B", "Level 1 B Desc");

			var codeList2 = new CodeDescriptionPairList();
			codeList2.AddPair("L2A", "Level 2 A Desc");
			codeList2.AddPair("L2B", "Level 2 B Desc");

			var codeLists = new CodeDescriptionPairList[] { codeList1, codeList2 };

			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 2, allDescriptions, codeLists);
			var node1 = tree.Add("AAA");

			var treeView1 = new CodeDescriptionBoolTreeView(tree, null);
			var newNode1 = ((IBindingList)treeView1).AddNew() as CodeDescriptionBoolTreeNode;
			AssertEquals(ZGuid.Empty, newNode1.ParentID);
			AssertContainsExactElementsInAnyOrder(codeList1, newNode1.CodeList);

			var treeView2 = new CodeDescriptionBoolTreeView(tree, node1);
			var newNode2 = ((IBindingList)treeView2).AddNew() as CodeDescriptionBoolTreeNode;
			AssertEquals(node1.ID, newNode2.ParentID);
			AssertContainsExactElementsInAnyOrder(codeList2, newNode2.CodeList);
		}

		public void TestSort()
		{
			var types = (NoResString)"Types";
			var subtypes = (NoResString)"Sub Types";
			var areas = (NoResString)"Areas";

			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4, new MultilingualString[] { types, subtypes, areas });
			tree.AddSystemChildren(null);
			var all = CodeDescriptionBoolTreeNode.AllCode;
			tree.AddSystemChildren(tree.Add("UDF"));
			tree.AddSystemChildren(tree.Add("UDF", tree.Find(all)));
			tree.AddSystemChildren(tree.Add("UDF", tree.Find(all, all)));
			AssertEquals("Precondition", 9, tree.Count);

			var treeView = new CodeDescriptionBoolTreeView(tree, null);
			var newNode1 = treeView.AddNew();
			newNode1.Code = "AAA";
			var newNode2 = treeView.AddNew();
			newNode2.Code = "CCC";
			var newNode3 = treeView.AddNew();
			newNode3.Code = "BBB";

			treeView.Sort("Code", ListSortDirection.Descending);
			AssertEquals(all, treeView[0].Code);
			AssertEquals("UDF", treeView[1].Code);
			AssertEquals("CCC", treeView[2].Code);
			AssertEquals("BBB", treeView[3].Code);
			AssertEquals("AAA", treeView[4].Code);

			treeView.Sort("Code", ListSortDirection.Ascending);
			AssertEquals(all, treeView[0].Code);
			AssertEquals("AAA", treeView[1].Code);
			AssertEquals("BBB", treeView[2].Code);
			AssertEquals("CCC", treeView[3].Code);
			AssertEquals("UDF", treeView[4].Code);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = new CodeDescriptionBoolTreeNode();
			return result;
		}
	}

	[TestedType(typeof(CodeDescriptionBoolTreeView))]
	public class CodeDescriptionBoolTreeViewTest : CodeDescriptionBoolTreeViewTest<CodeDescriptionBoolTreeView>
	{
		protected override CodeDescriptionBoolTreeView GetCollectionToTest()
		{
			var allNodes = new CodeDescriptionBoolTreeNodeCollection();
			return new CodeDescriptionBoolTreeView(allNodes, null);
		}
	}
}
