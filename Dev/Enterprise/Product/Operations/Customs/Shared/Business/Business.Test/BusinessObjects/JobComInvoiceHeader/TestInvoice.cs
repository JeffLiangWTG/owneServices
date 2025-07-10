using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TestInvoice : BaseJobComInvoiceHeader
	{
		public TestInvoice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		internal bool CheckJZ_Calc_TNICalled;
		protected override JobComInvoiceHeaderValidation GetNewValidation()
		{
			return new TestValidation(this);
		}
	}
}
