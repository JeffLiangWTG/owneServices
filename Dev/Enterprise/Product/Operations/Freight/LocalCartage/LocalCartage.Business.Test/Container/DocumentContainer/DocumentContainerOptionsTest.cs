using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business
{
	[TestedType(typeof(DocumentContainerOptions))]
	public class DocumentContainerOptionsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cartage = Factory.New<CommonCartage>();
			var legs = new DocumentContainerCollection(cartage.Containers, Factory);
			return new DocumentContainerOptions(legs);
		}
	}
}
