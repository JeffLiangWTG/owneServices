using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsInventoryHeldCodeInfo))]
	class WhsInventoryHeldCodeInfoTest : DataObjectInfoTestCase<WhsInventoryHeldCodeInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var heldCode = new WhsInventoryHeldCodeInfo();
			AssertConstructor(heldCode);
		}

		void AssertConstructor(WhsInventoryHeldCodeInfo heldCode)
		{
			AssertNotNull(heldCode);
			AssertEquals("", heldCode.Code);
			AssertEquals("", heldCode.Description);
			AssertEquals("", heldCode.CodeAndDescription);
			AssertEquals("", heldCode.ClientCode);
		}

		#endregion

		#region TestConstructor_WithWhsInventoryHeldCode

		public void TestConstructor_WithWhsInventoryHeldCode()
		{
			var client = Helper.CreateClient("C1");
			var heldCode = Helper.CreateInventoryHeldCode("AAA", "AAA for client 1", client.PK);
			Factory.Save();

			var heldCodeInfo = new WhsInventoryHeldCodeInfo(heldCode);
			AssertWhsInventoryHeldCodeConstructor(heldCodeInfo, heldCode);

			var heldCode2 = Helper.CreateInventoryHeldCode("", "", client.PK);

			var heldCodeInfo2 = new WhsInventoryHeldCodeInfo(heldCode2);
			AssertWhsInventoryHeldCodeConstructor(heldCodeInfo2, heldCode2);
		}

		#region AssertWhsInventoryHeldCodeConstructor

		void AssertWhsInventoryHeldCodeConstructor(WhsInventoryHeldCodeInfo heldCodeInfo, WhsInventoryHeldCode heldCode)
		{
			AssertEquals(heldCodeInfo.Code, heldCode.WHC_Code);
			AssertEquals(heldCodeInfo.Description, heldCode.WHC_DescriptionMultilingual);
			AssertEquals(heldCodeInfo.CodeAndDescription, string.IsNullOrEmpty(heldCode.WHC_Code) ? string.Empty : heldCode.WHC_Code + " - " + heldCode.WHC_DescriptionMultilingual);
			AssertEquals(heldCodeInfo.ClientCode, heldCode.Client?.OH_Code ?? "");
		}

		#endregion

		#endregion

		#region Properties

		public void TestCode()
		{
			AssertEquals("", Parent.Code);

			Parent.Code = "AAA";
			AssertEquals("AAA", Parent.Code);

			Parent.Code = "BBB";
			AssertEquals("BBB", Parent.Code);
		}

		public void TestDescription()
		{
			AssertEquals("", Parent.Description);

			Parent.Description = "AAA for system";
			AssertEquals("AAA for system", Parent.Description);

			Parent.Description = "BBB for client 1";
			AssertEquals("BBB for client 1", Parent.Description);
		}

		public void TestCodeAndDescription()
		{
			AssertEquals("", Parent.CodeAndDescription);

			Parent.CodeAndDescription = "ABC - AAA for system";
			AssertEquals("ABC - AAA for system", Parent.CodeAndDescription);
		}

		public void TestClientCode()
		{
			AssertEquals("", Parent.ClientCode);

			Parent.ClientCode = "Client1";
			AssertEquals("Client1", Parent.ClientCode);

			Parent.ClientCode = "Client2";
			AssertEquals("Client2", Parent.ClientCode);
		}

		#endregion

		#region Implementation

		protected new WhsInventoryHeldCodeInfo Parent
		{
			get { return (WhsInventoryHeldCodeInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsInventoryHeldCodeInfo();
		}

		#endregion
	}
}
