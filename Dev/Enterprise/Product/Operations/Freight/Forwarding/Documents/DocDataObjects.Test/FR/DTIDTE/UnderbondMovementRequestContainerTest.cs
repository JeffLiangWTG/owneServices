using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(UnderbondMovementRequestContainer))]
	sealed class UnderbondMovementRequestContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new CommonContext(Factory);

			return new UnderbondMovementRequestContainer
			{
				Number = "AAAA0000007",
				ECTICTNumber = "ECT0000976",
				ContainerType = new ContainerType(context.ContainerTypes)
				{
					Code = "20GP"
				},
				IsEmptyContainer = false,
				IsNonOperativeReefer = false
			};
		}
	}
}
