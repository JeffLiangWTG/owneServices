using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	class AmendmentWriterProviderTest : TestCaseWithFactory
	{
		public void TestCommercialInvoiceLineCollectionWriterStrategy()
		{
			using (((IExternalFetchHintSupporter)Factory).SetupCreator())
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				declaration.PublishShipmentForWHSInward(false);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded();
				var provider = new AmendmentWriterProvider();
				var writer = provider.GetPreAmendmentWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice),
					writerStrategy: new DataObjectWriterStrategyTestClass(s =>
						s != nameof(CommercialInvoiceHeader.CommercialInvoiceLineCollection))), RecipientRoleType.BWI);
				var shipment = (Shipment)writer.GetDataObject(declaration);
				var commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
				AssertNull("CommercialInvoiceLineCollection - writerStrategy not allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);

				writer = provider.GetPreAmendmentWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, invoice)), RecipientRoleType.BWI);
				shipment = (Shipment)writer.GetDataObject(declaration);
				commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
				AssertNotNull("CommercialInvoiceLineCollection - writerStrategy allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);
			}
		}
	}
}
