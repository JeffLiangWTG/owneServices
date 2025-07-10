using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadListLineProcessTaskCollection(ContainerLoadListLine parent)
		: ProcessTaskCollection(parent)
	{
		public new ContainerLoadListLineProcessTask this[int index]
		{
			get { return (ContainerLoadListLineProcessTask)Elements[index]; }
		}

		public new ContainerLoadListLineProcessTask AddNew()
		{
			return (ContainerLoadListLineProcessTask)base.AddNew();
		}
	}
}
