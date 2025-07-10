using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgFreeWaitingTimeCollection : ActiveBusinessObjectCollection<OrgFreeWaitingTime>
	{
		public OrgFreeWaitingTimeCollection(BusinessObjectFactory factory, OrgAddress address)
			: base(factory, OrgFreeWaitingRelationship(address))
		{
		}

		static ICollectionRelationship OrgFreeWaitingRelationship(OrgAddress address)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgFreeWaitingTimeSchema.OY_OA_Address, address.PK);
			query.AddToFilter(OrgFreeWaitingTimeSchema.OY_GC_Company, Env.CurrentCompany.PK);

			return new OrgFreeWaitingDependentRelationship(address, typeof(OrgFreeWaitingTime), query, OrgFreeWaitingTimeSchema.OY_OA_Address);
		}

		protected override void SetDefaultsForNewElementCore(OrgFreeWaitingTime newElement)
		{
			newElement.OY_DropMode = Constants.EquipmentNeeded.Any;
			base.SetDefaultsForNewElementCore(newElement);
		}

		class OrgFreeWaitingDependentRelationship : DependentRelationship
		{
			public OrgFreeWaitingDependentRelationship(BusinessObject master, Type elementType, ZQuery filter, SchemaGuidColumn fkColumn)
				: base(master, elementType, filter, fkColumn)
			{ }

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				base.AddToRelationship(businessObject);
				businessObject[OrgFreeWaitingTimeSchema.OY_GC_Company] = Env.CurrentCompany.PK;
			}
		}
	}
}
