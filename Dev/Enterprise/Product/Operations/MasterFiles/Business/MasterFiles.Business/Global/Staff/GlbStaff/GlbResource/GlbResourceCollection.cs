using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbResourceCollection : GlbStaffAndResourceCollection
	{
		public GlbResourceCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public GlbResourceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbResourceCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(GlbStaffSchema.GS_IsResource, true);
			return query;
		}

		#endregion

		#region Default Values

		protected override void SetRelationshipDefaultsForElementCore(GlbStaff newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			GlbStaff @object = newElement;
			if (!@object.GS_IsResource)
			{
				@object.GS_IsResource = true;
			}
		}

		#endregion
	}
}
