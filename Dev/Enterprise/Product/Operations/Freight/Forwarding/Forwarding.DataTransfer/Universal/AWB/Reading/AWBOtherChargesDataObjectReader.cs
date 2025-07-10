using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class AWBOtherChargesDataObjectReader : DataObjectReader<AWBOtherCharges, ExportAWBOtherCharges>
	{
		public AWBOtherChargesDataObjectReader(AWBOtherCharges otherChargesDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(otherChargesDataObject, logger, factory)
		{
		}

		protected override ExportAWBOtherCharges GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(ExportAWBOtherCharges otherChargesBO)
		{
			SetValue(otherChargesBO, ExportAWBOtherChargesSchema.EO_ChargeCode, dataObject.ChargeCode);
			SetValue(otherChargesBO, ExportAWBOtherChargesSchema.EO_EntitlementCode, dataObject.EntitlementDue);
			SetValue(otherChargesBO, ExportAWBOtherChargesSchema.EO_PPDCLT, dataObject.PrepaidCollect);
			SetValue(otherChargesBO, ExportAWBOtherChargesSchema.EO_ChargeDescription, dataObject.Description);
			SetValue(otherChargesBO, ExportAWBOtherChargesSchema.EO_Amount, dataObject.Amount);
		}
	}
}
