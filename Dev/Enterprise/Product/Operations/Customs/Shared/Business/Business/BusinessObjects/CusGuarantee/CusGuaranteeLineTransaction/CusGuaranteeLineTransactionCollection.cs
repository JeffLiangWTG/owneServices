using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusGuaranteeLineTransactionCollection : ActiveBusinessObjectCollection<BaseCusGuaranteeLineTransaction>
	{
		public CusGuaranteeLineTransactionCollection(BaseCusGuaranteeHeader master)
			: base(master.Factory, master, new ZQuery(), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader)
		{
		}

		public CusGuaranteeLineTransactionCollection(BaseCusGuaranteeHeader master, ZQuery query)
			: base(master.Factory, master, query, CusPermitLineTransactionSchema.CPL_CPH_PermitHeader)
		{
		}

		public BaseCusGuaranteeHeader GuaranteeHeader => (BaseCusGuaranteeHeader)Relationship.Master;

		protected override void SetDefaultsForNewElementCore(BaseCusGuaranteeLineTransaction newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.CPL_TransactionCategory = GuaranteeHeader.TransactionCategory;
		}

		protected override bool AllowNew => GuaranteeHeader.AllowNewLineTransactions;
	}
}
