using System.ComponentModel;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ImportedObject))]
	abstract class ImportedObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			ImportedObject importedObject = (ImportedObject)GetNewBusinessObject();
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(importedObject))
			{
				if (property.PropertyType == typeof(ImportedProperty) && !property.Name.StartsWith("AmendedProperty"))
				{
					ImportedProperty importedProperty = (ImportedProperty)property.GetValue(importedObject);
					if (importedProperty != null)
					{
						AssertNotEquals("", importedProperty.Name);
						ZString caption = importedProperty.Caption; // expect no exception
						if (!typeof(ZGuid).IsAssignableFrom(importedProperty.Value.GetType()) && !typeof(ZString).IsAssignableFrom(importedProperty.Value.GetType()) &&
							!importedProperty.Name.StartsWith("E_") && !importedProperty.Name.StartsWith("A_"))
						{
							AssertEquals("Property returning ImportedProperty should have the same name as the imported property", importedProperty.Name, property.Name);
						}
					}
				}
			}
		}

		public void TestGetAllImportedProperties()
		{
			ImportedObject importedObject = (ImportedObject)GetNewBusinessObject();
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(importedObject))
			{
				if (typeof(ImportedProperty).IsAssignableFrom(property.PropertyType) && !property.Name.StartsWith("AmendedProperty"))
				{
					ImportedProperty importedProperty = (ImportedProperty)property.GetValue(importedObject);
					AssertEquals("Property " + property.Name + " is defined on the class, but isn't returned from GetAllImportedProperties()", true, Contains(importedObject.GetAllImportedProperties(), importedProperty.Name));
				}
			}
		}

		protected void TestAmendedProperty(ZPropertyInfo property, int expectedAmendedPropertyIndex)
		{
			TestAmendedProperty(property, property.Name, expectedAmendedPropertyIndex);
		}

		protected void TestAmendedProperty(ZPropertyInfo property, ZString propertyName, int expectedAmendedPropertyIndex)
		{
			string amendedPropertyPropertyName = "AmendedProperty" + expectedAmendedPropertyIndex;

			SetHasChanges(property, false);
			ImportedProperty amendedProperty = GetAmendedImportedProperty(amendedPropertyPropertyName);
			AssertEquals("Property " + amendedPropertyPropertyName + " null initially", null, amendedProperty);

			SetHasChanges(property, true);
			amendedProperty = GetAmendedImportedProperty(amendedPropertyPropertyName);
			AssertNotNull("Expected property " + amendedPropertyPropertyName + " to have a value", amendedProperty);
			AssertEquals("AmendedProperty when a property value has changed", propertyName, amendedProperty.Name);
		}

		protected ImportedProperty GetAmendedImportedProperty(string amendedPropertyPropertyName)
		{
			ImportedObject importedObject = (ImportedObject)GetNewBusinessObject();
			PropertyInfo amendedPropertyProperty = importedObject.GetType().GetProperty(amendedPropertyPropertyName);
			return (ImportedProperty)amendedPropertyProperty.GetValue(importedObject, null);
		}

		bool Contains(ImportedProperty[] list, string propertyName)
		{
			foreach (ImportedProperty property in list)
			{
				if (property.Name == propertyName)
				{
					return true;
				}
			}
			return false;
		}

		void SetHasChanges(ZPropertyInfo property, bool value)
		{
			((IBusinessObjectInternals)property.BizObj).IsCopying = true;
			try
			{
				if (value)
				{
					if (property.PropertyType == typeof(ZDecimal))
					{
						property.Value = (ZDecimal)5m;
					}
					else if (property.PropertyType == typeof(ZDateTime))
					{
						property.Value = ZDateTime.Now;
					}
					else if (property.PropertyType == typeof(ZDateTimeOffset))
					{
						property.Value = ZDateTimeOffset.Now;
					}
					else if (property.PropertyType == typeof(ZString))
					{
						property.Value = (ZString)"x";
					}
					else if (property.PropertyType == typeof(ZGuid))
					{
						property.Value = ZGuid.NewZGuid();
					}
				}
				else
				{
					property.Value = property.OriginalValue;
				}
			}
			finally
			{
				((IBusinessObjectInternals)property.BizObj).IsCopying = false;
			}
			AssertEquals("HasChanges set to " + value + " correctly for the test", value, property.HasChanges);
		}
	}
}
