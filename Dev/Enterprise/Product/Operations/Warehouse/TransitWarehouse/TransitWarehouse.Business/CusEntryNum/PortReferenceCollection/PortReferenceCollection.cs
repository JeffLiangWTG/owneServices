using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business
{
	public sealed class PortReferenceCollection : ActiveBusinessObjectCollection<CusEntryNumber>, IPortReferenceCollection
	{
		public PortReferenceCollection(BusinessObject parent)
			: base(parent.Factory, parent, new ZQuery(), CusEntryNumSchema.CE_ParentID)
		{
		}

		#region IPortReferenceCollection

		ICusEntryNumber IPortReferenceCollection.this[int i] => base[i];

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewElementCore(CusEntryNumber newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.CE_Category = TransitWarehouseReferenceCategories.Codes.PortReference;
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
			query.AddToFilter(CusEntryNumSchema.CE_Category, TransitWarehouseReferenceCategories.Codes.PortReference);
			return query;
		}

		#endregion
	}
}
