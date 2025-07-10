using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderGeneratePickProcessorTest : WhsTestCaseWithFactory
	{
		#region TestWhsOrderGeneratePickProcessor_ConstructorNotNull

		public void TestWhsOrderGeneratePickProcessor_ConstructorNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>("Order cannot be null.",
				() => new WhsOrderGeneratePickProcessor(null));
		}

		#endregion

		#region TestWhsOrderGeneratePickProcessor

		public void TestWhsOrderGeneratePickProcessor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			Factory.Save();
			AssertEquals("Precondition - order does not have pick", null, order.Pick);

			IProcessor processor = new WhsOrderGeneratePickProcessor(order);
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				processor.Process(Notify);
			}

			AssertNull("Should have not saved the Pick in Factory.",
				new BusinessObjectFactory().Load<WhsOrder>(order.PK).Pick);

			Factory.Save();
			AssertNotNull("Should create Pick and attach order to it.",
				new BusinessObjectFactory().Load<WhsOrder>(order.PK).Pick);
		}

		#endregion

		#region TestWhsOrderGeneratePickProcessor_DoesNotCallFactorySave

		public void TestWhsOrderGeneratePickProcessor_DoesNotCallFactorySave_AutoPick()
		{
			TestWhsOrderGeneratePickProcessor_DoesNotCallFactorySaveCore(WhsPickOption.Codes.Auto);
		}

		public void TestWhsOrderGeneratePickProcessor_DoesNotCallFactorySave_ManualPick()
		{
			TestWhsOrderGeneratePickProcessor_DoesNotCallFactorySaveCore(WhsPickOption.Codes.Manual);
		}

		void TestWhsOrderGeneratePickProcessor_DoesNotCallFactorySaveCore(string pickOption)
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			order.WD_PickOption = pickOption;
			Factory.Save();
			AssertEquals("Precondition - order does not have pick", null, order.Pick);

			using (new FactorySaveAlerterForTest())
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				IProcessor processor = new WhsOrderGeneratePickProcessor(order);
				AssertNoExceptionThrown(() => processor.Process(Notify));
			}

			AssertNull("Should have not saved the Pick in Factory.",
				new BusinessObjectFactory().Load<WhsOrder>(order.PK).Pick);

			Factory.Save();
			AssertNotNull("Should create Pick and attach order to it.",
				new BusinessObjectFactory().Load<WhsOrder>(order.PK).Pick);
		}

		#endregion

		#region TestWhsOrderGeneratePickProcessor_NoAllocationRules

		[GuiTest]
		public void TestWhsOrderGeneratePickProcessor_NoAllocationRules()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product1 = Helper.CreateProduct(client, "P1");
			var receive = Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R1", product1, 5);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "O1", product1, 6);
			Factory.Save();

			var allocationRuleSetQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWA");
			allocationRuleSetQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var allocationRuleSet = Factory.LoadTop1<ProductionRuleSet>(allocationRuleSetQuery);
			allocationRuleSet.PRS_IsLive = false;
			Factory.Save();

			var expectedMessage = "No Stock was allocated. Issue occurred during allocation: No rules found for context: 'PWA'";

			IProcessor processor = new WhsOrderGeneratePickProcessor(order);
			processor.Process(Notify);
			var e = Notify.LastEvent;

			AssertNull("Should not create Pick and attach order to it.", new BusinessObjectFactory().Load<WhsOrder>(order.PK).Pick);
			AssertEquals("Should have this error message", expectedMessage, e.Message);
		}

		#endregion
	}
}
