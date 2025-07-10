using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(CustomsDeclarationCheck))]
	sealed class CustomsDeclarationCheckTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var customsDeclarationCheck = new CustomsDeclarationCheck("ForwardingShipment", "S00001001");

			customsDeclarationCheck.Containers = System.Array.Empty<Container>();

			return customsDeclarationCheck;
		}
	}
}
