using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ContainerMovementProcessTaskCollection : ProcessTaskCollection
	{
		public ContainerMovementProcessTaskCollection(ContainerMovement movement)
			: base(movement)
		{
		}

		public new ContainerMovement Parent
		{
			get { return (ContainerMovement)base.Parent; }
		}

		public new ContainerMovementProcessTask this[int index]
		{
			get { return (ContainerMovementProcessTask)Elements[index]; }
		}

		public new ContainerMovementProcessTask AddNew()
		{
			return (ContainerMovementProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new ContainerMovementProcessTaskCollection(Parent);
		}
	}
}
