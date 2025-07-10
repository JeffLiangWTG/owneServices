using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class AWBOtherChargesDataObjectWriter : DataObjectWriter<ExportAWBOtherCharges, AWBOtherCharges>
	{
		internal AWBOtherChargesDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override AWBOtherCharges PopulateDataObject(ExportAWBOtherCharges otherCharges)
		{
			var otherChargesDataObject = new AWBOtherCharges();

			otherChargesDataObject.ChargeCode = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(otherCharges.EO_ChargeCode, otherCharges.IATAChargeCodesList);
			otherChargesDataObject.EntitlementDue = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(otherCharges.EO_EntitlementCode, otherCharges.EntitlementCodesList);
			otherChargesDataObject.PrepaidCollect = ListHelper.GetWithDescription<CodeDescriptionPair>(otherCharges.EO_PPDCLT, otherCharges.PrepayCollectList);
			otherChargesDataObject.Description = otherCharges.EO_ChargeDescription;
			otherChargesDataObject.Amount = otherCharges.EO_Amount;

			return otherChargesDataObject;
		}
	}
}
