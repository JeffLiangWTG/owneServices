using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	public abstract class MessageBuilderTest : TestCaseWithFactory
	{
		public void AsserContainsXmlValueByType(string xml, object data, Type type, IEnumerable<ZString> ignoreAssertPropertyNames = null)
		{
			if (data == null)
			{
				return;
			}
			var properties = type.GetProperties();
			foreach (var info in properties)
			{
				var name = info.Name;
				if (ignoreAssertPropertyNames != null && ignoreAssertPropertyNames.Any(x => x == $"{info.DeclaringType.FullName}.{info.Name}"))
				{
					continue;
				}
				var propertyType = info.PropertyType;
				object val = null;
				var expected = "";
				if (propertyType == typeof(ZDateTime))
				{
					val = info.GetValue(data, null);
					if (val != null)
					{
						expected = ">" + ((ZDateTime)val).ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
					}
				}
				else if (propertyType == typeof(ZDate))
				{
					val = info.GetValue(data, null);
					if (val != null)
					{
						expected = ">" + ((ZDate)val).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
					}
				}
				else if (propertyType == typeof(ZInt))
				{
					val = info.GetValue(data, null);
					if (val != null && ((ZInt)val) > 0)
					{
						expected = ">" + val.ToString() + "</";
					}
				}
				else if (propertyType == typeof(ZLong))
				{
					val = info.GetValue(data, null);
					if (val != null && ((ZLong)val) > 0)
					{
						expected = ">" + val.ToString() + "</";
					}
				}
				else if (propertyType == typeof(ZDecimal))
				{
					var concreteTypeProperty = data.GetType().GetProperty(info.Name);
					val = info.GetValue(data, null);
					if (val != null)
					{
						var att = concreteTypeProperty?.GetCustomAttribute(typeof(DecimalPlacesAttribute), true) as DecimalPlacesAttribute
							?? info.GetCustomAttribute(typeof(DecimalPlacesAttribute), true) as DecimalPlacesAttribute
							?? info.DeclaringType.GetInterfaces().Select(interfaceType => interfaceType.GetProperty(info.Name)?.GetCustomAttribute<DecimalPlacesAttribute>(true)).FirstOrDefault();

						var num = (ZDecimal)val;
						if (att != null)
						{
							num = num.Round(att.DecimalPlaces);
						}

						if (num > 0)
						{
							expected = ">" + num.ToString() + "</";
						}
					}
				}
				else if (propertyType == typeof(ZString))
				{
					val = info.GetValue(data, null);
					if (val != null)
					{
						name = val.ToString();
						if (!string.IsNullOrEmpty(name))
						{
							expected = ">" + name + "</";
						}
					}
				}
				else
				{
					val = info.GetValue(data,
						data.GetType().FullName.Contains("System.Collections.Generic.List") && propertyType != typeof(int) ?
						new object[] { 0 } : null);
					AsserContainsXmlValueByType(xml, val, propertyType, ignoreAssertPropertyNames);
				}
				if (!string.IsNullOrEmpty(expected))
				{
					NUnit.Framework.Assert.That(xml, NUnit.Framework.Does.Contain(expected), name + " => " + expected);
				}
			}
		}

		public void AsserContainsXmlByType(string xml, Type type)
		{
			var properties = type.GetProperties();
			foreach (var info in properties)
			{
				var propertyType = info.PropertyType;
				var expected1 = "";
				var expected2 = "";
				var expected3 = "";
				if (propertyType == typeof(DateTime) || propertyType == typeof(int) || propertyType == typeof(string))
				{
					if (!type.Name.EndsWith("Collection"))
					{
						var name = info.Name;
						var attributes = (XmlElementAttribute[])info.GetCustomAttributes(typeof(XmlElementAttribute), true);
						if (attributes.Length > 0)
						{
							name = attributes[0].ElementName;
							expected1 = "<" + name + ">";
							expected2 = "</" + name + ">";
							expected3 = "<" + name + "/>";
						}
					}
				}
				else
				{
					AsserContainsXmlByType(xml, propertyType);
				}
				if (!string.IsNullOrEmpty(expected1))
				{
					NUnit.Framework.Assert.That((xml.Contains(expected1) && xml.Contains(expected2)) || xml.Contains(expected3), NUnit.Framework.Is.True, expected1 + expected2);
				}
			}
		}

		public static ZString GetExpectedMessageXML(ZString name)
		{
			var path = string.Format(System.Globalization.CultureInfo.CurrentCulture, @"Enterprise.Customs.TW.Messaging.Testing.TestFile.{0}", name);
			using (var inStream = typeof(MessageBuilderTest).Assembly.GetManifestResourceStream(path))
			{
				return new StreamReader(inStream).ReadToEnd();
			}
		}
	}
}
