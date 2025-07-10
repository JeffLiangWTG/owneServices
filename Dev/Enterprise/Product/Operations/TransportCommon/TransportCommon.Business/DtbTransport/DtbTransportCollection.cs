using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportCollection<T> : ActiveBusinessObjectCollection<T>, IDtbTransportCollection
		where T : DtbTransport
	{
		protected DtbTransportCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected DtbTransportCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected DtbTransportCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected DtbTransportCollection(BusinessObjectFactory factory, BusinessObject master, ZQuery filter, SchemaColumn relationshipColumn)
			: base(factory, master, filter, relationshipColumn)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = new ZQuery(DtbBookingSchema.KM_JobType, GetParentType());
			filter.AddToFilter(base.CreateRelationshipFilter());

			return filter;
		}

		protected abstract string GetParentType();

		// interfaces

		#region IDtbTransportCollection Members

		IEnumerable<DtbTransport> IDtbTransportCollection.Typed
		{
			get { return this; }
		}

		DtbTransport IDtbTransportCollection.this[int index] => this[index];

		#endregion
	}
}
