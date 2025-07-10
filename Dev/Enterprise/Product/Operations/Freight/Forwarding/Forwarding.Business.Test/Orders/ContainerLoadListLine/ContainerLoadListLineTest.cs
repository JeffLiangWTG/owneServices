using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadListLine))]
	sealed class ContainerLoadListLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOnUpdatePackedQuantity_ShouldUpdateRemainingQuantityToBePacked_GivenHeaderIsPacked()
		{
			AssertOnUpdate_PlannedOrPackedColumn_ChangesToBePackedColumnIfRelevant(
				nameof(ContainerLoadListLine.CLL_PackedQuantity),
				nameof(JobSupplierBookingLine.JSL_RemainingQuantityToBePacked),
				isPlannedColumn: false
			);
		}

		public void TestOnUpdatePackedPackages_ShouldUpdateRemainingPackagesToBePacked_GivenHeaderIsPacked()
		{
			AssertOnUpdate_PlannedOrPackedColumn_ChangesToBePackedColumnIfRelevant(
				nameof(ContainerLoadListLine.CLL_Packages),
				nameof(JobSupplierBookingLine.JSL_RemainingPackagesToBePacked),
				isPlannedColumn: false
			);
		}

		public void TestOnUpdatePackedWeight_ShouldUpdateRemainingWeightToBePacked_GivenHeaderIsPacked()
		{
			AssertOnUpdate_PlannedOrPackedColumn_ChangesToBePackedColumnIfRelevant(
				nameof(ContainerLoadListLine.CLL_Weight),
				nameof(JobSupplierBookingLine.JSL_RemainingWeightToBePacked),
				isPlannedColumn: false
			);
		}

		public void TestOnUpdatePackedVolume_ShouldUpdateRemainingVolumeToBePacked_GivenHeaderIsPacked()
		{
			AssertOnUpdate_PlannedOrPackedColumn_ChangesToBePackedColumnIfRelevant(
				nameof(ContainerLoadListLine.CLL_Volume),
				nameof(JobSupplierBookingLine.JSL_RemainingVolumeToBePacked),
				isPlannedColumn: false
			);
		}

		public void TestOnUpdatePlannedQuantity_ShouldUpdateRemainingQuantityToBePacked_GivenHeaderIsPlanned()
		{
			AssertOnUpdate_PlannedOrPackedColumn_ChangesToBePackedColumnIfRelevant(
				nameof(ContainerLoadListLine.CLL_PlannedQuantity),
				nameof(JobSupplierBookingLine.JSL_RemainingQuantityToBePacked),
				isPlannedColumn: true
			);
		}

		public void TestOnUpdatePlannedPackages_ShouldUpdateRemainingPackagesToBePacked_GivenHeaderIsPlanned()
		{
			AssertOnUpdate_PlannedOrPackedColumn_ChangesToBePackedColumnIfRelevant(
				nameof(ContainerLoadListLine.CLL_PlannedPackages),
				nameof(JobSupplierBookingLine.JSL_RemainingPackagesToBePacked),
				isPlannedColumn: true
			);
		}

		public void TestOnUpdatePlannedWeight_ShouldUpdateRemainingWeightToBePlanned_GivenHeaderIsPlanned()
		{
			AssertOnUpdate_PlannedOrPackedColumn_ChangesToBePackedColumnIfRelevant(
				nameof(ContainerLoadListLine.CLL_PlannedWeight),
				nameof(JobSupplierBookingLine.JSL_RemainingWeightToBePacked),
				isPlannedColumn: true
			);
		}

		public void TestOnUpdatePlannedVolume_ShouldUpdateRemainingVolumeToBePacked_GivenHeaderIsPlanned()
		{
			AssertOnUpdate_PlannedOrPackedColumn_ChangesToBePackedColumnIfRelevant(
				nameof(ContainerLoadListLine.CLL_PlannedVolume),
				nameof(JobSupplierBookingLine.JSL_RemainingVolumeToBePacked),
				isPlannedColumn: true
			);
		}

		public void TestOnUpdate_ContainerLoadListLineLinkage_ShouldUpdateCalculationIfIncluded()
		{
			foreach (var loadMode in new[] { Constants.ContainerLoadListHeaderLoadMode.ContainerYard, Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation })
			{
				foreach (var containerLoadListStatus in typeof(CommonContainerLoadListStatusList.Codes).GetFields().Select(x => x.GetValue(null)).Cast<string>())
				{
					var containerLoadListLine = GetContainerLoadListLine(loadMode, containerLoadListStatus);
					if (loadMode == Constants.ContainerLoadListHeaderLoadMode.ContainerYard || containerLoadListStatus == CommonContainerLoadListStatusList.Codes.SHP || containerLoadListStatus == CommonContainerLoadListStatusList.Codes.CNV)
					{
						containerLoadListLine.CLL_JC_Container = Factory.NewWithValidTestData<ForwardingContainer>().PK;
					}

					OrderManagerTestHelper.SetPackedValues(containerLoadListLine, 20m, 20, 20m, 20m);
					OrderManagerTestHelper.SetPlannedValues(containerLoadListLine, 10m, 10, 10m, 10m);

					var originalSupplierBookingLine = containerLoadListLine.SupplierBookingLine;
					var newSupplierBookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
					OrderManagerTestHelper.SetReceivedValues(newSupplierBookingLine, 30m, 30, 30m, 30m);

					containerLoadListLine.CLL_JSL_BookingLine = newSupplierBookingLine.PK;

					var shouldDoNothing = !containerLoadListLine.LoadListHeader.ShouldUpdateToBePackedColumns();

					CombineAssertions($"Should update calculation for both old and new supplier booking line to be packed values (Status: {containerLoadListStatus}, Load Mode: {loadMode})", () =>
					{
						AssertToBePacked(originalSupplierBookingLine, 50m, 50, 50m, 50m);

						if (shouldDoNothing)
						{
							AssertToBePacked(newSupplierBookingLine, 30m, 30, 30m, 30m);
						}
						else
						{
							if (CommonContainerLoadList.CheckUsePlanned(containerLoadListLine.LoadListHeader.CLH_LoadMode, containerLoadListLine.LoadListHeader.CLH_Status))
							{
								AssertToBePacked(newSupplierBookingLine, 20m, 20, 20m, 20m);
							}
							else
							{
								AssertToBePacked(newSupplierBookingLine, 10m, 10, 10m, 10m);
							}
						}
					});
				}
			}
		}

		public void TestOnDelete_ContainerLoadListLine_ShouldUpdateCalculationIfIncluded()
		{
			foreach (var loadMode in new[] { Constants.ContainerLoadListHeaderLoadMode.ContainerYard, Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation })
			{
				foreach (var containerLoadListStatus in typeof(CommonContainerLoadListStatusList.Codes).GetFields().Select(x => x.GetValue(null)).Cast<string>())
				{
					var containerLoadListLine = GetContainerLoadListLine(loadMode, containerLoadListStatus);
					OrderManagerTestHelper.SetPackedValues(containerLoadListLine, 20m, 20, 20m, 20m);
					OrderManagerTestHelper.SetPlannedValues(containerLoadListLine, 10m, 10, 10m, 10m);

					var supplierBookingLine = containerLoadListLine.SupplierBookingLine;

					containerLoadListLine.Delete();

					CombineAssertions($"Status: {containerLoadListStatus}, Load Mode: {loadMode}", () =>
					{
						AssertToBePacked(supplierBookingLine, 50m, 50, 50m, 50m);
					});
				}
			}
		}

		void AssertOnUpdate_PlannedOrPackedColumn_ChangesToBePackedColumnIfRelevant(string columnToTest, string toBePackedColumnName, bool isPlannedColumn)
		{
			foreach (var loadMode in new[] { Constants.ContainerLoadListHeaderLoadMode.ContainerYard, Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation })
			{
				foreach (var containerLoadListStatus in typeof(CommonContainerLoadListStatusList.Codes).GetFields().Select(x => x.GetValue(null)).Cast<string>())
				{
					var containerLoadListLine = GetContainerLoadListLine(loadMode, containerLoadListStatus);
					if (loadMode == Constants.ContainerLoadListHeaderLoadMode.ContainerYard || containerLoadListStatus == CommonContainerLoadListStatusList.Codes.SHP || containerLoadListStatus == CommonContainerLoadListStatusList.Codes.CNV)
					{
						containerLoadListLine.CLL_JC_Container = Factory.NewWithValidTestData<ForwardingContainer>().PK;
					}

					CombineAssertions("Preconditions", () =>
					{
						AssertEquals(0m, new ZDecimal(containerLoadListLine[columnToTest]));
						AssertEquals(50m, new ZDecimal(containerLoadListLine.SupplierBookingLine[toBePackedColumnName]));
					});

					var shouldDoNothing = !containerLoadListLine.LoadListHeader.ShouldUpdateToBePackedColumns(containerLoadListStatus) || (isPlannedColumn
						? (!CommonContainerLoadList.CheckUsePlanned(containerLoadListLine.LoadListHeader.CLH_LoadMode, containerLoadListLine.LoadListHeader.CLH_Status))
						: (CommonContainerLoadList.CheckUsePlanned(containerLoadListLine.LoadListHeader.CLH_LoadMode, containerLoadListLine.LoadListHeader.CLH_Status)));

					CombineAssertions($"Column: {columnToTest}, Status: {containerLoadListStatus}, Load Mode: {loadMode}", () =>
					{
						containerLoadListLine[columnToTest] = 50;
						AssertEquals(shouldDoNothing ? 50m : 0m, new ZDecimal(containerLoadListLine.SupplierBookingLine[toBePackedColumnName]));

						containerLoadListLine[columnToTest] = 40;
						AssertEquals(shouldDoNothing ? 50m : 10m, new ZDecimal(containerLoadListLine.SupplierBookingLine[toBePackedColumnName]));

						containerLoadListLine[columnToTest] = 0;
						AssertEquals(50m, new ZDecimal(containerLoadListLine.SupplierBookingLine[toBePackedColumnName]));
					});
				}
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order.OrderLines.AddNew();

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JO_OrderLine = orderLine1.PK;

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;

			var container = consol1.Containers.AddNew();
			container.FillWithValidTestData();
			container.JC_JSB_SupplierBooking = booking.PK;

			var loadListHeader = Factory.NewWithValidTestData<CYContainerLoadList>();
			var loadListLine = loadListHeader.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_JC_Container = container.PK;

			return loadListLine;
		}

		#region QtyPacked

		public void TestQtyPackedForDelete()
		{
			var bookingLine = OrderManagerTestHelper.PrepareSupplierBookingLineForQtyPacked(Factory);
			(var loadListLine1, var loadListLine2) = PrepareContainerLoadListLineForQtyPacked(bookingLine);

			Factory.Save();

			var orderLine = bookingLine.OrderLine;
			AssertEquals(0m, orderLine.JO_QtyPacked);

			loadListLine1.LoadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Converted;
			loadListLine1.CLL_PackedQuantity = 8;
			loadListLine2.LoadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Converted;
			loadListLine2.CLL_PackedQuantity = 5;
			AssertEquals(13m, orderLine.JO_QtyPacked);

			loadListLine1.Delete();
			AssertEquals(5m, orderLine.JO_QtyPacked);

			loadListLine2.Delete();
			AssertEquals(0m, orderLine.JO_QtyPacked);
		}

		public void TestQtyPackedForPackedQuantity()
		{
			var bookingLine = OrderManagerTestHelper.PrepareSupplierBookingLineForQtyPacked(Factory);
			(var loadListLine1, var loadListLine2) = PrepareContainerLoadListLineForQtyPacked(bookingLine);

			Factory.Save();

			var orderLine = bookingLine.OrderLine;
			AssertEquals(0m, orderLine.JO_QtyPacked);

			loadListLine1.LoadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Converted;

			AssertEquals(13m, orderLine.JO_QtyPacked);

			loadListLine1.CLL_PackedQuantity = 1;
			AssertEquals(6m, orderLine.JO_QtyPacked);

			loadListLine2.CLL_PackedQuantity = 12;
			AssertEquals(13m, orderLine.JO_QtyPacked);

			loadListLine1.LoadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Shipped;
			AssertEquals(13m, orderLine.JO_QtyPacked);

			loadListLine1.LoadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Incomplete;
			AssertEquals(0m, orderLine.JO_QtyPacked);
		}

		public void TestQtyPackedForSupplierBookingLine()
		{
			var bookingLine1 = OrderManagerTestHelper.PrepareSupplierBookingLineForQtyPacked(Factory);
			PrepareContainerLoadListLineForQtyPacked(bookingLine1);

			var bookingLine2 = OrderManagerTestHelper.PrepareSupplierBookingLineForQtyPacked(Factory);
			(var loadListLine21, _) = PrepareContainerLoadListLineForQtyPacked(bookingLine2);

			Factory.Save();

			var orderLine1 = bookingLine1.OrderLine;
			var orderLine2 = bookingLine2.OrderLine;
			AssertEquals(0m, orderLine1.JO_QtyPacked);

			loadListLine21.CLL_JSL_BookingLine = bookingLine2.PK;
			loadListLine21.LoadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Shipped;

			AssertEquals(13m, orderLine2.JO_QtyPacked);

			loadListLine21.CLL_JSL_BookingLine = bookingLine1.PK;
			AssertEquals(8m, orderLine1.JO_QtyPacked);
			AssertEquals(5m, orderLine2.JO_QtyPacked);
		}

		(ContainerLoadListLine loadListLine1, ContainerLoadListLine loadListLine2) PrepareContainerLoadListLineForQtyPacked(JobSupplierBookingLine bookingLine)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var container = consol.Containers.AddNew();
			container.FillWithValidTestData();
			container.JC_JSB_SupplierBooking = bookingLine.SupplierBooking.PK;

			var loadListHeader = Factory.NewWithValidTestData<CYContainerLoadList>();
			loadListHeader.CLH_JSB_Booking = bookingLine.SupplierBooking.PK;
			loadListHeader.CLH_Status = Constants.ContainerLoadListHeaderStatus.Placed;

			var loadListLine1 = loadListHeader.LoadListLines.AddNew();
			loadListLine1.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine1.CLL_JC_Container = container.PK;
			loadListLine1.CLL_PackedQuantity = 8;

			var loadListLine2 = loadListHeader.LoadListLines.AddNew();
			loadListLine2.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine2.CLL_JC_Container = container.PK;
			loadListLine2.CLL_PackedQuantity = 5;

			return (loadListLine1, loadListLine2);
		}

		#endregion

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetContainerLoadListLine(Constants.ContainerLoadListHeaderLoadMode.ContainerYard, Constants.ContainerLoadListHeaderStatus.Incomplete);

		ContainerLoadListLine GetContainerLoadListLine(string loadMode, string containerLoadListStatus)
		{
			var orderLine = Factory.NewWithValidTestData<Order>().OrderLines.AddNew();
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var supplierBookingLine1 = supplierBooking.SupplierBookingLines.AddNew();
			supplierBookingLine1.JSL_JO_OrderLine = orderLine.PK;
			OrderManagerTestHelper.SetReceivedValues(supplierBookingLine1, 50m, 50, 50m, 50m);

			var containerLoadList = Factory.NewWithValidTestData<CommonContainerLoadList>();
			containerLoadList.CLH_Status = containerLoadListStatus;
			containerLoadList.CLH_LoadMode = loadMode;

			var containerLoadListLine1 = containerLoadList.LoadListLines.AddNew();
			containerLoadListLine1.CLL_JSL_BookingLine = supplierBookingLine1.PK;

			return containerLoadListLine1;
		}

		void AssertToBePacked(JobSupplierBookingLine supplierBookingLine, decimal quantity, int packages, decimal weight, decimal volume)
		{
			AssertEquals("Quantity", quantity, supplierBookingLine.JSL_RemainingQuantityToBePacked);
			AssertEquals("Packages", packages, supplierBookingLine.JSL_RemainingPackagesToBePacked);
			AssertEquals("Weight", weight, supplierBookingLine.JSL_RemainingWeightToBePacked);
			AssertEquals("Volume", volume, supplierBookingLine.JSL_RemainingVolumeToBePacked);
		}

		#region IExternalRequestGenerationProvider

		public void TestRequestGenerationProvider_CFS()
		{
			var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var reviewerOrg = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			Factory.Save();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			order.JD_OA_BuyerAddress = assigneeOrg.MainAddress.PK;
			order.JD_OC_BuyerContact = assigneeOrg.Contacts[0].PK;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			order.OrderLines.Add(orderLine);

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.SupplierAddress.E2_OA_Address = reviewerOrg.MainAddress.PK;
			booking.SupplierAddress.E2_Contact = reviewerOrg.Contacts[0].OC_ContactName;

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.SupplierBookingLines.Add(bookingLine);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine, (DocAddressType.Manufacturer), manufactureOrg.MainAddress.PK, manufactureOrg.Contacts[0].OC_ContactName);

			var loadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			loadPlan.CLH_LoadListId = "CLP001";
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(loadPlan, (DocAddressType.ControllingCustomer), controllingCustomerOrg.MainAddress.PK, controllingCustomerOrg.Contacts[0].OC_ContactName);
			var loadPlanLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadPlanLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadPlanLine.CLL_LoadMode = Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation;
			loadPlan.LoadListLines.Add(loadPlanLine);
			Factory.Save();

			AssertEquals("CLP001", loadPlanLine.GetRequestJobID());
			AssertEquals(ExternalRequestTypes.Codes.ContainerLoadPlanLine, loadPlanLine.GetRequestTypeCode());
			AssertEquals(assigneeOrg.PK, loadPlanLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(assigneeOrg.Contacts[0].PK, loadPlanLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);
			AssertEquals(reviewerOrg.PK, loadPlanLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).OrginzationPK);
			AssertEquals(reviewerOrg.Contacts[0].PK, loadPlanLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).ContactPK);
			AssertEquals(controllingCustomerOrg.PK, loadPlanLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).OrginzationPK);
			AssertEquals(controllingCustomerOrg.Contacts[0].PK, loadPlanLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).ContactPK);
			AssertEquals(manufactureOrg.PK, loadPlanLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(manufactureOrg.Contacts[0].PK, loadPlanLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
		}

		public void TestRequestGenerationProvider_CY()
		{
			var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var reviewerOrg = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var loadListPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			order.JD_OA_BuyerAddress = assigneeOrg.MainAddress.PK;
			order.JD_OC_BuyerContact = assigneeOrg.Contacts[0].PK;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			order.OrderLines.Add(orderLine);

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.ControllingCustomer), controllingCustomerOrg.MainAddress.PK, controllingCustomerOrg.Contacts[0].OC_ContactName);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.SupplierDocumentaryAddress), reviewerOrg.MainAddress.PK, reviewerOrg.Contacts[0].OC_ContactName);

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.SupplierBookingLines.Add(bookingLine);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine, (DocAddressType.Manufacturer), manufactureOrg.MainAddress.PK, manufactureOrg.Contacts[0].OC_ContactName);

			var loadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			loadList.CLH_LoadListId = "CLH001";
			loadList.CLH_OH_LoadListParty = loadListPartyOrg.PK;
			loadList.CLH_JSB_Booking = booking.PK;
			var loadListLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_LoadMode = Constants.ContainerLoadListHeaderLoadMode.ContainerYard;
			loadList.LoadListLines.Add(loadListLine);
			Factory.Save();

			AssertEquals("CLH001", loadListLine.GetRequestJobID());
			AssertEquals(ExternalRequestTypes.Codes.ContainerLoadListLine, loadListLine.GetRequestTypeCode());
			AssertEquals(assigneeOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(assigneeOrg.Contacts[0].PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);
			AssertEquals(reviewerOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).OrginzationPK);
			AssertEquals(reviewerOrg.Contacts[0].PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).ContactPK);
			AssertEquals(controllingCustomerOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).OrginzationPK);
			AssertEquals(controllingCustomerOrg.Contacts[0].PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).ContactPK);
			AssertEquals(manufactureOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(manufactureOrg.Contacts[0].PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
			AssertEquals(loadListPartyOrg.PK, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.LoadListParty).OrginzationPK);
			AssertEquals(ZGuid.Empty, loadList.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.LoadListParty).ContactPK);
		}

		#endregion
	}
}
