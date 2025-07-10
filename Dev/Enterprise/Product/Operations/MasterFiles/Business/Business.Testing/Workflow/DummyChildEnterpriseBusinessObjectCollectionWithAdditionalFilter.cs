using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyChildEnterpriseBusinessObjectCollectionWithAdditionalFilter : DummyChildEnterpriseBusinessObjectCollection
	{
		public DummyChildEnterpriseBusinessObjectCollectionWithAdditionalFilter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyChildEnterpriseBusinessObjectCollectionWithAdditionalFilter(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		readonly List<ZGuid> addedToCollection = new List<ZGuid>();

		public new DummyChildEnterpriseBusinessObject AddNew()
		{
			var newObject = base.AddNew();
			addedToCollection.Add(newObject.PK);

			return newObject;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			query.AddToFilter(DummyBizoSchema.PK, addedToCollection.ToArray());

			return query;
		}
	}
}
