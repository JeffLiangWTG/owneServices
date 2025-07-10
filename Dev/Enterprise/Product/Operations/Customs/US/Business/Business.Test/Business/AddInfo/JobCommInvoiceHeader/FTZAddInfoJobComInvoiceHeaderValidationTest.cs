using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZAddInfoJobComInvoiceHeaderValidationTest : CommonImportAddInfoJobComHeaderValidationTest
	{
		public void TestValidateInvoiceLineForTemporaryDeposit()
		{
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.TemporaryDeposit;
			invoice.AddInfoValidation.ValidateAll();
			AssertHasRowWarningContaining(invoice, FTZAddInfoJobComInvoiceHeaderValidation.InvoiceLineIsNotRequiredForTemporaryDeposit);
		}

		public void TestCheckUS_UC_NKCountryOfExport()
		{
			AssertUS_UC_NKCountryOfExport(invoice);
		}

		public void TestCheckUS_UC_NKCountryOfOrigin()
		{
			AssertUS_UC_NKCountryOfOrigin(invoice);
		}

		public void TestCheckUS_ZoneStatus()
		{
			AssertUS_ZoneStatus(invoice);
		}

		[TestDate(2000, 11, 11)]
		public void TestCheckUS_SplitShipmentDetail()
		{
			void SetupSplit(ITAndSplitDetails split, ZString carrier, ZString flight, ZDateTime arrival)
			{
				split.US_CarrierCode = carrier;
				split.US_FlightNumber = flight;
				split.US_ArrivalDate = arrival;
				split.US_ITNumber = "1";
			}

			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
				var houseBill = declaration.Bills.AddNew();
				houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
				houseBill.CU_BillNum = "H43289";
				houseBill.US_SESplitShip = true;
				SetupSplit(houseBill.ITAndSplitDetails.AddNew(), "C1", "FLT11", ZDateTime.Today);
				SetupSplit(houseBill.ITAndSplitDetails.AddNew(), "C2", "FLT12", ZDateTime.Today.AddDays(10));
				var invoice = declaration.Invoices.AddNew();
				AssertNoMessageErrors("Split info is not needed yet before the related bill is selected", invoice.US_SplitShipmentDetailInfo);
				invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
				invoice.AddInfoValidation.ValidateUS_SplitShipmentDetail();
				AssertHasMessageErrors("Split bill requires split info", invoice.US_SplitShipmentDetailInfo);
				invoice.US_SplitShipmentDetail = "C1/FLT11/11-11-00";
				invoice.AddInfoValidation.ValidateUS_SplitShipmentDetail();
				AssertHasMessageErrors("Split info must be in the list", invoice.US_SplitShipmentDetailInfo);
				invoice.US_SplitShipmentDetail = "C1/FLT11/11-NOV-00";
				invoice.AddInfoValidation.ValidateUS_SplitShipmentDetail();
				AssertNoMessageErrors("Split info must be in the list", invoice.US_SplitShipmentDetailInfo);
				houseBill.US_SESplitShip = false;
				invoice.AddInfoValidation.ValidateUS_SplitShipmentDetail();
				invoice.US_SplitShipmentDetail = ZString.Empty;
				AssertNoMessageErrors("Split info should not be entered/checked if the related bill is not split", invoice.US_SplitShipmentDetailInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			invoice = declaration.Invoices.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
	}
}
