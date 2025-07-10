using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESValidationExtensionTest : TestCaseWithFactory
	{
		public void TestIsCarrierCodeRequired()
		{
			JobDeclaration declaration = null;
			AssertEquals(false, declaration.IsCarrierCodeRequired());
			declaration = Factory.New<JobDeclaration>();
			var list = new TransportTypeList();
			foreach (var transportMode in new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.BorderWaterBorne, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Truck })
			{
				list.RemoveCode(transportMode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = transportMode;
				AssertEquals(true, declaration.IsCarrierCodeRequired());

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(false, declaration.IsCarrierCodeRequired());
			}
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			foreach (ICodeDescription pair in list)
			{
				declaration.JE_TransportMode = pair.Code;
				AssertEquals(false, declaration.IsCarrierCodeRequired());
			}
		}

		public void TestIsExportingCarrierRequired()
		{
			JobDeclaration declaration = null;
			AssertEquals(false, declaration.IsExportingCarrierRequired());
			declaration = Factory.New<JobDeclaration>();
			var list = new TransportTypeList();
			foreach (var transportMode in new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.BorderWaterBorne, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Truck })
			{
				list.RemoveCode(transportMode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = transportMode;
				AssertEquals(true, declaration.IsExportingCarrierRequired());

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(false, declaration.IsExportingCarrierRequired());
			}
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			foreach (ICodeDescription pair in list)
			{
				declaration.JE_TransportMode = pair.Code;
				AssertEquals(false, declaration.IsExportingCarrierRequired());
			}
		}

		public void TestIsWeightRequired()
		{
			JobComInvoiceLine invoiceLine = null;
			JobDeclaration declaration = null;
			AssertEquals(false, invoiceLine.IsWeightRequired());
			invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(false, invoiceLine.IsWeightRequired());

			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			var list = new TransportTypeList();
			foreach (var transportMode in new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.BorderWaterBorne, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Truck })
			{
				list.RemoveCode(transportMode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = transportMode;
				AssertEquals(true, invoiceLine.IsWeightRequired());

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(false, invoiceLine.IsWeightRequired());
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertEquals(false, invoiceLine.IsWeightRequired());

			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.HH;
			AssertEquals(false, invoiceLine.IsWeightRequired());

			list.RemoveCode(TransportTypeList.Codes.FixedTransportInstallations);

			foreach (ICodeDescription pair in list)
			{
				declaration.JE_TransportMode = pair.Code;
				invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.OS;
				AssertEquals("OS TransportMode " + pair.Code, false, invoiceLine.IsWeightRequired());

				invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.HH;
				AssertEquals("HH TransportMode " + pair.Code, true, invoiceLine.IsWeightRequired());
			}
		}

		public void TestIsTransportReferenceNumberRequired()
		{
			JobDeclaration declaration = null;
			Assert(!declaration.IsTransportReferenceNumberRequired());
			declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!declaration.IsTransportReferenceNumberRequired());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Assert(declaration.IsTransportReferenceNumberRequired());

			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;
			var list = new TransportTypeList();
			list.RemoveCode(TransportTypeList.Codes.Sea);
			foreach (ICodeDescription pair in list)
			{
				declaration.JE_TransportMode = pair.Code;
				Assert(!declaration.IsTransportReferenceNumberRequired());
			}
		}

		public void TestIsTransportReferenceNumberForbidden()
		{
			JobDeclaration declaration = null;
			Assert(!declaration.IsTransportReferenceNumberForbidden());
			declaration = Factory.New<JobDeclaration>();
			var list = new TransportTypeList();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			foreach (var mode in new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Air, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Truck })
			{
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				Assert(!declaration.IsTransportReferenceNumberForbidden());
				list.RemoveCode(mode);
			}

			foreach (ICodeDescription pair in list)
			{
				declaration.JE_TransportMode = pair.Code;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert(declaration.IsTransportReferenceNumberForbidden());

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert(!declaration.IsTransportReferenceNumberForbidden());
			}
		}
	}
}
