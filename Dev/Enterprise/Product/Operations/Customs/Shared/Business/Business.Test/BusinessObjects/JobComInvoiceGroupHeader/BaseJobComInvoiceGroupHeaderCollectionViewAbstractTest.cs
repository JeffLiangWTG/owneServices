using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseJobComInvoiceGroupHeaderCollectionViewAbstractTest<T> : BusinessObjectCollectionViewTestCase<T> where T : BaseJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<BaseJobComInvoiceGroupHeader>();
			result.JZ_JE = declaration.PK;
			result.JZ_JZ_GroupInvoiceFK = invoiceGroupHeader.PK;
			return result;
		}

		protected override T GetCollectionToTest() => (T)Activator.CreateInstance(typeof(T), invoiceGroupHeader);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
		}
		BaseJobDeclaration declaration;
		BaseJobComInvoiceGroupHeader invoiceGroupHeader;
	}
}
