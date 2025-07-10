using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccChequeBook)]
	public class AccChequeBookCollection : BusinessObjectCollection<AccChequeBook>
	{
		public AccChequeBookCollection(BusinessObjectFactory factory, AccBankAccount bankAccount) : base(factory, CombineFilterWithStaticFilter(bankAccount))
		{
		}

		public AccChequeBookCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public AccChequeBookCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected static ZQuery CombineFilterWithStaticFilter(AccBankAccount bankAccount)
		{
			ZQuery filter = new ZQuery();
			if (bankAccount != null)
			{
				filter.AddToFilter(AccChequeBookSchema.AK_AB, SQLComparisonOperator.Equal, bankAccount.PK);
			}
			else
			{
				filter.AddToFilter(AccChequeBookSchema.AK_AB, SQLComparisonOperator.Equal, ZGuid.NewZGuid());//to prevent displaying things
			}

			return filter;
		}
	}
}
