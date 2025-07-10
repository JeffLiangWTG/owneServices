using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CommercialInvoiceLineDataObjectReader : DataObjectReader<CommercialInvoiceLine, ForwardingPackLine>
	{
		public CommercialInvoiceLineDataObjectReader(CommercialInvoiceLine commercialInvoiceLine, ForwardingShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, Func<CommercialInvoiceLine, ForwardingPackLine> packLineBizObjProvider = null)
			: base(commercialInvoiceLine, logger, factory)
		{
			this.shipment = shipment;
			this.packLineBizObjProvider = packLineBizObjProvider;
		}

		public CommercialInvoiceLineDataObjectReader(CommercialInvoiceLine commercialInvoiceLine, ForwardingShipment shipment, IXmlImportLogger logger, BusinessObjectFactory factory, Func<CommercialInvoiceLine, ForwardingPackLine> packLineBizObjProvider = null)
			: base(commercialInvoiceLine, logger, new UniversalObjectFactory())
		{
			factoryOverride = factory;
			this.shipment = shipment;
			this.packLineBizObjProvider = packLineBizObjProvider;
		}

		readonly BusinessObjectFactory factoryOverride;
		readonly ForwardingShipment shipment;
		readonly Func<CommercialInvoiceLine, ForwardingPackLine> packLineBizObjProvider;

		protected override ForwardingPackLine GetExistingBusinessObject()
		{
			return packLineBizObjProvider != null ? packLineBizObjProvider(dataObject) : null;
		}

		protected override ForwardingPackLine GetNewBusinessObject()
		{
			return factoryOverride == null ? base.GetNewBusinessObject() : factoryOverride.New<ForwardingPackLine>();
		}

		protected override void PopulateBusinessObject(ForwardingPackLine targetBO)
		{
			targetBO.JL_FreightMode = Enterprise.Freight.Business.FreightConstants.OuterPackType;
			targetBO.JL_JS = shipment.PK;

			SetValue(targetBO, JobPackLinesSchema.JL_PackageCount, (ZInt)dataObject.InvoiceQuantity);
			SetValue(targetBO, JobPackLinesSchema.JL_F3_NKPackType, dataObject.InvoiceQuantityUnit);
			SetValue(targetBO, JobPackLinesSchema.JL_ActualWeightUQ, dataObject.WeightUnit);
			SetValue(targetBO, JobPackLinesSchema.JL_ActualWeight, dataObject.Weight);
			SetValue(targetBO, JobPackLinesSchema.JL_Description, dataObject.Description);
		}
	}
}
