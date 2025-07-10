using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(UnderbondMovementRequest))]
	sealed class UnderbondMovementRequestTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var request = new UnderbondMovementRequest("ForwardingConsol", "C00001278");
			request.Containers = new List<UnderbondMovementRequestContainer>();
			return request;
		}
	}
}
