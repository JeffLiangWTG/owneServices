using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CommercialInvoiceLineCollectionDataObjectReader : DataObjectCollectionReader<CommercialInvoiceLine, ForwardingPackLine>
	{
		public CommercialInvoiceLineCollectionDataObjectReader(CommercialInvoiceLine[] commercialInvoiceLines, ForwardingShipment parentShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(commercialInvoiceLines)
		{
			this.parentShipment = parentShipment;
			this.logger = logger;
			this.factory = factory;
		}

		readonly ForwardingShipment parentShipment;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;

		protected override ForwardingPackLine[] BusinessObjects
		{
			get
			{
				return businessObjects ?? (businessObjects = parentShipment.OuterPackLines.ToArray<ForwardingPackLine>());
			}
		}
		ForwardingPackLine[] businessObjects;

		protected override CollectionContent DefaultCollectionContent
		{
			get
			{
				return CollectionContent.Complete;
			}
		}

		protected override void AddToCollection(ForwardingPackLine businessObject)
		{
			parentShipment.OuterPackLines.Add(businessObject);
		}

		protected override void RemoveFromCollection(ForwardingPackLine businessObject)
		{
			parentShipment.OuterPackLines.RemoveAndDelete(businessObject);
		}

		protected override ForwardingPackLine FindMatchingBusinessObject(CommercialInvoiceLine dataObject)
		{
			var invoiceQuantity = dataObject.InvoiceQuantity.GetValueOrDefault();
			var invoiceQuantityUnit = dataObject.InvoiceQuantityUnit.GetCodeAsUpperCase();
			var description = dataObject.Description.GetValueOrDefault();

			var duplicatedLines = DataObjects.Where(line => line.InvoiceQuantity.GetValueOrDefault() == invoiceQuantity
								&& line.InvoiceQuantityUnit.GetCodeAsUpperCase() == invoiceQuantityUnit
								&& line.Description.GetValueOrDefault() == description);

			if (duplicatedLines.Count() == 1)
			{
				var matchedPackingLines = BusinessObjects.Where(x => x.JL_PackageCount == invoiceQuantity
																&& x.JL_F3_NKPackType == invoiceQuantityUnit
																&& x.JL_Description == description);

				if (matchedPackingLines.Count() == 1)
				{
					return matchedPackingLines.First();
				}
			}

			return null;
		}

		protected override ForwardingPackLine ReadIntoBusinessObject(CommercialInvoiceLine dataObject, ForwardingPackLine businessObject)
		{
			var reader = new CommercialInvoiceLineDataObjectReader(dataObject, parentShipment, logger, factory, data => businessObject);

			return reader.ReadIntoBusinessObject();
		}
	}
}
