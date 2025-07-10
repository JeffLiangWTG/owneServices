using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceRelatedDeclarationGenPivot))]
	sealed class InvoiceRelatedDeclarationGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<InvoiceRelatedDeclarationGenPivot>();
		}
	}
}
