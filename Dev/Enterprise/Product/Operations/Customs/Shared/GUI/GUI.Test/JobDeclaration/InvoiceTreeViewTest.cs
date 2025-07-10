using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BusinessObjectTreeNodeTest : TestCase
	{
		public void TestBusinessObjectTreeNode()
		{
			var factory = new BusinessObjectFactory();
			var erCompany = factory.New<GlbCompany>();
			erCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var erBranch = erCompany.Branches.AddNew();
			erBranch.GB_RL_NKHomePort = "ERBRN";

			string inv1Number = "TEST1";
			string inv2Number = "Test2";
			string inv1NodeText = "TestFromNode1";
			string inv2NodeText = "TestFromNode2";
			BaseJobDeclaration testDec = factory.New<BaseJobDeclaration>();
			testDec.JE_GB = erBranch.PK;
			BaseJobComInvoiceHeader inv1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader inv2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			using (ZForm testForm = new ZForm(testDec))
			{
				InvoiceTreeView testTreeView = new InvoiceTreeView();
				testForm.Controls.Add(testTreeView);
				testForm.Show();
				ZBusinessObjectTreeNode rootNode = new JobComInvoiceHeaderTreeNode(testDec.JobComInvoiceGroupHeaders[0], "Test", 0, 0, 0, 0);
				testTreeView.Nodes.Add(rootNode);
				ZBusinessObjectTreeNode inv1Node = new JobComInvoiceHeaderTreeNode(inv1, "Inv 1", 1, 1, 1, 1);
				ZBusinessObjectTreeNode inv2Node = new JobComInvoiceHeaderTreeNode(inv2, "Inv 2", 1, 1, 1, 1);

				rootNode.Nodes.Add(inv1Node);
				rootNode.Nodes.Add(inv2Node);

				inv1.JZ_InvoiceNumber = inv1Number;
				AssertEquals("Inv1Node.Text", inv1Number, inv1Node.Text);
				inv2.JZ_InvoiceNumber = inv2Number;
				AssertEquals("Inv2Node.Text", inv2Number.ToUpper(), inv2Node.Text);

				FireLabelChangeEventInvTreeView(testTreeView, inv1Node, inv1NodeText);
				AssertEquals("Inv1 JZ_InvoiceNumber", inv1NodeText.ToUpper(), inv1.JZ_InvoiceNumber);
				FireLabelChangeEventInvTreeView(testTreeView, inv2Node, inv2NodeText);
				AssertEquals("Inv2 JZ_InvoiceNumber", inv2NodeText.ToUpper(), inv2.JZ_InvoiceNumber);
			}
		}

		public void TestJobComInvoiceHeaderTreeNode()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BaseJobDeclaration testDec = factory.New<BaseJobDeclaration>();

			InvoiceForTest inv1 = factory.New<InvoiceForTest>();
			inv1.JZ_JE = testDec.PK;

			InvoiceForTest inv2 = factory.New<InvoiceForTest>();
			inv2.JZ_JE = testDec.PK;

			const int NormalIndex = 0;
			const int ErrorIndex = 1;
			const int MessageErrorIndex = 2;
			const int WarningIndex = 3;

			using (var tree = new InvoiceTreeView())
			{
				var testNode = new JobComInvoiceHeaderTreeNode(inv1, "New Invoice", NormalIndex, ErrorIndex, MessageErrorIndex, WarningIndex);
				tree.Nodes.Add(testNode);
				inv1.JZ_OH_Supplier = factory.New<OrgHeader>().PK;
				testDec.ResumeApportionment();

				inv1.ShouldAddWarning = true;
				testNode.UpdateImageIndex(inv1, EventArgs.Empty);

				AssertEquals("Inv1 Warnings", true, inv1.HasWarnings);
				AssertEquals("Inv1 Errors", false, inv1.HasErrors);
				AssertEquals("Inv1 MessageErrors", false, inv1.HasMessageErrors);
				AssertEquals("Warning Index should be ImageIndex", WarningIndex, testNode.ImageIndex);

				inv1.ShouldAddWarning = false;
				inv1.ShouldAddMessageError = true;
				inv1.JZ_InvoiceNumber = "12";
				testNode.UpdateImageIndex(inv1, EventArgs.Empty);

				AssertEquals("Inv1 Errors", false, inv1.HasErrors);
				AssertEquals("Inv1 MessageErrors", true, inv1.HasMessageErrors);
				AssertEquals("Message Error Index should be ImageIndex", MessageErrorIndex, testNode.ImageIndex);

				inv1.ShouldAddWarning = false;
				inv1.ShouldAddMessageError = true;
				inv1.JZ_InvoiceNumber = "";
				testNode.UpdateImageIndex(inv1, EventArgs.Empty);

				AssertEquals("Inv1 Errors", false, inv1.HasErrors);
				AssertEquals("Inv1 MessageErrors", true, inv1.HasMessageErrors);
				AssertEquals("Message Error Index should be ImageIndex", MessageErrorIndex, testNode.ImageIndex);
			}
		}

		public void TestGetZBusinessObjectTreeNode()
		{
			BaseJobDeclaration testDec = new BusinessObjectFactory().New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader inv1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader inv2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			using (InvoiceTreeView testTreeView = new InvoiceTreeView())
			{
				ZBusinessObjectTreeNode rootNode = new JobComInvoiceHeaderTreeNode(testDec.JobComInvoiceGroupHeaders[0], "Test", 0, 0, 0, 0);
				ZBusinessObjectTreeNode groupNode1 = new ZBusinessObjectTreeNode(null, "g1", 0, 0, 0, 0);
				ZBusinessObjectTreeNode groupNode2 = new ZBusinessObjectTreeNode(null, "g2", 0, 0, 0, 0);
				testTreeView.Nodes.Add(rootNode);
				ZBusinessObjectTreeNode inv1Node = new JobComInvoiceHeaderTreeNode(inv1, "Inv 1", 1, 1, 1, 1);
				ZBusinessObjectTreeNode inv2Node = new JobComInvoiceHeaderTreeNode(inv2, "Inv 2", 1, 1, 1, 1);

				rootNode.Nodes.Add(groupNode1);
				rootNode.Nodes.Add(groupNode2);

				groupNode1.Nodes.Add(inv1Node);
				groupNode2.Nodes.Add(inv2Node);

				AssertEquals("Failed to locate Node for BizO", inv1Node, testTreeView.GetTreeNodeForBusinessObject(inv1));
				AssertEquals("Failed to locate Node for BizO", inv2Node, testTreeView.GetTreeNodeForBusinessObject(inv2));
				AssertEquals("Failed to locate Node for BizO", inv1Node, testTreeView.GetTreeNodeForBusinessObject(inv1));
			}
		}

		void FireLabelChangeEventInvTreeView(InvoiceTreeView treeView, ZBusinessObjectTreeNode node, string editText)
		{
			NodeLabelEditEventArgs nodeLabelEditEventArgs = new NodeLabelEditEventArgs(node, editText);
			MethodInfo onAfterLabelEditMethod = typeof(InvoiceTreeView).GetMethod("OnAfterLabelEdit", BindingFlags.Instance | BindingFlags.NonPublic);
			onAfterLabelEditMethod.Invoke(treeView, new object[] { nodeLabelEditEventArgs });
		}

		sealed class InvoiceForTest : BaseJobComInvoiceHeader
		{
			public InvoiceForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public bool ShouldAddWarning;
			public bool ShouldAddMessageError;

			protected override JobComInvoiceHeaderValidation GetNewValidation()
			{
				return new InvoiceForTestValidation(this);
			}

			public new InvoiceForTestValidation Validation
			{
				get { return (InvoiceForTestValidation)base.Validation; }
			}
		}

		sealed class InvoiceForTestValidation : InvoiceHeaderValidation
		{
			public InvoiceForTestValidation(InvoiceForTest invoice)
				: base(invoice)
			{
			}

			new InvoiceForTest Parent => (InvoiceForTest)base.Parent;

			protected override void CheckJZ_InvoiceNumber()
			{
				base.CheckJZ_InvoiceNumber();

				if (Parent.ShouldAddWarning)
				{
					Parent.JZ_InvoiceNumberInfo.AddWarning("Test Warning");
				}

				if (Parent.ShouldAddMessageError)
				{
					Parent.JZ_InvoiceNumberInfo.AddMessageError("Test Message Error");
				}
			}
		}
	}
}
