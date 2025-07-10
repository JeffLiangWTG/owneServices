using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDeferTriggerStrategiesTest : WhsTestCaseWithFactory
	{
		#region TestWhsCheckSerialNumberQuantityMatchWithAsnLine_DeferTriggerStrategy

		public void TestWhsCheckSerialNumberQuantityMatchWithAsnLine_DeferTriggerStrategy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var recieve = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var asnLine = Helper.CreateAsnLine(recieve, data.Part1, 1m);
			var serialNumber = Helper.CreateWhsSerialNumber(data.Org1, data.Part1, "SN1");
			Helper.CreateWhsSerialNumberPivot(asnLine, serialNumber);
			Factory.Save();

			var clientKey = "WhsDocket_Client|" + recieve.PK;
			var productKey = "WhsProduct|" + data.Part1.PK;
			Factory.ClearCachedValue<OrgHeader>(clientKey);
			Factory.ClearCachedValue<WhsProduct>(productKey);
			Factory.TryGetValueFromCacheOnly(clientKey, out OrgHeader clientValue);
			Factory.TryGetValueFromCacheOnly(productKey, out WhsProduct productValue);
			AssertEquals("Precondition", null, clientValue);
			AssertEquals("Precondition", null, productValue);

			var strategy = ObjectFactory.Get<IWhsCheckSerialNumberQuantityMatchWithAsnLine_DeferTriggerStrategy>() as IDeferTriggerConditionStrategy;

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition - should not be defer trigger if there are no changes for the line.", false, strategy.ShouldDeferTrigger(asnLine));

				AssertEquals("Precondition", 0, asnLine.WN_LineNo);
				asnLine.WN_LineNo = 20;
				AssertEquals("Should defer if not relevant property changes.", false, strategy.ShouldDeferTrigger(asnLine));
				asnLine.WN_LineNo = 0; // clean up
				AssertEquals("Should not defer after clean up.", false, strategy.ShouldDeferTrigger(asnLine));

				asnLine.WN_Quantity = 2m;
				AssertEquals("Should defer if relevant property changes.", true, strategy.ShouldDeferTrigger(asnLine));

				Factory.TryGetValueFromCacheOnly(clientKey, out clientValue);
				Factory.TryGetValueFromCacheOnly(productKey, out productValue);
				AssertEquals("Should cache client.", recieve.Client, clientValue);
				AssertEquals("Should cache product.", data.Part1, productValue.Parent);

				var relation = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, "OWN");
				relation.OU_UseSerialNumber = false;
				AssertEquals("Should not defer even serial number not used.", false, strategy.ShouldDeferTrigger(asnLine));
				relation.OU_UseSerialNumber = true; // clean up
				AssertEquals("Should still defer TransactionQuantity is changed.", true, strategy.ShouldDeferTrigger(asnLine));
			}

			AssertEquals("Should not defer if EnableSchemaRedesign is false.", false, strategy.ShouldDeferTrigger(asnLine));
			asnLine.WN_Quantity = 1m; // clean up

			Factory.Save();
			Factory.TryGetValueFromCacheOnly(clientKey, out clientValue);
			AssertEquals("Should clean cache after save ", null, clientValue);
		}

		#endregion

	}
}
