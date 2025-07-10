using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentCustomFieldProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomFieldFromRegistry()
		{
			var shipment = Factory.New<ForwardingShipment>();

			shipment.DocsAndCartage.JP_CustomAttrib1 = "Custom 1";
			shipment.DocsAndCartage.JP_CustomAttrib2 = "Custom 2";

			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Registry1", "Hint"));
			FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Registry2", "Hint"));
			try
			{
				AssertEquals("((ICustomFieldProvider)shipment).GetCustomField(\"Custom1\")", "Custom 1", (shipment).GetCustomField("Registry1", AddOnColumnDataType.Codes.String));
				AssertEquals("((ICustomFieldProvider)shipment).GetCustomField(\"Custom2\")", "Custom 2", shipment.GetCustomField("Registry2", AddOnColumnDataType.Codes.String));
			}
			finally
			{
				FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint());
				FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint());
			}
		}

		public void TestShipmentCustomFields()
		{
			var shipment = Factory.New<ForwardingShipment>();

			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("String1", "String1 hint"));
			FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("String2", "String2 hint"));
			FreightDataRegistry.Instance.ShipmentCustomDate1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date1", "Date1 hint"));
			FreightDataRegistry.Instance.ShipmentCustomDate2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date2", "Date2 hint"));
			FreightDataRegistry.Instance.ShipmentCustomDecimalNo1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal1", "Decimal1 hint"));
			FreightDataRegistry.Instance.ShipmentCustomDecimalNo2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal2", "Decimal2 hint"));
			FreightDataRegistry.Instance.ShipmentCustomFlag1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Flag1", "Flag1 hint"));
			FreightDataRegistry.Instance.ShipmentCustomFlag2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Flag2", "Flag2 hint"));

			shipment.DocsAndCartage.JP_CustomAttrib1 = "test string1";
			shipment.DocsAndCartage.JP_CustomAttrib2 = "test string2";
			shipment.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2010, 08, 30);
			shipment.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2010, 08, 31);
			shipment.DocsAndCartage.JP_CustomDecimal1 = 123.45;
			shipment.DocsAndCartage.JP_CustomDecimal2 = 54.321;
			shipment.DocsAndCartage.JP_CustomFlag1 = true;
			shipment.DocsAndCartage.JP_CustomFlag2 = false;

			ICustomFieldProvider customFieldProvider = shipment;

			AssertEquals("Attrib1 value", (ZString)"test string1", customFieldProvider.GetCustomField("String1", null));
			AssertEquals("Attrib2 value", (ZString)"test string2", customFieldProvider.GetCustomField("String2", null));
			AssertEquals("Date1 value", new ZDateTime(2010, 08, 30), customFieldProvider.GetCustomField("Date1", null));
			AssertEquals("Date2 value", new ZDateTime(2010, 08, 31), customFieldProvider.GetCustomField("Date2", null));
			AssertEquals("Decimal1 value", (ZDecimal)123.45, customFieldProvider.GetCustomField("Decimal1", null));
			AssertEquals("Decimal2 value", (ZDecimal)54.321, customFieldProvider.GetCustomField("Decimal2", null));
			AssertEquals("Flag1 value", ZBool.True, customFieldProvider.GetCustomField("Flag1", null));
			AssertEquals("Flag2 value", ZBool.False, customFieldProvider.GetCustomField("Flag2", null));
		}

		public void TestCustomBusinessObjectIsReloadedOnActiveTemplateChange()
		{
			var airTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			airTemplate.P0_ProcessType = "SHP";
			airTemplate.P0_SubType1 = "AIR";

			var airColumn = airTemplate.GenCustomColumnDefinitions.AddNew();
			airColumn.XC_Name = "Air Field 1";
			airColumn.XC_Type = AddOnColumnDataType.Codes.String;

			var seaTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			seaTemplate.P0_ProcessType = "SHP";
			seaTemplate.P0_SubType1 = "SEA";

			var seaColumn = seaTemplate.GenCustomColumnDefinitions.AddNew();
			seaColumn.XC_Name = "Sea Field 1";
			seaColumn.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";

			var customBusinessObject = ((ICustomFieldProvider)shipment).GetCustomBusinessObject() as IDynamicBusinessObject;
			AssertContainsExactElementsInAnyOrder(new[] { "__AIR FIELD 1__prop__ZString", "__AIR FIELD 1__prop__ZStringInfo" }, customBusinessObject.PropertyNames);

			shipment.JS_TransportMode = "SEA";
			customBusinessObject = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();

			AssertContainsExactElementsInAnyOrder(new[] { "__SEA FIELD 1__prop__ZString", "__SEA FIELD 1__prop__ZStringInfo" }, customBusinessObject.PropertyNames);
		}
	}
}
