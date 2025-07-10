using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefShippingLineMessagingRequirementCollection : ActiveBusinessObjectCollection<RefShippingLineMessagingRequirement>
	{
		public RefShippingLineMessagingRequirementCollection(RefShippingLine master)
				: base(Argument.NotNull(master, nameof(master)).Factory, master, new ZQuery(), RefShippingLineMessagingRequirementSchema.RSR_RSL_ShippingLine)
		{
			Master = master;
		}

		public RefShippingLineMessagingRequirementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public readonly RefShippingLine Master;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			if (Master != null)
			{
				query.AddToFilter(RefShippingLineMessagingRequirementSchema.RSR_RSL_ShippingLine, Master.PK);
			}

			return query;
		}
	}
}
