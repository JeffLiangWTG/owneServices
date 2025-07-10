using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionHeaderSubAccountCollection<T, MasterT> : DependentBusinessObjectCollection<T, MasterT>
			where T : AccTransactionHeaderSubAccount
			where MasterT : AccTransactionHeader
	{
		public AccTransactionHeaderSubAccountCollection(MasterT master) : base(master)
		{
		}
	}
}
