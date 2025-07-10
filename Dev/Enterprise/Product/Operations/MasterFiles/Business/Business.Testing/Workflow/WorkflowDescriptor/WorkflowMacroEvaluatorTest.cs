using System;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowMacroEvaluatorTest : TestCaseWithFactory
	{
		public void TestGetValue_CustomField() => CombineAssertions(() =>
		{
			var bizo = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipment = (Forwarding.IForwardingShipment)bizo;
			var org = Factory.New<OrgHeader>();
			org.SetUserDefinedValue("Custom Field 1", new ZString("Custom Value 1"));
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;
			var (businessObject, fieldName, fieldValue, isCustomField) = new WorkflowMacroEvaluator().GetValue(bizo, "Consignee.GetCustomField(Custom Field 1)", out _);
			AssertSame("businessObject", org, ((CustomBusinessObject)businessObject).Parent);
			AssertEquals("fieldName", "__CUSTOM FIELD 1__prop__ZString", fieldName);
			AssertEquals("fieldValue", "Custom Value 1", fieldValue);
			AssertEquals("isCustomField", true, isCustomField);
		});

		public void TestGetValue_Collection() => CombineAssertions(() =>
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			var item = dummy.Collection.AddNew();
			item.Z0_Bool = ZBool.False;
			var (businessObject, fieldName, fieldValue, isCustomField) = new WorkflowMacroEvaluator().GetValue(dummy, "Collection.First().Z0_Bool", out _);
			AssertSame("businessObject", item, businessObject);
			AssertEquals("fieldName", "Z0_Bool", fieldName);
			AssertEquals("fieldValue", ZBool.False, fieldValue);
			AssertEquals("isCustomField", false, isCustomField);
		});

		public void TestGetFinalPropertyInfoForCustomsFieldOfIBusinessObjectCollection_NoWarning()
		{
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			var consolType = consol.GetType();

			AssertGetPropertyInfo(consolType, "Shipments.GetCustomField(shipment1)", consol, null, "");
			AssertGetPropertyInfo(consolType, "Shipments.GetCustomField(shipment1)", consol, new ZString("123"), "");

			AssertGetPropertyInfo(consolType, "Shipments.GetCustomFieldWithType(shipment2, STR)", consol, null, "");
			AssertGetPropertyInfo(consolType, "Shipments.GetCustomFieldWithType(shipment2, STR)", consol, new ZInt("123"), "");
		}

		public void TestGetFinalPropertyInfoForCustomsField()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment.SetUserDefinedValue("shipment 1", ZBool.True);
			shipment.SetUserDefinedValue("shipment 2", new ZString("Lots Of Ice"));
			var shipmentType = shipment.GetType();

			AssertGetPropertyInfo(shipmentType, "GetCustomField(shipment 1)", shipment, null, "");
			AssertGetPropertyInfo(shipmentType, "GetCustomField(shipment XXX)", shipment, null, "Cannot find custom field 'shipment XXX' on Enterprise.Freight.Forwarding.Business.ForwardingShipment by Set Field trigger action.");
			AssertGetPropertyInfo(shipmentType, "GetCustomField(shipment 1)", shipment, new ZString("123"), "");

			AssertGetPropertyInfo(shipmentType, "GetCustomFieldWithType(shipment 2, STR)", shipment, null, "");
			AssertGetPropertyInfo(shipmentType, "GetCustomFieldWithType(shipment 2, STR)", shipment, new ZInt("123"), "");
			AssertGetPropertyInfo(shipmentType, "GetCustomFieldWithType(shipment 2, INT)", shipment, null, "Cannot find custom field 'shipment 2(INT)' on Enterprise.Freight.Forwarding.Business.ForwardingShipment by Set Field trigger action.");
			AssertGetPropertyInfo(shipmentType, "GetCustomFieldWithType(shipment 2, INT)", shipment, new ZInt(3), "Cannot find custom field 'shipment 2(INT)' on Enterprise.Freight.Forwarding.Business.ForwardingShipment to set value '3' by Set Field trigger action.");
			AssertGetPropertyInfo(shipmentType, "GetCustomFieldWithType(shipment XXX)", shipment, null, "Cannot find custom field 'shipment XXX' on Enterprise.Freight.Forwarding.Business.ForwardingShipment by Set Field trigger action.");
		}

		static void AssertGetPropertyInfo(Type componentType, string propertyPath, IBusiness bizo, IZType fieldValue, string expectedWarnings)
		{
			var notifications = new ProcessTaskNotificationValidation.ValidationNotifications();
			var macroEvaluator = new WorkflowMacroEvaluator(notifications);
			var propertyInfo = macroEvaluator.GetFinalPropertyInfo(componentType, propertyPath, bizo, fieldValue);

			AssertNull(propertyInfo);
			AssertEquals(expectedWarnings, notifications.ToString().Trim());
		}

		public void TestGetNextPropertyInfo_NotifyWarningIfMacroIgnore()
		{
			var notifications = new NotificationCollection();
			var propertyInfo = new WorkflowMacroEvaluator(notifications).GetNextPropertyInfo(typeof(HeaderForTest), "Line.Code", out _);
			CombineAssertions(() =>
			{
				AssertNotNull("Line", propertyInfo);
				AssertEquals("Message", @"Macro Ignore Property: 'Line' on 'Enterprise.MasterFiles.Business.Testing.WorkflowMacroEvaluatorTest+HeaderForTest'.", notifications.GetWarnings().GetFirstMessage());
			});
		}

		public void TestGetPropertyInfoNotNullWhenInterfaceInheritsInterfaceAndHasAttribute()
		{
			var result = WorkflowMacroEvaluator.GetPropertyInfo(typeof(IComplex), "Property1");
			AssertNotNull(result);
		}

		public void TestGetPropertyInfoNotNullWhenInterfaceInheritsInterfaceAndHasAttributeRecursive()
		{
			var result = WorkflowMacroEvaluator.GetPropertyInfo(typeof(IComplex), "Name");
			AssertNotNull(result);
		}

		public void TestGetPropertyInfoIsNullWhenInterfaceDoesNotInheritAttributeInterface()
		{
			var result = WorkflowMacroEvaluator.GetPropertyInfo(typeof(IIncorrectAttribute), "Unused");
			AssertNull(result);
		}

		public void TestGetPublicPropertyFromBusinessObject()
		{
			var parent = Factory.New<BizoForTest>();
			var child = Factory.New<BizoForTest>();
			child.PropertyOnInterface = "PropertyOnInterface";
			child.PropertyNotOnInterface = "PropertyNotOnInterface";
			parent.Child = child;

			var macroEvaluator = new WorkflowMacroEvaluator();
			var propertyInfo = macroEvaluator.GetFinalPropertyInfo(parent.GetType(), "Child.PropertyOnInterface", parent);
			AssertNotNull(propertyInfo);
			AssertEquals(child.PropertyOnInterface, propertyInfo.GetValue(child, null));

			propertyInfo = macroEvaluator.GetFinalPropertyInfo(parent.GetType(), "Child.PropertyNotOnInterface", parent);
			AssertNotNull(propertyInfo);
			AssertEquals(child.PropertyNotOnInterface, propertyInfo.GetValue(child, null));
		}

		public class HeaderForTest
		{
			[MacroIgnore]
			public LineForTest Line => line ?? (line = new LineForTest());
			LineForTest line;
		}

		public class LineForTest
		{
			public string Code => nameof(Code);
		}

		interface IBase
		{
			string Name { get; }
		}

		[FlattenPropertiesFromInheritanceForMacroEvaluation(nameof(IBase))]
		interface ISimple : IBase
		{
			string Property1 { get; }
		}

		[FlattenPropertiesFromInheritanceForMacroEvaluation(nameof(ISimple))]
		interface IComplex : ISimple
		{
			string Property2 { get; }
		}

		[CodeAlive("Used in testing FlattenPropertiesFromInheritanceForMacroEvaluation attribute of IIncorrectAttribute")]
		interface IUnused
		{
			string Unused { get; }
		}

		[FlattenPropertiesFromInheritanceForMacroEvaluation(nameof(IUnused))]
		interface IIncorrectAttribute { }

		public class BizoForTest : DummyWithWorkflow, IProperty
		{
			public BizoForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public IProperty Child { get; set; }

			public string PropertyOnInterface { get; set; }
			public string PropertyNotOnInterface { get; set; }
		}

		public interface IProperty
		{
			string PropertyOnInterface { get; }
		}
	}
}
