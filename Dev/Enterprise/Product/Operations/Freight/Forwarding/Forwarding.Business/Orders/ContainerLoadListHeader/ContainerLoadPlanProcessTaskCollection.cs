using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadPlanProcessTaskCollection : ProcessTaskCollection
	{
		public ContainerLoadPlanProcessTaskCollection(CFSContainerLoadList header) : base(header)
		{
		}

		public new ContainerLoadPlanProcessTask this[int index]
		{
			get { return (ContainerLoadPlanProcessTask)Elements[index]; }
		}

		public new ContainerLoadPlanProcessTask AddNew()
		{
			return (ContainerLoadPlanProcessTask)base.AddNew();
		}
	}
}
