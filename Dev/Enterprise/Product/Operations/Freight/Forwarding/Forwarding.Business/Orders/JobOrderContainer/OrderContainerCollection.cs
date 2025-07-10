using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderContainerCollection : DependentBusinessObjectCollection<OrderContainer, Order>
	{
		public OrderContainerCollection(Order parentOrder, BusinessObjectFactory factory) : base(parentOrder, factory)
		{
		}

		public void AddToCopiedCollection(OrderContainerCollection copiedContainers)
		{
			if (this.Count > 0)
			{
				ArrayList containerTypes = new ArrayList();
				OrderContainer newContainer;

				foreach (OrderContainer container in this)
				{
					if (!containerTypes.Contains(container.J1_RC))
					{
						containerTypes.Add(container.J1_RC);
					}
				}

				foreach (ZGuid type in containerTypes.ToArray(typeof(ZGuid)))
				{
					newContainer = copiedContainers.AddNew();
					newContainer.J1_ContainerNumber = ZString.Empty;
					newContainer.J1_RC = type;

					foreach (OrderContainer container in this)
					{
						if (container.J1_RC == type)
						{
							newContainer.J1_ContainerCount += container.J1_ContainerCount;
						}
					}
				}
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery(JobOrderContainerSchema.J1_ParentID, Master.PK);
			query.AddToFilter(JobOrderContainerSchema.J1_ParentTableCode, JobOrderHeaderSchema.Constants.Prefix);
			return query;
		}

		protected override string FkColumnName => JobOrderContainerSchema.J1_ParentID.Name;
	}
}
