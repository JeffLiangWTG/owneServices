using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(XmlSerializableNonPersistentBusinessObject))]
	public abstract class XmlSerializableNonPersistentBusinessObjectTest<TBusinessObject> : NonPersistentBusinessObjectTestCase
			where TBusinessObject : XmlSerializableNonPersistentBusinessObject
	{
		public virtual void TestSchemaPropertiesHaveFields()
		{
			Assert("This is not an emty test if there is no schema", true);
			var businessObject = GetNewBusinessObject();
			foreach (var property in BusinessObjectXmlSerializer.GetSchemaProperties(businessObject.GetType()))
			{
				var field = BusinessObjectXmlSerializer.GetBackField(property);
				AssertNotNull("Field exists", field);
				var value = GetValueForTest(field.FieldType);
				field.SetValue(businessObject, value);
				AssertEquals("This field corresponds to the property", value, property.GetValue(businessObject, null));
			}
		}

		public void TestCompositePropertiesNotNull()
		{
			Assert("This is not an emty test if there are no composite properties", true);
			var businessObject = GetNewBusinessObject();
			foreach (var property in BusinessObjectXmlSerializer.GetCompositeProperties(businessObject.GetType()))
			{
				AssertNotNull("Composite properties must be created by object itself", property.GetValue(businessObject, null));
			}
		}

		IZType GetValueForTest(Type type)
		{
			if (type == typeof(ZBlob))
			{
				return ZBlob.FromUTF8("CargoWise");
			}
			else if (type == typeof(ZBool))
			{
				return new ZBool(true);
			}
			else if (type == typeof(ZByte))
			{
				return new ZShort(42);
			}
			else if (type == typeof(ZDate))
			{
				return ZDate.BrettsBirthday;
			}
			else if (type == typeof(ZDateTime))
			{
				return ZDateTime.Now;
			}
			else if (type == typeof(ZDecimal))
			{
				return new ZDecimal(954.784m);
			}
			else if (type == typeof(ZGuid))
			{
				return new ZGuid("166427D1-62E9-4657-975A-54F8F29B3C12");
			}
			else if (type == typeof(ZInt))
			{
				return new ZInt(459);
			}
			else if (type == typeof(ZShort))
			{
				return new ZShort(148);
			}
			else if (type == typeof(ZString))
			{
				return new ZString("VALUE FOR TEST");
			}
			else
			{
				Assert("Unknown Type", false);
				return null;
			}
		}
	}
}
