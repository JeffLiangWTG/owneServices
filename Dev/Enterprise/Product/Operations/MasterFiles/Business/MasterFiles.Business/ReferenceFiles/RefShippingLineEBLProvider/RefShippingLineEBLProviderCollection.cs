using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefShippingLineEBLProviderCollection : ActiveBusinessObjectCollection<RefShippingLineEBLProvider>
	{
		public RefShippingLineEBLProviderCollection(RefShippingLine master)
		: base(Argument.NotNull(master, nameof(master)).Factory, master, new ZQuery(), RefShippingLineEBLProviderSchema.RSE_RSL_ShippingLine)
		{
			Master = master;
		}

		public RefShippingLineEBLProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public readonly RefShippingLine Master;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			if (Master != null)
			{
				query.AddToFilter(RefShippingLineEBLProviderSchema.RSE_RSL_ShippingLine, Master.PK);
			}

			return query;
		}
	}
}
