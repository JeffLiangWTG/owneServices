using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageConsignmentGovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
	{
		public LicensingMessageConsignmentGovernmentAgencyGoodsItem(JobComInvoiceLine firstInvoiceLine)
		{
			FirstInvoiceLine = firstInvoiceLine;
		}

		JobComInvoiceLine FirstInvoiceLine { get; }

		ZInt IGovernmentAgencyGoodsItem.SequenceNumeric => ZInt.Zero;

		ZString IGovernmentAgencyGoodsItem.CriteriaCode => ZString.Empty;

		ZString IGovernmentAgencyGoodsItem.PreferentialCriteria => ZString.Empty;

		ZString IGovernmentAgencyGoodsItem.ProducerCode => ZString.Empty;

		ZString IGovernmentAgencyGoodsItem.OtherCriteria => ZString.Empty;

		IGoodsMeasure IGovernmentAgencyGoodsItem.GoodsMeasure => null;

		IPartyDetails IGovernmentAgencyGoodsItem.Manufacturer => FirstInvoiceLine != null ? new LicensingMessageManufacturer(FirstInvoiceLine.ManufacturerDocAddress) : null;

		IPackaging IGovernmentAgencyGoodsItem.Packaging => null;

		IAdditionalDeclaration IGovernmentAgencyGoodsItem.AdditionalDeclaration => null;

		ICommodity IGovernmentAgencyGoodsItem.Commodity => null;

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
