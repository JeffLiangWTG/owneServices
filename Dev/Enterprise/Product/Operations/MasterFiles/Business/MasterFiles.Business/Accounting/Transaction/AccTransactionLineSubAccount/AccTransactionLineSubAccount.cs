using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLineSubAccount : AutoAccTransactionLineSubAccount
	{
		public AccTransactionLineSubAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
