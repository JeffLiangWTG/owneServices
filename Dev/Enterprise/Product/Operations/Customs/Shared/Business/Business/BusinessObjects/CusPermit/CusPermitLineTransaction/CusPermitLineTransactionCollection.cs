using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusPermitLineTransactionCollection : ActiveBusinessObjectCollection<BaseCusPermitLineTransaction>
	{
		public CusPermitLineTransactionCollection(BaseCusPermitHeader master)
			: base(master.Factory, master, new ZQuery(), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader)
		{
		}

		public CusPermitLineTransactionCollection(BaseCusPermitHeader master, ZQuery query)
			: base(master.Factory, master, query, CusPermitLineTransactionSchema.CPL_CPH_PermitHeader)
		{
		}

		BaseCusPermitHeader PermitHeader => (BaseCusPermitHeader)this.Relationship.Master;

		static ZQuery GetTransactionCategoryFilter(ZString transactionCategory)
		{
			return new ZQuery(CusPermitLineTransactionSchema.CPL_TransactionCategory, transactionCategory);
		}

		public void AddTransactionCategoryFilter(ZString transactionCategory)
		{
			if (transactionCategory.IsEmpty)
			{
				AdditionalFilter = new ZQuery();
			}
			else
			{
				AdditionalFilter = GetTransactionCategoryFilter(transactionCategory);
			}
		}

		protected override void SetDefaultsForNewElementCore(BaseCusPermitLineTransaction newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.CPL_TransactionCategory = PermitHeader.TransactionCategory;
		}

		protected override bool AllowNew => PermitHeader.AllowNewLineTransactions;
	}
}
