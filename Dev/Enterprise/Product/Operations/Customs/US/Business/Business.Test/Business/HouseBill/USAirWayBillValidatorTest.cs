using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USAirWayBillValidatorTest : AirWayBillValidatorTest
	{
		public void TestValidateAirMasterBillNumber()
		{
			var declaration = GetDeclaration();
			var warning = MAWBLengthWarningMessageForTest;
			declaration.JE_MasterBill = "AMF12378945";
			AssertNoWarning("No warnings for Import Air Master Bill number", declaration.JE_MasterBillInfo, warning);
			AssertNoWarning("No warnings for Import Air Master Bill number", declaration.JE_MasterBillInfo, "The MAWB can only contain numbers.");

			declaration.JE_MasterBill = "AMF1237894";
			AssertHasWarning("Warnings for Import Air Master Bill number", declaration.JE_MasterBillInfo, warning);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MasterBill = "AMF12378945";
			AssertHasWarning("Should be invalid format warnings, becuase this is Export Air Master Bill number", declaration.JE_MasterBillInfo, "The MAWB can only contain numbers.");
		}

		public void TestIsNotMatchingBillFormat()
		{
			var declaration = GetDeclaration();
			var validator = new USAirWayBillValidatorForTest(declaration);
			AssertEquals("Not Match correct format. Correct format should be: first 3 chars alpha or numeric, last 8 chars numeric only",
						false, validator.IsMatchingBillFormat("AMF1235K123"));

			AssertEquals("Matching correct format. Correct format should be: first 3 chars alpha or numeric, last 8 chars numeric only",
						true, validator.IsMatchingBillFormat("AMF12357123"));

			AssertEquals("Matching correct format. Correct format should be: first 3 chars alpha or numeric, last 8 chars numeric only",
						true, validator.IsMatchingBillFormat("A0F12357123"));

			AssertEquals("Matching correct format. Correct format should be: first 3 chars alpha or numeric, last 8 chars numeric only",
						true, validator.IsMatchingBillFormat("00112357123"));
		}

		public static string MAWBLengthWarningMessageForTest
		{
			get
			{
				var declaration = new BusinessObjectFactory().New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				return new USAirWayBillValidatorForTest(declaration).MAWBLengthWarningMessageForTest;
			}
		}

		JobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "A8"));
			if (usCarrier == null)
			{
				usCarrier = Factory.New<USCarrierCombined>();
				usCarrier.UI_Code = "A8";
				usCarrier.UI_ModeOfTransportation = "40";
				usCarrier.UI_Name = "Test Carrier";
				usCarrier.UI_AirwayBillPrefix = "AMF";
			}
			declaration.JE_MasterBillIssuerSCAC = usCarrier.UI_Code;
			return declaration;
		}

		public class USAirWayBillValidatorForTest : USAirWayBillValidator
		{
			public USAirWayBillValidatorForTest(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public string MAWBLengthWarningMessageForTest
			{
				get { return base.MAWBLengthWarningMessage; }
			}

			public new bool IsMatchingBillFormat(string masterBillNumber)
			{
				return base.IsMatchingBillFormat(masterBillNumber);
			}
		}
	}
}
