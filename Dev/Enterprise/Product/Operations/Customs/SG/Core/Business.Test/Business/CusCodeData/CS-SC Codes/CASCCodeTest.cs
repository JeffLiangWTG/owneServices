using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class CASCCodeTest<T> : Customs.Business.Testing.CusCodeDataTest<T> where T : CASCCode
	{
		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<T>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var parent = invoice.JobComInvoiceLines.AddNew();
			var result = factory.New<T>();
			result.Parent = parent;
			return result;
		}
		#endregion
	}
}
