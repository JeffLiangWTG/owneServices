using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(VehicleDetails))]
	public class VehicleDetailsTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<VehicleDetails>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().VehicleLines.AddNew();
		}

		public void TestBuildDateProperties()
		{
			VehicleDetails.Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			Assert("Build Date Location should be editable.", !VehicleDetails.US_MfrDateTypeInfo.ReadOnly);
			Assert("Build Date Description should be readonly.", VehicleDetails.US_BuildDateExplanationInfo.ReadOnly);

			VehicleDetails.Vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			Assert("Build Date Location should be editable.", !VehicleDetails.US_MfrDateTypeInfo.ReadOnly);
			Assert("Build Date Description should be readonly.", VehicleDetails.US_BuildDateExplanationInfo.ReadOnly);

			VehicleDetails.US_MfrDateType = ManufactureDateTypeList.Codes.OTH;
			Assert("Build Date Location should be editable.", !VehicleDetails.US_MfrDateTypeInfo.ReadOnly);
			Assert("Build Date Description should be editable.", !VehicleDetails.US_BuildDateExplanationInfo.ReadOnly);

			VehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;

			var additionalNumber1 = VehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber1.CY_Code = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			additionalNumber1.CY_Data = "V0000001";

			var additionalNumber2 = VehicleDetails.AdditionalNumbers.AddNew();
			additionalNumber2.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber2.CY_Data = "E0000001";

			var iDetails = VehicleDetails as IVNEDetails;
			var engineAdditionalNumbers = iDetails.EngineAdditionalNumbers;
			Assert(engineAdditionalNumbers.Any(x => x.Number == "E0000001"));

			var vehicleAdditionalNumbers = iDetails.VehicleAdditionalNumbers;
			Assert(vehicleAdditionalNumbers.Any(x => x.Number == "V0000001"));
		}

		public void TestSetDefaultValueManufacturerFrominvoiceLine()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.OH_FullName = "TEST MANUFACTURER";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			var vehicle = invoiceLine.VehicleLines.AddNew();
			var detail = vehicle.VehicleAndEngineDetails.AddNew();
			AssertEquals(ZString.Empty, detail.US_EngineManufacturer);
			AssertEquals(ZString.Empty, detail.US_VehicleManufacturer);
			detail.US_EngineNumber = "111111";
			AssertEquals("TEST MANUFACTURER", detail.US_EngineManufacturer);
			AssertEquals(ZString.Empty, detail.US_VehicleManufacturer);
			detail.US_IdentityNumber = "2222222";
			AssertEquals("TEST MANUFACTURER", detail.US_EngineManufacturer);
			AssertEquals("TEST MANUFACTURER", detail.US_VehicleManufacturer);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return VehicleDetails;
		}

		#endregion

		#region Implementation

		protected override IEnumerable<VehicleDetails> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			vehicle = invoiceLine.VehicleLines.AddNew();
			yield return vehicle.VehicleAndEngineDetails.AddNew();
		}

		VehicleDetails VehicleDetails
		{
			get
			{
				if (vehicleDetails == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					vehicle = invoiceLine.VehicleLines.AddNew();
					vehicleDetails = vehicle.VehicleAndEngineDetails.AddNew();
				}
				return vehicleDetails;
			}
		}
		VehicleDetails vehicleDetails;
		Vehicle vehicle;
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
