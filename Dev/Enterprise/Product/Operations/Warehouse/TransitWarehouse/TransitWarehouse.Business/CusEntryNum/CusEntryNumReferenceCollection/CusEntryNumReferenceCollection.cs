using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business
{
	public sealed class CusEntryNumReferenceCollection : ActiveBusinessObjectCollection<CusEntryNumber>, ICusEntryNumReferenceCollection
	{
		public CusEntryNumReferenceCollection(BusinessObject parent)
			: base(parent.Factory, parent, new ZQuery(), CusEntryNumSchema.CE_ParentID)
		{
		}

		#region ICusEntryNumReferenceCollection

		ICusEntryNumber ICusEntryNumReferenceCollection.this[int i] => base[i];

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewElementCore(CusEntryNumber newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.CE_EntryIsSystemGenerated = false;
		}

		protected override void SetRelationshipDefaultsForElementCore(CusEntryNumber newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.Parent = Relationship.Master;
		}

		protected override void OnAdded(CusEntryNumber businessObject)
		{
			base.OnAdded(businessObject);
			businessObject.Parent = Relationship.Master;
		}

		protected override void OnLoadedIntoCollectionCore(CusEntryNumber loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.Parent = Relationship.Master;
		}

		#endregion

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			query.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
			return query;
		}

		#endregion
	}
}
