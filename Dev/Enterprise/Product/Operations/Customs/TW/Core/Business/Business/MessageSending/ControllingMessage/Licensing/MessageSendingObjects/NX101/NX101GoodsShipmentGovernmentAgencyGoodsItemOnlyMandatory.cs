using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101GoodsShipmentGovernmentAgencyGoodsItemOnlyMandatory : IGovernmentAgencyGoodsItem
	{
		public NX101GoodsShipmentGovernmentAgencyGoodsItemOnlyMandatory(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine, ZString description)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
			this.sequenceNumeric = sequenceNumeric;
			this.description = description;
		}

		readonly ZInt sequenceNumeric;
		readonly JobComInvoiceLine invoiceLine;
		readonly ZString description;

		ZInt IGovernmentAgencyGoodsItem.SequenceNumeric => sequenceNumeric;

		ZString IGovernmentAgencyGoodsItem.CriteriaCode => ZString.Empty;

		ZString IGovernmentAgencyGoodsItem.PreferentialCriteria => ZString.Empty;

		ZString IGovernmentAgencyGoodsItem.ProducerCode => ZString.Empty;

		ZString IGovernmentAgencyGoodsItem.OtherCriteria => ZString.Empty;

		IGoodsMeasure IGovernmentAgencyGoodsItem.GoodsMeasure => new GoodsMeasure(0, invoiceLine.JI_PermitUQ);

		IPartyDetails IGovernmentAgencyGoodsItem.Manufacturer => null;

		IPackaging IGovernmentAgencyGoodsItem.Packaging => null;

		IAdditionalDeclaration IGovernmentAgencyGoodsItem.AdditionalDeclaration => null;

		ICommodity IGovernmentAgencyGoodsItem.Commodity => new NX101GoodsShipmentGovernmentAgencyGoodsItemCommodityOnlyMandatory(invoiceLine, description);

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
