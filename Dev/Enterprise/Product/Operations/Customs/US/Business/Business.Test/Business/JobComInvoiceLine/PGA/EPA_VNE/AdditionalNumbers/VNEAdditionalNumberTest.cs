using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(VNEAdditionalNumber))]
	public class VNEAdditionalNumberTest : Customs.Business.Testing.CusCodeDataTest<VNEAdditionalNumber>
	{
		public void TestProperties()
		{
			var additionalNumber = AdditionalNumbers.AddNew();
			AssertEquals("Type", CusCodeDataTypeList.Codes.VNEAdditionalNumber, additionalNumber.CY_Type);
			AssertEquals("Validation", typeof(VNEAdditionalNumberValidation), additionalNumber.Validation.GetType());
			additionalNumber.CY_Code = "SE";
			AssertEquals("VDE (SE)", additionalNumber.CY_DataInfo.HumanReadableName);

			additionalNumber.CY_Code = "";
			AssertEquals("VDE", additionalNumber.CY_DataInfo.HumanReadableName);

			additionalNumber.CY_Data = "testams";
			AssertEquals("TESTAMS", additionalNumber.CY_Data);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return AdditionalNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().VehicleLines.AddNew().VehicleAndEngineDetails.AddNew().AdditionalNumbers.AddNew();
		}

		VNEAdditionalNumberCollection AdditionalNumbers
		{
			get
			{
				if (additionalNumbers == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var vne = invoiceLine.VehicleLines.AddNew();
					var detail = vne.VehicleAndEngineDetails.AddNew();
					additionalNumbers = detail.AdditionalNumbers;
				}
				return additionalNumbers;
			}
		}
		VNEAdditionalNumberCollection additionalNumbers;
	}
}
