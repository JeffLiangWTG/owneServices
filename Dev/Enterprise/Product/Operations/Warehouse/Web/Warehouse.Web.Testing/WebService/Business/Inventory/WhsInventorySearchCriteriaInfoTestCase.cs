using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsInventorySearchCriteriaInfoTestCase : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var criteriaInfo = new WhsInventorySearchCriteriaInfo();
			AssertEquals(SearchJoinCondition.And, criteriaInfo.JoinCondition);
			AssertEquals("", criteriaInfo.ClientCode);
			AssertEquals(Guid.Empty, criteriaInfo.ProductPK);
			AssertEquals("", criteriaInfo.ProductCode);
			AssertEquals("", criteriaInfo.Location);
			AssertEquals("", criteriaInfo.PalletID);
			AssertEquals("", criteriaInfo.Attribute1);
			AssertEquals("", criteriaInfo.Attribute2);
			AssertEquals("", criteriaInfo.Attribute3);
			AssertEquals("", criteriaInfo.SerialNumber);
			AssertEquals(DateTime.MinValue, criteriaInfo.ExpiryDate);
			AssertEquals(DateTime.MinValue, criteriaInfo.PackingDate);
		}

		public void TestConstructor_WithParameters()
		{
			var now = ZDateTime.Now.ToDateTime();
			var criteriaInfo = new WhsInventorySearchCriteriaInfo("C1", "P1", "A-1", "PLT1", "A1", "A2", "A3", "SN1", now, now, WhsInventoryLevel.Level2, SearchJoinCondition.Or, SearchJoinCondition.Or);
			AssertEquals(SearchJoinCondition.Or, criteriaInfo.JoinCondition);
			AssertEquals("C1", criteriaInfo.ClientCode);
			AssertEquals(Guid.Empty, criteriaInfo.ProductPK);
			AssertEquals("P1", criteriaInfo.ProductCode);
			AssertEquals("A-1", criteriaInfo.Location);
			AssertEquals("PLT1", criteriaInfo.PalletID);
			AssertEquals("A1", criteriaInfo.Attribute1);
			AssertEquals("A2", criteriaInfo.Attribute2);
			AssertEquals("A3", criteriaInfo.Attribute3);
			AssertEquals("SN1", criteriaInfo.SerialNumber);
			AssertEquals(now, criteriaInfo.ExpiryDate);
			AssertEquals(now, criteriaInfo.PackingDate);
			AssertEquals(WhsInventoryLevel.Level2, criteriaInfo.DestinationInventoryLevel);
			AssertEquals(SearchJoinCondition.Or, criteriaInfo.PalletIDAndLocationJoinCondition);
		}

		#endregion

		#region TestUpdateInventoryLevel

		public void TestUpdateInventoryLevel_ProductNotExist()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventory = CreateInventoryViewForInventoryLevelTest(data);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, "notexist", "A-1", "PLT1");
			criteria.UpdateInventoryLevel(inventory);
			AssertEquals("Product not exist, should go to Level 1", WhsInventoryLevel.Level1, criteria.DestinationInventoryLevel);
		}

		public void TestUpdateInventoryLevel_CriteriaIsLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventory = CreateInventoryViewForInventoryLevelTest(data);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, "A-1", "A-1", "A-1");
			criteria.UpdateInventoryLevel(inventory);
			AssertEquals("Search criteria is Location, should go to Level 1", WhsInventoryLevel.Level1, criteria.DestinationInventoryLevel);
		}

		public void TestUpdateInventoryLevel_CriteriaIsPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventory = CreateInventoryViewForInventoryLevelTest(data);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, "PLT1", "A-1", "PLT1");
			criteria.UpdateInventoryLevel(inventory);
			AssertEquals("Search criteria is Pallet, should go to Level 1", WhsInventoryLevel.Level1, criteria.DestinationInventoryLevel);
		}

		public void TestUpdateInventoryLevel_CriteriaIsProduct_WithoutClientCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventory = CreateInventoryViewForInventoryLevelTest(data);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo("", data.Part1.OP_PartNum, "A-1", "PLT1");
			criteria.UpdateInventoryLevel(inventory);
			AssertEquals("Search criteria is Product without attributes and ClientCode is Empty, should go to Level 1", WhsInventoryLevel.Level1, criteria.DestinationInventoryLevel);
		}

		public void TestUpdateInventoryLevel_CriteriaIsProduct_HasClientCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventory = CreateInventoryViewForInventoryLevelTest(data);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "A-1", "PLT1");
			criteria.UpdateInventoryLevel(inventory);
			AssertEquals("Search criteria is Product without attributes and ClientCode is not Empty, should go to Level 1", WhsInventoryLevel.Level1, criteria.DestinationInventoryLevel);
		}

		public void TestUpdateInventoryLevel_CriteriaIsProduct_AttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventory = CreateInventoryViewForInventoryLevelTest(data, enableAttribute: true);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "A-1", "PLT1", destLevel: WhsInventoryLevel.Level2);
			criteria.UpdateInventoryLevel(inventory);
			AssertEquals("Search criteria is Product has attributes, should go to Level 2", WhsInventoryLevel.Level2, criteria.DestinationInventoryLevel);
		}

		public void TestUpdateInventoryLevel_CriteriaIsProduct_AttributeIsReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventory = CreateInventoryViewForInventoryLevelTest(data, enableAttribute: true, enableReleaseCaptured: true);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "A-1", "PLT1", destLevel: WhsInventoryLevel.Level2);
			criteria.UpdateInventoryLevel(inventory);
			AssertEquals("Search criteria is Product has attributes but is release captured, should go to Level 3", WhsInventoryLevel.Level3, criteria.DestinationInventoryLevel);
		}

		public void TestUpdateInventoryLevel_CriteriaIsProduct_HasAttribute_DestLevelIs3()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventory = CreateInventoryViewForInventoryLevelTest(data, enableAttribute: true);

			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "A-1", "PLT1", "", "", "", destLevel: WhsInventoryLevel.Level3);
			criteria.UpdateInventoryLevel(inventory);
			AssertEquals("Search criteria is Product has attribute(not serial number/release captured, but destination level 3, should go to level 3",
				WhsInventoryLevel.Level3, criteria.DestinationInventoryLevel);
		}

		WhsInventoryView CreateInventoryViewForInventoryLevelTest(TestDataSimpleEnvironment data, bool enableAttribute = false, bool enableReleaseCaptured = false, bool enableMultipleAttribute = false)
		{
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, enableAttribute, enableReleaseCaptured);

			if (enableMultipleAttribute)
			{
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			}

			var locationA1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			return Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT1");
		}

		public void TestUpdateInventoryLevel_CriteriaIsProduct_AttributeIsSerialNumber()
		{
			TestUpdateInventoryLevel_CriteriaIsProduct_AttributeIsSerialNumberCore(false);
		}

		public void TestUpdateInventoryLevel_CriteriaIsProduct_AttributeIsSerialNumber_ReleaseCaptured()
		{
			TestUpdateInventoryLevel_CriteriaIsProduct_AttributeIsSerialNumberCore(true);
		}

		void TestUpdateInventoryLevel_CriteriaIsProduct_AttributeIsSerialNumberCore(bool isReleaseCaptured)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, isReleaseCaptured);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT1");
			Factory.Save();

			var criteria = new WhsInventorySearchCriteriaInfo(data.Org1.OH_Code, data.Part1.OP_PartNum, "A-1", "PLT1", destLevel: WhsInventoryLevel.Level2);
			criteria.UpdateInventoryLevel(inventory);
			AssertEquals("Search criteria is Product has only 1 attribute but is serial number (release captured or not), should go to Level 3", WhsInventoryLevel.Level3, criteria.DestinationInventoryLevel);
		}

		#endregion

		#region TestIsAnyAttributesUsedButNotReleaseCaptured

		#region TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberUsed

		public void TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberUsed_ReleaseCaptured()
		{
			TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberUsedCore(false);
		}

		public void TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberUsed_NotReleaseCaptured()
		{
			TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberUsedCore(true);
		}

		public void TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberUsedCore(bool isReleaseCaptured)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, isReleaseCaptured);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			var flag = WhsInventorySearchCriteriaInfo.IsAnyAttributesUsedButNotSerialNumberOrReleaseCaptured(product, data.Org1);

			AssertEquals("If Serial Number is used, result should be false", false, flag);
		}

		#endregion

		#region TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberAndNonSerialNumberUsed

		public void TestIsAnyAttributesUsedButNotReleaseCaptured_SerialAndNonSerialAttribUsed_SerialNumberEnabled_SerialReleaseCapturedNonSerialAttribReleaseCaptured()
		{
			TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberAndNonSerialNumberUsed_SerialNumberEnabledCore(true, true, false);
		}

		public void TestIsAnyAttributesUsedButNotReleaseCaptured_SerialAndNonSerialAttribUsed_SerialNumberEnabled_SerialReleaseCapturedNonSerialAttribNotReleaseCaptured()
		{
			TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberAndNonSerialNumberUsed_SerialNumberEnabledCore(true, false, true);
		}

		public void TestIsAnyAttributesUsedButNotReleaseCaptured_SerialAndNonSerialAttribUsed_SerialNumberEnabled_SerialReleaseNotCapturedNonSerialAttribReleaseCaptured()
		{
			TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberAndNonSerialNumberUsed_SerialNumberEnabledCore(false, true, false);
		}

		public void TestIsAnyAttributesUsedButNotReleaseCaptured_SerialAndNonSerialAttribUsed_SerialNumberEnabled_SerialReleaseNotCapturedNonSerialAttribNotReleaseCaptured()
		{
			TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberAndNonSerialNumberUsed_SerialNumberEnabledCore(false, false, false);
		}

		void TestIsAnyAttributesUsedButNotReleaseCaptured_SerialNumberAndNonSerialNumberUsed_SerialNumberEnabledCore(bool isSerialNumberReleaseCaptured, bool isNonSerialNumberReleaseCaptured, bool expectedResult)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, isSerialNumberReleaseCaptured);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, isNonSerialNumberReleaseCaptured);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			var flag = WhsInventorySearchCriteriaInfo.IsAnyAttributesUsedButNotSerialNumberOrReleaseCaptured(product, data.Org1);

			AssertEquals(expectedResult, flag);
		}

		#endregion

		#endregion
	}
}
