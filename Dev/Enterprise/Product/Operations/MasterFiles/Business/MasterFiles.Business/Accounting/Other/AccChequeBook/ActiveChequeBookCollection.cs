
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveChequeBookCollection : AccChequeBookCollection
	{
		public ActiveChequeBookCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, GetCombinedFilter(filter, null, ZBool.False))
		{
		}

		public ActiveChequeBookCollection(BusinessObjectFactory factory)
			: base(factory, GetCombinedFilter(null, null, ZBool.False))
		{
		}

		public ActiveChequeBookCollection(BusinessObjectFactory factory, AccBankAccount bankAccount)
			: base(factory, GetCombinedFilter(null, bankAccount, ZBool.True))
		{
		}

		protected static ZQuery GetCombinedFilter(ZQuery initialFilter, AccBankAccount bankAccount, ZBool runningFromConstructorWithBankAccount)
		{
			ZQuery result = initialFilter;
			if (result == null)
			{
				result = new ZQuery(AccChequeBookSchema.AK_IsActive, ZBool.True);
			}
			else
			{
				result.AddToFilter(AccChequeBookSchema.AK_IsActive, ZBool.True);
			}

			if (bankAccount != null)
			{
				result.AddToFilter(AccChequeBookSchema.AK_AB, SQLComparisonOperator.Equal, bankAccount.PK);
			}
			else if (runningFromConstructorWithBankAccount)
			{
				result.AddToFilter(AccChequeBookSchema.AK_AB, SQLComparisonOperator.Equal, ZGuid.NewZGuid());//to prevent displaying things
			}

			return result;
		}
	}
}
