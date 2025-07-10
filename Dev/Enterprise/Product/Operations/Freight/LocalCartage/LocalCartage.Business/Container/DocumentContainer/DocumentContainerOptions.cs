using CargoWise.EntityFramework;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class DocumentContainerOptions : NonPersistentBusinessObject
	{
		public DocumentContainerOptions(DocumentContainerCollection containers)
			: base(containers.Factory)
		{
			this.containers = containers;
		}

		public DocumentContainerCollection Containers
		{
			get { return containers; }
		}
		readonly DocumentContainerCollection containers;
	}
}
