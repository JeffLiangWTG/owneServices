using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingDeclaration))]
	sealed class TrackingDeclarationTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Milestones

		public void TestMilestones()
		{
			var testDeclaration = new TrackingDeclaration(Factory.NewWithValidTestData<BaseJobDeclaration>());
			AssertNotNull(testDeclaration.Milestones);
			AssertEquals(0, testDeclaration.Milestones.Count);

			var milestone1 = testDeclaration.Declaration.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testDeclaration.Declaration.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			AssertEquals(0, testDeclaration.Milestones.Count);

			testDeclaration.ReloadMilestones();
			AssertEquals(2, testDeclaration.Milestones.Count);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TrackingDeclaration(TestJobDeclaration);
		}

		BusinessObjectFactory WebFactory
		{
			get { return Factory; }
		}

		#endregion Overrides

		#region Setup

		TrackingDeclaration TestDeclaration;
		ForwardingShipment TestShipment;

		protected override void SetUp()
		{
			base.SetUp();

			TestDeclaration = (TrackingDeclaration)GetNewBusinessObject();
			TestShipment = Factory.NewWithValidTestData<ForwardingShipment>();

			SetUpDocumentFactory();
		}

		BaseJobDeclaration TestJobDeclaration
		{
			get
			{
				if (fTestJobDeclaration == null)
				{
					fTestJobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				}
				return fTestJobDeclaration;
			}
		}
		BaseJobDeclaration fTestJobDeclaration;

		#region Document Factory Setup

		void SetUpDocumentFactory()
		{
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			Parent = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			Parent.SM_Type = "ORG";
			Parent.SM_DB = 1;
		}

		DocumentFactory MasterFactory;
		StorageMain Parent;

		#endregion Document Factory Setup

		#endregion Setup

		#region Declaration

		public void TestDeclarationNumber()
		{
			AssertNotNull(TestDeclaration.Declaration);
			AssertEquals(TestJobDeclaration, TestDeclaration.Declaration);
		}

		#endregion Declaration

		#region OrderReference

		public void TestOrderReference()
		{
			AssertEquals("initially blank", ZString.Empty, TestDeclaration.OrderReference);

			TestDeclaration.Declaration.JE_OwnerRef = "ABC";
			AssertEquals("", "ABC", TestDeclaration.OwnerReference);
			AssertEquals("", "", TestDeclaration.OrderReference);
			AssertEquals("", "", TestDeclaration.BookingReference);

			TestDeclaration.Declaration.DocsAndCartage.JP_OrderItemsAsString = "BCD,XYZ";
			AssertEquals("", "ABC", TestDeclaration.OwnerReference);
			AssertEquals("", "BCD, XYZ", TestDeclaration.OrderReference);
			AssertEquals("", "", TestDeclaration.BookingReference);

			TestDeclaration.Declaration.JE_OwnerRef = "";
			AssertEquals("", "", TestDeclaration.OwnerReference);
			AssertEquals("", "BCD, XYZ", TestDeclaration.OrderReference);

			TestDeclaration.Declaration.DocsAndCartage.JP_OrderItemsAsString = "";
			Order order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "AO1";
			order1.JD_JS = TestShipment.PK;
			TestDeclaration.Declaration.AttachedOrders.Add(order1);

			AssertEquals(1, TestDeclaration.Declaration.AttachedOrders.Count);
			AssertEquals("AO1", TestDeclaration.OrderReference);
		}

		#endregion

		#region Shipment Type

		public void TestShipmentType()
		{
			AssertEquals(TestDeclaration.ShipmentType, ZString.Empty);
			TestDeclaration.Declaration.JE_JS = TestShipment.PK;
			TestShipment.JS_ShipmentType = "ASM";
			AssertEquals(TestDeclaration.ShipmentType, TestShipment.JS_ShipmentType);
		}

		#endregion

		public void TestStorageDate()
		{
			AssertEquals(TestDeclaration.StorageDate, ZDateTime.Empty);
			AssertEquals(TestDeclaration.StorageDateInfo.Name, ShipmentDeclarationSchema.Constants.StorageDate);
		}

		#region TestRelatedTransportBookingPKsAdded

		public void TestRelatedTransportBookingPKsAdded()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var trackingDeclaration = new TrackingDeclaration(declaration);

			AssertEquals("Precondition: DocRelatedPKs should return no PKs at this stage.", 0, trackingDeclaration.DocRelatedPKs.Count);

			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_ParentID = declaration.PK;
			consolidation.KB_ParentTableCode = declaration.TablePrefix;

			var booking1 = Factory.NewWithValidTestData<DtbBooking>();
			var booking2 = Factory.NewWithValidTestData<DtbBooking>();
			var booking3 = Factory.NewWithValidTestData<DtbBooking>();

			consolidation.Bookings.AddRange(new List<DtbBooking> { booking1, booking2, booking3 });

			Factory.Save();

			AssertEquals("DocRelatedPKs should return 3 PKs.", 3, trackingDeclaration.DocRelatedPKs.Count);
			AssertContainsExactElementsInAnyOrder("DocRelatedPKs should return all 3 booking PKs", new[] { booking1.PK, booking2.PK, booking3.PK }, trackingDeclaration.DocRelatedPKs);
		}

		#endregion

		public void TestDocRelatedPKs()
		{
			var bizO1 = TestDeclaration.Declaration.Invoices.AddNew();
			var bizO2 = TestDeclaration.Declaration.CustomsEntryHeaders.AddNew();
			var landedCostHeader = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = TestDeclaration.Declaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = JobDeclarationSchema.Constants.Prefix;

			TestDeclaration.Declaration.JE_JS = TestShipment.PK;

			AssertEquals("Should have 3 related objects", TestDeclaration.DocRelatedPKs.Count, 4);
			AssertEquals(TestDeclaration.DocRelatedPKs[0], bizO2.PK);
			AssertEquals(TestDeclaration.DocRelatedPKs[1], bizO1.PK);
			AssertEquals(TestDeclaration.DocRelatedPKs[2], landedCostHeader.PK);
			AssertEquals(TestDeclaration.DocRelatedPKs[3], TestShipment.PK);
		}

		public void TestDeliveryAddressAsText()
		{
			AssertEquals("initially blank", ZString.Empty, TestDeclaration.DeliveryAddressAsText);

			OrgAddress address1 = Factory.New<OrgAddress>();
			address1.OA_State = "NSW";
			TestDeclaration.Declaration.ImporterDeliveryAddress.E2_OA_Address = address1.PK;
			AssertEquals(TestDeclaration.Declaration.ImporterDeliveryAddress.Address.AddressAsASingleLine, TestDeclaration.DeliveryAddressAsText);
		}

		public void TestPickupAddressAsText()
		{
			AssertEquals("initially blank", ZString.Empty, TestDeclaration.PickupAddressAsText);

			OrgAddress address1 = Factory.New<OrgAddress>();
			address1.OA_State = "NSW";
			TestDeclaration.Declaration.SupplierPickupAddress.E2_OA_Address = address1.PK;
			AssertEquals(TestDeclaration.Declaration.SupplierPickupAddress.Address.AddressAsASingleLine, TestDeclaration.PickupAddressAsText);
		}

		public void TestDateAtOriginWithSuppression()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, false);
				TrackingDeclaration dec1 = (TrackingDeclaration)GetNewBusinessObject();
				AssertEquals("initially it's blank", ZDateTime.Empty, dec1.DateAtOriginWithSuppression);
				dec1.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				ZDateTime testDate = ZDateTime.Now.AddDays(1);
				dec1.Declaration.JE_DateAtOrigin = testDate;
				AssertEquals("Should be set to date string", testDate.ToLongTimeString(), dec1.DateAtOriginWithSuppression.ToLongTimeString());

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, true);
				TrackingDeclaration dec2 = (TrackingDeclaration)GetNewBusinessObject();
				dec2.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				dec2.Declaration.JE_DateAtOrigin = testDate;
				AssertEquals("Should be suppressed", Suppression.SuppressedDate, dec2.DateAtOriginWithSuppression);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestExportDateWithSuppression()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				SuppressionTest.SetSuppressingFields(RegistryItem, false);
				TrackingDeclaration dec1 = (TrackingDeclaration)GetNewBusinessObject();
				AssertEquals("initially it's blank", ZDateTime.Empty, dec1.ExportDateWithSuppression);
				dec1.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				ZDateTime testDate = ZDateTime.Now.AddDays(1);
				dec1.Declaration.JE_ExportDate = testDate;
				AssertEquals("Should be set to date string", testDate.ToLongTimeString(), dec1.ExportDateWithSuppression.ToLongTimeString());

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, true);
				TrackingDeclaration dec2 = (TrackingDeclaration)GetNewBusinessObject();
				dec2.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				dec2.Declaration.JE_ExportDate = testDate;

				AssertEquals("Should be suppressed", Suppression.SuppressedDate, dec2.ExportDateWithSuppression);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestDateOfArrivalWithSuppression()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, false);
				TrackingDeclaration dec1 = (TrackingDeclaration)GetNewBusinessObject();
				AssertEquals("initially it's blank", ZDateTime.Empty, dec1.ExportDateWithSuppression);
				dec1.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				ZDateTime testDate = ZDateTime.Now.AddDays(1);
				dec1.Declaration.JE_DateAtFinalDestination = testDate;
				AssertEquals("Should be set to date string", testDate.ToLongTimeString(), dec1.DateOfArrivalWithSuppression.ToLongTimeString());

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, true);
				TrackingDeclaration dec2 = (TrackingDeclaration)GetNewBusinessObject();
				dec2.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				dec2.Declaration.JE_DateAtFinalDestination = testDate;

				AssertEquals("Should be suppressed", Suppression.SuppressedDate, dec2.DateOfArrivalWithSuppression);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestDateOfFirstArrivalWithSuppression()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, false);
				TrackingDeclaration dec1 = (TrackingDeclaration)GetNewBusinessObject();
				AssertEquals("initially it's blank", ZDateTime.Empty, dec1.ExportDateWithSuppression);
				dec1.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				ZDateTime testDate = ZDateTime.Now.AddDays(1);
				dec1.Declaration.JE_DateOfFirstArrival = testDate;
				AssertEquals("Should be set to date string", testDate.ToLongTimeString(), dec1.DateOfFirstArrivalWithSuppression.ToLongTimeString());

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, true);
				TrackingDeclaration dec2 = (TrackingDeclaration)GetNewBusinessObject();
				dec2.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				dec2.Declaration.JE_DateOfFirstArrival = testDate;

				AssertEquals("Should be suppressed", Suppression.SuppressedDate, dec2.DateOfFirstArrivalWithSuppression);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestVoyageFlightNoWithSuppression()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				SuppressionTest.SetSuppressingFields(RegistryItem, false);
				TrackingDeclaration dec1 = (TrackingDeclaration)GetNewBusinessObject();
				AssertEquals("initially it's blank", ZString.Empty, dec1.CurrentVoyageWithSuppression);
				dec1.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				ZDateTime testDate = ZDateTime.Now.AddDays(1);
				dec1.Declaration.JE_ExportDate = testDate;
				dec1.Declaration.JE_VoyageFlightNo = "Q123";
				AssertEquals("Should be FightNo string", "Q123", dec1.CurrentVoyageWithSuppression);

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, true);
				TrackingDeclaration dec2 = (TrackingDeclaration)GetNewBusinessObject();
				dec2.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				dec2.Declaration.JE_ExportDate = testDate;
				dec2.Declaration.JE_VoyageFlightNo = "Q123";

				AssertEquals("Should be suppressed", Suppression.SuppressedString, dec2.CurrentVoyageWithSuppression);

				TrackingDeclaration dec3 = (TrackingDeclaration)GetNewBusinessObject();
				dec3.Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				dec3.Declaration.JE_ExportDate = testDate;
				dec3.Declaration.JE_VoyageFlightNo = "Test Drive";

				AssertEquals("Should be Voyage string", "Test Drive", dec1.CurrentVoyageWithSuppression);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestFolioWithSuppression()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				SuppressionTest.SetSuppressingFields(RegistryItem, false);
				TrackingDeclaration dec1 = (TrackingDeclaration)GetNewBusinessObject();
				AssertEquals("initially it's blank", ZString.Empty, dec1.MainVoyageWithSuppression);
				dec1.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				ZDateTime testDate = ZDateTime.Now.AddDays(1);
				dec1.Declaration.JE_ExportDate = testDate;
				dec1.Declaration.JE_Folio = "123";
				AssertEquals("Should be Folio string", "123", dec1.FolioWithSuppression);

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, true);
				TrackingDeclaration dec2 = (TrackingDeclaration)GetNewBusinessObject();
				dec2.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				dec2.Declaration.JE_ExportDate = testDate;
				dec2.Declaration.JE_Folio = "123";

				AssertEquals("Should be suppressed", Suppression.SuppressedString, dec2.FolioWithSuppression);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		CodeDescriptionBoolRegistryItem RegistryItem
		{
			get { return WebDataRegistry.Instance.SuppressFlightDetailsForExport; }
		}
	}
}
