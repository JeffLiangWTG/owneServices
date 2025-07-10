using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectReader : Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader>
	{
		public CommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader)
		{
		}

		protected override void FillCountrySpecificDetails(CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, BaseJobComInvoiceLine parentInvoiceLine, bool invoiceLineIsInDatabase)
		{
			base.FillCountrySpecificDetails(invoiceLineData, invoiceLineRow, delaySetters, parentInvoiceLine, invoiceLineIsInDatabase);
			var vehicle = (invoiceLineRow as JobComInvoiceLine)?.FirstVehicle;
			if (invoiceLineData.AddInfoCollection != null && vehicle != null)
			{
				var cusVehicleRow = GetColumnIndexer(vehicle);

				var x = JobComInvoiceLineSchema.JI_AddInfo;

				SetValueIfNotNullAndEmpty(cusVehicleRow, CusVehicleSchema.CVH_Color, invoiceLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.InvoiceLine.Colour, logger), delaySetters);
				SetValueIfNotNullAndEmpty(cusVehicleRow, CusVehicleSchema.CVH_SerialNumber, invoiceLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.InvoiceLine.EngineNumber, logger), delaySetters);
				SetValueIfNotNullAndEmpty(cusVehicleRow, CusVehicleSchema.CVH_ModelName, invoiceLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.InvoiceLine.Make, logger), delaySetters);
				SetValueIfNotNullAndEmpty(cusVehicleRow, CusVehicleSchema.CVH_SupplyMethod, invoiceLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.InvoiceLine.VehicleFormat, logger), delaySetters);
				SetValueIfNotNullAndEmpty(cusVehicleRow, CusVehicleSchema.CVH_CarType, invoiceLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.InvoiceLine.VehicleType, logger), delaySetters);
				SetValueIfNotNullAndEmpty(cusVehicleRow, CusVehicleSchema.CVH_VehicleIdentificationNumber, invoiceLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.InvoiceLine.VIN, logger), delaySetters);
				SetManufacturedDate(cusVehicleRow, invoiceLineData.AddInfoCollection.GetZStringValue(AddInfoConstants.InvoiceLine.YearOfManufacture, logger), delaySetters);
				SetEngineCapacity(cusVehicleRow, invoiceLineData.AddInfoCollection.GetZIntValue(AddInfoConstants.InvoiceLine.EngineCapacity, logger), delaySetters);
			}
		}

		void SetManufacturedDate(IColumnIndexer row, ZString? value, Dictionary<string, ValueSetter> delaySetters)
		{
			if (value.HasValue && !value.Value.IsEmpty && Int32.TryParse(value.Value, out var manufacturedYear))
			{
				SetValue(row, CusVehicleSchema.CVH_ManufacturedDate, new ZDate(manufacturedYear, 1, 1), delaySetters);
			}
		}

		void SetEngineCapacity(IColumnIndexer row, ZInt? value, Dictionary<string, ValueSetter> delaySetters)
		{
			if (value.HasValue && !value.Value.IsEmpty)
			{
				ZShort capacity = (value.Value > short.MaxValue && value.Value < ZInt.Zero) ? ZShort.Zero : (short)value.Value;
				SetValue(row, CusVehicleSchema.CVH_EngineCapacity, capacity, delaySetters);
				SetValue(row, CusVehicleSchema.CVH_EngineCapacityUQ, "CC", delaySetters);
			}
		}

		void SetValueIfNotNullAndEmpty(IColumnIndexer row, SchemaStringColumn column, ZString? value, Dictionary<string, ValueSetter> delaySetters)
		{
			if (value.HasValue && !value.Value.IsEmpty)
			{
				SetValue(row, column, value.Value, delaySetters);
			}
		}
	}
}
