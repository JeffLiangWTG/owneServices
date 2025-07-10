using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusTransactionNumber : AutoCusTransactionNumber
	{
		public CusTransactionNumber(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
