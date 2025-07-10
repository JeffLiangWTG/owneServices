using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortMessageHostCollection))]
	internal class PortMessageHostCollectionBOTest : NonPersistentBusinessObjectCollectionTestCase<PortMessageHostCollection>
	{
		#region Implementation
		protected override PortMessageHostCollection GetCollectionToTest()
		{
			return new PortMessageHostCollection(Factory.New<JobVoyage>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PortMessageHost(Factory.New<JobVoyage>().Origins.AddNew());
		}
		#endregion
	}
}
