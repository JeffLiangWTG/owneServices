using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class GovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem,
		IGoodsMeasure,
		IOrigin,
		IGovernmentProcedure
	{
		public GovernmentAgencyGoodsItem(AsycudaPackedItem packedItem, int sequenceNumeric)
		{
			PackedItem = Argument.NotNull(packedItem, nameof(packedItem));
			SequenceNumeric = sequenceNumeric;
		}

		AsycudaPackedItem PackedItem { get; }

		public ZInt SequenceNumeric { get; }

		ICommodity IGovernmentAgencyGoodsItem.Commodity => new Commodity(PackedItem);

		IEnumerable<IAdditionalDocument> IGovernmentAgencyGoodsItem.AdditionalDocuments => Enumerable.Empty<IAdditionalDocument>();

		IEnumerable<IAdditionalInformation> IGovernmentAgencyGoodsItem.AdditionalInformations => Enumerable.Empty<IAdditionalInformation>();

		IGoodsMeasure IGovernmentAgencyGoodsItem.GoodsMeasure => this;

		IPartyDetails IGovernmentAgencyGoodsItem.Manufacturer => null;

		IOrigin IGovernmentAgencyGoodsItem.Origin => this;

		IPackaging IGovernmentAgencyGoodsItem.Packaging => null;

		IPreviousDocument IGovernmentAgencyGoodsItem.PreviousDocument => default;

		ILPCODetail IGovernmentAgencyGoodsItem.ApprovalDocument => null;

		ICommoditySpecification IGovernmentAgencyGoodsItem.CommoditySpecification => null;

		IGoodsLicensingStatisticalMeasure IGovernmentAgencyGoodsItem.GoodsLicensingStatisticalMeasure => null;

		IGoodsStatisticalMeasure IGovernmentAgencyGoodsItem.GoodsStatisticalMeasure => null;

		ILPCODetail IGovernmentAgencyGoodsItem.MedicalInstrument => null;

		IPreviousDocument IGovernmentAgencyGoodsItem.PreBondedDocument => PackedItem.IsAir ? null : new PreviousDocumentWrapper(PackedItem.API_PreviousEntryNo, PackedItem.API_PreviousEntryLineNo);

		IEnumerable<IShippingIdentification> IGovernmentAgencyGoodsItem.ShippingIdentifications => Enumerable.Empty<IShippingIdentification>();

		IGovernmentProcedure IGovernmentAgencyGoodsItem.GovernmentProcedure => this;

		ZDateTime IGovernmentAgencyGoodsItem.ControlInspectionStartDateTime => ZDateTime.Empty;

		ZString IGovernmentAgencyGoodsItem.ExaminationPlace => ZString.Empty;

		IEnumerable<ITransportEquipment> IGovernmentAgencyGoodsItem.TransportEquipments => Enumerable.Empty<ITransportEquipment>();

		IAdditionalDeclaration IGovernmentAgencyGoodsItem.AdditionalDeclaration => null;

		ZString IGovernmentAgencyGoodsItem.CriteriaCode => ZString.Empty;

		ZString IGovernmentAgencyGoodsItem.PreferentialCriteria => ZString.Empty;

		ZString IGovernmentAgencyGoodsItem.ProducerCode => ZString.Empty;

		ZString IGovernmentAgencyGoodsItem.OtherCriteria => ZString.Empty;

		#region IGoodsMeasure members
		ZDecimal IGoodsMeasure.NetWeightMeasure => PackedItem.PackedItemNetWeightInKG;

		ZDecimal IGoodsMeasure.TariffQuantity => PackedItem.API_CustomsQty;

		ZString IGoodsMeasure.UnitCode => PackedItem.API_CustomsUQ;

		ZString IGoodsMeasure.CustomUnitCode => ZString.Empty;
		#endregion

		#region IOrigin members
		ZString IOrigin.CountryCode => PackedItem.IsAir && PackedItem.IsExport ? ZString.Empty : PackedItem.API_RN_NKGoodsOrigin;

		IAdditionalDocument IOrigin.AdditionalDocument => null;
		#endregion

		#region IGovernmentProcedure members
		ZString IGovernmentProcedure.TransportTypeCode => ZString.Empty;

		ZString IGovernmentProcedure.CurrentCode => PackedItem.ModeOfStatisticsOrDutyTreatment;

		ZString IGovernmentProcedure.Description => ZString.Empty;
		#endregion
	}
}
