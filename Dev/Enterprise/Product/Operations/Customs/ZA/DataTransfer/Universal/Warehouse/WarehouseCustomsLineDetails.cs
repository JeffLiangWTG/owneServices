using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	class WarehouseCustomsLineDetails : WarehouseCustomsLineDetailsWithEntryInstruction
	{
		public WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail)
			: base(factory, invoiceLine, fallbackDetail)
		{
		}

		public WarehouseCustomsLineDetails(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail, Shipment shipment)
			: base(factory, invoiceLine, fallbackDetail, shipment)
		{
		}

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		protected override List<UniversalAddInfo> GetAddInfosApplicableForWarehousing()
		{
			var addInfos = base.GetAddInfosApplicableForWarehousing();
			if (!CustomsProcedureCode.IsEmpty)
			{
				addInfos.Add(new UniversalAddInfo() { Key = BondedWarehousingHelper.Constants.OriginalProcedureCode, Value = CustomsProcedureCode });
			}
			return addInfos;
		}

		protected override ZDecimal GetValueForDuty()
		{
			var entryInstructionID = InvoiceLine.EntryInstructionLink;
			var entryLineNumber = InvoiceLine.EntryLineNumber;
			var entryLine = shipment
								?.EntryHeaderCollection.FirstOrDefault(h => h.EntryInstructionLink == entryInstructionID)
								?.EntryLineCollection.FirstOrDefault(l => l.LineNumber == entryLineNumber);

			return entryLine?.CustomsValue.GetValueOrDefault() ?? InvoiceLine.CustomsValue.GetValueOrDefault().RoundUsingCustomsValueRule();
		}
	}
}
