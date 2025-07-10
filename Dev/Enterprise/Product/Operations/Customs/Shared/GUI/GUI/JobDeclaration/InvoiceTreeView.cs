using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class InvoiceTreeView : ZTreeView
	{
		public InvoiceTreeView()
		{
			LabelEdit = false;
		}

		public void ClearNodes()
		{
			if (Nodes.Count > 0)
			{
				AllInvoicesNode.UnhookInvoiceEvents();
				Nodes.Clear();
			}
		}

		protected override void OnAfterSelect(TreeViewEventArgs e)
		{
			base.OnAfterSelect(e);
			JobComInvoiceHeaderTreeNode selectedNode = SelectedNode as JobComInvoiceHeaderTreeNode;
			if (OnSelectedNodeChanged != null)
			{
				if (selectedNode != null && selectedNode.GroupInvoiceOrInvoice != null)
				{
					OnSelectedNodeChanged(selectedNode.GroupInvoiceOrInvoice.IsGroupInvoice);
				}
			}
		}

		public event SelectedNodeChangedEventHandler OnSelectedNodeChanged;
		public delegate void SelectedNodeChangedEventHandler(bool isGroupInvoiceSelected);

		public ZBusinessObjectTreeNode GetTreeNodeForBusinessObject(BusinessObject bizO)
		{
			return RecursivelyLocateBusinessObjectFromTag(Nodes[0] as ZBusinessObjectTreeNode, bizO);
		}

		public JobComInvoiceHeaderTreeNode AllInvoicesNode
		{
			get { return Nodes[0] as JobComInvoiceHeaderTreeNode; }
		}

		#region Implementation

		protected override void OnAfterLabelEdit(NodeLabelEditEventArgs e)
		{
			base.OnAfterLabelEdit(e);
			IGroupInvoiceOrInvoice groupInvoiceOrInvoice = ((JobComInvoiceHeaderTreeNode)e.Node).GroupInvoiceOrInvoice;
			if (groupInvoiceOrInvoice != null)
			{
				groupInvoiceOrInvoice.JZ_InvoiceNumber = e.Label;
			}
		}

		ZBusinessObjectTreeNode RecursivelyLocateBusinessObjectFromTag(ZBusinessObjectTreeNode currentNode, BusinessObject bizO)
		{
			if (currentNode.BizO == bizO)
			{
				return currentNode;
			}

			foreach (ZBusinessObjectTreeNode aNode in currentNode.Nodes)
			{
				ZBusinessObjectTreeNode result = RecursivelyLocateBusinessObjectFromTag(aNode, bizO);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		#region Disposing

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (Nodes.Count > 0)
				{
					UnhookNodeEvents(Nodes[0]);
				}
			}

			base.Dispose(isNotFinalizing);
		}

		void UnhookNodeEvents(TreeNode node)
		{
			if (node is JobComInvoiceHeaderTreeNode jobInvoiceHeaderNode)
			{
				jobInvoiceHeaderNode.UnhookInvoiceEvents();
			}

			foreach (ZBusinessObjectTreeNode childNode in node.Nodes)
			{
				UnhookNodeEvents(childNode);
			}
		}

		#endregion

		#endregion
	}

	public class JobComInvoiceHeaderTreeNode : ZBusinessObjectTreeNode
	{
		public JobComInvoiceHeaderTreeNode(IGroupInvoiceOrInvoice bizO, string initialText, int imageIndex, int errorIndex, int messageErrorIndex, int warningIndex)
			: base(bizO as BusinessObject, initialText, imageIndex, errorIndex, messageErrorIndex, warningIndex)
		{
		}

		protected override void Bind()
		{
			base.Bind();
			GroupInvoiceOrInvoice.JZ_InvoiceNumberInfo.ValueChanged += BizO_JZ_InvoiceNumberChanged;
		}

		void BizO_JZ_InvoiceNumberChanged(object sender, EventArgs e)
		{
			Text = GroupInvoiceOrInvoice.JZ_InvoiceNumber;
		}

		public IGroupInvoiceOrInvoice GroupInvoiceOrInvoice
		{
			get { return (IGroupInvoiceOrInvoice)BizO; }
		}

		internal void UnhookInvoiceEvents()
		{
			GroupInvoiceOrInvoice.JZ_InvoiceNumberInfo.ValueChanged -= BizO_JZ_InvoiceNumberChanged;
		}
	}
}
