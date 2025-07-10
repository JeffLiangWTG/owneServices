using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Models;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Processing;
using CargoWise.RefDbRepo.SharedReferenceData.Business;
using static System.FormattableString;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.CDSPortData
{
	public class PortBuilder : IPortBuilder
	{
		public PortBuilder(StringBuilder errorCollector)
		{
			if (errorCollector == null)
			{
				throw new ArgumentNullException(nameof(errorCollector));
			}

			ErrorCollector = errorCollector;
		}
		StringBuilder ErrorCollector;

		public void BuildXml(DateTime publicationDate, IEnumerable<PortData> data, string outputPath, Task<IEnumerable<CCSUKLocation>> ccsukData)
		{
			var refModels = ConvertToRefModels(data, ccsukData);
			var uniqueItems = new HashSet<string>();
			var content = new List<RefCusCodeList>();

			foreach (var refModel in refModels)
			{
				if (IsValid(refModel))
				{
					var uniqueId = UniqueId(refModel);
					if (!uniqueItems.Contains(uniqueId))
					{
						uniqueItems.Add(uniqueId);
						content.Add(refModel);
					}
					else
					{
						DuplicateError(refModel, uniqueId);
					}
				}
			}

			if (content.Any())
			{
				var sourceName = data.First().Source.Name;
				var dataSource = $"{XMLWriterDataSource}({sourceName})";
				Helper.ExportToXMLFile(dataSource, Path.Combine(outputPath, GetOutputFileName(sourceName, publicationDate)), XmlWriterConfiguration(), publicationDate, UpdateType.Partial, content);
			}
		}

		protected static string FilePrefix => "RefCusCodeList";
		protected static string XMLWriterDataSource => "CDS 5/23 Port Data";
		protected static string GetOutputFileName(string sourceName, DateTime publicationDate) => Invariant($"{FilePrefix}_{sourceName}_{publicationDate:HHmmssfff}.xml");

		protected static XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusCodeList>(true);

			entityConfig.IncludeColumn(x => x.ZZD_Code, true);
			entityConfig.IncludeColumn(x => x.ZZD_Description);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "PORT");
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CDSDataGrouping);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, DefaultValues.MinimumDateTime);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, DefaultValues.MaximumDateTime);
			entityConfig.IncludeColumn(x => x.RefCusCodeListAttributes);

			writerConfig.IncludeEntityTypeConfiguration(entityConfig);

			var attribConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			attribConfig.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			attribConfig.IncludeColumn(x => x.ZZE_Value, true);

			writerConfig.IncludeEntityTypeConfiguration(attribConfig);

			return writerConfig;
		}

		protected bool IsValid(RefCusCodeList refModel)
		{
			bool valid = true;

			var validationErrors = new StringBuilder();

			if (string.IsNullOrWhiteSpace(refModel.ZZD_Code))
			{
				validationErrors.Append("ZZD_Code is required. ");
				valid = false;
			}

			if (string.IsNullOrWhiteSpace(refModel.ZZD_Description))
			{
				validationErrors.Append("ZZD_Description is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"RefCusCodeList validation error. Key: '{refModel.ZZD_Code}' Errors: '{validationErrors}'");
				ErrorCollector.AppendLine(msg);
			}

			return valid;
		}
		protected void DuplicateError(RefCusCodeList refModel, string uniqueId)
		{
			var msg = Invariant($"RefCusCodeList duplicate exists. Key: '{uniqueId}' Description: {refModel.ZZD_Description}");
			ErrorCollector.AppendLine(msg);
		}
		protected static string UniqueId(RefCusCodeList refModel) => $"{refModel.ZZD_Code}_{refModel.ZZD_ZZK_NKCodeType}";

		protected static IEnumerable<RefCusCodeList> ConvertToRefModels(IEnumerable<PortData> data, Task<IEnumerable<CCSUKLocation>> ccsukData)
		{
			var results = new List<RefCusCodeList>();

			foreach (var pd in data)
			{
				if (pd.Code.Length > 4)
				{
					var newRefCode = new RefCusCodeList
					{
						ZZD_Code = pd.Code.Substring(4),
						ZZD_Description = pd.Description,
						RefCusCodeListAttributes = GetAttributes(pd)
					};

					results.Add(newRefCode);
				}
			}

			if (data.Any() && data.First().Source.CheckCCSUKLocation)
			{
				CreateCCSUKLocationAttributes(results, ccsukData);
			}

			return results;
		}

		static RefCusCodeListAttribute[] GetAttributes(PortData pd)
		{
			var attribs = new List<RefCusCodeListAttribute>();

			attribs.Add(CreateAttrib("FACTY", pd.Code.Substring(2, 2)));

			if (!string.IsNullOrEmpty(pd.Source.AttributeName) && !string.IsNullOrEmpty(pd.Source.Code))
			{
				var attribValue = ConvertAttributeValue(pd);
				attribs.Add(CreateAttrib(pd.Source.AttributeName, attribValue));
			}

			return attribs.ToArray();
		}

		static RefCusCodeListAttribute CreateAttrib(string code, string value)
		{
			return new RefCusCodeListAttribute { ZZE_ZXE_NKName = code, ZZE_Value = value };
		}

		static void CreateCCSUKLocationAttributes(List<RefCusCodeList> data, Task<IEnumerable<CCSUKLocation>> ccsukData)
		{
			var locations = ccsukData.Result;

			foreach (var portGroup in data.Where(x => x.ZZD_Code.Length >= 9).GroupBy(x => new { locationCode = x.ZZD_Code.Substring(3, 6) }))
			{
				if (locations.Any(x => x.Code == portGroup.Key.locationCode))
				{
					var cukPort = portGroup.FirstOrDefault(x => x.ZZD_Code.Length == 12 && x.ZZD_Code.EndsWith("CUK", StringComparison.Ordinal));
					if (cukPort != null)
					{
						AddCCSUKAttribute(cukPort, portGroup.Key.locationCode);
					}
					else
					{
						foreach (var item in portGroup)
						{
							AddCCSUKAttribute(item, portGroup.Key.locationCode);
						}
					}
				}
			}
		}

		static void AddCCSUKAttribute(RefCusCodeList item, string locationCode)
		{
			var attrib = CreateAttrib("CCSUK", locationCode);

			var attribArr = item.RefCusCodeListAttributes;
			Array.Resize(ref attribArr, attribArr.Length + 1);
			attribArr[attribArr.Length - 1] = attrib;
			item.RefCusCodeListAttributes = attribArr;
		}

		static string ConvertAttributeValue(PortData pd)
		{
			switch (pd.Source.AttributeName)
			{
				case Constants.AttributeNames.RORO:
					return ConvertROROAttributeValue(pd.Source.Code, pd.AdditionalInfo);

				default:
					return pd.Source.Code;
			}
		}

		static string ConvertROROAttributeValue(string code, string addInfo)
		{
			var value = code;

			if (string.IsNullOrWhiteSpace(addInfo) || addInfo.Trim() == "-" || addInfo.Trim() == "—") //(char)45 and (char)8212
			{
				value = string.Empty;
			}

			return value;
		}
	}
}
