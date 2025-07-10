using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class UnAllocatedPackLinesForSailingTest : BaseFreightTest
	{
		#region TestShouldIncludeThisPackLine

		public void TestShouldIncludeThisPackLine()
		{
			JobSailing sailing1 = CreateSailing(HomePort, OverseasPort);
			JobSailing sailing2 = CreateSailing(HomePort, OverseasPort);
			JobSailing sailing3 = CreateSailing(HomePort, OverseasPort2);

			Factory.Save();

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_IsForwardRegistered = false;
			shipment1.JS_IsBooking = true;
			shipment1.JS_RL_NKOrigin = sailing1.JX_JA_RL_NKPortOfLoading;
			shipment1.JS_RL_NKDestination = sailing1.JX_JB_RL_NKPortOfDischarge;
			shipment1.JS_JX = sailing1.PK;

			PackLine packLine1 = shipment1.OuterPackLines.AddNew();

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_IsForwardRegistered = false;
			shipment2.JS_IsBooking = true;
			shipment2.JS_RL_NKOrigin = sailing2.JX_JA_RL_NKPortOfLoading;
			shipment2.JS_RL_NKDestination = sailing2.JX_JB_RL_NKPortOfDischarge;
			shipment2.JS_JX = sailing2.PK;

			PackLine packLine2 = shipment2.OuterPackLines.AddNew();

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_IsForwardRegistered = false;
			shipment3.JS_IsBooking = true;
			shipment3.JS_RL_NKOrigin = sailing3.JX_JA_RL_NKPortOfLoading;
			shipment3.JS_RL_NKDestination = sailing3.JX_JB_RL_NKPortOfDischarge;
			shipment3.JS_JX = sailing3.PK;

			PackLine packLine3 = shipment3.OuterPackLines.AddNew();

			JobSailingCollection sailingCollection = new JobSailingCollection(Factory);
			sailingCollection.Add(sailing1);
			sailingCollection.Add(sailing2);
			sailingCollection.Add(sailing3);

			PackLinesForSailingsCollection packLines = new PackLinesForSailingsCollection(sailingCollection);
			packLines.Load();

			UnAllocatedPackLinesForSailing collection = new UnAllocatedPackLinesForSailing(packLines, sailing1);
			collection.ShowOnlyReceived = false;
			collection.ShowOnlyNonTranship = false;
			collection.ShowOnlyThisSailing = false;

			AssertCollectionContains("should contain packLine1", packLine1, collection);
			AssertCollectionContains("should contain packLine2", packLine2, collection);
			AssertCollectionContains("should contain packLine3", packLine3, collection);

			collection.ShowOnlyNonTranship = true;
			AssertCollectionContains("should contain packLine1", packLine1, collection);
			AssertCollectionContains("should contain packLine2", packLine2, collection);
			AssertCollectionNotContains("should not contain packLine3", packLine3, collection);

			collection.ShowOnlyThisSailing = true;
			AssertCollectionContains("should contain packLine1", packLine1, collection);
			AssertCollectionNotContains("should not contain packLine2", packLine2, collection);
			AssertCollectionNotContains("should not contain packLine3", packLine3, collection);
		}

		#endregion

		#region Implementation

		JobSailing CreateSailing(ZString loadPort, ZString dischargePort)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = loadPort;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = dischargePort;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#endregion
	}
}
