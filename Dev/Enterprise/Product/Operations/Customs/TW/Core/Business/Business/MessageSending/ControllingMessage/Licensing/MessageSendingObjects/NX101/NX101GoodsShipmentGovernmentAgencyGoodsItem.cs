using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101GoodsShipmentGovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
	{
		public NX101GoodsShipmentGovernmentAgencyGoodsItem(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
			this.sequenceNumeric = sequenceNumeric;
			this.header = header;
		}

		readonly ZInt sequenceNumeric;
		readonly CusTWControllingMessageHeader header;
		readonly JobComInvoiceLine invoiceLine;

		internal JobComInvoiceLine InvoiceLine => invoiceLine;

		ZInt IGovernmentAgencyGoodsItem.SequenceNumeric => sequenceNumeric;

		ZString IGovernmentAgencyGoodsItem.CriteriaCode => invoiceLine.JI_OriginCriteria;

		ZString IGovernmentAgencyGoodsItem.PreferentialCriteria => invoiceLine.JI_PTCriteria;

		ZString IGovernmentAgencyGoodsItem.ProducerCode => invoiceLine.JI_ManufacturerRelationship;

		ZString IGovernmentAgencyGoodsItem.OtherCriteria => invoiceLine.JI_PTCriteria2;

		IGoodsMeasure IGovernmentAgencyGoodsItem.GoodsMeasure => new GoodsMeasure(invoiceLine.JI_PermitQty, invoiceLine.JI_PermitUQ, invoiceLine.JI_CustomPermitUQ);

		IPartyDetails IGovernmentAgencyGoodsItem.Manufacturer
		{
			get
			{
				IPartyDetails partyDetails = null;
				var certificateType = header.TW1_CertificateType;
				if (certificateType == CertificateTypeList.Codes.Code9
					|| certificateType == CertificateTypeList.Codes.Code11
					|| certificateType == CertificateTypeList.Codes.Code13
					|| certificateType == CertificateTypeList.Codes.Code14
					|| certificateType == CertificateTypeList.Codes.Code15
					|| certificateType == CertificateTypeList.Codes.Code18)
				{
					partyDetails = new PartyWrapper(id: invoiceLine.ManufacturerDocAddress.IDCode);
				}
				return partyDetails;
			}
		}

		IPackaging IGovernmentAgencyGoodsItem.Packaging => new NX101GoodsShipmentPackaging(header, invoiceLine);

		IAdditionalDeclaration IGovernmentAgencyGoodsItem.AdditionalDeclaration
		{
			get
			{
				IAdditionalDeclaration result = null;
				if (!header.Declaration.ClearanceStatus.IsEmpty && header.IsCertificate15)
				{
					var entryLine = invoiceLine.CusEntryLine;
					var decl = invoiceLine.Declaration;
					if (entryLine != null && decl != null)
					{
						result = new NX101AdditionalDeclarationWrapper((ZDecimal)entryLine.CL_LineNumber, decl.DeclarationNumber);
					}
				}
				return result;
			}
		}

		ICommodity IGovernmentAgencyGoodsItem.Commodity => new NX101GoodsShipmentGovernmentAgencyGoodsItemCommodity(header, invoiceLine);

		IEnumerable<IAdditionalDocument> IGovernmentAgencyGoodsItem.AdditionalDocuments => null;

		IEnumerable<IAdditionalInformation> IGovernmentAgencyGoodsItem.AdditionalInformations => null;

		IOrigin IGovernmentAgencyGoodsItem.Origin => null;

		IPreviousDocument IGovernmentAgencyGoodsItem.PreviousDocument => null;

		ILPCODetail IGovernmentAgencyGoodsItem.ApprovalDocument => null;

		ICommoditySpecification IGovernmentAgencyGoodsItem.CommoditySpecification => null;

		IGoodsLicensingStatisticalMeasure IGovernmentAgencyGoodsItem.GoodsLicensingStatisticalMeasure => null;

		IGoodsStatisticalMeasure IGovernmentAgencyGoodsItem.GoodsStatisticalMeasure => null;

		ILPCODetail IGovernmentAgencyGoodsItem.MedicalInstrument => null;

		IPreviousDocument IGovernmentAgencyGoodsItem.PreBondedDocument => null;

		IEnumerable<IShippingIdentification> IGovernmentAgencyGoodsItem.ShippingIdentifications => null;

		IGovernmentProcedure IGovernmentAgencyGoodsItem.GovernmentProcedure => null;

		ZDateTime IGovernmentAgencyGoodsItem.ControlInspectionStartDateTime => ZDateTime.Empty;

		ZString IGovernmentAgencyGoodsItem.ExaminationPlace => null;

		IEnumerable<ITransportEquipment> IGovernmentAgencyGoodsItem.TransportEquipments => null;
	}
}
