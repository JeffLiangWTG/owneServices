using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadListProcessTaskCollection : ProcessTaskCollection
	{
		public ContainerLoadListProcessTaskCollection(CYContainerLoadList header) : base(header)
		{
		}

		public new ContainerLoadListProcessTask this[int index]
		{
			get { return (ContainerLoadListProcessTask)Elements[index]; }
		}

		public new ContainerLoadListProcessTask AddNew()
		{
			return (ContainerLoadListProcessTask)base.AddNew();
		}
	}
}
