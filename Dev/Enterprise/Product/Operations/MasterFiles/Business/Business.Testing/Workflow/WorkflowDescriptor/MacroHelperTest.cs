using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.MasterFiles.Business.Testing
{
	class MacroHelperTest : TestCaseWithFactory
	{
		public void TestGetCustomFieldNameAndType()
		{
			AssertGetCustomFieldNameAndType("Other(shipment 1)", "", "");
			AssertGetCustomFieldNameAndType("GetCustomField(shipment 1)", "shipment 1", "");
			AssertGetCustomFieldNameAndType("GetCustomFieldWithType(shipment 2, INT)", "shipment 2", "INT");
		}

		static void AssertGetCustomFieldNameAndType(ZString fieldPath, ZString expectedFieldName, ZString expectedFieldType)
		{
			var (customFieldName, customFieldType) = MacroHelper.GetCustomFieldNameAndType(fieldPath);
			AssertEquals(expectedFieldName, customFieldName);
			AssertEquals(expectedFieldType, customFieldType);
		}

		public void TestGetCustomProperty()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment.SetUserDefinedValue("shipment 1", ZBool.True);
			const string shipmentCustomBizoName = "Enterprise.Freight.Forwarding.Business.PhaseSecuritySupportableCustomBusinessObject";

			AssertGetCustomProperty("shipment 1", "", shipment, null, "__SHIPMENT 1__prop__ZBool", shipmentCustomBizoName);
			AssertGetCustomProperty("shipment 1", "", shipment, new ZString("123"), "__SHIPMENT 1__prop__ZBool", shipmentCustomBizoName);
			AssertGetCustomProperty("shipment XX", "", shipment, null, "", shipmentCustomBizoName);

			AssertGetCustomProperty("shipment 1", "BOO", shipment, null, "__SHIPMENT 1__prop__ZBool", shipmentCustomBizoName);
			AssertGetCustomProperty("shipment 1", "BOO", shipment, new ZString("123"), "__SHIPMENT 1__prop__ZBool", shipmentCustomBizoName);
			AssertGetCustomProperty("shipment 1", "INT", shipment, null, "", shipmentCustomBizoName);
			AssertGetCustomProperty("shipment 1", "INT", shipment, new ZString("123"), "", shipmentCustomBizoName);
		}

		static void AssertGetCustomProperty(string customFieldName, string customFieldType, BusinessObject businessObject, IZType value, string expectedCustomPropertyIdentifier, string shipmentCustomBizoName)
		{
			var (customProperty, customBizo) = MacroHelper.GetCustomProperty(customFieldName, customFieldType, businessObject, value);

			if (!string.IsNullOrEmpty(expectedCustomPropertyIdentifier))
			{
				AssertEquals(customProperty.Identifier, expectedCustomPropertyIdentifier);
			}
			else
			{
				AssertNull(customProperty);
			}

			AssertEquals(shipmentCustomBizoName, customBizo.GetType().FullName);
		}
	}
}
