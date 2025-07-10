using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class AWBOtherChargesDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteOtherCharges()
		{
			var otherCharges = Factory.New<ExportAWBOtherCharges>();
			otherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.DC;
			otherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharges.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			otherCharges.EO_ChargeDescription = "You owe me thirty cents mate";
			otherCharges.EO_Amount = 0.30;

			var writer = new AWBOtherChargesDataObjectWriter(new DataWritingManager(new ActionInfo(null, otherCharges)));
			var otherChargesDataObject = writer.GetDataObject(otherCharges);

			AssertEquals(Core.Constants.AWB.ChargeCodes.DC, otherChargesDataObject.ChargeCode.Code);
			AssertEquals("Certificate of Origin", otherChargesDataObject.ChargeCode.Description);
			AssertEquals(Core.Constants.AWB.EntitlementCode.Carrier, otherChargesDataObject.EntitlementDue.Code);
			AssertEquals("Carrier", otherChargesDataObject.EntitlementDue.Description);
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, otherChargesDataObject.PrepaidCollect.Code);
			AssertEquals("Prepaid", otherChargesDataObject.PrepaidCollect.Description);
			AssertEquals("You owe me thirty cents mate", otherChargesDataObject.Description);
			AssertEquals(new ZDecimal(0.30), otherChargesDataObject.Amount);
		}
	}
}
