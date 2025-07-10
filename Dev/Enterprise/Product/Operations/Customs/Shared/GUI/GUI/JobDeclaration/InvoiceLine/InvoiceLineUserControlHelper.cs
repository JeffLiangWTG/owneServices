using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GUI
{
	public static class InvoiceLineUserControlHelper
	{
		public static void SetTariffRelated(ZGrid customsInvoiceLinesBoundGrid, string name, Universal.GUI.TariffFindBox tariffFindBox, Func<string> getCustomsCountryCode, Func<ZString> getDataGrouping, ZString universalTariffType)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				SetColumnPositions(customsInvoiceLinesBoundGrid, name);

				tariffFindBox.GetCountryCode = getCustomsCountryCode;
				tariffFindBox.GetDataGrouping = getDataGrouping;
				tariffFindBox.TariffType = universalTariffType;

				if (customsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff) == null)
				{
					var tariffTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();

					tariffTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceLineUserControlHelper|64FEDA5E-F2A1-4A90-82F5-C03C3CBF52DD", "Tariff");
					tariffTextBoxColumnStyleInfo.ColumnName = BaseJobComInvoiceLine.Schema.JI_Tariff;
					customsInvoiceLinesBoundGrid.ColumnStyles.Add(tariffTextBoxColumnStyleInfo);
				}
			}
		}

		static void SetColumnPositions(ZGrid customsInvoiceLinesBoundGrid, string name)
		{
			var pos = 0;
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_LineNo, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_Calc_Invoice, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_PartNo, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CC, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_Tariff, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_InvoiceQuantity, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_InvoiceUQ, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CustomsQuantity, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CustomsUnitQty, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_LinePrice, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_Description, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CountryOfOrigin, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_RH_NKCommodity_Code, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_Weight, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_WeightUQ, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_NetWeight, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_NetWeightUQ, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_Volume, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_VolumeUQ, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_OrderNumber, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_PartAttrib1, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_PartAttrib2, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_PartAttrib3, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_SerialNumber, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.UnitPrice, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CustomAttrib1, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CustomAttrib2, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CustomAttrib3, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CustomAttrib4, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CustomAttrib5, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CustomAttrib6, name, pos++);
			SetColumnPositionBeforeBinding(customsInvoiceLinesBoundGrid, BaseJobComInvoiceLine.Schema.JI_CustomTextBlob1, name, pos++);
		}

		static void SetColumnPositionBeforeBinding(ZGrid grid, string columnName, string name, int newIndex)
		{
			Core.Forms.ZGridColumnInfo columnInfo = grid.GetColumnStyle(columnName);
			if (columnInfo == null)
			{
				ErrorReporter.ReportOnce(columnName + ":" + name + "." + grid.Name, "Column [" + columnName + "] does not exist in the Grid [" + name + "." + grid.Name + "]. Cannot set Column Position.");
			}
			else
			{
				grid.ColumnStyles.Remove(columnInfo);
				grid.ColumnStyles.Insert(newIndex, columnInfo);
			}
		}
	}
}
