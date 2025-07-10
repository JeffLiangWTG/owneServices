using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects.Testing
{
	[TestedType(typeof(CIN750CustomsDocument))]
	sealed class CIN750CustomsDocumentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bizO = new CIN750CustomsDocument();

			return bizO;
		}
	}
}
