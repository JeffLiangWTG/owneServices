using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	[ModuleID(Enterprise.ZArchitecture.Modules.ModuleId.JobShipment)]
	public class ForwardingShipmentCollection : ShipmentCollection
	{
		public ForwardingShipmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ForwardingShipmentCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public new ForwardingShipment this[int index]
		{
			get { return (ForwardingShipment)Elements[index]; }
		}

		public new ForwardingShipment AddNew()
		{
			return (ForwardingShipment)base.AddNew();
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return base.CreateAdditionalFilter().AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, ZBool.True);
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new ForwardingShipmentFindBoxListProvider(this); }
		}
	}

	class ForwardingShipmentFindBoxListProvider : FindBoxListProvider
	{
		public ForwardingShipmentFindBoxListProvider(ForwardingShipmentCollection collection)
			: base(collection)
		{
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
		{
			var bizObjs = Enumerable.Empty<ForwardingShipment>();

			if (!string.IsNullOrEmpty(code))
			{
				var query = new ZQuery();
				AddCodeEqualsFilter(query, code);
				query.AddToFilter(List.RelationshipFilter);
				query.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, ZBool.True);
				bizObjs = List.Factory.Load<ForwardingShipment>(query);
			}

			return bizObjs;
		}
	}
}
