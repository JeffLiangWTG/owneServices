using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IMergeOrgElement
	{
		IEnumerable<BusinessObject> NewObjectsCollection { get; }

		ZGuid NewObjectPK { get; set; }

		ZPropertyInfo NewObjectPKInfo { get; }

		OrgHeader NewOrganization { get; set; }

		BusinessObjectCollection ParentCollection { get; set; }

		bool ShouldIncludeSimilarCollection { get; set; }

		BusinessObject OldObject { get; }

		ZString Action { get; set; }

		ZPropertyInfo ActionInfo { get; }

		BusinessObjectCollection GetNewObjectsCollection(BusinessObjectFactory factory, ZQuery query);

		ZGuid FindFuzzyMatch(IEnumerable<BusinessObject> collection, string[] stringToMatch);

		SchemaColumn[] ColumnsForMatching { get; }
	}

	public abstract class MergeOrgElement<T> : NonPersistentBusinessObject, IObsoleteValidation, IMergeOrgElement
		where T : BusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected MergeOrgElement(BusinessObjectFactory factory, T oldObj, OrgHeader newOrg, BusinessObjectCollection newOrgElementCollection)
			: base(factory)
		{
			oldObject = oldObj;
			Action = ActionAdd;
			NewOrgElementCollection = newOrgElementCollection;
			NewOrganization = newOrg;
		}

		readonly T oldObject;

		readonly BusinessObjectCollection NewOrgElementCollection;

		#region Properties

		#region NewOrganization

		OrgHeader newOrganization;

		public OrgHeader NewOrganization
		{
			get
			{
				return newOrganization;
			}
			set
			{
				if (value != newOrganization)
				{
					newOrganization = value;
					ReloadNewObjectsCollection();
					SetFuzzyMatchNewObject();
					ValidateAction();
				}
			}
		}

		#endregion

		#region New Object PK

		ZGuid newObjectPK;
		public ZGuid NewObjectPK
		{
			get { return newObjectPK; }
			set
			{
				if (value != newObjectPK)
				{
					SetNonPersistentPropertyValue(NewObjectPKInfo, ref newObjectPK, value);
					if (!IsValidationSuspended)
					{
						ValidateNewObjectPK();
					}
				}
			}
		}

		public ZPropertyInfo NewObjectPKInfo
		{
			get
			{
				return GetZPropertyInfo(SchemaNewObjectPK);
			}
		}

		protected bool NewObjectPK_ReadOnly
		{
			get { return (Action == ActionAdd); }
		}

		protected abstract string SchemaNewObjectPK { get; }

		#endregion

		public bool ShouldIncludeSimilarCollection { get; set; }

		[BusinessObjectTestExclude]
		public BusinessObjectCollection ParentCollection { get; set; }

		#region Action

		ZString action;
		[List("ActionsList")]
		[MaxLength(3)]
		public ZString Action
		{
			get { return action; }
			set
			{
				if (value != action)
				{
					CheckMaximumLength(ActionInfo, value);

					SetNonPersistentPropertyValue(ActionInfo, ref action, value);
					if (action == ActionAdd)
					{
						newObjectPK = Guid.Empty;
					}
					NewObjectPKInfo.ClearAllNotifications();
					ValidateNewObjectPK();
					ValidateAction();
				}
			}
		}

		public ZPropertyInfo ActionInfo
		{
			get { return GetZPropertyInfo(nameof(Action)); }
		}

		#endregion

		#region NewObjectsCollection

		protected BusinessObjectCollection newObjectsCollection;

		public IEnumerable<BusinessObject> NewObjectsCollection
		{
			get
			{
				if (newObjectsCollection == null)
				{
					ReloadNewObjectsCollection();
				}
				IEnumerable<BusinessObject> result = newObjectsCollection.ToArray();

				if (ShouldIncludeSimilarCollection && ParentCollection != null)
				{
					List<BusinessObject> resultList = new List<BusinessObject>(result);
					foreach (BusinessObject obj in ParentCollection.ToArray())
					{
						IMergeOrgElement merge = (obj as IMergeOrgElement);
						if (merge != null)
						{
							if (!resultList.Contains(merge.OldObject) && OldObject != merge.OldObject && merge.Action == ActionAdd)
							{
								resultList.Add(merge.OldObject);
							}
						}
					}
					result = resultList;
				}
				return result;
			}
		}

		#endregion

		#region Old Object

		public T OldObjectCore
		{
			get
			{
				return oldObject;
			}
		}

		public BusinessObject OldObject
		{
			get
			{
				return OldObjectCore;
			}
		}

		#endregion

		#endregion

		#region Methods

		public void ReloadNewObjectsCollection()
		{
			if (NewOrgElementCollection != null)
			{
				newObjectsCollection = NewOrgElementCollection;
			}
			else
			{
				if (NewOrganization == null)
				{
					ZQuery query = new ZQuery(FKSchemaColumn, ZGuid.Empty);
					newObjectsCollection = GetNewObjectsCollection(Factory, query);
				}
				else
				{
					ZQuery query = new ZQuery(FKSchemaColumn, NewOrganization.PK);
					newObjectsCollection = GetNewObjectsCollection(Factory, query);
					newObjectsCollection.Load();
				}
			}
		}

		void SetFuzzyMatchNewObject()
		{
			ZGuid fuzzyMatchPK = FindFuzzyMatch();

			if (fuzzyMatchPK != ZGuid.Empty)
			{
				Action = ActionMerge;
				NewObjectPK = fuzzyMatchPK;
			}
			else
			{
				Action = ActionAdd;
			}
		}

		#endregion

		#region ActionsList

		public ZArchitecture.Core.CodeDescriptionPairList ActionsList
		{
			get
			{
				ZArchitecture.Core.CodeDescriptionPairList list = new ZArchitecture.Core.CodeDescriptionPairList();

				list.AddPair(ActionAdd, Res.GetString("a15d7921-b27f-41b4-a833-cf74e599cff1", "Add as new {0}", ElementNameForAction));
				list.AddPair(ActionMerge, Res.GetString("f7b608d0-e5e5-467f-9fa1-8e0f9caa0038", "Merge with match {0}", ElementNameForAction));

				return list;
			}
		}

		protected abstract string ElementNameForAction { get; }

		public const string ActionAdd = "ADD";
		public const string ActionMerge = "MRG";

		#endregion

		#region Validation

		public virtual void ValidateAction()
		{
			ActionInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ActionInfo);
			MandatoryValidation.CheckEntered(ActionInfo);
		}

		public void ValidateNewObjectPK()
		{
			NewObjectPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(NewObjectPKInfo);

			if (action == ActionMerge)
			{
				MandatoryValidation.CheckEntered(NewObjectPKInfo);
				if (!ShouldIncludeSimilarCollection)
				{
					bool isThere = false;
					foreach (var obj in NewObjectsCollection)
					{
						if (NewObjectPK == obj.PK)
						{
							isThere = true;
						}
					}
					if (!isThere)
					{
						NewObjectPKInfo.AddError(Res.GetString("c76963d9-da0e-4a87-8dca-f7337f5a9ec6", "Enter a valid value."));
					}
				}
				else if (!NewObjectPKInfo.HasErrors())
				{
					bool hasPK = false;
					foreach (BusinessObject obj in NewObjectsCollection)
					{
						if (obj.PK == NewObjectPK)
						{
							hasPK = true;
							break;
						}
					}
					if (!hasPK)
					{
						NewObjectPKInfo.AddError(ListValidation.GetNotificationMessage(Res.GetString("fed61d98-26d2-4ae2-bbf9-1c03fe1a1aa8", "code")).ToString());
					}
					if (ParentCollection != null)
					{
						CheckIfTargetAddressIsNotMerged();
						CheckIfCurrentAddressIsNotTargetForMerging();
					}
				}
			}
		}

		void CheckIfTargetAddressIsNotMerged()
		{
			foreach (IMergeOrgElement merge in ParentCollection)
			{
				if (merge.OldObject.PK == NewObjectPK)
				{
					if (merge.Action == ActionMerge)
					{
						NewObjectPKInfo.AddError(Res.GetString("f44f430c-bbbc-454a-b919-8d4ed7644432", "You cannot merge into this {0} as it is being merged into another {1} itself.", ElementNameForAction, ElementNameForAction));
					}
					break;
				}
			}
		}

		void CheckIfCurrentAddressIsNotTargetForMerging()
		{
			foreach (IMergeOrgElement merge in ParentCollection)
			{
				if (merge.NewObjectPK == OldObject.PK)
				{
					if (merge.Action == ActionMerge)
					{
						NewObjectPKInfo.AddError(Res.GetString("e7afdb6c-e387-4bac-81b2-17018994e2b7", "You cannot merge this {0} into any other {1}, because this {2} was previously setup to be added to the New Organization and there is currently at least one other {3} setup to be merged into this one.", ElementNameForAction, ElementNameForAction, ElementNameForAction, ElementNameForAction));
					}
					break;
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateNewObjectPK();
		}

		#endregion

		#region Abstractions

		public abstract BusinessObjectCollection GetNewObjectsCollection(BusinessObjectFactory factory, ZQuery query);

		protected abstract SchemaColumn FKSchemaColumn { get; }

		protected abstract ZGuid FindFuzzyMatch();

		public abstract ZGuid FindFuzzyMatch(IEnumerable<BusinessObject> collection, string[] stringToMatch);

		public abstract SchemaColumn[] ColumnsForMatching { get; }

		#endregion
	}
}
