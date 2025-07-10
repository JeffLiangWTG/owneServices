using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionCalculationQueue : AutoOrgCommissionCalculationQueue
	{
		public ICommissionableTransaction CommissionableTransaction => Factory.Load<ITransactionHeader>(CAQ_AH) as ICommissionableTransaction;  // to use TransactionHeaderTypeDecider

		public OrgCommissionCalculationQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
