using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal.Testing
{
	sealed class StandaloneCommercialInvoiceDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestDutiableWGTVOLQTYAddInfo()
		{
			var buyer = CreateOrganisation("JOO", "ABC!@#12");
			var supplier2 = CreateOrganisation("GARY", "ABC!@#13");
			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoice.InvoiceNumber = "INVABC123";
			invoice.InvoiceAmount = 150m;
			invoice.Supplier = invoice.AddOrgAddress(writeManager, supplier2, AddressTypes.Supplier);
			invoice.Buyer = invoice.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);

			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_UnitDutiableWGTVOLQTY), Value = "93.95" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_UnitDutiableWGTVOLQTYUnit), Value = "STK" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_TotalDutiableWGTVOLQTY), Value = "100.00" },
					new AddInfo() { Key = Business.AddInfo.GetDBNameMapped(SGAddInfoSchema.Constants.SG_TotalDutiableWGTVOLQTYUnit), Value = "PKT" }
				};

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<CommercialInvoiceHeader>(new[] { invoice }))
			};

			var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoice, Logger, Factory);
			var invoiceBO = reader.ReadIntoBusinessObject();
			Business.JobComInvoiceLine invoiceLineBO = (Business.JobComInvoiceLine)invoiceBO.JobComInvoiceLines[0];
			AssertContains("SG92=93.95", invoiceLineBO.JI_AddInfo);
			AssertContains("SG93=STK", invoiceLineBO.JI_AddInfo);
			AssertContains("SG89=100", invoiceLineBO.JI_AddInfo);
			AssertContains("SG90=PKT", invoiceLineBO.JI_AddInfo);
			AssertEquals("SG_UnitDutiableWGTVOLQTY", 93.95m, invoiceLineBO.SG_UnitDutiableWGTVOLQTY);
			AssertEquals("SG_UnitDutiableWGTVOLQTYUnit", "STK", invoiceLineBO.SG_UnitDutiableWGTVOLQTYUnit);
			AssertEquals("SG_TotalDutiableWGTVOLQTY", 100.00m, invoiceLineBO.SG_TotalDutiableWGTVOLQTY);
			AssertEquals("SG_TotalDutiableWGTVOLQTYUnit", "PKT", invoiceLineBO.SG_TotalDutiableWGTVOLQTYUnit);
		}
	}
}
