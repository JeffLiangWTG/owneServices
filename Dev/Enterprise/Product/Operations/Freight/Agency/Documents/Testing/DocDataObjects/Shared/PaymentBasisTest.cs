using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(PaymentBasis))]
	class PaymentBasisTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PaymentBasis();
		}
	}
}
