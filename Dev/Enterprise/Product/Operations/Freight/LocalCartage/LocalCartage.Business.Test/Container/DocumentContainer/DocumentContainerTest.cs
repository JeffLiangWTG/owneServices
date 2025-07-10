using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business
{
	[TestedType(typeof(DocumentContainer))]
	public class DocumentContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			return new DocumentContainer(container);
		}
	}
}
