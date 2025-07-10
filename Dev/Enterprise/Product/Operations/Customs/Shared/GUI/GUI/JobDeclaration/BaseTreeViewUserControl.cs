using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class BaseTreeViewUserControl : ZUserControl
	{
		public BaseTreeViewUserControl()
		{
			InitializeComponent();
			SetupCustomsImageList();
			TreeView.OnSelectedNodeChanged += new InvoiceTreeView.SelectedNodeChangedEventHandler(TreeView_OnSelectedNodeChanged);
		}

		public void RemoveContextMenu()
		{
			TreeViewContextMenu.MenuItems.Clear();
		}

		#region public interface

		InvoiceStructureChangedAnnouncer invoiceChangedAnnouncer;

		public new BaseJobDeclaration CurrentDataItem
		{
			get { return (BaseJobDeclaration)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (invoiceChangedAnnouncer != null)
			{
				invoiceChangedAnnouncer.Dispose();
			}
			if (CurrentDataItem != null && TreeView != null)
			{
				CurrentDataItem.NotificationsChanged -= new EventHandler<NotificationsChangedEventArgs>(JobDeclaration_NotificationsChanged);
				TreeView.DataBindings.RemoveBinding(nameof(ReadOnly));
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null && TreeView != null)
			{
				invoiceChangedAnnouncer = new InvoiceStructureChangedAnnouncer(CurrentDataItem);
				CurrentDataItem.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(JobDeclaration_NotificationsChanged);
				TreeView.DataBindings.Add(new KBinding(nameof(ReadOnly), CurrentDataItem, nameof(ReadOnly), false, DataSourceUpdateMode.Never));
				RebuildInvoiceTree();
			}
		}

		public bool ReadOnly
		{
			get { return TreeView.ReadOnly; }
			set { TreeView.ReadOnly = value; }
		}

		#endregion

		#region Redraw trees

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (invoiceChangedAnnouncer != null)
			{
				if (invoiceChangedAnnouncer.IsDirty)
				{
					RebuildInvoiceTree();
				}

				invoiceChangedAnnouncer.ClearDirtyStatus();
			}
		}

		void RebuildInvoiceTree()
		{
			TreeView.ClearNodes();

			JobComInvoiceHeaderTreeNode allInvoicesTreeNode = new JobComInvoiceHeaderTreeNode(CurrentDataItem.TopGroupInvoice, CurrentDataItem.TopGroupInvoice.JZ_InvoiceNumber, SelectedGroupImageIndex, invoiceGroupSelectedErrorImageIndex, invoiceGroupSelectedMessageErrorImageIndex, invoiceGroupSelectedWarningImageIndex);
			TreeView.Nodes.Add(allInvoicesTreeNode);
			TreeView.SelectedNode = allInvoicesTreeNode;
			AddInvoiceGroups(allInvoicesTreeNode, CurrentDataItem.TopGroupInvoice);

			TreeView.Nodes[0].Expand();
		}

		#endregion

		#region Implementation

		internal TreeNodeCollection Nodes
		{
			get { return TreeView.Nodes; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (CurrentDataItem != null)
				{
					CurrentDataItem.NotificationsChanged -= new EventHandler<NotificationsChangedEventArgs>(JobDeclaration_NotificationsChanged);
				}

				if (invoiceChangedAnnouncer != null)
				{
					invoiceChangedAnnouncer.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region TreeView Events

		void TreeView_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.None;

			if (!TreeView.ReadOnly)
			{
				if (e.Data.GetDataPresent(typeof(ArrayList)))
				{
					ZBusinessObjectTreeNode hitNode = TreeView.GetNodeAt(TreeView.PointToClient(ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y)))) as ZBusinessObjectTreeNode;
					if (hitNode != null)
					{
						e.Effect = DragDropEffects.Move;
						IGroupInvoiceOrInvoice groupInvoiceOrInvoice = ((JobComInvoiceHeaderTreeNode)hitNode).GroupInvoiceOrInvoice;
						TreeView.SelectedNode = (JobComInvoiceHeaderTreeNode)hitNode;
					}
				}
			}
		}

		void TreeView_DragDrop(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.None;
			if (!TreeView.ReadOnly)
			{
				// Hit test to new group
				var xValue = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X);
				var yValue = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y);
				Point translatedLocation = TreeView.PointToClient(ControlDpiScalingHelper.NewScaledPoint(xValue, yValue));
				ZBusinessObjectTreeNode hitNode = TreeView.GetNodeAt(translatedLocation) as ZBusinessObjectTreeNode;

				if (hitNode != null)
				{
					ArrayList data = e.Data.GetData(typeof(ArrayList)) as ArrayList;
					if (data != null)
					{
						JobComInvoiceHeaderTreeNode invoiceTreeNode = (JobComInvoiceHeaderTreeNode)hitNode;
						e.Effect = HandleDragDrop(invoiceTreeNode, data);
						TreeView.SelectedNode = invoiceTreeNode;
					}
				}
			}
		}

		internal DragDropEffects HandleDragDrop(JobComInvoiceHeaderTreeNode destinationNode, ArrayList groupInvoiceOrInvoiceToMove)
		{
			IGroupInvoiceOrInvoice destinationGroup = destinationNode.GroupInvoiceOrInvoice;

			BaseJobComInvoiceGroupHeader destinationGroupHeader = destinationNode.GroupInvoiceOrInvoice.GroupInvoiceOfInvoiceOrGroupInvoiceItself;

			DragDropEffects result = DragDropEffects.None;

			if (destinationGroupHeader != null)
			{
				foreach (IGroupInvoiceOrInvoice invoiceToMove in groupInvoiceOrInvoiceToMove)
				{
					if (!invoiceToMove.IsGroupInvoice || invoiceToMove != destinationGroupHeader)
					{
						if (destinationGroupHeader.GroupHeader == invoiceToMove)
						{
							IGroupInvoiceOrInvoice parent = invoiceToMove;
							IGroupInvoiceOrInvoice child = destinationGroupHeader;

							if (parent.IsGroupInvoice && child.IsGroupInvoice)
							{
								result = DragDropEffects.Move;
								SwapParentChild((BaseJobComInvoiceGroupHeader)parent, (BaseJobComInvoiceGroupHeader)child);
							}
						}
						else // normal move
						{
							if (IsParent(destinationGroupHeader, invoiceToMove))
							{
								Globals.Message.Show(Res.GetString("39ab4b22-5a31-4d20-956e-5f82bcdc4a53", "Sorry, that move is not possible. A group cannot be moved to a position in which it would be a sub-group of itself."));
							}
							else
							{
								result = DragDropEffects.Move;
								MoveToNewParent(invoiceToMove, destinationGroupHeader);
							}
						}
					}
				}
			}

			return result;
		}

		bool IsParent(IGroupInvoiceOrInvoice child, IGroupInvoiceOrInvoice potentialParent)
		{
			IGroupInvoiceOrInvoice current = child;
			while (current.ParentGroupInvoice != null)
			{
				if (current.ParentGroupInvoice == potentialParent)
				{
					return true;
				}
				current = current.ParentGroupInvoice;
			}
			return false;
		}

		void MoveToNewParent(IGroupInvoiceOrInvoice invoiceToMove, BaseJobComInvoiceGroupHeader newParent)
		{
			RemoveTreeNode(invoiceToMove.ParentGroupInvoice, invoiceToMove);
			invoiceToMove.Move(invoiceToMove.ParentGroupInvoice, newParent);
			InsertTreeNode(newParent, invoiceToMove);
		}

		void SwapParentChild(BaseJobComInvoiceGroupHeader parent, BaseJobComInvoiceGroupHeader child)
		{
			BaseJobComInvoiceGroupHeader top = parent.GroupHeader;

			RemoveTreeNode(top, parent);
			RemoveTreeNode(parent, child);

			((IGroupInvoiceOrInvoice)child).Move(parent, top);
			((IGroupInvoiceOrInvoice)parent).Move(top, child);

			InsertTreeNode(top, child);
		}

		void TreeView_KeyDown(object sender, KeyEventArgs e)
		{
			if (!TreeView.ReadOnly && e.KeyCode == Keys.Delete)
			{
				DeleteSelectedTreeNode();
			}
		}

		void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			//			TreeView.SelectedNode = (JobComInvoiceHeaderTreeNode)e.Node;
		}

		void TreeView_MouseDown(object sender, MouseEventArgs e)
		{
			var xValue = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X);
			var yValue = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y);
			JobComInvoiceHeaderTreeNode hitTestNode = (JobComInvoiceHeaderTreeNode)TreeView.GetNodeAt(ControlDpiScalingHelper.NewScaledPoint(xValue, yValue));
			//			TreeView.SelectedNode = HitTestNode;
		}

		#endregion

		#region Icon Management

		int CreateCombinedIcon(Image originalImage, IconTypes iconTypeToAdd)
		{
			Image iconImage = new Bitmap(originalImage);
			using (Graphics aGraphics = Graphics.FromImage(iconImage))
			{
				Image errorImage = Icons.GetIcon(iconTypeToAdd).ToBitmap();
				int x = errorImage.Width / 2;
				int y = errorImage.Height / 2;
				aGraphics.DrawImage(errorImage, x, y, x, y);
			}
			CustomsImageList.Images.Add(iconImage);
			return CustomsImageList.Images.Count - 1;
		}

		const int InvoiceImageIndex = 0;
		const int GroupImageIndex = 1;
		const int SelectedGroupImageIndex = 2;

		int invoiceErrorImageIndex;
		int invoiceMessageErrorImageIndex;
		int invoiceWarningImageIndex;
		int invoiceGroupErrorImageIndex;
		int invoiceGroupMessageErrorImageIndex;
		int invoiceGroupWarningImageIndex;
		int invoiceGroupSelectedErrorImageIndex;
		int invoiceGroupSelectedMessageErrorImageIndex;
		int invoiceGroupSelectedWarningImageIndex;

		void SetupCustomsImageList()
		{
			invoiceErrorImageIndex = CreateCombinedIcon(CustomsImageList.Images[InvoiceImageIndex], IconTypes.Error);
			invoiceMessageErrorImageIndex = CreateCombinedIcon(CustomsImageList.Images[InvoiceImageIndex], IconTypes.MessageError);
			invoiceWarningImageIndex = CreateCombinedIcon(CustomsImageList.Images[InvoiceImageIndex], IconTypes.Warning);

			invoiceGroupErrorImageIndex = CreateCombinedIcon(CustomsImageList.Images[GroupImageIndex], IconTypes.Error);
			invoiceGroupMessageErrorImageIndex = CreateCombinedIcon(CustomsImageList.Images[GroupImageIndex], IconTypes.MessageError);
			invoiceGroupWarningImageIndex = CreateCombinedIcon(CustomsImageList.Images[GroupImageIndex], IconTypes.Warning);

			invoiceGroupSelectedErrorImageIndex = CreateCombinedIcon(CustomsImageList.Images[SelectedGroupImageIndex], IconTypes.Error);
			invoiceGroupSelectedMessageErrorImageIndex = CreateCombinedIcon(CustomsImageList.Images[SelectedGroupImageIndex], IconTypes.MessageError);
			invoiceGroupSelectedWarningImageIndex = CreateCombinedIcon(CustomsImageList.Images[SelectedGroupImageIndex], IconTypes.Warning);
		}

		void SetSelectedGroupTreeNodeImages(TreeNode node)
		{
			ZBusinessObjectTreeNode selectedZNode = node as ZBusinessObjectTreeNode;
			if (selectedZNode != null)
			{
				selectedZNode.NormalImageIndex = SelectedGroupImageIndex;
				selectedZNode.ErrorImageIndex = invoiceGroupSelectedErrorImageIndex;
				selectedZNode.MessageErrorImageIndex = invoiceGroupSelectedMessageErrorImageIndex;
				selectedZNode.WarningImageIndex = invoiceGroupSelectedWarningImageIndex;
			}
		}

		void RecursivelyUpdateTreeNode(ZBusinessObjectTreeNode aNode)
		{
			aNode.UpdateImageIndex(aNode.BizO, EventArgs.Empty);
			foreach (ZBusinessObjectTreeNode node in aNode.Nodes)
			{
				RecursivelyUpdateTreeNode(node);
			}
		}

		void ResetTreeNodeImages(BaseJobComInvoiceGroupHeader header)
		{
			ZBusinessObjectTreeNode zNode = TreeView.GetTreeNodeForBusinessObject(header);
			if (zNode != null)
			{
				zNode.NormalImageIndex = GroupImageIndex;
				zNode.ErrorImageIndex = invoiceGroupErrorImageIndex;
				zNode.MessageErrorImageIndex = invoiceGroupMessageErrorImageIndex;
				zNode.WarningImageIndex = invoiceGroupWarningImageIndex;
			}
		}

		#endregion

		#region Adding and Removing Nodes

		void TreeView_OnSelectedNodeChanged(bool isGroupInvoiceSelected)
		{
			if (CurrentDataItem != null)
			{
				UpdateActiveGroup();
			}
		}

		/// <summary>
		/// 1. Update image of icons of nodes
		/// 2. Update ActiveGroupHeader
		/// </summary>
		void UpdateActiveGroup()
		{
			JobComInvoiceHeaderTreeNode selectedNode = TreeView.SelectedNode as JobComInvoiceHeaderTreeNode;
			IGroupInvoiceOrInvoice selectedInvoice = (IGroupInvoiceOrInvoice)selectedNode.BizO;

			if (selectedInvoice != CurrentDataItem.ActiveGroupHeader[0])
			{
				ResetTreeNodeImages(CurrentDataItem.ActiveGroupHeader[0]);
				CurrentDataItem.ActiveGroupHeader.SwapGroup(selectedInvoice.GroupInvoiceOfInvoiceOrGroupInvoiceItself);
				SetSelectedGroupTreeNodeImages(TreeView.SelectedNode);
			}
		}

		void AddInvoiceGroups(TreeNode node, BaseJobComInvoiceGroupHeader group)
		{
			foreach (BaseJobComInvoiceGroupHeader subGroup in group.JobComInvoiceGroupHeaders)
			{
				TreeNode newNode = new JobComInvoiceHeaderTreeNode(subGroup, subGroup.JZ_InvoiceNumber, GroupImageIndex, invoiceGroupErrorImageIndex, invoiceGroupMessageErrorImageIndex, invoiceGroupWarningImageIndex);
				node.Nodes.Add(newNode);
				AddInvoiceGroups(newNode, subGroup);
			}
			AddInvoiceHeaders(node, group.JobComInvoiceHeaders);
		}

		void AddInvoiceHeaders(TreeNode node, InvoiceHeaderActiveCollection invoices)
		{
			foreach (BaseJobComInvoiceHeader header in invoices)
			{
				TreeNode newNode = new JobComInvoiceHeaderTreeNode(header, header.JZ_InvoiceNumber, InvoiceImageIndex, invoiceErrorImageIndex, invoiceMessageErrorImageIndex, invoiceWarningImageIndex);
				node.Nodes.Add(newNode);
				newNode.EnsureVisible();
			}
		}

		internal void InsertTreeNode(BaseJobComInvoiceGroupHeader groupInvoiceToInsertAt, IGroupInvoiceOrInvoice bizOToInsert)
		{
			ZBusinessObjectTreeNode groupNodeToInsertAt = TreeView.AllInvoicesNode.FindNodeForObject(groupInvoiceToInsertAt);
			if (groupNodeToInsertAt != null)
			{
				TreeNode newNode = groupNodeToInsertAt.FindNodeForObject((BusinessObject)bizOToInsert);
				if (newNode == null || newNode.Parent != groupNodeToInsertAt)
				{
					ZString initialText = bizOToInsert.JZ_InvoiceNumber;
					if (initialText.IsEmpty && bizOToInsert.IsGroupInvoice)
					{
						initialText = CreateUniqueNodeName((NoResString)"New Group 1");
						bizOToInsert.JZ_InvoiceNumberInfo.Value = initialText;
					}

					if (newNode == null)
					{
						newNode = new JobComInvoiceHeaderTreeNode(bizOToInsert, initialText, InvoiceImageIndex, invoiceErrorImageIndex, invoiceMessageErrorImageIndex, invoiceWarningImageIndex);
					}
					groupNodeToInsertAt.Nodes.Add(newNode);
					newNode.EnsureVisible();
				}

				if (bizOToInsert.IsGroupInvoice)
				{
					foreach (BaseJobComInvoiceGroupHeader subGroup in bizOToInsert.ChildGroupInvoices)
					{
						InsertTreeNode((BaseJobComInvoiceGroupHeader)bizOToInsert, subGroup);
					}
					foreach (BaseJobComInvoiceHeader anInvoice in bizOToInsert.ChildInvoices)
					{
						InsertTreeNode((BaseJobComInvoiceGroupHeader)bizOToInsert, anInvoice);
					}
				}

				TreeView.SelectedNode = (JobComInvoiceHeaderTreeNode)newNode;
			}
		}

		void RemoveTreeNode(BaseJobComInvoiceGroupHeader group, IGroupInvoiceOrInvoice invoiceOrGroup)
		{
			ZBusinessObjectTreeNode parentGroupNode = TreeView.AllInvoicesNode.FindNodeForObject(group);
			if (parentGroupNode != null)
			{
				TreeNode invoiceNode = parentGroupNode.FindNodeForObject((BusinessObject)invoiceOrGroup);
				if (invoiceNode != null && parentGroupNode == invoiceNode.Parent)
				{
					parentGroupNode.Nodes.Remove(invoiceNode);
				}

				TreeView.SelectedNode = (JobComInvoiceHeaderTreeNode)parentGroupNode;
			}
		}

		void DeleteSelectedTreeNode()
		{
			IGroupInvoiceOrInvoice invoice = (IGroupInvoiceOrInvoice)((ZBusinessObjectTreeNode)TreeView.SelectedNode).BizO;
			if (invoice == TreeView.AllInvoicesNode.BizO)
			{
				Globals.Message.Show(Res.GetString("fe20ae30-f258-411a-b1cc-d7efe3c610e8", "The 'All Invoices' node cannot be deleted - Remove each invoice group individually"));
			}
			else
			{
				string deleteMessage = Res.GetString("26b2b9c1-3e79-4665-9ad1-0c48661281e8", "Delete group {0} and all Invoices under the group invoice", invoice.JZ_InvoiceNumber);
				if (Globals.Message.Show(deleteMessage, Res.GetString("5aa3a76b-6772-480f-bf50-0ac40483c06f", "Delete Group?"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					RemoveTreeNode(invoice.ParentGroupInvoice, invoice);
					((BusinessObject)invoice).Delete();
				}
			}
		}

		#endregion

		#region NodeName Management
		bool NodeNameUsed(TreeNode current, string textToCheck)
		{
			bool result = false;
			if (current.Text == textToCheck)
			{
				result = true;
			}
			if (!result)
			{
				foreach (TreeNode aNode in current.Nodes)
				{
					result |= (NodeNameUsed(aNode, textToCheck));
				}
			}
			return result;
		}

		string CreateUniqueNodeName(string text)
		{
			string result = text;
			int loopCount = 2;
			while (NodeNameUsed(TreeView.Nodes[0], result))
			{
				result = text + " " + loopCount.ToString();
				loopCount++;
			}
			return result;
		}
		#endregion

		#region Context Menu Management

		void AddGroupMenuItem_Click(object sender, EventArgs e)
		{
			BaseJobComInvoiceGroupHeader newGroup = CurrentDataItem.ActiveGroupHeader[0].JobComInvoiceGroupHeaders.AddNew();
			InsertTreeNode(CurrentDataItem.ActiveGroupHeader[0], newGroup);
		}

		void DeleteMenuItem_Click(object sender, EventArgs e)
		{
			DeleteSelectedTreeNode();
		}

		void TreeViewContextMenu_Popup(object sender, EventArgs e)
		{
			this.AddGroupMenuItem.Enabled = !CurrentDataItem.ReadOnly;
			this.DeleteMenuItem.Enabled = !CurrentDataItem.ReadOnly;
		}

		#endregion

		#region Notification Change

		void JobDeclaration_NotificationsChanged(object sender, EventArgs e)
		{
			UpdateTreeNodesNotifications();
		}

#if DEBUG
		protected virtual
#endif
 void UpdateTreeNodesNotifications()
		{
			if (!IsDisposed && TreeView.Nodes.Count > 0)
			{
				ZBusinessObjectTreeNode topNode = TreeView.Nodes[0] as ZBusinessObjectTreeNode;
				RecursivelyUpdateTreeNode(topNode);
			}
		}

		#endregion
	}
}
