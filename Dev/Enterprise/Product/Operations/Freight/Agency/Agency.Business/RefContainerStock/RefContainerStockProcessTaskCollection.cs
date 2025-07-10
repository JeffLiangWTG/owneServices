using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class RefContainerStockProcessTaskCollection : ProcessTaskCollection
	{
		public RefContainerStockProcessTaskCollection(RefContainerStock refContainerStock)
			: base(refContainerStock)
		{
		}

		public new RefContainerStock Parent
		{
			get { return (RefContainerStock)base.Parent; }
		}

		public new RefContainerStockProcessTask this[int index]
		{
			get { return (RefContainerStockProcessTask)Elements[index]; }
		}

		public new RefContainerStockProcessTask AddNew()
		{
			return (RefContainerStockProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new RefContainerStockProcessTaskCollection(Parent);
		}
	}
}
