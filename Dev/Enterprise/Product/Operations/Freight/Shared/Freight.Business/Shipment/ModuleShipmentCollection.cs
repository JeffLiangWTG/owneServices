using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Collection of ModuleShipments used in CommonShipment module main grid and Shipments findbox
	/// </summary>
	[ZArchitecture.ComponentModel.ModuleID(ModuleId.JobShipment)]
	public class ModuleShipmentCollection : BusinessObjectCollection<CommonShipment>, IRelationshipAdderForController
	{
		public ModuleShipmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		/// <summary>
		/// Allow public access to protected RelationshipFilter
		/// </summary>
		/// <param name="filter"></param>
		public void AddRelationshipFilter(ZQuery filter)
		{
			if (relationshipFilter == null)
			{
				relationshipFilter = new ZQuery();
			}
			relationshipFilter.AddToFilter(filter);
		}

		ZQuery relationshipFilter;
		/// <summary>
		/// Don't remove this override. Base creates a new RelationshipFilter each time in its getter.
		/// Users of this collection will add to this filter using the AddRelationshipFilter method.
		/// </summary>
		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(relationshipFilter);
		}

		#region IRelationshipAdderForController Members

		/// <summary>
		/// To be set when this collection is used in a findbox list from a Consol.
		/// Used by AddRelationshipToNewObject, CreateAdditionalFilter and AddNotificationWhenAdditionalFilterNotMet.
		/// </summary>
		public CommonConsol ParentConsol { get; set; }

		public void AddRelationshipToNewObject(BusinessObject newBusinessObject)
		{
			if (newBusinessObject is CommonShipment && ParentConsol != null)
			{
				(newBusinessObject as CommonShipment).Consols.AddFromDatabase(ParentConsol.PK);
			}
		}

		#endregion

		#region IsRelationshitFilterEmptyForTesting

#if DEBUG
		public bool IsRelationshipFilterEmptyForTesting()
		{
			return RelationshipFilter.IsEmpty;
		}
#endif

		#endregion
	}
}
