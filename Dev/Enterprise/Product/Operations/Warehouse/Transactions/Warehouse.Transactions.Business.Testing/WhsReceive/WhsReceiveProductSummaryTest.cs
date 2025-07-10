using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveProductSummary))]
	public class WhsReceiveProductSummaryTest : WhsNonPersistentBusinessObjectTestCase
	{
		#region TestProperties_ProductCode

		public void TestProperties_ProductCode()
		{
			var summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ProductCode correct.", ZString.Empty, summary.ProductCode);

			summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, "ZGuid.BrettsGuid", ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ProductCode correct.", "ZGuid.BrettsGuid", summary.ProductCode);
		}

		#endregion

		#region TestProperties_ProductDescription

		public void TestProperties_ProductDescription()
		{
			var summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ProductDescription correct.", ZString.Empty, summary.ProductDescription);

			summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, "ZGuid.BrettsGuid", ZString.Empty, 0m, 0m);
			AssertEquals("Summary ProductDescription correct.", "ZGuid.BrettsGuid", summary.ProductDescription);
		}

		#endregion

		#region TestProperties_ReceiveCategory

		public void TestProperties_ReceiveCategory()
		{
			var summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ReceiveCategory correct.", ZString.Empty, summary.ReceiveCategory);

			summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, "ZGuid.BrettsGuid", 0m, 0m);
			AssertEquals("Summary ReceiveCategory correct.", "ZGuid.BrettsGuid", summary.ReceiveCategory);
		}

		#endregion

		#region TestProperties_ExpectedQuantity

		public void TestProperties_ExpectedQuantity()
		{
			var summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ExpectedQuantity correct.", 0m, summary.ExpectedQuantity);

			summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, "ZGuid.BrettsGuid", ZString.Empty, 10m, 0m);
			AssertEquals("Summary ExpectedQuantity correct.", 10m, summary.ExpectedQuantity);
		}

		#endregion

		#region TestProperties_ReceivedQuantity

		public void TestProperties_ReceivedQuantity()
		{
			var summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ReceivedQuantity correct.", 0m, summary.ReceivedQuantity);

			summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 23m);
			AssertEquals("Summary ReceivedQuantity correct.", 23m, summary.ReceivedQuantity);

			summary.ReceivedQuantity = 2.2m;
			AssertEquals("Summary ReceivedQuantity correct.", 2.2m, summary.ReceivedQuantity);
		}

		#endregion

		#region TestProperties_ClientPk

		public void TestProperties_ClientPk()
		{
			var summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ClientPk correct.", ZGuid.Empty, summary.ClientPk);

			var clientPk = ZGuid.NewZGuid();
			summary = new WhsReceiveProductSummary(Factory, clientPk, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 23m);
			AssertEquals("Summary ClientPk correct.", clientPk, summary.ClientPk);
		}

		#endregion

		#region TestProperties_WarehousePk

		public void TestProperties_WarehousePk()
		{
			var summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary WarehousePk correct.", ZGuid.Empty, summary.WarehousePK);

			var warehousePk = ZGuid.NewZGuid();
			summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, warehousePk, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 23m);
			AssertEquals("Summary WarehousePk correct.", warehousePk, summary.WarehousePK);
		}

		#endregion

		#region TestProperties_ProductPk

		public void TestProperties_ProductPk()
		{
			var summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ProductPk correct.", ZGuid.Empty, summary.ProductPk);

			var productPk = ZGuid.NewZGuid();
			summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, productPk, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 23m);
			AssertEquals("Summary ProductPk correct.", productPk, summary.ProductPk);
		}

		#endregion

		#region TestProperties_IsBlindReceive

		public void TestProperties_IsBlindReceive()
		{
			var summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ProductCode correct.", false, summary.IsBlindReceive);

			summary = new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true, "ZGuid.BrettsGuid", ZString.Empty, ZString.Empty, 0m, 0m);
			AssertEquals("Summary ProductCode correct.", true, summary.IsBlindReceive);
		}

		#endregion

		#region TestProperties_Product

		public void TestProperties_Product()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1", OrgPartRelation.RelationshipTypes.Both);

			var summary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, ZString.Empty, 0m, 0m);
			AssertEquals("Summary Product correct.", product.PK, summary.Product.PK);
		}

		#endregion

		#region TestProperties_Client

		public void TestProperties_Client()
		{
			var client = Helper.CreateClient("C1");
			var warehouse = Helper.CreateWarehouse("W1");
			var product = Helper.CreateProduct(client, "P1", OrgPartRelation.RelationshipTypes.Both);

			var summary = new WhsReceiveProductSummary(Factory, client.PK, warehouse.PK, product.PK, false, product.OP_PartNum, product.OP_Desc, ZString.Empty, 0m, 0m);
			AssertEquals("Summary Client correct.", client.PK, summary.Client.PK);
		}

		#endregion

		#region TestValidation

		public void TestValidation()
		{
			AssertType(typeof(WhsReceiveProductSummaryValidation), CreateReceiveProductSummary().Validation);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);
		}

		WhsReceiveProductSummary CreateReceiveProductSummary() => new WhsReceiveProductSummary(Factory, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false, ZString.Empty, ZString.Empty, ZString.Empty, 0m, 0m);

		#endregion
	}
}
