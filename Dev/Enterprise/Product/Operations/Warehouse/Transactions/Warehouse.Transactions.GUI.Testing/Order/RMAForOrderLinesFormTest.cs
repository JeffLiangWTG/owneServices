using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(RMAForOrderLinesForm))]
	public class RMAForOrderLinesFormTest : ZFormBasherTest
	{
		#region TestOnload_AttributeVisibility

		public void TestOnload_AttributeVisibility()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org1 = Helper.CreateClient("CO1", "TestCO1");
			var org2 = Helper.CreateClient("CO2", "TestCO2");
			var org3 = Helper.CreateClient("CO3", "TestCO3");

			Helper.SetClientAttributeType(org2, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Model #");
			Helper.SetClientAttributeType(org2, AttributeNumber.Two, PartAttributeTypeList.Codes.VIN, "Vehicle #");
			Helper.SetClientAttributeType(org2, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Color");
			Helper.SetClientAttributeType(org2, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(org2, AttributeNumber.PackingDate, true);

			Helper.SetClientAttributeType(org3, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory, "Batch #");
			Helper.SetClientAttributeType(org3, AttributeNumber.PackingDate, true);

			using (var form = GetForm(data, org1))
			{
				form.Show();

				AssertAttributeVisibility(form.GridForTest, false, false, false, false, false);
			}

			using (var form = GetForm(data, org2))
			{
				form.Show();

				AssertAttributeVisibility(form.GridForTest, true, true, true, true, true);
				AssertAttributeTitles(form.GridForTest, "Model #", "Vehicle #", "Color");
			}

			using (var form = GetForm(data, org3))
			{
				form.Show();

				AssertAttributeVisibility(form.GridForTest, false, true, false, true, false);
				AssertAttributeTitles(form.GridForTest, "", "Batch #", "");
			}
		}

		public void TestOnload_AttributeVisibility_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org = Helper.CreateClient("ABC", "Test");

			Helper.SetClientAttributeType(org, AttributeNumber.Serial, true);

			using (var form = GetForm(data, org))
			{
				form.Show();
				var serialNumberColumn = form.GridForTest.Columns["SerialNumber"];
				AssertEquals(true, serialNumberColumn != null);
				AssertEquals("Serial Number column visibility is correct.", true, serialNumberColumn.IsVisible);
			}
		}

		RMAForOrderLinesForm GetForm(TestDataSimpleEnvironment data, OrgHeader client)
		{
			Helper.CreateProductClientRelationShip(client, data.Part1);
			var receive = Helper.CreateWhsReceive(client, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			return new RMAForOrderLinesForm(rmaOrder);
		}

		void AssertAttributeVisibility(ZGrid grid, bool expiry, bool packing, bool part1, bool part2, bool part3)
		{
			AssertEquals("Expiry visibility incorrect", expiry, grid.Columns["ExpiryDate"].IsVisible);
			AssertEquals("Packing visibility incorrect", packing, grid.Columns["PackingDate"].IsVisible);
			AssertEquals("PartAttrib1 visibility incorrect", part1, grid.Columns["PartAttrib1"].IsVisible);
			AssertEquals("PartAttrib2 visibility incorrect", part2, grid.Columns["PartAttrib2"].IsVisible);
			AssertEquals("PartAttrib3 visibility incorrect", part3, grid.Columns["PartAttrib3"].IsVisible);
		}

		void AssertAttributeTitles(ZGrid grid, string part1, string part2, string part3)
		{
			if (!string.IsNullOrEmpty(part1))
			{
				AssertEquals("PartAttrib1 title incorrect", part1, grid.Columns["PartAttrib1"].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part2))
			{
				AssertEquals("PartAttrib2 title incorrect", part2, grid.Columns["PartAttrib2"].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part3))
			{
				AssertEquals("PartAttrib3 title incorrect", part3, grid.Columns["PartAttrib3"].ColumnStyle.HeaderText);
			}
		}

		#endregion

		#region CommonOKClick

		#region TestCommonOKClick_Warning

		public void TestCommonOKClick_Warning()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var order = GetValidOrder(data, 2);
			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			AssertEquals("Precondition:", 0, order.RelatedJobs.Count);
			AssertEquals("Precondition:", 2, rmaOrder.Lines.Count);

			using (var form = new RMAForOrderLinesForm(rmaOrder))
			{
				form.Show();
				form.CommonOKClickForTest();
				AssertEquals("Please at least enter an amount more than 0 on one Line.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestCommonOKClick_Error

		public void TestCommonOKClick_Error()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var order = GetValidOrder(data, 2);
			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			AssertEquals("Precondition:", 0, order.RelatedJobs.Count);
			AssertEquals("Precondition:", 2, rmaOrder.Lines.Count);

			using (var form = new RMAForOrderLinesForm(rmaOrder))
			{
				rmaOrder.Lines[0].QuantityToReturn = 11m;
				AssertEquals(true, rmaOrder.HasErrors());
				form.CommonOKClickForTest();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestCommonOKClick

		public void TestCommonOKClick_GenerateAllLines()
		{
			TestCommonOKClickCore(2, true);
		}

		public void TestCommonOKClick_GenerateLinesByEnteredAmountMoreThanZero()
		{
			TestCommonOKClickCore(1, false);
		}

		void TestCommonOKClickCore(int expectedGenLinesCount, bool isGenerateAllLines)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var order = GetValidOrder(data, expectedGenLinesCount, isGenerateAllLines);
			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			var expectedLogText = "Following Receives have been generated and can be accessed on Order [W00000002] - Related Jobs tab:\r\n" +
				"W00000003";
			AssertEquals("Precondition:", isGenerateAllLines ? expectedGenLinesCount : expectedGenLinesCount + 1, rmaOrder.Lines.Count);

			using (var form = new RMAForOrderLinesForm(rmaOrder))
			{
				form.Show();

				for (int i = 0; i < expectedGenLinesCount; i++)
				{
					rmaOrder.Lines[i].QuantityToReturn = 10m;
				}
				form.CommonOKClickForTest();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(expectedLogText, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Order should Link to New Receive", 1, order.RelatedJobs.Count);

				var newReceive = Factory.Load<WhsReceive>(order.RelatedJobs[0].BusinessObjectPK);
				AssertReceive(data, newReceive, order.PK, expectedGenLinesCount, data.Whs1);
			}
		}

		public void TestCommonOKClickWithWhsOverride_GenerateAllLines()
		{
			TestCommonOKClickWithWhsOverrideCore(2, true);
		}

		public void TestCommonOKClickWithWhsOverride_GenerateLinesByEnteredAmountMoreThanZero()
		{
			TestCommonOKClickWithWhsOverrideCore(1, false);
		}

		void TestCommonOKClickWithWhsOverrideCore(int expectedGenLinesCount, bool isGenerateAllLines)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newWarehouse = Helper.CreateWarehouse("W2");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var order = GetValidOrder(data, expectedGenLinesCount, isGenerateAllLines);
			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			var expectedLogText = "Following Receives have been generated and can be accessed on Order [W00000002] - Related Jobs tab:\r\n" +
				"W00000003";

			AssertEquals("Precondition:", isGenerateAllLines ? expectedGenLinesCount : expectedGenLinesCount + 1, rmaOrder.Lines.Count);
			using (var form = new RMAForOrderLinesForm(rmaOrder))
			{
				form.Show();

				for (int i = 0; i < expectedGenLinesCount; i++)
				{
					rmaOrder.Lines[i].QuantityToReturn = 10m;
					rmaOrder.Lines[i].WhsOverride = newWarehouse.PK;
				}
				form.CommonOKClickForTest();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(expectedLogText, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Order should Link to New Receive", 1, order.RelatedJobs.Count);

				var newReceive = Factory.Load<WhsReceive>(order.RelatedJobs[0].BusinessObjectPK);
				AssertReceive(data, newReceive, order.PK, expectedGenLinesCount, newWarehouse);
			}
		}
		
		WhsOrder GetValidOrder(TestDataSimpleEnvironment data, int expectedGenLinesCount, bool isGenerateAllLines = true)
		{
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var date = ZDate.Today.AddDays(1);

			expectedGenLinesCount = isGenerateAllLines ? expectedGenLinesCount : expectedGenLinesCount + 1;

			for (int i = 0; i < expectedGenLinesCount; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, date, date, "PA1" + i, "PA2", "PA3", "");
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			for (int i = 0; i < expectedGenLinesCount; i++)
			{
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			}
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			return order;
		}

		void AssertReceive(TestDataSimpleEnvironment data, WhsReceive receive, ZGuid orderPK, int expectedGenLinesCount, WhsWarehouse expectedWarehouse)
		{
			AssertNotNull(receive);
			AssertEquals(data.Org1.PK, receive.WD_OH_Client);
			AssertEquals(expectedWarehouse.PK, receive.WD_WW_Whs);
			AssertEquals(CodeLists.ReceiveType.Codes.Returns, receive.WD_DocketSubType);
			AssertEquals("Reference", "O1 " + receive.WD_DocketID, receive.WD_ExternalReference);
			AssertEquals("Should link to Order", 1, receive.RelatedJobs.Count);
			AssertEquals("Should link to Order", orderPK, receive.RelatedJobs[0].BusinessObjectPK);

			AssertEquals("Should have receive lines", expectedGenLinesCount, receive.Lines.Count);
			for (int i = 0; i < expectedGenLinesCount; i++)
			{
				AssertEquals(data.Part1.PK, receive.Lines[i].WE_OP);
				AssertEquals(10m, receive.Lines[i].WE_TransactionQuantity);
				AssertEquals(ZDate.Today.AddDays(1), receive.Lines[i].WE_ExpiryDate);
				AssertEquals(ZDate.Today.AddDays(1), receive.Lines[i].WE_PackingDate);
				AssertEquals("PA1" + i, receive.Lines[i].WE_PartAttrib1);
				AssertEquals("PA2", receive.Lines[i].WE_PartAttrib2);
				AssertEquals("PA3", receive.Lines[i].WE_PartAttrib3);
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			Factory.Save();
			return new RMAForOrderLinesForm(rmaOrder);
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
