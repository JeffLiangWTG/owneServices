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
	public abstract class ExchangeRatesBuilder<TNode> : IExchangeRatesBuilder<TNode>
	{
		protected ExchangeRatesBuilder(StringBuilder errorCollector)
		{
			ErrorCollector = errorCollector ?? throw new ArgumentNullException(nameof(errorCollector));
		}

		protected StringBuilder ErrorCollector { get; }

		public void BuildXml(DateTime publicationDate, IEnumerable<TNode> data, string outputPath)
		{
			var content = ConvertRateNodesToRefExchangeRateZZ(data, publicationDate);
			GenerateUniversalReferenceDataXml(content, publicationDate, outputPath);
		}

		public Collection<RefExchangeRateZZ> ConvertRateNodesToRefExchangeRateZZ(IEnumerable<TNode> data, DateTime activationDate)
		{
			var refExchangeRateZZs = ConvertToRefExchangeRateZZs(data, activationDate);
			var uniqueItems = new HashSet<string>();
			var content = new Collection<RefExchangeRateZZ>();

			foreach (var refExchangeRateZZ in refExchangeRateZZs)
			{
				if (IsValid(refExchangeRateZZ))
				{
					var uniqueId = UniqueId(refExchangeRateZZ);
					if (!uniqueItems.Contains(uniqueId))
					{
						uniqueItems.Add(uniqueId);
						content.Add(refExchangeRateZZ);
					}
					else
					{
						DuplicateError(refExchangeRateZZ, uniqueId);
					}
				}
			}

			return content;
		}

		public void GenerateUniversalReferenceDataXml(IEnumerable<RefExchangeRateZZ> content, DateTime publicationDate, string outputPath)
		{
			if (content.Any())
			{
				var dataSource = $"{XMLWriterDataSource}";
				Helper.ExportToXMLFile(dataSource, Path.Combine(outputPath, GetOutputFileName(dataSource, publicationDate)), GetRefExchangeRateWriterConfiguration(publicationDate), publicationDate, GetUpdateType(), content);
			}
		}

		protected bool IsValid(RefExchangeRateZZ refExchangeRateZZ)
		{
			bool valid = true;

			var validationErrors = new StringBuilder();

			if (string.IsNullOrWhiteSpace(refExchangeRateZZ.ZZN_RX_NKExCurrency))
			{
				validationErrors.Append("ZZN_RX_NKExCurrency is required. ");
				valid = false;
			}

			if (refExchangeRateZZ.ZZN_StartDate == DateTime.MinValue)
			{
				validationErrors.Append("ZZN_StartDate is required. ");
				valid = false;
			}

			if (refExchangeRateZZ.ZZN_Rate.Equals(decimal.Zero))
			{
				validationErrors.Append("ZZN_Rate is required. ");
				valid = false;
			}

			if (refExchangeRateZZ.ZZN_Rate.CompareTo(decimal.Zero) <= 0)
			{
				validationErrors.Append("ZZN_Rate has to be greater than 0.0000. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"RefExchangeRateZZ validation error: Key '{refExchangeRateZZ.ZZN_RX_NKExCurrency}_{refExchangeRateZZ.ZZN_StartDate:yyyyMMdd}' Errors: {validationErrors}");
				ErrorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected void DuplicateError(RefExchangeRateZZ refExchangeRateZZ, string uniqueId)
		{
			var msg = Invariant($"RefExchangeRateZZ duplicate exists. Key: '{uniqueId}' Rate: {refExchangeRateZZ.ZZN_Rate}");
			ErrorCollector.AppendLine(msg);
		}

		protected string UniqueId(RefExchangeRateZZ refExchangeRateZZ) => $"{refExchangeRateZZ.ZZN_RX_NKExCurrency}_{refExchangeRateZZ.ZZN_StartDate:yyyyMMdd}";

		protected string GetOutputFileName(string sourceName, DateTime publicationDate) => Invariant($"{FilePrefix}_{sourceName}_{publicationDate:HHmmssfff}.xml");

		protected string FilePrefix => "RefExchangeRateZZ";

		protected virtual string XMLWriterDataSource => "NL Fiscal Exchange Rates";

		protected virtual UpdateType GetUpdateType() => UpdateType.Partial;

		protected abstract IEnumerable<RefExchangeRateZZ> ConvertToRefExchangeRateZZs(IEnumerable<TNode> data, DateTime activationDate);

		protected abstract XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(DateTime activationDate);
	}
}
