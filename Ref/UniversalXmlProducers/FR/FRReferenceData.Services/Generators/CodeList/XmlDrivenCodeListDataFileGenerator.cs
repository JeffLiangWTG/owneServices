using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public abstract class XmlDrivenCodeListDataFileGenerator : CodeListDataFileGenerator
	{
		public sealed override IEnumerable<string> InputFiles => InputFileNames;

		protected override List<RefCusCodeList> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var result = new List<RefCusCodeList>();

			foreach (var inputFile in InputFiles)
			{
				currentProcessedFile = inputFile;
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, inputFile));

				var nodeList = xmlDocument.GetElementsByTagName("ligne");
				foreach (var node in nodeList.Cast<XmlNode>())
				{
					var codeList = ProcessEachXmlNode(node);
					if (codeList != null)
					{
						result.Add(codeList);
					}
				}
			}

			return UniversalDataHelper.FilterOutputListByDate(result).OrderBy(x => x.ZZD_Code).ThenByDescending(x => x.ZZD_StartDate).ToList();
		}

		protected virtual RefCusCodeList ProcessEachXmlNode(XmlNode node)
		{
			RefCusCodeList result = null;

			if (string.IsNullOrEmpty(ValidityTag) || UniversalDataHelper.GetTagValue(node, ValidityTag) == "valid")
			{
				var startDate = string.IsNullOrEmpty(StartDateTag) ? UniversalDataHelper.MinimumDateTime : UniversalDataHelper.GetStartDateFromTag(node, StartDateTag);
				var endDate = string.IsNullOrEmpty(EndDateTag) ? UniversalDataHelper.MaximumDateTime : UniversalDataHelper.GetEndDateFromTag(node, EndDateTag);

				if (UniversalDataHelper.CheckDatesAreValid(startDate, endDate))
				{
					var code = UniversalDataHelper.GetTagValue(node, CodeTag);
					var description = HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(node, DescriptionTag));
					var additionalDescription = !string.IsNullOrEmpty(AdditionalDescriptionTag) ? HttpUtility.HtmlDecode(UniversalDataHelper.GetTagValue(node, AdditionalDescriptionTag)) : string.Empty;
					if (!string.IsNullOrEmpty(additionalDescription))
					{
						description += " - " + additionalDescription;
					}
					result = new RefCusCodeList
					{
						ZZD_Code = code,
						ZZD_Description = description,
						ZZD_StartDate = startDate,
						ZZD_EndDate = endDate,
						RefCusCodeListAttributes = GetCusCodeListAttributesFromNode(node)
					};
				}
			}

			return result;
		}


		protected virtual RefCusCodeListAttribute[] GetCusCodeListAttributesFromNode(XmlNode node) => null;


		protected abstract string ValidityTag { get; }

		protected abstract string CodeTag { get; }

		protected abstract string StartDateTag { get; }

		protected abstract string EndDateTag { get; }

		protected abstract string DescriptionTag { get; }

		protected abstract string AdditionalDescriptionTag { get; }

		protected override string DataGrouping => UniversalDataHelper.Constants.France;

		public sealed override string OutputFile => $"{DataSource}.xml";

		protected string CurrentProcessedFile => currentProcessedFile;
		string currentProcessedFile;
	}
}
