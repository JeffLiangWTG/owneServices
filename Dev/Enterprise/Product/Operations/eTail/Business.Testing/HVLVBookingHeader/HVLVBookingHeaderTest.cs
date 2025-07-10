using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVBookingHeader))]
	public class HVLVBookingHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocsAndCartageProperties()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.DocsAndCartage.JP_PickupRequiredFrom = new ZDateTime(2022, 12, 8);
			bookingHeader.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2022, 12, 9);
			bookingHeader.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2022, 12, 9);
			bookingHeader.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2022, 12, 10);
			bookingHeader.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2022, 12, 10);
			Factory.Save();

			var bookingHeaderInNewFactory = new BusinessObjectFactory().Load<HVLVBookingHeader>(bookingHeader.PK);

			CombineAssertions("Successfully load DocsAndCartage properties", () =>
			{
				AssertEquals(new ZDateTime(2022, 12, 8), bookingHeaderInNewFactory.DocsAndCartage.JP_PickupRequiredFrom);
				AssertEquals(new ZDateTime(2022, 12, 9), bookingHeaderInNewFactory.DocsAndCartage.JP_PickupRequiredBy);
				AssertEquals(new ZDateTime(2022, 12, 9), bookingHeaderInNewFactory.DocsAndCartage.JP_PickupCartageAdvised);
				AssertEquals(new ZDateTime(2022, 12, 10), bookingHeaderInNewFactory.DocsAndCartage.JP_EstimatedPickup);
				AssertEquals(new ZDateTime(2022, 12, 10), bookingHeaderInNewFactory.DocsAndCartage.JP_PickupCartageCompleted);
			});
		}

		public void TestHVLVBookingHeaderCodePropertyAttribute()
		{
			var attributes = TypeDescriptor.GetAttributes(typeof(HVLVBookingHeader));
			var codePropertyAttributeType = typeof(CodePropertyAttribute);
			CombineAssertions(() =>
			{
				AssertNotNull("CodePropertyAttribute exists for HVLVBookingheader", attributes[codePropertyAttributeType]);
				AssertEquals($"PropertyName is supposed to be {AutoHVLVBookingHeader.Schema.HVH_BookingReference}",
					AutoHVLVBookingHeader.Schema.HVH_BookingReference,
					(attributes[codePropertyAttributeType] as CodePropertyAttribute).PropertyName);
			});
		}

		public void TestIEDocsProvider()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			Assert(bookingHeader is IEDocsProvider);
			var docManagerSupport = (IDocManagerSupport)bookingHeader;
			AssertNotNull(docManagerSupport.DocManagerInfo);
			AssertEquals("HLB", docManagerSupport.DocManagerInfo.DocManagerCode);
			AssertNotNull(bookingHeader.GetEDocsProviderSupporter());
		}

		public void TestHumanReadableName()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			AssertEquals("HVLV Booking Header", bookingHeader.HumanReadableName);

			bookingHeader.HVH_BookingReference = "M00001042";
			AssertEquals("HVLV Booking Header M00001042", bookingHeader.HumanReadableName);
		}

		public void TestBookingStatus()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			bookingHeader.BookingStatus = HVLVBookingStatus.BookedCode;
			AssertEquals(HVLVBookingStatus.BookedCode, bookingHeader.BookingStatus);

			bookingHeader.BookingStatus = HVLVBookingStatus.ConfirmedCode;
			AssertEquals(HVLVBookingStatus.ConfirmedCode, bookingHeader.BookingStatus);
		}

		public void TestWhenReceivedAtOrigin_ThenBookingStatusIsReadOnly()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			AssertEquals("Expected booking header's booking status readonly to be false by default", false, bookingHeader.BookingStatusInfo.ReadOnly);

			bookingHeader.HVH_IsBookingReceived = true;
			AssertEquals("Expected booking header's booking status readonly if receveid at origin", true, bookingHeader.BookingStatusInfo.ReadOnly);
		}

		public void TestIsBookingReceivedCheckBoxReadOnly()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			AssertEquals("Expected received at origin readonly to be true by default", true, bookingHeader.HVH_IsBookingReceivedInfo.ReadOnly);

			bookingHeader.HVH_IsBookingConfirmed = true;
			AssertEquals("Expected received at origin to be read only if booking is not confirmed", false, bookingHeader.HVH_IsBookingReceivedInfo.ReadOnly);
		}

		public void TestGrossWeightRecalculation()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualWeight = 9;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedWeight = 0.5;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedWeight = 1;
			item2b.HVI_ItemId = "ITEM2B";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ItemCount = 1;
			var item3a = consignment3.Items.AddNew();
			item3a.HVI_ManifestedWeight = 1;
			item3a.HVI_ItemId = "ITEM3A";

			Factory.Save();

			AssertEquals("Gross weight should be 11.5", 11.5m, bookingHeader.HVH_GrossWeight);

			item2a.HVI_ManifestedWeight = 1;
			AssertEquals("Gross weight should be 12", 12m, bookingHeader.HVH_GrossWeight);

			item2a.HVI_ActualWeight = 3;
			item2b.HVI_ActualWeight = 2;

			Factory.Save();

			AssertEquals("Gross weight should be 15", 15m, bookingHeader.HVH_GrossWeight);

			item2a.HVI_ActualWeight = 0;
			AssertEquals("Gross weight should display 13", 13m, bookingHeader.HVH_GrossWeight);

			item2b.HVI_ActualWeight = 0;
			AssertEquals("Gross weight should display 12", 12m, bookingHeader.HVH_GrossWeight);

			item2b.HVI_ManifestedWeight = 0;
			item2b.HVI_ActualWeight = 3;
			AssertEquals("Gross weight should display 14", 14m, bookingHeader.HVH_GrossWeight);

			consignment2.Items.RemoveAndDelete(item2b);
			AssertEquals("Gross weight should be 11", 11m, bookingHeader.HVH_GrossWeight);

			bookingHeader.Consignments.RemoveAndDelete(consignment2);
			AssertEquals("Gross weight should be 10", 10m, bookingHeader.HVH_GrossWeight);
		}

		public void TestGrossWeightRecalculation_ExcludesInactiveConsignments()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualWeight = 9;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedWeight = 0;
			item2a.HVI_ActualWeight = 1;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedWeight = 3;
			item2b.HVI_ActualWeight = 0;
			item2b.HVI_ItemId = "ITEM2B";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ItemCount = 1;
			var item3a = consignment3.Items.AddNew();
			item3a.HVI_ManifestedWeight = 1;
			item3a.HVI_ItemId = "ITEM3A";

			AssertEquals("Gross weight should be 14", 14m, bookingHeader.HVH_GrossWeight);

			consignment2.HVC_IsActive = false;
			consignment3.HVC_IsActive = false;
			AssertEquals("Gross weight should be 9", 9m, bookingHeader.HVH_GrossWeight);

			consignment3.HVC_IsActive = true;
			AssertEquals("Gross weight should be 10", 10m, bookingHeader.HVH_GrossWeight);
		}

		public void TestGrossWeightRecalculation_ExcludesInactiveItems()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualWeight = 9;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedWeight = 5;
			item2a.HVI_ActualWeight = 0;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedWeight = 0;
			item2b.HVI_ActualWeight = 3;
			item2b.HVI_ItemId = "ITEM2B";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ItemCount = 1;
			var item3a = consignment3.Items.AddNew();
			item3a.HVI_ManifestedWeight = 7;
			item3a.HVI_ItemId = "ITEM3A";

			AssertEquals("Gross weight should be 24", 24m, bookingHeader.HVH_GrossWeight);

			item2b.HVI_IsActive = false;
			AssertEquals("Gross weight should be 21", 21m, bookingHeader.HVH_GrossWeight);

			item3a.HVI_IsActive = false;
			AssertEquals("Gross weight should be 14", 14m, bookingHeader.HVH_GrossWeight);

			item2b.HVI_IsActive = true;
			AssertEquals("Gross weight should be 17", 17m, bookingHeader.HVH_GrossWeight);
		}

		public void TestGrossWeightRecalculation_OnConsignmentUQChange()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_GrossWeightUQ = "KG";
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			consignment1.HVC_WeightUQ = "KG";
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualWeight = 90;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			consignment2.HVC_WeightUQ = "KG";
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedWeight = 5;
			item2a.HVI_ActualWeight = 0;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedWeight = 0;
			item2b.HVI_ActualWeight = 3;
			item2b.HVI_ItemId = "ITEM2B";

			AssertEquals("Gross Weight should be 98", 98m, bookingHeader.HVH_GrossWeight);

			consignment1.HVC_WeightUQ = "G"; // 90G = 0.09 KG
			AssertEquals("Gross Weight should be 8.09", 8.09m, bookingHeader.HVH_GrossWeight);
		}

		public void TestGrossWeightRecalculation_OnHVH_WeightUQChange()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_GrossWeightUQ = "KG";
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			consignment1.HVC_WeightUQ = "G";
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualWeight = 90;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			consignment2.HVC_WeightUQ = "KG";
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedWeight = 5;
			item2a.HVI_ActualWeight = 0;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedWeight = 0;
			item2b.HVI_ActualWeight = 3;
			item2b.HVI_ItemId = "ITEM2B";

			AssertEquals("Gross Weight should be 8.09", 8.09m, bookingHeader.HVH_GrossWeight);

			bookingHeader.HVH_GrossWeightUQ = "SS";
			AssertEquals("Gross Weight should keep previous value when unit is invalid", 8.09m, bookingHeader.HVH_GrossWeight);

			bookingHeader.HVH_GrossWeightUQ = ZString.Empty;
			AssertEquals("Gross Weight should keep previous value when unit is invalid", 8.09m, bookingHeader.HVH_GrossWeight);

			bookingHeader.HVH_GrossWeightUQ = "G";
			AssertEquals("Gross Weight should recalculate when change from invalid to valid UQ", 8090m, bookingHeader.HVH_GrossWeight);
		}

		public void TestGrossWeight_WhenUnrelatedItemPropertyChanges_UsesCachedValue()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_ActualWeight = 9;
			var item2 = consignment1.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_ActualWeight = 10;
			Factory.Save();

			var grossWeight = header.HVH_GrossWeight;
			AssertEquals("Precondition: correct weight", 19m, grossWeight);

			item2.HVI_GoodsDescription = "This property is unrelated to weight";
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ HVLVItemSchema.Constants.TableName, 0 }
			};

			Factory.ResetDatabaseLoadCount();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, Factory))
			{
				grossWeight = header.HVH_GrossWeight;
			}

			AssertEquals("Postcondition: correct weight", 19m, grossWeight);
		}

		public void TestGrossVolumeRecalculation()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualVolume = 9;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedVolume = 0.5;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedVolume = 1;
			item2b.HVI_ItemId = "ITEM2B";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ItemCount = 1;
			var item3a = consignment3.Items.AddNew();
			item3a.HVI_ManifestedVolume = 1;
			item3a.HVI_ItemId = "ITEM3A";

			Factory.Save();

			AssertEquals("Gross volume should be 11.5", 11.5m, bookingHeader.HVH_GrossVolume);

			item2a.HVI_ManifestedVolume = 1;
			AssertEquals("Gross volume should be 12", 12m, bookingHeader.HVH_GrossVolume);

			item2a.HVI_ActualVolume = 3;
			item2b.HVI_ActualVolume = 2;

			Factory.Save();

			AssertEquals("Gross volume should be 15", 15m, bookingHeader.HVH_GrossVolume);

			item2a.HVI_ActualVolume = 0;
			AssertEquals("Gross volume should display 13", 13m, bookingHeader.HVH_GrossVolume);

			item2b.HVI_ActualVolume = 0;
			AssertEquals("Gross volume should display 12", 12m, bookingHeader.HVH_GrossVolume);

			item2b.HVI_ManifestedVolume = 0;
			item2b.HVI_ActualVolume = 3;
			AssertEquals("Gross volume should display 14", 14m, bookingHeader.HVH_GrossVolume);

			consignment2.Items.RemoveAndDelete(item2b);
			AssertEquals("Gross volume should be 11", 11m, bookingHeader.HVH_GrossVolume);

			bookingHeader.Consignments.RemoveAndDelete(consignment2);
			AssertEquals("Gross volume should be 10", 10m, bookingHeader.HVH_GrossVolume);
		}

		public void TestGrossVolumeRecalculation_ExcludesInactiveConsignments()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualVolume = 9;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedVolume = 0;
			item2a.HVI_ActualVolume = 1;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedVolume = 3;
			item2b.HVI_ActualVolume = 0;
			item2b.HVI_ItemId = "ITEM2B";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ItemCount = 1;
			var item3a = consignment3.Items.AddNew();
			item3a.HVI_ManifestedVolume = 1;
			item3a.HVI_ItemId = "ITEM3A";

			AssertEquals("Gross volume should be 14", 14m, bookingHeader.HVH_GrossVolume);

			consignment2.HVC_IsActive = false;
			consignment3.HVC_IsActive = false;
			AssertEquals("Gross volume should be 9", 9m, bookingHeader.HVH_GrossVolume);

			consignment3.HVC_IsActive = true;
			AssertEquals("Gross volume should be 10", 10m, bookingHeader.HVH_GrossVolume);
		}

		public void TestGrossVolumeRecalculation_ExcludesInactiveItems()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualVolume = 9;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedVolume = 5;
			item2a.HVI_ActualVolume = 0;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedVolume = 0;
			item2b.HVI_ActualVolume = 3;
			item2b.HVI_ItemId = "ITEM2B";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			consignment3.HVC_ItemCount = 1;
			var item3a = consignment3.Items.AddNew();
			item3a.HVI_ManifestedVolume = 7;
			item3a.HVI_ItemId = "ITEM3A";

			AssertEquals("Gross volume should be 24", 24m, bookingHeader.HVH_GrossVolume);

			item2b.HVI_IsActive = false;
			AssertEquals("Gross volume should be 21", 21m, bookingHeader.HVH_GrossVolume);

			item3a.HVI_IsActive = false;
			AssertEquals("Gross volume should be 14", 14m, bookingHeader.HVH_GrossVolume);

			item2b.HVI_IsActive = true;
			AssertEquals("Gross volume should be 17", 17m, bookingHeader.HVH_GrossVolume);
		}

		public void TestGrossVolumeRecalculation_OnConsignmentUQChange()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_GrossVolumeUQ = "M3";
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			consignment1.HVC_VolumeUQ = "M3";
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualVolume = 90;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			consignment2.HVC_VolumeUQ = "M3";
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedVolume = 5;
			item2a.HVI_ActualVolume = 0;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedVolume = 0;
			item2b.HVI_ActualVolume = 3;
			item2b.HVI_ItemId = "ITEM2B";

			AssertEquals("Gross volume should be 98", 98m, bookingHeader.HVH_GrossVolume);

			consignment1.HVC_VolumeUQ = "L"; // 90L = 0.09 M3
			AssertEquals("Gross volume should be 8.09", 8.09m, bookingHeader.HVH_GrossVolume);
		}

		public void TestGrossVolumeRecalculation_OnHVH_VolumeUQChange()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_GrossVolumeUQ = "M3";
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			consignment1.HVC_VolumeUQ = "L";
			var item1a = consignment1.Items.AddNew();
			item1a.HVI_ActualVolume = 90;
			item1a.HVI_ItemId = "ITEM1A";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 2;
			consignment2.HVC_VolumeUQ = "M3";
			var item2a = consignment2.Items.AddNew();
			item2a.HVI_ManifestedVolume = 5;
			item2a.HVI_ActualVolume = 0;
			item2a.HVI_ItemId = "ITEM2A";
			var item2b = consignment2.Items.AddNew();
			item2b.HVI_ManifestedVolume = 0;
			item2b.HVI_ActualVolume = 3;
			item2b.HVI_ItemId = "ITEM2B";

			AssertEquals("Gross volume should be 8.09", 8.09m, bookingHeader.HVH_GrossVolume);

			bookingHeader.HVH_GrossVolumeUQ = "SS";
			AssertEquals("Gross volume should keep previous value when unit is invalid", 8.09m, bookingHeader.HVH_GrossVolume);

			bookingHeader.HVH_GrossVolumeUQ = ZString.Empty;
			AssertEquals("Gross volume should keep previous value when unit is invalid", 8.09m, bookingHeader.HVH_GrossVolume);

			bookingHeader.HVH_GrossVolumeUQ = "L";
			AssertEquals("Gross volume should recalculate when change from invalid to valid UQ", 8090m, bookingHeader.HVH_GrossVolume);
		}

		public void TestGrossVolume_WhenUnrelatedItemPropertyChanges_UsesCachedValue()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_ActualVolume = 9;
			var item2 = consignment1.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_ActualVolume = 10;
			Factory.Save();

			var grossVolume = header.HVH_GrossVolume;
			AssertEquals("Precondition: correct volume", 19m, grossVolume);

			item2.HVI_GoodsDescription = "This property is unrelated to volume";
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ HVLVItemSchema.Constants.TableName, 0 }
			};

			Factory.ResetDatabaseLoadCount();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, Factory))
			{
				grossVolume = header.HVH_GrossVolume;
			}

			AssertEquals("Postcondition: correct volume", 19m, grossVolume);
		}

		public void TestChangingBillToPartyDefaultsContact()
		{
			var org1 = Factory.New<OrgHeader>();
			var address1_1 = org1.Addresses.AddNew();
			var address1_2 = org1.Addresses.AddNew();
			var contact1_1 = org1.Contacts.AddNew();

			var org2 = Factory.New<OrgHeader>();
			org2.Contacts.AddNew();
			org2.Contacts.AddNew();

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = address1_1.PK;
			AssertEquals("Only one Contact on OrgHeader; should default", contact1_1.PK, bookingHeader.HVH_OC_BookedBy);

			bookingHeader.HVH_OA_BillToParty = address1_2.PK;
			AssertEquals("Different address but same OrgHeader; shouldn't reset contact", contact1_1.PK, bookingHeader.HVH_OC_BookedBy);

			bookingHeader.HVH_OA_BillToParty = org2.MainAddress.PK;
			AssertEquals("New OrgHeader with multiple Contacts; should default to empty", ZGuid.Empty, bookingHeader.HVH_OC_BookedBy);
		}

		public void TestBillToParty()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var billToPartyAddress = header.BillToParty;
			AssertNotNull(billToPartyAddress);
		}

		public void TestClusterKeyIsCascadedToConsignments()
		{
			var header = Factory.New<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();
			var item2 = consignment1.Items.AddNew();
			var item3 = consignment2.Items.AddNew();

			var clusterKey = 9999;
			header.HVH_ClusterKey = clusterKey;

			AssertEquals(clusterKey, consignment1.HVC_ClusterKey);
			AssertEquals(clusterKey, consignment2.HVC_ClusterKey);
			AssertEquals(clusterKey, item1.HVI_ClusterKey);
			AssertEquals(clusterKey, item2.HVI_ClusterKey);
			AssertEquals(clusterKey, item3.HVI_ClusterKey);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestHVHClusterKeyCanBeGenerated_WhenDuplicateKeyConflicts()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			Factory.Save();

			AssertEquals(1, bookingHeader1.HVH_ClusterKey);
			AssertEquals(2, consignmentHeader.HCH_ClusterKey);

			bookingHeader1.HVH_ClusterKey = 3;
			consignmentHeader.HCH_ClusterKey = 4;
			Factory.Save();

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();

			try
			{
				Factory.Save();
				Fail("Factory Save should fail");
			}
			catch (ZSaveException ex)
			{
				AssertContains("The value of HVLVBookingHeader|HVH_ClusterKey must be unique on Booking Header. The duplicate value(s) are: (3).", ex.FriendlyMessage);

				ZExceptionReporting.HandleSaveException(ex);
				Factory.Save();
			}

			AssertEquals("NumberFountain should spit 5 as HVLVConsignmentHeader ClusterKeys should be counted as well", 5, bookingHeader2.HVH_ClusterKey);
		}

		public void TestTotalMeasurements()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_ItemCount = 1;
			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ItemCount = 1;
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			var item2 = consignment2.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";

			item1.HVI_ManifestedWeight = 100;
			item1.HVI_ActualWeight = 200;
			item1.HVI_ManifestedVolume = 23;
			item2.HVI_ManifestedWeight = 300;
			item2.HVI_ManifestedVolume = 25;
			item2.HVI_ActualVolume = 35;
			Factory.Save();

			AssertEquals("HVH_ItemCount should be the total number of HVLVItems", 2, header.HVH_ItemCount);
			AssertEquals("Calculate total Weight for a new BookingHeader", 500m, header.HVH_GrossWeight);
			AssertEquals("Calculate total Volume for a new BookingHeader", 58m, header.HVH_GrossVolume);

			header.HVH_GrossWeightUQ = Weight.Tonnes;
			header.HVH_GrossVolumeUQ = Volume.CubicFeet;
			Factory.Save();

			AssertEquals("HVH_ItemCount should be the total number of HVLVItems", 2, header.HVH_ItemCount);
			AssertEquals("Changing unit on BookingHeader causes recalculation", 0.5m, header.HVH_GrossWeight);
			AssertEquals("Changing unit on BookingHeader causes recalculation", 2048.251m, header.HVH_GrossVolume.Round(3));

			item1.HVI_ActualWeight = 400;
			item1.HVI_ActualVolume = 33;
			Factory.Save();

			AssertEquals("HVH_ItemCount should be the total number of HVLVItems", 2, header.HVH_ItemCount);
			AssertEquals("Changing Consignment measurements causes recalculation", 0.7m, header.HVH_GrossWeight);
			AssertEquals("Changing Consignment measurements causes recalculation", 2401.397m, header.HVH_GrossVolume.Round(3));

			consignment1.HVC_WeightUQ = Weight.Tonnes;
			consignment1.HVC_VolumeUQ = Volume.CubicFeet;
			Factory.Save();

			AssertEquals("HVH_ItemCount should be the total number of HVLVItems", 2, header.HVH_ItemCount);
			AssertEquals("Changing Consignment unit causes recalculation", 400.3m, header.HVH_GrossWeight);
			AssertEquals("Changing Consignment unit causes recalculation", 1269.013m, header.HVH_GrossVolume.Round(3));
		}

		public void TestItemCount()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			var item2 = consignment1.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";

			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			var item3 = consignment2.Items.AddNew();
			item3.HVI_ItemId = "ITEM3";
			var item4 = consignment2.Items.AddNew();
			item4.HVI_ItemId = "ITEM4";
			var item5 = consignment2.Items.AddNew();
			item5.HVI_ItemId = "ITEM5";

			AssertEquals("Summary of HVC_ItemCount", 5, header.HVH_ItemCount);

			var item6 = consignment2.Items.AddNew();
			item6.HVI_ItemId = "ITEM6";
			var item7 = consignment2.Items.AddNew();
			item7.HVI_ItemId = "ITEM7";
			AssertEquals("Summary of HVC_ItemCount", 7, header.HVH_ItemCount);

			consignment2.Items.RemoveAndDelete(item6);
			AssertEquals("Summary of HVC_ItemCount", 6, header.HVH_ItemCount);

			header.Consignments.RemoveAndDelete(consignment2);
			Factory.Save();

			AssertEquals("Summary of HVC_ItemCount", 2, header.HVH_ItemCount);
		}

		public void TestItemCount_ExcludesInactiveConsignments()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			var item2 = consignment1.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";

			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			var item3 = consignment2.Items.AddNew();
			item3.HVI_ItemId = "ITEM3";
			var item4 = consignment2.Items.AddNew();
			item4.HVI_ItemId = "ITEM4";
			var item5 = consignment2.Items.AddNew();
			item5.HVI_ItemId = "ITEM5";

			var consignment3 = header.Consignments.AddNew();
			consignment3.HVC_ConsignmentId = "CONSIGN3";
			var item6 = consignment3.Items.AddNew();
			item6.HVI_ItemId = "ITEM6";
			var item7 = consignment3.Items.AddNew();
			item7.HVI_ItemId = "ITEM7";
			var item8 = consignment3.Items.AddNew();
			item8.HVI_ItemId = "ITEM8";
			var item9 = consignment3.Items.AddNew();
			item9.HVI_ItemId = "ITEM9";

			AssertEquals("Summary of HVC_ItemCount", 9, header.HVH_ItemCount);

			consignment2.HVC_IsActive = false;
			consignment3.HVC_IsActive = false;
			AssertEquals("Summary of HVC_ItemCount", 2, header.HVH_ItemCount);

			consignment2.HVC_IsActive = true;
			AssertEquals("Summary of HVC_ItemCount", 5, header.HVH_ItemCount);
		}

		public void TestItemCount_ExcludesInactiveItems()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			var item2 = consignment1.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";

			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			var item3 = consignment2.Items.AddNew();
			item3.HVI_ItemId = "ITEM3";
			var item4 = consignment2.Items.AddNew();
			item4.HVI_ItemId = "ITEM4";
			var item5 = consignment2.Items.AddNew();
			item5.HVI_ItemId = "ITEM5";

			AssertEquals("Summary of HVC_ItemCount", 5, header.HVH_ItemCount);

			item1.HVI_IsActive = false;
			item4.HVI_IsActive = false;
			//Factory.Save();

			AssertEquals("Summary of HVC_ItemCount", 3, header.HVH_ItemCount);

			item4.HVI_IsActive = true;
			//Factory.Save();

			AssertEquals("Summary of HVC_ItemCount", 4, header.HVH_ItemCount);
		}

		public void TestItemCount_WhenUnrelatedItemPropertyChanges_UsesCachedValue()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			var item2 = consignment1.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			Factory.Save();

			var itemCount = header.HVH_ItemCount;
			AssertEquals("Precondition: correct item count", 2, itemCount);

			item2.HVI_GoodsDescription = "This property is unrelated to item count";
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ HVLVItemSchema.Constants.TableName, 0 }
			};

			Factory.ResetDatabaseLoadCount();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, Factory))
			{
				itemCount = header.HVH_ItemCount;
			}

			AssertEquals("Postcondition: correct item count", 2, itemCount);
		}

		public void TestDeleteDoesNotCauseConcurrencyError()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			item1.HVI_ActualWeight = 1;
			item1.HVI_ActualVolume = 1;
			var item2 = consignment1.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";
			item2.HVI_ActualWeight = 1;
			item2.HVI_ActualVolume = 1;
			Factory.Save();

			Assert(!item1.IsDeleted);
			Assert(!item2.IsDeleted);
			Assert(!consignment1.IsDeleted);
			Assert(!header.IsDeleted);

			header.Delete();
			Factory.Save();
			Assert(item1.IsDeleted);
			Assert(item2.IsDeleted);
			Assert(consignment1.IsDeleted);
			Assert(header.IsDeleted);
		}

		public void TestTotalMeasurements_InvalidUnits()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_ItemId = "ITEM1";
			var item2 = consignment2.Items.AddNew();
			item2.HVI_ItemId = "ITEM2";

			consignment1.HVC_WeightUQ = "XX";
			item1.HVI_ManifestedWeight = 30m;
			item1.HVI_ManifestedVolume = 30m;
			consignment2.HVC_VolumeUQ = "XX";
			item2.HVI_ManifestedWeight = 50m;
			item2.HVI_ManifestedVolume = 50m;
			Factory.Save();

			AssertEquals("consignment1 Weight ignored due to invalid unit", 50m, header.HVH_GrossWeight);
			AssertEquals("consignment2 Volume ignored due to invalid unit", 30m, header.HVH_GrossVolume);

			consignment1.HVC_WeightUQ = Weight.Kilograms;
			consignment2.HVC_VolumeUQ = Volume.CubicMetres;
			header.HVH_GrossWeightUQ = "XX";
			header.HVH_GrossVolumeUQ = "XX";

			AssertEquals("Weight calculated as 0 due to invalid unit on BookingHeader", 0m, header.HVH_GrossWeight);
			AssertEquals("Volume calculated as 0 due to invalid unit on BookingHeader", 0m, header.HVH_GrossVolume);
		}

		public void TestUnitOfMeasureProperties_WhenSetValueLowerCase_GetIsUpperCase()
		{
			var header = Factory.New<HVLVBookingHeader>();
			header.HVH_GrossVolumeUQ = "m3";
			header.HVH_GrossWeightUQ = "kg";

			AssertEquals("M3", header.HVH_GrossVolumeUQ);
			AssertEquals("KG", header.HVH_GrossWeightUQ);
		}

		public void TestSaving()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			header.HVH_ClusterKey = 0;
			header.HVH_BookingReference = "";

			Factory.Save();

			Assert("Cluster key should be set", header.HVH_ClusterKey != 0);
			Assert("Booking reference should be set", !header.HVH_BookingReference.IsEmpty);
		}

		public void TestSaving_HVH_BookingReferenceComesFromHVLVBookingHeaderJobNumberFountain()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_BookingReference = "";
			Env.NumberFountains.HVLVBookingHeaderJobNumber.SetNext(Factory, 6734763);

			Factory.Save();

			AssertEquals("Booking reference should be set to 'M' + JobNumber with 0's padded to the left to a total 8 numnbers", "M06734763", header.HVH_BookingReference);
		}

		public void TestBookingHeader_AllItemsByBookingHeaderQuery()
		{
			var expectedProcessedQuery = @"WHERE (HVI_HVC_Consignment IN (SELECT HVC_PK FROM dbo.HVLVConsignment WHERE HVC_HVH_BookingHeader =";
			var expectedUnprocessedQuery = @"WHERE HVI_ClusterKey =";

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.Items.AddNew();
			consignment.Items.AddNew();

			Factory.Save();

			var sqlTracker = SqlEventTracker.Instance;
			sqlTracker.Clear();

			_ = bookingHeader.AllItems;

			AssertContains(expectedUnprocessedQuery, SqlEventTracker.Instance.LastSqlQuery);

			bookingHeader.HVH_IsProcessedAtOriginDepot = true;

			Factory.Save();

			sqlTracker.Clear();

			_ = bookingHeader.AllItems;

			AssertContains(expectedProcessedQuery, SqlEventTracker.Instance.LastSqlQuery);
		}

		public void TestBookingHeader_GetAllItemLinesByBookingHeaderQuery()
		{
			var expectedProcessedQuery = @"WHERE HVS_HVI_HVLVItem IN (SELECT HVI_PK FROM dbo.HVLVItem WHERE (HVI_HVC_Consignment IN (SELECT HVC_PK FROM dbo.HVLVConsignment WHERE HVC_HVH_BookingHeader =";
			var expectedUnprocessedQuery = @"WHERE HVS_ClusterKey =";

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.Items.AddNew();
			consignment.Items.AddNew();

			Factory.Save();

			var sqlTracker = SqlEventTracker.Instance;
			sqlTracker.Clear();

			_ = bookingHeader.AllItemLines;

			AssertContains(expectedUnprocessedQuery, SqlEventTracker.Instance.LastSqlQuery);

			bookingHeader.HVH_IsProcessedAtOriginDepot = true;

			Factory.Save();

			sqlTracker.Clear();

			_ = bookingHeader.AllItemLines;

			AssertContains(expectedProcessedQuery, SqlEventTracker.Instance.LastSqlQuery);
		}

		public void TestBookingHeaderOnSaving_WillBulkPopulateConsignment_UsingNumberFountainWhen_GS1InfoIsNull()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();
			var item2 = consignment1.Items.AddNew();
			var item3 = consignment2.Items.AddNew();
			var item4 = consignment2.Items.AddNew();

			CombineAssertions("preconditions", () =>
			{
				AssertNull("header GS1Info should be null", header.GS1Info);
				AssertNullOrEmpty(consignment1.HVC_ConsignmentId);
				AssertNullOrEmpty(consignment2.HVC_ConsignmentId);
				AssertNullOrEmpty(item1.HVI_ItemId);
				AssertNullOrEmpty(item2.HVI_ItemId);
				AssertNullOrEmpty(item3.HVI_ItemId);
				AssertNullOrEmpty(item4.HVI_ItemId);
			});

			Factory.Save();

			CombineAssertions("consignments and items should be populated", () =>
			{
				AssertEquals("HVC000000000000001", consignment1.HVC_ConsignmentId);
				AssertEquals("HVC000000000000002", consignment2.HVC_ConsignmentId);
				AssertEquals("HVI000000000000001", item1.HVI_ItemId);
				AssertEquals("HVI000000000000002", item2.HVI_ItemId);
				AssertEquals("HVI000000000000003", item3.HVI_ItemId);
				AssertEquals("HVI000000000000004", item4.HVI_ItemId);
			});
		}

		[UseSnapshotProtection(true)]
		public void TestBookingHeaderOnSaving_WillBulkPopulateConsignment_UsingGS1Info()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.NewWithValidTestData<HVLVBookingHeader>();
			var billToParty = factory.NewWithValidTestData<OrgAddress>();
			var code = billToParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "12345678");
			code.OK_OA_PremisesAddress = billToParty.PK;
			var orgHeader = factory.Load<OrgHeader>(billToParty.OA_OH);
			HVLVTestHelper.SetGS1FountainOnOrg(orgHeader, "12345678");
			header.HVH_OA_BillToParty = billToParty.PK;

			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();
			var item2 = consignment2.Items.AddNew();
			var item3 = consignment2.Items.AddNew();

			CombineAssertions("preconditions", () =>
			{
				AssertNotNull("header GS1Info should not be null", header.GS1Info);
				AssertNotNull(header.GS1Info.SSCCNumberFountain);
				AssertNullOrEmpty(consignment1.HVC_ConsignmentId);
				AssertNullOrEmpty(consignment2.HVC_ConsignmentId);
				AssertNullOrEmpty(item1.HVI_ItemId);
				AssertNullOrEmpty(item2.HVI_ItemId);
				AssertNullOrEmpty(item3.HVI_ItemId);
			});

			factory.Save();

			CombineAssertions("consignments and items should be populated", () =>
			{
				AssertEquals("012345678000000011", consignment1.HVC_ConsignmentId);
				AssertEquals("012345678000000028", consignment2.HVC_ConsignmentId);
				AssertEquals("012345678000000035", item1.HVI_ItemId);
				AssertEquals("012345678000000042", item2.HVI_ItemId);
				AssertEquals("012345678000000059", item3.HVI_ItemId);
			});
		}

		public void TestChangingBookingHeaderActiveStatus_DoesNotChangeConsignmentActiveStatus()
		{
			var header = Factory.New<HVLVBookingHeader>();
			header.HVH_IsActive = true;

			var consignment = header.Consignments.AddNew();
			consignment.HVC_IsActive = true;

			header.HVH_IsActive = false;

			Assert("Consignment is still active", consignment.HVC_IsActive);
		}

		#region ICancellable

		public void TestGivenActiveConsignments_ThenCannotCancelHeader()
		{
			var header = Factory.New<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			header.HVH_IsProcessedAtOriginDepot = false;
			consignment1.HVC_IsActive = true;
			consignment2.HVC_IsActive = true;

			Assert("Expect can NOT cancel when Header has active consignments", !string.IsNullOrEmpty(header.CanCancel()));

			consignment1.HVC_IsActive = false;

			Assert("Expect can NOT cancel when Header has active consignments", !string.IsNullOrEmpty(header.CanCancel()));

			consignment2.HVC_IsActive = false;

			Assert("Expect can cancel when Header has NO active consignments", string.IsNullOrEmpty(header.CanCancel()));
		}

		public void Test_GivenProcessedAtOriginDepot_ThenCannotCancelHeader()
		{
			var header = Factory.New<HVLVBookingHeader>();
			header.HVH_IsProcessedAtOriginDepot = true;

			Assert("Expect can NOT cancel when Header is processed at Origin Depot", !string.IsNullOrEmpty(header.CanCancel()));

			header.HVH_IsProcessedAtOriginDepot = false;

			Assert("Expect can cancel when Header is NOT processed at Origin Depot", string.IsNullOrEmpty(header.CanCancel()));
		}

		#endregion

		public void TestConsignmentLoading_WhenDataBinding_DoNotLoadIfCountEqualToOrLargerThanThreshold()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.AddNew();
			bookingHeader.Consignments.AddNew();

			Factory.Save();

			using (HVLVDataRegistry.Instance.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				AssertAreConsignmentsLoaded("Load all consignments if the count is under the threshold", useSetIsDataBinding: true, expectConsignmentsToBeLoaded: true);
			}

			using (HVLVDataRegistry.Instance.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				AssertAreConsignmentsLoaded("Load all consignments if the count is equal to the threshold", useSetIsDataBinding: true, expectConsignmentsToBeLoaded: true);
			}

			using (HVLVDataRegistry.Instance.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				AssertAreConsignmentsLoaded("Load all consignments if not during DataBinding and the count is above the threshold", useSetIsDataBinding: false, expectConsignmentsToBeLoaded: true);

				AssertAreConsignmentsLoaded("Don't load all consignments during DataBinding if the count is above the threshold", useSetIsDataBinding: true, expectConsignmentsToBeLoaded: false);
			}

			void AssertAreConsignmentsLoaded(string message, bool useSetIsDataBinding, bool expectConsignmentsToBeLoaded)
			{
				var headerInNewFactory = new BusinessObjectFactory().Load<HVLVBookingHeader>(bookingHeader.PK);

				using (useSetIsDataBinding
					? ((IHVLVConsignmentCollectionParent)headerInNewFactory).SetIsDataBinding()
					: null
					)
				{
					AssertEquals(message, expectConsignmentsToBeLoaded, headerInNewFactory.Consignments.Count > 0);
				}
			}
		}

		#region Transport Booking Related Tests

		public void TestIDtbBookingParentMembers()
		{
			var header = Factory.New<HVLVBookingHeader>();
			var dtbBookingParent = header as IDtbBookingParent;

			AssertNotNull("Booking header should implement IDtbBookingParent", dtbBookingParent);

			CombineAssertions("IDtbBookingParent members should be set correctly", () =>
			{
				AssertEquals("Controller ID", ControllerIDs.HVLVBookingHeader, dtbBookingParent.ControllerID);
				AssertEquals("Job Number", header.HVH_BookingReference, dtbBookingParent.JobNumber);
				AssertEquals("Job Status", "BKD", dtbBookingParent.JobStatus);
				AssertEquals("Job Description", "HVLV Booking Header", dtbBookingParent.JobDescription);
				AssertContainsExactElementsInExactOrder(new[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV }, dtbBookingParent.GetSupportedDirections());
			});
		}

		public void TestCanCreateTransportBooking()
		{
			var header = Factory.New<HVLVBookingHeader>();
			var dtbBookingParent = header as IDtbBookingParent;
			AssertEquals("CanCreateTransportBooking should always return true", true, dtbBookingParent.CanCreateTransportBooking);
		}

		public void TestBookingParentPK()
		{
			var header = Factory.New<HVLVBookingHeader>();
			var dtbBookingParent = header as IDtbBookingParent;
			AssertEquals("BookingParentPK should be the Header PK.", header.PK, dtbBookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var header = Factory.New<HVLVBookingHeader>();
			var dtbBookingParent = header as IDtbBookingParent;
			AssertEquals("BookingParentTablePrefix should be the Header table prefix.", header.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var header = Factory.New<HVLVBookingHeader>();
			var dtbBookingParent = header as IDtbBookingParent;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		void AssertContainsExactElementsInExactOrder(DtbBookingDirection[] expected, DtbBookingDirection[] actual)
		{
			AssertEquals(expected.Length, actual.Length);
			for (var i = 0; i < expected.Length; i++)
			{
				AssertEquals(expected[i], actual[i]);
			}
		}

		public void TestTransportBookingActualPickUpDateAdded_ItemsOnBookingHeaderUpdatesStatusToPUS()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			var consol = Helper.CreateConsolidation(bookingHeader);
			var booking = Helper.CreateBooking(consol);

			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickUpConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Actual date is blank, status should default to Available.", TransportStatuses.Codes.Available, instruction.KN_Status);
				AssertEquals("Precondition: HVI_Status for item1 is MAN.", HVLVItemStatus.Codes.ManifestedByETailer, item1.HVI_Status);
				AssertEquals("Precondition: HVI_Status for item2 is MAN.", HVLVItemStatus.Codes.ManifestedByETailer, item2.HVI_Status);
			});

			pickUpConfirmation.KK_Actual = ZDateTime.Now;
			instruction.UpdateStatus();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Actual date has been added, status should now be Picked Up.", TransportStatuses.Codes.PickedUp, instruction.KN_Status);
				AssertEquals("HVI_Status for item1 is now PUS.", HVLVItemStatus.Codes.PickUpFromShipper, item1.HVI_Status);
				AssertEquals("HVI_Status for item2 is now PUS.", HVLVItemStatus.Codes.PickUpFromShipper, item2.HVI_Status);
			});
		}

		public void TestTransportBookingActualDeliveryDateAdded_ItemsOnBookingHeaderUpdatesStatusToRAO()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			var consol = Helper.CreateConsolidation(bookingHeader);
			var booking = Helper.CreateBooking(consol);

			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var deliveryConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Actual date is blank, status should default to Available.", TransportStatuses.Codes.Available, instruction.KN_Status);
				AssertEquals("Precondition: HVI_Status for item1 is MAN.", HVLVItemStatus.Codes.ManifestedByETailer, item1.HVI_Status);
				AssertEquals("Precondition: HVI_Status for item2 is MAN.", HVLVItemStatus.Codes.ManifestedByETailer, item2.HVI_Status);
			});

			deliveryConfirmation.KK_Actual = ZDateTime.Now;
			instruction.UpdateStatus();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Actual date has been added, status should now be Delivered.", TransportStatuses.Codes.Delivered, instruction.KN_Status);
				AssertEquals("HVI_Status for item1 is now PUS.", HVLVItemStatus.Codes.ReceivedAtOrigin, item1.HVI_Status);
				AssertEquals("HVI_Status for item2 is now PUS.", HVLVItemStatus.Codes.ReceivedAtOrigin, item2.HVI_Status);
			});
		}

		public void TestTransportBookingDatesAdded_STUEventLogAddedOnItemsOnBookingHeader()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			var consol = Helper.CreateConsolidation(bookingHeader);
			var booking = Helper.CreateBooking(consol);

			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);

			Factory.Save();

			var logsOnItem1 = item1.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);
			var logsOnItem2 = item2.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: No logs on item1,", 0, logsOnItem1.Count());
				AssertEquals("Precondition: No logs on item2.", 0, logsOnItem2.Count());
			});

			pickupConfirmation.KK_Actual = ZDateTime.Now;
			instruction.UpdateStatus();
			Factory.Save();

			logsOnItem1 = item1.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);
			logsOnItem2 = item2.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);

			CombineAssertions(() =>
			{
				AssertEquals("There is 1 log for item1.", 1, logsOnItem1.Count());
				AssertEquals("There is 1 log for item2.", 1, logsOnItem2.Count());
			});

			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;

			var deliveryConfirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			deliveryConfirmation.KK_Actual = ZDateTime.Now;
			instruction.UpdateStatus();
			Factory.Save();

			logsOnItem1 = item1.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);
			logsOnItem2 = item2.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);

			CombineAssertions(() =>
			{
				AssertEquals("There are 2 logs for item1.", 2, logsOnItem1.Count());
				AssertEquals("There are 2 logs item2.", 2, logsOnItem2.Count());
			});

			var log1 = logsOnItem1.First();
			log1.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, out var log1OldValue);
			log1.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, out var log1NewValue);

			var log2 = logsOnItem2.Last();
			log2.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, out var log2OldValue);
			log2.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, out var log2NewValue);

			CombineAssertions(() =>
			{
				AssertEquals("Log 1 - old value should be MAN.", HVLVItemStatus.Codes.ManifestedByETailer, log1OldValue);
				AssertEquals("Log 1 - new value should be PUS.", HVLVItemStatus.Codes.PickUpFromShipper, log1NewValue);
				AssertEquals("Log 2 - old value should be PUS.", HVLVItemStatus.Codes.PickUpFromShipper, log2OldValue);
				AssertEquals("Log 2 - new value should be RAO.", HVLVItemStatus.Codes.ReceivedAtOrigin, log2NewValue);
			});
		}

		public void TestMultipleTransportBookingsActualDeliveryDateAdded_ItemsOnBookingHeaderUpdatesStatusToRAO()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			var consol1 = Helper.CreateConsolidation(bookingHeader);
			var booking1 = Helper.CreateBooking(consol1);

			var instruction1 = booking1.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickUpConfirmation1 = instruction1.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);

			var consol2 = Helper.CreateConsolidation(bookingHeader);
			consol2.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var booking2 = Helper.CreateBooking(consol1);

			var instruction2 = booking2.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickUpConfirmation2 = instruction2.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);

			var instruction3 = booking2.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var deliveryConfirmation = instruction3.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Actual date is blank, status should default to Available.", TransportStatuses.Codes.Available, instruction1.KN_Status);
				AssertEquals("Precondition: Actual date is blank, status should default to Available.", TransportStatuses.Codes.Available, instruction2.KN_Status);
				AssertEquals("Precondition: Actual date is blank, status should default to Available.", TransportStatuses.Codes.Available, instruction3.KN_Status);
				AssertEquals("Precondition: HVI_Status for item1 is MAN.", HVLVItemStatus.Codes.ManifestedByETailer, item1.HVI_Status);
				AssertEquals("Precondition: HVI_Status for item2 is MAN.", HVLVItemStatus.Codes.ManifestedByETailer, item2.HVI_Status);
			});

			pickUpConfirmation1.KK_Actual = ZDateTime.Now;
			instruction1.UpdateStatus();

			pickUpConfirmation2.KK_Actual = ZDateTime.Now;
			instruction2.UpdateStatus();

			deliveryConfirmation.KK_Actual = ZDateTime.Now;
			instruction3.UpdateStatus();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Actual date has been added, status should now be Picked Up.", TransportStatuses.Codes.PickedUp, instruction1.KN_Status);
				AssertEquals("Actual date has been added, status should now be Picked Up.", TransportStatuses.Codes.PickedUp, instruction2.KN_Status);
				AssertEquals("Actual date has been added, status should now be Delivered.", TransportStatuses.Codes.Delivered, instruction3.KN_Status);
				AssertEquals("HVI_Status for item1 is now RAO, since booking2 has instruction with DLV status.", HVLVItemStatus.Codes.ReceivedAtOrigin, item1.HVI_Status);
				AssertEquals("HVI_Status for item2 is now RAO, since booking2 has instruction with DLV status.", HVLVItemStatus.Codes.ReceivedAtOrigin, item2.HVI_Status);
			});
		}

		public void TestSavingConsignments_ShouldUseBulkSavingWhenPassThreshold()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			HVLVTestHelper.CreateDummyConsignments(bookingHeader.Consignments, Factory.DefaultBulkCopyThreshold() + 1);
			CombineAssertions("HVLVConsignment should have been BulkCopied", () =>
			{
				using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
				{
					AssertNoExceptionThrown(Factory.Save);
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVConsignmentSchema.Constants.TableName, ["FireTriggers"]));
				}
			});
		}

		public void TestBulkSaving_NoExceptionsThrown()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
			{
				var consignment = header.Consignments.AddNew();
				var item = consignment.Items.AddNew();
				item.Lines.AddNew();
				var outerPackage = Factory.New<HVLVOuterPackage>();
				item.HVI_HVO_OuterPackage = outerPackage.PK;
			}

			CombineAssertions("All these tables should have bulk save sql event", () =>
			{
				using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
				{
					AssertNoExceptionThrown(Factory.Save);
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVConsignmentSchema.Constants.TableName, ["FireTriggers"]));
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVOuterPackageSchema.Constants.TableName, ["FireTriggers"]));
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVItemLineSchema.Constants.TableName, ["FireTriggers"]));
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVItemSchema.Constants.TableName, ["CheckConstraints"]));
				}
			});
		}

		#region Helper

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		#endregion

		#endregion

		public void TestConsignmentsNotLoaded()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.Consignments.AddNew();
			Factory.Save();

			var headerInNewFactory = NewFactory().Load<HVLVBookingHeader>(header.PK);
			var parent = headerInNewFactory as IHVLVConsignmentCollectionParent;
			var consignmentsFieldInfo = headerInNewFactory.GetType().GetField("consignments", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var consignmentsField = (IBusinessObjectCollection)consignmentsFieldInfo.GetValue(headerInNewFactory);

			AssertNull("Scenario 1 Precondition: consignments field is null", consignmentsField);
			Assert("Scenario 1: When consignments is not assigned yet, returns true", headerInNewFactory.ConsignmentsNotLoaded);

			using (parent.SetIsDataBinding())
			using (HVLVDataRegistry.Instance.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				_ = headerInNewFactory.Consignments;
			}

			consignmentsField = (IBusinessObjectCollection)consignmentsFieldInfo.GetValue(headerInNewFactory);
			AssertNotNull("Scenario 2 Precondition: consignments field is not null", consignmentsField);
			Assert("Scenario 2 Precondition: consignments collection is not loaded", !consignmentsField.IsLoaded);
			Assert("Scenario 2: When consignments is not null and not loaded, returns true", headerInNewFactory.ConsignmentsNotLoaded);

			_ = headerInNewFactory.Consignments;

			consignmentsField = (IBusinessObjectCollection)consignmentsFieldInfo.GetValue(headerInNewFactory);
			Assert("Scenario 3 Precondition: consignments collection is loaded", consignmentsField.IsLoaded);
			Assert("Scenario 3: When consignments is loaded, returns false", !headerInNewFactory.ConsignmentsNotLoaded);
		}

		#region IScreeningPartyProvider Members

		public void TestGetWorstScreeningStatus()
		{
			var billToPartyAddress = Factory.NewWithValidTestData<OrgAddress>();
			billToPartyAddress.Header.OH_Code = "B2PARTY";
			billToPartyAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var dispatchAddress = Factory.NewWithValidTestData<OrgAddress>();
			dispatchAddress.Header.OH_Code = "DISPATCH";
			dispatchAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = billToPartyAddress.PK;
			bookingHeader.HVH_OA_DispatchAddress = dispatchAddress.PK;

			var screeningPartyProvider = bookingHeader as IScreeningPartyProvider;
			AssertEquals(ScreeningStatusesList.Codes.Matched, screeningPartyProvider.GetWorstScreeningStatus());
		}

		public void TestGetWorstScreeningStatusUnlessManuallyCleared()
		{
			var billToPartyAddress = Factory.NewWithValidTestData<OrgAddress>();
			billToPartyAddress.Header.OH_Code = "B2PARTY";
			billToPartyAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = billToPartyAddress.PK;

			var screeningPartyProvider = bookingHeader as IScreeningPartyProvider;
			AssertEquals("Precondition: worst status is currently MAT", ScreeningStatusesList.Codes.Matched, screeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared());

			bookingHeader.HVH_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertEquals(ScreeningStatusesList.Codes.JobCleared, screeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared());
		}

		public void TestScreeningParties()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var screeningPartyProvider = bookingHeader as IScreeningPartyProvider;

			var billToPartyAddress = Factory.NewWithValidTestData<OrgAddress>();
			billToPartyAddress.Header.OH_Code = "B2PARTY";

			var dispatchAddress = Factory.NewWithValidTestData<OrgAddress>();
			dispatchAddress.Header.OH_Code = "DISPATCH";

			var originDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			originDepotAddress.Header.OH_Code = "DEPOT";

			bookingHeader.HVH_OA_BillToParty = billToPartyAddress.PK;
			bookingHeader.HVH_OA_DispatchAddress = dispatchAddress.PK;
			bookingHeader.HVH_OA_OriginDepot = originDepotAddress.PK;

			Factory.Save();

			var screeningParties = screeningPartyProvider.ScreeningParties;
			var billToPartyScreeningParty = screeningParties.Single(x => x.OrgCode == "B2PARTY");
			var dispatchAddressScreeningParty = screeningParties.Single(x => x.OrgCode == "DISPATCH");
			var originDepotScreeningParty = screeningParties.Single(x => x.OrgCode == "DEPOT");

			CombineAssertions(() =>
			{
				AssertEquals(3, screeningParties.Length);
				AssertEquals("Bill to Party", billToPartyScreeningParty.Description);
				AssertEquals("Dispatch Address", dispatchAddressScreeningParty.Description);
				AssertEquals("Origin Depot", originDepotScreeningParty.Description);
			});
		}

		public void TestScreeningStatus()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var screeningPartyProvider = bookingHeader as IScreeningPartyProvider;

			bookingHeader.HVH_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertEquals("Screening status change should be reflected on booking header", ScreeningStatusesList.Codes.JobCleared, screeningPartyProvider.ScreeningStatus);
		}

		#endregion

		public void TestHVH_DeniedPartyScreeningStatus()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var deniedPartyScreeningStatusInfo = bookingHeader.HVH_DeniedPartyScreeningStatusInfo;
			var deniedPartyScreeningStatusData = DataBoundResourceStrings.GetDataForProperty(deniedPartyScreeningStatusInfo);

			CombineAssertions(() =>
			{
				Assert("Is read-only", deniedPartyScreeningStatusInfo.ReadOnly);
				AssertEquals("Screening Status", deniedPartyScreeningStatusData.Caption);
			});
		}

		public void TestConsignmentDPSStatusDoesNotAffectBookingHeaderDPSStatus()
		{
			var billToPartyAddress = Factory.NewWithValidTestData<OrgAddress>();
			billToPartyAddress.Header.OH_Code = "B2PARTY";
			billToPartyAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			var destinationDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepotAddress.Header.OH_Code = "B2PARTY";
			destinationDepotAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = billToPartyAddress.PK;

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OA_DestinationDepot = destinationDepotAddress.PK;

			var consignmentScreeningPartyProvider = consignment as IScreeningPartyProvider;
			AssertEquals("Precondition: consignment worst status is MAT", ScreeningStatusesList.Codes.Matched, consignmentScreeningPartyProvider.GetWorstScreeningStatus());

			var bookingHeaderScreeningPartyProvider = bookingHeader as IScreeningPartyProvider;
			AssertEquals("Booking header worst status is NOT", ScreeningStatusesList.Codes.NotScreened, bookingHeaderScreeningPartyProvider.GetWorstScreeningStatus());
		}

		public void TestBookingHeaderDPSStatusAffectsAllConsignmentDPSStatusesWhenMatched()
		{
			var billToPartyAddress = Factory.NewWithValidTestData<OrgAddress>();
			billToPartyAddress.Header.OH_Code = "B2PARTY";
			billToPartyAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = billToPartyAddress.PK;

			var consignment = bookingHeader.Consignments.AddNew();
			var consignmentScreeningPartyProvider = consignment as IScreeningPartyProvider;
			var bookingHeaderScreeningPartyProvider = bookingHeader as IScreeningPartyProvider;

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("Booking header DPS status is NOT", ScreeningStatusesList.Codes.NotScreened, bookingHeaderScreeningPartyProvider.ScreeningStatus);
				AssertEquals("Consignment DPS status is NOT", ScreeningStatusesList.Codes.NotScreened, consignmentScreeningPartyProvider.ScreeningStatus);
			});

			bookingHeaderScreeningPartyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertEquals("Consignment DPS status not changed", ScreeningStatusesList.Codes.NotScreened, consignment.HVC_DeniedPartyScreeningStatus);

			bookingHeaderScreeningPartyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertEquals("Consignment DPS status changed to matched", ScreeningStatusesList.Codes.Matched, consignment.HVC_DeniedPartyScreeningStatus);
		}

		public void TestNoAuditLog()
		{
			var newFactoryForLoading = new BusinessObjectFactory();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var query = new ZQuery(StmALogSchema.SL_Parent, bookingHeader.PK);
			Factory.Save();
			Assert("Not expecting Add event.", !newFactoryForLoading.Exists(typeof(StmALog), query));

			bookingHeader.HVH_BookingReference = "M0001";
			Factory.Save();
			Assert("Not expecting Edit event.", !newFactoryForLoading.Exists(typeof(StmALog), query));

			bookingHeader.Delete();
			Factory.Save();
			Assert("Not expecting Delete event.", !newFactoryForLoading.Exists(typeof(StmALog), query));
		}

		#region IDpsEntityProvider

		public void TestInvalidateScreeningStatusesNotChangeStatusWhenCurrentStatusIsCLP()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			bookingHeader.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, bookingHeader.HVH_DeniedPartyScreeningStatus);
		}

		public void TestInvalidateScreeningStatusesChangeStatus()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Matched;
			bookingHeader.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, bookingHeader.HVH_DeniedPartyScreeningStatus);

			bookingHeader.HVH_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			bookingHeader.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, bookingHeader.HVH_DeniedPartyScreeningStatus);
		}

		#endregion
	}

	[TestedType(typeof(HVLVBookingHeader))]
	public class HVLVBookingHeaderIDtbBookingParentTestCase : IDtbBookingParentTestCase<HVLVBookingHeader>
	{
		protected override HVLVBookingHeader GetNewParent() => Factory.New<HVLVBookingHeader>();

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking() => true;

		protected override bool CanHaveDirectCartageChild => false;
	}

	public class HVLVBookingHeaderJobNumberUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override SchemaColumn ColumnThatUsesNumberFountain => HVLVBookingHeaderSchema.HVH_BookingReference;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.HVLVBookingHeaderJobNumber;

		protected override Type BizOTypeToTest => typeof(HVLVBookingHeader);

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			var organisation = Factory.NewWithValidTestData<OrgAddress>();
			((HVLVBookingHeader)testBizO).HVH_OA_BillToParty = organisation.PK;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				if (fInsertValues == null)
				{
					fInsertValues = base.AdditionalInsertValues;

					var organisation = Factory.NewWithValidTestData<OrgAddress>();
					Factory.Save();

					fInsertValues.Add(HVLVBookingHeaderSchema.HVH_OA_BillToParty.Name, string.Format("'{0}'", organisation.PK.ToString()));
					fInsertValues.Add(HVLVBookingHeaderSchema.HVH_ClusterKey.Name, ClusterKeyThatWillNotConflict.ToString());
				}

				return fInsertValues;
			}
		}

		NameValueCollection fInsertValues;

		const int ClusterKeyThatWillNotConflict = 1000;
	}

	public class HVLVBookingHeaderClusterKeyUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override SchemaColumn ColumnThatUsesNumberFountain => HVLVBookingHeaderSchema.HVH_ClusterKey;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.HVLVConsignmentClusterKey;

		protected override Type BizOTypeToTest => typeof(HVLVBookingHeader);

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			var organisation = Factory.NewWithValidTestData<OrgAddress>();
			((HVLVBookingHeader)testBizO).HVH_OA_BillToParty = organisation.PK;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				if (fInsertValues == null)
				{
					fInsertValues = base.AdditionalInsertValues;

					var organisation = Factory.NewWithValidTestData<OrgAddress>();
					Factory.Save();

					fInsertValues.Add(HVLVBookingHeaderSchema.HVH_OA_BillToParty.Name, string.Format("'{0}'", organisation.PK.ToString()));
					fInsertValues.Add(HVLVBookingHeaderSchema.HVH_BookingReference.Name, string.Format("'{0}'", JobNumberThatWillNotConflict));
				}

				return fInsertValues;
			}
		}

		NameValueCollection fInsertValues;

		const string JobNumberThatWillNotConflict = "M00001000";
	}
}
