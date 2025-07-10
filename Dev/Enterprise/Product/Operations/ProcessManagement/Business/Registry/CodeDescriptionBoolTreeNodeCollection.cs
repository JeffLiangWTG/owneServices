using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class CodeDescriptionBoolTreeNodeCollection : CodeDescriptionBoolCollection
	{
		public CodeDescriptionBoolTreeNodeCollection()
			: this(true, 3, 4)
		{
		}

		public CodeDescriptionBoolTreeNodeCollection(bool defaultBoolForNewChild, int codeMaxLength = 3, int maxDepth = 4, MultilingualString[] allDescriptions = null, CodeDescriptionPairList[] codeLists = null)
			: base(null, defaultBoolForNewChild, codeMaxLength)
		{
			if (allDescriptions != null && allDescriptions.Length != maxDepth - 1)
			{
				throw new ArgumentException("the number of descriptions must equal maxDepth - 1", nameof(allDescriptions));
			}
			if (codeLists != null && codeLists.Length != maxDepth)
			{
				throw new ArgumentException("the number of code lists must equal maxDepth", nameof(codeLists));
			}
			this.MaxDepth = maxDepth;
			this.AllDescriptions = allDescriptions;
			this.CodeLists = codeLists;
		}

		protected CodeDescriptionBoolTreeNodeCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public int MaxDepth { get; set; }

		public IEnumerable<MultilingualString> AllDescriptions { get; set; }
		public IEnumerable<CodeDescriptionPairList> CodeLists { get; set; }

		public new CodeDescriptionBoolTreeNode this[int i]
		{
			get { return (CodeDescriptionBoolTreeNode)base[i]; }
		}

		public CodeDescriptionPairList GetParents(bool activeOnly)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (CodeDescriptionBoolTreeNode item in this)
			{
				if (item.ParentID.IsEmpty && item.Code != CodeDescriptionBoolTreeNode.AllCode &&
					(!activeOnly || item.Bool))
				{
					result.Add(item);
				}
			}
			result.SortByDescription();
			return result;
		}

		public CodeDescriptionBoolTreeNode FindParent(string parentCode)
		{
			CodeDescriptionBoolTreeNode result = null;
			foreach (CodeDescriptionBoolTreeNode item in this)
			{
				if (item.ParentID.IsEmpty && item.Code.EqualsIgnoringCase(parentCode))
				{
					result = item;
					break;
				}
			}

			return result;
		}

		public CodeDescriptionBoolTreeNode Find(params string[] codes)
		{
			foreach (CodeDescriptionBoolTreeNode item in this)
			{
				CodeDescriptionBoolTreeNode node = item;
				for (int depth = codes.Length; depth > 0 && node.Code == codes[depth - 1]; --depth)
				{
					if (node.ParentID.IsEmpty)
					{
						if (depth == 1)
						{
							return item;
						}
						else
						{
							break;
						}
					}
					else if (depth > 1)
					{
						node = (CodeDescriptionBoolTreeNode)FindByPK(node.ParentID);
					}
				}
			}
			return null;
		}

		public CodeDescriptionPairList GetChildren(string parentCode, bool activeOnly)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (CodeDescriptionBoolTreeNode item in this)
			{
				if (!item.ParentID.IsEmpty && item.Code != CodeDescriptionBoolTreeNode.AllCode && (!activeOnly || item.Bool))
				{
					var parent = (CodeDescriptionBoolTreeNode)FindByPK(item.ParentID);
					if (parent.ParentID.IsEmpty &&
						(parent.Code == parentCode || parent.Code == CodeDescriptionBoolTreeNode.AllCode || string.IsNullOrEmpty(parentCode)))
					{
						result.Add(item);
					}
				}
			}
			result.SortByDescriptionAndCombineIfSameCode();
			return result;
		}

		public CodeDescriptionPairList GetChildren(string depth1Code, string depth2Code, bool activeOnly)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (CodeDescriptionBoolTreeNode item in this)
			{
				if (!item.ParentID.IsEmpty && item.Code != CodeDescriptionBoolTreeNode.AllCode && (!activeOnly || item.Bool))
				{
					var parentA = (CodeDescriptionBoolTreeNode)FindByPK(item.ParentID);
					if (!parentA.ParentID.IsEmpty)
					{
						var parentB = (CodeDescriptionBoolTreeNode)FindByPK(parentA.ParentID);
						if (parentB.ParentID.IsEmpty &&
							(parentB.Code == depth1Code || parentB.Code == CodeDescriptionBoolTreeNode.AllCode || string.IsNullOrEmpty(depth1Code)) &&
							(parentA.Code == depth2Code || parentA.Code == CodeDescriptionBoolTreeNode.AllCode || string.IsNullOrEmpty(depth2Code)))
						{
							result.Add(item);
						}
					}
				}
			}
			result.SortByDescriptionAndCombineIfSameCode();
			return result;
		}

		public CodeDescriptionPairList GetChildren(bool activeOnly, params string[] codes)
		{
			return GetChildrenCore(false, activeOnly, codes);
		}

		public CodeDescriptionPairList GetChildrenExactMatchOnly(bool activeOnly, params string[] codes)
		{
			return GetChildrenCore(true, activeOnly, codes);
		}

		CodeDescriptionPairList GetChildrenCore(bool exactMatchOnly, bool activeOnly, params string[] codes)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			if (codes.Length != 0)
			{
				foreach (CodeDescriptionBoolTreeNode item in this)
				{
					if (!item.ParentID.IsEmpty && item.Code != CodeDescriptionBoolTreeNode.AllCode && (!activeOnly || item.Bool))
					{
						var parent = (CodeDescriptionBoolTreeNode)FindByPK(item.ParentID);
						for (int depth = codes.Length; depth > 0; --depth)
						{
							string code = codes[depth - 1];
							if (parent.Code == code || (!exactMatchOnly && (parent.Code == CodeDescriptionBoolTreeNode.AllCode || string.IsNullOrEmpty(code))))
							{
								if (parent.ParentID.IsEmpty)
								{
									if (depth == 1)
									{
										result.Add(item);
									}
									else
									{
										break;
									}
								}
								else
								{
									parent = (CodeDescriptionBoolTreeNode)FindByPK(parent.ParentID);
								}
							}
							else
							{
								break;
							}
						}
					}
				}
			}
			result.SortByDescriptionAndCombineIfSameCode();
			return result;
		}

		public CodeDescriptionBoolTreeNode Add(ZString code, MultilingualString desc, bool boolValue, bool isSystemDefined, CodeDescriptionBoolTreeNode parent)
		{
			var result = (CodeDescriptionBoolTreeNode)CreateNonPersistentBusinessObject();
			result.SystemDefined = isSystemDefined;
			result.Code = code;
			result.Description = desc;
			result.Bool = boolValue;
			result.ParentID = parent != null ? parent.ID : ZGuid.Empty;
			if (CodeLists != null)
			{
				result.CodeList = CodeLists.ElementAt(GetDepth(result) - 1);
			}
			Add(result);
			return result;
		}

		public new CodeDescriptionBoolTreeNode Add(ZString code, MultilingualString desc)
		{
			return Add(code, desc, true, false, null);
		}

		public CodeDescriptionBoolTreeNode Add(ZString code, MultilingualString desc, CodeDescriptionBoolTreeNode parent)
		{
			return Add(code, desc, true, false, parent);
		}

		public CodeDescriptionBoolTreeNode Add(ZString code, CodeDescriptionBoolTreeNode parent)
		{
			return Add(code, null, true, false, parent);
		}

		public new CodeDescriptionBoolTreeNode Add(ZString code)
		{
			return Add(code, null, true, true, null);
		}

		public new CodeDescriptionBoolTreeNode AddNew()
		{
			return (CodeDescriptionBoolTreeNode)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionBoolTreeNode();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = (CodeDescriptionBoolTreeNodeCollection)base.GetClone(fallbackLevel, factory);
			clone.MaxDepth = this.MaxDepth;
			clone.AllDescriptions = AllDescriptions;
			clone.CodeLists = CodeLists;
			return clone;
		}

		/// <summary>
		/// Called by base class GetClone method.
		/// </summary>
		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CodeDescriptionBoolTreeNodeCollection(CurrentFallbackLevel);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			var removed = (CodeDescriptionBoolTreeNode)bizO;
			List<CodeDescriptionBoolTreeNode> children = new List<CodeDescriptionBoolTreeNode>();

			foreach (CodeDescriptionBoolTreeNode item in this)
			{
				if (item.ParentID == removed.ID)
				{
					children.Add(item);
				}
			}

			foreach (var item in children)
			{
				if (removed.IsDeleted)
				{
					RemoveAndDelete(item);
				}
				else
				{
					Remove(item);
				}
			}
		}

		/// <summary>
		/// Get the depth of a node. Parents have depth 1. Their children have depth 2, etc.
		/// </summary>
		public int GetDepth(CodeDescriptionBoolTreeNode node)
		{
			int result = 0;
			while (node != null)
			{
				if (!node.ParentID.IsEmpty)
				{
					node = (CodeDescriptionBoolTreeNode)FindByPK(node.ParentID);
				}
				else
				{
					node = null;
				}
				++result;
			}

			return result;
		}

		/// <summary>
		/// Add the system defined "ALL" node for the children of the given parent.
		/// </summary>
		public void AddSystemChildren(CodeDescriptionBoolTreeNode parent = null)
		{
			int depth = GetDepth(parent);
			var allDescriptions = AllDescriptions != null ? AllDescriptions.ToArray() : null;

			while (++depth < MaxDepth)
			{
				var description = AllDescriptions != null ? allDescriptions[depth - 1] : DefaultAllDescription;
				parent = Add(CodeDescriptionBoolTreeNode.AllCode, description, true, true, parent);
			}
		}

		static MultilingualString DefaultAllDescription
		{
			get { return ResString.GetMultilingualString("C3F76166-D9BD-422D-8719-C3615F0BDF6C", "* All *"); }
		}
	}
}
