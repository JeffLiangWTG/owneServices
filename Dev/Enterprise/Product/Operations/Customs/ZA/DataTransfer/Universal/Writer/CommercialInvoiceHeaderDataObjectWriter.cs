using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectWriter : Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
	{
		internal CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, Customs.Business.CusEntryHeader relatedEntry = null)
			: base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override ZBool IsPopulateBondedWarehouseDetails(BaseJobComInvoiceLine invoiceLineBO)
		{
			var zaInvoiceLine = (JobComInvoiceLine)invoiceLineBO;
			var entry = zaInvoiceLine.CusEntryLine?.Header;
			return entry != null && entry.SupportsBondedWarehousing && ((zaInvoiceLine.SupplierPart != null && (entry.IsInwardBondedWarehousingEnabled || entry.IsOutwardBondedWarehousingEnabled)) || (zaInvoiceLine.NewOwnerProduct != null && (entry.EntryInstruction?.OwnerIsBondedWarehousing ?? false) && IsChangeOfOwnership(zaInvoiceLine.CusProcedure)));
		}

		protected override List<AddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoCollection(invoiceLineBO) ?? new List<AddInfo>();

			var invoiceLine = invoiceLineBO as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				AddAddInfoIfNotEmpty(result, AddInfoConstants.InvoiceLine.Colour, invoiceLine.JI_Colour);
				AddAddInfoIfNotEmpty(result, AddInfoConstants.InvoiceLine.EngineCapacity, invoiceLine.JI_EngineCapacity.ToString());
				AddAddInfoIfNotEmpty(result, AddInfoConstants.InvoiceLine.EngineNumber, invoiceLine.JI_EngineNumber);
				AddAddInfoIfNotEmpty(result, AddInfoConstants.InvoiceLine.Make, invoiceLine.JI_Make);
				AddAddInfoIfNotEmpty(result, AddInfoConstants.InvoiceLine.VehicleFormat, invoiceLine.JI_VehicleFormat);
				AddAddInfoIfNotEmpty(result, AddInfoConstants.InvoiceLine.VehicleType, invoiceLine.JI_VehicleType);
				AddAddInfoIfNotEmpty(result, AddInfoConstants.InvoiceLine.VIN, invoiceLine.JI_VIN);
				AddAddInfoIfNotEmpty(result, AddInfoConstants.InvoiceLine.YearOfManufacture, invoiceLine.JI_YearOfManufacture);
			}
			return result;
		}

		void AddAddInfoIfNotEmpty(List<AddInfo> addInfoList, string addInfoKey, ZString addInfoValue)
		{
			if (!addInfoValue.IsEmpty)
			{
				addInfoList.Add(new AddInfo()
				{
					Key = addInfoKey,
					Value = addInfoValue
				});
			}
		}

		bool IsChangeOfOwnership(RefCusProcedure procedure)
		{
			return procedure != null && procedure.IsIntoWarehouse() && procedure.IsOutOfWarehouse();
		}
	}
}
