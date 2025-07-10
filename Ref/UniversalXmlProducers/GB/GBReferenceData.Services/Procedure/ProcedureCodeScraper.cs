using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Procedure
{
	internal class ProcedureCodeScraper
	{
		public ProcedureCodeScraper(string html)
		{
			page = new CDSWebpageTableToDataTable(html);
		}

		readonly IWebpageTableToDataTable page;

		const string CategoryProcedureHeaderXPath = "//div/table[1]/thead/tr";
		const string CategoryProcedureDataXPath = "//div/table[1]/tbody/tr";
		const string ProcedureDescriptionsHeaderXPath = "//div/table[2]/thead/tr[th='Code']|//div/table[2]/tbody/tr[1][td[1]='Code']";
		const string ProcedureDescriptionsDataXPath = "//div/table[position()>1]/tbody/tr[td[1]!='Code']";

		public IReadOnlyCollection<CategoryProcedureMapping> ExtractCategoryProcedureMapping()
		{
			var dataTable = page.ExtractDatatableFromGrid(CategoryProcedureHeaderXPath, CategoryProcedureDataXPath);
			if (dataTable.Columns.Count == 3)
			{
				return dataTable.Rows.Cast<DataRow>()
					.Select(row => new CategoryProcedureMapping()
					{
						CategoryCode = row.Field<string>(0),
						ProcedureMapping = row.Field<string>(2)
					})
					.ToList()
					.AsReadOnly();
			}
			else
			{
				return new List<CategoryProcedureMapping>().AsReadOnly();
			}
		}

		internal IReadOnlyCollection<ProcedureCodeRaw> ExtractProcedureDescriptions(string shipmentType)
		{
			var dataTable = page.ExtractDatatableFromGrid(ProcedureDescriptionsHeaderXPath, ProcedureDescriptionsDataXPath);
			if (dataTable.Columns.Count == 2)
			{
				return dataTable.Rows.Cast<DataRow>()
					.Select(row => new ProcedureCodeRaw()
					{
						Code = row.Field<string>(0),
						Description = row.Field<string>(1),
						ShipmentType = shipmentType
					})
					.ToList()
					.AsReadOnly();
			}
			else
			{
				return new List<ProcedureCodeRaw>().AsReadOnly();
			}
		}
	}
}
