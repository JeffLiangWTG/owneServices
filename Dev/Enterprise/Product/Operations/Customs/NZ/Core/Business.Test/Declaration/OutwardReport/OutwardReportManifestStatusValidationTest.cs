using Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport.Testing
{
	using Enterprise.Freight.Business;

	public class OutwardReportManifestStatusValidationTest : OutwardReportValidationTest
	{
		public void TestOceanBill()
		{
			SetUpValidSEAConsol();
			Consol.JK_MasterBillNum = "OceanBillThatIsTooLongForMPI";
			validator = new OutwardReportManifestStatusValidation(ManifestStatus);
			int errorCount = validator.GetErrorCount(MessageBuilder.MessageTypes.Original);
			AssertNoErrors("Ocean Bill Number can now have full 35 characters for NZ MPI", Consol.JK_MasterBillNumInfo);
		}

		public void TestHouseBills()
		{
			SetUpValidAIRConsol();
			shipment = Consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SIA0003948";
			shipment.JS_HouseBill = "HouseBillThatIsLong";
			validator = new OutwardReportManifestStatusValidation(ManifestStatus);
			AssertNoErrors("House Bill Number can now have full 35 charactersr", shipment.JS_HouseBillInfo);
		}

		OutwardReportManifestStatusValidation validator;
		CommonShipment shipment;
	}
}
