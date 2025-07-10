using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseGroupInvoiceDirectChildInvoiceHeaderCollectionTest<T> : ActiveBusinessObjectCollectionTestCase<T> where T : InvoiceHeaderActiveCollection
	{
		protected override T GetCollectionToTest()
		{
			return (T)GroupHeader.JobComInvoiceHeaders;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceHeader result = Factory.New<BaseJobComInvoiceHeader>();
			result.JZ_JE = Declaration.PK;
			result.JZ_JZ_GroupInvoiceFK = GroupHeader.PK;
			return result;
		}

		BaseJobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<BaseJobDeclaration>()); }
		}
		BaseJobDeclaration declaration;

		BaseJobComInvoiceGroupHeader GroupHeader
		{
			get { return groupHeader ?? (groupHeader = Declaration.JobComInvoiceGroupHeaders[0]); }
		}
		BaseJobComInvoiceGroupHeader groupHeader;
	}
}
