using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(UnderbondMovementRequestStatus))]
	sealed class UnderbondMovementRequestStatusTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UnderbondMovementRequestStatus { IsProvisional = true };
		}
	}
}
