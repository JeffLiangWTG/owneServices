using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	interface IParentCollectionSetter
	{
		void SetParentCollection();
	}

	public abstract class MergeOrgElementCollection<T> : NonPersistentBusinessObjectCollection<T>, IParentCollectionSetter where T : NonPersistentBusinessObject, IMergeOrgElement
	{
		protected MergeOrgElementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected MergeOrgElementCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public void SetNewOrganisation(OrgHeader newOrganization)
		{
			foreach (IMergeOrgElement mergeElement in Elements)
			{
				mergeElement.NewOrganization = newOrganization;
			}
		}

		public void SetParentCollection()
		{
			foreach (IMergeOrgElement mergeElement in Elements)
			{
				mergeElement.ParentCollection = this;
				mergeElement.ShouldIncludeSimilarCollection = true;
			}
			SetMergesInsideCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		void SetMergesInsideCollection()
		{
			if (Count > 0)
			{
				BusinessObjectCollection uniqueCollection = GetListOfUniqueObjects();
				foreach (IMergeOrgElement element in this)
				{
					if (!uniqueCollection.Contains(element.OldObject) && !element.OldObject.IsDeleted)
					{
						ZGuid pk = element.FindFuzzyMatch(element.NewObjectsCollection, GetColumnsToCompare(element));
						if (pk != ZGuid.Empty)
						{
							element.Action = MergeOrgAddress.ActionMerge;
							element.NewObjectPK = pk;
						}
					}
				}
			}
		}

		BusinessObjectCollection GetListOfUniqueObjects()
		{
			BusinessObjectCollection result = this[0].GetNewObjectsCollection(Factory, new ZQuery());
			foreach (IMergeOrgElement obj in this)
			{
				if (!obj.OldObject.IsDeleted && obj.FindFuzzyMatch(result, GetColumnsToCompare(obj)) == ZGuid.Empty)
				{
					result.Add(obj.OldObject);
				}
			}
			return result;
		}

		string[] GetColumnsToCompare(IMergeOrgElement obj)
		{
			StringCollectionX columnsToCompare = new StringCollectionX();
			if (Count > 0)
			{
				foreach (SchemaColumn col in obj.ColumnsForMatching)
				{
					columnsToCompare.Add(obj.OldObject[col].ToString());
				}
			}
			return columnsToCompare.ToArray();
		}
	}
}
