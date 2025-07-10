using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders
{
	class PlannedShipmentValidationExtensionsTest : TestCaseWithFactory
	{
		[TestDate(2023, 07, 03)]
		public void TestRaisePlannedShipmentValidationException_ShouldAddLogWithCorrectValues()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.Logs.GetAllLogs().RemoveAndDeleteAll();
			PlannedShipmentValidationExtensions.RaisePlannedShipmentValidationException(order.Logs);
			var log = order.Logs.GetAllLogs().OfType<StmALog>().Single();
			CombineAssertions(() =>
			{
				AssertEquals(Events.ExceptionRaisedCode, log.SL_SE_NKEvent);
				AssertEquals(expected: false, log.SL_IsEstimate);
				AssertEquals(new ZDateTime(2023, 07, 03), log.SL_EventTimeUtc);
				AssertEquals($"|RES=The shipments were detached from the container's consol before the pack lines could be created", log.SL_Reference);
			});
		}

		public void TestValidateShipmentConsolLink_ShouldSucceed_WithoutUsingSPT()
		{
			BuildLoadListLine("CLH001_CLL001", null, null);
			Factory.Save();

			AssertPrerequisite("CLH001_CLL001", AllocatedConsolKey, []);
			AssertValidation(shouldSucceed: true);
		}

		public void TestValidateShipmentConsolLink_ShouldSucceed_WithAllLinesCorrectlyLinked()
		{
			BuildLoadListLine("CLH001_CLL001", "JS_CORRECT_1", AllocatedConsolKey);
			BuildLoadListLine("CLH001_CLL002", "JS_CORRECT_2", AllocatedConsolKey);
			Factory.Save();

			AssertPrerequisite("CLH001_CLL001", AllocatedConsolKey, [$"JS_CORRECT_1 {AllocatedConsolKey}"]);
			AssertPrerequisite("CLH001_CLL002", AllocatedConsolKey, [$"JS_CORRECT_2 {AllocatedConsolKey}"]);
			AssertValidation(shouldSucceed: true);
		}

		public void TestValidateShipmentConsolLink_ShouldReturnErrorMessage_WithAnyMislinkedShipemnt()
		{
			BuildLoadListLine("CLH001_CLL001", "JS_CORRECT", AllocatedConsolKey);
			BuildLoadListLine("CLH001_CLL002", "JS_MISLINKED", "JK_MISLINKED");
			Factory.Save();

			AssertPrerequisite("CLH001_CLL001", AllocatedConsolKey, [$"JS_CORRECT {AllocatedConsolKey}"]);
			AssertPrerequisite("CLH001_CLL002", AllocatedConsolKey, ["JS_MISLINKED JK_MISLINKED"]);
			AssertValidation(shouldSucceed: false);
		}

		public void TestValidateShipmentConsolLink_ShouldReturnErrorMessage_WithAnyUnlinkedShipemnt()
		{
			BuildLoadListLine("CLH001_CLL001", "JS_CORRECT", AllocatedConsolKey);
			BuildLoadListLine("CLH001_CLL002", "JS_UNLINKED", null);
			Factory.Save();

			AssertPrerequisite("CLH001_CLL001", AllocatedConsolKey, [$"JS_CORRECT {AllocatedConsolKey}"]);
			AssertPrerequisite("CLH001_CLL002", AllocatedConsolKey, ["JS_UNLINKED "]);
			AssertValidation(shouldSucceed: false);
		}

		public void TestValidateShipmentConsolLink_ShouldReturnErrorMessage_WhenShipmentIsLinkedToMultipleConsolAndNoneIsTheAllocatedConsol()
		{
			BuildLoadListLine("CLH001_CLL001", "JS_MULTIMISLINKED", "JK_MISLINKED_1");
			var shipmentWithMultipleMislinkedConsol = builder.MatchExistedEntity<ForwardingShipment>("JS_MULTIMISLINKED");
			shipmentWithMultipleMislinkedConsol.Consols.Add(builder.BuildConsol("JK_MISLINKED_2"));
			Factory.Save();

			AssertPrerequisite(
				"CLH001_CLL001",
				AllocatedConsolKey,
				["JS_MULTIMISLINKED JK_MISLINKED_1", "JS_MULTIMISLINKED JK_MISLINKED_2"]);
			AssertValidation(shouldSucceed: false);
		}

		public void TestValidateShipmentConsolLink_ShouldSucceed_WhenShipmentIsLinkedToMultipleConsolAndOneIsTheAllocatedConsol()
		{
			BuildLoadListLine("CLH001_CLL001", "JS_ONE_CORRECT", "JK_MISLINKED_1");
			var shipmentWithOneCorrectConsol = builder.MatchExistedEntity<ForwardingShipment>("JS_ONE_CORRECT");
			shipmentWithOneCorrectConsol.Consols.Add(builder.BuildConsol(AllocatedConsolKey));
			Factory.Save();

			AssertPrerequisite(
				"CLH001_CLL001",
				AllocatedConsolKey,
				[$"JS_ONE_CORRECT {AllocatedConsolKey}", "JS_ONE_CORRECT JK_MISLINKED_1"]);
			AssertValidation(shouldSucceed: true);
		}

		public void TestValidateShipmentConsolLink_ShouldSucceed_WhenBookingLineIsSplitToMultipleShipmentsWithOneCorrectLink()
		{
			BuildLoadListLine("CLH001_CLL001", "JS_CORRECT", AllocatedConsolKey);
			BuildLoadListLine("CLH001_CLL001", "JS_MISLINKED", "JK_MISLINKED");
			Factory.Save();

			AssertPrerequisite(
				"CLH001_CLL001",
				AllocatedConsolKey,
				[$"JS_CORRECT {AllocatedConsolKey}", "JS_MISLINKED JK_MISLINKED"]);
			AssertValidation(shouldSucceed: true);
		}

		public void TestValidateShipmentConsolLink_ShouldSucceed_ForCFS()
		{
			BuildLoadListLine("CLH001_CLL001", "JS_CORRECT", AllocatedConsolKey);
			var containerLoadListLine = builder.MatchExistedEntity<ContainerLoadListLine>("CLH001_CLL001");
			containerLoadListLine.CLL_LoadMode = CommonContainerLoadListLoadModeList.Codes.CFS;
			Factory.Save();

			AssertPrerequisite("CLH001_CLL001", AllocatedConsolKey, [$"JS_CORRECT {AllocatedConsolKey}"]);
			AssertValidation(shouldSucceed: true);
		}

		#region Implementation

		const string AllocatedConsolKey = "JK_ALLOCATED";
		const string AllocatedContainerKey = "JC_ALLOCATED";
		const string SupplierBookingKey = "JSB001";
		const string ContainerLoadListKey = "CLH001";
		OrderManagerMockDataBuilder builder;

		void BuildLoadListLine(string loadListLineKey, string plannedShipmentKey, string plannedConsolKey)
		{
			builder ??= new OrderManagerMockDataBuilder(Factory);
			var helper = new PlannedShipmentValidationTestHelper(builder);
			helper.BuildCYContainerLoadListWithPlannedShipment(
				SupplierBookingKey,
				ContainerLoadListKey,
				loadListLineKey,
				plannedShipmentKey,
				plannedConsolKey,
				AllocatedConsolKey,
				AllocatedContainerKey);
		}

		void AssertPrerequisite(string loadListLineKey, string packedConsolKey, string[] spaceSeparatedPlannedShipmentAndConsol)
		{
			builder ??= new OrderManagerMockDataBuilder(Factory);

			var loadListLine = builder.MatchExistedEntity<ContainerLoadListLine>(loadListLineKey);
			var packedResult = loadListLine.Container?.Consol?.JK_UniqueConsignRef;

			var plannedLineQuery = Factory.Load<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, loadListLine.CLL_JSL_BookingLine));
			var plannedResult = plannedLineQuery
				.OrderBy(jl => jl.Shipment.JS_UniqueConsignRef)
				.Select(jl => jl.Shipment)
				.SelectMany(
					js => js.Consols.OfType<ForwardingConsol>().OrderBy(x => x.JK_UniqueConsignRef).DefaultIfEmpty(),
					(shipment, consol) => $"{shipment.JS_UniqueConsignRef} {consol?.JK_UniqueConsignRef}");

			CombineAssertions(() => {
				AssertEquals(packedConsolKey, packedResult);
				AssertContainsExactElementsInExactOrder(spaceSeparatedPlannedShipmentAndConsol, plannedResult);
			});
		}

		void AssertValidation(bool shouldSucceed)
		{
			CombineAssertions(() =>
			{
				AssertEquals(shouldSucceed, builder.MatchExistedEntity<CYContainerLoadList>(ContainerLoadListKey).ValidateShipmentConsolLink());
				AssertEquals(shouldSucceed, builder.MatchExistedEntity<JobSupplierBooking>(SupplierBookingKey).ValidateShipmentConsolLink());
			});
		}

		#endregion
	}
}
