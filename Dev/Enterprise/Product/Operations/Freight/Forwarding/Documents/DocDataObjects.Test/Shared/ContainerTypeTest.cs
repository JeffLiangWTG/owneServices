using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(ContainerType))]
	sealed class ContainerTypeTest : NonPersistentBusinessObjectTestCase
	{
		#region TestUpdateISOCodeAndDescriptionWhenCodeChanges

		public void TestUpdateISOCodeAndDescriptionWhenCodeChanges()
		{
			IRefContainerCollection refContainers = new RefContainerCollection(Factory);

			var containerType = new ContainerType(refContainers)
			{
				Code = "20GP"
			};

			AssertEquals("Twenty foot general purpose", containerType.Description);
			AssertEquals("22G0", containerType.ISOCode);

			containerType.Code = "40HC";

			AssertEquals("Forty foot high cube", containerType.Description);
			AssertEquals("45G0", containerType.ISOCode);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			IRefContainerCollection refContainers = new RefContainerCollection(Factory);

			return new ContainerType(refContainers)
			{
				Code = "20GP",
			};
		}

		#endregion
	}
}
