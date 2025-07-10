using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Module.Testing
{
	[TestedType(typeof(PickupDeliveryConfirmFilterBusinessObject))]
	public class PickupDeliveryConfirmFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		//Please use filter testers ... see examples in LocalCartage.CartageLegFilterStripBusinessObjectTest

		#region NumbersAndReferencesFilters

		public void TestTransportBookingNoFilter()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.OuterPackLines.AddNew();
			var confirm1 = shipment.PickupConfirms.AddNew();
			confirm1.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);
			var confirm2 = shipment.DeliveryConfirms.AddNew();
			confirm2.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 26);

			var booking = Factory.New<CommonConsolidatedTransportBooking>();
			booking.D1_UniqueConsignRef = "CT000BOOK1";
			confirm1.EU_D1 = booking.PK;

			Factory.Save();

			var filter = new PickupDeliveryConfirmFilterBusinessObject();
			((ModuleTextFilter)filter["Transport Booking #"]).Property = "CT000BOOK1";
			((ModuleTextFilter)filter["Transport Booking #"]).IsActive = true;

			var collection = new CommonPickupDeliveryConfirmCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertCollectionContains(confirm1, collection);
			AssertCollectionNotContains(confirm2, collection);
		}

		#endregion

		#region LocationFilters

		public void TestPickupDeliveryFilter()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.OuterPackLines.AddNew();
			var confirm1 = shipment.PickupConfirms.AddNew();
			confirm1.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);
			var confirm2 = shipment.DeliveryConfirms.AddNew();
			confirm2.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 26);

			var orgAdress = Factory.New<OrgAddress>();
			orgAdress.OA_Address1 = "adr1";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "code1";

			orgAdress.OA_OH = orgHeader.PK;

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = orgAdress.PK;
			jobDocAddress.E2_ParentID = confirm1.PK;

			Factory.Save();

			var filter = new PickupDeliveryConfirmFilterBusinessObject();
			((ModuleGuidFilter)filter["Pickup/Delivery"]).Property = orgHeader.PK;
			((ModuleGuidFilter)filter["Pickup/Delivery"]).IsActive = true;

			var collection = new CommonPickupDeliveryConfirmCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertCollectionContains(confirm1, collection);
			AssertCollectionNotContains(confirm2, collection);
		}

		public void TestRelatedPort()
		{
			TestJobDocAddressTextFilter("Related Port", OrgAddressSchema.OA_RL_NKRelatedPortCode, "JPOSA", "USLAX");
			TestJobDocAddressTextFilter("Related Port", OrgHeaderSchema.OH_RL_NKClosestPort, "JPOSA", "USLAX");
		}

		#endregion

		#region FilterTesters

		#region TextFilterTesters

		#region JobDocAddress

		void TestJobDocAddressTextFilter(ZString filterName, SchemaColumn orgAddressHeaderDocAddressSchemaColumn, ZString value1, ZString value2)
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.OuterPackLines.AddNew();
			var confirm1 = shipment.PickupConfirms.AddNew();
			confirm1.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);
			var confirm2 = shipment.DeliveryConfirms.AddNew();
			confirm2.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 26);

			bool overrideDocAddress = orgAddressHeaderDocAddressSchemaColumn.TableName == JobDocAddressSchema.Constants.TableName;
			var docAddress1 = FreightTestHelper.CreateJobDocAddress(confirm1, DocAddressType.ConsigneePickupDeliveryAddress, "org1", "add1", "2000", "SYDNEY", "AUSYD", overrideDocAddress, Factory);
			SetOnDocAddress(docAddress1, orgAddressHeaderDocAddressSchemaColumn, value1);

			var docAddress2 = FreightTestHelper.CreateJobDocAddress(confirm2, DocAddressType.ConsignorPickupDeliveryAddress, "org2", "add2", "2000", "SYDNEY", "AUSYD", overrideDocAddress, Factory);
			SetOnDocAddress(docAddress2, orgAddressHeaderDocAddressSchemaColumn, value2);
			Factory.Save();

			var filter = new PickupDeliveryConfirmFilterBusinessObject();
			((ModuleTextBaseFilter)filter[filterName]).Property = value2;
			((ModuleTextBaseFilter)filter[filterName]).IsActive = true;

			var collection = new CommonPickupDeliveryConfirmCollection(Factory);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionNotContains("Should not have Cartage1", confirm1, collection);
			AssertCollectionContains("Should have Cartage2", confirm2, collection);

			((ModuleTextBaseFilter)filter[filterName]).Property = value1;
			collection.AdditionalFilter = filter.Filter;
			AssertEquals("Should have 1 CommonCartageLeg", 1, collection.Count);
			AssertCollectionContains("Should have Cartage1", confirm1, collection);
			AssertCollectionNotContains("Should not have Cartage2", confirm2, collection);

			//Don't mess up other tests
			shipment.JS_IsForwardRegistered = false;
			shipment.Delete();
			Factory.Save();
		}

		void SetOnDocAddress(JobDocAddress docAddress, SchemaColumn orgAddressHeaderDocAddressSchemaColumn, ZString value)
		{
			switch (orgAddressHeaderDocAddressSchemaColumn.TableName)
			{
				case JobDocAddressSchema.Constants.TableName:
					docAddress[orgAddressHeaderDocAddressSchemaColumn] = value;
					break;
				case OrgAddressSchema.Constants.TableName:
					docAddress.Address[orgAddressHeaderDocAddressSchemaColumn] = value;
					break;
				case OrgHeaderSchema.Constants.TableName:
					docAddress.Organisation[orgAddressHeaderDocAddressSchemaColumn] = value;
					break;
				default:
					throw new NotSupportedException("Table Name not supported");
			}
		}

		#endregion

		#endregion

		#endregion

		#region Implementaion

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PickupDeliveryConfirmFilterBusinessObject();
		}

		#endregion
	}
}
