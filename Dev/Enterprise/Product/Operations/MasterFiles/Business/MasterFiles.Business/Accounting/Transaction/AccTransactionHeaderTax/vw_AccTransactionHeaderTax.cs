using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class vw_AccTransactionHeaderTax : Autovw_AccTransactionHeaderTax
	{
		public vw_AccTransactionHeaderTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
