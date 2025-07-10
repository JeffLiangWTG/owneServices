using CargoWise.Data;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class MultiPersonMergerCore : IMultiPersonMergerCore
	{
		public MultiPersonMergerCore()
		{
		}

		public MultiPersonMergerCore(IPersonMergeTransactionSaver transactionSaver)
		{
			TransactionSaver = transactionSaver;
		}

		public IPersonMergeTransactionSaver TransactionSaver;

		public virtual MultiPersonMergerResult Merge(GlbPerson retained, GlbPerson dissolved)
		{
			var personMergerResult = new MultiPersonMergerResult();

			using (Db.DisposableActionForDbConnection())
			{
				using (var personMerger = new PersonMerger(retained, dissolved, TransactionSaver ?? new PersonMergeTransactionSaver(), new PersonMergeBusinessObjectFactoryLoader()))
				{
					if (personMerger.Merge(PersonMergeMode.Multi))
					{
						personMergerResult = personMerger.multiPersonMergerResult;
					}
				}
			}

			return personMergerResult;
		}
	}
}
