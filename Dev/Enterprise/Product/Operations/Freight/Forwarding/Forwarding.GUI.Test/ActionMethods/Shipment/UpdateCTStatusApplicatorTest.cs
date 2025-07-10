using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(UpdateCTStatusApplicator))]
	public class UpdateCTStatusApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestUpdateCTStatus()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_UniqueConsignRef = "S00001001";
			shipment1.JS_CommunityTransitStatus = "";

			shipment2.JS_UniqueConsignRef = "S00002002";
			shipment2.JS_CommunityTransitStatus = "X";

			shipment3.JS_UniqueConsignRef = "S00003003";
			shipment3.JS_CommunityTransitStatus = "C";

			Factory.Save();

			Applicator.StatusCode = "X";

			ApplyApplicator(new BusinessObject[] { shipment1, shipment2, shipment3 }, @"INFO: Processing shipment S00001001:
INFO: 	Shipment processed.
INFO: Processing shipment S00002002:
INFO: 	Skipped, because CT Status is the same.
INFO: Processing shipment S00003003:
INFO: 	Shipment processed.");

			AssertEquals("X", shipment1.JS_CommunityTransitStatus);
			AssertEquals("X", shipment2.JS_CommunityTransitStatus);
			AssertEquals("X", shipment3.JS_CommunityTransitStatus);
		}

		public void TestUpdateCTStatusValidation()
		{
			Applicator.StatusCode = ZString.Empty;
			AssertNoErrors("Blank status is allowed", Applicator.StatusCodeInfo);

			Applicator.StatusCode = "CCC";
			AssertHasError(Applicator.StatusCodeInfo, "Enter a valid selection.");

			Applicator.StatusCode = "X";
			AssertNoErrors(Applicator.StatusCodeInfo);
		}

		public void TestStatusCodeList()
		{
			Assert(Applicator.StatusCodeList.ContainsCode(""));
			Assert(Applicator.StatusCodeList.ContainsCode("T1"));
			Assert(Applicator.StatusCodeList.ContainsCode("X"));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateCTStatusApplicator();
		}

		new UpdateCTStatusApplicator Applicator => (UpdateCTStatusApplicator)base.Applicator;
	}
}
