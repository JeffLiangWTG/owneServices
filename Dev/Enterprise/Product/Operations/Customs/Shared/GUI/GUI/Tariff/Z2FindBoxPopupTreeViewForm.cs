using System;
using System.Collections.Specialized;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GUI
{
	[TestExcludeZWinFormsAllHaveFormBashers]
	public partial class Z2FindBoxPopupTreeViewForm : ZChildForm, IFindBoxPopup
	{
		public Z2FindBoxPopupTreeViewForm()
		{
		}

		protected Z2FindBoxPopupTreeViewForm(BusinessObjectCollection collection) : base(collection)
		{
			treeView.BeforeExpand += new TreeViewCancelEventHandler(treeView_BeforeExpand);
			treeView.AfterSelect += new TreeViewEventHandler(treeView_AfterSelect);
		}

		BusinessObjectFactory fFactory;
		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
					((IBusinessObjectFactoryInternals)fFactory).ReadOnly = true;
				}
				return fFactory;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public virtual TreeView treeView
		{
			get { return null; }
		}

		protected virtual BusinessObjectCollection GetTopLevelCollection(BusinessObjectFactory factory)
		{
			return null;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadTopLevelCollection();
		}

		void AddBusinessObjects(IFamilyMember bizObj, TreeNodeCollection nodes)
		{
			foreach (IFamilyMember child in bizObj.Children)
			{
				AddFamilyMember(child, nodes);
			}
		}

		bool fAllowDuplicateNode;
		public bool AllowDuplicateNode
		{
			get { return fAllowDuplicateNode; }
			set { fAllowDuplicateNode = value; }
		}

		StringCollection nodesHashTable;
		protected internal void AddFamilyMember(IFamilyMember bizObj, TreeNodeCollection nodes)
		{
			if (!AllowDuplicateNode)
			{
				if (nodesHashTable != null)
				{
					if (!nodesHashTable.Contains(bizObj.ShortDescription))
					{
						AddMember(bizObj, nodes);
					}
				}
				else
				{
					AddMember(bizObj, nodes);
				}
			}
			else
			{
				AddMember(bizObj, nodes);
			}
		}

		void AddMember(IFamilyMember bizObj, TreeNodeCollection nodes)
		{
			TreeNode node = nodes.Add(bizObj.ShortDescription);
			node.Tag = bizObj;
			if (nodesHashTable == null)
			{
				nodesHashTable = new StringCollection();
			}
			nodesHashTable.Add(bizObj.ShortDescription);
			if (bizObj.HasChildren)
			{
				node.Nodes.Add(PleaseWaitLoading);
			}
		}

		protected static string PleaseWaitLoading
		{
			get { return Res.GetString("64df5d85-e75c-410a-866f-e3443373fc1f", "Please wait - Loading"); }
		}

		protected BusinessObject SelectedElement
		{
			get { return treeView.SelectedNode == null ? null : (BusinessObject)treeView.SelectedNode.Tag; }
		}

		bool topLevelCollectionLoaded;
		public void LoadTopLevelCollection()
		{
			if (!this.IsDesignMode() && !topLevelCollectionLoaded)
			{
				topLevelCollectionLoaded = true;
				foreach (IFamilyMember familyMember in GetTopLevelCollection(Factory))
				{
					AddFamilyMember(familyMember, treeView.Nodes);
				}
			}
		}

		protected void ShowContentsOfNode(TreeNode node)
		{
			if (node.Nodes.Count >= 1)
			{
				if (node.Nodes[0].Text == PleaseWaitLoading)
				{
					if (node.Nodes.Count > 1)
					{
						ErrorReporter.ReportOnce("BadNodeCount", "Incorrect number of nodes - expected 1 but was " + node.Nodes.Count.ToString());
					}
					node.Nodes[0].Remove();
					AddBusinessObjects((IFamilyMember)node.Tag, node.Nodes);
				}
			}
		}

		protected virtual void NavigateToCode(string code)
		{
			throw new Exception("Pure Virtual (Abstract) call");
		}

		#region IFindBoxPopup Members

		protected IFindBox findBox;

		void IFindBoxPopup.SelectRowByPK(ZGuid pK)
		{
			// not necessary as can't add a new record to the TreeView
		}

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			this.findBox = findBox;
			NavigateToCode(findBox.Code);

			ZFormModaliser.Show(this, parentForm);
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		protected virtual void AcceptSelection()
		{
			DialogResult = DialogResult.OK;
			if (SelectedElement != null)
			{
				findBox.Code = CodePropertyAttribute.CodeFromBusinessObject(SelectedElement);
				findBox.Description = DescriptionPropertyAttribute.DescriptionFromBusinessObject(SelectedElement);
			}
			Hide();
		}

		protected virtual void CancelSelection()
		{
			DialogResult = DialogResult.Cancel;
			Hide();
		}

		#endregion

		void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			ShowContentsOfNode(e.Node);
		}

		protected virtual void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			BusinessObjectCollection collection = ((BusinessObjectCollection)BusinessEntity);
			collection.RemoveAll();
			collection.Add((BusinessObject)e.Node.Tag);
		}
	}
}
