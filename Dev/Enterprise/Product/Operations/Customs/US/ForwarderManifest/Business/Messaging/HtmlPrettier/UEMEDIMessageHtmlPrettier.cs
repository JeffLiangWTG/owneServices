using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Customs.US.MessageDefinitions.ExportManifest;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMEDIMessageHtmlPrettier
	{
		public string PrettyHtml(string xmlMessageText)
		{
			var tableCreator = new HtmlTableCreator(TableAttributes) { EnableHTMLEncoding = false };
			var manifestFiling = UEMMessageHelper.DeSerializeManifestFiling(xmlMessageText);
			CreateTable(tableCreator, manifestFiling, 0);
			return tableCreator.ToHtml();
		}

		void CreateTable(HtmlTableCreator htmlTableCreator, object obj, int level)
		{
			if (obj != null)
			{
				var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

				foreach (var property in properties)
				{
					var xmlElementAttribute = property.GetCustomAttribute(typeof(XmlElementAttribute)) as XmlElementAttribute;
					if (xmlElementAttribute == null)
					{
						continue;
					}
					var value = property.GetValue(obj);
					var fieldName = UEMMessageHelper.SeparateElementName(xmlElementAttribute.ElementName);
					var fieldCell = new CellWithFormatting(fieldName, PaddingLeftAttributes(level));

					if (property.PropertyType == typeof(string))
					{
						htmlTableCreator.WriteRow(fieldCell, value);
					}
					else if (value is FilingInfoType filingInfoType)
					{
						htmlTableCreator.WriteRow(BoldRowAttributes, fieldCell, ZString.Empty);
						CreateTable(htmlTableCreator, value, level + 1);
					}
					else if (value is ManifestStringType manifestStringType)
					{
						if (manifestStringType.ErrorListSpecified)
						{
							htmlTableCreator.WriteRow(BoldRowAttributes, fieldCell, ZString.Empty);
							CreateTable(htmlTableCreator, value, level + 1);
						}
						else
						{
							var stringValue = manifestStringType.Value;
							if (fieldName.Contains("Date"))
							{
								var formats = new string[] { "dd/MM/yyyy HH:mm:ss", "yyyyMMdd HHmmss", "yyyyMMdd" };
								if (DateTime.TryParseExact(stringValue, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
								{
									if (fieldName.Contains("Date Time"))
									{
										htmlTableCreator.WriteRow(fieldCell, dt.ToString("yyyy-MM-dd HH:mm:ss"));
									}
									else
									{
										htmlTableCreator.WriteRow(fieldCell, dt.ToString("yyyy-MM-dd"));
									}
								}
								else
								{
									htmlTableCreator.WriteRow(fieldCell, stringValue);
								}
							}
							else
							{
								htmlTableCreator.WriteRow(fieldCell, stringValue);
							}
						}
					}
					else if (value is IEnumerable collection)
					{
						foreach (var subObj in collection)
						{
							if (subObj.GetType() == typeof(string))
							{
								htmlTableCreator.WriteRow(fieldCell, subObj);
							}
							else
							{
								htmlTableCreator.WriteRow(BoldRowAttributes, fieldCell, ZString.Empty);
								CreateTable(htmlTableCreator, subObj, level + 1);
							}
						}
					}
				}
			}
		}

		NameValueCollection PaddingLeftAttributes(int level) => new ()
		{
			{ "style", $"padding-left:{level * 10}" }
		};

		NameValueCollection BoldRowAttributes => new ()
		{
			{ "style", "font-weight:bold;background-color:#8eaadb" }
		};

		NameValueCollection TableAttributes => new ()
		{
			{ "border", "1" },
			{ "cellpadding", "1" },
			{ "cellspacing", "0" },
			{ "width", "100%" },
			{ "class", "table" },
			{ "style", "table-layout: fixed;word-wrap: break-word;" }
		};
	}
}
