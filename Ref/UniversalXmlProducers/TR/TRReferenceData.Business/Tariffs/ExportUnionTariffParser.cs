using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public class ExportUnionTariffParser
	{
		public void GenerateUniversalReferenceData(string outputFolderPath)
		{
			try
			{
				var dataFilePath = Path.Combine(ApplicationConfig.ResPath, @"TR Export Union Export Tariff Additional Codes.xlsx");
				var data = ExportUnionTariffLoader.LoadData(dataFilePath);
				Helper.ExportToXMLFile("TR Export Union Tariff", outputFolderPath, GetWriterConfiguration(), PublicationDateTime, GetEntities(data));
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"GenerateUniversalReferenceData failed. Exception: {ex.GetBaseException().Message}");
			}
		}

		IEnumerable<RefDataRepoModelEntityType> GetEntities(IEnumerable<CodeDescriptionPair> data)
		{
			var entities = new List<RefDataRepoModelEntityType>();
			entities.AddRange(GetTariffs(data));
			return entities;
		}

		protected virtual IEnumerable<RefCusTariff> GetTariffs(IEnumerable<CodeDescriptionPair> data)
		{
			foreach (var record in data)
			{
				yield return new RefCusTariff()
				{
					ZZ1_TariffCode = record.Code,
					ZZ1_Description = record.Description,
				};
			}
		}

		protected XmlWriterConfiguration GetWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var tariff = new EntityTypeConfiguration<RefCusTariff>(true);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffType.Code.TREUA);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariff.IncludeColumn(x => x.ZZ1_Description);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_StartDate, false, PublicationDateTime);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_EndDate, false, Constants.MaximumDateTime);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(tariff);

			return xmlWriterConfiguration;
		}

		protected virtual DateTime PublicationDateTime => DateTime.Now;

		public string ErrorMessage => ErrorBuilder.ToString();

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
