using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionHeaderFiscalization : AutoAccTransactionHeaderFiscalization
	{
		public AccTransactionHeaderFiscalization(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
