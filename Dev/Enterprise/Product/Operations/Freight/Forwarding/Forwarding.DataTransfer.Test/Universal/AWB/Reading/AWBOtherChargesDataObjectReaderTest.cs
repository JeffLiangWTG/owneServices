using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class AWBOtherChargesDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestReadOtherCharges()
		{
			var otherChargesDO = new AWBOtherCharges();

			otherChargesDO.ChargeCode = new CodeDescriptionPair2Char { Code = Core.Constants.AWB.ChargeCodes.AC };
			otherChargesDO.EntitlementDue = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.EntitlementCode.Carrier };
			otherChargesDO.PrepaidCollect = new CodeDescriptionPair { Code = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid };
			otherChargesDO.Description = "Charge Description";
			otherChargesDO.Amount = 45.5;

			var reader = new AWBOtherChargesDataObjectReader(otherChargesDO, Logger, Factory);
			var otherChargesBO = reader.ReadIntoBusinessObject();

			AssertEquals(Core.Constants.AWB.ChargeCodes.AC, otherChargesBO.EO_ChargeCode);
			AssertEquals(Core.Constants.AWB.EntitlementCode.Carrier, otherChargesBO.EO_EntitlementCode);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, otherChargesBO.EO_PPDCLT);
			AssertEquals("Charge Description", otherChargesBO.EO_ChargeDescription);
			AssertEquals(new ZDecimal(45.5), otherChargesBO.EO_Amount);
		}
	}
}
