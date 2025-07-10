using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TR.GUI
{
	public static class InvoiceLineUserControlHelper
	{
		public static string[] ColumnNamesInSortOrder(List<string> defaultcolumnList)
		{
			var index = defaultcolumnList.IndexOf(JobComInvoiceLine.Schema.JI_Description);
			defaultcolumnList.Insert(index + 1, JobComInvoiceLine.Schema.JI_NDescription);
			return defaultcolumnList.ToArray();
		}

		public static void InitializeContainerGridLayout(ZGrid containerGrid)
		{
			var visibleColumns = new[] { NonPersistentCusContainer.Schema.ContainerNumber, NonPersistentCusContainer.Schema.IsForInvoiceLine, NonPersistentCusContainer.Schema.OwnerCountry };
			var invisibleColumns = containerGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(c => c.ColumnName).Except(visibleColumns).ToArray();
			containerGrid.SetColumnVisible(false, invisibleColumns);
			containerGrid.SetColumnVisible(true, visibleColumns);

			containerGrid.SetColumnCaption(NonPersistentCusContainer.Schema.ContainerNumber, Res.GetString("0027E928-990D-4219-8FBD-6AD853595B70", "Container No"));
			containerGrid.SetColumnCaption(NonPersistentCusContainer.Schema.OwnerCountry, Res.GetString("D751F30E-943A-40EA-B0E6-1FF708741801", "Container Country"));

			containerGrid.SetColumnWidth(NonPersistentCusContainer.Schema.IsForInvoiceLine, 120);
			containerGrid.SetColumnWidth(NonPersistentCusContainer.Schema.OwnerCountry, 120);
		}
	}
}
