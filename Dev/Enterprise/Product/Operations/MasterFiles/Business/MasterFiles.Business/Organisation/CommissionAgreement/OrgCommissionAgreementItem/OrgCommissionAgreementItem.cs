using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementItem : AutoOrgCommissionAgreementItem, ICommissionAgreementRelated<OrgCommissionAgreementItem>
	{
		public const string AllItemCode = "ALL";

		public OrgCommissionAgreementItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region CAI_Type

		public ZString CAI_TypeDescription
		{
			get { return new OrgCommissionAgreementItemTypes().GetDescriptionFromCode(base.CAI_Type); }
		}

		#endregion

		#region CAI_ParentID

		public OrgCommissionAgreement CommissionAgreement
		{
			get
			{
				if (CAI_ParentTableCode == OrgCommissionAgreementSchema.Constants.Prefix)
				{
					return Factory.Load<OrgCommissionAgreement>(CAI_ParentID);
				}

				var parentItem = ParentItem;
				return parentItem != null ? parentItem.CommissionAgreement : null;
			}
		}

		internal OrgCommissionAgreementItem ParentItem
		{
			get
			{
				if (CAI_ParentTableCode == OrgCommissionAgreementItemSchema.Constants.Prefix)
				{
					return Factory.Load<OrgCommissionAgreementItem>(CAI_ParentID);
				}

				return null;
			}
		}

		#endregion

		#region CAI_IsInclude

		public override ZBool CAI_IsInclude
		{
			get { return base.CAI_IsInclude; }
			set
			{
				var previousValue = base.CAI_IsInclude;
				base.CAI_IsInclude = value;

				if (previousValue != value)
				{
					ResetChildItems();
					RefreshReadOnlyForChildItems();
					var commissionAgreement = CommissionAgreement;
					if (commissionAgreement != null)
					{
						commissionAgreement.NotifyAgreementItemsChanged();
					}
				}
			}
		}

		#endregion

		#region CAI_Code

		[List("Lookups.Codes")]
		public override ZString CAI_Code
		{
			get { return base.CAI_Code; }
			set
			{
				var previousValue = base.CAI_Code;
				base.CAI_Code = value;
				if (value == AllItemCode)
				{
					CAI_IsInclude = true;
				}

				if (previousValue != value)
				{
					ResetChildItems();

					var commissionAgreement = CommissionAgreement;
					if (commissionAgreement != null)
					{
						commissionAgreement.NotifyAgreementItemsChanged();
					}

					if (!CanHaveConditions && ConditionCollection.Count > 0)
					{
						ConditionCollection.DeleteAll();
					}

					if (CanHaveConditions && ConditionCollection.Count == 0)
					{
						ConditionCollection.AddNew();
					}
				}
			}
		}

		#endregion

		#region IsAllItem

		public ZBool IsAllItem
		{
			get { return CAI_Code == AllItemCode; }
		}

		#endregion

		#endregion

		#region Draft

		public OrgCommissionAgreementItem ParentVersion
		{
			get
			{
				if (!parentVersionInitialized)
				{
					parentVersionInitialized = true;
					var parentVersionPk = DraftCommissionAgreementLogs.GetParentVersionPk(this);
					if (!parentVersionPk.IsEmpty)
					{
						parentVersion = Factory.Load<OrgCommissionAgreementItem>(parentVersionPk);
					}
				}

				return parentVersion;
			}
			private set
			{
				parentVersionInitialized = true;
				parentVersion = value;
				DraftCommissionAgreementLogs.AddDraftLog(this, value);
			}
		}
		OrgCommissionAgreementItem parentVersion;
		bool parentVersionInitialized;

		internal OrgCommissionAgreementItem CreateDraft()
		{
			var draft = Factory.New<OrgCommissionAgreementItem>();
			draft.CopyPersistentValuesFrom(this, GetDraftCloneArgs());
			draft.ParentVersion = this;

			foreach (var childServiceItem in ChildServiceItems.ToArray())
			{
				draft.ChildServiceItems.Add(childServiceItem.CreateDraft());
			}

			foreach (var childSubModuleItem in ChildSubModuleItems.ToArray())
			{
				draft.ChildSubModuleItems.Add(childSubModuleItem.CreateDraft());
			}

			foreach (var condition in ConditionCollection)
			{
				draft.ConditionCollection.Add(condition.CreateDraft());
			}

			return draft;
		}

		internal OrgCommissionAgreementItem MergeDraft()
		{
			try
			{
				isMerging = true;

				var draft = this;
				var parentVersion = ParentVersion;
				if (parentVersion != null)
				{
					parentVersion.CopyPersistentValuesFrom(this, GetDraftCloneArgs());

					parentVersion.ConditionCollection.RefreshFromDb();
					parentVersion.ConditionCollection.DeleteAll();

					foreach (var condition in ConditionCollection.ToArray())
					{
						condition.CIC_CAI = parentVersion.PK;
					}

					var parentVersionChildServiceItemsNotInDraft = new HashSet<OrgCommissionAgreementItem>(parentVersion.ChildServiceItems);
					foreach (var draftChildServiceItem in draft.ChildServiceItems.ToArray())
					{
						parentVersionChildServiceItemsNotInDraft.Remove(draftChildServiceItem.MergeDraft());
					}
					foreach (var parentVersionChildServiceItem in parentVersionChildServiceItemsNotInDraft)
					{
						parentVersionChildServiceItem.Delete();
					}

					var parentVersionChildSubModuleItemsNotInDraft = new HashSet<OrgCommissionAgreementItem>(parentVersion.ChildSubModuleItems);
					foreach (var draftChildSubModuleItem in draft.ChildSubModuleItems.ToArray())
					{
						parentVersionChildSubModuleItemsNotInDraft.Remove(draftChildSubModuleItem.MergeDraft());
					}
					foreach (var parentVersionChildSubModuleItem in parentVersionChildSubModuleItemsNotInDraft)
					{
						parentVersionChildSubModuleItem.Delete();
					}

					parentVersion.HasChanges = true;
					Delete();
					return parentVersion;
				}
				else
				{
					if (CAI_ParentTableCode == OrgCommissionAgreementSchema.Constants.Prefix)
					{
						CAI_ParentID = CommissionAgreement.MainVersion.PK;
					}
					else if (CAI_ParentTableCode == OrgCommissionAgreementItemSchema.Constants.Prefix)
					{
						CAI_ParentID = ParentItem.GetMainVersion().PK;
					}

					return this;
				}
			}
			finally
			{
				isMerging = false;
			}
		}
		bool isMerging;

		BusinessObjectCloneArgs GetDraftCloneArgs()
		{
			return new BusinessObjectCloneArgs(new[] { OrgCommissionAgreementItem.Schema.CAI_ParentID }, true);
		}

		#endregion

		#region Default Child Items

		void ResetChildItems()
		{
			using (CommissionAgreement != null ? CommissionAgreement.GetNotifyAgreementItemsChangedSuspender() : null)
			{
				DeleteAllChildItems();
				if (CAI_IsInclude)
				{
					AddChildAllItems();
				}
			}
		}

		void AddChildAllItems()
		{
			if (CAI_Type == OrgCommissionAgreementItemTypes.Codes.Product)
			{
				ChildServiceItems.AddNew(true, OrgCommissionAgreementItemLookups.AllServicesCode);
			}
			else if (CAI_Type == OrgCommissionAgreementItemTypes.Codes.Service)
			{
				ChildSubModuleItems.AddNew(true, OrgCommissionAgreementItemLookups.AllSubModulesCode);
			}
		}

		#endregion

		#region Item Path

		public Stack<Tuple<ZBool, ZString>> GetItemPath()
		{
			return !TopLevelItem.ConditionCollection.Any()
				? GetItemPathRegular()
				: GetItemConditionPathRaw();
		}

		Stack<Tuple<ZBool, ZString>> GetItemPathRegular()
		{
			var items = new Stack<Tuple<ZBool, ZString>>();
			items.Push(Tuple.Create(CAI_IsInclude, CAI_Code));

			var ancestorItem = ParentItem;
			while (ancestorItem != null)
			{
				items.Push(Tuple.Create(ancestorItem.CAI_IsInclude, ancestorItem.CAI_Code));
				ancestorItem = ancestorItem.ParentItem;
			}

			return items;
		}

		internal OrgCommissionAgreementItem TopLevelItem
		{
			get
			{
				var mostParent = this;
				while (mostParent.ParentItem != null)
				{
					mostParent = mostParent.ParentItem;
				}

				return mostParent;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Stack<Tuple<ZBool, ZString>> GetItemConditionPathRaw()
		{
			var items = new Stack<Tuple<ZBool, ZString>>();

			var mostParent = TopLevelItem;
			foreach (var condition in mostParent.ConditionCollection)
			{
				items.Push(Tuple.Create(ZBool.True, condition.CIC_Mode));
				items.Push(Tuple.Create(ZBool.True, (condition.CIC_RL_NKDestination.IsEmpty ? (ZString)AllItemCode : condition.CIC_RL_NKDestination)));
				items.Push(Tuple.Create(ZBool.True, (condition.CIC_RL_NKOrigin.IsEmpty ? (ZString)AllItemCode : condition.CIC_RL_NKOrigin)));
				items.Push(Tuple.Create(mostParent.CAI_IsInclude, mostParent.CAI_Code));
			}

			return items;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Stack<Tuple<ZBool, ZString>> GetItemConditionPathFormatted()
		{
			var items = new Stack<Tuple<ZBool, ZString>>();

			var mostParent = this;
			while (mostParent.ParentItem != null)
			{
				mostParent = mostParent.ParentItem;
			}

			ZString separator = "|";
			ZString fromTo = ">";

			int num = 0;
			foreach (var condition in mostParent.ConditionCollection)
			{
				if (num++ > 0)
				{
					items.Push(Tuple.Create(ZBool.True, (ZString)System.Environment.NewLine));
				}

				items.Push(Tuple.Create(ZBool.True, condition.CIC_Mode));
				items.Push(Tuple.Create(ZBool.True, separator));
				items.Push(Tuple.Create(ZBool.True, (condition.CIC_RL_NKDestination.IsEmpty ? (ZString)AllItemCode : condition.CIC_RL_NKDestination)));
				items.Push(Tuple.Create(ZBool.True, fromTo));
				items.Push(Tuple.Create(ZBool.True, (condition.CIC_RL_NKOrigin.IsEmpty ? (ZString)AllItemCode : condition.CIC_RL_NKOrigin)));
				items.Push(Tuple.Create(ZBool.True, separator));
				items.Push(Tuple.Create(mostParent.CAI_IsInclude, mostParent.CAI_Code));
			}

			return items;
		}

		public ZString GetItemPathDisplayText()
		{
			return !TopLevelItem.ConditionCollection.Any()
				? GetItemPathDisplayText(GetItemPath())
				: GetItemPathDisplayTextFormatted(GetItemConditionPathFormatted());
		}

		ZString GetItemPathDisplayTextFormatted(Stack<Tuple<ZBool, ZString>> itemPath)
		{
			var itemCodes = itemPath.Select(path => path.Item2);
			return string.Join(" ", itemCodes);
		}

		public Stack<Tuple<ZBool, ZString>> GetItemPathOriginalValue()
		{
			var items = new Stack<Tuple<ZBool, ZString>>();
			items.Push(Tuple.Create((ZBool)CAI_IsIncludeInfo.OriginalValue, (ZString)CAI_CodeInfo.OriginalValue));

			var ancestorItem = ParentItem;
			while (ancestorItem != null)
			{
				ancestorItem = ancestorItem.GetMainVersion();
				items.Push(Tuple.Create((ZBool)ancestorItem.CAI_IsIncludeInfo.OriginalValue, (ZString)ancestorItem.CAI_CodeInfo.OriginalValue));
				ancestorItem = ancestorItem.ParentItem;
			}

			return items;
		}

		public ZString GetItemPathDisplayTextOriginalValue()
		{
			var itemPath = GetItemPathOriginalValue();
			return GetItemPathDisplayText(itemPath);
		}

		static ZString GetItemPathDisplayText(Stack<Tuple<ZBool, ZString>> itemPath)
		{
			var itemCodes = itemPath.Select(path => path.Item2);
			if (itemPath.Last().Item1)
			{
				int maxItems = CommissionLookups.ShouldShowServicesAndSubModules ? 3 : 4;
				itemCodes = itemCodes.Concat(Enumerable.Repeat((ZString)OrgCommissionAgreementItem.AllItemCode, Math.Max(0, maxItems - itemPath.Count)));
			}

			return string.Join(" > ", itemCodes);
		}

		#endregion

		#region Item Conditions

		OrgCommissionAgreementItemConditionCollection conditionCollection;
		[ChildEditable]
		public OrgCommissionAgreementItemConditionCollection ConditionCollection
		{
			get
			{
				if (conditionCollection == null)
				{
					conditionCollection = new OrgCommissionAgreementItemConditionCollection(this);
					RegisterEditableChildObject(conditionCollection);
				}

				return conditionCollection;
			}
		}

		public ZBool HasCondition(ZString mode, ZString origin, ZString destination)
		{
			foreach (var condition in ConditionCollection)
			{
				if (condition.CIC_Mode.EqualsIgnoringCase(mode)
					&& condition.CIC_RL_NKOrigin.EqualsIgnoringCase(origin)
					&& condition.CIC_RL_NKDestination.EqualsIgnoringCase(destination))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Sibling Items

		public OrgCommissionAgreementItem[] SiblingItems
		{
			get { return Factory.Load<OrgCommissionAgreementItem>(GetSiblingItemsQuery()); }
		}

		ZQuery GetSiblingItemsQuery()
		{
			var query = new ZQuery(OrgCommissionAgreementItemSchema.CAI_ParentID, CAI_ParentID);
			query.AddToFilter(OrgCommissionAgreementItemSchema.CAI_Type, CAI_Type);
			return query;
		}

		#region Sibling All Item

		public OrgCommissionAgreementItem SiblingAllItem
		{
			get
			{
				if (IsAllItem)
				{
					return this;
				}
				else
				{
					return Factory.LoadTop1<OrgCommissionAgreementItem>(GetSiblingAllItemsQuery());
				}
			}
		}

		ZQuery GetSiblingAllItemsQuery()
		{
			var query = GetSiblingItemsQuery();
			query.AddToFilter(OrgCommissionAgreementItemSchema.CAI_Code, AllItemCode);
			return query;
		}

		#endregion

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return IsDeleted ?
					base.HumanReadableNameCore :
					(ZString)Res.GetString("2e08e88b-13f9-4ec4-8f95-f78a59111f30", "{0} Agreement Item '{1}'", CAI_TypeDescription, CAI_Code);
			}
		}

		#endregion

		#region Save

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || !this.IsUncommittedDraft()); }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			ModifiedLogs.AddLogsOnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			ModifiedLogs.OnFactorySaved(saveSucceeded);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			DeleteAllChildItems();

			if (!isMerging)
			{
				ModifiedLogs.AddDetachedLogOnDelete();
			}

			var commissionAgreement = CommissionAgreement;

			base.Delete();

			if (!isMerging && commissionAgreement != null)
			{
				commissionAgreement.NotifyAgreementItemsChanged();
			}
		}

		void DeleteAllChildItems()
		{
			foreach (var childItemCollection in ChildItemCollections)
			{
				((IActiveBusinessObjectCollection)childItemCollection).Refresh();
				childItemCollection.DeleteAll();
			}

			ConditionCollection.DeleteAll();
		}

		#endregion

		#region Logs

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				foreach (var childItemCollection in ChildItemCollections)
				{
					result.AddRange(childItemCollection.Where(x => x.IsMainVersion()));
					foreach (var childItem in childItemCollection)
					{
						result.AddRange(childItem.BusinessObjectsWithRelatedEvents);
					}
				}

				return result.ToArray();
			}
		}

		#region Modified Logs

		internal OrgCommissionAgreementItemModifiedLogsManager ModifiedLogs
		{
			get { return modifiedLogsManager ?? (modifiedLogsManager = new OrgCommissionAgreementItemModifiedLogsManager(this)); }
		}
		OrgCommissionAgreementItemModifiedLogsManager modifiedLogsManager;

		#endregion

		#endregion

		#region Related Business Objects

		public IEnumerable<OrgCommissionAgreementItemCollection> ChildItemCollections
		{
			get
			{
				if (CAI_Type == OrgCommissionAgreementItemTypes.Codes.Product)
				{
					yield return ChildServiceItems;
				}
				else if (CAI_Type == OrgCommissionAgreementItemTypes.Codes.Service)
				{
					yield return ChildSubModuleItems;
				}
			}
		}

		[ChildEditable]
		public OrgCommissionAgreementItemCollection ChildServiceItems
		{
			get
			{
				if (childServiceItems == null)
				{
					childServiceItems = new OrgCommissionAgreementItemCollection(this, OrgCommissionAgreementItemTypes.Codes.Service);
					RefreshChildServiceItemsReadOnly();
					RegisterEditableChildObject(childServiceItems);
				}

				return childServiceItems;
			}
		}
		OrgCommissionAgreementItemCollection childServiceItems;

		void RefreshChildServiceItemsReadOnly()
		{
			if (childServiceItems != null)
			{
				SetupChildEditable(childServiceItems, CAI_IsInclude);
			}
		}

		[ChildEditable]
		public OrgCommissionAgreementItemCollection ChildSubModuleItems
		{
			get
			{
				if (childSubModuleItems == null)
				{
					childSubModuleItems = new OrgCommissionAgreementItemCollection(this, OrgCommissionAgreementItemTypes.Codes.SubModule);
					RefreshChildSubModuleItemsReadOnly();
					RegisterEditableChildObject(childSubModuleItems);
				}

				return childSubModuleItems;
			}
		}

		public bool CanHaveConditions
		{
			get
			{
				if (IsDeleted || CAI_Type != OrgCommissionAgreementItemTypes.Codes.Product)
				{
					return false;
				}

				return CommissionRuleLookups.ProductSupportsTradeLane(CAI_Code);
			}
		}

		OrgCommissionAgreementItemCollection childSubModuleItems;

		void RefreshChildSubModuleItemsReadOnly()
		{
			if (childSubModuleItems != null)
			{
				SetupChildEditable(childSubModuleItems, CAI_IsInclude);
			}
		}

		void RefreshReadOnlyForChildItems()
		{
			RefreshChildServiceItemsReadOnly();
			RefreshChildSubModuleItemsReadOnly();
		}

		void SetupChildEditable<T>(ActiveBusinessObjectCollection<T> collection, bool shouldBeEditable) where T : BusinessObject
		{
			if (collection.ReadOnly == shouldBeEditable)
			{
				collection.SetReadOnlyIncludingChildren(!shouldBeEditable);
			}

			if (shouldBeEditable)
			{
				if (!IsRegisteredEditableChildObject(collection))
				{
					RegisterEditableChildObject(collection);
				}
			}
			else
			{
				if (IsRegisteredEditableChildObject(collection))
				{
					UnRegisterEditableChildObject(collection);
				}
			}
		}

		#endregion

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (CAI_ParentID.IsEmpty || CAI_ParentTableCode.IsEmpty)
			{
				var commissionAgreement = Factory.LoadTop1<OrgCommissionAgreement>(new ZQuery()) ?? Factory.NewWithValidTestData<OrgCommissionAgreement>();
				CAI_ParentID = commissionAgreement.PK;
				CAI_ParentTableCode = commissionAgreement.TablePrefix;
			}

			if (CAI_Type.IsEmpty)
			{
				CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
