using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(PreAllocation))]
	public class PreAllocationTest : NonPersistentBusinessObjectTestCase
	{
		#region IDocumentSupportable

		public void TestDocumentSupporterIsPreAllocationDocumentSupporter()
		{
			IDocumentSupportable iDoc = GetNewPreAllocation();
			AssertNotNull("Document Supporter", iDoc.DocumentSupporter);
			AssertNotNull("Is QuotedBookingDocumentSupporter", iDoc.DocumentSupporter as PreAllocationDocumentSupporter);
		}

		#endregion

		#region SetDefaults

		public void TestSetDefaults()
		{
			Assert(GetNewPreAllocation().IsPrePrinted);
		}

		#endregion

		#region Create

		public void TestCreate()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			//PreAllocation 1
			PreAllocation preAllocation = GetNewPreAllocation();
			client.MiscServ.OM_EXPreAllocPrefix = "ZXY";
			preAllocation.QuotedBooking.ClientPK = client.PK;
			preAllocation.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
			preAllocation.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			preAllocation.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, preAllocation.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			preAllocation.QuotedBooking.Origin = "AUSYD";
			preAllocation.QuotedBooking.Destination = "AUBNE";
			preAllocation.HouseBillCount = 5;
			preAllocation.RunPreSaveValidation();

			AssertEquals(0, preAllocation.Bookings.Count);
			preAllocation.Create();

			AssertEquals("5 should have been created", 5, preAllocation.Bookings.Count);
			AssertNull(preAllocation.QuotedBooking.Job);

			foreach (ForwardingShipment booking in preAllocation.Bookings)
			{
				AssertEquals("ClientPK", preAllocation.QuotedBooking.ClientPK, booking.ConsignorPickupAddress.Organisation?.PK ?? ZGuid.Empty);
				AssertNull("LoadPort", booking.LoadPort);
				AssertEquals("TransportMode", Core.Constants.TransportModes.Sea, booking.TransportMode);
				AssertEquals("ContainerMode", Core.Constants.ContainerModes.FCL, booking.PackingMode);
				AssertEquals("Consignor", preAllocation.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address, booking.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertEquals("Consignee", preAllocation.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address, booking.ConsigneeDocumentaryAddress.E2_OA_Address);
				AssertEquals("Origin", preAllocation.QuotedBooking.Origin, booking.JS_RL_NKOrigin);
				AssertEquals("Destination", preAllocation.QuotedBooking.Destination, booking.JS_RL_NKDestination);
				AssertEquals("ServiceLevel", preAllocation.QuotedBooking.ServiceLevel, booking.JS_RS_NKServiceLevel);
				AssertEquals("GoodsDesc", preAllocation.QuotedBooking.Booking.JS_GoodsDescription, booking.JS_GoodsDescription);
				AssertEquals("Goods Desc Note", preAllocation.QuotedBooking.Booking.DetailedGoodsDescriptionNoteText, booking.DetailedGoodsDescriptionNoteText);

				AssertNull(booking.ShipmentJobHeader);

				try
				{
					booking.CreateShipmentJobHeaderWithMutex();
					AssertEquals(client.MainAddress.PK, booking.ShipmentJobHeader.JH_OA_LocalChargesAddr);
				}
				finally
				{
					booking.Job.Dispose();
				}
			}

			Assert("Contains PPHZXY00000001", ContainsHouseBillNumber(preAllocation.Bookings, "PPHZXY00000001"));
			Assert("Contains PPHZXY00000002", ContainsHouseBillNumber(preAllocation.Bookings, "PPHZXY00000002"));
			Assert("Contains PPHZXY00000003", ContainsHouseBillNumber(preAllocation.Bookings, "PPHZXY00000003"));
			Assert("Contains PPHZXY00000004", ContainsHouseBillNumber(preAllocation.Bookings, "PPHZXY00000004"));
			Assert("Contains PPHZXY00000005", ContainsHouseBillNumber(preAllocation.Bookings, "PPHZXY00000005"));

			Factory.Save();
			AssertEquals(5, Factory.Load<ViewQuotedBooking>(new ZQuery()).Length);

			//PreAllocation 2 - Same Client, use existing booking like a template

			PreAllocation preAllocation2 = GetNewPreAllocation();
			preAllocation2.State = PreAllocation.PreAllocationState.Existing;
			preAllocation2.QuotedBooking.ClientPK = client.PK;
			preAllocation2.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
			preAllocation2.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			preAllocation2.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, preAllocation2.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			preAllocation2.QuotedBooking.Origin = "AUSYD";
			preAllocation2.QuotedBooking.Destination = "AUBNE";
			preAllocation2.HouseBillCount = 3;
			preAllocation2.RunPreSaveValidation();

			AssertEquals(0, preAllocation2.Bookings.Count);
			preAllocation2.Create();

			AssertEquals("3 should have been created", 3, preAllocation2.Bookings.Count);
			foreach (ForwardingShipment booking in preAllocation2.Bookings)
			{
				AssertEquals("TransportMode", Core.Constants.TransportModes.Sea, booking.TransportMode);
				AssertEquals("ContainerMode", Core.Constants.ContainerModes.FCL, booking.PackingMode);
				AssertEquals("Consignor", preAllocation2.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address, booking.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertEquals("Consignee", preAllocation2.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address, booking.ConsigneeDocumentaryAddress.E2_OA_Address);
				AssertEquals("Origin", preAllocation2.QuotedBooking.Origin, booking.JS_RL_NKOrigin);
				AssertEquals("Destination", preAllocation2.QuotedBooking.Destination, booking.JS_RL_NKDestination);
				AssertEquals("ServiceLevel", preAllocation2.QuotedBooking.ServiceLevel, booking.JS_RS_NKServiceLevel);
				AssertEquals("GoodsDesc", preAllocation2.QuotedBooking.Booking.JS_GoodsDescription, booking.JS_GoodsDescription);
				AssertEquals("Goods Desc Note", preAllocation2.QuotedBooking.Booking.DetailedGoodsDescriptionNoteText, booking.DetailedGoodsDescriptionNoteText);
			}

			Assert("Contains PPHZXY00000006", ContainsHouseBillNumber(preAllocation2.Bookings, "PPHZXY00000006"));
			Assert("Contains PPHZXY00000007", ContainsHouseBillNumber(preAllocation2.Bookings, "PPHZXY00000007"));
			Assert("Contains PPHZXY00000008", ContainsHouseBillNumber(preAllocation2.Bookings, "PPHZXY00000008"));

			Factory.Save();
			AssertEquals(9, Factory.Load<ViewQuotedBooking>(new ZQuery()).Length);

			//PreAllocation 3 - Dif Client

			PreAllocation preAllocation3 = GetNewPreAllocation();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.MiscServ.OM_EXPreAllocPrefix = "WAH";
			preAllocation3.QuotedBooking.ClientPK = client2.PK;
			preAllocation3.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
			preAllocation3.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			preAllocation3.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, preAllocation3.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			preAllocation3.QuotedBooking.Origin = "AUSYD";
			preAllocation3.QuotedBooking.Destination = "AUBNE";
			preAllocation3.HouseBillCount = 4;
			preAllocation3.RunPreSaveValidation();

			AssertEquals(0, preAllocation3.Bookings.Count);
			preAllocation3.Create();

			AssertEquals("4 should have been created", 4, preAllocation3.Bookings.Count);
			foreach (ForwardingShipment booking in preAllocation3.Bookings)
			{
				AssertEquals("TransportMode", Core.Constants.TransportModes.Sea, booking.TransportMode);
				AssertEquals("ContainerMode", Core.Constants.ContainerModes.FCL, booking.PackingMode);
				AssertEquals("Consignor", preAllocation3.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address, booking.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertEquals("Consignee", preAllocation3.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address, booking.ConsigneeDocumentaryAddress.E2_OA_Address);
				AssertEquals("Origin", preAllocation3.QuotedBooking.Origin, booking.JS_RL_NKOrigin);
				AssertEquals("Destination", preAllocation3.QuotedBooking.Destination, booking.JS_RL_NKDestination);
				AssertEquals("ServiceLevel", preAllocation3.QuotedBooking.ServiceLevel, booking.JS_RS_NKServiceLevel);
				AssertEquals("GoodsDesc", preAllocation3.QuotedBooking.Booking.JS_GoodsDescription, booking.JS_GoodsDescription);
				AssertEquals("Goods Desc Note", preAllocation3.QuotedBooking.Booking.DetailedGoodsDescriptionNoteText, booking.DetailedGoodsDescriptionNoteText);
			}

			Assert("Contains PPHWAH00000001", ContainsHouseBillNumber(preAllocation3.Bookings, "PPHWAH00000001"));
			Assert("Contains PPHWAH00000002", ContainsHouseBillNumber(preAllocation3.Bookings, "PPHWAH00000002"));
			Assert("Contains PPHWAH00000003", ContainsHouseBillNumber(preAllocation3.Bookings, "PPHWAH00000003"));
			Assert("Contains PPHWAH00000004", ContainsHouseBillNumber(preAllocation3.Bookings, "PPHWAH00000004"));
		}

		public void TestCreate_SuppressShipmentNumberValidation()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_EXPreAllocPrefix = "XXX";

			PreAllocation preAllocation1 = GetNewPreAllocation();
			preAllocation1.QuotedBooking.ClientPK = client.PK;
			preAllocation1.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
			preAllocation1.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			preAllocation1.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, preAllocation1.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			preAllocation1.QuotedBooking.Origin = "AUSYD";
			preAllocation1.QuotedBooking.Destination = "AUBNE";
			preAllocation1.HouseBillCount = 2;
			preAllocation1.RunPreSaveValidation();

			AssertEquals(0, preAllocation1.Bookings.Count);
			AssertEquals(true, preAllocation1.QuotedBooking.Booking.SuppressShipmentNumberValidation);
			preAllocation1.Create();

			AssertEquals(true, preAllocation1.Bookings[0].SuppressShipmentNumberValidation);
			AssertEquals(true, preAllocation1.Bookings[1].SuppressShipmentNumberValidation);
		}

		bool ContainsHouseBillNumber(ForwardingShipmentCollection bookings, ZString houseBill)
		{
			foreach (ForwardingShipment booking in bookings)
			{
				if (booking.JS_HouseBill == houseBill)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region State

		public void TestState()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			//New

			PreAllocation newPreAllocation = GetNewPreAllocation();
			newPreAllocation.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
			AssertEquals(PreAllocation.PreAllocationState.New, newPreAllocation.State);
			Assert(!newPreAllocation.QuotedBooking.ModeInfo.ReadOnly);
			Assert(!newPreAllocation.QuotedBooking.ClientAddrPKInfo.ReadOnly);
			Assert(!newPreAllocation.QuotedBooking.ConsignorDocumentaryAddress.ReadOnly);
			Assert(!newPreAllocation.QuotedBooking.ConsigneeDocumentaryAddress.ReadOnly);
			Assert(!newPreAllocation.QuotedBooking.OriginInfo.ReadOnly);
			Assert(!newPreAllocation.QuotedBooking.DestinationInfo.ReadOnly);
			Assert(!newPreAllocation.QuotedBooking.ServiceLevelInfo.ReadOnly);
			Assert(!newPreAllocation.QuotedBooking.Booking.JS_HouseBillOfLadingTypeInfo.ReadOnly);
			Assert(!newPreAllocation.HouseBillCountInfo.ReadOnly);

			client.MiscServ.OM_EXPreAllocPrefix = "ZXY";
			newPreAllocation.QuotedBooking.ClientPK = client.PK;
			newPreAllocation.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
			newPreAllocation.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			newPreAllocation.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, newPreAllocation.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			newPreAllocation.QuotedBooking.Origin = "AUSYD";
			newPreAllocation.QuotedBooking.Destination = "AUBNE";
			newPreAllocation.HouseBillCount = 2;
			newPreAllocation.RunPreSaveValidation();
			newPreAllocation.Create();

			Assert(newPreAllocation.QuotedBooking.ReadOnly);

			//Existing

			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			PreAllocation existingPreAllocation = new PreAllocation(quotedBooking, PreAllocation.PreAllocationState.Existing);
			existingPreAllocation.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
			AssertEquals(PreAllocation.PreAllocationState.Existing, existingPreAllocation.State);
			Assert(existingPreAllocation.QuotedBooking.TransportModeInfo.ReadOnly);
			Assert(existingPreAllocation.QuotedBooking.ContainerModeInfo.ReadOnly);
			Assert(existingPreAllocation.QuotedBooking.ClientAddrPKInfo.ReadOnly);
			Assert(existingPreAllocation.QuotedBooking.ConsignorDocumentaryAddress.ReadOnly);
			Assert(existingPreAllocation.QuotedBooking.ConsigneeDocumentaryAddress.ReadOnly);
			Assert(existingPreAllocation.QuotedBooking.OriginInfo.ReadOnly);
			Assert(existingPreAllocation.QuotedBooking.DestinationInfo.ReadOnly);
			Assert(existingPreAllocation.QuotedBooking.ServiceLevelInfo.ReadOnly);
			Assert(!existingPreAllocation.QuotedBooking.Booking.JS_HouseBillOfLadingTypeInfo.ReadOnly);
			Assert(!existingPreAllocation.HouseBillCountInfo.ReadOnly);

			existingPreAllocation.QuotedBooking.ClientPK = client.PK;
			existingPreAllocation.QuotedBooking.Mode = Core.Constants.RateMode.FCL;
			existingPreAllocation.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			existingPreAllocation.QuotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, existingPreAllocation.QuotedBooking.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			existingPreAllocation.QuotedBooking.Origin = "AUSYD";
			existingPreAllocation.QuotedBooking.Destination = "AUBNE";
			existingPreAllocation.HouseBillCount = 2;
			existingPreAllocation.RunPreSaveValidation();
			existingPreAllocation.Create();

			Assert(existingPreAllocation.QuotedBooking.ReadOnly);
		}

		#endregion

		#region HouseBillCount

		public void TestHouseBillCount()
		{
			PreAllocation preAllocation = GetNewPreAllocation();
			preAllocation.HouseBillCount = 0;
			AssertEquals(0, preAllocation.HouseBillNumberFrom);
			AssertEquals(0, preAllocation.HouseBillNumberTo);
			AssertEquals("", preAllocation.HouseBillFrom);
			AssertEquals("", preAllocation.HouseBillTo);

			preAllocation.HouseBillCount = 1;
			AssertEquals(1, preAllocation.HouseBillNumberFrom);
			AssertEquals(1, preAllocation.HouseBillNumberTo);
			AssertEquals("PPH???00000001", preAllocation.HouseBillFrom);
			AssertEquals("PPH???00000001", preAllocation.HouseBillTo);

			preAllocation.HouseBillCount = 2;
			AssertEquals(1, preAllocation.HouseBillNumberFrom);
			AssertEquals(2, preAllocation.HouseBillNumberTo);
			AssertEquals("PPH???00000001", preAllocation.HouseBillFrom);
			AssertEquals("PPH???00000002", preAllocation.HouseBillTo);

			preAllocation.HouseBillCount = 5;
			AssertEquals(1, preAllocation.HouseBillNumberFrom);
			AssertEquals(5, preAllocation.HouseBillNumberTo);
			AssertEquals("PPH???00000001", preAllocation.HouseBillFrom);
			AssertEquals("PPH???00000005", preAllocation.HouseBillTo);

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_EXPreAllocPrefix = "ZIZ";
			preAllocation.QuotedBooking.ClientPK = client.PK;
			preAllocation.HouseBillCount = 6;
			AssertEquals(1, preAllocation.HouseBillNumberFrom);
			AssertEquals(6, preAllocation.HouseBillNumberTo);
			AssertEquals("PPHZIZ00000001", preAllocation.HouseBillFrom);
			AssertEquals("PPHZIZ00000006", preAllocation.HouseBillTo);

			preAllocation.HouseBillCount = 10000;
			AssertEquals("PPHZIZ00010000", preAllocation.HouseBillTo);

			preAllocation.HouseBillCount = 10025;
			AssertEquals("", preAllocation.HouseBillTo);
		}

		#endregion

		#region ConcurrencyCheck

		public void TestPreAllocationSave_ShouldNotRaiseConcurrencyCheckException()
		{
			var firstFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var booking = QuotedBooking.CreateNewBooking(firstFactory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, firstFactory);
			var firstPreAllocation = new PreAllocation(quotedBooking, PreAllocation.PreAllocationState.New);
			var client = firstFactory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_EXPreAllocPrefix = "ZIZ";
			firstPreAllocation.QuotedBooking.ClientPK = client.PK;
			firstPreAllocation.HouseBillCount = 1;
			firstPreAllocation.Create();
			firstPreAllocation.Factory.Save();

			var secondFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var secondBooking = QuotedBooking.CreateNewBooking(secondFactory);
			var secondQuotedBooking = QuotedBooking.New(ZGuid.Empty, secondBooking.PK, secondFactory);
			var secondPreAllocation = new PreAllocation(secondQuotedBooking, PreAllocation.PreAllocationState.New);
			secondPreAllocation.QuotedBooking.ClientPK = client.PK;
			secondPreAllocation.HouseBillCount = 1;
			secondPreAllocation.Create();

			var firstCreatedBooking = firstPreAllocation.Bookings[0];
			firstCreatedBooking.JS_IsForwardRegistered = true;
			firstFactory.Save();

			AssertNoExceptionThrown(() => secondPreAllocation.Factory.Save());
		}

		#endregion

		#region Implementation

		PreAllocation GetNewPreAllocation()
		{
			return (PreAllocation)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			return new PreAllocation(quotedBooking, PreAllocation.PreAllocationState.New);
		}

		#endregion
	}
}
