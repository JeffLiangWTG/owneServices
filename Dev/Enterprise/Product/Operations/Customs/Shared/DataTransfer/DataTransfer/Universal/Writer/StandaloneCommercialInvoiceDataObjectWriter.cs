using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class StandaloneCommercialInvoiceDataObjectWriter : TopLevelDataObjectWriter<BaseJobComInvoiceHeader, UniversalShipment>
	{
		internal protected StandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected sealed override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected sealed override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.CustomsCommercialInvoice;
		}

		protected sealed override void PopulateDataObject(BaseJobComInvoiceHeader invoiceBO, UniversalShipment invoiceData)
		{
			var provider = invoiceBO.Factory.GetUniversalCustomsDataObjectProvider(invoiceBO.CountryCode);
			var writer = provider?.GetNewStandaloneCommercialInvoiceDataObjectWriter(writeManager) ?? this;
			writer.PopulateDataObjectCore(invoiceBO, invoiceData);
		}

		protected virtual UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobComInvoiceHeader invoiceBO)
		{
			return new UniversalDataObjectWriterHelper(invoiceBO.Factory, invoiceBO.CountryCode);
		}

		protected virtual CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(UniversalDataObjectWriterHelper helper)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper);
		}

		void PopulateDataObjectCore(BaseJobComInvoiceHeader invoiceBO, UniversalShipment invoiceData)
		{
			var helper = CreateNewUniversalDataObjectWriterHelper(invoiceBO);
			invoiceData.MessageType = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceBO.JZ_StandAloneInvoiceDirection, invoiceBO.Lookups.MessageTypes);
			invoiceData.Branch = Branch.New(invoiceBO.Branch);
			invoiceData.CommercialInfo = ProcessCommercialInfo(invoiceBO, helper);

			var transportLegs = invoiceBO.Transports;
			transportLegs.Sort(MovementLegComparer.PortsAndDatesBased(transportLegs));
			invoiceData.SetTransportLegCollection(() => ProcessCollection(transportLegs, new TransportLegDataObjectWriter(writeManager), CollectionContent.Complete, true));

			invoiceData.SetContainerCollection(() => ProcessContainerCollection(invoiceBO, helper));
			invoiceData.SetAdditionalBillCollection(() => ProcessAdditionalBillCollection(invoiceBO, helper));
			invoiceData.SetAdditionalReferenceCollection(() => ProcessAdditionalReferenceCollection(invoiceBO, helper));
		}

		List<AdditionalBill> ProcessAdditionalBillCollection(BaseJobComInvoiceHeader invoiceBO, UniversalDataObjectWriterHelper helper)
		{
			var result = new List<AdditionalBill>();
			foreach (var reference in invoiceBO.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => !x.J2_ReferenceNumber.IsEmpty && (x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.SH || x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.MB || x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB)))
			{
				result.Add(new AdditionalBill(writeManager.WriterStrategy)
				{
					BillNumber = reference.J2_ReferenceNumber,
					BillType = GetBillType(reference)
				});
			}
			return result;
		}

		WayBillType GetBillType(JobComInvoiceHeaderRefs reference)
		{
			ZString wayBillType;
			ZString wayBillTypeDescription;
			switch (reference.J2_ReferenceType)
			{
				case InvoiceHeaderRefsTypeList.Codes.MB:
					wayBillType = WayBillTypeList.Codes.Master;
					wayBillTypeDescription = WayBillTypeList.Descriptions.Master;
					break;
				case InvoiceHeaderRefsTypeList.Codes.HB:
					wayBillType = WayBillTypeList.Codes.House;
					wayBillTypeDescription = WayBillTypeList.Descriptions.House;
					break;
				case InvoiceHeaderRefsTypeList.Codes.SH:
					wayBillType = WayBillTypeList.Codes.SubHouse;
					wayBillTypeDescription = WayBillTypeList.Descriptions.SubHouse;
					break;
				default:
					wayBillType = reference.J2_ReferenceType;
					wayBillTypeDescription = reference.Lookups.ReferenceTypeList.GetDescriptionFromCode(wayBillType);
					break;
			}
			return new WayBillType() { Code = wayBillType, Description = wayBillTypeDescription };
		}

		DataObjectList<Container> ProcessContainerCollection(BaseJobComInvoiceHeader invoiceBO, UniversalDataObjectWriterHelper helper)
		{
			var result = new DataObjectList<Container>();
			result.Content = CollectionContent.Partial;
			foreach (var reference in invoiceBO.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => !x.J2_ReferenceNumber.IsEmpty && x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN))
			{
				result.Add(new Container(writeManager.WriterStrategy)
				{
					ContainerNumber = reference.J2_ReferenceNumber
				});
			}
			return result;
		}

		DataObjectList<AdditionalReference> ProcessAdditionalReferenceCollection(BaseJobComInvoiceHeader invoiceBO, UniversalDataObjectWriterHelper helper)
		{
			var result = new DataObjectList<AdditionalReference>();
			foreach (var reference in invoiceBO.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => !x.J2_ReferenceNumber.IsEmpty && x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.RP))
			{
				result.Add(new AdditionalReference()
				{
					Type = new EntryType() { Code = InvoiceHeaderRefsTypeList.Codes.RP, Description = InvoiceHeaderRefsTypeList.Descriptions.RP },
					ReferenceNumber = reference.J2_ReferenceNumber
				});
			}
			return result;
		}

		UniversalCustoms.CommercialInfo ProcessCommercialInfo(BaseJobComInvoiceHeader invoiceBO, UniversalDataObjectWriterHelper helper)
		{
			var commercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>();
			var writer = GetNewCommercialInvoiceHeaderDataObjectWriter(helper);
			commercialInvoiceCollection.Add(writer.GetDataObject(invoiceBO));
			return new UniversalCustoms.CommercialInfo() { Name = "STANDALONE", CommercialInvoiceCollection = commercialInvoiceCollection };
		}
	}
}
