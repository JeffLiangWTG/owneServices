using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionCalculationQueueCollection : ActiveBusinessObjectCollection<OrgCommissionCalculationQueue>
	{
		public OrgCommissionCalculationQueueCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObject master) : base(master)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObject master, ZQuery filter) : base(master, filter)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObjectFactory factory, BusinessObject master) : base(factory, master)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObjectFactory factory, BusinessObject master, ZQuery filter) : base(factory, master, filter)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObjectFactory factory, BusinessObject master, ZQuery filter, SchemaColumn relationshipColumn) : base(factory, master, filter, relationshipColumn)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObject master, Type pivotObjectType, ZQuery filter) : base(master, pivotObjectType, filter)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObject master, Type pivotObjectType) : base(master, pivotObjectType)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObject master, Type pivotObjectType, ZQuery filter, SchemaGuidColumn pivotTableFKToMaster, SchemaGuidColumn pivotTableFKToElements) : base(master, pivotObjectType, filter, pivotTableFKToMaster, pivotTableFKToElements)
		{
		}

		public OrgCommissionCalculationQueueCollection(BusinessObjectFactory factory, ICollectionRelationship relationship) : base(factory, relationship)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
