using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(VASOrderFilterBusinessObject))]
	public class VASOrderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestGetModuleFilterThatOverridesAllOtherFilters

		public void TestGetModuleFilterThatOverridesAllOtherFilters()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder1 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();
			Asserter.AddToScope(vasOrder1, vasOrder2);

			var filter = (ModuleFountainFilter)FilterStrip.ModuleFilters[VASOrderFilterBusinessObject.FilterConstants.VASOrderJobID];
			filter.Property = "WV00000001";
			Asserter.AssertMatches("", filter, vasOrder1);

			filter.Property = "WV00000002";
			Asserter.AssertMatches("", filter, vasOrder2);
		}

		#endregion

		#region TestClient

		public void TestClient()
		{
			var data = new TestDataForInventory(Factory, new TestNotificationBuffer());
			data.CreateMultiWarehouseClientProductInventory();

			var vasOrder1 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org2);
			Factory.Save();

			Asserter.AddToScope(vasOrder1, vasOrder2);

			var filter = (ModuleGuidFilter)FilterStrip.ModuleFilters[VASOrderFilterBusinessObject.FilterConstants.Client];
			AssertEquals(FilterCategories.Organisations, filter.Category);
			Asserter.AssertMatches("", filter, vasOrder1, vasOrder2);

			filter.Property = data.Org1.PK;
			Asserter.AssertMatches("", filter, vasOrder1);

			filter.Property = data.Org2.PK;
			Asserter.AssertMatches("", filter, vasOrder2);

			filter.Property = ZGuid.NewZGuid();
			Asserter.AssertMatches("", filter);
		}

		#endregion

		#region TestCustomerReferenceNo

		public void TestCustomerReferenceNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder1.WVO_CustomerReferenceNo = "123";
			vasOrder2.WVO_CustomerReferenceNo = "456";
			Factory.Save();
			Asserter.AddToScope(vasOrder1, vasOrder2);

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[VASOrderFilterBusinessObject.FilterConstants.CustomerReferenceNo];
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.Property = "123";
			Asserter.AssertMatches("Should match correct VAS Order.", filter, vasOrder1);

			filter.Property = "456";
			Asserter.AssertMatches("Should match correct VAS Order.", filter, vasOrder2);

			filter.Property = "XXX";
			Asserter.AssertMatches("Should match no VAS Orders.", filter);

			filter.Property = "";
			Asserter.AssertMatches("Should match all VAS Orders.", filter, vasOrder1, vasOrder2);
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			var notify = new TestNotificationBuffer();
			var data = new TestDataForInventory(Factory);
			Factory.Save();

			data.CreateSimpleInventory(false);
			data.Receive11.FinaliseDocket();
			Factory.Save();

			var vasOrder_Entered = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder_TransferringIn = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder_Working = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder_WorkCompleted = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder_TransferringOut = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder_FinalisedWithReturnTransfer = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder_FinalisedWithoutReturnTransfer = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);

			var transferIntoServiceArea1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", notify);
			var transferIntoServiceArea2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", notify);
			var transferIntoServiceArea3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", notify);
			var transferIntoServiceArea4 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR4", notify);
			var transferIntoServiceArea5 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR5", notify);
			var transferIntoServiceArea6 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR6", notify);

			Helper.CreateWhsTransferLine(transferIntoServiceArea2, data.Part1, 10m, "A-1", "A-2");
			Helper.CreateWhsTransferLine(transferIntoServiceArea3, data.Part1, 10m, "A-1", "A-2");
			Helper.CreateWhsTransferLine(transferIntoServiceArea4, data.Part1, 10m, "A-1", "A-2");
			Helper.CreateWhsTransferLine(transferIntoServiceArea5, data.Part1, 10m, "A-1", "A-2");
			Helper.CreateWhsTransferLine(transferIntoServiceArea6, data.Part1, 10m, "A-1", "A-2");

			vasOrder_TransferringIn.WVO_WD_TransferIntoServiceArea = transferIntoServiceArea1.PK;
			transferIntoServiceArea1.RunPreSaveValidation(); // to commit inventory

			vasOrder_Working.WVO_WD_TransferIntoServiceArea = transferIntoServiceArea2.PK;
			transferIntoServiceArea2.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferIntoServiceArea2);

			vasOrder_WorkCompleted.WVO_WD_TransferIntoServiceArea = transferIntoServiceArea3.PK;
			transferIntoServiceArea3.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferIntoServiceArea3);

			vasOrder_TransferringOut.WVO_WD_TransferIntoServiceArea = transferIntoServiceArea4.PK;
			transferIntoServiceArea4.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferIntoServiceArea4);

			vasOrder_FinalisedWithReturnTransfer.WVO_WD_TransferIntoServiceArea = transferIntoServiceArea5.PK;
			transferIntoServiceArea5.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferIntoServiceArea5);

			vasOrder_FinalisedWithoutReturnTransfer.WVO_WD_TransferIntoServiceArea = transferIntoServiceArea6.PK;
			transferIntoServiceArea6.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferIntoServiceArea6);
			Factory.Save();

			vasOrder_WorkCompleted.MarkVASOrderAsCompleted(notify);
			vasOrder_TransferringOut.MarkVASOrderAsCompleted(notify);
			vasOrder_FinalisedWithReturnTransfer.MarkVASOrderAsCompleted(notify);
			vasOrder_FinalisedWithoutReturnTransfer.MarkVASOrderAsCompleted(notify);
			Factory.Save();

			vasOrder_FinalisedWithoutReturnTransfer.FinaliseVASOrder(new TestNotificationBuffer());

			var transferOutOfServiceArea1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR7", notify);
			Helper.CreateWhsTransferLine(transferOutOfServiceArea1, data.Part1, 10m, "A-2", "A-1");
			vasOrder_TransferringOut.WVO_WD_TransferOutOfServiceArea = transferOutOfServiceArea1.PK;
			transferOutOfServiceArea1.RunPreSaveValidation(); // to commit inventory

			var transferOutOfServiceArea2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR8", notify);
			Helper.CreateWhsTransferLine(transferOutOfServiceArea2, data.Part1, 10m, "A-2", "A-1");
			vasOrder_FinalisedWithReturnTransfer.WVO_WD_TransferOutOfServiceArea = transferOutOfServiceArea2.PK;
			transferOutOfServiceArea2.RunPreSaveValidation(); // to commit inventory
			transferOutOfServiceArea2.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferOutOfServiceArea2);
			Factory.Save();

			AssertEquals("Precondition", WhsVASOrderStatuses.Descriptions.Entered.ToString().ToUpper(), vasOrder_Entered.Status);
			AssertEquals("Precondition", WhsVASOrderStatuses.Descriptions.TransferringIn.ToString().ToUpper(), vasOrder_TransferringIn.Status);
			AssertEquals("Precondition", WhsVASOrderStatuses.Descriptions.Working.ToString().ToUpper(), vasOrder_Working.Status);
			AssertEquals("Precondition", WhsVASOrderStatuses.Descriptions.WorkCompleted.ToString().ToUpper(), vasOrder_WorkCompleted.Status);
			AssertEquals("Precondition", WhsVASOrderStatuses.Descriptions.TransferringOut.ToString().ToUpper(), vasOrder_TransferringOut.Status);
			AssertEquals("Precondition", WhsVASOrderStatuses.Descriptions.Finalized.ToString().ToUpper(), vasOrder_FinalisedWithReturnTransfer.Status);
			AssertEquals("Precondition", WhsVASOrderStatuses.Descriptions.Finalized.ToString().ToUpper(), vasOrder_FinalisedWithoutReturnTransfer.Status);

			Asserter.AddToScope(vasOrder_Entered, vasOrder_TransferringIn, vasOrder_Working, vasOrder_TransferringOut,
				vasOrder_FinalisedWithoutReturnTransfer, vasOrder_FinalisedWithReturnTransfer, vasOrder_WorkCompleted);

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[VASOrderFilterBusinessObject.FilterConstants.Status];
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertEquals("Should not contain Cancelled - there is a bespoke filter for active/inactive", false, ((CodeDescriptionPairList)filter.List).ContainsCode(WhsVASOrderStatuses.Codes.Cancelled));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("", filter, vasOrder_Entered, vasOrder_TransferringIn, vasOrder_Working, vasOrder_TransferringOut,
				vasOrder_FinalisedWithoutReturnTransfer, vasOrder_FinalisedWithReturnTransfer, vasOrder_WorkCompleted);

			filter.Property = WhsVASOrderStatuses.Codes.Entered;
			Asserter.AssertMatches("", filter, vasOrder_Entered);

			filter.Property = WhsVASOrderStatuses.Codes.TransferringIn;
			Asserter.AssertMatches("", filter, vasOrder_TransferringIn);

			filter.Property = WhsVASOrderStatuses.Codes.Working;
			Asserter.AssertMatches("", filter, vasOrder_Working);

			filter.Property = WhsVASOrderStatuses.Codes.WorkCompleted;
			Asserter.AssertMatches("", filter, vasOrder_WorkCompleted);

			filter.Property = WhsVASOrderStatuses.Codes.TransferringOut;
			Asserter.AssertMatches("", filter, vasOrder_TransferringOut);

			filter.Property = WhsVASOrderStatuses.Codes.Finalized;
			Asserter.AssertMatches("", filter, vasOrder_FinalisedWithReturnTransfer, vasOrder_FinalisedWithoutReturnTransfer);

			vasOrder_Entered.IsCancelled = true;
			vasOrder_TransferringIn.IsCancelled = true;
			Factory.Save();

			var inactiveFilter = (ModuleTextFilter)FilterStrip.ModuleFilters["Active Status"];
			inactiveFilter.Property = "All";

			filter.Property = WhsVASOrderStatuses.Codes.Entered;
			Asserter.AssertMatches("", filter);

			filter.Property = WhsVASOrderStatuses.Codes.TransferringIn;
			Asserter.AssertMatches("", filter);
		}

		#endregion

		#region TestActiveStatusQuery

		public void TestActiveStatusQuery()
		{
			var activeFilter = (ModuleTextFilter)FilterStrip.ModuleFilters["Active Status"];
			activeFilter.IsActive = true;
			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					activeFilter.Property = FilterStripBusinessObject.StatusAll;
					AssertEquals("", FilterStrip.Filter.LiteralTextADO);

					activeFilter.Property = FilterStripBusinessObject.StatusActive;
					AssertEquals("WVO_CancelledTimeUtc is null", FilterStrip.Filter.LiteralTextADO);

					activeFilter.Property = FilterStripBusinessObject.StatusInactive;
					AssertEquals("WVO_CancelledTimeUtc is not null", FilterStrip.Filter.LiteralTextADO);
				}
			});
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder1 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder3 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);

			Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 1m);
			Helper.CreateWhsVASOrderLine(vasOrder3, data.Part1, 1m);

			Helper.CreateWhsVASOrderLine(vasOrder2, data.Part2, 1m);
			Helper.CreateWhsVASOrderLine(vasOrder3, data.Part2, 1m);
			Factory.Save();
			Asserter.AddToScope(vasOrder1, vasOrder2, vasOrder3);

			var filter = (ModuleGuidFilter)FilterStrip.ModuleFilters[VASOrderFilterBusinessObject.FilterConstants.Product];
			filter.Property = data.Part1.PK;
			Asserter.AssertMatches("", filter, vasOrder1, vasOrder3);

			filter.Property = data.Part2.PK;
			Asserter.AssertMatches("", filter, vasOrder2, vasOrder3);

			filter.Property = Guid.NewGuid();
			Asserter.AssertMatches("", filter);
		}

		#endregion

		#region TestProductCategoryFilter

		public void TestProductCategoryFilter()
		{
			var warehouse = Helper.CreateWarehouse("Warehouse", "A");
			var client = Helper.CreateClient("Client");

			// product categories and products
			var categoryBeverage = Helper.CreateProductCategory("BEV", "Beverages");
			var categorySoftdrink = helper.CreateProductCategory("SOFTDRK", "Soft Drinks", categoryBeverage);
			var categoryBeer = helper.CreateProductCategory("BEER", "All Beers", categoryBeverage);
			var categoryDarkBeer = helper.CreateProductCategory("DARKBEER", "Dark Beers", categoryBeer);

			var productCoke = Helper.CreateProduct(client, "Coke");
			var relationCoke = productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationCoke.OU_OPC_Category = categorySoftdrink.PK;

			var productTea = Helper.CreateProduct(client, "Tea");
			var relationTea = productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationTea.OU_OPC_Category = categorySoftdrink.PK;

			var productVB = Helper.CreateProduct(client, "VB");
			var relationVB = productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationVB.OU_OPC_Category = categoryBeer.PK;

			var productGuinness = Helper.CreateProduct(client, "Guinness");
			var relationGuinness = productGuinness.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationGuinness.OU_OPC_Category = categoryDarkBeer.PK;

			// dockets
			var orderCoke = Helper.CreateWhsVASOrder(warehouse.Areas[0], client);
			Helper.CreateWhsVASOrderLine(orderCoke, productCoke, 10m);

			var orderTea = Helper.CreateWhsVASOrder(warehouse.Areas[0], client);
			Helper.CreateWhsVASOrderLine(orderTea, productTea, 10m);

			var orderVB = Helper.CreateWhsVASOrder(warehouse.Areas[0], client);
			Helper.CreateWhsVASOrderLine(orderVB, productVB, 10m);

			var orderGuinness = Helper.CreateWhsVASOrder(warehouse.Areas[0], client);
			Helper.CreateWhsVASOrderLine(orderGuinness, productGuinness, 10m);

			Factory.Save();

			Asserter.AddToScope(orderCoke, orderTea, orderVB, orderGuinness);

			var filter = (ModuleGuidFilter)FilterStrip.ModuleFilters[VASOrderFilterBusinessObject.FilterConstants.ProductCategory];
			filter.Property = categorySoftdrink.PK;
			Asserter.AssertMatches("", filter, orderCoke, orderTea);

			filter.Property = categoryBeer.PK;
			Asserter.AssertMatches("", filter, orderVB, orderGuinness);

			filter.Property = categoryDarkBeer.PK;
			Asserter.AssertMatches("", filter, orderGuinness);

			filter.Property = categoryBeverage.PK;
			Asserter.AssertMatches("", filter, orderCoke, orderTea, orderVB, orderGuinness);
		}

		#endregion

		#region TestProductDates

		public void TestProductPackingDate()
		{
			TestProductDate(VASOrderFilterBusinessObject.FilterConstants.ProductPackingDate, (l, _) => l.WVL_PackingDate = _);
		}

		public void TestProductExpiryDate()
		{
			TestProductDate(VASOrderFilterBusinessObject.FilterConstants.ProductExpiryDate, (l, _) => l.WVL_ExpiryDate = _);
		}

		void TestProductDate(string filterConstant, Action<WhsVASOrderLine, ZDate> setDate)
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder1 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);

			var line1 = Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 1m);
			var line2 = Helper.CreateWhsVASOrderLine(vasOrder2, data.Part1, 1m);

			setDate(line1, today);
			setDate(line2, today.AddDays(50));
			Factory.Save();

			Asserter.AddToScope(vasOrder1, vasOrder2);

			var filter = (ModuleDateFilter)FilterStrip.ModuleFilters[filterConstant];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals(FilterCategories.Dates, filter.Category);
			Asserter.AssertMatches("Precondition", filter, vasOrder1, vasOrder2);

			filter.Property1 = today.AddDays(-100);
			filter.Property2 = today.AddDays(100);
			Asserter.AssertMatches("", filter, vasOrder1, vasOrder2);

			filter.Property1 = today;
			filter.Property2 = today.AddDays(1);
			Asserter.AssertMatches("", filter, vasOrder1);

			filter.Property1 = today.AddDays(-1);
			filter.Property2 = today;
			Asserter.AssertMatches("", filter, vasOrder1);

			filter.Property1 = today.AddDays(1);
			filter.Property2 = today.AddDays(100);
			Asserter.AssertMatches("", filter, vasOrder2);
		}

		#endregion

		#region TestProductPartAttributes

		public void TestProductPartAttribute1()
		{
			TestProductPartAttribute(VASOrderFilterBusinessObject.FilterConstants.ProductPartAttribute1, (l, _) => l.WVL_PartAttrib1 = _);
		}

		public void TestProductPartAttribute2()
		{
			TestProductPartAttribute(VASOrderFilterBusinessObject.FilterConstants.ProductPartAttribute2, (l, _) => l.WVL_PartAttrib2 = _);
		}

		public void TestProductPartAttribute3()
		{
			TestProductPartAttribute(VASOrderFilterBusinessObject.FilterConstants.ProductPartAttribute3, (l, _) => l.WVL_PartAttrib3 = _);
		}

		public void TestSerialNumber()
		{
			TestProductPartAttribute(VASOrderFilterBusinessObject.FilterConstants.ProductSerialNumber, (l, _) => l.WVL_SerialNumber = _);
		}

		void TestProductPartAttribute(string partAttributeConstant, Action<WhsVASOrderLine, string> setPartAttrib)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();
			var vasOrder1 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(data.Whs2.Areas[0], data.Org1);

			var line1 = Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 1m);
			var line2 = Helper.CreateWhsVASOrderLine(vasOrder2, data.Part1, 1m);

			setPartAttrib(line1, "1");
			setPartAttrib(line2, "2");

			Factory.Save();

			Asserter.AddToScope(vasOrder1, vasOrder2);

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[partAttributeConstant];
			AssertEquals(FilterCategories.AttributeSearch, filter.Category);
			filter.Property = "1";
			Asserter.AssertMatches("", filter, vasOrder1);

			filter.Property = "2";
			Asserter.AssertMatches("", filter, vasOrder2);

			filter.Property = "3";
			Asserter.AssertMatches("", filter);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var data = new TestDataForInventory(Factory, new TestNotificationBuffer());
			data.CreateMultiWarehouseClientProductInventory();

			var vasOrder1 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(data.Whs2.Areas[0], data.Org1);
			Factory.Save();

			Asserter.AddToScope(vasOrder1, vasOrder2);

			var filter = (ModuleGuidFilter)FilterStrip.ModuleFilters[VASOrderFilterBusinessObject.FilterConstants.Warehouse];
			Asserter.AssertMatches("", filter, vasOrder1, vasOrder2);

			filter.Property = data.Whs1.PK;
			Asserter.AssertMatches("", filter, vasOrder1);

			filter.Property = data.Whs2.PK;
			Asserter.AssertMatches("", filter, vasOrder2);

			filter.Property = ZGuid.NewZGuid();
			Asserter.AssertMatches("", filter);
		}

		#endregion

		#region TestServiceTypeDateBooked_WithNoFields

		public void TestServiceTypeDateBooked_WithNoFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "All VasOrders should be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					bookedFilter.IsActive = true;
				},
				(vasOrder1, vasOrder2) => new[] { vasOrder1, vasOrder2 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithDateField

		public void TestServiceTypeDateBooked_WithDateField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "VasOrders with matching date booked should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					bookedFilter.Property2 = filterDate;
					bookedFilter.IsActive = true;
				},
				(vasOrder1, vasOrder2) => new[] { vasOrder2 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithServiceTypeField

		public void TestServiceTypeDateBooked_WithServiceTypeField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "VasOrders with matching service type should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.JobServiceType = "FUM";
					bookedFilter.IsActive = true;
				},
				(vasOrder1, vasOrder2) => new[] { vasOrder2 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithBothDateAndServiceTypeFields

		public void TestServiceTypeDateBooked_WithBothDateAndServiceTypeFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "VasOrders with matching service type and date booked should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.JobServiceType = "FUM";
					bookedFilter.Property2 = filterDate;
					bookedFilter.IsActive = true;
				},
				(vasOrder1, vasOrder2) => new[] { vasOrder2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithNoFields

		public void TestServiceTypeDateCompleted_WithNoFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "All VasOrders should be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					completedFilter.IsActive = true;
				},
				(vasOrder1, vasOrder2) => new[] { vasOrder1, vasOrder2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithDateField

		public void TestServiceTypeDateCompleted_WithDateField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "VasOrders with matching completed date should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					completedFilter.Property2 = filterDate;
					completedFilter.IsActive = true;
				},
				(vasOrder1, vasOrder2) => new[] { vasOrder2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithServiceTypeField

		public void TestServiceTypeDateCompleted_WithServiceTypeField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "VasOrders with matching service type should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.JobServiceType = "FUM";
					completedFilter.IsActive = true;
				},
				(vasOrder1, vasOrder2) => new[] { vasOrder2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithBothDateAndServiceTypeFields

		public void TestServiceTypeDateCompleted_WithBothDateAndServiceTypeFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "VasOrders with matching service type and completed date should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.JobServiceType = "FUM";
					completedFilter.Property2 = filterDate;
					completedFilter.IsActive = true;
				},
				(vasOrder1, vasOrder2) => new[] { vasOrder2 });
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			var data = new TestDataForInventory(Factory, new TestNotificationBuffer());
			data.CreateMultiWarehouseClientProductInventory();

			var vasOrder1 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var job1 = new JobHeader.Loader(vasOrder1).TryLoadOrCreate();
			job1.JH_ProfitLossReasonCode = "ND1";

			var vasOrder2 = Helper.CreateWhsVASOrder(data.Whs2.Areas[0], data.Org1);
			var job2 = new JobHeader.Loader(vasOrder2).TryLoadOrCreate();
			job2.JH_ProfitLossReasonCode = "CD1";

			var vasOrder3 = Helper.CreateWhsVASOrder(data.Whs2.Areas[0], data.Org1);
			var job3 = new JobHeader.Loader(vasOrder3).TryLoadOrCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;

			Asserter.AddToScope(vasOrder1, vasOrder2, vasOrder3);
			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)FilterStrip["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for vasOrder1.", profitLossReasonFilter, vasOrder1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for vasOrder1.", profitLossReasonFilter, vasOrder1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			Asserter.AssertMatches("Has a Match for vasOrder1 and vasOrder2", profitLossReasonFilter, vasOrder1, vasOrder2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for vasOrder2 and vasOrder3", profitLossReasonFilter, vasOrder2, vasOrder3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for vasOrder2 and vasOrder3", profitLossReasonFilter, vasOrder2, vasOrder3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for vasOrder2 and vasOrder3", profitLossReasonFilter, vasOrder2, vasOrder3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			Asserter.AssertMatches("Has a Match for vasOrder3", profitLossReasonFilter, vasOrder3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for vasOrder1 and vasOrder2", profitLossReasonFilter, vasOrder1, vasOrder2);
		}

		#region Implementation

		void TestServiceTypeFilters(string filterName, ZDate filterDate, string assertionMessage, Action<ServiceTypeDateFilter> setupFilter, Func<WhsVASOrder, WhsVASOrder, IEnumerable<WhsVASOrder>> getExpectedJobs)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder1 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var testDate = new ZDate(2023, 12, 08);

			var fumigationService = vasOrder2.Services.AddNew();
			fumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			if (filterName.Equals(ServiceTypeDateFilter.ServiceTypeDateCompleted))
			{
				fumigationService.ES_Completed = filterDate;
			}
			else
			{
				fumigationService.ES_Booked = filterDate;
			}

			Factory.Save();
			Asserter.AddToScope(vasOrder1, vasOrder2);

			var filter = (ServiceTypeDateFilter)FilterStrip.ModuleFilters[filterName];
			setupFilter(filter);

			Asserter.AssertMatches(assertionMessage, filter, getExpectedJobs(vasOrder1, vasOrder2).ToArray());
		}

		FilterStripAsserter<WhsVASOrder> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsVASOrder>(Factory, v => v.WVO_JobID));
		FilterStripAsserter<WhsVASOrder> asserter;

		VASOrderFilterBusinessObject FilterStrip => filterStrip ?? (filterStrip = GetNewFilterStrip());
		VASOrderFilterBusinessObject filterStrip;

		VASOrderFilterBusinessObject GetNewFilterStrip() => new VASOrderFilterBusinessObject();

		WhsTestHelperFunctions Helper => helper = helper ?? new WhsTestHelperFunctions(Factory);
		WhsTestHelperFunctions helper;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new VASOrderFilterBusinessObject();
		}

		#endregion
	}

	#region AccountingFilterStripTest

	public class VASOrderFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<WhsVASOrder>
	{
		protected override WhsVASOrder GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<WhsVASOrder>();
		}

		protected override ModuleIdentifier FilterStripModuleID => ModuleIDs.WhsVASOrder;
	}

	#endregion

}
