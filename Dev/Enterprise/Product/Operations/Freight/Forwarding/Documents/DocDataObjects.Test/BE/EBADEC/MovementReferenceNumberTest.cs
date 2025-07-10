using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	[TestedType(typeof(MovementReferenceNumber))]
	sealed class MovementReferenceNumberTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var mrn = new MovementReferenceNumber(1)
			{
				MRN = "MRN123",
				CustomsDocumentCode = new DummyCodeDescription { Code = "EXS", Description = "Exit Summary Declaration" },
				CustomsOfficeCode = "BE1010"
			};

			return mrn;
		}
	}
}
