using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class TaskManagementLoadPackageFactTest : TestCaseWithFactory
	{
		public void TestConstructor_NullArguments_Throws()
		{
			var grouping = Mock.Of<ITaskManagementGroupingFact>();
			var dockDoorLocation = Mock.Of<ITaskManagementLocationFact>();
			var client = Mock.Of<IOrganisationFact>();
			var transportCompany = Mock.Of<IOrganisationFact>();
			var consigneeAddress = Mock.Of<IDocAddressFact>();

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementLoadPackageFact(null, dockDoorLocation, client,	transportCompany, consigneeAddress,	ZGuid.BrettsGuid, "EXP", false,	false, "BOX", "CAS"));
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementLoadPackageFact(grouping, null, client, transportCompany, consigneeAddress, ZGuid.BrettsGuid, "EXP", false, false, "BOX", "CAS"));
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementLoadPackageFact(grouping, dockDoorLocation, null, transportCompany, consigneeAddress, ZGuid.BrettsGuid, "EXP", false, false, "BOX", "CAS"));
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementLoadPackageFact(grouping, dockDoorLocation, client, null, consigneeAddress, ZGuid.BrettsGuid, "EXP", false, false, "BOX", "CAS"));
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementLoadPackageFact(grouping, dockDoorLocation, client, transportCompany, null, ZGuid.BrettsGuid, "EXP", false, false, "BOX", "CAS"));
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementLoadPackageFact(grouping, dockDoorLocation, client, transportCompany, consigneeAddress, ZGuid.BrettsGuid, null, false, false, "BOX", "CAS"));
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementLoadPackageFact(grouping, dockDoorLocation, client, transportCompany, consigneeAddress, ZGuid.BrettsGuid, "EXP", false, false, null, "CAS"));
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementLoadPackageFact(grouping, dockDoorLocation, client, transportCompany, consigneeAddress, ZGuid.BrettsGuid, "EXP", false, false, "BOX", null));
		}

		public void TestConstructor() => TestConstructor(hasDangerousGoods: false, isHandlingUnit: false);
		public void TestConstructor_HasDangerousGoods() => TestConstructor(hasDangerousGoods: true, isHandlingUnit: false);
		public void TestConstructor_IsHandlingUnit() => TestConstructor(hasDangerousGoods: false, isHandlingUnit: true);

		void TestConstructor(bool hasDangerousGoods, bool isHandlingUnit)
		{
			var grouping = Mock.Of<ITaskManagementGroupingFact>();
			var dockDoorLocation = Mock.Of<ITaskManagementLocationFact>();
			var client = Mock.Of<IOrganisationFact>();
			var transportCompany = Mock.Of<IOrganisationFact>();
			var consigneeAddress = Mock.Of<IDocAddressFact>();

			var packageFact = new TaskManagementLoadPackageFact(
				grouping,
				dockDoorLocation,
				client,
				transportCompany,
				consigneeAddress,
				ZGuid.BrettsGuid,
				"EXP",
				hasDangerousGoods,
				isHandlingUnit,
				"PLT",
				"CAS");
			AssertEquals(nameof(packageFact.PK), ZGuid.BrettsGuid, packageFact.PK);
			AssertEquals(nameof(packageFact.Grouping), grouping, packageFact.Grouping.Fact);
			AssertEquals(nameof(packageFact.DockDoorLocation), dockDoorLocation, packageFact.DockDoorLocation.Fact);
			AssertEquals(nameof(packageFact.TransportCompany), transportCompany, packageFact.TransportCompany.Fact);
			AssertEquals(nameof(packageFact.ConsigneeAddress), consigneeAddress, packageFact.ConsigneeAddress.Fact);
			AssertEquals(nameof(packageFact.CarrierServiceLevel), "EXP", packageFact.CarrierServiceLevel);
			AssertEquals(nameof(packageFact.HasDangerousGoods), hasDangerousGoods, packageFact.HasDangerousGoods);
			AssertEquals(nameof(packageFact.IsOnAHandlingUnit), isHandlingUnit, packageFact.IsOnAHandlingUnit);
			AssertEquals(nameof(packageFact.OuterPackageType), "PLT", packageFact.OuterPackageType);
			AssertEquals(nameof(packageFact.OuterUOMType), "CAS", packageFact.OuterUOMType);
		}
	}
}
