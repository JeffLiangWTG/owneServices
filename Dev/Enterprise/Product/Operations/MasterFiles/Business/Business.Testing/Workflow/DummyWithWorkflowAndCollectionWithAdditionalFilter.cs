using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	public class DummyWithWorkflowAndCollectionWithAdditionalFilter : DummyWithWorkflow
	{
		public DummyWithWorkflowAndCollectionWithAdditionalFilter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new DummyChildEnterpriseBusinessObjectCollectionWithAdditionalFilter Collection
		{
			get
			{
				return (DummyChildEnterpriseBusinessObjectCollectionWithAdditionalFilter)base.Collection;
			}
		}

		protected override DummyChildBusinessObjectCollection NewCollection()
		{
			return collectionCache.GetOrAdd(PK.ToGuid(), () => new DummyChildEnterpriseBusinessObjectCollectionWithAdditionalFilter(Factory));
		}
		static readonly Dictionary<Guid, DummyChildEnterpriseBusinessObjectCollectionWithAdditionalFilter> collectionCache = new Dictionary<Guid, DummyChildEnterpriseBusinessObjectCollectionWithAdditionalFilter>();

		public OrgHeaderCollection OrgHeaderCollection
		{
			get => orgHeaderCollectionCache.GetOrAdd(PK.ToGuid(), () => new OrgHeaderCollection(Factory));
		}
		static readonly Dictionary<Guid, OrgHeaderCollection> orgHeaderCollectionCache = new Dictionary<Guid, OrgHeaderCollection>();

		public ZString Z0_SetterThatBlocksChanges
		{
			get => "";
			set { }
		}
	}
}
