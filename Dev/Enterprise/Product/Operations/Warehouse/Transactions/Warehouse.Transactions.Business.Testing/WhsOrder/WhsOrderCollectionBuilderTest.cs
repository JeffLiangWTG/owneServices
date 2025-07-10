using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderCollectionBuilderTest : WhsDocketCollectionBuilderTestCase<WhsOrder, OrderLineData>
	{
		#region TestFindDocket_ConsigneeDocAddress

		public void TestFindDocket_ConsigneeDocAddress()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var client = Helper.CreateClient("1");
			var consignee = Helper.CreateClient("4");

			var orgAddress1 = consignee.Addresses.AddNew();
			var orgAddress2 = consignee.Addresses.AddNew();

			var data = new OrderLineData();
			data.OrgPK = client.PK;
			data.WhsPK = whs.PK;
			data.DocketSubType = CodeLists.OrderType.Codes.Order;
			data.Quantity = 1;

			var builder = new WhsOrderCollectionBuilder(Factory);

			data.ExternalReference = "ORDER1";
			var order1 = (WhsOrder)builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket;

			data.ExternalReference = "ORDER2";
			var order2 = (WhsOrder)builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket;

			data.ExternalReference = "ORDER3";
			var order3 = (WhsOrder)builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket;

			data.ExternalReference = "ORDER4";
			var order4 = (WhsOrder)builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket;

			order1.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			order1.ConsigneeDocAddress.E2_OA_Address = orgAddress1.PK;
			order2.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			order2.ConsigneeDocAddress.E2_OA_Address = orgAddress2.PK;
			order3.ConsigneeDocAddress.E2_AddressOverride = true;
			order3.ConsigneeDocAddress.E2_CompanyName = "OVERRIDEN 1";
			order4.ConsigneeDocAddress.E2_AddressOverride = true;
			order4.ConsigneeDocAddress.E2_CompanyName = "OVERRIDEN 2";
			order1.WD_ExternalReference = "";
			order2.WD_ExternalReference = "";
			order3.WD_ExternalReference = "";
			order4.WD_ExternalReference = "";

			data.ExternalReference = "";
			data.ConsigneeDocAddress = order2.ConsigneeDocAddress;
			AssertEquals(order2, builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket);

			data.ConsigneeDocAddress = order1.ConsigneeDocAddress;
			AssertEquals(order1, builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket);

			data.ConsigneeDocAddress = order4.ConsigneeDocAddress;
			AssertEquals(order4, builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket);

			data.ConsigneeDocAddress = order3.ConsigneeDocAddress;
			AssertEquals(order3, builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket);
		}

		#endregion

		#region TestFindDocket_DocketSubType

		public void TestFindDocket_DocketSubType()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var client = Helper.CreateClient("1");

			var data = new OrderLineData();
			data.OrgPK = client.PK;
			data.WhsPK = whs.PK;
			data.Quantity = 10m;

			var builder = new WhsOrderCollectionBuilder(Factory);

			data.ExternalReference = "ORDER1";
			var order1 = (WhsOrder)builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket;

			data.ExternalReference = "ORDER2";
			var order2 = (WhsOrder)builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket;

			data.ExternalReference = "ORDER3";
			var order3 = (WhsOrder)builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket;

			order1.WD_DocketSubType = CodeLists.OrderType.Codes.Order;
			order2.WD_DocketSubType = CodeLists.OrderType.Codes.Customs;
			order3.WD_DocketSubType = CodeLists.OrderType.Codes.CustomsReleaseWithPermit;
			order1.WD_ExternalReference = "";
			order2.WD_ExternalReference = "";
			order3.WD_ExternalReference = "";

			data.ExternalReference = "";
			data.DocketSubType = CodeLists.OrderType.Codes.Customs;
			AssertEquals(order2.PK, builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket.PK);

			data.DocketSubType = CodeLists.OrderType.Codes.Order;
			AssertEquals(order1.PK, builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket.PK);

			data.DocketSubType = CodeLists.OrderType.Codes.Customs;
			AssertEquals(order2.PK, builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket.PK);

			data.DocketSubType = CodeLists.OrderType.Codes.CustomsReleaseWithPermit;
			AssertEquals(order3.PK, builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None).Docket.PK);
		}

		#endregion

		#region TestAddLine_WithOptions_Dockets

		[TestDate(2010, 4, 16)]
		public void TestAddLine_WithOptions_Dockets()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 1m, 2m, 3m }, false);

			var inventory1 = (WhsReceiveLine)data.Receive11.Inventory[0].InDocketLine;
			var inventory2 = (WhsReceiveLine)data.Receive11.Inventory[1].InDocketLine;
			var inventory3 = (WhsReceiveLine)data.Receive11.Inventory[2].InDocketLine;

			Factory.Save(); // to generate DockeLine

			var consignee = Helper.CreateClient("CONSIGNEE");
			consignee.OH_IsConsignee = true;

			inventory2.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			inventory3.ConsigneeDocAddress.E2_AddressOverride = true;
			inventory3.ConsigneeDocAddress.E2_CompanyName = "CONSIGNEE"; // same as code for dataLine112
			var emptyConsigneeAddress = inventory1.ConsigneeDocAddress;
			var setConsigneeAddress = inventory2.ConsigneeDocAddress;
			var overridenConsigneeAddress = inventory3.ConsigneeDocAddress;

			var lineData = new OrderLineData();
			var options = WhsDocketCollectionBuilderLineOptions.MergeLine;

			lineData.WhsPK = data.Whs1.PK;
			lineData.OrgPK = data.Org1.PK;
			lineData.PartPK = data.Part1.PK;
			lineData.Quantity = 1m;

			// all fields are empty
			lineData.ExternalReference = "";
			lineData.ConsigneeDocAddress = emptyConsigneeAddress;
			lineData.RequiredDateOffset = ZDateTimeOffset.Empty;
			Builder.AddLine(lineData, options);

			// with set Order No
			lineData.ExternalReference = "OR1";
			Builder.AddLine(lineData, options);
			lineData.ExternalReference = ""; // clean up

			// with set Consignee.
			lineData.ConsigneeDocAddress = setConsigneeAddress;
			Builder.AddLine(lineData, options);

			// with set Overriden Consignee
			lineData.ConsigneeDocAddress = overridenConsigneeAddress;
			Builder.AddLine(lineData, options);
			lineData.ConsigneeDocAddress = emptyConsigneeAddress; // clean up

			// with set Required Date
			var requiredByDate = ZDateTimeOffset.Today.EndOfDay().AddSeconds(-59);

			lineData.RequiredDateOffset = requiredByDate;
			Builder.AddLine(lineData, options);

			AssertEquals("Should have created Dockets for all combinations of ExternalReference, Consignee and Required Date", 5, Builder.Dockets.Count);
			AssertAddLine_CreatedCorrectDocket(Builder.Dockets, data.Org1, data.Whs1, "", emptyConsigneeAddress, ZDateTimeOffset.Empty);
			AssertAddLine_CreatedCorrectDocket(Builder.Dockets, data.Org1, data.Whs1, "OR1", emptyConsigneeAddress, ZDateTimeOffset.Empty);
			AssertAddLine_CreatedCorrectDocket(Builder.Dockets, data.Org1, data.Whs1, "", setConsigneeAddress, ZDateTimeOffset.Empty);
			AssertAddLine_CreatedCorrectDocket(Builder.Dockets, data.Org1, data.Whs1, "", overridenConsigneeAddress, ZDateTimeOffset.Empty);
			AssertAddLine_CreatedCorrectDocket(Builder.Dockets, data.Org1, data.Whs1, "", emptyConsigneeAddress, requiredByDate);
		}

		void AssertAddLine_CreatedCorrectDocket(IEnumerable<WhsOrder> allDockets, OrgHeader client, WhsWarehouse whs, ZString externalReference, JobDocAddress consigneeDocAddress, ZDateTimeOffset requiredDate)
		{
			var docket = allDockets.FindDocket(whs.PK, client.PK, externalReference, d => d.ConsigneeDocAddress.IsTheSameAddressAs(consigneeDocAddress), requiredDate, true);
			AssertNotNull("Docket with set paramns wasn't found.", docket);
		}

		#endregion

		#region Finalise All Dockets

		protected override void ProcessDocketBeforeTestFinaliseAllDockets(WhsOrder docket)
		{
			base.ProcessDocketBeforeTestFinaliseAllDockets(docket);

			foreach (var line in docket.Lines)
			{
				line.WE_WL = ZGuid.Empty;
			}

			docket.WD_RequiredDate = ZDateTimeOffset.Today;
			docket.ConsigneePK = docket.Client.PK;
			docket.ConsigneeAddressPK = Factory.Load<OrgHeader>(docket.Client.PK).MainAddress.PK;

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(docket);
		}

		#endregion

		#region TestIsTheSameDocAddress

		public void TestIsTheSameDocAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var consignee1 = Helper.CreateClient("AAA");
			var consignee2 = Helper.CreateClient("BBB");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order1.ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			order2.ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);
			order3.ConsigneeDocAddress.OrganisationPK = consignee2.PK;

			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 1m);
			order4.ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			order4.ConsigneeDocAddress.E2_AddressOverride = true;
			order4.ConsigneeDocAddress.E2_Address1 = "SOME ADDRESS";
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 1m);
			order5.ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			order5.ConsigneeDocAddress.E2_AddressOverride = true;
			order5.ConsigneeDocAddress.E2_Address1 = "SOME ADDRESS";
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 1m);
			order6.ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			order6.ConsigneeDocAddress.E2_AddressOverride = true;
			order6.ConsigneeDocAddress.E2_Address1 = "ANOTHER ADDRESS";
			var order7 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O7", data.Part1, 1m);
			order7.ConsigneeDocAddress.OrganisationPK = consignee2.PK;
			order7.ConsigneeDocAddress.E2_AddressOverride = true;
			order7.ConsigneeDocAddress.E2_Address1 = "ANOTHER ADDRESS";

			Factory.Save();

			AssertEquals(true, WhsOrderCollectionBuilder.IsTheSameDocAddress(order1.ConsigneeDocAddress, order2.ConsigneeDocAddress));
			AssertEquals(false, WhsOrderCollectionBuilder.IsTheSameDocAddress(order1.ConsigneeDocAddress, order3.ConsigneeDocAddress));
			AssertEquals(false, WhsOrderCollectionBuilder.IsTheSameDocAddress(order1.ConsigneeDocAddress, order4.ConsigneeDocAddress));
			AssertEquals(true, WhsOrderCollectionBuilder.IsTheSameDocAddress(order4.ConsigneeDocAddress, order5.ConsigneeDocAddress));
			AssertEquals(false, WhsOrderCollectionBuilder.IsTheSameDocAddress(order4.ConsigneeDocAddress, order6.ConsigneeDocAddress));
			AssertEquals(false, WhsOrderCollectionBuilder.IsTheSameDocAddress(order6.ConsigneeDocAddress, order7.ConsigneeDocAddress));
		}

		#endregion

		#region Implementation

		protected override string SetTestAddLineDocketSubType => CodeLists.OrderType.Codes.Order;

		protected override WhsDocketCollectionBuilder<WhsOrder, OrderLineData> GetNewDocketBuilder(BusinessObjectFactory factory)
		{
			return new WhsOrderCollectionBuilder(factory);
		}

		protected override WhsOrder GetNewDocket()
		{
			return Factory.New<WhsOrder>();
		}

		protected new WhsOrderCollectionBuilder Builder
		{
			get { return (WhsOrderCollectionBuilder)base.Builder; }
			set { base.Builder = value; }
		}

		#endregion
	}
}
