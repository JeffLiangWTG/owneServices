using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLineDissectionAttributeCollection : DependentBusinessObjectCollection<AccTransactionLineDissectionAttribute, AccTransactionLines>
	{
		public AccTransactionLineDissectionAttributeCollection(AccTransactionLines transactionLine) : base(transactionLine)
		{
		}

		protected override string FkColumnName => AccTransactionLineDissectionAttributeSchema.ALD_AL_TransactionLine.Name;

		protected override bool AllowNewCore => false;
	}
}
