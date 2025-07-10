using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(SelectOrdersForm))]
	public class SelectOrdersFormTest : ZFormBasherTest
	{
		public void TestShowFormParameter()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => SelectOrdersForm.ShowForm(null));
			AssertNoExceptionThrown(() => SelectOrdersForm.ShowForm(Factory.New<ForwardingShipment>()));
		}

		[RequiresSTA]
		public void TestSelectButton_NoOrderLineToPackLineConversion()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.JD_TransportMode = Constants.TransportModes.Sea;
			order.JD_ContainerMode = Constants.ContainerModes.FCL;
			order.SupplierPK = supplier.PK;
			order.BuyerPK = buyer.PK;
			order.JD_ActualVolume = 99;
			order.JD_ActualWeight = 88;
			order.JD_Packs = 10;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsignorPK = supplier.PK;
			shipment.ConsigneePK = buyer.PK;

			AssertEquals(0, shipment.GenericOrders.Count); //Need to initialise generic orders collection

			using (var form = new SelectOrdersFormForTesting(shipment))
			{
				form.Show();
				((ZButton)form.Controls.Find("SelectButton", true)[0]).PerformClick();
			}

			AssertEquals(1, shipment.GenericOrders.Count);
			AssertEquals(1, shipment.OuterPackLines.Count);

			var convertedPackline = (ForwardingPackLine)shipment.OuterPackLines.First();

			AssertEquals((Decimal)99, convertedPackline.JL_ActualVolume);
			AssertEquals((Decimal)88, convertedPackline.JL_ActualWeight);
			AssertEquals(10, convertedPackline.JL_PackageCount);
		}

		public void TestSelectButton_OrderLineToPackLineConversion()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.JD_TransportMode = Constants.TransportModes.Sea;
			order.JD_ContainerMode = Constants.ContainerModes.FCL;
			order.SupplierPK = supplier.PK;
			order.BuyerPK = buyer.PK;
			order.JD_ActualVolume = 99;
			order.JD_ActualWeight = 88;
			order.JD_Packs = 10;

			var orderline = order.OrderLines.AddNew();
			orderline.JO_OuterPackLength = 30;
			orderline.JO_OuterPackWidth = 20;
			orderline.JO_OuterPackHeight = 10;
			orderline.JO_OuterPackUnitOfDimension = Core.Constants.Length.Centimetres;
			orderline.JO_UnitOfVolume = Core.Constants.Volume.CubicCentimeters;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsignorPK = supplier.PK;
			shipment.ConsigneePK = buyer.PK;
			shipment.OnOrderLineToPackLineConversion += (sender, e) => e.ShouldCreatePacklines = true;

			AssertEquals(0, shipment.GenericOrders.Count); //Need to initialise generic orders collection

			using (var form = new SelectOrdersFormForTesting(shipment))
			{
				form.Show();
				((ZButton)form.Controls.Find("SelectButton", true)[0]).PerformClick();
			}

			AssertEquals(1, shipment.GenericOrders.Count);
			AssertEquals(1, shipment.OuterPackLines.Count);

			var convertedPackline = (ForwardingPackLine)shipment.OuterPackLines.First();

			AssertEquals((Decimal)30, convertedPackline.JL_Length);
			AssertEquals((Decimal)20, convertedPackline.JL_Width);
			AssertEquals((Decimal)10, convertedPackline.JL_Height);
			AssertEquals(10, convertedPackline.JL_PackageCount); //pack line count will also change if there is only 1 packline
			AssertEquals((Decimal)60000, convertedPackline.JL_ActualVolume);
			AssertEquals("CM", convertedPackline.JL_UnitOfDimension);
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return new SelectOrdersFormForTesting(shipment);
		}

		class SelectOrdersFormForTesting : SelectOrdersForm
		{
			public SelectOrdersFormForTesting(ForwardingShipment shipment)
				: base(shipment)
			{
			}
		}

		#endregion
	}
}
