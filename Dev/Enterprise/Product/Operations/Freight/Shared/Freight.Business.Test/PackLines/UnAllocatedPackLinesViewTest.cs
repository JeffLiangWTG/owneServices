namespace Enterprise.Freight.Business.Testing
{
	sealed class UnAllocatedPackLinesViewTest : BaseFreightTest
	{
		#region TestIsInTheCollection

		public void TestIsInTheCollection()
		{
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();

			CommonShipment receivedShipment = Factory.New<CommonShipment>();
			receivedShipment.JS_IsBooking = true;
			receivedShipment.JS_IsForwardRegistered = false;
			receivedShipment.JS_InterimReceipt = "snth";
			receivedShipment.JS_JX = Factory.New<JobSailing>().PK;

			CommonShipment nonReceivedShipment = Factory.New<CommonShipment>();
			nonReceivedShipment.JS_IsBooking = true;
			nonReceivedShipment.JS_IsForwardRegistered = false;
			nonReceivedShipment.JS_JX = Factory.New<JobSailing>().PK;

			PackLine receivedPackLine = receivedShipment.OuterPackLines.AddNew();
			PackLine nonReceivedPackLine = nonReceivedShipment.OuterPackLines.AddNew();
			PackLine unpackedPackLine = Factory.New<CommonShipment>().OuterPackLines.AddNew();

			PackLine packedPackLine = container.Consol.Shipments.AddNew().OuterPackLines.AddNew();
			packedPackLine.SetContainer(container.Consol, container);

			PackLineNonDependentCollection packLines = new PackLineNonDependentCollection(Factory);
			packLines.Add(receivedPackLine);
			packLines.Add(nonReceivedPackLine);
			packLines.Add(packedPackLine);
			packLines.Add(unpackedPackLine);

			UnAllocatedPackLinesViewForTest collection = new UnAllocatedPackLinesViewForTest(packLines);
			collection.ShowOnlyReceived = false;
			AssertCollectionContains("should include receivedPackLine", receivedPackLine, collection);
			AssertCollectionContains("should include nonReceivedPackLine", nonReceivedPackLine, collection);
			AssertCollectionContains("should include unpackedPackLine", unpackedPackLine, collection);
			AssertCollectionNotContains("should not inculde packedPackLine", packedPackLine, collection);

			collection.ShowOnlyReceived = true;
			AssertCollectionContains("should include receivedPackLine", receivedPackLine, collection);
			AssertCollectionNotContains("should not include nonReceivedPackLine", nonReceivedPackLine, collection);
			AssertCollectionContains("should include unpackedPackLine", unpackedPackLine, collection);
			AssertCollectionNotContains("should not inculde packedPackLine", packedPackLine, collection);
		}

		#endregion
	}
}
