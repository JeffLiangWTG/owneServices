using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(UpdateLastKnownTransitWarehouseApplicator))]
	public class UpdateLastKnownTransitWarehouseApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestApplyCore()
		{
			var expectedAddress = Factory.NewWithValidTestData<OrgAddress>();
			expectedAddress.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			expectedAddress.OA_Code = "blerbity";

			var expectedStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched;
			var expectedDate = ZDateTime.Now.AddDays(-1);

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_OA_ExportReceivingDepot = expectedAddress.PK;

			var shipment1PackLine1 = shipment1.OuterPackLines.AddNew();
			var shipment1PackLine2 = shipment1.OuterPackLines.AddNew();
			shipment1PackLine2.JL_OA_LastKnownTransitWarehouseAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipment1PackLine2.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			shipment1PackLine2.JL_LastKnownTransitWarehouseStatusDateTime = ZDateTime.Now.AddDays(-5);

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2PackLine1 = shipment2.OuterPackLines.AddNew();
			var shipment2PackLine2 = shipment2.OuterPackLines.AddNew();
			shipment2PackLine2.JL_OA_LastKnownTransitWarehouseAddress = expectedAddress.PK;
			shipment2PackLine2.JL_LastKnownTransitWarehouseStatus = expectedStatus;
			shipment2PackLine2.JL_LastKnownTransitWarehouseStatusDateTime = expectedDate;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();

			Factory.Save();

			Applicator.TransitWarehouseAddressPK = expectedAddress.PK;
			Applicator.TransitWarehouseStatus = expectedStatus;
			Applicator.TransitWarehouseStatusDateTime = expectedDate;

			ApplyApplicator(new BusinessObject[] { shipment1, shipment2, shipment3 }, $@"
INFO: Processing shipment {shipment1.JS_UniqueConsignRef}:
INFO: 	Processing pack line:
INFO: 		Last known transit warehouse address updated.
INFO: 		Last known transit warehouse status updated.
INFO: 		Last known transit warehouse status date updated.
INFO: 	Pack line processed.
INFO: 	Processing pack line:
INFO: 		Last known transit warehouse address updated.
INFO: 		Last known transit warehouse status updated.
INFO: 		Last known transit warehouse status date updated.
INFO: 	Pack line processed.
INFO: Shipment processed.
INFO: Processing shipment {shipment2.JS_UniqueConsignRef}:
INFO: 	Processing pack line:
INFO: 		Last known transit warehouse address updated.
WARNING: 			Warning: Transit Warehouse/CFS Organization selected does not match any of CFS’s on the Shipment or Consol.
INFO: 		Last known transit warehouse status updated.
INFO: 		Last known transit warehouse status date updated.
INFO: 	Pack line processed.
INFO: 	Processing pack line:
INFO: 		Skipped updating last known transit warehouse address as it is the same.
INFO: 		Skipped updating last known transit warehouse status as it is the same.
INFO: 		Skipped updating last known transit warehouse status date as it is the same.
INFO: 	Pack line processed.
INFO: Shipment processed.
INFO: Processing shipment {shipment3.JS_UniqueConsignRef}:
INFO: Shipment processed.");

			AssertEquals(expectedAddress.PK, shipment1PackLine1.JL_OA_LastKnownTransitWarehouseAddress);
			AssertEquals(expectedStatus, shipment1PackLine1.JL_LastKnownTransitWarehouseStatus);
			AssertEquals(expectedDate, shipment1PackLine1.JL_LastKnownTransitWarehouseStatusDateTime);

			AssertEquals(expectedAddress.PK, shipment1PackLine2.JL_OA_LastKnownTransitWarehouseAddress);
			AssertEquals(expectedStatus, shipment1PackLine2.JL_LastKnownTransitWarehouseStatus);
			AssertEquals(expectedDate, shipment1PackLine2.JL_LastKnownTransitWarehouseStatusDateTime);

			AssertEquals(expectedAddress.PK, shipment2PackLine1.JL_OA_LastKnownTransitWarehouseAddress);
			AssertEquals(expectedStatus, shipment2PackLine1.JL_LastKnownTransitWarehouseStatus);
			AssertEquals(expectedDate, shipment2PackLine1.JL_LastKnownTransitWarehouseStatusDateTime);

			AssertEquals(expectedAddress.PK, shipment2PackLine2.JL_OA_LastKnownTransitWarehouseAddress);
			AssertEquals(expectedStatus, shipment2PackLine2.JL_LastKnownTransitWarehouseStatus);
			AssertEquals(expectedDate, shipment2PackLine2.JL_LastKnownTransitWarehouseStatusDateTime);
		}

		public void TestApplyCoreWithValidationError()
		{
			var expectedAddress = Factory.NewWithValidTestData<OrgAddress>();
			expectedAddress.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			expectedAddress.OA_Code = "blerbity";

			var expectedStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched;
			var expectedDate = ZDateTime.Now.AddDays(-1);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OA_ExportReceivingDepot = expectedAddress.PK;

			var shipmentPackLine1 = Factory.New<DummyForwardingPackLine>();
			shipmentPackLine1.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(shipmentPackLine1);

			var shipmentPackLine2 = Factory.New<DummyForwardingPackLine>();
			shipmentPackLine2.JL_FreightMode = FreightConstants.OuterPackType;
			shipmentPackLine2.JL_Description = "invalid";
			shipmentPackLine2.JL_OA_LastKnownTransitWarehouseAddress = expectedAddress.PK;
			shipmentPackLine2.JL_LastKnownTransitWarehouseStatus = expectedStatus;
			shipmentPackLine2.JL_LastKnownTransitWarehouseStatusDateTime = expectedDate;
			shipment.OuterPackLines.Add(shipmentPackLine2);

			Factory.Save();

			Applicator.TransitWarehouseAddressPK = Factory.NewWithValidTestData<OrgAddress>().PK;
			Applicator.TransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			Applicator.TransitWarehouseStatusDateTime = ZDateTime.Now.AddDays(-5);

			ApplyApplicator(new BusinessObject[] { shipment }, $@"
INFO: Processing shipment {shipment.JS_UniqueConsignRef}:
INFO: 	Processing pack line:
INFO: 		Last known transit warehouse address updated.
INFO: 		Last known transit warehouse status updated.
INFO: 		Last known transit warehouse status date updated.
INFO: 	Pack line processed.
INFO: 	Processing pack line:
INFO: 		Last known transit warehouse address updated.
ERROR: 			Error: TEST
ERROR: A save-preventing error occurred while processing the shipments, so the operational action must be aborted.");
		}

		public void TestApplyCoreWithNoShipments()
		{
			ApplyApplicator(Array.Empty<BusinessObject>(), "ERROR: No shipments selected.");
		}

		public void TestValidation()
		{
			Applicator.TransitWarehouseAddressPK = ZGuid.Empty;
			Applicator.TransitWarehouseStatus = ZString.Empty;
			Applicator.TransitWarehouseStatusDateTime = ZDateTime.Empty;

			Applicator.Validation.ValidateAll();

			var hasValueError = "Please enter a value.";

			AssertHasError(Applicator.TransitWarehouseAddressInfo, hasValueError);
			AssertHasError(Applicator.TransitWarehouseStatusInfo, hasValueError);
			AssertHasError(Applicator.TransitWarehouseStatusDateTimeInfo, hasValueError);

			Applicator.TransitWarehouseAddressPK = ZGuid.BrettsGuid;
			Applicator.TransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			Applicator.TransitWarehouseStatusDateTime = new ZDateTime(DateTime.MinValue);

			AssertNoNotifications(Applicator.TransitWarehouseAddressInfo);
			AssertNoNotifications(Applicator.TransitWarehouseStatusInfo);
			AssertHasError(Applicator.TransitWarehouseStatusDateTimeInfo, "Please enter a valid value.");

			Applicator.TransitWarehouseStatusDateTime = ZDateTime.Now.AddDays(1);
			AssertHasWarning(Applicator.TransitWarehouseStatusDateTimeInfo, "Last Known TW Date should only allow current or past date.");

			Applicator.TransitWarehouseStatusDateTime = ZDateTime.Now.AddDays(-1);
			AssertNoNotifications(Applicator.TransitWarehouseStatusDateTimeInfo);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateLastKnownTransitWarehouseApplicator(new BusinessObjectFactory());
		}

		class DummyForwardingPackLine : ForwardingPackLine
		{
			public DummyForwardingPackLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override JobPackLinesValidation GetNewValidation()
			{
				return new TestForwardingPackLineValidation(this);
			}
		}

		class TestForwardingPackLineValidation : ForwardingPackLineValidation
		{
			public TestForwardingPackLineValidation(ForwardingPackLine parent) : base(parent)
			{
			}

			protected override void CheckJL_OA_LastKnownTransitWarehouseAddress()
			{
				if (Parent.JL_Description.Equals("invalid"))
				{
					Parent.JL_OA_LastKnownTransitWarehouseAddressInfo.AddError("TEST");
				}
			}
		}

		#endregion

		new UpdateLastKnownTransitWarehouseApplicator Applicator => (UpdateLastKnownTransitWarehouseApplicator)base.Applicator;
	}
}
