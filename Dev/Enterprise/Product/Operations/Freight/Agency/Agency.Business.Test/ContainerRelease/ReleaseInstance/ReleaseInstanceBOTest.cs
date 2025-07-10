using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ReleaseInstance))]
	internal class ReleaseInstanceBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.BookedContainers.AddNew().JC_ReleaseNum = "Ref-1";
			ReleaseHeader header = new ReleaseHeader(shipment, true);
			header.Init();
			ReleaseInstance instance = header.Instances.AddNew();
			instance.ReleaseNumber = "Ref-1";
			return instance;
		}
		#endregion
	}
}
