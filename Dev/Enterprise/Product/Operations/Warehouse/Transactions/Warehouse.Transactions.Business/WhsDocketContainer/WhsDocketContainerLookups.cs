using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketContainerLookups : AutoWhsDocketContainerLookups
	{
		public WhsDocketContainerLookups(AutoWhsDocketContainer parent)
			: base(parent) { }

		#region RefContainers

		public RefContainerCollection RefContainers
		{
			get { return Factory.GetCachedValue("WhsDocketContainerLookups|RefContainers", () => new RefContainerCollection(Factory)); }
		}

		#endregion
	}
}
