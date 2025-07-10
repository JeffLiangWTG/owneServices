using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public class InvoiceLinesGridFilterStripBusinessObject : GridFilterStripBusinessObject
	{
		public InvoiceLinesGridFilterStripBusinessObject()
		{
		}

		public InvoiceLinesGridFilterStripBusinessObject(ZGrid grid) : base(grid)
		{
		}

		protected override ModuleFilter GetModuleFilterCore(BusinessObject current, ZGridColumnInfo columnInfo, ZGridColumnStyle columnStyle, SchemaColumn columnSchema, ICollection<string> flagNames, ICollection<SchemaBoolColumn> flagColumns, string headerText)
		{
			if (columnSchema?.Name == BaseJobComInvoiceLine.Schema.JI_FormattedTariff)
			{
				return new ModuleNumberFilter(BaseJobComInvoiceLine.Schema.JI_FormattedTariff, JobComInvoiceLineSchema.JI_Tariff) { MultilingualDescription = (NoResString)headerText };
			}

			return base.GetModuleFilterCore(current, columnInfo, columnStyle, columnSchema, flagNames, flagColumns, headerText);
		}
	}
}
