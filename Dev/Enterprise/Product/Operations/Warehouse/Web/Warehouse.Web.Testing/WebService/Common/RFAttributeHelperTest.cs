using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	public class RFAttributeHelperTest : TestCaseWithFactory
	{
		#region Constructors

		public void TestGetRFAttributeConfirm()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);

			var helper = new WhsTestHelperFunctions(factory);
			var org2 = helper.CreateClient();

			AssertEquals(0, RFAttributeHelper.GetRFAttributeConfirm(null, null));
			AssertEquals(0, RFAttributeHelper.GetRFAttributeConfirm(data.Part1, null));
			AssertEquals(0, RFAttributeHelper.GetRFAttributeConfirm(null, data.Org1));
			AssertEquals(0, RFAttributeHelper.GetRFAttributeConfirm(data.Part1, org2));

			AssertEquals(RFAttributeConfirmCode.Codes.None, data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm);
			AssertEquals(0, RFAttributeHelper.GetRFAttributeConfirm(data.Part1, data.Org1));

			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertEquals(1, RFAttributeHelper.GetRFAttributeConfirm(data.Part1, data.Org1));

			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;
			AssertEquals(2, RFAttributeHelper.GetRFAttributeConfirm(data.Part1, data.Org1));

			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertEquals(3, RFAttributeHelper.GetRFAttributeConfirm(data.Part1, data.Org1));

			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			AssertEquals(4, RFAttributeHelper.GetRFAttributeConfirm(data.Part1, data.Org1));
		}

		#endregion

		#region TestAddScannedReleaseCapturedSerialNumbers

		public void TestAddScannedReleaseCapturedSerialNumbers()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);

			var helper = new WhsTestHelperFunctions(factory);
			var org1 = data.Org1;
			var org2 = helper.CreateClient("ORG2");
			helper.CreateProductClientRelationShip(org2, data.Part1);

			helper.SetClientAttributeType(org1, AttributeNumber.Serial, true);
			helper.SetClientAttributeType(org2, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(org1, data.Part1, AttributeNumber.Serial, true, true);
			helper.SetProductAttributeUse(org2, data.Part1, AttributeNumber.Serial, true, true);
			factory.Save();

			helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 10m);
			factory.Save();

			var order1 = helper.CreateWhsOrder(org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var order2 = helper.CreateWhsOrder(org2, data.Whs1, "O1");
			var orderLine2 = helper.CreateWhsOrderLine(order2, data.Part1, 2m);

			var pick = helper.CreatePickNew(order1, order2);
			factory.Save();

			orderLine1.ReleaseLines.AddNew("", "", "", "S1", ZDate.Empty, ZDate.Empty, 1m);
			orderLine1.ReleaseLines.AddNew("", "", "", "S2", ZDate.Empty, ZDate.Empty, 1m);
			orderLine2.ReleaseLines.AddNew("", "", "", "S3", ZDate.Empty, ZDate.Empty, 1m);
			orderLine2.ReleaseLines.AddNew("", "", "", "S4", ZDate.Empty, ZDate.Empty, 1m);
			factory.Save();

			var scannedRCASerialNumbers = new List<ScannedRCASerialNumbersPerProductInfo>();
			RFAttributeHelper.AddScannedReleaseCapturedSerialNumbers(scannedRCASerialNumbers, pick.GetAllPickLines());

			AssertEquals("Should have 4 lines that are grouped by Product and Client.", 2, scannedRCASerialNumbers.Count);
			var scannedRCASerialNumbersForPart1Org1 = scannedRCASerialNumbers.Single(s => s.ProductPK == data.Part1.PK.ToGuid() && s.ClientPK == org1.PK.ToGuid());
			AssertContainsExactElementsInAnyOrder(new[] { "S1", "S2" }, scannedRCASerialNumbersForPart1Org1.ScannedRCASerialNumbers);

			var scannedRCASerialNumbersForPart1Org2 = scannedRCASerialNumbers.Single(s => s.ProductPK == data.Part1.PK.ToGuid() && s.ClientPK == org2.PK.ToGuid());
			AssertContainsExactElementsInAnyOrder(new[] { "S3", "S4" }, scannedRCASerialNumbersForPart1Org2.ScannedRCASerialNumbers);
		}

		public void TestAddScannedReleaseCapturedSerialNumbers_DBHits()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var helper = new WhsTestHelperFunctions(factory);
			var pickLines = new List<WhsPickLine>();

			for (int i = 0; i < 10; i++)
			{
				var client1 = helper.CreateClient("ORG1" + i);
				var client2 = helper.CreateClient("ORG2" + i);
				var part1 = helper.CreateProduct("Product1" + i, client1);
				var part2 = helper.CreateProduct("Product2" + i, client2);
				helper.SetClientAttributeType(client1, AttributeNumber.Serial, true);
				helper.SetProductAttributeUse(client1, part1, AttributeNumber.Serial, true, true);
				helper.SetClientAttributeType(client2, AttributeNumber.Serial, true);
				helper.SetProductAttributeUse(client2, part2, AttributeNumber.Serial, true, true);
				factory.Save();

				helper.CreateWhsReceiveWithInventory(client1, data.Whs1, "R1" + i, part1, 2m);
				helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2" + i, part2, 2m);
				factory.Save();

				var order1 = helper.CreateWhsOrder(client1, data.Whs1, "O1" + i);
				var orderLine1 = helper.CreateWhsOrderLine(order1, part1, 2m);
				var order2 = helper.CreateWhsOrder(client2, data.Whs1, "O2" + i);
				var orderLine2 = helper.CreateWhsOrderLine(order2, part2, 2m);
				var pick = helper.CreatePickNew(order1, order2);
				factory.Save();

				orderLine1.ReleaseLines.AddNew("", "", "", "S1", ZDate.Empty, ZDate.Empty, 1m);
				orderLine1.ReleaseLines.AddNew("", "", "", "S2", ZDate.Empty, ZDate.Empty, 1m);
				orderLine2.ReleaseLines.AddNew("", "", "", "S3", ZDate.Empty, ZDate.Empty, 1m);
				orderLine2.ReleaseLines.AddNew("", "", "", "S4", ZDate.Empty, ZDate.Empty, 1m);
				factory.Save();

				pickLines.AddRange(pick.GetAllPickLines());
			}

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickLinesInNewFactory = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, pickLines.Select(pl => pl.PK)));
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				var scannedRCASerialNumbers = new List<ScannedRCASerialNumbersPerProductInfo>();
				RFAttributeHelper.AddScannedReleaseCapturedSerialNumbers(scannedRCASerialNumbers, pickLinesInNewFactory);
			}
		}

		#endregion
	}
}
