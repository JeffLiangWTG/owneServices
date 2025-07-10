using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Services.ServiceHost.Tests
{
	sealed class EAdaptorSupportMessageSenderTest : TestCaseWithFactory
	{
		public void TestIsRegisteredInObjectFactory()
		{
			var sender = ObjectFactory.New<IEAdaptorSupportMessageSender>();
			AssertNotNull("eAdaptor sender should be accessible via ObjectFactory", sender);
		}

		public void TestSend()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			shipment.JS_TransportMode = "SEA";
			Factory.Save();

			const string universalXmlRequest =
@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S0001010</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>";

			var sender = new EAdaptorSupportMessageSender();
			var res = sender.Send(universalXmlRequest);

			AssertContains("Expected to generate UniversalShipment for ForwardingShipment S0001010", "UniversalResponse", res);
		}

		public void TestSendResetsUserContext()
		{
			const string testXml = @"
<UniversalShipment>
</UniversalShipment>";

			var companyPK = Factory.New<GlbCompany>().PK;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = companyPK;
			var branchPK = branch.PK;
			var departmentPK = Factory.New<GlbDepartment>().PK;
			var userPK = Factory.New<GlbStaff>().PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(userPK.ToGuid(), branchPK.ToGuid(), departmentPK.ToGuid()))
			{
				var oldContext = Env.CurrentUserContext;
				var sender = new EAdaptorSupportMessageSender();
				sender.Send(testXml);
				var newContext = Env.CurrentUserContext;

				CombineAssertions(() =>
				{
					AssertEquals("User context should be reset", oldContext, newContext);
					AssertEquals("Company should be reset", companyPK, newContext.Company.PK);
					AssertEquals("Branch should be reset", branchPK, newContext.Branch.PK);
					AssertEquals("Department should be reset", departmentPK, newContext.Department.PK);
					AssertEquals("User should be reset", userPK, newContext.User.PK);
				});
			}
		}
	}
}
