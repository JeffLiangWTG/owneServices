using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(UnderbondMovementRequestAuthorizationResponseType))]
	sealed class UnderbondMovementRequestAuthorizationResponseTypeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UnderbondMovementRequestAuthorizationResponseType { IsAgreement = true };
		}
	}
}
