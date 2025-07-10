using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLineSubAccountCollection<T, MasterT> : DependentBusinessObjectCollection<T, MasterT>
			where T : AccTransactionLineSubAccount
			where MasterT : AccTransactionLines
	{
		public AccTransactionLineSubAccountCollection(MasterT master) : base(master)
		{
		}
	}
}
