using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class CodeDescriptionBoolTreeView : NonPersistentBusinessObjectCollectionView<CodeDescriptionBoolTreeNode>, ICodeDescriptionBoolList
	{
		public CodeDescriptionBoolTreeView(CodeDescriptionBoolTreeNodeCollection allNodes, ZGuid parentID)
			: base(allNodes)
		{
			this.parentID = parentID;

			Rebuild();
		}

		public CodeDescriptionBoolTreeView(CodeDescriptionBoolTreeNodeCollection allNodes, CodeDescriptionBoolTreeNode parent)
			: this(allNodes, parent != null ? parent.ID : ZGuid.Empty)
		{
		}

		protected override BusinessObject AddNewCore()
		{
			var node = base.AddNewCore() as CodeDescriptionBoolTreeNode;
			node.ParentID = ParentID;
			var allNodes = CollectionToFilter as CodeDescriptionBoolTreeNodeCollection;
			allNodes.AddSystemChildren(node);
			return node;
		}

		public ZGuid ParentID
		{
			get { return parentID; }
			set
			{
				if (this.parentID != value)
				{
					parentID = value;
					Rebuild();
					RefreshBinding();
				}
			}
		}
		ZGuid parentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var node = (CodeDescriptionBoolTreeNode)child;
			node.ParentID = ParentID;
			var allNodes = CollectionToFilter as CodeDescriptionBoolTreeNodeCollection;
			if (allNodes != null && allNodes.CodeLists != null)
			{
				node.CodeList = allNodes.CodeLists.ElementAt(allNodes.GetDepth(node) - 1);
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var node = (CodeDescriptionBoolTreeNode)element;
			return node.ParentID == ParentID;
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (!Contains(elementToDelete) && CollectionToFilter.Contains(elementToDelete))
			{
				CollectionToFilter.RemoveAndDelete(elementToDelete);
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		protected override CodeDescriptionBoolTreeNode CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionBoolTreeNode();
		}

		protected override void RebuildOnConstruction()
		{
			// don't rebuild yet as we have not set the ParentID
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			bool addedToViewFirst = !CollectionToFilter.Contains(bizOAdded);
			base.OnAdded(bizOAdded);
			var bizOAddedTreeNode = (CodeDescriptionBoolTreeNode)bizOAdded;

			if (addedToViewFirst)
			{
				var allNodes = (CodeDescriptionBoolTreeNodeCollection)CollectionToFilter;
				allNodes.AddSystemChildren(bizOAddedTreeNode);
			}
			else if (bizOAddedTreeNode.SystemDefined)
			{
				RefreshBinding();
			}
		}

		public bool GetBoolFromCode(string code)
		{
			var item = FindByCode(code);
			return item != null && item.Bool;
		}

		ICodeDescriptionBool ICodeDescriptionBoolList.this[int i]
		{
			get { return this[i]; }
		}

		public CodeDescriptionBoolTreeNode FindByCode(string code)
		{
			return this.Cast<CodeDescriptionBoolTreeNode>().FirstOrDefault(element => string.Compare(element.Code, code, StringComparison.Ordinal) == 0);
		}

		public bool ContainsCode(object code)
		{
			return FindByCode(code != null ? code.ToString() : null) != null;
		}

		public string GetDescriptionFromCode(string code)
		{
			var item = FindByCode(code);
			return item != null ? item.Description : string.Empty;
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return new CodeDescriptionBoolTreeNodeComparer(property, direction);
		}

		class CodeDescriptionBoolTreeNodeComparer : PropertyComparer
		{
			public CodeDescriptionBoolTreeNodeComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
				: base(propertyDescriptor, direction)
			{ }

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				var treeNodeX = x as CodeDescriptionBoolTreeNode;
				var treeNodeY = y as CodeDescriptionBoolTreeNode;

				if (treeNodeX.IsSystemAll != treeNodeY.IsSystemAll)
				{
					return treeNodeX.IsSystemAll ? -1 : 1;
				}
				else
				{
					return base.Compare(x, y);
				}
			}
		}
	}
}
