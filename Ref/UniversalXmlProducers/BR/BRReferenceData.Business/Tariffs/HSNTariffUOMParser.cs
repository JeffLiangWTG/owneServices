using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class HSNTariffUOMParser : BaseParser
	{
		public HSNTariffUOMParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputFileName, string outputFileName, DateTime publicationTime)
		{
			var refCusCodeLists = GetRefCusTariffLists(inputFileName);
			var writer = Helper.GetRefCusTariffUnitOfMeasureWriterConfiguration();
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writer, refCusCodeLists);
		}

		public static List<RefCusTariff> GetRefCusCodeListsByPath(string inputFileName)
		{
			Argument.NotNullOrEmpty(inputFileName, nameof(inputFileName));

			using (var input = new FileStream(inputFileName, FileMode.Open))
			{
				return GetRefCusTariffLists(input);
			}
		}

		static List<RefCusTariff> GetRefCusTariffLists(Stream stream)
		{
			Argument.NotNull(stream, nameof(stream));
			var result = new List<RefCusTariff>();

			var xls = new XlsFile(stream, true);
			Contract.Assume(xls != null);

			var rowCount = xls.GetRowCount(xls.ActiveSheet);
			for (var rowId = 2; rowId <= rowCount; rowId++)
			{
				var code = xls.GetCellValue(rowId, 1)?.ToString().Trim();

				if (!string.IsNullOrEmpty(code))
				{
					var refCusTariff = new RefCusTariff
					{
						ZZ1_TariffCode = code
					};

					var refCusTariffUOM = new RefCusTariffUOM
					{
						ZZ8_Type = Constants.TariffUnitOfMeasureCodes.CustomsUOM1,
						ZZ8_UOM = xls.GetCellValue(rowId, 4)?.ToString().Trim()
					};

					refCusTariff.RefCusTariffUOMs = new RefCusTariffUOM[] { refCusTariffUOM };

					result.Add(refCusTariff);
				}
			}

			return result;
		}
	}
}
