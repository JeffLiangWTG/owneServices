using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Xsd
{
	/// <summary>
	/// This generate Xsd from biz obj
	/// </summary>
	public class BusinessObjectXsdGenerator
	{
		Dictionary<string, List<string>> trailingTypes;
		Dictionary<Type, List<string>> bizObjTypes;
		Dictionary<Type, string> typeToElementNameMapping;

		void Initialize()
		{
			trailingTypes = new Dictionary<string, List<string>>();
			bizObjTypes = new Dictionary<Type, List<string>>();
			typeToElementNameMapping = new Dictionary<Type, string>();
		}

		public string GenerateXsd(XmlSerializableNonPersistentBusinessObject serializableObject, int version, string xmlNamespace = null)
		{
			Initialize();
			var type = serializableObject.GetType();
			var xmlRoot = type.GetAttribute<XmlRootAttribute>();
			var actualNamespace = xmlNamespace ?? (xmlRoot != null ? xmlRoot.Namespace : null);
			var xmlName = xmlRoot != null ? xmlRoot.ElementName : "";
			var actualXmlName = string.IsNullOrWhiteSpace(xmlName) ? type.Name : xmlName;

			return GenerateXsd((XsdBuilder result) =>
				{
					WriteTopLevelElement(result, actualXmlName, GetTypeFullname(type));
					GenerateBizObjXsd(serializableObject);
				}, version, xmlNamespace);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codesniffer got a false positive")]
		public string GenerateXsd(IBusinessObjectCollection serializableObjects, string xmlRootName, int version, string xmlNamespace = null)
		{
			Initialize();
			var serializableObject = (serializableObjects.Count > 0 ? (XmlSerializableNonPersistentBusinessObject)serializableObjects[0] : null)
				?? throw new InvalidOperationException(string.Format("{0} collection needs to have at least 1 element to be able to generate the Xsd properly", serializableObjects.GetType().FullName));

			var collectionType = serializableObjects.GetType();
			var actualXmlName = string.IsNullOrWhiteSpace(xmlRootName) ? collectionType.Name : xmlRootName;
			var collectionName = GetTypeFullname(collectionType);
			var collectionItemType = serializableObject.GetType();
			return GenerateXsd((XsdBuilder result) =>
				{
					WriteTopLevelElement(result, actualXmlName, collectionName);
					result.Add((NoResString)@"<xs:complexType name=""{0}"">", collectionName);
					result.Add((NoResString)@"<xs:all>");
					result.Add(@"<xs:element name=""{0}"" type=""{1}"" />", collectionItemType.Name, GetTypeFullname(collectionItemType));
					result.Add(@"</xs:all>");
					result.Add(@"</xs:complexType>");

					GenerateBizObjXsd(serializableObject);
				}, version, xmlNamespace);
		}

		#region Implementation

		string GenerateXsd(Action<XsdBuilder> generate, int version, string actualNamespace)
		{
			var result = CreateXsdBuilderWithLazyLoadingSchema();
			WriteSchemaOpeningTag(result, actualNamespace, version);

			generate(result);
			foreach (var bizObjectType in bizObjTypes.OrderBy(o => o.Key.FullName))
			{
				foreach (var line in bizObjectType.Value)
				{
					result.Add(line);
				}
			}

			foreach (var trailingSimpleType in trailingTypes.OrderBy(o => o.Key))
			{
				foreach (var line in trailingSimpleType.Value)
				{
					result.Add(line);
				}
			}

			WriteSchemaClosingTag(result);
			return result.WriteOutXsd();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codesniffer got a false positive")]
		XsdBuilder CreateXsdBuilderWithLazyLoadingSchema()
		{
			var builder = new XsdBuilder();
			builder.Add(@"<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">");
			builder.Add(@"<xs:element name=""placeholder"">");
			builder.Add(@"<xs:complexType>");
			builder.Add(@"<xs:simpleContent>");
			builder.Add(@"<xs:extension base=""xs:string"">");
			builder.Add(@"<xs:attribute name=""LazyLoading"" type=""xs:string"" fixed=""Yes"">");
			builder.Add(@"</xs:attribute>");
			builder.Add(@"</xs:extension>");
			builder.Add(@"</xs:simpleContent>");
			builder.Add(@"</xs:complexType>");
			builder.Add(@"</xs:element>");
			builder.Add(@"</xs:schema>");

			return builder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codesniffer got a false positive")]
		void WriteTopLevelElement(XsdBuilder result, string rootElementName, string rootElementType)
		{
			result.Add(@"<xs:element name=""{0}"" type=""{1}"" />", rootElementName, rootElementType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codesniffer got a false positive")]
		void WriteSchemaOpeningTag(XsdBuilder result, string xmlNamespace, int version)
		{
			result.Add(@"<xs:schema targetNamespace=""{0}"" version=""{1}"" elementFormDefault=""qualified"" xmlns=""{0}"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">", xmlNamespace, version);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codesniffer got a false positive")]
		void WriteSchemaClosingTag(XsdBuilder result)
		{
			result.Add(@"</xs:schema>");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codesniffer got a false positive, ditionary key")]
		string GetXMLType(PropertyInfo propertyInfo)
		{
			var typeOfDataInField = propertyInfo.PropertyType;
			if (typeOfDataInField == typeof(ZString))
			{
				var maxLength = GetMaxLength(propertyInfo);
				string stringTypeName = "string_maxLength" + maxLength.ToString();

				if (!trailingTypes.ContainsKey(stringTypeName))
				{
					var simpleTypeDefinition = new List<string>();
					simpleTypeDefinition.Add(string.Format(@"<xs:simpleType name=""{0}"">", stringTypeName));
					simpleTypeDefinition.Add(@"<xs:restriction base=""xs:string"">");
					simpleTypeDefinition.Add(string.Format(@"<xs:maxLength value=""{0}"" />", maxLength));
					simpleTypeDefinition.Add(@"</xs:restriction>");
					simpleTypeDefinition.Add(@"</xs:simpleType>");
					trailingTypes.Add(stringTypeName, simpleTypeDefinition);
				}

				return stringTypeName;
			}
			else if (typeOfDataInField == typeof(ZGuid))
			{
				string guidTypeName = "guid";
				if (!trailingTypes.ContainsKey(guidTypeName))
				{
					var simpleTypeDefinition = new List<string>();
					simpleTypeDefinition.Add(@"<xs:simpleType name=""guid"">");
					simpleTypeDefinition.Add(@"<xs:restriction base=""xs:string"">");
					simpleTypeDefinition.Add(@"<xs:pattern value=""[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}"" />");
					simpleTypeDefinition.Add(@"</xs:restriction>");
					simpleTypeDefinition.Add(@"</xs:simpleType>");
					trailingTypes.Add(guidTypeName, simpleTypeDefinition);
				}

				return guidTypeName;
			}
			else if (typeOfDataInField == typeof(ZDecimal))
			{
				var decimalPlaces = GetDecimalPlaces(propertyInfo);
				var decimalPrecision = GetDecimalPrecision(propertyInfo);
				string decimalTypeName = string.Format("decimal_{0}_{1}", decimalPrecision, decimalPlaces);

				if (!trailingTypes.ContainsKey(decimalTypeName))
				{
					var simpleTypeDefinition = new List<string>();
					simpleTypeDefinition.Add(string.Format(@"<xs:simpleType name=""{0}"">", decimalTypeName));
					simpleTypeDefinition.Add(@"<xs:restriction base=""xs:decimal"">");
					simpleTypeDefinition.Add(string.Format(@"<xs:totalDigits value =""{0}"" />", decimalPrecision));
					simpleTypeDefinition.Add(string.Format(@"<xs:fractionDigits value=""{0}"" />", decimalPlaces));
					simpleTypeDefinition.Add(@"</xs:restriction>");
					simpleTypeDefinition.Add(@"</xs:simpleType>");
					trailingTypes.Add(decimalTypeName, simpleTypeDefinition);
				}

				return decimalTypeName;
			}
			else if (typeOfDataInField == typeof(ZDateTime))
			{
				return "xs:dateTime";
			}
			else if (typeOfDataInField == typeof(ZDate))
			{
				return "xs:date";
			}
			else if (typeOfDataInField == typeof(ZInt))
			{
				return "xs:int";
			}
			else if (typeOfDataInField == typeof(ZBool))
			{
				AddBooleanString();
				return "booleanString";
			}
			else if (typeOfDataInField == typeof(ZShort))
			{
				return "xs:short";
			}
			else if (typeOfDataInField == typeof(ZByte))
			{
				return "xs:byte";
			}
			else if (typeOfDataInField == typeof(ZBlob))
			{
				return "xs:base64Binary";
			}

			throw new InvalidOperationException("Type [" + typeOfDataInField.FullName + "] not handled by XSD Schema Generator.");
		}

		int GetMaxLength(PropertyInfo propertyInfo)
		{
			var maxLengthAttributes = propertyInfo.GetCustomAttributes(typeof(MaxLengthAttribute), false);
			if (maxLengthAttributes.Length == 0)
			{
				throw new InvalidOperationException("All ZString properties on must have the MaxLengthAttribute applied.");
			}

			return (maxLengthAttributes[0] as MaxLengthAttribute).MaxLength;
		}

		int GetDecimalPlaces(PropertyInfo propertyInfo)
		{
			var decimalPlacesAttributes = propertyInfo.GetCustomAttributes(typeof(DecimalPlacesAttribute), false);
			if (decimalPlacesAttributes.Length == 0)
			{
				throw new InvalidOperationException("All ZDecimal properties on must have the DecimalPlacesAttribute applied.");
			}

			return (decimalPlacesAttributes[0] as DecimalPlacesAttribute).DecimalPlaces;
		}

		int GetDecimalPrecision(PropertyInfo propertyInfo)
		{
			var decimalPrecisionAttributes = propertyInfo.GetCustomAttributes(typeof(DecimalPrecisionAttribute), false);
			if (decimalPrecisionAttributes.Length == 0)
			{
				throw new InvalidOperationException("All ZDecimal properties on must have the DecimalPrecisionAttribute applied.");
			}

			return (decimalPrecisionAttributes[0] as DecimalPrecisionAttribute).DecimalPrecision;
		}

		string GetTypeFullname(Type type)
		{
			string result;
			if (!typeToElementNameMapping.TryGetValue(type, out result))
			{
				result = type.Name + typeToElementNameMapping.Count;
				typeToElementNameMapping.Add(type, result);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codesniffer got a false positive, Non Human Readable XML Generation Key")]
		void GenerateBizObjXsd(XmlSerializableNonPersistentBusinessObject serializableObject)
		{
			var bizObjType = serializableObject.GetType();
			if (!bizObjTypes.ContainsKey(bizObjType))
			{
				var bizObjXmlBuilder = new List<string>();
				bizObjTypes.Add(bizObjType, bizObjXmlBuilder);
				bizObjXmlBuilder.Add(string.Format(@"<xs:complexType name=""{0}"">", GetTypeFullname(bizObjType)));
				bizObjXmlBuilder.Add(@"<xs:all>");
				foreach (var property in GetSerializableSchemaProperties(bizObjType))
				{
					var xmlElement = property.GetAttribute<XmlElementAttribute>();
					var propertyXmlName = xmlElement != null ? xmlElement.ElementName : property.Name;
					bizObjXmlBuilder.Add(string.Format(@"<xs:element name=""{0}"" minOccurs=""0"" type=""{1}"" />", propertyXmlName, GetXMLType(property)));
				}
				foreach (var property in GetCompositeProperties(bizObjType))
				{
					var value = property.GetValue(serializableObject, null);
					if (property.HasType<XmlSerializableNonPersistentBusinessObject>())
					{
						if (value == null && !bizObjTypes.ContainsKey(property.PropertyType))
						{
							throw new InvalidOperationException(string.Format("{0}.{1} property must not be null in order to generate the Xsd properly", bizObjType.FullName, property.Name));
						}
						var xmlElement = property.GetAttribute<XmlElementAttribute>();
						var propertyXmlName = xmlElement != null ? xmlElement.ElementName : property.Name;
						var valueType = value == null ? null : value.GetType();
						bizObjXmlBuilder.Add(string.Format(@"<xs:element name=""{0}"" minOccurs=""0"" type=""{1}"" />", propertyXmlName, GetTypeFullname(valueType ?? property.PropertyType)));
						GenerateBizObjXsd((XmlSerializableNonPersistentBusinessObject)value);
					}
					else if (property.HasType<IBusinessObjectCollection>())
					{
						var collection = (IBusinessObjectCollection)value;
						if (collection == null)
						{
							throw new InvalidOperationException(string.Format("{0}.{1} collection must not be null in order to generate the Xsd properly", bizObjType.FullName, property.Name));
						}
						else if (collection.Count == 0)
						{
							var collectionItemType = collection.AddNew().GetType();
							if (typeof(XmlSerializableNonPersistentBusinessObject).IsAssignableFrom(collectionItemType) && !bizObjTypes.ContainsKey(collectionItemType))
							{
								throw new InvalidOperationException(string.Format("{0}.{1} collection needs to have at least 1 element to be able to generate the Xsd properly", bizObjType.FullName, property.Name));
							}
						}
						else
						{
							var collectionItem = collection[0];
							var valueType = value == null ? null : value.GetType();
							var collectionItemType = collectionItem.GetType();
							if (typeof(XmlSerializableNonPersistentBusinessObject).IsAssignableFrom(collectionItemType))
							{
								var xmlArray = property.GetAttribute<XmlArrayAttribute>();
								var arrayName = xmlArray != null ? xmlArray.ElementName : property.Name;
								var xmlArrayItem = property.GetAttribute<XmlArrayItemAttribute>();
								var arrayItemName = xmlArrayItem == null ? collectionItemType.Name : xmlArrayItem.ElementName;
								bizObjXmlBuilder.Add(string.Format(@"<xs:element name=""{0}"" minOccurs=""0"">", arrayName));
								bizObjXmlBuilder.Add("<xs:complexType>");
								bizObjXmlBuilder.Add("<xs:sequence>");
								bizObjXmlBuilder.Add(string.Format(@"<xs:element name=""{0}"" minOccurs=""0"" maxOccurs=""unbounded"" type=""{1}"" />", arrayItemName, GetTypeFullname(collectionItemType)));
								bizObjXmlBuilder.Add("</xs:sequence>");
								bizObjXmlBuilder.Add("</xs:complexType>");
								bizObjXmlBuilder.Add("</xs:element>");
								GenerateBizObjXsd((XmlSerializableNonPersistentBusinessObject)collectionItem);
							}
						}
					}
				}
				bizObjXmlBuilder.Add(@"</xs:all>");
				bizObjXmlBuilder.Add(@"</xs:complexType>");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codesniffer got a false positive")]
		void AddBooleanString()
		{
			if (!trailingTypes.ContainsKey("booleanString"))
			{
				var booleanString = new List<string>();
				booleanString.Add(@"<xs:simpleType name=""booleanString"">");
				booleanString.Add(@"<xs:restriction base=""xs:string"">");
				booleanString.Add(@"<xs:length value=""1""/>");
				booleanString.Add(@"<xs:pattern value=""[YN]"" />");
				booleanString.Add(@"</xs:restriction>");
				booleanString.Add(@"</xs:simpleType>");
				trailingTypes.Add("empty_string", booleanString);
			}
		}

		IEnumerable<PropertyInfo> GetSerializableSchemaProperties(Type type)
		{
			return GetSchemaProperties(type).
					Where(property => !property.HasAttribute<XmlIgnoreAttribute>());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		IEnumerable<PropertyInfo> GetSchemaProperties(Type type)
		{
			var schema = type.GetNestedType("Schema", BindingFlags.Public);
			var properties = schema != null
				? schema.GetFields(BindingFlags.Public | BindingFlags.Static)
						.Where(schemaField => schemaField.FieldType == typeof(string))
						.Select(schemaField => type.GetProperty(schemaField.Name, BindingFlags.Public | BindingFlags.Instance))
						.Where(property => property.CanRead && property.CanWrite && property.HasType<IZType>())
				: Enumerable.Empty<PropertyInfo>();
			return (type.BaseType == null
				? properties
				: properties.Concat(GetSchemaProperties(type.BaseType))).OrderBy(schemaField => schemaField.Name);
		}

		IEnumerable<PropertyInfo> GetCompositeProperties(Type type)
		{
			return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).
				Where(PropertyIsComposite).
				OrderBy(schemaField => schemaField.Name);
		}

		bool PropertyIsComposite(PropertyInfo property)
		{
			return (property.HasType<XmlSerializableNonPersistentBusinessObject>() ||
					property.HasType<IBusinessObjectCollection>()) && !property.HasAttribute<XmlIgnoreAttribute>();
		}

		#endregion // Implementation
	}
}
