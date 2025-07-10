using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentHeader))]
	public class HVLVConsignmentHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateAndSaveHVLVBookingHeaderAndHVLVConsignmentHeaderTogether()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeaderForTest>();
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			AssertExceptionThrown<ValidTestDataGenerationException>(Factory.Save);
		}

		public void TestTotalWeight()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UnitOfWeight = Weight.Kilograms;

			var consignmentHeader = Factory.New<HVLVConsignmentHeader>();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_WeightUQ = Weight.Grams;
			var item = consignment.Items.AddNew();
			item.HVI_ActualWeight = 1000;

			AssertEquals("TotalWeight is calculated in memory because consignments collection is loaded", 1m, consignmentHeader.TotalWeight);

			Factory.Save();

			var updateHVLVItemWeightSQL = string.Format("Update HVLVItem set HVI_ActualWeight = 2000 where HVI_PK = '{0}'", item.PK);
			TestConnection.ExecuteNonQuery(updateHVLVItemWeightSQL);
			AssertEquals(1m, consignmentHeader.TotalWeight);

			var consignmentHeaderInNewFactory1 = new BusinessObjectFactory().Load<HVLVConsignmentHeader>(consignmentHeader.PK);
			using (TestConnection.TrackExecutedCommands())
			{
				AssertEquals(2m, consignmentHeaderInNewFactory1.TotalWeight);
				Assert("Should load from database", TestConnection.ExecutedCommands.Any(c => c.Contains("dbo.GetHVLVConsignmentHeaderTotalWeight")));
			}

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UnitOfWeight = Weight.Kilograms;

			var consignmentHeader2 = Factory.New<HVLVConsignmentHeader>();
			consignmentHeader2.HCH_JS_Shipment = shipment2.PK;

			using (TestConnection.TrackExecutedCommands())
			{
				AssertEquals("Total Weight should be calculated to 0 if consignment header is created but not saved", 0m, consignmentHeader2.TotalWeight);
				Assert("Should not load from database", !TestConnection.ExecutedCommands.Any(c => c.Contains("dbo.GetHVLVConsignmentHeaderTotalWeight")));
			}

			consignmentHeader2.Consignments.AddNew();
			Factory.Save();

			var consignmentHeaderInNewFactory2 = new BusinessObjectFactory().Load<HVLVConsignmentHeader>(consignmentHeader2.PK);

			using (TestConnection.TrackExecutedCommands())
			{
				AssertEquals("Total Weight should be calculated to 0 if the shipment is reopened but no consignment items in db", 0m, consignmentHeaderInNewFactory2.TotalWeight);
				Assert("Should load from database", TestConnection.ExecutedCommands.Any(c => c.Contains("dbo.GetHVLVConsignmentHeaderTotalWeight")));
			}
		}

		public void TestTotalVolume()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UnitOfVolume = Volume.CubicMetres;

			var consignmentHeader = Factory.New<HVLVConsignmentHeader>();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			var item = consignment.Items.AddNew();
			item.HVI_ActualVolume = 100000m;

			AssertEquals("TotalVolume is calculated in memory because consignments collection is loaded", 0.1m, consignmentHeader.TotalVolume);

			Factory.Save();

			var updateHVLVItemWeightSQL = string.Format("Update HVLVItem set HVI_ActualVolume = 200000 where HVI_PK = '{0}'", item.PK);
			TestConnection.ExecuteNonQuery(updateHVLVItemWeightSQL);
			AssertEquals(0.1m, consignmentHeader.TotalVolume);

			var consignmentHeaderInNewFactory1 = new BusinessObjectFactory().Load<HVLVConsignmentHeader>(consignmentHeader.PK);
			using (TestConnection.TrackExecutedCommands())
			{
				AssertEquals(0.2m, consignmentHeaderInNewFactory1.TotalVolume);
				Assert("Should load from database", TestConnection.ExecutedCommands.Any(c => c.Contains("dbo.GetHVLVConsignmentHeaderTotalVolume")));
			}

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UnitOfWeight = Weight.Kilograms;

			var consignmentHeader2 = Factory.New<HVLVConsignmentHeader>();
			consignmentHeader2.HCH_JS_Shipment = shipment2.PK;
			using (TestConnection.TrackExecutedCommands())
			{
				AssertEquals("Total Volume should be calculated to 0 if consignment header is created but not saved", 0m, consignmentHeader2.TotalVolume);
				Assert("Should not load from database", !TestConnection.ExecutedCommands.Any(c => c.Contains("dbo.GetHVLVConsignmentHeaderTotalVolume")));
			}

			consignmentHeader2.Consignments.AddNew();
			Factory.Save();

			var consignmentHeaderInNewFactory2 = new BusinessObjectFactory().Load<HVLVConsignmentHeader>(consignmentHeader2.PK);

			using (TestConnection.TrackExecutedCommands())
			{
				AssertEquals("Total Weight should be calculated to 0 if the shipment is reopened but no consignment items in db", 0m, consignmentHeaderInNewFactory2.TotalVolume);
				Assert("Should load from database", TestConnection.ExecutedCommands.Any(c => c.Contains("dbo.GetHVLVConsignmentHeaderTotalVolume")));
			}
		}

		public void TestIgnoreAssigningEmptyClusterKey()
		{
			var hvlShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = hvlShipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.HCH_ClusterKey = 123;
			AssertEquals("Precondition: consignmentHeader's clusterKey isn't empty", 123, consignmentHeader.HCH_ClusterKey);

			consignmentHeader.HCH_ClusterKey = ZInt.Zero;
			AssertEquals("ConsignmentHeader's clusterKey should not be emptied", 123, consignmentHeader.HCH_ClusterKey);
		}

		public void TestClusterKeyCascadesDownToConsignmentsAndOverrideExistingHVC_ClusterKey()
		{
			var hvlShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = hvlShipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_ClusterKey = 321;

			Assert("Precondition: Consignment already have a non Zero clusterKey",
				!consignment.HVC_ClusterKey.IsEmpty);

			consignmentHeader.HCH_ClusterKey = 123;
			AssertEquals("Consignment's clusterKey should be same as Header's", 123, consignment.HVC_ClusterKey);
		}

		public void TestSaving_HCH_JobNumberComesFromHVLVConsignmentHeaderJobNumberFountain()
		{
			var hvlShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = hvlShipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.HCH_JobNumber = "";
			Env.NumberFountains.HVLVConsignmentHeaderJobNumber.SetNext(Factory, 890092);

			Factory.Save();

			AssertEquals("Booking reference should be set to 'HCH' + JobNumber with 0's padded to the left to a total 12 numnbers", "HCH000000890092", consignmentHeader.HCH_JobNumber);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestHCHClusterKeyCanBeGenerated_WhenDuplicateKeyConflicts()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			var shipment1 = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader1 = shipment1.GetOrCreateHVLVConsignmentHeader();

			Factory.Save();

			AssertEquals(1, bookingHeader.HVH_ClusterKey);
			AssertEquals(2, consignmentHeader1.HCH_ClusterKey);

			consignmentHeader1.HCH_ClusterKey = 3;
			bookingHeader.HVH_ClusterKey = 4;

			Factory.Save();

			var shipment2 = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader2 = shipment2.GetOrCreateHVLVConsignmentHeader();

			try
			{
				Factory.Save();
				Fail("Factory Save should fail");
			}
			catch (ZSaveException ex)
			{
				AssertContains("The value of HVLVConsignmentHeader|HCH_ClusterKey must be unique on HVLVConsignmentHeader. The duplicate value(s) are: (3).", ex.FriendlyMessage);

				ZExceptionReporting.HandleSaveException(ex);
				Factory.Save();
			}

			AssertEquals("NumberFountain should spit 5 as HVLVBookingHeader ClusterKeys should be counted as well", 5, consignmentHeader2.HCH_ClusterKey);
		}

		[UseSnapshotProtection(true)]
		public void TestSaving_WillPopulateIds_UsingGS1Info()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "Consignor";
			HVLVTestHelper.SetGS1FountainOnOrg(consignor, "12345678");

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.ConsignorPK = consignor.PK;
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

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

			Factory.Save();

			CombineAssertions("consignments and items should be populated", () =>
			{
				AssertEquals("012345678000000011", consignment1.HVC_ConsignmentId);
				AssertEquals("012345678000000028", consignment2.HVC_ConsignmentId);
				AssertEquals("012345678000000035", item1.HVI_ItemId);
				AssertEquals("012345678000000042", item2.HVI_ItemId);
				AssertEquals("012345678000000059", item3.HVI_ItemId);
			});
		}

		public void TestGetOrCreateConsignmentHeader()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var createdHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
			AssertEquals("Created consignment header should be linked to shipment", shipment.PK, createdHeader.HCH_JS_Shipment);

			var loadedHeader = HVLVConsignmentHeader.Get(shipment);
			AssertEquals("Should load the same consignment header", createdHeader.PK, loadedHeader.PK);
		}

		public void TestShipment()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var header = HVLVConsignmentHeader.GetOrCreate(shipment);
			AssertEquals("Should load the parent shipment", shipment.PK, header.Shipment.PK);
		}

		public void TestHasNoItemsByDefault()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();

			AssertEquals(0, consignment.Items.Count);
		}

		public void TestConsignmentsForStandAloneDeclarationConversionWrapperCollection()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			BuildImportConsignment(header, HVLVReleaseStatus.Held);
			BuildImportConsignment(header, HVLVReleaseStatus.Held);
			BuildImportConsignment(header, HVLVReleaseStatus.Held);

			AssertEquals("Expected one consignment in ConsignmentsToStandAloneDeclaration collection", 3, header.ConsignmentsConvertToStandAloneDeclaration.Count);
		}

		public void TestConsignmentsForStandAloneDeclarationConversionWrapperCollection_ShouldOnlyContainUnclearedConsignments()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = BuildImportConsignment(header, HVLVReleaseStatus.None);
			var consignment2 = BuildImportConsignment(header, HVLVReleaseStatus.Cleared);
			var consignment3 = BuildImportConsignment(header, HVLVReleaseStatus.Held);

			AssertContainsExactElementsInAnyOrder(new List<HVLVConsignment> { consignment1, consignment3 }, header.ConsignmentsConvertToStandAloneDeclaration.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().Select(t => t.Consignment));
		}

		public void TestConsignmentsToConvertToStandAloneDeclarationView()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			BuildImportConsignment(header, HVLVReleaseStatus.Held);
			BuildImportConsignment(header, HVLVReleaseStatus.Held);
			BuildImportConsignment(header, HVLVReleaseStatus.Held);

			AssertEquals("Expected three consignments in ConsignmentsConvertToStandAloneDeclarationCollectionView", 3, header.ConsignmentsConvertToStandAloneDeclarationView.Count);
		}

		public void TestConsignmentsToConvertToStandAloneDeclarationView_ShouldOnlyContainUnclearedConsignments()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = BuildImportConsignment(header, HVLVReleaseStatus.None);
			var consignment2 = BuildImportConsignment(header, HVLVReleaseStatus.Cleared);
			var consignment3 = BuildImportConsignment(header, HVLVReleaseStatus.Held);

			AssertContainsExactElementsInAnyOrder(new List<HVLVConsignment> { consignment1, consignment3 }, header.ConsignmentsConvertToStandAloneDeclarationView.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().Select(t => t.Consignment));
		}

		public void TestConsignmentCountWithoutLoading()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			header.Consignments.AddNew();
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loadedHeader = factory.Load<HVLVConsignmentHeader>(header.PK);

			var iHeader = (IHVLVConsignmentHeader)loadedHeader;

			var query = new ZQuery(HVLVConsignmentSchema.HVC_HCH_Header, loadedHeader.PK);
			query.FetchOnlyFromLocalCache = true;

			AssertEquals("Should be 3 consignments", 3, iHeader.ConsignmentCountWithoutLoading);
			AssertEquals("No HVLVConsignment should be loaded.", 0, factory.Load<HVLVConsignment>(query).Length);
		}

		public void TestCollectionContainsAllChildrenAttachedToShipment()
		{
			var shipment1 = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			Func<ForwardingShipment, ForwardingShipment, HVLVConsignment> createConsignmentWithItem = (consignmentShipment, itemShipment) =>
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = consignmentShipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = itemShipment.PK;

				return consignment;
			};

			Action<HVLVConsignment, bool, bool> assertReadOnly = (consignment, consignmentReadOnly, itemReadOnly) =>
			{
				var item = consignment.Items.Single();

				AssertEquals(consignmentReadOnly, consignment.ReadOnly);
				AssertEquals(itemReadOnly, item.ReadOnly);

				var consignmentRowWarningAssertion = consignmentReadOnly ? (Action<BusinessObject, string>)AssertHasRowWarning : AssertNoRowWarningContaining;
				var itemRowWarningAssertion = itemReadOnly ? (Action<BusinessObject, string>)AssertHasRowWarning : AssertNoRowWarningContaining;

				consignmentRowWarningAssertion(consignment, $"{consignment.HumanReadableName} was not manifested against {consignment.ManagingShipment.HumanReadableName}.");
				itemRowWarningAssertion(item, $"{item.HumanReadableName} belongs to {consignment.HumanReadableName}, but is not attached to {consignment.ManagingShipment.HumanReadableName}.");
			};

			var consignment1 = createConsignmentWithItem(shipment1, shipment1);
			var consignment2 = createConsignmentWithItem(shipment1, shipment2);
			var consignment3 = createConsignmentWithItem(shipment2, shipment1);
			var consignment4 = createConsignmentWithItem(shipment2, shipment2);

			Factory.Save();
			ReleaseFactory();

			shipment1 = Factory.Load<HVLVForwardingShipment>(shipment1.PK);
			consignment1 = Factory.Load<HVLVConsignment>(consignment1.PK);
			consignment2 = Factory.Load<HVLVConsignment>(consignment2.PK);
			consignment3 = Factory.Load<HVLVConsignment>(consignment3.PK);

			var header = shipment1.GetOrCreateHVLVConsignmentHeader();
			AssertContainsExactElementsInAnyOrder(new[] { consignment1, consignment2, consignment3 }, header.Consignments);

			assertReadOnly(consignment1, false, false);
			assertReadOnly(consignment2, false, true);
			assertReadOnly(consignment3, true, false);
		}

		public void TestHasActiveConsignments_WithNoConsignments()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			Assert("HasActiveConsignments is false", !header.HasActiveConsignments);
		}

		public void TestHasActiveConsignments_WithConsignments()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = header.Consignments.AddNew();

			Assert("precondition", consignment1.HVC_IsActive);
			Assert("HasActiveConsignments", header.HasActiveConsignments);

			consignment1.HVC_IsActive = false;
			Assert("precondition", !consignment1.HVC_IsActive);

			Assert("HasActiveConsignments is false", !header.HasActiveConsignments);

			var consignment2 = header.Consignments.AddNew();

			Assert("precondition", consignment2.HVC_IsActive);
			Assert("HasActiveConsignments", header.HasActiveConsignments);
		}

		public void TestShipmentCounts()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();

			CombineAssertions("Counts should be zero when no items", () =>
			{
				AssertEquals("Item Count", 0, header.ItemCount);
				AssertEquals("Import Cleared Count", 0, header.ImportClearedCount);
				AssertEquals("Import Held Count", 0, header.ImportHeldCount);
				AssertEquals("Import None Reported Count", 0, header.ImportNoneReportedCount);
				AssertEquals("Export Cleared Count", 0, header.ExportClearedCount);
				AssertEquals("Export Held Count", 0, header.ExportHeldCount);
				AssertEquals("Export None Reported Count", 0, header.ExportNoneReportedCount);
				AssertEquals("Surplus Count", 0, header.SurplusCount);
				AssertEquals("Short Count", 0, header.ShortCount);
				AssertEquals("Delivered Count", 0, header.DeliveredCount);
				AssertEquals("Scanned Count", 0, header.ScannedCount);
				AssertEquals("Scanned Cleared Count", 0, header.ScannedClearedCount);
				AssertEquals("Scanned Held Count", 0, header.ScannedHeldCount);
				AssertEquals("Scanned None Reported Count", 0, header.ScannedNoneReportedCount);
			});

			var item1 = consignment.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var item2 = consignment.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;

			var item3 = consignment.Items.AddNew();
			item3.HVI_JS_LoadedOnShipment = shipment.PK;
			item3.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;

			var item4 = consignment.Items.AddNew();
			item4.HVI_JS_LoadedOnShipment = shipment.PK;
			item4.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;

			var item5 = consignment.Items.AddNew();
			item5.HVI_JS_LoadedOnShipment = shipment.PK;
			item5.HVI_Status = HVLVItemStatus.Codes.SurplusAtDestinationDepot;

			var item6 = consignment.Items.AddNew();
			item6.HVI_JS_LoadedOnShipment = shipment.PK;
			item6.HVI_IsScannedAtDestination = true;

			var item7 = consignment.Items.AddNew();
			item7.HVI_JS_LoadedOnShipment = shipment.PK;
			item7.HVI_IsScannedAtDestination = true;
			item7.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;

			var item8 = consignment.Items.AddNew();
			item8.HVI_JS_LoadedOnShipment = shipment.PK;
			item8.HVI_IsScannedAtDestination = true;
			item8.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;

			var item9 = consignment.Items.AddNew();
			item9.HVI_JS_LoadedOnShipment = shipment.PK;
			item9.HVI_IsScannedAtDestination = true;
			item9.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;

			var item10 = consignment.Items.AddNew();
			item10.HVI_JS_LoadedOnShipment = shipment.PK;
			item10.HVI_Status = HVLVItemStatus.Codes.ShortShippedAtDestinationDepot;

			var item11 = consignment.Items.AddNew();
			item11.HVI_JS_LoadedOnShipment = shipment.PK;
			item11.HVI_Status = HVLVItemStatus.Codes.Delivered;

			Factory.Save();

			CombineAssertions("Shipment counts are correctly calculated", () =>
			{
				AssertEquals("Item Count", 11, header.ItemCount);
				AssertEquals("Import Cleared Count", 1, header.ImportClearedCount);
				AssertEquals("Import Held Count", 1, header.ImportHeldCount);
				AssertEquals("Import None Reported Count", 9, header.ImportNoneReportedCount);
				AssertEquals("Export Cleared Count", 1, header.ExportClearedCount);
				AssertEquals("Export Held Count", 1, header.ExportHeldCount);
				AssertEquals("Export None Reported Count", 9, header.ExportNoneReportedCount);
				AssertEquals("Surplus Count", 1, header.SurplusCount);
				AssertEquals("Short Count", 1, header.ShortCount);
				AssertEquals("Delivered Count", 1, header.DeliveredCount);
				AssertEquals("Scanned Count", 5, header.ScannedCount);
				AssertEquals("Scanned Cleared Count", 1, header.ScannedClearedCount);
				AssertEquals("Scanned Held Count", 1, header.ScannedHeldCount);
				AssertEquals("Scanned None Reported Count", 3, header.ScannedNoneReportedCount);
			});
		}

		public void TestShipmentCounts_OnlyIncludesActiveItems()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var archivedShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var archivedHeader = archivedShipment.GetOrCreateHVLVConsignmentHeader();
			archivedHeader.HCH_IsArchived = true;
			var archivedConsignment = archivedHeader.Consignments.AddNew();

			var consignment = header.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item1.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;

			var item2 = consignment.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;

			var item3 = consignment.Items.AddNew();
			item3.HVI_JS_LoadedOnShipment = shipment.PK;
			item3.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;

			var item4 = consignment.Items.AddNew();
			item4.HVI_JS_LoadedOnShipment = shipment.PK;
			item4.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;

			var item5 = consignment.Items.AddNew();
			item5.HVI_JS_LoadedOnShipment = shipment.PK;
			item5.HVI_Status = HVLVItemStatus.Codes.SurplusAtDestinationDepot;
			item5.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;

			var item6 = consignment.Items.AddNew();
			item6.HVI_JS_LoadedOnShipment = shipment.PK;
			item6.HVI_IsScannedAtDestination = true;
			item6.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;

			var item7 = consignment.Items.AddNew();
			item7.HVI_JS_LoadedOnShipment = shipment.PK;
			item7.HVI_IsScannedAtDestination = true;
			item7.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
			item7.HVI_IsActive = false;

			var item8 = archivedConsignment.Items.AddNew();
			item8.HVI_JS_LoadedOnShipment = archivedShipment.PK;
			item8.HVI_IsScannedAtDestination = true;
			item8.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;

			var item9 = consignment.Items.AddNew();
			item9.HVI_JS_LoadedOnShipment = shipment.PK;
			item9.HVI_IsScannedAtDestination = true;
			item9.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
			item9.HVI_IsActive = false;

			var item10 = consignment.Items.AddNew();
			item10.HVI_JS_LoadedOnShipment = shipment.PK;
			item10.HVI_Status = HVLVItemStatus.Codes.ShortShippedAtDestinationDepot;

			var item11 = consignment.Items.AddNew();
			item11.HVI_JS_LoadedOnShipment = shipment.PK;
			item11.HVI_Status = HVLVItemStatus.Codes.Delivered;

			Factory.Save();

			CombineAssertions("Shipment counts do not include inactive or archived items", () =>
			{
				AssertEquals("Item Count", 8, header.ItemCount);
				AssertEquals("Import Cleared Count", 1, header.ImportClearedCount);
				AssertEquals("Import Held Count", 1, header.ImportHeldCount);
				AssertEquals("Import None Reported Count", 6, header.ImportNoneReportedCount);
				AssertEquals("Export Cleared Count", 1, header.ImportClearedCount);
				AssertEquals("Export Held Count", 1, header.ImportHeldCount);
				AssertEquals("Export None Reported Count", 6, header.ImportNoneReportedCount);
				AssertEquals("Surplus Count", 1, header.SurplusCount);
				AssertEquals("Short Count", 1, header.ShortCount);
				AssertEquals("Delivered Count", 1, header.DeliveredCount);
				AssertEquals("Scanned Count", 2, header.ScannedCount);
				AssertEquals("Scanned Cleared Count", 0, header.ScannedClearedCount);
				AssertEquals("Scanned Held Count", 0, header.ScannedHeldCount);
				AssertEquals("Scanned None Reported Count", 2, header.ScannedNoneReportedCount);
				AssertEquals("ArchivedHeader Scanned Held Count", 1, archivedHeader.ScannedHeldCount);
			});
		}

		public void TestSavingConsignments_ShouldUseBulkSavingWhenPassThreshold()
		{
			var hvlShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = hvlShipment.GetOrCreateHVLVConsignmentHeader();

			HVLVTestHelper.CreateDummyConsignments(consignmentHeader.Consignments, Factory.DefaultBulkCopyThreshold() + 1);

			using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
			{
				AssertNoExceptionThrown(Factory.Save);
				Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVConsignmentSchema.Constants.TableName, ["FireTriggers"]));
			}
		}

		public void TestBulkSaving_NoExceptionsThrown()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
			{
				var consignment = consignmentHeader.Consignments.AddNew();
			}

			for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
			{
				var item = consignmentHeader.Consignments[0].Items.AddNew();
			}

			for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
			{
				var itemLine = consignmentHeader.Consignments[0].Items[0].Lines.AddNew();
			}

			for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
			{
				var outerPackage = Factory.New<HVLVOuterPackage>();
				consignmentHeader.Consignments[0].Items[0].HVI_HVO_OuterPackage = outerPackage.PK;
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

		public void TestClusterKeyOnlySetOnConsignmentsBelongingToHeader()
		{
			var shipmentWithBothItems = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentWithBothItems.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var shipmentWithNoItems = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentWithNoItems.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var headerWithBothItems = shipmentWithBothItems.GetOrCreateHVLVConsignmentHeader();
			var consignment = headerWithBothItems.Consignments.AddNew();
			var item = consignment.Items.AddNew();

			var headerWithNoItems = shipmentWithNoItems.GetOrCreateHVLVConsignmentHeader();
			var otherConsignment = headerWithNoItems.Consignments.AddNew();
			var otherItem = otherConsignment.Items.AddNew();

			headerWithBothItems.HCH_ClusterKey = 123;
			headerWithNoItems.HCH_ClusterKey = 456;

			item.HVI_JS_LoadedOnShipment = shipmentWithBothItems.PK;
			otherItem.HVI_JS_LoadedOnShipment = shipmentWithBothItems.PK;

			Factory.Save();

			var differentFactory = new BusinessObjectFactory();
			var reloadedHeader = differentFactory.Load<HVLVConsignmentHeader>(headerWithBothItems.PK);

			AssertEquals("Precondition: there are 2 consignments on this header", 2, reloadedHeader.Consignments.Count);

			reloadedHeader.HCH_ClusterKey = 789;
			var reloadedConsignment = differentFactory.Load<HVLVConsignment>(consignment.PK);
			var reloadedOtherConsignment = differentFactory.Load<HVLVConsignment>(otherConsignment.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment belonging to this header will have its cluster key changed", 789, reloadedConsignment.HVC_ClusterKey);
				AssertEquals("Other consignment will not have its cluster key changed", 456, reloadedOtherConsignment.HVC_ClusterKey);
			});
		}

		public void TestShouldNotSaveIfShipmentIsNoLongerHVLVAndHeaderIsNotSaved()
		{
			void AssertNoHeaderRecordsExist()
			{
				AssertEquals(0, Factory.Load<HVLVConsignmentHeader>(new ZQuery()).Length);
			}

			AssertNoHeaderRecordsExist();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			var header = Factory.New<HVLVConsignmentHeader>();
			header.HCH_JS_Shipment = shipment.PK;
			Factory.Save();

			AssertNoHeaderRecordsExist();
		}

		public void TestConsignmentLoading_WhenDataBinding_DoNotLoadIfCountIsLargerThanThreshold()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignmentHeader.Consignments.AddNew();
			consignmentHeader.Consignments.AddNew();

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
				var consignmentHeaderInNewFactory = new BusinessObjectFactory().Load<HVLVConsignmentHeader>(consignmentHeader.PK);

				using (useSetIsDataBinding
					? ((IHVLVConsignmentCollectionParent)consignmentHeaderInNewFactory).SetIsDataBinding()
					: null
					)
				{
					AssertEquals(message, expectConsignmentsToBeLoaded, consignmentHeaderInNewFactory.Consignments.Count > 0);
				}
			}
		}

		public void TestConsignmentsNotLoaded()
		{
			var hvlShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = hvlShipment.GetOrCreateHVLVConsignmentHeader();
			header.Consignments.AddNew();
			Factory.Save();

			var headerInNewFactory = NewFactory().Load<HVLVConsignmentHeader>(header.PK);
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

		public void TestShipmentItemsCountViewIsNotNullAfterItemsAreAdded()
		{
			var shipment = Factory.New<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			Factory.Save();

			AssertNull("Precondition", header.ShipmentItemsCountView);

			header.Consignments.AddNew().Items.AddNew();
			Factory.Save();

			AssertNotNull(header.ShipmentItemsCountView);
		}

		public void TestCustomsJobCanNotCancelReason()
		{
			PrepareDataForCanCancelCustomJobs(out var consignmentHeader);
			Assert("A cannot cancel reason is attached to consignment header", consignmentHeader.CustomsJobCanNotCancelReason.Any());
		}

		public void TestCancellableCustomsJobs()
		{
			PrepareDataForCanCancelCustomJobs(out var consignmentHeader);
			Assert("A cancellable customs job is attached to consignment header", consignmentHeader.CancellableCustomsJobs.Any());
		}

		public void TestCancellableCustomsJobsIsNotCached()
		{
			PrepareDataForCanCancelCustomJobs(out var consignmentHeader);
			AssertEquals("A cancellable customs job is attached to consignment header", 1, consignmentHeader.CancellableCustomsJobs.Count);

			CreateCustomsJobForHeader(consignmentHeader);
			AssertEquals("Two cancellable customs job is attached to consignment header", 2, consignmentHeader.CancellableCustomsJobs.Count);
		}

		void PrepareDataForCanCancelCustomJobs(out HVLVConsignmentHeader consignmentHeader)
		{
			consignmentHeader = Factory.New<HVLVConsignmentHeader>();

			var customJob = CreateCustomsJobForHeader(consignmentHeader);

			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();

			customJob.CM_JK = consol.PK;
		}

		CusMAWB CreateCustomsJobForHeader(HVLVConsignmentHeader consignmentHeader)
		{
			var genPivot = Factory.New<IGenPivot>();
			genPivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
			genPivot.XX_Relation1ID = consignmentHeader.PK;
			genPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;

			var customJob = Factory.New<CusMAWB>();
			genPivot.XX_Relation2TableCode = CusMAWBSchema.Constants.Prefix;
			genPivot.XX_Relation2ID = customJob.PK;
			return customJob;
		}

		public void TestPopulateUsageType_ShipmentDestinationIsUSWithUSBranch()
		{
			AssertPopulateUsageType(CountryCodes.UnitedStates, false, HVLVConstants.HVLVConsignmentHeaderUsageTypesCodes.Plus);
		}

		public void TestPopulateUsageType_ShipmentDestinationIsUSWithAUBranchAndRegistryDisabled()
		{
			AssertPopulateUsageType(CountryCodes.Australia, false, HVLVConstants.HVLVConsignmentHeaderUsageTypesCodes.Standard);
		}

		public void TestPopulateUsageType_ShipmentDestinationIsUSWithAUBranchAndRegistryEnabled()
		{
			AssertPopulateUsageType(CountryCodes.Australia, true, HVLVConstants.HVLVConsignmentHeaderUsageTypesCodes.Plus);
		}

		void AssertPopulateUsageType(string currentCountryCode, bool registrySettingEnabled, string expectedUsageType)
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(currentCountryCode))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySettingEnabled))
			{
				var shipment = Factory.New<HVLVForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				Factory.Save();

				AssertEquals("Consignment Header Usage Type", expectedUsageType, header.HCH_UsageType);
			}
		}

		public void TestOnSavingDeleteConsignmentHeaderWhenShipmentIsNull()
		{
			var header = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			header.HCH_JS_Shipment = Guid.Empty;

			Factory.Save();
			Assert(header.IsDeleted);
		}

		public void TestHasHVLVDataCreated()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = "HVL";
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			Assert("HasHVLVDataCreated is false if consignment header is not in DB nor has no consignment", !consignmentHeader.HasHVLVDataCreated);

			consignmentHeader.Consignments.AddNew();
			Assert("HasHVLVDataCreated is true if consignment header has consignment", consignmentHeader.HasHVLVDataCreated);

			consignmentHeader.Consignments.DeleteAll();
			Factory.Save();

			Assert("HasHVLVDataCreated is true if consignment header is in DB", consignmentHeader.HasHVLVDataCreated);
		}

		public void TestOnConsolChanged_UpdatesItemsOuterPackageToArrivalConsol()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = "HVL";

				var outerPackage1 = Factory.New<HVLVOuterPackage>();
				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				var consignment = header.Consignments.AddNew();
				var item1 = consignment.Items.AddNew();
				item1.HVI_JS_LoadedOnShipment = shipment.PK;
				item1.HVI_HVO_OuterPackage = outerPackage1.PK;

				var outerPackage2 = Factory.New<HVLVOuterPackage>();
				var item2 = consignment.Items.AddNew();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;
				item2.HVI_HVO_OuterPackage = outerPackage2.PK;

				CombineAssertions("Pre condition", () =>
				{
					Assert(item1.OuterPackage.HVO_JK_LoadedOnConsol.IsEmpty);
					Assert(item1.OuterPackage.HVO_JK_LoadedOnConsol.IsEmpty);
				});

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

				var hvlvConsignmentHeader = header as IHVLVConsignmentHeader;
				hvlvConsignmentHeader.OnConsolChanged();
				Factory.Save();

				CombineAssertions("HVO_JK_LoadedOnConsol is populated", () =>
				{
					AssertEquals(arrivalConsol.PK, item1.OuterPackage.HVO_JK_LoadedOnConsol);
					AssertEquals(arrivalConsol.PK, item2.OuterPackage.HVO_JK_LoadedOnConsol);
				});

				shipment.Consols.RemoveAndDeleteAll();
				((IHVLVConsignmentHeader)header).OnConsolChanged();

				CombineAssertions("Sets to Empty if all consols deleted", () =>
				{
					AssertEquals(ZGuid.Empty, item1.OuterPackage.HVO_JK_LoadedOnConsol);
					AssertEquals(ZGuid.Empty, item2.OuterPackage.HVO_JK_LoadedOnConsol);
				});
			}
		}

		public void TestOnConsolChanged_DoesNotSetItemOutpackageLoadedOnConsolIfPopulated()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.NewZealand))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = "HVL";

				var outerPackage1 = Factory.New<HVLVOuterPackage>();
				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				var consignment = header.Consignments.AddNew();
				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				item.HVI_HVO_OuterPackage = outerPackage1.PK;

				var existingConsol = Factory.New<ForwardingConsol>();
				existingConsol.JK_RL_NKLoadPort = "AUSYD";
				existingConsol.JK_RL_NKDischargePort = "NZAKL";
				item.OuterPackage.HVO_JK_LoadedOnConsol = existingConsol.PK;

				Factory.Save();

				CombineAssertions("Preconditions - HVO_JK_LoadedOnConsol is populated", () =>
				{
					AssertEquals(existingConsol.PK, item.OuterPackage.HVO_JK_LoadedOnConsol);
				});

				var arrivalConsol = shipment.Consols.AddNew();
				arrivalConsol.JK_RL_NKLoadPort = "AUSYD";
				arrivalConsol.JK_RL_NKDischargePort = "NZAKL";
				var hvlvConsignmentHeader = header as IHVLVConsignmentHeader;
				hvlvConsignmentHeader.OnConsolChanged();
				Factory.Save();

				CombineAssertions("Post conditions - HVO_JK_LoadedOnConsol is not changed", () =>
				{
					AssertNotEquals(arrivalConsol.PK, item.OuterPackage.HVO_JK_LoadedOnConsol);
					AssertEquals(existingConsol.PK, item.OuterPackage.HVO_JK_LoadedOnConsol);
				});
			}
		}

		#region IScreeningPartyProvider Members

		public void TestGetWorstScreeningStatus()
		{
			var consignmentHeader = Factory.New<HVLVConsignmentHeader>();
			consignmentHeader.HCH_DeniedPartyScreeningStatus = "MAT";
			AssertEquals("MAT", ((IScreeningPartyProvider)consignmentHeader).GetWorstScreeningStatus());
		}

		public void TestGetWorstScreeningStatusUnlessManuallyCleared()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var shipmentScreeningPartyProvider = shipment as IScreeningPartyProvider;
			var proxyScreeningPartyProvider = header as IScreeningPartyProvider;
			AssertEquals(shipmentScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared(), proxyScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared());
		}

		public void TestScreeningParties()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();

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

			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_OA_DestinationDepot = destinationDepotAddress.PK;
			consignment.HVC_OA_ReturnLocation = returnLocationAddress.PK;
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;

			Factory.Save();

			var proxyScreeningPartyProvider = header as IScreeningPartyProvider;
			var screeningParties = proxyScreeningPartyProvider.ScreeningParties;
			var shipperScreeningParty = screeningParties.Single(x => x.OrgCode == "SHIPPER");
			var consigneeScreeningParty = screeningParties.Single(x => x.OrgCode == "CONSIGNEE");
			var destinationDepotScreeningParty = screeningParties.Single(x => x.OrgCode == "DEPOT");
			var returnLocationScreeningParty = screeningParties.Single(x => x.OrgCode == "RETURN");
			var lastMileCarrierScreeningParty = screeningParties.Single(x => x.OrgCode == "CARRIER");

			CombineAssertions(() =>
			{
				AssertEquals(5, screeningParties.Length);
				AssertEquals("Shipper", shipperScreeningParty.Description);
				AssertEquals("Consignee", consigneeScreeningParty.Description);
				AssertEquals("Destination Depot", destinationDepotScreeningParty.Description);
				AssertEquals("Return Location", returnLocationScreeningParty.Description);
				AssertEquals("Last Mile Carrier", lastMileCarrierScreeningParty.Description);
			});
		}

		public void TestScreeningParties_FreeTextShipperAndConsigneeAndReturn()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();

			consignment.HVC_ShipperName = "LINUS";
			consignment.HVC_ConsigneeName = "AMAZON";
			consignment.HVC_ReturnName = "LUCY";

			Factory.Save();

			var proxyScreeningPartyProvider = header as IScreeningPartyProvider;
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
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var shipmentScreeningPartyProvider = shipment as IScreeningPartyProvider;
			var proxyScreeningPartyProvider = header as IScreeningPartyProvider;
			AssertEquals("Precondition: screening status should be mirrored on header", shipmentScreeningPartyProvider.ScreeningStatus, proxyScreeningPartyProvider.ScreeningStatus);

			(shipment as IScreeningPartyProvider).ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertEquals("Screening status change should be reflected on header", ScreeningStatusesList.Codes.Matched, proxyScreeningPartyProvider.ScreeningStatus);
		}

		#endregion

		#region Show Import/Export Properties

		public void TestShowExportOrImport()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Australia);

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			CombineAssertions(() =>
			{
				Assert(header.ShowExport);
				Assert(!header.ShowImport);
			});

			shipment.JS_RL_NKDestination = "AUSYD";
			CombineAssertions(() =>
			{
				Assert(!header.ShowExport);
				Assert(header.ShowImport);
			});

			shipment.JS_RL_NKOrigin = "USLAX";
			CombineAssertions(() =>
			{
				Assert(!header.ShowExport);
				Assert(shipment.IsImport());
			});
		}

		#endregion

		#region Implementation

		BusinessObject GetNewConsignmentHeader()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			return shipment.GetOrCreateHVLVConsignmentHeader();
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewConsignmentHeader();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewConsignmentHeader();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewConsignmentHeader();

		HVLVConsignment BuildImportConsignment(HVLVConsignmentHeader header, string importReleaseStatus)
		{
			var consignment = header.Consignments.AddNew();
			consignment.HVC_ImportReleaseStatus = importReleaseStatus;
			consignment.HVC_RN_NKShipperCountryCode = "NZ";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			return consignment;
		}

		#endregion

		#region IHVLVConsignmentHeader

		public void TestScanTimePropertiesShouldBeExposed()
		{
			var header = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			header.HCH_ScanStartTime = new ZDateTimeOffset(2024, 11, 1);
			header.HCH_ScanCompleteTime = new ZDateTimeOffset(2024, 11, 2);

			var iHeader = header as IHVLVConsignmentHeader;

			CombineAssertions(() =>
			{
				AssertEquals(new ZDateTimeOffset(2024, 11, 1), iHeader.HCH_ScanStartTime);
				AssertEquals(new ZDateTimeOffset(2024, 11, 2), iHeader.HCH_ScanCompleteTime);
			});
		}
		#endregion
	}

	public class HVLVConsignmentHeaderUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override SchemaColumn ColumnThatUsesNumberFountain => HVLVConsignmentHeaderSchema.HCH_ClusterKey;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.HVLVConsignmentClusterKey;

		protected override Type BizOTypeToTest => typeof(HVLVConsignmentHeader);

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			var shipment = CreateShipment();
			((HVLVConsignmentHeader)testBizO).HCH_JS_Shipment = shipment.PK;
			shipment.JS_ShipmentType = "HVL";
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				if (fInsertValues == null)
				{
					fInsertValues = base.AdditionalInsertValues;

					var shipment = CreateShipment();
					Factory.Save();

					fInsertValues.Add(HVLVConsignmentHeaderSchema.HCH_JS_Shipment.Name, string.Format("'{0}'", shipment.PK.ToString()));
					fInsertValues.Add(HVLVConsignmentHeaderSchema.HCH_JobNumber.Name, "'RealHCHJobNumber'");
				}

				return fInsertValues;
			}
		}

		NameValueCollection fInsertValues;

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			return (ForwardingShipment)shipment;
		}
	}
}
