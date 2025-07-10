using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.ZArchitecture.Business
{
	public class HasChangesHunterExclusionDetails
	{
		public HasChangesHunterExclusionDetails(Type typeToExclude) : this(typeToExclude, true)
		{
		}

		/// <param name="typeToExclude">Type to exclude for a particular HasChanges</param>
		/// <param name="shouldIgnoreAllChildren">If true, then it exclude all children from this HasChanges. If false, then it considers its children for this HasChanges</param>
		public HasChangesHunterExclusionDetails(Type typeToExclude, bool shouldIgnoreAllChildren)
		{
			this.TypeToExclude = typeToExclude;
			this.ShouldIgnoreAllChildren = shouldIgnoreAllChildren;
		}

		public readonly Type TypeToExclude;
		public readonly bool ShouldIgnoreAllChildren;
	}

	public class HasChangesHunter
	{
		public HasChangesHunter(IBusiness businessEntity)
			: this(businessEntity, null)
		{
		}

		public HasChangesHunter(IBusiness businessEntity, params HasChangesHunterExclusionDetails[] typesToExclude)
		{
			this.businessEntity = businessEntity;
			this.typesToExclude = typesToExclude ?? (Array.Empty<HasChangesHunterExclusionDetails>());
			includedPrefixes = new ArrayList();
		}

		public bool HasChanges
		{
			get { return RunRecurser(new HasChangesRecurser(typesToExclude, IncludedPrefixes)); }
		}

		public bool HasChangesSinceLastMark
		{
			get { return RunRecurser(new HasChangesSinceLastMarkRecurser(markedChangeNumber, typesToExclude, IncludedPrefixes)); }
		}

		public bool BusinessObjectHasChangesSinceLastMark(IBusiness bizo)
		{
			return bizo != null && bizo.HasChangesNotIncludingChildren && bizo.LastChangeNumber > markedChangeNumber;
		}

		bool RunRecurser(HasChangesRecurser recurser)
		{
			bool result = recurser.HasChanges(businessEntity);
			lastEntityFoundWithChanges = recurser.LastEntityFoundWithChanges;
			return result;
		}

		public IBusiness LastEntityFoundWithChanges
		{
			get { return lastEntityFoundWithChanges; }
		}
		IBusiness lastEntityFoundWithChanges;

		public void Mark()
		{
			markedChangeNumber = businessEntity.Factory.LastChangeNumber;
		}
		uint markedChangeNumber;

		/// <summary>
		/// Included Prefixes contains a list of all included namespaces.
		/// This list is only active if one or more items are added.
		/// </summary>
		public string[] IncludedPrefixes
		{
			get { return (string[])includedPrefixes.ToArray(typeof(string)); }
		}
		readonly ArrayList includedPrefixes;

		public void AddIncludedNamespacePrefix(string namespacePrefix)
		{
			includedPrefixes.Add(namespacePrefix);
		}

#if DEBUG
		internal
#endif
		class HasChangesRecurser
		{
			public HasChangesRecurser(HasChangesHunterExclusionDetails[] typesToExclude, string[] namespacesToInclude)
			{
				this.typesToExclude = typesToExclude;
				this.namespacesToInclude = namespacesToInclude;
			}

			public IBusiness LastEntityFoundWithChanges
			{
				get { return lastEntityFoundWithChanges; }
			}
			IBusiness lastEntityFoundWithChanges;

			public bool HasChanges(IBusiness top)
			{
				bool result = false;
				if (!IsExcludedType(top))
				{
					if (
#if DEBUG
top.GetType().Namespace == null || // for dynamic mocks
#endif
						IsIncludedNamespace(top.GetType())
						)
					{
						result = HasChangesNotIncludingChildren(top);
						if (result)
						{
							lastEntityFoundWithChanges = top;
						}
					}
				}

				if (!result && ShouldCheckItsChildren(top.GetType()))
				{
					result = HasChanges(top.Children);
				}

				return result;
			}

			bool HasChanges(IBusiness[] children)
			{
				foreach (IBusiness child in children)
				{
					if (HasChanges(child))
					{
						return true;
					}
				}
				return false;
			}

			protected virtual bool HasChangesNotIncludingChildren(IBusiness businessEntity)
			{
				return businessEntity.HasChangesNotIncludingChildren;
			}

#if DEBUG
			internal
#endif
			bool IsIncludedNamespace(Type objectType)
			{
				bool result = true;
				if (namespacesToInclude.Length > 0)
				{
					result = false;
					foreach (string includedNamespace in namespacesToInclude)
					{
						if (IsIncludedNamespace(objectType, includedNamespace))
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}

			bool IsIncludedNamespace(Type objectType, string includedNamespace)
			{
				bool result = false;
				if (objectType.Namespace.StartsWith(includedNamespace))
				{
					result = true;
				}
				else if (objectType != typeof(object))
				{
					result = IsIncludedNamespace(objectType.BaseType, includedNamespace);
				}
				return result;
			}

			bool IsExcludedType(IBusiness bizObj)
			{
				var objectType = GetObjectToCheckForExclusion(bizObj)?.GetType();
				if (objectType == null)
				{
					return true;
				}
				else
				{
					if (!ShouldExcludeCache.TryGetValue(objectType, out bool result))
					{
						result = IsExcludedTypeCore(objectType);
						ShouldExcludeCache.Add(objectType, result);
					}
					return result;
				}
			}

			bool IsExcludedTypeCore(Type objectType)
			{
				return typesToExclude.Any(exclusionDetail => exclusionDetail.TypeToExclude == objectType || objectType.IsSubclassOf(exclusionDetail.TypeToExclude));
			}

			Dictionary<Type, bool> ShouldExcludeCache => shouldExcludeCache ?? (shouldExcludeCache = new Dictionary<Type, bool>());
			Dictionary<Type, bool> shouldExcludeCache;

			bool ShouldCheckItsChildren(Type typeToTest)
			{
				if (!ShouldCheckItsChildrenCache.TryGetValue(typeToTest, out bool result))
				{
					result = ShouldCheckItsChildrenCore(typeToTest);
					shouldCheckItsChildrenCache.Add(typeToTest, result);
				}
				return result;
			}

			bool ShouldCheckItsChildrenCore(Type typeToTest)
			{
				foreach (HasChangesHunterExclusionDetails detail in typesToExclude)
				{
					if (detail.TypeToExclude == typeToTest || typeToTest.IsSubclassOf(detail.TypeToExclude))
					{
						return !detail.ShouldIgnoreAllChildren;
					}
				}
				return true;
			}

			Dictionary<Type, bool> ShouldCheckItsChildrenCache => shouldCheckItsChildrenCache ?? (shouldCheckItsChildrenCache = new Dictionary<Type, bool>());
			Dictionary<Type, bool> shouldCheckItsChildrenCache;

			IBusiness GetObjectToCheckForExclusion(IBusiness bizObj)
			{
				if (bizObj is GenAddOnColumn genAddOnColumn)
				{
					return !genAddOnColumn.IsDeleted ? genAddOnColumn.Parent ?? bizObj : null;
				}
				return bizObj;
			}

			readonly HasChangesHunterExclusionDetails[] typesToExclude;
			readonly string[] namespacesToInclude;
		}

		class HasChangesSinceLastMarkRecurser : HasChangesRecurser
		{
			public HasChangesSinceLastMarkRecurser(uint lastMark, HasChangesHunterExclusionDetails[] typesToExclude, string[] namespacesToInclude)
				: base(typesToExclude, namespacesToInclude)
			{
				this.lastMark = lastMark;
			}
			protected uint lastMark;

			protected override bool HasChangesNotIncludingChildren(IBusiness businessEntity)
			{
				return base.HasChangesNotIncludingChildren(businessEntity) && businessEntity.LastChangeNumber > lastMark;
			}
		}

		protected IBusiness businessEntity;
		protected HasChangesHunterExclusionDetails[] typesToExclude;
	}
}
