using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusWHSOperatorTransactionCollection : DependentBusinessObjectCollection<CusWHSOperatorTransaction, CusWHSOperatorTransactionBatch>
	{
		public CusWHSOperatorTransactionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusWHSOperatorTransactionCollection(CusWHSOperatorTransactionBatch master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusWHSOperatorTransactionSchema.WOT_WOB_CusWHSTransactionBatch;
	}
}
