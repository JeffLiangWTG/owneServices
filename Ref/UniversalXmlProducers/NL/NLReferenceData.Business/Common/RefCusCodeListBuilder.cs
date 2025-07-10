using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public abstract class RefCusCodeListBuilder<T> : IDataBuilder<T> where T : class
	{
		protected RefCusCodeListBuilder(StringBuilder errorCollector)
		{
			if (errorCollector == null)
			{
				throw new ArgumentNullException(nameof(errorCollector));
			}
			ErrorCollector = errorCollector;
		}
		protected StringBuilder ErrorCollector { get; }

		public void BuildXml(DateTime publicationDate, IList<T> data, string outputPath)
		{
			var orderedData = OrderList(data);
			var content = ConvertTableElementsToRefData(orderedData);
			GenerateUniversalReferenceDataXml(content, publicationDate, outputPath);
		}

		protected Collection<RefCusCodeList> ConvertTableElementsToRefData(IEnumerable<T> data)
		{
			var refModels = ConvertToRefModels(data);
			var uniqueItems = new HashSet<string>();
			var content = new Collection<RefCusCodeList>();

			foreach (var refModel in refModels)
			{
				if (IsValid(refModel))
				{
					var uniqueId = UniqueId(refModel);
					if (uniqueItems.Add(uniqueId))
					{
						content.Add(refModel);
					}
					else
					{
						DuplicateError(refModel, uniqueId);
					}
				}
			}
			return content;
		}

		protected void GenerateUniversalReferenceDataXml(IEnumerable<RefCusCodeList> content, DateTime publicationDate, string outputPath)
		{
			if (content.Any())
			{
				var dataSource = $"NL CodeList - {XMLWriterDataSource}";
				Helper.ExportToXMLFile(dataSource, Path.Combine(outputPath, GetOutputFileName(dataSource, publicationDate)), XmlWriterConfiguration(), publicationDate, UpdateType.Full, content);
			}
		}

		protected abstract StringBuilder IsValidCore(RefCusCodeList refModel);

		protected bool IsValid(RefCusCodeList refModel)
		{
			StringBuilder validationErrors = IsValidCore(refModel);
			bool valid = validationErrors.Length == 0;

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
				var msg = Invariant($"RefCusCodeList validation error. Key '{refModel.ZZD_Code}_{refModel.ZZD_ZZK_NKCodeType}' Errors: {validationErrors}'");
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

		protected abstract IEnumerable<RefCusCodeList> ConvertToRefModels(IEnumerable<T> data);

		protected static RefCusCodeListAttribute CreateAttribute(string name, string value)
		{
			var result = new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = name,
				ZZE_Value = value
			};

			return result;
		}

		protected static RefCusCodeList CreateRefCusCodeList(TableElement element, string codeType, params RefCusCodeListAttribute[] attributes)
		{
			return new RefCusCodeList
			{
				ZZD_Code = element.elementCode,
				ZZD_Description = string.Join(" - ", element.elementDescription, element.elementLegalDescription).Trim(new char[] { ' ', '-' }),
				ZZD_ZZZ_NKDataGrouping = Constants.DefaultValues.NLDataGrouping,
				ZZD_ZZK_NKCodeType = codeType,
				ZZD_StartDate = Constants.DefaultValues.MinimumDateTime,
				ZZD_EndDate = Constants.DefaultValues.MaximumDateTime,
				RefCusCodeListAttributes = attributes
			};
		}

		protected abstract XmlWriterConfiguration XmlWriterConfiguration();

		protected abstract string XMLWriterDataSource { get; }
		protected static string GetOutputFileName(string sourceName, DateTime publicationDate) => Invariant($"{sourceName}_{publicationDate:HHmmssfff}.xml");

		protected abstract IEnumerable<T> OrderList(IList<T> data);
	}
}
