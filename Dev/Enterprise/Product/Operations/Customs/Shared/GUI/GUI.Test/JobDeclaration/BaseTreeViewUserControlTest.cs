using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseTreeViewUserControlTestCase : TestCaseWithFactory
	{
		public void TestNotifiedWhenNotificationChanged()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var topGroupHeader = testDec.JobComInvoiceGroupHeaders[0];
			var subGroupHeader = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			subGroupHeader.JZ_InvoiceNumber = "SUB";

			var invoice = topGroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceNumber = "Invoice1";

			using (var testTreeUserControl = new TestTreeViewUserControl())
			{
				testTreeUserControl.SetDataBinding(testDec, "");
				AssertEquals("TestTreeUserControl.UpdateTreeNodesNotificationsCalled is false", false, testTreeUserControl.UpdateTreeNodesNotificationsCalled);

				using (invoice.SuspendValidationTesting())
				{
					invoice.JZ_InvoiceNumberInfo.AddMessageError("TEST");
				}
				AssertEquals("TestTreeUserControl.UpdateTreeNodesNotificationsCalled should have called", true, testTreeUserControl.UpdateTreeNodesNotificationsCalled);
			}
		}

		public void TestUpdateTreeViewWhenChangingGroupInvoiceBeforeJobComInvoiceHeadersAreReferenced()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var topGroupHeader = testDec.JobComInvoiceGroupHeaders[0];
			var subGroupHeader = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			subGroupHeader.JZ_InvoiceNumber = "SUB";

			var invoice1 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceNumber = "Invoice1";
			var invoice2 = subGroupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceNumber = "Invoice2";
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var decLoaded = anotherFactory.Load<BaseJobDeclaration>(testDec.PK);
			var subGroupLoaded = anotherFactory.Load<BaseJobComInvoiceGroupHeader>(subGroupHeader.PK);
			using (var testTreeUserControl = new BaseTreeViewUserControl())
			{
				testTreeUserControl.SetDataBinding(decLoaded, "");

				AssertEquals("Test tree has 1 top node", 1, testTreeUserControl.TreeView.Nodes.Count);
				AssertEquals("Top node has two sub nodes", 2, testTreeUserControl.TreeView.Nodes[0].Nodes.Count);

				var topNode = testTreeUserControl.TreeView.Nodes[0] as ZBusinessObjectTreeNode;
				var nodeForSubGroup = topNode.FindNodeForObject(subGroupLoaded);
				AssertEquals("Sub group header has 1 node", 1, nodeForSubGroup.Nodes.Count);

				BaseJobComInvoiceHeader invoice2Loaded = null;
				foreach (var invoice in decLoaded.Invoices)
				{
					if (invoice.PK == invoice2.PK)
					{
						invoice2Loaded = invoice;
						break;
					}
				}

				invoice2Loaded.JZ_Calc_GroupInvoice = "TOP";

				testTreeUserControl.Visible = false;
				testTreeUserControl.Visible = true;

				topNode = testTreeUserControl.TreeView.Nodes[0] as ZBusinessObjectTreeNode;
				nodeForSubGroup = topNode.FindNodeForObject(subGroupLoaded);

				AssertEquals("Top node has a node for invoice2 now", 2, topNode.Nodes.Count);
				AssertEquals("sub group node has 1 node", 1, nodeForSubGroup.Nodes.Count);
			}
		}

		public void TestCannotMakeChildOfSelf()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var topGroup = declaration.JobComInvoiceGroupHeaders[0];
			var parent = topGroup.JobComInvoiceGroupHeaders.AddNew();
			var child = parent.JobComInvoiceGroupHeaders.AddNew();
			var grandChild = child.JobComInvoiceGroupHeaders.AddNew();

			using (var treeView = new BaseTreeViewUserControl())
			{
				treeView.SetDataBinding(declaration, "");

				var dragged = new ArrayList();
				dragged.Add(parent);
				treeView.HandleDragDrop(treeView.TreeView.GetTreeNodeForBusinessObject(grandChild) as JobComInvoiceHeaderTreeNode, dragged);
				Assert("Impossible move message should be shown", UnitTestUserNotification.Instance.LastMessage.Text.Contains("that move is not possible"));
			}
		}

		public void TestSwapOfParentChild()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var topGroup = declaration.JobComInvoiceGroupHeaders[0];
			var parent = topGroup.JobComInvoiceGroupHeaders.AddNew();
			var child = parent.JobComInvoiceGroupHeaders.AddNew();

			using (var treeView = new BaseTreeViewUserControl())
			{
				treeView.SetDataBinding(declaration, "");

				AssertEquals(1, topGroup.JobComInvoiceGroupHeaders.Count);
				AssertEquals(1, parent.JobComInvoiceGroupHeaders.Count);
				AssertEquals(0, child.JobComInvoiceGroupHeaders.Count);
				AssertEquals(null, topGroup.GroupHeader);
				AssertEquals(topGroup, parent.GroupHeader);
				AssertEquals(parent, child.GroupHeader);

				var dragged = new ArrayList();
				dragged.Add(child);
				treeView.HandleDragDrop(treeView.TreeView.GetTreeNodeForBusinessObject(parent) as JobComInvoiceHeaderTreeNode, dragged);
				AssertEquals(1, topGroup.JobComInvoiceGroupHeaders.Count);
				AssertEquals(1, parent.JobComInvoiceGroupHeaders.Count);
				AssertEquals(0, child.JobComInvoiceGroupHeaders.Count);
				AssertEquals(null, topGroup.GroupHeader);
				AssertEquals(topGroup, parent.GroupHeader);
				AssertEquals(parent, child.GroupHeader);

				dragged = new ArrayList();
				dragged.Add(child);
				treeView.HandleDragDrop(treeView.TreeView.GetTreeNodeForBusinessObject(topGroup) as JobComInvoiceHeaderTreeNode, dragged);
				AssertEquals(2, topGroup.JobComInvoiceGroupHeaders.Count);
				AssertEquals(0, parent.JobComInvoiceGroupHeaders.Count);
				AssertEquals(0, child.JobComInvoiceGroupHeaders.Count);
				AssertEquals(null, topGroup.GroupHeader);
				AssertEquals(topGroup, parent.GroupHeader);
				AssertEquals(topGroup, child.GroupHeader);
			}
		}

		public void TestHandleDrapDrop()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader topGroupInvoice = topGroup.JobComInvoiceHeaders.AddNew();

			BaseJobComInvoiceGroupHeader subGroup1 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader subGroup1Invoice = subGroup1.JobComInvoiceHeaders.AddNew();

			BaseJobComInvoiceGroupHeader subGroup2 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader subGroup2Invoice = subGroup2.JobComInvoiceHeaders.AddNew();

			using (BaseTreeViewUserControl testControl = new BaseTreeViewUserControl())
			{
				testControl.SetDataBinding(testDec, "");
				testControl.Visible = true;
				AssertEquals("Top node in the tree view", 1, testControl.TreeView.Nodes.Count);
				AssertEquals("Two sub-nodes for the top node", 3, testControl.TreeView.Nodes[0].Nodes.Count);

				ArrayList groupInvoiceToMove = new ArrayList();
				groupInvoiceToMove.Add(subGroup2);

				DragDropEffects result = testControl.HandleDragDrop(testControl.TreeView.GetTreeNodeForBusinessObject(subGroup1) as JobComInvoiceHeaderTreeNode, groupInvoiceToMove);
				AssertEquals("DragDrop Effect should be Move", DragDropEffects.Move, result);
				AssertEquals("SubGroup2's parent should now be SubGroup1", subGroup1, subGroup2.GroupHeader);

				groupInvoiceToMove = new ArrayList();
				groupInvoiceToMove.Add(subGroup2);
				result = testControl.HandleDragDrop(testControl.TreeView.GetTreeNodeForBusinessObject(subGroup2) as JobComInvoiceHeaderTreeNode, groupInvoiceToMove);
				AssertEquals("SubGroup2's parent should still be SubGroup1", subGroup1, subGroup2.GroupHeader);

				groupInvoiceToMove = new ArrayList();
				groupInvoiceToMove.Add(subGroup2Invoice);
				result = testControl.HandleDragDrop(testControl.TreeView.GetTreeNodeForBusinessObject(subGroup1) as JobComInvoiceHeaderTreeNode, groupInvoiceToMove);
				AssertEquals("DragDrop Effect should be Move", DragDropEffects.Move, result);
				AssertEquals("SubGroup2Invoice's parent should still be SubGroup1", subGroup1, subGroup2Invoice.GroupHeader);
			}
		}

		public void TestInsertTreeNodeForGroupHeader()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];

			using (BaseTreeViewUserControl testControl = new BaseTreeViewUserControl())
			{
				testControl.SetDataBinding(testDec, "");
				AssertEquals("There should be only one node in the tree", 1, testControl.TreeView.Nodes.Count);
				AssertEquals("There should be only one node in the tree", 0, testControl.TreeView.Nodes[0].Nodes.Count);

				BaseJobDeclaration testDec2ForTestingPurpose = BaseJobDeclaration.New(Factory);//This should not happen in real situations. If we dont test like this, nodes will be added automatically
				BaseJobComInvoiceGroupHeader groupToInsert = testDec2ForTestingPurpose.JobComInvoiceGroupHeaders[0];
				BaseJobComInvoiceGroupHeader newSubGroup = groupToInsert.JobComInvoiceGroupHeaders.AddNew();
				BaseJobComInvoiceHeader newInvoice = groupToInsert.JobComInvoiceHeaders.AddNew();

				testControl.InsertTreeNode(topGroup, groupToInsert);
				ZBusinessObjectTreeNode insertedNodeForGroupToInsert = testControl.TreeView.Nodes[0].Nodes[0] as ZBusinessObjectTreeNode;

				AssertEquals("GroupToInsert is a child of TopGroup now", groupToInsert, insertedNodeForGroupToInsert.BizO);
				AssertEquals("New node should have two sub nodes", 2, insertedNodeForGroupToInsert.Nodes.Count);

				AssertNotNull("New node should have a node for NewSubGroup", insertedNodeForGroupToInsert.FindNodeForObject(newSubGroup));
				AssertNotNull("New node should have a node for NewInvoice", insertedNodeForGroupToInsert.FindNodeForObject(newInvoice));
			}
		}

		public void TestInsertTreeNodeForInvoice()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];

			using (BaseTreeViewUserControl testControl = new BaseTreeViewUserControl())
			{
				testControl.SetDataBinding(testDec, "");
				testControl.Visible = true;
				AssertEquals("There should be only one node in the tree", 1, testControl.TreeView.Nodes.Count);
				AssertEquals("There should be only one node in the tree", 0, testControl.TreeView.Nodes[0].Nodes.Count);

				BaseJobComInvoiceHeader newInvoice = testDec.Invoices.AddNew();

				testControl.InsertTreeNode(topGroup, newInvoice);
				ZBusinessObjectTreeNode topNode = testControl.TreeView.Nodes[0] as ZBusinessObjectTreeNode;

				AssertNotNull("New node should have a node for NewInvoice", topNode.FindNodeForObject(newInvoice));
			}
		}

		public void TestCurrentDataItemChangedWithNullTreeView()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			using (BaseTreeViewUserControl treeView = new BaseTreeViewUserControl())
			{
				treeView.TreeView = null;
				AssertNoExceptionThrown(() => treeView.SetDataBinding(declaration, ""));
			}
		}

		sealed class TestTreeViewUserControl : BaseTreeViewUserControl
		{
			public bool UpdateTreeNodesNotificationsCalled;
			protected override void UpdateTreeNodesNotifications()
			{
				base.UpdateTreeNodesNotifications();
				UpdateTreeNodesNotificationsCalled = true;
			}
		}
	}
}
