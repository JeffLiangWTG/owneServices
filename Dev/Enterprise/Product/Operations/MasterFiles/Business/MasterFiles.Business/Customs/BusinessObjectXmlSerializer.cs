using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// This serialise biz obj's values and its children business object collections into xml and deserialise back into the object
	/// </summary>
	public static class BusinessObjectXmlSerializer
	{
		public static string Serialize(this XmlSerializableNonPersistentBusinessObject serializableObject, string xmlNamespace = null)
		{
			var xmlRoot = serializableObject.GetType().GetAttribute<XmlRootAttribute>();
			var xmlName = xmlRoot != null ? xmlRoot.ElementName : "";
			var actualNamespace = xmlNamespace ?? (xmlRoot == null || xmlRoot.Namespace == null ? "" : xmlRoot.Namespace);
			return GetXElement(serializableObject, xmlName, actualNamespace).ToString();
		}

		public static string Serialize(this IEnumerable<XmlSerializableNonPersistentBusinessObject> serializableObjects, string xmlRootName, string xmlNamespace = null)
		{
			XNamespace actualNamespace = xmlNamespace ?? "";
			var xElements = from XmlSerializableNonPersistentBusinessObject serializableObject in serializableObjects
							select GetXElement(serializableObject, "", xmlNamespace);

			return new XElement(actualNamespace + xmlRootName, xElements).ToString();
		}

		public static void Deserialize(this XmlSerializableNonPersistentBusinessObject serializableObject, string xml, string xmlNamespace = null)
		{
			SetObjectProperties(serializableObject, xmlNamespace ?? "", XElement.Parse(xml));
		}

		public static IEnumerable<XmlSerializableNonPersistentBusinessObject> Deserialize(string xml, Func<XmlSerializableNonPersistentBusinessObject> createSerailizableObject, string xmlNamespace = null)
		{
			var topElement = XElement.Parse(xml);
			xmlNamespace = xmlNamespace ?? "";
			foreach (var element in topElement.Elements())
			{
				var serializableObject = createSerailizableObject();
				serializableObject.Deserialize(element.ToString(), xmlNamespace);
				yield return serializableObject;
			}
		}

		#region Implementation

		#region Serialization

		static XElement GetXElement(XmlSerializableNonPersistentBusinessObject serializableObject, string xmlName, XNamespace xNamespace)
		{
			var actualXmlName = string.IsNullOrWhiteSpace(xmlName) ? serializableObject.GetType().Name : xmlName;
			var element = new XElement(xNamespace + actualXmlName);
			element.Add(GetSerializableProperties(serializableObject.GetType()).Select(property => GetXElement(serializableObject, property, xNamespace)));
			return element;
		}

		static XElement GetXElement(XmlSerializableNonPersistentBusinessObject serializableObject, PropertyInfo property, XNamespace xNamespace)
		{
			XElement result = null;
			var value = property.GetValue(serializableObject, null);
			if (property.HasType<XmlSerializableNonPersistentBusinessObject>())
			{
				var xmlElement = property.GetAttribute<XmlElementAttribute>();
				var xmlName = xmlElement != null ? xmlElement.ElementName : property.Name;
				result = GetXElement((XmlSerializableNonPersistentBusinessObject)value, xmlName, xNamespace);
			}
			else if (property.HasType<IBusinessObjectCollection>())
			{
				var collection = (IBusinessObjectCollection)value;
				if (typeof(XmlSerializableNonPersistentBusinessObject).IsAssignableFrom(collection.TypeOfElements))
				{
					var xmlArray = property.GetAttribute<XmlArrayAttribute>();
					var arrayName = xmlArray != null ? xmlArray.ElementName : property.Name;
					var xmlArrayItem = property.GetAttribute<XmlArrayItemAttribute>();
					var arrayItemName = xmlArrayItem != null ? xmlArrayItem.ElementName : "";
					result = new XElement(
						xNamespace + arrayName,
						collection.Cast<XmlSerializableNonPersistentBusinessObject>()
								  .Select(item => GetXElement(item, arrayItemName, xNamespace)));
				}
			}
			else if (property.HasType<IZType>())
			{
				var xmlElement = property.GetAttribute<XmlElementAttribute>();
				var xmlName = xmlElement != null ? xmlElement.ElementName : property.Name;
				result = new XElement(xNamespace + xmlName, ConvertToString((IZType)value));
			}
			return result != null && !string.IsNullOrEmpty(result.Value) ? result : null;
		}

		static string ConvertToString(IZType value)
		{
			if (value != null && !value.IsEmpty)
			{
				var valueType = value.GetType();
				try
				{
					if (valueType == typeof(ZBlob))
					{
						return Convert.ToBase64String((ZBlob)value);
					}
					else if (valueType == typeof(ZDate))
					{
						return ((ZDate)value).ToString("yyyy-MM-dd");
					}
					else if (valueType == typeof(ZDateTime))
					{
						return ((ZDateTime)value).ToISO8601String();
					}
					else
					{
						var converter = GetTypeConverter(valueType);
						if (converter != null && converter.CanConvertTo(typeof(string)))
						{
							return (string)converter.ConvertTo(value, typeof(string));
						}
						else
						{
							ErrorReporter.ReportOnce(valueType.Name + " does not support converting to string");
						}
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("Failed to convert '" + valueType.Name + "' to string");
				}
			}
			return null;
		}

		#endregion // Serialization

		#region Deserialization

		static void SetObjectProperties(XmlSerializableNonPersistentBusinessObject serializableObject, XNamespace xmlNamespace, XElement element)
		{
			if (serializableObject != null && element != null)
			{
				XNamespace actualNamespace;
				if (xmlNamespace == null)
				{
					var xmlRoot = serializableObject.GetType().GetAttribute<XmlRootAttribute>();
					actualNamespace = xmlRoot == null || xmlRoot.Namespace == null ? "" : xmlRoot.Namespace;
				}
				else
				{
					actualNamespace = xmlNamespace;
				}
				foreach (var property in GetSerializableProperties(serializableObject.GetType()))
				{
					SetObjectProperty(serializableObject, xmlNamespace, property, element);
				}
			}
		}

		static void SetObjectProperty(XmlSerializableNonPersistentBusinessObject serializableObject, XNamespace xmlNamespace, PropertyInfo property, XElement element)
		{
			if (property.HasType<XmlSerializableNonPersistentBusinessObject>())
			{
				var xmlElement = property.GetAttribute<XmlElementAttribute>();
				var xmlName = xmlElement != null ? xmlElement.ElementName : property.Name;
				SetObjectProperties((XmlSerializableNonPersistentBusinessObject)property.GetValue(serializableObject, null), xmlNamespace, element.Element(xmlNamespace + xmlName));
			}
			else if (property.HasType<IBusinessObjectCollection>())
			{
				var collection = (IBusinessObjectCollection)property.GetValue(serializableObject, null);
				if (collection != null && typeof(XmlSerializableNonPersistentBusinessObject).IsAssignableFrom(collection.TypeOfElements))
				{
					while (collection.Count > 0)
					{
						collection.RemoveAt(0);
					}
					var xmlArray = property.GetAttribute<XmlArrayAttribute>();
					var arrayName = xmlArray != null ? xmlArray.ElementName : property.Name;
					var arrayElement = element.Element(xmlNamespace + arrayName);
					if (arrayElement != null)
					{
						var xmlArrayItem = property.GetAttribute<XmlArrayItemAttribute>();
						var arrayItemName = xmlArrayItem != null
							? xmlArrayItem.ElementName
							: collection.TypeOfElements.Name;
						foreach (var arrayItemElement in arrayElement.Elements(xmlNamespace + arrayItemName))
						{
							SetObjectProperties((XmlSerializableNonPersistentBusinessObject)collection.AddNew(), xmlNamespace, arrayItemElement);
						}
					}
				}
			}
			else if (property.HasType<IZType>())
			{
				var xmlElement = property.GetAttribute<XmlElementAttribute>();
				var xmlName = xmlElement != null ? xmlElement.ElementName : property.Name;
				var propertyElement = element.Element(xmlNamespace + xmlName);
				if (propertyElement != null)
				{
					var value = ConvertToIZType(propertyElement.Value, property.PropertyType);
					if (value != null)
					{
						GetBackField(property).SetValue(serializableObject, value);
					}
				}
			}
		}

		static IZType ConvertToIZType(string value, Type type)
		{
			try
			{
				if (type == typeof(ZBlob))
				{
					return new ZBlob(Convert.FromBase64String(value));
				}
				else
				{
					var converter = GetTypeConverter(type);
					if (converter != null && converter.CanConvertFrom(typeof(string)))
					{
						return (IZType)converter.ConvertFrom(value);
					}
					else
					{
						ErrorReporter.ReportOnce(type.Name + " does not support string parsing");
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Failed to convert '" + value + "' to the type " + type.Name);
			}
			return null;
		}

		internal static FieldInfo GetBackField(PropertyInfo property)
		{
			var fieldName = property.Name.Substring(0, 1).ToLower() + property.Name.Substring(1);
			return property.DeclaringType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		}

		#endregion // Deserialization

		#region Common

		static TypeConverter GetTypeConverter(Type type)
		{
			TypeConverter converter;
			var converters = BusinessObjectXmlSerializer.converters ?? (BusinessObjectXmlSerializer.converters = new Dictionary<Type, TypeConverter>());
			if (!converters.TryGetValue(type, out converter))
			{
				var converterType = type.Assembly.GetType(type.FullName + "TypeConverter", false);
				if (converterType != null)
				{
					converter = Activator.CreateInstance(converterType) as TypeConverter;
					converters[type] = converter;
				}
			}
			return converter;
		}
		static Dictionary<Type, TypeConverter> converters;

		static IEnumerable<PropertyInfo> GetSerializableProperties(Type type)
		{
			return GetSchemaProperties(type).Concat(
				   GetCompositeProperties(type)).
				   Where(property => !property.HasAttribute<XmlIgnoreAttribute>());
		}

		internal static IEnumerable<PropertyInfo> GetSchemaProperties(Type type)
		{
			var schema = type.GetNestedType((NoResString)"Schema", BindingFlags.Public);
			var properties = schema != null
				? schema.GetFields(BindingFlags.Public | BindingFlags.Static)
						.Where(schemaField => schemaField.FieldType == typeof(string))
						.Select(schemaField => type.GetProperty(schemaField.Name, BindingFlags.Public | BindingFlags.Instance))
						.Where(property => property.CanRead && property.CanWrite)
				: Enumerable.Empty<PropertyInfo>();
			return type.BaseType == null
				? properties
				: properties.Concat(GetSchemaProperties(type.BaseType));
		}

		internal static IEnumerable<PropertyInfo> GetCompositeProperties(Type type)
		{
			return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(PropertyIsComposite);
		}

		static bool PropertyIsComposite(PropertyInfo property)
		{
			return property.HasType<XmlSerializableNonPersistentBusinessObject>() ||
				   property.HasType<IBusinessObjectCollection>();
		}

		internal static TAttribute GetAttribute<TAttribute>(this MemberInfo info, bool inherit = false)
			where TAttribute : Attribute
		{
			return info.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() as TAttribute;
		}

		internal static bool HasAttribute<TAttribute>(this MemberInfo info, bool inherit = false)
		{
			return info.GetAttribute<XmlIgnoreAttribute>(inherit) != null;
		}

		internal static bool HasType<T>(this PropertyInfo property)
		{
			return typeof(T).IsAssignableFrom(property.PropertyType);
		}

		#endregion // Common

		#endregion // Implementation
	}
}
