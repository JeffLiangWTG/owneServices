using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderToPrintEventArgsTest : WhsDocketToPrintEventArgsTest
	{
		public void TestLocalConstructor()
		{
			var order = Factory.New<WhsOrder>();
			var pickableCollection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
			pickableCollection.Add(order);
			var labelLines = new DeliveryLabelLineCollection(pickableCollection, Factory);
			var docketsLabelControl = new WhsDocketsLabelControl(labelLines);
			var e = new WhsOrderToPrintEventArgs(docketsLabelControl, Core.Constants.DataContext.WhsDeliveryLabels);
			AssertSame("DocketDocument must be the same as EventArgs DocketDocument", docketsLabelControl, e.DocketsLabelControl);
			AssertEquals("DataContext", Core.Constants.DataContext.WhsDeliveryLabels, e.DataContext);
			AssertEquals("Continue to print is true", false, e.ContinueToPrint);

			order.WD_ExternalReference = "ExtRef";
			order.WD_PackagesSent = 10;
			var line = order.Lines.AddNew();
			line.WE_PackQuantity = 10;
			labelLines = new DeliveryLabelLineCollection(pickableCollection, Factory);
			docketsLabelControl = new WhsDocketsLabelControl(labelLines);
			e = new WhsOrderToPrintEventArgs(docketsLabelControl, Core.Constants.DataContext.WhsDeliveryLabels);
			AssertEquals("Continue to print is true", true, e.ContinueToPrint);
		}

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsOrder>();
		}

		protected override WhsDocketToPrintEventArgs GetNewDocketToPrintEventArgs(WhsDocketLabelControl docketDocument, Core.Constants.DataContext dataContext)
		{
			return new WhsOrderToPrintEventArgs(docketDocument, dataContext);
		}

		protected override Core.Constants.DataContext GetDataContextToTest()
		{
			return Enterprise.Core.Constants.DataContext.WhsDeliveryLabels;
		}

		#endregion
	}
}
