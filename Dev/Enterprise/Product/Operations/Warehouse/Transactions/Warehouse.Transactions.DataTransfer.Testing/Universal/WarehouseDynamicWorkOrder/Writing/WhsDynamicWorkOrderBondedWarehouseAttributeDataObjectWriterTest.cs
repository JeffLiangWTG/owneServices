using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsDynamicWorkOrderBondedWarehouseAttributeDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var dynamicWorkOrderLine = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 10m);

			var customsData = WhsBondedWarehouseAttributeDataObjectWriterTest.GetCustomsData(dynamicWorkOrderLine);
			AssertExceptionThrown<ArgumentNullException>(() => new WhsDynamicWorkOrderBondedWarehouseAttributeDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WDO, customsData)), null));
		}

		public void TestBasicCustomsLevelFieldMappings_ParentLine() => TestBasicCustomsLevelFieldMappings(false);
		public void TestBasicCustomsLevelFieldMappings_ComponentLine() => TestBasicCustomsLevelFieldMappings(true);

		void TestBasicCustomsLevelFieldMappings(bool isComponentLine)
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var dynamicWorkOrderLine = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 10m);
			var componentLine = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 5m);
			componentLine.WE_WE_ParentDocketLine = dynamicWorkOrderLine.PK;

			var customsParentDocketLine = isComponentLine ? componentLine : dynamicWorkOrderLine;
			var customsData = WhsBondedWarehouseAttributeDataObjectWriterTest.GetCustomsData(customsParentDocketLine);
			var customsDataObject = new WhsDynamicWorkOrderBondedWarehouseAttributeDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WDO, customsData)), customsParentDocketLine).GetDataObject(customsData);

			AssertNotNull("customsDataObject", customsDataObject);

			CombineAssertions(delegate
			{
				AssertContents(customsDataObject, isComponentLine);
			});
		}

		internal static void AssertContents(CustomsEntryInfo customsDataObject, bool isComponentLine)
		{
			AssertNull("customsDataObject.AdditionalInformation", customsDataObject.AdditionalInformation);
			AssertNull("customsDataObject.CountryOfOrigin", customsDataObject.CountryOfOrigin);
			AssertNull("customsDataObject.CustomsQuantity", customsDataObject.CustomsQuantity);
			AssertNull("customsDataObject.CustomsQuantityUnit", customsDataObject.CustomsQuantityUnit);
			AssertNull("customsDataObject.DeclarationReference", customsDataObject.DeclarationReference);
			AssertNull("customsDataObject.EntryDate", customsDataObject.EntryDate);
			AssertNull("customsDataObject.TILV", customsDataObject.TILV);
			AssertNull("customsDataObject.ValueForDuty", customsDataObject.ValueForDuty);
			AssertNull("customsDataObject.CustomsSecondQuantity", customsDataObject.CustomsSecondQuantity);
			AssertNull("customsDataObject.Tariff", customsDataObject.Tariff);
			AssertNull("customsDataObject.PrimaryPreference", customsDataObject.PrimaryPreference);
			AssertNull("customsDataObject.CustomsThirdQuantity", customsDataObject.CustomsThirdQuantity);
			AssertNull("customsDataObject.ManufacturerAddress", customsDataObject.ManufacturerAddress);
			AssertNull("customsDataObject.ZoneStatus", customsDataObject.ZoneStatus);
			AssertNull("customsDataObject.IsFromOtherFTZWarehouse", customsDataObject.IsFromOtherFTZWarehouse);
			AssertNull("customsDataObject.OutwardType", customsDataObject.OutwardType);
			AssertNull("customsDataObject.CustomsDeadline", customsDataObject.CustomsDeadline);
			AssertNull("customsDataObject.InwardStyle", customsDataObject.InwardStyle);
			AssertNull("customsDataObject.InwardProcedure", customsDataObject.InwardProcedure);
			AssertNull("customsDataObject.EntryKey", customsDataObject.EntryKey);
			AssertNull("customsDataObject.EntryLineNumber", customsDataObject.EntryLineNumber);

			if (isComponentLine)
			{
				AssertEquals("customsDataObject.InwardsEntryKey", "INWARDSKEY", customsDataObject.InwardsEntryKey);
				AssertEquals("customsDataObject.InwardsEntryLineNumber", new ZShort(1), customsDataObject.InwardsEntryLineNumber);
				AssertNull("customsDataObject.IsMainInwardsProcessedItem", customsDataObject.IsMainInwardsProcessedItem);
				AssertNull("customsDataObject.IsSecondaryInwardsProcessedItem", customsDataObject.IsSecondaryInwardsProcessedItem);
			}
			else
			{
				AssertNull("customsDataObject.InwardsEntryKey", customsDataObject.InwardsEntryKey);
				AssertNull("customsDataObject.InwardsEntryLineNumber", customsDataObject.InwardsEntryLineNumber);
				AssertEquals("customsDataObject.IsMainInwardsProcessedItem", true, customsDataObject.IsMainInwardsProcessedItem);
				AssertEquals("customsDataObject.IsSecondaryInwardsProcessedItem", true, customsDataObject.IsSecondaryInwardsProcessedItem);
			}
		}
	}
}
