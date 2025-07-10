using System;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(CommonContainerLoadList))]
	sealed class CommonContainerLoadListTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.CommonContainerLoadList);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Factory.NewWithValidTestData<CommonContainerLoadList>();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		public void TestJobNumber()
		{
			var containerLoadList = Factory.NewWithValidTestData<CommonContainerLoadList>();
			containerLoadList.CLH_LoadListId = ZString.Empty;
			AssertEquals("The JobNumber should be empty as CLH_LoadListId is empty", ZString.Empty, containerLoadList.JobNumber);
			containerLoadList.CLH_LoadListId = "CLH0001";
			AssertEquals("The JobNumber should be valid", containerLoadList.CLH_LoadListId, containerLoadList.JobNumber);
		}

		#region QtyPacked

		public void TestQtyPackedForDelete()
		{
			foreach (var loadMode in typeof(ContainerLoadListHeaderLoadMode).GetFields().Select(x => x.GetValue(null)).Cast<string>())
			{
				var bookingLine = OrderManagerTestHelper.PrepareSupplierBookingLineForQtyPacked(Factory);
				var loadListHeader1 = OrderManagerTestHelper.PrepareContainerLoadListForQtyPacked(Factory, bookingLine, 8, loadMode);
				var loadListHeader2 = OrderManagerTestHelper.PrepareContainerLoadListForQtyPacked(Factory, bookingLine, 3, loadMode);

				Factory.Save();

				var orderLine = bookingLine.OrderLine;
				AssertEquals(0m, orderLine.JO_QtyPacked);

				loadListHeader1.CLH_Status = ContainerLoadListHeaderStatus.Converted;
				loadListHeader2.CLH_Status = ContainerLoadListHeaderStatus.Converted;
				AssertEquals(loadMode, 11m, orderLine.JO_QtyPacked);

				loadListHeader1.Delete();
				AssertEquals(loadMode, 3m, orderLine.JO_QtyPacked);

				loadListHeader2.Delete();
				AssertEquals(loadMode, 0m, orderLine.JO_QtyPacked);
			}
		}

		public void TestQtyPackedForStatus()
		{
			foreach (var loadMode in typeof(ContainerLoadListHeaderLoadMode).GetFields().Select(x => x.GetValue(null)).Cast<string>())
			{
				foreach (ICodeDescription originalStatus in new CommonContainerLoadListStatusList())
				{
					foreach (ICodeDescription newStatus in new CommonContainerLoadListStatusList())
					{
						var bookingLine = OrderManagerTestHelper.PrepareSupplierBookingLineForQtyPacked(Factory);
						var loadListHeader = OrderManagerTestHelper.PrepareContainerLoadListForQtyPacked(Factory, bookingLine, 8, loadMode, originalStatus.Code);
						var orderLine = bookingLine.OrderLine;
						AssertEquals($"{loadMode} when the status is {originalStatus.Code}", CommonContainerLoadList.IsConvertedOrShipped(loadListHeader.CLH_Status) ? 8m : 0m, orderLine.JO_QtyPacked);

						loadListHeader.CLH_Status = newStatus.Code;
						AssertEquals($"{loadMode} when the status is changed from {originalStatus.Code} to {newStatus.Code}", CommonContainerLoadList.IsConvertedOrShipped(loadListHeader.CLH_Status) ? 8m : 0m, orderLine.JO_QtyPacked);
					}
				}
			}
		}

		bool CheckStatusForShouldUpdatePackedQuantity(ZString status) => status == ContainerLoadListHeaderStatus.Placed
			|| status == ContainerLoadListHeaderStatus.Approved
			|| status == ContainerLoadListHeaderStatus.Converted
			|| status == ContainerLoadListHeaderStatus.Shipped
			|| status == ContainerLoadListHeaderStatus.Planned;

		public void TestIsConfirmedForQtyPacked()
		{
			foreach (ICodeDescription originalStatus in new CommonContainerLoadListStatusList())
			{
				var bookingLine = OrderManagerTestHelper.PrepareSupplierBookingLineForQtyPacked(Factory);
				var loadListHeader = OrderManagerTestHelper.PrepareContainerLoadListForQtyPacked(Factory, bookingLine, 8, originalStatus.Code);
				var shouldUpdate = CheckStatusForShouldUpdatePackedQuantity(loadListHeader.CLH_Status);

				AssertEquals(shouldUpdate, loadListHeader.IsConfirmed());
			}
		}

		#endregion

		public void TestDetailedGoodsDescription()
		{
			var containerLoadList = Factory.NewWithValidTestData<CommonContainerLoadList>();
			AssertEquals("Precondition: No note on the booking by default", 0, containerLoadList.Notes.GetAllNotes().Count);

			containerLoadList.CLH_DetailedGoodsDescription = "How you doing!";
			AssertEquals("Setting Note Description created a new note: Notes.Count", 1, containerLoadList.Notes.GetAllNotes().Count);
			AssertEquals("Setting Note Description created a new note: Note.Description", "Detailed Goods Description", containerLoadList.Notes.GetAllNotes().Cast<StmNote>().First().ST_Description);
			AssertEquals("Setting Note Description created a new note: Note.NoteText", "How you doing!", containerLoadList.Notes.GetAllNotes().Cast<StmNote>().First().ST_NoteText);
			AssertEquals("Can retrieve note text from CLH_DetailedGoodsDescription after creating note", "How you doing!", containerLoadList.CLH_DetailedGoodsDescription);

			containerLoadList.CLH_DetailedGoodsDescription = "I'm great thanks!";
			AssertEquals("Resetting Note Description updated existing note: Notes.Count", 1, containerLoadList.Notes.GetAllNotes().Count);
			AssertEquals("Resetting Note Description updated existing note: Note.Description", "Detailed Goods Description", containerLoadList.Notes.GetAllNotes().Cast<StmNote>().First().ST_Description);
			AssertEquals("Resetting Note Description updated existing note: Note.NoteText", "I'm great thanks!", containerLoadList.Notes.GetAllNotes().Cast<StmNote>().First().ST_NoteText);
			AssertEquals("Can retrieve note text from CLH_DetailedGoodsDescription after reseting it", "I'm great thanks!", containerLoadList.CLH_DetailedGoodsDescription);

			containerLoadList.Notes.RemoveAndDeleteAll();
			AssertEquals("accessing note text works after deleting note", ZString.Empty, containerLoadList.CLH_DetailedGoodsDescription);
			AssertEquals("no new note was created after accessing note text", 0, containerLoadList.Notes.GetAllNotes().Count);
		}

		public void TestControllingCustomerAddress()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCSZX";
			var containerLoadList = Factory.NewWithValidTestData<CommonContainerLoadList>();
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
			docAddress.E2_ParentID = containerLoadList.PK;
			docAddress.E2_ParentTableCode = "CLH";

			containerLoadList.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			Factory.Save();

			AssertNotNull(containerLoadList.ControllingCustomerAddress);
			AssertEquals("CCSZX", containerLoadList.ControllingCustomerAddress.Organisation.OH_Code);
			AssertEquals(controllingCustomer.PK, containerLoadList.ControllingCustomerAddress.OrganisationPK);
			AssertEquals(docAddress.E2_OA_Address, containerLoadList.ControllingCustomerAddress.E2_OA_Address);
		}

		public void TestContainerLoadListStatuses()
		{
			AssertEquals(
				@"If you have added a new status, please check that it should be included in the calculation of *_ToBePacked columns.
				  If it should be included, simply increment the number on this test.
				  If it should NOT be included, please add it to NotApplicableStatusesForToBePackedCalculation and increment the number on this test.",
				8, typeof(ContainerLoadListHeaderStatus).GetFields().Length
			);
		}

		public void TestShouldUpdateToBePackedColumns_ShouldBeTrueIfApplicableStatus()
		{
			var containerLoadList = Factory.New<CommonContainerLoadList>();

			foreach (var containerLoadListStatus in typeof(ContainerLoadListHeaderStatus).GetFields().Select(x => x.GetValue(null)).Cast<string>())
			{
				containerLoadList.CLH_Status = containerLoadListStatus;

				AssertEquals(!nonApplicableStatusesForToBePackedCalculation.Contains(containerLoadListStatus), containerLoadList.ShouldUpdateToBePackedColumns());
			}
		}

		public void TestShouldUpdateToBePackedColumns_ShouldEvaluatePassedInStatusIfGiven()
		{
			var containerLoadList = Factory.New<CommonContainerLoadList>();
			containerLoadList.CLH_Status = ContainerLoadListHeaderStatus.Incomplete;

			AssertEquals("Pre-Condition: status is not applicable", false, containerLoadList.ShouldUpdateToBePackedColumns());

			foreach (var containerLoadListStatus in typeof(ContainerLoadListHeaderStatus).GetFields().Select(x => x.GetValue(null)).Cast<string>())
			{
				AssertEquals(
					"Should evaluate passed in status, not status of the load list itself",
					!nonApplicableStatusesForToBePackedCalculation.Contains(containerLoadListStatus),
					containerLoadList.ShouldUpdateToBePackedColumns(containerLoadListStatus)
				);
			}
		}

		public void TestOnUpdateStatus_ShouldUpdateAllRelatedLines_WithValidStatusAndRelevantQuantities()
		{
			foreach (var loadMode in typeof(ContainerLoadListHeaderLoadMode).GetFields().Select(x => x.GetValue(null)).Cast<string>())
			{
				foreach (var originalStatus in typeof(ContainerLoadListHeaderStatus).GetFields().Select(x => x.GetValue(null)).Cast<string>())
				{
					foreach (var newStatus in typeof(ContainerLoadListHeaderStatus).GetFields().Select(x => x.GetValue(null)).Cast<string>().Where(s => s != originalStatus))
					{
						var order = Factory.New<Order>();
						var orderLine = order.OrderLines.AddNew();
						var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
						var supplierBookingLine1 = supplierBooking.SupplierBookingLines.AddNew();
						supplierBookingLine1.JSL_JO_OrderLine = orderLine.PK;
						OrderManagerTestHelper.SetReceivedValues(supplierBookingLine1, 50m, 50, 50m, 50m);

						var supplierBookingLine2 = supplierBooking.SupplierBookingLines.AddNew();
						supplierBookingLine2.JSL_JO_OrderLine = orderLine.PK;
						OrderManagerTestHelper.SetReceivedValues(supplierBookingLine2, 40m, 40, 40m, 40m);

						var containerLoadList = Factory.NewWithValidTestData<CommonContainerLoadList>();
						containerLoadList.CLH_LoadMode = loadMode;
						containerLoadList.CLH_Status = originalStatus;
						var originalUsePlanned = CommonContainerLoadList.CheckUsePlanned(containerLoadList.CLH_LoadMode, containerLoadList.CLH_Status);
						var originaShouldInclude = containerLoadList.ShouldUpdateToBePackedColumns();

						var containerLoadListLine1 = containerLoadList.LoadListLines.AddNew();
						containerLoadListLine1.CLL_JSL_BookingLine = supplierBookingLine1.PK;

						var containerLoadListLine2 = containerLoadList.LoadListLines.AddNew();
						containerLoadListLine2.CLL_JSL_BookingLine = supplierBookingLine2.PK;

						if (loadMode == ContainerLoadListHeaderLoadMode.ContainerYard)
						{
							containerLoadListLine1.CLL_JC_Container = Factory.NewWithValidTestData<Forwarding.Business.ForwardingContainer>().PK;
							containerLoadListLine2.CLL_JC_Container = Factory.NewWithValidTestData<Forwarding.Business.ForwardingContainer>().PK;
						}
						else
						{
							var containerLoadListLine3 = containerLoadList.LoadListLines.AddNew();
							containerLoadListLine3.CLL_JSL_BookingLine = supplierBookingLine1.PK;

							var containerLoadListLine4 = containerLoadList.LoadListLines.AddNew();
							containerLoadListLine4.CLL_JSL_BookingLine = supplierBookingLine2.PK;

							containerLoadListLine3.CLL_JC_Container = Factory.NewWithValidTestData<Forwarding.Business.ForwardingContainer>().PK;
							containerLoadListLine4.CLL_JC_Container = Factory.NewWithValidTestData<Forwarding.Business.ForwardingContainer>().PK;

							OrderManagerTestHelper.SetPackedValues(containerLoadListLine3, 10m, 10, 10m, 10m);
							OrderManagerTestHelper.SetPackedValues(containerLoadListLine4, 10m, 10, 10m, 10m);

							OrderManagerTestHelper.SetPlannedValues(containerLoadListLine3, 20m, 20, 20m, 20m);
							OrderManagerTestHelper.SetPlannedValues(containerLoadListLine4, 20m, 20, 20m, 20m);
						}

						OrderManagerTestHelper.SetPackedValues(containerLoadListLine1, 10m, 10, 10m, 10m);
						OrderManagerTestHelper.SetPackedValues(containerLoadListLine2, 10m, 10, 10m, 10m);

						OrderManagerTestHelper.SetPlannedValues(containerLoadListLine1, 20m, 20, 20m, 20m);
						OrderManagerTestHelper.SetPlannedValues(containerLoadListLine2, 20m, 20, 20m, 20m);

						containerLoadList.CLH_Status = newStatus;

						var shouldInclude = containerLoadList.ShouldUpdateToBePackedColumns();

						CombineAssertions($"Load Mode: {loadMode}, Status changed from {originalStatus} to {newStatus}", () =>
						{
							if (shouldInclude)
							{
								if (CommonContainerLoadList.CheckUsePlanned(containerLoadList.CLH_LoadMode, containerLoadList.CLH_Status))
								{
									AssertToBePacked(supplierBookingLine1, 30m, 30, 30m, 30m);
									AssertToBePacked(supplierBookingLine2, 20m, 20, 20m, 20m);
								}
								else
								{
									AssertToBePacked(supplierBookingLine1, 40m, 40, 40m, 40m);
									AssertToBePacked(supplierBookingLine2, 30m, 30, 30m, 30m);
								}
							}
							else
							{
								AssertToBePacked(supplierBookingLine1, 50m, 50, 50m, 50m);
								AssertToBePacked(supplierBookingLine2, 40m, 40, 40m, 40m);
							}
						});
					}
				}
			}
		}

		void AssertToBePacked(JobSupplierBookingLine supplierBookingLine, decimal quantity, int packages, decimal weight, decimal volume)
		{
			AssertEquals("Quantity", quantity, supplierBookingLine.JSL_RemainingQuantityToBePacked);
			AssertEquals("Packages", packages, supplierBookingLine.JSL_RemainingPackagesToBePacked);
			AssertEquals("Weight", weight, supplierBookingLine.JSL_RemainingWeightToBePacked);
			AssertEquals("Volume", volume, supplierBookingLine.JSL_RemainingVolumeToBePacked);
		}

		readonly string[] nonApplicableStatusesForToBePackedCalculation = new[]
		{
			ContainerLoadListHeaderStatus.Incomplete,
			ContainerLoadListHeaderStatus.Rejected,
			ContainerLoadListHeaderStatus.Cancelled,
		};
	}
}
