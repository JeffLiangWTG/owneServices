using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class AddInfoDataTransferTool
	{
		public AddInfoDataTransferTool(string addInfoPrefix, Predicate<ZPropertyInfo> shouldFieldBeExported)
		{
			this.addInfoPrefix = addInfoPrefix;
			this.shouldFieldBeExported = shouldFieldBeExported;
		}

		public readonly string addInfoPrefix;
		public Predicate<ZPropertyInfo> shouldFieldBeExported;

		public void ImportAddInfos(ZPropertyInfoHashtable zPropertyInfoHash, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
			if (!string.IsNullOrEmpty(addInfoPrefix))
			{
				var nameMapping = new Dictionary<string, ZPropertyInfo>();
				foreach (ZPropertyInfo info in zPropertyInfoHash)
				{
					nameMapping[GetNameForMatching(info.Name, addInfoPrefix)] = info;
				}
				ImportAddInfos(nameMapping, addCustomsDetails, context);
			}
		}

		public void ImportAddInfos<T>(T businessObject, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
			where T : BusinessObject, IAddInfoSchemaProvider
		{
			if (!string.IsNullOrEmpty(businessObject.TablePrefix))
			{
				var prefixWithUnderscore = businessObject.TablePrefix + "_";
				var nameMapping = new Dictionary<string, ZPropertyInfo>();
				foreach (var column in businessObject.AddInfoTableSchema.All)
				{
					var propertyInfo = businessObject.FindPropertyInfo(column.Name);
					if (propertyInfo != null)
					{
						nameMapping[GetNameForMatching(column.Name, prefixWithUnderscore)] = propertyInfo;
					}
				}

				ImportAddInfos(nameMapping, addCustomsDetails, context);
			}
		}

		static void ImportAddInfos(Dictionary<string, ZPropertyInfo> nameMapping, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
			if (addCustomsDetails != null)
			{
				foreach (Xsd.AdditionalCustomsInformation addCusInfo in addCustomsDetails)
				{
					if (nameMapping.TryGetValue(addCusInfo.CustomsDetailType, out var propertyInfo))
					{
						context.SetPropertyInfoValue(propertyInfo, addCusInfo.CustomsDetailValue, addCusInfo.CustomsDetailValueSpecified);
					}
				}
			}
		}

		public void ExportAddInfos(Xsd.AdditionalCustomsInformationCollection xmlAddInfo, ZPropertyInfoHashtable zPropertyInfoHash)
		{
			SortedList namesToValues = GetAddInfoNamesToValuesSortedDeterministically(zPropertyInfoHash, addInfoPrefix);

			WriteAddInfosToXml(namesToValues, xmlAddInfo);
		}

		public void ExportAddInfos<T>(Xsd.AdditionalCustomsInformationCollection xmlAddInfo, T businessObject)
			where T : BusinessObject, IAddInfoSchemaProvider
		{
			var zPropertyInfos = businessObject.AddInfoTableSchema.All.Where(x => !x.IsPKColumn && !x.Name.EndsWith("_ClusterKey")).Select(x => businessObject.FindPropertyInfo(x.Name));
			var namesToValues = GetAddInfoNamesToValuesSortedDeterministically(zPropertyInfos, businessObject.TablePrefix + "_");

			WriteAddInfosToXml(namesToValues, xmlAddInfo);
		}

		SortedList GetAddInfoNamesToValuesSortedDeterministically(IEnumerable zPropertyInfos, string tablePrefix)
		{
			SortedList result = new SortedList();

			if (zPropertyInfos != null)
			{
				foreach (ZPropertyInfo info in zPropertyInfos)
				{
					if (info != null && !info.Value.IsEmpty && shouldFieldBeExported(info))
					{
						string name = GetNameForMatching(info.Name, tablePrefix);
						string value = info.Value.ToString();

						result.Add(name, value);
					}
				}
			}

			return result;
		}

		static void WriteAddInfosToXml(SortedList namesToValues, Xsd.AdditionalCustomsInformationCollection xmlAddInfo)
		{
			foreach (DictionaryEntry entry in namesToValues)
			{
				string addInfoPropertyName = (string)entry.Key;
				string addInfoPropertyValue = (string)entry.Value;

				var newAddInfo = xmlAddInfo.FindByType(addInfoPropertyName) ?? xmlAddInfo.AddNew();

				newAddInfo.CustomsDetailType = addInfoPropertyName;
				newAddInfo.CustomsDetailValue = addInfoPropertyValue;
			}
		}

		static string GetNameForMatching(ZString propertyName, string addInfoPrefix)
		{
			if (propertyName.StartsWith(addInfoPrefix))
			{
				propertyName = propertyName.SubstringSafe(addInfoPrefix.Length);
			}

			return propertyName.Replace("_Hidden", "");
		}
	}
}
