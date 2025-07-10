using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(UnderbondMovementRequestAuthorizationRequired))]
	sealed class UnderbondMovementRequestAuthorizationRequiredTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UnderbondMovementRequestAuthorizationRequired { IsPhytosanitary = true };
		}
	}
}
