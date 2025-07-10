using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.CustomsReferenceNumberType;
using DtbBookingDirection = Enterprise.TransportCommon.Shared.DtbBookingDirection;
using EventReferenceConstants = CargoWise.EventReference.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignment))]
	public class HVLVConsignmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSaveUsingBulkCopyFailed_ConsignmentIDGetsCleared()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			Factory.Save();

			for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
			{
				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.Items.AddNew();
			}

			Factory.Saving += delegate
			{
				throw new Exception();
			};

			AssertExceptionThrown<Exception>(Factory.Save);

			AssertEquals("HVC_ConsignmentID should be empty after save failure", ZString.Empty, consignmentHeader.Consignments[0].HVC_ConsignmentId);
		}

		public void TestBulkSaveHVLVConsignments()
		{
			for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
			{
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			}

			using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
			{
				AssertNoExceptionThrown(Factory.Save);
				Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVConsignmentSchema.Constants.TableName, ["FireTriggers"]));
			}
		}

		public void TestTotalsCalculatedBeforeSaving_BookingHeaderAndConsignment_WhenBookingHeaderIsInDataBase()
		{
			var billToParty = Factory.NewWithValidTestData<OrgAddress>();

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_GrossWeightUQ = Weight.Kilograms;
			bookingHeader.HVH_GrossVolumeUQ = Volume.CubicFeet;

			Factory.Save();

			var bulkCopyThreshold = Factory.DefaultBulkCopyThreshold();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WeightUQ = Weight.Ounces;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;

			for (var i = 0; i < bulkCopyThreshold; i++)
			{
				var item1 = consignment.Items.AddNew();
				var item2 = consignment.Items.AddNew();

				item1.HVI_ActualVolume = 111;
				item1.HVI_ManifestedVolume = 101;
				item2.HVI_ActualVolume = 222;
				item2.HVI_ManifestedVolume = 202;

				item1.HVI_ActualWeight = 11;
				item1.HVI_ManifestedWeight = 10;
				item2.HVI_ActualWeight = 22;
				item2.HVI_ManifestedWeight = 20;
			}

			consignment.HVC_ActualWeight = 0;
			consignment.HVC_ActualVolume = 0;
			consignment.HVC_ManifestedWeight = 0;
			consignment.HVC_ManifestedWeight = 0;
			consignment.HVC_ItemCount = 0;

			using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
			{
				CombineAssertions("consignment properties has been calculated", () =>
				{
					AssertNoExceptionThrown(Factory.Save);
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVItemSchema.Constants.TableName, ["CheckConstraints"]));
					AssertEquals(33m * bulkCopyThreshold, consignment.HVC_ActualWeight);
					AssertEquals(333m * bulkCopyThreshold, consignment.HVC_ActualVolume);
					AssertEquals(30m * bulkCopyThreshold, consignment.HVC_ManifestedWeight);
					AssertEquals(303m * bulkCopyThreshold, consignment.HVC_ManifestedVolume);
					AssertEquals((short)(2 * bulkCopyThreshold), consignment.HVC_ItemCount);
				});
			}
		}

		public void TestTotalsCalculatedBeforeSaving_Consignment()
		{
			var billToParty = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var bulkCopyThreshold = Factory.DefaultBulkCopyThreshold();
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WeightUQ = Weight.Ounces;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;

			for (var i = 0; i < bulkCopyThreshold; i++)
			{
				var item1 = consignment.Items.AddNew();
				var item2 = consignment.Items.AddNew();
				var itemInactive = consignment.Items.AddNew();

				itemInactive.HVI_IsActive = false;

				item1.HVI_ActualVolume = 111;
				item1.HVI_ManifestedVolume = 101;
				item2.HVI_ActualVolume = 222;
				item2.HVI_ManifestedVolume = 202;
				itemInactive.HVI_ActualVolume = 200;
				itemInactive.HVI_ManifestedVolume = 300;

				item1.HVI_ActualWeight = 11;
				item1.HVI_ManifestedWeight = 10;
				item2.HVI_ActualWeight = 22;
				item2.HVI_ManifestedWeight = 20;
				itemInactive.HVI_ActualWeight = 33;
				itemInactive.HVI_ManifestedWeight = 30;
			}

			consignment.HVC_ActualWeight = 0;
			consignment.HVC_ActualVolume = 0;
			consignment.HVC_ManifestedWeight = 0;
			consignment.HVC_ManifestedWeight = 0;
			consignment.HVC_ItemCount = 0;

			using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
			{
				CombineAssertions("consignment properties has been calculated", () =>
				{
					AssertNoExceptionThrown(Factory.Save);
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVItemSchema.Constants.TableName, ["CheckConstraints"]));
					AssertEquals(33m * bulkCopyThreshold, consignment.HVC_ActualWeight);
					AssertEquals(333m * bulkCopyThreshold, consignment.HVC_ActualVolume);
					AssertEquals(30m * bulkCopyThreshold, consignment.HVC_ManifestedWeight);
					AssertEquals(303m * bulkCopyThreshold, consignment.HVC_ManifestedVolume);
					AssertEquals((short)(2 * bulkCopyThreshold), consignment.HVC_ItemCount);
				});
			}
		}

		public void TestTotalsCalculatedBeforeSaving_ConsignmentHeader()
		{
			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeaderForTest>();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_WeightUQ = Weight.Ounces;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			var bulkCopyThreshold = Factory.DefaultBulkCopyThreshold();

			for (var i = 0; i < bulkCopyThreshold; i++)
			{
				var item1 = consignment.Items.AddNew();
				var item2 = consignment.Items.AddNew();
				var itemInactive = consignment.Items.AddNew();

				itemInactive.HVI_IsActive = false;

				item1.HVI_ActualVolume = 111;
				item1.HVI_ManifestedVolume = 101;
				item2.HVI_ActualVolume = 222;
				item2.HVI_ManifestedVolume = 202;
				itemInactive.HVI_ActualVolume = 200;
				itemInactive.HVI_ManifestedVolume = 300;

				item1.HVI_ActualWeight = 11;
				item1.HVI_ManifestedWeight = 10;
				item2.HVI_ActualWeight = 22;
				item2.HVI_ManifestedWeight = 20;
				itemInactive.HVI_ActualWeight = 33;
				itemInactive.HVI_ManifestedWeight = 30;
			}

			consignment.HVC_ActualWeight = 0;
			consignment.HVC_ActualVolume = 0;
			consignment.HVC_ManifestedWeight = 0;
			consignment.HVC_ManifestedWeight = 0;
			consignment.HVC_ItemCount = 0;

			using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
			{
				CombineAssertions("consignment properties has been calculated", () =>
				{
					AssertNoExceptionThrown(Factory.Save);
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVItemSchema.Constants.TableName, ["CheckConstraints"]));
					AssertEquals(33m * bulkCopyThreshold, consignment.HVC_ActualWeight);
					AssertEquals(333m * bulkCopyThreshold, consignment.HVC_ActualVolume);
					AssertEquals(30m * bulkCopyThreshold, consignment.HVC_ManifestedWeight);
					AssertEquals(303m * bulkCopyThreshold, consignment.HVC_ManifestedVolume);
					AssertEquals((short)(2 * bulkCopyThreshold), consignment.HVC_ItemCount);
				});
			}
		}

		public void TestDefaultReferencesForNewConsignment_Shipment_Deduplication()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				const string ShipperReference = "SHIPPER REF";

				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

				var consignmentA = consignmentHeader.Consignments.AddNew();
				var consignmentB = consignmentHeader.Consignments.AddNew();

				consignmentA.HVC_ShipperReference = ShipperReference;
				consignmentB.HVC_ShipperReference = ShipperReference;

				consignmentA.OnSaving();
				AssertEquals("consignmentA should take SHIPPER REF because it's not duplicate yet", ShipperReference, consignmentA.HVC_ConsignmentId);

				consignmentB.OnSaving();
				AssertNotEquals("consignmentB should NOT take SHIPPER REF because it's taken", ShipperReference, consignmentB.HVC_ConsignmentId);
			}
		}

		public void TestDefaultReferencesForNewConsignment_BookingHeader_Deduplication()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				const string ShipperReference = "SHIPPER REF";

				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

				var consignmentA = bookingHeader.Consignments.AddNew();
				var consignmentB = bookingHeader.Consignments.AddNew();

				consignmentA.HVC_ShipperReference = ShipperReference;
				consignmentB.HVC_ShipperReference = ShipperReference;

				consignmentA.OnSaving();
				AssertEquals("consignmentA should take SHIPPER REF because it's not duplicate yet", ShipperReference, consignmentA.HVC_ConsignmentId);

				consignmentB.OnSaving();
				AssertNotEquals("consignmentB should NOT take SHIPPER REF because it's taken", ShipperReference, consignmentB.HVC_ConsignmentId);
			}
		}

		public void TestConsignmentIdRemovedFromCacheIfSaveFailed()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_WaybillNumber = "999888777";

				var consignmentIDCache = consignment.ConsignmentIDCache_Exposed;

				consignment.OnSaving();

				AssertCollectionContains("Consignment ID should be defaulted from WaybillNumber and cached", "999888777", consignmentIDCache);

				((ITransactionParticipant)Factory).OnAllTransactionsRolledBack();

				AssertCollectionNotContains("Consignment ID should be removed from cache once save failed", "999888777", consignmentIDCache);
			}
		}

		public void TestClusterKeyChangeFromBookingHeadersToConsignmentHeaders()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_ClusterKey = 123;
			bookingHeader.HVH_IsProcessedAtOriginDepot = true;
			var consignment = bookingHeader.Consignments.AddNew();

			AssertEquals("Precondition: consignment has the same clusterkey as bookingheader", 123, consignment.HVC_ClusterKey);

			var hvlShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = hvlShipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.HCH_ClusterKey = 321;

			consignment.HVC_HCH_Header = consignmentHeader.PK;
			AssertEquals("Consignment has the same clusterkey as consignmentHeader", 321, consignment.HVC_ClusterKey);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestReportError_AssignBookingHeaderWhenConsignmentHeaderExists()
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			using (Globals.TemporaryOverrideForIsTest(false))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				ExceptionReporter.Instance.TestingDoReportException.Value = true;
				var hvlShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				var consignmentHeader = hvlShipment.GetOrCreateHVLVConsignmentHeader();
				consignmentHeader.HCH_ClusterKey = 123;
				var consignment = consignmentHeader.Consignments.AddNew();

				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				consignment.HVC_HVH_BookingHeader = bookingHeader.PK;

				errorReporterMock.Verify(reporter => reporter.Report("HVLVConsignment|AssignBookingHeaderWhenHavingConsignmentHeaderError", "Should not assign a bookingHeader to a consignment after it's allocated onto a shipment.", null), Times.Once);
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestReportError_ChangeBookingHeader()
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			using (Globals.TemporaryOverrideForIsTest(false))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				ExceptionReporter.Instance.TestingDoReportException.Value = true;
				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				bookingHeader.HVH_ClusterKey = 123;
				var consignment = bookingHeader.Consignments.AddNew();
				AssertEquals("Precondition: consignment has same clusterKey as its bookingHeader", 123, consignment.HVC_ClusterKey);

				var newBookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
				newBookingHeader.HVH_ClusterKey = 321;
				consignment.HVC_HVH_BookingHeader = newBookingHeader.PK;

				errorReporterMock.Verify(reporter => reporter.Report("HVLVConsignment|ChangeBookingHeaderError", "Should not change bookingHeader for a consignment.", null), Times.Once);
			}
		}

		public void TestSetIsValidatedForUniquenessToTrue_WhenAutoGenerateID()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Factory.Save();
				Assert("Validated for uniqueness is true", consignment.HVC_IsValidatedForUniqueness);
			}
		}

		public void TestHVC_ReleaseStatusUpdatesHVI_ReleaseStatus()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			AssertEquals("precondition: HVI_ReleaseStatus is None by default", HVLVReleaseStatus.ShortVersion.None, item.HVI_ReleaseStatus);

			CombineAssertions(() =>
			{
				consignment.HVC_ReleaseStatus = HVLVReleaseStatus.Held;
				AssertEquals("HVI_ReleaseStatus should be Held ", HVLVReleaseStatus.ShortVersion.Held, item.HVI_ReleaseStatus);

				consignment.HVC_ReleaseStatus = HVLVReleaseStatus.Cleared;
				AssertEquals("HVI_ReleaseStatus should be Cleared ", HVLVReleaseStatus.ShortVersion.Cleared, item.HVI_ReleaseStatus);

				consignment.HVC_ReleaseStatus = HVLVReleaseStatus.None;
				AssertEquals("HVI_ReleaseStatus should be None ", HVLVReleaseStatus.ShortVersion.None, item.HVI_ReleaseStatus);
			});
		}

		public void TestHVC_ImportReleaseStatusUpdatesHVI_ImportReleaseStatus()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			AssertEquals("precondition: HVI_ImportReleaseStatus is None by default", HVLVReleaseStatus.ShortVersion.None, item.HVI_ImportReleaseStatus);

			CombineAssertions(() =>
			{
				consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;
				AssertEquals("HVI_ImportReleaseStatus should be Held ", HVLVReleaseStatus.ShortVersion.Held, item.HVI_ImportReleaseStatus);

				consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
				AssertEquals("HVI_ImportReleaseStatus should be Cleared ", HVLVReleaseStatus.ShortVersion.Cleared, item.HVI_ImportReleaseStatus);

				consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
				AssertEquals("HVI_ImportReleaseStatus should be None ", HVLVReleaseStatus.ShortVersion.None, item.HVI_ImportReleaseStatus);
			});
		}

		public void TestHVC_ExportReleaseStatusUpdatesHVI_ExportReleaseStatus()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			AssertEquals("precondition: HVI_ExportReleaseStatus is None by default", HVLVReleaseStatus.ShortVersion.None, item.HVI_ExportReleaseStatus);

			CombineAssertions(() =>
			{
				consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;
				AssertEquals("HVI_ExportReleaseStatus should be Held ", HVLVReleaseStatus.ShortVersion.Held, item.HVI_ExportReleaseStatus);

				consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Cleared;
				AssertEquals("HVI_ExportReleaseStatus should be Cleared ", HVLVReleaseStatus.ShortVersion.Cleared, item.HVI_ExportReleaseStatus);

				consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.None;
				AssertEquals("HVI_ExportReleaseStatus should be None ", HVLVReleaseStatus.ShortVersion.None, item.HVI_ExportReleaseStatus);
			});
		}

		public void TestReleaseStatusForCurrentDirection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
				consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;

				consignment.HVC_RN_NKShipperCountryCode = "US";
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				AssertEquals(Directions.Import, consignment.DirectionOfTrade);
				AssertEquals("release status should be the same as HVC_ImportReleaseStatus when direction is import", HVLVReleaseStatus.Cleared, consignment.ReleaseStatusForCurrentDirection);

				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				AssertEquals(Directions.Import, consignment.DirectionOfTrade);
				AssertEquals("release status should be the same as HVC_ImportReleaseStatus when direction is import", HVLVReleaseStatus.Cleared, consignment.ReleaseStatusForCurrentDirection);

				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = "US";
				AssertEquals(Directions.Export, consignment.DirectionOfTrade);
				AssertEquals("release status should be the same as HVC_ExportReleaseStatus when direction is export", HVLVReleaseStatus.Held, consignment.ReleaseStatusForCurrentDirection);

				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertEquals(Directions.Export, consignment.DirectionOfTrade);
				AssertEquals("release status should be the same as HVC_ExportReleaseStatus when direction is export", HVLVReleaseStatus.Held, consignment.ReleaseStatusForCurrentDirection);

				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = "US";
				AssertEquals(Directions.Export, consignment.DirectionOfTrade);
				AssertEquals("release status should be the same as HVC_ExportReleaseStatus when direction is export", HVLVReleaseStatus.Held, consignment.ReleaseStatusForCurrentDirection);

				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertEquals(Directions.Unknown, consignment.DirectionOfTrade);
				AssertEquals("release status should be empty if direction is not import or export", string.Empty, consignment.ReleaseStatusForCurrentDirection);

				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				AssertEquals(Directions.Domestic, consignment.DirectionOfTrade);
				AssertEquals("release status should be empty if direction is not import or export", string.Empty, consignment.ReleaseStatusForCurrentDirection);

				consignment.HVC_RN_NKShipperCountryCode = "US";
				consignment.HVC_RN_NKConsigneeCountryCode = "IE";
				AssertEquals(Directions.CrossTrade, consignment.DirectionOfTrade);
				AssertEquals("release status should be empty if direction is not import or export", string.Empty, consignment.ReleaseStatusForCurrentDirection);
			}
		}

		#region Customs Status Calculation

		public void TestSetHVC_ExportCustomsClearanceStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_ImportReleaseStatus = "HLD";

				consignment.HVC_ExportCustomsClearanceStatus = "INV";
				AssertEquals("Should update HVC_ExportReleaseStatus", HVLVReleaseStatus.Held, consignment.HVC_ExportReleaseStatus);

				consignment.HVC_ExportCustomsClearanceStatus = "REL";
				AssertEquals("Should not update HVC_ImportReleaseStatus", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);
				AssertEquals("Should update HVC_ExportReleaseStatus", HVLVReleaseStatus.Cleared, consignment.HVC_ExportReleaseStatus);
				AssertEquals("Should update HVC_ReleaseStatus", HVLVReleaseStatus.Cleared, consignment.HVC_ReleaseStatus);

				consignment.HVC_ExportCustomsClearanceStatus = string.Empty;
				AssertEquals("Should not update HVC_ExportReleaseStatus when clear HVC_ExportCustomsClearanceStatus", HVLVReleaseStatus.Cleared, consignment.HVC_ExportReleaseStatus);
			}
		}

		public void TestSetHVC_ImportCustomsClearanceStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_ExportReleaseStatus = "HLD";

				consignment.HVC_ImportCustomsClearanceStatus = "INV";
				AssertEquals("Should update HVC_ImportReleaseStatus", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);

				consignment.HVC_ImportCustomsClearanceStatus = "REL";
				AssertEquals("Should update HVC_ImportReleaseStatus", HVLVReleaseStatus.Cleared, consignment.HVC_ImportReleaseStatus);
				AssertEquals("Should update HVC_ReleaseStatus", HVLVReleaseStatus.Cleared, consignment.HVC_ReleaseStatus);

				consignment.HVC_ImportCustomsClearanceStatus = string.Empty;
				AssertEquals("Should not update HVC_ImportReleaseStatus when clear HVC_ImportCustomsClearanceStatus", HVLVReleaseStatus.Cleared, consignment.HVC_ImportReleaseStatus);
			}
		}

		public void TestImportCustomsClearanceStatusDescription_CalculatedWithConsignmentShipmentDestinationCountry()
		{
			var consignmentAUImport = Factory.New<HVLVConsignment>();
			consignmentAUImport.HVC_RN_NKConsigneeCountryCode = "AU";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "AUC", "Cleared in Australia", "CLR", "CSTA", countryCode: CountryCodes.Australia);
				consignmentAUImport.HVC_ImportCustomsClearanceStatus = "AUC";
				AssertEquals("Cleared in Australia", consignmentAUImport.ImportCustomsClearanceStatusDescription);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				AssertEquals("Cleared in Australia", consignmentAUImport.ImportCustomsClearanceStatusDescription);

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "NZC", "Cleared in New Zealand", "CLR", "CSTA", countryCode: CountryCodes.NewZealand);
				consignmentAUImport.HVC_ImportCustomsClearanceStatus = "NZC";
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "NZAKL";
				consignmentAUImport.ManagingShipment = shipment;

				AssertEquals("Cleared in New Zealand", consignmentAUImport.ImportCustomsClearanceStatusDescription);
			}
		}

		public void TestExportCustomsClearanceStatusDescription_CalculatedWithConsignmentShipmentOriginCountry()
		{
			var consignmentAUExport = Factory.New<HVLVConsignment>();
			consignmentAUExport.HVC_RN_NKShipperCountryCode = "AU";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "AUC", "Cleared in Australia", "CLR", "CSTEX", countryCode: CountryCodes.Australia);
				consignmentAUExport.HVC_ExportCustomsClearanceStatus = "AUC";
				AssertEquals("Cleared in Australia", consignmentAUExport.ExportCustomsClearanceStatusDescription);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				AssertEquals("Cleared in Australia", consignmentAUExport.ExportCustomsClearanceStatusDescription);

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "NZC", "Cleared in New Zealand", "CLR", "CSTA", countryCode: CountryCodes.NewZealand);
				consignmentAUExport.HVC_ExportCustomsClearanceStatus = "NZC";
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USLAX";
				consignmentAUExport.ManagingShipment = shipment;

				AssertEquals("Cleared in New Zealand", consignmentAUExport.ExportCustomsClearanceStatusDescription);
			}
		}

		public void TestImportCustomsClearanceStatusDescription_ErrorReportContainsRequiredInformation()
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.Code;

				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
				var importDeclaration = Factory.New<BaseJobDeclaration>();
				importDeclaration.JE_MessageSubType = "TS1";
				importDeclaration.IsCancelled = true;
				var exportDeclaration = Factory.New<BaseJobDeclaration>();
				exportDeclaration.JE_MessageSubType = "TS2";
				exportDeclaration.IsCancelled = false;

				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.UnitedStates;
				consignment.HVC_JE_ImportDeclaration = importDeclaration.PK;
				consignment.HVC_JE_ExportDeclaration = exportDeclaration.PK;

				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "###", "### description", countryCode: CountryCodes.NewZealand);
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "%%%", "%%% description", countryCode: CountryCodes.UnitedStates);
				Factory.Save();

				consignment.HVC_ImportCustomsClearanceStatus = "###";
				var description = consignment.ImportCustomsClearanceStatusDescription;
				AssertEquals("### - Description Unknown", description);
			}
		}

		#region Ireland

		public void TestMappingOfReleaseStatusAndCustomsStatus_IE()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var consignment = Factory.New<HVLVConsignment>();
				foreach (var customsStatusCode in new AISEntryStatusList().GetAllCodes())
				{
					consignment.HVC_ImportCustomsClearanceStatus = customsStatusCode;
					var expectReleaseStatus = string.Empty;
					switch (customsStatusCode)
					{
						case AISEntryStatusList.Codes.Released:
							expectReleaseStatus = HVLVReleaseStatus.Cleared;
							break;
						case AISEntryStatusList.Codes.Accepted:
						case AISEntryStatusList.Codes.AmendmentRequested:
						case AISEntryStatusList.Codes.AmendmentRequestRegistration:
						case AISEntryStatusList.Codes.AwaitingSupplementaryDeclaration:
						case AISEntryStatusList.Codes.CancellationRequested:
						case AISEntryStatusList.Codes.Cancelled:
						case AISEntryStatusList.Codes.Control:
						case AISEntryStatusList.Codes.GeneralNotificationReceived:
						case AISEntryStatusList.Codes.InsufficientFund:
						case AISEntryStatusList.Codes.Invalid:
						case AISEntryStatusList.Codes.NotReleased:
						case AISEntryStatusList.Codes.Prelodged:
						case AISEntryStatusList.Codes.RefundApplicationAccepted:
						case AISEntryStatusList.Codes.RefundApplicationRejected:
						case AISEntryStatusList.Codes.RefundApplicationRequested:
						case AISEntryStatusList.Codes.Registered:
						case AISEntryStatusList.Codes.Rejected:
							expectReleaseStatus = HVLVReleaseStatus.Held;
							break;
					}

					AssertEquals(expectReleaseStatus, consignment.HVC_ImportReleaseStatus);
					AssertEquals(expectReleaseStatus, consignment.HVC_ReleaseStatus);
				}
			}
		}

		public void TestImportReleaseStatusCalculatedFromCustomsStatus_Import_IE()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var consignment = Factory.New<HVLVConsignment>();
				AssertEquals("precondition - HVC_ImportReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ImportReleaseStatus);

				AssertEquals("precondition - ImportReleaseStatusDesciption is None by default", HVLVReleaseStatus.NoneDescription, consignment.ImportReleaseStatusDescription);
				AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.ImportCustomsClearanceStatusDescription);

				consignment.HVC_ImportCustomsClearanceStatus = "ACC";
				AssertEquals("HVC_ImportReleaseStatus should be HLD", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);

				AssertEquals("ImportReleaseStatusDesciption should be Held", HVLVReleaseStatus.HeldDescription, consignment.ImportReleaseStatusDescription);
				AssertEquals("CustomsStatusDescription", "Accepted - Accepted", consignment.ImportCustomsClearanceStatusDescription);
			}
		}

		public void TestReleaseStatusCalculatedFromCustomsStatus_Export_IE()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var consignment = Factory.New<HVLVConsignment>();
				AssertEquals("precondition - HVC_ReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ReleaseStatus);
				AssertEquals("precondition - ReleaseStatusDesciption is None by default", HVLVReleaseStatus.NoneDescription, consignment.ReleaseStatusDescription);
				AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.ExportCustomsClearanceStatusDescription);

				consignment.HVC_ExportCustomsClearanceStatus = "INV";
				AssertEquals("HVC_ReleaseStatus should be HLD", HVLVReleaseStatus.Held, consignment.HVC_ReleaseStatus);
				AssertEquals("ReleaseStatusDesciption should be Held", HVLVReleaseStatus.HeldDescription, consignment.ReleaseStatusDescription);
				AssertEquals("CustomsStatusDescription", "Invalid - Invalid", consignment.ExportCustomsClearanceStatusDescription);
			}
		}

		public void TestExportReleaseStatusCalculatedFromCustomsStatus_Export_IE()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var consignment = Factory.New<HVLVConsignment>();
				AssertEquals("precondition - HVC_ExportReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ExportReleaseStatus);
				AssertEquals("precondition - ExportReleaseStatusDesciption is None by default", HVLVReleaseStatus.NoneDescription, consignment.ExportReleaseStatusDescription);
				AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.ExportCustomsClearanceStatusDescription);

				consignment.HVC_ExportCustomsClearanceStatus = "INV";
				AssertEquals("HVC_ExportReleaseStatus should be HLD", HVLVReleaseStatus.Held, consignment.HVC_ExportReleaseStatus);
				AssertEquals("ExportReleaseStatusDesciption should be Held", HVLVReleaseStatus.HeldDescription, consignment.ExportReleaseStatusDescription);
				AssertEquals("CustomsStatusDescription", "Invalid - Invalid", consignment.ExportCustomsClearanceStatusDescription);
			}
		}

		#endregion

		#region NewZealand

		public void TestReleaseStatusCalculatedFromCustomsStatusViaRefDb_Export_NZ()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "WOF", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.CustomsStatus, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var consignment = Factory.New<HVLVConsignment>();
				AssertEquals("precondition - HVC_ReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ReleaseStatus);
				AssertEquals("precondition - ReleaseStatusDescription is None by default", HVLVReleaseStatus.NoneDescription, consignment.ReleaseStatusDescription);
				AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.ExportCustomsClearanceStatusDescription);

				consignment.HVC_ExportCustomsClearanceStatus = "WOF";
				AssertEquals("HVC_ReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ReleaseStatus);
				AssertEquals("ReleaseStatusDescription should be Held", HVLVReleaseStatus.HeldDescription, consignment.ReleaseStatusDescription);
				AssertEquals("CustomsStatusDescription", "COVID-19 go away!", consignment.ExportCustomsClearanceStatusDescription);
			}
		}

		public void TestExportReleaseStatusCalculatedFromCustomsStatusViaRefDb_Export_NZ()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "WOF", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.CustomsStatus, CountryCodes.NewZealand);
				var consignment = Factory.New<HVLVConsignment>();
				AssertEquals("precondition - HVC_ExportReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ExportReleaseStatus);
				AssertEquals("precondition - ExportReleaseStatusDescription is None by default", HVLVReleaseStatus.NoneDescription, consignment.ExportReleaseStatusDescription);
				AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.ExportCustomsClearanceStatusDescription);

				consignment.HVC_ExportCustomsClearanceStatus = "WOF";
				AssertEquals("HVC_ExportReleaseStatus should be Held", HVLVReleaseStatus.Held, consignment.HVC_ExportReleaseStatus);
				AssertEquals("ExportReleaseStatusDescription should be Held", HVLVReleaseStatus.HeldDescription, consignment.ExportReleaseStatusDescription);
				AssertEquals("CustomsStatusDescription", "COVID-19 go away!", consignment.ExportCustomsClearanceStatusDescription);
			}
		}

		#endregion

		#region Australia

		public void TestReleaseStatusCalculatedFromCustomsStatusViaRefDb_Export_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "REJ", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
				var consignment = Factory.New<HVLVConsignment>();
				AssertEquals("precondition - HVC_ReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ReleaseStatus);
				AssertEquals("precondition - ReleaseStatusDesciption is None by default", HVLVReleaseStatus.NoneDescription, consignment.ReleaseStatusDescription);
				AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.ExportCustomsClearanceStatusDescription);

				consignment.HVC_ExportCustomsClearanceStatus = "REJ";
				AssertEquals("HVC_ReleaseStatus should be HLD", HVLVReleaseStatus.Held, consignment.HVC_ReleaseStatus);
				AssertEquals("ReleaseStatusDesciption should be Held", HVLVReleaseStatus.HeldDescription, consignment.ReleaseStatusDescription);
				AssertEquals("CustomsStatusDescription", "COVID-19 go away!", consignment.ExportCustomsClearanceStatusDescription);
			}
		}

		public void TestExportReleaseStatusCalculatedFromCustomsStatusViaRefDb_Export_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "REJ", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
				var consignment = Factory.New<HVLVConsignment>();
				AssertEquals("precondition - HVC_ExportReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ExportReleaseStatus);
				AssertEquals("precondition - ExportReleaseStatusDesciption is None by default", HVLVReleaseStatus.NoneDescription, consignment.ExportReleaseStatusDescription);
				AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.ExportCustomsClearanceStatusDescription);

				consignment.HVC_ExportCustomsClearanceStatus = "REJ";
				AssertEquals("HVC_ExportReleaseStatus should be HLD", HVLVReleaseStatus.Held, consignment.HVC_ExportReleaseStatus);
				AssertEquals("ExportReleaseStatusDesciption should be Held", HVLVReleaseStatus.HeldDescription, consignment.ExportReleaseStatusDescription);
				AssertEquals("CustomsStatusDescription", "COVID-19 go away!", consignment.ExportCustomsClearanceStatusDescription);
			}
		}

		#endregion

		public void TestImportReleaseStatusCalculatedFromCustomsStatusViaRefDb_Import()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!");
			var consignment = Factory.New<HVLVConsignment>();
			AssertEquals("precondition - HVC_ImportReleaseStatus is NON by default", HVLVReleaseStatus.None, consignment.HVC_ImportReleaseStatus);

			AssertEquals("precondition - ImportReleaseStatusDesciption is None by default", HVLVReleaseStatus.NoneDescription, consignment.ImportReleaseStatusDescription);
			AssertEquals("precondition - CustomsStatusDescription is empty by default", string.Empty, consignment.ImportCustomsClearanceStatusDescription);

			consignment.HVC_ImportCustomsClearanceStatus = "&&&";
			AssertEquals("HVC_ImportReleaseStatus should be HLD", HVLVReleaseStatus.Held, consignment.HVC_ImportReleaseStatus);

			AssertEquals("ImportReleaseStatusDesciption should be Held", HVLVReleaseStatus.HeldDescription, consignment.ImportReleaseStatusDescription);
			AssertEquals("CustomsStatusDescription", "COVID-19 go away!", consignment.ImportCustomsClearanceStatusDescription);
		}

		public void TestReleaseStatusCalculatedFromCustomsStatusViaRefDb_ReportErrorForInvalidCustomsStatus()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "XXX", "COVID-19 go away!", "HLD");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "YYY", "Another Code", "CLR");
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "ZZZ", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);

			var consignment = Factory.New<HVLVConsignment>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				consignment.HVC_ImportCustomsClearanceStatus = "###";
				AssertEquals("Detail info should be reported in Error Reporter",
					@"Current Customs Status Codes is [###]
Of type: [CSTA]
Searched code list: [XXX,YYY]
CustomsStatusStore CountryCode: [AU]
HVLVConsignment Shipper Country: []
HVLVConsignment Consignee Country: []
Shipment Origin Country: []
Shipment Destination Country: []
User Login Country: [AU]
Export Declaration PK: [00000000-0000-0000-0000-000000000000]
Export Declaration Is Cancelled: []
Export Declaration Message Sub Type: []
Import Declaration PK: [00000000-0000-0000-0000-000000000000]
Import Declaration Is Cancelled: []
Import Declaration Message Sub Type: []",
					ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();

				consignment.HVC_ExportCustomsClearanceStatus = "###";

				AssertEquals("Detail info should be reported in Error Reporter",
						@"Current Customs Status Codes is [###]
Of type: [CSTEX]
Searched code list: [ZZZ]
CustomsStatusStore CountryCode: [AU]
HVLVConsignment Shipper Country: []
HVLVConsignment Consignee Country: []
Shipment Origin Country: []
Shipment Destination Country: []
User Login Country: [AU]
Export Declaration PK: [00000000-0000-0000-0000-000000000000]
Export Declaration Is Cancelled: []
Export Declaration Message Sub Type: []
Import Declaration PK: [00000000-0000-0000-0000-000000000000]
Import Declaration Is Cancelled: []
Import Declaration Message Sub Type: []",
						ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		public void TestHVLVCustomsStatusCalculation_SwitchingCountry_NoExceptions()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "AUC", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.CustomsStatus, CountryCodes.Australia);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "USC", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.CustomsStatus, CountryCodes.UnitedStates);
			var consignment = Factory.New<HVLVConsignment>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				consignment.HVC_ImportCustomsClearanceStatus = "USC";
				Assert(!ErrorReporter.LastMessageReported.Any());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				consignment.HVC_ImportCustomsClearanceStatus = "AUC";
				Assert(!ErrorReporter.LastMessageReported.Any());
			}
		}

		#endregion

		#region TestDirectionOfTrade

		public void TestDirectionOfTrade_ImportConsignment()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = "US";
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				AssertEquals(Directions.Import, consignment.DirectionOfTrade);
			}
		}

		public void TestDirectionOfTrade_ExportConsignment()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = "US";
				AssertEquals(Directions.Export, consignment.DirectionOfTrade);
			}
		}

		public void TestDirectionOfTrade_UnknownConsignment()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertEquals(Directions.Unknown, consignment.DirectionOfTrade);
			}
		}

		public void TestDirectionOfTrade_ImportShipment()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var manifestedOnShipment = Factory.New<ForwardingShipment>();
				manifestedOnShipment.JS_RL_NKOrigin = "CNSHA";
				manifestedOnShipment.JS_RL_NKDestination = "AUSYD";

				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = manifestedOnShipment.PK;
				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertEquals(Directions.Import, consignment.DirectionOfTrade);
			}
		}

		public void TestDirectionOfTrade_ExportShipment()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var manifestedOnShipment = Factory.New<ForwardingShipment>();
				manifestedOnShipment.JS_RL_NKOrigin = "AUSYD";
				manifestedOnShipment.JS_RL_NKDestination = "CNSHA";

				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = manifestedOnShipment.PK;
				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertEquals(Directions.Export, consignment.DirectionOfTrade);
			}
		}

		public void TestDirectionOfTrade_UnknownShipment()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var manifestedOnShipment = Factory.New<ForwardingShipment>();
				manifestedOnShipment.JS_RL_NKOrigin = string.Empty;
				manifestedOnShipment.JS_RL_NKDestination = string.Empty;

				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = manifestedOnShipment.PK;
				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertEquals(Directions.Unknown, consignment.DirectionOfTrade);
			}
		}

		public void TestDirectionOfTrade_ImportDestinationDepot()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var billToParty = Factory.New<OrgAddress>();
				var destinationDepot = Factory.New<OrgAddress>();
				billToParty.OA_RN_NKCountryCode = "NZ";
				destinationDepot.OA_RN_NKCountryCode = "AU";

				var bookingHeader = Factory.New<HVLVBookingHeader>();
				bookingHeader.HVH_OA_BillToParty = billToParty.PK;

				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
				consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertEquals(Directions.Import, consignment.DirectionOfTrade);
			}
		}

		public void TestDirectionOfTrade_ExportDestinationDepot()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var billToParty = Factory.New<OrgAddress>();
				var destinationDepot = Factory.New<OrgAddress>();
				billToParty.OA_RN_NKCountryCode = "AU";
				destinationDepot.OA_RN_NKCountryCode = "NZ";

				var bookingHeader = Factory.New<HVLVBookingHeader>();
				bookingHeader.HVH_OA_BillToParty = billToParty.PK;

				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
				consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertEquals(Directions.Export, consignment.DirectionOfTrade);
			}
		}

		public void TestDirectionOfTrade_UnknownDestinationDepot()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var billToParty = Factory.New<OrgAddress>();
				var destinationDepot = Factory.New<OrgAddress>();
				billToParty.OA_RN_NKCountryCode = string.Empty;
				destinationDepot.OA_RN_NKCountryCode = string.Empty;

				var bookingHeader = Factory.New<HVLVBookingHeader>();
				bookingHeader.HVH_OA_BillToParty = billToParty.PK;

				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
				consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
				consignment.HVC_RN_NKShipperCountryCode = string.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = string.Empty;
				AssertEquals(Directions.Unknown, consignment.DirectionOfTrade);
			}
		}

		#endregion

		public void TestConsignmentIdStaysIfNotNewWhenSaveFails()
		{
			var newConsignment = CreateConsignment("W001", "S001", new ItemReferences("I001", "S001001", "W001001"));
			Factory.Save();
			AssertNotNullOrEmpty("Consignment Id should not be empty", newConsignment.HVC_ConsignmentId);
			newConsignment.HVC_Status = "AB";
			try
			{
				Factory.Save();
			}
			catch (ZSaveException)
			{ }

			Assert("Save should fail", newConsignment.HasChanges);
			AssertNotNullOrEmpty("Consignment Id should not be cleared", newConsignment.HVC_ConsignmentId);
		}

		public void TestLocalTransportCompanyLabel()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HVH_BookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>().PK;
			var registryValue = new LocalTransportCompanyBrandingCollection();
			using (DocumentsDataRegistry.Instance.LocalTransportCompanyBrand.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				AssertEquals(ZString.Empty, consignment.LocalTransportCompanyLabel);
			}

			var lmc = Factory.NewWithValidTestData<OrgHeader>();
			lmc.OH_IsShippingProvider = true;
			lmc.OH_IsLocalTransport = true;
			var lmcPK = lmc.PK;
			var branding = registryValue.AddNew();
			branding.LabelName = LabelNames.EParcelLabel;
			branding.LocalTransportCompanyPK = lmcPK;
			branding.Code = lmc.OH_Code;

			Factory.Save();

			consignment.HVC_OH_LastMileCarrier = lmcPK;
			using (DocumentsDataRegistry.Instance.LocalTransportCompanyBrand.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				AssertEquals(LabelNames.EParcelLabel, consignment.LocalTransportCompanyLabel);
			}
		}

		public void TestIEDocsProvider()
		{
			var consignment = CreateConsignment("ABC123", "DHL");
			Assert(consignment is IEDocsProvider);
			var docManagerSupport = (IDocManagerSupport)consignment;
			AssertNotNull(docManagerSupport.DocManagerInfo);
			AssertEquals("HLC", docManagerSupport.DocManagerInfo.DocManagerCode);
			AssertNotNull(consignment.GetEDocsProviderSupporter());
		}

		public void TestIsSurplusAtDestination()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			item1.HVI_IsUnmanifestedAtDestination = true;

			AssertEquals("Is surplus as all items are unmanifested at destination", true, consignment.IsSurplusAtDestination);

			var item2 = consignment.Items.AddNew();
			item2.HVI_IsUnmanifestedAtDestination = false;

			AssertEquals("No longer surplus as contains an item not unmanifested", false, consignment.IsSurplusAtDestination);

			item2.HVI_IsUnmanifestedAtDestination = true;

			AssertEquals("Is surplus at destination again", true, consignment.IsSurplusAtDestination);
		}

		public void TestIsSurplusAtDestination_ExcludesInactiveItems()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item1.HVI_IsUnmanifestedAtDestination = true;
			item2.HVI_IsUnmanifestedAtDestination = false;

			item2.HVI_IsActive = false;
			AssertEquals("Inactive items are excluded when determining if surplus", true, consignment.IsSurplusAtDestination);

			item2.HVI_IsActive = true;
			AssertEquals("Reactivated items are included when determining if surplus", false, consignment.IsSurplusAtDestination);
		}

		public void TestIsSurplusAtDestination_WithNoActiveItems_IsFalse()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();

			var item1a = consignment1.Items.AddNew();
			var item1b = consignment1.Items.AddNew();
			var item2a = consignment2.Items.AddNew();
			var item2b = consignment2.Items.AddNew();

			item1a.HVI_IsUnmanifestedAtDestination = true;
			item1b.HVI_IsUnmanifestedAtDestination = true;
			item2a.HVI_IsUnmanifestedAtDestination = false;
			item2b.HVI_IsUnmanifestedAtDestination = false;

			consignment1.HVC_IsActive = false;
			item2a.HVI_IsActive = false;
			item2b.HVI_IsActive = false;

			CombineAssertions("Should not be surplus if all items are inactive", () =>
			{
				AssertEquals("Inactive Consignment with Inactive Items", false, consignment1.IsSurplusAtDestination);
				AssertEquals("Active Consignment with Inactive Items", false, consignment2.IsSurplusAtDestination);
			});
		}

		public void TestHVLVConsignmentActiveStatusIsLogged_WhenChanged()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			Assert("precondition", consignment.HVC_IsActive);
			Factory.Save();

			consignment.HVC_IsActive = false;
			Factory.Save();

			var status = consignment.Logs.MostRecentLog;

			AssertEquals("status should be INA", AutoEvents.SetToInactive, status.Event);

			consignment.HVC_IsActive = true;
			Factory.Save();
			status = consignment.Logs.MostRecentLog;

			AssertEquals("status should be ACT", AutoEvents.SetToActive, status.Event);
		}

		public void TestHVLVConsignmentActiveStatusIsReadOnlyWhenNewRecordNotSaved()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			AssertEquals("pre condition", false, consignment.IsInDatabase);
			AssertEquals("When new consignment is not saved, HVC_IsActive should be readonly", true, consignment.HVC_IsActiveInfo.ReadOnly);

			Factory.Save();

			AssertEquals(true, consignment.IsInDatabase);
			AssertEquals("When new consignment is saved, HVC_IsActive should no longer be readonly", false, consignment.HVC_IsActiveInfo.ReadOnly);
		}

		public void TestShipperFieldsDefaultedOnPreSaveValidation()
		{
			HVLVTestHelper.SetGS1FountainOnOrgProxy(Factory, "1234567");

			var shipment1 = Factory.New<ForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgAddress>();
			consignor.Header.OH_FullName = "ABC Company";
			consignor.OA_Address1 = "88 Lucky Lane";
			consignor.OA_Address2 = "Unit 8";
			consignor.OA_City = "Dragonville";
			consignor.OA_State = "Ping Pong";
			consignor.OA_PostCode = "8888";
			consignor.OA_RN_NKCountryCode = "CN";
			consignor.OA_Email = "email@address.com";
			consignor.OA_Phone = "12345";
			consignor.OA_Mobile = "9999 9999";
			consignor.OA_Fax = "54321";

			var consignorContact = consignor.Header.Contacts.AddNew();
			consignorContact.OC_ContactName = "Barry";
			consignorContact.OC_Mobile = "8888 8888";

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment1.PK;

			Action<string, bool> assertShipperFieldsPopulated = (message, expectedToBePopulated) =>
			{
				AssertEquals(message, expectedToBePopulated ? "ABC Company" : "", consignment.HVC_ShipperName);
				AssertEquals(message, expectedToBePopulated ? "88 Lucky Lane" : "", consignment.HVC_ShipperAddress1);
				AssertEquals(message, expectedToBePopulated ? "Unit 8" : "", consignment.HVC_ShipperAddress2);
				AssertEquals(message, expectedToBePopulated ? "Dragonville" : "", consignment.HVC_ShipperCity);
				AssertEquals(message, expectedToBePopulated ? "Ping Pong" : "", consignment.HVC_ShipperState);
				AssertEquals(message, expectedToBePopulated ? "8888" : "", consignment.HVC_ShipperPostcode);
				AssertEquals(message, expectedToBePopulated ? "CN" : "", consignment.HVC_RN_NKShipperCountryCode);
				AssertEquals(message, expectedToBePopulated ? "email@address.com" : "", consignment.HVC_ShipperEmail);
				AssertEquals(message, expectedToBePopulated ? "12345" : "", consignment.HVC_ShipperPhone);
				AssertEquals(message, expectedToBePopulated ? "54321" : "", consignment.HVC_ShipperFax);
				AssertEquals(message, expectedToBePopulated ? "Barry" : "", consignment.HVC_ShipperContact);
				AssertEquals(message, expectedToBePopulated ? "8888 8888" : "", consignment.HVC_ShipperMobile);
			};

			Action emptyShipperFields = () =>
			{
				consignment.HVC_ShipperName = ZString.Empty;
				consignment.HVC_ShipperAddress1 = ZString.Empty;
				consignment.HVC_ShipperAddress2 = ZString.Empty;
				consignment.HVC_ShipperCity = ZString.Empty;
				consignment.HVC_ShipperState = ZString.Empty;
				consignment.HVC_ShipperPostcode = ZString.Empty;
				consignment.HVC_RN_NKShipperCountryCode = ZString.Empty;
				consignment.HVC_ShipperEmail = ZString.Empty;
				consignment.HVC_ShipperPhone = ZString.Empty;
				consignment.HVC_ShipperFax = ZString.Empty;
				consignment.HVC_ShipperContact = ZString.Empty;
				consignment.HVC_ShipperMobile = ZString.Empty;
			};

			consignment.RunPreSaveValidation();
			assertShipperFieldsPopulated("No Consignor on attached Shipment", false);

			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.PK;
			shipment1.ConsignorDocumentaryAddress.ContactPK = consignorContact.PK;
			consignment.RunPreSaveValidation();
			assertShipperFieldsPopulated("Consignor on attached Shipment", true);
			emptyShipperFields();

			consignment.HVC_ShipperEmail = "fred@fredscookingutensils.com";
			consignment.RunPreSaveValidation();
			consignment.HVC_ShipperEmail = ZString.Empty;
			assertShipperFieldsPopulated("A Shipper Field is set on the consignment, Shipper Fields aren't eligible for defaulting", false);

			consignment.HVC_ShipperEmail = ZString.Empty;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor.PK;
			shipment2.ConsignorDocumentaryAddress.ContactPK = consignorContact.PK;
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment2.PK;
			consignment.RunPreSaveValidation();
			assertShipperFieldsPopulated("Multiple shipments attached, but they all have the same consignor, Shipper Fields can be defaulted", true);
			emptyShipperFields();

			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipment2.ConsignorDocumentaryAddress.ContactPK = ZGuid.Empty;
			consignment.RunPreSaveValidation();
			assertShipperFieldsPopulated("Multiple shipments attached, but with different consignors, can't default Shipper Fields", false);

			Factory.Save();

			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor.PK;
			shipment2.ConsignorDocumentaryAddress.ContactPK = consignorContact.PK;
			assertShipperFieldsPopulated("Consignment is in the database, Shipper Fields are not eligible for defaulting", false);
		}

		public void TestConsigneeAddress()
		{
			var consignment = Factory.New<HVLVConsignment>();
			AssertNull("pre condition", consignment.ConsigneeAddress);
			AssertEquals(false, consignment.ConsigneeIsOrganisation);

			consignment.HVC_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			AssertNotNull("Should find address", consignment.ConsigneeAddress);
			AssertEquals("Should have same PK", consignment.HVC_OA_ConsigneeAddress, consignment.ConsigneeAddress.PK);
			AssertEquals(true, consignment.ConsigneeIsOrganisation);
		}

		public void TestConsigneeAddressFields_WhenConsigneeIsOrganisation_ReturnAddressValues()
		{
			var consignment = Factory.New<HVLVConsignment>();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.Addresses.AddNew();
			address.OA_CompanyNameOverride = "Test Company Name";
			address.OA_Address1 = "Test Address 1";
			address.OA_Address2 = "Test Address 2";
			address.OA_City = "Test City";
			address.OA_PostCode = "012345678";
			address.OA_State = "NSW";
			address.OA_RN_NKCountryCode = "AU";

			consignment.HVC_OA_ConsigneeAddress = address.PK;

			CombineAssertions("Should return value from address", () =>
			{
				AssertEquals("Consignee name", address.OA_CompanyNameOverride, consignment.HVC_ConsigneeName);
				AssertEquals(true, consignment.HVC_ConsigneeNameInfo.ReadOnly);
				AssertEquals("Consignee address 1", address.OA_Address1, consignment.HVC_ConsigneeAddress1);
				AssertEquals(true, consignment.HVC_ConsigneeAddress1Info.ReadOnly);
				AssertEquals("Consignee address 2", address.OA_Address2, consignment.HVC_ConsigneeAddress2);
				AssertEquals(true, consignment.HVC_ConsigneeAddress2Info.ReadOnly);
				AssertEquals("Consignee city", address.OA_City, consignment.HVC_ConsigneeCity);
				AssertEquals(true, consignment.HVC_ConsigneeCityInfo.ReadOnly);
				AssertEquals("Consignee postcode", address.OA_PostCode, consignment.HVC_ConsigneePostcode);
				AssertEquals(true, consignment.HVC_ConsigneePostcodeInfo.ReadOnly);
				AssertEquals("Consignee state", address.OA_State, consignment.HVC_ConsigneeState);
				AssertEquals(true, consignment.HVC_ConsigneeStateInfo.ReadOnly);
				AssertEquals("Consignee country code", address.OA_RN_NKCountryCode, consignment.HVC_RN_NKConsigneeCountryCode);
				AssertEquals(true, consignment.HVC_RN_NKConsigneeCountryCodeInfo.ReadOnly);
			});
		}

		public void TestConsigneeContactFields_WhenHasContactFromOrgHeader_ReturnContactValues()
		{
			var consignment = Factory.New<HVLVConsignment>();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.Addresses.AddNew();
			var contact = organisation.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_ContactName = "Test Contact";
			contact.OC_Phone = "012345678";
			contact.OC_Mobile = "012345677";
			contact.OC_Fax = "012345676";
			contact.OC_Email = "Test Email";

			consignment.HVC_OA_ConsigneeAddress = address.PK;
			consignment.HVC_ConsigneeContact = contact.OC_ContactName;

			CombineAssertions("Should return value from contact", () =>
			{
				AssertEquals("Consignee phone", contact.OC_Phone, consignment.HVC_ConsigneePhone);
				AssertEquals(false, consignment.HVC_ConsigneePhoneInfo.ReadOnly);
				AssertEquals("Consignee mobile", contact.OC_Mobile, consignment.HVC_ConsigneeMobile);
				AssertEquals(false, consignment.HVC_ConsigneeMobileInfo.ReadOnly);
				AssertEquals("Consignee fax", contact.OC_Fax, consignment.HVC_ConsigneeFax);
				AssertEquals(false, consignment.HVC_ConsigneeFaxInfo.ReadOnly);
				AssertEquals("Consignee email", contact.OC_Email, consignment.HVC_ConsigneeEmail);
				AssertEquals(false, consignment.HVC_ConsigneeEmailInfo.ReadOnly);
			});
		}

		public void TestShipperAddress()
		{
			var consignment = Factory.New<HVLVConsignment>();
			AssertNull("pre condition", consignment.ShipperAddress);
			AssertEquals(false, consignment.ShipperIsOrganisation);

			consignment.HVC_OA_ShipperAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			AssertNotNull("Should find address", consignment.ShipperAddress);
			AssertEquals("Should have same PK", consignment.HVC_OA_ShipperAddress, consignment.ShipperAddress.PK);
			AssertEquals(true, consignment.ShipperIsOrganisation);
		}

		public void TestShipperAddressFields_WhenShipperIsOrganisation_ReturnAddressValues()
		{
			var consignment = Factory.New<HVLVConsignment>();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.Addresses.AddNew();
			address.OA_CompanyNameOverride = "Test Company Name";
			address.OA_Address1 = "Test Address 1";
			address.OA_Address2 = "Test Address 2";
			address.OA_City = "Test City";
			address.OA_PostCode = "012345678";
			address.OA_State = "NSW";
			address.OA_RN_NKCountryCode = "AU";

			consignment.HVC_OA_ShipperAddress = address.PK;

			CombineAssertions("Should return value from address", () =>
			{
				AssertEquals("Shipper name", address.OA_CompanyNameOverride, consignment.HVC_ShipperName);
				AssertEquals(true, consignment.HVC_ShipperNameInfo.ReadOnly);
				AssertEquals("Shipper address 1", address.OA_Address1, consignment.HVC_ShipperAddress1);
				AssertEquals(true, consignment.HVC_ShipperAddress1Info.ReadOnly);
				AssertEquals("Shipper address 2", address.OA_Address2, consignment.HVC_ShipperAddress2);
				AssertEquals(true, consignment.HVC_ShipperAddress2Info.ReadOnly);
				AssertEquals("Shipper city", address.OA_City, consignment.HVC_ShipperCity);
				AssertEquals(true, consignment.HVC_ShipperCityInfo.ReadOnly);
				AssertEquals("Shipper postcode", address.OA_PostCode, consignment.HVC_ShipperPostcode);
				AssertEquals(true, consignment.HVC_ShipperPostcodeInfo.ReadOnly);
				AssertEquals("Shipper state", address.OA_State, consignment.HVC_ShipperState);
				AssertEquals(true, consignment.HVC_ShipperStateInfo.ReadOnly);
				AssertEquals("Shipper country code", address.OA_RN_NKCountryCode, consignment.HVC_RN_NKShipperCountryCode);
				AssertEquals(true, consignment.HVC_RN_NKShipperCountryCodeInfo.ReadOnly);
			});
		}

		public void TestShipperContactFields_WhenHasContactFromOrgHeader_ReturnContactValues()
		{
			var consignment = Factory.New<HVLVConsignment>();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.Addresses.AddNew();
			var contact = organisation.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_ContactName = "Test Contact";
			contact.OC_Phone = "012345678";
			contact.OC_Mobile = "012345677";
			contact.OC_Fax = "012345676";
			contact.OC_Email = "Test Email";

			consignment.HVC_OA_ShipperAddress = address.PK;
			consignment.HVC_ShipperContact = contact.OC_ContactName;

			CombineAssertions("Should return value from contact", () =>
			{
				AssertEquals("Shipper phone", contact.OC_Phone, consignment.HVC_ShipperPhone);
				AssertEquals(false, consignment.HVC_ShipperPhoneInfo.ReadOnly);
				AssertEquals("Shipper mobile", contact.OC_Mobile, consignment.HVC_ShipperMobile);
				AssertEquals(false, consignment.HVC_ShipperMobileInfo.ReadOnly);
				AssertEquals("Shipper fax", contact.OC_Fax, consignment.HVC_ShipperFax);
				AssertEquals(false, consignment.HVC_ShipperFaxInfo.ReadOnly);
				AssertEquals("Shipper email", contact.OC_Email, consignment.HVC_ShipperEmail);
				AssertEquals(false, consignment.HVC_ShipperEmailInfo.ReadOnly);
			});
		}

		public void TestReturnAddressFields_WhenReturnAddressIsOrgAddress()
		{
			var consignment = Factory.New<HVLVConsignment>();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.Addresses.AddNew();
			address.OA_CompanyNameOverride = "Test Company Name";
			address.OA_Address1 = "Test Address 1";
			address.OA_Address2 = "Test Address 2";
			address.OA_City = "Test City";
			address.OA_PostCode = "012345678";
			address.OA_State = "NSW";
			address.OA_RN_NKCountryCode = "AU";

			consignment.HVC_OA_ReturnLocation = address.PK;

			CombineAssertions("Should return value from address", () =>
			{
				AssertEquals("Return name", address.OA_CompanyNameOverride, consignment.HVC_ReturnName);
				AssertEquals(true, consignment.HVC_ReturnNameInfo.ReadOnly);
				AssertEquals("Return address 1", address.OA_Address1, consignment.HVC_ReturnAddress1);
				AssertEquals(true, consignment.HVC_ReturnAddress1Info.ReadOnly);
				AssertEquals("Return address 2", address.OA_Address2, consignment.HVC_ReturnAddress2);
				AssertEquals(true, consignment.HVC_ReturnAddress2Info.ReadOnly);
				AssertEquals("Return city", address.OA_City, consignment.HVC_ReturnCity);
				AssertEquals(true, consignment.HVC_ReturnCityInfo.ReadOnly);
				AssertEquals("Return postcode", address.OA_PostCode, consignment.HVC_ReturnPostcode);
				AssertEquals(true, consignment.HVC_ReturnPostcodeInfo.ReadOnly);
				AssertEquals("Return state", address.OA_State, consignment.HVC_ReturnState);
				AssertEquals(true, consignment.HVC_ReturnStateInfo.ReadOnly);
				AssertEquals("Return country code", address.OA_RN_NKCountryCode, consignment.HVC_RN_NKReturnCountryCode);
				AssertEquals(true, consignment.HVC_RN_NKReturnCountryCodeInfo.ReadOnly);
			});
		}

		public void TestClusterKeyIsCascadedFromBookingHeader()
		{
			var header = Factory.New<HVLVBookingHeader>();
			header.HVH_BookingReference = "TestHeaderRef";
			header.HVH_ClusterKey = 9999;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_HVH_BookingHeader = header.PK;

			AssertEquals(header.HVH_BookingReference, consignment.BookingHeader.HVH_BookingReference);
			AssertEquals(header.HVH_ClusterKey, consignment.HVC_ClusterKey);
		}

		public void TestClusterKeyIsCascadedToItems()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			consignment.HVC_ClusterKey = 4758;
			AssertEquals(consignment.HVC_ClusterKey, item1.HVI_ClusterKey);
			AssertEquals(consignment.HVC_ClusterKey, item2.HVI_ClusterKey);
		}

		public void TestHumanReadableName()
		{
			var consignment = Factory.New<HVLVConsignment>();
			AssertEquals("HVLV Consignment", consignment.HumanReadableName);

			consignment.HVC_ConsignmentId = "CONSIGN1234";
			AssertEquals("HVLV Consignment CONSIGN1234", consignment.HumanReadableName);
		}

		public void TestReleaseStatusDescription()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ReleaseStatus = "NON";
			AssertEquals("None/Not Available", consignment.ReleaseStatusDescription);

			consignment.HVC_ReleaseStatus = "HLD";
			AssertEquals("Held", consignment.ReleaseStatusDescription);

			consignment.HVC_ReleaseStatus = "CLR";
			AssertEquals("Cleared", consignment.ReleaseStatusDescription);

			consignment.HVC_ReleaseStatus = "XYZ";
			AssertEquals("", consignment.ReleaseStatusDescription);
		}

		public void TestExportReleaseStatusDescription()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ExportReleaseStatus = "NON";
			AssertEquals("None/Not Available", consignment.ExportReleaseStatusDescription);

			consignment.HVC_ExportReleaseStatus = "HLD";
			AssertEquals("Held", consignment.ExportReleaseStatusDescription);

			consignment.HVC_ExportReleaseStatus = "CLR";
			AssertEquals("Cleared", consignment.ExportReleaseStatusDescription);

			consignment.HVC_ExportReleaseStatus = "XYZ";
			AssertEquals("", consignment.ExportReleaseStatusDescription);
		}

		public void TestDeliveryCartageZone_ReturnsExpectedZoneName()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "2200";
			postCode.RK_RN_NKCountry = "AU";

			var cityTown = LoadRefCityTown("SYDNEY", "NSW", "AU");

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			lastMileCarrier.OH_IsLocalTransport = true;

			var zoneRateTransportProvider = CreateZoneRateProvider(RatingConstants.RatingZoneTypes.All, "AU", lastMileCarrier, cityTown);
			var zone = CreateZone("ZONE NAME", zoneRateTransportProvider);
			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_ToPostCode = postCode.RK_CityTownPostCode;
			zoneItem.TQ_FromPostCode = postCode.RK_CityTownPostCode;
			zoneItem.TQ_R9_CityTown = cityTown.PK;
			zoneItem.TQ_RN_NKCountry = "AU";

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			consignment.HVC_ConsigneeCity = cityTown.R9_InternationalName;
			consignment.HVC_ConsigneePostcode = postCode.RK_CityTownPostCode;
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;

			Factory.Save();

			var location = LocationHelper.GetLocationFromString(shipment.JS_RL_NKDestination, Factory);

			var rateTransportZoneHelper = ObjectFactory.Get<IRateTransportZoneHelper>();
			var expectedZoneName = rateTransportZoneHelper.GetZoneName(Factory, consignment.LastMileCarrier, location, consignment.HVC_RN_NKConsigneeCountryCode, consignment.HVC_ConsigneePostcode, consignment.HVC_ConsigneeCity);

			AssertEquals("Precondition: RateTransportZoneHelper should have found our test zone", "ZONE NAME", expectedZoneName);
			AssertEquals("Delivery Cartage Zone should be set using RateTransportZoneHelper", expectedZoneName, consignment.HVC_Calc_DeliveryCartageZone);
		}

		public void TestDeliveryCartageZone_WhenNotAttachedToShipment_DeterminesZoneFromConsigneeCountryCode()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "2200";
			postCode.RK_RN_NKCountry = "AU";

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			lastMileCarrier.OH_IsLocalTransport = true;

			var zoneRateTransportProvider = CreateZoneRateProvider(RatingConstants.RatingZoneTypes.All, "AU", lastMileCarrier);
			var zone = CreateZone("ZONE NAME", zoneRateTransportProvider);
			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_ToPostCode = postCode.RK_CityTownPostCode;
			zoneItem.TQ_FromPostCode = postCode.RK_CityTownPostCode;
			zoneItem.TQ_RN_NKCountry = "AU";

			var consignmentHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			consignment.HVC_ConsigneePostcode = postCode.RK_CityTownPostCode;
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;

			Factory.Save();

			var location = LocationHelper.GetLocationFromString(consignment.HVC_RN_NKConsigneeCountryCode, Factory);

			var rateTransportZoneHelper = ObjectFactory.Get<IRateTransportZoneHelper>();
			var expectedZoneName = rateTransportZoneHelper.GetZoneName(Factory, consignment.LastMileCarrier, location, consignment.HVC_RN_NKConsigneeCountryCode, consignment.HVC_ConsigneePostcode, consignment.HVC_ConsigneeCity);

			AssertEquals("Precondition: RateTransportZoneHelper should have found our test zone", "ZONE NAME", expectedZoneName);
			AssertEquals("Delivery Cartage Zone should be set from RateTransportZoneHelper", expectedZoneName, consignment.HVC_Calc_DeliveryCartageZone);
		}

		public void TestDeliveryCartageZone_WhenConsignmentHasNoBookingHeader_UsesConsigneeCountryCodeForLocation()
		{
			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			lastMileCarrier.OH_IsLocalTransport = true;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			consignment.HVC_ConsigneePostcode = "2015";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;

			Factory.Save();

			consignment.HVC_HVH_BookingHeader = ZGuid.NewZGuid();

			var expectedLocation = LocationHelper.GetLocationFromString(consignment.HVC_RN_NKConsigneeCountryCode, Factory);

			var mockTransportZoneHelper = new Mock<IRateTransportZoneHelper>();
			mockTransportZoneHelper.Setup(mock => mock.GetZoneName(It.IsAny<BusinessObjectFactory>(), It.IsAny<IOrgHeader>(), It.IsAny<ILocation>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback((BusinessObjectFactory factory, IOrgHeader lmc, object location, string countryCode, string postCode, string citySuburb) => AssertEquals(expectedLocation.Code, ((ILocation)location).Code))
				.Returns((BusinessObjectFactory factory, IOrgHeader lmc, object location, string countryCode, string postCode, string citySuburb) => $"ZONE FOR {((ILocation)location).Code}");

			using (ObjectFactory.Substitute(mockTransportZoneHelper.Object))
			{
				AssertEquals("Delivery Cartage Zone should be set from RateTransportZoneHelper", "ZONE FOR AU", consignment.HVC_Calc_DeliveryCartageZone);
			}
		}

		public void TestDeliveryCartageZone_WhenNoMatchingZones_ReturnBlank()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();

			Factory.Save();

			var location = LocationHelper.GetLocationFromString(shipment.JS_RL_NKDestination, Factory);

			var rateTransportZoneHelper = ObjectFactory.Get<IRateTransportZoneHelper>();
			var expectedZoneName = rateTransportZoneHelper.GetZoneName(Factory, consignment.LastMileCarrier, location, consignment.HVC_RN_NKConsigneeCountryCode, consignment.HVC_ConsigneePostcode, consignment.HVC_ConsigneeCity);
			AssertEquals("Precondition: RateTransportZoneHelper should not find any zones", string.Empty, expectedZoneName);
			AssertEquals("Delivery Cartage Zone should be empty", expectedZoneName, consignment.HVC_Calc_DeliveryCartageZone);
		}

		public void TestDeliveryCartageZone_IsCached()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "2000";
			postCode.RK_RN_NKCountry = "AU";

			var cityTown = LoadRefCityTown("SYDNEY", "NSW", "AU");

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			lastMileCarrier.OH_IsLocalTransport = true;

			var zoneRateTransportProvider = CreateZoneRateProvider(RatingConstants.RatingZoneTypes.All, "AU", lastMileCarrier, cityTown);
			var zone = CreateZone("ZONE NAME", zoneRateTransportProvider);
			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_ToPostCode = postCode.RK_CityTownPostCode;
			zoneItem.TQ_FromPostCode = postCode.RK_CityTownPostCode;
			zoneItem.TQ_R9_CityTown = cityTown.PK;
			zoneItem.TQ_RN_NKCountry = "AU";

			var rateTransportZoneHelper = ObjectFactory.Get<IRateTransportZoneHelper>();

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			consignment.HVC_ConsigneeCity = cityTown.R9_InternationalName;
			consignment.HVC_ConsigneePostcode = postCode.RK_CityTownPostCode;
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;

			Factory.Save();

			var location = LocationHelper.GetLocationFromString(shipment.JS_RL_NKDestination, Factory);

			var key = string.Join("-",
				consignment.LastMileCarrier?.OH_Code.ToString() ?? string.Empty,
				location.Code.ToString(),
				consignment.HVC_RN_NKConsigneeCountryCode,
				consignment.HVC_ConsigneePostcode,
				consignment.HVC_ConsigneeCity);

			var initialEmptyCacheValue = consignment.Factory.GetCachedValue(key, () => "KEY NOT FOUND");
			AssertEquals("Precondition: No value should be stored in the cache", "KEY NOT FOUND", initialEmptyCacheValue);

			consignment.Factory.ClearCachedValue<string>(key);

			var expectedZoneName = rateTransportZoneHelper.GetZoneName(Factory, consignment.LastMileCarrier, location, consignment.HVC_RN_NKConsigneeCountryCode, consignment.HVC_ConsigneePostcode, consignment.HVC_ConsigneeCity);
			var calculatedCartageZone = consignment.HVC_Calc_DeliveryCartageZone;
			AssertEquals("Precondition: RateTransportZoneHelper should have found our test zone", "ZONE NAME", expectedZoneName);
			AssertEquals("Precondition: Delivery Cartage Zone should be set using RateTransportZoneHelper", expectedZoneName, calculatedCartageZone);

			var zoneNameFromCache = consignment.Factory.GetCachedValue(key, () => "KEY NOT FOUND");
			AssertEquals("Calculated Delivery Cartage Zone should have been stored in the cache", expectedZoneName, zoneNameFromCache);
		}

		public void TestDeliveryCartageZone_WhenCacheReturnsEmptyStringValue_CacheValueCanBeOverwritten()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "2000";
			postCode.RK_RN_NKCountry = "AU";

			var cityTown = LoadRefCityTown("SYDNEY", "NSW", "AU");

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			lastMileCarrier.OH_IsLocalTransport = true;

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			consignment.HVC_ConsigneeCity = cityTown.R9_InternationalName;
			consignment.HVC_ConsigneePostcode = postCode.RK_CityTownPostCode;
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;

			Factory.Save();

			var location = LocationHelper.GetLocationFromString(shipment.JS_RL_NKDestination, Factory);

			var rateTransportZoneHelper = ObjectFactory.Get<IRateTransportZoneHelper>();
			var zoneName1 = rateTransportZoneHelper.GetZoneName(Factory, consignment.LastMileCarrier, location, consignment.HVC_RN_NKConsigneeCountryCode, consignment.HVC_ConsigneePostcode, consignment.HVC_ConsigneeCity);
			AssertEquals("Precondition: RateTransportZoneHelper should not find any zones", string.Empty, zoneName1);

			var zoneRateTransportProvider = CreateZoneRateProvider(RatingConstants.RatingZoneTypes.All, "AU", lastMileCarrier, cityTown);
			var zone = CreateZone("ZONE NAME", zoneRateTransportProvider);
			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_ToPostCode = postCode.RK_CityTownPostCode;
			zoneItem.TQ_FromPostCode = postCode.RK_CityTownPostCode;
			zoneItem.TQ_R9_CityTown = cityTown.PK;
			zoneItem.TQ_RN_NKCountry = "AU";

			Factory.Save();

			var deliveryCartageZoneAfterCreatingZone = consignment.HVC_Calc_DeliveryCartageZone;
			AssertEquals("Precondition: DeliveryCartageZone should be set to new zone value", "ZONE NAME", deliveryCartageZoneAfterCreatingZone);

			var key = string.Join("-",
				consignment.LastMileCarrier?.OH_Code.ToString() ?? string.Empty,
				location.Code.ToString() ?? string.Empty,
				consignment.HVC_RN_NKConsigneeCountryCode,
				consignment.HVC_ConsigneePostcode,
				consignment.HVC_ConsigneeCity);

			var cachedValue = Factory.GetCachedValue(key, () => "KEY NOT FOUND");
			AssertEquals("Cache should have been populated with expected value", deliveryCartageZoneAfterCreatingZone, cachedValue);
		}

		public void TestDeliveryCartageZone_WhenCalculatingNewDeliveryCartageZoneAfterConsignmentChangesPostcodeAndCityTown_ReturnsNewMatchingZoneNameFromCache()
		{
			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "2200";
			postCode1.RK_RN_NKCountry = "AU";

			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "90001";
			postCode2.RK_RN_NKCountry = "US";

			var cityTown1 = LoadRefCityTown("SYDNEY", "NSW", "AU");
			var cityTown2 = LoadRefCityTown("BANKSTOWN", "NSW", "AU");

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			lastMileCarrier.OH_IsLocalTransport = true;

			var zoneRateTransportProvider1 = CreateZoneRateProvider(RatingConstants.RatingZoneTypes.All, "AU", lastMileCarrier, cityTown1);
			var zone1 = CreateZone("SYDNEY ZONE 1", zoneRateTransportProvider1);
			var zoneItem1 = zone1.Items.AddNew();
			zoneItem1.TQ_ToPostCode = postCode1.RK_CityTownPostCode;
			zoneItem1.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			zoneItem1.TQ_R9_CityTown = cityTown1.PK;
			zoneItem1.TQ_RN_NKCountry = "AU";

			var zoneRateTransportProvider2 = CreateZoneRateProvider(RatingConstants.RatingZoneTypes.All, "AU", lastMileCarrier, cityTown2);
			var zone2 = CreateZone("BANKSTOWN ZONE 2", zoneRateTransportProvider2);
			var zoneItem2 = zone2.Items.AddNew();
			zoneItem2.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
			zoneItem2.TQ_FromPostCode = postCode2.RK_CityTownPostCode;
			zoneItem2.TQ_R9_CityTown = cityTown2.PK;
			zoneItem2.TQ_RN_NKCountry = "AU";

			var shipment1 = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment1.JS_RL_NKDestination = "AUSYD";

			var location1 = LocationHelper.GetLocationFromString(shipment1.JS_RL_NKDestination, Factory);

			var consignmentHeader1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader1.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			consignment.HVC_ConsigneeCity = cityTown1.R9_InternationalName;
			consignment.HVC_ConsigneePostcode = postCode1.RK_CityTownPostCode;
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;

			Factory.Save();

			var key1 = string.Join("-",
				consignment.LastMileCarrier?.OH_Code.ToString() ?? string.Empty,
				location1.Code.ToString() ?? string.Empty,
				consignment.HVC_RN_NKConsigneeCountryCode,
				consignment.HVC_ConsigneePostcode,
				consignment.HVC_ConsigneeCity);

			var initialEmptyCacheValue1 = Factory.GetCachedValue(key1, () => "KEY1 NOT FOUND");
			AssertEquals("Precondition: No value should be stored in the cache", "KEY1 NOT FOUND", initialEmptyCacheValue1);
			consignment.Factory.ClearCachedValue<string>(key1);

			var rateTransportZoneHelper = ObjectFactory.Get<IRateTransportZoneHelper>();
			var expectedZoneName1 = rateTransportZoneHelper.GetZoneName(Factory, consignment.LastMileCarrier, location1, consignment.HVC_RN_NKConsigneeCountryCode, consignment.HVC_ConsigneePostcode, consignment.HVC_ConsigneeCity);
			var calculatedCartageZone1 = consignment.HVC_Calc_DeliveryCartageZone;

			AssertEquals("Precondition: RateTransportZoneHelper should have found our test zone", "SYDNEY ZONE 1", expectedZoneName1);
			AssertEquals("Precondition: Delivery Cartage Zone should be set using RateTransportZoneHelper", expectedZoneName1, calculatedCartageZone1);

			var zoneNameFromCache1 = consignment.Factory.GetCachedValue(key1, () => "KEY1 NOT FOUND");
			AssertEquals("Precondition: Calculated Delivery Cartage Zone should have been stored in the cache", calculatedCartageZone1, zoneNameFromCache1);

			var shipment2 = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment2.JS_RL_NKDestination = "AUBWU";

			var location2 = LocationHelper.GetLocationFromString(shipment2.JS_RL_NKDestination, Factory);

			var consignmentHeader2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			var consignment2 = consignmentHeader2.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN1";
			consignment2.HVC_ConsigneeCity = cityTown1.R9_InternationalName;
			consignment2.HVC_ConsigneePostcode = postCode1.RK_CityTownPostCode;
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment2.HVC_OH_LastMileCarrier = lastMileCarrier.PK;
			consignment2.HVC_ConsigneeCity = cityTown2.R9_InternationalName;
			consignment2.HVC_ConsigneePostcode = postCode2.RK_CityTownPostCode;
			consignment2.HVC_RN_NKConsigneeCountryCode = cityTown2.R9_RN_NKCountry;

			Factory.Save();

			var key2 = string.Join("-",
				consignment2.LastMileCarrier?.OH_Code.ToString() ?? string.Empty,
				location2.Code.ToString() ?? string.Empty,
				consignment2.HVC_RN_NKConsigneeCountryCode,
				consignment2.HVC_ConsigneePostcode,
				consignment2.HVC_ConsigneeCity);

			var initialEmptyCacheValue2 = Factory.GetCachedValue(key2, () => "KEY2 NOT FOUND");
			AssertEquals("Precondition: No value should be stored in the cache", "KEY2 NOT FOUND", initialEmptyCacheValue2);
			consignment2.Factory.ClearCachedValue<string>(key2);

			var expectedZoneName2 = rateTransportZoneHelper.GetZoneName(Factory, consignment2.LastMileCarrier, location2, consignment2.HVC_RN_NKConsigneeCountryCode, consignment2.HVC_ConsigneePostcode, consignment2.HVC_ConsigneeCity);
			var calculatedCartageZone2 = consignment2.HVC_Calc_DeliveryCartageZone;

			AssertEquals("Precondition: RateTransportZoneHelper should have found our test zone", "BANKSTOWN ZONE 2", expectedZoneName2);
			AssertEquals("Precondition: Delivery Cartage Zone should be set using RateTransportZoneHelper", expectedZoneName2, calculatedCartageZone2);

			var zoneNameFromCache2 = Factory.GetCachedValue(key2, () => "KEY2 NOT FOUND");
			AssertEquals("Cached value for Delivery Cartage Zone should be updated", expectedZoneName2, zoneNameFromCache2);
		}

		public void TestHVC_LastMileCarrierServiceCode()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "2000";
			postCode.RK_RN_NKCountry = "AU";

			var cityTown = LoadRefCityTown("SYDNEY", "NSW", "AU");

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			lastMileCarrier.OH_IsLocalTransport = true;
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			lastMileCarrier.MiscServ = miscServ;

			var servicelevel1 = miscServ.CarrierServiceLevels.AddNew();
			servicelevel1.PL_Code = "PRM";
			servicelevel1.PL_CarrierServiceCode = "";
			servicelevel1.PL_CarrierServiceLevelDescription = "123";
			var servicelevel2 = miscServ.CarrierServiceLevels.AddNew();
			servicelevel2.PL_Code = "STG";
			servicelevel2.PL_CarrierServiceCode = "456";
			servicelevel2.PL_CarrierServiceLevelDescription = "456";
			var servicelevel3 = miscServ.CarrierServiceLevels.AddNew();
			servicelevel3.PL_Code = "ALL";
			servicelevel3.PL_CarrierServiceCode = "789";
			servicelevel3.PL_CarrierServiceLevelDescription = "789";

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			consignment.HVC_ConsigneeCity = cityTown.R9_InternationalName;
			consignment.HVC_ConsigneePostcode = postCode.RK_CityTownPostCode;
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;

			Factory.Save();

			AssertEquals(ZString.Empty, consignment.LastMileCarrierServiceCode);

			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "STG";
			AssertEquals("456", consignment.LastMileCarrierServiceCode);
			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "PRM";
			AssertEquals(ZString.Empty, consignment.LastMileCarrierServiceCode);
			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "ALL";
			AssertEquals("789", consignment.LastMileCarrierServiceCode);

			var servicelevel4 = miscServ.CarrierServiceLevels.AddNew();
			servicelevel4.PL_Code = "ABC";
			servicelevel4.PL_CarrierServiceCode = "ABC";
			servicelevel4.PL_CarrierServiceLevelDescription = "ABC";
			Factory.Save();

			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "ABC";
			AssertEquals("ABC", consignment.LastMileCarrierServiceCode);

			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "STM";
			AssertEquals(ZString.Empty, consignment.LastMileCarrierServiceCode);
		}

		public void TestLastMileCarrierDepotID()
		{
			var consignment = Factory.New<HVLVConsignment>();
			AssertEquals("pre condition", ZString.Empty, consignment.LastMileCarrierDepotID);

			var carrier = Factory.New<OrgHeader>();
			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "123456";
			carrierAccount.OAN_DepotID = "Test Depot ID";

			consignment.HVC_OH_LastMileCarrier = carrier.PK;
			AssertEquals("Depot ID is not populated when no matching account number", ZString.Empty, consignment.LastMileCarrierDepotID);

			consignment.HVC_CarrierAccountNumber = "123456";
			AssertEquals("Depot ID is populated with matching last mile carrier and account number", "Test Depot ID", consignment.LastMileCarrierDepotID);
		}

		public void TestManifestedOnShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0100000";

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertEquals("S0100000", consignment.ManifestedOnShipment.JS_UniqueConsignRef);
		}

		public void TestHVLVConsignmentStatuses_AreTheSameAsTheConstants_IneTail()
		{
			var codes = typeof(HVLVConsignmentStatus.Codes).GetFields();
			var descriptions = typeof(HVLVConsignmentStatus.Descriptions).GetFields();
			var descriptionList = new List<string>();
			var consignment = Factory.New<HVLVConsignment>();

			foreach (var field in descriptions)
			{
				descriptionList.Add(field.GetValue(null).ToString());
			}

			CombineAssertions("Pre-conditions", delegate
			{
				AssertEquals(codes.Length, descriptions.Length);
				AssertEquals(codes.Length, consignment.Lookups.HVC_Status_List.GetAllCodes().Length);
			});

			foreach (var field in codes)
			{
				var code = field.GetValue(null);
				AssertEquals(true, consignment.Lookups.HVC_Status_List.ContainsCode(code));

				var description = consignment.Lookups.HVC_Status_List.GetDescriptionFromCode(code.ToString());
				AssertEquals(true, descriptionList.Contains(description));
			}
		}

		public void TestIsReturn_WhenFormerConsignmentExist_IsTrue()
		{
			var consignmentA = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentB = Factory.NewWithValidTestData<HVLVConsignment>();

			var returnPivot = Factory.NewWithValidTestData<HVLVReturnPivot>();

			returnPivot.HVP_HVC_Return = consignmentA.PK;
			returnPivot.HVP_HVC_Former = consignmentB.PK;

			Factory.Save();

			AssertEquals("The former consignment collection should have at least one element", true, consignmentA.FormerConsignments.Count > 0);
			AssertEquals("This consignment should be a return", true, consignmentA.IsReturn);
		}

		public void TestIsReturn_WhenNoFormerConsignment_IsFalse()
		{
			var consignmentA = Factory.NewWithValidTestData<HVLVConsignment>();

			Factory.Save();

			AssertEquals("The former consignment collection should be empty", false, consignmentA.FormerConsignments.Count > 0);
			AssertEquals("This consignment should not be a return", false, consignmentA.IsReturn);
		}

		public void TestFormerConsignmentCollection_ContainsFormerConsignmentsFromReturnPivot()
		{
			var consignmentA = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentB = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentC = Factory.NewWithValidTestData<HVLVConsignment>();
			Factory.NewWithValidTestData<HVLVConsignment>();

			var pivot1 = Factory.NewWithValidTestData<HVLVReturnPivot>();
			var pivot2 = Factory.NewWithValidTestData<HVLVReturnPivot>();

			pivot1.HVP_HVC_Former = consignmentB.PK;
			pivot1.HVP_HVC_Return = consignmentA.PK;

			pivot2.HVP_HVC_Former = consignmentC.PK;
			pivot2.HVP_HVC_Return = consignmentA.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { consignmentB, consignmentC }, consignmentA.FormerConsignments);
		}

		public void TestReturnConsignmentCollection_ContainsReturnConsignmentsFromReturnPivot()
		{
			var consignmentA = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentB = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentC = Factory.NewWithValidTestData<HVLVConsignment>();
			Factory.NewWithValidTestData<HVLVConsignment>();

			var pivot1 = Factory.NewWithValidTestData<HVLVReturnPivot>();
			var pivot2 = Factory.NewWithValidTestData<HVLVReturnPivot>();

			pivot1.HVP_HVC_Former = consignmentA.PK;
			pivot1.HVP_HVC_Return = consignmentB.PK;

			pivot2.HVP_HVC_Former = consignmentA.PK;
			pivot2.HVP_HVC_Return = consignmentC.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { consignmentB, consignmentC }, consignmentA.ReturnConsignments);
		}

		public void TestLoadCollectionPerformance_WithFetchHint()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();
			var line1 = item1.Lines.AddNew();
			line1.HVS_Quantity = 1;

			var consignment2 = header.Consignments.AddNew();
			var item2 = consignment2.Items.AddNew();
			var line2 = item2.Lines.AddNew();
			line2.HVS_Quantity = 1;

			Factory.Save();

			var expectedDbHitsWithFetchHint = new Dictionary<string, int>()
			{
				{ HVLVItemSchema.Constants.TableName, 1 },
				{ HVLVItemLineSchema.Constants.TableName, 1 },
			};

			CombineAssertions("Assert Db hit for both linq and separate collection load", () =>
			{
				var newFactory1 = new BusinessObjectFactory();
				var collectionCountThresholdToApplyFetchHints = 5;

				HVLVItemCollection.SetCollectionCountForTest(newFactory1, header.HCH_ClusterKey, collectionCountThresholdToApplyFetchHints);
				HVLVItemLineCollection.SetCollectionCountForTest(newFactory1, header.HCH_ClusterKey, collectionCountThresholdToApplyFetchHints);

				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHitsWithFetchHint, newFactory1, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
				{
					var loadedShipment = newFactory1.Load<ForwardingShipment>(shipment.PK);
					var allItems = loadedShipment.HVLVConsignments.OfType<HVLVConsignment>().SelectMany(x => x.Items.OfType<HVLVItem>()).ToArray();
					var allItemLines = loadedShipment.HVLVConsignments.OfType<HVLVConsignment>().SelectMany(x => x.Items.OfType<HVLVItem>()).SelectMany(x => x.Lines).ToArray();
				}

				var newFactory2 = new BusinessObjectFactory();
				HVLVItemCollection.SetCollectionCountForTest(newFactory2, header.HCH_ClusterKey, collectionCountThresholdToApplyFetchHints);
				HVLVItemLineCollection.SetCollectionCountForTest(newFactory2, header.HCH_ClusterKey, collectionCountThresholdToApplyFetchHints);

				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHitsWithFetchHint, newFactory2, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
				{
					var loadedConsignment1 = newFactory2.Load<HVLVConsignment>(consignment1.PK);
					loadedConsignment1.Items.Load();
					loadedConsignment1.Items[0].Lines.Load();

					var loadedConsignment2 = newFactory2.Load<HVLVConsignment>(consignment2.PK);
					loadedConsignment2.Items.Load();
					loadedConsignment2.Items[0].Lines.Load();
				}
			});
		}

		#region Item Totals

		public void TestItemCount()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.Items.AddNew();
			consignment.Items.AddNew();
			consignment.Items.AddNew();

			AssertEquals("Item count should be equal to number of items", (ZShort)3, consignment.HVC_ItemCount);
		}

		public void TestItemCount_ExcludesInactiveItems()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.Items.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item1.HVI_IsActive = false;
			item2.HVI_IsActive = false;

			AssertEquals("Item count should exclude inactive items", (ZShort)1, consignment.HVC_ItemCount);

			item1.HVI_IsActive = true;
			AssertEquals("Item count should include reactived items", (ZShort)2, consignment.HVC_ItemCount);
		}

		public void TestManifestedWeight()
		{
			var consignment = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);

			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item1.HVI_ManifestedWeight = 5;
			item2.HVI_ManifestedWeight = 10;

			AssertEquals("Manifested weight should be equal to sum of item manifested weights", 15m, consignment.HVC_ManifestedWeight);
		}

		public void TestManifestedWeight_ExcludesInactiveItems()
		{
			var consignment = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();

			item1.HVI_ManifestedWeight = 5;
			item2.HVI_ManifestedWeight = 7;
			item3.HVI_ManifestedWeight = 9;

			item2.HVI_IsActive = false;
			item3.HVI_IsActive = false;

			AssertEquals("Manifested Weight should exclude inactive items", 5m, consignment.HVC_ManifestedWeight);

			item2.HVI_ManifestedWeight = 11;
			AssertEquals("Changing weight of inactive item should have no affect", 5m, consignment.HVC_ManifestedWeight);

			item2.HVI_IsActive = true;
			AssertEquals("Manifested Weight should include reactivated items", 16m, consignment.HVC_ManifestedWeight);
		}

		public void TestManifestedVolume()
		{
			var consignment = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item1.HVI_ManifestedVolume = 5;
			item2.HVI_ManifestedVolume = 10;

			AssertEquals("Manifested volume should be equal to sum of item manifested volumes", 15m, consignment.HVC_ManifestedVolume);
		}

		public void TestManifestedVolume_ExcludesInactiveItems()
		{
			var consignment = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();

			item1.HVI_ManifestedVolume = 5;
			item2.HVI_ManifestedVolume = 7;
			item3.HVI_ManifestedVolume = 9;

			item2.HVI_IsActive = false;
			item3.HVI_IsActive = false;

			AssertEquals("Manifested Volume should exclude inactive items", 5m, consignment.HVC_ManifestedVolume);

			item2.HVI_ManifestedVolume = 11;
			AssertEquals("Changing volume of inactive item should have no affect", 5m, consignment.HVC_ManifestedVolume);

			item2.HVI_IsActive = true;
			AssertEquals("Manifested Volume should include reactivated items", 16m, consignment.HVC_ManifestedVolume);
		}

		public void TestActualWeight()
		{
			var consignment = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item1.HVI_ActualWeight = 5;
			item2.HVI_ActualWeight = 10;

			AssertEquals("Actual weight should be equal to sum of item actual weights", 15m, consignment.HVC_ActualWeight);
		}

		public void TestActualWeight_ExcludesInactiveItems()
		{
			var consignment = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();

			item1.HVI_ActualWeight = 5;
			item2.HVI_ActualWeight = 7;
			item3.HVI_ActualWeight = 9;

			item2.HVI_IsActive = false;
			item3.HVI_IsActive = false;

			AssertEquals("Actual Weight should exclude inactive items", 5m, consignment.HVC_ActualWeight);

			item2.HVI_ActualWeight = 11;
			AssertEquals("Changing weight of inactive item should have no affect", 5m, consignment.HVC_ActualWeight);

			item2.HVI_IsActive = true;
			AssertEquals("Actual Weight should include reactivated items", 16m, consignment.HVC_ActualWeight);
		}

		public void TestActualVolume()
		{
			var consignment = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item1.HVI_ActualVolume = 5;
			item2.HVI_ActualVolume = 10;

			AssertEquals("Actual volume should be equal to sum of item actual volumes", 15m, consignment.HVC_ActualVolume);
		}

		public void TestActualVolume_ExcludesInactiveItems()
		{
			var consignment = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();

			item1.HVI_ActualVolume = 5;
			item2.HVI_ActualVolume = 7;
			item3.HVI_ActualVolume = 9;

			item2.HVI_IsActive = false;
			item3.HVI_IsActive = false;

			AssertEquals("Actual Volume should exclude inactive items", 5m, consignment.HVC_ActualVolume);

			item2.HVI_ActualVolume = 11;
			AssertEquals("Changing volume of inactive item should have no affect", 5m, consignment.HVC_ActualVolume);

			item2.HVI_IsActive = true;
			AssertEquals("Actual Volume should include reactivated items", 16m, consignment.HVC_ActualVolume);
		}

		public void TestBookingHeaderItemCount()
		{
			var header = SetupBookingHeaderWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var consignment1 = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var consignment2 = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);

			header.Consignments.Add(consignment1);
			header.Consignments.Add(consignment2);

			consignment1.Items.AddNew();
			consignment2.Items.AddNew();
			consignment2.Items.AddNew();

			AssertEquals("Item count should be equal to sum of consignment items", (ZShort)3, header.HVH_ItemCount);
		}

		public void TestBookingHeaderGrossWeight()
		{
			var header = SetupBookingHeaderWithUnits(Weight.Kilograms, Volume.CubicFeet);
			var consignment1 = SetupConsignmentWithUnits(Weight.Ounces, Volume.CubicCentimeters);
			var consignment2 = SetupConsignmentWithUnits(Weight.Ounces, Volume.CubicCentimeters);

			header.Consignments.Add(consignment1);
			header.Consignments.Add(consignment2);

			var item1 = consignment1.Items.AddNew();
			item1.HVI_ManifestedWeight = 11;

			var item2 = consignment2.Items.AddNew();
			item2.HVI_ManifestedWeight = 22;

			AssertEquals("Gross weight should be equal to sum of consignment weights", 0.935535m, header.HVH_GrossWeight, 0.000001m);
		}

		public void TestBookingHeaderGrossWeightConsignmentActualOverridesManifested()
		{
			var header = SetupBookingHeaderWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var consignment1 = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var consignment2 = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);

			header.Consignments.Add(consignment1);
			header.Consignments.Add(consignment2);

			var item1 = consignment1.Items.AddNew();
			item1.HVI_ManifestedWeight = 5;

			var item2 = consignment2.Items.AddNew();
			item2.HVI_ManifestedWeight = 10;

			var item3 = consignment1.Items.AddNew();
			item3.HVI_ActualWeight = 10;

			AssertEquals("Gross weight should be equal to sum of item effective weights", (ZDecimal)25, header.HVH_GrossWeight);
		}

		public void TestBookingHeaderGrossVolume()
		{
			var header = SetupBookingHeaderWithUnits(Weight.Kilograms, Volume.CubicFeet);
			var consignment1 = SetupConsignmentWithUnits(Weight.Ounces, Volume.CubicCentimeters);
			var consignment2 = SetupConsignmentWithUnits(Weight.Ounces, Volume.CubicCentimeters);

			header.Consignments.Add(consignment1);
			header.Consignments.Add(consignment2);

			var item1 = consignment1.Items.AddNew();
			item1.HVI_ManifestedVolume = 111;

			var item2 = consignment2.Items.AddNew();
			item2.HVI_ManifestedVolume = 222;

			AssertEquals("Gross volume should be equal to sum of consignment volumes", 0.01176m, header.HVH_GrossVolume);

			var item3 = consignment1.Items.AddNew();
			item3.HVI_ActualVolume = 333;

			AssertEquals("Gross volume should be equal to sum of item effective volumes", 0.02352m, header.HVH_GrossVolume);
		}

		public void TestBookingHeaderGrossVolumeConsignmentActualOverridesManifested()
		{
			var header = SetupBookingHeaderWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var consignment1 = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);
			var consignment2 = SetupConsignmentWithUnits(Weight.Kilograms, Volume.CubicMetres);

			header.Consignments.Add(consignment1);
			header.Consignments.Add(consignment2);

			var item1 = consignment1.Items.AddNew();
			item1.HVI_ManifestedVolume = 5;

			var item2 = consignment2.Items.AddNew();
			item2.HVI_ManifestedVolume = 10;

			var item3 = consignment1.Items.AddNew();
			item3.HVI_ActualVolume = 10;

			AssertEquals("Gross volume should be equal to sum of item effective volumes", (ZDecimal)25, header.HVH_GrossVolume);
		}

		#endregion

		#region Item Line Totals

		public void TestItemLineTotals()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item1.Lines.AddNew();
			item1.Lines.AddNew();
			item2.Lines.AddNew();

			AssertEquals("Should have 3 item lines", (ZShort)3, consignment.TotalItemLines);
		}

		public void TestItemLineTotals_ExcludesInactiveItems()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item1.Lines.AddNew();
			item1.Lines.AddNew();
			item2.Lines.AddNew();

			item1.HVI_IsActive = false;
			AssertEquals("Should have excluded lines from inactive items", (ZShort)1, consignment.TotalItemLines);

			item1.HVI_IsActive = true;
			AssertEquals("Should have included reactived items", (ZShort)3, consignment.TotalItemLines);
		}

		public void TestLineValueTotals()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var itemA = consignment.Items.AddNew();
			var itemB = consignment.Items.AddNew();

			var lineA1 = itemA.Lines.AddNew();
			var lineA2 = itemA.Lines.AddNew();
			var lineB1 = itemB.Lines.AddNew();
			var lineB2 = itemB.Lines.AddNew();

			lineA1.HVS_CustomsValue = 10m;
			lineA2.HVS_CustomsValue = 0.28m;
			lineB1.HVS_CustomsValue = 6m;
			lineB2.HVS_CustomsValue = 300m;

			AssertEquals("Should have total customs value of 316.28", 316.28m, consignment.TotalLineCustomsValues);
		}

		public void TestConsignmentsBelongToSameConsignee()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();
			var consignment3 = bookingHeader.Consignments.AddNew();
			var consigneeAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress1.Address1 = "Consignee1 Address1";
			consigneeAddress1.Address2 = "Consignee1 Address2";
			consigneeAddress1.City = "City 1";
			consigneeAddress1.State = "State 1";
			consigneeAddress1.Postcode = "1111";
			consigneeAddress1.OA_RN_NKCountryCode = "AU";
			var consigneeAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress2.Address1 = "Consignee2 Address1";
			consigneeAddress2.Address2 = "Consignee2 Address2";
			consigneeAddress2.City = "City 2";
			consigneeAddress2.State = "State 2";
			consigneeAddress2.Postcode = "2222";
			consigneeAddress2.OA_RN_NKCountryCode = "US";
			consignment1.HVC_OA_ConsigneeAddress = consigneeAddress1.PK;
			consignment2.HVC_OA_ConsigneeAddress = consigneeAddress1.PK;
			consignment3.HVC_OA_ConsigneeAddress = consigneeAddress2.PK;

			Factory.Save();

			CombineAssertions("Expected consignment to retain a list of consignments with the same consignees", () =>
			{
				AssertArrayEqualsByElements("Expected consignment1.ConsignmentsBelongToSameConsignee to return consignment1 and consignment2", new HVLVConsignment[] { consignment1, consignment2 }, consignment1.ConsignmentsBelongToSameConsignee.ToArray());
				AssertArrayEqualsByElements("Expected consignment2.ConsignmentsBelongToSameConsignee to return consignment1 and consignment2", new HVLVConsignment[] { consignment1, consignment2 }, consignment2.ConsignmentsBelongToSameConsignee.ToArray());
				AssertArrayEqualsByElements("Expected consignment3.ConsignmentsBelongToSameConsignee to return consignment3", new HVLVConsignment[] { consignment3 }, consignment3.ConsignmentsBelongToSameConsignee.ToArray());
			});
		}

		public void TestConsignmentsBelongToSameConsigneeExcludingParent()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();
			var consignment3 = bookingHeader.Consignments.AddNew();
			var consigneeAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress1.Address1 = "Consignee1 Address1";
			consigneeAddress1.Address2 = "Consignee1 Address2";
			consigneeAddress1.City = "City 1";
			consigneeAddress1.State = "State 1";
			consigneeAddress1.Postcode = "1111";
			consigneeAddress1.OA_RN_NKCountryCode = "AU";
			var consigneeAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress2.Address1 = "Consignee2 Address1";
			consigneeAddress2.Address2 = "Consignee2 Address2";
			consigneeAddress2.City = "City 2";
			consigneeAddress2.State = "State 2";
			consigneeAddress2.Postcode = "2222";
			consigneeAddress2.OA_RN_NKCountryCode = "US";
			consignment1.HVC_OA_ConsigneeAddress = consigneeAddress1.PK;
			consignment2.HVC_OA_ConsigneeAddress = consigneeAddress1.PK;
			consignment3.HVC_OA_ConsigneeAddress = consigneeAddress2.PK;

			Factory.Save();

			CombineAssertions("Expected consignment to retain a list of consignments with the same consignees excluding parent consignment", () =>
			{
				AssertArrayEqualsByElements("Expected consignment1.ConsignmentsBelongToSameConsigneeIncludingParent to return consignment2", new HVLVConsignment[] { consignment2 }, consignment1.ConsignmentsBelongToSameConsigneeExcludingParent.ToArray());
				AssertArrayEqualsByElements("Expected consignment2.ConsignmentsBelongToSameConsigneeIncludingParent to return consignment1", new HVLVConsignment[] { consignment1 }, consignment2.ConsignmentsBelongToSameConsigneeExcludingParent.ToArray());
				AssertArrayEqualsByElements("Expected consignment3.ConsignmentsBelongToSameConsigneeIncludingParent to return empty result", Array.Empty<HVLVConsignment>(), consignment3.ConsignmentsBelongToSameConsigneeExcludingParent.ToArray());
			});
		}

		public void TestConsignmentsBelongToSameConsignee_MustHaveSameConsigneeName()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();

			consignment1.HVC_ConsigneeName = "Consignee Name XYZ";
			consignment1.HVC_ConsigneeAddress1 = "Address1";
			consignment1.HVC_ConsigneeAddress2 = "Address2";
			consignment1.HVC_ConsigneeCity = "City";
			consignment1.HVC_ConsigneeState = "State";
			consignment1.HVC_ConsigneePostcode = "1111";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";

			consignment2.HVC_ConsigneeName = "Consignee Name ABC";
			consignment2.HVC_ConsigneeAddress1 = "Address1";
			consignment2.HVC_ConsigneeAddress2 = "Address2";
			consignment2.HVC_ConsigneeCity = "City";
			consignment2.HVC_ConsigneeState = "State";
			consignment2.HVC_ConsigneePostcode = "1111";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";

			Factory.Save();

			CombineAssertions("Expected no consignments to have a common consignee, as consignee1 and consignee2 have different names", () =>
			{
				AssertEquals(1, consignment1.ConsignmentsBelongToSameConsignee.Count);
				AssertEquals(1, consignment2.ConsignmentsBelongToSameConsignee.Count);
			});

			consignment2.HVC_ConsigneeName = "Consignee Name XYZ";

			Factory.Save();

			CombineAssertions("Expected consignment to retain a list of consignments with the same consignees", () =>
			{
				AssertArrayEqualsByElements("Expected consignment1.ConsignmentsBelongToSameConsignee to return consignment1 and consignment2", new HVLVConsignment[] { consignment1, consignment2 }, consignment1.ConsignmentsBelongToSameConsignee.ToArray());
				AssertArrayEqualsByElements("Expected consignment2.ConsignmentsBelongToSameConsignee to return consignment1 and consignment2", new HVLVConsignment[] { consignment1, consignment2 }, consignment2.ConsignmentsBelongToSameConsignee.ToArray());
			});
		}

		public void TestLineValueTotals_ExcludesInactiveItems()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var itemA = consignment.Items.AddNew();
			var itemB = consignment.Items.AddNew();

			var lineA1 = itemA.Lines.AddNew();
			var lineA2 = itemA.Lines.AddNew();
			var lineB1 = itemB.Lines.AddNew();
			var lineB2 = itemB.Lines.AddNew();

			lineA1.HVS_CustomsValue = 10m;
			lineA2.HVS_CustomsValue = 0.28m;
			lineB1.HVS_CustomsValue = 6m;
			lineB2.HVS_CustomsValue = 300m;

			itemA.HVI_IsActive = false;
			AssertEquals("Should have excluded lines from inactive Item", 306.00m, consignment.TotalLineCustomsValues);

			itemA.HVI_IsActive = true;
			AssertEquals("Should have included reactived items", 316.28m, consignment.TotalLineCustomsValues);
		}

		public void TestFirstLineWeightUnit()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_WeightUQ = Weight.Kilograms;
			AssertEquals("With no item lines, First Line Weight Unit shall be consignment's WeightUQ", Weight.Kilograms, consignment.FirstLineWeightUnit);

			var itemA = consignment.Items.AddNew();
			var itemB = consignment.Items.AddNew();
			var lineA1 = itemA.Lines.AddNew();
			var lineA2 = itemA.Lines.AddNew();
			var lineB1 = itemB.Lines.AddNew();

			lineA1.HVS_WeightUnit = Weight.Tonnes;
			AssertEquals("First line unit Changed", Weight.Tonnes, consignment.FirstLineWeightUnit);

			lineA2.HVS_WeightUnit = Weight.Hectograms;
			AssertEquals("Second line unit Changed", Weight.Tonnes, consignment.FirstLineWeightUnit);

			lineB1.HVS_WeightUnit = Weight.Milligrams;
			AssertEquals("First line unit Changed", Weight.Tonnes, consignment.FirstLineWeightUnit);

			lineA1.HVS_WeightUnit = Weight.Kilotonnes;
			AssertEquals("First line unit Changed", Weight.Kilotonnes, consignment.FirstLineWeightUnit);

			lineA1.HVS_WeightUnit = string.Empty;
			AssertEquals("First line unit Changed", Weight.Kilograms, consignment.FirstLineWeightUnit);

			consignment.HVC_WeightUQ = Weight.Tonnes;
			AssertEquals("consignment unit Changed", Weight.Tonnes, consignment.FirstLineWeightUnit);
		}

		public void TestFirstLineWeightUnit_ExcludesInactiveItems()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_WeightUQ = Weight.Kilograms;
			AssertEquals("With no item lines, First Line Weight Unit shall be consignment's WeightUQ", Weight.Kilograms, consignment.FirstLineWeightUnit);

			var itemA = consignment.Items.AddNew();
			var itemB = consignment.Items.AddNew();

			var lineA1 = itemA.Lines.AddNew();
			var lineA2 = itemA.Lines.AddNew();
			var lineB1 = itemB.Lines.AddNew();

			lineA1.HVS_WeightUnit = Weight.Tonnes;
			lineA2.HVS_WeightUnit = Weight.Hectograms;
			lineB1.HVS_WeightUnit = Weight.Milligrams;

			itemA.HVI_IsActive = false;
			AssertEquals("First line unit should exclude inactive items", Weight.Milligrams, consignment.FirstLineWeightUnit);

			lineA1.HVS_WeightUnit = Weight.Kilotonnes;

			AssertEquals("First line unit doesn't change if changed item is inactive", Weight.Milligrams, consignment.FirstLineWeightUnit);

			itemA.HVI_IsActive = true;

			AssertEquals("Reactivated item is used to determine first line unit", Weight.Kilotonnes, consignment.FirstLineWeightUnit);
		}

		public void TestTotalLineGrossWeight()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			AssertEquals("Should have total line net weight of 0", 0m, consignment.TotalLineGrossWeight);
			consignment.HVC_WeightUQ = Weight.Kilograms;

			var itemA = consignment.Items.AddNew();
			var itemB = consignment.Items.AddNew();

			var lineA1 = itemA.Lines.AddNew();
			var lineA2 = itemA.Lines.AddNew();
			var lineB1 = itemB.Lines.AddNew();
			var lineB2 = itemB.Lines.AddNew();

			lineA1.HVS_WeightUnit = Weight.Kilograms;
			lineA2.HVS_WeightUnit = Weight.Kilograms;
			lineB1.HVS_WeightUnit = Weight.Kilograms;
			lineB2.HVS_WeightUnit = Weight.Kilograms;

			lineA1.HVS_GrossWeight = 30m;
			lineA2.HVS_GrossWeight = 0.56m;
			lineB1.HVS_GrossWeight = 8m;
			lineB2.HVS_GrossWeight = 500m;

			CombineAssertions("Should have total line gross weight of 538.56 KG", () =>
			{
				AssertEquals("Weight unit:", Weight.Kilograms, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 538.56m, consignment.TotalLineGrossWeight);
			});

			lineA1.HVS_WeightUnit = Weight.Tonnes;

			CombineAssertions("Should have total line gross weight of 30.50856 T", () =>
			{
				AssertEquals("Weight unit:", Weight.Tonnes, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 30.50856m, consignment.TotalLineGrossWeight);
			});

			lineA1.HVS_WeightUnit = Weight.Grams;

			CombineAssertions("Should have total line gross weight of 508590 G", () =>
			{
				AssertEquals("Weight unit:", Weight.Grams, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 508590m, consignment.TotalLineGrossWeight);
			});
		}

		public void TestTotalLineGrossWeight_ExcludesInactiveItems()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			AssertEquals("Should have total line net weight of 0", 0m, consignment.TotalLineGrossWeight);
			consignment.HVC_WeightUQ = Weight.Kilograms;

			var itemA = consignment.Items.AddNew();
			var itemB = consignment.Items.AddNew();

			var lineA1 = itemA.Lines.AddNew();
			var lineA2 = itemA.Lines.AddNew();
			var lineB1 = itemB.Lines.AddNew();
			var lineB2 = itemB.Lines.AddNew();

			lineA1.HVS_WeightUnit = Weight.Kilograms;
			lineA2.HVS_WeightUnit = Weight.Kilograms;
			lineB1.HVS_WeightUnit = Weight.Kilograms;
			lineB2.HVS_WeightUnit = Weight.Kilograms;

			lineA1.HVS_GrossWeight = 30m;
			lineA2.HVS_GrossWeight = 0.56m;
			lineB1.HVS_GrossWeight = 8m;
			lineB2.HVS_GrossWeight = 500m;

			itemA.HVI_IsActive = false;

			CombineAssertions("Total line gross weight should exclude lines from inactive items", () =>
			{
				AssertEquals("Weight unit:", Weight.Kilograms, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 508m, consignment.TotalLineGrossWeight);
			});

			itemA.HVI_IsActive = true;

			CombineAssertions("Total line gross weight should include lines from reactivated items", () =>
			{
				AssertEquals("Weight unit:", Weight.Kilograms, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 538.56m, consignment.TotalLineGrossWeight);
			});
		}

		public void TestTotalLineNetWeight()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			AssertEquals("Should have total line net weight of 0", 0m, consignment.TotalLineNetWeight);
			consignment.HVC_WeightUQ = Weight.Kilograms;

			var itemA = consignment.Items.AddNew();
			var itemB = consignment.Items.AddNew();

			var lineA1 = itemA.Lines.AddNew();
			var lineA2 = itemA.Lines.AddNew();
			var lineB1 = itemB.Lines.AddNew();
			var lineB2 = itemB.Lines.AddNew();

			lineA1.HVS_WeightUnit = Weight.Kilograms;
			lineA2.HVS_WeightUnit = Weight.Kilograms;
			lineB1.HVS_WeightUnit = Weight.Kilograms;
			lineB2.HVS_WeightUnit = Weight.Kilograms;

			lineA1.HVS_NetWeight = 60m;
			lineA2.HVS_NetWeight = 0.89m;
			lineB1.HVS_NetWeight = 7m;
			lineB2.HVS_NetWeight = 400m;

			CombineAssertions("Should have total line gross weight of 467.89 KG", () =>
			{
				AssertEquals("Weight unit:", Weight.Kilograms, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 467.89m, consignment.TotalLineNetWeight);
			});

			lineA1.HVS_WeightUnit = Weight.Tonnes;

			CombineAssertions("Should have total line gross weight of 60.40789 T", () =>
			{
				AssertEquals("Weight unit:", Weight.Tonnes, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 60.40789m, consignment.TotalLineNetWeight);
			});

			lineA1.HVS_WeightUnit = Weight.Grams;

			CombineAssertions("Should have total line gross weight of 407950 G", () =>
			{
				AssertEquals("Weight unit:", Weight.Grams, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 407950m, consignment.TotalLineNetWeight);
			});
		}

		public void TestTotalLineNetWeight_ExcludesInactiveItems()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			AssertEquals("Should have total line net weight of 0", 0m, consignment.TotalLineNetWeight);
			consignment.HVC_WeightUQ = Weight.Kilograms;

			var itemA = consignment.Items.AddNew();
			var itemB = consignment.Items.AddNew();

			var lineA1 = itemA.Lines.AddNew();
			var lineA2 = itemA.Lines.AddNew();
			var lineB1 = itemB.Lines.AddNew();
			var lineB2 = itemB.Lines.AddNew();

			lineA1.HVS_WeightUnit = Weight.Kilograms;
			lineA2.HVS_WeightUnit = Weight.Kilograms;
			lineB1.HVS_WeightUnit = Weight.Kilograms;
			lineB2.HVS_WeightUnit = Weight.Kilograms;

			lineA1.HVS_NetWeight = 60m;
			lineA2.HVS_NetWeight = 0.89m;
			lineB1.HVS_NetWeight = 7m;
			lineB2.HVS_NetWeight = 400m;

			itemA.HVI_IsActive = false;

			CombineAssertions("Total line net weight should exclude lines from inactive items", () =>
			{
				AssertEquals("Weight unit:", Weight.Kilograms, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 407m, consignment.TotalLineNetWeight);
			});

			itemA.HVI_IsActive = true;

			CombineAssertions("Total line net weight should include lines from reactivated items", () =>
			{
				AssertEquals("Weight unit:", Weight.Kilograms, consignment.FirstLineWeightUnit);
				AssertEquals("Total gross weight:", 467.89m, consignment.TotalLineNetWeight);
			});
		}

		#endregion

		#region Chargeable

		public void TestChargeable()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			AssertEquals("Pre-condition", ConversionFactor.Standard.Metric.Air, FreightDataRegistry.Instance.InternationalChargeableFactorAir.Value.MetricFactor);

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			consignment.ManagingShipment = shipment;

			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			using (HVLVDataRegistry.Instance.CalculateHVLVChargeablePerItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				item1.HVI_ManifestedWeight = 8;
				item1.HVI_ActualWeight = 5.3;
				item1.HVI_ActualVolume = 36000; // 6 KG chargeable
				item2.HVI_ManifestedWeight = 7.4;
				item2.HVI_ManifestedVolume = 27000; // 4.5 KG chargeable

				consignment.CalculateChargeable();

				AssertEquals("Chargeable calculated from item1 actual volume and item2 manifested weight", "13.4 KG", consignment.ChargeableForDisplay);
			}

			using (HVLVDataRegistry.Instance.CalculateHVLVChargeablePerItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				item1.HVI_ManifestedWeight = 9; // recalculate

				consignment.CalculateChargeable();

				AssertEquals("Chargeable calculated from item1 actual weight + item2 manifested weight", "12.7 KG", consignment.ChargeableForDisplay);
			}
		}

		public void TestChargeable_ExcludesInactiveItems()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			AssertEquals("Pre-condition", ConversionFactor.Standard.Metric.Air, FreightDataRegistry.Instance.InternationalChargeableFactorAir.Value.MetricFactor);

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			consignment.ManagingShipment = shipment;
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			item2.HVI_IsActive = false;
			using (HVLVDataRegistry.Instance.CalculateHVLVChargeablePerItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				item1.HVI_ManifestedWeight = 8;
				item1.HVI_ActualWeight = 5.3;
				item1.HVI_ActualVolume = 36000; // 6 KG chargeable
				item2.HVI_ManifestedWeight = 7.4;
				item2.HVI_ManifestedVolume = 27000; // 4.5 KG chargeable

				consignment.CalculateChargeable();

				AssertEquals("Inactive Items excluded from chargeable calculation", "6 KG", consignment.ChargeableForDisplay);
			}
		}

		public void TestChargeableOnDemand()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			consignment.ManagingShipment = shipment;

			AssertEquals("Chargeable for display not returning correct value for null input", "Not Calculated", consignment.ChargeableForDisplay);
			AssertEquals("Pre-condition", ConversionFactor.Standard.Metric.Air, FreightDataRegistry.Instance.InternationalChargeableFactorAir.Value.MetricFactor);

			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			using (HVLVDataRegistry.Instance.CalculateHVLVChargeablePerItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				item1.HVI_ManifestedWeight = 8;
				item1.HVI_ActualWeight = 5.3;
				item1.HVI_ActualVolume = 36000; // 6 KG chargeable
				item2.HVI_ManifestedWeight = 7.4;
				item2.HVI_ManifestedVolume = 27000; // 4.5 KG chargeable

				consignment.CalculateChargeable();

				AssertEquals("Chargeable calculated from item1 actual volume and item2 manifested weight", "13.4 KG", consignment.ChargeableForDisplay);
			}
		}

		public void TestChargeable_WhenChangeUQ_UpdateItemChargeableInfo()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;

			var item = consignment.Items.AddNew();
			item.HVI_ActualVolume = 1;
			item.HVI_ActualWeight = 1;

			AssertEquals("0.001 M3", item.VolumeWeightForDisplay);
			AssertEquals("1 M3", item.ChargeableForDisplay);

			consignment.HVC_WeightUQ = Weight.Kilotonnes;

			CombineAssertions("update item chargeabel and weight volume after change weight UQ", () =>
			{
				AssertEquals("weight volume", "1000 M3", item.VolumeWeightForDisplay);
				AssertEquals("chargeable", "1000 M3", item.ChargeableForDisplay);
			});

			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.Litre;

			CombineAssertions("update item chargeabel and weight volume after change volume UQ", () =>
			{
				AssertEquals("weight volume", "0.001 M3", item.VolumeWeightForDisplay);
				AssertEquals("chargeable", "0.001 M3", item.ChargeableForDisplay);
			});
		}

		public void TestShipmentForChargeableCalculationFallback()
		{
			var managingShipment = Factory.New<ForwardingShipment>();
			managingShipment.JS_TransportMode = TransportModes.Air;

			var manifestedOnShipment = Factory.New<ForwardingShipment>();
			manifestedOnShipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_VolumeUQ = "M3";
			consignment.HVC_WeightUQ = "KG";
			consignment.HVC_JS_ManifestedOnShipment = manifestedOnShipment.PK;

			AssertNull("Pre-condition: consignment managing shipment is null", consignment.ManagingShipment);
			AssertEquals("Chargeable UQ are calculated from manifested on shipment when managing shipment is null", "M3", consignment.ChargeableUQ);

			consignment.ManagingShipment = managingShipment;
			AssertEquals("Chargeable UQ are calculated from managing shipment if it's not null", "KG", consignment.ChargeableUQ);
		}

		public void TestVolumeWeight_ByTransportMode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			Assert("Precondition: Shipment is chargeable by weight when transport mode is Air", shipment.IsShipmentChargeableByWeight);

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			consignment.ManagingShipment = shipment;

			var item1 = consignment.Items.AddNew();
			item1.HVI_ActualWeight = 5.3;
			item1.HVI_ActualVolume = 36000;

			var item2 = consignment.Items.AddNew();
			item2.HVI_ManifestedWeight = 7.4;
			item2.HVI_ManifestedVolume = 27000;

			AssertEquals("10.5 KG", consignment.VolumeWeightForDisplay);

			shipment.JS_TransportMode = TransportModes.Sea;
			Assert("Precondition: Shipment is chargeable by weight when transport mode is Air", !shipment.IsShipmentChargeableByWeight);

			consignment.VolumeWeightForDisplayInfo.RefreshBinding();
			AssertEquals("0.013 M3", consignment.VolumeWeightForDisplay);
		}

		public void TestVolumeWeight_RecalculateAfterChangeWeightOrVolumeOrUQ()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			Assert("Precondition: Shipment is chargeable by weight when transport mode is Air", shipment.IsShipmentChargeableByWeight);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = consignment.Items.AddNew();
			item.HVI_ManifestedVolume = 1;
			AssertEquals("166.667 KG", consignment.VolumeWeightForDisplay);

			item.HVI_ManifestedVolume = 2;
			AssertEquals("consignment manifested volume has been changed", 2m, consignment.HVC_ManifestedVolume);
			AssertEquals("333.333 KG", consignment.VolumeWeightForDisplay);

			item.HVI_ActualVolume = 3;
			AssertEquals("consignment actual volume has been changed", 3m, consignment.HVC_ActualVolume);
			AssertEquals("500 KG", consignment.VolumeWeightForDisplay);

			consignment.HVC_VolumeUQ = Volume.CubicFeet;
			AssertEquals("14.158 KG", consignment.VolumeWeightForDisplay);

			shipment.JS_TransportMode = TransportModes.Sea;
			Assert("Precondition: Shipment is chargeable by volume when transport mode is Sea", !shipment.IsShipmentChargeableByWeight);

			item.HVI_ManifestedWeight = 1;
			AssertEquals("0.001 M3", consignment.VolumeWeightForDisplay);

			item.HVI_ManifestedWeight = 2;
			AssertEquals("consignment manifested Weight has been changed", 2m, consignment.HVC_ManifestedWeight);
			AssertEquals("0.002 M3", consignment.VolumeWeightForDisplay);

			item.HVI_ActualWeight = 3;
			AssertEquals("consignment actual Weight has been changed", 3m, consignment.HVC_ActualWeight);
			AssertEquals("0.003 M3", consignment.VolumeWeightForDisplay);

			consignment.HVC_WeightUQ = Weight.Kilotonnes;
			AssertEquals("3000 M3", consignment.VolumeWeightForDisplay);
		}

		#endregion

		public void TestSetConsignmentWeightUnit_UpdatesItemManifestedWeight()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = Weight.Kilograms;

			var item = consignment.Items.AddNew();

			var itemLine1 = item.Lines.AddNew();
			var itemLine2 = item.Lines.AddNew();

			itemLine1.HVS_WeightUnit = Weight.Kilograms;
			itemLine2.HVS_WeightUnit = Weight.Kilograms;
			itemLine1.HVS_GrossWeight = 3;
			itemLine2.HVS_GrossWeight = 2;

			AssertEquals("Manifested Weight before Consignment Weight Unit change", 5M, item.HVI_ManifestedWeight);

			consignment.HVC_WeightUQ = Weight.Grams;
			item.CalculateManifestedWeight();
			AssertEquals("Item Manifested Weight recalculated", 5000M, item.HVI_ManifestedWeight);
		}

		public void TestHasDeclarationForCurrentDirection()
		{
			var consignment = SetupConsignment();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.UnitedStates))
			{
				Assert("Does not have declaration", !consignment.HasDeclarationForCurrentDirection);

				var declaration = Factory.New<BaseJobDeclaration>();
				consignment.HVC_JE_ImportDeclaration = declaration.PK;

				Assert("Has import declaration", consignment.HasDeclarationForCurrentDirection);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				Assert("Does not have declaration", !consignment.HasDeclarationForCurrentDirection);

				var declaration = Factory.New<BaseJobDeclaration>();
				consignment.HVC_JE_ExportDeclaration = declaration.PK;

				Assert("Has export declaration", consignment.HasDeclarationForCurrentDirection);
			}
		}

		public void TestCanConvertToStandAloneDeclaration()
		{
			var consignment = SetupConsignment();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				Assert("Cannot convert to standalone declaration when there is no consignment header", !consignment.CanConvertToStandAloneDeclaration);

				var consignmentHeader = Factory.New<HVLVConsignmentHeader>();
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				Assert("Can convert when there is consignment Header and login as export party", consignment.CanConvertToStandAloneDeclaration);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				Assert("Cannot convert to standalone declaration when logging in as third party", !consignment.CanConvertToStandAloneDeclaration);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.UnitedStates))
			{
				Assert("Can convert when there is consignment Header and login as import party", consignment.CanConvertToStandAloneDeclaration);

				consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;

				Assert("Cannot convert when ReleaseStatus is cleared", !consignment.CanConvertToStandAloneDeclaration);

				var declaration = Factory.New<BaseJobDeclaration>();
				consignment.HVC_JE_ImportDeclaration = declaration.PK;

				Assert("Cannot convert to import standalone declaration again", !consignment.CanConvertToStandAloneDeclaration);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				Assert("Can convert to export standalone declaration when logging in as export party", consignment.CanConvertToStandAloneDeclaration);

				consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Cleared;

				Assert("Cannot convert when ReleaseStatus is cleared", !consignment.CanConvertToStandAloneDeclaration);

				var declaration = Factory.New<BaseJobDeclaration>();
				consignment.HVC_JE_ExportDeclaration = declaration.PK;

				Assert("Cannot convert to export standalone declaration again", !consignment.CanConvertToStandAloneDeclaration);
			}
		}

		HVLVConsignment SetupConsignment()
		{
			var org = Factory.New<OrgHeader>();
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = "AU";

			var destinationDepot = Factory.New<OrgAddress>();
			destinationDepot.OA_RN_NKCountryCode = "US";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = orgAddress.PK;
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
			return consignment;
		}

		public void TestStandAloneDeclarationReference()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.NewZealand;

			AssertEquals("Should be empty without export stand alone declaration", ZString.Empty, consignment.DeclarationReferenceForDisplay);

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_DeclarationReference = "Test Export Ref";
			consignment.HVC_JE_ExportDeclaration = declaration1.PK;
			Factory.Save();

			AssertEquals("Should be equal to JE_DeclarationReference after export stand alone declaration is set", "Test Export Ref", consignment.DeclarationReferenceForDisplay);

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_DeclarationReference = "Test Import Ref";
			consignment.HVC_JE_ImportDeclaration = declaration2.PK;
			Factory.Save();

			AssertEquals("Should display 2 references after import stand alone declaration is set", "Test Export Ref,Test Import Ref", consignment.DeclarationReferenceForDisplay);
		}

		public void TestStandAloneDeclarationReference_RefreshAfterHVC_JE_DeclarationChangeFromAnotherFactory()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.NewZealand;
			Factory.Save();

			AssertEquals("Should be empty without export stand alone declaration", ZString.Empty, consignment.DeclarationReferenceForDisplay);

			var anotherFactory = new BusinessObjectFactory();
			var consignmentInAnotherFactory = anotherFactory.Load<HVLVConsignment>(consignment.PK);

			var declaration1 = anotherFactory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_DeclarationReference = "Test Export Ref";
			consignmentInAnotherFactory.HVC_JE_ExportDeclaration = declaration1.PK;
			anotherFactory.Save();

			AssertEquals("Declaration reference should refresh when changes been made in another factory", "Test Export Ref", consignment.DeclarationReferenceForDisplay);

			var declaration2 = anotherFactory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_DeclarationReference = "Test Import Ref";
			consignmentInAnotherFactory.HVC_JE_ImportDeclaration = declaration2.PK;
			anotherFactory.Save();

			AssertEquals("Declaration reference should refresh when changes been made in another factory", "Test Export Ref,Test Import Ref", consignment.DeclarationReferenceForDisplay);
		}

		public void TestCustomsReferenceNumberAddedWhenStandAloneDeclarationIsSaved_Import()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_DeclarationReference = "LTTSTORE";
				consignment.HVC_JE_ImportDeclaration = declaration.PK;

				var customsReferenceNumber = consignment.CustomsReferenceNumbers.GetAllReferenceNumbersByType(CustomsAdditionalReferenceNumbersCodes.DeclarationReference).SingleOrDefault();
				AssertNullOrEmpty("Precondition: there is no Customs reference number before declaration is saved.", customsReferenceNumber);

				Factory.Save();

				customsReferenceNumber = consignment.CustomsReferenceNumbers.GetAllReferenceNumbersByType(CustomsAdditionalReferenceNumbersCodes.DeclarationReference).SingleOrDefault();
				AssertEquals("The Customs reference number is equal to the Declaration reference.", "LTTSTORE", customsReferenceNumber);
			}
		}

		public void TestCustomsReferenceNumberAddedWhenStandAloneDeclarationIsSaved_Export()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.UnitedStates;

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_DeclarationReference = "LTTSTORE";
				consignment.HVC_JE_ExportDeclaration = declaration.PK;

				var customsReferenceNumber = consignment.CustomsReferenceNumbers.GetAllReferenceNumbersByType(CustomsAdditionalReferenceNumbersCodes.DeclarationReference).SingleOrDefault();
				AssertNullOrEmpty("Precondition: there is no Customs reference number before declaration is saved.", customsReferenceNumber);

				Factory.Save();

				customsReferenceNumber = consignment.CustomsReferenceNumbers.GetAllReferenceNumbersByType(CustomsAdditionalReferenceNumbersCodes.DeclarationReference).SingleOrDefault();
				AssertEquals("The Customs reference number is equal to the Declaration reference.", "LTTSTORE", customsReferenceNumber);
			}
		}

		public void TestFirstSetDeclaration_ForExportShipment_CreatesTCELog()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.UnitedStates;

				var initialDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				consignment.HVC_JE_ExportDeclaration = initialDeclaration.PK;

				var exportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsExportsDec.Code);
				AssertEquals("Log is not added before save", 0, exportLogs.Count());

				Factory.Save();
				exportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsExportsDec.Code);
				AssertEquals("After save, TCE log is added", 1, exportLogs.Count());

				var firstLog = exportLogs.Single();
				AssertEquals("Log references created declaration", $"Transfer to Customs Exports Dec., Reference No. {initialDeclaration.JE_DeclarationReference}", firstLog.DisplayEventReference);

				var newDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				consignment.HVC_JE_ExportDeclaration = newDeclaration.PK;

				Factory.Save();
				exportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsExportsDec.Code);
				AssertEquals("Did not add new log - still only 1 log created", 1, exportLogs.Count());

				var secondLog = exportLogs.Single();
				AssertEquals("Log still refers to initial declaration", $"Transfer to Customs Exports Dec., Reference No. {initialDeclaration.JE_DeclarationReference}", secondLog.DisplayEventReference);
			}
		}

		public void TestFirstSetDeclaration_ForImportShipment_CreatesTCILog()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Australia))
			{
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

				var initialDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				consignment.HVC_JE_ImportDeclaration = initialDeclaration.PK;

				var importLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDec.Code);
				AssertEquals("Log is not added before save", 0, importLogs.Count());

				Factory.Save();
				importLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDec.Code);
				AssertEquals("After save, TCI log is added", 1, importLogs.Count());

				var firstLog = importLogs.Single();
				AssertEquals("Log references created declaration", $"Transfer to Customs Imports Dec., Reference No. {initialDeclaration.JE_DeclarationReference}", firstLog.DisplayEventReference);

				var newDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				consignment.HVC_JE_ImportDeclaration = newDeclaration.PK;

				Factory.Save();
				importLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDec.Code);
				AssertEquals("Did not add new log - still only 1 log created", 1, importLogs.Count());

				var secondLog = importLogs.Single();
				AssertEquals("Log still refers to initial declaration", $"Transfer to Customs Imports Dec., Reference No. {initialDeclaration.JE_DeclarationReference}", secondLog.DisplayEventReference);
			}
		}

		public void TestSetExistedDeclaration_ForExportShipment_CreatesTCELog()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.UnitedStates))
			{
				var existedDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				Factory.Save();

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

				consignment.HVC_JE_ExportDeclaration = existedDeclaration.PK;

				var exportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsExportsDec.Code);
				AssertEquals("Log is not added before save", 0, exportLogs.Count());

				Factory.Save();
				exportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsExportsDec.Code);
				AssertEquals("After save, TCE log is added", 1, exportLogs.Count());

				var firstLog = exportLogs.Single();
				AssertEquals("Log references created declaration", $"Transfer to Customs Exports Dec., Reference No. {existedDeclaration.JE_DeclarationReference}", firstLog.DisplayEventReference);
			}
		}

		public void TestSetExistedDeclaration_ForImportShipment_CreatesTCILog()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.UnitedStates))
			{
				var existedDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				Factory.Save();

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = CountryCodes.Australia;
				consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.UnitedStates;

				consignment.HVC_JE_ImportDeclaration = existedDeclaration.PK;

				var exportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDec.Code);
				AssertEquals("Log is not added before save", 0, exportLogs.Count());

				Factory.Save();
				exportLogs = consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDec.Code);
				AssertEquals("After save, TCI log is added", 1, exportLogs.Count());

				var firstLog = exportLogs.Single();
				AssertEquals("Log references created declaration", $"Transfer to Customs Imports Dec., Reference No. {existedDeclaration.JE_DeclarationReference}", firstLog.DisplayEventReference);
			}
		}

		public void TestTCIEventReference_WhenNoREFOrRFNSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec);
			AssertEquals("Transfer to Customs Imports Dec.", log.DisplayEventReference);
		}

		public void TestTCIEventReference_WhenREFIsSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, "TESTREF");
			AssertEquals("Transfer to Customs Imports Dec., TESTREF", log.DisplayEventReference);
		}

		public void TestTCIEventReference_WhenRFNIsSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var referenceNumber = new KeyValuePair<string, string>(EventReferenceConstants.EventReferenceParameters.Codes.ReferenceNumber, "RFN123");
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, referenceNumber);
			AssertEquals("Transfer to Customs Imports Dec., Reference No. RFN123", log.DisplayEventReference);
		}

		public void TestTCIEventReference_WhenREFAndRFNAreSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var referenceNumber = new KeyValuePair<string, string>(EventReferenceConstants.EventReferenceParameters.Codes.ReferenceNumber, "RFN123");
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, "TESTREF", referenceNumber);
			AssertEquals("Transfer to Customs Imports Dec., TESTREF, Reference No. RFN123", log.DisplayEventReference);
		}

		public void TestTCIEventReference_DoesNotShowOtherEventParams()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var otherParam = new KeyValuePair<string, string>(EventReferenceConstants.EventReferenceParameters.Codes.RequestNumber, "XXXX");
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, otherParam);
			AssertEquals("Transfer to Customs Imports Dec.", log.DisplayEventReference);
		}

		public void TestTCEEventReference_WhenNoREFOrRFNSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsExportsDec);
			AssertEquals("Transfer to Customs Exports Dec.", log.DisplayEventReference);
		}

		public void TestTCEEventReference_WhenREFIsSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsExportsDec, "TESTREF");
			AssertEquals("Transfer to Customs Exports Dec., TESTREF", log.DisplayEventReference);
		}

		public void TestTCEEventReference_WhenRFNIsSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var referenceNumber = new KeyValuePair<string, string>(EventReferenceConstants.EventReferenceParameters.Codes.ReferenceNumber, "RFN123");
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsExportsDec, referenceNumber);
			AssertEquals("Transfer to Customs Exports Dec., Reference No. RFN123", log.DisplayEventReference);
		}

		public void TestTCEEventReference_WhenREFAndRFNAreSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var referenceNumber = new KeyValuePair<string, string>(EventReferenceConstants.EventReferenceParameters.Codes.ReferenceNumber, "RFN123");
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsExportsDec, "TESTREF", referenceNumber);
			AssertEquals("Transfer to Customs Exports Dec., TESTREF, Reference No. RFN123", log.DisplayEventReference);
		}

		public void TestTCEEventReference_DoesNotShowOtherEventParams()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var otherParam = new KeyValuePair<string, string>(EventReferenceConstants.EventReferenceParameters.Codes.RequestNumber, "XXXX");
			var log = consignment.Logs.AddNew(AutoEvents.TransferToCustomsExportsDec, otherParam);
			AssertEquals("Transfer to Customs Exports Dec.", log.DisplayEventReference);
		}

		public void TestHLREventReference_WhenNoRESOrREFSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var log = consignment.Logs.AddNew(AutoEvents.HVLVReady);
			AssertEquals("HVLV ", log.DisplayEventReference);
		}

		public void TestHLREventReference_WhenRESIsSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var reason = new KeyValuePair<string, string>(EventReferenceConstants.EventReferenceParameters.Codes.Reason, "REASON1");
			var log = consignment.Logs.AddNew(AutoEvents.HVLVReady, reason);
			AssertEquals("HVLV REASON1", log.DisplayEventReference);
		}

		public void TestHLREventReference_WhenREFIsSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var log = consignment.Logs.AddNew(AutoEvents.HVLVReady, "REFTEST");
			AssertEquals("HVLV , REFTEST", log.DisplayEventReference);
		}

		public void TestHLREventReference_WhenRESAndREFAreSet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var reason = new KeyValuePair<string, string>(EventReferenceConstants.EventReferenceParameters.Codes.Reason, "REASON1");
			var log = consignment.Logs.AddNew(AutoEvents.HVLVReady, "REFTEST", reason);
			AssertEquals("HVLV REASON1, REFTEST", log.DisplayEventReference);
		}

		public void TestIEManifestLineMembers()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WaybillNumber = "WAYBIL123";
			consignment.HVC_ItemCount = 222;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.NewZealand;
			consignment.HVC_ShipperName = "IOWNGOODS";
			consignment.HVC_GoodsDescription = "SOME GOOD GOODS";

			var eManifestLine = consignment as IEManifestLine;

			CombineAssertions(() =>
			{
				AssertEquals("PK", eManifestLine.PK, consignment.PK);
				AssertEquals("Reference", eManifestLine.Reference, consignment.HVC_WaybillNumber);
				AssertEquals("PackCount", eManifestLine.PackCount, consignment.HVC_ItemCount);
				AssertEquals("CountryOfDestination", eManifestLine.CountryOfDestination, consignment.HVC_RN_NKConsigneeCountryCode);
				AssertEquals("GoodsOwner", eManifestLine.GoodsOwner, consignment.HVC_ShipperName);
				AssertEquals("GoodsDescription", eManifestLine.GoodsDescription, consignment.HVC_GoodsDescription);
			});
		}

		public void TestAddNewCustomsReferenceNumber()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			Factory.Save();

			var reference = consignment.CustomsReferenceNumbers.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Parent ID of new Customs reference is PK of consignment", consignment.PK, reference.CE_ParentID);
				AssertEquals("Parent table of new Customs reference is HVLVConsignment", "HVLVConsignment", reference.CE_ParentTable);
			});
		}

		public void TestSetLastUsageCodeForAllItems()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			var testUsageCode = "ACA";
			consignment.SetLastUsageCodeForAllItems(testUsageCode);
			Assert(consignment.Items.Cast<HVLVItem>().All(x => x.HVI_LastUsageCode == testUsageCode));
		}

		#region Save

		public void TestHVLVConsignmentShipperReferenceUniqueIndexFailureHandlerWhenDuplicates()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

				var consignment2 = header.Consignments.AddNew();
				consignment2.HVC_WaybillNumber = "CONS2";
				consignment2.HVC_ShipperReference = "DUPLICATE";

				var consignment3 = header.Consignments.AddNew();
				consignment3.HVC_WaybillNumber = "CONS3";
				consignment3.Items.AddNew().HVI_ShipperReference = "NONDUPLICATE";

				var consignment5 = header.Consignments.AddNew();
				consignment5.HVC_WaybillNumber = "CONS5";
				consignment5.HVC_ShipperReference = "DUPLICATE";

				var consignment6 = header.Consignments.AddNew();
				consignment6.HVC_WaybillNumber = "CONS6";
				consignment6.HVC_ShipperReference = "NOTADUPLICATE";

				try
				{
					Factory.Save();
					Fail("Should cause UniqueIndexFailure");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				const string expectedError = "Error saving record. Consignment Shipper Reference must be unique on the Shipment or Booking Header. The duplicate value is (DUPLICATE).";

				AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				Assert(!header.IsInDatabase);
				AssertIDs(nameof(consignment2), consignment2, ZString.Empty, "CONS2", "DUPLICATE");
				AssertIDs(nameof(consignment3), consignment3, ZString.Empty, "CONS3", ZString.Empty, ItemReferences.New("NONDUPLICATE", "NONDUPLICATE"));
				AssertIDs(nameof(consignment5), consignment5, ZString.Empty, "CONS5", "DUPLICATE");
				AssertIDs(nameof(consignment6), consignment6, ZString.Empty, "CONS6", "NOTADUPLICATE");
			}
		}

		public void TestHVLVConsignmentWaybillNumberUniqueIndexFailureHandlerWhenDuplicates()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

				var consignment2 = header.Consignments.AddNew();
				consignment2.HVC_WaybillNumber = "DUPLICATE";

				var consignment5 = header.Consignments.AddNew();
				consignment5.HVC_WaybillNumber = "DUPLICATE";

				var consignment6 = header.Consignments.AddNew();
				consignment6.HVC_WaybillNumber = "NOTADUPLICATE";

				try
				{
					Factory.Save();
					Fail("Should cause UniqueIndexFailure");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				const string expectedError = "Error saving record. Consignment Waybill Number must be unique on the Shipment or Booking Header. The duplicate value is (DUPLICATE).";

				AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				Assert(!header.IsInDatabase);
				AssertIDs(nameof(consignment2), consignment2, ZString.Empty, "DUPLICATE", ZString.Empty);
				AssertIDs(nameof(consignment5), consignment5, ZString.Empty, "DUPLICATE", ZString.Empty);
				AssertIDs(nameof(consignment6), consignment6, ZString.Empty, "NOTADUPLICATE", ZString.Empty);
			}
		}

		public void TestSavingWithoutBookingHeader()
		{
			var hvlShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = hvlShipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_HCH_Header = header.PK;
			consignment.HVC_ClusterKey = header.HCH_ClusterKey = 1;

			var item = consignment.Items.AddNew();
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			AssertEquals("Precondition: Consignment shouldn't have a HVLVBookingHeader", ZGuid.Empty, consignment.HVC_HVH_BookingHeader);
			AssertNull("Precondition: Consignment shouldn't have a HVLVBookingHeader", consignment.BookingHeader);

			AssertNoExceptionThrown(() => { Factory.Save(); });
		}

		public void TestMappingEmptyHVI_JS_LoadedOnShipment()
		{
			var hvlShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = hvlShipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = Factory.New<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			var originalPk = Guid.NewGuid();
			item2.HVI_JS_LoadedOnShipment = originalPk = Guid.NewGuid();
			consignment.HVC_HCH_Header = header.PK;

			AssertEquals("item1 HVI_JS_LoadedOnShipment should be updated", header.HCH_JS_Shipment, item1.HVI_JS_LoadedOnShipment);
			AssertEquals("item2 HVI_JS_LoadedOnShipment should not be updated", originalPk, item2.HVI_JS_LoadedOnShipment);
		}

		public void TestIdSettingOnSaving()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions(() =>
				{
					var consignment1 = CreateConsignment(ZString.Empty, ZString.Empty, ItemReferences.New(string.Empty, string.Empty));
					Factory.Save();
					AssertIDs(nameof(consignment1), consignment1, "HVC000000000000001", "HVC000000000000001", ZString.Empty, ItemReferences.New("HVC000000000000001", "", "HVC000000000000001"));

					var consignment2 = CreateConsignment(ZString.Empty, ZString.Empty, ItemReferences.New("ITEMSHP", string.Empty));
					Factory.Save();
					AssertIDs(nameof(consignment2), consignment2, "ITEMSHP", "ITEMSHP", ZString.Empty, ItemReferences.New("ITEMSHP", "ITEMSHP", "ITEMSHP"));

					var consignment3 = CreateConsignment(ZString.Empty, ZString.Empty, ItemReferences.New(string.Empty, "ITEMBARCODE"));
					Factory.Save();
					AssertIDs(nameof(consignment3), consignment3, "ITEMBARCODE", "ITEMBARCODE", ZString.Empty, ItemReferences.New("ITEMBARCODE", "", "ITEMBARCODE"));

					var consignment4 = CreateConsignment(ZString.Empty, ZString.Empty, ItemReferences.New("ITEMSHP", "ITEMBARCODE"));
					Factory.Save();
					AssertIDs(nameof(consignment4), consignment4, "ITEMBARCODE", "ITEMBARCODE", ZString.Empty, ItemReferences.New("ITEMBARCODE", "ITEMSHP", "ITEMBARCODE"));

					var consignment5 = CreateConsignment(ZString.Empty, "CONSIGNSHP", ItemReferences.New(string.Empty, string.Empty));
					Factory.Save();
					AssertIDs(nameof(consignment5), consignment5, "CONSIGNSHP", "CONSIGNSHP", "CONSIGNSHP", ItemReferences.New("CONSIGNSHP", "", "CONSIGNSHP"));

					var consignment6 = CreateConsignment(ZString.Empty, "CONSIGNSHP", ItemReferences.New("ITEMSHP", string.Empty));
					Factory.Save();
					AssertIDs(nameof(consignment6), consignment6, "CONSIGNSHP", "CONSIGNSHP", "CONSIGNSHP", ItemReferences.New("ITEMSHP", "ITEMSHP", "ITEMSHP"));

					var consignment7 = CreateConsignment(ZString.Empty, "CONSIGNSHP", ItemReferences.New(string.Empty, "ITEMBARCODE"));
					Factory.Save();
					AssertIDs(nameof(consignment7), consignment7, "CONSIGNSHP", "CONSIGNSHP", "CONSIGNSHP", ItemReferences.New("ITEMBARCODE", "", "ITEMBARCODE"));

					var consignment8 = CreateConsignment(ZString.Empty, "CONSIGNSHP", ItemReferences.New("ITEMSHP", "ITEMBARCODE"));
					Factory.Save();
					AssertIDs(nameof(consignment8), consignment8, "CONSIGNSHP", "CONSIGNSHP", "CONSIGNSHP", ItemReferences.New("ITEMBARCODE", "ITEMSHP", "ITEMBARCODE"));

					var consignment9 = CreateConsignment("CONSIGNWAYBILL", ZString.Empty, ItemReferences.New(string.Empty, string.Empty));
					Factory.Save();
					AssertIDs(nameof(consignment9), consignment9, "CONSIGNWAYBILL", "CONSIGNWAYBILL", ZString.Empty, ItemReferences.New("CONSIGNWAYBILL", "", "CONSIGNWAYBILL"));

					var consignment10 = CreateConsignment("CONSIGNWAYBILL", ZString.Empty, ItemReferences.New("ITEMSHP", string.Empty));
					Factory.Save();
					AssertIDs(nameof(consignment10), consignment10, "CONSIGNWAYBILL", "CONSIGNWAYBILL", ZString.Empty, ItemReferences.New("ITEMSHP", "ITEMSHP", "ITEMSHP"));

					var consignment11 = CreateConsignment("CONSIGNWAYBILL", ZString.Empty, ItemReferences.New(string.Empty, "ITEMBARCODE"));
					Factory.Save();
					AssertIDs(nameof(consignment11), consignment11, "CONSIGNWAYBILL", "CONSIGNWAYBILL", ZString.Empty, ItemReferences.New("ITEMBARCODE", "", "ITEMBARCODE"));

					var consignment12 = CreateConsignment("CONSIGNWAYBILL", ZString.Empty, ItemReferences.New("ITEMSHP", "ITEMBARCODE"));
					Factory.Save();
					AssertIDs(nameof(consignment12), consignment12, "CONSIGNWAYBILL", "CONSIGNWAYBILL", ZString.Empty, ItemReferences.New("ITEMBARCODE", "ITEMSHP", "ITEMBARCODE"));

					var consignment13 = CreateConsignment("CONSIGNWAYBILL", "CONSIGNSHP", ItemReferences.New(string.Empty, string.Empty));
					Factory.Save();
					AssertIDs(nameof(consignment13), consignment13, "CONSIGNWAYBILL", "CONSIGNWAYBILL", "CONSIGNSHP", ItemReferences.New("CONSIGNWAYBILL", "", "CONSIGNWAYBILL"));

					var consignment14 = CreateConsignment("CONSIGNWAYBILL", "CONSIGNSHP", ItemReferences.New("ITEMSHP", string.Empty));
					Factory.Save();
					AssertIDs(nameof(consignment14), consignment14, "CONSIGNWAYBILL", "CONSIGNWAYBILL", "CONSIGNSHP", ItemReferences.New("ITEMSHP", "ITEMSHP", "ITEMSHP"));

					var consignment15 = CreateConsignment("CONSIGNWAYBILL", "CONSIGNSHP", ItemReferences.New(string.Empty, "ITEMBARCODE"));
					Factory.Save();
					AssertIDs(nameof(consignment15), consignment15, "CONSIGNWAYBILL", "CONSIGNWAYBILL", "CONSIGNSHP", ItemReferences.New("ITEMBARCODE", "", "ITEMBARCODE"));

					var consignment16 = CreateConsignment("CONSIGNWAYBILL", "CONSIGNSHP", ItemReferences.New("ITEMSHP", "ITEMBARCODE"));
					Factory.Save();
					AssertIDs(nameof(consignment16), consignment16, "CONSIGNWAYBILL", "CONSIGNWAYBILL", "CONSIGNSHP", ItemReferences.New("ITEMBARCODE", "ITEMSHP", "ITEMBARCODE"));

					var consignment17 = CreateConsignment(ZString.Empty, ZString.Empty, ItemReferences.New(string.Empty, string.Empty), ItemReferences.New(string.Empty, string.Empty));
					Factory.Save();
					AssertIDs(nameof(consignment17), consignment17, "HVC000000000000002", "HVC000000000000002", ZString.Empty, ItemReferences.New("HVC000000000000002", "", "HVC000000000000002"), ItemReferences.New("HVI000000000000001", "", "HVI000000000000001"));
				});
			}
		}

		[UseSnapshotProtection(true)]
		public void TestIdSettingOnSaving_NumberFountains()
		{
			var factory = new BusinessObjectFactory();
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingHeader1 = factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment1 = bookingHeader1.Consignments.AddNew();
				consignment1.Items.AddNew();
				consignment1.Items.AddNew();
				factory.Save();
				AssertIDs(nameof(consignment1), consignment1, "HVC000000000000001", "HVC000000000000001", ZString.Empty, ItemReferences.New("HVC000000000000001", "", "HVC000000000000001"), ItemReferences.New("HVI000000000000001", "", "HVI000000000000001"));

				HVLVTestHelper.SetGS1FountainOnOrgProxy(factory, "1234567");

				var bookingHeader2 = factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment2 = bookingHeader2.Consignments.AddNew();
				consignment2.Items.AddNew();
				consignment2.Items.AddNew();
				factory.Save();
				AssertIDs(nameof(consignment2), consignment2, "012345670000000015", "012345670000000015", ZString.Empty, ItemReferences.New("012345670000000015", "", "012345670000000015"), ItemReferences.New("012345670000000022", "", "012345670000000022"));

				var testShipper = factory.NewWithValidTestData<OrgHeader>();
				HVLVTestHelper.SetGS1FountainOnOrg(testShipper, "9999999");

				var bookingHeader3 = factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment3 = bookingHeader3.Consignments.AddNew();
				consignment3.BookingHeader.HVH_OA_BillToParty = testShipper.MainAddress.PK;
				consignment3.Items.AddNew();
				consignment3.Items.AddNew();
				factory.Save();
				AssertIDs(nameof(consignment3), consignment3, "099999990000000010", "099999990000000010", ZString.Empty, ItemReferences.New("099999990000000010", "", "099999990000000010"), ItemReferences.New("099999990000000027", "", "099999990000000027"));
			}
		}

		[UseSnapshotProtection(true)]
		public void TestIdSettingOnSaving_NumberFountains_WithoutBookingHeader()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "Consignor";
			HVLVTestHelper.SetGS1FountainOnOrg(consignor, "9999999");

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.ConsignorPK = consignor.PK;
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			consignment.Items.AddNew();
			Factory.Save();

			AssertIDs(nameof(consignment), consignment, "099999990000000010", "099999990000000010", ZString.Empty, ItemReferences.New("099999990000000027", "", "099999990000000027"));
		}

		public void TestDataCalculatedByTriggersDoesNotSave()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_WeightUQ = "KG";
			consignment.HVC_VolumeUQ = "M3";
			consignment.HVC_ConsignmentId = "CONSIGN1";

			Action<ZDecimal, ZDecimal> addItem = (weight, volume) =>
			{
				var item = consignment.Items.AddNew();
				item.HVI_ManifestedWeight = weight;
				item.HVI_ManifestedVolume = volume;
				item.HVI_ItemId = "Item" + consignment.Items.Count;
			};

			addItem(5.5, 6.6);
			addItem(10.2, 11.3);
			addItem(100, 200);
			Factory.Save();

			AssertEquals((ZShort)3, consignment.HVC_ItemCount);
			AssertEquals((ZDecimal)115.7, consignment.HVC_ManifestedWeight);
			AssertEquals((ZDecimal)217.9, consignment.HVC_ManifestedVolume);

			consignment.HVC_ItemCount = 10;
			consignment.HVC_ManifestedWeight = 100;
			consignment.HVC_ManifestedVolume = 200;
			consignment.HVC_SystemLastEditTimeUtc = ZDateTime.Now;

			AssertEquals((ZShort)10, consignment.HVC_ItemCount);
			AssertEquals((ZDecimal)100, consignment.HVC_ManifestedWeight);
			AssertEquals((ZDecimal)200, consignment.HVC_ManifestedVolume);
			Factory.Save();

			AssertEquals((ZShort)3, consignment.HVC_ItemCount);
			AssertEquals((ZDecimal)115.7, consignment.HVC_ManifestedWeight);
			AssertEquals((ZDecimal)217.9, consignment.HVC_ManifestedVolume);

			var newFactory = Factory.CreateNewFactory();
			var consignment2 = newFactory.Load<HVLVConsignment>(consignment.PK);

			AssertEquals((ZShort)3, consignment2.HVC_ItemCount);
			AssertEquals((ZDecimal)115.7, consignment2.HVC_ManifestedWeight);
			AssertEquals((ZDecimal)217.9, consignment2.HVC_ManifestedVolume);
		}

		public void TestHVLVConsigment_TransactionParticipantDBHits() // This fails with new validation things
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CSN_01";
			consignment1.HVC_ShipperReference = "SHPREF_01";
			consignment1.HVC_ItemCount = 1;

			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CSN_02";
			consignment2.HVC_ShipperReference = "SHPREF_02";
			consignment2.HVC_ItemCount = 2;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var headerInNewFactory = newFactory.Load<HVLVBookingHeader>(header.PK);

			//Precondition
			AssertDbHits(new Dictionary<string, int>
			{
				{ HVLVBookingHeaderSchema.Constants.TableName, 1 },
				{ HVLVConsignmentSchema.Constants.TableName, 0 }
			}, newFactory);
			newFactory.ResetDatabaseLoadCount();

			var participant = newFactory.SaveInTransactionActions.OfType<HVLVConsignmentTransactionParticipant>().ToList();
			AssertEquals("PreCondition: Should not have any transaction participant registered", 0, participant.Count);

			headerInNewFactory.Consignments[0].HVC_ItemCount = 11;

			participant = newFactory.SaveInTransactionActions.OfType<HVLVConsignmentTransactionParticipant>().ToList();
			AssertEquals("Consignment has changed, transaction participant should have registered", 1, participant.Count);

			//Consignments are loaded. HVLVConsignment DBHits count should increased
			AssertDbHits(new Dictionary<string, int>
			{
				{ HVLVConsignmentSchema.Constants.TableName, 1 }
			}, newFactory);

			newFactory.Save();

			//After saved, ReloadAllSafe is called, HVLVConsignment DBHits count should increase 1
			AssertDbHits(new Dictionary<string, int>
			{
				{ HVLVConsignmentSchema.Constants.TableName, 2 }
			}, newFactory);
		}

		public void TestUsageLoggedWhenConsignmentUsedByOtherCompany()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "IEC";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "IE";
			branch.GB_GC = company.PK;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TUS";
			staff.GS_LoginName = "test user";
			staff.GS_GB_LastLogonBranch = branch.PK;
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_SystemCreateUser = staff.GS_Code;

			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			Factory.Save();

			AssertEquals("TUS", consignment.HVC_SystemCreateUser);

			var currentUser = Factory.New<GlbStaff>();
			currentUser.GS_Code = "XX";
			currentUser.GS_LoginName = "xx test";
			Factory.Save();

			using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(new UserContext("xx test", Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				consignment.HVC_GoodsDescription = "goods description";
				Factory.Save();

				AssertEquals("XX", consignment.HVC_SystemLastEditUser);
				AssertEquals("2 new records have been created (one for each item) since editing company is different from creating company", 2, HVLVTestHelper.GetRecordCountFromHXUTable());
				CombineAssertions(() =>
				{
					AssertEquals("The user in HVLVUsage table should record the current login user", currentUser.GS_Code, HVLVTestHelper.GetUserFromHXUTable());
					AssertEquals("The usageCode in HVLVUsage table should be CWE", HVLVConstants.UsageCodes.CargoWiseUsageByOtherCompany, HVLVTestHelper.GetUsageCodeFromHXUTable());
					Assert("There should be one row for item1", HVLVTestHelper.ExistsUsageCodeWithItemPK(item1.PK.ToString()));
					Assert("There should be one row for item2", HVLVTestHelper.ExistsUsageCodeWithItemPK(item2.PK.ToString()));
				});
			}
		}

		public void TestNoUsageLoggedWhenConsignmentUsedByTheSameCompany()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "IEC";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "IE";
			branch.GB_GC = company.PK;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TUS";
			staff.GS_LoginName = "test user";
			staff.GS_GB_LastLogonBranch = branch.PK;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_SystemCreateUser = staff.GS_Code;
			consignment.Items.AddNew();
			consignment.Items.AddNew();
			Factory.Save();

			AssertEquals("TUS", consignment.HVC_SystemCreateUser);

			var anotherStaffInTheSameCompany = Factory.New<GlbStaff>();
			anotherStaffInTheSameCompany.GS_Code = "XX";
			anotherStaffInTheSameCompany.GS_LoginName = "xx user";
			anotherStaffInTheSameCompany.GS_GB_LastLogonBranch = branch.PK;
			Factory.Save();

			using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(new UserContext("xx user", branch.PK.ToGuid(), Env.CurrentDepartmentPK)))
			{
				consignment.HVC_GoodsDescription = "goods description";
				Factory.Save();

				AssertEquals("XX", consignment.HVC_SystemLastEditUser);
				AssertEquals("No new records created as both staff are from same company", 0, HVLVTestHelper.GetRecordCountFromHXUTable());
			}
		}

		public void TestNoUsageLoggedWhenCreateUserIsInvalidUser()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.Items.AddNew();
			consignment.Items.AddNew();

			var editUser = Factory.New<GlbStaff>();
			editUser.GS_Code = "XX";
			editUser.GS_LoginName = "xx user";

			AssertUsageLogWithInvalidCreateUser(User.ServiceUserCode);
			AssertUsageLogWithInvalidCreateUser(User.SupportUserCode);
			AssertUsageLogWithInvalidCreateUser(User.UnKnownUserCode);
			AssertUsageLogWithInvalidCreateUser(User.InterchangeUserCode);
			AssertUsageLogWithInvalidCreateUser(User.WebUserCode);

			void AssertUsageLogWithInvalidCreateUser(string userCode)
			{
				consignment.HVC_SystemCreateUser = userCode;
				Factory.Save();

				using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (Env.SetTemporaryUserContext(new UserContext("xx user", Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					consignment.HVC_GoodsDescription = "goods description";
					Factory.Save();

					AssertEquals("No new records created as the created user is a invalid user", 0, HVLVTestHelper.GetRecordCountFromHXUTable());
				}
			}
		}

		public void TestNoUsageLoggedWhenEditUserIsInvalidUser()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "IEC";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "IE";
			branch.GB_GC = company.PK;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TUS";
			staff.GS_LoginName = "test user";
			staff.GS_GB_LastLogonBranch = branch.PK;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_SystemCreateUser = staff.GS_Code;
			consignment.Items.AddNew();
			consignment.Items.AddNew();
			Factory.Save();

			AssertUsageLogWithInvalidEditUser(User.ServiceUserCode);
			AssertUsageLogWithInvalidEditUser(User.SupportUserCode);
			AssertUsageLogWithInvalidEditUser(User.UnKnownUserCode);
			AssertUsageLogWithInvalidEditUser(User.InterchangeUserCode);
			AssertUsageLogWithInvalidEditUser(User.WebUserCode);

			void AssertUsageLogWithInvalidEditUser(string userCode)
			{
				var editUserPK = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, userCode)).PK;
				using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (Env.SetTemporaryUserContext(new UserContext(editUserPK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					consignment.HVC_GoodsDescription = "goods description";
					Factory.Save();

					AssertEquals("No new records created as the edit user is a invalid user", 0, HVLVTestHelper.GetRecordCountFromHXUTable());
				}
			}
		}

		public void TestUsageLoggedSuccessfullyForNewCreatedItem()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "IEC";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "IE";
			branch.GB_GC = company.PK;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TUS";
			staff.GS_LoginName = "test user";
			staff.GS_GB_LastLogonBranch = branch.PK;
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_SystemCreateUser = staff.GS_Code;

			var item1 = consignment.Items.AddNew();
			Factory.Save();

			AssertEquals("TUS", consignment.HVC_SystemCreateUser);

			var currentUser = Factory.New<GlbStaff>();
			currentUser.GS_Code = "XX";
			currentUser.GS_LoginName = "xx test";
			Factory.Save();

			using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Env.SetTemporaryUserContext(new UserContext("xx test", Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				consignment.HVC_GoodsDescription = "goods description";
				var item2 = consignment.Items.AddNew();
				Factory.Save();

				AssertEquals("XX", consignment.HVC_SystemLastEditUser);
				AssertEquals("2 new records have been created (one for each item) since editing company is different from creating company", 2, HVLVTestHelper.GetRecordCountFromHXUTable());
				CombineAssertions(() =>
				{
					AssertEquals("The user in HVLVUsage table should record the current login user", currentUser.GS_Code, HVLVTestHelper.GetUserFromHXUTable());
					AssertEquals("The usageCode in HVLVUsage table should be CWE", HVLVConstants.UsageCodes.CargoWiseUsageByOtherCompany, HVLVTestHelper.GetUsageCodeFromHXUTable());
					Assert("There should be one row for item1", HVLVTestHelper.ExistsUsageCodeWithItemPK(item1.PK.ToString()));
					Assert("There should be one row for item2", HVLVTestHelper.ExistsUsageCodeWithItemPK(item2.PK.ToString()));
				});
			}
		}

		#endregion

		#region Delete/Deactivate

		public void TestSavedInactiveConsignmentHasReadOnlyProperties()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_IsActive = false;
			Factory.Save();

			var propertyInfos = consignment.GetType().GetProperties().Where(p => p.PropertyType == typeof(ZPropertyInfo)).Select(info => (ZPropertyInfo)info.GetValue(consignment));

			bool ExpectReadOnly(ZPropertyInfo propertyInfo) => propertyInfo.Name != nameof(HVLVConsignment.HVC_IsActive);

			CombineAssertions(() =>
			{
				propertyInfos.ForEach(info => AssertEquals($"{info.Name} ReadOnly:", ExpectReadOnly(info), info.ReadOnly));
			});
		}

		public void TestDeletePropagatesToItems()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			consignment.Delete();

			CombineAssertions("All Items should be deleted", () =>
			{
				AssertEquals("Item 1:", true, item1.IsDeleted);
				AssertEquals("Item 2:", true, item2.IsDeleted);
			});
		}

		public void TestDeactivatePropogatesToItems()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			consignment.HVC_IsActive = true;
			item1.HVI_IsActive = true;
			item2.HVI_IsActive = true;

			consignment.HVC_IsActive = false;

			CombineAssertions("All Items should be deactivated", () =>
			{
				AssertEquals("Item 1:", false, item1.HVI_IsActive);
				AssertEquals("Item 2:", false, item2.HVI_IsActive);
			});
		}

		public void TestActivatePropogatesToItems()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			consignment.HVC_IsActive = false;
			item1.HVI_IsActive = false;
			item2.HVI_IsActive = false;

			consignment.HVC_IsActive = true;

			CombineAssertions("All Items should be activated", () =>
			{
				AssertEquals("Item 1:", true, item1.HVI_IsActive);
				AssertEquals("Item 2:", true, item2.HVI_IsActive);
			});
		}

		public void TestCanDelete()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			Factory.Save();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();

			CombineAssertions(() =>
			{
				AssertEquals("Cannot delete Consignment 1", false, ((ICanDelete)consignment1).CanDelete);
				AssertEquals("Can delete Consignment 2", true, ((ICanDelete)consignment2).CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			Factory.Save();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			CombineAssertions(() =>
			{
				AssertEquals("Cannot delete Consignment 1", "Existing consignments cannot be deleted. Mark them as inactive instead.", ((ICanDelete)consignment1).ReasonForNotAbleToDelete);
				AssertEquals("No reason for not able to delete Consignment 2", string.Empty, ((ICanDelete)consignment2).ReasonForNotAbleToDelete);
			});
		}

		#endregion

		#region IncoTerms and payment term displays

		static readonly Dictionary<string, string> IncoTermCodeToPaymentTermDisplayMappings = new Dictionary<string, string>()
		{
			{ IncoTerms.FreeOnBoard, HVLVConsignmentPaymentTermDisplay.FreightCollect },
			{ IncoTerms.FreeCarrier, HVLVConsignmentPaymentTermDisplay.FreightCollect },
			{ IncoTerms.FreeCarrierSeller, HVLVConsignmentPaymentTermDisplay.FreightCollect },
			{ IncoTerms.FreeCarrierBuyer, HVLVConsignmentPaymentTermDisplay.FreightCollect },
			{ IncoTerms.FreeAlongsideShip, HVLVConsignmentPaymentTermDisplay.FreightCollect },
			{ IncoTerms.ExWorks, HVLVConsignmentPaymentTermDisplay.FreightCollect },
			{ IncoTerms.DeliveredDutyUnpaid, HVLVConsignmentPaymentTermDisplay.FreightPrepaid },
			{ IncoTerms.DeliveredDutyPaid, HVLVConsignmentPaymentTermDisplay.FreightPrepaid },
			{ IncoTerms.DeliveredAtTerminal, HVLVConsignmentPaymentTermDisplay.FreightPrepaid },
			{ IncoTerms.DeliveredAtPlace, HVLVConsignmentPaymentTermDisplay.FreightPrepaid },
			{ IncoTerms.CarriagePaidTo, HVLVConsignmentPaymentTermDisplay.FreightPrepaid },
			{ IncoTerms.CarriageAndInsurancePaidTo, HVLVConsignmentPaymentTermDisplay.FreightPrepaid },
			{ IncoTerms.CostInsuranceAndFreight, HVLVConsignmentPaymentTermDisplay.FreightPrepaid },
			{ IncoTerms.CostAndFreight, HVLVConsignmentPaymentTermDisplay.FreightPrepaid },
		};

		public void TestHVC_INCO_Validation()
		{
			var consignment = Factory.New<HVLVConsignment>();
			AssertEquals("Precondition", IncoTerms.FreeOnBoard, consignment.HVC_INCO);

			consignment.HVC_INCO = "XXX";
			Assert(consignment.HVC_INCOInfo.HasErrors());
		}

		public void TestDefaultValues_PaymentTermDisplay()
		{
			var consignment = Factory.New<HVLVConsignment>();
			AssertEquals(HVLVConsignmentPaymentTermDisplay.FreightCollect, consignment.PaymentTermDisplay);
		}

		public void TestConsignmentPaymentTermDisplay_WhenIncoTermChanges_PaymentTermDisplayUpdates()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_INCO = IncoTerms.FreeOnBoard;
			AssertEquals("Precondition: Expecting the payment term display to be free on board for a FOB IncoTerm", consignment.PaymentTermDisplay, IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.FreeOnBoard]);

			consignment.HVC_INCO = IncoTerms.CostAndFreight;
			AssertEquals("The payment term display for the consignment did not update", consignment.PaymentTermDisplay, IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.CostAndFreight]);
		}

		public void TestWhenIncoTermsIsFOB_ThenDisplayTermsSetToFreightCollect()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.FreeOnBoard], IncoTerms.FreeOnBoard);
		}

		public void TestWhenIncoTermsIsFCA_ThenDisplayTermsSetToFreightCollect()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.FreeCarrier], IncoTerms.FreeCarrier);
		}

		public void TestWhenIncoTermsIsFC1_ThenDisplayTermsSetToFreightCollect()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.FreeCarrierSeller], IncoTerms.FreeCarrierSeller);
		}

		public void TestWhenIncoTermsIsFC2_ThenDisplayTermsSetToFreightCollect()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.FreeCarrierBuyer], IncoTerms.FreeCarrierBuyer);
		}

		public void TestWhenIncoTermsIsFAS_ThenDisplayTermsSetToFreightCollect()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.FreeAlongsideShip], IncoTerms.FreeAlongsideShip);
		}

		public void TestWhenIncoTermsIsEXW_ThenDisplayTermsSetToFreightCollect()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.ExWorks], IncoTerms.ExWorks);
		}

		public void TestWhenIncoTermsIsDDU_ThenDisplayTermsSetToFreightPrepaid()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.DeliveredDutyUnpaid], IncoTerms.DeliveredDutyUnpaid);
		}

		public void TestWhenIncoTermsIsDDP_ThenDisplayTermsSetToFreightPrepaid()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.DeliveredDutyPaid], IncoTerms.DeliveredDutyPaid);
		}

		public void TestWhenIncoTermsIsDAT_ThenDisplayTermsSetToFreightPrepaid()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.DeliveredAtTerminal], IncoTerms.DeliveredAtTerminal);
		}

		public void TestWhenIncoTermsIsDAP_ThenDisplayTermsSetToFreightPrepaid()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.DeliveredAtPlace], IncoTerms.DeliveredAtPlace);
		}

		public void TestWhenIncoTermsIsCPT_ThenDisplayTermsSetToFreightPrepaid()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.CarriagePaidTo], IncoTerms.CarriagePaidTo);
		}

		public void TestWhenIncoTermsIsCIP_ThenDisplayTermsSetToFreightPrepaid()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.CarriageAndInsurancePaidTo], IncoTerms.CarriageAndInsurancePaidTo);
		}

		public void TestWhenIncoTermsIsCIF_ThenDisplayTermsSetToFreightPrepaid()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.CostInsuranceAndFreight], IncoTerms.CostInsuranceAndFreight);
		}

		public void TestWhenIncoTermsIsCFR_ThenDisplayTermsSetToFreightPrepaid()
		{
			TestIncoTermCodeMatchesExpectedPaymentTermDisplay(IncoTermCodeToPaymentTermDisplayMappings[IncoTerms.CostAndFreight], IncoTerms.CostAndFreight);
		}

		void TestIncoTermCodeMatchesExpectedPaymentTermDisplay(string expectedPaymentTermDisplay, string incoTermCode)
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_INCO = incoTermCode;
			AssertEquals(expectedPaymentTermDisplay, consignment.PaymentTermDisplay);
		}

		#endregion

		#region Pre-Screening status

		public void TestFailedAcceptedAsEntered_WhenPrescreeningStatusIsFailed_PrescreeningStatusSetToFailAcceptedAsEntered()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			consignment.AcceptFailedPreScreeningAsEntered();

			AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered, consignment.HVC_PreScreeningStatus);
		}

		public void TestFailedAcceptedAsEntered_EventIsLogged()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			consignment.AcceptFailedPreScreeningAsEntered();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdated.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, "|TYP=Pre-Screening|RES=Failed Accepted as Entered");

			AssertEquals(true, consignment.Logs.HasLogWith(query));
		}

		public void TestWhenPrescreeningStatusIsUnknownAndConsignmentIsActive_RescreenButtonVisibilityReturnTrue()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_IsActive = true;
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown;

			AssertEquals(true, consignment.HasUnknownPrescreeningStatus);
		}

		public void TestWhenPrescreeningStatusFailAndConsignmentIsActive_RescreenButtonIsVisibleReturnFalse()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_IsActive = true;
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			AssertEquals(false, consignment.HasUnknownPrescreeningStatus);
		}

		public void TestWhenConsignmentIsInactive_RescreenButtonIsVisibleReturnFalse()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			AssertEquals("pre condition", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);

			consignment.HVC_IsActive = false;

			AssertEquals(false, consignment.HasUnknownPrescreeningStatus);
		}

		public void TestWhenPrescreeningStatusFail_AcceptFailedAsEnteredButtonVisibilityReturnTrue()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			AssertEquals(true, consignment.HasFailedPreScreeningStatus);
		}

		public void TestWhenPrescreeningStatusUnknown_AcceptFailedAsEnteredButtonVisibilityReturnFalse()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown;

			AssertEquals(false, consignment.HasFailedPreScreeningStatus);
		}

		public void TestFailedAcceptedAsEntered_WhenPrescreeningStatusIsPassed_PrescreeningStatusNotChanged()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

			consignment.AcceptFailedPreScreeningAsEntered();

			AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Passed, consignment.HVC_PreScreeningStatus);
		}

		public void TestPreScreeningStatusSetToUnknown_WhenConsignmentFieldIsEdited_NormalProperty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.ManagingShipment = shipment;
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;
			Factory.Save();

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration { IsEnabled = true };

			var rule = preScreeningConfiguration.Rules.AddNew();
			rule.TransportMode = TransportModes.Sea;
			rule.OriginCountryCode = CountryCodes.UnitedStates;
			rule.DestinationCountryCode = CountryCodes.Australia;

			var field = rule.Fields.AddNew();
			var fieldDescriptionList = field.FieldDescriptionList;

			var descriptionsToExclude = new[] {
				HVLVConsignmentSchema.Constants.HVC_GoodsValue,
				HVLVConsignmentSchema.Constants.HVC_GoodsDescription,
				HVLVItemLineSchema.Constants.HVS_OriginTariff,
				HVLVItemLineSchema.Constants.HVS_DestinationTariff,
				fieldDescriptionList.GetDescriptionFromCode("User Defined"),
				fieldDescriptionList.GetDescriptionFromCode("Special Characters"),
				fieldDescriptionList.GetDescriptionFromCode("Total Lines Customs Value"),
				fieldDescriptionList.GetDescriptionFromCode("Total Lines Intrinsic Value")
			};

			CombineAssertions("Edit normal property sets HVC_PreScreeningStatus to UNK", () =>
			{
				foreach (var fieldCode in fieldDescriptionList.GetAllCodes())
				{
					consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;

					field.FieldDescription = fieldCode;
					field.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Error;

					if (!descriptionsToExclude.Contains(fieldDescriptionList.GetDescriptionFromCode(fieldCode)))
					{
						using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
						{
							consignment.GetType().GetProperty(field.FieldName)?.SetValue(consignment, new ZString("test"));
						}

						AssertEquals($"{fieldCode} edit did not set HVC_PreScreeningStatus to UNK", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);
					}
				}
			});
		}

		public void TestPreScreeningStatusSetToUnknown_WhenConsignmentFieldIsEdited_GoodsDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.ManagingShipment = shipment;
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true };

			var rule = preScreeningConfiguration.Rules.AddNew();
			rule.TransportMode = TransportModes.Sea;
			rule.OriginCountryCode = CountryCodes.UnitedStates;
			rule.DestinationCountryCode = CountryCodes.Australia;

			var field = rule.Fields.AddNew();
			var fieldDescriptionList = field.FieldDescriptionList;
			field.FieldDescription = fieldDescriptionList.GetCodeFromDescription(HVLVConsignmentSchema.Constants.HVC_GoodsDescription);
			field.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Error;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			{
				CombineAssertions("Edit Consignment Goods Description sets HVC_PreScreeningStatus to UNK", () =>
				{
					consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
					Factory.Save();
					consignment.HVC_GoodsDescription = "test";
					AssertEquals("Consignment edit did not set HVC_PreScreeningStatus to UNK", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);

					consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
					var item = consignment.Items.AddNew();
					Factory.Save();
					item.HVI_GoodsDescription = "test";
					AssertEquals("Item edit did not set HVC_PreScreeningStatus to UNK", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);

					consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
					var line = item.Lines.AddNew();
					line.HVS_Quantity = 1;
					Factory.Save();
					line.HVS_GoodsDescription = "test";
					AssertEquals("Line edit did not set HVC_PreScreeningStatus to UNK", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);
				});
			}
		}

		public void TestGoodsValueInLocalCurrency_WhenGoodsValueIsUSDInItalyCompany()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Italy))
			{
				var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCodes.UnitedStates);
				var now = ZDateTime.Now;
				var rate = currency.ExchangeRates.AddNew();
				rate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
				rate.RE_StartDate = now.AddDays(-2);
				rate.RE_ExpiryDate = now.AddDays(2);
				rate.RE_SellRate = 2;
				rate.RE_GC = GlbCompany.CurrentCompany.PK;

				Factory.Save();

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_GoodsValue = 100;
				consignment.HVC_RX_NKGoodsValueCurrency = "USD";

				AssertEquals(50m, consignment.GoodsValueInLocalCurrency);
			}
		}

		public void TestLocalCurrencyCode()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.UnitedStates))
			{
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				AssertEquals(consignment.LocalCurrencyCode, "USD");
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Italy))
			{
				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				AssertEquals(consignment.LocalCurrencyCode, "EUR");
			}
		}

		public void TestPreScreeningStatusSetToUnknown_WhenConsignmentFieldIsEdited_GoodsValue()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.ManagingShipment = shipment;
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true };

			var rule = preScreeningConfiguration.Rules.AddNew();
			rule.TransportMode = TransportModes.Sea;
			rule.OriginCountryCode = CountryCodes.UnitedStates;
			rule.DestinationCountryCode = CountryCodes.Australia;

			var field = rule.Fields.AddNew();
			var fieldDescriptionList = field.FieldDescriptionList;
			field.FieldDescription = fieldDescriptionList.GetCodeFromDescription(HVLVConsignmentSchema.Constants.HVC_GoodsValue);
			field.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Error;

			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
			Factory.Save();
			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			{
				consignment.HVC_GoodsValue = 1000;
			}

			AssertEquals("Consignment Goods Value edit did not set HVC_PreScreeningStatus to UNK", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);
		}

		public void TestPreScreeningStatusSetToUnknown_WhenConsignmentFieldIsEdited_ItemLineTariff()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.ManagingShipment = shipment;
			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true };

			var rule = preScreeningConfiguration.Rules.AddNew();
			rule.TransportMode = TransportModes.Sea;
			rule.OriginCountryCode = CountryCodes.UnitedStates;
			rule.DestinationCountryCode = CountryCodes.Australia;

			var field1 = rule.Fields.AddNew();
			var fieldDescriptionList = field1.FieldDescriptionList;
			field1.FieldDescription = fieldDescriptionList.GetCodeFromDescription(HVLVItemLineSchema.Constants.HVS_OriginTariff);
			field1.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Error;

			var field2 = rule.Fields.AddNew();
			field2.FieldDescription = fieldDescriptionList.GetCodeFromDescription(HVLVItemLineSchema.Constants.HVS_DestinationTariff);
			field2.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Error;

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			{
				CombineAssertions("Edit Consignment Item Line Tariff sets HVC_PreScreeningStatus to UNK", () =>
				{
					consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
					var item = consignment.Items.AddNew();
					var line = item.Lines.AddNew();
					line.HVS_Quantity = 1;
					Factory.Save();
					line.HVS_FormattedOriginTariff = "1234.56.78";
					AssertEquals("Origin Tariff edit did not set HVC_PreScreeningStatus to UNK", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);

					consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
					line.HVS_FormattedDestinationTariff = "1234.56.78";
					AssertEquals("Destination Tariff edit did not set HVC_PreScreeningStatus to UNK", HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment.HVC_PreScreeningStatus);
				});
			}
		}

		public void TestHVLVPreScreeningStatusChangedToPAS_WhenConfigIsEmpty_ForParticularConsignment()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var consignment3 = header.Consignments.AddNew();
			var consignment4 = header.Consignments.AddNew();

			Factory.Save();

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true };
			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			{
				var provider = new ETailPreScreeningProvider(consignment1);
				provider.Screen();
				provider.SyncScreeningResult();
				CombineAssertions(() =>
				{
					AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment2.HVC_PreScreeningStatus);
					AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment3.HVC_PreScreeningStatus);
					AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown, consignment4.HVC_PreScreeningStatus);

					AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Passed, consignment1.HVC_PreScreeningStatus);
				});
			}
		}

		public void TestHVLVPreScreeningStatusChangedToPAS_WhenConfigIsEmpty_MultipleConsignments()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var consignment3 = header.Consignments.AddNew();

			Factory.Save();

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true };
			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			{
				var providerWithFactory = new ETailPreScreeningProvider(header);
				providerWithFactory.Screen();
				providerWithFactory.SyncScreeningResult();
				CombineAssertions(() =>
				{
					AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Passed, consignment1.HVC_PreScreeningStatus);
					AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Passed, consignment2.HVC_PreScreeningStatus);
					AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.Passed, consignment3.HVC_PreScreeningStatus);
				});
			}
		}

		#endregion

		#region ACAS

		public void TestACASStatusReadOnly()
		{
			var type = typeof(HVLVConsignment);
			AssertHasCustomAttribute<ReadOnlyAttribute>(type, AutoHVLVConsignment.Schema.HVC_ACASStatus, false, a => a.IsReadOnly);
		}

		public void TestListAttributeOfHVC_ACASStatus()
		{
			var type = typeof(HVLVConsignment);
			AssertHasCustomAttribute<ListAttribute>(type, AutoHVLVConsignment.Schema.HVC_ACASStatus, false,
						a => a.ListDataSourceMember == "Lookups.HVC_ACASStatus_List");
		}

		public void TestACASMessageStatusReadOnly()
		{
			var type = typeof(HVLVConsignment);
			AssertHasCustomAttribute<ReadOnlyAttribute>(type, AutoHVLVConsignment.Schema.HVC_ACASMessageStatus, false, a => a.IsReadOnly);
		}

		public void TestListAttributeOfHVC_ACASMessageStatus()
		{
			var type = typeof(HVLVConsignment);
			AssertHasCustomAttribute<ListAttribute>(type, AutoHVLVConsignment.Schema.HVC_ACASMessageStatus, false,
						a => a.ListDataSourceMember == "Lookups.HVC_ACASMessageStatus_List");
		}

		public void TestACASInterchangeStatusReadOnly()
		{
			var type = typeof(HVLVConsignment);
			AssertHasCustomAttribute<ReadOnlyAttribute>(type, AutoHVLVConsignment.Schema.HVC_ACASInterchangeStatus, false, a => a.IsReadOnly);
		}

		public void TestListAttributeOfHVC_ACASInterchangeStatus()
		{
			var type = typeof(HVLVConsignment);
			AssertHasCustomAttribute<ListAttribute>(type, AutoHVLVConsignment.Schema.HVC_ACASInterchangeStatus, false,
						a => a.ListDataSourceMember == "Lookups.HVC_ACASInterchangeStatus_List");
		}

		#endregion

		#region Events

		public void TestBusinessObjectsWithRelatedEvents()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol1 = Factory.New<ForwardingConsol>();
				consol1.JK_DatePortOfFirstArrival = ZDateTime.Now;
				consol1.JK_RL_NKDischargePort = "AUSYD";

				var consol2 = Factory.New<ForwardingConsol>();
				consol2.JK_DatePortOfFirstArrival = ZDateTime.Now;
				consol2.JK_RL_NKDischargePort = "AUSYD";

				var shipment1 = Factory.New<ForwardingShipment>();
				shipment1.Consols.Add(consol1);
				shipment1.JS_RL_NKDestination = "AUBNE";
				shipment1.JS_HouseBill = "MB1";
				shipment1.TransportsIncludingRelated.RemoveAll();
				var transport1 = shipment1.TransportsIncludingRelated.AddNew();

				var shipment2 = Factory.New<ForwardingShipment>();
				shipment2.Consols.Add(consol2);
				shipment2.TransportsIncludingRelated.RemoveAll();
				var transport2 = shipment2.TransportsIncludingRelated.AddNew();

				var loadList = Factory.New<HVLVOriginLoadList>();

				var outerPackage = Factory.New<HVLVOuterPackage>();

				var hawb = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
				var mawb = Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
				mawb.CM_MasterHouseBill = "MB1";
				mawb.CM_ArrivalDate = ZDateTime.Now;
				hawb.CS_HAWB = "HB1";
				hawb.CS_JS = shipment1.PK;
				hawb.CS_CM = mawb.PK;

				var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment = header.Consignments.AddNew();

				consignment.HVC_WaybillNumber = "HB1";
				var item1 = consignment.Items.AddNew();
				item1.HVI_JS_LoadedOnShipment = shipment1.PK;
				item1.HVI_ShipperReference = "Shipper1";
				item1.HVI_HVL_LoadList = loadList.PK;

				var item2 = consignment.Items.AddNew();
				item2.HVI_JS_LoadedOnShipment = shipment2.PK;
				item2.HVI_ShipperReference = "Shipper2";
				item2.HVI_HVO_OuterPackage = outerPackage.PK;

				var item3 = consignment.Items.AddNew();
				item3.HVI_JS_LoadedOnShipment = shipment1.PK;
				item3.HVI_ShipperReference = "Shipper3";

				var item4 = consignment.Items.AddNew();
				item4.HVI_ShipperReference = "Shipper4";

				var item5 = consignment.Items.AddNew();
				item5.HVI_ShipperReference = "Shipper5";
				item5.HVI_HVL_LoadList = loadList.PK;

				var item6 = consignment.Items.AddNew();
				item6.HVI_ShipperReference = "Shipper6";
				item6.HVI_HVO_OuterPackage = outerPackage.PK;

				Factory.Save();

				AssertEquals("Precondition: Consignment ID defaulted", "HB1", consignment.HVC_ConsignmentId);
				AssertEquals("Precondition: Item ID defaulted", "Shipper1", item1.HVI_ItemId);
				AssertEquals("Precondition: Item ID defaulted", "Shipper2", item2.HVI_ItemId);
				AssertEquals("Precondition: Item ID defaulted", "Shipper3", item3.HVI_ItemId);
				AssertEquals("Precondition: Item ID defaulted", "Shipper4", item4.HVI_ItemId);
				AssertEquals("Precondition: Item ID defaulted", "Shipper5", item5.HVI_ItemId);
				AssertEquals("Precondition: Item ID defaulted", "Shipper6", item6.HVI_ItemId);

				var expectedResults = new[] { consol1.PK, consol2.PK, shipment1.PK, shipment2.PK, loadList.PK, outerPackage.PK, transport1.PK, transport2.PK, item1.PK, item2.PK, item3.PK, item4.PK, item5.PK, item6.PK, hawb.PK, header.PK };
				AssertContainsExactElementsInAnyOrder(expectedResults, consignment.BusinessObjectsWithRelatedEvents.Select(b => b.PK));
			}
		}

		public void TestBusinessObjectWithRelatedEvents_IncludesStandAloneDeclaration()
		{
			var importDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var exportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JE_ImportDeclaration = importDeclaration.PK;
			consignment.HVC_JE_ExportDeclaration = exportDeclaration.PK;

			Factory.Save();

			CombineAssertions("Bizos with related Events should included linked stand alone declaration", () =>
			{
				AssertCollectionContains(importDeclaration, consignment.BusinessObjectsWithRelatedEvents);
				AssertCollectionContains(exportDeclaration, consignment.BusinessObjectsWithRelatedEvents);
			});
		}

		public void TestBusinessObjectsWithRelatedEvents_IncludesItemTransportBookings()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();

			var transportBooking1 = Factory.NewWithValidTestData<DtbBooking>();
			var transportBooking2 = Factory.NewWithValidTestData<DtbBooking>();
			var transportBooking3 = Factory.NewWithValidTestData<DtbBooking>();

			item1.HVI_KM_LastMileTransportBooking = transportBooking1.PK;
			item2.HVI_KM_LastMileTransportBooking = transportBooking2.PK;
			item3.HVI_KM_LastMileTransportBooking = transportBooking3.PK;

			Factory.Save();

			CombineAssertions("Bizos with related Events should included linked transport bookings from items", () =>
			{
				AssertCollectionContains(transportBooking1, consignment.BusinessObjectsWithRelatedEvents);
				AssertCollectionContains(transportBooking2, consignment.BusinessObjectsWithRelatedEvents);
				AssertCollectionContains(transportBooking3, consignment.BusinessObjectsWithRelatedEvents);
			});
		}

		#endregion

		#region Show Export or Import Custom Status
		public void TestIsImport()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = "US";
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				Assert(consignment.IsImport);

				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = "US";
				Assert(!consignment.IsImport);

				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				Assert(!consignment.IsImport);

				consignment.HVC_RN_NKShipperCountryCode = "US";
				consignment.HVC_RN_NKConsigneeCountryCode = "DE";
				Assert(!consignment.IsImport);

				consignment.HVC_RN_NKShipperCountryCode = ZString.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = ZString.Empty;
				Assert(!consignment.IsImport);
			}
		}

		public void TestIsExport()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = "US";
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				Assert(!consignment.IsExport);

				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = "US";
				Assert(consignment.IsExport);

				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				Assert(!consignment.IsExport);

				consignment.HVC_RN_NKShipperCountryCode = "US";
				consignment.HVC_RN_NKConsigneeCountryCode = "DE";
				Assert(!consignment.IsExport);

				consignment.HVC_RN_NKShipperCountryCode = ZString.Empty;
				consignment.HVC_RN_NKConsigneeCountryCode = ZString.Empty;
				Assert(!consignment.IsExport);
			}
		}

		public void TestShowExportAndShowImport()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!", "HLD");
				HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "%%%", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);

				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = "US";
				Assert(consignment.ShowExport);
				Assert(!consignment.ShowImport);

				consignment.HVC_RN_NKShipperCountryCode = "US";
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				Assert(!consignment.ShowExport);
				Assert(consignment.ShowImport);

				consignment.HVC_RN_NKShipperCountryCode = "AU";
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				consignment.HVC_ExportCustomsClearanceStatus = "%%%";
				Assert(consignment.ShowExport);
				Assert(!consignment.ShowImport);

				consignment.HVC_ImportCustomsClearanceStatus = "&&&";
				Assert(!consignment.ShowExport);
				Assert(consignment.ShowImport);
			}
		}

		#endregion

		public void TestHasItemsNotOnManagingShipment()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			var consignmentHeader1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var consignmentHeader2 = shipment2.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader1.Consignments.AddNew();
			var consignment2 = consignmentHeader2.Consignments.AddNew();

			consignment1.Items.AddNew().HVI_JS_LoadedOnShipment = shipment1.PK;
			consignment1.Items.AddNew().HVI_JS_LoadedOnShipment = shipment2.PK;

			consignment2.Items.AddNew().HVI_JS_LoadedOnShipment = shipment1.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Consignment 1 has items on another shipment:", true, consignment1.HasItemsNotOnManagingShipment);
				AssertEquals("Consignment 2 does not have items on another shipment:", false, consignment2.HasItemsNotOnManagingShipment);
			});
		}

		public void TestIDtbBookingParentMembers()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WaybillNumber = "H000245";
			consignment.HVC_Status = "OPN";
			var dtbBookingParent = consignment as IDtbBookingParent;

			AssertNotNull("Consignment should implement IDtbBookingParent", dtbBookingParent);

			CombineAssertions("IDtbBookingParent members should be set correctly", () =>
			{
				AssertEquals("Controller ID", ControllerIDs.HVLVConsignment, dtbBookingParent.ControllerID);
				AssertEquals("Job Number", consignment.HVC_ConsignmentId, dtbBookingParent.JobNumber);
				AssertEquals("Job Status", "OPN", dtbBookingParent.JobStatus);
				AssertEquals("Job Description", "HVLV Consignment", dtbBookingParent.JobDescription);
				AssertContainsExactElementsInAnyOrder("Supported Directions", new[] { DtbBookingDirection.DLV }, dtbBookingParent.GetSupportedDirections());
			});
		}

		public void TestCanCreateTransportBooking()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var dtbBookingParent = consignment as IDtbBookingParent;
			AssertEquals("CanCreateTransportBooking should always return true", true, dtbBookingParent.CanCreateTransportBooking);
		}

		public void TestBookingParentPK()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var dtbBookingParent = consignment as IDtbBookingParent;
			AssertEquals("BookingParentPK should be the Consignment PK.", consignment.PK, dtbBookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var dtbBookingParent = consignment as IDtbBookingParent;
			AssertEquals("BookingParentTablePrefix should be the Consignment table prefix.", consignment.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var dtbBookingParent = consignment as IDtbBookingParent;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		#region Shipment/Consol Properties

		public void TestShipmentTransportMode()
		{
			var parentShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			parentShipment.JS_TransportMode = TransportModes.Rail;
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();

			consignment1.HVC_JS_ManifestedOnShipment = parentShipment.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Transport Mode from Shipment", TransportModes.Rail, consignment1.ShipmentTransportMode);
				AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignment2.ShipmentTransportMode);
			});
		}

		public void TestShipmentPackingMode()
		{
			var parentShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			parentShipment.JS_PackingMode = ContainerModes.Loose;
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();

			consignment1.HVC_JS_ManifestedOnShipment = parentShipment.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Packing Mode from Shipment", ContainerModes.Loose, consignment1.ShipmentPackingMode);
				AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignment2.ShipmentPackingMode);
			});
		}

		public void TestConsolOrigin_Import()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "NZAKL";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "USCHI";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Import, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Origin Port from Arrival Consol", "AUSYD", consignment.ConsolOrigin);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolOrigin);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolOrigin);
				});
			}
		}

		public void TestConsolOrigin_Export()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USCHI";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "USCHI";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Export, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Origin Port from Departure Consol", "NZAKL", consignment.ConsolOrigin);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolOrigin);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolOrigin);
				});
			}
		}

		public void TestConsolDestination_Import()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "NZAKL";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "USCHI";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Import, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Destination Port from Arrival Consol", "NZAKL", consignment.ConsolDestination);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolDestination);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolDestination);
				});
			}
		}

		public void TestConsolDestination_Export()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USCHI";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "USCHI";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Export, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Destination Port from Departure Consol", "AUSYD", consignment.ConsolDestination);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolDestination);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolDestination);
				});
			}
		}

		[TestDate(2020, 09, 10)]
		public void TestConsolETD_Import()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "NZAKL";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "USCHI";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

				var departureLeg = departureConsol.Transports[0];
				departureLeg.JW_ETA = ZDateTime.Today.AddDays(-3);
				departureLeg.JW_ETD = ZDateTime.Today.AddDays(-4);

				var arrivalLeg = arrivalConsol.Transports[0];
				arrivalLeg.JW_ETA = ZDateTime.Today;
				arrivalLeg.JW_ETD = ZDateTime.Today.AddDays(-1);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Import, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("ETD from Arrival Consol", ZDateTime.Today.AddDays(-1).Date, consignment.ConsolETD.Date);
					AssertEquals("Should be empty if not attached to Shipment", ZDateTime.Empty, consignmentNotOnShipment.ConsolETD);
					AssertEquals("Should be empty if attached to Shipment with no consol", ZDateTime.Empty, consignmentOnShipmentWithNoConsol.ConsolETD);
				});
			}
		}

		[TestDate(2020, 09, 10)]
		public void TestConsolETD_Export()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USCHI";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "USCHI";

				var departureLeg = departureConsol.Transports[0];
				departureLeg.JW_ETA = ZDateTime.Today.AddDays(-3);
				departureLeg.JW_ETD = ZDateTime.Today.AddDays(-4);

				var arrivalLeg = arrivalConsol.Transports[0];
				arrivalLeg.JW_ETA = ZDateTime.Today;
				arrivalLeg.JW_ETD = ZDateTime.Today.AddDays(-1);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Export, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("ETD from Departure Consol", ZDateTime.Today.AddDays(-4).Date, consignment.ConsolETD.Date);
					AssertEquals("Should be empty if not attached to Shipment", ZDateTime.Empty, consignmentNotOnShipment.ConsolETD);
					AssertEquals("Should be empty if attached to Shipment with no consol", ZDateTime.Empty, consignmentOnShipmentWithNoConsol.ConsolETD);
				});
			}
		}

		[TestDate(2020, 09, 10)]
		public void TestConsolETA_Import()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "NZAKL";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "USCHI";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

				var departureLeg = departureConsol.Transports[0];
				departureLeg.JW_ETA = ZDateTime.Today.AddDays(-3);
				departureLeg.JW_ETD = ZDateTime.Today.AddDays(-4);

				var arrivalLeg = arrivalConsol.Transports[0];
				arrivalLeg.JW_ETA = ZDateTime.Today;
				arrivalLeg.JW_ETD = ZDateTime.Today.AddDays(-1);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Import, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("ETA from Arrival Consol", ZDateTime.Today.Date, consignment.ConsolETA.Date);
					AssertEquals("Should be empty if not attached to Shipment", ZDateTime.Empty, consignmentNotOnShipment.ConsolETA);
					AssertEquals("Should be empty if attached to Shipment with no consol", ZDateTime.Empty, consignmentOnShipmentWithNoConsol.ConsolETA);
				});
			}
		}

		[TestDate(2020, 09, 10)]
		public void TestConsolETA_Export()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USCHI";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "USCHI";

				var departureLeg = departureConsol.Transports[0];
				departureLeg.JW_ETA = ZDateTime.Today.AddDays(-3);
				departureLeg.JW_ETD = ZDateTime.Today.AddDays(-4);

				var arrivalLeg = arrivalConsol.Transports[0];
				arrivalLeg.JW_ETA = ZDateTime.Today;
				arrivalLeg.JW_ETD = ZDateTime.Today.AddDays(-1);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Export, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("ETA from Departure Consol", ZDateTime.Today.AddDays(-3).Date, consignment.ConsolETA.Date);
					AssertEquals("Should be empty if not attached to Shipment", ZDateTime.Empty, consignmentNotOnShipment.ConsolETA);
					AssertEquals("Should be empty if attached to Shipment with no consol", ZDateTime.Empty, consignmentOnShipmentWithNoConsol.ConsolETA);
				});
			}
		}

		public void TestConsolMasterBill_Import()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "NZAKL";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "USCHI";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";
				departureConsol.JK_MasterBillNum = "1234";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";
				arrivalConsol.JK_MasterBillNum = "5678";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Import, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Master Bill Number from Arrival Consol", "5678", consignment.ConsolMasterBill);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolMasterBill);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolMasterBill);
				});
			}
		}

		public void TestConsolMasterBill_Export()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USCHI";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";
				departureConsol.JK_MasterBillNum = "1234";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "USCHI";
				arrivalConsol.JK_MasterBillNum = "5678";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Export, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Master Bill Number from Departure Consol", "1234", consignment.ConsolMasterBill);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolMasterBill);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolMasterBill);
				});
			}
		}

		public void TestConsolVoyageFlight_Import()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "NZAKL";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "USCHI";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

				var departureLeg = departureConsol.Transports[0];
				departureLeg.JW_VoyageFlight = "AB12";

				var arrivalLeg = arrivalConsol.Transports[0];
				arrivalLeg.JW_VoyageFlight = "CD34";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Import, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Voyage/Flight from Arrival Consol", "CD34", consignment.ConsolVoyageFlight);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolVoyageFlight);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolVoyageFlight);
				});
			}
		}

		public void TestConsolVoyageFlight_Export()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USCHI";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "USCHI";

				var departureLeg = departureConsol.Transports[0];
				departureLeg.JW_VoyageFlight = "AB12";

				var arrivalLeg = arrivalConsol.Transports[0];
				arrivalLeg.JW_VoyageFlight = "CD34";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Export, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Voyage/Flight from Departure Consol", "AB12", consignment.ConsolVoyageFlight);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolVoyageFlight);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolVoyageFlight);
				});
			}
		}

		public void TestConsolVessel_Import()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "NZAKL";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "USCHI";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

				var departureLeg = departureConsol.Transports[0];
				departureLeg.JW_Vessel = "PQ12";

				var arrivalLeg = arrivalConsol.Transports[0];
				arrivalLeg.JW_Vessel = "XY34";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Import, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Vessel from Arrival Consol", "XY34", consignment.ConsolVessel);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolVessel);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolVessel);
				});
			}
		}

		public void TestConsolVessel_Export()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipmentWithNoConsol = Factory.New<ForwardingShipment>();
				shipmentWithNoConsol.JS_RL_NKOrigin = "NZAKL";
				shipmentWithNoConsol.JS_RL_NKDestination = "USCHI";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USCHI";

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "NZAKL";
				departureConsol.JK_RL_NKDischargePort = "AUSYD";

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "USCHI";

				var departureLeg = departureConsol.Transports[0];
				departureLeg.JW_Vessel = "PQ12";

				var arrivalLeg = arrivalConsol.Transports[0];
				arrivalLeg.JW_Vessel = "XY34";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentNotOnShipment = Factory.NewWithValidTestData<HVLVConsignment>();
				var consignmentOnShipmentWithNoConsol = Factory.NewWithValidTestData<HVLVConsignment>();

				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentOnShipmentWithNoConsol.HVC_JS_ManifestedOnShipment = shipmentWithNoConsol.PK;

				AssertEquals("Precondition:", Directions.Export, consignment.DirectionOfTrade);
				CombineAssertions(() =>
				{
					AssertEquals("Vessel from Departure Consol", "PQ12", consignment.ConsolVessel);
					AssertEquals("Should be empty if not attached to Shipment", string.Empty, consignmentNotOnShipment.ConsolVessel);
					AssertEquals("Should be empty if attached to Shipment with no consol", string.Empty, consignmentOnShipmentWithNoConsol.ConsolVessel);
				});
			}
		}

		#endregion

		#region HVC_Status default behaviour

		public void TestStatusDefaultValue()
		{
			var consignment = Factory.New<HVLVConsignment>();
			AssertEquals("BKD", consignment.HVC_Status);
		}

		public void TestStatusDefault_WhenConsignmentIsCreatedInBookingHeader()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_IsBookingConfirmed = false;
			var consignment = bookingHeader.Consignments.AddNew();
			Factory.Save();
			AssertEquals("BKD", consignment.HVC_Status);
		}

		public void TestStatusDefault_WhenConsignmentIsCreatedInConfirmedBookingHeader()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_IsBookingConfirmed = true;
			var consignment = bookingHeader.Consignments.AddNew();
			Factory.Save();
			AssertEquals("CNF", consignment.HVC_Status);
		}

		public void TestStatusDefault_WhenBookingHeaderIsConfirmed()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			bookingHeader.HVH_IsBookingConfirmed = true;

			AssertEquals("CNF", consignment.HVC_Status);
		}

		public void TestHVC_Status_WhenBookingHeaderConsfirmationIsCancelled()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			bookingHeader.HVH_IsBookingConfirmed = true;
			AssertEquals("CNF", consignment.HVC_Status);

			bookingHeader.HVH_IsBookingConfirmed = false;
			AssertEquals("BKD", consignment.HVC_Status);
		}

		public void TestStatus_WhenHVC_ImportReleaseStatusIsCLR()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;

			AssertEquals("CLR", consignment.HVC_Status);
		}

		public void TestStatus_WhenHVC_ImportReleaseStatusIsHLD()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;

			AssertEquals("HLD", consignment.HVC_Status);
		}

		public void TestStatus_WhenHVC_ExportReleaseStatusIsCLR()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Cleared;
			AssertEquals("BKD", consignment.HVC_Status);
		}

		public void TestStatus_WhenHVC_ExportReleaseStatusIsHLD()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;
			AssertEquals("BKD", consignment.HVC_Status);
		}

		public void TestStatus_WhenItemHasDLVStatus()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_Status = HVLVItemStatus.Codes.Delivered;

			AssertEquals("DLV", consignment.HVC_Status);
		}

		#endregion

		#region CarrierServiceLevel

		public void TestCarrierServiceLevelNoExceptionFromNonUniqueCode()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "ABC";

			var serviceLevel = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel.PL_Code = "ABC";

			var serviceLevel2 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel2.PL_Code = "ABC";

			AssertNoExceptionThrown("Accessing CarrierServiceLevel should not throw exception", () => { _ = consignment.LastMileCarrierServiceLevel; });
		}

		#endregion

		#region Unit Conversion

		public void TestConvertToChargeableUQWeight_InvalidUQ_NoExceptionAndReturns0()
		{
			var consignment = SetupConsignmentWithUnits("0", "0");
			var shipment = Factory.New<ForwardingShipment>();
			consignment.ManagingShipment = shipment;

			shipment.JS_TransportMode = TransportModes.Air;
			AssertEquals("Precondition: Chargeable by weight", Weight.Kilograms, consignment.ChargeableUQ);

			var convertedAmount = 0m;
			AssertNoExceptionThrown(() => convertedAmount = consignment.ConvertToChargeableUQ(10, consignment.HVC_WeightUQ));
			AssertEquals(0m, convertedAmount);
		}

		public void TestConvertToChargeableUQVolume_InvalidUQ_NoExceptionAndReturns0()
		{
			var consignment = SetupConsignmentWithUnits("0", "0");
			var shipment = Factory.New<ForwardingShipment>();
			consignment.ManagingShipment = shipment;

			shipment.JS_TransportMode = TransportModes.Sea;
			AssertEquals("Precondition: Chargeable by volume", Volume.CubicMetres, consignment.ChargeableUQ);

			var convertedAmount = 0m;
			AssertNoExceptionThrown(() => convertedAmount = consignment.ConvertToChargeableUQ(10, consignment.HVC_VolumeUQ));
			AssertEquals(0m, convertedAmount);
		}

		public void TestUnitOfMeasureProperties_WhenSetValueLowerCase_GetIsUpperCase()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_VolumeUQ = "m3";
			consignment.HVC_WeightUQ = "kg";

			AssertEquals("M3", consignment.HVC_VolumeUQ);
			AssertEquals("KG", consignment.HVC_WeightUQ);
		}

		#endregion

		public void TestCanCancelWhenConsignmentHasCustomsReleaseStatus()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "LTTSTORE01";
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.None;

			CombineAssertions("Preconditions:", () =>
			{
				Assert("Consignment does not have Custom release status.", !consignment.HasCustomsStatus);
				AssertEquals("CanCancel will return null when consignment does not have release status.", null, consignment.CanCancel());
			});

			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;

			CombineAssertions("When Consignment has ReleaseStatus not NON", () =>
			{
				Assert("Consignment has Customs release status.", consignment.HasCustomsStatus);
				AssertEquals("Cannot deactivate Consignment LTTSTORE01 as it has a Customs release status.", consignment.CanCancel());
			});

			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;

			CombineAssertions("When Consignment has HVC_ExportReleaseStatus not NON", () =>
			{
				Assert("Consignment has Customs release status.", consignment.HasCustomsStatus);
				AssertEquals("Cannot deactivate Consignment LTTSTORE01 as it has a Customs release status.", consignment.CanCancel());
			});
		}

		public void TestConcurrencyPolicyIsSetToIgnoreForDataTable()
		{
			var bizo = Factory.New<HVLVConsignment>();
			var consignmentDataTable = ((INeedTable)bizo).Table;
			AssertEquals(ConcurrencyPolicy.Ignore, consignmentDataTable.ExtendedProperties[typeof(ConcurrencyPolicy)]);
		}

		public override void TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed()
		{
			// TODO: This should be undo when when Enterprise.Integration.Customs.AU.ICustomsManifestLineSequence is changed to make JobConsol it's parent; there is a requirement that a CustomsManifestLineSequence record still exists after this bizobj is deleted and this record is can be linked back JobConsol
			Assert(true);
		}

		#region EDT Logs

		public void TestShouldNotCreateEDTLogWhenConsignmentIsAltered()
		{
			var newFactoryForLoading = new BusinessObjectFactory();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			Factory.Save();

			var reloadedConsignment = Factory.Load<HVLVConsignment>(consignment.PK);
			reloadedConsignment.HVC_IsSignatureRequired = true;
			Factory.Save();

			reloadedConsignment = newFactoryForLoading.Load<HVLVConsignment>(consignment.PK);

			AssertEquals(0, reloadedConsignment.Logs.GetAllLogs().Count(log => ((StmALog)log).SL_SE_NKEvent == AutoEvents.EditedARecord.Code));
		}

		#endregion

		#region IScreeningPartyProvider Members

		public void TestGetWorstScreeningStatus()
		{
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipperAddress.Header.OH_Code = "SHIPPER";
			shipperAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var screeningPartyProvider = consignment as IScreeningPartyProvider;
			AssertEquals(ScreeningStatusesList.Codes.Matched, screeningPartyProvider.GetWorstScreeningStatus());
		}

		public void TestGetWorstScreeningStatus_PartiesInCalculation()
		{
			var matchedAddress = Factory.NewWithValidTestData<OrgAddress>();
			matchedAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var expectedProperties = new string[]
			{
				HVLVConsignmentSchema.Constants.HVC_OA_ShipperAddress,
				HVLVConsignmentSchema.Constants.HVC_OA_ConsigneeAddress,
				HVLVConsignmentSchema.Constants.HVC_OA_DestinationDepot,
				HVLVConsignmentSchema.Constants.HVC_OA_ReturnLocation,
				HVLVConsignmentSchema.Constants.HVC_OH_LastMileCarrier,
				HVLVConsignmentSchema.Constants.HVC_OH_LastMileCarrierBookingAgent
			};

			CombineAssertions(() =>
			{
				foreach (var propertyToTest in expectedProperties)
				{
					ZGuid propertyValueToTest;
					if (propertyToTest.StartsWith("HVC_OA_"))
					{
						propertyValueToTest = matchedAddress.PK;
					}
					else
					{
						propertyValueToTest = matchedAddress.Header.PK;
					}

					var consignment = Factory.New<HVLVConsignment>();
					consignment[propertyToTest] = propertyValueToTest;
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Clear;
					AssertEquals(propertyToTest, ScreeningStatusesList.Codes.Matched, ((IScreeningPartyProvider)consignment).GetWorstScreeningStatus());
				}
			});
		}

		public void TestGetWorstScreeningStatusUnlessManuallyCleared()
		{
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipperAddress.Header.OH_Code = "SHIPPER";
			shipperAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;

			var screeningPartyProvider = consignment as IScreeningPartyProvider;
			AssertEquals("Precondition: worst status is currently MAT", ScreeningStatusesList.Codes.Matched, screeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared());

			consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertEquals(ScreeningStatusesList.Codes.JobCleared, screeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared());
		}

		public void TestScreeningParties()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var screeningPartyProvider = consignment as IScreeningPartyProvider;

			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipperAddress.Header.OH_Code = "SHIPPER";

			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Header.OH_Code = "CONSIGNEE";

			var destinationDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepotAddress.Header.OH_Code = "DEPOT";

			var returnLocationAddress = Factory.NewWithValidTestData<OrgAddress>();
			returnLocationAddress.Header.OH_Code = "RETURN";

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_Code = "CARRIER";

			var lastMileCarrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrierBookingAgent.OH_Code = "AGENT";

			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_OA_DestinationDepot = destinationDepotAddress.PK;
			consignment.HVC_OA_ReturnLocation = returnLocationAddress.PK;
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;
			consignment.HVC_OH_LastMileCarrierBookingAgent = lastMileCarrierBookingAgent.PK;

			Factory.Save();

			var screeningParties = screeningPartyProvider.ScreeningParties;
			var shipperScreeningParty = screeningParties.Single(x => x.OrgCode == "SHIPPER");
			var consigneeScreeningParty = screeningParties.Single(x => x.OrgCode == "CONSIGNEE");
			var destinationDepotScreeningParty = screeningParties.Single(x => x.OrgCode == "DEPOT");
			var returnLocationScreeningParty = screeningParties.Single(x => x.OrgCode == "RETURN");
			var lastMileCarrierScreeningParty = screeningParties.Single(x => x.OrgCode == "CARRIER");
			var lastMileCarrierBookingAgentScreeningParty = screeningParties.Single(x => x.OrgCode == "AGENT");

			CombineAssertions(() =>
			{
				AssertEquals(6, screeningParties.Length);
				AssertEquals("Shipper", shipperScreeningParty.Description);
				AssertEquals("Consignee", consigneeScreeningParty.Description);
				AssertEquals("Destination Depot", destinationDepotScreeningParty.Description);
				AssertEquals("Return Location", returnLocationScreeningParty.Description);
				AssertEquals("Last Mile Carrier", lastMileCarrierScreeningParty.Description);
				AssertEquals("Last Mile Carrier Booking Agent", lastMileCarrierBookingAgentScreeningParty.Description);
			});
		}

		public void TestScreeningParties_FreeTextShipperAndConsigneeAndReturn()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ShipperName = "LINUS";
			consignment.HVC_ConsigneeName = "AMAZON";
			consignment.HVC_ReturnName = "LUCY";

			var screeningPartyProvider = consignment as IScreeningPartyProvider;

			Factory.Save();

			var proxyScreeningPartyProvider = consignment as IScreeningPartyProvider;
			var screeningParties = proxyScreeningPartyProvider.ScreeningParties;
			var shipperScreeningParty = screeningParties.Single(x => x.NaturalPerson.Name == "LINUS");
			var consigneeScreeningParty = screeningParties.Single(x => x.NaturalPerson.Name == "AMAZON");
			var returnScreeningParty = screeningParties.Single(x => x.NaturalPerson.Name == "LUCY");

			CombineAssertions(() =>
			{
				AssertEquals(3, screeningParties.Length);
				AssertEquals("Shipper", shipperScreeningParty.Description);
				AssertEquals("Consignee", consigneeScreeningParty.Description);
				AssertEquals("Return Location", returnScreeningParty.Description);
			});
		}

		public void TestScreeningStatus()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var screeningPartyProvider = consignment as IScreeningPartyProvider;

			consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertEquals("Screening status change should be reflected on consignment", ScreeningStatusesList.Codes.JobCleared, screeningPartyProvider.ScreeningStatus);

			screeningPartyProvider.ScreeningStatus = "TST";
			Assert(consignment.Items.Cast<HVLVItem>().All(x => x.HVI_LastUsageCode == "DPS"));
		}

		#endregion

		public void TestHVC_DeniedPartyScreeningStatus()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var deniedPartyScreeningStatusInfo = consignment.HVC_DeniedPartyScreeningStatusInfo;
			var deniedPartyScreeningStatusData = DataBoundResourceStrings.GetDataForProperty(deniedPartyScreeningStatusInfo);

			CombineAssertions(() =>
			{
				Assert("Is read-only", deniedPartyScreeningStatusInfo.ReadOnly);
				AssertEquals("Denied Party Screening Status", deniedPartyScreeningStatusData.Caption);
			});
		}

		public void TestConsignmentDPSStatusCannotBeChangedIfBookingHeaderDPSStatusIsMatched()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			var bookingHeaderScreeningPartyProvider = bookingHeader as IScreeningPartyProvider;
			var consignmentScreeningPartyProvider = consignment as IScreeningPartyProvider;

			bookingHeaderScreeningPartyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertEquals("Precondition: Consignment DPS status changed to matched", ScreeningStatusesList.Codes.Matched, consignment.HVC_DeniedPartyScreeningStatus);

			consignmentScreeningPartyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertEquals("Consignment DPS status cannot be changed from matched", ScreeningStatusesList.Codes.Matched, consignment.HVC_DeniedPartyScreeningStatus);

			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			consignmentScreeningPartyProvider.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertEquals("Consignment DPS status can be changed from matched as it now has a consignment header (i.e. loaded on shipment)", ScreeningStatusesList.Codes.Clear, consignment.HVC_DeniedPartyScreeningStatus);
		}

		#region Detach and Attach

		public void TestUnableToDetachShipmentWhenStatusIsNotMet()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S000000001";
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignment.HVC_ConsignmentId = "C000000001";

			TestUnableToDetach(HVLVConsignmentStatus.Codes.ArrivedAtDestination, HVLVConsignmentStatus.Descriptions.ArrivedAtDestination);
			TestUnableToDetach(HVLVConsignmentStatus.Codes.CustomsClearedAtDestination, HVLVConsignmentStatus.Descriptions.CustomsClearedAtDestination);
			TestUnableToDetach(HVLVConsignmentStatus.Codes.CustomsHeldAtDestination, HVLVConsignmentStatus.Descriptions.CustomsHeldAtDestination);
			TestUnableToDetach(HVLVConsignmentStatus.Codes.Delivered, HVLVConsignmentStatus.Descriptions.Delivered);

			void TestUnableToDetach(string status, string description)
			{
				consignment.HVC_Status = status;
				AssertEquals(false, consignment.CanDetach);
				AssertEquals($"Unable to detach Consignment C000000001 from Shipment S000000001 since Consignment Status = {description}. ", consignment.ReasonNotToBeAbleToDetach);
			}
		}

		public void TestOnlyActiveConsignmentCanBeDetached()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S000000001";
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignment.HVC_ConsignmentId = "C000000001";
			consignment.HVC_IsActive = false;

			AssertEquals(false, consignment.CanDetach);
			AssertEquals($"Unable to detach Consignment C000000001 from Shipment S000000001 since Consignment is not active. ", consignment.ReasonNotToBeAbleToDetach);
		}

		public void TestOnlyConsignmentWhoseShipmentWithoutCustomsJobCanBeDetached()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S000000001";
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.GenPivotCollection.AddChild(Factory.NewWithValidTestData<BaseJobDeclaration>());
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignment.HVC_ConsignmentId = "C000000001";
			consignment.HVC_Status = HVLVConsignmentStatus.Codes.Booked;

			AssertEquals(false, consignment.CanDetach);
			AssertEquals($"Unable to detach Consignment C000000001 from Shipment S000000001 since Customs job has already been created. ", consignment.ReasonNotToBeAbleToDetach);
		}

		#endregion

		#region IDpsEntityProvider

		public void TestInvalidateScreeningStatusesNotChangeStatusWhenCurrentStatusIsCLP()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			consignment.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, consignment.HVC_DeniedPartyScreeningStatus);
		}

		public void TestInvalidateScreeningStatusesChangeStatus()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Matched;
			consignment.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, consignment.HVC_DeniedPartyScreeningStatus);

			consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			consignment.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, consignment.HVC_DeniedPartyScreeningStatus);
		}

		public void TestInvalidateScreeningStatusesUpdatesShipmentStatusOnSave()
		{
			using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = true }))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
				consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				consignment.InvalidateScreeningStatuses();

				Factory.Save();
				AssertEquals("Consignment status", ScreeningStatusesList.Codes.Unknown, consignment.HVC_DeniedPartyScreeningStatus);
				AssertEquals("Shipment status", ScreeningStatusesList.Codes.Unknown, shipment.JS_ScreeningStatus);

				consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				consignment.InvalidateScreeningStatuses();

				Factory.Save();
				AssertEquals("Shipment status is not recalculated if consignment status didn't change", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
			}
		}

		#endregion

		public void TestShipperFreeTextDeniedPartyIsConsideredAsOrg()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var consignmentScreeningPartyProvider = consignment as IScreeningPartyProvider;
			var screeningParties = consignmentScreeningPartyProvider.ScreeningParties;
			var shipperScreeningParty = screeningParties.Single(p => p.Description == "Shipper");

			Assert("Shipper party should be screened as Organisation", shipperScreeningParty.NaturalPerson.IsConsideredAsOrganization);
		}

		public void TestUpdateCustomsClearanceStatusForTwoConsighmentsUnderSameDeclaration()
		{
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.CustomsStatus);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "%%%", "Another Code", "CLR", RefCusCodeListTypes.Codes.CustomsStatus);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "&&&", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "%%%", "Another Code", "CLR", RefCusCodeListTypes.Codes.ExportCustomsStatus);

			var manifestedShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = manifestedShipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = consignmentHeader.Consignments.AddNew();
			var consignment2 = consignmentHeader.Consignments.AddNew();
			var exportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var importDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			exportDeclaration.IsCancelled = false;
			importDeclaration.IsCancelled = false;
			var exportJobDeclarationPK = exportDeclaration.PK;
			var importJobDeclarationPK = importDeclaration.PK;
			consignment1.HVC_JE_ExportDeclaration = exportJobDeclarationPK;
			consignment2.HVC_JE_ExportDeclaration = exportJobDeclarationPK;
			consignment1.HVC_JE_ImportDeclaration = importJobDeclarationPK;
			consignment2.HVC_JE_ImportDeclaration = importJobDeclarationPK;

			consignment1.HVC_ExportCustomsClearanceStatus = "&&&";
			consignment2.HVC_ExportCustomsClearanceStatus = "&&&";
			consignment1.HVC_ImportCustomsClearanceStatus = "&&&";
			consignment2.HVC_ImportCustomsClearanceStatus = "&&&";

			var manifestedShipmentOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			manifestedShipmentOrigin.RL_RN_NKCountryCode = "AU";
			manifestedShipment.JS_RL_NKOrigin = manifestedShipmentOrigin.RL_Code;

			var manifestedShipmentDestination = Factory.NewWithValidTestData<RefUNLOCO>();
			manifestedShipmentDestination.RL_RN_NKCountryCode = "US";
			manifestedShipment.JS_RL_NKDestination = manifestedShipmentDestination.RL_Code;

			consignment1.HVC_ExportCustomsClearanceStatus = "%%%";

			CombineAssertions("consignment1 and consignment2 should be updated to the same clearance status", () =>
			{
				AssertEquals("%%%", consignment1.HVC_ExportCustomsClearanceStatus);
				AssertEquals("%%%", consignment2.HVC_ExportCustomsClearanceStatus);
			});

			manifestedShipmentOrigin.RL_RN_NKCountryCode = "US";
			manifestedShipmentDestination.RL_RN_NKCountryCode = "AU";
			consignment1.HVC_ImportCustomsClearanceStatus = "%%%";

			CombineAssertions("consignment1 and consignment2 should be updated to the same clearance status", () =>
			{
				AssertEquals("%%%", consignment1.HVC_ImportCustomsClearanceStatus);
				AssertEquals("%%%", consignment2.HVC_ImportCustomsClearanceStatus);
			});
		}

		#region Update DPS Status when address properties change

		readonly string[] consigneeAddressProperties = new[]
		{
			HVLVConsignmentSchema.Constants.HVC_ConsigneeName,
			HVLVConsignmentSchema.Constants.HVC_ConsigneeAddress1,
			HVLVConsignmentSchema.Constants.HVC_ConsigneeAddress2,
			HVLVConsignmentSchema.Constants.HVC_ConsigneeCity,
			HVLVConsignmentSchema.Constants.HVC_ConsigneeState,
			HVLVConsignmentSchema.Constants.HVC_ConsigneePostcode,
			HVLVConsignmentSchema.Constants.HVC_RN_NKConsigneeCountryCode,
		};

		readonly string[] shipperAddressProperties = new[]
		{
			HVLVConsignmentSchema.Constants.HVC_ShipperName,
			HVLVConsignmentSchema.Constants.HVC_ShipperAddress1,
			HVLVConsignmentSchema.Constants.HVC_ShipperAddress2,
			HVLVConsignmentSchema.Constants.HVC_ShipperCity,
			HVLVConsignmentSchema.Constants.HVC_ShipperState,
			HVLVConsignmentSchema.Constants.HVC_ShipperPostcode,
			HVLVConsignmentSchema.Constants.HVC_RN_NKShipperCountryCode,
		};

		readonly string[] returnAddressProperties = new[]
		{
			HVLVConsignmentSchema.Constants.HVC_ReturnName,
			HVLVConsignmentSchema.Constants.HVC_ReturnAddress1,
			HVLVConsignmentSchema.Constants.HVC_ReturnAddress2,
			HVLVConsignmentSchema.Constants.HVC_ReturnCity,
			HVLVConsignmentSchema.Constants.HVC_ReturnState,
			HVLVConsignmentSchema.Constants.HVC_ReturnPostcode,
			HVLVConsignmentSchema.Constants.HVC_RN_NKReturnCountryCode,
		};

		public void TestDeniedPartyScreeningStatusInvalidated_WhenAddressFreeTextFieldsUpdated_Consignee()
		{
			var consignment = Factory.New<HVLVConsignment>();

			CombineAssertions("When Consignee is not organization", () => {
				Assert("Precondition: Consignee is not organization", !consignment.ConsigneeIsOrganisation);
				foreach (var property in consigneeAddressProperties)
				{
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Clear;
					consignment[property] = "A";
					AssertEquals(property, ScreeningStatusesList.Codes.Unknown, consignment.HVC_DeniedPartyScreeningStatus);
				}
			});

			CombineAssertions("When Consignee is organization, no change", () => {
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				consignment.HVC_OA_ConsigneeAddress = orgAddress.PK;

				Assert("Precondition: Consignee is organization", consignment.ConsigneeIsOrganisation);
				foreach (var property in consigneeAddressProperties)
				{
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Clear;
					consignment[property] = "A";
					AssertEquals(property, ScreeningStatusesList.Codes.Clear, consignment.HVC_DeniedPartyScreeningStatus);
				}
			});
		}

		public void TestDeniedPartyScreeningStatusInvalidated_WhenAddressFreeTextFieldsUpdated_Shipper()
		{
			var consignment = Factory.New<HVLVConsignment>();

			CombineAssertions("When Shipper is not organization", () => {
				Assert("Precondition: Shipper is not organization", !consignment.ShipperIsOrganisation);
				foreach (var property in shipperAddressProperties)
				{
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Clear;
					consignment[property] = "A";
					AssertEquals(property, ScreeningStatusesList.Codes.Unknown, consignment.HVC_DeniedPartyScreeningStatus);
				}
			});

			CombineAssertions("When Shipper is organization, no change", () => {
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				consignment.HVC_OA_ShipperAddress = orgAddress.PK;

				Assert("Precondition: Shipper is organization", consignment.ShipperIsOrganisation);
				foreach (var property in shipperAddressProperties)
				{
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Clear;
					consignment[property] = "A";
					AssertEquals(property, ScreeningStatusesList.Codes.Clear, consignment.HVC_DeniedPartyScreeningStatus);
				}
			});
		}

		public void TestDeniedPartyScreeningStatusInvalidated_WhenAddressFreeTextFieldsUpdated_Return()
		{
			var consignment = Factory.New<HVLVConsignment>();

			CombineAssertions("When Return is not organization", () => {
				Assert("Precondition: Return is not organization", !consignment.ReturnIsOrganisation);
				foreach (var property in returnAddressProperties)
				{
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Clear;
					consignment[property] = "A";
					AssertEquals(property, ScreeningStatusesList.Codes.Unknown, consignment.HVC_DeniedPartyScreeningStatus);
				}
			});

			CombineAssertions("When Return is organization, no change", () => {
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				consignment.HVC_OA_ReturnLocation = orgAddress.PK;

				Assert("Precondition: Return is organization", consignment.ReturnIsOrganisation);
				foreach (var property in returnAddressProperties)
				{
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Clear;
					consignment[property] = "A";
					AssertEquals(property, ScreeningStatusesList.Codes.Clear, consignment.HVC_DeniedPartyScreeningStatus);
				}
			});
		}

		public void TestUpdateScreeningStatusFromParties_WhenConsigneeUpdated()
		{
			AssertUpdateScreeningStatusFromParties(HVLVConsignmentSchema.Constants.HVC_OA_ConsigneeAddress);
		}

		public void TestUpdateScreeningStatusFromParties_WhenShipperUpdated()
		{
			AssertUpdateScreeningStatusFromParties(HVLVConsignmentSchema.Constants.HVC_OA_ShipperAddress);
		}

		public void TestUpdateScreeningStatusFromParties_WhenReturnLocationUpdated()
		{
			AssertUpdateScreeningStatusFromParties(HVLVConsignmentSchema.Constants.HVC_OA_ReturnLocation);
		}

		public void TestUpdateScreeningStatusFromParties_WhenDestinationDepotUpdated()
		{
			AssertUpdateScreeningStatusFromParties(HVLVConsignmentSchema.Constants.HVC_OA_DestinationDepot);
		}

		public void TestUpdateScreeningStatusFromParties_WhenLastMileCarrierUpdated()
		{
			AssertUpdateScreeningStatusFromParties(HVLVConsignmentSchema.Constants.HVC_OH_LastMileCarrier, isPropertyOrgAddress: false);
		}

		public void TestUpdateScreeningStatusFromParties_WhenLastMileCarrierBookingAgentUpdated()
		{
			AssertUpdateScreeningStatusFromParties(HVLVConsignmentSchema.Constants.HVC_OH_LastMileCarrierBookingAgent, isPropertyOrgAddress: false);
		}

		void AssertUpdateScreeningStatusFromParties(string propertyToTest, bool isPropertyOrgAddress = true)
		{
			using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = true }))
			{
				var matchedOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
				var clearOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
				Factory.Save();
				matchedOrgAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				clearOrgAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				var clearPKValueForTest = isPropertyOrgAddress ? clearOrgAddress.PK : clearOrgAddress.Header.PK;
				var matchedPKValueForTest = isPropertyOrgAddress ? matchedOrgAddress.PK : matchedOrgAddress.Header.PK;

				CombineAssertions(() =>
				{
					AssertStatusShouldNotUpdate_WhenCurrentStatusNOT();
					AssertStatusShouldNotUpdate_WhenCurrentStatusCLP();
					AssertStatusShouldRecalculate_DoesNotIncludeCurrentStatus_IncludesUNKForFreeTextParties();
					AssertStatusShouldRecalculate_DoesNotIncludeCurrentStatus();
					AssertWhenStatusRecalculationDoesNotChangeStatus_ShipmentShouldNotRecalculate();
				});

				void AssertStatusShouldNotUpdate_WhenCurrentStatusNOT()
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
					shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

					var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();

					consignment[propertyToTest] = null;
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

					consignment[propertyToTest] = matchedPKValueForTest;
					Factory.Save();
					AssertEquals("Test1: consignment Status should not update away from NOT", "NOT", consignment.HVC_DeniedPartyScreeningStatus);
					AssertEquals("Test1: Shipment Status should not recalculate", "CLR", shipment.JS_ScreeningStatus);
				}

				void AssertStatusShouldNotUpdate_WhenCurrentStatusCLP()
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

					var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();

					consignment[propertyToTest] = null;
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

					Factory.Save();
					shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

					consignment[propertyToTest] = matchedPKValueForTest;
					Factory.Save();
					AssertEquals("Test2: consignment Status should not update away from CLP", "CLP", consignment.HVC_DeniedPartyScreeningStatus);
					AssertEquals("Test2: Shipment Status should not recalculate", "CLR", shipment.JS_ScreeningStatus);
				}

				void AssertStatusShouldRecalculate_DoesNotIncludeCurrentStatus_IncludesUNKForFreeTextParties()
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

					var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
					consignment.HVC_OA_ConsigneeAddress = ZGuid.Empty;
					consignment.HVC_OA_ShipperAddress = ZGuid.Empty;
					consignment.HVC_OA_ReturnLocation = ZGuid.Empty;

					consignment[propertyToTest] = null;
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Matched;
					Factory.Save();

					shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

					consignment[propertyToTest] = clearPKValueForTest;
					Factory.Save();
					AssertEquals("Test3: consignment Status should recalculate ignoring current status and include UNK for freetext parties", "UNK", consignment.HVC_DeniedPartyScreeningStatus);
					AssertEquals("Test3: Shipment Status should recalculate", "UNK", shipment.JS_ScreeningStatus);
				}

				void AssertStatusShouldRecalculate_DoesNotIncludeCurrentStatus()
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

					var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
					consignment.HVC_OA_ConsigneeAddress = clearOrgAddress.PK;
					consignment.HVC_OA_ShipperAddress = clearOrgAddress.PK;
					consignment.HVC_OA_ReturnLocation = clearOrgAddress.PK;
					Assert("Test4 Precondition: no free text parties included", (consignment.ShipperIsOrganisation && consignment.ConsigneeIsOrganisation && consignment.ReturnIsOrganisation));

					consignment[propertyToTest] = matchedPKValueForTest;
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Matched;
					Factory.Save();

					shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

					consignment[propertyToTest] = clearPKValueForTest;
					Factory.Save();
					AssertEquals("Test4: consignment Status should recalculate ignoring current status", "CLR", consignment.HVC_DeniedPartyScreeningStatus);
					AssertEquals("Test4: Shipment Status should recalculate", "CLR", shipment.JS_ScreeningStatus);
				}

				void AssertWhenStatusRecalculationDoesNotChangeStatus_ShipmentShouldNotRecalculate()
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

					var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
					consignment.HVC_OA_ConsigneeAddress = matchedOrgAddress.PK;
					consignment.HVC_OA_ShipperAddress = matchedOrgAddress.PK;

					consignment[propertyToTest] = matchedPKValueForTest;
					consignment.HVC_DeniedPartyScreeningStatus = ScreeningStatusesList.Codes.Matched;
					Factory.Save();

					shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

					consignment[propertyToTest] = clearPKValueForTest;
					Factory.Save();
					AssertEquals("Test5: consignment Status should not have changed", "MAT", consignment.HVC_DeniedPartyScreeningStatus);
					AssertEquals("Test5: Shipment Status should not recalculate", "CLR", shipment.JS_ScreeningStatus);
				}
			}
		}

		public void TestUpdateConsignmentHeaderDeniedPartyScreeningStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consignmentHeader = Factory.New<HVLVConsignmentHeader>();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;
			consignmentHeader.HCH_DeniedPartyScreeningStatus = "CLR";

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_DeniedPartyScreeningStatus = "MAT";

			AssertEquals("MAT", consignmentHeader.HCH_DeniedPartyScreeningStatus);

			consignment.HVC_DeniedPartyScreeningStatus = "CLR";
			AssertEquals("CLR", consignmentHeader.HCH_DeniedPartyScreeningStatus);
		}

		#endregion

		public void TestNoAuditLog()
		{
			var newFactoryForLoading = new BusinessObjectFactory();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var query = new ZQuery(StmALogSchema.SL_Parent, consignment.PK);
			Factory.Save();
			Assert("Not expecting Add event.", !newFactoryForLoading.Exists(typeof(StmALog), query));

			consignment.HVC_GoodsDescription = "TestDesc";
			Factory.Save();
			Assert("Not expecting Edit event.", !newFactoryForLoading.Exists(typeof(StmALog), query));

			consignment.Delete();
			Factory.Save();
			Assert("Not expecting Delete event.", !newFactoryForLoading.Exists(typeof(StmALog), query));
		}

		public void TestRelatedFkColumns()
		{
			var consignment = Factory.New<HVLVConsignment>();
			AssertNotNull(((IAuditParent)consignment).
							RelatedAuditChildren.
							SingleOrDefault(x => x.KeyColumn == HVLVItemSchema.HVI_HVC_Consignment && x.InfoColumn == HVLVItemSchema.HVI_ItemId));
		}

		#region IHVLVISFBillInfoProvider

		public void TestShouldImplementIHVLVISFBillInfoProvider()
		{
			var shipperAddress = Factory.New<OrgAddress>();
			shipperAddress.OA_Address1 = "Shipper Address 1";

			var consigneeAddress = Factory.New<OrgAddress>();
			consigneeAddress.OA_Address1 = "Consignee Address 1";

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			var reference = consignment.CustomsReferenceNumbers.AddNew();
			reference.CE_EntryNum = "REF00001";
			reference.CE_EntryType = "REF";

			var isfConsignment = consignment as IHVLVISFBillInfoProvider;

			CombineAssertions(() =>
			{
				AssertEquals("Should implement CustomsReferenceNumbers", "REF00001", isfConsignment.CustomsReferenceNumbers.GetAllReferenceNumbersByType("REF").First());
			});
		}

		#endregion

		#region Implementation

		class ItemReferences
		{
			public ItemReferences(string itemId, string shipperReference, string barcode)
			{
				ItemId = itemId;
				ShipperReference = shipperReference;
				Barcode = barcode;
			}

			public static ItemReferences New(string itemId, string shipperReference, string barcode) =>
				new ItemReferences(itemId, shipperReference, barcode);

			public static ItemReferences New(string shipperReference, string barcode) =>
				new ItemReferences(string.Empty, shipperReference, barcode);

			public string ItemId { get; }
			public string ShipperReference { get; }
			public string Barcode { get; }

			public override bool Equals(object obj)
			{
				var other = (ItemReferences)obj;
				return ItemId == other.ItemId && ShipperReference == other.ShipperReference && Barcode == other.Barcode;
			}

			public override int GetHashCode() => ItemId.GetHashCode() ^ ShipperReference.GetHashCode() ^ Barcode.GetHashCode();

			public override string ToString() => $@"ItemId: {ItemId}
Barcode: {Barcode}
ShipperReference: {ShipperReference}";
		}

		HVLVConsignment CreateConsignment(ZString waybillNumber, ZString shipperRef, params ItemReferences[] itemsReferences)
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = header.Consignments.AddNew();
			consignment.HVC_WaybillNumber = waybillNumber;
			consignment.HVC_ShipperReference = shipperRef;

			foreach (var itemReferences in itemsReferences)
			{
				var item = consignment.Items.AddNew();
				item.HVI_ShipperReference = itemReferences.ShipperReference;
				item.HVI_CurrentBarcode = itemReferences.Barcode;
			}

			return consignment;
		}

		public ZString GetZoneName(IRateTransportZoneHelper rateTransportZoneHelper, HVLVConsignment consignment)
		{
			var zoneName = (ZString)rateTransportZoneHelper.GetZoneName(Factory, consignment.LastMileCarrier, consignment.DestinationDepot, consignment.HVC_RN_NKConsigneeCountryCode, consignment.HVC_ConsigneePostcode, consignment.HVC_ConsigneeCity);
			return zoneName == "" ? null : zoneName;
		}

		public RateTransportProvider CreateZoneRateProvider(string zoneType, string countryCode, OrgHeader zoneOwner = null, RefCityTown zoneHubLocation = null)
		{
			var provider = Factory.New<RateTransportProvider>();
			provider.TP_ZoneType = zoneType;
			provider.TP_RN_NKCountry = countryCode;

			if (zoneOwner != null)
			{
				provider.TP_OH_RelatedParty = zoneOwner.PK;
			}

			if (zoneHubLocation != null)
			{
				provider.TP_R9_ZoneHubLocation = zoneHubLocation.PK;
			}

			return provider;
		}

		RateTransportZone CreateZone(string name, RateTransportProvider provider)
		{
			var zone = provider.Zones.AddNew();
			zone.TZ_ZoneName = name;
			return zone;
		}

		RefCityTown LoadRefCityTown(string internationalName, string state, string countryCode)
		{
			var cityTownQuery = new ZQuery()
					.AddToFilter(RefCityTownSchema.R9_InternationalName, internationalName)
					.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, countryCode)
					.AddToFilter(RefCityTownSchema.R9_RW_NKState, state);
			return Factory.Load<RefCityTown>(cityTownQuery).Single();
		}

		void AssertIDs(string consignmentName, HVLVConsignment consignment, ZString consignmentId, ZString waybillNumber, ZString shipperRef, params ItemReferences[] itemsReferences)
		{
			AssertEquals($"{consignmentName} ConsignmentID", consignmentId, consignment.HVC_ConsignmentId);
			AssertEquals($"{consignmentName} WaybillNumber", waybillNumber, consignment.HVC_WaybillNumber);
			AssertEquals($"{consignmentName} ShipperReference", shipperRef, consignment.HVC_ShipperReference);
			AssertContainsExactElementsInAnyOrder($"{consignmentName} ItemReferences",
				itemsReferences,
				consignment.Items.Cast<HVLVItem>().Select(x => ItemReferences.New(x.HVI_ItemId, x.HVI_ShipperReference, x.HVI_CurrentBarcode)));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bo = (HVLVConsignment)base.GetNewBusinessObject();
			bo.HVC_Status = HVLVConsignmentStatus.Codes.Booked;
			return bo;
		}

		HVLVConsignment SetupConsignmentWithUnits(ZString weightUQ, ZString volumeUQ)
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = weightUQ;
			consignment.HVC_VolumeUQ = volumeUQ;

			return consignment;
		}

		HVLVBookingHeader SetupBookingHeaderWithUnits(ZString weightUQ, ZString volumeUQ)
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_GrossWeightUQ = weightUQ;
			header.HVH_GrossVolumeUQ = volumeUQ;

			return header;
		}

		#endregion
	}

	public class HVLVConsignmentIdUniqueIndexFailureHandlerTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		HVLVConsignmentHeader ConsignmentHeader => consignmentHeader ?? (consignmentHeader = GetHVLVConsignmentHeaderForTest());
		HVLVConsignmentHeader consignmentHeader;

		HVLVConsignmentHeader GetHVLVConsignmentHeaderForTest()
		{
			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			consignmentHeader.HCH_ClusterKey = 1;

			return consignmentHeader;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				if (fInsertValues == null)
				{
					fInsertValues = base.AdditionalInsertValues;

					fInsertValues.Add(HVLVConsignmentSchema.HVC_IsValidatedForUniqueness.Name, "1");
					fInsertValues.Add(HVLVConsignmentSchema.HVC_HCH_Header.Name, string.Format("'{0}'", ConsignmentHeader.PK.ToString()));
					fInsertValues.Add(HVLVConsignmentSchema.HVC_ClusterKey.Name, string.Format("'{0}'", ConsignmentHeader.HCH_ClusterKey.ToString()));
				}

				return fInsertValues;
			}
		}

		NameValueCollection fInsertValues;

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			((HVLVConsignment)testBizO).HVC_IsValidatedForUniqueness = true;
			((HVLVConsignment)testBizO).HVC_HCH_Header = ConsignmentHeader.PK;
			((HVLVConsignment)testBizO).HVC_ClusterKey = ConsignmentHeader.HCH_ClusterKey;
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain => HVLVConsignmentSchema.HVC_ConsignmentId;

		protected override Type BizOTypeToTest => typeof(HVLVConsignment);

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.HVLVConsignmentId;

		public override void TestNumberFountainFix()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				base.TestNumberFountainFix();
			}
		}
	}

	[TestedType(typeof(HVLVConsignment))]
	public class HVLVConsignmentIDtbBookingParentTestCase : IDtbBookingParentTestCase<HVLVConsignment>
	{
		protected override HVLVConsignment GetNewParent()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.Items.AddNew();
			return consignment;
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking() => true;

		protected override bool CanHaveDirectCartageChild => false;
	}
}
